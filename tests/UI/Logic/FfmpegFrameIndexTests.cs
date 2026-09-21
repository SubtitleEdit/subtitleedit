using Nikse.SubtitleEdit.Logic.VideoPlayers.Ffmpeg;

namespace UITests.Logic;

public class FfmpegFrameIndexTests
{
    /// <summary>Frames at 0, 40, 80, 200, 240 ms (a gap, as in a variable frame rate file); key frames at 0 and 200.</summary>
    private static FfmpegFrameIndex VariableIndex()
    {
        // Decode order differs from presentation order, as with B-frames.
        var ticks = new long[] { 0, 80, 40, 200, 240 };
        var keys = new[] { true, false, false, true, false };
        return FfmpegFrameIndex.Create(ticks, keys, 0.001, 0);
    }

    [Fact]
    public void Create_SortsByPresentationTime()
    {
        var index = VariableIndex();

        Assert.Equal(5, index.Count);
        Assert.Equal(new[] { 0.0, 0.04, 0.08, 0.2, 0.24 }, Enumerable.Range(0, 5).Select(index.SecondsAt).ToArray());
        Assert.Equal(new[] { 0.0, 0.2 }, index.KeyFrameSeconds());
        Assert.Equal(200, index.TicksAt(3));
    }

    [Fact]
    public void Create_SubtractsStartTime()
    {
        var index = FfmpegFrameIndex.Create(new long[] { 1000, 1040 }, new[] { true, false }, 0.001, 1.0);

        Assert.Equal(0, index.SecondsAt(0), 6);
        Assert.Equal(0.04, index.SecondsAt(1), 6);
        Assert.Equal(1040, index.TicksAt(1)); // ticks stay raw: they go back to libavformat as they came
    }

    [Fact]
    public void Create_DuplicateTimeStampKeptOnce_KeyFlagWins()
    {
        var index = FfmpegFrameIndex.Create(new long[] { 0, 40, 40, 80 }, new[] { false, false, true, false }, 0.001, 0);

        Assert.Equal(3, index.Count);
        Assert.Equal(new[] { 0.04 }, index.KeyFrameSeconds());
    }

    [Theory]
    [InlineData(-1.0, 0)]
    [InlineData(0.019, 0)]
    [InlineData(0.021, 1)]
    [InlineData(0.13, 2)] // inside the gap, closer to 80 ms
    [InlineData(0.15, 3)] // inside the gap, closer to 200 ms
    [InlineData(99.0, 4)]
    public void NearestIndex_PicksTheClosestFrame(double seconds, int expected)
    {
        Assert.Equal(expected, VariableIndex().NearestIndex(seconds));
    }

    [Theory]
    [InlineData(-0.5, -1)]
    [InlineData(0.0, 0)]
    [InlineData(0.0399, 1)] // a time rounded down to milliseconds still finds its own frame
    [InlineData(0.19, 2)] // the 80 ms frame stays on screen through the gap
    [InlineData(0.2, 3)]
    public void IndexAtOrBefore_IsTheFrameOnScreen(double seconds, int expected)
    {
        Assert.Equal(expected, VariableIndex().IndexAtOrBefore(seconds));
    }

    [Fact]
    public void NeighbourIndex_StepsOverTheGapInOneStep()
    {
        var index = VariableIndex();

        Assert.Equal(3, index.NeighbourIndex(0.08, forward: true));
        Assert.Equal(2, index.NeighbourIndex(0.2, forward: false));
    }

    [Fact]
    public void NeighbourIndex_EndsOfFile()
    {
        var index = VariableIndex();

        Assert.Equal(-1, index.NeighbourIndex(0.0, forward: false));
        Assert.Equal(-1, index.NeighbourIndex(0.24, forward: true));
        Assert.Equal(0, index.NeighbourIndex(-5, forward: true));
    }

    [Fact]
    public void NeighbourIndex_BetweenFrames_BackGoesToTheFrameOnScreen()
    {
        var index = VariableIndex();

        Assert.Equal(2, index.NeighbourIndex(0.15, forward: false));
        Assert.Equal(3, index.NeighbourIndex(0.15, forward: true));
    }

    [Fact]
    public void KeyFrameLookups()
    {
        var index = VariableIndex();

        Assert.Equal(0, index.KeyFrameAtOrBefore(2));
        Assert.Equal(3, index.KeyFrameAtOrBefore(3));
        Assert.Equal(3, index.KeyFrameAtOrBefore(4));
        Assert.Equal(0, index.NearestKeyFrame(0.09));
        Assert.Equal(3, index.NearestKeyFrame(0.15)); // the key frame AFTER the target, when that is closer
        Assert.Equal(3, index.NearestKeyFrame(50));
    }

    [Fact]
    public void KeyFrameLookups_WithoutKeyFrames()
    {
        var index = FfmpegFrameIndex.Create(new long[] { 0, 40 }, new[] { false, false }, 0.001, 0);

        Assert.Equal(-1, index.KeyFrameAtOrBefore(1));
        Assert.Equal(-1, index.NearestKeyFrame(0.04));
    }

    [Fact]
    public void Contains_UsesHalfTheSmallestFrameDistance()
    {
        var index = VariableIndex();

        Assert.Equal(0.02, index.MatchTolerance, 6);
        Assert.True(index.Contains(0.0405));
        Assert.False(index.Contains(0.14));
    }

    [Fact]
    public void IsConstantFrameRate()
    {
        Assert.False(VariableIndex().IsConstantFrameRate);

        // 23.976: 1001/24000 s per frame in a 1/24000 time base.
        var ticks = Enumerable.Range(0, 500).Select(n => n * 1001L).ToArray();
        var cfr = FfmpegFrameIndex.Create(ticks, new bool[500], 1 / 24000.0, 0);
        Assert.True(cfr.IsConstantFrameRate);
    }

    [Fact]
    public void RealFrameTimes_DriftFromTheNominalGrid()
    {
        // The reason the index exists: after an hour at "23.976", the n/23.976 grid is 4 ms off
        // the real 1001/24000 frame times; the index has the real ones.
        var ticks = Enumerable.Range(0, 86400).Select(n => n * 1001L).ToArray();
        var index = FfmpegFrameIndex.Create(ticks, new bool[ticks.Length], 1 / 24000.0, 0);

        var frame = 86313;
        Assert.Equal(frame * 1001 / 24000.0, index.SecondsAt(frame), 9);
        Assert.Equal(frame, index.NearestIndex(frame / 23.976));
    }

    [Fact]
    public void EmptyIndex()
    {
        var index = FfmpegFrameIndex.Create(Array.Empty<long>(), Array.Empty<bool>(), 0.001, 0);

        Assert.Equal(0, index.Count);
        Assert.Equal(-1, index.NearestIndex(1));
        Assert.Equal(-1, index.NeighbourIndex(1, forward: true));
        Assert.False(index.Contains(0));
    }
}
