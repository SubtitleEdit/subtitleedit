using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Settings;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Core.Translate;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Core.AutoTranslate
{
    public class GeminiTranslate : IAutoTranslator, IDisposable
    {
        public static string StaticName { get; set; } = "Google Gemini";
        public override string ToString() => StaticName;
        public string Name => StaticName;
        public string Url => "https://deepmind.google/technologies/gemini/";
        public string Error { get; set; }
        public int MaxCharacters => 1500;

        private HttpClient _httpClient;
        private string _baseUrl;


        /// <summary>
        /// See https://cloud.google.com/vertex-ai/generative-ai/docs/learn/models 
        /// </summary>
        public static string[] Models => new[]
        {
            // Auto-updating Aliases
            "gemini-flash-latest", 
            "gemini-pro-latest",  

            // Gemini 3 - Latest Generation
            "gemini-3-pro",
            "gemini-3-flash",
            "gemini-3-deep-think",

            // Gemini 2.5 - Stable Reasoning Models
            "gemini-2.5-pro",
            "gemini-2.5-flash",
            "gemini-2.5-flash-lite",
            "gemini-2.5-flash-image",
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
            HttpResponseMessage result = null;
            string resultContent = null;
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

                resultContent = await result.Content.ReadAsStringAsync();

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
                return string.Empty;
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
            return new List<TranslationPair>
            {
               MakePair("Albanian","sq"),
               MakePair("Arabic","ar"),
               MakePair("Armenian","hy"),
               MakePair("Awadhi","ay"),
               MakePair("Azerbaijani","az"),
               MakePair("Bashkir","ba"),
               MakePair("Basque","eu"),
               MakePair("Belarusian","be"),
               MakePair("Bengali","bn"),
               MakePair("Bhojpuri",""),
               MakePair("Bosnian","bs"),
               MakePair("Brazilian Portuguese","br"),
               MakePair("Bulgarian","bg"),
               MakePair("Cantonese","zh"),
               MakePair("Catalan","ca"),
               MakePair("Chhattisgarhi",""),
               MakePair("Chinese (Simplified)", "zho-Hans"),
               MakePair("Chinese (Traditional)","zh-Hant"),
               MakePair("Croatian","hr"),
               MakePair("Czech","cs"),
               MakePair("Danish","da"),
               MakePair("Dogri",""),
               MakePair("Dutch","nl"),
               MakePair("English","en"),
               MakePair("Estonian","et"),
               MakePair("Faroese","fo"),
               MakePair("Finnish","fi"),
               MakePair("French","fr"),
               MakePair("Galician","gl"),
               MakePair("Georgian","ka"),
               MakePair("German","de"),
               MakePair("Greek","el"),
               MakePair("Gujarati","gu"),
               MakePair("Haryanvi",""),
               MakePair("Hebrew","iw"),
               MakePair("Hindi","hi"),
               MakePair("Hungarian","hu"),
               MakePair("Icelandic","is"),
               MakePair("Indonesian","id"),
               MakePair("Irish","ga"),
               MakePair("Italian","it"),
               MakePair("Japanese","ja"),
               MakePair("Javanese","jv"),
               MakePair("Kannada","kn"),
               MakePair("Kashmiri","ks"),
               MakePair("Kazakh","kk"),
               MakePair("Kurdish","ku"),
               MakePair("Central Kurdish (Sorani)","ckb"),
               MakePair("Konkani",""),
               MakePair("Korean","ko"),
               MakePair("Kyrgyz","ky"),
               MakePair("Latvian","lv"),
               MakePair("Lithuanian","lt"),
               MakePair("Macedonian","mk"),
               MakePair("Maithili",""),
               MakePair("Malay","ms"),
               MakePair("Maltese","mt"),
               MakePair("Mandarin","zh"),
               MakePair("Mandarin Chinese","zh"),
               MakePair("Marathi","mr"),
               MakePair("Marwari",""),
               MakePair("Min Nan",""),
               MakePair("Mongolian","mn"),
               MakePair("Montenegrin",""),
               MakePair("Myanmar(Burmese)", "my"),
               MakePair("Nepali","ne"),
               MakePair("Norwegian","no"),
               MakePair("Oriya","or"),
               MakePair("Pashto","ps"),
               MakePair("Persian","fa"),
               MakePair("Polish","pl"),
               MakePair("Portuguese","pt"),
               MakePair("Punjabi","pa"),
               MakePair("Rajasthani",""),
               MakePair("Romanian","ro"),
               MakePair("Russian","ru"),
               MakePair("Sanskrit","sa"),
               MakePair("Santali",""),
               MakePair("Serbian","sr"),
               MakePair("Sindhi","sd"),
               MakePair("Sinhala","si"),
               MakePair("Slovak","sk"),
               MakePair("Slovene","sl"),
               MakePair("Slovenian","sl"),
               MakePair("Spanish","es"),
               MakePair("Spanish (Latin America)","es-419"),
               MakePair("Swedish","sv"),
               MakePair("Tatar","tt"),
               MakePair("Thai","th"),
               MakePair("Turkish","tr"),
               MakePair("Ukrainian","uk"),
               MakePair("Urdu","ur"),
               MakePair("Uzbek","uz"),
               MakePair("Vietnamese","vi"),
               MakePair("Welsh","cy"),
               MakePair("Wu",""),
            };
        }

        private static TranslationPair MakePair(string nameCode, string twoLetter)
        {
            return new TranslationPair(nameCode, nameCode, twoLetter);
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
