using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle56 : SubtitleFormat
    {
        //0001  01:00:37:22 01:00:39:11
        private static readonly Regex RegexTimeCodes1 = new Regex(@"^\d\d\d\d\t\d\d:\d\d:\d\d:\d\d\t\d\d:\d\d:\d\d:\d\d$", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Unknown 56";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            const string format = "{0:0000}\t{1}\t{2}";
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.AppendLine(string.Format(format, 1, EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime)));
                sb.AppendLine(HtmlUtil.RemoveHtmlTags(p.Text));
                sb.AppendLine();
            }
            return sb.ToString();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            return $"{time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}:{MillisecondsToFramesMaxFrameRate(time.Milliseconds):00}";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            bool expectStartTime = true;
            var p = new Paragraph();
            subtitle.Paragraphs.Clear();
            char[] splitChars = { '.', ':' };
            foreach (string line in lines)
            {
                string s = line.Trim();
                var match = RegexTimeCodes1.Match(s);
                if (match.Success)
                {
                    string[] parts = s.Split('\t');
                    if (parts.Length == 3)
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(p.Text))
                            {
                                subtitle.Paragraphs.Add(p);
                                p = new Paragraph();
                            }
                            p.StartTime = DecodeTimeCodeFrames(parts[1], splitChars);
                            p.EndTime = DecodeTimeCodeFrames(parts[2], splitChars);
                            expectStartTime = false;
                        }
                        catch (Exception exception)
                        {
                            _errorCount++;
                            System.Diagnostics.Debug.WriteLine(exception.Message);
                        }
                    }
                }
                else if (string.IsNullOrWhiteSpace(line))
                {
                    if (Math.Abs(p.StartTime.TotalMilliseconds) < 0.01 && Math.Abs(p.EndTime.TotalMilliseconds) < 0.01)
                    {
                        _errorCount++;
                    }
                    else
                    {
                        subtitle.Paragraphs.Add(p);
                    }

                    p = new Paragraph();
                }
                else if (!expectStartTime)
                {
                    p.Text = (p.Text + Environment.NewLine + line).Trim();
                    if (p.Text.Length > 500)
                    {
                        _errorCount += 10;
                        return;
                    }
                    while (p.Text.Contains(Environment.NewLine + " "))
                    {
                        p.Text = p.Text.Replace(Environment.NewLine + " ", Environment.NewLine);
                    }
                }
            }
            if (p.EndTime.TotalMilliseconds > 0)
            {
                subtitle.Paragraphs.Add(p);
            }

            foreach (Paragraph temp in subtitle.Paragraphs)
            {
                temp.Text = temp.Text.Replace("<", "@ITALIC_START").Replace(">", "</i>").Replace("@ITALIC_START", "<i>");
            }

            subtitle.RemoveEmptyLines();
            subtitle.Renumber();
        }

    }
}
