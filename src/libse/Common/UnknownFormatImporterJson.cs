using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Common
{
    public class UnknownFormatImporterJson
    {
        private static readonly string[] StartTags =
        {
            "start", "in", "begin",
            "startTime", "start_time", "starttime",
            "startMillis", "start_Millis", "startmillis",
            "startMs", "start_ms", "startms",
            "startMilliseconds", "start_millisesonds", "startmilliseconds",
            "from", "fromTime", "from_ms","fromms", "fromMilliseconds", "from_milliseconds", "show",
            "tStartMs", "displayTimeOffset", "milliseconds", "timestamp", "timestamp_begin", "time",
            "t1", "st", "s"
        };

        private static readonly string[] EndTags =
        {
            "end", "out", "stop",
            "endTime", "end_time", "endtime",
            "endMillis", "end_Millis", "endmillis",
            "endMs", "end_ms", "endms",
            "endMilliseconds", "end_millisesonds", "endmilliseconds",
            "to", "toTime", "to_ms", "toms", "toMilliseconds", "to_milliseconds", "hide",
            "tEndMs", "timestamp_end",
            "t2", "et", "e"
        };

        private static readonly string[] DurationTags =
        {
            "duration",
            "durationMs",
            "dDurationMs",
            "dur",
            "d",
        };

        private static readonly string[] TextTags =
        {
            "text", "content", "value", "caption", "sentence", "dialog", "dialogue",
            "line", "utf8", "tt", "t", "n"
        };

        private class Fields
        {
            public string Start { get; set; }
            public string End { get; set; }
            public string Duration { get; set; }
            public string Text { get; set; }
            public string Source { get; set; }

            public bool HasTimes => Start != null && (End != null || Duration != null);
        }

        public Subtitle AutoGuessImport(List<string> lines)
        {
            var sb = new StringBuilder();
            foreach (var s in lines)
            {
                sb.Append(s);
            }

            var allText = sb.ToString().Trim();
            if (!allText.Contains("{", StringComparison.Ordinal))
            {
                return new Subtitle();
            }

            var subtitle1 = ImportFromSegments(allText.Split('{', '}', '[', ']'));
            var subtitle2 = ImportFromSegments(allText.Split('{', '}'));

            var subtitle3 = new Subtitle();
            try
            {
                var count = 0;
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
            catch
            {
                // ignored
            }

            if (subtitle1.Paragraphs.Count >= subtitle2.Paragraphs.Count && subtitle1.Paragraphs.Count >= subtitle3.Paragraphs.Count)
            {
                subtitle1.Renumber();
                return FixTimeCodeMsOrSeconds(FixMissingEndTimes(subtitle1));
            }

            if (subtitle2.Paragraphs.Count >= subtitle1.Paragraphs.Count && subtitle2.Paragraphs.Count >= subtitle3.Paragraphs.Count)
            {
                subtitle2.Renumber();
                return FixTimeCodeMsOrSeconds(FixMissingEndTimes(subtitle2));
            }

            subtitle3.Renumber();
            return FixTimeCodeMsOrSeconds(FixMissingEndTimes(subtitle3));
        }

        private static Subtitle ImportFromSegments(string[] segments)
        {
            var subtitle = new Subtitle();
            try
            {
                var count = 0;
                Fields pendingTimes = null;
                var pendingAge = 0;
                foreach (var segment in segments)
                {
                    count++;
                    var fields = ReadFields(segment);
                    if (fields == null)
                    {
                        if (count > 20 && subtitle.Paragraphs.Count == 0)
                        {
                            break;
                        }

                        continue;
                    }

                    if (fields.Start != null && fields.Text != null)
                    {
                        AddParagraph(subtitle, fields, fields.Text);
                        pendingTimes = null;
                    }
                    else if (fields.HasTimes && fields.Text == null)
                    {
                        // nested object layout: the time codes and the text live in different
                        // segments after splitting (e.g. {"start":..,"end":..,"metadata":{"text":..}})
                        pendingTimes = fields;
                        pendingAge = 0;
                    }
                    else if (fields.Text != null && fields.Start == null && pendingTimes != null)
                    {
                        AddParagraph(subtitle, pendingTimes, fields.Text);
                        pendingTimes = null;
                    }

                    if (pendingTimes != null)
                    {
                        pendingAge++;
                        if (pendingAge > 3)
                        {
                            pendingTimes = null;
                        }
                    }

                    if (count > 20 && subtitle.Paragraphs.Count == 0)
                    {
                        break;
                    }
                }
            }
            catch
            {
                // ignored
            }

            return subtitle;
        }

        private static Subtitle FixMissingEndTimes(Subtitle subtitle)
        {
            if (subtitle.Paragraphs.Count < 2 ||
                !subtitle.Paragraphs.All(p => Math.Abs(p.EndTime.TotalMilliseconds) < 0.001))
            {
                return subtitle;
            }

            // start-time-only format (e.g. {"milliseconds":1500,"line":"..."})
            for (var index = 0; index < subtitle.Paragraphs.Count; index++)
            {
                var paragraph = subtitle.Paragraphs[index];
                var next = subtitle.GetParagraphOrDefault(index + 1);
                if (next != null)
                {
                    paragraph.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                }
                else
                {
                    paragraph.EndTime.TotalMilliseconds = paragraph.StartTime.TotalMilliseconds + Utilities.GetOptimalDisplayMilliseconds(paragraph.Text);
                }
            }

            return subtitle;
        }

        private static Subtitle FixTimeCodeMsOrSeconds(Subtitle subtitle)
        {
            if (subtitle == null || subtitle.Paragraphs.Count < 2)
            {
                return subtitle;
            }

            var msKeys = new[]
            {
                "start", "startMs", "start_ms", "startMillis", "start_millis",
                "startMilliseconds", "start_millisecondsMs", "tStartMs", "milliseconds",
                "fromMs", "from_ms", "fromms", "fromMillis", "fromMilliseconds", "from_milliseconds", "show"
            };

            double totalDuration = 0;
            var msFound = 0;
            foreach (var p in subtitle.Paragraphs)
            {
                totalDuration += p.DurationTotalMilliseconds;
                if (msKeys.Any(key => ReadKeyValue(p.Style, key, out _) != null))
                {
                    msFound++;
                }
            }

            var averageDuration = totalDuration / subtitle.Paragraphs.Count;
            if (averageDuration > 1000000 || msFound == subtitle.Paragraphs.Count && averageDuration > 100000)
            {
                // Time codes were read as seconds, but they are actually milliseconds,
                // so all time codes are divided by 1000.
                foreach (var p in subtitle.Paragraphs)
                {
                    p.StartTime.TotalMilliseconds /= TimeCode.BaseUnit;
                    p.EndTime.TotalMilliseconds /= TimeCode.BaseUnit;
                }
            }

            return new Subtitle(subtitle.Paragraphs);
        }

        private static void ReadParagraph(string s, Subtitle subtitle)
        {
            var fields = ReadFields(s);
            if (fields?.Start == null || fields.Text == null)
            {
                return;
            }

            AddParagraph(subtitle, fields, fields.Text);
        }

        private static Fields ReadFields(string s)
        {
            s = s.Trim();
            if (s.Length < 5)
            {
                return null;
            }

            if (!s.EndsWith('}'))
            {
                s += '}';
            }

            var start = NormalizeTime(ReadFirstTimeTag(s, StartTags));
            var end = NormalizeTime(ReadFirstTimeTag(s, EndTags));
            var duration = ReadFirstTimeTag(s, DurationTags);
            var text = ReadTextTag(s);
            if (start == null && end == null && duration == null && text == null)
            {
                return null;
            }

            return new Fields { Start = start, End = end, Duration = duration, Text = text, Source = s };
        }

        private static void AddParagraph(Subtitle subtitle, Fields times, string text)
        {
            var start = times.Start?.TrimEnd('s');
            var end = times.End?.TrimEnd('s');
            var duration = times.Duration?.TrimEnd('s');

            if (start == null)
            {
                return;
            }

            if (!double.TryParse(start, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var startSeconds))
            {
                return;
            }

            if (end != null &&
                double.TryParse(end, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var endSeconds))
            {
                subtitle.Paragraphs.Add(new Paragraph(Json.DecodeJsonText(text), startSeconds * TimeCode.BaseUnit, endSeconds * TimeCode.BaseUnit) { Extra = times.Start, Style = times.Source });
                return;
            }

            if (duration != null &&
                double.TryParse(duration, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var durationSeconds))
            {
                subtitle.Paragraphs.Add(new Paragraph(Json.DecodeJsonText(text), startSeconds * TimeCode.BaseUnit, (startSeconds + durationSeconds) * TimeCode.BaseUnit) { Extra = times.Start, Style = times.Source });
                return;
            }

            if (end == null && duration == null)
            {
                // start-time-only - end times are computed at the end of the import
                subtitle.Paragraphs.Add(new Paragraph(Json.DecodeJsonText(text), startSeconds * TimeCode.BaseUnit, 0) { Extra = times.Start, Style = times.Source });
            }
        }

        private static string NormalizeTime(string value)
        {
            if (value == null)
            {
                return null;
            }

            if (value.Contains(":") && value.Length >= 11 && value.Length <= 12 && value.Split(new[] { ':', ',', '.' }, StringSplitOptions.RemoveEmptyEntries).Length == 4)
            {
                return DecodeFormatToSeconds(value);
            }

            if (value.Contains(":") && value.Length == 8 && value.Split(new[] { ':', ',', '.' }, StringSplitOptions.RemoveEmptyEntries).Length == 3)
            {
                return DecodeFormatHourMinuteSecondsToSeconds(value);
            }

            return value;
        }

        private static string DecodeFormatToSeconds(string s)
        {
            var ms = s.Length == 11 && s[8] == ':' ? TimeCode.ParseHHMMSSFFToMilliseconds(s) : TimeCode.ParseToMilliseconds(s);
            return (ms / TimeCode.BaseUnit).ToString(CultureInfo.InvariantCulture);
        }

        private static string DecodeFormatHourMinuteSecondsToSeconds(string s)
        {
            return DecodeFormatToSeconds(s + ":00");
        }

        private static string ReadFirstTimeTag(string s, string[] tags)
        {
            foreach (var tag in tags)
            {
                var res = ReadKeyValue(s, tag, out var isStringValue);
                if (!string.IsNullOrEmpty(res) && IsTimeLikeValue(res))
                {
                    return res;
                }
            }

            return null;
        }

        private static bool IsTimeLikeValue(string value)
        {
            var v = value.Trim().TrimEnd('s');
            if (v.Length == 0 || v.Length > 20)
            {
                return false;
            }

            var hasDigit = false;
            foreach (var ch in v)
            {
                if (char.IsDigit(ch))
                {
                    hasDigit = true;
                }
                else if (ch != ':' && ch != '.' && ch != ',' && ch != ';' && ch != '-')
                {
                    return false;
                }
            }

            return hasDigit;
        }

        /// <summary>
        /// Reads a key whose value is an array of strings; null when the key is missing,
        /// the value is not an array, or the array holds anything but strings.
        /// </summary>
        private static List<string> ReadStringArrayValue(string s, string tag)
        {
            var from = 0;
            while (from < s.Length)
            {
                var idx = s.IndexOf("\"" + tag + "\"", from, StringComparison.OrdinalIgnoreCase);
                if (idx < 0)
                {
                    return null;
                }

                var i = idx + tag.Length + 2;
                while (i < s.Length && char.IsWhiteSpace(s[i]))
                {
                    i++;
                }

                if (i >= s.Length || s[i] != ':')
                {
                    from = idx + 1;
                    continue;
                }

                i++;
                while (i < s.Length && char.IsWhiteSpace(s[i]))
                {
                    i++;
                }

                if (i >= s.Length || s[i] != '[')
                {
                    return null;
                }

                i++;
                var list = new List<string>();
                while (i < s.Length)
                {
                    while (i < s.Length && (char.IsWhiteSpace(s[i]) || s[i] == ','))
                    {
                        i++;
                    }

                    if (i >= s.Length)
                    {
                        return null; // unterminated
                    }

                    if (s[i] == ']')
                    {
                        return list;
                    }

                    if (s[i] != '"')
                    {
                        return null; // not an array of strings
                    }

                    i++;
                    var sb = new StringBuilder();
                    while (i < s.Length && s[i] != '"')
                    {
                        if (s[i] == '\\' && i + 1 < s.Length)
                        {
                            sb.Append(s[i]);
                            sb.Append(s[i + 1]);
                            i += 2;
                            continue;
                        }

                        sb.Append(s[i]);
                        i++;
                    }

                    if (i >= s.Length)
                    {
                        return null;
                    }

                    i++; // closing quote
                    list.Add(sb.ToString());
                }

                return null;
            }

            return null;
        }

        /// <summary>
        /// Reads the value of a json key. Unlike a plain IndexOf, the tag must actually be in
        /// key position (followed by a colon) - otherwise string VALUES that happen to match a
        /// tag name hijack the lookup (e.g. "lineAlign":"start" matching the tag "start").
        /// </summary>
        private static string ReadKeyValue(string s, string tag, out bool isStringValue)
        {
            isStringValue = false;
            var from = 0;
            while (from < s.Length)
            {
                var idx = s.IndexOf("\"" + tag + "\"", from, StringComparison.OrdinalIgnoreCase);
                if (idx < 0)
                {
                    return null;
                }

                var i = idx + tag.Length + 2;
                while (i < s.Length && char.IsWhiteSpace(s[i]))
                {
                    i++;
                }

                if (i >= s.Length || s[i] != ':')
                {
                    from = idx + 1;
                    continue;
                }

                i++;
                while (i < s.Length && char.IsWhiteSpace(s[i]))
                {
                    i++;
                }

                if (i >= s.Length)
                {
                    return null;
                }

                if (s[i] == '"')
                {
                    isStringValue = true;
                    var sb = new StringBuilder();
                    i++;
                    while (i < s.Length)
                    {
                        var ch = s[i];
                        if (ch == '\\' && i + 1 < s.Length)
                        {
                            sb.Append(ch);
                            sb.Append(s[i + 1]);
                            i += 2;
                            continue;
                        }

                        if (ch == '"')
                        {
                            break;
                        }

                        sb.Append(ch);
                        i++;
                    }

                    return sb.ToString();
                }

                var end = i;
                while (end < s.Length && s[end] != ',' && s[end] != '}' && s[end] != ']')
                {
                    end++;
                }

                return s.Substring(i, end - i).Trim();
            }

            return null;
        }

        private static string ReadTextTag(string s)
        {
            // text as an array of lines, e.g. "text": ["line 1", "line 2"]
            foreach (var arrayTag in new[] { "text", "content", "lines" })
            {
                var textLines = ReadStringArrayValue(s, arrayTag);
                if (textLines != null && textLines.Count > 0)
                {
                    var joined = string.Join(Environment.NewLine, textLines.Where(p => !string.IsNullOrEmpty(p)));
                    if (!string.IsNullOrWhiteSpace(joined))
                    {
                        return joined;
                    }
                }
            }

            foreach (var tag in TextTags)
            {
                var res = ReadKeyValue(s, tag, out var isStringValue);
                if (isStringValue && !string.IsNullOrEmpty(res))
                {
                    return res
                        .Replace("&#039;", "'")
                        .Replace("<br />", Environment.NewLine)
                        .Replace("<br \\/>", Environment.NewLine);
                }
            }

            return null;
        }
    }
}
