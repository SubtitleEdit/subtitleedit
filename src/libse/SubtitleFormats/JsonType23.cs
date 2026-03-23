using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class JsonType23 : SubtitleFormat
    {
        public override string Extension => ".json";

        public override string Name => "JSON Type 23";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder("{" + Environment.NewLine + "  \"titulky\": [");
            var count = 0;
            foreach (var p in subtitle.Paragraphs)
            {
                if (count > 0)
                {
                    sb.Append(',');
                }

                count++;
                sb.AppendLine();
                sb.AppendLine("    {");
                sb.AppendLine($"      \"s\": {p.StartTime.TotalSeconds.ToString(CultureInfo.InvariantCulture)},");
                sb.AppendLine($"      \"d\": {p.Duration.TotalSeconds.ToString(CultureInfo.InvariantCulture)},");
                sb.AppendLine($"      \"n\": \"{Json.EncodeJsonText(p.Text, "\\n")}\"");
                sb.Append("    }");
            }

            sb.Append(" ]" + Environment.NewLine + "}");

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
            if (!text.Contains("\"titulky\"", StringComparison.Ordinal))
            {
                return;
            }

            var parser = new SeJsonParser();
            var words = parser.GetArrayElementsByName(text, "titulky");
            foreach (var word in words)
            {
                var startTimeObject = parser.GetFirstObject(word, "s");
                var durationObject = parser.GetFirstObject(word, "d");
                var s = parser.GetFirstObject(word, "n");
                if (!string.IsNullOrEmpty(s) && !string.IsNullOrEmpty(startTimeObject) && !string.IsNullOrEmpty(durationObject))
                {
                    if (double.TryParse(startTimeObject.TrimEnd('s'), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var startSecs) &&
                        double.TryParse(durationObject.TrimEnd('s'), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var duration))
                    {
                        var pText = Json.DecodeJsonText(s);
                        var p = new Paragraph(pText, startSecs * 1000.0, (startSecs + duration) * 1000.0);
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
