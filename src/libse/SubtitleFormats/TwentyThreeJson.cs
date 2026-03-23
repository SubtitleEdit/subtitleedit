using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class TwentyThreeJson : SubtitleFormat
    {
        public override string Extension => ".json";

        public override string Name => "TwentyThree json";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            sb.AppendLine("{" + Environment.NewLine + "\t\"subtitles\": [");
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                if (i > 0)
                {
                    sb.AppendLine("\t,");
                }

                sb.AppendLine("\t{");
                sb.AppendLine("\t\t\"text\": [");
                Paragraph p = subtitle.Paragraphs[i];
                var lines = p.Text.SplitToLines();
                for (int j = 0; j < lines.Count; j++)
                {
                    sb.Append("\t\t");
                    sb.Append('"');
                    sb.Append(Json.EncodeJsonText(lines[j]));
                    sb.Append('"');
                    if (j < lines.Count - 1)
                    {
                        sb.Append(',');
                    }
                    sb.AppendLine();
                }
                sb.AppendLine("\t\t],");

                sb.AppendLine($"\t\t\"timestamp_begin\": {p.StartTime.TotalSeconds.ToString(CultureInfo.InvariantCulture)}, ");
                sb.AppendLine($"\t\t\"timestamp_end\": {p.EndTime.TotalSeconds.ToString(CultureInfo.InvariantCulture)}");
                sb.AppendLine("\t}");
            }

            sb.AppendLine("\t]");
            sb.Append("}");
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

            string allText = sb.ToString();
            var indexOfMainTag = allText.IndexOf("\"timestamp_begin\"", StringComparison.Ordinal);
            if (indexOfMainTag < 0)
            {
                return;
            }

            var indexOfSubtitles = allText.IndexOf("subtitles", StringComparison.Ordinal);
            if (indexOfSubtitles <= 0)
            {
                return;
            }

            var startText = allText.Substring(0, indexOfSubtitles);
            var startIndex = startText.LastIndexOf('{');
            if (startIndex < 0)
            {
                return;
            }

            var json = allText.Substring(startIndex);
            var parser = new SeJsonParser();
            var items = parser.GetArrayElementsByName(json, "subtitles");
            foreach (var item in items)
            {
                var textLines = parser.GetArrayElementsByName(item, "text");
                var beginTime = parser.GetAllTagsByNameAsStrings(item, "timestamp_begin");
                var endTime = parser.GetAllTagsByNameAsStrings(item, "timestamp_end");
                if (textLines.Count > 0 && beginTime.Count == 1 && endTime.Count == 1 &&
                    double.TryParse(beginTime[0], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var startSeconds) &&
                    double.TryParse(endTime[0], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var endSeconds))
                {
                    var text = new StringBuilder();
                    foreach (var line in textLines)
                    {
                        text.AppendLine(Json.DecodeJsonText(line));
                    }

                    var p = new Paragraph(text.ToString().Trim(), startSeconds * 1000.0, endSeconds * 1000.0);
                    subtitle.Paragraphs.Add(p);
                }
            }

            subtitle.RemoveEmptyLines();
            subtitle.Renumber();
        }
    }
}
