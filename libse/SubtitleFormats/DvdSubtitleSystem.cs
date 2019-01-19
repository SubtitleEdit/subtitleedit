using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class DvdSubtitleSystem : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d\d:\d\d:\d\d:\d\d \d\d:\d\d:\d\d:\d\d ", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "DVD Subtitle System";

        public override bool IsMine(List<string> lines, string fileName)
        {
            var sb = new StringBuilder();
            foreach (string line in lines)
            {
                sb.AppendLine(line);
            }

            if (sb.ToString().Contains("#INPOINT OUTPOINT PATH"))
            {
                return false; // Pinnacle Impression
            }

            return base.IsMine(lines, fileName);
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                //00:03:15:22 00:03:23:10 This is line one.
                //This is line two.
                sb.AppendLine($"{EncodeTimeCode(p.StartTime)} {EncodeTimeCode(p.EndTime)} {HtmlUtil.RemoveHtmlTags(p.Text.Replace(Environment.NewLine, "//"), true)}");
            }
            return sb.ToString();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            //00:03:15:22 (last is ms div 10)
            return $"{time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}:{MillisecondsToFramesMaxFrameRate(time.Milliseconds):00}";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            //00:03:15:22 00:03:23:10 This is line one.
            //This is line two.
            subtitle.Paragraphs.Clear();
            _errorCount = 0;
            foreach (string line in lines)
            {
                // line must contain atleast 24 characters (time-code)...
                if (line.Length < 24)
                {
                    _errorCount += 10;
                    continue;
                }

                Match match = RegexTimeCodes.Match(line);

                if (match.Success)
                {
                    string temp = line.Substring(0, match.Length);
                    string start = temp.Substring(0, 11);
                    string end = temp.Substring(12, 11);

                    string[] startParts = start.Split(SplitCharColon, StringSplitOptions.RemoveEmptyEntries);
                    string[] endParts = end.Split(SplitCharColon, StringSplitOptions.RemoveEmptyEntries);
                    string text = line.Substring(match.Length).Trim();
                    text = text.Replace("//", Environment.NewLine);
                    var p = new Paragraph(DecodeTimeCodeFramesFourParts(startParts), DecodeTimeCodeFramesFourParts(endParts), text);
                    subtitle.Paragraphs.Add(p);
                }
            }

            subtitle.Renumber();
        }

    }
}
