using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.UiLogic.Media;

/// <summary>
/// Decides whether the video offset applies to a second subtitle on the video (#16434). With an
/// offset like 10:00:00:00 the main file is shown minus the offset, but a second file may be
/// either a sibling written with the same 10-hour times or one already timed from zero. Each way
/// of reading it is scored by how many of its lines start inside the reference window (the video,
/// or else the main subtitle on the video timeline), and the better fit wins.
/// </summary>
public static class SecondarySubtitleOffset
{
    private const double ToleranceMs = 1000;

    /// <param name="secondary">The second subtitle with its file time codes.</param>
    /// <param name="offsetMs">The video offset, subtracted from file times to get video times.</param>
    /// <param name="windowStartMs">Start of the reference window on the video timeline.</param>
    /// <param name="windowEndMs">End of the reference window; <see cref="double.MaxValue"/> when unknown.</param>
    /// <returns>True when the offset should be subtracted; also on a tie, so a file that fits
    /// neither way keeps the main file's split.</returns>
    public static bool ShouldApply(Subtitle secondary, double offsetMs, double windowStartMs, double windowEndMs)
    {
        if (offsetMs == 0 || secondary.Paragraphs.Count == 0)
        {
            return offsetMs != 0;
        }

        var low = windowStartMs - ToleranceMs;
        var high = windowEndMs == double.MaxValue ? double.MaxValue : windowEndMs + ToleranceMs;
        var asIs = 0;
        var shifted = 0;
        foreach (var p in secondary.Paragraphs)
        {
            var start = p.StartTime.TotalMilliseconds;
            if (start >= low && start <= high)
            {
                asIs++;
            }

            var shiftedStart = start - offsetMs;
            if (shiftedStart >= low && shiftedStart <= high)
            {
                shifted++;
            }
        }

        return shifted >= asIs;
    }
}
