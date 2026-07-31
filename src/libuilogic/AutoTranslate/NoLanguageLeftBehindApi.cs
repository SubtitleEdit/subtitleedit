using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.UiLogic.Http;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.UiLogic.Translate;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.UiLogic.AutoTranslate
{
    public class NoLanguageLeftBehindApi : IAutoTranslator, IDisposable
    {
        private HttpClient _httpClient = null!;

        public static string StaticName { get; set; } = "winstxnhdw-nllb-api";
        public override string ToString() => StaticName;
        public string Name => StaticName;
        public string Url => "https://github.com/winstxnhdw/nllb-api";
        public string Error { get; set; } = string.Empty;
        public int MaxCharacters => 250;

        public void Initialize()
        {
            _httpClient?.Dispose();
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "application/json");
            // The endpoint is a relative URI against BaseAddress, and .NET drops the base's
            // last path segment when it lacks a trailing slash - a user-entered
            // ".../api/v4" then silently posts to ".../api/..." and 404s (#12641).
            var url = Configuration.Settings.Tools.AutoTranslateNllbApiUrl;
            if (!string.IsNullOrEmpty(url) && !url.EndsWith('/'))
            {
                url += "/";
            }

            _httpClient.BaseAddress = new Uri(url);
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
            using var result = await _httpClient.PostAsync("translator", content, cancellationToken);
            result.EnsureSuccessStatusCode();

            var responseString = await result.Content.ReadAsStringAsync(cancellationToken);

            var parser = new SeJsonParser();
            var resultText = parser.GetFirstObject(responseString, "result");
            if (resultText == null)
            {
                Error = responseString;
                SeLogger.Error($"{GetType().Name} got unexpected JSON: {responseString}");
                throw new Exception($"{StaticName} returned an unexpected response: {responseString}");
            }

            return Json.DecodeJsonText(resultText);
        }

        public void Dispose() => _httpClient?.Dispose();
    }
}
