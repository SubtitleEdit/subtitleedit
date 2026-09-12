using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers.Ffmpeg.Audio;

/// <summary>
/// Windows audio output through the classic waveOut API (winmm.dll). It is available on every
/// Windows, needs no COM apartment, and reports the played position straight from the driver -
/// which is what makes it a good master clock. A fixed ring of small buffers is queued to the
/// device; <see cref="Write"/> blocks while all of them are in flight.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed unsafe partial class WaveOutAudioSink : IAudioSink
{
    private const int BufferCount = 12;
    private const int BufferMilliseconds = 20;

    private const uint WaveMapper = 0xFFFFFFFF;
    private const uint CallbackEvent = 0x00050000;
    private const uint WhdrDone = 0x00000001;
    private const uint TimeBytes = 0x0004;
    private const uint TimeSamples = 0x0002;
    private const uint MmSysErrNoError = 0;

    [StructLayout(LayoutKind.Sequential)]
    private struct WaveFormatEx
    {
        public ushort wFormatTag;
        public ushort nChannels;
        public uint nSamplesPerSec;
        public uint nAvgBytesPerSec;
        public ushort nBlockAlign;
        public ushort wBitsPerSample;
        public ushort cbSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WaveHdr
    {
        public IntPtr lpData;
        public uint dwBufferLength;
        public uint dwBytesRecorded;
        public IntPtr dwUser;
        public uint dwFlags;
        public uint dwLoops;
        public IntPtr lpNext;
        public IntPtr reserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MmTime
    {
        public uint wType;
        public uint u; // cb / sample / ms depending on wType
        public uint pad; // the union is 8 bytes (smpte)
    }

    [LibraryImport("winmm.dll")]
    private static partial uint waveOutOpen(out IntPtr hWaveOut, uint uDeviceId, ref WaveFormatEx lpFormat, IntPtr dwCallback, IntPtr dwInstance, uint dwFlags);

    [LibraryImport("winmm.dll")]
    private static partial uint waveOutClose(IntPtr hWaveOut);

    [LibraryImport("winmm.dll")]
    private static partial uint waveOutPrepareHeader(IntPtr hWaveOut, IntPtr lpWaveOutHdr, uint uSize);

    [LibraryImport("winmm.dll")]
    private static partial uint waveOutUnprepareHeader(IntPtr hWaveOut, IntPtr lpWaveOutHdr, uint uSize);

    [LibraryImport("winmm.dll")]
    private static partial uint waveOutWrite(IntPtr hWaveOut, IntPtr lpWaveOutHdr, uint uSize);

    [LibraryImport("winmm.dll")]
    private static partial uint waveOutPause(IntPtr hWaveOut);

    [LibraryImport("winmm.dll")]
    private static partial uint waveOutRestart(IntPtr hWaveOut);

    [LibraryImport("winmm.dll")]
    private static partial uint waveOutReset(IntPtr hWaveOut);

    [LibraryImport("winmm.dll")]
    private static partial uint waveOutGetPosition(IntPtr hWaveOut, ref MmTime pmmt, uint cbmmt);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial IntPtr CreateEventW(IntPtr lpEventAttributes, [MarshalAs(UnmanagedType.Bool)] bool bManualReset, [MarshalAs(UnmanagedType.Bool)] bool bInitialState, IntPtr lpName);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool CloseHandle(IntPtr hObject);

    private readonly Lock _lock = new();
    private IntPtr _device = IntPtr.Zero;
    private IntPtr _doneEvent = IntPtr.Zero;
    private IntPtr _headers = IntPtr.Zero; // BufferCount x WaveHdr
    private IntPtr _data = IntPtr.Zero; // BufferCount x _bufferBytes
    private int _bufferBytes;
    private int _bytesPerSecond;
    private int _blockAlign = 4;
    private int _nextBuffer;
    private int _generation;
    private volatile bool _disposed;
    private volatile bool _paused;

    // Audio that has left the device before the last Reset. waveOutReset does not rewind the
    // driver's position counter on every driver, so the played time is measured relative to
    // the counter value seen at the reset.
    private long _positionBase;
    private long _lastRawPosition;

    public void Open(int sampleRate, int channels)
    {
        lock (_lock)
        {
            CloseCore();

            var format = new WaveFormatEx
            {
                wFormatTag = 1, // WAVE_FORMAT_PCM
                nChannels = (ushort)channels,
                nSamplesPerSec = (uint)sampleRate,
                wBitsPerSample = 16,
                nBlockAlign = (ushort)(channels * 2),
                nAvgBytesPerSec = (uint)(sampleRate * channels * 2),
                cbSize = 0,
            };
            _bytesPerSecond = (int)format.nAvgBytesPerSec;
            _blockAlign = format.nBlockAlign;
            _bufferBytes = _bytesPerSecond * BufferMilliseconds / 1000;
            _bufferBytes -= _bufferBytes % format.nBlockAlign;

            _doneEvent = CreateEventW(IntPtr.Zero, false, false, IntPtr.Zero);
            var result = waveOutOpen(out _device, WaveMapper, ref format, _doneEvent, IntPtr.Zero, CallbackEvent);
            if (result != MmSysErrNoError)
            {
                CloseCore();
                throw new InvalidOperationException($"waveOutOpen failed with error {result}");
            }

            _headers = Marshal.AllocHGlobal(sizeof(WaveHdr) * BufferCount);
            _data = Marshal.AllocHGlobal(_bufferBytes * BufferCount);
            for (var i = 0; i < BufferCount; i++)
            {
                var header = (WaveHdr*)_headers + i;
                *header = new WaveHdr
                {
                    lpData = _data + i * _bufferBytes,
                    dwBufferLength = (uint)_bufferBytes,
                    dwFlags = 0, // must be zero when prepared
                };
                waveOutPrepareHeader(_device, (IntPtr)header, (uint)sizeof(WaveHdr));
                header->dwFlags |= WhdrDone; // free
            }

            _nextBuffer = 0;
            _positionBase = 0;
            _lastRawPosition = 0;
            _paused = false;
        }
    }

    public double PlayedSeconds
    {
        get
        {
            lock (_lock)
            {
                if (_device == IntPtr.Zero || _bytesPerSecond == 0)
                {
                    return 0;
                }

                var raw = GetRawPositionBytes();
                return Math.Max(0, raw - _positionBase) / (double)_bytesPerSecond;
            }
        }
    }

    private long GetRawPositionBytes()
    {
        var time = new MmTime { wType = TimeBytes };
        if (waveOutGetPosition(_device, ref time, (uint)sizeof(MmTime)) != MmSysErrNoError)
        {
            return _lastRawPosition;
        }

        long position;
        if (time.wType == TimeBytes)
        {
            position = time.u;
        }
        else if (time.wType == TimeSamples)
        {
            // Some drivers refuse TIME_BYTES and answer in sample frames instead.
            position = (long)time.u * _blockAlign;
        }
        else
        {
            return _lastRawPosition;
        }

        // The 32-bit counter wraps after ~6 hours of 48 kHz stereo; keep it monotonic.
        if (position < (_lastRawPosition & 0xFFFFFFFF))
        {
            _lastRawPosition += 0x100000000;
        }

        _lastRawPosition = (_lastRawPosition & ~0xFFFFFFFFL) | position;
        return _lastRawPosition;
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

            WaveHdr* header;
            lock (_lock)
            {
                if (_device == IntPtr.Zero)
                {
                    return false;
                }

                header = (WaveHdr*)_headers + _nextBuffer;
                if ((header->dwFlags & WhdrDone) == 0)
                {
                    header = null;
                }
            }

            if (header == null)
            {
                // All buffers are queued - wait for the driver to hand one back.
                WaitForSingleObject(_doneEvent, (uint)BufferMilliseconds);
                continue;
            }

            var count = Math.Min(_bufferBytes, pcm.Length - offset);
            pcm.Slice(offset, count).CopyTo(new Span<byte>((void*)header->lpData, count));
            offset += count;

            lock (_lock)
            {
                if (_device == IntPtr.Zero || generation != _generation)
                {
                    return false;
                }

                header->dwBufferLength = (uint)count;
                header->dwFlags &= ~WhdrDone;
                if (waveOutWrite(_device, (IntPtr)header, (uint)sizeof(WaveHdr)) != MmSysErrNoError)
                {
                    header->dwFlags |= WhdrDone;
                    return false;
                }

                _nextBuffer = (_nextBuffer + 1) % BufferCount;
            }
        }

        return true;
    }

    public void Reset()
    {
        lock (_lock)
        {
            Interlocked.Increment(ref _generation);
            if (_device == IntPtr.Zero)
            {
                return;
            }

            waveOutReset(_device); // returns every queued buffer with WHDR_DONE set

            // Drivers differ on whether waveOutReset rewinds the position counter; forget the
            // wrap-around history first so a rewind to 0 is not mistaken for a 32-bit wrap.
            _lastRawPosition = 0;
            _positionBase = GetRawPositionBytes();
            _nextBuffer = 0;
            for (var i = 0; i < BufferCount; i++)
            {
                ((WaveHdr*)_headers + i)->dwFlags |= WhdrDone;
            }

            if (_paused)
            {
                waveOutPause(_device); // waveOutReset implicitly restarts a paused device
            }
        }
    }

    public void Pause()
    {
        lock (_lock)
        {
            _paused = true;
            if (_device != IntPtr.Zero)
            {
                waveOutPause(_device);
            }
        }
    }

    public void Resume()
    {
        lock (_lock)
        {
            _paused = false;
            if (_device != IntPtr.Zero)
            {
                waveOutRestart(_device);
            }
        }
    }

    private void CloseCore()
    {
        if (_device != IntPtr.Zero)
        {
            waveOutReset(_device);
            if (_headers != IntPtr.Zero)
            {
                for (var i = 0; i < BufferCount; i++)
                {
                    waveOutUnprepareHeader(_device, (IntPtr)((WaveHdr*)_headers + i), (uint)sizeof(WaveHdr));
                }
            }

            waveOutClose(_device);
            _device = IntPtr.Zero;
        }

        if (_headers != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(_headers);
            _headers = IntPtr.Zero;
        }

        if (_data != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(_data);
            _data = IntPtr.Zero;
        }

        if (_doneEvent != IntPtr.Zero)
        {
            CloseHandle(_doneEvent);
            _doneEvent = IntPtr.Zero;
        }
    }

    public void Dispose()
    {
        _disposed = true;
        lock (_lock)
        {
            Interlocked.Increment(ref _generation);
            CloseCore();
        }
    }
}
