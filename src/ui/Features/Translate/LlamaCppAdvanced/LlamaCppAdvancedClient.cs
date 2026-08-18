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
///
/// Requests are streamed (SSE) rather than buffered: a grammar-cornered model that generates
/// forever now trips the inter-token watchdog instead of sitting silent until the HTTP timeout,
/// and cancelling mid-generation closes the connection, which makes llama-server abort the slot
/// instead of finishing a reply nobody reads (#13830).
/// </summary>
public class LlamaCppAdvancedClient : IDisposable
{
    private readonly HttpClient _httpClient;

    // Watchdog knobs (internal so tests can shrink them). FirstData covers prompt processing,
    // which streams nothing - a large context on slow hardware can legitimately take minutes
    // before the first token. Idle applies between chunks once generation has started.
    internal static TimeSpan FirstDataTimeout = TimeSpan.FromMinutes(5);
    internal static TimeSpan IdleTimeout = TimeSpan.FromMinutes(2);
    internal static TimeSpan TotalTimeout = TimeSpan.FromMinutes(15);

    public string Error { get; private set; } = string.Empty;

    public LlamaCppAdvancedClient()
    {
        _httpClient = HttpClientFactoryWithProxy.CreateHttpClientWithProxy();
        _httpClient.Timeout = TimeSpan.FromMinutes(15);
    }

    public async Task<string> ChatAsync(string url, string systemPrompt, string userContent, string? responseFormatJson, CancellationToken cancellationToken, string? model = null, int defaultMaxTokens = -1)
    {
        Error = string.Empty;

        var response = await PostAsync(url, BuildRequestJson(systemPrompt, userContent, responseFormatJson, model, defaultMaxTokens), cancellationToken);

        // A stall is not a format rejection - retrying the same prompt with a looser format
        // would just burn another watchdog period, so only the format fallbacks run here.
        if (!response.ok && !response.stalled && responseFormatJson != null && !cancellationToken.IsCancellationRequested)
        {
            response = await PostAsync(url, BuildRequestJson(systemPrompt, userContent, "{\"type\":\"json_object\"}", model, defaultMaxTokens), cancellationToken);
        }

        if (!response.ok && !response.stalled && responseFormatJson != null && !cancellationToken.IsCancellationRequested)
        {
            response = await PostAsync(url, BuildRequestJson(systemPrompt, userContent, null, model, defaultMaxTokens), cancellationToken);
        }

        if (!response.ok)
        {
            Error = response.body;
            SeLogger.Error("llama.cpp advanced translate: engine call failed: " + response.body);
            throw new HttpRequestException(ShortError(response.body));
        }

        // Streamed replies arrive already assembled from the deltas; a server that ignored
        // "stream" returns a regular completion object instead.
        return response.isAssembledContent ? response.body : ExtractContent(response.body);
    }

    /// <summary>
    /// The "model" field is only written when the caller supplies one (Ollama requires it;
    /// llama-server serves the single model it was started with, and sending one would only
    /// risk a mismatch). cache_prompt is llama.cpp-specific; other servers ignore it.
    /// </summary>
    private static string BuildRequestJson(string systemPrompt, string userContent, string? responseFormatJson, string? model, int defaultMaxTokens)
    {
        using var stream = new System.IO.MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            if (!string.IsNullOrWhiteSpace(model))
            {
                writer.WriteString("model", model);
            }

            writer.WriteBoolean("stream", true);
            writer.WriteBoolean("cache_prompt", true);

            WriteSampling(writer, defaultMaxTokens);

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
    /// stayed consistent with no downside. max_tokens falls back to the caller's batch-derived
    /// cap so a looping model runs out of tokens (a retryable failed batch) instead of
    /// generating until the context fills (#13830).
    /// </summary>
    private static void WriteSampling(Utf8JsonWriter writer, int defaultMaxTokens)
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

        var maxTokens = advanced.MaxTokens > 0 ? advanced.MaxTokens : defaultMaxTokens;
        if (maxTokens > 0)
        {
            writer.WriteNumber("max_tokens", maxTokens);
        }
    }

