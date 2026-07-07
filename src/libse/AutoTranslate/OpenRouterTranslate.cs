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
    public class OpenRouterTranslate : IAutoTranslator, IAutoTranslatorWithContext, IDisposable
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
            "openai/gpt-5.5-pro",                       // Top-tier reasoning for complex linguistics
            "openai/gpt-5.5",                           // Flagship general-purpose translator
            "anthropic/claude-opus-4.7",                // Exceptional nuance and formal tone preservation
            "anthropic/claude-sonnet-4.6",              // Strong balance of quality and cost
            "google/gemini-3.1-pro-preview",            // Massive context for translating whole books
            "deepseek/deepseek-v4-pro",                 // The most cost-effective reasoning translator

            // Efficient / Fast Translation
            "openai/gpt-5.4-mini",                      // Faster and cost-efficient
            "openai/gpt-5.4-nano",                      // Smallest/fastest in the GPT-5.4 family
            "google/gemini-3.1-flash-lite",             // Speed-optimized multimodal
            "anthropic/claude-haiku-4.5",               // Fast Anthropic option
            "deepseek/deepseek-v4-flash",               // Low-latency DeepSeek

            // Specialized & Open-Weight (High Translation Performance)
            "qwen/qwen3.6-max-preview",                 // Dominant for Asian & Middle Eastern languages
            "meta-llama/llama-4-maverick",              // MoE architecture, strong multilingual coverage
            "mistralai/mistral-large-2512",             // Superior for European romance languages
            "mistralai/mistral-medium-3-5",             // Solid mid-tier European-language model

            // Free tier
            "meta-llama/llama-3.3-70b-instruct:free",   // High-quality free-tier model
            "qwen/qwen3-next-80b-a3b-instruct:free",    // Free-tier MoE alternative
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
            return await Translate(text, sourceLanguageCode, targetLanguageCode, string.Empty, string.Empty, cancellationToken);
        }

        public async Task<string> Translate(string text, string sourceLanguageCode, string targetLanguageCode, string previousLineText, string nextLineText, CancellationToken cancellationToken)
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
            var prompt = string.Format(Configuration.Settings.Tools.OpenRouterPrompt, sourceLanguageCode, targetLanguageCode, previousLineText, nextLineText);
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
