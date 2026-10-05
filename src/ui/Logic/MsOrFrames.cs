using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// A tool dialog's duration/gap box holds frames in frame mode and milliseconds otherwise, like
/// Bridge gaps and Apply min gap (#14959). The setting behind the box stays in milliseconds;
/// these convert at the current frame rate with the same rounding Bridge gaps uses.
/// </summary>
public static class MsOrFrames
{
    /// <summary>The number the box shows for a millisecond value.</summary>
    public static int FromMilliseconds(int milliseconds, bool frameMode)
    {
        return frameMode ? SubtitleFormat.MillisecondsToFrames(milliseconds) : milliseconds;
    }

    /// <summary>The milliseconds a number typed in the box stands for.</summary>
    public static int ToMilliseconds(int value, bool frameMode)
    {
        return frameMode ? SubtitleFormat.FramesToMilliseconds(value) : value;
    }

    /// <summary>
    /// The milliseconds to save for the box's value. When the frame count is the one the stored
    /// value was shown as, the stored value is kept, so just opening and closing a dialog in frame
    /// mode does not round the setting to whole frames (1200 ms at 23.976 fps is 29 frames, and
    /// 29 frames is 1210 ms).
    /// </summary>
    public static int ToMillisecondsForSave(int value, bool frameMode, int storedMilliseconds)
    {
        if (frameMode && value == FromMilliseconds(storedMilliseconds, frameMode: true))
        {
            return storedMilliseconds;
        }

        return ToMilliseconds(value, frameMode);
    }

    /// <summary>
    /// True when a measured duration or gap is above the box's limit. In frame mode both sides are
    /// compared in frames: the limit converted to rounded milliseconds can be one millisecond short
    /// of a span exactly that many frames long (one frame at 29.97 fps is 33 ms, but two time codes
    /// one frame apart can be 34 ms apart after their own rounding).
    /// </summary>
    public static bool IsAbove(double milliseconds, int limit, bool frameMode)
    {
        return frameMode ? SubtitleFormat.MillisecondsToFrames(milliseconds) > limit : milliseconds > limit;
    }

    /// <summary>
    /// True when a measured duration or gap is below the box's limit, compared in frames in frame
    /// mode (see <see cref="IsAbove"/>).
    /// </summary>
    public static bool IsBelow(double milliseconds, int limit, bool frameMode)
    {
        return frameMode ? SubtitleFormat.MillisecondsToFrames(milliseconds) < limit : milliseconds < limit;
    }

    /// <summary>
    /// The "max between" milliseconds to hand to code that compares millisecond gaps with
    /// <c>gap &gt; max</c>. In frame mode this is the largest whole millisecond gap that still
    /// rounds to at most <paramref name="value"/> frames, so the comparison matches comparing in
    /// frames for whole-millisecond time codes.
    /// </summary>
    public static int ToMaxGapMilliseconds(int value, bool frameMode)
    {
        if (!frameMode || value < 0)
        {
            return value;
        }

        var ms = SubtitleFormat.FramesToMilliseconds(value);
        while (SubtitleFormat.MillisecondsToFrames(ms + 1) <= value)
        {
            ms++;
        }

        return ms;
    }
}
