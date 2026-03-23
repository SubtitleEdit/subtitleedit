using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{

    /// <summary>
    /// subtitle format from www.gremots.com
    /// </summary>
    public class Gremots : SubtitleFormat
    {
        public override string Extension => ".gst";

        public override string Name => "Gremots json";

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

                sb.Append("{\"st\":");
                sb.Append(p.StartTime.TotalMilliseconds.ToString(System.Globalization.CultureInfo.InvariantCulture));
                sb.Append(",\"et\":");
                sb.Append(p.EndTime.TotalMilliseconds.ToString(System.Globalization.CultureInfo.InvariantCulture));
                sb.Append(",\"tt\":\"");
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
                string start = Json.ReadTag(s, "st");
                string end = Json.ReadTag(s, "et");
                string text = Json.ReadTag(s, "tt");
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
