using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.OpenAiCompatible;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.OpenRouter;

/// <summary>
/// Speech-to-text via OpenRouter's audio transcription API. Unlike OpenAI's
/// multipart <c>/v1/audio/transcriptions</c>, OpenRouter takes a JSON body with
/// the audio base64-encoded under <c>input_audio</c>. The documented response is
/// <c>text</c> + usage only. Whisper-compatible models receive
/// <c>response_format=verbose_json</c> and <c>timestamp_granularities[]</c> when
/// supported. OpenAI's newer GPT transcription models only accept
/// <c>response_format=json</c>, so they receive the plain response format and
/// no timestamp hints - matched by name shape via <see cref="IsJsonOnlyModel"/>.
/// OpenRouter also hosts other providers' transcription models (e.g. Google's
/// Chirp) that reject verbose_json for reasons no name pattern can predict.
/// Different providers wrap that rejection differently - sometimes with a
/// specific "response_format ... verbose_json" message, sometimes as an opaque
/// passthrough like "Provider returned 400" - so rather than pattern-matching
/// error text, any 400 on the initial verbose_json attempt gets one automatic
/// retry with plain json before failing. Chirp also rejects mp3 audio outright
/// (again via that same opaque "Provider returned 400"), which no request-level
/// retry can fix since the audio itself was already encoded upstream before
/// this service ever saw it - the caller picks wav instead of mp3 for those
/// models up front, via <see cref="RequiresWavAudio"/>. When only <c>text</c>
/// comes back, the caller's chunk pipeline spans each chunk's duration and
/// splits into sentences so timing survives.
/// </summary>
public class OpenRouterSttService : ISttTranscriber
{
    public const string DefaultEndpointUrl = "https://openrouter.ai/api/v1/audio/transcriptions";

    private readonly HttpClient _httpClient;
    private readonly OpenRouterSttSettings _settings;

    public OpenRouterSttService(OpenRouterSttSettings settings)
        : this(OpenAiSttService.SharedHttpClient, settings)
    {
    }

    public OpenRouterSttService(HttpClient httpClient, OpenRouterSttSettings settings)
    {
        _httpClient = httpClient;
        _settings = settings;
    }

    public Task<OpenAiCompatibleSttResponse> TranscribeAsync(
        string audioFilePath,
        string? language,
        IProgress<string>? progress,
        IProgress<OpenAiCompatibleSegment>? segmentProgress,
        CancellationToken cancellationToken)
    {
        var bytes = File.ReadAllBytes(audioFilePath);
        var format = OpenAiSttService.GetFileExtensionForFormat(Path.GetExtension(audioFilePath).TrimStart('.'));
        return TranscribeAsync(bytes, format, language, cancellationToken);
    }

