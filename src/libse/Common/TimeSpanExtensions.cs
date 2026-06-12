using Nikse.SubtitleEdit.Core.Common.TimeFormatters;
using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Core.Common
{
    public static class TimeSpanExtensions
    {
        /// <summary>
        /// Formats a <see cref="TimeSpan"/> with the given <see cref="ITimeFormatter"/>.
        /// </summary>
        public static string ToString(this TimeSpan timeSpan, ITimeFormatter formatter)
        {
            return formatter.Format(timeSpan);
        }

        /// <summary>
        /// Formats as "HH:MM:SS,mmm" (comma replaced by the culture's decimal separator when localized).
        /// </summary>
        public static string ToString(this TimeSpan timeSpan, bool localize)
        {
            var decimalSeparator = localize ? CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator : ",";
            var s = $"{timeSpan.Hours + timeSpan.Days * 24:00}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}{decimalSeparator}{timeSpan.Milliseconds:000}";

            return TimeCode.PrefixSign(s, timeSpan.TotalMilliseconds);
        }

        public static string ToShortDisplayString(this TimeSpan timeSpan)
        {
            if (Math.Abs(timeSpan.TotalMilliseconds - TimeCode.MaxTimeTotalMilliseconds) < 0.01)
            {
                return "-";
            }

            if (Configuration.Settings?.General.UseTimeFormatHHMMSSFF == true)
            {
                return timeSpan.ToString(TimeFormatter.ShortHhMmSsFf);
            }

            return timeSpan.ToShortString(true);
        }

        /// <summary>
        /// Formats as "HH:MM:SS,mmm" with leading zero groups trimmed.
        /// </summary>
        public static string ToShortString(this TimeSpan timeSpan, bool localize = false)
        {
            var decimalSeparator = localize ? CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator : ",";
            string s;
            if (timeSpan.Minutes == 0 && timeSpan.Hours == 0 && timeSpan.Days == 0)
            {
                s = $"{timeSpan.Seconds:0}{decimalSeparator}{timeSpan.Milliseconds:000}";

                if (s == $"0{decimalSeparator}000")
                {
                    return s; // no sign
                }
            }
            else if (timeSpan.Hours == 0 && timeSpan.Days == 0)
            {
                s = $"{timeSpan.Minutes:0}:{timeSpan.Seconds:00}{decimalSeparator}{timeSpan.Milliseconds:000}";

                if (s == $"0:00{decimalSeparator}000")
                {
                    return s; // no sign
                }
            }
            else
            {
                s = $"{timeSpan.Hours + timeSpan.Days * 24:0}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}{decimalSeparator}{timeSpan.Milliseconds:000}";

                if (s == $"0:00:00{decimalSeparator}000")
                {
                    return s; // no sign
                }
            }

            return TimeCode.PrefixSign(s, timeSpan.TotalMilliseconds);
        }
    }
}
