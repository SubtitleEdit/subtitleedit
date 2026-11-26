using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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
            "offset", "offsetMs", "startOffset", "showTime",
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
            "tEndMs", "timestamp_end", "endOffset", "hideTime",
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
            "line", "body", "subtitle_content", "word", "utf8", "tt", "t", "n"
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

            return ImportJsonText(sb.ToString().Trim(), unwrapEnvelope: true);
        }

        private static Subtitle ImportJsonText(string allText, bool unwrapEnvelope)
        {
            if (!allText.Contains("{", StringComparison.Ordinal))
            {
                return new Subtitle();
            }

            if (unwrapEnvelope && allText.StartsWith('{') && allText.Contains("\"data\"", StringComparison.Ordinal))
            {
                var embedded = ImportEmbeddedData(allText);
                if (embedded != null)
                {
                    return embedded;
                }
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

            var nested = ReadNestedClockCues(allText);
            if (nested.Paragraphs.Count >= 2 &&
                nested.Paragraphs.Count > Math.Max(subtitle1.Paragraphs.Count, Math.Max(subtitle2.Paragraphs.Count, subtitle3.Paragraphs.Count)))
            {
                nested.Renumber();
                return nested;
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

        private static Subtitle ImportEmbeddedData(string json)
        {
            try
            {
                if (!(new JsonParser().Parse(json) is IDictionary<string, object> envelope) ||
                    !envelope.TryGetValue("data", out var data))
                {
                    return null;
                }

                if (data is string embedded)
                {
                    embedded = embedded.Trim();
                    if (embedded.StartsWith('{') || embedded.StartsWith('['))
                    {
                        var subtitle = ImportJsonText(embedded, unwrapEnvelope: false);
                        return subtitle.Paragraphs.Count >= 2 ? subtitle : null;
                    }
                }
                else if (data is IDictionary<string, object> container &&
                         container.TryGetValue("websrt", out var value) && value is string srt)
                {
                    var subtitle = new Subtitle();
                    return new SubRip().TryLoadAndDetect(subtitle, srt.SplitToLines(), string.Empty, keep: true)
                        ? subtitle
                        : null;
                }
            }
            catch (ParserException)
            {
                // Leave malformed envelopes to the normal recovery heuristics.
            }

            return null;
        }

        private static Subtitle ReadNestedClockCues(string json)
        {
            var subtitle = new Subtitle();
            if (!json.Contains(':') || !json.Contains("instances", StringComparison.OrdinalIgnoreCase))
            {
                return subtitle;
            }

            object root;
            try
            {
                root = new JsonParser().Parse(json);
            }
            catch (ParserException)
            {
                return subtitle;
            }

            var nodes = new Stack<object>();
            nodes.Push(root);
            while (nodes.Count > 0)
            {
                var node = nodes.Pop();
                if (node is IList<object> array)
                {
                    for (var index = array.Count - 1; index >= 0; index--)
                    {
                        nodes.Push(array[index]);
                    }
                }
                else if (node is IDictionary<string, object> record)
                {
                    var text = TextTags.Select(tag => record.TryGetValue(tag, out var value) ? value as string : null)
                        .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
                    if (text != null && record.TryGetValue("instances", out var value) && value is IList<object> instances)
                    {
                        foreach (var instance in instances.OfType<IDictionary<string, object>>())
                        {
                            var start = ReadClock(instance, "start");
                            var end = ReadClock(instance, "end");
                            if (start >= 0 && end > start)
                            {
                                subtitle.Paragraphs.Add(new Paragraph(HtmlLineBreakRegex.Replace(text, Environment.NewLine), start.Value, end.Value));
                            }
                        }
                    }
                    else
                    {
                        foreach (var child in record.Values.Reverse())
                        {
                            nodes.Push(child);
                        }
                    }
                }
            }

            return subtitle;
        }

        private static double? ReadClock(IDictionary<string, object> record, string key)
        {
            return record.TryGetValue(key, out var value) && value is string time && time.Contains(':')
                ? UnknownFormatImporterXml.TryParseTime(time)
                : null;
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
            if (subtitle.Paragraphs.Count < 2)
            {
                return subtitle;
            }

            // Fill missing endpoints and clamp estimated display durations when required.
            for (var index = 0; index < subtitle.Paragraphs.Count; index++)
            {
                var paragraph = subtitle.Paragraphs[index];
                var next = subtitle.GetParagraphOrDefault(index + 1);
                if (paragraph.EndTime.TotalMilliseconds > 0)
                {
                    if (next != null && next.StartTime.TotalMilliseconds < paragraph.EndTime.TotalMilliseconds &&
                        ReadKeyValue(paragraph.Style, "displayTimeOffset", out _) != null)
                    {
                        paragraph.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                    }

                    continue;
                }

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
            if (text == null && start != null && duration != null && ReadKeyValue(s, "time", out _) != null)
            {
                var name = ReadKeyValue(s, "name", out var quotedName);
                if (quotedName)
                {
                    text = HtmlLineBreakRegex.Replace(name, Environment.NewLine);
                }
            }

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
                var milliseconds = ReadKeyValue(times.Source, "milliseconds", out _) != null;
                var displayOffset = ReadKeyValue(times.Source, "displayTimeOffset", out _) != null;
                var startMs = milliseconds ? startSeconds : startSeconds * TimeCode.BaseUnit;
                var sourceText = ReadKeyValue(times.Source, milliseconds ? "line" : "text", out var quotedText);
                // These start-only schemas use the stored display text for duration estimation.
                var endMs = milliseconds || displayOffset
                    ? startMs + Utilities.GetOptimalDisplayMilliseconds(quotedText ? sourceText : text)
                    : 0;
                subtitle.Paragraphs.Add(new Paragraph(Json.DecodeJsonText(text), startMs, endMs) { Extra = times.Start, Style = times.Source });
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

        private static readonly Regex HtmlLineBreakRegex =
            new Regex(@"[ \t]*<br[ \t]*/?>[ \t]*",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static string ReadTextTag(string s)
        {
            // text as an array of lines, e.g. "text": ["line 1", "line 2"]
            foreach (var arrayTag in new[] { "text", "content", "lines", "textLines" })
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
                    return HtmlLineBreakRegex.Replace(res, Environment.NewLine)
                        .Replace("&#039;", "'")
                        .Replace("<br \\/>", Environment.NewLine);
                }
            }

            return null;
        }
    }
}
