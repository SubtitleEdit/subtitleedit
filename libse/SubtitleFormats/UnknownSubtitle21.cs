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

        public override string Extension => ".rtf";

        public override string Name => "Unknown 21";

        private static string MakeTimeCode(TimeCode tc)
        {
            return $"{tc.Hours:00}:{tc.Minutes:00}:{tc.Seconds:00};{MillisecondsToFramesMaxFrameRate(tc.Milliseconds):00}";
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
                sb.AppendLine($"{count.ToString().PadLeft(5, ' ')}\t{MakeTimeCode(p.StartTime)}\t{text}\t");
                sb.AppendLine("\t\t\t\t");
                Paragraph next = subtitle.GetParagraphOrDefault(i + 1);
                if (next == null || Math.Abs(p.EndTime.TotalMilliseconds - next.StartTime.TotalMilliseconds) > 50)
                {
                    count++;
                    sb.AppendLine($"{count.ToString().PadLeft(5, ' ')}\t{MakeTimeCode(p.EndTime)}");
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
            {
                sb.AppendLine(line);
            }

            string rtf = sb.ToString().Trim();
            if (!rtf.StartsWith("{\\rtf", StringComparison.Ordinal))
            {
                return;
            }

            lines = rtf.FromRtf().SplitToLines();
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
                        {
                            subtitle.Paragraphs.Add(p);
                        }

                        string[] arr = s.Split('\t');
                        if (arr.Length > 2)
                        {
                            p = new Paragraph(DecodeTimeCodeFrames(arr[1], splitChar), new TimeCode(), arr[2].Trim());
                        }
                        else
                        {
                            p = new Paragraph(DecodeTimeCodeFrames(arr[1], splitChar), new TimeCode(), string.Empty);
                        }
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
                    {
                        p.Text = p.Text + Environment.NewLine + s.Trim();
                    }
                }
                else if (!string.IsNullOrWhiteSpace(s))
                {
                    _errorCount++;
                }
            }
            if (p != null)
            {
                subtitle.Paragraphs.Add(p);
            }

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
