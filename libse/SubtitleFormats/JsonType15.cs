using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class JsonType15 : SubtitleFormat
    {
        public override string Extension => ".json";

        public override string Name => "JSON Type 15";

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

                sb.Append("{\"displayTimeOffset\":");
                sb.Append(p.StartTime.TotalSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture));
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
                string start = Json.ReadTag(s, "displayTimeOffset");
                string text = Json.ReadTag(s, "text");
                if (start != null && text != null)
                {
                    double startSeconds;
                    if (double.TryParse(start, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out startSeconds))
                    {
                        var endMs = Utilities.GetOptimalDisplayMilliseconds(text) + startSeconds * 1000.0;
                        subtitle.Paragraphs.Add(new Paragraph(Json.DecodeJsonText(text), startSeconds * 1000.0, endMs));
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

            for (int i = 0; i < subtitle.Paragraphs.Count - 1; i++)
            {
                var paragraph = subtitle.Paragraphs[i];
                var next = subtitle.GetParagraphOrDefault(i+1);
                if (next.StartTime.TotalMilliseconds < paragraph.EndTime.TotalMilliseconds)
                {
                    paragraph.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                }
            }
            subtitle.Renumber();
        }
    }
}
