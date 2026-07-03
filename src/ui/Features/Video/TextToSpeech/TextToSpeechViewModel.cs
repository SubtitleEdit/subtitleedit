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
    [ObservableProperty] private bool _isVoiceCountVisible;
    [ObservableProperty] private string _linesInfo = string.Empty;
    [ObservableProperty] private bool _hasVideoFile;
    [ObservableProperty] private string _videoInfo = string.Empty;
    [ObservableProperty] private string _engineDescription = string.Empty;
    [ObservableProperty] private bool _hasEngineDescription;
    [ObservableProperty] private string _progressPercentText = string.Empty;
    [ObservableProperty] private string _progressEtaText = string.Empty;
    [ObservableProperty] private bool _isVoiceTestEnabled;
    [ObservableProperty] private bool _isVoiceComboEnabled;
    [ObservableProperty] private bool _doReviewAudioClips;
    [ObservableProperty] private bool _doGenerateVideoFile;
    [ObservableProperty] private bool _isEdgeTtsEngine;
    [ObservableProperty] private bool _isGenerating;
    [ObservableProperty] private bool _isNotGenerating;
    [ObservableProperty] private bool _isEngineSettingsVisible;
    [ObservableProperty] private bool _isModelDownloadVisible;
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

            new ZonosTtsCrispAsr(),

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
            // Read back by name on model change (this file + ReviewSpeechViewModel) but was never
            // written anywhere, so the language choice silently reset to English every time.
            Se.Settings.Video.TextToSpeech.ElevenLabsLanguage = SelectedLanguage?.Name ?? string.Empty;
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
        // "Default", CustomVoice exposes the nine fixed built-in speakers, and Voice clone
        // lists the imported reference WAVs. Persist the model change first so GetVoices reads
        // the new value, then
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
        bool isQwen3Clone = false;
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
        else if (voice.EngineVoice is Voices.Qwen3TtsVoice qwen3 && !string.IsNullOrEmpty(qwen3.FilePath))
        {
            // Only the Voice clone (Base) model carries a FilePath; CustomVoice/VoiceDesign leave
            // it empty, so this never fires for them. A usable transcript may already exist —
            // imported by the user, or filled from the OmniVoice pack by NormalizeVoiceTranscripts.
            var existing = Qwen3TtsCrispAsr.TryReadUsableTranscript(qwen3.FilePath);
            if (string.IsNullOrEmpty(existing))
            {
                wavPath = qwen3.FilePath;
                isQwen3Clone = true;
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
            // Qwen3 (CrispASR) clone: auto-run Whisper first so the user rarely has to type the
            // transcript (the sibling engines keep their type-or-click-STT prompt). The result is
            // still shown for a quick review/correction since clone quality is sensitive to it.
            var initialText = string.Empty;
            if (isQwen3Clone)
            {
                initialText = await RunSpeechToTextForRefTextAsync(audioFileName) ?? string.Empty;
            }

            var result = await _windowService.ShowDialogAsync<PromptTextBoxWindow, PromptTextBoxViewModel>(Window, vm =>
            {
                vm.Initialize(
                    Se.Language.Video.TextToSpeech.VoiceCloneTranscriptTitle,
                    initialText,
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
            else if (isQwen3Clone)
            {
                written = Qwen3TtsCrispAsr.TryWriteRefTextSidecar(wavPath, result.Text);
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
            if (!File.Exists(sidecar))
            {
                return null;
            }

            // An attribution-blurb sidecar (pre-filter seeding) is not a transcript - report it
            // as missing so the transcript prompt still fires instead of being suppressed.
            var text = File.ReadAllText(sidecar).Trim();
            return Qwen3TtsCrispAsr.LooksLikeAttributionBlurb(text) ? null : text;
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

        // Context line under the title: what is about to be spoken, and whether a video is
        // loaded (the add-to-video option depends on it).
        var subtitleName = Path.GetFileName(subtitle.FileName ?? string.Empty);
        LinesInfo = string.IsNullOrEmpty(subtitleName)
            ? string.Format(Se.Language.Video.TextToSpeech.XLines, subtitle.Paragraphs.Count)
            : string.Format(Se.Language.Video.TextToSpeech.XLinesFromY, subtitle.Paragraphs.Count, subtitleName);
        HasVideoFile = !string.IsNullOrEmpty(videoFileName);
        VideoInfo = HasVideoFile
            ? string.Format(Se.Language.Video.TextToSpeech.VideoX, CapFileName(Path.GetFileName(videoFileName), 40))
            : string.Empty;

        _cancellationTokenSource = new CancellationTokenSource();
        _cancellationToken = _cancellationTokenSource.Token;
        IsGenerating = false;
        IsNotGenerating = true;
        IsEngineSettingsVisible = false;
        IsModelDownloadVisible = false;
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
        if (keepAlive is not CosyVoice3CrispAsr)
        {
            CosyVoice3CrispAsr.StopServer();
        }
        if (keepAlive is not ChatterboxTtsCpp)
        {
            ChatterboxTtsCpp.StopServer();
        }
        if (keepAlive is not VoxCPM2CrispAsr)
        {
            VoxCPM2CrispAsr.StopServer();
        }
        if (keepAlive is not ZonosTtsCrispAsr)
        {
            ZonosTtsCrispAsr.StopServer();
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
        if (voice == null)
        {
            // A failed engine switch (network blip, missing API key) legitimately leaves the
            // voice list empty now - without this, Generate ended instantly with no message.
            await MessageBox.Show(
                Window!,
                Se.Language.General.Error,
                "No voices are loaded for the selected engine - check the engine settings (e.g. API key) and re-select the engine to reload its voices.",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        if (!await TtsVoiceInstaller.EnsureVoiceInstalled(engine, voice, Window, _windowService))
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
        ProgressPercentText = string.Empty;
        ProgressEtaText = string.Empty;
        _generateStopwatch.Restart();
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

        // The speed/merge stages shell out to ffmpeg, so record which build actually runs - a too
        // old or missing ffmpeg is a common cause of "stuck"/failed generation in bug reports
        // (#12093). Gated on the setting so the version probe only runs when logging is on.
        if (Se.Settings.Tools.WriteToolsLog)
        {
            var ffmpegPath = string.IsNullOrEmpty(Se.Settings.General.FfmpegPath) ? "ffmpeg" : Se.Settings.General.FfmpegPath;
            var banner = FfmpegHelper.GetVersionBanner(ffmpegPath);
            Se.WriteToolsLog($"Text-to-speech: ffmpeg=\"{ffmpegPath}\" - {(string.IsNullOrEmpty(banner) ? "version probe failed (ffmpeg missing or not runnable?)" : banner)}");
        }

        // Every step below reports expected failures by returning null (after logging/showing the
        // error itself). This catch is the safety net for everything unexpected: without it, an
        // exception escaping the async command was swallowed and the window stayed disabled on the
        // last progress text forever - the "stuck on Adjusting speed" state in #12093.
        try
        {
            // Generate
            var generateSpeechResult = await GenerateSpeech(_cancellationToken);
            if (generateSpeechResult == null)
            {
                ResetGeneratingUiState();
                return;
            }

            // Fix speed
            var fixSpeedResult = await FixSpeed(generateSpeechResult, _cancellationToken);
            if (fixSpeedResult == null)
            {
                ResetGeneratingUiState();
                return;
            }

            // Post-processing (pro audio chain, silence padding, sample rate)
            var postProcessResult = await ApplyPostProcessing(fixSpeedResult, _cancellationToken);
            if (postProcessResult == null)
            {
                ResetGeneratingUiState();
                return;
            }

            // Review audio clips
            if (DoReviewAudioClips)
            {
                var reviewAudioClipsResult = await ReviewAudioClips(postProcessResult);
                if (reviewAudioClipsResult == null)
                {
                    ResetGeneratingUiState();
                    return;
                }

                postProcessResult = reviewAudioClipsResult;
            }

            await MergeAndAddToVideo(postProcessResult);
        }
        catch (OperationCanceledException)
        {
            ResetGeneratingUiState();
        }
        catch (Exception ex)
        {
            SeLogger.Error(ex, "Text-to-speech: generation failed unexpectedly");
            Se.WriteToolsLog("Text-to-speech failed unexpectedly: " + ex, true);
            ResetGeneratingUiState();

            if (Window != null)
            {
                await MessageBox.Show(
                    Window,
                    Se.Language.General.Error,
                    "Text to speech failed: " + ex.Message + Environment.NewLine + Environment.NewLine +
                    "See error-log.txt in the Subtitle Edit data folder for details.",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }

    /// <summary>
    /// Returns the window from "generating" (buttons disabled, progress showing) to its idle state.
    /// </summary>
    private void ResetGeneratingUiState()
    {
        _generateStopwatch.Stop();
        ProgressPercentText = string.Empty;
        ProgressEtaText = string.Empty;
        DoneOrCancelText = Se.Language.General.Done;
        IsGenerating = false;
        IsNotGenerating = true;
        ProgressOpacity = 0;
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

    // Downloads the selected engine's currently selected model up front, instead of waiting for
    // the user to click "Generate speech from text". Reuses IsEngineInstalled - the same path
    // Generate/Test use - so the runtime and the model are fetched (with the usual size prompts)
    // and no download logic is duplicated. Only shown for local per-model engines via
    // IsModelDownloadVisible.
    [RelayCommand]
    private async Task DownloadModel()
    {
        var engine = SelectedEngine;
        if (engine == null || Window == null)
        {
            return;
        }

        // Already present? The combo's green dot already says so, and IsEngineInstalled would
        // just return true with no visible feedback - so offer a re-download instead (e.g. to
        // repair a corrupted or partial download).
        if (IsSelectedModelInstalled())
        {
            var sizeText = GetModelDownloadSizeText(engine, SelectedModel);
            var sizeSuffix = string.IsNullOrEmpty(sizeText) ? string.Empty : $" ({sizeText})";
            var answer = await MessageBox.Show(
                Window,
                string.Format(Se.Language.General.ReDownloadX, SelectedModel),
                $"{Environment.NewLine}\"{SelectedModel}\" is already downloaded.{sizeSuffix}{Environment.NewLine}{Environment.NewLine}Re-download it?",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (answer == MessageBoxResult.Yes)
            {
                await RedownloadSelectedModel(engine);
                RefreshDownloadDots?.Invoke();
            }

            return;
        }

        await IsEngineInstalled(engine);
        RefreshDownloadDots?.Invoke();
    }

    // Re-fetches the selected model's files (overwriting what's on disk) via the per-engine model
    // download dialog. The StartDownload*Models calls download unconditionally - unlike
    // IsEngineInstalled, which skips already-installed models - so this is the path for repairing
    // or refreshing a model the user already has.
    private async Task RedownloadSelectedModel(ITtsEngine engine)
    {
        switch (engine)
        {
            case Qwen3TtsCpp:
                await _windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(Window!, vm => vm.StartDownloadQwen3TtsModels(Qwen3TtsCpp.ResolveModelKey(SelectedModel)));
                break;
            case Qwen3TtsCrispAsr:
                await _windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(Window!, vm => vm.StartDownloadQwen3TtsCrispAsrModels(Qwen3TtsCrispAsr.ResolveModelKey(SelectedModel)));
                break;
            case VibeVoiceCrispAsr:
                await _windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(Window!, vm => vm.StartDownloadVibeVoiceCrispAsrModels(VibeVoiceCrispAsr.ResolveModelKey(SelectedModel)));
                break;
            case IndexTtsCrispAsr:
                await _windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(Window!, vm => vm.StartDownloadIndexTtsCrispAsrModels(IndexTtsCrispAsr.ResolveModelKey(SelectedModel)));
                break;
            case CosyVoice3CrispAsr:
                await _windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(Window!, vm => vm.StartDownloadCosyVoice3CrispAsrModels(CosyVoice3CrispAsr.ResolveModelKey(SelectedModel)));
                break;
            case F5TtsCrispAsr:
                await _windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(Window!, vm => vm.StartDownloadF5TtsCrispAsrModels(F5TtsCrispAsr.ResolveModelKey(SelectedModel)));
                break;
            case VoxCPM2CrispAsr:
                await _windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(Window!, vm => vm.StartDownloadVoxCPM2CrispAsrModels(VoxCPM2CrispAsr.ResolveModelKey(SelectedModel)));
                break;
            case ZonosTtsCrispAsr:
                await _windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(Window!, vm => vm.StartDownloadZonosTtsCrispAsrModels());
                break;
            case ChatterboxTtsCpp:
                await _windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(Window!, vm => vm.StartDownloadChatterboxModels(ChatterboxTtsCpp.ResolveModelKey(SelectedModel)));
                break;
        }
    }

    private bool IsSelectedModelInstalled()
    {
        return GetModelDotStatus(SelectedEngine, SelectedModel) == DownloadDotStatus.UpToDate;
    }

    // Approximate on-disk download size for a per-model engine's model key, e.g. "~1.6 GB".
    // Single source of truth for the size shown in the model combo and the download-confirmation
    // prompts in IsEngineInstalled. Returns string.Empty when the engine has no per-model
    // download, or when the model key already embeds its size in the label (VibeVoice / IndexTTS
    // keys read like "Q8_0 (~2.8 GB)").
    public string GetModelDownloadSizeText(ITtsEngine? engine, string? modelKey)
    {
        return TtsEngineInstaller.GetModelDownloadSizeText(engine, modelKey);
    }

    // Display text for a model combo entry: the model key plus its approximate download size when
    // known (e.g. "1.7B  (~1.6 GB)"), so the cost is visible before downloading.
    public string GetModelDisplayText(string modelKey)
    {
        var size = GetModelDownloadSizeText(SelectedEngine, modelKey);
        return string.IsNullOrEmpty(size) ? modelKey : $"{modelKey}  ({size})";
    }

    // Install-status dot for a model in the model combo. Local engines (Qwen3, Chatterbox, ...)
    // get a green/grey dot per model; cloud engines (ElevenLabs, Mistral) have nothing to install.
    public DownloadDotStatus GetModelDotStatus(ITtsEngine? engine, string? modelKey)
    {
        if (string.IsNullOrEmpty(modelKey))
        {
            return DownloadDotStatus.None;
        }

        return engine switch
        {
            Qwen3TtsCpp => Qwen3TtsCpp.IsModelsInstalled(modelKey)
                ? DownloadDotStatus.UpToDate
                : DownloadDotStatus.NotInstalled,
            Qwen3TtsCrispAsr => Qwen3TtsCrispAsr.AreModelsInstalled(modelKey)
                ? DownloadDotStatus.UpToDate
                : DownloadDotStatus.NotInstalled,
            VibeVoiceCrispAsr => VibeVoiceCrispAsr.AreModelsInstalled(modelKey)
                ? DownloadDotStatus.UpToDate
                : DownloadDotStatus.NotInstalled,
            IndexTtsCrispAsr => IndexTtsCrispAsr.AreModelsInstalled(modelKey)
                ? DownloadDotStatus.UpToDate
                : DownloadDotStatus.NotInstalled,
            CosyVoice3CrispAsr => CosyVoice3CrispAsr.AreModelsInstalled(modelKey)
                ? DownloadDotStatus.UpToDate
                : DownloadDotStatus.NotInstalled,
            F5TtsCrispAsr => F5TtsCrispAsr.AreModelsInstalled(modelKey)
                ? DownloadDotStatus.UpToDate
                : DownloadDotStatus.NotInstalled,
            VoxCPM2CrispAsr => VoxCPM2CrispAsr.AreModelsInstalled(modelKey)
                ? DownloadDotStatus.UpToDate
                : DownloadDotStatus.NotInstalled,
            ZonosTtsCrispAsr => ZonosTtsCrispAsr.AreModelsInstalled()
                ? DownloadDotStatus.UpToDate
                : DownloadDotStatus.NotInstalled,
            ChatterboxTtsCpp => ChatterboxTtsCpp.AreModelsInstalled(modelKey)
                ? DownloadDotStatus.UpToDate
                : DownloadDotStatus.NotInstalled,
            _ => DownloadDotStatus.None,
        };
    }

    [RelayCommand]
    private async Task TestVoice()
    {
        var engine = SelectedEngine;
        var voice = SelectedVoice;
        if (engine == null || voice == null || Window == null || IsGenerating)
        {
            // IsGenerating guard: the button's IsVoiceTestEnabled binding is timer-driven (mpv
            // idle) and stays true during a generate run, so this can be clicked mid-pipeline.
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
        // A local token only: assigning the popup's CTS into the shared _cancellationTokenSource/
        // _cancellationToken fields hijacked a running generate pipeline (the pipeline re-reads
        // the fields per stage) - the popup's Cancel then aborted the whole run, and the main
        // Cancel button cancelled the popup instead of the generation.
        var testVoiceToken = generatingAudioVm.CancellationTokenSource.Token;
        try
        {
            var result = await engine.Speak(text, _waveFolder, voice, SelectedLanguage, SelectedRegion, SelectedModel, testVoiceToken);
            if (!testVoiceToken.IsCancellationRequested)
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
        Voice[] voices;
        try
        {
            voices = await engine.RefreshVoices(string.Empty, CancellationToken.None);
        }
        catch (Exception ex)
        {
            // The voice-list downloads throw on HTTP failure (so an error body cannot overwrite
            // the cached list). Keep the current voices and tell the user instead of crashing
            // whichever command triggered the refresh.
            SeLogger.Error(ex, $"Refreshing voices for {engine.Name} failed - keeping the cached voice list");
            if (Window != null)
            {
                await MessageBox.Show(
                    Window,
                    Se.Language.General.Error,
                    $"Refreshing voices failed: {ex.Message}",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

            return;
        }

        Voices.Clear();
        foreach (var voice in voices)
        {
            Voices.Add(voice);
        }
        SelectedVoice = Voices.FirstOrDefault(v => v.Name == Se.Settings.Video.TextToSpeech.Voice) ?? Voices.FirstOrDefault();
        VoiceCount = Voices.Count;
        VoiceCountInfo = string.Format(Se.Language.Video.TextToSpeech.XVoices, Voices.Count);
        IsVoiceCountVisible = Voices.Count > 0;
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

        // The pattern must start with '*' as-is: passing "SubtitleEditTts.json" would get a "*."
        // prefix from PickOpenFile, and that pattern cannot match the exported file itself (Export
        // writes exactly "SubtitleEditTts.json"), hiding it in SE's own open dialog (#12093).
        var fileName = await _fileHelper.PickOpenFile(Window, "Open SubtitleEditTts.json file", "TTS json files", "*SubtitleEditTts.json");
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
        var jsonFolder = Path.GetDirectoryName(fileName) ?? string.Empty;
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
                CurrentFileName = ResolveImportedAudioFileName(item.AudioFileName, jsonFolder),
                Paragraph = paragraph,
                SpeedFactor = item.SpeedFactor <= 0 ? 1.0f : item.SpeedFactor,
                Voice = voice,
                // Restore the per-line engine snapshot so the review window's click-to-sync
                // (and any future regeneration) can recover the exact engine/model/instruction
                // each line was produced with.
                EngineName = item.EngineName ?? string.Empty,
                Model = item.Model ?? string.Empty,
                Instruction = item.Instruction ?? string.Empty,
                Include = item.Include,
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

        // The review window needs at least one engine to offer regeneration; an empty global
        // voice list is fine (each imported line resolved its own engine's voices above, and
        // the review window guards regenerate on a missing voice) - blocking on it rejected
        // perfectly reviewable sessions whenever the *selected* engine's voice load failed.
        if (Engines.Count == 0)
        {
            await MessageBox.Show(
                Window,
                Se.Language.General.Error,
                "No usable TTS engines are available - check the engine settings and try again.",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
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
                SelectedVoice ?? Voices.FirstOrDefault(),
                Languages.ToArray(),
                SelectedLanguage,
                videoFileNameForReview,
                // Scratch folder for regenerate intermediates - NOT the imported JSON's folder:
                // using that folder filled the user's own export folder with GUID-named temp
                // wavs on every regenerate (#12093). Same location the fresh Generate flow uses.
                Path.GetTempPath(),
                peaksForReview);
            // Forward the imported cast so a subsequent Export round-trips the mappings instead
            // of writing ActorVoiceMappings = [] back to SubtitleEditTts.json.
            vm.ActorVoiceMappings.AddRange(_actorVoiceMappings);

            if (peaksForReview == null || peaksForReview.Peaks.Count == 0)
            {
                _ = GenerateWavePeaksIfNeededAsync(videoFileNameForReview, vm);
            }
        });

        // OK means "publish": run the same merge/add-to-video tail as the generate pipeline.
        // This result used to be discarded, so OK after an import behaved exactly like Cancel -
        // no merged wav, no dialog, nothing (#12093).
        if (result.OkPressed)
        {
            // Enter the same visible "generating" state as a fresh Generate run - progress bar,
            // status text and a working Cancel button. Without this the merge ran with the
            // window looking completely idle, giving no sign anything was happening (#12093).
            ProgressValue = 0;
            ProgressText = string.Empty;
            ProgressPercentText = string.Empty;
            ProgressEtaText = string.Empty;
            IsGenerating = true;
            IsNotGenerating = false;
            ProgressOpacity = 1.0;
            DoneOrCancelText = Se.Language.General.Cancel;

            try
            {
                await MergeAndAddToVideo(result.StepResults);
            }
            catch (OperationCanceledException)
            {
                ResetGeneratingUiState();
            }
            catch (Exception ex)
            {
                SeLogger.Error(ex, "Text-to-speech: merging imported audio segments failed");
                Se.WriteToolsLog("Text-to-speech: merging imported audio segments failed: " + ex, true);
                ResetGeneratingUiState();

                await MessageBox.Show(
                    Window,
                    Se.Language.General.Error,
                    "Merging the audio segments failed: " + ex.Message + Environment.NewLine + Environment.NewLine +
                    "See error-log.txt in the Subtitle Edit data folder for details.",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }

    /// <summary>
    /// Resolves an imported item's audio file. New exports store just the file name (resolved
    /// against the JSON's own folder, so the export folder can be moved or shared); older exports
    /// stored absolute paths - honored when they still exist, otherwise the same name is tried
    /// next to the JSON.
    /// </summary>
    private static string ResolveImportedAudioFileName(string? audioFileName, string jsonFolder)
    {
        if (string.IsNullOrEmpty(audioFileName))
        {
            return string.Empty;
        }

        // A Windows path is not recognized as rooted on Unix ("C:\...", "\\server\share\...",
        // "\folder\..."), so Path.IsPathRooted alone let legacy Windows exports opened on
        // macOS/Linux fall through to a garbage jsonFolder + "C:\..." combine - skipping the
        // sibling retry that would find the file. New-format exports store bare file names
        // (never containing a backslash), so any backslash marks a legacy Windows path.
        var isLegacyWindowsPath = audioFileName.Contains('\\');

        if (Path.IsPathRooted(audioFileName) || isLegacyWindowsPath)
        {
            if (File.Exists(audioFileName))
            {
                return audioFileName;
            }

            // Path.GetFileName does not treat '\' as a separator on Unix - split on both.
            var separatorIndex = Math.Max(audioFileName.LastIndexOf('\\'), audioFileName.LastIndexOf('/'));
            var name = separatorIndex >= 0 ? audioFileName.Substring(separatorIndex + 1) : audioFileName;
            var siblingFileName = Path.Combine(jsonFolder, name);
            return File.Exists(siblingFileName) ? siblingFileName : audioFileName;
        }

        return Path.Combine(jsonFolder, audioFileName);
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
        return await TtsEngineInstaller.EnsureEngineInstalled(engine, Window, _windowService, SelectedRegion, SelectedModel, ApiKey, KeyFile, () => RefreshVoices(engine));
    }

    private async Task MergeAndAddToVideo(TtsStepResult[] fixSpeedResult)
    {
        // Merge audio paragraphs
        var mergedAudioFileName = await MergeAudioParagraphs(fixSpeedResult, _cancellationToken);
        if (string.IsNullOrEmpty(mergedAudioFileName))
        {
            ResetGeneratingUiState();
            return;
        }

        // Save dialog with the subtitle's name as the default, instead of a folder picker plus
        // a silently chosen name. The old exists-check fallback chain (video name -> subtitle
        // name -> GUID) gave repeated exports to the same folder a different name every run,
        // ending in a bare GUID (#12093). The native dialog owns the overwrite confirmation.
        var audioFileName = await _fileHelper.PickSaveFile(Window!, ".wav", GetSuggestedMergedAudioFileName(), Se.Language.General.SaveFileAsTitle);
        if (string.IsNullOrEmpty(audioFileName))
        {
            ResetGeneratingUiState();
            return;
        }
        var outputFolder = Path.GetDirectoryName(audioFileName)!;

        File.Move(mergedAudioFileName, audioFileName, true);
        Se.WriteToolsLog($"TTS merge done: wrote \"{audioFileName}\"");

        await HandleAddToVideo(audioFileName, outputFolder, _cancellationToken);

        ResetGeneratingUiState();
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

        // Never let the output collide with the source video: picking the video's own folder as
        // the output folder can make the paths identical, and ffmpeg runs with -y - depending on
        // the build that either aborts or *truncates the user's source video* before reading it.
        if (string.Equals(Path.GetFullPath(outputFileName), Path.GetFullPath(_videoFileName), StringComparison.OrdinalIgnoreCase))
        {
            outputFileName = Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(audioFileName) + "_tts" + videoExt);
        }

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

        // ffmpeg failures (bad custom encoding string, codec/container mismatch, ...) used to go
        // unnoticed here, and the caller then showed "Video file generated" for a file that does
        // not exist. Verify the output and report instead.
        if (!File.Exists(outputFileName) || new FileInfo(outputFileName).Length == 0)
        {
            SeLogger.Error($"TextToSpeech: adding audio to video failed - no output produced (encoding=\"{audioEncoding}\", ducking={Se.Settings.Video.TextToSpeech.AudioDuckingEnabled})");
            Se.WriteToolsLog($"TTS add-to-video failed: ffmpeg produced no output for \"{outputFileName}\" (encoding=\"{audioEncoding}\", ducking={Se.Settings.Video.TextToSpeech.AudioDuckingEnabled})", true);
            if (Window != null)
            {
                await MessageBox.Show(
                    Window,
                    Se.Language.General.Error,
                    "Adding the audio track to the video failed - the audio file was still saved." + Environment.NewLine + Environment.NewLine +
                    "See error-log.txt in the Subtitle Edit data folder for details.",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

            return null;
        }

        return outputFileName;
    }

    private async Task<string?> MergeAudioParagraphs(TtsStepResult[] previousStepResult, CancellationToken cancellationToken)
    {
        try
        {
            // No engine/voice needed here - each step result carries its own audio file. An
            // engine/voice null-guard used to silently abort the merge for imported sessions
            // whose selected engine had no loadable voice list ("OK does nothing", #12093).
            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }

            bool forceStereo = false;
            var useCustomAudioEncoding = !string.IsNullOrEmpty(Se.Settings.Video.TextToSpeech.CustomAudioEncoding);
            if (Se.Settings.Video.TextToSpeech.CustomAudioStereo && useCustomAudioEncoding)
            {
                forceStereo = true;
            }

            var silenceFileName = await GenerateSilenceWaveFile(previousStepResult, cancellationToken);

            var inputFileName = silenceFileName;
            ProgressValue = 0;
            for (var index = 0; index < previousStepResult.Length; index++)
            {
                ProgressText = $"Merging audio: segment {index + 1} of {previousStepResult.Length}";

                var item = previousStepResult[index];

                // Upstream steps can drop a segment's audio (failed generation kept by the user,
                // deleted files in an imported session). Leave silence for it instead of feeding
                // ffmpeg a nonexistent input.
                if (string.IsNullOrEmpty(item.CurrentFileName) || !File.Exists(item.CurrentFileName))
                {
                    Se.WriteToolsLog($"TTS merge: segment {index + 1} has no audio file - leaving silence", true);
                    continue;
                }

                var outputFileName = Path.Combine(_waveFolder, $"silence{index}.wav");
                if (File.Exists(outputFileName))
                {
                    outputFileName = Path.Combine(_waveFolder, $"silence_{Guid.NewGuid()}.wav");
                }

                var mergeProcess = FfmpegGenerator.MergeAudioTracks(inputFileName, item.CurrentFileName, outputFileName, (float)item.Paragraph.StartTime.TotalSeconds, forceStereo);
                await mergeProcess.StartAndWaitAsync(cancellationToken);

                // A failed merge used to go unnoticed: the loop advanced to the (missing) output
                // and deleted the last good intermediate, so every later merge failed too and the
                // wreck only surfaced far downstream. Stop with an error instead.
                if (!File.Exists(outputFileName) || new FileInfo(outputFileName).Length == 0)
                {
                    SeLogger.Error($"TextToSpeech: merging segment {index + 1} produced no output (input=\"{item.CurrentFileName}\")");
                    Se.WriteToolsLog($"TTS merge: segment {index + 1} failed - ffmpeg produced no output for \"{item.CurrentFileName}\"", true);
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        if (Window != null)
                        {
                            await MessageBox.Show(
                                Window,
                                Se.Language.General.Error,
                                $"Merging audio failed at segment {index + 1}." + Environment.NewLine + Environment.NewLine +
                                "See error-log.txt in the Subtitle Edit data folder for details.",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    });
                    return null;
                }

                DeleteFileNoError(inputFileName);
                inputFileName = outputFileName;

                ProgressValue = (double)(index + 1) / previousStepResult.Length * 100.0;
            }
            ProgressValue = 100;

            // The chain head is the fully merged track (or the bare silence track if every
            // segment was skipped - still a valid, if empty, result the user gets told about
            // via the forced tools-log entries above).
            return inputFileName;
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

    private async Task<string> GenerateSilenceWaveFile(TtsStepResult[] stepResults, CancellationToken cancellationToken)
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

        // An imported session can outlast the subtitle currently open in the main window (or
        // there may be no video at all) - the silence base track must cover every segment being
        // merged, or the tail would be dropped.
        if (stepResults.Length > 0)
        {
            var maxEndSeconds = (float)stepResults.Max(r => r.Paragraph.EndTime.TotalSeconds);
            durationInSeconds = Math.Max(durationInSeconds, maxEndSeconds);
        }

        var silenceProcess = FfmpegGenerator.GenerateEmptyAudio(silenceFileName, durationInSeconds);
        await silenceProcess.StartAndWaitAsync(cancellationToken);
        return silenceFileName;
    }

    /// <summary>
    /// Default name offered in the merged-wav save dialog: the subtitle's own name (what the
    /// user asked for in #12093), the video's name when the subtitle is unsaved, and a generic
    /// name as the last resort. Returns a full path when the source's folder is known, so the
    /// dialog also starts in that folder.
    /// </summary>
    private string GetSuggestedMergedAudioFileName()
    {
        // A new, never-saved subtitle has the literal FileName "Untitled" (no path/extension).
        var subtitleFileName = _subtitle.FileName;
        var sourceFileName = !string.IsNullOrEmpty(subtitleFileName) && subtitleFileName != "Untitled"
            ? subtitleFileName
            : _videoFileName;
        if (string.IsNullOrEmpty(sourceFileName))
        {
            return "SubtitleEditTts.wav";
        }

        var folder = Path.GetDirectoryName(sourceFileName);
        var name = Path.GetFileNameWithoutExtension(sourceFileName) + ".wav";
        return string.IsNullOrEmpty(folder) ? name : Path.Combine(folder, name);
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

            // Distinct failure reasons reported by the engines, so the dialogs below can say *why*
            // segments failed (e.g. the ElevenLabs 429 text) instead of a bare count (#12093).
            var errorMessages = new List<string>();

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
                if (speakResult.Error && !string.IsNullOrEmpty(speakResult.ErrorMessage) && !errorMessages.Contains(speakResult.ErrorMessage))
                {
                    errorMessages.Add(speakResult.ErrorMessage);
                }
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
            // First engine-reported failure reason, e.g. the ElevenLabs 429 text. The generic
            // rate/pitch hint only applies when no engine said anything more specific.
            var firstError = errorMessages.Count > 0
                ? errorMessages[0]
                : "Check the engine settings (rate/pitch/volume must be a signed integer, e.g. \"+10\") and try again.";
            if (failedCount == resultList.Count && resultList.Count > 0)
            {
                var msg = $"Text-to-speech failed for all {failedCount} segments." +
                          Environment.NewLine + Environment.NewLine + firstError;
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
                // Failed segments were silent before (log only), so users ended up with audio
                // missing lines and no idea why (#12093) - tell them and let them stop the run.
                SeLogger.Error($"TextToSpeech: {failedCount} of {resultList.Count} segments failed to generate; continuing with the rest.");
                Se.WriteToolsLog($"TTS generation: {failedCount} of {resultList.Count} segments failed - see error-log.txt for the engine errors", true);

                var proceed = await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    if (Window == null)
                    {
                        return true;
                    }

                    var detail = errorMessages.Count > 0
                        ? Environment.NewLine + Environment.NewLine + errorMessages[0]
                        : string.Empty;
                    var answer = await MessageBox.Show(
                        Window,
                        Se.Language.General.Warning,
                        $"{failedCount} of {resultList.Count} segments failed to generate (see error-log.txt in the Subtitle Edit data folder).{detail}" +
                        Environment.NewLine + Environment.NewLine +
                        "Continue with the remaining segments? The failed lines will be missing from the audio.",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);
                    return answer == MessageBoxResult.Yes;
                });

                if (!proceed)
                {
                    return null;
                }
            }
            else
            {
                Se.WriteToolsLog($"TTS generation done: all {resultList.Count} segments generated");
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

        // Fallbacks must carry the panel's instruction, not "": TtsInstructionSwap swaps the
        // engine's saved instruction to whatever is passed here for the duration of the Speak
        // call, so an empty fallback silently wiped the user's typed voice-design instruction
        // for every line without an actor mapping - i.e. for every line of a normal subtitle.
        var globalInstruction = Instruction ?? string.Empty;

        if (string.IsNullOrEmpty(actor) || !ctx.ByActor.TryGetValue(actor, out var mapping))
        {
            return new ResolvedVoice(defaultEngine, defaultVoice, null, text, globalInstruction);
        }

        if (!ctx.EnginesByName.TryGetValue(mapping.EngineName, out var mappedEngine)
            || !ctx.VoicesByEngine.TryGetValue(mapping.EngineName, out var voices))
        {
            return new ResolvedVoice(defaultEngine, defaultVoice, null, text, globalInstruction);
        }

        var mappedVoice = voices.FirstOrDefault(v => string.Equals(v.Name, mapping.VoiceName, StringComparison.OrdinalIgnoreCase));
        if (mappedVoice == null)
        {
            return new ResolvedVoice(defaultEngine, defaultVoice, null, text, globalInstruction);
        }

        // Per-row model overrides the global one; empty string means "use the global model".
        var modelOverride = string.IsNullOrWhiteSpace(mapping.Model) ? null : mapping.Model;
        // A mapped row with no instruction of its own, on the globally selected engine, follows
        // the panel instruction - otherwise mapping an actor to the same engine/voice made that
        // actor's lines drop the voice design every unmapped line gets.
        var instruction = string.IsNullOrWhiteSpace(mapping.Instruction) && ReferenceEquals(mappedEngine, defaultEngine)
            ? globalInstruction
            : mapping.Instruction ?? string.Empty;
        return new ResolvedVoice(mappedEngine, mappedVoice, modelOverride, text, instruction);
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

        // Generous per-ffmpeg-call bound: each call processes a single subtitle's audio (seconds
        // long), so minutes means ffmpeg is wedged - kill it and surface an error instead of
        // waiting forever with the window disabled (#12093).
        var segmentOperationTimeout = TimeSpan.FromMinutes(5);

        var skippedNoAudioCount = 0;
        var skippedNoDurationCount = 0;
        var stretchedCount = 0;
        var failedCount = 0;

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
                    skippedNoAudioCount++;
                    SeLogger.Error($"TextToSpeech: skipping segment {index + 1} in FixSpeed - upstream produced no audio file");
                    continue;
                }

                // A single bad segment (corrupt audio, wedged/failed ffmpeg call) must not kill the
                // whole run: keep its audio at original speed, log it, and continue with the rest -
                // same policy as the generation step. Cancellation still aborts the run.
                try
                {
                    // Step 1: Trim silence from start and end
                    var outputFileName1 = Path.Combine(Path.GetDirectoryName(item.CurrentFileName)!, Guid.NewGuid() + ".wav");
                    var trimProcess = FfmpegGenerator.TrimSilenceStartAndEnd(item.CurrentFileName, outputFileName1);
                    await trimProcess.StartAndWaitAsync(cancellationToken, segmentOperationTimeout);

                    var currentFile = outputFileName1;

                    // Step 2: VAD-based internal silence compression
                    // Compress pauses between words/phrases before touching tempo.
                    // This preserves phoneme quality by only removing redundant silence.
                    if (doVad)
                    {
                        var vadOutput = Path.Combine(Path.GetDirectoryName(item.CurrentFileName)!, $"vad_{Guid.NewGuid()}.wav");
                        var vadProcess = FfmpegGenerator.CompressInternalSilence(currentFile, vadOutput, vadMaxSilence);
                        await vadProcess.StartAndWaitAsync(cancellationToken, segmentOperationTimeout);

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
                        // The trim/VAD output is missing or unreadable (ffmpeg problem). Keep the
                        // engine's original audio at original speed - same policy as the other
                        // failure paths - instead of silently dropping the line from the output.
                        skippedNoDurationCount++;
                        Se.WriteToolsLog($"TTS FixSpeed: segment {index + 1} - could not read duration of \"{currentFile}\" (trim output missing or unreadable; ffmpeg problem?) - keeping original audio", true);
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
                    await speedProcess.StartAndWaitAsync(cancellationToken, segmentOperationTimeout);

                    // Fallback: if rubberband failed (not available in FFmpeg build), retry with atempo
                    if (doHighQualityStretch && (!File.Exists(outputFileName2) || new FileInfo(outputFileName2).Length == 0))
                    {
                        var fallbackProcess = FfmpegGenerator.ChangeSpeed(currentFile, outputFileName2, (float)factor);
                        await fallbackProcess.StartAndWaitAsync(cancellationToken, segmentOperationTimeout);
                    }

                    if (!File.Exists(outputFileName2) || new FileInfo(outputFileName2).Length == 0)
                    {
                        // Speed change produced nothing - fall back to the un-stretched audio so the
                        // segment is not lost (it may overlap the next line slightly).
                        failedCount++;
                        resultList[resultList.Count - 1].CurrentFileName = currentFile;
                        resultList[resultList.Count - 1].SpeedFactor = 1.0f;
                        Se.WriteToolsLog($"TTS FixSpeed: segment {index + 1} speed change (factor {factor:0.###}) produced no output - keeping original speed", true);
                    }
                    else
                    {
                        stretchedCount++;
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    // Keep the segment at original speed and continue with the rest.
                    failedCount++;
                    SeLogger.Error(ex, $"TextToSpeech: FixSpeed failed for segment {index + 1} - keeping original audio");
                    Se.WriteToolsLog($"TTS FixSpeed: segment {index + 1} failed ({ex.Message}) - keeping original audio", true);

                    // The stretch path adds its result entry before running ffmpeg, so this
                    // segment may already be in the list - point that entry back at the original
                    // audio instead of adding a duplicate.
                    if (resultList.Count > 0 && ReferenceEquals(resultList[resultList.Count - 1].Paragraph, p))
                    {
                        resultList[resultList.Count - 1].CurrentFileName = item.CurrentFileName;
                        resultList[resultList.Count - 1].SpeedFactor = 1.0f;
                    }
                    else
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
                    }
                }
            }
            ProgressValue = 100;

            Se.WriteToolsLog(
                $"TTS FixSpeed done: {resultList.Count} of {previousStepResult.Length} segments" +
                $", stretched={stretchedCount}" +
                $", skippedNoAudio={skippedNoAudioCount}" +
                $", skippedNoDuration={skippedNoDurationCount}" +
                $", failed={failedCount}",
                force: skippedNoAudioCount + skippedNoDurationCount + failedCount > 0);

            return resultList.ToArray();
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        catch (Exception ex)
        {
            // Unexpected failure outside the per-segment guard (e.g. ffmpeg missing entirely).
            // Report it instead of leaving the window disabled on the last progress text (#12093).
            SeLogger.Error(ex, "TextToSpeech: FixSpeed failed");
            Se.WriteToolsLog("TTS FixSpeed failed: " + ex, true);
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                if (Window != null)
                {
                    await MessageBox.Show(
                        Window,
                        Se.Language.General.Error,
                        "Adjusting audio speed failed: " + ex.Message + Environment.NewLine + Environment.NewLine +
                        "See error-log.txt in the Subtitle Edit data folder for details.",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            });
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
            var failedCount = 0;
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

                var processedFile = item.CurrentFileName;
                try
                {
                    processedFile = await TtsPostProcessor.ApplyPostProcessing(item.CurrentFileName, Path.GetDirectoryName(item.CurrentFileName)!, cancellationToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    // Keep the unprocessed audio and continue with the rest - one bad segment
                    // must not abort the whole run (#12093).
                    failedCount++;
                    SeLogger.Error(ex, $"TextToSpeech: post-processing failed for segment {index + 1} - keeping unprocessed audio");
                    Se.WriteToolsLog($"TTS post-processing: segment {index + 1} failed ({ex.Message}) - keeping unprocessed audio", true);
                }

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

            Se.WriteToolsLog(
                $"TTS post-processing done: {resultList.Count} of {previousStepResult.Length} segments, failed={failedCount}",
                force: failedCount > 0);

            return resultList.ToArray();
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        catch (Exception ex)
        {
            SeLogger.Error(ex, "TextToSpeech: post-processing failed");
            Se.WriteToolsLog("TTS post-processing failed: " + ex, true);
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                if (Window != null)
                {
                    await MessageBox.Show(
                        Window,
                        Se.Language.General.Error,
                        "Audio post-processing failed: " + ex.Message + Environment.NewLine + Environment.NewLine +
                        "See error-log.txt in the Subtitle Edit data folder for details.",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            });
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

    // The compact cloud-engine descriptions ("pay/fast/good") read like debug output - expand
    // them into words; free-form descriptions (the CrispASR engines) are shown as-is. The
    // source strings are hardcoded English in the engines, so this map matches that.
    // Middle-truncates a file name so the tail (and thus the extension) stays visible.
    private static string CapFileName(string fileName, int maxLength)
    {
        if (fileName.Length <= maxLength)
        {
            return fileName;
        }

        const int keepEnd = 12;
        return string.Concat(fileName.AsSpan(0, maxLength - keepEnd - 1), "…", fileName.AsSpan(fileName.Length - keepEnd));
    }

    private static string PrettifyEngineDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return string.Empty;
        }

        var parts = description.Split('/');
        if (parts.Length < 2 || parts.Any(x => x.Contains(' ')))
        {
            return description;
        }

        var words = parts.Select(x => x.Trim().ToLowerInvariant() switch
        {
            "free" => "Free",
            "pay" => "Paid",
            "fast" => "Fast",
            "slow" => "Slow",
            "good" => "good quality",
            "ok" => "ok quality",
            "multilingual" => "multilingual",
            _ => x.Trim(),
        });
        return string.Join(" \u00b7 ", words);
    }

    // Elapsed/remaining for the progress row. Driven by ProgressValue changes (always raised on
    // the UI thread by the pipeline); the estimate is a simple rate projection and only shows
    // once enough progress exists for it not to jump around.
    private readonly Stopwatch _generateStopwatch = new();

    partial void OnProgressValueChanged(double value)
    {
        if (!IsGenerating || value <= 0)
        {
            return;
        }

        ProgressPercentText = $"{Math.Clamp((int)Math.Round(value), 0, 100)}%";

        var elapsed = _generateStopwatch.Elapsed;
        if (elapsed.TotalSeconds < 3)
        {
            return;
        }

        if (value >= 3 && value <= 100)
        {
            var remaining = TimeSpan.FromSeconds(elapsed.TotalSeconds * (100 - value) / value);
            ProgressEtaText = string.Format(Se.Language.Video.TextToSpeech.XElapsedYLeft, FormatProgressDuration(elapsed), FormatProgressDuration(remaining));
        }
        else
        {
            ProgressEtaText = string.Format(Se.Language.Video.TextToSpeech.XElapsed, FormatProgressDuration(elapsed));
        }
    }

    private static string FormatProgressDuration(TimeSpan t) =>
        t.TotalHours >= 1 ? $"{(int)t.TotalHours}:{t.Minutes:00}:{t.Seconds:00}" : $"{t.Minutes}:{t.Seconds:00}";

    internal void SelectedEngineChanged(object? sender, SelectionChangedEventArgs e)
    {
        var engine = SelectedEngine;
        if (engine == null)
        {
            return;
        }

        Dispatcher.UIThread.PostSafe(async () =>
        {
            // Captured before any SelectedModel assignment below: OnSelectedModelChanged
            // persists the Qwen3 (CrispASR) model on every change, so the generic
            // "SelectedModel = Models.FirstOrDefault()" default overwrote the saved model
            // with "1.7B VoiceDesign" right before the engine block tried to restore it -
            // the user's model choice never survived a window open or engine re-switch.
            var savedQwen3CrispAsrModel = Se.Settings.Video.TextToSpeech.Qwen3TtsCrispAsrModel;

            IsEngineSettingsVisible = false;
            IsModelDownloadVisible = false;

            // Engine capability flags first, and never skipped: when the voice load below fails
            // or returns nothing (missing API key, no network), the user must still get the new
            // engine's own fields (API key, region, model, ...) to fix the cause. The old flow
            // returned early on an empty voice list, leaving every panel flag - and the voice
            // list itself - describing the *previous* engine, so Generate could then hand the
            // wrong engine's Voice object to Speak.
            HasLanguageParameter = engine.HasLanguageParameter;
            HasApiKey = engine.HasApiKey;
            HasRegion = engine.HasRegion;
            HasModel = engine.HasModel;
            HasKeyFile = engine.HasKeyFile;
            IsEdgeTtsEngine = engine is EdgeTts;
            EngineDescription = PrettifyEngineDescription(engine.Description);
            HasEngineDescription = !string.IsNullOrEmpty(EngineDescription);

            Voice[] voices;
            try
            {
                voices = await engine.GetVoices(SelectedLanguage?.Code ?? string.Empty);
            }
            catch (Exception ex)
            {
                // PostSafe used to swallow this, skipping the whole engine switch. Show an empty
                // voice list for the new engine instead of the previous engine's voices.
                SeLogger.Error(ex, $"Loading voices for {engine.Name} failed");
                voices = [];
            }

            Voices.Clear();
            foreach (var vo in voices)
            {
                Voices.Add(vo);
            }
            VoiceCount = Voices.Count;
            // The label binds VoiceCountInfo; only VoiceCount was ever written, so the voice
            // count next to the combo stayed permanently blank.
            VoiceCountInfo = string.Format(Se.Language.Video.TextToSpeech.XVoices, Voices.Count);
            IsVoiceCountVisible = Voices.Count > 0;

            var lastVoice = Voices.FirstOrDefault(v => v.Name == Se.Settings.Video.TextToSpeech.Voice);
            if (lastVoice == null)
            {
                lastVoice = Voices.FirstOrDefault(p => p.Name.StartsWith("en", StringComparison.OrdinalIgnoreCase) ||
                                                       p.Name.Contains("English", StringComparison.OrdinalIgnoreCase));
            }
            SelectedVoice = lastVoice ?? Voices.FirstOrDefault();

            // Unconditional (handles a null voice fine): gating this on a loaded voice left the
            // previous engine's instruction box and voice-combo lock in place when the new
            // engine's voice list failed to load or was empty.
            ApplyInstructionForEngine(engine);

            if (HasLanguageParameter && SelectedVoice != null)
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
            else if (SelectedVoice == null)
            {
                // No voices for the new engine - don't keep listing the previous engine's
                // languages next to it.
                Languages.Clear();
                SelectedLanguage = null;
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
                IsModelDownloadVisible = true;
            }
            else if (SelectedEngine is Qwen3TtsCrispAsr)
            {
                SelectedModel = Models.FirstOrDefault(p => p == savedQwen3CrispAsrModel);
                if (string.IsNullOrEmpty(SelectedModel))
                {
                    SelectedModel = Models.FirstOrDefault();
                }
                IsEngineSettingsVisible = true;
                IsModelDownloadVisible = true;
            }
            else if (SelectedEngine is VibeVoiceCrispAsr)
            {
                SelectedModel = Models.FirstOrDefault(p => p == Se.Settings.Video.TextToSpeech.VibeVoiceCrispAsrModel);
                if (string.IsNullOrEmpty(SelectedModel))
                {
                    SelectedModel = Models.FirstOrDefault();
                }
                IsEngineSettingsVisible = true;
                IsModelDownloadVisible = true;
            }
            else if (SelectedEngine is IndexTtsCrispAsr)
            {
                SelectedModel = Models.FirstOrDefault(p => p == Se.Settings.Video.TextToSpeech.IndexTtsCrispAsrModel);
                if (string.IsNullOrEmpty(SelectedModel))
                {
                    SelectedModel = Models.FirstOrDefault();
                }
                IsEngineSettingsVisible = true;
                IsModelDownloadVisible = true;
            }
            else if (SelectedEngine is CosyVoice3CrispAsr)
            {
                SelectedModel = Models.FirstOrDefault(p => p == Se.Settings.Video.TextToSpeech.CosyVoice3CrispAsrModel);
                if (string.IsNullOrEmpty(SelectedModel))
                {
                    SelectedModel = Models.FirstOrDefault();
                }
                IsEngineSettingsVisible = true;
                IsModelDownloadVisible = true;
            }
            else if (SelectedEngine is VoxCPM2CrispAsr)
            {
                SelectedModel = Models.FirstOrDefault(p => p == Se.Settings.Video.TextToSpeech.VoxCPM2CrispAsrModel);
                if (string.IsNullOrEmpty(SelectedModel))
                {
                    SelectedModel = Models.FirstOrDefault();
                }
                IsEngineSettingsVisible = true;
                IsModelDownloadVisible = true;
            }
            else if (SelectedEngine is F5TtsCrispAsr)
            {
                SelectedModel = Models.FirstOrDefault(p => p == Se.Settings.Video.TextToSpeech.F5TtsCrispAsrModel);
                if (string.IsNullOrEmpty(SelectedModel))
                {
                    SelectedModel = Models.FirstOrDefault();
                }
                IsEngineSettingsVisible = true;
                IsModelDownloadVisible = true;
            }
            else if (SelectedEngine is ZonosTtsCrispAsr)
            {
                // Minimal engine: single fixed quant (no model dropdown) and no settings
                // dialog. Show only the model-download button so the user can fetch the GGUFs.
                IsModelDownloadVisible = true;
            }
            else if (SelectedEngine is ChatterboxTtsCpp)
            {
                SelectedModel = Models.FirstOrDefault(p => p == Se.Settings.Video.TextToSpeech.ChatterboxModel);
                if (string.IsNullOrEmpty(SelectedModel))
                {
                    SelectedModel = Models.FirstOrDefault();
                }
                IsEngineSettingsVisible = true;
                IsModelDownloadVisible = true;
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
            // Escape during a run cancels the run (like the Cancel button) instead of closing
            // the window over a live pipeline - closing also fires StopAllCrispAsrServers,
            // killing the engine server an in-flight Speak is talking to.
            if (IsGenerating)
            {
                _cancellationTokenSource?.Cancel();
                return;
            }

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
        // Title-bar close during a run: cancel the pipeline so it unwinds instead of running
        // headless against a closed window (progress writes, folder picker, message boxes) while
        // StopAllCrispAsrServers below kills the engine server it is talking to.
        try { _cancellationTokenSource?.Cancel(); } catch (ObjectDisposedException) { }

        // An enabled System.Timers.Timer is rooted: without stopping it here, every open/close
        // of this window leaked a view model whose Elapsed handler kept firing every 100 ms for
        // the rest of the session (ReviewSpeechViewModel already does this on close).
        _timer.Stop();
        _timer.Elapsed -= OnTimerOnElapsed;
        _timer.Dispose();

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