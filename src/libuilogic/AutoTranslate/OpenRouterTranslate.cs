using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Settings;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.UiLogic.Translate;
using System.Net.Http.Headers;
using System.Text;
using Nikse.SubtitleEdit.UiLogic.Http;

namespace Nikse.SubtitleEdit.UiLogic.AutoTranslate
{
    public class OpenRouterTranslate : IAutoTranslator, IDisposable
    {
        private HttpClient _httpClient = null!;

        public static string StaticName { get; set; } = "OpenRouter";
        public override string ToString() => StaticName;
        public string Name => StaticName;
        public string Url => "https://openrouter.ai/";
        public string Error { get; set; } = string.Empty;
        public int MaxCharacters => 1500;

        /// <summary>
        /// See https://openrouter.ai/docs/models
        /// </summary>
        public static string[] Models => new[]
        {
            // High-Reasoning & Multilingual (The Leaders)
            "openai/gpt-5.6-sol",                       // OpenAI flagship for complex professional work
            "openai/gpt-6.1-sol",                       // Newest OpenAI flagship
            "openai/gpt-6-sol",                         // GPT-6 flagship
            "openai/gpt-5.5",                           // Previous-gen flagship general-purpose translator
            "anthropic/claude-opus-5.5",                // Current Anthropic flagship for nuanced translation
            "anthropic/claude-sonnet-5.5",              // Strong balance of quality and cost
            "anthropic/claude-opus-5",                  // Previous Anthropic flagship
            "anthropic/claude-opus-4.8",                // Exceptional nuance and formal tone preservation
            "anthropic/claude-sonnet-5",                // Previous-gen Sonnet
            "google/gemini-4-argon",                    // Google's newest frontier model
            "google/gemini-3.1-pro-preview",            // Massive context for translating whole books
            "deepseek/deepseek-v4-pro",                 // The most cost-effective reasoning translator
            "z-ai/glm-5.3",                             // Strong open-weight generalist
            "z-ai/glm-5.2",                             // Previous GLM generalist
            "moonshotai/kimi-k3",                       // Strong for Asian languages
            "moonshotai/kimi-k2.6",                     // Previous Kimi
            "x-ai/grok-4.7",                            // xAI flagship

            // Efficient / Fast Translation
            "openai/gpt-5.6-terra",                     // Balanced intelligence and cost
            "openai/gpt-6-luna",                        // GPT-6 cost-optimized, high-volume
            "openai/gpt-5.6-luna",                      // Cost-optimized, high-volume
            "google/gemini-3.8-flash",                  // Google's newest flash model, cheaper and smarter
            "google/gemini-3.6-flash",                  // Strong previous-gen flash model
            "google/gemini-3.1-flash-lite",             // Speed-optimized multimodal
            "anthropic/claude-haiku-5.5",               // Fast Anthropic option
            "anthropic/claude-haiku-4.5",               // Previous fast Anthropic option
            "deepseek/deepseek-v4.1-flash",             // Low-latency DeepSeek

            // Specialized & Open-Weight (High Translation Performance)
            "qwen/qwen3.8-max-0902",                    // Dominant for Asian & Middle Eastern languages
            "meta-llama/llama-4-maverick",              // MoE architecture, strong multilingual coverage
            "mistralai/mistral-large-4-0",              // Superior for European romance languages (preview)
            "mistralai/mistral-large-2512",             // Mistral Large 3
            "mistralai/mistral-medium-3-5",             // Solid mid-tier European-language model

            // Free tier
            "google/gemma-4-31b-it:free",               // High-quality free-tier model
            "google/gemma-4-26b-a4b-it:free",           // Free-tier MoE alternative
        };

        /// <summary>
        /// Endpoint used when the url in settings is only the service base - see <see cref="AutoTranslateUrl"/>.
        /// </summary>
        public const string DefaultUrl = "https://openrouter.ai/api/v1/chat/completions";

        public void Initialize()
        {
            _httpClient?.Dispose();
            _httpClient = HttpClientFactoryWithProxy.CreateHttpClientWithProxy();
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "application/json");
            _httpClient.BaseAddress = new Uri(AutoTranslateUrl.Complete(Configuration.Settings.Tools.OpenRouterUrl, DefaultUrl));
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
            var input = "{\"model\": \"" + Json.EncodeJsonText(model) + "\",\"messages\": [{ \"role\": \"user\", \"content\": \"" + Json.EncodeJsonText(prompt) + "\\n\\n" + Json.EncodeJsonText(text.Trim()) + "\" }]}";

            int[] retryDelays = { 2555, 5007, 9013 };
            HttpResponseMessage result = null!;
            var json = string.Empty;
            for (var attempt = 0; attempt <= retryDelays.Length; attempt++)
            {
                var content = new StringContent(input, Encoding.UTF8);
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                result = await _httpClient.PostAsync(string.Empty, content, cancellationToken);
                var bytes = await result.Content.ReadAsByteArrayAsync(cancellationToken);
                json = Encoding.UTF8.GetString(bytes).Trim();

                if (!DeepLTranslate.ShouldRetry(result, json) || attempt == retryDelays.Length)
                {
                    break;
                }

                await Task.Delay(retryDelays[attempt], cancellationToken);
            }

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
