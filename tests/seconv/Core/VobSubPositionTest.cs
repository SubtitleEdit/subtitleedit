using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.VobSub;
using Nikse.SubtitleEdit.UiLogic.Ocr;
using SeConv.Core;
using SkiaSharp;
using Xunit;
using Paragraph = Nikse.SubtitleEdit.Core.Common.Paragraph;

namespace SeConvTests.Core;

/// <summary>
/// Issue #15136 (gambar3, beta 9): --ocr-auto-detect-assa-alignment put {\an7}/{\an8} on
/// every VobSub cue. The loader paired the bitmap (cropped to the ink) with the display
/// area's origin, and on discs that declare the whole frame as the display area that is
/// (0,0) - so every line scored as top-left, or top-centre when wide enough.
/// </summary>
public class VobSubPositionTest : IDisposable
{
    private readonly string _tempRoot;

    public VobSubPositionTest()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "VobPos_" + Guid.NewGuid().ToString("N"));
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
    /// Writes a .sub/.idx where every cue's display area is the full 720x576 frame and the
    /// ink sits at the given rectangle inside it.
    /// </summary>
    private string WriteFullFramePair(params SKRectI[] inks)
    {
        var subPath = Path.Combine(_tempRoot, "fullframe.sub");
        using (var writer = new VobSubWriter(
                   subPath, 720, 576, bottomMargin: 10, leftRightMargin: 10, languageStreamId: 32,
                   SKColors.White, SKColors.Black, useInnerAntiAliasing: false, DvdSubtitleLanguage.English))
        {
            var start = 1000;
            foreach (var ink in inks)
            {
                using var bmp = new SKBitmap(new SKImageInfo(720, 576, SKColorType.Rgba8888, SKAlphaType.Unpremul));
                bmp.Erase(SKColors.Transparent);
                using (var canvas = new SKCanvas(bmp))
                using (var paint = new SKPaint { Color = SKColors.White })
                {
                    canvas.DrawRect(ink, paint);
                }

                writer.WriteParagraph(new Paragraph(string.Empty, start, start + 2000), bmp, BluRayContentAlignment.BottomCenter, new SKPoint(0, 0));
                start += 3000;
            }

            writer.WriteIdxFile();
        }

        return subPath;
    }

    [Fact]
    public void LoadVobSub_PositionFollowsTheInk_NotTheFullFrameDisplayArea()
    {
        var bottomCentre = new SKRectI(300, 500, 420, 540);
        var topCentre = new SKRectI(300, 30, 420, 70);
        var middleRight = new SKRectI(600, 270, 700, 300);
        var subPath = WriteFullFramePair(bottomCentre, topCentre, middleRight);

        var items = BitmapSubtitleLoader.LoadVobSub(subPath, Path.ChangeExtension(subPath, ".idx"), isPal: true);

        Assert.Equal(3, items.Count);
        var expected = new[] { bottomCentre, topCentre, middleRight };
        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            Assert.NotNull(item.Position);
            Assert.InRange(item.Position!.Value.X, expected[i].Left - 8, expected[i].Left);
            Assert.InRange(item.Position.Value.Y, expected[i].Top - 8, expected[i].Top);
            Assert.True(item.Bitmap.Height < 100, $"cue {i}: bitmap should be cropped to the ink, got {item.Bitmap.Width}x{item.Bitmap.Height}");
        }

        // What --ocr-auto-detect-assa-alignment makes of it: bottom-centre gets no tag,
        // the others get the tag for where the ink is - never the top-left of the frame.
        var tags = items.Select(item => OcrAssaAlignment.Detect(
            item.Bitmap, item.Position!.Value.X, item.Position.Value.Y, item.ScreenWidth!.Value, item.ScreenHeight!.Value,
            "Text", writeAn2Tag: false).Text).ToList();
        Assert.Equal(new[] { "Text", "{\\an8}Text", "{\\an6}Text" }, tags);

        foreach (var item in items)
        {
            item.Dispose();
        }
    }
}
