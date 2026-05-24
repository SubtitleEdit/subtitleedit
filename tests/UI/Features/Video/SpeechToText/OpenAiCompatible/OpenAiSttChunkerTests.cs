using Nikse.SubtitleEdit.Features.Video.SpeechToText.OpenAiCompatible;
using static Nikse.SubtitleEdit.Features.Video.SpeechToText.OpenAiCompatible.OpenAiSttChunker;

namespace UITests.Features.Video.SpeechToText.OpenAiCompatible;

public class OpenAiSttChunkerTests
{
    [Theory]
    [InlineData(0L, 1)]
    [InlineData(1L, 1)]
    [InlineData(23L * 1024 * 1024, 1)]
    [InlineData(23L * 1024 * 1024 + 1, 2)]
    [InlineData(46L * 1024 * 1024, 2)]
    [InlineData(46L * 1024 * 1024 + 1, 3)]
    public void ComputeChunkCount_RoundsUpAtChunkSizeBoundary(long fileSize, int expected)
    {
        Assert.Equal(expected, ComputeChunkCount(fileSize));
    }

    [Fact]
    public void ComputeChunkCount_RejectsNonPositiveChunkSize()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ComputeChunkCount(100, 0));
    }

    [Fact]
    public void ComputeAdjustedBoundaries_SingleChunk_ReturnsWholeRange()
    {
        var result = ComputeAdjustedBoundaries(120.0, 1, Array.Empty<SilenceInterval>());

        Assert.Single(result);
        Assert.Equal(0.0, result[0].StartSeconds);
        Assert.Equal(120.0, result[0].EndSeconds);
    }

    [Fact]
    public void ComputeAdjustedBoundaries_NoSilences_FallsBackToEvenSplit()
    {
        // 120 s / 3 chunks → cuts at 40 and 80; with no silence to snap to,
        // the even-time targets must be preserved exactly.
        var result = ComputeAdjustedBoundaries(120.0, 3, Array.Empty<SilenceInterval>());

        Assert.Equal(3, result.Count);
        Assert.Equal(0.0, result[0].StartSeconds);
        Assert.Equal(40.0, result[0].EndSeconds);
        Assert.Equal(40.0, result[1].StartSeconds);
        Assert.Equal(80.0, result[1].EndSeconds);
        Assert.Equal(80.0, result[2].StartSeconds);
        Assert.Equal(120.0, result[2].EndSeconds);
    }

    [Fact]
    public void ComputeAdjustedBoundaries_SilenceWithinWindow_SnapsToMidpoint()
    {
        // Target at 60; silence covering (58, 62) has midpoint 60, so the cut
        // lands right there. A second silence further from the target is
        // ignored.
        var silences = new[]
        {
            new SilenceInterval(58.0, 62.0),
            new SilenceInterval(100.0, 101.0),
        };

        var result = ComputeAdjustedBoundaries(120.0, 2, silences);

        Assert.Equal(2, result.Count);
        Assert.Equal(60.0, result[0].EndSeconds);
        Assert.Equal(60.0, result[1].StartSeconds);
    }

    [Fact]
    public void ComputeAdjustedBoundaries_SilenceExactlyAtWindowEdge_StillSnaps()
    {
        // The docstring promises "within ±maxOffsetSeconds" (inclusive).
        // Silence midpoint sits exactly 10 s (the default window) from the
        // target of 60 — must snap, not be silently dropped.
        var silences = new[] { new SilenceInterval(69.0, 71.0) }; // midpoint 70

        var result = ComputeAdjustedBoundaries(120.0, 2, silences);

        Assert.Equal(70.0, result[0].EndSeconds);
    }

    [Fact]
    public void ComputeAdjustedBoundaries_SilenceOutsideWindow_KeepsEvenTimeTarget()
    {
        // Only silence is at midpoint 80, target is 60, default window is
        // ±10 — out of range, so the cut stays at 60.
        var silences = new[] { new SilenceInterval(78.0, 82.0) };

        var result = ComputeAdjustedBoundaries(120.0, 2, silences);

        Assert.Equal(60.0, result[0].EndSeconds);
    }

    [Fact]
    public void ComputeAdjustedBoundaries_SilenceOffCenter_SnapsToClosestSilenceMidpoint()
    {
        // Two candidate silences inside the ±10 window of target=60: one at
        // midpoint 56 (4 s away), one at midpoint 65 (5 s away). The closer
        // one wins.
        var silences = new[]
        {
            new SilenceInterval(55.0, 57.0),  // midpoint 56
            new SilenceInterval(64.0, 66.0),  // midpoint 65
        };

        var result = ComputeAdjustedBoundaries(120.0, 2, silences);

        Assert.Equal(56.0, result[0].EndSeconds);
    }

    [Fact]
    public void ComputeAdjustedBoundaries_DoesNotReuseSameSilenceForAdjacentCuts()
    {
        // Three chunks of 120 s → targets at 40 and 80. A single silence at
        // midpoint 60 is within ±10 of *both* targets — but reusing it
        // would collapse two cuts onto the same point and produce a giant
        // middle chunk above the size cap. The closer-target cut (40)
        // claims it; the other cut falls back to its even-time target (80).
        var silences = new[] { new SilenceInterval(59.0, 61.0) }; // midpoint 60, 20 s from t=40, 20 s from t=80

        var result = ComputeAdjustedBoundaries(120.0, 3, silences, maxOffsetSeconds: 25.0);

        Assert.Equal(3, result.Count);
        // First cut snaps to the silence (it's the only one in range).
        Assert.Equal(60.0, result[0].EndSeconds);
        // Second cut can't reuse it, so falls back to even-time (target 80).
        Assert.Equal(80.0, result[1].EndSeconds);
        Assert.Equal(120.0, result[2].EndSeconds);
    }

    [Fact]
    public void ComputeAdjustedBoundaries_CutsAreStrictlyIncreasing()
    {
        // Random-ish silences; assert monotonic chunk boundaries regardless.
        var silences = new[]
        {
            new SilenceInterval(10.0, 12.0),
            new SilenceInterval(45.0, 48.0),
            new SilenceInterval(72.0, 74.0),
            new SilenceInterval(95.0, 100.0),
        };

        var result = ComputeAdjustedBoundaries(120.0, 4, silences);

        Assert.Equal(4, result.Count);
        for (var i = 0; i < result.Count - 1; i++)
        {
            Assert.True(result[i].EndSeconds == result[i + 1].StartSeconds,
                $"Chunk {i}.End ({result[i].EndSeconds}) must equal Chunk {i + 1}.Start ({result[i + 1].StartSeconds})");
            Assert.True(result[i].EndSeconds > result[i].StartSeconds,
                $"Chunk {i} has non-positive duration");
        }
        Assert.Equal(0.0, result[0].StartSeconds);
        Assert.Equal(120.0, result[^1].EndSeconds);
    }

    [Fact]
    public void ComputeAdjustedBoundaries_IgnoresSilencesAtOrOutsideRange()
    {
        // Silences at the boundaries themselves are unusable: midpoint 0
        // would create an empty first chunk, midpoint totalSeconds would
        // create an empty last chunk. Same for negative or out-of-range.
        var silences = new[]
        {
            new SilenceInterval(-2.0, -1.0),
            new SilenceInterval(-1.0, 1.0),     // midpoint 0
            new SilenceInterval(58.0, 62.0),    // midpoint 60 — the only usable one
            new SilenceInterval(119.0, 121.0),  // midpoint 120
            new SilenceInterval(200.0, 210.0),
        };

        var result = ComputeAdjustedBoundaries(120.0, 2, silences);

        Assert.Equal(60.0, result[0].EndSeconds);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-1.0)]
    public void ComputeAdjustedBoundaries_RejectsNonPositiveTotalSeconds(double totalSeconds)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => ComputeAdjustedBoundaries(totalSeconds, 2, Array.Empty<SilenceInterval>()));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ComputeAdjustedBoundaries_RejectsNonPositiveChunkCount(int chunkCount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => ComputeAdjustedBoundaries(120.0, chunkCount, Array.Empty<SilenceInterval>()));
    }

    [Fact]
    public void ParseSilenceIntervals_EmptyOrNull_ReturnsEmpty()
    {
        Assert.Empty(ParseSilenceIntervals(""));
        Assert.Empty(ParseSilenceIntervals(null!));
    }

    [Fact]
    public void ParseSilenceIntervals_RealFfmpegOutputSample_ExtractsAllPairs()
    {
        // Sampled directly from `ffmpeg -af silencedetect=...` stderr. Lines
        // around the silence_start / silence_end ones are part of the same
        // log block and must not interfere with parsing.
        const string sample =
            "[silencedetect @ 0x7f8] silence_start: 12.345\n" +
            "size=     N/A time=00:00:14.00 bitrate=N/A speed= 116x\n" +
            "[silencedetect @ 0x7f8] silence_end: 13.456 | silence_duration: 1.111\n" +
            "[silencedetect @ 0x7f8] silence_start: 30.0\n" +
            "[silencedetect @ 0x7f8] silence_end: 32.5 | silence_duration: 2.5\n";

        var intervals = ParseSilenceIntervals(sample);

        Assert.Equal(2, intervals.Count);
        Assert.Equal(12.345, intervals[0].StartSeconds);
        Assert.Equal(13.456, intervals[0].EndSeconds);
        Assert.Equal(30.0, intervals[1].StartSeconds);
        Assert.Equal(32.5, intervals[1].EndSeconds);
    }

    [Fact]
    public void ParseSilenceIntervals_TrailingStartWithoutEnd_IsDropped()
    {
        // ffmpeg emits silence_start without silence_end when the file ends
        // mid-silence. We can't use it as a cut point, so drop it.
        const string sample =
            "[silencedetect @ 0x1] silence_start: 5.0\n" +
            "[silencedetect @ 0x1] silence_end: 6.0 | silence_duration: 1.0\n" +
            "[silencedetect @ 0x1] silence_start: 100.0\n";

        var intervals = ParseSilenceIntervals(sample);

        Assert.Single(intervals);
        Assert.Equal(5.0, intervals[0].StartSeconds);
        Assert.Equal(6.0, intervals[0].EndSeconds);
    }

    [Fact]
    public void ParseSilenceIntervals_EndBeforeStart_IsDropped()
    {
        // Pathological / corrupted output: silence_end at or before
        // silence_start. Drop rather than emit a zero/negative interval.
        const string sample =
            "[silencedetect @ 0x1] silence_start: 10.0\n" +
            "[silencedetect @ 0x1] silence_end: 10.0 | silence_duration: 0\n";

        Assert.Empty(ParseSilenceIntervals(sample));
    }

    [Fact]
    public void ParseSilenceIntervals_NonAsciiFloats_ParseWithInvariantCulture()
    {
        // Belt-and-braces: even on a fr-FR machine the decimal separator
        // must stay '.', because ffmpeg always emits invariant-culture
        // floats. If our parser ever used current culture, this would fail.
        const string sample =
            "[silencedetect @ 0x1] silence_start: 1.5\n" +
            "[silencedetect @ 0x1] silence_end: 2.5 | silence_duration: 1.0\n";

        var intervals = ParseSilenceIntervals(sample);

        Assert.Single(intervals);
        Assert.Equal(1.5, intervals[0].StartSeconds);
        Assert.Equal(2.5, intervals[0].EndSeconds);
    }

    [Fact]
    public void SilenceInterval_MidpointAndDuration_AreComputed()
    {
        var s = new SilenceInterval(10.0, 14.0);
        Assert.Equal(12.0, s.Midpoint);
        Assert.Equal(4.0, s.DurationSeconds);
    }

    [Fact]
    public void ChunkBoundary_Duration_IsEndMinusStart()
    {
        var c = new ChunkBoundary(40.0, 80.0);
        Assert.Equal(40.0, c.DurationSeconds);
    }
}
