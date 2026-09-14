using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.VideoPlayers.Ffmpeg;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;

namespace Nikse.SubtitleEdit.Features.Video.Letterbox;

/// <summary>
/// Video menu > More > Letterboxing... (#14845): top/bottom black bars drawn over the already-open
/// video preview, purely as a visual overlay - the video file itself is never touched.
///
/// This dialog does not open its own video player (unlike BurnInLogoWindow's overlay-drag
/// approach). It drives the SAME player instance the main window already has open - passed in
/// via <see cref="Initialize"/> - which ShowDialogAsync has already paused for the duration of
/// the dialog, so "live preview while adjusting" means literally watching the real editing
/// preview's bars grow and shrink behind this window, with whatever subtitle is already
/// showing rendered on top of them (the actual use case in #14845's own description: covering
/// hardcoded subtitles already burned into the source video).
///
/// Only mpv and the ffmpeg player actually draw the bars (see ApplyLive). VLC has no equivalent
/// of mpv's "vf" filter chain reachable after libvlc_new, so <see cref="IsPlayerSupported"/>
/// disables the controls and the window shows an explanatory note instead of pretending to work.
/// </summary>
public partial class LetterboxViewModel : ObservableObject
{
    [ObservableProperty] private bool _enabled;
    [ObservableProperty] private double _topHeightPercent;
    [ObservableProperty] private double _bottomHeightPercent;
    [ObservableProperty] private bool _isPlayerSupported = true;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    private VideoPlayerControl? _videoPlayerControl;
    private bool _initialized;

    private bool _snapshotEnabled;
    private double _snapshotTop;
    private double _snapshotBottom;

    public LetterboxViewModel()
    {
        LoadSettings();
    }

    /// <summary>Called once, right after construction, with the already-open (and now paused) video player.</summary>
    public void Initialize(VideoPlayerControl? videoPlayerControl)
    {
        _videoPlayerControl = videoPlayerControl;
        IsPlayerSupported = videoPlayerControl?.VideoPlayer is LibMpvDynamicPlayer or FfmpegPlayer;
        _initialized = true;
    }

    private void LoadSettings()
    {
        var settings = Se.Settings.Video.Letterbox;
        Enabled = settings.Enabled;
        TopHeightPercent = settings.TopHeightPercent;
        BottomHeightPercent = settings.BottomHeightPercent;
        RefreshSnapshot();
    }

    /// <summary>
    /// Remembers the current values as what Cancel (or closing without OK) should revert to.
    /// Called on load, and again by Apply - otherwise Apply-then-Cancel would revert past the
    /// values Apply already committed to disk, leaving the settings file and the live preview
    /// disagreeing with each other.
    /// </summary>
    private void RefreshSnapshot()
    {
        _snapshotEnabled = Enabled;
        _snapshotTop = TopHeightPercent;
        _snapshotBottom = BottomHeightPercent;
    }

    partial void OnEnabledChanged(bool value) => ApplyLive();

    partial void OnTopHeightPercentChanged(double value) => ApplyLive();

    partial void OnBottomHeightPercentChanged(double value) => ApplyLive();

    /// <summary>
    /// Pushes the current in-memory values to both the settings object and the live player, so
    /// every slider tick is visible immediately - this is what makes the dialog's preview "live"
    /// without needing a second, dialog-owned video player.
    /// </summary>
    private void ApplyLive()
    {
        if (!_initialized)
        {
            return; // avoid pushing the constructor's initial LoadSettings() values as a spurious change
        }

        var settings = Se.Settings.Video.Letterbox;
        settings.Enabled = Enabled;
        settings.TopHeightPercent = TopHeightPercent;
        settings.BottomHeightPercent = BottomHeightPercent;

        switch (_videoPlayerControl?.VideoPlayer)
        {
            case LibMpvDynamicPlayer mpv:
                mpv.ApplyLetterboxRibbon();
                break;

            case FfmpegPlayer:
                // FfmpegSoftwareControl.Render reads Se.Settings.Video.Letterbox directly, but it
                // only repaints on a new decoded frame or a subtitle-text change - nothing tells it
                // to repaint when only the letterbox settings change, and ShowDialogAsync pauses
                // playback for the dialog's duration, so no new frame arrives to force one anyway.
                _videoPlayerControl?.PlayerContent?.InvalidateVisual();
                break;
        }
    }

    [RelayCommand]
    private void Apply()
    {
        Se.SaveSettings();
        RefreshSnapshot();
    }

    [RelayCommand]
    private void Ok()
    {
        Se.SaveSettings();
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        RevertIfNotConfirmed();
        Window?.Close();
    }

    /// <summary>
    /// Reverts to the last snapshot unless OK was pressed. Called directly by Cancel (so the
    /// revert happens even with no Window attached, e.g. in a unit test), and again from the
    /// window's Closing override for every OTHER close path (title-bar X, Cmd+W, Escape) that
    /// does not go through the Cancel command at all - calling it twice on the Cancel path is
    /// harmless, the second call finds nothing left to revert. ApplyLive() writes every slider
    /// tick straight into Se.Settings and the live player with nothing reverting it on its own -
    /// only OK is supposed to keep the dragged values. Without this, closing via the title-bar X
    /// left the player showing (and Se.Settings holding in memory) whatever was last dragged, and
    /// the next Se.SaveSettings() from anywhere would have persisted it.
    /// </summary>
    internal void RevertIfNotConfirmed()
    {
        if (OkPressed)
        {
            return;
        }

        Enabled = _snapshotEnabled;
        TopHeightPercent = _snapshotTop;
        BottomHeightPercent = _snapshotBottom;
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Cancel();
        }
    }
}
