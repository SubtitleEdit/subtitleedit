using System;
using System.Collections.Generic;
using System.Net;
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
    public class DeepLXTranslate : IAutoTranslator
    {
        private string _apiUrl;
        private HttpClient _client;

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

            _client = new HttpClient();
            _client.BaseAddress = new Uri(_apiUrl.Trim().TrimEnd('/'));
        }

        public List<TranslationPair> GetSupportedSourceLanguages()
        {
            return new DeepLTranslate().GetSupportedSourceLanguages();
        }

        public List<TranslationPair> GetSupportedTargetLanguages()
        {
            return new DeepLTranslate().GetSupportedTargetLanguages();
        }

        public Task<string> Translate(string text, string sourceLanguageCode, string targetLanguageCode, CancellationToken cancellationToken)
        {
            const int httpStatusCodeTooManyRequests = 429;

            if (sourceLanguageCode.StartsWith("en", StringComparison.InvariantCultureIgnoreCase))
            {
                sourceLanguageCode = "en";
            }
            else if (sourceLanguageCode.StartsWith("pt", StringComparison.InvariantCultureIgnoreCase))
            {
                sourceLanguageCode = "pt";
            }
            else if (sourceLanguageCode.StartsWith("zh", StringComparison.InvariantCultureIgnoreCase))
            {
                sourceLanguageCode = "zh";
            }

            var postContent = MakeStringContent(text, sourceLanguageCode, targetLanguageCode);
            var result = _client.PostAsync("/translate", postContent, cancellationToken).Result;
            var resultContent = result.Content.ReadAsStringAsync().Result;

            if (result.StatusCode == HttpStatusCode.ServiceUnavailable || (int)result.StatusCode == httpStatusCodeTooManyRequests)
            {
                Task.Delay(3755).Wait(cancellationToken);
                postContent = MakeStringContent(text, sourceLanguageCode, targetLanguageCode);
                result = _client.PostAsync("/translate", postContent, cancellationToken).Result;
                resultContent = result.Content.ReadAsStringAsync().Result;
            }

            if (result.StatusCode == HttpStatusCode.ServiceUnavailable || (int)result.StatusCode == httpStatusCodeTooManyRequests)
            {
                Task.Delay(7555).Wait(cancellationToken);
                postContent = MakeStringContent(text, sourceLanguageCode, targetLanguageCode);
                result = _client.PostAsync("/translate", postContent, cancellationToken).Result;
                resultContent = result.Content.ReadAsStringAsync().Result;
            }

            if (result.StatusCode == HttpStatusCode.ServiceUnavailable || (int)result.StatusCode == httpStatusCodeTooManyRequests)
            {
                Task.Delay(9555).Wait(cancellationToken);
                postContent = MakeStringContent(text, sourceLanguageCode, targetLanguageCode);
                result = _client.PostAsync("/translate", postContent, cancellationToken).Result;
                resultContent = result.Content.ReadAsStringAsync().Result;
            }

            if (!result.IsSuccessStatusCode)
            {
                SeLogger.Error("DeepLTranslate error: " + resultContent);
            }

            if (result.StatusCode == HttpStatusCode.Forbidden)
            {
                Error = resultContent;
                throw new Exception("Forbidden! " + Environment.NewLine + Environment.NewLine + resultContent);
            }

            try
            {
                var resultList = new List<string>();
                var parser = new SeJsonParser();
                var alternatives = parser.GetArrayElementsByName(resultContent, "alternatives");
                var data = parser.GetFirstObject(resultContent, "data");

                if (data == null && alternatives.Count > 0 && alternatives[0] != null)
                {
                    data = alternatives[0];
                }

                if (!string.IsNullOrEmpty(data))
                { 
                    var resultText = Json.DecodeJsonText(data);
                    var resultTextWithFixedNewLines = ChatGptTranslate.FixNewLines(resultText);
                    return Task.FromResult(resultTextWithFixedNewLines.Trim());
                }
                else
                {
                    SeLogger.Error("DeepLXTranslate.Translate: " + resultContent);
                    return Task.FromResult(string.Empty);
                }
            }
            catch (Exception ex)
            {
                SeLogger.Error(ex, "DeepLXTranslate.Translate: " + ex.Message + Environment.NewLine + resultContent);
                throw;
            }
        }

        private static StringContent MakeStringContent(string text, string sourceLanguageCode, string targetLanguageCode)
        {
            var input = "{ \"source_lang\": \"" + sourceLanguageCode + "\", \"target_lang\": \"" + targetLanguageCode + "\", \"text\": \"" + Json.EncodeJsonText(text.Trim()) + "\" }]}";
            var content = new StringContent(input, Encoding.UTF8);
            return content;
        }
    }
}
