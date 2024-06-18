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
    public class AnthropicTranslate : IAutoTranslator
    {
        private HttpClient _httpClient;

        public static string StaticName { get; set; } = "Anthropic Claude";
        public string Name => StaticName;
        public string Url => "https://www.anthropic.com/";
        public string Error { get; set; }
        public int MaxCharacters => 900;

        public void Initialize()
        {
            _httpClient?.Dispose();
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "application/json");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("anthropic-version", "2023-06-01");
            _httpClient.BaseAddress = new Uri(Configuration.Settings.Tools.AnthropicApiUrl.TrimEnd('/'));

            if (!string.IsNullOrEmpty(Configuration.Settings.Tools.AnthropicApiKey))
            {
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("x-api-key", Configuration.Settings.Tools.AnthropicApiKey);
            }
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
            var model = Configuration.Settings.Tools.AnthropicApiModel;
            if (string.IsNullOrEmpty(model))
            {
                model = "claude-3-opus-20240229";
                Configuration.Settings.Tools.AnthropicApiModel = model;
            }

            if (string.IsNullOrEmpty(Configuration.Settings.Tools.AnthropicPrompt))
            {
                Configuration.Settings.Tools.AnthropicPrompt = new ToolsSettings().AnthropicPrompt;
            }
            var prompt = string.Format(Configuration.Settings.Tools.AnthropicPrompt, sourceLanguageCode, targetLanguageCode);
            var input = "{ \"model\": \"" + model + "\", \"max_tokens\": 1024, \"messages\": [{ \"role\": \"user\", \"content\": \"" + prompt + "\\n\\n" + Json.EncodeJsonText(text.Trim()) + "\" }]}";
            var content = new StringContent(input, Encoding.UTF8);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            var result = await _httpClient.PostAsync(string.Empty, content, cancellationToken);
            var bytes = await result.Content.ReadAsByteArrayAsync();
            var json = Encoding.UTF8.GetString(bytes).Trim();
            if (!result.IsSuccessStatusCode)
            {
                Error = json;
                SeLogger.Error("Anthropic Translate failed calling API: Status code=" + result.StatusCode + Environment.NewLine + json);
            }

            result.EnsureSuccessStatusCode();

            SeLogger.Error($"{StaticName}: debug json: " + json);

            var parser = new SeJsonParser();
            var resultText = parser.GetFirstObject(json, "text");
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
