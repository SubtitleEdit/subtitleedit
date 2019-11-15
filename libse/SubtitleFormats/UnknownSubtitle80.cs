using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle80 : SubtitleFormat
    {
        // [00:02:08.21]abc 123[00:02:13.01]
        private static readonly Regex RegexTimeCode = new Regex(@"^\[\d\d:\d\d:\d\d.\d\d\].*\[\d\d:\d\d:\d\d.\d\d\]$", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Unknown 80";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                var text = HtmlUtil.RemoveHtmlTags(p.Text.Trim()).Replace(Environment.NewLine, "//");
                sb.AppendLine(EncodeTimeCode(p.StartTime) + text +  EncodeTimeCode(p.EndTime));
            }
            return sb.ToString();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            var splitChars = new[] { ':', '.' };
            _errorCount = 0;
            subtitle.Paragraphs.Clear();
            foreach (string line in lines)
            {
                string s = line.Trim();
                if (s.Length > 25 && RegexTimeCode.IsMatch(s))
                {
                    {
                        try
                        {
                            var text = line.Substring(13, line.Length - 26).Replace("//", Environment.NewLine);

                            var startTime = line.Substring(1, 11);
                            var endTime = line.Substring(line.Length - 12, 11);

                            var startTimeParts = startTime.Split(splitChars);
                            var endTimeParts = endTime.Split(splitChars);

                            var paragraph = new Paragraph { StartTime = DecodeTimeCodeFramesFourParts(startTimeParts), EndTime = DecodeTimeCodeFramesFourParts(endTimeParts), Text = text };
                            subtitle.Paragraphs.Add(paragraph);

                        }
                        catch (Exception)
                        {
                            _errorCount++;
                        }
                    }
                }
                else if (s.Length > 0)
                {
                    _errorCount++;
                    if (_errorCount > 10)
                    {
                        return;
                    }
                }
            }
            subtitle.RemoveEmptyLines();
            subtitle.Renumber();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            return "[" + time.ToHHMMSSFF().Remove(8, 1).Insert(8, ".") + "]";
        }

    }
}
