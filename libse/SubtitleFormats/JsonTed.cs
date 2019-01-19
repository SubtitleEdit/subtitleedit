using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class JsonTed : SubtitleFormat
    {
        public override string Extension => ".json";

        public override string Name => "JSON TED";

        public override string ToText(Subtitle subtitle, string title)
        {
            string languageCode;
            string languageDisplayName;
            try
            {
                languageCode = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);
                var ci = new CultureInfo(languageCode);
                languageDisplayName = ci.DisplayName;
            }
            catch
            {
                languageCode = "en";
                languageDisplayName = "English";
            }

            var sb = new StringBuilder("{\"subtitle\":[{\"captions\":[");
            int count = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                if (count > 0)
                {
                    sb.Append(',');
                }

                sb.Append("{\"duration\":");
                sb.Append(p.Duration.TotalMilliseconds.ToString(System.Globalization.CultureInfo.InvariantCulture));
                sb.Append(",\"content\":\"");
                sb.Append(Json.EncodeJsonText(p.Text) + "\"");
                sb.Append(",\"startOfParagraph\":true");
                sb.Append(",\"startTime\":");
                sb.Append(p.StartTime.TotalMilliseconds.ToString(System.Globalization.CultureInfo.InvariantCulture));
                sb.Append('}');
                count++;
            }
            sb.Append("],\"lang\":\"" + languageCode + "\", \"langDisplayName\":\"" + languageDisplayName + "\"}],\"introDuration\":0}");
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

            int startIndex = sb.ToString().IndexOf("\"captions\"", StringComparison.Ordinal);
            if (startIndex < 0)
            {
                return;
            }

            string text = sb.ToString().Substring(startIndex);
            foreach (string line in text.Replace("},{", Environment.NewLine).SplitToLines())
            {
                string s = line.Trim() + "}";
                string start = Json.ReadTag(s, "startTime");
                string duration = Json.ReadTag(s, "duration");
                string content = Json.ReadTag(s, "content");
                if (start != null && duration != null && content != null)
                {
                    double startSeconds;
                    double durationSeconds;
                    if (double.TryParse(start, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out startSeconds) &&
                        double.TryParse(duration, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out durationSeconds))
                    {
                        subtitle.Paragraphs.Add(new Paragraph(Json.DecodeJsonText(content), startSeconds, startSeconds + durationSeconds));
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
