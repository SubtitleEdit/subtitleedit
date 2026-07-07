using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.AudioToText;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Shared.GetAudioClips;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.EngineSettings;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.OpenAiCompatible;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using Nikse.SubtitleEdit.Logic.Media;
using Optris.Icons.Avalonia;
using System.Net.Http;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText;

public partial class SpeechToTextViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<ISpeechToTextEngine> _engines;
    [ObservableProperty] private ISpeechToTextEngine _selectedEngine;

    [ObservableProperty] private ObservableCollection<WhisperLanguage> _languages;
    [ObservableProperty] private WhisperLanguage? _selectedLanguage;

    [ObservableProperty] private ObservableCollection<SpeechToTextModelDisplay> _models;
    [ObservableProperty] private SpeechToTextModelDisplay? _selectedModel;

    [ObservableProperty] private ObservableCollection<SpeechToTextJobItem> _batchItems;
    [ObservableProperty] private SpeechToTextJobItem? _selectedBatchItem;

    [ObservableProperty] private bool _doTranslateToEnglish;
    [ObservableProperty] private bool _doAdjustTimings;
    [ObservableProperty] private bool _doPostProcessing;

    [ObservableProperty] private string _parameters;

    [ObservableProperty] private string _consoleLog;

    [ObservableProperty] private bool _isBatchMode;
    [ObservableProperty] private bool _isBatchModeVisible;
    [ObservableProperty] private bool _isSingleModeVisible;
    [ObservableProperty] private bool _isWhisperCppActive;
    [ObservableProperty] private bool _isWhisperPurfviewXxlActive;
    [ObservableProperty] private bool _isTranscribeEnabled;
    [ObservableProperty] private bool _isTranslateVisible;
    [ObservableProperty] private bool _isBackendSelectionVisible;
    [ObservableProperty] private bool _isModelSelectionVisible;
    [ObservableProperty] private bool _isLanguageSelectionVisible;
    [ObservableProperty] private bool _isWhisperCppSelected;
    [ObservableProperty] private ObservableCollection<ISpeechToTextEngine> _whisperCppBackends;
    [ObservableProperty] private ISpeechToTextEngine? _selectedWhisperCppBackend;
    [ObservableProperty] private bool _isCrispAsrSelected;
    [ObservableProperty] private ObservableCollection<CrispAsrEngineBase> _crispAsrBackends;
    [ObservableProperty] private CrispAsrEngineBase? _selectedCrispAsrBackend;
    [ObservableProperty] private bool _isForcedAlignerVisible;
    [ObservableProperty] private ObservableCollection<ForcedAlignerOption> _forcedAligners;
    [ObservableProperty] private ForcedAlignerOption? _selectedForcedAligner;
    [ObservableProperty] private double _progressOpacity;

    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private string _progressText;
    [ObservableProperty] private string _elapsedText;
    [ObservableProperty] private string _estimatedText;
    [ObservableProperty] private bool _isReDownloadVisible;
    [ObservableProperty] private string _reDownloadText;

    [ObservableProperty] private bool _isEngineDownloadButtonVisible;
    [ObservableProperty] private bool _isEngineSettingsButtonVisible;
    [ObservableProperty] private string _engineDownloadHint;

    [ObservableProperty] private bool _isOpenAiCompatibleSttVisible;
    [ObservableProperty] private bool _isAdvancedSettingsVisible = true;
    [ObservableProperty] private string? _openAiCompatibleSttUrl;
    [ObservableProperty] private string? _openAiCompatibleSttApiKey;
    [ObservableProperty] private string? _openAiCompatibleSttModel;
    [ObservableProperty] private string? _openAiCompatibleSttLanguage;
    [ObservableProperty] private int _openAiCompatibleSttTimeoutSeconds;
    [ObservableProperty] private decimal _openAiCompatibleSttTemperature;
    [ObservableProperty] private string? _openAiCompatibleSttPrompt;
    [ObservableProperty] private string? _openAiCompatibleSttExtraHeaders;
    [ObservableProperty] private bool _openAiCompatibleSttStream;
    [ObservableProperty] private string _openAiCompatibleSttAudioFormat = "mp3";
    public ObservableCollection<string> OpenAiCompatibleSttAudioFormats { get; } = new(new[] { "mp3", "m4a", "webm", "wav" });

    [ObservableProperty] private bool _isOpenRouterSttVisible;
    [ObservableProperty] private string? _openRouterSttApiKey;
    [ObservableProperty] private string? _openRouterSttModel;
    [ObservableProperty] private string? _openRouterSttLanguage;
    [ObservableProperty] private decimal _openRouterSttTemperature;
    [ObservableProperty] private string? _openRouterSttPrompt;
    [ObservableProperty] private int _openRouterSttTimeoutSeconds;

    [ObservableProperty] private bool _isDashScopeSttVisible;
    [ObservableProperty] private string? _dashScopeSttApiKey;
    [ObservableProperty] private string? _dashScopeSttModel;
    [ObservableProperty] private string? _dashScopeSttLanguage;
    [ObservableProperty] private string _dashScopeSttRegion = "international";
    [ObservableProperty] private bool _dashScopeSttEnableWords;
    [ObservableProperty] private int _dashScopeSttTimeoutSeconds;
    public ObservableCollection<string> DashScopeSttRegions { get; } = new(new[] { "international", "china" });

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }
    public Subtitle TranscribedSubtitle { get; private set; }
    public List<AudioClip> ResultAudioClips { get; private set; }
    public string? LastBatchSubtitleFileName { get; private set; }
    // Two views host the console log — the batch-mode grid layout and the single-mode
    // standalone TextBox. Only one is visible at a time, so we track both and tail
    // both in LogToConsole; otherwise the hidden one's reference can shadow the
    // visible one and the user sees the log freeze at the top.
    public TextBox TextBoxConsoleLogBatch { get; internal set; }
    public TextBox TextBoxConsoleLogSingle { get; internal set; }
    public Button? CopyConsoleLogButton { get; internal set; }
    public DataGrid BatchGrid { get; internal set; }

    private bool _unknownArgument;
    private bool _cudaOutOfMemory;
    private bool _incompleteModel;
    private bool _loadedFromStdOut;
    private string? _videoFileName;
    private string _audioFileName = string.Empty;
    private int _audioTrackNumber;
    private readonly List<string> _filesToDelete = new();
    private string? _sttTempFolder;
    private readonly ConcurrentQueue<string> _outputText = new();
    private long _startTicks = 0;
    private double _endSeconds;
    private double _showProgressPct = -1;
    private readonly VideoInfo _videoInfo = new();
    private bool _abort;
    private CancellationTokenSource? _openAiCts;
    private readonly List<ResultText> _resultList = new();
    private bool _useCenterChannelOnly;

    private readonly Regex _timeRegexShort =
        new(@"^\[\d\d:\d\d[\.,]\d\d\d --> \d\d:\d\d[\.,]\d\d\d\]", RegexOptions.Compiled);

    private readonly Regex _timeRegexLong =
        new(@"^\[\d\d:\d\d:\d\d[\.,]\d\d\d --> \d\d:\d\d:\d\d[\.,]\d\d\d]", RegexOptions.Compiled);

    private readonly Regex _pctWhisper = new(@"^\d+%\|", RegexOptions.Compiled);
    private readonly Regex _pctWhisperFaster = new(@"^\s*\d+%\s*\|", RegexOptions.Compiled);
    private readonly System.Timers.Timer _timerWhisper = new();
    private Process _whisperProcess = new();
    private Process? _audioExtractProcess = new();
    private readonly System.Timers.Timer _timerAudioExtract = new();
    private Stopwatch _sw = new();
    private StringBuilder _ffmpegLog = new();
    private readonly Lock _lockObj = new();
    private int _batchIndex = -1;
    private string _error;
    private List<AudioClip>? _audioClips;
    private bool _audioClipsAutoStart;
    private string _chatLlmText = string.Empty;
    private string _qwen3AsrOutputJsonPath = string.Empty;

    private readonly IWindowService _windowService;
    private readonly IFileHelper _fileHelper;
    private bool _isUpdatingWhisperCppBackend;
    private bool _isUpdatingCrispAsrBackend;
    private static bool _crispAsrUpdatePromptShown;
    private static bool _whisperCppUpdatePromptShown;

    /// <summary>
    /// Hook the view wires up so the engine combobox can re-evaluate its install-status dots
    /// after a re-download. The combo's <c>FuncDataTemplate</c> snapshots the install state when
    /// each row is realised, so without an explicit refresh the dot stays on its old colour
    /// (typically amber) even though the sidecar now reports up-to-date.
    /// </summary>
    public Action? RefreshEngineCombo { get; set; }

    public SpeechToTextViewModel(IWindowService windowService, IFileHelper fileHelper)
    {
        _windowService = windowService;
        _fileHelper = fileHelper;

        Engines = [new WhisperCppEngine()];
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Engines.Add(new WhisperEnginePurfviewFasterWhisperXxl());
            Engines.Add(new WhisperEngineConstMe());
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) &&
                 RuntimeInformation.ProcessArchitecture == Architecture.X64)
        {
            // Purfview only ships Windows and Linux x86_64 builds - there is no Linux ARM64
            // binary, so don't offer the engine on Linux ARM (it could only fail to run).
            Engines.Add(new WhisperEnginePurfviewFasterWhisperXxl());
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
            (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && RuntimeInformation.ProcessArchitecture == Architecture.Arm64) ||
            (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && RuntimeInformation.ProcessArchitecture == Architecture.X64))
        {
            Engines.Add(new WhisperEngineCTranslate2());
        }

        // MLX Whisper runs Whisper on the Apple GPU / Neural Engine and is arm64-only, so offer it
        // only on Apple Silicon.
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) &&
            RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
        {
            Engines.Add(new MlxWhisperMac());
        }

        // Faster Whisper Mac drives the pip-installed faster-whisper (CTranslate2) library.
        // CPU-only (CTranslate2 has no Apple GPU backend) and slower than MLX, but its decoding
        // gives better results in some languages (e.g. Arabic). Works on Apple Silicon and Intel.
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Engines.Add(new FasterWhisperMac());
        }

        Engines.Add(new WhisperEngineOpenAi());

        // Add OpenAI Compatible STT engine (available on all platforms)
        Engines.Add(new OpenAiCompatibleSttEngine());

        // Online STT services reachable on all platforms
        Engines.Add(new OpenRouterSttEngine());
        Engines.Add(new DashScopeQwen3SttEngine());

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            //Engines.Add(new ChatLlmCppEngine());
            Engines.Add(new Qwen3AsrCppEngine());
        }

        Engines.Add(new CrispAsrEngine());

        SelectedEngine = Engines[0];

        Languages = new ObservableCollection<WhisperLanguage>(GetEngineLanguages(GetEffectiveSelectedEngine()));
        SelectedLanguage = PickDefaultLanguage(Languages);

        Models = new ObservableCollection<SpeechToTextModelDisplay>();

        BatchItems = new ObservableCollection<SpeechToTextJobItem>();
        WhisperCppBackends = new ObservableCollection<ISpeechToTextEngine>();
        CrispAsrBackends = new ObservableCollection<CrispAsrEngineBase>();
        ForcedAligners = new ObservableCollection<ForcedAlignerOption>();

        ResultAudioClips = new List<AudioClip>();

        IsTranscribeEnabled = true;
        IsTranslateVisible = IsTranslateAvailable(GetEffectiveSelectedEngine());
        IsBackendSelectionVisible = false;
        IsModelSelectionVisible = true;
        IsWhisperCppSelected = false;
        IsCrispAsrSelected = false;
        Parameters = string.Empty;
        ConsoleLog = string.Empty;
        ProgressText = string.Empty;
        ElapsedText = string.Empty;
        EstimatedText = string.Empty;
        TranscribedSubtitle = new Subtitle();
        TextBoxConsoleLogBatch = new TextBox();
        TextBoxConsoleLogSingle = new TextBox();
        BatchGrid = new DataGrid();
        ReDownloadText = string.Empty;
        EngineDownloadHint = string.Empty;
        _audioTrackNumber = -1;
        _error = string.Empty;

        LoadSettings();

        _timerWhisper.Interval = 100;
        _timerWhisper.Elapsed += OnTimerWhisperOnElapsed;

        _timerAudioExtract.Interval = 100;
        _timerAudioExtract.Elapsed += OnTimerAudioExtractOnElapsed;
    }

    private void LoadSettings()
    {
        DoTranslateToEnglish = false;
        DoAdjustTimings = Se.Settings.Tools.AudioToText.WhisperAutoAdjustTimings;
        DoPostProcessing = Se.Settings.Tools.AudioToText.PostProcessing;

        OpenAiCompatibleSttUrl = Se.Settings.Tools.OpenAiCompatibleSttUrl;
        OpenAiCompatibleSttApiKey = Se.Settings.Tools.OpenAiCompatibleSttApiKey;
        OpenAiCompatibleSttModel = Se.Settings.Tools.OpenAiCompatibleSttModel;
        OpenAiCompatibleSttLanguage = Se.Settings.Tools.OpenAiCompatibleSttLanguage;
        OpenAiCompatibleSttTimeoutSeconds = Se.Settings.Tools.OpenAiCompatibleSttTimeoutSeconds;
        OpenAiCompatibleSttTemperature = Se.Settings.Tools.OpenAiCompatibleSttTemperature;
        OpenAiCompatibleSttPrompt = Se.Settings.Tools.OpenAiCompatibleSttPrompt;
        OpenAiCompatibleSttExtraHeaders = Se.Settings.Tools.OpenAiCompatibleSttExtraHeaders;
        OpenAiCompatibleSttStream = Se.Settings.Tools.OpenAiCompatibleSttStream;
        var savedFormat = Se.Settings.Tools.OpenAiCompatibleSttAudioFormat;
        OpenAiCompatibleSttAudioFormat = OpenAiCompatibleSttAudioFormats.Contains(savedFormat) ? savedFormat : "mp3";

        OpenRouterSttApiKey = Se.Settings.Tools.OpenRouterSttApiKey;
        OpenRouterSttModel = Se.Settings.Tools.OpenRouterSttModel;
        OpenRouterSttLanguage = Se.Settings.Tools.OpenRouterSttLanguage;
        OpenRouterSttTemperature = Se.Settings.Tools.OpenRouterSttTemperature;
        OpenRouterSttPrompt = Se.Settings.Tools.OpenRouterSttPrompt;
        OpenRouterSttTimeoutSeconds = Se.Settings.Tools.OpenRouterSttTimeoutSeconds;

        DashScopeSttApiKey = Se.Settings.Tools.DashScopeSttApiKey;
        DashScopeSttModel = Se.Settings.Tools.DashScopeSttModel;
        DashScopeSttLanguage = Se.Settings.Tools.DashScopeSttLanguage;
        var savedRegion = Se.Settings.Tools.DashScopeSttRegion;
        DashScopeSttRegion = DashScopeSttRegions.Contains(savedRegion) ? savedRegion : "international";
        DashScopeSttEnableWords = Se.Settings.Tools.DashScopeSttEnableWords;
        DashScopeSttTimeoutSeconds = Se.Settings.Tools.DashScopeSttTimeoutSeconds;

        var savedChoice = Se.Settings.Tools.AudioToText.WhisperChoice;
        var whisperCppEngine = Engines.OfType<WhisperCppEngine>().FirstOrDefault();
        var crispAsrEngine = Engines.OfType<CrispAsrEngine>().FirstOrDefault();
        if (whisperCppEngine != null && whisperCppEngine.TrySelectBackendChoice(savedChoice))
        {
            SelectedEngine = whisperCppEngine;
        }
        else if (crispAsrEngine != null && crispAsrEngine.TrySelectBackendChoice(savedChoice))
        {
            SelectedEngine = crispAsrEngine;
        }
        else
        {
            var selectedEngine = Enumerable.FirstOrDefault<ISpeechToTextEngine>((IEnumerable<ISpeechToTextEngine>)Engines, p => p.Choice == savedChoice);
            if (selectedEngine != null)
            {
                SelectedEngine = selectedEngine;
            }
        }

        Parameters = GetEffectiveSelectedEngine().CommandLineParameter;

        EngineChanged();
    }

    private void SaveSettings()
    {
        Se.Settings.Tools.AudioToText.WhisperAutoAdjustTimings = DoAdjustTimings;
        Se.Settings.Tools.AudioToText.PostProcessing = DoPostProcessing;
        var engine = GetEffectiveSelectedEngine();
        engine.CommandLineParameter = Parameters;
        Se.Settings.Tools.AudioToText.WhisperChoice = engine.Choice;
        Se.Settings.Tools.AudioToText.WhisperModel = SelectedModel?.Model.Name ?? string.Empty;
        Se.Settings.Tools.AudioToText.WhisperLanguageCode = SelectedLanguage?.Code ?? string.Empty;
        Se.Settings.Tools.AudioToText.CrispAsrForcedAligner = SelectedForcedAligner?.Choice ?? ForcedAlignerOption.BuiltInChoice;

        Se.Settings.Tools.OpenAiCompatibleSttUrl = OpenAiCompatibleSttUrl ?? string.Empty;
        Se.Settings.Tools.OpenAiCompatibleSttApiKey = OpenAiCompatibleSttApiKey ?? string.Empty;
        Se.Settings.Tools.OpenAiCompatibleSttModel = OpenAiCompatibleSttModel ?? string.Empty;
        Se.Settings.Tools.OpenAiCompatibleSttLanguage = OpenAiCompatibleSttLanguage ?? string.Empty;
        Se.Settings.Tools.OpenAiCompatibleSttTimeoutSeconds = OpenAiCompatibleSttTimeoutSeconds;
        Se.Settings.Tools.OpenAiCompatibleSttTemperature = OpenAiCompatibleSttTemperature;
        Se.Settings.Tools.OpenAiCompatibleSttPrompt = OpenAiCompatibleSttPrompt ?? string.Empty;
        Se.Settings.Tools.OpenAiCompatibleSttExtraHeaders = OpenAiCompatibleSttExtraHeaders ?? string.Empty;
        Se.Settings.Tools.OpenAiCompatibleSttStream = OpenAiCompatibleSttStream;
        Se.Settings.Tools.OpenAiCompatibleSttAudioFormat = OpenAiCompatibleSttAudioFormat ?? "mp3";

        Se.Settings.Tools.OpenRouterSttApiKey = OpenRouterSttApiKey ?? string.Empty;
        Se.Settings.Tools.OpenRouterSttModel = OpenRouterSttModel ?? string.Empty;
        Se.Settings.Tools.OpenRouterSttLanguage = OpenRouterSttLanguage ?? string.Empty;
        Se.Settings.Tools.OpenRouterSttTemperature = OpenRouterSttTemperature;
        Se.Settings.Tools.OpenRouterSttPrompt = OpenRouterSttPrompt ?? string.Empty;
        Se.Settings.Tools.OpenRouterSttTimeoutSeconds = OpenRouterSttTimeoutSeconds;

        Se.Settings.Tools.DashScopeSttApiKey = DashScopeSttApiKey ?? string.Empty;
        Se.Settings.Tools.DashScopeSttModel = DashScopeSttModel ?? string.Empty;
        Se.Settings.Tools.DashScopeSttLanguage = DashScopeSttLanguage ?? string.Empty;
        Se.Settings.Tools.DashScopeSttRegion = DashScopeSttRegion ?? "international";
        Se.Settings.Tools.DashScopeSttEnableWords = DashScopeSttEnableWords;
        Se.Settings.Tools.DashScopeSttTimeoutSeconds = DashScopeSttTimeoutSeconds;

        Se.SaveSettings();
    }

    private ISpeechToTextEngine GetEffectiveSelectedEngine()
    {
        return SelectedEngine switch
        {
            WhisperCppEngine whisperCppEngine => whisperCppEngine.SelectedBackend,
            CrispAsrEngine crispAsrEngine => crispAsrEngine.SelectedBackend,
            _ => SelectedEngine,
        };
    }

    // The mainstream Whisper engines that can auto-detect the spoken language
    // (useful for files with mixed languages). See issue #11848.
    private static bool EngineSupportsAutoLanguageDetection(ISpeechToTextEngine engine)
    {
        return engine.Choice is WhisperChoice.Cpp
            or WhisperChoice.CppCuBlas
            or WhisperChoice.CppVulkan
            or WhisperChoice.CppCuBlasLib
            or WhisperChoice.ConstMe
            or WhisperChoice.PurfviewFasterWhisperXxl
            or WhisperChoice.CTranslate2
            or WhisperChoice.FasterWhisperMac
            or WhisperChoice.OpenAi;
    }

    // Builds the language dropdown for an engine, prepending an "Auto detect" entry
    // (code "auto") for engines that support automatic language detection.
    private static IEnumerable<WhisperLanguage> GetEngineLanguages(ISpeechToTextEngine engine)
    {
        var result = new List<WhisperLanguage>();
        if (EngineSupportsAutoLanguageDetection(engine))
        {
            result.Add(new WhisperLanguage("auto", "Auto detect"));
        }

        // Bubble the user's favorite languages to the top (the "Auto detect" entry stays first).
        result.AddRange(LanguageFavoritesHelper.Order(engine.Languages, l => l.Code));
        return result;
    }

    private static bool IsTranslateAvailable(ISpeechToTextEngine engine)
    {
        return engine is not ChatLlmCppEngine and not Qwen3AsrCppEngine and not ICrispAsrEngine and not IOnlineSttEngine;
    }

    private void UpdateBackendSelectionUi()
    {
        UpdateWhisperCppBackendUi();
        UpdateCrispAsrBackendUi();
        UpdateForcedAlignerUi();
        IsBackendSelectionVisible = IsWhisperCppSelected || IsCrispAsrSelected;
        IsForcedAlignerVisible = IsCrispAsrSelected;
    }

    private void UpdateForcedAlignerUi()
    {
        var engine = GetEffectiveSelectedEngine();
        var crispEngine = engine as ICrispAsrEngine;
        var hasNative = crispEngine?.HasNativeTimestamps == true;

        var newOptions = new List<ForcedAlignerOption>();
        if (hasNative)
        {
            newOptions.Add(ForcedAlignerOption.BuiltIn());
        }
        newOptions.Add(ForcedAlignerOption.CanaryCtc());
        newOptions.Add(ForcedAlignerOption.Qwen3());
        // wav2vec2 "WhisperX aligner zoo" — 12 language-specific CTC aligners
        // that work on top of any Crisp ASR backend via `-am <path>`.
        newOptions.AddRange(ForcedAlignerOption.Wav2Vec2All());

        foreach (var opt in newOptions)
        {
            opt.IsInstalled = IsAlignerInstalled(opt, crispEngine);
            opt.Display = opt.BaseDisplay;
        }

        ForcedAligners.Clear();
        foreach (var opt in newOptions)
        {
            ForcedAligners.Add(opt);
        }

        if (crispEngine == null)
        {
            return;
        }

        // Fun-ASR is a CJK/Korean-focused speech-LLM; Canary CTC only aligns English/European,
        // so prefer the multilingual Qwen3 aligner (like Qwen3/Mega) which covers its languages.
        var preferredChoice = hasNative
            ? ForcedAlignerOption.BuiltInChoice
            : (crispEngine is CrispAsrQwen3 or CrispAsrMega or CrispAsrFunAsrNano ? ForcedAlignerOption.Qwen3Choice : ForcedAlignerOption.CanaryCtcChoice);

        var match = ForcedAligners.FirstOrDefault(p => p.Choice == preferredChoice)
                    ?? ForcedAligners.FirstOrDefault();
        if (!ReferenceEquals(SelectedForcedAligner, match))
        {
            SelectedForcedAligner = match;
        }
    }

    // Whether the aligner GGUF is already on disk for the given engine. A partial/aborted
    // download leaves a tiny stub behind, so require a plausible model size (> 10 MB) before
    // treating it as installed.
    private static bool IsAlignerInstalled(ForcedAlignerOption option, ICrispAsrEngine? crispEngine)
    {
        if (option.IsBuiltIn || string.IsNullOrEmpty(option.FileName) || crispEngine is not CrispAsrEngineBase baseEngine)
        {
            return false;
        }

        var path = baseEngine.GetModelForCmdLine(option.FileName);
        return File.Exists(path) && new FileInfo(path).Length > 10_000_000;
    }

    private void UpdateWhisperCppBackendUi()
    {
        if (SelectedEngine is WhisperCppEngine whisperCppEngine)
        {
            IsWhisperCppSelected = true;
            _isUpdatingWhisperCppBackend = true;
            try
            {
                if (WhisperCppBackends.Count != whisperCppEngine.Backends.Count)
                {
                    WhisperCppBackends.Clear();
                    foreach (var backend in whisperCppEngine.Backends)
                    {
                        WhisperCppBackends.Add(backend);
                    }
                }

                var match = WhisperCppBackends.FirstOrDefault(p => p.Choice == whisperCppEngine.SelectedBackend.Choice);
                if (!ReferenceEquals(SelectedWhisperCppBackend, match))
                {
                    SelectedWhisperCppBackend = match;
                }
            }
            finally
            {
                _isUpdatingWhisperCppBackend = false;
            }

            return;
        }

        IsWhisperCppSelected = false;
        _isUpdatingWhisperCppBackend = true;
        try
        {
            SelectedWhisperCppBackend = null;
        }
        finally
        {
            _isUpdatingWhisperCppBackend = false;
        }
    }

    private void UpdateCrispAsrBackendUi()
    {
        if (SelectedEngine is CrispAsrEngine crispAsrEngine)
        {
            IsCrispAsrSelected = true;
            _isUpdatingCrispAsrBackend = true;
            try
            {
                if (CrispAsrBackends.Count != crispAsrEngine.Backends.Count)
                {
                    CrispAsrBackends.Clear();
                    foreach (var backend in crispAsrEngine.Backends)
                    {
                        CrispAsrBackends.Add(backend);
                    }
                }

                var match = CrispAsrBackends.FirstOrDefault(p => p.Choice == crispAsrEngine.SelectedBackend.Choice);
                if (!ReferenceEquals(SelectedCrispAsrBackend, match))
                {
                    SelectedCrispAsrBackend = match;
                }
            }
            finally
            {
                _isUpdatingCrispAsrBackend = false;
            }

            return;
        }

        IsCrispAsrSelected = false;
        _isUpdatingCrispAsrBackend = true;
        try
        {
            SelectedCrispAsrBackend = null;
        }
        finally
        {
            _isUpdatingCrispAsrBackend = false;
        }
    }

    partial void OnSelectedWhisperCppBackendChanged(ISpeechToTextEngine? value)
    {
        if (_isUpdatingWhisperCppBackend || value == null || SelectedEngine is not WhisperCppEngine whisperCppEngine)
        {
            return;
        }

        if (string.Equals(whisperCppEngine.SelectedBackend.Choice, value.Choice, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        whisperCppEngine.SelectBackend(value);
        EngineChanged();
    }

    partial void OnSelectedCrispAsrBackendChanged(CrispAsrEngineBase? value)
    {
        if (_isUpdatingCrispAsrBackend || value == null || SelectedEngine is not CrispAsrEngine crispAsrEngine)
        {
            return;
        }

        if (string.Equals(crispAsrEngine.SelectedBackend.Choice, value.Choice, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        crispAsrEngine.SelectBackend(value);
        EngineChanged();
    }

    private void OnTimerWhisperOnElapsed(object? sender, ElapsedEventArgs args)
    {
        lock (_lockObj)
        {
            if (_abort)
            {
                _timerWhisper.Stop();
#pragma warning disable CA1416
                _whisperProcess.Kill(true);
#pragma warning restore CA1416

                Dispatcher.UIThread.Invoke<Task>(async () =>
                {
                    ProgressOpacity = 0;
                    var partialSub = new Subtitle();
                    partialSub.Paragraphs.AddRange(_resultList.OrderBy(p => p.Start)
                        .Select(p => new Paragraph(p.Text, (double)p.Start * 1000.0, (double)p.End * 1000.0)).ToList());

                    if (partialSub.Paragraphs.Count > 0)
                    {
                        var answer = await MessageBox.Show(
                            Window!,
                            $"Keep partial transcription?",
                            $"Do you want to keep {partialSub.Paragraphs.Count} lines?",
                            MessageBoxButtons.YesNoCancel,
                            MessageBoxIcon.Question);

                        if (answer != MessageBoxResult.Yes)
                        {
                            _resultList.Clear();
                            partialSub.Paragraphs.Clear();
                            IsTranscribeEnabled = true;
                            HideProgressBar();
                            return;
                        }
                    }

                    await MakeResult(partialSub);
                });

                return;
            }

            if (!_whisperProcess.HasExited)
            {
                var durationMs = (DateTime.UtcNow.Ticks - _startTicks) / 10_000;
                ProgressText = GetProgressText();

                ElapsedText = $"Time elapsed: {new TimeCode(durationMs).ToShortDisplayString()}";
                if (_endSeconds <= 0)
                {
                    if (_showProgressPct > 0)
                    {
                        SetProgressBarPct(_showProgressPct);
                    }

                    return;
                }

                ShowProgressBar();

                _videoInfo.TotalSeconds = Math.Max(_endSeconds, _videoInfo.TotalSeconds);
                var msPerFrame = durationMs / (_endSeconds * 1000.0);
                var estimatedTotalMs = msPerFrame * _videoInfo.TotalMilliseconds;
                var msEstimatedLeft = estimatedTotalMs - durationMs;

                if (_showProgressPct > 0)
                {
                    SetProgressBarPct(_showProgressPct);
                }
                else
                {
                    SetProgressBarPct(_endSeconds * 100.0 / _videoInfo.TotalSeconds);
                }

                EstimatedText = ProgressHelper.ToProgressTime(msEstimatedLeft);

                return;
            }

            _timerWhisper.Stop();

            var settings = Se.Settings.Tools.AudioToText;
            _whisperProcess.Dispose();

            var engine = GetEffectiveSelectedEngine();

            if (engine is ChatLlmCppEngine chatLlm)
            {
                ProcessChatLlmTranscription(settings, chatLlm);
                return;
            }

            if (engine is Qwen3AsrCppEngine)
            {
                ProcessQwen3AsrCppTranscription(settings);
                return;
            }

            Dispatcher.UIThread.Invoke<Task>(async () =>
            {
                LogToConsole($"Speech to text ({settings.WhisperChoice}) done in {_sw.Elapsed}{Environment.NewLine}");
                ProgressValue = 100;

                var hasError = false;
                if (_incompleteModel)
                {
                    await MessageBox.Show(Window!, "Incomplete model",
                        "The model is incomplete. Please download the full model.");
                    hasError = true;
                }
                else if (_unknownArgument && !string.IsNullOrEmpty(settings.WhisperCustomCommandLineArguments))
                {
                    await MessageBox.Show(Window!, $"Unknown argument: {settings.WhisperCustomCommandLineArguments}",
                        "Unknown argument. Please check the advanced settings.");
                    hasError = true;
                }
                else if (_cudaOutOfMemory)
                {
                    await MessageBox.Show(Window!, $"CUDA failed",
                        "Whisper ran out of CUDA memory - try a smaller model or run on CPU.");
                    hasError = true;
                }

                if (!hasError && GetResultFromSrt(_audioFileName, _videoFileName!, out var resultTexts, _outputText, _filesToDelete))
                {
                    _loadedFromStdOut = false;
                    var subtitle = new Subtitle();
                    subtitle.Paragraphs.AddRange(resultTexts
                        .Select(p => new Paragraph(p.Text, (double)p.Start * 1000.0, (double)p.End * 1000.0)).ToList());

                    var postProcessedSubtitle = PostProcess(subtitle);

                    if (_audioClips != null && ResultAudioClips.Count > 0)
                    {
                        var outputAudioClip = ResultAudioClips.FirstOrDefault(p => p.AudioFileName == _videoFileName);
                        if (outputAudioClip != null)
                        {
                            outputAudioClip.Transcription = new Subtitle(postProcessedSubtitle);
                        }
                    }

                    await MakeResult(postProcessedSubtitle);

                    return;
                }

                _outputText.Enqueue("Loading result from STDOUT");
                var transcribedSubtitleFromStdOut = new Subtitle();
                transcribedSubtitleFromStdOut.Paragraphs.AddRange(_resultList.OrderBy(p => p.Start)
                    .Select(p => new Paragraph(p.Text, (double)p.Start * 1000.0, (double)p.End * 1000.0)).ToList());
                _loadedFromStdOut = transcribedSubtitleFromStdOut.Paragraphs.Count > 0;
                await MakeResult(transcribedSubtitleFromStdOut);
            });
        }
    }

    private void ProcessChatLlmTranscription(SeAudioToText settings, ChatLlmCppEngine chatLlm)
    {
        var sbLog = new StringBuilder();
        foreach (var s in _outputText)
        {
            sbLog.AppendLine(s.TrimEnd());
        }

        var text = sbLog.ToString();

        if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(_chatLlmText))
        {
            var originalText = _chatLlmText;
            _chatLlmText = string.Empty;
            var lines = text.SplitToLines();
            var subtitle = new Subtitle();
            new SubRip().LoadSubtitle(subtitle, lines, string.Empty);
            if (subtitle.Paragraphs.Count > 0)
            {
                var last = subtitle.Paragraphs.Last();
                var indexOfTimings = last.Text.IndexOf("\ntimings:");
                if (indexOfTimings > 0)
                {
                    last.Text = last.Text.Substring(0, indexOfTimings).Trim();
                }

                ReInsertPeriodsEtc(originalText, subtitle);
                FixNegativeDuration(subtitle);

                var postProcessedSubtitle = PostProcess(subtitle);

                if (_audioClips != null && ResultAudioClips.Count > 0)
                {
                    var outputAudioClip = ResultAudioClips.FirstOrDefault(p => p.AudioFileName == _videoFileName);
                    if (outputAudioClip != null)
                    {
                        outputAudioClip.Transcription = new Subtitle(postProcessedSubtitle);
                    }
                }

                Dispatcher.UIThread.Invoke<Task>(async () =>
                {
                    LogToConsole($"Speech to text ({settings.WhisperChoice}) done in {_sw.Elapsed}{Environment.NewLine}");
                    ProgressValue = 100;
                    await MakeResult(postProcessedSubtitle);
                });
            }

            return;
        }


        var tag = "<asr_text>";
        var start = text.IndexOf(tag);
        if (start < 0)
        {
            LogToConsole($"Speech to text ({settings.WhisperChoice}) done in {_sw.Elapsed}{Environment.NewLine}");
            LogToConsole($"Speech to text: Could not find '{tag}' in text{Environment.NewLine}");
        }

        text = text.Remove(0, start + tag.Length);
        LogToConsole($"Speech to text step 1/2 ({settings.WhisperChoice}) done in {_sw.Elapsed}{Environment.NewLine}");
        LogToConsole($"Speech to text step 2/2 ({settings.WhisperChoice}) qwen3-focedaligner-0.6b.bin starting...");

        _chatLlmText = text;

        sbLog.Clear();
        _outputText.Clear();

        var exe = chatLlm.GetExecutable();
        var chatLlmParams = $" -m \"{chatLlm.GetModelForCmdLine("qwen3-focedaligner-0.6b.bin")}\" --multimedia-file-tags {{{{ }}}} -p \"{{{{audio:{_audioFileName}}}}}{_chatLlmText}\"";

        var p = new Process
        {
            StartInfo = new ProcessStartInfo(exe, chatLlmParams)
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                UseShellExecute = false,
                WorkingDirectory = Path.GetDirectoryName(exe),
            }
        };

        _whisperProcess = p;

        var dataReceivedHandler = (DataReceivedEventHandler)OutputHandler;
        if (dataReceivedHandler != null)
        {
            p.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            p.StartInfo.StandardErrorEncoding = Encoding.UTF8;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.OutputDataReceived += dataReceivedHandler;
            p.ErrorDataReceived += dataReceivedHandler;
        }

#pragma warning disable CA1416
        p.Start();
#pragma warning restore CA1416


        if (dataReceivedHandler != null)
        {
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();
        }

        _timerWhisper.Start();
    }

    private void ProcessQwen3AsrCppTranscription(SeAudioToText settings)
    {
        var jsonPath = _qwen3AsrOutputJsonPath;
        if (string.IsNullOrEmpty(jsonPath) || !File.Exists(jsonPath))
        {
            Dispatcher.UIThread.Invoke<Task>(async () =>
            {
                LogToConsole($"Speech to text ({settings.WhisperChoice}) done in {_sw.Elapsed}{Environment.NewLine}");
                if (_unknownArgument && !string.IsNullOrEmpty(settings.WhisperCustomCommandLineArguments))
                {
                    await MessageBox.Show(Window!, $"Unknown argument: {settings.WhisperCustomCommandLineArguments}",
                        "Unknown argument. Please check the advanced settings.");
                }
                LogToConsole($"Speech to text: Could not find output JSON file{Environment.NewLine}");
                ProgressValue = 100;
                IsTranscribeEnabled = true;
                await Task.CompletedTask;
            });
            return;
        }

        var rawJson = string.Empty;
        try
        {
            rawJson = File.ReadAllText(jsonPath);
            // qwen3-asr-cli can write raw control chars (e.g. a literal newline) inside JSON
            // string values, which strict System.Text.Json rejects ("'0x0A' is invalid within a
            // JSON string"). Escape those so a result is still produced (issue #11717).
            var jsonText = JsonRepair.EscapeControlCharsInStrings(rawJson);
            var jsonDoc = JsonDocument.Parse(jsonText);
            var words = jsonDoc.RootElement.GetProperty("words");

            var subtitle = new Subtitle();
            var currentText = new StringBuilder();
            var startTime = 0.0;
            var endTime = 0.0;
            var first = true;

            foreach (var word in words.EnumerateArray())
            {
                var text = word.GetProperty("word").GetString() ?? string.Empty;
                var start = word.GetProperty("start").GetDouble();
                var end = word.GetProperty("end").GetDouble();

                if (first)
                {
                    startTime = start;
                    first = false;
                }

                var newParagraph = false;
                if (currentText.Length > 0 && (start - endTime > 0.5 || currentText.Length + text.Length > 80))
                {
                    newParagraph = true;
                }

                if (newParagraph)
                {
                    subtitle.Paragraphs.Add(new Paragraph(currentText.ToString().Trim(), startTime * 1000.0, endTime * 1000.0));
                    currentText.Clear();
                    startTime = start;
                }

                if (currentText.Length > 0)
                {
                    currentText.Append(' ');
                }

                currentText.Append(text);
                endTime = end;
            }

            if (currentText.Length > 0)
            {
                subtitle.Paragraphs.Add(new Paragraph(currentText.ToString().Trim(), startTime * 1000.0, endTime * 1000.0));
            }

            FixNegativeDuration(subtitle);
            var postProcessedSubtitle = PostProcess(subtitle);

            if (_audioClips != null && ResultAudioClips.Count > 0)
            {
                var outputAudioClip = ResultAudioClips.FirstOrDefault(p => p.AudioFileName == _videoFileName);
                if (outputAudioClip != null)
                {
                    outputAudioClip.Transcription = new Subtitle(postProcessedSubtitle);
                }
            }

            Dispatcher.UIThread.Invoke<Task>(async () =>
            {
                LogToConsole($"Speech to text ({settings.WhisperChoice}) done in {_sw.Elapsed}{Environment.NewLine}");
                ProgressValue = 100;
                await MakeResult(postProcessedSubtitle);
            });
        }
        catch (Exception ex)
        {
            Se.LogError(ex, $"Failed to read Qwen3 ASR CPP output JSON '{jsonPath}'");
            // Persist the offending output so the failure is diagnosable — the temp .json is
            // deleted below, so logging its raw content here is the only record (issue #11717,
            // #11375). Force the write: the "write tools log" setting is off by default, so without
            // this the JSON that caused the parse error never reaches the user's bug report.
            var loggedJson = string.IsNullOrEmpty(rawJson)
                ? "<output JSON could not be read>"
                : rawJson;
            Se.WriteToolsLog($"Qwen3 ASR CPP output JSON failed to parse ({ex.Message}):{Environment.NewLine}{loggedJson}", true);
            Dispatcher.UIThread.Invoke<Task>(async () =>
            {
                LogToConsole($"Speech to text ({settings.WhisperChoice}) failed: {ex.Message}{Environment.NewLine}");
                if (ex is JsonException)
                {
                    LogToConsole($"The Qwen3 ASR engine produced output that could not be read. This is usually a problem with the engine or the chosen forced aligner (e.g. with some non-Latin scripts). Try re-running, a different model/aligner, or report it at {new Qwen3AsrCppEngine().Url}{Environment.NewLine}");
                }
                ProgressValue = 100;
                IsTranscribeEnabled = true;
                await Task.CompletedTask;
            });
        }
        finally
        {
            try
            {
                File.Delete(jsonPath);
            }
            catch
            {
                // ignore
            }
        }
    }

    /// <summary>
    /// Reachability check before running a transcription. Sends HEAD to the
    /// URL's authority (scheme://host:port/) — NOT the transcription endpoint
    /// path itself. We only confirm the server is up; many STT servers will
    /// 404 the root path, and any 2xx/3xx/4xx still proves the server
    /// answered. Network/DNS/timeout failures bubble up as the returned
    /// error string, which the caller displays to the user.
    /// </summary>
    private static async Task<string?> ProbeOpenAiUrlAsync(string url, CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return $"Invalid URL: '{url}'";
        }

        // Apply the 8-second probe deadline via a linked CTS so we can reuse
        // the shared HttpClient (whose Timeout is InfiniteTimeSpan) without
        // mutating it.
        using var probeCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        probeCts.CancelAfter(TimeSpan.FromSeconds(8));

        try
        {
            var baseUri = new Uri(uri.GetLeftPart(UriPartial.Authority));
            using var request = new HttpRequestMessage(HttpMethod.Head, baseUri);
            using var response = await OpenAiSttService.SharedHttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, probeCts.Token);
            return null;
        }
        catch (OperationCanceledException)
        {
            // If the caller cancelled, propagate; if our probe timed out, surface a clear message.
            if (cancellationToken.IsCancellationRequested)
            {
                throw;
            }

            return "Probe timed out after 8 seconds";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    private async Task ProcessOnlineSttTranscription(IOnlineSttEngine engine, string audioFileName, string? language = null, CancellationToken cancellationToken = default)
    {
        // Online STT engines read their config from Configuration.Settings.Tools,
        // so the user's in-window edits (URL, key, model, prompt, ...) must be
        // persisted before we read them. The transcribe action is the commit
        // moment for these engines — there is no separate OK button.
        SaveSettings();

        var transcriber = engine.CreateTranscriber(out var configError);
        if (transcriber == null)
        {
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await MessageBox.Show(Window!,
                    configError ?? Se.Language.General.OpenAiCompatibleSttUrlMissing,
                    Se.Language.General.ConfigurationRequired);
                IsTranscribeEnabled = true;
            });
            return;
        }

        ProgressText = Se.Language.Video.AudioToText.Transcribing;
        ProgressValue = 5;

        var probeError = await ProbeOpenAiUrlAsync(engine.ProbeUrl, cancellationToken);
        if (probeError != null)
        {
            LogToConsole($"Online STT endpoint probe failed: {probeError}. Retrying...");
            // Brief delay so transient DNS/socket hiccups have a chance to recover before retrying.
            await Task.Delay(300, cancellationToken);
            probeError = await ProbeOpenAiUrlAsync(engine.ProbeUrl, cancellationToken);
        }

        if (probeError != null)
        {
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await MessageBox.Show(Window!,
                    string.Format(Se.Language.General.OpenAiCompatibleSttUrlNotResponding, probeError),
                    Se.Language.General.TranscriptionError);
                IsTranscribeEnabled = true;
            });
            return;
        }

        ProgressValue = 10;

        var subtitle = new Subtitle();
        var segmentCount = 0;

        try
        {
            var service = transcriber;

            var segmentProgress = new Progress<OpenAiCompatibleSegment>(seg =>
            {
                Interlocked.Increment(ref segmentCount);
                LogToConsole($"  Segment #{segmentCount}: {TimeSpan.FromSeconds(seg.Start):mm\\:ss\\.fff} -> {TimeSpan.FromSeconds(seg.End):mm\\:ss\\.fff}: {seg.Text.Trim()}");
                if (_startTicks > 0)
                {
                    var durationMs = (DateTime.UtcNow.Ticks - _startTicks) / 10_000;
                    ElapsedText = $"Time elapsed: {new TimeCode(durationMs).ToShortDisplayString()}";
                }
                if (_videoInfo.TotalSeconds > 0)
                {
                    SetProgressBarPct(seg.End / _videoInfo.TotalSeconds * 100.0);
                }
                lock (subtitle.Paragraphs)
                {
                    var text = seg.Text.Trim();
                    if (!string.IsNullOrEmpty(text))
                    {
                        subtitle.Paragraphs.Add(new Paragraph(text, seg.Start * 1000.0, seg.End * 1000.0));
                    }
                }
            });

            var audioSizeBytes = new FileInfo(audioFileName).Length;
            if (audioSizeBytes > engine.UploadThresholdBytes && _videoInfo.TotalSeconds > 0)
            {
                LogToConsole($"Audio file is {audioSizeBytes / (1024 * 1024)} MB — splitting into chunks to stay under the upload cap");
                await TranscribeInChunksAsync(service, engine, audioFileName, language, subtitle, segmentProgress, cancellationToken);
            }
            else
            {
                var response = await service.TranscribeAsync(audioFileName, language, null, segmentProgress, cancellationToken);
                IngestTranscriptionResponse(
                    response,
                    subtitle,
                    offsetSeconds: 0.0,
                    chunkEndSeconds: _videoInfo.TotalSeconds,
                    paragraphsBeforeResponse: 0);
            }

            ProgressValue = 90;
            ProgressText = Se.Language.General.ProcessingResponse;

            ProgressValue = 100;
            ProgressText = Se.Language.General.TranscriptionComplete;
            LogToConsole($"Transcription completed: {subtitle.Paragraphs.Count} segment(s)");

            var postProcessedSubtitle = PostProcess(subtitle);

            if (_audioClips != null && ResultAudioClips.Count > 0)
            {
                // Match on _videoFileName (the original clip), NOT audioFileName:
                // for the OpenAI engine audioFileName is a transcoded temp file
                // (e.g. <GUID>.mp3) that never equals a clip's AudioFileName, so
                // matching on it would leave Transcription unset and the selected
                // line empty. Mirrors the whisper paths above.
                var outputAudioClip = ResultAudioClips.FirstOrDefault(p => p.AudioFileName == _videoFileName);
                if (outputAudioClip != null)
                {
                    outputAudioClip.Transcription = new Subtitle(postProcessedSubtitle);
                }
            }

            await Dispatcher.UIThread.InvokeAsync(async () => await MakeResult(postProcessedSubtitle));
        }
        catch (OperationCanceledException)
        {
            LogToConsole("Transcription cancelled by user");
            if (subtitle.Paragraphs.Count > 0)
            {
                LogToConsole($"Returning {subtitle.Paragraphs.Count} partial segment(s)");
                var postProcessedSubtitle = PostProcess(subtitle);

                if (_audioClips != null && ResultAudioClips.Count > 0)
                {
                    // See note above: match the original clip via _videoFileName,
                    // not the transcoded temp file passed as audioFileName.
                    var outputAudioClip = ResultAudioClips.FirstOrDefault(p => p.AudioFileName == _videoFileName);
                    if (outputAudioClip != null)
                    {
                        outputAudioClip.Transcription = new Subtitle(postProcessedSubtitle);
                    }
                }

                await Dispatcher.UIThread.InvokeAsync(async () => await MakeResult(postProcessedSubtitle));
            }
            else
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    IsTranscribeEnabled = true;
                    HideProgressBar();
                });
            }
        }
        catch (HttpRequestException ex)
        {
            // Log the full exception before simplifying the dialog text — the
            // response body (e.g. DashScope's error code) is the only clue to
            // what actually failed, and the dialog may hide it.
            Se.WriteToolsLog($"Online STT transcription failed ({engine.Name}): {ex}");

            var message = ex.Message;
            if (message.Contains("401") || message.Contains("Unauthorized"))
            {
                message = Se.Language.General.UnauthorizedApiKey;
                if (engine is DashScopeQwen3SttEngine)
                {
                    // DashScope keys are region-scoped: a China (Beijing) key is
                    // rejected by the international endpoint and vice versa.
                    message += Environment.NewLine + Environment.NewLine + Se.Language.General.DashScopeSttRegionKeyHint;
                }
            }
            else if (message.Contains("timeout") || message.Contains("timed out"))
            {
                message = Se.Language.General.RequestTimeout;
            }

            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await MessageBox.Show(Window!, message, Se.Language.General.TranscriptionError);
                IsTranscribeEnabled = true;
            });
        }
        catch (TimeoutException ex)
        {
            // Raised by the online STT services when their own timeout fires
            // (distinct from a user cancel, which arrives as
            // OperationCanceledException above).
            Se.WriteToolsLog($"Online STT transcription timed out ({engine.Name}): {ex.Message}");

            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await MessageBox.Show(Window!, Se.Language.General.RequestTimeout, Se.Language.General.TranscriptionError);
                IsTranscribeEnabled = true;
            });

            if (subtitle.Paragraphs.Count > 0)
            {
                LogToConsole($"Timed out - returning {subtitle.Paragraphs.Count} partial segment(s)");
                var postProcessedSubtitle = PostProcess(subtitle);

                if (_audioClips != null && ResultAudioClips.Count > 0)
                {
                    // See note above: match the original clip via _videoFileName,
                    // not the transcoded temp file passed as audioFileName.
                    var outputAudioClip = ResultAudioClips.FirstOrDefault(p => p.AudioFileName == _videoFileName);
                    if (outputAudioClip != null)
                    {
                        outputAudioClip.Transcription = new Subtitle(postProcessedSubtitle);
                    }
                }

                await Dispatcher.UIThread.InvokeAsync(async () => await MakeResult(postProcessedSubtitle));
            }
        }
        catch (Exception ex)
        {
            Se.WriteToolsLog($"Online STT transcription failed ({engine.Name}): {ex}");

            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await MessageBox.Show(Window!, $"{Se.Language.General.TranscriptionFailed}: {ex.Message}", "Error");
                IsTranscribeEnabled = true;
            });
        }
        finally
        {
            _openAiCts?.Dispose();
            _openAiCts = null;
        }
    }

    /// <summary>
    /// Merge one TranscribeAsync response into the running subtitle. If the
    /// streaming progress callback already added the segments for this slice
    /// (paragraph count grew while we were waiting), do nothing — that's the
    /// common case. Otherwise fall back to whatever the non-streaming
    /// response gave us, with absolute timestamps obtained by adding the
    /// slice's offset into the source audio. <paramref name="chunkEndSeconds"/>
    /// is the absolute end time of this slice and is used to span the
    /// text-only fallback paragraph across the chunk's duration; otherwise
    /// chunks after the first would get zero-duration paragraphs.
    /// </summary>
    private static void IngestTranscriptionResponse(
        OpenAiCompatibleSttResponse response,
        Subtitle subtitle,
        double offsetSeconds,
        double chunkEndSeconds,
        int paragraphsBeforeResponse)
    {
        lock (subtitle.Paragraphs)
        {
            if (subtitle.Paragraphs.Count > paragraphsBeforeResponse)
            {
                return;
            }

            if (response.Segments != null && response.Segments.Count > 0)
            {
                foreach (var segment in response.Segments.OrderBy(s => s.Start))
                {
                    var text = segment.Text.Trim();
                    if (!string.IsNullOrEmpty(text))
                    {
                        subtitle.Paragraphs.Add(new Paragraph(
                            text,
                            (segment.Start + offsetSeconds) * 1000.0,
                            (segment.End + offsetSeconds) * 1000.0));
                    }
                }
                return;
            }

            if (!string.IsNullOrEmpty(response.Text))
            {
                // No segment timings (e.g. OpenRouter's Whisper returns only
                // `text`): split into sentences and spread the slice's duration
                // across them by length, so the result isn't one giant cue
                // (issue #12154). Falling back to a historical 5 s window only
                // when we genuinely don't know the end.
                var startMs = offsetSeconds * 1000.0;
                var endMs = chunkEndSeconds > offsetSeconds
                    ? chunkEndSeconds * 1000.0
                    : startMs + 5000.0;
                AddTextAsTimedSentences(subtitle, response.Text.Trim(), startMs, endMs);
            }
        }
    }

    /// <summary>
    /// Split a block of transcript text into sentence-sized paragraphs and
    /// distribute the [<paramref name="startMs"/>, <paramref name="endMs"/>]
    /// window across them proportionally to each sentence's length. Used for
    /// providers that return only recognized text with no per-segment timings.
    /// Language-agnostic: breaks on Latin and CJK sentence punctuation.
    /// </summary>
    internal static void AddTextAsTimedSentences(Subtitle subtitle, string text, double startMs, double endMs)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        var sentences = SplitIntoSentences(text);
        var totalChars = sentences.Sum(s => s.Length);
        if (sentences.Count <= 1 || totalChars == 0 || endMs <= startMs)
        {
            var end = endMs > startMs ? endMs : startMs + 2000.0;
            subtitle.Paragraphs.Add(new Paragraph(text.Trim(), startMs, end));
            return;
        }

        var span = endMs - startMs;
        var cursor = startMs;
        for (var i = 0; i < sentences.Count; i++)
        {
            var duration = span * ((double)sentences[i].Length / totalChars);
            var pEnd = i == sentences.Count - 1 ? endMs : cursor + duration;
            subtitle.Paragraphs.Add(new Paragraph(sentences[i].Trim(), cursor, pEnd));
            cursor = pEnd;
        }
    }

    /// <summary>
    /// Break text into sentences, keeping trailing sentence punctuation. Handles
    /// Latin (. ! ?) and CJK (。！？…) terminators; returns the whole string as a
    /// single element when it has no sentence punctuation.
    /// </summary>
    internal static List<string> SplitIntoSentences(string text)
    {
        var result = new List<string>();
        if (string.IsNullOrWhiteSpace(text))
        {
            return result;
        }

        var matches = Regex.Matches(text.Trim(), @"[^.!?。！？…]*[.!?。！？…]+[""'”’)\]]*\s*|[^.!?。！？…]+$");
        foreach (Match m in matches)
        {
            var sentence = m.Value.Trim();
            if (sentence.Length > 0)
            {
                result.Add(sentence);
            }
        }

        if (result.Count == 0)
        {
            result.Add(text.Trim());
        }

        return result;
    }

    /// <summary>
    /// Split audioFileName into ~23 MB pieces, snap each cut to nearby silence
    /// via ffmpeg silencedetect, then upload each chunk sequentially with
    /// segment timestamps offset back to absolute time. Any chunk failure
    /// aborts the run; partial subtitle so far is preserved by the outer
    /// catch-blocks like the single-file path.
    /// </summary>
    private async Task TranscribeInChunksAsync(
        ISttTranscriber service,
        IOnlineSttEngine engine,
        string audioFileName,
        string? language,
        Subtitle subtitle,
        IProgress<OpenAiCompatibleSegment> segmentProgress,
        CancellationToken cancellationToken)
    {
        var totalSeconds = _videoInfo.TotalSeconds;
        var fileSize = new FileInfo(audioFileName).Length;
        var chunkCount = OpenAiSttChunker.ComputeChunkCount(fileSize, engine.ChunkSizeBytes);

        var ffmpegPath = Se.Settings.General.FfmpegPath;
        if (!File.Exists(ffmpegPath))
        {
            ffmpegPath = "ffmpeg";
        }

        LogToConsole($"Running silencedetect on audio file...");
        var silences = await OpenAiSttChunker.DetectSilenceIntervalsAsync(
            ffmpegPath, audioFileName, cancellationToken: cancellationToken);
        LogToConsole($"  Found {silences.Count} silence interval(s); computing {chunkCount} chunk boundaries");

        var boundaries = OpenAiSttChunker.ComputeAdjustedBoundaries(totalSeconds, chunkCount, silences);
        var extension = Path.GetExtension(audioFileName);

        for (var i = 0; i < boundaries.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var boundary = boundaries[i];
            var chunkPath = Path.Combine(GetSttTempFolder(), $"se-stt-chunk-{Guid.NewGuid()}{extension}");
            // Register before extraction so a throw mid-extract still drains
            // the (possibly partial) file via the outer _filesToDelete sweep.
            _filesToDelete.Add(chunkPath);

            LogToConsole(
                $"Chunk {i + 1}/{boundaries.Count}: " +
                $"{TimeSpan.FromSeconds(boundary.StartSeconds):mm\\:ss} → {TimeSpan.FromSeconds(boundary.EndSeconds):mm\\:ss}");

            try
            {
                var extractOk = await OpenAiSttChunker.ExtractChunkAsync(
                    ffmpegPath, audioFileName, chunkPath,
                    boundary.StartSeconds, boundary.DurationSeconds, cancellationToken);
                if (!extractOk)
                {
                    throw new InvalidOperationException(
                        $"ffmpeg failed to extract chunk {i + 1}/{boundaries.Count} " +
                        $"({boundary.StartSeconds:0.##}s → {boundary.EndSeconds:0.##}s) from {audioFileName}");
                }

                // Wrap the caller's segment progress so streaming segments coming
                // from this chunk get offset back to absolute time before the UI
                // sees them.
                var offsetSeconds = boundary.StartSeconds;
                var offsettingProgress = new Progress<OpenAiCompatibleSegment>(seg =>
                {
                    segmentProgress.Report(new OpenAiCompatibleSegment
                    {
                        Id = seg.Id,
                        Start = seg.Start + offsetSeconds,
                        End = seg.End + offsetSeconds,
                        Text = seg.Text,
                    });
                });

                int paragraphsBeforeChunk;
                lock (subtitle.Paragraphs)
                {
                    paragraphsBeforeChunk = subtitle.Paragraphs.Count;
                }

                var chunkResponse = await service.TranscribeAsync(
                    chunkPath, language, null, offsettingProgress, cancellationToken);

                IngestTranscriptionResponse(
                    chunkResponse,
                    subtitle,
                    offsetSeconds,
                    chunkEndSeconds: boundary.EndSeconds,
                    paragraphsBeforeChunk);
            }
            finally
            {
                // Delete this chunk as soon as we're done with it instead of
                // accumulating up to N×23 MB of WAV in temp for long runs. The
                // entry stays in _filesToDelete for the outer sweep as a
                // safety net in case Delete throws here.
                try { if (File.Exists(chunkPath))
                {
                    File.Delete(chunkPath);
                } } catch { /* swept later */ }
            }
        }
    }

    /// <summary>
    /// Fixes small/small-negative durations in the subtitle by taking time from prevoius subtitle line.
    /// </summary>
    private static void FixNegativeDuration(Subtitle subtitle)
    {
        for (int i = 0; i < subtitle.Paragraphs.Count; i++)
        {
            var paragraph = subtitle.Paragraphs[i];
            if (i > 0 &&
                paragraph.DurationTotalMilliseconds < 5 && paragraph.DurationTotalMilliseconds > -20 && paragraph.StartTime.TotalMilliseconds > 20)
            {
                var prev = subtitle.Paragraphs[i - 1];
                if (prev.DurationTotalMilliseconds < 50)
                {
                    continue;
                }

                paragraph.StartTime.TotalMilliseconds = paragraph.EndTime.TotalMilliseconds - 10;
                if (prev.EndTime.TotalMilliseconds > paragraph.StartTime.TotalMilliseconds)
                {
                    prev.EndTime.TotalMilliseconds = paragraph.StartTime.TotalMilliseconds;
                }
            }
        }
    }

    /// <summary>
    /// Re-inserts periods, exclamation marks, and question marks into the subtitle text based on the original text.
    /// </summary>
    private static void ReInsertPeriodsEtc(string originalText, Subtitle subtitle)
    {
        var words = originalText.Split([' ', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        var wordIndex = 0;
        var consecutiveNoMatch = 0;
        const int maxConsecutiveNoMatch = 10;

        foreach (var p in subtitle.Paragraphs)
        {
            // each paragraph.Text contains one word
            var text = p.Text.Trim();
            if (string.IsNullOrEmpty(text))
            {
                continue;
            }

            if (wordIndex >= words.Length)
            {
                break;
            }

            // Try to find matching word in original text (look ahead a few words in case of slight misalignment)
            var found = false;
            var searchEnd = Math.Min(wordIndex + 5, words.Length);

            for (var i = wordIndex; i < searchEnd; i++)
            {
                var originalWord = words[i];
                var cleanWord = originalWord.TrimEnd('.', '!', '?', ',').ToLowerInvariant();

                if (string.Equals(text, cleanWord, StringComparison.OrdinalIgnoreCase))
                {
                    // Found match - restore punctuation/casing from original word
                    p.Text = originalWord;

                    wordIndex = i + 1;
                    found = true;
                    consecutiveNoMatch = 0;
                    break;
                }
            }

            if (!found)
            {
                consecutiveNoMatch++;
                if (consecutiveNoMatch >= maxConsecutiveNoMatch)
                {
                    // Exit if no words match for a while
                    return;
                }
            }
        }
    }

    private string GetProgressText()
    {
        if (IsBatchMode)
        {
            return string.Format(Se.Language.Video.AudioToText.TranscribingXOfY, _batchIndex + 1, BatchItems.Count);
        }
        else
        {
            return Se.Language.Video.AudioToText.Transcribing;
        }
    }

    private void StartNext(Subtitle? transcribedSubtitle)
    {
        var currentItem = BatchItems[_batchIndex];
        if (transcribedSubtitle != null && transcribedSubtitle.Paragraphs.Count > 0)
        {
            currentItem.Status = Se.Language.General.Converted;
            var subtitleFileName = GetSubtitleFileName(currentItem.InputVideoFileName);
            var format = new SubRip();
            var text = format.ToText(transcribedSubtitle, string.Empty);
            File.WriteAllText(subtitleFileName, text);
            LastBatchSubtitleFileName = subtitleFileName;
        }

        // Delete temp files from the just-finished item so disk usage doesn't grow across long batches
        DeleteTempFiles();
        _filesToDelete.Clear();

        _batchIndex++;
        if (_batchIndex < BatchItems.Count)
        {
            ProgressValue = 0;
            _startTicks = 0;
            _endSeconds = 0; ;
            _showProgressPct = -1;
            _outputText.Clear();
            ConsoleLog = string.Empty;
            ProgressText = string.Empty;
            ElapsedText = string.Empty;
            EstimatedText = string.Empty;

            var jobItem = BatchItems[_batchIndex];
            _videoFileName = jobItem.InputVideoFileName;
            _videoInfo.TotalMilliseconds = jobItem.MediaInfo.Duration.TotalMilliseconds;
            _videoInfo.TotalSeconds = jobItem.MediaInfo.Duration.TotalSeconds;
            _videoInfo.Width = jobItem.MediaInfo.Dimension.Width;
            _videoInfo.Height = jobItem.MediaInfo.Dimension.Height;

            ProgressOpacity = 1;
            ProgressText = Se.Language.General.GeneratingAudioFile;
            _startTicks = DateTime.UtcNow.Ticks;

            Dispatcher.UIThread.Post(() =>
            {
                if (BatchGrid == null)
                {
                    return;
                }

                BatchGrid.SelectedItem = jobItem;
                BatchGrid.ScrollIntoView(jobItem, null);
            });

            var startGenerateAudioFileOk = GenerateAudioFile(_videoFileName, _audioTrackNumber);
            return;
        }

        var convertedJobs = Enumerable.Count<SpeechToTextJobItem>(BatchItems, p => p.Status == Se.Language.General.Converted);
        var failed = Enumerable.Count<SpeechToTextJobItem>(BatchItems, p => p.Status != Se.Language.General.Converted);

        Dispatcher.UIThread.Invoke<Task>(async () =>
        {
            var msg = $"Videos converted: " + convertedJobs;
            if (failed > 0)
            {
                msg += Environment.NewLine + $"Videos failed: " + failed;
            }

            _timerWhisper.Stop();
            await Task.Delay(250);
            HideProgressBar();
            ProgressText = string.Empty;
            EstimatedText = string.Empty;
            ElapsedText = string.Empty;

            if (_audioClips != null && failed == 0)
            {
                OkPressed = true;
                Window?.Close();
                return;
            }

            await MessageBox.Show(
                Window!,
                Se.Language.Video.AudioToText.Title,
                msg,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            IsTranscribeEnabled = true;

            if (failed == 0)
            {
                OkPressed = true;
                Window?.Close();
            }
        });
    }

    private static string GetSubtitleFileName(string videoFileName)
    {
        var path = Path.GetDirectoryName(videoFileName);
        var fileName = Path.GetFileNameWithoutExtension(videoFileName);
        var extension = ".srt";
        var subtitleFileName = Path.Combine(path!, fileName + extension);
        int count = 2;
        while (File.Exists(subtitleFileName))
        {
            subtitleFileName = Path.Combine(path!, fileName + "_" + count + extension);
            count++;
        }

        return subtitleFileName;
    }

    private Subtitle PostProcess(Subtitle transcript)
    {
        var languageCode = SelectedLanguage?.Code;
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            // Online engines null out SelectedLanguage; fall back to their configured
            // language hint so post-processing (merge/split/casing) still runs (issue
            // #12154). With no hint we can't pick language-specific rules safely, so
            // leave the transcript as-is.
            languageCode = GetOnlineEngineLanguageHint();
        }

        if (string.IsNullOrWhiteSpace(languageCode))
        {
            return transcript;
        }

        if (DoAdjustTimings || DoPostProcessing)
        {
            ProgressText = "Post-processing...";
        }

        var postProcessor = new SpeechToTextPostProcessor(DoTranslateToEnglish ? "en" : languageCode)
        {
            ParagraphMaxChars = Configuration.Settings.General.SubtitleLineMaximumLength * 2,
        };

        WavePeakData2? wavePeaks = null;
        if (DoAdjustTimings)
        {
            wavePeaks = MakeWavePeaks();
        }

        if (DoAdjustTimings && wavePeaks != null)
        {
            transcript = SpeechToTextTimingFixer.ShortenLongDuration(transcript);
            transcript = SpeechToTextTimingFixer.ShortenViaWavePeaks(transcript, wavePeaks);
        }

        var settings = Se.Settings.Tools.AudioToText;
        transcript = postProcessor.Fix(
            SpeechToTextPostProcessor.Engine.Whisper,
            transcript,
            DoPostProcessing,
            settings.WhisperPostProcessingAddPeriods,
            settings.WhisperPostProcessingMergeLines,
            settings.WhisperPostProcessingFixCasing,
            settings.WhisperPostProcessingFixShortDuration,
            settings.WhisperPostProcessingSplitLines,
            settings.WhisperPostProcessingChangeUnderlineToColor,
            settings.WhisperPostProcessingChangeUnderlineToColorColor.FromHexToColor()
            );

        return transcript;
    }

    /// <summary>
    /// The language hint configured for the selected online STT engine, or null
    /// for local engines. Online engines don't use the shared language dropdown
    /// (it's nulled out), so post-processing reads the per-engine setting instead.
    /// </summary>
    private string? GetOnlineEngineLanguageHint()
    {
        return GetEffectiveSelectedEngine() switch
        {
            OpenRouterSttEngine => Se.Settings.Tools.OpenRouterSttLanguage,
            DashScopeQwen3SttEngine => Se.Settings.Tools.DashScopeSttLanguage,
            OpenAiCompatibleSttEngine => Se.Settings.Tools.OpenAiCompatibleSttLanguage,
            _ => null,
        };
    }

    private WavePeakData2? MakeWavePeaks()
    {
        if (string.IsNullOrEmpty(_videoFileName) || !File.Exists(_videoFileName))
        {
            return null;
        }

        var targetFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".wav");
        _filesToDelete.Add(targetFile);
        try
        {
            var process = GetFfmpegProcess(_videoFileName, _audioTrackNumber, targetFile);
            if (process == null)
            {
                return null;
            }

#pragma warning disable CA1416
            process.Start();
#pragma warning restore CA1416

            while (!process.HasExited)
            {
                Task.Delay(100);
            }

            // check for delay in matroska files
            var delayInMilliseconds = 0;
            var audioTrackNames = new List<string>();
            var mkvAudioTrackNumbers = new Dictionary<int, int>();
            if (_videoFileName.ToLowerInvariant().EndsWith(".mkv", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    using (var matroska = new MatroskaFile(_videoFileName))
                    {
                        if (matroska.IsValid)
                        {
                            foreach (var track in matroska.GetTracks())
                            {
                                if (track.IsAudio)
                                {
                                    if (track.CodecId != null && track.Language != null)
                                    {
                                        audioTrackNames.Add("#" + track.TrackNumber + ": " +
                                                            track.CodecId.Replace("\0", string.Empty) + " - " +
                                                            track.Language.Replace("\0", string.Empty));
                                    }
                                    else
                                    {
                                        audioTrackNames.Add("#" + track.TrackNumber);
                                    }

                                    mkvAudioTrackNumbers.Add(mkvAudioTrackNumbers.Count, track.TrackNumber);
                                }
                            }

                            if (mkvAudioTrackNumbers.Count > 0)
                            {
                                delayInMilliseconds =
                                    (int)matroska.GetAudioTrackDelayMilliseconds(mkvAudioTrackNumbers[0]);
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    SeLogger.Error(exception, $"Error getting delay from mkv: {_videoFileName}");
                }
            }

            if (File.Exists(targetFile))
            {
                using var waveFile = new WavePeakGenerator2(targetFile);
                if (!string.IsNullOrEmpty(_videoFileName) && File.Exists(_videoFileName))
                {
                    return waveFile.GeneratePeaks(delayInMilliseconds,
                        WavePeakGenerator2.GetPeakWaveFileName(_videoFileName));
                }
            }
        }
        catch
        {
            // ignore
        }

        return null;
    }

    public bool GetResultFromSrt(
        string waveFileName,
        string videoFileName,
        out List<ResultText> resultTexts,
        ConcurrentQueue<string> outputText,
        List<string> filesToDelete)
    {
        Task.Delay(500);

        var engine = GetEffectiveSelectedEngine();

        if (string.IsNullOrEmpty(waveFileName) && videoFileName.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
        {
            waveFileName = videoFileName;
        }

        var jsonFileName = waveFileName + ".json";
        if (File.Exists(jsonFileName))
        {
            var json = File.ReadAllText(jsonFileName);
            var jsonTranscription = WhisperCppJson.GetTranscription(json);
            if (jsonTranscription.Count > 0)
            {
                resultTexts = jsonTranscription.SelectMany(s => s.ToUnderlineActiveWords().Select(p => new ResultText
                {
                    Start = (decimal)p.StartTime.TotalSeconds,
                    End = (decimal)p.EndTime.TotalSeconds,
                    Text = p.Text
                        .Replace("<u> ", " <u>")
                        .Replace(" </u>", "</u> "),
                })).ToList();

                filesToDelete?.Add(jsonFileName);
                return true;
            }
        }

        var whisperFolder = engine.GetAndCreateWhisperFolder();
        var srtCandidates = GetResultFileCandidates(".srt", waveFileName, videoFileName, whisperFolder, outputText);
        var vttCandidates = GetResultFileCandidates(".vtt", waveFileName, videoFileName, whisperFolder, outputText);
        var assaCandidates = GetResultFileCandidates(".ass", waveFileName, videoFileName, whisperFolder, outputText);

        var srtFileName = srtCandidates.FirstOrDefault(File.Exists);
        var vttFileName = vttCandidates.FirstOrDefault(File.Exists);
        var assaFileName = assaCandidates.FirstOrDefault(File.Exists);

        if (string.IsNullOrEmpty(srtFileName) && string.IsNullOrEmpty(vttFileName))
        {
            resultTexts = new List<ResultText>();
            return false;
        }

        var sub = new Subtitle();

        if (File.Exists(srtFileName))
        {
            var rawText = FileUtil.ReadAllLinesShared(srtFileName, Encoding.UTF8);
            new SubRip().LoadSubtitle(sub, rawText, srtFileName);
            outputText?.Enqueue($"Loading result from {srtFileName}");
        }
        else if (File.Exists(vttFileName))
        {
            var rawText = FileUtil.ReadAllLinesShared(vttFileName, Encoding.UTF8);
            new WebVTT().LoadSubtitle(sub, rawText, vttFileName);
            outputText?.Enqueue($"Loading result from {vttFileName}");
        }

        sub.RemoveEmptyLines();

        resultTexts = sub.Paragraphs.Select(p => new ResultText
        {
            Start = (decimal)p.StartTime.TotalSeconds,
            End = (decimal)p.EndTime.TotalSeconds,
            Text = p.Text
        }).ToList();

        if (!string.IsNullOrEmpty(srtFileName))
        {
            filesToDelete?.Add(srtFileName);
        }
        if (!string.IsNullOrEmpty(vttFileName))
        {
            filesToDelete?.Add(vttFileName);
        }
        if (!string.IsNullOrEmpty(assaFileName))
        {
            filesToDelete?.Add(assaFileName);
        }

        return true;
    }

    private static List<string> GetResultFileCandidates(string ext, string waveFileName, string videoFileName, string whisperFolder, ConcurrentQueue<string> outputText)
    {
        var candidates = new List<string>
        {
            waveFileName + ext,
            Path.Combine(Directory.GetCurrentDirectory(), Path.GetFileNameWithoutExtension(videoFileName) + ext),
            Path.Combine(Directory.GetCurrentDirectory(), Path.GetFileNameWithoutExtension(waveFileName) + ext),
            Path.Combine(AppContext.BaseDirectory, Path.GetFileNameWithoutExtension(videoFileName) + ext),
            Path.Combine(AppContext.BaseDirectory, Path.GetFileNameWithoutExtension(waveFileName) + ext),
            Path.Combine(Se.DataFolder, Path.GetFileNameWithoutExtension(videoFileName) + ext),
            Path.Combine(Se.DataFolder, Path.GetFileNameWithoutExtension(waveFileName) + ext),
        };

        if (waveFileName.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
        {
            candidates.Add(waveFileName.Remove(waveFileName.Length - 4) + ext);
        }

        if (!string.IsNullOrEmpty(whisperFolder))
        {
            if (!string.IsNullOrEmpty(videoFileName))
            {
                candidates.Add(Path.Combine(whisperFolder, Path.GetFileNameWithoutExtension(videoFileName) + ext));
            }

            candidates.Add(Path.Combine(whisperFolder, Path.GetFileNameWithoutExtension(waveFileName) + ext));
        }

        var pathFromOutput = TryFindFilePathInOutput(ext.Trim('.'), outputText);
        if (!string.IsNullOrEmpty(pathFromOutput))
        {
            candidates.Insert(0, pathFromOutput);
        }

        return candidates;
    }

    private static string? TryFindFilePathInOutput(string format, ConcurrentQueue<string> outputText)
    {
        string findText = "output_" + format + ": saving output to";
        foreach (var line in outputText)
        {
            if (line.Contains(findText, StringComparison.OrdinalIgnoreCase))
            {
                var filePath = line.Substring(line.IndexOf(findText, StringComparison.OrdinalIgnoreCase) + findText.Length + 1)
                    .Trim('"', ' ', '\'', '\r', '\n');

                if (File.Exists(filePath))
                {
                    return filePath;
                }
            }
        }

        return null;
    }

    private void ShowProgressBar()
    {
        if (ProgressOpacity == 0)
        {
            ProgressValue = 0;
            ProgressOpacity = 1;
        }
    }

    private void HideProgressBar()
    {
        if (ProgressOpacity > 0)
        {
            ProgressValue = 0;
            ProgressOpacity = 0;
        }
    }

    private void SetProgressBarPct(double pct)
    {
        if (pct > 100)
        {
            pct = 100;
        }

        if (pct < 0)
        {
            pct = 0;
        }

        if (pct > ProgressValue)
        {
            ProgressValue = pct;
        }
        // _taskbarList.SetProgressValue(_windowHandle, Math.Max(0, Math.Min((int)pct, 100)), 100);
    }

    private async Task MakeResult(Subtitle? transcribedSubtitle)
    {
        // Small delay to ensure all output is captured and flushed
        await Task.Delay(100);

        var sbLog = new StringBuilder();
        foreach (var s in _outputText)
        {
            sbLog.AppendLine(s.TrimEnd());
        }

        Se.WriteToolsLog(sbLog.ToString().Trim());

        var anyLinesTranscribed = transcribedSubtitle != null && transcribedSubtitle.Paragraphs.Count > 0;

        if (IsBatchMode)
        {
            StartNext(transcribedSubtitle);
            return;
        }
        else if (_abort)
        {
            // User cancelled mid-run. Leave the dialog open so they can adjust
            // settings and retry (or close it themselves) instead of yanking it
            // out from under them.
            IsTranscribeEnabled = true;
            HideProgressBar();
        }
        else
        {
            var settings = Se.Settings.Tools.AudioToText;
            IsTranscribeEnabled = true;
            HideProgressBar();

            if (_loadedFromStdOut)
            {
                await MessageBox.Show(Window!, "No result file",
                    "No result file was generated by Whisper, but some text was captured from standard output.");

                if (Window != null)
                {
                    FileHelper.OpenFileWithDefaultProgram(Se.GetToolsLogFilePath());
                }

                OkPressed = anyLinesTranscribed;
                TranscribedSubtitle = transcribedSubtitle ?? new Subtitle();
                Window?.Close();
            }
            else if (anyLinesTranscribed)
            {
                OkPressed = anyLinesTranscribed;
                TranscribedSubtitle = transcribedSubtitle ?? new Subtitle();
                Window?.Close();
            }
            else if (GetEffectiveSelectedEngine() is ICrispAsrEngine)
            {
                await MessageBox.Show(Window!, "No transcription result",
                    "Crisp ASR finished without generating subtitles. Please check the tools log for engine output.");

                if (Window != null)
                {
                    FileHelper.OpenFileWithDefaultProgram(Se.GetToolsLogFilePath());
                }
            }
        }
    }

    private void OnTimerAudioExtractOnElapsed(object? sender, ElapsedEventArgs e)
    {
        lock (_lockObj)
        {
            if (_audioExtractProcess == null)
            {
                return;
            }

            if (_abort)
            {
                _timerAudioExtract.Stop();

#pragma warning disable CA1416
                _audioExtractProcess.Kill(true);
#pragma warning restore CA1416

                ProgressOpacity = 0;
                IsTranscribeEnabled = true;
                return;
            }

            if (!_audioExtractProcess.HasExited)
            {
                var durationMs = (DateTime.UtcNow.Ticks - _startTicks) / 10_000;
                ElapsedText = $"Time elapsed: {new TimeCode(durationMs).ToShortDisplayString()}";

                return;
            }

            _timerAudioExtract.Stop();

            if (!File.Exists(_audioFileName))
            {
                Se.WriteToolsLog("Generated audio file not found: " + _audioFileName + Environment.NewLine +
                                     "ffmpeg: " + _audioExtractProcess.StartInfo.FileName + Environment.NewLine +
                                     "Parameters: " + _audioExtractProcess.StartInfo.Arguments + Environment.NewLine +
                                     "OS: " + Environment.OSVersion + Environment.NewLine +
                                     "64-bit: " + Environment.Is64BitOperatingSystem + Environment.NewLine +
                                     "ffmpeg exit code: " + _audioExtractProcess.ExitCode + Environment.NewLine +
                                     "ffmpeg log: " + _ffmpegLog);
                IsTranscribeEnabled = true;
                _audioExtractProcess = null;
                return;
            }

            _audioExtractProcess = null;
            if (string.IsNullOrEmpty(_videoFileName))
            {
                IsTranscribeEnabled = true;
                Dispatcher.UIThread.Invoke(async () =>
                {
                    await MessageBox.Show(Window!, "No video file", "No video file found!");
                });

                return;
            }

            var startOk = TranscribeViaWhisper(_audioFileName, _videoFileName);
            if (!startOk)
            {
                IsTranscribeEnabled = true;
                Dispatcher.UIThread.Invoke(async () =>
                {
                    if (string.IsNullOrEmpty(_error))
                    {
                        await MessageBox.Show(Window!, "Unknown error", "Unable to start Whisper!");
                    }
                    else
                    {
                        await MessageBox.Show(Window!, "Error", "Unable to start Whisper: " + _error);
                    }
                });
            }
        }
    }

    /// <summary>
    /// Returns the dedicated temp subfolder for the current speech-to-text run,
    /// creating it on first use. The extracted audio (and any chunk files) live
    /// here so the whole folder - including engine output and stray .tmp files
    /// written next to the input - can be removed in one go (#11837).
    /// </summary>
    private string GetSttTempFolder()
    {
        if (string.IsNullOrEmpty(_sttTempFolder))
        {
            _sttTempFolder = Path.Combine(Path.GetTempPath(), "se-stt-" + Guid.NewGuid());
            Directory.CreateDirectory(_sttTempFolder);
        }

        return _sttTempFolder;
    }

    public void DeleteTempFiles()
    {
        foreach (var file in _filesToDelete)
        {
            try
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
            catch
            {
                // ignore
            }
        }

        if (!string.IsNullOrEmpty(_sttTempFolder))
        {
            try
            {
                if (Directory.Exists(_sttTempFolder))
                {
                    Directory.Delete(_sttTempFolder, true);
                }
            }
            catch
            {
                // ignore
            }

            _sttTempFolder = null;
        }
    }

    private static bool IsModelEnglishOnly(WhisperModel model)
    {
        return model.Name.EndsWith(".en", StringComparison.InvariantCulture) ||
               model.Name == "distil-large-v2" ||
               model.Name == "distil-large-v3";
    }



    [RelayCommand]
    private void ShowWebLink()
    {
        if (Window == null)
        {
            return;
        }

        var engine = GetEffectiveSelectedEngine();
        UiUtil.OpenUrl(engine.Url);
    }

    [RelayCommand]
    private void ViewToolsLogFile()
    {
        var logFilePath = Se.GetToolsLogFilePath();
        if (Window != null)
        {
            FileHelper.OpenFileWithDefaultProgram(logFilePath);
        }
    }

    [RelayCommand]
    private async Task ReDownloadWhisperEngine()
    {
        if (Window == null)
        {
            return;
        }

        var engine = GetEffectiveSelectedEngine();
        var crispVariant = "vulkan";
        if (engine is ICrispAsrEngine && Configuration.IsRunningOnWindows)
        {
            var answer = await MessageBox.Show(
                Window,
                $"Download {CrispAsrEngine.StaticName}?",
                $"{Environment.NewLine}\"{CrispAsrEngine.StaticName}\" requires downloading the CrispASR engine.{Environment.NewLine}{Environment.NewLine}Select a version to download:",
                MessageBoxButtons.Cancel,
                MessageBoxIcon.Question,
                "CPU",
                "Vulkan",
                "CUDA");

            if (answer == MessageBoxResult.None || answer == MessageBoxResult.Cancel)
            {
                return;
            }

            crispVariant = answer switch
            {
                MessageBoxResult.Custom1 => "cpu",
                MessageBoxResult.Custom3 => "cuda",
                _ => "vulkan",
            };

            if (crispVariant == "cpu")
            {
                var cpuAnswer = await PromptCrispAsrCpuFlavorAsync();
                if (cpuAnswer == null)
                {
                    return;
                }
                crispVariant = cpuAnswer;
            }

            if (crispVariant == "vulkan" && !VulkanHelper.IsInstalled())
            {
                var vulkanAnswer = await MessageBox.Show(
                    Window,
                    "Vulkan SDK may be required",
                    $"The Vulkan version requires the Vulkan SDK to be installed.{Environment.NewLine}{Environment.NewLine}You can download it from:{Environment.NewLine}https://vulkan.lunarg.com/sdk/home{Environment.NewLine}{Environment.NewLine}Continue with Vulkan download?",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (vulkanAnswer == MessageBoxResult.No)
                {
                    UiUtil.OpenUrl("https://vulkan.lunarg.com/sdk/home");
                    return;
                }

                if (vulkanAnswer != MessageBoxResult.Yes)
                {
                    return;
                }
            }
        }
        else if (engine is ICrispAsrEngine
                 && RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                 && RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
        {
            var linuxAnswer = await PromptCrispAsrLinuxVariantAsync(CrispAsrEngine.StaticName);
            if (linuxAnswer == null)
            {
                return;
            }
            crispVariant = linuxAnswer;
        }

        var qwen3UseVulkan = false;
        if (engine is Qwen3AsrCppEngine && Qwen3AsrCppDownloadService.IsVulkanBuildAvailable())
        {
            var pick = await PromptQwen3AsrGpuAsync(engine.Name);
            if (pick == null)
            {
                return;
            }

            qwen3UseVulkan = pick.Value;
        }

        await _windowService.ShowDialogAsync<DownloadSpeechToTextEngineWindow, DownloadSpeechToTextEngineViewModel>(
            Window, viewModel =>
            {
                viewModel.Engine = engine;
                viewModel.CrispAsrWindowsVariant = crispVariant;
                viewModel.Qwen3AsrUseVulkan = qwen3UseVulkan;
                viewModel.StartDownload();
            });

        RefreshEngineCombo?.Invoke();
    }

    /// <summary>
    /// Qwen3 ASR build prompt (win64 / linux-x64, where a Vulkan build exists): CPU vs GPU (Vulkan).
    /// Returns true for the Vulkan build, false for CPU, or null when the user cancels.
    /// </summary>
    private async Task<bool?> PromptQwen3AsrGpuAsync(string engineName)
    {
        var answer = await MessageBox.Show(
            Window!,
            $"Download {engineName}?",
            $"{Environment.NewLine}\"{engineName}\" requires downloading the engine.{Environment.NewLine}{Environment.NewLine}Select a version to download:",
            MessageBoxButtons.Cancel,
            MessageBoxIcon.Question,
            "CPU",
            "GPU (Vulkan)");

        if (answer == MessageBoxResult.None || answer == MessageBoxResult.Cancel)
        {
            return null;
        }

        var useVulkan = answer == MessageBoxResult.Custom2;
        if (useVulkan && !VulkanHelper.IsInstalled())
        {
            var vulkanAnswer = await MessageBox.Show(
                Window!,
                "Vulkan may be required",
                $"The GPU (Vulkan) build needs a Vulkan-capable GPU and runtime.{Environment.NewLine}{Environment.NewLine}You can get the Vulkan SDK from:{Environment.NewLine}https://vulkan.lunarg.com/sdk/home{Environment.NewLine}{Environment.NewLine}Continue with the GPU download?",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (vulkanAnswer == MessageBoxResult.No)
            {
                UiUtil.OpenUrl("https://vulkan.lunarg.com/sdk/home");
                return null;
            }

            if (vulkanAnswer != MessageBoxResult.Yes)
            {
                return null;
            }
        }

        return useVulkan;
    }

    /// <summary>
    /// Linux x86_64 build prompt: CPU vs CUDA. Returns "cuda", empty string for the default CPU build,
    /// or null when the user cancels.
    /// </summary>
    private async Task<string?> PromptCrispAsrLinuxVariantAsync(string engineName)
    {
        var answer = await MessageBox.Show(
            Window!,
            $"Download {engineName}?",
            $"{Environment.NewLine}\"{engineName}\" requires downloading the CrispASR engine.{Environment.NewLine}{Environment.NewLine}Select a version to download:",
            MessageBoxButtons.Cancel,
            MessageBoxIcon.Question,
            "CPU",
            "CUDA");

        return answer switch
        {
            MessageBoxResult.Custom1 => string.Empty,
            MessageBoxResult.Custom2 => "cuda",
            _ => null,
        };
    }

    /// <summary>
    /// Follow-up prompt after the user picks "CPU" in the CrispASR variant selector.
    /// Returns "cpu" (modern, recommended), "cpu-legacy" (compatibility build for CPUs without AVX2),
    /// or null when the user cancels.
    /// </summary>
    private async Task<string?> PromptCrispAsrCpuFlavorAsync()
    {
        var cpuAnswer = await MessageBox.Show(
            Window!,
            "CrispASR CPU build",
            $"{Environment.NewLine}Standard is recommended for most machines.{Environment.NewLine}{Environment.NewLine}Legacy is a fallback for older CPUs without AVX2 support.",
            MessageBoxButtons.Cancel,
            MessageBoxIcon.Question,
            "Standard",
            "Legacy");

        return cpuAnswer switch
        {
            MessageBoxResult.Custom1 => "cpu",
            MessageBoxResult.Custom2 => "cpu-legacy",
            _ => null,
        };
    }

    [RelayCommand]
    private void SingleMode()
    {
        IsBatchMode = false;
        IsSingleModeVisible = false;
        IsBatchModeVisible = true;
    }

    [RelayCommand]
    private void BatchMode()
    {
        IsBatchMode = true;
        IsSingleModeVisible = true;
        IsBatchModeVisible = false;
    }

    [RelayCommand]
    private async Task CopyConsoleLog()
    {
        if (Window == null || string.IsNullOrEmpty(ConsoleLog))
        {
            return;
        }

        await ClipboardHelper.SetTextAsync(Window, ConsoleLog);

        if (CopyConsoleLogButton != null)
        {
            Attached.SetIcon(CopyConsoleLogButton, IconNames.Check);
            await Task.Delay(1500);
            Attached.SetIcon(CopyConsoleLogButton, IconNames.Copy);
        }
    }

    [RelayCommand]
    private async Task Add()
    {
        var fileNames = await _fileHelper.PickOpenVideoFiles(Window!, Se.Language.General.AddVideoFiles);
        if (fileNames.Length == 0 || Window == null)
        {
            return;
        }

        var error = await AddFiles(fileNames);

        if (error)
        {
            await MessageBox.Show(Window!,
                 "Unable to get video info",
                 "File skipped as video info was unavailable",
             MessageBoxButtons.OK,
             MessageBoxIcon.Error);
        }
    }

    private async Task<bool> AddFiles(string[] fileNames)
    {
        var error = false;
        if (Window == null)
        {
            return false;
        }

        Window.Cursor = new Cursor(StandardCursorType.Wait);

        try
        {
            // Process files on background thread
            await Task.Run((Func<Task?>)(async () =>
            {
                foreach (var fileName in fileNames)
                {
                    var mediaInfo = FfmpegMediaInfo.Parse(fileName);
                    if (mediaInfo.Duration == null || mediaInfo.Duration.TotalMilliseconds < 1)
                    {
                        error = true;
                    }
                    else
                    {
                        var batchItem = new SpeechToTextJobItem(fileName, string.Empty, mediaInfo);
                        await Dispatcher.UIThread.InvokeAsync((Action)(() => BatchItems.Add(batchItem)));
                    }
                }
            }));
        }
        finally
        {
            Window.Cursor = new Cursor(StandardCursorType.Arrow);
        }

        return error;
    }

    [RelayCommand]
    private void Remove()
    {
        if (SelectedBatchItem != null)
        {
            var idx = BatchItems.IndexOf(SelectedBatchItem);
            BatchItems.Remove(SelectedBatchItem);
        }
    }

    [RelayCommand]
    private void Clear()
    {
        BatchItems.Clear();
    }

    [RelayCommand]
    private async Task ShowAdvancedSettings()
    {
        var vm = await _windowService.ShowDialogAsync<SpeechToTextAdvancedWindow, SpeechToTextAdvancedViewModel>(Window!,
            viewModal =>
            {
                viewModal.Engines = Enumerable.ToList<ISpeechToTextEngine>((IEnumerable<ISpeechToTextEngine>)Engines);
                viewModal.EngineClickedCommand.Execute((ISpeechToTextEngine)SelectedEngine);
            });

        if (vm.OkPressed)
        {
            Parameters = GetEffectiveSelectedEngine().CommandLineParameter;
        }
    }

    [RelayCommand]
    private async Task ShowPostProcessingSettings()
    {
        var vm = await _windowService.ShowDialogAsync<SpeechToTextPostProcessingWindow, SpeechToTextPostProcessingViewModel>(
            Window!, viewModal =>
            {
                viewModal.AdjustTimings = Se.Settings.Tools.AudioToText.WhisperAutoAdjustTimings;
                viewModal.FixShortDuration = Se.Settings.Tools.AudioToText.WhisperPostProcessingFixShortDuration;
                viewModal.FixCasing = Se.Settings.Tools.AudioToText.WhisperPostProcessingFixCasing;
                viewModal.AddPeriods = Se.Settings.Tools.AudioToText.WhisperPostProcessingAddPeriods;
                viewModal.MergeShortLines = Se.Settings.Tools.AudioToText.WhisperPostProcessingMergeLines;
                viewModal.BreakSplitLongLines = Se.Settings.Tools.AudioToText.WhisperPostProcessingSplitLines;
                viewModal.ChangeUnderlineToColor = Se.Settings.Tools.AudioToText.WhisperPostProcessingChangeUnderlineToColor;
                viewModal.ChangeUnderlineToColorColor = Se.Settings.Tools.AudioToText.WhisperPostProcessingChangeUnderlineToColorColor.FromHexToColor();
                viewModal.CueRebuild = Se.Settings.Tools.AudioToText.WhisperCueRebuild;
                viewModal.CueMaxChars = Se.Settings.Tools.AudioToText.WhisperCueMaxChars;
                viewModal.CueMaxSeconds = Se.Settings.Tools.AudioToText.WhisperCueMaxSeconds;
                viewModal.CueMaxCps = Se.Settings.Tools.AudioToText.WhisperCueMaxCps;
                viewModal.VocabularyPrompt = Se.Settings.Tools.AudioToText.WhisperVocabularyPrompt;
            });

        if (vm.OkPressed)
        {
            DoAdjustTimings = vm.AdjustTimings;
            Se.Settings.Tools.AudioToText.WhisperAutoAdjustTimings = vm.AdjustTimings;
            Se.Settings.Tools.AudioToText.WhisperPostProcessingFixShortDuration = vm.FixShortDuration;
            Se.Settings.Tools.AudioToText.WhisperPostProcessingFixCasing = vm.FixCasing;
            Se.Settings.Tools.AudioToText.WhisperPostProcessingAddPeriods = vm.AddPeriods;
            Se.Settings.Tools.AudioToText.WhisperPostProcessingMergeLines = vm.MergeShortLines;
            Se.Settings.Tools.AudioToText.WhisperPostProcessingSplitLines = vm.BreakSplitLongLines;
            Se.Settings.Tools.AudioToText.WhisperPostProcessingChangeUnderlineToColor = vm.ChangeUnderlineToColor;
            Se.Settings.Tools.AudioToText.WhisperPostProcessingChangeUnderlineToColorColor = vm.ChangeUnderlineToColorColor.FromColorToHex();
            Se.Settings.Tools.AudioToText.WhisperCueRebuild = vm.CueRebuild;
            Se.Settings.Tools.AudioToText.WhisperCueMaxChars = vm.CueMaxChars;
            Se.Settings.Tools.AudioToText.WhisperCueMaxSeconds = vm.CueMaxSeconds;
            Se.Settings.Tools.AudioToText.WhisperCueMaxCps = vm.CueMaxCps;
            Se.Settings.Tools.AudioToText.WhisperVocabularyPrompt = vm.VocabularyPrompt ?? string.Empty;
        }
    }


    [RelayCommand]
    private async Task DownloadModel()
    {
        var vm = await _windowService.ShowDialogAsync<DownloadSpeechToTextModelsWindow, DownloadSpeechToTextModelsViewModel>(
            Window!, viewModel => { viewModel.SetModels(Models, GetEffectiveSelectedEngine(), SelectedModel); });

        if (vm.OkPressed)
        {
            RefreshDownloadStatus(vm.SelectedModel?.Model);
        }
    }

    [RelayCommand]
    private async Task DownloadForcedAligner()
    {
        if (Window == null)
        {
            return;
        }

        var engine = GetEffectiveSelectedEngine();
        if (engine is not ICrispAsrEngine)
        {
            return;
        }

        var displays = new ObservableCollection<SpeechToTextModelDisplay>();
        foreach (var aligner in ForcedAlignerOption.All())
        {
            if (aligner.IsBuiltIn)
            {
                continue;
            }

            displays.Add(new SpeechToTextModelDisplay
            {
                Model = aligner.ToWhisperModel(),
                Engine = engine,
            });
        }

        if (displays.Count == 0)
        {
            return;
        }

        SpeechToTextModelDisplay? preSelected = null;
        if (SelectedForcedAligner != null && !SelectedForcedAligner.IsBuiltIn)
        {
            preSelected = displays.FirstOrDefault(d => d.Model.Name == SelectedForcedAligner.FileName);
        }

        preSelected ??= displays[0];

        DownloadSpeechToTextModelsViewModel? downloadViewModel = null;
        await _windowService.ShowDialogAsync<DownloadSpeechToTextModelsWindow, DownloadSpeechToTextModelsViewModel>(
            Window, viewModel =>
            {
                downloadViewModel = viewModel;
                viewModel.SetModels(displays, engine, preSelected);
            });

        UpdateForcedAlignerUi();

        // After a successful download, switch the combo to the model that was just downloaded
        // instead of leaving it on the engine's default aligner.
        if (downloadViewModel is { OkPressed: true, SelectedModel.Model.Name: { } downloadedFileName })
        {
            var downloaded = ForcedAligners.FirstOrDefault(a => a.FileName == downloadedFileName);
            if (downloaded != null)
            {
                SelectedForcedAligner = downloaded;
            }
        }
    }

    private static string GetForcedAlignerPath(ICrispAsrEngine crispEngine, ForcedAlignerOption aligner)
    {
        if (aligner.IsBuiltIn || string.IsNullOrEmpty(aligner.FileName) || crispEngine is not CrispAsrEngineBase baseEngine)
        {
            return string.Empty;
        }

        return baseEngine.GetModelForCmdLine(aligner.FileName);
    }

    [RelayCommand]
    private async Task Transcribe()
    {
        if (IsBatchMode && BatchItems.Count == 0)
        {
            await Add();

            if (IsBatchMode && BatchItems.Count == 0)
            {
                return;
            }
        }

        if (IsBatchMode && BatchItems.Count > 0)
        {
            _videoFileName = BatchItems[0].InputVideoFileName;
        }

        if (string.IsNullOrEmpty(_videoFileName))
        {
            return;
        }

        var engine = GetEffectiveSelectedEngine();

        if (engine is not IOnlineSttEngine)
        {
            if (SelectedModel is not SpeechToTextModelDisplay model)
            {
                return;
            }

            if (SelectedLanguage is not WhisperLanguage language)
            {
                return;
            }

            _unknownArgument = false;
            _cudaOutOfMemory = false;
            _incompleteModel = false;
            _loadedFromStdOut = false;

            Se.Settings.Tools.AudioToText.WhisperChoice = engine.Choice;

            if (!engine.IsEngineInstalled())
            {
                if (engine is MlxWhisperMac)
                {
                    // pip-managed engine - Subtitle Edit cannot download it.
                    await MessageBox.Show(
                        Window!,
                        $"{engine.Name} not found",
                        "mlx-whisper not found - install it with: pip3 install mlx-whisper");
                    return;
                }

                if (engine is FasterWhisperMac)
                {
                    // pip-managed engine - Subtitle Edit cannot download it.
                    await MessageBox.Show(
                        Window!,
                        $"{engine.Name} not found",
                        "faster-whisper not found - install it with: pip3 install faster-whisper");
                    return;
                }

                if (engine is ICrispAsrEngine && Configuration.IsRunningOnWindows)
                {
                    var answer = await MessageBox.Show(
                        Window!,
                        $"Download {engine.Name}?",
                        $"{Environment.NewLine}\"{engine.Name}\" requires downloading the CrispASR engine.{Environment.NewLine}{Environment.NewLine}Select a version to download:",
                        MessageBoxButtons.Cancel,
                        MessageBoxIcon.Question,
                        "CPU",
                        "Vulkan",
                        "CUDA");

                    if (answer == MessageBoxResult.None || answer == MessageBoxResult.Cancel)
                    {
                        return;
                    }

                    var crispVariant = answer switch
                    {
                        MessageBoxResult.Custom1 => "cpu",
                        MessageBoxResult.Custom3 => "cuda",
                        _ => "vulkan",
                    };

                    if (crispVariant == "cpu")
                    {
                        var cpuAnswer = await PromptCrispAsrCpuFlavorAsync();
                        if (cpuAnswer == null)
                        {
                            return;
                        }
                        crispVariant = cpuAnswer;
                    }

                    if (crispVariant == "vulkan" && !VulkanHelper.IsInstalled())
                    {
                        var vulkanAnswer = await MessageBox.Show(
                            Window!,
                            "Vulkan SDK may be required",
                            $"The Vulkan version requires the Vulkan SDK to be installed.{Environment.NewLine}{Environment.NewLine}You can download it from:{Environment.NewLine}https://vulkan.lunarg.com/sdk/home{Environment.NewLine}{Environment.NewLine}Continue with Vulkan download?",
                            MessageBoxButtons.YesNoCancel,
                            MessageBoxIcon.Question);

                        if (vulkanAnswer == MessageBoxResult.No)
                        {
                            UiUtil.OpenUrl("https://vulkan.lunarg.com/sdk/home");
                            return;
                        }

                        if (vulkanAnswer != MessageBoxResult.Yes)
                        {
                            return;
                        }
                    }

                    var crispVm = await _windowService.ShowDialogAsync<DownloadSpeechToTextEngineWindow, DownloadSpeechToTextEngineViewModel>(
                        Window!, viewModel =>
                        {
                            viewModel.Engine = engine;
                            viewModel.CrispAsrWindowsVariant = crispVariant;
                            viewModel.StartDownload();
                        });

                    if (!crispVm.OkPressed)
                    {
                        return;
                    }
                }
                else if (engine is ICrispAsrEngine
                         && RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                         && RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
                {
                    var crispVariant = await PromptCrispAsrLinuxVariantAsync(engine.Name);
                    if (crispVariant == null)
                    {
                        return;
                    }

                    var crispVm = await _windowService.ShowDialogAsync<DownloadSpeechToTextEngineWindow, DownloadSpeechToTextEngineViewModel>(
                        Window!, viewModel =>
                        {
                            viewModel.Engine = engine;
                            viewModel.CrispAsrWindowsVariant = crispVariant;
                            viewModel.StartDownload();
                        });

                    if (!crispVm.OkPressed)
                    {
                        return;
                    }
                }
                else
                {
                    var qwen3UseVulkan = false;
                    if (engine is Qwen3AsrCppEngine && Qwen3AsrCppDownloadService.IsVulkanBuildAvailable())
                    {
                        // Qwen3 ASR has a Vulkan (GPU) build on win64/linux-x64 — let the user choose.
                        var pick = await PromptQwen3AsrGpuAsync(engine.Name);
                        if (pick == null)
                        {
                            return;
                        }

                        qwen3UseVulkan = pick.Value;
                    }
                    else
                    {
                        var answer = await MessageBox.Show(
                            Window!,
                            $"Download {engine.Name}?",
                            $"Download and use {engine.Name}?",
                            MessageBoxButtons.YesNoCancel,
                            MessageBoxIcon.Question);

                        if (answer != MessageBoxResult.Yes)
                        {
                            return;
                        }
                    }

                    var vm = await _windowService.ShowDialogAsync<DownloadSpeechToTextEngineWindow, DownloadSpeechToTextEngineViewModel>(
                        Window!, viewModel =>
                        {
                            viewModel.Engine = engine;
                            viewModel.Qwen3AsrUseVulkan = qwen3UseVulkan;
                            viewModel.StartDownload();
                        });

                    if (!vm.OkPressed)
                    {
                        return;
                    }
                }

                RefreshEngineCombo?.Invoke();
            }

            if (!engine.IsModelInstalled(model.Model))
            {
                var answer = await MessageBox.Show(
                    Window!,
                    $"Download {model}?",
                    $"Download and use {model.Model.Name}?",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (answer != MessageBoxResult.Yes)
                {
                    return;
                }

                var vm = await _windowService.ShowDialogAsync<DownloadSpeechToTextModelsWindow, DownloadSpeechToTextModelsViewModel>(
                    Window!, viewModel =>
                    {
                        viewModel.SetModels(Models, engine, SelectedModel);
                        viewModel.StartDownload();
                    });

                RefreshDownloadStatus(vm.SelectedModel?.Model as WhisperModel);
            }

            if (engine is ChatLlmCppEngine chatLlm)
            {
                var modelAligner = chatLlm.ForcedAlignerModel;
                var displayModelAligner = new SpeechToTextModelDisplay
                {
                    Model = modelAligner,
                    Display = modelAligner.Name + " (forced aligner for timestamps)",
                    Engine = engine,
                };
                if (!engine.IsModelInstalled(modelAligner))
                {
                    var answer = await MessageBox.Show(
                                    Window!,
                                    $"Download {modelAligner}?",
                                    $"'Chat LLM' requires a forced aligner to create timestamps.\nDownload and use {modelAligner.Name}?",
                                    MessageBoxButtons.YesNoCancel,
                                    MessageBoxIcon.Question);

                    if (answer != MessageBoxResult.Yes)
                    {
                        return;
                    }

                    var models = new ObservableCollection<SpeechToTextModelDisplay>
                {
                    displayModelAligner
                };
                    var vm = await _windowService.ShowDialogAsync<DownloadSpeechToTextModelsWindow, DownloadSpeechToTextModelsViewModel>(
                        Window!, viewModel =>
                        {
                            viewModel.SetModels(models, engine, displayModelAligner);
                            viewModel.StartDownload();
                        });

                    if (!vm.OkPressed)
                    {
                        return;
                    }
                }
            }

            if (engine is Qwen3AsrCppEngine qwen3Asr)
            {
                var modelAligner = qwen3Asr.ForcedAlignerModel;
                var displayModelAligner = new SpeechToTextModelDisplay
                {
                    Model = modelAligner,
                    Display = modelAligner.Name + " (forced aligner for timestamps)",
                    Engine = engine,
                };
                if (!engine.IsModelInstalled(modelAligner))
                {
                    var answer = await MessageBox.Show(
                                    Window!,
                                    $"Download {modelAligner}?",
                                    $"'Qwen3 ASR CPP' requires a forced aligner to create timestamps.\nDownload and use {modelAligner.Name}?",
                                    MessageBoxButtons.YesNoCancel,
                                    MessageBoxIcon.Question);

                    if (answer != MessageBoxResult.Yes)
                    {
                        return;
                    }

                    var models = new ObservableCollection<SpeechToTextModelDisplay>
                {
                    displayModelAligner
                };
                    var vm = await _windowService.ShowDialogAsync<DownloadSpeechToTextModelsWindow, DownloadSpeechToTextModelsViewModel>(
                        Window!, viewModel =>
                        {
                            viewModel.SetModels(models, engine, displayModelAligner);
                            viewModel.StartDownload();
                        });

                    if (!vm.OkPressed)
                    {
                        return;
                    }
                }
            }

            if (engine is CrispAsrQwen3 crispQwen3Engine
                && (SelectedForcedAligner == null || SelectedForcedAligner.IsBuiltIn))
            {
                var modelAligner = crispQwen3Engine.ForcedAlignerModel;
                var displayModelAligner = new SpeechToTextModelDisplay
                {
                    Model = modelAligner,
                    Display = modelAligner.Name + " (forced aligner for timestamps)",
                    Engine = engine,
                };
                if (!engine.IsModelInstalled(modelAligner))
                {
                    var answer = await MessageBox.Show(
                                    Window!,
                                    $"Download {modelAligner}?",
                                    $"'Crisp ASR Qwen3' requires a forced aligner to create timestamps.\nDownload and use {modelAligner.Name}?",
                                    MessageBoxButtons.YesNoCancel,
                                    MessageBoxIcon.Question);

                    if (answer != MessageBoxResult.Yes)
                    {
                        return;
                    }

                    var models = new ObservableCollection<SpeechToTextModelDisplay>
                {
                    displayModelAligner
                };
                    var vm = await _windowService.ShowDialogAsync<DownloadSpeechToTextModelsWindow, DownloadSpeechToTextModelsViewModel>(
                        Window!, viewModel =>
                        {
                            viewModel.SetModels(models, engine, displayModelAligner);
                            viewModel.StartDownload();
                        });

                    if (!vm.OkPressed)
                    {
                        return;
                    }
                }
            }

            if (engine is CrispAsrMega crispMegaEngine
                && (SelectedForcedAligner == null || SelectedForcedAligner.IsBuiltIn))
            {
                var modelAligner = crispMegaEngine.ForcedAlignerModel;
                var displayModelAligner = new SpeechToTextModelDisplay
                {
                    Model = modelAligner,
                    Display = modelAligner.Name + " (forced aligner for timestamps)",
                    Engine = engine,
                };
                if (!engine.IsModelInstalled(modelAligner))
                {
                    var answer = await MessageBox.Show(
                                    Window!,
                                    $"Download {modelAligner}?",
                                    $"'Crisp ASR Mega' requires a forced aligner to create timestamps.\nDownload and use {modelAligner.Name}?",
                                    MessageBoxButtons.YesNoCancel,
                                    MessageBoxIcon.Question);

                    if (answer != MessageBoxResult.Yes)
                    {
                        return;
                    }

                    var models = new ObservableCollection<SpeechToTextModelDisplay>
                {
                    displayModelAligner
                };
                    var vm = await _windowService.ShowDialogAsync<DownloadSpeechToTextModelsWindow, DownloadSpeechToTextModelsViewModel>(
                        Window!, viewModel =>
                        {
                            viewModel.SetModels(models, engine, displayModelAligner);
                            viewModel.StartDownload();
                        });

                    if (!vm.OkPressed)
                    {
                        return;
                    }
                }
            }

            if (engine is ICrispAsrEngine crispAsrEngineForAligner
                && SelectedForcedAligner != null && !SelectedForcedAligner.IsBuiltIn)
            {
                var alignerPath = GetForcedAlignerPath(crispAsrEngineForAligner, SelectedForcedAligner);
                if (string.IsNullOrEmpty(alignerPath) || !File.Exists(alignerPath))
                {
                    var alignerWhisperModel = SelectedForcedAligner.ToWhisperModel();
                    var displayAligner = new SpeechToTextModelDisplay
                    {
                        Model = alignerWhisperModel,
                        Engine = engine,
                    };
                    var answer = await MessageBox.Show(
                                    Window!,
                                    $"Download {SelectedForcedAligner.BaseDisplay}?",
                                    $"'{SelectedForcedAligner.BaseDisplay}' is selected but not installed.\nDownload and use {SelectedForcedAligner.FileName}?",
                                    MessageBoxButtons.YesNoCancel,
                                    MessageBoxIcon.Question);

                    if (answer != MessageBoxResult.Yes)
                    {
                        return;
                    }

                    var alignerModels = new ObservableCollection<SpeechToTextModelDisplay> { displayAligner };
                    var vm = await _windowService.ShowDialogAsync<DownloadSpeechToTextModelsWindow, DownloadSpeechToTextModelsViewModel>(
                        Window!, viewModel =>
                        {
                            viewModel.SetModels(alignerModels, engine, displayAligner);
                            viewModel.StartDownload();
                        });

                    if (!vm.OkPressed)
                    {
                        return;
                    }

                    UpdateForcedAlignerUi();
                }
            }

            if (language.Code != "en" && IsModelEnglishOnly(model.Model))
            {
                var answer = await MessageBox.Show(
                    Window!,
                    "Warning",
                    "English model should only be used with English language.\nContinue anyway?",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (answer != MessageBoxResult.Yes)
                {
                    return;
                }
            }
        }

        IsTranscribeEnabled = false;
        ConsoleLog = string.Empty;

        if (!IsBatchMode)
        {
            var mediaInfo = FfmpegMediaInfo.Parse(_videoFileName);
            if (mediaInfo.Tracks.Count(p => p.TrackType == FfmpegTrackType.Audio) == 0)
            {
                var answer = await MessageBox.Show(
                    Window!,
                    "No audio track found",
                    $"No audio track was found in {_videoFileName}",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                IsTranscribeEnabled = true;
                return;
            }

            BatchItems.Add(new SpeechToTextJobItem(_videoFileName, string.Empty, mediaInfo));
        }
        _batchIndex = 0;

        if (BatchItems.Count == 0)
        {
            return;
        }

        if (IsBatchMode)
        {
            var jobItem = BatchItems[0];
            Dispatcher.UIThread.Post(() =>
            {
                if (BatchGrid == null)
                {
                    return;
                }

                BatchGrid.SelectedItem = jobItem;
                BatchGrid.ScrollIntoView(jobItem, null);
            });
        }


        _videoFileName = BatchItems[0].InputVideoFileName;
        _videoInfo.TotalMilliseconds = BatchItems[0].MediaInfo.Duration.TotalMilliseconds;
        _videoInfo.TotalSeconds = BatchItems[0].MediaInfo.Duration.TotalSeconds;
        _videoInfo.Width = BatchItems[0].MediaInfo.Dimension.Width;
        _videoInfo.Height = BatchItems[0].MediaInfo.Dimension.Height;

        ProgressOpacity = 1;
        ProgressText = Se.Language.General.GeneratingAudioFile;
        _startTicks = DateTime.UtcNow.Ticks;

        _batchIndex = 0;
        var startGenerateAudioFileOk = GenerateAudioFile(_videoFileName, _audioTrackNumber);
        if (!startGenerateAudioFileOk)
        {
            if (string.IsNullOrEmpty(_error))
            {
                await MessageBox.Show(Window!, "Unknown error", "Unable to start Whisper!");
            }
            else
            {
                await MessageBox.Show(Window!, "Error", "Unable to start Whisper: " + _error);
            }
            _error = string.Empty;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        if (!IsTranscribeEnabled)
        {
            if (GetEffectiveSelectedEngine() is IOnlineSttEngine)
            {
                _openAiCts?.Cancel();
                return;
            }
            _abort = true;
            return;
        }

        Window?.Close();
    }

    public bool TranscribeViaWhisper(string waveFileName, string videoFileName)
    {
        var engine = GetEffectiveSelectedEngine();

        if (_videoFileName == null)
        {
            return false;
        }

        if (engine is IOnlineSttEngine onlineEngine)
        {
            var languageCode = SelectedLanguage?.Code;
            _timerWhisper.Stop();
            ShowProgressBar();
            _openAiCts = new CancellationTokenSource();
            _ = ProcessOnlineSttTranscription(onlineEngine, waveFileName, languageCode, _openAiCts.Token);
            return true;
        }

        if (SelectedModel is not SpeechToTextModelDisplay model)
        {
            return false;
        }

        if (SelectedLanguage is not WhisperLanguage language)
        {
            return false;
        }

        var settings = Se.Settings.Tools.AudioToText;
        settings.WhisperChoice = engine.Choice;
        SaveSettings();

        _showProgressPct = -1;
        IsTranscribeEnabled = false;
        ProgressOpacity = 1;
        ProgressText = GetProgressText();

        _useCenterChannelOnly = Configuration.Settings.General.FFmpegUseCenterChannelOnly &&
                                FfmpegMediaInfo.Parse(_videoFileName).HasFrontCenterAudio(_audioTrackNumber);

        //Delete invalid preprocessor_config.json file
        if (settings.WhisperChoice is WhisperChoice.PurfviewFasterWhisperXxl)
        {
            var dir = Path.Combine(engine.GetAndCreateWhisperModelFolder(model.Model), model.Model.Folder);
            if (Directory.Exists(dir))
            {
                try
                {
                    var jsonFileName = Path.Combine(dir, "preprocessor_config.json");
                    if (File.Exists(jsonFileName))
                    {
                        var text = FileUtil.ReadAllTextShared(jsonFileName, Encoding.UTF8);
                        if (text.StartsWith("Entry not found", StringComparison.OrdinalIgnoreCase))
                        {
                            File.Delete(jsonFileName);
                        }
                    }

                    jsonFileName = Path.Combine(dir, "vocabulary.json");
                    if (File.Exists(jsonFileName))
                    {
                        var text = FileUtil.ReadAllTextShared(jsonFileName, Encoding.UTF8);
                        if (text.StartsWith("Entry not found", StringComparison.OrdinalIgnoreCase))
                        {
                            File.Delete(jsonFileName);
                        }
                    }
                }
                catch
                {
                    // ignore
                }
            }
        }

        _resultList.Clear();

        var inputFile = waveFileName;
        if (!_useCenterChannelOnly &&
            (engine.Name == WhisperEnginePurfviewFasterWhisperXxl.StaticName) &&
            (videoFileName.EndsWith(".mkv", StringComparison.OrdinalIgnoreCase) ||
             videoFileName.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase)) &&
            _audioTrackNumber < 0)
        {
            inputFile = videoFileName;
        }

        try
        {
            _whisperProcess = GetWhisperProcess(engine, inputFile, model.Model.Name, language.Code, DoTranslateToEnglish,
                OutputHandler);
        }
        catch (Exception e)
        {
            _error = e.Message;
            return false;
        }
        _sw = Stopwatch.StartNew();
        LogToConsole(
            $"Calling speech-to-text ({settings.WhisperChoice}) with : {_whisperProcess.StartInfo.FileName} {_whisperProcess.StartInfo.Arguments}{Environment.NewLine}");

        _abort = false;

        ProgressText = GetProgressText();
        _timerWhisper.Start();

        return true;
    }

    private Process GetWhisperProcess(
        ISpeechToTextEngine engine,
        string waveFileName,
        string model,
        string language,
        bool translate,
        DataReceivedEventHandler? dataReceivedHandler = null)
    {
        if (engine is ChatLlmCppEngine chatLlm)
        {
            var exe = chatLlm.GetExecutable();
            var chatLlmParams = $" -m \"{chatLlm.GetModelForCmdLine(model)}\" -p \"{waveFileName}\"";

            if (OperatingSystem.IsWindows())
            {
                var ffmpegPath = Se.Settings.General.FfmpegPath;
                var targetFfmpegPath = Path.Combine(Path.GetDirectoryName(exe) ?? string.Empty, "ffmpeg.exe");
                if (!string.IsNullOrEmpty(ffmpegPath) && File.Exists(ffmpegPath) &&
                    !File.Exists(targetFfmpegPath))
                {
                    try
                    {
                        File.Copy(ffmpegPath, targetFfmpegPath, false);
                    }
                    catch (Exception ex)
                    {
                        SeLogger.Error(ex, "Error copying ffmpeg to chat-llm folder");
                    }
                }
            }

            var p = new Process
            {
                StartInfo = new ProcessStartInfo(exe, chatLlmParams)
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WorkingDirectory = Path.GetDirectoryName(exe),
                }
            };

            if (dataReceivedHandler != null)
            {
                p.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                p.StartInfo.StandardErrorEncoding = Encoding.UTF8;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.OutputDataReceived += dataReceivedHandler;
                p.ErrorDataReceived += dataReceivedHandler;
            }

#pragma warning disable CA1416
            p.Start();
#pragma warning restore CA1416

            if (dataReceivedHandler != null)
            {
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
            }

            return p;
        }

        if (engine is Qwen3AsrCppEngine qwen3Asr)
        {
            var exe = qwen3Asr.GetExecutable();
            var alignerModel = qwen3Asr.ForcedAlignerModel;
            _qwen3AsrOutputJsonPath = Path.Combine(Path.GetTempPath(), $"qwen3_asr_{Guid.NewGuid():N}.json");
            var qwen3ExtraArgs = engine.CommandLineParameter;

            var qwen3Params = string.IsNullOrWhiteSpace(qwen3ExtraArgs)
                ? $"-m \"{qwen3Asr.GetModelForCmdLine(model)}\" --aligner-model \"{qwen3Asr.GetModelForCmdLine(alignerModel.Name)}\" -f \"{waveFileName}\" --transcribe-align -o \"{_qwen3AsrOutputJsonPath}\""
                : $"{qwen3ExtraArgs} -m \"{qwen3Asr.GetModelForCmdLine(model)}\" --aligner-model \"{qwen3Asr.GetModelForCmdLine(alignerModel.Name)}\" -f \"{waveFileName}\" --transcribe-align -o \"{_qwen3AsrOutputJsonPath}\"";

            var p = new Process
            {
                StartInfo = new ProcessStartInfo(exe, qwen3Params)
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WorkingDirectory = Path.GetDirectoryName(exe),
                }
            };

            if (dataReceivedHandler != null)
            {
                p.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                p.StartInfo.StandardErrorEncoding = Encoding.UTF8;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.OutputDataReceived += dataReceivedHandler;
                p.ErrorDataReceived += dataReceivedHandler;
            }

#pragma warning disable CA1416
            p.Start();
#pragma warning restore CA1416

            if (dataReceivedHandler != null)
            {
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
            }

            return p;
        }

        if (engine is ICrispAsrEngine crispAsrEngine)
        {
            var exe = crispAsrEngine.GetExecutable();
            var crispArgs = crispAsrEngine.CommandLineParameter;
            var crispModel = crispAsrEngine.GetModelForCmdLine(model);
            var langCode = SelectedLanguage?.Code ?? crispAsrEngine.DefaultLanguage;
            var langPart = crispAsrEngine.IncludeLanguage || langCode == "auto"
                ? $"-l {langCode} "
                : string.Empty;
            var alignerPart = string.Empty;
            var selectedAligner = SelectedForcedAligner ?? ForcedAlignerOption.BuiltIn();
            if (!selectedAligner.IsBuiltIn)
            {
                var explicitAlignerPath = GetForcedAlignerPath(crispAsrEngine, selectedAligner);
                if (!string.IsNullOrEmpty(explicitAlignerPath) && File.Exists(explicitAlignerPath))
                {
                    alignerPart = $" -am \"{explicitAlignerPath}\"";
                }
            }
            else if (crispAsrEngine is CrispAsrQwen3 crispQwen3)
            {
                var alignerPath = crispQwen3.GetModelForCmdLine(crispQwen3.ForcedAlignerModel.Name);
                if (File.Exists(alignerPath))
                {
                    alignerPart = $" -am \"{alignerPath}\"";
                }
            }
            else if (crispAsrEngine is CrispAsrMega crispMega)
            {
                var alignerPath = crispMega.GetModelForCmdLine(crispMega.ForcedAlignerModel.Name);
                if (File.Exists(alignerPath))
                {
                    alignerPart = $" -am \"{alignerPath}\"";
                }
            }

            var vadPart = string.Empty;
            // Mega-ASR (crispasr 0.6.10) silently writes a zero-byte SRT unless VAD chunking
            // is enabled — the transcription log says it succeeded but no segments are emitted.
            if (crispAsrEngine is CrispAsrCohere or CrispAsrMega
                && !Regex.IsMatch(crispArgs ?? string.Empty, @"(^|\s)(--vad|-vm|--vad-model)\b"))
            {
                var crispFolder = crispAsrEngine.GetAndCreateWhisperFolder();
                var vadFiles = Directory.Exists(crispFolder)
                    ? Directory.GetFiles(crispFolder, "ggml-silero-v*.bin", SearchOption.TopDirectoryOnly)
                    : Array.Empty<string>();
                var vadPath = vadFiles.OrderByDescending(p => p).FirstOrDefault()
                              ?? Path.Combine(crispFolder, "ggml-silero-vad.bin");
                if (File.Exists(vadPath))
                {
                    vadPart = $" --vad --vad-model \"{vadPath}\"";
                }
            }

            var crispParams = string.IsNullOrWhiteSpace(crispArgs)
                ? $"--backend {crispAsrEngine.BackendName} {langPart}-m \"{crispModel}\"{alignerPart}{vadPart} -f \"{waveFileName}\" --output-srt"
                : $"--backend {crispAsrEngine.BackendName} {langPart}-m \"{crispModel}\"{alignerPart}{vadPart} -f \"{waveFileName}\" --output-srt {crispArgs}";

            Se.WriteToolsLog($"{exe} {crispParams}");

            var p = new Process
            {
                StartInfo = new ProcessStartInfo(exe, crispParams)
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WorkingDirectory = Path.GetDirectoryName(exe),
                }
            };

            if (dataReceivedHandler != null)
            {
                p.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                p.StartInfo.StandardErrorEncoding = Encoding.UTF8;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.OutputDataReceived += dataReceivedHandler;
                p.ErrorDataReceived += dataReceivedHandler;
            }

#pragma warning disable CA1416
            p.Start();
#pragma warning restore CA1416

            if (dataReceivedHandler != null)
            {
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
            }

            return p;
        }

        // Cue-building settings for the helper-script engines, from the Whisper
        // post-processing dialog. The helpers ignore unknown flags, but only these two
        // engines understand them, so they are added only in their blocks below.
        static string GetCueArgs()
        {
            var audioToText = Se.Settings.Tools.AudioToText;

            // Vocabulary prompt: Whisper's documented way to bias recognition toward
            // names and technical terms. Quotes are stripped so the value stays a
            // single command line argument.
            var promptPart = string.Empty;
            var vocabularyPrompt = (audioToText.WhisperVocabularyPrompt ?? string.Empty)
                .Replace("\"", string.Empty).Trim();
            if (vocabularyPrompt.Length > 0)
            {
                promptPart = $" --initial-prompt \"{vocabularyPrompt}\"";
            }

            if (!audioToText.WhisperCueRebuild)
            {
                return promptPart + " --raw-segments";
            }

            return promptPart +
                   $" --max-cue-chars {audioToText.WhisperCueMaxChars}" +
                   $" --max-cue-duration {audioToText.WhisperCueMaxSeconds.ToString(CultureInfo.InvariantCulture)}" +
                   $" --max-cps {audioToText.WhisperCueMaxCps.ToString(CultureInfo.InvariantCulture)}";
        }

        if (engine is MlxWhisperMac mlxWhisperMac)
        {
            // mlx-whisper is a library, not a CLI, so we run a bundled helper script via python3.
            // It writes "<audio-basename>.srt" into the audio's folder, which GetResultFromSrt then
            // picks up. MLX runs Whisper on the Apple GPU / Neural Engine.
            var python = mlxWhisperMac.GetExecutable();
            var scriptPath = mlxWhisperMac.GetTranscribeScript();
            var outputDir = Path.GetDirectoryName(waveFileName) ?? string.Empty;

            var mlxLanguage = language;
            if (mlxLanguage.Equals("english", StringComparison.OrdinalIgnoreCase))
            {
                mlxLanguage = "en";
            }

            var languagePart = !string.IsNullOrWhiteSpace(mlxLanguage) &&
                               !mlxLanguage.Equals("auto", StringComparison.OrdinalIgnoreCase)
                ? $" --language {mlxLanguage}"
                : string.Empty;
            var taskPart = translate ? " --task translate" : string.Empty;
            var mlxExtraArgs = engine.CommandLineParameter;
            var extraPart = string.IsNullOrWhiteSpace(mlxExtraArgs) ? string.Empty : " " + mlxExtraArgs.Trim();

            var mlxParameters =
                $"\"{scriptPath}\" --audio \"{waveFileName}\" --model {mlxWhisperMac.GetModelForCmdLine(model)} " +
                $"--output-format srt --output-dir \"{outputDir}\"{languagePart}{taskPart}{GetCueArgs()}{extraPart}";

            Se.WriteToolsLog($"{python} {mlxParameters}");

            var mlxProcess = new Process
            {
                StartInfo = new ProcessStartInfo(python, mlxParameters)
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                }
            };

            mlxProcess.StartInfo.EnvironmentVariables["PYTHONIOENCODING"] = "utf-8";
            mlxProcess.StartInfo.EnvironmentVariables["PYTHONUTF8"] = "1";

            if (dataReceivedHandler != null)
            {
                mlxProcess.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                mlxProcess.StartInfo.StandardErrorEncoding = Encoding.UTF8;
                mlxProcess.StartInfo.RedirectStandardOutput = true;
                mlxProcess.StartInfo.RedirectStandardError = true;
                mlxProcess.OutputDataReceived += dataReceivedHandler;
                mlxProcess.ErrorDataReceived += dataReceivedHandler;
            }

#pragma warning disable CA1416
            mlxProcess.Start();
#pragma warning restore CA1416

            if (dataReceivedHandler != null)
            {
                mlxProcess.BeginOutputReadLine();
                mlxProcess.BeginErrorReadLine();
            }

            return mlxProcess;
        }

        if (engine is FasterWhisperMac fasterWhisperMac)
        {
            // faster-whisper is a library, not a CLI, so we run a bundled helper script via
            // python3. It writes "<audio-basename>.srt" into the audio's folder, which
            // GetResultFromSrt then picks up. CTranslate2 has no Apple GPU backend, so this
            // decodes on the CPU; the script uses batched inference and one thread per core
            // to compensate (see faster_whisper_transcribe.py).
            var python = fasterWhisperMac.GetExecutable();
            var scriptPath = fasterWhisperMac.GetTranscribeScript();
            var outputDir = Path.GetDirectoryName(waveFileName) ?? string.Empty;

            var fwLanguage = language;
            if (fwLanguage.Equals("english", StringComparison.OrdinalIgnoreCase))
            {
                fwLanguage = "en";
            }

            var languagePart = !string.IsNullOrWhiteSpace(fwLanguage) &&
                               !fwLanguage.Equals("auto", StringComparison.OrdinalIgnoreCase)
                ? $" --language {fwLanguage}"
                : string.Empty;
            var taskPart = translate ? " --task translate" : string.Empty;
            var fwExtraArgs = engine.CommandLineParameter;
            var extraPart = string.IsNullOrWhiteSpace(fwExtraArgs) ? string.Empty : " " + fwExtraArgs.Trim();

            var fwParameters =
                $"\"{scriptPath}\" --audio \"{waveFileName}\" --model {fasterWhisperMac.GetModelForCmdLine(model)} " +
                $"--output-format srt --output-dir \"{outputDir}\"{languagePart}{taskPart}{GetCueArgs()}{extraPart}";

            Se.WriteToolsLog($"{python} {fwParameters}");

            var fwProcess = new Process
            {
                StartInfo = new ProcessStartInfo(python, fwParameters)
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                }
            };

            fwProcess.StartInfo.EnvironmentVariables["PYTHONIOENCODING"] = "utf-8";
            fwProcess.StartInfo.EnvironmentVariables["PYTHONUTF8"] = "1";

            if (dataReceivedHandler != null)
            {
                fwProcess.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                fwProcess.StartInfo.StandardErrorEncoding = Encoding.UTF8;
                fwProcess.StartInfo.RedirectStandardOutput = true;
                fwProcess.StartInfo.RedirectStandardError = true;
                fwProcess.OutputDataReceived += dataReceivedHandler;
                fwProcess.ErrorDataReceived += dataReceivedHandler;
            }

#pragma warning disable CA1416
            fwProcess.Start();
#pragma warning restore CA1416

            if (dataReceivedHandler != null)
            {
                fwProcess.BeginOutputReadLine();
                fwProcess.BeginErrorReadLine();
            }

            return fwProcess;
        }

        var settings = Se.Settings.Tools.AudioToText;
        var args = engine.CommandLineParameter;
        var cppVulkanDevice = string.Empty;
        if (args.Contains("--device", StringComparison.Ordinal) &&
            engine.Name == WhisperEngineCppVulkan.StaticName)
        {
            var deviceMatch = Regex.Match(args, @"--device\s+(\d+)");
            if (deviceMatch.Success)
            {
                cppVulkanDevice = deviceMatch.Groups[1].Value;
                args = Regex.Replace(args, @"--device\s+\d+", "").Trim(); // Remove --device and its value from args
            }
        }

        var translateToEnglish = translate ? GetWhisperTranslateParameter(engine) : string.Empty;
        if (language.ToLowerInvariant() == "english" || language.ToLowerInvariant() == "en")
        {
            language = "en";
            translateToEnglish = string.Empty;
        }

        if (settings.WhisperChoice is WhisperChoice.Cpp or WhisperChoice.CppCuBlas or WhisperChoice.CppVulkan)
        {
            if (!args.Contains("--print-progress"))
            {
                translateToEnglish += "--print-progress ";
            }
        }

        var outputSrt = string.Empty;
        var postParams = string.Empty;
        if (settings.WhisperChoice is WhisperChoice.Cpp or WhisperChoice.CppCuBlas or WhisperChoice.ConstMe or WhisperChoice.CppVulkan)
        {
            outputSrt = "--output-srt ";
        }
        else if (settings.WhisperChoice == WhisperChoice.StableTs)
        {
            var srtFileName = Path.GetFileNameWithoutExtension(waveFileName);
            postParams = $" -o {srtFileName}.srt";
        }

        var w = engine.GetExecutable();
        var m = engine.GetModelForCmdLine(model);

        // Automatic language detection (#11848). whisper.cpp/Const-me accept the literal
        // "auto" (their default is "en", so the flag is required). The faster-whisper based
        // engines (Purfview, CTranslate2) and OpenAI reject "auto" but auto-detect when no
        // --language is given, so the flag is omitted there.
        var languageArg = $"--language {language} ";
        if (language.Equals("auto", StringComparison.OrdinalIgnoreCase))
        {
            languageArg = settings.WhisperChoice is WhisperChoice.Cpp or WhisperChoice.CppCuBlas
                or WhisperChoice.CppVulkan or WhisperChoice.CppCuBlasLib or WhisperChoice.ConstMe
                ? "--language auto "
                : string.Empty;
        }

        var parameters =
            $"{languageArg}--model \"{m}\" {outputSrt}{translateToEnglish}{args} \"{waveFileName}\"{postParams}";

        if (engine is WhisperEngineCTranslate2)
        {
            parameters = parameters.Replace("--model", "--model_directory");
        }

        Se.WriteToolsLog($"{w} {parameters}");

        var process = new Process
        {
            StartInfo = new ProcessStartInfo(w, parameters)
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                UseShellExecute = false,
            }
        };

        if (!string.IsNullOrEmpty(cppVulkanDevice))
        {
            process.StartInfo.EnvironmentVariables["GGML_VULKAN_DEVICE"] = cppVulkanDevice;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if (!string.IsNullOrEmpty(Se.Settings.General.FfmpegPath) &&
                process.StartInfo.EnvironmentVariables["Path"] != null)
            {
                process.StartInfo.EnvironmentVariables["Path"] =
                    process.StartInfo.EnvironmentVariables["Path"]?.TrimEnd(';') + ";" +
                    Path.GetDirectoryName(Se.Settings.General.FfmpegPath);
            }
        }

        var whisperFolder = engine.GetAndCreateWhisperFolder();
        if (!string.IsNullOrEmpty(whisperFolder))
        {
            if (File.Exists(whisperFolder))
            {
                whisperFolder = Path.GetDirectoryName(whisperFolder);
            }

            if (whisperFolder != null)
            {
                process.StartInfo.WorkingDirectory = whisperFolder;
            }
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if (!string.IsNullOrEmpty(whisperFolder) && process.StartInfo.EnvironmentVariables["Path"] != null)
            {
                process.StartInfo.EnvironmentVariables["Path"] =
                    process.StartInfo.EnvironmentVariables["Path"]?.TrimEnd(';') + ";" + whisperFolder;
            }
        }

        if (settings.WhisperChoice != WhisperChoice.Cpp &&
            settings.WhisperChoice != WhisperChoice.CppCuBlas &&
            settings.WhisperChoice != WhisperChoice.ConstMe)
        {
            process.StartInfo.EnvironmentVariables["PYTHONIOENCODING"] = "utf-8";
            process.StartInfo.EnvironmentVariables["PYTHONUTF8"] = "1";
        }

        if (dataReceivedHandler != null)
        {
            process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            process.StartInfo.StandardErrorEncoding = Encoding.UTF8;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.OutputDataReceived += dataReceivedHandler;
            process.ErrorDataReceived += dataReceivedHandler;
        }

#pragma warning disable CA1416
        process.Start();
#pragma warning restore CA1416

        if (dataReceivedHandler != null)
        {
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
        }

        return process;
    }

    private static string GetWhisperTranslateParameter(ISpeechToTextEngine engine)
    {
        if (engine.Choice == new WhisperEnginePurfviewFasterWhisperXxl().Choice ||
            engine.Choice == new WhisperEngineOpenAi().Choice ||
            engine.Choice == new WhisperEngineCTranslate2().Choice)
        {
            return "--task translate ";
        }

        return "--translate ";
    }

    private bool GenerateAudioFile(string videoFileName, int audioTrackNumber)
    {
        if (string.IsNullOrEmpty(videoFileName))
        {
            return false;
        }

        // Local whisper engines read the file directly and want 16 kHz PCM, so
        // a pre-extracted 16 kHz WAV can be handed to them as-is. Online engines
        // upload the file instead, and a 16 kHz WAV easily exceeds the upload
        // cap on long audio — skip the short-circuit and transcode through
        // ffmpeg into the chosen compressed format.
        var isOpenAiEngine = GetEffectiveSelectedEngine() is IOnlineSttEngine;
        if (!isOpenAiEngine && videoFileName.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                using var waveFile = new WavePeakGenerator2(videoFileName);
                if (waveFile.Header != null && waveFile.Header.SampleRate == 16000)
                {
                    _videoFileName = videoFileName;
                    var startOk = TranscribeViaWhisper(videoFileName, _videoFileName);
                    return startOk;
                }
            }
            catch
            {
                // ignore
            }
        }

        _ffmpegLog = new StringBuilder();

        // Online STT endpoints cap uploads (OpenAI 25 MB, DashScope 10 MB) and a
        // 2-hour WAV blows past that. When an online engine is selected,
        // transcode to a compressed format so the upload stays under the limit;
        // the OpenAI-compatible engine honors the user's chosen format, the
        // others default to mp3. Local engines (whisper.cpp, faster-whisper, ...)
        // keep getting WAV because they read the file locally and expect PCM.
        var sttAudioFormat = isOpenAiEngine
            ? (GetEffectiveSelectedEngine() is OpenAiCompatibleSttEngine ? OpenAiCompatibleSttAudioFormat : "mp3")
            : "wav";
        var extension = OpenAiSttService.GetFileExtensionForFormat(sttAudioFormat);
        // Place the extracted audio in a dedicated per-run subfolder. Engines like
        // Purfview Faster-Whisper-XXL write their output (.srt/.ass) and intermediate
        // (.tmp) files next to the input, so keeping the input isolated lets us delete
        // the whole folder afterwards and leave no leftovers in the temp directory (#11837).
        _audioFileName = Path.Combine(GetSttTempFolder(), Guid.NewGuid() + "." + extension);
        _filesToDelete.Add(_audioFileName);
        _audioExtractProcess = GetFfmpegProcess(videoFileName, audioTrackNumber, _audioFileName, sttAudioFormat);
        if (_audioExtractProcess == null)
        {
            return false;
        }

        _audioExtractProcess.ErrorDataReceived += (sender, args) => { _ffmpegLog.AppendLine(args.Data); };

        _audioExtractProcess.StartInfo.RedirectStandardError = true;
#pragma warning disable CA1416
        var started = _audioExtractProcess.Start();
#pragma warning restore CA1416

        _audioExtractProcess.BeginErrorReadLine();
        _abort = false;
        _timerAudioExtract.Start();
        return true;
    }

    private void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
    {
        if (string.IsNullOrWhiteSpace(outLine.Data))
        {
            return;
        }

        if (SelectedLanguage is not WhisperLanguage language)
        {
            return;
        }

        if (outLine.Data.Contains("not all tensors loaded from model file"))
        {
            _incompleteModel = true;
        }

        if (outLine.Data.Contains("error: unknown argument: ", StringComparison.OrdinalIgnoreCase))
        {
            _unknownArgument = true;
        }
        else if (outLine.Data.Contains("error: unrecognized argument: ", StringComparison.OrdinalIgnoreCase))
        {
            _unknownArgument = true;
        }
        else if (outLine.Data.Contains("error: unrecognized arguments: ", StringComparison.OrdinalIgnoreCase))
        {
            _unknownArgument = true;
        }
        else if (outLine.Data.Contains("CUDA failed with error out of memory", StringComparison.OrdinalIgnoreCase))
        {
            _cudaOutOfMemory = true;
        }
        //if (outLine.Data.Contains("running on: CUDA", StringComparison.OrdinalIgnoreCase))
        //{
        //    _runningOnCuda = true;
        //}

        LogToConsole(outLine.Data.Trim() + Environment.NewLine);

        foreach (var line in outLine.Data.SplitToLines())
        {
            if (_timeRegexShort.IsMatch(line))
            {
                var start = line.Substring(1, 10);
                var end = line.Substring(14, 10);
                var text = line.Remove(0, 25).Trim();
                var rt = new ResultText
                {
                    Start = GetSeconds(start),
                    End = GetSeconds(end),
                    Text = Utilities.AutoBreakLine(text, language.Code),
                };

                if (_showProgressPct < 0)
                {
                    _endSeconds = (double)rt.End;
                }

                _resultList.Add(rt);
            }
            else if (_timeRegexLong.IsMatch(line))
            {
                var start = line.Substring(1, 12);
                var end = line.Substring(18, 12);
                var text = line.Remove(0, 31).Trim();
                var rt = new ResultText
                {
                    Start = GetSeconds(start),
                    End = GetSeconds(end),
                    Text = Utilities.AutoBreakLine(text, language.Code),
                };

                if (_showProgressPct < 0)
                {
                    _endSeconds = (double)rt.End;
                }

                _resultList.Add(rt);
            }
            else if (line.StartsWith("whisper_full: progress =", StringComparison.OrdinalIgnoreCase))
            {
                var arr = line.Split('=');
                if (arr.Length == 2)
                {
                    var pctString = arr[1].Trim().TrimEnd('%').TrimEnd();
                    if (double.TryParse(pctString, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture,
                            out var pct))
                    {
                        _endSeconds = _videoInfo.TotalSeconds * pct / 100.0;
                        _showProgressPct = pct;
                    }
                }
            }
            else if (_pctWhisper.IsMatch(line.TrimStart()))
            {
                var arr = line.Split('%');
                if (arr.Length > 1 && double.TryParse(arr[0], NumberStyles.AllowDecimalPoint,
                        CultureInfo.InvariantCulture, out var pct))
                {
                    _endSeconds = _videoInfo.TotalSeconds * pct / 100.0;
                    _showProgressPct = pct;
                }
            }
            else if (_pctWhisperFaster.IsMatch(line))
            {
                var arr = line.Split('%');
                if (arr.Length > 1 && double.TryParse(arr[0].Trim(), NumberStyles.AllowDecimalPoint,
                        CultureInfo.InvariantCulture, out var pct))
                {
                    _endSeconds = _videoInfo.TotalSeconds * pct / 100.0;
                    _showProgressPct = pct;
                }
            }
        }
    }

    private void LogToConsole(string s, bool skipOutputText = false)
    {
        if (!skipOutputText)
        {
            _outputText.Enqueue(s);
        }

        ConsoleLog += s.Trim() + "\n";

        // Tail behavior: keep the console scrolled to the latest line so the user
        // doesn't have to camp on PageDown. CaretIndex / TextBox.ScrollToLine
        // proved unreliable here — BringCaretToView short-circuits while the
        // TextBox is unfocused, and ScrollToLine silently no-ops when the inner
        // TextPresenter isn't templated yet. Drive the inner ScrollViewer
        // directly. Post at Background priority so the Render-priority layout
        // pass has updated Extent for the freshly-appended line first.
        //
        // Scroll BOTH the batch and single-mode TextBoxes — only one is visible
        // at any time, and which one depends on IsBatchMode. Scrolling the hidden
        // one is a harmless no-op; scrolling only the wrong one (the bug before
        // this) left the visible log frozen at the top.
        Dispatcher.UIThread.Post(() =>
        {
            ScrollTextBoxToEnd(TextBoxConsoleLogBatch);
            ScrollTextBoxToEnd(TextBoxConsoleLogSingle);
        }, DispatcherPriority.Background);
    }

    private static void ScrollTextBoxToEnd(TextBox? textBox)
    {
        var scrollViewer = textBox?.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
        scrollViewer?.ScrollToEnd();
    }

    private static decimal GetSeconds(string timeCode)
    {
        return (decimal)(TimeCode.ParseToMilliseconds(timeCode) / 1000.0);
    }

    private Process? GetFfmpegProcess(string videoFileName, int audioTrackNumber, string outAudioFile, string audioFormat = "wav")
    {
        if (!File.Exists(Se.Settings.General.FfmpegPath) && Configuration.IsRunningOnWindows)
        {
            return null;
        }

        var audioParameter = string.Empty;
        if (audioTrackNumber >= 0)
        {
            audioParameter = $"-map 0:{audioTrackNumber}";
        }

        var fFmpegAudioTranscodeSettings = GetFfmpegTranscodeFormatString(audioFormat, _useCenterChannelOnly);

        //-i indicates the input
        //-vn means no video output
        //-ar 44100 indicates the sampling frequency.
        //-ab indicates the bit rate (in this example 160kb/s)
        //-af volume=1.75 will boot volume... 1.0 is normal
        //-ac 2 means 2 channels
        // "-map 0:a:0" is the first audio stream, "-map 0:a:1" is the second audio stream

        var exeFilePath = Se.Settings.General.FfmpegPath;
        if (!File.Exists(exeFilePath))
        {
            exeFilePath = "ffmpeg";
        }

        var parameters = string.Format(fFmpegAudioTranscodeSettings, videoFileName, outAudioFile, audioParameter);
        return new Process
        {
            StartInfo = new ProcessStartInfo(exeFilePath, parameters)
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                UseShellExecute = false,
            }
        };
    }

    /// <summary>
    /// ffmpeg argument template for transcoding the source audio. WAV stays on
    /// the historical pipeline (lossless 16 kHz mono PCM); the compressed
    /// formats target ~32 kbit/s mono at 16 kHz, which is plenty for speech
    /// recognition and keeps a 2-hour video well under OpenAI's 25 MB upload
    /// limit. Opus is shipped inside a webm container because OpenAI accepts
    /// webm but rejects bare ".opus" uploads.
    /// </summary>
    private static string GetFfmpegTranscodeFormatString(string audioFormat, bool useCenterChannelOnly)
    {
        var normalized = string.IsNullOrWhiteSpace(audioFormat) ? "wav" : audioFormat.Trim().ToLowerInvariant();
        var channelArgs = useCenterChannelOnly
            ? "-af \"pan=mono|c0=FC,volume=1.75\""
            : "-ac 1 -af volume=1.75";

        return normalized switch
        {
            "mp3" => "-i \"{0}\" -vn -ar 16000 " + channelArgs + " -c:a libmp3lame -b:a 32k -f mp3 {2} \"{1}\"",
            "m4a" => "-i \"{0}\" -vn -ar 16000 " + channelArgs + " -c:a aac -b:a 32k -f ipod {2} \"{1}\"",
            "webm" => "-i \"{0}\" -vn -ar 16000 " + channelArgs + " -c:a libopus -b:a 28k -f webm {2} \"{1}\"",
            _ => useCenterChannelOnly
                ? "-i \"{0}\" -vn -ar 16000 -ab 32k -af volume=1.75 -af \"pan=mono|c0=FC\" -f wav {2} \"{1}\""
                : "-i \"{0}\" -vn -ar 16000 -ac 1 -ab 32k -af volume=1.75 -f wav {2} \"{1}\"",
        };
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
            UiUtil.ShowHelp("features/speech-to-text");
        }
    }

    private void RefreshDownloadStatus(WhisperModel? result)
    {
        var engine = GetEffectiveSelectedEngine();

        if (SelectedModel is not SpeechToTextModelDisplay oldModel)
        {
            return;
        }

        Models.Clear();
        foreach (var model in engine.Models)
        {
            Models.Add(new SpeechToTextModelDisplay
            {
                Model = model,
                Engine = engine,
            });
        }

        if (result != null)
        {
            SelectedModel = Enumerable.FirstOrDefault<SpeechToTextModelDisplay>((IEnumerable<SpeechToTextModelDisplay>)Models, m => m.Model.Name == result.Name);
        }
        else
        {
            SelectedModel = Enumerable.FirstOrDefault<SpeechToTextModelDisplay>((IEnumerable<SpeechToTextModelDisplay>)Models, m => m.Model.Name == oldModel.Model.Name);
        }
    }

    internal void OnEngineChanged(object? sender, SelectionChangedEventArgs e)
    {
        EngineChanged();
    }

    private void EngineChanged()
    {
        var engine = GetEffectiveSelectedEngine();
        UpdateBackendSelectionUi();

        Languages.Clear();
        foreach (var l in GetEngineLanguages(engine))
        {
            Languages.Add(l);
        }

        var savedCode = Se.Settings.Tools.AudioToText.WhisperLanguageCode;
        WhisperLanguage? language = null;
        if (!string.IsNullOrEmpty(savedCode))
        {
            language = Enumerable.FirstOrDefault<WhisperLanguage>(Languages, p => p.Code == savedCode);
        }

        if (language == null && SelectedLanguage is { } prev)
        {
            language = Enumerable.FirstOrDefault<WhisperLanguage>(Languages, p => string.Equals(p.Name, prev.Name, StringComparison.OrdinalIgnoreCase));
        }

        SelectedLanguage = language ?? PickDefaultLanguage(Languages);

        Models.Clear();
        foreach (var model in engine.Models)
        {
            Models.Add(new SpeechToTextModelDisplay
            {
                Model = model,
                Engine = engine,
            });
        }

        if (Models.Count > 0)
        {
            var model = Enumerable.FirstOrDefault<SpeechToTextModelDisplay>((IEnumerable<SpeechToTextModelDisplay>)Models, p => p.Model.Name == Se.Settings.Tools.AudioToText.WhisperModel);
            if (model != null)
            {
                SelectedModel = model;
            }
            else
            {
                SelectedModel = Enumerable.FirstOrDefault<SpeechToTextModelDisplay>((IEnumerable<SpeechToTextModelDisplay>)Models);
            }
        }

        var isPurfview = engine.Name == WhisperEnginePurfviewFasterWhisperXxl.StaticName;

        var isOnlineSttEngine = engine is IOnlineSttEngine;

        IsModelSelectionVisible = !isOnlineSttEngine;
        if (!IsModelSelectionVisible)
        {
            SelectedModel = null;
        }

        IsLanguageSelectionVisible = !isOnlineSttEngine;
        if (!IsLanguageSelectionVisible)
        {
            SelectedLanguage = null;
        }

        IsOpenAiCompatibleSttVisible = engine is OpenAiCompatibleSttEngine;
        IsOpenRouterSttVisible = engine is OpenRouterSttEngine;
        IsDashScopeSttVisible = engine is DashScopeQwen3SttEngine;
        IsAdvancedSettingsVisible = !isOnlineSttEngine;

        IsTranslateVisible = IsTranslateAvailable(engine);

        Parameters = engine.CommandLineParameter;

        UpdateEngineStatusUi(engine);

        SaveSettings();

        if (engine is ICrispAsrEngine && !_crispAsrUpdatePromptShown)
        {
            Dispatcher.UIThread.Post(async () => await CheckCrispAsrForUpdateAsync());
        }
        else if (engine is WhisperEngineCpp or WhisperEngineCppCuBlas or WhisperEngineCppVulkan
                 && !_whisperCppUpdatePromptShown)
        {
            Dispatcher.UIThread.Post(async () => await CheckWhisperCppForUpdateAsync());
        }
    }

    private void UpdateEngineStatusUi(ISpeechToTextEngine engine)
    {
        var canDownload = engine.CanBeDownloaded();
        var isInstalled = engine.IsEngineInstalled();

        // Settings gear is for downloadable engines that already have a binary on disk.
        // It opens a dialog with the installed backend, status and Re-download — which is
        // also the answer to issue #11022 (switch backend after the initial install).
        IsEngineSettingsButtonVisible = canDownload && isInstalled && IsSettingsCapable(engine);

        if (engine is MlxWhisperMac && !isInstalled)
        {
            // pip-managed engine - Subtitle Edit cannot download it, so show install help instead.
            EngineDownloadHint = "mlx-whisper not found - install it with: pip3 install mlx-whisper";
            IsEngineDownloadButtonVisible = false;
            return;
        }

        if (engine is FasterWhisperMac && !isInstalled)
        {
            // pip-managed engine - Subtitle Edit cannot download it, so show install help instead.
            EngineDownloadHint = "faster-whisper not found - install it with: pip3 install faster-whisper";
            IsEngineDownloadButtonVisible = false;
            return;
        }

        if (!canDownload || isInstalled)
        {
            EngineDownloadHint = string.Empty;
            IsEngineDownloadButtonVisible = false;
            return;
        }

        var size = engine.DownloadSizeText;
        EngineDownloadHint = string.IsNullOrEmpty(size)
            ? string.Format(Se.Language.Video.AudioToText.DownloadX, engine.Name)
            : string.Format(Se.Language.Video.AudioToText.DownloadX, engine.Name) + $" ({size})";
        IsEngineDownloadButtonVisible = true;
    }

    // Show the gear for any engine that is locally downloadable (i.e. produces a binary on disk
    // we can describe, re-download or open the folder for). Whisper.cpp variants and CrispASR
    // additionally have hash tracking so their Status row is meaningful; CTranslate2 / ConstMe /
    // Purfview have no DownloadHashManager entry yet and will show "Unknown" until hashes land.
    private static bool IsSettingsCapable(ISpeechToTextEngine engine)
        => engine is WhisperEngineCpp
                   or WhisperEngineCppCuBlas
                   or WhisperEngineCppVulkan
                   or WhisperEngineCTranslate2
                   or WhisperEngineConstMe
                   or WhisperEnginePurfviewFasterWhisperXxl
                   or ICrispAsrEngine;

    [RelayCommand]
    private async Task DownloadSelectedEngine()
    {
        // Reuses the same prompt flow as the context-menu "Re-download" entry, so
        // CrispASR variant selection and Vulkan-SDK warnings stay in one place.
        // The status indicator next to the combo refreshes from VM properties below; the
        // per-item ✓/⬇ markers in the dropdown reflect what was on disk when the window
        // opened — that's acceptable since the typical path is download → transcribe and
        // the user doesn't revisit the dropdown mid-session.
        await ReDownloadWhisperEngine();
        UpdateEngineStatusUi(GetEffectiveSelectedEngine());
    }

    [RelayCommand]
    private async Task ShowEngineSettings()
    {
        if (Window == null)
        {
            return;
        }

        var engine = GetEffectiveSelectedEngine();
        if (!IsSettingsCapable(engine) || !engine.IsEngineInstalled())
        {
            return;
        }

        await _windowService.ShowDialogAsync<SpeechToTextEngineSettingsWindow, SpeechToTextEngineSettingsViewModel>(
            Window,
            vm => vm.Initialize(engine, async () => await ReDownloadWhisperEngine()));

        UpdateEngineStatusUi(GetEffectiveSelectedEngine());
    }

    private async Task CheckCrispAsrForUpdateAsync()
    {
        if (_crispAsrUpdatePromptShown || Window == null)
        {
            return;
        }

        var engine = GetEffectiveSelectedEngine();
        if (engine is not ICrispAsrEngine || !engine.IsEngineInstalled())
        {
            return;
        }

        var folder = engine.GetAndCreateWhisperFolder();
        var lookup = TryReadSidecarHash(folder) ?? TryHashInstalledExecutable(engine, folder);
        if (lookup is not var (key, hash))
        {
            return;
        }

        if (DownloadHashManager.GetStatus(key, hash) != DownloadHashManager.UpdateStatus.UpdateAvailable)
        {
            return;
        }

        _crispAsrUpdatePromptShown = true;

        var answer = await MessageBox.Show(
            Window!,
            string.Format(Se.Language.Video.AudioToText.UpdateXTitle, engine.Name),
            string.Format(Se.Language.Video.AudioToText.UpdateXMessage, engine.Name, Environment.NewLine),
            MessageBoxButtons.YesNoCancel,
            MessageBoxIcon.Question);

        if (answer != MessageBoxResult.Yes)
        {
            return;
        }

        var crispVariant = DownloadHashManager.GetCrispAsrVariant(key)
                           ?? (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "vulkan" : string.Empty);

        await _windowService.ShowDialogAsync<DownloadSpeechToTextEngineWindow, DownloadSpeechToTextEngineViewModel>(
            Window!, viewModel =>
            {
                viewModel.Engine = engine;
                viewModel.CrispAsrWindowsVariant = crispVariant;
                viewModel.StartDownload();
            });

        RefreshEngineCombo?.Invoke();
    }

    private async Task CheckWhisperCppForUpdateAsync()
    {
        if (_whisperCppUpdatePromptShown || Window == null)
        {
            return;
        }

        var engine = GetEffectiveSelectedEngine();
        if (engine is not (WhisperEngineCpp or WhisperEngineCppCuBlas or WhisperEngineCppVulkan)
            || !engine.IsEngineInstalled())
        {
            return;
        }

        var folder = engine.GetAndCreateWhisperFolder();
        var lookup = TryReadSidecarHash(folder) ?? TryHashWhisperCppExecutable(engine);
        if (lookup is not var (key, hash))
        {
            return;
        }

        if (DownloadHashManager.GetStatus(key, hash) != DownloadHashManager.UpdateStatus.UpdateAvailable)
        {
            return;
        }

        _whisperCppUpdatePromptShown = true;

        var answer = await MessageBox.Show(
            Window!,
            string.Format(Se.Language.Video.AudioToText.UpdateXTitle, engine.Name),
            string.Format(Se.Language.Video.AudioToText.UpdateXMessage, engine.Name, Environment.NewLine),
            MessageBoxButtons.YesNoCancel,
            MessageBoxIcon.Question);

        if (answer != MessageBoxResult.Yes)
        {
            return;
        }

        await _windowService.ShowDialogAsync<DownloadSpeechToTextEngineWindow, DownloadSpeechToTextEngineViewModel>(
            Window!, viewModel =>
            {
                viewModel.Engine = engine;
                viewModel.StartDownload();
            });

        RefreshEngineCombo?.Invoke();
    }

    private static (string key, string hash)? TryHashWhisperCppExecutable(ISpeechToTextEngine engine)
    {
        try
        {
            var key = DownloadHashManager.ResolveWhisperCppExecutableKey(engine.Choice);
            if (key == null)
            {
                return null;
            }

            var exePath = engine.GetExecutable();
            var hash = DownloadHashManager.ComputeSha256(exePath);
            return hash == null ? null : (key, hash);
        }
        catch
        {
            return null;
        }
    }

    private static (string key, string hash)? TryReadSidecarHash(string folder)
    {
        var sidecar = Path.Combine(folder, ".installed.sha256");
        if (!File.Exists(sidecar))
        {
            return null;
        }

        try
        {
            var lines = File.ReadAllLines(sidecar);
            if (lines.Length < 2)
            {
                return null;
            }

            var key = lines[0].Trim();
            var hash = lines[1].Trim();
            if (key.Length == 0 || hash.Length == 0)
            {
                return null;
            }

            return (key, hash);
        }
        catch
        {
            return null;
        }
    }

    private static (string key, string hash)? TryHashInstalledExecutable(ISpeechToTextEngine engine, string folder)
    {
        try
        {
            string? variant = null;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                variant = DownloadHashManager.DetectCrispAsrWindowsVariant(folder);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                     && RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
            {
                variant = DownloadHashManager.DetectCrispAsrLinuxVariant(folder);
            }
            var key = DownloadHashManager.ResolveCrispAsrExecutableKey(variant);
            if (key == null)
            {
                return null;
            }

            var exePath = engine.GetExecutable();
            var hash = DownloadHashManager.ComputeSha256(exePath);
            return hash == null ? null : (key, hash);
        }
        catch
        {
            return null;
        }
    }

    private static WhisperLanguage? PickDefaultLanguage(IEnumerable<WhisperLanguage> languages)
    {
        var list = languages as IList<WhisperLanguage> ?? languages.ToList();
        return Enumerable.FirstOrDefault<WhisperLanguage>(list, p => string.Equals(p.Name, "English", StringComparison.OrdinalIgnoreCase))
            ?? Enumerable.FirstOrDefault<WhisperLanguage>(list, p => p.Code == "en" || p.Code == "eng_Latn")
            ?? Enumerable.FirstOrDefault<WhisperLanguage>(list);
    }

    internal void Initialize(string? videoFileName, int audioTrackNumber)
    {
        _videoFileName = videoFileName;
        _audioTrackNumber = audioTrackNumber;
        if (string.IsNullOrEmpty(_videoFileName) || !File.Exists(_videoFileName))
        {
            IsBatchModeVisible = false;
            IsSingleModeVisible = false;
            IsBatchMode = true;
        }
        else
        {
            IsBatchModeVisible = true;
            IsSingleModeVisible = false;
            IsBatchMode = false;
        }
    }

    internal void InitializeBatch(List<AudioClip> audioClips, int audioTrackNumber, bool autoStart, string? language)
    {
        _audioTrackNumber = audioTrackNumber;
        IsBatchMode = true;
        _audioClips = audioClips;
        _audioClipsAutoStart = autoStart;
        ResultAudioClips = audioClips.Select(ac => new AudioClip(ac)).ToList();

        if (language != null)
        {
            var match = Languages.FirstOrDefault(p => p.Code == language)
                        ?? Languages.FirstOrDefault(p => string.Equals(p.Name, language, StringComparison.OrdinalIgnoreCase));
            if (match != null)
            {
                SelectedLanguage = match;
            }
        }
    }

    internal void OnWindowLoaded()
    {
        UiUtil.RestoreWindowPosition(Window);

        if (_audioClips != null)
        {
            Dispatcher.UIThread.Post(async () =>
            {
                await AddFiles(_audioClips.Select(ac => ac.AudioFileName).ToArray());
                if (_audioClipsAutoStart)
                {
                    await Transcribe();
                }
            });
        }
    }

    internal void OnWindowClosing(WindowClosingEventArgs e)
    {
        UiUtil.SaveWindowPosition(Window);
        Task.Run(() => { DeleteTempFiles(); });
    }

    internal void WindowContextMenuOpening(object? sender, EventArgs e)
    {
        var engine = GetEffectiveSelectedEngine();
        if (!engine.CanBeDownloaded())
        {
            IsReDownloadVisible = false;
            return;
        }

        IsReDownloadVisible = true;
        var displayName = engine is ICrispAsrEngine ? CrispAsrEngine.StaticName : engine.Name;
        if (engine.IsEngineInstalled())
        {
            ReDownloadText = string.Format(Se.Language.General.ReDownloadX, displayName);
        }
        else
        {
            ReDownloadText = string.Format(Se.Language.General.DownloadX, displayName);
        }
    }

    internal void FileGridOnDragOver(object? sender, DragEventArgs e)
    {
        if (e.DataTransfer.Contains(DataFormat.File))
        {
            e.DragEffects = DragDropEffects.Copy; // show copy cursor
        }
        else
        {
            e.DragEffects = DragDropEffects.None;
        }

        e.Handled = true;
    }

    internal void FileGridOnDrop(object? sender, DragEventArgs e)
    {
        if (!e.DataTransfer.Contains(DataFormat.File))
        {
            return;
        }

        var files = e.DataTransfer.TryGetFiles();
        if (files != null)
        {
            Dispatcher.UIThread.Post(async () =>
            {
                await AddFiles(files.Select(p => p.Path.LocalPath).ToArray());
            });
        }
    }
}