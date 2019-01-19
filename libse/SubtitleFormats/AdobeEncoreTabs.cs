using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class AdobeEncoreTabs : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d\d:\d\d:\d\d:\d\d\t\d\d:\d\d:\d\d:\d\d\t", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Adobe Encore (tabs)";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                //00:00:54:08   00:00:58:06 - Saucers... - ... a dry lake bed.  (newline is \r)
                sb.AppendLine($"{EncodeTimeCode(p.StartTime)}\t{EncodeTimeCode(p.EndTime)}\t{HtmlUtil.RemoveHtmlTags(p.Text, true).Replace(Environment.NewLine, "\r")}");
            }
            return sb.ToString();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            //00:03:15:22 (last is frame)
            return time.ToHHMMSSFF();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            //00:03:15:22 00:03:23:10 This is line one.
            //This is line two.
            Paragraph p = null;
            subtitle.Paragraphs.Clear();
            _errorCount = 0;
            foreach (string line in lines)
            {
                if (RegexTimeCodes.IsMatch(line))
                {
                    string temp = line.Substring(0, RegexTimeCodes.Match(line).Length);
                    string start = temp.Substring(0, 11);
                    string end = temp.Substring(12, 11);

                    string[] startParts = start.Split(SplitCharColon, StringSplitOptions.RemoveEmptyEntries);
                    string[] endParts = end.Split(SplitCharColon, StringSplitOptions.RemoveEmptyEntries);
                    if (startParts.Length == 4 && endParts.Length == 4)
                    {
                        string text = line.Remove(0, RegexTimeCodes.Match(line).Length - 1).Trim();
                        if (!text.Contains(Environment.NewLine))
                        {
                            text = text.Replace("\r", Environment.NewLine);
                        }

                        p = new Paragraph(DecodeTimeCodeFramesFourParts(startParts), DecodeTimeCodeFramesFourParts(endParts), text);
                        subtitle.Paragraphs.Add(p);
                    }
                }
                else if (string.IsNullOrWhiteSpace(line))
                {
                }
                else
                {
                    if (p?.Text.Length < 200)
                    {
                        p.Text = (p.Text + Environment.NewLine + line).Trim();
                    }
                }
            }

            subtitle.Renumber();
        }

    }
}
