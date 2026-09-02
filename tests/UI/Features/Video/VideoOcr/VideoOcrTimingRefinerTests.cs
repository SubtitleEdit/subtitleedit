using System.Collections.Generic;
using Nikse.SubtitleEdit.Features.Video.VideoOcr;

namespace UITests.Features.Video.VideoOcr;

public class VideoOcrTimingRefinerTests
{
    [Fact]
    public void PickTransition_Start_FirstSimilarFrameWins()
    {
        var frames = new List<(double, bool)>
        {
            (1000, false),
            (1040, false),
            (1080, true), // subtitle appears here
            (1120, true),
            (1160, true),
        };

        var refined = VideoOcrTimingRefiner.PickTransition(frames, findStart: true, 40);

        Assert.Equal(1080, refined);
    }

    [Fact]
    public void PickTransition_End_LastSimilarFramePlusOneStep()
    {
        var frames = new List<(double, bool)>
        {
            (1000, true),
            (1040, true), // last frame with the subtitle - it stays visible for one frame
            (1080, false),
            (1120, false),
        };

        var refined = VideoOcrTimingRefiner.PickTransition(frames, findStart: false, 40);

        Assert.Equal(1080, refined);
    }

    [Fact]
    public void PickTransition_NoSimilarFrame_ReturnsNull()
    {
        // A fade or borderline mask: nothing matches, so the coarse time must be kept.
        var frames = new List<(double, bool)>
        {
            (1000, false),
            (1040, false),
        };

        Assert.Null(VideoOcrTimingRefiner.PickTransition(frames, findStart: true, 40));
        Assert.Null(VideoOcrTimingRefiner.PickTransition(frames, findStart: false, 40));
    }

    [Fact]
    public void PickTransition_AllSimilar_StartIsWindowStart_EndIsPastWindow()
    {
        var frames = new List<(double, bool)>
        {
            (1000, true),
            (1040, true),
        };

        Assert.Equal(1000, VideoOcrTimingRefiner.PickTransition(frames, findStart: true, 40));
        Assert.Equal(1080, VideoOcrTimingRefiner.PickTransition(frames, findStart: false, 40));
    }

    [Fact]
    public void PickTransition_EmptyWindow_ReturnsNull()
    {
        var frames = new List<(double, bool)>();

        Assert.Null(VideoOcrTimingRefiner.PickTransition(frames, findStart: true, 40));
        Assert.Null(VideoOcrTimingRefiner.PickTransition(frames, findStart: false, 40));
    }
}
