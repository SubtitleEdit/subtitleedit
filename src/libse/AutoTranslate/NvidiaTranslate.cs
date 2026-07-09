using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Core.Translate;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.Core.Settings;

namespace Nikse.SubtitleEdit.Core.AutoTranslate
{
    public class NvidiaTranslate : IAutoTranslator, IAutoTranslatorWithContext, IDisposable
    {
        private HttpClient _httpClient;

        public static string StaticName { get; set; } = "NVIDIA";
        public override string ToString() => StaticName;
        public string Name => StaticName;
        public string Url => "https://build.nvidia.com/models";
        public string Error { get; set; }
        public int MaxCharacters => 1500;

        /// <summary>
        /// See https://build.nvidia.com/models
        /// </summary>
        public static string[] Models => new[]
        {
            // Meta Llama
            "meta/llama-4-maverick-17b-128e-instruct",
            "meta/llama-3.3-70b-instruct",
            "meta/llama-3.1-70b-instruct",
            "meta/llama-3.1-8b-instruct",

            // NVIDIA Nemotron
            "nvidia/nemotron-3-super-120b-a12b",
            "nvidia/llama-3.3-nemotron-super-49b-v1.5",
            "nvidia/llama-3.1-nemotron-ultra-253b-v1",
            "nvidia/llama-3.1-nemotron-70b-instruct",
            "nvidia/nemotron-nano-3-30b-a3b",
            "nvidia/nvidia-nemotron-nano-9b-v2",

            // NVIDIA Riva (translation-specific)
            "nvidia/riva-translate-4b-instruct-v1.1",
            "nvidia/riva-translate-4b-instruct",

            // DeepSeek
            "deepseek-ai/deepseek-v4-pro",
            "deepseek-ai/deepseek-v4-flash",

            // Mistral
            "mistralai/mistral-large-3-675b-instruct-2512",
            "mistralai/mistral-medium-3.5-128b",
            "mistralai/mistral-medium-3-instruct",
            "mistralai/mistral-small-4-119b-2603",
            "mistralai/mixtral-8x22b-instruct-v0.1",
            "mistralai/mistral-nemotron",

            // Google Gemma
            "google/gemma-4-31b-it",
            "google/gemma-3-27b-it",
            "google/gemma-3-12b-it",

            // Qwen
            "qwen/qwen3.5-397b-a17b",
            "qwen/qwen3.5-122b-a10b",
            "qwen/qwen3-next-80b-a3b-instruct",

            // Moonshot Kimi
            "moonshotai/kimi-k2.6",
            "moonshotai/kimi-k2-thinking",

            // OpenAI (open weights)
            "openai/gpt-oss-120b",
            "openai/gpt-oss-20b",

            // Microsoft Phi
            "microsoft/phi-4-mini-instruct",
            "microsoft/phi-3.5-moe-instruct",
        };

        public void Initialize()
        {
            _httpClient?.Dispose();
            _httpClient = HttpClientFactoryWithProxy.CreateHttpClientWithProxy();
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "application/json");
            _httpClient.BaseAddress = new Uri(Configuration.Settings.Tools.NvidiaUrl.TrimEnd('/'));
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

        public async Task<string> Translate(string text, string sourceLanguageCode, string targetLanguageCode, CancellationToken cancellationToken)
        {
            return await Translate(text, sourceLanguageCode, targetLanguageCode, string.Empty, string.Empty, cancellationToken);
        }

        public async Task<string> Translate(string text, string sourceLanguageCode, string targetLanguageCode, string previousLineText, string nextLineText, CancellationToken cancellationToken)
        {
            var model = Configuration.Settings.Tools.NvidiaModel;
            if (string.IsNullOrEmpty(model))
            {
                model = Models[0];
                Configuration.Settings.Tools.NvidiaModel = model;
            }

            if (string.IsNullOrEmpty(Configuration.Settings.Tools.NvidiaPrompt))
            {
                Configuration.Settings.Tools.NvidiaPrompt = new ToolsSettings().NvidiaPrompt;
            }
            var prompt = string.Format(Configuration.Settings.Tools.NvidiaPrompt, sourceLanguageCode, targetLanguageCode, previousLineText, nextLineText);
            var input = "{\"model\": \"" + model + "\",\"messages\": [{ \"role\": \"user\", \"content\": \"" + Json.EncodeJsonText(prompt) + "\\n\\n" + Json.EncodeJsonText(text.Trim()) + "\" }]}";

            int[] retryDelays = { 2555, 5007, 9013 };
            HttpResponseMessage result = null;
            string resultContent = null;
            for (var attempt = 0; attempt <= retryDelays.Length; attempt++)
            {
                var content = new StringContent(input, Encoding.UTF8);
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                result = await _httpClient.PostAsync(string.Empty, content, cancellationToken);
                resultContent = await result.Content.ReadAsStringAsync();

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
