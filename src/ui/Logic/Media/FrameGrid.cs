using System;

namespace Nikse.SubtitleEdit.Logic.Media;

/// <summary>
/// Where the loaded video's frames sit on the player's timeline. The player, the waveform and the
/// shot changes all count from the file's start time, so a 25 fps video starting at 0.02 s has its
/// frames at 20, 60, 100... ms - a grid counted from 0 snaps every time half a frame off (#17149).
/// </summary>
public static class FrameGrid
{
    /// <summary>
    /// A usable grid start: positive and finite, else 0. The offset belongs to one editor window's
    /// video - File &gt; New window opens windows with videos of their own, so it is not shared.
    /// </summary>
    public static double NormalizeOffset(double offsetSeconds) =>
        offsetSeconds > 0 && double.IsFinite(offsetSeconds) ? offsetSeconds : 0;

    /// <summary>
    /// Nearest frame time to <paramref name="seconds"/> on a grid starting at <paramref name="offsetSeconds"/>.
    /// Any unit works, as long as all three values use the same one.
    /// </summary>
    public static double Snap(double seconds, double frameDurationSeconds, double offsetSeconds)
    {
        // Nothing is shown before the first frame.
        var frame = Math.Max(0, Math.Round((seconds - offsetSeconds) / frameDurationSeconds, MidpointRounding.AwayFromZero));
        return frame * frameDurationSeconds + offsetSeconds;
    }
}
