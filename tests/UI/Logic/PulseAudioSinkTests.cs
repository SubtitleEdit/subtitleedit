using Nikse.SubtitleEdit.Logic.VideoPlayers.Ffmpeg.Audio;
using System.Diagnostics;

namespace UITests.Logic;

/// <summary>
/// The Linux audio sink of the FFmpeg player. The clock arithmetic runs everywhere; the tests
/// that talk to a sound server need Linux with libpulse and a PulseAudio or PipeWire server
/// (a null sink in a container will do) and skip themselves otherwise.
/// </summary>
#pragma warning disable CA1416 // the pure helpers are callable anywhere; the live tests check the platform themselves
public class PulseAudioSinkTests
{
    private const int SampleRate = 48000;
    private const int Channels = 2;
    private const int BytesPerSecond = SampleRate * Channels * 2;

    [Fact]
    public void PlayedSeconds_IsWrittenMinusLatency()
    {
        Assert.Equal(0.75, PulseAudioSink.PlayedSecondsFrom(BytesPerSecond, 250_000, false, BytesPerSecond, 0), 6);
    }

    [Fact]
    public void PlayedSeconds_NeverMoreThanWritten_NeverNegative()
    {
        Assert.Equal(1.0, PulseAudioSink.PlayedSecondsFrom(BytesPerSecond, 0, false, BytesPerSecond, 0), 6);
        Assert.Equal(1.0, PulseAudioSink.PlayedSecondsFrom(BytesPerSecond, 40_000, true, BytesPerSecond, 0), 6); // "negative latency" is a record-stream thing
        Assert.Equal(0.0, PulseAudioSink.PlayedSecondsFrom(BytesPerSecond / 10, 900_000, false, BytesPerSecond, 0), 6); // device latency above what was written
    }

    [Fact]
    public void PlayedSeconds_DoesNotGoBackWhenTheLatencyEstimateWobbles()
    {
        var first = PulseAudioSink.PlayedSecondsFrom(BytesPerSecond, 200_000, false, BytesPerSecond, 0);
        var second = PulseAudioSink.PlayedSecondsFrom(BytesPerSecond, 215_000, false, BytesPerSecond, first);

        Assert.Equal(0.8, first, 6);
        Assert.Equal(0.8, second, 6);
    }

    private static PulseAudioSink OpenOrSkip()
    {
        if (!OperatingSystem.IsLinux())
        {
            Assert.Skip("PulseAudio sink is Linux only.");
        }

        if (!PulseAudioSink.IsLibraryAvailable())
        {
            Assert.Skip("libpulse.so.0 not installed.");
        }

        var sink = new PulseAudioSink();
        try
        {
            sink.Open(SampleRate, Channels);
        }
        catch (InvalidOperationException exception)
        {
            sink.Dispose();
            Assert.Skip($"No sound server to test against: {exception.Message}");
        }

        return sink;
    }

    private static byte[] Tone(double seconds)
    {
        var pcm = new byte[(int)(BytesPerSecond * seconds) / 4 * 4];
        for (var i = 0; i < pcm.Length / 4; i++)
        {
            var sample = (short)(Math.Sin(i * 2 * Math.PI * 440 / SampleRate) * 2000);
            pcm[i * 4] = pcm[i * 4 + 2] = (byte)sample;
            pcm[i * 4 + 1] = pcm[i * 4 + 3] = (byte)(sample >> 8);
        }

        return pcm;
    }

    private static bool WaitFor(Func<bool> condition, int timeoutMs)
    {
        var start = Stopwatch.GetTimestamp();
        while (Stopwatch.GetElapsedTime(start).TotalMilliseconds < timeoutMs)
        {
            if (condition())
            {
                return true;
            }

            Thread.Sleep(5);
        }

        return condition();
    }

    [Fact]
    public void Live_Write_IsPacedByTheDevice_AndTheClockFollowsRealTime()
    {
        using var sink = OpenOrSkip();
        var chunk = Tone(0.02);

        // Warm up until sound is flowing. A sound server's null sink (what a container has) runs
        // on a timer of up to two seconds until a stream asks for less, so the very first audio
        // can sit there that long; real devices start at once.
        var written = 0.0;
        for (var i = 0; i < 15; i++)
        {
            Assert.True(sink.Write(chunk));
            written += 0.02;
        }

        Assert.True(WaitFor(() => sink.PlayedSeconds > 0.05, 5000), "playback never started");

        var start = Stopwatch.GetTimestamp();
        var playedAtStart = sink.PlayedSeconds;
        for (var i = 0; i < 100; i++) // 2 seconds of audio
        {
            Assert.True(sink.Write(chunk));
            written += 0.02;
        }

        // The buffer was full, so taking two more seconds of audio takes two seconds.
        var elapsed = Stopwatch.GetElapsedTime(start).TotalSeconds;
        Assert.InRange(elapsed, 1.6, 2.4);

        // ...and the clock moved by as much as the wall clock did.
        Assert.InRange(sink.PlayedSeconds - playedAtStart, elapsed - 0.15, elapsed + 0.15);

        // The rest plays out and the clock comes to rest at exactly what was written.
        Assert.True(WaitFor(() => Math.Abs(sink.PlayedSeconds - written) < 0.0001, 3000), $"rested at {sink.PlayedSeconds} of {written}");
    }

