using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class AdobeEncoreWithLineNumbersNtsc : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d+ \d\d;\d\d;\d\d;\d\d \d\d;\d\d;\d\d;\d\d ", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Adobe Encore NTSC";

        public override bool IsMine(List<string> lines, string fileName)
        {
            var sb = new StringBuilder();
            foreach (string line in lines)
            {
                sb.AppendLine(line);
            }

            if (sb.ToString().Contains("#INPOINT OUTPOINT PATH"))
            {
                return false; // Pinnacle Impression
            }

            return base.IsMine(lines, fileName);
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            int index = 1;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                //00:03:15:22 00:03:23:10 This is line one.
                //This is line two.
                sb.AppendLine($"{index} {EncodeTimeCode(p.StartTime)} {EncodeTimeCode(p.EndTime)} {HtmlUtil.RemoveHtmlTags(p.Text, true)}");
                index++;
            }
            return sb.ToString();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            //00;03;15;22 (last is frame)
            return $"{time.Hours:00};{time.Minutes:00};{time.Seconds:00};{MillisecondsToFramesMaxFrameRate(time.Milliseconds):00}";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            //00;03;15;22 00;03;23;10 This is line one.
            //This is line two.
            Paragraph p = null;
            subtitle.Paragraphs.Clear();
            _errorCount = 0;
            foreach (string line in lines)
            {
                if (RegexTimeCodes.IsMatch(line))
                {
                    string temp = line.Substring(line.IndexOf(' ')).Trim();
                    string start = temp.Substring(0, 11);
                    string end = temp.Substring(12, 11);

                    string[] startParts = start.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    string[] endParts = end.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    if (startParts.Length == 4 && endParts.Length == 4)
                    {
                        string text = line.Remove(0, RegexTimeCodes.Match(line).Length - 1).Trim();
                        p = new Paragraph(DecodeTimeCodeFramesFourParts(startParts), DecodeTimeCodeFramesFourParts(endParts), text);
                        subtitle.Paragraphs.Add(p);
                    }
                }
                else if (string.IsNullOrWhiteSpace(line))
                {
                    // skip these lines
                }
                else if (p != null)
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
    }
}
