using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Controls.VideoPlayer;

namespace Nikse.SubtitleEdit.Features.Video.BurnIn;

public partial class SelectVideoPositionViewModel : ObservableObject
{
    public Window? Window { get; set; }
    public string VideoFileName { get; set; }
    public bool OkPressed { get; private set; }
    public VideoPlayerControl? VideoPlayerControl { get; set; }
    public double PositionInSeconds { get; set; }

    public SelectVideoPositionViewModel()
    {
        VideoFileName = string.Empty;
    }

    public void Initialize(string videoFileName)
    {
        VideoFileName = videoFileName;        
    }

    [RelayCommand]
    private void Ok()
    {
        PositionInSeconds = VideoPlayerControl?.Position ?? 0;
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
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
    }

    internal void OnLoaded()
    {
        Dispatcher.UIThread.Post(async() =>
        {
            await VideoPlayerControl!.Open(VideoFileName);
        });
    }
}