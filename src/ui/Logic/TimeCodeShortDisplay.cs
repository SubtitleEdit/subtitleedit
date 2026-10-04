using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Short time code text that follows the time code mode: a frame number in frame numbers mode,
/// "ss:ff" style in frame mode, "ss,mmm" style otherwise - the same text the grid's
/// gap/duration cells show (DoubleToDisplayShortConverter).
/// </summary>
public static class TimeCodeShortDisplay
{
    public static string Format(double milliseconds, bool localize = false)
    {
        return Format(milliseconds, new TimeCode(), localize);
    }

    /// <param name="buffer">Reused by callers formatting many values, to avoid a TimeCode per call.</param>
    internal static string Format(double milliseconds, TimeCode buffer, bool localize = false)
    {
        if (Se.Settings.General.UseFrameNumbers)
        {
            return FrameNumbers.Format(milliseconds);
        }

        buffer.TotalMilliseconds = milliseconds;
        return Se.Settings.General.UseFrameMode
            ? buffer.ToShortStringHHMMSSFF()
            : buffer.ToShortString(localize);
    }
}
