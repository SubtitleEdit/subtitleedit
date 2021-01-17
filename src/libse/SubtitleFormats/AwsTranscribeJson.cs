using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class AwsTranscribeJson : SubtitleFormat
    {
        public override string Extension => ".json";

        public override string Name => "AWS Transcribe";

        public override string ToText(Subtitle subtitle, string title)
        {
            if (string.IsNullOrEmpty(title))
            {
                title = "untitled";
            }
            var sb = new StringBuilder("{ \"jobName\": \"" + Json.EncodeJsonText(title) + "\", \"accountId\": \"0\", \"results\": { \"items\": [ ");
            for (var index = 0; index < subtitle.Paragraphs.Count; index++)
            {
                Paragraph p = subtitle.Paragraphs[index];
                if (index > 0)
                {
                    sb.Append(',');
                }

                sb.Append("{ \"start_time\":");
                sb.Append(p.StartTime.TotalSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture));
                sb.Append(", \"end_time\":");
                sb.Append(p.EndTime.TotalSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture));
                sb.Append(", \"alternatives\": [ { \"confidence\": \"1\", \"content\": \"");
                sb.Append(Json.EncodeJsonText(p.Text) + "\" } ]");
                sb.Append(",\"type\": \"pronunciation\"");
                sb.Append('}');
            }

            sb.Append("]}}");
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

            var text = sb.ToString();
            if (!text.Contains("\"items\"") || !text.Contains("\"alternatives\"", StringComparison.Ordinal))
            {
                return;
            }

            var parser = new SeJsonParser();
            foreach (string json in parser.GetArrayElementsByName(text, "items"))
            {
                var start = parser.GetAllTagsByNameAsStrings(json, "start_time").FirstOrDefault();
                var end = parser.GetAllTagsByNameAsStrings(json, "end_time").FirstOrDefault();
                var content = parser.GetAllTagsByNameAsStrings(json, "content").FirstOrDefault();
                if (start != null && end != null && !string.IsNullOrEmpty(content))
                {
                    if (double.TryParse(start, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out var startSeconds) &&
                        double.TryParse(end, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out var endSeconds))
                    {
                        subtitle.Paragraphs.Add(new Paragraph(Json.DecodeJsonText(content), startSeconds * TimeCode.BaseUnit, endSeconds * TimeCode.BaseUnit));
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
