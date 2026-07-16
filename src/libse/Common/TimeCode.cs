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
            var parts = text.Split(TimeSplitChars, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 4)
            {
                var msString = parts[3].PadRight(3,'0');
                if (int.TryParse(parts[0], out var hours) && int.TryParse(parts[1], out var minutes) && int.TryParse(parts[2], out var seconds) && int.TryParse(msString, out var milliseconds))
                {
                    return new TimeSpan(0, hours, minutes, seconds, milliseconds).TotalMilliseconds;
                }
            }

            if (parts.Length == 3)
            {
                var msString = parts[2].PadRight(3, '0');
                if (int.TryParse(parts[0], out var minutes) && int.TryParse(parts[1], out var seconds) && int.TryParse(msString, out var milliseconds))
                {
                    return new TimeSpan(0, 0, minutes, seconds, milliseconds).TotalMilliseconds;
                }
            }

            return 0;
        }

        public static double ParseHHMMSSFFToMilliseconds(string text)
        {
            var parts = text.Split(TimeSplitChars, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 4)
            {
                if (int.TryParse(parts[0], out var hours) && int.TryParse(parts[1], out var minutes) && int.TryParse(parts[2], out var seconds) && int.TryParse(parts[3], out var frames))
                {
                    return new TimeCode(hours, minutes, seconds, SubtitleFormat.FramesToMillisecondsMax999(frames)).TotalMilliseconds;
                }
            }
            return 0;
        }

        public static double ParseHHMMSSToMilliseconds(string text)
        {
            var parts = text.Split(TimeSplitChars, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 3)
            {
                if (int.TryParse(parts[0], out var hours) && int.TryParse(parts[1], out var minutes) && int.TryParse(parts[2], out var seconds))
                {
                    return new TimeCode(hours, minutes, seconds, 0).TotalMilliseconds;
                }
            }

            return 0;
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

        public string ToHHMMSSFF()
        {
            string s;
            var ts = TimeSpan;
            var frameRate = Configuration.Settings.General.CurrentFrameRate;
            var frames = Math.Round(ts.Milliseconds / (BaseUnit / frameRate));
            if (frames >= frameRate - 0.001)
            {
                var newTs = ts.Add(new TimeSpan(0, 0, 1));
                s = FormatFrameTime(newTs.Days * 24 + newTs.Hours, newTs.Minutes, newTs.Seconds, 0, ':');
            }
            else
            {
                s = FormatFrameTime(ts.Days * 24 + ts.Hours, ts.Minutes, ts.Seconds, SubtitleFormat.MillisecondsToFramesMaxFrameRate(ts.Milliseconds), ':');
            }

            return PrefixSign(s);
        }

        public string ToHHMMSS()
        {
            string s;
            var ts = TimeSpan;
            var frameRate = Configuration.Settings.General.CurrentFrameRate;
            var frames = Math.Round(ts.Milliseconds / (BaseUnit / frameRate));
            if (frames >= frameRate - 0.001)
            {
                var newTs = ts.Add(new TimeSpan(0, 0, 1));
                s = FormatFrameTime(newTs.Days * 24 + newTs.Hours, newTs.Minutes, newTs.Seconds, 0, ':', includeFrames: false);
            }
            else
            {
                s = FormatFrameTime(ts.Days * 24 + ts.Hours, ts.Minutes, ts.Seconds, 0, ':', includeFrames: false);
            }
            return PrefixSign(s);
        }

        public string ToHHMMSSFFDropFrame()
        {
            string s;
            var ts = TimeSpan;
            var frameRate = Configuration.Settings.General.CurrentFrameRate;
            var frames = Math.Round(ts.Milliseconds / (BaseUnit / frameRate));
            if (frames >= frameRate - 0.001)
            {
                var newTs = ts.Add(new TimeSpan(0, 0, 1));
                s = FormatFrameTime(newTs.Days * 24 + newTs.Hours, newTs.Minutes, newTs.Seconds, 0, ';');
            }
            else
            {
                s = FormatFrameTime(ts.Days * 24 + ts.Hours, ts.Minutes, ts.Seconds, SubtitleFormat.MillisecondsToFramesMaxFrameRate(ts.Milliseconds), ';');
            }
            return PrefixSign(s);
        }

        public string ToSSFF()
        {
            string s;
            var ts = TimeSpan;
            var frameRate = Configuration.Settings.General.CurrentFrameRate;
            var frames = Math.Round(ts.Milliseconds / (BaseUnit / frameRate));
            if (frames >= frameRate - 0.001)
            {
                s = FormatFrameTime(0, 0, ts.Seconds + 1, 0, ':', includeHoursAndMinutes: false);
            }
            else
            {
                s = FormatFrameTime(0, 0, ts.Seconds, SubtitleFormat.MillisecondsToFramesMaxFrameRate(ts.Milliseconds), ':', includeHoursAndMinutes: false);
            }

            return PrefixSign(s);
        }

        public string ToHHMMSSPeriodFF()
        {
            string s;
            var ts = TimeSpan;
            var frameRate = Configuration.Settings.General.CurrentFrameRate;
            var frames = Math.Round(ts.Milliseconds / (BaseUnit / frameRate));
            if (frames >= frameRate - 0.001)
            {
                var newTs = ts.Add(new TimeSpan(0, 0, 1));
                s = FormatFrameTime(newTs.Days * 24 + newTs.Hours, newTs.Minutes, newTs.Seconds, 0, '.');
            }
            else
            {
                s = FormatFrameTime(ts.Days * 24 + ts.Hours, ts.Minutes, ts.Seconds, SubtitleFormat.MillisecondsToFramesMaxFrameRate(ts.Milliseconds), '.');
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

        private string PrefixSign(string time) => TotalMilliseconds >= 0 ? time : $"-{time.RemoveChar('-')}";

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
