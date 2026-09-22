using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.VobSub;
using SkiaSharp;

namespace LibSETests.VobSub;

/// <summary>
/// Issue #15136 follow-up: GetBitmap crops the subpicture to its ink, but the position was
/// still the display area's origin. Discs that declare the whole frame as the display area
/// then had every cue "at the top-left" - seconv's --ocr-auto-detect-assa-alignment tagged
/// them all {\an7}/{\an8}. ImagePosition / GetPosition must follow the crop.
/// </summary>
public class SubPictureCropOffsetTest : IDisposable
{
    private readonly string _tempRoot;

    public SubPictureCropOffsetTest()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "SubPicCrop_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempRoot);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
        {
            Directory.Delete(_tempRoot, recursive: true);
        }
    }

    /// <summary>
    /// One cue whose display area is the full 720x576 frame, with an opaque block drawn at
    /// <paramref name="ink"/> inside it.
    /// </summary>
    private VobSubMergedPack WriteAndReadFullFramePack(SKRectI ink)
    {
        var subPath = Path.Combine(_tempRoot, "fullframe.sub");
        using (var writer = new VobSubWriter(
                   subPath, 720, 576, bottomMargin: 10, leftRightMargin: 10, languageStreamId: 32,
                   SKColors.White, SKColors.Black, useInnerAntiAliasing: false, DvdSubtitleLanguage.English))
        {
            using var bmp = new SKBitmap(new SKImageInfo(720, 576, SKColorType.Rgba8888, SKAlphaType.Unpremul));
            bmp.Erase(SKColors.Transparent);
            using (var canvas = new SKCanvas(bmp))
            using (var paint = new SKPaint { Color = SKColors.White })
            {
                canvas.DrawRect(ink, paint);
            }

            writer.WriteParagraph(new Paragraph(string.Empty, 1000, 3000), bmp, BluRayContentAlignment.BottomCenter, new SKPoint(0, 0));
            writer.WriteIdxFile();
        }

        var parser = new VobSubParser(true);
        parser.OpenSubIdx(subPath, Path.ChangeExtension(subPath, ".idx"));
        var packs = parser.MergeVobSubPacks();
        Assert.Single(packs);
        packs[0].Palette = parser.IdxPalette;
        return packs[0];
    }

    [Fact]
    public void GetPosition_FollowsTheCrop_WhenDisplayAreaIsTheWholeFrame()
    {
        var ink = new SKRectI(300, 500, 420, 540);
        var pack = WriteAndReadFullFramePack(ink);

        using var bmp = pack.GetBitmap();
        var position = pack.GetPosition();

        // The display area is still the whole frame...
        Assert.Equal(0, pack.SubPicture.ImageDisplayArea.Left);
        Assert.Equal(0, pack.SubPicture.ImageDisplayArea.Top);
        Assert.True(pack.SubPicture.ImageDisplayArea.Right >= 700, "display area should span the frame");

        // ...but the bitmap is cropped to the ink (with the crop's few pixels of margin), so
        // the position must be where that crop sits, not (0,0).
        Assert.True(bmp.Width < 200 && bmp.Height < 100, $"expected a cropped bitmap, got {bmp.Width}x{bmp.Height}");
        Assert.InRange(position.Left, ink.Left - 8, ink.Left);
        Assert.InRange(position.Top, ink.Top - 8, ink.Top);
        Assert.Equal(pack.SubPicture.ImagePosition.X, position.Left);
        Assert.Equal(pack.SubPicture.ImagePosition.Y, position.Top);

        // The cropped bitmap placed at that position covers the ink.
        Assert.True(position.Left + bmp.Width >= ink.Right, "crop + position should reach the ink's right edge");
        Assert.True(position.Top + bmp.Height >= ink.Bottom, "crop + position should reach the ink's bottom edge");
    }

    [Fact]
    public void ImageCropOffset_IsZero_WhenNotCropping()
    {
        var pack = WriteAndReadFullFramePack(new SKRectI(300, 500, 420, 540));

        using var uncropped = pack.SubPicture.GetBitmap(pack.Palette, SKColors.Transparent, SKColors.Black, SKColors.White, SKColors.Black, false, crop: false);

        Assert.True(uncropped.Width >= 700, "uncropped bitmap should span the display area");
        Assert.Equal(new SKPointI(0, 0), pack.SubPicture.ImageCropOffset);
        Assert.Equal(0, pack.SubPicture.ImagePosition.X);
        Assert.Equal(0, pack.SubPicture.ImagePosition.Y);
    }
}
