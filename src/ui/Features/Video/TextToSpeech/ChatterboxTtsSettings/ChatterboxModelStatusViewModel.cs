using System;
using System.Threading.Tasks;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.ChatterboxTtsSettings;

/// <summary>
/// One downloadable Chatterbox model pair (T3 + S3Gen) as shown in the settings dialog:
/// its install state and the button that (re-)downloads it.
/// </summary>
public partial class ChatterboxModelStatusViewModel : ObservableObject
{
    public string ModelKey { get; }
    public string DisplayName { get; }

    [ObservableProperty] private string _statusLabel = string.Empty;
    [ObservableProperty] private IBrush _statusBrush = Brushes.Gray;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DownloadButtonAccessibleName))]
    private string _downloadButtonText = string.Empty;

    /// <summary>
    /// Screen-reader name for the download button: the caption alone is the same on every
    /// row, so the model name is added ("Download Multilingual") (#12087).
    /// </summary>
    public string DownloadButtonAccessibleName => $"{DownloadButtonText} {DisplayName}";

    public IAsyncRelayCommand DownloadCommand { get; }

    public ChatterboxModelStatusViewModel(string modelKey, string displayName, Func<string, Task> download)
    {
        ModelKey = modelKey;
        DisplayName = displayName;
        DownloadCommand = new AsyncRelayCommand(() => download(modelKey));
    }
}
