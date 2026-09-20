using Nikse.SubtitleEdit.Logic.VideoPlayers.Ffmpeg;
using System.Diagnostics;

namespace UITests.Logic;

/// <summary>
/// Drives the real decode pipeline of <see cref="FfmpegPlayer"/> against small generated clips:
/// the frame index, exact seeks, frame stepping and scrub bursts. Needs the FFmpeg shared
/// libraries the player loads and an ffmpeg executable to make the clips, so it skips itself
/// where either is missing (CI).
/// </summary>
public sealed class FfmpegPlayerLiveTests : IDisposable
{
    private const double FrameSeconds = 1001 / 24000.0;

    private static readonly Lock ClipLock = new();
    private static string? _clipFolder;
    private static string? _clipError;

    private readonly FfmpegPlayer _player = new();

    public void Dispose()
    {
        _player.Dispose();
    }

    /// <summary>23.976 fps, B-frames, key frames 250 pictures apart (0 s, 10.4 s, 20.9 s), with audio.</summary>
    private static string ConstantRateClip => Path.Combine(Clips(), "cfr.mp4");

    /// <summary>30 fps with two of every five pictures removed: frames at 0, 33, 67, 167, 200, 233, 333 ms ...</summary>
    private static string VariableRateClip => Path.Combine(Clips(), "vfr.mkv");

    private static string Clips()
    {
        lock (ClipLock)
        {
            if (_clipFolder == null && _clipError == null)
            {
                MakeClips();
            }

            if (_clipError != null)
            {
                Assert.Skip(_clipError);
            }

            return _clipFolder!;
        }
    }

    private static void MakeClips()
    {
        if (!FfmpegLibraries.IsAvailable())
        {
            _clipError = $"FFmpeg {FfmpegLibraries.MajorVersion} shared libraries not found.";
            return;
        }

        var ffmpeg = FindFfmpeg();
        if (ffmpeg == null)
        {
            _clipError = "No ffmpeg executable found to generate the test clips.";
            return;
        }

        var folder = Path.Combine(Path.GetTempPath(), "se-ffmpeg-player-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        var ok = Run(ffmpeg, folder,
                     "-v error -y -f lavfi -i testsrc2=size=320x180:rate=24000/1001:duration=30 -f lavfi -i sine=frequency=440:duration=30 " +
                     "-c:v libx264 -preset ultrafast -g 250 -keyint_min 250 -sc_threshold 0 -bf 3 -pix_fmt yuv420p -c:a aac -shortest cfr.mp4") &&
                 Run(ffmpeg, folder,
                     "-v error -y -f lavfi -i testsrc2=size=320x180:rate=30:duration=20 -vf select='lt(mod(n\\,5)\\,3)' " +
                     "-c:v libx264 -preset ultrafast -g 120 -bf 2 -pix_fmt yuv420p vfr.mkv");
        if (!ok)
        {
            _clipError = $"'{ffmpeg}' could not generate the test clips (no libx264?).";
            return;
        }

        _clipFolder = folder;
        AppDomain.CurrentDomain.ProcessExit += (_, _) =>
        {
            try
            {
                Directory.Delete(folder, true);
            }
            catch
            {
                // best effort
            }
        };
    }

