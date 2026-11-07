using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.UiLogic.Http;

namespace Nikse.SubtitleEdit.Features.Tools.AiReview;

/// <summary>
/// Minimal OpenAI-compatible chat-completions client - works against both Ollama
/// (http://localhost:11434/v1/chat/completions) and a local llama.cpp server.
/// </summary>
public class AiReviewClient : IDisposable
{
    private readonly HttpClient _httpClient;

    // A server accepts or rejects an optional parameter for every request alike, so remember what it
    // turned down instead of paying a failed request per chunk for the rest of the run.
    private bool _temperatureUnsupported;
    private bool _jsonObjectUnsupported;

    public string Error { get; private set; } = string.Empty;

    /// <summary>
    /// Gets problem replies - cut off at the length limit, or empty content - with the server's
    /// finish_reason and token usage (#16701). Null = no logging.
    /// </summary>
    public Action<string>? Log { get; set; }

    public AiReviewClient()
    {
        _httpClient = HttpClientFactoryWithProxy.CreateHttpClientWithProxy();
        _httpClient.Timeout = TimeSpan.FromMinutes(15);
    }

    public async Task<string> ChatAsync(string url, string model, string systemPrompt, string userContent, CancellationToken cancellationToken, string? apiKey = null, bool preferJsonObject = true, int? maxTokens = null)
    {
        Error = string.Empty;

        // AI review wants a JSON object back; the text box assistant wants the plain
        // reply, so it passes preferJsonObject: false to avoid a JSON-wrapped answer.
        var jsonMode = preferJsonObject && !_jsonObjectUnsupported;
        var includeTemperature = !_temperatureUnsupported;

        (bool ok, string body) response;
        while (true)
        {
            response = await PostAsync(url,
                BuildRequestJson(model, systemPrompt, userContent, jsonMode, includeTemperature, maxTokens),
                apiKey, cancellationToken);
            if (response.ok || cancellationToken.IsCancellationRequested)
            {
                break;
            }

            // OpenAI's reasoning models (gpt-5*, o*) only accept the default temperature and answer
            // "Unsupported value: 'temperature' does not support 0 with this model" - see issue #13473.
            if (includeTemperature && IsUnsupportedParameter(response.body, "temperature"))
            {
                _temperatureUnsupported = true;
                includeTemperature = false;
                continue;
            }

            // Some servers reject response_format, so a JSON request also retries plain. Only remember
            // it when the server actually named the parameter - a rate limit or a network blip must not
            // downgrade every later request in the run.
            if (jsonMode)
            {
                _jsonObjectUnsupported = IsUnsupportedParameter(response.body, "response_format");
                jsonMode = false;
                continue;
            }

            break;
        }

        if (!response.ok)
        {
            Error = response.body;
            SeLogger.Error("AI review: engine call failed: " + response.body);
            throw new HttpRequestException(ShortError(response.body));
        }

        var content = ExtractContent(response.body);
        if (Log != null)
        {
            var problem = DescribeProblemReply(response.body, content);
            if (problem != null)
            {
                Log(problem);
            }
        }

        return content;
    }

    /// <summary>
    /// A log line when the reply looks unusable for a reason the server states - finish_reason
    /// "length" (context or max_tokens too small) or empty content (often a thinking model that
    /// spent the whole budget on reasoning_content) - else null.
    /// </summary>
    internal static string? DescribeProblemReply(string body, string content)
    {
        string? finishReason = null;
        int? promptTokens = null;
        int? completionTokens = null;
        var reasoningLength = 0;
        try
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            if (root.TryGetProperty("choices", out var choices) && choices.ValueKind == JsonValueKind.Array && choices.GetArrayLength() > 0)
            {
                var choice = choices[0];
                if (choice.TryGetProperty("finish_reason", out var fr) && fr.ValueKind == JsonValueKind.String)
                {
                    finishReason = fr.GetString();
                }

                if (choice.TryGetProperty("message", out var message) &&
                    message.TryGetProperty("reasoning_content", out var reasoning) &&
                    reasoning.ValueKind == JsonValueKind.String)
                {
                    reasoningLength = reasoning.GetString()?.Length ?? 0;
                }
            }
            else if (root.TryGetProperty("done_reason", out var dr) && dr.ValueKind == JsonValueKind.String)
            {
                finishReason = dr.GetString(); // Ollama native
            }

            if (root.TryGetProperty("usage", out var usage) && usage.ValueKind == JsonValueKind.Object)
            {
                promptTokens = ReadInt(usage, "prompt_tokens");
                completionTokens = ReadInt(usage, "completion_tokens");
            }
        }
        catch (JsonException)
        {
            return null;
        }

