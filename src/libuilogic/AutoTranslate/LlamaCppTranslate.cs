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
    public class LlamaCppTranslate : IAutoTranslator, IDisposable
    {
        private HttpClient _httpClient = null!;

        public static string StaticName { get; set; } = "llama.cpp (local LLM)";
        public override string ToString() => StaticName;
        public string Name => StaticName;
        public string Url => "https://github.com/ggml-org/llama.cpp";
        public string Error { get; set; } = string.Empty;
        public int MaxCharacters => 1000;

        /// <summary>
        /// Endpoint used when the url in settings is only the service base - see <see cref="AutoTranslateUrl"/>.
        /// </summary>
        public const string DefaultUrl = "http://localhost:8080/v1/chat/completions";

        public void Initialize()
        {
            _httpClient?.Dispose();
            _httpClient = HttpClientFactoryWithProxy.CreateHttpClientWithProxy();
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "application/json");
            _httpClient.BaseAddress = new Uri(AutoTranslateUrl.Complete(Configuration.Settings.Tools.LlamaCppApiUrl, DefaultUrl));
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
            var template = Configuration.Settings.Tools.LlamaCppModelPrompt;
            if (string.IsNullOrWhiteSpace(template))
            {
                if (string.IsNullOrWhiteSpace(Configuration.Settings.Tools.LlamaCppPrompt))
                {
                    Configuration.Settings.Tools.LlamaCppPrompt = new ToolsSettings().LlamaCppPrompt;
                }

                template = Configuration.Settings.Tools.LlamaCppPrompt;
            }

            // Placeholders are replaced (not string.Format'ed) so braces in a user-edited prompt
            // cannot throw. The "codes" this engine receives are already English language names -
            // ListLanguages() puts the name in TranslationPair.Code - which is what the templates
            // expect.
            string encodedUserMessage;
            if (template.Contains("{2}"))
            {
                // Completion-format models (MiLMMT-46): the trained prompt with the text embedded
                // must reach the model verbatim, real newlines included - under the "<br />"
                // placeholder encoding the model still translates but starts mirroring placeholder
                // fragments into its output.
                encodedUserMessage = Json.EncodeJsonText(BuildCompletionPrompt(template, sourceLanguageCode, targetLanguageCode, text), "\\n");
            }
            else
            {
                // Historical chat wire format: prompt, a real blank line, then the text - with
                // line breaks inside either encoded as the "<br />" placeholder (decoded back below).
                var prompt = template.Replace("{0}", sourceLanguageCode).Replace("{1}", targetLanguageCode);
                encodedUserMessage = Json.EncodeJsonText(prompt) + "\\n\\n" + Json.EncodeJsonText(text.Trim());
            }

            // No "model" field: llama-server serves the single model it was started with, and for a
            // remote server the user's own llama-server does the same. Sending one would only risk a
            // mismatch with whatever that server has loaded.
            var input = "{ \"messages\": [{ \"role\": \"user\", \"content\": \"" + encodedUserMessage + "\" }]" + MakeSamplingJson() + "}";
            var content = new StringContent(input, Encoding.UTF8);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            var result = await _httpClient.PostAsync(string.Empty, content, cancellationToken);
            var bytes = await result.Content.ReadAsByteArrayAsync(cancellationToken);
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
        /// Fills a completion-format prompt template: {0} = source language English name, {1} =
        /// target language English name, {2} = the text to translate. Models like MiLMMT-46 are
        /// trained with the text inside the prompt and a trailing target-language cue after it.
        /// The text is substituted last so braces in subtitle text (ASSA override tags) can never
        /// hit a placeholder.
        /// </summary>
        public static string BuildCompletionPrompt(string template, string sourceLanguageCode, string targetLanguageCode, string text)
        {
            return template
                .Replace("{0}", sourceLanguageCode)
                .Replace("{1}", targetLanguageCode)
                .Replace("{2}", text.Trim());
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
