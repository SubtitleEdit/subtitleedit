using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// #00:00:06-8#
    /// http://www.audiotranskription.de
    /// </summary>
    public class F4Text : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d\d:\d\d:\d\d-\d$", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "F4 Text";

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (fileName?.EndsWith(Extension, StringComparison.OrdinalIgnoreCase) == false)
            {
                return false;
            }
            return base.IsMine(lines, fileName);
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            //double lastEndTimeMilliseconds = -1;
            const string writeFormat = "{0}{1}";
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                // if (p.StartTime.TotalMilliseconds == lastEndTimeMilliseconds)
                sb.AppendFormat(writeFormat, HtmlUtil.RemoveHtmlTags(p.Text, true), EncodeTime(p.EndTime));
                //else
                //    sb.Append(string.Format("{0}{1}{2}", EncodeTimeCode(p.StartTime), HtmlUtil.RemoveHtmlTags(p.Text), EncodeTimeCode(p.EndTime)));
                //lastEndTimeMilliseconds = p.EndTime.TotalMilliseconds;
            }
            return sb.ToString().Trim();
        }

        private static string EncodeTime(TimeCode time) => $" #{time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}-{Math.Round(time.Milliseconds / 100.0, 0):0}# ";

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
            var lines = text.Trim().Split(new[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder(Configuration.Settings.General.SubtitleLineMaximumLength * 2);
            char[] timeSplitChars = { ':', '-' };
            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();
                if (trimmedLine.Length == 10 && RegexTimeCodes.IsMatch(trimmedLine))
                {
                    if (p == null)
                    {
                        p = new Paragraph();
                        if (sb.Length > 0)
                        {
                            p.Text = sb.ToString().Trim();
                            p.Text = p.Text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                            subtitle.Paragraphs.Add(p);
                            p = new Paragraph();
                        }
                    }
                    if (Math.Abs(p.StartTime.TotalMilliseconds) < 0.01 || sb.Length == 0)
                    {
                        p.StartTime = DecodeTimeCode(trimmedLine.Split(timeSplitChars, StringSplitOptions.RemoveEmptyEntries));
                    }
                    else
                    {
                        p.EndTime = DecodeTimeCode(trimmedLine.Split(timeSplitChars, StringSplitOptions.RemoveEmptyEntries));
                        p.Text = sb.ToString().Trim();
                        p.Text = p.Text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                        subtitle.Paragraphs.Add(p);
                        p = null;
                        sb.Clear();
                    }
                }
                else
                {
                    if (p == null && subtitle.Paragraphs.Count > 0)
                    {
                        p = new Paragraph();
                        p.StartTime.TotalMilliseconds = subtitle.Paragraphs[subtitle.Paragraphs.Count - 1].EndTime.TotalMilliseconds;
                    }
                    sb.AppendLine(trimmedLine);
                }
            }
            if (sb.Length > 0 && subtitle.Paragraphs.Count > 0 && sb.Length < 1000)
            {
                if (p == null)
                    p = new Paragraph();

                p.Text = sb.ToString().Trim().Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + 3000;
                subtitle.Paragraphs.Add(p);
            }
            subtitle.Renumber();
        }

        private TimeCode DecodeTimeCode(string[] tokens)
        {
            var tc = new TimeCode();
            try
            {
                int hour = int.Parse(tokens[0]);
                int minutes = int.Parse(tokens[1]);
                int seconds = int.Parse(tokens[2]);
                int millisecond = int.Parse(tokens[3]);
                int milliseconds = Math.Min(999, millisecond * 100);
                tc = new TimeCode(hour, minutes, seconds, milliseconds);
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception.Message);
                _errorCount++;
            }
            return tc;
        }

    }
}
