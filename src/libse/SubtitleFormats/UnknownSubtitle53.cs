using Nikse.SubtitleEdit.Core.Common;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle53 : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d\d\:\d\d\:\d\d\:\d\d .+", RegexOptions.Compiled);
        private static readonly Regex RegexTimeCodesEnd = new Regex(@"^\d\d\:\d\d\:\d\d\:\d\d$", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Unknown 53";

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
            return $"{timeCode.Hours:00}:{timeCode.Minutes:00}:{timeCode.Seconds:00}:{MillisecondsToFramesMaxFrameRate(timeCode.Milliseconds):00}";
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
                    if (!string.IsNullOrEmpty(p?.Text))
                    {
                        subtitle.Paragraphs.Add(p);
                    }

                    p = new Paragraph();

                    try
                    {
                        var arr = s.Substring(0, 11).Split(':');
                        p.StartTime = DecodeTimeCodeFramesFourParts(arr);
                        string text = s.Substring(11).Trim();
                        p.Text = text;
                        if (text.Length > 1 && Utilities.IsInteger(text.Substring(0, 2)))
                        {
                            _errorCount++;
                        }

                        if (text.Contains("<<Graphic>>"))
                        {
                            _errorCount++;
                        }
                    }
                    catch
                    {
                        _errorCount++;
                    }
                }
                else if (s.Length == 11 && RegexTimeCodesEnd.IsMatch(line) && p != null)
                {
                    p.EndTime = DecodeTimeCodeFramesFourParts(s.Split(':'));
                }
                else if (s.Length > 0)
                {
                    _errorCount++;
                }
            }
            if (!string.IsNullOrEmpty(p?.Text))
            {
                subtitle.Paragraphs.Add(p);
            }

            int index = 1;
            foreach (Paragraph paragraph in subtitle.Paragraphs)
            {
                paragraph.Text = paragraph.Text.Replace("\\M", "♪");

                Paragraph next = subtitle.GetParagraphOrDefault(index);
                if (next != null && paragraph.EndTime.TotalMilliseconds < 0.01)
                {
                    paragraph.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - 1;
                }
                else if (next == null)
                {
                    paragraph.EndTime.TotalMilliseconds = paragraph.StartTime.TotalMilliseconds + Utilities.GetOptimalDisplayMilliseconds(paragraph.Text);
                }
                index++;
            }
            subtitle.RemoveEmptyLines();
            subtitle.Renumber();
        }

    }
}
