using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class SubViewer20 : SubtitleFormat
    {
        private enum ExpectingLine
        {
            TimeCodes,
            Text
        }

        private static readonly Regex RegexTimeCodes = new Regex(@"^\d\d:\d\d:\d\d.\d+,\d\d:\d\d:\d\d.\d+$", RegexOptions.Compiled);

        public override string Extension => ".sub";

        public override string Name => "SubViewer 2.0";

        public override bool IsMine(List<string> lines, string fileName)
        {
            var sbv = new YouTubeSbv();
            if (sbv.IsMine(lines, fileName) && !string.Join(string.Empty, lines.ToArray()).Contains("[br]"))
            {
                return false;
            }

            return base.IsMine(lines, fileName);
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            const string paragraphWriteFormat = "{0:00}:{1:00}:{2:00}.{3:00},{4:00}:{5:00}:{6:00}.{7:00}{8}{9}";
            const string header = @"[INFORMATION]
[TITLE]{0}
[AUTHOR]
[SOURCE]
[PRG]
[FILEPATH]
[DELAY]0
[CD TRACK]0
[COMMENT]
[END INFORMATION]
[SUBTITLE]
[COLF]&H000000,[STYLE]bd,[SIZE]25,[FONT]Arial
";
            //00:00:06.61,00:00:13.75
            //text1[br]text2
            var sb = new StringBuilder();
            if (subtitle.Header != null && subtitle.Header.Contains("[INFORMATION]"))
            {
                sb.AppendLine(subtitle.Header);
            }
            else
            {
                sb.AppendFormat(header, title);
            }
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string text = p.Text.Replace(Environment.NewLine, "[br]");
                text = text.Replace("<i>", "{\\i1}");
                text = text.Replace("</i>", "{\\i0}");
                text = text.Replace("<b>", "{\\b1}");
                text = text.Replace("</b>", "{\\b0}");
                text = text.Replace("<u>", "{\\u1}");
                text = text.Replace("</u>", "{\\u0}");

                sb.AppendLine(string.Format(paragraphWriteFormat,
                                        p.StartTime.Hours,
                                        p.StartTime.Minutes,
                                        p.StartTime.Seconds,
                                        RoundTo2Cifres(p.StartTime.Milliseconds),
                                        p.EndTime.Hours,
                                        p.EndTime.Minutes,
                                        p.EndTime.Seconds,
                                        RoundTo2Cifres(p.EndTime.Milliseconds),
                                        Environment.NewLine,
                                        text));
                sb.AppendLine();
            }
            return sb.ToString().Trim();
        }

        private static int RoundTo2Cifres(int milliseconds)
        {
            return (int)Math.Round(milliseconds / 10.0);
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            var paragraph = new Paragraph();
            var expecting = ExpectingLine.TimeCodes;
            _errorCount = 0;
            char[] splitChars = { ':', ',', '.' };
            subtitle.Paragraphs.Clear();
            var header = new StringBuilder();
            foreach (string line in lines)
            {
                if (subtitle.Paragraphs.Count == 0 && expecting == ExpectingLine.TimeCodes && line.StartsWith("[", StringComparison.Ordinal))
                {
                    header.AppendLine(line);
                }
                else if (line.Length > 20 && char.IsDigit(line[0]) && RegexTimeCodes.IsMatch(line))
                {
                    var parts = line.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 8)
                    {
                        try
                        {
                            paragraph.StartTime = DecodeTimeCode(parts, 0);
                            paragraph.EndTime = DecodeTimeCode(parts, 4);
                            expecting = ExpectingLine.Text;
                        }
                        catch
                        {
                            _errorCount++;
                            expecting = ExpectingLine.TimeCodes;
                        }
                    }
                }
                else if (expecting == ExpectingLine.Text && line.Length > 0)
                {
                    string text = line.Replace("[br]", Environment.NewLine);
                    if (text.Contains("{\\", StringComparison.Ordinal))
                    {
                        text = text.Replace("{\\i1}", "<i>");
                        text = text.Replace("{\\i0}", "</i>");
                        text = text.Replace("{\\i}", "</i>");
                        text = text.Replace("{\\b1}", "<b>");
                        text = text.Replace("{\\b0}", "</b>");
                        text = text.Replace("{\\b}", "</b>");
                        text = text.Replace("{\\u1}", "<u>");
                        text = text.Replace("{\\u0}", "</u>");
                        text = text.Replace("{\\u}", "</u>");
                    }
                    paragraph.Text = text;
                    subtitle.Paragraphs.Add(paragraph);
                    paragraph = new Paragraph();
                    expecting = ExpectingLine.TimeCodes;
                }
            }
            subtitle.Renumber();
            if (header.ToString().Contains("[INFORMATION]"))
            {
                subtitle.Header = header.ToString().TrimEnd();
            }
        }

        public static TimeCode DecodeTimeCode(string[] encodedTimeCode, int index)
        {
            // Hours, Minutes, Seconds, Milliseconds / 10.
            return new TimeCode(int.Parse(encodedTimeCode[index]),
                int.Parse(encodedTimeCode[index + 1]),
                int.Parse(encodedTimeCode[index + 2]),
                int.Parse(encodedTimeCode[index + 3]) * 10);
        }
    }
}
