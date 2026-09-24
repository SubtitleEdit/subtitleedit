using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.VideoPlayers;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;
using System;

namespace Nikse.SubtitleEdit.Features.Video.CutVideo;

/// <summary>
/// Plays the short clip "Preview transition" renders of one join.
/// </summary>
public partial class CutVideoPreviewViewModel : ObservableObject
{
    public Window? Window { get; set; }
    public VideoPlayerControl VideoPlayer { get; internal set; }

    private string _fileName;

    public CutVideoPreviewViewModel()
    {
        VideoPlayer = new VideoPlayerControl(new EmptyVideoPlayer());
        _fileName = string.Empty;
    }

    internal void Initialize(string fileName)
    {
        _fileName = fileName;
    }

    [RelayCommand]
    private void Ok()
    {
        Window?.Close();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
        else if (e.Key == Key.Space)
        {
            e.Handled = true;
            VideoPlayer.TogglePlayPause();
        }
    }

    internal void OnLoaded()
    {
        UiUtil.RestoreWindowPosition(Window);
        if (string.IsNullOrEmpty(_fileName))
        {
            return;
        }

        // A freshly built player is not ready when the window loads - playing right after the
        // open was lost, and the clip just sat on its first frame.
        Dispatcher.UIThread.Post(async () =>
        {
            try
            {
                await VideoPlayer.Open(_fileName);
                await VideoPlayer.WaitForPlayersReadyAsync();
                if (VideoPlayer.IsDisposed)
                {
                    return;
                }

                // The clip is only a few seconds; stopping on its last frame looked like the
                // player had frozen. Keep replaying the join until the window is closed.
                if (VideoPlayer.VideoPlayer is LibMpvDynamicPlayer mpv)
                {
                    mpv.SetOptionString("loop-file", "inf");
                }

                VideoPlayer.VideoPlayer.Play();
            }
            catch (Exception exception)
            {
                Se.LogError(exception, "Cut video transition preview: unable to play " + _fileName);
            }
        });
    }

    internal void OnClosing()
    {
        VideoPlayer.VideoPlayer.CloseFile();
        UiUtil.SaveWindowPosition(Window);
    }

    internal void OnClosed()
    {
        VideoPlayer.CloseAndDisposePlayer();
    }
}
