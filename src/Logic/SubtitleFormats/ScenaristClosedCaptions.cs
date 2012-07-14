using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{

    /// <summary>
    /// http://www.theneitherworld.com/mcpoodle/SCC_TOOLS/DOCS/SCC_FORMAT.HTML
    /// Maxinum four lines + max 32 characters on each line
    /// </summary>
    public class ScenaristClosedCaptions : SubtitleFormat
    {

        public class SCCPositionAndStyle
        {
            public Color ForeColor { get; set; }
            public FontStyle Style { get; set; }
            public int X { get; set; }
            public int Y { get; set; }

            public SCCPositionAndStyle(Color color, FontStyle style, int y, int x)
            {
                ForeColor = color;
                Style = style;
                X = x;
                Y = y;
            }
        }

        //00:01:00:29   9420 9420 94ae 94ae 94d0 94d0 4920 f761 7320 ...
        static readonly Regex RegexTimeCodes = new Regex(@"^\d+:\d\d:\d\d[:,]\d\d\t", RegexOptions.Compiled);

        private static readonly List<string> _letters = new List<string>
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
                                                         "", //9440=?
                                                         "", //9452
                                                         "", //9454
                                                         "", //9470=?
                                                         "", //94d0=?
                                                         "", //94d6=?
                                                         "", //942f=End of Caption
                                                         "", //94f2=?
                                                         "", //94f4=?
                                                         " ", //9723=?
                                                         " ", //97a1=?
                                                         " ", //97a2=?
                                                         "", //1370=?
                                                         "", //13e0=?
                                                         "", //13f2=?
                                                         "", //136e=?
                                                         "", //94ce=?
                                                         "", //2c2f=?
                                                     };

        private static  readonly List<string> _letterCodes = new List<string>
                                                         {
                                                             "20",    //  " ",
                                                             "a1",    //  "!",
                                                             "a2",    //  "\"",
                                                             "23",    //  "#",
                                                             "a4",    //  "$",
                                                             "25",    //  "%",
                                                             "26",    //  "&",
                                                             "a7",    //  "'",
                                                             "a8",    //  "(",
                                                             "29",    //  ")",
                                                             "2a",    //  "á",
                                                             "ab",    //  "+",
                                                             "2c",    //  ",",
                                                             "ad",    //  "-",
                                                             "ae",    //  ".",
                                                             "2f",    //  "/",
                                                             "b0",    //  "0",
                                                             "31",    //  "1",
                                                             "32",    //  "2",
                                                             "b3",    //  "3",
                                                             "34",    //  "4",
                                                             "b5",    //  "5",
                                                             "b6",    //  "6",
                                                             "37",    //  "7",
                                                             "38",    //  "8",
                                                             "b9",    //  "9",
                                                             "ba",    //  ":",
                                                             "3b",    //  ";",
                                                             "bc",    //  "<",
                                                             "3d",    //  "=",
                                                             "3e",    //  ">",
                                                             "bf",    //  "?",
                                                             "40",    //  "@",
                                                             "c1",    //  "A",
                                                             "c2",    //  "B",
                                                             "43",    //  "C",
                                                             "c4",    //  "D",
                                                             "45",    //  "E",
                                                             "46",    //  "F",
                                                             "c7",    //  "G",
                                                             "c8",    //  "H",
                                                             "49",    //  "I",
                                                             "4a",    //  "J",
                                                             "cb",    //  "K",
                                                             "4c",    //  "L",
                                                             "cd",    //  "M",
                                                             "ce",    //  "N",
                                                             "4f",    //  "O",
                                                             "d0",    //  "P",
                                                             "51",    //  "Q",
                                                             "52",    //  "R",
                                                             "d3",    //  "S",
                                                             "54",    //  "T",
                                                             "d5",    //  "U",
                                                             "d6",    //  "V",
                                                             "57",    //  "W",
                                                             "58",    //  "X",
                                                             "d9",    //  "Y",
                                                             "da",    //  "Z",
                                                             "5b",    //  "[",
                                                             "dc",    //  "é",
                                                             "5d",    //  "]",
                                                             "5e",    //  "í",
                                                             "df",    //  "ó",
                                                             "e0",    //  "ú",
                                                             "61",    //  "a",
                                                             "62",    //  "b",
                                                             "e3",    //  "c",
                                                             "64",    //  "d",
                                                             "e5",    //  "e",
                                                             "e6",    //  "f",
                                                             "67",    //  "g",
                                                             "68",    //  "h",
                                                             "e9",    //  "i",
                                                             "ea",    //  "j",
                                                             "6b",    //  "k",
                                                             "ec",    //  "l",
                                                             "6d",    //  "m",
                                                             "6e",    //  "n",
                                                             "ef",    //  "o",
                                                             "70",    //  "p",
                                                             "f1",    //  "q",
                                                             "f2",    //  "r",
                                                             "73",    //  "s",
                                                             "f4",    //  "t",
                                                             "75",    //  "u",
                                                             "76",    //  "v",
                                                             "f7",    //  "w",
                                                             "f8",    //  "x",
                                                             "79",    //  "y",
                                                             "7a",    //  "z",
                                                             "fb",    //  "",
                                                             "7c",    //  "",
                                                             "fd",    //  "Ñ",
                                                             "fe",    //  "ñ",
                                                             "7f",    //  "",
                                                             "91b0",  //  "",
                                                             "9131",  //  "",
                                                             "9132",  //  "½",
                                                             "91b3",  //  "",
                                                             "9134",  //  "",
                                                             "91b5",  //  "",
                                                             "91b6",  //  "£",
                                                             "9137",  //  "",
                                                             "9138",  //  "à",
                                                             "91b9",  //  "",
                                                             "91ba",  //  "è",
                                                             "913b",  //  "â",
                                                             "91bc",  //  "ê",
                                                             "913d",  //  "î",
                                                             "913e",  //  "ô",
                                                             "91bf",  //  "û",
                                                             "9220",  //  "Á",
                                                             "92a1",  //  "É",
                                                             "92a2",  //  "Ó",
                                                             "9223",  //  "Ú",
                                                             "92a4",  //  "Ü",
                                                             "9225",  //  "ü",
                                                             "9226",  //  "'",
                                                             "92a7",  //  "i",
                                                             "92a8",  //  "*",
                                                             "9229",  //  "'",
                                                             "922a",  //  "-",
                                                             "92ab",  //  "",
                                                             "922c",  //  "",
                                                             "92ad",  //  "\"",
                                                             "92ae",  //  "\"",
                                                             "922f",  //  "",
                                                             "92b0",  //  "À",
                                                             "9231",  //  "Â",
                                                             "9232",  //  "",
                                                             "92b3",  //  "È",
                                                             "9234",  //  "Ê",
                                                             "92b5",  //  "Ë",
                                                             "92b6",  //  "ë",
                                                             "9237",  //  "Î",
                                                             "9238",  //  "Ï",
                                                             "92b9",  //  "ï",
                                                             "92b3",  //  "ô",
                                                             "923b",  //  "Ù",
                                                             "92b3",  //  "ù",
                                                             "923d",  //  "Û",
                                                             "923e",  //  "",
                                                             "92bf",  //  "",
                                                             "1320",  //  "Ã",
                                                             "13a1",  //  "ã",
                                                             "13a2",  //  "Í",
                                                             "1323",  //  "Ì",
                                                             "13a4",  //  "ì",
                                                             "1325",  //  "Ò",
                                                             "1326",  //  "ò",
                                                             "13a7",  //  "Õ",
                                                             "13a8",  //  "õ",
                                                             "1329",  //  "{",
                                                             "132a",  //  "}",
                                                             "13ab",  //  "\\",
                                                             "132c",  //  "^",
                                                             "13ad",  //  "_",
                                                             "13ae",  //  "|",
                                                             "132f",  //  "~",
                                                             "13b0",  //  "Ä",
                                                             "1331",  //  "ä",
                                                             "1332",  //  "Ö",
                                                             "13b3",  //  "ö",
                                                             "1334",  //  "",
                                                             "13b5",  //  "",
                                                             "13b6",  //  "",
                                                             "1337",  //  "|",
                                                             "1338",  //  "Å",
                                                             "13b9",  //  "å",
                                                             "13b3",  //  "Ø",
                                                             "133b",  //  "ø",
                                                             "13b3",  //  "",
                                                             "133d",  //  "",
                                                             "133e",  //  "",
                                                             "13bf",  //  "",
                                                             "9420", //9420=RCL, Resume Caption Loading
                                                             "94ae", //94ae=Clear Buffer
                                                             "942c", //942c=Clear Caption
                                                             "8080", //8080=Wait One Frame
                                                             "942f", //942f=Display Caption
                                                             "9440", //9440=? first sub?
                                                             "9452", //?
                                                             "9454", //?
                                                             "9470", //9470=?
                                                             "94d0", //94d0=?
                                                             "94d6", //94d6=?
                                                             "942f", //942f=End of Caption
                                                             "94f2", //
                                                             "94f4", //
                                                             "9723", // ?
                                                             "97a1", // ?
                                                             "97a2", // ?
                                                             "1370", //1370=?
                                                             "13e0", //13e0=?
                                                             "13f2", //13f2=?
                                                             "136e", //136e=?
                                                             "94ce", //94ce=?
                                                             "2c2f", //?
                                                         };

        public override string Extension
        {
            get { return ".scc"; }
        }

        public override string Name
        {
            get { return "Scenarist Closed Captions"; }
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

        private string FixMax4LinesAndMax32CharsPerLine(string text)
        {
            var lines = text.Trim().Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            bool allOk = true;
            foreach (string line in lines)
            {
                if (line.Length > 32)
                    allOk = false;
            }
            if (lines.Length > 4)
                allOk = false;

            if (allOk)
                return text;

            text = Utilities.AutoBreakLine(text, 1, 32, 4);
            lines = text.Trim().Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            allOk = true;
            foreach (string line in lines)
            {
                if (line.Length > 32)
                    allOk = false;
            }
            if (lines.Length > 4)
                allOk = false;

            if (allOk)
                return text;

            text = AutoBreakLineMax4Lines(text, 32);
            lines = text.Trim().Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            allOk = true;
            foreach (string line in lines)
            {
                if (line.Length > 32)
                    allOk = false;
            }
            if (lines.Length > 4)
                allOk = false;

            if (allOk)
                return text;

            var sb = new StringBuilder();
            int count = 0;
            foreach (string line in lines)
            {
                if (count < 4)
                {
                    if (line.Length > 32)
                        sb.AppendLine(line.Substring(0, 32));
                    else
                        sb.AppendLine(line);
                }
                count++;
            }
            return sb.ToString().Trim();
        }

        private int GetLastIndexOfSpace(string s, int endCount)
        {
            int end = endCount;
            if (end >= s.Length)
                end = s.Length - 1;

            int i = end;
            while (i > 0)
            {
                if (s[i] == ' ')
                    return i;
                i--;
            }
            return -1;
        }

        private string AutoBreakLineMax4Lines(string text, int maxLength)
        {
            string s = text.Replace(Environment.NewLine, " ");
            s = s.Replace("  ", " ");
            var sb = new StringBuilder();
            int i = GetLastIndexOfSpace(s, maxLength);
            if (i > 0)
            {
                sb.AppendLine(s.Substring(0, i));
                s = s.Remove(0,i).Trim();
                if (s.Length <= maxLength)
                    i = s.Length;
                else
                    i = GetLastIndexOfSpace(s, maxLength);
                if (i > 0)
                {
                    sb.AppendLine(s.Substring(0, i));
                    s = s.Remove(0, i).Trim();
                    if (s.Length <= maxLength)
                        i = s.Length;
                    else
                        i = GetLastIndexOfSpace(s, maxLength);
                    if (i > 0)
                    {
                        sb.AppendLine(s.Substring(0, i));
                        s = s.Remove(0, i).Trim();
                        if (s.Length <= maxLength)
                            i = s.Length;
                        else
                            i = GetLastIndexOfSpace(s, maxLength);
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
            text = FixMax4LinesAndMax32CharsPerLine(text);
            var lines = text.Trim().Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            var sb = new StringBuilder();
            int count = 1;
            foreach (string line in lines)
            {
                text = line.Trim();
                if (count > 0)
                    sb.Append(" ");
                sb.Append(GetCenterCodes(text, count, lines.Length));
                count++;
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
            }

            return sb.ToString().Trim();
        }

        public string GetCenterCodes(string text, int lineNumber, int totalLines)
        {
            int row = 14 - (totalLines - lineNumber);

            var rowCodes = new List<string> { "91", "91", "92", "92", "15", "15", "16", "16", "97", "97", "10", "13", "13", "94", "94" };
            string rowCode = rowCodes[row];

            int left = (32 - text.Length) / 2;
            int columnRest = left % 4;
            int column = left - columnRest;

            List<string> columnCodes = null;
            switch (column)
            {
                case 0: columnCodes = new List<string> { "d0", "70", "d0", "70", "d0", "70", "d0", "70", "d0", "70", "d0", "d0", "70", "d0", "70" };
                    break;
                case 4: columnCodes = new List<string> { "52", "f2", "52", "f2", "52", "f2", "52", "f2", "52", "f2", "52", "52", "f2", "52", "f2" };
                    break;
                case 8: columnCodes = new List<string> { "54", "f4", "54", "f4", "54", "f4", "54", "f4", "54", "f4", "54", "54", "f4", "54", "f4" };
                    break;
                case 12: columnCodes = new List<string> { "d6", "76", "d6", "76", "d6", "76", "d6", "76", "d6", "76", "d6", "d6", "76", "d6", "76" };
                    break;
                case 16: columnCodes = new List<string> { "58", "f8", "58", "f8", "58", "f8", "58", "f8", "58", "f8", "58", "58", "f8", "58", "f8" };
                    break;
                case 20: columnCodes = new List<string> { "da", "7a", "da", "7a", "da", "7a", "da", "7a", "da", "7a", "da", "da", "7a", "da", "7a" };
                    break;
                case 24: columnCodes = new List<string> { "dc", "7c", "dc", "7c", "dc", "7c", "dc", "7c", "dc", "7c", "dc", "dc", "7c", "dc", "7c" };
                    break;
                case 28: columnCodes = new List<string> { "5e", "fe", "5e", "fe", "5e", "fe", "5e", "fe", "5e", "fe", "5e", "5e", "fe", "5e", "fe" };
                    break;
            }
            string code = rowCode + columnCodes[row];

            if (columnRest == 1)
                return code + " " + code + " 97a1 97a1 ";
            else if (columnRest == 2)
                return code + " " + code + " 97a2 97a2 ";
            else if (columnRest == 3)
                return code + " " + code + " 9723 9723 ";
            return code + " " + code + " ";
        }

        private string ToTimeCode(double totalMilliseconds)
        {
            TimeSpan ts = TimeSpan.FromMilliseconds(totalMilliseconds);
            return string.Format("{0:00}:{1:00}:{2:00}:{3:00}", ts.Hours, ts.Minutes, ts.Seconds, MillisecondsToFrames(ts.Milliseconds));
        }

        public static SCCPositionAndStyle GetColorAndPosition(string code)
        {
            switch (code.ToLower())
            {
                case "91d0": return new SCCPositionAndStyle(Color.White, FontStyle.Regular, 1, 0);
                case "9151": return new SCCPositionAndStyle(Color.White, FontStyle.Underline, 1, 0);
                case "91c2": return new SCCPositionAndStyle(Color.Green, FontStyle.Regular, 1, 0);
                case "9143": return new SCCPositionAndStyle(Color.Green, FontStyle.Underline, 1, 0);
                case "91c4": return new SCCPositionAndStyle(Color.Blue, FontStyle.Regular, 1, 0);
                case "9145": return new SCCPositionAndStyle(Color.Blue, FontStyle.Underline, 1, 0);
                case "9146": return new SCCPositionAndStyle(Color.Cyan, FontStyle.Regular, 1, 0);
                case "91c7": return new SCCPositionAndStyle(Color.Cyan, FontStyle.Underline, 1, 0);
                case "91c8": return new SCCPositionAndStyle(Color.Red, FontStyle.Regular, 1, 0);
                case "9149": return new SCCPositionAndStyle(Color.Red, FontStyle.Underline, 1, 0);
                case "914a": return new SCCPositionAndStyle(Color.Yellow, FontStyle.Regular, 1, 0);
                case "91cb": return new SCCPositionAndStyle(Color.Yellow, FontStyle.Underline, 1, 0);
                case "914c": return new SCCPositionAndStyle(Color.Magenta, FontStyle.Regular, 1, 0);
                case "91cd": return new SCCPositionAndStyle(Color.Magenta, FontStyle.Underline, 1, 0);

                case "9170": return new SCCPositionAndStyle(Color.White, FontStyle.Regular, 2, 0);
                case "91f1": return new SCCPositionAndStyle(Color.White, FontStyle.Underline, 2, 0);
                case "9162": return new SCCPositionAndStyle(Color.Green, FontStyle.Regular, 2, 0);
                case "91e3": return new SCCPositionAndStyle(Color.Green, FontStyle.Underline, 2, 0);
                case "9164": return new SCCPositionAndStyle(Color.Blue, FontStyle.Regular, 2, 0);
                case "91e5": return new SCCPositionAndStyle(Color.Blue, FontStyle.Underline, 2, 0);
                case "91e6": return new SCCPositionAndStyle(Color.Cyan, FontStyle.Regular, 2, 0);
                case "9167": return new SCCPositionAndStyle(Color.Cyan, FontStyle.Underline, 2, 0);
                case "9168": return new SCCPositionAndStyle(Color.Red, FontStyle.Regular, 2, 0);
                case "91e9": return new SCCPositionAndStyle(Color.Red, FontStyle.Underline, 2, 0);
                case "91ea": return new SCCPositionAndStyle(Color.Yellow, FontStyle.Regular, 2, 0);
                case "916b": return new SCCPositionAndStyle(Color.Yellow, FontStyle.Underline, 2, 0);
                case "91ec": return new SCCPositionAndStyle(Color.Magenta, FontStyle.Regular, 2, 0);
                case "916d": return new SCCPositionAndStyle(Color.Magenta, FontStyle.Underline, 2, 0);


                case "92d0": return new SCCPositionAndStyle(Color.White, FontStyle.Regular, 3, 0);
                case "9251": return new SCCPositionAndStyle(Color.White, FontStyle.Underline, 3, 0);
                case "92c2": return new SCCPositionAndStyle(Color.Green, FontStyle.Regular, 3, 0);
                case "9243": return new SCCPositionAndStyle(Color.Green, FontStyle.Underline, 3, 0);
                case "92c4": return new SCCPositionAndStyle(Color.Blue, FontStyle.Regular, 3, 0);
                case "9245": return new SCCPositionAndStyle(Color.Blue, FontStyle.Underline, 3, 0);
                case "9246": return new SCCPositionAndStyle(Color.Cyan, FontStyle.Regular, 3, 0);
                case "92c7": return new SCCPositionAndStyle(Color.Cyan, FontStyle.Underline, 3, 0);
                case "92c8": return new SCCPositionAndStyle(Color.Red, FontStyle.Regular, 3, 0);
                case "9249": return new SCCPositionAndStyle(Color.Red, FontStyle.Underline, 3, 0);
                case "924a": return new SCCPositionAndStyle(Color.Yellow, FontStyle.Regular, 3, 0);
                case "92cb": return new SCCPositionAndStyle(Color.Yellow, FontStyle.Underline, 3, 0);
                case "924c": return new SCCPositionAndStyle(Color.Magenta, FontStyle.Regular, 3, 0);
                case "92cd": return new SCCPositionAndStyle(Color.Magenta, FontStyle.Underline, 3, 0);

                case "9270": return new SCCPositionAndStyle(Color.White, FontStyle.Regular, 4, 0);
                case "92f1": return new SCCPositionAndStyle(Color.White, FontStyle.Underline, 4, 0);
                case "9262": return new SCCPositionAndStyle(Color.Green, FontStyle.Regular, 4, 0);
                case "92e3": return new SCCPositionAndStyle(Color.Green, FontStyle.Underline, 4, 0);
                case "9264": return new SCCPositionAndStyle(Color.Blue, FontStyle.Regular, 4, 0);
                case "92e5": return new SCCPositionAndStyle(Color.Blue, FontStyle.Underline, 4, 0);
                case "92e6": return new SCCPositionAndStyle(Color.Cyan, FontStyle.Regular, 4, 0);
                case "9267": return new SCCPositionAndStyle(Color.Cyan, FontStyle.Underline, 4, 0);
                case "9268": return new SCCPositionAndStyle(Color.Red, FontStyle.Regular, 4, 0);
                case "92e9": return new SCCPositionAndStyle(Color.Red, FontStyle.Underline, 4, 0);
                case "92ea": return new SCCPositionAndStyle(Color.Yellow, FontStyle.Regular, 4, 0);
                case "926b": return new SCCPositionAndStyle(Color.Yellow, FontStyle.Underline, 4, 0);
                case "92ec": return new SCCPositionAndStyle(Color.Magenta, FontStyle.Regular, 4, 0);
                case "926d": return new SCCPositionAndStyle(Color.Magenta, FontStyle.Underline, 4, 0);


                case "15d0": return new SCCPositionAndStyle(Color.White, FontStyle.Regular, 5, 0);
                case "1551": return new SCCPositionAndStyle(Color.White, FontStyle.Underline, 5, 0);
                case "15c2": return new SCCPositionAndStyle(Color.Green, FontStyle.Regular, 5, 0);
                case "1543": return new SCCPositionAndStyle(Color.Green, FontStyle.Underline, 5, 0);
                case "15c4": return new SCCPositionAndStyle(Color.Blue, FontStyle.Regular, 5, 0);
                case "1545": return new SCCPositionAndStyle(Color.Blue, FontStyle.Underline, 5, 0);
                case "1546": return new SCCPositionAndStyle(Color.Cyan, FontStyle.Regular, 5, 0);
                case "15c7": return new SCCPositionAndStyle(Color.Cyan, FontStyle.Underline, 5, 0);
                case "15c8": return new SCCPositionAndStyle(Color.Red, FontStyle.Regular, 5, 0);
                case "1549": return new SCCPositionAndStyle(Color.Red, FontStyle.Underline, 5, 0);
                case "154a": return new SCCPositionAndStyle(Color.Yellow, FontStyle.Regular, 5, 0);
                case "15cb": return new SCCPositionAndStyle(Color.Yellow, FontStyle.Underline, 5, 0);
                case "154c": return new SCCPositionAndStyle(Color.Magenta, FontStyle.Regular, 5, 0);
                case "15cd": return new SCCPositionAndStyle(Color.Magenta, FontStyle.Underline, 5, 0);

                case "1570": return new SCCPositionAndStyle(Color.White, FontStyle.Regular, 6, 0);
                case "15f1": return new SCCPositionAndStyle(Color.White, FontStyle.Underline, 6, 0);
                case "1562": return new SCCPositionAndStyle(Color.Green, FontStyle.Regular, 6, 0);
                case "15e3": return new SCCPositionAndStyle(Color.Green, FontStyle.Underline, 6, 0);
                case "1564": return new SCCPositionAndStyle(Color.Blue, FontStyle.Regular, 6, 0);
                case "15e5": return new SCCPositionAndStyle(Color.Blue, FontStyle.Underline, 6, 0);
                case "15e6": return new SCCPositionAndStyle(Color.Cyan, FontStyle.Regular, 6, 0);
                case "1567": return new SCCPositionAndStyle(Color.Cyan, FontStyle.Underline, 6, 0);
                case "1568": return new SCCPositionAndStyle(Color.Red, FontStyle.Regular, 6, 0);
                case "15e9": return new SCCPositionAndStyle(Color.Red, FontStyle.Underline, 6, 0);
                case "15ea": return new SCCPositionAndStyle(Color.Yellow, FontStyle.Regular, 6, 0);
                case "156b": return new SCCPositionAndStyle(Color.Yellow, FontStyle.Underline, 6, 0);
                case "15ec": return new SCCPositionAndStyle(Color.Magenta, FontStyle.Regular, 6, 0);
                case "156d": return new SCCPositionAndStyle(Color.Magenta, FontStyle.Underline, 6, 0);


                case "16d0": return new SCCPositionAndStyle(Color.White, FontStyle.Regular, 7, 0);
                case "1651": return new SCCPositionAndStyle(Color.White, FontStyle.Underline, 7, 0);
                case "16c2": return new SCCPositionAndStyle(Color.Green, FontStyle.Regular, 7, 0);
                case "1643": return new SCCPositionAndStyle(Color.Green, FontStyle.Underline, 7, 0);
                case "16c4": return new SCCPositionAndStyle(Color.Blue, FontStyle.Regular, 7, 0);
                case "1645": return new SCCPositionAndStyle(Color.Blue, FontStyle.Underline, 7, 0);
                case "1646": return new SCCPositionAndStyle(Color.Cyan, FontStyle.Regular, 7, 0);
                case "16c7": return new SCCPositionAndStyle(Color.Cyan, FontStyle.Underline, 7, 0);
                case "16c8": return new SCCPositionAndStyle(Color.Red, FontStyle.Regular, 7, 0);
                case "1649": return new SCCPositionAndStyle(Color.Red, FontStyle.Underline, 7, 0);
                case "164a": return new SCCPositionAndStyle(Color.Yellow, FontStyle.Regular, 7, 0);
                case "16cb": return new SCCPositionAndStyle(Color.Yellow, FontStyle.Underline, 7, 0);
                case "164c": return new SCCPositionAndStyle(Color.Magenta, FontStyle.Regular, 7, 0);
                case "16cd": return new SCCPositionAndStyle(Color.Magenta, FontStyle.Underline, 7, 0);

                case "1670": return new SCCPositionAndStyle(Color.White, FontStyle.Regular, 8, 0);
                case "16f1": return new SCCPositionAndStyle(Color.White, FontStyle.Underline, 8, 0);
                case "1662": return new SCCPositionAndStyle(Color.Green, FontStyle.Regular, 8, 0);
                case "16e3": return new SCCPositionAndStyle(Color.Green, FontStyle.Underline, 8, 0);
                case "1664": return new SCCPositionAndStyle(Color.Blue, FontStyle.Regular, 8, 0);
                case "16e5": return new SCCPositionAndStyle(Color.Blue, FontStyle.Underline, 8, 0);
                case "16e6": return new SCCPositionAndStyle(Color.Cyan, FontStyle.Regular, 8, 0);
                case "1667": return new SCCPositionAndStyle(Color.Cyan, FontStyle.Underline, 8, 0);
                case "1668": return new SCCPositionAndStyle(Color.Red, FontStyle.Regular, 8, 0);
                case "16e9": return new SCCPositionAndStyle(Color.Red, FontStyle.Underline, 8, 0);
                case "16ea": return new SCCPositionAndStyle(Color.Yellow, FontStyle.Regular, 8, 0);
                case "166b": return new SCCPositionAndStyle(Color.Yellow, FontStyle.Underline, 8, 0);
                case "16ec": return new SCCPositionAndStyle(Color.Magenta, FontStyle.Regular, 8, 0);
                case "166d": return new SCCPositionAndStyle(Color.Magenta, FontStyle.Underline, 8, 0);


                case "97d0": return new SCCPositionAndStyle(Color.White, FontStyle.Regular, 9, 0);
                case "9751": return new SCCPositionAndStyle(Color.White, FontStyle.Underline, 9, 0);
                case "97c2": return new SCCPositionAndStyle(Color.Green, FontStyle.Regular, 9, 0);
                case "9743": return new SCCPositionAndStyle(Color.Green, FontStyle.Underline, 9, 0);
                case "97c4": return new SCCPositionAndStyle(Color.Blue, FontStyle.Regular, 9, 0);
                case "9745": return new SCCPositionAndStyle(Color.Blue, FontStyle.Underline, 9, 0);
                case "9746": return new SCCPositionAndStyle(Color.Cyan, FontStyle.Regular, 9, 0);
                case "97c7": return new SCCPositionAndStyle(Color.Cyan, FontStyle.Underline, 9, 0);
                case "97c8": return new SCCPositionAndStyle(Color.Red, FontStyle.Regular, 9, 0);
                case "9749": return new SCCPositionAndStyle(Color.Red, FontStyle.Underline, 9, 0);
                case "974a": return new SCCPositionAndStyle(Color.Yellow, FontStyle.Regular, 9, 0);
                case "97cb": return new SCCPositionAndStyle(Color.Yellow, FontStyle.Underline, 9, 0);
                case "974c": return new SCCPositionAndStyle(Color.Magenta, FontStyle.Regular, 9, 0);
                case "97cd": return new SCCPositionAndStyle(Color.Magenta, FontStyle.Underline, 9, 0);

                case "9770": return new SCCPositionAndStyle(Color.White, FontStyle.Regular, 10, 0);
                case "97f1": return new SCCPositionAndStyle(Color.White, FontStyle.Underline, 10, 0);
                case "9762": return new SCCPositionAndStyle(Color.Green, FontStyle.Regular, 10, 0);
                case "97e3": return new SCCPositionAndStyle(Color.Green, FontStyle.Underline, 10, 0);
                case "9764": return new SCCPositionAndStyle(Color.Blue, FontStyle.Regular, 10, 0);
                case "97e5": return new SCCPositionAndStyle(Color.Blue, FontStyle.Underline, 10, 0);
                case "97e6": return new SCCPositionAndStyle(Color.Cyan, FontStyle.Regular, 10, 0);
                case "9767": return new SCCPositionAndStyle(Color.Cyan, FontStyle.Underline, 10, 0);
                case "9768": return new SCCPositionAndStyle(Color.Red, FontStyle.Regular, 10, 0);
                case "97e9": return new SCCPositionAndStyle(Color.Red, FontStyle.Underline, 10, 0);
                case "97ea": return new SCCPositionAndStyle(Color.Yellow, FontStyle.Regular, 10, 0);
                case "976b": return new SCCPositionAndStyle(Color.Yellow, FontStyle.Underline, 10, 0);
                case "97ec": return new SCCPositionAndStyle(Color.Magenta, FontStyle.Regular, 10, 0);
                case "976d": return new SCCPositionAndStyle(Color.Magenta, FontStyle.Underline, 10, 0);


                case "10d0": return new SCCPositionAndStyle(Color.White, FontStyle.Regular, 11, 0);
                case "1051": return new SCCPositionAndStyle(Color.White, FontStyle.Underline, 11, 0);
                case "10c2": return new SCCPositionAndStyle(Color.Green, FontStyle.Regular, 11, 0);
                case "1043": return new SCCPositionAndStyle(Color.Green, FontStyle.Underline, 11, 0);
                case "10c4": return new SCCPositionAndStyle(Color.Blue, FontStyle.Regular, 11, 0);
                case "1045": return new SCCPositionAndStyle(Color.Blue, FontStyle.Underline, 11, 0);
                case "1046": return new SCCPositionAndStyle(Color.Cyan, FontStyle.Regular, 11, 0);
                case "10c7": return new SCCPositionAndStyle(Color.Cyan, FontStyle.Underline, 11, 0);
                case "10c8": return new SCCPositionAndStyle(Color.Red, FontStyle.Regular, 11, 0);
                case "1049": return new SCCPositionAndStyle(Color.Red, FontStyle.Underline, 11, 0);
                case "104a": return new SCCPositionAndStyle(Color.Yellow, FontStyle.Regular, 11, 0);
                case "10cb": return new SCCPositionAndStyle(Color.Yellow, FontStyle.Underline, 11, 0);
                case "104c": return new SCCPositionAndStyle(Color.Magenta, FontStyle.Regular, 11, 0);
                case "10cd": return new SCCPositionAndStyle(Color.Magenta, FontStyle.Underline, 11, 0);


                case "13d0": return new SCCPositionAndStyle(Color.White, FontStyle.Regular, 12, 0);
                case "1351": return new SCCPositionAndStyle(Color.White, FontStyle.Underline, 12, 0);
                case "13c2": return new SCCPositionAndStyle(Color.Green, FontStyle.Regular, 12, 0);
                case "1343": return new SCCPositionAndStyle(Color.Green, FontStyle.Underline, 12, 0);
                case "13c4": return new SCCPositionAndStyle(Color.Blue, FontStyle.Regular, 12, 0);
                case "1345": return new SCCPositionAndStyle(Color.Blue, FontStyle.Underline, 12, 0);
                case "1346": return new SCCPositionAndStyle(Color.Cyan, FontStyle.Regular, 12, 0);
                case "13c7": return new SCCPositionAndStyle(Color.Cyan, FontStyle.Underline, 12, 0);
                case "13c8": return new SCCPositionAndStyle(Color.Red, FontStyle.Regular, 12, 0);
                case "1349": return new SCCPositionAndStyle(Color.Red, FontStyle.Underline, 12, 0);
                case "134a": return new SCCPositionAndStyle(Color.Yellow, FontStyle.Regular, 12, 0);
                case "13cb": return new SCCPositionAndStyle(Color.Yellow, FontStyle.Underline, 12, 0);
                case "134c": return new SCCPositionAndStyle(Color.Magenta, FontStyle.Regular, 12, 0);
                case "13cd": return new SCCPositionAndStyle(Color.Magenta, FontStyle.Underline, 12, 0);

                case "1370": return new SCCPositionAndStyle(Color.White, FontStyle.Regular, 13, 0);
                case "13f1": return new SCCPositionAndStyle(Color.White, FontStyle.Underline, 13, 0);
                case "1362": return new SCCPositionAndStyle(Color.Green, FontStyle.Regular, 13, 0);
                case "13e3": return new SCCPositionAndStyle(Color.Green, FontStyle.Underline, 13, 0);
                case "1364": return new SCCPositionAndStyle(Color.Blue, FontStyle.Regular, 13, 0);
                case "13e5": return new SCCPositionAndStyle(Color.Blue, FontStyle.Underline, 13, 0);
                case "13e6": return new SCCPositionAndStyle(Color.Cyan, FontStyle.Regular, 13, 0);
                case "1367": return new SCCPositionAndStyle(Color.Cyan, FontStyle.Underline, 13, 0);
                case "1368": return new SCCPositionAndStyle(Color.Red, FontStyle.Regular, 13, 0);
                case "13e9": return new SCCPositionAndStyle(Color.Red, FontStyle.Underline, 13, 0);
                case "13ea": return new SCCPositionAndStyle(Color.Yellow, FontStyle.Regular, 13, 0);
                case "136b": return new SCCPositionAndStyle(Color.Yellow, FontStyle.Underline, 13, 0);
                case "13ec": return new SCCPositionAndStyle(Color.Magenta, FontStyle.Regular, 13, 0);
                case "136d": return new SCCPositionAndStyle(Color.Magenta, FontStyle.Underline, 13, 0);


                case "94d0": return new SCCPositionAndStyle(Color.White, FontStyle.Regular, 14, 0);
                case "9451": return new SCCPositionAndStyle(Color.White, FontStyle.Underline, 14, 0);
                case "94c2": return new SCCPositionAndStyle(Color.Green, FontStyle.Regular, 14, 0);
                case "9443": return new SCCPositionAndStyle(Color.Green, FontStyle.Underline, 14, 0);
                case "94c4": return new SCCPositionAndStyle(Color.Blue, FontStyle.Regular, 14, 0);
                case "9445": return new SCCPositionAndStyle(Color.Blue, FontStyle.Underline, 14, 0);
                case "9446": return new SCCPositionAndStyle(Color.Cyan, FontStyle.Regular, 14, 0);
                case "94c7": return new SCCPositionAndStyle(Color.Cyan, FontStyle.Underline, 14, 0);
                case "94c8": return new SCCPositionAndStyle(Color.Red, FontStyle.Regular, 14, 0);
                case "9449": return new SCCPositionAndStyle(Color.Red, FontStyle.Underline, 14, 0);
                case "944a": return new SCCPositionAndStyle(Color.Yellow, FontStyle.Regular, 14, 0);
                case "94cb": return new SCCPositionAndStyle(Color.Yellow, FontStyle.Underline, 14, 0);
                case "944c": return new SCCPositionAndStyle(Color.Magenta, FontStyle.Regular, 14, 0);
                case "94cd": return new SCCPositionAndStyle(Color.Magenta, FontStyle.Underline, 14, 0);

                case "9470": return new SCCPositionAndStyle(Color.White, FontStyle.Regular, 15, 0);
                case "94f1": return new SCCPositionAndStyle(Color.White, FontStyle.Underline, 15, 0);
                case "9462": return new SCCPositionAndStyle(Color.Green, FontStyle.Regular, 15, 0);
                case "94e3": return new SCCPositionAndStyle(Color.Green, FontStyle.Underline, 15, 0);
                case "9464": return new SCCPositionAndStyle(Color.Blue, FontStyle.Regular, 15, 0);
                case "94e5": return new SCCPositionAndStyle(Color.Blue, FontStyle.Underline, 15, 0);
                case "94e6": return new SCCPositionAndStyle(Color.Cyan, FontStyle.Regular, 15, 0);
                case "9467": return new SCCPositionAndStyle(Color.Cyan, FontStyle.Underline, 15, 0);
                case "9468": return new SCCPositionAndStyle(Color.Red, FontStyle.Regular, 15, 0);
                case "94e9": return new SCCPositionAndStyle(Color.Red, FontStyle.Underline, 15, 0);
                case "94ea": return new SCCPositionAndStyle(Color.Yellow, FontStyle.Regular, 15, 0);
                case "946b": return new SCCPositionAndStyle(Color.Yellow, FontStyle.Underline, 15, 0);
                case "94ec": return new SCCPositionAndStyle(Color.Magenta, FontStyle.Regular, 15, 0);
                case "946d": return new SCCPositionAndStyle(Color.Magenta, FontStyle.Underline, 15, 0);



                //Columns 4-28

                case "9152": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 1, 4);
                case "91d3": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 1, 4);
                case "9154": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 1, 8);
                case "91d5": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 1, 8);
                case "91d6": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 1, 12);
                case "9157": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 1, 12);
                case "9158": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 1, 16);
                case "91d9": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 1, 16);
                case "91da": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 1, 20);
                case "915b": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 1, 20);
                case "91dc": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 1, 24);
                case "915d": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 1, 24);
                case "915e": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 1, 28);
                case "91df": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 1, 28);

                case "91f2": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 2, 4);
                case "9173": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 2, 4);
                case "91f4": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 2, 8);
                case "9175": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 2, 8);
                case "9176": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 2, 12);
                case "91f7": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 2, 12);
                case "91f8": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 2, 16);
                case "9179": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 2, 16);
                case "917a": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 2, 20);
                case "91fb": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 2, 20);
                case "917c": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 2, 24);
                case "91fd": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 2, 24);
                case "91fe": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 2, 28);
                case "917f": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 2, 28);


                case "9252": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 3, 4);
                case "92d3": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 3, 4);
                case "9254": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 3, 8);
                case "92d5": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 3, 8);
                case "92d6": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 3, 12);
                case "9257": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 3, 12);
                case "9258": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 3, 16);
                case "92d9": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 3, 16);
                case "92da": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 3, 20);
                case "925b": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 3, 20);
                case "92dc": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 3, 24);
                case "925d": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 3, 24);
                case "925e": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 3, 28);
                case "92df": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 3, 28);

                case "92f2": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 4, 4);
                case "9273": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 4, 4);
                case "92f4": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 4, 8);
                case "9275": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 4, 8);
                case "9276": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 4, 12);
                case "92f7": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 4, 12);
                case "92f8": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 4, 16);
                case "9279": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 4, 16);
                case "927a": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 4, 20);
                case "92fb": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 4, 20);
                case "927c": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 4, 24);
                case "92fd": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 4, 24);
                case "92fe": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 4, 28);
                case "927f": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 4, 28);


                case "1552": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 5, 4);
                case "15d3": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 5, 4);
                case "1554": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 5, 8);
                case "15d5": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 5, 8);
                case "15d6": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 5, 12);
                case "1557": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 5, 12);
                case "1558": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 5, 16);
                case "15d9": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 5, 16);
                case "15da": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 5, 20);
                case "155b": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 5, 20);
                case "15dc": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 5, 24);
                case "155d": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 5, 24);
                case "155e": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 5, 28);
                case "15df": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 5, 28);

                case "15f2": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 6, 4);
                case "1573": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 6, 4);
                case "15f4": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 6, 8);
                case "1575": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 6, 8);
                case "1576": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 6, 12);
                case "15f7": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 6, 12);
                case "15f8": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 6, 16);
                case "1579": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 6, 16);
                case "157a": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 6, 20);
                case "15fb": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 6, 20);
                case "157c": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 6, 24);
                case "15fd": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 6, 24);
                case "15fe": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 6, 28);
                case "157f": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 6, 28);


                case "1652": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 7, 4);
                case "16d3": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 7, 4);
                case "1654": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 7, 8);
                case "16d5": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 7, 8);
                case "16d6": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 7, 12);
                case "1657": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 7, 12);
                case "1658": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 7, 16);
                case "16d9": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 7, 16);
                case "16da": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 7, 20);
                case "165b": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 7, 20);
                case "16dc": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 7, 24);
                case "165d": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 7, 24);
                case "165e": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 7, 28);
                case "16df": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 7, 28);

                case "16f2": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 8, 4);
                case "1673": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 8, 4);
                case "16f4": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 8, 8);
                case "1675": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 8, 8);
                case "1676": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 8, 12);
                case "16f7": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 8, 12);
                case "16f8": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 8, 16);
                case "1679": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 8, 16);
                case "167a": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 8, 20);
                case "16fb": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 8, 20);
                case "167c": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 8, 24);
                case "16fd": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 8, 24);
                case "16fe": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 8, 28);
                case "167f": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 8, 28);


                case "9752": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 9, 4);
                case "97d3": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 9, 4);
                case "9754": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 9, 8);
                case "97d5": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 9, 8);
                case "97d6": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 9, 12);
                case "9757": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 9, 12);
                case "9758": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 9, 16);
                case "97d9": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 9, 16);
                case "97da": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 9, 20);
                case "975b": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 9, 20);
                case "97dc": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 9, 24);
                case "975d": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 9, 24);
                case "975e": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 9, 28);
                case "97df": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 9, 28);

                case "97f2": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 10, 4);
                case "9773": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 10, 4);
                case "97f4": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 10, 8);
                case "9775": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 10, 8);
                case "9776": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 10, 12);
                case "97f7": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 10, 12);
                case "97f8": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 10, 16);
                case "9779": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 10, 16);
                case "977a": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 10, 20);
                case "97fb": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 10, 20);
                case "977c": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 10, 24);
                case "97fd": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 10, 24);
                case "97fe": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 10, 28);
                case "977f": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 10, 28);


                case "1052": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 11, 4);
                case "10d3": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 11, 4);
                case "1054": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 11, 8);
                case "10d5": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 11, 8);
                case "10d6": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 11, 12);
                case "1057": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 11, 12);
                case "1058": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 11, 16);
                case "10d9": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 11, 16);
                case "10da": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 11, 20);
                case "105b": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 11, 20);
                case "10dc": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 11, 24);
                case "105d": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 11, 24);
                case "105e": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 11, 28);
                case "10df": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 11, 28);


                case "1352": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 12, 4);
                case "13d3": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 12, 4);
                case "1354": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 12, 8);
                case "13d5": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 12, 8);
                case "13d6": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 12, 12);
                case "1357": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 12, 12);
                case "1358": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 12, 16);
                case "13d9": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 12, 16);
                case "13da": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 12, 20);
                case "135b": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 12, 20);
                case "13dc": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 12, 24);
                case "135d": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 12, 24);
                case "135e": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 12, 28);
                case "13df": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 12, 28);

                case "13f2": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 13, 4);
                case "1373": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 13, 4);
                case "13f4": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 13, 8);
                case "1375": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 13, 8);
                case "1376": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 13, 12);
                case "13f7": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 13, 12);
                case "13f8": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 13, 16);
                case "1379": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 13, 16);
                case "137a": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 13, 20);
                case "13fb": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 13, 20);
                case "137c": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 13, 24);
                case "13fd": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 13, 24);
                case "13fe": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 13, 28);
                case "137f": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 13, 28);


                case "9452": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 14, 4);
                case "94d3": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 14, 4);
                case "9454": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 14, 8);
                case "94d5": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 14, 8);
                case "94d6": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 14, 12);
                case "9457": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 14, 12);
                case "9458": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 14, 16);
                case "94d9": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 14, 16);
                case "94da": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 14, 20);
                case "945b": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 14, 20);
                case "94dc": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 14, 24);
                case "945d": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 14, 24);
                case "945e": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 14, 28);
                case "94df": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 14, 28);

                case "94f2": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 15, 4);
                case "9473": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 15, 4);
                case "94f4": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 15, 8);
                case "9475": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 15, 8);
                case "9476": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 15, 12);
                case "94f7": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 15, 12);
                case "94f8": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 15, 16);
                case "9479": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 15, 16);
                case "947a": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 15, 20);
                case "94fb": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 15, 20);
                case "947c": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 15, 24);
                case "94fd": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 15, 24);
                case "94fe": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, 15, 28);
                case "947f": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Underline, 15, 28);


                // mid-row commands
                case "9120": return new SCCPositionAndStyle(Color.White, FontStyle.Regular, -1, -1);
                case "91a1": return new SCCPositionAndStyle(Color.White, FontStyle.Underline, -1, -1);
                case "91a2": return new SCCPositionAndStyle(Color.Green, FontStyle.Regular, -1, -1);
                case "9123": return new SCCPositionAndStyle(Color.Green, FontStyle.Underline, -1, -1);
                case "91a4": return new SCCPositionAndStyle(Color.Blue, FontStyle.Regular, -1, -1);
                case "9125": return new SCCPositionAndStyle(Color.Blue, FontStyle.Underline, -1, -1);
                case "9126": return new SCCPositionAndStyle(Color.Cyan, FontStyle.Regular, -1, -1);
                case "91a7": return new SCCPositionAndStyle(Color.Cyan, FontStyle.Underline, -1, -1);
                case "91a8": return new SCCPositionAndStyle(Color.Red, FontStyle.Regular, -1, -1);
                case "9129": return new SCCPositionAndStyle(Color.Red, FontStyle.Underline, -1, -1);
                case "912a": return new SCCPositionAndStyle(Color.Yellow, FontStyle.Regular, -1, -1);
                case "91ab": return new SCCPositionAndStyle(Color.Yellow, FontStyle.Underline, -1, -1);
                case "912c": return new SCCPositionAndStyle(Color.Magenta, FontStyle.Regular, -1, -1);
                case "91ad": return new SCCPositionAndStyle(Color.Magenta, FontStyle.Underline, -1, -1);

                case "91ae": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Italic, -1, -1);
                case "912f": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Italic | FontStyle.Underline, -1, -1);

                case "94a8": return new SCCPositionAndStyle(Color.Transparent, FontStyle.Regular, -1, -1); // turn flash on
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
                    TimeCode startTime = ParseTimeCode(s.Substring(0, match.Length-1));
                    string text = GetSccText(s.Substring(match.Index), ref _errorCount);

                    if (text == "942c 942c" || text == "942c")
                    {
                        p.EndTime = new TimeCode(TimeSpan.FromMilliseconds(startTime.TotalMilliseconds));
                    }
                    else
                    {
                        p = new Paragraph(startTime, new TimeCode(TimeSpan.FromMilliseconds(startTime.TotalMilliseconds)), text);
                        subtitle.Paragraphs.Add(p);
                    }
                }
            }
            for (int i = subtitle.Paragraphs.Count - 2; i >= 0; i--)
            {
                p = subtitle.GetParagraphOrDefault(i);
                Paragraph next = subtitle.GetParagraphOrDefault(i + 1);
                if (p != null && next != null && p.EndTime.TotalMilliseconds == p.StartTime.TotalMilliseconds)
                    p.EndTime = new TimeCode(TimeSpan.FromMilliseconds(next.StartTime.TotalMilliseconds));
                if (next != null && string.IsNullOrEmpty(next.Text))
                    subtitle.Paragraphs.Remove(next);
            }
            p = subtitle.GetParagraphOrDefault(0);
            if (p != null && string.IsNullOrEmpty(p.Text))
                subtitle.Paragraphs.Remove(p);

            subtitle.Renumber(1);
        }

        public static string GetSccText(string s, ref int errorCount)
        {
            int y = 0;
            string[] parts = s.Split(" \t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder();
            bool first = true;
            bool italicOn = false;
            foreach (var part in parts)
            {
                if (part.Length == 4)
                {
                    if (part == "94ae" || part == "9420")
                    {
                    }
                    else
                    {
                        var cp = GetColorAndPosition(part);
                        if (cp != null)
                        {
                            if (cp.Y > 0 && y > 0 && cp.Y > y)
                                sb.AppendLine();
                            if (cp.Y > 0)
                                y = cp.Y;
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
                            string result = GetLetter(part);
                            if (part == "94e0")
                            {
                                if (!sb.ToString().EndsWith(Environment.NewLine))
                                    sb.AppendLine();
                            }
                            else if (part == "2c94"|| part == "2c52")
                            {
                            }
                            else if (result == null)
                            {
                                sb.Append(GetLetter(part.Substring(0, 2)));
                                sb.Append(GetLetter(part.Substring(2, 2)));
                            }
                            else
                            {
                                sb.Append(result);
                            }
                        }
                    }
                }
                else if (part.Length > 0)
                {
                    if (first)
                        errorCount++;
                }
                first = false;
            }
            string res = sb.ToString().Replace("<i></i>", string.Empty).Replace("</i><i>", string.Empty);
            res = res.Replace("  ", " ").Replace("  ", " ").Replace(Environment.NewLine + " ", Environment.NewLine).Trim();
            return Utilities.FixInvalidItalicTags(res);
        }

        private static string GetLetter(string hexCode)
        {
            int index = _letterCodes.IndexOf(hexCode.ToLower());
            if (index < 0)
                return null;

            return _letters[index];
        }

        private TimeCode ParseTimeCode(string start)
        {
            string[] arr = start.Split(':');

            int milliseconds = (int)((1000 / Configuration.Settings.General.CurrentFrameRate) * int.Parse(arr[3]));
            if (milliseconds > 999)
                milliseconds = 999;

            var ts = new TimeSpan(0, int.Parse(arr[0]), int.Parse(arr[1]), int.Parse(arr[2]), milliseconds);
            return new TimeCode(ts);
        }

    }
}


