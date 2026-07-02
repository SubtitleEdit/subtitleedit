using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Tools.AiReview;

/// <summary>
/// Minimal OpenAI-compatible chat-completions client - works against both Ollama
/// (http://localhost:11434/v1/chat/completions) and a local llama.cpp server.
/// </summary>
public class AiReviewClient : IDisposable
{
    private readonly HttpClient _httpClient;

    public string Error { get; private set; } = string.Empty;

    public AiReviewClient()
    {
        _httpClient = HttpClientFactoryWithProxy.CreateHttpClientWithProxy();
        _httpClient.Timeout = TimeSpan.FromMinutes(15);
    }

    public async Task<string> ChatAsync(string url, string model, string systemPrompt, string userContent, CancellationToken cancellationToken)
    {
        Error = string.Empty;

        // First try with enforced JSON output; some servers reject response_format, so retry without.
        var response = await PostAsync(url, BuildRequestJson(model, systemPrompt, userContent, jsonMode: true), cancellationToken);
        if (!response.ok && !cancellationToken.IsCancellationRequested)
        {
            response = await PostAsync(url, BuildRequestJson(model, systemPrompt, userContent, jsonMode: false), cancellationToken);
        }

        if (!response.ok)
        {
            Error = response.body;
            SeLogger.Error("AI review: engine call failed: " + response.body);
            throw new HttpRequestException(ShortError(response.body));
        }

        return ExtractContent(response.body);
    }

    private static string BuildRequestJson(string model, string systemPrompt, string userContent, bool jsonMode)
    {
        using var stream = new System.IO.MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            if (!string.IsNullOrWhiteSpace(model))
            {
                writer.WriteString("model", model);
            }

            writer.WriteNumber("temperature", 0);
            writer.WriteBoolean("stream", false);
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

    private async Task<(bool ok, string body)> PostAsync(string url, string json, CancellationToken cancellationToken)
    {
        var content = new StringContent(json, Encoding.UTF8);
        content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
        try
        {
            var result = await _httpClient.PostAsync(url, content, cancellationToken).ConfigureAwait(false);
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
