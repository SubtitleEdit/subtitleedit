using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle33 : SubtitleFormat
    {

        private static readonly Regex RegexTimeCodes = new Regex(@"^\d\d\:\d\d\:\d\d\s+\d+    ", RegexOptions.Compiled);
        private static readonly Regex RegexNumberAndText = new Regex(@"^\d+    [^ ]+", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Unknown 33";

        public override string ToText(Subtitle subtitle, string title)
        {
            //08:59:51  3    ON THE PANEL THIS WEEK WE HAVE EMILY LAWLER AND ZACH
            //08:59:54  4    GORCHOW, ALONG WITH RON DZWONKOWSKI.
            //          5    HERE IS THE RUNDOWN.
            //          6    A POSSIBLE REDO OF THE EM LAW IF VOTERS REJECT IT.
            //09:00:03  7    AND MIKE DUGAN AND LATER GENE CLEM IS DISCUSSING THIS

            const string paragraphWriteFormat = "{0} {1}    {2}";

            var sb = new StringBuilder();
            int count = 1;
            int count2 = 1;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                var lines = HtmlUtil.RemoveHtmlTags(p.Text).SplitToLines();
                sb.AppendLine(string.Format(paragraphWriteFormat, EncodeTimeCode(p.StartTime), count.ToString(CultureInfo.InvariantCulture).PadLeft(2, ' '), lines[0]));
                for (int i = 1; i < lines.Count; i++)
                {
                    count++;
                    if (count > 26)
                    {
                        sb.Append(string.Empty.PadLeft(38, ' ') + count2);
                        sb.AppendLine();
                        sb.AppendLine();
                        count = 1;
                        count2++;
                    }

                    sb.AppendLine(string.Format(paragraphWriteFormat, string.Empty, count.ToString(CultureInfo.InvariantCulture).PadLeft(10, ' '), lines[i]));
                }

                count++;
                if (count > 26)
                {
                    sb.Append(string.Empty.PadLeft(38, ' ') + count2);
                    sb.AppendLine();
                    sb.AppendLine();
                    count = 1;
                    count2++;
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
                if (s.Length > 4 && s[2] == ':' && RegexTimeCodes.Match(s).Success)
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
                            string text = s.Remove(0, 12).Trim();
                            p.Text = text;
                        }
                    }
                    catch
                    {
                        _errorCount++;
                    }
                }
                else if (p != null && RegexNumberAndText.Match(s).Success)
                {
                    if (p.Text.Length > 1000)
                    {
                        _errorCount += 100;
                        return;
                    }
                    string text = s.Remove(0, 2).Trim();
                    p.Text = (p.Text + Environment.NewLine + text).Trim();
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
                index++;
            }

            subtitle.RemoveEmptyLines();
            subtitle.Renumber();
        }

    }
}
