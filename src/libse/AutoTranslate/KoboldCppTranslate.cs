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
    public class KoboldCppTranslate : IAutoTranslator, IDisposable
    {
        private HttpClient _httpClient;

        public static string StaticName { get; set; } = "KoboldCpp (local LLM)";
        public override string ToString() => StaticName;
        public string Name => StaticName;
        public string Url => "https://github.com/LostRuins/koboldcpp/releases";
        public string Error { get; set; }
        public int MaxCharacters => 1000;

        public void Initialize()
        {
            _httpClient?.Dispose();
            _httpClient = HttpClientFactoryWithProxy.CreateHttpClientWithProxy();
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "application/json");
            _httpClient.BaseAddress = new Uri(Configuration.Settings.Tools.KoboldCppUrl.TrimEnd('/'));
            _httpClient.Timeout = TimeSpan.FromMinutes(25);
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
            //            var model = Configuration.Settings.Tools.KoboldCppModel;
            var modelJson = string.Empty;
            //if (!string.IsNullOrEmpty(model))
            //{
            //    modelJson = "\"model\": \"" + model + "\",";
            //    Configuration.Settings.Tools.OllamaModel = model;
            //}

            var temperatureJson = string.Empty;
            if (Configuration.Settings.Tools.KoboldCppTemperature >= 0 && Configuration.Settings.Tools.KoboldCppTemperature < 2)
            {
                temperatureJson = "\"temperature\": " + Configuration.Settings.Tools.KoboldCppTemperature.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + ",";
            }

            if (string.IsNullOrWhiteSpace(Configuration.Settings.Tools.KoboldCppPrompt))
            {
                Configuration.Settings.Tools.KoboldCppPrompt = new ToolsSettings().KoboldCppPrompt;
            }
            var prompt = string.Format(Configuration.Settings.Tools.KoboldCppPrompt, sourceLanguageCode, targetLanguageCode);
            var input = "{ " + modelJson + temperatureJson + " \"prompt\": \"" + prompt + "\\n\\n" + Json.EncodeJsonText(text.Trim()) + "\", \"stream\": false }";
            var content = new StringContent(input, Encoding.UTF8);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            var result = await _httpClient.PostAsync(string.Empty, content, cancellationToken).ConfigureAwait(false);
            var bytes = await result.Content.ReadAsByteArrayAsync();
            var json = Encoding.UTF8.GetString(bytes).Trim();
            if (!result.IsSuccessStatusCode)
            {
                Error = json;
                SeLogger.Error("Error calling + " + StaticName + ": Status code=" + result.StatusCode + Environment.NewLine + json);
            }

            result.EnsureSuccessStatusCode();

            var parser = new SeJsonParser();
            var resultText = parser.GetFirstObject(json, "response");
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
