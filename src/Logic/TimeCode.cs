using System;
using Nikse.SubtitleEdit.Logic.SubtitleFormats;

namespace Nikse.SubtitleEdit.Logic
{
    public class TimeCode
    {
        public static TimeCode MaxTime = new TimeCode(99, 59, 59, 999);

        private double _totalMilliseconds;

        public bool IsMaxTime
        {
            get
            {
                return _totalMilliseconds == MaxTime.TotalMilliseconds;
            }
        }

        public static TimeCode FromSeconds(double seconds)
        {
            return new TimeCode(seconds * 1000.0);
        }

        public static TimeCode FromTimestampTokens(string hours, string minutes, string seconds, string milliseconds)
        {
            var ms = 0;
            if (milliseconds != null && int.TryParse(milliseconds, out ms))
            {
                if (milliseconds.Length == 2)
                {
                    // 0.01s = 1cs (centisecond)
                    ms *= 10;
                }
                else if (milliseconds.Length == 1)
                {
                    // 0.1s = 1ds (decisecond)
                    ms *= 100;
                }
            }

            int ss;
            if (seconds != null && int.TryParse(seconds, out ss))
            {
                ms += ss * 1000;
            }

            int mm;
            if (minutes != null && int.TryParse(minutes, out mm))
            {
                ms += mm * 60000;
            }

            int hh;
            if (hours != null && int.TryParse(hours, out hh))
            {
                ms += hh * 3600000;
            }

            return new TimeCode(ms);
        }

        public static TimeCode FromFrameTokens(string hours, string minutes, string seconds, string frames, double frameRate)
        {
            var ms = 0;
            if (frames != null && int.TryParse(frames, out ms))
            {
                ms = Math.Min((int)Math.Round(ms * 1000.0 / frameRate), 999);
            }

            int ss;
            if (seconds != null && int.TryParse(seconds, out ss))
            {
                ms += ss * 1000;
            }

            int mm;
            if (minutes != null && int.TryParse(minutes, out mm))
            {
                ms += mm * 60000;
            }

            int hh;
            if (hours != null && int.TryParse(hours, out hh))
            {
                ms += hh * 3600000;
            }

            return new TimeCode(ms);
        }

        public static TimeCode FromFrameTokens(string hours, string minutes, string seconds, string frames)
        {
            return FromFrameTokens(hours, minutes, seconds, frames, Configuration.Settings.General.CurrentFrameRate);
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
                    TimeSpan ts = new TimeSpan(0, hours, minutes, seconds, milliseconds);
                    return ts.TotalMilliseconds;
                }
            }
            return 0;
        }

        public static double ParseHHMMSSFFToMilliseconds(string text)
        {
            var tokens = text.Split(new[] { ':', ',', '.' }, StringSplitOptions.RemoveEmptyEntries);
            return tokens.Length == 4
                ? FromFrameTokens(tokens[0], tokens[1], tokens[2], tokens[3]).TotalMilliseconds
                : 0;
        }

        public TimeCode(TimeSpan timeSpan)
        {
            _totalMilliseconds = timeSpan.TotalMilliseconds;
        }

        public TimeCode(DateTime dateTime)
            : this(dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Millisecond)
        {
        }

        public TimeCode(double totalMilliseconds)
        {
            _totalMilliseconds = totalMilliseconds;
        }

        public TimeCode(int hour, int minute, int seconds, int milliseconds)
        {
            _totalMilliseconds = hour * 60 * 60 * 1000 + minute * 60 * 1000 + seconds * 1000 + milliseconds;
        }

        public TimeCode()
            : this(0)
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
                _totalMilliseconds = new TimeSpan(0, value, ts.Minutes, ts.Seconds, ts.Milliseconds).TotalMilliseconds;
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
                TimeSpan = new TimeSpan(0, ts.Hours, value, ts.Seconds, ts.Milliseconds);
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
                TimeSpan = new TimeSpan(0, ts.Hours, ts.Minutes, value, ts.Milliseconds);
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
                _totalMilliseconds = new TimeSpan(0, ts.Hours, ts.Minutes, ts.Seconds, value).TotalMilliseconds;
            }
        }

        public double TotalMilliseconds
        {
            get { return _totalMilliseconds; }
            set { _totalMilliseconds = value; }
        }

        public double TotalSeconds
        {
            get { return _totalMilliseconds / 1000.0; }
            set { _totalMilliseconds = value * 1000.0; }
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

        public void AddTime(int hour, int minutes, int seconds, int milliseconds)
        {
            Hours += hour;
            Minutes += minutes;
            Seconds += seconds;
            Milliseconds += milliseconds;
        }

        public void AddTime(long milliseconds)
        {
            _totalMilliseconds += milliseconds;
        }

        public void AddTime(TimeSpan timeSpan)
        {
            _totalMilliseconds += timeSpan.TotalMilliseconds;
        }

        public void AddTime(double milliseconds)
        {
            _totalMilliseconds += milliseconds;
        }

        public override string ToString()
        {
            var ts = TimeSpan;
            string s = string.Format("{0:00}:{1:00}:{2:00},{3:000}", ts.Hours + ts.Days * 24, ts.Minutes, ts.Seconds, ts.Milliseconds);

            if (TotalMilliseconds >= 0)
                return s;
            else
                return "-" + s.Replace("-", string.Empty);
        }

        public string ToShortString()
        {
            var ts = TimeSpan;
            string s;
            if (ts.Minutes == 0 && ts.Hours == 0 && ts.Days == 0)
                s = string.Format("{0:0},{1:000}", ts.Seconds, ts.Milliseconds);
            else if (ts.Hours == 0 && ts.Days == 0)
                s = string.Format("{0:0}:{1:00},{2:000}", ts.Minutes, ts.Seconds, ts.Milliseconds);
            else
                s = string.Format("{0:0}:{1:00}:{2:00},{3:000}", ts.Hours + ts.Days * 24, ts.Minutes, ts.Seconds, ts.Milliseconds);

            if (TotalMilliseconds >= 0)
                return s;
            else
                return "-" + s.Replace("-", string.Empty);
        }

        public string ToShortStringHHMMSSFF()
        {
            var ts = TimeSpan;
            if (ts.Minutes == 0 && ts.Hours == 0)
                return string.Format("{0:00}:{1:00}", ts.Seconds, SubtitleFormat.MillisecondsToFramesMaxFrameRate(ts.Milliseconds));
            if (ts.Hours == 0)
                return string.Format("{0:00}:{1:00}:{2:00}", ts.Minutes, ts.Seconds, SubtitleFormat.MillisecondsToFramesMaxFrameRate(ts.Milliseconds));
            return string.Format("{0:00}:{1:00}:{2:00}:{3:00}", ts.Hours, ts.Minutes, ts.Seconds, SubtitleFormat.MillisecondsToFramesMaxFrameRate(ts.Milliseconds));
        }

        public string ToHHMMSSFF()
        {
            var ts = TimeSpan;
            return string.Format("{0:00}:{1:00}:{2:00}:{3:00}", ts.Hours, ts.Minutes, ts.Seconds, SubtitleFormat.MillisecondsToFramesMaxFrameRate(ts.Milliseconds));
        }

        public string ToHHMMSSPeriodFF()
        {
            var ts = TimeSpan;
            return string.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, SubtitleFormat.MillisecondsToFramesMaxFrameRate(ts.Milliseconds));
        }

    }
}
