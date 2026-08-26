using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.UiLogic.Http;
using Nikse.SubtitleEdit.UiLogic.Translate;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.UiLogic.AutoTranslate
{
    /// <summary>
    /// Google translate via Google Cloud V2 API - see https://cloud.google.com/translate/
    /// </summary>
    public class GoogleTranslateV2 : IAutoTranslator, IDisposable
    {
        private string _apiKey = string.Empty;
        private HttpClient _httpClient = null!;

        public static string StaticName { get; set; } = "Google Translate V2 API";
        public override string ToString() => StaticName;
        public string Name => StaticName;
        public string Url => "https://translate.google.com/";
        public string Error { get; set; } = string.Empty;
        public int MaxCharacters => 1500;

        public void Initialize()
        {
            _apiKey = Configuration.Settings.Tools.GoogleApiV2Key;
            _httpClient = HttpClientFactoryWithProxy.CreateHttpClientWithProxy();
            _httpClient.BaseAddress = new Uri("https://translation.googleapis.com/language/translate/v2/");
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public List<TranslationPair> GetSupportedSourceLanguages()
        {
            return GetTranslationPairs();
        }

        public List<TranslationPair> GetSupportedTargetLanguages()
        {
            return GetTranslationPairs();
        }

        /// <summary>
        /// The languages Google documents for Cloud Translation
        /// (https://cloud.google.com/translate/docs/languages, checked 2026-08-25). This used to
        /// borrow the free endpoint's list, which speaks about 50 languages the paid API does not.
        /// Every code SE has offered here is kept - only "mni" (rejected with 400; the code is
        /// "mni-Mtei") and "romanji" (not a language code - it answers with the untranslated
        /// source text) are gone.
        /// </summary>
        private static List<TranslationPair> GetTranslationPairs()
        {
            return new List<TranslationPair>
            {
                new TranslationPair("ABKHAZ", "ab"),
                new TranslationPair("ACEHNESE", "ace"),
                new TranslationPair("ACHOLI", "ach"),
                new TranslationPair("AFAR", "aa"),
                new TranslationPair("AFRIKAANS", "af"),
                new TranslationPair("ALBANIAN", "sq"),
                new TranslationPair("ALUR", "alz"),
                new TranslationPair("AMHARIC", "am"),
                new TranslationPair("ARABIC", "ar"),
                new TranslationPair("ARMENIAN", "hy"),
                new TranslationPair("ASSAMESE", "as"),
                new TranslationPair("AWADHI", "awa"),
                new TranslationPair("AYMARA", "ay"),
                new TranslationPair("AZERBAIJANI", "az"),
                new TranslationPair("BALINESE", "ban"),
                new TranslationPair("BAMBARA", "bm"),
                new TranslationPair("BASHKIR", "ba"),
                new TranslationPair("BASQUE", "eu"),
                new TranslationPair("BATAK_KARO", "btx"),
                new TranslationPair("BATAK_SIMALUNGUN", "bts"),
                new TranslationPair("BATAK_TOBA", "bbc"),
                new TranslationPair("BELARUSIAN", "be"),
                new TranslationPair("BEMBA", "bem"),
                new TranslationPair("BENGALI", "bn"),
                new TranslationPair("BETAWI", "bew"),
                new TranslationPair("BHOJPURI", "bho"),
                new TranslationPair("BIKOL", "bik"),
                new TranslationPair("BOSNIAN", "bs"),
                new TranslationPair("BRETON", "br"),
                new TranslationPair("BULGARIAN", "bg"),
                new TranslationPair("BURYAT", "bua"),
                new TranslationPair("CANTONESE", "yue"),
                new TranslationPair("CATALAN", "ca"),
                new TranslationPair("CEBUANO", "ceb"),
                new TranslationPair("CHICHEWA", "ny"),
                new TranslationPair("CHINESE", "zh"),
                new TranslationPair("CHINESE_SIMPLIFIED", "zh-CN"),
                new TranslationPair("CHINESE_TRADITIONAL", "zh-TW"),
                new TranslationPair("CHUVASH", "cv"),
                new TranslationPair("CORSICAN", "co"),
                new TranslationPair("CRIMEAN_TATAR_(CYRILLIC)", "crh"),
                new TranslationPair("CROATIAN", "hr"),
                new TranslationPair("CZECH", "cs"),
                new TranslationPair("DANISH", "da"),
                new TranslationPair("DHIVEHI", "dv"),
                new TranslationPair("DINKA", "din"),
                new TranslationPair("DOGRI", "doi"),
                new TranslationPair("DOMBE", "dov"),
                new TranslationPair("DUTCH", "nl"),
                new TranslationPair("DZONGKHA", "dz"),
                new TranslationPair("ENGLISH", "en"),
                new TranslationPair("ESPERANTO", "eo"),
                new TranslationPair("ESTONIAN", "et"),
                new TranslationPair("EWE", "ee"),
                new TranslationPair("FIJIAN", "fj"),
                new TranslationPair("FILIPINO", "tl"),
                new TranslationPair("FINNISH", "fi"),
                new TranslationPair("FRENCH", "fr"),
                new TranslationPair("FRENCH_(CANADA)", "fr-CA"),
                new TranslationPair("FRISIAN", "fy"),
                new TranslationPair("FULANI", "ff"),
                new TranslationPair("GA", "gaa"),
                new TranslationPair("GALICIAN", "gl"),
                new TranslationPair("GEORGIAN", "ka"),
                new TranslationPair("GERMAN", "de"),
                new TranslationPair("GREEK", "el"),
                new TranslationPair("GUARANI", "gn"),
                new TranslationPair("GUJARATI", "gu"),
                new TranslationPair("HAITIAN CREOLE", "ht"),
                new TranslationPair("HAKHA_CHIN", "cnh"),
                new TranslationPair("HAUSA", "ha"),
                new TranslationPair("HAWAIIAN", "haw"),
                new TranslationPair("HEBREW", "he"),
                new TranslationPair("HILIGAYNON", "hil"),
                new TranslationPair("HINDI", "hi"),
                new TranslationPair("HMOUNG", "hmn"),
                new TranslationPair("HUNGARIAN", "hu"),
                new TranslationPair("HUNSRIK", "hrx"),
                new TranslationPair("ICELANDIC", "is"),
                new TranslationPair("IGBO", "ig"),
                new TranslationPair("ILOCANO", "ilo"),
                new TranslationPair("INDONESIAN", "id"),
                new TranslationPair("IRISH", "ga"),
                new TranslationPair("ITALIAN", "it"),
                new TranslationPair("JAPANESE", "ja"),
                new TranslationPair("JAVANESE", "jw"),
                new TranslationPair("KANNADA", "kn"),
                new TranslationPair("KAPAMPANGAN", "pam"),
                new TranslationPair("KAZAKH", "kk"),
                new TranslationPair("KHMER", "km"),
                new TranslationPair("KIGA", "cgg"),
                new TranslationPair("KINYARWANDA", "rw"),
                new TranslationPair("KITUBA", "ktu"),
                new TranslationPair("KONKANI", "gom"),
                new TranslationPair("KOREAN", "ko"),
                new TranslationPair("KRIO", "kri"),
                new TranslationPair("KURDISH", "ku"),
                new TranslationPair("KURDISH (SORANI)", "ckb"),
                new TranslationPair("KYRGYZ", "ky"),
                new TranslationPair("LAO", "lo"),
                new TranslationPair("LATGALIAN", "ltg"),
                new TranslationPair("LATIN", "la"),
                new TranslationPair("LATVIAN", "lv"),
                new TranslationPair("LIGURIAN", "lij"),
                new TranslationPair("LIMBURGISH", "li"),
                new TranslationPair("LINGALA", "ln"),
                new TranslationPair("LITHUANIAN", "lt"),
                new TranslationPair("LOMBARD", "lmo"),
                new TranslationPair("LUGANDA", "lg"),
                new TranslationPair("LUO", "luo"),
                new TranslationPair("LUXEMBOURGISH", "lb"),
                new TranslationPair("MACEDONIAN", "mk"),
                new TranslationPair("MAITILI", "mai"),
                new TranslationPair("MAKASSAR", "mak"),
                new TranslationPair("MALAGASY", "mg"),
                new TranslationPair("MALAY", "ms"),
                new TranslationPair("MALAYALAM", "ml"),
                new TranslationPair("MALAY_(JAWI)", "ms-Arab"),
                new TranslationPair("MALTESE", "mt"),
                new TranslationPair("MANX", "gv"),
                new TranslationPair("MAORI", "mi"),
                new TranslationPair("MARATHI", "mr"),
                new TranslationPair("MEADOW_MARI", "chm"),
                new TranslationPair("MEITEILON (MANIPURI)", "mni-Mtei"),
                new TranslationPair("MINANG", "min"),
                new TranslationPair("MIZO", "lus"),
                new TranslationPair("MONGOLIAN", "mn"),
                new TranslationPair("MYANMAR", "my"),
                new TranslationPair("NDEBELE_(SOUTH)", "nr"),
                new TranslationPair("NEPALBHASA_(NEWARI)", "new"),
                new TranslationPair("NEPALI", "ne"),
                new TranslationPair("NKO", "bm-Nkoo"),
                new TranslationPair("NORWEGIAN", "no"),
                new TranslationPair("NUER", "nus"),
                new TranslationPair("OCCITAN", "oc"),
                new TranslationPair("ODIA", "or"),
                new TranslationPair("OROMO", "om"),
                new TranslationPair("PANGASINAN", "pag"),
                new TranslationPair("PAPIAMENTO", "pap"),
                new TranslationPair("PASHTO", "ps"),
                new TranslationPair("PERSIAN", "fa"),
                new TranslationPair("POLISH", "pl"),
                new TranslationPair("PORTUGUESE", "pt-PT"),
                new TranslationPair("PORTUGUESE (BRAZIL)", "pt"),
                new TranslationPair("PUNJABI", "pa"),
                new TranslationPair("PUNJABI (Shahmukhi)", "pa-Arab"),
                new TranslationPair("QUECHUABI", "qu"),
                new TranslationPair("ROMANI", "rom"),
                new TranslationPair("ROMANIAN", "ro"),
                new TranslationPair("RUNDI", "rn"),
                new TranslationPair("RUSSIAN", "ru"),
                new TranslationPair("SAMOAN", "sm"),
                new TranslationPair("SANGO", "sg"),
                new TranslationPair("SANSKRIT", "sa"),
                new TranslationPair("SCOTS GAELIC", "gd"),
                new TranslationPair("SEPEDI", "nso"),
                new TranslationPair("SERBIAN", "sr"),
                new TranslationPair("SESOTHO", "st"),
                new TranslationPair("SEYCHELLOIS_CREOLE", "crs"),
                new TranslationPair("SHAN", "shn"),
                new TranslationPair("SHONA", "sn"),
                new TranslationPair("SICILIAN", "scn"),
                new TranslationPair("SILESIAN", "szl"),
                new TranslationPair("SINDHI", "sd"),
                new TranslationPair("SINHALA", "si"),
                new TranslationPair("SLOVAK", "sk"),
                new TranslationPair("SLOVENIAN", "sl"),
                new TranslationPair("SOMALI", "so"),
                new TranslationPair("SPANISH", "es"),
                new TranslationPair("SUNDANESE", "su"),
                new TranslationPair("SWAHILI", "sw"),
                new TranslationPair("SWATI", "ss"),
                new TranslationPair("SWEDISH", "sv"),
                new TranslationPair("TAJIK", "tg"),
                new TranslationPair("TAMAZIGHT", "ber"),
                new TranslationPair("TAMIL", "ta"),
                new TranslationPair("TATAR", "tt"),
                new TranslationPair("TELUGU", "te"),
                new TranslationPair("TETUM", "tet"),
                new TranslationPair("THAI", "th"),
                new TranslationPair("TIGRINYA", "ti"),
                new TranslationPair("TOK PISIN", "tpi"),
                new TranslationPair("TSONGA", "ts"),
                new TranslationPair("TSWANA", "tn"),
                new TranslationPair("TURKISH", "tr"),
                new TranslationPair("TURKMEN", "tk"),
                new TranslationPair("TWI", "ak"),
                new TranslationPair("UKRAINIAN", "uk"),
                new TranslationPair("URDU", "ur"),
                new TranslationPair("UYGHUR", "ug"),
                new TranslationPair("UZBEK", "uz"),
                new TranslationPair("VIETNAMESE", "vi"),
                new TranslationPair("WELSH", "cy"),
                new TranslationPair("XHOSA", "xh"),
                new TranslationPair("YIDDISH", "yi"),
                new TranslationPair("YORUBA", "yo"),
                new TranslationPair("YUCATEC_MAYA", "yua"),
                new TranslationPair("ZULU", "zu"),
            };
        }

        public async Task<string> Translate(string text, string sourceLanguageCode, string targetLanguageCode, CancellationToken cancellationToken)
        {
            var format = "text";
            var input = new StringBuilder();
            input.Append("q=" + Utilities.UrlEncode(text));
            var uri = $"?{input}&target={targetLanguageCode}&source={sourceLanguageCode}&format={format}&key={_apiKey}";
            string content;
            try
            {
                var result = await _httpClient.PostAsync(uri, new StringContent(string.Empty), cancellationToken);

                if (!result.IsSuccessStatusCode)
                {
                    try
                    {
                        Error = await result.Content.ReadAsStringAsync(cancellationToken);
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

                content = await result.Content.ReadAsStringAsync(cancellationToken);
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
            return string.Join(Environment.NewLine, resultList);
        }

        public void Dispose() => _httpClient?.Dispose();
    }
}
