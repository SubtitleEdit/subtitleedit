using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class JsonType10 : SubtitleFormat
    {
        public override string Extension => ".json";

        public override string Name => "JSON Type 10";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder("{\"vostf\":[");
            int count = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                if (count > 0)
                {
                    sb.Append(',');
                }

                sb.Append("{\"startTime\":");
                sb.Append(p.StartTime.TotalSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture));
                sb.Append(",\"endTime\":");
                sb.Append(p.EndTime.TotalSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture));
                sb.Append(",\"positionAlign\":");
                sb.Append("\"middle\"");
                sb.Append(",\"lineAlign\":");
                sb.Append("\"start\"");
                sb.Append(",\"text\":\"");
                sb.Append(Json.EncodeJsonText(p.Text));
                sb.Append("\"}");
                count++;
            }
            sb.Append("]}");
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

            string allText = sb.ToString().Trim();
            if (!(allText.StartsWith("{", StringComparison.Ordinal) && allText.Contains("\"vostf\"", StringComparison.Ordinal)))
            {
                return;
            }

            foreach (string line in allText.Split('{', '}', '[', ']'))
            {
                string s = line.Trim();
                if (s.Length > 10)
                {
                    string start = Json.ReadTag(s, "startTime");
                    string end = Json.ReadTag(s, "endTime");
                    string text = Json.ReadTag(s, "text");
                    if (start != null && end != null && text != null)
                    {
                        if (double.TryParse(start, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out var startSeconds) &&
                            double.TryParse(end, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out var endSeconds))
                        {
                            subtitle.Paragraphs.Add(new Paragraph(Json.DecodeJsonText(text), startSeconds * TimeCode.BaseUnit, endSeconds * TimeCode.BaseUnit));
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
            }
            subtitle.Renumber();
        }
    }
}
