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

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.IndexTtsCrispAsrSettings;

public partial class IndexTtsCrispAsrSettingsViewModel : ObservableObject
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

    [ObservableProperty] private string _talkerQ4KLabel = string.Empty;
    [ObservableProperty] private IBrush _talkerQ4KBrush = Grey();

    [ObservableProperty] private string _talkerQ8_0Label = string.Empty;
    [ObservableProperty] private IBrush _talkerQ8_0Brush = Grey();

    [ObservableProperty] private string _talkerF16Label = string.Empty;
    [ObservableProperty] private IBrush _talkerF16Brush = Grey();

    [ObservableProperty] private string _codecLabel = string.Empty;
    [ObservableProperty] private IBrush _codecBrush = Grey();

    [ObservableProperty] private string _voicesLabel = string.Empty;

    [ObservableProperty] private double _speed = 1.0;

    public string SpeedLabel => $"Speed {Speed:0.00}x";

    partial void OnSpeedChanged(double value)
    {
        OnPropertyChanged(nameof(SpeedLabel));
        Se.Settings.Video.TextToSpeech.IndexTtsCrispAsrSpeed = Math.Clamp(value, 0.25, 4.0);
    }

    [ObservableProperty] private string _modelsFolder = string.Empty;
    [ObservableProperty] private string _voicesFolder = string.Empty;
    [ObservableProperty] private bool _isEngineInstalled;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    public IndexTtsCrispAsrSettingsViewModel(IWindowService windowService, IFolderHelper folderHelper)
    {
        _windowService = windowService;
        _folderHelper = folderHelper;
    }

    public void Initialize()
    {
        ModelsFolder = IndexTtsCrispAsr.GetSetModelsFolder();
        VoicesFolder = IndexTtsCrispAsr.GetSetVoicesFolder();
        Speed = Math.Clamp(Se.Settings.Video.TextToSpeech.IndexTtsCrispAsrSpeed, 0.25, 4.0);
        Refresh();
    }

    private void Refresh()
    {
        var exe = IndexTtsCrispAsr.GetCrispAsrExecutable();
        IsEngineInstalled = File.Exists(exe);

        if (!IsEngineInstalled)
        {
            EngineLabel = "CrispASR not installed";
            EngineBrush = Red();
            EngineDownloadButtonText = "Download CrispASR";
        }
        else if (IndexTtsCrispAsr.GetEngineUpdateStatus() == DownloadHashManager.UpdateStatus.UpdateAvailable)
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
                    Se.LogError(ex, "IndexTtsCrispAsrSettings: CrispASR version probe failed");
                }
            });
        }

        // GPT talker (one of three quants) + BigVGAN codec live in the engine's models folder
        // OR are auto-downloaded by crispasr into ~/.cache/crispasr. We can only verify the
        // engine-folder copy here; when CrispASR is installed, a missing local file is fine
        // ("Auto-download on first use"). When CrispASR is NOT installed, that message is
        // misleading — there's no path to auto-download anything until the user installs the
        // runtime first.
        var talkerQ4K = IndexTtsCrispAsr.GetTalkerPath(IndexTtsCrispAsr.ModelKeyQ4K);
        ApplyModelStatus(IndexTtsCrispAsr.IsValidLocalModelFile(talkerQ4K, IndexTtsCrispAsr.TalkerQ4KFileName),
            IsEngineInstalled,
            label => TalkerQ4KLabel = label,
            brush => TalkerQ4KBrush = brush);

        var talkerQ8_0 = IndexTtsCrispAsr.GetTalkerPath(IndexTtsCrispAsr.ModelKeyQ8_0);
        ApplyModelStatus(IndexTtsCrispAsr.IsValidLocalModelFile(talkerQ8_0, IndexTtsCrispAsr.TalkerQ8_0FileName),
            IsEngineInstalled,
            label => TalkerQ8_0Label = label,
            brush => TalkerQ8_0Brush = brush);

        var talkerF16 = IndexTtsCrispAsr.GetTalkerPath(IndexTtsCrispAsr.ModelKeyF16);
        ApplyModelStatus(IndexTtsCrispAsr.IsValidLocalModelFile(talkerF16, IndexTtsCrispAsr.TalkerF16FileName),
            IsEngineInstalled,
            label => TalkerF16Label = label,
            brush => TalkerF16Brush = brush);

        var codecPath = IndexTtsCrispAsr.GetCodecPath();
        ApplyModelStatus(IndexTtsCrispAsr.IsValidLocalModelFile(codecPath, IndexTtsCrispAsr.CodecFileName),
            IsEngineInstalled,
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
            setBrush(Green());
            return;
        }

        if (!engineInstalled)
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

        // CrispASR is shared across all CrispASR-driven TTS engines; we use the IndexTTS
        // entry point so prompts read "IndexTTS (CrispASR)" rather than another engine's name
        // (the crispasr binary itself doesn't care which engine triggered the download).
        await TtsVoiceInstaller.EnsureCrispAsrForIndexTts(Window, _windowService, forceRedownload: true);
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
