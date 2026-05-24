using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Settings;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.OpenAiCompatible;

public class OpenAiSttService
{
    private static HttpClient? _sharedHttpClient;
    private static readonly object _sharedHttpClientLock = new();

    /// <summary>
    /// Shared HttpClient for the OpenAI Compatible STT service. Created once,
    /// reused across calls — a fresh HttpClient per request leaks sockets into
    /// TIME_WAIT and skips DNS caching. Timeout is set to InfiniteTimeSpan so
    /// per-call deadlines can be applied via a linked CancellationTokenSource;
    /// callers must NOT mutate Timeout on this instance.
    /// </summary>
    public static HttpClient SharedHttpClient
    {
        get
        {
            if (_sharedHttpClient != null)
            {
                return _sharedHttpClient;
            }

            lock (_sharedHttpClientLock)
            {
                if (_sharedHttpClient == null)
                {
                    var client = HttpClientFactoryWithProxy.CreateHttpClientWithProxy();
                    client.Timeout = Timeout.InfiniteTimeSpan;
                    _sharedHttpClient = client;
                }
            }

            return _sharedHttpClient;
        }
    }

    private readonly HttpClient _httpClient;
    private readonly OpenAiCompatibleSettings _settings;

    public OpenAiSttService(OpenAiCompatibleSettings settings)
        : this(SharedHttpClient, settings)
    {
    }

    public OpenAiSttService(HttpClient httpClient, OpenAiCompatibleSettings settings)
    {
        _httpClient = httpClient;
        _settings = settings;
        // Don't mutate _httpClient.Timeout — the instance may be shared. The
        // per-call timeout from settings.TimeoutSeconds is applied via a
        // linked CancellationTokenSource in TranscribeAsync.
    }

    public async Task<OpenAiCompatibleSttResponse> TranscribeAsync(
        string audioFilePath,
        string? language = null,
        IProgress<string>? progress = null,
        IProgress<OpenAiCompatibleSegment>? segmentProgress = null,
        CancellationToken cancellationToken = default)
    {
        using var fileStream = File.OpenRead(audioFilePath);
        return await TranscribeAsync(fileStream, Path.GetFileName(audioFilePath), language, progress, segmentProgress, cancellationToken);
    }

    public async Task<OpenAiCompatibleSttResponse> TranscribeAsync(
        Stream audioStream,
        string fileName,
        string? language = null,
        IProgress<string>? progress = null,
        IProgress<OpenAiCompatibleSegment>? segmentProgress = null,
        CancellationToken cancellationToken = default)
    {
        // Apply the per-call deadline via a linked CTS rather than the shared
        // HttpClient.Timeout, so the shared client stays unmodified.
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        if (_settings.TimeoutSeconds > 0)
        {
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(_settings.TimeoutSeconds));
        }
        cancellationToken = timeoutCts.Token;

        using var content = new MultipartFormDataContent();

        // Add file
        var fileContent = new StreamContent(audioStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");
        content.Add(fileContent, "file", fileName);

        // Add model
        if (!string.IsNullOrEmpty(_settings.Model))
        {
            content.Add(new StringContent(_settings.Model), "model");
        }

        // Add language if specified
        var languageToUse = language ?? _settings.Language;
        if (!string.IsNullOrEmpty(languageToUse))
        {
            content.Add(new StringContent(languageToUse), "language");
        }

        // Pick response_format based on streaming: OpenAI's streaming endpoint
        // only emits `json`, while `timestamp_granularities[]` is rejected
        // unless `response_format=verbose_json` (issue #11146). Send the
        // granularity hints only with verbose_json — segments come through
        // the SSE `transcript.text.done` event anyway during streaming.
        var responseFormat = _settings.Stream ? "json" : "verbose_json";
        content.Add(new StringContent(responseFormat), "response_format");

        if (!_settings.Stream)
        {
            content.Add(new StringContent("segment"), "timestamp_granularities[]");
            content.Add(new StringContent("word"), "timestamp_granularities[]");
        }

        // Enable streaming (some OpenAI-compatible servers, e.g. Groq, reject this param)
        if (_settings.Stream)
        {
            content.Add(new StringContent("true"), "stream");
        }

        // Add temperature if specified
        if (_settings.Temperature > 0)
        {
            content.Add(new StringContent(_settings.Temperature.ToString("F2", CultureInfo.InvariantCulture)), "temperature");
        }

        // Add prompt if specified
        if (!string.IsNullOrEmpty(_settings.Prompt))
        {
            content.Add(new StringContent(_settings.Prompt), "prompt");
        }

        // Create request
        using var request = new HttpRequestMessage(HttpMethod.Post, _settings.EndpointUrl)
        {
            Content = content
        };

        // Add authorization header
        if (!string.IsNullOrEmpty(_settings.ApiKey))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
        }

