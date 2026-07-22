using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.UiLogic;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.EngineSettings;

public partial class SpeechToTextEngineSettingsViewModel : ObservableObject
{
    private readonly IFolderHelper _folderHelper;

    private ISpeechToTextEngine? _engine;
    private Func<Task>? _redownloadAsync;

    [ObservableProperty] private string _titleText = string.Empty;
    [ObservableProperty] private string _subtitleText = string.Empty;
    [ObservableProperty] private string _backendLabel = string.Empty;
    [ObservableProperty] private string _statusLabel = string.Empty;
    [ObservableProperty] private IBrush _statusBrush = Brushes.Gray;
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

    public SpeechToTextEngineSettingsViewModel(IFolderHelper folderHelper)
    {
        _folderHelper = folderHelper;
    }

    public void Initialize(ISpeechToTextEngine engine, Func<Task> redownloadAsync)
    {
        _engine = engine;
        _redownloadAsync = redownloadAsync;

        TitleText = engine.Name;
        SubtitleText = Se.Language.Video.AudioToText.EngineSettingsSubtitle;

        Refresh();
    }

    private void Refresh()
    {
        if (_engine == null)
        {
            return;
        }

        InstallFolder = _engine.GetAndCreateWhisperFolder();
        IsInstalled = _engine.IsEngineInstalled();
        var backendLabel = IsInstalled ? DetectBackend(_engine, InstallFolder) : Se.Language.General.NotInstalled;
        BackendLabel = backendLabel;
        EngineUpdateStatus = IsInstalled ? ComputeStatus(_engine, InstallFolder) : DownloadHashManager.UpdateStatus.Unknown;
        ApplyStatus(EngineUpdateStatus, IsInstalled);

        // For CrispASR engines, append the installed runtime version once it's been probed.
        // The probe shells out to `crispasr --version` with a 5 s timeout, so do it off the
        // UI thread and patch BackendLabel via the dispatcher when the result lands. First
        // call is uncached (a few hundred ms typically); subsequent ones hit the cache and
        // return immediately, but we still go through Task.Run for consistency.
        if (IsInstalled && _engine is ICrispAsrEngine crispAsr)
        {
            var exe = crispAsr.GetExecutable();
            _ = Task.Run(() =>
            {
                try
                {
                    var version = CrispAsrVersion.TryGet(exe);
                    if (string.IsNullOrEmpty(version))
                    {
                        return;
                    }
                    var withVersion = $"{backendLabel}, v{version}";
                    Dispatcher.UIThread.Post(() =>
                    {
                        if (BackendLabel == backendLabel)
                        {
                            BackendLabel = withVersion;
                        }
                    });
                }
                catch (Exception ex)
                {
                    Se.LogError(ex, "SpeechToTextEngineSettings: CrispASR version probe failed");
                }
            });
        }
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

    // Whisper.cpp variants live in their own folder per backend, so the choice is the backend.
    // CrispASR shares a single folder across variants — DownloadHashManager.Detect* reads the
    // sidecar / installed DLLs to figure out which backend is active.
    private static string DetectBackend(ISpeechToTextEngine engine, string folder)
    {
        if (engine is WhisperEngineCpp)
        {
            return Configuration.IsRunningOnLinux ? "Vulkan (Linux)"
                : Configuration.IsRunningOnMac ? "macOS native"
                : "BLAS (Windows CPU)";
        }
        if (engine is WhisperEngineCppCuBlas)
        {
            return "cuBLAS (CUDA)";
        }
        if (engine is WhisperEngineCppVulkan)
        {
            return "Vulkan";
        }

        if (engine is ICrispAsrEngine)
        {
            if (Configuration.IsRunningOnMac)
            {
                return "macOS universal";
            }

            if (Configuration.IsRunningOnLinux)
            {
                if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
                {
                    return "Linux ARM64 (CPU)";
                }
                var linuxVariant = DownloadHashManager.DetectCrispAsrLinuxVariant(folder);
                return linuxVariant switch
                {
                    "cuda" => "Linux x64 (CUDA)",
                    "cuda13" => "Linux x64 (CUDA 13)",
                    "vulkan" => "Linux x64 (Vulkan)",
                    "hip" => "Linux x64 (ROCm)",
                    _ => "Linux x64 (CPU)",
                };
            }

            var winVariant = DownloadHashManager.DetectCrispAsrWindowsVariant(folder);
            return winVariant switch
            {
                "cuda" => "Windows x64 (CUDA)",
                "vulkan" => "Windows x64 (Vulkan)",
                "cpu-legacy" => "Windows x64 (CPU, legacy)",
                "cpu" => "Windows x64 (CPU)",
                _ => "Windows x64",
            };
        }

        // Single-backend engines (CTranslate2 / ConstMe / Purfview): no variant to disambiguate -
        // just show the OS/arch the install is for so the row says something useful.
        if (Configuration.IsRunningOnMac)
        {
            return RuntimeInformation.ProcessArchitecture == Architecture.Arm64
                ? "macOS ARM64"
                : "macOS x64";
        }
        if (Configuration.IsRunningOnLinux)
        {
            return RuntimeInformation.ProcessArchitecture == Architecture.Arm64
                ? "Linux ARM64"
                : "Linux x64";
        }
        return "Windows x64";
    }

    private static DownloadHashManager.UpdateStatus ComputeStatus(ISpeechToTextEngine engine, string folder)
    {
        var lookup = TryReadSidecarHash(folder)
                     ?? (engine is ICrispAsrEngine
                         ? TryHashCrispAsrExecutable(engine, folder)
                         : TryHashWhisperCppExecutable(engine));

        return lookup is var (key, hash)
            ? DownloadHashManager.GetStatus(key, hash)
            : DownloadHashManager.UpdateStatus.Unknown;
    }

    private static (string key, string hash)? TryReadSidecarHash(string folder)
    {
        var sidecar = Path.Combine(folder, ".installed.sha256");
        if (!File.Exists(sidecar))
        {
            return null;
        }
        try
        {
            var lines = File.ReadAllLines(sidecar);
            if (lines.Length < 2)
            {
                return null;
            }
            var key = lines[0].Trim();
            var hash = lines[1].Trim();
            return (key.Length == 0 || hash.Length == 0) ? null : (key, hash);
        }
        catch
        {
            return null;
        }
    }

    private static (string key, string hash)? TryHashWhisperCppExecutable(ISpeechToTextEngine engine)
    {
        try
        {
            var key = DownloadHashManager.ResolveWhisperCppExecutableKey(engine.Choice);
            if (key == null)
            {
                return null;
            }
            var hash = Sha256Util.ComputeSha256(engine.GetExecutable());
            return hash == null ? null : (key, hash);
        }
        catch
        {
            return null;
        }
    }

    private static (string key, string hash)? TryHashCrispAsrExecutable(ISpeechToTextEngine engine, string folder)
    {
        try
        {
            string? variant = null;
            if (Configuration.IsRunningOnWindows)
            {
                variant = DownloadHashManager.DetectCrispAsrWindowsVariant(folder);
            }
            else if (Configuration.IsRunningOnLinux && RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
            {
                variant = DownloadHashManager.DetectCrispAsrLinuxVariant(folder);
            }
            var key = DownloadHashManager.ResolveCrispAsrExecutableKey(variant);
            if (key == null)
            {
                return null;
            }
            var hash = Sha256Util.ComputeSha256(engine.GetExecutable());
            return hash == null ? null : (key, hash);
        }
        catch
        {
            return null;
        }
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
            // best-effort
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
