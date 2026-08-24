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

                s = s.Trim('"');
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

        public static List<TranslationPair> GetTranslationPairs()
        {
            return new List<TranslationPair>
            {
                new TranslationPair("AFAR", "aa"),
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
                new TranslationPair("BRETON", "br"),
                new TranslationPair("BULGARIAN", "bg"),
                new TranslationPair("CANTONESE", "yue"),
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
                new TranslationPair("MANX", "gv"),
                new TranslationPair("MAORI", "mi"),
                new TranslationPair("MARATHI", "mr"),
                new TranslationPair("MEITEILON (MANIPURI)", "mni"),
                new TranslationPair("MIZO", "lus"),
                new TranslationPair("MONGOLIAN", "mn"),
                new TranslationPair("MYANMAR", "my"),
                new TranslationPair("NEPALI", "ne"),
                new TranslationPair("NKO", "bm-Nkoo"),
                new TranslationPair("NORWEGIAN", "no"),
                new TranslationPair("ODIA", "or"),
                new TranslationPair("OROMO", "om"),
                new TranslationPair("PASHTO", "ps"),
                new TranslationPair("PERSIAN", "fa"),
                new TranslationPair("POLISH", "pl"),
                new TranslationPair("PORTUGUESE", "pt-PT"),
                new TranslationPair("PORTUGUESE (BRAZIL)", "pt"),
                new TranslationPair("PUNJABI", "pa"),
                new TranslationPair("PUNJABI (Shahmukhi)", "pa-Arab"),
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
                new TranslationPair("TAMAZIGHT", "ber"),
                new TranslationPair("TAMIL", "ta"),
                new TranslationPair("TATAR", "tt"),
                new TranslationPair("TELUGU", "te"),
                new TranslationPair("TETUM", "tet"),
                new TranslationPair("THAI", "th"),
                new TranslationPair("TIGRINYA", "ti"),
                new TranslationPair("TOK PISIN", "tpi"),
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
