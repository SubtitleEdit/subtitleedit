using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Translate;

namespace Nikse.SubtitleEdit.Core.AutoTranslate
{
    /// <summary>
    /// DeepLX translator - see https://github.com/OwO-Network/DeepLX
    /// </summary>
    public class DeepLXTranslate : IAutoTranslator, IDisposable
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

            var postContent = MakeContent(text, sourceLanguageCode, targetLanguageCode);
            var result = _client.PostAsync("/v2/translate", postContent, cancellationToken).Result;
            var resultContent = result.Content.ReadAsStringAsync().Result;

            if (result.StatusCode == HttpStatusCode.ServiceUnavailable || (int)result.StatusCode == httpStatusCodeTooManyRequests)
            {
                Task.Delay(2555).Wait();
                postContent = MakeContent(text, sourceLanguageCode, targetLanguageCode);
                result = _client.PostAsync("/v2/translate", postContent, cancellationToken).Result;
                resultContent = result.Content.ReadAsStringAsync().Result;
            }

            if (result.StatusCode == HttpStatusCode.ServiceUnavailable || (int)result.StatusCode == httpStatusCodeTooManyRequests)
            {
                try
                {
                    _client.Dispose();
                }
                catch
                {
                    // ignore
                }
                Task.Delay(5307).Wait();
                postContent = MakeContent(text, sourceLanguageCode, targetLanguageCode);
                result = _client.PostAsync("/v2/translate", postContent, cancellationToken).Result;
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
                var parser = new JsonParser();
                var x = (Dictionary<string, object>)parser.Parse(resultContent);
                foreach (var k in x.Keys)
                {
                    if (x[k] is List<object> mainList)
                    {
                        foreach (var mainListItem in mainList)
                        {
                            if (mainListItem is Dictionary<string, object> innerDic)
                            {
                                foreach (var transItem in innerDic.Keys)
                                {
                                    if (transItem == "text")
                                    {
                                        var s = innerDic[transItem].ToString();
                                        resultList.Add(s);
                                    }
                                }
                            }
                        }
                    }
                }

                return Task.FromResult(string.Join(Environment.NewLine, resultList));
            }
            catch (Exception ex)
            {
                SeLogger.Error(ex, "DeepLXTranslate.Translate: " + ex.Message + Environment.NewLine + resultContent);
                throw;
            }
        }

        private static FormUrlEncodedContent MakeContent(string text, string sourceLanguageCode, string targetLanguageCode)
        {
            return new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("text", text),
                new KeyValuePair<string, string>("target_lang", targetLanguageCode),
                new KeyValuePair<string, string>("source_lang", sourceLanguageCode),
            });
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}
