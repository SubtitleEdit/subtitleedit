using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class Bilibili : SubtitleFormat
    {
        public override string Extension => ".bcc";

        public override string Name => "Bilibili json";

        public override string ToText(Subtitle subtitle, string title)
        {
            var template = @"{
    'font_size': 0.4,
    'font_color': '#FFFFFF',
    'background_alpha': 0.5,
    'background_color': '#9C27B0',
    'Stroke': 'none',
    'body': [
      LINES
    ]
}".Replace('\'', '"');

            var sb = new StringBuilder();
            int count = 0;
            foreach (var p in subtitle.Paragraphs)
            {
                if (count > 0)
                {
                    sb.Append(',');
                }
                sb.AppendLine();
                sb.Append("\t{ ");
                sb.Append("\"from\":" + p.StartTime.TotalSeconds.ToString(CultureInfo.InvariantCulture) + ", ");
                sb.Append("\"to\":" + p.EndTime.TotalSeconds.ToString(CultureInfo.InvariantCulture) + ", ");
                sb.Append("\"location\": 2, ");
                sb.Append("\"content\":\"" + Json.EncodeJsonText(p.Text) + "\"");
                sb.Append(" }");
                count++;
            }
            return template.Replace("LINES", sb.ToString()).Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            var sb = new StringBuilder();
            foreach (string s in lines)
            {
                sb.Append(s);
            }

            var text = sb.ToString().TrimStart();
            if (!text.Contains("\"body\""))
            {
                return;
            }

            var parser = new SeJsonParser();
            var elements = parser.GetArrayElementsByName(text, "body");
            foreach (var element in elements)
            {
                var startTimeObject = parser.GetFirstObject(element, "from");
                var endTimeObject = parser.GetFirstObject(element, "to");
                var texts = parser.GetAllTagsByNameAsStrings(element, "content");
                if (texts.Count == 1 && !string.IsNullOrEmpty(startTimeObject) && !string.IsNullOrEmpty(endTimeObject))
                {
                    var startTime = GetTimeCode(startTimeObject);
                    var endTime = GetTimeCode(endTimeObject);
                    if (Math.Abs(startTime - double.MinValue) < 0.01 || Math.Abs(endTime - double.MinValue) < 0.01)
                    {
                        _errorCount++;
                    }
                    else
                    {
                        var p = new Paragraph(texts[0], startTime, endTime);
                        subtitle.Paragraphs.Add(p);
                    }
                }
            }
            subtitle.Renumber();
        }

        private static double GetTimeCode(string timeObject)
        {
            if (double.TryParse(timeObject, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var seconds))
            {
                var tc = new TimeCode(seconds * 1000.0);
                return tc.TotalMilliseconds;
            }
            return double.MinValue;
        }
    }
}
