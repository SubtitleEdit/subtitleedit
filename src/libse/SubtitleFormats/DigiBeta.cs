using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class DigiBeta : SubtitleFormat
    {
        private static readonly Regex RegexTimeCode = new Regex(@"^\d\d \d\d \d\d \d\d\t\d\d \d\d \d\d \d\d\t", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "DigiBeta";

        public override string ToText(Subtitle subtitle, string title)
        {
            //10 01 37 23   10 01 42 01 Makkhi  (newline is TAB)
            const string paragraphWriteFormat = "{0}\t{1}\t{2}";

            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.AppendLine(string.Format(paragraphWriteFormat, EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime), p.Text.Replace(Environment.NewLine, "\t")));
            }
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            subtitle.Paragraphs.Clear();
            foreach (string line in lines)
            {
                if (RegexTimeCode.IsMatch(line) && line.Length > 24)
                {
                    string[] parts = line.Substring(0, 11).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 4)
                    {
                        try
                        {
                            var start = DecodeTimeCodeFramesFourParts(parts);
                            parts = line.Substring(12, 11).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            var end = DecodeTimeCodeFramesFourParts(parts);
                            var paragraph = new Paragraph
                            {
                                StartTime = start,
                                EndTime = end,
                                Text = line.Substring(24).Trim().Replace("\t", Environment.NewLine)
                            };

                            subtitle.Paragraphs.Add(paragraph);
                        }
                        catch
                        {
                            _errorCount++;
                        }
                    }
                }
            }
            subtitle.Renumber();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            return $"{time.Hours:00} {time.Minutes:00} {time.Seconds:00} {MillisecondsToFramesMaxFrameRate(time.Milliseconds):00}";
        }

    }
}
