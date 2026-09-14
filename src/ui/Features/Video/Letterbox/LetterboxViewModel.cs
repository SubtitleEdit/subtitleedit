using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;

namespace Nikse.SubtitleEdit.Features.Video.Letterbox;

/// <summary>
/// Video menu > Letterboxing... (#14845): top/bottom black bars drawn over the already-open
/// video preview, purely as a visual overlay - the video file itself is never touched.
///
/// This dialog does not open its own video player (unlike BurnInLogoWindow's overlay-drag
/// approach). It drives the SAME player instance the main window already has open - passed in
/// via <see cref="Initialize"/> - which ShowDialogAsync has already paused for the duration of
/// the dialog, so "live preview while adjusting" means literally watching the real editing
/// preview's bars grow and shrink behind this window, with whatever subtitle is already
/// showing rendered on top of them (the actual use case in #14845's own description: covering
/// hardcoded subtitles already burned into the source video).
/// </summary>
public partial class LetterboxViewModel : ObservableObject
{
    [ObservableProperty] private bool _enabled;
    [ObservableProperty] private double _topHeightPercent;
    [ObservableProperty] private double _bottomHeightPercent;

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
        _initialized = true;
    }

    private void LoadSettings()
    {
        var settings = Se.Settings.Video.Letterbox;
        Enabled = settings.Enabled;
        TopHeightPercent = settings.TopHeightPercent;
        BottomHeightPercent = settings.BottomHeightPercent;

        // So Cancel can put the live preview back exactly as it was before this dialog opened.
        _snapshotEnabled = Enabled;
        _snapshotTop = TopHeightPercent;
        _snapshotBottom = BottomHeightPercent;
    }

    partial void OnEnabledChanged(bool value) => ApplyLive();

    partial void OnTopHeightPercentChanged(double value) => ApplyLive();

    partial void OnBottomHeightPercentChanged(double value) => ApplyLive();

    /// <summary>
    /// Pushes the current in-memory values to both the settings object (read by
    /// <see cref="LibMpvDynamicPlayer.ApplyLetterboxRibbon"/>) and the live player, so every
    /// slider tick is visible immediately - this is what makes the dialog's preview "live"
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

        if (_videoPlayerControl?.VideoPlayer is LibMpvDynamicPlayer mpv)
        {
            mpv.ApplyLetterboxRibbon();
        }
    }

    [RelayCommand]
    private void Apply()
    {
        Se.SaveSettings();
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
        Enabled = _snapshotEnabled;
        TopHeightPercent = _snapshotTop;
        BottomHeightPercent = _snapshotBottom;
        Window?.Close();
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
