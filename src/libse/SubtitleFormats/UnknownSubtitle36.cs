using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{

    //00:00:31:17           ,           00:00:35:12           , Signori e Signorine come state?|Va tutto bene?

    //00:00:35:24           ,           00:00:40:17           ,Oggi riceveremo un |grande artista che viene dall’Africa.

    //00:00:40:20           ,           00:00:44:12           ,Indovinate come si chiama?|Indovinate come si chiama?

    //00:00:44:15           ,           00:00:48:24           ,Si chiama Bella Balde.

    //00:00:49:04           ,           00:00:50:16           ,Grazie Signore e Signori.

    //00:00:50:18           ,           00:00:52:24           ,per ricevere questo grande artista| che viene dall’Africa.

    //00:00:53:00           ,           00:00:55:08           ,Grazie di nuovo,|grazie ancora.

    //---------------------------------------------------

    //00:02:13:14           ,           00:02:18:21            ,Stiamo preparando un festival|nel centro scolastico.

    //00:02:20:16            ,           00:02:24:00           ,Gente di questo quartiere |Un gran festival!

    public class UnknownSubtitle36 : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d+:\d+:\d+:\d+           ,           \d+:\d+:\d+:\d+           ,", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Unknown 36";

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (lines.Count > 0 && lines[0] != null && lines[0].StartsWith("{\\rtf1", StringComparison.Ordinal))
            {
                return false;
            }

            return base.IsMine(lines, fileName);
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            string paragraphWriteFormat = "{0}           ,           {1}           ,{2}" + Environment.NewLine;
            const string timeFormat = "{0:00}:{1:00}:{2:00}:{3:00}";
            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string startTime = string.Format(timeFormat, p.StartTime.Hours, p.StartTime.Minutes, p.StartTime.Seconds, MillisecondsToFramesMaxFrameRate(p.StartTime.Milliseconds));
                string endTime = string.Format(timeFormat, p.EndTime.Hours, p.EndTime.Minutes, p.EndTime.Seconds, MillisecondsToFramesMaxFrameRate(p.EndTime.Milliseconds));
                sb.AppendFormat(paragraphWriteFormat, startTime, endTime, HtmlUtil.RemoveHtmlTags(p.Text.Replace(Environment.NewLine, " | ")));
            }
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            int number = 1;
            char[] splitChar = { ',' };
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || string.IsNullOrWhiteSpace(line.Trim('-')))
                {
                    continue;
                }

                Match match;
                if (line.Length > 57 && (match = RegexTimeCodes.Match(line)).Success)
                {
                    try
                    {
                        string[] timeParts = line.Split(splitChar, StringSplitOptions.RemoveEmptyEntries);
                        TimeCode startTime = DecodeTimeCodeFrames(timeParts[0].Trim(), SplitCharColon);
                        TimeCode endTime = DecodeTimeCodeFrames(timeParts[1].Trim(), SplitCharColon);
                        var p = new Paragraph { Number = number++, StartTime = startTime, EndTime = endTime };
                        p.Text = line.Substring(match.Length).Replace(" | ", Environment.NewLine).Replace("|", Environment.NewLine);
                        subtitle.Paragraphs.Add(p);
                    }
                    catch
                    {
                        _errorCount++;
                    }
                }
                else
                {
                    _errorCount++;
                }
            }
        }
    }
}
