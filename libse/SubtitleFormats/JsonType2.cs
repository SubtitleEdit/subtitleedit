using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class JsonType2 : SubtitleFormat
    {
        public override string Extension => ".json";

        public override string Name => "JSON Type 2";

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

                sb.Append("{\"startMillis\":");
                sb.Append(p.StartTime.TotalMilliseconds.ToString(System.Globalization.CultureInfo.InvariantCulture));
                sb.Append(",\"endMillis\":");
                sb.Append(p.EndTime.TotalMilliseconds.ToString(System.Globalization.CultureInfo.InvariantCulture));
                sb.Append(",\"text\":\"");
                sb.Append(Json.EncodeJsonText(p.Text));
                sb.Append("\"}");
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

            if (!sb.ToString().TrimStart().StartsWith("[{\"", StringComparison.Ordinal))
            {
                return;
            }

            foreach (string line in sb.ToString().Replace("},{", Environment.NewLine).SplitToLines())
            {
                string s = line.Trim() + "}";
                string start = Json.ReadTag(s, "startMillis");
                string end = Json.ReadTag(s, "endMillis");
                string text = Json.ReadTag(s, "text");
                if (start != null && end != null && text != null)
                {
                    double startSeconds;
                    double endSeconds;
                    if (double.TryParse(start, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out startSeconds) &&
                        double.TryParse(end, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out endSeconds))
                    {
                        subtitle.Paragraphs.Add(new Paragraph(Json.DecodeJsonText(text), startSeconds, endSeconds));
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
