using SkiaSharp;

namespace Nikse.SubtitleEdit.UiLogic.Ocr;

/// <summary>
/// Where the visible pixels of a subtitle image are. DVB subtitles decode to an image the size
/// of the whole video frame with the text drawn in place, so the frame-sized bitmap says
/// nothing about where the subtitle is - its ink does.
/// </summary>
public static class BitmapInkBounds
{
    /// <summary>The smallest rectangle holding every non-transparent pixel, or null if there is none.</summary>
    public static SKRectI? Get(SKBitmap bitmap)
    {
        var width = bitmap.Width;
        var height = bitmap.Height;
        var left = int.MaxValue;
        var top = -1;
        var right = -1;
        var bottom = -1;

        // Alpha is the fourth byte in both 32-bit layouts; anything else goes pixel by pixel.
        var fast = bitmap.ColorType is SKColorType.Rgba8888 or SKColorType.Bgra8888 && bitmap.RowBytes >= width * 4;
        var pixels = fast ? bitmap.GetPixelSpan() : default;
        for (var y = 0; y < height; y++)
        {
            var rowLeft = -1;
            var rowRight = -1;
            if (fast)
            {
                var row = pixels.Slice(y * bitmap.RowBytes, width * 4);
                for (var x = 0; x < width; x++)
                {
                    if (row[x * 4 + 3] != 0)
                    {
                        rowLeft = x;
                        break;
                    }
                }

                for (var x = width - 1; x >= 0 && rowLeft >= 0; x--)
                {
                    if (row[x * 4 + 3] != 0)
                    {
                        rowRight = x;
                        break;
                    }
                }
            }
            else
            {
                for (var x = 0; x < width; x++)
                {
                    if (bitmap.GetPixel(x, y).Alpha != 0)
                    {
                        rowLeft = rowLeft < 0 ? x : rowLeft;
                        rowRight = x;
                    }
                }
            }

            if (rowLeft < 0)
            {
                continue;
            }

            top = top < 0 ? y : top;
            bottom = y;
            left = Math.Min(left, rowLeft);
            right = Math.Max(right, rowRight);
        }

        return top < 0 ? null : new SKRectI(left, top, right + 1, bottom + 1);
    }

    /// <summary>
    /// Crops a frame-sized image to its ink. Returns the cropped copy and where it sits in the
    /// original, or null when the image is empty.
    /// </summary>
    public static (SKBitmap Bitmap, SKPointI Position)? Crop(SKBitmap bitmap)
    {
        var bounds = Get(bitmap);
        if (bounds is not { } b)
        {
            return null;
        }

        var cropped = new SKBitmap(new SKImageInfo(b.Width, b.Height, bitmap.ColorType, bitmap.AlphaType));
        if (!bitmap.ExtractSubset(cropped, b))
        {
            cropped.Dispose();
            return null;
        }

        // ExtractSubset shares the source's pixels; copy so the source can be disposed.
        var copy = cropped.Copy();
        cropped.Dispose();
        return copy is null ? null : (copy, new SKPointI(b.Left, b.Top));
    }
}
