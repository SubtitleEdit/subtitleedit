using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.UiLogic.Translate;
using System.Net.Http.Headers;
using System.Text;
using Nikse.SubtitleEdit.Core.Settings;
using Nikse.SubtitleEdit.UiLogic.Http;

namespace Nikse.SubtitleEdit.UiLogic.AutoTranslate
{
    public class GroqTranslate : IAutoTranslator, IDisposable
    {
        private HttpClient _httpClient = null!;

        public static string StaticName { get; set; } = "Groq";
        public override string ToString() => StaticName;
        public string Name => StaticName;
        public string Url => "https://groq.com/";
        public string Error { get; set; } = string.Empty;
        public int MaxCharacters => 1500;

        /// <summary>
        /// See https://console.groq.com/docs/models
        /// </summary>
        public static string[] Models => new[]
        {
            // High-Performance / Frontier Multilingual
            "openai/gpt-oss-120b",                          // Extreme reasoning & nuance (Production)

            // Medium / Efficient Multilingual
            "openai/gpt-oss-20b",                           // High-speed reasoning model (Production)
            "qwen/qwen3.8-27b",                             // Strong multilingual mid-size model (Preview)
            "minimaxai/minimax-m2.7",                       // MiniMax M2.7 (Preview)
            "meta-models/muse-spark-open",                  // Meta Muse Spark open weights (Preview)
        };

        /// <summary>
        /// Models Groq has shut down (see https://console.groq.com/docs/deprecations).
        /// A saved setting pointing at one of these is moved to the default model.
        /// </summary>
        private static readonly HashSet<string> RetiredModels = new(StringComparer.OrdinalIgnoreCase)
        {
            "llama-3.3-70b-versatile",                      // shut down 2026-08-16
            "llama-3.1-8b-instant",                         // shut down 2026-08-16
            "groq/compound",                                // shut down 2026-09-21
            "groq/compound-mini",                           // shut down 2026-09-21
            "qwen/qwen3.6-27b",                             // shut down 2026-09-14
            "qwen/qwen3-32b",                               // shut down 2026-07-17
            "meta-llama/llama-4-scout-17b-16e-instruct",    // shut down 2026-07-17
            "meta-llama/llama-4-maverick-17b-128e-instruct",// shut down 2026-03-09
            "moonshotai/kimi-k2-instruct-0905",             // shut down 2026-04-15
        };

        /// <summary>
        /// Endpoint used when the url in settings is only the service base - see <see cref="AutoTranslateUrl"/>.
        /// </summary>
        public const string DefaultUrl = "https://api.groq.com/openai/v1/chat/completions";

        public void Initialize()
        {
            _httpClient?.Dispose();
            _httpClient = HttpClientFactoryWithProxy.CreateHttpClientWithProxy();
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "application/json");
            _httpClient.BaseAddress = new Uri(AutoTranslateUrl.Complete(Configuration.Settings.Tools.GroqUrl, DefaultUrl));
            _httpClient.Timeout = TimeSpan.FromMinutes(15);

            if (!string.IsNullOrEmpty(Configuration.Settings.Tools.GroqApiKey))
            {
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + Configuration.Settings.Tools.GroqApiKey);
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

        //curl -X POST "https://api.groq.com/openai/v1/chat/completions" \
        //-H "Authorization: Bearer $GROQ_API_KEY" \
        //-H "Content-Type: application/json" \
        //-d '{"messages": [{"role": "user", "content": "Explain the importance of fast language models"}], "model": "llama3-8b-8192"}

        /// <summary>
        /// Retired ids are only swapped on Groq's own endpoint - another OpenAI compatible server may still serve them.
        /// </summary>
        private static bool IsGroqCloud()
        {
            var url = Configuration.Settings.Tools.GroqUrl;
            return string.IsNullOrWhiteSpace(url) || url.Contains("api.groq.com", StringComparison.OrdinalIgnoreCase);
        }

        public async Task<string> Translate(string text, string sourceLanguageCode, string targetLanguageCode, CancellationToken cancellationToken)
        {
            var model = Configuration.Settings.Tools.GroqModel;
            if (string.IsNullOrEmpty(model) || (RetiredModels.Contains(model) && IsGroqCloud()))
            {
                model = Models[0];
                Configuration.Settings.Tools.GroqModel = model;
            }

            if (string.IsNullOrEmpty(Configuration.Settings.Tools.GroqPrompt))
            {
                Configuration.Settings.Tools.GroqPrompt = new ToolsSettings().GroqPrompt;
            }
            var prompt = string.Format(Configuration.Settings.Tools.GroqPrompt, sourceLanguageCode, targetLanguageCode);
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
                SeLogger.Error("Groq Translate failed calling API: Status code=" + result.StatusCode + Environment.NewLine + json);
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
