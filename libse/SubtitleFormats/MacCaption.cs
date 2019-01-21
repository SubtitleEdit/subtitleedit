using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class MacCaption : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d\d:\d\d:\d\d:\d\d\t", RegexOptions.Compiled);

        public override string Extension => ".mcc";

        public override string Name => "MacCaption 1.0";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            sb.AppendLine(@"File Format=MacCaption_MCC V1.0
///////////////////////////////////////////////////////////////////////////////////
// Computer Prompting and Captioning Company
// Ancillary Data Packet Transfer File
//
// Permission to generate this format is granted provided that
// 1. This ANC Transfer file format is used on an as-is basis and no warranty is given, and
// 2. This entire descriptive information text is included in a generated .mcc file.
//
// General file format:
// HH:MM:SS:FF(tab)[Hexadecimal ANC data in groups of 2 characters]
// Hexadecimal data starts with the Ancillary Data Packet DID (Data ID defined in S291M)
// and concludes with the Check Sum following the User Data Words.
// Each time code line must contain at most one complete ancillary data packet.
// To transfer additional ANC Data successive lines may contain identical time code.
// Time Code Rate=[24, 25, 30, 30DF, 50, 60]
//
// ANC data bytes may be represented by one ASCII character according to the following schema:
// G FAh 00h 00h
// H 2 x (FAh 00h 00h)
// I 3 x (FAh 00h 00h)
// J 4 x (FAh 00h 00h)
// K 5 x (FAh 00h 00h)
// L 6 x (FAh 00h 00h)
// M 7 x (FAh 00h 00h)
// N 8 x (FAh 00h 00h)
// O 9 x (FAh 00h 00h)
// P FBh 80h 80h
// Q FCh 80h 80h
// R FDh 80h 80h
// S 96h 69h
// T 61h 01h
// U E1h 00h 00h
// Z 00h
//
///////////////////////////////////////////////////////////////////////////////////");
            sb.AppendLine();
            sb.AppendLine("UUID=" + Guid.NewGuid().ToString().ToUpperInvariant());// UUID=9F6112F4-D9D0-4AAF-AA95-854710D3B57A
            sb.AppendLine("Creation Program=Subtitle Edit");
            sb.AppendLine("Creation Date=" + DateTime.Now.ToLongDateString());
            sb.AppendLine("Creation Time=" + DateTime.Now.ToShortTimeString());
            sb.AppendLine();

            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                sb.AppendLine(string.Format("{0}\t{1}", ToTimeCode(p.StartTime.TotalMilliseconds), p.Text)); // TODO: Encode text - how???
                sb.AppendLine();

                Paragraph next = subtitle.GetParagraphOrDefault(i + 1);
                if (next == null || Math.Abs(next.StartTime.TotalMilliseconds - p.EndTime.TotalMilliseconds) > 100)
                {
                    sb.AppendLine(string.Format("{0}\t???", ToTimeCode(p.EndTime.TotalMilliseconds))); // TODO: Some end text???
                    sb.AppendLine();
                }
            }
            return sb.ToString();
        }

        private static string ToTimeCode(double totalMilliseconds)
        {
            TimeSpan ts = TimeSpan.FromMilliseconds(totalMilliseconds);
            return $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}:{MillisecondsToFramesMaxFrameRate(ts.Milliseconds):00}";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            Paragraph p = null;
            var header = new StringBuilder();
            char[] splitChars = { ':', ';', ',' };
            foreach (string line in lines)
            {
                string s = line.Trim();

                if (string.IsNullOrEmpty(s) || s.StartsWith("//", StringComparison.Ordinal) || s.StartsWith("File Format=MacCaption_MCC", StringComparison.Ordinal) || s.StartsWith("UUID=", StringComparison.Ordinal) ||
                    s.StartsWith("Creation Program=") || s.StartsWith("Creation Date=") || s.StartsWith("Creation Time=") ||
                    s.StartsWith("Code Rate=", StringComparison.Ordinal) || s.StartsWith("Time Code Rate=", StringComparison.Ordinal))
                {
                    header.AppendLine(line);
                }
                else
                {
                    var match = RegexTimeCodes.Match(s);
                    if (match.Success)
                    {
                        TimeCode startTime = DecodeTimeCodeFrames(s.Substring(0, match.Length - 1), splitChars);
                        string text = GetSccText(s.Substring(match.Index));

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

        public static string GetSccText(string s)
        {
            string[] parts = s.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder();
            foreach (string part in ExecuteReplacesAndGetParts(parts))
            {
                try
                {
                    // TODO: How to decode???
                    int num = int.Parse(part, System.Globalization.NumberStyles.HexNumber);
                    if (num >= 32 && num <= 255)
                    {
                        var encoding = Encoding.GetEncoding("ISO-8859-1");
                        byte[] bytes = new byte[1];
                        bytes[0] = (byte)num;
                        sb.Append(encoding.GetString(bytes));
                    }
                }
                catch
                {
                    // ignored
                }
            }
            string res = sb.ToString().Replace("<i></i>", string.Empty).Replace("</i><i>", string.Empty);
            res = res.Replace("♪♪", "♪");
            res = res.Replace("'''", "'");
            res = res.Replace("  ", " ").Replace("  ", " ").Replace(Environment.NewLine + " ", Environment.NewLine).Trim();
            return HtmlUtil.FixInvalidItalicTags(res);
        }

        private static List<string> ExecuteReplacesAndGetParts(string[] parts)
        {
            var list = new List<string>();
            if (parts.Length != 2)
            {
                return list;
            }
            string s = parts[1];
            s = s.Replace("G", "FA0000");
            s = s.Replace("H", "FA0000FA0000");
            s = s.Replace("I", "FA0000FA0000FA0000");
            s = s.Replace("J", "FA0000FA0000FA0000FA0000");
            s = s.Replace("K", "FA0000FA0000FA0000FA0000FA0000");
            s = s.Replace("L", "FA0000FA0000FA0000FA0000FA0000FA0000");
            s = s.Replace("M", "FA0000FA0000FA0000FA0000FA0000FA0000FA0000");
            s = s.Replace("N", "FA0000FA0000FA0000FA0000FA0000FA0000FA0000FA0000");
            s = s.Replace("O", "FA0000FA0000FA0000FA0000FA0000FA0000FA0000FA0000FA0000");
            s = s.Replace("P", "FB8080");
            s = s.Replace("Q", "FC8080");
            s = s.Replace("R", "FD80h80");
            s = s.Replace("S", "9669");
            s = s.Replace("T", "6101");
            s = s.Replace("U", "E10000");
            s = s.Replace("Z", "00");
            for (int i = 0; i < s.Length; i += 4)
            {
                string sub = s.Substring(i);
                if (sub.Length >= 2)
                {
                    list.Add(sub.Substring(0, 2));
                }
            }
            return list;
        }

    }
}
