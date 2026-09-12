using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers.Ffmpeg.Audio;

/// <summary>
/// macOS audio output through Core Audio's AudioQueue API (AudioToolbox.framework). It is part
/// of every macOS, needs no extra libraries and, like waveOut on Windows, hands out a ring of
/// small buffers that the queue returns through a callback when they have been played. The queue
/// runs its own internal thread for the callbacks, so nothing here touches a run loop.
/// <para>
/// The played position comes from the queue's sample-time clock. That clock keeps running while
/// the queue is starved (it plays silence), so it is re-based whenever new audio is queued into an
/// empty queue and clamped to the bytes actually written - otherwise a starved gap after a seek
/// would show up as audio that never played.
/// </para>
/// </summary>
[SupportedOSPlatform("macos")]
public sealed unsafe partial class AudioQueueAudioSink : IAudioSink
{
    private const int BufferCount = 12;
    private const int BufferMilliseconds = 20;

    private const string AudioToolbox = "/System/Library/Frameworks/AudioToolbox.framework/AudioToolbox";

    private const uint FormatLinearPcm = 0x6C70636D; // 'lpcm'
    private const uint FlagIsSignedInteger = 0x4;
    private const uint FlagIsPacked = 0x8;

    [StructLayout(LayoutKind.Sequential)]
    private struct AudioStreamBasicDescription
    {
        public double mSampleRate;
        public uint mFormatID;
        public uint mFormatFlags;
        public uint mBytesPerPacket;
        public uint mFramesPerPacket;
        public uint mBytesPerFrame;
        public uint mChannelsPerFrame;
        public uint mBitsPerChannel;
        public uint mReserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct AudioQueueBuffer
    {
        public uint mAudioDataBytesCapacity;
        public IntPtr mAudioData;
        public uint mAudioDataByteSize;
        public IntPtr mUserData;
        public uint mPacketDescriptionCapacity;
        public IntPtr mPacketDescriptions;
        public uint mPacketDescriptionCount;
    }

    /// <summary>AudioTimeStamp is 64 bytes; only the leading mSampleTime is used here.</summary>
    [StructLayout(LayoutKind.Sequential, Size = 64)]
    private struct AudioTimeStamp
    {
        public double mSampleTime;
    }

    [LibraryImport(AudioToolbox)]
    private static partial int AudioQueueNewOutput(in AudioStreamBasicDescription inFormat, delegate* unmanaged<IntPtr, IntPtr, AudioQueueBuffer*, void> inCallbackProc, IntPtr inUserData, IntPtr inCallbackRunLoop, IntPtr inRunLoopMode, uint inFlags, out IntPtr outAQ);

    [LibraryImport(AudioToolbox)]
    private static partial int AudioQueueDispose(IntPtr inAQ, byte inImmediate);

    [LibraryImport(AudioToolbox)]
    private static partial int AudioQueueAllocateBuffer(IntPtr inAQ, uint inBufferByteSize, out AudioQueueBuffer* outBuffer);

    [LibraryImport(AudioToolbox)]
    private static partial int AudioQueueEnqueueBuffer(IntPtr inAQ, AudioQueueBuffer* inBuffer, uint inNumPacketDescs, IntPtr inPacketDescs);

    [LibraryImport(AudioToolbox)]
    private static partial int AudioQueueStart(IntPtr inAQ, IntPtr inStartTime);

    [LibraryImport(AudioToolbox)]
    private static partial int AudioQueuePause(IntPtr inAQ);

    [LibraryImport(AudioToolbox)]
    private static partial int AudioQueueStop(IntPtr inAQ, byte inImmediate);

    [LibraryImport(AudioToolbox)]
    private static partial int AudioQueueReset(IntPtr inAQ);

    [LibraryImport(AudioToolbox)]
    private static partial int AudioQueueGetCurrentTime(IntPtr inAQ, IntPtr inTimeline, out AudioTimeStamp outTimeStamp, IntPtr outTimelineDiscontinuity);

