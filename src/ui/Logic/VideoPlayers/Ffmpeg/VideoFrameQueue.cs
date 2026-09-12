using System.Collections.Generic;
using System.Threading;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers.Ffmpeg;

/// <summary>
/// Converted pictures waiting to be shown, in decode order, with a small fixed capacity so the
/// video decoder cannot run away from the clock. Frames come from and go back to a pool owned by
/// the player; the queue only hands them around.
/// </summary>
public sealed class VideoFrameQueue
{
    private readonly Queue<VideoFrame> _frames = new();
    private readonly Stack<VideoFrame> _pool = new();
    // A plain object: Monitor.Wait/PulseAll are used for the hand-off, and those do not work with System.Threading.Lock.
    private readonly object _lock = new();
    private readonly int _capacity;
    private bool _closed;
    private int _width;
    private int _height;

    public VideoFrameQueue(int capacity)
    {
        _capacity = capacity;
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

    /// <summary>
    /// A free buffer of the given size. Buffers of another size (the picture size changed
    /// mid-stream) are dropped and reallocated. Blocks while the queue is full; null when closed.
    /// </summary>
    public VideoFrame? Rent(int width, int height, int serial, ref int currentSerial)
    {
        lock (_lock)
        {
            while (!_closed && _frames.Count >= _capacity && serial == Volatile.Read(ref currentSerial))
            {
                Monitor.Wait(_lock, 20);
            }

            if (_closed || serial != Volatile.Read(ref currentSerial))
            {
                return null;
            }

            if (_width != width || _height != height)
            {
                while (_pool.Count > 0)
                {
                    _pool.Pop().Dispose();
                }

                _width = width;
                _height = height;
            }

            if (_pool.Count > 0)
            {
                return _pool.Pop();
            }

            return new VideoFrame(width, height);
        }
    }

    public void Push(VideoFrame frame)
    {
        lock (_lock)
        {
            if (_closed)
            {
                frame.Dispose();
                return;
            }

            _frames.Enqueue(frame);
            Monitor.PulseAll(_lock);
        }
    }

    public VideoFrame? Peek()
    {
        lock (_lock)
        {
            return _frames.Count > 0 ? _frames.Peek() : null;
        }
    }

    /// <summary>The frame behind the head, or null when there is none.</summary>
    public VideoFrame? PeekSecond()
    {
        lock (_lock)
        {
            if (_frames.Count < 2)
            {
                return null;
            }

            var index = 0;
            foreach (var frame in _frames)
            {
                if (index++ == 1)
                {
                    return frame;
                }
            }

            return null;
        }
    }

    public VideoFrame? Pop()
    {
        lock (_lock)
        {
            if (_frames.Count == 0)
            {
                return null;
            }

            var frame = _frames.Dequeue();
            Monitor.PulseAll(_lock);
            return frame;
        }
    }

    /// <summary>Hands a buffer back once the picture is no longer shown.</summary>
    public void Return(VideoFrame? frame)
    {
        if (frame == null)
        {
            return;
        }

        lock (_lock)
        {
            if (_closed || frame.Width != _width || frame.Height != _height || frame.Data == System.IntPtr.Zero)
            {
                frame.Dispose();
                return;
            }

            frame.IsEndOfStream = false;
            _pool.Push(frame);
            Monitor.PulseAll(_lock);
        }
    }

    /// <summary>Drops all queued frames (back to the pool).</summary>
    public void Flush()
    {
        lock (_lock)
        {
            while (_frames.Count > 0)
            {
                var frame = _frames.Dequeue();
                if (frame.Data != System.IntPtr.Zero && frame.Width == _width && frame.Height == _height)
                {
                    _pool.Push(frame);
                }
                else
                {
                    frame.Dispose();
                }
            }

            Monitor.PulseAll(_lock);
        }
    }

    public void Close()
    {
        lock (_lock)
        {
            _closed = true;
            while (_frames.Count > 0)
            {
                _frames.Dequeue().Dispose();
            }

            while (_pool.Count > 0)
            {
                _pool.Pop().Dispose();
            }

            Monitor.PulseAll(_lock);
        }
    }
}
