using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Nikse.SubtitleEdit.Features.Video.OpenFromUrl;

public enum OpenFromUrlMode
{
    OpenOnline,
    DownloadAndOpen,
}

public partial class OpenFromUrlViewModel : ObservableObject
{
    [ObservableProperty] private string _url;

    public Window? Window { get; set; }

    public OpenFromUrlMode? SelectedMode { get; private set; }

    public bool OkPressed => SelectedMode != null;

    public OpenFromUrlViewModel()
    {
        Url = string.Empty;
    }

    [RelayCommand]
    private void OpenOnline()
    {
        if (string.IsNullOrWhiteSpace(Url))
        {
            return;
        }

        SelectedMode = OpenFromUrlMode.OpenOnline;
        Window?.Close();
    }

    [RelayCommand]
    private void DownloadAndOpen()
    {
        if (string.IsNullOrWhiteSpace(Url))
        {
            return;
        }

        SelectedMode = OpenFromUrlMode.DownloadAndOpen;
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
}
