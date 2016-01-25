using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle21 : SubtitleFormat
    {
        private static readonly Regex RegexTimeCode1 = new Regex(@"^\s*\d+\t\d\d:\d\d:\d\d;\d\d", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".rtf"; }
        }

        public override string Name
        {
            get { return "Unknown 21"; }
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
                string text = HtmlUtil.RemoveHtmlTags(p.Text);
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

            return sb.ToString().ToRtf();
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

            lines = rtf.FromRtf().SplitToLines().ToList();
            _errorCount = 0;
            Paragraph p = null;
            char[] splitChar = { ':', ';', ',' };
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
                            p = new Paragraph(DecodeTimeCode(arr[1], splitChar), new TimeCode(0, 0, 0, 0), arr[2].Trim());
                        else
                            p = new Paragraph(DecodeTimeCode(arr[1], splitChar), new TimeCode(0, 0, 0, 0), string.Empty);
                    }
                    catch
                    {
                        _errorCount++;
                        p = null;
                    }
                }
                else if (s.StartsWith("\t\t", StringComparison.Ordinal))
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
                p.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines;
            }
            if (subtitle.Paragraphs.Count > 0)
            {
                p = subtitle.Paragraphs[subtitle.Paragraphs.Count - 1];
                p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + Utilities.GetOptimalDisplayMilliseconds(p.Text);
            }
            subtitle.RemoveEmptyLines();
            subtitle.Renumber();
        }

    }
}
