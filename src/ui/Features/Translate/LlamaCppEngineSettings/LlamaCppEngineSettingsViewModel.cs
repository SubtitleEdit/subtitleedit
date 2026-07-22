using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using Nikse.SubtitleEdit.Logic.LlamaCpp;
using Nikse.SubtitleEdit.Logic.Media;

namespace Nikse.SubtitleEdit.Features.Translate.LlamaCppEngineSettings;

/// <summary>
/// Shows which llama.cpp build is installed (backend variant, pinned release tag, install folder)
/// and whether it is up to date, mirroring the per-engine text-to-speech settings dialogs.
/// </summary>
public partial class LlamaCppEngineSettingsViewModel : ObservableObject
{
    private readonly IFolderHelper _folderHelper;

    private Func<Task>? _redownloadAsync;

    [ObservableProperty] private string _backendLabel = string.Empty;
    [ObservableProperty] private string _statusLabel = string.Empty;
    [ObservableProperty] private IBrush _statusBrush = Brushes.Gray;
    [ObservableProperty] private string _releaseTag = LlamaCppDownloadService.ReleaseTag;
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

    public LlamaCppEngineSettingsViewModel(IFolderHelper folderHelper)
    {
        _folderHelper = folderHelper;
    }

    /// <summary>
    /// <paramref name="redownloadAsync"/> is supplied by the caller so the download runs through the
    /// same flow the caller already owns (stopping the server first, refreshing its model list and
    /// status dots afterwards) instead of this dialog duplicating it.
    /// </summary>
    public void Initialize(Func<Task> redownloadAsync)
    {
        _redownloadAsync = redownloadAsync;
        Refresh();
    }

    private void Refresh()
    {
        InstallFolder = LlamaCppServerManager.GetAndCreateFolder();
        IsInstalled = LlamaCppServerManager.IsEngineInstalled();
        BackendLabel = IsInstalled ? DetectInstalledBackend(InstallFolder) : Se.Language.General.NotInstalled;
        EngineUpdateStatus = IsInstalled
            ? LlamaCppUpdateStatus.GetEngineUpdateStatus()
            : DownloadHashManager.UpdateStatus.Unknown;
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

    /// <summary>
    /// llama.cpp ships CPU/Vulkan/CUDA builds into one folder, so the backend cannot be inferred from
    /// the folder alone. Only Windows has a proven detector (<see cref="DownloadHashManager.DetectLlamaCppWindowsVariant"/>,
    /// which reads the shipped ggml backend DLLs); elsewhere we report OS+arch only rather than guess.
    /// </summary>
    private static string DetectInstalledBackend(string folder)
    {
        if (Configuration.IsRunningOnMac)
        {
            return RuntimeInformation.ProcessArchitecture == Architecture.Arm64
                ? "macOS ARM64 (Metal)"
                : "macOS x64";
        }

        if (Configuration.IsRunningOnLinux)
        {
            return RuntimeInformation.ProcessArchitecture == Architecture.Arm64
                ? "Linux ARM64"
                : "Linux x64";
        }

        return DownloadHashManager.DetectLlamaCppWindowsVariant(folder) switch
        {
            "cuda" => "Windows x64 (CUDA)",
            "vulkan" => "Windows x64 (Vulkan)",
            "cpu" => "Windows x64 (CPU)",
            _ => "Windows x64",
        };
    }

    [RelayCommand]
    private async Task Redownload()
    {
        if (_redownloadAsync == null)
        {
            return;
        }

        await _redownloadAsync();
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
