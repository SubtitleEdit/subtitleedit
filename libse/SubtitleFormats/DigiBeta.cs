using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class DigiBeta : SubtitleFormat
    {
        private static readonly Regex RegexTimeCode = new Regex(@"^\d\d \d\d \d\d \d\d\t\d\d \d\d \d\d \d\d\t", RegexOptions.Compiled);
        private readonly string NewLineToken = "\t";

        public override string Extension => ".txt";

        public override string Name => "DigiBeta";

        public override string ToText(Subtitle subtitle, string title)
        {
            //10 01 37 23   10 01 42 01 Makkhi  (newline is TAB)
            const string paragraphWriteFormat = "{0}\t{1}\t{2}";

            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.AppendLine(string.Format(paragraphWriteFormat, EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime), p.Text.Replace(Environment.NewLine, NewLineToken)));
            }
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            subtitle.Paragraphs.Clear();
            char[] splitChar = { ' ' };
            int lineNumber = 1;

            foreach (string line in lines)
            {
                if (line.Length > 24 && RegexTimeCode.IsMatch(line))
                {
                    try
                    {
                        string[] startTokens = line.Substring(0, 11).Split(splitChar, StringSplitOptions.RemoveEmptyEntries);
                        string[] endTokens = line.Substring(12, 11).Split(splitChar, StringSplitOptions.RemoveEmptyEntries);

                        subtitle.Paragraphs.Add(new Paragraph
                        {
                            Number = lineNumber++,
                            StartTime = DecodeTimeCodeFramesFourParts(startTokens),
                            EndTime = DecodeTimeCodeFramesFourParts(endTokens),
                            Text = line.Substring(24).Trim().Replace(NewLineToken, Environment.NewLine)
                        });
                    }
                    catch
                    {
                        _errorCount++;
                    }
                }
                else
                {
                    _errorCount++;
                }
            }

        }

        private static string EncodeTimeCode(TimeCode time) => $"{time.Hours:00} {time.Minutes:00} {time.Seconds:00} {MillisecondsToFramesMaxFrameRate(time.Milliseconds):00}";

    }
}
