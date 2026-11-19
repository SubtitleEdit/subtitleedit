using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.UiLogic.Translate;
using System.Net.Http.Headers;
using System.Text;
using Nikse.SubtitleEdit.Core.Settings;
using Nikse.SubtitleEdit.UiLogic.Http;

namespace Nikse.SubtitleEdit.UiLogic.AutoTranslate
{
    public class NvidiaTranslate : IAutoTranslator, IDisposable
    {
        private HttpClient _httpClient = null!;

        public static string StaticName { get; set; } = "NVIDIA";
        public override string ToString() => StaticName;
        public string Name => StaticName;
        public string Url => "https://build.nvidia.com/models";
        public string Error { get; set; } = string.Empty;
        public int MaxCharacters => 1500;

        /// <summary>
        /// See https://build.nvidia.com/models
        /// </summary>
        public static string[] Models => new[]
        {
            // NVIDIA Nemotron
            "nvidia/nemotron-3-ultra-550b-a55b",
            "nvidia/nemotron-3.5-super-vl-120b-a12b",
            "nvidia/nemotron-3.5-lightning-30b-a3b",

            // NVIDIA Riva (translation-specific)
            "nvidia/riva-translate-4b-instruct-v2",

            // Mistral
            "mistralai/mistral-large-4-1t-a52b",

            // Moonshot Kimi
            "moonshotai/kimi-k3.1-pro",

            // Zhipu GLM
            "z-ai/glm-5.3-flash",

            // MiniMax
            "minimaxai/minimax-m3.1-flash",

            // StepFun
            "stepfun-ai/step-5-preview",

            // Google Gemma
            "google/gemma-4-31b-it",
            "google/gemma-3-12b-it",
        };

        /// <summary>
        /// Models NVIDIA has taken down from build.nvidia.com (the API answers "410 Gone").
        /// A saved setting pointing at one of these is moved to the default model.
        /// </summary>
        private static readonly HashSet<string> RetiredModels = new(StringComparer.OrdinalIgnoreCase)
        {
            "meta/llama-4-maverick-17b-128e-instruct",
            "meta/llama-3.3-70b-instruct",
            "nvidia/nemotron-3-super-120b-a12b",
            "nvidia/llama-3.3-nemotron-super-49b-v1.5",
            "nvidia/llama-3.1-nemotron-ultra-253b-v1",
            "nvidia/llama-3.1-nemotron-70b-instruct",
            "nvidia/nemotron-nano-3-30b-a3b",
            "nvidia/nvidia-nemotron-nano-9b-v2",
            "nvidia/riva-translate-4b-instruct-v1.1",
            "nvidia/riva-translate-4b-instruct",
            "deepseek-ai/deepseek-v4.1-flash",
            "deepseek-ai/deepseek-v4-pro",
            "deepseek-ai/deepseek-v4-flash",
            "mistralai/mistral-large-3-675b-instruct-2512",
            "mistralai/mistral-medium-3.5-128b",
            "mistralai/mistral-small-4-119b-2603",
            "mistralai/mistral-nemotron",
            "qwen/qwen3.5-397b-a17b",
            "qwen/qwen3.5-122b-a10b",
            "qwen/qwen3-next-80b-a3b-instruct",
            "moonshotai/kimi-k2.6",
            "z-ai/glm-5.3",
            "z-ai/glm-5.2",
            "openai/gpt-oss-120b",
            "openai/gpt-oss-20b",
            "microsoft/phi-4-mini-instruct",
            "microsoft/phi-3.5-moe-instruct",
        };

        /// <summary>
        /// Endpoint used when the url in settings is only the service base - see <see cref="AutoTranslateUrl"/>.
        /// </summary>
        public const string DefaultUrl = "https://integrate.api.nvidia.com/v1/chat/completions";

        public void Initialize()
        {
            _httpClient?.Dispose();
            _httpClient = HttpClientFactoryWithProxy.CreateHttpClientWithProxy();
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "application/json");
            _httpClient.BaseAddress = new Uri(AutoTranslateUrl.Complete(Configuration.Settings.Tools.NvidiaUrl, DefaultUrl));
            _httpClient.Timeout = TimeSpan.FromMinutes(15);

            if (!string.IsNullOrEmpty(Configuration.Settings.Tools.NvidiaApiKey))
            {
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + Configuration.Settings.Tools.NvidiaApiKey);
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

        /// <summary>
        /// Retired ids are only swapped on NVIDIA's own cloud - a self-hosted NIM may still serve them.
        /// </summary>
        private static bool IsNvidiaCloud()
        {
            var url = Configuration.Settings.Tools.NvidiaUrl;
            return string.IsNullOrWhiteSpace(url) || url.Contains("integrate.api.nvidia.com", StringComparison.OrdinalIgnoreCase);
        }

        public async Task<string> Translate(string text, string sourceLanguageCode, string targetLanguageCode, CancellationToken cancellationToken)
        {
            var model = Configuration.Settings.Tools.NvidiaModel;
            if (string.IsNullOrEmpty(model) || (RetiredModels.Contains(model) && IsNvidiaCloud()))
            {
                model = Models[0];
                Configuration.Settings.Tools.NvidiaModel = model;
            }

            if (string.IsNullOrEmpty(Configuration.Settings.Tools.NvidiaPrompt))
            {
                Configuration.Settings.Tools.NvidiaPrompt = new ToolsSettings().NvidiaPrompt;
            }
            var prompt = string.Format(Configuration.Settings.Tools.NvidiaPrompt, sourceLanguageCode, targetLanguageCode);
            var input = "{\"model\": \"" + Json.EncodeJsonText(model) + "\",\"messages\": [{ \"role\": \"user\", \"content\": \"" + Json.EncodeJsonText(prompt) + "\\n\\n" + Json.EncodeJsonText(text.Trim()) + "\" }]}";

            int[] retryDelays = { 2555, 5007, 9013 };
            HttpResponseMessage result = null!;
            var resultContent = string.Empty;
            for (var attempt = 0; attempt <= retryDelays.Length; attempt++)
            {
                var content = new StringContent(input, Encoding.UTF8);
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                result = await _httpClient.PostAsync(string.Empty, content, cancellationToken);
                resultContent = await result.Content.ReadAsStringAsync(cancellationToken);

                if (!DeepLTranslate.ShouldRetry(result, resultContent) || attempt == retryDelays.Length)
                {
                    break;
                }

                await Task.Delay(retryDelays[attempt], cancellationToken);
            }

            if (!result.IsSuccessStatusCode)
            {
                Error = resultContent;
                SeLogger.Error("NVIDIA Translate failed calling API: Status code=" + result.StatusCode + Environment.NewLine + resultContent);
            }

            result.EnsureSuccessStatusCode();

            var parser = new SeJsonParser();
            var resultText = parser.GetFirstObject(resultContent, "content");
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