    [Fact]
    public void Live_PlayedSeconds_NeverGoesBack_AndNeverPassesWhatWasWritten()
    {
        using var sink = OpenOrSkip();
        var chunk = Tone(0.02);
        var written = 0.0;
        var last = 0.0;
        for (var i = 0; i < 60; i++)
        {
            Assert.True(sink.Write(chunk));
            written += 0.02;
            var played = sink.PlayedSeconds;
            Assert.True(played >= last, $"went back from {last} to {played}");
            Assert.True(played <= written + 0.0001, $"played {played} of {written} written");
            last = played;
        }
    }

    [Fact]
    public void Live_Pause_HoldsTheClock_AndKeepsTheBufferedAudio()
    {
        using var sink = OpenOrSkip();
        var chunk = Tone(0.02);
        for (var i = 0; i < 25; i++)
        {
            Assert.True(sink.Write(chunk));
        }

        sink.Pause();
        Thread.Sleep(150); // let the cork reach the server
        var paused = sink.PlayedSeconds;
        Thread.Sleep(400);
        Assert.InRange(sink.PlayedSeconds, paused, paused + 0.03);
        Assert.True(paused < 0.5 - 0.05, $"nothing left buffered at {paused}");

        sink.Resume();
        Assert.True(WaitFor(() => Math.Abs(sink.PlayedSeconds - 0.5) < 0.0001, 3000), $"rested at {sink.PlayedSeconds}");
    }

    [Fact]
    public void Live_WriteWhilePaused_FillsTheBufferThenBlocks_AndResetReleasesIt()
    {
        using var sink = OpenOrSkip();
        sink.Pause();
        var chunk = Tone(0.02);
        var accepted = 0;
        var result = true;
        var writer = new Thread(() =>
        {
            for (var i = 0; i < 200 && result; i++) // 4 seconds - far more than the buffer holds
            {
                result = sink.Write(chunk);
                if (result)
                {
                    Interlocked.Increment(ref accepted);
                }
            }
        });
        writer.Start();

        Thread.Sleep(600);
        var buffered = Volatile.Read(ref accepted);
        Assert.InRange(buffered, 5, 150); // pre-decoded audio is taken while paused, up to the buffer
        Assert.True(writer.IsAlive, "the writer should be blocked on the full buffer");
        Assert.Equal(0, sink.PlayedSeconds, 3);

        sink.Reset(); // a seek
        Assert.True(WaitFor(() => !result || Volatile.Read(ref accepted) > buffered, 2000));

        sink.Dispose();
        Assert.True(writer.Join(3000), "Dispose must release a blocked writer");
    }

    [Fact]
    public void Live_Resume_PlaysWhatWasQueuedWhilePaused()
    {
        // The player opens the sink, pauses it and pre-decodes: the first Play must start sound
        // from audio that was all queued before anything ever played.
        using var sink = OpenOrSkip();
        sink.Pause();
        var chunk = Tone(0.02);
        for (var i = 0; i < 5; i++)
        {
            Assert.True(sink.Write(chunk));
        }

        Thread.Sleep(300);
        Assert.Equal(0, sink.PlayedSeconds, 3);

        sink.Resume();
        Assert.True(WaitFor(() => Math.Abs(sink.PlayedSeconds - 0.1) < 0.0001, 5000), $"rested at {sink.PlayedSeconds}");
    }

    [Fact]
    public void Live_Reset_StartsTheClockOver()
    {
        using var sink = OpenOrSkip();
        var chunk = Tone(0.02);
        for (var i = 0; i < 30; i++)
        {
            Assert.True(sink.Write(chunk));
        }

        Assert.True(WaitFor(() => sink.PlayedSeconds > 0.2, 3000));

        sink.Reset();
        Assert.Equal(0, sink.PlayedSeconds, 6);

        for (var i = 0; i < 10; i++)
        {
            Assert.True(sink.Write(chunk));
        }

        Assert.True(WaitFor(() => Math.Abs(sink.PlayedSeconds - 0.2) < 0.0001, 3000), $"rested at {sink.PlayedSeconds}");
    }

    [Fact]
    public void Live_Underrun_KeepsTheClockContinuous()
    {
        using var sink = OpenOrSkip();
        var chunk = Tone(0.02);
        for (var i = 0; i < 10; i++)
        {
            Assert.True(sink.Write(chunk));
        }

        Assert.True(WaitFor(() => Math.Abs(sink.PlayedSeconds - 0.2) < 0.0001, 3000)); // ran dry
        Thread.Sleep(300);
        Assert.Equal(0.2, sink.PlayedSeconds, 4); // silence is not playback

        for (var i = 0; i < 10; i++)
        {
            Assert.True(sink.Write(chunk));
        }

        Assert.True(WaitFor(() => Math.Abs(sink.PlayedSeconds - 0.4) < 0.0001, 3000), $"rested at {sink.PlayedSeconds}");
    }

    [Fact]
    public void Live_OpenTwice_AndDisposeTwice_AreHarmless()
    {
        using var sink = OpenOrSkip();
        sink.Open(SampleRate, Channels);
        Assert.True(sink.Write(Tone(0.02)));
        sink.Dispose();
        sink.Dispose();
        Assert.False(sink.Write(Tone(0.02)));
        Assert.Equal(0, sink.PlayedSeconds);
    }
}
