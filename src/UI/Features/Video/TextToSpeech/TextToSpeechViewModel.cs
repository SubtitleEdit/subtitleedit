using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.DownloadTts;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.ElevenLabsSettings;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.EncodingSettings;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.ReviewSpeech;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.VoiceSettings;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using ElevenLabsSettingsViewModel = Nikse.SubtitleEdit.Features.Video.TextToSpeech.ElevenLabsSettings.ElevenLabsSettingsViewModel;
using ReviewSpeechViewModel = Nikse.SubtitleEdit.Features.Video.TextToSpeech.ReviewSpeech.ReviewSpeechViewModel;
using Timer = System.Timers.Timer;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech;

public partial class TextToSpeechViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<ITtsEngine> _engines;
    [ObservableProperty] private ITtsEngine? _selectedEngine;
    [ObservableProperty] private ObservableCollection<Voice> _voices;
    [ObservableProperty] private Voice? _selectedVoice;
    [ObservableProperty] private ObservableCollection<TtsLanguage> _languages;
    [ObservableProperty] private TtsLanguage? _selectedLanguage;
    [ObservableProperty] private ObservableCollection<string> _regions;
    [ObservableProperty] private string? _selectedRegion;
    [ObservableProperty] private ObservableCollection<string> _models;
    [ObservableProperty] private string? _selectedModel;
    [ObservableProperty] private bool _hasLanguageParameter;
    [ObservableProperty] private bool _hasApiKey;
    [ObservableProperty] private string _apiKey;
    [ObservableProperty] private bool _hasRegion;
    [ObservableProperty] private string _region;
    [ObservableProperty] private bool _hasModel;
    [ObservableProperty] private int _voiceCount;
    [ObservableProperty] private string _voiceCountInfo;
    [ObservableProperty] private bool _isVoiceTestEnabled;
    [ObservableProperty] private bool _doReviewAudioClips;
    [ObservableProperty] private bool _doGenerateVideoFile;
    [ObservableProperty] private bool _isGenerating;
    [ObservableProperty] private bool _isNotGenerating;
    [ObservableProperty] private bool _isEngineSettingsVisible;
    [ObservableProperty] private string _progressText;
    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private double _progressOpacity;
    [ObservableProperty] private string _doneOrCancelText;
    [ObservableProperty] private bool _hasKeyFile;
    [ObservableProperty] private string _keyFile;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    private Subtitle _subtitle = new();
    private readonly IFileHelper _fileHelper;
    private readonly IFolderHelper _folderHelper;
    private string _waveFolder;
    private CancellationTokenSource _cancellationTokenSource;
    private CancellationToken _cancellationToken;
    private WavePeakData2? _wavePeakData;
    private FfmpegMediaInfo? _mediaInfo;
    private string _videoFileName = string.Empty;
    private LibMpvDynamicPlayer? _mpvContext;
    private Lock _playLock;
    private readonly Timer _timer;
    private readonly IWindowService _windowService;

    public TextToSpeechViewModel(ITtsDownloadService ttsDownloadService, IWindowService windowService, IFileHelper fileHelper, IFolderHelper folderHelper)
    {
        _windowService = windowService;
        _fileHelper = fileHelper;
        _folderHelper = folderHelper;

        Engines = new ObservableCollection<ITtsEngine>();
        Voices = new ObservableCollection<Voice>();
        Regions = new ObservableCollection<string>();
        Models = new ObservableCollection<string>();
        Languages = new ObservableCollection<TtsLanguage>();
        ApiKey = string.Empty;
        Region = string.Empty;
        VoiceCountInfo = string.Empty;
        ProgressText = string.Empty;
        ProgressText = string.Empty;
        DoneOrCancelText = string.Empty;
        IsVoiceTestEnabled = true;
        IsGenerating = false;
        IsNotGenerating = true;
        KeyFile = string.Empty;

        _cancellationTokenSource = new CancellationTokenSource();
        _playLock = new Lock();
        _timer = new Timer(100);
        _timer.Elapsed += OnTimerOnElapsed;
        _waveFolder = string.Empty;
        _wavePeakData = new WavePeakData2(1, new List<WavePeak2>());
        _mediaInfo = null;

        Engines =
        [
            new Piper(ttsDownloadService),
            new AllTalk(ttsDownloadService),
            new ElevenLabs(ttsDownloadService),
            new AzureSpeech(ttsDownloadService),
            new Murf(ttsDownloadService),
            new GoogleSpeech(ttsDownloadService)
        ];
    }

    private void OnTimerOnElapsed(object? sender, ElapsedEventArgs args)
    {
        lock (_playLock)
        {
            if (_mpvContext == null)
            {
                IsVoiceTestEnabled = true;
            }
            else
            {
                IsVoiceTestEnabled = _mpvContext.IsPaused;
            }
        }
    }

    private void LoadSettings()
    {
        var lastEngine = Engines.FirstOrDefault(e => e.Name == Se.Settings.Video.TextToSpeech.Engine);
        if (lastEngine != null)
        {
            SelectedEngine = lastEngine;
        }

        var lastVoice = Voices.FirstOrDefault(v => v.Name == Se.Settings.Video.TextToSpeech.Voice);
        if (lastVoice == null)
        {
            lastVoice = Voices.FirstOrDefault(p => p.Name.StartsWith("en", StringComparison.OrdinalIgnoreCase) ||
                                                        p.Name.Contains("English", StringComparison.OrdinalIgnoreCase));
            SelectedVoice = lastVoice ?? Voices.FirstOrDefault();
        }
        else
        {
            SelectedVoice = lastVoice;
        }

        DoReviewAudioClips = Se.Settings.Video.TextToSpeech.ReviewAudioClips;
        DoGenerateVideoFile = Se.Settings.Video.TextToSpeech.GenerateVideoFile;

        if (SelectedEngine is AzureSpeech)
        {
            ApiKey = Se.Settings.Video.TextToSpeech.AzureApiKey;
            SelectedRegion = Se.Settings.Video.TextToSpeech.AzureRegion;
        }
        else if (SelectedEngine is ElevenLabs)
        {
            ApiKey = Se.Settings.Video.TextToSpeech.ElevenLabsApiKey;
        }
        else if (SelectedEngine is Murf)
        {
            ApiKey = Se.Settings.Video.TextToSpeech.MurfApiKey;
        }
        else if (SelectedEngine is GoogleSpeech)
        {
            ApiKey = Se.Settings.Video.TextToSpeech.GoogleApiKey;
            KeyFile = Se.Settings.Video.TextToSpeech.GoogleKeyFile;
        }
    }

    private void SaveSettings()
    {
        Se.Settings.Video.TextToSpeech.Engine = SelectedEngine?.Name ?? string.Empty;
        Se.Settings.Video.TextToSpeech.Voice = SelectedVoice?.Name ?? string.Empty;
        Se.Settings.Video.TextToSpeech.ReviewAudioClips = DoReviewAudioClips;
        Se.Settings.Video.TextToSpeech.GenerateVideoFile = DoGenerateVideoFile;

        if (SelectedEngine is AzureSpeech)
        {
            Se.Settings.Video.TextToSpeech.AzureApiKey = ApiKey;
            Se.Settings.Video.TextToSpeech.AzureRegion = SelectedRegion ?? string.Empty;
        }
        else if (SelectedEngine is ElevenLabs)
        {
            Se.Settings.Video.TextToSpeech.ElevenLabsApiKey = ApiKey;
            Se.Settings.Video.TextToSpeech.ElevenLabsModel = SelectedModel ?? string.Empty;
        }
        else if (SelectedEngine is Murf)
        {
            Se.Settings.Video.TextToSpeech.MurfApiKey = ApiKey;
        }
        else if (SelectedEngine is GoogleSpeech)
        {
            Se.Settings.Video.TextToSpeech.GoogleApiKey = ApiKey;
            Se.Settings.Video.TextToSpeech.GoogleKeyFile = KeyFile;
        }

        Se.SaveSettings();
    }

    public void Initialize(Subtitle subtitle, string videoFileName, WavePeakData2? wavePeakData, string waveFolder)
    {
        _subtitle = subtitle;
        _subtitle.RemoveEmptyLines();

        _videoFileName = videoFileName;
        _wavePeakData = wavePeakData;
        _cancellationTokenSource = new CancellationTokenSource();
        _cancellationToken = _cancellationTokenSource.Token;
        IsGenerating = false;
        IsNotGenerating = true;
        IsEngineSettingsVisible = false;
        ProgressText = string.Empty;
        ProgressValue = 0.0;
        _waveFolder = waveFolder;
        //_mediaInfo = mediaInfo;
    }

    [RelayCommand]
    public async Task BrowseKeyFile()
    {
        var fileName = await _fileHelper.PickOpenFile(Window!, "Open key file", "json files", "*.json");
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        KeyFile = fileName;
    }

    [RelayCommand]
    public async Task GenerateTts()
    {
        var engine = SelectedEngine;
        if (engine == null)
        {
            return;
        }

        var isInstalled = await IsEngineInstalled(engine);
        if (!isInstalled)
        {
            return;
        }

        _cancellationTokenSource = new();
        _cancellationToken = _cancellationTokenSource.Token;
        ProgressValue = 0;
        ProgressText = string.Empty;
        IsGenerating = true;
        IsNotGenerating = false;
        ProgressOpacity = 1.0;
        DoneOrCancelText = Se.Language.General.Cancel;
        SaveSettings();

        // Generate
        var generateSpeechResult = await GenerateSpeech(_cancellationToken);
        if (generateSpeechResult == null)
        {
            DoneOrCancelText = Se.Language.General.Done;
            IsGenerating = false;
            IsNotGenerating = true;
            ProgressOpacity = 0;
            return;
        }

        // Fix speed
        var fixSpeedResult = await FixSpeed(generateSpeechResult, _cancellationToken);
        if (fixSpeedResult == null)
        {
            DoneOrCancelText = Se.Language.General.Done;
            IsGenerating = false;
            IsNotGenerating = true;
            ProgressOpacity = 0;
            return;
        }

        // Review audio clips
        if (DoReviewAudioClips)
        {
            var reviewAudioClipsResult = await ReviewAudioClips(fixSpeedResult);
            if (reviewAudioClipsResult == null)
            {
                DoneOrCancelText = Se.Language.General.Done;
                IsGenerating = false;
                IsNotGenerating = true;
                ProgressOpacity = 0;
                return;
            }

            fixSpeedResult = reviewAudioClipsResult;
        }

        await MergeAndAddToVideo(fixSpeedResult);
    }

    [RelayCommand]
    private async Task ShowEngineSettings()
    {
        await _windowService.ShowDialogAsync<ElevenLabsSettingsWindow, ElevenLabsSettingsViewModel>(Window!, vm => { });
    }

    [RelayCommand]
    private async Task TestVoice()
    {
        var engine = SelectedEngine;
        var voice = SelectedVoice;
        if (engine == null || voice == null || Window == null)
        {
            return;
        }

        var isInstalled = await IsEngineInstalled(engine);
        if (!isInstalled)
        {
            return;
        }

        if (!engine.IsVoiceInstalled(voice) && voice.EngineVoice is PiperVoice piperVoice)
        {
            var modelFileName = Path.Combine(Piper.GetSetPiperFolder(), piperVoice.ModelShort);
            var configFileName = Path.Combine(Piper.GetSetPiperFolder(), piperVoice.ConfigShort);
            if (!File.Exists(modelFileName) || !File.Exists(configFileName))
            {
                var dlResult = await _windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(Window, vm => vm.StartDownloadPiperVoice(piperVoice));
                if (!dlResult.OkPressed)
                {
                    SafeDelete(modelFileName);
                    SafeDelete(configFileName);
                    return;
                }
            }
        }

        SaveSettings();

        var text = Se.Settings.Video.TextToSpeech.VoiceTestText;
        if (string.IsNullOrEmpty(text))
        {
            text = "This is a test";
        }

        _cancellationTokenSource = new CancellationTokenSource();
        _cancellationToken = _cancellationTokenSource.Token;
        var result = await engine.Speak(text, _waveFolder, voice, SelectedLanguage, SelectedRegion, SelectedModel, _cancellationToken);
        if (!File.Exists(result.FileName))
        {
            await MessageBox.Show(
                Window,
                "Test voice error",
                $"Output audio file was not generated: {result.FileName}",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            return;
        }

        IsVoiceTestEnabled = false;
        await PlayAudio(result.FileName);
    }


    [RelayCommand]
    private async Task ShowTestVoiceSettings()
    {
        var result = await _windowService.ShowDialogAsync<VoiceSettingsWindow, VoiceSettingsViewModel>(Window!, vm => { });

        var engine = SelectedEngine;
        if (result.RefreshVoices && engine != null)
        {
            var voices = await engine.RefreshVoices(string.Empty, CancellationToken.None);
            Voices.Clear();
            foreach (var voice in voices)
            {
                Voices.Add(voice);
            }
            SelectedVoice = Voices.FirstOrDefault(v => v.Name == Se.Settings.Video.TextToSpeech.Voice) ?? Voices.FirstOrDefault();
        }
    }

    [RelayCommand]
    private async Task ShowEncodingSettings()
    {
        await _windowService.ShowDialogAsync<EncodingSettingsWindow, EncodingSettingsViewModel>(Window!, vm => { });
    }

    [RelayCommand]
    private async Task Import()
    {
        if (Window == null)
        {
            return;
        }

        _cancellationTokenSource = new CancellationTokenSource();
        _cancellationToken = _cancellationTokenSource.Token;

        var fileName = await _fileHelper.PickOpenFile(Window, "Open SubtitleEditTts.json file", "TTS json files", "SubtitleEditTts.json");
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var json = await File.ReadAllTextAsync(fileName, _cancellationToken);
        var importExport = JsonSerializer.Deserialize<TtsImportExport>(json);
        if (importExport == null)
        {
            var answer = await MessageBox.Show(
                Window,
                "Text to speech",
                "Nothing to import",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            return;
        }

        var stepResults = new List<TtsStepResult>();
        for (var index = 0; index < importExport.Items.Count; index++)
        {
            var item = importExport.Items[index];
            var paragraph = new Paragraph(item.Text, item.StartMs, item.EndMs) { Number = index + 1 };
            stepResults.Add(new TtsStepResult
            {
                Text = item.Text,
                CurrentFileName = item.AudioFileName,
                Paragraph = paragraph,
                SpeedFactor = 1.0f,
                Voice = Voices.FirstOrDefault(v => v.Name == item.VoiceName),
            });
        }

        var result = await _windowService.ShowDialogAsync<ReviewSpeechWindow, ReviewSpeechViewModel>(Window, vm =>
        {
            vm.Initialize(
                stepResults.ToArray(),
                Engines.ToArray(),
                SelectedEngine ?? Engines.First(),
                Voices.ToArray(),
                SelectedVoice ?? Voices.First(),
                Languages.ToArray(),
                SelectedLanguage,
                _videoFileName,
                Path.GetDirectoryName(fileName)!,
                _wavePeakData);
        });
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        _cancellationTokenSource.Cancel();
        IsGenerating = false;
        IsNotGenerating = true;
        ProgressOpacity = 0;
    }

    [RelayCommand]
    private void Done()
    {
        SaveSettings();
        _cancellationTokenSource.Cancel();
        IsGenerating = false;
        IsNotGenerating = true;
        ProgressOpacity = 0;
        Close();
    }

    private static void SafeDelete(string fileName)
    {
        try
        {
            File.Delete(fileName);
        }
        catch
        {
            // ignore
        }
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Window?.Close();
        });
    }

    private async Task PlayAudio(string fileName)
    {
        lock (_playLock)
        {
            _mpvContext?.Stop();
            _mpvContext?.Dispose();
            _mpvContext = new LibMpvDynamicPlayer();
        }
        await _mpvContext.LoadFile(fileName);
    }

    private async Task<bool> IsEngineInstalled(ITtsEngine engine)
    {
        if (await engine.IsInstalled(SelectedRegion) || Window == null)
        {
            return true;
        }

        if (engine is Piper)
        {
            var answer = await MessageBox.Show(
                Window,
                "Download Piper?",
                $"{Environment.NewLine}\"Text to speech\" requires Piper.{Environment.NewLine}{Environment.NewLine}Download and use Piper?",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (answer != MessageBoxResult.Yes)
            {
                return false;
            }

            var result = await _windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(Window, vm => vm.StartDownloadPiper());
            return await engine.IsInstalled(SelectedRegion);
        }

        if (engine is AllTalk)
        {
            var answer = await MessageBox.Show(
                Window,
                Se.Language.General.Error,
                $"\"AllTalk\" text to speech requires a running local AllTalk web server.{Environment.NewLine}{Environment.NewLine}Read more?",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (answer != MessageBoxResult.Yes)
            {
                return false;
            }

            await Window.Launcher.LaunchUriAsync(new Uri("https://github.com/erew123/alltalk_tts"));

            return await engine.IsInstalled(SelectedRegion);
        }

        if (engine.HasKeyFile)
        {
            if (string.IsNullOrEmpty(KeyFile) || !File.Exists(KeyFile))
            {
                await MessageBox.Show(
                Window,
                Se.Language.General.Error,
                $"\"{engine.Name}\" requires a key file",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

                return false;
            }

            return true;
        }

        if (engine.HasApiKey)
        {
            if (string.IsNullOrEmpty(ApiKey))
            {
                await MessageBox.Show(
                Window,
                Se.Language.General.Error,
                $"\"{engine.Name}\" requires an API key",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

                return false;
            }

            return true;
        }

        return false;
    }

    private async Task MergeAndAddToVideo(TtsStepResult[] fixSpeedResult)
    {
        // Merge audio paragraphs
        var mergedAudioFileName = await MergeAudioParagraphs(fixSpeedResult, _cancellationToken);
        if (string.IsNullOrEmpty(mergedAudioFileName))
        {
            DoneOrCancelText = Se.Language.General.Done;
            IsGenerating = false;
            IsNotGenerating = true;
            ProgressOpacity = 0;
            return;
        }

        var result = await _folderHelper.PickFolderAsync(Window!, Se.Language.General.SelectedAFolderToSaveTo);
        if (string.IsNullOrEmpty(result))
        {
            DoneOrCancelText = Se.Language.General.Done;
            IsGenerating = false;
            IsNotGenerating = true;
            ProgressOpacity = 0;
            return;
        }
        var outputFolder = result;
        var audioFileName = Path.Combine(outputFolder, GetBestFileName(outputFolder, ".wav"));

        File.Move(mergedAudioFileName, audioFileName);

        await HandleAddToVideo(audioFileName, outputFolder, _cancellationToken);

        DoneOrCancelText = Se.Language.General.Done;
        IsGenerating = false;
        IsNotGenerating = true;
        ProgressOpacity = 0;
    }

    private async Task HandleAddToVideo(string mergedAudioFileName, string outputFolder, CancellationToken cancellationToken)
    {
        if (DoGenerateVideoFile && !string.IsNullOrEmpty(_videoFileName))
        {
            var outputFileName = await AddAudioToVideoFile(mergedAudioFileName, outputFolder, cancellationToken);
            if (!string.IsNullOrEmpty(outputFileName) && Window != null)
            {
                await MessageBox.Show(
                    Window,
                    Se.Language.Video.TextToSpeech.Title,
                    string.Format(Se.Language.General.VideoFileGeneratedX, outputFileName),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                await _folderHelper.OpenFolder(Window!, outputFolder);
            }
        }
        else
        {
            var path = Path.GetDirectoryName(mergedAudioFileName)!;
            await _folderHelper.OpenFolder(Window!, outputFolder);
        }
    }

    private async Task<string?> AddAudioToVideoFile(string audioFileName, string outputFolder, CancellationToken cancellationToken)
    {
        var videoExt = ".mkv";
        if (_videoFileName.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase))
        {
            videoExt = ".mp4";
        }

        ProgressText = Se.Language.Video.TextToSpeech.AddingAudioToVideoFileDotDotDot;
        var outputFileName = Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(audioFileName) + videoExt);

        var useCustomAudioEncoding = !string.IsNullOrEmpty(Se.Settings.Video.TextToSpeech.CustomAudioEncoding);
        var audioEncoding = Se.Settings.Video.TextToSpeech.CustomAudioEncoding;
        if (string.IsNullOrWhiteSpace(audioEncoding) || !useCustomAudioEncoding)
        {
            audioEncoding = string.Empty;
        }

        bool? stereo = null;
        if (Se.Settings.Video.TextToSpeech.CustomAudioStereo && useCustomAudioEncoding)
        {
            stereo = true;
        }

        var addAudioProcess = FfmpegGenerator.AddAudioTrack(_videoFileName, audioFileName, outputFileName, audioEncoding, stereo);
#pragma warning disable CA1416 // Validate platform compatibility
        var _ = addAudioProcess.Start();
#pragma warning restore CA1416 // Validate platform compatibility
        await addAudioProcess.WaitForExitAsync(cancellationToken);

        ProgressText = string.Empty;
        return outputFileName;
    }

    private async Task<string?> MergeAudioParagraphs(TtsStepResult[] previousStepResult, CancellationToken cancellationToken)
    {
        try
        {
            var engine = SelectedEngine;
            var voice = SelectedVoice;
            if (engine == null || voice == null || cancellationToken.IsCancellationRequested)
            {
                return null;
            }

            bool forceStereo = false;
            var useCustomAudioEncoding = !string.IsNullOrEmpty(Se.Settings.Video.TextToSpeech.CustomAudioEncoding);
            if (Se.Settings.Video.TextToSpeech.CustomAudioStereo && useCustomAudioEncoding)
            {
                forceStereo = true;
            }

            var silenceFileName = await GenerateSilenceWaveFile(cancellationToken);

            var outputFileName = string.Empty;
            var inputFileName = silenceFileName;
            ProgressValue = 0;
            for (var index = 0; index < previousStepResult.Length; index++)
            {
                ProgressText = $"Merging audio: segment {index + 1} of {_subtitle.Paragraphs.Count}";

                var item = previousStepResult[index];
                outputFileName = Path.Combine(_waveFolder, $"silence{index}.wav");
                if (File.Exists(outputFileName))
                {
                    outputFileName = Path.Combine(_waveFolder, $"silence_{Guid.NewGuid()}.wav");
                }

                var mergeProcess = FfmpegGenerator.MergeAudioTracks(inputFileName, item.CurrentFileName, outputFileName, (float)item.Paragraph.StartTime.TotalSeconds, forceStereo);
                var fileNameToDelete = inputFileName;
                inputFileName = outputFileName;
#pragma warning disable CA1416 // Validate platform compatibility
                _ = mergeProcess.Start();
#pragma warning restore CA1416 // Validate platform compatibility
                await mergeProcess.WaitForExitAsync(cancellationToken);

                ProgressValue = (double)(index + 1) / previousStepResult.Length * 100.0;

                DeleteFileNoError(fileNameToDelete);
            }
            ProgressValue = 100;

            return outputFileName;
        }
        catch (OperationCanceledException)
        {
            ProgressText = Se.Language.General.Cancelled; ;
            return null;
        }
    }

    private static void DeleteFileNoError(string file)
    {
        try
        {
            File.Delete(file);
        }
        catch
        {
            // ignored
        }
    }

    private async Task<string> GenerateSilenceWaveFile(CancellationToken cancellationToken)
    {
        ProgressText = Se.Language.Video.TextToSpeech.PreparingMergeDotDotDot;
        ProgressValue = 0;
        var silenceFileName = Path.Combine(_waveFolder, "silence.wav");
        var silenceIdx = 0;
        while (File.Exists(silenceFileName))
        {
            silenceIdx++;
            silenceFileName = Path.Combine(_waveFolder, $"silence_{silenceIdx}.wav");
        }

        var durationInSeconds = 10f;
        if (_mediaInfo != null)
        {
            durationInSeconds = (float)_mediaInfo.Duration.TotalSeconds;
        }
        else if (_subtitle.Paragraphs.Count > 0)
        {
            durationInSeconds = (float)_subtitle.Paragraphs.Max(p => p.EndTime.TotalSeconds);
        }

        var silenceProcess = FfmpegGenerator.GenerateEmptyAudio(silenceFileName, durationInSeconds);
#pragma warning disable CA1416 // Validate platform compatibility
        _ = silenceProcess.Start();
#pragma warning restore CA1416 // Validate platform compatibility
        await silenceProcess.WaitForExitAsync(cancellationToken);
        return silenceFileName;
    }

    private string GetBestFileName(string folder, string extension)
    {
        var returnFileName = string.Empty;
        if (!string.IsNullOrEmpty(_videoFileName))
        {
            returnFileName = Path.GetFileNameWithoutExtension(_videoFileName) + extension;
        }
        if (!string.IsNullOrEmpty(returnFileName) && !File.Exists(Path.Combine(folder, returnFileName)))
        {
            return returnFileName;
        }

        if (!string.IsNullOrEmpty(_subtitle.FileName))
        {
            returnFileName = Path.GetFileNameWithoutExtension(_subtitle.FileName) + extension;
        }
        if (!string.IsNullOrEmpty(returnFileName) && !File.Exists(Path.Combine(folder, returnFileName)))
        {
            return returnFileName;
        }

        return Guid.NewGuid() + extension;
    }

    private async Task<TtsStepResult[]?> GenerateSpeech(CancellationToken cancellationToken)
    {
        var engine = SelectedEngine;
        var voice = SelectedVoice;
        if (engine == null || voice == null || cancellationToken.IsCancellationRequested)
        {
            return null;
        }

        try
        {
            var resultList = new List<TtsStepResult>();
            ProgressValue = 0;
            for (var index = 0; index < _subtitle.Paragraphs.Count; index++)
            {
                ProgressText = $"Generating speech: segment {index + 1} of {_subtitle.Paragraphs.Count}";
                var paragraph = _subtitle.Paragraphs[index];
                var speakResult = await engine.Speak(paragraph.Text, _waveFolder, voice, SelectedLanguage, SelectedRegion, SelectedModel, cancellationToken);
                resultList.Add(new TtsStepResult
                {
                    Text = paragraph.Text,
                    CurrentFileName = speakResult.FileName,
                    Paragraph = paragraph,
                    SpeedFactor = 1.0f,
                    Voice = voice,
                });
                ProgressValue = (double)(index + 1) / _subtitle.Paragraphs.Count * 100.0;

                if (cancellationToken.IsCancellationRequested)
                {
                    return null;
                }
            }
            ProgressValue = 100;

            return resultList.ToArray();
        }
        catch (OperationCanceledException)
        {
            return null;
        }
    }

    private async Task<TtsStepResult[]?> FixSpeed(TtsStepResult[] previousStepResult, CancellationToken cancellationToken)
    {
        var engine = SelectedEngine;
        var voice = SelectedVoice;
        if (engine == null || voice == null || cancellationToken.IsCancellationRequested)
        {
            return null;
        }

        try
        {
            var resultList = new List<TtsStepResult>();
            ProgressValue = 0;
            for (var index = 0; index < previousStepResult.Length; index++)
            {
                ProgressText = $"Adjusting speed: segment {index + 1} of {_subtitle.Paragraphs.Count}";
                ProgressValue = (double)index / _subtitle.Paragraphs.Count * 100;

                var item = previousStepResult[index];
                var p = item.Paragraph;
                var next = index + 1 < previousStepResult.Length ? previousStepResult[index + 1] : null;
                var outputFileName1 = Path.Combine(Path.GetDirectoryName(item.CurrentFileName)!, Guid.NewGuid() + ".wav");
                var trimProcess = FfmpegGenerator.TrimSilenceStartAndEnd(item.CurrentFileName, outputFileName1);
#pragma warning disable CA1416 // Validate platform compatibility
                _ = trimProcess.Start();
#pragma warning restore CA1416 // Validate platform compatibility
                await trimProcess.WaitForExitAsync(cancellationToken);

                var addDuration = 0d;
                if (next != null && p.EndTime.TotalMilliseconds < next.Paragraph.StartTime.TotalMilliseconds)
                {
                    var diff = next.Paragraph.StartTime.TotalMilliseconds - p.EndTime.TotalMilliseconds;
                    addDuration = Math.Min(1000, diff);
                    if (addDuration < 0)
                    {
                        addDuration = 0;
                    }
                }

                var mediaInfo = FfmpegMediaInfo.Parse(outputFileName1);
                if (mediaInfo.Duration == null)
                {
                    continue;
                }

                if (mediaInfo.Duration.TotalMilliseconds <= p.DurationTotalMilliseconds + addDuration)
                {
                    resultList.Add(new TtsStepResult
                    {
                        Paragraph = p,
                        Text = item.Text,
                        CurrentFileName = outputFileName1,
                        SpeedFactor = 1.0f,
                        Voice = item.Voice,
                    });
                    continue;
                }

                var divisor = (decimal)(p.DurationTotalMilliseconds + addDuration);
                if (divisor <= 0)
                {
                    resultList.Add(new TtsStepResult
                    {
                        Paragraph = p,
                        Text = item.Text,
                        CurrentFileName = item.CurrentFileName,
                        SpeedFactor = 1.0f,
                        Voice = item.Voice,
                    });

                    SeLogger.Error($"TextToSpeech: Duration is zero (skipping): {item.CurrentFileName}, {p}");
                    continue;
                }

                var ext = ".wav";
                var factor = (decimal)mediaInfo.Duration.TotalMilliseconds / divisor;
                var outputFileName2 = Path.Combine(_waveFolder, $"{index}_{Guid.NewGuid()}{ext}");
                var overrideFileName = string.Empty;
                if (!string.IsNullOrEmpty(overrideFileName) && File.Exists(Path.Combine(_waveFolder, overrideFileName)))
                {
                    outputFileName2 = Path.Combine(_waveFolder, $"{Path.GetFileNameWithoutExtension(overrideFileName)}_{Guid.NewGuid()}{ext}");
                }

                resultList.Add(new TtsStepResult
                {
                    Paragraph = p,
                    Text = item.Text,
                    CurrentFileName = outputFileName2,
                    SpeedFactor = (float)factor,
                    Voice = item.Voice,
                });

                var mergeProcess = FfmpegGenerator.ChangeSpeed(outputFileName1, outputFileName2, (float)factor);
#pragma warning disable CA1416 // Validate platform compatibility
                _ = mergeProcess.Start();
#pragma warning restore CA1416 // Validate platform compatibility
                await mergeProcess.WaitForExitAsync(cancellationToken);
            }
            ProgressValue = 100;

            return resultList.ToArray();
        }
        catch (OperationCanceledException)
        {
            return null;
        }
    }

    private async Task<TtsStepResult[]?> ReviewAudioClips(TtsStepResult[] previousStepResult)
    {
        if (!DoReviewAudioClips)
        {
            return previousStepResult;
        }

        var engine = SelectedEngine;
        var voice = SelectedVoice;
        if (engine == null || voice == null)
        {
            return previousStepResult;
        }

        var result = await _windowService.ShowDialogAsync<ReviewSpeechWindow, ReviewSpeechViewModel>(Window!, vm =>
        {
            vm.Initialize(
                previousStepResult,
                Engines.ToArray(),
                engine,
                Voices.ToArray(),
                voice,
                Languages.ToArray(),
                SelectedLanguage,
                _videoFileName,
                _waveFolder,
                _wavePeakData);
        });

        if (result.OkPressed)
        {
            return result.StepResults;
        }

        return null;
    }

    internal void SelectedEngineChanged(object? sender, SelectionChangedEventArgs e)
    {
        var engine = SelectedEngine;
        if (engine == null)
        {
            return;
        }

        Dispatcher.UIThread.Post(async () =>
        {
            IsEngineSettingsVisible = false;
            var voices = await engine.GetVoices(SelectedLanguage?.Code ?? string.Empty);
            Voices.Clear();
            foreach (var vo in voices)
            {
                Voices.Add(vo);
            }
            VoiceCount = Voices.Count;

            var lastVoice = Voices.FirstOrDefault(v => v.Name == Se.Settings.Video.TextToSpeech.Voice);
            if (lastVoice == null)
            {
                lastVoice = Voices.FirstOrDefault(p => p.Name.StartsWith("en", StringComparison.OrdinalIgnoreCase) ||
                                                       p.Name.Contains("English", StringComparison.OrdinalIgnoreCase));
            }
            SelectedVoice = lastVoice ?? Voices.FirstOrDefault();
            if (SelectedVoice == null)
            {
                return;
            }

            HasLanguageParameter = engine.HasLanguageParameter;
            HasApiKey = engine.HasApiKey;
            HasRegion = engine.HasRegion;
            HasModel = engine.HasModel;
            HasKeyFile = engine.HasKeyFile;

            if (HasLanguageParameter)
            {
                var languages = await engine.GetLanguages(SelectedVoice, SelectedModel);
                Languages.Clear();
                foreach (var language in languages)
                {
                    Languages.Add(language);
                }

                SelectedLanguage = Languages.FirstOrDefault();
            }

            if (HasRegion)
            {
                var regions = await engine.GetRegions();
                Regions.Clear();
                foreach (var region in regions)
                {
                    Regions.Add(region);
                }

                SelectedRegion = Regions.FirstOrDefault();
            }

            if (HasModel)
            {
                var models = await engine.GetModels();
                Models.Clear();
                foreach (var model in models)
                {
                    Models.Add(model);
                }

                SelectedModel = Models.FirstOrDefault();
            }

            if (SelectedEngine is AzureSpeech)
            {
                ApiKey = Se.Settings.Video.TextToSpeech.AzureApiKey;
                SelectedRegion = Se.Settings.Video.TextToSpeech.AzureRegion;
                if (string.IsNullOrEmpty(SelectedRegion))
                {
                    SelectedRegion = "westeurope";
                }
            }
            else if (SelectedEngine is ElevenLabs)
            {
                ApiKey = Se.Settings.Video.TextToSpeech.ElevenLabsApiKey;
                SelectedModel = Models.FirstOrDefault(p => p == Se.Settings.Video.TextToSpeech.ElevenLabsModel);
                if (string.IsNullOrEmpty(SelectedModel))
                {
                    SelectedModel = Models.First();
                }
                IsEngineSettingsVisible = true;
            }
        });
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
            UiUtil.ShowHelp("features/text-to-speech");
        }
    }

    internal void OnClosing(WindowClosingEventArgs e)
    {
        lock (_playLock)
        {
            _mpvContext?.Dispose();
            _mpvContext = null;
        }
        UiUtil.SaveWindowPosition(Window);
    }

    internal void OnLoaded(RoutedEventArgs e)
    {
        UiUtil.RestoreWindowPosition(Window);
        LoadSettings();
        _timer.Start();
    }

    internal void SelectedLanguageChanged(object? sender, SelectionChangedEventArgs e)
    {
        var engine = SelectedEngine;
        if (engine == null)
        {
            return;
        }

        if (engine is Murf murf)
        {
            Dispatcher.UIThread.Post(async () =>
            {
                var voices = await murf.GetVoices(SelectedLanguage?.Code ?? string.Empty);
                Voices.Clear();
                Voices.AddRange(voices);

                var lastVoice = Voices.FirstOrDefault(v => v.Name == Se.Settings.Video.TextToSpeech.Voice);
                if (lastVoice == null)
                {
                    lastVoice = Voices.FirstOrDefault(p => p.Name.StartsWith("en", StringComparison.OrdinalIgnoreCase) ||
                                                           p.Name.Contains("English", StringComparison.OrdinalIgnoreCase));
                }
                SelectedVoice = lastVoice ?? Voices.First();
            });
        }
    }

    internal void SelectedModelChanged(object? sender, SelectionChangedEventArgs e)
    {
        var engine = SelectedEngine;
        var voice = SelectedVoice;
        var model = SelectedModel;
        if (engine == null || voice == null || model == null)
        {
            return;
        }

        Dispatcher.UIThread.Post(async () =>
        {
            if (engine.HasLanguageParameter)
            {
                var languages = await engine.GetLanguages(voice, model);
                Languages.Clear();
                foreach (var language in languages)
                {
                    Languages.Add(language);
                }

                SelectedLanguage = Languages.FirstOrDefault(p => p.Name == Se.Settings.Video.TextToSpeech.ElevenLabsLanguage);
                if (SelectedLanguage == null)
                {
                    SelectedLanguage = Languages.FirstOrDefault(p => p.Code == "en");
                }
            }
        });
    }
}