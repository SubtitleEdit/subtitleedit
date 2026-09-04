using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.UiLogic.AudioToText;
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
using Nikse.SubtitleEdit.UiLogic;
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
using Nikse.SubtitleEdit.UiLogic.Media;
using Nikse.SubtitleEdit.UiLogic.Common;

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
    [ObservableProperty] private bool _addLanguageCodeToFileName;

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
    public TableView BatchGrid { get; internal set; }

    private bool _unknownArgument;
    private bool _cudaOutOfMemory;
    private bool _cudaComputeTypeNotSupported;
    private bool _incompleteModel;
    private string? _missingSharedLibrary;

    // Crisp ASR VAD state for the empty-result retry (#13911): whether the run that just
    // finished passed --vad, and whether it is itself the retry that leaves --vad off.
    private bool _crispAsrVadWasUsed;
    private bool _crispAsrVadSuppressed;
    private bool _loadedFromStdOut;
    private SpeechToTextQualityReport? _qualityReport;
    private string? _videoFileName;
    private string _audioFileName = string.Empty;
    private int _audioTrackNumber;

    // The file _audioTrackNumber was picked from. A stream index only means anything in its own
    // file, and batch mode reuses this view model for other videos - see GetFfmpegProcess.
    private string? _audioTrackVideoFileName;
    private readonly List<string> _filesToDelete = new();
    private string? _sttTempFolder;
    private readonly ConcurrentQueue<string> _outputText = new();
    private long _startTicks = 0;
    private double _endSeconds;
    private double _showProgressPct = -1;
    private readonly VideoInfo _videoInfo = new();
    private bool _abort;
    private CancellationTokenSource? _openAiCts;

    /// <summary>
    /// Language code reported by an online STT provider for the current run.
    /// Post-processing needs a language, and online engines have no entry in the
    /// shared language dropdown — so when the user leaves the per-engine hint
    /// empty (auto-detect), this is what keeps merge/split/line-breaking from
    /// being skipped entirely (issue #12860).
    /// </summary>
    private string? _onlineDetectedLanguage;

    private readonly List<ResultText> _resultList = new();
    private bool _useCenterChannelOnly;

    private readonly Regex _timeRegexShort =
        new(@"^\[\d\d:\d\d[\.,]\d\d\d --> \d\d:\d\d[\.,]\d\d\d\]", RegexOptions.Compiled);

    private readonly Regex _timeRegexLong =
        new(@"^\[\d\d:\d\d:\d\d[\.,]\d\d\d --> \d\d:\d\d:\d\d[\.,]\d\d\d]", RegexOptions.Compiled);

    private readonly Regex _pctWhisper = new(@"^\d+%\|", RegexOptions.Compiled);
    private readonly Regex _pctWhisperFaster = new(@"^\s*\d+%\s*\|", RegexOptions.Compiled);

    // Sentence chunks with trailing terminator (+ closing quotes/brackets), or a
    // final unterminated tail. Latin (. ! ?) and CJK (。！？…) terminators.
    private static readonly Regex SentenceRegex =
        new(@"[^.!?。！？…]*[.!?。！？…]+[""'”’)\]]*\s*|[^.!?。！？…]+$", RegexOptions.Compiled);
    private readonly System.Timers.Timer _timerWhisper = new();
    private Process _whisperProcess = new();
    private Process? _audioExtractProcess;
    private readonly System.Timers.Timer _timerAudioExtract = new();
    private Stopwatch _sw = new();
    private StringBuilder _ffmpegLog = new();
    private readonly Lock _lockObj = new();
    private int _batchIndex = -1;
    private IList<SpeechToTextJobItem> _jobItems = new List<SpeechToTextJobItem>(); // items for the current run: aliases BatchItems in batch mode, a standalone single item otherwise
    private string _error;
    private List<AudioClip>? _audioClips;
    private bool _audioClipsAutoStart;
    private string _qwen3AsrOutputJsonPath = string.Empty;
    private int? _engineExitCode;

    private readonly IWindowService _windowService;
    private readonly IFileHelper _fileHelper;
    private readonly IFolderHelper _folderHelper;
    private string? _batchOutputFolder;
    private bool _isUpdatingWhisperCppBackend;
    private bool _isUpdatingCrispAsrBackend;
    private static bool _crispAsrUpdatePromptShown;
    private static bool _whisperCppUpdatePromptShown;
    private static bool _qwen3AsrCppUpdatePromptShown;

    /// <summary>
    /// Hook the view wires up so the engine combobox can re-evaluate its install-status dots
    /// after a re-download. The combo's <c>FuncDataTemplate</c> snapshots the install state when
    /// each row is realised, so without an explicit refresh the dot stays on its old colour
    /// (typically amber) even though the sidecar now reports up-to-date.
    /// </summary>
    public Action? RefreshEngineCombo { get; set; }

    public SpeechToTextViewModel(IWindowService windowService, IFileHelper fileHelper, IFolderHelper folderHelper)
    {
        _windowService = windowService;
        _fileHelper = fileHelper;
        _folderHelper = folderHelper;

        Engines = [new WhisperCppEngine()];
        if (OperatingSystem.IsWindows())
        {
            Engines.Add(new WhisperEnginePurfviewFasterWhisperXxl());
            Engines.Add(new WhisperEngineConstMe());
        }
        else if (OperatingSystem.IsLinux() &&
                 RuntimeInformation.ProcessArchitecture == Architecture.X64)
        {
            // Purfview only ships Windows and Linux x86_64 builds - there is no Linux ARM64
            // binary, so don't offer the engine on Linux ARM (it could only fail to run).
            Engines.Add(new WhisperEnginePurfviewFasterWhisperXxl());
        }

        if (OperatingSystem.IsWindows() ||
            (OperatingSystem.IsMacOS() && RuntimeInformation.ProcessArchitecture == Architecture.Arm64) ||
            (OperatingSystem.IsLinux() && RuntimeInformation.ProcessArchitecture == Architecture.X64))
        {
            Engines.Add(new WhisperEngineCTranslate2());
        }

        // Same platform/architecture support as the standalone build published at
        // https://github.com/muaz978/subtitleedit-whisperx-standalone - only builds for
        // Windows x64 (not ARM64, which would silently get a mismatched x64 binary).
        if ((OperatingSystem.IsWindows() && RuntimeInformation.ProcessArchitecture == Architecture.X64) ||
            (OperatingSystem.IsMacOS() && RuntimeInformation.ProcessArchitecture == Architecture.Arm64) ||
            (OperatingSystem.IsLinux() && RuntimeInformation.ProcessArchitecture == Architecture.X64))
        {
            Engines.Add(new WhisperEngineWhisperX());
        }

        Engines.Add(new WhisperEngineOpenAi());

        // Add OpenAI Compatible STT engine (available on all platforms)
        Engines.Add(new OpenAiCompatibleSttEngine());

        // Online STT services reachable on all platforms
        Engines.Add(new OpenRouterSttEngine());
        Engines.Add(new DashScopeQwen3SttEngine());

        if (OperatingSystem.IsWindows() ||
            OperatingSystem.IsLinux() ||
            OperatingSystem.IsMacOS())
        {
            Engines.Add(new Qwen3AsrCppEngine());
        }

        Engines.Add(new CrispAsrEngine());

        SelectedEngine = Engines[0];

        Languages = new ObservableCollection<WhisperLanguage>(GetEngineLanguages(GetEffectiveSelectedEngine()));

        // Restore the last-used language (SE4 behavior, #11744) - EngineChanged keeps the
        // current selection on engine switches, so the initial selection must already be the
        // remembered one, not the English default.
        var savedLanguageCode = Se.Settings.Tools.AudioToText.WhisperLanguageCode;
        SelectedLanguage = (string.IsNullOrEmpty(savedLanguageCode)
                               ? null
                               : Languages.FirstOrDefault(p => p.Code == savedLanguageCode))
                           ?? PickDefaultLanguage(Languages);

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
        BatchGrid = new TableView();
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
        AddLanguageCodeToFileName = Se.Settings.Tools.AudioToText.WhisperAddLanguageCodeToFileName;

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
            var selectedEngine = Engines.FirstOrDefault(p => p.Choice == savedChoice);
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
        Se.Settings.Tools.AudioToText.WhisperAddLanguageCodeToFileName = AddLanguageCodeToFileName;
        var engine = GetEffectiveSelectedEngine();
        engine.CommandLineParameter = Parameters;
        Se.Settings.Tools.AudioToText.WhisperChoice = engine.Choice;
        // Keep the remembered model/language when the current engine simply doesn't show
        // those pickers (online STT engines null them out): overwriting with empty strings
        // lost the user's choice for good, so switching back fell to tiny/English.
        if (SelectedModel != null || IsModelSelectionVisible)
        {
            Se.Settings.Tools.AudioToText.WhisperModel = SelectedModel?.Model.Name ?? string.Empty;
        }

        if (SelectedLanguage != null || IsLanguageSelectionVisible)
        {
            Se.Settings.Tools.AudioToText.WhisperLanguageCode = SelectedLanguage?.Code ?? string.Empty;
        }
        // Only write when this window actually has an aligner selected. SaveSettings runs on
        // every EngineChanged, so with a non-CrispASR engine (SelectedForcedAligner == null) the
        // "?? built-in" fallback overwrote the aligner the user had chosen in Import plain text >
        // Forced aligner setup - the only place that reads this key back.
        if (SelectedForcedAligner != null)
        {
            Se.Settings.Tools.AudioToText.CrispAsrForcedAligner = SelectedForcedAligner.Choice;
        }

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
            or WhisperChoice.WhisperX
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
        return engine is not Qwen3AsrCppEngine and not ICrispAsrEngine and not IOnlineSttEngine;
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

        // Prefer what was saved - otherwise this window could never restore the user's own pick
        // either, since nothing here ever read the setting back.
        var match = ForcedAligners.FirstOrDefault(p => p.Choice == Se.Settings.Tools.AudioToText.CrispAsrForcedAligner)
                    ?? ForcedAligners.FirstOrDefault(p => p.Choice == preferredChoice)
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
                    partialSub.Paragraphs.AddRange(_resultList
                        .Select(p => new Paragraph(p.Text, (double)p.Start * 1000.0, (double)p.End * 1000.0)).ToList());

                    // Engine output is not guaranteed to be sorted or free of overlaps
                    // (issue #13548) - a kept partial must go through the same repair as
                    // a completed run, or the overlapping cues land in the document.
                    partialSub = SpeechToTextTimingFixer.SortAndRemoveOverlaps(partialSub);

                    if (!IsBatchMode && partialSub.Paragraphs.Count > 0)
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

                        // The user chose to keep the lines - clear the abort flag so
                        // MakeResult delivers them like a completed run instead of
                        // hitting its cancelled-branch, which discards the result.
                        _abort = false;
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

            // Grab the exit code before disposing - it is the key diagnostic when the engine
            // dies without producing output (a Qwen3 ASR GPU/Vulkan crash, issue #12815; a Crisp
            // ASR build that needs CPU instructions this machine lacks, issue #14038).
            try
            {
                _engineExitCode = _whisperProcess.ExitCode;
            }
            catch
            {
                _engineExitCode = null;
            }

            _whisperProcess.Dispose();

            var engine = GetEffectiveSelectedEngine();

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
                if (_missingSharedLibrary != null)
                {
                    await MessageBox.Show(Window!, "Speech to text engine could not start",
                        GetMissingSharedLibraryMessage(_missingSharedLibrary));
                    hasError = true;
                }
                else if (_incompleteModel)
                {
                    await MessageBox.Show(Window!, "Incomplete model",
                        "The model is incomplete. Please download the full model.");
                    hasError = true;
                }
                else if (_unknownArgument)
                {
                    // Report the parameters the engine actually ran with. This used to be
                    // gated on a global setting nothing ever writes, so a mistyped flag made
                    // the run fail with no message at all.
                    var badArgs = GetEffectiveSelectedEngine()?.CommandLineParameter ?? string.Empty;
                    await MessageBox.Show(Window!, $"Unknown argument: {badArgs}",
                        "Unknown argument. Please check the advanced settings.");
                    hasError = true;
                }
                else if (_cudaOutOfMemory)
                {
                    await MessageBox.Show(Window!, $"CUDA failed",
                        "Whisper ran out of CUDA memory - try a smaller model or run on CPU.");
                    hasError = true;
                }
                else if (_cudaComputeTypeNotSupported)
                {
                    await ShowCudaComputeTypeNotSupported(engine);
                    hasError = true;
                }

                if (!hasError && GetResultFromSrt(_audioFileName, _videoFileName!, out var resultTexts, _outputText, _filesToDelete))
                {
                    _loadedFromStdOut = false;
                    var subtitle = new Subtitle();
                    subtitle.Paragraphs.AddRange(resultTexts
                        .Select(p => new Paragraph(p.Text, (double)p.Start * 1000.0, (double)p.End * 1000.0)).ToList());

                    // The result file is engine output and is not guaranteed to be
                    // sorted or free of overlaps (issue #13548), so straighten the
                    // timings out before post-processing merges anything.
                    subtitle = SpeechToTextTimingFixer.SortAndRemoveOverlaps(subtitle);

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

                if (!hasError && _resultList.Count == 0 && RetryCrispAsrWithoutVad())
                {
                    return;
                }

                _outputText.Enqueue("Loading result from STDOUT");
                var transcribedSubtitleFromStdOut = new Subtitle();
                transcribedSubtitleFromStdOut.Paragraphs.AddRange(_resultList
                    .Select(p => new Paragraph(p.Text, (double)p.Start * 1000.0, (double)p.End * 1000.0)).ToList());
                transcribedSubtitleFromStdOut = SpeechToTextTimingFixer.SortAndRemoveOverlaps(transcribedSubtitleFromStdOut);
                _loadedFromStdOut = transcribedSubtitleFromStdOut.Paragraphs.Count > 0;
                await MakeResult(transcribedSubtitleFromStdOut);
            });
        }
    }

    /// <summary>
    /// Whether SE adds its own "--vad --vad-model ..." to a Crisp ASR command line.
    ///
    /// Mega-ASR (crispasr 0.6.10) silently writes a zero-byte SRT unless VAD chunking is enabled -
    /// the transcription log says it succeeded but no segments are emitted. Cohere gets the same
    /// treatment because crispasr auto-enables VAD for that backend on long audio anyway; passing
    /// the bundled Silero model keeps it from downloading its own copy into ~/.cache/crispasr
    /// mid-transcription.
    ///
    /// --chunk-seconds/-ck in the user's parameters means "no VAD, use fixed chunks" - that is
    /// crispasr's own documented way to switch its auto-VAD back off, and it is the only way to
    /// switch VAD off at all (--vad is a plain flag with no --no-vad). So it has to suppress our
    /// own --vad too, or the user has no opt-out (#13849).
    /// </summary>
    /// <param name="vadSuppressed">
    /// Set on the re-run of a job that came back empty with VAD on (#13911).
    /// </param>
    internal static bool ShouldForceCrispAsrVad(ISpeechToTextEngine engine, string? crispArgs, bool vadSuppressed)
    {
        if (engine is not (CrispAsrCohere or CrispAsrMega) || vadSuppressed)
        {
            return false;
        }

        return !Regex.IsMatch(crispArgs ?? string.Empty, @"(^|\s)(--vad|-vm|--vad-model|--chunk-seconds|-ck)\b");
    }

    /// <summary>
    /// Windows terminates a process that executes an instruction its CPU does not implement with
    /// STATUS_ILLEGAL_INSTRUCTION; Unix reports the SIGILL as 128+4.
    /// </summary>
    internal const int StatusIllegalInstruction = unchecked((int)0xC000001D);

    /// <summary>The shell convention for a child killed by SIGILL (128 + SIGILL).</summary>
    internal const int UnixSigill = 132;

    /// <summary>
    /// Explains a Crisp ASR run that produced nothing because the engine died, or null when the
    /// exit code says it did not - an empty result with a clean exit has some other cause and the
    /// generic message is the honest one.
    ///
    /// Worth spelling out because the failure is invisible: a process killed for an illegal
    /// instruction never reaches stdout, so SE sees a well-behaved engine that simply produced no
    /// subtitles and said so, which is all the user was told in #14038. The concrete case there
    /// was the crispasr v0.8.29 GPU packages, built with AVX-512 against a CI runner that had it
    /// (CrispASR #374) - every CPU without AVX-512 got this on the CUDA/Vulkan build while the CPU
    /// build ran fine, so naming the installed package is most of the answer. That build flaw is
    /// fixed from v0.8.30 (the current pin), but the message still earns its keep: a pre-AVX2 CPU
    /// hits the same silent death on the AVX2 CPU package, and an install predating the pin bump
    /// keeps the broken GPU binary until the user downloads the engine again.
    /// </summary>
    /// <param name="exitCode">The engine process exit code, or null when it could not be read.</param>
    /// <param name="variant">
    /// The installed Crisp ASR package ("cuda", "vulkan", "cpu", ...) as reported by
    /// <see cref="DownloadHashManager.GetCrispAsrVariant"/>, or null when it is not known.
    /// </param>
    internal static string? DescribeCrispAsrCrash(int? exitCode, string? variant)
    {
        if (exitCode is null or 0)
        {
            return null;
        }

        var code = $"exit code {exitCode.Value} (0x{(uint)exitCode.Value:X8})";
        if (exitCode.Value is not (StatusIllegalInstruction or UnixSigill))
        {
            return $"Crisp ASR crashed before producing any output ({code}).{Environment.NewLine}{Environment.NewLine}" +
                   "Please check the tools log for engine output.";
        }

        var isGpuBuild = variant is "cuda" or "cuda13" or "vulkan" or "hip";
        var advice = isGpuBuild
            ? $"The installed \"{variant}\" package needs a newer CPU than this one. Download the speech to text " +
              "engine again and choose the CPU build."
            : "Download the speech to text engine again and choose the CPU (legacy) build, which targets the " +
              "oldest CPUs.";

        return $"Crisp ASR was stopped for using CPU instructions this computer does not have ({code}, " +
               $"illegal instruction), so it never produced any output.{Environment.NewLine}{Environment.NewLine}" +
               advice;
    }

    /// <summary>
    /// The Crisp ASR package the user actually has installed, from the download sidecar. Best-effort:
    /// this only sharpens a diagnostic message, so an unreadable sidecar is not worth failing over.
    /// </summary>
    private static string? TryGetCrispAsrVariant(ICrispAsrEngine engine)
    {
        try
        {
            var sidecar = DownloadHashManager.TryReadSidecar(engine.GetAndCreateWhisperFolder());
            return sidecar == null ? null : DownloadHashManager.GetCrispAsrVariant(sidecar.Value.Key);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Re-runs a Crisp ASR job that produced nothing, this time without the VAD pass.
    ///
    /// SE forces --vad on for the Cohere and Mega backends because they otherwise write a
    /// zero-byte SRT on long audio. On a short clip that trade goes the other way: Silero can
    /// reject the whole clip as non-speech and the run ends with no segments at all, which is
    /// how "transcribe selected lines" ended up quietly leaving clips unconverted (#13911).
    /// Nothing is lost by trying again without it - the alternative is the empty result we
    /// already have - and the retry is a one-shot: the second run has VAD suppressed, so it
    /// cannot ask for a third.
    /// </summary>
    /// <returns>True when a retry was started and this result should be dropped.</returns>
    private bool RetryCrispAsrWithoutVad()
    {
        // _crispAsrVadWasUsed is only ever set by the Crisp ASR branch of GetWhisperProcess and is
        // cleared at the start of every attempt, so it doubles as "this was a Crisp ASR VAD run".
        if (!_crispAsrVadWasUsed || _crispAsrVadSuppressed || _abort)
        {
            return false;
        }

        if (_videoFileName == null)
        {
            return false;
        }

        // Nothing is extracted when the source already is a 16 kHz wav - which is exactly what
        // "transcribe selected lines" hands over - and _audioFileName is blank in that case, so
        // the engine input is the source file itself. Same fallback GetResultFromSrt makes.
        var inputFileName = string.IsNullOrEmpty(_audioFileName) ? _videoFileName : _audioFileName;
        if (!File.Exists(inputFileName))
        {
            return false;
        }

        LogToConsole($"No speech found with VAD - trying again without it{Environment.NewLine}");
        Se.WriteToolsLog($"Crisp ASR produced no segments for \"{inputFileName}\" with VAD; retrying without VAD");

        return TranscribeViaWhisper(inputFileName, _videoFileName, retryWithoutCrispAsrVad: true);
    }

    /// <summary>
    /// Shown when cuBLAS refuses the compute type the engine picked and the run dies inside
    /// encode() without producing a single segment (issue #13902). The cure is always the same -
    /// force fp16 - so offer to add the parameter instead of only naming it in a message.
    /// </summary>
    private async Task ShowCudaComputeTypeNotSupported(ISpeechToTextEngine engine)
    {
        const string title = "cuBLAS failed";
        const string computeTypeArgument = "--compute_type float16";
        var nl = Environment.NewLine;
        var cause =
            "The GPU could not run the model with the compute type the engine picked - cuBLAS " +
            $"returned CUBLAS_STATUS_NOT_SUPPORTED, and no text was transcribed.{nl}{nl}";

        // Only the faster-whisper based engines take --compute_type; suggesting it anywhere else
        // would just trade this error for "unrecognized argument".
        if (!SupportsComputeTypeParameter(engine))
        {
            await MessageBox.Show(Window!, title,
                cause + "Try another model, another engine, or run on CPU.");
            return;
        }

        var parameters = Parameters ?? string.Empty;
        if (parameters.Contains("--compute_type", StringComparison.OrdinalIgnoreCase))
        {
            // Only the floating point types are worth naming: on the RTX 50 series - the cards
            // this error shows up on most - every int8 variant fails with this exact cuBLAS
            // error, so listing "int8" sent the user straight back to the same dialog
            // (Purfview/whisper-standalone-win#403, OpenNMT/CTranslate2#1865).
            await MessageBox.Show(Window!, title,
                cause + "The parameters already set \"--compute_type\" - try another value, " +
                "such as \"float16\", \"float32\", or \"bfloat16\" - the \"int8\" types fail " +
                "this way on many newer GPUs.");
            return;
        }

        var answer = await MessageBox.Show(Window!, title,
            cause + $"Adding \"{computeTypeArgument}\" to the parameters normally fixes this.{nl}{nl}" +
            "Add it now?",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);

        if (answer != MessageBoxResult.Yes)
        {
            return;
        }

        Parameters = string.IsNullOrWhiteSpace(parameters)
            ? computeTypeArgument
            : parameters.Trim() + " " + computeTypeArgument;
        engine.CommandLineParameter = Parameters;
        SaveSettings();
    }

    /// <summary>
    /// True for the engines built on faster-whisper/CTranslate2, which are the ones that both
    /// accept "--compute_type" and can hit the cuBLAS compute type error in the first place.
    /// </summary>
    private static bool SupportsComputeTypeParameter(ISpeechToTextEngine engine)
    {
        return engine.Choice is WhisperChoice.PurfviewFasterWhisperXxl
            or WhisperChoice.CTranslate2
            or WhisperChoice.WhisperX;
    }

    /// <summary>
    /// Builds the message shown when the engine binary could not be started because a shared
    /// library it links against is missing (issue #12970).
    /// </summary>
    private static string GetMissingSharedLibraryMessage(string libraryName)
    {
        // Libraries that ship inside the engine download itself. Telling the user to install
        // these with their package manager is a dead end - no distro packages them, and the
        // real cause is a bad or incomplete engine folder (issue #13680).
        if (MissingSharedLibrary.IsBundledWithEngine(libraryName))
        {
            return
                $"The speech to text engine could not start - the shared library \"{libraryName}\" is missing.{Environment.NewLine}{Environment.NewLine}" +
                "This library is part of the engine download, so the installed engine is incomplete." +
                $"{Environment.NewLine}{Environment.NewLine}" +
                "Re-download the engine (Download button next to the engine) and try again.";
        }

        var message =
            $"The speech to text engine could not start - the shared library \"{libraryName}\" is missing.{Environment.NewLine}{Environment.NewLine}" +
            "Install it with your package manager and try again.";

        if (libraryName.StartsWith("libopenblas", StringComparison.OrdinalIgnoreCase))
        {
            message +=
                $"{Environment.NewLine}{Environment.NewLine}" +
                $"Debian/Ubuntu: sudo apt install libopenblas0{Environment.NewLine}" +
                $"Fedora: sudo dnf install openblas{Environment.NewLine}" +
                "Arch: sudo pacman -S openblas";
        }

        return message;
    }

    private void ProcessQwen3AsrCppTranscription(SeAudioToText settings)
    {
        var jsonPath = _qwen3AsrOutputJsonPath;
        if (string.IsNullOrEmpty(jsonPath) || !File.Exists(jsonPath))
        {
            var exitCode = _engineExitCode;
            var isVulkan = false;
            try
            {
                var folder = new Qwen3AsrCppEngine().GetAndCreateWhisperFolder();
                isVulkan = DownloadHashManager.IsQwen3AsrCppVulkanInstall(folder);
            }
            catch
            {
                // best-effort diagnostics only
            }

            // The engine ran but produced no output JSON - almost always a native crash of the
            // GPU (Vulkan) build. Record the exit code (0xC0000005-style values indicate a hard
            // crash) and force it to the tools log, since that setting is off by default and
            // otherwise there is no record of why it failed (issue #12815).
            var exitCodeText = exitCode.HasValue
                ? $"exit code {exitCode.Value} (0x{(uint)exitCode.Value:X8})"
                : "exit code unavailable";
            Se.WriteToolsLog($"Qwen3 ASR CPP produced no output JSON; {exitCodeText}; Vulkan build: {isVulkan}", true);

            Dispatcher.UIThread.Invoke<Task>(async () =>
            {
                LogToConsole($"Speech to text ({settings.WhisperChoice}) done in {_sw.Elapsed}{Environment.NewLine}");
                if (_missingSharedLibrary != null)
                {
                    await MessageBox.Show(Window!, "Speech to text engine could not start",
                        GetMissingSharedLibraryMessage(_missingSharedLibrary));
                    ProgressValue = 100;
                    IsTranscribeEnabled = true;
                    return;
                }

                if (_unknownArgument)
                {
                    var badArgs = GetEffectiveSelectedEngine()?.CommandLineParameter ?? string.Empty;
                    await MessageBox.Show(Window!, $"Unknown argument: {badArgs}",
                        "Unknown argument. Please check the advanced settings.");
                }
                LogToConsole($"Speech to text: Could not find output JSON file ({exitCodeText}){Environment.NewLine}");
                if (exitCode is not (null or 0))
                {
                    var url = new Qwen3AsrCppEngine().Url;
                    LogToConsole(isVulkan
                        ? $"The Qwen3 ASR GPU (Vulkan) engine crashed before producing output. Try the CPU build instead (re-download the engine and choose CPU), or report it at {url}{Environment.NewLine}"
                        : $"The Qwen3 ASR engine crashed before producing output. Try re-running, a different model, or report it at {url}{Environment.NewLine}");
                }
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
            // Log the raw engine output on every run, not only on failure - the temp .json is
            // deleted below, and bug reports for "the timings are off"-class issues (#11375)
            // need the actual data, not just the crash cases. Respects the tools-log setting.
            Se.WriteToolsLog($"Qwen3 ASR CPP output JSON ('{jsonPath}'):{Environment.NewLine}{rawJson}");
            // qwen3-asr-cli can write raw control chars (e.g. a literal newline) inside JSON
            // string values, which strict System.Text.Json rejects ("'0x0A' is invalid within a
            // JSON string"). Escape those so a result is still produced (issue #11717).
            // Engines up to v0.1.7 also wrote locale-formatted timestamps on Windows with a
            // comma-decimal regional format ("start": 1,840 — French/German/...); fixed at the
            // source in v0.1.8, but repair it here too so installs that skip the engine update
            // still get a result.
            var jsonText = JsonRepair.FixCommaDecimalSeparators(JsonRepair.EscapeControlCharsInStrings(rawJson));
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

        _onlineDetectedLanguage = null;

        var transcriber = engine.CreateTranscriber(out var configError);
        if (transcriber == null)
        {
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await MessageBox.Show(Window!,
                    Se.Language.General.ConfigurationRequired,
                    configError ?? Se.Language.General.OpenAiCompatibleSttUrlMissing);
                FailCurrentOnlineSttJob();
            });
            return;
        }

        ProgressText = Se.Language.Video.AudioToText.Transcribing;
        ProgressValue = 5;

        string? probeError;
        try
        {
            probeError = await ProbeOpenAiUrlAsync(engine.ProbeUrl, cancellationToken);
            if (probeError != null)
            {
                LogToConsole($"Online STT endpoint probe failed: {probeError}. Retrying...");
                // Brief delay so transient DNS/socket hiccups have a chance to recover before retrying.
                await Task.Delay(300, cancellationToken);
                probeError = await ProbeOpenAiUrlAsync(engine.ProbeUrl, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // This runs as a fire-and-forget task, so a cancel during the probe phase
            // (up to two 8-second probes) must reset the UI here — nothing downstream
            // will, and the window would otherwise be stuck on a live progress bar.
            LogToConsole("Transcription cancelled by user");
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                IsTranscribeEnabled = true;
                HideProgressBar();
            });
            return;
        }

        if (probeError != null)
        {
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await MessageBox.Show(Window!,
                    Se.Language.General.TranscriptionError,
                    string.Format(Se.Language.General.OpenAiCompatibleSttUrlNotResponding, probeError));
                FailCurrentOnlineSttJob();
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
                RememberDetectedLanguage(response);
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
                    FailCurrentOnlineSttJob();
                    HideProgressBar();
                });
            }
        }
        catch (HttpRequestException ex)
        {
            // Log the full exception before simplifying the dialog text — the
            // response body (e.g. DashScope's error code, or xAI's "check the
            // URL" for a wrong endpoint path) is the only clue to what actually
            // failed. Force the write: "write tools log" is off by default, so
            // without it the server's answer is gone the moment the user closes
            // the dialog (issue #12860).
            Se.WriteToolsLog($"Online STT transcription failed ({engine.Name}): {ex}", true);

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
            else if (IsModelRejectedByServer(ex, engine))
            {
                message += Environment.NewLine + Environment.NewLine + Se.Language.General.OpenAiCompatibleSttModelRejectedHint;
            }

            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await MessageBox.Show(Window!, Se.Language.General.TranscriptionError, message);
                FailCurrentOnlineSttJob();
            });
        }
        catch (TimeoutException ex)
        {
            // Raised by the online STT services when their own timeout fires
            // (distinct from a user cancel, which arrives as
            // OperationCanceledException above).
            Se.WriteToolsLog($"Online STT transcription timed out ({engine.Name}): {ex.Message}", true);

            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await MessageBox.Show(Window!, Se.Language.General.TranscriptionError, Se.Language.General.RequestTimeout);
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
            Se.WriteToolsLog($"Online STT transcription failed ({engine.Name}): {ex}", true);

            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await MessageBox.Show(Window!, Se.Language.General.TranscriptionError, $"{Se.Language.General.TranscriptionFailed}: {ex.Message}");
                FailCurrentOnlineSttJob();
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

        var matches = SentenceRegex.Matches(text.Trim());
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

                RememberDetectedLanguage(chunkResponse);
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

    private string GetProgressText()
    {
        if (IsBatchMode)
        {
            return string.Format(Se.Language.Video.AudioToText.TranscribingXOfY, _batchIndex + 1, _jobItems.Count);
        }
        else
        {
            return Se.Language.Video.AudioToText.Transcribing;
        }
    }

    private void StartNext(Subtitle? transcribedSubtitle)
    {
        var currentItem = _jobItems[_batchIndex];
        if (transcribedSubtitle != null && transcribedSubtitle.Paragraphs.Count > 0)
        {
            currentItem.Status = Se.Language.General.Converted;
            var languageCode = AddLanguageCodeToFileName ? GetFileNameLanguageCode(transcribedSubtitle) : null;
            var subtitleFileName = GetSubtitleFileName(currentItem.InputVideoFileName, languageCode, _batchOutputFolder);
            var format = new SubRip();
            var text = format.ToText(transcribedSubtitle, string.Empty);
            File.WriteAllText(subtitleFileName, text);
            LastBatchSubtitleFileName = subtitleFileName;
        }

        // Delete temp files from the just-finished item so disk usage doesn't grow across long batches
        DeleteTempFiles();
        _filesToDelete.Clear();

        _batchIndex++;
        if (_batchIndex < _jobItems.Count)
        {
            ProgressValue = 0;
            _startTicks = 0;
            _endSeconds = 0;
            _showProgressPct = -1;
            _outputText.Clear();
            ConsoleLog = string.Empty;
            ProgressText = string.Empty;
            ElapsedText = string.Empty;
            EstimatedText = string.Empty;

            var jobItem = _jobItems[_batchIndex];
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
                BatchGrid.ScrollIntoView(jobItem);
            });

            var startGenerateAudioFileOk = GenerateAudioFile(_videoFileName, _audioTrackNumber);
            if (!startGenerateAudioFileOk)
            {
                // Nothing was started, so no timer will ever fire for this item -
                // without this the batch just stalls with a frozen progress bar.
                // Mark the item failed and move on; the closing summary reports it.
                jobItem.Status = Se.Language.General.Error;
                StartNext(null);
            }

            return;
        }

        var convertedJobs = _jobItems.Count(p => p.Status == Se.Language.General.Converted);
        var failed = _jobItems.Count(p => p.Status != Se.Language.General.Converted);

        Dispatcher.UIThread.Invoke<Task>(async () =>
        {
            var msg = $"Videos converted: " + convertedJobs;
            if (failed > 0)
            {
                msg += Environment.NewLine + $"Videos failed: " + failed +
                       Environment.NewLine + "Please check the tools log for details.";
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

    public static string GetSubtitleFileName(string videoFileName, string? languageCode, string? outputFolder = null)
    {
        // For document portal video paths the output goes to the folder picked in
        // Transcribe() - only the granted video file name itself can exist in such a folder.
        var path = !string.IsNullOrEmpty(outputFolder) && DocumentPortal.IsPortalPath(videoFileName)
            ? outputFolder
            : Path.GetDirectoryName(videoFileName);
        var fileName = Path.GetFileNameWithoutExtension(videoFileName);
        // "video.en.srt" style - the language token must stay right before the
        // extension for media players to pick it up, so the collision counter
        // goes on the base name: "video_2.en.srt".
        var languagePart = string.IsNullOrWhiteSpace(languageCode) ? string.Empty : "." + languageCode;
        var extension = ".srt";
        var subtitleFileName = Path.Combine(path!, fileName + languagePart + extension);
        int count = 2;
        while (File.Exists(subtitleFileName))
        {
            subtitleFileName = Path.Combine(path!, fileName + "_" + count + languagePart + extension);
            count++;
        }

        return subtitleFileName;
    }

    /// <summary>
    /// The language code to embed in a generated subtitle file name ("video.en.srt"),
    /// or null when no usable code can be determined. Resolution mirrors PostProcess:
    /// selected language, then the online engine's configured hint, then auto-detection
    /// on the transcript itself - but "auto" is never usable as a file name token.
    /// </summary>
    private string? GetFileNameLanguageCode(Subtitle? transcript)
    {
        if (DoTranslateToEnglish)
        {
            return "en";
        }

        var languageCode = SelectedLanguage?.Code;
        if (string.IsNullOrWhiteSpace(languageCode) || languageCode.Equals("auto", StringComparison.OrdinalIgnoreCase))
        {
            // Normalized here (not just at the end) so a hint that can't be mapped to a
            // code falls through to auto-detection instead of being dropped outright.
            languageCode = NormalizeFileNameLanguageCode(GetOnlineEngineLanguageHint());
        }

        if (string.IsNullOrWhiteSpace(languageCode) && transcript != null)
        {
            languageCode = LanguageAutoDetect.AutoDetectGoogleLanguageOrNull(transcript);
        }

        if (string.IsNullOrWhiteSpace(languageCode) || languageCode.Equals("auto", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return NormalizeFileNameLanguageCode(languageCode);
    }

    /// <summary>
    /// Maps a language token to a code usable as a file name part, or null when it can't be.
    /// The token may come from the online engines' free-text "language hint" setting, so it
    /// can be a full name ("English" - the APIs accept those) or any arbitrary text; a full
    /// name is mapped to its whisper code and anything not code-shaped is dropped rather than
    /// embedded in the file name (path separators and the like would make the save throw).
    /// </summary>
    internal static string? NormalizeFileNameLanguageCode(string? languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            return null;
        }

        var token = languageCode.Trim();
        var match = WhisperLanguage.Languages.FirstOrDefault(p =>
            p.Code.Equals(token, StringComparison.OrdinalIgnoreCase) ||
            p.Name.Equals(token, StringComparison.OrdinalIgnoreCase));
        if (match != null)
        {
            return match.Code;
        }

        // Unknown but code-shaped ("pt-BR", "yue") - keep as typed, lowercased.
        if (token.Length <= 6 && token.All(c => char.IsAsciiLetter(c) || c == '-'))
        {
            return token.ToLowerInvariant();
        }

        return null;
    }

    private Subtitle PostProcess(Subtitle transcript)
    {
        if (GetEffectiveSelectedEngine() is ICrispAsrEngine &&
            SelectedModel is SpeechToTextModelDisplay { Model.Name: { } modelName } &&
            CrispAsrParakeet.IsPureCtcModel(modelName))
        {
            // The Vietnamese Parakeet CTC tokenizer has a space-prefixed "▁," / "▁." piece that the
            // model prefers over the bare one, so its transcripts read "gần xe , và ... dàng ." -
            // a training-text habit, not a decode bug, and not optional post-processing either.
            transcript = SpeechToTextPostProcessor.RemoveSpaceBeforePunctuation(transcript);
        }

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
            // Last resort: read the language off the transcript itself. Providers
            // that never report one leave the hint fallback empty - xAI's /v1/stt
            // documents its "language" response field as "currently empty" - and
            // without a code the whole post-processing step is skipped (#12877).
            languageCode = LanguageAutoDetect.AutoDetectGoogleLanguageOrNull(transcript);
        }

        if (string.IsNullOrWhiteSpace(languageCode))
        {
            return transcript;
        }

        if (DoAdjustTimings || DoPostProcessing)
        {
            ProgressText = Se.Language.Video.PostProcessing;
        }

        var postProcessor = new SpeechToTextPostProcessor(DoTranslateToEnglish ? "en" : languageCode)
        {
            ParagraphMaxChars = Configuration.Settings.General.SubtitleLineMaximumLength * 2,
            RemoveNonSpeechLines = Se.Settings.Tools.AudioToText.WhisperPostProcessingRemoveNonSpeechLines,
            RemoveRepeatedLines = Se.Settings.Tools.AudioToText.WhisperPostProcessingRemoveRepeatedLines,
            // The engine's own parameters, so word-highlighted output is left alone by the
            // merge/split steps (they are stored per engine, not in one global setting).
            EngineCommandLineArguments = GetEffectiveSelectedEngine()?.CommandLineParameter ?? string.Empty,
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

        // Keep the report for MakeResult (shown once the run is done) and log a
        // one-line summary so batch runs leave a trace too (issue #13973).
        _qualityReport = postProcessor.QualityReport;
        LogToConsole(_qualityReport.ToLogString());

        return transcript;
    }

    /// <summary>
    /// The language hint configured for the selected online STT engine, or null
    /// for local engines. Online engines don't use the shared language dropdown
    /// (it's nulled out), so post-processing reads the per-engine setting instead
    /// — falling back to the language the provider detected when that setting is
    /// left empty.
    /// </summary>
    private string? GetOnlineEngineLanguageHint()
    {
        var configured = GetEffectiveSelectedEngine() switch
        {
            OpenRouterSttEngine => Se.Settings.Tools.OpenRouterSttLanguage,
            DashScopeQwen3SttEngine => Se.Settings.Tools.DashScopeSttLanguage,
            OpenAiCompatibleSttEngine => Se.Settings.Tools.OpenAiCompatibleSttLanguage,
            _ => null,
        };

        if (configured == null || !string.IsNullOrWhiteSpace(configured))
        {
            return configured;
        }

        // Hint left empty (auto-detect): use whatever language the provider said
        // it recognized, so post-processing still runs (issue #12860).
        return _onlineDetectedLanguage;
    }

    /// <summary>
    /// Note the language an online provider reported for this run. Providers are
    /// inconsistent here — OpenAI's verbose_json says "english" while others send
    /// "en" — so a full name is mapped back to its whisper language code. The
    /// first non-empty value wins; later chunks don't overwrite it.
    /// </summary>
    private void RememberDetectedLanguage(OpenAiCompatibleSttResponse response)
    {
        if (!string.IsNullOrEmpty(_onlineDetectedLanguage) || string.IsNullOrWhiteSpace(response.Language))
        {
            return;
        }

        var reported = response.Language.Trim();
        if (reported.Length == 2)
        {
            _onlineDetectedLanguage = reported.ToLowerInvariant();
            return;
        }

        var match = WhisperLanguage.Languages.FirstOrDefault(p =>
            p.Name.Equals(reported, StringComparison.OrdinalIgnoreCase) ||
            p.Code.Equals(reported, StringComparison.OrdinalIgnoreCase));
        if (match != null)
        {
            _onlineDetectedLanguage = match.Code;
        }
    }

    /// <summary>
    /// True when a failed OpenAI-compatible STT request looks like the server
    /// turning down the configured model name. Endpoints differ on this: xAI's
    /// /v1/stt has no "model" parameter and answers 404 for any value sent,
    /// while others accept only their own ids - so the fix is to clear the
    /// field, which isn't something the raw server message says (issue #12877).
    /// </summary>
    private static bool IsModelRejectedByServer(HttpRequestException ex, ISpeechToTextEngine engine)
    {
        if (engine is not OpenAiCompatibleSttEngine)
        {
            return false;
        }

        var model = Se.Settings.Tools.OpenAiCompatibleSttModel;
        if (string.IsNullOrWhiteSpace(model))
        {
            return false;
        }

        if (ex.StatusCode is not { } status || (int)status < 400 || (int)status >= 500)
        {
            return false;
        }

        // The message holds the server's response body, so an error naming the
        // model we sent - or just talking about models at all - points at the field.
        return ex.Message.Contains(model.Trim(), StringComparison.OrdinalIgnoreCase) ||
               ex.Message.Contains("model", StringComparison.OrdinalIgnoreCase);
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
            // One ffmpeg per batch item; the sibling _whisperProcess is disposed, and the
            // text-to-speech twin uses "using var" for the same call.
            using var process = GetFfmpegProcess(_videoFileName, _audioTrackNumber, targetFile);
            if (process == null)
            {
                return null;
            }

#pragma warning disable CA1416
            process.Start();
#pragma warning restore CA1416

            process.WaitForExit();

            // check for delay in matroska files
            var delayInMilliseconds = 0;
            if (_videoFileName.EndsWith(".mkv", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    using var matroska = new MatroskaFile(_videoFileName);
                    if (matroska.IsValid)
                    {
                        var firstAudioTrack = matroska.GetTracks().FirstOrDefault(track => track.IsAudio);
                        if (firstAudioTrack != null)
                        {
                            delayInMilliseconds =
                                (int)matroska.GetAudioTrackDelayMilliseconds(firstAudioTrack.TrackNumber);
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
        var srtCandidates = GetResultFileCandidates(".srt", waveFileName, videoFileName, whisperFolder, outputText, _sttTempFolder);
        var vttCandidates = GetResultFileCandidates(".vtt", waveFileName, videoFileName, whisperFolder, outputText, _sttTempFolder);
        var assaCandidates = GetResultFileCandidates(".ass", waveFileName, videoFileName, whisperFolder, outputText, _sttTempFolder);

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

    internal static List<string> GetResultFileCandidates(string ext, string waveFileName, string videoFileName, string whisperFolder, ConcurrentQueue<string> outputText, string? sttTempFolder = null)
    {
        var candidates = new List<string>
        {
            waveFileName + ext,
            // The engines that read the source file directly are told to write into the extracted
            // WAV's own per-run folder, and they name the output after their input - so the result
            // is "<run folder>/<video name><ext>", which none of the other candidates covers.
            Path.Combine(Path.GetDirectoryName(waveFileName) ?? string.Empty, Path.GetFileNameWithoutExtension(videoFileName) + ext),
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

        if (!string.IsNullOrEmpty(sttTempFolder))
        {
            // A pre-extracted 16 kHz WAV skips extraction, so the engines' contained output lands
            // in the per-run folder under the USER'S file name - no other candidate covers that.
            // The per-run folder is where SE told the engine to write, so it must be probed BEFORE
            // the folder of the user's own file: with the wave-dir candidate first, a stale
            // "<name>.srt" already sitting next to the user's WAV was picked up as the result and
            // then deleted as one of SE's temp files.
            candidates.Insert(0, Path.Combine(sttTempFolder, Path.GetFileNameWithoutExtension(waveFileName) + ext));
            candidates.Insert(0, Path.Combine(sttTempFolder, Path.GetFileNameWithoutExtension(videoFileName) + ext));
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

        // Purfview XXL announces where it wrote its result ("Subtitles are written to '<dir>'
        // directory."). When the user's own parameters carry an "--output_dir" (e.g. "-o source"),
        // SE does not pass its per-run folder, so the result can land somewhere none of the fixed
        // candidates cover - e.g. next to the user's video (issue #13505). The announced folder is
        // authoritative, so probe it first; the file is named after the engine's input.
        var dirFromOutput = TryFindOutputDirInOutput(outputText);
        if (!string.IsNullOrEmpty(dirFromOutput))
        {
            candidates.Insert(0, Path.Combine(dirFromOutput, Path.GetFileNameWithoutExtension(waveFileName) + ext));
            if (!string.IsNullOrEmpty(videoFileName))
            {
                candidates.Insert(0, Path.Combine(dirFromOutput, Path.GetFileNameWithoutExtension(videoFileName) + ext));
            }
        }

        return candidates;
    }

    private static string? TryFindOutputDirInOutput(ConcurrentQueue<string> outputText)
    {
        const string findText = "Subtitles are written to '";
        foreach (var line in outputText)
        {
            var idx = line.IndexOf(findText, StringComparison.OrdinalIgnoreCase);
            if (idx < 0)
            {
                continue;
            }

            var start = idx + findText.Length;
            var end = line.LastIndexOf('\'');
            if (end > start)
            {
                var dir = line.Substring(start, end - start).Trim();
                if (Directory.Exists(dir))
                {
                    return dir;
                }
            }
        }

        return null;
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

        // A crashed engine leaves the same empty result as a clean run that found no speech, and
        // the exit code is the only thing that tells them apart (#14038). Resolved here rather than
        // in the dialog branch below so a batch run - which returns before any of that - still
        // records why the file came back empty.
        string? crispAsrCrash = null;
        if (!anyLinesTranscribed && GetEffectiveSelectedEngine() is ICrispAsrEngine crispAsrEngine)
        {
            crispAsrCrash = DescribeCrispAsrCrash(_engineExitCode, TryGetCrispAsrVariant(crispAsrEngine));
            if (crispAsrCrash != null)
            {
                Se.WriteToolsLog(crispAsrCrash, true);
            }
        }

        if (_abort)
        {
            // User cancelled mid-run. Leave the dialog open so they can adjust
            // settings and retry (or close it themselves) instead of yanking it
            // out from under them. Checked before the batch branch: cancelling a
            // batch must stop the whole batch, not skip to the next item.
            IsTranscribeEnabled = true;
            HideProgressBar();
        }
        else if (IsBatchMode)
        {
            StartNext(transcribedSubtitle);
            return;
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
                await ShowQualityReport();
                OkPressed = anyLinesTranscribed;
                TranscribedSubtitle = transcribedSubtitle ?? new Subtitle();
                Window?.Close();
            }
            else if (GetEffectiveSelectedEngine() is ICrispAsrEngine)
            {
                await MessageBox.Show(Window!, "No transcription result",
                    crispAsrCrash ??
                    "Crisp ASR finished without generating subtitles. Please check the tools log for engine output.");

                if (Window != null)
                {
                    FileHelper.OpenFileWithDefaultProgram(Se.GetToolsLogFilePath());
                }
            }
        }
    }

    /// <summary>
    /// Tell the user what post-processing found (issue #13973) before the dialog
    /// closes. Only for the single-file flow - batch runs log the summary instead.
    /// </summary>
    private async Task ShowQualityReport()
    {
        var report = _qualityReport;
        _qualityReport = null;
        if (report == null || !report.HasIssues || !Se.Settings.Tools.AudioToText.WhisperPostProcessingShowQualityReport || Window == null)
        {
            return;
        }

        var vm = await _windowService.ShowDialogAsync<SpeechToTextQualityReportWindow, SpeechToTextQualityReportViewModel>(
            Window, viewModel => viewModel.Initialize(report));

        if (vm.DoNotShowAgain)
        {
            Se.Settings.Tools.AudioToText.WhisperPostProcessingShowQualityReport = false;
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

                // Tell the user - writing to the tools log only left the run looking
                // frozen: the progress indicator stayed up and no dialog appeared (#13621).
                var exitCode = _audioExtractProcess.ExitCode;
                _audioExtractProcess = null;

                if (IsBatchMode)
                {
                    // One unreadable file must not sink the whole batch: mark this job
                    // failed and move on, exactly like a job whose engine produced no
                    // text (MakeResult -> StartNext(null)). The closing summary reports
                    // the failure count, so nothing is swallowed.
                    if (_batchIndex >= 0 && _batchIndex < _jobItems.Count)
                    {
                        _jobItems[_batchIndex].Status = Se.Language.General.Error;
                    }

                    StartNext(null);
                    return;
                }

                IsTranscribeEnabled = true;
                HideProgressBar();
                ProgressText = string.Empty;

                Dispatcher.UIThread.Post(async () =>
                {
                    await MessageBox.Show(Window!, Se.Language.General.Error,
                        $"Could not generate the audio file (ffmpeg exit code {exitCode})." +
                        Environment.NewLine + "Please check the tools log for the ffmpeg output.");

                    if (Window != null)
                    {
                        FileHelper.OpenFileWithDefaultProgram(Se.GetToolsLogFilePath());
                    }
                });

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
                ProgressOpacity = 0;
                Dispatcher.UIThread.Invoke(async () => { await ShowUnableToStartEngineErrorAsync(); });
            }
        }
    }

    /// <summary>
    /// Shown when the speech-to-text engine process could not be started. If the engine's
    /// executable has disappeared after the install check that ran when transcribe was
    /// clicked - typically quarantined by antivirus software (#12220) - offer to
    /// re-download the engine instead of only echoing the raw exception message.
    /// </summary>
    private async Task ShowUnableToStartEngineErrorAsync()
    {
        var error = _error;
        _error = string.Empty;

        var engine = GetEffectiveSelectedEngine();
        if (engine.CanBeDownloaded() && !engine.IsEngineInstalled())
        {
            var answer = await MessageBox.Show(
                Window!,
                $"Unable to start {engine.Name}",
                $"The {engine.Name} program file was not found:{Environment.NewLine}" +
                $"{engine.GetExecutable()}{Environment.NewLine}{Environment.NewLine}" +
                $"It may have been deleted or quarantined by antivirus software - if so, please restore it or add an exclusion for the folder.{Environment.NewLine}{Environment.NewLine}" +
                $"Do you want to re-download \"{engine.Name}\"?",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (answer == MessageBoxResult.Yes)
            {
                await ReDownloadWhisperEngine();
            }

            return;
        }

        if (string.IsNullOrEmpty(error))
        {
            await MessageBox.Show(Window!, Se.Language.General.UnknownError, $"Unable to start {engine.Name}!");
        }
        else
        {
            await MessageBox.Show(Window!, "Error", $"Unable to start {engine.Name}: {error}");
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
            var windowsVariant = await PromptCrispAsrWindowsVariantAsync(CrispAsrEngine.StaticName);
            if (windowsVariant == null)
            {
                return;
            }
            crispVariant = windowsVariant;
        }
        else if (engine is ICrispAsrEngine
                 && OperatingSystem.IsLinux()
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
    /// Windows build prompt: CPU (with a standard/legacy follow-up), Vulkan (with a
    /// Vulkan-SDK warning when none is detected) or CUDA. Returns the variant string
    /// for the download dialog, or null when the user cancels.
    /// </summary>
    private async Task<string?> PromptCrispAsrWindowsVariantAsync(string engineName)
    {
        var answer = await MessageBox.Show(
            Window!,
            $"Download {engineName}?",
            $"{Environment.NewLine}\"{engineName}\" requires downloading the CrispASR engine.{Environment.NewLine}{Environment.NewLine}Select a version to download:",
            MessageBoxButtons.Cancel,
            MessageBoxIcon.Question,
            "CPU",
            "Vulkan",
            "CUDA");

        if (answer == MessageBoxResult.None || answer == MessageBoxResult.Cancel)
        {
            return null;
        }

        var crispVariant = answer switch
        {
            MessageBoxResult.Custom1 => "cpu",
            MessageBoxResult.Custom3 => "cuda",
            _ => "vulkan",
        };

        if (crispVariant == "cuda")
        {
            // Upstream added a Windows CUDA 13 build alongside the CUDA 12 one in v0.8.31, so
            // Windows now gets the same follow-up Linux has had since v0.8.30 (#14343).
            return await PromptCrispAsrCudaVersionAsync();
        }

        if (crispVariant == "cpu")
        {
            return await PromptCrispAsrCpuFlavorAsync();
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
                return null;
            }

            if (vulkanAnswer != MessageBoxResult.Yes)
            {
                return null;
            }
        }

        return crispVariant;
    }

    /// <summary>
    /// Linux x86_64 build prompt: CPU, Vulkan (any GPU), CUDA (NVIDIA) or ROCm (AMD).
    /// Returns "vulkan" / "cuda" / "cuda13" / "hip", empty string for the default CPU build,
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
            "Vulkan",
            "CUDA",
            "ROCm");

        if (answer == MessageBoxResult.Custom3)
        {
            return await PromptCrispAsrCudaVersionAsync();
        }

        return answer switch
        {
            MessageBoxResult.Custom1 => string.Empty,
            MessageBoxResult.Custom2 => "vulkan",
            MessageBoxResult.Custom4 => "hip",
            _ => null,
        };
    }

    /// <summary>
    /// Follow-up prompt after the user picks "CUDA" in the CrispASR variant selector, on both
    /// Windows and Linux - upstream ships a CUDA 12 and a CUDA 13 build for each.
    /// Returns "cuda" (CUDA 12 build) or "cuda13", or null when the user cancels.
    /// </summary>
    private async Task<string?> PromptCrispAsrCudaVersionAsync()
    {
        var answer = await MessageBox.Show(
            Window!,
            "CrispASR CUDA build",
            $"{Environment.NewLine}CUDA 12 works with most current NVIDIA drivers.{Environment.NewLine}{Environment.NewLine}Pick CUDA 13 only if your driver stack is built for CUDA 13.",
            MessageBoxButtons.Cancel,
            MessageBoxIcon.Question,
            "CUDA 12",
            "CUDA 13");

        return answer switch
        {
            MessageBoxResult.Custom1 => "cuda",
            MessageBoxResult.Custom2 => "cuda13",
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
            await Task.Run(async () =>
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
                        await Dispatcher.UIThread.InvokeAsync(() => BatchItems.Add(batchItem));
                    }
                }
            });
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
        if (SelectedBatchItem == null)
        {
            return;
        }

        var idx = BatchItems.IndexOf(SelectedBatchItem);
        BatchItems.Remove(SelectedBatchItem);

        // Keep a selection so repeated Remove clicks keep working down the list.
        if (BatchItems.Count > 0)
        {
            SelectedBatchItem = BatchItems[Math.Min(idx, BatchItems.Count - 1)];
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
                viewModal.Engines = Engines.ToList();
                viewModal.EngineClickedCommand.Execute(SelectedEngine);
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
                viewModal.RemoveNonSpeechLines = Se.Settings.Tools.AudioToText.WhisperPostProcessingRemoveNonSpeechLines;
                viewModal.RemoveRepeatedLines = Se.Settings.Tools.AudioToText.WhisperPostProcessingRemoveRepeatedLines;
                viewModal.ShowQualityReport = Se.Settings.Tools.AudioToText.WhisperPostProcessingShowQualityReport;
                viewModal.ChangeUnderlineToColor = Se.Settings.Tools.AudioToText.WhisperPostProcessingChangeUnderlineToColor;
                viewModal.ChangeUnderlineToColorColor = Se.Settings.Tools.AudioToText.WhisperPostProcessingChangeUnderlineToColorColor.FromHexToColor();
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
            Se.Settings.Tools.AudioToText.WhisperPostProcessingRemoveNonSpeechLines = vm.RemoveNonSpeechLines;
            Se.Settings.Tools.AudioToText.WhisperPostProcessingRemoveRepeatedLines = vm.RemoveRepeatedLines;
            Se.Settings.Tools.AudioToText.WhisperPostProcessingShowQualityReport = vm.ShowQualityReport;
            Se.Settings.Tools.AudioToText.WhisperPostProcessingChangeUnderlineToColor = vm.ChangeUnderlineToColor;
            Se.Settings.Tools.AudioToText.WhisperPostProcessingChangeUnderlineToColorColor = vm.ChangeUnderlineToColorColor.FromColorToHex();
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

    /// <summary>
    /// Makes sure the forced-aligner model an engine needs for timestamps is on disk,
    /// prompting for a download when it is missing. Returns false when the user
    /// declines or cancels - the transcribe run must not start in that case.
    /// </summary>
    private async Task<bool> EnsureAlignerModelDownloadedAsync(ISpeechToTextEngine engine, WhisperModel modelAligner, string engineDisplayName)
    {
        if (engine.IsModelInstalled(modelAligner))
        {
            return true;
        }

        var answer = await MessageBox.Show(
            Window!,
            $"Download {modelAligner}?",
            $"'{engineDisplayName}' requires a forced aligner to create timestamps.\nDownload and use {modelAligner.Name}?",
            MessageBoxButtons.YesNoCancel,
            MessageBoxIcon.Question);

        if (answer != MessageBoxResult.Yes)
        {
            return false;
        }

        var displayModelAligner = new SpeechToTextModelDisplay
        {
            Model = modelAligner,
            Display = modelAligner.Name + " (forced aligner for timestamps)",
            Engine = engine,
        };
        var models = new ObservableCollection<SpeechToTextModelDisplay> { displayModelAligner };
        var vm = await _windowService.ShowDialogAsync<DownloadSpeechToTextModelsWindow, DownloadSpeechToTextModelsViewModel>(
            Window!, viewModel =>
            {
                viewModel.SetModels(models, engine, displayModelAligner);
                viewModel.StartDownload();
            });

        return vm.OkPressed;
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

            // Every run picks its output folder anew - a folder chosen for an earlier batch
            // in this dialog session must not silently receive a later batch's output.
            _batchOutputFolder = null;

            if (BatchItems.Any(b => DocumentPortal.IsPortalPath(b.InputVideoFileName)))
            {
                // Videos opened through the Flatpak document portal live in a single-file
                // grant where a sibling .srt can never materialize as a real file (issue
                // #13308), so ask for a real output folder before transcribing.
                var folder = await _folderHelper.PickFolderAsync(Window!, Se.Language.General.PickOutputFolder);
                if (string.IsNullOrEmpty(folder))
                {
                    return;
                }

                _batchOutputFolder = folder;
            }
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
            _cudaComputeTypeNotSupported = false;
            _incompleteModel = false;
            _missingSharedLibrary = null;
            _loadedFromStdOut = false;

            Se.Settings.Tools.AudioToText.WhisperChoice = engine.Choice;

            if (!engine.IsEngineInstalled())
            {
                if (engine is ICrispAsrEngine && Configuration.IsRunningOnWindows)
                {
                    var crispVariant = await PromptCrispAsrWindowsVariantAsync(engine.Name);
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
                else if (engine is ICrispAsrEngine
                         && OperatingSystem.IsLinux()
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

                RefreshDownloadStatus(vm.SelectedModel?.Model);
            }

            // Engines without native timestamps need a forced-aligner model on disk.
            var alignerOk = engine switch
            {
                Qwen3AsrCppEngine qwen3Asr =>
                    await EnsureAlignerModelDownloadedAsync(engine, qwen3Asr.ForcedAlignerModel, "Qwen3 ASR CPP"),
                CrispAsrQwen3 crispQwen3Engine when SelectedForcedAligner is null or { IsBuiltIn: true } =>
                    await EnsureAlignerModelDownloadedAsync(engine, crispQwen3Engine.ForcedAlignerModel, "Crisp ASR Qwen3"),
                CrispAsrMega crispMegaEngine when SelectedForcedAligner is null or { IsBuiltIn: true } =>
                    await EnsureAlignerModelDownloadedAsync(engine, crispMegaEngine.ForcedAlignerModel, "Crisp ASR Mega"),
                _ => true,
            };
            if (!alignerOk)
            {
                return;
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

            // do not touch BatchItems here - it holds the user's queued batch list
            _jobItems = new List<SpeechToTextJobItem> { new SpeechToTextJobItem(_videoFileName, string.Empty, mediaInfo) };
        }
        else
        {
            _jobItems = BatchItems;
        }

        _batchIndex = 0;

        if (_jobItems.Count == 0)
        {
            return;
        }

        if (IsBatchMode)
        {
            var jobItem = _jobItems[0];
            Dispatcher.UIThread.Post(() =>
            {
                if (BatchGrid == null)
                {
                    return;
                }

                BatchGrid.SelectedItem = jobItem;
                BatchGrid.ScrollIntoView(jobItem);
            });
        }

        _videoFileName = _jobItems[0].InputVideoFileName;
        _videoInfo.TotalMilliseconds = _jobItems[0].MediaInfo.Duration.TotalMilliseconds;
        _videoInfo.TotalSeconds = _jobItems[0].MediaInfo.Duration.TotalSeconds;
        _videoInfo.Width = _jobItems[0].MediaInfo.Dimension.Width;
        _videoInfo.Height = _jobItems[0].MediaInfo.Dimension.Height;

        ProgressOpacity = 1;
        ProgressText = Se.Language.General.GeneratingAudioFile;
        _startTicks = DateTime.UtcNow.Ticks;

        var startGenerateAudioFileOk = GenerateAudioFile(_videoFileName, _audioTrackNumber);
        if (!startGenerateAudioFileOk)
        {
            IsTranscribeEnabled = true;
            ProgressOpacity = 0;
            await ShowUnableToStartEngineErrorAsync();
        }
    }

    /// <summary>
    /// Fails the online-STT job that is currently running. In batch mode the item is marked
    /// failed and the batch moves on, the same contract the ffmpeg-failure path uses - every
    /// online error path used to just re-enable the window, which stopped the batch dead at
    /// that file with no item marked and no summary.
    /// </summary>
    private void FailCurrentOnlineSttJob()
    {
        if (IsBatchMode && !_abort)
        {
            if (_batchIndex >= 0 && _batchIndex < _jobItems.Count)
            {
                _jobItems[_batchIndex].Status = Se.Language.General.Error;
            }

            StartNext(null);
            return;
        }

        IsTranscribeEnabled = true;
    }

    [RelayCommand]
    private void Cancel()
    {
        if (!IsTranscribeEnabled)
        {
            // Always set _abort, online engines included: during the audio-extraction phase
            // _openAiCts does not exist yet (it is created once ffmpeg has exited), so
            // cancelling did nothing at all and the upload started anyway. _abort is also
            // what MakeResult checks to stop a whole batch rather than skip one item.
            _abort = true;
            if (GetEffectiveSelectedEngine() is IOnlineSttEngine)
            {
                _openAiCts?.Cancel();
            }

            return;
        }

        Window?.Close();
    }

    /// <param name="retryWithoutCrispAsrVad">
    /// Re-run of a Crisp ASR job that came back empty, this time without the VAD pass SE adds for
    /// the Cohere/Mega backends - see RetryCrispAsrWithoutVad (#13911).
    /// </param>
    public bool TranscribeViaWhisper(string waveFileName, string videoFileName, bool retryWithoutCrispAsrVad = false)
    {
        _crispAsrVadSuppressed = retryWithoutCrispAsrVad;
        _crispAsrVadWasUsed = false;

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

        _useCenterChannelOnly = false; // FFmpeg center-channel extraction is not configurable in SE 5 yet

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
        var engineOutputFolder = string.Empty;
        if (CanEngineReadSourceFileDirectly(engine) && CanSendSourceFileToEngine(videoFileName))
        {
            inputFile = videoFileName;
        }

        if (CanEngineReadSourceFileDirectly(engine) &&
            !inputFile.StartsWith(GetSttTempFolder(), StringComparison.OrdinalIgnoreCase))
        {
            // Both engines save their output next to the input file, so pointing them at the user's
            // own media would write "<video>.srt" into that folder - overwriting any subtitle already
            // sitting there, which SE then deletes again as one of its temp files. Send the output to
            // the per-run folder instead, the same isolation the extracted WAV gets (#11837). The
            // input is the user's own file both when the source file is sent directly and when a
            // pre-extracted 16 kHz WAV skipped the extraction step, so key on the location, not on
            // which of the two paths was taken.
            engineOutputFolder = GetSttTempFolder();
        }

        try
        {
            _whisperProcess = GetWhisperProcess(engine, inputFile, model.Model.Name, language.Code, DoTranslateToEnglish,
                OutputHandler, engineOutputFolder);
        }
        catch (Exception e)
        {
            _error = e.Message;
            SeLogger.Error(e, $"Unable to start speech-to-text engine \"{engine.Name}\"");
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

    /// <summary>
    /// Starts an engine executable with the standard hidden-window setup, wiring
    /// stdout/stderr to <paramref name="dataReceivedHandler"/> when one is given.
    /// The working directory is the executable's folder.
    /// </summary>
    private static Process StartEngineProcess(
        string executable,
        string arguments,
        DataReceivedEventHandler? dataReceivedHandler,
        Action<ProcessStartInfo>? configureStartInfo = null)
    {
        var p = new Process
        {
            StartInfo = new ProcessStartInfo(executable, arguments)
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                UseShellExecute = false,
                WorkingDirectory = Path.GetDirectoryName(executable),
            }
        };

        configureStartInfo?.Invoke(p.StartInfo);

        if (dataReceivedHandler != null)
        {
            p.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            p.StartInfo.StandardErrorEncoding = Encoding.UTF8;
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

    /// <summary>
    /// Puts the engine folder on the dynamic loader's search path, so an engine that ships its
    /// own shared libraries next to the executable can find them.
    ///
    /// Windows resolves DLLs from the executable's own directory, so this is a Linux/macOS-only
    /// concern - and there setting WorkingDirectory is not enough, because the loader does not
    /// search the working directory. The libraries are supposed to carry an $ORIGIN/@loader_path
    /// RPATH that makes this unnecessary, but whisper.cpp archives shipped for four releases
    /// without one (issue #13680), so set it as a belt-and-braces measure.
    /// </summary>
    private static void AddEngineFolderToLibrarySearchPath(ProcessStartInfo startInfo, string engineFolder)
    {
        var variable = OperatingSystem.IsMacOS() ? "DYLD_LIBRARY_PATH" : "LD_LIBRARY_PATH";
        var existing = ProcessEnvironmentHelper.GetOrNull(startInfo, variable);

        startInfo.EnvironmentVariables[variable] = string.IsNullOrEmpty(existing)
            ? engineFolder
            : engineFolder + Path.PathSeparator + existing;
    }

    /// <summary>
    /// WhisperX shells out to a real "ffmpeg" binary via subprocess for audio loading (it is not
    /// bundled in the standalone build - see subtitleedit-whisperx-standalone's README). Puts
    /// Subtitle Edit's own configured/bundled ffmpeg on PATH so WhisperX finds it without
    /// requiring a separate ffmpeg install. When ffmpeg is not configured to an actual file
    /// (e.g. still the bare "ffmpeg" fallback resolved through the system PATH), this leaves
    /// PATH untouched - the child process falls back to the exact same system PATH resolution
    /// Subtitle Edit's own ffmpeg calls would use in that situation.
    /// </summary>
    private static void AddFfmpegToPath(ProcessStartInfo startInfo)
    {
        var ffmpegLocation = FfmpegHelper.GetFfmpegLocation();
        if (string.IsNullOrEmpty(ffmpegLocation) || !File.Exists(ffmpegLocation))
        {
            return;
        }

        var ffmpegDir = Path.GetDirectoryName(ffmpegLocation);
        if (string.IsNullOrEmpty(ffmpegDir))
        {
            return;
        }

        var existingPath = ProcessEnvironmentHelper.GetOrNull(startInfo, "PATH");
        startInfo.EnvironmentVariables["PATH"] = string.IsNullOrEmpty(existingPath)
            ? ffmpegDir
            : ffmpegDir + Path.PathSeparator + existingPath;
    }

    /// <summary>
    /// Engines that demux and decode the source media themselves, so they can be pointed at the
    /// user's original file instead of SE's extracted WAV: Purfview Faster-Whisper-XXL bundles
    /// ffmpeg, whisper-ctranslate2 bundles PyAV. Verified that the others cannot - whisper.cpp
    /// answers "failed to read audio data as wav" for an mp4, and CrispASR's own help lists
    /// flac/mp3/ogg/wav only - so those keep getting the WAV.
    /// </summary>
    private static bool CanEngineReadSourceFileDirectly(ISpeechToTextEngine engine)
    {
        return engine.Name == WhisperEnginePurfviewFasterWhisperXxl.StaticName ||
               engine is WhisperEngineCTranslate2 or WhisperEngineWhisperX;
    }

    /// <summary>
    /// True if the source file itself can go to the engine, which is only safe when there is
    /// exactly one audio track: with several, the engines read the FIRST audio stream (Purfview
    /// XXL's --ff_track defaults to 1, CTranslate2's PyAV decode is hardcoded to the first and has
    /// no selector) - not the container's default track that mpv plays and the waveform shows, and
    /// not a track the user picked. The extracted WAV expresses both: the picked track via -map on
    /// the video it was picked from, and otherwise ffmpeg's automatic selection, which honors the
    /// default-track flag like mpv does (verified both ways on a two-track file). SE 4 drew the
    /// same line at "anything but the default first track goes through the WAV".
    /// </summary>
    private bool CanSendSourceFileToEngine(string videoFileName)
    {
        if (_useCenterChannelOnly || string.IsNullOrEmpty(videoFileName) || !File.Exists(videoFileName))
        {
            return false;
        }

        try
        {
            var audioTrackCount = FfmpegMediaInfo.Parse(videoFileName).Tracks
                .Count(t => t.TrackType == FfmpegTrackType.Audio);

            return audioTrackCount == 1;
        }
        catch (Exception exception)
        {
            SeLogger.Error(exception, $"Unable to read audio tracks from: {videoFileName}");
            return false;
        }
    }

    private Process GetWhisperProcess(
        ISpeechToTextEngine engine,
        string waveFileName,
        string model,
        string language,
        bool translate,
        DataReceivedEventHandler? dataReceivedHandler = null,
        string engineOutputFolder = "")
    {
        if (engine is WhisperEngineWhisperX whisperX)
        {
            var exe = whisperX.GetExecutable();
            var whisperXArgs = whisperX.CommandLineParameter;
            var languageArgX = language.Equals("auto", StringComparison.OrdinalIgnoreCase)
                ? string.Empty
                : $"--language {language} ";
            var taskArg = translate ? "--task translate " : string.Empty;
            var outputDir = string.IsNullOrEmpty(engineOutputFolder)
                ? GetSttTempFolder()
                : engineOutputFolder;
            var parametersX =
                $"{languageArgX}--model \"{model}\" --output_format srt --output_dir \"{outputDir}\" " +
                $"{taskArg}{whisperXArgs} \"{waveFileName}\"";

            // The generic launch path is bypassed here, so repeat the two pieces of its setup a
            // PyInstaller-frozen Python engine needs: the glibc 2.41+ executable-stack repair,
            // and the Python UTF-8/unbuffered variables - without them Windows decodes piped
            // output with the ANSI code page (mojibake, or a UnicodeEncodeError killing the run)
            // and stdout block-buffers so the log sits empty until the process exits.
            EnsureExecutableStackCleared(whisperX, whisperX.GetAndCreateWhisperFolder());

            Se.WriteToolsLog($"{exe} {parametersX}");
            return StartEngineProcess(exe, parametersX, dataReceivedHandler, startInfo =>
            {
                AddFfmpegToPath(startInfo);
                startInfo.EnvironmentVariables["PYTHONIOENCODING"] = "utf-8";
                startInfo.EnvironmentVariables["PYTHONUTF8"] = "1";
                startInfo.EnvironmentVariables["PYTHONUNBUFFERED"] = "1";
            });
        }

        if (engine is Qwen3AsrCppEngine qwen3Asr)
        {
            var exe = qwen3Asr.GetExecutable();
            var alignerModel = qwen3Asr.ForcedAlignerModel;
            _qwen3AsrOutputJsonPath = Path.Combine(Path.GetTempPath(), $"qwen3_asr_{Guid.NewGuid():N}.json");
            _engineExitCode = null;
            var qwen3ExtraArgs = engine.CommandLineParameter;

            var qwen3Params = string.IsNullOrWhiteSpace(qwen3ExtraArgs)
                ? $"-m \"{qwen3Asr.GetModelForCmdLine(model)}\" --aligner-model \"{qwen3Asr.GetModelForCmdLine(alignerModel.Name)}\" -f \"{waveFileName}\" --transcribe-align -o \"{_qwen3AsrOutputJsonPath}\""
                : $"{qwen3ExtraArgs} -m \"{qwen3Asr.GetModelForCmdLine(model)}\" --aligner-model \"{qwen3Asr.GetModelForCmdLine(alignerModel.Name)}\" -f \"{waveFileName}\" --transcribe-align -o \"{_qwen3AsrOutputJsonPath}\"";

            return StartEngineProcess(exe, qwen3Params, dataReceivedHandler);
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
            if (ShouldForceCrispAsrVad(crispAsrEngine, crispArgs, _crispAsrVadSuppressed))
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

            // Remembered so an empty result can be told apart from an empty result *because of*
            // VAD - only the latter is worth re-running without it (#13911).
            _crispAsrVadWasUsed = vadPart.Length > 0;

            // Both are per model, not per backend: Parakeet's pure-CTC models run on a different
            // crispasr backend than its transducer models and need crispasr's punctuation
            // restoration kept off (see CrispAsrParakeet.GetBackendName / GetModelArguments).
            var backendName = crispAsrEngine.GetBackendName(model);
            var modelArgs = crispAsrEngine.GetModelArguments(model, crispArgs);
            var modelArgsPart = modelArgs.Length > 0 ? $" {modelArgs}" : string.Empty;

            // --print-progress: crispasr streams "crispasr: progress = NN% (i/n slices)" lines
            // in real time (parsed in OutputHandler), while the transcript segments only print
            // once the whole file is done - without this the progress bar sat idle for the
            // entire run and jumped straight to 100%.
            var crispParams = string.IsNullOrWhiteSpace(crispArgs)
                ? $"--backend {backendName} {langPart}-m \"{crispModel}\"{alignerPart}{vadPart}{modelArgsPart} -f \"{waveFileName}\" --output-srt --print-progress"
                : $"--backend {backendName} {langPart}-m \"{crispModel}\"{alignerPart}{vadPart}{modelArgsPart} -f \"{waveFileName}\" --output-srt --print-progress {crispArgs}";

            Se.WriteToolsLog($"{exe} {crispParams}");

            return StartEngineProcess(exe, crispParams, dataReceivedHandler);
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

        // Only set when the engine reads the user's own media instead of the extracted WAV; both
        // engines accept "--output_dir" (Purfview XXL defaults to the input's folder, ctranslate2
        // to the working directory). A user who set their own output folder in the extra
        // parameters keeps it.
        var outputDirArg = string.Empty;
        if (!string.IsNullOrEmpty(engineOutputFolder) &&
            !args.Contains("--output_dir", StringComparison.Ordinal) &&
            !Regex.IsMatch(args, @"(^|\s)-o(\s|$)"))
        {
            outputDirArg = $"--output_dir \"{engineOutputFolder}\" ";
        }

        var parameters =
            $"{languageArg}--model \"{m}\" {outputSrt}{outputDirArg}{translateToEnglish}{args} \"{waveFileName}\"{postParams}";

        if (engine is WhisperEngineCTranslate2)
        {
            parameters = parameters.Replace("--model", "--model_directory");

            // whisper-ctranslate2 prints no segment lines unless --verbose is on; its only
            // other progress output is a carriage-return tqdm bar, which never completes a
            // line, so the newline-based console stays empty and a long CPU decode looks
            // completely frozen (reported on macOS with large-v3). Default verbose on so
            // segments stream live, unless the user set their own preference.
            if (!parameters.Contains("--verbose", StringComparison.Ordinal))
            {
                parameters = "--verbose True " + parameters;
            }
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

        // Python Whisper and huggingface_hub derive their default caches from these variables.
        // Keep their expected subfolder names so model-name arguments continue to work while
        // the actual weights live under the user-selected Subtitle Edit model root.
        if (Se.HasCustomModelsFolder && settings.WhisperChoice == WhisperChoice.OpenAi)
        {
            process.StartInfo.EnvironmentVariables["XDG_CACHE_HOME"] = Path.Combine(Se.ModelsFolder, "SpeechToText");
        }
        else if (Se.HasCustomModelsFolder && settings.WhisperChoice == WhisperChoice.CTranslate2)
        {
            process.StartInfo.EnvironmentVariables["HF_HOME"] = Path.Combine(Se.ModelsFolder, "SpeechToText", "HuggingFace");
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

            EnsureExecutableStackCleared(engine, whisperFolder);
        }

        if (OperatingSystem.IsWindows() && ProcessEnvironmentHelper.GetOrNull(process.StartInfo, "Path") != null)
        {
            if (!string.IsNullOrEmpty(Se.Settings.General.FfmpegPath))
            {
                process.StartInfo.EnvironmentVariables["Path"] =
                    process.StartInfo.EnvironmentVariables["Path"]?.TrimEnd(';') + ";" +
                    Path.GetDirectoryName(Se.Settings.General.FfmpegPath);
            }

            if (!string.IsNullOrEmpty(whisperFolder))
            {
                process.StartInfo.EnvironmentVariables["Path"] =
                    process.StartInfo.EnvironmentVariables["Path"]?.TrimEnd(';') + ";" + whisperFolder;
            }
        }
        else if (!string.IsNullOrEmpty(whisperFolder))
        {
            AddEngineFolderToLibrarySearchPath(process.StartInfo, whisperFolder);
        }

        if (settings.WhisperChoice != WhisperChoice.Cpp &&
            settings.WhisperChoice != WhisperChoice.CppCuBlas &&
            settings.WhisperChoice != WhisperChoice.ConstMe)
        {
            process.StartInfo.EnvironmentVariables["PYTHONIOENCODING"] = "utf-8";
            process.StartInfo.EnvironmentVariables["PYTHONUTF8"] = "1";

            // Without a terminal attached, Python block-buffers stdout (~8 KB), so a
            // Python-based engine's progress output (e.g. whisper-ctranslate2's --verbose
            // segment lines) sits in the buffer until the process exits and the console
            // log stays empty for the whole run, which reads as frozen. Unbuffered mode
            // makes every line arrive as soon as it is printed.
            process.StartInfo.EnvironmentVariables["PYTHONUNBUFFERED"] = "1";
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

    private static bool _executableStackChecked;

    /// <summary>
    /// Repairs an already-installed Purfview Faster-Whisper-XXL or WhisperX before launching it.
    /// <para>
    /// Both bundle a libctranslate2 built with PT_GNU_STACK = RWE, and glibc 2.41 stopped making
    /// the stack executable at dlopen time, so on a distro with glibc 2.41+ (Fedora 42, Arch,
    /// Ubuntu 25.10) the run dies immediately with "cannot enable executable stack as shared
    /// object requires: Invalid argument". The download path clears the flag on unpack, but
    /// installs made by an earlier SE are already on disk and re-downloading is over a gigabyte,
    /// so fix them here too. Once per session: the scan only reads ELF headers, but there is no
    /// reason to repeat it for every transcription.
    /// </para>
    /// </summary>
    private static void EnsureExecutableStackCleared(ISpeechToTextEngine engine, string? whisperFolder)
    {
        if (_executableStackChecked ||
            !OperatingSystem.IsLinux() ||
            string.IsNullOrEmpty(whisperFolder) ||
            engine is not (WhisperEnginePurfviewFasterWhisperXxl or WhisperEngineWhisperX))
        {
            return;
        }

        _executableStackChecked = true;
        var patched = ElfHelper.ClearExecutableStackInFolder(whisperFolder);
        if (patched > 0)
        {
            Se.WriteToolsLog($"Cleared the executable-stack flag on {patched} shared librar" +
                             (patched == 1 ? "y" : "ies") + $" in \"{whisperFolder}\"");
        }
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
                    // No extraction happened - clear a stale name from an earlier run so result
                    // discovery falls back to the video file name deterministically.
                    _audioFileName = string.Empty;
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
        _audioExtractProcess.Start();
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

        // Check before the language guard below - the dynamic loader fails before the engine
        // prints anything else, and this must be reported even if no language is selected.
        if (_missingSharedLibrary == null)
        {
            _missingSharedLibrary = MissingSharedLibrary.GetName(outLine.Data);
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
        else if (outLine.Data.Contains("CUBLAS_STATUS_NOT_SUPPORTED", StringComparison.OrdinalIgnoreCase))
        {
            // faster-whisper picks float32 when it cannot tell what the GPU supports, and cuBLAS
            // then refuses the matmul with CUBLAS_STATUS_NOT_SUPPORTED - the run dies inside
            // encode() and leaves no output at all, so without this the user just gets an empty
            // result (issue #13902).
            _cudaComputeTypeNotSupported = true;
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
                // "[mm:ss.mmm --> mm:ss.mmm]  text"
                AddResultTextFromLine(line, startIndex: 1, timeLength: 10, endIndex: 14, textIndex: 25, language.Code);
            }
            else if (_timeRegexLong.IsMatch(line))
            {
                // "[hh:mm:ss.mmm --> hh:mm:ss.mmm]  text"
                AddResultTextFromLine(line, startIndex: 1, timeLength: 12, endIndex: 18, textIndex: 31, language.Code);
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
            else if (line.StartsWith("crispasr: progress =", StringComparison.OrdinalIgnoreCase))
            {
                // crispasr --print-progress: "crispasr: progress =  14% (1/7 slices)" - streamed
                // per 30 s slice for every ASR backend, while the transcript segments only print
                // after the whole file is processed, so this is the only live progress source.
                var arr = line.Split('=');
                if (arr.Length == 2)
                {
                    var pctString = arr[1].TrimStart();
                    var pctEnd = pctString.IndexOf('%');
                    if (pctEnd > 0 && double.TryParse(pctString[..pctEnd], NumberStyles.AllowDecimalPoint,
                            CultureInfo.InvariantCulture, out var pct))
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

    /// <summary>
    /// Parses one "[start --> end]  text" transcript line from engine output into
    /// the running result list, and drives the progress estimate off the segment
    /// end time when no explicit percentage is being streamed.
    /// </summary>
    private void AddResultTextFromLine(string line, int startIndex, int timeLength, int endIndex, int textIndex, string languageCode)
    {
        var rt = new ResultText
        {
            Start = GetSeconds(line.Substring(startIndex, timeLength)),
            End = GetSeconds(line.Substring(endIndex, timeLength)),
            Text = Utilities.AutoBreakLine(line.Remove(0, textIndex).Trim(), languageCode),
        };

        if (_showProgressPct < 0)
        {
            _endSeconds = (double)rt.End;
        }

        _resultList.Add(rt);
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

    /// <summary>
    /// The "-map" argument for extracting <paramref name="inputFileName"/>'s audio, or an empty
    /// string to leave the choice to ffmpeg's automatic stream selection.
    /// </summary>
    /// <remarks>
    /// A stream index only addresses a stream in the file it was read from, so it is applied to
    /// that file alone: batch mode reuses this view model for other videos, and "transcribe
    /// selected lines" feeds it already-demuxed "se_audioclip_*.wav" clips.
    ///
    /// The trailing "?" (#13621, same fix as in WaveFileExtractor for #10835) only covers stream N
    /// being *missing* - it does nothing when N exists but is the wrong kind. A file whose audio is
    /// stream 0 and video stream 1 (ffmpeg lists streams in container order, and plenty of muxers
    /// put audio first) got "-map 0:1" pointing at its video, which -vn then dropped: "Output file
    /// does not contain any stream", ffmpeg exit -22, and the run aborted with "Generated audio
    /// file not found" (#13781).
    ///
    /// Do not "fix" the no-map fallback to "-map 0:a:0?" (tried in #13787, reverted): ffmpeg's
    /// automatic selection prefers the stream with the default disposition (verified on the
    /// bundled ffmpeg 7.1.1 - a default-flagged stereo track beats a non-flagged 5.1), and that
    /// is the wanted behavior. It is the same track a fresh mpv plays and the main window follows
    /// on open (#13233 - the first track can be commentary or audio description), while the first
    /// track in container order is only mpv's last-resort fallback.
    /// </remarks>
    internal static string BuildAudioMapParameter(string inputFileName, int audioTrackNumber, string? audioTrackVideoFileName)
    {
        if (audioTrackNumber < 0 ||
            string.IsNullOrEmpty(audioTrackVideoFileName) ||
            !string.Equals(inputFileName, audioTrackVideoFileName, StringComparison.OrdinalIgnoreCase))
        {
            return string.Empty;
        }

        return $"-map 0:{audioTrackNumber}?";
    }

    private Process? GetFfmpegProcess(string videoFileName, int audioTrackNumber, string outAudioFile, string audioFormat = "wav")
    {
        if (!File.Exists(Se.Settings.General.FfmpegPath) && Configuration.IsRunningOnWindows)
        {
            return null;
        }

        var audioParameter = BuildAudioMapParameter(videoFileName, audioTrackNumber, _audioTrackVideoFileName);

        var fFmpegAudioTranscodeSettings = GetFfmpegTranscodeFormatString(audioFormat, _useCenterChannelOnly);

        //-i indicates the input
        //-vn means no video output
        //-ar 16000 indicates the sampling frequency.
        //-b:a indicates the bit rate (only used for the compressed formats)
        //-ac 1 means 1 channel (mono)
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
    /// ffmpeg argument template for transcoding the source audio. WAV stays
    /// lossless 16 kHz mono PCM, unmodified apart from the downmix; the compressed
    /// formats target ~32 kbit/s mono at 16 kHz, which is plenty for speech
    /// recognition and keeps a 2-hour video well under OpenAI's 25 MB upload
    /// limit. Opus is shipped inside a webm container because OpenAI accepts
    /// webm but rejects bare ".opus" uploads.
    /// </summary>
    private static string GetFfmpegTranscodeFormatString(string audioFormat, bool useCenterChannelOnly)
    {
        var normalized = string.IsNullOrWhiteSpace(audioFormat) ? "wav" : audioFormat.Trim().ToLowerInvariant();

        // No "volume=1.75" here, unlike the waveform extraction in WaveFileExtractor where the boost
        // only makes the drawing easier to read. +4.9 dB into 16-bit PCM hard-clips every peak of an
        // already-mastered source - measured at ~5% of all samples pinned to full scale for speech
        // peaking at -0.5 dBFS - and that distortion costs recognition accuracy (#13738). The gain
        // buys nothing in return: whisper's log-mel front end clamps to "max - 8 dB" and rescales,
        // so a uniform gain is normalized away before the model ever sees it.
        var channelArgs = useCenterChannelOnly
            ? "-af \"pan=mono|c0=FC\""
            : "-ac 1";

        return normalized switch
        {
            "mp3" => "-i \"{0}\" -vn -ar 16000 " + channelArgs + " -c:a libmp3lame -b:a 32k -f mp3 {2} \"{1}\"",
            "m4a" => "-i \"{0}\" -vn -ar 16000 " + channelArgs + " -c:a aac -b:a 32k -f ipod {2} \"{1}\"",
            "webm" => "-i \"{0}\" -vn -ar 16000 " + channelArgs + " -c:a libopus -b:a 28k -f webm {2} \"{1}\"",
            // pcm_s16le is already ffmpeg's default for wav, but spell it out: SE's own peak reader
            // (WavePeakGenerator2, shared with MakeWavePeaks) only handles integer PCM, so the sample
            // format must not drift. "-ab" is dropped - it is a no-op for an uncompressed encoder.
            _ => "-i \"{0}\" -vn -ar 16000 " + channelArgs + " -c:a pcm_s16le -f wav {2} \"{1}\"",
        };
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            // Route through Cancel so Escape during a run aborts the run (like the
            // Cancel button) instead of closing the window over a live engine process.
            Cancel();
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
            SelectedModel = Models.FirstOrDefault(m => m.Model.Name == result.Name);
        }
        else
        {
            SelectedModel = Models.FirstOrDefault(m => m.Model.Name == oldModel.Model.Name);
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

        // Keep the user's current in-session choice when switching engine/backend - jumping
        // back to the last *saved* language on every settings change forced the user to
        // re-pick the language each time (#11744). The saved code is only the fallback when
        // the new engine does not offer the current language.
        WhisperLanguage? language = null;
        if (SelectedLanguage is { } prev)
        {
            language = Languages.FirstOrDefault(p => p.Code == prev.Code)
                       ?? Languages.FirstOrDefault(p => string.Equals(p.Name, prev.Name, StringComparison.OrdinalIgnoreCase));
        }

        var savedCode = Se.Settings.Tools.AudioToText.WhisperLanguageCode;
        if (language == null && !string.IsNullOrEmpty(savedCode))
        {
            language = Languages.FirstOrDefault(p => p.Code == savedCode);
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
            var model = Models.FirstOrDefault(p => p.Model.Name == Se.Settings.Tools.AudioToText.WhisperModel);
            if (model != null)
            {
                SelectedModel = model;
            }
            else
            {
                SelectedModel = Models.FirstOrDefault();
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
        else if (engine is Qwen3AsrCppEngine && !_qwen3AsrCppUpdatePromptShown)
        {
            Dispatcher.UIThread.Post(async () => await CheckQwen3AsrCppForUpdateAsync());
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

        if (!canDownload || isInstalled)
        {
            EngineDownloadHint = string.Empty;
            IsEngineDownloadButtonVisible = false;
            return;
        }

        var size = engine.DownloadSizeText;
        EngineDownloadHint = string.IsNullOrEmpty(size)
            ? string.Format(Se.Language.General.DownloadX, engine.Name)
            : string.Format(Se.Language.General.DownloadX, engine.Name) + $" ({size})";
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
                           ?? (OperatingSystem.IsWindows() ? "vulkan" : string.Empty);

        await _windowService.ShowDialogAsync<DownloadSpeechToTextEngineWindow, DownloadSpeechToTextEngineViewModel>(
            Window!, viewModel =>
            {
                viewModel.Engine = engine;
                viewModel.CrispAsrWindowsVariant = crispVariant;
                viewModel.StartDownload();
            });

        RefreshEngineCombo?.Invoke();
    }

    private async Task CheckQwen3AsrCppForUpdateAsync()
    {
        if (_qwen3AsrCppUpdatePromptShown || Window == null)
        {
            return;
        }

        var engine = GetEffectiveSelectedEngine();
        if (engine is not Qwen3AsrCppEngine || !engine.IsEngineInstalled())
        {
            return;
        }

        // Sidecar (written since the fix for #11375 landed) or, for every older install,
        // the hash of the unpacked qwen3-asr-cli - that is what recognizes the builds with
        // the broken JSON output that never got replaced.
        var folder = engine.GetAndCreateWhisperFolder();
        var useVulkan = DownloadHashManager.IsQwen3AsrCppVulkanInstall(folder);
        (string key, string hash)? lookup = TryReadSidecarHash(folder);
        if (lookup == null)
        {
            var key = DownloadHashManager.ResolveQwen3AsrCppExecutableKey(useVulkan);
            var hash = key == null ? null : await Sha256Util.ComputeSha256Async(engine.GetExecutable());
            lookup = key == null || hash == null ? null : (key, hash);
        }

        if (lookup is not var (lookupKey, lookupHash))
        {
            return;
        }

        if (DownloadHashManager.GetStatus(lookupKey, lookupHash) != DownloadHashManager.UpdateStatus.UpdateAvailable)
        {
            return;
        }

        _qwen3AsrCppUpdatePromptShown = true;

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
                viewModel.Qwen3AsrUseVulkan = useVulkan;
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
            var hash = Sha256Util.ComputeSha256(exePath);
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
            if (OperatingSystem.IsWindows())
            {
                variant = DownloadHashManager.DetectCrispAsrWindowsVariant(folder);
            }
            else if (OperatingSystem.IsLinux()
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
            var hash = Sha256Util.ComputeSha256(exePath);
            return hash == null ? null : (key, hash);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Selects the engine behind <paramref name="choice"/>, including the ones that live as a
    /// backend inside the Whisper.cpp or CrispASR engine entries. No-op for an unknown or empty
    /// choice, leaving the last-used engine selected.
    /// </summary>
    private void TrySelectEngineChoice(string? choice)
    {
        if (string.IsNullOrEmpty(choice) || GetEffectiveSelectedEngine().Choice == choice)
        {
            return;
        }

        var whisperCppEngine = Engines.OfType<WhisperCppEngine>().FirstOrDefault();
        var crispAsrEngine = Engines.OfType<CrispAsrEngine>().FirstOrDefault();
        if (whisperCppEngine != null && whisperCppEngine.TrySelectBackendChoice(choice))
        {
            SelectedEngine = whisperCppEngine;
        }
        else if (crispAsrEngine != null && crispAsrEngine.TrySelectBackendChoice(choice))
        {
            SelectedEngine = crispAsrEngine;
        }
        else
        {
            var engine = Engines.FirstOrDefault(p => p.Choice == choice);
            if (engine == null)
            {
                return;
            }

            SelectedEngine = engine;
        }

        Parameters = GetEffectiveSelectedEngine().CommandLineParameter;
        EngineChanged();
    }

    private static WhisperLanguage? PickDefaultLanguage(IEnumerable<WhisperLanguage> languages)
    {
        var list = languages as IList<WhisperLanguage> ?? languages.ToList();
        return list.FirstOrDefault(p => string.Equals(p.Name, "English", StringComparison.OrdinalIgnoreCase))
            ?? list.FirstOrDefault(p => p.Code == "en" || p.Code == "eng_Latn")
            ?? list.FirstOrDefault();
    }

    /// <param name="preferredEngineChoice">
    /// A <see cref="WhisperChoice"/> to start on instead of the last-used engine, for callers that
    /// need a specific one - "find the voices in the video" needs an engine that tells speakers
    /// apart. The user can still switch it in the window; nothing is forced beyond the first view.
    /// </param>
    internal void Initialize(string? videoFileName, int audioTrackNumber, string? preferredEngineChoice = null)
    {
        _videoFileName = videoFileName;
        _audioTrackNumber = audioTrackNumber;
        _audioTrackVideoFileName = videoFileName;
        TrySelectEngineChoice(preferredEngineChoice);
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

        // The clips are already-demuxed single-stream wavs, so the video's stream index does not
        // address anything in them - leave the owning file unset and let ffmpeg pick the audio.
        _audioTrackVideoFileName = null;
        IsBatchMode = true;
        _audioClips = audioClips;
        _audioClipsAutoStart = autoStart;
        ResultAudioClips = audioClips.Select(ac => new AudioClip(ac)).ToList();

        // The remembered last-used language wins (SE4 behavior, #11744): the language
        // auto-detected from the selected lines' existing text is only used before any
        // transcription has been run - it must not override the user's choice on every run.
        var savedCode = Se.Settings.Tools.AudioToText.WhisperLanguageCode;
        var hasRememberedLanguage = !string.IsNullOrEmpty(savedCode) && Languages.Any(p => p.Code == savedCode);
        if (language != null && !hasRememberedLanguage)
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
        _timerWhisper.StopAndDispose(OnTimerWhisperOnElapsed);
        _timerAudioExtract.StopAndDispose(OnTimerAudioExtractOnElapsed);

        // With the timers gone nothing will ever reap a still-running engine or
        // ffmpeg process - kill them so closing the window mid-run doesn't leave
        // an orphan burning CPU in the background.
        KillRunningProcesses();
        _openAiCts?.Cancel();

        UiUtil.SaveWindowPosition(Window);
        Task.Run(() => { DeleteTempFiles(); });
    }

    private void KillRunningProcesses()
    {
        try
        {
            if (!_whisperProcess.HasExited)
            {
#pragma warning disable CA1416
                _whisperProcess.Kill(true);
#pragma warning restore CA1416
            }
        }
        catch
        {
            // never started, already exited/disposed - nothing to reap
        }

        try
        {
            if (_audioExtractProcess is { HasExited: false })
            {
#pragma warning disable CA1416
                _audioExtractProcess.Kill(true);
#pragma warning restore CA1416
            }
        }
        catch
        {
            // never started, already exited/disposed - nothing to reap
        }
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
