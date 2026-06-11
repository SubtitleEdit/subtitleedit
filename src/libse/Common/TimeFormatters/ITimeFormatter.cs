namespace Nikse.SubtitleEdit.Core.Common.TimeFormatters
{
    /// <summary>
    /// Formats a <see cref="TimeCode"/> as a string. Pass to <see cref="TimeCode.ToString(ITimeFormatter)"/>.
    /// </summary>
    public interface ITimeFormatter
    {
        string Format(TimeCode timeCode);
    }
}
