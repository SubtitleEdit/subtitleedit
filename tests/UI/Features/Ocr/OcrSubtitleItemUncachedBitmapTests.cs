using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Features.Ocr;
using Nikse.SubtitleEdit.Features.Ocr.OcrSubtitle;
using SkiaSharp;

namespace UITests.Features.Ocr;

/// <summary>
/// Point sync shows a thumbnail per image row; the cached GetBitmapCropped kept every row's full
/// frame alive (gigabytes for a long DVB track), so the thumbnails decode fresh and free it.
/// </summary>
public class OcrSubtitleItemUncachedBitmapTests
{
    private sealed class FakeOcrSubtitle : IOcrSubtitle
    {
        public List<SKBitmap> Decoded { get; } = new();

        public int Count => 1;

        public SKBitmap GetBitmap(int index)
        {
            // A 720x576 frame with a 100x40 opaque block in it, decoded fresh on every call.
            var bitmap = new SKBitmap(720, 576);
            bitmap.Erase(SKColors.Transparent);
            using (var canvas = new SKCanvas(bitmap))
            using (var paint = new SKPaint { Color = SKColors.White })
            {
                canvas.DrawRect(new SKRect(300, 500, 400, 540), paint);
            }

            Decoded.Add(bitmap);
            return bitmap;
        }

        public TimeSpan GetStartTime(int index) => TimeSpan.Zero;
        public TimeSpan GetEndTime(int index) => TimeSpan.FromSeconds(1);
        public List<OcrSubtitleItem> MakeOcrSubtitleItems() => new() { new OcrSubtitleItem(this, 0) };
        public bool GetIsForced(int index) => false;
        public SKPointI GetPosition(int index) => new(0, 0);
        public SKSizeI GetScreenSize(int index) => new(720, 576);
    }

    [AvaloniaFact]
    public void GetBitmapCroppedUncached_CropsAndFreesTheDecodedFrame()
    {
        var source = new FakeOcrSubtitle();
        var item = new OcrSubtitleItem(source, 0);

        using var first = item.GetBitmapCroppedUncached();
        using var second = item.GetBitmapCroppedUncached();

        Assert.Equal(100, first.PixelSize.Width);
        Assert.Equal(40, first.PixelSize.Height);
        Assert.Equal(2, source.Decoded.Count); // nothing cached
        Assert.All(source.Decoded, b => Assert.Equal(IntPtr.Zero, b.Handle)); // every frame freed
    }
}
