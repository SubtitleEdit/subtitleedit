using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    /// <summary>
    /// #00:00:06-8#
    /// http://www.audiotranskription.de
    /// </summary>
    public class F4Text : SubtitleFormat
    {
        private static Regex regexTimeCodes = new Regex(@"^\d\d:\d\d:\d\d-\d$", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".txt"; }
        }

        public override string Name
        {
            get { return "F4 Text"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (fileName != null && !fileName.EndsWith(Extension, StringComparison.OrdinalIgnoreCase))
                return false;

            var subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public static string ToF4Text(Subtitle subtitle)
        {
            var sb = new StringBuilder();
            double lastEndTimeMilliseconds = -1;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                // if (p.StartTime.TotalMilliseconds == lastEndTimeMilliseconds)
                sb.AppendFormat("{0}{1}", Utilities.RemoveHtmlTags(p.Text), EncodeTimeCode(p.EndTime));
                //else
                //    sb.Append(string.Format("{0}{1}{2}", EncodeTimeCode(p.StartTime), Utilities.RemoveHtmlTags(p.Text), EncodeTimeCode(p.EndTime)));
                lastEndTimeMilliseconds = p.EndTime.TotalMilliseconds;
            }
            return sb.ToString().Trim();
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            return ToF4Text(subtitle);
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            return string.Format(" #{0:00}:{1:00}:{2:00}-{3:0}# ", time.Hours, time.Minutes, time.Seconds, Math.Round(time.Milliseconds / 100.0, 0));
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            var sb = new StringBuilder();
            foreach (string line in lines)
                sb.AppendLine(line);
            string text = sb.ToString();
            if (text.Contains("{\\rtf") || text.Contains("<transcript>"))
                return;
            LoadF4TextSubtitle(subtitle, text);
        }

        protected void LoadF4TextSubtitle(Subtitle subtitle, string text)
        {
            Paragraph p = null;
            subtitle.Paragraphs.Clear();
            var arr = text.Trim().Split(new[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
            var currentText = new StringBuilder();
            foreach (string line in arr)
            {
                if (regexTimeCodes.IsMatch(line))
                {
                    if (p == null)
                    {
                        p = new Paragraph();
                        if (currentText.Length > 0)
                        {
                            p.Text = currentText.ToString().Trim().Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                            p.Text = p.Text.Trim('\n');
                            p.Text = p.Text.Trim('\r');
                            subtitle.Paragraphs.Add(p);
                            p = new Paragraph();
                        }
                    }
                    if (p.StartTime.TotalMilliseconds == 0 || currentText.Length == 0)
                    {
                        p.StartTime = DecodeTimeCode(line);
                    }
                    else
                    {
                        p.EndTime = DecodeTimeCode(line);
                        p.Text = currentText.ToString().Trim().Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                        p.Text = p.Text.Trim('\n');
                        p.Text = p.Text.Trim('\r');
                        p.Text = p.Text.Trim();
                        subtitle.Paragraphs.Add(p);
                        p = null;
                        currentText = new StringBuilder();
                    }
                }
                else
                {
                    if (p == null && subtitle.Paragraphs.Count > 0)
                    {
                        p = new Paragraph();
                        p.StartTime.TotalMilliseconds = subtitle.Paragraphs[subtitle.Paragraphs.Count - 1].EndTime.TotalMilliseconds;
                    }
                    currentText.AppendLine(line.Trim());
                }
            }
            if (currentText.Length > 0 && subtitle.Paragraphs.Count > 0 && currentText.Length < 1000)
            {
                if (p == null)
                    p = new Paragraph();

                p.Text = currentText.ToString().Trim().Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                p.Text = p.Text.Trim('\n');
                p.Text = p.Text.Trim('\r');
                p.Text = p.Text.Trim();
                p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + 3000;
                subtitle.Paragraphs.Add(p);
            }
            subtitle.Renumber(1);
        }

        private static TimeCode DecodeTimeCode(string line)
        {
            // hh:mm:ss-f
            var parts = line.Split(new[] { ':', '-' }, StringSplitOptions.RemoveEmptyEntries);
            return TimeCode.FromTimestampTokens(parts[0], parts[1], parts[2], parts[3]);
        }
    }
}
