using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle34 : SubtitleFormat
    {

        private static readonly Regex RegexTimeCodes = new Regex(@"^\d\d\:\d\d\:\d\d\t[^ ]+", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Unknown 34";

        public override string ToText(Subtitle subtitle, string title)
        {
            //08:55:05  >>> WELCOME BACK.
            //08:59:49  """OFF THE RECORD"" STARTS RIGHT NOW."
            //08:59:51  ON THE PANEL THIS WEEK WE HAVE EMILY LAWLER AND ZACH
            //08:59:54  "GORCHOW, ALONG WITH RON DZWONKOWSKI."
            //    HERE IS THE RUNDOWN.
            //    A POSSIBLE REDO OF THE EM LAW IF VOTERS REJECT IT.
            //09:00:03  AND MIKE DUGAN AND LATER GENE CLEM IS DISCUSSING THIS

            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                var lines = HtmlUtil.RemoveHtmlTags(p.Text).SplitToLines();
                sb.AppendLine(EncodeTimeCode(p.StartTime) + "\t" + lines[0]);
                for (int i = 1; i < lines.Count; i++)
                {
                    sb.AppendLine("\t" + lines[i]);
                }
            }

            return sb.ToString().Trim();
        }

        private static string EncodeTimeCode(TimeCode timeCode)
        {
            int seconds = (int)Math.Round(timeCode.Seconds + timeCode.Milliseconds / 1000.0);
            return $"{timeCode.Hours:00}:{timeCode.Minutes:00}:{seconds:00}";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            Paragraph p = null;
            foreach (string line in lines)
            {
                string s = line.Trim();
                if (RegexTimeCodes.Match(s).Success && !UnknownSubtitle59.RegexTimeCodes.IsMatch(s))
                {
                    if (p != null && !string.IsNullOrEmpty(p.Text))
                    {
                        subtitle.Paragraphs.Add(p);
                    }

                    p = new Paragraph();

                    try
                    {
                        string[] arr = s.Substring(0, 8).Split(':');
                        if (arr.Length == 3)
                        {
                            int hours = int.Parse(arr[0]);
                            int minutes = int.Parse(arr[1]);
                            int seconds = int.Parse(arr[2]);
                            p.StartTime = new TimeCode(hours, minutes, seconds, 0);
                            string text = s.Remove(0, 8).Trim();
                            p.Text = text;
                            if (text.Length > 1 && Utilities.IsInteger(text.Substring(0, 2)))
                            {
                                _errorCount++;
                            }
                        }
                    }
                    catch
                    {
                        _errorCount++;
                    }
                }
                else if (line.StartsWith("\t") && p != null)
                {
                    if (p.Text.Length > 1000)
                    {
                        _errorCount += 100;
                        return;
                    }
                    p.Text = (p.Text + Environment.NewLine + s).Trim();
                }
                else if (s.Length > 0 && !Utilities.IsInteger(s))
                {
                    _errorCount++;
                }
            }
            if (p != null && !string.IsNullOrEmpty(p.Text))
            {
                subtitle.Paragraphs.Add(p);
            }

            int index = 1;
            foreach (Paragraph paragraph in subtitle.Paragraphs)
            {
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
