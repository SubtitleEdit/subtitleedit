using Nikse.SubtitleEdit.Core.Common.TimeFormatters;
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

        public string ToString(bool localize) => TimeSpan.ToString(localize);

        public string ToShortString(bool localize = false) => TimeSpan.ToShortString(localize);

        public string ToString(ITimeFormatter formatter) => formatter.Format(TimeSpan);

        internal static string PrefixSign(string time, double totalMilliseconds) => totalMilliseconds >= 0 ? time : $"-{time.RemoveChar('-')}";

        private string PrefixSign(string time) => PrefixSign(time, TotalMilliseconds);

        public string ToDisplayString()
        {
            if (IsMaxTime)
            {
                return "-";
            }

            if (Configuration.Settings?.General.UseTimeFormatHHMMSSFF == true)
            {
                return ToString(TimeFormatter.HhMmSsFf);
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
                return ToString(TimeFormatter.ShortHhMmSsFf);
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
