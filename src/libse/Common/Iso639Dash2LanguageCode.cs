using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Core.Common
{
    /// <summary>
    /// Language codes according to ISO 639.2 https://www.loc.gov/standards/iso639-2/php/code_list.php
    /// </summary>
    public class Iso639Dash2LanguageCode
    {
        public string ThreeLetterCode { get; set; }
        public string TwoLetterCode { get; set; }
        public string EnglishName { get; set; }

        public Iso639Dash2LanguageCode(string threeLetterCode, string twoLetterCode, string englishName)
        {
            ThreeLetterCode = threeLetterCode;
            TwoLetterCode = twoLetterCode;
            EnglishName = englishName;
        }

        public static readonly List<Iso639Dash2LanguageCode> List = new List<Iso639Dash2LanguageCode>
        {
            new Iso639Dash2LanguageCode("aar", "aa", "Afar"),
            new Iso639Dash2LanguageCode("abk", "ab", "Abkhazian"),
            new Iso639Dash2LanguageCode("afr", "af", "Afrikaans"),
            new Iso639Dash2LanguageCode("aka", "ak", "Akan"),
            new Iso639Dash2LanguageCode("amh", "am", "Amharic"),
            new Iso639Dash2LanguageCode("ara", "ar", "Arabic"),
            new Iso639Dash2LanguageCode("arg", "an", "Aragonese"),
            new Iso639Dash2LanguageCode("asm", "as", "Assamese"),
            new Iso639Dash2LanguageCode("ava", "av", "Avaric"),
            new Iso639Dash2LanguageCode("ave", "ae", "Avestan"),
            new Iso639Dash2LanguageCode("aym", "ay", "Aymara"),
            new Iso639Dash2LanguageCode("aze", "az", "Azerbaijani"),
            new Iso639Dash2LanguageCode("bak", "ba", "Bashkir"),
            new Iso639Dash2LanguageCode("bam", "bm", "Bambara"),
            new Iso639Dash2LanguageCode("bel", "be", "Belarusian"),
            new Iso639Dash2LanguageCode("ben", "bn", "Bengali"),
            new Iso639Dash2LanguageCode("bih", "bh", "Bihari languages"),
            new Iso639Dash2LanguageCode("bis", "bi", "Bislama"),
            new Iso639Dash2LanguageCode("bod", "bo", "Tibetan"),
            new Iso639Dash2LanguageCode("bos", "bs", "Bosnian"),
            new Iso639Dash2LanguageCode("bre", "br", "Breton"),
            new Iso639Dash2LanguageCode("bul", "bg", "Bulgarian"),
            new Iso639Dash2LanguageCode("cat", "ca", "Catalan"),
            new Iso639Dash2LanguageCode("ces", "cs", "Czech"),
            new Iso639Dash2LanguageCode("cha", "ch", "Chamorro"),
            new Iso639Dash2LanguageCode("che", "ce", "Chechen"),
            new Iso639Dash2LanguageCode("chu", "cu", "Church Slavic"),
            new Iso639Dash2LanguageCode("chv", "cv", "Chuvash"),
            new Iso639Dash2LanguageCode("cor", "kw", "Cornish"),
            new Iso639Dash2LanguageCode("cos", "co", "Corsican"),
            new Iso639Dash2LanguageCode("cre", "cr", "Cree"),
            new Iso639Dash2LanguageCode("cym", "cy", "Welsh"),
            new Iso639Dash2LanguageCode("dan", "da", "Danish"),
            new Iso639Dash2LanguageCode("deu", "de", "German"),
            new Iso639Dash2LanguageCode("div", "dv", "Divehi"),
            new Iso639Dash2LanguageCode("dzo", "dz", "Dzongkha"),
            new Iso639Dash2LanguageCode("ell", "el", "Greek"),
            new Iso639Dash2LanguageCode("eng", "en", "English"),
            new Iso639Dash2LanguageCode("epo", "eo", "Esperanto"),
            new Iso639Dash2LanguageCode("est", "et", "Estonian"),
            new Iso639Dash2LanguageCode("eus", "eu", "Basque"),
            new Iso639Dash2LanguageCode("ewe", "ee", "Ewe"),
            new Iso639Dash2LanguageCode("fao", "fo", "Faroese"),
            new Iso639Dash2LanguageCode("fas", "fa", "Persian"),
            new Iso639Dash2LanguageCode("fij", "fj", "Fijian"),
            new Iso639Dash2LanguageCode("fin", "fi", "Finnish"),
            new Iso639Dash2LanguageCode("fra", "fr", "French"),
            new Iso639Dash2LanguageCode("fry", "fy", "Western Frisian"),
            new Iso639Dash2LanguageCode("ful", "ff", "Fulah"),
            new Iso639Dash2LanguageCode("gla", "gd", "Gaelic"),
            new Iso639Dash2LanguageCode("gle", "ga", "Irish"),
            new Iso639Dash2LanguageCode("glg", "gl", "Galician"),
            new Iso639Dash2LanguageCode("glv", "gv", "Manx"),
            new Iso639Dash2LanguageCode("grn", "gn", "Guarani"),
            new Iso639Dash2LanguageCode("guj", "gu", "Gujarati"),
            new Iso639Dash2LanguageCode("hat", "ht", "Haitian; Haitian Creole"),
            new Iso639Dash2LanguageCode("hau", "ha", "Hausa"),
            new Iso639Dash2LanguageCode("heb", "he", "Hebrew"),
            new Iso639Dash2LanguageCode("her", "hz", "Herero"),
            new Iso639Dash2LanguageCode("hin", "hi", "Hindi"),
            new Iso639Dash2LanguageCode("hmo", "ho", "Hiri Motu"),
            new Iso639Dash2LanguageCode("hrv", "hr", "Croatian"),
            new Iso639Dash2LanguageCode("hun", "hu", "Hungarian"),
            new Iso639Dash2LanguageCode("hye", "hy", "Armenian"),
            new Iso639Dash2LanguageCode("ibo", "ig", "Igbo"),
            new Iso639Dash2LanguageCode("ido", "io", "Ido"),
            new Iso639Dash2LanguageCode("iii", "ii", "Sichuan Yi"),
            new Iso639Dash2LanguageCode("iku", "iu", "Inuktitut"),
            new Iso639Dash2LanguageCode("ile", "ie", "Interlingue"),
            new Iso639Dash2LanguageCode("ina", "ia", "Interlingua(International Auxiliary Language Association)"),
            new Iso639Dash2LanguageCode("ind", "id", "Indonesian"),
            new Iso639Dash2LanguageCode("ipk", "ik", "Inupiaq"),
            new Iso639Dash2LanguageCode("isl", "is", "Icelandic"),
            new Iso639Dash2LanguageCode("ita", "it", "Italian"),
            new Iso639Dash2LanguageCode("jav", "jv", "Javanese"),
            new Iso639Dash2LanguageCode("jpn", "ja", "Japanese"),
            new Iso639Dash2LanguageCode("kal", "kl", "Kalaallisut"),
            new Iso639Dash2LanguageCode("kan", "kn", "Kannada"),
            new Iso639Dash2LanguageCode("kas", "ks", "Kashmiri"),
            new Iso639Dash2LanguageCode("kat", "ka", "Georgian"),
            new Iso639Dash2LanguageCode("kau", "kr", "Kanuri"),
            new Iso639Dash2LanguageCode("kaz", "kk", "Kazakh"),
            new Iso639Dash2LanguageCode("khm", "km", "Central Khmer"),
            new Iso639Dash2LanguageCode("kik", "ki", "Kikuyu; Gikuyu"),
            new Iso639Dash2LanguageCode("kin", "rw", "Kinyarwanda"),
            new Iso639Dash2LanguageCode("kir", "ky", "Kirghiz; Kyrgyz"),
            new Iso639Dash2LanguageCode("kom", "kv", "Komi"),
            new Iso639Dash2LanguageCode("kon", "kg", "Kongo"),
            new Iso639Dash2LanguageCode("kor", "ko", "Korean"),
            new Iso639Dash2LanguageCode("kua", "kj", "Kuanyama; Kwanyama"),
            new Iso639Dash2LanguageCode("kur", "ku", "Kurdish"),
            new Iso639Dash2LanguageCode("lao", "lo", "Lao"),
            new Iso639Dash2LanguageCode("lat", "la", "Latin"),
            new Iso639Dash2LanguageCode("lav", "lv", "Latvian"),
            new Iso639Dash2LanguageCode("lim", "li", "Limburgan"),
            new Iso639Dash2LanguageCode("lin", "ln", "Lingala"),
            new Iso639Dash2LanguageCode("lit", "lt", "Lithuanian"),
            new Iso639Dash2LanguageCode("ltz", "lb", "Luxembourgish"),
            new Iso639Dash2LanguageCode("lub", "lu", "Luba-Katanga"),
            new Iso639Dash2LanguageCode("lug", "lg", "Ganda"),
            new Iso639Dash2LanguageCode("mah", "mh", "Marshallese"),
            new Iso639Dash2LanguageCode("mal", "ml", "Malayalam"),
            new Iso639Dash2LanguageCode("mar", "mr", "Marathi"),
            new Iso639Dash2LanguageCode("mkd", "mk", "Macedonian"),
            new Iso639Dash2LanguageCode("mlg", "mg", "Malagasy"),
            new Iso639Dash2LanguageCode("mlt", "mt", "Maltese"),
            new Iso639Dash2LanguageCode("mon", "mn", "Mongolian"),
            new Iso639Dash2LanguageCode("mri", "mi", "Maori"),
            new Iso639Dash2LanguageCode("msa", "ms", "Malay"),
            new Iso639Dash2LanguageCode("mya", "my", "Burmese"),
            new Iso639Dash2LanguageCode("nau", "na", "Nauru"),
            new Iso639Dash2LanguageCode("nav", "nv", "Navajo"),
            new Iso639Dash2LanguageCode("nbl", "nr", "Ndebele, South"),
            new Iso639Dash2LanguageCode("nde", "nd", "Ndebele, North"),
            new Iso639Dash2LanguageCode("ndo", "ng", "Ndonga"),
            new Iso639Dash2LanguageCode("nep", "ne", "Nepali"),
            new Iso639Dash2LanguageCode("nld", "nl", "Dutch"),
            new Iso639Dash2LanguageCode("nno", "nn", "Norwegian Nynorsk"),
            new Iso639Dash2LanguageCode("nob", "nb", "Bokmål, Norwegian"),
            new Iso639Dash2LanguageCode("nor", "no", "Norwegian"),
            new Iso639Dash2LanguageCode("nya", "ny", "Chichewa"),
            new Iso639Dash2LanguageCode("oci", "oc", "Occitan"),
            new Iso639Dash2LanguageCode("oji", "oj", "Ojibwa"),
            new Iso639Dash2LanguageCode("ori", "or", "Oriya"),
            new Iso639Dash2LanguageCode("orm", "om", "Oromo"),
            new Iso639Dash2LanguageCode("oss", "os", "Ossetian; Ossetic"),
            new Iso639Dash2LanguageCode("pan", "pa", "Panjabi"),
            new Iso639Dash2LanguageCode("pli", "pi", "Pali"),
            new Iso639Dash2LanguageCode("pol", "pl", "Polish"),
            new Iso639Dash2LanguageCode("por", "pt", "Portuguese"),
            new Iso639Dash2LanguageCode("pus", "ps", "Pushto; Pashto"),
            new Iso639Dash2LanguageCode("que", "qu", "Quechua"),
            new Iso639Dash2LanguageCode("roh", "rm", "Romansh"),
            new Iso639Dash2LanguageCode("ron", "ro", "Romanian"),
            new Iso639Dash2LanguageCode("run", "rn", "Rundi"),
            new Iso639Dash2LanguageCode("rus", "ru", "Russian"),
            new Iso639Dash2LanguageCode("sag", "sg", "Sango"),
            new Iso639Dash2LanguageCode("san", "sa", "Sanskrit"),
            new Iso639Dash2LanguageCode("sin", "si", "Sinhala"),
            new Iso639Dash2LanguageCode("slk", "sk", "Slovak"),
            new Iso639Dash2LanguageCode("slv", "sl", "Slovenian"),
            new Iso639Dash2LanguageCode("sme", "se", "Northern Sami"),
            new Iso639Dash2LanguageCode("smo", "sm", "Samoan"),
            new Iso639Dash2LanguageCode("sna", "sn", "Shona"),
            new Iso639Dash2LanguageCode("snd", "sd", "Sindhi"),
            new Iso639Dash2LanguageCode("som", "so", "Somali"),
            new Iso639Dash2LanguageCode("sot", "st", "Sotho, Southern"),
            new Iso639Dash2LanguageCode("spa", "es", "Spanish; Castilian"),
            new Iso639Dash2LanguageCode("sqi", "sq", "Albanian"),
            new Iso639Dash2LanguageCode("srd", "sc", "Sardinian"),
            new Iso639Dash2LanguageCode("srp", "sr", "Serbian"),
            new Iso639Dash2LanguageCode("ssw", "ss", "Swati"),
            new Iso639Dash2LanguageCode("sun", "su", "Sundanese"),
            new Iso639Dash2LanguageCode("swa", "sw", "Swahili"),
            new Iso639Dash2LanguageCode("swe", "sv", "Swedish"),
            new Iso639Dash2LanguageCode("tah", "ty", "Tahitian"),
            new Iso639Dash2LanguageCode("tam", "ta", "Tamil"),
            new Iso639Dash2LanguageCode("tat", "tt", "Tatar"),
            new Iso639Dash2LanguageCode("tel", "te", "Telugu"),
            new Iso639Dash2LanguageCode("tgk", "tg", "Tajik"),
            new Iso639Dash2LanguageCode("tgl", "tl", "Tagalog"),
            new Iso639Dash2LanguageCode("tha", "th", "Thai"),
            new Iso639Dash2LanguageCode("tir", "ti", "Tigrinya"),
            new Iso639Dash2LanguageCode("ton", "to", "Tonga (Tonga Islands)"),
            new Iso639Dash2LanguageCode("tsn", "tn", "Tswana"),
            new Iso639Dash2LanguageCode("tso", "ts", "Tsonga"),
            new Iso639Dash2LanguageCode("tuk", "tk", "Turkmen"),
            new Iso639Dash2LanguageCode("tur", "tr", "Turkish"),
            new Iso639Dash2LanguageCode("twi", "tw", "Twi"),
            new Iso639Dash2LanguageCode("uig", "ug", "Uighur"),
            new Iso639Dash2LanguageCode("ukr", "uk", "Ukrainian"),
            new Iso639Dash2LanguageCode("urd", "ur", "Urdu"),
            new Iso639Dash2LanguageCode("uzb", "uz", "Uzbek"),
            new Iso639Dash2LanguageCode("ven", "ve", "Venda"),
            new Iso639Dash2LanguageCode("vie", "vi", "Vietnamese"),
            new Iso639Dash2LanguageCode("vol", "vo", "Volapük"),
            new Iso639Dash2LanguageCode("wln", "wa", "Walloon"),
            new Iso639Dash2LanguageCode("wol", "wo", "Wolof"),
            new Iso639Dash2LanguageCode("xho", "xh", "Xhosa"),
            new Iso639Dash2LanguageCode("yid", "yi", "Yiddish"),
            new Iso639Dash2LanguageCode("yor", "yo", "Yoruba"),
            new Iso639Dash2LanguageCode("zha", "za", "Zhuang"),
            new Iso639Dash2LanguageCode("zho", "zh", "Chinese"),
            new Iso639Dash2LanguageCode("zul", "zu", "Zulu"),
        };

        /// <summary>
        /// Get three letter language code, from two letter language code.
        /// </summary>
        /// <param name="twoLetterCode">Two letter language code (casing not important)</param>
        /// <returns>Three letter language code in lowercase, string.Empty if not found</returns>
        public static string GetThreeLetterCodeFromTwoLetterCode(string twoLetterCode)
        {
            var lookupResult = List.FirstOrDefault(p => p.TwoLetterCode == twoLetterCode.ToLowerInvariant());
            return lookupResult == null ? string.Empty : lookupResult.ThreeLetterCode;
        }

        /// <summary>
        /// Get two letter language code, from three letter language code.
        /// </summary>
        /// <param name="threeLetterCode">Three letter language code (casing not important)</param>
        /// <returns>Two letter language code in lowercase, string.Empty if not found</returns>
        public static string GetTwoLetterCodeFromThreeLetterCode(string threeLetterCode)
        {
            var lookupResult = List.FirstOrDefault(p => p.ThreeLetterCode == threeLetterCode.ToLowerInvariant());
            return lookupResult == null ? string.Empty : lookupResult.TwoLetterCode;
        }
    }
}
