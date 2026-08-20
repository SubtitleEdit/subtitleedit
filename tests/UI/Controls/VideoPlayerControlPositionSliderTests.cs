using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.VideoPlayers;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;
using Xunit;

namespace UITests.Controls;

/// <summary>
/// Covers the position slider jumping under the mouse during playback (issue #13910). The 50 ms
/// position timer publishes the player position into the control, which the slider is bound to.
/// A seek does not land instantly, so mid-drag the timer kept writing the pre-seek position back
/// and yanked the thumb off the mouse until the next mouse move pulled it forward again. Pausing
/// hid it: a paused player reports the seeked-to position straight away.
/// </summary>
public class VideoPlayerControlPositionSliderTests
{
    /// <summary>
    /// A player whose reported position lags behind the seeks asked of it - what every real
    /// player does, and the whole reason the timer and the drag can disagree.
    /// </summary>
    private sealed class SeekLaggingVideoPlayer : IVideoPlayer
    {
        private double _reported;

        public SeekLaggingVideoPlayer(double reported) => _reported = reported;

        public double LastSeekTo { get; private set; } = -1;

        /// <summary>Moves the position the player reports, as playback or a completed seek would.</summary>
        public void ReportPosition(double seconds) => _reported = seconds;

        public string Name => "seek-lagging";
        public string FileName { get; private set; } = string.Empty;
        public bool CanLoad() => true;

        public Task LoadFile(string fileName, double startPositionSeconds = 0)
        {
            FileName = fileName;
            return Task.CompletedTask;
        }

        public void CloseFile() => FileName = string.Empty;
        public void Play() { }
        public void PlayOrPause() { }
        public void Pause() { }
        public void Stop() { }
        public AudioTrackInfo? ToggleAudioTrack() => null;
        public bool IsPlaying => true;
        public bool IsPaused => false;

        public double Position
        {
            get => _reported;
            set => LastSeekTo = value; // seek requested; the reported position catches up later
        }

        public double Duration => 600;
        public int VolumeMaximum => 100;
        public double Volume { get; set; } = 50;
        public double Speed { get; set; } = 1.0;
    }

    private static Slider FindPositionSlider(VideoPlayerControl control)
    {
        return control.GetLogicalDescendants()
            .OfType<Slider>()
            .First(s => Equals(AutomationProperties.GetName(s), Se.Language.General.VideoPosition));
    }

    private static void PressPointerOn(VideoPlayerControl control, Slider slider)
    {
        slider.RaiseEvent(new PointerPressedEventArgs(
            slider,
            new Pointer(1, PointerType.Mouse, true),
            control,
            new Point(0, 0),
            0,
            new PointerPointProperties(RawInputModifiers.LeftMouseButton, PointerUpdateKind.LeftButtonPressed),
            KeyModifiers.None));
    }

    private static void ReleasePointerOn(VideoPlayerControl control, Slider slider)
    {
        slider.RaiseEvent(new PointerReleasedEventArgs(
            slider,
            new Pointer(1, PointerType.Mouse, true),
            control,
            new Point(0, 0),
            0,
            new PointerPointProperties(RawInputModifiers.None, PointerUpdateKind.LeftButtonReleased),
            KeyModifiers.None,
            MouseButton.Left));
    }

    [AvaloniaFact]
    public async Task PositionTimerLeavesTheSliderAloneWhileTheUserDragsIt()
    {
        var player = new SeekLaggingVideoPlayer(reported: 5);
        var control = new VideoPlayerControl(player);
        await control.Open("fake.mkv");
        await Task.Delay(150); // let the timer publish the duration and the starting position

        var slider = FindPositionSlider(control);
        PressPointerOn(control, slider);
        slider.Value = 300; // the user drags the thumb; the seek has not landed yet

        await Task.Delay(200); // several 50 ms timer ticks

        Assert.Equal(300, slider.Value, 3);
        Assert.Equal(300, player.LastSeekTo, 3); // and the drag still seeks
    }

    [AvaloniaFact]
    public async Task PositionTimerTakesOverAgainOnceTheDragEnds()
    {
        var player = new SeekLaggingVideoPlayer(reported: 5);
        var control = new VideoPlayerControl(player);
        await control.Open("fake.mkv");
        await Task.Delay(150);

        var slider = FindPositionSlider(control);
        PressPointerOn(control, slider);
        slider.Value = 300;
        ReleasePointerOn(control, slider);

        player.ReportPosition(301); // playback carries on from where the user dropped the thumb
        await Task.Delay(200);

        Assert.Equal(301, slider.Value, 3);
    }
}
