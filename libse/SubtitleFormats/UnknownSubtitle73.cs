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

        public override string Extension
        {
            get { return ".txt"; }
        }

        public override string Name
        {
            get { return "Unknown 73"; }
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
            return string.Format("{0:00}:{1:00}", p.Duration.Seconds, MillisecondsToFramesMaxFrameRate(p.Duration.Milliseconds));
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            //00:03:15:22 (last is frame)
            return string.Format("{0:00}:{1:00}:{2:00}:{3:00}", time.Hours, time.Minutes, time.Seconds, MillisecondsToFramesMaxFrameRate(time.Milliseconds));
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
                                p = new Paragraph(DecodeTimeCodeFrames(startParts), DecodeTimeCodeFrames(endParts), string.Empty);
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
