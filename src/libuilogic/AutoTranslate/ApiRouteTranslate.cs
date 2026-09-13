using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Settings;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.UiLogic.Translate;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.UiLogic.Http;

namespace Nikse.SubtitleEdit.UiLogic.AutoTranslate
{
    public class ApiRouteTranslate : IAutoTranslator, IDisposable
    {
        private HttpClient _httpClient = null!;

        public static string StaticName { get; set; } = "API-Route";
        public override string ToString() => StaticName;
        public string Name => StaticName;
        public string Url => "https://www.api-route.com";
        public string Error { get; set; } = string.Empty;
        public int MaxCharacters => 1500;

        /// <summary>
        /// See https://www.api-route.com
        /// </summary>
        public static string[] Models => new[]
        {
            // Anthropic Claude
            "claude-sonnet-4-5",
            "claude-haiku-4-5",
            "claude-opus-4-1",

            // OpenAI GPT Series
            "gpt-4o",
            "gpt-4o-mini",
            "gpt-4.1",
            "gpt-4.1-mini",
            "o3-mini",

            // DeepSeek
            "deepseek-chat",
            "deepseek-reasoner",

            // Google Gemini
            "gemini-2.5-pro",
            "gemini-2.5-flash",

            // Qwen
            "qwen-2.5-72b-instruct",
        };

        /// <summary>
        /// Endpoint used when the url in settings is only the service base - see <see cref="AutoTranslateUrl"/>.
        /// </summary>
        public const string DefaultUrl = "https://global.api-route.com/v1/chat/completions";

        public void Initialize()
        {
            _httpClient?.Dispose();
            _httpClient = HttpClientFactoryWithProxy.CreateHttpClientWithProxy();
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "application/json");
            _httpClient.BaseAddress = new Uri(AutoTranslateUrl.Complete(Configuration.Settings.Tools.ApiRouteUrl, DefaultUrl));
            _httpClient.Timeout = TimeSpan.FromMinutes(15);

            if (!string.IsNullOrEmpty(Configuration.Settings.Tools.ApiRouteApiKey))
            {
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + Configuration.Settings.Tools.ApiRouteApiKey);
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
            var model = Configuration.Settings.Tools.ApiRouteModel;
            if (string.IsNullOrEmpty(model))
            {
                model = Models[0];
                Configuration.Settings.Tools.ApiRouteModel = model;
            }

            if (string.IsNullOrEmpty(Configuration.Settings.Tools.ApiRoutePrompt))
            {
                Configuration.Settings.Tools.ApiRoutePrompt = new ToolsSettings().ApiRoutePrompt;
            }
            var prompt = string.Format(Configuration.Settings.Tools.ApiRoutePrompt, sourceLanguageCode, targetLanguageCode);
            var input = "{\"model\": \"" + model + "\",\"messages\": [{ \"role\": \"user\", \"content\": \"" + Json.EncodeJsonText(prompt) + "\\n\\n" + Json.EncodeJsonText(text.Trim()) + "\" }]}";

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
                SeLogger.Error("API-Route Translate failed calling API: Status code=" + result.StatusCode + Environment.NewLine + json);
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
