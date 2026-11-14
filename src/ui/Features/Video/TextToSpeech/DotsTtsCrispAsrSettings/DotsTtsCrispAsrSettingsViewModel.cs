using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using Nikse.SubtitleEdit.Logic.Media;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.DotsTtsCrispAsrSettings;

public partial class DotsTtsCrispAsrSettingsViewModel : ObservableObject
{

    private readonly IWindowService _windowService;
    private readonly IFolderHelper _folderHelper;

    [ObservableProperty] private string _engineLabel = string.Empty;
    [ObservableProperty] private IBrush _engineBrush = TtsSettingsStatus.Grey();
    [ObservableProperty] private string _engineDownloadButtonText = string.Empty;

    [ObservableProperty] private string _coreQ4KLabel = string.Empty;
    [ObservableProperty] private IBrush _coreQ4KBrush = TtsSettingsStatus.Grey();

    [ObservableProperty] private string _coreQ8_0Label = string.Empty;
    [ObservableProperty] private IBrush _coreQ8_0Brush = TtsSettingsStatus.Grey();

    [ObservableProperty] private string _coreF16Label = string.Empty;
    [ObservableProperty] private IBrush _coreF16Brush = TtsSettingsStatus.Grey();

    [ObservableProperty] private string _vocoderLabel = string.Empty;
    [ObservableProperty] private IBrush _vocoderBrush = TtsSettingsStatus.Grey();

    [ObservableProperty] private string _speakerEncoderLabel = string.Empty;
    [ObservableProperty] private IBrush _speakerEncoderBrush = TtsSettingsStatus.Grey();

    [ObservableProperty] private string _voicesLabel = string.Empty;

    [ObservableProperty] private double _odeSteps = DotsTtsCrispAsr.DefaultOdeSteps;

    public string OdeStepsLabel => $"{(int)OdeSteps} steps";

    partial void OnOdeStepsChanged(double value)
    {
        OnPropertyChanged(nameof(OdeStepsLabel));
        Se.Settings.Video.TextToSpeech.DotsTtsCrispAsrOdeSteps =
            Math.Clamp((int)Math.Round(value), DotsTtsCrispAsr.MinOdeSteps, DotsTtsCrispAsr.MaxOdeSteps);
    }

    [ObservableProperty] private string _modelsFolder = string.Empty;
    [ObservableProperty] private string _voicesFolder = string.Empty;
    [ObservableProperty] private bool _isEngineInstalled;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    public DotsTtsCrispAsrSettingsViewModel(IWindowService windowService, IFolderHelper folderHelper)
    {
        _windowService = windowService;
        _folderHelper = folderHelper;
    }

    public void Initialize()
    {
        ModelsFolder = DotsTtsCrispAsr.GetSetModelsFolder();
        VoicesFolder = DotsTtsCrispAsr.GetSetVoicesFolder();
        OdeSteps = DotsTtsCrispAsr.ResolveOdeSteps();
        Refresh();
    }

    private void Refresh()
    {
        var exe = DotsTtsCrispAsr.GetCrispAsrExecutable();
        IsEngineInstalled = File.Exists(exe);

        if (!IsEngineInstalled)
        {
            EngineLabel = string.Format(Se.Language.Video.TtsEngineNotInstalled, "CrispASR");
            EngineBrush = TtsSettingsStatus.Red();
            EngineDownloadButtonText = string.Format(Se.Language.General.DownloadX, "CrispASR");
        }
        else if (DotsTtsCrispAsr.GetEngineUpdateStatus() == DownloadHashManager.UpdateStatus.UpdateAvailable)
        {
            EngineLabel = string.Format(Se.Language.Video.TtsEngineUpdateAvailable, "CrispASR");
            EngineBrush = TtsSettingsStatus.Amber();
            EngineDownloadButtonText = string.Format(Se.Language.General.UpdateX, "CrispASR");
        }
        else
        {
            EngineLabel = "CrispASR";
            EngineBrush = TtsSettingsStatus.Green();
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
                    Se.LogError(ex, "DotsTtsCrispAsrSettings: CrispASR version probe failed");
                }
            });
        }

        var coreQ4K = DotsTtsCrispAsr.GetCorePath(DotsTtsCrispAsr.ModelKeyQ4K);
        TtsSettingsStatus.ApplyModelStatus(DotsTtsCrispAsr.IsValidLocalModelFile(coreQ4K, DotsTtsCrispAsr.CoreQ4KFileName),
            IsEngineInstalled,
            label => CoreQ4KLabel = label,
            brush => CoreQ4KBrush = brush);

        var coreQ8_0 = DotsTtsCrispAsr.GetCorePath(DotsTtsCrispAsr.ModelKeyQ8_0);
        TtsSettingsStatus.ApplyModelStatus(DotsTtsCrispAsr.IsValidLocalModelFile(coreQ8_0, DotsTtsCrispAsr.CoreQ8_0FileName),
            IsEngineInstalled,
            label => CoreQ8_0Label = label,
            brush => CoreQ8_0Brush = brush);

        var coreF16 = DotsTtsCrispAsr.GetCorePath(DotsTtsCrispAsr.ModelKeyF16);
        TtsSettingsStatus.ApplyModelStatus(DotsTtsCrispAsr.IsValidLocalModelFile(coreF16, DotsTtsCrispAsr.CoreF16FileName),
            IsEngineInstalled,
            label => CoreF16Label = label,
            brush => CoreF16Brush = brush);

        var vocoderPath = DotsTtsCrispAsr.GetVocoderPath();
        TtsSettingsStatus.ApplyModelStatus(DotsTtsCrispAsr.IsValidLocalModelFile(vocoderPath, DotsTtsCrispAsr.VocoderFileName),
            IsEngineInstalled,
            label => VocoderLabel = label,
            brush => VocoderBrush = brush);

        var speakerPath = DotsTtsCrispAsr.GetSpeakerEncoderPath();
        TtsSettingsStatus.ApplyModelStatus(DotsTtsCrispAsr.IsValidLocalModelFile(speakerPath, DotsTtsCrispAsr.SpeakerEncoderFileName),
            IsEngineInstalled,
            label => SpeakerEncoderLabel = label,
            brush => SpeakerEncoderBrush = brush);

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

    [RelayCommand]
    private async Task RedownloadEngine()
    {
        if (Window == null)
        {
            return;
        }

        await TtsVoiceInstaller.EnsureCrispAsrForDotsTts(Window, _windowService, forceRedownload: true);
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

    internal void OnKeyDown(KeyEventArgs e) => UiUtil.CloseOnEscapeOrShowHelp(e, Window, "features/text-to-speech", "engine-settings");
}
