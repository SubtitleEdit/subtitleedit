using Nikse.SubtitleEdit.Core.Common;
using SeConv.Core;
using SkiaSharp;
using Xunit;

namespace SeConvTests.Core;

/// <summary>
/// A DVB subtitle decodes to an image of the whole video frame. seconv passed that on with the
/// text's position and no frame size, so an image export put a frame-sized image at an offset
/// inside the default 1920x1080 frame. The loader now crops to the ink and reports the frame.
/// </summary>
public class DvbSubFrameTest
{
    [Fact]
    public void DvbFrameToItem_CropsToInkAndKeepsFrame()
    {
        var frame = new SKBitmap(new SKImageInfo(720, 576, SKColorType.Rgba8888, SKAlphaType.Unpremul));
        frame.Erase(SKColors.Transparent);
        using (var canvas = new SKCanvas(frame))
        using (var paint = new SKPaint { Color = SKColors.White })
        {
            canvas.DrawRect(200, 480, 320, 50, paint);
        }

        using var item = BitmapSubtitleLoader.DvbFrameToItem(frame, new TimeCode(1000), new TimeCode(3000));

        Assert.NotNull(item);
        Assert.Equal(320, item.Bitmap.Width);
        Assert.Equal(50, item.Bitmap.Height);
        Assert.Equal(new SKPointI(200, 480), item.Position);
        Assert.Equal(720, item.ScreenWidth);
        Assert.Equal(576, item.ScreenHeight);
        Assert.Equal(1000, item.StartTime.TotalMilliseconds);
        Assert.Equal(IntPtr.Zero, frame.Handle); // the frame-sized source is released
    }

    [Fact]
    public void DvbFrameToItem_EmptyFrame_IsSkipped()
    {
        var frame = new SKBitmap(new SKImageInfo(720, 576, SKColorType.Rgba8888, SKAlphaType.Unpremul));
        frame.Erase(SKColors.Transparent);

        Assert.Null(BitmapSubtitleLoader.DvbFrameToItem(frame, new TimeCode(1000), new TimeCode(3000)));
        Assert.Null(BitmapSubtitleLoader.DvbFrameToItem(null, new TimeCode(1000), new TimeCode(3000)));
    }
}
