using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core
{
    public class UnknownFormatImporterJson
    {
        public Subtitle AutoGuessImport(List<string> lines)
        {

            var sb = new StringBuilder();
            foreach (string s in lines)
            {
                sb.Append(s);
            }

            var allText = sb.ToString().Trim();
            if (!allText.Contains("{", StringComparison.Ordinal))
            {
                return new Subtitle();
            }

            var subtitle1 = new Subtitle();
            try
            {
                int count = 0;
                foreach (string line in allText.Split('{', '}', '[', ']'))
                {
                    count++;
                    ReadParagraph(line, subtitle1);
                    if (count > 20 && subtitle1.Paragraphs.Count == 0)
                    {
                        break;
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            var subtitle2 = new Subtitle();
            try
            {
                int count = 0;
                foreach (string line in allText.Split('{', '}'))
                {
                    count++;
                    ReadParagraph(line, subtitle2);
                    if (count > 20 && subtitle2.Paragraphs.Count == 0)
                    {
                        break;
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            var subtitle3 = new Subtitle();
            try
            {
                int count = 0;
                foreach (var line in Json.ReadObjectArray(allText))
                {
                    count++;
                    ReadParagraph(line, subtitle3);
                    if (count > 20 && subtitle3.Paragraphs.Count == 0)
                    {
                        break;
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            if (subtitle1.Paragraphs.Count >= subtitle2.Paragraphs.Count && subtitle1.Paragraphs.Count >= subtitle3.Paragraphs.Count)
            {
                subtitle1.Renumber();
                return FixTimeCodeMsOrSeconds(subtitle1);
            }
            if (subtitle2.Paragraphs.Count >= subtitle1.Paragraphs.Count && subtitle2.Paragraphs.Count >= subtitle3.Paragraphs.Count)
            {
                subtitle2.Renumber();
                return FixTimeCodeMsOrSeconds(subtitle2);
            }
            subtitle3.Renumber();
            return FixTimeCodeMsOrSeconds(subtitle3);
        }

        private Subtitle FixTimeCodeMsOrSeconds(Subtitle subtitle)
        {
            if (subtitle == null || subtitle.Paragraphs.Count < 5)
            {
                return subtitle;
            }

            double totalDuration = 0;
            int msFound = 0;
            foreach (var p in subtitle.Paragraphs)
            {
                totalDuration += p.Duration.TotalMilliseconds;
                if (p.Style.Contains("\"startMs\"") ||
                    p.Style.Contains("\"start_ms\"") ||
                    p.Style.Contains("\"startMillis\"") ||
                    p.Style.Contains("\"start_millis\"") ||
                    p.Style.Contains("\"startMilliseconds\"") ||
                    p.Style.Contains("\"start_millisecondsMs\"") ||
                    p.Style.Contains("\"fromMs\"") ||
                    p.Style.Contains("\"from_ms\"") ||
                    p.Style.Contains("\"fromMillis\"") ||
                    p.Style.Contains("\"fromMilliseconds\"") ||
                    p.Style.Contains("\"from_milliseconds\""))
                {
                    msFound++;
                }
            }

            if (totalDuration / subtitle.Paragraphs.Count > 1000000 || msFound == subtitle.Paragraphs.Count)
            {
                // Time codes were read as seconds, but they are actually milliseconds,
                // so all time codes are divided by 1000.
                foreach (var p in subtitle.Paragraphs)
                {
                    p.StartTime.TotalMilliseconds = p.StartTime.TotalMilliseconds / TimeCode.BaseUnit;
                    p.EndTime.TotalMilliseconds = p.EndTime.TotalMilliseconds / TimeCode.BaseUnit;
                }
            }

            return new Subtitle(subtitle.Paragraphs);
        }

        private static void ReadParagraph(string s, Subtitle subtitle)
        {
            s = s.Trim();
            if (s.Length < 7)
            {
                return;
            }

            if (!s.EndsWith('}'))
            {
                s += '}';
            }

            var start = ReadStartTag(s);
            var end = ReadEndTag(s);
            var duration = ReadDurationTag(s);
            var text = ReadTextTag(s);
            var originalStart = start;

            if (start != null && start.Contains(":") && start.Length >= 11 && start.Length <= 12 && start.Split(new[] { ':', ',', '.' }, StringSplitOptions.RemoveEmptyEntries).Length == 4)
            {
                start = DecodeFormatToSeconds(start);
            }

            if (end != null && end.Contains(":") && end.Length >= 11 && end.Length <= 12 && end.Split(new[] { ':', ',', '.' }, StringSplitOptions.RemoveEmptyEntries).Length == 4)
            {
                end = DecodeFormatToSeconds(end);
            }

            if (start != null && end != null && text != null)
            {
                start = start.TrimEnd('s');
                end = end.TrimEnd('s');
                double startSeconds;
                double endSeconds;
                if (double.TryParse(start, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out startSeconds) &&
                    double.TryParse(end, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out endSeconds))
                {
                    var p = new Paragraph(Json.DecodeJsonText(text), startSeconds * TimeCode.BaseUnit, endSeconds * TimeCode.BaseUnit) { Extra = originalStart, Style = s };
                    subtitle.Paragraphs.Add(p);
                }
            }
            else if (start != null && duration != null && text != null)
            {
                start = start.TrimEnd('s');
                duration = duration.TrimEnd('s');
                double startSeconds;
                double durationSeconds;
                if (double.TryParse(start, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out startSeconds) &&
                    double.TryParse(duration, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out durationSeconds))
                {
                    var p = new Paragraph(Json.DecodeJsonText(text), startSeconds * TimeCode.BaseUnit, (startSeconds + durationSeconds) * TimeCode.BaseUnit) { Extra = originalStart, Style = s };
                    subtitle.Paragraphs.Add(p);
                }
            }
        }

        private static string DecodeFormatToSeconds(string s)
        {
            var ms = s.Length == 11 && s[8] == ':' ? TimeCode.ParseHHMMSSFFToMilliseconds(s) : TimeCode.ParseToMilliseconds(s);
            return (ms / TimeCode.BaseUnit).ToString(CultureInfo.InvariantCulture);
        }

        private static string ReadStartTag(string s)
        {
            return ReadFirstMultiTag(s, new[]
            {
                "start",
                "startTime", "start_time", "starttime",
                "startMillis", "start_Millis", "startmillis",
                "startMs", "start_ms", "startms",
                "startMilliseconds", "start_Millisesonds", "startmilliseconds",
                "from", "fromTime", "from_ms", "fromMilliseconds", "from_milliseconds"
            });
        }

        private static string ReadEndTag(string s)
        {
            return ReadFirstMultiTag(s, new[]
            {
                "end",
                "endTime", "end_time", "endtime",
                "endMillis", "end_Millis", "endmillis",
                "endMs", "end_ms", "startms",
                "endMilliseconds", "end_Millisesonds", "endmilliseconds",
                "to", "toTime", "to_ms", "toMilliseconds", "to_milliseconds"
            });
        }

        private static string ReadDurationTag(string s)
        {
            return ReadFirstMultiTag(s, new[]
            {
                "duration",
                "dur",
            });
        }

        private static string ReadTextTag(string s)
        {
            var idx = s.IndexOf("\"text", StringComparison.OrdinalIgnoreCase);
            if (idx < 0)
            {
                idx = s.IndexOf("\"content", StringComparison.OrdinalIgnoreCase);
            }

            if (idx < 0)
            {
                return null;
            }

            s = s.Substring(idx);
            idx = s.IndexOf(']');
            if (idx > 0)
            {
                s = s.Substring(0, idx + 1);
            }

            var text = Json.ReadTag(s, "text");
            if (text == null)
            {
                text = Json.ReadTag(s, "content");
            }

            var textLines = Json.ReadArray(s, "text");
            if (textLines == null || textLines.Count == 0)
            {
                textLines = Json.ReadArray(s, "content");
            }

            bool isArray = s.Contains("[");
            if (isArray && textLines.Any(p => p == "end_time" || p == "endTime" || p == "end" || p == "endMs" || p == "endMilliseconds" || p == "end_ms" || p == "to" || p == "to_ms" || p == "from" || p == "from_ms"))
            {
                isArray = false;
            }

            if (!isArray && !string.IsNullOrEmpty(text))
            {
                return text.Replace("&#039;", "'");
            }

            if (textLines != null && textLines.Count > 0)
            {
                return string.Join(Environment.NewLine, textLines);
            }

            return ReadFirstMultiTag(s, new[] { "text", "content" });
        }

        private static string ReadFirstMultiTag(string s, string[] tags)
        {
            foreach (var tag in tags)
            {
                var res = Json.ReadTag(s, tag);
                if (!string.IsNullOrEmpty(res))
                {
                    return res;
                }
            }
            return null;
        }

    }
}
