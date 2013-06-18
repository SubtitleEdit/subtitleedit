using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class UnknownSubtitle58 : SubtitleFormat
    {

        //[01:01:53:09]
        static Regex regexTimeCodes = new Regex(@"^\[\d\d:\d\d:\d\d:\d\d]$", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".rtf"; }
        }

        public override string Name
        {
            get { return "Unknown 58"; }
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
            string format = "[{0}]{3}{3}{2}{3}{3}[{1}]{3}";
            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.AppendLine(string.Format(format, EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime), p.Text, Environment.NewLine));
            }

            System.Windows.Forms.RichTextBox rtBox = new System.Windows.Forms.RichTextBox();
            rtBox.Text = sb.ToString();
            return rtBox.Rtf;
        }

        private string EncodeTimeCode(TimeCode time)
        {
            return string.Format("{0:00}:{1:00}:{2:00}:{3:00}", time.Hours, time.Minutes, time.Seconds, MillisecondsToFramesMaxFrameRate(time.Milliseconds));
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            var sb = new StringBuilder();
            foreach (string line in lines)
                sb.AppendLine(line);

            string rtf = sb.ToString().Trim();
            if (!rtf.StartsWith("{\\rtf"))
                return;

            var rtBox = new System.Windows.Forms.RichTextBox();
            try
            {
                rtBox.Rtf = rtf;
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception.Message);
                return;
            }

            var p = new Paragraph();
            subtitle.Paragraphs.Clear();
            foreach (string line in rtBox.Text.Replace("\r", "").Split('\n'))
            {
                string s = line.Trim();
                if (s.StartsWith("[") && s.EndsWith(">") && s.Length > 13 && s.Substring(12, 1) == "]")
                    s = s.Substring(0, 13);

                var match = regexTimeCodes.Match(s);
                if (match.Success)
                {
                    string[] parts = s.Replace("[", string.Empty).Replace("]", string.Empty).Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 1)
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(p.Text))
                            {
                                p.EndTime = DecodeTimeCode(parts[0]);
                                subtitle.Paragraphs.Add(p);
                                p = new Paragraph();
                            }
                            else
                            {
                                p.StartTime = DecodeTimeCode(parts[0]);
                            }
                        }
                        catch (Exception exception)
                        {
                            _errorCount++;
                            System.Diagnostics.Debug.WriteLine(exception.Message);
                        }
                    }
                }
                else if (line.Trim().Length == 0)
                {
                }
                else if (line.Trim().Length > 0)
                {
                    p.Text = (p.Text + Environment.NewLine + line).Trim();
                    if (p.Text.Length > 500)
                    {
                        _errorCount+=10;
                        return;
                    }
                    while (p.Text.Contains(Environment.NewLine + " "))
                        p.Text = p.Text.Replace(Environment.NewLine + " ", Environment.NewLine);
                }
            }
            if (!string.IsNullOrEmpty(p.Text))
                subtitle.Paragraphs.Add(p);

            subtitle.RemoveEmptyLines();
            subtitle.Renumber(1);
        }

        private TimeCode DecodeTimeCode(string part)
        {
            string[] parts = part.Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            //00:00:07:12
            string hour = parts[0];
            string minutes = parts[1];
            string seconds = parts[2];
            string frames = parts[3];

            return new TimeCode(int.Parse(hour), int.Parse(minutes), int.Parse(seconds), FramesToMillisecondsMax999(int.Parse(frames)));
        }

    }
}

