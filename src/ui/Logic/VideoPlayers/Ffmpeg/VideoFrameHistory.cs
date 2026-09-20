using System;
using System.Collections.Generic;
using System.Threading;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers.Ffmpeg;

/// <summary>
/// The last few pictures before the one on screen, kept converted so "one frame back" is a
/// buffer swap instead of a seek. Without it every step back decodes from the previous key frame
/// up to the target again - hundreds of pictures on a long-GOP file, for each key press.
/// <para>
/// Frames are in presentation order and gapless: the video thread adds the pictures it decodes on
/// the way to a seek target, the presenter adds each picture as the next one replaces it. Anything
/// that breaks the run (a seek, dropped pictures) starts the history over. All frames belong to
/// one seek serial; a frame from another serial is refused, so a late add after a seek cannot mix
/// two positions.
/// </para>
/// </summary>
public sealed class VideoFrameHistory
{
    /// <summary>Upper bound for the pictures held, across all sizes.</summary>
    public const int MaxFrames = 16;

    private const long MemoryBudgetBytes = 64L * 1024 * 1024;

    private readonly List<VideoFrame> _frames = new(); // ascending Pts
    private readonly VideoFrameQueue _pool;
    private readonly Lock _lock = new();
    private int _serial = -1;

    public VideoFrameHistory(VideoFrameQueue pool)
    {
        _pool = pool;
    }

    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _frames.Count;
            }
        }
    }

    /// <summary>How many pictures of this size fit the memory budget (1920x1080 BGRA is 8 MB).</summary>
    public static int CapacityFor(int stride, int height)
    {
        var bytes = (long)stride * height;
        if (bytes <= 0)
        {
            return MaxFrames;
        }

        return (int)Math.Clamp(MemoryBudgetBytes / bytes, 2, MaxFrames);
    }

    /// <summary>Drops everything and starts collecting for <paramref name="serial"/>.</summary>
    public void Reset(int serial)
    {
        lock (_lock)
        {
            _serial = serial;
            ReturnAll();
        }
    }

    /// <summary>Drops everything but keeps the serial - the run of pictures was interrupted.</summary>
    public void Clear()
    {
        lock (_lock)
        {
            ReturnAll();
        }
    }

    /// <summary>
    /// Takes ownership of <paramref name="frame"/> as the newest picture. A frame from another
    /// serial goes straight back to the pool; one that is not later than the newest held means
    /// the run restarted, so the older pictures are dropped first.
    /// </summary>
    public void Add(VideoFrame frame)
    {
        lock (_lock)
        {
            if (frame.Serial != _serial || frame.IsEndOfStream)
            {
                _pool.Return(frame);
                return;
            }

            if (_frames.Count > 0 && frame.Pts <= _frames[^1].Pts)
            {
                ReturnAll();
            }

            _frames.Add(frame);
            var capacity = CapacityFor(frame.Stride, frame.Height);
            while (_frames.Count > capacity)
            {
                _pool.Return(_frames[0]);
                _frames.RemoveAt(0);
            }
        }
    }

    /// <summary>
    /// Hands out the newest picture (the one just before the picture on screen), or null when
    /// there is none for <paramref name="serial"/>. The caller owns the frame afterwards.
    /// </summary>
    public VideoFrame? TakeNewest(int serial)
    {
        lock (_lock)
        {
            if (serial != _serial || _frames.Count == 0)
            {
                return null;
            }

            var frame = _frames[^1];
            _frames.RemoveAt(_frames.Count - 1);
            return frame;
        }
    }

    private void ReturnAll()
    {
        foreach (var frame in _frames)
        {
            _pool.Return(frame);
        }

        _frames.Clear();
    }
}
