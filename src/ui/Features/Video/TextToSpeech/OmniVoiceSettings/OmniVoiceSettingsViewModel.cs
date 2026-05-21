using System;
using System.IO;
using System.Linq;
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

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.OmniVoiceSettings;

public partial class OmniVoiceSettingsViewModel : ObservableObject
{
    private readonly IWindowService _windowService;
    private readonly IFolderHelper _folderHelper;

    [ObservableProperty] private string _backendLabel = string.Empty;
    [ObservableProperty] private string _statusLabel = string.Empty;
    [ObservableProperty] private IBrush _statusBrush = Brushes.Gray;
    [ObservableProperty] private string _releaseTag = OmniVoiceDownloadService.ReleaseTag;
    [ObservableProperty] private string _installFolder = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DownloadButtonLabel))]
    private bool _isInstalled;

    public string DownloadButtonLabel => IsInstalled
        ? Se.Language.General.Redownload
        : Se.Language.General.Download;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    public OmniVoiceSettingsViewModel(IWindowService windowService, IFolderHelper folderHelper)
    {
        _windowService = windowService;
        _folderHelper = folderHelper;
    }

    public void Initialize()
    {
        InstallFolder = OmniVoiceTtsCpp.GetSetFolder();
        Refresh();
    }

    private void Refresh()
    {
        IsInstalled = File.Exists(OmniVoiceTtsCpp.GetExecutableFileName());
        BackendLabel = IsInstalled ? DetectInstalledBackend() : Se.Language.General.NotInstalled;
        ApplyStatus(IsInstalled ? OmniVoiceTtsCpp.GetEngineUpdateStatus() : DownloadHashManager.UpdateStatus.Unknown, IsInstalled);
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

    // Reads the .installed.sha256 sidecar (written by DownloadTtsViewModel after unpack) to learn
    // which Windows variant is installed. Falls back to ggml-*.dll detection on Windows, and to
    // OS+arch on macOS and Linux where the variant is unambiguous.
    private static string DetectInstalledBackend()
    {
        if (Configuration.IsRunningOnMac)
        {
            return "macOS universal (CPU + Metal)";
        }

        if (Configuration.IsRunningOnLinux)
        {
            return RuntimeInformation.ProcessArchitecture == Architecture.Arm64
                ? "Linux ARM64 (CPU)"
                : "Linux x64 (CPU)";
        }

        var folder = OmniVoiceTtsCpp.GetSetFolder();
        var sidecar = Path.Combine(folder, ".installed.sha256");
        if (File.Exists(sidecar))
        {
            try
            {
                var key = File.ReadLines(sidecar).FirstOrDefault()?.Trim();
                if (!string.IsNullOrEmpty(key))
                {
                    return key switch
                    {
                        DownloadHashManager.OmniVoice.WindowsCuda => "Windows x64 (CUDA)",
                        DownloadHashManager.OmniVoice.WindowsVulkan => "Windows x64 (Vulkan)",
                        DownloadHashManager.OmniVoice.WindowsCpu => "Windows x64 (CPU)",
                        _ => "Windows x64",
                    };
                }
            }
            catch
            {
                // fall through to DLL detection
            }
        }

        if (File.Exists(Path.Combine(folder, "ggml-cuda.dll"))) return "Windows x64 (CUDA)";
        if (File.Exists(Path.Combine(folder, "ggml-vulkan.dll"))) return "Windows x64 (Vulkan)";
        return "Windows x64 (CPU)";
    }

    [RelayCommand]
    private async Task Redownload()
    {
        if (Window == null)
        {
            return;
        }

        var variant = OmniVoiceDownloadService.WindowsVariantVulkan;
        if (Configuration.IsRunningOnWindows)
        {
            var variantAnswer = await MessageBox.Show(
                Window,
                Se.Language.Video.TextToSpeech.ReDownloadOmniVoiceTts,
                Se.Language.Video.TextToSpeech.SelectTheBuildToDownload,
                MessageBoxButtons.Cancel,
                MessageBoxIcon.Question,
                "CPU",
                "Vulkan",
                "CUDA");

            if (variantAnswer == MessageBoxResult.None || variantAnswer == MessageBoxResult.Cancel)
            {
                return;
            }

            variant = variantAnswer switch
            {
                MessageBoxResult.Custom1 => OmniVoiceDownloadService.WindowsVariantCpu,
                MessageBoxResult.Custom3 => OmniVoiceDownloadService.WindowsVariantCuda,
                _ => OmniVoiceDownloadService.WindowsVariantVulkan,
            };

            if (variant == OmniVoiceDownloadService.WindowsVariantVulkan && !VulkanHelper.IsInstalled())
            {
                const string vulkanUrl = "https://vulkan.lunarg.com/sdk/home";
                var vulkanAnswer = await MessageBox.Show(
                    Window,
                    Se.Language.Video.TextToSpeech.VulkanRuntimeMayBeRequired,
                    string.Format(Se.Language.Video.TextToSpeech.VulkanRuntimeNotDetectedMessage, vulkanUrl),
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (vulkanAnswer == MessageBoxResult.No)
                {
                    UiUtil.OpenUrl(vulkanUrl);
                    return;
                }
                if (vulkanAnswer != MessageBoxResult.Yes)
                {
                    return;
                }
            }
        }
        else
        {
            var answer = await MessageBox.Show(
                Window,
                Se.Language.Video.TextToSpeech.ReDownloadOmniVoiceTts,
                Se.Language.Video.TextToSpeech.DownloadTheLatestOmniVoiceTtsPrompt,
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);
            if (answer != MessageBoxResult.Yes)
            {
                return;
            }
        }

        await _windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(Window, vm => vm.StartDownloadOmniVoice(variant));
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
