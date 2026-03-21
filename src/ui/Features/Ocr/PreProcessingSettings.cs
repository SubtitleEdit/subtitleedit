using SkiaSharp;

namespace Nikse.SubtitleEdit.Features.Ocr;

public class PreProcessingSettings
{
    public bool CropTransparentColors { get; set; }
    
    public bool InverseColors { get; set; }
    
    public bool Binarize { get; set; }
    
    public bool RemoveBorders { get; set; }
    public int BorderSize { get; set; } = 2;

    public bool ToOneColor { get; set; }
    public int OneColorDarknessThreshold { get; set; } = 128; // 0-255, pixels darker than this become transparent

    public SKBitmap PreProcess(SKBitmap bitmap)
    {
        var bmp = bitmap;

        if (CropTransparentColors)
        {
            bmp = CropTransparent(bmp);
        }

        if (RemoveBorders)
        {
            bmp = RemoveBorder(bmp, BorderSize);

            if (CropTransparentColors)
            {
                bmp = CropTransparent(bmp);
            }
        }

        if (InverseColors)
        {
            bmp = InvertColors(bmp);
        }

        if (ToOneColor)
        {
            bmp = ConvertToOneColor(bmp, OneColorDarknessThreshold);
        }

        if (Binarize)
        {
            bmp = BinarizeOtsu(bmp);
        }

        return bmp;
    }

