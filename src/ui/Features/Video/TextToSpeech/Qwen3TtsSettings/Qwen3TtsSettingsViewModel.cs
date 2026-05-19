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

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Qwen3TtsSettings;

public partial class Qwen3TtsSettingsViewModel : ObservableObject
{
    private readonly IWindowService _windowService;
    private readonly IFolderHelper _folderHelper;

    [ObservableProperty] private string _backendLabel = string.Empty;
    [ObservableProperty] private string _statusLabel = string.Empty;
    [ObservableProperty] private IBrush _statusBrush = Brushes.Gray;
    [ObservableProperty] private string _releaseTag = Qwen3TtsCppDownloadService.ReleaseTag;
    [ObservableProperty] private string _installFolder = string.Empty;
    [ObservableProperty] private bool _isInstalled;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    public Qwen3TtsSettingsViewModel(IWindowService windowService, IFolderHelper folderHelper)
    {
        _windowService = windowService;
        _folderHelper = folderHelper;
    }

    public void Initialize()
    {
        InstallFolder = Qwen3TtsCpp.GetSetFolder();
        Refresh();
    }

    private void Refresh()
    {
        IsInstalled = File.Exists(Qwen3TtsCpp.GetExecutableFileName());
        BackendLabel = IsInstalled ? DetectInstalledBackend() : Se.Language.General.NotInstalled;
        ApplyStatus(IsInstalled ? Qwen3TtsCpp.GetEngineUpdateStatus() : DownloadHashManager.UpdateStatus.Unknown, IsInstalled);
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
    // which Windows variant is installed. Falls back to OS+arch on macOS and Linux where the
    // variant is unambiguous.
    private static string DetectInstalledBackend()
    {
        if (Configuration.IsRunningOnMac)
        {
            return "macOS arm64 (Metal)";
        }

        if (Configuration.IsRunningOnLinux)
        {
            return RuntimeInformation.ProcessArchitecture == Architecture.Arm64
                ? "Linux ARM64 (CPU)"
                : "Linux x64 (Vulkan)";
        }

        var folder = Qwen3TtsCpp.GetSetFolder();
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
                        DownloadHashManager.Qwen3TtsCpp.WindowsCpu => "Windows x64 (CPU)",
                        DownloadHashManager.Qwen3TtsCpp.WindowsVulkan => "Windows x64 (Vulkan)",
                        _ => "Windows x64",
                    };
                }
            }
            catch
            {
                // fall through
            }
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

        var variant = Qwen3TtsCppDownloadService.WindowsVariantVulkan;
        if (Configuration.IsRunningOnWindows)
        {
            var variantAnswer = await MessageBox.Show(
                Window,
                "Re-download Qwen3 TTS",
                $"{Environment.NewLine}Select the build to download:",
                MessageBoxButtons.Cancel,
                MessageBoxIcon.Question,
                "CPU",
                "Vulkan (GPU)",
                "CUDA (NVIDIA GPU)");

            if (variantAnswer == MessageBoxResult.None || variantAnswer == MessageBoxResult.Cancel)
            {
                return;
            }

            variant = variantAnswer switch
            {
                MessageBoxResult.Custom1 => Qwen3TtsCppDownloadService.WindowsVariantCpu,
                MessageBoxResult.Custom3 => Qwen3TtsCppDownloadService.WindowsVariantCuda,
                _ => Qwen3TtsCppDownloadService.WindowsVariantVulkan,
            };

            if (variant == Qwen3TtsCppDownloadService.WindowsVariantVulkan && !VulkanHelper.IsInstalled())
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
                "Re-download Qwen3 TTS",
                $"{Environment.NewLine}Download the latest Qwen3 TTS now?",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);
            if (answer != MessageBoxResult.Yes)
            {
                return;
            }
        }

        await _windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(Window, vm => vm.StartDownloadQwen3TtsCpp(variant));
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
