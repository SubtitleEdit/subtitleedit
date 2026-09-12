using System;
using System.Diagnostics;
using System.Threading;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers.Ffmpeg.Audio;

/// <summary>
/// A sink with no device behind it: consumes audio at wall-clock speed so the player's clock and
/// pacing work exactly as with a real device, just without sound. Used where no platform sink is
/// implemented and as the fallback when the platform sink fails to open.
/// </summary>
public sealed class SilentAudioSink : IAudioSink
{
    private readonly Stopwatch _clock = new();
    private readonly ManualResetEventSlim _wake = new(false);
    private int _bytesPerSecond = 48000 * 2 * 2;
    private long _bytesWritten;
    private volatile bool _disposed;
    private volatile bool _paused;
    private int _generation;

    /// <summary>How far ahead of the clock a write may run before it blocks.</summary>
    private const double LeadSeconds = 0.2;

    public void Open(int sampleRate, int channels)
    {
        _bytesPerSecond = Math.Max(1, sampleRate * channels * 2);
        Reset();
    }

    public double PlayedSeconds => Math.Min(_clock.Elapsed.TotalSeconds, Interlocked.Read(ref _bytesWritten) / (double)_bytesPerSecond);

    public bool Write(ReadOnlySpan<byte> pcm)
    {
        var generation = Volatile.Read(ref _generation);
        var seconds = pcm.Length / (double)_bytesPerSecond;
        while (!_disposed && generation == Volatile.Read(ref _generation))
        {
            var ahead = Interlocked.Read(ref _bytesWritten) / (double)_bytesPerSecond - _clock.Elapsed.TotalSeconds;
            // Take the chunk when the clock is starved, or when it still fits within the lead.
            if (ahead <= 0 || ahead + seconds <= LeadSeconds)
            {
                Interlocked.Add(ref _bytesWritten, pcm.Length);
                return true;
            }

            // Sleep until enough has drained for this chunk to fit (never a negative wait).
            var waitSeconds = Math.Clamp(ahead + seconds - LeadSeconds, 0.002, Math.Max(0.002, seconds));
            _wake.Wait(TimeSpan.FromSeconds(waitSeconds));
            _wake.Reset();
        }

        return false;
    }

    public void Reset()
    {
        Interlocked.Increment(ref _generation);
        Interlocked.Exchange(ref _bytesWritten, 0);
        _clock.Reset();
        if (!_paused)
        {
            _clock.Start();
        }

        _wake.Set();
    }

    public void Pause()
    {
        _paused = true;
        _clock.Stop();
    }

    public void Resume()
    {
        _paused = false;
        _clock.Start();
    }

    public void Dispose()
    {
        _disposed = true;
        _wake.Set();
        _wake.Dispose();
    }
}