    public static unsafe SKBitmap CropTransparent(SKBitmap bitmap)
    {
        var left = bitmap.Width;
        var top = bitmap.Height;
        var right = 0;
        var bottom = 0;

        var pixels = (uint*)bitmap.GetPixels().ToPointer();
        var width = bitmap.Width;
        var height = bitmap.Height;

        // Find the bounding box of non-transparent pixels
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var pixel = pixels[y * width + x];
                var alpha = (pixel >> 24) & 0xFF;

                if (alpha > 0) // Non-transparent pixel
                {
                    if (x < left) left = x;
                    if (x > right) right = x;
                    if (y < top) top = y;
                    if (y > bottom) bottom = y;
                }
            }
        }

        // If no non-transparent pixels found, return original
        if (left > right || top > bottom)
        {
            return bitmap;
        }

        // Calculate cropped dimensions
        int cropWidth = right - left + 1;
        int cropHeight = bottom - top + 1;

        // Create new cropped bitmap
        var cropped = new SKBitmap(cropWidth, cropHeight);
        using (var canvas = new SKCanvas(cropped))
        {
            var srcRect = new SKRect(left, top, right + 1, bottom + 1);
            var destinationRect = new SKRect(0, 0, cropWidth, cropHeight);
            canvas.DrawBitmap(bitmap, srcRect, destinationRect);
        }

        return cropped;
    }

    private static unsafe SKBitmap InvertColors(SKBitmap bitmap)
    {
        var inverted = new SKBitmap(bitmap.Width, bitmap.Height);

        var srcPixels = (uint*)bitmap.GetPixels().ToPointer();
        var dstPixels = (uint*)inverted.GetPixels().ToPointer();
        var totalPixels = bitmap.Width * bitmap.Height;

        for (var i = 0; i < totalPixels; i++)
        {
            var pixel = srcPixels[i];

            // Extract ARGB components
            var a = (pixel >> 24) & 0xFF;
            var r = (pixel >> 16) & 0xFF;
            var g = (pixel >> 8) & 0xFF;
            var b = pixel & 0xFF;

            // Invert RGB, preserve alpha
            r = 255 - r;
            g = 255 - g;
            b = 255 - b;

            // Pack back into uint
            dstPixels[i] = (a << 24) | (r << 16) | (g << 8) | b;
        }

        return inverted;
    }

    private static unsafe SKBitmap ConvertToOneColor(SKBitmap bitmap, int threshold)
    {
        var result = new SKBitmap(bitmap.Width, bitmap.Height);

        var srcPixels = (uint*)bitmap.GetPixels().ToPointer();
        var dstPixels = (uint*)result.GetPixels().ToPointer();
        var totalPixels = bitmap.Width * bitmap.Height;

        for (int i = 0; i < totalPixels; i++)
        {
            var pixel = srcPixels[i];

            // Extract RGB components
            var r = (pixel >> 16) & 0xFF;
            var g = (pixel >> 8) & 0xFF;
            var b = pixel & 0xFF;

            // Calculate brightness (same formula as grayscale conversion)
            var brightness = (byte)(0.299 * r + 0.587 * g + 0.114 * b);

            // If brightness is above threshold, make it white; otherwise transparent
            if (brightness > threshold)
            {
                dstPixels[i] = 0xFFFFFFFF; // White with full alpha
            }
            else
            {
                dstPixels[i] = 0x00000000; // Transparent
            }
        }

        return result;
    }

    private static SKBitmap RemoveBorder(SKBitmap bitmap, int borderSize)
    {
        if (borderSize <= 0 || borderSize * 2 >= bitmap.Width || borderSize * 2 >= bitmap.Height)
        {
            return bitmap;
        }

        var width = bitmap.Width - (borderSize * 2);
        var height = bitmap.Height - (borderSize * 2);

        var cropped = new SKBitmap(width, height);
        using (var canvas = new SKCanvas(cropped))
        {
            var srcRect = new SKRect(borderSize, borderSize, bitmap.Width - borderSize, bitmap.Height - borderSize);
            var destinationRect = new SKRect(0, 0, width, height);
            canvas.DrawBitmap(bitmap, srcRect, destinationRect);
        }

        return cropped;
    }

    private static unsafe SKBitmap BinarizeOtsu(SKBitmap bitmap)
    {
        var width = bitmap.Width;
        var height = bitmap.Height;
        var totalPixels = width * height;

        // Convert to grayscale and build histogram in one pass
        var grayscale = new SKBitmap(width, height);
        var srcPixels = (uint*)bitmap.GetPixels().ToPointer();
        var grayPixels = (uint*)grayscale.GetPixels().ToPointer();
        var histogram = new int[256];

        for (var i = 0; i < totalPixels; i++)
        {
            var pixel = srcPixels[i];

            var a = (pixel >> 24) & 0xFF;
            var r = (pixel >> 16) & 0xFF;
            var g = (pixel >> 8) & 0xFF;
            var b = pixel & 0xFF;

            // Convert to grayscale using standard luminance formula
            var gray = (byte)(0.299 * r + 0.587 * g + 0.114 * b);

            grayPixels[i] = a << 24 | (uint)(gray << 16) | (uint)(gray << 8) | gray;
            histogram[gray]++;
        }

        // Calculate Otsu's threshold
        float sum = 0;
        for (var i = 0; i < 256; i++)
        {
            sum += i * histogram[i];
        }

        float sumB = 0;
        int wB = 0;
        int wF;
        float maxVariance = 0;
        int threshold = 0;

        for (var i = 0; i < 256; i++)
        {
            wB += histogram[i];
            if (wB == 0) continue;

            wF = totalPixels - wB;
            if (wF == 0) break;

            sumB += i * histogram[i];
            float mB = sumB / wB;
            float mF = (sum - sumB) / wF;

            float variance = wB * wF * (mB - mF) * (mB - mF);

            if (variance > maxVariance)
            {
                maxVariance = variance;
                threshold = i;
            }
        }

        // Apply threshold
        var binarized = new SKBitmap(width, height);
        var binPixels = (uint*)binarized.GetPixels().ToPointer();

        for (var i = 0; i < totalPixels; i++)
        {
            var pixel = grayPixels[i];
            var gray = (pixel >> 16) & 0xFF;
            var a = (pixel >> 24) & 0xFF;

            var value = gray >= threshold ? 255u : 0u;
            binPixels[i] = (a << 24) | (value << 16) | (value << 8) | value;
        }

        return binarized;
    }
}