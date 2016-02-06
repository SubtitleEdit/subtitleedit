using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle46 : SubtitleFormat
    {
        //7:00:01:27AM
        private static readonly Regex regexTimeCodesAM = new Regex(@"^\d\:\d\d\:\d\d\:\d\dAM", RegexOptions.Compiled);
        private static readonly Regex regexTimeCodesPM = new Regex(@"^\d\:\d\d\:\d\d\:\d\dPM", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".pst"; }
        }

        public override string Name
        {
            get { return "Unknown 46"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            Subtitle subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            //OFF THE RECORD STARTS RIGHT NOW.   7:00:01:27AM
            //HERE IS THE RUNDOWN.               7:00:05:03AM
            var sb = new StringBuilder();
            const string format = "{0}{1}";
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.AppendLine(string.Format(format, p.Text.Replace(Environment.NewLine, " ").PadRight(35), EncodeTimeCode(p.StartTime)));
            }
            return sb.ToString().Trim();
        }

        private static string EncodeTimeCode(TimeCode timeCode)
        {
            return string.Format("{0}:{1:00}:{2:00}:{3:00}AM", timeCode.Hours, timeCode.Minutes, timeCode.Seconds, MillisecondsToFramesMaxFrameRate(timeCode.Milliseconds));
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            Paragraph p = null;
            foreach (string line in lines)
            {
                string s = line.Trim();
                string[] arr = line.Split();
                var timeCode = arr[arr.Length - 1];
                if (regexTimeCodesAM.Match(timeCode).Success || regexTimeCodesPM.Match(timeCode).Success)
                {
                    try
                    {
                        arr = timeCode.Substring(0, 10).Split(':');
                        if (arr.Length == 4)
                        {
                            p = new Paragraph();
                            p.StartTime = DecodeTimeCodeFrames(arr);
                            p.Text = s.Substring(0, s.IndexOf(timeCode, StringComparison.Ordinal)).Trim();
                            subtitle.Paragraphs.Add(p);
                        }
                    }
                    catch
                    {
                        _errorCount++;
                    }
                }
                else if (s.Length > 0)
                {
                    _errorCount++;
                }
            }

            int index = 1;
            foreach (Paragraph paragraph in subtitle.Paragraphs)
            {
                Paragraph next = subtitle.GetParagraphOrDefault(index);
                if (next != null)
                {
                    paragraph.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - 1;
                }
                if (paragraph.Duration.TotalMilliseconds > Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds)
                {
                    paragraph.EndTime.TotalMilliseconds = paragraph.StartTime.TotalMilliseconds + Utilities.GetOptimalDisplayMilliseconds(p.Text);
                }
                index++;
            }
            subtitle.RemoveEmptyLines();
            subtitle.Renumber();
        }

    }
}
