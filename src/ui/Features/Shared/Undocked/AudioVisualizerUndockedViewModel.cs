using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Shared.Undocked;

public partial class AudioVisualizerUndockedViewModel : ObservableObject
{
    [ObservableProperty] private string _error = string.Empty;

    public Window? Window { get; set; }
    public Grid? OriginalParent { get; set; }
    public int OriginalRow { get; set; }
    public int OriginalColumn { get; set; }
    public int OriginalIndex { get; set; }
    public Grid? AudioVisualizer { get; set; }
    public Main.MainViewModel? MainViewModel { get; set; }
    public bool AllowClose { get; set; }

    internal void Initialize(AudioVisualizer? audioVisualizer, Main.MainViewModel mainViewModel)
    {
        AudioVisualizer = InitWaveform.MakeWaveform(mainViewModel);
        AudioVisualizer.DataContext = mainViewModel;

        if (mainViewModel.AudioVisualizer != null && audioVisualizer != null)
        {
            mainViewModel.AudioVisualizer.WavePeaks = audioVisualizer.WavePeaks;
        }

        MainViewModel = mainViewModel;
    }

    internal void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        // Only intercept explicit user closes (clicking the title-bar X). Owner-window
        // and app-shutdown closes must be allowed through; otherwise the app gets stuck
        // when the main window is closing or the OS is shutting down. Since this window
        // is now an independent top-level (no owner), the cascade comes from
        // MainViewModel.CleanUp() which sets AllowClose=true before calling Close().
        if (!AllowClose && e.CloseReason == WindowCloseReason.WindowClosing)
        {
            e.Cancel = true;

            if (Window != null)
            {
                Window.WindowState = WindowState.Minimized;
            }

            return;
        }

        UiUtil.SaveWindowPosition(Window);
    }

    internal void OnKeyDown(object? sender, KeyEventArgs e)
    {
        var videoPlayer = MainViewModel?.GetVideoPlayerControl();

        MainViewModel?.OnKeyDownHandler(sender, e);
        if (e.Handled)
        {
            return;
        }

        if (videoPlayer != null)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
                videoPlayer.TogglePlayPause();
                return;
            }
            else if (e.Key == Key.Right)
            {
                e.Handled = true;
                videoPlayer.Position += 2;
                return;
            }
            else if (e.Key == Key.Left)
            {
                e.Handled = true;
                videoPlayer.Position -= 2;
                return;
            }
            else if (e.Key == Key.Up && e.KeyModifiers == KeyModifiers.None)
            {
                e.Handled = true;
                videoPlayer.Volume += 2;
                return;
            }
            else if (e.Key == Key.Down && e.KeyModifiers == KeyModifiers.None)
            {
                e.Handled = true;
                videoPlayer.Volume -= 2;
                return;
            }
        }
    }

    internal void OnKeyUp(object? sender, KeyEventArgs e)
    {
        MainViewModel?.OnKeyUpHandler(sender, e);
    }

    internal void Onloaded(object? sender, RoutedEventArgs e)
    {
        Window!.Content = AudioVisualizer;
        UiUtil.RestoreWindowPosition(Window);
    }
}
