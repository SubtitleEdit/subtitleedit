using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Http;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Core.Translate;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Core.AutoTranslate
{
    /// <summary>
    /// https://docs.microsoft.com/en-us/azure/cognitive-services/translator/reference/v3-0-translate
    /// </summary>
    public class MicrosoftTranslator : IAutoTranslator
    {
        public const string SignUpUrl = "https://learn.microsoft.com/en-us/azure/ai-services/translator/create-translator-resource";
        public const string GoToUrl = "https://www.bing.com/translator";
        private const string LanguagesUrl = "https://api.cognitive.microsofttranslator.com/languages?api-version=3.0&scope=translation";
        private const string TranslateUrl = "translate?api-version=3.0&from={0}&to={1}";
        private const string SecurityHeaderName = "Ocp-Apim-Subscription-Key";
        private static List<TranslationPair> _translationPairs;
        private string _accessToken;
        private string _category;
        private IDownloader _httpClient;

        public static string StaticName { get; set; } = "Bing Microsoft Translator";
        public string Name => StaticName;
        public string Url => "https://www.bing.com/translator";
        public string Error { get; set; }
        public int MaxCharacters => 1500;

        public void Initialize()
        {
            var apiKey = Configuration.Settings.Tools.MicrosoftTranslatorApiKey;
            var tokenEndpoint = Configuration.Settings.Tools.MicrosoftTranslatorTokenEndpoint;
            _category = Configuration.Settings.Tools.MicrosoftTranslatorCategory;

            try
            {
                _accessToken = GetAccessToken(apiKey, tokenEndpoint);
            }
            catch (Exception e)
            {
                throw new Exception("Can't get Access Token", e);
            }
        }

        public List<TranslationPair> GetSupportedSourceLanguages()
        {
            return GetTranslationPairs();
        }

        public List<TranslationPair> GetSupportedTargetLanguages()
        {
            return GetTranslationPairs();
        }

        public Task<string> Translate(string text, string sourceLanguageCode, string targetLanguageCode, CancellationToken cancellationToken)
        {
            var url = string.Format(TranslateUrl, sourceLanguageCode, targetLanguageCode);
            if (!string.IsNullOrEmpty(_category))
            {
                url += "&category=" + _category.Trim();
            }

            var results = new List<string>();

            var httpClient = GetTranslateClient();
            var jsonBuilder = new StringBuilder();
            jsonBuilder.Append("[");
            jsonBuilder.Append("{ \"Text\":\"" + Json.EncodeJsonText(text) + "\"}");
            jsonBuilder.Append("]");
            var json = jsonBuilder.ToString();
            var content = new StringContent(json, Encoding.UTF8);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            var result = httpClient.PostAsync(url, content).Result;
            var parser = new JsonParser();
            var jsonResult = result.Content.ReadAsStringAsync().Result;

            if (!result.IsSuccessStatusCode)
            {
                Error = json;

                if (result.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new Exception("API key is not valid!" + Environment.NewLine + Environment.NewLine + jsonResult);
                }

                throw new Exception("An error occurred during translate:" + Environment.NewLine + Environment.NewLine + jsonResult);
            }

            var x = (List<object>)parser.Parse(jsonResult);
            foreach (var xElement in x)
            {
                var dict = (Dictionary<string, object>)xElement;
                var y = (List<object>)dict["translations"];
                foreach (var o in y)
                {
                    var textDictionary = (Dictionary<string, object>)o;
                    var res = (string)textDictionary["text"];
                    res = res.Replace("<br />", Environment.NewLine);
                    res = res.Replace("<br/>", Environment.NewLine);
                    res = res.Replace("<br>", Environment.NewLine);
                    results.Add(res);
                }
            }

            return Task.FromResult(string.Join(Environment.NewLine, results));
        }

        private IDownloader GetTranslateClient()
        {
            if (_httpClient == null)
            {
                _httpClient = DownloaderFactory.MakeHttpClient();
                _httpClient.BaseAddress = new Uri("https://api.cognitive.microsofttranslator.com/");
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            }

            return _httpClient;
        }

        private static string GetAccessToken(string apiKey, string tokenEndpoint)
        {
            using (var httpClient = DownloaderFactory.MakeHttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation(SecurityHeaderName, apiKey);
                var response = httpClient.PostAsync(tokenEndpoint, new StringContent(string.Empty)).Result;
                var result = response.Content.ReadAsStringAsync().Result;
                if (!response.IsSuccessStatusCode)
                {
                    SeLogger.Error($"{StaticName}: Error getting access token via {tokenEndpoint} and API key {apiKey}: {result}");
                }

                return response.Content.ReadAsStringAsync().Result;
            }
        }

        private static List<TranslationPair> GetTranslationPairs()
        {
            if (_translationPairs != null)
            {
                return _translationPairs;
            }

            using (var httpClient = DownloaderFactory.MakeHttpClient())
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
                            list.Add(new TranslationPair(l["name"].ToString(), innerKey, innerKey));
                        }
                    }
                }
            }
            return list;
        }
    }
}
