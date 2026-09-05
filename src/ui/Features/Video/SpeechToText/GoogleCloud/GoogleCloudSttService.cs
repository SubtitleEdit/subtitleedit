using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.OpenAiCompatible;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.GoogleCloud;

public class GoogleCloudSttSettings
{
    public string KeyFile { get; set; } = string.Empty;
    public string Region { get; set; } = "us";
    public string Model { get; set; } = "chirp_3";
    public string Language { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 3600;
    public bool DynamicBatching { get; set; }
    public Action<string>? Logger { get; set; }

    public static GoogleCloudSttSettings FromConfiguration() => new()
    {
        KeyFile = Se.Settings.Tools.GoogleCloudSttKeyFile,
        Region = Se.Settings.Tools.GoogleCloudSttRegion,
        Model = Se.Settings.Tools.GoogleCloudSttModel,
        Language = Se.Settings.Tools.GoogleCloudSttLanguage,
        BucketName = Se.Settings.Tools.GoogleCloudSttBucketName,
        TimeoutSeconds = Se.Settings.Tools.GoogleCloudSttTimeoutSeconds,
        DynamicBatching = Se.Settings.Tools.GoogleCloudSttDynamicBatching,
        Logger = message => Se.WriteToolsLog(message),
    };
}

/// <summary>
/// Speech-to-text via Google Cloud Speech-to-Text v2 over plain REST, returning
/// real word timings. v2 rejects API keys, so a service-account JSON key is
/// turned into a bearer token via the Google.Apis.Auth package SE already ships
/// for Google TTS. BatchRecognize only reads from Cloud Storage, so the flow is:
/// ensure bucket → upload object → batchRecognize → poll operation → delete object.
/// Sync Recognize would avoid the bucket but is capped at one minute of audio.
/// </summary>
public class GoogleCloudSttService : ISttTranscriber
{
    private const string Scope = "https://www.googleapis.com/auth/cloud-platform";
    private const string StorageApi = "https://storage.googleapis.com/storage/v1";
    private const string StorageUploadApi = "https://storage.googleapis.com/upload/storage/v1";

    private readonly HttpClient _httpClient;
    private readonly GoogleCloudSttSettings _settings;
    private ITokenAccess? _credential;

    public GoogleCloudSttService(GoogleCloudSttSettings settings)
        : this(OpenAiSttService.SharedHttpClient, settings)
    {
    }

    public GoogleCloudSttService(HttpClient httpClient, GoogleCloudSttSettings settings)
    {
        _httpClient = httpClient;
        _settings = settings;
    }

    /// <summary>Chirp models are served only from regional hosts ("us", "eu", ...); "global" uses the plain host.</summary>
    public static string GetSpeechHost(string? region)
    {
        var r = (region ?? string.Empty).Trim().ToLowerInvariant();
        return r.Length == 0 || r == "global" ? "speech.googleapis.com" : $"{r}-speech.googleapis.com";
    }

    /// <summary>Bucket names are globally unique, so derive one from the (globally unique) project id.</summary>
    public static string DeriveBucketName(string projectId)
    {
        const string suffix = "-subtitle-edit-stt";
        var cleaned = new string(projectId.ToLowerInvariant().Select(c => char.IsAsciiLetterOrDigit(c) ? c : '-').ToArray()).Trim('-');
        var max = 63 - suffix.Length;
        return (cleaned.Length > max ? cleaned[..max] : cleaned) + suffix;
    }

    public async Task<OpenAiCompatibleSttResponse> TranscribeAsync(
        string audioFilePath,
        string? language,
        IProgress<string>? progress,
        IProgress<OpenAiCompatibleSegment>? segmentProgress,
        CancellationToken cancellationToken)
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        if (_settings.TimeoutSeconds > 0)
        {
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(_settings.TimeoutSeconds));
        }
        var ct = timeoutCts.Token;

