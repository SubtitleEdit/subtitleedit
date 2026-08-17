using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.DownloadTts;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using Nikse.SubtitleEdit.Logic.Media;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.ChatterboxTtsSettings;

public partial class ChatterboxTtsSettingsViewModel : ObservableObject
{
    private readonly IWindowService _windowService;
    private readonly IFolderHelper _folderHelper;

    [ObservableProperty] private string _engineLabel = string.Empty;
    [ObservableProperty] private IBrush _engineBrush = Brushes.Gray;
    [ObservableProperty] private string _engineDownloadButtonText = string.Empty;
    [ObservableProperty] private string _modelsFolder = string.Empty;
    [ObservableProperty] private string _voicesFolder = string.Empty;
    [ObservableProperty] private bool _isEngineInstalled;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    /// <summary>
    /// The Base pair in its three quantizations plus Turbo, each with its own install state
    /// and download button. Quantization names are technical identifiers, so they are appended
    /// to the translated "Base model" label rather than translated themselves.
    /// </summary>
    public ObservableCollection<ChatterboxModelStatusViewModel> Models { get; } = new();

    /// <summary>
    /// Language spoken in imported reference WAVs, sent as the request's <c>source_lang</c> field
    /// so cross-lingual cloning engages when the target language differs. "Auto" (empty code)
    /// sends no field, which leaves the clone speaking the target language with the reference
    /// language's accent. Chatterbox clones from the WAV alone and never asks for a transcript,
    /// so unlike CosyVoice3 there is usually nothing to detect this from.
    /// </summary>
    public ObservableCollection<TtsLanguage> SourceLanguages { get; } = new(ChatterboxLanguages.All);

    [ObservableProperty] private TtsLanguage? _selectedSourceLanguage;

    partial void OnSelectedSourceLanguageChanged(TtsLanguage? value)
    {
        Se.Settings.Video.TextToSpeech.ChatterboxCrispAsrSourceLanguage = value?.Code ?? string.Empty;
    }

    public ChatterboxTtsSettingsViewModel(IWindowService windowService, IFolderHelper folderHelper)
    {
        _windowService = windowService;
        _folderHelper = folderHelper;

        foreach (var key in ChatterboxTtsCppDownloadService.GetAllModelKeys())
        {
            Models.Add(new ChatterboxModelStatusViewModel(key, GetDisplayName(key), RedownloadModelsCore));
        }
    }

    private static string GetDisplayName(string modelKey) => modelKey switch
    {
        ChatterboxTtsCppDownloadService.ModelKeyBaseF16 => $"{Se.Language.Video.BaseModel} (F16)",
        ChatterboxTtsCppDownloadService.ModelKeyBaseQ4K => $"{Se.Language.Video.BaseModel} (Q4_K)",
        ChatterboxTtsCppDownloadService.ModelKeyTurbo => Se.Language.Video.TurboModel,
        _ => $"{Se.Language.Video.BaseModel} (Q8_0)",
    };

    public void Initialize()
    {
        ModelsFolder = ChatterboxTtsCpp.GetSetModelsFolder();
        VoicesFolder = ChatterboxTtsCpp.GetSetVoicesFolder();
        var savedSourceLanguage = Se.Settings.Video.TextToSpeech.ChatterboxCrispAsrSourceLanguage;
        SelectedSourceLanguage =
            SourceLanguages.FirstOrDefault(l => l.Code == savedSourceLanguage && !string.IsNullOrEmpty(l.Code))
            ?? SourceLanguages.FirstOrDefault();
        Refresh();
    }

    private void Refresh()
    {
        var exe = ChatterboxTtsCpp.GetCrispAsrExecutable();
        IsEngineInstalled = File.Exists(exe);

        if (!IsEngineInstalled)
        {
            EngineLabel = string.Format(Se.Language.Video.TtsEngineNotInstalled, "CrispASR");
            EngineBrush = new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36)); // red
            EngineDownloadButtonText = string.Format(Se.Language.General.DownloadX, "CrispASR");
        }
        else if (!ChatterboxTtsCpp.IsCrispAsrChatterboxCapable())
        {
            EngineLabel = string.Format(Se.Language.Video.TtsEngineTooOldUpdateRequired, "CrispASR");
            EngineBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0x98, 0x00)); // amber
            EngineDownloadButtonText = string.Format(Se.Language.Video.TtsUpdateX, "CrispASR");
        }
        else if (ChatterboxTtsCpp.GetEngineUpdateStatus() == DownloadHashManager.UpdateStatus.UpdateAvailable)
        {
            EngineLabel = string.Format(Se.Language.Video.TtsEngineUpdateAvailable, "CrispASR");
            EngineBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0x98, 0x00)); // amber
            EngineDownloadButtonText = string.Format(Se.Language.Video.TtsUpdateX, "CrispASR");
        }
        else
        {
            EngineLabel = string.Format(Se.Language.Video.TtsEngineChatterboxCapable, "CrispASR");
            EngineBrush = new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50)); // green
            EngineDownloadButtonText = string.Format(Se.Language.General.ReDownloadX, "CrispASR");
        }

        // Append the installed CrispASR version asynchronously so the `crispasr --version`
        // probe doesn't block the dialog opening. Label gets patched in once the result is
        // available. Skip when CrispASR isn't installed (nothing to probe).
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
                    Se.LogError(ex, "ChatterboxTtsSettings: CrispASR version probe failed");
                }
            });
        }

        foreach (var model in Models)
        {
            var installed = ChatterboxTtsCpp.AreModelsInstalled(model.ModelKey);
            model.StatusLabel = installed ? Se.Language.General.Installed : Se.Language.General.NotInstalled;
            model.StatusBrush = installed
                ? new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50))  // green
                : new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E)); // grey
            model.DownloadButtonText = installed
                ? string.Format(Se.Language.General.ReDownloadX, model.ModelKey)
                : string.Format(Se.Language.General.DownloadX, model.ModelKey);
        }
    }

    private async Task RedownloadModelsCore(string modelKey)
    {
        var sizeText = ChatterboxTtsCppDownloadService.GetDownloadSizeText(modelKey);
        if (Window == null)
        {
            return;
        }

        var installed = ChatterboxTtsCpp.AreModelsInstalled(modelKey);
        var title = installed ? "Re-download Chatterbox TTS models" : "Download Chatterbox TTS models";

        var answer = await MessageBox.Show(
            Window,
            title,
            $"{Environment.NewLine}Download the Chatterbox TTS \"{modelKey}\" models ({sizeText}) now?",
            MessageBoxButtons.YesNoCancel,
            MessageBoxIcon.Question);
        if (answer != MessageBoxResult.Yes)
        {
            return;
        }

        await _windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(Window, vm => vm.StartDownloadChatterboxModels(modelKey));
        Refresh();
    }

    [RelayCommand]
    private async Task RedownloadEngine()
    {
        if (Window == null)
        {
            return;
        }

        // CrispASR is shared with Speech-to-text; this runs the same download flow as the
        // "Generate speech" path, then refreshes the status shown here.
        await TtsVoiceInstaller.EnsureCrispAsrForChatterbox(Window, _windowService, forceRedownload: true);
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
            // ignore - best-effort UX
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
