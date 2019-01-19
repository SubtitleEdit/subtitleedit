using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class SubUrbia : SubtitleFormat
    {
        public override string Extension => ".json";

        public override string Name => "SubUrbia";

        public override string ToText(Subtitle subtitle, string title)
        {
            Guid subtitleId;
            if (subtitle.Header == null || !Guid.TryParse(subtitle.Header, out subtitleId))
            {
                subtitleId = Guid.NewGuid();
            }

            var sb = new StringBuilder();
            sb.Append("{\"header\": { \"subtitleId\": \"" + subtitleId + "\" },");
            sb.Append("\"subtitles\": [");

            int count = 0;
            foreach (var p in subtitle.Paragraphs)
            {
                if (count > 0)
                {
                    sb.Append(',');
                }

                sb.Append("{\"start\":");
                sb.Append(p.StartTime.TotalMilliseconds.ToString(System.Globalization.CultureInfo.InvariantCulture));
                sb.Append(",\"end\":");
                sb.Append(p.EndTime.TotalMilliseconds.ToString(System.Globalization.CultureInfo.InvariantCulture));

                sb.Append(",\"text\":\"");
                sb.Append(Json.EncodeJsonText(p.Text));
                sb.Append("\"}");

                if (!string.IsNullOrWhiteSpace(p.Extra))
                {
                    sb.Append(",\"comment\":\"");
                    sb.Append(Json.EncodeJsonText(p.Extra));
                    sb.Append("\"}");
                }

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

            string allLines = sb.ToString();
            if (!allLines.Contains("\"subtitles\":", StringComparison.Ordinal))
            {
                return;
            }

            foreach (string line in sb.ToString().Replace("},{", Environment.NewLine).SplitToLines())
            {
                string s = line.Trim() + "}";
                string start = Json.ReadTag(s, "start");
                string end = Json.ReadTag(s, "end");
                string text = Json.ReadTag(s, "text");
                string extra = Json.ReadTag(s, "comment");
                if (start != null && end != null && text != null)
                {
                    double startMilliseconds;
                    double endMilliseconds;
                    if (double.TryParse(start, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out startMilliseconds) &&
                        double.TryParse(end, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out endMilliseconds))
                    {
                        var p = new Paragraph(Json.DecodeJsonText(text), startMilliseconds, endMilliseconds);
                        if (!string.IsNullOrEmpty(extra))
                        {
                            p.Extra = extra;
                        }

                        subtitle.Paragraphs.Add(p);
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
