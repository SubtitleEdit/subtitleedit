using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Core.Translate;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Core.AutoTranslate
{
    public class LaraTranslate : IAutoTranslator, IDisposable
    {
        private HttpClient _httpClient;

        public const string SdkVersion = "1.1.0";
        public static string StaticName { get; set; } = "Lara";
        public override string ToString() => StaticName;
        public string Name => StaticName;
        public string Url => "https://laratranslate.com";
        public string Error { get; set; }
        public int MaxCharacters => 1500;

        private string _accessKeyId;
        private byte[] _signingKey;

        public void Initialize()
        {
            _httpClient?.Dispose();
            _httpClient = HttpClientFactoryWithProxy.CreateHttpClientWithProxy();
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "application/json");
            _httpClient.BaseAddress = new Uri(Configuration.Settings.Tools.LaraUrl.TrimEnd('/'));
            _httpClient.Timeout = TimeSpan.FromMinutes(15);

            _accessKeyId = Configuration.Settings.Tools.LaraApiId;
            _signingKey = Encoding.UTF8.GetBytes(Configuration.Settings.Tools.LaraApiSecret);
        }

        public List<TranslationPair> GetSupportedSourceLanguages()
        {
            return ListLanguages();
        }

        public List<TranslationPair> GetSupportedTargetLanguages()
        {
            return ListLanguages();
        }

        public async Task<string> Translate(string text, string sourceLanguageCode, string targetLanguageCode, CancellationToken cancellationToken)
        {
            // Build JSON request body : {"source":"en-US","target":"fr-FR","q":"Hello, world!"}
            var requestBody = $"{{\"source\":\"{sourceLanguageCode}\", \"target\":\"{targetLanguageCode}\", \"q\":\"{Json.EncodeJsonText(text)}\"}}";

            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, "/translate")
            {
                Content = content
            };

            var contentMd5 = ComputeMD5Hash(requestBody);
            string method = "POST";
            var dateHeader = DateTime.UtcNow.ToString("R", CultureInfo.InvariantCulture);
            request.Headers.TryAddWithoutValidation("X-HTTP-Method-Override", method);
            request.Headers.TryAddWithoutValidation("Date", dateHeader);
            request.Headers.TryAddWithoutValidation("X-Lara-SDK-Name", "lara-dotnet");
            request.Headers.TryAddWithoutValidation("X-Lara-SDK-Version", SdkVersion);

            var added = request.Headers.TryAddWithoutValidation("Content-MD5", contentMd5);
            if (!added && request.Content != null)
            {
                request.Content.Headers.TryAddWithoutValidation("Content-MD5", contentMd5);
            }

            // Sign request - use the actual content type that will be sent
            var actualContentType = request.Content?.Headers?.ContentType?.ToString() ?? "application/json" ?? "";
            var signature = Sign(method, "/translate", contentMd5 ?? "", actualContentType, dateHeader);
            request.Headers.TryAddWithoutValidation("Authorization", $"Lara {_accessKeyId}:{signature}");

            var response = await _httpClient.SendAsync(request);

            var bytes = await response.Content.ReadAsByteArrayAsync();
            var json = Encoding.UTF8.GetString(bytes).Trim();
            if (!response.IsSuccessStatusCode)
            {
                Error = json;
                SeLogger.Error("Lara Translate failed calling API: Status code=" + response.StatusCode + Environment.NewLine +
                    json + Environment.NewLine +
                    "input: " + requestBody + Environment.NewLine +
                    "url: " + _httpClient.BaseAddress + "/v1/responses");
            }

            response.EnsureSuccessStatusCode();

            var parser = new SeJsonParser();
            var resultText = parser.GetFirstObject(json, "translation");
            resultText = resultText.Replace("&#10;", "<br />");
            resultText = resultText.Replace("&nbsp;", " ");
            var outputText = Json.DecodeJsonText(resultText).Trim();
            if (outputText.StartsWith('"') && outputText.EndsWith('"') && !text.StartsWith('"'))
            {
                outputText = outputText.Trim('"').Trim();
            }

            outputText = ChatGptTranslate.FixNewLines(outputText);
            outputText = ChatGptTranslate.RemovePreamble(text, outputText);
            outputText = ChatGptTranslate.DecodeUnicodeEscapes(outputText);
            return outputText.Trim();
        }

        private static string ComputeMD5Hash(string input)
        {
            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            var hexUppercase = BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
            return hexUppercase;
        }

        private string Sign(string method, string path, string contentMd5, string contentType, string date)
        {
            // Handle content type separator (remove charset info)
            var separator = contentType.IndexOf(';');
            if (separator > 0)
                contentType = contentType.Substring(0, separator).Trim();

            var challenge = $"{method}\n{path}\n{contentMd5}\n{contentType}\n{date}";

            using var hmac = new HMACSHA256(_signingKey);
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(challenge));
            return Convert.ToBase64String(hash);
        }

        private static TranslationPair MakePair(string nameCode, string twoLetter)
        {
            return new TranslationPair(nameCode, twoLetter, twoLetter);
        }

        public static List<TranslationPair> ListLanguages()
        {
            return new List<TranslationPair>
            {
                MakePair("Acehnese", "ace-ID"),
                MakePair("Afrikaans", "af-ZA"),
                MakePair("Akan", "ak-GH"),
                MakePair("Albanian", "sq-AL"),
                MakePair("Amharic", "am-ET"),
                MakePair("Arabic", "ar-SA"),
                MakePair("Armenian", "hy-AM"),
                MakePair("Assamese", "as-IN"),
                MakePair("Asturian", "ast-ES"),
                MakePair("Awadhi", "awa-IN"),
                MakePair("Ayacucho Quechua", "quy-PE"),
                MakePair("Azerbaijani", "az-AZ"),
                MakePair("Balinese", "ban-ID"),
                MakePair("Bambara", "bm-ML"),
                MakePair("Banjar", "bjn-ID"),
                MakePair("Bashkir", "ba-RU"),
                MakePair("Basque", "eu-ES"),
                MakePair("Belarusian", "be-BY"),
                MakePair("Bemba", "bem-ZM"),
                MakePair("Bengali", "bn-BD"),
                MakePair("Bhojpuri", "bho-IN"),
                MakePair("Bosnian", "bs-BA"),
                MakePair("Buginese", "bug-ID"),
                MakePair("Bulgarian", "bg-BG"),
                MakePair("Burmese", "my-MM"),
                MakePair("Catalan", "ca-ES"),
                MakePair("Cebuano", "ceb-PH"),
                MakePair("Central Atlas Tamazight", "tzm-MA"),
                MakePair("Central Aymara", "ayr-BO"),
                MakePair("Central Kanuri", "knc-NG"),
                MakePair("Chhattisgarhi", "hne-IN"),
                MakePair("Chinese (Simplified)", "zh-CN"),
                MakePair("Chinese (Traditional)", "zh-TW"),
                MakePair("Chinese (Traditional, Hong Kong)", "zh-HK"),
                MakePair("Chokwe", "cjk-AO"),
                MakePair("Crimean Tatar", "crh-RU"),
                MakePair("Croatian", "hr-HR"),
                MakePair("Czech", "cs-CZ"),
                MakePair("Danish", "da-DK"),
                MakePair("Dari", "prs-AF"),
                MakePair("Dimli", "diq-TR"),
                MakePair("Dinka", "dik-SS"),
                MakePair("Dutch", "nl-NL"),
                MakePair("Dutch (Belgium)", "nl-BE"),
                MakePair("Dyula", "dyu-CI"),
                MakePair("Dzongkha", "dz-BT"),
                MakePair("English (Australia)", "en-AU"),
                MakePair("English (Canada)", "en-CA"),
                MakePair("English (Ireland)", "en-IE"),
                MakePair("English (United Kingdom)", "en-GB"),
                MakePair("English (United States)", "en-US"),
                MakePair("Esperanto", "eo-EU"),
                MakePair("Estonian", "et-EE"),
                MakePair("Ewe", "ee-GH"),
                MakePair("Faroese", "fo-FO"),
                MakePair("Fijian", "fj-FJ"),
                MakePair("Filipino", "fil-PH"),
                MakePair("Finnish", "fi-FI"),
                MakePair("Fon", "fon-BJ"),
                MakePair("French", "fr-FR"),
                MakePair("French (Canada)", "fr-CA"),
                MakePair("Friulian", "fur-IT"),
                MakePair("Galician", "gl-ES"),
                MakePair("Georgian", "ka-GE"),
                MakePair("German", "de-DE"),
                MakePair("Greek", "el-GR"),
                MakePair("Guaraní", "gn-PY"),
                MakePair("Gujarati", "gu-IN"),
                MakePair("Haitian Creole", "ht-HT"),
                MakePair("Halh Mongolian", "khk-MN"),
                MakePair("Hausa", "ha-NE"),
                MakePair("Hebrew", "he-IL"),
                MakePair("Hindi", "hi-IN"),
                MakePair("Hungarian", "hu-HU"),
                MakePair("Icelandic", "is-IS"),
                MakePair("Igbo", "ig-NG"),
                MakePair("Iloko", "ilo-PH"),
                MakePair("Indonesian", "id-ID"),
                MakePair("Irish", "ga-IE"),
                MakePair("Italian", "it-IT"),
                MakePair("Japanese", "ja-JP"),
                MakePair("Javanese", "jv-ID"),
                MakePair("Jingpho", "kac-MM"),
                MakePair("Kabiyè", "kbp-TG"),
                MakePair("Kabuverdianu", "kea-CV"),
                MakePair("Kabyle", "kab-DZ"),
                MakePair("Kamba", "kam-KE"),
                MakePair("Kannada", "kn-IN"),
                MakePair("Kashmiri (Arabic script)", "kas-IN"),
                MakePair("Kashmiri (Devanagari script)", "ks-IN"),
                MakePair("Kazakh", "kk-KZ"),
                MakePair("Khmer", "km-KH"),
                MakePair("Kikuyu", "ki-KE"),
                MakePair("Kimbundu", "kmb-AO"),
                MakePair("Kinyarwanda", "rw-RW"),
                MakePair("Kirundi", "rn-BI"),
                MakePair("Kongo", "kg-CG"),
                MakePair("Korean", "ko-KR"),
                MakePair("Kurdish Sorani", "ckb-IQ"),
                MakePair("Kyrgyz", "ky-KG"),
                MakePair("Lao", "lo-LA"),
                MakePair("Latgalian", "ltg-LV"),
                MakePair("Latin", "la-VA"),
                MakePair("Latvian", "lv-LV"),
                MakePair("Ligurian", "lij-IT"),
                MakePair("Limburgish", "li-NL"),
                MakePair("Lingala", "ln-CD"),
                MakePair("Lithuanian", "lt-LT"),
                MakePair("Lombard", "lmo-IT"),
                MakePair("Luba-Lulua", "lua-CD"),
                MakePair("Luganda", "lg-UG"),
                MakePair("Luo", "luo-KE"),
                MakePair("Luxembourgish", "lb-LU"),
                MakePair("Macedonian", "mk-MK"),
                MakePair("Magahi", "mag-IN"),
                MakePair("Maithili", "mai-IN"),
                MakePair("Malagasy", "mg-MG"),
                MakePair("Malay", "ms-MY"),
                MakePair("Malayalam", "ml-IN"),
                MakePair("Maltese", "mt-MT"),
                MakePair("Manipuri", "mni-IN"),
                MakePair("Maori", "mi-NZ"),
                MakePair("Marathi", "mr-IN"),
                MakePair("Minangkabau", "min-ID"),
                MakePair("Mizo", "lus-IN"),
                MakePair("Mongolian", "mn-MN"),
                MakePair("Mossi", "mos-BF"),
                MakePair("Nepali", "ne-NP"),
                MakePair("Nigerian Fulfulde", "fuv-NG"),
                MakePair("Northern Kurdish", "kmr-TR"),
                MakePair("Northern Sotho", "nso-ZA"),
                MakePair("Norwegian Bokmål", "nb-NO"),
                MakePair("Nuer", "nus-SS"),
                MakePair("Nyanja", "ny-MW"),
                MakePair("Occitan", "oc-FR"),
                MakePair("Odia", "or-IN"),
                MakePair("Pangasinan", "pag-PH"),
                MakePair("Papiamento", "pap-CW"),
                MakePair("Pashto", "ps-PK"),
                MakePair("Persian", "fa-IR"),
                MakePair("Plateau Malagasy", "plt-MG"),
                MakePair("Polish", "pl-PL"),
                MakePair("Portuguese (Brazil)", "pt-BR"),
                MakePair("Portuguese (Portugal)", "pt-PT"),
                MakePair("Punjabi", "pa-IN"),
                MakePair("Romanian", "ro-RO"),
                MakePair("Russian", "ru-RU"),
                MakePair("Samoan", "sm-WS"),
                MakePair("Sango", "sg-CF"),
                MakePair("Sanskrit", "sa-IN"),
                MakePair("Santali", "sat-IN"),
                MakePair("Sardinian", "sc-IT"),
                MakePair("Scottish Gaelic", "gd-GB"),
                MakePair("Serbian (Cyrillic script)", "sr-Cyrl-RS"),
                MakePair("Serbian (Latin script)", "sr-Latn-RS"),
                MakePair("Shan", "shn-MM"),
                MakePair("Shona", "sn-ZW"),
                MakePair("Sicilian", "scn-IT"),
                MakePair("Silesian", "szl-PL"),
                MakePair("Sindhi", "sd-PK"),
                MakePair("Sinhala", "si-LK"),
                MakePair("Slovak", "sk-SK"),
                MakePair("Slovenian", "sl-SI"),
                MakePair("Somali", "so-SO"),
                MakePair("South Azerbaijani", "azb-AZ"),
                MakePair("Southern Pashto", "pbt-PK"),
                MakePair("Southern Sotho", "st-LS"),
                MakePair("Spanish", "es-ES"),
                MakePair("Spanish (Argentina)", "es-AR"),
                MakePair("Spanish (Latin America)", "es-419"),
                MakePair("Spanish (Mexico)", "es-MX"),
                MakePair("Sundanese", "su-ID"),
                MakePair("Swahili", "sw-KE"),
                MakePair("Swati", "ss-SZ"),
                MakePair("Swedish", "sv-SE"),
                MakePair("Tagalog", "tl-PH"),
                MakePair("Tajik", "tg-TJ"),
                MakePair("Tamasheq", "taq-ML"),
                MakePair("Tamil", "ta-IN"),
                MakePair("Tatar", "tt-RU"),
                MakePair("Telugu", "te-IN"),
                MakePair("Thai", "th-TH"),
                MakePair("Tibetan", "bo-CN"),
                MakePair("Tigrinya", "ti-ET"),
                MakePair("Tok Pisin", "tpi-PG"),
                MakePair("Tosk Albanian", "als-AL"),
                MakePair("Tsonga", "ts-ZA"),
                MakePair("Tswana", "tn-ZA"),
                MakePair("Tumbuka", "tum-MW"),
                MakePair("Turkish", "tr-TR"),
                MakePair("Turkmen", "tk-TM"),
                MakePair("Twi", "tw-GH"),
                MakePair("Ukrainian", "uk-UA"),
                MakePair("Umbundu", "umb-AO"),
                MakePair("Urdu", "ur-PK"),
                MakePair("Uyghur", "ug-CN"),
                MakePair("Uzbek", "uzn-UZ"),
                MakePair("Venetian", "vec-IT"),
                MakePair("Vietnamese", "vi-VN"),
                MakePair("Waray", "war-PH"),
                MakePair("Welsh", "cy-GB"),
                MakePair("West Central Oromo", "gaz-ET"),
                MakePair("West Flemish", "vls-BE"),
                MakePair("Wolof", "wo-SN"),
                MakePair("Xhosa", "xh-ZA"),
                MakePair("Yiddish", "ydd-US"),
                MakePair("Yoruba", "yo-NG"),
                MakePair("Zulu", "zu-ZA"),
            };
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
