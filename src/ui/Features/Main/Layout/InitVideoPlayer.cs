using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.VideoPlayers;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;
using System;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Features.Main.Layout;

public static class InitVideoPlayer
{
    public static Grid MakeLayoutVideoPlayer(MainViewModel vm)
    {
        return MakeLayoutVideoPlayer(vm, out _);
    }

    public static Grid MakeLayoutVideoPlayer(MainViewModel vm, out VideoPlayerControl videoPlayerControl)
    {
        var mediaFile = string.Empty;
        double position = 0;
        if (vm.VideoPlayerControl != null)
        {
            mediaFile = vm.VideoPlayerControl.VideoPlayer.FileName;
            position = vm.VideoPlayerControl.VideoPlayer.Position;
            vm.VideoPlayerControl.VideoPlayer.CloseFile();
            vm.VideoPlayerControl.Content = null;
            vm.VideoPlayerControl = null;
        }

        var nonFullScreenMargin = new Thickness(0, 0, 8, 0);
        var mainGrid = new Grid
        {
            RowDefinitions = new RowDefinitions("*"),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Margin = nonFullScreenMargin,
        };

        DragDrop.SetAllowDrop(mainGrid, true);
        mainGrid.AddHandler(DragDrop.DragOverEvent, vm.VideoOnDragOver, RoutingStrategies.Bubble);
        mainGrid.AddHandler(DragDrop.DropEvent, vm.VideoOnDrop, RoutingStrategies.Bubble);

        var control = MakeVideoPlayer();
        control.IsFullScreenChanged += isFullScreen =>
        {
            mainGrid.Margin = isFullScreen ? new Thickness(0) : nonFullScreenMargin;
        };
        if (!string.IsNullOrEmpty(mediaFile))
        {
            Dispatcher.UIThread.Post(async () =>
            {
                await control.Open(mediaFile);
                await control.WaitForPlayersReadyAsync();
                for (var i = 0; i < 10; i++)
                {
                    await System.Threading.Tasks.Task.Delay(10);
                    control.Position = position;
                }
            });
        }

        control.FullScreenCommand = vm.VideoFullScreenCommand;
        videoPlayerControl = control;
        vm.VideoPlayerControl = control;
        control.Volume = Se.Settings.Video.Volume;
        control.VideoPlayerDisplayTimeLeft = Se.Settings.Video.VideoPlayerDisplayTimeLeft;
        control.VolumeChanged += v => { Se.Settings.Video.Volume = v; };
        control.ToggleDisplayProgressTextModeRequested += () => { vm.ToggleVideoPlayerDisplayTimeLeftCommand.Execute(null); };
        control.VideoFileNamePointerPressed += vm.VideoPlayerControlPointerPressed;
        control.SurfacePointerPressed += (_, _) => vm.VideoPlayerAreaPointerPressed();

        Grid.SetRow(control, 0);
        mainGrid.Children.Add(control);

        return mainGrid;
    }

    public static VideoPlayerControl MakeVideoPlayer()
    {
        try
        {
            if (Se.Settings.Video.VideoPlayer.Equals(VideoPlayerName.Vlc, StringComparison.OrdinalIgnoreCase))
            {
                var player = new LibVlcDynamicPlayer();
                if (player.CanLoad())
                {
                    var view = new LibVlcDynamicNativeControl(player);
                    return MakeVideoPlayerControl(player, view);
                }
            }

            if (Se.Settings.Video.VideoPlayer.Equals(VideoPlayerName.MpvWid, StringComparison.OrdinalIgnoreCase))
            {
                var player = new LibMpvDynamicPlayer();
                if (player.CanLoad())
                {
                    var view = new LibMpvDynamicNativeControl(player);
                    return MakeVideoPlayerControl(player, view);
                }
            }

            if (Se.Settings.Video.VideoPlayer.Equals(VideoPlayerName.MpvSw, StringComparison.OrdinalIgnoreCase))
            {
                var player = new LibMpvDynamicPlayer();
                if (player.CanLoad())
                {
                    var view = new LibMpvDynamicSoftwareControl(player);
                    return MakeVideoPlayerControl(player, view);
                }
            }

            if (Se.Settings.Video.VideoPlayer.Equals(VideoPlayerName.MpvMetal, StringComparison.OrdinalIgnoreCase)
                && RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var player = new LibMpvDynamicPlayer();
                if (player.CanLoad())
                {
                    var view = new LibMpvDynamicMetalControl(player);
                    return MakeVideoPlayerControl(player, view);
                }
            }

            if (Se.Settings.Video.VideoPlayer.StartsWith("mpv", StringComparison.OrdinalIgnoreCase)) // VideoPlayerCodes.MpvOpenGl
            {
                var player = new LibMpvDynamicPlayer();
                if (player.CanLoad())
                {
                    var view = new LibMpvDynamicOpenGlControl(player);
                    return MakeVideoPlayerControl(player, view);
                }
            }

            return MakeVideoPlayerControl(new EmptyVideoPlayer(), new Label());
        }
        catch
        {
            return MakeVideoPlayerControl(new EmptyVideoPlayer(), new Label());
        }

        throw new InvalidOperationException("Failed to create video player control.");
    }

    /// <summary>
    /// Creates a video player that avoids native window embedding on Windows.
    /// On Windows, NativeControlHost (used by mpv-wid and VLC) creates a Win32 HWND
    /// that always renders on top of Avalonia overlays (the "airspace problem"),
    /// making logo/overlay previews invisible. Using software rendering avoids this.
    /// On non-Windows or when mpv is unavailable, falls back to the default player.
    /// </summary>
    public static VideoPlayerControl MakeVideoPlayerPreferNonNative()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && Se.Settings.Video.VideoPlayer != VideoPlayerName.MpvOpenGl)
        {
            var player = new LibMpvDynamicPlayer();
            if (player.CanLoad())
            {
                var view = new LibMpvDynamicSoftwareControl(player);
                return MakeVideoPlayerControl(player, view);
            }
        }

        return MakeVideoPlayer();
    }

    private static VideoPlayerControl MakeVideoPlayerControl(IVideoPlayer videoPlayer, Control view)
    {
        return new VideoPlayerControl(videoPlayer)
        {
            PlayerContent = view,
            StopIsVisible = Se.Settings.Video.ShowStopButton,
            FullScreenIsVisible = Se.Settings.Video.ShowFullscreenButton,
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
    }
}