        try
        {
            var projectId = ReadProjectId(_settings.KeyFile);
            _credential = (await GoogleCredential.FromFileAsync(_settings.KeyFile, ct)).CreateScoped(Scope);

            var bucket = string.IsNullOrWhiteSpace(_settings.BucketName) ? DeriveBucketName(projectId) : _settings.BucketName.Trim();
            await EnsureBucketAsync(projectId, bucket, ct);

            var objectName = $"subtitle-edit/{Guid.NewGuid():N}{Path.GetExtension(audioFilePath)}";
            progress?.Report("Uploading audio to Cloud Storage...");
            await UploadAsync(bucket, objectName, audioFilePath, ct);
            var gcsUri = $"gs://{bucket}/{objectName}";

            try
            {
                progress?.Report("Submitting transcription...");
                var operationName = await SubmitAsync(projectId, gcsUri, language, ct);
                progress?.Report("Waiting for transcription to complete...");
                var json = await PollAsync(operationName, ct);
                var response = ParseResponse(json, _settings.Logger);
                return await RecoverTruncationAsync(response, projectId, bucket, audioFilePath, language, progress, ct);
            }
            finally
            {
                await DeleteObjectAsync(bucket, objectName);
            }
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException($"Google Cloud transcription timed out after {_settings.TimeoutSeconds} seconds.");
        }
    }

    /// <summary>
    /// Re-transcribes the tail when Google stopped part way through and still reported
    /// success. This has been observed on real media: an 18 minute chunk returned words
    /// only up to 398 s and silently discarded the remaining 11.4 minutes. Logging that is
    /// not enough, because the user is handed a subtitle that looks complete and only turns
    /// out to have a hole in it while watching.
    ///
    /// One attempt only. If the tail truncates as well there is nothing further to try
    /// automatically, and the log line still records it.
    /// </summary>
    private async Task<OpenAiCompatibleSttResponse> RecoverTruncationAsync(
        OpenAiCompatibleSttResponse response,
        string projectId,
        string bucket,
        string audioFilePath,
        string? language,
        IProgress<string>? progress,
        CancellationToken ct)
    {
        var billed = response.Duration ?? 0.0;
        var covered = response.Segments is { Count: > 0 } segments ? segments[^1].End : 0.0;
        if (billed <= 60 || covered >= billed * 0.9)
        {
            return response;
        }

        // Start a second early so a word straddling the cut is not lost between the passes.
        var resumeAt = Math.Max(0.0, covered - 1.0);
        _settings.Logger?.Invoke($"Google Cloud: re-transcribing from {resumeAt:0.#} s to recover the missing audio");
        progress?.Report("Recovering the part Google stopped transcribing...");

        var remainderPath = await CutRemainderAsync(audioFilePath, resumeAt, ct);
        if (remainderPath == null)
        {
            return response;
        }

        var objectName = $"subtitle-edit/{Guid.NewGuid():N}{Path.GetExtension(remainderPath)}";
        try
        {
            await UploadAsync(bucket, objectName, remainderPath, ct);
            var operationName = await SubmitAsync(projectId, $"gs://{bucket}/{objectName}", language, ct);
            var json = await PollAsync(operationName, ct);
            var tail = ParseResponse(json, _settings.Logger);

            var text = new StringBuilder(response.Text ?? string.Empty);
            foreach (var segment in tail.Segments)
            {
                // The tail's timings restart at zero, so they need the resume point back.
                var start = segment.Start + resumeAt;
                if (start <= covered - 0.5)
                {
                    continue; // Overlap with what the first pass already produced.
                }

                segment.Id = response.Segments.Count;
                segment.Start = start;
                segment.End = segment.End + resumeAt;
                if (segment.Words != null)
                {
                    foreach (var word in segment.Words)
                    {
                        word.Start += resumeAt;
                        word.End += resumeAt;
                    }
                }

                response.Segments.Add(segment);
                text.Append(' ').Append(segment.Text);
            }

            response.Text = text.ToString().Trim();
            _settings.Logger?.Invoke(
                $"Google Cloud: recovery added {response.Segments.Count} segments in total, now covering to {(response.Segments.Count > 0 ? response.Segments[^1].End : 0):0.#} s");
            return response;
        }
        finally
        {
            await DeleteObjectAsync(bucket, objectName);
            TryDelete(remainderPath);
        }
    }

    /// <summary>
    /// Cuts from <paramref name="fromSeconds"/> to the end of the audio. Re-encodes rather
    /// than stream copies so the cut is sample accurate, and passes -sample_fmt explicitly:
    /// without it ffmpeg produces 24-bit flac, which is larger than the raw PCM it replaces.
    /// </summary>
    private async Task<string?> CutRemainderAsync(string audioFilePath, double fromSeconds, CancellationToken ct)
    {
        var ffmpeg = Se.Settings.General.FfmpegPath;
        if (!File.Exists(ffmpeg))
        {
            ffmpeg = "ffmpeg";
        }

        var extension = Path.GetExtension(audioFilePath);
        var outputPath = Path.Combine(Path.GetDirectoryName(audioFilePath) ?? Path.GetTempPath(),
            Path.GetFileNameWithoutExtension(audioFilePath) + "-tail" + extension);

        var codec = extension.Equals(".flac", StringComparison.OrdinalIgnoreCase)
            ? "-c:a flac -sample_fmt s16"
            : extension.Equals(".wav", StringComparison.OrdinalIgnoreCase)
                ? "-c:a pcm_s16le"
                : "-c:a copy";

        var arguments =
            $"-hide_banner -nostdin -y -ss {fromSeconds.ToString("0.###", CultureInfo.InvariantCulture)} " +
            $"-i \"{audioFilePath}\" -vn {codec} \"{outputPath}\"";

        try
        {
            using var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo(ffmpeg, arguments)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                },
            };

            process.Start();
            await process.WaitForExitAsync(ct);
            if (process.ExitCode == 0 && File.Exists(outputPath) && new FileInfo(outputPath).Length > 0)
            {
                return outputPath;
            }

            _settings.Logger?.Invoke($"Google Cloud: could not cut the remaining audio for recovery (ffmpeg exit {process.ExitCode})");
        }
        catch (Exception exception)
        {
            _settings.Logger?.Invoke("Google Cloud: could not cut the remaining audio for recovery: " + exception.Message);
        }

        return null;
    }

    private static void TryDelete(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch
        {
            // Left in the run's temp folder, which SE cleans up afterwards.
        }
    }

    internal static string ReadProjectId(string keyFile)
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(keyFile));
        if (doc.RootElement.TryGetProperty("project_id", out var p) && p.ValueKind == JsonValueKind.String)
        {
            return p.GetString()!;
        }

        throw new HttpRequestException("The key file is not a Google Cloud service account key (no project_id). In the Google Cloud console create a service account key of type JSON.");
    }

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string url, HttpContent? content, CancellationToken ct)
    {
        var token = await _credential!.GetAccessTokenForRequestAsync(null, ct);
        using var request = new HttpRequestMessage(method, url) { Content = content };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
    }

    private async Task<string> SendAndReadAsync(HttpMethod method, string url, HttpContent? content, string what, CancellationToken ct)
    {
        using var response = await SendAsync(method, url, content, ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
        {
            _settings.Logger?.Invoke($"Google Cloud {what} failed: {method} {url} => {(int)response.StatusCode}: {body}");
            var hint = response.StatusCode == HttpStatusCode.Forbidden
                ? " Grant the service account the 'Cloud Speech Client' and 'Storage Admin' roles and enable the Speech-to-Text API."
                : string.Empty;
            throw new HttpRequestException($"Google Cloud {what} failed ({(int)response.StatusCode}).{hint} Response: {body}");
        }

        return body;
    }

    private async Task EnsureBucketAsync(string projectId, string bucket, CancellationToken ct)
    {
        using var probe = await SendAsync(HttpMethod.Get, $"{StorageApi}/b/{bucket}", null, ct);
        if (probe.StatusCode != HttpStatusCode.NotFound)
        {
            if (!probe.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Google Cloud bucket check failed ({(int)probe.StatusCode}). Response: {await probe.Content.ReadAsStringAsync(ct)}");
            }
            return;
        }

        _settings.Logger?.Invoke($"Google Cloud: creating bucket {bucket}");
        var location = GetSpeechHost(_settings.Region) == "speech.googleapis.com" ? "us" : _settings.Region.Trim();
        var body = JsonSerializer.Serialize(new
        {
            name = bucket,
            location,
            // Objects are deleted after each run; the rule sweeps leftovers from a killed run.
            lifecycle = new { rule = new[] { new { action = new { type = "Delete" }, condition = new { age = 1 } } } },
        });
        using var content = new StringContent(body, Encoding.UTF8, "application/json");
        await SendAndReadAsync(HttpMethod.Post, $"{StorageApi}/b?project={Uri.EscapeDataString(projectId)}", content, "bucket create", ct);
    }

    private async Task UploadAsync(string bucket, string objectName, string filePath, CancellationToken ct)
    {
        await using var stream = File.OpenRead(filePath);
        using var content = new StreamContent(stream);
        content.Headers.ContentType = new MediaTypeHeaderValue(Path.GetExtension(filePath).ToLowerInvariant() switch
        {
            ".mp3" => "audio/mpeg",
            ".wav" => "audio/wav",
            ".flac" => "audio/flac",
            _ => "application/octet-stream",
        });
        await SendAndReadAsync(HttpMethod.Post,
            $"{StorageUploadApi}/b/{bucket}/o?uploadType=media&name={Uri.EscapeDataString(objectName)}", content, "upload", ct);
    }

    private async Task DeleteObjectAsync(string bucket, string objectName)
    {
        try
        {
            using var _ = await SendAsync(HttpMethod.Delete, $"{StorageApi}/b/{bucket}/o/{Uri.EscapeDataString(objectName)}", null, CancellationToken.None);
        }
        catch
        {
            // Swept by the bucket's one-day lifecycle rule instead.
        }
    }

    private async Task<string> SubmitAsync(string projectId, string gcsUri, string? language, CancellationToken ct)
    {
        var region = _settings.Region.Trim();
        var url = $"https://{GetSpeechHost(region)}/v2/projects/{projectId}/locations/{(region.Length == 0 ? "global" : region)}/recognizers/_:batchRecognize";
        using var content = new StringContent(BuildRequestBody(_settings, gcsUri, language), Encoding.UTF8, "application/json");
        var json = await SendAndReadAsync(HttpMethod.Post, url, content, "batchRecognize", ct);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("name").GetString()!;
    }

    /// <summary>"auto" lets Chirp detect the language; other models need a BCP-47 code such as en-US.</summary>
    internal static string BuildRequestBody(GoogleCloudSttSettings settings, string gcsUri, string? language)
    {
        var lang = string.IsNullOrWhiteSpace(language) ? settings.Language : language;
        var config = new
        {
            autoDecodingConfig = new { },
            languageCodes = new[] { string.IsNullOrWhiteSpace(lang) ? "auto" : lang.Trim() },
            model = string.IsNullOrWhiteSpace(settings.Model) ? "chirp_3" : settings.Model.Trim(),
            features = new { enableWordTimeOffsets = true, enableAutomaticPunctuation = true },
        };

        var files = new[] { new { uri = gcsUri } };
        var output = new { inlineResponseConfig = new { } };

        // DYNAMIC_BATCHING is billed at about a fifth of the normal rate. Only sent when
        // asked for: it carries no latency guarantee, and the default timeout is an hour.
        return settings.DynamicBatching
            ? JsonSerializer.Serialize(new { config, files, recognitionOutputConfig = output, processingStrategy = "DYNAMIC_BATCHING" })
            : JsonSerializer.Serialize(new { config, files, recognitionOutputConfig = output });
    }

    private async Task<string> PollAsync(string operationName, CancellationToken ct)
    {
        var url = $"https://{GetSpeechHost(_settings.Region)}/v2/{operationName}";
        while (true)
        {
            await Task.Delay(TimeSpan.FromSeconds(5), ct);
            var json = await SendAndReadAsync(HttpMethod.Get, url, null, "operation poll", ct);
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("done", out var done) && done.GetBoolean())
            {
                if (doc.RootElement.TryGetProperty("error", out var error))
                {
                    throw new HttpRequestException($"Google Cloud transcription failed: {error}");
                }

                return json;
            }
        }
    }

    /// <summary>
    /// Map a finished operation into timed segments: one per Google result, timed
    /// by its first and last word. Words are range-checked against the billed
    /// duration because the API has been seen returning offsets far outside the
    /// audio; a result whose words are all bogus falls back to resultEndOffset.
    /// </summary>
    internal static OpenAiCompatibleSttResponse ParseResponse(string json, Action<string>? logger = null)
    {
        var response = new OpenAiCompatibleSttResponse { Segments = new List<OpenAiCompatibleSegment>() };
        using var doc = JsonDocument.Parse(json);
        if (!doc.RootElement.TryGetProperty("response", out var batch) || !batch.TryGetProperty("results", out var files))
        {
            return response;
        }

        var billed = batch.TryGetProperty("totalBilledDuration", out var b) ? ParseDuration(b) : 0.0;

        // totalBilledDuration is the natural bound for the range check below, but it is not
        // guaranteed to be present. Without a fallback the check degrades to "not negative
        // and not inverted", which lets through exactly the kind of value it exists to
        // catch: a word claiming 6,324 s inside a 1,080 s chunk. resultEndOffset is reported
        // per result and serves as an upper bound when the billed duration is missing.
        var bound = billed > 0 ? billed : LargestResultEndOffset(files);
        var text = new StringBuilder();
        var previousEnd = 0.0;
        foreach (var file in files.EnumerateObject())
        {
            if (file.Value.TryGetProperty("error", out var error) && error.TryGetProperty("message", out var message))
            {
                throw new HttpRequestException($"Google Cloud could not transcribe the audio: {message.GetString()}");
            }

            if (!file.Value.TryGetProperty("inlineResult", out var inline) ||
                !inline.TryGetProperty("transcript", out var transcript) ||
                !transcript.TryGetProperty("results", out var results))
            {
                continue;
            }

            foreach (var result in results.EnumerateArray())
            {
                if (!result.TryGetProperty("alternatives", out var alternatives) || alternatives.GetArrayLength() == 0)
                {
                    continue;
                }

                var alternative = alternatives[0];
                var segmentText = alternative.TryGetProperty("transcript", out var t) ? (t.GetString() ?? string.Empty).Trim() : string.Empty;
                if (segmentText.Length == 0)
                {
                    continue;
                }

                if (response.Language == null && result.TryGetProperty("languageCode", out var lc))
                {
                    response.Language = lc.GetString()?.Split('-')[0];
                }

                var words = new List<OpenAiCompatibleWord>();
                if (alternative.TryGetProperty("words", out var wordArray))
                {
                    foreach (var w in wordArray.EnumerateArray())
                    {
                        var start = w.TryGetProperty("startOffset", out var s) ? ParseDuration(s) : -1;
                        var end = w.TryGetProperty("endOffset", out var e) ? ParseDuration(e) : -1;
                        var word = w.TryGetProperty("word", out var wt) ? wt.GetString() ?? string.Empty : string.Empty;
                        if (start < 0 || end < start || (bound > 0 && end > bound + 1))
                        {
                            logger?.Invoke($"Google Cloud: dropped word '{word}' with impossible timing {start:0.###}-{end:0.###} s");
                            continue;
                        }

                        words.Add(new OpenAiCompatibleWord { Word = word, Start = start, End = end });
                    }
                }

                var resultEnd = result.TryGetProperty("resultEndOffset", out var re) ? ParseDuration(re) : previousEnd;
                var segment = new OpenAiCompatibleSegment
                {
                    Id = response.Segments.Count,
                    Start = words.Count > 0 ? words[0].Start : previousEnd,
                    End = words.Count > 0 ? words[^1].End : resultEnd,
                    Text = segmentText,
                    Words = words.Count > 0 ? words : null,
                };
                previousEnd = segment.End;
                response.Segments.Add(segment);
                text.Append(segmentText).Append(' ');
            }
        }

        response.Text = text.ToString().Trim();
        response.Duration = billed > 0 ? billed : null;
        if (billed > 60 && previousEnd < billed * 0.9)
        {
            // Google has been seen stopping part-way through a chunk while still reporting success.
            logger?.Invoke($"Google Cloud: transcript ends at {previousEnd:0.#} s but {billed:0.#} s of audio was billed - part of the audio may be missing");
        }

        return response;
    }

    /// <summary>Largest resultEndOffset in the response, used when no billed duration is reported.</summary>
    private static double LargestResultEndOffset(JsonElement files)
    {
        var largest = 0.0;
        foreach (var file in files.EnumerateObject())
        {
            if (!file.Value.TryGetProperty("inlineResult", out var inline) ||
                !inline.TryGetProperty("transcript", out var transcript) ||
                !transcript.TryGetProperty("results", out var results))
            {
                continue;
            }

            foreach (var result in results.EnumerateArray())
            {
                if (result.TryGetProperty("resultEndOffset", out var re))
                {
                    largest = Math.Max(largest, ParseDuration(re));
                }
            }
        }

        return largest;
    }

    /// <summary>REST encodes proto Durations as strings like "1.500s".</summary>
    internal static double ParseDuration(JsonElement element)
    {
        var s = element.ValueKind == JsonValueKind.String ? element.GetString() ?? string.Empty : element.GetRawText();
        return double.TryParse(s.TrimEnd('s'), NumberStyles.Float, CultureInfo.InvariantCulture, out var d) ? d : -1;
    }
}
