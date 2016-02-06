using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle57 : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d\d:\d\d:\d\d\.\d\d \d\d:\d\d:\d\d\.\d\d .+", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".txt"; }
        }

        public override string Name
        {
            get { return "Unknown 57"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            var subtitle = new Subtitle();

            var sb = new StringBuilder();
            foreach (string line in lines)
                sb.AppendLine(line);

            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            int index = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                //00:00:54.08 00:00:58.06 - Saucers... - ... a dry lake bed.  (newline is //)
                sb.AppendLine(string.Format("{0} {1} {2}", EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime), HtmlUtil.RemoveHtmlTags(p.Text).Replace(Environment.NewLine, "//")));
                index++;
            }
            return sb.ToString();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            //00:03:15.22 (last is frame)
            return string.Format("{0:00}:{1:00}:{2:00}.{3:00}", time.Hours, time.Minutes, time.Seconds, MillisecondsToFramesMaxFrameRate(time.Milliseconds));
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            //00:03:15.22 00:03:23.10 This is line one.//This is line two.
            _errorCount = 0;
            Paragraph p = null;
            subtitle.Paragraphs.Clear();
            char[] splitChars = { ':', '.' };
            foreach (string line in lines)
            {
                var match = RegexTimeCodes.Match(line);
                if (match.Success)
                {
                    string temp = line.Substring(0, match.Length);
                    if (line.Length >= 23)
                    {
                        string text = line.Remove(0, 23).Trim();
                        if (!text.Contains(Environment.NewLine))
                            text = text.Replace("//", Environment.NewLine);
                        p = new Paragraph(DecodeTimeCodeFrames(temp.Substring(0, 11), splitChars), DecodeTimeCodeFrames(temp.Substring(12, 11), splitChars), text);
                        subtitle.Paragraphs.Add(p);
                    }
                }
                else if (string.IsNullOrWhiteSpace(line))
                {
                }
                else if (p != null)
                {
                    if (p.Text.Length < 200)
                        p.Text = (p.Text + Environment.NewLine + line).Trim();
                }
            }

            subtitle.Renumber();
        }
    }
}
