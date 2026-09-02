using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.DownloadTts;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.ModelLicense;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.AudioCppTtsSettings;

/// <summary>
/// Everything the shared audio.cpp settings dialog needs to know about one engine. Higgs
/// Audio v3 and Fish Audio S2 Pro expose no per-request knobs (both auto-handle language and
/// take their expression from the text itself), so their settings dialogs are the same shape:
/// runtime status, two model quants, voices, folders. IndexTTS 2.5 keeps its own dialog —
/// it has emotion and speed controls this one does not.
/// </summary>
public sealed record AudioCppTtsSettingsAdapter(
    string EngineName,
    string Description,
    string ModelKeyDefault,
    string ModelKeyAlt,
    Func<string?, string> ResolveModelKey,
    Func<string?, bool> AreModelsInstalled,
    Func<string> GetModelsFolder,
    Func<string> GetVoicesFolder,
    Func<bool> IsLicenseAccepted,
    ModelLicenseDefinition LicenseDefinition,
    Action<DownloadTtsViewModel, string> StartDownloadModels);

public static class AudioCppTtsSettingsAdapters
{
    public static AudioCppTtsSettingsAdapter Higgs { get; } = new(
        EngineName: "Higgs Audio v3 (audio.cpp)",
        Description: new HiggsTtsAudioCpp().Description,
        ModelKeyDefault: HiggsTtsAudioCpp.ModelKeyQ8_0,
        ModelKeyAlt: HiggsTtsAudioCpp.ModelKeyBf16,
        ResolveModelKey: HiggsTtsAudioCpp.ResolveModelKey,
        AreModelsInstalled: HiggsTtsAudioCpp.AreModelsInstalled,
        GetModelsFolder: HiggsTtsAudioCpp.GetSetModelsFolder,
        GetVoicesFolder: HiggsTtsAudioCpp.GetSetVoicesFolder,
        IsLicenseAccepted: HiggsTtsAudioCpp.IsLicenseAccepted,
        LicenseDefinition: HiggsTtsAudioCpp.LicenseDefinition,
        StartDownloadModels: (vm, modelKey) => vm.StartDownloadHiggsTtsAudioCppModels(modelKey));

    public static AudioCppTtsSettingsAdapter Fish { get; } = new(
        EngineName: "Fish Audio S2 Pro (audio.cpp)",
        Description: new FishTtsAudioCpp().Description,
        ModelKeyDefault: FishTtsAudioCpp.ModelKeyQ8_0,
        ModelKeyAlt: FishTtsAudioCpp.ModelKeyBf16,
        ResolveModelKey: FishTtsAudioCpp.ResolveModelKey,
        AreModelsInstalled: FishTtsAudioCpp.AreModelsInstalled,
        GetModelsFolder: FishTtsAudioCpp.GetSetModelsFolder,
        GetVoicesFolder: FishTtsAudioCpp.GetSetVoicesFolder,
        IsLicenseAccepted: FishTtsAudioCpp.IsLicenseAccepted,
        LicenseDefinition: FishTtsAudioCpp.LicenseDefinition,
        StartDownloadModels: (vm, modelKey) => vm.StartDownloadFishTtsAudioCppModels(modelKey));
}

public partial class AudioCppTtsSettingsViewModel : ObservableObject
{
    // A SolidColorBrush set as Shape.Fill is parented into that shape's visual tree, so the
    // same instance can't be shared across bound Ellipses. Fresh brush per assignment.
    private static IBrush Green() => new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50));
    private static IBrush Amber() => new SolidColorBrush(Color.FromRgb(0xFF, 0x98, 0x00));
    private static IBrush Red() => new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36));
    private static IBrush Grey() => new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E));

    private readonly IWindowService _windowService;
    private readonly IFolderHelper _folderHelper;

    [ObservableProperty] private string _engineLabel = string.Empty;
    [ObservableProperty] private IBrush _engineBrush = Grey();
    [ObservableProperty] private string _engineDownloadButtonText = string.Empty;
    [ObservableProperty] private bool _isEngineInstalled;

    [ObservableProperty] private string _modelDefaultLabel = string.Empty;
    [ObservableProperty] private IBrush _modelDefaultBrush = Grey();
    [ObservableProperty] private string _modelAltLabel = string.Empty;
    [ObservableProperty] private IBrush _modelAltBrush = Grey();

    [ObservableProperty] private string _voicesLabel = string.Empty;
    [ObservableProperty] private string _modelsFolder = string.Empty;
    [ObservableProperty] private string _voicesFolder = string.Empty;

    public AudioCppTtsSettingsAdapter Adapter { get; private set; } = AudioCppTtsSettingsAdapters.Higgs;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    public AudioCppTtsSettingsViewModel(IWindowService windowService, IFolderHelper folderHelper)
    {
        _windowService = windowService;
        _folderHelper = folderHelper;
    }

    public void Initialize(AudioCppTtsSettingsAdapter adapter)
    {
        Adapter = adapter;
        ModelsFolder = adapter.GetModelsFolder();
        VoicesFolder = adapter.GetVoicesFolder();
        Refresh();
    }

    private void Refresh()
    {
        var exe = AudioCppRuntime.GetServerExecutable();
        IsEngineInstalled = File.Exists(exe);

        var backend = Se.Settings.Video.TextToSpeech.IndexTts25AudioCppBackend;
        var backendSuffix = string.IsNullOrEmpty(backend) ? string.Empty : $" ({backend})";

        if (!IsEngineInstalled)
        {
            EngineLabel = string.Format(Se.Language.Video.TtsEngineNotInstalled, "audio.cpp");
            EngineBrush = Red();
            EngineDownloadButtonText = string.Format(Se.Language.General.DownloadX, "audio.cpp");
        }
        else if (DownloadHashManager.GetSidecarStatus(Path.GetDirectoryName(exe) ?? string.Empty) == DownloadHashManager.UpdateStatus.UpdateAvailable)
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
            Adapter.AreModelsInstalled(Adapter.ModelKeyDefault),
            label => ModelDefaultLabel = label,
            brush => ModelDefaultBrush = brush);

        ApplyModelStatus(
            Adapter.AreModelsInstalled(Adapter.ModelKeyAlt),
            label => ModelAltLabel = label,
            brush => ModelAltBrush = brush);

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

    [RelayCommand]
    private async Task RedownloadEngine()
    {
        if (Window == null)
        {
            return;
        }

        await TtsVoiceInstaller.EnsureAudioCppRuntime(Window, _windowService, forceRedownload: true, Adapter.EngineName);
        Refresh();
    }

    [RelayCommand]
    private async Task DownloadModel(string? modelKey)
    {
        if (Window == null)
        {
            return;
        }

        var resolved = Adapter.ResolveModelKey(modelKey);

        // Same gate as the install flow: nothing is fetched before the model licence is
        // accepted, including a download started from this dialog.
        if (!Adapter.IsLicenseAccepted())
        {
            var licenseResult = await _windowService.ShowDialogAsync<ModelLicenseWindow, ModelLicenseViewModel>(
                Window, vm => vm.Initialize(Adapter.LicenseDefinition));
            if (!licenseResult.OkPressed || !Adapter.IsLicenseAccepted())
            {
                return;
            }
        }

        await _windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(
            Window, vm => Adapter.StartDownloadModels(vm, resolved));
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
