using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers.Ffmpeg.Audio;

/// <summary>
/// Linux audio output through the PulseAudio client library (libpulse.so.0). That library is the
/// one sound API present on practically every Linux desktop: PulseAudio itself, and PipeWire
/// through pipewire-pulse, which is what current distributions ship. It is also what a Flatpak
/// gets with <c>--socket=pulseaudio</c>. Going to ALSA directly instead would, on a PipeWire
/// system without the ALSA bridge, either fail with "device busy" or grab the sound card away
/// from every other application.
/// <para>
/// This uses the asynchronous API with a threaded main loop, because the simple API cannot do
/// what a master clock needs: it has no pause (cork), and every latency query is a round trip to
/// the server. Here the server's stream buffer is the device queue: <see cref="Write"/> waits for
/// room in it, <see cref="Pause"/> corks it (instant, and what is buffered stays buffered),
/// <see cref="Reset"/> flushes it.
/// </para>
/// <para>
/// The played position is "bytes written minus the latency": the latency libpulse reports is how
/// long a sample written now takes to reach the speaker, interpolated locally between timing
/// updates, so reading it costs no round trip. While the stream is starved the latency falls to
/// zero and the position rests at what was written - it never runs ahead of the audio, and never
/// goes back.
/// </para>
/// <para>
/// A sound server can go away while a video is open (a PipeWire restart is enough). The stream is
/// dead then, and a clock that stands still would freeze the picture for as long as the video
/// plays. So from that moment a <see cref="SilentAudioSink"/> takes over, continuing from the
/// position reached: the video plays on without sound, and loading it again brings the sound back.
/// </para>
/// <para>
/// Locking: every libpulse call is made under the main loop lock. The callbacks libpulse runs on
/// its own thread do nothing but wake the waiters - they never call back into this class or the
/// player, which is what keeps that lock free of ordering problems.
/// </para>
/// </summary>
[SupportedOSPlatform("linux")]
public sealed unsafe partial class PulseAudioSink : IAudioSink
{
    /// <summary>How much audio the server buffers for the stream - the depth of the "device queue" that paces the decoder.</summary>
    private const int BufferMilliseconds = 200;

    private const string LibPulse = "libpulse.so.0";

    private const int SampleS16Le = 3; // PA_SAMPLE_S16LE
    private const int ContextReady = 4; // PA_CONTEXT_READY
    private const int ContextFailed = 5;
    private const int ContextTerminated = 6;
    private const int StreamReady = 2; // PA_STREAM_READY
    private const int StreamFailed = 3;
    private const int StreamTerminated = 4;
    private const int StreamInterpolateTiming = 0x0002; // PA_STREAM_INTERPOLATE_TIMING
    private const int StreamAutoTimingUpdate = 0x0008; // PA_STREAM_AUTO_TIMING_UPDATE
    private const int StreamAdjustLatency = 0x2000; // PA_STREAM_ADJUST_LATENCY
    private const uint AttributeDefault = uint.MaxValue;
    private static readonly nuint WritableSizeError = nuint.MaxValue; // (size_t)-1

    [StructLayout(LayoutKind.Sequential)]
    private struct SampleSpec
    {
        public int Format;
        public uint Rate;
        public byte Channels;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct BufferAttributes
    {
        public uint MaxLength;
        public uint TargetLength;
        public uint PreBuffer;
        public uint MinimumRequest;
        public uint FragmentSize;
    }

    [LibraryImport(LibPulse)]
    private static partial IntPtr pa_threaded_mainloop_new();

    [LibraryImport(LibPulse)]
    private static partial void pa_threaded_mainloop_free(IntPtr mainloop);

    [LibraryImport(LibPulse)]
    private static partial int pa_threaded_mainloop_start(IntPtr mainloop);

    [LibraryImport(LibPulse)]
    private static partial void pa_threaded_mainloop_stop(IntPtr mainloop);

    [LibraryImport(LibPulse)]
    private static partial void pa_threaded_mainloop_lock(IntPtr mainloop);

    [LibraryImport(LibPulse)]
    private static partial void pa_threaded_mainloop_unlock(IntPtr mainloop);

