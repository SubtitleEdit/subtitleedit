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
    public class GroqTranslate : IAutoTranslator, IAutoTranslatorWithContext, IDisposable
    {
        private HttpClient _httpClient;

        public static string StaticName { get; set; } = "Groq";
        public override string ToString() => StaticName;
        public string Name => StaticName;
        public string Url => "https://groq.com/";
        public string Error { get; set; }
        public int MaxCharacters => 1500;

        /// <summary>
        /// See https://console.groq.com/docs/models
        /// </summary>
        public static string[] Models => new[]
        {
            // High-Performance / Frontier Multilingual
            "llama-3.3-70b-versatile",                      // Proven high-reliability translator (Production)
            "openai/gpt-oss-120b",                          // Extreme reasoning & nuance (Production)

            // Medium / Efficient Multilingual
            "openai/gpt-oss-20b",                           // High-speed reasoning model (Production)
            "qwen/qwen3-32b",                               // Exceptional for Asian & European languages (Preview)
            "meta-llama/llama-4-scout-17b-16e-instruct",    // Massive context for long documents (Preview)

            // Lightweight / Fast Translation
            "llama-3.1-8b-instant",                         // Lowest latency for short strings (Production)

            // Agentic systems
            "groq/compound",                                // Groq compound system
            "groq/compound-mini",                           // Smaller compound system
        };

        public void Initialize()
        {
            _httpClient?.Dispose();
            _httpClient = HttpClientFactoryWithProxy.CreateHttpClientWithProxy();
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "application/json");
            _httpClient.BaseAddress = new Uri(Configuration.Settings.Tools.GroqUrl.TrimEnd('/'));
            _httpClient.Timeout = TimeSpan.FromMinutes(15);

            if (!string.IsNullOrEmpty(Configuration.Settings.Tools.GroqApiKey))
            {
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + Configuration.Settings.Tools.GroqApiKey);
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

        //curl -X POST "https://api.groq.com/openai/v1/chat/completions" \
        //-H "Authorization: Bearer $GROQ_API_KEY" \
        //-H "Content-Type: application/json" \
        //-d '{"messages": [{"role": "user", "content": "Explain the importance of fast language models"}], "model": "llama3-8b-8192"}

        public async Task<string> Translate(string text, string sourceLanguageCode, string targetLanguageCode, CancellationToken cancellationToken)
        {
            return await Translate(text, sourceLanguageCode, targetLanguageCode, string.Empty, string.Empty, cancellationToken);
        }

        public async Task<string> Translate(string text, string sourceLanguageCode, string targetLanguageCode, string previousLineText, string nextLineText, CancellationToken cancellationToken)
        {
            var model = Configuration.Settings.Tools.GroqModel;
            if (string.IsNullOrEmpty(model))
            {
                model = Models[0];
                Configuration.Settings.Tools.GroqModel = model;
            }

            if (string.IsNullOrEmpty(Configuration.Settings.Tools.GroqPrompt))
            {
                Configuration.Settings.Tools.GroqPrompt = new ToolsSettings().GroqPrompt;
            }
            var prompt = string.Format(Configuration.Settings.Tools.GroqPrompt, sourceLanguageCode, targetLanguageCode, previousLineText, nextLineText);
            var input = "{\"model\": \"" + model + "\",\"messages\": [{ \"role\": \"user\", \"content\": \"" + Json.EncodeJsonText(prompt) + "\\n\\n" + Json.EncodeJsonText(text.Trim()) + "\" }]}";
            var content = new StringContent(input, Encoding.UTF8);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            var result = await _httpClient.PostAsync(string.Empty, content, cancellationToken);
            var bytes = await result.Content.ReadAsByteArrayAsync();
            var json = Encoding.UTF8.GetString(bytes).Trim();
            if (!result.IsSuccessStatusCode)
            {
                Error = json;
                SeLogger.Error("Groq Translate failed calling API: Status code=" + result.StatusCode + Environment.NewLine + json);
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
