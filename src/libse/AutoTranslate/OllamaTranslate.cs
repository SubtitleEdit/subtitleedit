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
    public class OllamaTranslate : IAutoTranslator, IDisposable
    {
        private HttpClient _httpClient;

        public static string StaticName { get; set; } = "Ollama (local LLM)";
        public override string ToString() => StaticName;
        public string Name => StaticName;
        public string Url => "https://github.com/ollama/ollama";
        public string Error { get; set; }
        public int MaxCharacters => 1000;

        public void Initialize()
        {
            _httpClient?.Dispose();
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "application/json");
            _httpClient.BaseAddress = new Uri(Configuration.Settings.Tools.OllamaApiUrl.TrimEnd('/'));
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
            var model = Configuration.Settings.Tools.OllamaModel;
            var modelJson = string.Empty;
            if (!string.IsNullOrEmpty(model))
            {
                modelJson = "\"model\": \"" + model + "\",";
                Configuration.Settings.Tools.OllamaModel = model;
            }

            if (string.IsNullOrWhiteSpace(Configuration.Settings.Tools.OllamaPrompt))
            {
                Configuration.Settings.Tools.OllamaPrompt = new ToolsSettings().OllamaPrompt;
            }
            var prompt = string.Format(Configuration.Settings.Tools.OllamaPrompt, sourceLanguageCode, targetLanguageCode);
            var input = "{ " + modelJson + " \"prompt\": \"" + Json.EncodeJsonText(prompt) + "\\n\\n" + Json.EncodeJsonText(text.Trim()) + "\", \"stream\": false }";
            if (Configuration.Settings.Tools.OllamaApiUrl.TrimEnd('/').EndsWith("v1/chat/completions"))
            {
                input = "{ " + modelJson + " \"messages\": [{ \"role\": \"user\", \"content\": \"" + Json.EncodeJsonText(prompt) + "\\n\\n" + Json.EncodeJsonText(text.Trim()) + "\" }]}";
            }
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
            if (string.IsNullOrEmpty(resultText))
            {
                resultText = parser.GetFirstObject(json, "content");
            }

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
