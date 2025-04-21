using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Core.Translate;

namespace Nikse.SubtitleEdit.Core.AutoTranslate
{
    /// <summary>
    /// DeepLX translator - see https://github.com/OwO-Network/DeepLX
    /// </summary>
    public class DeepLXTranslate : IAutoTranslator, IDisposable
    {
        private string _apiUrl;
        private HttpClient _httpClient;

        public static string StaticName { get; set; } = "DeepLX translate";
        public override string ToString() => StaticName;
        public string Name => StaticName;
        public string Url => "https://github.com/OwO-Network/DeepLX";
        public string Error { get; set; }
        public int MaxCharacters => 1500;

        public void Initialize()
        {
            if (string.IsNullOrEmpty(Configuration.Settings.Tools.AutoTranslateDeepLXUrl))
            {
                Configuration.Settings.Tools.AutoTranslateDeepLXUrl = "http://localhost:1188";
            }
            _apiUrl = Configuration.Settings.Tools.AutoTranslateDeepLXUrl;

            _httpClient = HttpClientFactoryWithProxy.CreateHttpClientWithProxy();
            _httpClient.BaseAddress = new Uri(_apiUrl.Trim().TrimEnd('/'));
        }

        public List<TranslationPair> GetSupportedSourceLanguages()
        {
            return new DeepLTranslate().GetSupportedSourceLanguages();
        }

        public List<TranslationPair> GetSupportedTargetLanguages()
        {
            return new DeepLTranslate().GetSupportedTargetLanguages();
        }

        public async Task<string> Translate(string text, string sourceLanguageCode, string targetLanguageCode, CancellationToken cancellationToken)
        {
            int[] retryDelays = { 555, 3007, 7013 };
            HttpResponseMessage result = null;
            string resultContent = null;
            for (var attempt = 0; attempt <= retryDelays.Length; attempt++)
            {
                var postContent = MakeContent(text, sourceLanguageCode, targetLanguageCode);
                result = await _httpClient.PostAsync("/translate", postContent, cancellationToken);
                resultContent = await result.Content.ReadAsStringAsync();

                if (!DeepLTranslate.ShouldRetry(result, resultContent) || attempt == retryDelays.Length)
                {
                    break;
                }

                await Task.Delay(retryDelays[attempt], cancellationToken);
            }

            if (!result.IsSuccessStatusCode)
            {
                throw new Exception("DeepLXTranslate error: StatusCode=" + result.StatusCode + Environment.NewLine + resultContent);
            }

            try
            {
                var parser = new SeJsonParser();
                var alternatives = parser.GetArrayElementsByName(resultContent, "alternatives");
                var data = string.Empty;
                if (alternatives.Count > 0 && alternatives[0] != null)
                {
                    data = alternatives[0];
                }

                if (!string.IsNullOrEmpty(data))
                {
                    var resultText = Json.DecodeJsonText(data);
                    var resultTextWithFixedNewLines = ChatGptTranslate.FixNewLines(resultText);
                    return resultTextWithFixedNewLines.Trim();
                }
                else
                {
                    SeLogger.Error("DeepLXTranslate.Translate: " + resultContent);
                    throw new Exception("DeepLXTranslate gave empty alternatives: StatusCode=" + result.StatusCode + Environment.NewLine + resultContent);
                }
            }
            catch (Exception ex)
            {
                SeLogger.Error(ex, "DeepLXTranslate.Translate: " + ex.Message + Environment.NewLine + resultContent);
                throw;
            }
        }

        private static StringContent MakeContent(string text, string sourceLanguageCode, string targetLanguageCode)
        {
            var input = "{ \"source_lang\": \"" + sourceLanguageCode + "\", \"target_lang\": \"" + targetLanguageCode + "\", \"text\": \"" + Json.EncodeJsonText(text.Trim(), "\\n") + "\" }]}";
            var content = new StringContent(input, Encoding.UTF8);
            return content;
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
