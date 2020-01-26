using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{

    /// <summary>
    /// http://www.theneitherworld.com/mcpoodle/SCC_TOOLS/DOCS/SCC_FORMAT.HTML
    /// § 15.119 47 CFR Ch. I (10–1–10 Edition) (pdf)
    /// Maximum four lines + max 32 characters on each line
    /// </summary>
    public class ScenaristClosedCaptions : SubtitleFormat
    {
        public class SccPositionAndStyle
        {
            public Color ForeColor { get; set; }
            public FontStyle Style { get; set; }
            public int X { get; set; }
            public int Y { get; set; }

            public SccPositionAndStyle(Color color, FontStyle style, int y, int x)
            {
                ForeColor = color;
                Style = style;
                X = x;
                Y = y;
            }
        }

        //00:01:00:29   9420 9420 94ae 94ae 94d0 94d0 4920 f761 7320 ...    semi colon (instead of colon) before frame number is used to indicate drop frame
        private const string TimeCodeRegEx = @"^\d+:\d\d:\d\d[:,]\d\d\t";
        private static readonly Regex Regex = new Regex(TimeCodeRegEx, RegexOptions.Compiled);
        protected virtual Regex RegexTimeCodes => Regex;
        protected bool DropFrame;

        private static readonly List<KeyValuePair<string, string>> LetterDictionary = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("20",                  " " ),
            new KeyValuePair<string, string>("a1",                  "!" ),
            new KeyValuePair<string, string>("a2",                  "\""),
            new KeyValuePair<string, string>("23",                  "#" ),
            new KeyValuePair<string, string>("a4",                  "$" ),
            new KeyValuePair<string, string>("25",                  "%" ),
            new KeyValuePair<string, string>("26",                  "&" ),
            new KeyValuePair<string, string>("a7",                  "'" ),
            new KeyValuePair<string, string>("a8",                  "(" ),
            new KeyValuePair<string, string>("29",                  ")" ),
            new KeyValuePair<string, string>("2a",                  "á" ),
            new KeyValuePair<string, string>("ab",                  "+" ),
            new KeyValuePair<string, string>("2c",                  "," ),
            new KeyValuePair<string, string>("ad",                  "-" ),
            new KeyValuePair<string, string>("ae",                  "." ),
            new KeyValuePair<string, string>("2f",                  "/" ),
            new KeyValuePair<string, string>("b0",                  "0" ),
            new KeyValuePair<string, string>("31",                  "1" ),
            new KeyValuePair<string, string>("32",                  "2" ),
            new KeyValuePair<string, string>("b3",                  "3" ),
            new KeyValuePair<string, string>("34",                  "4" ),
            new KeyValuePair<string, string>("b5",                  "5" ),
            new KeyValuePair<string, string>("b6",                  "6" ),
            new KeyValuePair<string, string>("37",                  "7" ),
            new KeyValuePair<string, string>("38",                  "8" ),
            new KeyValuePair<string, string>("b9",                  "9" ),
            new KeyValuePair<string, string>("ba",                  ":" ),
            new KeyValuePair<string, string>("3b",                  ";" ),
            new KeyValuePair<string, string>("bc",                  "<" ),
            new KeyValuePair<string, string>("3d",                  "=" ),
            new KeyValuePair<string, string>("3e",                  ">" ),
            new KeyValuePair<string, string>("bf",                  "?" ),
            new KeyValuePair<string, string>("40",                  "@" ),
            new KeyValuePair<string, string>("c1",                  "A" ),
            new KeyValuePair<string, string>("c2",                  "B" ),
            new KeyValuePair<string, string>("43",                  "C" ),
            new KeyValuePair<string, string>("c4",                  "D" ),
            new KeyValuePair<string, string>("45",                  "E" ),
            new KeyValuePair<string, string>("46",                  "F" ),
            new KeyValuePair<string, string>("c7",                  "G" ),
            new KeyValuePair<string, string>("c8",                  "H" ),
            new KeyValuePair<string, string>("49",                  "I" ),
            new KeyValuePair<string, string>("4a",                  "J" ),
            new KeyValuePair<string, string>("cb",                  "K" ),
            new KeyValuePair<string, string>("4c",                  "L" ),
            new KeyValuePair<string, string>("cd",                  "M" ),
            new KeyValuePair<string, string>("ce",                  "N" ),
            new KeyValuePair<string, string>("4f",                  "O" ),
            new KeyValuePair<string, string>("d0",                  "P" ),
            new KeyValuePair<string, string>("51",                  "Q" ),
            new KeyValuePair<string, string>("52",                  "R" ),
            new KeyValuePair<string, string>("d3",                  "S" ),
            new KeyValuePair<string, string>("54",                  "T" ),
            new KeyValuePair<string, string>("d5",                  "U" ),
            new KeyValuePair<string, string>("d6",                  "V" ),
            new KeyValuePair<string, string>("57",                  "W" ),
            new KeyValuePair<string, string>("58",                  "X" ),
            new KeyValuePair<string, string>("d9",                  "Y" ),
            new KeyValuePair<string, string>("da",                  "Z" ),
            new KeyValuePair<string, string>("5b",                  "[" ),
            new KeyValuePair<string, string>("dc",                  "é" ),
            new KeyValuePair<string, string>("5d",                  "]" ),
            new KeyValuePair<string, string>("5e",                  "í" ),
            new KeyValuePair<string, string>("df",                  "ó" ),
            new KeyValuePair<string, string>("e0",                  "ú" ),
            new KeyValuePair<string, string>("61",                  "a" ),
            new KeyValuePair<string, string>("62",                  "b" ),
            new KeyValuePair<string, string>("e3",                  "c" ),
            new KeyValuePair<string, string>("64",                  "d" ),
            new KeyValuePair<string, string>("e5",                  "e" ),
            new KeyValuePair<string, string>("e6",                  "f" ),
            new KeyValuePair<string, string>("67",                  "g" ),
            new KeyValuePair<string, string>("68",                  "h" ),
            new KeyValuePair<string, string>("e9",                  "i" ),
            new KeyValuePair<string, string>("ea",                  "j" ),
            new KeyValuePair<string, string>("6b",                  "k" ),
            new KeyValuePair<string, string>("ec",                  "l" ),
            new KeyValuePair<string, string>("6d",                  "m" ),
            new KeyValuePair<string, string>("ef",                  "o" ),
            new KeyValuePair<string, string>("70",                  "p" ),
            new KeyValuePair<string, string>("f1",                  "q" ),
            new KeyValuePair<string, string>("f2",                  "r" ),
            new KeyValuePair<string, string>("73",                  "s" ),
            new KeyValuePair<string, string>("f4",                  "t" ),
            new KeyValuePair<string, string>("75",                  "u" ),
            new KeyValuePair<string, string>("76",                  "v" ),
            new KeyValuePair<string, string>("f7",                  "w" ),
            new KeyValuePair<string, string>("f8",                  "x" ),
            new KeyValuePair<string, string>("fb",                  "ç" ),
            new KeyValuePair<string, string>("79",                  "y" ),
            new KeyValuePair<string, string>("7a",                  "z" ),
            new KeyValuePair<string, string>("7c",                  ""  ),
            new KeyValuePair<string, string>("fd",                  "Ñ" ),
            new KeyValuePair<string, string>("fe",                  "ñ" ),
            new KeyValuePair<string, string>("7f",                  "■" ),
            new KeyValuePair<string, string>("7b",                  "ç" ),
            new KeyValuePair<string, string>("63",                  "c" ),
            new KeyValuePair<string, string>("65",                  "e" ),
            new KeyValuePair<string, string>("66",                  "f" ),
            new KeyValuePair<string, string>("69",                  "i" ),
            new KeyValuePair<string, string>("6a",                  "j" ),
            new KeyValuePair<string, string>("6c",                  "l" ),
            new KeyValuePair<string, string>("6e",                  "n" ),
            new KeyValuePair<string, string>("6f",                  "o" ),
            new KeyValuePair<string, string>("71",                  "q" ),
            new KeyValuePair<string, string>("72",                  "r" ),
            new KeyValuePair<string, string>("74",                  "t" ),
            new KeyValuePair<string, string>("77",                  "w" ),
            new KeyValuePair<string, string>("78",                  "x" ),
            new KeyValuePair<string, string>("91b0",                ""  ),
            new KeyValuePair<string, string>("91b3",                ""  ),
            new KeyValuePair<string, string>("91b5",                ""  ),
            new KeyValuePair<string, string>("91b6",                "£" ),
            new KeyValuePair<string, string>("9137",                "♪" ),
            new KeyValuePair<string, string>("9138",                "à" ),
            new KeyValuePair<string, string>("91b9",                ""  ),
            new KeyValuePair<string, string>("91ba",                "è" ),
            new KeyValuePair<string, string>("913b",                "â" ),
            new KeyValuePair<string, string>("91bc",                "ê" ),
            new KeyValuePair<string, string>("913d",                "î" ),
            new KeyValuePair<string, string>("913e",                "ô" ),
            new KeyValuePair<string, string>("91bf",                "û" ),
            new KeyValuePair<string, string>("9220",                "Á" ),
            new KeyValuePair<string, string>("92a1",                "É" ),
            new KeyValuePair<string, string>("92a2",                "Ó" ),
            new KeyValuePair<string, string>("9223",                "Ú" ),
            new KeyValuePair<string, string>("92a4",                "Ü" ),
            new KeyValuePair<string, string>("9225",                "ü" ),
            new KeyValuePair<string, string>("9226",                "'" ),
            new KeyValuePair<string, string>("92a7",                "i" ),
            new KeyValuePair<string, string>("92a8",                "*" ),
            new KeyValuePair<string, string>("9229",                "'" ),
            new KeyValuePair<string, string>("922a",                "-" ),
            new KeyValuePair<string, string>("92ab",                ""  ),
            new KeyValuePair<string, string>("922c",                ""  ),
            new KeyValuePair<string, string>("92ad",                "\""),
            new KeyValuePair<string, string>("92ae",                "\""),
            new KeyValuePair<string, string>("922f",                ""  ),
            new KeyValuePair<string, string>("92b0",                "À" ),
            new KeyValuePair<string, string>("9231",                "Â" ),
            new KeyValuePair<string, string>("9232",                "Ç" ),
            new KeyValuePair<string, string>("92b3",                "È" ),
            new KeyValuePair<string, string>("9234",                "Ê" ),
            new KeyValuePair<string, string>("92b5",                "Ë" ),
            new KeyValuePair<string, string>("92b6",                "ë" ),
            new KeyValuePair<string, string>("9237",                "Î" ),
            new KeyValuePair<string, string>("9238",                "Ï" ),
            new KeyValuePair<string, string>("92b9",                "ï" ),
            new KeyValuePair<string, string>("923b",                "Ù" ),
            new KeyValuePair<string, string>("923d",                "Û" ),
            new KeyValuePair<string, string>("923e",                ""  ),
            new KeyValuePair<string, string>("92bf",                ""  ),
            new KeyValuePair<string, string>("1320",                "Ã" ),
            new KeyValuePair<string, string>("c1 1320",             "Ã" ),
            new KeyValuePair<string, string>("c180 1320",           "Ã" ),
            new KeyValuePair<string, string>("13a1",                "ã" ),
            new KeyValuePair<string, string>("80 13a1",             "ã" ),
            new KeyValuePair<string, string>("6180 13a1",           "ã" ),
            new KeyValuePair<string, string>("13a2",                "Í" ),
            new KeyValuePair<string, string>("49 13a2",             "Í" ),
            new KeyValuePair<string, string>("4980 13a2",           "Í" ),
            new KeyValuePair<string, string>("1323",                "Ì" ),
            new KeyValuePair<string, string>("49 1323",             "Ì" ),
            new KeyValuePair<string, string>("4980 1323",           "Ì" ),
            new KeyValuePair<string, string>("13a4",                "ì" ),
            new KeyValuePair<string, string>("e9 13a4",             "ì" ),
            new KeyValuePair<string, string>("e980 13a4",           "ì" ),
            new KeyValuePair<string, string>("1325",                "Ò" ),
            new KeyValuePair<string, string>("4f 1325",             "Ò" ),
            new KeyValuePair<string, string>("4f80 1325",           "Ò" ),
            new KeyValuePair<string, string>("1326",                "ò" ),
            new KeyValuePair<string, string>("ef 1326",             "ò" ),
            new KeyValuePair<string, string>("ef80 1326",           "ò" ),
            new KeyValuePair<string, string>("13a7",                "Õ" ),
            new KeyValuePair<string, string>("4f 13a7",             "Õ" ),
            new KeyValuePair<string, string>("4f80 13a7",           "Õ" ),
            new KeyValuePair<string, string>("13a8",                "õ" ),
            new KeyValuePair<string, string>("ef 13a8",             "õ" ),
            new KeyValuePair<string, string>("ef80 13a8",           "õ" ),
            new KeyValuePair<string, string>("1329",                "{" ),
            new KeyValuePair<string, string>("132a",                "}" ),
            new KeyValuePair<string, string>("13ab",                "\\"),
            new KeyValuePair<string, string>("132c",                "^" ),
            new KeyValuePair<string, string>("13ad",                "_" ),
            new KeyValuePair<string, string>("13ae",                "|" ),
            new KeyValuePair<string, string>("132f",                "~" ),
            new KeyValuePair<string, string>("13b0",                "Ä" ),
            new KeyValuePair<string, string>("c180 13b0",           "Ä" ),
            new KeyValuePair<string, string>("1331",                "ä" ),
            new KeyValuePair<string, string>("6180 1331",           "ä" ),
            new KeyValuePair<string, string>("1332",                "Ö" ),
            new KeyValuePair<string, string>("4f80 1332",           "Ö" ),
            new KeyValuePair<string, string>("13b3",                "ö" ),
            new KeyValuePair<string, string>("ef80 13b3",           "ö" ),
            new KeyValuePair<string, string>("1334",                ""  ),
            new KeyValuePair<string, string>("13b5",                ""  ),
            new KeyValuePair<string, string>("13b6",                ""  ),
            new KeyValuePair<string, string>("1337",                "|" ),
            new KeyValuePair<string, string>("1338",                "Å" ),
            new KeyValuePair<string, string>("13b9",                "å" ),
            new KeyValuePair<string, string>("4f80 13ba 13ba",      "Ø" ),
            new KeyValuePair<string, string>("133b",                "ø" ),
            new KeyValuePair<string, string>("133d",                ""  ),
            new KeyValuePair<string, string>("133e",                ""  ),
            new KeyValuePair<string, string>("13bf",                ""  ),
            new KeyValuePair<string, string>("9420",                ""  ), //9420=RCL, Resume Caption Loading
            new KeyValuePair<string, string>("94ae",                ""  ), //94ae=Clear Buffer
            new KeyValuePair<string, string>("942c",                ""  ), //942c=Clear Caption
            new KeyValuePair<string, string>("8080",                ""  ), //8080=Wait One Frame
            new KeyValuePair<string, string>("942f",                ""  ), //942f=Display Caption
            new KeyValuePair<string, string>("9440",                ""  ), //9440=? first sub?
            new KeyValuePair<string, string>("9452",                ""  ), //?
            new KeyValuePair<string, string>("9454",                ""  ), //?
            new KeyValuePair<string, string>("9470",                ""  ), //9470=?
            new KeyValuePair<string, string>("94d0",                ""  ), //94d0=?
            new KeyValuePair<string, string>("94d6",                ""  ), //94d6=?
            new KeyValuePair<string, string>("94f2",                ""  ),
            new KeyValuePair<string, string>("94f4",                ""  ),
            new KeyValuePair<string, string>("9723",                " " ), // ?
            new KeyValuePair<string, string>("97a1",                " " ), // ?
            new KeyValuePair<string, string>("97a2",                " " ), // ?
            new KeyValuePair<string, string>("1370",                ""  ), //1370=?
            new KeyValuePair<string, string>("13e0",                ""  ), //13e0=?
            new KeyValuePair<string, string>("13f2",                ""  ), //13f2=?
            new KeyValuePair<string, string>("136e",                ""  ), //136e=?
            new KeyValuePair<string, string>("94ce",                ""  ), //94ce=?
            new KeyValuePair<string, string>("2c2f",                ""  ), //?
            new KeyValuePair<string, string>("1130",                "®" ),
            new KeyValuePair<string, string>("1131",                "°" ),
            new KeyValuePair<string, string>("1132",                "½" ),
            new KeyValuePair<string, string>("1133",                "¿" ),
            new KeyValuePair<string, string>("1134",                "TM"),
            new KeyValuePair<string, string>("1135",                "¢" ),
            new KeyValuePair<string, string>("1136",                "£" ),
            new KeyValuePair<string, string>("1137",                "♪" ),
            new KeyValuePair<string, string>("1138",                "à" ),
            new KeyValuePair<string, string>("113a",                "è" ),
            new KeyValuePair<string, string>("113b",                "â" ),
            new KeyValuePair<string, string>("113c",                "ê" ),
            new KeyValuePair<string, string>("113d",                "î" ),
            new KeyValuePair<string, string>("113e",                "ô" ),
            new KeyValuePair<string, string>("113f",                "û" ),
            new KeyValuePair<string, string>("9130",                "®" ),
            new KeyValuePair<string, string>("9131",                "°" ),
            new KeyValuePair<string, string>("9132",                "½" ),
            new KeyValuePair<string, string>("9133",                "¿" ),
            new KeyValuePair<string, string>("9134",                "TM"),
            new KeyValuePair<string, string>("9135",                "¢" ),
            new KeyValuePair<string, string>("9136",                "£" ),
            new KeyValuePair<string, string>("913a",                "è" ),
            new KeyValuePair<string, string>("913c",                "ê" ),
            new KeyValuePair<string, string>("913f",                "û" ),
            new KeyValuePair<string, string>("a180 92a7 92a7",      "¡" ),
            new KeyValuePair<string, string>("92a7 92a7",           "¡" ),
            new KeyValuePair<string, string>("91b3 91b3",           "¿" ),

            new KeyValuePair<string, string>("6180 9138 9138",      "à"), //61=a
            new KeyValuePair<string, string>("9138 9138",           "à"),

            new KeyValuePair<string, string>("6180 913b 913b",      "â"),
            new KeyValuePair<string, string>("913b 913b",           "â"),

            new KeyValuePair<string, string>("6180 1331 1331",      "ä"),
            new KeyValuePair<string, string>("1331 1331",           "ä"),

            new KeyValuePair<string, string>("e580 91ba 91ba",      "è"),
            new KeyValuePair<string, string>("6180 91ba 91ba",      "è"),
            new KeyValuePair<string, string>("91ba 91ba",           "è"),

            new KeyValuePair<string, string>("e580 91bc 91bc",      "ê"),
            new KeyValuePair<string, string>("6180 91bc 91bc",      "ê"),
            new KeyValuePair<string, string>("91bc 91bc",           "ê"),


            new KeyValuePair<string, string>("e580 92b6 92b6",      "ë"), //e5=e (+65?)
            new KeyValuePair<string, string>("6580 92b6 92b6",      "ë"),
            new KeyValuePair<string, string>("92b6 92b6",           "ë"),

            new KeyValuePair<string, string>("e980 13a4 13a4",      "ì"), //e9 = i
            new KeyValuePair<string, string>("13a4 13a4",           "ì"),

            new KeyValuePair<string, string>("e980 913d 913d",      "î"),
            new KeyValuePair<string, string>("913d 913d",           "î"),

            new KeyValuePair<string, string>("e980 92b9 92b9",      "ï"),
            new KeyValuePair<string, string>("92b9 92b9",           "ï"),

            new KeyValuePair<string, string>("1326 1326",           "ò"), //o=ef or 6f
            new KeyValuePair<string, string>("ef80 1326 1326",      "ò"),
            new KeyValuePair<string, string>("6f80 1326 1326",      "ò"),

            new KeyValuePair<string, string>("913e 913e",           "ô"),
            new KeyValuePair<string, string>("ef80 913e 913e",      "ô"),
            new KeyValuePair<string, string>("6f80 913e 913e",      "ô"),

            new KeyValuePair<string, string>("13b3 13b3",           "ö"),
            new KeyValuePair<string, string>("ef80 13b3 13b3",      "ö"),
            new KeyValuePair<string, string>("6f80 13b3 13b3",      "ö"),

            new KeyValuePair<string, string>("7580 13b3 13b3",      "ù"), //u=75

            new KeyValuePair<string, string>("7580 92bc 92bc",      "ù"),
            new KeyValuePair<string, string>("92bc 92bc",           "ù"),

            new KeyValuePair<string, string>("7580 91bf 91bf",      "û"),
            new KeyValuePair<string, string>("91bf 91bf",           "û"),

            new KeyValuePair<string, string>("7580 9225 9225",      "ü"),
            new KeyValuePair<string, string>("9225 9225",           "ü"),

            new KeyValuePair<string, string>("4380 9232 9232",      "Ç"), //43=C
            new KeyValuePair<string, string>("9232 9232",           "Ç"),

            new KeyValuePair<string, string>("c180 1338 1338",      "Å"), //c1=A
            new KeyValuePair<string, string>("1338 1338",           "Å"),

            new KeyValuePair<string, string>("c180 92b0 92b0",      "À"),
            new KeyValuePair<string, string>("92b0 92b0",           "À"),

            new KeyValuePair<string, string>("c180 9220 9220",      "Á"),
            new KeyValuePair<string, string>("9220 9220",           "Á"),

            new KeyValuePair<string, string>("c180 9231 9231",      "Â"),
            new KeyValuePair<string, string>("9231 9231",           "Â"),

            new KeyValuePair<string, string>("c180 1320 1320",      "Ã"),
            new KeyValuePair<string, string>("1320 1320",           "Ã"),

            new KeyValuePair<string, string>("c180 13b0 13b0",      "Ä"),
            new KeyValuePair<string, string>("13b0 13b0",           "Ä"),

            new KeyValuePair<string, string>("4580 92b3 92b3",      "È"),
            new KeyValuePair<string, string>("92b3 92b3",           "È"),

            new KeyValuePair<string, string>("4580 92a1 92a1",      "É"),
            new KeyValuePair<string, string>("92a1 92a1",           "É"),

            new KeyValuePair<string, string>("4580 9234 9234",      "Ê"),
            new KeyValuePair<string, string>("9234 9234",           "Ê"),

            new KeyValuePair<string, string>("4580 92b5 92b5",      "Ë"),
            new KeyValuePair<string, string>("92b5 92b5",           "Ë"),

            new KeyValuePair<string, string>("4980 1323 1323",      "Ì"),
            new KeyValuePair<string, string>("1323 1323",           "Ì"),

            new KeyValuePair<string, string>("4980 13a2 13a2",      "Í"),
            new KeyValuePair<string, string>("13a2 13a2",           "Í"),

            new KeyValuePair<string, string>("4980 9237 9237",      "Î"),
            new KeyValuePair<string, string>("9237 9237",           "Î"),

            new KeyValuePair<string, string>("4980 9238 9238",      "Ï"),
            new KeyValuePair<string, string>("9238 9238",           "Ï"),

            new KeyValuePair<string, string>("4f80 92a2 92a2",      "Ó"), //4f=O
            new KeyValuePair<string, string>("92a2 92a2",           "Ó"),

            new KeyValuePair<string, string>("4f80 1325 1325",      "Ò"),
            new KeyValuePair<string, string>("1325 1325",           "Ò"),

            new KeyValuePair<string, string>("4f80 92ba 92ba",      "Ô"),
            new KeyValuePair<string, string>("92ba 92ba",           "Ô"),

            new KeyValuePair<string, string>("4f80 13a7 13a7",      "Õ"),
            new KeyValuePair<string, string>("13a7 13a7",           "Õ"),

            new KeyValuePair<string, string>("4f80 1332 1332",      "Ö"),
            new KeyValuePair<string, string>("1332 1332",           "Ö"),

            new KeyValuePair<string, string>("d580 9223 9223",      "Ú"),
            new KeyValuePair<string, string>("923d 923d",           "Û"),

            new KeyValuePair<string, string>("d580 923b 923b",      "Ù"),
            new KeyValuePair<string, string>("9223 9223",           "Ú"),

            new KeyValuePair<string, string>("d580 92a4 92a4",      "Ü"),
            new KeyValuePair<string, string>("92a4 92a4",           "Ü"),

            new KeyValuePair<string, string>("d580 923d 923d",      "Û"),
        };

        private static readonly Dictionary<string, string> LettersCodeLookup = LetterDictionary.ToDictionary(p => p.Key, p => p.Value);

        public override string Extension => ".scc";

        public override string Name => "Scenarist Closed Captions";

        private static string FixMax4LinesAndMax32CharsPerLine(string text, string language)
        {
            // fix attempt 1
            var lines = text.Trim().SplitToLines();
            if (IsAllOkay(lines))
            {
                return text;
            }

            // fix attempt 2
            text = Utilities.AutoBreakLine(text, 1, 4, language);
            lines = text.Trim().SplitToLines();
            if (IsAllOkay(lines))
            {
                return text;
            }

            // fix attempt 3
            text = AutoBreakLineMax4Lines(text, 32);
            lines = text.Trim().SplitToLines();
            if (IsAllOkay(lines))
            {
                return text;
            }

            var sb = new StringBuilder();
            int count = 0;
            foreach (string line in lines)
            {
                if (count < 4)
                {
                    sb.AppendLine(line.Length > 32 ? line.Substring(0, 32) : line);
                }
                count++;
            }
            return sb.ToString().Trim();
        }

        private static bool IsAllOkay(List<string> lines)
        {
            if (lines.Count > 4)
            {
                return false;
            }

            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Length > 32)
                {
                    return false;
                }
            }
            return true;
        }

        private static int GetLastIndexOfSpace(string s, int endCount)
        {
            var end = Math.Min(endCount, s.Length - 1);
            while (end > 0)
            {
                if (s[end] == ' ')
                {
                    return end;
                }

                end--;
            }
            return -1;
        }

        private static string AutoBreakLineMax4Lines(string text, int maxLength)
        {
            string s = text.Replace(Environment.NewLine, " ");
            s = s.Replace("  ", " ");
            var sb = new StringBuilder();
            int i = GetLastIndexOfSpace(s, maxLength);
            if (i > 0)
            {
                sb.AppendLine(s.Substring(0, i));
                s = s.Remove(0, i).Trim();
                i = s.Length <= maxLength ? s.Length : GetLastIndexOfSpace(s, maxLength);

                if (i > 0)
                {
                    sb.AppendLine(s.Substring(0, i));
                    s = s.Remove(0, i).Trim();
                    i = s.Length <= maxLength ? s.Length : GetLastIndexOfSpace(s, maxLength);

                    if (i > 0)
                    {
                        sb.AppendLine(s.Substring(0, i));
                        s = s.Remove(0, i).Trim();
                        i = s.Length <= maxLength ? s.Length : GetLastIndexOfSpace(s, maxLength);

                        if (i > 0)
                        {
                            sb.AppendLine(s.Substring(0, i));
                        }
                        else
                        {
                            sb.Append(s);
                        }
                    }
                    else
                    {
                        sb.Append(s);
                    }
                }
                return sb.ToString().Trim();
            }
            return text;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Scenarist_SCC V1.0");
            sb.AppendLine();
            string language = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                sb.AppendLine($"{ToTimeCode(p.StartTime.TotalMilliseconds)}\t94ae 94ae 9420 9420 {ToSccText(p.Text, language)} 942f 942f");
                sb.AppendLine();

                Paragraph next = subtitle.GetParagraphOrDefault(i + 1);
                if (next == null || Math.Abs(next.StartTime.TotalMilliseconds - p.EndTime.TotalMilliseconds) > 100)
                {
                    sb.AppendLine($"{ToTimeCode(p.EndTime.TotalMilliseconds)}\t942c 942c");
                    sb.AppendLine();
                }
            }
            return sb.ToString();
        }

        private static string ToSccText(string text, string language)
        {
            text = FixMax4LinesAndMax32CharsPerLine(text, language);
            var topAlign = text.StartsWith("{\\an7}", StringComparison.Ordinal) ||
                           text.StartsWith("{\\an8}", StringComparison.Ordinal) ||
                           text.StartsWith("{\\an9}", StringComparison.Ordinal);
            text = Utilities.RemoveSsaTags(text);
            var lines = text.Trim().SplitToLines();
            int italic = 0;
            var sb = new StringBuilder();
            int count = 1;
            foreach (string line in lines)
            {
                text = line.Trim();
                if (count > 0)
                {
                    sb.Append(' ');
                }

                sb.Append(GetCenterCodes(text, count, lines.Count, topAlign));
                count++;
                int i = 0;
                string code = string.Empty;
                if (italic > 0)
                {
                    sb.Append("91ae 91ae "); // italic
                }
                while (i < text.Length)
                {
                    string s = text.Substring(i, 1);
                    string codeFromLetter = GetCodeFromLetter(s);
                    string newCode;
                    if (text.Substring(i).StartsWith("<i>", StringComparison.Ordinal))
                    {
                        newCode = "91ae";
                        i += 2;
                        italic++;
                    }
                    else if (text.Substring(i).StartsWith("</i>", StringComparison.Ordinal) && italic > 0)
                    {
                        newCode = "9120";
                        i += 3;
                        italic--;
                    }
                    else if (text[i] == '’')
                    {
                        if (code.Length == 4)
                        {
                            sb.Append(code + " ");
                            code = string.Empty;
                        }
                        if (code.Length == 0)
                        {
                            code = "80";
                        }
                        if (code.Length == 2)
                        {
                            code += "a7";
                            sb.Append(code + " ");
                        }
                        code = "9229";
                        newCode = "";
                    }
                    else if (codeFromLetter == null)
                    {
                        newCode = GetCodeFromLetter(" ");
                    }
                    else
                    {
                        newCode = codeFromLetter;
                    }

                    if (code.Length == 2 && newCode.Length == 4)
                    {
                        code += "80";
                    }

                    if (code.Length == 4)
                    {
                        sb.Append(code + " ");
                        if (code.StartsWith('9') || code.StartsWith('8')) // control codes must be double
                        {
                            sb.Append(code + " ");
                        }

                        code = string.Empty;
                    }

                    if (code.Length == 2 && newCode.Length == 2)
                    {
                        code += newCode;
                        newCode = string.Empty;
                    }

                    if (newCode.Length == 4 && code.Length == 0)
                    {
                        code = newCode;
                    }
                    else if (newCode.Length == 2 && code.Length == 0)
                    {
                        code = newCode;
                    }
                    else if (newCode.Length > 4)
                    {
                        if (code.Length == 2)
                        {
                            code += "80";
                            sb.Append(code + " ");
                            if (code.StartsWith('9') || code.StartsWith('8')) // control codes must be double
                            {
                                sb.Append(code + " ");
                            }

                            code = string.Empty;
                        }
                        else if (code.Length == 4)
                        {
                            sb.Append(code + " ");
                            if (code.StartsWith('9') || code.StartsWith('8')) // control codes must be double
                            {
                                sb.Append(code + " ");
                            }

                            code = string.Empty;
                        }
                        sb.Append(newCode.TrimEnd() + " ");
                    }

                    i++;
                }
                if (code.Length == 2)
                {
                    code += "80";
                }

                if (code.Length == 4)
                {
                    sb.Append(code);
                }
            }

            return sb.ToString().Trim();
        }

        private static string GetCodeFromLetter(string letter)
        {
            var code = LetterDictionary.FirstOrDefault(x => x.Value == letter);
            if (code.Equals(new KeyValuePair<string, string>()))
            {
                return null;
            }

            return code.Key;
        }

        private static string GetLetterFromCode(string hexCode)
        {
            return LettersCodeLookup.TryGetValue(hexCode, out var letter) ? letter : null;
        }

        public static string GetCenterCodes(string text, int lineNumber, int totalLines, bool topAlign)
        {
            int row = 14 - (totalLines - lineNumber);
            //if (topAlign)
            //{
            //    row = 1;
            //}
            var rowCodes = new List<string> { "91", "91", "92", "92", "15", "15", "16", "16", "97", "97", "10", "13", "13", "94", "94" };
            string rowCode = rowCodes[row];

            int left = (32 - text.Length) / 2;
            int columnRest = left % 4;
            int column = left - columnRest;

            List<string> columnCodes;
            switch (column)
            {
                case 0:
                    columnCodes = new List<string> { "d0", "70", "d0", "70", "d0", "70", "d0", "70", "d0", "70", "d0", "d0", "70", "d0", "70" };
                    break;
                case 4:
                    columnCodes = new List<string> { "52", "f2", "52", "f2", "52", "f2", "52", "f2", "52", "f2", "52", "52", "f2", "52", "f2" };
                    break;
                case 8:
                    columnCodes = new List<string> { "54", "f4", "54", "f4", "54", "f4", "54", "f4", "54", "f4", "54", "54", "f4", "54", "f4" };
                    break;
                case 12:
                    columnCodes = new List<string> { "d6", "76", "d6", "76", "d6", "76", "d6", "76", "d6", "76", "d6", "d6", "76", "d6", "76" };
                    break;
                case 16:
                    columnCodes = new List<string> { "58", "f8", "58", "f8", "58", "f8", "58", "f8", "58", "f8", "58", "58", "f8", "58", "f8" };
                    break;
                case 20:
                    columnCodes = new List<string> { "da", "7a", "da", "7a", "da", "7a", "da", "7a", "da", "7a", "da", "da", "7a", "da", "7a" };
                    break;
                case 24:
                    columnCodes = new List<string> { "dc", "7c", "dc", "7c", "dc", "7c", "dc", "7c", "dc", "7c", "dc", "dc", "7c", "dc", "7c" };
                    break;
                default: // 28
                    columnCodes = new List<string> { "5e", "fe", "5e", "fe", "5e", "fe", "5e", "fe", "5e", "fe", "5e", "5e", "fe", "5e", "fe" };
                    break;
            }
            string code = rowCode + columnCodes[row];

            if (columnRest == 1)
            {
                return code + " " + code + " 97a1 97a1 ";
            }

            if (columnRest == 2)
            {
                return code + " " + code + " 97a2 97a2 ";
            }

            if (columnRest == 3)
            {
                return code + " " + code + " 9723 9723 ";
            }

            return code + " " + code + " ";
        }

        private string ToTimeCode(double totalMilliseconds)
        {
            TimeSpan ts = TimeSpan.FromMilliseconds(totalMilliseconds);
            if (DropFrame)
            {
                return $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00};{MillisecondsToFramesMaxFrameRate(ts.Milliseconds):00}";
            }

            return $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}:{MillisecondsToFramesMaxFrameRate(ts.Milliseconds):00}";
        }

        public static SccPositionAndStyle GetColorAndPosition(string code)
        {
            switch (code.ToLower(CultureInfo.InvariantCulture))
            {
                //NO x-coordinate?
                case "1140": return new SccPositionAndStyle(Color.White, FontStyle.Regular, 1, 0);
                case "1160": return new SccPositionAndStyle(Color.White, FontStyle.Regular, 2, 0);
                case "1240": return new SccPositionAndStyle(Color.White, FontStyle.Regular, 3, 0);
                case "1260": return new SccPositionAndStyle(Color.White, FontStyle.Regular, 4, 0);
                case "1540": return new SccPositionAndStyle(Color.White, FontStyle.Regular, 5, 0);
                case "1560": return new SccPositionAndStyle(Color.White, FontStyle.Regular, 6, 0);
                case "1640": return new SccPositionAndStyle(Color.White, FontStyle.Regular, 7, 0);
                case "1660": return new SccPositionAndStyle(Color.White, FontStyle.Regular, 8, 0);
                case "1740": return new SccPositionAndStyle(Color.White, FontStyle.Regular, 9, 0);
                case "1760": return new SccPositionAndStyle(Color.White, FontStyle.Regular, 10, 0);
                case "1040": return new SccPositionAndStyle(Color.White, FontStyle.Regular, 11, 0);
                case "1340": return new SccPositionAndStyle(Color.White, FontStyle.Regular, 12, 0);
                case "1360": return new SccPositionAndStyle(Color.White, FontStyle.Regular, 13, 0);
                case "1440": return new SccPositionAndStyle(Color.White, FontStyle.Regular, 14, 0);
                case "1460": return new SccPositionAndStyle(Color.White, FontStyle.Regular, 15, 0);

                case "1141": return new SccPositionAndStyle(Color.White, FontStyle.Underline, 1, 0);
                case "1161": return new SccPositionAndStyle(Color.White, FontStyle.Underline, 2, 0);
                case "1241": return new SccPositionAndStyle(Color.White, FontStyle.Underline, 3, 0);
                case "1261": return new SccPositionAndStyle(Color.White, FontStyle.Underline, 4, 0);
                case "1541": return new SccPositionAndStyle(Color.White, FontStyle.Underline, 5, 0);
                case "1561": return new SccPositionAndStyle(Color.White, FontStyle.Underline, 6, 0);
                case "1641": return new SccPositionAndStyle(Color.White, FontStyle.Underline, 7, 0);
                case "1661": return new SccPositionAndStyle(Color.White, FontStyle.Underline, 8, 0);
                case "1741": return new SccPositionAndStyle(Color.White, FontStyle.Underline, 9, 0);
                case "1761": return new SccPositionAndStyle(Color.White, FontStyle.Underline, 10, 0);
                case "1041": return new SccPositionAndStyle(Color.White, FontStyle.Underline, 11, 0);
                case "1341": return new SccPositionAndStyle(Color.White, FontStyle.Underline, 12, 0);
                case "1361": return new SccPositionAndStyle(Color.White, FontStyle.Underline, 13, 0);
                case "1441": return new SccPositionAndStyle(Color.White, FontStyle.Underline, 14, 0);
                case "1461": return new SccPositionAndStyle(Color.White, FontStyle.Underline, 15, 0);

                case "1142": return new SccPositionAndStyle(Color.Green, FontStyle.Regular, 1, 0);
                case "1162": return new SccPositionAndStyle(Color.Green, FontStyle.Regular, 2, 0);
                case "1242": return new SccPositionAndStyle(Color.Green, FontStyle.Regular, 3, 0);
                case "1262": return new SccPositionAndStyle(Color.Green, FontStyle.Regular, 4, 0);
                case "1542": return new SccPositionAndStyle(Color.Green, FontStyle.Regular, 5, 0);
                case "1562": return new SccPositionAndStyle(Color.Green, FontStyle.Regular, 6, 0);
                case "1642": return new SccPositionAndStyle(Color.Green, FontStyle.Regular, 7, 0);
                case "1662": return new SccPositionAndStyle(Color.Green, FontStyle.Regular, 8, 0);
                case "1742": return new SccPositionAndStyle(Color.Green, FontStyle.Regular, 9, 0);
                case "1762": return new SccPositionAndStyle(Color.Green, FontStyle.Regular, 10, 0);
                case "1042": return new SccPositionAndStyle(Color.Green, FontStyle.Regular, 11, 0);
                case "1342": return new SccPositionAndStyle(Color.Green, FontStyle.Regular, 12, 0);
                case "1362": return new SccPositionAndStyle(Color.Green, FontStyle.Regular, 13, 0);
                case "1442": return new SccPositionAndStyle(Color.Green, FontStyle.Regular, 14, 0);
                case "1462": return new SccPositionAndStyle(Color.Green, FontStyle.Regular, 15, 0);

                case "1143": return new SccPositionAndStyle(Color.Green, FontStyle.Underline, 1, 0);
                case "1163": return new SccPositionAndStyle(Color.Green, FontStyle.Underline, 2, 0);
                case "1243": return new SccPositionAndStyle(Color.Green, FontStyle.Underline, 3, 0);
                case "1263": return new SccPositionAndStyle(Color.Green, FontStyle.Underline, 4, 0);
                case "1543": return new SccPositionAndStyle(Color.Green, FontStyle.Underline, 5, 0);
                case "1563": return new SccPositionAndStyle(Color.Green, FontStyle.Underline, 6, 0);
                case "1643": return new SccPositionAndStyle(Color.Green, FontStyle.Underline, 7, 0);
                case "1663": return new SccPositionAndStyle(Color.Green, FontStyle.Underline, 8, 0);
                case "1743": return new SccPositionAndStyle(Color.Green, FontStyle.Underline, 9, 0);
                case "1763": return new SccPositionAndStyle(Color.Green, FontStyle.Underline, 10, 0);
                case "1043": return new SccPositionAndStyle(Color.Green, FontStyle.Underline, 11, 0);
                case "1343": return new SccPositionAndStyle(Color.Green, FontStyle.Underline, 12, 0);
                case "1363": return new SccPositionAndStyle(Color.Green, FontStyle.Underline, 13, 0);
                case "1443": return new SccPositionAndStyle(Color.Green, FontStyle.Underline, 14, 0);
                case "1463": return new SccPositionAndStyle(Color.Green, FontStyle.Underline, 15, 0);

                case "1144": return new SccPositionAndStyle(Color.Blue, FontStyle.Regular, 1, 0);
                case "1164": return new SccPositionAndStyle(Color.Blue, FontStyle.Regular, 2, 0);
                case "1244": return new SccPositionAndStyle(Color.Blue, FontStyle.Regular, 3, 0);
                case "1264": return new SccPositionAndStyle(Color.Blue, FontStyle.Regular, 4, 0);
                case "1544": return new SccPositionAndStyle(Color.Blue, FontStyle.Regular, 5, 0);
                case "1564": return new SccPositionAndStyle(Color.Blue, FontStyle.Regular, 6, 0);
                case "1644": return new SccPositionAndStyle(Color.Blue, FontStyle.Regular, 7, 0);
                case "1664": return new SccPositionAndStyle(Color.Blue, FontStyle.Regular, 8, 0);
                case "1744": return new SccPositionAndStyle(Color.Blue, FontStyle.Regular, 9, 0);
                case "1764": return new SccPositionAndStyle(Color.Blue, FontStyle.Regular, 10, 0);
                case "1044": return new SccPositionAndStyle(Color.Blue, FontStyle.Regular, 11, 0);
                case "1344": return new SccPositionAndStyle(Color.Blue, FontStyle.Regular, 12, 0);
                case "1364": return new SccPositionAndStyle(Color.Blue, FontStyle.Regular, 13, 0);
                case "1444": return new SccPositionAndStyle(Color.Blue, FontStyle.Regular, 14, 0);
                case "1464": return new SccPositionAndStyle(Color.Blue, FontStyle.Regular, 15, 0);

                case "1145": return new SccPositionAndStyle(Color.Blue, FontStyle.Underline, 1, 0);
                case "1165": return new SccPositionAndStyle(Color.Blue, FontStyle.Underline, 2, 0);
                case "1245": return new SccPositionAndStyle(Color.Blue, FontStyle.Underline, 3, 0);
                case "1265": return new SccPositionAndStyle(Color.Blue, FontStyle.Underline, 4, 0);
                case "1545": return new SccPositionAndStyle(Color.Blue, FontStyle.Underline, 5, 0);
                case "1565": return new SccPositionAndStyle(Color.Blue, FontStyle.Underline, 6, 0);
                case "1645": return new SccPositionAndStyle(Color.Blue, FontStyle.Underline, 7, 0);
                case "1665": return new SccPositionAndStyle(Color.Blue, FontStyle.Underline, 8, 0);
                case "1745": return new SccPositionAndStyle(Color.Blue, FontStyle.Underline, 9, 0);
                case "1765": return new SccPositionAndStyle(Color.Blue, FontStyle.Underline, 10, 0);
                case "1045": return new SccPositionAndStyle(Color.Blue, FontStyle.Underline, 11, 0);
                case "1345": return new SccPositionAndStyle(Color.Blue, FontStyle.Underline, 12, 0);
                case "1365": return new SccPositionAndStyle(Color.Blue, FontStyle.Underline, 13, 0);
                case "1445": return new SccPositionAndStyle(Color.Blue, FontStyle.Underline, 14, 0);
                case "1465": return new SccPositionAndStyle(Color.Blue, FontStyle.Underline, 15, 0);

                case "1146": return new SccPositionAndStyle(Color.Cyan, FontStyle.Regular, 1, 0);
                case "1166": return new SccPositionAndStyle(Color.Cyan, FontStyle.Regular, 2, 0);
                case "1246": return new SccPositionAndStyle(Color.Cyan, FontStyle.Regular, 3, 0);
                case "1266": return new SccPositionAndStyle(Color.Cyan, FontStyle.Regular, 4, 0);
                case "1546": return new SccPositionAndStyle(Color.Cyan, FontStyle.Regular, 5, 0);
                case "1566": return new SccPositionAndStyle(Color.Cyan, FontStyle.Regular, 6, 0);
                case "1646": return new SccPositionAndStyle(Color.Cyan, FontStyle.Regular, 7, 0);
                case "1666": return new SccPositionAndStyle(Color.Cyan, FontStyle.Regular, 8, 0);
                case "1746": return new SccPositionAndStyle(Color.Cyan, FontStyle.Regular, 9, 0);
                case "1766": return new SccPositionAndStyle(Color.Cyan, FontStyle.Regular, 10, 0);
                case "1046": return new SccPositionAndStyle(Color.Cyan, FontStyle.Regular, 11, 0);
                case "1346": return new SccPositionAndStyle(Color.Cyan, FontStyle.Regular, 12, 0);
                case "1366": return new SccPositionAndStyle(Color.Cyan, FontStyle.Regular, 13, 0);
                case "1446": return new SccPositionAndStyle(Color.Cyan, FontStyle.Regular, 14, 0);
                case "1466": return new SccPositionAndStyle(Color.Cyan, FontStyle.Regular, 15, 0);

                case "1147": return new SccPositionAndStyle(Color.Cyan, FontStyle.Underline, 1, 0);
                case "1167": return new SccPositionAndStyle(Color.Cyan, FontStyle.Underline, 2, 0);
                case "1247": return new SccPositionAndStyle(Color.Cyan, FontStyle.Underline, 3, 0);
                case "1267": return new SccPositionAndStyle(Color.Cyan, FontStyle.Underline, 4, 0);
                case "1547": return new SccPositionAndStyle(Color.Cyan, FontStyle.Underline, 5, 0);
                case "1567": return new SccPositionAndStyle(Color.Cyan, FontStyle.Underline, 6, 0);
                case "1647": return new SccPositionAndStyle(Color.Cyan, FontStyle.Underline, 7, 0);
                case "1667": return new SccPositionAndStyle(Color.Cyan, FontStyle.Underline, 8, 0);
                case "1747": return new SccPositionAndStyle(Color.Cyan, FontStyle.Underline, 9, 0);
                case "1767": return new SccPositionAndStyle(Color.Cyan, FontStyle.Underline, 10, 0);
                case "1047": return new SccPositionAndStyle(Color.Cyan, FontStyle.Underline, 11, 0);
                case "1347": return new SccPositionAndStyle(Color.Cyan, FontStyle.Underline, 12, 0);
                case "1367": return new SccPositionAndStyle(Color.Cyan, FontStyle.Underline, 13, 0);
                case "1447": return new SccPositionAndStyle(Color.Cyan, FontStyle.Underline, 14, 0);
                case "1467": return new SccPositionAndStyle(Color.Cyan, FontStyle.Underline, 15, 0);

                case "1148": return new SccPositionAndStyle(Color.Red, FontStyle.Regular, 1, 0);
                case "1168": return new SccPositionAndStyle(Color.Red, FontStyle.Regular, 2, 0);
                case "1248": return new SccPositionAndStyle(Color.Red, FontStyle.Regular, 3, 0);
                case "1268": return new SccPositionAndStyle(Color.Red, FontStyle.Regular, 4, 0);
                case "1548": return new SccPositionAndStyle(Color.Red, FontStyle.Regular, 5, 0);
                case "1568": return new SccPositionAndStyle(Color.Red, FontStyle.Regular, 6, 0);
                case "1648": return new SccPositionAndStyle(Color.Red, FontStyle.Regular, 7, 0);
                case "1668": return new SccPositionAndStyle(Color.Red, FontStyle.Regular, 8, 0);
                case "1748": return new SccPositionAndStyle(Color.Red, FontStyle.Regular, 9, 0);
                case "1768": return new SccPositionAndStyle(Color.Red, FontStyle.Regular, 10, 0);
                case "1048": return new SccPositionAndStyle(Color.Red, FontStyle.Regular, 11, 0);
                case "1348": return new SccPositionAndStyle(Color.Red, FontStyle.Regular, 12, 0);
                case "1368": return new SccPositionAndStyle(Color.Red, FontStyle.Regular, 13, 0);
                case "1448": return new SccPositionAndStyle(Color.Red, FontStyle.Regular, 14, 0);
                case "1468": return new SccPositionAndStyle(Color.Red, FontStyle.Regular, 15, 0);

                case "1149": return new SccPositionAndStyle(Color.Red, FontStyle.Underline, 1, 0);
                case "1169": return new SccPositionAndStyle(Color.Red, FontStyle.Underline, 2, 0);
                case "1249": return new SccPositionAndStyle(Color.Red, FontStyle.Underline, 3, 0);
                case "1269": return new SccPositionAndStyle(Color.Red, FontStyle.Underline, 4, 0);
                case "1549": return new SccPositionAndStyle(Color.Red, FontStyle.Underline, 5, 0);
                case "1569": return new SccPositionAndStyle(Color.Red, FontStyle.Underline, 6, 0);
                case "1649": return new SccPositionAndStyle(Color.Red, FontStyle.Underline, 7, 0);
                case "1669": return new SccPositionAndStyle(Color.Red, FontStyle.Underline, 8, 0);
                case "1749": return new SccPositionAndStyle(Color.Red, FontStyle.Underline, 9, 0);
                case "1769": return new SccPositionAndStyle(Color.Red, FontStyle.Underline, 10, 0);
                case "1049": return new SccPositionAndStyle(Color.Red, FontStyle.Underline, 11, 0);
                case "1349": return new SccPositionAndStyle(Color.Red, FontStyle.Underline, 12, 0);
                case "1369": return new SccPositionAndStyle(Color.Red, FontStyle.Underline, 13, 0);
                case "1449": return new SccPositionAndStyle(Color.Red, FontStyle.Underline, 14, 0);
                case "1469": return new SccPositionAndStyle(Color.Red, FontStyle.Underline, 15, 0);

                case "114a": return new SccPositionAndStyle(Color.Yellow, FontStyle.Regular, 1, 0);
                case "116a": return new SccPositionAndStyle(Color.Yellow, FontStyle.Regular, 2, 0);
                case "124a": return new SccPositionAndStyle(Color.Yellow, FontStyle.Regular, 3, 0);
                case "126a": return new SccPositionAndStyle(Color.Yellow, FontStyle.Regular, 4, 0);
                case "154a": return new SccPositionAndStyle(Color.Yellow, FontStyle.Regular, 5, 0);
                case "156a": return new SccPositionAndStyle(Color.Yellow, FontStyle.Regular, 6, 0);
                case "164a": return new SccPositionAndStyle(Color.Yellow, FontStyle.Regular, 7, 0);
                case "166a": return new SccPositionAndStyle(Color.Yellow, FontStyle.Regular, 8, 0);
                case "174a": return new SccPositionAndStyle(Color.Yellow, FontStyle.Regular, 9, 0);
                case "176a": return new SccPositionAndStyle(Color.Yellow, FontStyle.Regular, 10, 0);
                case "104a": return new SccPositionAndStyle(Color.Yellow, FontStyle.Regular, 11, 0);
                case "134a": return new SccPositionAndStyle(Color.Yellow, FontStyle.Regular, 12, 0);
                case "136a": return new SccPositionAndStyle(Color.Yellow, FontStyle.Regular, 13, 0);
                case "144a": return new SccPositionAndStyle(Color.Yellow, FontStyle.Regular, 14, 0);
                case "146a": return new SccPositionAndStyle(Color.Yellow, FontStyle.Regular, 15, 0);

                case "114b": return new SccPositionAndStyle(Color.Yellow, FontStyle.Underline, 1, 0);
                case "116b": return new SccPositionAndStyle(Color.Yellow, FontStyle.Underline, 2, 0);
                case "124b": return new SccPositionAndStyle(Color.Yellow, FontStyle.Underline, 3, 0);
                case "126b": return new SccPositionAndStyle(Color.Yellow, FontStyle.Underline, 4, 0);
                case "154b": return new SccPositionAndStyle(Color.Yellow, FontStyle.Underline, 5, 0);
                case "156b": return new SccPositionAndStyle(Color.Yellow, FontStyle.Underline, 6, 0);
                case "164b": return new SccPositionAndStyle(Color.Yellow, FontStyle.Underline, 7, 0);
                case "166b": return new SccPositionAndStyle(Color.Yellow, FontStyle.Underline, 8, 0);
                case "174b": return new SccPositionAndStyle(Color.Yellow, FontStyle.Underline, 9, 0);
                case "176b": return new SccPositionAndStyle(Color.Yellow, FontStyle.Underline, 10, 0);
                case "104b": return new SccPositionAndStyle(Color.Yellow, FontStyle.Underline, 11, 0);
                case "134b": return new SccPositionAndStyle(Color.Yellow, FontStyle.Underline, 12, 0);
                case "136b": return new SccPositionAndStyle(Color.Yellow, FontStyle.Underline, 13, 0);
                case "144b": return new SccPositionAndStyle(Color.Yellow, FontStyle.Underline, 14, 0);
                case "146b": return new SccPositionAndStyle(Color.Yellow, FontStyle.Underline, 15, 0);

                case "114c": return new SccPositionAndStyle(Color.Magenta, FontStyle.Regular, 1, 0);
                case "116c": return new SccPositionAndStyle(Color.Magenta, FontStyle.Regular, 2, 0);
                case "124c": return new SccPositionAndStyle(Color.Magenta, FontStyle.Regular, 3, 0);
                case "126c": return new SccPositionAndStyle(Color.Magenta, FontStyle.Regular, 4, 0);
                case "154c": return new SccPositionAndStyle(Color.Magenta, FontStyle.Regular, 5, 0);
                case "156c": return new SccPositionAndStyle(Color.Magenta, FontStyle.Regular, 6, 0);
                case "164c": return new SccPositionAndStyle(Color.Magenta, FontStyle.Regular, 7, 0);
                case "166c": return new SccPositionAndStyle(Color.Magenta, FontStyle.Regular, 8, 0);
                case "174c": return new SccPositionAndStyle(Color.Magenta, FontStyle.Regular, 9, 0);
                case "176c": return new SccPositionAndStyle(Color.Magenta, FontStyle.Regular, 10, 0);
                case "104c": return new SccPositionAndStyle(Color.Magenta, FontStyle.Regular, 11, 0);
                case "134c": return new SccPositionAndStyle(Color.Magenta, FontStyle.Regular, 12, 0);
                case "136c": return new SccPositionAndStyle(Color.Magenta, FontStyle.Regular, 13, 0);
                case "144c": return new SccPositionAndStyle(Color.Magenta, FontStyle.Regular, 14, 0);
                case "146c": return new SccPositionAndStyle(Color.Magenta, FontStyle.Regular, 15, 0);

                case "114d": return new SccPositionAndStyle(Color.Magenta, FontStyle.Underline, 1, 0);
                case "116d": return new SccPositionAndStyle(Color.Magenta, FontStyle.Underline, 2, 0);
                case "124d": return new SccPositionAndStyle(Color.Magenta, FontStyle.Underline, 3, 0);
                case "126d": return new SccPositionAndStyle(Color.Magenta, FontStyle.Underline, 4, 0);
                case "154d": return new SccPositionAndStyle(Color.Magenta, FontStyle.Underline, 5, 0);
                case "156d": return new SccPositionAndStyle(Color.Magenta, FontStyle.Underline, 6, 0);
                case "164d": return new SccPositionAndStyle(Color.Magenta, FontStyle.Underline, 7, 0);
                case "166d": return new SccPositionAndStyle(Color.Magenta, FontStyle.Underline, 8, 0);
                case "174d": return new SccPositionAndStyle(Color.Magenta, FontStyle.Underline, 9, 0);
                case "176d": return new SccPositionAndStyle(Color.Magenta, FontStyle.Underline, 10, 0);
                case "104d": return new SccPositionAndStyle(Color.Magenta, FontStyle.Underline, 11, 0);
                case "134d": return new SccPositionAndStyle(Color.Magenta, FontStyle.Underline, 12, 0);
                case "136d": return new SccPositionAndStyle(Color.Magenta, FontStyle.Underline, 13, 0);
                case "144d": return new SccPositionAndStyle(Color.Magenta, FontStyle.Underline, 14, 0);
                case "146d": return new SccPositionAndStyle(Color.Magenta, FontStyle.Underline, 15, 0);

                case "114e": return new SccPositionAndStyle(Color.White, FontStyle.Italic, 1, 0);
                case "116e": return new SccPositionAndStyle(Color.White, FontStyle.Italic, 2, 0);
                case "124e": return new SccPositionAndStyle(Color.White, FontStyle.Italic, 3, 0);
                case "126e": return new SccPositionAndStyle(Color.White, FontStyle.Italic, 4, 0);
                case "154e": return new SccPositionAndStyle(Color.White, FontStyle.Italic, 5, 0);
                case "156e": return new SccPositionAndStyle(Color.White, FontStyle.Italic, 6, 0);
                case "164e": return new SccPositionAndStyle(Color.White, FontStyle.Italic, 7, 0);
                case "166e": return new SccPositionAndStyle(Color.White, FontStyle.Italic, 8, 0);
                case "174e": return new SccPositionAndStyle(Color.White, FontStyle.Italic, 9, 0);
                case "176e": return new SccPositionAndStyle(Color.White, FontStyle.Italic, 10, 0);
                case "104e": return new SccPositionAndStyle(Color.White, FontStyle.Italic, 11, 0);
                case "134e": return new SccPositionAndStyle(Color.White, FontStyle.Italic, 12, 0);
                case "136e": return new SccPositionAndStyle(Color.White, FontStyle.Italic, 13, 0);
                case "144e": return new SccPositionAndStyle(Color.White, FontStyle.Italic, 14, 0);
                case "146e": return new SccPositionAndStyle(Color.White, FontStyle.Italic, 15, 0);

                case "114f": return new SccPositionAndStyle(Color.White, FontStyle.Underline | FontStyle.Italic, 1, 0);
                case "116f": return new SccPositionAndStyle(Color.White, FontStyle.Underline | FontStyle.Italic, 2, 0);
                case "124f": return new SccPositionAndStyle(Color.White, FontStyle.Underline | FontStyle.Italic, 3, 0);
                case "126f": return new SccPositionAndStyle(Color.White, FontStyle.Underline | FontStyle.Italic, 4, 0);
                case "154f": return new SccPositionAndStyle(Color.White, FontStyle.Underline | FontStyle.Italic, 5, 0);
                case "156f": return new SccPositionAndStyle(Color.White, FontStyle.Underline | FontStyle.Italic, 6, 0);
                case "164f": return new SccPositionAndStyle(Color.White, FontStyle.Underline | FontStyle.Italic, 7, 0);
                case "166f": return new SccPositionAndStyle(Color.White, FontStyle.Underline | FontStyle.Italic, 8, 0);
                case "174f": return new SccPositionAndStyle(Color.White, FontStyle.Underline | FontStyle.Italic, 9, 0);
                case "176f": return new SccPositionAndStyle(Color.White, FontStyle.Underline | FontStyle.Italic, 10, 0);
                case "104f": return new SccPositionAndStyle(Color.White, FontStyle.Underline | FontStyle.Italic, 11, 0);
                case "134f": return new SccPositionAndStyle(Color.White, FontStyle.Underline | FontStyle.Italic, 12, 0);
                case "136f": return new SccPositionAndStyle(Color.White, FontStyle.Underline | FontStyle.Italic, 13, 0);
                case "144f": return new SccPositionAndStyle(Color.White, FontStyle.Underline | FontStyle.Italic, 14, 0);
                case "146f": return new SccPositionAndStyle(Color.White, FontStyle.Underline | FontStyle.Italic, 15, 0);

                case "91d0": return new SccPositionAndStyle(Color.White, FontStyle.Regular, 1, 0);
                case "9151": return new SccPositionAndStyle(Color.White, FontStyle.Underline, 1, 0);
                case "91c2": return new SccPositionAndStyle(Color.Green, FontStyle.Regular, 1, 0);
                case "9143": return new SccPositionAndStyle(Color.Green, FontStyle.Underline, 1, 0);
                case "91c4": return new SccPositionAndStyle(Color.Blue, FontStyle.Regular, 1, 0);
                case "9145": return new SccPositionAndStyle(Color.Blue, FontStyle.Underline, 1, 0);
                case "9146": return new SccPositionAndStyle(Color.Cyan, FontStyle.Regular, 1, 0);
                case "91c7": return new SccPositionAndStyle(Color.Cyan, FontStyle.Underline, 1, 0);
                case "91c8": return new SccPositionAndStyle(Color.Red, FontStyle.Regular, 1, 0);
                case "9149": return new SccPositionAndStyle(Color.Red, FontStyle.Underline, 1, 0);
                case "914a": return new SccPositionAndStyle(Color.Yellow, FontStyle.Regular, 1, 0);
                case "91cb": return new SccPositionAndStyle(Color.Yellow, FontStyle.Underline, 1, 0);
                case "914c": return new SccPositionAndStyle(Color.Magenta, FontStyle.Regular, 1, 0);
                case "91cd": return new SccPositionAndStyle(Color.Magenta, FontStyle.Underline, 1, 0);

                case "9170": return new SccPositionAndStyle(Color.White, FontStyle.Regular, 2, 0);
                case "91f1": return new SccPositionAndStyle(Color.White, FontStyle.Underline, 2, 0);
                case "9162": return new SccPositionAndStyle(Color.Green, FontStyle.Regular, 2, 0);
                case "91e3": return new SccPositionAndStyle(Color.Green, FontStyle.Underline, 2, 0);
                case "9164": return new SccPositionAndStyle(Color.Blue, FontStyle.Regular, 2, 0);
                case "91e5": return new SccPositionAndStyle(Color.Blue, FontStyle.Underline, 2, 0);
                case "91e6": return new SccPositionAndStyle(Color.Cyan, FontStyle.Regular, 2, 0);
                case "9167": return new SccPositionAndStyle(Color.Cyan, FontStyle.Underline, 2, 0);
                case "9168": return new SccPositionAndStyle(Color.Red, FontStyle.Regular, 2, 0);
                case "91e9": return new SccPositionAndStyle(Color.Red, FontStyle.Underline, 2, 0);
                case "91ea": return new SccPositionAndStyle(Color.Yellow, FontStyle.Regular, 2, 0);
                case "916b": return new SccPositionAndStyle(Color.Yellow, FontStyle.Underline, 2, 0);
                case "91ec": return new SccPositionAndStyle(Color.Magenta, FontStyle.Regular, 2, 0);
                case "916d": return new SccPositionAndStyle(Color.Magenta, FontStyle.Underline, 2, 0);

                case "92d0": return new SccPositionAndStyle(Color.White, FontStyle.Regular, 3, 0);
                case "9251": return new SccPositionAndStyle(Color.White, FontStyle.Underline, 3, 0);
                case "92c2": return new SccPositionAndStyle(Color.Green, FontStyle.Regular, 3, 0);
                case "9243": return new SccPositionAndStyle(Color.Green, FontStyle.Underline, 3, 0);
                case "92c4": return new SccPositionAndStyle(Color.Blue, FontStyle.Regular, 3, 0);
                case "9245": return new SccPositionAndStyle(Color.Blue, FontStyle.Underline, 3, 0);
                case "9246": return new SccPositionAndStyle(Color.Cyan, FontStyle.Regular, 3, 0);
                case "92c7": return new SccPositionAndStyle(Color.Cyan, FontStyle.Underline, 3, 0);
                case "92c8": return new SccPositionAndStyle(Color.Red, FontStyle.Regular, 3, 0);
                case "9249": return new SccPositionAndStyle(Color.Red, FontStyle.Underline, 3, 0);
                case "924a": return new SccPositionAndStyle(Color.Yellow, FontStyle.Regular, 3, 0);
                case "92cb": return new SccPositionAndStyle(Color.Yellow, FontStyle.Underline, 3, 0);
                case "924c": return new SccPositionAndStyle(Color.Magenta, FontStyle.Regular, 3, 0);
                case "92cd": return new SccPositionAndStyle(Color.Magenta, FontStyle.Underline, 3, 0);

                case "9270": return new SccPositionAndStyle(Color.White, FontStyle.Regular, 4, 0);
                case "92f1": return new SccPositionAndStyle(Color.White, FontStyle.Underline, 4, 0);
                case "9262": return new SccPositionAndStyle(Color.Green, FontStyle.Regular, 4, 0);
                case "92e3": return new SccPositionAndStyle(Color.Green, FontStyle.Underline, 4, 0);
                case "9264": return new SccPositionAndStyle(Color.Blue, FontStyle.Regular, 4, 0);
                case "92e5": return new SccPositionAndStyle(Color.Blue, FontStyle.Underline, 4, 0);
                case "92e6": return new SccPositionAndStyle(Color.Cyan, FontStyle.Regular, 4, 0);
                case "9267": return new SccPositionAndStyle(Color.Cyan, FontStyle.Underline, 4, 0);
                case "9268": return new SccPositionAndStyle(Color.Red, FontStyle.Regular, 4, 0);
                case "92e9": return new SccPositionAndStyle(Color.Red, FontStyle.Underline, 4, 0);
                case "92ea": return new SccPositionAndStyle(Color.Yellow, FontStyle.Regular, 4, 0);
                case "926b": return new SccPositionAndStyle(Color.Yellow, FontStyle.Underline, 4, 0);
                case "92ec": return new SccPositionAndStyle(Color.Magenta, FontStyle.Regular, 4, 0);
                case "926d": return new SccPositionAndStyle(Color.Magenta, FontStyle.Underline, 4, 0);

                case "15d0": return new SccPositionAndStyle(Color.White, FontStyle.Regular, 5, 0);
                case "1551": return new SccPositionAndStyle(Color.White, FontStyle.Underline, 5, 0);
                case "15c2": return new SccPositionAndStyle(Color.Green, FontStyle.Regular, 5, 0);
                //                case "1543": return new SCCPositionAndStyle(Color.Green, FontStyle.Underline, 5, 0);
                case "15c4": return new SccPositionAndStyle(Color.Blue, FontStyle.Regular, 5, 0);
                //                case "1545": return new SCCPositionAndStyle(Color.Blue, FontStyle.Underline, 5, 0);
                //                case "1546": return new SCCPositionAndStyle(Color.Cyan, FontStyle.Regular, 5, 0);
                case "15c7": return new SccPositionAndStyle(Color.Cyan, FontStyle.Underline, 5, 0);
                case "15c8": return new SccPositionAndStyle(Color.Red, FontStyle.Regular, 5, 0);
                //case "1549": return new SCCPositionAndStyle(Color.Red, FontStyle.Underline, 5, 0);
                //case "154a": return new SCCPositionAndStyle(Color.Yellow, FontStyle.Regular, 5, 0);
                case "15cb": return new SccPositionAndStyle(Color.Yellow, FontStyle.Underline, 5, 0);
                //case "154c": return new SCCPositionAndStyle(Color.Magenta, FontStyle.Regular, 5, 0);
                case "15cd": return new SccPositionAndStyle(Color.Magenta, FontStyle.Underline, 5, 0);

                case "1570": return new SccPositionAndStyle(Color.White, FontStyle.Regular, 6, 0);
                case "15f1": return new SccPositionAndStyle(Color.White, FontStyle.Underline, 6, 0);
                //case "1562": return new SCCPositionAndStyle(Color.Green, FontStyle.Regular, 6, 0);
                //case "15e3": return new SCCPositionAndStyle(Color.Green, FontStyle.Underline, 6, 0);
                //case "1564": return new SCCPositionAndStyle(Color.Blue, FontStyle.Regular, 6, 0);
                case "15e5": return new SccPositionAndStyle(Color.Blue, FontStyle.Underline, 6, 0);
                case "15e6": return new SccPositionAndStyle(Color.Cyan, FontStyle.Regular, 6, 0);
                //case "1567": return new SCCPositionAndStyle(Color.Cyan, FontStyle.Underline, 6, 0);
                //case "1568": return new SCCPositionAndStyle(Color.Red, FontStyle.Regular, 6, 0);
                case "15e9": return new SccPositionAndStyle(Color.Red, FontStyle.Underline, 6, 0);
                case "15ea": return new SccPositionAndStyle(Color.Yellow, FontStyle.Regular, 6, 0);
                //case "156b": return new SCCPositionAndStyle(Color.Yellow, FontStyle.Underline, 6, 0);
                case "15ec": return new SccPositionAndStyle(Color.Magenta, FontStyle.Regular, 6, 0);
                //case "156d": return new SCCPositionAndStyle(Color.Magenta, FontStyle.Underline, 6, 0);

                case "16d0": return new SccPositionAndStyle(Color.White, FontStyle.Regular, 7, 0);
                case "1651": return new SccPositionAndStyle(Color.White, FontStyle.Underline, 7, 0);
                case "16c2": return new SccPositionAndStyle(Color.Green, FontStyle.Regular, 7, 0);
                //case "1643": return new SCCPositionAndStyle(Color.Green, FontStyle.Underline, 7, 0);
                case "16c4": return new SccPositionAndStyle(Color.Blue, FontStyle.Regular, 7, 0);
                //case "1645": return new SCCPositionAndStyle(Color.Blue, FontStyle.Underline, 7, 0);
                //case "1646": return new SCCPositionAndStyle(Color.Cyan, FontStyle.Regular, 7, 0);
                case "16c7": return new SccPositionAndStyle(Color.Cyan, FontStyle.Underline, 7, 0);
                case "16c8": return new SccPositionAndStyle(Color.Red, FontStyle.Regular, 7, 0);
                //case "1649": return new SCCPositionAndStyle(Color.Red, FontStyle.Underline, 7, 0);
                //case "164a": return new SCCPositionAndStyle(Color.Yellow, FontStyle.Regular, 7, 0);
                case "16cb": return new SccPositionAndStyle(Color.Yellow, FontStyle.Underline, 7, 0);
                //case "164c": return new SCCPositionAndStyle(Color.Magenta, FontStyle.Regular, 7, 0);
                case "16cd": return new SccPositionAndStyle(Color.Magenta, FontStyle.Underline, 7, 0);

                case "1670": return new SccPositionAndStyle(Color.White, FontStyle.Regular, 8, 0);
                case "16f1": return new SccPositionAndStyle(Color.White, FontStyle.Underline, 8, 0);
                //case "1662": return new SCCPositionAndStyle(Color.Green, FontStyle.Regular, 8, 0);
                case "16e3": return new SccPositionAndStyle(Color.Green, FontStyle.Underline, 8, 0);
                //case "1664": return new SCCPositionAndStyle(Color.Blue, FontStyle.Regular, 8, 0);
                case "16e5": return new SccPositionAndStyle(Color.Blue, FontStyle.Underline, 8, 0);
                case "16e6": return new SccPositionAndStyle(Color.Cyan, FontStyle.Regular, 8, 0);
                //case "1667": return new SCCPositionAndStyle(Color.Cyan, FontStyle.Underline, 8, 0);
                //case "1668": return new SCCPositionAndStyle(Color.Red, FontStyle.Regular, 8, 0);
                case "16e9": return new SccPositionAndStyle(Color.Red, FontStyle.Underline, 8, 0);
                case "16ea": return new SccPositionAndStyle(Color.Yellow, FontStyle.Regular, 8, 0);
                //case "166b": return new SCCPositionAndStyle(Color.Yellow, FontStyle.Underline, 8, 0);
                case "16ec": return new SccPositionAndStyle(Color.Magenta, FontStyle.Regular, 8, 0);
                //case "166d": return new SCCPositionAndStyle(Color.Magenta, FontStyle.Underline, 8, 0);

                case "97d0": return new SccPositionAndStyle(Color.White, FontStyle.Regular, 9, 0);
                case "9751": return new SccPositionAndStyle(Color.White, FontStyle.Underline, 9, 0);
                case "97c2": return new SccPositionAndStyle(Color.Green, FontStyle.Regular, 9, 0);
                case "9743": return new SccPositionAndStyle(Color.Green, FontStyle.Underline, 9, 0);
                case "97c4": return new SccPositionAndStyle(Color.Blue, FontStyle.Regular, 9, 0);
                case "9745": return new SccPositionAndStyle(Color.Blue, FontStyle.Underline, 9, 0);
                case "9746": return new SccPositionAndStyle(Color.Cyan, FontStyle.Regular, 9, 0);
                case "97c7": return new SccPositionAndStyle(Color.Cyan, FontStyle.Underline, 9, 0);
                case "97c8": return new SccPositionAndStyle(Color.Red, FontStyle.Regular, 9, 0);
                case "9749": return new SccPositionAndStyle(Color.Red, FontStyle.Underline, 9, 0);
                case "974a": return new SccPositionAndStyle(Color.Yellow, FontStyle.Regular, 9, 0);
                case "97cb": return new SccPositionAndStyle(Color.Yellow, FontStyle.Underline, 9, 0);
                case "974c": return new SccPositionAndStyle(Color.Magenta, FontStyle.Regular, 9, 0);
                case "97cd": return new SccPositionAndStyle(Color.Magenta, FontStyle.Underline, 9, 0);

                case "9770": return new SccPositionAndStyle(Color.White, FontStyle.Regular, 10, 0);
                case "97f1": return new SccPositionAndStyle(Color.White, FontStyle.Underline, 10, 0);
                case "9762": return new SccPositionAndStyle(Color.Green, FontStyle.Regular, 10, 0);
                case "97e3": return new SccPositionAndStyle(Color.Green, FontStyle.Underline, 10, 0);
                case "9764": return new SccPositionAndStyle(Color.Blue, FontStyle.Regular, 10, 0);
                case "97e5": return new SccPositionAndStyle(Color.Blue, FontStyle.Underline, 10, 0);
                case "97e6": return new SccPositionAndStyle(Color.Cyan, FontStyle.Regular, 10, 0);
                case "9767": return new SccPositionAndStyle(Color.Cyan, FontStyle.Underline, 10, 0);
                case "9768": return new SccPositionAndStyle(Color.Red, FontStyle.Regular, 10, 0);
                case "97e9": return new SccPositionAndStyle(Color.Red, FontStyle.Underline, 10, 0);
                case "97ea": return new SccPositionAndStyle(Color.Yellow, FontStyle.Regular, 10, 0);
                case "976b": return new SccPositionAndStyle(Color.Yellow, FontStyle.Underline, 10, 0);
                case "97ec": return new SccPositionAndStyle(Color.Magenta, FontStyle.Regular, 10, 0);
                case "976d": return new SccPositionAndStyle(Color.Magenta, FontStyle.Underline, 10, 0);

                case "10d0": return new SccPositionAndStyle(Color.White, FontStyle.Regular, 11, 0);
                case "1051": return new SccPositionAndStyle(Color.White, FontStyle.Underline, 11, 0);
                case "10c2": return new SccPositionAndStyle(Color.Green, FontStyle.Regular, 11, 0);
                //case "1043": return new SCCPositionAndStyle(Color.Green, FontStyle.Underline, 11, 0);
                case "10c4": return new SccPositionAndStyle(Color.Blue, FontStyle.Regular, 11, 0);
                //case "1045": return new SCCPositionAndStyle(Color.Blue, FontStyle.Underline, 11, 0);
                //case "1046": return new SCCPositionAndStyle(Color.Cyan, FontStyle.Regular, 11, 0);
                case "10c7": return new SccPositionAndStyle(Color.Cyan, FontStyle.Underline, 11, 0);
                case "10c8": return new SccPositionAndStyle(Color.Red, FontStyle.Regular, 11, 0);
                //case "1049": return new SCCPositionAndStyle(Color.Red, FontStyle.Underline, 11, 0);
                //case "104a": return new SCCPositionAndStyle(Color.Yellow, FontStyle.Regular, 11, 0);
                case "10cb": return new SccPositionAndStyle(Color.Yellow, FontStyle.Underline, 11, 0);
                //case "104c": return new SCCPositionAndStyle(Color.Magenta, FontStyle.Regular, 11, 0);
                case "10cd": return new SccPositionAndStyle(Color.Magenta, FontStyle.Underline, 11, 0);

                case "13d0": return new SccPositionAndStyle(Color.White, FontStyle.Regular, 12, 0);
                case "1351": return new SccPositionAndStyle(Color.White, FontStyle.Underline, 12, 0);
                case "13c2": return new SccPositionAndStyle(Color.Green, FontStyle.Regular, 12, 0);
                //case "1343": return new SCCPositionAndStyle(Color.Green, FontStyle.Underline, 12, 0);
                case "13c4": return new SccPositionAndStyle(Color.Blue, FontStyle.Regular, 12, 0);
                //case "1345": return new SCCPositionAndStyle(Color.Blue, FontStyle.Underline, 12, 0);
                //case "1346": return new SCCPositionAndStyle(Color.Cyan, FontStyle.Regular, 12, 0);
                case "13c7": return new SccPositionAndStyle(Color.Cyan, FontStyle.Underline, 12, 0);
                case "13c8": return new SccPositionAndStyle(Color.Red, FontStyle.Regular, 12, 0);
                //case "1349": return new SCCPositionAndStyle(Color.Red, FontStyle.Underline, 12, 0);
                //case "134a": return new SCCPositionAndStyle(Color.Yellow, FontStyle.Regular, 12, 0);
                case "13cb": return new SccPositionAndStyle(Color.Yellow, FontStyle.Underline, 12, 0);
                //case "134c": return new SCCPositionAndStyle(Color.Magenta, FontStyle.Regular, 12, 0);
                case "13cd": return new SccPositionAndStyle(Color.Magenta, FontStyle.Underline, 12, 0);

                case "1370": return new SccPositionAndStyle(Color.White, FontStyle.Regular, 13, 0);
                case "13f1": return new SccPositionAndStyle(Color.White, FontStyle.Underline, 13, 0);
                //case "1362": return new SCCPositionAndStyle(Color.Green, FontStyle.Regular, 13, 0);
                case "13e3": return new SccPositionAndStyle(Color.Green, FontStyle.Underline, 13, 0);
                //case "1364": return new SCCPositionAndStyle(Color.Blue, FontStyle.Regular, 13, 0);
                case "13e5": return new SccPositionAndStyle(Color.Blue, FontStyle.Underline, 13, 0);
                case "13e6": return new SccPositionAndStyle(Color.Cyan, FontStyle.Regular, 13, 0);
                //case "1367": return new SCCPositionAndStyle(Color.Cyan, FontStyle.Underline, 13, 0);
                //case "1368": return new SCCPositionAndStyle(Color.Red, FontStyle.Regular, 13, 0);
                case "13e9": return new SccPositionAndStyle(Color.Red, FontStyle.Underline, 13, 0);
                case "13ea": return new SccPositionAndStyle(Color.Yellow, FontStyle.Regular, 13, 0);
                //case "136b": return new SCCPositionAndStyle(Color.Yellow, FontStyle.Underline, 13, 0);
                case "13ec": return new SccPositionAndStyle(Color.Magenta, FontStyle.Regular, 13, 0);
                //case "136d": return new SCCPositionAndStyle(Color.Magenta, FontStyle.Underline, 13, 0);

                case "94d0": return new SccPositionAndStyle(Color.White, FontStyle.Regular, 14, 0);
                case "9451": return new SccPositionAndStyle(Color.White, FontStyle.Underline, 14, 0);
                case "94c2": return new SccPositionAndStyle(Color.Green, FontStyle.Regular, 14, 0);
                case "9443": return new SccPositionAndStyle(Color.Green, FontStyle.Underline, 14, 0);
                case "94c4": return new SccPositionAndStyle(Color.Blue, FontStyle.Regular, 14, 0);
                case "9445": return new SccPositionAndStyle(Color.Blue, FontStyle.Underline, 14, 0);
                case "9446": return new SccPositionAndStyle(Color.Cyan, FontStyle.Regular, 14, 0);
                case "94c7": return new SccPositionAndStyle(Color.Cyan, FontStyle.Underline, 14, 0);
                case "94c8": return new SccPositionAndStyle(Color.Red, FontStyle.Regular, 14, 0);
                case "9449": return new SccPositionAndStyle(Color.Red, FontStyle.Underline, 14, 0);
                case "944a": return new SccPositionAndStyle(Color.Yellow, FontStyle.Regular, 14, 0);
                case "94cb": return new SccPositionAndStyle(Color.Yellow, FontStyle.Underline, 14, 0);
                case "944c": return new SccPositionAndStyle(Color.Magenta, FontStyle.Regular, 14, 0);
                case "94cd": return new SccPositionAndStyle(Color.Magenta, FontStyle.Underline, 14, 0);

                case "9470": return new SccPositionAndStyle(Color.White, FontStyle.Regular, 15, 0);
                case "94f1": return new SccPositionAndStyle(Color.White, FontStyle.Underline, 15, 0);
                case "9462": return new SccPositionAndStyle(Color.Green, FontStyle.Regular, 15, 0);
                case "94e3": return new SccPositionAndStyle(Color.Green, FontStyle.Underline, 15, 0);
                case "9464": return new SccPositionAndStyle(Color.Blue, FontStyle.Regular, 15, 0);
                case "94e5": return new SccPositionAndStyle(Color.Blue, FontStyle.Underline, 15, 0);
                case "94e6": return new SccPositionAndStyle(Color.Cyan, FontStyle.Regular, 15, 0);
                case "9467": return new SccPositionAndStyle(Color.Cyan, FontStyle.Underline, 15, 0);
                case "9468": return new SccPositionAndStyle(Color.Red, FontStyle.Regular, 15, 0);
                case "94e9": return new SccPositionAndStyle(Color.Red, FontStyle.Underline, 15, 0);
                case "94ea": return new SccPositionAndStyle(Color.Yellow, FontStyle.Regular, 15, 0);
                case "946b": return new SccPositionAndStyle(Color.Yellow, FontStyle.Underline, 15, 0);
                case "94ec": return new SccPositionAndStyle(Color.Magenta, FontStyle.Regular, 15, 0);
                case "946d": return new SccPositionAndStyle(Color.Magenta, FontStyle.Underline, 15, 0);

                //Columns 4-28

                case "9152": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 1, 4);
                case "91d3": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 1, 4);
                case "9154": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 1, 8);
                case "91d5": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 1, 8);
                case "91d6": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 1, 12);
                case "9157": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 1, 12);
                case "9158": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 1, 16);
                case "91d9": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 1, 16);
                case "91da": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 1, 20);
                case "915b": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 1, 20);
                case "91dc": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 1, 24);
                case "915d": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 1, 24);
                case "915e": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 1, 28);
                case "91df": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 1, 28);

                case "91f2": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 2, 4);
                case "9173": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 2, 4);
                case "91f4": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 2, 8);
                case "9175": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 2, 8);
                case "9176": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 2, 12);
                case "91f7": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 2, 12);
                case "91f8": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 2, 16);
                case "9179": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 2, 16);
                case "917a": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 2, 20);
                case "91fb": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 2, 20);
                case "917c": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 2, 24);
                case "91fd": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 2, 24);
                case "91fe": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 2, 28);
                case "917f": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 2, 28);

                case "9252": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 3, 4);
                case "92d3": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 3, 4);
                case "9254": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 3, 8);
                case "92d5": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 3, 8);
                case "92d6": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 3, 12);
                case "9257": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 3, 12);
                case "9258": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 3, 16);
                case "92d9": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 3, 16);
                case "92da": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 3, 20);
                case "925b": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 3, 20);
                case "92dc": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 3, 24);
                case "925d": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 3, 24);
                case "925e": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 3, 28);
                case "92df": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 3, 28);

                case "92f2": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 4, 4);
                case "9273": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 4, 4);
                case "92f4": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 4, 8);
                case "9275": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 4, 8);
                case "9276": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 4, 12);
                case "92f7": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 4, 12);
                case "92f8": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 4, 16);
                case "9279": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 4, 16);
                case "927a": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 4, 20);
                case "92fb": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 4, 20);
                case "927c": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 4, 24);
                case "92fd": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 4, 24);
                case "92fe": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 4, 28);
                case "927f": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 4, 28);

                case "1552": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 5, 4);
                case "15d3": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 5, 4);
                case "1554": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 5, 8);
                case "15d5": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 5, 8);
                case "15d6": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 5, 12);
                case "1557": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 5, 12);
                case "1558": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 5, 16);
                case "15d9": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 5, 16);
                case "15da": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 5, 20);
                case "155b": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 5, 20);
                case "15dc": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 5, 24);
                case "155d": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 5, 24);
                case "155e": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 5, 28);
                case "15df": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 5, 28);

                case "15f2": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 6, 4);
                case "1573": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 6, 4);
                case "15f4": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 6, 8);
                case "1575": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 6, 8);
                case "1576": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 6, 12);
                case "15f7": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 6, 12);
                case "15f8": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 6, 16);
                case "1579": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 6, 16);
                case "157a": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 6, 20);
                case "15fb": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 6, 20);
                case "157c": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 6, 24);
                case "15fd": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 6, 24);
                case "15fe": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 6, 28);
                case "157f": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 6, 28);

                case "1652": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 7, 4);
                case "16d3": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 7, 4);
                case "1654": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 7, 8);
                case "16d5": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 7, 8);
                case "16d6": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 7, 12);
                case "1657": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 7, 12);
                case "1658": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 7, 16);
                case "16d9": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 7, 16);
                case "16da": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 7, 20);
                case "165b": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 7, 20);
                case "16dc": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 7, 24);
                case "165d": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 7, 24);
                case "165e": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 7, 28);
                case "16df": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 7, 28);

                case "16f2": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 8, 4);
                case "1673": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 8, 4);
                case "16f4": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 8, 8);
                case "1675": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 8, 8);
                case "1676": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 8, 12);
                case "16f7": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 8, 12);
                case "16f8": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 8, 16);
                case "1679": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 8, 16);
                case "167a": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 8, 20);
                case "16fb": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 8, 20);
                case "167c": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 8, 24);
                case "16fd": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 8, 24);
                case "16fe": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 8, 28);
                case "167f": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 8, 28);

                case "9752": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 9, 4);
                case "97d3": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 9, 4);
                case "9754": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 9, 8);
                case "97d5": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 9, 8);
                case "97d6": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 9, 12);
                case "9757": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 9, 12);
                case "9758": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 9, 16);
                case "97d9": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 9, 16);
                case "97da": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 9, 20);
                case "975b": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 9, 20);
                case "97dc": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 9, 24);
                case "975d": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 9, 24);
                case "975e": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 9, 28);
                case "97df": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 9, 28);

                case "97f2": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 10, 4);
                case "9773": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 10, 4);
                case "97f4": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 10, 8);
                case "9775": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 10, 8);
                case "9776": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 10, 12);
                case "97f7": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 10, 12);
                case "97f8": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 10, 16);
                case "9779": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 10, 16);
                case "977a": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 10, 20);
                case "97fb": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 10, 20);
                case "977c": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 10, 24);
                case "97fd": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 10, 24);
                case "97fe": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 10, 28);
                case "977f": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 10, 28);

                case "1052": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 11, 4);
                case "10d3": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 11, 4);
                case "1054": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 11, 8);
                case "10d5": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 11, 8);
                case "10d6": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 11, 12);
                case "1057": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 11, 12);
                case "1058": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 11, 16);
                case "10d9": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 11, 16);
                case "10da": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 11, 20);
                case "105b": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 11, 20);
                case "10dc": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 11, 24);
                case "105d": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 11, 24);
                case "105e": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 11, 28);
                case "10df": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 11, 28);

                case "1352": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 12, 4);
                case "13d3": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 12, 4);
                case "1354": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 12, 8);
                case "13d5": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 12, 8);
                case "13d6": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 12, 12);
                case "1357": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 12, 12);
                case "1358": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 12, 16);
                case "13d9": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 12, 16);
                case "13da": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 12, 20);
                case "135b": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 12, 20);
                case "13dc": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 12, 24);
                case "135d": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 12, 24);
                case "135e": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 12, 28);
                case "13df": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 12, 28);

                case "13f2": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 13, 4);
                case "1373": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 13, 4);
                case "13f4": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 13, 8);
                case "1375": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 13, 8);
                case "1376": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 13, 12);
                case "13f7": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 13, 12);
                case "13f8": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 13, 16);
                case "1379": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 13, 16);
                case "137a": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 13, 20);
                case "13fb": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 13, 20);
                case "137c": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 13, 24);
                case "13fd": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 13, 24);
                case "13fe": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 13, 28);
                case "137f": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 13, 28);

                case "9452": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 14, 4);
                case "94d3": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 14, 4);
                case "9454": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 14, 8);
                case "94d5": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 14, 8);
                case "94d6": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 14, 12);
                case "9457": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 14, 12);
                case "9458": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 14, 16);
                case "94d9": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 14, 16);
                case "94da": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 14, 20);
                case "945b": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 14, 20);
                case "94dc": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 14, 24);
                case "945d": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 14, 24);
                case "945e": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 14, 28);
                case "94df": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 14, 28);

                case "94f2": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 15, 4);
                case "9473": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 15, 4);
                case "94f4": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 15, 8);
                case "9475": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 15, 8);
                case "9476": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 15, 12);
                case "94f7": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 15, 12);
                case "94f8": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 15, 16);
                case "9479": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 15, 16);
                case "947a": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 15, 20);
                case "94fb": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 15, 20);
                case "947c": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 15, 24);
                case "94fd": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 15, 24);
                case "94fe": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, 15, 28);
                case "947f": return new SccPositionAndStyle(Color.Transparent, FontStyle.Underline, 15, 28);

                // mid-row commands
                case "9120": return new SccPositionAndStyle(Color.White, FontStyle.Regular, -1, -1);
                case "91a1": return new SccPositionAndStyle(Color.White, FontStyle.Underline, -1, -1);
                case "91a2": return new SccPositionAndStyle(Color.Green, FontStyle.Regular, -1, -1);
                case "9123": return new SccPositionAndStyle(Color.Green, FontStyle.Underline, -1, -1);
                case "91a4": return new SccPositionAndStyle(Color.Blue, FontStyle.Regular, -1, -1);
                case "9125": return new SccPositionAndStyle(Color.Blue, FontStyle.Underline, -1, -1);
                case "9126": return new SccPositionAndStyle(Color.Cyan, FontStyle.Regular, -1, -1);
                case "91a7": return new SccPositionAndStyle(Color.Cyan, FontStyle.Underline, -1, -1);
                case "91a8": return new SccPositionAndStyle(Color.Red, FontStyle.Regular, -1, -1);
                case "9129": return new SccPositionAndStyle(Color.Red, FontStyle.Underline, -1, -1);
                case "912a": return new SccPositionAndStyle(Color.Yellow, FontStyle.Regular, -1, -1);
                case "91ab": return new SccPositionAndStyle(Color.Yellow, FontStyle.Underline, -1, -1);
                case "912c": return new SccPositionAndStyle(Color.Magenta, FontStyle.Regular, -1, -1);
                case "91ad": return new SccPositionAndStyle(Color.Magenta, FontStyle.Underline, -1, -1);

                case "91ae": return new SccPositionAndStyle(Color.Transparent, FontStyle.Italic, -1, -1);
                case "912f": return new SccPositionAndStyle(Color.Transparent, FontStyle.Italic | FontStyle.Underline, -1, -1);

                case "94a8": return new SccPositionAndStyle(Color.Transparent, FontStyle.Regular, -1, -1); // turn flash on
            }
            return null;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            Paragraph p = null;
            foreach (string line in lines)
            {
                string s = line.Trim();
                var match = RegexTimeCodes.Match(s);
                if (match.Success)
                {
                    TimeCode startTime = ParseTimeCode(s.Substring(0, match.Length - 1));
                    string text = GetSccText(s.Substring(match.Index), ref _errorCount);

                    if (text == "942c 942c" || text == "942c")
                    {
                        if (p != null)
                        {
                            p.EndTime = new TimeCode(startTime.TotalMilliseconds);
                        }
                    }
                    else
                    {
                        p = new Paragraph(startTime, new TimeCode(startTime.TotalMilliseconds), text);
                        subtitle.Paragraphs.Add(p);
                    }
                }
            }
            for (int i = subtitle.Paragraphs.Count - 2; i >= 0; i--)
            {
                p = subtitle.GetParagraphOrDefault(i);
                Paragraph next = subtitle.GetParagraphOrDefault(i + 1);
                if (p != null && next != null && Math.Abs(p.EndTime.TotalMilliseconds - p.StartTime.TotalMilliseconds) < 0.001)
                {
                    p.EndTime = new TimeCode(next.StartTime.TotalMilliseconds);
                }

                if (next != null && string.IsNullOrEmpty(next.Text))
                {
                    subtitle.Paragraphs.Remove(next);
                }
            }
            p = subtitle.GetParagraphOrDefault(0);
            if (p != null && string.IsNullOrEmpty(p.Text))
            {
                subtitle.Paragraphs.Remove(p);
            }

            subtitle.Renumber();
        }

        public static string GetSccText(string s, ref int errorCount)
        {
            int y = 0;
            string[] parts = s.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder();
            bool first = true;
            bool italicOn = false;
            int k = 0;
            var alignment = string.Empty;

            while (k < parts.Length)
            {
                string part = parts[k];
                if (part.Length == 4)
                {
                    if (part != "94ae" && part != "9420" && part != "94ad" && part != "9426" && part != "946e" && part != "91ce" && part != "13ce" && part != "9425" && part != "9429")
                    {

                        // skewed apostrophe "’"
                        if (part == "9229" && k < parts.Length - 1 && parts[k + 1] == "9229" && sb.EndsWith('\''))
                        {
                            sb.Remove(sb.Length - 1, 1);
                            sb.Append("’");
                            k += 2;
                            continue;
                        }

                        // 3 codes
                        if (k < parts.Length - 2)
                        {
                            var letter = GetLetterFromCode(part + " " + parts[k + 1] + " " + parts[k + 2]);
                            if (letter != null)
                            {
                                sb.Append(letter);
                                k += 3;
                                continue;
                            }
                        }

                        // two codes
                        if (k < parts.Length - 1)
                        {
                            var letter = GetLetterFromCode(part + " " + parts[k + 1]);
                            if (letter != null)
                            {
                                sb.Append(letter);
                                k += 2;
                                continue;
                            }
                        }

                        if (part[0] == '9' || part[0] == '8')
                        {
                            if (k + 1 < parts.Length && parts[k + 1] == part)
                            {
                                k++;
                            }
                        }

                        var cp = GetColorAndPosition(part);
                        if (cp != null)
                        {
                            if (!string.IsNullOrWhiteSpace(sb.ToString()) && cp.Y > 0 && y >= 0 && cp.Y > y && !sb.ToString().EndsWith(Environment.NewLine, StringComparison.Ordinal))
                            {
                                sb.AppendLine();
                            }

                            if (cp.Y > 0)
                            {
                                y = cp.Y;
                            }

                            if (y == 1)
                            {
                                alignment = "{\\an8}";
                            }

                            if ((cp.Style & FontStyle.Italic) == FontStyle.Italic && !italicOn)
                            {
                                sb.Append("<i>");
                                italicOn = true;
                            }
                            else if (cp.Style == FontStyle.Regular && italicOn)
                            {
                                sb.Append("</i>");
                                italicOn = false;
                            }
                        }
                        else
                        {
                            switch (part)
                            {
                                case "9440":
                                case "94e0":
                                    if (!sb.ToString().EndsWith(Environment.NewLine, StringComparison.Ordinal))
                                    {
                                        sb.AppendLine();
                                    }

                                    break;
                                case "2c75":
                                case "2cf2":
                                case "2c6f":
                                case "2c6e":
                                case "2c6d":
                                case "2c6c":
                                case "2c6b":
                                case "2c6a":
                                case "2c69":
                                case "2c68":
                                case "2c67":
                                case "2c66":
                                case "2c65":
                                case "2c64":
                                case "2c63":
                                case "2c62":
                                case "2c61":
                                    sb.Append(GetLetterFromCode(part.Substring(2, 2)));
                                    break;
                                case "2c52":
                                case "2c94":
                                    break;
                                default:
                                    var result = GetLetterFromCode(part);
                                    if (result == null)
                                    {
                                        sb.Append(GetLetterFromCode(part.Substring(0, 2)));
                                        var secondPart = part.Substring(2, 2) + "80";

                                        // 3 codes
                                        if (k < parts.Length - 2)
                                        {
                                            var letter = GetLetterFromCode(secondPart + " " + parts[k + 1] + " " + parts[k + 2]);
                                            if (letter != null)
                                            {
                                                sb.Append(letter);
                                                k += 3;
                                                continue;
                                            }
                                        }

                                        // two codes
                                        if (k < parts.Length - 1)
                                        {
                                            var letter = GetLetterFromCode(secondPart + " " + parts[k + 1]);
                                            if (letter != null)
                                            {
                                                sb.Append(letter);
                                                k += 2;
                                                continue;
                                            }
                                        }

                                        sb.Append(GetLetterFromCode(part.Substring(2, 2)));
                                    }
                                    else
                                    {
                                        sb.Append(result);
                                    }
                                    break;
                            }
                        }
                    }
                }
                else if (part.Length > 0)
                {
                    if (!first)
                    {
                        errorCount++;
                    }
                }
                first = false;
                k++;
            }
            string res = sb.ToString().Replace("<i></i>", string.Empty).Replace("</i><i>", string.Empty);
            res = res.Replace("  ", " ").Replace("  ", " ").Replace(Environment.NewLine + " ", Environment.NewLine).Trim();
            if (res.Contains("<i>") && !res.Contains("</i>"))
            {
                res += "</i>";
            }

            return alignment + HtmlUtil.FixInvalidItalicTags(res);
        }

        private static TimeCode ParseTimeCode(string start)
        {
            string[] arr = start.Split(new[] { ':', ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
            return new TimeCode(int.Parse(arr[0]), int.Parse(arr[1]), int.Parse(arr[2]), FramesToMillisecondsMax999(int.Parse(arr[3])));
        }
    }
}
