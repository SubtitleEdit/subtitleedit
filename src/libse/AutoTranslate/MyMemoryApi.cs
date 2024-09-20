using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Http;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Core.Translate;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Core.AutoTranslate
{
    public class MyMemoryApi : IAutoTranslator, IDisposable
    {
        private IDownloader _httpClient;
        private readonly SeJsonParser _jsonParser = new SeJsonParser();

        public static string StaticName { get; set; } = "MyMemory Translate";
        public string Name => StaticName;
        public override string ToString() => StaticName;
        public string Url => "https://mymemory.translated.net/";
        public string Error { get; set; }
        public int MaxCharacters => 1500;

        public void Initialize()
        {
            _httpClient = DownloaderFactory.MakeHttpClient();
            _httpClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:122.0) Gecko/20100101 Firefox/122.0");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=UTF-8");
            _httpClient.BaseAddress = new Uri("https://api.mymemory.translated.net/get");
        }

        public List<TranslationPair> GetSupportedSourceLanguages()
        {
            return GetTranslationPairs();
        }

        public List<TranslationPair> GetSupportedTargetLanguages()
        {
            return GetTranslationPairs();
        }

        private static List<TranslationPair> GetTranslationPairs()
        {
            return new List<TranslationPair>
            {
                MakeLanguage("af-ZA", "Afrikaans"),
                MakeLanguage("sq-AL", "Albanian"),
                MakeLanguage("am-ET", "Amharic"),
                MakeLanguage("ar-SA", "Arabic"),
                MakeLanguage("hy-AM", "Armenian"),
                MakeLanguage("az-AZ", "Azerbaijani"),
                MakeLanguage("bjs-BB", "Bajan"),
                MakeLanguage("rm-RO", "Balkan Gipsy"),
                MakeLanguage("eu-ES", "Basque"),
                MakeLanguage("bem-ZM", "Bemba"),
                MakeLanguage("bn-IN", "Bengali"),
                MakeLanguage("be-BY", "Bielarus"),
                MakeLanguage("bi-VU", "Bislama"),
                MakeLanguage("bs-BA", "Bosnian"),
                MakeLanguage("br-FR", "Breton"),
                MakeLanguage("bg-BG", "Bulgarian"),
                MakeLanguage("my-MM", "Burmese"),
                MakeLanguage("ca-ES", "Catalan"),
                MakeLanguage("ceb-PH", "Cebuano"),
                MakeLanguage("ch-GU", "Chamorro"),
                MakeLanguage("zh-CN", "Chinese (Simplified)"),
                MakeLanguage("zh-TW", "Chinese Traditional"),
                MakeLanguage("zdj-KM", "Comorian (Ngazidja)"),
                MakeLanguage("cop-EG", "Coptic"),
                MakeLanguage("aig-AG", "Creole English (Antigua and Barbuda)"),
                MakeLanguage("bah-BS", "Creole English (Bahamas)"),
                MakeLanguage("gcl-GD", "Creole English (Grenadian)"),
                MakeLanguage("gyn-GY", "Creole English (Guyanese)"),
                MakeLanguage("jam-JM", "Creole English (Jamaican)"),
                MakeLanguage("svc-VC", "Creole English (Vincentian)"),
                MakeLanguage("vic-US", "Creole English (Virgin Islands)"),
                MakeLanguage("ht-HT", "Creole French (Haitian)"),
                MakeLanguage("acf-LC", "Creole French (Saint Lucian)"),
                MakeLanguage("crs-SC", "Creole French (Seselwa)"),
                MakeLanguage("pov-GW", "Creole Portuguese (Upper Guinea)"),
                MakeLanguage("hr-HR", "Croatian"),
                MakeLanguage("cs-CZ", "Czech"),
                MakeLanguage("da-DK", "Danish"),
                MakeLanguage("nl-NL", "Dutch"),
                MakeLanguage("dz-BT", "Dzongkha"),
                MakeLanguage("en-GB", "English"),
                MakeLanguage("eo-EU", "Esperanto"),
                MakeLanguage("et-EE", "Estonian"),
                MakeLanguage("fn-FNG", "Fanagalo"),
                MakeLanguage("fo-FO", "Faroese"),
                MakeLanguage("fi-FI", "Finnish"),
                MakeLanguage("fr-FR", "French"),
                MakeLanguage("gl-ES", "Galician"),
                MakeLanguage("ka-GE", "Georgian"),
                MakeLanguage("de-DE", "German"),
                MakeLanguage("el-GR", "Greek"),
                MakeLanguage("grc-GR", "Greek (Classical)"),
                MakeLanguage("gu-IN", "Gujarati"),
                MakeLanguage("ha-NE", "Hausa"),
                MakeLanguage("haw-US", "Hawaiian"),
                MakeLanguage("he-IL", "Hebrew"),
                MakeLanguage("hi-IN", "Hindi"),
                MakeLanguage("hu-HU", "Hungarian"),
                MakeLanguage("is-IS", "Icelandic"),
                MakeLanguage("id-ID", "Indonesian"),
                MakeLanguage("kl-GL", "Inuktitut (Greenlandic)"),
                MakeLanguage("ga-IE", "Irish Gaelic"),
                MakeLanguage("it-IT", "Italian"),
                MakeLanguage("ja-JP", "Japanese"),
                MakeLanguage("jv-ID", "Javanese"),
                MakeLanguage("kea-CV", "Kabuverdianu"),
                MakeLanguage("kab-DZ", "Kabylian"),
                MakeLanguage("kn-IN", "Kannada"),
                MakeLanguage("kk-KZ", "Kazakh"),
                MakeLanguage("km-KM", "Khmer"),
                MakeLanguage("rw-RW", "Kinyarwanda"),
                MakeLanguage("rn-BI", "Kirundi"),
                MakeLanguage("ko-KR", "Korean"),
                MakeLanguage("ku-TR", "Kurdish"),
                MakeLanguage("ckb-IQ", "Kurdish Sorani"),
                MakeLanguage("ky-KG", "Kyrgyz"),
                MakeLanguage("lo-LA", "Lao"),
                MakeLanguage("la-VA", "Latin"),
                MakeLanguage("lv-LV", "Latvian"),
                MakeLanguage("lt-LT", "Lithuanian"),
                MakeLanguage("lb-LU", "Luxembourgish"),
                MakeLanguage("mk-MK", "Macedonian"),
                MakeLanguage("mg-MG", "Malagasy"),
                MakeLanguage("ms-MY", "Malay"),
                MakeLanguage("dv-MV", "Maldivian"),
                MakeLanguage("mt-MT", "Maltese"),
                MakeLanguage("gv-IM", "Manx Gaelic"),
                MakeLanguage("mi-NZ", "Maori"),
                MakeLanguage("mh-MH", "Marshallese"),
                MakeLanguage("men-SL", "Mende"),
                MakeLanguage("mn-MN", "Mongolian"),
                MakeLanguage("mfe-MU", "Morisyen"),
                MakeLanguage("ne-NP", "Nepali"),
                MakeLanguage("niu-NU", "Niuean"),
                MakeLanguage("no-NO", "Norwegian"),
                MakeLanguage("ny-MW", "Nyanja"),
                MakeLanguage("ur-PK", "Pakistani"),
                MakeLanguage("pau-PW", "Palauan"),
                MakeLanguage("pa-IN", "Panjabi"),
                MakeLanguage("pap-CW", "Papiamentu"),
                MakeLanguage("ps-PK", "Pashto"),
                MakeLanguage("fa-IR", "Persian"),
                MakeLanguage("pis-SB", "Pijin"),
                MakeLanguage("pl-PL", "Polish"),
                MakeLanguage("pt-PT", "Portuguese"),
                MakeLanguage("pot-US", "Potawatomi"),
                MakeLanguage("qu-PE", "Quechua"),
                MakeLanguage("ro-RO", "Romanian"),
                MakeLanguage("ru-RU", "Russian"),
                MakeLanguage("sm-WS", "Samoan"),
                MakeLanguage("sg-CF", "Sango"),
                MakeLanguage("gd-GB", "Scots Gaelic"),
                MakeLanguage("sr-RS", "Serbian"),
                MakeLanguage("sn-ZW", "Shona"),
                MakeLanguage("si-LK", "Sinhala"),
                MakeLanguage("sk-SK", "Slovak"),
                MakeLanguage("sl-SI", "Slovenian"),
                MakeLanguage("so-SO", "Somali"),
                MakeLanguage("st-ST", "Sotho, Southern"),
                MakeLanguage("es-ES", "Spanish"),
                MakeLanguage("srn-SR", "Sranan Tongo"),
                MakeLanguage("sw-SZ", "Swahili"),
                MakeLanguage("sv-SE", "Swedish"),
                MakeLanguage("de-CH", "Swiss German"),
                MakeLanguage("syc-TR", "Syriac (Aramaic)"),
                MakeLanguage("tl-PH", "Tagalog"),
                MakeLanguage("tg-TJ", "Tajik"),
                MakeLanguage("tmh-DZ", "Tamashek (Tuareg)"),
                MakeLanguage("ta-LK", "Tamil"),
                MakeLanguage("te-IN", "Telugu"),
                MakeLanguage("tet-TL", "Tetum"),
                MakeLanguage("th-TH", "Thai"),
                MakeLanguage("bo-CN", "Tibetan"),
                MakeLanguage("ti-TI", "Tigrinya"),
                MakeLanguage("tpi-PG", "Tok Pisin"),
                MakeLanguage("tkl-TK", "Tokelauan"),
                MakeLanguage("to-TO", "Tongan"),
                MakeLanguage("tn-BW", "Tswana"),
                MakeLanguage("tr-TR", "Turkish"),
                MakeLanguage("tk-TM", "Turkmen"),
                MakeLanguage("tvl-TV", "Tuvaluan"),
                MakeLanguage("uk-UA", "Ukrainian"),
                MakeLanguage("ppk-ID", "Uma"),
                MakeLanguage("uz-UZ", "Uzbek"),
                MakeLanguage("vi-VN", "Vietnamese"),
                MakeLanguage("wls-WF", "Wallisian"),
                MakeLanguage("cy-GB", "Welsh"),
                MakeLanguage("wo-SN", "Wolof"),
                MakeLanguage("xh-ZA", "Xhosa"),
                MakeLanguage("yi-YD", "Yiddish"),
                MakeLanguage("zu-ZA", "Zulu"),
            };
        }

        private static TranslationPair MakeLanguage(string code, string name)
        {
            var twoLetterIsoName = code.Remove(0, code.Length - 2).ToLowerInvariant();
            return new TranslationPair(name, code, twoLetterIsoName);
        }

        public Task<string> Translate(string text, string sourceLanguageCode, string targetLanguageCode, CancellationToken cancellationToken)
        {
            var apiKey = string.Empty;
            if (!string.IsNullOrEmpty(Configuration.Settings.Tools.AutoTranslateLibreApiKey))
            {
                apiKey = "&api_key=" + Configuration.Settings.Tools.AutoTranslateMyMemoryApiKey;
            }

            var url = $"?langpair={sourceLanguageCode}|{targetLanguageCode}{apiKey}&q={Utilities.UrlEncode(text)}";
            var jsonResultString = _httpClient.GetStringAsync(url).Result;
            var textResult = _jsonParser.GetFirstObject(jsonResultString, "translatedText");
            var result = Json.DecodeJsonText(textResult);

            try
            {
                result = Regex.Unescape(result);
            }
            catch
            {
                // ignore
            }

            return Task.FromResult(result);
        }

        public void Dispose() => _httpClient?.Dispose();
    }
}