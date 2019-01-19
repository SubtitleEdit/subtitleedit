using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.Translate
{
    /// <summary>
    /// Google translate via Google Cloud V2 API - see https://cloud.google.com/translate/
    /// </summary>
    public class GoogleTranslator2 : ITranslator
    {
        private readonly string _apiKey;

        public GoogleTranslator2(string apiKey)
        {
            _apiKey = apiKey;
        }

        public List<TranslationPair> GetTranslationPairs()
        {
            return new List<TranslationPair>
            {
                new TranslationPair("AFRIKAANS", "af"),
                new TranslationPair("ALBANIAN", "sq"),
                new TranslationPair("AMHARIC", "am"),
                new TranslationPair("ARABIC", "ar"),
                new TranslationPair("ARMENIAN", "hy"),
                new TranslationPair("AZERBAIJANI", "az"),
                new TranslationPair("BASQUE", "eu"),
                new TranslationPair("BELARUSIAN", "be"),
                new TranslationPair("BENGALI", "bn"),
                new TranslationPair("BOSNIAN", "bs"),
                new TranslationPair("BULGARIAN", "bg"),
                new TranslationPair("BURMESE", "my"),
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
                new TranslationPair("DUTCH", "nl"),
                new TranslationPair("ENGLISH", "en"),
                new TranslationPair("ESPERANTO", "eo"),
                new TranslationPair("ESTONIAN", "et"),
                new TranslationPair("FILIPINO", "tl"),
                new TranslationPair("FINNISH", "fi"),
                new TranslationPair("FRENCH", "fr"),
                new TranslationPair("FRISIAN", "fy"),
                new TranslationPair("GALICIAN", "gl"),
                new TranslationPair("GEORGIAN", "ka"),
                new TranslationPair("GERMAN", "de"),
                new TranslationPair("GREEK", "el"),
                new TranslationPair("GUJARATI", "gu"),
                new TranslationPair("HAITIAN CREOLE", "ht"),
                new TranslationPair("HAUSA", "ha"),
                new TranslationPair("HAWAIIAN", "haw"),
                new TranslationPair("HEBREW", "iw"),
                new TranslationPair("HINDI", "hi"),
                new TranslationPair("HMOUNG", "hmn"),
                new TranslationPair("HUNGARIAN", "hu"),
                new TranslationPair("ICELANDIC", "is"),
                new TranslationPair("IGBO", "ig"),
                new TranslationPair("INDONESIAN", "id"),
                new TranslationPair("IRISH", "ga"),
                new TranslationPair("ITALIAN", "it"),
                new TranslationPair("JAPANESE", "ja"),
                new TranslationPair("JAVANESE", "jw"),
                new TranslationPair("KANNADA", "kn"),
                new TranslationPair("KAZAKH", "kk"),
                new TranslationPair("KHMER", "km"),
                new TranslationPair("KOREAN", "ko"),
                new TranslationPair("KURDISH", "ku"),
                new TranslationPair("KYRGYZ", "ky"),
                new TranslationPair("LAO", "lo"),
                new TranslationPair("LATIN", "la"),
                new TranslationPair("LATVIAN", "lv"),
                new TranslationPair("LITHUANIAN", "lt"),
                new TranslationPair("LUXEMBOURGISH", "lb"),
                new TranslationPair("MACEDONIAN", "mk"),
                new TranslationPair("MALAY", "ms"),
                new TranslationPair("MALAGASY", "mg"),
                new TranslationPair("MALAYALAM", "ml"),
                new TranslationPair("MALTESE", "mt"),
                new TranslationPair("MAORI", "mi"),
                new TranslationPair("MARATHI", "mr"),
                new TranslationPair("MONGOLIAN", "mn"),
                new TranslationPair("MYANMAR", "my"),
                new TranslationPair("NEPALI", "ne"),
                new TranslationPair("NORWEGIAN", "no"),
                new TranslationPair("PASHTO", "ps"),
                new TranslationPair("PERSIAN", "fa"),
                new TranslationPair("POLISH", "pl"),
                new TranslationPair("PORTUGUESE", "pt"),
                new TranslationPair("PUNJABI", "pa"),
                new TranslationPair("ROMANIAN", "ro"),
                new TranslationPair("ROMANJI", "romanji"),
                new TranslationPair("RUSSIAN", "ru"),
                new TranslationPair("SAMOAN", "sm"),
                new TranslationPair("SCOTS GAELIC", "gd"),
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
                new TranslationPair("TELUGU", "te"),
                new TranslationPair("THAI", "th"),
                new TranslationPair("TURKISH", "tr"),
                new TranslationPair("UKRAINIAN", "uk"),
                new TranslationPair("URDU", "ur"),
                new TranslationPair("UZBEK", "uz"),
                new TranslationPair("VIETNAMESE", "vi"),
                new TranslationPair("WELSH", "cy"),
                new TranslationPair("XHOSA", "xh"),
                new TranslationPair("YIDDISH", "yi"),
                new TranslationPair("YORUBA", "yo"),
                new TranslationPair("ZULU", "zu"),
            };
        }

        public string GetName()
        {
            return "Google translate";
        }

        public string GetUrl()
        {
            return "https://translate.google.com/";
        }

        public List<string> Translate(string sourceLanguage, string targetLanguage, List<Paragraph> paragraphs, StringBuilder log)
        {
            var baseUrl = "https://translation.googleapis.com/language/translate/v2";
            var format = "text";
            var input = new StringBuilder();
            var formattings = new Formatting[paragraphs.Count];
            for (var index = 0; index < paragraphs.Count; index++)
            {
                var p = paragraphs[index];
                var f = new Formatting();
                formattings[index] = f;
                if (input.Length > 0)
                {
                    input.Append("&");
                }

                var text = f.SetTagsAndReturnTrimmed(TranslationHelper.PreTranslate(p.Text, sourceLanguage), sourceLanguage);
                input.Append("q=" + Utilities.UrlEncode(text));
            }

            string uri = $"{baseUrl}/?{input}&target={targetLanguage}&source={sourceLanguage}&format={format}&key={_apiKey}";

            var request = WebRequest.Create(uri);
            request.Proxy = Utilities.GetProxy();
            request.ContentType = "application/json";
            request.ContentLength = 0;
            request.Method = "POST";
            var response = request.GetResponse();
            var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            string content = reader.ReadToEnd();

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
                                            translatedText = Regex.Unescape(translatedText);
                                            translatedText = string.Join(Environment.NewLine, translatedText.SplitToLines());
                                            translatedText = TranslationHelper.PostTranslate(translatedText, targetLanguage);
                                            if (resultList.Count < formattings.Length)
                                            {
                                                translatedText = formattings[resultList.Count].ReAddFormatting(translatedText);
                                            }
                                            resultList.Add(translatedText);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return resultList;
        }
    }
}
