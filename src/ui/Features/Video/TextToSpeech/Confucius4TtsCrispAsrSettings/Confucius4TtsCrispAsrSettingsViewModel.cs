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

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Confucius4TtsCrispAsrSettings;

public partial class Confucius4TtsCrispAsrSettingsViewModel : ObservableObject
{
    // A SolidColorBrush set as Shape.Fill is parented into that shape's visual tree, so the same
    // instance can't be shared across bound Ellipses (only one dot would render). Fresh brush per
    // assignment instead.
    private static IBrush Green() => new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50));
    private static IBrush Amber() => new SolidColorBrush(Color.FromRgb(0xFF, 0x98, 0x00));
    private static IBrush Red() => new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36));
    private static IBrush Grey() => new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E));

    private readonly IWindowService _windowService;
    private readonly IFolderHelper _folderHelper;

    [ObservableProperty] private string _engineLabel = string.Empty;
    [ObservableProperty] private IBrush _engineBrush = Grey();
    [ObservableProperty] private string _engineDownloadButtonText = string.Empty;

    [ObservableProperty] private string _coreQ8_0Label = string.Empty;
    [ObservableProperty] private IBrush _coreQ8_0Brush = Grey();

    [ObservableProperty] private string _coreF16Label = string.Empty;
    [ObservableProperty] private IBrush _coreF16Brush = Grey();

    [ObservableProperty] private string _vocoderLabel = string.Empty;
    [ObservableProperty] private IBrush _vocoderBrush = Grey();

    [ObservableProperty] private string _w2vLabel = string.Empty;
    [ObservableProperty] private IBrush _w2vBrush = Grey();

    [ObservableProperty] private string _voicesLabel = string.Empty;

    [ObservableProperty] private double _odeSteps = Confucius4TtsCrispAsr.DefaultOdeSteps;

    public string OdeStepsLabel => $"{(int)OdeSteps} steps";

    partial void OnOdeStepsChanged(double value)
    {
        OnPropertyChanged(nameof(OdeStepsLabel));
        Se.Settings.Video.TextToSpeech.Confucius4TtsCrispAsrOdeSteps =
            Math.Clamp((int)Math.Round(value), Confucius4TtsCrispAsr.MinOdeSteps, Confucius4TtsCrispAsr.MaxOdeSteps);
    }

    [ObservableProperty] private string _modelsFolder = string.Empty;
    [ObservableProperty] private string _voicesFolder = string.Empty;
    [ObservableProperty] private bool _isEngineInstalled;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    public Confucius4TtsCrispAsrSettingsViewModel(IWindowService windowService, IFolderHelper folderHelper)
    {
        _windowService = windowService;
        _folderHelper = folderHelper;
    }

    public void Initialize()
    {
        ModelsFolder = Confucius4TtsCrispAsr.GetSetModelsFolder();
        VoicesFolder = Confucius4TtsCrispAsr.GetSetVoicesFolder();
        OdeSteps = Confucius4TtsCrispAsr.ResolveOdeSteps();
        Refresh();
    }

    private void Refresh()
    {
        var exe = Confucius4TtsCrispAsr.GetCrispAsrExecutable();
        IsEngineInstalled = File.Exists(exe);

        if (!IsEngineInstalled)
        {
            EngineLabel = string.Format(Se.Language.Video.TtsEngineNotInstalled, "CrispASR");
            EngineBrush = Red();
            EngineDownloadButtonText = string.Format(Se.Language.General.DownloadX, "CrispASR");
        }
        else if (Confucius4TtsCrispAsr.GetEngineUpdateStatus() == DownloadHashManager.UpdateStatus.UpdateAvailable)
        {
            EngineLabel = string.Format(Se.Language.Video.TtsEngineUpdateAvailable, "CrispASR");
            EngineBrush = Amber();
            EngineDownloadButtonText = string.Format(Se.Language.Video.TtsUpdateX, "CrispASR");
        }
        else
        {
            EngineLabel = "CrispASR";
            EngineBrush = Green();
            EngineDownloadButtonText = string.Format(Se.Language.General.ReDownloadX, "CrispASR");
        }

        // Append the installed CrispASR version asynchronously — the probe spawns a child process
        // and the first call can take a few hundred ms.
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
                    Se.LogError(ex, "Confucius4TtsCrispAsrSettings: CrispASR version probe failed");
                }
            });
        }

        var coreQ8_0Ok = Confucius4TtsCrispAsr.IsValidLocalModelFile(
                Confucius4TtsCrispAsr.GetT2sPath(Confucius4TtsCrispAsr.ModelKeyQ8_0), Confucius4TtsCrispAsr.T2sQ8_0FileName)
            && Confucius4TtsCrispAsr.IsValidLocalModelFile(
                Confucius4TtsCrispAsr.GetS2aPath(Confucius4TtsCrispAsr.ModelKeyQ8_0), Confucius4TtsCrispAsr.S2aQ8_0FileName);
        ApplyModelStatus(coreQ8_0Ok,
            IsEngineInstalled,
            label => CoreQ8_0Label = label,
            brush => CoreQ8_0Brush = brush);

        var coreF16Ok = Confucius4TtsCrispAsr.IsValidLocalModelFile(
                Confucius4TtsCrispAsr.GetT2sPath(Confucius4TtsCrispAsr.ModelKeyF16), Confucius4TtsCrispAsr.T2sF16FileName)
            && Confucius4TtsCrispAsr.IsValidLocalModelFile(
                Confucius4TtsCrispAsr.GetS2aPath(Confucius4TtsCrispAsr.ModelKeyF16), Confucius4TtsCrispAsr.S2aF16FileName);
        ApplyModelStatus(coreF16Ok,
            IsEngineInstalled,
            label => CoreF16Label = label,
            brush => CoreF16Brush = brush);

        var vocoderPath = Confucius4TtsCrispAsr.GetVocoderPath();
        ApplyModelStatus(Confucius4TtsCrispAsr.IsValidLocalModelFile(vocoderPath, Confucius4TtsCrispAsr.VocoderFileName),
            IsEngineInstalled,
            label => VocoderLabel = label,
            brush => VocoderBrush = brush);

        var w2vPath = Confucius4TtsCrispAsr.GetW2vPath();
        ApplyModelStatus(Confucius4TtsCrispAsr.IsValidLocalModelFile(w2vPath, Confucius4TtsCrispAsr.W2vFileName),
            IsEngineInstalled,
            label => W2vLabel = label,
            brush => W2vBrush = brush);

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
            // No CrispASR runtime means there is nothing to auto-download into, so don't promise
            // a download that can't happen until the user installs CrispASR.
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

        await TtsVoiceInstaller.EnsureCrispAsrForConfucius4Tts(Window, _windowService, forceRedownload: true);
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
