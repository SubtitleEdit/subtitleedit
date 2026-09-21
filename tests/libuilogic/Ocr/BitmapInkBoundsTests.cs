using Nikse.SubtitleEdit.UiLogic.Ocr;
using SkiaSharp;

namespace LibUiLogicTests.Ocr;

// DVB subtitles decode to an image of the whole video frame; where the subtitle is has to be
// read off its ink.
public class BitmapInkBoundsTests
{
    private static SKBitmap FrameWithInk(int width, int height, SKRectI ink, SKColorType colorType = SKColorType.Rgba8888)
    {
        var bitmap = new SKBitmap(new SKImageInfo(width, height, colorType, SKAlphaType.Unpremul));
        bitmap.Erase(SKColors.Transparent);
        for (var y = ink.Top; y < ink.Bottom; y++)
        {
            for (var x = ink.Left; x < ink.Right; x++)
            {
                bitmap.SetPixel(x, y, SKColors.White);
            }
        }

        return bitmap;
    }

    [Theory]
    [InlineData(SKColorType.Rgba8888)]
    [InlineData(SKColorType.Bgra8888)]
    [InlineData(SKColorType.RgbaF16)] // not a 32-bit layout: the pixel-by-pixel path
    public void Get_ReturnsTheInkRectangle(SKColorType colorType)
    {
        using var bitmap = FrameWithInk(720, 576, new SKRectI(200, 480, 520, 530), colorType);

        Assert.Equal(new SKRectI(200, 480, 520, 530), BitmapInkBounds.Get(bitmap));
    }

    [Fact]
    public void Get_EmptyImage_ReturnsNull()
    {
        using var bitmap = FrameWithInk(720, 576, SKRectI.Empty);

        Assert.Null(BitmapInkBounds.Get(bitmap));
        Assert.Null(BitmapInkBounds.Crop(bitmap));
    }

    [Fact]
    public void Crop_ReturnsTightCopyAndItsPlaceInTheFrame()
    {
        var frame = FrameWithInk(720, 576, new SKRectI(200, 480, 520, 530));

        var cropped = BitmapInkBounds.Crop(frame);
        frame.Dispose(); // the crop must not share the frame's pixels

        Assert.NotNull(cropped);
        using var bitmap = cropped.Value.Bitmap;
        Assert.Equal(new SKPointI(200, 480), cropped.Value.Position);
        Assert.Equal(320, bitmap.Width);
        Assert.Equal(50, bitmap.Height);
        Assert.Equal(SKColors.White, bitmap.GetPixel(0, 0));
        Assert.Equal(SKColors.White, bitmap.GetPixel(319, 49));
    }
}
