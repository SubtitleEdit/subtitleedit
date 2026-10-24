using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.Core.Common;

using Nikse.SubtitleEdit.UiLogic.Http;

namespace Nikse.SubtitleEdit.UiLogic.Translate.Llm;

/// <summary>
/// OpenAI-compatible chat-completions client shared by cloud and local translation profiles.
/// Requests grammar-constrained structured output (response_format json_schema); servers that
/// reject it (older llama.cpp, Ollama, other OpenAI-compatible endpoints) get a json_object and
/// finally a plain retry. cache_prompt is set explicitly so llama-server reuses the KV cache for
/// the stable prompt prefix across batches.
///
/// Requests are streamed (SSE) rather than buffered: a grammar-cornered model that generates
/// forever now trips the inter-token watchdog instead of sitting silent until the HTTP timeout,
/// and cancelling mid-generation closes the connection, which makes llama-server abort the slot
/// instead of finishing a reply nobody reads (#13830).
///
/// A server that files the answer under "reasoning_content" is read from there when "content"
/// stays empty: KoboldCpp does that for the whole reply when the chat template opens a think
/// block, because the JSON grammar never lets the model close it (#15009). A reasoning-only
/// reply also switches thinking off for the following requests of this client
/// (chat_template_kwargs.enable_thinking=false): a hybrid model the server was started without
/// "--reasoning off" for (a self-supplied .gguf, custom server arguments, a server SE did not
/// start) otherwise spends the whole token budget thinking and never reaches the translation.
/// </summary>
public class OpenAiLlmClient : ILlmClient
{
    private readonly HttpClient _httpClient;
    private readonly AdvancedTranslationSettings _settings;
    private readonly bool _llamaCpp;

    // Watchdog knobs (internal so tests can shrink them). FirstData covers prompt processing,
    // which streams nothing - a large context on slow hardware can legitimately take minutes
    // before the first token. Idle applies between chunks once generation has started.
    internal static TimeSpan FirstDataTimeout = TimeSpan.FromMinutes(5);
    internal static TimeSpan IdleTimeout = TimeSpan.FromMinutes(2);
    internal static TimeSpan TotalTimeout = TimeSpan.FromMinutes(15);

    public string Error { get; private set; } = string.Empty;

    /// <summary>True when the last reply had no "content" and was taken from "reasoning_content".</summary>
    public bool ReplyFromReasoning { get; private set; }

    /// <summary>
    /// Set once a reply came back in "reasoning_content" only; from then on every request asks
    /// the chat template to skip thinking. Servers that do not know the field ignore it.
    /// </summary>
    public bool DisableThinking { get; private set; }

    /// <summary>
    /// True when the request that produced the last reply (or failure) carried
    /// chat_template_kwargs.enable_thinking=false - <see cref="DisableThinking"/> can be set by
    /// that very reply, so it does not tell whether the server already had the flag.
    /// </summary>
    public bool LastRequestDisabledThinking { get; private set; }

    public OpenAiLlmClient(AdvancedTranslationSettings? settings = null, string apiKey = "", bool llamaCpp = true, HttpClient? httpClient = null)
    {
        _settings = settings ?? new();
        _llamaCpp = llamaCpp;
        _httpClient = httpClient ?? HttpClientFactoryWithProxy.CreateHttpClientWithProxy();
        if (!string.IsNullOrWhiteSpace(apiKey)) _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        _httpClient.Timeout = TimeSpan.FromMinutes(15);
    }

