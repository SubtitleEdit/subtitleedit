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
            "google/gemini-flash-1.5-8b",
            "google/gemini-2.0-flash-001",
            "deepseek/deepseek-r1",
            "deepseek/deepseek-r1:free",
            "microsoft/phi-4",
            "mistralai/mistral-nemo",
            "meta-llama/llama-3.3-70b-instruct",
            "openai/gpt-4o-mini",
            "anthropic/claude-3.5-sonnet",
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
            var input = "{\"model\": \"" + model + "\",\"messages\": [{ \"role\": \"user\", \"content\": \"" + prompt + "\\n\\n" + Json.EncodeJsonText(text.Trim()) + "\" }]}";
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
