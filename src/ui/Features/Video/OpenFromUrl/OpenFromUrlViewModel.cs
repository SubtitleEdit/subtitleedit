using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.OpenFromUrl;

public enum OpenFromUrlMode
{
    OpenOnline,
    DownloadAndOpen,
}

public partial class OpenFromUrlViewModel : ObservableObject
{
    [ObservableProperty] private string _url;
    [ObservableProperty] private bool _downloadSubtitles;

    public Window? Window { get; set; }

    public OpenFromUrlMode? SelectedMode { get; private set; }

    public bool OkPressed => SelectedMode != null;

    /// <summary>
    /// Set by the caller (MainViewModel) to reuse its existing yt-dlp
    /// install/update prompt. Invoked by the "Download yt-dlp" button so the
    /// window doesn't duplicate the download orchestration.
    /// </summary>
    public Func<Task>? DownloadOrUpdateYtDlpRequested { get; set; }

    public OpenFromUrlViewModel()
    {
        Url = string.Empty;
        DownloadSubtitles = false;
    }

    [RelayCommand]
    private async Task DownloadYtDlp()
    {
        if (DownloadOrUpdateYtDlpRequested is not null)
        {
            await DownloadOrUpdateYtDlpRequested();
        }
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
