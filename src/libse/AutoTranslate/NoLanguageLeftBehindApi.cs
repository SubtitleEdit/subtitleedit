using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Http;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Core.Translate;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Core.AutoTranslate
{
    public class NoLanguageLeftBehindApi : IAutoTranslator, IDisposable
    {
        private HttpClient _httpClient;

        public static string StaticName { get; set; } = "winstxnhdw-nllb-api";
        public override string ToString() => StaticName;
        public string Name => StaticName;
        public string Url => "https://github.com/winstxnhdw/nllb-api";
        public string Error { get; set; }
        public int MaxCharacters => 250;

        public void Initialize()
        {
            _httpClient?.Dispose();
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "application/json");
            _httpClient.BaseAddress = new Uri(Configuration.Settings.Tools.AutoTranslateNllbApiUrl);
        }

        public List<TranslationPair> GetSupportedSourceLanguages()
        {
            return new NoLanguageLeftBehindServe().GetSupportedSourceLanguages();
        }

        public List<TranslationPair> GetSupportedTargetLanguages()
        {
            return new NoLanguageLeftBehindServe().GetSupportedTargetLanguages();
        }

        public async Task<string> Translate(string text, string sourceLanguageCode, string targetLanguageCode, CancellationToken cancellationToken)
        {
            var content = new StringContent("{ \"text\": \"" + Json.EncodeJsonText(text) + "\",  \"source\": \"" + sourceLanguageCode + "\", \"target\": \"" + targetLanguageCode + "\" }", Encoding.UTF8, "application/json");
            var result = await _httpClient.PostAsync("translator", content);
            result.EnsureSuccessStatusCode();

            var responseString = await result.Content.ReadAsStringAsync();

            var validator = new SeJsonValidator();
            var isValidJson = validator.ValidateJson(responseString);
            if (isValidJson)
            {
                const string key = "\"result\":";
                var startIndex = responseString.IndexOf(key, StringComparison.OrdinalIgnoreCase);
                if (startIndex >= 0)
                {
                    startIndex += key.Length;

                    var firstQuoteIndex = responseString.IndexOf('"', startIndex);
                    var secondQuoteIndex = responseString.IndexOf('"', firstQuoteIndex + 1);

                    if (firstQuoteIndex >= 0 && secondQuoteIndex > firstQuoteIndex)
                    {
                        return responseString.Substring(firstQuoteIndex + 1, secondQuoteIndex - firstQuoteIndex - 1);
                    }
                }

                SeLogger.Error($"{this.GetType().Name} recibió un JSON inesperado: {responseString}");
            }
            else
            {
                SeLogger.Error($"{this.GetType().Name} no recibió un JSON válido: {responseString}");
            }

            return responseString;
        }

        public void Dispose() => _httpClient?.Dispose();
    }
}
