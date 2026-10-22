using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// SubRip (.srt) reader/writer.
    /// </summary>
    /// <remarks>
    /// The reader is anchored on time code lines instead of walking a number/time code/text
    /// state machine: every line that parses as "start --> end" starts a paragraph, and the
    /// lines between two time code lines are the text of the first one, minus surrounding blank
    /// lines and minus a trailing number line (the next paragraph's number). That makes missing
    /// numbers, missing or extra blank lines and blank lines inside the text all fall out of the
    /// same rule, with no look-ahead window.
    ///
    /// Time code lines are parsed in place (no Replace/Split/Substring chains), tolerating the
    /// usual hand-edit damage: '.', ';', ':' or Arabic commas as decimal separator, missing
    /// hours or milliseconds, one- or four-digit fields, '->', '—>', '-- >', '- -' etc. as the
    /// arrow, stray spaces, a letter O for a zero and trailing position info.
    ///
    /// The canonical "hh:mm:ss,mmm --> hh:mm:ss,mmm" line and a single, untouched text line are
    /// handled without allocating anything beyond the paragraph itself.
    /// </remarks>
    public partial class SubRip : SubtitleFormat
    {
        public string Errors { get; private set; }

#if NET7_0_OR_GREATER
        [GeneratedRegex(@"<3\d>")]
        private static partial Regex _regExWsrtItalicStartGen();
        private static readonly Regex _regExWsrtItalicStart = _regExWsrtItalicStartGen();

        [GeneratedRegex(@"</3\d>")]
        private static partial Regex _regExWsrtItalicEndGen();
        private static readonly Regex _regExWsrtItalicEnd = _regExWsrtItalicEndGen();
#else
        private static readonly Regex _regExWsrtItalicStart = new Regex(@"<3\d>", RegexOptions.Compiled);
        private static readonly Regex _regExWsrtItalicEnd = new Regex(@"</3\d>", RegexOptions.Compiled);
#endif

        private const int MaxErrorsLength = 2000;

        public override string Extension => ".srt";

        public const string NameOfFormat = "SubRip";

        public override string Name => NameOfFormat;

        public override List<string> AlternateExtensions => new List<string> { ".wsrt" };

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (lines.Count > 0 && lines[0].StartsWith("WEBVTT", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);
            Errors = null;
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var paragraphs = subtitle.Paragraphs;
            var newLine = Environment.NewLine;

            var capacity = 2 * newLine.Length;
            foreach (var p in paragraphs)
            {
                capacity += 40 + 3 * newLine.Length + (p.Text?.Length ?? 0);
            }

            var sb = new StringBuilder(capacity);
            foreach (var p in paragraphs)
            {
                sb.Append(p.Number).Append(newLine);
                AppendTimeCode(sb, p.StartTime);
                sb.Append(" --> ");
                AppendTimeCode(sb, p.EndTime);
                sb.Append(newLine);
                sb.Append(p.Text).Append(newLine).Append(newLine);
            }

            sb.Trim();
            return sb.Append(newLine).Append(newLine).ToString();
        }

        // Same output as TimeCode.ToString() ("hh:mm:ss,fff"), written straight into the builder.
        private static void AppendTimeCode(StringBuilder sb, TimeCode timeCode)
        {
            var totalMilliseconds = timeCode.TotalMilliseconds;
            if (totalMilliseconds < 0 || totalMilliseconds > TimeCode.MaxTimeTotalMilliseconds)
            {
                sb.Append(timeCode.ToString());
                return;
            }

            var ts = timeCode.TimeSpan;
            var hours = ts.Hours + ts.Days * 24;
            if (hours > 99)
            {
                sb.Append(hours);
            }
            else
            {
                AppendTwoDigits(sb, hours);
            }

            sb.Append(':');
            AppendTwoDigits(sb, ts.Minutes);
            sb.Append(':');
            AppendTwoDigits(sb, ts.Seconds);
            sb.Append(',');
            var ms = ts.Milliseconds;
            sb.Append((char)('0' + ms / 100));
            AppendTwoDigits(sb, ms % 100);
        }

        private static void AppendTwoDigits(StringBuilder sb, int value)
        {
            sb.Append((char)('0' + value / 10));
            sb.Append((char)('0' + value % 10));
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            var isWsrt = fileName != null && fileName.EndsWith(".wsrt", StringComparison.OrdinalIgnoreCase);
            var paragraphs = subtitle.Paragraphs;
            paragraphs.Clear();

            var lineCount = lines.Count;
            if (paragraphs.Capacity < lineCount / 4)
            {
                paragraphs.Capacity = lineCount / 4; // rough estimate: 4 lines per paragraph
            }

            var state = new LoadState { Lines = lines, IsWsrt = isWsrt, IsMsFrames = true };
            Paragraph current = null; // paragraph whose text lines are still being collected
            var blockStart = 0; // first line after the last time code line
            var renumber = false;

            for (var i = 0; i < lineCount; i++)
            {
                var line = lines[i];
                if (!TryParseTimeCodeLine(line, out var tc))
                {
                    if (tc.LooksLikeTimeCode)
                    {
                        // Unreadable time code: the text below it belongs to no paragraph
                        // (and is counted as errors), as do its number line above it.
                        CloseBlock(ref state, current, blockStart, i);
                        state.AddError(FormatLanguage.LineNumberXErrorReadingTimeCodeFromSourceLineY, i, line);
                        current = null;
                        blockStart = i + 1;
                    }

                    continue;
                }

                var number = CloseBlock(ref state, current, blockStart, i);

                // Two-digit "milliseconds" no higher than 30 everywhere means they are frames.
                if (IsNotFrames(tc.StartMsDigits, tc.StartMs) || IsNotFrames(tc.EndMsDigits, tc.EndMs))
                {
                    state.IsMsFrames = false;
                }

                current = new Paragraph(new TimeCode(tc.StartTotal), new TimeCode(tc.EndTotal), string.Empty);
                if (number >= 0)
                {
                    current.Number = number;
                }
                else
                {
                    renumber = true;
                }

                paragraphs.Add(current);
                blockStart = i + 1;
            }

            CloseBlock(ref state, current, blockStart, lineCount);

            if (renumber)
            {
                subtitle.Renumber();
            }

            if (state.IsMsFrames && paragraphs.Count > 0)
            {
                foreach (var p in paragraphs)
                {
                    p.StartTime.Milliseconds = FramesToMillisecondsMax999(p.StartTime.Milliseconds);
                    p.EndTime.Milliseconds = FramesToMillisecondsMax999(p.EndTime.Milliseconds);
                }
            }

            _errorCount = state.ErrorCount;
            Errors = state.Errors?.ToString() ?? string.Empty;
        }

        private static bool IsNotFrames(int msDigits, int ms) => msDigits != 0 && (msDigits != 2 || ms > 30);

        private struct LoadState
        {
            public List<string> Lines;
            public bool IsWsrt;
            public bool IsMsFrames;
            public int ErrorCount;
            public StringBuilder Errors;

            public void AddError(string format, int lineIndex, string line)
            {
                ErrorCount++;
                if (Errors == null)
                {
                    Errors = new StringBuilder();
                }

                if (Errors.Length < MaxErrorsLength)
                {
                    Errors.AppendLine(string.Format(format, lineIndex + 1, line));
                }
            }
        }

        /// <summary>
        /// Handles the lines [from, to) between two time code lines: a trailing number line is
        /// the next paragraph's number (returned, or -1), the rest is the text of
        /// <paramref name="paragraph"/> - or, with no paragraph (before the first time code or
        /// after an unreadable one), errors.
        /// </summary>
        private static int CloseBlock(ref LoadState state, Paragraph paragraph, int from, int to)
        {
            var lines = state.Lines;
            var end = to;
            while (end > from && IsBlank(lines[end - 1]))
            {
                end--;
            }

            // At the end of the file there is no next paragraph, so a last number line is only a
            // (dangling) paragraph number when a blank line separates it from the text - a
            // paragraph whose only text is a number ("1999") keeps it.
            var number = -1;
            if (end > from && TryParseNumber(lines[end - 1], out var n) &&
                (to < lines.Count || paragraph == null || end - 1 > from && IsBlank(lines[end - 2])))
            {
                number = n;
                end--;
                while (end > from && IsBlank(lines[end - 1]))
                {
                    end--;
                }
            }

            var start = from;
            while (start < end && IsBlank(lines[start]))
            {
                start++;
            }

            if (start >= end)
            {
                return number;
            }

            if (paragraph == null)
            {
                for (var i = start; i < end; i++)
                {
                    if (!IsBlank(lines[i]))
                    {
                        state.AddError(FormatLanguage.LineNumberXExpectedNumberFromSourceLineY, i, lines[i]);
                    }
                }

                return number;
            }

            var text = BuildText(lines, start, end);
            if (state.IsWsrt && text.Contains("<3"))
            {
                text = _regExWsrtItalicStart.Replace(text, "<i>");
                text = _regExWsrtItalicEnd.Replace(text, "</i>");
            }

            paragraph.Text = text;
            return number;
        }

        // Joins the text lines [start, end) with Environment.NewLine. Each line loses trailing
        // white space and DEL (0x7F) characters plus leading DELs, and NUL becomes a space.
        private static string BuildText(List<string> lines, int start, int end)
        {
            var newLine = Environment.NewLine;
            var length = 0;
            var hasNul = false;
            for (var i = start; i < end; i++)
            {
                var line = lines[i];
                GetTextBounds(line, out var s, out var e);
                length += e - s;
                hasNul |= line.IndexOf('\0', s, e - s) >= 0;
            }

            if (end - start == 1 && !hasNul)
            {
                var line = lines[start];
                GetTextBounds(line, out var s, out var e);
                return s == 0 && e == line.Length ? line : line.Substring(s, e - s);
            }

            length += (end - start - 1) * newLine.Length;
            return string.Create(length, (lines, start, end, hasNul), (span, args) =>
            {
                var pos = 0;
                var nl = Environment.NewLine;
                for (var i = args.start; i < args.end; i++)
                {
                    if (i > args.start)
                    {
                        nl.AsSpan().CopyTo(span.Slice(pos));
                        pos += nl.Length;
                    }

                    var line = args.lines[i];
                    GetTextBounds(line, out var s, out var e);
                    line.AsSpan(s, e - s).CopyTo(span.Slice(pos));
                    pos += e - s;
                }

                if (args.hasNul)
                {
                    for (var i = 0; i < span.Length; i++)
                    {
                        if (span[i] == '\0')
                        {
                            span[i] = ' ';
                        }
                    }
                }
            });
        }

        private static void GetTextBounds(string line, out int start, out int end)
        {
            start = 0;
            end = line.Length;
            while (end > 0)
            {
                var c = line[end - 1];
                if (c == '\u007F' || c == '\0' || char.IsWhiteSpace(c))
                {
                    end--;
                }
                else
                {
                    break;
                }
            }

            while (start < end && line[start] == '\u007F')
            {
                start++;
            }
        }

        private static bool IsBlank(string line)
        {
            for (var i = 0; i < line.Length; i++)
            {
                if (!IsSpace(line[i]) && line[i] != '\0')
                {
                    return false;
                }
            }

            return true;
        }

        private static bool TryParseNumber(string line, out int number)
        {
            number = 0;
            var i = 0;
            var len = line.Length;
            while (i < len && IsSpace(line[i]))
            {
                i++;
            }

            while (len > i && IsSpace(line[len - 1]))
            {
                len--;
            }

            if (len - i == 0 || len - i > 9)
            {
                return false;
            }

            for (; i < len; i++)
            {
                var d = line[i] - '0';
                if ((uint)d > 9)
                {
                    return false;
                }

                number = number * 10 + d;
            }

            return true;
        }

        // White space, plus the zero-width characters and DEL that turn up around time codes.
        private static bool IsSpace(char c)
        {
            if (c <= ' ')
            {
                return c == ' ' || c == '\t' || c == '\r' || c == '\n' || c == '\v' || c == '\f';
            }

            if (c < 0x7F)
            {
                return false;
            }

            return c == '\u007F' || c == '​' || c == '﻿' || char.IsWhiteSpace(c);
        }

        internal struct TimeCodeLine
        {
            public long StartTotal;
            public long EndTotal;
            public int StartMs;
            public int EndMs;
            public int StartMsDigits;
            public int EndMsDigits;

            /// <summary>
            /// Set when parsing failed but the line starts with a digit and contains '>' and at least
            /// three ':' - a damaged time code rather than a text line.
            /// </summary>
            public bool LooksLikeTimeCode;
        }

        internal static bool TryParseTimeCodeLine(string line, out TimeCodeLine result)
        {
            result = default;
            var len = line.Length;
            var pos = 0;
            while (pos < len && IsSpace(line[pos]))
            {
                pos++;
            }

            if (len - pos < 7)
            {
                return false;
            }

            var first = line[pos];
            if ((uint)(first - '0') > 9 && first != '-')
            {
                return false;
            }

            if (len - pos >= 29 && TryParseCanonical(line, pos, ref result))
            {
                return true;
            }

            if (TryParseTolerant(line, pos, ref result))
            {
                return true;
            }

            result.LooksLikeTimeCode = first != '-' && line.IndexOf('>', pos) >= 0 && CountColons(line, pos) >= 3;
            return false;
        }

        // A damaged time code line has the colons of two time codes; text such as
        // "10:30 --> 11:00" does not.
        private static int CountColons(string line, int pos)
        {
            var count = 0;
            for (var i = pos; i < line.Length; i++)
            {
                if (line[i] == ':')
                {
                    count++;
                }
            }

            return count;
        }

        // "hh:mm:ss,mmm --> hh:mm:ss,mmm", optionally followed by a space and anything else
        // (position info).
        private static bool TryParseCanonical(string s, int p, ref TimeCodeLine result)
        {
            if (s[p + 2] != ':' || s[p + 5] != ':' || s[p + 8] != ',' ||
                s[p + 12] != ' ' || s[p + 13] != '-' || s[p + 14] != '-' || s[p + 15] != '>' || s[p + 16] != ' ' ||
                s[p + 19] != ':' || s[p + 22] != ':' || s[p + 25] != ',' ||
                !IsValidTail(s, p + 29, allowAnyAfterSpace: true))
            {
                return false;
            }

            var startH = Digits2(s, p);
            var startM = Digits2(s, p + 3);
            var startS = Digits2(s, p + 6);
            var startMs = Digits3(s, p + 9);
            var endH = Digits2(s, p + 17);
            var endM = Digits2(s, p + 20);
            var endS = Digits2(s, p + 23);
            var endMs = Digits3(s, p + 26);
            if ((startH | startM | startS | startMs | endH | endM | endS | endMs) < 0)
            {
                return false;
            }

            result.StartTotal = ((startH * 60L + startM) * 60 + startS) * 1000 + startMs;
            result.EndTotal = ((endH * 60L + endM) * 60 + endS) * 1000 + endMs;
            result.StartMs = startMs;
            result.EndMs = endMs;
            result.StartMsDigits = 3;
            result.EndMsDigits = 3;
            return true;
        }

        private static int Digits2(string s, int i)
        {
            var a = s[i] - '0';
            var b = s[i + 1] - '0';
            return (uint)a > 9 || (uint)b > 9 ? -1 : a * 10 + b;
        }

        private static int Digits3(string s, int i)
        {
            var a = s[i] - '0';
            var b = s[i + 1] - '0';
            var c = s[i + 2] - '0';
            return (uint)a > 9 || (uint)b > 9 || (uint)c > 9 ? -1 : a * 100 + b * 10 + c;
        }

        private struct TimeCodeSide
        {
            public long Total;
            public int Ms;
            public int MsDigits;
            public int Groups;
            public bool MsAfterDecimal; // ',' or '.' before the milliseconds
        }

        private static bool TryParseTolerant(string s, int pos, ref TimeCodeLine result)
        {
            if (!TryParseSide(s, ref pos, out var start))
            {
                return false;
            }

            // The arrow: any mix of '-', '–', '—', '=', '~', '>' and spaces ("-->", "->", "—>",
            // "- ->", "-->>", "==>", "--"...). A '-' straight after the '>' that is followed by a
            // digit is the sign of a negative end time, not part of the arrow.
            var len = s.Length;
            var arrowChars = 0;
            var hasGreaterThan = false;
            while (pos < len)
            {
                var c = s[pos];
                if (c == '>')
                {
                    hasGreaterThan = true;
                }
                else if (c == '-' || c == '–' || c == '—' || c == '=' || c == '~')
                {
                    if (hasGreaterThan && pos + 1 < len && (uint)(s[pos + 1] - '0') <= 9)
                    {
                        break;
                    }
                }
                else if (!IsSpace(c))
                {
                    break;
                }
                else
                {
                    pos++;
                    continue;
                }

                arrowChars++;
                pos++;
            }

            if (arrowChars == 0 || !TryParseSide(s, ref pos, out var end))
            {
                return false;
            }

            // Only accept full-looking time codes, so text such as "10:30 --> 11:00" or
            // "10:30:00 - 11:00:00" does not turn into a paragraph. Without a '>' they must be
            // complete "hh:mm:ss,mmm" - other formats use "-" or "–" between frame time codes.
            if (hasGreaterThan
                    ? start.Groups < 3 || end.Groups < 3
                    : !IsCompleteWithMilliseconds(start) || !IsCompleteWithMilliseconds(end))
            {
                return false;
            }

            if (!IsValidTail(s, pos, allowAnyAfterSpace: false))
            {
                return false;
            }

            result.StartTotal = start.Total;
            result.EndTotal = end.Total;
            result.StartMs = start.Ms;
            result.EndMs = end.Ms;
            result.StartMsDigits = start.MsDigits;
            result.EndMsDigits = end.MsDigits;
            return true;
        }

        // After the end time: nothing but white space, or position info ("X1:100 X2:200 Y1:10
        // Y2:20"). The canonical line also keeps the old reader's "a space
        // and anything". Other text there is another format's line (Sony DVDArchitect,
        // Unknown 101...), not SubRip.
        private static bool IsValidTail(string s, int pos, bool allowAnyAfterSpace)
        {
            var len = s.Length;
            var i = pos;
            while (i < len && IsSpace(s[i]))
            {
                i++;
            }

            if (i >= len)
            {
                return true;
            }

            if ((s[i] == 'X' || s[i] == 'x') && i + 1 < len && (uint)(s[i + 1] - '0') <= 9)
            {
                return true;
            }

            return allowAnyAfterSpace && s[pos] == ' ';
        }

        private static bool IsCompleteWithMilliseconds(TimeCodeSide side) =>
            side.Groups == 4 && side.MsDigits == 3 && side.MsAfterDecimal;

        private static bool IsGroupSeparator(char c)
        {
            return c == ':' || c == ',' || c == '.' || c == ';' ||
                   c == '،' || c == '¡' || c == '\u0098' || c == '：' || c == '，' || c == '٫';
        }

        private static bool IsDecimalSeparator(char c) => c != ':' && c != '：';

        // Reads one time code: 2-4 numeric groups separated by ':', ',', '.', ';' (with optional
        // spaces around the separator), with an optional leading '-'.
        private static bool TryParseSide(string s, ref int pos, out TimeCodeSide side)
        {
            side = default;
            var len = s.Length;
            while (pos < len && IsSpace(s[pos]))
            {
                pos++;
            }

            var negative = false;
            if (pos < len && s[pos] == '-')
            {
                negative = true;
                pos++;
                while (pos < len && IsSpace(s[pos]))
                {
                    pos++;
                }
            }

            int g0 = 0, g1 = 0, g2 = 0, g3 = 0;
            int lastDigits = 0, groups = 0;
            var lastSeparator = '\0';
            while (true)
            {
                var value = 0;
                var digits = 0;
                var realDigits = 0;
                while (pos < len)
                {
                    var c = s[pos];
                    int d;
                    if ((uint)(c - '0') <= 9)
                    {
                        d = c - '0';
                        realDigits++;
                    }
                    else if (c == 'O' || c == 'o')
                    {
                        d = 0; // OCR/typing slip: letter O for zero
                    }
                    else
                    {
                        break;
                    }

                    if (++digits > 9)
                    {
                        return false;
                    }

                    value = value * 10 + d;
                    pos++;
                }

                if (realDigits == 0)
                {
                    return false;
                }

                switch (groups)
                {
                    case 0: g0 = value; break;
                    case 1: g1 = value; break;
                    case 2: g2 = value; break;
                    default: g3 = value; break;
                }

                groups++;
                lastDigits = digits;

                var p = pos;
                while (p < len && s[p] == ' ')
                {
                    p++;
                }

                if (groups == 4 || p >= len || !IsGroupSeparator(s[p]))
                {
                    break;
                }

                var separator = s[p];
                p++;
                while (p < len && s[p] == ' ')
                {
                    p++;
                }

                if (p >= len || (uint)(s[p] - '0') > 9 && s[p] != 'O' && s[p] != 'o')
                {
                    break; // e.g. a trailing '.', leave it
                }

                lastSeparator = separator;
                pos = p;
            }

            int h = 0, m = 0, sec, ms = 0, msDigits = 0;
            switch (groups)
            {
                case 4:
                    h = g0; m = g1; sec = g2; ms = g3; msDigits = lastDigits;
                    break;
                case 3:
                    if (IsDecimalSeparator(lastSeparator) || lastDigits == 3)
                    {
                        m = g0; sec = g1; ms = g2; msDigits = lastDigits; // missing hours
                    }
                    else
                    {
                        h = g0; m = g1; sec = g2; // missing milliseconds
                    }

                    break;
                case 2:
                    if (IsDecimalSeparator(lastSeparator))
                    {
                        sec = g0; ms = g1; msDigits = lastDigits;
                    }
                    else
                    {
                        m = g0; sec = g1;
                    }

                    break;
                default:
                    return false;
            }

            // Milliseconds with too many digits: keep the first three ("01,0005" -> 0 ms).
            while (msDigits > 3)
            {
                ms /= 10;
                msDigits--;
            }

            var total = ((h * 60L + m) * 60 + sec) * 1000 + ms;
            side.Total = negative ? -total : total;
            side.Ms = ms;
            side.MsDigits = msDigits;
            side.Groups = groups;
            side.MsAfterDecimal = msDigits > 0 && IsDecimalSeparator(lastSeparator);
            return true;
        }
    }
}
