using System;
using System.IO;
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
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using Nikse.SubtitleEdit.Logic.Media;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Qwen3TtsCrispAsrSettings;

public partial class Qwen3TtsCrispAsrSettingsViewModel : ObservableObject
{
    private static readonly IBrush Green = new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50));
    private static readonly IBrush Amber = new SolidColorBrush(Color.FromRgb(0xFF, 0x98, 0x00));
    private static readonly IBrush Red = new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36));
    private static readonly IBrush Grey = new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E));

    private readonly IWindowService _windowService;
    private readonly IFolderHelper _folderHelper;

    [ObservableProperty] private string _engineLabel = string.Empty;
    [ObservableProperty] private IBrush _engineBrush = Grey;
    [ObservableProperty] private string _engineDownloadButtonText = string.Empty;

    [ObservableProperty] private string _voiceDesignTalkerLabel = string.Empty;
    [ObservableProperty] private IBrush _voiceDesignTalkerBrush = Grey;

    [ObservableProperty] private string _customVoiceTalkerLabel = string.Empty;
    [ObservableProperty] private IBrush _customVoiceTalkerBrush = Grey;

    [ObservableProperty] private string _codecLabel = string.Empty;
    [ObservableProperty] private IBrush _codecBrush = Grey;

    [ObservableProperty] private string _voicesLabel = string.Empty;

    [ObservableProperty] private string _modelsFolder = string.Empty;
    [ObservableProperty] private string _voicesFolder = string.Empty;
    [ObservableProperty] private bool _isEngineInstalled;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    public Qwen3TtsCrispAsrSettingsViewModel(IWindowService windowService, IFolderHelper folderHelper)
    {
        _windowService = windowService;
        _folderHelper = folderHelper;
    }

    public void Initialize()
    {
        ModelsFolder = Qwen3TtsCrispAsr.GetSetModelsFolder();
        VoicesFolder = Qwen3TtsCrispAsr.GetSetVoicesFolder();
        Refresh();
    }

    private void Refresh()
    {
        var exe = Qwen3TtsCrispAsr.GetCrispAsrExecutable();
        IsEngineInstalled = File.Exists(exe);

        if (!IsEngineInstalled)
        {
            EngineLabel = "CrispASR not installed";
            EngineBrush = Red;
            EngineDownloadButtonText = "Download CrispASR";
        }
        else if (Qwen3TtsCrispAsr.GetEngineUpdateStatus() == DownloadHashManager.UpdateStatus.UpdateAvailable)
        {
            EngineLabel = "CrispASR - update available";
            EngineBrush = Amber;
            EngineDownloadButtonText = "Update CrispASR";
        }
        else
        {
            EngineLabel = "CrispASR";
            EngineBrush = Green;
            EngineDownloadButtonText = "Re-download CrispASR";
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
                    Se.LogError(ex, "Qwen3TtsCrispAsrSettings: CrispASR version probe failed");
                }
            });
        }

        // Talker GGUFs live in the engine's own models folder OR are auto-downloaded by
        // crispasr into ~/.cache/crispasr. We can only verify the engine-folder copy here;
        // when CrispASR is installed, a missing local file is fine ("Auto-download on first
        // use"). When CrispASR is NOT installed, that message is misleading — there's no
        // path to auto-download anything until the user installs the runtime first.
        var voiceDesignPath = Qwen3TtsCrispAsr.GetTalkerPath(Qwen3TtsCrispAsr.ModelKeyVoiceDesign);
        ApplyModelStatus(File.Exists(voiceDesignPath), IsEngineInstalled,
            label => VoiceDesignTalkerLabel = label,
            brush => VoiceDesignTalkerBrush = brush);

        var customVoicePath = Qwen3TtsCrispAsr.GetTalkerPath(Qwen3TtsCrispAsr.ModelKeyCustomVoice);
        ApplyModelStatus(File.Exists(customVoicePath), IsEngineInstalled,
            label => CustomVoiceTalkerLabel = label,
            brush => CustomVoiceTalkerBrush = brush);

        var codecPath = Qwen3TtsCrispAsr.GetCodecPath();
        ApplyModelStatus(File.Exists(codecPath), IsEngineInstalled,
            label => CodecLabel = label,
            brush => CodecBrush = brush);

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
            setBrush(Green);
            return;
        }

        if (!engineInstalled)
        {
            // No CrispASR runtime means there's nothing to auto-download into, so don't
            // promise a download that can't happen until the user installs CrispASR.
            setLabel("CrispASR required");
            setBrush(Grey);
            return;
        }

        setLabel("Auto-download on first use");
        setBrush(Grey);
    }

    [RelayCommand]
    private async Task RedownloadEngine()
    {
        if (Window == null)
        {
            return;
        }

        // CrispASR is shared with Speech-to-text and Chatterbox; we use the Qwen3-specific
        // entry point so the prompts read "Qwen3 TTS (CrispASR)" rather than "Chatterbox TTS"
        // (the crispasr binary itself doesn't care which engine triggered the download).
        await TtsVoiceInstaller.EnsureCrispAsrForQwen3(Window, _windowService, forceRedownload: true);
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
