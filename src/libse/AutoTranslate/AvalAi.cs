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
    public class AvalAi : IAutoTranslator, IDisposable
    {
        private HttpClient _httpClient;

        public static string StaticName { get; set; } = "AvalAI";
        public override string ToString() => StaticName;
        public string Name => StaticName;
        public string Url => "https://avalai.ir";
        public string Error { get; set; }
        public int MaxCharacters => 1500;

        /// <summary>
        /// See https://avalai.ir
        /// </summary>
        public static string[] Models => new[]
        {
            "gpt-4o-mini",
            "gpt-4o",
            "o1-mini",
            "o3-mini",
            "gemini-2.0-pro-exp-02-05",
            "gemini-2.0-flash",
            "gemini-2.0-flash-lite-preview-02-05",
            "gemini-2.0-flash-thinking-exp-01-21",
            "gemini-2.0-flash-thinking-exp",
            "gemini-1.5-flash",
            "gemini-1.5-pro",
            "cohere.command-light-text-v14",
            "cohere.command-r-v1:0",
            "deepseek-reasoner",
            "deepseek-chat",
            "anthropic.claude-3-5-sonnet-20240620-v1:0",
            "anthropic.claude-3-5-haiku-20241022-v1:0",
            "meta.llama3-1-70b-instruct-v1:0",
            "meta.llama3-1-405b-instruct-v1:0",
            "meta.llama3-3-70b-instruct-v1:0",
            "mistral.mistral-large-2407-v1:0"
        };

        public void Initialize()
        {
            _httpClient?.Dispose();
            _httpClient = HttpClientFactoryWithProxy.CreateHttpClientWithProxy();
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "application/json");
            _httpClient.BaseAddress = new Uri(Configuration.Settings.Tools.AvalAiUrl.TrimEnd('/'));
            _httpClient.Timeout = TimeSpan.FromMinutes(15);

            if (!string.IsNullOrEmpty(Configuration.Settings.Tools.AvalAiApiKey))
            {
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + Configuration.Settings.Tools.AvalAiApiKey);
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
            var model = Configuration.Settings.Tools.AvalAiModel;
            if (string.IsNullOrEmpty(model))
            {
                model = Models[0];
                Configuration.Settings.Tools.AvalAiModel = model;
            }

            if (string.IsNullOrEmpty(Configuration.Settings.Tools.AvalAiPrompt))
            {
                Configuration.Settings.Tools.AvalAiPrompt = new ToolsSettings().AvalAiPrompt;
            }
            var prompt = string.Format(Configuration.Settings.Tools.AvalAiPrompt, sourceLanguageCode, targetLanguageCode);
            var input = "";
            var input2 = "{\"model\": \"" + model + "\",\"messages\": [{ \"role\": \"user\", \"content\": \"" + prompt + "\\n\\n" + Json.EncodeJsonText(text.Trim()) + "\" }]}";
            var modelJson = "\"" + model + "\"";
            var promptJson = "\"" + prompt.Replace("\"", "\\\"") + "\\n\\n" + Json.EncodeJsonText(text.Trim()).Replace("\"", "\\\"") + "\"";

            var input3 = "{" +
                "\"model\": " + modelJson + "," +
                "\"prompt\": " + promptJson +
            "}";
            if (model.Contains("deep11111111111"))
            {
                input = input3;
                //_httpClient.BaseAddress = new Uri("https://api.avalai.ir/v1/completions/");
            }
            else
            {
                //_httpClient.BaseAddress =  "https://api.avalai.ir/v1/chat/completions/";
                //_httpClient.BaseAddress = new Uri("https://api.avalai.ir/v1/chat/completions/");
                input = input2;
            }
            var content = new StringContent(input, Encoding.UTF8);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            var result = await _httpClient.PostAsync(string.Empty, content, cancellationToken);
            var bytes = await result.Content.ReadAsByteArrayAsync();
            var json = Encoding.UTF8.GetString(bytes).Trim();
            if (!result.IsSuccessStatusCode)
            {
                Error = json;
                SeLogger.Error("AvalAi Translate failed calling API: Status code=" + result.StatusCode + Environment.NewLine + json);
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
