using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Core.Translate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Core.AutoTranslate
{
    public class LibreTranslate : IAutoTranslator
    {
        private HttpClient _httpClient;

        public static string StaticName { get; set; } = "LibreTranslate";
        public string Name => StaticName;
        public string Url => "https://github.com/LibreTranslate/LibreTranslate";
        public string Error { get; set; }
        public int MaxCharacters => 220;

        public void Initialize()
        {
            _httpClient?.Dispose();
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "application/json");
            _httpClient.BaseAddress = new Uri(Configuration.Settings.Tools.AutoTranslateLibreUrl);
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
            var apiKey = string.Empty;
            if (!string.IsNullOrEmpty(Configuration.Settings.Tools.AutoTranslateLibreApiKey))
            {
                apiKey = " \"api_key\": \"" + Json.EncodeJsonText(Configuration.Settings.Tools.AutoTranslateLibreApiKey) + "\" ";
            }

            // LibreTranslate seems to have a problem when starting with lowercase letter
            if (text.Length > 0 && char.IsLower(text[0]))
            {
                text = text.CapitalizeFirstLetter();
            }

            var input = "{\"q\": \"" + Json.EncodeJsonText(text.Trim()) + "\", \"source\": \"" + sourceLanguageCode + "\", \"target\": \"" + targetLanguageCode + "\"" + apiKey + "}";
            var content = new StringContent(input, Encoding.UTF8);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            var result = _httpClient.PostAsync("translate", content).Result;
            var bytes = await result.Content.ReadAsByteArrayAsync();
            var json = Encoding.UTF8.GetString(bytes).Trim();
            if (!result.IsSuccessStatusCode)
            {
                Error = json;
                SeLogger.Error("LibreTranslate failed calling API: Status code=" + result.StatusCode + Environment.NewLine + json);
            }
            result.EnsureSuccessStatusCode();
            var parser = new SeJsonParser();
            var resultText = parser.GetFirstObject(json, "translatedText");
            if (resultText == null)
            {
                return string.Empty;
            }

            return Json.DecodeJsonText(resultText).Trim();
        }

        private static List<TranslationPair> ListLanguages()
        {
            var languageCodes = new List<string>
            {
                "ar",
                "az",
                "cs",
                "da",
                "de",
                "el",
                "en",
                "eo",
                "es",
                "fa",
                "fi",
                "fr",
                "ga",
                "he",
                "hi",
                "hu",
                "id",
                "it",
                "ja",
                "ko",
                "nl",
                "pl",
                "pt",
                "ru",
                "ru",
                "sk",
                "sv",
                "tr",
                "uk",
                "zh",
            };

            var result = new List<TranslationPair>();
            var cultures = Utilities.GetSubtitleLanguageCultures(false).ToList();
            foreach (var code in languageCodes)
            {
                var culture = cultures.FirstOrDefault(p => p.TwoLetterISOLanguageName == code);
                if (culture != null)
                {
                    result.Add(new TranslationPair(culture.EnglishName, code, code));
                }
            }

            return result;
        }
    }
}
