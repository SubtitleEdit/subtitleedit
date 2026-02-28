using Nikse.SubtitleEdit.Core.Common;
using System;

namespace Nikse.SubtitleEdit.Logic
{
    public static class ProgressHelper
    {
        private const string TimeRemainingMinutes = "Time remaining: {0} minutes";
        private const string TimeRemainingOneMinute = "Time remaining: One minute";
        private const string TimeRemainingSeconds = "Time remaining: {0} seconds";
        private const string TimeRemainingAFewSeconds = "Time remaining: A few seconds";
        private const string TimeRemainingMinutesAndSeconds = "Time remaining: {0} minutes and {1} seconds";
        private const string TimeRemainingOneMinuteAndSeconds = "Time remaining: One minute and {0} seconds";

        public static string ToProgressTime(double estimatedTotalMs)
        {
            var totalSeconds = (int)Math.Round(estimatedTotalMs / 1000.0, MidpointRounding.AwayFromZero);
            
            if (totalSeconds > 20_000) // 5.5 hours
            {
                return "Time remaining: A long time";
            }

            if (totalSeconds < 60)
            {
                if (totalSeconds < 3)
                {
                    return string.Format(TimeRemainingAFewSeconds);
                }

                return string.Format(TimeRemainingSeconds, totalSeconds);
            }

            if (totalSeconds / 60 > 5)
            {
                return string.Format(TimeRemainingMinutes, (int)Math.Round(totalSeconds / 60.0, MidpointRounding.AwayFromZero));
            }

            var timeCode = new TimeCode(estimatedTotalMs);
            if (timeCode.Seconds == 0 && timeCode.Minutes > 0)
            {
                if (timeCode.Minutes == 1)
                {
                    return string.Format(TimeRemainingOneMinute);
                }

                return string.Format(TimeRemainingMinutes, timeCode.Minutes);
            }

            if (timeCode.Hours == 0 && timeCode.Minutes == 1)
            {
                return string.Format(TimeRemainingOneMinuteAndSeconds, timeCode.Seconds);
            }

            return string.Format(TimeRemainingMinutesAndSeconds, timeCode.Minutes + timeCode.Hours * 60, timeCode.Seconds);
        }

        public static string ToTimeResult(double totalMilliseconds)
        {
            if (totalMilliseconds < 1000)
            {
                return string.Format("{0:##0} milliseconds", totalMilliseconds);
            }

            var totalSeconds = (int)Math.Round(totalMilliseconds / 1000.0, MidpointRounding.AwayFromZero);
            if (totalSeconds < 60)
            {
                return string.Format("{0} seconds", totalSeconds);
            }

            if (totalSeconds / 60 > 5)
            {
                return string.Format("{0} minutes", (int)Math.Round(totalSeconds / 60.0, MidpointRounding.AwayFromZero));
            }

            var timeCode = new TimeCode(totalMilliseconds);
            if (timeCode.Seconds == 0 && timeCode.Minutes > 0)
            {
                if (timeCode.Minutes == 1)
                {
                    return "One minute";
                }

                return string.Format("{0} minutes", timeCode.Minutes);
            }

            if (timeCode.Hours == 0 && timeCode.Minutes == 1)
            {
                return string.Format("One minutes and {0} seconds", timeCode.Seconds);
            }

            return string.Format("{0} minutes and {1} seconds", timeCode.Minutes + timeCode.Hours * 60, timeCode.Seconds);
        }
    }
}
