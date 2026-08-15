using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Ocr.Download;
using Nikse.SubtitleEdit.Features.Ocr.Engines;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using Nikse.SubtitleEdit.Logic.Media;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Ocr.CrispEmbedSettings;

/// <summary>
/// Everything downloadable about the CrispEmbed OCR engine in one place: the engine binaries
/// (with the hardware build actually installed) and every backend's models. Replaces the pair of
/// small download icon buttons that used to sit next to the engine picker.
/// </summary>
public partial class CrispEmbedSettingsViewModel : ObservableObject
{
    private readonly IWindowService _windowService;
    private readonly IFolderHelper _folderHelper;

    [ObservableProperty] private string _engineLabel = string.Empty;
    [ObservableProperty] private IBrush _engineBrush = Brushes.Gray;
    [ObservableProperty] private string _engineDownloadButtonText = string.Empty;
    [ObservableProperty] private string _installFolder = string.Empty;

    public ObservableCollection<CrispEmbedModelStatusViewModel> Models { get; } = new();

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    public CrispEmbedSettingsViewModel(IWindowService windowService, IFolderHelper folderHelper)
    {
        _windowService = windowService;
        _folderHelper = folderHelper;

        foreach (var backend in CrispEmbedEngine.GetBackends())
        {
            foreach (var model in backend.Models)
            {
                Models.Add(new CrispEmbedModelStatusViewModel(backend, model, DownloadModel));
            }
        }
    }

    public void Initialize()
    {
        InstallFolder = CrispEmbedEngine.GetAndCreateFolder();
        Refresh();
    }

    private void Refresh()
    {
        var isEngineInstalled = CrispEmbedEngine.IsEngineInstalled();
        if (!isEngineInstalled)
        {
            EngineLabel = $"{Se.Language.General.NotInstalled} ({CrispEmbedEngine.DownloadSizeText})";
            EngineBrush = new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36)); // red
            EngineDownloadButtonText = string.Format(Se.Language.General.DownloadX, CrispEmbedEngine.StaticName);
        }
        else if (DownloadHashManager.GetSidecarStatus(CrispEmbedEngine.GetAndCreateFolder())
                 == DownloadHashManager.UpdateStatus.UpdateAvailable)
        {
            EngineLabel = WithBuild(Se.Language.General.UpdateAvailable);
            EngineBrush = StatusDots.Amber;
            EngineDownloadButtonText = string.Format(Se.Language.General.UpdateX, CrispEmbedEngine.StaticName);
        }
        else
        {
            EngineLabel = WithBuild(Se.Language.General.Installed);
            EngineBrush = StatusDots.Green;
            EngineDownloadButtonText = string.Format(Se.Language.General.ReDownloadX, CrispEmbedEngine.StaticName);
        }

        foreach (var model in Models)
        {
            var installed = model.Backend.IsModelInstalled(model.Model);
            model.StatusLabel = installed ? Se.Language.General.Installed : Se.Language.General.NotInstalled;
            model.StatusBrush = installed ? StatusDots.Green : StatusDots.Grey;
            model.DownloadButtonText = installed ? Se.Language.General.Redownload : Se.Language.General.Download;
        }
    }

    /// <summary>
    /// Appends the hardware build recorded in the install sidecar - the CPU/Vulkan/CUDA choice is
    /// made once at download time and is otherwise invisible, so someone wondering why CrispEmbed
    /// is slow can see they are on the CPU build (issue #13400).
    /// </summary>
    private static string WithBuild(string status)
    {
        var sidecar = DownloadHashManager.TryReadSidecar(CrispEmbedEngine.GetAndCreateFolder());
        var build = sidecar == null ? null : DownloadHashManager.GetCrispEmbedVariant(sidecar.Value.Key);
        var buildName = build switch
        {
            "cuda" => "CUDA",
            "vulkan" => "Vulkan",
            "cpu" => "CPU",
            _ => string.Empty,
        };

        return string.IsNullOrEmpty(buildName) ? status : $"{status} ({buildName})";
    }

    private async Task DownloadModel(CrispEmbedModelStatusViewModel item)
    {
        if (Window == null)
        {
            return;
        }

        if (item.Backend.IsModelInstalled(item.Model))
        {
            var answer = await MessageBox.Show(
                Window,
                Se.Language.General.Download,
                string.Format(Se.Language.Translate.XIsAlreadyDownloadedReDownload, item.Model.Name),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (answer != MessageBoxResult.Yes)
            {
                return;
            }
        }

        // Goes through EnsureReadyAsync rather than straight to the model download so a model
        // picked before the engine exists still offers the engine download first.
        await CrispEmbedDownloadHelper.EnsureReadyAsync(
            Window, _windowService, item.Backend, item.Model, forceModelDownload: true);

        Refresh();
    }

    [RelayCommand]
    private async Task RedownloadEngine()
    {
        if (Window == null)
        {
            return;
        }

        await CrispEmbedDownloadHelper.DownloadEngineAsync(Window, _windowService);
        Refresh();
    }

    [RelayCommand]
    private async Task OpenInstallFolder()
    {
        if (Window == null || string.IsNullOrEmpty(InstallFolder))
        {
            return;
        }

        try
        {
            await _folderHelper.OpenFolder(Window, InstallFolder);
        }
        catch
        {
            // ignore - best-effort UX
        }
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
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
