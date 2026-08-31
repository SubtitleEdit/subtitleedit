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

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.PocketTtsCrispAsrSettings;

public partial class PocketTtsCrispAsrSettingsViewModel : ObservableObject
{
    // A SolidColorBrush set as Shape.Fill is parented into that shape's visual tree, so the
    // same instance can't be shared across multiple bound Ellipses (only one dot would render).
    // Construct a fresh brush per assignment instead.
    private static IBrush Green() => new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50));
    private static IBrush Amber() => new SolidColorBrush(Color.FromRgb(0xFF, 0x98, 0x00));
    private static IBrush Red() => new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36));
    private static IBrush Grey() => new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E));

    private readonly IWindowService _windowService;
    private readonly IFolderHelper _folderHelper;

    [ObservableProperty] private string _engineLabel = string.Empty;
    [ObservableProperty] private IBrush _engineBrush = Grey();
    [ObservableProperty] private string _engineDownloadButtonText = string.Empty;

    [ObservableProperty] private string _englishF16Label = string.Empty;
    [ObservableProperty] private IBrush _englishF16Brush = Grey();

    [ObservableProperty] private string _englishQ8_0Label = string.Empty;
    [ObservableProperty] private IBrush _englishQ8_0Brush = Grey();

    [ObservableProperty] private string _germanLabel = string.Empty;
    [ObservableProperty] private IBrush _germanBrush = Grey();

    [ObservableProperty] private string _spanishLabel = string.Empty;
    [ObservableProperty] private IBrush _spanishBrush = Grey();

    [ObservableProperty] private string _italianLabel = string.Empty;
    [ObservableProperty] private IBrush _italianBrush = Grey();

    [ObservableProperty] private string _portugueseLabel = string.Empty;
    [ObservableProperty] private IBrush _portugueseBrush = Grey();

    [ObservableProperty] private string _frenchLabel = string.Empty;
    [ObservableProperty] private IBrush _frenchBrush = Grey();

    [ObservableProperty] private string _voicesLabel = string.Empty;

    [ObservableProperty] private double _speed = 1.0;

    public string SpeedLabel => $"Speed {Speed:0.00}x";

    partial void OnSpeedChanged(double value)
    {
        OnPropertyChanged(nameof(SpeedLabel));
        Se.Settings.Video.TextToSpeech.PocketTtsCrispAsrSpeed = Math.Clamp(value, 0.25, 4.0);
    }

    [ObservableProperty] private string _modelsFolder = string.Empty;
    [ObservableProperty] private string _voicesFolder = string.Empty;
    [ObservableProperty] private bool _isEngineInstalled;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    public PocketTtsCrispAsrSettingsViewModel(IWindowService windowService, IFolderHelper folderHelper)
    {
        _windowService = windowService;
        _folderHelper = folderHelper;
    }

    public void Initialize()
    {
        ModelsFolder = PocketTtsCrispAsr.GetSetModelsFolder();
        VoicesFolder = PocketTtsCrispAsr.GetSetVoicesFolder();
        Speed = Math.Clamp(Se.Settings.Video.TextToSpeech.PocketTtsCrispAsrSpeed, 0.25, 4.0);
        Refresh();
    }

    private void Refresh()
    {
        var exe = PocketTtsCrispAsr.GetCrispAsrExecutable();
        IsEngineInstalled = File.Exists(exe);

        if (!IsEngineInstalled)
        {
            EngineLabel = string.Format(Se.Language.Video.TtsEngineNotInstalled, "CrispASR");
            EngineBrush = Red();
            EngineDownloadButtonText = string.Format(Se.Language.General.DownloadX, "CrispASR");
        }
        else if (PocketTtsCrispAsr.GetEngineUpdateStatus() == DownloadHashManager.UpdateStatus.UpdateAvailable)
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

        // Append the installed CrispASR version asynchronously - the probe is a child
        // process and we don't want to block the dialog opening (the probe is cached
        // after the first call but the first one can be a few hundred ms).
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
                    Se.LogError(ex, "PocketTtsCrispAsrSettings: CrispASR version probe failed");
                }
            });
        }

        // One GGUF per language, in the engine's models folder OR auto-downloaded by crispasr
        // into ~/.cache/crispasr. We can only verify the engine-folder copy here; when CrispASR
        // is installed a missing local file is fine ("Auto-download on first use"), and when it
        // is NOT installed that message would promise a download that can't happen yet.
        ApplyStatus(PocketTtsCrispAsr.ModelKeyEnglishF16, PocketTtsCrispAsr.EnglishF16FileName,
            label => EnglishF16Label = label, brush => EnglishF16Brush = brush);
        ApplyStatus(PocketTtsCrispAsr.ModelKeyEnglishQ8_0, PocketTtsCrispAsr.EnglishQ8_0FileName,
            label => EnglishQ8_0Label = label, brush => EnglishQ8_0Brush = brush);
        ApplyStatus(PocketTtsCrispAsr.ModelKeyGermanQ8_0, PocketTtsCrispAsr.GermanQ8_0FileName,
            label => GermanLabel = label, brush => GermanBrush = brush);
        ApplyStatus(PocketTtsCrispAsr.ModelKeySpanishQ8_0, PocketTtsCrispAsr.SpanishQ8_0FileName,
            label => SpanishLabel = label, brush => SpanishBrush = brush);
        ApplyStatus(PocketTtsCrispAsr.ModelKeyItalianQ8_0, PocketTtsCrispAsr.ItalianQ8_0FileName,
            label => ItalianLabel = label, brush => ItalianBrush = brush);
        ApplyStatus(PocketTtsCrispAsr.ModelKeyPortugueseQ8_0, PocketTtsCrispAsr.PortugueseQ8_0FileName,
            label => PortugueseLabel = label, brush => PortugueseBrush = brush);
        ApplyStatus(PocketTtsCrispAsr.ModelKeyFrenchQ8_0, PocketTtsCrispAsr.FrenchQ8_0FileName,
            label => FrenchLabel = label, brush => FrenchBrush = brush);

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

    private void ApplyStatus(string modelKey, string fileName, Action<string> setLabel, Action<IBrush> setBrush)
    {
        var path = PocketTtsCrispAsr.GetModelPath(modelKey);
        if (PocketTtsCrispAsr.IsValidLocalModelFile(path, fileName))
        {
            setLabel("Installed");
            setBrush(Green());
            return;
        }

        if (!IsEngineInstalled)
        {
            // No CrispASR runtime means there's nothing to auto-download into, so don't
            // promise a download that can't happen until the user installs CrispASR.
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

        // CrispASR is shared across all CrispASR-driven TTS engines; we use the Pocket TTS
        // entry point so prompts read "Pocket TTS (CrispASR)" rather than another engine's name
        // (the crispasr binary itself doesn't care which engine triggered the download).
        await TtsVoiceInstaller.EnsureCrispAsrForPocketTts(Window, _windowService, forceRedownload: true);
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
    private void OpenLicensePage()
    {
        UiUtil.OpenUrl("https://huggingface.co/kyutai/pocket-tts");
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
