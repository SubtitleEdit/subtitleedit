using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Files.ImportPlainText;

/// <summary>
/// Decides how a script is fed to a forced aligner window by window.
///
/// A CTC forced aligner runs the whole window through its encoder in one pass, and
/// encoder attention is quadratic: measured on canary-ctc, 60 s of audio aligns in 2 s,
/// 600 s in 68 s, and 900 s in 146 s, while a 57 minute file fails outright trying to
/// allocate a 24 GB buffer. Chunking is therefore not a compromise - it is both faster
/// and bounded in memory, and it is the only way a two hour video aligns at all.
///
/// The logic here is pure so it can be tested without an aligner binary; the process
/// work lives in <see cref="ForcedAligner"/>.
/// </summary>
public static class ForcedAlignPlanner
{
    public readonly record struct Cue(double StartSeconds, double EndSeconds);

    public sealed record Options
    {
        /// <summary>Target window length. 240 s sits well inside the quadratic cost curve.</summary>
        public double WindowSeconds { get; init; } = 240;

        /// <summary>
        /// How much more text than we think fits is fed to a window. Kept deliberately
        /// small: forced alignment has to consume every token it is given, so once it runs
        /// short of audio it starts taking time from real speech. Measured with a 1.7x
        /// overshoot, alignment held for the first third of a window and then drifted by
        /// up to 166 s; at 1.15x the distortion stays in the discarded tail.
        /// </summary>
        public double Overshoot { get; init; } = 1.15;

        /// <summary>Fraction of a window whose cues are trusted; the rest is re-aligned by the next window.</summary>
        public double AcceptFraction { get; init; } = 0.75;

        /// <summary>Cues ending within this much of the window edge are never trusted.</summary>
        public double TailGuardSeconds { get; init; } = 1.0;
    }

    /// <summary>
    /// How many of the remaining lines to feed a window. The last window always gets
    /// everything left, so no line is ever dropped.
    /// </summary>
    public static int LinesForWindow(
        IReadOnlyList<string> remainingLines,
        double windowSeconds,
        double charsPerSecond,
        Options options,
        bool isLastWindow)
    {
        ArgumentNullException.ThrowIfNull(remainingLines);
        ArgumentNullException.ThrowIfNull(options);

        if (remainingLines.Count == 0)
        {
            return 0;
        }

        if (isLastWindow || charsPerSecond <= 0 || windowSeconds <= 0)
        {
            return remainingLines.Count;
        }

        var budget = windowSeconds * charsPerSecond * options.Overshoot;
        var accumulated = 0.0;
        var take = 0;
        foreach (var line in remainingLines)
        {
            take++;
            accumulated += line.Length;
            if (accumulated >= budget)
            {
                break;
            }
        }

        return Math.Max(1, take);
    }

    /// <summary>
    /// How many of a window's cues to keep. Cues near the window edge are discarded and
    /// realigned by the next window, because that is where the overshoot text is crammed.
    /// </summary>
    public static int AcceptCount(
        IReadOnlyList<Cue> cues,
        double windowSeconds,
        Options options,
        bool isLastWindow)
    {
        ArgumentNullException.ThrowIfNull(cues);
        ArgumentNullException.ThrowIfNull(options);

        if (cues.Count == 0)
        {
            return 0;
        }

        if (isLastWindow)
        {
            return cues.Count;
        }

        var limit = Math.Min(
            windowSeconds * options.AcceptFraction,
            windowSeconds - options.TailGuardSeconds);

        var accepted = 0;
        foreach (var cue in cues)
        {
            if (cue.EndSeconds > limit)
            {
                break;
            }

            accepted++;
        }

        // Never stall. A window whose cues all sit past the limit still has to yield
        // something, or the same window is planned again forever.
        return accepted > 0 ? accepted : Math.Max(1, cues.Count / 3);
    }
}
