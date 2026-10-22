using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Nikse.SubtitleEdit.UiLogic.Http;

namespace Nikse.SubtitleEdit.UiLogic.Translate.Llm;

public sealed class AnthropicLlmClient : ILlmClient
{
    private readonly HttpClient _httpClient;
    private readonly AdvancedTranslationSettings _settings;
    public string Error { get; private set; } = "";
    public bool ReplyFromReasoning => false;
    public bool LastRequestDisabledThinking => false;
    public AnthropicLlmClient(AdvancedTranslationSettings settings, string apiKey, HttpClient? httpClient = null)
    {
        _settings = settings;
        _httpClient = httpClient ?? HttpClientFactoryWithProxy.CreateHttpClientWithProxy();
        _httpClient.Timeout = TimeSpan.FromMinutes(15);
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("anthropic-version", "2023-06-01");
        if (!string.IsNullOrWhiteSpace(apiKey)) _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("x-api-key", apiKey);
    }
    public async Task<string> ChatAsync(string url, string systemPrompt, string userContent, string? responseFormatJson,
        CancellationToken cancellationToken, string? model = null, int defaultMaxTokens = -1, double temperatureBump = 0)
    {
        Error = "";
        var request = new Dictionary<string, object>
        {
            ["model"] = model ?? "",
            ["max_tokens"] = _settings.MaxTokens > 0 ? _settings.MaxTokens : Math.Max(1024, defaultMaxTokens),
            ["system"] = systemPrompt,
            ["messages"] = new[] { new { role = "user", content = userContent } },
            ["temperature"] = Math.Min(1, (_settings.Temperature >= 0 ? _settings.Temperature : 0.2) + temperatureBump),
        };
        // Anthropic rejects simultaneous temperature and top_p on some models; prefer explicit top_p.
        if (_settings.TopP >= 0) { request.Remove("temperature"); request["top_p"] = _settings.TopP; }
        if (_settings.TopK >= 0) request["top_k"] = _settings.TopK;
        var body = "";
        for (var attempt = 0; ; attempt++)
        {
            using var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            using var response = await _httpClient.PostAsync(url, content, cancellationToken).ConfigureAwait(false);
            body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            if (response.IsSuccessStatusCode) break;
            if (attempt < 2 && ((int)response.StatusCode == 429 || (int)response.StatusCode >= 500))
            {
                await Task.Delay(TimeSpan.FromSeconds(attempt + 1), cancellationToken).ConfigureAwait(false);
                continue;
            }
            Error = $"HTTP {(int)response.StatusCode}: {body[..Math.Min(body.Length, 500)]}";
            throw new HttpRequestException(Error, null, response.StatusCode);
        }
        using var document = JsonDocument.Parse(body);
        if (document.RootElement.TryGetProperty("stop_reason", out var stop) && stop.GetString() == "max_tokens")
            throw new HttpRequestException("Anthropic reply was truncated; increase max tokens or reduce the batch size.");
        var text = new StringBuilder();
        if (document.RootElement.TryGetProperty("content", out var blocks))
            foreach (var block in blocks.EnumerateArray())
                if (block.TryGetProperty("type", out var type) && type.GetString() == "text" && block.TryGetProperty("text", out var value))
                    text.Append(value.GetString());
        return text.ToString();
    }
    public void Dispose() => _httpClient.Dispose();
}
