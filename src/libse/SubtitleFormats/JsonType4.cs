using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class JsonType4 : SubtitleFormat
    {
        public override string Extension => ".json";

        public override string Name => "JSON Type 4";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder(@"[");
            int count = 0;

            string guid = Guid.NewGuid().ToString();
            string segmentTypeId = Guid.NewGuid().ToString().RemoveChar('-').Substring(0, 24);

            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string id = Guid.NewGuid().ToString().RemoveChar('-').Substring(0, 24);
                if (count > 0)
                {
                    sb.Append(',');
                }

                sb.Append("{\"hitType\":\"tag\",\"subTrack\":null,\"tags\":[],\"track\":\"Closed Captioning\",\"startTime\":");
                sb.Append(p.StartTime.TotalSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture));
                sb.Append(",\"guid\":\"" + guid + "\",\"segmentTypeId\":\"" + segmentTypeId + "\",\"endTime\":");
                sb.Append(p.EndTime.TotalSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture));
                sb.Append(",\"id\":\"" + id + "\",\"metadata\":{\"Text\":\"");
                sb.Append(Json.EncodeJsonText(p.Text) + "\"");

                sb.Append(",\"ID\":\"\",\"Language\":\"en\"}}");
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

            int startIndex = sb.ToString().IndexOf("[{\"hitType", StringComparison.Ordinal);
            if (startIndex < 0)
            {
                return;
            }

            string text = sb.ToString().Substring(startIndex);
            foreach (string line in text.Replace("},{", Environment.NewLine).SplitToLines())
            {
                string s = line.Trim() + "}";
                string start = Json.ReadTag(s, "startTime");
                string end = Json.ReadTag(s, "endTime");
                string content = Json.ReadTag(s, "Text");
                if (start != null && end != null && content != null)
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
                else
                {
                    _errorCount++;
                }
            }
            subtitle.Renumber();
        }
    }
}