    private static void WriteNumberIfSet(Utf8JsonWriter writer, string name, double value)
    {
        if (value >= 0)
        {
            writer.WriteNumber(name, value);
        }
    }

    /// <summary>
    /// Sends one streamed chat-completions request and assembles the SSE deltas.
    /// <c>stalled</c> is set when a watchdog aborted the request (no data within the timeout,
    /// or the total cap was hit) - as opposed to the server rejecting it, which is retryable
    /// with a different response format. A user cancellation propagates as
    /// <see cref="OperationCanceledException"/> instead.
    /// </summary>
    private async Task<(bool ok, bool stalled, bool isAssembledContent, string body)> PostAsync(string url, string json, CancellationToken cancellationToken)
    {
        using var overall = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        overall.CancelAfter(TotalTimeout);
        try
        {
            // The first-data watchdog also covers the wait for response headers - a wedged server
            // that accepts the connection but never answers should not get the full total timeout.
            // Re-armed on every received line; disposing the reader/stream on abort closes the
            // connection, which is what makes llama-server stop generating.
            using var idle = CancellationTokenSource.CreateLinkedTokenSource(overall.Token);
            idle.CancelAfter(FirstDataTimeout);

            var content = new StringContent(json, Encoding.UTF8);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
            using var result = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, idle.Token).ConfigureAwait(false);
            if (!result.IsSuccessStatusCode)
            {
                var errorBody = await result.Content.ReadAsStringAsync(idle.Token).ConfigureAwait(false);
                return (false, false, false, errorBody);
            }

            await using var stream = await result.Content.ReadAsStreamAsync(idle.Token).ConfigureAwait(false);
            using var reader = new System.IO.StreamReader(stream, Encoding.UTF8);

            var assembled = new StringBuilder();
            var raw = new StringBuilder();
            var sawSseData = false;

            while (true)
            {
                var line = await reader.ReadLineAsync(idle.Token).ConfigureAwait(false);
                if (line == null)
                {
                    break;
                }

                idle.CancelAfter(IdleTimeout);
                if (!line.StartsWith("data:", StringComparison.Ordinal))
                {
                    // Not SSE (a proxy or server that ignored "stream") - keep the raw body so it
                    // can be parsed as a regular completion object below.
                    raw.AppendLine(line);
                    continue;
                }

                var payload = line.Substring(5).Trim();
                if (payload == "[DONE]")
                {
                    break;
                }

                sawSseData = true;
                if (!TryAppendDelta(payload, assembled, out var chunkError))
                {
                    return (false, false, false, chunkError);
                }
            }

            return sawSseData
                ? (true, false, true, assembled.ToString())
                : (true, false, false, raw.ToString());
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            return (false, true, false,
                "The server stopped sending data (generation looks stalled) and the request was aborted - " +
                "the model may not fit in GPU memory, or it may be stuck in a generation loop.");
        }
        catch (Exception e)
        {
            return (false, false, false, e.Message);
        }
    }

    /// <summary>Appends one SSE chunk's delta content; returns false on an in-stream error object.</summary>
    private static bool TryAppendDelta(string payload, StringBuilder assembled, out string error)
    {
        error = string.Empty;
        try
        {
            using var doc = JsonDocument.Parse(payload);
            if (doc.RootElement.TryGetProperty("error", out var errorElement))
            {
                error = errorElement.ToString();
                return false;
            }

            if (doc.RootElement.TryGetProperty("choices", out var choices) &&
                choices.ValueKind == JsonValueKind.Array &&
                choices.GetArrayLength() > 0 &&
                choices[0].TryGetProperty("delta", out var delta) &&
                delta.TryGetProperty("content", out var contentElement) &&
                contentElement.ValueKind == JsonValueKind.String)
            {
                assembled.Append(contentElement.GetString());
            }

            return true;
        }
        catch (JsonException)
        {
            // Tolerate malformed keep-alive/comment chunks.
            return true;
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