    [LibraryImport(LibPulse)]
    private static partial void pa_threaded_mainloop_wait(IntPtr mainloop);

    [LibraryImport(LibPulse)]
    private static partial void pa_threaded_mainloop_signal(IntPtr mainloop, int waitForAccept);

    [LibraryImport(LibPulse)]
    private static partial IntPtr pa_threaded_mainloop_get_api(IntPtr mainloop);

    [LibraryImport(LibPulse, StringMarshalling = StringMarshalling.Utf8)]
    private static partial IntPtr pa_context_new(IntPtr mainloopApi, string name);

    [LibraryImport(LibPulse)]
    private static partial void pa_context_set_state_callback(IntPtr context, delegate* unmanaged<IntPtr, IntPtr, void> callback, IntPtr userData);

    [LibraryImport(LibPulse)]
    private static partial int pa_context_connect(IntPtr context, IntPtr server, int flags, IntPtr spawnApi);

    [LibraryImport(LibPulse)]
    private static partial int pa_context_get_state(IntPtr context);

    [LibraryImport(LibPulse)]
    private static partial int pa_context_errno(IntPtr context);

    [LibraryImport(LibPulse)]
    private static partial void pa_context_disconnect(IntPtr context);

    [LibraryImport(LibPulse)]
    private static partial void pa_context_unref(IntPtr context);

    [LibraryImport(LibPulse)]
    private static partial IntPtr pa_strerror(int error);

    [LibraryImport(LibPulse, StringMarshalling = StringMarshalling.Utf8)]
    private static partial IntPtr pa_stream_new(IntPtr context, string name, in SampleSpec sampleSpec, IntPtr channelMap);

    [LibraryImport(LibPulse)]
    private static partial void pa_stream_set_state_callback(IntPtr stream, delegate* unmanaged<IntPtr, IntPtr, void> callback, IntPtr userData);

    [LibraryImport(LibPulse)]
    private static partial void pa_stream_set_write_callback(IntPtr stream, delegate* unmanaged<IntPtr, nuint, IntPtr, void> callback, IntPtr userData);

    [LibraryImport(LibPulse)]
    private static partial int pa_stream_connect_playback(IntPtr stream, IntPtr device, in BufferAttributes attributes, int flags, IntPtr volume, IntPtr syncStream);

    [LibraryImport(LibPulse)]
    private static partial int pa_stream_get_state(IntPtr stream);

    [LibraryImport(LibPulse)]
    private static partial nuint pa_stream_writable_size(IntPtr stream);

    [LibraryImport(LibPulse)]
    private static partial int pa_stream_write(IntPtr stream, byte* data, nuint bytes, IntPtr freeCallback, long offset, int seekMode);

    [LibraryImport(LibPulse)]
    private static partial IntPtr pa_stream_cork(IntPtr stream, int cork, IntPtr callback, IntPtr userData);

    [LibraryImport(LibPulse)]
    private static partial IntPtr pa_stream_flush(IntPtr stream, IntPtr callback, IntPtr userData);

    [LibraryImport(LibPulse)]
    private static partial int pa_stream_get_latency(IntPtr stream, out ulong microseconds, out int negative);

    [LibraryImport(LibPulse)]
    private static partial int pa_stream_disconnect(IntPtr stream);

    [LibraryImport(LibPulse)]
    private static partial void pa_stream_unref(IntPtr stream);

    [LibraryImport(LibPulse)]
    private static partial void pa_operation_unref(IntPtr operation);

    private IntPtr _mainloop;
    private IntPtr _context;
    private IntPtr _stream;
    private int _sampleRate;
    private int _channels;
    private int _bytesPerSecond;
    private int _blockAlign = 4;
    private volatile bool _paused;

    // Set once the stream has failed (see class remarks); _fallbackBase is the position it starts from.
    private volatile SilentAudioSink? _fallback;
    private double _fallbackBase;

    // Guarded by the main loop lock.
    private long _bytesWritten;
    private double _lastPlayed;
    private int _generation;

    // Calls in flight against the native objects; Close waits for them to leave before freeing.
    private int _users;
    private volatile bool _closed = true;
    private volatile bool _disposed;

