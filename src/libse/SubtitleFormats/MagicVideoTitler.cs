using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class MagicVideoTitler : SubtitleFormat
    {
        public override string Extension => ".mvt";
        public override string Name => "Magic Video Titler";
        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            sb.AppendLine("`ABOUT=Subtitle Edit export to Magic Video Titler III Professional");
            sb.AppendLine();

            foreach (var p in subtitle.Paragraphs)
            {
                sb.AppendLine($"`TITLIN={(p.StartTime.TotalMilliseconds / 10):00000000}");
                sb.AppendLine($"`TITLOUT={(p.EndTime.TotalMilliseconds / 10):00000000}");

                var lineLetters = new[] { 'A', 'B', 'C', 'D', 'E' };
                var list = p.Text.SplitToLines();
                for (var index = 0; index < list.Count; index++)
                {
                    if (index < lineLetters.Length)
                    {
                        var line = list[index];
                        var lineLetter = lineLetters[index];
                        sb.AppendLine($"`TITL{lineLetter}=" + EncodeText(line));
                    }
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            var p = new Paragraph();
            foreach (var line in lines)
            {
                var s = line.Trim();

                if (s.StartsWith("`TITLIN=", StringComparison.Ordinal))
                {
                    if (p.EndTime.TotalMilliseconds > 0 && !string.IsNullOrEmpty(p.Text))
                    {
                        subtitle.Paragraphs.Add(p);
                    }

                    p = new Paragraph();
                    var numberText = s.Remove(0, 8).Trim();
                    if (long.TryParse(numberText, out var number))
                    {
                        p.StartTime.TotalMilliseconds = number * 10;
                    }
                }
                else if (s.StartsWith("`TITLOUT=", StringComparison.Ordinal))
                {
                    var numberText = s.Remove(0, 9).Trim();
                    if (long.TryParse(numberText, out var number))
                    {
                        p.EndTime.TotalMilliseconds = number * 10;
                    }
                }
                else if (s.Length > 7 && s[6] == '=' && s.StartsWith("`TITL", StringComparison.Ordinal))
                {
                    p.Text = (p.Text + Environment.NewLine + DecodeText(s.Remove(0, 7))).Trim();
                }
            }

            if (p.EndTime.TotalMilliseconds > 0 && !string.IsNullOrEmpty(p.Text))
            {
                subtitle.Paragraphs.Add(p);
            }

            subtitle.Renumber();
        }

        private static readonly Dictionary<string, string> DecodeDictionary = new Dictionary<string, string>
        {
            { "Q", "Lj" },
            { "q", "lj" },
            { "W", "Nj" },
            { "w", "nj" },
            { "|", "Ž" },
            { "\\", "ž" },
            { "}", "Đ" },
            { "]", "đ" },
            { "{", "Š" },
            { "[", "š" },
            { "\"", "Ć" },
            { "'", "ć" },
            { ":", "Č" },
            { ";", "č" },
            { "Y", "Dž" },
            { "y", "dž" },
            { "@", "\"" },
            { ">", ":" },
            { "`", "'" },
        };

        // Char-keyed views of DecodeDictionary: the per-character lookups allocated a one-char
        // (or two-char) string for every character of every line as the dictionary key.
        private static readonly Dictionary<char, string> DecodeByChar = BuildDecodeByChar();

        private static Dictionary<char, string> BuildDecodeByChar()
        {
            var dic = new Dictionary<char, string>();
            foreach (var kvp in DecodeDictionary)
            {
                if (kvp.Key.Length == 1)
                {
                    dic[kvp.Key[0]] = kvp.Value;
                }
            }

            return dic;
        }

        private static string DecodeText(string s)
        {
            var sb = new StringBuilder();
            foreach (var ch in s)
            {
                if (DecodeByChar.TryGetValue(ch, out var v))
                {
                    sb.Append(v);
                }
                else
                {
                    sb.Append(ch);
                }
            }

            return sb.ToString();
        }

        private static Dictionary<char, string> _encodeOneChar;
        private static Dictionary<int, string> _encodeTwoChars;

        public static string EncodeText(string s)
        {
            if (_encodeOneChar == null)
            {
                var one = new Dictionary<char, string>();
                var two = new Dictionary<int, string>();
                foreach (var kvp in DecodeDictionary)
                {
                    if (kvp.Value.Length == 1)
                    {
                        one.Add(kvp.Value[0], kvp.Key);
                    }
                    else if (kvp.Value.Length == 2)
                    {
                        two.Add((kvp.Value[0] << 16) | kvp.Value[1], kvp.Key);
                    }
                }

                _encodeTwoChars = two;
                _encodeOneChar = one;
            }

            // Several decoded values are TWO characters ("nj", "Lj", "Dz"...), so a per-character
            // lookup could never match them: the digraphs were left as-is while the reader kept
            // mapping the single ASCII letter back to them, so text drifted on every save.
            var sb = new StringBuilder();
            for (var i = 0; i < s.Length; i++)
            {
                if (i + 1 < s.Length && _encodeTwoChars.TryGetValue((s[i] << 16) | s[i + 1], out var twoCharValue))
                {
                    sb.Append(twoCharValue);
                    i++;
                    continue;
                }

                if (_encodeOneChar.TryGetValue(s[i], out var v))
                {
                    sb.Append(v);
                }
                else
                {
                    sb.Append(s[i]);
                }
            }

            return sb.ToString();
        }
    }
}
