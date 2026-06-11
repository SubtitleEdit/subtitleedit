using System;

namespace Nikse.SubtitleEdit.Core.Common.TimeFormatters
{
    /// <summary>
    /// Formats as "HH:MM:SS", rounding seconds up when the frame count carries over.
    /// </summary>
    public class HhMmSsTimeFormatter : FrameBasedTimeFormatter
    {
        protected override string FormatTime(TimeSpan ts, int frames)
        {
            return $"{ts.Days * 24 + ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}";
        }
    }
}
