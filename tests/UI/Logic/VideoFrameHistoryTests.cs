using Nikse.SubtitleEdit.Logic.VideoPlayers.Ffmpeg;

namespace UITests.Logic;

public class VideoFrameHistoryTests
{
    private static VideoFrame Frame(VideoFrameQueue pool, int serial, double pts, int width = 16, int height = 8)
    {
        var current = serial;
        var frame = pool.Rent(width, height, serial, ref current)!;
        frame.Serial = serial;
        frame.Pts = pts;
        return frame;
    }

    [Fact]
    public void TakeNewest_HandsFramesBackNewestFirst()
    {
        var pool = new VideoFrameQueue(3);
        var history = new VideoFrameHistory(pool);
        history.Reset(1);
        var a = Frame(pool, 1, 0.00);
        var b = Frame(pool, 1, 0.04);
        history.Add(a);
        history.Add(b);

        Assert.Same(b, history.TakeNewest(1));
        Assert.Same(a, history.TakeNewest(1));
        Assert.Null(history.TakeNewest(1));
        pool.Close();
    }

    [Fact]
    public void Add_FrameFromAnotherSerial_IsRefused()
    {
        var pool = new VideoFrameQueue(3);
        var history = new VideoFrameHistory(pool);
        history.Reset(2);

        history.Add(Frame(pool, 1, 0.5)); // decoded before the seek that started serial 2

        Assert.Equal(0, history.Count);
        pool.Close();
    }

    [Fact]
    public void TakeNewest_ForAnotherSerial_GivesNothing()
    {
        var pool = new VideoFrameQueue(3);
        var history = new VideoFrameHistory(pool);
        history.Reset(1);
        history.Add(Frame(pool, 1, 0.5));

        Assert.Null(history.TakeNewest(2));
        Assert.Equal(1, history.Count);
        pool.Close();
    }

    [Fact]
    public void Add_TimeGoingBackwards_StartsTheRunOver()
    {
        var pool = new VideoFrameQueue(3);
        var history = new VideoFrameHistory(pool);
        history.Reset(1);
        history.Add(Frame(pool, 1, 1.00));
        history.Add(Frame(pool, 1, 1.04));

        var restart = Frame(pool, 1, 0.50);
        history.Add(restart);

        Assert.Equal(1, history.Count);
        Assert.Same(restart, history.TakeNewest(1));
        pool.Close();
    }

    [Fact]
    public void Add_BeyondCapacity_DropsTheOldest()
    {
        var pool = new VideoFrameQueue(3);
        var history = new VideoFrameHistory(pool);
        history.Reset(1);
        for (var i = 0; i < VideoFrameHistory.MaxFrames + 5; i++)
        {
            history.Add(Frame(pool, 1, i * 0.04));
        }

        Assert.Equal(VideoFrameHistory.MaxFrames, history.Count);
        Assert.Equal((VideoFrameHistory.MaxFrames + 4) * 0.04, history.TakeNewest(1)!.Pts, 6);
        pool.Close();
    }

    [Fact]
    public void Reset_ReturnsTheFramesToThePool()
    {
        var pool = new VideoFrameQueue(3);
        var history = new VideoFrameHistory(pool);
        history.Reset(1);
        var frame = Frame(pool, 1, 0.5);
        history.Add(frame);

        history.Reset(2);

        Assert.Equal(0, history.Count);
        var serial = 2;
        Assert.Same(frame, pool.Rent(16, 8, 2, ref serial)); // pooled, not leaked
        pool.Close();
    }

    [Theory]
    [InlineData(1920, 1080, 8)] // 7.9 MiB pictures: 64 MiB holds 8
    [InlineData(1280, 720, 16)]
    [InlineData(640, 360, 16)]
    public void CapacityFor_FollowsTheMemoryBudget(int width, int height, int expected)
    {
        Assert.Equal(expected, VideoFrameHistory.CapacityFor(VideoFrame.StrideFor(width), height));
    }
}
