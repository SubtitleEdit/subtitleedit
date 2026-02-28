using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Settings;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Core.Translate;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Core.AutoTranslate
{
    public class OpenRouterTranslate : IAutoTranslator, IDisposable
    {
        private HttpClient _httpClient;

        public static string StaticName { get; set; } = "OpenRouter";
        public override string ToString() => StaticName;
        public string Name => StaticName;
        public string Url => "https://openrouter.ai/";
        public string Error { get; set; }
        public int MaxCharacters => 1500;

        /// <summary>
        /// See https://openrouter.ai/docs/models
        /// </summary>
        public static string[] Models => new[]
        {
            // High-Reasoning & Multilingual (The Leaders)
            "openai/gpt-5.2",                  // Top-tier reasoning for complex linguistics
            "anthropic/claude-4.5-sonnet",     // Exceptional nuance and formal tone preservation
            "google/gemini-3-pro",             // Massive context for translating whole books
            "deepseek/deepseek-v3.2",          // The most cost-effective reasoning translator

            // Efficient / Fast Translation
            "google/gemini-3-flash",           // Speed-optimized with 2026 multimodal capabilities
            "meta-llama/llama-4-maverick",     // Meta's new MoE architecture, strong in 12+ languages
            "openai/gpt-5.2-mini",             // Replaces 4o-mini; faster and more accurate

            // Specialized & Open-Weight (High Translation Performance)
            "qwen/qwen3-32b",                  // Specifically dominant for Asian & Middle Eastern languages
            "mistralai/mistral-large-3",       // Superior for European romance languages
            "deepseek/deepseek-v3.2:free",     // High-quality free-tier alternative to R1
        };

        public void Initialize()
        {
            _httpClient?.Dispose();
            _httpClient = HttpClientFactoryWithProxy.CreateHttpClientWithProxy();
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "application/json");
            _httpClient.BaseAddress = new Uri(Configuration.Settings.Tools.OpenRouterUrl.TrimEnd('/'));
            _httpClient.Timeout = TimeSpan.FromMinutes(15);

            if (!string.IsNullOrEmpty(Configuration.Settings.Tools.OpenRouterApiKey))
            {
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + Configuration.Settings.Tools.OpenRouterApiKey);
            }
        }

        public List<TranslationPair> GetSupportedSourceLanguages()
        {
            return ListLanguages();
        }

        public List<TranslationPair> GetSupportedTargetLanguages()
        {
            return ListLanguages();
        }

        public async Task<string> Translate(string text, string sourceLanguageCode, string targetLanguageCode, CancellationToken cancellationToken)
        {
            var model = Configuration.Settings.Tools.OpenRouterModel;
            if (string.IsNullOrEmpty(model))
            {
                model = Models[0];
                Configuration.Settings.Tools.OpenRouterModel = model;
            }

            if (string.IsNullOrEmpty(Configuration.Settings.Tools.OpenRouterPrompt))
            {
                Configuration.Settings.Tools.OpenRouterPrompt = new ToolsSettings().OpenRouterPrompt;
            }
            var prompt = string.Format(Configuration.Settings.Tools.OpenRouterPrompt, sourceLanguageCode, targetLanguageCode);
            var input = "{\"model\": \"" + model + "\",\"messages\": [{ \"role\": \"user\", \"content\": \"" + Json.EncodeJsonText(prompt) + "\\n\\n" + Json.EncodeJsonText(text.Trim()) + "\" }]}";
            var content = new StringContent(input, Encoding.UTF8);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            var result = await _httpClient.PostAsync(string.Empty, content, cancellationToken);
            var bytes = await result.Content.ReadAsByteArrayAsync();
            var json = Encoding.UTF8.GetString(bytes).Trim();
            if (!result.IsSuccessStatusCode)
            {
                Error = json;
                SeLogger.Error("OpenRouter Translate failed calling API: Status code=" + result.StatusCode + Environment.NewLine + json);
            }

            result.EnsureSuccessStatusCode();

            var parser = new SeJsonParser();
            var resultText = parser.GetFirstObject(json, "content");
            if (resultText == null)
            {
                return string.Empty;
            }

            var outputText = Json.DecodeJsonText(resultText).Trim();
            if (outputText.StartsWith('"') && outputText.EndsWith('"') && !text.StartsWith('"'))
            {
                outputText = outputText.Trim('"').Trim();
            }

            outputText = ChatGptTranslate.FixNewLines(outputText);
            outputText = ChatGptTranslate.RemovePreamble(text, outputText);
            outputText = ChatGptTranslate.DecodeUnicodeEscapes(outputText);
            return outputText.Trim();
        }

        public static List<TranslationPair> ListLanguages()
        {
            return ChatGptTranslate.ListLanguages();
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
