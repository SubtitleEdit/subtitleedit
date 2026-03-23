using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// Google Cloud Speech-to-text?
    /// </summary>
    public class JsonType21 : SubtitleFormat
    {
        public override string Extension => ".json";

        public override string Name => "JSON Type 21";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder(@"[");
            var count = 0;
            foreach (var p in subtitle.Paragraphs)
            {
                if (count > 0)
                {
                    sb.Append(',');
                }

                count++;
                sb.AppendLine();
                sb.AppendLine("{");
                sb.AppendLine($"  \"timestamp\": [{p.StartTime.TotalSeconds.ToString(CultureInfo.InvariantCulture)}, {p.EndTime.TotalSeconds.ToString(CultureInfo.InvariantCulture)}],");
                sb.AppendLine($"  \"text\": \"{Json.EncodeJsonText(p.Text, "\\n")}\"");
                sb.Append("}");
            }

            sb.Append(@"]");

            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            var sb = new StringBuilder();
            foreach (var s in lines)
            {
                sb.Append(s);
            }

            var text = sb.ToString().TrimStart();
            if (!text.Contains("\"timestamp\"", StringComparison.Ordinal) || !text.Contains("\"text\"", StringComparison.Ordinal))
            {
                return;
            }

            var parser = new SeJsonParser();
            var elements = parser.GetArrayElements(text);
            foreach (var element in elements)
            {
                var arr = parser.GetAllTagsByNameAsStrings(element, "timestamp");
                if (arr.Count != 2)
                {
                    _errorCount++;
                    continue;
                }

                var startTimeObject = arr[0];
                var endTimeObject = arr[1];
                var s = parser.GetFirstObject(element, "text");
                if (!string.IsNullOrEmpty(s) && !string.IsNullOrEmpty(startTimeObject) && !string.IsNullOrEmpty(endTimeObject))
                {
                    if (double.TryParse(startTimeObject.TrimEnd('s'), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var startSecs) &&
                        double.TryParse(endTimeObject.TrimEnd('s'), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var endSecs))
                    {
                        var pText = Json.DecodeJsonText(s);
                        pText = Utilities.AutoBreakLine(pText);
                        var p = new Paragraph(pText, startSecs * 1000.0, endSecs * 1000.0);
                        subtitle.Paragraphs.Add(p);
                    }
                    else
                    {
                        _errorCount++;
                    }
                }
            }

            subtitle.Renumber();
        }
    }
}