    /// <summary>
    /// Build the OpenRouter request body and POST it. Exposed for unit tests so
    /// the JSON shape can be asserted without a network call via
    /// <see cref="BuildRequestBody"/>.
    /// </summary>
    public async Task<OpenAiCompatibleSttResponse> TranscribeAsync(
        byte[] audioBytes,
        string format,
        string? language,
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
            return await TranscribeCoreAsync(audioBytes, format, language, ct);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // Our own timeout fired, not a user cancel — surface it as an error
            // so the caller doesn't mistake it for cancellation.
            throw new TimeoutException($"OpenRouter transcription timed out after {_settings.TimeoutSeconds} seconds.");
        }
    }

    private async Task<OpenAiCompatibleSttResponse> TranscribeCoreAsync(
        byte[] audioBytes,
        string format,
        string? language,
        CancellationToken cancellationToken)
    {
        var forceJsonOnly = IsJsonOnlyModel(_settings.Model);
        var result = await SendOnceAsync(audioBytes, format, language, forceJsonOnly, cancellationToken);

        // IsJsonOnlyModel only recognizes OpenAI's own gpt-*-transcribe name shape. OpenRouter
        // hosts plenty of other providers' transcription models under the same endpoint (e.g.
        // Google's Chirp models) that also reject verbose_json, with no reliable way to know
        // that ahead of time short of maintaining a list of every provider's catalog - and no
        // reliable way to recognize the rejection by message text either, since OpenRouter
        // sometimes forwards the provider's specific complaint and sometimes just wraps it as
        // a generic "Provider returned 400". So any 400 on this first, verbose_json attempt is
        // reason enough to retry once with the plain json format before giving up.
        if (!result.Success && !forceJsonOnly && result.StatusCode == 400)
        {
            _settings.Logger?.Invoke(
                $"OpenRouter STT: model \"{_settings.Model}\" rejected the verbose_json request (400: {result.Body}), retrying with plain json.");
            result = await SendOnceAsync(audioBytes, format, language, forceJsonOnly: true, cancellationToken);
        }

        if (!result.Success)
        {
            _settings.Logger?.Invoke(
                $"OpenRouter STT failed: POST {_settings.EndpointUrl}{Environment.NewLine}" +
                $"Status: {result.StatusCode}{Environment.NewLine}" +
                $"RequestParams: model={_settings.Model}, language={language ?? _settings.Language}, format={format}, bytes={audioBytes.Length}{Environment.NewLine}" +
                $"ResponseBody: {result.Body}");
            throw new HttpRequestException(
                $"OpenRouter STT request failed with status {result.StatusCode}. Response: {result.Body}");
        }

        return ParseResponse(result.Body);
    }

    private async Task<(bool Success, int StatusCode, string Body)> SendOnceAsync(
        byte[] audioBytes,
        string format,
        string? language,
        bool forceJsonOnly,
        CancellationToken cancellationToken)
    {
        var body = BuildRequestBody(_settings, audioBytes, format, language, forceJsonOnly);
        using var content = new StringContent(body, Encoding.UTF8, "application/json");
        using var request = new HttpRequestMessage(HttpMethod.Post, _settings.EndpointUrl)
        {
            Content = content,
        };

        if (!string.IsNullOrEmpty(_settings.ApiKey))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
        }

        // OpenRouter uses these to attribute traffic; harmless if the server ignores them.
        request.Headers.TryAddWithoutValidation("HTTP-Referer", "https://www.nikse.dk/subtitleedit");
        request.Headers.TryAddWithoutValidation("X-Title", "Subtitle Edit");

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        var responseText = await response.Content.ReadAsStringAsync(cancellationToken);
        return (response.IsSuccessStatusCode, (int)response.StatusCode, responseText);
    }

    /// <summary>
    /// Serialize the OpenRouter transcription request body. The audio is
    /// base64-encoded (raw, not a data URI) under <c>input_audio</c>, and
    /// The newer GPT transcription models only accept <c>json</c>, while
    /// Whisper-compatible models can return <c>verbose_json</c> with segment and
    /// word timings.
    /// </summary>
    internal static string BuildRequestBody(OpenRouterSttSettings settings, byte[] audioBytes, string format, string? language)
        => BuildRequestBody(settings, audioBytes, format, language, IsJsonOnlyModel(settings.Model));

    /// <summary>
    /// As above, but lets the caller force the plain <c>json</c> format regardless of
    /// <see cref="IsJsonOnlyModel"/> - used to retry a model that turned out to reject
    /// <c>verbose_json</c> even though its name didn't match the known OpenAI shape.
    /// </summary>
    internal static string BuildRequestBody(OpenRouterSttSettings settings, byte[] audioBytes, string format, string? language, bool forceJsonOnly)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            writer.WriteString("model", settings.Model);

            writer.WritePropertyName("input_audio");
            writer.WriteStartObject();
            writer.WriteString("data", Convert.ToBase64String(audioBytes));
            writer.WriteString("format", string.IsNullOrWhiteSpace(format) ? "mp3" : format);
            writer.WriteEndObject();

            if (forceJsonOnly)
            {
                writer.WriteString("response_format", "json");
            }
            else
            {
                writer.WriteString("response_format", "verbose_json");

                writer.WritePropertyName("timestamp_granularities");
                writer.WriteStartArray();
                writer.WriteStringValue("segment");
                writer.WriteStringValue("word");
                writer.WriteEndArray();
            }

            var languageToUse = language ?? settings.Language;
            if (!string.IsNullOrWhiteSpace(languageToUse))
            {
                writer.WriteString("language", languageToUse);
            }

            if (settings.Temperature > 0)
            {
                writer.WriteNumber("temperature", settings.Temperature);
            }

            if (!string.IsNullOrWhiteSpace(settings.Prompt))
            {
                writer.WriteString("prompt", settings.Prompt);
            }

            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    /// <summary>
    /// OpenAI's GPT transcription models (e.g. <c>gpt-4o-transcribe</c>,
    /// <c>gpt-4o-mini-transcribe</c>) reject <c>verbose_json</c>, unlike Whisper.
    /// Matched by name shape rather than an exact list so future <c>gpt-*-transcribe</c>
    /// models are covered without a code change.
    /// </summary>
    internal static bool IsJsonOnlyModel(string? model)
    {
        if (string.IsNullOrWhiteSpace(model))
        {
            return false;
        }

        var name = model[(model.LastIndexOf('/') + 1)..];
        return name.StartsWith("gpt-", StringComparison.OrdinalIgnoreCase) &&
               name.Contains("transcribe", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Google's Chirp models reject mp3 audio outright - not a response_format
    /// complaint, just an opaque "Provider returned 400" from OpenRouter with no
    /// further detail (confirmed by sending byte-identical audio as mp3 vs. wav
    /// through OpenRouter's own playground: only wav succeeded). Matched by name
    /// shape, same reasoning as <see cref="IsJsonOnlyModel"/> - this covers future
    /// chirp-* models without a code change, though it can't help with some other
    /// provider's undocumented format requirement; that would need its own case
    /// once discovered.
    /// </summary>
    internal static bool RequiresWavAudio(string? model)
    {
        if (string.IsNullOrWhiteSpace(model))
        {
            return false;
        }

        var name = model[(model.LastIndexOf('/') + 1)..];
        return name.Contains("chirp", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Parse an OpenRouter transcription response into the shared shape. Reuses
    /// the OpenAI verbose_json model; if only word timings come back, they are
    /// grouped into segments, and a bare <c>text</c> response falls through to
    /// the caller's chunk-spanning path.
    /// </summary>
    internal static OpenAiCompatibleSttResponse ParseResponse(string json)
    {
        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<OpenAiCompatibleSttResponse>(json, options);
            if (result != null)
            {
                if ((result.Segments == null || result.Segments.Count == 0) &&
                    result.Words != null && result.Words.Count > 0)
                {
                    result.Segments = OpenAiSttService.BuildSegmentsFromWords(result.Words);
                }

                return result;
            }
        }
        catch (JsonException)
        {
            // Fall through to plain-text handling.
        }

        return new OpenAiCompatibleSttResponse { Text = json.Trim() };
    }

    public static OpenRouterSttSettings GetSettingsFromConfiguration()
    {
        var tools = Se.Settings.Tools;
        return new OpenRouterSttSettings
        {
            EndpointUrl = DefaultEndpointUrl,
            ApiKey = tools.OpenRouterSttApiKey,
            Model = tools.OpenRouterSttModel,
            Language = tools.OpenRouterSttLanguage,
            Temperature = (double)tools.OpenRouterSttTemperature,
            Prompt = tools.OpenRouterSttPrompt,
            TimeoutSeconds = tools.OpenRouterSttTimeoutSeconds,
            // See OpenAiSttService.GetSettingsFromConfiguration: hard-failure
            // diagnostics must survive the default-off "write tools log" setting.
            Logger = log => Se.WriteToolsLog(log, true),
        };
    }
}

public class OpenRouterSttSettings
{
    public string EndpointUrl { get; set; } = OpenRouterSttService.DefaultEndpointUrl;
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "openai/whisper-1";
    public string Language { get; set; } = string.Empty;
    public double Temperature { get; set; }
    public string Prompt { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 300;
    public Action<string>? Logger { get; set; }
}
