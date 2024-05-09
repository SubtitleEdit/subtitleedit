using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Translate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Core.AutoTranslate
{
    /// <summary>
    /// Google translate via Google V1 API - see https://cloud.google.com/translate/
    /// </summary>
    public class GoogleTranslateV1 : IAutoTranslator
    {
        private HttpClient _httpClient;

        public static string StaticName { get; set; } = "Google Translate V1 API";
        public string Name => StaticName;
        public string Url => "https://translate.google.com/";
        public string Error { get; set; }
        public int MaxCharacters => 1500;

        public void Initialize()
        {
            _httpClient?.Dispose();
            _httpClient = new HttpClient(); //DownloaderFactory.MakeHttpClient();
            _httpClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=UTF-8");
            _httpClient.BaseAddress = new Uri("https://translate.googleapis.com/");
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
            string jsonResultString;

            try
            {
                var url = $"translate_a/single?client=gtx&sl={sourceLanguageCode}&tl={targetLanguageCode}&dt=t&q={Utilities.UrlEncode(text)}";

                var result = _httpClient.GetAsync(url).Result;
                var bytes = result.Content.ReadAsByteArrayAsync().Result;
                jsonResultString = Encoding.UTF8.GetString(bytes).Trim();

                if (!result.IsSuccessStatusCode)
                {
                    Error = jsonResultString;
                    SeLogger.Error($"Error in {StaticName}.Translate: " + Error);
                }
            }
            catch (WebException webException)
            {
                throw new Exception("Free API quota exceeded?", webException);
            }

            var resultList = ConvertJsonObjectToStringLines(jsonResultString);
            return Task.FromResult(string.Join(Environment.NewLine, resultList));
        }

        public static List<TranslationPair> GetTranslationPairs()
        {
            return new List<TranslationPair>
            {
                new TranslationPair("AFRIKAANS", "af"),
                new TranslationPair("ALBANIAN", "sq"),
                new TranslationPair("AMHARIC", "am"),
                new TranslationPair("ARABIC", "ar"),
                new TranslationPair("ARMENIAN", "hy"),
                new TranslationPair("ASSAMESE", "as"),
                new TranslationPair("AYMARA", "ay"),
                new TranslationPair("AZERBAIJANI", "az"),
                new TranslationPair("BAMBARA", "bm"),
                new TranslationPair("BASQUE", "eu"),
                new TranslationPair("BELARUSIAN", "be"),
                new TranslationPair("BENGALI", "bn"),
                new TranslationPair("BHOJPURI", "bho"),
                new TranslationPair("BOSNIAN", "bs"),
                new TranslationPair("BULGARIAN", "bg"),
                new TranslationPair("CATALAN", "ca"),
                new TranslationPair("CEBUANO", "ceb"),
                new TranslationPair("CHICHEWA", "ny"),
                new TranslationPair("CHINESE", "zh"),
                new TranslationPair("CHINESE_SIMPLIFIED", "zh-CN"),
                new TranslationPair("CHINESE_TRADITIONAL", "zh-TW"),
                new TranslationPair("CORSICAN", "co"),
                new TranslationPair("CROATIAN", "hr"),
                new TranslationPair("CZECH", "cs"),
                new TranslationPair("DANISH", "da"),
                new TranslationPair("DHIVEHI", "dv"),
                new TranslationPair("DOGRI", "doi"),
                new TranslationPair("DUTCH", "nl"),
                new TranslationPair("ENGLISH", "en"),
                new TranslationPair("ESPERANTO", "eo"),
                new TranslationPair("ESTONIAN", "et"),
                new TranslationPair("EWE", "ee"),
                new TranslationPair("FILIPINO", "tl"),
                new TranslationPair("FINNISH", "fi"),
                new TranslationPair("FRENCH", "fr"),
                new TranslationPair("FRISIAN", "fy"),
                new TranslationPair("GALICIAN", "gl"),
                new TranslationPair("GEORGIAN", "ka"),
                new TranslationPair("GERMAN", "de"),
                new TranslationPair("GREEK", "el"),
                new TranslationPair("GUARANI", "gn"),
                new TranslationPair("GUJARATI", "gu"),
                new TranslationPair("HAITIAN CREOLE", "ht"),
                new TranslationPair("HAUSA", "ha"),
                new TranslationPair("HAWAIIAN", "haw"),
                new TranslationPair("HEBREW", "he"),
                new TranslationPair("HINDI", "hi"),
                new TranslationPair("HMOUNG", "hmn"),
                new TranslationPair("HUNGARIAN", "hu"),
                new TranslationPair("ICELANDIC", "is"),
                new TranslationPair("IGBO", "ig"),
                new TranslationPair("ILOCANO", "ilo"),
                new TranslationPair("INDONESIAN", "id"),
                new TranslationPair("IRISH", "ga"),
                new TranslationPair("ITALIAN", "it"),
                new TranslationPair("JAPANESE", "ja"),
                new TranslationPair("JAVANESE", "jw"),
                new TranslationPair("KANNADA", "kn"),
                new TranslationPair("KAZAKH", "kk"),
                new TranslationPair("KHMER", "km"),
                new TranslationPair("KINYARWANDA", "rw"),
                new TranslationPair("KONKANI", "gom"),
                new TranslationPair("KOREAN", "ko"),
                new TranslationPair("KRIO", "kri"),
                new TranslationPair("KURDISH", "ku"),
                new TranslationPair("KURDISH (SORANI)", "ckb"),
                new TranslationPair("KYRGYZ", "ky"),
                new TranslationPair("LAO", "lo"),
                new TranslationPair("LATIN", "la"),
                new TranslationPair("LATVIAN", "lv"),
                new TranslationPair("LINGALA", "ln"),
                new TranslationPair("LITHUANIAN", "lt"),
                new TranslationPair("LUGANDA", "lg"),
                new TranslationPair("LUXEMBOURGISH", "lb"),
                new TranslationPair("MACEDONIAN", "mk"),
                new TranslationPair("MAITILI", "mai"),
                new TranslationPair("MALAGASY", "mg"),
                new TranslationPair("MALAY", "ms"),
                new TranslationPair("MALAYALAM", "ml"),
                new TranslationPair("MALTESE", "mt"),
                new TranslationPair("MAORI", "mi"),
                new TranslationPair("MARATHI", "mr"),
                new TranslationPair("MEITEILON (MANIPURI)", "mni"),
                new TranslationPair("MIZO", "lus"),
                new TranslationPair("MONGOLIAN", "mn"),
                new TranslationPair("MYANMAR", "my"),
                new TranslationPair("NEPALI", "ne"),
                new TranslationPair("NORWEGIAN", "no"),
                new TranslationPair("ODIA", "or"),
                new TranslationPair("OROMO", "om"),
                new TranslationPair("PASHTO", "ps"),
                new TranslationPair("PERSIAN", "fa"),
                new TranslationPair("POLISH", "pl"),
                new TranslationPair("PORTUGUESE", "pt"),
                new TranslationPair("PUNJABI", "pa"),
                new TranslationPair("QUECHUABI", "qu"),
                new TranslationPair("ROMANIAN", "ro"),
//                new TranslationPair("ROMANJI", "romanji"),
                new TranslationPair("RUSSIAN", "ru"),
                new TranslationPair("SAMOAN", "sm"),
                new TranslationPair("SANSKRIT", "sa"),
                new TranslationPair("SCOTS GAELIC", "gd"),
                new TranslationPair("SEPEDI", "nso"),
                new TranslationPair("SERBIAN", "sr"),
                new TranslationPair("SESOTHO", "st"),
                new TranslationPair("SHONA", "sn"),
                new TranslationPair("SINDHI", "sd"),
                new TranslationPair("SINHALA", "si"),
                new TranslationPair("SLOVAK", "sk"),
                new TranslationPair("SLOVENIAN", "sl"),
                new TranslationPair("SOMALI", "so"),
                new TranslationPair("SPANISH", "es"),
                new TranslationPair("SUNDANESE", "su"),
                new TranslationPair("SWAHILI", "sw"),
                new TranslationPair("SWEDISH", "sv"),
                new TranslationPair("TAJIK", "tg"),
                new TranslationPair("TAMIL", "ta"),
                new TranslationPair("TATAR", "tt"),
                new TranslationPair("TELUGU", "te"),
                new TranslationPair("THAI", "th"),
                new TranslationPair("TIGRINYA", "ti"),
                new TranslationPair("TSONGA", "ts"),
                new TranslationPair("TURKISH", "tr"),
                new TranslationPair("TWI", "ak"),
                new TranslationPair("TURKMEN", "tk"),
                new TranslationPair("UKRAINIAN", "uk"),
                new TranslationPair("URDU", "ur"),
                new TranslationPair("UYGHUR", "ug"),
                new TranslationPair("UZBEK", "uz"),
                new TranslationPair("VIETNAMESE", "vi"),
                new TranslationPair("WELSH", "cy"),
                new TranslationPair("XHOSA", "xh"),
                new TranslationPair("YIDDISH", "yi"),
                new TranslationPair("YORUBA", "yo"),
                new TranslationPair("ZULU", "zu"),
            };
        }

        private static List<string> ConvertJsonObjectToStringLines(string result)
        {
            var sbAll = new StringBuilder();
            var count = 0;
            var i = 1;
            var level = result.StartsWith('[') ? 1 : 0;
            while (i < result.Length - 1)
            {
                var sb = new StringBuilder();
                var start = false;
                for (; i < result.Length - 1; i++)
                {
                    var c = result[i];
                    if (start)
                    {
                        if (c == '\\' && result[i + 1] == '\\')
                        {
                            i++;
                        }
                        else if (c == '\\' && result[i + 1] == '"')
                        {
                            c = '"';
                            i++;
                        }
                        else if (c == '"')
                        {
                            count++;
                            if (count % 2 == 1 && level > 2 && level < 5) // even numbers are original text, level 3 is translation
                            {
                                sbAll.Append(" " + sb);
                            }

                            i++;
                            break;
                        }

                        sb.Append(c);
                    }
                    else if (c == '"')
                    {
                        start = true;
                    }
                    else if (c == '[')
                    {
                        level++;
                    }
                    else if (c == ']')
                    {
                        level--;
                    }
                }
            }

            var res = sbAll.ToString().Trim();
            try
            {
                res = Regex.Unescape(res);
            }
            catch
            {
                res = res.Replace("\\n", "\n");
            }

            var lines = res.SplitToLines().ToList();
            return lines;
        }
    }
}
