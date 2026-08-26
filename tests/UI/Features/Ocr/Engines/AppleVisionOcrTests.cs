using Nikse.SubtitleEdit.Features.Ocr.Engines;
using SkiaSharp;
using System.Threading;

namespace UITests.Features.Ocr.Engines;

/// <summary>
/// End-to-end cover for the Vision interop: does the framework actually load, list languages and
/// read a subtitle image through objc_msgSend.
///
/// Every test here is skipped off macOS, so CI (Linux) never runs them - which is the point of
/// <see cref="AppleVisionTextLayoutTests"/> carrying the logic that can be tested anywhere. What
/// is left here is exactly the part no amount of pure logic can stand in for: the P/Invoke
/// signatures, the selector names, and whether Vision reads SE-shaped images at all.
///
/// The images are drawn here rather than checked in: white subtitle text with a black outline on
/// a transparent background is what SE hands an OCR engine once it has pulled a Blu-ray sup or
/// VobSub image apart, and drawing it keeps the fixture honest about that.
/// </summary>
public class AppleVisionOcrTests
{
    private static bool SkipIfUnavailable()
    {
        if (!OperatingSystem.IsMacOS())
        {
            Assert.Skip("Apple Vision is macOS only.");
            return true;
        }

        if (!AppleVisionOcr.IsAvailable())
        {
            Assert.Skip("Vision.framework did not load on this machine.");
            return true;
        }

        return false;
    }

    [Fact]
    public void IsAvailable_OffMacOs_IsFalse()
    {
        if (OperatingSystem.IsMacOS())
        {
            Assert.Skip("Only meaningful off macOS.");
            return;
        }

        Assert.False(AppleVisionOcr.IsAvailable());
        Assert.Empty(AppleVisionOcr.GetLanguages());
    }

    [Fact]
    public void GetLanguages_ReturnsTheFrameworksOwnList()
    {
        if (SkipIfUnavailable())
        {
            return;
        }

        var languages = AppleVisionOcr.GetLanguages();

        Assert.NotEmpty(languages);

        // en-US has been in every revision of the recognizer; asserting the exact set would
        // just break on the next macOS that adds a language.
        Assert.Contains(languages, l => l.Code == "en-US");
        Assert.All(languages, l => Assert.False(string.IsNullOrWhiteSpace(l.Name)));
    }

    [Fact]
    public void Ocr_NullBitmap_IsEmptyRatherThanThrowing()
    {
        Assert.Equal(string.Empty, AppleVisionOcr.Ocr(null, "en-US", fast: false, CancellationToken.None));
    }

    [Fact]
    public void Ocr_SingleLineOnTransparentBackground_ReadsTheText()
    {
        if (SkipIfUnavailable())
        {
            return;
        }

        using var bitmap = DrawSubtitle(["It's a beautiful day."]);

        var text = AppleVisionOcr.Ocr(bitmap, "en-US", fast: false, CancellationToken.None);

        Assert.Equal("It's a beautiful day.", text);
    }

    [Fact]
    public void Ocr_TwoLines_KeepsThemInReadingOrder()
    {
        if (SkipIfUnavailable())
        {
            return;
        }

        using var bitmap = DrawSubtitle(["- Are you coming with us?", "- No, I will stay here."]);

        var text = AppleVisionOcr.Ocr(bitmap, "en-US", fast: false, CancellationToken.None);
        var lines = text.Split(Environment.NewLine);

        Assert.Equal(2, lines.Length);
        Assert.StartsWith("- Are you coming", lines[0]);
        Assert.StartsWith("- No, I will stay", lines[1]);
    }

    [Fact]
    public void Ocr_TransparentAndBlackBackgrounds_ReadTheSame()
    {
        if (SkipIfUnavailable())
        {
            return;
        }

        // SE hands over images with a transparent background. Compositing them onto black first
        // would be easy to add and is what the PaddleOCR path does, so this pins the finding
        // that Vision does not need it - if a future macOS starts reading the two differently,
        // this is the test that says so.
        using var transparent = DrawSubtitle(["It's a beautiful day."]);
        using var onBlack = DrawSubtitle(["It's a beautiful day."], transparentBackground: false);

        Assert.Equal(
            AppleVisionOcr.Ocr(onBlack, "en-US", fast: false, CancellationToken.None),
            AppleVisionOcr.Ocr(transparent, "en-US", fast: false, CancellationToken.None));
    }

    [Fact]
    public void Ocr_EmptyImage_IsEmpty()
    {
        if (SkipIfUnavailable())
        {
            return;
        }

        using var bitmap = DrawSubtitle([]);

        Assert.Equal(string.Empty, AppleVisionOcr.Ocr(bitmap, "en-US", fast: false, CancellationToken.None));
    }

    /// <summary>
    /// Deliberately not gated on macOS: an already-cancelled token has to be honoured on every
    /// platform, and it was not - off macOS the availability gate returned empty before the token
    /// was ever looked at, which is how CI caught this.
    /// </summary>
    [Fact]
    public void Ocr_CancelledBeforeStart_Throws()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        using var bitmap = DrawSubtitle(["Anything at all"]);

        Assert.Throws<OperationCanceledException>(
            () => AppleVisionOcr.Ocr(bitmap, "en-US", fast: false, cts.Token));

        // A null bitmap takes the same early-return branch the unavailable engine does, so this
        // reproduces the CI failure on macOS too: before the fix it returned empty here.
        Assert.Throws<OperationCanceledException>(
            () => AppleVisionOcr.Ocr(null, "en-US", fast: false, cts.Token));
    }

    private static SKBitmap DrawSubtitle(string[] lines, bool transparentBackground = true)
    {
        const int width = 1200;
        const int lineHeight = 90;
        var height = lineHeight * Math.Max(lines.Length, 1) + 40;

        var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(transparentBackground ? SKColors.Transparent : SKColors.Black);

        using var typeface = SKTypeface.FromFamilyName("Helvetica Neue", SKFontStyle.Bold);
        using var font = new SKFont(typeface, 64);
        using var fill = new SKPaint { Color = SKColors.White, IsAntialias = true };
        using var outline = new SKPaint
        {
            Color = SKColors.Black,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 4,
        };

        for (var i = 0; i < lines.Length; i++)
        {
            var x = (width - font.MeasureText(lines[i])) / 2;
            var y = 70 + i * lineHeight;
            canvas.DrawText(lines[i], x, y, font, outline);
            canvas.DrawText(lines[i], x, y, font, fill);
        }

        return bitmap;
    }
}
