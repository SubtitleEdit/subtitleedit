using SkiaSharp;
using System;
using System.IO;
using System.IO.Compression;

namespace Nikse.SubtitleEdit.Logic.Ocr.GoogleLens;

internal class Helper
{
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