    private readonly Lock _lock = new();
    private readonly AutoResetEvent _doneEvent = new(false);
    private GCHandle _self;
    private IntPtr _queue = IntPtr.Zero;
    private AudioQueueBuffer*[] _buffers = [];
    private readonly bool[] _free = new bool[BufferCount];
    private int _bufferBytes;
    private int _bytesPerSecond;
    private int _blockAlign = 4;
    private int _nextBuffer;
    private int _inFlight;
    private int _generation;
    private volatile bool _disposed;
    private volatile bool _paused;
    private bool _started;

    // Sample time of the queue clock when the current run of audio started (see class remarks),
    // and how many bytes have been queued since - the clock can never be ahead of that.
    private double _sampleBase;
    private double _lastSampleTime;
    private long _bytesWritten;

    public void Open(int sampleRate, int channels)
    {
        CloseCore();
        lock (_lock)
        {
            var format = new AudioStreamBasicDescription
            {
                mSampleRate = sampleRate,
                mFormatID = FormatLinearPcm,
                mFormatFlags = FlagIsSignedInteger | FlagIsPacked,
                mBytesPerPacket = (uint)(channels * 2),
                mFramesPerPacket = 1,
                mBytesPerFrame = (uint)(channels * 2),
                mChannelsPerFrame = (uint)channels,
                mBitsPerChannel = 16,
            };
            _bytesPerSecond = sampleRate * channels * 2;
            _blockAlign = channels * 2;
            _bufferBytes = _bytesPerSecond * BufferMilliseconds / 1000;
            _bufferBytes -= _bufferBytes % _blockAlign;

            _self = GCHandle.Alloc(this, GCHandleType.Weak);
            var result = AudioQueueNewOutput(in format, &OutputCallback, GCHandle.ToIntPtr(_self), IntPtr.Zero, IntPtr.Zero, 0, out _queue);
            if (result != 0)
            {
                CloseCore();
                throw new InvalidOperationException($"AudioQueueNewOutput failed with error {result}");
            }

            _buffers = new AudioQueueBuffer*[BufferCount];
            for (var i = 0; i < BufferCount; i++)
            {
                result = AudioQueueAllocateBuffer(_queue, (uint)_bufferBytes, out _buffers[i]);
                if (result != 0)
                {
                    CloseCore();
                    throw new InvalidOperationException($"AudioQueueAllocateBuffer failed with error {result}");
                }

                _buffers[i]->mUserData = i;
                _free[i] = true;
            }

            _nextBuffer = 0;
            _inFlight = 0;
            _started = false;
            _paused = false;
            _sampleBase = 0;
            _lastSampleTime = 0;
            _bytesWritten = 0;
        }
    }

    [UnmanagedCallersOnly]
    private static void OutputCallback(IntPtr userData, IntPtr queue, AudioQueueBuffer* buffer)
    {
        if (GCHandle.FromIntPtr(userData).Target is not AudioQueueAudioSink sink)
        {
            return;
        }

        var index = (int)buffer->mUserData;
        if ((uint)index < BufferCount)
        {
            lock (sink._lock)
            {
                if (!sink._free[index])
                {
                    sink._free[index] = true;
                    sink._inFlight--;
                }
            }
        }

        try
        {
            sink._doneEvent.Set();
        }
        catch (ObjectDisposedException)
        {
            // disposed while a buffer was still draining
        }
    }

    public double PlayedSeconds
    {
        get
        {
            lock (_lock)
            {
                if (_queue == IntPtr.Zero || _bytesPerSecond == 0)
                {
                    return 0;
                }

                var played = Math.Max(0, CurrentSampleTime() - _sampleBase) * _blockAlign;
                return Math.Min(played, _bytesWritten) / _bytesPerSecond;
            }
        }
    }

    /// <summary>Queue clock in sample frames; the last known value when the queue is not running.</summary>
    private double CurrentSampleTime()
    {
        if (!_started)
        {
            return _lastSampleTime;
        }

        var result = AudioQueueGetCurrentTime(_queue, IntPtr.Zero, out var time, IntPtr.Zero);
        if (result == 0 && !double.IsNaN(time.mSampleTime))
        {
            _lastSampleTime = time.mSampleTime;
        }

        return _lastSampleTime;
    }

