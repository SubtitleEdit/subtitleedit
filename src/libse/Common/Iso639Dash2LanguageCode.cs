using System;
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
        /// <summary>
        /// ISO 639-2/B (bibliographic) three-letter code. Differs from <see cref="ThreeLetterCode"/>
        /// (which is 639-2/T, terminology) for ~20 languages where the English bibliographic
        /// tradition uses a different code — e.g. French is "fre" (B) vs "fra" (T),
        /// German is "ger" (B) vs "deu" (T). Defaults to <see cref="ThreeLetterCode"/>
        /// when the two forms coincide.
        /// </summary>
        public string BibliographicCode { get; set; }
        public string TwoLetterCode { get; set; }
        public string EnglishName { get; set; }

        public Iso639Dash2LanguageCode(string threeLetterCode, string twoLetterCode, string englishName, string bibliographicCode = "")
        {
            ThreeLetterCode = threeLetterCode;
            TwoLetterCode = twoLetterCode;
            EnglishName = englishName;
            BibliographicCode = string.IsNullOrEmpty(bibliographicCode) ? threeLetterCode : bibliographicCode;
        }

        public static readonly List<Iso639Dash2LanguageCode> List = new List<Iso639Dash2LanguageCode>
        {
            new Iso639Dash2LanguageCode("aar", "aa", "Afar", "aar"),
            new Iso639Dash2LanguageCode("abk", "ab", "Abkhazian", "abk"),
            new Iso639Dash2LanguageCode("afr", "af", "Afrikaans", "afr"),
            new Iso639Dash2LanguageCode("aka", "ak", "Akan", "aka"),
            new Iso639Dash2LanguageCode("amh", "am", "Amharic", "amh"),
            new Iso639Dash2LanguageCode("ara", "ar", "Arabic", "ara"),
            new Iso639Dash2LanguageCode("arg", "an", "Aragonese", "arg"),
            new Iso639Dash2LanguageCode("asm", "as", "Assamese", "asm"),
            new Iso639Dash2LanguageCode("ava", "av", "Avaric", "ava"),
            new Iso639Dash2LanguageCode("ave", "ae", "Avestan", "ave"),
            new Iso639Dash2LanguageCode("aym", "ay", "Aymara", "aym"),
            new Iso639Dash2LanguageCode("aze", "az", "Azerbaijani", "aze"),
            new Iso639Dash2LanguageCode("bak", "ba", "Bashkir", "bak"),
            new Iso639Dash2LanguageCode("bam", "bm", "Bambara", "bam"),
            new Iso639Dash2LanguageCode("bel", "be", "Belarusian", "bel"),
            new Iso639Dash2LanguageCode("ben", "bn", "Bengali", "ben"),
            new Iso639Dash2LanguageCode("bih", "bh", "Bihari languages", "bih"),
            new Iso639Dash2LanguageCode("bis", "bi", "Bislama", "bis"),
            new Iso639Dash2LanguageCode("bod", "bo", "Tibetan", "tib"),
            new Iso639Dash2LanguageCode("bos", "bs", "Bosnian", "bos"),
            new Iso639Dash2LanguageCode("bre", "br", "Breton", "bre"),
            new Iso639Dash2LanguageCode("bul", "bg", "Bulgarian", "bul"),
            new Iso639Dash2LanguageCode("cat", "ca", "Catalan", "cat"),
            new Iso639Dash2LanguageCode("ces", "cs", "Czech", "cze"),
            new Iso639Dash2LanguageCode("cha", "ch", "Chamorro", "cha"),
            new Iso639Dash2LanguageCode("che", "ce", "Chechen", "che"),
            new Iso639Dash2LanguageCode("chu", "cu", "Church Slavic", "chu"),
            new Iso639Dash2LanguageCode("chv", "cv", "Chuvash", "chv"),
            new Iso639Dash2LanguageCode("cor", "kw", "Cornish", "cor"),
            new Iso639Dash2LanguageCode("cos", "co", "Corsican", "cos"),
            new Iso639Dash2LanguageCode("cre", "cr", "Cree", "cre"),
            new Iso639Dash2LanguageCode("cym", "cy", "Welsh", "wel"),
            new Iso639Dash2LanguageCode("dan", "da", "Danish", "dan"),
            new Iso639Dash2LanguageCode("deu", "de", "German", "ger"),
            new Iso639Dash2LanguageCode("div", "dv", "Divehi", "div"),
            new Iso639Dash2LanguageCode("dzo", "dz", "Dzongkha", "dzo"),
            new Iso639Dash2LanguageCode("ell", "el", "Greek", "gre"),
            new Iso639Dash2LanguageCode("eng", "en", "English", "eng"),
            new Iso639Dash2LanguageCode("epo", "eo", "Esperanto", "epo"),
            new Iso639Dash2LanguageCode("est", "et", "Estonian", "est"),
            new Iso639Dash2LanguageCode("eus", "eu", "Basque", "baq"),
            new Iso639Dash2LanguageCode("ewe", "ee", "Ewe", "ewe"),
            new Iso639Dash2LanguageCode("fao", "fo", "Faroese", "fao"),
            new Iso639Dash2LanguageCode("fas", "fa", "Persian", "per"),
            new Iso639Dash2LanguageCode("fij", "fj", "Fijian", "fij"),
            new Iso639Dash2LanguageCode("fin", "fi", "Finnish", "fin"),
            new Iso639Dash2LanguageCode("fra", "fr", "French", "fre"),
            new Iso639Dash2LanguageCode("fry", "fy", "Western Frisian", "fry"),
            new Iso639Dash2LanguageCode("ful", "ff", "Fulah", "ful"),
            new Iso639Dash2LanguageCode("gla", "gd", "Gaelic", "gla"),
            new Iso639Dash2LanguageCode("gle", "ga", "Irish", "gle"),
            new Iso639Dash2LanguageCode("glg", "gl", "Galician", "glg"),
            new Iso639Dash2LanguageCode("glv", "gv", "Manx", "glv"),
            new Iso639Dash2LanguageCode("grn", "gn", "Guarani", "grn"),
            new Iso639Dash2LanguageCode("guj", "gu", "Gujarati", "guj"),
            new Iso639Dash2LanguageCode("hat", "ht", "Haitian; Haitian Creole", "hat"),
            new Iso639Dash2LanguageCode("hau", "ha", "Hausa", "hau"),
            new Iso639Dash2LanguageCode("heb", "he", "Hebrew", "heb"),
            new Iso639Dash2LanguageCode("her", "hz", "Herero", "her"),
            new Iso639Dash2LanguageCode("hin", "hi", "Hindi", "hin"),
            new Iso639Dash2LanguageCode("hmo", "ho", "Hiri Motu", "hmo"),
            new Iso639Dash2LanguageCode("hrv", "hr", "Croatian", "hrv"),
            new Iso639Dash2LanguageCode("hun", "hu", "Hungarian", "hun"),
            new Iso639Dash2LanguageCode("hye", "hy", "Armenian", "arm"),
            new Iso639Dash2LanguageCode("ibo", "ig", "Igbo", "ibo"),
            new Iso639Dash2LanguageCode("ido", "io", "Ido", "ido"),
            new Iso639Dash2LanguageCode("iii", "ii", "Sichuan Yi", "iii"),
            new Iso639Dash2LanguageCode("iku", "iu", "Inuktitut", "iku"),
            new Iso639Dash2LanguageCode("ile", "ie", "Interlingue", "ile"),
            new Iso639Dash2LanguageCode("ina", "ia", "Interlingua(International Auxiliary Language Association)", "ina"),
            new Iso639Dash2LanguageCode("ind", "id", "Indonesian", "ind"),
            new Iso639Dash2LanguageCode("ipk", "ik", "Inupiaq", "ipk"),
            new Iso639Dash2LanguageCode("isl", "is", "Icelandic", "ice"),
            new Iso639Dash2LanguageCode("ita", "it", "Italian", "ita"),
            new Iso639Dash2LanguageCode("jav", "jv", "Javanese", "jav"),
            new Iso639Dash2LanguageCode("jpn", "ja", "Japanese", "jpn"),
            new Iso639Dash2LanguageCode("kal", "kl", "Kalaallisut", "kal"),
            new Iso639Dash2LanguageCode("kan", "kn", "Kannada", "kan"),
            new Iso639Dash2LanguageCode("kas", "ks", "Kashmiri", "kas"),
            new Iso639Dash2LanguageCode("kat", "ka", "Georgian", "geo"),
            new Iso639Dash2LanguageCode("kau", "kr", "Kanuri", "kau"),
            new Iso639Dash2LanguageCode("kaz", "kk", "Kazakh", "kaz"),
            new Iso639Dash2LanguageCode("khm", "km", "Central Khmer", "khm"),
            new Iso639Dash2LanguageCode("kik", "ki", "Kikuyu; Gikuyu", "kik"),
            new Iso639Dash2LanguageCode("kin", "rw", "Kinyarwanda", "kin"),
            new Iso639Dash2LanguageCode("kir", "ky", "Kirghiz; Kyrgyz", "kir"),
            new Iso639Dash2LanguageCode("kom", "kv", "Komi", "kom"),
            new Iso639Dash2LanguageCode("kon", "kg", "Kongo", "kon"),
            new Iso639Dash2LanguageCode("kor", "ko", "Korean", "kor"),
            new Iso639Dash2LanguageCode("kua", "kj", "Kuanyama; Kwanyama", "kua"),
            new Iso639Dash2LanguageCode("kur", "ku", "Kurdish", "kur"),
            new Iso639Dash2LanguageCode("lao", "lo", "Lao", "lao"),
            new Iso639Dash2LanguageCode("lat", "la", "Latin", "lat"),
            new Iso639Dash2LanguageCode("lav", "lv", "Latvian", "lav"),
            new Iso639Dash2LanguageCode("lim", "li", "Limburgan", "lim"),
            new Iso639Dash2LanguageCode("lin", "ln", "Lingala", "lin"),
            new Iso639Dash2LanguageCode("lit", "lt", "Lithuanian", "lit"),
            new Iso639Dash2LanguageCode("ltz", "lb", "Luxembourgish", "ltz"),
            new Iso639Dash2LanguageCode("lub", "lu", "Luba-Katanga", "lub"),
            new Iso639Dash2LanguageCode("lug", "lg", "Ganda", "lug"),
            new Iso639Dash2LanguageCode("mah", "mh", "Marshallese", "mah"),
            new Iso639Dash2LanguageCode("mal", "ml", "Malayalam", "mal"),
            new Iso639Dash2LanguageCode("mar", "mr", "Marathi", "mar"),
            new Iso639Dash2LanguageCode("mkd", "mk", "Macedonian", "mac"),
            new Iso639Dash2LanguageCode("mlg", "mg", "Malagasy", "mlg"),
            new Iso639Dash2LanguageCode("mlt", "mt", "Maltese", "mlt"),
            new Iso639Dash2LanguageCode("mon", "mn", "Mongolian", "mon"),
            new Iso639Dash2LanguageCode("mri", "mi", "Maori", "mao"),
            new Iso639Dash2LanguageCode("msa", "ms", "Malay", "may"),
            new Iso639Dash2LanguageCode("mya", "my", "Burmese", "bur"),
            new Iso639Dash2LanguageCode("nau", "na", "Nauru", "nau"),
            new Iso639Dash2LanguageCode("nav", "nv", "Navajo", "nav"),
            new Iso639Dash2LanguageCode("nbl", "nr", "Ndebele, South", "nbl"),
            new Iso639Dash2LanguageCode("nde", "nd", "Ndebele, North", "nde"),
            new Iso639Dash2LanguageCode("ndo", "ng", "Ndonga", "ndo"),
            new Iso639Dash2LanguageCode("nep", "ne", "Nepali", "nep"),
            new Iso639Dash2LanguageCode("nld", "nl", "Dutch", "dut"),
            new Iso639Dash2LanguageCode("nno", "nn", "Norwegian Nynorsk", "nno"),
            new Iso639Dash2LanguageCode("nob", "nb", "Bokmål, Norwegian", "nob"),
            new Iso639Dash2LanguageCode("nor", "no", "Norwegian", "nor"),
            new Iso639Dash2LanguageCode("nya", "ny", "Chichewa", "nya"),
            new Iso639Dash2LanguageCode("oci", "oc", "Occitan", "oci"),
            new Iso639Dash2LanguageCode("oji", "oj", "Ojibwa", "oji"),
            new Iso639Dash2LanguageCode("ori", "or", "Oriya", "ori"),
            new Iso639Dash2LanguageCode("orm", "om", "Oromo", "orm"),
            new Iso639Dash2LanguageCode("oss", "os", "Ossetian; Ossetic", "oss"),
            new Iso639Dash2LanguageCode("pan", "pa", "Panjabi", "pan"),
            new Iso639Dash2LanguageCode("pli", "pi", "Pali", "pli"),
            new Iso639Dash2LanguageCode("pol", "pl", "Polish", "pol"),
            new Iso639Dash2LanguageCode("por", "pt", "Portuguese", "por"),
            new Iso639Dash2LanguageCode("pus", "ps", "Pushto; Pashto", "pus"),
            new Iso639Dash2LanguageCode("que", "qu", "Quechua", "que"),
            new Iso639Dash2LanguageCode("roh", "rm", "Romansh", "roh"),
            new Iso639Dash2LanguageCode("ron", "ro", "Romanian", "rum"),
            new Iso639Dash2LanguageCode("run", "rn", "Rundi", "run"),
            new Iso639Dash2LanguageCode("rus", "ru", "Russian", "rus"),
            new Iso639Dash2LanguageCode("sag", "sg", "Sango", "sag"),
            new Iso639Dash2LanguageCode("san", "sa", "Sanskrit", "san"),
            new Iso639Dash2LanguageCode("sin", "si", "Sinhala", "sin"),
            new Iso639Dash2LanguageCode("slk", "sk", "Slovak", "slo"),
            new Iso639Dash2LanguageCode("slv", "sl", "Slovenian", "slv"),
            new Iso639Dash2LanguageCode("sme", "se", "Northern Sami", "sme"),
            new Iso639Dash2LanguageCode("smo", "sm", "Samoan", "smo"),
            new Iso639Dash2LanguageCode("sna", "sn", "Shona", "sna"),
            new Iso639Dash2LanguageCode("snd", "sd", "Sindhi", "snd"),
            new Iso639Dash2LanguageCode("som", "so", "Somali", "som"),
            new Iso639Dash2LanguageCode("sot", "st", "Sotho, Southern", "sot"),
            new Iso639Dash2LanguageCode("spa", "es", "Spanish; Castilian", "spa"),
            new Iso639Dash2LanguageCode("sqi", "sq", "Albanian", "alb"),
            new Iso639Dash2LanguageCode("srd", "sc", "Sardinian", "srd"),
            new Iso639Dash2LanguageCode("srp", "sr", "Serbian", "srp"),
            new Iso639Dash2LanguageCode("ssw", "ss", "Swati", "ssw"),
            new Iso639Dash2LanguageCode("sun", "su", "Sundanese", "sun"),
            new Iso639Dash2LanguageCode("swa", "sw", "Swahili", "swa"),
            new Iso639Dash2LanguageCode("swe", "sv", "Swedish", "swe"),
            new Iso639Dash2LanguageCode("tah", "ty", "Tahitian", "tah"),
            new Iso639Dash2LanguageCode("tam", "ta", "Tamil", "tam"),
            new Iso639Dash2LanguageCode("tat", "tt", "Tatar", "tat"),
            new Iso639Dash2LanguageCode("tel", "te", "Telugu", "tel"),
            new Iso639Dash2LanguageCode("tgk", "tg", "Tajik", "tgk"),
            new Iso639Dash2LanguageCode("tgl", "tl", "Tagalog", "tgl"),
            new Iso639Dash2LanguageCode("tha", "th", "Thai", "tha"),
            new Iso639Dash2LanguageCode("tir", "ti", "Tigrinya", "tir"),
            new Iso639Dash2LanguageCode("ton", "to", "Tonga (Tonga Islands)", "ton"),
            new Iso639Dash2LanguageCode("tsn", "tn", "Tswana", "tsn"),
            new Iso639Dash2LanguageCode("tso", "ts", "Tsonga", "tso"),
            new Iso639Dash2LanguageCode("tuk", "tk", "Turkmen", "tuk"),
            new Iso639Dash2LanguageCode("tur", "tr", "Turkish", "tur"),
            new Iso639Dash2LanguageCode("twi", "tw", "Twi", "twi"),
            new Iso639Dash2LanguageCode("uig", "ug", "Uighur", "uig"),
            new Iso639Dash2LanguageCode("ukr", "uk", "Ukrainian", "ukr"),
            new Iso639Dash2LanguageCode("urd", "ur", "Urdu", "urd"),
            new Iso639Dash2LanguageCode("uzb", "uz", "Uzbek", "uzb"),
            new Iso639Dash2LanguageCode("ven", "ve", "Venda", "ven"),
            new Iso639Dash2LanguageCode("vie", "vi", "Vietnamese", "vie"),
            new Iso639Dash2LanguageCode("vol", "vo", "Volapük", "vol"),
            new Iso639Dash2LanguageCode("wln", "wa", "Walloon", "wln"),
            new Iso639Dash2LanguageCode("wol", "wo", "Wolof", "wol"),
            new Iso639Dash2LanguageCode("xho", "xh", "Xhosa", "xho"),
            new Iso639Dash2LanguageCode("yid", "yi", "Yiddish", "yid"),
            new Iso639Dash2LanguageCode("yor", "yo", "Yoruba", "yor"),
            new Iso639Dash2LanguageCode("zha", "za", "Zhuang", "zha"),
            new Iso639Dash2LanguageCode("zho", "zh", "Chinese", "chi"),
            new Iso639Dash2LanguageCode("zul", "zu", "Zulu", "zul"),
        };

        /// <summary>
        /// Get three letter language code, from two letter language code.
        /// </summary>
        /// <param name="twoLetterCode">Two letter language code (casing not important)</param>
        /// <returns>Three letter language code in lowercase, string.Empty if not found</returns>
        public static string GetThreeLetterCodeFromTwoLetterCode(string twoLetterCode)
        {
            if (string.IsNullOrEmpty(twoLetterCode))
            {
                return string.Empty;
            }

            var lookupResult = List.FirstOrDefault(p => p.TwoLetterCode == twoLetterCode.ToLowerInvariant());
            return lookupResult == null ? string.Empty : lookupResult.ThreeLetterCode;
        }

        /// <summary>
        /// Get two letter language code, from three letter language code. Matches both
        /// the ISO 639-2/T (terminology) and 639-2/B (bibliographic) form, so e.g.
        /// "fre" and "fra" both resolve to "fr".
        /// </summary>
        /// <param name="threeLetterCode">Three letter language code (casing not important)</param>
        /// <returns>Two letter language code in lowercase, string.Empty if not found</returns>
        public static string GetTwoLetterCodeFromThreeLetterCode(string threeLetterCode)
        {
            if (string.IsNullOrEmpty(threeLetterCode))
            {
                return string.Empty;
            }

            var normalized = threeLetterCode.ToLowerInvariant();
            var lookupResult = List.FirstOrDefault(p =>
                p.ThreeLetterCode == normalized || p.BibliographicCode == normalized);
            return lookupResult == null ? string.Empty : lookupResult.TwoLetterCode;
        }

        /// <summary>
        /// Get the ISO 639-2/B (bibliographic) three letter language code, from a two letter
        /// language code. Returns the terminology code when there is no separate bibliographic
        /// form.
        /// </summary>
        /// <param name="twoLetterCode">Two letter language code (casing not important)</param>
        /// <returns>Three letter bibliographic code in lowercase, string.Empty if not found</returns>
        public static string GetThreeLetterBibliographicCodeFromTwoLetterCode(string twoLetterCode)
        {
            if (string.IsNullOrEmpty(twoLetterCode))
            {
                return string.Empty;
            }

            var lookupResult = List.FirstOrDefault(p => p.TwoLetterCode == twoLetterCode.ToLowerInvariant());
            return lookupResult == null ? string.Empty : lookupResult.BibliographicCode;
        }

        public static string GetTwoLetterCodeFromEnglishName(string englishName)
        {
            if (string.IsNullOrEmpty(englishName))
            {
                return string.Empty;
            }

            var lookupResult = List.FirstOrDefault(p => string.Equals(p.EnglishName, englishName, StringComparison.InvariantCultureIgnoreCase));
            return lookupResult == null ? string.Empty : lookupResult.TwoLetterCode;
        }
    }
}
