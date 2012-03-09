using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    class MidwayInscriberCGX : SubtitleFormat
    {
        static Regex regexTimeCodes = new Regex(@"<\d\d:\d\d:\d\d:\d\d> <\d\d:\d\d:\d\d:\d\d>$", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".txt"; }
        }

        public override string Name
        {
            get { return "Midway Inscriber CG-X"; }
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
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            StringBuilder sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.AppendLine(string.Format("{3} <{0}> <{1}>{2}", EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime), Environment.NewLine, Utilities.RemoveHtmlTags(p.Text)));
                //Var vi bedre end japanerne
                //eller bare mere heldige? <12:03:29:03> <12:03:35:06>
            }
            return sb.ToString();
        }

        private string EncodeTimeCode(TimeCode time)
        {
            //00:50:39:13 (last is frame)
            int frames = MillisecondsToFrames(time.Milliseconds);
            return string.Format("{0:00}:{1:00}:{2:00}:{3:00}", time.Hours, time.Minutes, time.Seconds, frames);
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            var sb = new StringBuilder();
            foreach (string line in lines)
                sb.Append(line);
            if (!sb.ToString().Contains("> <"))
                return;

            //Var vi bedre end japanerne
            //eller bare mere heldige? <12:03:29:03> <12:03:35:06>

            Paragraph p = null;
            subtitle.Paragraphs.Clear();
            sb = new StringBuilder();
            foreach (string line in lines)
            {
                if (line.Trim().Length > 0)
                {
                    if (regexTimeCodes.IsMatch(line))
                    {
                        int idx = regexTimeCodes.Match(line).Index;
                        string temp = line.Substring(0, idx);
                        sb.AppendLine(temp.Trim());

                        string start = line.Substring(idx + 1, 11);
                        string end = line.Substring(idx + 15, 11);

                        string[] startParts = start.Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        string[] endParts = end.Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        if (startParts.Length == 4 && endParts.Length == 4)
                        {
                            p = new Paragraph(DecodeTimeCode(startParts), DecodeTimeCode(endParts), sb.ToString().Trim());
                            subtitle.Paragraphs.Add(p);
                        }
                        sb = new StringBuilder();
                    }
                    else
                    {
                        sb.AppendLine(line.Trim());
                    }
                }
                if (sb.Length > 1000)
                    return;
            }
            subtitle.Renumber(1);
        }

        private TimeCode DecodeTimeCode(string[] parts)
        {
            //00:00:07:12
            string hour = parts[0];
            string minutes = parts[1];
            string seconds = parts[2];
            string frames = parts[3];

            int milliseconds = (int)((1000 / Configuration.Settings.General.CurrentFrameRate) * int.Parse(frames));
            if (milliseconds > 999)
                milliseconds = 999;

            TimeCode tc = new TimeCode(int.Parse(hour), int.Parse(minutes), int.Parse(seconds), milliseconds);
            return tc;
        }

    }
}

