using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.ReviewSpeech;

public partial class GeneratingAudioViewModel : ObservableObject
{
    public Window? Window { get; set; }
    public CancellationTokenSource CancellationTokenSource { get; } = new();

    [RelayCommand]
    private void Cancel()
    {
        CancellationTokenSource.Cancel();
        Dispatcher.UIThread.Post(() => Window?.Close());
    }

    public void Close()
    {
        Dispatcher.UIThread.Post(() => Window?.Close());
    }
}