    public bool Write(ReadOnlySpan<byte> pcm)
    {
        var generation = _generation;
        var offset = 0;
        while (offset < pcm.Length)
        {
            if (_disposed || generation != _generation)
            {
                return false;
            }

            AudioQueueBuffer* buffer;
            lock (_lock)
            {
                if (_queue == IntPtr.Zero)
                {
                    return false;
                }

                buffer = _free[_nextBuffer] ? _buffers[_nextBuffer] : null;
            }

            if (buffer == null)
            {
                // All buffers are queued - wait for the queue to hand one back.
                _doneEvent.WaitOne(BufferMilliseconds);
                continue;
            }

            var count = Math.Min(_bufferBytes, pcm.Length - offset);
            pcm.Slice(offset, count).CopyTo(new Span<byte>((void*)buffer->mAudioData, count));
            offset += count;

            lock (_lock)
            {
                if (_queue == IntPtr.Zero || generation != _generation)
                {
                    return false;
                }

                if (_inFlight == 0)
                {
                    // The queue was starved: whatever the clock did meanwhile was silence, so the
                    // audio time of the new run starts at the clock's current value.
                    _sampleBase = CurrentSampleTime();
                    _bytesWritten = 0;
                }

                buffer->mAudioDataByteSize = (uint)count;
                _free[_nextBuffer] = false;
                _inFlight++;
                if (AudioQueueEnqueueBuffer(_queue, buffer, 0, IntPtr.Zero) != 0)
                {
                    _free[_nextBuffer] = true;
                    _inFlight--;
                    return false;
                }

                _bytesWritten += count;
                _nextBuffer = (_nextBuffer + 1) % BufferCount;

                if (!_started && !_paused)
                {
                    // Start once real audio is queued; starting an empty queue just plays silence.
                    _started = AudioQueueStart(_queue, IntPtr.Zero) == 0;
                }
            }
        }

        return true;
    }

    public void Reset()
    {
        Interlocked.Increment(ref _generation);
        IntPtr queue;
        lock (_lock)
        {
            queue = _queue;
        }

        if (queue == IntPtr.Zero)
        {
            return;
        }

        // Drops the queued buffers and returns them through the callback, which may run
        // synchronously on the queue thread - so this must not hold _lock.
        AudioQueueReset(queue);

        lock (_lock)
        {
            if (_queue != queue)
            {
                return;
            }

            for (var i = 0; i < BufferCount; i++)
            {
                _free[i] = true;
            }

            _inFlight = 0;
            _nextBuffer = 0;
            _bytesWritten = 0;
            _sampleBase = CurrentSampleTime();
            _doneEvent.Set();
        }
    }

    public void Pause()
    {
        lock (_lock)
        {
            _paused = true;
            if (_queue != IntPtr.Zero && _started)
            {
                AudioQueuePause(_queue);
            }
        }
    }

    public void Resume()
    {
        lock (_lock)
        {
            _paused = false;
            if (_queue != IntPtr.Zero && _started)
            {
                AudioQueueStart(_queue, IntPtr.Zero);
            }
        }
    }

    /// <summary>Tears the queue down. Not called under <see cref="_lock"/>: disposing may run callbacks.</summary>
    private void CloseCore()
    {
        IntPtr queue;
        lock (_lock)
        {
            queue = _queue;
            _queue = IntPtr.Zero;
            _buffers = [];
            _started = false;
        }

        if (queue != IntPtr.Zero)
        {
            AudioQueueStop(queue, 1);
            AudioQueueDispose(queue, 1); // frees the queue's buffers as well
        }

        if (_self.IsAllocated)
        {
            _self.Free();
        }
    }

    public void Dispose()
    {
        _disposed = true;
        Interlocked.Increment(ref _generation);
        CloseCore();
        _doneEvent.Set();
        _doneEvent.Dispose();
    }
}
