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

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.DotsTtsCrispAsrSettings;

public partial class DotsTtsCrispAsrSettingsViewModel : ObservableObject
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

    [ObservableProperty] private string _coreQ4KLabel = string.Empty;
    [ObservableProperty] private IBrush _coreQ4KBrush = Grey();

    [ObservableProperty] private string _coreQ8_0Label = string.Empty;
    [ObservableProperty] private IBrush _coreQ8_0Brush = Grey();

    [ObservableProperty] private string _coreF16Label = string.Empty;
    [ObservableProperty] private IBrush _coreF16Brush = Grey();

    [ObservableProperty] private string _vocoderLabel = string.Empty;
    [ObservableProperty] private IBrush _vocoderBrush = Grey();

    [ObservableProperty] private string _speakerEncoderLabel = string.Empty;
    [ObservableProperty] private IBrush _speakerEncoderBrush = Grey();

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
            EngineBrush = Red();
            EngineDownloadButtonText = string.Format(Se.Language.General.DownloadX, "CrispASR");
        }
        else if (DotsTtsCrispAsr.GetEngineUpdateStatus() == DownloadHashManager.UpdateStatus.UpdateAvailable)
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
                    Se.LogError(ex, "DotsTtsCrispAsrSettings: CrispASR version probe failed");
                }
            });
        }

        var coreQ4K = DotsTtsCrispAsr.GetCorePath(DotsTtsCrispAsr.ModelKeyQ4K);
        ApplyModelStatus(DotsTtsCrispAsr.IsValidLocalModelFile(coreQ4K, DotsTtsCrispAsr.CoreQ4KFileName),
            IsEngineInstalled,
            label => CoreQ4KLabel = label,
            brush => CoreQ4KBrush = brush);

        var coreQ8_0 = DotsTtsCrispAsr.GetCorePath(DotsTtsCrispAsr.ModelKeyQ8_0);
        ApplyModelStatus(DotsTtsCrispAsr.IsValidLocalModelFile(coreQ8_0, DotsTtsCrispAsr.CoreQ8_0FileName),
            IsEngineInstalled,
            label => CoreQ8_0Label = label,
            brush => CoreQ8_0Brush = brush);

        var coreF16 = DotsTtsCrispAsr.GetCorePath(DotsTtsCrispAsr.ModelKeyF16);
        ApplyModelStatus(DotsTtsCrispAsr.IsValidLocalModelFile(coreF16, DotsTtsCrispAsr.CoreF16FileName),
            IsEngineInstalled,
            label => CoreF16Label = label,
            brush => CoreF16Brush = brush);

        var vocoderPath = DotsTtsCrispAsr.GetVocoderPath();
        ApplyModelStatus(DotsTtsCrispAsr.IsValidLocalModelFile(vocoderPath, DotsTtsCrispAsr.VocoderFileName),
            IsEngineInstalled,
            label => VocoderLabel = label,
            brush => VocoderBrush = brush);

        var speakerPath = DotsTtsCrispAsr.GetSpeakerEncoderPath();
        ApplyModelStatus(DotsTtsCrispAsr.IsValidLocalModelFile(speakerPath, DotsTtsCrispAsr.SpeakerEncoderFileName),
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

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }
}