    /// <summary>True when libpulse can be loaded. Says nothing about a server being there - <see cref="Open"/> finds that out.</summary>
    public static bool IsLibraryAvailable()
    {
        if (!NativeLibrary.TryLoad(LibPulse, out var handle))
        {
            return false;
        }

        NativeLibrary.Free(handle);
        return true;
    }

    public void Open(int sampleRate, int channels)
    {
        Close();

        _sampleRate = sampleRate;
        _channels = channels;
        _paused = false;
        _bytesPerSecond = sampleRate * channels * 2;
        _blockAlign = channels * 2;
        _bytesWritten = 0;
        _lastPlayed = 0;

        _mainloop = pa_threaded_mainloop_new();
        if (_mainloop == IntPtr.Zero)
        {
            throw new InvalidOperationException("pa_threaded_mainloop_new failed");
        }

        var locked = false;
        try
        {
            _context = pa_context_new(pa_threaded_mainloop_get_api(_mainloop), "Subtitle Edit");
            if (_context == IntPtr.Zero)
            {
                throw new InvalidOperationException("pa_context_new failed");
            }

            pa_context_set_state_callback(_context, &StateCallback, _mainloop);

            pa_threaded_mainloop_lock(_mainloop);
            locked = true;
            if (pa_threaded_mainloop_start(_mainloop) < 0)
            {
                throw new InvalidOperationException("pa_threaded_mainloop_start failed");
            }

            if (pa_context_connect(_context, IntPtr.Zero, 0, IntPtr.Zero) < 0)
            {
                throw new InvalidOperationException($"pa_context_connect: {ContextError()}");
            }

            // No sound server (or no access to its socket) ends in FAILED right away.
            while (true)
            {
                var state = pa_context_get_state(_context);
                if (state == ContextReady)
                {
                    break;
                }

                if (state is ContextFailed or ContextTerminated)
                {
                    throw new InvalidOperationException($"No PulseAudio/PipeWire server: {ContextError()}");
                }

                pa_threaded_mainloop_wait(_mainloop);
            }

            var spec = new SampleSpec { Format = SampleS16Le, Rate = (uint)sampleRate, Channels = (byte)channels };
            _stream = pa_stream_new(_context, "Video audio", in spec, IntPtr.Zero);
            if (_stream == IntPtr.Zero)
            {
                throw new InvalidOperationException($"pa_stream_new: {ContextError()}");
            }

            pa_stream_set_state_callback(_stream, &StateCallback, _mainloop);
            pa_stream_set_write_callback(_stream, &WriteCallback, _mainloop);

            // No pre-buffering: sound starts with the first write and picks up again by itself
            // after running dry, instead of waiting for the buffer to fill up first.
            var bufferBytes = _bytesPerSecond * BufferMilliseconds / 1000;
            bufferBytes -= bufferBytes % _blockAlign;
            var attributes = new BufferAttributes
            {
                MaxLength = AttributeDefault,
                TargetLength = (uint)bufferBytes,
                PreBuffer = 0,
                MinimumRequest = AttributeDefault,
                FragmentSize = AttributeDefault,
            };
            const int flags = StreamInterpolateTiming | StreamAutoTimingUpdate | StreamAdjustLatency;
            if (pa_stream_connect_playback(_stream, IntPtr.Zero, in attributes, flags, IntPtr.Zero, IntPtr.Zero) < 0)
            {
                throw new InvalidOperationException($"pa_stream_connect_playback: {ContextError()}");
            }

            while (true)
            {
                var state = pa_stream_get_state(_stream);
                if (state == StreamReady)
                {
                    break;
                }

                if (state is StreamFailed or StreamTerminated)
                {
                    throw new InvalidOperationException($"PulseAudio stream failed: {ContextError()}");
                }

                pa_threaded_mainloop_wait(_mainloop);
            }

            pa_threaded_mainloop_unlock(_mainloop);
            locked = false;
            _closed = false;
        }
        catch
        {
            if (locked)
            {
                pa_threaded_mainloop_unlock(_mainloop);
            }

            Teardown();
            throw;
        }
    }

