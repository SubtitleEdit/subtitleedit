using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.AdvancedTtsSettings;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.DownloadTts;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.ElevenLabsSettings;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.EncodingSettings;
using Nikse.SubtitleEdit.Features.Video.SpeechToText;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.ChatterboxTtsSettings;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.KokoroTtsSettings;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.OmniVoiceSettings;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Qwen3TtsSettings;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Qwen3TtsCrispAsrSettings;
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
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
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
    [ObservableProperty] private bool _isVoiceComboEnabled;
    [ObservableProperty] private bool _doReviewAudioClips;
    [ObservableProperty] private bool _doGenerateVideoFile;
    [ObservableProperty] private bool _isEdgeTtsEngine;
    [ObservableProperty] private bool _isGenerating;
    [ObservableProperty] private bool _isNotGenerating;
    [ObservableProperty] private bool _isEngineSettingsVisible;
    [ObservableProperty] private string _progressText;
    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private double _progressOpacity;
    [ObservableProperty] private string _doneOrCancelText;
    [ObservableProperty] private bool _hasKeyFile;
    [ObservableProperty] private string _keyFile;
    [ObservableProperty] private bool _hasInstruction;
    [ObservableProperty] private bool _isInstructionTextVisible;
    [ObservableProperty] private bool _isInstructionPickerVisible;
    [ObservableProperty] private bool _isInstructionPickerEnabled;
    [ObservableProperty] private bool _isInstructionVoiceHintVisible;
    [ObservableProperty] private string _instruction;
    [ObservableProperty] private string _selectedOmniVoiceGender;
    [ObservableProperty] private string _selectedOmniVoiceAge;
    [ObservableProperty] private string _selectedOmniVoicePitch;
    [ObservableProperty] private string _selectedOmniVoiceAccent;
    [ObservableProperty] private bool _omniVoiceWhisper;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    // Set by the window; re-evaluates the install-status dots in the engine and model combos.
    public Action? RefreshDownloadDots { get; set; }

    // The OmniVoice TTS voice-design keyword picker - one combo box per mutually exclusive
    // group. omnivoice-tts' --instruct only accepts these keyword values.
    public ObservableCollection<string> OmniVoiceGenders { get; }
    public ObservableCollection<string> OmniVoiceAges { get; }
    public ObservableCollection<string> OmniVoicePitches { get; }
    public ObservableCollection<string> OmniVoiceAccents { get; }

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
    private bool _suppressKeywordSync;
    private const string OmniVoiceAny = "(any)";

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
        IsVoiceComboEnabled = true;
        IsGenerating = false;
        IsNotGenerating = true;
        KeyFile = string.Empty;
        Instruction = string.Empty;

        OmniVoiceGenders = BuildKeywordOptions(OmniVoiceTtsCpp.InstructionGenders);
        OmniVoiceAges = BuildKeywordOptions(OmniVoiceTtsCpp.InstructionAges);
        OmniVoicePitches = BuildKeywordOptions(OmniVoiceTtsCpp.InstructionPitches);
        OmniVoiceAccents = BuildKeywordOptions(OmniVoiceTtsCpp.InstructionAccents);

        _suppressKeywordSync = true;
        SelectedOmniVoiceGender = OmniVoiceAny;
        SelectedOmniVoiceAge = OmniVoiceAny;
        SelectedOmniVoicePitch = OmniVoiceAny;
        SelectedOmniVoiceAccent = OmniVoiceAny;
        _suppressKeywordSync = false;

        _cancellationTokenSource = new CancellationTokenSource();
        _playLock = new Lock();
        _timer = new Timer(100);
        _timer.Elapsed += OnTimerOnElapsed;
        _waveFolder = string.Empty;
        _wavePeakData = new WavePeakData2(1, new List<WavePeak2>());
        _mediaInfo = null;

        Engines =
        [
            new EdgeTts(),
            new AllTalk(ttsDownloadService),
            new ElevenLabs(ttsDownloadService),
            new AzureSpeech(ttsDownloadService),
            new MistralSpeech(ttsDownloadService),
            new Murf(ttsDownloadService),
            new GoogleSpeech(ttsDownloadService),
            new Qwen3TtsCpp(),
            new Qwen3TtsCrispAsr(),
            new KokoroTtsCpp(),
            new ChatterboxTtsCpp(),
            new OmniVoiceTtsCpp(),
        ];

        if (!OperatingSystem.IsMacOS())
        {
            Engines.Insert(0, new Piper(ttsDownloadService));
        }
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
        // Migrate the pre-rename engine name so users who had "Chatterbox TTS" saved
        // before the rename to "Chatterbox TTS (CrispASR)" land on the same engine.
        // Cheap enough to do unconditionally; only kicks in once per affected install.
        var savedEngine = Se.Settings.Video.TextToSpeech.Engine ?? string.Empty;
        if (savedEngine == "Chatterbox TTS")
        {
            savedEngine = "Chatterbox TTS (CrispASR)";
        }

        var lastEngine = Engines.FirstOrDefault(e => e.Name == savedEngine);
        if (lastEngine != null)
        {
            SelectedEngine = lastEngine;
        }

        if (SelectedEngine != null)
        {
            HasLanguageParameter = SelectedEngine.HasLanguageParameter;
            HasApiKey = SelectedEngine.HasApiKey;
            HasRegion = SelectedEngine.HasRegion;
            HasModel = SelectedEngine.HasModel;
            HasKeyFile = SelectedEngine.HasKeyFile;
            IsEdgeTtsEngine = SelectedEngine is EdgeTts;
        }

        ApplyInstructionForEngine(SelectedEngine);

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
        else if (SelectedEngine is MistralSpeech)
        {
            ApiKey = Se.Settings.Video.TextToSpeech.MistralApiKey;
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
        else if (SelectedEngine is MistralSpeech)
        {
            Se.Settings.Video.TextToSpeech.MistralApiKey = ApiKey;
            Se.Settings.Video.TextToSpeech.MistralModel = SelectedModel ?? "voxtral-mini-tts-2603";
        }
        else if (SelectedEngine is Qwen3TtsCpp)
        {
            Se.Settings.Video.TextToSpeech.Qwen3TtsCppModel = SelectedModel ?? Qwen3TtsCpp.DefaultModelKey;
            Se.Settings.Video.TextToSpeech.Qwen3TtsCppInstruction = (Instruction ?? string.Empty).Trim();
        }
        else if (SelectedEngine is Qwen3TtsCrispAsr)
        {
            Se.Settings.Video.TextToSpeech.Qwen3TtsCrispAsrModel = SelectedModel ?? Qwen3TtsCrispAsr.DefaultModelKey;
            // Shared instruction text with the qwen3-tts.cpp engine so the same voice
            // description applies whichever Qwen3 backend the user picks.
            Se.Settings.Video.TextToSpeech.Qwen3TtsCppInstruction = (Instruction ?? string.Empty).Trim();
        }
        else if (SelectedEngine is OmniVoiceTtsCpp)
        {
            Se.Settings.Video.TextToSpeech.OmniVoiceTtsCppInstruction = (Instruction ?? string.Empty).Trim();
        }
        else if (SelectedEngine is ChatterboxTtsCpp)
        {
            Se.Settings.Video.TextToSpeech.ChatterboxModel = SelectedModel ?? ChatterboxTtsCpp.DefaultModelKey;
        }
        else if (SelectedEngine is KokoroTtsCpp)
        {
            if (SelectedVoice?.EngineVoice is Voices.KokoroTtsVoice kokoroVoice && !string.IsNullOrEmpty(kokoroVoice.Voice))
            {
                Se.Settings.Video.TextToSpeech.KokoroVoice = kokoroVoice.Voice;
            }
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

    // The instruction control is shared in the UI; each engine that supports it keeps its own
    // persisted value, so it always reflects the currently selected engine. Both Qwen3 engines
    // (qwen3-tts.cpp and CrispASR) share the same Qwen3TtsCppInstruction setting so users get
    // the same voice description whichever backend they're testing with.
    private static string GetInstructionForEngine(ITtsEngine? engine) => engine switch
    {
        Qwen3TtsCpp => Se.Settings.Video.TextToSpeech.Qwen3TtsCppInstruction ?? string.Empty,
        Qwen3TtsCrispAsr => Se.Settings.Video.TextToSpeech.Qwen3TtsCppInstruction ?? string.Empty,
        OmniVoiceTtsCpp => Se.Settings.Video.TextToSpeech.OmniVoiceTtsCppInstruction ?? string.Empty,
        _ => string.Empty,
    };

    // Loads the saved instruction value for the engine and refreshes which instruction control
    // is shown (free-text box for Qwen3 VoiceDesign, keyword picker for OmniVoice).
    private void ApplyInstructionForEngine(ITtsEngine? engine)
    {
        Instruction = GetInstructionForEngine(engine);

        if (engine is OmniVoiceTtsCpp)
        {
            SyncOmniVoicePickerFromInstruction();
        }

        RefreshInstructionVisibility();
        UpdateVoiceLock();
        UpdateOmniVoicePickerState();
    }

    // Qwen3's free-text instruction only does anything on the VoiceDesign model - the 0.6B and
    // 1.7B Base models are not instruction-tuned. OmniVoice always shows its keyword picker.
    private void RefreshInstructionVisibility()
    {
        IsInstructionTextVisible =
            (SelectedEngine is Qwen3TtsCpp && Qwen3TtsCpp.IsVoiceDesignModel(SelectedModel))
            || (SelectedEngine is Qwen3TtsCrispAsr && Qwen3TtsCrispAsr.IsVoiceDesignModel(SelectedModel));
        IsInstructionPickerVisible = SelectedEngine is OmniVoiceTtsCpp;
        HasInstruction = IsInstructionTextVisible || IsInstructionPickerVisible;
    }

    // The Qwen3 VoiceDesign model has no speaker encoder - the voice is defined entirely by the
    // instruction. Lock the voice combo to the "Default" entry so an imported voice cannot route
    // through the (unsupported) cloning path. Applies to both Qwen3 engines (qwen3-tts.cpp and
    // CrispASR) when the VoiceDesign model is selected.
    private void UpdateVoiceLock()
    {
        var isVoiceDesign =
            (SelectedEngine is Qwen3TtsCpp && Qwen3TtsCpp.IsVoiceDesignModel(SelectedModel))
            || (SelectedEngine is Qwen3TtsCrispAsr && Qwen3TtsCrispAsr.IsVoiceDesignModel(SelectedModel));
        IsVoiceComboEnabled = !isVoiceDesign;

        if (isVoiceDesign
            && SelectedVoice?.EngineVoice is Voices.Qwen3TtsVoice currentVoice
            && !string.IsNullOrEmpty(currentVoice.FilePath))
        {
            var defaultVoice = Voices.FirstOrDefault(v =>
                v.EngineVoice is Voices.Qwen3TtsVoice q && string.IsNullOrEmpty(q.FilePath));
            if (defaultVoice != null)
            {
                SelectedVoice = defaultVoice;
            }
        }
    }

    partial void OnSelectedModelChanged(string? value)
    {
        RefreshInstructionVisibility();
        UpdateVoiceLock();

        // Qwen3 (CrispASR) returns a different voice list per model — VoiceDesign exposes
        // "Default", CustomVoice only shows imported WAVs since it can't synthesise without
        // a reference. Persist the model change first so GetVoices reads the new value, then
        // re-pull the voice list. Wrap the fire-and-forget in a try/catch so any throw doesn't
        // surface later as an unobserved task exception; the worst case is the combo keeps
        // showing the old voice list.
        if (SelectedEngine is Qwen3TtsCrispAsr engine)
        {
            Se.Settings.Video.TextToSpeech.Qwen3TtsCrispAsrModel = value ?? Qwen3TtsCrispAsr.DefaultModelKey;
            _ = Task.Run(async () =>
            {
                try
                {
                    await Dispatcher.UIThread.InvokeAsync(async () => await RefreshVoices(engine));
                }
                catch (Exception ex)
                {
                    Se.LogError(ex, "Qwen3 TTS (CrispASR): refreshing voices on model change failed");
                }
            });
        }
    }

    // omnivoice-tts applies voice-design keywords only without a reference WAV, so the picker
    // is usable for the "Default" voice but disabled (with a note) for cloned voices.
    private void UpdateOmniVoicePickerState()
    {
        var isOmniVoice = SelectedEngine is OmniVoiceTtsCpp;
        var isDefaultVoice = SelectedVoice?.EngineVoice is Voices.OmniVoiceVoice omniVoice
                             && string.IsNullOrEmpty(omniVoice.FilePath);

        IsInstructionPickerEnabled = isOmniVoice && isDefaultVoice;
        IsInstructionVoiceHintVisible = isOmniVoice && !isDefaultVoice;
    }

    partial void OnSelectedVoiceChanged(Voice? value)
    {
        UpdateOmniVoicePickerState();
    }

    private static ObservableCollection<string> BuildKeywordOptions(string[] keywords)
    {
        var options = new ObservableCollection<string> { OmniVoiceAny };
        foreach (var keyword in keywords)
        {
            options.Add(keyword);
        }
        return options;
    }

    private static bool IsRealOmniVoiceKeyword(string? value)
    {
        return !string.IsNullOrEmpty(value) && value != OmniVoiceAny;
    }

    partial void OnSelectedOmniVoiceGenderChanged(string value) => OnOmniVoicePickerChanged();
    partial void OnSelectedOmniVoiceAgeChanged(string value) => OnOmniVoicePickerChanged();
    partial void OnSelectedOmniVoicePitchChanged(string value) => OnOmniVoicePickerChanged();
    partial void OnSelectedOmniVoiceAccentChanged(string value) => OnOmniVoicePickerChanged();
    partial void OnOmniVoiceWhisperChanged(bool value) => OnOmniVoicePickerChanged();

    private void OnOmniVoicePickerChanged()
    {
        if (!_suppressKeywordSync)
        {
            Instruction = BuildOmniVoiceInstruction();
        }
    }

    private string BuildOmniVoiceInstruction()
    {
        var parts = new List<string>();
        if (IsRealOmniVoiceKeyword(SelectedOmniVoiceGender)) parts.Add(SelectedOmniVoiceGender);
        if (IsRealOmniVoiceKeyword(SelectedOmniVoiceAge)) parts.Add(SelectedOmniVoiceAge);
        if (IsRealOmniVoiceKeyword(SelectedOmniVoicePitch)) parts.Add(SelectedOmniVoicePitch);
        if (IsRealOmniVoiceKeyword(SelectedOmniVoiceAccent)) parts.Add(SelectedOmniVoiceAccent);
        if (OmniVoiceWhisper) parts.Add(OmniVoiceTtsCpp.InstructionWhisper);
        return string.Join(", ", parts);
    }

    // Reflects the saved instruction string into the picker controls, then rewrites Instruction
    // from the picker so any unrecognised text (e.g. legacy free-text values from before the
    // picker existed) is dropped instead of failing omnivoice-tts.
    private void SyncOmniVoicePickerFromInstruction()
    {
        var present = (Instruction ?? string.Empty)
            .Split(',')
            .Select(s => s.Trim())
            .Where(s => s.Length > 0)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        _suppressKeywordSync = true;
        SelectedOmniVoiceGender = MatchOmniVoiceGroup(OmniVoiceTtsCpp.InstructionGenders, present);
        SelectedOmniVoiceAge = MatchOmniVoiceGroup(OmniVoiceTtsCpp.InstructionAges, present);
        SelectedOmniVoicePitch = MatchOmniVoiceGroup(OmniVoiceTtsCpp.InstructionPitches, present);
        SelectedOmniVoiceAccent = MatchOmniVoiceGroup(OmniVoiceTtsCpp.InstructionAccents, present);
        OmniVoiceWhisper = present.Contains(OmniVoiceTtsCpp.InstructionWhisper);
        _suppressKeywordSync = false;

        Instruction = BuildOmniVoiceInstruction();
    }

    // The first keyword from the group present in the saved instruction, or "(any)" if none.
    private static string MatchOmniVoiceGroup(string[] group, HashSet<string> present)
    {
        return Array.Find(group, present.Contains) ?? OmniVoiceAny;
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

        // The engine and/or its models may have just been downloaded - refresh the combo dots.
        RefreshDownloadDots?.Invoke();

        var voice = SelectedVoice;
        if (voice != null && !await TtsVoiceInstaller.EnsureVoiceInstalled(engine, voice, Window, _windowService))
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

        Se.WriteToolsLog(
            $"Text-to-speech: engine={engine.Name}" +
            $", voice={SelectedVoice?.Name ?? "(none)"}" +
            $", language={SelectedLanguage?.Code ?? "(default)"}" +
            $", region={SelectedRegion ?? "(default)"}" +
            $", model={SelectedModel ?? "(default)"}" +
            $", segments={_subtitle.Paragraphs.Count}" +
            $", reviewAudioClips={DoReviewAudioClips}" +
            $", generateVideoFile={DoGenerateVideoFile}");

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

        // Post-processing (pro audio chain, silence padding, sample rate)
        var postProcessResult = await ApplyPostProcessing(fixSpeedResult, _cancellationToken);
        if (postProcessResult == null)
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
            var reviewAudioClipsResult = await ReviewAudioClips(postProcessResult);
            if (reviewAudioClipsResult == null)
            {
                DoneOrCancelText = Se.Language.General.Done;
                IsGenerating = false;
                IsNotGenerating = true;
                ProgressOpacity = 0;
                return;
            }

            postProcessResult = reviewAudioClipsResult;
        }

        await MergeAndAddToVideo(postProcessResult);
    }

    [RelayCommand]
    private async Task ShowEngineSettings()
    {
        if (SelectedEngine is OmniVoiceTtsCpp)
        {
            await _windowService.ShowDialogAsync<OmniVoiceSettingsWindow, OmniVoiceSettingsViewModel>(Window!, vm => vm.Initialize());
        }
        else if (SelectedEngine is Qwen3TtsCpp)
        {
            await _windowService.ShowDialogAsync<Qwen3TtsSettingsWindow, Qwen3TtsSettingsViewModel>(Window!, vm => vm.Initialize());
        }
        else if (SelectedEngine is Qwen3TtsCrispAsr)
        {
            await _windowService.ShowDialogAsync<Qwen3TtsCrispAsrSettingsWindow, Qwen3TtsCrispAsrSettingsViewModel>(Window!, vm => vm.Initialize());
        }
        else if (SelectedEngine is KokoroTtsCpp)
        {
            await _windowService.ShowDialogAsync<KokoroTtsSettingsWindow, KokoroTtsSettingsViewModel>(Window!, vm => vm.Initialize());
        }
        else if (SelectedEngine is ChatterboxTtsCpp)
        {
            await _windowService.ShowDialogAsync<ChatterboxTtsSettingsWindow, ChatterboxTtsSettingsViewModel>(Window!, vm => vm.Initialize());
        }
        else
        {
            await _windowService.ShowDialogAsync<ElevenLabsSettingsWindow, ElevenLabsSettingsViewModel>(Window!, vm => { });
        }

        // An engine may have been (re)downloaded inside its settings dialog - re-check the
        // install-status dots in the engine and model combos.
        RefreshDownloadDots?.Invoke();
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

        RefreshDownloadDots?.Invoke();

        if (!await TtsVoiceInstaller.EnsureVoiceInstalled(engine, voice, Window, _windowService))
        {
            return;
        }

        SaveSettings();

        var text = Se.Settings.Video.TextToSpeech.VoiceTestText;
        if (string.IsNullOrEmpty(text))
        {
            text = "This is a test";
        }

        var generatingAudioVm = _windowService.ShowWindow<GeneratingAudioWindow, GeneratingAudioViewModel>(Window!);
        _cancellationTokenSource = generatingAudioVm.CancellationTokenSource;
        _cancellationToken = _cancellationTokenSource.Token;
        try
        {
            var result = await engine.Speak(text, _waveFolder, voice, SelectedLanguage, SelectedRegion, SelectedModel, _cancellationToken);
            if (!_cancellationToken.IsCancellationRequested)
            {
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
        }
        catch (OperationCanceledException)
        {
            // User clicked Cancel on the GeneratingAudio window — close quietly.
        }
        catch (Exception ex)
        {
            // Anything else (engine startup failure, server crash, network error, ...) must
            // not propagate or the Avalonia dispatcher tears the whole app down. Surface a
            // dialog with the message instead.
            Se.LogError(ex, "Test voice failed.");
            if (Window != null)
            {
                await MessageBox.Show(
                    Window,
                    "Test voice error",
                    ex.Message,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        finally
        {
            generatingAudioVm.Close();
        }
    }


    [RelayCommand]
    private async Task ShowTestVoiceSettings()
    {
        var engine = SelectedEngine;
        if (engine == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<VoiceSettingsWindow, VoiceSettingsViewModel>(Window!, vm => 
        {
            vm.Initialize(engine);
        });

        if (result.RefreshVoices)
        {
            await RefreshVoices(engine);
        }
    }

    private async Task RefreshVoices(ITtsEngine engine)
    {
        var voices = await engine.RefreshVoices(string.Empty, CancellationToken.None);
        Voices.Clear();
        foreach (var voice in voices)
        {
            Voices.Add(voice);
        }
        SelectedVoice = Voices.FirstOrDefault(v => v.Name == Se.Settings.Video.TextToSpeech.Voice) ?? Voices.FirstOrDefault();
    }

    [RelayCommand]
    private async Task ShowEncodingSettings()
    {
        await _windowService.ShowDialogAsync<EncodingSettingsWindow, EncodingSettingsViewModel>(Window!, vm => { });
    }

    [RelayCommand]
    private async Task ShowAdvancedSettings()
    {
        await _windowService.ShowDialogAsync<AdvancedTtsSettingsWindow, AdvancedTtsSettingsViewModel>(Window!, vm =>
        {
            vm.IsEdgeTtsEngine = SelectedEngine is EdgeTts;
        });
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
            _mpvContext.LoadLib(); // core not initialized"
            var err = _mpvContext.Initialize();
            if (err < 0)
            {
                throw new InvalidOperationException($"Failed to initialize mpv: {_mpvContext.GetErrorString(err)}");
            }
        }
        await _mpvContext.LoadAudio(fileName);
    }

    private async Task<bool> IsEngineInstalled(ITtsEngine engine)
    {
        if (Window == null)
        {
            return false;
        }

        if (engine is Qwen3TtsCpp)
        {
            if (!await engine.IsInstalled(SelectedRegion))
            {
                var qwen3Variant = Qwen3TtsCppDownloadService.WindowsVariantVulkan;
                if (Configuration.IsRunningOnWindows)
                {
                    var variantAnswer = await MessageBox.Show(
                        Window,
                        "Download Qwen3 TTS?",
                        $"{Environment.NewLine}\"Text to speech\" requires Qwen3 TTS.{Environment.NewLine}{Environment.NewLine}Select a build to download:",
                        MessageBoxButtons.Cancel,
                        MessageBoxIcon.Question,
                        "CPU",
                        "Vulkan (GPU)",
                        "CUDA (NVIDIA GPU)");

                    if (variantAnswer == MessageBoxResult.None || variantAnswer == MessageBoxResult.Cancel)
                    {
                        return false;
                    }

                    qwen3Variant = variantAnswer switch
                    {
                        MessageBoxResult.Custom1 => Qwen3TtsCppDownloadService.WindowsVariantCpu,
                        MessageBoxResult.Custom3 => Qwen3TtsCppDownloadService.WindowsVariantCuda,
                        _ => Qwen3TtsCppDownloadService.WindowsVariantVulkan,
                    };

                    if (qwen3Variant == Qwen3TtsCppDownloadService.WindowsVariantVulkan && !VulkanHelper.IsInstalled())
                    {
                        var vulkanAnswer = await MessageBox.Show(
                            Window,
                            "Vulkan runtime may be required",
                            $"The Vulkan version requires the Vulkan runtime (vulkan-1.dll) which usually ships with current GPU drivers, but was not detected on this system.{Environment.NewLine}{Environment.NewLine}You can install it from:{Environment.NewLine}https://vulkan.lunarg.com/sdk/home{Environment.NewLine}{Environment.NewLine}Continue with Vulkan download anyway?",
                            MessageBoxButtons.YesNoCancel,
                            MessageBoxIcon.Question);

                        if (vulkanAnswer == MessageBoxResult.No)
                        {
                            UiUtil.OpenUrl("https://vulkan.lunarg.com/sdk/home");
                            return false;
                        }

                        if (vulkanAnswer != MessageBoxResult.Yes)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    var answer = await MessageBox.Show(
                        Window,
                        "Download Qwen3 TTS?",
                        $"{Environment.NewLine}\"Text to speech\" requires Qwen3 TTS.{Environment.NewLine}{Environment.NewLine}Download and use Qwen3 TTS?",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question);

                    if (answer != MessageBoxResult.Yes)
                    {
                        return false;
                    }
                }

                var dlResult = await _windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(Window, vm => vm.StartDownloadQwen3TtsCpp(qwen3Variant));
                if (!dlResult.OkPressed)
                {
                    return false;
                }

                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await RefreshVoices(engine);
                });
            }

            var qwen3ModelKey = Qwen3TtsCpp.ResolveModelKey(SelectedModel);
            if (!Qwen3TtsCpp.IsModelsInstalled(qwen3ModelKey))
            {
                var sizeText = qwen3ModelKey switch
                {
                    Qwen3TtsCpp.ModelKey17BBase => "~2.7 GB",
                    Qwen3TtsCpp.ModelKey17BVoiceDesign => "~2.8 GB",
                    _ => "~1.6 GB",
                };
                var answer = await MessageBox.Show(
                    Window,
                    "Download Qwen3 TTS models?",
                    $"{Environment.NewLine}\"Qwen3 TTS\" ({qwen3ModelKey}) requires models ({sizeText}).{Environment.NewLine}{Environment.NewLine}Download models?",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (answer != MessageBoxResult.Yes)
                {
                    return false;
                }

                var dlResult = await _windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(Window!, vm => vm.StartDownloadQwen3TtsModels(qwen3ModelKey));
                return dlResult.OkPressed && Qwen3TtsCpp.IsModelsInstalled(qwen3ModelKey);
            }

            return true;
        }

        if (engine is Qwen3TtsCrispAsr)
        {
            // Runtime first: the same crispasr.exe that Speech-to-text / Chatterbox use.
            if (!await TtsVoiceInstaller.EnsureCrispAsrForQwen3(Window, _windowService, forceRedownload: false))
            {
                return false;
            }

            var crispAsrModelKey = Qwen3TtsCrispAsr.ResolveModelKey(SelectedModel);
            if (!Qwen3TtsCrispAsr.AreModelsInstalled(crispAsrModelKey))
            {
                // Both keys ship the same ~358 MB 12 Hz codec; the talker is ~2 GB regardless.
                const string sizeText = "~2.4 GB";
                var answer = await MessageBox.Show(
                    Window,
                    "Download Qwen3 TTS (CrispASR) models?",
                    $"{Environment.NewLine}\"Qwen3 TTS (CrispASR)\" ({crispAsrModelKey}) requires models ({sizeText}).{Environment.NewLine}{Environment.NewLine}Download models?",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (answer != MessageBoxResult.Yes)
                {
                    return false;
                }

                var dlResult = await _windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(Window!, vm => vm.StartDownloadQwen3TtsCrispAsrModels(crispAsrModelKey));
                return dlResult.OkPressed && Qwen3TtsCrispAsr.AreModelsInstalled(crispAsrModelKey);
            }

            return true;
        }

        if (engine is KokoroTtsCpp)
        {
            if (!await engine.IsInstalled(SelectedRegion))
            {
                var answer = await MessageBox.Show(
                    Window,
                    "Download Kokoro TTS?",
                    $"{Environment.NewLine}\"Text to speech\" requires Kokoro TTS.{Environment.NewLine}{Environment.NewLine}Download and use Kokoro TTS?",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (answer != MessageBoxResult.Yes)
                {
                    return false;
                }

                var dlResult = await _windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(Window, vm => vm.StartDownloadKokoroTtsCpp());
                if (!dlResult.OkPressed)
                {
                    return false;
                }

                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await RefreshVoices(engine);
                });
            }

            if (!KokoroTtsCpp.AreModelsInstalled())
            {
                var answer = await MessageBox.Show(
                    Window,
                    "Download Kokoro TTS models?",
                    $"{Environment.NewLine}\"Kokoro TTS\" requires models (~380 MB).{Environment.NewLine}{Environment.NewLine}Download models?",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (answer != MessageBoxResult.Yes)
                {
                    return false;
                }

                var dlResult = await _windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(Window!, vm => vm.StartDownloadKokoroTtsModels());
                return dlResult.OkPressed && KokoroTtsCpp.AreModelsInstalled();
            }

            return true;
        }

        if (engine is ChatterboxTtsCpp)
        {
            if (!await TtsVoiceInstaller.EnsureCrispAsrForChatterbox(Window, _windowService, forceRedownload: false))
            {
                return false;
            }

            var chatterboxModelKey = ChatterboxTtsCpp.ResolveModelKey(SelectedModel);
            if (!ChatterboxTtsCpp.AreModelsInstalled(chatterboxModelKey))
            {
                var sizeText = chatterboxModelKey == ChatterboxTtsCpp.ModelKeyTurbo ? "~1 GB" : "~990 MB";
                var answer = await MessageBox.Show(
                    Window,
                    "Download Chatterbox TTS models?",
                    $"{Environment.NewLine}\"Chatterbox TTS\" ({chatterboxModelKey}) requires models ({sizeText}).{Environment.NewLine}{Environment.NewLine}Download models?",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (answer != MessageBoxResult.Yes)
                {
                    return false;
                }

                var dlResult = await _windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(Window!, vm => vm.StartDownloadChatterboxModels(chatterboxModelKey));
                return dlResult.OkPressed && ChatterboxTtsCpp.AreModelsInstalled(chatterboxModelKey);
            }

            return true;
        }

        if (engine is OmniVoiceTtsCpp)
        {
            if (!await engine.IsInstalled(SelectedRegion))
            {
                var omniVariant = OmniVoiceDownloadService.WindowsVariantVulkan;
                if (Configuration.IsRunningOnWindows)
                {
                    var variantAnswer = await MessageBox.Show(
                        Window,
                        "Download OmniVoice TTS?",
                        $"{Environment.NewLine}\"Text to speech\" requires OmniVoice TTS.{Environment.NewLine}{Environment.NewLine}Select a build to download:",
                        MessageBoxButtons.Cancel,
                        MessageBoxIcon.Question,
                        "CPU",
                        "Vulkan",
                        "CUDA");

                    if (variantAnswer == MessageBoxResult.None || variantAnswer == MessageBoxResult.Cancel)
                    {
                        return false;
                    }

                    omniVariant = variantAnswer switch
                    {
                        MessageBoxResult.Custom1 => OmniVoiceDownloadService.WindowsVariantCpu,
                        MessageBoxResult.Custom3 => OmniVoiceDownloadService.WindowsVariantCuda,
                        _ => OmniVoiceDownloadService.WindowsVariantVulkan,
                    };

                    if (omniVariant == OmniVoiceDownloadService.WindowsVariantVulkan && !VulkanHelper.IsInstalled())
                    {
                        var vulkanAnswer = await MessageBox.Show(
                            Window,
                            "Vulkan runtime may be required",
                            $"The Vulkan version requires the Vulkan runtime (vulkan-1.dll) which usually ships with current GPU drivers, but was not detected on this system.{Environment.NewLine}{Environment.NewLine}You can install it from:{Environment.NewLine}https://vulkan.lunarg.com/sdk/home{Environment.NewLine}{Environment.NewLine}Continue with Vulkan download anyway?",
                            MessageBoxButtons.YesNoCancel,
                            MessageBoxIcon.Question);

                        if (vulkanAnswer == MessageBoxResult.No)
                        {
                            UiUtil.OpenUrl("https://vulkan.lunarg.com/sdk/home");
                            return false;
                        }

                        if (vulkanAnswer != MessageBoxResult.Yes)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    var answer = await MessageBox.Show(
                        Window,
                        "Download OmniVoice TTS?",
                        $"{Environment.NewLine}\"Text to speech\" requires OmniVoice TTS.{Environment.NewLine}{Environment.NewLine}Download and use OmniVoice TTS?",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question);

                    if (answer != MessageBoxResult.Yes)
                    {
                        return false;
                    }
                }

                var dlResult = await _windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(Window, vm => vm.StartDownloadOmniVoice(omniVariant));
                if (!dlResult.OkPressed)
                {
                    return false;
                }

                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await RefreshVoices(engine);
                });
            }

            if (!OmniVoiceTtsCpp.IsModelsInstalled())
            {
                var answer = await MessageBox.Show(
                    Window,
                    "Download OmniVoice TTS models?",
                    $"{Environment.NewLine}\"OmniVoice TTS\" requires models (~1.4 GB).{Environment.NewLine}{Environment.NewLine}Download models?",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (answer != MessageBoxResult.Yes)
                {
                    return false;
                }

                var dlResult = await _windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(Window!, vm => vm.StartDownloadOmniVoiceModels());
                return dlResult.OkPressed && OmniVoiceTtsCpp.IsModelsInstalled();
            }

            return true;
        }

        if (await engine.IsInstalled(SelectedRegion) || Window == null)
        {
            return true;
        }

        if (engine is Piper)
        {
            var answer = await MessageBox.Show(
                Window,
                string.Format(Se.Language.General.DownloadX, "Piper"),
                Se.Language.Video.TextToSpeech.DownloadPiperPrompt,
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

        if (engine is EdgeTts)
        {
            var answer = await MessageBox.Show(
                Window,
                Se.Language.General.Error,
                $"\"EdgeTts\" text to speech requires the edge-tts CLI tool.{Environment.NewLine}{Environment.NewLine}Install with: pipx install edge-tts{Environment.NewLine}(or pip install edge-tts){Environment.NewLine}{Environment.NewLine}Read more?",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (answer != MessageBoxResult.Yes)
            {
                return false;
            }

            await Window.Launcher.LaunchUriAsync(new Uri("https://github.com/rany2/edge-tts"));
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

        var addAudioProcess = Se.Settings.Video.TextToSpeech.AudioDuckingEnabled
            ? FfmpegGenerator.AddAudioTrackWithDucking(_videoFileName, audioFileName, outputFileName, audioEncoding, stereo, Se.Settings.Video.TextToSpeech.AudioDuckingOriginalVolume)
            : FfmpegGenerator.AddAudioTrack(_videoFileName, audioFileName, outputFileName, audioEncoding, stereo);
        await addAudioProcess.StartAndWaitAsync(cancellationToken);

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
                await mergeProcess.StartAndWaitAsync(cancellationToken);

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
        await silenceProcess.StartAndWaitAsync(cancellationToken);
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

            var failedCount = resultList.Count(r => string.IsNullOrEmpty(r.CurrentFileName));
            if (failedCount == resultList.Count && resultList.Count > 0)
            {
                var msg = $"Text-to-speech failed for all {failedCount} segments. " +
                          "Check the engine settings (rate/pitch/volume must be a signed integer, e.g. \"+10\") and try again.";
                SeLogger.Error(msg);
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    if (Window != null)
                    {
                        await MessageBox.Show(Window, Se.Language.General.Error, msg, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                });
                return null;
            }

            if (failedCount > 0)
            {
                SeLogger.Error($"TextToSpeech: {failedCount} of {resultList.Count} segments failed to generate; continuing with the rest.");
            }

            return resultList.ToArray();
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        catch (HttpRequestException ex)
        {
            SeLogger.Error(ex, "TTS server error during speech generation.");
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                if (Window != null)
                {
                    await MessageBox.Show(
                        Window,
                        Se.Language.General.Error,
                        "TTS server error: " + ex.Message,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            });
            return null;
        }
        catch (Exception ex)
        {
            // Anything else (engine startup failure, server segfault, native crash, ...)
            // must not propagate or the Avalonia dispatcher tears the whole app down.
            SeLogger.Error(ex, "Unexpected error during speech generation.");
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                if (Window != null)
                {
                    await MessageBox.Show(
                        Window,
                        Se.Language.General.Error,
                        ex.Message,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            });
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

        var doVad = Se.Settings.Video.TextToSpeech.VadSilenceCompressionEnabled;
        var vadMaxSilence = Se.Settings.Video.TextToSpeech.VadMaxSilenceSeconds;
        var doHighQualityStretch = Se.Settings.Video.TextToSpeech.HighQualityTimeStretchEnabled;

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

                if (string.IsNullOrEmpty(item.CurrentFileName) || !File.Exists(item.CurrentFileName))
                {
                    SeLogger.Error($"TextToSpeech: skipping segment {index + 1} in FixSpeed - upstream produced no audio file");
                    continue;
                }

                // Step 1: Trim silence from start and end
                var outputFileName1 = Path.Combine(Path.GetDirectoryName(item.CurrentFileName)!, Guid.NewGuid() + ".wav");
                var trimProcess = FfmpegGenerator.TrimSilenceStartAndEnd(item.CurrentFileName, outputFileName1);
                await trimProcess.StartAndWaitAsync(cancellationToken);

                var currentFile = outputFileName1;

                // Step 2: VAD-based internal silence compression
                // Compress pauses between words/phrases before touching tempo.
                // This preserves phoneme quality by only removing redundant silence.
                if (doVad)
                {
                    var vadOutput = Path.Combine(Path.GetDirectoryName(item.CurrentFileName)!, $"vad_{Guid.NewGuid()}.wav");
                    var vadProcess = FfmpegGenerator.CompressInternalSilence(currentFile, vadOutput, vadMaxSilence);
                    await vadProcess.StartAndWaitAsync(cancellationToken);

                    if (File.Exists(vadOutput) && new FileInfo(vadOutput).Length > 0)
                    {
                        currentFile = vadOutput;
                    }
                }

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

                var mediaInfo = FfmpegMediaInfo.Parse(currentFile);
                if (mediaInfo.Duration == null)
                {
                    continue;
                }

                // If audio already fits after silence removal/compression, no time-stretching needed
                if (mediaInfo.Duration.TotalMilliseconds <= p.DurationTotalMilliseconds + addDuration)
                {
                    resultList.Add(new TtsStepResult
                    {
                        Paragraph = p,
                        Text = item.Text,
                        CurrentFileName = currentFile,
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

                // Step 3: Time-stretching (only for audio that still exceeds subtitle duration)
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

                // Use rubberband (WSOLA) for high-quality pitch-preserving stretch, or atempo as fallback
                Process speedProcess;
                if (doHighQualityStretch)
                {
                    speedProcess = FfmpegGenerator.ChangeSpeedHighQuality(currentFile, outputFileName2, (float)factor);
                }
                else
                {
                    speedProcess = FfmpegGenerator.ChangeSpeed(currentFile, outputFileName2, (float)factor);
                }
                await speedProcess.StartAndWaitAsync(cancellationToken);

                // Fallback: if rubberband failed (not available in FFmpeg build), retry with atempo
                if (doHighQualityStretch && (!File.Exists(outputFileName2) || new FileInfo(outputFileName2).Length == 0))
                {
                    var fallbackProcess = FfmpegGenerator.ChangeSpeed(currentFile, outputFileName2, (float)factor);
                    await fallbackProcess.StartAndWaitAsync(cancellationToken);
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

    private async Task<TtsStepResult[]?> ApplyPostProcessing(TtsStepResult[] previousStepResult, CancellationToken cancellationToken)
    {
        var doProChain = Se.Settings.Video.TextToSpeech.ProAudioChainEnabled;
        var silencePaddingMs = Se.Settings.Video.TextToSpeech.SilencePaddingMs;
        var outputSampleRate = Se.Settings.Video.TextToSpeech.OutputSampleRate;

        if (!doProChain && silencePaddingMs <= 0 && outputSampleRate <= 0)
        {
            return previousStepResult;
        }

        try
        {
            var resultList = new List<TtsStepResult>();
            ProgressValue = 0;

            for (var index = 0; index < previousStepResult.Length; index++)
            {
                ProgressText = $"Post-processing: segment {index + 1} of {previousStepResult.Length}";
                var item = previousStepResult[index];

                if (string.IsNullOrEmpty(item.CurrentFileName) || !File.Exists(item.CurrentFileName))
                {
                    SeLogger.Error($"TextToSpeech: skipping segment {index + 1} in post-processing - no audio file");
                    continue;
                }

                var processedFile = await TtsPostProcessor.ApplyPostProcessing(item.CurrentFileName, Path.GetDirectoryName(item.CurrentFileName)!, cancellationToken);

                resultList.Add(new TtsStepResult
                {
                    Paragraph = item.Paragraph,
                    Text = item.Text,
                    CurrentFileName = processedFile,
                    SpeedFactor = item.SpeedFactor,
                    Voice = item.Voice,
                });

                ProgressValue = (double)(index + 1) / previousStepResult.Length * 100.0;

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

        Dispatcher.UIThread.PostSafe(async () =>
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
            IsEdgeTtsEngine = engine is EdgeTts;
            ApplyInstructionForEngine(engine);

            if (HasLanguageParameter)
            {
                var languages = await engine.GetLanguages(SelectedVoice, SelectedModel);
                Languages.Clear();
                foreach (var language in languages)
                {
                    Languages.Add(language);
                }

                // OmniVoice has 646 alphabetically-sorted languages; the first entry ("Abadi") is
                // a useless default. Default to English so the engine is usable out of the box.
                SelectedLanguage = engine is OmniVoiceTtsCpp
                    ? Languages.FirstOrDefault(l => l.Code == "en") ?? Languages.FirstOrDefault()
                    : Languages.FirstOrDefault();
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
            else if (SelectedEngine is MistralSpeech)
            {
                ApiKey = Se.Settings.Video.TextToSpeech.MistralApiKey;
                SelectedModel = Models.FirstOrDefault(p => p == Se.Settings.Video.TextToSpeech.MistralModel);
                if (string.IsNullOrEmpty(SelectedModel))
                {
                    SelectedModel = Models.FirstOrDefault();
                }
            }
            else if (SelectedEngine is Qwen3TtsCpp)
            {
                SelectedModel = Models.FirstOrDefault(p => p == Se.Settings.Video.TextToSpeech.Qwen3TtsCppModel);
                if (string.IsNullOrEmpty(SelectedModel))
                {
                    SelectedModel = Models.FirstOrDefault();
                }
                IsEngineSettingsVisible = true;
            }
            else if (SelectedEngine is Qwen3TtsCrispAsr)
            {
                SelectedModel = Models.FirstOrDefault(p => p == Se.Settings.Video.TextToSpeech.Qwen3TtsCrispAsrModel);
                if (string.IsNullOrEmpty(SelectedModel))
                {
                    SelectedModel = Models.FirstOrDefault();
                }
                IsEngineSettingsVisible = true;
            }
            else if (SelectedEngine is ChatterboxTtsCpp)
            {
                SelectedModel = Models.FirstOrDefault(p => p == Se.Settings.Video.TextToSpeech.ChatterboxModel);
                if (string.IsNullOrEmpty(SelectedModel))
                {
                    SelectedModel = Models.FirstOrDefault();
                }
                IsEngineSettingsVisible = true;
            }
            else if (SelectedEngine is OmniVoiceTtsCpp)
            {
                IsEngineSettingsVisible = true;
            }
            else if (SelectedEngine is KokoroTtsCpp)
            {
                var savedVoice = Se.Settings.Video.TextToSpeech.KokoroVoice;
                if (!string.IsNullOrEmpty(savedVoice))
                {
                    var match = Voices.FirstOrDefault(v =>
                        v.EngineVoice is Voices.KokoroTtsVoice kv && kv.Voice == savedVoice);
                    if (match != null)
                    {
                        SelectedVoice = match;
                    }
                }
                IsEngineSettingsVisible = true;
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
            Dispatcher.UIThread.PostSafe(async () =>
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

        Dispatcher.UIThread.PostSafe(async () =>
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