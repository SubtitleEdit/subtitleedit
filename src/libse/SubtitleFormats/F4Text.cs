using Nikse.SubtitleEdit.Core.Common;
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
            if (fileName != null && !fileName.EndsWith(Extension, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return base.IsMine(lines, fileName);
        }

        public static string ToF4Text(Subtitle subtitle)
        {
            var sb = new StringBuilder();
            const string writeFormat = "{0}{1}";
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.AppendFormat(writeFormat, HtmlUtil.RemoveHtmlTags(p.Text, true), EncodeTimeCode(p.EndTime));
            }
            return sb.ToString().Trim();
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            return ToF4Text(subtitle);
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            return $" #{time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}-{Math.Round(time.Milliseconds / 100.0, 0):0}# ";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            var sb = new StringBuilder();
            foreach (string line in lines)
            {
                sb.AppendLine(line);
            }

            string text = sb.ToString();
            if (text.Contains("{\\rtf") || text.Contains("<transcript>"))
            {
                return;
            }

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
                if (RegexTimeCodes.IsMatch(line))
                {
                    if (p == null)
                    {
                        p = new Paragraph();
                        if (currentText.Length > 0)
                        {
                            p.Text = currentText.ToString().Trim().Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                            p.Text = p.Text.Trim('\n', '\r');
                            subtitle.Paragraphs.Add(p);
                            p = new Paragraph();
                        }
                    }
                    if (Math.Abs(p.StartTime.TotalMilliseconds) < 0.01 || currentText.Length == 0)
                    {
                        p.StartTime = DecodeTimeCode(line.Split(new[] { ':', '-' }, StringSplitOptions.RemoveEmptyEntries));
                    }
                    else
                    {
                        p.EndTime = DecodeTimeCode(line.Split(new[] { ':', '-' }, StringSplitOptions.RemoveEmptyEntries));
                        p.Text = currentText.ToString().Trim().Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                        p.Text = p.Text.Trim('\n', '\r').Trim();
                        subtitle.Paragraphs.Add(p);
                        p = null;
                        currentText.Clear();
                    }
                }
                else
                {
                    if (p == null && subtitle.Paragraphs.Count > 0)
                    {
                        p = new Paragraph { StartTime = { TotalMilliseconds = subtitle.Paragraphs[subtitle.Paragraphs.Count - 1].EndTime.TotalMilliseconds } };
                    }
                    currentText.AppendLine(line.Trim());
                }
            }
            if (currentText.Length > 0 && subtitle.Paragraphs.Count > 0 && currentText.Length < 1000)
            {
                if (p == null)
                {
                    p = new Paragraph();
                }

                p.Text = currentText.ToString().Trim().Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                p.Text = p.Text.Trim('\n', '\r').Trim();
                p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + 3000;
                subtitle.Paragraphs.Add(p);
            }
            subtitle.Renumber();
        }

        private TimeCode DecodeTimeCode(string[] parts)
        {
            var tc = new TimeCode();
            try
            {
                int hour = int.Parse(parts[0]);
                int minutes = int.Parse(parts[1]);
                int seconds = int.Parse(parts[2]);
                int millisecond = int.Parse(parts[3]);

                int milliseconds = (int)Math.Round(millisecond * 100.0);
                if (milliseconds > 999)
                {
                    milliseconds = 999;
                }

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
