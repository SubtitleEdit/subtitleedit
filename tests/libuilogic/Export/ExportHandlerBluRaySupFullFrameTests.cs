using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.UiLogic.Export;
using SkiaSharp;
using System.Text;

namespace LibUiLogicTests.Export;

/// <summary>
/// The "full frame" branch of the Blu-ray sup handler never drew the subtitle onto the frame it
/// built (the draw call was commented out during the port), so it wrote blank frames.
/// </summary>
public class ExportHandlerBluRaySupFullFrameTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "bluraysup_" + Guid.NewGuid().ToString("N"));

    public ExportHandlerBluRaySupFullFrameTests() => Directory.CreateDirectory(_dir);

    public void Dispose()
    {
        try { Directory.Delete(_dir, true); } catch { /* best effort */ }
    }

    private static ImageParameter Cue(bool fullFrame)
    {
        var bitmap = new SKBitmap(300, 80);
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(SKColors.Transparent);
            using var paint = new SKPaint { Color = SKColors.White };
            canvas.DrawRect(0, 0, 300, 80, paint);
        }

        return new ImageParameter
        {
            Text = "Hello",
            Bitmap = bitmap,
            StartTime = TimeSpan.FromSeconds(1),
            EndTime = TimeSpan.FromSeconds(3),
            Index = 0,
            ScreenWidth = 1280,
            ScreenHeight = 720,
            Alignment = ExportAlignment.BottomCenter,
            BottomTopMargin = 50,
            LeftRightMargin = 40,
            FramesPerSecond = 25,
            FontColor = SKColors.White,
            IsFullFrame = fullFrame,
        };
    }

    private SKBitmap ExportAndReadBack(bool fullFrame)
    {
        var fileName = Path.Combine(_dir, fullFrame + ".sup");
        var handler = new ExportHandlerBluRaySup();
        var cue = Cue(fullFrame);

        handler.WriteHeader(fileName, cue);
        handler.CreateParagraph(cue);
        handler.WriteParagraph(cue);
        handler.WriteFooter();

        var subtitles = BluRaySupParser.ParseBluRaySup(fileName, new StringBuilder());
        return Assert.Single(subtitles).GetBitmap();
    }

    [Fact]
    public void FullFrame_WritesAFrameSizedImageWithTheSubtitleInIt()
    {
        using var bitmap = ExportAndReadBack(fullFrame: true);

        Assert.Equal(1280, bitmap.Width);
        Assert.Equal(720, bitmap.Height);

        // Bottom centered with a 50 px bottom margin, as the alignment and margins ask for.
        Assert.True(bitmap.GetPixel(1280 / 2, 720 - 50 - 1).Alpha > 0, "the subtitle is missing from the full frame image");
        Assert.Equal(0, bitmap.GetPixel(0, 0).Alpha);
    }

    [Fact]
    public void WithoutFullFrame_TheImageIsStillCroppedToTheSubtitle()
    {
        using var bitmap = ExportAndReadBack(fullFrame: false);

        Assert.True(bitmap.Width < 1280, "the cropped export should not cover the frame");
        Assert.True(bitmap.Height < 720, "the cropped export should not cover the frame");
    }
}
