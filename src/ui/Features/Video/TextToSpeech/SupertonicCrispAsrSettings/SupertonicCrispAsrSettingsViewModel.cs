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

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.SupertonicCrispAsrSettings;

public partial class SupertonicCrispAsrSettingsViewModel : ObservableObject
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

    [ObservableProperty] private string _modelLabel = string.Empty;
    [ObservableProperty] private IBrush _modelBrush = Grey();

    [ObservableProperty] private double _speed = 1.0;

    public string SpeedLabel => $"Speed {Speed:0.00}x";

    partial void OnSpeedChanged(double value)
    {
        OnPropertyChanged(nameof(SpeedLabel));
        Se.Settings.Video.TextToSpeech.SupertonicCrispAsrSpeed = Math.Clamp(value, 0.25, 4.0);
    }

    [ObservableProperty] private string _modelsFolder = string.Empty;
    [ObservableProperty] private bool _isEngineInstalled;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    public SupertonicCrispAsrSettingsViewModel(IWindowService windowService, IFolderHelper folderHelper)
    {
        _windowService = windowService;
        _folderHelper = folderHelper;
    }

    public void Initialize()
    {
        ModelsFolder = SupertonicCrispAsr.GetSetModelsFolder();
        Speed = Math.Clamp(Se.Settings.Video.TextToSpeech.SupertonicCrispAsrSpeed, 0.25, 4.0);
        Refresh();
    }

    private void Refresh()
    {
        var exe = SupertonicCrispAsr.GetCrispAsrExecutable();
        IsEngineInstalled = File.Exists(exe);

        if (!IsEngineInstalled)
        {
            EngineLabel = string.Format(Se.Language.Video.TtsEngineNotInstalled, "CrispASR");
            EngineBrush = Red();
            EngineDownloadButtonText = string.Format(Se.Language.General.DownloadX, "CrispASR");
        }
        else if (SupertonicCrispAsr.GetEngineUpdateStatus() == DownloadHashManager.UpdateStatus.UpdateAvailable)
        {
            EngineLabel = string.Format(Se.Language.Video.TtsEngineUpdateAvailable, "CrispASR");
            EngineBrush = Amber();
            EngineDownloadButtonText = string.Format(Se.Language.General.UpdateX, "CrispASR");
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
                    Se.LogError(ex, "SupertonicCrispAsrSettings: CrispASR version probe failed");
                }
            });
        }

        // The single GGUF lives in the engine's models folder OR is auto-downloaded by crispasr
        // into ~/.cache/crispasr. We can only verify the engine-folder copy here; when CrispASR
        // is installed a missing local file is fine ("Auto-download on first use"), and when it
        // is NOT installed that message would promise a download that can't happen yet.
        if (SupertonicCrispAsr.IsModelInstalled())
        {
            ModelLabel = "Installed";
            ModelBrush = Green();
        }
        else
        {
            ModelLabel = IsEngineInstalled ? "Auto-download on first use" : "CrispASR required";
            ModelBrush = Grey();
        }
    }

    [RelayCommand]
    private async Task RedownloadEngine()
    {
        if (Window == null)
        {
            return;
        }

        // CrispASR is shared across all CrispASR-driven TTS engines; we use the Supertonic
        // entry point so prompts read "Supertonic (CrispASR)" rather than another engine's name
        // (the crispasr binary itself doesn't care which engine triggered the download).
        await TtsVoiceInstaller.EnsureCrispAsrForSupertonic(Window, _windowService, forceRedownload: true);
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
    private void OpenLicensePage()
    {
        UiUtil.OpenUrl("https://huggingface.co/Supertone/supertonic-3");
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
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/text-to-speech", "engine-settings");
        }
    }
}
