using System.Collections.Generic;
using System.Linq;
using Nikse.SubtitleEdit.Features.Ocr.Engines;
using Nikse.SubtitleEdit.Features.Video.VideoOcr;
using SkiaSharp;

namespace UITests.Features.Video.VideoOcr;

public class VideoOcrObservationFilterTests
{
    /// <summary>200x100 frame: bright (white) band in the bottom quarter, dim (gray 150) band at the top.</summary>
    private static SKBitmap MakeFrame()
    {
        var bitmap = new SKBitmap(200, 100);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(new SKColor(40, 40, 40));
        using var dim = new SKPaint { Color = new SKColor(150, 150, 150) };
        canvas.DrawRect(0, 10, 200, 20, dim); // scene text band (below the 190 threshold)
        using var bright = new SKPaint { Color = SKColors.White };
        canvas.DrawRect(40, 75, 120, 20, bright); // subtitle band
        return bitmap;
    }

    // Vision coordinates: normalized, origin bottom-left, Y grows upward.
    private static AppleVisionObservation Box(string text, double left, double right, double topY, double bottomY)
    {
        return new AppleVisionObservation(text, left, right, topY, bottomY);
    }

    [Fact]
    public void FilterByBrightness_DropsDimSceneText_KeepsBrightSubtitle()
    {
        using var bitmap = MakeFrame();
        var subtitle = Box("Hello", 0.2, 0.8, 0.25, 0.05);   // pixel rows 75-95
        var sceneText = Box("NEW YOR", 0.0, 1.0, 0.9, 0.7);  // pixel rows 10-30

        var kept = VideoOcrObservationFilter.FilterByBrightness(
            new List<AppleVisionObservation> { sceneText, subtitle }, bitmap, 190);

        Assert.Single(kept);
        Assert.Equal("Hello", kept[0].Text);
    }

    [Fact]
    public void FilterByBrightness_AllDim_AllDropped()
    {
        // A non-blank frame whose only text is below the threshold: the bright thing on
        // screen is not text, so dropping every observation is the correct answer.
        using var bitmap = MakeFrame();
        var sceneText = Box("NEW YOR", 0.0, 1.0, 0.9, 0.7);

        var kept = VideoOcrObservationFilter.FilterByBrightness(
            new List<AppleVisionObservation> { sceneText }, bitmap, 190);

        Assert.Empty(kept);
    }

    [Fact]
    public void FilterByBrightness_ThresholdOff_KeepsEverything()
    {
        using var bitmap = MakeFrame();
        var sceneText = Box("NEW YOR", 0.0, 1.0, 0.9, 0.7);
        var subtitle = Box("Hello", 0.2, 0.8, 0.25, 0.05);

        var kept = VideoOcrObservationFilter.FilterByBrightness(
            new List<AppleVisionObservation> { sceneText, subtitle }, bitmap, 0);

        Assert.Equal(2, kept.Count);
    }

    [Fact]
    public void GetBrightFraction_BoxOutsideBrightArea_IsZero()
    {
        using var bitmap = MakeFrame();
        var fraction = VideoOcrObservationFilter.GetBrightFraction(
            Box("x", 0.0, 1.0, 0.9, 0.7), bitmap.Pixels, bitmap.Width, bitmap.Height, 190);

        Assert.Equal(0, fraction, 3);
    }
}