    private string ContextError()
    {
        var text = _context == IntPtr.Zero ? IntPtr.Zero : pa_strerror(pa_context_errno(_context));
        return text == IntPtr.Zero ? "unknown error" : Marshal.PtrToStringUTF8(text) ?? "unknown error";
    }

    /// <summary>Context and stream state changes: wake whoever waits in <see cref="Open"/> or <see cref="Write"/>.</summary>
    [UnmanagedCallersOnly]
    private static void StateCallback(IntPtr source, IntPtr userData)
    {
        pa_threaded_mainloop_signal(userData, 0);
    }

    /// <summary>The server has room for more audio: wake the writer.</summary>
    [UnmanagedCallersOnly]
    private static void WriteCallback(IntPtr stream, nuint bytes, IntPtr userData)
    {
        pa_threaded_mainloop_signal(userData, 0);
    }

    private bool Enter()
    {
        Interlocked.Increment(ref _users);
        if (_closed)
        {
            Interlocked.Decrement(ref _users);
            return false;
        }

        return true;
    }

    private void Exit()
    {
        Interlocked.Decrement(ref _users);
    }

    public double PlayedSeconds
    {
        get
        {
            if (!Enter())
            {
                return 0;
            }

            try
            {
                pa_threaded_mainloop_lock(_mainloop);
                try
                {
                    var fallback = _fallback ?? (pa_stream_get_state(_stream) == StreamReady ? null : FallBackToSilent());
                    if (fallback != null)
                    {
                        return _fallbackBase + fallback.PlayedSeconds;
                    }

                    // Fails with "no data" until the first timing update after connecting or
                    // flushing; the last known position stands until then.
                    if (pa_stream_get_latency(_stream, out var microseconds, out var negative) == 0)
                    {
                        _lastPlayed = PlayedSecondsFrom(_bytesWritten, microseconds, negative != 0, _bytesPerSecond, _lastPlayed);
                    }

                    return _lastPlayed;
                }
                finally
                {
                    pa_threaded_mainloop_unlock(_mainloop);
                }
            }
            finally
            {
                Exit();
            }
        }
    }

    /// <summary>
    /// Seconds played, from what was written and the latency libpulse reports: never more than
    /// was written, never less than reported before (the latency estimate wobbles a little
    /// between timing updates; a clock must not).
    /// </summary>
    internal static double PlayedSecondsFrom(long bytesWritten, ulong latencyMicroseconds, bool latencyNegative, int bytesPerSecond, double lastPlayed)
    {
        if (bytesPerSecond <= 0)
        {
            return 0;
        }

        var written = bytesWritten / (double)bytesPerSecond;
        var latency = latencyNegative ? 0 : latencyMicroseconds / 1_000_000.0;
        return Math.Max(lastPlayed, Math.Clamp(written - latency, 0, written));
    }

    public bool Write(ReadOnlySpan<byte> pcm)
    {
        if (!Enter())
        {
            return false;
        }

        try
        {
            var offset = 0;
            var fallback = _fallback;
            pa_threaded_mainloop_lock(_mainloop);
            try
            {
                var generation = _generation;
                while (offset < pcm.Length && fallback == null)
                {
                    if (_closed || generation != _generation)
                    {
                        return false;
                    }

                    if (pa_stream_get_state(_stream) != StreamReady)
                    {
                        fallback = FallBackToSilent();
                        break;
                    }

                    var writable = pa_stream_writable_size(_stream);
                    if (writable == WritableSizeError)
                    {
                        return false;
                    }

                    var count = (int)Math.Min(writable, (nuint)(pcm.Length - offset));
                    count -= count % _blockAlign;
                    if (count <= 0)
                    {
                        // The stream buffer is full. The wait lets go of the lock; the write
                        // callback, Reset and Close all signal it.
                        pa_threaded_mainloop_wait(_mainloop);
                        continue;
                    }

                    fixed (byte* data = pcm.Slice(offset, count))
                    {
                        if (pa_stream_write(_stream, data, (nuint)count, IntPtr.Zero, 0, 0) < 0)
                        {
                            return false;
                        }
                    }

                    _bytesWritten += count;
                    offset += count;
                }
            }
            finally
            {
                pa_threaded_mainloop_unlock(_mainloop);
            }

            // Not under the main loop lock: the silent sink paces the writer by sleeping.
            return fallback == null || fallback.Write(pcm.Slice(offset));
        }
        finally
        {
            Exit();
        }
    }

