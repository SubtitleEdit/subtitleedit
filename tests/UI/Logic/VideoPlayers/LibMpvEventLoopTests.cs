using System.Diagnostics;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;

namespace UITests.Logic.VideoPlayers;

/// <summary>
/// Live-core tests for the mpv event loop (PR #13925 + observed time-pos): a real libmpv core
/// with null video/audio outputs, so the observed-property caches (pause, duration, time-pos)
/// and the MPV_EVENT_PLAYBACK_RESTART signal are exercised end to end - these back the waveform
/// playhead cursor, so a regression here is a cursor regression.
///
/// The tests need a loadable libmpv; on machines without one (bare CI runners) every test
/// returns early and passes vacuously rather than failing on missing infrastructure.
/// </summary>
public class LibMpvEventLoopTests
{
    private const int WavSeconds = 2;

    [Fact]
    public async Task LoadFile_EventLoopServesPauseAndDuration()
    {
        var player = CreatePlayer();
        if (player == null)
        {
            return; // no libmpv on this machine
        }

        var wavFileName = WriteTempWav();
        try
        {
            Assert.True(player.SupportsPlaybackRestartEvents);

            await player.LoadFile(wavFileName);
            Assert.True(await WaitUntilAsync(() => player.Duration > 1.0, 5000),
                $"duration never arrived via the event cache (was {player.Duration})");

            Assert.InRange(player.Duration, 1.5, 2.5);
            Assert.True(player.IsPaused, "LoadFile must leave the core paused");
            Assert.False(player.IsPlaying);
        }
        finally
        {
            player.Dispose();
            TryDelete(wavFileName);
        }
    }

    [Fact]
    public async Task Play_ObservedTimePosAdvances()
    {
        var player = CreatePlayer();
        if (player == null)
        {
            return; // no libmpv on this machine
        }

        var wavFileName = WriteTempWav();
        try
        {
            await player.LoadFile(wavFileName);
            Assert.True(await WaitUntilAsync(() => player.Duration > 1.0, 5000), "file never loaded");

            player.Play();
            Assert.True(await WaitUntilAsync(() => player.IsPlaying, 3000),
                "IsPlaying never flipped via the observed pause cache");
            Assert.True(await WaitUntilAsync(() => player.Position > 0.1, 3000),
                $"observed time-pos never started advancing (was {player.Position})");

            var earlier = player.Position;
            await Task.Delay(400, TestContext.Current.CancellationToken);
            var later = player.Position;
            Assert.True(later > earlier,
                $"observed time-pos did not advance during playback ({earlier} -> {later})");
        }
        finally
        {
            player.Dispose();
            TryDelete(wavFileName);
        }
    }

    [Fact]
    public async Task Seek_FiresPlaybackRestartAndLandsObservedTimePos()
    {
        var player = CreatePlayer();
        if (player == null)
        {
            return; // no libmpv on this machine
        }

        var wavFileName = WriteTempWav();
        try
        {
            await player.LoadFile(wavFileName);
            Assert.True(await WaitUntilAsync(() => player.Duration > 1.0, 5000), "file never loaded");

            player.Play();
            Assert.True(await WaitUntilAsync(() => player.IsPlaying, 3000), "playback never started");

            var beforeSeekTimestamp = Stopwatch.GetTimestamp();
            player.Position = 1.2;

            Assert.True(await WaitUntilAsync(() => player.HasPlaybackRestartedSince(beforeSeekTimestamp), 5000),
                "MPV_EVENT_PLAYBACK_RESTART never arrived after the seek");

            // After the restart the observed position must be at the seek target (allow a little
            // playback advance past it, and hr-seek tolerance before it).
            Assert.True(await WaitUntilAsync(() => player.Position >= 1.0, 3000),
                $"observed time-pos never reached the seek target (was {player.Position})");
            Assert.InRange(player.Position, 1.0, WavSeconds + 0.3);
        }
        finally
        {
            player.Dispose();
            TryDelete(wavFileName);
        }
    }

    [Fact]
    public async Task SeekStorm_LandsOnTheLastTarget()
    {
        var player = CreatePlayer();
        if (player == null)
        {
            return; // no libmpv on this machine
        }

        var wavFileName = WriteTempWav();
        try
        {
            await player.LoadFile(wavFileName);
            Assert.True(await WaitUntilAsync(() => player.Duration > 1.0, 5000), "file never loaded");

            player.Play();
            Assert.True(await WaitUntilAsync(() => player.IsPlaying, 3000), "playback never started");

            // What a slider drag looks like to the player: a burst of fire-and-forget seeks,
            // one per input event, newest target winning. The storm must neither wedge the
            // core nor land anywhere but the last target.
            var beforeStormTimestamp = Stopwatch.GetTimestamp();
            for (var i = 0; i < 25; i++)
            {
                player.Position = 0.2 + i * 0.05; // ends at 1.4
            }

            Assert.True(await WaitUntilAsync(() => player.HasPlaybackRestartedSince(beforeStormTimestamp), 5000),
                "no playback restart arrived after the seek storm");
            Assert.True(await WaitUntilAsync(() => player.Position >= 1.2, 5000),
                $"observed time-pos never reached the storm's final target (was {player.Position})");
            Assert.InRange(player.Position, 1.2, WavSeconds + 0.3);
        }
        finally
        {
            player.Dispose();
            TryDelete(wavFileName);
        }
    }

