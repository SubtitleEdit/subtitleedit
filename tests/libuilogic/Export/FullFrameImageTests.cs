using Nikse.SubtitleEdit.UiLogic.Export;
using SkiaSharp;

namespace LibUiLogicTests.Export;

public class FullFrameImageTests
{
    private static ImageParameter Param(ExportAlignment alignment, int bitmapWidth = 300, int bitmapHeight = 80)
    {
        var bitmap = new SKBitmap(bitmapWidth, bitmapHeight);
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(SKColors.Transparent);
            using var paint = new SKPaint { Color = SKColors.Red };
            canvas.DrawRect(0, 0, bitmapWidth, bitmapHeight, paint);
        }

        return new ImageParameter
        {
            Bitmap = bitmap,
            ScreenWidth = 1920,
            ScreenHeight = 1080,
            Alignment = alignment,
            BottomTopMargin = 50,
            LeftRightMargin = 40,
            IsFullFrame = true,
        };
    }

    [Fact]
    public void GetPosition_BottomCenter_CentersAndUsesBottomMargin()
    {
        var param = Param(ExportAlignment.BottomCenter);

        var position = FullFrameImage.GetPosition(param, param.Bitmap.Width, param.Bitmap.Height);

        Assert.Equal((1920 - 300) / 2, position.X);
        Assert.Equal(1080 - 80 - 50, position.Y);
    }

    [Fact]
    public void GetPosition_TopLeft_UsesBothMargins()
    {
        var param = Param(ExportAlignment.TopLeft);

        var position = FullFrameImage.GetPosition(param, param.Bitmap.Width, param.Bitmap.Height);

        Assert.Equal(40, position.X);
        Assert.Equal(50, position.Y);
    }

    [Fact]
    public void GetPosition_BottomRight_MeasuresFromTheRightEdge()
    {
        var param = Param(ExportAlignment.BottomRight);

        var position = FullFrameImage.GetPosition(param, param.Bitmap.Width, param.Bitmap.Height);

        Assert.Equal(1920 - 300 - 40, position.X);
        Assert.Equal(1080 - 80 - 50, position.Y);
    }

    /// <summary>
    /// Middle alignment centers the bitmap vertically. The Blu-ray sup handler used to compute
    /// "ScreenHeight - Height / 2", which put the subtitle below the bottom edge.
    /// </summary>
    [Fact]
    public void GetPosition_MiddleCenter_CentersVertically()
    {
        var param = Param(ExportAlignment.MiddleCenter);

        var position = FullFrameImage.GetPosition(param, param.Bitmap.Width, param.Bitmap.Height);

        Assert.Equal((1920 - 300) / 2, position.X);
        Assert.Equal((1080 - 80) / 2, position.Y);
    }

    [Fact]
    public void GetPosition_OverridePositionInsideFrame_Wins()
    {
        var param = Param(ExportAlignment.BottomCenter);
        param.OverridePosition = new SKPointI(111, 222);

        var position = FullFrameImage.GetPosition(param, param.Bitmap.Width, param.Bitmap.Height);

        Assert.Equal(111, position.X);
        Assert.Equal(222, position.Y);
    }

    [Fact]
    public void GetPosition_OverridePositionOutsideFrame_FallsBackToAlignment()
    {
        var param = Param(ExportAlignment.BottomCenter);
        param.OverridePosition = new SKPointI(5000, -10);

        var position = FullFrameImage.GetPosition(param, param.Bitmap.Width, param.Bitmap.Height);

        Assert.Equal((1920 - 300) / 2, position.X);
        Assert.Equal(1080 - 80 - 50, position.Y);
    }

    [Fact]
    public void Create_ReturnsFrameSizedBitmapWithTheSubtitleDrawnIn()
    {
        var param = Param(ExportAlignment.BottomCenter);

        using var fullFrame = FullFrameImage.Create(param);

        Assert.Equal(1920, fullFrame.Width);
        Assert.Equal(1080, fullFrame.Height);

        var position = FullFrameImage.GetPosition(param, param.Bitmap.Width, param.Bitmap.Height);
        Assert.Equal(SKColors.Red, fullFrame.GetPixel(position.X + 1, position.Y + 1));
        Assert.Equal(SKColors.Red, fullFrame.GetPixel(position.X + 299, position.Y + 79));
    }

    [Fact]
    public void Create_DefaultBackgroundIsTransparent()
    {
        var param = Param(ExportAlignment.BottomCenter);

        using var fullFrame = FullFrameImage.Create(param);

        Assert.Equal(0, fullFrame.GetPixel(0, 0).Alpha);
    }

    [Fact]
    public void Create_FillsWithTheFullFrameBackgroundColor()
    {
        var param = Param(ExportAlignment.BottomCenter);
        param.FullFrameBackgroundColor = SKColors.Blue;

        using var fullFrame = FullFrameImage.Create(param);

        Assert.Equal(SKColors.Blue, fullFrame.GetPixel(0, 0));
        Assert.Equal(SKColors.Blue, fullFrame.GetPixel(1919, 1079));
    }

    /// <summary>
    /// The box colour behind the text and the full frame background are separate settings - a
    /// black box behind the subtitle must not paint the whole frame black.
    /// </summary>
    [Fact]
    public void Create_IgnoresTheTextBoxBackgroundColor()
    {
        var param = Param(ExportAlignment.BottomCenter);
        param.BackgroundColor = SKColors.Black;

        using var fullFrame = FullFrameImage.Create(param);

        Assert.Equal(0, fullFrame.GetPixel(0, 0).Alpha);
    }
}
