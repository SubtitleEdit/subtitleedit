using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// Caption files downloaded from Wistia:
    /// {"captions":[{"bcp47LanguageTag":"en", ... ,"hash":{"lines":[{"start":0.17,"end":0.62,"text":["line 1","line 2"]}]}}]}
    /// Times are seconds, and "text" is an array with one entry per display line
    /// (older exports use a plain string instead).
    /// A file can hold several caption tracks (one per language) - like the converters
    /// people write for these files, only the first track is read.
    /// </summary>
    public class WistiaJson : SubtitleFormat
    {
        public override string Extension => ".json";

        public override string Name => "Wistia json";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine("  \"captions\": [");
            sb.AppendLine("    {");
            sb.AppendLine("      \"bcp47LanguageTag\": \"en\",");
            sb.AppendLine("      \"hasCaptions\": true,");
            sb.AppendLine("      \"hash\": {");
            sb.AppendLine("        \"lines\": [");
            for (var i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                var p = subtitle.Paragraphs[i];
                sb.AppendLine("          {");
                sb.AppendLine($"            \"start\": {p.StartTime.TotalSeconds.ToString(CultureInfo.InvariantCulture)},");
                sb.AppendLine($"            \"end\": {p.EndTime.TotalSeconds.ToString(CultureInfo.InvariantCulture)},");
                sb.AppendLine("            \"text\": [");
                var textLines = p.Text.SplitToLines();
                for (var j = 0; j < textLines.Count; j++)
                {
                    var comma = j == textLines.Count - 1 ? string.Empty : ",";
                    sb.AppendLine($"              \"{Json.EncodeJsonText(textLines[j])}\"{comma}");
                }

                sb.AppendLine("            ]");
                sb.AppendLine(i == subtitle.Paragraphs.Count - 1 ? "          }" : "          },");
            }

            sb.AppendLine("        ]");
            sb.AppendLine("      }");
            sb.AppendLine("    }");
            sb.AppendLine("  ]");
            sb.AppendLine("}");
            return sb.ToString();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            subtitle.Paragraphs.Clear();

            var sb = new StringBuilder();
            foreach (var line in lines)
            {
                sb.Append(line);
            }

            var text = sb.ToString().TrimStart();
            if (!text.StartsWith('{') ||
                !text.Contains("\"captions\"", StringComparison.Ordinal) ||
                !text.Contains("\"hash\"", StringComparison.Ordinal) ||
                !text.Contains("\"lines\"", StringComparison.Ordinal))
            {
                return;
            }

            var parser = new SeJsonParser();
            var captionTracks = parser.GetArrayElementsByName(text, "captions");
            if (captionTracks.Count == 0)
            {
                return;
            }

            foreach (var element in parser.GetArrayElementsByName(captionTracks[0], "lines"))
            {
                var startObject = parser.GetFirstObject(element, "start");
                var endObject = parser.GetFirstObject(element, "end");
                if (!double.TryParse(startObject, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var startSeconds) ||
                    !double.TryParse(endObject, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var endSeconds))
                {
                    _errorCount++;
                    continue;
                }

                var textLines = parser.GetArrayElementsByName(element, "text");
                var paragraphText = new StringBuilder();
                foreach (var textLine in textLines)
                {
                    if (paragraphText.Length > 0)
                    {
                        paragraphText.AppendLine();
                    }

                    paragraphText.Append(Json.DecodeJsonText(textLine));
                }

                subtitle.Paragraphs.Add(new Paragraph(paragraphText.ToString(), startSeconds * 1000.0, endSeconds * 1000.0));
            }

            subtitle.Renumber();
        }
    }
}
