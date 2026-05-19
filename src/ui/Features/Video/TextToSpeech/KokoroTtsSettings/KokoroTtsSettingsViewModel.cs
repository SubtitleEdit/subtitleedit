using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.DownloadTts;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using Nikse.SubtitleEdit.Logic.Media;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.KokoroTtsSettings;

public partial class KokoroTtsSettingsViewModel : ObservableObject
{
    private readonly IWindowService _windowService;
    private readonly IFolderHelper _folderHelper;

    [ObservableProperty] private string _backendLabel = string.Empty;
    [ObservableProperty] private string _statusLabel = string.Empty;
    [ObservableProperty] private IBrush _statusBrush = Brushes.Gray;
    [ObservableProperty] private string _releaseTag = KokoroTtsCppDownloadService.ReleaseTag;
    [ObservableProperty] private string _installFolder = string.Empty;
    [ObservableProperty] private bool _isInstalled;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    public KokoroTtsSettingsViewModel(IWindowService windowService, IFolderHelper folderHelper)
    {
        _windowService = windowService;
        _folderHelper = folderHelper;
    }

    public void Initialize()
    {
        InstallFolder = KokoroTtsCpp.GetSetFolder();
        Refresh();
    }

    private void Refresh()
    {
        IsInstalled = File.Exists(KokoroTtsCpp.GetExecutableFileName());
        BackendLabel = IsInstalled ? DetectInstalledBackend() : Se.Language.General.NotInstalled;
        ApplyStatus(IsInstalled ? KokoroTtsCpp.GetEngineUpdateStatus() : DownloadHashManager.UpdateStatus.Unknown, IsInstalled);
    }

    private void ApplyStatus(DownloadHashManager.UpdateStatus status, bool installed)
    {
        if (!installed)
        {
            StatusLabel = Se.Language.General.NotInstalled;
            StatusBrush = new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36));   // red
            return;
        }

        switch (status)
        {
            case DownloadHashManager.UpdateStatus.UpToDate:
                StatusLabel = Se.Language.General.UpToDate;
                StatusBrush = new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50)); // green
                break;
            case DownloadHashManager.UpdateStatus.UpdateAvailable:
                StatusLabel = Se.Language.General.UpdateAvailable;
                StatusBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0x98, 0x00)); // amber
                break;
            default:
                StatusLabel = Se.Language.General.UnknownNoInstallRecord;
                StatusBrush = new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E)); // grey
                break;
        }
    }

    // Kokoro has only one build per platform, so the backend label is purely OS+arch.
    private static string DetectInstalledBackend()
    {
        if (Configuration.IsRunningOnMac)
        {
            return "macOS arm64";
        }

        if (Configuration.IsRunningOnLinux)
        {
            return RuntimeInformation.ProcessArchitecture == Architecture.Arm64
                ? "Linux ARM64"
                : "Linux x64";
        }

        return "Windows x64";
    }

    [RelayCommand]
    private async Task Redownload()
    {
        if (Window == null)
        {
            return;
        }

        var answer = await MessageBox.Show(
            Window,
            "Re-download Kokoro TTS",
            $"{Environment.NewLine}Download the latest Kokoro TTS now?",
            MessageBoxButtons.YesNoCancel,
            MessageBoxIcon.Question);
        if (answer != MessageBoxResult.Yes)
        {
            return;
        }

        await _windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(Window, vm => vm.StartDownloadKokoroTtsCpp());
        Refresh();
    }

    [RelayCommand]
    private async Task OpenFolder()
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
            // ignore - best-effort UX, the path label is still shown
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
