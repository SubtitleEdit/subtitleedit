using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;

namespace Nikse.SubtitleEdit.Core.Common.TimeFormatters
{
    /// <summary>
    /// Formats as "SS:FF". On frame carry-over the seconds value is incremented
    /// without rolling over into minutes (59 becomes 60).
    /// </summary>
    public class SsFfTimeFormatter : ITimeFormatter
    {
        public string Format(TimeSpan timeSpan)
        {
            var frames = Math.Round(timeSpan.Milliseconds / (TimeCode.BaseUnit / Configuration.Settings.General.CurrentFrameRate));
            var s = frames >= Configuration.Settings.General.CurrentFrameRate - 0.001
                ? $"{timeSpan.Seconds + 1:00}:{0:00}"
                : $"{timeSpan.Seconds:00}:{SubtitleFormat.MillisecondsToFramesMaxFrameRate(timeSpan.Milliseconds):00}";

            return TimeCode.PrefixSign(s, timeSpan.TotalMilliseconds);
        }
    }
}