    /// <summary>
    /// A restart is stamped when the event thread DEQUEUES it, not when mpv emitted it, so the
    /// restart that opening the file fires can be stamped later than a seek issued in the
    /// meantime - and "has anything restarted since some old moment?" would then answer yes for
    /// a seek that has not run at all. mpv's event queue is FIFO, so the seek's own
    /// MPV_EVENT_COMMAND_REPLY orders the two; while a seek is outstanding the answer must be
    /// about that seek. Concretely: reporting a restart since the beginning of time while
    /// reporting none since the seek was issued is the contradiction that broke Pause() (#14187).
    /// </summary>
    [Fact]
    public async Task HasPlaybackRestartedSince_AnswersForTheOutstandingSeekNotAnOlderRestart()
    {
        var player = CreatePlayer();
        if (player == null)
        {
            return; // no libmpv on this machine
        }

        var wavFileName = WriteTempWav();
        try
        {
            await player.LoadFile(wavFileName);
            Assert.True(await WaitUntilAsync(() => player.Duration > 1.0, 5000), "file never loaded");

            player.Play();
            Assert.True(await WaitUntilAsync(() => player.IsPlaying, 3000), "playback never started");

            // Opening the file fires a restart of its own (unpausing does not) - that is the one
            // that must not be mistaken for the seek's below. No seek has been issued yet, so
            // this is a plain "any restart at all?" question; wait until it is definitely in.
            Assert.True(await WaitUntilAsync(() => player.HasPlaybackRestartedSince(0), 5000),
                "no playback restart arrived before the seek");

            var beforeSeekTimestamp = Stopwatch.GetTimestamp();
            player.Position = 1.2;

            var landed = false;
            var end = Environment.TickCount64 + 5000;
            while (Environment.TickCount64 < end)
            {
                // "Since forever" first, "since the seek" second: read the other way round, a
                // restart landing between the two reads would look like a violation. In this
                // order a restart that lands mid-iteration simply satisfies both.
                var restartedSinceForever = player.HasPlaybackRestartedSince(0);
                landed = player.HasPlaybackRestartedSince(beforeSeekTimestamp);
                Assert.False(restartedSinceForever && !landed,
                    "a restart from before the seek passed for the outstanding seek's own restart");
                if (landed)
                {
                    break;
                }
            }

            Assert.True(landed, "the seek's playback restart never arrived");
            Assert.True(player.HasPlaybackRestartedSince(0), "the landed seek should satisfy any older timestamp");
        }
        finally
        {
            player.Dispose();
            TryDelete(wavFileName);
        }
    }

    /// <summary>
    /// The observed pause cache must be truthful the instant Play()/Pause()/PlayOrPause()
    /// return. Those commands are synchronous - the core has applied them - but mpv's pause
    /// property-change event only reaches the cache a moment later on the event thread, and
    /// IsPaused/IsPlaying answer from that cache. The Position getter gates its cached
    /// seek-target branch on IsPaused, so a stale "still playing" answer there is what let a
    /// waveform click report mpv's pre-seek time-pos instead of the clicked position (#14187) -
    /// a race the click test below only loses when the machine is busy.
    /// </summary>
    [Fact]
    public async Task PlayAndPause_UpdateTheObservedPauseCacheImmediately()
    {
        var player = CreatePlayer();
        if (player == null)
        {
            return; // no libmpv on this machine
        }

        var wavFileName = WriteTempWav();
        try
        {
            await player.LoadFile(wavFileName);
            Assert.True(await WaitUntilAsync(() => player.Duration > 1.0, 5000), "file never loaded");

            player.Play();
            Assert.True(player.IsPlaying, "IsPlaying still false on the very next read after Play()");
            Assert.False(player.IsPaused, "IsPaused still true on the very next read after Play()");

            player.Pause();
            Assert.True(player.IsPaused, "IsPaused still false on the very next read after Pause()");
            Assert.False(player.IsPlaying, "IsPlaying still true on the very next read after Pause()");

            player.PlayOrPause();
            Assert.True(player.IsPlaying, "IsPlaying still false on the very next read after PlayOrPause()");

            player.PlayOrPause();
            Assert.True(player.IsPaused, "IsPaused still false on the very next read after PlayOrPause()");
        }
        finally
        {
            player.Dispose();
            TryDelete(wavFileName);
        }
    }

