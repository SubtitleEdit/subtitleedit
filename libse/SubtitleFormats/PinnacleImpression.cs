using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class PinnacleImpression : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d\d:\d\d:\d\d:\d\d \d\d:\d\d:\d\d:\d\d ", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Pinnacle Impression";

        public override bool IsMine(List<string> lines, string fileName)
        {
            var sb = new StringBuilder();
            foreach (string line in lines)
            {
                sb.AppendLine(line);
            }

            if (sb.ToString().Contains("#INPOINT OUTPOINT PATH"))
            {
                return base.IsMine(lines, fileName);
            }
            return false;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            sb.AppendLine(@"-------------------------------------------------
#INPOINT OUTPOINT PATH
-------------------------------------------------");
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                //00:03:15:22 00:03:23:10 This is line one.
                //This is line two.
                sb.AppendLine($"{EncodeTimeCode(p.StartTime)} {EncodeTimeCode(p.EndTime)} {HtmlUtil.RemoveHtmlTags(p.Text)}");
            }
            sb.AppendLine(@"-------------------------------------------------");

            return sb.ToString();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            //00:03:15:22 (last is frame)
            int frames = time.Milliseconds / (1000 / 30);
            return $"{time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}:{frames:00}";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            //00:03:15:22 00:03:23:10 This is line one.
            //This is line two.
            Paragraph p = null;
            subtitle.Paragraphs.Clear();
            _errorCount = 0;
            foreach (string line in lines)
            {
                if (RegexTimeCodes.IsMatch(line))
                {
                    string temp = line.Substring(0, RegexTimeCodes.Match(line).Length);
                    string start = temp.Substring(0, 11);
                    string end = temp.Substring(12, 11);

                    string[] startParts = start.Split(SplitCharColon, StringSplitOptions.RemoveEmptyEntries);
                    string[] endParts = end.Split(SplitCharColon, StringSplitOptions.RemoveEmptyEntries);
                    if (startParts.Length == 4 && endParts.Length == 4)
                    {
                        string text = line.Remove(0, RegexTimeCodes.Match(line).Length - 1).Trim();
                        p = new Paragraph(DecodeTimeCode(startParts), DecodeTimeCode(endParts), text);
                        subtitle.Paragraphs.Add(p);
                    }
                }
                else if (string.IsNullOrWhiteSpace(line.Trim('-')))
                {
                    // skip these lines
                }
                else if (!string.IsNullOrWhiteSpace(line) && p != null)
                {
                    if (string.IsNullOrEmpty(p.Text))
                    {
                        p.Text = line;
                    }
                    else
                    {
                        p.Text = p.Text + Environment.NewLine + line;
                    }
                }
            }

            subtitle.Renumber();
        }

        private static TimeCode DecodeTimeCode(string[] parts)
        {
            //00:00:07:12
            string hour = parts[0];
            string minutes = parts[1];
            string seconds = parts[2];
            string frames = parts[3];

            int milliseconds = (int)Math.Round(1000.0 / 30.0 * int.Parse(frames));
            if (milliseconds > 999)
            {
                milliseconds = 999;
            }

            return new TimeCode(int.Parse(hour), int.Parse(minutes), int.Parse(seconds), milliseconds);
        }

    }
}
