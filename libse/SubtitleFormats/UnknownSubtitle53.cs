using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle53 : SubtitleFormat
    {

        private static readonly Regex RegexTimeCodes = new Regex(@"^\d\d\:\d\d\:\d\d\:\d\d [^ ]+", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".txt"; }
        }

        public override string Name
        {
            get { return "Unknown 53"; }
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
            //10:56:54:12 FEATURING BRIAN LORENTE AND THE
            //10:56:59:18 USUAL SUSPECTS.
            //10:57:15:18 \M
            //10:57:20:07 >> HOW WE DOING TONIGHT,
            //10:57:27:17 MICHIGAN?

            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string text = HtmlUtil.RemoveHtmlTags(p.Text).Replace("♪", "\\M");
                sb.AppendLine(EncodeTimeCode(p.StartTime) + " " + text);
            }
            return sb.ToString().Trim();
        }

        private static string EncodeTimeCode(TimeCode timeCode)
        {
            return string.Format("{0:00}:{1:00}:{2:00}:{3:00}", timeCode.Hours, timeCode.Minutes, timeCode.Seconds, MillisecondsToFramesMaxFrameRate(timeCode.Milliseconds));
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            Paragraph p = null;
            foreach (string line in lines)
            {
                string s = line.Trim();
                if (RegexTimeCodes.Match(s).Success)
                {
                    if (p != null && !string.IsNullOrEmpty(p.Text))
                        subtitle.Paragraphs.Add(p);
                    p = new Paragraph();

                    try
                    {
                        string[] arr = s.Substring(0, 11).Split(':');
                        if (arr.Length == 4)
                        {
                            int hours = int.Parse(arr[0]);
                            int minutes = int.Parse(arr[1]);
                            int seconds = int.Parse(arr[2]);
                            int frames = int.Parse(arr[3]);
                            p.StartTime = new TimeCode(hours, minutes, seconds, FramesToMillisecondsMax999(frames));
                            string text = s.Remove(0, 11).Trim();
                            p.Text = text;
                            if (text.Length > 1 && Utilities.IsInteger(text.Substring(0, 2)))
                                _errorCount++;
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
            if (p != null && !string.IsNullOrEmpty(p.Text))
                subtitle.Paragraphs.Add(p);

            int index = 1;
            foreach (Paragraph paragraph in subtitle.Paragraphs)
            {
                paragraph.Text = paragraph.Text.Replace("\\M", "♪");

                Paragraph next = subtitle.GetParagraphOrDefault(index);
                if (next != null)
                {
                    paragraph.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - 1;
                }
                else
                {
                    paragraph.EndTime.TotalMilliseconds = paragraph.StartTime.TotalMilliseconds + Utilities.GetOptimalDisplayMilliseconds(paragraph.Text);
                }
                index++;
            }
            subtitle.RemoveEmptyLines();
        }

    }
}
