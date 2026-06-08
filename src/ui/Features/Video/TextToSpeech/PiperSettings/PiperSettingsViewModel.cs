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

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.PiperSettings;

public partial class PiperSettingsViewModel : ObservableObject
{
    // Piper is pinned to a single fixed rhasspy/piper release (see TtsDownloadService.cs).
    private const string PiperReleaseTag = "2023.11.14-2";

    private readonly IWindowService _windowService;
    private readonly IFolderHelper _folderHelper;

    [ObservableProperty] private string _backendLabel = string.Empty;
    [ObservableProperty] private string _statusLabel = string.Empty;
    [ObservableProperty] private IBrush _statusBrush = Brushes.Gray;
    [ObservableProperty] private string _releaseTag = PiperReleaseTag;
    [ObservableProperty] private string _installFolder = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DownloadButtonLabel))]
    private bool _isInstalled;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DownloadButtonLabel))]
    private DownloadHashManager.UpdateStatus _engineUpdateStatus;

    // "Download" when not installed, "Update" when a newer engine release is available,
    // otherwise "Re-download".
    public string DownloadButtonLabel
    {
        get
        {
            if (!IsInstalled)
            {
                return Se.Language.General.Download;
            }

            return EngineUpdateStatus == DownloadHashManager.UpdateStatus.UpdateAvailable
                ? Se.Language.General.Update
                : Se.Language.General.Redownload;
        }
    }

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    public PiperSettingsViewModel(IWindowService windowService, IFolderHelper folderHelper)
    {
        _windowService = windowService;
        _folderHelper = folderHelper;
    }

    public void Initialize()
    {
        InstallFolder = Piper.GetSetPiperFolder();
        Refresh();
    }

    private void Refresh()
    {
        IsInstalled = File.Exists(Piper.GetPiperExecutableFileName());
        BackendLabel = IsInstalled ? DetectInstalledBackend() : Se.Language.General.NotInstalled;
        EngineUpdateStatus = IsInstalled ? Piper.GetEngineUpdateStatus() : DownloadHashManager.UpdateStatus.Unknown;
        ApplyStatus(EngineUpdateStatus, IsInstalled);
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

    // Piper ships a single build per platform, so the backend label is purely OS+arch.
    private static string DetectInstalledBackend()
    {
        if (Configuration.IsRunningOnMac)
        {
            return "macOS x64";
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
            "Re-download Piper",
            $"{Environment.NewLine}Download the latest Piper now?",
            MessageBoxButtons.YesNoCancel,
            MessageBoxIcon.Question);
        if (answer != MessageBoxResult.Yes)
        {
            return;
        }

        await _windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(Window, vm => vm.StartDownloadPiper());
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
