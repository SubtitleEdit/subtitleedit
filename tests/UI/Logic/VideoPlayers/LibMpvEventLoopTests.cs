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
