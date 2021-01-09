using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
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
            foreach (var line in lines)
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
            return lines.Count <= 4 && lines.All(line => line.Length <= 32);
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
                var p = subtitle.Paragraphs[i];
                sb.AppendLine($"{ToTimeCode(p.StartTime.TotalMilliseconds)}\t94ae 94ae 9420 9420 {ToSccText(p.Text, language)} 942f 942f");
                sb.AppendLine();

                var next = subtitle.GetParagraphOrDefault(i + 1);
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

            var leftAlign = text.StartsWith("{\\an7}", StringComparison.Ordinal) ||
                            text.StartsWith("{\\an4}", StringComparison.Ordinal) ||
                            text.StartsWith("{\\an1}", StringComparison.Ordinal);

            var rightAlign = text.StartsWith("{\\an9}", StringComparison.Ordinal) ||
                             text.StartsWith("{\\an6}", StringComparison.Ordinal) ||
                             text.StartsWith("{\\an3}", StringComparison.Ordinal);

            var verticalCenter = text.StartsWith("{\\an4}", StringComparison.Ordinal) ||
                                 text.StartsWith("{\\an5}", StringComparison.Ordinal) ||
                                 text.StartsWith("{\\an6}", StringComparison.Ordinal);

            text = Utilities.RemoveSsaTags(text);
            var lines = text.Trim().SplitToLines();
            int italic = 0;
            var sb = new StringBuilder();
            int count = 1;
            foreach (var line in lines)
            {
                text = line.Trim();
                if (count > 0)
                {
                    sb.Append(' ');
                }

                var centerCodes = GetCenterCodes(text, count, lines.Count, topAlign, leftAlign, rightAlign, verticalCenter);
                sb.Append(centerCodes);
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

        public static string GetCenterCodes(string text, int lineNumber, int totalLines, bool topAlign, bool leftAlign, bool rightAlign, bool verticalCenter)
        {
            var row = 14 - (totalLines - lineNumber);
            if (topAlign)
            {
                row = lineNumber;
            }
            else if (verticalCenter)
            {
                row = 6 - totalLines / 2 + lineNumber;
            }
            var rowCodes = new List<string> { "91", "91", "92", "92", "15", "15", "16", "16", "97", "97", "10", "13", "13", "94", "94" };
            var rowCode = rowCodes[row];

            var left = (32 - text.Length) / 2;
            if (leftAlign)
            {
                left = 0;
            }
            else if (rightAlign)
            {
                left = 32 - text.Length;
            }
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
            var code = rowCode + columnCodes[row];

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
                return $"{code} {code} 9723 9723 ";
            }

            return $"{code} {code} ";
        }

        private string ToTimeCode(double totalMilliseconds)
        {
            var ts = TimeSpan.FromMilliseconds(totalMilliseconds);
            if (DropFrame)
            {
                return $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00};{MillisecondsToFramesMaxFrameRate(ts.Milliseconds):00}";
            }

            return $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}:{MillisecondsToFramesMaxFrameRate(ts.Milliseconds):00}";
        }

        private static Dictionary<string, SccPositionAndStyle> _styleLookup;
        public static SccPositionAndStyle GetColorAndPosition(string code)
        {
            if (_styleLookup == null)
            {
                _styleLookup = SccPositionAndStyleTable.SccPositionAndStyles.ToDictionary(p => p.Code, p => p);
            }

            return _styleLookup.TryGetValue(code, out var style) ? style : null;
        }

        public static string GetCodeFromPositionAndColor(int x, int y, FontStyle fontStyle, Color color)
        {
            var match = SccPositionAndStyleTable.SccPositionAndStyles.FirstOrDefault(p => p.X == x && p.Y == y && p.Style == fontStyle && p.ForeColor == color);
            return match == null ? string.Empty : match.Code;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            Paragraph p = null;
            foreach (string line in lines)
            {
                var s = line.Trim();
                var match = RegexTimeCodes.Match(s);
                if (match.Success)
                {
                    var startTime = ParseTimeCode(s.Substring(0, match.Length - 1));
                    var text = GetSccText(s.Substring(match.Index).ToLowerInvariant(), ref _errorCount);

                    if (text == "942c 942c" || text == "942c")
                    {
                        if (p != null)
                        {
                            p.EndTime.TotalMilliseconds = startTime.TotalMilliseconds;
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
                var next = subtitle.GetParagraphOrDefault(i + 1);
                if (p != null && next != null && Math.Abs(p.EndTime.TotalMilliseconds - p.StartTime.TotalMilliseconds) < 0.001)
                {
                    p.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds;
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

            for (var index = 0; index < subtitle.Paragraphs.Count - 1; index++)
            {
                var current = subtitle.Paragraphs[index];
                var next = subtitle.Paragraphs[index + 1];
                if (Math.Abs(current.EndTime.TotalMilliseconds - next.StartTime.TotalMilliseconds) < 0.01)
                {
                    if (current.EndTime.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines >
                        current.StartTime.TotalMilliseconds)
                    {
                        current.EndTime.TotalMilliseconds -= Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                    }
                }
            }

            subtitle.Renumber();
        }

        public static string GetSccText(string s, ref int errorCount)
        {
            int y = 0;
            int x = -1;
            string[] parts = s.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder();
            bool first = true;
            bool italicOn = false;
            bool fontOn = false;
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

                            x = cp.X;

                            if (y < 4)
                            {
                                alignment = "{\\an8}";
                            }

                            if (y >= 7 && y <= 9)
                            {
                                alignment = "{\\an5}";
                            }

                            if (cp.ForeColor == Color.Green)
                            {
                                if (fontOn)
                                {
                                    sb.Append("</font>");
                                }
                                sb.Append("<font color=\"Green\">");
                                fontOn = true;
                            }
                            else if (cp.ForeColor == Color.Blue)
                            {
                                if (fontOn)
                                {
                                    sb.Append("</font>");
                                }
                                sb.Append("<font color=\"Blue\">");
                                fontOn = true;
                            }
                            else if (cp.ForeColor == Color.Cyan)
                            {
                                if (fontOn)
                                {
                                    sb.Append("</font>");
                                }
                                sb.Append("<font color=\"Cyan\">");
                                fontOn = true;
                            }
                            else if (cp.ForeColor == Color.Red)
                            {
                                if (fontOn)
                                {
                                    sb.Append("</font>");
                                }
                                sb.Append("<font color=\"Red\">");
                                fontOn = true;
                            }
                            else if (cp.ForeColor == Color.Yellow)
                            {
                                if (fontOn)
                                {
                                    sb.Append("</font>");
                                }
                                sb.Append("<font color=\"Yellow\">");
                                fontOn = true;
                            }
                            else if (cp.ForeColor == Color.Magenta)
                            {
                                if (fontOn)
                                {
                                    sb.Append("</font>");
                                }
                                sb.Append("<font color=\"Magenta\">");
                                fontOn = true;
                            }
                            else if (cp.ForeColor == Color.White && fontOn)
                            {
                                sb.Append("</font>");
                                sb.Append("</font>");
                                fontOn = false;
                            }
                            else if (cp.ForeColor == Color.Black && fontOn)
                            {
                                sb.Append("</font>");
                                fontOn = false;
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

            var leftAlign = false;
            var rightAlign = false;
            if (x >= 0)
            {
                var text = sb.ToString().Trim().SplitToLines().LastOrDefault();
                text = HtmlUtil.RemoveHtmlTags(text);
                if (text != null && !string.IsNullOrEmpty(text))
                {
                    if (text.Length < 28)
                    {
                        if (x < 3)
                        {
                            leftAlign = true;
                        }
                        else if (x + text.Length > 30)
                        {
                            rightAlign = true;
                        }
                    }
                }
            }

            if (alignment == "{\\an8}" && leftAlign)
            {
                alignment = "{\\an7}";
            }
            else if (alignment == "{\\an8}" && rightAlign)
            {
                alignment = "{\\an9}";
            }
            else if (alignment == "{\\an5}" && leftAlign)
            {
                alignment = "{\\an4}";
            }
            else if (alignment == "{\\an5}" && rightAlign)
            {
                alignment = "{\\an6}";
            }
            else if (string.IsNullOrEmpty(alignment) && leftAlign)
            {
                alignment = "{\\an1}";
            }
            else if (string.IsNullOrEmpty(alignment) && rightAlign)
            {
                alignment = "{\\an3}";
            }

            var res = sb.ToString().Replace("<i></i>", string.Empty).Replace("</i><i>", string.Empty);
            if (fontOn)
            {
                res += "</font>";
            }

            res = res.Replace(Environment.NewLine + "</font>", "</font>" + Environment.NewLine);
            res = res.Replace("  ", " ").Replace("  ", " ").Replace(Environment.NewLine + " ", Environment.NewLine).Trim();
            if (res.Contains("<i>") && !res.Contains("</i>"))
            {
                res += "</i>";
            }

            return alignment + HtmlUtil.FixInvalidItalicTags(res);
        }

        private static TimeCode ParseTimeCode(string start)
        {
            var arr = start.Split(new[] { ':', ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
            return new TimeCode(int.Parse(arr[0]), int.Parse(arr[1]), int.Parse(arr[2]), FramesToMillisecondsMax999(int.Parse(arr[3])));
        }
    }
}
