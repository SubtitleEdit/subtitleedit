using Nikse.SubtitleEdit.UiLogic.Export;
using SkiaSharp;

namespace LibUiLogicTests.Export;

/// <summary>
/// SE4's 3D export (half side-by-side / half top-bottom): the subtitle is squeezed into each
/// eye's half of the frame, at the place the flat subtitle would have, and the two copies move
/// apart by the depth.
/// </summary>
public class Stereo3DImageTests
{
    private const int ScreenWidth = 1920;
    private const int ScreenHeight = 1080;

    private static ImageParameter MakeParameter(int width, int height, Export3DMode mode, int depth = 0,
        ExportAlignment alignment = ExportAlignment.BottomCenter, int margin = 10)
    {
        var bitmap = new SKBitmap(width, height);
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(SKColors.White);
        }

        return new ImageParameter
        {
            Bitmap = bitmap,
            ScreenWidth = ScreenWidth,
            ScreenHeight = ScreenHeight,
            Alignment = alignment,
            BottomTopMargin = margin,
            LeftRightMargin = margin,
            Mode3D = mode,
            Depth3D = depth,
        };
    }

    [Fact]
    public void None_LeavesTheBitmapAndPositionAlone()
    {
        var ip = MakeParameter(100, 40, Export3DMode.None);
        var bitmap = ip.Bitmap;

        Stereo3DImage.Apply(ip);

        Assert.Same(bitmap, ip.Bitmap);
        Assert.Null(ip.OverridePosition);
    }

    [Fact]
    public void HalfSideBySide_PutsAHalfWidthCopyInEachEye()
    {
        // Flat: 100x40 bottom centered at 910,1030. Each eye's view is 960 wide, so the copy is
        // 50 wide at 455 in the left half and at 960 + 455 in the right half.
        var ip = MakeParameter(100, 40, Export3DMode.HalfSideBySide);

        Stereo3DImage.Apply(ip);

        Assert.Equal(new SKPointI(455, 1030), ip.OverridePosition);
        Assert.Equal(960 + 50, ip.Bitmap.Width);
        Assert.Equal(40, ip.Bitmap.Height);
        Assert.True(ip.Bitmap.GetPixel(25, 20).Alpha > 0, "left eye copy missing");
        Assert.Equal(0, ip.Bitmap.GetPixel(500, 20).Alpha);
        Assert.True(ip.Bitmap.GetPixel(960 + 25, 20).Alpha > 0, "right eye copy missing");
    }

    [Fact]
    public void HalfSideBySide_PositiveDepthMovesTheEyesTowardsEachOther()
    {
        var ip = MakeParameter(100, 40, Export3DMode.HalfSideBySide, depth: 5);

        Stereo3DImage.Apply(ip);

        // Left eye copy 5 to the right, right eye copy 5 to the left.
        Assert.Equal(new SKPointI(460, 1030), ip.OverridePosition);
        Assert.Equal(960 + 50 - 10, ip.Bitmap.Width);
    }

    [Fact]
    public void HalfTopBottom_PutsAHalfHeightCopyInEachEye_KeepingTheMarginOnScreen()
    {
        // Flat: 100x40 at 910,1030. Each eye's view is 540 high, so the copy is 20 high at 515 in
        // the top half and at 540 + 515 in the bottom half - 5 frame pixels from the bottom, which
        // the player stretches back to the 10 pixel margin.
        var ip = MakeParameter(100, 40, Export3DMode.HalfTopBottom);

        Stereo3DImage.Apply(ip);

        Assert.Equal(new SKPointI(910, 515), ip.OverridePosition);
        Assert.Equal(100, ip.Bitmap.Width);
        Assert.Equal(540 + 20, ip.Bitmap.Height);
        Assert.True(ip.Bitmap.GetPixel(50, 10).Alpha > 0, "top eye copy missing");
        Assert.Equal(0, ip.Bitmap.GetPixel(50, 100).Alpha);
        Assert.True(ip.Bitmap.GetPixel(50, 540 + 10).Alpha > 0, "bottom eye copy missing");
    }

    [Fact]
    public void HalfTopBottom_DepthShiftsTheCopiesSideways()
    {
        var ip = MakeParameter(100, 40, Export3DMode.HalfTopBottom, depth: 8);

        Stereo3DImage.Apply(ip);

        // Top copy at 918, bottom copy at 902.
        Assert.Equal(new SKPointI(902, 515), ip.OverridePosition);
        Assert.Equal(100 + 16, ip.Bitmap.Width);
        Assert.True(ip.Bitmap.GetPixel(16 + 99, 10).Alpha > 0);
        Assert.Equal(0, ip.Bitmap.GetPixel(0, 10).Alpha);
        Assert.True(ip.Bitmap.GetPixel(0, 540 + 10).Alpha > 0);
    }

    [Fact]
    public void FollowsAPositionOverride()
    {
        // "{\pos(..)}" (or a source bitmap's own position) is where the flat subtitle goes.
        var ip = MakeParameter(100, 40, Export3DMode.HalfSideBySide, alignment: ExportAlignment.TopLeft);
        ip.OverridePosition = new SKPointI(300, 100);

        Stereo3DImage.Apply(ip);

        Assert.Equal(new SKPointI(150, 100), ip.OverridePosition);
    }

    [Fact]
    public void AnEyesCopyNeverCrossesIntoTheOtherEyesHalf()
    {
        // Right aligned, 200 wide: the left eye copy is 100 wide at 860, and a depth of 50 moves it
        // to 910 - past the middle of the frame, where the right eye would see its last 50 pixels.
        var ip = MakeParameter(200, 40, Export3DMode.HalfSideBySide, depth: 50, alignment: ExportAlignment.BottomRight, margin: 0);

        Stereo3DImage.Apply(ip);

        Assert.Equal(910, ip.OverridePosition!.Value.X);
        Assert.True(ip.Bitmap.GetPixel(955 - 910, 20).Alpha > 0);
        Assert.Equal(0, ip.Bitmap.GetPixel(965 - 910, 20).Alpha);
        Assert.True(ip.Bitmap.GetPixel(1775 - 910, 20).Alpha > 0);
    }

    [Fact]
    public void DisposeSourceFalse_KeepsTheCallersBitmap()
    {
        var ip = MakeParameter(100, 40, Export3DMode.HalfSideBySide);
        var source = ip.Bitmap;

        Stereo3DImage.Apply(ip, disposeSource: false);

        Assert.NotSame(source, ip.Bitmap);
        Assert.Equal(SKColors.White, source.GetPixel(0, 0));
        source.Dispose();
    }

    [Fact]
    public void DCinema_HasNoModeButEveryOtherImageFormatDoes()
    {
        Assert.False(Stereo3DImage.IsModeSupported(ExportImageType.DCinemaPng));
        Assert.True(Stereo3DImage.IsModeSupported(ExportImageType.BluRaySup));
        Assert.True(Stereo3DImage.IsModeSupported(ExportImageType.VobSub));
    }
}
