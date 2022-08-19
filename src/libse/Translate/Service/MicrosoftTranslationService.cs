using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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
        private const string TranslateUrl = "translate?api-version=3.0&from={0}&to={1}";
        private const string SecurityHeaderName = "Ocp-Apim-Subscription-Key";
        private static List<TranslationPair> _translationPairs;
        private readonly string _accessToken;
        private readonly string _category;
        private HttpClient _httpClient;

        public MicrosoftTranslationService(string apiKey, string tokenEndpoint, string category)
        {
            _category = category; // Optional parameter - used to get translations from a customized system built with Custom Translator

            try
            {
                _accessToken = GetAccessToken(apiKey, tokenEndpoint);
            }
            catch (Exception e)
            {
                throw new TranslationException("Can't get Access Token", e);
            }
        }

        private HttpClient GetTranslateClient()
        {
            if (_httpClient == null)
            {
                _httpClient = HttpClientHelper.MakeHttpClient();
                _httpClient.BaseAddress = new Uri("https://api.cognitive.microsofttranslator.com/");
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            }

            return _httpClient;
        }

        private static string GetAccessToken(string apiKey, string tokenEndpoint)
        {
            var httpClient = HttpClientHelper.MakeHttpClient();
            httpClient.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation(SecurityHeaderName, apiKey);
            var response = httpClient.PostAsync(tokenEndpoint, new StringContent(string.Empty)).Result;
            return response.Content.ReadAsStringAsync().Result;
        }

        private static List<TranslationPair> GetTranslationPairs()
        {
            if (_translationPairs != null)
            {
                return _translationPairs;
            }

            using (var httpClient = HttpClientHelper.MakeHttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36");
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=UTF-8");
                var json = httpClient.GetStringAsync(LanguagesUrl).Result;
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
                var httpClient = GetTranslateClient();
                var jsonBuilder = new StringBuilder();
                jsonBuilder.Append("[");
                var isFirst = true;
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
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var result = httpClient.PostAsync(url, content).Result;

                var parser = new JsonParser();
                var x = (List<object>)parser.Parse(result.Content.ReadAsStringAsync().Result);
                foreach (var xElement in x)
                {
                    var dict = (Dictionary<string, object>)xElement;
                    var y = (List<object>)dict["translations"];
                    foreach (var o in y)
                    {
                        var textDics = (Dictionary<string, object>)o;
                        var res = (string)textDics["text"];

                        if (formatList.Count > results.Count)
                        {
                            res = formatList[results.Count].ReAddFormatting(res);
                            res = formatList[results.Count].ReBreak(res, targetLanguage);
                        }
                        res = TranslationHelper.PostTranslate(res, targetLanguage);
                        results.Add(res);
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
            return GetTranslationPairs();
        }

        public List<TranslationPair> GetSupportedTargetLanguages()
        {
            return GetTranslationPairs();
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