using System;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers.Ffmpeg;

/// <summary>
/// One decoded picture, converted to BGRA and ready to be copied into a WriteableBitmap.
/// Buffers are pooled by <see cref="FfmpegPlayer"/>, so a frame is reused rather than freed.
/// </summary>
public sealed class VideoFrame : IDisposable
{
    private const int StrideAlignment = 64;
    private const int RowSlack = 64;

    public IntPtr Data { get; private set; }
    public int Stride { get; }
    public int Width { get; }
    public int Height { get; }

    /// <summary>Presentation time in seconds from the start of the file.</summary>
    public double Pts { get; set; }

    /// <summary>Seek serial the frame was decoded under.</summary>
    public int Serial { get; set; }

    /// <summary>A marker pushed after the last picture of the stream; carries no pixels.</summary>
    public bool IsEndOfStream { get; set; }

    public VideoFrame(int width, int height)
    {
        Width = width;
        Height = height;
        // swscale's SIMD converters write whole vectors, so for widths that are not a multiple of
        // the vector size the last row overruns its exact byte length (32 bytes seen for a 680 px
        // BGRA picture). Align the stride and keep slack after the last row so that stays inside
        // the allocation; a tight buffer here silently corrupts the heap.
        Stride = StrideFor(width);
        Data = width > 0 && height > 0 ? Marshal.AllocHGlobal(Stride * height + RowSlack) : IntPtr.Zero;
    }

    /// <summary>Bytes per row of a BGRA picture <paramref name="width"/> pixels wide, as allocated here.</summary>
    public static int StrideFor(int width)
    {
        return (width * 4 + StrideAlignment - 1) & ~(StrideAlignment - 1);
    }

    public void Dispose()
    {
        if (Data != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(Data);
            Data = IntPtr.Zero;
        }
    }
}
