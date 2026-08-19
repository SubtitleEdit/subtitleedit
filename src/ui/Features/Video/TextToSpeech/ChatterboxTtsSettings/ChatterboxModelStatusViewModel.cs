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
    [ObservableProperty] private string _downloadButtonText = string.Empty;

    public IAsyncRelayCommand DownloadCommand { get; }

    public ChatterboxModelStatusViewModel(string modelKey, string displayName, Func<string, Task> download)
    {
        ModelKey = modelKey;
        DisplayName = displayName;
        DownloadCommand = new AsyncRelayCommand(() => download(modelKey));
    }
}
