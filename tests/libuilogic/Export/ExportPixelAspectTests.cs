using Nikse.SubtitleEdit.UiLogic.Export;
using SkiaSharp;

namespace LibUiLogicTests.Export;

/// <summary>
/// SD exports with non-square pixels (doom9 p=2052176): the rendered subtitle is pre-scaled
/// horizontally by 1/SAR, so it has normal proportions once the player stretches the pixels.
/// </summary>
public class ExportPixelAspectTests
{
    private static ImageParameter MakeParameter(double pixelAspectRatio)
    {
        return new ImageParameter
        {
            Text = "Hello world, a long line",
            FontName = "Arial",
            FontSize = 40,
            FontColor = SKColors.White,
            OutlineColor = SKColors.Black,
            OutlineWidth = 2,
            ScreenWidth = 720,
            ScreenHeight = 576,
            ContentAlignment = ExportContentAlignment.Center,
            PixelAspectRatio = pixelAspectRatio,
        };
    }

    [Theory]
    [InlineData(ExportPixelAspect.Pal16x9, 64.0 / 45.0)]
    [InlineData(ExportPixelAspect.Pal4x3, 16.0 / 15.0)]
    [InlineData(ExportPixelAspect.Ntsc16x9, 32.0 / 27.0)]
    [InlineData(ExportPixelAspect.Ntsc4x3, 8.0 / 9.0)]
    [InlineData("64/45", 64.0 / 45.0)]
    [InlineData("1.5", 1.5)]
    [InlineData("", 1.0)]
    [InlineData(null, 1.0)]
    [InlineData("abc", 1.0)]
    [InlineData("1:0", 1.0)]
    [InlineData("100:1", 1.0)]
    public void Parse_ReadsRatio(string? value, double expected)
    {
        Assert.Equal(expected, ExportPixelAspect.Parse(value), 6);
    }

    [Theory]
    [InlineData(ExportPixelAspect.Pal16x9)]
    [InlineData(ExportPixelAspect.Pal4x3)]
    [InlineData(ExportPixelAspect.Ntsc16x9)]
    [InlineData(ExportPixelAspect.Ntsc4x3)]
    public void GenerateBitmap_ScalesWidthByInverseSar_KeepsHeight(string pixelAspect)
    {
        var sar = ExportPixelAspect.Parse(pixelAspect);
        using var square = ImageRenderer.GenerateBitmap(MakeParameter(1));
        using var scaled = ImageRenderer.GenerateBitmap(MakeParameter(sar));

        Assert.Equal(square.Height, scaled.Height);
        Assert.Equal(Math.Round(square.Width / sar, MidpointRounding.AwayFromZero), scaled.Width);
    }

    [Fact]
    public void GenerateBitmap_SquarePixels_Unchanged()
    {
        using var a = ImageRenderer.GenerateBitmap(MakeParameter(1));
        using var b = ImageRenderer.GenerateBitmap(new ImageParameter
        {
            Text = "Hello world, a long line",
            FontName = "Arial",
            FontSize = 40,
            FontColor = SKColors.White,
            OutlineColor = SKColors.Black,
            OutlineWidth = 2,
            ScreenWidth = 720,
            ScreenHeight = 576,
            ContentAlignment = ExportContentAlignment.Center,
        });

        Assert.Equal(a.Width, b.Width);
        Assert.Equal(a.Height, b.Height);
    }

    [Fact]
    public void GenerateBitmap_Pal16x9_DrawnInkSpansNarrower()
    {
        using var square = ImageRenderer.GenerateBitmap(MakeParameter(1));
        using var scaled = ImageRenderer.GenerateBitmap(MakeParameter(64.0 / 45.0));

        var squareInk = InkSpan(square);
        var scaledInk = InkSpan(scaled);
        Assert.InRange(scaledInk / (double)squareInk, 45.0 / 64.0 - 0.02, 45.0 / 64.0 + 0.02);
    }

    private static int InkSpan(SKBitmap bitmap)
    {
        int min = int.MaxValue, max = -1;
        for (var y = 0; y < bitmap.Height; y++)
        {
            for (var x = 0; x < bitmap.Width; x++)
            {
                if (bitmap.GetPixel(x, y).Alpha >= 128)
                {
                    min = Math.Min(min, x);
                    max = Math.Max(max, x);
                }
            }
        }

        return max - min + 1;
    }
}
