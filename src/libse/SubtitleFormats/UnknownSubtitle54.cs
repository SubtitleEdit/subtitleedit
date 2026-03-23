using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle54 : SubtitleFormat
    {
        //10:00:31:01
        //10:00:33:02
        //This is the king's royal court.

        //10:00:33:19
        //10:00:35:00
        //This is the place,
        private static readonly Regex RegexTimeCodes1 = new Regex(@"^\d\d:\d\d:\d\d:\d\d$", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Unknown 54";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.AppendLine(EncodeTimeCode(p.StartTime));
                sb.AppendLine(EncodeTimeCode(p.EndTime));
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
            foreach (string line in lines)
            {
                string s = line.Trim();
                var match = RegexTimeCodes1.Match(s);
                if (match.Success && s.Length == 11)
                {
                    string[] parts = s.Split(':');
                    if (parts.Length == 4)
                    {
                        try
                        {
                            if (expectStartTime)
                            {
                                p.StartTime = DecodeTimeCodeFramesFourParts(parts);
                                expectStartTime = false;
                            }
                            else
                            {
                                if (p.StartTime.TotalMilliseconds < 0.01)
                                {
                                    _errorCount++;
                                }

                                if (!string.IsNullOrEmpty(p.Text))
                                {
                                    _errorCount++;
                                }

                                p.EndTime = DecodeTimeCodeFramesFourParts(parts);
                            }
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
                    if (Math.Abs(p.StartTime.TotalMilliseconds) < 0.001 && Math.Abs(p.EndTime.TotalMilliseconds) < 0.001)
                    {
                        _errorCount++;
                    }
                    else
                    {
                        subtitle.Paragraphs.Add(p);
                    }

                    p = new Paragraph();
                }
                else
                {
                    expectStartTime = true;
                    p.Text = (p.Text + Environment.NewLine + line).Trim();
                    if (p.Text.Length > 500)
                    {
                        _errorCount += 10;
                        return;
                    }
                }
            }
            if (p.EndTime.TotalMilliseconds > 0)
            {
                subtitle.Paragraphs.Add(p);
            }

            bool allNullEndTime = true;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                if (Math.Abs(subtitle.Paragraphs[i].EndTime.TotalMilliseconds) > 0.001)
                {
                    allNullEndTime = false;
                }
            }
            if (allNullEndTime)
            {
                subtitle.Paragraphs.Clear();
            }

            subtitle.RemoveEmptyLines();
            subtitle.Renumber();
        }

    }
}
