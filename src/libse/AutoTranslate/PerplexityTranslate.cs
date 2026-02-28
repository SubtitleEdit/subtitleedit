using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Core.Translate;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.Core.Settings;

namespace Nikse.SubtitleEdit.Core.AutoTranslate
{
    public class PerplexityTranslate : IAutoTranslator, IDisposable
    {
        private HttpClient _httpClient;

        public static string StaticName { get; set; } = "Perplexity";
        public override string ToString() => StaticName;
        public string Name => StaticName;
        public string Url => "https://www.paerplexity.ai/";
        public string Error { get; set; }
        public int MaxCharacters => 1500;

        /// <summary>
        /// See https://docs.perplexity.ai/docs/getting-started/models
        /// </summary>
        public static string[] Models => new[]
        {
            "perplexity/sonar",
            "google/gemini-2.5-flash",
            "google/gemini-3-flash-preview",
            "anthropic/claude-haiku-4-5",
            "google/gemini-2.5-pro",
            "openai/gpt-5.1",
            "openai/gpt-5.2",
            "google/gemini-3-pro-preview",
            "anthropic/claude-sonnet-4-5",
            "anthropic/claude-opus-4-5",
            "anthropic/claude-opus-4-6",
        };

        public void Initialize()
        {
            _httpClient?.Dispose();
            _httpClient = HttpClientFactoryWithProxy.CreateHttpClientWithProxy();
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "application/json");
            _httpClient.BaseAddress = new Uri(Configuration.Settings.Tools.PerplexityUrl.TrimEnd('/'));
            _httpClient.Timeout = TimeSpan.FromMinutes(15);

            if (!string.IsNullOrEmpty(Configuration.Settings.Tools.PerplexityApiKey))
            {
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + Configuration.Settings.Tools.PerplexityApiKey);
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

        // CURL example for Perplexity API:
        // curl https://api.perplexity.ai/v1/responses \
        //     -H "Authorization: Bearer $PERPLEXITY_API_KEY" \
        //     -H "Content-Type: application/json" \
        //     -d '{
        //     "model": "openai/gpt-5-mini",
        //     "input": "Translate this to French: Hello, how are you?",
        //     "instructions": "You are a professional translator."
        // }'

        // Response example:
        // {
        //     "id": "resp_1234567890",
        //     "object": "response",
        //     "created_at": 1756485272,
        //     "model": "openai/gpt-5.1",
        //     "status": "completed",
        //     "output": [
        //     {
        //         "type": "message",
        //         "role": "assistant",
        //         "content": [
        //         {
        //             "type": "output_text",
        //             "text": "Recent developments in AI include...",
        //             "annotations": [
        //             {
        //                 "type": "citation",
        //                 "url": "https://example.com/article1"
        //             }
        //             ]
        //         }
        //         ]
        //     }
        //     ]
        // }

        public async Task<string> Translate(string text, string sourceLanguageCode, string targetLanguageCode, CancellationToken cancellationToken)
        {
            var model = Configuration.Settings.Tools.PerplexityModel;
            if (string.IsNullOrEmpty(model))
            {
                model = Models[0];
                Configuration.Settings.Tools.PerplexityModel = model;
            }

            if (string.IsNullOrEmpty(Configuration.Settings.Tools.PerplexityPrompt))
            {
                Configuration.Settings.Tools.PerplexityPrompt = new ToolsSettings().PerplexityPrompt;
            }

            var prompt = string.Format(Configuration.Settings.Tools.PerplexityPrompt, sourceLanguageCode, targetLanguageCode);
            var input = prompt + Environment.NewLine + Environment.NewLine + text;

            // Build JSON request body according to Perplexity API
            var requestBody = $"{{\"model\":\"{Json.EncodeJsonText(model)}\",\"input\":\"{Json.EncodeJsonText(input)}\",\"instructions\":\"{Json.EncodeJsonText(prompt)}\"}}";

            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            var result = await _httpClient.PostAsync("/v1/responses", content, cancellationToken);
            var bytes = await result.Content.ReadAsByteArrayAsync();
            var json = Encoding.UTF8.GetString(bytes).Trim();
            if (!result.IsSuccessStatusCode)
            {
                Error = json;
                SeLogger.Error("Perplexity Translate failed calling API: Status code=" + result.StatusCode + Environment.NewLine +
                    json + Environment.NewLine +
                    "input: " + input + Environment.NewLine +
                    "url: " + _httpClient.BaseAddress + "/v1/responses");
            }

            result.EnsureSuccessStatusCode();

            // Parse response according to Perplexity API structure: output[0].content[0].text
            var parser = new SeJsonParser();
            var output = parser.GetFirstObject(json, "output");
            if (output == null)
            {
                return string.Empty;
            }

            var contentObj = parser.GetFirstObject(output, "content");
            if (contentObj == null)
            {
                return string.Empty;
            }

            var resultText = parser.GetFirstObject(contentObj, "text");
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
