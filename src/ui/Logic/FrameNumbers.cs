using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// The "frame numbers" time code mode: a time is shown as its absolute frame number ("15230")
/// at the current frame rate, so a sync offset is just "video frame - start frame" (#15603).
/// </summary>
public static class FrameNumbers
{
    public static int FromMilliseconds(double milliseconds) => SubtitleFormat.MillisecondsToFrames(milliseconds);

    public static double ToMilliseconds(int frames) => SubtitleFormat.FramesToMilliseconds(frames);

    public static string Format(double milliseconds) => FromMilliseconds(milliseconds).ToString(CultureInfo.InvariantCulture);

    public static string Format(TimeSpan time) => Format(time.TotalMilliseconds);

    /// <summary>
    /// Parses a whole frame number, optionally negative. Anything else (separators, decimals)
    /// is rejected.
    /// </summary>
    public static bool TryParse(string? text, out TimeSpan time)
    {
        time = TimeSpan.Zero;
        if (string.IsNullOrWhiteSpace(text) ||
            !int.TryParse(text.Trim(), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var frames))
        {
            return false;
        }

        // Checked in frames first: the millisecond conversion is an int and would overflow.
        var maxFrames = FromMilliseconds(Core.Common.TimeCode.MaxTimeTotalMilliseconds);
        if (frames > maxFrames || frames < -maxFrames)
        {
            return false;
        }

        time = TimeSpan.FromMilliseconds(ToMilliseconds(frames));
        return true;
    }

    /// <summary>
    /// The time of the frame <paramref name="time"/> is shown as - what typing the displayed
    /// frame number back in stores.
    /// </summary>
    public static TimeSpan Snap(TimeSpan time) => TimeSpan.FromMilliseconds(ToMilliseconds(FromMilliseconds(time.TotalMilliseconds)));
}
