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
        /// <summary>
        /// Window length. Only needs to be long enough to hold a chunk's speech plus
        /// whatever unmatched audio has to be skipped over, and encoder cost is quadratic -
        /// 240 s costs ~25 s per run against ~7 s at 120 s - so it is kept modest. Chunks
        /// are re-aligned from wherever the previous one ended, so a gap longer than one
        /// window is still crossed, just over several of them.
        /// </summary>
        public double WindowSeconds { get; init; } = 120;

        /// <summary>
        /// Lines handed to the aligner at a time. Deliberately far less text than the window
        /// holds: given slack, CTC alignment places the text where it acoustically belongs
        /// and emits blanks over the rest, so audio the script says nothing about is simply
        /// skipped. Measured feeding 8 lines against a 240 s window whose matching speech
        /// only starts 182 s in - every interior line landed within 60 ms.
        ///
        /// Filling the window instead is what forces the failure: the aligner must consume
        /// every line it is given, so it crushes them onto whatever audio is there.
        /// </summary>
        public int LinesPerChunk { get; init; } = 12;

        /// <summary>
        /// A chunk's text must stay well under the window, or the slack disappears and the
        /// aligner is back to being forced. Chunks are cut at whichever comes first, this
        /// share of the window's worth of reading time or <see cref="LinesPerChunk"/>.
        /// </summary>
        public double MaxChunkShareOfWindow { get; init; } = 0.35;

        /// <summary>
        /// Lines refined together in the second pass. Small, so each batch's window is
        /// short: the refine pass is anchored on the first pass's answer, so the aligner
        /// gets nearly exactly the audio its text belongs to, and quadratic encoder cost
        /// makes short windows almost free.
        /// </summary>
        public int RefineBatchLines { get; init; } = 6;

        /// <summary>Audio kept either side of a refine batch, in case the first pass clipped its edges.</summary>
        public double RefinePaddingSeconds { get; init; } = 3.0;

        /// <summary>
        /// How far the refine pass may move a line. It exists to sharpen a time that is
        /// already about right, not to relocate one - a batch whose window was anchored on
        /// a bad first-pass guess would otherwise be re-fitted just as confidently to the
        /// wrong audio. A move beyond this is treated as disagreement and the first pass's
        /// answer is kept.
        /// </summary>
        public double MaxRefineShiftSeconds { get; init; } = 2.5;

        /// <summary>
        /// How far the audio cursor creeps when a window aligns to nothing usable, as
        /// happens over a passage the script is missing. Deliberately small: the cursor has
        /// to stop as soon as it reaches the audio the script resumes at, and a stride of
        /// most-of-a-window sails straight past it and never re-syncs.
        /// </summary>
        public double SkipSeconds { get; init; } = 15.0;
    }

    /// <summary>
    /// How many lines to hand the aligner for one window: a small chunk, capped both by
    /// line count and by reading time, so the window keeps the slack that lets the aligner
    /// skip audio the script does not cover.
    /// </summary>
    public static int ChunkSize(
        IReadOnlyList<string> remainingLines,
        double windowSeconds,
        double readingCharsPerSecond,
        Options options)
    {
        ArgumentNullException.ThrowIfNull(remainingLines);
        ArgumentNullException.ThrowIfNull(options);

        if (remainingLines.Count == 0)
        {
            return 0;
        }

        var budgetSeconds = windowSeconds * options.MaxChunkShareOfWindow;
        var take = 0;
        var seconds = 0.0;

        foreach (var line in remainingLines)
        {
            var lineSeconds = readingCharsPerSecond > 0 ? line.Length / readingCharsPerSecond : 0;
            if (take > 0 && (take >= options.LinesPerChunk || seconds + lineSeconds > budgetSeconds))
            {
                break;
            }

            seconds += lineSeconds;
            take++;
        }

        return Math.Max(1, take);
    }

    /// <summary>
    /// How many of a chunk's cues to believe.
    ///
    /// A chunk deliberately carries less text than its window holds, so once the aligner
    /// runs out of script it smears the remaining cues across the leftover audio. Measured
    /// feeding 12 lines whose speech ends 43 s into a 240 s window: the first ten landed
    /// within 60 ms, then the next cue opened 54 s after its predecessor closed and the
    /// rest ran away down the window.
    ///
    /// So accepting stops at the first cue that either opens implausibly long after the
    /// previous one closed, or lasts far longer than its text takes to read. Consecutive
    /// script lines are consecutive speech; a minute of silence between them means the
    /// aligner has stopped tracking and is filling space.
    /// </summary>
    public static int AcceptChunk(
        IReadOnlyList<Cue> cues,
        IReadOnlyList<double> readingSeconds,
        double maxGapSeconds = 12.0,
        double maxDurationRatio = 2.5)
    {
        ArgumentNullException.ThrowIfNull(cues);
        ArgumentNullException.ThrowIfNull(readingSeconds);

        var accepted = 0;
        for (var i = 0; i < cues.Count && i < readingSeconds.Count; i++)
        {
            if (i > 0 && cues[i].StartSeconds - cues[i - 1].EndSeconds > maxGapSeconds)
            {
                break;
            }

            // The first cue absorbs whatever leading audio the script does not cover, so
            // its length says nothing; every later one should match its text.
            if (i > 0 && readingSeconds[i] > 0)
            {
                var duration = cues[i].EndSeconds - cues[i].StartSeconds;
                if (duration > readingSeconds[i] * maxDurationRatio)
                {
                    break;
                }
            }

            accepted++;
        }

        return accepted;
    }

}