    /// <summary>
    /// Issue #14187: a waveform click seeks first (pointer release) and pauses a moment later
    /// (tap) - and the second Position assignment no-ops at the Avalonia property layer, so the
    /// player-level setter never runs again after the pause. Pause() must therefore not clear
    /// the cached seek target while that seek is still in flight: the getter would fall back to
    /// mpv's pre-seek time-pos and the waveform cursor jumped off the clicked position.
    /// </summary>
    [Fact]
    public async Task Pause_RightAfterSeek_KeepsReportingTheSeekTarget()
    {
        var player = CreatePlayer();
        if (player == null)
        {
            return; // no libmpv on this machine
        }

        var wavFileName = WriteTempWav();
        try
        {
            await player.LoadFile(wavFileName);
            Assert.True(await WaitUntilAsync(() => player.Duration > 1.0, 5000), "file never loaded");

            player.Play();
            Assert.True(await WaitUntilAsync(() => player.IsPlaying, 3000), "playback never started");
            Assert.True(await WaitUntilAsync(() => player.Position > 0.1, 3000), "time-pos never advanced");

            // The click sequence, back to back so the seek is still in flight when Pause runs.
            player.Position = 1.5;
            player.Pause();

            // The very first read must already be the click target - the cursor pin releases
            // onto whatever this getter returns.
            Assert.Equal(1.5, player.Position, 3);

            // And it must stay there while the async seek lands and the pause settles.
            var end = Environment.TickCount64 + 800;
            while (Environment.TickCount64 < end)
            {
                Assert.Equal(1.5, player.Position, 3);
                await Task.Delay(40, TestContext.Current.CancellationToken);
            }
        }
        finally
        {
            player.Dispose();
            TryDelete(wavFileName);
        }
    }

    /// <summary>
    /// The stale-target half of the same invariant: a seek whose restart has long since fired is
    /// finished business, so pausing later must NOT jump the reported position back to it.
    /// </summary>
    [Fact]
    public async Task Pause_LongAfterSeek_DoesNotJumpBackToTheOldTarget()
    {
        var player = CreatePlayer();
        if (player == null)
        {
            return; // no libmpv on this machine
        }

        var wavFileName = WriteTempWav();
        try
        {
            await player.LoadFile(wavFileName);
            Assert.True(await WaitUntilAsync(() => player.Duration > 1.0, 5000), "file never loaded");

            player.Play();
            Assert.True(await WaitUntilAsync(() => player.IsPlaying, 3000), "playback never started");

            var beforeSeekTimestamp = Stopwatch.GetTimestamp();
            player.Position = 0.5;
            Assert.True(await WaitUntilAsync(() => player.HasPlaybackRestartedSince(beforeSeekTimestamp), 5000),
                "seek never restarted");

            // Let playback run well past the old target, then pause.
            Assert.True(await WaitUntilAsync(() => player.Position >= 0.9, 5000),
                $"playback never moved past the old seek target (was {player.Position})");
            player.Pause();
            Assert.True(await WaitUntilAsync(() => player.IsPaused, 3000), "pause never observed");

            // Reported position must be where playback stopped, not the 0.5 from the old seek.
            Assert.True(player.Position >= 0.85,
                $"paused position jumped back toward the stale seek target (was {player.Position})");
        }
        finally
        {
            player.Dispose();
            TryDelete(wavFileName);
        }
    }

    /// <summary>
    /// A render-free core, like the text-to-speech preview players: null video/audio outputs so
    /// nothing opens a window or touches an audio device, but the clock still runs in real time.
    /// </summary>
    private static LibMpvDynamicPlayer? CreatePlayer()
    {
        var player = new LibMpvDynamicPlayer();
        if (!player.CanLoad())
        {
            return null;
        }

        player.SetOptionString("vo", "null");
        player.SetOptionString("ao", "null");
        player.Initialize();
        return player;
    }

    private static async Task<bool> WaitUntilAsync(Func<bool> condition, int timeoutMs)
    {
        var end = Environment.TickCount64 + timeoutMs;
        while (Environment.TickCount64 < end)
        {
            if (condition())
            {
                return true;
            }

            await Task.Delay(25, TestContext.Current.CancellationToken);
        }

        return condition();
    }

    private static string WriteTempWav()
    {
        const int sampleRate = 8000;
        var dataSize = sampleRate * WavSeconds * 2;
        var path = Path.Combine(Path.GetTempPath(), $"se-mpv-eventloop-{Guid.NewGuid():N}.wav");
        using var writer = new BinaryWriter(File.Create(path));
        writer.Write("RIFF"u8);
        writer.Write(36 + dataSize);
        writer.Write("WAVE"u8);
        writer.Write("fmt "u8);
        writer.Write(16);
        writer.Write((short)1); // PCM
        writer.Write((short)1); // mono
        writer.Write(sampleRate);
        writer.Write(sampleRate * 2); // byte rate
        writer.Write((short)2); // block align
        writer.Write((short)16); // bits per sample
        writer.Write("data"u8);
        writer.Write(dataSize);
        for (var i = 0; i < dataSize / 2; i++)
        {
            writer.Write((short)(Math.Sin(i * 0.05) * 2000)); // quiet tone rather than digital silence
        }

        return path;
    }

    private static void TryDelete(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch
        {
            // a temp file left behind must not fail the test
        }
    }
}
