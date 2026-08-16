using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.IndexTts25AudioCppSettings;

public partial class IndexTts25AudioCppSettingsViewModel : ObservableObject
{
    // A SolidColorBrush set as Shape.Fill is parented into that shape's visual tree, so the
    // same instance can't be shared across bound Ellipses. Fresh brush per assignment.
    private static IBrush Green() => new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50));
    private static IBrush Amber() => new SolidColorBrush(Color.FromRgb(0xFF, 0x98, 0x00));
    private static IBrush Red() => new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36));
    private static IBrush Grey() => new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E));

    /// <summary>Shown in the emotion combo when no emotion conditioning is applied.</summary>
    public const string EmotionNone = "None";

    private readonly IWindowService _windowService;
    private readonly IFolderHelper _folderHelper;

    [ObservableProperty] private string _engineLabel = string.Empty;
    [ObservableProperty] private IBrush _engineBrush = Grey();
    [ObservableProperty] private string _engineDownloadButtonText = string.Empty;
    [ObservableProperty] private bool _isEngineInstalled;

    [ObservableProperty] private string _modelQ8_0Label = string.Empty;
    [ObservableProperty] private IBrush _modelQ8_0Brush = Grey();
    [ObservableProperty] private string _modelF16Label = string.Empty;
    [ObservableProperty] private IBrush _modelF16Brush = Grey();

    [ObservableProperty] private string _voicesLabel = string.Empty;
    [ObservableProperty] private string _modelsFolder = string.Empty;
    [ObservableProperty] private string _voicesFolder = string.Empty;

    public ObservableCollection<string> Emotions { get; } = new();

    [ObservableProperty] private string _selectedEmotion = EmotionNone;
    [ObservableProperty] private double _emotionAlpha = 0.8;
    [ObservableProperty] private double _speed = 1.0;

    /// <summary>
    /// Whether the emotion strength slider does anything — it is ignored with no emotion set.
    /// </summary>
    public bool IsEmotionAlphaEnabled => !IsNone(SelectedEmotion);

    public string EmotionAlphaLabel => $"Emotion strength {EmotionAlpha:0.00}";

    /// <summary>
    /// The UI talks about speed (right = faster), which is what everyone expects from a
    /// speaking-rate slider. IndexTTS-2.5 itself takes the reciprocal — `duration_factor`,
    /// where >1 means a LONGER output and therefore slower speech — so the two are inverted
    /// on the way in and out. Storing the model's own units keeps the request payload and the
    /// logs honest; only this dialog deals in speed.
    /// </summary>
    public string SpeedLabel => $"Speed {Speed:0.00}x";

    partial void OnSelectedEmotionChanged(string value)
    {
        Se.Settings.Video.TextToSpeech.IndexTts25AudioCppEmotion = IsNone(value) ? string.Empty : value.ToLowerInvariant();
        OnPropertyChanged(nameof(IsEmotionAlphaEnabled));
    }

    partial void OnEmotionAlphaChanged(double value)
    {
        OnPropertyChanged(nameof(EmotionAlphaLabel));
        Se.Settings.Video.TextToSpeech.IndexTts25AudioCppEmotionAlpha = Math.Clamp(value, 0.0, 1.0);
    }

    partial void OnSpeedChanged(double value)
    {
        OnPropertyChanged(nameof(SpeedLabel));
        var speed = Math.Clamp(value, 0.5, 2.0);
        Se.Settings.Video.TextToSpeech.IndexTts25AudioCppDurationFactor = Math.Clamp(1.0 / speed, 0.5, 2.0);
    }

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    public IndexTts25AudioCppSettingsViewModel(IWindowService windowService, IFolderHelper folderHelper)
    {
        _windowService = windowService;
        _folderHelper = folderHelper;
    }

    public void Initialize()
    {
        Emotions.Clear();
        Emotions.Add(EmotionNone);
        foreach (var emotion in IndexTts25AudioCpp.EmotionNames)
        {
            Emotions.Add(Capitalize(emotion));
        }

        var savedEmotion = Se.Settings.Video.TextToSpeech.IndexTts25AudioCppEmotion;
        SelectedEmotion = string.IsNullOrEmpty(savedEmotion) ? EmotionNone : Capitalize(savedEmotion);
        EmotionAlpha = Math.Clamp(Se.Settings.Video.TextToSpeech.IndexTts25AudioCppEmotionAlpha, 0.0, 1.0);

        var durationFactor = Math.Clamp(Se.Settings.Video.TextToSpeech.IndexTts25AudioCppDurationFactor, 0.5, 2.0);
        Speed = Math.Clamp(1.0 / durationFactor, 0.5, 2.0);

        ModelsFolder = IndexTts25AudioCpp.GetSetModelsFolder();
        VoicesFolder = IndexTts25AudioCpp.GetSetVoicesFolder();
        Refresh();
    }

    private void Refresh()
    {
        var exe = IndexTts25AudioCpp.GetServerExecutable();
        IsEngineInstalled = File.Exists(exe);

        var backend = Se.Settings.Video.TextToSpeech.IndexTts25AudioCppBackend;
        var backendSuffix = string.IsNullOrEmpty(backend) ? string.Empty : $" ({backend})";

        if (!IsEngineInstalled)
        {
            EngineLabel = string.Format(Se.Language.Video.TtsEngineNotInstalled, "audio.cpp");
            EngineBrush = Red();
            EngineDownloadButtonText = string.Format(Se.Language.General.DownloadX, "audio.cpp");
        }
        else if (IndexTts25AudioCpp.GetEngineUpdateStatus() == DownloadHashManager.UpdateStatus.UpdateAvailable)
        {
            EngineLabel = string.Format(Se.Language.Video.TtsEngineUpdateAvailable, "audio.cpp" + backendSuffix);
            EngineBrush = Amber();
            EngineDownloadButtonText = string.Format(Se.Language.Video.TtsUpdateX, "audio.cpp");
        }
        else
        {
            EngineLabel = "audio.cpp" + backendSuffix;
            EngineBrush = Green();
            EngineDownloadButtonText = string.Format(Se.Language.General.ReDownloadX, "audio.cpp");
        }

        ApplyModelStatus(
            IndexTts25AudioCpp.AreModelsInstalled(IndexTts25AudioCpp.ModelKeyQ8_0),
            label => ModelQ8_0Label = label,
            brush => ModelQ8_0Brush = brush);

        ApplyModelStatus(
            IndexTts25AudioCpp.AreModelsInstalled(IndexTts25AudioCpp.ModelKeyF16),
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

    /// <summary>
    /// Unlike the CrispASR engines there is no auto-download fallback: audio.cpp is pointed at
    /// a model directory and fails if the GGUF is not there, so a missing model is simply
    /// "Not downloaded" rather than "Auto-download on first use".
    /// </summary>
    private static void ApplyModelStatus(bool installed, Action<string> setLabel, Action<IBrush> setBrush)
    {
        if (installed)
        {
            setLabel("Installed");
            setBrush(Green());
            return;
        }

        setLabel("Not downloaded");
        setBrush(Grey());
    }

    private static bool IsNone(string? emotion) =>
        string.IsNullOrEmpty(emotion) || string.Equals(emotion, EmotionNone, StringComparison.OrdinalIgnoreCase);

    private static string Capitalize(string value) =>
        string.IsNullOrEmpty(value) ? value : char.ToUpperInvariant(value[0]) + value[1..];

    [RelayCommand]
    private async Task RedownloadEngine()
    {
        if (Window == null)
        {
            return;
        }

        await TtsVoiceInstaller.EnsureAudioCppForIndexTts25(Window, _windowService, forceRedownload: true);
        Refresh();
    }

    [RelayCommand]
    private async Task DownloadModel(string? modelKey)
    {
        if (Window == null)
        {
            return;
        }

        var resolved = IndexTts25AudioCpp.ResolveModelKey(modelKey);

        // Same gate as the install flow: nothing is fetched before the model licence is
        // accepted, including a download started from this dialog.
        if (!IndexTts25AudioCpp.IsLicenseAccepted())
        {
            var licenseResult = await _windowService.ShowDialogAsync<IndexTts25License.IndexTts25LicenseWindow, IndexTts25License.IndexTts25LicenseViewModel>(Window, _ => { });
            if (!licenseResult.OkPressed || !IndexTts25AudioCpp.IsLicenseAccepted())
            {
                return;
            }
        }

        await _windowService.ShowDialogAsync<DownloadTts.DownloadTtsWindow, DownloadTts.DownloadTtsViewModel>(
            Window, vm => vm.StartDownloadIndexTts25AudioCppModels(resolved));
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
