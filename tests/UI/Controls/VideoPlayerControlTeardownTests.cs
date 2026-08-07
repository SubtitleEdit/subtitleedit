using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Logic.VideoPlayers;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace UITests.Controls;

/// <summary>
/// Covers the teardown leak behind issue #13048: Options/OK rebuilds the layout on any
/// setting change, and the discarded <see cref="VideoPlayerControl"/> used to keep its
/// 50 ms position timer running (rooted in the dispatcher, so it polled a dead player
/// from the UI thread forever) and never destroyed the native player core. One leaked
/// player and poller per OK is what made the waveform playhead stutter until restart.
/// </summary>
public class VideoPlayerControlTeardownTests
{
    private sealed class FakeVideoPlayer : IVideoPlayer, IDisposable
    {
        public int DisposeCount;
        public int CloseFileCount;

        public string Name => "fake";
        public string FileName { get; private set; } = string.Empty;

        public bool CanLoad() => true;

        public Task LoadFile(string fileName, double startPositionSeconds = 0)
        {
            FileName = fileName;
            return Task.CompletedTask;
        }

        public void CloseFile()
        {
            CloseFileCount++;
            FileName = string.Empty;
        }

        public void Play()
        {
        }

        public void PlayOrPause()
        {
        }

        public void Pause()
        {
        }

        public void Stop()
        {
        }

        public AudioTrackInfo? ToggleAudioTrack() => null;

        public bool IsPlaying => false;
        public bool IsPaused => true;
        public double Position { get; set; }
        public double Duration => 60;
        public int VolumeMaximum => 100;
        public double Volume { get; set; } = 50;
        public double Speed { get; set; } = 1.0;

        public void Dispose() => DisposeCount++;
    }

    private static DispatcherTimer? GetPositionTimer(VideoPlayerControl control) =>
        (DispatcherTimer?)typeof(VideoPlayerControl)
            .GetField("_positionTimer", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(control);

    private static async Task<VideoPlayerControl> MakeOpenedControlAsync(FakeVideoPlayer player)
    {
        var control = new VideoPlayerControl(player);
        await control.Open("fake.mkv");
        return control;
    }

    [AvaloniaFact]
    public async Task OpenStartsThePositionTimer()
    {
        var control = await MakeOpenedControlAsync(new FakeVideoPlayer());

        Assert.True(GetPositionTimer(control)?.IsEnabled);
    }

    [AvaloniaFact]
    public async Task CloseAndDisposePlayerStopsThePositionTimer()
    {
        var control = await MakeOpenedControlAsync(new FakeVideoPlayer());

        control.CloseAndDisposePlayer();

        // A running DispatcherTimer keeps the control (and through it the player) alive for
        // the rest of the session, so leaving it enabled is the leak, not just wasted work.
        Assert.False(GetPositionTimer(control)?.IsEnabled);
    }

    [AvaloniaFact]
    public async Task CloseAndDisposePlayerDetachesTheRenderHost()
    {
        var control = await MakeOpenedControlAsync(new FakeVideoPlayer());

        control.CloseAndDisposePlayer();

        // Dropping the content is what destroys the embedded native window; it has to happen
        // before the core is destroyed, while the player is already stopped.
        Assert.Null(control.Content);
    }

    [AvaloniaFact]
    public async Task CloseAndDisposePlayerDisposesThePlayer()
    {
        var player = new FakeVideoPlayer();
        var control = await MakeOpenedControlAsync(player);

        control.CloseAndDisposePlayer();

        // The dispose runs on a worker thread on purpose (mpv_terminate_destroy blocks until
        // every worker has exited - see issue #11176), so give it a moment to land.
        await WaitForAsync(() => player.DisposeCount > 0);
        Assert.Equal(1, player.DisposeCount);
        Assert.True(player.CloseFileCount > 0);
    }

    [AvaloniaFact]
    public async Task CloseAndDisposePlayerIsSafeToRepeat()
    {
        var player = new FakeVideoPlayer();
        var control = await MakeOpenedControlAsync(player);

        control.CloseAndDisposePlayer();
        await WaitForAsync(() => player.DisposeCount > 0);

        // Some paths (a control detaching itself, then its owner tearing it down) can reach
        // this twice; the player's own Dispose is the guard, this must not throw.
        control.CloseAndDisposePlayer();

        Assert.Null(control.Content);
        Assert.False(GetPositionTimer(control)?.IsEnabled);
    }

    [AvaloniaFact]
    public async Task CloseLeavesThePlayerUsable()
    {
        var player = new FakeVideoPlayer();
        var control = await MakeOpenedControlAsync(player);

        // Close() is the "closed the video file, control stays" path (Video > Close video),
        // so it must keep the player alive for the next Open.
        control.Close();

        Assert.Equal(0, player.DisposeCount);
        Assert.NotNull(control.Content);

        await control.Open("other.mkv");
        Assert.Equal("other.mkv", player.FileName);
        Assert.True(GetPositionTimer(control)?.IsEnabled);
    }

    private static async Task WaitForAsync(Func<bool> condition, int timeoutMs = 5000)
    {
        var waited = 0;
        while (!condition() && waited < timeoutMs)
        {
            await Task.Delay(10);
            waited += 10;
        }
    }
}
