using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class JsonAeneas : SubtitleFormat
    {
        public override string Extension => ".json";

        public override string Name => "JSON Aeneas";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            sb.AppendLine("{ \"fragments\": [");

            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                var number = (i + 1).ToString(CultureInfo.InvariantCulture).PadLeft(6, '0');
                Paragraph p = subtitle.Paragraphs[i];
                sb.AppendLine(@"    {
      ""begin"": """ + p.StartTime.TotalSeconds.ToString(CultureInfo.InvariantCulture) + @""",
      ""children"": [],
      ""end"": """ + p.EndTime.TotalSeconds.ToString(CultureInfo.InvariantCulture) + @""",
      ""id"": ""f" + number + @""",
      ""language"": ""eng"",
      ""lines"": [
        " + GetTextArray(p.Text) + @"
      ]
    },");
            }

            var result = sb.ToString().TrimEnd(',') + @"  ]
}";

            return result;
        }

        private string GetTextArray(string text)
        {
            var sb = new StringBuilder();
            foreach (var line in text.SplitToLines())
            {
                sb.Append("\"  " + Json.EncodeJsonText(line) + " \",");
            }

            return sb.ToString().TrimEnd(',');
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            var sb = new StringBuilder();
            foreach (var s in lines)
            {
                sb.Append(s);
            }

            var allText = sb.ToString();
            if (!allText.Contains("\"lines\""))
            {
                return;
            }

            var jp = new SeJsonParser();
            var fragments = jp.GetArrayElementsByName(allText, "fragments");
            for (var i = 0; i < fragments.Count; i++)
            {
                var fragment = fragments[i];
                var startTime = jp.GetFirstObject(fragment, "begin");
                var endTime = jp.GetFirstObject(fragment, "end");
                var texts = jp.GetArrayElementsByName(fragment, "lines");
                if (decimal.TryParse(startTime, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var startSeconds) &&
                    decimal.TryParse(endTime, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var endSeconds))
                {
                    var text = string.Join(Environment.NewLine, texts);
                    var p = new Paragraph(text, (double)startSeconds * 1000.0, (double)endSeconds * 1000.0);
                    subtitle.Paragraphs.Add(p);
                }
                else
                {
                    _errorCount++;
                }
            }

            subtitle.RemoveEmptyLines();
            subtitle.Renumber();
        }
    }
}
