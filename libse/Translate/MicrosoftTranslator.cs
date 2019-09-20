using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.Core.Translate
{
    /// <summary>
    /// https://docs.microsoft.com/en-us/azure/cognitive-services/translator/reference/v3-0-translate
    /// </summary>
    public class MicrosoftTranslator : ITranslator
    {
        public const string SignUpUrl = "https://docs.microsoft.com/en-us/azure/cognitive-services/translator/translator-text-how-to-signup";
        public const string GoToUrl = "https://www.bing.com/translator";
        public const int MaximumRequestArrayLength = 25;
        private const string LanguagesUrl = "https://api.cognitive.microsofttranslator.com/languages?api-version=3.0&scope=translation";
        private const string TranslateUrl = "https://api.cognitive.microsofttranslator.com/translate?api-version=3.0&from={0}&to={1}";
        private const string SecurityHeaderName = "Ocp-Apim-Subscription-Key";
        private static List<TranslationPair> _translationPairs;
        private readonly string _accessToken;

        public MicrosoftTranslator(string apiKey, string tokenEndpoint)
        {
            _accessToken = GetAccessToken(apiKey, tokenEndpoint);
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

        public List<TranslationPair> GetTranslationPairs()
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

        public List<string> Translate(string sourceLanguage, string targetLanguage, List<Paragraph> paragraphs, StringBuilder log)
        {
            var url = string.Format(TranslateUrl, sourceLanguage, targetLanguage);
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.Proxy = Utilities.GetProxy();
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            httpWebRequest.Headers.Add("Authorization", "Bearer " + _accessToken);

            var jsonBuilder = new StringBuilder();
            jsonBuilder.Append("[");
            bool isFirst = true;
            foreach (var p in paragraphs)
            {
                if (!isFirst)
                {
                    jsonBuilder.Append(",");
                }
                else
                {
                    isFirst = false;
                }
                jsonBuilder.Append("{ \"Text\":\"" + Json.EncodeJsonText(p.Text) + "\"}");
            }
            jsonBuilder.Append("]");
            string json = jsonBuilder.ToString();
            httpWebRequest.ContentLength = json.Length;
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            var results = new List<string>();
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
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
                        res = TranslationHelper.PostTranslate(res, targetLanguage);
                        results.Add(res);
                    }
                }
            }
            return results;
        }
    }
}
