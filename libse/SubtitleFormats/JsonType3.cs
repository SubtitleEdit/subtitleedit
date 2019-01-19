using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class JsonType3 : SubtitleFormat
    {
        public override string Extension => ".json";

        public override string Name => "JSON Type 3";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder(@"[");
            int count = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                if (count > 0)
                {
                    sb.Append(',');
                }

                sb.Append("{\"duration\":");
                sb.Append(p.Duration.TotalMilliseconds.ToString(System.Globalization.CultureInfo.InvariantCulture));
                sb.Append(",\"content\":\"");
                sb.Append(Json.EncodeJsonText(p.Text) + "\"");
                sb.Append(",\"startOfParagraph\":false");
                sb.Append(",\"startTime\":");
                sb.Append(p.StartTime.TotalMilliseconds.ToString(System.Globalization.CultureInfo.InvariantCulture));
                sb.Append('}');
                count++;
            }
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

            var text = sb.ToString();
            int startIndex = text.IndexOf("[{\"", StringComparison.Ordinal);
            if (startIndex < 0 || text.Contains("\"captions\"", StringComparison.Ordinal))
            {
                return;
            }

            text = text.Substring(startIndex);
            foreach (string line in text.Replace("},{", Environment.NewLine).SplitToLines())
            {
                string s = line.Trim() + "}";
                string start = Json.ReadTag(s, "startTime");
                string duration = Json.ReadTag(s, "duration");
                string content = Json.ReadTag(s, "content");
                if (start != null && duration != null && content != null)
                {
                    double startSeconds;
                    double durationSeconds;
                    if (double.TryParse(start, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out startSeconds) &&
                        double.TryParse(duration, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out durationSeconds))
                    {
                        subtitle.Paragraphs.Add(new Paragraph(Json.DecodeJsonText(content), startSeconds, startSeconds + durationSeconds));
                    }
                    else
                    {
                        _errorCount++;
                    }
                }
                else
                {
                    _errorCount++;
                }
            }
            subtitle.Renumber();
        }

    }
}
