using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class SmartTitler : SubtitleFormat
    {
        private static readonly Regex RegexTimeCode = new Regex(@"^\d+ \d+ \d \d \d$", RegexOptions.Compiled);

        public override string Extension => ".tek";

        public override string Name => "Smart Titler";

        public override bool IsTimeBased => false;

        public override string ToText(Subtitle subtitle, string title)
        {
            //1.
            //8.03
            //10.06
            //- Labai aèiû.
            //- Jûs rimtai?

            //2.
            //16.00
            //19.06
            //Kaip reikalai ðunø grobimo versle?

            const string paragraphWriteFormat = "{0} {1} 1 1 0\r\n{2}";
            var sb = new StringBuilder();
            sb.AppendLine(@"ý Smart Titl Editor / Smart Titler  (A)(C)1992-2001. Dragutin Nikolic
ý Serial No: XXXXXXXXXXXXXX
ý Korisnik: Prava i Prevodi - prevodioci
ý
ý KONFIGURACIONI PODACI
ý Dozvoljeno slova u redu: 30
ý Vremenska korekcija:  1.0000000000E+00
ý Radjeno vremenskih korekcija: TRUE
ý Slovni raspored ASCIR
ý
ý                                       Kraj info blocka.");
            sb.AppendLine();
            foreach (var p in subtitle.Paragraphs)
            {
                var text = HtmlUtil.RemoveOpenCloseTags(p.Text, HtmlUtil.TagFont);
                sb.AppendLine(string.Format(paragraphWriteFormat, MillisecondsToFrames(p.StartTime.TotalMilliseconds), MillisecondsToFrames(p.EndTime.TotalMilliseconds), EncodeText(text)));
            }
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            Paragraph paragraph = null;
            _errorCount = 0;

            subtitle.Paragraphs.Clear();
            foreach (var line in lines)
            {
                var s = line.Trim();
                if (RegexTimeCode.IsMatch(s))
                {
                    if (paragraph != null)
                    {
                        subtitle.Paragraphs.Add(paragraph);
                    }

                    paragraph = new Paragraph();
                    var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 5)
                    {
                        try
                        {
                            paragraph.StartTime.TotalMilliseconds = FramesToMilliseconds(int.Parse(parts[0]));
                            paragraph.EndTime.TotalMilliseconds = FramesToMilliseconds(int.Parse(parts[1]));
                        }
                        catch
                        {
                            _errorCount++;
                        }
                    }
                }
                else if (paragraph != null && s.Length > 0)
                {
                    paragraph.Text = (paragraph.Text + Environment.NewLine + DecodeText(s)).Trim();
                    if (paragraph.Text.Length > 2000)
                    {
                        _errorCount += 100;
                        return;
                    }
                }
                else if (s.Length > 0 && !s.StartsWith('ý'))
                {
                    _errorCount++;
                }
            }

            if (paragraph != null)
            {
                subtitle.Paragraphs.Add(paragraph);
            }

            subtitle.Renumber();
        }

        private static readonly Dictionary<string, string> DecodeDictionary = new Dictionary<string, string>
        {
            { "Q", "Lj" },
            { "q", "lj" },
            { "W", "Nj" },
            { "w", "nj" },
            { "@", "Ž" },
            { "`", "ž" },
            { "\\", "Đ" },
            { "|", "đ" },
            { "[", "Š" },
            { "{", "š" },
            { "]", "Ć" },
            { "}", "ć" },
            { "^", "Č" },
            { "~", "č" },
            { "Y", "Dž" },
            { "y", "dž" },
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
