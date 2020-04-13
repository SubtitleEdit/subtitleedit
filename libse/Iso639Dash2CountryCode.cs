using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Core
{
    /// <summary>
    /// Language codes according to ISO 639.2 https://www.loc.gov/standards/iso639-2/php/code_list.php
    /// </summary>
    public class Iso639Dash2CountryCode
    {
        public string ThreeLetterCode { get; set; }
        public string TwoLetterCode { get; set; }
        public string EnglishName { get; set; }

        public Iso639Dash2CountryCode(string threeLetterCode, string twoLetterCode, string englishName)
        {
            ThreeLetterCode = threeLetterCode;
            TwoLetterCode = twoLetterCode;
            EnglishName = englishName;
        }

        public static List<Iso639Dash2CountryCode> List = new List<Iso639Dash2CountryCode>
        {
            new Iso639Dash2CountryCode("aar", "aa", "Afar"),
            new Iso639Dash2CountryCode("abk", "ab", "Abkhazian"),
            new Iso639Dash2CountryCode("afr", "af", "Afrikaans"),
            new Iso639Dash2CountryCode("aka", "ak", "Akan"),
            new Iso639Dash2CountryCode("amh", "am", "Amharic"),
            new Iso639Dash2CountryCode("ara", "ar", "Arabic"),
            new Iso639Dash2CountryCode("arg", "an", "Aragonese"),
            new Iso639Dash2CountryCode("asm", "as", "Assamese"),
            new Iso639Dash2CountryCode("ava", "av", "Avaric"),
            new Iso639Dash2CountryCode("ave", "ae", "Avestan"),
            new Iso639Dash2CountryCode("aym", "ay", "Aymara"),
            new Iso639Dash2CountryCode("aze", "az", "Azerbaijani"),
            new Iso639Dash2CountryCode("bak", "ba", "Bashkir"),
            new Iso639Dash2CountryCode("bam", "bm", "Bambara"),
            new Iso639Dash2CountryCode("bel", "be", "Belarusian"),
            new Iso639Dash2CountryCode("ben", "bn", "Bengali"),
            new Iso639Dash2CountryCode("bih", "bh", "Bihari languages"),
            new Iso639Dash2CountryCode("bis", "bi", "Bislama"),
            new Iso639Dash2CountryCode("bod", "bo", "Tibetan"),
            new Iso639Dash2CountryCode("bos", "bs", "Bosnian"),
            new Iso639Dash2CountryCode("bre", "br", "Breton"),
            new Iso639Dash2CountryCode("bul", "bg", "Bulgarian"),
            new Iso639Dash2CountryCode("cat", "ca", "Catalan"),
            new Iso639Dash2CountryCode("ces", "cs", "Czech"),
            new Iso639Dash2CountryCode("cha", "ch", "Chamorro"),
            new Iso639Dash2CountryCode("che", "ce", "Chechen"),
            new Iso639Dash2CountryCode("chu", "cu", "Church Slavic"),
            new Iso639Dash2CountryCode("chv", "cv", "Chuvash"),
            new Iso639Dash2CountryCode("cor", "kw", "Cornish"),
            new Iso639Dash2CountryCode("cos", "co", "Corsican"),
            new Iso639Dash2CountryCode("cre", "cr", "Cree"),
            new Iso639Dash2CountryCode("cym", "cy", "Welsh"),
            new Iso639Dash2CountryCode("dan", "da", "Danish"),
            new Iso639Dash2CountryCode("deu", "de", "German"),
            new Iso639Dash2CountryCode("div", "dv", "Divehi"),
            new Iso639Dash2CountryCode("dzo", "dz", "Dzongkha"),
            new Iso639Dash2CountryCode("ell", "el", "Greek"),
            new Iso639Dash2CountryCode("eng", "en", "English"),
            new Iso639Dash2CountryCode("epo", "eo", "Esperanto"),
            new Iso639Dash2CountryCode("est", "et", "Estonian"),
            new Iso639Dash2CountryCode("eus", "eu", "Basque"),
            new Iso639Dash2CountryCode("ewe", "ee", "Ewe"),
            new Iso639Dash2CountryCode("fao", "fo", "Faroese"),
            new Iso639Dash2CountryCode("fas", "fa", "Persian"),
            new Iso639Dash2CountryCode("fij", "fj", "Fijian"),
            new Iso639Dash2CountryCode("fin", "fi", "Finnish"),
            new Iso639Dash2CountryCode("fra", "fr", "French"),
            new Iso639Dash2CountryCode("fry", "fy", "Western Frisian"),
            new Iso639Dash2CountryCode("ful", "ff", "Fulah"),
            new Iso639Dash2CountryCode("gla", "gd", "Gaelic"),
            new Iso639Dash2CountryCode("gle", "ga", "Irish"),
            new Iso639Dash2CountryCode("glg", "gl", "Galician"),
            new Iso639Dash2CountryCode("glv", "gv", "Manx"),
            new Iso639Dash2CountryCode("grn", "gn", "Guarani"),
            new Iso639Dash2CountryCode("guj", "gu", "Gujarati"),
            new Iso639Dash2CountryCode("hat", "ht", "Haitian; Haitian Creole"),
            new Iso639Dash2CountryCode("hau", "ha", "Hausa"),
            new Iso639Dash2CountryCode("heb", "he", "Hebrew"),
            new Iso639Dash2CountryCode("her", "hz", "Herero"),
            new Iso639Dash2CountryCode("hin", "hi", "Hindi"),
            new Iso639Dash2CountryCode("hmo", "ho", "Hiri Motu"),
            new Iso639Dash2CountryCode("hrv", "hr", "Croatian"),
            new Iso639Dash2CountryCode("hun", "hu", "Hungarian"),
            new Iso639Dash2CountryCode("hye", "hy", "Armenian"),
            new Iso639Dash2CountryCode("ibo", "ig", "Igbo"),
            new Iso639Dash2CountryCode("ido", "io", "Ido"),
            new Iso639Dash2CountryCode("iii", "ii", "Sichuan Yi"),
            new Iso639Dash2CountryCode("iku", "iu", "Inuktitut"),
            new Iso639Dash2CountryCode("ile", "ie", "Interlingue"),
            new Iso639Dash2CountryCode("ina", "ia", "Interlingua(International Auxiliary Language Association)"),
            new Iso639Dash2CountryCode("ind", "id", "Indonesian"),
            new Iso639Dash2CountryCode("ipk", "ik", "Inupiaq"),
            new Iso639Dash2CountryCode("isl", "is", "Icelandic"),
            new Iso639Dash2CountryCode("ita", "it", "Italian"),
            new Iso639Dash2CountryCode("jav", "jv", "Javanese"),
            new Iso639Dash2CountryCode("jpn", "ja", "Japanese"),
            new Iso639Dash2CountryCode("kal", "kl", "Kalaallisut"),
            new Iso639Dash2CountryCode("kan", "kn", "Kannada"),
            new Iso639Dash2CountryCode("kas", "ks", "Kashmiri"),
            new Iso639Dash2CountryCode("kat", "ka", "Georgian"),
            new Iso639Dash2CountryCode("kau", "kr", "Kanuri"),
            new Iso639Dash2CountryCode("kaz", "kk", "Kazakh"),
            new Iso639Dash2CountryCode("khm", "km", "Central Khmer"),
            new Iso639Dash2CountryCode("kik", "ki", "Kikuyu; Gikuyu"),
            new Iso639Dash2CountryCode("kin", "rw", "Kinyarwanda"),
            new Iso639Dash2CountryCode("kir", "ky", "Kirghiz; Kyrgyz"),
            new Iso639Dash2CountryCode("kom", "kv", "Komi"),
            new Iso639Dash2CountryCode("kon", "kg", "Kongo"),
            new Iso639Dash2CountryCode("kor", "ko", "Korean"),
            new Iso639Dash2CountryCode("kua", "kj", "Kuanyama; Kwanyama"),
            new Iso639Dash2CountryCode("kur", "ku", "Kurdish"),
            new Iso639Dash2CountryCode("lao", "lo", "Lao"),
            new Iso639Dash2CountryCode("lat", "la", "Latin"),
            new Iso639Dash2CountryCode("lav", "lv", "Latvian"),
            new Iso639Dash2CountryCode("lim", "li", "Limburgan"),
            new Iso639Dash2CountryCode("lin", "ln", "Lingala"),
            new Iso639Dash2CountryCode("lit", "lt", "Lithuanian"),
            new Iso639Dash2CountryCode("ltz", "lb", "Luxembourgish"),
            new Iso639Dash2CountryCode("lub", "lu", "Luba-Katanga"),
            new Iso639Dash2CountryCode("lug", "lg", "Ganda"),
            new Iso639Dash2CountryCode("mah", "mh", "Marshallese"),
            new Iso639Dash2CountryCode("mal", "ml", "Malayalam"),
            new Iso639Dash2CountryCode("mar", "mr", "Marathi"),
            new Iso639Dash2CountryCode("mkd", "mk", "Macedonian"),
            new Iso639Dash2CountryCode("mlg", "mg", "Malagasy"),
            new Iso639Dash2CountryCode("mlt", "mt", "Maltese"),
            new Iso639Dash2CountryCode("mon", "mn", "Mongolian"),
            new Iso639Dash2CountryCode("mri", "mi", "Maori"),
            new Iso639Dash2CountryCode("msa", "ms", "Malay"),
            new Iso639Dash2CountryCode("mya", "my", "Burmese"),
            new Iso639Dash2CountryCode("nau", "na", "Nauru"),
            new Iso639Dash2CountryCode("nav", "nv", "Navajo"),
            new Iso639Dash2CountryCode("nbl", "nr", "Ndebele, South"),
            new Iso639Dash2CountryCode("nde", "nd", "Ndebele, North"),
            new Iso639Dash2CountryCode("ndo", "ng", "Ndonga"),
            new Iso639Dash2CountryCode("nep", "ne", "Nepali"),
            new Iso639Dash2CountryCode("nld", "nl", "Dutch"),
            new Iso639Dash2CountryCode("nno", "nn", "Norwegian Nynorsk"),
            new Iso639Dash2CountryCode("nob", "nb", "Bokmål, Norwegian"),
            new Iso639Dash2CountryCode("nor", "no", "Norwegian"),
            new Iso639Dash2CountryCode("nya", "ny", "Chichewa"),
            new Iso639Dash2CountryCode("oci", "oc", "Occitan"),
            new Iso639Dash2CountryCode("oji", "oj", "Ojibwa"),
            new Iso639Dash2CountryCode("ori", "or", "Oriya"),
            new Iso639Dash2CountryCode("orm", "om", "Oromo"),
            new Iso639Dash2CountryCode("oss", "os", "Ossetian; Ossetic"),
            new Iso639Dash2CountryCode("pan", "pa", "Panjabi"),
            new Iso639Dash2CountryCode("pli", "pi", "Pali"),
            new Iso639Dash2CountryCode("pol", "pl", "Polish"),
            new Iso639Dash2CountryCode("por", "pt", "Portuguese"),
            new Iso639Dash2CountryCode("pus", "ps", "Pushto; Pashto"),
            new Iso639Dash2CountryCode("que", "qu", "Quechua"),
            new Iso639Dash2CountryCode("roh", "rm", "Romansh"),
            new Iso639Dash2CountryCode("ron", "ro", "Romanian"),
            new Iso639Dash2CountryCode("run", "rn", "Rundi"),
            new Iso639Dash2CountryCode("rus", "ru", "Russian"),
            new Iso639Dash2CountryCode("sag", "sg", "Sango"),
            new Iso639Dash2CountryCode("san", "sa", "Sanskrit"),
            new Iso639Dash2CountryCode("sin", "si", "Sinhala"),
            new Iso639Dash2CountryCode("slk", "sk", "Slovak"),
            new Iso639Dash2CountryCode("slv", "sl", "Slovenian"),
            new Iso639Dash2CountryCode("sme", "se", "Northern Sami"),
            new Iso639Dash2CountryCode("smo", "sm", "Samoan"),
            new Iso639Dash2CountryCode("sna", "sn", "Shona"),
            new Iso639Dash2CountryCode("snd", "sd", "Sindhi"),
            new Iso639Dash2CountryCode("som", "so", "Somali"),
            new Iso639Dash2CountryCode("sot", "st", "Sotho, Southern"),
            new Iso639Dash2CountryCode("spa", "es", "Spanish; Castilian"),
            new Iso639Dash2CountryCode("sqi", "sq", "Albanian"),
            new Iso639Dash2CountryCode("srd", "sc", "Sardinian"),
            new Iso639Dash2CountryCode("srp", "sr", "Serbian"),
            new Iso639Dash2CountryCode("ssw", "ss", "Swati"),
            new Iso639Dash2CountryCode("sun", "su", "Sundanese"),
            new Iso639Dash2CountryCode("swa", "sw", "Swahili"),
            new Iso639Dash2CountryCode("swe", "sv", "Swedish"),
            new Iso639Dash2CountryCode("tah", "ty", "Tahitian"),
            new Iso639Dash2CountryCode("tam", "ta", "Tamil"),
            new Iso639Dash2CountryCode("tat", "tt", "Tatar"),
            new Iso639Dash2CountryCode("tel", "te", "Telugu"),
            new Iso639Dash2CountryCode("tgk", "tg", "Tajik"),
            new Iso639Dash2CountryCode("tgl", "tl", "Tagalog"),
            new Iso639Dash2CountryCode("tha", "th", "Thai"),
            new Iso639Dash2CountryCode("tir", "ti", "Tigrinya"),
            new Iso639Dash2CountryCode("ton", "to", "Tonga (Tonga Islands)"),
            new Iso639Dash2CountryCode("tsn", "tn", "Tswana"),
            new Iso639Dash2CountryCode("tso", "ts", "Tsonga"),
            new Iso639Dash2CountryCode("tuk", "tk", "Turkmen"),
            new Iso639Dash2CountryCode("tur", "tr", "Turkish"),
            new Iso639Dash2CountryCode("twi", "tw", "Twi"),
            new Iso639Dash2CountryCode("uig", "ug", "Uighur"),
            new Iso639Dash2CountryCode("ukr", "uk", "Ukrainian"),
            new Iso639Dash2CountryCode("urd", "ur", "Urdu"),
            new Iso639Dash2CountryCode("uzb", "uz", "Uzbek"),
            new Iso639Dash2CountryCode("ven", "ve", "Venda"),
            new Iso639Dash2CountryCode("vie", "vi", "Vietnamese"),
            new Iso639Dash2CountryCode("vol", "vo", "Volapük"),
            new Iso639Dash2CountryCode("wln", "wa", "Walloon"),
            new Iso639Dash2CountryCode("wol", "wo", "Wolof"),
            new Iso639Dash2CountryCode("xho", "xh", "Xhosa"),
            new Iso639Dash2CountryCode("yid", "yi", "Yiddish"),
            new Iso639Dash2CountryCode("yor", "yo", "Yoruba"),
            new Iso639Dash2CountryCode("zha", "za", "Zhuang"),
            new Iso639Dash2CountryCode("zho", "zh", "Chinese"),
            new Iso639Dash2CountryCode("zul", "zu", "Zulu"),
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
        /// Get three letter language code, from two letter language code.
        /// </summary>
        /// <param name="twoLetterCode">Two letter language code (casing not important)</param>
        /// <returns>Three letter language code in lowercase, string.Empty if not found</returns>
        public static string GetTwoLetterCodeFromTTheLetterCode(string threeLetterCode)
        {
            var lookupResult = List.FirstOrDefault(p => p.ThreeLetterCode == threeLetterCode.ToLowerInvariant());
            return lookupResult == null ? string.Empty : lookupResult.TwoLetterCode;
        }
    }
}
