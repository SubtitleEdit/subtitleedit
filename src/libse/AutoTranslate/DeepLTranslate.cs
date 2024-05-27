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
    /// DeepL Pro V2 translator - see https://www.deepl.com/api.html
    /// </summary>
    public class DeepLTranslate : IAutoTranslator
    {
        private string _apiKey;
        private string _apiUrl;
        private string _formality;
        private HttpClient _client;

        public static string StaticName { get; set; } = "DeepL V2 translate";
        public string Name => StaticName;
        public string Url => "https://www.deepl.com";
        public string Error { get; set; }
        public int MaxCharacters => 1500;

        public void Initialize()
        {
            _apiKey = Configuration.Settings.Tools.AutoTranslateDeepLApiKey;
            _apiUrl = Configuration.Settings.Tools.AutoTranslateDeepLUrl;
            _formality = Configuration.Settings.Tools.AutoTranslateDeepLFormality;

            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_apiUrl))
            {
                return;
            }

            _client = new HttpClient();
            _client.BaseAddress = new Uri(_apiUrl.Trim().TrimEnd('/'));
            _client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "DeepL-Auth-Key " + _apiKey.Trim());
            _formality = string.IsNullOrWhiteSpace(_formality) ? "default" : _formality.Trim();
        }

        public List<TranslationPair> GetSupportedSourceLanguages()
        {
            return new List<TranslationPair>
            {
                MakeTranslationPair("Arabic", "ar"),
                MakeTranslationPair("Bulgarian", "bg"),
                MakeTranslationPair("Chinese", "zh"),
                MakeTranslationPair("Czech", "cs"),
                MakeTranslationPair("Danish", "da"),
                MakeTranslationPair("Dutch", "nl", true),
                MakeTranslationPair("English", "en", true),
                MakeTranslationPair("Estonian", "et"),
                MakeTranslationPair("Finnish", "fi"),
                MakeTranslationPair("French", "fr", true),
                MakeTranslationPair("German", "de", true),
                MakeTranslationPair("Greek", "el"),
                MakeTranslationPair("Hungarian", "hu"),
                MakeTranslationPair("Indonesian", "id"),
                MakeTranslationPair("Italian", "it", true),
                MakeTranslationPair("Japanese", "ja", true),
                MakeTranslationPair("Korean", "ko"),
                MakeTranslationPair("Latvian", "lv"),
                MakeTranslationPair("Lithuanian", "lt"),
                MakeTranslationPair("Norwegian (Bokmål)", "nb"),
                MakeTranslationPair("Polish", "pl", true),
                MakeTranslationPair("Portuguese", "pt", true),
                MakeTranslationPair("Romanian", "ro"),
                MakeTranslationPair("Russian", "ru", true),
                MakeTranslationPair("Slovak", "sk"),
                MakeTranslationPair("Slovenian", "sl"),
                MakeTranslationPair("Spanish", "es", true),
                MakeTranslationPair("Swedish", "sv"),
                MakeTranslationPair("Turkish", "tr"),
                MakeTranslationPair("Ukranian", "uk"),
            };
        }

        public List<TranslationPair> GetSupportedTargetLanguages()
        {
            return new List<TranslationPair>
            {
                MakeTranslationPair("Arabic", "ar"),
                MakeTranslationPair("Bulgarian", "bg"),
                MakeTranslationPair("Chinese", "zh"),
                MakeTranslationPair("Czech", "cs"),
                MakeTranslationPair("Danish", "da"),
                MakeTranslationPair("Dutch", "nl", true),
                MakeTranslationPair("English (British)", "en-gb", true),
                MakeTranslationPair("English (American)", "en-us", true),
                MakeTranslationPair("Estonian", "et"),
                MakeTranslationPair("Finnish", "fi"),
                MakeTranslationPair("French", "fr", true),
                MakeTranslationPair("German", "de", true),
                MakeTranslationPair("Greek", "el"),
                MakeTranslationPair("Hungarian", "hu"),
                MakeTranslationPair("Indonesian", "id"),
                MakeTranslationPair("Italian", "it", true),
                MakeTranslationPair("Japanese", "ja", true),
                MakeTranslationPair("Korean", "ko"),
                MakeTranslationPair("Latvian", "lv"),
                MakeTranslationPair("Lithuanian", "lt"),
                MakeTranslationPair("Norwegian (Bokmål)", "nb"),
                MakeTranslationPair("Polish", "pl", true),
                MakeTranslationPair("Portuguese", "pt-pt", true),
                MakeTranslationPair("Portuguese (Brazil)", "pt-br", true),
                MakeTranslationPair("Romanian", "ro"),
                MakeTranslationPair("Russian", "ru", true),
                MakeTranslationPair("Slovak", "sk"),
                MakeTranslationPair("Slovenian", "sl"),
                MakeTranslationPair("Spanish", "es", true),
                MakeTranslationPair("Swedish", "sv"),
                MakeTranslationPair("Turkish", "tr"),
                MakeTranslationPair("Ukranian", "uk"),
            };
        }

        private static TranslationPair MakeTranslationPair(string name, string code)
        {
            return new TranslationPair(name, code, code);
        }

        private static TranslationPair MakeTranslationPair(string name, string code, bool hasFormality)
        {
            return new TranslationPair(name, code, hasFormality);
        }

        public Task<string> Translate(string text, string sourceLanguageCode, string targetLanguageCode, CancellationToken cancellationToken)
        {
            var postContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("text", text),
                new KeyValuePair<string, string>("target_lang", targetLanguageCode),
                new KeyValuePair<string, string>("source_lang", sourceLanguageCode),
                new KeyValuePair<string, string>("formality", _formality),
            });
            var result = _client.PostAsync("/v2/translate", postContent, cancellationToken).Result;
            var resultContent = result.Content.ReadAsStringAsync().Result;

            if (!result.IsSuccessStatusCode)
            {
                SeLogger.Error("DeepLTranslate error: " + resultContent);
            }

            if (result.StatusCode == HttpStatusCode.Forbidden)
            {
                Error = resultContent;
                throw new Exception("Forbidden! " + Environment.NewLine + Environment.NewLine + resultContent);
            }

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
    }
}
