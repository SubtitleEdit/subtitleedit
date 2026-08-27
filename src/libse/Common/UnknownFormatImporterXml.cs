using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.Common
{
    /// <summary>
    /// Generic importer for unknown XML subtitle dialects: finds the repeated element that
    /// carries time codes (as attributes or child elements) plus text, without knowing the
    /// schema. Handles TTML-like ("begin"/"end" attributes with inline text), wrapper
    /// elements with a text child (D-Cinema like), child-element time codes (Matroska
    /// chapters like), time codes on an empty child element, text in an attribute, and
    /// start-only samples where an empty element closes the previous one (GPAC TTXT like).
    /// </summary>
    public class UnknownFormatImporterXml
    {
        private class Candidate
        {
            public double StartMs { get; set; }
            public double? EndMs { get; set; }
            public double? DurationMs { get; set; }
            public string Text { get; set; }
        }

        private bool _use250TickTimeCodes;

        public Subtitle AutoGuessImport(List<string> lines)
        {
            var allText = string.Join(Environment.NewLine, lines).Trim().TrimStart('﻿').TrimStart();
            if (!allText.StartsWith('<') || !allText.EndsWith('>'))
            {
                return new Subtitle();
            }

            var doc = new XmlDocument { XmlResolver = null };
            try
            {
                var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore, XmlResolver = null };
                using (var stringReader = new System.IO.StringReader(allText))
                using (var reader = XmlReader.Create(stringReader, settings))
                {
                    doc.Load(reader);
                }
            }
            catch
            {
                return new Subtitle();
            }

            if (doc.DocumentElement == null)
            {
                return new Subtitle();
            }

            // D-Cinema interop writes the last time code part in 250ths of a second ("editable units")
            _use250TickTimeCodes = doc.DocumentElement.Name == "DCSubtitle";

            // group candidates per element name - the subtitle element is the name that
            // repeats with time codes, everything else (styles, regions, metadata) won't
            var candidatesByName = new Dictionary<string, List<Candidate>>();
            Walk(doc.DocumentElement, candidatesByName);

            List<Candidate> best = null;
            var bestScore = 0;
            var bestTextCount = 0;
            foreach (var kvp in candidatesByName)
            {
                var textCount = kvp.Value.Count(c => !string.IsNullOrWhiteSpace(c.Text));
                // a group that also knows when subtitles end beats one with bare start times
                // (e.g. a layout/config element with an index that parses as a time)
                var score = textCount * 2 + kvp.Value.Count(c => c.EndMs.HasValue || c.DurationMs.HasValue);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestTextCount = textCount;
                    best = kvp.Value;
                }
            }

            if (best == null || bestTextCount < 2)
            {
                return new Subtitle();
            }

            return MakeSubtitle(best);
        }

        private static Subtitle MakeSubtitle(List<Candidate> candidates)
        {
            var subtitle = new Subtitle();
            Paragraph last = null;
            foreach (var c in candidates)
            {
                var end = c.EndMs ?? (c.DurationMs.HasValue ? c.StartMs + c.DurationMs.Value : 0);
                if (string.IsNullOrWhiteSpace(c.Text))
                {
                    // start-only sample with no text: it closes the previous paragraph
                    if (last != null && !c.EndMs.HasValue && !c.DurationMs.HasValue &&
                        Math.Abs(last.EndTime.TotalMilliseconds) < 0.01 && c.StartMs > last.StartTime.TotalMilliseconds)
                    {
                        last.EndTime.TotalMilliseconds = c.StartMs;
                    }

                    continue;
                }

                var p = new Paragraph(c.Text.Trim(), c.StartMs, end);
                subtitle.Paragraphs.Add(p);
                last = p;
            }

            // fill in missing end times
            for (var i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                var p = subtitle.Paragraphs[i];
                if (Math.Abs(p.EndTime.TotalMilliseconds) > 0.01)
                {
                    continue;
                }

                var optimalDurationMs = Utilities.GetOptimalDisplayMilliseconds(p.Text);
                var next = subtitle.GetParagraphOrDefault(i + 1);
                if (next != null && next.StartTime.TotalMilliseconds < p.StartTime.TotalMilliseconds + optimalDurationMs + 2000)
                {
                    p.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                }
                else
                {
                    p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + optimalDurationMs;
                }
            }

            subtitle.Renumber();
            return subtitle;
        }

        private void Walk(XmlElement element, Dictionary<string, List<Candidate>> candidatesByName)
        {
            var candidate = TryGetCandidate(element);
            if (candidate != null)
            {
                if (!candidatesByName.TryGetValue(element.LocalName, out var list))
                {
                    list = new List<Candidate>();
                    candidatesByName.Add(element.LocalName, list);
                }

                list.Add(candidate);
            }

            foreach (XmlNode child in element.ChildNodes)
            {
                if (child is XmlElement childElement)
                {
                    Walk(childElement, candidatesByName);
                }
            }
        }

        private Candidate TryGetCandidate(XmlElement element)
        {
            double? start = null;
            double? end = null;
            double? duration = null;
            var timeChildNames = new List<string>();

            foreach (XmlAttribute attribute in element.Attributes)
            {
                ReadTimeField(attribute.LocalName, attribute.Value, ref start, ref end, ref duration);
            }

            foreach (XmlNode child in element.ChildNodes)
            {
                if (!(child is XmlElement el))
                {
                    continue;
                }

                var before = (start, end, duration);

                // simple child element whose value is a time code, e.g. <ChapterTimeStart>00:00:01.500</ChapterTimeStart>
                if (!el.HasChildNodes || el.ChildNodes.Count == 1 && el.FirstChild is XmlText)
                {
                    ReadTimeField(el.LocalName, el.InnerText, ref start, ref end, ref duration);
                }

                // empty child element carrying the time codes as attributes, e.g. <time start="..." end="..." />
                if (el.Attributes.Count > 0 && !el.HasChildNodes)
                {
                    foreach (XmlAttribute attribute in el.Attributes)
                    {
                        ReadTimeField(attribute.LocalName, attribute.Value, ref start, ref end, ref duration);
                    }
                }

                if (before != (start, end, duration))
                {
                    timeChildNames.Add(el.LocalName);
                }
            }

            if (!start.HasValue)
            {
                return null;
            }

            var text = GetText(element, timeChildNames);
            if (string.IsNullOrWhiteSpace(text) && !end.HasValue && !duration.HasValue)
            {
                // keep as an end marker only when it's a completely empty element
                // (GPAC TTXT like) - a start-time-only element with non-time children
                // is a container we shouldn't consume
                if (element.ChildNodes.Count > 0)
                {
                    return null;
                }
            }

            return new Candidate { StartMs = start.Value, EndMs = end, DurationMs = duration, Text = text };
        }

        private void ReadTimeField(string name, string value, ref double? start, ref double? end, ref double? duration)
        {
            var kind = ClassifyTimeName(name);
            if (kind == 0)
            {
                return;
            }

            var ms = TryParseTime(value, _use250TickTimeCodes);
            if (!ms.HasValue)
            {
                return;
            }

            if (kind == 1 && !start.HasValue)
            {
                start = ms;
            }
            else if (kind == 2 && !end.HasValue)
            {
                end = ms;
            }
            else if (kind == 3 && !duration.HasValue)
            {
                duration = ms;
            }
        }

        /// <returns>0 = not a time name, 1 = start, 2 = end, 3 = duration</returns>
        private static int ClassifyTimeName(string rawName)
        {
            var name = Normalize(rawName);
            if (name.Length == 0)
            {
                return 0;
            }

            if (name == "d" || name.Contains("dur", StringComparison.Ordinal))
            {
                return 3;
            }

            if (name == "out" ||
                name == "to" ||
                name == "hide" ||
                name == "et" ||
                name == "secout" ||
                name == "clear" ||
                name.Contains("end", StringComparison.Ordinal) ||
                name.Contains("stop", StringComparison.Ordinal) ||
                name.Contains("timeout", StringComparison.Ordinal) ||
                name.Contains("tcout", StringComparison.Ordinal))
            {
                return 2;
            }

            if (name == "in" ||
                name == "from" ||
                name == "show" ||
                name == "time" ||
                name == "timestamp" ||
                name == "offset" ||
                name == "t" ||
                name == "st" ||
                name == "secin" ||
                name == "display" ||
                name == "position" ||
                name == "value" ||
                name.Contains("start", StringComparison.Ordinal) ||
                name.Contains("begin", StringComparison.Ordinal) ||
                name.Contains("timein", StringComparison.Ordinal) ||
                name.Contains("tcin", StringComparison.Ordinal) ||
                name.Contains("sampletime", StringComparison.Ordinal))
            {
                return 1;
            }

            return 0;
        }

        private static string Normalize(string name)
        {
            var sb = new StringBuilder(name.Length);
            foreach (var ch in name)
            {
                if (char.IsLetterOrDigit(ch))
                {
                    sb.Append(char.ToLowerInvariant(ch));
                }
            }

            return sb.ToString();
        }

        internal static double? TryParseTime(string input, bool use250TickTimeCodes = false)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }

            var s = input.Trim();
            if (s.Length > 30 || s.Contains('%'))
            {
                return null;
            }

            // rational seconds, e.g. "3003/24000s" (Final Cut like)
            if (s.EndsWith('s') && s.Contains('/'))
            {
                var arr = s.TrimEnd('s').Split('/');
                if (arr.Length == 2 &&
                    double.TryParse(arr[0], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var numerator) &&
                    double.TryParse(arr[1], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var denominator) &&
                    denominator > 0)
                {
                    return numerator / denominator * TimeCode.BaseUnit;
                }

                return null;
            }

            // TTML ticks, e.g. "15000000t" (default tick rate is 10,000,000/s)
            if (s.EndsWith('t') && long.TryParse(s.TrimEnd('t'), NumberStyles.None, CultureInfo.InvariantCulture, out var ticks))
            {
                return ticks / 10000.0;
            }

            // "150ms"
            if (s.EndsWith("ms", StringComparison.Ordinal) &&
                double.TryParse(s.Remove(s.Length - 2), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var milliseconds))
            {
                return milliseconds;
            }

            // seconds with unit, e.g. "1.5s"
            if (s.EndsWith('s') &&
                double.TryParse(s.TrimEnd('s'), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var seconds))
            {
                return seconds * TimeCode.BaseUnit;
            }

            if (s.Contains(':'))
            {
                var parts = s.Split(':', '.', ',', ';');
                if (parts.Length < 2 || parts.Length > 4 || parts.Any(p => p.Length == 0 || p.Length > 9 || !p.All(char.IsDigit)))
                {
                    return null;
                }

                if (parts.Length == 2)
                {
                    // MM:SS
                    var m = int.Parse(parts[0]);
                    var sec = int.Parse(parts[1]);
                    if (sec >= 60)
                    {
                        return null;
                    }

                    return (m * 60 + sec) * TimeCode.BaseUnit;
                }

                int hours;
                int minutes;
                int secs;
                string last;
                if (parts.Length == 3)
                {
                    if (s.Contains('.') || s.Contains(','))
                    {
                        // MM:SS.mmm
                        hours = 0;
                        minutes = int.Parse(parts[0]);
                        secs = int.Parse(parts[1]);
                        last = parts[2];
                    }
                    else
                    {
                        // HH:MM:SS
                        hours = int.Parse(parts[0]);
                        minutes = int.Parse(parts[1]);
                        secs = int.Parse(parts[2]);
                        if (minutes >= 60 || secs >= 60 || hours > 99)
                        {
                            return null;
                        }

                        return new TimeCode(hours, minutes, secs, 0).TotalMilliseconds;
                    }
                }
                else
                {
                    hours = int.Parse(parts[0]);
                    minutes = int.Parse(parts[1]);
                    secs = int.Parse(parts[2]);
                    last = parts[3];
                }

                if (minutes >= 60 || secs >= 60 || hours > 99)
                {
                    return null;
                }

                double lastMs;
                if (last.Length <= 3 && use250TickTimeCodes)
                {
                    // D-Cinema interop: last part is in 4 ms units
                    lastMs = int.Parse(last) * 4;
                }
                else if (last.Length == 2 && !s.Contains('.') && !s.Contains(','))
                {
                    // HH:MM:SS:FF
                    lastMs = SubtitleFormat.FramesToMillisecondsMax999(int.Parse(last));
                }
                else
                {
                    // fraction of a second - "5", "50", "500", "500000000" all mean the same lead digits
                    lastMs = double.Parse("0." + last, CultureInfo.InvariantCulture) * TimeCode.BaseUnit;
                }

                return new TimeCode(hours, minutes, secs, 0).TotalMilliseconds + lastMs;
            }

            // plain number: integers are milliseconds, decimals are seconds
            if (double.TryParse(s, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var number))
            {
                if (s.Contains('.'))
                {
                    return number * TimeCode.BaseUnit;
                }

                return number;
            }

            return null;
        }

        private static readonly string[] TextElementNames = { "text", "string", "content", "caption", "dialog", "dialogue", "sentence", "line", "sub" };
        private static readonly string[] WeakTextElementNames = { "comment", "name", "label" };

        private static string GetText(XmlElement element, List<string> timeChildNames)
        {
            // inline text (TTML like) - text nodes plus inline markup like <br/> and <span>
            var sb = new StringBuilder();
            AppendInlineText(element, sb);
            var text = sb.ToString().Trim();
            if (!string.IsNullOrWhiteSpace(text))
            {
                return text;
            }

            // named text descendants (D-Cinema "Text", Matroska "ChapterString", ...) -
            // multi-line subtitles repeat the element, so join all of them
            var textElements = new List<XmlElement>();
            FindTextElements(element, timeChildNames, TextElementNames, textElements);
            if (textElements.Count == 0)
            {
                FindTextElements(element, timeChildNames, WeakTextElementNames, textElements);
            }

            if (textElements.Count > 0)
            {
                sb.Clear();
                foreach (var textElement in textElements)
                {
                    var lineSb = new StringBuilder();
                    AppendInlineText(textElement, lineSb);
                    var line = lineSb.ToString().Trim();
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        // text wrapped in unknown markup (styling runs etc.)
                        line = textElement.InnerText.Trim();
                    }

                    if (line.Length > 0)
                    {
                        if (sb.Length > 0)
                        {
                            sb.AppendLine();
                        }

                        sb.Append(line);
                    }
                }

                text = sb.ToString().Trim();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    return text;
                }
            }

            // text in an attribute, e.g. <Clip start="1.5" end="4.3" text="..." />
            foreach (XmlAttribute attribute in element.Attributes)
            {
                var name = Normalize(attribute.LocalName);
                foreach (var textName in TextElementNames)
                {
                    if (name.Contains(textName, StringComparison.Ordinal) && !string.IsNullOrWhiteSpace(attribute.Value))
                    {
                        return attribute.Value.Trim();
                    }
                }
            }

            return string.Empty;
        }

        private static void AppendInlineText(XmlElement element, StringBuilder sb)
        {
            foreach (XmlNode child in element.ChildNodes)
            {
                if (child is XmlText || child is XmlCDataSection || child is XmlWhitespace || child is XmlSignificantWhitespace)
                {
                    sb.Append(child.Value);
                }
                else if (child is XmlElement childElement)
                {
                    if (childElement.Name.Equals("br", StringComparison.OrdinalIgnoreCase))
                    {
                        sb.AppendLine();
                    }
                    else if (childElement.LocalName.Equals("span", StringComparison.OrdinalIgnoreCase) ||
                             childElement.LocalName.Equals("font", StringComparison.OrdinalIgnoreCase) ||
                             childElement.LocalName.Equals("i", StringComparison.OrdinalIgnoreCase) ||
                             childElement.LocalName.Equals("b", StringComparison.OrdinalIgnoreCase) ||
                             childElement.LocalName.Equals("u", StringComparison.OrdinalIgnoreCase))
                    {
                        AppendInlineText(childElement, sb);
                    }
                }
            }
        }

        private static void FindTextElements(XmlElement element, List<string> timeChildNames, string[] textNames, List<XmlElement> result)
        {
            foreach (XmlNode child in element.ChildNodes)
            {
                if (!(child is XmlElement childElement) || timeChildNames.Contains(childElement.LocalName))
                {
                    continue;
                }

                var name = Normalize(childElement.LocalName);
                var isTextElement = false;
                foreach (var textName in textNames)
                {
                    if (name.Contains(textName, StringComparison.Ordinal))
                    {
                        isTextElement = true;
                        break;
                    }
                }

                if (isTextElement)
                {
                    result.Add(childElement);
                }
                else
                {
                    FindTextElements(childElement, timeChildNames, textNames, result);
                }
            }
        }
    }
}
