using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// [{"id":13077087,"start_time":192665,"end_time":193887,"subtitle_language_code":"en","subtitle_id":27992065,"subtitle_content":"Hi."}]
    /// Times are in milliseconds. Not to be confused with JSON Type 8, which uses start_time/end_time in seconds with a "text" tag.
    /// </summary>
    public class JsonType24 : SubtitleFormat
    {
        public override string Extension => ".json";

        public override string Name => "JSON Type 24";

        public override bool IsMine(List<string> lines, string fileName)
        {
            var allText = JoinLinesTrimmed(lines);
            if (!allText.StartsWith('[') || !allText.Contains("\"subtitle_content\"", StringComparison.Ordinal))
            {
                return false;
            }

            var subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > 0 && _errorCount == 0;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder("[");
            for (var i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                var p = subtitle.Paragraphs[i];
                if (i > 0)
                {
                    sb.Append(',');
                }

                sb.Append("{\"id\":");
                sb.Append(p.Number.ToString(CultureInfo.InvariantCulture));
                sb.Append(",\"start_time\":");
                sb.Append(((long)Math.Round(p.StartTime.TotalMilliseconds)).ToString(CultureInfo.InvariantCulture));
                sb.Append(",\"end_time\":");
                sb.Append(((long)Math.Round(p.EndTime.TotalMilliseconds)).ToString(CultureInfo.InvariantCulture));
                sb.Append(",\"subtitle_id\":");
                sb.Append(p.Number.ToString(CultureInfo.InvariantCulture));
                sb.Append(",\"subtitle_content\":\"");
                sb.Append(Json.EncodeJsonText(p.Text, "\\n"));
                sb.Append("\"}");
            }

            sb.Append(']');
            return sb.ToString();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            var allText = JoinLinesTrimmed(lines);
            if (!allText.StartsWith('['))
            {
                return;
            }

            foreach (var item in Json.ReadObjectArray(allText))
            {
                var start = Json.ReadTag(item, "start_time");
                var end = Json.ReadTag(item, "end_time");
                var text = Json.ReadTag(item, "subtitle_content");
                if (start == null || end == null || text == null ||
                    !double.TryParse(start, NumberStyles.Float, CultureInfo.InvariantCulture, out var startMs) ||
                    !double.TryParse(end, NumberStyles.Float, CultureInfo.InvariantCulture, out var endMs))
                {
                    _errorCount++;
                    continue;
                }

                subtitle.Paragraphs.Add(new Paragraph(Json.DecodeJsonText(text), startMs, endMs));
            }

            subtitle.Renumber();
        }
    }
}
