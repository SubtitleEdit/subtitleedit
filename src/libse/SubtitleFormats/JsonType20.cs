using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// Google Cloud Speech-to-text?
    /// </summary>
    public class JsonType20 : SubtitleFormat
    {
        public override string Extension => ".json";

        public override string Name => "JSON Type 20";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder(@"{
    'results': [
        {
            'alternatives': [
                {
                    'confidence': 1.0,
                    'transcript': '...', 
                    'words': [".Replace('\'', '"'));
            var count = 0;
            foreach (var p in subtitle.Paragraphs)
            {
                if (count > 0)
                {
                    sb.Append(',');
                }

                count++;
                sb.AppendLine();
                sb.AppendLine("                        {");
                sb.AppendLine($"                            \"startTime\": {p.StartTime.TotalSeconds.ToString(CultureInfo.InvariantCulture)},");
                sb.AppendLine($"                            \"endTime\": {p.EndTime.TotalSeconds.ToString(CultureInfo.InvariantCulture)},");
                sb.AppendLine($"                            \"word\": \"{Json.EncodeJsonText(p.Text, "\\n")}\"");
                sb.Append("                        }");
            }
            sb.Append(@"
                    ]
                } 
            ]
        }
    ]
}");
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            var sb = new StringBuilder();
            foreach (var s in lines)
            {
                sb.Append(s);
            }

            var text = sb.ToString().TrimStart();
            if (!text.Contains("\"alternatives\"", StringComparison.Ordinal) || !text.Contains("\"words\"", StringComparison.Ordinal))
            {
                return;
            }

            var parser = new SeJsonParser();
            var words = parser.GetArrayElementsByName(text, "words");
            foreach (var word in words)
            {
                var startTimeObject = parser.GetFirstObject(word, "startTime");
                var endTimeObject = parser.GetFirstObject(word, "endTime");
                var s = parser.GetFirstObject(word, "word");
                if (!string.IsNullOrEmpty(s) && !string.IsNullOrEmpty(startTimeObject) && !string.IsNullOrEmpty(endTimeObject))
                {
                    if (double.TryParse(startTimeObject.TrimEnd('s'), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var startSecs) &&
                        double.TryParse(endTimeObject.TrimEnd('s'), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var endSecs))
                    {
                        var pText = Json.DecodeJsonText(s);
                        var p = new Paragraph(pText, startSecs * 1000.0, endSecs * 1000.0);
                        subtitle.Paragraphs.Add(p);
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
