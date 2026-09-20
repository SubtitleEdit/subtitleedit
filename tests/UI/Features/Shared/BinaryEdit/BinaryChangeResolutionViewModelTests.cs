using Nikse.SubtitleEdit.Features.Shared.BinaryEdit;
using Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinaryChangeResolution;
using System.Collections.Generic;

namespace UITests.Features.Shared.BinaryEdit;

public class BinaryChangeResolutionViewModelTests
{
    [Fact]
    public void ApplyResolution_ScalesPositionsByWidthAndHeightIndependently()
    {
        var item = new BinarySubtitleItem(TimeSpan.Zero, TimeSpan.FromSeconds(1)) { X = 960, Y = 1000 };

        BinaryChangeResolutionViewModel.ApplyResolution(new List<BinarySubtitleItem> { item }, 1920, 1080, 720, 576);

        Assert.Equal(360, item.X);
        Assert.Equal(533, item.Y); // 1000 * 576 / 1080 = 533.3
    }

    [Fact]
    public void ApplyResolution_SameSizeIsNoOp()
    {
        var item = new BinarySubtitleItem(TimeSpan.Zero, TimeSpan.FromSeconds(1)) { X = 7, Y = 9 };

        BinaryChangeResolutionViewModel.ApplyResolution(new List<BinarySubtitleItem> { item }, 1920, 1080, 1920, 1080);

        Assert.Equal(7, item.X);
        Assert.Equal(9, item.Y);
    }

    [Theory]
    [InlineData(0, 1280, 1.0)]
    [InlineData(1920, 0, 1.0)]
    [InlineData(1920, 1280, 2.0 / 3.0)]
    public void GetScale_UnknownSourceOrTargetMeansNoScaling(int from, int to, double expected)
    {
        Assert.Equal(expected, BinaryChangeResolutionViewModel.GetScale(from, to), 6);
    }

    [Fact]
    public void ScaleCoordinate_RoundsHalfAwayFromZero()
    {
        Assert.Equal(1, BinaryChangeResolutionViewModel.ScaleCoordinate(1, 0.5));
        Assert.Equal(2, BinaryChangeResolutionViewModel.ScaleCoordinate(3, 0.5));
    }
}
