using Nikse.SubtitleEdit.UiLogic.Export;
using SkiaSharp;

namespace LibUiLogicTests.Export;

/// <summary>
/// ASSA "{\frz..}" reaches the renderer as <see cref="ImageParameter.TagRotation"/> and turns
/// the finished subtitle counter-clockwise.
/// </summary>
public class ImageRendererRotationTagTests
{
    private static ImageParameter MakeParameter(float rotation = 0, int padding = 0)
    {
        return new ImageParameter
        {
            Text = "Hello world, a long line",
            FontName = "Arial",
            FontSize = 40,
            FontColor = SKColors.White,
            OutlineColor = SKColors.Black,
            OutlineWidth = 2,
            ScreenWidth = 1280,
            ScreenHeight = 720,
            ContentAlignment = ExportContentAlignment.Center,
            TagRotation = rotation,
            PaddingLeftRight = padding,
            PaddingTopBottom = padding,
        };
    }

    // Average x of the drawn pixels in the top and the bottom quarter of the bitmap.
    private static (double Top, double Bottom) DrawnCenters(SKBitmap bitmap)
    {
        double topSum = 0, bottomSum = 0;
        var topCount = 0;
        var bottomCount = 0;
        for (var y = 0; y < bitmap.Height; y++)
        {
            for (var x = 0; x < bitmap.Width; x++)
            {
                if (bitmap.GetPixel(x, y).Alpha < 128)
                {
                    continue;
                }

                if (y < bitmap.Height / 4)
                {
                    topSum += x;
                    topCount++;
                }
                else if (y >= bitmap.Height * 3 / 4)
                {
                    bottomSum += x;
                    bottomCount++;
                }
            }
        }

        return (topSum / Math.Max(1, topCount), bottomSum / Math.Max(1, bottomCount));
    }

    [Fact]
    public void Rotation90_SwapsWidthAndHeight()
    {
        using var plain = ImageRenderer.GenerateBitmap(MakeParameter());
        using var turned = ImageRenderer.GenerateBitmap(MakeParameter(rotation: 90));

        Assert.Equal(plain.Height, turned.Width);
        Assert.Equal(plain.Width, turned.Height);
    }

    [Fact]
    public void PositiveRotation_IsCounterClockwise()
    {
        // Turned counter-clockwise, the end of the line rises: the top of the image holds its
        // right side, the bottom its left side.
        using var turned = ImageRenderer.GenerateBitmap(MakeParameter(rotation: 20));

        var (top, bottom) = DrawnCenters(turned);
        Assert.True(top > bottom, $"top {top} bottom {bottom}");
    }

    [Fact]
    public void FullTurn_IsNoRotation()
    {
        using var plain = ImageRenderer.GenerateBitmap(MakeParameter());
        using var turned = ImageRenderer.GenerateBitmap(MakeParameter(rotation: 360));

        Assert.Equal(plain.Width, turned.Width);
        Assert.Equal(plain.Height, turned.Height);
    }

    [Fact]
    public void Rotation_TurnsThePaddingWithTheText()
    {
        using var plain = ImageRenderer.GenerateBitmap(MakeParameter(padding: 7));
        using var turned = ImageRenderer.GenerateBitmap(MakeParameter(rotation: 180, padding: 7));

        Assert.Equal(plain.Width, turned.Width);
        Assert.Equal(plain.Height, turned.Height);
    }
}
