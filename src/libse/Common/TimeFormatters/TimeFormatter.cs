namespace Nikse.SubtitleEdit.Core.Common.TimeFormatters
{
    /// <summary>
    /// Shared formatter instances for use with <see cref="TimeCode.ToString(ITimeFormatter)"/>.
    /// </summary>
    public static class TimeFormatter
    {
        public static readonly ITimeFormatter HhMmSsFf = new HhMmSsFfTimeFormatter();
        public static readonly ITimeFormatter ShortHhMmSsFf = new ShortHhMmSsFfTimeFormatter();
        public static readonly ITimeFormatter HhMmSs = new HhMmSsTimeFormatter();
        public static readonly ITimeFormatter HhMmSsFfDropFrame = new HhMmSsFfDropFrameTimeFormatter();
        public static readonly ITimeFormatter SsFf = new SsFfTimeFormatter();
        public static readonly ITimeFormatter HhMmSsPeriodFf = new HhMmSsPeriodFfTimeFormatter();
    }
}
