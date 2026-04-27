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
}
