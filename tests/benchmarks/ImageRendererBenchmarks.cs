using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.UiLogic.Export;
using SkiaSharp;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>
/// Image-based export (Blu-ray SUP, VobSub, BDN-XML, "export as images") calls
/// <see cref="ImageRenderer.GenerateBitmap"/> once per subtitle line, so a feature-length file runs
/// it 1500-2500 times. Each call rasterises into an oversized 4000x2000 scratch canvas and then has
/// to find the drawn pixels in it.
/// </summary>
[MemoryDiagnoser]
public class ImageRendererBenchmarks
{
    private ImageParameter _parameter = new();
    private SKBitmap _scratch = new(1, 1);

    /// <summary>Extra transparent margin around the exported bitmap (an export setting).</summary>
    [Params(0, 10)]
    public int Padding { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _parameter = new ImageParameter
        {
            Text = "It was the best of times," + Environment.NewLine + "it was the worst of times.",
            FontName = "Arial",
            FontSize = 60,
            FontColor = SKColors.White,
            OutlineColor = SKColors.Black,
            OutlineWidth = 3,
            ShadowColor = SKColors.Black,
            ShadowWidth = 2,
            ScreenWidth = 1920,
            ScreenHeight = 1080,
            BottomTopMargin = 20,
            LeftRightMargin = 20,
            LineSpacingPercent = 100,
            PaddingLeftRight = Padding,
            PaddingTopBottom = Padding,
            Alignment = ExportAlignment.BottomCenter,
        };

        // The scratch canvas GenerateBitmap allocates internally, with a realistic amount of drawn
        // content in it, so the trim scan can be measured on its own.
        _scratch = new SKBitmap(4000, 2000);
        using (var canvas = new SKCanvas(_scratch))
        {
            canvas.Clear(SKColors.Transparent);
            using var paint = new SKPaint { Color = SKColors.White, IsAntialias = true };
            using var font = new SKFont(SKTypeface.Default, 60);
            canvas.DrawText("It was the best of times,", 40, 120, SKTextAlign.Left, font, paint);
            canvas.DrawText("it was the worst of times.", 40, 200, SKTextAlign.Left, font, paint);
        }
    }

    [GlobalCleanup]
    public void Cleanup() => _scratch.Dispose();

    [Benchmark]
    public int GenerateBitmap()
    {
        using var bitmap = ImageRenderer.GenerateBitmap(_parameter);
        return bitmap.Height;
    }

    [Benchmark]
    public int TrimTransparentPixels()
    {
        var result = _scratch.TrimTransparentPixels();
        using (result.TrimmedBitmap)
        {
            return result.Top;
        }
    }
}
