using SkiaSharp;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Ocr.GoogleLens;

internal class Helper
{
    public static async Task<byte[]> ResizeImageAsync(byte[] buffer, int maxWidth, int maxHeight)
    {
        using var inputStream = new MemoryStream(buffer);
        using var original = SKBitmap.Decode(inputStream);
        
        if (original == null)
        {
            throw new InvalidOperationException("Could not decode image");
        }

        // Calculate new dimensions maintaining aspect ratio
        float ratioX = (float)maxWidth / original.Width;
        float ratioY = (float)maxHeight / original.Height;
        float ratio = Math.Min(ratioX, ratioY);
        
        int newWidth = (int)(original.Width * ratio);
        int newHeight = (int)(original.Height * ratio);

        using var resized = original.Resize(new SKImageInfo(newWidth, newHeight), new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));
        if (resized == null)
        {
            throw new InvalidOperationException("Could not resize image");
        }
            
        using var image = SKImage.FromBitmap(resized);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 90);
        
        return await Task.FromResult(data.ToArray());
    }
    
    public static (int Width, int Height) ImageDimensionsFromData(byte[] data)
    {
        using var inputStream = new MemoryStream(data);
        using var codec = SKCodec.Create(inputStream);
        
        if (codec == null)
        {
            throw new InvalidOperationException("Could not decode image");
        }
            
        return (codec.Info.Width, codec.Info.Height);
    }
    
    public static byte[] DecompressDeflate(byte[] deflateData)
    {
        using var inputStream = new MemoryStream(deflateData);
        using var deflateStream = new DeflateStream(inputStream, CompressionMode.Decompress);
        using var outputStream = new MemoryStream();
        deflateStream.CopyTo(outputStream);
        return outputStream.ToArray();
    }
    
    public static byte[] DecompressGzip(byte[] gzipData)
    {
        using var inputStream = new MemoryStream(gzipData);
        using var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress);
        using var outputStream = new MemoryStream();
        gzipStream.CopyTo(outputStream);
        return outputStream.ToArray();
    }
    
    public static byte[] DecompressBrotli(byte[] brotliData)
    {
        using var inputStream = new MemoryStream(brotliData);
        using var brotliStream = new BrotliStream(inputStream, CompressionMode.Decompress);
        using var outputStream = new MemoryStream();
        brotliStream.CopyTo(outputStream);
        return outputStream.ToArray();
    }
}