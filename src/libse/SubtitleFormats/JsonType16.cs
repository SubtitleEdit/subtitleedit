using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class JsonType16 : SubtitleFormat
    {
        public override string Extension => ".json";

        public override string Name => "JSON Type 16";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder(@"[");
            int count = 0;
            foreach (var p in subtitle.Paragraphs)
            {
                if (count > 0)
                {
                    sb.Append(',');
                }
                sb.AppendLine();
                sb.Append("{");
                sb.Append("\"endTime\": { \"f\": " + MillisecondsToFramesMaxFrameRate(p.EndTime.Milliseconds) + ", \"h\": " + p.EndTime.Hours + ", \"m\": " + p.EndTime.Minutes + ", \"s\": " + p.EndTime.Seconds + " },");
                sb.Append("\"id\":\"" + GenerateId() + "\",");
                sb.Append("\"isSequelToNextSubtitle\":false,");
                sb.Append("\"isSequelToPrevSubtitle\":false,");
                sb.Append("\"isStartingNewSmartText\":false,");
                sb.Append("\"speechOptions\":null,");
                sb.Append("\"startTime\": { \"f\": " + MillisecondsToFramesMaxFrameRate(p.StartTime.Milliseconds) + ", \"h\": " + p.StartTime.Hours + ", \"m\": " + p.StartTime.Minutes + ", \"s\": " + p.StartTime.Seconds + " },");
                sb.Append("\"text\":\"" + Json.EncodeJsonText(p.Text) + "\"");
                sb.Append("}");
                count++;
            }
            sb.AppendLine();
            sb.Append(']');
            return sb.ToString().Trim();
        }

        private static string GenerateId()
        {
            var s = Guid.NewGuid().ToString();
            return s.Remove(0, s.Length - 10);
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
            if (!text.StartsWith("[", StringComparison.Ordinal))
            {
                var tag = "\"subtitles\":";
                var startTag = text.IndexOf(tag, StringComparison.Ordinal);
                if (startTag >= 0)
                {
                    text = text.Remove(0, startTag + tag.Length).TrimStart();
                }
                if (!text.StartsWith("[", StringComparison.Ordinal))
                {
                    return;
                }
            }

            var parser = new SeJsonParser();
            var elements = parser.GetArrayElements(text);
            foreach (var element in elements)
            {
                var startTimeObject = parser.GetFirstObject(element, "startTime");
                var endTimeObject = parser.GetFirstObject(element, "endTime");
                var texts = parser.GetAllTagsByNameAsStrings(element, "text");
                if (texts.Count == 1 && !string.IsNullOrEmpty(startTimeObject) && !string.IsNullOrEmpty(endTimeObject))
                {
                    var startTime = GetTimeCode(parser, startTimeObject);
                    var endTime = GetTimeCode(parser, endTimeObject);
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

        private static double GetTimeCode(SeJsonParser parser, string timeObject)
        {
            var h = parser.GetAllTagsByNameAsStrings(timeObject, "h").FirstOrDefault();
            var m = parser.GetAllTagsByNameAsStrings(timeObject, "m").FirstOrDefault();
            var s = parser.GetAllTagsByNameAsStrings(timeObject, "s").FirstOrDefault();
            var f = parser.GetAllTagsByNameAsStrings(timeObject, "f").FirstOrDefault();
            if (h != null && m != null && s != null && f != null &&
                int.TryParse(h, NumberStyles.Integer, CultureInfo.InvariantCulture, out var hNumber) &&
                int.TryParse(m, NumberStyles.Integer, CultureInfo.InvariantCulture, out var mNumber) &&
                int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var sNumber) &&
                int.TryParse(f, NumberStyles.Integer, CultureInfo.InvariantCulture, out var fNumber))
            {
                var tc = new TimeCode(hNumber, mNumber, sNumber, FramesToMillisecondsMax999(fNumber));
                return tc.TotalMilliseconds;
            }
            return double.MinValue;
        }
    }
}
