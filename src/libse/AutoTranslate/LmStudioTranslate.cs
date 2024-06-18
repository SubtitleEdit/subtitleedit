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

namespace Nikse.SubtitleEdit.Core.AutoTranslate
{
    public class LmStudioTranslate : IAutoTranslator
    {
        private HttpClient _httpClient;

        public static string StaticName { get; set; } = "LM Studio (local ChatGPT)";
        public string Name => StaticName;
        public string Url => "https://lmstudio.ai/";
        public string Error { get; set; }
        public int MaxCharacters => 1000;

        public void Initialize()
        {
            _httpClient?.Dispose();
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "application/json");
            _httpClient.BaseAddress = new Uri(Configuration.Settings.Tools.LmStudioApiUrl.TrimEnd('/'));
            _httpClient.Timeout = TimeSpan.FromMinutes(15);
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
            var model = Configuration.Settings.Tools.LmStudioModel;
            var modelJson = string.Empty;
            if (!string.IsNullOrEmpty(model))
            {
                modelJson = "\"model\": \"" + model + "\",";
                Configuration.Settings.Tools.LmStudioModel = model;
            }

            if (string.IsNullOrWhiteSpace(Configuration.Settings.Tools.LmStudioPrompt))
            {
                Configuration.Settings.Tools.OllamaPrompt = new ToolsSettings().LmStudioPrompt;
            }
            var prompt = string.Format(Configuration.Settings.Tools.LmStudioPrompt, sourceLanguageCode, targetLanguageCode);
            var input = "{ " + modelJson + " \"messages\": [{ \"role\": \"user\", \"content\": \"" + prompt + "\\n\\n" + Json.EncodeJsonText(text.Trim()) + "\" }]}";
            var content = new StringContent(input, Encoding.UTF8);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            var result = await _httpClient.PostAsync(string.Empty, content, cancellationToken);
            var bytes = await result.Content.ReadAsByteArrayAsync();
            var json = Encoding.UTF8.GetString(bytes).Trim();
            if (!result.IsSuccessStatusCode)
            {
                Error = json;
                SeLogger.Error("Error calling + " + StaticName + ": Status code=" + result.StatusCode + Environment.NewLine + json);
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

            outputText = outputText.Replace("<br />", Environment.NewLine);
            outputText = outputText.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
            outputText = ChatGptTranslate.RemovePreamble(text, outputText);
            return outputText.Trim();
        }
    }
}
