using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Features.Shared.BinaryEdit;
using Nikse.SubtitleEdit.Logic;
using SkiaSharp;
using System.Collections.Generic;

namespace UITests.Features.Shared.BinaryEdit;

public class FadeRemoverTests
{
    /// <summary>A 20x10 white box on transparent, at the given alpha.</summary>
    private static SKBitmap MakeBox(byte alpha, SKColor? color = null)
    {
        var bitmap = new SKBitmap(20, 10, SKColorType.Bgra8888, SKAlphaType.Unpremul);
        bitmap.Erase(SKColors.Transparent);
        var c = color ?? SKColors.White;
        for (var y = 2; y < 8; y++)
        {
            for (var x = 2; x < 18; x++)
            {
                bitmap.SetPixel(x, y, new SKColor(c.Red, c.Green, c.Blue, alpha));
            }
        }

        return bitmap;
    }

    private static BinarySubtitleItem MakeItem(int startMs, int endMs, byte alpha, SKColor? color = null, int x = 100, int y = 900)
    {
        using var sk = MakeBox(alpha, color);
        return new BinarySubtitleItem(TimeSpan.FromMilliseconds(startMs), TimeSpan.FromMilliseconds(endMs))
        {
            X = x,
            Y = y,
            Bitmap = sk.ToAvaloniaBitmap(),
        };
    }

    [AvaloniaFact]
    public void FadeInRunCollapsesToOneLineWithMostOpaqueImage()
    {
        var items = new List<BinarySubtitleItem>
        {
            MakeItem(0, 40, 40),
            MakeItem(40, 80, 120),
            MakeItem(80, 120, 255),
            MakeItem(120, 2000, 255),
        };

        var removed = FadeRemover.RemoveFades(items);

        Assert.Equal(3, removed);
        Assert.Single(items);
        Assert.Equal(TimeSpan.Zero, items[0].StartTime);
        Assert.Equal(TimeSpan.FromMilliseconds(2000), items[0].EndTime);
        Assert.Equal(TimeSpan.FromMilliseconds(2000), items[0].Duration);
        using var kept = items[0].Bitmap!.ToSkBitmap();
        Assert.Equal(255, kept.GetPixel(10, 5).Alpha);
    }

    [AvaloniaFact]
    public void DifferentPictureIsNotMerged()
    {
        var items = new List<BinarySubtitleItem>
        {
            MakeItem(0, 1000, 255, SKColors.White),
            MakeItem(1000, 2000, 255, SKColors.Red),
        };

        var removed = FadeRemover.RemoveFades(items);

        Assert.Equal(0, removed);
        Assert.Equal(2, items.Count);
    }

    [AvaloniaFact]
    public void GapLargerThanFadeThresholdIsNotMerged()
    {
        var items = new List<BinarySubtitleItem>
        {
            MakeItem(0, 1000, 255),
            MakeItem(1500, 2000, 255),
        };

        var removed = FadeRemover.RemoveFades(items);

        Assert.Equal(0, removed);
        Assert.Equal(2, items.Count);
    }

    [AvaloniaFact]
    public void MovedCaptionIsNotMerged()
    {
        var items = new List<BinarySubtitleItem>
        {
            MakeItem(0, 1000, 255, y: 900),
            MakeItem(1000, 2000, 255, y: 100),
        };

        var removed = FadeRemover.RemoveFades(items);

        Assert.Equal(0, removed);
    }

    [AvaloniaFact]
    public void TwoSeparateFadesAreEachCollapsed()
    {
        var items = new List<BinarySubtitleItem>
        {
            MakeItem(0, 40, 60),
            MakeItem(40, 1000, 255),
            MakeItem(3000, 3040, 60, SKColors.Red),
            MakeItem(3040, 4000, 255, SKColors.Red),
        };

        var removed = FadeRemover.RemoveFades(items);

        Assert.Equal(2, removed);
        Assert.Equal(2, items.Count);
        Assert.Equal(TimeSpan.FromMilliseconds(1000), items[0].EndTime);
        Assert.Equal(TimeSpan.FromMilliseconds(3000), items[1].StartTime);
        Assert.Equal(TimeSpan.FromMilliseconds(4000), items[1].EndTime);
    }
}
