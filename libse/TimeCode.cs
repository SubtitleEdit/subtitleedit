﻿using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Core
{
    public class TimeCode
    {
        public static readonly TimeCode MaxTime = new TimeCode(99, 59, 59, 999);

        public const double BaseUnit = 1000.0; // Base unit of time
        private double _totalMilliseconds;

        public bool IsMaxTime
        {
            get
            {
                return Math.Abs(_totalMilliseconds - MaxTime.TotalMilliseconds) < 0.01;
            }
        }

        public static TimeCode FromSeconds(double seconds)
        {
            return new TimeCode(seconds * BaseUnit);
        }

        public static double ParseToMilliseconds(string text)
        {
            string[] parts = text.Split(new[] { ':', ',', '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 4)
            {
                int hours;
                int minutes;
                int seconds;
                int milliseconds;
                if (int.TryParse(parts[0], out hours) && int.TryParse(parts[1], out minutes) && int.TryParse(parts[2], out seconds) && int.TryParse(parts[3], out milliseconds))
                {
                    return new TimeSpan(0, hours, minutes, seconds, milliseconds).TotalMilliseconds;
                }
            }
            return 0;
        }

        public static double ParseHHMMSSFFToMilliseconds(string text)
        {
            string[] parts = text.Split(new[] { ':', ',', '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 4)
            {
                int hours;
                int minutes;
                int seconds;
                int frames;
                if (int.TryParse(parts[0], out hours) && int.TryParse(parts[1], out minutes) && int.TryParse(parts[2], out seconds) && int.TryParse(parts[3], out frames))
                {
                    return new TimeCode(hours, minutes, seconds, SubtitleFormat.FramesToMillisecondsMax999(frames)).TotalMilliseconds;
                }
            }
            return 0;
        }

        public TimeCode()
        {
        }

        public TimeCode(TimeSpan timeSpan)
        {
            _totalMilliseconds = timeSpan.TotalMilliseconds;
        }

        public TimeCode(double totalMilliseconds)
        {
            _totalMilliseconds = totalMilliseconds;
        }

        public TimeCode(int hours, int minutes, int seconds, int milliseconds)
        {
            _totalMilliseconds = hours * 60 * 60 * BaseUnit + minutes * 60 * BaseUnit + seconds * BaseUnit + milliseconds;
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
            get
            {
                return TimeSpan.Minutes;
            }
            set
            {
                var ts = TimeSpan;
                _totalMilliseconds = new TimeSpan(ts.Days, ts.Hours, value, ts.Seconds, ts.Milliseconds).TotalMilliseconds;
            }
        }

        public int Seconds
        {
            get
            {
                return TimeSpan.Seconds;
            }
            set
            {
                var ts = TimeSpan;
                _totalMilliseconds = new TimeSpan(ts.Days, ts.Hours, ts.Minutes, value, ts.Milliseconds).TotalMilliseconds;
            }
        }

        public int Milliseconds
        {
            get
            {
                return TimeSpan.Milliseconds;
            }
            set
            {
                var ts = TimeSpan;
                _totalMilliseconds = new TimeSpan(ts.Days, ts.Hours, ts.Minutes, ts.Seconds, value).TotalMilliseconds;
            }
        }

        public double TotalMilliseconds
        {
            get { return _totalMilliseconds; }
            set { _totalMilliseconds = value; }
        }

        public double TotalSeconds
        {
            get { return _totalMilliseconds / BaseUnit; }
            set { _totalMilliseconds = value * BaseUnit; }
        }

        public TimeSpan TimeSpan
        {
            get
            {
                return TimeSpan.FromMilliseconds(_totalMilliseconds);
            }
            set
            {
                _totalMilliseconds = value.TotalMilliseconds;
            }
        }

        public void AddTime(int hours, int minutes, int seconds, int milliseconds)
        {
            Hours += hours;
            Minutes += minutes;
            Seconds += seconds;
            Milliseconds += milliseconds;
        }

        public override string ToString() => ToString(false);

        public string ToString(bool localize)
        {
            var ts = TimeSpan;
            string decimalSeparator = localize ? CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator : ",";
            string s = string.Format("{0:00}:{1:00}:{2:00}{3}{4:000}", ts.Hours + ts.Days * 24, ts.Minutes, ts.Seconds, decimalSeparator, ts.Milliseconds);

            if (TotalMilliseconds >= 0)
                return s;
            return "-" + s.Replace("-", string.Empty);
        }

        public string ToShortString(bool localize = false)
        {
            var ts = TimeSpan;
            string decimalSeparator = localize ? CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator : ",";
            string s;
            if (ts.Minutes == 0 && ts.Hours == 0 && ts.Days == 0)
                s = string.Format("{0:0}{1}{2:000}", ts.Seconds, decimalSeparator, ts.Milliseconds);
            else if (ts.Hours == 0 && ts.Days == 0)
                s = string.Format("{0:0}:{1:00}{2}{3:000}", ts.Minutes, ts.Seconds, decimalSeparator, ts.Milliseconds);
            else
                s = string.Format("{0:0}:{1:00}:{2:00}{3}{4:000}", ts.Hours + ts.Days * 24, ts.Minutes, ts.Seconds, decimalSeparator, ts.Milliseconds);

            if (TotalMilliseconds >= 0)
                return s;
            return "-" + s.Replace("-", string.Empty);
        }

        public string ToShortStringHHMMSSFF()
        {
            string s = ToHHMMSSFF();
            int j = 0;
            int len = s.Length;
            while (j + 6 < len && s[j] == '0' && s[j + 1] == '0' && s[j + 2] == ':')
            {
                j += 3;
            }
            return j > 0 ? s.Substring(j) : s;
        }

        public string ToHHMMSSFF()
        {
            var ts = TimeSpan;
            var frames = Math.Round(ts.Milliseconds / (BaseUnit / Configuration.Settings.General.CurrentFrameRate));
            if (frames >= Configuration.Settings.General.CurrentFrameRate - 0.001)
                return string.Format("{0:00}:{1:00}:{2:00}:{3:00}", ts.Hours, ts.Minutes, ts.Seconds + 1, 0);
            return string.Format("{0:00}:{1:00}:{2:00}:{3:00}", ts.Hours, ts.Minutes, ts.Seconds, SubtitleFormat.MillisecondsToFramesMaxFrameRate(ts.Milliseconds));
        }

        public string ToSSFF()
        {
            var ts = TimeSpan;
            var frames = Math.Round(ts.Milliseconds / (BaseUnit / Configuration.Settings.General.CurrentFrameRate));
            if (frames >= Configuration.Settings.General.CurrentFrameRate - 0.001)
                return string.Format("{0:00}:{1:00}", ts.Seconds + 1, 0);
            return string.Format("{0:00}:{1:00}", ts.Seconds, SubtitleFormat.MillisecondsToFramesMaxFrameRate(ts.Milliseconds));
        }

        public string ToHHMMSSPeriodFF()
        {
            var ts = TimeSpan;
            var frames = Math.Round(ts.Milliseconds / (BaseUnit / Configuration.Settings.General.CurrentFrameRate));
            if (frames >= Configuration.Settings.General.CurrentFrameRate - 0.001)
                return string.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds + 1, 0);
            return string.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, SubtitleFormat.MillisecondsToFramesMaxFrameRate(ts.Milliseconds));
        }

        public string ToDisplayString()
        {
            if (IsMaxTime)
                return "-";

            if (Configuration.Settings?.General.UseTimeFormatHHMMSSFF == true)
                return ToHHMMSSFF();

            return ToString(true);
        }

        public string ToShortDisplayString()
        {
            if (IsMaxTime)
                return "-";

            if (Configuration.Settings?.General.UseTimeFormatHHMMSSFF == true)
                return ToShortStringHHMMSSFF();

            return ToShortString(true);
        }

    }
}
