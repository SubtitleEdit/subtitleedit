using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class WhisperRaw : SubtitleFormat
    {
        private readonly Regex _timeRegexShort = new Regex(@"^\[\d\d:\d\d[\.,]\d\d\d --> \d\d:\d\d[\.,]\d\d\d\]", RegexOptions.Compiled);
        private readonly Regex _timeRegexLong = new Regex(@"^\[\d\d:\d\d:\d\d[\.,]\d\d\d --> \d\d:\d\d:\d\d[\.,]\d\d\d]", RegexOptions.Compiled);
        public override string Extension => ".txt";
        public override string Name => "Whisper Raw";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            const string writeFormat = "[{0} --> {1}]   {2}";
            foreach (var p in subtitle.Paragraphs)
            {
                sb.AppendLine(string.Format(writeFormat, EncodeEndTimeCode(p.StartTime), EncodeEndTimeCode(p.EndTime), HtmlUtil.RemoveHtmlTags(p.Text.Replace(Environment.NewLine, " "), true)));
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private static string EncodeEndTimeCode(TimeCode time)
        {
            return $"{time.ToDisplayString()}";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            subtitle.Paragraphs.Clear();
            _errorCount = 0;
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (trimmedLine.StartsWith('['))
                {
                    if (_timeRegexShort.IsMatch(trimmedLine))
                    {
                        var start = trimmedLine.Substring(1, 10);
                        var end = trimmedLine.Substring(14, 10);
                        var text = trimmedLine.Remove(0, 25).Trim();
                        if (!string.IsNullOrEmpty(text))
                        {
                            subtitle.Paragraphs.Add(new Paragraph(text, GetMs(start), GetMs(end)));
                        }
                    }
                    else if (_timeRegexLong.IsMatch(trimmedLine))
                    {
                        var start = trimmedLine.Substring(1, 12);
                        var end = trimmedLine.Substring(18, 12);
                        var text = trimmedLine.Remove(0, 31).Trim();
                        if (!string.IsNullOrEmpty(text))
                        {
                            subtitle.Paragraphs.Add(new Paragraph(text, GetMs(start), GetMs(end)));
                        }
                    }
                }
            }

            subtitle.Sort(SubtitleSortCriteria.StartTime);
            subtitle.Renumber();
        }

        private static double GetMs(string timeCode)
        {
            return TimeCode.ParseToMilliseconds(timeCode);
        }
    }
}
