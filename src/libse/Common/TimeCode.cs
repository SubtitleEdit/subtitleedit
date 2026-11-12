using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Core.Common
{
    public class TimeCode
    {
        public static readonly char[] TimeSplitChars = { ':', ',', '.' };
        public const double BaseUnit = 1000.0; // Base unit of time

        public bool IsMaxTime => Math.Abs(TotalMilliseconds - MaxTimeTotalMilliseconds) < 0.01;
        public const double MaxTimeTotalMilliseconds = 359999999; // new TimeCode(99, 59, 59, 999).TotalMilliseconds

        public static TimeCode FromSeconds(double seconds)
        {
            return new TimeCode(seconds * BaseUnit);
        }

        /// <summary>
        /// Parse time string to milliseconds, format: HH[:,.]MM[:,.]SS[:,.]MSec or MM[:,.]SS[:,.]MSec
        /// </summary>
        /// <param name="text">Time code as string.</param>
        /// <returns>Total milliseconds.</returns>
        public static double ParseToMilliseconds(string text)
        {
            // Shared time-code parse entry for many format importers, called per line while
            // loading - tokenize with spans instead of Split (string[] + a substring per part
            // + a PadRight copy for the fraction).
            Span<int> starts = stackalloc int[MaxTimeParts];
            Span<int> lengths = stackalloc int[MaxTimeParts];
            var count = SplitTimeParts(text, starts, lengths);
            var s = text.AsSpan();

            if (count == 4)
            {
                if (int.TryParse(s.Slice(starts[0], lengths[0]), out var hours) &&
                    int.TryParse(s.Slice(starts[1], lengths[1]), out var minutes) &&
                    int.TryParse(s.Slice(starts[2], lengths[2]), out var seconds) &&
                    TryParsePaddedMilliseconds(s.Slice(starts[3], lengths[3]), out var milliseconds))
                {
                    return new TimeSpan(0, hours, minutes, seconds, milliseconds).TotalMilliseconds;
                }
            }

            if (count == 3)
            {
                if (int.TryParse(s.Slice(starts[0], lengths[0]), out var minutes) &&
                    int.TryParse(s.Slice(starts[1], lengths[1]), out var seconds) &&
                    TryParsePaddedMilliseconds(s.Slice(starts[2], lengths[2]), out var milliseconds))
                {
                    return new TimeSpan(0, 0, minutes, seconds, milliseconds).TotalMilliseconds;
                }
            }

            return 0;
        }

        public static double ParseHHMMSSFFToMilliseconds(string text)
        {
            Span<int> starts = stackalloc int[MaxTimeParts];
            Span<int> lengths = stackalloc int[MaxTimeParts];
            var count = SplitTimeParts(text, starts, lengths);
            var s = text.AsSpan();

            if (count == 4)
            {
                if (int.TryParse(s.Slice(starts[0], lengths[0]), out var hours) &&
                    int.TryParse(s.Slice(starts[1], lengths[1]), out var minutes) &&
                    int.TryParse(s.Slice(starts[2], lengths[2]), out var seconds) &&
                    int.TryParse(s.Slice(starts[3], lengths[3]), out var frames))
                {
                    return new TimeCode(hours, minutes, seconds, SubtitleFormat.FramesToMillisecondsMax999(frames)).TotalMilliseconds;
                }
            }
            return 0;
        }

        public static double ParseHHMMSSToMilliseconds(string text)
        {
            Span<int> starts = stackalloc int[MaxTimeParts];
            Span<int> lengths = stackalloc int[MaxTimeParts];
            var count = SplitTimeParts(text, starts, lengths);
            var s = text.AsSpan();

            if (count == 3)
            {
                if (int.TryParse(s.Slice(starts[0], lengths[0]), out var hours) &&
                    int.TryParse(s.Slice(starts[1], lengths[1]), out var minutes) &&
                    int.TryParse(s.Slice(starts[2], lengths[2]), out var seconds))
                {
                    return new TimeCode(hours, minutes, seconds, 0).TotalMilliseconds;
                }
            }

            return 0;
        }

        // One extra slot beyond the longest accepted shape (4 parts), so a 5th part is
        // representable and makes the count checks above fail like Split's array length did.
        private const int MaxTimeParts = 5;

        // Tokenizes like text.Split(TimeSplitChars, RemoveEmptyEntries) without allocating.
        // Returns the part count; more parts than fit are counted (not stored) so callers
        // still see "not 3/4 parts" for over-long inputs.
        private static int SplitTimeParts(string text, Span<int> starts, Span<int> lengths)
        {
            var count = 0;
            var start = -1;
            for (var i = 0; i <= text.Length; i++)
            {
                var isSeparator = i == text.Length || text[i] == ':' || text[i] == ',' || text[i] == '.';
                if (isSeparator)
                {
                    if (start >= 0)
                    {
                        if (count < starts.Length)
                        {
                            starts[count] = start;
                            lengths[count] = i - start;
                        }

                        count++;
                        start = -1;
                    }
                }
                else if (start < 0)
                {
                    start = i;
                }
            }

            return count;
        }

        // The fraction part parses like PadRight(3, '0') + int.TryParse always did: "5" -> 500,
        // "12" -> 120, three or more digits as-is. Padding into a stack buffer keeps the exact
        // historical semantics for odd inputs too - " " pads to " 00" (leading whitespace, parses
        // as 0) while "5 " pads to "5 0" (embedded whitespace, fails); a hand-rolled whitespace
        // check here previously got the all-whitespace case wrong.
        private static bool TryParsePaddedMilliseconds(ReadOnlySpan<char> part, out int milliseconds)
        {
            if (part.Length >= 3)
            {
                return int.TryParse(part, out milliseconds);
            }

            Span<char> padded = stackalloc char[3];
            part.CopyTo(padded);
            for (var i = part.Length; i < 3; i++)
            {
                padded[i] = '0';
            }

            return int.TryParse(padded, out milliseconds);
        }

        public TimeCode()
        {
        }

        public TimeCode(TimeSpan timeSpan)
        {
            TotalMilliseconds = timeSpan.TotalMilliseconds;
        }

        public TimeCode(double totalMilliseconds)
        {
            TotalMilliseconds = totalMilliseconds;
        }

        public TimeCode(int hours, int minutes, int seconds, int milliseconds)
        {
            TotalMilliseconds = hours * 60 * 60 * BaseUnit + minutes * 60 * BaseUnit + seconds * BaseUnit + milliseconds;
        }

        public int Hours
        {
            get
            {
                var ts = TimeSpan;
                return ts.Hours + ts.Days * 24;
            }
            set
            {
                var ts = TimeSpan;
                TotalMilliseconds = new TimeSpan(ts.Days, value, ts.Minutes, ts.Seconds, ts.Milliseconds).TotalMilliseconds;
            }
        }

        public int Minutes
        {
            get => TimeSpan.Minutes;
            set
            {
                var ts = TimeSpan;
                TotalMilliseconds = new TimeSpan(ts.Days, ts.Hours, value, ts.Seconds, ts.Milliseconds).TotalMilliseconds;
            }
        }

        public int Seconds
        {
            get => TimeSpan.Seconds;
            set
            {
                var ts = TimeSpan;
                TotalMilliseconds = new TimeSpan(ts.Days, ts.Hours, ts.Minutes, value, ts.Milliseconds).TotalMilliseconds;
            }
        }

        public int Milliseconds
        {
            get => TimeSpan.Milliseconds;
            set
            {
                var ts = TimeSpan;
                TotalMilliseconds = new TimeSpan(ts.Days, ts.Hours, ts.Minutes, ts.Seconds, value).TotalMilliseconds;
            }
        }

        public double TotalMilliseconds { get; set; }

        public double TotalSeconds
        {
            get => TotalMilliseconds / BaseUnit;
            set => TotalMilliseconds = value * BaseUnit;
        }

        public TimeSpan TimeSpan
        {
            get
            {
                if (TotalMilliseconds > MaxTimeTotalMilliseconds || TotalMilliseconds < -MaxTimeTotalMilliseconds)
                {
                    return new TimeSpan();
                }

                return TimeSpan.FromMilliseconds(TotalMilliseconds);
            }
            set => TotalMilliseconds = value.TotalMilliseconds;
        }

        public override string ToString() => ToString(false);

        public string ToString(bool localize)
        {
            var ts = TimeSpan;
            var decimalSeparator = localize ? CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator : ",";
            var s = FormatTime(ts.Hours + ts.Days * 24, 2, true, ts.Minutes, 2, true, ts.Seconds, 2, decimalSeparator, ts.Milliseconds);

            return PrefixSign(s);
        }

        public string ToShortString(bool localize = false)
        {
            // Runs for the grid's start/end/duration/gap cells on every repaint and for the
            // waveform paragraph labels on every frame. The previous version built the string
            // via interpolation - which on netstandard2.1 compiles to string.Format (boxing +
            // per-call format parsing) - plus a second interpolated "is this zero" sentinel
            // string per call; format into a stack buffer instead.
            var ts = TimeSpan;
            var decimalSeparator = localize ? CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator : ",";
            string s;
            if (ts.Minutes == 0 && ts.Hours == 0 && ts.Days == 0)
            {
                s = FormatTime(0, 0, false, 0, 0, false, ts.Seconds, 1, decimalSeparator, ts.Milliseconds);

                if (ts.Seconds == 0 && ts.Milliseconds == 0)
                {
                    return s; // no sign
                }
            }
            else if (ts.Hours == 0 && ts.Days == 0)
            {
                // never all-zero here: Minutes != 0 in this branch
                s = FormatTime(0, 0, false, ts.Minutes, 1, true, ts.Seconds, 2, decimalSeparator, ts.Milliseconds);
            }
            else
            {
                // never all-zero here: Hours/Days != 0 in this branch
                s = FormatTime(ts.Hours + ts.Days * 24, 1, true, ts.Minutes, 2, true, ts.Seconds, 2, decimalSeparator, ts.Milliseconds);
            }

            return PrefixSign(s);
        }

        // Formats [hours:][minutes:]seconds<separator>milliseconds. Negative components render
        // like "{value:00}" does ("-05"); PrefixSign then normalizes the sign as before.
        private static string FormatTime(int hours, int hoursMinDigits, bool includeHours,
            int minutes, int minutesMinDigits, bool includeMinutes,
            int seconds, int secondsMinDigits, string decimalSeparator, int milliseconds)
        {
            Span<char> buffer = stackalloc char[48 + decimalSeparator.Length];
            var pos = 0;
            if (includeHours)
            {
                pos = WriteInt(buffer, pos, hours, hoursMinDigits);
                buffer[pos++] = ':';
            }

            if (includeMinutes)
            {
                pos = WriteInt(buffer, pos, minutes, minutesMinDigits);
                buffer[pos++] = ':';
            }

            pos = WriteInt(buffer, pos, seconds, secondsMinDigits);
            foreach (var c in decimalSeparator)
            {
                buffer[pos++] = c;
            }

            pos = WriteInt(buffer, pos, milliseconds, 3);
            return new string(buffer.Slice(0, pos));
        }

        private static int WriteInt(Span<char> buffer, int pos, int value, int minDigits)
        {
            if (value < 0)
            {
                buffer[pos++] = '-';
                value = -value;
            }

            var digits = 1;
            for (var v = value; v >= 10; v /= 10)
            {
                digits++;
            }

            if (digits < minDigits)
            {
                digits = minDigits;
            }

            for (var i = pos + digits - 1; i >= pos; i--)
            {
                buffer[i] = (char)('0' + value % 10);
                value /= 10;
            }

            return pos + digits;
        }

        public string ToShortStringHHMMSSFF()
        {
            var s = ToHHMMSSFF();
            var pre = string.Empty;
            if (s.StartsWith('-'))
            {
                pre = "-";
                s = s.TrimStart('-');
            }

            var j = 0;
            var len = s.Length;
            while (j + 6 < len && s[j] == '0' && s[j + 1] == '0' && s[j + 2] == ':')
            {
                j += 3;
            }
            s = j > 0 ? s.Substring(j) : s;
            return pre + s;
        }

        public string ToHHMMSSFF() => ToFrameString(':');

        public string ToHHMMSS() => ToFrameString(':', includeFrames: false);

        /// <summary>
        /// Only 29.97 and 59.94 have drop-frame numbering (ST 12-1). 23.976 is 1000/1001 too, but
        /// the standard gives it none, so it counts every frame like 24, 25, 30, 50 and 60. The
        /// tolerance matches GetFrameForCalculation - a wider one would number frames at one rate
        /// and label them at another.
        /// </summary>
        public static bool IsDropFrameRate(double frameRate) =>
            Math.Abs(frameRate - 29.97) < 0.001 || Math.Abs(frameRate - 59.94) < 0.001;

        /// <summary>
        /// Labels skipped at each minute that is not a tenth one - 2 at 29.97, 4 at 59.94, and 0
        /// at a rate without a drop-frame mode so a caller that forgets the check gets plain labels.
        /// </summary>
        public static int DropFrameSkipCount(double frameRate) =>
            Math.Abs(frameRate - 59.94) < 0.001 ? 4 :
            Math.Abs(frameRate - 29.97) < 0.001 ? 2 : 0;

        /// <summary>
        /// The rate the formatters count with: 30000/1001 for 29.97, or the whole-number time code
        /// clock in SMPTE timing, where the stored times are on that clock too.
        /// </summary>
        private static double FrameRateForCalculation(double frameRate) =>
            SubtitleFormat.GetTimeCodeFrameRate(frameRate);

        /// <summary>
        /// Frame number on screen at this time, clamped to 0 .. 99:59:59.999. Past that the frame
        /// count would keep growing while <see cref="TimeSpan"/> renders zero, and a negative time
        /// has no frame of its own; the caller adds the sign.
        /// </summary>
        private static int RoundToFrame(double totalMilliseconds, double frameRate) =>
            (int)Math.Round(
                Math.Min(MaxTimeTotalMilliseconds, Math.Max(0, totalMilliseconds)) * FrameRateForCalculation(frameRate) / 1000.0,
                MidpointRounding.AwayFromZero);

        /// <summary>
        /// Label number of the frame at this time - a count of labels, not of frames. The first
        /// minute of every ten keeps all 1800 labels (3600 at 59.94) and the other nine skip 2 (4),
        /// which keeps the label within 2 frames of the clock (4 at 59.94) instead of drifting
        /// 3.6 s an hour. A ten-minute block is 17982 frames at 29.97 and 35964 at 59.94 - the
        /// identity FFmpeg's av_timecode_adjust_ntsc_framenum2 uses.
        /// </summary>
        public static int DropFrameLabelNumber(double totalMilliseconds, double frameRate)
        {
            var frame = RoundToFrame(totalMilliseconds, frameRate);
            var drop = DropFrameSkipCount(frameRate);
            var firstMinute = (int)Math.Round(frameRate) * 60;
            var otherMinute = firstMinute - drop;
            var framesPer10Minutes = firstMinute + 9 * otherMinute;

            var block = frame / framesPer10Minutes;
            var rest = frame % framesPer10Minutes;
            var minutes = block * 10 + (rest < firstMinute ? 0 : 1 + (rest - firstMinute) / otherMinute);

            return frame + drop * (minutes - minutes / 10);
        }

        /// <summary>
        /// The same label as <see cref="ToHHMMSSFF()"/> with a ';' before the frame field. It only
        /// changes the separator - the numbers stay plain, so at 29.97 this reports the wall clock
        /// rounded onto a 30 fps grid rather than the frame that is on screen. Use
        /// <see cref="ToDropFrameString()"/> when the label is meant to be read as drop frame.
        /// </summary>
        public string ToHHMMSSFFDropFrame() => ToFrameString(';');

        /// <summary>
        /// Time code of this time in drop-frame numbering, e.g. "00:01:00;02" - the label of the
        /// frame on screen, which drop frame keeps in step with the clock by skipping numbers.
        /// Falls back to a plain ":" time code at a rate without a drop-frame mode.
        /// </summary>
        public string ToDropFrameString()
        {
            var frameRate = Configuration.Settings.General.CurrentFrameRate;
            if (!IsDropFrameRate(frameRate))
            {
                return ToFrameString(':');
            }

            // A negative time has no label either - format the absolute one, as the rest do.
            var negative = TotalMilliseconds < 0;
            var framesPerSecond = (int)Math.Round(frameRate);
            var label = DropFrameLabelNumber(Math.Abs(TotalMilliseconds), frameRate);
            var totalSeconds = label / framesPerSecond;
            var s = $"{totalSeconds / 3600:00}:{totalSeconds / 60 % 60:00}:{totalSeconds % 60:00};{label % framesPerSecond:00}";
            return negative ? "-" + s : s;
        }

        /// <summary>
        /// The label one frame on, for the out point of a one-frame event. Counted from the label
        /// rather than by adding a frame duration: a plain label is built from whole milliseconds,
        /// so at 23.976 the 41.708 ms step truncates to 41 or 42 and can round back onto the label
        /// it started from. At a rate without a drop-frame mode the result can be one away from
        /// the next frame's label, but it is never the label it started from.
        /// </summary>
        public string ToNextFrameString()
        {
            var frameRate = Configuration.Settings.General.CurrentFrameRate;
            var dropFrame = IsDropFrameRate(frameRate);
            var drop = DropFrameSkipCount(frameRate);
            var framesPerSecond = Math.Max(1, (int)Math.Round(frameRate));

            var label = ToDropFrameString();
            var negative = label.StartsWith("-", StringComparison.Ordinal);
            if (negative)
            {
                label = label.Substring(1);
            }

            var parts = label.Split(':', ';');
            var frames = int.Parse(parts[3], CultureInfo.InvariantCulture);
            var seconds = int.Parse(parts[2], CultureInfo.InvariantCulture);
            var minutes = int.Parse(parts[1], CultureInfo.InvariantCulture);
            var hours = int.Parse(parts[0], CultureInfo.InvariantCulture);

            // A negative time smaller than half a frame has no label before it - one frame on is
            // zero itself, and counting back from it would underflow into a second sign.
            if (negative && hours == 0 && minutes == 0 && seconds == 0 && frames == 0)
            {
                return label;
            }

            // A negative time runs toward zero, so its label comes back one rather than going on
            // one - and coming back can land on a label drop frame skips, which borrows a second.
            var step = negative ? -1 : 1;
            frames += step;
            if (frames >= framesPerSecond || frames < 0 ||
                (frames < drop && seconds == 0 && minutes % 10 != 0))
            {
                frames = step > 0 ? 0 : framesPerSecond - 1;
                seconds += step;
                if (seconds >= 60 || seconds < 0)
                {
                    seconds = step > 0 ? 0 : 59;
                    minutes += step;
                    if (minutes >= 60 || minutes < 0)
                    {
                        minutes = step > 0 ? 0 : 59;
                        hours += step;
                    }

                    // The first 'drop' labels of a minute that is not a tenth one do not exist.
                    if (step > 0 && dropFrame && minutes % 10 != 0)
                    {
                        frames = drop;
                    }
                }
            }

            var separator = dropFrame ? ';' : ':';
            var next = $"{hours:00}:{minutes:00}:{seconds:00}{separator}{frames:00}";

            // Zero has one spelling: the "-" belongs to the label this started from.
            var steppedToZero = hours == 0 && minutes == 0 && seconds == 0 && frames == 0;
            return negative && !steppedToZero ? "-" + next : next;
        }

        /// <summary>
        /// Inverse of <see cref="ToDropFrameString()"/>. The project rate decides whether the label
        /// counts drop frame; at a rate without that mode it is read as a plain label, so labels
        /// that skip numbers cannot be placed by a conversion done at such a rate.
        /// </summary>
        public static TimeCode FromDropFrameLabel(int hours, int minutes, int seconds, int frames)
        {
            var frameRate = Configuration.Settings.General.CurrentFrameRate;
            var framesPerSecond = Math.Max(1, (int)Math.Round(frameRate));
            var label = ((long)hours * 3600 + minutes * 60 + seconds) * framesPerSecond + frames;
            if (!IsDropFrameRate(frameRate))
            {
                return new TimeCode(hours, minutes, seconds, SubtitleFormat.FramesToMillisecondsMax999(frames));
            }

            var drop = DropFrameSkipCount(frameRate);
            var firstMinute = framesPerSecond * 60;
            var otherMinute = firstMinute - drop;
            var framesPer10Minutes = firstMinute + 9 * otherMinute;
            var labelsPer10Minutes = framesPerSecond * 600;
            var block = label / labelsPer10Minutes;
            var rest = label % labelsPer10Minutes;
            // The frame number is long, not int: an "hh" field is only bounded by the caller's
            // parser, and a 999999-hour one overflows an int many times over.
            var frame = block * framesPer10Minutes + rest - drop * (rest / firstMinute);
            return new TimeCode(frame * 1000.0 / FrameRateForCalculation(frameRate));
        }

        public string ToSSFF() => ToFrameString(':', includeHoursAndMinutes: false);

        public string ToHHMMSSPeriodFF() => ToFrameString('.');

        // The five frame-based formats above were five copies of this method that differed only
        // in the separator and which components they include - including the "rounds up to a whole
        // second" carry, which is easy to get subtly wrong in one copy only.
        private string ToFrameString(char frameSeparator, bool includeHoursAndMinutes = true, bool includeFrames = true)
        {
            var ts = TimeSpan;
            var frameRate = Configuration.Settings.General.CurrentFrameRate;
            var frames = Math.Round(ts.Milliseconds / (BaseUnit / frameRate));

            string s;
            if (frames >= frameRate - 0.001)
            {
                // Carry into the next second. ToSSFF deliberately does not carry into
                // minutes/hours, matching what it did as a separate method.
                if (includeHoursAndMinutes)
                {
                    var newTs = ts.Add(new TimeSpan(0, 0, 1));
                    s = FormatFrameTime(newTs.Days * 24 + newTs.Hours, newTs.Minutes, newTs.Seconds, 0, frameSeparator, true, includeFrames);
                }
                else
                {
                    s = FormatFrameTime(0, 0, ts.Seconds + 1, 0, frameSeparator, false, includeFrames);
                }
            }
            else
            {
                // ToHHMMSS drops the frame component entirely, so it never computed one.
                var frameComponent = includeFrames ? SubtitleFormat.MillisecondsToFramesMaxFrameRate(ts.Milliseconds) : 0;
                s = includeHoursAndMinutes
                    ? FormatFrameTime(ts.Days * 24 + ts.Hours, ts.Minutes, ts.Seconds, frameComponent, frameSeparator, true, includeFrames)
                    : FormatFrameTime(0, 0, ts.Seconds, frameComponent, frameSeparator, false, includeFrames);
            }

            return PrefixSign(s);
        }

        // Formats [hh:mm:]ss[<separator>ff] for the frame-based display family above. Same
        // stack-buffer approach as FormatTime: these run for every start/end/duration/gap cell
        // repaint when the HH:MM:SS:FF time format is active, and the interpolated strings they
        // replace compile to string.Format on netstandard2.1 (boxed ints + format parsing).
        private static string FormatFrameTime(int hours, int minutes, int seconds, int frames,
            char frameSeparator, bool includeHoursAndMinutes = true, bool includeFrames = true)
        {
            Span<char> buffer = stackalloc char[40];
            var pos = 0;
            if (includeHoursAndMinutes)
            {
                pos = WriteInt(buffer, pos, hours, 2);
                buffer[pos++] = ':';
                pos = WriteInt(buffer, pos, minutes, 2);
                buffer[pos++] = ':';
            }

            pos = WriteInt(buffer, pos, seconds, 2);
            if (includeFrames)
            {
                buffer[pos++] = frameSeparator;
                pos = WriteInt(buffer, pos, frames, 2);
            }

            return new string(buffer.Slice(0, pos));
        }

        // The negative path used to be $"-{time.RemoveChar('-')}", which allocated a char[], the
        // stripped string and the interpolated result on top of the string being fixed up - 208
        // bytes where the positive path costs 48. Write the sign and the kept characters straight
        // into one buffer instead. The formatters above never produce a time longer than their
        // stack buffers, so a rented span is not worth it.
        private string PrefixSign(string time)
        {
            if (TotalMilliseconds >= 0)
            {
                return time;
            }

            const int maxStackChars = 64;
            if (time.Length >= maxStackChars)
            {
                return "-" + time.RemoveChar('-'); // not reachable from the formatters; no unbounded stackalloc
            }

            Span<char> buffer = stackalloc char[time.Length + 1];
            buffer[0] = '-';
            var pos = 1;
            foreach (var c in time)
            {
                if (c != '-')
                {
                    buffer[pos++] = c;
                }
            }

            return new string(buffer.Slice(0, pos));
        }

        public string ToDisplayString()
        {
            if (IsMaxTime)
            {
                return "-";
            }

            if (Configuration.Settings?.General.UseTimeFormatHHMMSSFF == true)
            {
                return ToHHMMSSFF();
            }

            return ToString(true);
        }

        public string ToShortDisplayString()
        {
            if (IsMaxTime)
            {
                return "-";
            }

            if (Configuration.Settings?.General.UseTimeFormatHHMMSSFF == true)
            {
                return ToShortStringHHMMSSFF();
            }

            return ToShortString(true);
        }

        /// <summary>
        /// Align time to frame rate.
        /// </summary>
        public TimeCode AlignToFrame()
        {
            var ts = TimeSpan.FromMilliseconds(Math.Round(TotalMilliseconds, MidpointRounding.AwayFromZero));
            var frames = SubtitleFormat.MillisecondsToFrames(ts.Milliseconds);
            TimeSpan ts2;
            if (frames >= Configuration.Settings.General.CurrentFrameRate - 0.001)
            {
                ts = ts.Add(new TimeSpan(0, 0, 1));
                ts2 = new TimeSpan(ts.Days, ts.Hours, ts.Minutes, ts.Seconds, 0);
            }
            else
            {
                var ms = SubtitleFormat.FramesToMillisecondsMax999(SubtitleFormat.MillisecondsToFramesMaxFrameRate(ts.Milliseconds));
                ts2 = new TimeSpan(ts.Days, ts.Hours, ts.Minutes, ts.Seconds, ms);
            }

            return new TimeCode(ts2.TotalMilliseconds);
        }
    }
}
