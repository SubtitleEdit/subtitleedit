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
        private IDownloader _httpClient;

        public static string StaticName { get; set; } = "winstxnhdw-nllb-api";
        public string Name => StaticName;
        public string Url => "https://github.com/winstxnhdw/nllb-api";
        public string Error { get; set; }
        public int MaxCharacters => 250;

        public void Initialize()
        {
            _httpClient?.Dispose();
            _httpClient = DownloaderFactory.MakeHttpClient();
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
            var result = _httpClient.PostAsync("translate", content).Result;
            result.EnsureSuccessStatusCode();
            var bytes = await result.Content.ReadAsByteArrayAsync();
            var resultText = Encoding.UTF8.GetString(bytes).Trim();

            // error messages are returned as json, translated text are just returned as normal utf-8 text
            var validator = new SeJsonValidator();
            var isValidJson = validator.ValidateJson(resultText);
            if (isValidJson)
            {
                SeLogger.Error($"{this.GetType().Name} got json back which is probably an error: {resultText}");
            }

            return resultText;
        }

        public void Dispose() => _httpClient?.Dispose();
    }
}
