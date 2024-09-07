using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class PodcastIndexer : SubtitleFormat
    {
        public override string Extension => ".json";

        public override string Name => "PodcastIndexer";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder("{" +
               Environment.NewLine + "  \"version\": \"1.0.0\"" +
               Environment.NewLine + "  \"segments\": [");
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

                if (!string.IsNullOrEmpty(p.Actor))
                {
                    sb.AppendLine($"      \"speaker\": \"{p.Actor}\"");
                }

                sb.AppendLine($"      \"startTime\": {p.StartTime.TotalSeconds.ToString(CultureInfo.InvariantCulture)},");
                sb.AppendLine($"      \"endTime\": {p.EndTime.TotalSeconds.ToString(CultureInfo.InvariantCulture)},");
                sb.AppendLine($"      \"body\": \"{Json.EncodeJsonText(p.Text, "\\n")}\"");
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
            if (!text.Contains("\"segments\"", StringComparison.Ordinal))
            {
                return;
            }

            var parser = new SeJsonParser();
            var words = parser.GetArrayElementsByName(text, "segments");
            foreach (var word in words)
            {
                var startTimeObject = parser.GetFirstObject(word, "startTime");
                var endTimeObject = parser.GetFirstObject(word, "endTime");
                var actorObject = parser.GetFirstObject(word, "speaker");
                var s = parser.GetFirstObject(word, "body");
                if (!string.IsNullOrEmpty(s) && !string.IsNullOrEmpty(startTimeObject) && !string.IsNullOrEmpty(endTimeObject))
                {
                    if (double.TryParse(startTimeObject, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var startSecs) &&
                        double.TryParse(endTimeObject, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var endSecs))
                    {
                        var pText = Json.DecodeJsonText(s);
                        var p = new Paragraph(pText, startSecs * 1000.0, endSecs * 1000.0);

                        if (!string.IsNullOrEmpty(actorObject))
                        {
                            p.Actor = actorObject;
                        }

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
