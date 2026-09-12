using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Romanize
{
    /// <summary>
    /// Converts Ge'ez script (Ethiopic, U+1200-U+137F) text to a Latin transliteration
    /// for readability. Covers Amharic, Tigrinya, and Ge'ez itself, since they all share
    /// the same core Ethiopic syllabary.
    ///
    /// Unlike Cyrillic, Ethiopic is a syllabic script (an abugida): each character already
    /// represents a full consonant+vowel syllable (e.g. "ለ" = "le", "ላ" = "laa"), so there is
    /// no case or "previous letter" logic involved - it's a direct character-to-syllable lookup.
    ///
    /// Punctuation (U+1361-U+1368) and the digits 1-9 (U+1369-U+1371) are also mapped.
    /// The multiplicative Ethiopic numerals (ten, twenty, hundred, ten-thousand, U+1372-U+137C)
    /// are intentionally left untouched since they don't have a simple 1:1 Latin equivalent -
    /// romanizing them correctly requires interpreting the whole numeral, not a single glyph.
    /// </summary>
    public class GeezRomanizer : IRomanizer
    {
        public const char CharLowerBound = '\u1200';
        public const char CharUpperBound = '\u137F';

        private static readonly Dictionary<char, string> SyllableMap = new Dictionary<char, string>
        {
            // Ha family
            ['\u1200'] = "ha",
            ['\u1201'] = "hu",
            ['\u1202'] = "hi",
            ['\u1203'] = "haa",
            ['\u1204'] = "hee",
            ['\u1205'] = "he",
            ['\u1206'] = "ho",
            ['\u1207'] = "hoa",

            // La family
            ['\u1208'] = "la",
            ['\u1209'] = "lu",
            ['\u120A'] = "li",
            ['\u120B'] = "laa",
            ['\u120C'] = "lee",
            ['\u120D'] = "le",
            ['\u120E'] = "lo",
            ['\u120F'] = "lwa",

            // Hha family
            ['\u1210'] = "hha",
            ['\u1211'] = "hhu",
            ['\u1212'] = "hhi",
            ['\u1213'] = "hhaa",
            ['\u1214'] = "hhee",
            ['\u1215'] = "hhe",
            ['\u1216'] = "hho",
            ['\u1217'] = "hhwa",

            // Ma family
            ['\u1218'] = "ma",
            ['\u1219'] = "mu",
            ['\u121A'] = "mi",
            ['\u121B'] = "maa",
            ['\u121C'] = "mee",
            ['\u121D'] = "me",
            ['\u121E'] = "mo",
            ['\u121F'] = "mwa",

            // Sza family
            ['\u1220'] = "sza",
            ['\u1221'] = "szu",
            ['\u1222'] = "szi",
            ['\u1223'] = "szaa",
            ['\u1224'] = "szee",
            ['\u1225'] = "sze",
            ['\u1226'] = "szo",
            ['\u1227'] = "szwa",

            // Ra family
            ['\u1228'] = "ra",
            ['\u1229'] = "ru",
            ['\u122A'] = "ri",
            ['\u122B'] = "raa",
            ['\u122C'] = "ree",
            ['\u122D'] = "re",
            ['\u122E'] = "ro",
            ['\u122F'] = "rwa",

            // Sa family
            ['\u1230'] = "sa",
            ['\u1231'] = "su",
            ['\u1232'] = "si",
            ['\u1233'] = "saa",
            ['\u1234'] = "see",
            ['\u1235'] = "se",
            ['\u1236'] = "so",
            ['\u1237'] = "swa",

            // Sha family
            ['\u1238'] = "sha",
            ['\u1239'] = "shu",
            ['\u123A'] = "shi",
            ['\u123B'] = "shaa",
            ['\u123C'] = "shee",
            ['\u123D'] = "she",
            ['\u123E'] = "sho",
            ['\u123F'] = "shwa",

            // Qa family (+ labialized Qwa sub-series)
            ['\u1240'] = "qa",
            ['\u1241'] = "qu",
            ['\u1242'] = "qi",
            ['\u1243'] = "qaa",
            ['\u1244'] = "qee",
            ['\u1245'] = "qe",
            ['\u1246'] = "qo",
            ['\u1247'] = "qoa",
            ['\u1248'] = "qwa",
            ['\u124A'] = "qwi",
            ['\u124B'] = "qwaa",
            ['\u124C'] = "qwee",
            ['\u124D'] = "qwe",

            // Qha family (+ labialized Qhwa sub-series)
            ['\u1250'] = "qha",
            ['\u1251'] = "qhu",
            ['\u1252'] = "qhi",
            ['\u1253'] = "qhaa",
            ['\u1254'] = "qhee",
            ['\u1255'] = "qhe",
            ['\u1256'] = "qho",
            ['\u1258'] = "qhwa",
            ['\u125A'] = "qhwi",
            ['\u125B'] = "qhwaa",
            ['\u125C'] = "qhwee",
            ['\u125D'] = "qhwe",

            // Ba family
            ['\u1260'] = "ba",
            ['\u1261'] = "bu",
            ['\u1262'] = "bi",
            ['\u1263'] = "baa",
            ['\u1264'] = "bee",
            ['\u1265'] = "be",
            ['\u1266'] = "bo",
            ['\u1267'] = "bwa",

            // Va family
            ['\u1268'] = "va",
            ['\u1269'] = "vu",
            ['\u126A'] = "vi",
            ['\u126B'] = "vaa",
            ['\u126C'] = "vee",
            ['\u126D'] = "ve",
            ['\u126E'] = "vo",
            ['\u126F'] = "vwa",

            // Ta family
            ['\u1270'] = "ta",
            ['\u1271'] = "tu",
            ['\u1272'] = "ti",
            ['\u1273'] = "taa",
            ['\u1274'] = "tee",
            ['\u1275'] = "te",
            ['\u1276'] = "to",
            ['\u1277'] = "twa",

            // Ca family
            ['\u1278'] = "ca",
            ['\u1279'] = "cu",
            ['\u127A'] = "ci",
            ['\u127B'] = "caa",
            ['\u127C'] = "cee",
            ['\u127D'] = "ce",
            ['\u127E'] = "co",
            ['\u127F'] = "cwa",

            // Xa family (+ labialized Xwa sub-series)
            ['\u1280'] = "xa",
            ['\u1281'] = "xu",
            ['\u1282'] = "xi",
            ['\u1283'] = "xaa",
            ['\u1284'] = "xee",
            ['\u1285'] = "xe",
            ['\u1286'] = "xo",
            ['\u1287'] = "xoa",
            ['\u1288'] = "xwa",
            ['\u128A'] = "xwi",
            ['\u128B'] = "xwaa",
            ['\u128C'] = "xwee",
            ['\u128D'] = "xwe",

            // Na family
            ['\u1290'] = "na",
            ['\u1291'] = "nu",
            ['\u1292'] = "ni",
            ['\u1293'] = "naa",
            ['\u1294'] = "nee",
            ['\u1295'] = "ne",
            ['\u1296'] = "no",
            ['\u1297'] = "nwa",

            // Nya family
            ['\u1298'] = "nya",
            ['\u1299'] = "nyu",
            ['\u129A'] = "nyi",
            ['\u129B'] = "nyaa",
            ['\u129C'] = "nyee",
            ['\u129D'] = "nye",
            ['\u129E'] = "nyo",
            ['\u129F'] = "nywa",

            // Glottal (Alef) family
            ['\u12A0'] = "a",
            ['\u12A1'] = "u",
            ['\u12A2'] = "i",
            ['\u12A3'] = "aa",
            ['\u12A4'] = "ee",
            ['\u12A5'] = "e",
            ['\u12A6'] = "o",
            ['\u12A7'] = "wa",

            // Ka family (+ labialized Kwa sub-series)
            ['\u12A8'] = "ka",
            ['\u12A9'] = "ku",
            ['\u12AA'] = "ki",
            ['\u12AB'] = "kaa",
            ['\u12AC'] = "kee",
            ['\u12AD'] = "ke",
            ['\u12AE'] = "ko",
            ['\u12AF'] = "koa",
            ['\u12B0'] = "kwa",
            ['\u12B2'] = "kwi",
            ['\u12B3'] = "kwaa",
            ['\u12B4'] = "kwee",
            ['\u12B5'] = "kwe",

            // Kxa family (+ labialized Kxwa sub-series)
            ['\u12B8'] = "kxa",
            ['\u12B9'] = "kxu",
            ['\u12BA'] = "kxi",
            ['\u12BB'] = "kxaa",
            ['\u12BC'] = "kxee",
            ['\u12BD'] = "kxe",
            ['\u12BE'] = "kxo",
            ['\u12C0'] = "kxwa",
            ['\u12C2'] = "kxwi",
            ['\u12C3'] = "kxwaa",
            ['\u12C4'] = "kxwee",
            ['\u12C5'] = "kxwe",

            // Wa family
            ['\u12C8'] = "wa",
            ['\u12C9'] = "wu",
            ['\u12CA'] = "wi",
            ['\u12CB'] = "waa",
            ['\u12CC'] = "wee",
            ['\u12CD'] = "we",
            ['\u12CE'] = "wo",
            ['\u12CF'] = "woa",

            // Pharyngeal (Ayn) family
            ['\u12D0'] = "a",
            ['\u12D1'] = "u",
            ['\u12D2'] = "i",
            ['\u12D3'] = "aa",
            ['\u12D4'] = "ee",
            ['\u12D5'] = "e",
            ['\u12D6'] = "o",

            // Za family
            ['\u12D8'] = "za",
            ['\u12D9'] = "zu",
            ['\u12DA'] = "zi",
            ['\u12DB'] = "zaa",
            ['\u12DC'] = "zee",
            ['\u12DD'] = "ze",
            ['\u12DE'] = "zo",
            ['\u12DF'] = "zwa",

            // Zha family
            ['\u12E0'] = "zha",
            ['\u12E1'] = "zhu",
            ['\u12E2'] = "zhi",
            ['\u12E3'] = "zhaa",
            ['\u12E4'] = "zhee",
            ['\u12E5'] = "zhe",
            ['\u12E6'] = "zho",
            ['\u12E7'] = "zhwa",

            // Ya family
            ['\u12E8'] = "ya",
            ['\u12E9'] = "yu",
            ['\u12EA'] = "yi",
            ['\u12EB'] = "yaa",
            ['\u12EC'] = "yee",
            ['\u12ED'] = "ye",
            ['\u12EE'] = "yo",
            ['\u12EF'] = "yoa",

            // Da family
            ['\u12F0'] = "da",
            ['\u12F1'] = "du",
            ['\u12F2'] = "di",
            ['\u12F3'] = "daa",
            ['\u12F4'] = "dee",
            ['\u12F5'] = "de",
            ['\u12F6'] = "do",
            ['\u12F7'] = "dwa",

            // Dda family
            ['\u12F8'] = "dda",
            ['\u12F9'] = "ddu",
            ['\u12FA'] = "ddi",
            ['\u12FB'] = "ddaa",
            ['\u12FC'] = "ddee",
            ['\u12FD'] = "dde",
            ['\u12FE'] = "ddo",
            ['\u12FF'] = "ddwa",

            // Ja family
            ['\u1300'] = "ja",
            ['\u1301'] = "ju",
            ['\u1302'] = "ji",
            ['\u1303'] = "jaa",
            ['\u1304'] = "jee",
            ['\u1305'] = "je",
            ['\u1306'] = "jo",
            ['\u1307'] = "jwa",

            // Ga family (+ labialized Gwa sub-series)
            ['\u1308'] = "ga",
            ['\u1309'] = "gu",
            ['\u130A'] = "gi",
            ['\u130B'] = "gaa",
            ['\u130C'] = "gee",
            ['\u130D'] = "ge",
            ['\u130E'] = "go",
            ['\u130F'] = "goa",
            ['\u1310'] = "gwa",
            ['\u1312'] = "gwi",
            ['\u1313'] = "gwaa",
            ['\u1314'] = "gwee",
            ['\u1315'] = "gwe",

            // Gga family
            ['\u1318'] = "gga",
            ['\u1319'] = "ggu",
            ['\u131A'] = "ggi",
            ['\u131B'] = "ggaa",
            ['\u131C'] = "ggee",
            ['\u131D'] = "gge",
            ['\u131E'] = "ggo",
            ['\u131F'] = "ggwaa",

            // Tha family
            ['\u1320'] = "tha",
            ['\u1321'] = "thu",
            ['\u1322'] = "thi",
            ['\u1323'] = "thaa",
            ['\u1324'] = "thee",
            ['\u1325'] = "the",
            ['\u1326'] = "tho",
            ['\u1327'] = "thwa",

            // Cha family
            ['\u1328'] = "cha",
            ['\u1329'] = "chu",
            ['\u132A'] = "chi",
            ['\u132B'] = "chaa",
            ['\u132C'] = "chee",
            ['\u132D'] = "che",
            ['\u132E'] = "cho",
            ['\u132F'] = "chwa",

            // Pha family
            ['\u1330'] = "pha",
            ['\u1331'] = "phu",
            ['\u1332'] = "phi",
            ['\u1333'] = "phaa",
            ['\u1334'] = "phee",
            ['\u1335'] = "phe",
            ['\u1336'] = "pho",
            ['\u1337'] = "phwa",

            // Tsa family
            ['\u1338'] = "tsa",
            ['\u1339'] = "tsu",
            ['\u133A'] = "tsi",
            ['\u133B'] = "tsaa",
            ['\u133C'] = "tsee",
            ['\u133D'] = "tse",
            ['\u133E'] = "tso",
            ['\u133F'] = "tswa",

            // Tza family
            ['\u1340'] = "tza",
            ['\u1341'] = "tzu",
            ['\u1342'] = "tzi",
            ['\u1343'] = "tzaa",
            ['\u1344'] = "tzee",
            ['\u1345'] = "tze",
            ['\u1346'] = "tzo",
            ['\u1347'] = "tzoa",

            // Fa family
            ['\u1348'] = "fa",
            ['\u1349'] = "fu",
            ['\u134A'] = "fi",
            ['\u134B'] = "faa",
            ['\u134C'] = "fee",
            ['\u134D'] = "fe",
            ['\u134E'] = "fo",
            ['\u134F'] = "fwa",

            // Pa family
            ['\u1350'] = "pa",
            ['\u1351'] = "pu",
            ['\u1352'] = "pi",
            ['\u1353'] = "paa",
            ['\u1354'] = "pee",
            ['\u1355'] = "pe",
            ['\u1356'] = "po",
            ['\u1357'] = "pwa",

            // Extra syllables used in loanwords / place names
            ['\u1358'] = "rya",
            ['\u1359'] = "mya",
            ['\u135A'] = "fya",
        };

        private static readonly Dictionary<char, string> PunctuationMap = new Dictionary<char, string>
        {
            ['\u1361'] = " ",   // wordspace
            ['\u1362'] = ".",   // full stop
            ['\u1363'] = ",",   // comma
            ['\u1364'] = ";",   // semicolon
            ['\u1365'] = ":",   // colon
            ['\u1366'] = ":-",  // preface colon
            ['\u1367'] = "?",   // question mark
        };

        private static readonly Dictionary<char, string> DigitMap = new Dictionary<char, string>
        {
            ['\u1369'] = "1",
            ['\u136A'] = "2",
            ['\u136B'] = "3",
            ['\u136C'] = "4",
            ['\u136D'] = "5",
            ['\u136E'] = "6",
            ['\u136F'] = "7",
            ['\u1370'] = "8",
            ['\u1371'] = "9",
        };

        RomanizerLanguages IRomanizer.Language { get; } = RomanizerLanguages.Geez;

        public bool IsValid(char chr)
        {
            return (chr >= CharLowerBound) && (chr <= CharUpperBound);
        }

        public bool IsValid(string text)
        {
            return !string.IsNullOrWhiteSpace(text) && text.Any(IsValid);
        }

        public string Romanize(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            var sb = new StringBuilder(text.Length * 2);
            foreach (var current in text)
            {
                if (SyllableMap.TryGetValue(current, out var translit))
                {
                    sb.Append(translit);
                    continue;
                }

                if (PunctuationMap.TryGetValue(current, out var punct))
                {
                    sb.Append(punct);
                    continue;
                }

                if (DigitMap.TryGetValue(current, out var digit))
                {
                    sb.Append(digit);
                    continue;
                }

                // Unmapped Ethiopic characters (e.g. gemination marks, the multiplicative
                // numerals for ten/twenty/hundred/etc., or characters outside the block)
                // are passed through unchanged.
                sb.Append(current);
            }

            return sb.ToString();
        }
    }
}