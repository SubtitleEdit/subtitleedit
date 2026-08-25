using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.UiLogic.Translate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.UiLogic.Http;

namespace Nikse.SubtitleEdit.UiLogic.AutoTranslate
{
    /// <summary>
    /// Google translate via Google V1 API - see https://cloud.google.com/translate/
    /// </summary>
    public class GoogleTranslateV1 : IAutoTranslator, IDisposable
    {
        private HttpClient _httpClient = null!;
        private bool _paceRequests;
        private DateTime _lastRequestTime = DateTime.MinValue;

        // Once Google has answered a non-success status this run, request bursts are what
        // keep feeding the "unusual traffic" scoring (issue #14015) - space the remaining
        // requests out instead of hammering on. Successful runs pay no delay at all.
        private const int PacedRequestIntervalMs = 500;

        public static string StaticName { get; set; } = "Google Translate V1 API";
        public override string ToString() => StaticName;
        public string Name => StaticName;
        public string Url => "https://translate.google.com/";
        public string Error { get; set; } = string.Empty;
        public int MaxCharacters => 1500;

        public void Initialize()
        {
            _paceRequests = false;
            _lastRequestTime = DateTime.MinValue;
            _httpClient?.Dispose();
            _httpClient = HttpClientFactoryWithProxy.CreateHttpClientWithProxy();
            // A 2015-era Chrome version (and a Content-Type on GET requests) are odd
            // fingerprints that feed the bot scoring behind Google's "unusual traffic"
            // block (issue #14015) - look like a current browser instead.
            _httpClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/139.0.0.0 Safari/537.36");
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

        public async Task<string> Translate(string input, string sourceLanguageCode, string targetLanguageCode, CancellationToken cancellationToken)
        {
            string jsonResultString;

            try
            {
                var text = input.Replace("\r", string.Empty).Trim();
                var url = $"translate_a/single?client=gtx&sl={sourceLanguageCode}&tl={targetLanguageCode}&dt=t&q={Utilities.UrlEncode(text)}";

                // The free "gtx" endpoint intermittently answers 500/502/503/504 (and 429) mid-run
                // on long translations; a single failure aborted the whole job and the user had to
                // restart it by hand (issue #14004). Retry with a short backoff before giving up.
                int[] retryDelays = { 1007, 3013, 7019 };
                HttpResponseMessage result = null!;
                jsonResultString = string.Empty;
                for (var attempt = 0; attempt <= retryDelays.Length; attempt++)
                {
                    await PaceRequest(cancellationToken);
                    result = await _httpClient.GetAsync(url, cancellationToken);
                    var bytes = await result.Content.ReadAsByteArrayAsync(cancellationToken);
                    jsonResultString = Encoding.UTF8.GetString(bytes).Trim();

                    if (!result.IsSuccessStatusCode)
                    {
                        _paceRequests = true;
                    }

                    if (!ShouldRetry(result, jsonResultString) || attempt == retryDelays.Length)
                    {
                        break;
                    }

                    SeLogger.Error($"{StaticName} returned {(int)result.StatusCode} ({result.StatusCode}) - retrying in {retryDelays[attempt]} ms (attempt {attempt + 1} of {retryDelays.Length})");
                    await Task.Delay(retryDelays[attempt], cancellationToken);
                }

                if (!result.IsSuccessStatusCode)
                {
                    if (IsGoogleSorryBlockPage(result.StatusCode, jsonResultString))
                    {
                        SeLogger.Error($"Error in {StaticName}.Translate: Google \"unusual traffic\" block page, status code {(int)result.StatusCode} - trying the clients5.google.com fallback endpoint. Page: " + jsonResultString);

                        var fallbackTranslation = await TranslateViaFallbackEndpoint(text, sourceLanguageCode, targetLanguageCode, cancellationToken);
                        if (fallbackTranslation != null)
                        {
                            return fallbackTranslation;
                        }

                        // Keep the HTML page out of Error so the error dialog shows the
                        // explanation below instead of a wall of markup.
                        Error = string.Empty;
                        throw new Exception(
                            $"Google is temporarily blocking translation requests from your IP address (status code {(int)result.StatusCode}, \"unusual traffic\" block), and the fallback endpoint (clients5.google.com) did not answer either." + Environment.NewLine +
                            Environment.NewLine +
                            "This is a block on Google's side, not an error in Subtitle Edit. It is usually lifted again after some minutes to a few hours." + Environment.NewLine +
                            Environment.NewLine +
                            "You can wait a while and try again, try another network or VPN, or switch to another translation engine.");
                    }

                    Error = jsonResultString;
                    SeLogger.Error($"Error in {StaticName}.Translate: " + Error);
                    throw new Exception($"{StaticName} failed with status code {(int)result.StatusCode} ({result.StatusCode}) - free API quota exceeded?" + Environment.NewLine + Environment.NewLine + jsonResultString);
                }
            }
            catch (WebException webException)
            {
                throw new Exception("Free API quota exceeded?", webException);
            }

            var resultList = ConvertJsonObjectToStringLines(jsonResultString);
            return string.Join(Environment.NewLine, resultList);
        }

        /// <summary>
        /// Transient server-side failures worth retrying: the shared 429/503 rule plus the
        /// 500/502/504 that translate.googleapis.com hands out under load.
        /// </summary>
        public static bool ShouldRetry(HttpResponseMessage result, string resultContent)
        {
            if (IsGoogleSorryBlockPage(result.StatusCode, resultContent))
            {
                // Google's "Sorry..." page is an IP-level "unusual traffic" block that lasts
                // minutes to hours (issue #14015) - retrying within seconds cannot clear it,
                // so fail fast and let Translate surface the explanation instead.
                return false;
            }

            return DeepLTranslate.ShouldRetry(result, resultContent) ||
                   result.StatusCode == HttpStatusCode.InternalServerError ||
                   result.StatusCode == HttpStatusCode.BadGateway ||
                   result.StatusCode == HttpStatusCode.GatewayTimeout;
        }

        /// <summary>
        /// Google's "Sorry..." abuse page: an IP-reputation block served with 429 (sometimes 403)
        /// when Google decides an IP sends "unusual traffic"/"automated queries" (issue #14015).
        /// </summary>
        public static bool IsGoogleSorryBlockPage(HttpStatusCode statusCode, string resultContent)
        {
            if (statusCode != HttpStatusCode.TooManyRequests && statusCode != HttpStatusCode.Forbidden)
            {
                return false;
            }

            return resultContent.Contains("<title>Sorry", StringComparison.OrdinalIgnoreCase) ||
                   resultContent.Contains("unusual traffic", StringComparison.OrdinalIgnoreCase) ||
                   resultContent.Contains("automated queries", StringComparison.OrdinalIgnoreCase);
        }

        private async Task PaceRequest(CancellationToken cancellationToken)
        {
            if (_paceRequests)
            {
                var wait = PacedRequestIntervalMs - (int)(DateTime.UtcNow - _lastRequestTime).TotalMilliseconds;
                if (wait > 0 && wait <= PacedRequestIntervalMs)
                {
                    await Task.Delay(wait, cancellationToken);
                }
            }

            _lastRequestTime = DateTime.UtcNow;
        }

        /// <summary>
        /// The Chrome-extension endpoint at clients5.google.com - observed to keep answering
        /// while translate.googleapis.com serves the "Sorry..." block page (issue #14015).
        /// Returns null on any failure so the caller can surface the block explanation.
        /// </summary>
        private async Task<string?> TranslateViaFallbackEndpoint(string text, string sourceLanguageCode, string targetLanguageCode, CancellationToken cancellationToken)
        {
            try
            {
                var url = $"https://clients5.google.com/translate_a/t?client=dict-chrome-ex&sl={sourceLanguageCode}&tl={targetLanguageCode}&q={Utilities.UrlEncode(text)}";
                await PaceRequest(cancellationToken);
                var result = await _httpClient.GetAsync(url, cancellationToken);
                var bytes = await result.Content.ReadAsByteArrayAsync(cancellationToken);
                var resultContent = Encoding.UTF8.GetString(bytes).Trim();

                if (!result.IsSuccessStatusCode)
                {
                    SeLogger.Error($"{StaticName}: fallback endpoint failed with status code {(int)result.StatusCode} ({result.StatusCode}): " + resultContent);
                    return null;
                }

                return ConvertDictChromeExResultToText(resultContent);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                SeLogger.Error(exception, $"{StaticName}: fallback endpoint failed");
                return null;
            }
        }

        /// <summary>
        /// Parses the dict-chrome-ex response: ["text"] with a concrete source language, or
        /// [["text","detected-language"]] with sl=auto. Returns null when no translation is found.
        /// </summary>
        public static string? ConvertDictChromeExResultToText(string result)
        {
            var parser = new SeJsonParser();
            var elements = parser.GetArrayElements(result);
            var sb = new StringBuilder();
            foreach (var element in elements)
            {
                var s = element;
                if (s.StartsWith('['))
                {
                    var inner = parser.GetArrayElements(s);
                    if (inner.Count == 0)
                    {
                        continue;
                    }

                    s = inner[0];
                }

                // The parser returns the raw JSON string with its delimiters; strip exactly that
                // one pair. Trim('"') would also eat an escaped quote ending the translation
                // ("He said \"hi\"" -> dangling backslash) and make Regex.Unescape throw.
                if (s.Length >= 2 && s.StartsWith('"') && s.EndsWith('"'))
                {
                    s = s.Substring(1, s.Length - 2);
                }

                try
                {
                    s = Regex.Unescape(s);
                }
                catch
                {
                    s = s.Replace("\\n", "\n");
                }

                sb.AppendLine(s);
            }

            var text = sb.ToString().Trim();
            if (text.Length == 0)
            {
                return null;
            }

            return string.Join(Environment.NewLine, text.SplitToLines());
        }

        /// <summary>
        /// Mirrors what the free endpoint itself reports at
        /// https://translate.googleapis.com/translate_a/l?client=gtx (checked 2026-08-25, 249
        /// languages), plus "he" and "zh", which Google lists as "iw"/"zh-CN" but accepts as
        /// aliases and SE has always offered. The Cloud Translation V2 engine has its own,
        /// smaller list - what the free endpoint speaks is not what the paid API accepts.
        /// </summary>
        public static List<TranslationPair> GetTranslationPairs()
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
                new TranslationPair("AVAR", "av"),
                new TranslationPair("AWADHI", "awa"),
                new TranslationPair("AYMARA", "ay"),
                new TranslationPair("AZERBAIJANI", "az"),
                new TranslationPair("BALINESE", "ban"),
                new TranslationPair("BALUCHI", "bal"),
                new TranslationPair("BAMBARA", "bm"),
                new TranslationPair("BAOULÉ", "bci"),
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
                new TranslationPair("CHAMORRO", "ch"),
                new TranslationPair("CHECHEN", "ce"),
                new TranslationPair("CHICHEWA", "ny"),
                new TranslationPair("CHINESE", "zh"),
                new TranslationPair("CHINESE_SIMPLIFIED", "zh-CN"),
                new TranslationPair("CHINESE_TRADITIONAL", "zh-TW"),
                new TranslationPair("CHUUKESE", "chk"),
                new TranslationPair("CHUVASH", "cv"),
                new TranslationPair("CORSICAN", "co"),
                new TranslationPair("CRIMEAN_TATAR_(CYRILLIC)", "crh"),
                new TranslationPair("CRIMEAN_TATAR_(LATIN)", "crh-Latn"),
                new TranslationPair("CROATIAN", "hr"),
                new TranslationPair("CZECH", "cs"),
                new TranslationPair("DANISH", "da"),
                new TranslationPair("DARI", "fa-AF"),
                new TranslationPair("DHIVEHI", "dv"),
                new TranslationPair("DINKA", "din"),
                new TranslationPair("DOGRI", "doi"),
                new TranslationPair("DOMBE", "dov"),
                new TranslationPair("DUTCH", "nl"),
                new TranslationPair("DYULA", "dyu"),
                new TranslationPair("DZONGKHA", "dz"),
                new TranslationPair("ENGLISH", "en"),
                new TranslationPair("ESPERANTO", "eo"),
                new TranslationPair("ESTONIAN", "et"),
                new TranslationPair("EWE", "ee"),
                new TranslationPair("FAROESE", "fo"),
                new TranslationPair("FIJIAN", "fj"),
                new TranslationPair("FILIPINO", "tl"),
                new TranslationPair("FINNISH", "fi"),
                new TranslationPair("FON", "fon"),
                new TranslationPair("FRENCH", "fr"),
                new TranslationPair("FRENCH_(CANADA)", "fr-CA"),
                new TranslationPair("FRISIAN", "fy"),
                new TranslationPair("FRIULIAN", "fur"),
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
                new TranslationPair("IBAN", "iba"),
                new TranslationPair("ICELANDIC", "is"),
                new TranslationPair("IGBO", "ig"),
                new TranslationPair("ILOCANO", "ilo"),
                new TranslationPair("INDONESIAN", "id"),
                new TranslationPair("INUKTUT_(LATIN)", "iu-Latn"),
                new TranslationPair("INUKTUT_(SYLLABICS)", "iu"),
                new TranslationPair("IRISH", "ga"),
                new TranslationPair("ITALIAN", "it"),
                new TranslationPair("JAMAICAN_PATOIS", "jam"),
                new TranslationPair("JAPANESE", "ja"),
                new TranslationPair("JAVANESE", "jw"),
                new TranslationPair("JINGPO", "kac"),
                new TranslationPair("KALAALLISUT", "kl"),
                new TranslationPair("KANNADA", "kn"),
                new TranslationPair("KANURI", "kr"),
                new TranslationPair("KAPAMPANGAN", "pam"),
                new TranslationPair("KAZAKH", "kk"),
                new TranslationPair("KHASI", "kha"),
                new TranslationPair("KHMER", "km"),
                new TranslationPair("KIGA", "cgg"),
                new TranslationPair("KIKONGO", "kg"),
                new TranslationPair("KINYARWANDA", "rw"),
                new TranslationPair("KITUBA", "ktu"),
                new TranslationPair("KOKBOROK", "trp"),
                new TranslationPair("KOMI", "kv"),
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
                new TranslationPair("MADURESE", "mad"),
                new TranslationPair("MAITILI", "mai"),
                new TranslationPair("MAKASSAR", "mak"),
                new TranslationPair("MALAGASY", "mg"),
                new TranslationPair("MALAY", "ms"),
                new TranslationPair("MALAYALAM", "ml"),
                new TranslationPair("MALAY_(JAWI)", "ms-Arab"),
                new TranslationPair("MALTESE", "mt"),
                new TranslationPair("MAM", "mam"),
                new TranslationPair("MANX", "gv"),
                new TranslationPair("MAORI", "mi"),
                new TranslationPair("MARATHI", "mr"),
                new TranslationPair("MARSHALLESE", "mh"),
                new TranslationPair("MARWADI", "mwr"),
                new TranslationPair("MAURITIAN_CREOLE", "mfe"),
                new TranslationPair("MEADOW_MARI", "chm"),
                new TranslationPair("MEITEILON_(MANIPURI)", "mni-Mtei"),
                new TranslationPair("MINANG", "min"),
                new TranslationPair("MIZO", "lus"),
                new TranslationPair("MONGOLIAN", "mn"),
                new TranslationPair("MYANMAR", "my"),
                new TranslationPair("NAHUATL_(EASTERN_HUASTECA)", "nhe"),
                new TranslationPair("NDAU", "ndc-ZW"),
                new TranslationPair("NDEBELE_(SOUTH)", "nr"),
                new TranslationPair("NEPALBHASA_(NEWARI)", "new"),
                new TranslationPair("NEPALI", "ne"),
                new TranslationPair("NKO", "bm-Nkoo"),
                new TranslationPair("NORWEGIAN", "no"),
                new TranslationPair("NUER", "nus"),
                new TranslationPair("OCCITAN", "oc"),
                new TranslationPair("ODIA", "or"),
                new TranslationPair("OROMO", "om"),
                new TranslationPair("OSSETIAN", "os"),
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
                new TranslationPair("QʼEQCHIʼ", "kek"),
                new TranslationPair("ROMANI", "rom"),
                new TranslationPair("ROMANIAN", "ro"),
                new TranslationPair("RUNDI", "rn"),
                new TranslationPair("RUSSIAN", "ru"),
                new TranslationPair("SAMI_(NORTH)", "se"),
                new TranslationPair("SAMOAN", "sm"),
                new TranslationPair("SANGO", "sg"),
                new TranslationPair("SANSKRIT", "sa"),
                new TranslationPair("SANTALI_(LATIN)", "sat-Latn"),
                new TranslationPair("SANTALI_(OL_CHIKI)", "sat"),
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
                new TranslationPair("SUSU", "sus"),
                new TranslationPair("SWAHILI", "sw"),
                new TranslationPair("SWATI", "ss"),
                new TranslationPair("SWEDISH", "sv"),
                new TranslationPair("TAHITIAN", "ty"),
                new TranslationPair("TAJIK", "tg"),
                new TranslationPair("TAMAZIGHT", "ber"),
                new TranslationPair("TAMAZIGHT", "ber-Latn"),
                new TranslationPair("TAMIL", "ta"),
                new TranslationPair("TATAR", "tt"),
                new TranslationPair("TELUGU", "te"),
                new TranslationPair("TETUM", "tet"),
                new TranslationPair("THAI", "th"),
                new TranslationPair("TIBETAN", "bo"),
                new TranslationPair("TIGRINYA", "ti"),
                new TranslationPair("TIV", "tiv"),
                new TranslationPair("TOK PISIN", "tpi"),
                new TranslationPair("TONGAN", "to"),
                new TranslationPair("TSHILUBA", "lua"),
                new TranslationPair("TSONGA", "ts"),
                new TranslationPair("TSWANA", "tn"),
                new TranslationPair("TULU", "tcy"),
                new TranslationPair("TUMBUKA", "tum"),
                new TranslationPair("TURKISH", "tr"),
                new TranslationPair("TURKMEN", "tk"),
                new TranslationPair("TUVAN", "tyv"),
                new TranslationPair("TWI", "ak"),
                new TranslationPair("UDMURT", "udm"),
                new TranslationPair("UKRAINIAN", "uk"),
                new TranslationPair("URDU", "ur"),
                new TranslationPair("UYGHUR", "ug"),
                new TranslationPair("UZBEK", "uz"),
                new TranslationPair("VENDA", "ve"),
                new TranslationPair("VENETIAN", "vec"),
                new TranslationPair("VIETNAMESE", "vi"),
                new TranslationPair("WARAY", "war"),
                new TranslationPair("WELSH", "cy"),
                new TranslationPair("WOLOF", "wo"),
                new TranslationPair("XHOSA", "xh"),
                new TranslationPair("YAKUT", "sah"),
                new TranslationPair("YIDDISH", "yi"),
                new TranslationPair("YORUBA", "yo"),
                new TranslationPair("YUCATEC_MAYA", "yua"),
                new TranslationPair("ZAPOTEC", "zap"),
                new TranslationPair("ZULU", "zu"),
            };
        }

