using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class JsonType17 : SubtitleFormat
    {
        public override string Extension => ".json";

        public override string Name => "JSON Type 17";

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
                sb.Append("{");
                sb.Append("\"text\": [ ");
                var list = p.Text.SplitToLines();
                for (var index = 0; index < list.Count; index++)
                {
                    var line = list[index];
                    if (index > 0)
                    {
                        sb.Append(", ");
                    }
                    sb.Append($"\"{Json.EncodeJsonText(line)}\"");
                }

                sb.Append(" ], ");
                sb.Append($"\"index\":{count},");
                sb.Append($"\"start\": {p.StartTime.TotalMilliseconds}, ");
                sb.Append($"\"end\": {p.EndTime.TotalMilliseconds} ");
                sb.Append("}");
            }
            sb.AppendLine();
            sb.Append(']');
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            var sb = new StringBuilder();
            foreach (string s in lines)
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
                var startTimeObject = parser.GetFirstObject(element, "start");
                var endTimeObject = parser.GetFirstObject(element, "end");
                var texts = parser.GetAllTagsByNameAsStrings(element, "text");
                if (texts.Count > 0 && !string.IsNullOrEmpty(startTimeObject) && !string.IsNullOrEmpty(endTimeObject))
                {
                    if (long.TryParse(startTimeObject, out var startMs) && long.TryParse(endTimeObject, out var endMs))
                    {
                        var p = new Paragraph(string.Join(Environment.NewLine, texts), startMs, endMs);
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
