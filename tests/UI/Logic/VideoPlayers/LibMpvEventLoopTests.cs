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
    /// Two-tier scrub seeking (#14441): a burst of seeks serves all but its first two at keyframes
    /// and must then pay exactly one exact landing once it settles. The follow-up used to be
    /// skipped when mpv's cached position looked close to the target - and it always did, because
    /// mpv reports the seek target as time-pos while seeking - so the video stayed a GOP short.
    /// </summary>
    [Fact]
    public async Task SeekBurst_PaysOneExactLandingOnceItSettles()
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

            // A drag: seeks back to back until one is issued into an unfinished seek that was
            // itself issued into an unfinished seek - a burst - and so is served at keyframes and
            // owes an exact landing. On this tiny file mpv can land a seek in well under a
            // millisecond, so how many it takes varies; a real drag delivers hundreds.
            var target = 0.0;
            for (var i = 0; i < 500 && !player.OwesExactLanding; i++)
            {
                target = 0.2 + (i % 8) * 0.2;
                player.Position = target;
            }

            Assert.True(player.OwesExactLanding, "no seek burst could be formed - mpv landed every seek before the next one was issued");
            var issued = player.IssuedSeekCount;

            // The burst settles: the follow-up goes out as one more seek and clears the debt.
            Assert.True(await WaitUntilAsync(() => !player.OwesExactLanding, 5000),
                "the burst settled but the exact landing was never paid");
            Assert.Equal(issued + 1, player.IssuedSeekCount);

            // And it lands where the user pointed, with nothing left owing and no further seeks.
            await Task.Delay(300, TestContext.Current.CancellationToken);
            Assert.False(player.OwesExactLanding);
            Assert.Equal(issued + 1, player.IssuedSeekCount);
            Assert.Equal(target, player.Position, 2);
        }
        finally
        {
            player.Dispose();
            TryDelete(wavFileName);
        }
    }

    /// <summary>
    /// The waveform click seeks twice in one input event (pointer release, then the tap with the
    /// frame-snapped position). That pair is not a burst: both seeks stay exact, so no keyframe
    /// frame flashes before the landing and nothing is left owing.
    /// </summary>
    [Fact]
    public async Task ClickSeekPair_StaysExactAndOwesNothing()
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

            player.Position = 1.3;
            player.Pause();
            player.Position = 1.32;

            Assert.Equal(2, player.IssuedSeekCount);
            Assert.False(player.OwesExactLanding, "the second seek of a click must not be a keyframe seek");

            await Task.Delay(500, TestContext.Current.CancellationToken);
            Assert.Equal(2, player.IssuedSeekCount);
            Assert.False(player.OwesExactLanding);
            Assert.Equal(1.32, player.Position, 2);
        }
        finally
        {
            player.Dispose();
            TryDelete(wavFileName);
        }
    }

    /// <summary>
    /// Frame mode used to skip the paused seek-target cache, so after a waveform click the
    /// reported position hopped from the click to the first decoded frame at or after it - a
    /// visible one-frame jump forward on every click, in the mode SE forces on for EBU STL
    /// (#14441). The seek target is where the user pointed, in either mode.
    /// </summary>
    [Fact]
    public async Task PausedSeek_InFrameMode_KeepsReportingTheSeekTarget()
    {
        var player = CreatePlayer();
        if (player == null)
        {
            return; // no libmpv on this machine
        }

        var wavFileName = WriteTempWav();
        var originalOverride = Nikse.SubtitleEdit.Logic.Config.Se.Settings.General.UseFrameModeOverride;
        Nikse.SubtitleEdit.Logic.Config.Se.Settings.General.UseFrameModeOverride = true;
        try
        {
            await player.LoadFile(wavFileName);
            Assert.True(await WaitUntilAsync(() => player.Duration > 1.0, 5000), "file never loaded");
            Assert.True(player.IsPaused);

            // A click on the waveform while paused: a target that is not on a frame boundary.
            player.Position = 1.2345;

            var end = Environment.TickCount64 + 800;
            while (Environment.TickCount64 < end)
            {
                Assert.Equal(1.2345, player.Position, 4);
                await Task.Delay(40, TestContext.Current.CancellationToken);
            }
        }
        finally
        {
            Nikse.SubtitleEdit.Logic.Config.Se.Settings.General.UseFrameModeOverride = originalOverride;
            player.Dispose();
            TryDelete(wavFileName);
        }
    }

    /// <summary>
    /// A render-free core, like the text-to-speech preview players: null video/audio outputs so
    /// nothing opens a window or touches an audio device, but the clock still runs in real time.
    /// </summary>
    /// <summary>
    /// #14523: the report behind it ("cursor and time display freeze for a second, worst after
    /// pause/resume") was mpv's "Audio device underrun detected." - a warning mpv printed and SE
    /// threw away. The event loop now forwards mpv's warnings and errors to the error log, so the
    /// next such report carries the core's own diagnosis. A file that cannot be opened is the
    /// simplest reliable way to make the core log at error level.
    /// </summary>
    [Fact]
    public async Task EventLoop_ForwardsMpvWarningsAndErrors()
    {
        var player = CreatePlayer();
        if (player == null)
        {
            return; // no libmpv on this machine
        }

        try
        {
            // A video extension: for an audio extension LoadFile polls the duration for up to
            // 2.5 s after the (failed) open, which is all this test used to spend.
            var missing = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".mp4");
            try
            {
                await player.LoadFile(missing);
            }
            catch
            {
                // a failed open may surface as an exception on some builds; the log line is what counts
            }

            Assert.True(await WaitUntilAsync(() => player.ForwardedMpvLogMessageCount > 0, 5000),
                "mpv's error for the missing file never reached the log forwarder");
            Assert.StartsWith("mpv [", player.LastForwardedMpvLogMessage);
        }
        finally
        {
            player.Dispose();
        }
    }

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
