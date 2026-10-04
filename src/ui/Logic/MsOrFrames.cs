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
}
