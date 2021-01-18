using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle73 : SubtitleFormat
    {
        //59:00:22:09:14 00:22:12:04 02:15
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d+:\d\d:\d\d:\d\d:\d\d \d\d:\d\d:\d\d:\d\d \d\d:\d\d$", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Unknown 73";

        public override string ToText(Subtitle subtitle, string title)
        {
            //59:00:22:09:14 00:22:12:04 02:15
            //
            //LÕaria fresca arriva
            //
            //dai monti di Petr—polis

            var sb = new StringBuilder();
            int index = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                index++;
                string text = HtmlUtil.RemoveHtmlTags(p.Text);
                text = text.Replace(Environment.NewLine, "\n\n");
                sb.AppendFormat("{0}:{1} {2} {3}\n{4}\n\n", index, EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime), EncodeDuration(p), text);
            }
            return sb.ToString();
        }

        private static string EncodeDuration(Paragraph p)
        {
            return $"{p.Duration.Seconds:00}:{MillisecondsToFramesMaxFrameRate(p.Duration.Milliseconds):00}";
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            //00:03:15:22 (last is frame)
            return $"{time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}:{MillisecondsToFramesMaxFrameRate(time.Milliseconds):00}";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            Paragraph p = null;
            subtitle.Paragraphs.Clear();
            _errorCount = 0;
            foreach (string line in lines)
            {
                string s = line;
                if (RegexTimeCodes.IsMatch(s))
                {
                    s = s.Remove(0, s.IndexOf(':') + 1).Trim();
                    var temp = s.Split(' ');
                    if (temp.Length > 1)
                    {
                        string start = temp[0];
                        string end = temp[1];

                        string[] startParts = start.Split(':');
                        string[] endParts = end.Split(':');
                        if (startParts.Length == 4 && endParts.Length == 4)
                        {
                            try
                            {
                                p = new Paragraph(DecodeTimeCodeFramesFourParts(startParts), DecodeTimeCodeFramesFourParts(endParts), string.Empty);
                                subtitle.Paragraphs.Add(p);
                            }
                            catch (Exception exception)
                            {
                                _errorCount++;
                                System.Diagnostics.Debug.WriteLine(exception.Message);
                            }
                        }
                    }
                }
                else if (string.IsNullOrWhiteSpace(line))
                {
                    // skip empty lines
                }
                else if (p == null)
                {
                    _errorCount++;
                }
                else
                {
                    p.Text = (p.Text + Environment.NewLine + line).Trim();
                    if (p.Text.Length > 1000)
                    {
                        _errorCount += 10;
                        return;
                    }
                }
            }

            subtitle.Renumber();
        }

    }
}
