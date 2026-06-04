using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using Nikse.SubtitleEdit.Logic.Media;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.CosyVoice3CrispAsrSettings;

public partial class CosyVoice3CrispAsrSettingsViewModel : ObservableObject
{
    private static IBrush Green() => new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50));
    private static IBrush Amber() => new SolidColorBrush(Color.FromRgb(0xFF, 0x98, 0x00));
    private static IBrush Red() => new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36));
    private static IBrush Grey() => new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E));

    private readonly IWindowService _windowService;
    private readonly IFolderHelper _folderHelper;

    [ObservableProperty] private string _engineLabel = string.Empty;
    [ObservableProperty] private IBrush _engineBrush = Grey();
    [ObservableProperty] private string _engineDownloadButtonText = string.Empty;

    [ObservableProperty] private string _q4KBundleLabel = string.Empty;
    [ObservableProperty] private IBrush _q4KBundleBrush = Grey();

    [ObservableProperty] private string _f16BundleLabel = string.Empty;
    [ObservableProperty] private IBrush _f16BundleBrush = Grey();

    [ObservableProperty] private string _presetsLabel = string.Empty;
    [ObservableProperty] private string _voicesLabel = string.Empty;

    [ObservableProperty] private double _speed = 1.0;

    public string SpeedLabel => $"Speed {Speed:0.00}x";

    partial void OnSpeedChanged(double value)
    {
        OnPropertyChanged(nameof(SpeedLabel));
        Se.Settings.Video.TextToSpeech.CosyVoice3CrispAsrSpeed = Math.Clamp(value, 0.25, 4.0);
    }

    [ObservableProperty] private string _modelsFolder = string.Empty;
    [ObservableProperty] private string _voicesFolder = string.Empty;
    [ObservableProperty] private bool _isEngineInstalled;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    public CosyVoice3CrispAsrSettingsViewModel(IWindowService windowService, IFolderHelper folderHelper)
    {
        _windowService = windowService;
        _folderHelper = folderHelper;
    }

    public void Initialize()
    {
        ModelsFolder = CosyVoice3CrispAsr.GetSetModelsFolder();
        VoicesFolder = CosyVoice3CrispAsr.GetSetVoicesFolder();
        Speed = Math.Clamp(Se.Settings.Video.TextToSpeech.CosyVoice3CrispAsrSpeed, 0.25, 4.0);
        PresetsLabel = $"{CosyVoice3CrispAsr.Presets.Length} baked presets (zero-shot + FLEURS en/de/zh/ja/fr/es/ko)";
        Refresh();
    }

    private void Refresh()
    {
        var exe = CosyVoice3CrispAsr.GetCrispAsrExecutable();
        IsEngineInstalled = File.Exists(exe);

        if (!IsEngineInstalled)
        {
            EngineLabel = "CrispASR not installed";
            EngineBrush = Red();
            EngineDownloadButtonText = "Download CrispASR";
        }
        else if (CosyVoice3CrispAsr.GetEngineUpdateStatus() == DownloadHashManager.UpdateStatus.UpdateAvailable)
        {
            EngineLabel = "CrispASR - update available";
            EngineBrush = Amber();
            EngineDownloadButtonText = "Update CrispASR";
        }
        else
        {
            EngineLabel = "CrispASR";
            EngineBrush = Green();
            EngineDownloadButtonText = "Re-download CrispASR";
        }

        if (IsEngineInstalled)
        {
            var baseLabel = EngineLabel;
            _ = Task.Run(() =>
            {
                try
                {
                    var version = CrispAsrVersion.TryGet(exe);
                    if (string.IsNullOrEmpty(version))
                    {
                        return;
                    }
                    Dispatcher.UIThread.Post(() =>
                    {
                        if (EngineLabel == baseLabel)
                        {
                            EngineLabel = baseLabel.Replace("CrispASR", $"CrispASR v{version}");
                        }
                    });
                }
                catch (Exception ex)
                {
                    Se.LogError(ex, "CosyVoice3CrispAsrSettings: CrispASR version probe failed");
                }
            });
        }

        // CosyVoice3 needs 6 files per quant — the status row reports the bundle as a whole
        // (green only when every file is present + correct-sized).
        ApplyModelStatus(
            CosyVoice3CrispAsr.AreModelsInstalled(CosyVoice3CrispAsr.ModelKeyQ4K),
            IsEngineInstalled,
            label => Q4KBundleLabel = label,
            brush => Q4KBundleBrush = brush);

        ApplyModelStatus(
            CosyVoice3CrispAsr.AreModelsInstalled(CosyVoice3CrispAsr.ModelKeyF16),
            IsEngineInstalled,
            label => F16BundleLabel = label,
            brush => F16BundleBrush = brush);

        try
        {
            var wavCount = Directory.Exists(VoicesFolder)
                ? Directory.GetFiles(VoicesFolder, "*.wav").Length
                : 0;
            VoicesLabel = wavCount == 0
                ? "No cloned voices imported (baked presets always available)"
                : (wavCount == 1 ? "1 voice imported" : $"{wavCount} voices imported");
        }
        catch
        {
            VoicesLabel = string.Empty;
        }
    }

    private static void ApplyModelStatus(bool fileInstalled, bool engineInstalled, Action<string> setLabel, Action<IBrush> setBrush)
    {
        if (fileInstalled)
        {
            setLabel("Installed");
            setBrush(Green());
            return;
        }

        if (!engineInstalled)
        {
            setLabel("CrispASR required");
            setBrush(Grey());
            return;
        }

        setLabel("Auto-download on first use");
        setBrush(Grey());
    }

    [RelayCommand]
    private async Task RedownloadEngine()
    {
        if (Window == null)
        {
            return;
        }

        await TtsVoiceInstaller.EnsureCrispAsrForCosyVoice3(Window, _windowService, forceRedownload: true);
        Refresh();
    }

    [RelayCommand]
    private async Task OpenModelsFolder()
    {
        if (Window == null || string.IsNullOrEmpty(ModelsFolder))
        {
            return;
        }

        try
        {
            await _folderHelper.OpenFolder(Window, ModelsFolder);
        }
        catch
        {
            // best-effort UX
        }
    }

    [RelayCommand]
    private async Task OpenVoicesFolder()
    {
        if (Window == null || string.IsNullOrEmpty(VoicesFolder))
        {
            return;
        }

        try
        {
            await _folderHelper.OpenFolder(Window, VoicesFolder);
        }
        catch
        {
            // best-effort UX
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
