using Nikse.SubtitleEdit.Features.Video.RemoveBurnedIn;

namespace UITests.Features.Video.RemoveBurnedIn;

public class BurnedInRectangleTests
{
    private static BurnedInRemovalOptions Options(params BurnedInRectangle[] rectangles) => new()
    {
        VideoWidth = 100,
        VideoHeight = 50,
        FrameRate = 25,
        DurationSeconds = 10,
        Rectangles = rectangles,
    };

    [Fact]
    public void RectangleBoxes_CoverTheSpanFramesAndAreClampedToTheVideo()
    {
        var boxes = BurnedInSubtitleRemover.GetRectangleBoxes(Options(
            new BurnedInRectangle(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(4), 90, -5, 30, 20),
            new BurnedInRectangle(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1), 0, 0, 10, 10), // empty span
            new BurnedInRectangle(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), 200, 0, 10, 10))); // outside

        var box = Assert.Single(boxes);
        Assert.Equal((50L, 99L, 90, 0, 10, 15), box); // frames 2 s .. just before 4 s
    }

    [Fact]
    public void FillBoxes_FillsOnlyFramesInsideTheSpan()
    {
        var boxes = BurnedInSubtitleRemover.GetRectangleBoxes(Options(
            new BurnedInRectangle(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(4), 10, 5, 4, 3)));
        var mask = new byte[100 * 50];

        Assert.False(BurnedInSubtitleRemover.FillBoxes(boxes, 49, 100, mask));
        Assert.False(BurnedInSubtitleRemover.FillBoxes(boxes, 100, 100, mask));
        Assert.All(mask, v => Assert.Equal(0, v));

        Assert.True(BurnedInSubtitleRemover.FillBoxes(boxes, 50, 100, mask));
        Assert.Equal(4 * 3, mask.Count(v => v == 255));
        Assert.Equal(255, mask[5 * 100 + 10]);
        Assert.Equal(255, mask[7 * 100 + 13]);
        Assert.Equal(0, mask[8 * 100 + 13]);
        Assert.Equal(0, mask[7 * 100 + 14]);
    }

    [Fact]
    public void RectangleBoxes_GoIntoTheFilterGraph()
    {
        var o = new BurnedInRemovalOptions
        {
            VideoWidth = 1280, VideoHeight = 720, FrameRate = 25, Method = BurnedInMethod.Delogo,
            Rectangles = new[] { new BurnedInRectangle(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(4), 1100, 40, 120, 60) },
        };
        var graph = BurnedInSubtitleRemover.MakeFilterGraph(o, BurnedInSubtitleRemover.GetRectangleBoxes(o), "m.ass");
        Assert.Contains("delogo=x=1100:y=40:w=120:h=60:enable='between(t,2,4)'", graph);
    }
}
