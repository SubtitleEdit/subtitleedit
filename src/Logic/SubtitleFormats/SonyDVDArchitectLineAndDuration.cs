using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class SonyDVDArchitectLineAndDuration : SubtitleFormat
    {
        public override string Extension
        {
            get { return ".txt"; }
        }

        public override string Name
        {
            get { return "Sony DVDArchitect line/duration"; }
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
            var sb = new StringBuilder();
            sb.AppendLine("Title: " + title);
            sb.AppendLine("Translator: No Author");
            sb.AppendLine("Date: " + DateTime.Now.ToString("dd-MM-yyyy").Replace("-",".")); //  25.08.2011
            double milliseconds = 0;
            if (subtitle.Paragraphs.Count > 0)
                milliseconds = subtitle.Paragraphs[subtitle.Paragraphs.Count-1].EndTime.TotalMilliseconds;
            var tc = new TimeCode(TimeSpan.FromMilliseconds(milliseconds));
            sb.AppendLine(string.Format("Duration: {0:00}:{1:00}:{2:00}:{3:00}", tc.Hours, tc.Minutes, tc.Seconds, MillisecondsToFrames(tc.Milliseconds))); // 01:20:49:12
            sb.AppendLine("Program start: 00:00:00:00");
            sb.AppendLine("Title count: " + subtitle.Paragraphs.Count);
            sb.AppendLine();
            sb.AppendLine("#\tIn\tOut\tDuration");
            sb.AppendLine();
            int count = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                count++;
                string text = Utilities.RemoveHtmlTags(p.Text);
                sb.AppendLine(string.Format("{13}\t{0:00}:{1:00}:{2:00}:{3:00}\t{4:00}:{5:00}:{6:00}:{7:00}\t{8:00}:{9:00}:{10:00}:{11:00}\r\n{12:00}" + Environment.NewLine,
                                            p.StartTime.Hours, p.StartTime.Minutes, p.StartTime.Seconds, MillisecondsToFrames(p.StartTime.Milliseconds),
                                            p.EndTime.Hours, p.EndTime.Minutes, p.EndTime.Seconds, MillisecondsToFrames(p.EndTime.Milliseconds),
                                            p.Duration.Hours, p.Duration.Minutes, p.Duration.Seconds, MillisecondsToFrames(p.Duration.Milliseconds),
                                            text, count));
            }
            return sb.ToString().Trim() + Environment.NewLine + Environment.NewLine + Environment.NewLine;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {   //22    00:04:19:12 00:04:21:09 00:00:01:21

            var regex = new Regex(@"^\d+\t\d\d:\d\d:\d\d:\d\d\t\d\d:\d\d:\d\d:\d\d\t\d\d:\d\d:\d\d:\d\d$", RegexOptions.Compiled);
            _errorCount = 0;
            Paragraph lastParagraph = null;
            int count = 0;
            foreach (string line in lines)
            {
                string s = line;
                if (s.Length > 0)
                {
                    bool success = false;
                    var match = regex.Match(s);
                    if (s.Length > 31 && match.Success)
                    {
                        if (lastParagraph != null)
                            subtitle.Paragraphs.Add(lastParagraph);

                        var arr = s.Split('\t');
                        TimeCode start = DecodeTimeCode(arr[1]);
                        TimeCode end = DecodeTimeCode(arr[2]);
                        lastParagraph = new Paragraph(start, end, string.Empty);
                        success = true;
                    }
                    else if (line.Trim().Length > 0 && lastParagraph != null && Utilities.CountTagInText(lastParagraph.Text, Environment.NewLine) < 4)
                    {
                        lastParagraph.Text = (lastParagraph.Text + Environment.NewLine + line).Trim();
                        success = true;
                    }
                    if (!success && count > 9)
                        _errorCount++;
                }
                count++;
            }
            if (lastParagraph != null)
                subtitle.Paragraphs.Add(lastParagraph);
            subtitle.Renumber(1);
        }

        private TimeCode DecodeTimeCode(string s)
        {
            var parts = s.Split(':');

            //00:00:07:12
            string hour = parts[0];
            string minutes = parts[1];
            string seconds = parts[2];
            string frames = parts[3];

            int milliseconds = (int)((1000.0 / Configuration.Settings.General.CurrentFrameRate) * int.Parse(frames));
            if (milliseconds > 999)
                milliseconds = 999;

            TimeCode tc = new TimeCode(int.Parse(hour), int.Parse(minutes), int.Parse(seconds), milliseconds);
            return tc;
        }
    }
}
