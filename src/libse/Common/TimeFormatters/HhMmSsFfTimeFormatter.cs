using System;

namespace Nikse.SubtitleEdit.Core.Common.TimeFormatters
{
    /// <summary>
    /// Formats as "HH:MM:SS:FF".
    /// </summary>
    public class HhMmSsFfTimeFormatter : FrameBasedTimeFormatter
    {
        protected override string FormatTime(TimeSpan ts, int frames)
        {
            return $"{ts.Days * 24 + ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}:{frames:00}";
        }
    }
}