    /// <summary>
    /// The stream has failed: hand over to a silent sink that carries the clock on from here.
    /// Called under the main loop lock.
    /// </summary>
    private SilentAudioSink FallBackToSilent()
    {
        var fallback = _fallback;
        if (fallback != null)
        {
            return fallback;
        }

        fallback = new SilentAudioSink();
        fallback.Open(_sampleRate, _channels);
        if (_paused)
        {
            fallback.Pause();
        }

        _fallbackBase = _lastPlayed;
        _fallback = fallback;
        return fallback;
    }

    public void Reset()
    {
        if (!Enter())
        {
            return;
        }

        try
        {
            pa_threaded_mainloop_lock(_mainloop);
            _generation++;
            _bytesWritten = 0;
            _lastPlayed = 0;
            _fallbackBase = 0;
            _fallback?.Reset();

            // Flushing leaves the cork state alone, so a paused stream stays paused. The flush is
            // not waited for: commands are carried out in order, so audio written after this
            // call is queued behind it.
            Unref(pa_stream_flush(_stream, IntPtr.Zero, IntPtr.Zero));
            pa_threaded_mainloop_signal(_mainloop, 0);
            pa_threaded_mainloop_unlock(_mainloop);
        }
        finally
        {
            Exit();
        }
    }

    public void Pause()
    {
        _paused = true;
        Cork(true);
    }

    public void Resume()
    {
        _paused = false;
        Cork(false);
    }

    private void Cork(bool cork)
    {
        if (!Enter())
        {
            return;
        }

        try
        {
            pa_threaded_mainloop_lock(_mainloop);
            Unref(pa_stream_cork(_stream, cork ? 1 : 0, IntPtr.Zero, IntPtr.Zero));
            var fallback = _fallback;
            if (cork)
            {
                fallback?.Pause();
            }
            else
            {
                fallback?.Resume();
            }

            pa_threaded_mainloop_unlock(_mainloop);
        }
        finally
        {
            Exit();
        }
    }

    private static void Unref(IntPtr operation)
    {
        if (operation != IntPtr.Zero)
        {
            pa_operation_unref(operation);
        }
    }

    private void Close()
    {
        if (_closed)
        {
            return;
        }

        _closed = true;

        // A writer may be parked in pa_threaded_mainloop_wait: keep waking it until every call
        // that got in before _closed was set has left. Freeing the main loop under one of them
        // would pull the mutex out from under it.
        var spin = new SpinWait();
        while (Volatile.Read(ref _users) > 0)
        {
            pa_threaded_mainloop_lock(_mainloop);
            pa_threaded_mainloop_signal(_mainloop, 0);
            pa_threaded_mainloop_unlock(_mainloop);
            _fallback?.Reset(); // releases a writer the silent sink is pacing
            spin.SpinOnce();
        }

        _fallback?.Dispose();
        _fallback = null;
        Teardown();
    }

    private void Teardown()
    {
        if (_mainloop == IntPtr.Zero)
        {
            return;
        }

        pa_threaded_mainloop_lock(_mainloop);
        if (_stream != IntPtr.Zero)
        {
            pa_stream_set_state_callback(_stream, null, IntPtr.Zero);
            pa_stream_set_write_callback(_stream, null, IntPtr.Zero);
            pa_stream_disconnect(_stream);
            pa_stream_unref(_stream);
            _stream = IntPtr.Zero;
        }

        if (_context != IntPtr.Zero)
        {
            pa_context_set_state_callback(_context, null, IntPtr.Zero);
            pa_context_disconnect(_context);
            pa_context_unref(_context);
            _context = IntPtr.Zero;
        }

        pa_threaded_mainloop_unlock(_mainloop);

        // Stopping joins the main loop thread, so it must not hold the lock.
        pa_threaded_mainloop_stop(_mainloop);
        pa_threaded_mainloop_free(_mainloop);
        _mainloop = IntPtr.Zero;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        Close();
    }
}
