﻿using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Core
{
    public class TimeCode
    {
        private static readonly char[] TimeSplitChars = { ':', ',', '.' };
        public const double BaseUnit = 1000.0; // Base unit of time
        private double _totalMilliseconds;

        public bool IsMaxTime => Math.Abs(_totalMilliseconds - MaxTimeTotalMilliseconds) < 0.01;
        public const double MaxTimeTotalMilliseconds = 359999999; // new TimeCode(99, 59, 59, 999).TotalMilliseconds

        public static TimeCode FromSeconds(double seconds)
        {
            return new TimeCode(seconds * BaseUnit);
        }

        public static double ParseToMilliseconds(string text)
        {
            var parts = text.Split(TimeSplitChars, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 4)
            {
                if (int.TryParse(parts[0], out var hours) && int.TryParse(parts[1], out var minutes) && int.TryParse(parts[2], out var seconds) && int.TryParse(parts[3], out var milliseconds))
                {
                    return new TimeSpan(0, hours, minutes, seconds, milliseconds).TotalMilliseconds;
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

        public TimeCode()
        {
        }

        public TimeCode(TimeSpan ts) :
            this(ts.TotalMilliseconds)
        {
        }

        public TimeCode(double totalMilliseconds)
        {
            _totalMilliseconds = totalMilliseconds;
        }

        public TimeCode(int hours, int minutes, int seconds, int milliseconds) :
            this(BaseUnit * (60 * (hours * 60 + minutes) + seconds) + milliseconds)
        {
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
                _totalMilliseconds = new TimeSpan(ts.Days, value, ts.Minutes, ts.Seconds, ts.Milliseconds).TotalMilliseconds;
            }
        }

        public int Minutes
        {
            get => TimeSpan.Minutes;
            set
            {
                var ts = TimeSpan;
                _totalMilliseconds = new TimeSpan(ts.Days, ts.Hours, value, ts.Seconds, ts.Milliseconds).TotalMilliseconds;
            }
        }

        public int Seconds
        {
            get => TimeSpan.Seconds;
            set
            {
                var ts = TimeSpan;
                _totalMilliseconds = new TimeSpan(ts.Days, ts.Hours, ts.Minutes, value, ts.Milliseconds).TotalMilliseconds;
            }
        }

        public int Milliseconds
        {
            get => TimeSpan.Milliseconds;
            set
            {
                var ts = TimeSpan;
                _totalMilliseconds = new TimeSpan(ts.Days, ts.Hours, ts.Minutes, ts.Seconds, value).TotalMilliseconds;
            }
        }

        public double TotalMilliseconds
        {
            get => _totalMilliseconds;
            set => _totalMilliseconds = value;
        }

        public double TotalSeconds
        {
            get => _totalMilliseconds / BaseUnit;
            set => _totalMilliseconds = value * BaseUnit;
        }

        public TimeSpan TimeSpan
        {
            get => TimeSpan.FromMilliseconds(_totalMilliseconds);
            set => _totalMilliseconds = value.TotalMilliseconds;
        }

        public override string ToString() => ToString(false);

        public string ToString(bool localize)
        {
            var ts = TimeSpan;
            string decimalSeparator = localize ? CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator : ",";
            return PrefixSign($"{ts.Hours + ts.Days * 24:00}:{ts.Minutes:00}:{ts.Seconds:00}{decimalSeparator}{ts.Milliseconds:000}");
        }

        public string ToShortString(bool localize = false)
        {
            var ts = TimeSpan;
            string decimalSeparator = localize ? CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator : ",";
            if (ts.Minutes == 0 && ts.Hours == 0 && ts.Days == 0)
            {
                return PrefixSign($"{ts.Seconds:0}{decimalSeparator}{ts.Milliseconds:000}");
            }
            if (ts.Hours == 0 && ts.Days == 0)
            {
                return PrefixSign($"{ts.Minutes:0}:{ts.Seconds:00}{decimalSeparator}{ts.Milliseconds:000}");
            }
            return PrefixSign($"{ts.Hours + ts.Days * 24:0}:{ts.Minutes:00}:{ts.Seconds:00}{decimalSeparator}{ts.Milliseconds:000}");
        }

        public string ToShortStringHHMMSSFF()
        {
            string s = ToHHMMSSFF();
            string pre = string.Empty;
            if (s.StartsWith('-'))
            {
                pre = "-";
                s = s.TrimStart('-');
            }
            int j = 0;
            int len = s.Length;
            while (j + 6 < len && s[j] == '0' && s[j + 1] == '0' && s[j + 2] == ':')
            {
                j += 3;
            }
            s = j > 0 ? s.Substring(j) : s;
            return pre + s;
        }

        public string ToHHMMSSFF()
        {
            var ts = TimeSpan;
            var frames = Math.Round(ts.Milliseconds / (BaseUnit / Configuration.Settings.General.CurrentFrameRate));
            if (frames >= Configuration.Settings.General.CurrentFrameRate - 0.001)
            {
                var newTs = new TimeSpan(ts.Ticks);
                newTs = newTs.Add(new TimeSpan(0, 0, 1));
                return PrefixSign($"{newTs.Days * 24 + newTs.Hours:00}:{newTs.Minutes:00}:{newTs.Seconds:00}:{0:00}");
            }
            return PrefixSign($"{ts.Days * 24 + ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}:{SubtitleFormat.MillisecondsToFramesMaxFrameRate(ts.Milliseconds):00}");
        }

        public string ToSSFF()
        {
            var ts = TimeSpan;
            var frames = Math.Round(ts.Milliseconds / (BaseUnit / Configuration.Settings.General.CurrentFrameRate));
            if (frames >= Configuration.Settings.General.CurrentFrameRate - 0.001)
            {
                return PrefixSign($"{ts.Seconds + 1:00}:{0:00}");
            }
            return PrefixSign($"{ts.Seconds:00}:{SubtitleFormat.MillisecondsToFramesMaxFrameRate(ts.Milliseconds):00}");
        }

        public string ToHHMMSSPeriodFF()
        {
            var ts = TimeSpan;
            var frames = Math.Round(ts.Milliseconds / (BaseUnit / Configuration.Settings.General.CurrentFrameRate));
            if (frames >= Configuration.Settings.General.CurrentFrameRate - 0.001)
            {
                var newTs = new TimeSpan(ts.Ticks);
                newTs = newTs.Add(new TimeSpan(0, 0, 1));
                return PrefixSign($"{newTs.Days * 24 + newTs.Hours:00}:{newTs.Minutes:00}:{newTs.Seconds:00}.{0:00}");
            }
            return PrefixSign($"{ts.Days * 24 + ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{SubtitleFormat.MillisecondsToFramesMaxFrameRate(ts.Milliseconds):00}");
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

    }
}