    public async Task<string> ChatAsync(string url, string systemPrompt, string userContent, string? responseFormatJson, CancellationToken cancellationToken, string? model = null, int defaultMaxTokens = -1, double temperatureBump = 0)
    {
        Error = string.Empty;
        ReplyFromReasoning = false;

        var response = await PostWithThinkingFallbackAsync(url, systemPrompt, userContent, responseFormatJson, model, defaultMaxTokens, temperatureBump, cancellationToken);
        for (var retry = 0; retry < 2 && (response.status == 429 || response.status >= 500); retry++)
        {
            await Task.Delay(TimeSpan.FromSeconds(retry + 1), cancellationToken).ConfigureAwait(false);
            response = await PostWithThinkingFallbackAsync(url, systemPrompt, userContent, responseFormatJson, model, defaultMaxTokens, temperatureBump, cancellationToken);
        }

        // A stall is not a format rejection - retrying the same prompt with a looser format
        // would just burn another watchdog period, so only the format fallbacks run here.
        if (!response.ok && !response.stalled && IsFormatRejection(response.status, response.body) && responseFormatJson != null && !cancellationToken.IsCancellationRequested)
        {
            response = await PostWithThinkingFallbackAsync(url, systemPrompt, userContent, "{\"type\":\"json_object\"}", model, defaultMaxTokens, temperatureBump, cancellationToken);
        }

        if (!response.ok && !response.stalled && IsFormatRejection(response.status, response.body) && responseFormatJson != null && !cancellationToken.IsCancellationRequested)
        {
            response = await PostWithThinkingFallbackAsync(url, systemPrompt, userContent, null, model, defaultMaxTokens, temperatureBump, cancellationToken);
        }

        if (!response.ok)
        {
            Error = response.body;
            SeLogger.Error("OpenAI translation: engine call failed: " + response.body);
            throw new HttpRequestException(ShortError(response.body), null, response.status > 0 ? (System.Net.HttpStatusCode)response.status : null);
        }

        // Streamed replies arrive already assembled from the deltas; a server that ignored
        // "stream" returns a regular completion object instead.
        var reply = response.isAssembledContent
            ? PickReply(response.body, response.reasoning, out var fromReasoning)
            : ExtractContent(response.body, out fromReasoning);
        ReplyFromReasoning = fromReasoning;
        DisableThinking |= fromReasoning;

        return reply;
    }

    /// <summary>
    /// Posts one request with the current <see cref="DisableThinking"/> flag. A strict
    /// OpenAI-compatible server can reject the unknown chat_template_kwargs field with a 4xx, so
    /// such a rejection is retried once without it; the flag stays off only when that retry gets
    /// through - when it fails too, the field was not the problem (e.g. a json_schema rejection
    /// that the format fallbacks handle) and the flag is kept.
    /// </summary>
    private async Task<(bool ok, bool stalled, bool isAssembledContent, string body, string reasoning, int status)> PostWithThinkingFallbackAsync(string url, string systemPrompt, string userContent, string? responseFormatJson, string? model, int defaultMaxTokens, double temperatureBump, CancellationToken cancellationToken)
    {
        LastRequestDisabledThinking = DisableThinking;
        var response = await PostAsync(url, BuildRequestJson(systemPrompt, userContent, responseFormatJson, model, defaultMaxTokens, temperatureBump, DisableThinking, _settings, _llamaCpp), cancellationToken);
        if (response.ok || response.stalled || !DisableThinking || !IsThinkingRejection(response.status, response.body) || cancellationToken.IsCancellationRequested)
        {
            return response;
        }

        var retry = await PostAsync(url, BuildRequestJson(systemPrompt, userContent, responseFormatJson, model, defaultMaxTokens, temperatureBump, false, _settings, _llamaCpp), cancellationToken);
        if (!retry.ok)
        {
            return response;
        }

        SeLogger.Error("OpenAI translation: server rejected chat_template_kwargs.enable_thinking, sending requests without it: " + response.body);
        DisableThinking = false;
        LastRequestDisabledThinking = false;
        return retry;
    }

    /// <summary>
    /// The "model" field is only written when the caller supplies one (Ollama requires it;
    /// llama-server serves the single model it was started with, and sending one would only
    /// risk a mismatch). cache_prompt is llama.cpp-specific; other servers ignore it.
    /// chat_template_kwargs.enable_thinking=false is read by llama-server (Jinja templates) and
    /// vLLM - the Qwen 3 / Qwen 3.5 family and their fine-tunes honour it.
    /// </summary>
    public static string BuildRequestJson(string systemPrompt, string userContent, string? responseFormatJson, string? model, int defaultMaxTokens, double temperatureBump = 0, bool disableThinking = false, AdvancedTranslationSettings? settings = null, bool llamaCpp = true)
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
            if (llamaCpp) writer.WriteBoolean("cache_prompt", true);

            WriteSampling(writer, settings ?? new(), defaultMaxTokens, temperatureBump, llamaCpp);

