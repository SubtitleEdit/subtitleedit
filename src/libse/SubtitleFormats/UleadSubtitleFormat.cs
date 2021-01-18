using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UleadSubtitleFormat : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^#\d+ \d\d;\d\d;\d\d;\d\d \d\d;\d\d;\d\d;\d\d", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Ulead subtitle format";

        public override string ToText(Subtitle subtitle, string title)
        {
            const string Header = @"#Ulead subtitle format

#Subtitle stream attribute begin
#FR:25.00
#Subtitle stream attribute end

#Subtitle text begin";

            const string footer = @"#Subtitle text end
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
            sb.AppendLine(string.Format(footer, subtitle.Paragraphs.Count));
            return sb.ToString();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            //00;04;27;05
            int frames = time.Milliseconds / (1000 / 25);
            return $"{time.Hours:00};{time.Minutes:00};{time.Seconds:00};{frames:00}";
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
                if (line.StartsWith('#') && RegexTimeCodes.IsMatch(line))
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
                    {
                        p.Text = line.TrimEnd();
                    }
                    else
                    {
                        p.Text = p.Text + Environment.NewLine + line.TrimEnd();
                    }
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

            int milliseconds = (int)Math.Round(1000.0 / 25.0 * frames);
            if (milliseconds > 999)
            {
                milliseconds = 999;
            }

            return new TimeCode(hour, minutes, seconds, milliseconds);
        }

    }
}
