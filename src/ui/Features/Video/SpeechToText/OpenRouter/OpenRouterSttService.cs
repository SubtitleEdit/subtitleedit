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
/// the audio base64-encoded under <c>input_audio</c>. It proxies Whisper /
/// GPT-4o-transcribe / Groq / Chirp, so with <c>response_format=verbose_json</c>
/// and <c>timestamp_granularities[]</c> it returns the same segment/word shape
/// we already parse into <see cref="OpenAiCompatibleSttResponse"/>. When a model
/// or provider ignores those hints and returns only <c>text</c>, the caller's
/// chunk pipeline still spans each chunk's duration so timing survives.
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
        cancellationToken = timeoutCts.Token;

        var body = BuildRequestBody(_settings, audioBytes, format, language);
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
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var statusCode = (int)response.StatusCode;
            _settings.Logger?.Invoke(
                $"OpenRouter STT failed: POST {_settings.EndpointUrl}{Environment.NewLine}" +
                $"Status: {statusCode} {response.StatusCode}{Environment.NewLine}" +
                $"RequestParams: model={_settings.Model}, language={language ?? _settings.Language}, format={format}, bytes={audioBytes.Length}{Environment.NewLine}" +
                $"ResponseBody: {errorContent}");
            throw new HttpRequestException(
                $"OpenRouter STT request failed with status {statusCode} ({response.StatusCode}). Response: {errorContent}");
        }

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return ParseResponse(json);
    }

    /// <summary>
    /// Serialize the OpenRouter transcription request body. The audio is
    /// base64-encoded (raw, not a data URI) under <c>input_audio</c>, and
    /// <c>verbose_json</c> + <c>timestamp_granularities[]</c> ask for segment and
    /// word timings when the underlying model supports them.
    /// </summary>
    internal static string BuildRequestBody(OpenRouterSttSettings settings, byte[] audioBytes, string format, string? language)
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

            writer.WriteString("response_format", "verbose_json");

            writer.WritePropertyName("timestamp_granularities");
            writer.WriteStartArray();
            writer.WriteStringValue("segment");
            writer.WriteStringValue("word");
            writer.WriteEndArray();

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
        var tools = Configuration.Settings.Tools;
        return new OpenRouterSttSettings
        {
            EndpointUrl = DefaultEndpointUrl,
            ApiKey = tools.OpenRouterSttApiKey,
            Model = tools.OpenRouterSttModel,
            Language = tools.OpenRouterSttLanguage,
            Temperature = (double)tools.OpenRouterSttTemperature,
            Prompt = tools.OpenRouterSttPrompt,
            TimeoutSeconds = tools.OpenRouterSttTimeoutSeconds,
            Logger = Se.WriteToolsLog,
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
