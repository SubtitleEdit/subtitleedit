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

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.VoxCPM2CrispAsrSettings;

public partial class VoxCPM2CrispAsrSettingsViewModel : ObservableObject
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

    [ObservableProperty] private string _modelQ4KLabel = string.Empty;
    [ObservableProperty] private IBrush _modelQ4KBrush = Grey();

    [ObservableProperty] private string _modelF16Label = string.Empty;
    [ObservableProperty] private IBrush _modelF16Brush = Grey();

    [ObservableProperty] private string _voicesLabel = string.Empty;

    [ObservableProperty] private double _speed = 1.0;

    public string SpeedLabel => $"Speed {Speed:0.00}x";

    partial void OnSpeedChanged(double value)
    {
        OnPropertyChanged(nameof(SpeedLabel));
        Se.Settings.Video.TextToSpeech.VoxCPM2CrispAsrSpeed = Math.Clamp(value, 0.25, 4.0);
    }

    [ObservableProperty] private string _modelsFolder = string.Empty;
    [ObservableProperty] private string _voicesFolder = string.Empty;
    [ObservableProperty] private bool _isEngineInstalled;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    public VoxCPM2CrispAsrSettingsViewModel(IWindowService windowService, IFolderHelper folderHelper)
    {
        _windowService = windowService;
        _folderHelper = folderHelper;
    }

    public void Initialize()
    {
        ModelsFolder = VoxCPM2CrispAsr.GetSetModelsFolder();
        VoicesFolder = VoxCPM2CrispAsr.GetSetVoicesFolder();
        Speed = Math.Clamp(Se.Settings.Video.TextToSpeech.VoxCPM2CrispAsrSpeed, 0.25, 4.0);
        Refresh();
    }

    private void Refresh()
    {
        var exe = VoxCPM2CrispAsr.GetCrispAsrExecutable();
        IsEngineInstalled = File.Exists(exe);

        if (!IsEngineInstalled)
        {
            EngineLabel = "CrispASR not installed";
            EngineBrush = Red();
            EngineDownloadButtonText = "Download CrispASR";
        }
        else if (VoxCPM2CrispAsr.GetEngineUpdateStatus() == DownloadHashManager.UpdateStatus.UpdateAvailable)
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
                    Se.LogError(ex, "VoxCPM2CrispAsrSettings: CrispASR version probe failed");
                }
            });
        }

        ApplyModelStatus(
            VoxCPM2CrispAsr.AreModelsInstalled(VoxCPM2CrispAsr.ModelKeyQ4K),
            IsEngineInstalled,
            label => ModelQ4KLabel = label,
            brush => ModelQ4KBrush = brush);

        ApplyModelStatus(
            VoxCPM2CrispAsr.AreModelsInstalled(VoxCPM2CrispAsr.ModelKeyF16),
            IsEngineInstalled,
            label => ModelF16Label = label,
            brush => ModelF16Brush = brush);

        try
        {
            var wavCount = Directory.Exists(VoicesFolder)
                ? Directory.GetFiles(VoicesFolder, "*.wav").Length
                : 0;
            VoicesLabel = wavCount == 0
                ? "No voices imported"
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

        await TtsVoiceInstaller.EnsureCrispAsrForVoxCPM2(Window, _windowService, forceRedownload: true);
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
