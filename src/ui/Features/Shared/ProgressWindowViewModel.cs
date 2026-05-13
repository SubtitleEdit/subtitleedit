using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Nikse.SubtitleEdit.Features.Shared;

public partial class ProgressWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private double _progress;

    [ObservableProperty]
    private string _statusText = string.Empty;

    [ObservableProperty]
    private string _error = string.Empty;

    public Window? Window { get; set; }

    public ProgressWindowViewModel()
    {
        Progress = 0;
    }

    public void UpdateProgress(double value, string? statusText = null)
    {
        Dispatcher.UIThread.Post(() =>
        {
            Progress = value;
            if (statusText != null)
            {
                StatusText = statusText;
            }
        });
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }
}