    private static string? FindFfmpeg()
    {
        var name = OperatingSystem.IsWindows() ? "ffmpeg.exe" : "ffmpeg";
        var folders = new List<string> { "/opt/homebrew/bin", "/usr/local/bin" };
        folders.AddRange((Environment.GetEnvironmentVariable("PATH") ?? string.Empty).Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries));
        return folders.Select(folder => Path.Combine(folder, name)).FirstOrDefault(File.Exists);
    }

    private static bool Run(string fileName, string workingFolder, string arguments)
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo(fileName, arguments)
            {
                WorkingDirectory = workingFolder,
                UseShellExecute = false,
                CreateNoWindow = true,
            });
            return process != null && process.WaitForExit(120_000) && process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    private static bool WaitFor(Func<bool> condition, int timeoutMs = 10_000)
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

    /// <summary>Loads the clip paused at <paramref name="startSeconds"/> and waits for the first picture and the frame index.</summary>
    private FfmpegFrameIndex Load(string clip, double startSeconds = 0)
    {
        var started = Stopwatch.GetTimestamp();
        _player.LoadFile(clip, startSeconds).Wait(TestContext.Current.CancellationToken);
        Assert.True(WaitFor(() => _player.HasPlaybackRestartedSince(started)), "the first picture never arrived");
        Assert.True(WaitFor(() => _player.FrameIndex != null), "the frame index never arrived");
        return _player.FrameIndex!;
    }

    private void LoadWithoutIndex(string clip)
    {
        var started = Stopwatch.GetTimestamp();
        _player.LoadFile(clip).Wait(TestContext.Current.CancellationToken);
        Assert.True(WaitFor(() => _player.HasPlaybackRestartedSince(started)), "the first picture never arrived");
        Assert.Null(_player.FrameIndex);
    }

    private void SeekAndWait(double seconds)
    {
        var started = Stopwatch.GetTimestamp();
        _player.Position = seconds;
        Assert.True(WaitFor(() => _player.HasPlaybackRestartedSince(started)), $"the seek to {seconds} never landed");
    }

    private void AssertPosition(double expected, string what)
    {
        Assert.True(WaitFor(() => Math.Abs(_player.Position - expected) < 0.0005, 5_000),
            $"{what}: expected {expected:0.0000}, position is {_player.Position:0.0000}");
    }

    [Fact]
    public void FrameIndex_ConstantRate_HasTheRealFrameTimes()
    {
        var index = Load(ConstantRateClip);

        Assert.InRange(index.Count, 715, 722);
        Assert.True(index.IsConstantFrameRate);
        Assert.Equal(0, index.SecondsAt(0), 4);
        Assert.Equal(300 * FrameSeconds, index.SecondsAt(300), 4);
        var keyFrames = index.KeyFrameSeconds();
        Assert.Equal(3, keyFrames.Length);
        Assert.Equal(0, keyFrames[0], 3);
        Assert.Equal(250 * FrameSeconds, keyFrames[1], 3);
        Assert.Equal(500 * FrameSeconds, keyFrames[2], 3);
    }

    [Fact]
    public void FrameIndex_VariableRate_HasTheGaps()
    {
        var index = Load(VariableRateClip);

        Assert.False(index.IsConstantFrameRate);
        Assert.Equal(0.067, index.SecondsAt(2), 3);
        Assert.Equal(0.167, index.SecondsAt(3), 3);
    }

    [Theory]
    [InlineData(12.5)]
    [InlineData(10.4)] // a few pictures before the key frame at 10.427: must come from the GOP before it
    [InlineData(10.43)]
    [InlineData(29.0)]
    [InlineData(0.0)]
    public void Seek_LandsOnTheNearestRealFrame(double target)
    {
        var index = Load(ConstantRateClip);

        SeekAndWait(target);

        AssertPosition(index.SecondsAt(index.NearestIndex(target)), $"seek to {target}");
    }

    /// <summary>Until the scan of a big file is done the player works from the average frame rate; on a constant rate file that must give the same frames.</summary>
    [Theory]
    [InlineData(12.5)]
    [InlineData(10.4)]
    [InlineData(10.42)]
    [InlineData(10.43)]
    [InlineData(20.84)]
    [InlineData(0.0)]
    public void Seek_WithoutFrameIndex_LandsOnTheNearestFrame(double target)
    {
        _player.UseFrameIndex = false;
        LoadWithoutIndex(ConstantRateClip);

        SeekAndWait(target);

        AssertPosition(Math.Round(target / FrameSeconds, MidpointRounding.AwayFromZero) * FrameSeconds, $"seek to {target}");
    }

    [Fact]
    public void Step_WithoutFrameIndex_MovesOneFrame()
    {
        _player.UseFrameIndex = false;
        LoadWithoutIndex(ConstantRateClip);
        SeekAndWait(15.0);
        var landed = (int)Math.Round(15.0 / FrameSeconds, MidpointRounding.AwayFromZero);

        for (var i = 1; i <= 20; i++)
        {
            _player.StepOneFrameBack();
            AssertPosition((landed - i) * FrameSeconds, $"step back {i}");
        }

        _player.StepOneFrameForward();
        AssertPosition((landed - 19) * FrameSeconds, "step forward");
    }

    [Fact]
    public void StepBackAndForward_AroundASeek_NeedsNoFurtherSeek()
    {
        var index = Load(ConstantRateClip);
        SeekAndWait(15.0);
        var landed = index.NearestIndex(15.0);
        var seeks = _player.SeeksPerformed;

        for (var i = 1; i <= 5; i++)
        {
            _player.StepOneFrameBack();
            AssertPosition(index.SecondsAt(landed - i), $"step back {i}");
        }

        for (var i = 4; i >= -3; i--)
        {
            _player.StepOneFrameForward();
            AssertPosition(index.SecondsAt(landed - i), $"step forward to {-i}");
        }

        Assert.Equal(seeks, _player.SeeksPerformed);
    }

    [Fact]
    public void StepBack_PastTheHistory_SeeksOnceAndRefills()
    {
        var index = Load(ConstantRateClip);
        SeekAndWait(15.0);
        var landed = index.NearestIndex(15.0);
        var seeks = _player.SeeksPerformed;

        const int steps = 40;
        for (var i = 1; i <= steps; i++)
        {
            _player.StepOneFrameBack();
            AssertPosition(index.SecondsAt(landed - i), $"step back {i}");
        }

        // 16 pictures of history per seek at this size: 40 steps are a few refills, not 40 seeks.
        Assert.InRange(_player.SeeksPerformed - seeks, 1, 3);
    }

    [Fact]
    public void StepBack_AtTheFirstFrame_StaysThere()
    {
        var index = Load(ConstantRateClip);

        _player.StepOneFrameBack();
        Thread.Sleep(200);

        AssertPosition(index.SecondsAt(0), "step back at the start");
    }

    [Fact]
    public void Step_VariableRate_CrossesTheGapInOneStep()
    {
        var index = Load(VariableRateClip);
        SeekAndWait(0.067);
        AssertPosition(index.SecondsAt(2), "seek");

        _player.StepOneFrameForward();
        AssertPosition(index.SecondsAt(3), "forward over the gap"); // 0.167, not 0.067 + 1/avg fps

        _player.StepOneFrameBack();
        AssertPosition(index.SecondsAt(2), "back over the gap");
    }

    [Fact]
    public void StepBack_VariableRate_ByASeek_LandsOnThePreviousRealFrame()
    {
        var index = Load(VariableRateClip);
        SeekAndWait(10.0);
        var landed = index.NearestIndex(10.0);

        // Walk back further than the history holds, so some of the steps are seeks.
        for (var i = 1; i <= 25; i++)
        {
            _player.StepOneFrameBack();
            AssertPosition(index.SecondsAt(landed - i), $"step back {i}");
        }
    }

    [Fact]
    public void SeekBurst_EndsOnTheExactFrameOfTheLastTarget()
    {
        var index = Load(ConstantRateClip);
        SeekAndWait(1.0);

        // A drag: seeks arrive faster than they land. The ones mid-burst may be served at a key
        // frame, but the last target must end up exact - and the position must never report the
        // key frame it passed through.
        var targets = new[] { 14.0, 15.0, 16.0, 17.0, 18.0, 18.5, 18.7 };
        foreach (var target in targets)
        {
            _player.Position = target;
            var position = _player.Position;
            Assert.True(Math.Abs(position - target) < 0.0005, $"position {position} while seeking to {target}");
        }

        var expected = index.SecondsAt(index.NearestIndex(18.7));
        Assert.True(WaitFor(() => Math.Abs(_player.Position - expected) < 0.0005 && _player.Position != 18.7), $"ended at {_player.Position}, expected {expected}");

        Assert.True(_player.FastSeeksPerformed > 0, "no seek of the burst was served at a key frame");

        // And it is a real landing: stepping from it moves by exactly one frame.
        _player.StepOneFrameForward();
        AssertPosition(index.SecondsAt(index.NearestIndex(18.7) + 1), "step after the burst");
    }

    [Fact]
    public void SingleSeeks_AreNeverServedAtAKeyFrame()
    {
        Load(ConstantRateClip);

        // A waveform click seeks twice in one input event; that is not a burst.
        for (var i = 0; i < 5; i++)
        {
            var started = Stopwatch.GetTimestamp();
            _player.Position = 12 + i;
            _player.Position = 12.02 + i;
            Assert.True(WaitFor(() => _player.HasPlaybackRestartedSince(started)));
        }

        Assert.Equal(0, _player.FastSeeksPerformed);
    }

    [Fact]
    public void RandomOperations_DoNotWedgeThePlayer()
    {
        var index = Load(ConstantRateClip);
        _player.Volume = 0;
        var random = new Random(12345);
        var start = Stopwatch.GetTimestamp();
        while (Stopwatch.GetElapsedTime(start).TotalSeconds < 4)
        {
            switch (random.Next(7))
            {
                case 0:
                    _player.Position = random.NextDouble() * 29;
                    break;
                case 1:
                case 2:
                    _player.StepOneFrameBack();
                    break;
                case 3:
                case 4:
                    _player.StepOneFrameForward();
                    break;
                case 5:
                    _player.PlayOrPause();
                    break;
                default:
                    _ = _player.Position;
                    break;
            }

            Thread.Sleep(random.Next(0, 12));
        }

        _player.Pause();
        SeekAndWait(7.0);
        AssertPosition(index.SecondsAt(index.NearestIndex(7.0)), "seek after the storm");
        _player.StepOneFrameBack();
        AssertPosition(index.SecondsAt(index.NearestIndex(7.0) - 1), "step after the storm");
    }

    [Fact]
    public void Play_AfterStepping_StartsFromThePictureOnScreen()
    {
        var index = Load(ConstantRateClip);
        SeekAndWait(15.0);
        var landed = index.NearestIndex(15.0);
        for (var i = 1; i <= 4; i++)
        {
            _player.StepOneFrameBack();
            AssertPosition(index.SecondsAt(landed - i), $"step back {i}");
        }

        var from = _player.Position;
        _player.Volume = 0;
        _player.Play();
        Thread.Sleep(400);
        _player.Pause();

        Assert.InRange(_player.Position, from + 0.1, from + 1.5);
    }
}
