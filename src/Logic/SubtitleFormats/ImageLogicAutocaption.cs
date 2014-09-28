using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class ImageLogicAutocaption : SubtitleFormat
    {
        private static readonly Regex RegexTimeCode1 = new Regex(@"^\s*\d+\t\d\d:\d\d:\d\d;\d\d", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".rtf"; }
        }

        public override string Name
        {
            get { return "Image Logic Autocaption"; }
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

        private static string MakeTimeCode(TimeCode tc)
        {
            return string.Format("{0:00}:{1:00}:{2:00};{3:00}", tc.Hours, tc.Minutes, tc.Seconds, MillisecondsToFramesMaxFrameRate(tc.Milliseconds));
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            sb.AppendLine("#\tAppearance\tCaption\t");
            sb.AppendLine();
            int count = 1;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                string text = Utilities.RemoveHtmlTags(p.Text);
                sb.AppendLine(string.Format("{0}\t{1}\t{2}\t", count.ToString().PadLeft(5, ' '), MakeTimeCode(p.StartTime), text));
                sb.AppendLine("\t\t\t\t");
                Paragraph next = subtitle.GetParagraphOrDefault(i + 1);
                if (next == null || Math.Abs(p.EndTime.TotalMilliseconds - next.StartTime.TotalMilliseconds) > 50)
                {
                    count++;
                    sb.AppendLine(string.Format("{0}\t{1}", count.ToString().PadLeft(5, ' '), MakeTimeCode(p.EndTime)));
                }
                count++;
            }

            var rtBox = new System.Windows.Forms.RichTextBox();
            rtBox.Text = sb.ToString();
            string rtf = rtBox.Rtf;
            rtBox.Dispose();
            return rtf;
        }

        private static TimeCode DecodeTimeCode(string timeCode)
        {
            string[] arr = timeCode.Split(new[] { ':', ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
            return new TimeCode(int.Parse(arr[0]), int.Parse(arr[1]), int.Parse(arr[2]), FramesToMillisecondsMax999(int.Parse(arr[3])));
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            var sb = new StringBuilder();
            foreach (string line in lines)
                sb.AppendLine(line);

            string rtf = sb.ToString().Trim();
            if (!rtf.StartsWith("{\\rtf"))
                return;

            System.Windows.Forms.RichTextBox rtBox = null;
            try
            {
                rtBox = new System.Windows.Forms.RichTextBox
                {
                    Rtf = rtf
                };

                lines = new List<string>(rtBox.Text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None));
            }
            catch (ArgumentException)
            {
                // Invalid format
                return;
            }
            finally
            {
                if (rtBox != null)
                {
                    rtBox.Dispose();
                }
            }

            _errorCount = 0;
            Paragraph p = null;
            foreach (string line in lines)
            {
                string s = line.TrimEnd();
                if (RegexTimeCode1.IsMatch(s))
                {
                    try
                    {
                        if (p != null)
                            subtitle.Paragraphs.Add(p);
                        string[] arr = s.Split('\t');
                        if (arr.Length > 2)
                            p = new Paragraph(DecodeTimeCode(arr[1]), new TimeCode(0, 0, 0, 0), arr[2].Trim());
                        else
                            p = new Paragraph(DecodeTimeCode(arr[1]), new TimeCode(0, 0, 0, 0), string.Empty);
                    }
                    catch
                    {
                        _errorCount++;
                        p = null;
                    }
                }
                else if (s.StartsWith("\t\t"))
                {
                    if (p != null)
                        p.Text = p.Text + Environment.NewLine + s.Trim();
                }
                else if (!string.IsNullOrWhiteSpace(s))
                {
                    _errorCount++;
                }
            }
            if (p != null)
                subtitle.Paragraphs.Add(p);

            for (int j = 0; j < subtitle.Paragraphs.Count - 1; j++)
            {
                p = subtitle.Paragraphs[j];
                Paragraph next = subtitle.Paragraphs[j + 1];
                p.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - Configuration.Settings.General.MininumMillisecondsBetweenLines;
            }
            if (subtitle.Paragraphs.Count > 0)
            {
                p = subtitle.Paragraphs[subtitle.Paragraphs.Count - 1];
                p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + Utilities.GetOptimalDisplayMilliseconds(p.Text);
            }
            subtitle.RemoveEmptyLines();
            subtitle.Renumber(1);
        }

    }
}
