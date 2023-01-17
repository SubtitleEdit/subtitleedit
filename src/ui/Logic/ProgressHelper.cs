using Nikse.SubtitleEdit.Core.Common;
using System;

namespace Nikse.SubtitleEdit.Logic
{
    public static class ProgressHelper
    {
        public static string ToProgressTime(double estimatedTotalMs)
        {
            var totalSeconds = (int)Math.Round(estimatedTotalMs / 1000.0);
            if (totalSeconds < 60)
            {
                return totalSeconds < 3
                    ? string.Format(LanguageSettings.Current.GenerateVideoWithBurnedInSubs.TimeRemainingAFewSeconds)
                    : string.Format(LanguageSettings.Current.GenerateVideoWithBurnedInSubs.TimeRemainingSeconds, totalSeconds);
            }

            if (totalSeconds / 60 > 5)
            {
                return string.Format(LanguageSettings.Current.GenerateVideoWithBurnedInSubs.TimeRemainingMinutes, (int)Math.Round(totalSeconds / 60.0));
            }

            var timeCode = new TimeCode(estimatedTotalMs);
            return string.Format(LanguageSettings.Current.GenerateVideoWithBurnedInSubs.TimeRemainingMinutesAndSeconds, timeCode.Minutes + timeCode.Hours * 60, timeCode.Seconds);
        }
    }
}
