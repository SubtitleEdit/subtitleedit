using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class JsonType11 : SubtitleFormat
    {
        public override string Extension => ".json";

        public override string Name => "JSON Type 11";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder("[");
            int count = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                if (count > 0)
                {
                    sb.Append(',');
                }

                sb.Append("{\"milliseconds\":");
                sb.Append(Json.EncodeJsonText(p.StartTime.TotalMilliseconds.ToString(System.Globalization.CultureInfo.InvariantCulture)));
                sb.Append(",\"line\":\"");
                sb.Append(Json.EncodeJsonText(p.Text));
                sb.Append("\"}");
                count++;
            }
            sb.Append("]");
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
            if (!(allText.StartsWith("[", StringComparison.Ordinal) && allText.Contains("milliseconds", StringComparison.Ordinal)))
            {
                return;
            }

            foreach (string line in allText.Split('{', '}', '[', ']'))
            {
                string s = line.Trim();
                if (s.Length > 10)
                {
                    string start = Json.ReadTag(s, "milliseconds");
                    string text = Json.ReadTag(s, "line");
                    if (start != null && text != null)
                    {
                        if (double.TryParse(start, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out var startMilliseconds))
                        {
                            var duration = Utilities.GetOptimalDisplayMilliseconds(text);
                            var endMilliseconds = startMilliseconds + duration;
                            subtitle.Paragraphs.Add(new Paragraph(Json.DecodeJsonText(text), startMilliseconds, endMilliseconds));
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
