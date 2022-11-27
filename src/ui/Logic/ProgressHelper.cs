using Nikse.SubtitleEdit.Core.Common;
using System;

namespace Nikse.SubtitleEdit.Logic
{
    public static class ProgressHelper
    {
        public static string ToProgressTime(double estimatedTotalMs)
        {
            var timeCode = new TimeCode(estimatedTotalMs);
            if (timeCode.TotalSeconds < 60)
            {
                return string.Format(LanguageSettings.Current.GenerateVideoWithBurnedInSubs.TimeRemainingSeconds, (int)Math.Round(timeCode.TotalSeconds));
            }

            if (timeCode.TotalSeconds / 60 > 5)
            {
                return string.Format(LanguageSettings.Current.GenerateVideoWithBurnedInSubs.TimeRemainingMinutes, (int)Math.Round(timeCode.TotalSeconds / 60));
            }

            return string.Format(LanguageSettings.Current.GenerateVideoWithBurnedInSubs.TimeRemainingMinutesAndSeconds, timeCode.Minutes + timeCode.Hours * 60, timeCode.Seconds);
        }
    }
}
