using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class Son : SubtitleFormat
    {
        public override string Extension => ".son";

        public override string Name => "SON";

        public override string ToText(Subtitle subtitle, string title)
        {
            const string writeFormat = "{0:0000}\t{1}\t{2}\t{3}";
            var sb = new StringBuilder();
            int index = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                index++;
                sb.AppendLine(string.Format(writeFormat, index, EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime), HtmlUtil.RemoveHtmlTags(p.Text).Replace(Environment.NewLine, "\t")));
            }
            return sb.ToString();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            //00:03:15:22 (last is frame)
            return time.ToHHMMSSFF();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            //0001  00:00:19:13 00:00:22:10 a_0001.tif
            var regexTimeCodes = new Regex(@"^\d\d\d\d\t+(\d\d:\d\d:\d\d:\d\d)\t(\d\d:\d\d:\d\d:\d\d)\t.+\.(?:tif|tiff|png|bmp|TIF|TIFF|PNG|BMP)", RegexOptions.Compiled);
            Paragraph p = null;
            subtitle.Paragraphs.Clear();
            _errorCount = 0;
            int index = 0;
            foreach (string line in lines)
            {
                var match = regexTimeCodes.Match(line);
                if (match.Success)
                {
                    var start = DecodeTimeCodeFramesFourParts(match.Groups[1].Value.Split(SplitCharColon, StringSplitOptions.RemoveEmptyEntries));
                    var end = DecodeTimeCodeFramesFourParts(match.Groups[2].Value.Split(SplitCharColon, StringSplitOptions.RemoveEmptyEntries));
                    var lastTabIndex = line.LastIndexOf('\t');
                    var text = line.Substring(lastTabIndex + 1).Trim();
                    p = new Paragraph(start, end, text);
                    subtitle.Paragraphs.Add(p);
                }
                else if (index < 10 || string.IsNullOrWhiteSpace(line) || line[0] == '#' || line.StartsWith("Display_Area", StringComparison.Ordinal) || line.StartsWith("Color", StringComparison.Ordinal))
                {
                    // skip these lines
                }
                else if (p != null)
                {
                    _errorCount++;
                }
                index++;
            }
            subtitle.Renumber();
        }

    }
}
