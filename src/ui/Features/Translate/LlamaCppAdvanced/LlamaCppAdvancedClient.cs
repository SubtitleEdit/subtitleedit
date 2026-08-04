using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.Http;

namespace Nikse.SubtitleEdit.Features.Translate.LlamaCppAdvanced;

/// <summary>
/// OpenAI-compatible chat-completions client for the advanced llama.cpp translate engine.
/// Requests grammar-constrained structured output (response_format json_schema); servers that
/// reject it (older llama.cpp, Ollama, other OpenAI-compatible endpoints) get a json_object and
/// finally a plain retry. cache_prompt is set explicitly so llama-server reuses the KV cache for
/// the stable prompt prefix across batches.
/// </summary>
public class LlamaCppAdvancedClient : IDisposable
{
    private readonly HttpClient _httpClient;

    public string Error { get; private set; } = string.Empty;

    public LlamaCppAdvancedClient()
    {
        _httpClient = HttpClientFactoryWithProxy.CreateHttpClientWithProxy();
        _httpClient.Timeout = TimeSpan.FromMinutes(15);
    }

    public async Task<string> ChatAsync(string url, string systemPrompt, string userContent, string? responseFormatJson, CancellationToken cancellationToken, string? model = null)
    {
        Error = string.Empty;

        var response = await PostAsync(url, BuildRequestJson(systemPrompt, userContent, responseFormatJson, model), cancellationToken);
        if (!response.ok && responseFormatJson != null && !cancellationToken.IsCancellationRequested)
        {
            response = await PostAsync(url, BuildRequestJson(systemPrompt, userContent, "{\"type\":\"json_object\"}", model), cancellationToken);
        }

        if (!response.ok && responseFormatJson != null && !cancellationToken.IsCancellationRequested)
        {
            response = await PostAsync(url, BuildRequestJson(systemPrompt, userContent, null, model), cancellationToken);
        }

        if (!response.ok)
        {
            Error = response.body;
            SeLogger.Error("llama.cpp advanced translate: engine call failed: " + response.body);
            throw new HttpRequestException(ShortError(response.body));
        }

        return ExtractContent(response.body);
    }

    /// <summary>
    /// The "model" field is only written when the caller supplies one (Ollama requires it;
    /// llama-server serves the single model it was started with, and sending one would only
    /// risk a mismatch). cache_prompt is llama.cpp-specific; other servers ignore it.
    /// </summary>
    private static string BuildRequestJson(string systemPrompt, string userContent, string? responseFormatJson, string? model)
    {
        using var stream = new System.IO.MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            if (!string.IsNullOrWhiteSpace(model))
            {
                writer.WriteString("model", model);
            }

            writer.WriteBoolean("stream", false);
            writer.WriteBoolean("cache_prompt", true);

            WriteSampling(writer);

            if (responseFormatJson != null)
            {
                writer.WritePropertyName("response_format");
                writer.WriteRawValue(responseFormatJson);
            }

            writer.WriteStartArray("messages");
            writer.WriteStartObject();
            writer.WriteString("role", "system");
            writer.WriteString("content", systemPrompt);
            writer.WriteEndObject();
            writer.WriteStartObject();
            writer.WriteString("role", "user");
            writer.WriteString("content", userContent);
            writer.WriteEndObject();
            writer.WriteEndArray();

            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    /// <summary>
    /// Advanced-settings sampling overrides first, then the curated model's recommended values
    /// (persisted as Tools.LlamaCppModel* when the engine was selected), then server defaults -
    /// except temperature, which bottoms out at 0.2: the server default (0.8) drifts terminology
    /// across batches ("the Chief" became three different Danish words in testing), while 0.0-0.2
    /// stayed consistent with no downside.
    /// </summary>
    private static void WriteSampling(Utf8JsonWriter writer)
    {
        var advanced = Se.Settings.AutoTranslate.LlamaCppAdvanced;
        var tools = Configuration.Settings.Tools;

        var temperature = advanced.Temperature >= 0 ? advanced.Temperature
            : tools.LlamaCppModelTemperature >= 0 ? tools.LlamaCppModelTemperature
            : 0.2;
        WriteNumberIfSet(writer, "temperature", temperature);
        WriteNumberIfSet(writer, "top_p", advanced.TopP >= 0 ? advanced.TopP : tools.LlamaCppModelTopP);
        WriteNumberIfSet(writer, "top_k", advanced.TopK >= 0 ? advanced.TopK : tools.LlamaCppModelTopK);
        WriteNumberIfSet(writer, "repeat_penalty", advanced.RepeatPenalty >= 0 ? advanced.RepeatPenalty : tools.LlamaCppModelRepeatPenalty);
        WriteNumberIfSet(writer, "max_tokens", advanced.MaxTokens > 0 ? advanced.MaxTokens : -1);
    }

    private static void WriteNumberIfSet(Utf8JsonWriter writer, string name, double value)
    {
        if (value >= 0)
        {
            writer.WriteNumber(name, value);
        }
    }

    private async Task<(bool ok, string body)> PostAsync(string url, string json, CancellationToken cancellationToken)
    {
        var content = new StringContent(json, Encoding.UTF8);
        content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
            var result = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            var body = await result.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            return (result.IsSuccessStatusCode, body);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            return (false, e.Message);
        }
    }

    private static string ExtractContent(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("choices", out var choices) &&
                choices.ValueKind == JsonValueKind.Array &&
                choices.GetArrayLength() > 0 &&
                choices[0].TryGetProperty("message", out var message) &&
                message.TryGetProperty("content", out var contentElement))
            {
                return contentElement.GetString() ?? string.Empty;
            }
        }
        catch (JsonException)
        {
            // fall through - treat the raw body as content
        }

        return json;
    }

    private static string ShortError(string body)
    {
        var s = (body ?? string.Empty).Trim();
        return s.Length > 300 ? s.Substring(0, 300) + "..." : s;
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
