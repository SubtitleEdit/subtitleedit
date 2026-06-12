using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;

namespace Nikse.SubtitleEdit.Core.Common.TimeFormatters
{
    /// <summary>
    /// Base for formatters that convert milliseconds to frames using the current frame rate,
    /// carrying over to the next second when the frame count rounds up to the frame rate.
    /// </summary>
    public abstract class FrameBasedTimeFormatter : ITimeFormatter
    {
        public string Format(TimeSpan timeSpan)
        {
            var frames = Math.Round(timeSpan.Milliseconds / (TimeCode.BaseUnit / Configuration.Settings.General.CurrentFrameRate));
            string s;
            if (frames >= Configuration.Settings.General.CurrentFrameRate - 0.001)
            {
                s = FormatTime(timeSpan.Add(new TimeSpan(0, 0, 1)), 0);
            }
            else
            {
                s = FormatTime(timeSpan, SubtitleFormat.MillisecondsToFramesMaxFrameRate(timeSpan.Milliseconds));
            }

            return TimeCode.PrefixSign(s, timeSpan.TotalMilliseconds);
        }

        protected abstract string FormatTime(TimeSpan ts, int frames);
    }
}
