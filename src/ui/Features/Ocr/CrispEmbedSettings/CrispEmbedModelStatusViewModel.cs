using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Ocr.Engines;
using System;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Ocr.CrispEmbedSettings;

/// <summary>
/// One downloadable CrispEmbed model as shown in the settings dialog: which backend it belongs
/// to, its install state, and the button that (re-)downloads it. Every backend's models are
/// listed, so the backend name is part of the row rather than a heading.
/// </summary>
public partial class CrispEmbedModelStatusViewModel : ObservableObject
{
    public CrispEmbedBackend Backend { get; }
    public CrispEmbedModel Model { get; }

    public string BackendName => Backend.Name;
    public string ModelName => Model.Name;
    public string SizeText => Model.Size;

    [ObservableProperty] private string _statusLabel = string.Empty;
    [ObservableProperty] private IBrush _statusBrush = Brushes.Gray;
    [ObservableProperty] private string _downloadButtonText = string.Empty;

    public IAsyncRelayCommand DownloadCommand { get; }

    public CrispEmbedModelStatusViewModel(
        CrispEmbedBackend backend,
        CrispEmbedModel model,
        Func<CrispEmbedModelStatusViewModel, Task> download)
    {
        Backend = backend;
        Model = model;
        DownloadCommand = new AsyncRelayCommand(() => download(this));
    }
}
