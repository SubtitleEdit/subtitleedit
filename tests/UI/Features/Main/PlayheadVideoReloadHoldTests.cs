using Avalonia.Headless.XUnit;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.VideoPlayers;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace UITests.Features.Main;

/// <summary>
/// The waveform playhead while the video player is being reloaded (issue #15027). A settings
/// change, dock/undock and fullscreen all throw the player away and build a new one, and a player
/// that is still loading reports 0 - so the cursor, which follows the player, jumped to the start
/// of the waveform and back again once the restore seek landed. In center mode the whole waveform
/// scrolled with it, and "select current subtitle" moved the grid selection to the first line.
/// </summary>
public class PlayheadVideoReloadHoldTests : IDisposable
{
    // See PlayheadFrameStepTests: a test-built VideoPlayerControl must not stay assigned to the
    // view model on the per-assembly headless application (PR #14258).
    private MainViewModel? _vm;

    public void Dispose()
    {
        if (_vm != null)
        {
            _vm.VideoPlayerControl = null;
            _vm.AudioVisualizer = null;
        }
    }

    /// <summary>A player that comes up at 0 and ignores seeks until the test says it has loaded.</summary>
    private sealed class LoadingVideoPlayer : IVideoPlayer
    {
        private double _position;

        public bool IsLoaded { get; set; }

        public string Name => "loading";
        public string FileName { get; private set; } = string.Empty;

        public bool CanLoad() => true;

        public Task LoadFile(string fileName, double startPositionSeconds = 0)
        {
            FileName = fileName;
            _position = 0;
            return Task.CompletedTask;
        }

        public void CloseFile() => FileName = string.Empty;

        public void Play() => IsPlaying = true;

        public void PlayOrPause() => IsPlaying = !IsPlaying;

        public void Pause() => IsPlaying = false;

        public void Stop() => IsPlaying = false;

        public AudioTrackInfo? ToggleAudioTrack() => null;

        public bool IsPlaying { get; set; }
        public bool IsPaused => !IsPlaying;

        public double Position
        {
            get => _position;
            set
            {
                if (IsLoaded)
                {
                    _position = value;
                }
            }
        }

        public double Duration => 2595;
        public int VolumeMaximum => 100;
        public double Volume { get; set; } = 50;
        public double Speed { get; set; } = 1.0;
    }

    [AvaloniaFact]
    public void TheCursorStaysPutWhileTheReloadedPlayerStillReportsZero()
    {
        var (vm, vp, player) = MakeViewModelWithReloadingPlayer(restoreSeconds: 54);

        // Loading: the player says 0. Following it is the jump to the start of the waveform.
        Assert.Equal(0, player.Position, 4);
        Assert.Equal(54, Tick(vm, vp, isPlaying: false), 4);
        Assert.Equal(54, Tick(vm, vp, isPlaying: false), 4);

        // mpv's default is pause=no, so a load can read as playing for a moment - still held.
        Assert.Equal(54, Tick(vm, vp, isPlaying: true), 4);

        // The file is up and the restore seek lands: tracking resumes where the cursor already is.
        player.IsLoaded = true;
        vp.SeekTo(54);
        vp.EndPositionRestoreIfArrived();
        Assert.Null(vp.PositionRestoreHoldSeconds);
        Assert.Equal(54, Tick(vm, vp, isPlaying: false), 4);
    }

    [AvaloniaFact]
    public void ASeekDuringTheReloadMovesTheHeldCursor()
    {
        var (vm, vp, _) = MakeViewModelWithReloadingPlayer(restoreSeconds: 54);
        Assert.Equal(54, Tick(vm, vp, isPlaying: false), 4);

        // The user clicks somewhere else before the player is back: the cursor goes there, not
        // to the 0 the player reports and not back to the old spot.
        vp.SeekTo(120);

        Assert.Equal(120, Tick(vm, vp, isPlaying: false), 4);
    }

    [AvaloniaFact]
    public async Task ARestoreThatGivesUpHandsTheCursorBackToThePlayer()
    {
        var (vm, vp, player) = MakeViewModelWithReloadingPlayer(restoreSeconds: 54);
        Assert.Equal(54, Tick(vm, vp, isPlaying: false), 4);

        await vp.RestorePositionAsync(54, 100); // never lands: the player swallows every seek

        // Held any longer, the cursor would sit frozen through whatever the player does next.
        Assert.Equal(player.Position, Tick(vm, vp, isPlaying: false), 4);
    }

    private (MainViewModel Vm, VideoPlayerControl Vp, LoadingVideoPlayer Player) MakeViewModelWithReloadingPlayer(double restoreSeconds)
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        Locator.Services = services.BuildServiceProvider();

        var vm = Locator.Services.GetRequiredService<MainViewModel>();
        var player = new LoadingVideoPlayer();
        var vp = new VideoPlayerControl(player);

        vm.VideoPlayerControl = vp;
        _vm = vm; // detached again in Dispose - see the note on the field
        SetField(vm, "_videoFileName", "video.mkv");

        // Where the view model stood before the rebuild: paused and settled on restoreSeconds.
        SetField(vm, "_playheadEstimateSeconds", restoreSeconds);
        SetField(vm, "_playheadLastRealSeconds", restoreSeconds);
        SetField(vm, "_playheadValid", true);
        SetField(vm, "_playheadPausedSettled", true);
        SetField(vm, "_playheadWasPlaying", false);

        // What the layout rebuild does before it opens the file in the fresh control.
        vp.BeginPositionRestore(restoreSeconds);

        return (vm, vp, player);
    }

    private static double Tick(MainViewModel vm, VideoPlayerControl vp, bool isPlaying) =>
        (double)typeof(MainViewModel)
            .GetMethod("UpdatePlayheadEstimate", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(vm, [vp, isPlaying])!;

    private static void SetField(object target, string name, object value) =>
        target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(target, value);
}
