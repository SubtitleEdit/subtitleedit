using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.Common
{
    /// <summary>
    /// Generic subtitle format parser
    /// </summary>
    public class UnknownFormatImporter
    {
        private static readonly char[] ExpectedSplitChars = { '.', ',', ';', ':' };
        private static readonly char[] BracketChars = { '-', '>', '{', '}', '[', ']' };

        // One pass instead of six chained Replace(char, char) calls, each of which allocates a
        // copy of the line whether or not the character occurs - run per line during auto-detect.
        private static string ReplaceBracketsWithSpaces(string line)
        {
            if (line.IndexOfAny(BracketChars) < 0)
            {
                return line;
            }

            var chars = line.ToCharArray();
            for (var i = 0; i < chars.Length; i++)
            {
                if (Array.IndexOf(BracketChars, chars[i]) >= 0)
                {
                    chars[i] = ' ';
                }
            }

            return new string(chars);
        }


        // Static: RegexOptions.Compiled emits IL on construction, so building these per call
        // paid the compile cost every time and never amortized it.
        private static readonly Regex NumbersRegex = new Regex(@"\d+", RegexOptions.Compiled);
        private static readonly Regex TimeCodeWithHoursRegex = new Regex(@"\d+[:.,;]{1}\d\d[:.,;]{1}\d\d[:.,;]{1}\d+", RegexOptions.Compiled);
        private static readonly Regex TimeCodeWithoutHoursRegex = new Regex(@"\d+[:.,;]{1}\d\d[:.,;]{1}\d+", RegexOptions.Compiled);
        private static readonly Regex SpaceSeparatedTimeCodeWithHoursRegex = new Regex(@"\d+ {1}\d\d {1}\d\d {1}\d+", RegexOptions.Compiled);
        private static readonly Regex SpaceSeparatedTimeCodeWithoutHoursRegex = new Regex(@"\d+  {1}\d\d {1}\d+", RegexOptions.Compiled);
        private static readonly Regex SubRipLikeTimeCodeRegex = new Regex(@"\G\d+ \d+:\d+:\d+[.,:;]\d+ --> \d+:\d+:\d+[.,:;]\d+\b", RegexOptions.Compiled); // e.g.: 1 00:00:01.502 --> 00:00:03.604
        private static readonly Regex LooseTimeCodeRegex = new Regex(@"\G(\d+: *)?\d+ *: *\d+[.,:;] *\d+ *-{0,3}> *(\d+: *)?\d+ *: *\d+[.,:;] *\d+\b", RegexOptions.Compiled); // e.g.: 1 00:00:01.502 --> 00:00:03.604

        private static readonly Regex WholeSecondRangeRegex = new Regex(
            @"^\s*(?<open>[{\[])?(?<start>[0-9]{1,5}(?::[0-9]{2}){1,2})" +
            @"(?(open)[}\]]\s*[{\[]|\s*(?:--?>|[-–—])\s*)" +
            @"(?<end>[0-9]{1,5}(?::[0-9]{2}){1,2})(?(open)[}\]]|(?![0-9:.,;]))\s*(?<text>.*)$",
            RegexOptions.Compiled);

        private static readonly Regex SlashLineBreakRegex = new Regex(@"\S*://\S*|/{2,3}", RegexOptions.Compiled);
        private static readonly Regex ColonHundredthsRegex = new Regex(@"^(?:[0-9]+:){3}[0-9]{2}$", RegexOptions.Compiled);
        private static readonly Regex DotFramePairRegex = new Regex(@"^\s*[0-9]+:[0-9]{2}:[0-9]{2}\.[0-9]{2}(?:\s*,\s*|\s+)[0-9]+:[0-9]{2}:[0-9]{2}\.[0-9]{2}(?:,|\s)", RegexOptions.Compiled);
        private static readonly Regex NumericCueIndexRegex = new Regex(@"^\s*[0-9]+\.\s*$", RegexOptions.Compiled);

        private static readonly Regex LabelledTimeInRegex = new Regex(@"^\s*(?:#\s*)?(?<numbered>SUBTITLE:?\s+[0-9]+\s+)?TIME\s*IN\s*:\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex LabelledTimeOutRegex = new Regex(@"^\s*(?:DURATION\s*:\s*(?<duration>[0-9]{1,2}:[0-9]{2})\s+)?TIME\s*OUT\s*:\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex ShortFrameDurationRegex = new Regex(@"^[0-9]{1,2}:[0-9]{2}$", RegexOptions.Compiled);
        private static readonly Regex NumberedDurationPrefixRegex = new Regex(@"^\s*[0-9]+(?:\s*-\s*[0-9]+|:)\s*$", RegexOptions.Compiled);

        private const string SplitClockPattern = @"[0-9]{1,3}:[0-9]{2}:[0-9]{2}[:;.,][0-9]{2,3}";
        private static readonly Regex SplitMarkerRegex = new Regex(
            @"^(?:(?<clock>" + SplitClockPattern + @")|(?<spaced>[0-9]{1,3} [0-9]{2} [0-9]{2} [0-9]{2})|(?:-->>\s*|/)(?<frames>[0-9]+))(?=\s|$)\s*(?<text>.*)$",
            RegexOptions.Compiled);
        private static readonly Regex TrailingRangeRegex = new Regex(
            @"^(?:#(?<start>" + SplitClockPattern + @"|[0-9]+),(?<end>" + SplitClockPattern + @"|[0-9]+)|<(?<start>" +
            SplitClockPattern + @")><(?<end>" + SplitClockPattern + @")>)$", RegexOptions.Compiled);

        private static readonly Regex SamiHeaderRegex = new Regex(@"^<SAMI(?:\s|>)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex WholeSecondStartRegex = new Regex(
            @"^(?<clock>[0-9]{1,3}:[0-9]{2}:[0-9]{2})(?<separator>:(?![0-9])[ \t]*|[ \t]+|$)(?<text>.*)$",
            RegexOptions.Compiled);

        private const string ReportClock = @"[0-9]{2}:[0-9]{2}:[0-9]{2}[:.][0-9]{2}";
        private static readonly Regex NumberedReportRegex = new Regex(
            @"^\s*[0-9]+(?:[):]\s*|\s+)(?<start>" + ReportClock + @")\s+(?<end>" + ReportClock +
            @")\s+(?:\[[0-9]+\]|[0-9]{2}:[0-9]{2}|Durée\s*:?\s*[0-9]{2}:[0-9]{2}\s+Lis.*\bNbc\s*:\s*[0-9]+|" +
            ReportClock + @"\s+#F\s+[0-9A-F]+\s+#C)\s*$", RegexOptions.Compiled);
        private static readonly Regex BracketedReportRegex = new Regex(
            @"^\s*SUB\s+\[[0-9]+\s+[A-Z]\s+(?<start>" + ReportClock + @")>(?<end>" + ReportClock + @")\]\s*$",
            RegexOptions.Compiled);
        private static readonly Regex CompactFrameRangeRegex = new Regex(
            @"^\s*(?<start>[0-9]{4}[.:][0-9]{2}|(?:[0-9]{2}\.){3}[0-9]{2})\s*[-–]\s*" +
            @"(?<end>[0-9]{4}[.:][0-9]{2}|(?:[0-9]{2}\.){3}[0-9]{2})(?:\s+(?<text>.*)|$)$", RegexOptions.Compiled);
        private static readonly Regex DecimalSecondsRangeRegex = new Regex(
            @"^\s*(?:\[(?<start>[0-9]+\.[0-9]{2,3})\s+(?<end>[0-9]+\.[0-9]{2,3})\]\s*(?<text>.*)|" +
            @"(?<start>[0-9]+\.[0-9]{2,3})\t+(?<end>[0-9]+\.[0-9]{2,3})\t*)$", RegexOptions.Compiled);

        private static readonly Regex DropFrameSccRowRegex = new Regex(@"^\s*[0-9]+:[0-9]{2}:[0-9]{2}[;,][0-9]{2}\t", RegexOptions.Compiled);
        private static readonly Regex IntegerCueRowRegex = new Regex(
            @"^\s*(?:\{(?<start>[0-9]+)\}\{(?<end>[0-9]+)\}(?<text>.+)|" +
            @"(?<start>[0-9]+)(?<milliseconds>\|\|)(?<end>[0-9]+)\|\|(?<text>.+)\|\||" +
            @"(?<start>[0-9]+)\s+(?<end>[0-9]+)\s+(?<text>(?<rtf>\{\\uc[0-9].*\})))\s*$",
            RegexOptions.Compiled);
        private static readonly Regex FrameCsvRowRegex = new Regex(@"^\s*[0-9]+\s*,\s*[0-9]+\s*,\s*""", RegexOptions.Compiled);

        public bool UseFrames { get; set; }

        public Subtitle AutoGuessImport(List<string> lines, string fileName)
        {
            // Word .doc/.docx: use the document text instead of the raw bytes. A known
            // format pasted into Word (SubRip, a time code list, ...) loads as that format;
            // anything else goes through the generic guessing below.
            if (WordDocumentReader.IsWordDocument(fileName))
            {
                var docLines = WordDocumentReader.GetLines(fileName);
                if (docLines == null || docLines.Count == 0)
                {
                    return new Subtitle();
                }

                var knownFormat = Subtitle.Parse(docLines, ".txt");
                if (knownFormat != null && knownFormat.Paragraphs.Count > 0)
                {
                    return knownFormat;
                }

                lines = docLines;
            }

            if (!string.IsNullOrEmpty(fileName) && fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                var xlsxSubtitle = new UnknownFormatImporterXlsx().AutoGuessImport(fileName);
                if (xlsxSubtitle != null && xlsxSubtitle.Paragraphs.Count > 0)
                {
                    return xlsxSubtitle;
                }
            }

            if (!string.IsNullOrEmpty(fileName) && fileName.EndsWith(".ods", StringComparison.OrdinalIgnoreCase))
            {
                var odsSubtitle = new UnknownFormatImporterOds().AutoGuessImport(fileName);
                if (odsSubtitle != null && odsSubtitle.Paragraphs.Count > 0)
                {
                    return odsSubtitle;
                }
            }

            if (!string.IsNullOrEmpty(fileName) && fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                var csvSubtitle = new UnknownFormatImporterCsv().AutoGuessImport(lines);
                if (csvSubtitle != null && csvSubtitle.Paragraphs.Count > 0)
                {
                    return csvSubtitle;
                }
            }

            var firstNonEmptyLine = lines.FirstOrDefault(l => !string.IsNullOrWhiteSpace(l));
            if (firstNonEmptyLine != null &&
                firstNonEmptyLine.TrimStart('﻿', ' ', '\t').StartsWith(@"{\rtf", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var documentText = RichTextToPlainText.ConvertToText(string.Join(Environment.NewLine, lines));
                    if (!string.IsNullOrWhiteSpace(documentText))
                    {
                        lines = documentText.SplitToLines();
                        firstNonEmptyLine = lines.FirstOrDefault(l => !string.IsNullOrWhiteSpace(l));
                    }
                }
                catch (Exception exception) when (exception is FormatException || exception is OverflowException)
                {
                    return new Subtitle();
                }
            }

            var structuredSubtitle = TryImportStructuredText(lines, fileName, firstNonEmptyLine);
            if (structuredSubtitle != null)
            {
                return structuredSubtitle;
            }

            if (firstNonEmptyLine != null && firstNonEmptyLine.TrimStart('﻿', ' ', '\t').StartsWith('<'))
            {
                var xmlSubtitle = new UnknownFormatImporterXml().AutoGuessImport(lines);
                if (xmlSubtitle.Paragraphs.Count >= 2)
                {
                    return xmlSubtitle;
                }
            }

            if (firstNonEmptyLine?.StartsWith("# SWIFT INTERCHANGE FILE", StringComparison.OrdinalIgnoreCase) == true)
            {
                lines = JoinLabelledTimeLines(lines);
            }

            var beginMarker = lines.FindIndex(line => line.Trim().Equals("<begin subtitles>", StringComparison.OrdinalIgnoreCase));
            if (beginMarker >= 0)
            {
                var endMarker = lines.FindIndex(beginMarker + 1, line => line.Trim().Equals("<end subtitles>", StringComparison.OrdinalIgnoreCase));
                if (endMarker > beginMarker)
                {
                    lines = lines.GetRange(beginMarker + 1, endMarker - beginMarker - 1);
                }
            }

            var unlabelledTable = ImportUnlabelledCueTable(lines, fileName);
            if (unlabelledTable != null)
            {
                return unlabelledTable;
            }

            lines = NormalizeTimeRanges(lines);

            var delimited = ImportTextAroundTimeMarkers(lines);
            if (delimited.Paragraphs.Count > 1)
            {
                CleanUp(delimited);
                if (delimited.Paragraphs.Count > 1 && lines.Count / delimited.Paragraphs.Count <= 25)
                {
                    delimited.Renumber();
                    return delimited;
                }
            }

            var subtitle = ImportWholeSecondRanges(lines);
            if (subtitle.Paragraphs.Count > 1)
            {
                CleanUp(subtitle);
                subtitle.Renumber();
                if (subtitle.Paragraphs.Count > 1 && lines.Count / subtitle.Paragraphs.Count <= 25)
                {
                    return subtitle;
                }
            }

            var wholeSecondStarts = ImportWholeSecondStarts(lines);
            if (wholeSecondStarts.Paragraphs.Count > 1)
            {
                return wholeSecondStarts;
            }

            subtitle = ImportTimeCodesOnSameSeparateLine(lines);
            if (subtitle.Paragraphs.Count < 2)
            {
                subtitle = ImportTimeCodesAndTextOnSameLineOnlySpaceAsSeparator(lines);
            }

            var subTcAndTextOnSameLine = ImportTimeCodesAndTextOnSameLine(lines);
            if (subTcAndTextOnSameLine.Paragraphs.Count > subtitle.Paragraphs.Count)
            {
                subtitle = subTcAndTextOnSameLine;
            }

            var subTcOnAloneLines = ImportTimeCodesOnAloneLines(lines);
            if (subTcOnAloneLines.Paragraphs.Count > subtitle.Paragraphs.Count)
            {
                subtitle = subTcOnAloneLines;
            }

            var jsonHeader = firstNonEmptyLine?.TrimStart('﻿', ' ', '\t');
            var isJson = IsJson(lines);
            var looksLikeJsonFile = !string.IsNullOrEmpty(jsonHeader) &&
                (jsonHeader.StartsWith('{') || jsonHeader.StartsWith('[')) &&
                fileName?.EndsWith(".json", StringComparison.OrdinalIgnoreCase) == true;

            if (subtitle.Paragraphs.Count < 2 && !isJson)
            {
                subtitle = ImportTimeCodesInFramesOnSameSeparateLine(lines);
                if (subtitle.Paragraphs.Count < 2)
                {
                    var sameLineSub = ImportTimeCodesInFramesAndTextOnSameLine(lines);
                    if (sameLineSub.Paragraphs.Count < 10 &&
                        (sameLineSub.Paragraphs.Count(p => p.DurationTotalMilliseconds < 0) > 2 ||
                         sameLineSub.Paragraphs.Count(p => p.Text.Length > 100) > 1))
                    {
                        // probably not a subtitle
                    }
                    else if (sameLineSub.Paragraphs.Count < 20 &&
                        (sameLineSub.Paragraphs.Count(p => p.DurationTotalMilliseconds < 0) > 8 ||
                         sameLineSub.Paragraphs.Count(p => p.Text.Length > 100) > 5))
                    {
                        // probably not a subtitle
                    }
                    else
                    {
                        subtitle = sameLineSub;
                    }
                }
            }

            if (subtitle.Paragraphs.Count > 1)
            {
                CleanUp(subtitle);
            }

            if (subtitle.Paragraphs.Count < 2 || isJson || looksLikeJsonFile)
            {
                var jsonSubtitle = new UnknownFormatImporterJson().AutoGuessImport(lines);
                if (jsonSubtitle != null && jsonSubtitle.Paragraphs.Count > 2)
                {
                    subtitle = jsonSubtitle;
                }
            }

            if (subtitle.Paragraphs.Count == 0 && lines.Count == 1 && lines[0].Contains(" --> "))
            {
                subtitle = ImportSubtitleWithNoLineBreaks(lines[0]);
            }

            if (subtitle.Paragraphs.Count < 10 || subtitle.Paragraphs.Average(p => p.Text.Length) > 100)
            {
                var text = string.Join(Environment.NewLine, lines);
                var noLineBreakSub = ImportSubtitleWithNoLineBreaksWithExtraSpaces(text);
                if (noLineBreakSub.Paragraphs.Count > subtitle.Paragraphs.Count * 1.5)
                {
                    subtitle = noLineBreakSub;
                }
            }

            if (subtitle.Paragraphs.Count > 0 && lines.Count > 0 && lines.Count / subtitle.Paragraphs.Count > 25)
            { // no more than 25 raw lines per subtitle lines
                return new Subtitle();
            }

            if (subtitle.Paragraphs.Count == 0 && !string.IsNullOrEmpty(fileName) && fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                var unknownCsvImporter = new UnknownFormatImporterCsv();
                return unknownCsvImporter.AutoGuessImport(lines);
            }

            return subtitle;
        }

        private Subtitle ImportUnlabelledCueTable(List<string> lines, string fileName)
        {
            var firstIndex = lines.FindIndex(line => !string.IsNullOrWhiteSpace(line));
            if (firstIndex < 0)
            {
                return null;
            }

            var first = lines[firstIndex].Trim();
            var isFrameCsv = FrameCsvRowRegex.IsMatch(first);
            if (isFrameCsv || (first.StartsWith('"') && fileName?.EndsWith(".csv", StringComparison.OrdinalIgnoreCase) == true))
            {
                return UnknownFormatImporterCsv.ImportUnlabelledRows(lines.Skip(firstIndex), isFrameCsv, UseFrames);
            }

            if (first.StartsWith('{'))
            {
                return null; // The existing frame parser handles ordinary brace-delimited rows.
            }

            // A bracketed metadata header may appear before a list of brace-delimited frames.
            var subtitle = new Subtitle();
            for (var index = firstIndex; index < lines.Count; index++)
            {
                var line = lines[index].Trim();
                if (line.Length == 0)
                {
                    continue;
                }

                var match = IntegerCueRowRegex.Match(line);
                if (!match.Success)
                {
                    if (index == firstIndex && line.StartsWith('[') && line.EndsWith(']'))
                    {
                        continue;
                    }

                    return null;
                }

                if (!int.TryParse(match.Groups["start"].Value, out var start) ||
                    !int.TryParse(match.Groups["end"].Value, out var end) || end <= start)
                {
                    return null;
                }

                var text = match.Groups["text"].Value.Trim();
                var isMilliseconds = match.Groups["milliseconds"].Success;
                if (isMilliseconds)
                {
                    text = text.Replace(@"\~", Environment.NewLine);
                }
                else if (match.Groups["rtf"].Success)
                {
                    try
                    {
                        text = RichTextToPlainText.ConvertToText(@"{\rtf1" + text + "}").Trim();
                    }
                    catch (Exception exception) when (exception is FormatException || exception is OverflowException)
                    {
                        return null;
                    }
                }
                else
                {
                    text = text.Replace("|", Environment.NewLine);
                }

                var startMs = !isMilliseconds && UseFrames ? SubtitleFormat.FramesToMilliseconds(start) : start;
                var endMs = !isMilliseconds && UseFrames ? SubtitleFormat.FramesToMilliseconds(end) : end;
                subtitle.Paragraphs.Add(new Paragraph(text, startMs, endMs));
            }

            if (subtitle.Paragraphs.Count < 2)
            {
                return null;
            }

            subtitle.Renumber();
            return subtitle;
        }

        private static List<string> NormalizeTimeRanges(List<string> lines)
        {
            var replacements = new List<(int Index, string Line)>();
            for (var index = 0; index < lines.Count; index++)
            {
                var line = lines[index];
                var match = NumberedReportRegex.Match(line);
                if (!match.Success)
                {
                    match = BracketedReportRegex.Match(line);
                }

                if (!match.Success)
                {
                    match = CompactFrameRangeRegex.Match(line);
                }

                if (match.Success)
                {
                    var start = ReadReportFrameTime(match.Groups["start"].Value);
                    var end = ReadReportFrameTime(match.Groups["end"].Value);
                    if (end > start)
                    {
                        replacements.Add((index, FormatRangeLine(start, end, match.Groups["text"].Value)));
                    }

                    continue;
                }

                match = DecimalSecondsRangeRegex.Match(line);
                if (match.Success)
                {
                    var start = UnknownFormatImporterXml.TryParseTime(match.Groups["start"].Value);
                    var end = UnknownFormatImporterXml.TryParseTime(match.Groups["end"].Value);
                    if (start.HasValue && end > start && end <= TimeCode.MaxSupportedTotalMilliseconds)
                    {
                        replacements.Add((index, FormatRangeLine(start.Value, end.Value, match.Groups["text"].Value)));
                    }
                }
            }

            if (replacements.Count < 2)
            {
                return lines;
            }

            var normalized = new List<string>(lines);
            foreach (var replacement in replacements)
            {
                normalized[replacement.Index] = replacement.Line;
            }

            return normalized;
        }

        private static double ReadReportFrameTime(string value)
        {
            var parts = value.Split('.', ':');
            if (parts.Length == 2)
            {
                return int.Parse(parts[0]) * TimeCode.BaseUnit + SubtitleFormat.FramesToMillisecondsMax999(int.Parse(parts[1]));
            }

            return DecodeTime(parts).TotalMilliseconds;
        }

        private static string FormatRangeLine(double start, double end, string text)
        {
            return (new TimeCode(start).ToString(false) + " --> " + new TimeCode(end).ToString(false) + " " + text).TrimEnd();
        }

        private static Subtitle TryImportStructuredText(List<string> lines, string fileName, string firstNonEmptyLine)
        {
            var header = firstNonEmptyLine?.Trim().TrimStart('﻿');
            if (string.IsNullOrEmpty(header))
            {
                return null;
            }

            // These signatures identify shared grammars with their own text escaping,
            // style information, and timing rules.
            SubtitleFormat format = null;
            if (SamiHeaderRegex.IsMatch(header))
            {
                format = lines.Exists(line => line.Contains("<AVDicPlayer", StringComparison.Ordinal))
                    ? (SubtitleFormat)new SamiAvDicPlayer()
                    : new SamiModern();
            }
            else if (header.Equals("Scenarist_SCC V1.0", StringComparison.OrdinalIgnoreCase))
            {
                format = lines.Exists(line => DropFrameSccRowRegex.IsMatch(line))
                    ? (SubtitleFormat)new ScenaristClosedCaptionsDropFrame()
                    : new ScenaristClosedCaptions();
            }
            else if (header.StartsWith("File Format=MacCaption_MCC ", StringComparison.OrdinalIgnoreCase))
            {
                format = new MacCaption10();
            }
            else if (header.Equals("[Script Info]", StringComparison.OrdinalIgnoreCase) && SubStationAlpha.HasEvents(lines))
            {
                format = lines.Exists(line => line.Trim().Equals("[V4+ Styles]", StringComparison.OrdinalIgnoreCase) ||
                    line.Trim().Equals("[V4++ Styles]", StringComparison.OrdinalIgnoreCase) ||
                    (line.StartsWith("ScriptType:", StringComparison.OrdinalIgnoreCase) && line.TrimEnd().EndsWith('+')))
                    ? (SubtitleFormat)new AdvancedSubStationAlpha()
                    : new SubStationAlpha();
            }

            if (format == null)
            {
                return null;
            }

            var subtitle = new Subtitle();
            format.LoadSubtitle(subtitle, lines, fileName);
            if (subtitle.Paragraphs.Count == 0)
            {
                return null;
            }

            subtitle.OriginalFormat = format;
            return subtitle;
        }

        private static Subtitle ImportWholeSecondStarts(List<string> lines)
        {
            var subtitle = new Subtitle();
            var text = new StringBuilder();
            Paragraph cue = null;
            var separator = string.Empty;
            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();
                if (line.Length == 0)
                {
                    continue;
                }

                var marker = WholeSecondStartRegex.Match(rawLine.TrimStart());
                if (marker.Success)
                {
                    var clock = marker.Groups["clock"].Value;
                    var start = UnknownFormatImporterXml.TryParseTime(clock);
                    var inlineText = marker.Groups["text"].Value.Trim();
                    var possibleEnd = TimeCodeWithHoursRegex.Match(inlineText);
                    if (start == null || (cue != null && start <= cue.StartTime.TotalMilliseconds) ||
                        inlineText.StartsWith("-->", StringComparison.Ordinal) ||
                        (possibleEnd.Success && IsOnlyTimeCodeSeparators(inlineText.Substring(0, possibleEnd.Index))))
                    {
                        return new Subtitle();
                    }

                    if (cue != null)
                    {
                        FinishWholeSecondCue(cue, text.ToString(), separator, start);
                        subtitle.Paragraphs.Add(cue);
                        text.Clear();
                    }

                    separator = marker.Groups["separator"].Value;
                    cue = new Paragraph(string.Empty, start.Value, 0);
                    text.Append(separator.StartsWith(":  ", StringComparison.Ordinal)
                        ? inlineText.Replace("  ", Environment.NewLine)
                        : inlineText);
                }
                else if (cue != null)
                {
                    if (text.Length > 0)
                    {
                        text.AppendLine();
                    }

                    text.Append(line);
                }
                else
                {
                    return new Subtitle();
                }
            }

            if (cue != null)
            {
                FinishWholeSecondCue(cue, text.ToString(), separator, null);
                subtitle.Paragraphs.Add(cue);
                CleanUp(subtitle);
                subtitle.Renumber();
            }

            return subtitle.Paragraphs.Count > 1 && lines.Count / subtitle.Paragraphs.Count <= 25 ? subtitle : new Subtitle();
        }

        private static void FinishWholeSecondCue(Paragraph cue, string text, string separator, double? nextStart)
        {
            cue.Text = text.Replace("|", Environment.NewLine);
            var optimal = cue.StartTime.TotalMilliseconds + Utilities.GetOptimalDisplayMilliseconds(cue.Text);
            if (separator.Length == 0 || separator.Contains('\t'))
            {
                cue.EndTime.TotalMilliseconds = nextStart.HasValue ? nextStart.Value - 1
                    : separator.Length == 0 ? cue.StartTime.TotalMilliseconds + 2500 : optimal;
            }
            else if (separator.StartsWith(":  ", StringComparison.Ordinal) && nextStart.HasValue)
            {
                var general = Configuration.Settings.General;
                cue.EndTime.TotalMilliseconds = Math.Min(nextStart.Value - general.MinimumMillisecondsBetweenLines,
                    cue.StartTime.TotalMilliseconds + general.SubtitleMaximumDisplayMilliseconds);
            }
            else
            {
                cue.EndTime.TotalMilliseconds = nextStart.HasValue ? Math.Min(optimal, nextStart.Value - 1) : optimal;
            }
        }

        private Subtitle ImportTextAroundTimeMarkers(List<string> lines)
        {
            var subtitle = new Subtitle();
            if (!lines.Exists(line => SplitMarkerRegex.IsMatch(line.Trim()) || TrailingRangeRegex.IsMatch(line.Trim())))
            {
                return subtitle;
            }

            var text = new StringBuilder();
            double? start = null;
            bool? timingFollowsText = null;
            bool? timingPrecedesText = null;
            double? pendingEnd = null;

            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();
                if (line.Length == 0 || (timingFollowsText == null && text.Length == 0 &&
                    line.Equals("#PE2 Format file", StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                var range = TrailingRangeRegex.Match(line);
                if (range.Success && timingFollowsText != false && start == null && text.Length > 0)
                {
                    timingFollowsText = true;
                    var dotMeansFrames = line.StartsWith('#');
                    var rangeStart = DecodeSplitMarker(range.Groups["start"].Value, dotMeansFrames);
                    var rangeEnd = DecodeSplitMarker(range.Groups["end"].Value, dotMeansFrames);
                    if (rangeStart == null || rangeEnd == null || rangeEnd <= rangeStart)
                    {
                        return new Subtitle();
                    }

                    subtitle.Paragraphs.Add(new Paragraph(text.ToString(), rangeStart.Value, rangeEnd.Value));
                    text.Clear();
                    continue;
                }

                var marker = SplitMarkerRegex.Match(line);
                if (marker.Success)
                {
                    if (pendingEnd.HasValue)
                    {
                        if (text.Length == 0)
                        {
                            return new Subtitle();
                        }

                        subtitle.Paragraphs.Add(new Paragraph(text.ToString(), start.Value, pendingEnd.Value));
                        start = null;
                        pendingEnd = null;
                        text.Clear();
                    }

                    var clock = marker.Groups["clock"];
                    var markerTime = clock.Success
                        ? DecodeSplitMarker(clock.Value, false)
                        : marker.Groups["spaced"].Success
                            ? DecodeSplitMarker(marker.Groups["spaced"].Value.Replace(' ', ':'), false)
                            : DecodeSplitMarker(marker.Groups["frames"].Value, false);
                    if (markerTime == null)
                    {
                        return new Subtitle();
                    }

                    timingFollowsText ??= text.Length > 0;
                    var inlineText = marker.Groups["text"].Value;
                    if (start == null)
                    {
                        if (timingFollowsText == true && (text.Length == 0 || inlineText.Length > 0))
                        {
                            return new Subtitle();
                        }

                        start = markerTime;
                        if (inlineText.Length > 0)
                        {
                            text.Append(SlashLineBreakRegex.Replace(inlineText,
                                match => match.Value[0] == '/' ? Environment.NewLine : match.Value));
                        }

                        continue;
                    }

                    if (markerTime <= start || inlineText.Length > 0)
                    {
                        return new Subtitle();
                    }

                    timingPrecedesText ??= text.Length == 0;
                    if (timingPrecedesText == true)
                    {
                        if (text.Length > 0 || timingFollowsText == true)
                        {
                            return new Subtitle();
                        }

                        pendingEnd = markerTime;
                        continue;
                    }

                    if (text.Length == 0)
                    {
                        return new Subtitle();
                    }

                    subtitle.Paragraphs.Add(new Paragraph(text.ToString(), start.Value, markerTime.Value));
                    text.Clear();
                    start = null;
                }
                else
                {
                    if ((timingFollowsText == false && start == null) ||
                        (timingFollowsText == true && start != null))
                    {
                        return new Subtitle();
                    }

                    if (text.Length > 0)
                    {
                        text.AppendLine();
                    }

                    text.Append(line);
                }
            }

            if (pendingEnd.HasValue && text.Length > 0)
            {
                subtitle.Paragraphs.Add(new Paragraph(text.ToString(), start.Value, pendingEnd.Value));
                return subtitle;
            }

            return start == null && text.Length == 0 ? subtitle : new Subtitle();
        }

        private double? DecodeSplitMarker(string value, bool dotMeansFrames)
        {
            if (int.TryParse(value, out var units))
            {
                return UseFrames ? SubtitleFormat.FramesToMilliseconds(units) : units;
            }

            return value.IndexOfAny(ExpectedSplitChars) < 0
                ? (double?)null
                : DecodeTime(SplitTimeCode(value, useDotFrames: dotMeansFrames)).TotalMilliseconds;
        }

        private static List<string> JoinLabelledTimeLines(List<string> lines)
        {
            var result = new List<string>();
            string start = null;
            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();
                if (line.StartsWith("# TIMEIN ", StringComparison.OrdinalIgnoreCase))
                {
                    start = line.Substring(9).Trim();
                }
                else if (start != null && line.StartsWith("# TIMEOUT ", StringComparison.OrdinalIgnoreCase))
                {
                    result.Add("TIMEIN: " + start + " TIMEOUT: " + line.Substring(10).Trim());
                    start = null;
                }
                else if (!line.StartsWith('#'))
                {
                    result.Add(rawLine);
                }
            }

            return result;
        }

        private static Subtitle ImportWholeSecondRanges(List<string> lines)
        {
            var subtitle = new Subtitle();
            Paragraph paragraph = null;
            var text = new StringBuilder();
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var match = WholeSecondRangeRegex.Match(line);
                if (match.Success)
                {
                    var start = UnknownFormatImporterXml.TryParseTime(match.Groups["start"].Value);
                    var end = UnknownFormatImporterXml.TryParseTime(match.Groups["end"].Value);
                    if (start.HasValue && end.HasValue && end > start)
                    {
                        if (paragraph != null)
                        {
                            paragraph.Text = text.ToString();
                        }

                        text.Clear();
                        text.Append(match.Groups["text"].Value);
                        paragraph = new Paragraph(string.Empty, start.Value, end.Value);
                        subtitle.Paragraphs.Add(paragraph);
                        continue;
                    }
                }

                if (paragraph == null)
                {
                    break;
                }

                if (text.Length > 0)
                {
                    text.AppendLine();
                }

                text.Append(line.Trim());
            }

            if (paragraph != null)
            {
                paragraph.Text = text.ToString();
            }

            return subtitle;
        }

        private static bool IsJson(List<string> lines)
        {
            var jp = new JsonParser();
            try
            {
                var sb = new StringBuilder();
                foreach (var line in lines)
                {
                    sb.AppendLine(line);
                }
                jp.Parse(sb.ToString());
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void CleanUp(Subtitle subtitle)
        {
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                p.Text = p.Text.Replace("<html>", string.Empty);
                p.Text = p.Text.Replace("</html>", string.Empty);
                p.Text = p.Text.Replace("<div>", string.Empty);
                p.Text = p.Text.Replace("</div>", string.Empty);
                p.Text = p.Text.Replace("<body>", string.Empty);
                p.Text = p.Text.Replace("</body>", string.Empty);
                p.Text = p.Text.Replace("<tt>", string.Empty);
                p.Text = p.Text.Replace("</tt>", string.Empty);
                p.Text = p.Text.Replace("<tr>", string.Empty);
                p.Text = p.Text.Replace("</tr>", string.Empty);
                p.Text = p.Text.Replace("<td>", string.Empty);
                p.Text = p.Text.Replace("</td>", string.Empty);
                p.Text = p.Text.Replace("<table>", string.Empty);
                p.Text = p.Text.Replace("</table>", string.Empty);
                p.Text = p.Text.Replace("<br>", Environment.NewLine);
                p.Text = p.Text.Replace("<br/>", Environment.NewLine);
                p.Text = p.Text.Replace("<br />", Environment.NewLine);
                if (p.Text.IndexOf('&') >= 0)
                {
                    // &amp; &quot; &#39; &nbsp; ... not only &lt; and &gt;
                    p.Text = WebUtility.HtmlDecode(p.Text).Replace(' ', ' ');
                }

                p.Text = p.Text.Replace("  ", " ");
                p.Text = p.Text.Replace("  ", " ");
                p.Text = p.Text.Replace("  ", " ");
                p.Text = p.Text.Replace(" | ", Environment.NewLine).Replace("|", Environment.NewLine).Replace("<p>", Environment.NewLine).Replace("</p>", Environment.NewLine).Trim();
                p.Text = p.Text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine).Trim();
                p.Text = p.Text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine).Trim();
            }
            subtitle.RemoveEmptyLines();
        }

        private Subtitle ImportTimeCodesInFramesAndTextOnSameLine(List<string> lines)
        {
            var regexTimeCodes1 = NumbersRegex;
            Paragraph p = null;
            var subtitle = new Subtitle();
            var sb = new StringBuilder();
            for (int idx = 0; idx < lines.Count; idx++)
            {
                string line = lines[idx];

                var matches = regexTimeCodes1.Matches(line);
                if (matches.Count >= 2)
                {
                    string start = matches[0].ToString();
                    string end = matches[1].ToString();

                    if (p != null)
                    {
                        p.Text = sb.ToString().Trim();
                        subtitle.Paragraphs.Add(p);
                    }
                    p = new Paragraph();
                    sb.Clear();
                    try
                    {
                        if (UseFrames)
                        {
                            p.StartTime.TotalMilliseconds = SubtitleFormat.FramesToMilliseconds(int.Parse(start));
                            p.EndTime.TotalMilliseconds = SubtitleFormat.FramesToMilliseconds(int.Parse(end));
                        }
                        else
                        {
                            p.StartTime.TotalMilliseconds = double.Parse(start);
                            p.EndTime.TotalMilliseconds = double.Parse(end);
                        }
                    }
                    catch
                    {
                        p = null;
                    }

                    if (matches[0].Index < 9)
                    {
                        line = line.Remove(0, matches[0].Index);
                    }

                    line = line.Replace(matches[0].ToString(), string.Empty);
                    line = line.Replace(matches[1].ToString(), string.Empty);
                    line = line.Trim();
                    if (line.StartsWith("}{}", StringComparison.Ordinal) || line.StartsWith("][]", StringComparison.Ordinal))
                    {
                        line = line.Remove(0, 3);
                    }

                    line = line.Trim();
                }
                if (p != null && line.Length > 1)
                {
                    sb.AppendLine(line.Trim());
                    if (sb.Length > 200)
                    {
                        return new Subtitle();
                    }
                }
            }
            if (p != null)
            {
                p.Text = sb.ToString().Trim();
                subtitle.Paragraphs.Add(p);
            }
            subtitle.Renumber();
            return subtitle;
        }

        private Subtitle ImportTimeCodesInFramesOnSameSeparateLine(List<string> lines)
        {
            Paragraph p = null;
            var subtitle = new Subtitle();
            var sb = new StringBuilder();
            for (int idx = 0; idx < lines.Count; idx++)
            {
                string line = lines[idx];
                string lineWithPerhapsOnlyNumbers = GetLineWithPerhapsOnlyNumbers(line);
                bool allNumbers = lineWithPerhapsOnlyNumbers.Length > 0;
                foreach (char c in lineWithPerhapsOnlyNumbers)
                {
                    if (!char.IsDigit(c))
                    {
                        allNumbers = false;
                    }
                }
                if (allNumbers && lineWithPerhapsOnlyNumbers.Length > 2)
                {
                    string[] arr = ReplaceBracketsWithSpaces(line).Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (arr.Length == 2)
                    {
                        string[] start = arr[0].Trim().Split(ExpectedSplitChars, StringSplitOptions.RemoveEmptyEntries);
                        string[] end = arr[1].Trim().Split(ExpectedSplitChars, StringSplitOptions.RemoveEmptyEntries);
                        if (start.Length == 1 && end.Length == 1)
                        {
                            if (p != null)
                            {
                                p.Text = sb.ToString().Trim();
                                subtitle.Paragraphs.Add(p);
                            }
                            p = new Paragraph();
                            sb.Clear();
                            try
                            {
                                if (UseFrames)
                                {
                                    p.StartTime.TotalMilliseconds = SubtitleFormat.FramesToMilliseconds(int.Parse(start[0]));
                                    p.EndTime.TotalMilliseconds = SubtitleFormat.FramesToMilliseconds(int.Parse(end[0]));
                                }
                                else
                                {
                                    p.StartTime.TotalMilliseconds = double.Parse(start[0]);
                                    p.EndTime.TotalMilliseconds = double.Parse(end[0]);
                                }
                            }
                            catch
                            {
                                p = null;
                            }
                        }
                    }
                    else if (arr.Length == 3)
                    {
                        string[] start = arr[0].Trim().Split(ExpectedSplitChars, StringSplitOptions.RemoveEmptyEntries);
                        string[] end = arr[1].Trim().Split(ExpectedSplitChars, StringSplitOptions.RemoveEmptyEntries);
                        string[] duration = arr[2].Trim().Split(ExpectedSplitChars, StringSplitOptions.RemoveEmptyEntries);

                        if (end.Length == 1 && duration.Length == 1)
                        {
                            start = end;
                            end = duration;
                        }

                        if (start.Length == 1 && end.Length == 1)
                        {
                            if (p != null)
                            {
                                p.Text = sb.ToString().Trim();
                                subtitle.Paragraphs.Add(p);
                            }
                            p = new Paragraph();
                            sb.Clear();
                            try
                            {
                                if (UseFrames)
                                {
                                    p.StartTime.TotalMilliseconds = SubtitleFormat.FramesToMilliseconds(int.Parse(start[0]));
                                    p.EndTime.TotalMilliseconds = SubtitleFormat.FramesToMilliseconds(int.Parse(end[0]));
                                }
                                else
                                {
                                    p.StartTime.TotalMilliseconds = double.Parse(start[0]);
                                    p.EndTime.TotalMilliseconds = double.Parse(end[0]);
                                }
                            }
                            catch
                            {
                                p = null;
                            }
                        }
                    }
                }
                if (p != null && !allNumbers && line.Length > 1)
                {
                    line = line.Trim();
                    if (line.StartsWith("}{}", StringComparison.Ordinal) || line.StartsWith("][]", StringComparison.Ordinal))
                    {
                        line = line.Remove(0, 3);
                    }

                    sb.AppendLine(line.Trim());
                }
            }
            if (p != null)
            {
                p.Text = sb.ToString().Trim();
                subtitle.Paragraphs.Add(p);
            }
            subtitle.Renumber();
            return subtitle;
        }

        private static Subtitle ImportTimeCodesOnAloneLines(List<string> lines)
        {
            Paragraph p = null;
            var subtitle = new Subtitle();
            var sb = new StringBuilder();
            char[] splitChars = { ' ', '\t', '-', '>', '<', '{', '}', '[', ']' };
            for (int idx = 0; idx < lines.Count; idx++)
            {
                string line = lines[idx];
                string lineWithPerhapsOnlyNumbers = GetLineWithPerhapsOnlyNumbers(line);
                bool allNumbers = lineWithPerhapsOnlyNumbers.Length > 0;
                foreach (char c in lineWithPerhapsOnlyNumbers)
                {
                    if (!char.IsDigit(c))
                    {
                        allNumbers = false;
                        break;
                    }
                }
                if (allNumbers && lineWithPerhapsOnlyNumbers.Length > 5)
                {
                    string[] arr = line.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                    if (arr.Length == 1)
                    {
                        string[] tc = SplitTimeCode(arr[0].Trim());
                        if (p == null || Math.Abs(p.EndTime.TotalMilliseconds) > 0.001)
                        {
                            if (p != null)
                            {
                                p.Text = sb.ToString().Trim();
                                subtitle.Paragraphs.Add(p);
                                sb.Clear();
                            }
                            p = new Paragraph { StartTime = DecodeTime(tc) };
                        }
                        else
                        {
                            p.EndTime = DecodeTime(tc);
                        }
                    }
                }
                if (p != null && !allNumbers && line.Length > 1)
                {
                    line = line.Trim();
                    if (line.StartsWith("}{}", StringComparison.Ordinal) || line.StartsWith("][]", StringComparison.Ordinal))
                    {
                        line = line.Remove(0, 3);
                    }

                    sb.AppendLine(line.Trim());
                }
            }
            if (p != null)
            {
                p.Text = sb.ToString().Trim();
                subtitle.Paragraphs.Add(p);
            }
            subtitle.Renumber();
            return subtitle;
        }

        private static Subtitle ImportTimeCodesAndTextOnSameLine(List<string> lines)
        {
            var regexTimeCodes1 = TimeCodeWithHoursRegex;
            var regexTimeCodes2 = TimeCodeWithoutHoursRegex;
            Paragraph p = null;
            var subtitle = new Subtitle();
            var sb = new StringBuilder();

            // A two-digit colon field can be hundredths when it exceeds the frame counter.
            var hundredthsThreshold = Math.Max(50, Math.Ceiling(Configuration.Settings.General.CurrentFrameRate));
            var useColonHundredths = lines.Any(line =>
            {
                var times = regexTimeCodes1.Matches(line);
                return times.Count >= 2 &&
                       ColonHundredthsRegex.IsMatch(times[0].Value) &&
                       ColonHundredthsRegex.IsMatch(times[1].Value) &&
                       (int.Parse(times[0].Value.Substring(times[0].Length - 2)) >= hundredthsThreshold ||
                        int.Parse(times[1].Value.Substring(times[1].Length - 2)) >= hundredthsThreshold);
            });
            // A pair of dot-separated two-digit fields with slash-coded dialogue is a
            // common frame-based export. Other dot-separated fractions remain decimal.
            var useDotFrames = lines.Any(line => DotFramePairRegex.IsMatch(line) &&
                SlashLineBreakRegex.Matches(line).Cast<Match>().Any(m => m.Value.StartsWith("//", StringComparison.Ordinal)));

            var hasAlignmentHeader = lines.TakeWhile(line => !regexTimeCodes1.IsMatch(line))
                .Any(line => IsAlignmentDirective(line));
            var isFirstLineNumber = false;
            var hasOnlyOneTimeCode = true;
            int count = -1;
            for (int idx = 0; idx < lines.Count; idx++)
            {
                string line = lines[idx];
                var matches = regexTimeCodes1.Matches(line);
                if (matches.Count == 0)
                {
                    matches = regexTimeCodes2.Matches(line);
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        hasOnlyOneTimeCode = false;
                    }
                }

                if (matches.Count >= 2)
                {
                    hasOnlyOneTimeCode = false;
                }

                if (matches.Count == 2)
                {
                    var start = matches[0].Value.Split(ExpectedSplitChars, StringSplitOptions.RemoveEmptyEntries);
                    if (int.TryParse(start[0], out var i))
                    {
                        if (count == -1 && i < 2)
                        {
                            count = i;
                        }

                        if (count != i)
                        {
                            isFirstLineNumber = false;
                            break;
                        }
                        count++;
                    }
                }
                if (count > 2)
                {
                    isFirstLineNumber = true;
                }
            }

            for (int idx = 0; idx < lines.Count; idx++)
            {
                string line = lines[idx];

                if (isFirstLineNumber)
                {
                    while (line.Length > 0 && char.IsDigit(line[0]))
                    {
                        line = line.Remove(0, 1);
                    }
                }

                var matches = regexTimeCodes1.Matches(line);
                if (matches.Count == 0)
                {
                    matches = regexTimeCodes2.Matches(line);
                }

                var hasDurationColumn = matches.Count >= 3 && HasMatchingDurationColumn(line, matches);
                if (matches.Count == 1 && hasOnlyOneTimeCode)
                {
                    var start = SplitTimeCode(matches[0].ToString());
                    if (start.Length == 3 || start.Length == 4)
                    {
                        if (p != null)
                        {
                            p.Text = sb.ToString().Trim();
                            subtitle.Paragraphs.Add(p);
                        }
                        p = new Paragraph();
                        sb.Clear();
                        p.StartTime = DecodeTime(start);
                    }
                    if (matches[0].Index < 9)
                    {
                        line = line.Remove(0, matches[0].Index);
                    }

                    line = line.Replace(matches[0].ToString(), string.Empty);
                    line = line.Trim().TrimStart(']');
                    if (Utilities.IsInteger(line.RemoveChar('[', ']')))
                    {
                        line = string.Empty;
                    }
                    if (line.StartsWith("}{}", StringComparison.Ordinal) || line.StartsWith("][]", StringComparison.Ordinal))
                    {
                        line = line.Remove(0, 3);
                    }

                    line = line.Trim();
                }
                else if (matches.Count == 2 || hasDurationColumn)
                {
                    var start = SplitTimeCode(matches[0].Value, useColonHundredths, useDotFrames);
                    var end = SplitTimeCode(matches[1].Value, useColonHundredths, useDotFrames);
                    if ((start.Length == 3 || start.Length == 4) && (end.Length == 3 || end.Length == 4))
                    {
                        if (p != null)
                        {
                            p.Text = sb.ToString().Trim();
                            subtitle.Paragraphs.Add(p);
                        }
                        p = new Paragraph();
                        sb.Clear();
                        p.StartTime = DecodeTime(start);
                        p.EndTime = DecodeTime(end);
                    }

                    var betweenStart = matches[0].Index + matches[0].Length;
                    var between = line.Substring(betweenStart, Math.Max(0, matches[1].Index - betweenStart));
                    var before = line.Substring(0, matches[0].Index);
                    var timeInLabel = LabelledTimeInRegex.Match(before);
                    var timeOutLabel = LabelledTimeOutRegex.Match(between);
                    var labelledRange = timeInLabel.Success && timeOutLabel.Success;
                    if (labelledRange && !timeInLabel.Groups["numbered"].Success && timeOutLabel.Groups["duration"].Success && p != null)
                    {
                        var duration = timeOutLabel.Groups["duration"].Value.Split(':');
                        var durationMs = int.Parse(duration[0]) * TimeCode.BaseUnit +
                            SubtitleFormat.FramesToMillisecondsMax999(int.Parse(duration[1]));
                        if (durationMs > 0)
                        {
                            p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + durationMs;
                        }
                    }
                    var hasShortDurationBetweenTimes = between.StartsWith('\t') && between.EndsWith('\t') &&
                        ShortFrameDurationRegex.IsMatch(between.Trim()) &&
                        before.Trim().All(char.IsDigit) && matches[1].Index + matches[1].Length == line.TrimEnd().Length;
                    if (labelledRange || hasShortDurationBetweenTimes || IsOnlyTimeCodeSeparators(between))
                    {
                        // drop "start --> end", "start - end", "[start --> end]" as one block so the
                        // arrow/dash between the time codes does not end up in the text
                        var prefix = labelledRange || hasShortDurationBetweenTimes || matches[0].Index < 9 ? string.Empty : before;
                        var lastTimingColumn = matches[hasDurationColumn ? 2 : 1];
                        var text = line.Substring(lastTimingColumn.Index + lastTimingColumn.Length).TrimStart();
                        if (ShortFrameDurationRegex.IsMatch(text) && NumberedDurationPrefixRegex.IsMatch(before))
                        {
                            text = string.Empty;
                        }

                        var delimiter = between.Trim();
                        if ((delimiter == "," && text.StartsWith(',')) ||
                            (delimiter == "&" && text.StartsWith('#')))
                        {
                            text = text.Substring(1).TrimStart();
                        }

                        if (between.Contains('\t'))
                        {
                            text = text.Replace("\t", Environment.NewLine);
                        }

                        line = prefix + text.TrimStart(']', ')', '}', '|');
                        var time = matches[0].Value;
                        if (time.Length >= 4 && (time[time.Length - 3] == ':' || time[time.Length - 3] == ';' || useDotFrames))
                        {
                            line = SlashLineBreakRegex.Replace(line, match => match.Value[0] == '/' ? Environment.NewLine : match.Value);
                        }
                    }
                    else
                    {
                        if (matches[0].Index < 9)
                        {
                            line = line.Remove(0, matches[0].Index);
                        }

                        line = line.Replace(matches[0].ToString(), string.Empty);
                        line = line.Replace(matches[1].ToString(), string.Empty);
                    }

                    line = line.Trim().TrimStart(']');
                    if (Utilities.IsInteger(line.RemoveChar('[', ']')))
                    {
                        line = string.Empty;
                    }
                    if (line.StartsWith("}{}", StringComparison.Ordinal) || line.StartsWith("][]", StringComparison.Ordinal))
                    {
                        line = line.Remove(0, 3);
                    }

                    line = line.Trim();
                }

                if (p != null && line.Length > 0 &&
                    !(NumericCueIndexRegex.IsMatch(line) && idx + 1 < lines.Count && regexTimeCodes1.Matches(lines[idx + 1]).Count >= 2) &&
                    !(hasAlignmentHeader && IsAlignmentDirective(line)))
                {
                    sb.AppendLine(line.Trim());
                }
            }
            if (p != null)
            {
                p.Text = sb.ToString().Trim();
                subtitle.Paragraphs.Add(p);
            }

            // fix end time
            if (hasOnlyOneTimeCode)
            {
                for (var index = 0; index < subtitle.Paragraphs.Count; index++)
                {
                    var paragraph = subtitle.Paragraphs[index];
                    var next = subtitle.GetParagraphOrDefault(index + 1);
                    var optimalDurationMs = Utilities.GetOptimalDisplayMilliseconds(paragraph.Text);
                    if (next == null)
                    {
                        paragraph.EndTime.TotalMilliseconds = paragraph.StartTime.TotalMilliseconds + optimalDurationMs;
                    }
                    else
                    {
                        if (next.StartTime.TotalMilliseconds < paragraph.StartTime.TotalMilliseconds + optimalDurationMs + 2000)
                        {
                            paragraph.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                        }
                        else
                        {
                            paragraph.EndTime.TotalMilliseconds = paragraph.StartTime.TotalMilliseconds + optimalDurationMs;
                        }
                    }
                }
            }

            // remove all equal headers
            if (subtitle.Paragraphs.Count > 5)
            {
                string prefix = subtitle.Paragraphs[0].Text;
                var newPrefix = new StringBuilder();
                foreach (var paragraph in subtitle.Paragraphs)
                {
                    string text = paragraph.Text.Trim();
                    newPrefix.Clear();
                    int i = 0;
                    while (i < prefix.Length && i < text.Length && text[i] == prefix[i])
                    {
                        newPrefix.Append(text[i]);
                        i++;
                    }
                    prefix = newPrefix.ToString();
                }
                if (prefix.Length > 3 && prefix[1] == ':' && prefix[2] == '\\')
                {
                    prefix = string.Empty;
                }

                if (prefix.Length > 0)
                {
                    foreach (var paragraph in subtitle.Paragraphs)
                    {
                        string text = paragraph.Text.Trim();
                        if (text.StartsWith(prefix, StringComparison.Ordinal))
                        {
                            paragraph.Text = text.Remove(0, prefix.Length);
                        }
                    }
                }
            }

            subtitle.Renumber();
            return subtitle;
        }

        private static bool IsAlignmentDirective(string line)
        {
            var assignment = line.IndexOf('=');
            return assignment > 0 &&
                   line.Substring(0, assignment).Trim().Equals("$HorzAlign", StringComparison.OrdinalIgnoreCase);
        }

        private static bool HasMatchingDurationColumn(string line, MatchCollection times)
        {
            var first = times[0];
            var second = times[1];
            var third = times[2];
            if (line.Substring(first.Index + first.Length, second.Index - first.Index - first.Length) != "\t" ||
                line.Substring(second.Index + second.Length, third.Index - second.Index - second.Length) != "\t" ||
                third.Index + third.Length >= line.Length || line[third.Index + third.Length] != '\t')
            {
                return false;
            }

            var start = DecodeTime(SplitTimeCode(first.Value)).TotalMilliseconds;
            var end = DecodeTime(SplitTimeCode(second.Value)).TotalMilliseconds;
            var duration = DecodeTime(SplitTimeCode(third.Value)).TotalMilliseconds;
            return duration > 0 && Math.Abs(end - start - duration) < 1;
        }

        private static Subtitle ImportTimeCodesAndTextOnSameLineOnlySpaceAsSeparator(List<string> lines)
        {
            var regexTimeCodes1 = SpaceSeparatedTimeCodeWithHoursRegex;
            var regexTimeCodes2 = SpaceSeparatedTimeCodeWithoutHoursRegex;
            Paragraph p = null;
            var subtitle = new Subtitle();
            var sb = new StringBuilder();
            char[] splitChar = { ' ' };
            for (int idx = 0; idx < lines.Count; idx++)
            {
                string line = lines[idx];

                var matches = regexTimeCodes1.Matches(line);
                if (matches.Count == 0)
                {
                    matches = regexTimeCodes2.Matches(line);
                }

                if (matches.Count == 2)
                {
                    string[] start = matches[0].ToString().Split(splitChar, StringSplitOptions.RemoveEmptyEntries);
                    string[] end = matches[1].ToString().Split(splitChar, StringSplitOptions.RemoveEmptyEntries);
                    if ((start.Length == 3 || start.Length == 4) && (end.Length == 3 || end.Length == 4))
                    {
                        if (p != null)
                        {
                            p.Text = sb.ToString().Trim();
                            subtitle.Paragraphs.Add(p);
                        }
                        p = new Paragraph();
                        sb.Clear();
                        p.StartTime = DecodeTime(start);
                        p.EndTime = DecodeTime(end);
                    }
                    if (matches[0].Index < 9)
                    {
                        line = line.Remove(0, matches[0].Index);
                    }

                    line = line.Replace(matches[0].ToString(), string.Empty);
                    line = line.Replace(matches[1].ToString(), string.Empty);
                    line = line.Trim();
                    if (line.StartsWith("}{}", StringComparison.Ordinal) || line.StartsWith("][]", StringComparison.Ordinal))
                    {
                        line = line.Remove(0, 3);
                    }

                    line = line.Trim();
                }
                if (matches.Count == 2 && lines[idx].Contains('\t'))
                {
                    line = line.Trim().Replace("\t", Environment.NewLine);
                }

                if (p != null && line.Length > 1)
                {
                    sb.AppendLine(line.Trim());
                }
            }
            if (p != null)
            {
                p.Text = sb.ToString().Trim();
                subtitle.Paragraphs.Add(p);
            }
            subtitle.Renumber();
            return subtitle;
        }

        private static Subtitle ImportTimeCodesOnSameSeparateLine(List<string> lines)
        {
            Paragraph p = null;
            var subtitle = new Subtitle();
            var sb = new StringBuilder();
            char[] splitChars = { ' ', '\t' };
            for (int idx = 0; idx < lines.Count; idx++)
            {
                string line = lines[idx];
                string lineWithPerhapsOnlyNumbers = GetLineWithPerhapsOnlyNumbers(line);
                bool allNumbers = lineWithPerhapsOnlyNumbers.Length > 0;
                foreach (char c in lineWithPerhapsOnlyNumbers)
                {
                    if (!char.IsDigit(c))
                    {
                        allNumbers = false;
                        break;
                    }
                }
                if (allNumbers && lineWithPerhapsOnlyNumbers.Length > 5)
                {
                    if (line.Contains("->"))
                    {
                        line = line.RemoveChar(' ');
                    }

                    string[] arr = ReplaceBracketsWithSpaces(line).Trim().Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                    if (arr.Length == 2)
                    {
                        string[] start = SplitTimeCode(arr[0].Trim());
                        string[] end = SplitTimeCode(arr[1].Trim());
                        if ((start.Length == 3 || start.Length == 4) && (end.Length == 3 || end.Length == 4))
                        {
                            if (p != null)
                            {
                                p.Text = sb.ToString().Trim();
                                subtitle.Paragraphs.Add(p);
                            }
                            p = new Paragraph();
                            sb.Clear();
                            p.StartTime = DecodeTime(start);
                            p.EndTime = DecodeTime(end);
                        }
                    }
                    else if (arr.Length > 3)
                    {
                        string[] start;
                        string[] end;
                        if (arr[0].Length > 9)
                        {
                            start = SplitTimeCode(arr[0].Trim());
                            end = SplitTimeCode(arr[1].Trim());
                        }
                        else
                        {
                            start = SplitTimeCode(arr[1].Trim());
                            end = SplitTimeCode(arr[2].Trim());
                        }
                        if ((start.Length == 3 || start.Length == 4) && (end.Length == 3 || end.Length == 4))
                        {
                            if (p != null)
                            {
                                p.Text = sb.ToString().Trim();
                                subtitle.Paragraphs.Add(p);
                            }
                            p = new Paragraph();
                            sb.Clear();
                            p.StartTime = DecodeTime(start);
                            p.EndTime = DecodeTime(end);
                        }
                    }
                }
                if (p != null && !allNumbers && line.Length > 1)
                {
                    line = line.Trim();
                    if (line.StartsWith("}{}", StringComparison.Ordinal) || line.StartsWith("][]", StringComparison.Ordinal))
                    {
                        line = line.Remove(0, 3);
                    }

                    sb.AppendLine(line.Trim());
                }
            }
            if (p != null)
            {
                p.Text = sb.ToString().Trim();
                subtitle.Paragraphs.Add(p);
            }

            double averateDuration = 0;
            foreach (Paragraph a in subtitle.Paragraphs)
            {
                double d = a.DurationTotalSeconds;
                if (d > 10)
                {
                    d = 8;
                }

                averateDuration += d;
            }
            averateDuration = averateDuration / subtitle.Paragraphs.Count;
            if (averateDuration < 0.2 || (averateDuration < 0.5 && subtitle.Paragraphs.Count > 100 && subtitle.Paragraphs[subtitle.Paragraphs.Count - 1].StartTime.TotalSeconds < 140 && subtitle.Paragraphs[subtitle.Paragraphs.Count - 2].StartTime.TotalSeconds < 140))
            {
                subtitle = ImportTimeCodesOnSameSeparateLineNoMilliseconds(lines);
                int i = 0;
                foreach (Paragraph a in subtitle.Paragraphs)
                {
                    i++;
                    var next = subtitle.GetParagraphOrDefault(i);
                    if (next != null && a.EndTime.TotalMilliseconds >= next.StartTime.TotalMilliseconds)
                    {
                        a.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                    }
                }
                return subtitle;
            }

            subtitle.Renumber();
            return subtitle;
        }

        private static Subtitle ImportTimeCodesOnSameSeparateLineNoMilliseconds(List<string> lines)
        {
            Paragraph p = null;
            var subtitle = new Subtitle();
            var sb = new StringBuilder();
            char[] splitChar = { ' ' };
            for (int idx = 0; idx < lines.Count; idx++)
            {
                string line = lines[idx];
                string lineWithPerhapsOnlyNumbers = GetLineWithPerhapsOnlyNumbers(line);
                bool allNumbers = lineWithPerhapsOnlyNumbers.Length > 0;
                foreach (char c in lineWithPerhapsOnlyNumbers)
                {
                    if (!char.IsDigit(c))
                    {
                        allNumbers = false;
                    }
                }
                if (allNumbers && lineWithPerhapsOnlyNumbers.Length > 5)
                {
                    string[] arr = ReplaceBracketsWithSpaces(line).Trim().Split(splitChar, StringSplitOptions.RemoveEmptyEntries);
                    if (arr.Length == 2)
                    {
                        string[] start = arr[0].Trim().Split(ExpectedSplitChars, StringSplitOptions.RemoveEmptyEntries);
                        string[] end = arr[1].Trim().Split(ExpectedSplitChars, StringSplitOptions.RemoveEmptyEntries);
                        if ((start.Length == 3 || start.Length == 4) && (end.Length == 3 || end.Length == 4))
                        {
                            if (start.Length == 3)
                            {
                                start = (arr[0].Trim() + ".000").Split(ExpectedSplitChars, StringSplitOptions.RemoveEmptyEntries);
                            }

                            if (end.Length == 3)
                            {
                                end = (arr[1].Trim() + ".000").Split(ExpectedSplitChars, StringSplitOptions.RemoveEmptyEntries);
                            }

                            if (p != null)
                            {
                                p.Text = sb.ToString().Trim();
                                subtitle.Paragraphs.Add(p);
                            }
                            p = new Paragraph();
                            sb.Clear();
                            p.StartTime = DecodeTime(start);
                            p.EndTime = DecodeTime(end);
                        }
                    }
                    else if (arr.Length == 3)
                    {
                        string[] start = arr[0].Trim().Split(ExpectedSplitChars, StringSplitOptions.RemoveEmptyEntries);
                        string[] end = arr[1].Trim().Split(ExpectedSplitChars, StringSplitOptions.RemoveEmptyEntries);
                        string[] duration = arr[2].Trim().Split(ExpectedSplitChars, StringSplitOptions.RemoveEmptyEntries);

                        if (start.Length == 3)
                        {
                            start = (arr[0].Trim() + ".000").Split(ExpectedSplitChars, StringSplitOptions.RemoveEmptyEntries);
                        }

                        if (end.Length == 3)
                        {
                            end = (arr[1].Trim() + ".000").Split(ExpectedSplitChars, StringSplitOptions.RemoveEmptyEntries);
                        }

                        if (duration.Length == 3)
                        {
                            duration = (arr[2].Trim() + ".000").Split(ExpectedSplitChars, StringSplitOptions.RemoveEmptyEntries);
                        }

                        if (start.Length < 3)
                        {
                            start = end;
                            end = duration;
                        }

                        if ((start.Length == 3 || start.Length == 4) && (end.Length == 3 || end.Length == 4))
                        {
                            if (p != null)
                            {
                                p.Text = sb.ToString().Trim();
                                subtitle.Paragraphs.Add(p);
                            }
                            p = new Paragraph();
                            sb.Clear();
                            p.StartTime = DecodeTime(start);
                            p.EndTime = DecodeTime(end);
                        }
                    }
                }
                if (p != null && !allNumbers && line.Length > 1)
                {
                    line = line.Trim();
                    if (line.StartsWith("}{}", StringComparison.Ordinal) || line.StartsWith("][]", StringComparison.Ordinal))
                    {
                        line = line.Remove(0, 3);
                    }

                    sb.AppendLine(line.Trim());
                }
            }
            if (p != null)
            {
                p.Text = sb.ToString().Trim();
                subtitle.Paragraphs.Add(p);
            }

            subtitle.Renumber();
            return subtitle;
        }

        /// <summary>
        /// Splits a time code into parts. Two digits (or one) after a "." or "," are hundredths
        /// (or tenths) of a second, not frames: "00:00:01.50" is 1.500 s. Frames stay for ":"/";".
        /// </summary>
        private static string[] SplitTimeCode(string timeCode, bool useColonHundredths = false, bool useDotFrames = false)
        {
            var parts = timeCode.Split(ExpectedSplitChars, StringSplitOptions.RemoveEmptyEntries);
            var lastSeparator = timeCode.TrimEnd().LastIndexOfAny(ExpectedSplitChars);
            if (parts.Length >= 3 && lastSeparator > 0 &&
                ((timeCode[lastSeparator] == '.' && !(useDotFrames && parts.Length == 4 && parts[3].Length == 2)) || timeCode[lastSeparator] == ',' ||
                 (useColonHundredths && ColonHundredthsRegex.IsMatch(timeCode))) &&
                parts[parts.Length - 1].Length <= 2)
            {
                parts[parts.Length - 1] = parts[parts.Length - 1].PadRight(3, '0');
            }
            else if (parts.Length >= 3 && parts[parts.Length - 1].Length > 3)
            {
                parts[parts.Length - 1] = parts[parts.Length - 1].Substring(0, 3);
            }

            return parts;
        }

        /// <summary>
        /// True for what usually sits between a start and an end time code: " --> ", " - ", " – ", "}{", ...
        /// </summary>
        private static bool IsOnlyTimeCodeSeparators(string s)
        {
            foreach (var ch in s)
            {
                if (!char.IsWhiteSpace(ch) && "-–—>=~|[](){},;/&".IndexOf(ch) < 0)
                {
                    return false;
                }
            }

            return true;
        }

        private static string GetLineWithPerhapsOnlyNumbers(string line)
        {
            return line.RemoveChar(' ', '.', ',', '\t', ':', ';', '{', '}', '[', ']', '-', '>', '<');
        }

        private static TimeCode DecodeTime(string[] parts)
        {
            try
            {
                string hour = parts[0];
                string minutes = parts[1];
                string seconds = parts[2];
                string frames;
                if (parts.Length < 4)
                {
                    frames = seconds;
                    seconds = minutes;
                    minutes = hour;
                    hour = "0";
                }
                else
                {
                    frames = parts[3];
                }

                if (frames.Length < 3)
                {
                    return new TimeCode(int.Parse(hour), int.Parse(minutes), int.Parse(seconds), SubtitleFormat.FramesToMillisecondsMax999(int.Parse(frames)));
                }

                return new TimeCode(int.Parse(hour), int.Parse(minutes), int.Parse(seconds), int.Parse(frames));
            }
            catch
            {
                return new TimeCode();
            }
        }

        private static Subtitle ImportSubtitleWithNoLineBreaks(string text)
        {
            // \G anchors at the startat position passed to Match(text, i) below - matching
            // in place instead of allocating text.Substring(i) per digit, which made this
            // loop O(n²) on large inputs with many numeric characters (issue #12683).
            var regex = SubRipLikeTimeCodeRegex;
            var subtitle = new Subtitle();
            int i = 0;
            var sb = new StringBuilder();
            Paragraph p = null;
            while (i < text.Length)
            {
                var ch = text[i];
                if (char.IsNumber(ch))
                {
                    var match = regex.Match(text, i);
                    if (match.Success)
                    {
                        if (p != null)
                        {
                            p.Text = Utilities.AutoBreakLine(sb.ToString().Trim());
                        }

                        sb.Clear();
                        var arr = match.Value.Split(' ');
                        if (arr.Length == 4)
                        {
                            i += match.Value.Length;
                            p = new Paragraph
                            {
                                StartTime = DecodeTime(SplitTimeCode(arr[1])),
                                EndTime = DecodeTime(SplitTimeCode(arr[3]))
                            };
                            subtitle.Paragraphs.Add(p);
                            continue;
                        }
                    }

                    if (char.IsDigit(ch))
                    {
                        AppendDigitRun(text, sb, ref i);
                        continue;
                    }
                }
                sb.Append(ch);
                i++;
            }
            if (p != null && string.IsNullOrEmpty(p.Text))
            {
                p.Text = Utilities.AutoBreakLine(sb.ToString().Trim());
            }

            subtitle.Renumber();
            return subtitle;
        }

        /// <summary>
        /// Appends the run of decimal digits starting at <paramref name="i"/> and advances past it.
        /// The time code regexes' leading \d+ must consume digits up to the first non-digit no
        /// matter where in the run the match attempt starts, so a failed attempt at the run's
        /// first digit fails at every later digit too - retrying there just made digit-heavy
        /// binary input quadratic (issue #12683).
        /// </summary>
        private static void AppendDigitRun(string text, StringBuilder sb, ref int i)
        {
            do
            {
                sb.Append(text[i]);
                i++;
            }
            while (i < text.Length && char.IsDigit(text[i]));
        }

        private static Subtitle ImportSubtitleWithNoLineBreaksWithExtraSpaces(string text)
        {
            // \G anchors at the startat position passed to Match(text, i) below - see
            // ImportSubtitleWithNoLineBreaks for why Substring(i) is not used here.
            var regex = LooseTimeCodeRegex;
            var subtitle = new Subtitle();
            int i = 0;
            var sb = new StringBuilder();
            Paragraph p = null;
            while (i < text.Length)
            {
                var ch = text[i];
                if (char.IsNumber(ch))
                {
                    var match = regex.Match(text, i);
                    if (match.Success)
                    {
                        if (p != null)
                        {
                            p.Text = Utilities.AutoBreakLine(sb.ToString().Trim());
                        }

                        sb.Clear();
                        var arr = match.Value.Split('>');
                        if (arr.Length == 2)
                        {
                            i += match.Value.Length;
                            p = new Paragraph
                            {
                                StartTime = DecodeTime(SplitTimeCode(arr[0].RemoveChar(' ').TrimEnd('-'))),
                                EndTime = DecodeTime(SplitTimeCode(arr[1].RemoveChar(' ').TrimEnd('-')))
                            };
                            subtitle.Paragraphs.Add(p);
                            continue;
                        }
                    }

                    if (char.IsDigit(ch))
                    {
                        AppendDigitRun(text, sb, ref i);
                        continue;
                    }
                }
                sb.Append(ch);
                i++;
            }
            if (p != null && string.IsNullOrEmpty(p.Text))
            {
                p.Text = Utilities.AutoBreakLine(sb.ToString().Trim());
            }

            subtitle.Renumber();
            return subtitle;
        }
    }
}
