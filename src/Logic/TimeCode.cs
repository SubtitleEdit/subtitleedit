using System;

namespace Nikse.SubtitleEdit.Logic
{
    public class TimeCode
    {
        TimeSpan _time;

        public static double ParseToMilliseconds(string text)
        {
            string[] parts = text.Split(":,.".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
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

        public TimeCode(TimeSpan timeSpan)
        {
            TimeSpan = timeSpan;
        }

        public TimeCode(int hour, int minute, int seconds, int milliseconds)
        {
            _time = new TimeSpan(0, hour, minute, seconds, milliseconds);
        }

        public int Hours
        {
            get { return _time.Hours; }
            set { _time = new TimeSpan(0, value, _time.Minutes, _time.Seconds, _time.Milliseconds); }
        }

        public int Minutes
        {
            get { return _time.Minutes; }
            set { _time = new TimeSpan(0, _time.Hours, value, _time.Seconds, _time.Milliseconds); }
        }

        public int Seconds
        {
            get { return _time.Seconds; }
            set { _time = new TimeSpan(0, _time.Hours, _time.Minutes, value, _time.Milliseconds); }
        }

        public int Milliseconds
        {
            get { return _time.Milliseconds; }
            set { _time = new TimeSpan(0, _time.Hours, _time.Minutes, _time.Seconds, value); }
        }

        public double TotalMilliseconds
        {
            get { return _time.TotalMilliseconds; }
            set { _time = TimeSpan.FromMilliseconds(value); }
        }

        public double TotalSeconds
        {
            get { return _time.TotalSeconds; }
            set { _time = TimeSpan.FromSeconds(value); }
        }

        public TimeSpan TimeSpan
        {
            get
            {
                return _time;
            }
            set
            {
                _time = value;
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
            _time = TimeSpan.FromMilliseconds(_time.TotalMilliseconds + milliseconds);
        }

        public void AddTime(TimeSpan timeSpan)
        {
            _time = TimeSpan.FromMilliseconds(_time.TotalMilliseconds + timeSpan.TotalMilliseconds);
        }

        public void AddTime(double milliseconds)
        {
            _time = TimeSpan.FromMilliseconds(_time.TotalMilliseconds + milliseconds);
        }

        public override string ToString()
        {
            return string.Format("{0:00}:{1:00}:{2:00},{3:000}", _time.Hours, _time.Minutes, _time.Seconds, _time.Milliseconds);
        }

        public string ToShortString()
        {
            if (_time.Minutes == 0 && _time.Hours == 0)
                return string.Format("{0:00},{1:000}", _time.Seconds, _time.Milliseconds);
            else if (_time.Hours == 0)
                return string.Format("{0:00}:{1:00},{2:000}", _time.Minutes, _time.Seconds, _time.Milliseconds);
            else
                return string.Format("{0:00}:{1:00}:{2:00},{3:000}", _time.Hours, _time.Minutes, _time.Seconds, _time.Milliseconds);
        }

    }
}
