using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Settings;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.UiLogic.Translate;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.UiLogic.Http;

namespace Nikse.SubtitleEdit.UiLogic.AutoTranslate
{
    public class GeminiTranslate : IAutoTranslator, IDisposable
    {
        public static string StaticName { get; set; } = "Google Gemini";
        public override string ToString() => StaticName;
        public string Name => StaticName;
        public string Url => "https://deepmind.google/technologies/gemini/";
        public string Error { get; set; } = string.Empty;
        public int MaxCharacters => 1500;

        private HttpClient _httpClient = null!;
        private string _baseUrl = string.Empty;


        /// <summary>
        /// See https://ai.google.dev/gemini-api/docs/models and https://ai.google.dev/gemma/docs/core/gemma_on_gemini_api
        /// </summary>
        public static string[] Models => new[]
        {
            // Auto-updating alias
            "gemini-flash-latest",
            "gemini-flash-lite-latest",

            // Gemini 3.x - Latest Generation
            "gemini-3.6-flash",
            "gemini-3.5-flash",
            "gemini-3.5-flash-lite",
            "gemini-3.1-pro-preview",
            "gemini-3.1-flash-lite",
            "gemini-3-flash-preview",

            // Gemini 2.5 - Stable
            "gemini-2.5-pro",
            "gemini-2.5-flash",
            "gemini-2.5-flash-lite",

            // Gemma - open models served via the Gemini API
            "gemma-4-31b-it",
            "gemma-4-26b-a4b-it",
        };

        public void Initialize()
        {
            _httpClient?.Dispose();
            _httpClient = HttpClientFactoryWithProxy.CreateHttpClientWithProxy();
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "application/json");

            if (!string.IsNullOrEmpty(Configuration.Settings.Tools.GeminiProApiKey))
            {
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("x-goog-api-key", Configuration.Settings.Tools.GeminiProApiKey);
            }

            if (string.IsNullOrEmpty(Configuration.Settings.Tools.GeminiModel))
            {
                Configuration.Settings.Tools.GeminiModel = Models[0];
            }

            if (string.IsNullOrEmpty(Configuration.Settings.Tools.GeminiPrompt))
            {
                Configuration.Settings.Tools.GeminiPrompt = new ToolsSettings().GeminiPrompt;
            }

            _baseUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{Configuration.Settings.Tools.GeminiModel}:generateContent";
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
            int[] retryDelays = { 555, 3007, 7013 };
            HttpResponseMessage result = null!;
            var resultContent = string.Empty;
            var switchedBaseUrl = false;
            for (var attempt = 0; attempt <= retryDelays.Length; attempt++)
            {
                var content = MakeContent(text, sourceLanguageCode, targetLanguageCode);
                result = await _httpClient.PostAsync(_baseUrl, content, cancellationToken);

                if (result.StatusCode == System.Net.HttpStatusCode.NotFound && !switchedBaseUrl)
                {
                    if (_baseUrl.Contains("v1beta"))
                    {
                        _baseUrl = $"https://generativelanguage.googleapis.com/v1/models/{Configuration.Settings.Tools.GeminiModel}:generateContent";
                        switchedBaseUrl = true;
                    }
                    else
                    {
                        _baseUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{Configuration.Settings.Tools.GeminiModel}:generateContent";
                        switchedBaseUrl = true;
                    }

                    continue;
                }

                resultContent = await result.Content.ReadAsStringAsync(cancellationToken);

                if (!DeepLTranslate.ShouldRetry(result, resultContent) || attempt == retryDelays.Length)
                {
                    break;
                }

                await Task.Delay(retryDelays[attempt], cancellationToken);
            }

            if (!result.IsSuccessStatusCode)
            {
                Error = resultContent;
                SeLogger.Error($"GeminiTranslate failed calling API at {_baseUrl}: Status code={result.StatusCode}{Environment.NewLine}{resultContent}");
            }

            result.EnsureSuccessStatusCode();

            var parser = new SeJsonParser();
            var resultText = parser.GetFirstObject(resultContent, "text");
            if (resultText == null)
            {
                // A 200 without any text part - a blocked prompt, a stopped candidate or a model
                // that spent its output on reasoning. Returning an empty string made the translate
                // loop retry the line several times, each as slow as the first, with nothing on
                // screen but a disabled Translate button (#14926). Say why right away instead.
                Error = resultContent;
                SeLogger.Error($"GeminiTranslate got no text from {_baseUrl}: {resultContent}");
                throw new Exception(MakeNoTextMessage(resultContent));
            }

            var outputText = Json.DecodeJsonText(resultText).Trim();
            if (outputText.StartsWith('"') && outputText.EndsWith('"') && !text.StartsWith('"'))
            {
                outputText = outputText.Trim('"').Trim();
            }

            if (!string.IsNullOrEmpty(outputText))
            {
                outputText = ChatGptTranslate.FixNewLines(outputText);
            }

            return outputText;
        }

        /// <summary>
        /// The error shown when a Gemini reply carries no translated text, naming the block or
        /// finish reason when the reply has one.
        /// </summary>
        public static string MakeNoTextMessage(string resultContent)
        {
            var parser = new SeJsonParser();
            var blockReason = parser.GetFirstObject(resultContent ?? string.Empty, "blockReason");
            var finishReason = parser.GetFirstObject(resultContent ?? string.Empty, "finishReason");

            var message = $"{StaticName} returned no translated text";
            if (!string.IsNullOrEmpty(blockReason))
            {
                message += $" (prompt blocked: {blockReason})";
            }
            else if (!string.IsNullOrEmpty(finishReason))
            {
                message += $" (finish reason: {finishReason})";
            }

            return message + "." + Environment.NewLine + Environment.NewLine +
                   "If this keeps happening, try another Gemini model - for example gemini-flash-lite-latest.";
        }

        private HttpContent MakeContent(string text, string sourceLanguageCode, string targetLanguageCode)
        {
            var prompt = string.Format(Configuration.Settings.Tools.GeminiPrompt, sourceLanguageCode, targetLanguageCode);
            var input = "{ \"contents\": [ { \"role\": \"user\", \"parts\": [{ \"text\": \"" + Json.EncodeJsonText(prompt) + "\\n\\n" + Json.EncodeJsonText(text.Trim()) + "\" }]}]}";
            var content = new StringContent(input, Encoding.UTF8);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            return content;
        }

        private static List<TranslationPair> ListLanguages()
        {
            return ChatGptTranslate.ListLanguages();
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
