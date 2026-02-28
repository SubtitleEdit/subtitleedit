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
            // OpenAI GPT Series
            "gpt-5.2",
            "gpt-5.1",
            "gpt-5.1-codex-max",
            "gpt-5-mini",
            "gpt-5-nano",
            "gpt-4.1",
            "gpt-4.1-mini",
            "gpt-4o",
            "gpt-4o-mini",
            "o4-mini",
            // OpenAI OSS releases
            "gpt-oss-120b",
            "gpt-oss-20b",

            // Google / Gemini
            "gemini-3-pro",
            "gemini-3-flash",
            "gemini-2.5-pro",
            "gemini-2.5-flash",
            "gemini-2.5-flash-lite",
            "gemini-2.0-flash",

            // Anthropic Claude
            "claude-opus-4.5",
            "claude-sonnet-4.5",
            "claude-haiku-4.5",
            "claude-4-5-opus",
            "claude-4-5-sonnet",

            // xAI / Grok
            "grok-4",
            "grok-3",
            "grok-3-mini",

            // DeepSeek
            "deepseek-r1",
            "deepseek-v3.1",
            "deepseek-v3.1-thinking",
            "deepseek-chat",

            // Other notable models
            "qwen-3-235b"
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
            var prompt = string.Format(Json.EncodeJsonText(Configuration.Settings.Tools.AvalAiPrompt), sourceLanguageCode, targetLanguageCode);
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
