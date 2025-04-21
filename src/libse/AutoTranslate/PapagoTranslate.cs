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
    public class PapagoTranslate : IAutoTranslator, IDisposable
    {
        private HttpClient _httpClient;

        public static string StaticName { get; set; } = "Papago Translate";
        public override string ToString() => StaticName;
        public string Name => StaticName;
        public string Url => "https://papago.naver.com/";
        public string Error { get; set; }
        public int MaxCharacters => 1500;

        public void Initialize()
        {
            _httpClient?.Dispose();
            _httpClient = HttpClientFactoryWithProxy.CreateHttpClientWithProxy();
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "application/json");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-NCP-APIGW-API-KEY-ID", Configuration.Settings.Tools.AutoTranslatePapagoApiKeyId);
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-NCP-APIGW-API-KEY", Configuration.Settings.Tools.AutoTranslatePapagoApiKey);
            _httpClient.BaseAddress = new Uri("https://naveropenapi.apigw.ntruss.com/nmt/v1/");
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
            var input = "{\"text\": \"" + Json.EncodeJsonText(text.Trim()) + "\", \"source\": \"" + sourceLanguageCode + "\", \"target\": \"" + targetLanguageCode + "\"}";
            var content = new StringContent(input, Encoding.UTF8);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            var result = await _httpClient.PostAsync("translation", content, cancellationToken);
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
                "ko",
                "en",
                "ja",
                "zh-CN",
                "zh-TW",
                "vi",
                "id",
                "fr",
                "es",
                "ru",
                "de",
                "it",
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

                var culture = cultures.FirstOrDefault(p=>p.TwoLetterISOLanguageName == code);
                if (culture != null)
                {
                    result.Add(new TranslationPair(culture.EnglishName, code, code));
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
