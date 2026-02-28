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
            mediaFile = vm.VideoPlayerControl.VideoPlayerInstance.FileName;
            position = vm.VideoPlayerControl.VideoPlayerInstance.Position;
            vm.VideoPlayerControl.VideoPlayerInstance.CloseFile();
            vm.VideoPlayerControl.Content = null;
            vm.VideoPlayerControl = null;
        }

        var mainGrid = new Grid
        {
            RowDefinitions = new RowDefinitions("*"),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Margin = new Thickness(0),
        };

        DragDrop.SetAllowDrop(mainGrid, true);
        mainGrid.AddHandler(DragDrop.DragOverEvent, vm.VideoOnDragOver, RoutingStrategies.Bubble);
        mainGrid.AddHandler(DragDrop.DropEvent, vm.VideoOnDrop, RoutingStrategies.Bubble);

        var control = MakeVideoPlayer();
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
        control.VolumeChanged += v =>
        {
            Se.Settings.Video.Volume = v;
        };
        control.ToggleDisplayProgressTextModeRequested += () =>
        {
            vm.ToggleVideoPlayerDisplayTimeLeftCommand.Execute(null);
        };
        control.VideoFileNamePointerPressed += vm.VideoPlayerControlPointerPressed;
        control.SurfacePointerPressed += (_,_) => vm.VideoPlayerAreaPointerPressed();

        Grid.SetRow(control, 0);
        mainGrid.Children.Add(control);

        return mainGrid;
    }

    public static VideoPlayerControl MakeVideoPlayer()
    {
        try
        {
            if (Se.Settings.Video.VideoPlayer.Equals("vlc", StringComparison.OrdinalIgnoreCase))
            {
                var player = new LibVlcDynamicPlayer();
                if (player.CanLoad())
                {
                    var view = new LibVlcDynamicNativeControl(player);
                    return MakeVideoPlayerControl(player, view);
                }
            }

            if (Se.Settings.Video.VideoPlayer.Equals("mpv-wid", StringComparison.OrdinalIgnoreCase))
            {
                var player = new LibMpvDynamicPlayer();
                if (player.CanLoad())
                {
                    var view = new LibMpvDynamicNativeControl(player);
                    return MakeVideoPlayerControl(player, view);
                }
            }

            if (Se.Settings.Video.VideoPlayer.Equals("mpv-sw", StringComparison.OrdinalIgnoreCase))
            {
                var player = new LibMpvDynamicPlayer();
                if (player.CanLoad())
                {
                    var view = new LibMpvDynamicSoftwareControl(player);
                    return MakeVideoPlayerControl(player, view);
                }
            }

            if (Se.Settings.Video.VideoPlayer.StartsWith("mpv", StringComparison.OrdinalIgnoreCase)) // mpv-opengl
            {
                var player = new LibMpvDynamicPlayer();
                if (player.CanLoad())
                {
                    var view = new LibMpvDynamicOpenGlControl(player);
                    return MakeVideoPlayerControl(player, view);
                }
            }

            return MakeVideoPlayerControl(new VideoPlayerInstanceNone(), new Label());
        }
        catch
        {
            return MakeVideoPlayerControl(new VideoPlayerInstanceNone(), new Label());
        }

        throw new InvalidOperationException("Failed to create video player control.");
    }

    private static VideoPlayerControl MakeVideoPlayerControl(IVideoPlayerInstance videoPlayer, Control view)
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
