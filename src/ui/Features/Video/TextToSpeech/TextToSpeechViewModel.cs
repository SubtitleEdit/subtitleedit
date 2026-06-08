using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Shared.PromptTextBox;
using Nikse.SubtitleEdit.Features.Tools.MergeContinuationLines;
using Nikse.SubtitleEdit.Features.Video.SpeechToText;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.ActorVoices;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.AdvancedTtsSettings;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.ChatterboxTtsSettings;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.CosyVoice3CrispAsrSettings;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.DownloadTts;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.ElevenLabsSettings;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.EncodingSettings;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.F5TtsCrispAsrSettings;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.VoxCPM2CrispAsrSettings;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.IndexTtsCrispAsrSettings;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.KokoroTtsSettings;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.PiperSettings;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.OmniVoiceSettings;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Qwen3TtsCrispAsrSettings;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Qwen3TtsSettings;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.ReviewSpeech;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.VibeVoiceCrispAsrSettings;
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
using System.Diagnostics;
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
    [ObservableProperty] private bool _hasCast;
    [ObservableProperty] private string _castButtonText;

    // Per-actor (ASSA) or per-voice (WebVTT) mappings the user has configured via the Cast
    // dialog. When empty, every paragraph falls back to the global SelectedEngine/SelectedVoice.
    private readonly List<ActorVoiceMapping> _actorVoiceMappings = new();
    private ActorVoiceDetector.CastKind _castKind = ActorVoiceDetector.CastKind.None;

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
        CastButtonText = Se.Language.Video.TextToSpeech.SetupCast;

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
            new KokoroTtsCpp(),
            new OmniVoiceTtsCpp(),
            
            // CrispASR-based engines grouped at the bottom: both share the same heavy CrispASR
            // runtime download (~hundreds of MB) and are typically picked together, so we group
            // them last so the lighter cloud/local engines surface first in the list.
            // Qwen3TtsCpp hidden: talker produces scrambled noise on 1.7B —
            // use Qwen3TtsCrispAsr until upstream qwen3-tts.cpp is fixed.
            new Qwen3TtsCrispAsr(),
            
            // VibeVoiceCrispAsr hidden: output quality is unusable in practice even after
            // bumping the post-synth speed knob (#11223). The engine class + download service +
            // settings dialog are kept so this becomes a one-line re-enable when upstream
            // CrispASR's VibeVoice backend ships a higher-quality build.
            // new VibeVoiceCrispAsr(),
            
            new IndexTtsCrispAsr(),
            
            // F5-TTS (CrispASR) hidden: CrispASR 0.6.12 has no GPU backend for f5-tts, so
            // synthesis runs the fixed 32-step Euler ODE through a 22-layer DiT + Vocos on
            // CPU only. That's 3-8 minutes per short utterance on Mac CPU — unusable for the
            // typical TTS-from-subtitles workflow. Engine + download service + settings dialog
            // are kept so this is a one-line re-enable when upstream CrispASR adds Metal/CUDA
            // support or exposes an --ode-steps flag.
            //new F5TtsCrispAsr(),

            // VoxCPM2 (CrispASR) — unlike f5-tts, the voxcpm2-tts backend has Metal/CUDA in
            // CrispASR v0.7.0, so synthesis is fast enough for the TTS-from-subtitles workflow.
            new VoxCPM2CrispAsr(),

            new ChatterboxTtsCpp(),
        ];

        // Insert CosyVoice3 (CrispASR) immediately after IndexTtsCrispAsr to keep the
        // CrispASR engines grouped visually in the engine combo.
        var indexTtsIndex = -1;
        for (var i = 0; i < Engines.Count; i++)
        {
            if (Engines[i] is IndexTtsCrispAsr)
            {
                indexTtsIndex = i;
                break;
            }
        }
        if (indexTtsIndex >= 0)
        {
            Engines.Insert(indexTtsIndex + 1, new CosyVoice3CrispAsr());
        }
        else
        {
            Engines.Add(new CosyVoice3CrispAsr());
        }

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
        else if (SelectedEngine is VibeVoiceCrispAsr)
        {
            Se.Settings.Video.TextToSpeech.VibeVoiceCrispAsrModel = SelectedModel ?? VibeVoiceCrispAsr.DefaultModelKey;
        }
        else if (SelectedEngine is IndexTtsCrispAsr)
        {
            Se.Settings.Video.TextToSpeech.IndexTtsCrispAsrModel = SelectedModel ?? IndexTtsCrispAsr.DefaultModelKey;
        }
        else if (SelectedEngine is CosyVoice3CrispAsr)
        {
            Se.Settings.Video.TextToSpeech.CosyVoice3CrispAsrModel = SelectedModel ?? CosyVoice3CrispAsr.DefaultModelKey;
        }
        else if (SelectedEngine is F5TtsCrispAsr)
        {
            Se.Settings.Video.TextToSpeech.F5TtsCrispAsrModel = SelectedModel ?? F5TtsCrispAsr.DefaultModelKey;
        }
        else if (SelectedEngine is VoxCPM2CrispAsr)
        {
            Se.Settings.Video.TextToSpeech.VoxCPM2CrispAsrModel = SelectedModel ?? VoxCPM2CrispAsr.DefaultModelKey;
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
            if (SelectedVoice?.EngineVoice is Voices.KokoroVoice kokoroVoice && !string.IsNullOrEmpty(kokoroVoice.Voice))
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
        var isDefaultVoice = SelectedVoice?.EngineVoice is Voices.OmniVoice omniVoice
                             && string.IsNullOrEmpty(omniVoice.FilePath);

        IsInstructionPickerEnabled = isOmniVoice && isDefaultVoice;
        IsInstructionVoiceHintVisible = isOmniVoice && !isDefaultVoice;
    }

    partial void OnSelectedVoiceChanged(Voice? value)
    {
        UpdateOmniVoicePickerState();
        // Fire-and-forget: when the user picks a CosyVoice3 or F5-TTS clone voice that has
        // no .txt sidecar, immediately prompt for the transcription. Writes the sidecar and
        // refreshes the voice list so the engine picks up the new RefText on the next synth.
        // Without this the user only sees the failure at Generate time with a confusing
        // "add a .txt sidecar" message, which is hard to act on inside the SE UI.
        _ = EnsureClonedVoiceRefTextAsync(value);
    }

    private bool _isPromptingForRefText;

    private async Task EnsureClonedVoiceRefTextAsync(Voice? voice)
    {
        if (_isPromptingForRefText || Window == null || voice == null)
        {
            return;
        }

        // CosyVoice3 requires ref-text (server fails outright). F5-TTS recommends it strongly
        // (cloning falls back to a generic voice without it). Both prompt the same way.
        string? wavPath = null;
        bool isCosyVoice3 = false;
        bool isVoxCPM2 = false;
        if (voice.EngineVoice is CosyVoice3Voice cosy && !string.IsNullOrEmpty(cosy.FilePath) && string.IsNullOrEmpty(cosy.RefText))
        {
            wavPath = cosy.FilePath;
            isCosyVoice3 = true;
        }
        else if (voice.EngineVoice is F5TtsVoice f5 && !string.IsNullOrEmpty(f5.FilePath))
        {
            var existing = TryReadRefTextSibling(f5.FilePath);
            if (string.IsNullOrEmpty(existing))
            {
                wavPath = f5.FilePath;
            }
        }
        else if (voice.EngineVoice is VoxCPM2Voice vox && !string.IsNullOrEmpty(vox.FilePath))
        {
            var existing = TryReadRefTextSibling(vox.FilePath);
            if (string.IsNullOrEmpty(existing))
            {
                wavPath = vox.FilePath;
                isVoxCPM2 = true;
            }
        }

        if (string.IsNullOrEmpty(wavPath))
        {
            return;
        }

        _isPromptingForRefText = true;
        try
        {
            var audioFileName = wavPath;
            var result = await _windowService.ShowDialogAsync<PromptTextBoxWindow, PromptTextBoxViewModel>(Window, vm =>
            {
                vm.Initialize(
                    Se.Language.Video.TextToSpeech.VoiceCloneTranscriptTitle,
                    string.Empty,
                    500,
                    150);
                vm.ConfigureExtraButton(
                    Se.Language.Video.TextToSpeech.UseSpeechToTextDotDotDot,
                    () => RunSpeechToTextForRefTextAsync(audioFileName));
            });

            if (!result.OkPressed || string.IsNullOrWhiteSpace(result.Text))
            {
                return;
            }

            bool written;
            if (isCosyVoice3)
            {
                written = CosyVoice3CrispAsr.TryWriteRefTextSidecar(wavPath, result.Text);
            }
            else if (isVoxCPM2)
            {
                written = VoxCPM2CrispAsr.TryWriteRefTextSidecar(wavPath, result.Text);
            }
            else
            {
                written = F5TtsCrispAsr.TryWriteRefTextSidecar(wavPath, result.Text);
            }

            if (!written || SelectedEngine == null)
            {
                return;
            }

            // Re-pull the voice list so the picked entry now carries RefText and the engine's
            // (model, voice, refText) cache key on the running server picks up the change.
            await RefreshVoices(SelectedEngine);
        }
        finally
        {
            _isPromptingForRefText = false;
        }
    }

    private static string? TryReadRefTextSibling(string wavPath)
    {
        try
        {
            var sidecar = Path.ChangeExtension(wavPath, ".txt");
            return File.Exists(sidecar) ? File.ReadAllText(sidecar).Trim() : null;
        }
        catch
        {
            return null;
        }
    }

    private async Task<string?> RunSpeechToTextForRefTextAsync(string audioFileName)
    {
        if (Window == null)
        {
            return null;
        }

        var sttResult = await _windowService.ShowDialogAsync<SpeechToTextWindow, SpeechToTextViewModel>(Window, vm =>
        {
            vm.Initialize(audioFileName, -1);
        });

        if (!sttResult.OkPressed || sttResult.TranscribedSubtitle == null || sttResult.TranscribedSubtitle.Paragraphs.Count == 0)
        {
            return null;
        }

        return string.Join(' ', sttResult.TranscribedSubtitle.Paragraphs
            .Select(p => p.Text?.Replace('\n', ' ').Replace('\r', ' ').Trim())
            .Where(t => !string.IsNullOrWhiteSpace(t)));
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
        if (IsRealOmniVoiceKeyword(SelectedOmniVoiceGender))
        {
            parts.Add(SelectedOmniVoiceGender);
        }
        if (IsRealOmniVoiceKeyword(SelectedOmniVoiceAge))
        {
            parts.Add(SelectedOmniVoiceAge);
        }
        if (IsRealOmniVoiceKeyword(SelectedOmniVoicePitch))
        {
            parts.Add(SelectedOmniVoicePitch);
        }
        if (IsRealOmniVoiceKeyword(SelectedOmniVoiceAccent))
        {
            parts.Add(SelectedOmniVoiceAccent);
        }
        if (OmniVoiceWhisper)
        {
            parts.Add(OmniVoiceTtsCpp.InstructionWhisper);
        }
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
        => Initialize(subtitle, null, videoFileName, wavePeakData, waveFolder);

    public void Initialize(Subtitle subtitle, SubtitleFormat? format, string videoFileName, WavePeakData2? wavePeakData, string waveFolder)
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

        _castKind = ActorVoiceDetector.Detect(subtitle, format);
        // Only surface the cast button when there's actually more than one actor/voice to assign
        // — a single-speaker subtitle uses the global engine/voice and the button would be a no-op.
        var actorCount = _castKind == ActorVoiceDetector.CastKind.None
            ? 0
            : ActorVoiceDetector.GetNames(subtitle, _castKind).Count;
        HasCast = actorCount > 1;
        CastButtonText = HasCast
            ? string.Format("{0} ({1})", Se.Language.Video.TextToSpeech.SetupCast.TrimEnd('.'), actorCount)
            : Se.Language.Video.TextToSpeech.SetupCast;

        // Seed from last session's persisted cast so users don't have to re-assign every time
        // they open the same set of actors. The Cast dialog merges fresh edits back on save.
        _actorVoiceMappings.Clear();
        if (HasCast)
        {
            var actorNames = ActorVoiceDetector.GetNames(subtitle, _castKind)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            foreach (var saved in Se.Settings.Video.TextToSpeech.LastActorVoiceMappings ?? new List<ActorVoiceMapping>())
            {
                if (saved == null || string.IsNullOrWhiteSpace(saved.Actor))
                {
                    continue;
                }

                // Trim before probing the set — the set was built from ActorVoiceDetector.GetNames
                // which trims, but a legacy saved entry might have stored whitespace around the
                // actor name and the case-insensitive Contains() still treats trailing spaces
                // as a mismatch.
                var actorKey = saved.Actor.Trim();
                if (actorNames.Contains(actorKey))
                {
                    _actorVoiceMappings.Add(new ActorVoiceMapping
                    {
                        Actor = actorKey,
                        EngineName = saved.EngineName ?? string.Empty,
                        VoiceName = saved.VoiceName ?? string.Empty,
                        Model = saved.Model ?? string.Empty,
                        Instruction = saved.Instruction ?? string.Empty,
                    });
                }
            }
        }
    }

    [RelayCommand]
    private async Task ShowCast()
    {
        if (Window == null || !HasCast)
        {
            return;
        }

        var actorNames = ActorVoiceDetector.GetNames(_subtitle, _castKind);
        if (actorNames.Count == 0 && Window != null)
        {
            await MessageBox.Show(
                Window,
                Se.Language.Video.TextToSpeech.ActorVoicesTitle,
                _castKind == ActorVoiceDetector.CastKind.AssaActors
                    ? Se.Language.Video.TextToSpeech.NoActorsFoundMessage
                    : Se.Language.Video.TextToSpeech.NoWebVttVoicesFoundMessage,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        // Same filter the Review window uses — only show engines that are usable right now
        // (have credentials / are installed). Otherwise the user can pick an engine the cast
        // can't actually drive, BuildCastContextAsync silently swallows the GetVoices failure,
        // and the row quietly falls back to the global voice.
        var usableEngines = ActorVoiceDetector.FilterUsableEngines(Engines).ToList();
        var result = await _windowService.ShowDialogAsync<ActorVoiceMappingWindow, ActorVoiceMappingViewModel>(Window!, vm =>
        {
            vm.Initialize(actorNames, usableEngines, _actorVoiceMappings, SelectedEngine, SelectedVoice, _castKind, _waveFolder);
        });

        if (result.OkPressed)
        {
            _actorVoiceMappings.Clear();
            _actorVoiceMappings.AddRange(result.Mappings);
        }
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

    private async Task PromptMergeContinuationLines()
    {
        if (Window == null || _subtitle.Paragraphs.Count < 2)
        {
            return;
        }

        var language = LanguageAutoDetect.AutoDetectGoogleLanguage(_subtitle);
        if (MergeContinuationLinesHelper.IsLanguageSkipped(language))
        {
            return;
        }

        var format = _subtitle.OriginalFormat ?? new SubRip();
        var viewModels = _subtitle.Paragraphs
            .Select(p => new SubtitleLineViewModel(p, format))
            .ToList();

        const int maxGapMs = 500;
        const int maxCharacters = 500;
        var candidates = MergeContinuationLinesHelper.Detect(viewModels, language, maxGapMs, maxCharacters);
        if (candidates.Count == 0)
        {
            return;
        }

        var explain = await MessageBox.Show(
            Window!,
            Se.Language.Video.TextToSpeech.MergeContinuationLinesPromptTitle,
            Se.Language.Video.TextToSpeech.MergeContinuationLinesPromptMessage,
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);
        if (explain != MessageBoxResult.Yes)
        {
            return;
        }

        var result = await _windowService
            .ShowDialogAsync<MergeContinuationLinesWindow, MergeContinuationLinesViewModel>(
                Window!, vm => vm.Initialize(viewModels, language, maxGapMs, maxCharacters));

        if (!result.OkPressed || result.AllSubtitlesFixed.Count == _subtitle.Paragraphs.Count)
        {
            return;
        }

        // Clone via copy constructor to preserve Header/Footer/OriginalEncoding and any other
        // metadata, then replace Paragraphs with the merged set. The caller's Subtitle is untouched.
        var merged = new Subtitle(_subtitle);
        merged.Paragraphs.Clear();
        foreach (var line in result.AllSubtitlesFixed)
        {
            merged.Paragraphs.Add(line.ToParagraph(_subtitle.OriginalFormat));
        }
        _subtitle = merged;
    }

    /// <summary>
    /// Stop the crispasr.exe servers of every CrispASR-based TTS engine EXCEPT
    /// <paramref name="keepAlive"/>. Pass null to stop all four. Called before starting synth
    /// (Test Voice / Generate TTS) and on window close, so models from previously selected
    /// engines don't pile up in VRAM and OOM the next load — typical user GPUs (8 GB) can fit
    /// at most one of these engines at a time once you account for the codec/vocoder and KV
    /// cache. No-op for engines that aren't currently running. Cheap when nothing's running.
    /// </summary>
    private static void StopOtherCrispAsrServers(ITtsEngine? keepAlive)
    {
        if (keepAlive is not Qwen3TtsCrispAsr)
        {
            Qwen3TtsCrispAsr.StopServer();
        }
        if (keepAlive is not VibeVoiceCrispAsr)
        {
            VibeVoiceCrispAsr.StopServer();
        }
        if (keepAlive is not IndexTtsCrispAsr)
        {
            IndexTtsCrispAsr.StopServer();
        }
        if (keepAlive is not ChatterboxTtsCpp)
        {
            ChatterboxTtsCpp.StopServer();
        }
        if (keepAlive is not VoxCPM2CrispAsr)
        {
            VoxCPM2CrispAsr.StopServer();
        }
    }

    private static void StopAllCrispAsrServers() => StopOtherCrispAsrServers(null);

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

        if (Se.Settings.Tools.TextToSpeechPromptMergeContinuationLines)
        {
            await PromptMergeContinuationLines();
        }

        var voice = SelectedVoice;
        if (voice != null && !await TtsVoiceInstaller.EnsureVoiceInstalled(engine, voice, Window, _windowService))
        {
            return;
        }

        // Free GPU memory held by any other CrispASR-based engine before we load this one's
        // model. Without this, switching between Qwen3 / VibeVoice / IndexTTS / Chatterbox
        // within a session stacks crispasr.exe processes (each holding 1-3 GB of GGUFs)
        // until the next load OOMs on a typical 8 GB GPU. Each StopServer does Kill +
        // WaitForExit(2000), so do it off the UI thread — calling synchronously here can
        // freeze the app for up to 4 × 2 s when multiple engines have been used in the
        // session.
        await Task.Run(() => StopOtherCrispAsrServers(engine));

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
        else if (SelectedEngine is VibeVoiceCrispAsr)
        {
            await _windowService.ShowDialogAsync<VibeVoiceCrispAsrSettingsWindow, VibeVoiceCrispAsrSettingsViewModel>(Window!, vm => vm.Initialize());
        }
        else if (SelectedEngine is IndexTtsCrispAsr)
        {
            await _windowService.ShowDialogAsync<IndexTtsCrispAsrSettingsWindow, IndexTtsCrispAsrSettingsViewModel>(Window!, vm => vm.Initialize());
        }
        else if (SelectedEngine is CosyVoice3CrispAsr)
        {
            await _windowService.ShowDialogAsync<CosyVoice3CrispAsrSettingsWindow, CosyVoice3CrispAsrSettingsViewModel>(Window!, vm => vm.Initialize());
        }
        else if (SelectedEngine is F5TtsCrispAsr)
        {
            await _windowService.ShowDialogAsync<F5TtsCrispAsrSettingsWindow, F5TtsCrispAsrSettingsViewModel>(Window!, vm => vm.Initialize());
        }
        else if (SelectedEngine is VoxCPM2CrispAsr)
        {
            await _windowService.ShowDialogAsync<VoxCPM2CrispAsrSettingsWindow, VoxCPM2CrispAsrSettingsViewModel>(Window!, vm => vm.Initialize());
        }
        else if (SelectedEngine is KokoroTtsCpp)
        {
            await _windowService.ShowDialogAsync<KokoroTtsSettingsWindow, KokoroTtsSettingsViewModel>(Window!, vm => vm.Initialize());
        }
        else if (SelectedEngine is ChatterboxTtsCpp)
        {
            await _windowService.ShowDialogAsync<ChatterboxTtsSettingsWindow, ChatterboxTtsSettingsViewModel>(Window!, vm => vm.Initialize());
        }
        else if (SelectedEngine is Piper)
        {
            await _windowService.ShowDialogAsync<PiperSettingsWindow, PiperSettingsViewModel>(Window!, vm => vm.Initialize());
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

        // Free GPU memory held by any other CrispASR engine — see GenerateTts for rationale.
        // Off the UI thread so a Kill + WaitForExit on a sluggish process doesn't freeze
        // the test-voice button for a few seconds.
        await Task.Run(() => StopOtherCrispAsrServers(engine));

        var text = Se.Settings.Video.TextToSpeech.VoiceTestText;
        if (string.IsNullOrEmpty(text))
        {
            text = "This is a test";
        }
        text = Utilities.UnbreakLine(text);

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

        // Restore the cast that travelled with the export so re-generating uses the same voices.
        if (importExport.ActorVoiceMappings != null && importExport.ActorVoiceMappings.Count > 0)
        {
            _actorVoiceMappings.Clear();
            _actorVoiceMappings.AddRange(importExport.ActorVoiceMappings);
        }

        // Resolve each line's Voice against the engine that *generated* it (per-line
        // EngineName), not the currently selected engine — otherwise cast lines from another
        // engine would land with Voice = null and click-to-sync/regenerate would break. Load
        // each distinct engine's voice list once and reuse it for every line referencing that
        // engine.
        var voicesByEngine = new Dictionary<string, Voice[]>(StringComparer.OrdinalIgnoreCase);
        var engineNames = importExport.Items
            .Select(i => i.EngineName ?? string.Empty)
            .Where(n => !string.IsNullOrEmpty(n))
            .Distinct(StringComparer.OrdinalIgnoreCase);
        foreach (var engineName in engineNames)
        {
            var matchingEngine = Engines.FirstOrDefault(e => string.Equals(e.Name, engineName, StringComparison.OrdinalIgnoreCase));
            if (matchingEngine == null)
            {
                continue;
            }
            try
            {
                voicesByEngine[engineName] = await matchingEngine.GetVoices(string.Empty);
            }
            catch (Exception ex)
            {
                SeLogger.Error(ex, $"Import: loading voices for engine '{engineName}' failed — lines using it will fall back to the global voice list.");
            }
        }

        var stepResults = new List<TtsStepResult>();
        for (var index = 0; index < importExport.Items.Count; index++)
        {
            var item = importExport.Items[index];
            var paragraph = new Paragraph(item.Text, item.StartMs, item.EndMs) { Number = index + 1 };
            Voice? voice = null;
            if (!string.IsNullOrEmpty(item.EngineName)
                && voicesByEngine.TryGetValue(item.EngineName, out var perEngineVoices))
            {
                voice = perEngineVoices.FirstOrDefault(v => string.Equals(v.Name, item.VoiceName, StringComparison.OrdinalIgnoreCase));
            }
            voice ??= Voices.FirstOrDefault(v => v.Name == item.VoiceName);
            stepResults.Add(new TtsStepResult
            {
                Text = item.Text,
                CurrentFileName = item.AudioFileName,
                Paragraph = paragraph,
                SpeedFactor = item.SpeedFactor <= 0 ? 1.0f : item.SpeedFactor,
                Voice = voice,
                // Restore the per-line engine snapshot so the review window's click-to-sync
                // (and any future regeneration) can recover the exact engine/model/instruction
                // each line was produced with.
                EngineName = item.EngineName ?? string.Empty,
                Model = item.Model ?? string.Empty,
                Instruction = item.Instruction ?? string.Empty,
            });
        }

        // Prefer the video referenced in the imported JSON — the user is reopening that specific
        // TTS session and the main TTS window may have been opened without a video at all.
        var videoFileNameForReview = !string.IsNullOrEmpty(importExport.VideoFileName) && File.Exists(importExport.VideoFileName)
            ? importExport.VideoFileName
            : _videoFileName;
        if (string.IsNullOrEmpty(_videoFileName) && !string.IsNullOrEmpty(videoFileNameForReview))
        {
            _videoFileName = videoFileNameForReview;
        }

        // Try to populate _wavePeakData synchronously from the on-disk cache (fast, no ffmpeg).
        // If the cache miss, GenerateWavePeaksIfNeededAsync below kicks off a background ffmpeg
        // job and pushes the result into the review VM once ready.
        var peaksForReview = TryLoadWavePeaksFromDisk(videoFileNameForReview) ?? _wavePeakData;

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
                videoFileNameForReview,
                Path.GetDirectoryName(fileName)!,
                peaksForReview);
            // Forward the imported cast so a subsequent Export round-trips the mappings instead
            // of writing ActorVoiceMappings = [] back to SubtitleEditTts.json.
            vm.ActorVoiceMappings.AddRange(_actorVoiceMappings);

            if (peaksForReview == null || peaksForReview.Peaks.Count == 0)
            {
                _ = GenerateWavePeaksIfNeededAsync(videoFileNameForReview, vm);
            }
        });
    }

    private static WavePeakData2? TryLoadWavePeaksFromDisk(string videoFileName)
    {
        if (string.IsNullOrEmpty(videoFileName) || !File.Exists(videoFileName))
        {
            return null;
        }

        try
        {
            var peakWaveFileName = WavePeakGenerator2.GetPeakWaveFileName(videoFileName);
            if (File.Exists(peakWaveFileName))
            {
                return WavePeakData2.FromDisk(peakWaveFileName);
            }
        }
        catch (Exception ex)
        {
            SeLogger.Error(ex, $"Failed to load cached wave peaks for '{videoFileName}'");
        }

        return null;
    }

    // Background peak generation for imported TTS sessions. The review window opens immediately
    // with a blank waveform; when ffmpeg finishes, the resulting peaks are pushed into the VM
    // and the AudioVisualizer's WavePeaks binding picks them up.
    private static async Task GenerateWavePeaksIfNeededAsync(string videoFileName, ReviewSpeechViewModel reviewVm)
    {
        if (string.IsNullOrEmpty(videoFileName) || !File.Exists(videoFileName))
        {
            return;
        }

        if (!FfmpegHelper.IsFfmpegInstalled())
        {
            return;
        }

        var tempWaveFileName = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.wav");
        try
        {
            var peakWaveFileName = WavePeakGenerator2.GetPeakWaveFileName(videoFileName);

            using (var process = WaveFileExtractor.GetCommandLineProcess(
                videoFileName,
                -1,
                tempWaveFileName,
                Configuration.Settings.General.VlcWaveTranscodeSettings,
                out _))
            {
                process.Start();
                await process.WaitForExitAsync();
                if (process.ExitCode != 0)
                {
                    SeLogger.Error($"ffmpeg exited with code {process.ExitCode} extracting wave for TTS review: '{videoFileName}'");
                    return;
                }
            }

            if (!File.Exists(tempWaveFileName))
            {
                return;
            }

            WavePeakData2? peaks;
            using (var waveFile = new WavePeakGenerator2(tempWaveFileName))
            {
                peaks = waveFile.GeneratePeaks(0, peakWaveFileName);
            }

            if (peaks != null)
            {
                await Dispatcher.UIThread.InvokeAsync(() => reviewVm.WavePeakData = peaks);
            }
        }
        catch (Exception ex)
        {
            SeLogger.Error(ex, $"Background wave-peak generation failed for '{videoFileName}'");
        }
        finally
        {
            try { File.Delete(tempWaveFileName); } catch { /* best effort */ }
        }
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
                if (!dlResult.OkPressed || !Qwen3TtsCrispAsr.AreModelsInstalled(crispAsrModelKey))
                {
                    return false;
                }

                // The download dialog also pulls voices.zip when none are present, so
                // refresh the voice list to surface them in the combo.
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await RefreshVoices(engine);
                });
                return true;
            }

            return true;
        }

        if (engine is VibeVoiceCrispAsr)
        {
            if (!await TtsVoiceInstaller.EnsureCrispAsrForVibeVoice(Window, _windowService, forceRedownload: false))
            {
                return false;
            }

            var vibeModelKey = VibeVoiceCrispAsr.ResolveModelKey(SelectedModel);
            if (!VibeVoiceCrispAsr.AreModelsInstalled(vibeModelKey))
            {
                // Model key already includes the size in its label (e.g. "Q8_0 (~2.8 GB)") so
                // we don't append a separate size — avoids duplication in the prompt.
                var answer = await MessageBox.Show(
                    Window,
                    "Download VibeVoice (CrispASR) model?",
                    $"{Environment.NewLine}\"VibeVoice (CrispASR)\" ({vibeModelKey}) requires a model.{Environment.NewLine}{Environment.NewLine}Download model?",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (answer != MessageBoxResult.Yes)
                {
                    return false;
                }

                var dlResult = await _windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(Window!, vm => vm.StartDownloadVibeVoiceCrispAsrModels(vibeModelKey));
                if (!dlResult.OkPressed || !VibeVoiceCrispAsr.AreModelsInstalled(vibeModelKey))
                {
                    return false;
                }

                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await RefreshVoices(engine);
                });
                return true;
            }

            return true;
        }

        if (engine is IndexTtsCrispAsr)
        {
            if (!await TtsVoiceInstaller.EnsureCrispAsrForIndexTts(Window, _windowService, forceRedownload: false))
            {
                return false;
            }

            var indexModelKey = IndexTtsCrispAsr.ResolveModelKey(SelectedModel);
            if (!IndexTtsCrispAsr.AreModelsInstalled(indexModelKey))
            {
                // Model key already includes the size in its label (e.g. "Q8_0 (~870 MB)")
                // so we don't append a separate size — avoids duplication in the prompt.
                var answer = await MessageBox.Show(
                    Window,
                    "Download IndexTTS (CrispASR) models?",
                    $"{Environment.NewLine}\"IndexTTS (CrispASR)\" ({indexModelKey}) requires models.{Environment.NewLine}{Environment.NewLine}Download models?",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (answer != MessageBoxResult.Yes)
                {
                    return false;
                }

                var dlResult = await _windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(Window!, vm => vm.StartDownloadIndexTtsCrispAsrModels(indexModelKey));
                if (!dlResult.OkPressed || !IndexTtsCrispAsr.AreModelsInstalled(indexModelKey))
                {
                    return false;
                }

                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await RefreshVoices(engine);
                });
                return true;
            }

            return true;
        }

        if (engine is CosyVoice3CrispAsr)
        {
            if (!await TtsVoiceInstaller.EnsureCrispAsrForCosyVoice3(Window, _windowService, forceRedownload: false))
            {
                return false;
            }

            var cosyModelKey = CosyVoice3CrispAsr.ResolveModelKey(SelectedModel);
            if (!CosyVoice3CrispAsr.AreModelsInstalled(cosyModelKey))
            {
                var answer = await MessageBox.Show(
                    Window,
                    "Download CosyVoice3 (CrispASR) models?",
                    $"{Environment.NewLine}\"CosyVoice3 (CrispASR)\" ({cosyModelKey}) requires LLM + flow + hift + s3tok + campplus + voice-bank GGUFs (all sized into the total above).{Environment.NewLine}{Environment.NewLine}Download models?",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (answer != MessageBoxResult.Yes)
                {
                    return false;
                }

                var dlResult = await _windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(Window!, vm => vm.StartDownloadCosyVoice3CrispAsrModels(cosyModelKey));
                if (!dlResult.OkPressed || !CosyVoice3CrispAsr.AreModelsInstalled(cosyModelKey))
                {
                    return false;
                }

                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await RefreshVoices(engine);
                });
                return true;
            }

            return true;
        }

        if (engine is F5TtsCrispAsr)
        {
            if (!await TtsVoiceInstaller.EnsureCrispAsrForF5Tts(Window, _windowService, forceRedownload: false))
            {
                return false;
            }

            var f5ModelKey = F5TtsCrispAsr.ResolveModelKey(SelectedModel);
            if (!F5TtsCrispAsr.AreModelsInstalled(f5ModelKey))
            {
                var answer = await MessageBox.Show(
                    Window,
                    "Download F5-TTS (CrispASR) model?",
                    $"{Environment.NewLine}\"F5-TTS (CrispASR)\" ({f5ModelKey}) requires a model.{Environment.NewLine}{Environment.NewLine}Download model?",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (answer != MessageBoxResult.Yes)
                {
                    return false;
                }

                var dlResult = await _windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(Window!, vm => vm.StartDownloadF5TtsCrispAsrModels(f5ModelKey));
                if (!dlResult.OkPressed || !F5TtsCrispAsr.AreModelsInstalled(f5ModelKey))
                {
                    return false;
                }

                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await RefreshVoices(engine);
                });
                return true;
            }

            return true;
        }

        if (engine is VoxCPM2CrispAsr)
        {
            if (!await TtsVoiceInstaller.EnsureCrispAsrForVoxCPM2(Window, _windowService, forceRedownload: false))
            {
                return false;
            }

            var voxModelKey = VoxCPM2CrispAsr.ResolveModelKey(SelectedModel);
            if (!VoxCPM2CrispAsr.AreModelsInstalled(voxModelKey))
            {
                var answer = await MessageBox.Show(
                    Window,
                    "Download VoxCPM2 (CrispASR) model?",
                    $"{Environment.NewLine}\"VoxCPM2 (CrispASR)\" ({voxModelKey}) requires a model.{Environment.NewLine}{Environment.NewLine}Download model?",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (answer != MessageBoxResult.Yes)
                {
                    return false;
                }

                var dlResult = await _windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(Window!, vm => vm.StartDownloadVoxCPM2CrispAsrModels(voxModelKey));
                if (!dlResult.OkPressed || !VoxCPM2CrispAsr.AreModelsInstalled(voxModelKey))
                {
                    return false;
                }

                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await RefreshVoices(engine);
                });
                return true;
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

            // Resolved per-actor cast (engine + voice + instruction). When _actorVoiceMappings is
            // empty, every paragraph falls back to the globally selected engine/voice.
            var castContext = await BuildCastContextAsync();

            for (var index = 0; index < _subtitle.Paragraphs.Count; index++)
            {
                ProgressText = $"Generating speech: segment {index + 1} of {_subtitle.Paragraphs.Count}";
                var paragraph = _subtitle.Paragraphs[index];
                var resolution = ResolveVoiceForParagraph(paragraph, castContext, engine, voice);
                // When the row's engine differs from the globally selected engine, the global
                // SelectedLanguage/SelectedRegion/SelectedModel were resolved for a different
                // engine and don't apply (e.g. ElevenLabs voice IDs would be passed to Edge
                // TTS). Pass null in that case and let the row's engine fall back to its own
                // defaults.
                var isCrossEngine = !ReferenceEquals(resolution.Engine, engine);
                var language = isCrossEngine ? null : SelectedLanguage;
                var region = isCrossEngine ? null : SelectedRegion;
                var model = resolution.Model ?? (isCrossEngine ? null : SelectedModel);
                // Per-actor cast workflow: rows can target different engines (e.g. Alice uses
                // Qwen3, Bob uses VibeVoice). Free GPU memory held by any *other* CrispASR
                // engine before this Speak so they don't stack — see StopOtherCrispAsrServers
                // for VRAM math. No-op when the engine doesn't change between rows or when
                // the row's engine isn't CrispASR-backed. Off the UI thread because Kill +
                // WaitForExit can take a few seconds per stuck process.
                await Task.Run(() => StopOtherCrispAsrServers(resolution.Engine));
                var speakResult = await TtsInstructionSwap.RunAsync(
                    resolution.Engine,
                    resolution.Instruction,
                    () => resolution.Engine.Speak(resolution.Text, _waveFolder, resolution.Voice,
                        language, region, model, cancellationToken));
                resultList.Add(new TtsStepResult
                {
                    Text = resolution.Text,
                    CurrentFileName = speakResult.FileName,
                    Paragraph = paragraph,
                    SpeedFactor = 1.0f,
                    Voice = resolution.Voice,
                    EngineName = resolution.Engine.Name,
                    // Record the model the engine actually saw: empty for cross-engine lines
                    // with no per-row override (which used the engine's own default), the
                    // override when set, or the global SelectedModel for same-engine lines.
                    // Avoids snapshotting a global model that belongs to a different engine.
                    Model = model ?? string.Empty,
                    Instruction = resolution.Instruction,
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

    private sealed class CastContext
    {
        public Dictionary<string, ActorVoiceMapping> ByActor { get; init; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, ITtsEngine> EnginesByName { get; init; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, Voice[]> VoicesByEngine { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    }

    // Loads voices for every engine referenced by a mapping once, so the per-paragraph lookup
    // in the generate loop is a cheap dictionary hit. Engines whose GetVoices() throws (e.g.
    // missing API key) are skipped silently — those mappings fall through to the global voice.
    private async Task<CastContext> BuildCastContextAsync()
    {
        var ctx = new CastContext();
        if (_actorVoiceMappings.Count == 0 || _castKind == ActorVoiceDetector.CastKind.None)
        {
            return ctx;
        }

        foreach (var m in _actorVoiceMappings)
        {
            if (!string.IsNullOrWhiteSpace(m.Actor))
            {
                ctx.ByActor[m.Actor.Trim()] = m;
            }
        }

        var engineNames = _actorVoiceMappings
            .Select(m => m.EngineName)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Distinct(StringComparer.OrdinalIgnoreCase);

        foreach (var name in engineNames)
        {
            var engine = Engines.FirstOrDefault(e => string.Equals(e.Name, name, StringComparison.OrdinalIgnoreCase));
            if (engine == null)
            {
                continue;
            }

            ctx.EnginesByName[name] = engine;
            try
            {
                ctx.VoicesByEngine[name] = await engine.GetVoices(string.Empty);
            }
            catch (Exception ex)
            {
                SeLogger.Error(ex, $"Cast: loading voices for engine '{name}' failed - mappings using it will fall back to the global voice.");
            }
        }

        return ctx;
    }

    private readonly struct ResolvedVoice
    {
        public ResolvedVoice(ITtsEngine engine, Voice voice, string? model, string text, string instruction)
        {
            Engine = engine;
            Voice = voice;
            Model = model;
            Text = text;
            Instruction = instruction;
        }

        public ITtsEngine Engine { get; }
        public Voice Voice { get; }
        public string? Model { get; }
        public string Text { get; }
        public string Instruction { get; }
    }

    // Picks engine + voice + text for a paragraph. For WebVTT it also strips the <v ...> wrapper
    // out of the text so the engine doesn't speak the tag. Falls back to the global engine/voice
    // when the paragraph has no actor, the mapping points at an unknown engine, or that engine's
    // voice list couldn't be loaded.
    private ResolvedVoice ResolveVoiceForParagraph(Paragraph paragraph, CastContext ctx, ITtsEngine defaultEngine, Voice defaultVoice)
    {
        var actor = ActorVoiceDetector.GetParagraphActor(paragraph, _castKind);
        var rawText = _castKind == ActorVoiceDetector.CastKind.WebVttVoices
            ? ActorVoiceDetector.StripWebVttVoiceTags(paragraph.Text)
            : paragraph.Text;
        // Unbreak line endings centrally so every engine sees a single flowing line —
        // many TTS engines otherwise insert weird pauses on \r\n.
        var text = Utilities.UnbreakLine(rawText);

        if (string.IsNullOrEmpty(actor) || !ctx.ByActor.TryGetValue(actor, out var mapping))
        {
            return new ResolvedVoice(defaultEngine, defaultVoice, null, text, string.Empty);
        }

        if (!ctx.EnginesByName.TryGetValue(mapping.EngineName, out var mappedEngine)
            || !ctx.VoicesByEngine.TryGetValue(mapping.EngineName, out var voices))
        {
            return new ResolvedVoice(defaultEngine, defaultVoice, null, text, string.Empty);
        }

        var mappedVoice = voices.FirstOrDefault(v => string.Equals(v.Name, mapping.VoiceName, StringComparison.OrdinalIgnoreCase));
        if (mappedVoice == null)
        {
            return new ResolvedVoice(defaultEngine, defaultVoice, null, text, string.Empty);
        }

        // Per-row model overrides the global one; empty string means "use the global model".
        var modelOverride = string.IsNullOrWhiteSpace(mapping.Model) ? null : mapping.Model;
        return new ResolvedVoice(mappedEngine, mappedVoice, modelOverride, text, mapping.Instruction ?? string.Empty);
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
                        EngineName = item.EngineName,
                        Model = item.Model,
                        Instruction = item.Instruction,
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
                        EngineName = item.EngineName,
                        Model = item.Model,
                        Instruction = item.Instruction,
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
                    EngineName = item.EngineName,
                    Model = item.Model,
                    Instruction = item.Instruction,
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
                    EngineName = item.EngineName,
                    Model = item.Model,
                    Instruction = item.Instruction,
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
            vm.ActorVoiceMappings.AddRange(_actorVoiceMappings);
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
            else if (SelectedEngine is VibeVoiceCrispAsr)
            {
                SelectedModel = Models.FirstOrDefault(p => p == Se.Settings.Video.TextToSpeech.VibeVoiceCrispAsrModel);
                if (string.IsNullOrEmpty(SelectedModel))
                {
                    SelectedModel = Models.FirstOrDefault();
                }
                IsEngineSettingsVisible = true;
            }
            else if (SelectedEngine is IndexTtsCrispAsr)
            {
                SelectedModel = Models.FirstOrDefault(p => p == Se.Settings.Video.TextToSpeech.IndexTtsCrispAsrModel);
                if (string.IsNullOrEmpty(SelectedModel))
                {
                    SelectedModel = Models.FirstOrDefault();
                }
                IsEngineSettingsVisible = true;
            }
            else if (SelectedEngine is CosyVoice3CrispAsr)
            {
                SelectedModel = Models.FirstOrDefault(p => p == Se.Settings.Video.TextToSpeech.CosyVoice3CrispAsrModel);
                if (string.IsNullOrEmpty(SelectedModel))
                {
                    SelectedModel = Models.FirstOrDefault();
                }
                IsEngineSettingsVisible = true;
            }
            else if (SelectedEngine is VoxCPM2CrispAsr)
            {
                SelectedModel = Models.FirstOrDefault(p => p == Se.Settings.Video.TextToSpeech.VoxCPM2CrispAsrModel);
                if (string.IsNullOrEmpty(SelectedModel))
                {
                    SelectedModel = Models.FirstOrDefault();
                }
                IsEngineSettingsVisible = true;
            }
            else if (SelectedEngine is F5TtsCrispAsr)
            {
                SelectedModel = Models.FirstOrDefault(p => p == Se.Settings.Video.TextToSpeech.F5TtsCrispAsrModel);
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
            else if (SelectedEngine is Piper)
            {
                IsEngineSettingsVisible = true;
            }
            else if (SelectedEngine is KokoroTtsCpp)
            {
                var savedVoice = Se.Settings.Video.TextToSpeech.KokoroVoice;
                if (!string.IsNullOrEmpty(savedVoice))
                {
                    var match = Voices.FirstOrDefault(v =>
                        v.EngineVoice is Voices.KokoroVoice kv && kv.Voice == savedVoice);
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
        // Release VRAM held by any still-running crispasr.exe servers so models don't stay
        // pinned for the rest of the SE session. Fire-and-forget on the threadpool — each
        // StopServer is Kill + WaitForExit(2000), so doing this on the UI thread could block
        // the close by up to 4 × 2 s if all four engines were used. AppDomain.ProcessExit
        // performs the same teardown if SE exits before the task completes, so nothing is
        // left running.
        _ = Task.Run(StopAllCrispAsrServers);
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