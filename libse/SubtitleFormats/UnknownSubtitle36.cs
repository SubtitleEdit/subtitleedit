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
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d+:\d+:\d+:\d+           ,           \d+:\d+:\d+:\d+           ,.*$", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".txt"; }
        }

        public override string Name
        {
            get { return "Unknown 36"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (lines.Count > 0 && lines[0] != null && lines[0].StartsWith("{\\rtf1"))
                return false;

            var subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            const string paragraphWriteFormat = "{0}           ,           {1}           ,{2}\r\n";
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
            int number = 0;
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || string.IsNullOrWhiteSpace(line.Trim('-')))
                {
                    continue;
                }

                if (RegexTimeCodes.Match(line).Success)
                {
                    string[] threePart = line.Split(new[] { ',' }, StringSplitOptions.None);
                    var p = new Paragraph();
                    if (threePart.Length > 2 &&
                        line.Length > 58 &&
                        GetTimeCode(p.StartTime, threePart[0].Trim()) &&
                        GetTimeCode(p.EndTime, threePart[1].Trim()))
                    {
                        number++;
                        p.Number = number;
                        p.Text = line.Remove(0, 57).Trim().Replace(" | ", Environment.NewLine).Replace("|", Environment.NewLine);
                        subtitle.Paragraphs.Add(p);
                    }
                }
                else
                {
                    _errorCount++;
                }
            }
        }

        private static bool GetTimeCode(TimeCode timeCode, string timeString)
        {
            try
            {
                string[] timeParts = timeString.Split(':');
                timeCode.Hours = int.Parse(timeParts[0]);
                timeCode.Minutes = int.Parse(timeParts[1]);
                timeCode.Seconds = int.Parse(timeParts[2]);
                timeCode.Milliseconds = FramesToMillisecondsMax999(int.Parse(timeParts[3]));
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