        var cutOff = string.Equals(finishReason, "length", StringComparison.OrdinalIgnoreCase);
        var empty = string.IsNullOrWhiteSpace(content);
        if (!cutOff && !empty)
        {
            return null;
        }

        var sb = new StringBuilder("AI chat: ");
        sb.Append(cutOff
            ? "reply cut off at the length limit - the server context size or max_tokens is too small"
            : "reply has no content");
        sb.Append($" (finish_reason={finishReason ?? "?"}");
        sb.Append($", prompt_tokens={promptTokens?.ToString() ?? "?"}");
        sb.Append($", completion_tokens={completionTokens?.ToString() ?? "?"}");
        if (reasoningLength > 0)
        {
            sb.Append($", reasoning_content={reasoningLength} chars");
        }

        sb.Append(')');
        return sb.ToString();
    }

    private static int? ReadInt(JsonElement element, string name)
    {
        return element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var i) ? i : null;
    }

    /// <summary>
    /// True when an error body blames a request parameter by name, like OpenAI's
    /// "Unsupported value: 'temperature' does not support 0 with this model." (issue #13473).
    /// </summary>
    internal static bool IsUnsupportedParameter(string? body, string parameterName)
    {
        if (string.IsNullOrEmpty(body))
        {
            return false;
        }

        // The rejection keyword must sit near the parameter name: some servers echo the whole
        // submitted request in their error body, so the name plus a stray "invalid" about
        // something else entirely (a bad API key, say) must not downgrade every later request
        // in the run.
        var index = body.IndexOf(parameterName, StringComparison.OrdinalIgnoreCase);
        while (index >= 0)
        {
            var start = Math.Max(0, index - 120);
            var length = Math.Min(body.Length - start, parameterName.Length + 240);
            var nearby = body.AsSpan(start, length);
            if (nearby.Contains("unsupported", StringComparison.OrdinalIgnoreCase) ||
                nearby.Contains("not support", StringComparison.OrdinalIgnoreCase) ||
                nearby.Contains("unrecognized", StringComparison.OrdinalIgnoreCase) ||
                nearby.Contains("invalid", StringComparison.OrdinalIgnoreCase) ||
                nearby.Contains("unknown", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            index = body.IndexOf(parameterName, index + parameterName.Length, StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    /// <param name="maxTokens">Output cap; only for servers that take "max_tokens" (llama-server) - OpenAI's
    /// reasoning models reject it, so remote engines leave it null.</param>
    internal static string BuildRequestJson(string model, string systemPrompt, string userContent, bool jsonMode, bool includeTemperature, int? maxTokens = null)
    {
        using var stream = new System.IO.MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            if (!string.IsNullOrWhiteSpace(model))
            {
                writer.WriteString("model", model);
            }

            if (includeTemperature)
            {
                writer.WriteNumber("temperature", 0);
            }

            writer.WriteBoolean("stream", false);
            if (maxTokens is > 0)
            {
                writer.WriteNumber("max_tokens", maxTokens.Value);
            }

            if (jsonMode)
            {
                writer.WriteStartObject("response_format");
                writer.WriteString("type", "json_object");
                writer.WriteEndObject();
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

    private async Task<(bool ok, string body)> PostAsync(string url, string json, string? apiKey, CancellationToken cancellationToken)
    {
        var content = new StringContent(json, Encoding.UTF8);
        content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey.Trim());
            }

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

            // Ollama native /api/chat + /api/generate shapes
            if (doc.RootElement.TryGetProperty("message", out var msg) &&
                msg.TryGetProperty("content", out var c))
            {
                return c.GetString() ?? string.Empty;
            }

            if (doc.RootElement.TryGetProperty("response", out var resp))
            {
                return resp.GetString() ?? string.Empty;
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
