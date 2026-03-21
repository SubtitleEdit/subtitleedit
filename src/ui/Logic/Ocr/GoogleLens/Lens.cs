using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Ocr.GoogleLens;

public interface ILens
{
    Task<LensResult> ScanByBitmap(SKBitmap bitmap, string twoLetterLanguageCode);
}

public class Lens : LensCore, ILens
{
    public Lens(Dictionary<string, object>? config = null, Func<HttpRequestMessage, Task<HttpResponseMessage>>? fetch = null)
        : base(config, fetch)
    {
        if (config != null && config.GetType() != typeof(Dictionary<string, object>))
        {
            Console.WriteLine($"Lens constructor expects a dictionary, got {config.GetType()}");
            config = new Dictionary<string, object>();
        }
    }

    public async Task<LensResult> ScanByBitmap(SKBitmap bitmap, string twoLetterLanguageCode)
    {
        if (bitmap == null)
        {
            throw new ArgumentNullException(nameof(bitmap));
        }

        var originalWidth = bitmap.Width;
        var originalHeight = bitmap.Height;

        if (originalWidth == 0 || originalHeight == 0)
        {
            throw new Exception("Could not determine original image dimensions.");
        }

        var bitmapToProcess = bitmap;
        var finalMime = "image/png";

        const int MAX_DIMENSION = 1200;

        // Only process if absolutely necessary
        if (originalWidth > MAX_DIMENSION || originalHeight > MAX_DIMENSION)
        {
            // Calculate new dimensions maintaining aspect ratio
            float ratio = Math.Min((float)MAX_DIMENSION / originalWidth, (float)MAX_DIMENSION / originalHeight);
            int newWidth = (int)(originalWidth * ratio);
            int newHeight = (int)(originalHeight * ratio);

            using var resized = bitmap.Resize(new SKImageInfo(newWidth, newHeight), new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));
            if (resized == null)
            {
                throw new Exception("Could not resize image");
            }

            var imageToProcessBuffer = resized.ToPngArray();
            return await ScanByData(imageToProcessBuffer, finalMime, new[] { originalWidth, originalHeight }, twoLetterLanguageCode);
        }

        var buffer = bitmapToProcess.ToPngArray();
        return await ScanByData(buffer, finalMime, [originalWidth, originalHeight], twoLetterLanguageCode);
    }
}
