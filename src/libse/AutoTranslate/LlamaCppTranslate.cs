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
    public class LlamaCppTranslate : IAutoTranslator, IDisposable
    {
        private HttpClient _httpClient;

        public static string StaticName { get; set; } = "llama.cpp (local LLM)";
        public override string ToString() => StaticName;
        public string Name => StaticName;
        public string Url => "https://github.com/ggml-org/llama.cpp";
        public string Error { get; set; }
        public int MaxCharacters => 1000;

        public void Initialize()
        {
            _httpClient?.Dispose();
            _httpClient = HttpClientFactoryWithProxy.CreateHttpClientWithProxy();
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "application/json");
            _httpClient.BaseAddress = new Uri(Configuration.Settings.Tools.LlamaCppApiUrl.TrimEnd('/'));
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
            string prompt;
            var modelPrompt = Configuration.Settings.Tools.LlamaCppModelPrompt;
            if (!string.IsNullOrWhiteSpace(modelPrompt))
            {
                // A curated model with its own trained-in prompt (e.g. Hy-MT2). The "codes" this
                // engine receives are already English language names - ListLanguages() puts the
                // name in TranslationPair.Code - which is exactly what these templates expect.
                prompt = string.Format(modelPrompt, sourceLanguageCode, targetLanguageCode);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(Configuration.Settings.Tools.LlamaCppPrompt))
                {
                    Configuration.Settings.Tools.LlamaCppPrompt = new ToolsSettings().LlamaCppPrompt;
                }
                prompt = string.Format(Configuration.Settings.Tools.LlamaCppPrompt, sourceLanguageCode, targetLanguageCode);
            }

            // No "model" field: llama-server serves the single model it was started with, and for a
            // remote server the user's own llama-server does the same. Sending one would only risk a
            // mismatch with whatever that server has loaded.
            var input = "{ \"messages\": [{ \"role\": \"user\", \"content\": \"" + Json.EncodeJsonText(prompt) + "\\n\\n" + Json.EncodeJsonText(text.Trim()) + "\" }]" + MakeSamplingJson() + "}";
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
            outputText = ChatGptTranslate.DecodeUnicodeEscapes(outputText);
            return outputText.Trim();
        }

        /// <summary>
        /// Model-recommended sampling parameters as extra JSON fields (empty when the selected
        /// model defines none, keeping the server defaults - the behavior before per-model
        /// sampling existed).
        /// </summary>
        private static string MakeSamplingJson()
        {
            var sb = new StringBuilder();
            var tools = Configuration.Settings.Tools;
            if (tools.LlamaCppModelTemperature >= 0)
            {
                sb.Append(", \"temperature\": ").Append(tools.LlamaCppModelTemperature.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
            if (tools.LlamaCppModelTopP >= 0)
            {
                sb.Append(", \"top_p\": ").Append(tools.LlamaCppModelTopP.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
            if (tools.LlamaCppModelTopK >= 0)
            {
                sb.Append(", \"top_k\": ").Append(tools.LlamaCppModelTopK.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
            if (tools.LlamaCppModelRepeatPenalty >= 0)
            {
                sb.Append(", \"repeat_penalty\": ").Append(tools.LlamaCppModelRepeatPenalty.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }

            return sb.ToString();
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