        // Add extra headers if specified
        if (!string.IsNullOrEmpty(_settings.ExtraHeaders))
        {
            AddExtraHeaders(request.Headers, _settings.ExtraHeaders);
        }

        // Send request
        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var statusCode = (int)response.StatusCode;
            var temperatureSummary = _settings.Temperature > 0
                ? _settings.Temperature.ToString("F2", CultureInfo.InvariantCulture)
                : "(not sent)";
            var granularitiesSummary = _settings.Stream ? "(not sent)" : "[segment,word]";
            var paramSummary =
                $"model={_settings.Model}, language={languageToUse}, " +
                $"response_format={responseFormat}, timestamp_granularities={granularitiesSummary}, " +
                $"stream={(_settings.Stream ? "true" : "(not sent)")}, " +
                $"temperature={temperatureSummary}, " +
                $"promptLen={_settings.Prompt?.Length ?? 0}, file={fileName}";
            var safeEndpoint = SanitizeEndpointForLog(_settings.EndpointUrl);
            _settings.Logger?.Invoke(
                $"OpenAI-compatible STT failed: POST {safeEndpoint}{Environment.NewLine}" +
                $"Status: {statusCode} {response.StatusCode}{Environment.NewLine}" +
                $"RequestParams: {paramSummary}{Environment.NewLine}" +
                $"ResponseBody: {errorContent}");
            throw new HttpRequestException(
                $"STT request failed with status {statusCode} ({response.StatusCode}) " +
                $"calling {safeEndpoint}. Response: {errorContent}");
        }

        // Check if streaming (SSE)
        var contentType = response.Content.Headers.ContentType?.MediaType ?? "";
        if (contentType.Contains("text/event-stream") || response.Headers.Contains("Server-Sent-Events"))
        {
            return await ParseSseStreamAsync(response, progress, segmentProgress, cancellationToken);
        }

        // Non-streaming response
        var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
        return ParseJsonResponse(jsonResponse);
    }

    private static async Task<OpenAiCompatibleSttResponse> ParseSseStreamAsync(
        HttpResponseMessage response,
        IProgress<string>? progress = null,
        IProgress<OpenAiCompatibleSegment>? segmentProgress = null,
        CancellationToken cancellationToken = default)
    {
        var segments = new List<OpenAiCompatibleSegment>();
        var fullText = new StringBuilder();
        string? currentSegmentText = null;
        int currentSegmentId = 0;
        double currentSegmentStart = 0;
        double currentSegmentEnd = 0;

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        string? line;
        string eventType = "";
        var dataBuilder = new StringBuilder();

        while ((line = await reader.ReadLineAsync()) != null)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(line))
            {
                // Empty line = end of event, process it
                if (dataBuilder.Length > 0 && !string.IsNullOrEmpty(eventType))
                {
                    var data = dataBuilder.ToString().Trim();
                    ProcessSseEvent(eventType, data, segments, fullText, progress, segmentProgress, ref currentSegmentText, ref currentSegmentId, ref currentSegmentStart, ref currentSegmentEnd);
                }
                eventType = "";
                dataBuilder.Clear();
                continue;
            }

            if (line.StartsWith("event:"))
            {
                eventType = line.Substring(6).Trim();
            }
            else if (line.StartsWith("data:"))
            {
                dataBuilder.AppendLine(line.Substring(5).Trim());
            }
        }

        // Process any remaining event
        if (dataBuilder.Length > 0 && !string.IsNullOrEmpty(eventType))
        {
            var data = dataBuilder.ToString().Trim();
            ProcessSseEvent(eventType, data, segments, fullText, progress, segmentProgress, ref currentSegmentText, ref currentSegmentId, ref currentSegmentStart, ref currentSegmentEnd);
        }

        return new OpenAiCompatibleSttResponse
        {
            Text = fullText.ToString().Trim(),
            Segments = segments,
            Language = null,
            Duration = segments.Count > 0 ? segments[^1].End : 0
        };
    }

    private static void ProcessSseEvent(
        string eventType,
        string data,
        List<OpenAiCompatibleSegment> segments,
        StringBuilder fullText,
        IProgress<string>? progress,
        IProgress<OpenAiCompatibleSegment>? segmentProgress,
        ref string? currentSegmentText,
        ref int currentSegmentId,
        ref double currentSegmentStart,
        ref double currentSegmentEnd)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            options.Converters.Add(new JsonStringEnumConverter());

            if (eventType == "transcript.text.delta")
            {
                var deltaObj = JsonSerializer.Deserialize<TranscriptDelta>(data, options);
                if (deltaObj?.Delta != null)
                {
                    fullText.Append(deltaObj.Delta);
                    progress?.Report(deltaObj.Delta);
                }
            }
            else if (eventType == "transcript.text.done")
            {
                var doneObj = JsonSerializer.Deserialize<TranscriptDone>(data, options);
                if (doneObj?.Segments != null && doneObj.Segments.Count > 0)
                {
                    if (segments.Count == 0)
                    {
                        foreach (var seg in doneObj.Segments)
                        {
                            segments.Add(seg);
                            segmentProgress?.Report(seg);
                        }
                    }
                }
                else if (segments.Count == 0)
                {
                    // No segments from streaming or from done.segments array
                    if (!string.IsNullOrEmpty(doneObj?.Text) && fullText.Length > 0)
                    {
                        var newSeg = new OpenAiCompatibleSegment
                        {
                            Id = 0,
                            Start = 0,
                            End = 0,
                            Text = fullText.ToString().Trim()
                        };
                        segments.Add(newSeg);
                        segmentProgress?.Report(newSeg);
                    }
                }
            }
            else if (eventType == "transcript.segment" || eventType == "transcript.text.segment")
            {
                var segObj = JsonSerializer.Deserialize<TranscriptSegment>(data, options);
                if (segObj?.Segment != null)
                {
                    segments.Add(segObj.Segment);
                    segmentProgress?.Report(segObj.Segment);
                }
            }
        }
        catch (JsonException)
        {
            // Ignore malformed JSON
        }
    }

    private static OpenAiCompatibleSttResponse ParseJsonResponse(string jsonResponse)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            options.Converters.Add(new JsonStringEnumConverter());

            var result = JsonSerializer.Deserialize<OpenAiCompatibleSttResponse>(jsonResponse, options);
            if (result != null)
            {
                return result;
            }
        }
        catch (JsonException)
        {
            // If verbose_json fails, try simple text response
        }

        // Fallback: treat as plain text response
        return new OpenAiCompatibleSttResponse
        {
            Text = jsonResponse.Trim(),
            Segments = new List<OpenAiCompatibleSegment>
            {
                new OpenAiCompatibleSegment
                {
                    Start = 0,
                    End = 0,
                    Text = jsonResponse.Trim()
                }
            }
        };
    }

    /// <summary>
    /// Strip credentials from an endpoint URL before logging or echoing it back
    /// to the user. Removes any userinfo segment and redacts the values of
    /// query parameters that commonly carry secrets (api_key, apikey, key,
    /// token, access_token). Non-URL strings are returned unchanged.
    /// </summary>
    internal static string SanitizeEndpointForLog(string endpointUrl)
    {
        if (string.IsNullOrWhiteSpace(endpointUrl))
        {
            return endpointUrl;
        }

        if (!Uri.TryCreate(endpointUrl, UriKind.Absolute, out var uri))
        {
            return endpointUrl;
        }

        var builder = new UriBuilder(uri)
        {
            UserName = string.Empty,
            Password = string.Empty,
        };

        if (!string.IsNullOrEmpty(uri.Query))
        {
            var query = uri.Query.StartsWith('?') ? uri.Query[1..] : uri.Query;
            var redacted = new StringBuilder();
            foreach (var part in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                if (redacted.Length > 0)
                {
                    redacted.Append('&');
                }
                var eq = part.IndexOf('=');
                var name = eq >= 0 ? part[..eq] : part;
                if (eq >= 0 && IsSensitiveQueryParam(name))
                {
                    redacted.Append(name).Append("=***");
                }
                else
                {
                    redacted.Append(part);
                }
            }
            builder.Query = redacted.ToString();
        }

        return builder.Uri.ToString();
    }

    private static bool IsSensitiveQueryParam(string name)
    {
        return name.Equals("api_key", StringComparison.OrdinalIgnoreCase)
            || name.Equals("apikey", StringComparison.OrdinalIgnoreCase)
            || name.Equals("key", StringComparison.OrdinalIgnoreCase)
            || name.Equals("token", StringComparison.OrdinalIgnoreCase)
            || name.Equals("access_token", StringComparison.OrdinalIgnoreCase);
    }

    private static void AddExtraHeaders(HttpRequestHeaders headers, string extraHeaders)
    {
        if (string.IsNullOrWhiteSpace(extraHeaders))
        {
            return;
        }

        var lines = extraHeaders.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            var parts = line.Split(':', 2);
            if (parts.Length == 2)
            {
                var headerName = parts[0].Trim();
                var headerValue = parts[1].Trim();

                // Skip if header already exists (like Authorization)
                if (!headers.Contains(headerName))
                {
                    try
                    {
                        headers.Add(headerName, headerValue);
                    }
                    catch
                    {
                        // Ignore invalid headers
                    }
                }
            }
        }
    }

    public static OpenAiCompatibleSettings GetSettingsFromConfiguration()
    {
        var settings = Configuration.Settings.Tools;
        return new OpenAiCompatibleSettings
        {
            EndpointUrl = settings.OpenAiCompatibleSttUrl,
            ApiKey = settings.OpenAiCompatibleSttApiKey,
            Model = settings.OpenAiCompatibleSttModel,
            ExtraHeaders = settings.OpenAiCompatibleSttExtraHeaders,
            TimeoutSeconds = settings.OpenAiCompatibleSttTimeoutSeconds,
            Language = settings.OpenAiCompatibleSttLanguage,
            Temperature = (double)settings.OpenAiCompatibleSttTemperature,
            Prompt = settings.OpenAiCompatibleSttPrompt,
            Stream = settings.OpenAiCompatibleSttStream,
            Logger = Se.WriteToolsLog
        };
    }

    public static bool IsConfigured()
    {
        var settings = GetSettingsFromConfiguration();
        // For local servers, only endpoint URL is required
        return !string.IsNullOrEmpty(settings.EndpointUrl);
    }
}

public class OpenAiCompatibleSettings
{
    public string EndpointUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string ExtraHeaders { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 300;
    public string Language { get; set; } = string.Empty;
    public double Temperature { get; set; }
    public string Prompt { get; set; } = string.Empty;
    public bool Stream { get; set; }

    /// <summary>
    /// Optional diagnostic sink used on failure paths. Set by
    /// <see cref="OpenAiSttService.GetSettingsFromConfiguration"/> to
    /// <c>Se.WriteToolsLog</c>; left null in unit tests so the service
    /// does not touch the SE data folder.
    /// </summary>
    public Action<string>? Logger { get; set; }
}

// SSE event models
internal class TranscriptDelta
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("delta")]
    public string? Delta { get; set; }

    [JsonPropertyName("segment_id")]
    public int SegmentId { get; set; }
}

internal class TranscriptDone
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("segments")]
    public List<OpenAiCompatibleSegment>? Segments { get; set; }
}

internal class TranscriptSegment
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("segment")]
    public OpenAiCompatibleSegment? Segment { get; set; }
}
