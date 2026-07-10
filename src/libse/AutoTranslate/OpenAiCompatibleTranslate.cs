using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Settings;
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
    /// <summary>
    /// Generic engine for any service exposing an OpenAI-compatible "chat/completions"
    /// endpoint (vLLM, Xiaomi MIMO, Together, etc.) - URL, API key, and model are all
    /// user-configured, so providers SE has no dedicated engine for still work (#12324).
    /// </summary>
    public class OpenAiCompatibleTranslate : IAutoTranslator, IDisposable
    {
        private HttpClient _httpClient;

        public static string StaticName { get; set; } = "OpenAI Compatible API";
        public override string ToString() => StaticName;
        public string Name => StaticName;
        public string Url => "https://platform.openai.com/docs/api-reference/chat";
        public string Error { get; set; }
        public int MaxCharacters => 1500;

        public void Initialize()
        {
            _httpClient?.Dispose();
            _httpClient = HttpClientFactoryWithProxy.CreateHttpClientWithProxy();
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "application/json");
            _httpClient.BaseAddress = new Uri(Configuration.Settings.Tools.OpenAiCompatibleTranslateUrl.TrimEnd('/'));
            _httpClient.Timeout = TimeSpan.FromMinutes(15);

            if (!string.IsNullOrEmpty(Configuration.Settings.Tools.OpenAiCompatibleTranslateApiKey))
            {
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + Configuration.Settings.Tools.OpenAiCompatibleTranslateApiKey);
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
            // Model stays optional: single-model servers (e.g. llama.cpp, vLLM with one
            // model loaded) ignore it, while hosted providers require it.
            var model = Configuration.Settings.Tools.OpenAiCompatibleTranslateModel;
            var modelJson = string.Empty;
            if (!string.IsNullOrEmpty(model))
            {
                modelJson = "\"model\": \"" + Json.EncodeJsonText(model) + "\",";
            }

            if (string.IsNullOrWhiteSpace(Configuration.Settings.Tools.OpenAiCompatibleTranslatePrompt))
            {
                Configuration.Settings.Tools.OpenAiCompatibleTranslatePrompt = new ToolsSettings().OpenAiCompatibleTranslatePrompt;
            }
            var prompt = string.Format(Configuration.Settings.Tools.OpenAiCompatibleTranslatePrompt, sourceLanguageCode, targetLanguageCode);
            var input = "{" + modelJson + "\"messages\": [{ \"role\": \"user\", \"content\": \"" + Json.EncodeJsonText(prompt) + "\\n\\n" + Json.EncodeJsonText(text.Trim()) + "\" }]}";
            var content = new StringContent(input, Encoding.UTF8);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            var result = await _httpClient.PostAsync(string.Empty, content, cancellationToken);
            var bytes = await result.Content.ReadAsByteArrayAsync();
            var json = Encoding.UTF8.GetString(bytes).Trim();
            if (!result.IsSuccessStatusCode)
            {
                Error = json;
                SeLogger.Error("Error calling " + StaticName + ": Status code=" + result.StatusCode + Environment.NewLine + json);
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
