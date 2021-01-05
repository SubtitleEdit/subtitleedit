using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Translate.Service
{
    /// <summary>
    /// https://docs.microsoft.com/en-us/azure/cognitive-services/translator/reference/v3-0-translate
    /// </summary>
    public class MicrosoftTranslationService : ITranslationService
    {
        public const string SignUpUrl = "https://docs.microsoft.com/en-us/azure/cognitive-services/translator/translator-text-how-to-signup";
        public const string GoToUrl = "https://www.bing.com/translator";
        private const string LanguagesUrl = "https://api.cognitive.microsofttranslator.com/languages?api-version=3.0&scope=translation";
        private const string TranslateUrl = "https://api.cognitive.microsofttranslator.com/translate?api-version=3.0&from={0}&to={1}";
        private const string SecurityHeaderName = "Ocp-Apim-Subscription-Key";
        private static List<TranslationPair> _translationPairs;
        private string _accessToken;
        private readonly string _category;
        private readonly string _apiKey;
        private readonly string _tokenEndpoint;

        public MicrosoftTranslationService(string apiKey, string tokenEndpoint, string category)
        {
            _apiKey = apiKey;
            _tokenEndpoint = tokenEndpoint;
            _category = category; // Optional parameter - used to get translations from a customized system built with Custom Translator

            try
            {
                _accessToken = GetAccessToken(_apiKey, _tokenEndpoint);
            }
            catch (Exception e)
            {
                throw new TranslationException("Can't get Access Token", e);
            }
        }

        private static string GetAccessToken(string apiKey, string tokenEndpoint)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(tokenEndpoint);
            httpWebRequest.Proxy = Utilities.GetProxy();
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            httpWebRequest.Headers.Add(SecurityHeaderName, apiKey);
            httpWebRequest.ContentLength = 0;
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream() ?? throw new InvalidOperationException()))
            {
                return streamReader.ReadToEnd();
            }
        }

        private List<TranslationPair> GetTranslationPairs()
        {
            if (_translationPairs != null)
            {
                return _translationPairs;
            }

            using (var wc = new WebClient { Proxy = Utilities.GetProxy(), Encoding = Encoding.UTF8 })
            {
                var json = wc.DownloadString(LanguagesUrl);
                _translationPairs = FillTranslationPairsFromJson(json);
                return _translationPairs;
            }
        }

        private static List<TranslationPair> FillTranslationPairsFromJson(string json)
        {
            var list = new List<TranslationPair>();
            var parser = new JsonParser();
            var x = (Dictionary<string, object>)parser.Parse(json);
            foreach (var k in x.Keys)
            {
                if (x[k] is Dictionary<string, object> v)
                {
                    foreach (var innerKey in v.Keys)
                    {
                        if (v[innerKey] is Dictionary<string, object> l)
                        {
                            list.Add(new TranslationPair(l["name"].ToString(), innerKey));
                        }
                    }
                }
            }
            return list;
        }

        public string GetName()
        {
            return "Microsoft translate";
        }

        public string GetUrl()
        {
            return GoToUrl;
        }

        public List<string> Translate(string sourceLanguage, string targetLanguage, List<Paragraph> sourceParagraphs)
        {
            var url = string.Format(TranslateUrl, sourceLanguage, targetLanguage);
            if (!string.IsNullOrEmpty(_category))
            {
                url += "&category=" + _category.Trim();
            }
            var results = new List<string>();
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.Proxy = Utilities.GetProxy();
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";
                httpWebRequest.Headers.Add("Authorization", "Bearer " + _accessToken);

                var jsonBuilder = new StringBuilder();
                jsonBuilder.Append("[");
                bool isFirst = true;
                var formatList = new List<Formatting>();
                for (var index = 0; index < sourceParagraphs.Count; index++)
                {
                    var p = sourceParagraphs[index];
                    if (!isFirst)
                    {
                        jsonBuilder.Append(",");
                    }
                    else
                    {
                        isFirst = false;
                    }

                    var f = new Formatting();
                    formatList.Add(f);
                    var text = f.SetTagsAndReturnTrimmed(TranslationHelper.PreTranslate(p.Text, sourceLanguage), sourceLanguage);
                    text = f.UnBreak(text, p.Text);
                    jsonBuilder.Append("{ \"Text\":\"" + Json.EncodeJsonText(text) + "\"}");
                }

                jsonBuilder.Append("]");
                var json = jsonBuilder.ToString();
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }


                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                var skipCount = 0;
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream() ?? throw new InvalidOperationException()))
                {
                    var result = streamReader.ReadToEnd();

                    var parser = new JsonParser();
                    var x = (List<object>)parser.Parse(result);
                    foreach (var xElement in x)
                    {
                        var dict = (Dictionary<string, object>)xElement;
                        var y = (List<object>)dict["translations"];
                        foreach (var o in y)
                        {
                            var textDics = (Dictionary<string, object>)o;
                            var res = (string)textDics["text"];

                            if (formatList.Count > results.Count - skipCount)
                            {
                                res = formatList[results.Count - skipCount].ReAddFormatting(res);
                                res = formatList[results.Count - skipCount].ReBreak(res, targetLanguage);
                            }
                            res = TranslationHelper.PostTranslate(res, targetLanguage);
                            results.Add(res);
                        }
                    }
                }
            }
            catch (WebException webException)
            {
                throw new TranslationException(webException);
            }

            return results;
        }

        public List<TranslationPair> GetSupportedSourceLanguages()
        {
            return this.GetTranslationPairs();
        }

        public List<TranslationPair> GetSupportedTargetLanguages()
        {
            return this.GetTranslationPairs();
        }

        public int GetMaxTextSize()
        {
            return 1000;
        }

        public int GetMaximumRequestArraySize()
        {
            return 25;
        }

        public override string ToString()
        {
            return GetName();
        }
    }
}