using Nikse.SubtitleEdit.UiLogic.Export;
using SeConv.Core;
using SkiaSharp;
using Xunit;

namespace SeConvTests.Core;

/// <summary>
/// <c>--override-position</c>: SE4's transport-stream "override original X/Y position" for
/// the image → image pass-through path.
/// </summary>
public class PositionOverrideTest
{
    private static readonly SKBitmap Bitmap = new(100, 40);

    [Fact]
    public void NoOverride_KeepsSourcePosition()
    {
        var style = new ImageExportStyle();
        var result = ImageOutputWriter.ApplyPositionOverride(new SKPointI(10, 20), Bitmap, 1920, 1080, style);
        Assert.Equal(new SKPointI(10, 20), result);
    }

    [Fact]
    public void NoOverride_NoSourcePosition_StaysNull()
    {
        var style = new ImageExportStyle();
        Assert.Null(ImageOutputWriter.ApplyPositionOverride(null, Bitmap, 1920, 1080, style));
    }

    [Fact]
    public void OverrideX_CentersHorizontally_KeepsSourceY()
    {
        var style = new ImageExportStyle { OverridePositionX = true };
        var result = ImageOutputWriter.ApplyPositionOverride(new SKPointI(10, 20), Bitmap, 1920, 1080, style);
        Assert.Equal(new SKPointI((1920 - 100) / 2, 20), result);
    }

    [Fact]
    public void OverrideY_UsesBottomMargin_KeepsSourceX()
    {
        var style = new ImageExportStyle { OverridePositionY = true, BottomTopMargin = 30 };
        var result = ImageOutputWriter.ApplyPositionOverride(new SKPointI(10, 20), Bitmap, 1920, 1080, style);
        Assert.Equal(new SKPointI(10, 1080 - 30 - 40), result);
    }

    [Fact]
    public void OverrideBoth_LeftAlignment_UsesLeftMargin()
    {
        var style = new ImageExportStyle
        {
            OverridePositionX = true,
            OverridePositionY = true,
            Alignment = ExportAlignment.BottomLeft,
            LeftRightMargin = 25,
            BottomTopMargin = 30,
        };
        var result = ImageOutputWriter.ApplyPositionOverride(new SKPointI(500, 500), Bitmap, 1920, 1080, style);
        Assert.Equal(new SKPointI(25, 1010), result);
    }

    [Fact]
    public void OverrideBoth_RightAlignment_UsesRightMargin()
    {
        var style = new ImageExportStyle
        {
            OverridePositionX = true,
            OverridePositionY = true,
            Alignment = ExportAlignment.TopRight,
            LeftRightMargin = 25,
            BottomTopMargin = 30,
        };
        var result = ImageOutputWriter.ApplyPositionOverride(null, Bitmap, 1920, 1080, style);
        Assert.Equal(new SKPointI(1920 - 25 - 100, 30), result);
    }

    [Fact]
    public void DefaultMargins_AreFivePercentOfScreen()
    {
        var style = new ImageExportStyle { OverridePositionX = true, OverridePositionY = true, Alignment = ExportAlignment.TopLeft };
        var result = ImageOutputWriter.ApplyPositionOverride(null, Bitmap, 1000, 800, style);
        Assert.Equal(new SKPointI(50, 40), result);
    }
}
