using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Settings;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Core.Translate;

namespace Nikse.SubtitleEdit.Core.AutoTranslate
{
    /// <summary>
    /// Mistral AI translator - see https://mistral.ai/en
    /// </summary>
    public class MistralTranslate : IAutoTranslator, IDisposable
    {
        private string _apiKey;
        private string _apiUrl;
        private HttpClient _httpClient;

        public static string StaticName { get; set; } = "Mistral AI Translate";
        public override string ToString() => StaticName;
        public string Name => StaticName;
        public string Url => "https://mistral.ai";
        public string Error { get; set; }
        public int MaxCharacters => 1500;

        /// <summary>
        /// See https://docs.mistral.ai/getting-started/models/models_overview/
        /// </summary>
        public static string[] Models => new[]
        {
            "mistral-small-latest",
            "mistral-medium-latest",
            "open-mistral-nemo-latest",
        };

        public void Initialize()
        {
            _apiKey = Configuration.Settings.Tools.AutoTranslateMistralApiKey;
            _apiUrl = Configuration.Settings.Tools.AutoTranslateMistralUrl;

            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_apiUrl))
            {
                return;
            }

            _httpClient = HttpClientFactoryWithProxy.CreateHttpClientWithProxy();
            _httpClient.BaseAddress = new Uri(_apiUrl.Trim().TrimEnd('/'));
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + _apiKey.Trim());
        }

        public List<TranslationPair> GetSupportedSourceLanguages()
        {
            return ChatGptTranslate.ListLanguages();
        }

        public List<TranslationPair> GetSupportedTargetLanguages()
        {
            return ChatGptTranslate.ListLanguages();
        }

        public async Task<string> Translate(string text, string sourceLanguageCode, string targetLanguageCode, CancellationToken cancellationToken)
        {
            var model = Configuration.Settings.Tools.AutoTranslateMistralModel;
            if (string.IsNullOrEmpty(model))
            {
                model = Models[0];
                Configuration.Settings.Tools.AutoTranslateMistralModel = model;
            }

            if (string.IsNullOrEmpty(Configuration.Settings.Tools.AutoTranslateMistralPrompt))
            {
                Configuration.Settings.Tools.AutoTranslateMistralPrompt = new ToolsSettings().AutoTranslateMistralPrompt;
            }
            var prompt = string.Format(Configuration.Settings.Tools.AutoTranslateMistralPrompt, sourceLanguageCode, targetLanguageCode);
            var input = "{\"model\": \"" + model + "\",\"messages\": [{ \"role\": \"user\", \"content\": \"" + Json.EncodeJsonText(prompt) + "\\n\\n" + Json.EncodeJsonText(text.Trim()) + "\" }]}";
            var content = new StringContent(input, Encoding.UTF8);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            var result = await _httpClient.PostAsync(string.Empty, content, cancellationToken);
            var bytes = await result.Content.ReadAsByteArrayAsync();
            var json = Encoding.UTF8.GetString(bytes).Trim();
            if (!result.IsSuccessStatusCode)
            {
                Error = json;
                SeLogger.Error("MistralTranslate failed calling API: Status code=" + result.StatusCode + Environment.NewLine + json);
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

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
