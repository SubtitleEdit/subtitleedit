using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UleadSubtitleFormat : SubtitleFormat
    {
        private static readonly Regex regexTimeCodes = new Regex(@"^#\d+ \d\d;\d\d;\d\d;\d\d \d\d;\d\d;\d\d;\d\d", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".txt"; }
        }

        public override string Name
        {
            get { return "Ulead subtitle format"; }
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
            const string Header = @"#Ulead subtitle format

#Subtitle stream attribute begin
#FR:25.00
#Subtitle stream attribute end

#Subtitle text begin";

            const string Footer = @"#Subtitle text end
#Subtitle text attribute begin
#/R:1,{0} /FP:8  /FS:24
#Subtitle text attribute end";

            var sb = new StringBuilder();
            sb.AppendLine(Header);
            int index = 0;
            const string writeFormat = "#{0} {1} {2}";
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                //#3 00;04;26;04 00;04;27;05
                //How much in there? -
                //Three...
                sb.AppendLine(string.Format(writeFormat, index, EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime)));
                sb.AppendLine(HtmlUtil.RemoveHtmlTags(p.Text, true));
                index++;
            }
            sb.AppendLine(string.Format(Footer, subtitle.Paragraphs.Count));
            return sb.ToString();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            //00;04;27;05
            int frames = time.Milliseconds / (1000 / 25);
            return string.Format("{0:00};{1:00};{2:00};{3:00}", time.Hours, time.Minutes, time.Seconds, frames);
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            //#3 00;04;26;04 00;04;27;05
            //How much in there? -
            //Three...
            Paragraph p = null;
            subtitle.Paragraphs.Clear();
            _errorCount = 0;
            char[] splitChar = { ' ' };
            foreach (string l2 in lines)
            {
                string line = l2.TrimEnd('ഀ');
                if (line.StartsWith('#') && regexTimeCodes.IsMatch(line))
                {
                    string[] parts = line.Split(splitChar, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 3)
                    {
                        string start = parts[1];
                        string end = parts[2];
                        try
                        {
                            p = new Paragraph(DecodeTimeCode(start), DecodeTimeCode(end), string.Empty);
                            subtitle.Paragraphs.Add(p);
                        }
                        catch
                        {
                            _errorCount++;
                        }
                    }
                }
                else if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                {
                    // skip these lines
                }
                else if (p != null)
                {
                    if (string.IsNullOrEmpty(p.Text))
                        p.Text = line.TrimEnd();
                    else
                        p.Text = p.Text + Environment.NewLine + line.TrimEnd();
                }
                else
                {
                    _errorCount++;
                }
            }
            subtitle.Renumber();
        }

        private static TimeCode DecodeTimeCode(string time)
        {
            //00;04;26;04

            var hour = int.Parse(time.Substring(0, 2));
            var minutes = int.Parse(time.Substring(3, 2));
            var seconds = int.Parse(time.Substring(6, 2));
            var frames = int.Parse(time.Substring(9, 2));

            int milliseconds = (int)(1000 / 25.0 * frames);
            if (milliseconds > 999)
                milliseconds = 999;

            return new TimeCode(hour, minutes, seconds, milliseconds);
        }

    }
}