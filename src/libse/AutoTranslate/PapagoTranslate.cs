using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Core.Translate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Core.AutoTranslate
{
    public class PapagoTranslate : IAutoTranslator, IDisposable
    {
        private HttpClient _httpClient;

        public static string StaticName { get; set; } = "Papago Translate";

        public override string ToString() => StaticName;

        public string Name => StaticName;

        public string Url => "https://papago.naver.com/";

        public string Error { get; set; }

        public int MaxCharacters => 5000;

        public void Initialize()
        {
            _httpClient?.Dispose();
            _httpClient = HttpClientFactoryWithProxy.CreateHttpClientWithProxy();

            // Updated authentication headers for the current Papago API
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Naver-Client-Id", Configuration.Settings.Tools.AutoTranslatePapagoApiKeyId);
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Naver-Client-Secret", Configuration.Settings.Tools.AutoTranslatePapagoApiKey);

            // Updated base address for the current API
            _httpClient.BaseAddress = new Uri("https://openapi.naver.com/v1/papago/");
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
            // The current API uses form data instead of JSON
            var formData = new Dictionary<string, string>
            {
                { "source", sourceLanguageCode },
                { "target", targetLanguageCode },
                { "text", text.Trim() }
            };

            var content = new FormUrlEncodedContent(formData);

            var result = await _httpClient.PostAsync("n2mt", content, cancellationToken);
            var bytes = await result.Content.ReadAsByteArrayAsync();
            var json = Encoding.UTF8.GetString(bytes).Trim();

            if (!result.IsSuccessStatusCode)
            {
                Error = json;
                SeLogger.Error("PapagoTranslate failed calling API: Status code=" + result.StatusCode + Environment.NewLine + json);
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
                "ko", "en", "ja", "zh-CN", "zh-TW", "vi", "id",
                "th", "de", "ru", "es", "it", "fr", "pt", "hi", "ar"
            };

            var result = new List<TranslationPair>();
            var cultures = Utilities.GetSubtitleLanguageCultures(false).ToList();

            foreach (var code in languageCodes)
            {
                if (code == "zh-CN")
                {
                    result.Add(new TranslationPair("Chinese (simplified)", code, code));
                    continue;
                }

                if (code == "zh-TW")
                {
                    result.Add(new TranslationPair("Chinese (traditional)", code, code));
                    continue;
                }

                var culture = cultures.FirstOrDefault(p => p.TwoLetterISOLanguageName == code);
                if (culture != null)
                {
                    result.Add(new TranslationPair(culture.EnglishName, code, code));
                }
                else
                {
                    // Handle codes that might not have a culture match
                    switch (code)
                    {
                        case "th":
                            result.Add(new TranslationPair("Thai", code, code));
                            break;
                        case "pt":
                            result.Add(new TranslationPair("Portuguese", code, code));
                            break;
                        case "hi":
                            result.Add(new TranslationPair("Hindi", code, code));
                            break;
                        case "ar":
                            result.Add(new TranslationPair("Arabic", code, code));
                            break;
                    }
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