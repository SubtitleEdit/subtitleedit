using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Settings;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.UiLogic.Translate;
using System.Net.Http.Headers;
using System.Text;
using Nikse.SubtitleEdit.UiLogic.Http;

namespace Nikse.SubtitleEdit.UiLogic.AutoTranslate
{
    public class OpperTranslate : IAutoTranslator, IDisposable
    {
        private HttpClient _httpClient = null!;

        public static string StaticName { get; set; } = "Opper";
        public override string ToString() => StaticName;
        public string Name => StaticName;
        public string Url => "https://opper.ai/";
        public string Error { get; set; } = string.Empty;
        public int MaxCharacters => 1500;

        /// <summary>
        /// See https://opper.ai/models
        /// </summary>
        public static string[] Models => new[]
        {
            // Pooled models: Opper picks the route per request
            "claude-sonnet-4-6",
            "claude-opus-5",
            "gpt-5.5",
            "gpt-5.4-mini",
            "gemini-3.8-flash",
            "gemini-3.1-flash-lite",
            "deepseek-v4-pro",
            "kimi-k3",
            "glm-5.2",
            "qwen3-max",
            "mistral-large-2512",
            "claude-haiku-4-5",
        };

        /// <summary>
        /// Endpoint used when the url in settings is only the service base - see <see cref="AutoTranslateUrl"/>.
        /// </summary>
        public const string DefaultUrl = "https://api.opper.ai/v3/compat/chat/completions";

        public void Initialize()
        {
            _httpClient?.Dispose();
            _httpClient = HttpClientFactoryWithProxy.CreateHttpClientWithProxy();
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "application/json");
            _httpClient.BaseAddress = new Uri(AutoTranslateUrl.Complete(Configuration.Settings.Tools.OpperUrl, DefaultUrl));
            _httpClient.Timeout = TimeSpan.FromMinutes(15);

            if (!string.IsNullOrEmpty(Configuration.Settings.Tools.OpperApiKey))
            {
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + Configuration.Settings.Tools.OpperApiKey);
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
            var model = Configuration.Settings.Tools.OpperModel;
            if (string.IsNullOrEmpty(model))
            {
                model = Models[0];
                Configuration.Settings.Tools.OpperModel = model;
            }

            if (string.IsNullOrEmpty(Configuration.Settings.Tools.OpperPrompt))
            {
                Configuration.Settings.Tools.OpperPrompt = new ToolsSettings().OpperPrompt;
            }
            var prompt = string.Format(Configuration.Settings.Tools.OpperPrompt, sourceLanguageCode, targetLanguageCode);
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
                SeLogger.Error("Opper Translate failed calling API: Status code=" + result.StatusCode + Environment.NewLine + json);
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
