using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class WhisperRaw2 : SubtitleFormat
    {
        private readonly Regex _timeRegex = new Regex(@"^\[\d+.\d+s -> \d+.\d+s\]", RegexOptions.Compiled);
        public override string Extension => ".txt";
        public override string Name => "Whisper Raw 2";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            const string writeFormat = "[{0} -> {1}]   {2}";
            foreach (var p in subtitle.Paragraphs)
            {
                sb.AppendLine(string.Format(writeFormat, EncodeEndTimeCode(p.StartTime), EncodeEndTimeCode(p.EndTime), HtmlUtil.RemoveHtmlTags(p.Text.Replace(Environment.NewLine, " "), true)));
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private static string EncodeEndTimeCode(TimeCode time)
        {
            return $"{time.TotalSeconds:0.00}s";
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
                    var match = _timeRegex.Match(trimmedLine);
                    if (match.Success)
                    {
                        var timeString = trimmedLine.Substring(0, match.Length).Trim('[', ']');
                        var splitPos = timeString.IndexOf('>');
                        if (splitPos > 1 && splitPos < timeString.Length - 3)
                        {
                            var start = timeString.Substring(0, splitPos -1).Trim().TrimEnd('s');
                            var end = timeString.Substring(splitPos +1).Trim().TrimEnd('s');
                            var text = trimmedLine.Remove(0, match.Length).Trim();
                            if (!string.IsNullOrEmpty(text))
                            {
                                if (double.TryParse(start, NumberStyles.Any, CultureInfo.InvariantCulture, out var dStart) &&
                                    double.TryParse(end, NumberStyles.Any, CultureInfo.InvariantCulture, out var dEnd))
                                {
                                    subtitle.Paragraphs.Add(new Paragraph(text, dStart * 1000.0, dEnd * 1000.0));
                                }
                            }
                        }
                    }
                }
            }

            subtitle.Sort(SubtitleSortCriteria.StartTime);
            subtitle.Renumber();
        }
    }
}
