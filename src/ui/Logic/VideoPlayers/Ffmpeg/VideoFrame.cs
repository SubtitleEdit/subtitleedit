using System;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers.Ffmpeg;

/// <summary>
/// One decoded picture, converted to BGRA and ready to be copied into a WriteableBitmap.
/// Buffers are pooled by <see cref="FfmpegPlayer"/>, so a frame is reused rather than freed.
/// </summary>
public sealed class VideoFrame : IDisposable
{
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
        Stride = width * 4;
        Data = width > 0 && height > 0 ? Marshal.AllocHGlobal(Stride * height) : IntPtr.Zero;
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
