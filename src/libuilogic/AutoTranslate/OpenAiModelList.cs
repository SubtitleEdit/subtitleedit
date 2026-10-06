using Nikse.SubtitleEdit.UiLogic.Http;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.UiLogic.AutoTranslate
{
    /// <summary>
    /// Lists the models an OpenAI-compatible server offers (GET {base}/v1/models, "data[].id") -
    /// OpenAI, LM Studio, llama-server, vLLM, Ollama's /v1 endpoint and the like.
    /// </summary>
    public static class OpenAiModelList
    {
        private static readonly HttpClient HttpClient = CreateHttpClient();

        private static HttpClient CreateHttpClient()
        {
            var client = HttpClientFactoryWithProxy.CreateHttpClientWithProxy();
            client.Timeout = TimeSpan.FromSeconds(30);
            return client;
        }

        /// <summary>
        /// Derives the models url from a chat-completions url:
        /// "http://localhost:1234/v1/chat/completions" -> "http://localhost:1234/v1/models".
        /// A bare origin gets "/v1/models". Returns empty for an invalid url.
        /// </summary>
        public static string GetModelsUrl(string? chatCompletionsUrl)
        {
            var url = (chatCompletionsUrl ?? string.Empty).Trim().TrimEnd('/');
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                return string.Empty;
            }

            var path = uri.AbsolutePath.TrimEnd('/');
            foreach (var suffix in new[] { "/chat/completions", "/completions", "/models" })
            {
                if (path.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    path = path.Substring(0, path.Length - suffix.Length);
                    break;
                }
            }

            if (path.Length == 0)
            {
                path = "/v1";
            }

            return uri.GetLeftPart(UriPartial.Authority) + path + "/models";
        }

        /// <summary>
        /// Fetches the model ids. Sends "Authorization: Bearer" when an API key is given.
        /// Throws on network/HTTP/parse errors - callers log and show an empty list.
        /// </summary>
        public static async Task<List<string>> GetModelsAsync(string? chatCompletionsUrl, string? apiKey, CancellationToken cancellationToken = default)
        {
            var modelsUrl = GetModelsUrl(chatCompletionsUrl);
            if (string.IsNullOrEmpty(modelsUrl))
            {
                throw new ArgumentException("Invalid url: " + chatCompletionsUrl);
            }

            using var request = new HttpRequestMessage(HttpMethod.Get, modelsUrl);
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey.Trim());
            }

            using var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"GET {modelsUrl} failed with {(int)response.StatusCode} {response.StatusCode}: {json}");
            }

            return ParseModelIds(json);
        }

        /// <summary>
        /// Parses an OpenAI "list models" response: { "data": [ { "id": "..." }, ... ] }.
        /// </summary>
        public static List<string> ParseModelIds(string json)
        {
            var result = new List<string>();
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Object ||
                !document.RootElement.TryGetProperty("data", out var data) ||
                data.ValueKind != JsonValueKind.Array)
            {
                return result;
            }

            foreach (var item in data.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.Object &&
                    item.TryGetProperty("id", out var id) &&
                    id.ValueKind == JsonValueKind.String)
                {
                    var name = id.GetString();
                    if (!string.IsNullOrWhiteSpace(name) && !result.Contains(name))
                    {
                        result.Add(name);
                    }
                }
            }

            return result;
        }
    }
}
