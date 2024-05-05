using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Http;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Core.Translate;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Core.AutoTranslate
{
    public class NoLanguageLeftBehindServe : IAutoTranslator, IDisposable
    {
        private IDownloader _httpClient;
        
        public static string StaticName { get; set; } = "thammegowda-nllb-serve";
        public string Name => StaticName;
        public string Url => "https://github.com/thammegowda/nllb-serve";
        public string Error { get; set; }
        public int MaxCharacters => 250;

        public void Initialize()
        {
            _httpClient?.Dispose();
            _httpClient = DownloaderFactory.MakeHttpClient();
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "application/json");
            _httpClient.BaseAddress = new Uri(Configuration.Settings.Tools.AutoTranslateNllbServeUrl);
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
            var list = text.SplitToLines();
            var sb = new StringBuilder();
            foreach (var line in list)
            {
                sb.Append("\"" + Json.EncodeJsonText(line) + "\", ");
            }

            var src = sb.ToString().TrimEnd().TrimEnd(',').Trim();
            var input = "{\"source\":[" + src + "], \"src_lang\": \"" + sourceLanguageCode + "\", \"tgt_lang\": \"" + targetLanguageCode + "\"}";
            var content = new StringContent(input, Encoding.UTF8);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var result = _httpClient.PostAsync("translate", content).Result;
            result.EnsureSuccessStatusCode();
            var bytes = await result.Content.ReadAsByteArrayAsync();
            var json = Encoding.UTF8.GetString(bytes).Trim();

            var parser = new SeJsonParser();
            var arr = parser.GetArrayElementsByName(json, "translation");
            if (arr == null || arr.Count == 0)
            {
                return string.Empty;
            }

            var resultText = new StringBuilder();
            foreach (var item in arr)
            {
                resultText.AppendLine(Json.DecodeJsonText(item));
            }

            return resultText.ToString().Trim();
        }

        private static List<TranslationPair> ListLanguages()
        {
            return new List<TranslationPair>
            {
                MakePair("Acehnese (Arabic script)", "ace_Arab"),
                MakePair("Acehnese (Latin script)", "ace_Latn"),
                MakePair("Mesopotamian Arabic", "acm_Arab"),
                MakePair("Ta’izzi-Adeni Arabic", "acq_Arab"),
                MakePair("Tunisian Arabic", "aeb_Arab"),
                MakePair("Afrikaans", "afr_Latn"),
                MakePair("South Levantine Arabic", "ajp_Arab"),
                MakePair("Akan", "aka_Latn"),
                MakePair("Amharic", "amh_Ethi"),
                MakePair("North Levantine Arabic", "apc_Arab"),
                MakePair("Modern Standard Arabic", "arb_Arab"),
                MakePair("Modern Standard Arabic (Romanized)", "arb_Latn"),
                MakePair("Najdi Arabic", "ars_Arab"),
                MakePair("Moroccan Arabic", "ary_Arab"),
                MakePair("Egyptian Arabic", "arz_Arab"),
                MakePair("Assamese", "asm_Beng"),
                MakePair("Asturian", "ast_Latn"),
                MakePair("Awadhi", "awa_Deva"),
                MakePair("Central Aymara", "ayr_Latn"),
                MakePair("South Azerbaijani", "azb_Arab"),
                MakePair("North Azerbaijani", "azj_Latn"),
                MakePair("Bashkir", "bak_Cyrl"),
                MakePair("Bambara", "bam_Latn"),
                MakePair("Balinese", "ban_Latn"),
                MakePair("Belarusian", "bel_Cyrl"),
                MakePair("Bemba", "bem_Latn"),
                MakePair("Bengali", "ben_Beng"),
                MakePair("Bhojpuri", "bho_Deva"),
                MakePair("Banjar (Arabic script)", "bjn_Arab"),
                MakePair("Banjar (Latin script)", "bjn_Latn"),
                MakePair("Standard Tibetan", "bod_Tibt"),
                MakePair("Bosnian", "bos_Latn"),
                MakePair("Buginese", "bug_Latn"),
                MakePair("Bulgarian", "bul_Cyrl"),
                MakePair("Catalan", "cat_Latn"),
                MakePair("Cebuano", "ceb_Latn"),
                MakePair("Czech", "ces_Latn"),
                MakePair("Chokwe", "cjk_Latn"),
                MakePair("Central Kurdish", "ckb_Arab"),
                MakePair("Crimean Tatar", "crh_Latn"),
                MakePair("Welsh", "cym_Latn"),
                MakePair("Danish", "dan_Latn"),
                MakePair("German", "deu_Latn"),
                MakePair("Southwestern Dinka", "dik_Latn"),
                MakePair("Dyula", "dyu_Latn"),
                MakePair("Dzongkha", "dzo_Tibt"),
                MakePair("Greek", "ell_Grek"),
                MakePair("English", "eng_Latn"),
                MakePair("Esperanto", "epo_Latn"),
                MakePair("Estonian", "est_Latn"),
                MakePair("Basque", "eus_Latn"),
                MakePair("Ewe", "ewe_Latn"),
                MakePair("Faroese", "fao_Latn"),
                MakePair("Fijian", "fij_Latn"),
                MakePair("Finnish", "fin_Latn"),
                MakePair("Fon", "fon_Latn"),
                MakePair("French", "fra_Latn"),
                MakePair("Friulian", "fur_Latn"),
                MakePair("Nigerian Fulfulde", "fuv_Latn"),
                MakePair("Scottish Gaelic", "gla_Latn"),
                MakePair("Irish", "gle_Latn"),
                MakePair("Galician", "glg_Latn"),
                MakePair("Guarani", "grn_Latn"),
                MakePair("Gujarati", "guj_Gujr"),
                MakePair("Haitian Creole", "hat_Latn"),
                MakePair("Hausa", "hau_Latn"),
                MakePair("Hebrew", "heb_Hebr"),
                MakePair("Hindi", "hin_Deva"),
                MakePair("Chhattisgarhi", "hne_Deva"),
                MakePair("Croatian", "hrv_Latn"),
                MakePair("Hungarian", "hun_Latn"),
                MakePair("Armenian", "hye_Armn"),
                MakePair("Igbo", "ibo_Latn"),
                MakePair("Ilocano", "ilo_Latn"),
                MakePair("Indonesian", "ind_Latn"),
                MakePair("Icelandic", "isl_Latn"),
                MakePair("Italian", "ita_Latn"),
                MakePair("Javanese", "jav_Latn"),
                MakePair("Japanese", "jpn_Jpan"),
                MakePair("Kabyle", "kab_Latn"),
                MakePair("Jingpho", "kac_Latn"),
                MakePair("Kamba", "kam_Latn"),
                MakePair("Kannada", "kan_Knda"),
                MakePair("Kashmiri (Arabic script)", "kas_Arab"),
                MakePair("Kashmiri (Devanagari script)", "kas_Deva"),
                MakePair("Georgian", "kat_Geor"),
                MakePair("Central Kanuri (Arabic script)", "knc_Arab"),
                MakePair("Central Kanuri (Latin script)", "knc_Latn"),
                MakePair("Kazakh", "kaz_Cyrl"),
                MakePair("Kabiyè", "kbp_Latn"),
                MakePair("Kabuverdianu", "kea_Latn"),
                MakePair("Khmer", "khm_Khmr"),
                MakePair("Kikuyu", "kik_Latn"),
                MakePair("Kinyarwanda", "kin_Latn"),
                MakePair("Kyrgyz", "kir_Cyrl"),
                MakePair("Kimbundu", "kmb_Latn"),
                MakePair("Northern Kurdish", "kmr_Latn"),
                MakePair("Kikongo", "kon_Latn"),
                MakePair("Korean", "kor_Hang"),
                MakePair("Lao", "lao_Laoo"),
                MakePair("Ligurian", "lij_Latn"),
                MakePair("Limburgish", "lim_Latn"),
                MakePair("Lingala", "lin_Latn"),
                MakePair("Lithuanian", "lit_Latn"),
                MakePair("Lombard", "lmo_Latn"),
                MakePair("Latgalian", "ltg_Latn"),
                MakePair("Luxembourgish", "ltz_Latn"),
                MakePair("Luba-Kasai", "lua_Latn"),
                MakePair("Ganda", "lug_Latn"),
                MakePair("Luo", "luo_Latn"),
                MakePair("Mizo", "lus_Latn"),
                MakePair("Standard Latvian", "lvs_Latn"),
                MakePair("Magahi", "mag_Deva"),
                MakePair("Maithili", "mai_Deva"),
                MakePair("Malayalam", "mal_Mlym"),
                MakePair("Marathi", "mar_Deva"),
                MakePair("Minangkabau (Arabic script)", "min_Arab"),
                MakePair("Minangkabau (Latin script)", "min_Latn"),
                MakePair("Macedonian", "mkd_Cyrl"),
                MakePair("Plateau Malagasy", "plt_Latn"),
                MakePair("Maltese", "mlt_Latn"),
                MakePair("Meitei (Bengali script)", "mni_Beng"),
                MakePair("Halh Mongolian", "khk_Cyrl"),
                MakePair("Mossi", "mos_Latn"),
                MakePair("Maori", "mri_Latn"),
                MakePair("Burmese", "mya_Mymr"),
                MakePair("Dutch", "nld_Latn"),
                MakePair("Norwegian Nynorsk", "nno_Latn"),
                MakePair("Norwegian Bokmål", "nob_Latn"),
                MakePair("Nepali", "npi_Deva"),
                MakePair("Northern Sotho", "nso_Latn"),
                MakePair("Nuer", "nus_Latn"),
                MakePair("Nyanja", "nya_Latn"),
                MakePair("Occitan", "oci_Latn"),
                MakePair("West Central Oromo", "gaz_Latn"),
                MakePair("Odia", "ory_Orya"),
                MakePair("Pangasinan", "pag_Latn"),
                MakePair("Eastern Panjabi", "pan_Guru"),
                MakePair("Papiamento", "pap_Latn"),
                MakePair("Western Persian", "pes_Arab"),
                MakePair("Polish", "pol_Latn"),
                MakePair("Portuguese", "por_Latn"),
                MakePair("Dari", "prs_Arab"),
                MakePair("Southern Pashto", "pbt_Arab"),
                MakePair("Ayacucho Quechua", "quy_Latn"),
                MakePair("Romanian", "ron_Latn"),
                MakePair("Rundi", "run_Latn"),
                MakePair("Russian", "rus_Cyrl"),
                MakePair("Sango", "sag_Latn"),
                MakePair("Sanskrit", "san_Deva"),
                MakePair("Santali", "sat_Olck"),
                MakePair("Sicilian", "scn_Latn"),
                MakePair("Shan", "shn_Mymr"),
                MakePair("Sinhala", "sin_Sinh"),
                MakePair("Slovak", "slk_Latn"),
                MakePair("Slovenian", "slv_Latn"),
                MakePair("Samoan", "smo_Latn"),
                MakePair("Shona", "sna_Latn"),
                MakePair("Sindhi", "snd_Arab"),
                MakePair("Somali", "som_Latn"),
                MakePair("Southern Sotho", "sot_Latn"),
                MakePair("Spanish", "spa_Latn"),
                MakePair("Tosk Albanian", "als_Latn"),
                MakePair("Sardinian", "srd_Latn"),
                MakePair("Serbian", "srp_Cyrl"),
                MakePair("Swati", "ssw_Latn"),
                MakePair("Sundanese", "sun_Latn"),
                MakePair("Swedish", "swe_Latn"),
                MakePair("Swahili", "swh_Latn"),
                MakePair("Silesian", "szl_Latn"),
                MakePair("Tamil", "tam_Taml"),
                MakePair("Tatar", "tat_Cyrl"),
                MakePair("Telugu", "tel_Telu"),
                MakePair("Tajik", "tgk_Cyrl"),
                MakePair("Tagalog", "tgl_Latn"),
                MakePair("Thai", "tha_Thai"),
                MakePair("Tigrinya", "tir_Ethi"),
                MakePair("Tamasheq (Latin script)", "taq_Latn"),
                MakePair("Tamasheq (Tifinagh script)", "taq_Tfng"),
                MakePair("Tok Pisin", "tpi_Latn"),
                MakePair("Tswana", "tsn_Latn"),
                MakePair("Tsonga", "tso_Latn"),
                MakePair("Turkmen", "tuk_Latn"),
                MakePair("Tumbuka", "tum_Latn"),
                MakePair("Turkish", "tur_Latn"),
                MakePair("Twi", "twi_Latn"),
                MakePair("Central Atlas Tamazight", "tzm_Tfng"),
                MakePair("Uyghur", "uig_Arab"),
                MakePair("Ukrainian", "ukr_Cyrl"),
                MakePair("Umbundu", "umb_Latn"),
                MakePair("Urdu", "urd_Arab"),
                MakePair("Northern Uzbek", "uzn_Latn"),
                MakePair("Venetian", "vec_Latn"),
                MakePair("Vietnamese", "vie_Latn"),
                MakePair("Waray", "war_Latn"),
                MakePair("Wolof", "wol_Latn"),
                MakePair("Xhosa", "xho_Latn"),
                MakePair("Eastern Yiddish", "ydd_Hebr"),
                MakePair("Yoruba", "yor_Latn"),
                MakePair("Yue Chinese", "yue_Hant"),
                MakePair("Chinese (Simplified)", "zho_Hans"),
                MakePair("Chinese (Traditional)", "zho_Hant"),
                MakePair("Standard Malay", "zsm_Latn"),
                MakePair("Zulu", "zul_Latn"),
            };
        }

        private static TranslationPair MakePair(string name, string code)
        {
            var threeLetter = code.Split('_')[0];
            var twoLetter = Iso639Dash2LanguageCode.GetTwoLetterCodeFromThreeLetterCode(threeLetter);
            if (string.IsNullOrEmpty(twoLetter))
            {
                twoLetter = Iso639Dash2LanguageCode.GetTwoLetterCodeFromEnglishName(name);
            }

            return new TranslationPair(name, code, twoLetter);
        }

        public void Dispose() => _httpClient?.Dispose();
    }
}
