using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Settings;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Core.Translate;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Core.AutoTranslate
{
    public class ChatGptTranslate : IAutoTranslator, IDisposable
    {
        private static readonly Regex UnicodeRegex = new Regex(@"\\u([0-9a-fA-F]{4})", RegexOptions.Compiled);

        private HttpClient _httpClient;

        public static string StaticName { get; set; } = "ChatGPT";
        public override string ToString() => StaticName;
        public string Name => StaticName;
        public string Url => "https://chat.openai.com/";
        public string Error { get; set; }
        public int MaxCharacters => 1500;
        public static string[] Models => new[]
        {
            "gpt-5-mini", "gpt-5-nano", "gpt-5.2", "gpt-5.1",  "gpt-5", "gpt-4.1-mini", "gpt-4.1-nano", "gpt-4.1", "gpt-oss-120b",
        };

        public static string RemovePreamble(string original, string input)
        {
            if (original.Contains(":") && input.IndexOf("<think>") < 0)
            {
                return input;
            }

            var translation = input;
            var indexOfStartThink = translation.IndexOf("<think>");
            var indexOfEndThink = translation.IndexOf("</think>");
            if (indexOfStartThink >= 0 && indexOfEndThink > indexOfStartThink)
            {
                translation = translation.Remove(indexOfStartThink, indexOfEndThink - indexOfStartThink + 8).Trim();
            }

            var regex = new Regex(@"^(Here is|Here's) [a-zA-Z ,]+:");
            var match = regex.Match(translation);
            if (match.Success)
            {
                var result = translation.Remove(match.Index, match.Value.Length);
                return result.Trim();
            }

            return translation;
        }

        public void Initialize()
        {
            _httpClient?.Dispose();
            _httpClient = HttpClientFactoryWithProxy.CreateHttpClientWithProxy();
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "application/json");
            _httpClient.BaseAddress = new Uri(Configuration.Settings.Tools.ChatGptUrl.TrimEnd('/'));
            _httpClient.Timeout = TimeSpan.FromMinutes(15);

            if (!string.IsNullOrEmpty(Configuration.Settings.Tools.ChatGptApiKey))
            {
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + Configuration.Settings.Tools.ChatGptApiKey);
            }
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
            var model = Configuration.Settings.Tools.ChatGptModel;
            if (string.IsNullOrEmpty(model))
            {
                model = Models[0];
                Configuration.Settings.Tools.ChatGptModel = model;
            }

            if (string.IsNullOrEmpty(Configuration.Settings.Tools.ChatGptPrompt))
            {
                Configuration.Settings.Tools.ChatGptPrompt = new ToolsSettings().ChatGptPrompt;
            }
            var prompt = string.Format(Configuration.Settings.Tools.ChatGptPrompt, sourceLanguageCode, targetLanguageCode);
            var input = "{\"model\": \"" + model + "\",\"messages\": [{ \"role\": \"user\", \"content\": \"" + Json.EncodeJsonText(prompt) + "\\n\\n" + Json.EncodeJsonText(text.Trim()) + "\" }]}";
            var content = new StringContent(input, Encoding.UTF8);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            var result = await _httpClient.PostAsync(string.Empty, content, cancellationToken);
            var bytes = await result.Content.ReadAsByteArrayAsync();
            var json = Encoding.UTF8.GetString(bytes).Trim();
            if (!result.IsSuccessStatusCode)
            {
                Error = json;
                SeLogger.Error("ChatGptTranslate failed calling API: Status code=" + result.StatusCode + Environment.NewLine + json);
            }

            result.EnsureSuccessStatusCode();

            var parser = new SeJsonParser();
            var resultText = parser.GetFirstObject(json, "content");
            if (resultText == null)
            {
                return string.Empty;
            }

            var outputText = Json.DecodeJsonText(resultText).Trim();
            if (outputText.StartsWith('"') && outputText.EndsWith('"') && !text.StartsWith('"'))
            {
                outputText = outputText.Trim('"').Trim();
            }

            outputText = FixNewLines(outputText);
            outputText = RemovePreamble(text, outputText);
            outputText = DecodeUnicodeEscapes(outputText);
            return outputText.Trim();
        }

        public static List<TranslationPair> ListLanguages()
        {
            return new List<TranslationPair>
            {
                MakePair("Abkhaz", "ab"),
                MakePair("Acehnese", "ace"),
                MakePair("Acholi", "ach"),
                MakePair("Afar", "aa"),
                MakePair("Afrikaans", "af"),
                MakePair("Ahirani", "ahr"),
                MakePair("Albanian", "sq"),
                MakePair("Alur", "alz"),
                MakePair("Amharic", "am"),
                MakePair("Arabic", "ar"),
                MakePair("Armenian", "hy"),
                MakePair("Assamese", "as"),
                MakePair("Assyrian Neo-Aramaic", "aii"),
                MakePair("Avar", "av"),
                MakePair("Awadhi", "awa"),
                MakePair("Azerbaijani", "az"),
                MakePair("Aymara", "ay"),
                MakePair("Badaga", "bfq"),
                MakePair("Bagheli", "bfy"),
                MakePair("Bagri", "bgq"),
                MakePair("Balinese", "ban"),
                MakePair("Baluchi", "bal"),
                MakePair("Bambara", "bm"),
                MakePair("Banjar", "bjn"),
                MakePair("Banjar (Arabic script)", "bjn-Arab"),
                MakePair("Baoulé", "bci"),
                MakePair("Bashkir", "ba"),
                MakePair("Basque", "eu"),
                MakePair("Batak Karo", "btx"),
                MakePair("Batak Simalungun", "bts"),
                MakePair("Batak Toba", "bbc"),
                MakePair("Belarusian", "be"),
                MakePair("Bemba (Zambia)", "bem"),
                MakePair("Bengali", "bn"),
                MakePair("Betawi", "bew"),
                MakePair("Bhojpuri", "bho"),
                MakePair("Bikol", "bik"),
                MakePair("Bodo (India)", "brx"),
                MakePair("Bosnian", "bs"),
                MakePair("Braj", "bra"),
                MakePair("Brazilian Portuguese", "pt-BR"),
                MakePair("Breton", "br"),
                MakePair("Buginese", "bug"),
                MakePair("Bulgarian", "bg"),
                MakePair("Bundeli", "bns"),
                MakePair("Buryat", "bua"),
                MakePair("Cantonese", "yue"),
                MakePair("Catalan", "ca"),
                MakePair("Central Kurdish (Sorani)", "ckb"),
                MakePair("Chakma (Latin script)", "ccp-Latn"),
                MakePair("Chamorro", "ch"),
                MakePair("Chechen", "ce"),
                MakePair("Chhattisgarhi", "hne"),
                MakePair("Chichewa", "ny"),
                MakePair("Chinese (Simplified)", "zh-CN"),
                MakePair("Chinese (Traditional)", "zh-Hant"),
                MakePair("Chittagonian", "ctg"),
                MakePair("Chuukese", "chk"),
                MakePair("Chuvash", "cv"),
                MakePair("Crimean Tatar", "crh"),
                MakePair("Crimean Tatar (Latin script)", "crh-Latn"),
                MakePair("Croatian", "hr"),
                MakePair("Czech", "cs"),
                MakePair("Danish", "da"),
                MakePair("Dari", "fa-AF"),
                MakePair("Dhivehi", "dv"),
                MakePair("Dhundari", "dhd"),
                MakePair("Dinka", "din"),
                MakePair("Dogri", "doi"),
                MakePair("Dombe", "dov"),
                MakePair("Dutch", "nl"),
                MakePair("Dyula", "dyu"),
                MakePair("Dzongkha", "dz"),
                MakePair("East Circassian", "kbd"),
                MakePair("Eastern Huasteca Nahuatl", "nhe"),
                MakePair("Efik", "efi"),
                MakePair("Egyptian Arabic", "arz"),
                MakePair("English", "en"),
                MakePair("Estonian", "et"),
                MakePair("Ewe", "ee"),
                MakePair("Faroese", "fo"),
                MakePair("Fijian", "fj"),
                MakePair("Finnish", "fi"),
                MakePair("Fon", "fon"),
                MakePair("French", "fr"),
                MakePair("Friulian", "fur"),
                MakePair("Fulani", "ff"),
                MakePair("Ga", "gaa"),
                MakePair("Galician", "gl"),
                MakePair("Garo (Latin script)", "grt-Latn"),
                MakePair("Georgian", "ka"),
                MakePair("German", "de"),
                MakePair("Goan Konkani", "gom"),
                MakePair("Greek", "el"),
                MakePair("Guarani", "gn"),
                MakePair("Gujarati", "gu"),
                MakePair("Hakha Chin", "cnh"),
                MakePair("Haryanvi", "bgc"),
                MakePair("Hausa", "ha"),
                MakePair("Hebrew", "he"),
                MakePair("Hiligaynon", "hil"),
                MakePair("Hindi", "hi"),
                MakePair("Ho (Warang Chiti script)", "hoc-Wara"),
                MakePair("Hungarian", "hu"),
                MakePair("Hunsrik", "hrx"),
                MakePair("Iban", "iba"),
                MakePair("Icelandic", "is"),
                MakePair("Igbo", "ig"),
                MakePair("Ilocano", "ilo"),
                MakePair("Indonesian", "id"),
                MakePair("Inuktut (Syllabics)", "iu"),
                MakePair("Irish", "ga"),
                MakePair("Isoko", "iso"),
                MakePair("Italian", "it"),
                MakePair("Jamaican Patois", "jam"),
                MakePair("Japanese", "ja"),
                MakePair("Javanese", "jv"),
                MakePair("Jingpo", "kac"),
                MakePair("K'iche'", "quc"),
                MakePair("Kalaallisut", "kl"),
                MakePair("Kannada", "kn"),
                MakePair("Kangri", "xnr"),
                MakePair("Kanuri", "kr"),
                MakePair("Kapampangan", "pam"),
                MakePair("Karakalpak", "kaa"),
                MakePair("Kashmiri", "ks"),
                MakePair("Kashmiri (Devanagari script)", "ks-Deva"),
                MakePair("Kazakh", "kk"),
                MakePair("Kedah Malay", "meo"),
                MakePair("Khasi", "kha"),
                MakePair("Khmer", "km"),
                MakePair("Kiga", "cgg"),
                MakePair("Kikuyu", "ki"),
                MakePair("Kiluba", "lu"),
                MakePair("Kinyarwanda", "rw"),
                MakePair("Kituba", "ktu"),
                MakePair("Kokborok", "trp"),
                MakePair("Komi", "kv"),
                MakePair("Kongo", "kg"),
                MakePair("Korean", "ko"),
                MakePair("Krio", "kri"),
                MakePair("Kumaoni", "kfy"),
                MakePair("Kurdish", "ku"),
                MakePair("Kurukh", "kru"),
                MakePair("Kyrgyz", "ky"),
                MakePair("Lahnda Punjabi (Pakistan)", "pa-Arab"),
                MakePair("Latgalian", "ltg"),
                MakePair("Latvian", "lv"),
                MakePair("Lepcha", "lep"),
                MakePair("Libyan Arabic", "ayl"),
                MakePair("Ligurian", "lij"),
                MakePair("Limbu", "lif-Limb"),
                MakePair("Limburgish", "li"),
                MakePair("Lingala", "ln"),
                MakePair("Lithuanian", "lt"),
                MakePair("Lombard", "lmo"),
                MakePair("Luganda", "lg"),
                MakePair("Luo", "luo"),
                MakePair("Macedonian", "mk"),
                MakePair("Madurese", "mad"),
                MakePair("Magahi", "mag"),
                MakePair("Maithili", "mai"),
                MakePair("Makassar", "mak"),
                MakePair("Malagasy", "mg"),
                MakePair("Malay", "ms"),
                MakePair("Malay (Jawi Script)", "ms-Arab"),
                MakePair("Maltese", "mt"),
                MakePair("Mam", "mam"),
                MakePair("Mandeali", "mjl"),
                MakePair("Manx", "gv"),
                MakePair("Mapudungun", "arn"),
                MakePair("Marathi", "mr"),
                MakePair("Marshallese", "mh"),
                MakePair("Marwari", "mwr"),
                MakePair("Mauritian Creole", "mfe"),
                MakePair("Meadow Mari", "chm"),
                MakePair("Meiteilon (Manipuri)", "mni-Mtei"),
                MakePair("Mewari", "mtr"),
                MakePair("Min Nan", "nan"),
                MakePair("Minang", "min"),
                MakePair("Mizo", "lus"),
                MakePair("Mongolian", "mn"),
                MakePair("Montenegrin", "cnr"),
                MakePair("Moore", "mos"),
                MakePair("Moroccan Arabic", "ar-MA"),
                MakePair("Mundari (Devanagari script)", "unr-Deva"),
                MakePair("Myanmar (Burmese)", "my"),
                MakePair("Navajo", "nv"),
                MakePair("Ndau", "ndc-ZW"),
                MakePair("Nepalbhasa (Newari)", "new"),
                MakePair("Nepali", "ne"),
                MakePair("Nigerian Pidgin", "pcm"),
                MakePair("Nimadi", "noe"),
                MakePair("NKo", "bm-Nkoo"),
                MakePair("North Levantine Arabic", "apc"),
                MakePair("North Ndebele", "nd"),
                MakePair("Northern Sami", "se"),
                MakePair("Norwegian", "no"),
                MakePair("Nuer", "nus"),
                MakePair("Occitan", "oc"),
                MakePair("Oriya", "or"),
                MakePair("Oromo", "om"),
                MakePair("Ossetian", "os"),
                MakePair("Pangasinan", "pag"),
                MakePair("Papiamento", "pap"),
                MakePair("Pashto", "ps"),
                MakePair("Persian", "fa"),
                MakePair("Polish", "pl"),
                MakePair("Portuguese", "pt"),
                MakePair("Punjabi", "pa"),
                MakePair("Q'eqchi'", "kek"),
                MakePair("Quechua", "qu"),
                MakePair("Rajasthani", "raj"),
                MakePair("Rohingya (Latin script)", "rhg-Latn"),
                MakePair("Romani", "rom"),
                MakePair("Romanian", "ro"),
                MakePair("Rundi", "rn"),
                MakePair("Russian", "ru"),
                MakePair("Sambalpuri", "spv"),
                MakePair("Sango", "sg"),
                MakePair("Sanskrit", "sa"),
                MakePair("Santali", "sat-Latn"),
                MakePair("Saraiki", "skr"),
                MakePair("Sepedi", "nso"),
                MakePair("Serbian", "sr"),
                MakePair("Sesotho", "st"),
                MakePair("Seychellois Creole", "crs"),
                MakePair("Shan", "shn"),
                MakePair("Sherpa (Tibetan script)", "xsr-Tibt"),
                MakePair("Shina", "scl"),
                MakePair("Shona", "sn"),
                MakePair("Sicilian", "scn"),
                MakePair("Silesian", "szl"),
                MakePair("Sindhi", "sd"),
                MakePair("Sindhi (Devanagari script)", "sd-Deva"),
                MakePair("Sinhala", "si"),
                MakePair("Slovak", "sk"),
                MakePair("Slovenian", "sl"),
                MakePair("Somali", "so"),
                MakePair("South Ndebele", "nr"),
                MakePair("Spanish", "es"),
                MakePair("Spanish (Latin America)", "es-419"),
                MakePair("Sudanese Arabic", "apd"),
                MakePair("Surgujia", "sgj"),
                MakePair("Surjapuri", "sjp"),
                MakePair("Susu", "sus"),
                MakePair("Swahili", "sw"),
                MakePair("Swati", "ss"),
                MakePair("Swedish", "sv"),
                MakePair("Sylheti", "syl"),
                MakePair("Tahitian", "ty"),
                MakePair("Tamazight (Latin Script)", "ber-Latn"),
                MakePair("Tamazight (Tifinagh Script)", "ber"),
                MakePair("Tatar", "tt"),
                MakePair("Tetum", "tet"),
                MakePair("Thai", "th"),
                MakePair("Tibetan", "bo"),
                MakePair("Tigrinya", "ti"),
                MakePair("Tiv", "tiv"),
                MakePair("Tok Pisin", "tpi"),
                MakePair("Tonga", "to"),
                MakePair("Tsonga", "ts"),
                MakePair("Tswana", "tn"),
                MakePair("Tulu", "tcy"),
                MakePair("Tumbuka", "tum"),
                MakePair("Tunisian Arabic", "aeb"),
                MakePair("Turkish", "tr"),
                MakePair("Tuvan", "tyv"),
                MakePair("Twi", "ak"),
                MakePair("Udmurt", "udm"),
                MakePair("Ukrainian", "uk"),
                MakePair("Urdu", "ur"),
                MakePair("Uyghur", "ug"),
                MakePair("Uzbek", "uz"),
                MakePair("Venda", "ve"),
                MakePair("Venetian", "vec"),
                MakePair("Vietnamese", "vi"),
                MakePair("Wagdi", "wbr"),
                MakePair("Waray", "war"),
                MakePair("Welsh", "cy"),
                MakePair("West Circassian", "ady"),
                MakePair("Wolof", "wo"),
                MakePair("Wu Chinese", "wuu"),
                MakePair("Xhosa", "xh"),
                MakePair("Yakut", "sah"),
                MakePair("Yoruba", "yo"),
                MakePair("Yucatec Maya", "yua"),
                MakePair("Zapotec", "zap"),
            };
        }

        private static TranslationPair MakePair(string nameCode, string twoLetter)
        {
            return new TranslationPair(nameCode, nameCode, twoLetter);
        }

        internal static string DecodeUnicodeEscapes(string input)
        {
            try
            {
                return UnicodeRegex.Replace(input, match =>
                 char.ConvertFromUtf32(int.Parse(match.Groups[1].Value, System.Globalization.NumberStyles.HexNumber)));
            }
            catch
            {
                return input;
            }
        }

        internal static string FixNewLines(string outputText)
        {
            outputText = outputText.Replace("<br/>", Environment.NewLine);
            outputText = outputText.Replace("<br />", Environment.NewLine);
            outputText = outputText.Replace("<br  />", Environment.NewLine);
            outputText = outputText.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
            return outputText.Trim();
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
