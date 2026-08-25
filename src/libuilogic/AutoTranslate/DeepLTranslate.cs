using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.UiLogic.Translate;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.UiLogic.Http;

namespace Nikse.SubtitleEdit.UiLogic.AutoTranslate
{
    /// <summary>
    /// DeepL Pro V2 translator - see https://www.deepl.com/api.html
    /// </summary>
    public class DeepLTranslate : IAutoTranslator, IDisposable
    {
        private string _apiKey = string.Empty;
        private string _apiUrl = string.Empty;
        private string _formality = string.Empty;
        private HttpClient _httpClient = null!;

        public static string StaticName { get; set; } = "DeepL V2 translate";
        public override string ToString() => StaticName;
        public string Name => StaticName;
        public string Url => "https://www.deepl.com";
        public string Error { get; set; } = string.Empty;
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

            _apiUrl = ResolveApiUrl(_apiUrl, _apiKey);

            _httpClient = HttpClientFactoryWithProxy.CreateHttpClientWithProxy();
            _httpClient.BaseAddress = new Uri(_apiUrl.Trim().TrimEnd('/'));
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "DeepL-Auth-Key " + _apiKey.Trim());
            _formality = string.IsNullOrWhiteSpace(_formality) ? "default" : _formality.Trim();
        }

        /// <summary>
        /// Pairs the API key with the host it belongs to. A key ending in ":fx" is a free-tier key
        /// and works only on api-free.deepl.com; a key without it works only on api.deepl.com -
        /// the other way round DeepL answers 403 "Wrong endpoint". Only the two official hosts are
        /// swapped, so a self-hosted or proxied URL is left exactly as the user entered it.
        /// </summary>
        public static string ResolveApiUrl(string apiUrl, string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiUrl) || string.IsNullOrWhiteSpace(apiKey))
            {
                return apiUrl;
            }

            const string freeHost = "api-free.deepl.com";
            const string proHost = "api.deepl.com";

            var isFreeKey = apiKey.Trim().EndsWith(":fx", StringComparison.OrdinalIgnoreCase);

            if (isFreeKey && apiUrl.Contains(proHost, StringComparison.OrdinalIgnoreCase))
            {
                return apiUrl.Replace(proHost, freeHost, StringComparison.OrdinalIgnoreCase);
            }

            if (!isFreeKey && apiUrl.Contains(freeHost, StringComparison.OrdinalIgnoreCase))
            {
                return apiUrl.Replace(freeHost, proHost, StringComparison.OrdinalIgnoreCase);
            }

            return apiUrl;
        }

        /// <summary>
        /// Mirrors GET /v2/languages?type=source (checked 2026-08-25, 101 languages). DeepL takes no
        /// regional variant as a source - the ones listed here are the pairs SE has always offered,
        /// and <see cref="MakeContent"/> cuts them back to the base code before sending.
        /// To refresh: curl -H "Authorization: DeepL-Auth-Key KEY" "https://api-free.deepl.com/v2/languages?type=source"
        /// </summary>
        public List<TranslationPair> GetSupportedSourceLanguages()
        {
            return new List<TranslationPair>
            {
                MakeTranslationPair("Afrikaans", "af", false),
                MakeTranslationPair("Albanian", "sq", false),
                MakeTranslationPair("Arabic", "ar", false),
                MakeTranslationPair("Aragonese", "an", false),
                MakeTranslationPair("Armenian", "hy", false),
                MakeTranslationPair("Assamese", "as", false),
                MakeTranslationPair("Aymara", "ay", false),
                MakeTranslationPair("Azerbaijani", "az", false),
                MakeTranslationPair("Bashkir", "ba", false),
                MakeTranslationPair("Basque", "eu", false),
                MakeTranslationPair("Belarusian", "be", false),
                MakeTranslationPair("Bengali", "bn", false),
                MakeTranslationPair("Bosnian", "bs", false),
                MakeTranslationPair("Breton", "br", false),
                MakeTranslationPair("Bulgarian", "bg", false),
                MakeTranslationPair("Burmese", "my", false),
                MakeTranslationPair("Catalan", "ca", false),
                MakeTranslationPair("Chinese (Simplified)", "zh-hans", false),
                MakeTranslationPair("Chinese (Traditional)", "zh-hant", false),
                MakeTranslationPair("Croatian", "hr", false),
                MakeTranslationPair("Czech", "cs", false),
                MakeTranslationPair("Danish", "da", false),
                MakeTranslationPair("Dutch", "nl", true),
                MakeTranslationPair("English (American)", "en-US", false),
                MakeTranslationPair("English (British)", "en-GB", false),
                MakeTranslationPair("Esperanto", "eo", false),
                MakeTranslationPair("Estonian", "et", false),
                MakeTranslationPair("Finnish", "fi", false),
                MakeTranslationPair("French", "fr", true),
                MakeTranslationPair("Galician", "gl", false),
                MakeTranslationPair("Georgian", "ka", false),
                MakeTranslationPair("German", "de", true),
                MakeTranslationPair("Greek", "el", false),
                MakeTranslationPair("Guarani", "gn", false),
                MakeTranslationPair("Gujarati", "gu", false),
                MakeTranslationPair("Haitian Creole", "ht", false),
                MakeTranslationPair("Hausa", "ha", false),
                MakeTranslationPair("Hebrew", "he", false),
                MakeTranslationPair("Hindi", "hi", false),
                MakeTranslationPair("Hungarian", "hu", false),
                MakeTranslationPair("Icelandic", "is", false),
                MakeTranslationPair("Igbo", "ig", false),
                MakeTranslationPair("Indonesian", "id", false),
                MakeTranslationPair("Irish", "ga", false),
                MakeTranslationPair("Italian", "it", true),
                MakeTranslationPair("Japanese", "ja", true),
                MakeTranslationPair("Javanese", "jv", false),
                MakeTranslationPair("Kazakh", "kk", false),
                MakeTranslationPair("Korean", "ko", false),
                MakeTranslationPair("Kyrgyz", "ky", false),
                MakeTranslationPair("Latin", "la", false),
                MakeTranslationPair("Latvian", "lv", false),
                MakeTranslationPair("Lingala", "ln", false),
                MakeTranslationPair("Lithuanian", "lt", false),
                MakeTranslationPair("Luxembourgish", "lb", false),
                MakeTranslationPair("Macedonian", "mk", false),
                MakeTranslationPair("Malagasy", "mg", false),
                MakeTranslationPair("Malay", "ms", false),
                MakeTranslationPair("Malayalam", "ml", false),
                MakeTranslationPair("Maltese", "mt", false),
                MakeTranslationPair("Maori", "mi", false),
                MakeTranslationPair("Marathi", "mr", false),
                MakeTranslationPair("Mongolian", "mn", false),
                MakeTranslationPair("Nepali", "ne", false),
                MakeTranslationPair("Norwegian (Bokmål)", "nb", false),
                MakeTranslationPair("Occitan", "oc", false),
                MakeTranslationPair("Oromo", "om", false),
                MakeTranslationPair("Pashto", "ps", false),
                MakeTranslationPair("Persian", "fa", false),
                MakeTranslationPair("Polish", "pl", true),
                MakeTranslationPair("Portuguese (Brazilian)", "pt-BR", true),
                MakeTranslationPair("Portuguese (European)", "pt-PT", true),
                MakeTranslationPair("Punjabi", "pa", false),
                MakeTranslationPair("Quechua", "qu", false),
                MakeTranslationPair("Romanian", "ro", false),
                MakeTranslationPair("Russian", "ru", true),
                MakeTranslationPair("Sanskrit", "sa", false),
                MakeTranslationPair("Serbian", "sr", false),
                MakeTranslationPair("Sesotho", "st", false),
                MakeTranslationPair("Slovak", "sk", false),
                MakeTranslationPair("Slovenian", "sl", false),
                MakeTranslationPair("Spanish", "es", true),
                MakeTranslationPair("Spanish (Latin American)", "es-419", true),
                MakeTranslationPair("Sundanese", "su", false),
                MakeTranslationPair("Swahili", "sw", false),
                MakeTranslationPair("Swedish", "sv", false),
                MakeTranslationPair("Tagalog", "tl", false),
                MakeTranslationPair("Tajik", "tg", false),
                MakeTranslationPair("Tamil", "ta", false),
                MakeTranslationPair("Tatar", "tt", false),
                MakeTranslationPair("Telugu", "te", false),
                MakeTranslationPair("Thai", "th", false),
                MakeTranslationPair("Tsonga", "ts", false),
                MakeTranslationPair("Tswana", "tn", false),
                MakeTranslationPair("Turkish", "tr", false),
                MakeTranslationPair("Turkmen", "tk", false),
                MakeTranslationPair("Ukrainian", "uk", false),
                MakeTranslationPair("Urdu", "ur", false),
                MakeTranslationPair("Uzbek", "uz", false),
                MakeTranslationPair("Vietnamese", "vi", false),
                MakeTranslationPair("Welsh", "cy", false),
                MakeTranslationPair("Wolof", "wo", false),
                MakeTranslationPair("Xhosa", "xh", false),
                MakeTranslationPair("Yiddish", "yi", false),
                MakeTranslationPair("Zulu", "zu", false),
            };
        }

        /// <summary>
        /// Mirrors GET /v2/languages?type=target (checked 2026-08-25, 110 languages), minus the three
        /// codes that are aliases of an entry already here: ZH (= zh-hans), DE-DE (= de) and FR-FR
        /// (= fr). The last argument is the API's supports_formality flag.
        /// To refresh: curl -H "Authorization: DeepL-Auth-Key KEY" "https://api-free.deepl.com/v2/languages?type=target"
        /// </summary>
        public List<TranslationPair> GetSupportedTargetLanguages()
        {
            return new List<TranslationPair>
            {
                MakeTranslationPair("Afrikaans", "af", false),
                MakeTranslationPair("Albanian", "sq", false),
                MakeTranslationPair("Arabic", "ar", false),
                MakeTranslationPair("Aragonese", "an", false),
                MakeTranslationPair("Armenian", "hy", false),
                MakeTranslationPair("Assamese", "as", false),
                MakeTranslationPair("Aymara", "ay", false),
                MakeTranslationPair("Azerbaijani", "az", false),
                MakeTranslationPair("Bashkir", "ba", false),
                MakeTranslationPair("Basque", "eu", false),
                MakeTranslationPair("Belarusian", "be", false),
                MakeTranslationPair("Bengali", "bn", false),
                MakeTranslationPair("Bosnian", "bs", false),
                MakeTranslationPair("Breton", "br", false),
                MakeTranslationPair("Bulgarian", "bg", false),
                MakeTranslationPair("Burmese", "my", false),
                MakeTranslationPair("Catalan", "ca", false),
                MakeTranslationPair("Chinese (Simplified)", "zh-hans", false),
                MakeTranslationPair("Chinese (Traditional)", "zh-hant", false),
                MakeTranslationPair("Croatian", "hr", false),
                MakeTranslationPair("Czech", "cs", false),
                MakeTranslationPair("Danish", "da", false),
                MakeTranslationPair("Dutch", "nl", true),
                MakeTranslationPair("English (American)", "en-US", false),
                MakeTranslationPair("English (British)", "en-GB", false),
                MakeTranslationPair("Esperanto", "eo", false),
                MakeTranslationPair("Estonian", "et", false),
                MakeTranslationPair("Finnish", "fi", false),
                MakeTranslationPair("French", "fr", true),
                MakeTranslationPair("French (Canadian)", "fr-CA", true),
                MakeTranslationPair("Galician", "gl", false),
                MakeTranslationPair("Georgian", "ka", false),
                MakeTranslationPair("German", "de", true),
                MakeTranslationPair("German (Swiss)", "de-CH", true),
                MakeTranslationPair("Greek", "el", false),
                MakeTranslationPair("Guarani", "gn", false),
                MakeTranslationPair("Gujarati", "gu", false),
                MakeTranslationPair("Haitian Creole", "ht", false),
                MakeTranslationPair("Hausa", "ha", false),
                MakeTranslationPair("Hebrew", "he", false),
                MakeTranslationPair("Hindi", "hi", false),
                MakeTranslationPair("Hungarian", "hu", false),
                MakeTranslationPair("Icelandic", "is", false),
                MakeTranslationPair("Igbo", "ig", false),
                MakeTranslationPair("Indonesian", "id", false),
                MakeTranslationPair("Irish", "ga", false),
                MakeTranslationPair("Italian", "it", true),
                MakeTranslationPair("Japanese", "ja", true),
                MakeTranslationPair("Javanese", "jv", false),
                MakeTranslationPair("Kazakh", "kk", false),
                MakeTranslationPair("Korean", "ko", false),
                MakeTranslationPair("Kyrgyz", "ky", false),
                MakeTranslationPair("Latin", "la", false),
                MakeTranslationPair("Latvian", "lv", false),
                MakeTranslationPair("Lingala", "ln", false),
                MakeTranslationPair("Lithuanian", "lt", false),
                MakeTranslationPair("Luxembourgish", "lb", false),
                MakeTranslationPair("Macedonian", "mk", false),
                MakeTranslationPair("Malagasy", "mg", false),
                MakeTranslationPair("Malay", "ms", false),
                MakeTranslationPair("Malayalam", "ml", false),
                MakeTranslationPair("Maltese", "mt", false),
                MakeTranslationPair("Maori", "mi", false),
                MakeTranslationPair("Marathi", "mr", false),
                MakeTranslationPair("Mongolian", "mn", false),
                MakeTranslationPair("Nepali", "ne", false),
                MakeTranslationPair("Norwegian (Bokmål)", "nb", false),
                MakeTranslationPair("Occitan", "oc", false),
                MakeTranslationPair("Oromo", "om", false),
                MakeTranslationPair("Pashto", "ps", false),
                MakeTranslationPair("Persian", "fa", false),
                MakeTranslationPair("Polish", "pl", true),
                MakeTranslationPair("Portuguese (Brazilian)", "pt-BR", true),
                MakeTranslationPair("Portuguese (European)", "pt-PT", true),
                MakeTranslationPair("Punjabi", "pa", false),
                MakeTranslationPair("Quechua", "qu", false),
                MakeTranslationPair("Romanian", "ro", false),
                MakeTranslationPair("Russian", "ru", true),
                MakeTranslationPair("Sanskrit", "sa", false),
                MakeTranslationPair("Serbian", "sr", false),
                MakeTranslationPair("Sesotho", "st", false),
                MakeTranslationPair("Slovak", "sk", false),
                MakeTranslationPair("Slovenian", "sl", false),
                MakeTranslationPair("Spanish", "es", true),
                MakeTranslationPair("Spanish (Latin American)", "es-419", true),
                MakeTranslationPair("Sundanese", "su", false),
                MakeTranslationPair("Swahili", "sw", false),
                MakeTranslationPair("Swedish", "sv", false),
                MakeTranslationPair("Tagalog", "tl", false),
                MakeTranslationPair("Tajik", "tg", false),
                MakeTranslationPair("Tamil", "ta", false),
                MakeTranslationPair("Tatar", "tt", false),
                MakeTranslationPair("Telugu", "te", false),
                MakeTranslationPair("Thai", "th", false),
                MakeTranslationPair("Tsonga", "ts", false),
                MakeTranslationPair("Tswana", "tn", false),
                MakeTranslationPair("Turkish", "tr", false),
                MakeTranslationPair("Turkmen", "tk", false),
                MakeTranslationPair("Ukrainian", "uk", false),
                MakeTranslationPair("Urdu", "ur", false),
                MakeTranslationPair("Uzbek", "uz", false),
                MakeTranslationPair("Vietnamese", "vi", false),
                MakeTranslationPair("Welsh", "cy", false),
                MakeTranslationPair("Wolof", "wo", false),
                MakeTranslationPair("Xhosa", "xh", false),
                MakeTranslationPair("Yiddish", "yi", false),
                MakeTranslationPair("Zulu", "zu", false),
            };
        }

        private static TranslationPair MakeTranslationPair(string name, string code, bool hasFormality)
        {
            return new TranslationPair(name, code, hasFormality);
        }

        public async Task<string> Translate(string text, string sourceLanguageCode, string targetLanguageCode, CancellationToken cancellationToken)
        {
            if (_httpClient == null)
            {
                // Initialize() bails out without creating the client when the API key or URL is
                // empty - fail with a readable message instead of a NullReferenceException.
                Error = $"{StaticName} requires an API key";
                throw new Exception($"{StaticName} requires an API key - please enter your DeepL API key and try again.");
            }

            int[] retryDelays = { 555, 3007, 7013 };
            HttpResponseMessage result = null!;
            var resultContent = string.Empty;
            for (var attempt = 0; attempt <= retryDelays.Length; attempt++)
            {
                var postContent = MakeContent(text, sourceLanguageCode, targetLanguageCode);
                result = await _httpClient.PostAsync("/v2/translate", postContent, cancellationToken);
                resultContent = await result.Content.ReadAsStringAsync(cancellationToken);

                if (!ShouldRetry(result, resultContent) || attempt == retryDelays.Length)
                {
                    break;
                }

                await Task.Delay(retryDelays[attempt], cancellationToken);
            }

            if (result.StatusCode == HttpStatusCode.Forbidden)
            {
                Error = resultContent;
                throw new Exception("Forbidden! " + Environment.NewLine + Environment.NewLine + resultContent);
            }

            if (!result.IsSuccessStatusCode)
            {
                Error = resultContent;
                SeLogger.Error("DeepLTranslate error: " + resultContent);
                throw new Exception($"DeepL failed with status code {(int)result.StatusCode} ({result.StatusCode})" + Environment.NewLine + Environment.NewLine + resultContent);
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
                                        var s = innerDic[transItem]?.ToString();
                                        resultList.Add(s ?? string.Empty);
                                    }
                                }
                            }
                        }
                    }
                }

                return string.Join(Environment.NewLine, resultList);
            }
            catch (Exception ex)
            {
                SeLogger.Error(ex, "DeepLTranslate.Translate: " + ex.Message + Environment.NewLine + resultContent);
                throw;
            }
        }

        public static bool ShouldRetry(HttpResponseMessage result, string resultContent)
        {
            const int httpStatusCodeTooManyRequests = 429;

            return result.StatusCode == HttpStatusCode.ServiceUnavailable ||
                   (int)result.StatusCode == httpStatusCodeTooManyRequests ||
                   (result != null && resultContent.Contains("<head><title>429 Too Many Requests</title></head>", StringComparison.Ordinal));
        }

        private FormUrlEncodedContent MakeContent(string text, string sourceLanguageCode, string targetLanguageCode)
        {
            var sourceLang = sourceLanguageCode.Contains("-")
                ? sourceLanguageCode.Substring(0, sourceLanguageCode.IndexOf('-'))
                : sourceLanguageCode;

            var array = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("text", text),
                new KeyValuePair<string, string>("target_lang", targetLanguageCode),
                new KeyValuePair<string, string>("source_lang", sourceLang),
            };

            var targetLanguages = GetSupportedTargetLanguages();
            var hasFormality = targetLanguages.Find(x => x.Code.Equals(targetLanguageCode, StringComparison.OrdinalIgnoreCase))?.HasFormality ?? false;
            if (hasFormality && !string.IsNullOrEmpty(_formality))
            {
                array.Add(new KeyValuePair<string, string>("formality", _formality));
            }

            return new FormUrlEncodedContent(array);
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
