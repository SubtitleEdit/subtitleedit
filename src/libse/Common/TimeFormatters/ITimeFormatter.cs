using System;

namespace Nikse.SubtitleEdit.Core.Common.TimeFormatters
{
    /// <summary>
    /// Formats a <see cref="TimeSpan"/> as a string. Pass to <see cref="TimeCode.ToString(ITimeFormatter)"/>
    /// or <see cref="TimeSpanExtensions.ToString(TimeSpan, ITimeFormatter)"/>.
    /// </summary>
    public interface ITimeFormatter
    {
        string Format(TimeSpan timeSpan);
    }
}
