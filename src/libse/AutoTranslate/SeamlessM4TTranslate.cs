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
    public class SeamlessM4TTranslate : IAutoTranslator, IDisposable
    {
        private HttpClient _httpClient;

        public static string StaticName { get; set; } = "SeamlessM4T";
        public override string ToString() => StaticName;
        public string Name => StaticName;
        public string Url => "https://replicate.com/cjwbw/seamless_communication/api";
        public string Error { get; set; }
        public int MaxCharacters => 250;

        public void Initialize()
        {
            _httpClient?.Dispose();
            _httpClient = HttpClientFactoryWithProxy.CreateHttpClientWithProxy();
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "application/json");
            _httpClient.BaseAddress = new Uri(Configuration.Settings.Tools.AutoTranslateSeamlessM4TUrl.TrimEnd('/') + "/");
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
            if (sourceLanguageCode == "en")
            {
                sourceLanguageCode = "English";
            }
            if (sourceLanguageCode == "da")
            {
                sourceLanguageCode = "Danish";
            }

            if (targetLanguageCode == "en")
            {
                targetLanguageCode = "English";
            }
            if (targetLanguageCode == "da")
            {
                targetLanguageCode = "Danish";
            }

            var input = "{ \"input\": { \"task_name\":\"T2TT (Text to Text translation)\", \"input_text\": \"" + Json.EncodeJsonText(text.Trim()) + "\", \"input_text_language\": \"" + sourceLanguageCode + "\", \"target_language_text_only\": \"" + targetLanguageCode + "\" }}";
            var content = new StringContent(input, Encoding.UTF8);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            var result = _httpClient.PostAsync("predictions", content).Result;
            result.EnsureSuccessStatusCode();
            var bytes = await result.Content.ReadAsByteArrayAsync();
            var json = Encoding.UTF8.GetString(bytes).Trim();

            var parser = new SeJsonParser();
            var textOutput = parser.GetFirstObject(json, "text_output");
            var resultText = parser.GetFirstObject(textOutput, "title");
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
                var culture = cultures.FirstOrDefault(p=>p.TwoLetterISOLanguageName == code);
                if (culture != null)
                {
                    result.Add(new TranslationPair(culture.EnglishName, code));
                }
            }

            return result;
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
