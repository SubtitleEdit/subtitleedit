using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class SubUrbia : SubtitleFormat
    {
        public override string Extension
        {
            get { return ".json"; }
        }

        public override string Name
        {
            get { return "SubUrbia"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            var subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            sb.Append("{\"header\": { \"subtitleId\": \"" + Guid.NewGuid() + "\" },");
            sb.Append("\"subtitles\": [");

            int count = 0;
            foreach (var p in subtitle.Paragraphs)
            {
                if (count > 0)
                    sb.Append(',');
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
                sb.Append(s);
            string allLines = sb.ToString();
            if (!allLines.Contains("\"subtitles\":", StringComparison.Ordinal))
                return;

            foreach (string line in sb.ToString().Replace("},{", Environment.NewLine).SplitToLines())
            {
                string s = line.Trim() + "}";
                string start = Json.ReadTag(s, "start");
                string end = Json.ReadTag(s, "end");
                string text = Json.ReadTag(s, "text");
                string extra = Json.ReadTag(s, "comment");
                if (start != null && end != null && text != null)
                {
                    double startSeconds;
                    double endSeconds;
                    if (double.TryParse(start, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out startSeconds) &&
                        double.TryParse(end, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out endSeconds))
                    {
                        var p = new Paragraph(Json.DecodeJsonText(text), startSeconds, endSeconds);
                        if (!string.IsNullOrEmpty(extra))
                            p.Extra = extra;
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