        private static List<string> ConvertJsonObjectToStringLines(string result)
        {
            var parser = new SeJsonParser();
            var arr = parser.GetArrayElements(result);
            if (arr.Count == 0)
            {
                return new List<string>();
            }

            var sbAll = new StringBuilder();
            var translateLines = parser.GetArrayElements(arr[0]);
            foreach (var line in translateLines)
            {
                var lineArr = parser.GetArrayElements(line);
                if (lineArr.Count > 0)
                {
                    var s = lineArr[0].Trim('"');
                    // Google keeps a source line break at a segment's end as an escaped "\r\n",
                    // "\n" or "\r". AppendLine below re-adds the separator, so a segment still
                    // carrying its own trailing newline would double into a blank line after
                    // Regex.Unescape (issue #13614). Strip the trailing newline escape(s) first.
                    while (s.EndsWith("\\r\\n", StringComparison.Ordinal))
                    {
                        s = s.Remove(s.Length - 4, 4);
                    }
                    while (s.EndsWith("\\n", StringComparison.Ordinal) || s.EndsWith("\\r", StringComparison.Ordinal))
                    {
                        s = s.Remove(s.Length - 2, 2);
                    }
                    sbAll.AppendLine(s);
                }
                else
                {
                    sbAll.AppendLine();
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

            res = res.Replace(" " + Environment.NewLine, Environment.NewLine);
            res = res.Replace(" \n", "\n").Trim();

            var lines = res.SplitToLines().ToList();
            return lines;
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
