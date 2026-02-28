using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Shared.Undocked;

public partial class VideoPlayerUndockedViewModel : ObservableObject
{
    [ObservableProperty] private string _error = string.Empty;

    public Window? Window { get; set; }
    public Grid? OriginalParent { get; set; }
    public int OriginalRow { get; set; }
    public int OriginalColumn { get; set; }
    public int OriginalIndex { get; set; }
    public Grid? VideoPlayer { get; set; }
    public Main.MainViewModel? MainViewModel { get; set; }
    public bool AllowClose { get; set; }
    public VideoPlayerControl? VideoPlayerControl { get; set; }
    
    private DispatcherTimer? _mouseMoveDetectionTimer;
    private (int X, int Y) _lastCursorPosition;
    private (int X, int Y) _lastPointerMovedCursorPosition;

    private string _originalVideoFileName = string.Empty;   
    private double _originalVolume;
    private double _originalPosition;

    [RelayCommand]
    private void ToggleFullScreen()
    {
        if (Window == null || VideoPlayerControl == null)
        {
            return;
        }

        if (Window.WindowState == WindowState.FullScreen)
        {
            Window.WindowState = WindowState.Normal;
            VideoPlayerControl.IsFullScreen = false;
        }
        else
        {
            Window.WindowState = WindowState.FullScreen;
            VideoPlayerControl.IsFullScreen = true;
        }
    }

    internal void Initialize(string videoFileName, double originalPosition, double originalVolume, Main.MainViewModel mainViewModel)
    {
        _originalVideoFileName = videoFileName;
        _originalVolume = originalVolume;
        _originalPosition = originalPosition;
         MainViewModel = mainViewModel;
    }

    internal void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (!AllowClose && e.CloseReason != WindowCloseReason.OwnerWindowClosing)
        {
            e.Cancel = true;

            if (Window != null)
            {
                Window.WindowState = WindowState.Minimized;
            }
        }
        else
        {
            UiUtil.SaveWindowPosition(Window);
        }
    }

    internal void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && e.KeyModifiers.HasFlag(KeyModifiers.Alt) || e.Key == Key.F11)
        {
            e.Handled = true;

            ToggleFullScreen();

            VideoPlayerControl?.NotifyUserActivity();
            return;
        }

        MainViewModel?.OnKeyDownHandler(sender, e);
        if (e.Handled)
        {
            return;
        }

        if (VideoPlayerControl != null)
        {
            if (e.Key == Key.Escape && Window is { } && Window.WindowState == WindowState.FullScreen)
            {
                ToggleFullScreen();
                e.Handled = true;
                VideoPlayerControl.NotifyUserActivity();
                return;
            }

            if (e.Key == Key.Space)
            {
                VideoPlayerControl.TogglePlayPause();
                e.Handled = true;
                VideoPlayerControl.NotifyUserActivity();
                return;
            }

            if (e.Key == Key.Right)
            {
                e.Handled = true;
                VideoPlayerControl.Position += 2;
                VideoPlayerControl.NotifyUserActivity();
                return;
            }

            if (e.Key == Key.Left)
            {
                e.Handled = true;
                VideoPlayerControl.Position -= 2;
                VideoPlayerControl.NotifyUserActivity();
                return;
            }

            if (e.Key == Key.Up && e.KeyModifiers == KeyModifiers.None)
            {
                e.Handled = true;
                VideoPlayerControl.Volume += 2;
                VideoPlayerControl.NotifyUserActivity();
                return;
            }

            if (e.Key == Key.Down && e.KeyModifiers == KeyModifiers.None)
            {
                e.Handled = true;
                VideoPlayerControl.Volume -= 2;
                VideoPlayerControl.NotifyUserActivity();
                return;
            }

            VideoPlayerControl.NotifyUserActivity();
        }
    }

    internal void Onloaded(object? sender, RoutedEventArgs e)
    {
        if (Window == null || MainViewModel == null)
        {
            return;
        }   

        UiUtil.RestoreWindowPosition(Window);

        VideoPlayer = InitVideoPlayer.MakeLayoutVideoPlayer(MainViewModel, out var videoPlayerControl);
        Window!.Content = VideoPlayer;
        VideoPlayerControl = videoPlayerControl;
        VideoPlayerControl.FullScreenCommand = ToggleFullScreenCommand;
        VideoPlayerControl.FullscreenCollapseRequested += () => ToggleFullScreen();

        if (!string.IsNullOrEmpty(_originalVideoFileName))
        {
            Dispatcher.UIThread.Post(async () =>
            {
                Task.Delay(100).Wait();
                await videoPlayerControl.Open(_originalVideoFileName);
                Task.Delay(100).Wait();
                await videoPlayerControl.WaitForPlayersReadyAsync();
                Task.Delay(100).Wait();

                videoPlayerControl.Volume = _originalVolume;
                videoPlayerControl.Position = _originalPosition;
            });
        }

        const int mouseMovementMinPixels = 20;

        // Poll for actual cursor position using platform APIs
        // This works regardless of Avalonia event handling or MPV
        _mouseMoveDetectionTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
        _mouseMoveDetectionTimer.Tick += (s, e) =>
        {
            try
            {
                var cursorPos = CursorPositionHelper.GetCursorPosition();
                if (cursorPos.HasValue)
                {
                    if (Math.Abs(cursorPos.Value.X - _lastCursorPosition.X) > mouseMovementMinPixels ||
                        Math.Abs(cursorPos.Value.Y - _lastCursorPosition.Y) > mouseMovementMinPixels)
                    {
                        _lastCursorPosition = cursorPos.Value;
                        VideoPlayerControl.NotifyUserActivity();
                    }
                }
            }
            catch
            {
                // Ignore errors
            }
        };

        // Keep these handlers as fallback if native APIs fail
        videoPlayerControl.PointerMoved += (_, e) =>
        {
            var pos = e.GetCurrentPoint(videoPlayerControl);
            if (Math.Abs(pos.Position.X - _lastPointerMovedCursorPosition.X) > mouseMovementMinPixels ||
                Math.Abs(pos.Position.Y - _lastPointerMovedCursorPosition.Y) > mouseMovementMinPixels)
            {
                VideoPlayerControl.NotifyUserActivity();
                _lastPointerMovedCursorPosition = ((int)pos.Position.X, (int)pos.Position.Y);
            }

            if (Window != null)
            {
                VideoPlayerControl.IsFullScreen = Window.WindowState == WindowState.FullScreen;
            }
        };
    }

    internal void OnKeyUp(object? sender, KeyEventArgs e)
    {
        MainViewModel?.OnKeyUpHandler(sender, e);
    }
}
