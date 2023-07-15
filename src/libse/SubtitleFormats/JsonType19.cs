using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// Partial "GooglePlayJson"
    /// </summary>
    public class JsonType19 : SubtitleFormat
    {
        public override string Extension => ".json";

        public override string Name => "JSON Type 19";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder(@"[");
            int count = 0;
            foreach (var p in subtitle.Paragraphs)
            {
                if (count > 0)
                {
                    sb.Append(',');
                }
                count++;
                sb.AppendLine();
                sb.AppendLine("  {");
                sb.AppendLine($"    \"tStartMs\": {p.StartTime.TotalMilliseconds.ToString(CultureInfo.InvariantCulture)},");
                sb.AppendLine($"    \"dDurationMs\": {p.DurationTotalMilliseconds.ToString(CultureInfo.InvariantCulture)},");
                sb.AppendLine($"    \"segs\": [{{ \"utf8\": \"{Json.EncodeJsonText(p.Text, "\\n")}\" }} ]");
                sb.Append("  }");
            }
            sb.AppendLine();
            sb.Append(']');
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
            if (!text.Contains("\"segs\"", StringComparison.Ordinal) || !text.Contains('['))
            {
                return;
            }

            var parser = new SeJsonParser();
            var elements = parser.GetArrayElements(text.Substring(text.IndexOf('[')));
            foreach (var element in elements)
            {
                var startTimeObject = parser.GetFirstObject(element, "tStartMs");
                var durationTimeObject = parser.GetFirstObject(element, "dDurationMs");
                var idx = element.IndexOf('[');
                var s = string.Empty;
                if (idx > 0)
                {
                    sb = new StringBuilder();
                    var arr = parser.GetArrayElements(element.Substring(idx));
                    foreach (var line in arr)
                    {
                        var content = parser.GetFirstObject(line, "utf8");
                        if (content != null)
                        {
                            sb.Append(content.Replace("\\n", Environment.NewLine) + " ");
                        }
                    }
                    s = sb.ToString().Trim();

                    if (!string.IsNullOrEmpty(s) && !string.IsNullOrEmpty(startTimeObject) && !string.IsNullOrEmpty(durationTimeObject))
                    {
                        if (double.TryParse(startTimeObject, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var startMs) &&
                            double.TryParse(durationTimeObject, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var durationMs))
                        {
                            var pText = Json.DecodeJsonText(s);
                            var p = new Paragraph(pText, startMs, startMs + durationMs);
                            subtitle.Paragraphs.Add(p);
                        }
                        else
                        {
                            _errorCount++;
                        }
                    }
                }
            }
            subtitle.Renumber();
        }
    }
}
