using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Http;
using Nikse.SubtitleEdit.Core.Translate;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Core.AutoTranslate
{
    /// <summary>
    /// Google translate via Google Cloud V2 API - see https://cloud.google.com/translate/
    /// </summary>
    public class GoogleTranslateV2 : IAutoTranslator, IDisposable
    {
        private string _apiKey;
        private IDownloader _httpClient;

        public static string StaticName { get; set; } = "Google Translate V2 API";
        public string Name => StaticName;
        public string Url => "https://translate.google.com/";
        public string Error { get; set; }
        public int MaxCharacters => 1500;

        public void Initialize()
        {
            _apiKey = Configuration.Settings.Tools.GoogleApiV2Key;
            _httpClient = DownloaderFactory.MakeHttpClient();
            _httpClient.BaseAddress = new Uri("https://translation.googleapis.com/language/translate/v2/");
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public List<TranslationPair> GetSupportedSourceLanguages()
        {
            return GoogleTranslateV1.GetTranslationPairs();
        }

        public List<TranslationPair> GetSupportedTargetLanguages()
        {
            return GoogleTranslateV1.GetTranslationPairs();
        }

        public Task<string> Translate(string text, string sourceLanguageCode, string targetLanguageCode, CancellationToken cancellationToken)
        {
            var format = "text";
            var input = new StringBuilder();
            input.Append("q=" + Utilities.UrlEncode(text));
            var uri = $"?{input}&target={targetLanguageCode}&source={sourceLanguageCode}&format={format}&key={_apiKey}";
            string content;
            try
            {
                var result = _httpClient.PostAsync(uri, new StringContent(string.Empty)).Result;

                if (!result.IsSuccessStatusCode)
                {
                    try
                    {
                        Error = result.Content.ReadAsStringAsync().Result;
                        SeLogger.Error($"Error in {StaticName}.Translate: " + Error);
                    }
                    catch
                    {

                        // ignore
                    }
                }

                if ((int)result.StatusCode == 400)
                {                   
                    throw new Exception("API key invalid (or perhaps billing is not enabled)?");
                }
                if ((int)result.StatusCode == 403)
                {
                    throw new Exception("\"Perhaps billing is not enabled (or API key is invalid)?\"");
                }

                if (!result.IsSuccessStatusCode)
                {
                    throw new Exception($"An error occurred calling GT translate - status code: {result.StatusCode}");
                }

                content = result.Content.ReadAsStringAsync().Result;
            }
            catch (WebException webException)
            {
                var message = string.Empty;
                if (webException.Message.Contains("(400) Bad Request"))
                {
                    message = "API key invalid (or perhaps API/billing is not enabled)?";
                }
                else if (webException.Message.Contains("(403) Forbidden."))
                {
                    message = "Perhaps billing is not enabled (or API not enabled or API key is invalid)?";
                }

                throw new Exception(message, webException);
            }

            var resultList = new List<string>();
            var parser = new JsonParser();
            var x = (Dictionary<string, object>)parser.Parse(content);
            foreach (var k in x.Keys)
            {
                if (x[k] is Dictionary<string, object> v)
                {
                    foreach (var innerKey in v.Keys)
                    {
                        if (v[innerKey] is List<object> l)
                        {
                            foreach (var o2 in l)
                            {
                                if (o2 is Dictionary<string, object> v2)
                                {
                                    foreach (var innerKey2 in v2.Keys)
                                    {
                                        if (v2[innerKey2] is string translatedText)
                                        {
                                            try
                                            {
                                                translatedText = Regex.Unescape(translatedText);
                                            }
                                            catch
                                            {
                                                translatedText = translatedText.Replace("\\n", "\n");
                                            }

                                            translatedText = string.Join(Environment.NewLine, translatedText.SplitToLines());
                                            translatedText = TranslationHelper.PostTranslate(translatedText, targetLanguageCode);
                                            resultList.Add(translatedText);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return Task.FromResult(string.Join(Environment.NewLine, resultList));
        }

        public void Dispose() => _httpClient?.Dispose();
    }
}
