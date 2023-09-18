using Nikse.SubtitleEdit.Core.Common;
using System;

namespace Nikse.SubtitleEdit.Logic
{
    public static class ProgressHelper
    {
        public static string ToProgressTime(double estimatedTotalMs)
        {
            var totalSeconds = (int)Math.Round(estimatedTotalMs / 1000.0, MidpointRounding.AwayFromZero);
            if (totalSeconds < 60)
            {
                if (totalSeconds < 3)
                {
                    return string.Format(LanguageSettings.Current.GenerateVideoWithBurnedInSubs.TimeRemainingAFewSeconds);
                }

                return string.Format(LanguageSettings.Current.GenerateVideoWithBurnedInSubs.TimeRemainingSeconds, totalSeconds);
            }

            if (totalSeconds / 60 > 5)
            {
                return string.Format(LanguageSettings.Current.GenerateVideoWithBurnedInSubs.TimeRemainingMinutes, (int)Math.Round(totalSeconds / 60.0, MidpointRounding.AwayFromZero));
            }

            var timeCode = new TimeCode(estimatedTotalMs);
            if (timeCode.Seconds == 0 && timeCode.Minutes > 0)
            {
                if (timeCode.Minutes == 1)
                {
                    return string.Format(LanguageSettings.Current.GenerateVideoWithBurnedInSubs.TimeRemainingOneMinute);
                }

                return string.Format(LanguageSettings.Current.GenerateVideoWithBurnedInSubs.TimeRemainingMinutes, timeCode.Minutes);
            }

            if (timeCode.Hours == 0 && timeCode.Minutes == 1)
            {
                return string.Format(LanguageSettings.Current.GenerateVideoWithBurnedInSubs.TimeRemainingOneMinuteAndSeconds, timeCode.Seconds);
            }

            return string.Format(LanguageSettings.Current.GenerateVideoWithBurnedInSubs.TimeRemainingMinutesAndSeconds, timeCode.Minutes + timeCode.Hours * 60, timeCode.Seconds);
        }
    }
}
