using SkiaSharp;

namespace Nikse.SubtitleEdit.UiLogic.Export;

internal static class SkBitmapExportExtensions
{
    public static byte[] ToPngArray(this SKBitmap bitmap)
    {
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    public static byte[] ToPng8BitArray(this SKBitmap bitmap)
    {
        return Png8BitEncoder.Encode(bitmap);
    }
}
