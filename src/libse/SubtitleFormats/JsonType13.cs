using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class JsonType13 : SubtitleFormat
    {
        public override string Extension => ".json";

        public override string Name => "JSON Type 13";

        public override string ToText(Subtitle subtitle, string title)
        {
            var language = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);
            var userId = "0";
            var duration = "0";
            var last = subtitle.Paragraphs.LastOrDefault();
            if (last != null)
            {
                duration = (last.StartTime.TotalSeconds + last.Duration.TotalSeconds).ToString(CultureInfo.InvariantCulture);
            }

            var createdAt = "";
            var id = "0";
            var sb = new StringBuilder();
            sb.AppendLine("{" + Environment.NewLine +
                          "  \"job\": {" + Environment.NewLine +
                          "    \"lang\": \"" + language + "\"," + Environment.NewLine +
                          "    \"user_id\": \"" + userId + "\"," + Environment.NewLine +
                          "    \"name\": \"" + Json.EncodeJsonText(title) + "\"," + Environment.NewLine +
                          "    \"duration\": \"" + duration + "\"," + Environment.NewLine +
                          "    \"created_at\": \"" + createdAt + "\"," + Environment.NewLine +
                          "    \"id\": \"" + id + "\"," + Environment.NewLine +
                          "  }," + Environment.NewLine +
                          "  \"speakers\": [");

            int count = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                if (count > 0)
                {
                    sb.AppendLine(", ");
                }

                sb.AppendLine("  {");
                sb.AppendLine("    \"duration\": \"" + p.Duration.TotalSeconds.ToString(CultureInfo.InvariantCulture) + "\",");
                sb.AppendLine("    \"confidence\": null,");
                sb.AppendLine("    \"name\": \"" + Json.EncodeJsonText(p.Text) + "\",");
                sb.AppendLine("    \"time\": \"" + p.StartTime.TotalSeconds.ToString(CultureInfo.InvariantCulture) + "\"");
                sb.Append("  }");
                count++;
            }
            sb.AppendLine();
            sb.AppendLine("  ],");
            sb.AppendLine("  \"format\": \"1.0\"");
            sb.AppendLine("}");
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            subtitle.Paragraphs.Clear();

            var sb = new StringBuilder();
            foreach (string s in lines)
            {
                sb.AppendLine(s);
            }

            string allText = sb.ToString();
            var indxOfSpeakers = allText.IndexOf("speakers", StringComparison.Ordinal);
            if (indxOfSpeakers < 0)
            {
                return;
            }

            var idx = allText.IndexOf('[', indxOfSpeakers);
            if (idx == -1)
            {
                return;
            }

            allText = allText.Substring(idx);
            allText = allText.Substring(0, allText.LastIndexOf(']') + 1);

            var speakers = Json.ReadObjectArray(allText);
            foreach (var s in speakers)
            {
                var duration = Json.ReadTag(s, "duration");
                var start = Json.ReadTag(s, "time");
                var text = Json.ReadTag(s, "name");
                bool skip = false;
                if (!string.IsNullOrEmpty(duration) && !string.IsNullOrEmpty(start) && !string.IsNullOrEmpty(text) &&
                    double.TryParse(start, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double startSeconds) &&
                    double.TryParse(duration, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double durationSeconds))
                {
                    var startMilliseconds = TimeSpan.FromSeconds(startSeconds).TotalMilliseconds;
                    var p = new Paragraph(text, startMilliseconds, startMilliseconds + TimeSpan.FromSeconds(durationSeconds).Milliseconds);
                    if (p.Text != null && (p.Text == "." || p.Text == "?" || p.Text == "!"))
                    {
                        var last = subtitle.Paragraphs.LastOrDefault();
                        if (last != null && last.EndTime.TotalMilliseconds + 1000 > p.StartTime.TotalMilliseconds)
                        {
                            last.Text += p.Text;
                            last.EndTime.TotalMilliseconds = p.EndTime.TotalMilliseconds;
                            skip = true;
                        }
                    }
                    if (!skip)
                    {
                        subtitle.Paragraphs.Add(p);
                    }
                }
            }
            subtitle.Renumber();
        }

    }
}
