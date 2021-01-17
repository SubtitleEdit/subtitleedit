using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class JsonType18 : SubtitleFormat
    {
        public override string Extension => ".json";

        public override string Name => "JSON Type 18";

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
                sb.AppendLine($"    \"s\": {p.StartTime.TotalSeconds.ToString(CultureInfo.InvariantCulture)},");
                sb.AppendLine($"    \"d\": {p.Duration.TotalSeconds.ToString(CultureInfo.InvariantCulture)},");
                sb.AppendLine($"    \"n\": \"{Json.EncodeJsonText(p.Text, "\\n")}\"");
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
            if (!text.StartsWith("[", StringComparison.Ordinal))
            {
                return;
            }

            var parser = new SeJsonParser();
            var elements = parser.GetArrayElements(text);
            foreach (var element in elements)
            {
                var startTimeObject = parser.GetFirstObject(element, "s");
                var durationTimeObject = parser.GetFirstObject(element, "d");
                var s = parser.GetFirstObject(element, "n");
                if (!string.IsNullOrEmpty(s) && !string.IsNullOrEmpty(startTimeObject) && !string.IsNullOrEmpty(durationTimeObject))
                {
                    if (double.TryParse(startTimeObject, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var startSeconds) &&
                        double.TryParse(durationTimeObject, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var durationSeconds))
                    {
                        var pText = Json.DecodeJsonText(s);
                        var p = new Paragraph(pText, startSeconds * 1000.0, (startSeconds + durationSeconds) * 1000.0);
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
