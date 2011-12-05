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
        public override string Extension
        {
            get { return ".txt"; }
        }

        public override string Name
        {
            get { return "F4 Text"; }
        }

        public override bool HasLineNumber
        {
            get { return false; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (fileName != null && !fileName.ToLower().EndsWith(Extension))
                return false;

            var subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public static string ToF4Text(Subtitle subtitle, string title)
        {
            StringBuilder sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.Append(string.Format("{0}{1}{2}", EncodeTimeCode(p.StartTime), Utilities.RemoveHtmlTags(p.Text), EncodeTimeCode(p.EndTime)));
            }
            return sb.ToString().Trim();
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            return ToF4Text(subtitle, title);
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            return string.Format(" #{0:00}:{1:00}:{2:00}-{3:0}# ", time.Hours, time.Minutes, time.Seconds, Math.Round(time.Milliseconds / 100.0, 0));
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string line in lines)
                sb.AppendLine(line);
            string text = sb.ToString();
            if (text.Contains("{\\rtf"))
                return;
            LoadF4TextSubtitle(subtitle, text);
        }

        protected void LoadF4TextSubtitle(Subtitle subtitle, string text)
        {
            var regexTimeCodes = new Regex(@"^\d\d:\d\d:\d\d-\d$", RegexOptions.Compiled);
            Paragraph p = null;
            subtitle.Paragraphs.Clear();
            var arr = text.ToString().Trim().Split("#".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
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
                        p.StartTime = DecodeTimeCode(line.Split(":-".ToCharArray(), StringSplitOptions.RemoveEmptyEntries));
                    }
                    else
                    {
                        p.EndTime = DecodeTimeCode(line.Split(":-".ToCharArray(), StringSplitOptions.RemoveEmptyEntries));
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

        private TimeCode DecodeTimeCode(string[] parts)
        {
            TimeCode tc = new TimeCode(0, 0, 0, 0);
            try
            {
                string hour = parts[0];
                string minutes = parts[1];
                string seconds = parts[2];
                string millisecond = parts[3];

                int milliseconds = (int)(int.Parse(millisecond) * 100.0);
                if (milliseconds > 999)
                    milliseconds = 999;

                tc = new TimeCode(int.Parse(hour), int.Parse(minutes), int.Parse(seconds), milliseconds);
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

