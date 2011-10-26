using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{

    /// <summary>
    /// http://www.geocities.com/mcpoodle43/SCC_TOOLS/DOCS/cc_charset.gif
    /// </summary>
    public class ScenaristClosedCaptions : SubtitleFormat
    {
        //00:01:00:29   9420 9420 94ae 94ae 94d0 94d0 4920 f761 7320 ...
        readonly Regex _regexTimeCodes = new Regex(@"^\d+:\d\d:\d\d[:,]\d\d\t", RegexOptions.Compiled);

        private readonly List<string> _letters = new List<string>
                                                     {
                                                         " ",
                                                         "!",
                                                         "\"",
                                                         "#",
                                                         "$",
                                                         "%",
                                                         "&",
                                                         "'",
                                                         "(",
                                                         ")",
                                                         "á",
                                                         "+",
                                                         ",",
                                                         "-",
                                                         ".",
                                                         "/",
                                                         "0",
                                                         "1",
                                                         "2",
                                                         "3",
                                                         "4",
                                                         "5",
                                                         "6",
                                                         "7",
                                                         "8",
                                                         "9",
                                                         ":",
                                                         ";",
                                                         "<",
                                                         "=",
                                                         ">",
                                                         "?",
                                                         "@",
                                                         "A",
                                                         "B",
                                                         "C",
                                                         "D",
                                                         "E",
                                                         "F",
                                                         "G",
                                                         "H",
                                                         "I",
                                                         "J",
                                                         "K",
                                                         "L",
                                                         "M",
                                                         "N",
                                                         "O",
                                                         "P",
                                                         "Q",
                                                         "R",
                                                         "S",
                                                         "T",
                                                         "U",
                                                         "V",
                                                         "W",
                                                         "X",
                                                         "Y",
                                                         "Z",
                                                         "[",
                                                         "é",
                                                         "]",
                                                         "í",
                                                         "ó",
                                                         "ú",
                                                         "a",
                                                         "b",
                                                         "c",
                                                         "d",
                                                         "e",
                                                         "f",
                                                         "g",
                                                         "h",
                                                         "i",
                                                         "j",
                                                         "k",
                                                         "l",
                                                         "m",
                                                         "n",
                                                         "o",
                                                         "p",
                                                         "q",
                                                         "r",
                                                         "s",
                                                         "t",
                                                         "u",
                                                         "v",
                                                         "w",
                                                         "x",
                                                         "y",
                                                         "z",
                                                         "",
                                                         "",
                                                         "Ñ",
                                                         "ñ",
                                                         "",
                                                         "",
                                                         "",
                                                         "½",
                                                         "",
                                                         "",
                                                         "",
                                                         "£",
                                                         "",
                                                         "à",
                                                         "",
                                                         "è",
                                                         "â",
                                                         "ê",
                                                         "î",
                                                         "ô",
                                                         "û",
                                                         "Á",
                                                         "É",
                                                         "Ó",
                                                         "Ú",
                                                         "Ü",
                                                         "ü",
                                                         "'",
                                                         "i",
                                                         "*",
                                                         "'",
                                                         "-",
                                                         "",
                                                         "",
                                                         "\"",
                                                         "\"",
                                                         "",
                                                         "À",
                                                         "Â",
                                                         "",
                                                         "È",
                                                         "Ê",
                                                         "Ë",
                                                         "ë",
                                                         "Î",
                                                         "Ï",
                                                         "ï",
                                                         "ô",
                                                         "Ù",
                                                         "ù",
                                                         "Û",
                                                         "",
                                                         "",
                                                         "Ã",
                                                         "ã",
                                                         "Í",
                                                         "Ì",
                                                         "ì",
                                                         "Ò",
                                                         "ò",
                                                         "Õ",
                                                         "õ",
                                                         "{",
                                                         "}",
                                                         "\\",
                                                         "^",
                                                         "_",
                                                         "|",
                                                         "~",
                                                         "Ä",
                                                         "ä",
                                                         "Ö",
                                                         "ö",
                                                         "",
                                                         "",
                                                         "",
                                                         "|",
                                                         "Å",
                                                         "å",
                                                         "Ø",
                                                         "ø",
                                                         "",
                                                         "",
                                                         "",
                                                         "",
                                                         "", //9420=RCL, Resume Caption Loading
                                                         "", //94ae=Clear Buffer
                                                         "", //942c=Clear Caption
                                                         "", //8080=Wait One Frame
                                                         "", //942f=Display Caption
                                                         "", //9470=?
                                                         "", //94d0=?
                                                         "", //942f=End of Caption
                                                         "", //1370=?
                                                     };

        private readonly List<string> _letterCodes = new List<string>
                                                         {
                                                             "20",
                                                             "a1",
                                                             "a2",
                                                             "23",
                                                             "a4",
                                                             "25",
                                                             "26",
                                                             "a7",
                                                             "a8",
                                                             "29",
                                                             "2a",
                                                             "ab",
                                                             "2c",
                                                             "ad",
                                                             "ae",
                                                             "2f",
                                                             "b0",
                                                             "31",
                                                             "32",
                                                             "b3",
                                                             "34",
                                                             "b5",
                                                             "b6",
                                                             "37",
                                                             "38",
                                                             "b9",
                                                             "ba",
                                                             "3b",
                                                             "bc",
                                                             "3d",
                                                             "3e",
                                                             "bf",
                                                             "40",
                                                             "c1",
                                                             "c2",
                                                             "43",
                                                             "c4",
                                                             "45",
                                                             "46",
                                                             "c7",
                                                             "c8",
                                                             "49",
                                                             "4a",
                                                             "cb",
                                                             "4c",
                                                             "cd",
                                                             "ce",
                                                             "4f",
                                                             "d0",
                                                             "51",
                                                             "52",
                                                             "d3",
                                                             "54",
                                                             "d5",
                                                             "d6",
                                                             "57",
                                                             "58",
                                                             "d9",
                                                             "da",
                                                             "5b",
                                                             "dc",
                                                             "5d",
                                                             "5e",
                                                             "df",
                                                             "e0",
                                                             "61",
                                                             "62",
                                                             "e3",
                                                             "64",
                                                             "e5",
                                                             "e6",
                                                             "67",
                                                             "68",
                                                             "e9",
                                                             "ea",
                                                             "6b",
                                                             "ec",
                                                             "6d",
                                                             "6e",
                                                             "ef",
                                                             "70",
                                                             "f1",
                                                             "f2",
                                                             "73",
                                                             "f4",
                                                             "75",
                                                             "76",
                                                             "f7",
                                                             "f8",
                                                             "79",
                                                             "7a",
                                                             "fb",
                                                             "7c",
                                                             "fd",
                                                             "fe",
                                                             "7f",
                                                             "91b0",
                                                             "9131",
                                                             "9132",
                                                             "91b3",
                                                             "9134",
                                                             "91b5",
                                                             "91b6",
                                                             "9137",
                                                             "9138",
                                                             "91b9",
                                                             "91ba",
                                                             "913b",
                                                             "91bc",
                                                             "913d",
                                                             "913e",
                                                             "91bf",
                                                             "9220",
                                                             "92a1",
                                                             "92a2",
                                                             "9223",
                                                             "92a4",
                                                             "9225",
                                                             "9226",
                                                             "92a7",
                                                             "92a8",
                                                             "9229",
                                                             "922a",
                                                             "92ab",
                                                             "922c",
                                                             "92ad",
                                                             "92ae",
                                                             "922f",
                                                             "92b0",
                                                             "9231",
                                                             "9232",
                                                             "92b3",
                                                             "9234",
                                                             "92b5",
                                                             "92b6",
                                                             "9237",
                                                             "9238",
                                                             "92b9",
                                                             "92b3",
                                                             "923b",
                                                             "92b3",
                                                             "923d",
                                                             "923e",
                                                             "92bf",
                                                             "1320",
                                                             "13a1",
                                                             "13a2",
                                                             "1323",
                                                             "13a4",
                                                             "1325",
                                                             "1326",
                                                             "13a7",
                                                             "13a8",
                                                             "1329",
                                                             "132a",
                                                             "13ab",
                                                             "132c",
                                                             "13ad",
                                                             "13ae",
                                                             "132f",
                                                             "13b0",
                                                             "1331",
                                                             "1332",
                                                             "13b3",
                                                             "1334",
                                                             "13b5",
                                                             "13b6",
                                                             "1337",
                                                             "1338",
                                                             "13b9",
                                                             "13b3",
                                                             "133b",
                                                             "13b3",
                                                             "133d",
                                                             "133e",
                                                             "13bf",
                                                             "9420", //9420=RCL, Resume Caption Loading
                                                             "94ae", //94ae=Clear Buffer
                                                             "942c", //942c=Clear Caption
                                                             "8080", //8080=Wait One Frame
                                                             "942f", //942f=Display Caption
                                                             "9470", //9470=?
                                                             "94d0", //94d0=?
                                                             "942f", //942f=End of Caption
                                                             "1370", //1370=?
                                                         };

        public override string Extension
        {
            get { return ".scc"; }
        }

        public override string Name
        {
            get { return "Scenarist Closed Captions"; }
        }

        public override bool HasLineNumber
        {
            get { return false; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            var subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > 0;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Scenarist_SCC V1.0");
            sb.AppendLine();

            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.AppendLine(string.Format("{0}\t94ae 94ae 9420 9420 {1} 942f 942f", ToTimeCode(p.StartTime.TotalMilliseconds), ToSccText(p.Text)));
                sb.AppendLine();
                sb.AppendLine(string.Format("{0}\t942c 942c", ToTimeCode(p.EndTime.TotalMilliseconds)));
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private string ToSccText(string text)
        {
            var sb = new StringBuilder();
            int i = 0;
            string code = string.Empty;
            while (i < text.Length)
            {
                string s = text.Substring(i, 1);
                int index = _letters.IndexOf(s);
                string newCode = string.Empty;
                if (index < 0)
                    newCode = _letterCodes[_letters.IndexOf(" ")];
                else
                    newCode = _letterCodes[index];


                if (code.Length == 2 && newCode.Length == 4)
                {
                    code += "80";
                }

                if (code.Length == 4)
                {
                    sb.Append(code + " ");
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
                    newCode = string.Empty;
                }
                else if (newCode.Length == 2 && code.Length == 0)
                {
                    code = newCode;
                    newCode = string.Empty;
                }

                i++;
            }
            if (code.Length == 2)
                code += "80";
            if (code.Length == 4)
                sb.Append(code);


            return sb.ToString().Trim();
        }

        private string ToTimeCode(double totalMilliseconds)
        {
            TimeSpan ts = TimeSpan.FromMilliseconds(totalMilliseconds);
            return string.Format("{0:00}:{1:00}:{2:00}:{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            Paragraph p = null;
            foreach (string line in lines)
            {
                string s = line.Trim();
                var match = _regexTimeCodes.Match(s);
                if (match.Success)
                {
                    TimeCode startTime = ParseTimeCode(s.Substring(0, match.Length-1));
                    string text = GetSccText(s.Substring(match.Index));

                    if (text == "942c 942c" || text == "942c")
                    {
                        p.EndTime = startTime;
                    }
                    else
                    {
                        p = new Paragraph(startTime, startTime, text);
                        subtitle.Paragraphs.Add(p);
                    }
                }
            }
            for (int i = subtitle.Paragraphs.Count - 2; i >= 0; i--)
            {
                p = subtitle.GetParagraphOrDefault(i);
                Paragraph next = subtitle.GetParagraphOrDefault(i + 1);
                if (p != null && next != null && p.EndTime.TotalMilliseconds == p.StartTime.TotalMilliseconds)
                    p.EndTime = next.StartTime;
                if (next != null && string.IsNullOrEmpty(next.Text))
                    subtitle.Paragraphs.Remove(next);
            }
            p = subtitle.GetParagraphOrDefault(0);
            if (p != null && string.IsNullOrEmpty(p.Text))
                subtitle.Paragraphs.Remove(p);

            subtitle.Renumber(1);
        }

        private string GetSccText(string s)
        {
            string[] parts = s.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder();
            foreach (var part in parts)
            {
                if (part.Length == 4)
                {
                    string result = GetLetter(part);
                    if (result == null)
                    {
                        sb.Append(GetLetter(part.Substring(0, 2)));
                        sb.Append(GetLetter(part.Substring(2, 2)));
                    }
                    else
                    {
                        sb.Append(result);
                    }
                }
                else if (part.Length > 0)
                {
                    _errorCount++;
                }
            }
            return sb.ToString();
        }

        private string GetLetter(string hexCode)
        {
            int index = _letterCodes.IndexOf(hexCode.ToLower());
            if (index < 0)
                return null;

            return _letters[index];
        }

        private TimeCode ParseTimeCode(string start)
        {
            string[] arr = start.Split(':');
            var ts = new TimeSpan(0, int.Parse(arr[0]), int.Parse(arr[1]), int.Parse(arr[2]), int.Parse(arr[3]));
            return new TimeCode(ts);
        }

    }
}