            if (disableThinking && llamaCpp)
            {
                writer.WriteStartObject("chat_template_kwargs");
                writer.WriteBoolean("enable_thinking", false);
                writer.WriteEndObject();
            }

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
    /// generating until the context fills (#13830). <paramref name="temperatureBump"/> raises the
    /// temperature for a retry after the model echoed the source untranslated - the same request
    /// at the same low temperature tends to echo again.
    /// </summary>
    private static void WriteSampling(Utf8JsonWriter writer, AdvancedTranslationSettings advanced, int defaultMaxTokens, double temperatureBump, bool llamaCpp)
    {


        var temperature = advanced.Temperature >= 0 ? advanced.Temperature
            : 0.2;
        if (temperatureBump > 0)
        {
            temperature = Math.Min(1.0, temperature + temperatureBump);
        }

        WriteNumberIfSet(writer, "temperature", temperature);
        WriteNumberIfSet(writer, "top_p", advanced.TopP);
        if (llamaCpp)
        {
            WriteNumberIfSet(writer, "top_k", advanced.TopK);
            WriteNumberIfSet(writer, "repeat_penalty", advanced.RepeatPenalty);
        }

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
    private async Task<(bool ok, bool stalled, bool isAssembledContent, string body, string reasoning, int status)> PostAsync(string url, string json, CancellationToken cancellationToken)
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
                return (false, false, false, errorBody, string.Empty, (int)result.StatusCode);
            }

            await using var stream = await result.Content.ReadAsStreamAsync(idle.Token).ConfigureAwait(false);
            using var reader = new System.IO.StreamReader(stream, Encoding.UTF8);

            var assembled = new StringBuilder();
            var reasoning = new StringBuilder();
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
                if (!TryAppendDelta(payload, assembled, reasoning, out var chunkError))
                {
                    return (false, false, false, chunkError, string.Empty, 0);
                }
            }

            return sawSseData
                ? (true, false, true, assembled.ToString(), reasoning.ToString(), (int)result.StatusCode)
                : (true, false, false, raw.ToString(), string.Empty, (int)result.StatusCode);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            return (false, true, false,
                "The server stopped sending data (generation looks stalled) and the request was aborted - " +
                "the model may not fit in GPU memory, or it may be stuck in a generation loop.",
                string.Empty, 0);
        }
        catch (Exception e)
        {
            return (false, false, false, e.Message, string.Empty, 0);
        }
    }

    /// <summary>
    /// Appends one SSE chunk's delta content (and, separately, its reasoning_content); returns
    /// false on an in-stream error object.
    /// </summary>
    internal static bool TryAppendDelta(string payload, StringBuilder assembled, StringBuilder reasoning, out string error)
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
                delta.ValueKind == JsonValueKind.Object)
            {
                if (delta.TryGetProperty("content", out var contentElement) &&
                    contentElement.ValueKind == JsonValueKind.String)
                {
                    assembled.Append(contentElement.GetString());
                }

                if (delta.TryGetProperty("reasoning_content", out var reasoningElement) &&
                    reasoningElement.ValueKind == JsonValueKind.String)
                {
                    reasoning.Append(reasoningElement.GetString());
                }
            }

            return true;
        }
        catch (JsonException)
        {
            // Tolerate malformed keep-alive/comment chunks.
            return true;
        }
    }

    /// <summary>
    /// The reply to parse: "content", or the reasoning text when the server left content blank.
    /// Safe for real chain-of-thought too - the protocol only accepts a complete JSON object, so
    /// thoughts without one fail the batch exactly as the empty content did.
    /// </summary>
    internal static string PickReply(string content, string reasoning, out bool fromReasoning)
    {
        fromReasoning = string.IsNullOrWhiteSpace(content) && !string.IsNullOrWhiteSpace(reasoning);
        return fromReasoning ? reasoning : content;
    }

    internal static string ExtractContent(string json, out bool fromReasoning)
    {
        fromReasoning = false;
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("choices", out var choices) &&
                choices.ValueKind == JsonValueKind.Array &&
                choices.GetArrayLength() > 0 &&
                choices[0].TryGetProperty("message", out var message) &&
                message.ValueKind == JsonValueKind.Object)
            {
                return PickReply(GetStringOrEmpty(message, "content"), GetStringOrEmpty(message, "reasoning_content"), out fromReasoning);
            }
        }
        catch (JsonException)
        {
            // fall through - treat the raw body as content
        }

        return json;
    }

    private static string GetStringOrEmpty(JsonElement element, string name)
    {
        return element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? string.Empty
            : string.Empty;
    }

    private static string ShortError(string body)
    {
        var s = (body ?? string.Empty).Trim();
        return s.Length > 300 ? s.Substring(0, 300) + "..." : s;
    }

    private static bool IsFormatRejection(int status, string body) =>
        (status == 400 || status == 422) &&
        (body.Contains("response_format", StringComparison.OrdinalIgnoreCase) || body.Contains("json_schema", StringComparison.OrdinalIgnoreCase));

    private static bool IsThinkingRejection(int status, string body) =>
        (status == 400 || status == 422) && body.Contains("chat_template_kwargs", StringComparison.OrdinalIgnoreCase);

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
