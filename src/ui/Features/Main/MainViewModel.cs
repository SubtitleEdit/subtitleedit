using Nikse.SubtitleEdit.UiLogic.Export;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaEdit;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Core.AudioToText;
using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes;
using Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream;
using Nikse.SubtitleEdit.Core.Forms;
using Nikse.SubtitleEdit.Core.Interfaces;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Core.VobSub;
using Nikse.SubtitleEdit.Features.Assa;
using Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect;
using Nikse.SubtitleEdit.Features.Assa.AssaApplyCustomOverrideTags;
using Nikse.SubtitleEdit.Features.Assa.AssaDraw;
using Nikse.SubtitleEdit.Features.Assa.AssaImageColorPicker;
using Nikse.SubtitleEdit.Features.Assa.AssaProgressBar;
using Nikse.SubtitleEdit.Features.Assa.AssaSetBackground;
using Nikse.SubtitleEdit.Features.Assa.AssaSetPosition;
using Nikse.SubtitleEdit.Features.Assa.ResolutionResampler;
using Nikse.SubtitleEdit.Features.Edit.Find;
using Nikse.SubtitleEdit.Features.Edit.ModifySelection;
using Nikse.SubtitleEdit.Features.Edit.MultipleReplace;
using Nikse.SubtitleEdit.Features.Edit.Replace;
using Nikse.SubtitleEdit.Features.Edit.ShowHistory;
using Nikse.SubtitleEdit.Features.Files.Compare;
using Nikse.SubtitleEdit.Features.Files.Export.ExportEbuStl;
using Nikse.SubtitleEdit.Features.Files.ExportCavena890;
using Nikse.SubtitleEdit.Features.Files.ExportCustomTextFormat;
using Nikse.SubtitleEdit.Features.Files.ExportEbuStl;
using Nikse.SubtitleEdit.Features.Files.ExportImageBased;
using Nikse.SubtitleEdit.Features.Files.ExportPac;
using Nikse.SubtitleEdit.Features.Files.ExportPlainText;
using Nikse.SubtitleEdit.Features.Files.FormatProperties.DCinemaSmpteProperties;
using Nikse.SubtitleEdit.Features.Files.FormatProperties.ItunesTimedTextProperties;
using Nikse.SubtitleEdit.Features.Files.FormatProperties.RosettaProperties;
using Nikse.SubtitleEdit.Features.Files.FormatProperties.TimedText10Properties;
using Nikse.SubtitleEdit.Features.Files.FormatProperties.TimedTextImsc11Properties;
using Nikse.SubtitleEdit.Features.Files.FormatProperties.TmpegEncXmlProperties;
using Nikse.SubtitleEdit.Features.Files.ImportImages;
using Nikse.SubtitleEdit.Features.Files.ImportCsvXlsxCustomColumns;
using Nikse.SubtitleEdit.Features.Files.ImportPlainText;
using Nikse.SubtitleEdit.Features.Files.ManualChosenEncoding;
using Nikse.SubtitleEdit.Features.Files.RestoreAutoBackup;
using Nikse.SubtitleEdit.Features.Files.Statistics;
using Nikse.SubtitleEdit.Features.Help.About;
using Nikse.SubtitleEdit.Features.Help.CheckForUpdates;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Features.Main.MainHelpers;
using Nikse.SubtitleEdit.Features.Ocr;
using Nikse.SubtitleEdit.Features.Options.Language;
using Nikse.SubtitleEdit.Features.Options.Plugins;
using Nikse.SubtitleEdit.Features.Options.Settings;
using Nikse.SubtitleEdit.Features.Options.Shortcuts;
using Nikse.SubtitleEdit.Features.Options.WordLists;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Shared.AddToNamesList;
using Nikse.SubtitleEdit.Features.Shared.BinaryEdit;
using Nikse.SubtitleEdit.Features.Shared.Bookmarks;
using Nikse.SubtitleEdit.Features.Shared.ColorPicker;
using Nikse.SubtitleEdit.Features.Shared.ColumnPaste;
using Nikse.SubtitleEdit.Features.Shared.ErrorList;
using Nikse.SubtitleEdit.Features.Shared.GetAudioClips;
using Nikse.SubtitleEdit.Features.Shared.GoToLineNumber;
using Nikse.SubtitleEdit.Features.Shared.MediaInfoView;
using Nikse.SubtitleEdit.Features.Shared.PickAlignment;
using Nikse.SubtitleEdit.Features.Shared.PickFontName;
using Nikse.SubtitleEdit.Features.Shared.PickLayer;
using Nikse.SubtitleEdit.Features.Shared.PickLayerFilter;
using Nikse.SubtitleEdit.Features.Shared.PickMatroskaTrack;
using Nikse.SubtitleEdit.Features.Shared.PickMp4Track;
using Nikse.SubtitleEdit.Features.Shared.PickRuleProfile;
using Nikse.SubtitleEdit.Features.Shared.PickSpellCheckDictionary;
using Nikse.SubtitleEdit.Features.Shared.PickSubtitleFormat;
using Nikse.SubtitleEdit.Features.Shared.PickTsTrack;
using Nikse.SubtitleEdit.Features.Shared.PickVobSubLanguage;
using Nikse.SubtitleEdit.Features.Shared.PromptFileSaved;
using Nikse.SubtitleEdit.Features.Shared.PromptTextBox;
using Nikse.SubtitleEdit.Features.Shared.SetVideoOffset;
using Nikse.SubtitleEdit.Features.Shared.SourceView;
using Nikse.SubtitleEdit.Features.Shared.TextBoxUtils;
using Nikse.SubtitleEdit.Features.Shared.Undocked;
using Nikse.SubtitleEdit.Features.Shared.WaveformGuessTimeCodes;
using Nikse.SubtitleEdit.Features.Shared.WaveformSeekSilence;
using Nikse.SubtitleEdit.Features.SpellCheck;
using Nikse.SubtitleEdit.Features.SpellCheck.FindDoubleLines;
using Nikse.SubtitleEdit.Features.SpellCheck.FindDoubleWords;
using Nikse.SubtitleEdit.Features.SpellCheck.GetDictionaries;
using Nikse.SubtitleEdit.Features.Ssa;
using Nikse.SubtitleEdit.Features.Sync.AdjustAllTimes;
using Nikse.SubtitleEdit.Features.Sync.ChangeFrameRate;
using Nikse.SubtitleEdit.Features.Sync.ChangeSpeed;
using Nikse.SubtitleEdit.Features.Sync.PointSync;
using Nikse.SubtitleEdit.Features.Sync.PointSyncViaOther;
using Nikse.SubtitleEdit.Features.Sync.VisualSync;
using Nikse.SubtitleEdit.Features.Tools.AdjustDuration;
using Nikse.SubtitleEdit.Features.Tools.ApplyDurationLimits;
using Nikse.SubtitleEdit.Features.Tools.ApplyMinGap;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.Features.Tools.BeautifyTimeCodes;
using Nikse.SubtitleEdit.Features.Tools.BridgeGaps;
using Nikse.SubtitleEdit.Features.Tools.ChangeCasing;
using Nikse.SubtitleEdit.Features.Tools.ChangeFormatting;
using Nikse.SubtitleEdit.Features.Tools.ConvertActors;
using Nikse.SubtitleEdit.Features.Tools.FixCommonErrors;
using Nikse.SubtitleEdit.Features.Tools.FixNetflixErrors;
using Nikse.SubtitleEdit.Features.Tools.JoinSubtitles;
using Nikse.SubtitleEdit.Features.Tools.MergeTwoSubtitles;
using Nikse.SubtitleEdit.Features.Tools.MergeContinuationLines;
using Nikse.SubtitleEdit.Features.Tools.MergeShortLines;
using Nikse.SubtitleEdit.Features.Tools.MergeSubtitlesWithSameText;
using Nikse.SubtitleEdit.Features.Tools.MergeSubtitlesWithSameTimeCodes;
using Nikse.SubtitleEdit.Features.Tools.RemoveTextForHearingImpaired;
using Nikse.SubtitleEdit.Features.Tools.Renumber;
using Nikse.SubtitleEdit.Features.Tools.SortBy;
using Nikse.SubtitleEdit.Features.Tools.SplitBreakLongLines;
using Nikse.SubtitleEdit.Features.Tools.SplitSubtitle;
using Nikse.SubtitleEdit.Features.Translate;
using Nikse.SubtitleEdit.Features.Video.BlankVideo;
using Nikse.SubtitleEdit.Features.Video.BurnIn;
using Nikse.SubtitleEdit.Features.Video.CutVideo;
using Nikse.SubtitleEdit.Features.Video.EmbeddedSubtitlesEdit;
using Nikse.SubtitleEdit.Features.Video.GoToVideoPosition;
using Nikse.SubtitleEdit.Features.Video.OpenFromUrl;
using Nikse.SubtitleEdit.Features.Video.OpenFromUrl.PickOnlineSubtitle;
using Nikse.SubtitleEdit.Features.Video.ReEncodeVideo;
using Nikse.SubtitleEdit.Features.Video.ShotChanges;
using Nikse.SubtitleEdit.Features.Video.SpeechToText;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.OpenAiCompatible;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech;
using Nikse.SubtitleEdit.Features.Video.TransparentSubtitles;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Config.Language;
using Nikse.SubtitleEdit.Logic.Download;
using Nikse.SubtitleEdit.Logic.Initializers;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.Logic.Platform.Windows;
using Nikse.SubtitleEdit.Logic.Plugins;
using Nikse.SubtitleEdit.Logic.UndoRedo;
using Nikse.SubtitleEdit.Logic.ValueConverters;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Nikse.SubtitleEdit.Features.Main;

public partial class MainViewModel :
    ObservableObject,
    IAdjustCallback,
    IFocusSubtitleLine,
    IUndoRedoClient,
    IFindResult,
    IApplyAssaStyles,
    IApplySsaStyles
{
    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> _subtitles;
    [ObservableProperty] private SubtitleLineViewModel? _selectedSubtitle;
    private List<SubtitleLineViewModel>? _selectedSubtitles;
    [ObservableProperty] private int? _selectedSubtitleIndex;

    [ObservableProperty] private string _editText;
    [ObservableProperty] private string _editTextCharactersPerSecond;
    [ObservableProperty] private IBrush _editTextCharactersPerSecondBackground;
    [ObservableProperty] private string _editTextTotalLength;
    [ObservableProperty] private IBrush _editTextTotalLengthBackground;

    [ObservableProperty] private string _editTextOriginal;
    [ObservableProperty] private string _editTextCharactersPerSecondOriginal;
    [ObservableProperty] private IBrush _editTextCharactersPerSecondBackgroundOriginal;
    [ObservableProperty] private string _editTextTotalLengthOriginal;
    [ObservableProperty] private IBrush _editTextTotalLengthBackgroundOriginal;

    [ObservableProperty] private ObservableCollection<SubtitleFormat> _subtitleFormats;
    [ObservableProperty] private SubtitleFormat _selectedSubtitleFormat;

    [ObservableProperty] private ObservableCollection<TextEncoding> _encodings;
    [ObservableProperty] private TextEncoding _selectedEncoding;

    [ObservableProperty] private ObservableCollection<string> _frameRates;
    [ObservableProperty] private string? _selectedFrameRate;

    [ObservableProperty] private string _statusTextLeft;
    [ObservableProperty] private string _statusTextRight;

    [ObservableProperty] private bool _isWaveformToolbarVisible;
    [ObservableProperty] private bool _isSubtitleGridFlyoutHeaderVisible;
    [ObservableProperty] private bool _isSubtitleGridDataMenuVisible;
    [ObservableProperty] private bool _isMergeWithNextOrPreviousVisible;
    [ObservableProperty] private bool _isInsertLineNoSelectionVisible;
    [ObservableProperty] private bool _showColumnOriginalText;
    [ObservableProperty] private bool _showColumnStartTime;
    [ObservableProperty] private bool _showColumnEndTime;
    [ObservableProperty] private bool _showColumnGap;
    [ObservableProperty] private bool _showColumnDuration;
    [ObservableProperty] private bool _showColumnActor;
    [ObservableProperty] private bool _showColumnStyle;
    [ObservableProperty] private bool _showColumnCps;
    [ObservableProperty] private bool _showColumnWpm;
    [ObservableProperty] private bool _showColumnPixelWidth;
    [ObservableProperty] private bool _showColumnLayer;
    [ObservableProperty] private bool _showUpDownStartTime;
    [ObservableProperty] private bool _showUpDownEndTime;
    [ObservableProperty] private bool _showUpDownDuration;
    [ObservableProperty] private bool _showUpDownLabels;
    [ObservableProperty] private bool _isColumnLayerVisible;
    [ObservableProperty] private bool _lockTimeCodes;
    [ObservableProperty] private bool _areVideoControlsUndocked;
    [ObservableProperty] private bool _isFormatAssa;
    [ObservableProperty] private bool _isFormatSsa;
    [ObservableProperty] private bool _hasFormatStyle;
    [ObservableProperty] private bool _areAssaContentMenuItemsVisible;
    [ObservableProperty] private bool _selectCurrentSubtitleWhilePlaying;
    [ObservableProperty] private bool _waveformCenter;
    [ObservableProperty] private bool _isRightToLeftEnabled;
    [ObservableProperty] private bool _showAutoTranslateSelectedLines;
    [ObservableProperty] private bool _hasMultipleLinesSelected;
    [ObservableProperty] private bool _showShotChangesListMenuItem;
    [ObservableProperty] private bool _showLayer;
    [ObservableProperty] private bool _showLayerFilterIcon;
    [ObservableProperty] private bool _showColumnLayerFlyoutMenuItem;
    [ObservableProperty] private bool _isVideoLoaded;
    [ObservableProperty] private bool _isTextBoxSplitAtCursorAndVideoPositionVisible;
    [ObservableProperty] private ObservableCollection<string> _speeds;
    [ObservableProperty] private string _selectedSpeed;
    [ObservableProperty] private bool _showWaveformDisplayModeSeparator;
    [ObservableProperty] private bool _showWaveformOnlyWaveform;
    [ObservableProperty] private bool _showWaveformOnlySpectrogram;
    [ObservableProperty] private bool _showWaveformWaveformAndSpectrogram;
    [ObservableProperty] private bool _isSmpteTimingEnabled;
    [ObservableProperty] private string _videoOffsetText;
    [ObservableProperty] private string _setVideoOffsetText;
    [ObservableProperty] private bool _isVideoOffsetVisible;
    [ObservableProperty] private bool _isAudioTracksVisible;
    [ObservableProperty] private bool _isWaveformGenerating;
    [ObservableProperty] private string _waveformGeneratingText;
    [ObservableProperty] private bool _isFilePropertiesVisible;
    [ObservableProperty] private string _filePropertiesText;
    [ObservableProperty] private string _surroundWith1Text;
    [ObservableProperty] private string _surroundWith2Text;
    [ObservableProperty] private string _surroundWith3Text;
    [ObservableProperty] private bool _isSubtitleSecondaryVisible;

    public DataGrid SubtitleGrid { get; set; }
    public Border? SubtitleGridDropHost { get; set; }
    public Window? Window { get; set; }
    public Grid ContentGrid { get; set; }
    public MainView? MainView { get; set; }
    public TextBlock StatusTextLeftLabel { get; set; }
    public MenuItem MenuReopen { get; set; }
    public MenuItem MenuPlugins { get; set; }
    public NativeMenuItem? NativeMenuReopen { get; set; }
    public NativeMenuItem? NativeMenuPlugins { get; set; }
    public NativeMenuItem? NativeMenuAudioTracks { get; set; }
    public AudioVisualizer? AudioVisualizer { get; set; }

    VideoPlayerUndockedViewModel? _videoPlayerUndockedViewModel;
    AudioVisualizerUndockedViewModel? _audioVisualizerUndockedViewModel;
    FindViewModel? _findViewModel;
    Control? _findPreviousFocus;
    bool _findClosingProgrammatically;
    ReplaceViewModel? _replaceViewModel;
    Control? _replacePreviousFocus;
    bool _replaceClosingProgrammatically;
    AdjustAllTimesViewModel? _adjustAllTimesViewModel;

    private static Color _errorColor = Se.Settings.General.ErrorColor.FromHexToColor();

    private bool _updateAudioVisualizer;
    private bool _mpvPreviewDirty = true; // true = subtitle preview needs refresh in mpv
    private string? _subtitleFileName;
    private string? _subtitleFileNameOriginal;
    private bool _converted;
    private Subtitle _subtitle;
    private Subtitle? _subtitleSecondary;
    private Subtitle _subtitleOriginal;
    private SubtitleFormat? _lastOpenSaveFormat;
    private string? _videoFileName;
    private AudioTrackInfo? _audioTrack;
    private string? _audioTrackLangauge;
    private CancellationTokenSource? _statusFadeCts;
    private CancellationTokenSource? _autoTranscribeCts;
    private int _changeSubtitleHash = -1;
    private int _changeSubtitleHashOriginal = -1;
    private bool _subtitleGridSelectionChangedSkip;
    private long _lastKeyPressedMs;
    private bool _loading;
    private bool _opening;
    private PointerEventArgs? _lastTextEditorPointerArgs;
    private HashSet<int>? _visibleLayers;
    private readonly List<SubtitleLineViewModel> _waveformSubtitleBuffer = new();
    private DispatcherTimer _positionTimer = new();
    private DispatcherTimer _slowTimer = new();
    private CancellationTokenSource _videoOpenTokenSource;
    private readonly HashSet<string> _waveformsBeingGenerated = new(StringComparer.OrdinalIgnoreCase);
    private readonly Lock _waveformsBeingGeneratedLock = new();
    private VideoPlayerControl? _fullScreenVideoPlayerControl;
    private static SolidColorBrush _transparentBrush = new SolidColorBrush(Colors.Transparent);
    private static SolidColorBrush _errorBrush = new SolidColorBrush(_errorColor);
    private SpellCheckDictionaryDisplay? _currentSpellCheckDictionary;
    private FfmpegMediaInfo2? _mediaInfo;
    string _dropDownFormatsSearchText = string.Empty;
    private System.Timers.Timer _dropDownFormatsSearchTimer = new System.Timers.Timer(1000);
    private PlaySelectionItem? _playSelectionItem;
    private int _pendingScrollIndex = -1;
    private SubtitleLineViewModel? _pendingScrollSubtitle = null;
    private readonly object _scrollLock = new object();
    private bool _changingFormatProgrammatically;
    private bool _formatChangedByUser;
    private SubtitleFormat? _formatChangedFrom;

    private readonly IFileHelper _fileHelper;
    private readonly IFolderHelper _folderHelper;
    private readonly IShortcutManager _shortcutManager;
    private readonly IWindowService _windowService;
    private readonly IInsertService _insertService;
    private readonly IMergeManager _mergeManager;
    private readonly ISplitManager _splitManager;
    private readonly IAutoBackupService _autoBackupService;
    private readonly IUndoRedoManager _undoRedoManager;
    private readonly IBluRayHelper _bluRayHelper;
    private readonly IMpvReloader _mpvReloader;
    private readonly IVlcReloader _vlcReloader;
    private readonly IFindService _findService;
    private readonly IColorService _colorService;
    private readonly IFontNameService _fontNameService;
    private readonly ISpellCheckManager _spellCheckManager;
    private readonly ICasingToggler _casingToggler;
    private readonly IPasteFromClipboardHelper _pasteFromClipboardHelper;
    private readonly IPluginCatalog _pluginCatalog;
    private readonly IPluginRunner _pluginRunner;
    private readonly IYtDlpDownloadService _ytDlpDownloadService;

    private bool IsEmpty => Subtitles.Count == 0 || (Subtitles.Count == 1 && string.IsNullOrEmpty(Subtitles[0].Text));

    private bool IsEmptyOriginal => Subtitles.Count == 0 ||
                                    (Subtitles.Count == 1 && string.IsNullOrEmpty(Subtitles[0].OriginalText));

    public VideoPlayerControl? VideoPlayerControl { get; internal set; }
    public Menu Menu { get; internal set; }
    public Border Toolbar { get; internal set; }
    public Separator? ToolbarTopSeparator { get; internal set; }
    public StackPanel PanelSingleLineLengths { get; internal set; }
    public MenuItem MenuItemMergeAsDialog { get; internal set; }
    public MenuItem MenuItemExtendToLineBefore { get; internal set; }
    public MenuItem MenuItemExtendToLineAfter { get; internal set; }
    public MenuItem MenuItemMerge { get; internal set; }
    public MenuItem MenuItemAudioVisualizerInsertNewSelection { get; set; }
    public MenuItem MenuItemAudioVisualizerPasteNewSelection { get; set; }
    public MenuItem MenuIteminsertSubtitleFileAtPositionMenuItem { get; set; }
    public MenuItem MenuItemAudioVisualizerDelete { get; set; }
    public MenuItem MenuItemAudioVisualizerInsertBefore { get; set; }
    public MenuItem MenuItemAudioVisualizerInsertAfter { get; set; }
    public MenuItem MenuItemAudioVisualizerSplit { get; set; }
    public Separator MenuItemAudioVisualizerSeparator1 { get; set; }
    public MenuItem MenuItemAudioVisualizerInsertAtPosition { get; set; }
    public MenuItem MenuItemAudioVisualizerPasteFromClipboardMenuItem { get; set; }
    public MenuItem MenuItemAudioVisualizerDeleteAtPosition { get; set; }
    public MenuItem MenuItemAudioVisualizerSplitAtPosition { get; set; }
    public MenuItem MenuItemAudioVisualizerMergeWithPrevious { get; set; }
    public MenuItem MenuItemAudioVisualizerMergeWithNext { get; set; }
    public MenuItem MenuItemAudioVisualizerSpeechToTextSelectedLines { get; set; }
    public MenuItem MenuItemAudioVisualizerSpeechToTextNewSelection { get; set; }
    public MenuItem MenuItemAudioVisualizerExtractAudio { get; set; }
    public ITextBoxWrapper EditTextBoxOriginal { get; set; }
    public ITextBoxWrapper EditTextBox { get; set; }
    public TextEditorBindingHelper? EditTextBoxHelper { get; set; }
    public TextEditorBindingHelper? EditTextBoxOriginalHelper { get; set; }
    public TextEditorBindingCoordinator? EditTextBoxBindingCoordinator { get; set; }
    public StackPanel PanelSingleLineLengthsOriginal { get; set; }
    public MenuItem MenuItemStyles { get; set; }
    public MenuItem MenuItemActors { get; set; }
    public Button ButtonWaveformPlay { get; set; }
    public MenuItem AudioTraksMenuItem { get; set; }
    public TextWithSubtitleSyntaxHighlightingConverter SubtitleDataGridSyntaxHighlighting { get; internal set; }

    public MainViewModel(
        IFileHelper fileHelper,
        IFolderHelper folderHelper,
        IShortcutManager shortcutManager,
        IWindowService windowService,
        IInsertService insertService,
        IMergeManager mergeManager,
        ISplitManager splitManager,
        IAutoBackupService autoBackupService,
        IUndoRedoManager undoRedoManager,
        IBluRayHelper bluRayHelper,
        IMpvReloader mpvReloader,
        IVlcReloader vlcReloader,
        IFindService findService,
        IDictionaryInitializer dictionaryInitializer,
        ILanguageInitializer languageInitializer,
        IOcrInitializer ocrInitializer,
        IThemeInitializer themeInitializer,
        IColorService colorService,
        IFontNameService fontNameService,
        ISpellCheckManager spellCheckManager,
        ICasingToggler casingToggler,
        IPasteFromClipboardHelper pasteFromClipboardHelper,
        IPluginCatalog pluginCatalog,
        IPluginRunner pluginRunner,
        IYtDlpDownloadService ytDlpDownloadService)
    {
        _fileHelper = fileHelper;
        _folderHelper = folderHelper;
        _shortcutManager = shortcutManager;
        _windowService = windowService;
        _insertService = insertService;
        _mergeManager = mergeManager;
        _splitManager = splitManager;
        _autoBackupService = autoBackupService;
        _undoRedoManager = undoRedoManager;
        _bluRayHelper = bluRayHelper;
        _mpvReloader = mpvReloader;
        _vlcReloader = vlcReloader;
        _findService = findService;
        _colorService = colorService;
        _fontNameService = fontNameService;
        _spellCheckManager = spellCheckManager;
        _casingToggler = casingToggler;
        _pasteFromClipboardHelper = pasteFromClipboardHelper;
        _pluginCatalog = pluginCatalog;
        _pluginRunner = pluginRunner;
        _ytDlpDownloadService = ytDlpDownloadService;

        _loading = true;
        EditText = string.Empty;
        EditTextCharactersPerSecond = string.Empty;
        EditTextCharactersPerSecondBackground = Brushes.Transparent;
        EditTextTotalLength = string.Empty;
        EditTextTotalLengthBackground = Brushes.Transparent;
        StatusTextLeftLabel = new TextBlock();
        SubtitleGrid = new DataGrid();
        EditTextBox = new TextBoxWrapper(new TextBox());
        ContentGrid = new Grid();
        MenuReopen = new MenuItem();
        MenuPlugins = new MenuItem();
        Menu = new Menu();
        PanelSingleLineLengths = new StackPanel();
        MenuItemMergeAsDialog = new MenuItem();
        MenuItemExtendToLineBefore = new MenuItem();
        MenuItemExtendToLineAfter = new MenuItem();
        MenuItemMerge = new MenuItem();
        MenuItemAudioVisualizerInsertNewSelection = new MenuItem();
        MenuItemAudioVisualizerPasteNewSelection = new MenuItem();
        MenuIteminsertSubtitleFileAtPositionMenuItem = new MenuItem();
        MenuItemAudioVisualizerDelete = new MenuItem();
        MenuItemAudioVisualizerInsertBefore = new MenuItem();
        MenuItemAudioVisualizerInsertAfter = new MenuItem();
        MenuItemAudioVisualizerSplit = new MenuItem();
        MenuItemAudioVisualizerSeparator1 = new Separator();
        MenuItemAudioVisualizerInsertAtPosition = new MenuItem();
        MenuItemAudioVisualizerPasteFromClipboardMenuItem = new MenuItem();
        MenuItemAudioVisualizerDeleteAtPosition = new MenuItem();
        MenuItemAudioVisualizerSplitAtPosition = new MenuItem();
        MenuItemAudioVisualizerMergeWithPrevious = new MenuItem();
        MenuItemAudioVisualizerMergeWithNext = new MenuItem();
        MenuItemAudioVisualizerSpeechToTextSelectedLines = new MenuItem();
        MenuItemAudioVisualizerSpeechToTextNewSelection = new MenuItem();
        MenuItemAudioVisualizerExtractAudio = new MenuItem();
        MenuItemStyles = new MenuItem();
        MenuItemActors = new MenuItem();
        AudioTraksMenuItem = new MenuItem();
        SubtitleDataGridSyntaxHighlighting = new TextWithSubtitleSyntaxHighlightingConverter();
        Toolbar = new Border();
        ButtonWaveformPlay = new Button();
        _subtitle = new Subtitle();
        _subtitleOriginal = new Subtitle();
        _videoFileName = string.Empty;
        _subtitleFileName = string.Empty;
        Subtitles = [];
        FilePropertiesText = string.Empty;

        SubtitleFormats = [.. SubtitleFormatHelper.GetSubtitleFormatsWithFavoritesAtTop()];
        _changingFormatProgrammatically = true;
        SelectedSubtitleFormat = SubtitleFormats.FirstOrDefault(p =>
            p.FriendlyName == Se.Settings.General.DefaultSubtitleFormat ||
            p.Name == Se.Settings.General.DefaultSubtitleFormat) ?? SubtitleFormats[0];
        _changingFormatProgrammatically = false;

        Encodings = new ObservableCollection<TextEncoding>(EncodingHelper.GetEncodings());
        SelectedEncoding = Encodings.FirstOrDefault(p => p.DisplayName == Se.Settings.General.DefaultEncoding) ?? Encodings[0];

        FrameRates = new ObservableCollection<string>
        {
            "23.976",
            "24",
            "25",
            "29.97",
            "30",
            "50",
            "59.94",
            "60",
            "120"
        };
        SelectedFrameRate = FrameRates[0];

        StatusTextLeft = string.Empty;
        StatusTextRight = string.Empty;
        ShowColumnStartTime = Se.Settings.General.ShowColumnStartTime;
        ShowColumnEndTime = Se.Settings.General.ShowColumnEndTime;
        ShowColumnDuration = Se.Settings.General.ShowColumnDuration;
        ShowColumnGap = Se.Settings.General.ShowColumnGap;
        ShowColumnActor = Se.Settings.General.ShowColumnActor;
        ShowColumnStyle = Se.Settings.General.ShowColumnStyle;
        ShowColumnCps = Se.Settings.General.ShowColumnCps;
        ShowColumnWpm = Se.Settings.General.ShowColumnWpm;
        ShowColumnPixelWidth = Se.Settings.General.ShowColumnPixelWidth;
        ShowColumnLayer = Se.Settings.General.ShowColumnLayer;
        ShowUpDownStartTime = Se.Settings.Appearance.ShowUpDownStartTime;
        ShowUpDownEndTime = Se.Settings.Appearance.ShowUpDownEndTime;
        ShowUpDownDuration = Se.Settings.Appearance.ShowUpDownDuration;
        ShowUpDownLabels = Se.Settings.Appearance.ShowUpDownLabels;
        SelectCurrentSubtitleWhilePlaying = Se.Settings.General.SelectCurrentSubtitleWhilePlaying;
        WaveformCenter = Se.Settings.Waveform.CenterVideoPosition;
        EditTextBoxOriginal = new TextBoxWrapper(new TextBox());
        EditTextCharactersPerSecondOriginal = string.Empty;
        EditTextCharactersPerSecondBackgroundOriginal = Brushes.Transparent;
        EditTextTotalLengthOriginal = string.Empty;
        EditTextTotalLengthBackgroundOriginal = Brushes.Transparent;
        EditTextOriginal = string.Empty;
        PanelSingleLineLengthsOriginal = new StackPanel();
        IsWaveformToolbarVisible = Se.Settings.Waveform.ShowToolbar;
        _videoOpenTokenSource = new CancellationTokenSource();
        Speeds = new ObservableCollection<string>(new[]
        {
            "0.2x",
            "0.4x",
            "0.6x",
            "0.8x",
            "1.0x",
            "1.2x",
            "1.4x",
            "1.6x",
            "1.8x",
            "2.0x",
            "2.2x",
            "2.4x",
            "2.6x",
            "2.8x",
            "3.0x"
        });
        SelectedSpeed = "1.0x";
        VideoOffsetText = string.Empty;
        SetVideoOffsetText = Se.Language.Main.Menu.SetVideoOffset;
        WaveformGeneratingText = string.Empty;

        themeInitializer.UpdateThemesIfNeeded().ConfigureAwait(true);
        Dispatcher.UIThread.Post(async void () =>
        {
            try
            {
                await languageInitializer.UpdateLanguagesIfNeeded();
                await dictionaryInitializer.UpdateDictionariesIfNeeded();
                await ocrInitializer.UpdateOcrDictionariesIfNeeded();
            }
            catch (Exception e)
            {
                Se.LogError(e);
            }
        }, DispatcherPriority.Loaded);

        if (!File.Exists(Se.GetSettingsFilePath()))
        {
            FirstLoad();
        }

        InitializeFfmpeg();
        InitializeLibMpv();
        LibVlcDynamicPlayer.LibVlcPath = Se.VlcFolder;
        LoadShortcuts();
        SurroundWith1Text = string.Format(Se.Language.Options.Shortcuts.SurroundWithXY, Se.Settings.Surround1Left, Se.Settings.Surround1Right);
        SurroundWith2Text = string.Format(Se.Language.Options.Shortcuts.SurroundWithXY, Se.Settings.Surround2Left, Se.Settings.Surround2Right);
        SurroundWith3Text = string.Format(Se.Language.Options.Shortcuts.SurroundWithXY, Se.Settings.Surround3Left, Se.Settings.Surround3Right);

        StartTimers();
        _autoBackupService.StartAutoBackup(this);
        _undoRedoManager.SetupChangeDetection(this, TimeSpan.FromSeconds(1));
        LockTimeCodes = Se.Settings.General.LockTimeCodes;
        SetLibSeSettings();
        _dropDownFormatsSearchTimer.Elapsed += (s, e) =>
        {
            _dropDownFormatsSearchText = string.Empty;
            _dropDownFormatsSearchTimer.Stop();
        };
    }

    private void FirstLoad()
    {
        try
        {
            var setupLanguageFile = Path.Combine(Se.DataFolder, "SetupLanguage.txt");
            if (File.Exists(setupLanguageFile))
            {
                var languageCode = File.ReadAllText(setupLanguageFile).Trim();
                var language = Iso639Dash2LanguageCode.List
                    .FirstOrDefault(p => languageCode.StartsWith(p.ThreeLetterCode, StringComparison.OrdinalIgnoreCase) ||
                                         languageCode.StartsWith(p.TwoLetterCode, StringComparison.OrdinalIgnoreCase));
                if (language != null)
                {
                    var translationFiles = Directory.GetFiles(Se.TranslationFolder, language.EnglishName + ".json");
                    if (translationFiles?.Length > 0)
                    {
                        Dispatcher.UIThread.Post(async void () => { await LoadLanguage(translationFiles[0]); });
                    }
                }
            }

            File.Delete(setupLanguageFile);
        }
        catch
        {
            // ignore
        }
    }

    private void SetSubtitleFormat(SubtitleFormat format)
    {
        _changingFormatProgrammatically = true;
        SelectedSubtitleFormat = format;
        _changingFormatProgrammatically = false;
    }

    private static void SetLibSeSettings()
    {
        Configuration.Settings.General.SubtitleLineMaximumLength = Se.Settings.General.SubtitleLineMaximumLength;
        Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds = Se.Settings.General.SubtitleMaximumCharactersPerSeconds;
        Configuration.Settings.General.SubtitleOptimalCharactersPerSeconds = Se.Settings.General.SubtitleOptimalCharactersPerSeconds;
        Configuration.Settings.General.SubtitleMaximumWordsPerMinute = Se.Settings.General.SubtitleMaximumWordsPerMinute;
        Configuration.Settings.General.MinimumMillisecondsBetweenLines = Se.Settings.General.MinimumBetweenLines.GetMilliseconds();
        Configuration.Settings.General.MaxNumberOfLines = Se.Settings.General.MaxNumberOfLines;
        Configuration.Settings.General.MergeLinesShorterThan = Se.Settings.General.UnbreakLinesShorterThan;

        if (Enum.TryParse<Core.Enums.DialogType>(Se.Settings.General.DialogStyle, out var dt))
        {
            Configuration.Settings.General.DialogStyle = dt;
        }

        if (Enum.TryParse<Core.Enums.ContinuationStyle>(Se.Settings.General.ContinuationStyle, out var cs))
        {
            Configuration.Settings.General.ContinuationStyle = cs;
        }

        Configuration.Settings.General.CpsLineLengthStrategy = Se.Settings.General.CpsLineLengthStrategy;

        TimedTextImscRosetta.LineHeight = Se.Settings.Formats.RosettaLineHeight;
        TimedTextImscRosetta.FontSize = Se.Settings.Formats.RosettaFontSize;
        if (Se.Settings.Formats.RosettaLanguageAutoDetect)
        {
            TimedTextImscRosetta.Language = string.Empty;
        }
        else
        {
            TimedTextImscRosetta.Language = Se.Settings.Formats.RosettaLanguage;
        }

        TmpegEncXml.FontName = Se.Settings.Formats.TmpegEncXmlFontName;
        TmpegEncXml.FontHeight = Se.Settings.Formats.TmpegEncXmlFontHeight.ToString(CultureInfo.InvariantCulture);
        TmpegEncXml.OffsetX = Se.Settings.Formats.TmpegEncXmlOffsetX.ToString(CultureInfo.InvariantCulture);
        TmpegEncXml.OffsetY = Se.Settings.Formats.TmpegEncXmlOffsetY.ToString(CultureInfo.InvariantCulture);
        TmpegEncXml.FontBold = Se.Settings.Formats.TmpegEncXmlFontBold ? "1" : "0";

        Configuration.Settings.SubtitleSettings.DCinemaFontFile = Se.Settings.File.DCinemaSmpte.DCinemaFontFile;
        Configuration.Settings.SubtitleSettings.DCinemaLoadFontResource = Se.Settings.File.DCinemaSmpte.DCinemaLoadFontResource;
        Configuration.Settings.SubtitleSettings.DCinemaFontSize = Se.Settings.File.DCinemaSmpte.DCinemaFontSize;
        Configuration.Settings.SubtitleSettings.DCinemaBottomMargin = Se.Settings.File.DCinemaSmpte.DCinemaBottomMargin;
        Configuration.Settings.SubtitleSettings.DCinemaZPosition = Se.Settings.File.DCinemaSmpte.DCinemaZPosition;
        Configuration.Settings.SubtitleSettings.DCinemaFadeUpTime = Se.Settings.File.DCinemaSmpte.DCinemaFadeUpTime;
        Configuration.Settings.SubtitleSettings.DCinemaFadeDownTime = Se.Settings.File.DCinemaSmpte.DCinemaFadeDownTime;
        Configuration.Settings.SubtitleSettings.DCinemaAutoGenerateSubtitleId = Se.Settings.File.DCinemaSmpte.DCinemaAutoGenerateSubtitleId;
    }

    private static void InitializeFfmpeg()
    {
        if (!string.IsNullOrEmpty(Se.Settings.General.FfmpegPath) &&
            File.Exists(Se.Settings.General.FfmpegPath))
        {
            return;
        }

        var ffmpegFileName = DownloadFfmpegViewModel.GetFfmpegFileName();
        if (!string.IsNullOrEmpty(ffmpegFileName) && File.Exists(ffmpegFileName))
        {
            Se.Settings.General.FfmpegPath = DownloadFfmpegViewModel.GetFfmpegFileName();
        }
    }

    private void InitializeLibMpv()
    {
        LibMpvDynamicPlayer.MpvPath = Se.DataFolder;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var newFileName = DownloadLibMpvViewModel.GetFallbackLibMpvFileName(false);
            if (string.IsNullOrEmpty(Se.Settings.General.LibMpvPath) || !File.Exists(Se.Settings.General.LibMpvPath) || File.Exists(newFileName))
            {
                var libMpvFileName = DownloadLibMpvViewModel.GetLibMpvFileName();
                if (File.Exists(newFileName))
                {
                    try
                    {
                        if (File.Exists(libMpvFileName))
                        {
                            File.Delete(libMpvFileName);
                        }

                        File.Move(newFileName, libMpvFileName);

                        Directory.Delete(Path.GetDirectoryName(newFileName)!);
                    }
                    catch
                    {
                        // ignore
                    }
                }

                if (File.Exists(libMpvFileName))
                {
                    Se.Settings.General.LibMpvPath = libMpvFileName;

                    Dispatcher.UIThread.Post(async void () =>
                    {
                        var _ = await RequireFfmpegOk();
                    });
                }
                else
                {
                    var baseFileName = Path.Combine(AppContext.BaseDirectory, "libmpv-2.dll");
                    if (File.Exists(baseFileName))
                    {
                        Se.Settings.General.LibMpvPath = baseFileName;
                    }
                }
            }
        }
    }

    private void LoadShortcuts()
    {
        Se.Settings.InitializeMainShortcuts(this);
        foreach (var shortCut in ShortcutsMain.GetUsedShortcuts(this))
        {
            _shortcutManager.RegisterShortcut(shortCut);
        }
    }

    private void ReloadShortcuts()
    {
        _shortcutManager.ClearShortcuts();
        Se.Settings.InitializeMainShortcuts(this);
        // Rebuild shortcut display names so the Shortcuts window reflects the current
        // UI language (the dictionary captured Se.Language strings at type init).
        ShortcutsMain.ReloadCommandTranslations();
        foreach (var shortCut in ShortcutsMain.GetUsedShortcuts(this))
        {
            _shortcutManager.RegisterShortcut(shortCut);
        }

        SurroundWith1Text = string.Format(Se.Language.Options.Shortcuts.SurroundWithXY, Se.Settings.Surround1Left, Se.Settings.Surround1Right);
        SurroundWith2Text = string.Format(Se.Language.Options.Shortcuts.SurroundWithXY, Se.Settings.Surround2Left, Se.Settings.Surround2Right);
        SurroundWith3Text = string.Format(Se.Language.Options.Shortcuts.SurroundWithXY, Se.Settings.Surround3Left, Se.Settings.Surround3Right);
    }

    [RelayCommand]
    private void CommandExit()
    {
        if (Window == null)
        {
            return;
        }

        Window.Close();
    }

    [RelayCommand]
    private async Task CommandShowLayout()
    {
        var vm = await ShowDialogAsync<LayoutWindow, LayoutViewModel>(viewModel => { viewModel.SelectedLayout = Se.Settings.General.LayoutNumber; });

        if (vm.OkPressed && vm.SelectedLayout != null && vm.SelectedLayout != Se.Settings.General.LayoutNumber)
        {
            if (AreVideoControlsUndocked)
            {
                VideoRedockControls();
            }

            SetLayout(vm.SelectedLayout.Value);
            AutoFitColumns();
        }
    }

    private void SetLayout(int layoutNumber)
    {
        var idx = SubtitleGrid.SelectedIndex;
        var savedAudioTrack = _audioTrack;
        Se.Settings.General.LayoutNumber = InitLayout.MakeLayout(MainView!, this, layoutNumber);
        SelectAndScrollToRow(Math.Max(0, idx));
        Dispatcher.UIThread.Post(() => SubtitleGrid.Focus());
        RefreshSubtitlePreview();

        if (savedAudioTrack != null && !string.IsNullOrEmpty(_videoFileName))
        {
            Dispatcher.UIThread.Post(async () =>
            {
                var vp = GetVideoPlayerControl();
                if (vp?.VideoPlayer is LibMpvDynamicPlayer mpv)
                {
                    await vp.WaitForPlayersReadyAsync();
                    mpv.SetAudioTrack(savedAudioTrack.Id);
                    var _ = Task.Run(LoadAudioTrackMenuItems);
                }
            });
        }
    }

    private void RefreshSubtitlePreview()
    {
        _mpvReloader.Reset();
        _vlcReloader.Reset();
        _mpvPreviewDirty = true;
    }

    [RelayCommand]
    private void Play()
    {
        var vp = GetVideoPlayerControl();
        if (vp == null)
        {
            return;
        }

        vp.VideoPlayer.Play();
    }

    [RelayCommand]
    private void PlayNext()
    {
        var vp = GetVideoPlayerControl();
        if (vp == null)
        {
            return;
        }

        var next = Subtitles.FirstOrDefault(s => s.StartTime.TotalSeconds >= vp.Position);
        if (next == null)
        {
            return;
        }

        vp.Position = next.StartTime.TotalSeconds;
        SelectAndScrollToSubtitle(next);
        vp.VideoPlayer.Play();
    }

    [RelayCommand]
    private void Pause()
    {
        var vp = GetVideoPlayerControl();
        if (vp == null)
        {
            return;
        }

        vp.VideoPlayer.Pause();
    }

    [RelayCommand]
    private void TogglePlayPause()
    {
        var control = GetVideoPlayerControl();
        if (control == null)
        {
            return;
        }

        control.TogglePlayPause();
    }

    [RelayCommand]
    private void TogglePlayPause2()
    {
        TogglePlayPause();
    }

    [RelayCommand]
    private void ToggleLockTimeCodes()
    {
        LockTimeCodes = !LockTimeCodes;
        Se.Settings.General.LockTimeCodes = LockTimeCodes;
        if (AudioVisualizer != null)
        {
            AudioVisualizer.IsReadOnly = LockTimeCodes;
        }
    }

    [RelayCommand]
    private async Task ShowHelp()
    {
        UiUtil.ShowHelp("index");
    }

    [RelayCommand]
    private async Task ShowSourceView()
    {
        var result = await ShowDialogAsync<SourceViewWindow, SourceViewViewModel>(vm =>
        {
            var text = GetUpdateSubtitle().ToText(SelectedSubtitleFormat);
            var title = string.Format(Se.Language.General.SourceViewX, (string.IsNullOrEmpty(_subtitleFileName)
                ? Se.Language.General.Untitled
                : Path.GetFileName(_subtitleFileName)));
            vm.Initialize(title, text, SelectedSubtitleFormat);
        });

        var oldSelectedIndex = SelectedSubtitleIndex ?? 0;

        if (result.OkPressed)
        {
            SetSubtitles(result.Subtitle);
            var idx = Math.Min(oldSelectedIndex, Subtitles.Count - 1);
            SelectAndScrollToRow(idx);
            _updateAudioVisualizer = true;
        }
    }

    [RelayCommand]
    private async Task ShowAssaStyles()
    {
        if (Window == null || !IsFormatAssa)
        {
            return;
        }

        var result = await ShowDialogAsync<AssaStylesWindow, AssaStylesViewModel>(vm =>
        {
            vm.Initialize(_subtitle, SelectedSubtitleFormat, _subtitleFileName ?? string.Empty,
                SelectedSubtitle?.Style ?? string.Empty, this);
        });

        if (result.OkPressed)
        {
            ApplyAssaStyles(result);
            _subtitle.Footer = result.ResultSubtitle.Footer;

            var styles = AdvancedSubStationAlpha.GetStylesFromHeader(_subtitle.Header);
            foreach (var s in Subtitles)
            {
                if (!styles.Contains(s.Style))
                {
                    s.Style = styles.FirstOrDefault() ?? "Default";
                }
            }

            RefreshSubtitlePreview();
        }
    }

    public void ApplyAssaStyles(AssaStylesViewModel result)
    {
        _subtitle.Header = result.Header;
        var styles = AdvancedSubStationAlpha.GetStylesFromHeader(_subtitle.Header);
        var first = styles.FirstOrDefault() ?? "Default";

        for (var i = 0; i < Subtitles.Count; i++)
        {
            var s = Subtitles[i];

            if (string.IsNullOrEmpty(s.Style) || !styles.Contains(s.Style))
            {
                s.Style = first;
            }

            if (i < result.ResultSubtitle.Paragraphs.Count)
            {
                var extra = result.ResultSubtitle.Paragraphs[i].Extra;
                if (!string.IsNullOrEmpty(extra))
                {
                    s.Style = extra.TrimStart('*');
                }
            }
        }

        RefreshSubtitlePreview();
    }

    [RelayCommand]
    private async Task ShowSsaStyles()
    {
        if (Window == null || !IsFormatSsa)
        {
            return;
        }

        var result = await ShowDialogAsync<SsaStylesWindow, SsaStylesViewModel>(vm =>
        {
            vm.Initialize(_subtitle, SelectedSubtitleFormat, _subtitleFileName ?? string.Empty,
                SelectedSubtitle?.Style ?? string.Empty, this);
        });

        if (result.OkPressed)
        {
            ApplySsaStyles(result);
            _subtitle.Footer = result.ResultSubtitle.Footer;

            var styles = AdvancedSubStationAlpha.GetStylesFromHeader(_subtitle.Header);
            foreach (var s in Subtitles)
            {
                if (!styles.Contains(s.Style))
                {
                    s.Style = styles.FirstOrDefault() ?? "Default";
                }
            }

            RefreshSubtitlePreview();
        }
    }

    public void ApplySsaStyles(SsaStylesViewModel result)
    {
        _subtitle.Header = result.Header;
        var styles = AdvancedSubStationAlpha.GetStylesFromHeader(_subtitle.Header);
        var first = styles.FirstOrDefault() ?? "Default";

        for (var i = 0; i < Subtitles.Count; i++)
        {
            var s = Subtitles[i];

            if (string.IsNullOrEmpty(s.Style) || !styles.Contains(s.Style))
            {
                s.Style = first;
            }

            if (i < result.ResultSubtitle.Paragraphs.Count)
            {
                var extra = result.ResultSubtitle.Paragraphs[i].Extra;
                if (!string.IsNullOrEmpty(extra))
                {
                    s.Style = extra.TrimStart('*');
                }
            }
        }

        RefreshSubtitlePreview();
    }

    [RelayCommand]
    private async Task ShowAssaProperties()
    {
        if (Window == null || !IsFormatAssa)
        {
            return;
        }

        var result = await ShowDialogAsync<AssaPropertiesWindow, AssaPropertiesViewModel>(vm =>
        {
            vm.Initialize(_subtitle, SelectedSubtitleFormat, _subtitleFileName ?? string.Empty,
                _videoFileName ?? string.Empty);
        });

        if (result.OkPressed)
        {
            _subtitle.Header = result.Header;
            RefreshSubtitlePreview();
        }
    }

    [RelayCommand]
    private async Task ShowAssaAttachments()
    {
        if (Window == null || !IsFormatAssa)
        {
            return;
        }

        var result = await ShowDialogAsync<AssaAttachmentsWindow, AssaAttachmentsViewModel>(vm =>
        {
            vm.Initialize(GetUpdateSubtitle(), SelectedSubtitleFormat, _subtitleFileName ?? string.Empty);
        });

        if (result.OkPressed)
        {
            _subtitle.Header = result.Header;
            _subtitle.Footer = result.Footer;
            RefreshSubtitlePreview();
        }
    }

    [RelayCommand]
    private async Task ShowAssaDraw()
    {
        if (Window == null || !IsFormatAssa)
        {
            return;
        }

        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();

        SetAssaResolution(false);

        var result = await ShowDialogAsync<AssaDrawWindow, AssaDrawViewModel>(vm =>
        {
            vm.Initialize(GetUpdateSubtitle(), selectedItems, _mediaInfo?.Dimension.Width, _mediaInfo?.Dimension.Height);
        });

        if (!result.OkPressed)
        {
            return;
        }

        _subtitle = result.ResultSubtitle;
        var assa = new SubStationAlpha();
        var firstParagraph = selectedItems.FirstOrDefault();
        var lastParagraph = selectedItems.LastOrDefault();
        if (lastParagraph == null)
        {
            lastParagraph = new SubtitleLineViewModel()
            {
                StartTime = TimeSpan.FromSeconds(firstParagraph != null ? firstParagraph.StartTime.TotalSeconds : 0),
                EndTime = TimeSpan.FromSeconds(firstParagraph != null ? firstParagraph.EndTime.TotalSeconds : 2),
                Text = string.Empty
            };
        }

        for (var index = 0; index < result.ResultSubtitle.Paragraphs.Count; index++)
        {
            var p = result.ResultSubtitle.Paragraphs[index];
            if (index < selectedItems.Count)
            {
                selectedItems[index].Text = p.Text;
                selectedItems[index].Style = p.Extra;
                selectedItems[index].Layer = p.Layer;
                lastParagraph = selectedItems[index];
            }
            else
            {
                var newP = new SubtitleLineViewModel(p, assa);
                newP.StartTime = lastParagraph.StartTime;
                newP.EndTime = lastParagraph.EndTime;
                var insertIndex = Subtitles.IndexOf(lastParagraph) + 1;
                if (insertIndex <= 0)
                {
                    insertIndex = Subtitles.Count;
                }

                Subtitles.Insert(insertIndex, newP);
                lastParagraph = newP;
            }
        }

        Renumber();
        RefreshSubtitlePreview();
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private async Task ShowAssaGenerateProgressBar()
    {
        if (!IsFormatAssa)
        {
            return;
        }

        SetAssaResolution(false);

        var result = await ShowDialogAsync<AssaProgressBarWindow, AssaProgressBarViewModel>(vm =>
        {
            var width = _mediaInfo?.Dimension.Width ?? 1920;
            var height = _mediaInfo?.Dimension.Height ?? 1080;
            var duration = (double)(_mediaInfo?.Duration?.TotalMilliseconds ?? 60_000);
            vm.Initialize(_subtitle, width, height, duration, _videoFileName);
        });

        if (!result.OkPressed)
        {
            return;
        }

        _subtitle = result.ResultSubtitle;
        SetSubtitles(_subtitle);

        Renumber();
        _updateAudioVisualizer = true;
        RefreshSubtitlePreview();
    }

    [RelayCommand]
    private async Task ShowAssaChangeResolution()
    {
        var result = await ShowDialogAsync<AssaResolutionResamplerWindow, AssaResolutionResamplerViewModel>(vm =>
        {
            vm.Initialize(GetUpdateSubtitle(), _videoFileName, _mediaInfo?.Dimension.Width, _mediaInfo?.Dimension.Height);
        });

        if (!result.OkPressed)
        {
            return;
        }

        SetSubtitles(result.ResultSubtitle);
        _subtitle.Header = result.ResultSubtitle.Header;
        ShowStatus(Se.Language.Main.AssaResolutionResamplerDone);
        RefreshSubtitlePreview();
    }

    [RelayCommand]
    private async Task ShowAssaGenerateBackground()
    {
        if (string.IsNullOrEmpty(_videoFileName) || !IsFormatAssa)
        {
            return;
        }

        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
        if (selectedItems.Count == 0)
        {
            return;
        }

        var ffmpegOk = await RequireFfmpegOk();
        if (!ffmpegOk)
        {
            return;
        }

        SetAssaResolution(false);

        var result = await ShowDialogAsync<AssSetBackgroundWindow, AssSetBackgroundViewModel>(vm =>
        {
            vm.Initialize(GetUpdateSubtitle(), selectedItems, _mediaInfo?.Dimension.Width ?? 1920, _mediaInfo?.Dimension.Height ?? 1080, _videoFileName);
        });

        if (!result.OkPressed)
        {
            return;
        }

        _subtitle.Header = result.ResultSubtitle.Header;
        SetSubtitles(result.ResultSubtitle);
        RefreshSubtitlePreview();
    }

    [RelayCommand]
    private async Task ShowAssaImageColorPicker()
    {
        var ffmpegOk = await RequireFfmpegOk();
        if (!ffmpegOk)
        {
            return;
        }

        var selectedItem = SubtitleGrid.SelectedItem as SubtitleLineViewModel;
        if (selectedItem == null || string.IsNullOrEmpty(_videoFileName))
        {
            return;
        }

        var result = await ShowDialogAsync<AssaImageColorPickerWindow, AssaImageColorPickerViewModel>(vm =>
        {
            vm.Initialize(_subtitle, selectedItem, _videoFileName, _mediaInfo?.Dimension.Width, _mediaInfo?.Dimension.Height);
        });

        RefreshSubtitlePreview();
    }

    [RelayCommand]
    private async Task ShowAssaSetPosition()
    {
        var selectedItem = SubtitleGrid.SelectedItem as SubtitleLineViewModel;
        if (selectedItem == null)
        {
            return;
        }

        var ffmpegOk = await RequireFfmpegOk();
        if (!ffmpegOk)
        {
            return;
        }

        var result = await ShowDialogAsync<AssaSetPositionWindow, AssaSetPositionViewModel>(vm =>
        {
            vm.Initialize(_subtitle, selectedItem, _videoFileName, _mediaInfo?.Dimension.Width, _mediaInfo?.Dimension.Height);
        });

        if (!result.OkPressed)
        {
            return;
        }

        var x = result.ResultX;
        var y = result.ResultY;
        selectedItem.Text = $"{{\\pos({x},{y})}}" + RemovePositionTags(selectedItem.Text);
        RefreshSubtitlePreview();
    }

    private static string RemovePositionTags(string text)
    {
        string result = Regex.Replace(text, @"\\pos\(\d+,\d+\)", string.Empty).Replace("{}", string.Empty);
        return result;
    }

    [RelayCommand]
    private async Task ShowAssaApplyAdvancedEffect()
    {
        var result = await ShowDialogAsync<AssaApplyAdvancedEffectWindow, AssaApplyAdvancedEffectViewModel>(vm =>
        {
            var paragraphs = Subtitles.Select(p => new SubtitleLineViewModel(p)).ToList();
            var selectedParagraphs = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
            vm.Initialize(GetUpdateSubtitle(), paragraphs, selectedParagraphs, _videoFileName, _mediaInfo, AudioVisualizer);
        });

        if (result.OkPressed)
        {
            SetSubtitles(result.UpdatedSubtitle);
        }
    }

    [RelayCommand]
    private async Task ShowAssaApplyCustomOverrideTags()
    {
        var result = await ShowDialogAsync<AssaApplyCustomOverrideTagsWindow, AssaApplyCustomOverrideTagsViewModel>(vm =>
        {
            var paragraphs = Subtitles.Select(p => new SubtitleLineViewModel(p)).ToList();
            var selectedParagraphs = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
            vm.Initialize(paragraphs, selectedParagraphs, _videoFileName);
        });

        if (result.OkPressed && result.UpdatedSubtitle.Paragraphs.Count == Subtitles.Count)
        {
            for (int i = 0; i < Subtitles.Count; i++)
            {
                Subtitles[i].Text = result.UpdatedSubtitle.Paragraphs[i].Text;
            }

            _updateAudioVisualizer = true;
        }
    }

    [RelayCommand]
    private async Task SaveLanguageFile()
    {
        if (Window == null)
        {
            return;
        }

        var json = JsonSerializer.Serialize(Se.Language, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        });

        var currentDirectory = Directory.GetCurrentDirectory();
        var fileName = Path.Combine(currentDirectory, "English.json");
        await File.WriteAllTextAsync(fileName, json, Encoding.UTF8);
        ShowStatus(string.Format(Se.Language.Main.LanguageFileSavedToX, fileName));
        await _folderHelper.OpenFolderWithFileSelected(Window, fileName);

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowAbout()
    {
        await ShowDialogAsync<AboutWindow, AboutViewModel>();
    }

    [RelayCommand]
    private async Task ShowCheckForUpdates()
    {
        await ShowDialogAsync<CheckForUpdatesWindow, CheckForUpdatesViewModel>();
    }

    [RelayCommand]
    private async Task CommandFileNew()
    {
        var doContinue = await HasChangesContinue();
        if (!doContinue)
        {
            return;
        }

        AddToRecentFiles(true);
        ResetSubtitle();
        VideoCloseFile();
        AddToRecentFiles(false);
    }

    [RelayCommand]
    private async Task CommandFileNewKeepVideo()
    {
        var doContinue = await HasChangesContinue();
        if (!doContinue)
        {
            return;
        }

        AddToRecentFiles(true);
        ResetSubtitle();
        AddToRecentFiles(false);
    }

    private void ResetSubtitle(SubtitleFormat? format = null)
    {
        _videoOpenTokenSource?.Cancel();
        ResetPlaySelection();
        ClearSecondarySubtitle();
        IsSmpteTimingEnabled = false;
        ShowColumnOriginalText = false;
        _subtitle.Paragraphs.Clear();
        Subtitles.Clear();
        Se.Settings.General.CurrentVideoIsSmpte = false;
        Se.Settings.General.CurrentVideoOffsetInMs = 0;
        UpdateVideoOffsetStatus();
        _currentSpellCheckDictionary = null;

        if (format != null)
        {
            SetSubtitleFormat(SubtitleFormats.FirstOrDefault(f => f.FriendlyName == format.FriendlyName)
                              ?? SubtitleFormats[0]);
        }
        else
        {
            SetSubtitleFormat(
                SubtitleFormats.FirstOrDefault(f => f.FriendlyName == Se.Settings.General.DefaultSubtitleFormat) ??
                SubtitleFormats[0]);
        }

        SelectedEncoding = Encodings.FirstOrDefault(p => p.DisplayName == Se.Settings.General.DefaultEncoding) ??
                           Encodings[0];
        _subtitleFileName = string.Empty;
        _subtitle = new Subtitle();
        _changeSubtitleHash = GetFastHash();

        _subtitleFileNameOriginal = string.Empty;
        _subtitleOriginal = new Subtitle();
        _changeSubtitleHashOriginal = GetFastHashOriginal();

        _visibleLayers = null;
        ShowLayerFilterIcon = false;

        AutoFitColumns();
        SubtitleGridSelectionChanged();
        RefreshSubtitlePreview();

        if (_findViewModel != null)
        {
            _findViewModel.Window?.Close();
            _findViewModel = null;
        }

        if (_replaceViewModel != null)
        {
            _replaceViewModel.Window?.Close();
            _replaceViewModel = null;
        }

        if (_adjustAllTimesViewModel != null)
        {
            if (_adjustAllTimesViewModel.Window is { IsVisible: true })
            {
                _adjustAllTimesViewModel.ResetForNewSubtitle();
            }
            else
            {
                _adjustAllTimesViewModel = null;
            }
        }

        var vp = GetVideoPlayerControl();
        if (vp != null)
        {
            vp.IsSmpteTimingEnabled = IsSmpteTimingEnabled;
        }

        _mpvReloader.SmpteMode = IsSmpteTimingEnabled;
        _vlcReloader.SmpteMode = IsSmpteTimingEnabled;

        _formatChangedByUser = false;
        _undoRedoManager.Reset();
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task FileOpenOriginal()
    {
        var selectedIndex = SelectedSubtitleIndex ?? 0;

        var fileName =
            await _fileHelper.PickOpenSubtitleFile(Window!, Se.Language.General.OpenOriginalSubtitleFileTitle);
        if (string.IsNullOrEmpty(fileName))
        {
            _shortcutManager.ClearKeys();
            return;
        }

        await SubtitleOpenOriginal(selectedIndex, fileName);

        _shortcutManager.ClearKeys();
    }

    private async Task<bool> SubtitleOpenOriginal(int selectedIndex, string fileName)
    {
        var subtitle = Subtitle.Parse(fileName);
        if (subtitle == null)
        {
            foreach (var f in SubtitleFormat.GetBinaryFormats(false))
            {
                if (f.IsMine(null, fileName))
                {
                    subtitle = new Subtitle();
                    f.LoadSubtitle(subtitle, null, fileName);
                    subtitle.OriginalFormat = f;
                    break; // format found, exit the loop
                }
            }

            if (subtitle == null)
            {
                var message = Se.Language.General.UnknownSubtitleFormat;
                await MessageBox.Show(Window!, Se.Language.General.Error, message, MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                _shortcutManager.ClearKeys();
                return false;
            }
        }

        if (subtitle.Paragraphs.Count == Subtitles.Count)
        {
            ImportOriginalSubtitle(selectedIndex, fileName, subtitle);
            return true;
        }
        else
        {
            var msg = string.Format(Se.Language.Main.OpenOriginalDifferentNumberOfSubtitlesXY, subtitle.Paragraphs.Count, Subtitles.Count);
            await MessageBox.Show(Window!, Se.Language.General.Information, msg, MessageBoxButtons.OK, MessageBoxIcon.Error);

            var newOriginal = ImportOriginalHelper.GetMatchingOriginalLines(Subtitles, subtitle);
            var originalWithTextCount = newOriginal.Paragraphs.Count(p => !string.IsNullOrEmpty(p.Text));
            if (originalWithTextCount > 0)
            {
                msg = string.Format(Se.Language.Main.ImportXMatchingOriginalLines, originalWithTextCount);
                var answer = await MessageBox.Show(Window!, string.Empty, msg, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (answer == MessageBoxResult.Yes)
                {
                    ImportOriginalSubtitle(selectedIndex, fileName, newOriginal);
                    return true;
                }
            }
        }

        return false;
    }

    private void ImportOriginalSubtitle(int selectedIndex, string fileName, Subtitle subtitle)
    {
        _subtitleOriginal = subtitle;
        _subtitleFileNameOriginal = fileName;
        for (var i = 0; i < Subtitles.Count; i++)
        {
            Subtitles[i].OriginalText = subtitle.Paragraphs[i].Text;
        }

        _changeSubtitleHashOriginal = GetFastHashOriginal();
        ShowColumnOriginalText = true;

        AutoFitColumns();
        SelectAndScrollToRow(selectedIndex);
        AddToRecentFiles(true);
    }

    [RelayCommand]
    private void FileCloseOriginal()
    {
        ShowColumnOriginalText = false;
        AutoFitColumns();
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task FileCloseTranslation()
    {
        if (!ShowColumnOriginalText)
        {
            return;
        }

        if (_changeSubtitleHash != GetFastHash() && !IsEmpty)
        {
            var name = string.IsNullOrEmpty(_subtitleFileName) ? Se.Language.General.Untitled : _subtitleFileName;
            var promptText = string.Format(Se.Language.General.SaveChangesToX, name);
            var dr = await MessageBox.Show(Window!, Se.Language.General.SaveChangesTitle, promptText,
                MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (dr == MessageBoxResult.Cancel)
            {
                return;
            }

            if (dr == MessageBoxResult.Yes)
            {
                if (string.IsNullOrEmpty(_subtitleFileName))
                {
                    var saved = await SaveSubtitleAs();
                    if (!saved)
                    {
                        return;
                    }
                }
                else
                {
                    await SaveSubtitle();
                }
            }
        }

        foreach (var subtitle in Subtitles)
        {
            subtitle.Text = subtitle.OriginalText;
            subtitle.OriginalText = string.Empty;
        }

        _subtitleFileName = _subtitleFileNameOriginal;
        _subtitleFileNameOriginal = string.Empty;
        _subtitleOriginal = new Subtitle();
        _changeSubtitleHash = GetFastHash();
        _changeSubtitleHashOriginal = GetFastHashOriginal();
        ShowColumnOriginalText = false;
        AutoFitColumns();
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task CommandFileOpen()
    {
        var doContinue = await HasChangesContinue();
        if (!doContinue)
        {
            return;
        }

        var fileName = await _fileHelper.PickOpenSubtitleFile(Window!, Se.Language.General.OpenSubtitleFileTitle, lastOpenedFilePath: _subtitleFileName);
        if (!string.IsNullOrEmpty(fileName))
        {
            VideoCloseFile();
            await SubtitleOpen(fileName);
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task OpenSecondarySubtitle()
    {
        if (Window == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickOpenSubtitleFile(Window!, Se.Language.General.OpenSubtitleFileTitle, lastOpenedFilePath: _subtitleFileName);
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var subtitle = Subtitle.Parse(fileName);
        if (subtitle == null)
        {
            return;
        }

        var result = await ShowDialogAsync<OpenSecondarySubtitleWindow, OpenSecondarySubtitleViewModel>(vm =>
        {
            vm.Initialize(subtitle, GetUpdateSubtitle(), SelectedSubtitleFormat, _mediaInfo, _videoFileName);
        });

        if (!result.OkPressed)
        {
            return;
        }

        _subtitleSecondary = result.ResultSubtitle;
        IsSubtitleSecondaryVisible = true;

        var vp = GetVideoPlayerControl();
        if (vp != null)
        {
            if (vp.VideoPlayer is LibMpvDynamicPlayer mpv)
            {
                _mpvReloader.Reset();
                _ = _mpvReloader.RefreshMpv(mpv, GetUpdateSubtitle(), _subtitleSecondary, SelectedSubtitleFormat);
            }
            else if (vp.VideoPlayer is LibVlcDynamicPlayer vlc)
            {
                _vlcReloader.Reset();
                _ = _vlcReloader.RefreshVlc(vlc, GetUpdateSubtitle(), _subtitleSecondary, SelectedSubtitleFormat);
            }
        }
    }

    [RelayCommand]
    private void ClearSecondarySubtitle()
    {
        IsSubtitleSecondaryVisible = false;
        _subtitleSecondary = null;
    }

    [RelayCommand]
    private async Task CommandFileOpenKeepVideo()
    {
        var doContinue = await HasChangesContinue();
        if (!doContinue)
        {
            return;
        }

        var fileName = await _fileHelper.PickOpenSubtitleFile(Window!, Se.Language.General.OpenSubtitleFileTitle, lastOpenedFilePath: _subtitleFileName);
        if (!string.IsNullOrEmpty(fileName))
        {
            await SubtitleOpen(fileName, skipLoadVideo: true);
        }

        _shortcutManager.ClearKeys();
    }


    [RelayCommand]
    private void CommandFileReopen(RecentFile recentFile)
    {
        var idx = SelectedSubtitleIndex;
        if (idx.HasValue &&
            recentFile.VideoFileName == _videoFileName &&
            recentFile.SubtitleFileName == _subtitleFileName)
        {
            recentFile.SelectedLine = idx.Value;
        }

        Dispatcher.UIThread.Post(async void () =>
        {
            var doContinue = await HasChangesContinue();
            if (!doContinue)
            {
                return;
            }

            VideoCloseFile();
            await SubtitleOpen(recentFile.SubtitleFileName, recentFile.VideoFileName, recentFile.SelectedLine, desiredAudioTrackId: recentFile.AudioTrack);

            if (!string.IsNullOrEmpty(recentFile.SubtitleFileNameOriginal) &&
                File.Exists(recentFile.SubtitleFileNameOriginal))
            {
                var selectedIndex = recentFile.SelectedLine;
                await SubtitleOpenOriginal(selectedIndex, recentFile.SubtitleFileNameOriginal);
            }

            SetRecentFileProperties(recentFile);

            _shortcutManager.ClearKeys();
        });
    }

    private void SetRecentFileProperties(RecentFile recentFile)
    {
        if (recentFile.VideoIsSmpte)
        {
            IsSmpteTimingEnabled = false;
            ToggleSmpteTiming();
        }
        else
        {
            IsSmpteTimingEnabled = false;
        }

        Se.Settings.General.CurrentVideoOffsetInMs = recentFile.VideoOffsetInMs;
        UpdateVideoOffsetStatus();

        var vp = GetVideoPlayerControl();

        if (vp != null && vp.VideoPlayer is LibMpvDynamicPlayer mpv)
        {
            var audioTracks = mpv.GetAudioTracks();
            var desiredTrack = audioTracks.FirstOrDefault(p => p.Id == recentFile.AudioTrack);

            // Only switch track and regenerate waveform if different from current
            if (desiredTrack != null && (_audioTrack == null || _audioTrack.Id != desiredTrack.Id))
            {
                _audioTrack = desiredTrack;
                var _ = Task.Run(() => PickAudioTrack(_audioTrack));
            }
        }
    }

    [RelayCommand]
    private void CommandFileClearRecentFiles(RecentFile recentFile)
    {
        Se.Settings.File.RecentFiles.Clear();
        InitMenu.UpdateRecentFiles(this);
        if (OperatingSystem.IsMacOS())
            Layout.InitNativeMacMenu.UpdateRecentFiles(this);
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task CommandFileSave()
    {
        var saved = await SaveSubtitle();
        var savedOriginal = false;

        if (ShowColumnOriginalText && _changeSubtitleHashOriginal != GetFastHashOriginal())
        {
            savedOriginal = await SaveSubtitleOriginal();
        }

        if (saved && savedOriginal)
        {
            ShowStatus(string.Format(Se.Language.General.SavedChangesToXAndY, _subtitleFileName, _subtitleFileNameOriginal));
        }
        else if (saved)
        {
            ShowStatus(string.Format(Se.Language.General.SavedChangesToX, _subtitleFileName));
        }
        else if (savedOriginal)
        {
            ShowStatus(string.Format(Se.Language.General.SavedChangesToX, _subtitleFileNameOriginal));
        }
    }

    [RelayCommand]
    private async Task CommandFileSaveAs()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        var saved = await SaveSubtitleAs();
        if (saved)
        {
            ShowStatus(string.Format(Se.Language.General.SavedChangesToX, _subtitleFileName));
        }
    }

    [RelayCommand]
    private async Task FilePropertiesShow()
    {
        if (Window == null)
        {
            return;
        }

        var format = SelectedSubtitleFormat;

        if (format is TimedTextImsc11)
        {
            var result = await ShowDialogAsync<TimedTextImsc11PropertiesWindow, TimedTextImsc11PropertiesViewModel>(vm => { vm.Initialize(GetUpdateSubtitle()); });
            SetLibSeSettings();
        }

        if (format is ItunesTimedText)
        {
            var result = await ShowDialogAsync<ItunesTimedTextPropertiesWindow, ItunesTimedTextPropertiesViewModel>(vm => { vm.Initialize(GetUpdateSubtitle()); });
            SetLibSeSettings();
        }

        if (format is TimedText10)
        {
            var result = await ShowDialogAsync<TimedText10PropertiesWindow, TimedText10PropertiesViewModel>(vm => { vm.Initialize(GetUpdateSubtitle()); });
            SetLibSeSettings();
        }

        if (format is TimedTextImscRosetta)
        {
            var result = await ShowDialogAsync<RosettaPropertiesWindow, RosettaPropertiesViewModel>(vm => { vm.Initialize(GetUpdateSubtitle()); });
            SetLibSeSettings();
        }

        if (format is TmpegEncXml)
        {
            var result = await ShowDialogAsync<TmpegEncXmlPropertiesWindow, TmpegEncXmlPropertiesViewModel>(vm => { });
            SetLibSeSettings();
        }

        if (format is DCinemaSmpte2007 or DCinemaSmpte2010 or DCinemaSmpte2014)
        {
            Se.Settings.File.DCinemaSmpte.CurrentDCinemaSubtitleId = Configuration.Settings.SubtitleSettings.CurrentDCinemaSubtitleId;
            Se.Settings.File.DCinemaSmpte.CurrentDCinemaMovieTitle = Configuration.Settings.SubtitleSettings.CurrentDCinemaMovieTitle;
            Se.Settings.File.DCinemaSmpte.CurrentDCinemaReelNumber = Configuration.Settings.SubtitleSettings.CurrentDCinemaReelNumber;
            Se.Settings.File.DCinemaSmpte.CurrentDCinemaIssueDate = Configuration.Settings.SubtitleSettings.CurrentDCinemaIssueDate;
            Se.Settings.File.DCinemaSmpte.CurrentDCinemaLanguage = Configuration.Settings.SubtitleSettings.CurrentDCinemaLanguage;
            Se.Settings.File.DCinemaSmpte.CurrentDCinemaEditRate = Configuration.Settings.SubtitleSettings.CurrentDCinemaEditRate;
            Se.Settings.File.DCinemaSmpte.CurrentDCinemaTimeCodeRate = Configuration.Settings.SubtitleSettings.CurrentDCinemaTimeCodeRate;
            Se.Settings.File.DCinemaSmpte.CurrentDCinemaStartTime = Configuration.Settings.SubtitleSettings.CurrentDCinemaStartTime;
            Se.Settings.File.DCinemaSmpte.CurrentDCinemaFontId = Configuration.Settings.SubtitleSettings.CurrentDCinemaFontId;
            Se.Settings.File.DCinemaSmpte.CurrentDCinemaFontUri = Configuration.Settings.SubtitleSettings.CurrentDCinemaFontUri;
            Se.Settings.File.DCinemaSmpte.CurrentDCinemaFontColor = Configuration.Settings.SubtitleSettings.CurrentDCinemaFontColor.ToHex();
            Se.Settings.File.DCinemaSmpte.CurrentDCinemaFontEffect = Configuration.Settings.SubtitleSettings.CurrentDCinemaFontEffect;
            Se.Settings.File.DCinemaSmpte.CurrentDCinemaFontEffectColor = Configuration.Settings.SubtitleSettings.CurrentDCinemaFontEffectColor.ToHex();
            Se.Settings.File.DCinemaSmpte.CurrentDCinemaFontSize = Configuration.Settings.SubtitleSettings.CurrentDCinemaFontSize;

            var result = await ShowDialogAsync<DCinemaSmptePropertiesWindow, DCinemaSmptePropertiesViewModel>(vm => { vm.Initialize(GetUpdateSubtitle(), format); });

            Configuration.Settings.SubtitleSettings.CurrentDCinemaSubtitleId = Se.Settings.File.DCinemaSmpte.CurrentDCinemaSubtitleId;
            Configuration.Settings.SubtitleSettings.CurrentDCinemaMovieTitle = Se.Settings.File.DCinemaSmpte.CurrentDCinemaMovieTitle;
            Configuration.Settings.SubtitleSettings.CurrentDCinemaReelNumber = Se.Settings.File.DCinemaSmpte.CurrentDCinemaReelNumber;
            Configuration.Settings.SubtitleSettings.CurrentDCinemaIssueDate = Se.Settings.File.DCinemaSmpte.CurrentDCinemaIssueDate;
            Configuration.Settings.SubtitleSettings.CurrentDCinemaLanguage = Se.Settings.File.DCinemaSmpte.CurrentDCinemaLanguage;
            Configuration.Settings.SubtitleSettings.CurrentDCinemaEditRate = Se.Settings.File.DCinemaSmpte.CurrentDCinemaEditRate;
            Configuration.Settings.SubtitleSettings.CurrentDCinemaTimeCodeRate = Se.Settings.File.DCinemaSmpte.CurrentDCinemaTimeCodeRate;
            Configuration.Settings.SubtitleSettings.CurrentDCinemaStartTime = Se.Settings.File.DCinemaSmpte.CurrentDCinemaStartTime;
            Configuration.Settings.SubtitleSettings.CurrentDCinemaFontId = Se.Settings.File.DCinemaSmpte.CurrentDCinemaFontId;
            Configuration.Settings.SubtitleSettings.CurrentDCinemaFontUri = Se.Settings.File.DCinemaSmpte.CurrentDCinemaFontUri;
            Configuration.Settings.SubtitleSettings.CurrentDCinemaFontColor = Se.Settings.File.DCinemaSmpte.CurrentDCinemaFontColor.FromHexToColor().ToSkColor();
            Configuration.Settings.SubtitleSettings.CurrentDCinemaFontEffect = Se.Settings.File.DCinemaSmpte.CurrentDCinemaFontEffect;
            Configuration.Settings.SubtitleSettings.CurrentDCinemaFontEffectColor = Se.Settings.File.DCinemaSmpte.CurrentDCinemaFontEffectColor.FromHexToColor().ToSkColor();
            Configuration.Settings.SubtitleSettings.CurrentDCinemaFontSize = Se.Settings.File.DCinemaSmpte.CurrentDCinemaFontSize;

            SetLibSeSettings();
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task OpenContainingFolder()
    {
        if (string.IsNullOrEmpty(_subtitleFileName))
        {
            return;
        }

        await _folderHelper.OpenFolderWithFileSelected(Window!, _subtitleFileName);
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private void ToggleCurrentSubtitleWhilePlaying()
    {
        SelectCurrentSubtitleWhilePlaying = !SelectCurrentSubtitleWhilePlaying;

        if (SelectCurrentSubtitleWhilePlaying)
        {
            ShowStatus(Se.Language.Main.SelectCurrentSubtitleWhilePlayingOn);
        }
        else
        {
            ShowStatus(Se.Language.Main.SelectCurrentSubtitleWhilePlayingOff);
        }
    }

    [RelayCommand]
    private async Task CopySubtitlePathToClipboard()
    {
        if (Window == null || Window.Clipboard == null || string.IsNullOrEmpty(_subtitleFileName))
        {
            return;
        }

        await ClipboardHelper.SetTextAsync(Window, _subtitleFileName);
    }

    [RelayCommand]
    private async Task CopySubtitleOriginalPathToClipboard()
    {
        if (Window == null || Window.Clipboard == null || string.IsNullOrEmpty(_subtitleFileNameOriginal))
        {
            return;
        }

        await ClipboardHelper.SetTextAsync(Window, _subtitleFileNameOriginal);
    }

    [RelayCommand]
    private async Task CopyMsRelativeToCurrentSubtitleLineToClipboard()
    {
        var vp = GetVideoPlayerControl();
        var selectedSubtitle = SelectedSubtitle;
        if (Window == null || Window.Clipboard == null || vp == null || selectedSubtitle == null)
        {
            return;
        }

        var ms = (int)Math.Round(vp.Position * 1000 - selectedSubtitle.StartTime.TotalMilliseconds, MidpointRounding.AwayFromZero);

        await ClipboardHelper.SetTextAsync(Window, ms.ToString(CultureInfo.InvariantCulture));
    }

    [RelayCommand]
    private async Task ShowCompare()
    {
        var result = await ShowDialogAsync<CompareWindow, CompareViewModel>(vm =>
        {
            var right = new ObservableCollection<SubtitleLineViewModel>();
            vm.Initialize(Subtitles, _subtitleFileName ?? string.Empty, right, string.Empty, HasChanges());
        });
    }

    [RelayCommand]
    private async Task ShowStatistics()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        var result = await ShowDialogAsync<StatisticsWindow, StatisticsViewModel>(vm =>
        {
            vm.Initialize(GetUpdateSubtitle(), SelectedSubtitleFormat, _subtitleFileName ?? string.Empty);
        });
    }

    [RelayCommand]
    private async Task ExportBluRaySup()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        IExportHandler exportHandler = new ExportHandlerBluRaySup();
        var result = await ShowDialogAsync<ExportImageBasedWindow, ExportImageBasedViewModel>(vm =>
        {
            vm.Initialize(exportHandler, Subtitles, _subtitleFileName, _videoFileName);
        });

        if (!result.OkPressed)
        {
            return;
        }
    }

    [RelayCommand]
    private async Task ExportBdnXml()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        IExportHandler exportHandler = new ExportHandlerBdnXml();
        var result = await ShowDialogAsync<ExportImageBasedWindow, ExportImageBasedViewModel>(vm =>
        {
            vm.Initialize(exportHandler, Subtitles, _subtitleFileName, _videoFileName);
        });

        if (!result.OkPressed)
        {
            return;
        }
    }

    [RelayCommand]
    private async Task ExportWebVttThumbnails()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        IExportHandler exportHandler = new ExportHandlerWebVttThumbnail();
        var result = await ShowDialogAsync<ExportImageBasedWindow, ExportImageBasedViewModel>(vm =>
        {
            vm.Initialize(exportHandler, Subtitles, _subtitleFileName, _videoFileName);
        });

        if (!result.OkPressed)
        {
            return;
        }
    }

    [RelayCommand]
    private async Task ExportDCinemaInteropPng()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        IExportHandler exportHandler = new ExportHandlerDCinemaInteropPng();
        var result = await ShowDialogAsync<ExportImageBasedWindow, ExportImageBasedViewModel>(vm =>
        {
            vm.Initialize(exportHandler, Subtitles, _subtitleFileName, _videoFileName);
        });

        if (!result.OkPressed)
        {
            return;
        }
    }

    [RelayCommand]
    private async Task ExportDCinemaSmpte2014Png()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        IExportHandler exportHandler = new ExportHandlerDCinemaSmpte2014Png();
        var result = await ShowDialogAsync<ExportImageBasedWindow, ExportImageBasedViewModel>(vm =>
        {
            vm.Initialize(exportHandler, Subtitles, _subtitleFileName, _videoFileName);
        });

        if (!result.OkPressed)
        {
            return;
        }
    }

    [RelayCommand]
    private async Task ExportDostPng()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        IExportHandler exportHandler = new ExportHandlerDost();
        var result = await ShowDialogAsync<ExportImageBasedWindow, ExportImageBasedViewModel>(vm =>
        {
            vm.Initialize(exportHandler, Subtitles, _subtitleFileName, _videoFileName);
        });

        if (!result.OkPressed)
        {
            return;
        }
    }

    [RelayCommand]
    private async Task ExportFcpPng()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        IExportHandler exportHandler = new ExportHandlerFcp();
        var result = await ShowDialogAsync<ExportImageBasedWindow, ExportImageBasedViewModel>(vm =>
        {
            vm.Initialize(exportHandler, Subtitles, _subtitleFileName, _videoFileName);
        });

        if (!result.OkPressed)
        {
            return;
        }
    }

    [RelayCommand]
    private async Task ExportImagesWithTimeCode()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        IExportHandler exportHandler = new ExportHandlerImagesWithTimeCode();
        var result = await ShowDialogAsync<ExportImageBasedWindow, ExportImageBasedViewModel>(vm =>
        {
            vm.Initialize(exportHandler, Subtitles, _subtitleFileName, _videoFileName);
        });

        if (!result.OkPressed)
        {
            return;
        }
    }

    [RelayCommand]
    private async Task ExportVobSub()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        IExportHandler exportHandler = new ExportHandlerVobSub();
        var result = await ShowDialogAsync<ExportImageBasedWindow, ExportImageBasedViewModel>(vm =>
        {
            vm.Initialize(exportHandler, Subtitles, _subtitleFileName, _videoFileName);
        });

        if (!result.OkPressed)
        {
            return;
        }
    }

    [RelayCommand]
    private async Task ShowExportCustomTextFormat()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        var result = await ShowDialogAsync<ExportCustomTextFormatWindow, ExportCustomTextFormatViewModel>(vm =>
        {
            vm.Initialize(Subtitles.ToList(), _subtitleFileName, _videoFileName);
        });
    }

    [RelayCommand]
    private async Task ShowExportPlainText()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        var result = await ShowDialogAsync<ExportPlainTextWindow, ExportPlainTextViewModel>(vm => { vm.Initialize(Subtitles.ToList(), _subtitleFileName, _videoFileName); });
    }

    [RelayCommand]
    private async Task ExportCapMakerPlus()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        var format = new CapMakerPlus();
        using var ms = new MemoryStream();
        format.Save(_subtitleFileName, ms, GetUpdateSubtitle(), false);

        var fileName = await _fileHelper.PickSaveSubtitleFile(
            Window!,
            format,
            GetNewFileName(),
            $"Save {format.Name} file as");

        if (!string.IsNullOrEmpty(fileName))
        {
            File.WriteAllBytes(fileName, ms.ToArray());
            ShowStatus(string.Format(Se.Language.Main.FileExportedInFormatXToY, format.Name, fileName));
        }
    }

    [RelayCommand]
    private async Task ExportCheetahCaption()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        var format = new CheetahCaption();
        using var ms = new MemoryStream();
        format.Save(_subtitleFileName, ms, GetUpdateSubtitle(), false);

        var fileName = await _fileHelper.PickSaveSubtitleFile(
            Window!,
            format,
            GetNewFileName(),
            $"Save {format.Name} file as");

        if (!string.IsNullOrEmpty(fileName))
        {
            File.WriteAllBytes(fileName, ms.ToArray());
            ShowStatus(string.Format(Se.Language.Main.FileExportedInFormatXToY, format.Name, fileName));
        }
    }

    [RelayCommand]
    private async Task ExportCheetahCaptionOld()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        var format = new CheetahCaptionOld();
        using var ms = new MemoryStream();
        format.Save(_subtitleFileName, ms, GetUpdateSubtitle(), false);

        var fileName = await _fileHelper.PickSaveSubtitleFile(
            Window!,
            format,
            GetNewFileName(),
            $"Save {format.Name} file as");

        if (!string.IsNullOrEmpty(fileName))
        {
            File.WriteAllBytes(fileName, ms.ToArray());
            ShowStatus(string.Format(Se.Language.Main.FileExportedInFormatXToY, format.Name, fileName));
        }
    }

    [RelayCommand]
    private async Task ExportCavena890()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        var result = await ShowDialogAsync<ExportCavena890Window, ExportCavena890ViewModel>();
        if (!result.OkPressed)
        {
            return;
        }

        var cavena = new Cavena890();
        var fileName = await _fileHelper.PickSaveSubtitleFile(Window!, cavena, GetNewFileName(),
            string.Format(Se.Language.Main.SaveXFileAs, cavena.Name));
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        using (var ms = new MemoryStream())
        {
            cavena.Save(fileName, ms, GetUpdateSubtitle(), false);
            ms.Position = 0;
            File.WriteAllBytes(fileName, ms.ToArray());
        }

        ShowStatus(string.Format(Se.Language.Main.FileExportedInFormatXToY, cavena.Name, fileName));
    }

    [RelayCommand]
    private async Task ExportPac()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        var result = await ShowDialogAsync<ExportPacWindow, ExportPacViewModel>();
        if (!result.OkPressed || result.PacCodePage == null)
        {
            return;
        }

        var pac = new Pac { CodePage = result.PacCodePage!.Value };

        var fileName = await _fileHelper.PickSaveSubtitleFile(Window!, pac, GetNewFileName(),
            string.Format(Se.Language.Main.SaveXFileAs, pac.Name));
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        using var ms = new MemoryStream();
        pac.Save(fileName, ms, GetUpdateSubtitle(), false);
        ms.Position = 0;
        await File.WriteAllBytesAsync(fileName, ms.ToArray());

        ShowStatus(string.Format(Se.Language.Main.FileExportedInFormatXToY, pac.Name, fileName));
    }

    [RelayCommand]
    private async Task ExportPacUnicode()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        var format = new PacUnicode();
        using var ms = new MemoryStream();
        format.Save(_subtitleFileName, ms, GetUpdateSubtitle());

        var fileName = await _fileHelper.PickSaveSubtitleFile(
            Window!,
            format,
            GetNewFileName(),
            $"Save {format.Name} file as");

        if (!string.IsNullOrEmpty(fileName))
        {
            await File.WriteAllBytesAsync(fileName, ms.ToArray());
            ShowStatus(string.Format(Se.Language.Main.FileExportedInFormatXToY, format.Name, fileName));
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ExportEbuStl()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        var result =
            await ShowDialogAsync<ExportEbuStlWindow, ExportEbuStlViewModel>(vm => { vm.Initialize(GetUpdateSubtitle()); });

        if (!result.OkPressed)
        {
            return;
        }

        Ebu.EbuUiHelper ??= new UiEbuSaveHelper();

        var format = new Ebu();

        var fileName = await _fileHelper.PickSaveSubtitleFile(
            Window!,
            format,
            GetNewFileName(),
            $"Save {format.Name} file as");

        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        format.Save(fileName, result.Subtitle);
        ShowStatus(string.Format(Se.Language.Main.FileExportedInFormatXToFileY, format.Name, fileName));
    }

    [RelayCommand]
    private async Task ImportPlainText()
    {
        if (Window == null)
        {
            return;
        }

        var result = await ShowDialogAsync<ImportPlainTextWindow, ImportPlainTextViewModel>(vm => vm.Initialize(_subtitle, _videoFileName));
        if (result.OkPressed && result.Subtitles.Count > 0)
        {
            _subtitleFileName = string.Empty;
            ResetSubtitle();
            foreach (var item in result.Subtitles)
            {
                Subtitles.Add(item);
            }

            Renumber();
            _updateAudioVisualizer = true;
        }
    }

    [RelayCommand]
    private async Task ImportCsvXlsxCustomColumns()
    {
        if (Window == null)
        {
            return;
        }

        var result = await ShowDialogAsync<ImportCsvXlsxCustomColumnsWindow, ImportCsvXlsxCustomColumnsViewModel>();
        if (result.OkPressed && result.ResultSubtitles.Count > 0)
        {
            _subtitleFileName = string.Empty;
            ResetSubtitle();
            foreach (var item in result.ResultSubtitles)
            {
                Subtitles.Add(item);
            }

            Renumber();
            _updateAudioVisualizer = true;
        }
    }

    [RelayCommand]
    private async Task ImportImages()
    {
        if (Window == null)
        {
            return;
        }

        var result = await ShowDialogAsync<ImportImagesWindow, ImportImagesViewModel>();
        if (!result.OkPressed || result.Images.Count == 0)
        {
            return;
        }

        var ocrResult = await ShowDialogAsync<OcrWindow, OcrViewModel>(vm => { vm.Initialize(result.Images.ToList()); });
        if (ocrResult.OkPressed)
        {
            _subtitleFileName = string.Empty;
            ResetSubtitle();
            Subtitles.AddRange(ocrResult.OcredSubtitle);
            SelectAndScrollToRow(0);
        }
    }

    [RelayCommand]
    private async Task ImportImageSubtitleForOcr()
    {
        if (Window == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickOpenFile(Window!, Se.Language.General.OpenImageBasedSubtitle, Se.Language.General.ImageBasedSubtitles, "*.sup;*.sub;*.ts;*.xml",
            Se.Language.General.AllFiles, "*.*");
        if (string.IsNullOrEmpty(fileName))
        {
            _shortcutManager.ClearKeys();
            return;
        }

        await SubtitleOpen(fileName);
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ImportImageSubtitleForEdit()
    {
        if (Window == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickOpenFile(Window!, Se.Language.General.OpenImageBasedSubtitle, Se.Language.General.ImageBasedSubtitles, "*.sup;*.sub;*.ts;*.xml",
            Se.Language.General.AllFiles, "*.*");
        if (string.IsNullOrEmpty(fileName))
        {
            _shortcutManager.ClearKeys();
            return;
        }

        var result = await ShowDialogAsync<BinaryEditWindow, BinaryEditViewModel>(vm => { vm.Initialize(fileName, null); });
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ImportTimeCodes()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        if (Subtitles.Count == 0)
        {
            await MessageBox.Show(Window, Se.Language.General.Error, Se.Language.General.NoVideoLoaded,
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            _shortcutManager.ClearKeys();
            return;
        }

        var fileName = await _fileHelper.PickOpenSubtitleFile(Window, Se.Language.General.OpenSubtitleFileTitle, false);
        if (string.IsNullOrEmpty(fileName))
        {
            _shortcutManager.ClearKeys();
            return;
        }

        var subtitle = Subtitle.Parse(fileName);
        if (subtitle == null)
        {
            await MessageBox.Show(Window, Se.Language.General.Error, Se.Language.General.UnknownSubtitleFormat,
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            _shortcutManager.ClearKeys();
            return;
        }

        if (subtitle.Paragraphs.Count != Subtitles.Count)
        {
            var message = "The time code import subtitle does not have the same number of lines as the current subtitle." + Environment.NewLine
                + "Imported lines: " + subtitle.Paragraphs.Count + Environment.NewLine
                + "Current lines: " + Subtitles.Count + Environment.NewLine
                + Environment.NewLine +
                "Do you want to continue anyway?";

            var answer = await MessageBox.Show(Window, Se.Language.General.Import, message, MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Error);

            if (answer != MessageBoxResult.Yes)
            {
                _shortcutManager.ClearKeys();
                return;
            }
        }

        for (var i = 0; i < Subtitles.Count && i < subtitle.Paragraphs.Count; i++)
        {
            Subtitles[i].SetStartTimeOnly(subtitle.Paragraphs[i].StartTime.TimeSpan);
            Subtitles[i].EndTime = subtitle.Paragraphs[i].EndTime.TimeSpan;
        }

        _updateAudioVisualizer = true;
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ImportStyling()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        if (Subtitles.Count == 0)
        {
            await MessageBox.Show(Window, Se.Language.General.Error, Se.Language.General.NoVideoLoaded,
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            _shortcutManager.ClearKeys();
            return;
        }

        var fileName = await _fileHelper.PickOpenSubtitleFile(Window, Se.Language.General.OpenSubtitleFileTitle, false);
        if (string.IsNullOrEmpty(fileName))
        {
            _shortcutManager.ClearKeys();
            return;
        }

        var subtitle = Subtitle.Parse(fileName);
        if (subtitle == null)
        {
            await MessageBox.Show(Window, Se.Language.General.Error, Se.Language.General.UnknownSubtitleFormat,
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            _shortcutManager.ClearKeys();
            return;
        }

        if (subtitle.Paragraphs.Count != Subtitles.Count)
        {
            var message = "The style import subtitle does not have the same number of lines as the current subtitle." + Environment.NewLine
                                                                                                                      + "Imported lines: " + subtitle.Paragraphs.Count +
                                                                                                                      Environment.NewLine
                                                                                                                      + "Current lines: " + Subtitles.Count + Environment.NewLine
                                                                                                                      + Environment.NewLine +
                                                                                                                      "Do you want to continue anyway?";

            var answer = await MessageBox.Show(Window, Se.Language.General.Import, message, MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Error);

            if (answer != MessageBoxResult.Yes)
            {
                _shortcutManager.ClearKeys();
                return;
            }
        }

        for (var i = 0; i < Subtitles.Count && i < subtitle.Paragraphs.Count; i++)
        {
            var importText = subtitle.Paragraphs[i].Text;
            var s = Subtitles[i].Text;

            var pre = string.Empty;
            if (importText.StartsWith("{\\") && importText.Contains('}'))
            {
                pre = importText.Substring(0, importText.IndexOf('}') + 1);
                importText = importText.Remove(0, pre.Length);
            }

            for (var j = 0; j < 3; j++)
            {
                if (importText.StartsWith("<i>", StringComparison.OrdinalIgnoreCase) && importText.EndsWith("</i>", StringComparison.OrdinalIgnoreCase))
                {
                    importText = importText.Substring(0, importText.Length - 4);
                    s = "<i>" + s + "</i>";
                }

                if (importText.StartsWith("<b>", StringComparison.OrdinalIgnoreCase) && importText.EndsWith("</b>", StringComparison.OrdinalIgnoreCase))
                {
                    importText = importText.Substring(0, importText.Length - 4);
                    s = "<b>" + s + "</b>";
                }

                if (importText.StartsWith("<u>", StringComparison.OrdinalIgnoreCase) && importText.EndsWith("</u>", StringComparison.OrdinalIgnoreCase))
                {
                    importText = importText.Substring(0, importText.Length - 4);
                    s = "<u>" + s + "</u>";
                }

                var startFont = importText.IndexOf("<font", StringComparison.OrdinalIgnoreCase);
                if (startFont >= 0)
                {
                    var startFontEnd = importText.IndexOf('>', startFont);
                    var endFont = importText.IndexOf("</font>", StringComparison.OrdinalIgnoreCase);
                    if (startFontEnd > startFont && startFont < endFont)
                    {
                        var fontTag = importText.Substring(startFont, startFontEnd - startFont + 1);
                        s = fontTag + s + "</font>";
                    }
                }
            }

            s = pre + s;

            if (!string.IsNullOrWhiteSpace(s))
            {
                Subtitles[i].Text = s;
            }
        }

        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private async Task InsertSubtitleFileAtVideoPosition()
    {
        var vp = GetVideoPlayerControl();
        if (Window == null || vp == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickOpenSubtitleFile(Window, Se.Language.General.OpenSubtitleFileTitle, false);
        if (string.IsNullOrEmpty(fileName))
        {
            _shortcutManager.ClearKeys();
            return;
        }

        var subtitle = Subtitle.Parse(fileName);
        if (subtitle == null || subtitle.Paragraphs.Count <= 0)
        {
            await MessageBox.Show(Window, Se.Language.General.Error, Se.Language.General.UnknownSubtitleFormat,
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            _shortcutManager.ClearKeys();
            return;
        }

        var videoPosition = vp.Position;
        var firstStartTime = subtitle.Paragraphs[0].StartTime.TotalMilliseconds;
        foreach (var p in subtitle.Paragraphs)
        {
            var newParagraph = new SubtitleLineViewModel(p, SelectedSubtitleFormat);
            var offset = p.StartTime.TotalMilliseconds - firstStartTime;
            newParagraph.StartTime = TimeSpan.FromMilliseconds(videoPosition * 1000 + offset);

            _insertService.InsertInCorrectPosition(Subtitles, newParagraph);
        }

        Renumber();
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private async Task ShowImportSubtitleWithManuallyChosenEncoding()
    {
        var doContinue = await HasChangesContinue();
        if (!doContinue)
        {
            return;
        }

        var fileName =
            await _fileHelper.PickOpenSubtitleFile(Window!, Se.Language.General.OpenSubtitleFileTitle, false);
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var result =
            await ShowDialogAsync<ManualChosenEncodingWindow, ManualChosenEncodingViewModel>(vm => { vm.Initialize(fileName); });

        if (result.OkPressed && result.SelectedEncoding != null)
        {
            await SubtitleOpen(fileName, null, null, result.SelectedEncoding);
        }
    }

    [RelayCommand]
    private async Task ShowWaveformSeekSilence()
    {
        var vp = GetVideoPlayerControl();
        if (Window == null || AudioVisualizer?.WavePeaks == null || vp == null)
        {
            return;
        }

        var result =
            await ShowDialogAsync<WaveformSeekSilenceWindow, WaveformSeekSilenceViewModel>(vm => { vm.Initialize(AudioVisualizer.WavePeaks); });

        if (!result.OkPressed || !result.SilenceMaxVolume.HasValue || !result.SilenceMinDuration.HasValue)
        {
            return;
        }

        var seconds = vp.Position;
        if (result.SeekForward)
        {
            seconds = AudioVisualizer.FindDataBelowThreshold(result.SilenceMaxVolume.Value, result.SilenceMinDuration.Value);
        }
        else
        {
            seconds = AudioVisualizer.FindDataBelowThresholdBack(result.SilenceMaxVolume.Value, result.SilenceMinDuration.Value);
        }

        if (seconds >= 0)
        {
            vp.Position = seconds;
        }
    }

    [RelayCommand]
    private void SeekSilenceBack()
    {
        var vp = GetVideoPlayerControl();
        if (AudioVisualizer?.WavePeaks == null || vp == null)
        {
            return;
        }

        var s = Se.Settings.Waveform;
        var seconds = AudioVisualizer.FindDataBelowThresholdBack(s.SeekSilenceMaxVolume, s.SeekSilenceMinDurationSeconds);
        if (seconds >= 0)
        {
            vp.Position = seconds;
        }
    }

    [RelayCommand]
    private void SeekSilenceForward()
    {
        var vp = GetVideoPlayerControl();
        if (AudioVisualizer?.WavePeaks == null || vp == null)
        {
            return;
        }

        var s = Se.Settings.Waveform;
        var seconds = AudioVisualizer.FindDataBelowThreshold(s.SeekSilenceMaxVolume, s.SeekSilenceMinDurationSeconds);
        if (seconds >= 0)
        {
            vp.Position = seconds;
        }
    }

    [RelayCommand]
    private async Task WaveformExtractAudio()
    {
        if (Window == null || string.IsNullOrEmpty(_videoFileName))
        {
            return;
        }

        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
        if (selectedItems.Count != 1)
        {
            return;
        }

        var ffmpegOk = await RequireFfmpegOk();
        if (!ffmpegOk)
        {
            return;
        }

        var line = selectedItems[0];
        var suggestedName = $"{Path.GetFileNameWithoutExtension(_videoFileName)}_{line.Number}.wav";
        var outputFileName = await _fileHelper.PickSaveFile(Window, ".wav", suggestedName, Se.Language.Waveform.ExtractAudioDotDotDot);
        if (string.IsNullOrEmpty(outputFileName))
        {
            return;
        }

        try
        {
            var arguments = FfmpegGenerator.ExtractAudioClipFromVideoParameters(
                _videoFileName,
                line.StartTime.TotalSeconds,
                line.Duration.TotalSeconds,
                false,
                outputFileName,
                _audioTrack?.FfIndex ?? -1);

            using var process = FfmpegGenerator.GetProcess(arguments, (_, _) => { });
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await Task.Run(() => process.WaitForExit());

            if (process.ExitCode != 0 || !File.Exists(outputFileName))
            {
                await MessageBox.Show(Window, Se.Language.General.Error, "Could not extract audio clip from video.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            await ShowDialogAsync<PromptFileSavedWindow, PromptFileSavedViewModel>(vm =>
            {
                vm.Initialize(
                    Se.Language.General.FileSaved,
                    string.Format(Se.Language.General.FileSavedToX, outputFileName),
                    outputFileName,
                    true,
                    true);
            });
        }
        catch (Exception exception)
        {
            await MessageBox.Show(Window, Se.Language.General.Error, exception.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    [RelayCommand]
    private async Task WaveformToggleWaveformSpectrogramHeight()
    {
        if (Window == null || AudioVisualizer?.WavePeaks == null)
        {
            return;
        }

        Se.Settings.Waveform.SpectrogramCombinedWaveformHeight += 10;
        if (Se.Settings.Waveform.SpectrogramCombinedWaveformHeight > 90)
        {
            Se.Settings.Waveform.SpectrogramCombinedWaveformHeight = 10;
        }

        AudioVisualizer.WaveformHeightPercentage = Se.Settings.Waveform.SpectrogramCombinedWaveformHeight;
        AudioVisualizer.ResetCache();
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private async Task SpectrogramToggleStyle()
    {
        if (Window == null || AudioVisualizer == null || AudioVisualizer?.WavePeaks == null || string.IsNullOrEmpty(_videoFileName))
        {
            return;
        }

        var currentStyle = Se.Settings.Waveform.SpectrogramStyle;

        // Cycle through available spectrogram styles
        if (currentStyle == nameof(SeSpectrogramStyle.Classic))
        {
            Se.Settings.Waveform.SpectrogramStyle = nameof(SeSpectrogramStyle.ClassicViridis);
        }
        else if (currentStyle == nameof(SeSpectrogramStyle.ClassicViridis))
        {
            Se.Settings.Waveform.SpectrogramStyle = nameof(SeSpectrogramStyle.ClassicPlasma);
        }
        else if (currentStyle == nameof(SeSpectrogramStyle.ClassicPlasma))
        {
            Se.Settings.Waveform.SpectrogramStyle = nameof(SeSpectrogramStyle.ClassicInferno);
        }
        else if (currentStyle == nameof(SeSpectrogramStyle.ClassicInferno))
        {
            Se.Settings.Waveform.SpectrogramStyle = nameof(SeSpectrogramStyle.ClassicTurbo);
        }
        else if (currentStyle == nameof(SeSpectrogramStyle.ClassicTurbo))
        {
            Se.Settings.Waveform.SpectrogramStyle = nameof(SeSpectrogramStyle.Neon);
        }
        else // Neon or any other value, cycle back to Classic
        {
            Se.Settings.Waveform.SpectrogramStyle = nameof(SeSpectrogramStyle.Classic);
        }

        var spectrogramFileName = WavePeakGenerator2.SpectrogramDrawer.GetSpectrogramFileName(_videoFileName, _audioTrack?.FfIndex ?? -1);
        if (File.Exists(spectrogramFileName))
        {
            var spectrogram = SpectrogramData2.FromDisk(spectrogramFileName);
            if (spectrogram != null)
            {
                spectrogram.Load();
                AudioVisualizer.SetSpectrogram(spectrogram);
            }

            AudioVisualizer.ResetCache();
            _updateAudioVisualizer = true;
        }

        ShowStatus(AddSpaceBeforeUppercaseLetter(Se.Settings.Waveform.SpectrogramStyle));
    }

    private string AddSpaceBeforeUppercaseLetter(string spectrogramStyle)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < spectrogramStyle.Length; i++)
        {
            var c = spectrogramStyle[i];
            if (i > 0 && char.IsUpper(c) && !char.IsUpper(spectrogramStyle[i - 1]))
            {
                sb.Append(' ');
            }

            sb.Append(c);
        }

        return sb.ToString().Trim();
    }

    [RelayCommand]
    private async Task ShowWaveformGuessTimeCodes()
    {
        var vp = GetVideoPlayerControl();
        if (Window == null || AudioVisualizer?.WavePeaks == null || vp == null)
        {
            return;
        }

        var result = await ShowDialogAsync<WaveformGuessTimeCodesWindow, WaveformGuessTimeCodesViewModel>();

        if (!result.OkPressed ||
            !result.ScanBlockSize.HasValue ||
            !result.ScanBlockAverageMin.HasValue ||
            !result.ScanBlockAverageMax.HasValue ||
            !result.SplitLongSubtitlesAtMs.HasValue)
        {
            _shortcutManager.ClearKeys();
            return;
        }

        double startFromSeconds = 0;
        if (result.StartFromVideoPosition)
        {
            startFromSeconds = vp.Position;
        }

        if (result.DeleteLinesAll)
        {
            _subtitle.Paragraphs.Clear();
        }
        else if (result.DeleteLinesFromVideoPosition)
        {
            var subsToKeep = Subtitles.Where(p => p.StartTime.TotalSeconds >= vp.Position).ToList();
            _subtitle.Paragraphs.Clear();
            _subtitle.Paragraphs.AddRange(subsToKeep.Select(p => p.ToParagraph()));
        }

        AudioVisualizer.GenerateTimeCodes(_subtitle, startFromSeconds, result.ScanBlockSize.Value, result.ScanBlockAverageMin.Value, result.ScanBlockAverageMax.Value,
            result.SplitLongSubtitlesAtMs.Value);
        SetSubtitles(_subtitle);
    }

    [RelayCommand]
    private async Task RecalculateDurationSelectedLines()
    {
        var selectedLines = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
        if (selectedLines.Count == 0)
        {
            return;
        }

        foreach (var selectedLine in selectedLines)
        {
            var idx = Subtitles.IndexOf(selectedLine);
            if (idx < 0)
            {
                continue;
            }

            var nextSubtitle = Subtitles.GetOrNull(idx + 1);
            var charCount = selectedLine.Text?.Length ?? 0;

            var optimalDuration = TimeSpan.FromSeconds(charCount / Se.Settings.General.SubtitleOptimalCharactersPerSeconds);
            var maxDuration = TimeSpan.FromSeconds(charCount / Se.Settings.General.SubtitleMaximumCharactersPerSeconds);
            var maxEndTime = TimeSpan.FromMilliseconds(selectedLine.StartTime.TotalMilliseconds + Se.Settings.General.SubtitleMaximumDisplayMilliseconds);
            if (nextSubtitle != null)
            {
                var tempMaxEntTime = TimeSpan.FromMilliseconds(nextSubtitle.StartTime.TotalMilliseconds - Se.Settings.General.MinimumBetweenLines.GetMilliseconds());
                if (tempMaxEntTime < maxEndTime)
                {
                    maxEndTime = tempMaxEntTime;
                }
            }

            var proposedEndTime = selectedLine.StartTime + optimalDuration;
            if (proposedEndTime.TotalMilliseconds - selectedLine.StartTime.TotalMilliseconds < Se.Settings.General.SubtitleMinimumDisplayMilliseconds)
            {
                proposedEndTime = TimeSpan.FromMilliseconds(selectedLine.StartTime.TotalMilliseconds + Se.Settings.General.SubtitleMinimumDisplayMilliseconds);
            }

            var fallbackEndTime = selectedLine.StartTime + maxDuration;

            if (proposedEndTime <= maxEndTime)
            {
                selectedLine.EndTime = proposedEndTime;
            }
            else if (fallbackEndTime <= maxEndTime)
            {
                selectedLine.EndTime = fallbackEndTime;
            }
        }

        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void SetDurationMaxCpsSelectedLines()
    {
        var selectedLines = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
        if (selectedLines.Count == 0)
        {
            return;
        }

        foreach (var selectedLine in selectedLines)
        {
            var idx = Subtitles.IndexOf(selectedLine);
            if (idx < 0)
            {
                continue;
            }

            var charCount = selectedLine.Text?.Length ?? 0;
            if (charCount == 0)
            {
                continue;
            }

            var maxCps = Se.Settings.General.SubtitleMaximumCharactersPerSeconds;
            if (maxCps <= 0)
            {
                continue;
            }

            var minEndTime = TimeSpan.FromMilliseconds(selectedLine.StartTime.TotalMilliseconds + Se.Settings.General.SubtitleMinimumDisplayMilliseconds);
            var maxEndTime = TimeSpan.FromMilliseconds(selectedLine.StartTime.TotalMilliseconds + Se.Settings.General.SubtitleMaximumDisplayMilliseconds);
            var nextSubtitle = Subtitles.GetOrNull(idx + 1);
            if (nextSubtitle != null)
            {
                var gapBound = TimeSpan.FromMilliseconds(nextSubtitle.StartTime.TotalMilliseconds - Se.Settings.General.MinimumBetweenLines.GetMilliseconds());
                if (gapBound < maxEndTime)
                {
                    maxEndTime = gapBound;
                }
            }

            if (maxEndTime < minEndTime)
            {
                continue;
            }

            var newEndTime = selectedLine.StartTime + TimeSpan.FromSeconds(charCount / maxCps);
            if (newEndTime < minEndTime)
            {
                newEndTime = minEndTime;
            }
            if (newEndTime > maxEndTime)
            {
                newEndTime = maxEndTime;
            }

            selectedLine.EndTime = newEndTime;
        }

        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void DoWaveformCenter()
    {
        var vp = GetVideoPlayerControl();
        if (Window == null || AudioVisualizer?.WavePeaks == null || vp == null)
        {
            return;
        }

        AudioVisualizer.CenterOnPosition(vp.Position);
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void WaveformShowOnlySpectrogram()
    {
        if (Window == null || AudioVisualizer?.WavePeaks == null)
        {
            return;
        }

        ShowWaveformOnlyWaveform = true;
        ShowWaveformOnlySpectrogram = false;
        ShowWaveformWaveformAndSpectrogram = true;
        ShowWaveformDisplayModeSeparator = true;

        AudioVisualizer.SetDisplayMode(WaveformDisplayMode.OnlySpectrogram);
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void WaveformShowOnlyWaveform()
    {
        if (Window == null || AudioVisualizer?.WavePeaks == null)
        {
            return;
        }

        ShowWaveformOnlyWaveform = false;
        ShowWaveformOnlySpectrogram = true;
        ShowWaveformWaveformAndSpectrogram = true;
        ShowWaveformDisplayModeSeparator = true;

        AudioVisualizer.SetDisplayMode(WaveformDisplayMode.OnlyWaveform);
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void WaveformShowWaveformAndSpectrogram()
    {
        if (Window == null || AudioVisualizer?.WavePeaks == null)
        {
            return;
        }

        ShowWaveformOnlyWaveform = true;
        ShowWaveformOnlySpectrogram = true;
        ShowWaveformWaveformAndSpectrogram = false;
        ShowWaveformDisplayModeSeparator = true;

        AudioVisualizer.SetDisplayMode(WaveformDisplayMode.WaveformAndSpectrogram);
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private async Task ShowPickLayer()
    {
        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
        if (Window == null || AudioVisualizer?.WavePeaks == null || selectedItems.Count == 0)
        {
            return;
        }

        var result =
            await ShowDialogAsync<PickLayerWindow, PickLayerViewModel>(vm => { vm.Initialize(selectedItems); });

        if (!result.OkPressed)
        {
            return;
        }

        foreach (var item in selectedItems)
        {
            item.Layer = result.Layer;
        }

        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private async Task ShowPickLayerFilter()
    {
        if (Window == null)
        {
            return;
        }

        var result =
            await ShowDialogAsync<PickLayerFilterWindow, PickLayerFilterViewModel>(vm => { vm.Initialize(Subtitles.ToList(), _visibleLayers?.ToList()); });

        if (!result.OkPressed)
        {
            return;
        }

        _visibleLayers = result.SelectedLayers != null ? new HashSet<int>(result.SelectedLayers) : null;
        ShowLayerFilterIcon = IsFormatAssa && Se.Settings.Appearance.ShowLayer && _visibleLayers != null;
        _updateAudioVisualizer = true;
    }

    private string GetNewFileName()
    {
        if (!string.IsNullOrEmpty(_subtitleFileName))
        {
            return Path.GetFileNameWithoutExtension(_subtitleFileName);
        }

        if (!string.IsNullOrEmpty(_videoFileName))
        {
            return Path.GetFileNameWithoutExtension(_videoFileName);
        }

        return string.Empty;
    }

    [RelayCommand]
    private void ColumnDeleteText()
    {
        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
        if (Window == null || selectedItems.Count == 0)
        {
            return;
        }

        foreach (var item in selectedItems)
        {
            item.Text = string.Empty;
        }

        _updateAudioVisualizer = true;
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private void ColumnDeleteTextAndShiftCellsUp()
    {
        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
        if (Window == null || selectedItems.Count == 0)
        {
            return;
        }

        // Build selection mask
        var total = Subtitles.Count;
        var isSelected = new bool[total];
        for (int i = 0; i < total; i++)
        {
            isSelected[i] = false;
        }

        foreach (var item in selectedItems)
        {
            var idx = Subtitles.IndexOf(item);
            if (idx >= 0 && idx < total)
            {
                isSelected[idx] = true;
            }
        }

        // Remove selected cells (texts) and shift remaining up
        var newTexts = new List<string>(total);
        for (int i = 0; i < total; i++)
        {
            if (!isSelected[i])
            {
                newTexts.Add(Subtitles[i].Text);
            }
        }

        // Pad with empty strings at the end to keep row count
        while (newTexts.Count < total)
        {
            newTexts.Add(string.Empty);
        }

        for (int i = 0; i < total; i++)
        {
            Subtitles[i].Text = newTexts[i];
        }

        _shortcutManager.ClearKeys();
    }


    [RelayCommand]
    private void ColumnInsertEmptyTextAndShiftCellsDown()
    {
        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
        if (Window == null || selectedItems.Count == 0)
        {
            return;
        }

        var total = Subtitles.Count;
        var texts = Subtitles.Select(p => p.Text).ToList();

        // Insert empty cells at the original selection indices and shift down
        // Use delta to keep correct original target positions
        var indices = selectedItems
            .Select(x => Subtitles.IndexOf(x))
            .Where(i => i >= 0 && i < total)
            .Distinct()
            .OrderBy(i => i)
            .ToList();

        int delta = 0;
        foreach (var idx in indices)
        {
            var insertAt = idx + delta;
            if (insertAt < 0) insertAt = 0;
            if (insertAt > texts.Count) insertAt = texts.Count;
            texts.Insert(insertAt, string.Empty);
            delta++;
        }

        // Trim overflow to match row count
        if (texts.Count > total)
        {
            texts.RemoveRange(total, texts.Count - total);
        }
        else
        {
            while (texts.Count < total)
            {
                texts.Add(string.Empty);
            }
        }

        for (int i = 0; i < total; i++)
        {
            Subtitles[i].Text = texts[i];
        }

        _shortcutManager.ClearKeys();
    }


    [RelayCommand]
    private async Task ColumnInsertTextFromSubtitle()
    {
        if (Window == null || SelectedSubtitle == null)
        {
            return;
        }

        var idx = Subtitles.IndexOf(SelectedSubtitle);
        if (idx < 0)
        {
            return;
        }

        var fileName =
            await _fileHelper.PickOpenSubtitleFile(Window!, Se.Language.General.OpenSubtitleFileTitle, false);
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var subtitle = Subtitle.Parse(fileName);
        if (subtitle == null)
        {
            await MessageBox.Show(Window, Se.Language.General.Error, Se.Language.General.UnknownSubtitleFormat,
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            _shortcutManager.ClearKeys();
            return;
        }

        var count = 0;
        for (var i = 0; i < subtitle.Paragraphs.Count; i++)
        {
            if (idx + i >= Subtitles.Count)
            {
                break;
            }

            Subtitles[idx + i].Text = subtitle.Paragraphs[i].Text;
            count++;
        }

        ShowStatus(string.Format(Se.Language.Main.InsertedXTextsFromSubtitleY, count, Path.GetFileName(fileName)));

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ColumnCopyTextFromOriginalToCurrent()
    {
        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
        if (!ShowColumnOriginalText || selectedItems.Count == 0)
        {
            return;
        }

        _undoRedoManager.StopChangeDetection();
        foreach (var selectedItem in selectedItems)
        {
            selectedItem.Text = selectedItem.OriginalText;
        }

        _undoRedoManager.StartChangeDetection();

        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private async Task ColumnPasteFromClipboard()
    {
        if (Window == null || SelectedSubtitle == null)
        {
            return;
        }

        var idx = Subtitles.IndexOf(SelectedSubtitle);
        if (idx < 0)
        {
            return;
        }

        var text = await ClipboardHelper.GetTextAsync(Window);
        if (string.IsNullOrEmpty(text))
        {
            ShowStatus(Se.Language.Main.NoTextInClipboard);
            return;
        }

        var lines = text.SplitToLines();
        SubtitleFormat? detectedFormat = null;
        foreach (var format in SubtitleFormats)
        {
            if (format.IsMine(lines, string.Empty))
            {
                detectedFormat = format;
                break;
            }
        }

        if (detectedFormat == null)
        {
            await MessageBox.Show(Window, Se.Language.General.Error, Se.Language.General.UnknownSubtitleFormat,
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            _shortcutManager.ClearKeys();
            return;
        }

        var subtitle = new Subtitle();
        detectedFormat.LoadSubtitle(subtitle, lines, null);

        var result = await ShowDialogAsync<ColumnPasteWindow, ColumnPasteViewModel>();
        if (!result.OkPressed)
        {
            return;
        }

        var count = 0;
        var overWrite = result.ModeOverwrite;
        for (var i = 0; i < subtitle.Paragraphs.Count && idx < Subtitles.Count; i++)
        {
            if (!overWrite)
            {
                for (int j = Subtitles.Count - 1; j > idx; j--)
                {
                    if (result.ColumnsAll)
                    {
                        Subtitles[j].SetStartTimeOnly(Subtitles[j - 1].StartTime);
                        Subtitles[j].EndTime = Subtitles[j - 1].EndTime;
                        Subtitles[j].Text = Subtitles[j - 1].Text;
                    }
                    else if (result.ColumnsTextOnly)
                    {
                        Subtitles[j].Text = Subtitles[j - 1].Text;
                    }
                    else if (result.ColumnsTimeCodesOnly)
                    {
                        Subtitles[j].SetStartTimeOnly(Subtitles[j - 1].StartTime);
                        Subtitles[j].EndTime = Subtitles[j - 1].EndTime;
                    }
                }
            }

            if (result.ColumnsAll)
            {
                Subtitles[idx].SetStartTimeOnly(subtitle.Paragraphs[i].StartTime.TimeSpan);
                Subtitles[idx].EndTime = subtitle.Paragraphs[i].EndTime.TimeSpan;
                Subtitles[idx].Text = subtitle.Paragraphs[i].Text;
            }
            else if (result.ColumnsTextOnly)
            {
                Subtitles[idx].Text = subtitle.Paragraphs[i].Text;
            }
            else if (result.ColumnsTimeCodesOnly)
            {
                Subtitles[idx].SetStartTimeOnly(subtitle.Paragraphs[i].StartTime.TimeSpan);
                Subtitles[idx].EndTime = subtitle.Paragraphs[i].EndTime.TimeSpan;
            }

            count++;
            idx++;
        }
    }

    [RelayCommand]
    private void ColumnTextUp()
    {
        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
        if (Window == null || selectedItems.Count == 0)
        {
            return;
        }

        var total = Subtitles.Count;
        var sel = selectedItems
            .Select(x => Subtitles.IndexOf(x))
            .Where(i => i >= 0 && i < total)
            .Distinct()
            .OrderBy(i => i)
            .ToList();

        if (sel.Count == 0)
        {
            _shortcutManager.ClearKeys();
            return;
        }

        // Build contiguous blocks of selected indices
        List<(int start, int end)> blocks = new();
        int pos = 0;
        while (pos < sel.Count)
        {
            int start = sel[pos];
            int end = start;
            while (pos + 1 < sel.Count && sel[pos + 1] == end + 1)
            {
                pos++;
                end = sel[pos];
            }

            blocks.Add((start, end));
            pos++;
        }

        // Move each block up by one by rotating:
        // [above][selected block] -> [selected block shifts up][above moves to last selected]
        foreach (var (start, end) in blocks)
        {
            if (start <= 0)
            {
                // cannot move topmost line up
                continue;
            }

            var temp = Subtitles[start - 1].Text;
            // shift up
            for (int i = start - 1; i < end; i++)
            {
                Subtitles[i].Text = Subtitles[i + 1].Text;
            }

            Subtitles[end].Text = temp;
        }

        _shortcutManager.ClearKeys();
    }


    [RelayCommand]
    private void ColumnTextDown()
    {
        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
        if (Window == null || selectedItems.Count == 0)
        {
            return;
        }

        var total = Subtitles.Count;
        var sel = selectedItems
            .Select(x => Subtitles.IndexOf(x))
            .Where(i => i >= 0 && i < total)
            .Distinct()
            .OrderBy(i => i)
            .ToList();

        if (sel.Count == 0)
        {
            _shortcutManager.ClearKeys();
            return;
        }

        // Build contiguous blocks of selected indices
        List<(int start, int end)> blocks = new();
        int pos = 0;
        while (pos < sel.Count)
        {
            int start = sel[pos];
            int end = start;
            while (pos + 1 < sel.Count && sel[pos + 1] == end + 1)
            {
                pos++;
                end = sel[pos];
            }

            blocks.Add((start, end));
            pos++;
        }

        // Process from bottommost block to top to avoid interference
        for (int b = blocks.Count - 1; b >= 0; b--)
        {
            var (start, end) = blocks[b];
            if (end >= total - 1)
            {
                // cannot move bottommost line down
                continue;
            }

            var temp = Subtitles[end + 1].Text;
            // shift down
            for (int i = end + 1; i > start; i--)
            {
                Subtitles[i].Text = Subtitles[i - 1].Text;
            }

            Subtitles[start].Text = temp;
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task VideoGenerateBlank()
    {
        var ffmpegOk = await RequireFfmpegOk();
        if (!ffmpegOk)
        {
            return;
        }

        var result = await ShowDialogAsync<BlankVideoWindow, BlankVideoViewModel>(vm => { vm.Initialize(_subtitleFileName ?? string.Empty, SelectedSubtitleFormat); });

        if (!result.OkPressed)
        {
            return;
        }

        if (string.IsNullOrEmpty(_videoFileName) && File.Exists(result.VideoFileName))
        {
            await VideoOpenFile(result.VideoFileName);
        }
    }

    [RelayCommand]
    private async Task VideoCut()
    {
        var ffmpegOk = await RequireFfmpegOk();
        if (!ffmpegOk)
        {
            return;
        }

        if (string.IsNullOrEmpty(_videoFileName))
        {
            await CommandVideoOpen();
        }

        if (string.IsNullOrEmpty(_videoFileName))
        {
            return;
        }

        var result = await ShowDialogAsync<CutVideoWindow, CutVideoViewModel>(vm =>
        {
            vm.Initialize(_videoFileName, AudioVisualizer?.WavePeaks, GetUpdateSubtitle(), SelectedSubtitleFormat, this);
        });

        if (!result.OkPressed)
        {
            return;
        }
    }

    [RelayCommand]
    private async Task CutVideoSelectedLines()
    {
        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
        if (Window == null || selectedItems.Count == 0)
        {
            return;
        }

        var ffmpegOk = await RequireFfmpegOk();
        if (!ffmpegOk)
        {
            return;
        }

        if (string.IsNullOrEmpty(_videoFileName))
        {
            await CommandVideoOpen();
        }

        if (string.IsNullOrEmpty(_videoFileName))
        {
            return;
        }

        var result = await ShowDialogAsync<CutVideoWindow, CutVideoViewModel>(vm =>
        {
            vm.Initialize(_videoFileName, AudioVisualizer?.WavePeaks, GetUpdateSubtitle(), SelectedSubtitleFormat, this, selectedItems);
        });

        if (!result.OkPressed)
        {
            return;
        }
    }

    [RelayCommand]
    private async Task VideoEmbed()
    {
        var ffmpegOk = await RequireFfmpegOk();
        if (!ffmpegOk)
        {
            return;
        }

        if (IsMp4LikeFile(_videoFileName))
        {
            var mp4Result = await ShowDialogAsync<EmbeddedSubtitlesEditMp4Window, EmbeddedSubtitlesEditMp4ViewModel>(vm =>
            {
                vm.Initialize(_videoFileName ?? string.Empty, GetUpdateSubtitle(), SelectedSubtitleFormat, _mediaInfo);
            });

            if (!mp4Result.OkPressed)
            {
                return;
            }

            return;
        }

        var result = await ShowDialogAsync<EmbeddedSubtitlesEditWindow, EmbeddedSubtitlesEditViewModel>(vm =>
        {
            vm.Initialize(_videoFileName ?? string.Empty, GetUpdateSubtitle(), SelectedSubtitleFormat, _mediaInfo);
        });

        if (!result.OkPressed)
        {
            return;
        }
    }

    private static bool IsMp4LikeFile(string? fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return false;
        }

        var ext = Path.GetExtension(fileName);
        return ext.Equals(".mp4", StringComparison.OrdinalIgnoreCase)
            || ext.Equals(".m4v", StringComparison.OrdinalIgnoreCase)
            || ext.Equals(".mov", StringComparison.OrdinalIgnoreCase);
    }

    [RelayCommand]
    private async Task VideoReEncode()
    {
        var ffmpegOk = await RequireFfmpegOk();
        if (!ffmpegOk)
        {
            return;
        }

        if (string.IsNullOrEmpty(_videoFileName))
        {
            await CommandVideoOpen();
        }

        if (string.IsNullOrEmpty(_videoFileName))
        {
            return;
        }

        var result = await ShowDialogAsync<ReEncodeVideoWindow, ReEncodeVideoViewModel>(vm => { vm.Initialize(_videoFileName, SelectedSubtitleFormat); });

        if (!result.OkPressed)
        {
            return;
        }
    }

    [RelayCommand]
    private void Undo()
    {
        PerformUndo();
    }

    [RelayCommand]
    private void Redo()
    {
        PerformRedo();
    }

    [RelayCommand]
    private async Task ShowToolsAdjustDurations()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        var result =
            await ShowDialogAsync<AdjustDurationWindow, AdjustDurationViewModel>();

        if (result.OkPressed)
        {
            RunWithoutChangeDetection(() =>
            {
                result.AdjustDuration(Subtitles);
            });
            _updateAudioVisualizer = true;
        }
    }

    public void RunWithoutChangeDetection(Action action)
    {
        _undoRedoManager.StopChangeDetection();
        try
        {
            action();
        }
        finally
        {
            _undoRedoManager.StartChangeDetection();
        }
    }

    [RelayCommand]
    private async Task ShowApplyDurationLimits()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        var result = await ShowDialogAsync<ApplyDurationLimitsWindow, ApplyDurationLimitsViewModel>(vm =>
        {
            var shotChanges = AudioVisualizer?.ShotChanges ?? new List<double>();
            vm.Initialize(Subtitles.ToList(), shotChanges);
        });

        if (result.OkPressed && result.AllSubtitlesFixed.Count > 0)
        {
            RunWithoutChangeDetection(() =>
            {
                var idx = SelectedSubtitleIndex;
                Subtitles.Clear();
                Subtitles.AddRange(result.AllSubtitlesFixed);
                SelectAndScrollToRow(idx ?? 0);
                _updateAudioVisualizer = true;
            });
        }
    }

    [RelayCommand]
    private async Task ShowToolsBatchConvert()
    {
        await ShowDialogAsync<BatchConvertWindow, BatchConvertViewModel>();
    }

    [RelayCommand]
    private async Task ShowToolsJoin()
    {
        var result =
            await ShowDialogAsync<JoinSubtitlesWindow, JoinSubtitlesViewModel>();
        if (!result.OkPressed)
        {
            return;
        }

        ResetSubtitle();
        SetSubtitles(result.JoinedSubtitle);
        SetSubtitleFormat(SubtitleFormats.FirstOrDefault(p => p.Name == result.JoinedFormat.Name) ??
                          SubtitleFormats[0]);
        SelectAndScrollToRow(0);
        ShowStatus(Se.Language.Main.JoinedSubtitleLoaded);
    }

    [RelayCommand]
    private async Task ShowToolsMergeTwoSubtitles()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        var lines = Subtitles.ToList();
        var hasOriginal = ShowColumnOriginalText && lines.Any(p => !string.IsNullOrEmpty(p.OriginalText));
        var result = await ShowDialogAsync<MergeTwoSubtitlesWindow, MergeTwoSubtitlesViewModel>(vm =>
        {
            vm.Initialize(lines, hasOriginal);
        });

        if (!result.OkPressed)
        {
            return;
        }

        ResetSubtitle();
        // Set the format before building the rows so SubtitleLineViewModel copies the ASSA style (Extra) into Style
        SetSubtitleFormat(SubtitleFormats.FirstOrDefault(p => p.Name == result.ResultFormat.Name) ??
                          SubtitleFormats[0]);
        SetSubtitles(result.ResultSubtitle);
        // Carry over the header so the merged styles (Style1/Style2) are kept
        _subtitle.Header = result.ResultSubtitle.Header;
        SelectAndScrollToRow(0);
    }

    [RelayCommand]
    private async Task ShowToolsMergeLinesWithSameText()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        var result = await ShowDialogAsync<MergeSameTextWindow, MergeSameTextViewModel>(vm => { vm.Initialize(Subtitles.Select(p => new SubtitleLineViewModel(p)).ToList()); });

        if (!result.OkPressed)
        {
            return;
        }

        Subtitles.Clear();
        Subtitles.AddRange(result.ResultSubtitles.Select(p => new SubtitleLineViewModel(p)));
        Renumber();
        SelectAndScrollToRow(0);
    }

    [RelayCommand]
    private async Task ShowToolsMergeLinesWithSameTimeCodes()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        var result = await ShowDialogAsync<MergeSameTimeCodesWindow, MergeSameTimeCodesViewModel>(vm =>
        {
            vm.Initialize(Subtitles.Select(p => new SubtitleLineViewModel(p)).ToList(), GetUpdateSubtitle());
        });

        if (!result.OkPressed)
        {
            return;
        }

        Subtitles.Clear();
        Subtitles.AddRange(result.ResultSubtitles.Select(p => new SubtitleLineViewModel(p)));
        Renumber();
        SelectAndScrollToRow(0);
    }

    [RelayCommand]
    private void ToolsMakeEmptyTranslationFromCurrentSubtitle()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        foreach (var subtitle in Subtitles)
        {
            subtitle.OriginalText = subtitle.Text;
            subtitle.Text = string.Empty;
        }

        _subtitleFileNameOriginal = _subtitleFileName;
        _subtitleOriginal ??= new Subtitle();
        _subtitleOriginal.OriginalFormat = _subtitle.OriginalFormat ?? SelectedSubtitleFormat;
        _subtitleFileName = null;
        _converted = true;
        _shortcutManager.ClearKeys();
        ShowColumnOriginalText = true;
        AutoFitColumns();
        ShowStatus(Se.Language.Main.CreatedEmptyTranslation);
    }

    [RelayCommand]
    private void CopyTextFromOriginalToTranslation()
    {
        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
        if (Window == null || selectedItems.Count == 0 || !ShowColumnOriginalText)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        foreach (var subtitle in selectedItems)
        {
            subtitle.Text = subtitle.OriginalText;
        }

        _shortcutManager.ClearKeys();
        var msg = string.Format(Se.Language.Main.XLinesCopiedFromOriginal, selectedItems.Count);
        if (selectedItems.Count == 1)
        {
            msg = Se.Language.Main.OneLineCopiedFromOriginal;
        }

        ShowStatus(msg);
    }

    [RelayCommand]
    private void SwitchOriginalAndTranslationTextSelectedLines()
    {
        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
        if (Window == null || selectedItems.Count == 0 || !ShowColumnOriginalText)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        foreach (var subtitle in selectedItems)
        {
            (subtitle.Text, subtitle.OriginalText) = (subtitle.OriginalText, subtitle.Text);
        }

        _shortcutManager.ClearKeys();
        var msg = string.Format(Se.Language.Main.XLinesSwitched, selectedItems.Count);
        if (selectedItems.Count == 1)
        {
            msg = Se.Language.Main.OneLineSwitched;
        }

        ShowStatus(msg);
    }

    [RelayCommand]
    private void MergeOriginalIntoTranslationSelectedLines()
    {
        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
        if (Window == null || selectedItems.Count == 0 || !ShowColumnOriginalText)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        foreach (var subtitle in selectedItems)
        {
            subtitle.Text = subtitle.Text + Environment.NewLine + subtitle.OriginalText;
        }

        _shortcutManager.ClearKeys();
        var msg = string.Format(Se.Language.Main.XLinesMerged, selectedItems.Count);
        if (selectedItems.Count == 1)
        {
            msg = Se.Language.Main.OneLineMerged;
        }

        ShowStatus(msg);
    }

    [RelayCommand]
    private async Task CopyTextFromOriginalToClipboard()
    {
        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
        if (Window == null || selectedItems.Count == 0 || !ShowColumnOriginalText)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        var sb = new StringBuilder();
        foreach (var subtitle in selectedItems)
        {
            sb.AppendLine(subtitle.OriginalText);
        }

        await ClipboardHelper.SetTextAsync(Window, sb.ToString());

        ShowStatus(string.Format("{0} lines copied to clipboard", selectedItems.Count));
    }

    [RelayCommand]
    private async Task CopyTextToClipboard()
    {
        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
        if (Window == null || selectedItems.Count == 0)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        var sb = new StringBuilder();
        foreach (var subtitle in selectedItems)
        {
            sb.AppendLine(subtitle.Text);
        }

        await ClipboardHelper.SetTextAsync(Window, sb.ToString());

        ShowStatus(string.Format("{0} lines copied to clipboard", selectedItems.Count));
    }

    [RelayCommand]
    private async Task ShowToolsSplit()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        var s = GetUpdateSubtitle();
        var fileName = _subtitleFileName;
        if (s.Paragraphs.Count == 0)
        {
            fileName = await _fileHelper.PickOpenSubtitleFile(Window!, Se.Language.General.OpenSubtitleFileTitle,
                false);
            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }

            s = Subtitle.Parse(fileName);
        }

        if (s == null || s.Paragraphs.Count == 0)
        {
            return;
        }

        await ShowDialogAsync<SplitSubtitleWindow, SplitSubtitleViewModel>(vm => { vm.Initialize(fileName ?? string.Empty, s); });
    }

    [RelayCommand]
    private async Task ShowBridgeGaps()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        var result = await ShowDialogAsync<BridgeGapsWindow, BridgeGapsViewModel>(vm => { vm.Initialize(Subtitles.Select(p => new SubtitleLineViewModel(p)).ToList()); });

        if (result.OkPressed)
        {
            Subtitles.Clear();
            Subtitles.AddRange(result.Subtitles.Select(p => new SubtitleLineViewModel(p.SubtitleLineViewModel)));
            SelectAndScrollToRow(0);
            _updateAudioVisualizer = true;
        }
    }


    [RelayCommand]
    private async Task ShowApplyMinGap()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        var result = await ShowDialogAsync<ApplyMinGapWindow, ApplyMinGapViewModel>(vm => { vm.Initialize(Subtitles.Select(p => new SubtitleLineViewModel(p)).ToList()); });

        if (result.OkPressed)
        {
            Subtitles.Clear();
            Subtitles.AddRange(result.FixedSubtitles);
            SelectAndScrollToRow(0);
            _updateAudioVisualizer = true;
        }
    }

    [RelayCommand]
    private async Task ShowToolsChangeCasing()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        var result =
            await ShowDialogAsync<ChangeCasingWindow, ChangeCasingViewModel>(vm => { vm.Initialize(GetUpdateSubtitle()); });

        if (result.OkPressed)
        {
            for (var i = 0; i < Subtitles.Count; i++)
            {
                if (result.Subtitle.Paragraphs.Count <= i)
                {
                    break;
                }

                Subtitles[i].Text = result.Subtitle.Paragraphs[i].Text;
            }

            ShowStatus(result.Info);
        }
    }

    [RelayCommand]
    private async Task ShowToolsChangeFormatting()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        var idx = SelectedSubtitleIndex ?? 0;
        var result =
            await ShowDialogAsync<ChangeFormattingWindow, ChangeFormattingViewModel>(vm => { vm.Initialize(Subtitles.ToList(), SelectedSubtitleFormat); });

        if (result.OkPressed)
        {
            ApplyFixedSubtitle(result.FixedSubtitle, idx);
        }
    }

    [RelayCommand]
    private async Task ShowToolsRenumber()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        _ = await ShowDialogAsync<RenumberWindow, RenumberViewModel>(vm => { vm.Initialize(Subtitles.ToList()); });
    }

    [RelayCommand]
    private async Task ShowToolsConvertActors()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        var idx = SelectedSubtitleIndex ?? 0;
        var result =
            await ShowDialogAsync<ConvertActorsWindow, ConvertActorsViewModel>(vm => { vm.Initialize(Subtitles.ToList(), SelectedSubtitleFormat); });

        if (result.OkPressed)
        {
            ApplyFixedSubtitle(result.FixedSubtitle, idx);
        }
    }

    public IReadOnlyList<InstalledPlugin> GetInstalledPlugins()
    {
        try
        {
            return _pluginCatalog.GetPlugins();
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "Failed to load plugins");
            return new List<InstalledPlugin>();
        }
    }

    [RelayCommand]
    private async Task RunPlugin(InstalledPlugin? plugin)
    {
        if (Window == null || plugin == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        if (!plugin.CanRun)
        {
            await MessageBox.Show(Window, Se.Language.General.Error,
                string.Format(Se.Language.Plugins.PluginXHasNoExecutable, plugin.Manifest.Name),
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var selectedIndices = GetSelectedSubtitleIndices();
        if (selectedIndices.Count > 0 && selectedIndices.Count < Subtitles.Count)
        {
            var choice = await MessageBox.Show(
                Window,
                plugin.Manifest.Name,
                string.Format(Se.Language.Plugins.ApplyPluginToWhichLinesX, plugin.Manifest.Name),
                MessageBoxButtons.Cancel,
                MessageBoxIcon.Question,
                custom1: string.Format(Se.Language.Plugins.ApplyToSelectedLinesX, selectedIndices.Count),
                custom2: string.Format(Se.Language.Plugins.ApplyToAllLinesX, Subtitles.Count));

            if (choice == MessageBoxResult.Cancel || choice == MessageBoxResult.None)
            {
                return;
            }

            if (choice == MessageBoxResult.Custom2)
            {
                selectedIndices.Clear();
            }
        }

        var request = BuildPluginRequest(plugin, selectedIndices);
        var runResult = await _pluginRunner.RunAsync(plugin, request, System.Threading.CancellationToken.None);
        if (runResult.WasCancelled)
        {
            return;
        }

        if (!runResult.Succeeded || runResult.Response == null)
        {
            await MessageBox.Show(Window, Se.Language.General.Error,
                runResult.ErrorMessage ?? string.Format(Se.Language.Plugins.PluginXFailed, plugin.Manifest.Name),
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var response = runResult.Response;
        if (response.Status == PluginConstants.StatusCancelled)
        {
            return;
        }

        if (response.Status == PluginConstants.StatusError)
        {
            await MessageBox.Show(Window, Se.Language.General.Error,
                string.IsNullOrWhiteSpace(response.Message)
                    ? string.Format(Se.Language.Plugins.PluginXReportedError, plugin.Manifest.Name)
                    : response.Message!,
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        SavePluginSettings(plugin, response);

        if (response.Subtitle == null || string.IsNullOrWhiteSpace(response.Subtitle.Native))
        {
            if (!string.IsNullOrWhiteSpace(response.Message))
            {
                ShowStatus(response.Message!);
            }

            return;
        }

        if (!await ApplyPluginSubtitle(plugin, response))
        {
            return;
        }

        ShowStatus(string.IsNullOrWhiteSpace(response.Message)
            ? string.Format(Se.Language.Plugins.PluginXDone, plugin.Manifest.Name)
            : response.Message!);
    }

    private List<int> GetSelectedSubtitleIndices()
    {
        var selectedIndices = new List<int>();
        if (SubtitleGrid.SelectedItems != null)
        {
            foreach (var item in SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>())
            {
                var index = Subtitles.IndexOf(item);
                if (index >= 0)
                {
                    selectedIndices.Add(index);
                }
            }

            selectedIndices.Sort();
        }

        return selectedIndices;
    }

    private PluginRequest BuildPluginRequest(InstalledPlugin plugin, List<int> selectedIndices)
    {
        var subtitle = GetUpdateSubtitle();
        var format = SelectedSubtitleFormat;

        var frameRate = _mediaInfo != null
            ? (double)_mediaInfo.FramesRateNonNormalized
            : Se.Settings.General.CurrentFrameRate;

        JsonElement? settings = null;
        if (Se.Settings.Plugins.Settings.TryGetValue(plugin.Manifest.Name, out var settingsJson) &&
            !string.IsNullOrWhiteSpace(settingsJson))
        {
            try
            {
                using var doc = JsonDocument.Parse(settingsJson);
                settings = doc.RootElement.Clone();
            }
            catch (Exception exception)
            {
                Se.LogError(exception, "Invalid stored plugin settings for " + plugin.Manifest.Name);
            }
        }

        int? settingsVersion = null;
        if (Se.Settings.Plugins.SettingsVersions.TryGetValue(plugin.Manifest.Name, out var storedVersion))
        {
            settingsVersion = storedVersion;
        }

        return new PluginRequest
        {
            Subtitle = new PluginSubtitle
            {
                Format = format.Name,
                FileName = _subtitleFileName ?? string.Empty,
                Native = subtitle.ToText(format),
                SubRip = subtitle.ToText(new Nikse.SubtitleEdit.Core.SubtitleFormats.SubRip()),
            },
            SelectedIndices = selectedIndices,
            VideoFileName = _videoFileName ?? string.Empty,
            FrameRate = frameRate,
            VideoDurationSeconds = _mediaInfo?.Duration?.TotalSeconds,
            VideoWidth = _mediaInfo?.Dimension.Width,
            VideoHeight = _mediaInfo?.Dimension.Height,
            UiLanguage = Se.Settings.General.Language ?? string.Empty,
            Theme = UiTheme.ThemeName,
            ThemeColors = PluginThemeColorsFactory.Build(),
            SeVersion = Se.Version,
            Settings = settings,
            SettingsVersion = settingsVersion,
        };
    }

    private async Task<bool> ApplyPluginSubtitle(InstalledPlugin plugin, PluginResponse response)
    {
        var pluginSubtitle = response.Subtitle!;
        var lines = pluginSubtitle.Native.SplitToLines().ToList();

        var format = SubtitleFormat.AllSubtitleFormats
            .FirstOrDefault(f => f.Name.Equals(pluginSubtitle.Format, StringComparison.OrdinalIgnoreCase));

        var subtitle = new Subtitle();
        var loadedFormat = subtitle.ReloadLoadSubtitle(lines, string.Empty, format);
        if (loadedFormat == null || subtitle.Paragraphs.Count == 0)
        {
            await MessageBox.Show(Window!, Se.Language.General.Error,
                string.Format(Se.Language.Plugins.PluginXReturnedUnparsableSubtitle, plugin.Manifest.Name),
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        var idx = SelectedSubtitleIndex ?? 0;
        SetSubtitles(subtitle);
        SelectAndScrollToRow(Math.Min(idx, Math.Max(0, Subtitles.Count - 1)));
        return true;
    }

    private static void SavePluginSettings(InstalledPlugin plugin, PluginResponse response)
    {
        if (!response.Settings.HasValue)
        {
            return;
        }

        try
        {
            Se.Settings.Plugins.Settings[plugin.Manifest.Name] = response.Settings.Value.GetRawText();
            if (response.SettingsVersion.HasValue)
            {
                Se.Settings.Plugins.SettingsVersions[plugin.Manifest.Name] = response.SettingsVersion.Value;
            }
            else
            {
                Se.Settings.Plugins.SettingsVersions.Remove(plugin.Manifest.Name);
            }
            Se.SaveSettings();
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "Failed to persist plugin settings for " + plugin.Manifest.Name);
        }
    }

    [RelayCommand]
    private async Task ShowPluginManager()
    {
        if (Window == null)
        {
            return;
        }

        await ShowDialogAsync<PluginManagerWindow, PluginManagerViewModel>(vm => vm.Initialize());
        Layout.InitMenu.UpdatePluginsMenu(this);
        if (OperatingSystem.IsMacOS())
            Layout.InitNativeMacMenu.UpdatePluginsMenu(this);
    }

    [RelayCommand]
    private async Task ShowToolsFixCommonErrors()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        var idx = SelectedSubtitleIndex ?? 0;
        var viewModel = await ShowDialogAsync<FixCommonErrorsWindow, FixCommonErrorsViewModel>(vm => { vm.Initialize(GetUpdateSubtitle(), SelectedSubtitleFormat); });

        if (viewModel.OkPressed)
        {
            ApplyFixedSubtitle(viewModel.FixedSubtitle, idx);
            ShowStatus(string.Format(Se.Language.Main.FixedXLines, viewModel.FixedSubtitle.Paragraphs.Count));
        }
    }

    [RelayCommand]
    private async Task ShowToolsFixNetflixErrors()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        var idx = SelectedSubtitleIndex ?? 0;
        var viewModel = await ShowDialogAsync<FixNetflixErrorsWindow, FixNetflixErrorsViewModel>(vm => { vm.Initialize(GetUpdateSubtitle(), _videoFileName ?? string.Empty); });

        if (viewModel.OkPressed)
        {
            ApplyFixedSubtitle(viewModel.FixedSubtitle, idx);
            ShowStatus(string.Format(Se.Language.Main.FixedXLines, viewModel.FixedSubtitle.Paragraphs.Count));
        }
    }

    [RelayCommand]
    private async Task ShowSubtitleFormatPicker()
    {
        if (Window == null)
        {
            return;
        }

        var viewModel = await ShowDialogAsync<PickSubtitleFormatWindow, PickSubtitleFormatViewModel>(vm => { vm.Initialize(SelectedSubtitleFormat, GetUpdateSubtitle()); });

        if (viewModel.OkPressed)
        {
            var selectedFormat = viewModel.GetSelectedFormat();
            if (selectedFormat != null && selectedFormat != SelectedSubtitleFormat)
            {
                SetSubtitleFormat(selectedFormat);
            }
        }
    }

    private void ShowSubtitleNotLoadedMessage()
    {
        Dispatcher.UIThread.Post(async void () =>
        {
            await MessageBox.Show(
                Window!,
                Se.Language.General.Error,
                Se.Language.General.NoSubtitleLoaded,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        });
    }


    [RelayCommand]
    private async Task ShowToolsSplitBreakLongLines()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        var result = await ShowDialogAsync<SplitBreakLongLinesWindow, SplitBreakLongLinesViewModel>(
            vm => { vm.Initialize(Subtitles.ToList()); });

        if (result.OkPressed && result.AllSubtitlesFixed.Count > 0)
        {
            Subtitles.Clear();
            Subtitles.AddRange(result.AllSubtitlesFixed);
            SelectAndScrollToRow(0);
            _updateAudioVisualizer = true;
            RefreshSubtitlePreview();
        }
    }

    [RelayCommand]
    private async Task ShowToolsMergeShortLines()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        var result = await ShowDialogAsync<MergeShortLinesWindow, MergeShortLinesViewModel>(
            vm => { vm.Initialize(Subtitles.ToList(), AudioVisualizer?.ShotChanges ?? new List<double>()); });

        if (result.OkPressed)
        {
            Subtitles.Clear();
            Subtitles.AddRange(result.AllSubtitlesFixed);
            SelectAndScrollToRow(0);
            _updateAudioVisualizer = true;
            RefreshSubtitlePreview();
        }
    }

    [RelayCommand]
    private void SnapAllTimesToFrames()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        var frameRate = Se.Settings.General.CurrentFrameRate;
        if (frameRate < 1)
        {
            return;
        }

        var frameDurMs = 1000.0 / frameRate;
        var changed = 0;
        RunWithoutChangeDetection(() =>
        {
            foreach (var s in Subtitles)
            {
                var newStartMs = Math.Round(s.StartTime.TotalMilliseconds / frameDurMs, MidpointRounding.AwayFromZero) * frameDurMs;
                var newEndMs = Math.Round(s.EndTime.TotalMilliseconds / frameDurMs, MidpointRounding.AwayFromZero) * frameDurMs;

                // Snapping can collapse start and end to the same frame (or invert them) for
                // sub-frame durations; keep the cue at least one frame long.
                if (newEndMs <= newStartMs)
                {
                    newEndMs = newStartMs + frameDurMs;
                }

                if (Math.Abs(newStartMs - s.StartTime.TotalMilliseconds) >= 0.5)
                {
                    s.SetStartTimeOnly(TimeSpan.FromMilliseconds(newStartMs));
                    changed++;
                }

                if (Math.Abs(newEndMs - s.EndTime.TotalMilliseconds) >= 0.5)
                {
                    s.EndTime = TimeSpan.FromMilliseconds(newEndMs);
                    changed++;
                }
            }
        });

        _updateAudioVisualizer = true;
        ShowStatus(string.Format(Se.Language.Main.SnappedXTimesToFrames, changed, frameRate));
    }

    [RelayCommand]
    private async Task ShowToolsMergeContinuationLines()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        var language = Subtitles.AutoDetectGoogleLanguage();
        var result = await ShowDialogAsync<MergeContinuationLinesWindow, MergeContinuationLinesViewModel>(
            vm => { vm.Initialize(Subtitles.ToList(), language); });

        if (result.OkPressed)
        {
            Subtitles.Clear();
            Subtitles.AddRange(result.AllSubtitlesFixed);
            SelectAndScrollToRow(0);
            _updateAudioVisualizer = true;
            RefreshSubtitlePreview();
        }
    }

    [RelayCommand]
    private async Task ShowToolsRemoveTextForHearingImpaired()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        var result = await ShowDialogAsync<RemoveTextForHearingImpairedWindow, RemoveTextForHearingImpairedViewModel>(
            vm => { vm.Initialize(GetUpdateSubtitle()); });

        if (result.OkPressed)
        {
            Subtitles.Clear();
            Subtitles.AddRange(
                result.FixedSubtitle.Paragraphs.Select(p => new SubtitleLineViewModel(p, SelectedSubtitleFormat)));
            SelectAndScrollToRow(0);
        }
    }

    [RelayCommand]
    private void ToggleAudioTracks()
    {
        var control = GetVideoPlayerControl();
        if (control == null)
        {
            return;
        }

        var track = control.ToggleAudioTrack();
        if (track == null)
        {
            return;
        }

        _audioTrack = track;
        var _ = Task.Run(LoadAudioTrackMenuItems);

        ShowStatus(string.Format(Se.Language.Main.AudioTrackIsNowX, track));
    }

    [RelayCommand]
    private void PickAudioTrack(object? parameter)
    {
        if (string.IsNullOrEmpty(_videoFileName) || parameter == null)
        {
            return;
        }

        var vp = GetVideoPlayerControl();
        if (vp == null)
        {
            return;
        }

        if (vp.VideoPlayer is LibMpvDynamicPlayer mpv && parameter is AudioTrackInfo audioTrack)
        {
            mpv.SetAudioTrack(audioTrack.Id);
            _audioTrack = audioTrack;
            var _ = Task.Run(LoadAudioTrackMenuItems);
            ShowStatus(string.Format(Se.Language.Main.AudioTrackIsNowX, _audioTrack));

            if (AudioVisualizer != null)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    AudioVisualizer.WavePeaks = null;
                    AudioVisualizer.ShotChanges = new List<double>();
                });
            }

            _updateAudioVisualizer = true;
            LoadWaveformAndSpectrogram(_videoFileName);
        }
    }

    [RelayCommand]
    private async Task CommandVideoOpen()
    {
        var fileName = await _fileHelper.PickOpenVideoFile(Window!, Se.Language.General.OpenVideoFileTitle);
        if (!string.IsNullOrEmpty(fileName))
        {
            await VideoOpenFile(fileName);

            if (!string.IsNullOrEmpty(_videoFileName) && InitLayout.LayoutHasNoVideo(Se.Settings.General.LayoutNumber))
            {
                var answer = await MessageBox.Show(
                    Window!,
                    Se.Language.Title,
                    Se.Language.Main.VideoOpenedChangeLayoutQuestion,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                if (answer == MessageBoxResult.Yes)
                {
                    await CommandShowLayout();
                }
            }
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private void CommandVideoClose()
    {
        VideoCloseFile();
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowGoToVideoPosition()
    {
        var vp = GetVideoPlayerControl();
        if (Subtitles.Count == 0 || string.IsNullOrEmpty(_videoFileName) || vp == null || Window == null)
        {
            return;
        }

        var viewModel =
            await ShowDialogAsync<GoToVideoPositionWindow, GoToVideoPositionViewModel>(vm => { vm.Time = TimeSpan.FromSeconds(vp.Position); });

        if (viewModel is { OkPressed: true, Time.TotalMicroseconds: >= 0 })
        {
            vp.Position = viewModel.Time.TotalSeconds;
            _updateAudioVisualizer = true;
        }
    }

    [RelayCommand]
    private void ToggleIsWaveformToolbarVisible()
    {
        IsWaveformToolbarVisible = !IsWaveformToolbarVisible;
    }

    [RelayCommand]
    private void VideoUndockControls()
    {
        var vp = GetVideoPlayerControl();
        if (Window == null || vp == null)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            AreVideoControlsUndocked = true;

            var position = vp.Position;
            var volume = vp.Volume;
            var videoFileName = vp.VideoPlayer.FileName;
            VideoPlayerControl?.Close();
            VideoPlayerControl = null;

            if (_videoPlayerUndockedViewModel != null)
            {
                _videoPlayerUndockedViewModel.AllowClose = true;
                _videoPlayerUndockedViewModel.Window?.Close();
                _videoPlayerUndockedViewModel = null;
            }

            if (_audioVisualizerUndockedViewModel != null)
            {
                _audioVisualizerUndockedViewModel.AllowClose = true;
                _audioVisualizerUndockedViewModel.Window?.Close();
                _audioVisualizerUndockedViewModel = null;
            }

            _windowService.ShowIndependentWindow<VideoPlayerUndockedWindow, VideoPlayerUndockedViewModel>((window, vm) =>
            {
                _videoPlayerUndockedViewModel = vm;
                vm.Initialize(_videoFileName ?? string.Empty, position, volume, this);
                // Float above main while main (or this window) is active, but drop behind when
                // SE loses focus to another app. Mirrors the Find/Replace helper from #11243.
                // The two undocked windows are still independent in Alt+Tab — KeepTopmost… is
                // just a Z-order knob, not an ownership change.
                WindowService.KeepTopmostWhileOwnerActive(window, Window!);
            });

            _windowService.ShowIndependentWindow<AudioVisualizerUndockedWindow, AudioVisualizerUndockedViewModel>((window, vm) =>
            {
                _audioVisualizerUndockedViewModel = vm;
                vm.Initialize(AudioVisualizer, this);
                ReloadAudioVisualizer();
                WindowService.KeepTopmostWhileOwnerActive(window, Window!);
            });

            InitLayout.MakeLayout12KeepVideo(MainView!, this);
            RefreshSubtitlePreview();
        });
    }

    [RelayCommand]
    private void VideoRedockControls()
    {
        AreVideoControlsUndocked = false;
        var videoFileName = _videoFileName ?? string.Empty;
        VideoCloseFile();

        if (_videoPlayerUndockedViewModel != null)
        {
            _videoPlayerUndockedViewModel.AllowClose = true;
            _videoPlayerUndockedViewModel.Window?.Close();
        }

        if (_audioVisualizerUndockedViewModel != null)
        {
            _audioVisualizerUndockedViewModel.AllowClose = true;
            _audioVisualizerUndockedViewModel.Window?.Close();
        }

        VideoPlayerControl = null;
        SetLayout(Se.Settings.General.LayoutNumber);

        if (!string.IsNullOrEmpty(videoFileName))
        {
            Dispatcher.UIThread.Post(async void () => { await VideoOpenFile(videoFileName); });
            RefreshSubtitlePreview();
        }
    }

    [RelayCommand]
    private async Task ShowSpellCheck()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        var dictionaryFileName = _currentSpellCheckDictionary?.DictionaryFileName ?? null;
        var result = await ShowDialogAsync<SpellCheckWindow, SpellCheckViewModel>(vm => { vm.Initialize(Subtitles, SelectedSubtitleIndex, this, dictionaryFileName); });

        if (result.OkPressed)
        {
            _currentSpellCheckDictionary = result.SelectedDictionary;

            var msg = string.Format(
                Se.Language.Main.SpellCheckResult,
                result.TotalChangedWords,
                result.TotalSkippedWords);

            await MessageBox.Show(
                Window,
                Se.Language.SpellCheck.SpellCheck,
                msg,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }

    [RelayCommand]
    private async Task ShowAddToNameList()
    {
        if (Window == null)
        {
            return;
        }

        var word = EditTextBox.SelectedText;
        var dictionaries = _spellCheckManager.GetDictionaryLanguages(Se.DictionariesFolder);
        if (dictionaries.Count == 0)
        {
            await MessageBox.Show(Window, Se.Language.General.Error, Se.Language.SpellCheck.NoDictionariesFound,
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            _shortcutManager.ClearKeys();
            return;
        }

        var language = LanguageAutoDetect.AutoDetectGoogleLanguage(GetUpdateSubtitle());
        var selectedDictionary = dictionaries.First();

        foreach (var dict in dictionaries)
        {
            var fiveLetterName = dict.GetFiveLetterLanguageName();
            if (fiveLetterName != null && fiveLetterName.Contains(language))
            {
                selectedDictionary = dict;
                break;
            }
        }

        var result = await ShowDialogAsync<AddToNamesListWindow, AddToNamesListViewModel>(vm => { vm.Initialize(word, dictionaries, selectedDictionary); });
    }

    [RelayCommand]
    private async Task ShowFindDoubleWords()
    {
        if (Window == null)
        {
            return;
        }

        var result = await ShowDialogAsync<FindDoubleWordsWindow, FindDoubleWordsViewModel>(vm => { vm.Initialize(Subtitles.ToList()); });
        if (result.GoToPressed && result.SelectedSubtitle != null)
        {
            SelectAndScrollToSubtitle(result.SelectedSubtitle.Subtitle);
        }
    }

    [RelayCommand]
    private async Task ShowFindDoubleLines()
    {
        if (Window == null)
        {
            return;
        }

        var result = await ShowDialogAsync<FindDoubleLinesWindow, FindDoubleLinesViewModel>(vm => { vm.Initialize(Subtitles.ToList()); });
        if (result.GoToPressed && result.SelectedSubtitle != null)
        {
            SelectAndScrollToSubtitle(result.SelectedSubtitle.Subtitle);
        }
    }

    [RelayCommand]
    private async Task ShowSpellCheckDictionaries()
    {
        await ShowDialogAsync<GetDictionariesWindow, GetDictionariesViewModel>();
    }

    [RelayCommand]
    private async Task ShowChooseProfile()
    {
        if (Window == null)
        {
            return;
        }

        var result = await ShowDialogAsync<PickRuleProfileWindow, PickRuleProfileViewModel>();

        if (result is { OkPressed: true, SelectedProfile: not null })
        {
            var p = result.SelectedProfile.ToRulesProfile();
            Se.Settings.General.CurrentProfile = p.Name;
            Se.Settings.General.SubtitleLineMaximumLength = p.SubtitleLineMaximumLength;
            Se.Settings.General.SubtitleMaximumCharactersPerSeconds = (double)p.SubtitleMaximumCharactersPerSeconds;
            Se.Settings.General.SubtitleOptimalCharactersPerSeconds = (double)p.SubtitleOptimalCharactersPerSeconds;
            Se.Settings.General.SubtitleMaximumWordsPerMinute = (double)p.SubtitleMaximumWordsPerMinute;
            Se.Settings.General.MinimumBetweenLines.Milliseconds = p.MinimumMillisecondsBetweenLines;
            Se.Settings.General.MinimumBetweenLines.Frames = SubtitleFormat.MillisecondsToFrames(p.MinimumMillisecondsBetweenLines);
            Se.Settings.General.MaxNumberOfLines = p.MaxNumberOfLines;
            Se.Settings.General.UnbreakLinesShorterThan = p.MergeLinesShorterThan;
            Se.Settings.General.DialogStyle = p.DialogStyle.ToString();
            Se.Settings.General.ContinuationStyle = p.ContinuationStyle.ToString();
            Se.Settings.General.CpsLineLengthStrategy = p.CpsLineLengthStrategy;

            ShowStatus(string.Format(Se.Language.Main.RuleProfileIsX, p.Name));
        }
    }

    [RelayCommand]
    private async Task ShowSpeechToTextWhisper()
    {
        if (Window == null)
        {
            return;
        }

        var ffmpegOk = await RequireFfmpegOk();
        if (!ffmpegOk)
        {
            return;
        }

        var doContinue = await HasChangesContinue();
        if (!doContinue)
        {
            return;
        }

        var result =
            await ShowDialogAsync<SpeechToTextWindow, SpeechToTextViewModel>(vm => { vm.Initialize(_videoFileName, _audioTrack?.FfIndex ?? -1); });

        if (result.OkPressed && !result.IsBatchMode)
        {
            ResetSubtitle();

            _subtitle = result.TranscribedSubtitle;

            if (SelectedSubtitleFormat is AdvancedSubStationAlpha)
            {
                foreach (var p in _subtitle.Paragraphs)
                {
                    p.Text = AdvancedSubStationAlpha.FormatText(p.Text);
                }
            }

            _subtitleFileName = Path.ChangeExtension(_videoFileName ?? "transcription", SelectedSubtitleFormat.Extension);
            _converted = true;

            SetSubtitles(_subtitle);
            SelectAndScrollToRow(0);
            ShowStatus(string.Format(Se.Language.Main.TranscriptionCompletedWithXLines, _subtitle.Paragraphs.Count));
        }
        else if (result.OkPressed && result.IsBatchMode && result.BatchItems.Count == 1)
        {
            var batchVideoFileName = result.BatchItems[0].InputVideoFileName;
            var batchSubtitleFileName = result.LastBatchSubtitleFileName;

            if (!string.IsNullOrEmpty(batchSubtitleFileName) && File.Exists(batchSubtitleFileName))
            {
                var answer = await MessageBox.Show(
                    Window!,
                    Se.Language.Video.AudioToText.Title,
                    string.Format(Se.Language.Main.OpenSubtitleFileX, Path.GetFileName(batchSubtitleFileName)),
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (answer == MessageBoxResult.Yes)
                {
                    await VideoOpenFile(batchVideoFileName);
                    await SubtitleOpen(batchSubtitleFileName, batchVideoFileName);
                }
            }
        }
    }

    [RelayCommand]
    private async Task SpeechToTextSelectedLines()
    {
        if (Se.Settings.Tools.SpeechToTextSelectedLinesPromptFirstTimeOnly)
        {
            await SpeechToTextSelectedLinesPromptForLangaugeFirstTime();
        }
        else
        {
            var language = LanguageAutoDetect.AutoDetectGoogleLanguageOrNull(GetUpdateSubtitle());
            await SpeechToTextSelectedLines(true, language);
        }
    }

    [RelayCommand]
    private async Task SpeechToTextSelectedLinesPromptForLangaugeAlways()
    {
        await SpeechToTextSelectedLines(true, null);
    }

    [RelayCommand]
    private async Task SpeechToTextSelectedLinesPromptForLangaugeFirstTime()
    {
        var s = GetUpdateSubtitle();
        var language = LanguageAutoDetect.AutoDetectGoogleLanguage(s);
        var nullableLanguage = LanguageAutoDetect.AutoDetectGoogleLanguageOrNull(s);
        await SpeechToTextSelectedLines(_audioTrackLangauge != language, nullableLanguage);
        _audioTrackLangauge = language;
    }

    private async Task<bool> SpeechToTextSelectedLines(bool promptEngineAndLanguage, string? language)
    {
        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
        if (Window == null || selectedItems.Count == 0 || string.IsNullOrEmpty(_videoFileName))
        {
            return false;
        }

        var ffmpegOk = await RequireFfmpegOk();
        if (!ffmpegOk)
        {
            return false;
        }

        var resultGetAudioClips = await ShowDialogAsync<GetAudioClipsWindow, GetAudioClipsViewModel>(vm => { vm.Initialize(_videoFileName ?? string.Empty, selectedItems, _audioTrack?.FfIndex ?? -1); });

        if (!resultGetAudioClips.OkPressed || resultGetAudioClips.AudioClips.Count == 0)
        {
            return false;
        }

        var resultSpeechToText = await ShowDialogAsync<SpeechToTextWindow, SpeechToTextViewModel>(vm =>
        {
            vm.InitializeBatch(resultGetAudioClips.AudioClips, _audioTrack?.FfIndex ?? -1, !promptEngineAndLanguage, language);
        });

        if (!resultSpeechToText.OkPressed)
        {
            return false;
        }

        // Update selected lines with transcribed text
        var newLines = new List<SubtitleLineViewModel>();
        var deleteLines = new List<SubtitleLineViewModel>();
        for (var i = 0; i < selectedItems.Count; i++)
        {
            var selectedLine = selectedItems[i];
            var transcribedLine = resultSpeechToText.ResultAudioClips[i];
            if (transcribedLine != null)
            {
                if (selectedLine.Duration.TotalSeconds > 10 && transcribedLine.Transcription.Paragraphs.Count > 1)
                {
                    // generate new subtitles
                    deleteLines.Add(selectedLine);
                    foreach (var p in transcribedLine.Transcription.Paragraphs)
                    {
                        var newLine = new SubtitleLineViewModel(p, SelectedSubtitleFormat);
                        newLine.StartTime = selectedLine.StartTime + p.StartTime.TimeSpan;
                        newLines.Add(newLine);
                    }
                }
                else
                {
                    // single line update
                    var sb = new StringBuilder();
                    foreach (var line in transcribedLine.Transcription.Paragraphs)
                    {
                        sb.AppendLine(line.Text);
                    }

                    selectedLine.Text = sb.ToString().Trim();
                }
            }
        }

        foreach (var line in deleteLines)
        {
            Subtitles.Remove(line);
        }

        foreach (var line in newLines)
        {
            _insertService.InsertInCorrectPosition(Subtitles, line);
        }

        _updateAudioVisualizer = true;
        return true;
    }

    private void ResetPlaySelection()
    {
        _playSelectionItem = null;
    }

    [RelayCommand]
    private void PlaySelectedLinesWithLoop()
    {
        PlayerSelectedLines(true);
    }

    [RelayCommand]
    private void PlaySelectedLinesWithoutLoop()
    {
        PlayerSelectedLines(false);
    }

    [RelayCommand]
    private void PlaySelectedLinesAndFocusWaveform()
    {
        if (PlayerSelectedLines(false))
        {
            AudioVisualizer?.Focus();
        }
    }

    [RelayCommand]
    private void PlaySelectedLinesWithLoopAndFocusWaveform()
    {
        if (PlayerSelectedLines(true))
        {
            AudioVisualizer?.Focus();
        }
    }

    private bool PlayerSelectedLines(bool loop)
    {
        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().OrderBy(p => p.StartTime).ToList();
        var vp = GetVideoPlayerControl();
        if (Window == null || selectedItems.Count == 0 || vp == null)
        {
            return false;
        }

        vp.VideoPlayer.Pause();
        var p = selectedItems.First();
        vp.Position = p.StartTime.TotalSeconds;
        _playSelectionItem = new PlaySelectionItem(selectedItems, p.EndTime, loop);
        vp.VideoPlayer.Play();

        return true;
    }

    [RelayCommand]
    private async Task ShowVideoBurnIn()
    {
        if (Window == null)
        {
            return;
        }

        var ffmpegOk = await RequireFfmpegOk();
        if (!ffmpegOk)
        {
            return;
        }

        await ShowDialogAsync<BurnInWindow, BurnInViewModel>(vm => { vm.Initialize(_videoFileName ?? string.Empty, GetUpdateSubtitle(), SelectedSubtitleFormat); });
    }

    [RelayCommand]
    private async Task ShowVideoOpenFromUrl()
    {
        if (Window == null)
        {
            return;
        }

        var isYouTubeDlInstalled = File.Exists(YtDlpDownloadService.GetFullFileName());

        // If yt-dlp is installed, run the "is it outdated" check in the background
        // while the URL dialog is on screen — IsInstalledVersionOutdated spawns
        // `yt-dlp --version` which can take a noticeable moment (especially on
        // macOS where the self-extracting binary unpacks on first run). We only
        // need the result if the user actually submits a URL.
        Task<bool>? outdatedCheckTask = isYouTubeDlInstalled
            ? Task.Run(() => YtDlpDownloadService.IsInstalledVersionOutdated(CancellationToken.None))
            : null;

        if (!isYouTubeDlInstalled)
        {
            // Without the binary on disk there's nothing useful the URL dialog can
            // do — block on installing it first.
            if (!await PromptToDownloadYtDlp(Se.Language.Main.YoutubeDlNotInstalledDownloadNow))
            {
                return;
            }
        }

        var result = await ShowDialogAsync<OpenFromUrlWindow, OpenFromUrlViewModel>();

        if (!result.OkPressed || result.SelectedMode is null)
        {
            return;
        }

        var url = result.Url.Trim();
        if (string.IsNullOrEmpty(url))
        {
            return;
        }

        // Now that the user has committed to a URL, see whether the background
        // version check came back "outdated" — and only then interrupt with the
        // upgrade prompt. The check is almost always finished by this point, but
        // we await it to be safe.
        if (outdatedCheckTask is not null)
        {
            bool isOutdated;
            try
            {
                isOutdated = await outdatedCheckTask;
            }
            catch
            {
                // Treat check failures as not-outdated; a stale binary will surface
                // its own error on the next yt-dlp invocation.
                isOutdated = false;
            }

            if (isOutdated &&
                !await PromptToDownloadYtDlp(Se.Language.Main.YoutubeDlOutdatedDownloadNow))
            {
                return;
            }
        }

        switch (result.SelectedMode.Value)
        {
            case OpenFromUrlMode.OpenOnline:
                await VideoOpenFile(url);
                if (result.DownloadSubtitles)
                {
                    // No video on disk — fetch subs to a temp dir, show picker, then
                    // clean up the temp dir once the picked subtitle is loaded.
                    await DownloadSubtitlesOnlyAndPickAsync(url);
                }
                break;

            case OpenFromUrlMode.DownloadAndOpen:
                // The picker is invoked inside DownloadVideoFromUrlAndOpen so it
                // only fires when the video download actually succeeds — skipping
                // it when the user cancels the save-as or the download itself.
                await DownloadVideoFromUrlAndOpen(url, result.DownloadSubtitles);
                break;
        }
    }

    /// <summary>
    /// Streaming-mode helper: runs yt-dlp once to write every available subtitle
    /// to a temp directory, shows the chooser, loads the selected file, then
    /// cleans the temp directory. Best-effort — failures don't roll back the
    /// already-opened video.
    /// </summary>
    private async Task DownloadSubtitlesOnlyAndPickAsync(string url)
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), "SE-online-subs-" + Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(tempDirectory);
            var stem = Path.Combine(tempDirectory, "sub");

            ShowStatus(Se.Language.Video.PickOnlineSubtitleFetching);
            await _ytDlpDownloadService.DownloadAllSubtitlesAsync(url, stem, CancellationToken.None);

            var downloaded = YtDlpDownloadService.EnumerateDownloadedSubtitles(tempDirectory, "sub");
            await ShowPickerAndLoadAsync(downloaded);
        }
        catch (Exception ex)
        {
            Se.LogError(ex, "Failed to fetch subtitles for streaming URL");
        }
        finally
        {
            TryDeleteDirectory(tempDirectory);
        }
    }

    /// <summary>
    /// Shows the subtitle chooser populated with <paramref name="subtitles"/>; if
    /// the user picks one, loads it into the editor. The picker itself never
    /// downloads anything — it's just a chooser over files already on disk.
    /// </summary>
    private async Task ShowPickerAndLoadAsync(IReadOnlyList<DownloadedSubtitleInfo> subtitles)
    {
        var pickerResult = await ShowDialogAsync<PickOnlineSubtitleWindow, PickOnlineSubtitleViewModel>(
            vm => { vm.Initialize(subtitles); });

        if (!pickerResult.OkPressed || string.IsNullOrEmpty(pickerResult.SelectedSubtitlePath))
        {
            return;
        }

        await SubtitleOpen(pickerResult.SelectedSubtitlePath, skipLoadVideo: true);
    }

    private static void TryDeleteDirectory(string? path)
    {
        if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
        {
            return;
        }
        try
        {
            Directory.Delete(path, recursive: true);
        }
        catch
        {
            // best effort — leave the OS to garbage-collect /tmp
        }
    }

    /// <summary>
    /// Asks the user whether to download yt-dlp, runs the download window if so, and
    /// returns true when a usable current-version binary is on disk afterwards.
    /// Returns false if the user declined, no file was produced, or — for the
    /// outdated-upgrade path — the download didn't take and the old binary is
    /// still on disk.
    /// </summary>
    private async Task<bool> PromptToDownloadYtDlp(string message)
    {
        var download = await MessageBox.Show(Window!, Se.Language.General.Information,
            message,
            MessageBoxButtons.YesNo, MessageBoxIcon.Information);
        if (download != MessageBoxResult.Yes)
        {
            return false;
        }

        await ShowDialogAsync<DownloadYtDlpWindow, DownloadYtDlpViewModel>();
        if (!File.Exists(YtDlpDownloadService.GetFullFileName()))
        {
            return false;
        }

        // For the upgrade path File.Exists alone isn't enough — if the user
        // cancelled the download or it errored, the old (outdated) binary is
        // still on disk and File.Exists still returns true. Re-run the version
        // check to confirm the binary really is current. Safe because
        // IsVersionOutdated treats unknown/unparseable output as "not outdated"
        // (a flaky --version on a freshly written binary won't false-positive).
        YtDlpDownloadService.InvalidateInstalledVersionCache();
        if (await YtDlpDownloadService.IsInstalledVersionOutdated(CancellationToken.None, forceRefresh: true))
        {
            return false;
        }

        ShowStatus(Se.Language.Main.YoutubeDlDownloadedSuccessfully);
        return true;
    }

    private async Task DownloadVideoFromUrlAndOpen(string url, bool downloadSubtitles)
    {
        if (Window == null)
        {
            return;
        }

        var suggestedName = MakeDownloadSuggestedFileName(url);
        var outputPath = await _fileHelper.PickSaveFile(Window, ".mkv", suggestedName, Se.Language.Video.OpenFromUrlSaveAs);
        if (string.IsNullOrEmpty(outputPath))
        {
            return;
        }

        var downloadResult = await ShowDialogAsync<DownloadVideoFromUrlWindow, DownloadVideoFromUrlViewModel>(vm =>
        {
            vm.Initialize(url, outputPath, downloadSubtitles);
        });

        if (downloadResult.Success && File.Exists(downloadResult.OutputPath))
        {
            await VideoOpenFile(downloadResult.OutputPath);

            if (downloadSubtitles)
            {
                // Subs were written into a per-download GUID temp directory so the
                // picker can't accidentally list any pre-existing sidecar files
                // sitting next to the user's chosen save folder. Show the picker,
                // load the chosen file, then clean the temp dir up.
                try
                {
                    if (downloadResult.DownloadedSubtitles.Count > 0)
                    {
                        await ShowPickerAndLoadAsync(downloadResult.DownloadedSubtitles);
                    }
                }
                finally
                {
                    TryDeleteDirectory(downloadResult.TempSubtitleDirectory);
                }
            }
        }
    }

    private static string MakeDownloadSuggestedFileName(string url)
    {
        try
        {
            var uri = new Uri(url);
            var segments = uri.Segments;
            for (var i = segments.Length - 1; i >= 0; i--)
            {
                var segment = segments[i].Trim('/');
                if (!string.IsNullOrEmpty(segment))
                {
                    var sanitized = string.Join('_', segment.Split(Path.GetInvalidFileNameChars()));
                    return Path.ChangeExtension(sanitized, ".mkv");
                }
            }
        }
        catch
        {
            // fall through to default
        }

        return "video.mkv";
    }

    [RelayCommand]
    private async Task ShowVideoSetOffset()
    {
        if (Window == null)
        {
            return;
        }

        var result = await ShowDialogAsync<SetVideoOffsetWindow, SetVideoOffsetViewModel>();

        if (result.ResetPressed)
        {
            Se.Settings.General.CurrentVideoOffsetInMs = 0;
            UpdateVideoOffsetStatus();
            _updateAudioVisualizer = true;
            return;
        }

        if (!result.OkPressed || !result.TimeOffset.HasValue)
        {
            return;
        }

        var offset = result.TimeOffset.Value;
        if (result.RelativeToCurrentVideoPosition)
        {
            var vp = GetVideoPlayerControl();
            if (vp != null)
            {
                offset = offset - TimeSpan.FromSeconds(vp.Position);
            }

            Se.Settings.General.CurrentVideoOffsetInMs = (int)Math.Round(offset.TotalMilliseconds, MidpointRounding.AwayFromZero);
        }

        if (result.KeepTimeCodes)
        {
            foreach (var s in Subtitles)
            {
                s.StartTime = s.StartTime - offset;
            }
        }

        UpdateVideoOffsetStatus();
        _updateAudioVisualizer = true;
    }

    private void UpdateVideoOffsetStatus()
    {
        IsVideoOffsetVisible = Se.Settings.General.CurrentVideoOffsetInMs != 0;
        if (IsVideoOffsetVisible)
        {
            VideoOffsetText = new TimeCode(Se.Settings.General.CurrentVideoOffsetInMs).ToShortString();
            SetVideoOffsetText = string.Format(Se.Language.Main.Menu.UpdateVideoOffsetX, VideoOffsetText);
        }
        else
        {
            VideoOffsetText = string.Empty;
            SetVideoOffsetText = Se.Language.Main.Menu.SetVideoOffset;
        }

        // refresh all Subtitle rows
        foreach (var s in Subtitles)
        {
            s.RefreshTimeCodes();
        }

        var ss = SelectedSubtitle;
        if (ss != null)
        {
            // trigger UI update
            ss.StartTime = ss.StartTime.Add(TimeSpan.FromMilliseconds(1));
            ss.StartTime = ss.StartTime.Add(TimeSpan.FromMilliseconds(-1));
            SubtitleGridSelectionChanged();
        }
    }

    [RelayCommand]
    private void ToggleSmpteTiming()
    {
        if (Window == null || string.IsNullOrEmpty(_videoFileName))
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        if (IsSmpteTimingEnabled)
        {
            IsSmpteTimingEnabled = false;
            if (AudioVisualizer != null)
            {
                ReloadAudioVisualizer();
            }
        }
        else
        {
            IsSmpteTimingEnabled = true;
            if (AudioVisualizer != null)
            {
                AudioVisualizer.UseSmpteDropFrameTime();
            }
        }

        var vp = GetVideoPlayerControl();
        if (vp != null)
        {
            vp.IsSmpteTimingEnabled = IsSmpteTimingEnabled;
        }

        _mpvReloader.SmpteMode = IsSmpteTimingEnabled;
        _vlcReloader.SmpteMode = IsSmpteTimingEnabled;
    }

    [RelayCommand]
    private async Task ShowSmpteTiming()
    {
        if (Window == null || string.IsNullOrEmpty(_videoFileName))
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        var answer = await MessageBox.Show(Window, Se.Language.General.Information,
            "Turn SMPTE timing off?",
            MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

        if (answer != MessageBoxResult.Yes)
        {
            return;
        }

        ToggleSmpteTiming();
    }

    private void ReloadAudioVisualizer()
    {
        if (string.IsNullOrEmpty(_videoFileName))
        {
            return;
        }

        var peakWaveFileName = WavePeakGenerator2.GetPeakWaveFileName(_videoFileName, _audioTrack?.FfIndex ?? -1);
        if (string.IsNullOrEmpty(peakWaveFileName) || !File.Exists(peakWaveFileName))
        {
            return;
        }

        var wavePeaks = WavePeakData2.FromDisk(peakWaveFileName);
        if (AudioVisualizer != null)
        {
            AudioVisualizer.WavePeaks = wavePeaks;

            if (IsSmpteTimingEnabled)
            {
                AudioVisualizer.UseSmpteDropFrameTime();
            }

            var spectrogramFileName = WavePeakGenerator2.SpectrogramDrawer.GetSpectrogramFileName(_videoFileName, _audioTrack?.FfIndex ?? -1);
            if (File.Exists(spectrogramFileName))
            {
                var spectrogram = SpectrogramData2.FromDisk(spectrogramFileName);
                if (spectrogram != null)
                {
                    spectrogram.Load();
                    AudioVisualizer.SetSpectrogram(spectrogram);
                }
            }

            InitializeWaveformDisplayMode();

            AudioVisualizer.ShotChanges = ShotChangesHelper.FromDisk(_videoFileName);
            if (AudioVisualizer.ShotChanges.Count == 0)
            {
                ExtractShotChanges(_videoFileName, _audioTrack?.FfIndex ?? -1);
            }
        }
    }

    [RelayCommand]
    private async Task ShowVideoTextToSpeech()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        var ffmpegOk = await RequireFfmpegOk();
        if (!ffmpegOk)
        {
            return;
        }

        await ShowDialogAsync<TextToSpeechWindow, TextToSpeechViewModel>(vm =>
        {
            // Pass SelectedSubtitleFormat explicitly so the TTS window's cast detection uses the
            // user's *current* selection (the editor normalises subtitles to ASSA internally, so
            // subtitle.OriginalFormat doesn't reflect what's shown in the format combo).
            vm.Initialize(GetUpdateSubtitle(), SelectedSubtitleFormat,
                _videoFileName ?? string.Empty, AudioVisualizer?.WavePeaks, Path.GetTempPath());
        });
    }

    [RelayCommand]
    private async Task ShowVideoTransparentSubtitles()
    {
        if (Window == null)
        {
            return;
        }

        var ffmpegOk = await RequireFfmpegOk();
        if (!ffmpegOk)
        {
            return;
        }

        await ShowDialogAsync<TransparentSubtitlesWindow, TransparentSubtitlesViewModel>(vm =>
        {
            vm.Initialize(_videoFileName ?? string.Empty, GetUpdateSubtitle(), SelectedSubtitleFormat);
        });
    }

    [RelayCommand]
    private async Task ShowShotChangesSubtitles()
    {
        if (Window == null)
        {
            return;
        }

        var vp = GetVideoPlayerControl();
        if (string.IsNullOrEmpty(_videoFileName) || vp == null || AudioVisualizer == null)
        {
            return;
        }

        var ffmpegOk = await RequireFfmpegOk();
        if (!ffmpegOk)
        {
            return;
        }

        var result = await ShowDialogAsync<ShotChangesWindow, ShotChangesViewModel>(vm => { vm.Initialize(_videoFileName); });

        if (result.OkPressed && result.FfmpegLines.Count > 0)
        {
            AudioVisualizer.ShotChanges = result.FfmpegLines.Select(p => p.Seconds).ToList();
            ShowShotChangesListMenuItem = AudioVisualizer.ShotChanges.Count > 0;
            _updateAudioVisualizer = true;
            ShotChangesHelper.SaveShotChanges(_videoFileName, AudioVisualizer.ShotChanges, _audioTrack?.FfIndex ?? -1);
            ShowStatus(string.Format(Se.Language.Main.XShotChangedLoaded, AudioVisualizer.ShotChanges.Count));
        }
    }


    [RelayCommand]
    private async Task ShowShotChangesList()
    {
        var vp = GetVideoPlayerControl();
        if (Window == null || vp == null)
        {
            return;
        }

        var selected = SelectedSubtitle;
        if (selected == null)
        {
            return;
        }

        var result = await ShowDialogAsync<ShotChangeListWindow, ShotChangeListViewModel>(vm => { vm.Initialize(AudioVisualizer?.ShotChanges ?? new List<double>()); });

        if (result.OKProssed && AudioVisualizer != null)
        {
            AudioVisualizer.ShotChanges = result.ShotChanges.Select(p => p.Seconds).ToList();
        }

        if (result.GoToPressed && result.SelectedShotChange != null)
        {
            vp.Position = result.SelectedShotChange.Seconds;
        }

        ShowShotChangesListMenuItem = AudioVisualizer?.ShotChanges.Count > 0;
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void ToggleShotChangesAtVideoPosition()
    {
        var vp = GetVideoPlayerControl();
        if (string.IsNullOrEmpty(_videoFileName) || vp == null || AudioVisualizer == null)
        {
            return;
        }

        var cp = AudioVisualizer.CurrentVideoPositionSeconds;
        var idx = AudioVisualizer.GetShotChangeIndex(cp);
        if (idx >= 0)
        {
            RemoveShotChange(idx);
            if (AudioVisualizer.ShotChanges.Count == 0)
            {
                ShotChangesHelper.DeleteShotChanges(_videoFileName, _audioTrack?.FfIndex ?? -1);
            }
        }
        else
        {
            // add shot change
            var list = AudioVisualizer.ShotChanges.Where(p => p > 0).ToList();
            list.Add(cp);
            list.Sort();
            AudioVisualizer.ShotChanges = list;
            ShotChangesHelper.SaveShotChanges(_videoFileName, list, _audioTrack?.FfIndex ?? -1);
        }

        ShowShotChangesListMenuItem = AudioVisualizer?.ShotChanges.Count > 0;
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void GoToPreviousShotChange()
    {
        var vp = GetVideoPlayerControl();
        if (string.IsNullOrEmpty(_videoFileName) || vp == null || AudioVisualizer == null || AudioVisualizer.ShotChanges.Count == 0)
        {
            return;
        }

        var shotChange = AudioVisualizer.ShotChanges.LastOrDefault(s => s < vp.Position - 0.001);
        if (shotChange <= 0)
        {
            return;
        }

        vp.Position = shotChange;
        AudioVisualizerCenterOnPositionIfNeeded(shotChange);
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void GoToNextShotChange()
    {
        var vp = GetVideoPlayerControl();
        if (string.IsNullOrEmpty(_videoFileName) || vp == null || AudioVisualizer == null || AudioVisualizer.ShotChanges.Count == 0)
        {
            return;
        }

        var shotChange = AudioVisualizer.ShotChanges.FirstOrDefault(s => s > vp.Position + 0.001);
        if (shotChange <= 0)
        {
            return;
        }

        vp.Position = shotChange;
        AudioVisualizerCenterOnPositionIfNeeded(shotChange);
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void ExtendSelectedLinesToNextShotChangeOrNextSubtitle()
    {
        var selectedLines = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().OrderBy(p => p.StartTime).ToList();
        var vp = GetVideoPlayerControl();
        if (string.IsNullOrEmpty(_videoFileName) ||
            vp == null ||
            AudioVisualizer == null ||
            AudioVisualizer.ShotChanges.Count == 0 ||
            selectedLines.Count == 0)
        {
            return;
        }

        foreach (var line in selectedLines)
        {
            var idx = Subtitles.IndexOf(line);
            var next = Subtitles.GetOrNull(idx + 1);
            var shotChange = AudioVisualizer.ShotChanges.FirstOrDefault(s => s > line.EndTime.TotalSeconds - 0.01);
            if (next == null && shotChange == 0)
            {
                continue;
            }

            if (shotChange == 0)
            {
                var newDuration = next!.StartTime - line.StartTime;
                if (newDuration.TotalMilliseconds <= Se.Settings.General.SubtitleMaximumDisplayMilliseconds)
                {
                    line.EndTime = TimeSpan.FromMilliseconds(next.StartTime.TotalMilliseconds - Se.Settings.General.MinimumBetweenLines.GetMilliseconds());
                }

                continue;
            }

            if (next == null)
            {
                var newDuration = TimeSpan.FromSeconds(shotChange) - line.StartTime;
                if (newDuration.TotalMilliseconds <= Se.Settings.General.SubtitleMaximumDisplayMilliseconds)
                {
                    line.EndTime = TimeSpan.FromSeconds(shotChange);
                }

                continue;
            }

            if (TimeSpan.FromSeconds(shotChange) < next.StartTime)
            {
                var newDuration = TimeSpan.FromSeconds(shotChange) - line.StartTime;
                if (newDuration.TotalMilliseconds <= Se.Settings.General.SubtitleMaximumDisplayMilliseconds)
                {
                    line.EndTime = TimeSpan.FromSeconds(shotChange);
                }
            }
            else
            {
                var newDuration = next.StartTime - line.StartTime;
                if (newDuration.TotalMilliseconds <= Se.Settings.General.SubtitleMaximumDisplayMilliseconds)
                {
                    line.EndTime = TimeSpan.FromMilliseconds(next.StartTime.TotalMilliseconds - Se.Settings.General.MinimumBetweenLines.GetMilliseconds());
                }
            }
        }

        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void SnapSelectedLinesToNearestShotChange()
    {
        var selectedLines = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().OrderBy(p => p.StartTime).ToList();
        var vp = GetVideoPlayerControl();
        if (string.IsNullOrEmpty(_videoFileName) ||
            vp == null ||
            AudioVisualizer == null ||
            AudioVisualizer.ShotChanges.Count == 0 ||
            selectedLines.Count == 0)
        {
            return;
        }

        foreach (var line in selectedLines)
        {
            var idx = Subtitles.IndexOf(line);
            var prev = Subtitles.GetOrNull(idx - 1);
            var next = Subtitles.GetOrNull(idx + 1);

            var nearestStartShotChange = AudioVisualizer.ShotChanges
                .OrderBy(s => Math.Abs(s - line.StartTime.TotalSeconds))
                .FirstOrDefault(s => Math.Abs(s - line.StartTime.TotalSeconds) < 1.0); //TODO: customizable

            var nearestEndShotChange = AudioVisualizer.ShotChanges
                .OrderBy(s => Math.Abs(s - line.EndTime.TotalSeconds))
                .FirstOrDefault(s => Math.Abs(s - line.EndTime.TotalSeconds) < 1.5); //TODO: customizable

            if (nearestStartShotChange == 0 && nearestEndShotChange == 0)
            {
                continue;
            }

            if (nearestStartShotChange == 0)
            {
                var newDuration = TimeSpan.FromSeconds(nearestEndShotChange) - line.StartTime;
                if (newDuration.TotalMilliseconds <= Se.Settings.General.SubtitleMaximumDisplayMilliseconds &&
                    newDuration.TotalMilliseconds >= Se.Settings.General.SubtitleMinimumDisplayMilliseconds)
                {
                    line.EndTime = TimeSpan.FromSeconds(nearestEndShotChange);
                }

                continue;
            }

            if (nearestEndShotChange == 0)
            {
                var newDuration = line.EndTime - TimeSpan.FromSeconds(nearestStartShotChange);
                if (newDuration.TotalMilliseconds <= Se.Settings.General.SubtitleMaximumDisplayMilliseconds &&
                    newDuration.TotalMilliseconds >= Se.Settings.General.SubtitleMinimumDisplayMilliseconds)
                {
                    line.StartTime = TimeSpan.FromSeconds(nearestStartShotChange);
                }

                continue;
            }

            if (nearestStartShotChange == nearestEndShotChange)
            {
                nearestEndShotChange = AudioVisualizer.ShotChanges
                    .OrderBy(s => Math.Abs(s - line.EndTime.TotalSeconds))
                    .FirstOrDefault(s => Math.Abs(s - line.EndTime.TotalSeconds) < 0.5); //TODO: customizable

                if (nearestEndShotChange > 0 && nearestEndShotChange > line.StartTime.TotalSeconds)
                {
                    line.EndTime = TimeSpan.FromSeconds(nearestEndShotChange);
                }

                continue;
            }

            var newStartTime = TimeSpan.FromSeconds(nearestStartShotChange);
            var newEndTime = TimeSpan.FromSeconds(nearestEndShotChange);
            var newCombinedDuration = newEndTime - newStartTime;
            if (newCombinedDuration.TotalMilliseconds <= Se.Settings.General.SubtitleMaximumDisplayMilliseconds &&
                newCombinedDuration.TotalMilliseconds >= Se.Settings.General.SubtitleMinimumDisplayMilliseconds)
            {
                line.StartTime = newStartTime;
                line.EndTime = newEndTime;
            }
        }

        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void ExtendSelectedLinesToPreviousShotChange()
    {
        var selectedLines = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().OrderBy(p => p.StartTime).ToList();
        var vp = GetVideoPlayerControl();
        if (string.IsNullOrEmpty(_videoFileName) ||
            vp == null ||
            AudioVisualizer == null ||
            AudioVisualizer.ShotChanges.Count == 0 ||
            selectedLines.Count == 0)
        {
            return;
        }

        var gapMs = Se.Settings.General.MinimumBetweenLines.GetMilliseconds();
        var maxDurationMs = Se.Settings.General.SubtitleMaximumDisplayMilliseconds;
        foreach (var line in selectedLines)
        {
            var idx = Subtitles.IndexOf(line);
            var prev = Subtitles.GetOrNull(idx - 1);
            // Cast to nullable so "no match" is null and a real shot change
            // at t=0 isn't conflated with the default value. Strict `<` so
            // shot changes at or after the current start can't qualify and
            // cause "extend to previous" to actually move the start forward.
            var shotChange = AudioVisualizer.ShotChanges
                .Cast<double?>()
                .LastOrDefault(s => s < line.StartTime.TotalSeconds);

            // Lower bound for the new start: the previous shot change and, if
            // present, the previous subtitle's end plus the configured gap.
            // Whichever is later wins so we never pull the start back past
            // either constraint.
            double? candidateStartMs = shotChange.HasValue ? shotChange.Value * 1000.0 : (double?)null;
            if (prev != null)
            {
                var prevEndPlusGapMs = prev.EndTime.TotalMilliseconds + gapMs;
                candidateStartMs = candidateStartMs.HasValue
                    ? Math.Max(candidateStartMs.Value, prevEndPlusGapMs)
                    : prevEndPlusGapMs;
            }

            if (candidateStartMs == null)
            {
                continue;
            }

            var newStartMs = candidateStartMs.Value;
            var newDurationMs = line.EndTime.TotalMilliseconds - newStartMs;
            if (newDurationMs <= 0 || newDurationMs > maxDurationMs)
            {
                continue;
            }

            // Use SetStartTimeOnly so EndTime stays fixed (the StartTime
            // setter would otherwise shift EndTime to preserve Duration —
            // see SubtitleLineViewModel.OnStartTimeChanged).
            line.SetStartTimeOnly(TimeSpan.FromMilliseconds(newStartMs));
        }

        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void SnapSelectedLinesStartToNextShotChange()
    {
        var selectedLines = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
        if (string.IsNullOrEmpty(_videoFileName) ||
            AudioVisualizer == null ||
            AudioVisualizer.ShotChanges.Count == 0 ||
            selectedLines.Count == 0)
        {
            return;
        }

        var minDurationMs = Se.Settings.General.SubtitleMinimumDisplayMilliseconds;
        var maxDurationMs = Se.Settings.General.SubtitleMaximumDisplayMilliseconds;
        foreach (var line in selectedLines)
        {
            // Cast to nullable so a shot change at t=0 isn't lost to the
            // default-value collision.
            var next = AudioVisualizer.ShotChanges
                .Cast<double?>()
                .FirstOrDefault(s => s > line.StartTime.TotalSeconds + 0.001);
            if (next == null)
            {
                continue;
            }

            var newStart = TimeSpan.FromSeconds(next.Value);
            if (newStart >= line.EndTime)
            {
                continue;
            }

            var newDurationMs = (line.EndTime - newStart).TotalMilliseconds;
            if (newDurationMs < minDurationMs || newDurationMs > maxDurationMs)
            {
                continue;
            }

            // Use SetStartTimeOnly so EndTime stays fixed (the StartTime
            // setter would otherwise shift EndTime to preserve Duration).
            line.SetStartTimeOnly(newStart);
        }

        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void SnapSelectedLinesEndToPreviousShotChange()
    {
        var selectedLines = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
        if (string.IsNullOrEmpty(_videoFileName) ||
            AudioVisualizer == null ||
            AudioVisualizer.ShotChanges.Count == 0 ||
            selectedLines.Count == 0)
        {
            return;
        }

        var minDurationMs = Se.Settings.General.SubtitleMinimumDisplayMilliseconds;
        var maxDurationMs = Se.Settings.General.SubtitleMaximumDisplayMilliseconds;
        foreach (var line in selectedLines)
        {
            // Cast to nullable so a shot change at t=0 isn't lost to the
            // default-value collision.
            var prev = AudioVisualizer.ShotChanges
                .Cast<double?>()
                .LastOrDefault(s => s < line.EndTime.TotalSeconds - 0.001);
            if (prev == null)
            {
                continue;
            }

            var newEnd = TimeSpan.FromSeconds(prev.Value);
            if (newEnd <= line.StartTime)
            {
                continue;
            }

            var newDurationMs = (newEnd - line.StartTime).TotalMilliseconds;
            if (newDurationMs < minDurationMs || newDurationMs > maxDurationMs)
            {
                continue;
            }

            line.EndTime = newEnd;
        }

        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void SetInCueToClosestShotChangeLeftGreenZone() =>
        SetCueToClosestShotChangeGreenZone(isInCue: true, isLeftZone: true,
            Se.Language.General.SetInCueToClosestShotChangeLeftGreenZone);

    [RelayCommand]
    private void SetInCueToClosestShotChangeRightGreenZone() =>
        SetCueToClosestShotChangeGreenZone(isInCue: true, isLeftZone: false,
            Se.Language.General.SetInCueToClosestShotChangeRightGreenZone);

    [RelayCommand]
    private void SetOutCueToClosestShotChangeLeftGreenZone() =>
        SetCueToClosestShotChangeGreenZone(isInCue: false, isLeftZone: true,
            Se.Language.General.SetOutCueToClosestShotChangeLeftGreenZone);

    [RelayCommand]
    private void SetOutCueToClosestShotChangeRightGreenZone() =>
        SetCueToClosestShotChangeGreenZone(isInCue: false, isLeftZone: false,
            Se.Language.General.SetOutCueToClosestShotChangeRightGreenZone);

    // Snaps a cue (in or out) onto the closest shot change, offset by the
    // configured green-zone frame count from BeautifyTimeCodes.Profile.
    // Left zone = cue lands BEFORE the shot change; right zone = AFTER.
    // Mirrors SE 4's push-neighbor behavior: if the new cue would overlap
    // the previous (for in-cue) or next (for out-cue) subtitle's gap, the
    // neighbor is shortened to make room rather than skipping the line.
    // Both subtitles must still end up with positive duration; otherwise
    // the line is skipped.
    private void SetCueToClosestShotChangeGreenZone(bool isInCue, bool isLeftZone, string actionLabel)
    {
        var selectedLines = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>()
            .OrderBy(p => p.StartTime).ToList();
        var vp = GetVideoPlayerControl();
        if (string.IsNullOrEmpty(_videoFileName) ||
            vp == null ||
            AudioVisualizer == null ||
            AudioVisualizer.ShotChanges.Count == 0 ||
            selectedLines.Count == 0)
        {
            return;
        }

        var profile = Configuration.Settings.BeautifyTimeCodes.Profile;
        int zoneFrames;
        if (isInCue)
        {
            zoneFrames = isLeftZone ? profile.InCuesLeftGreenZone : profile.InCuesRightGreenZone;
        }
        else
        {
            zoneFrames = isLeftZone ? profile.OutCuesLeftGreenZone : profile.OutCuesRightGreenZone;
        }

        // Fall back to the user-configured frame rate when ffmpeg parsing
        // didn't produce a usable rate (e.g., ffmpeg missing → FramesRate is 0,
        // which would make FramesToMilliseconds divide by zero).
        var frameRate = _mediaInfo != null && _mediaInfo.FramesRateNonNormalized > 0
            ? (double)_mediaInfo.FramesRateNonNormalized
            : Se.Settings.General.CurrentFrameRate;
        var zoneMs = SubtitleFormat.FramesToMilliseconds(zoneFrames, frameRate);
        var gapMs = Se.Settings.General.MinimumBetweenLines.GetMilliseconds();

        var selectedSet = new HashSet<SubtitleLineViewModel>(selectedLines);
        var adjustedNeighbors = new HashSet<SubtitleLineViewModel>();
        var changed = 0;
        foreach (var line in selectedLines)
        {
            var idx = Subtitles.IndexOf(line);
            var originalCueMs = isInCue ? line.StartTime.TotalMilliseconds : line.EndTime.TotalMilliseconds;
            var cue = isInCue ? line.StartTime : line.EndTime;
            var closestShotChange = ShotChangeHelper.GetClosestShotChange(AudioVisualizer.ShotChanges, new TimeCode(cue));
            if (closestShotChange == null)
            {
                continue;
            }

            var shotChangeMs = closestShotChange.Value * 1000.0;

            if (isInCue)
            {
                var newInCueMs = isLeftZone ? shotChangeMs - zoneMs : shotChangeMs + zoneMs;
                if (newInCueMs < 0 || newInCueMs >= line.EndTime.TotalMilliseconds)
                {
                    continue;
                }

                var newStartMs = newInCueMs;
                var prev = Subtitles.GetOrNull(idx - 1);
                if (prev != null)
                {
                    double newPreviousEndMs;
                    if (isLeftZone)
                    {
                        // Cue lands before the shot change — keep previous
                        // where it was unless it crosses into our gap.
                        newPreviousEndMs = Math.Min(prev.EndTime.TotalMilliseconds, newStartMs - gapMs);
                    }
                    else
                    {
                        // Cue lands after the shot change — cap previous at
                        // the green-zone in-cue position (matching SE 4), then
                        // push the new start later if shortening didn't free
                        // up enough room for the gap.
                        newPreviousEndMs = Math.Min(newInCueMs, prev.EndTime.TotalMilliseconds);
                        newStartMs = Math.Max(newInCueMs, newPreviousEndMs + gapMs);
                    }

                    // Both subtitles must still have positive duration.
                    if (newPreviousEndMs - prev.StartTime.TotalMilliseconds <= 0 ||
                        line.EndTime.TotalMilliseconds - newStartMs <= 0)
                    {
                        continue;
                    }

                    // Use SetStartTimeOnly so the line's EndTime stays put
                    // (the StartTime setter would otherwise shift EndTime to
                    // preserve Duration — see OnStartTimeChanged).
                    line.SetStartTimeOnly(TimeSpan.FromMilliseconds(newStartMs));
                    if (Math.Abs(newPreviousEndMs - prev.EndTime.TotalMilliseconds) > 0.5)
                    {
                        prev.EndTime = TimeSpan.FromMilliseconds(newPreviousEndMs);
                        if (!selectedSet.Contains(prev))
                        {
                            adjustedNeighbors.Add(prev);
                        }
                    }
                }
                else
                {
                    line.SetStartTimeOnly(TimeSpan.FromMilliseconds(newStartMs));
                }
            }
            else
            {
                var newOutCueMs = isLeftZone ? shotChangeMs - zoneMs : shotChangeMs + zoneMs;
                if (newOutCueMs <= line.StartTime.TotalMilliseconds)
                {
                    continue;
                }

                var newEndMs = newOutCueMs;
                var next = Subtitles.GetOrNull(idx + 1);
                if (next != null)
                {
                    double newNextStartMs;
                    if (!isLeftZone)
                    {
                        // Cue lands after the shot change — keep next where
                        // it was unless it crosses into our gap.
                        newNextStartMs = Math.Max(next.StartTime.TotalMilliseconds, newEndMs + gapMs);
                    }
                    else
                    {
                        // Cue lands before the shot change — cap next at
                        // the green-zone out-cue position (matching SE 4),
                        // then pull the new end earlier if shortening didn't
                        // free up enough room for the gap.
                        newNextStartMs = Math.Max(next.StartTime.TotalMilliseconds, newOutCueMs);
                        newEndMs = Math.Min(newNextStartMs - gapMs, newOutCueMs);
                    }

                    // Both subtitles must still have positive duration.
                    if (next.EndTime.TotalMilliseconds - newNextStartMs <= 0 ||
                        newEndMs - line.StartTime.TotalMilliseconds <= 0)
                    {
                        continue;
                    }

                    line.EndTime = TimeSpan.FromMilliseconds(newEndMs);
                    if (Math.Abs(newNextStartMs - next.StartTime.TotalMilliseconds) > 0.5)
                    {
                        // SetStartTimeOnly so the next subtitle's EndTime
                        // isn't shifted to preserve its previous Duration.
                        next.SetStartTimeOnly(TimeSpan.FromMilliseconds(newNextStartMs));
                        if (!selectedSet.Contains(next))
                        {
                            adjustedNeighbors.Add(next);
                        }
                    }
                }
                else
                {
                    line.EndTime = TimeSpan.FromMilliseconds(newEndMs);
                }
            }

            // Only count this line as "updated" if its cue actually moved —
            // re-running the command on already-snapped lines should report 0.
            var newCueMs = isInCue ? line.StartTime.TotalMilliseconds : line.EndTime.TotalMilliseconds;
            if (Math.Abs(newCueMs - originalCueMs) > 0.5)
            {
                changed++;
            }
        }

        var neighborsAdjusted = adjustedNeighbors.Count;
        if (changed > 0 || neighborsAdjusted > 0)
        {
            var statusMessage = neighborsAdjusted > 0
                ? string.Format(Se.Language.General.XOfYLinesUpdatedAndZNeighborsAdjusted, actionLabel, changed, selectedLines.Count, neighborsAdjusted)
                : string.Format(Se.Language.General.XOfYLinesUpdated, actionLabel, changed, selectedLines.Count);
            ShowStatus(statusMessage);
            _updateAudioVisualizer = true;
        }
        else
        {
            ShowStatus(string.Format(Se.Language.General.XNoLinesUpdated, actionLabel));
        }
    }

    [RelayCommand]
    private void MoveAllShotChangeOneFrameBack()
    {
        if (AudioVisualizer == null || AudioVisualizer.ShotChanges.Count == 0)
        {
            return;
        }

        var frameRate = _mediaInfo != null ? (double)_mediaInfo.FramesRateNonNormalized : Se.Settings.General.CurrentFrameRate;
        var milliseconds = 1000.0 / frameRate;
        var seconds = milliseconds / 1000.0;

        var newShotChanges = new List<double>();
        foreach (var shotChange in AudioVisualizer.ShotChanges)
        {
            var newTime = shotChange - seconds;
            if (newTime < 0)
            {
                continue;
            }

            newShotChanges.Add(newTime);
        }

        AudioVisualizer.ShotChanges = newShotChanges;

        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void MoveAllShotChangeOneFrameForward()
    {
        if (AudioVisualizer == null || AudioVisualizer.ShotChanges.Count == 0)
        {
            return;
        }

        var frameRate = _mediaInfo != null ? (double)_mediaInfo.FramesRateNonNormalized : Se.Settings.General.CurrentFrameRate;
        var milliseconds = 1000.0 / frameRate;
        var seconds = milliseconds / 1000.0;

        var newShotChanges = new List<double>();
        foreach (var shotChange in AudioVisualizer.ShotChanges)
        {
            var newTime = shotChange + seconds;
            newShotChanges.Add(newTime);
        }

        AudioVisualizer.ShotChanges = newShotChanges;

        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void ShowSyncAdjustAllTimes()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }


        if (_adjustAllTimesViewModel != null && _adjustAllTimesViewModel.Window != null && _adjustAllTimesViewModel.Window.IsVisible)
        {
            _adjustAllTimesViewModel.Window.Activate();
            return;
        }

        var result = _windowService.ShowWindow<AdjustAllTimesWindow, AdjustAllTimesViewModel>(Window, (window, vm) =>
        {
            _adjustAllTimesViewModel = vm;
            var selectedCount = SubtitleGrid.SelectedItems.Count;
            vm.Initialize(this, selectedCount); // uses call from IAdjustCallback: Adjust
        });
    }

    [RelayCommand]
    private async Task ShowVisualSync()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        var result = await ShowDialogAsync<VisualSyncWindow, VisualSyncViewModel>(vm =>
        {
            var paragraphs = Subtitles.Select(p => new SubtitleLineViewModel(p)).ToList();
            vm.Initialize(paragraphs, _videoFileName, _subtitleFileName, AudioVisualizer);
        });

        if (result.OkPressed)
        {
            Subtitles.Clear();
            foreach (var p in result.Paragraphs)
            {
                Subtitles.Add(p.Subtitle);
            }

            SelectAndScrollToRow(0);
        }
    }

    [RelayCommand]
    private async Task ShowSyncChangeFrameRate()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        var result = await ShowDialogAsync<ChangeFrameRateWindow, ChangeFrameRateViewModel>(vm => { vm.Initialize(_videoFileName, _mediaInfo); });
        if (result.OkPressed)
        {
            ChangeFrameRateViewModel.ChangeFrameRate(Subtitles, result.SelectedFromFrameRate, result.SelectedToFrameRate);
            _updateAudioVisualizer = true;
        }
    }

    [RelayCommand]
    private async Task ShowSyncChangeSpeed()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        var result = await ShowDialogAsync<ChangeSpeedWindow, ChangeSpeedViewModel>(vm => { vm.Initialize(Subtitles); });
        if (result.OkPressed)
        {
            ChangeSpeedViewModel.ChangeSpeed(Subtitles, result.SpeedPercent);
            _updateAudioVisualizer = true;
        }
    }

    [RelayCommand]
    private async Task ShowPointSync()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        var result = await ShowDialogAsync<PointSyncWindow, PointSyncViewModel>(vm =>
        {
            var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
            var paragraphs = Subtitles.Select(p => new SubtitleLineViewModel(p)).ToList();
            vm.Initialize(paragraphs, selectedItems, _videoFileName ?? string.Empty, _subtitleFileName ?? string.Empty, AudioVisualizer);
        });

        if (result.OkPressed)
        {
            Subtitles.Clear();
            foreach (var p in result.SyncedSubtitles)
            {
                Subtitles.Add(p);
            }

            SelectAndScrollToRow(0);
        }
    }

    [RelayCommand]
    private async Task ShowPointSyncViaOther()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        var result = await ShowDialogAsync<PointSyncViaOtherWindow, PointSyncViaOtherViewModel>(vm =>
        {
            var paragraphs = Subtitles.Select(p => new SubtitleLineViewModel(p)).ToList();
            vm.Initialize(paragraphs, _videoFileName ?? string.Empty, _subtitleFileName ?? string.Empty);
        });

        if (result.OkPressed)
        {
            Subtitles.Clear();
            foreach (var p in result.SyncedSubtitles)
            {
                Subtitles.Add(p);
            }

            SelectAndScrollToRow(0);
        }
    }

    [RelayCommand]
    private async Task ShowAutoTranslate()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        var result = await ShowDialogAsync<AutoTranslateWindow, AutoTranslateViewModel>(vm => { vm.Initialize(GetUpdateSubtitle()); });

        if (!result.OkPressed)
        {
            return;
        }

        var wasOldTranslationChanged = _changeSubtitleHash != GetFastHash();

        for (var i = 0; i < Subtitles.Count; i++)
        {
            if (result.Rows.Count <= i)
            {
                break;
            }

            Subtitles[i].OriginalText = Subtitles[i].Text;
            Subtitles[i].Text = result.Rows[i].TranslatedText;
        }

        if (!wasOldTranslationChanged)
        {
            _changeSubtitleHashOriginal = GetFastHashOriginal();
        }

        var targetLanguageCode = result.SelectedTargetLanguage?.TwoLetterIsoLanguageName;
        _subtitleFileNameOriginal = _subtitleFileName;
        _subtitleOriginal ??= new Subtitle();
        _subtitleOriginal.OriginalFormat = _subtitle.OriginalFormat ?? SelectedSubtitleFormat;
        if (!string.IsNullOrEmpty(_subtitleFileName) && !string.IsNullOrEmpty(targetLanguageCode))
        {
            var directory = Path.GetDirectoryName(_subtitleFileName) ?? string.Empty;
            var fileName = Path.GetFileName(_subtitleFileName);

            var nameWithoutExt = fileName;
            var extension = string.Empty;
            foreach (var ext in SubtitleFormats.Select(f => f.Extension))
            {
                if (fileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                {
                    nameWithoutExt = fileName.Substring(0, fileName.Length - ext.Length);
                    extension = ext;
                    break;
                }
            }

            var lastDot = nameWithoutExt.LastIndexOf('.');
            if (lastDot > 0)
            {
                var possibleCode = nameWithoutExt.Substring(lastDot + 1);
                if ((possibleCode.Length == 2 || possibleCode.Length == 3) &&
                    Iso639Dash2LanguageCode.List.Any(l =>
                        possibleCode.Equals(l.TwoLetterCode, StringComparison.OrdinalIgnoreCase) ||
                        possibleCode.Equals(l.ThreeLetterCode, StringComparison.OrdinalIgnoreCase)))
                {
                    nameWithoutExt = nameWithoutExt.Substring(0, lastDot);
                }
            }

            _subtitleFileName = Path.Combine(directory, nameWithoutExt + "." + targetLanguageCode + extension);
        }
        else
        {
            _subtitleFileName = string.Empty;
        }

        _converted = true;
        ShowColumnOriginalText = true;
        AutoFitColumns();
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private async Task AutoTranslateSelectedLines()
    {
        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
        if (selectedItems.Count == 0 || !ShowColumnOriginalText)
        {
            return;
        }

        var result = await ShowDialogAsync<AutoTranslateWindow, AutoTranslateViewModel>(vm =>
        {
            var sub = new Subtitle();
            foreach (var line in selectedItems)
            {
                var p = new Paragraph()
                {
                    Number = line.Number,
                    StartTime = new TimeCode(line.StartTime),
                    EndTime = new TimeCode(line.EndTime),
                    Text = line.OriginalText,
                    Actor = line.Actor,
                    Style = line.Style,
                    Language = line.Language,
                    Region = line.Region,
                    Layer = line.Layer,
                    Bookmark = line.Bookmark,
                };
                sub.Paragraphs.Add(p);
            }

            vm.Initialize(sub);
        });

        if (!result.OkPressed)
        {
            _shortcutManager.ClearKeys();
            return;
        }

        for (int i = 0; i < result.Rows.Count; i++)
        {
            var translatedText = result.Rows[i].TranslatedText;
            var id = selectedItems[i].Id;
            var p = Subtitles.FirstOrDefault(x => x.Id == id);
            if (p != null && !string.IsNullOrEmpty(translatedText))
            {
                p.Text = translatedText;
            }
        }

        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private async Task ShowTranslateViaCopyPaste()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        var result = await ShowDialogAsync<CopyPasteTranslateWindow, CopyPasteTranslateViewModel>(vm => { vm.Initialize(Subtitles.ToList()); });

        if (!result.OkPressed)
        {
            return;
        }

        for (var i = 0; i < Subtitles.Count; i++)
        {
            if (result.Rows.Count <= i)
            {
                break;
            }

            Subtitles[i].OriginalText = Subtitles[i].Text;
            Subtitles[i].Text = result.Rows[i].TranslatedText;
        }

        _subtitleFileNameOriginal = _subtitleFileName;
        _subtitleOriginal ??= new Subtitle();
        _subtitleOriginal.OriginalFormat = _subtitle.OriginalFormat ?? SelectedSubtitleFormat;
        _subtitleFileName = string.Empty;
        ShowColumnOriginalText = true;
        AutoFitColumns();
        _updateAudioVisualizer = true;
        _converted = true;
    }

    [RelayCommand]
    private async Task ChangeCasingSelectedLines()
    {
        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
        if (selectedItems.Count == 0)
        {
            return;
        }

        var result = await ShowDialogAsync<ChangeCasingWindow, ChangeCasingViewModel>(vm =>
        {
            var sub = new Subtitle();
            foreach (var line in selectedItems)
            {
                var p = new Paragraph()
                {
                    Number = line.Number,
                    StartTime = new TimeCode(line.StartTime),
                    EndTime = new TimeCode(line.EndTime),
                    Text = line.Text,
                    Actor = line.Actor,
                    Style = line.Style,
                    Language = line.Language,
                    Region = line.Region,
                    Layer = line.Layer,
                    Bookmark = line.Bookmark,
                };
                sub.Paragraphs.Add(p);
            }

            vm.Initialize(sub);
        });

        if (!result.OkPressed)
        {
            _shortcutManager.ClearKeys();
            return;
        }

        for (var i = 0; i < result.Subtitle.Paragraphs.Count; i++)
        {
            var text = result.Subtitle.Paragraphs[i].Text;
            var id = selectedItems[i].Id;
            var p = Subtitles.FirstOrDefault(x => x.Id == id);
            if (p != null)
            {
                p.Text = text;
            }
        }

        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private async Task StatisticsSelectedLines()
    {
        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
        if (selectedItems.Count == 0)
        {
            return;
        }

        var result = await ShowDialogAsync<StatisticsWindow, StatisticsViewModel>(vm =>
        {
            var sub = new Subtitle();
            foreach (var line in selectedItems)
            {
                var p = new Paragraph()
                {
                    Number = line.Number,
                    StartTime = new TimeCode(line.StartTime),
                    EndTime = new TimeCode(line.EndTime),
                    Text = line.Text,
                    Actor = line.Actor,
                    Style = line.Style,
                    Language = line.Language,
                    Region = line.Region,
                    Layer = line.Layer,
                    Bookmark = line.Bookmark,
                };
                sub.Paragraphs.Add(p);
            }

            vm.Initialize(sub, SelectedSubtitleFormat, _subtitleFileName ?? string.Empty);
        });
    }

    [RelayCommand]
    private async Task FillSelectedLinesWithClipboard()
    {
        if (Window == null)
        {
            return;
        }

        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
        if (selectedItems.Count < 2)
        {
            return;
        }

        var text = await ClipboardHelper.GetTextAsync(Window);
        if (string.IsNullOrEmpty(text))
        {
            ShowStatus(Se.Language.Main.NoTextInClipboard);
            _shortcutManager.ClearKeys();
            return;
        }

        foreach (var line in selectedItems)
        {
            var p = Subtitles.FirstOrDefault(x => x.Id == line.Id);
            if (p != null)
            {
                p.Text = text;
            }
        }

        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private async Task MultipleReplaceSelectedLines()
    {
        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
        if (selectedItems.Count == 0)
        {
            return;
        }

        var result = await ShowDialogAsync<MultipleReplaceWindow, MultipleReplaceViewModel>(vm =>
        {
            var sub = new Subtitle();
            foreach (var line in selectedItems)
            {
                var p = new Paragraph()
                {
                    Number = line.Number,
                    StartTime = new TimeCode(line.StartTime),
                    EndTime = new TimeCode(line.EndTime),
                    Text = line.Text,
                    Actor = line.Actor,
                    Style = line.Style,
                    Language = line.Language,
                    Region = line.Region,
                    Layer = line.Layer,
                    Bookmark = line.Bookmark,
                };
                sub.Paragraphs.Add(p);
            }

            vm.Initialize(sub);
        });

        if (!result.OkPressed)
        {
            _shortcutManager.ClearKeys();
            return;
        }

        for (var i = 0; i < result.FixedSubtitle.Paragraphs.Count; i++)
        {
            var text = result.FixedSubtitle.Paragraphs[i].Text;
            var id = selectedItems[i].Id;
            var p = Subtitles.FirstOrDefault(x => x.Id == id);
            if (p != null)
            {
                p.Text = text;
            }
        }

        _updateAudioVisualizer = true;
    }


    [RelayCommand]
    private async Task ShowBeautifyTimeCodes()
    {
        if (Window == null || AudioVisualizer == null || string.IsNullOrEmpty(_videoFileName))
        {
            return;
        }

        var viewModel = await ShowDialogAsync<BeautifyTimeCodesWindow, BeautifyTimeCodesViewModel>(
            vm => { vm.Initialize(Subtitles.ToList(), AudioVisualizer, _videoFileName); });

        if (!viewModel.OkPressed)
        {
            return;
        }
    }

    [RelayCommand]
    private async Task FixCommonErrorsSelectedLines()
    {
        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
        if (selectedItems.Count == 0)
        {
            return;
        }

        var result = await ShowDialogAsync<FixCommonErrorsWindow, FixCommonErrorsViewModel>(vm =>
        {
            var sub = new Subtitle();
            foreach (var line in selectedItems)
            {
                var p = new Paragraph()
                {
                    Number = line.Number,
                    StartTime = new TimeCode(line.StartTime),
                    EndTime = new TimeCode(line.EndTime),
                    Text = line.Text,
                    Actor = line.Actor,
                    Style = line.Style,
                    Language = line.Language,
                    Region = line.Region,
                    Layer = line.Layer,
                    Bookmark = line.Bookmark,
                };
                sub.Paragraphs.Add(p);
            }

            vm.Initialize(sub, SelectedSubtitleFormat);
        });

        if (!result.OkPressed)
        {
            _shortcutManager.ClearKeys();
            return;
        }

        for (var i = 0; i < result.FixedSubtitle.Paragraphs.Count; i++)
        {
            var text = result.FixedSubtitle.Paragraphs[i].Text;
            var id = selectedItems[i].Id;
            var p = Subtitles.FirstOrDefault(x => x.Id == id);
            if (p != null)
            {
                p.Text = text;
            }
        }

        _updateAudioVisualizer = true;
    }

    private DataGrid _oldSubtitleGrid = new DataGrid();
    private ITextBoxWrapper _oldEditTextBox = new TextBoxWrapper(new TextBox());
    private bool _oldGenerateSpectrogram;
    private string _oldSpectrogramStyle = string.Empty;

    [RelayCommand]
    private async Task CommandShowSettings()
    {
        _oldSubtitleGrid = SubtitleGrid;
        _oldEditTextBox = EditTextBox;
        _oldGenerateSpectrogram = Se.Settings.Waveform.GenerateSpectrogram;
        _oldSpectrogramStyle = Se.Settings.Waveform.SpectrogramStyle;

        Se.Settings.General.WindowPositions = Se.Settings.General.WindowPositions.OrderBy(p => p.WindowName).ToList();
        var oldSettngsSerialized = JsonSerializer.Serialize(Se.Settings);

        var viewModel = await ShowDialogAsync<SettingsWindow, SettingsViewModel>(
            vm => { vm.Initialize(this); });

        if (!viewModel.OkPressed)
        {
            return;
        }

        Se.Settings.General.WindowPositions = Se.Settings.General.WindowPositions.OrderBy(p => p.WindowName).ToList();
        var newSettingsSerialized = JsonSerializer.Serialize(Se.Settings);

        if (oldSettngsSerialized != newSettingsSerialized)
        {
            var firstSelectedIndex = SubtitleGrid.SelectedIndex;

            // Apply video-player visibility toggles directly on the existing
            // VideoPlayerControl. StopIsVisible / FullScreenIsVisible are plain
            // Avalonia styled properties, so writing to them updates the button
            // bar in place — no new control, no new native HWND, no layout
            // rebuild. The full ApplySettings path below only runs when
            // *other* settings changed, which is what avoided #10815 in beta 26
            // via the Dispatcher.Post defer but apparently still races the
            // modal teardown on some Windows configurations (re-reported in
            // beta 30, comment 4526345049 on the issue). Removing the trigger
            // entirely for these toggles is more robust than tightening the
            // defer priority.
            var vp = GetVideoPlayerControl();
            if (vp != null)
            {
                vp.StopIsVisible = Se.Settings.Video.ShowStopButton;
                vp.FullScreenIsVisible = Se.Settings.Video.ShowFullscreenButton;
            }

            if (OnlyVideoPlayerVisibilityFlagsChanged(oldSettngsSerialized, newSettingsSerialized))
            {
                return;
            }

            // Defer to a fresh dispatcher cycle: ApplySettings rebuilds the layout, which
            // destroys and recreates the video player control (a native Win32 HWND on Windows
            // with mpv-wid/VLC). Doing that inside the ShowDialog continuation races the modal
            // dialog's teardown and can leave the main window disabled - a full UI freeze (#10815).
            Dispatcher.UIThread.Post(() =>
            {
                ApplySettings();
                SelectAndScrollToRow(firstSelectedIndex);
            });
        }
    }

    /// <summary>
    /// True iff the two serialized <see cref="Se.Settings"/> snapshots differ
    /// only in <c>Video.ShowStopButton</c> and/or <c>Video.ShowFullscreenButton</c>.
    /// Callers use this to skip the heavyweight <see cref="ApplySettings"/>
    /// path (which rebuilds the entire layout and recreates the video player's
    /// native HWND) when the only changes are visibility flags that can be
    /// applied in place on the existing <see cref="VideoPlayerControl"/>.
    /// Conservative on parse failure: returns false so the caller still
    /// performs the full rebuild.
    /// </summary>
    private static bool OnlyVideoPlayerVisibilityFlagsChanged(string oldJson, string newJson)
    {
        try
        {
            var oldNode = JsonNode.Parse(oldJson);
            var newNode = JsonNode.Parse(newJson);
            if (oldNode is null || newNode is null)
            {
                return false;
            }

            // Mask both flag fields to the same canonical value in both trees.
            // If the trees are equal after masking, those flags were the only
            // differences. The fields may legitimately be absent from a JSON
            // snapshot (older settings file), in which case the masking is a
            // no-op and the comparison is still correct.
            MaskVideoVisibilityFlags(oldNode);
            MaskVideoVisibilityFlags(newNode);

            return string.Equals(oldNode.ToJsonString(), newNode.ToJsonString(), StringComparison.Ordinal);
        }
        catch
        {
            return false;
        }
    }

    private static void MaskVideoVisibilityFlags(JsonNode root)
    {
        if (root is JsonObject rootObj
            && rootObj.TryGetPropertyValue("Video", out var videoNode)
            && videoNode is JsonObject videoObj)
        {
            videoObj["ShowStopButton"] = false;
            videoObj["ShowFullscreenButton"] = false;
        }
    }

    public void ApplySettings()
    {
        UiUtil.SetFontName(Se.Settings.Appearance.FontName);
        UiTheme.SetCurrentTheme();

        if (ToolbarTopSeparator != null)
        {
            ToolbarTopSeparator.IsVisible = Se.Settings.Appearance.ShowHorizontalLineAboveToolbar;
        }

        if (Toolbar is Border toolbarBorder)
        {
            var tb = InitToolbar.Make(this);
            if (tb is Border newToolbarBorder)
            {
                var grid = newToolbarBorder.Child;
                newToolbarBorder.Child = null;
                toolbarBorder.Child = grid;
            }
        }

        MenuPlugins.IsVisible = Se.Settings.Appearance.ShowPluginsMenu;

        LockTimeCodes = Se.Settings.General.LockTimeCodes;
        IsWaveformToolbarVisible = Se.Settings.Waveform.ShowToolbar;

        if (AudioVisualizer != null)
        {
            AudioVisualizer.DrawGridLines = Se.Settings.Waveform.DrawGridLines;
            AudioVisualizer.WaveformColor = Se.Settings.Waveform.WaveformColor.FromHexToColor();
            AudioVisualizer.WaveformBackgroundColor = Se.Settings.Waveform.WaveformBackgroundColor.FromHexToColor();
            AudioVisualizer.WaveformSelectedColor = Se.Settings.Waveform.WaveformSelectedColor.FromHexToColor();
            AudioVisualizer.WaveformCursorColor = Se.Settings.Waveform.WaveformCursorColor.FromHexToColor();
            AudioVisualizer.WaveformShotChangeColor = Se.Settings.Waveform.WaveformShotChangeColor.FromHexToColor();
            AudioVisualizer.WaveformParagraphLeftColor = Se.Settings.Waveform.WaveformParagraphLeftColor.FromHexToColor();
            AudioVisualizer.WaveformParagraphRightColor = Se.Settings.Waveform.WaveformParagraphRightColor.FromHexToColor();
            AudioVisualizer.WaveformFancyHighColor = Se.Settings.Waveform.WaveformFancyHighColor.FromHexToColor();
            AudioVisualizer.ParagraphBackground = Se.Settings.Waveform.ParagraphBackground.FromHexToColor();
            AudioVisualizer.ParagraphSelectedBackground = Se.Settings.Waveform.ParagraphSelectedBackground.FromHexToColor();
            AudioVisualizer.InvertMouseWheel = Se.Settings.Waveform.InvertMouseWheel;
            AudioVisualizer.UpdateTheme();
            AudioVisualizer.IsReadOnly = LockTimeCodes;
            AudioVisualizer.WaveformDrawStyle = InitWaveform.GetWaveformDrawStyle(Se.Settings.Waveform.WaveformDrawStyle);
            AudioVisualizer.MinGapSeconds = Se.Settings.General.MinimumBetweenLines.GetMilliseconds() / 1000.0;
            AudioVisualizer.WaveformHeightPercentage = Se.Settings.Waveform.SpectrogramCombinedWaveformHeight;
            AudioVisualizer.FocusOnMouseOver = Se.Settings.Waveform.FocusOnMouseOver;
            AudioVisualizer.ResetCache();

            InitializeLibMpv();
            InitializeFfmpeg();
            LoadShortcuts();

            if (!string.IsNullOrEmpty(_videoFileName))
            {
                if (_oldGenerateSpectrogram == false && Se.Settings.Waveform.GenerateSpectrogram ||
                    _oldSpectrogramStyle != Se.Settings.Waveform.SpectrogramStyle)
                {
                    SettingsViewModel.DeleteWaveformAndSpectrogramFiles();

                    var peakWaveFileName = WavePeakGenerator2.GetPeakWaveFileName(_videoFileName, _audioTrack?.FfIndex ?? -1);
                    var spectrogramFileName = WavePeakGenerator2.SpectrogramDrawer.GetSpectrogramFileName(_videoFileName, _audioTrack?.FfIndex ?? -1);
                    if (!File.Exists(peakWaveFileName))
                    {
                        if (FfmpegHelper.IsFfmpegInstalled())
                        {
                            var tempWaveFileName = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.wav");
                            var process = WaveFileExtractor.GetCommandLineProcess(_videoFileName, _audioTrack?.FfIndex ?? -1, tempWaveFileName,
                                Configuration.Settings.General.VlcWaveTranscodeSettings, out _);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                            Task.Run(async () =>
                            {
                                await ExtractWaveformAndSpectrogramAndShotChanges(process, tempWaveFileName, peakWaveFileName, spectrogramFileName, _videoFileName);
                            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        }
                    }

                    if (_oldGenerateSpectrogram == false && Se.Settings.Waveform.GenerateSpectrogram)
                    {
                        Se.Settings.Waveform.LastDisplayMode = WaveformDisplayMode.WaveformAndSpectrogram.ToString();
                        AudioVisualizer?.SetDisplayMode(WaveformDisplayMode.WaveformAndSpectrogram);
                    }
                }
                else if (_oldGenerateSpectrogram && !Se.Settings.Waveform.GenerateSpectrogram)
                {
                    Se.Settings.Waveform.LastDisplayMode = WaveformDisplayMode.OnlyWaveform.ToString();
                    AudioVisualizer?.SetSpectrogram(null);
                    AudioVisualizer?.SetDisplayMode(WaveformDisplayMode.OnlyWaveform);
                }

                _oldGenerateSpectrogram = Se.Settings.Waveform.GenerateSpectrogram;
                _oldSpectrogramStyle = Se.Settings.Waveform.SpectrogramStyle;
            }
        }

        ShowUpDownStartTime = Se.Settings.Appearance.ShowUpDownStartTime;
        ShowUpDownEndTime = Se.Settings.Appearance.ShowUpDownEndTime;
        ShowUpDownDuration = Se.Settings.Appearance.ShowUpDownDuration;
        ShowUpDownLabels = Se.Settings.Appearance.ShowUpDownLabels;

        _errorColor = Se.Settings.General.ErrorColor.FromHexToColor();
        _errorBrush = new SolidColorBrush(_errorColor);
        if (AreVideoControlsUndocked)
        {
            VideoUndockControls();
        }
        else
        {
            Se.Settings.Appearance.CurrentLayoutPositions = InitLayout.SaveLayoutPositions(ContentGrid.Children.FirstOrDefault() as Grid);
            SetLayout(Se.Settings.General.LayoutNumber);
            InitLayout.RestoreLayoutPositions(Se.Settings.Appearance.CurrentLayoutPositions, ContentGrid.Children.FirstOrDefault() as Grid);
        }

        _autoBackupService.StopAutobackup();
        _autoBackupService.StartAutoBackup(this);

        _updateAudioVisualizer = true;

        _oldSubtitleGrid = SubtitleGrid;
        _oldEditTextBox = EditTextBox;

        var vp = GetVideoPlayerControl();
        if (vp != null)
        {
            if (vp.VideoPlayer is LibMpvDynamicPlayer mpv)
            {
                _mpvReloader.Reset();
                _mpvReloader.RefreshMpv(mpv, GetUpdateSubtitle(), _subtitleSecondary, SelectedSubtitleFormat);
            }
            else if (vp.VideoPlayer is LibVlcDynamicPlayer vlc)
            {
                _vlcReloader.Reset();
                _vlcReloader.RefreshVlc(vlc, GetUpdateSubtitle(), _subtitleSecondary, SelectedSubtitleFormat);
            }
        }

        if (Se.Settings.Appearance.RightToLeft)
        {
            Se.Settings.Appearance.RightToLeft = !Se.Settings.Appearance.RightToLeft;
            RightToLeftToggle();
        }

        UpdateVideoOffsetStatus();
        SetLibSeSettings();
        SubtitleLineViewModel.ErrorColor = Se.Settings.General.ErrorColor.FromHexToColor();

        var selectedSubtitleFormatName = SelectedSubtitleFormat.Name;
        SubtitleFormats.Clear();
        foreach (var format in SubtitleFormatHelper.GetSubtitleFormatsWithFavoritesAtTop())
        {
            SubtitleFormats.Add(format);
        }

        SetSubtitleFormat(SubtitleFormats.FirstOrDefault(p => p.Name == selectedSubtitleFormatName) ?? SubtitleFormats.First());

        SetupLiveSpellCheck();

        if (Subtitles.Count == 0 && string.IsNullOrEmpty(_subtitleFileName))
        {
            SelectedSubtitleFormat = SubtitleFormats.FirstOrDefault(p =>
                p.FriendlyName == Se.Settings.General.DefaultSubtitleFormat ||
                p.Name == Se.Settings.General.DefaultSubtitleFormat) ?? SubtitleFormats[0];
        }

        RefreshSubtitlePreview();
    }

    public VideoPlayerControl? GetVideoPlayerControl()
    {
        if (_fullScreenVideoPlayerControl != null)
        {
            return _fullScreenVideoPlayerControl;
        }
        else if (AreVideoControlsUndocked)
        {
            return _videoPlayerUndockedViewModel?.VideoPlayerControl;
        }
        else
        {
            return VideoPlayerControl;
        }
    }

    [RelayCommand]
    private async Task CommandShowSettingsShortcuts()
    {
        await ShowDialogAsync<ShortcutsWindow, ShortcutsViewModel>(vm => { vm.LoadShortCuts(this); });
        ReloadShortcuts();
        if (OperatingSystem.IsMacOS())
            Layout.InitNativeMacMenu.UpdateShortcuts(this);
    }

    [RelayCommand]
    private async Task ShowWordLists()
    {
        var result = await ShowDialogAsync<WordListsWindow, WordListsViewModel>();
    }

    [RelayCommand]
    private async Task AddOrEditBookmark()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
        if (selectedItems.Count == 0)
        {
            return;
        }

        var result = await ShowDialogAsync<BookmarkEditWindow, BookmarkEditViewModel>(vm =>
        {
            var allBookmarks = Subtitles.Where(p => p.Bookmark != null).ToList();
            vm.Initialize(selectedItems, allBookmarks);
        });

        if (result.OkPressed)
        {
            foreach (var item in selectedItems)
            {
                item.Bookmark = result.BookmarkText;
            }
        }

        new BookmarkPersistence(GetUpdateSubtitle(), _subtitleFileName).Save();

        if (result.ListPressed)
        {
            await ListBookmarks();
        }

        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void ToggleBookmarkSelectedLinesNoText()
    {
        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
        foreach (var item in selectedItems)
        {
            if (item.Bookmark == null)
            {
                item.Bookmark = string.Empty;
            }
            else
            {
                item.Bookmark = null;
            }
        }

        new BookmarkPersistence(GetUpdateSubtitle(), _subtitleFileName).Save();

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ListBookmarks()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        var selected = SelectedSubtitle;
        if (selected == null)
        {
            return;
        }

        var result = await ShowDialogAsync<BookmarksListWindow, BookmarksListViewModel>(vm => { vm.Initialize(Subtitles.Where(p => p.Bookmark != null).ToList()); });

        new BookmarkPersistence(GetUpdateSubtitle(), _subtitleFileName).Save();

        if (result.GoToPressed && result.SelectedSubtitle != null)
        {
            SelectAndScrollToSubtitle(result.SelectedSubtitle);
        }
    }

    [RelayCommand]
    private void RemoveBookmarkSelectedLines()
    {
        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
        foreach (var item in selectedItems)
        {
            item.Bookmark = null;
        }

        new BookmarkPersistence(GetUpdateSubtitle(), _subtitleFileName).Save();

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private void GoToNextBookmark()
    {
        var selected = SelectedSubtitle;
        if (selected == null)
        {
            return;
        }

        var idx = Subtitles.IndexOf(selected);
        if (idx < 0)
        {
            return;
        }

        for (var i = idx + 1; i < Subtitles.Count; i++)
        {
            if (Subtitles[i].Bookmark != null)
            {
                SelectAndScrollToSubtitle(Subtitles[i]);
                return;
            }
        }

        ShowStatus(Se.Language.General.NoBookmarksFound);
    }

    [RelayCommand]
    private void GoToPreviousBookmark()
    {
        var selected = SelectedSubtitle;
        if (selected == null)
        {
            return;
        }

        var idx = Subtitles.IndexOf(selected);
        if (idx < 0)
        {
            return;
        }

        for (var i = idx - 1; i >= 0; i--)
        {
            if (Subtitles[i].Bookmark != null)
            {
                SelectAndScrollToSubtitle(Subtitles[i]);
                return;
            }
        }

        ShowStatus(Se.Language.General.NoBookmarksFound);
    }

    [RelayCommand]
    private void SortByStartTime()
    {
        if (IsEmpty)
        {
            return;
        }

        var selected = SelectedSubtitle;

        var sortedSubtitles = Subtitles.OrderBy(p => p.StartTime).ToList();
        Subtitles.Clear();
        foreach (var s in sortedSubtitles)
        {
            Subtitles.Add(s);
        }

        Renumber();

        if (selected != null)
        {
            SelectAndScrollToSubtitle(selected);
        }
        else
        {
            SelectAndScrollToRow(0);
        }

        ShowStatus(Se.Language.Main.SortedByStartTime);
    }

    [RelayCommand]
    private void SortByEndTime()
    {
        if (IsEmpty)
        {
            return;
        }

        var selected = SelectedSubtitle;

        var sortedSubtitles = Subtitles.OrderBy(p => p.EndTime).ToList();
        Subtitles.Clear();
        foreach (var s in sortedSubtitles)
        {
            Subtitles.Add(s);
        }

        Renumber();

        if (selected != null)
        {
            SelectAndScrollToSubtitle(selected);
        }
        else
        {
            SelectAndScrollToRow(0);
        }

        ShowStatus(Se.Language.Main.SortedByEndTime);
    }

    [RelayCommand]
    private void SortByNumber()
    {
        if (IsEmpty)
        {
            return;
        }

        var selected = SelectedSubtitle;

        var sortedSubtitles = Subtitles.OrderBy(p => p.Number).ToList();
        Subtitles.Clear();
        foreach (var s in sortedSubtitles)
        {
            Subtitles.Add(s);
        }

        Renumber();

        if (selected != null)
        {
            SelectAndScrollToSubtitle(selected);
        }
        else
        {
            SelectAndScrollToRow(0);
        }

        ShowStatus(Se.Language.Main.SortedByNumber);
    }

    [RelayCommand]
    private async Task ShowSortBy()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        var result = await ShowDialogAsync<SortByWindow, SortByViewModel>(vm => { vm.Initialize(Subtitles.ToList()); });

        if (result.OkPressed)
        {
            var selectedSubtitle = SelectedSubtitle;
            Subtitles.Clear();
            foreach (var s in result.Subtitles)
            {
                Subtitles.Add(s);
            }

            Renumber();

            if (selectedSubtitle != null)
            {
                SelectAndScrollToSubtitle(selectedSubtitle);
            }
            else
            {
                SelectAndScrollToRow(0);
            }
        }
    }

    [RelayCommand]
    private async Task ListErrors()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        var list = new List<SubtitleLineViewModel>();
        for (int i = 0; i < Subtitles.Count; i++)
        {
            SubtitleLineViewModel? s = Subtitles[i];
            var prev = i > 0 ? Subtitles[i - 1] : null;
            var next = i < Subtitles.Count - 1 ? Subtitles[i + 1] : null;
            if (!string.IsNullOrEmpty(s.GetErrors(prev, next)))
            {
                list.Add(s);
            }
        }

        var result = await ShowDialogAsync<ErrorListWindow, ErrorListViewModel>(vm => { vm.Initialize(list.ToList()); });

        if (result.GoToPressed && result.SelectedSubtitle != null)
        {
            SelectAndScrollToSubtitle(result.SelectedSubtitle.Subtitle);
        }
    }

    [RelayCommand]
    private void GoToPreviousError()
    {
        if (Window == null || IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        var selected = SelectedSubtitle;
        if (selected == null)
        {
            return;
        }

        var idx = Subtitles.IndexOf(selected);
        if (idx < 0)
        {
            return;
        }

        for (var i = idx - 1; i >= 0; i--)
        {
            var s = Subtitles[i];
            var prev = i > 0 ? Subtitles[i - 1] : null;
            var next = i < Subtitles.Count - 1 ? Subtitles[i + 1] : null;
            if (!string.IsNullOrEmpty(s.GetErrors(prev, next)))
            {
                SelectAndScrollToRow(i);
                break;
            }
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private void GoToNextError()
    {
        if (Window == null || IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        var selected = SelectedSubtitle;
        if (selected == null)
        {
            return;
        }

        var idx = Subtitles.IndexOf(selected);
        if (idx < 0)
        {
            return;
        }

        for (var i = idx + 1; i < Subtitles.Count; i++)
        {
            var s = Subtitles[i];
            var prev = i > 0 ? Subtitles[i - 1] : null;
            var next = i < Subtitles.Count - 1 ? Subtitles[i + 1] : null;
            if (!string.IsNullOrEmpty(s.GetErrors(prev, next)))
            {
                SelectAndScrollToRow(i);
                break;
            }
        }

        _shortcutManager.ClearKeys();
    }


    [RelayCommand]
    private async Task CommandShowSettingsLanguage()
    {
        var viewModel = await ShowDialogAsync<LanguageWindow, LanguageViewModel>();
        if (viewModel.OkPressed && viewModel.SelectedLanguage != null)
        {
            await LoadLanguage(viewModel.SelectedLanguage.FileName);
        }
    }

    private async Task LoadLanguage(string jsonFileName)
    {
        var json = await File.ReadAllTextAsync(jsonFileName, Encoding.UTF8);
        var language = JsonSerializer.Deserialize<SeLanguage>(json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            });

        Se.Language = language ?? new SeLanguage();

        // Rebuild settings-page dropdown labels that capture Se.Language at type init.
        SettingsViewModel.ReloadLanguageMaps();

        // reload current layout
        InitMenu.Make(this);
        if (OperatingSystem.IsMacOS())
            Layout.InitNativeMacMenu.Rebuild(this);
        SetLayout(Se.Settings.General.LayoutNumber);

        if (Toolbar is Border toolbarBorder)
        {
            var tb = InitToolbar.Make(this);
            if (tb is Border newToolbarBorder)
            {
                var grid = newToolbarBorder.Child;
                newToolbarBorder.Child = null;
                toolbarBorder.Child = grid;
            }
        }

        ReloadShortcuts();
    }

    [RelayCommand]
    private async Task OpenDataFolder()
    {
        await _folderHelper.OpenFolder(Window!, Se.DataFolder);
    }

    [RelayCommand]
    private void ToggleShowColumnStartTime()
    {
        Se.Settings.General.ShowColumnStartTime = !Se.Settings.General.ShowColumnStartTime;
        ShowColumnStartTime = Se.Settings.General.ShowColumnStartTime;
        AutoFitColumns();
    }

    [RelayCommand]
    private void ToggleShowColumnEndTime()
    {
        Se.Settings.General.ShowColumnEndTime = !Se.Settings.General.ShowColumnEndTime;
        ShowColumnEndTime = Se.Settings.General.ShowColumnEndTime;
        AutoFitColumns();
    }

    [RelayCommand]
    private void ToggleShowColumnGap()
    {
        Se.Settings.General.ShowColumnGap = !Se.Settings.General.ShowColumnGap;
        ShowColumnGap = Se.Settings.General.ShowColumnGap;
        AutoFitColumns();
    }

    [RelayCommand]
    private void ToggleShowColumnDuration()
    {
        Se.Settings.General.ShowColumnDuration = !Se.Settings.General.ShowColumnDuration;
        ShowColumnDuration = Se.Settings.General.ShowColumnDuration;
        AutoFitColumns();
    }

    [RelayCommand]
    private void ToggleShowColumnActor()
    {
        Se.Settings.General.ShowColumnActor = !Se.Settings.General.ShowColumnActor;
        ShowColumnActor = Se.Settings.General.ShowColumnActor;
        AutoFitColumns();
    }

    [RelayCommand]
    private void ToggleShowColumnStyle()
    {
        Se.Settings.General.ShowColumnStyle = !Se.Settings.General.ShowColumnStyle;
        ShowColumnStyle = Se.Settings.General.ShowColumnStyle;
        AutoFitColumns();
    }

    [RelayCommand]
    private void ToggleShowColumnCps()
    {
        Se.Settings.General.ShowColumnCps = !Se.Settings.General.ShowColumnCps;
        ShowColumnCps = Se.Settings.General.ShowColumnCps;
        AutoFitColumns();
    }

    [RelayCommand]
    private void ToggleShowColumnWpm()
    {
        Se.Settings.General.ShowColumnWpm = !Se.Settings.General.ShowColumnWpm;
        ShowColumnWpm = Se.Settings.General.ShowColumnWpm;
        AutoFitColumns();
    }

    [RelayCommand]
    private void ToggleShowColumnPixelWidth()
    {
        Se.Settings.General.ShowColumnPixelWidth = !Se.Settings.General.ShowColumnPixelWidth;
        ShowColumnPixelWidth = Se.Settings.General.ShowColumnPixelWidth;
        AutoFitColumns();
    }

    [RelayCommand]
    private void ToggleShowColumnLayer()
    {
        ShowColumnLayer = !ShowColumnLayer;
        Se.Settings.General.ShowColumnLayer = ShowColumnLayer;
        ShowColumnLayer = Se.Settings.General.ShowColumnLayer;
        AutoFitColumns();
    }

    [RelayCommand]
    private void DuplicateSelectedLines()
    {
        var newSubtitles = new List<SubtitleLineViewModel>();
        foreach (var selected in _selectedSubtitles ?? [])
        {
            newSubtitles.Add(new SubtitleLineViewModel(selected));
        }

        foreach (var newSubtitle in newSubtitles)
        {
            _insertService.InsertInCorrectPosition(Subtitles, newSubtitle);
        }

        _shortcutManager.ClearKeys();
    }


    [RelayCommand]
    private async Task DeleteSelectedLines()
    {
        await DeleteSelectedItems();
    }

    [RelayCommand]
    private async Task RippleDeleteSelectedLines()
    {
        await RippleDeleteSelectedItems();
    }

    [RelayCommand]
    private void InsertLineBefore()
    {
        RunWithoutChangeDetection(() =>
        {
            InsertBeforeSelectedItem();
        });

        if (Se.Settings.Tools.GridFocusTextboxAfterInsertNew)
        {
            FocusEditTextBox();
        }
    }

    [RelayCommand]
    private void InsertLineAfter()
    {
        RunWithoutChangeDetection(() =>
        {
            InsertAfterSelectedItem();
        });

        if (Se.Settings.Tools.GridFocusTextboxAfterInsertNew)
        {
            FocusEditTextBox();
        }
    }

    [RelayCommand]
    private void InsertLineAtEnd()
    {
        RunWithoutChangeDetection(() =>
        {
            if (Subtitles.Count == 0)
            {
                _insertService.InsertBefore(SelectedSubtitleFormat, _subtitle, Subtitles, 0, string.Empty);
                Renumber();
                SelectAndScrollToRow(0);
            }
            else
            {
                var lastIndex = Subtitles.Count - 1;
                _insertService.InsertAfter(SelectedSubtitleFormat, _subtitle, Subtitles, lastIndex, string.Empty);
                Renumber();
                SelectAndScrollToRow(lastIndex + 1);
            }

            _updateAudioVisualizer = true;
        });

        if (Se.Settings.Tools.GridFocusTextboxAfterInsertNew)
        {
            FocusEditTextBox();
        }
    }

    [RelayCommand]
    private void MergeWithLineBefore()
    {
        RunWithoutChangeDetection(() =>
        {
            MergeLineBefore();
        });
    }

    [RelayCommand]
    private void MergeWithLineAfter()
    {
        RunWithoutChangeDetection(() =>
        {
            MergeLineAfter();
        });
    }

    [RelayCommand]
    private void MergeWithLineBeforeKeepBreaks()
    {
        RunWithoutChangeDetection(() =>
        {
            MergeLineBeforeKeepBreaks();
        });
    }

    [RelayCommand]
    private void MergeWithLineAfterKeepBreaks()
    {
        RunWithoutChangeDetection(() =>
        {
            MergeLineAfterKeepBreaks();
        });
    }

    [RelayCommand]
    private void MergeWithLineAfterAsDialog()
    {
        RunWithoutChangeDetection(() =>
        {
            var selected = SelectedSubtitle;
            if (selected == null)
            {
                return;
            }

            var index = Subtitles.IndexOf(selected);
            var next = Subtitles.GetOrNull(index + 1);
            if (next == null)
            {
                return;
            }

            _mergeManager.MergeSelectedLinesAsDialog(Subtitles, new List<SubtitleLineViewModel> { selected, next });
            Renumber();
            SelectAndScrollToRow(index);
            _updateAudioVisualizer = true;
        });
    }

    [RelayCommand]
    private void MergeWithLineBeforeAsDialog()
    {
        RunWithoutChangeDetection(() =>
        {
            var selected = SelectedSubtitle;
            if (selected == null)
            {
                return;
            }

            var index = Subtitles.IndexOf(selected);
            if (index <= 0)
            {
                return;
            }

            var prev = Subtitles[index - 1];
            _mergeManager.MergeSelectedLinesAsDialog(Subtitles, new List<SubtitleLineViewModel> { prev, selected });
            Renumber();
            SelectAndScrollToRow(index - 1);
            _updateAudioVisualizer = true;
        });
    }

    [RelayCommand]
    private void ToggleDialogDashes()
    {
        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
        if (selectedItems.Count == 0)
        {
            return;
        }

        // Determine direction from the first selected line: if any of its lines
        // already start with a dash, toggle OFF for every selected line; else
        // toggle ON. Matches SE 4 behaviour so a single hotkey flips the whole
        // selection in one direction.
        var firstLines = selectedItems[0].Text?.SplitToLines() ?? new List<string>();
        var hasStartDash = firstLines.Any(l =>
            HtmlUtil.RemoveHtmlTags(l, true).TrimStart().StartsWith('-'));

        var dialogStyle = Enum.TryParse<DialogType>(Se.Settings.General.DialogStyle, out var ds)
            ? ds
            : DialogType.DashBothLinesWithSpace;
        var dialogHelper = new DialogSplitMerge { DialogStyle = dialogStyle, SkipLineEndingCheck = true };

        foreach (var item in selectedItems)
        {
            item.Text = hasStartDash
                ? RemoveDialogDashes(item.Text)
                : AddDialogDashes(item.Text, dialogHelper);

            // Keep the translation/original column in sync so the two views
            // don't drift apart — matches what MergeManager.MergeSelectedLinesAsDialog
            // already does for OriginalText.
            if (!string.IsNullOrEmpty(item.OriginalText))
            {
                item.OriginalText = hasStartDash
                    ? RemoveDialogDashes(item.OriginalText)
                    : AddDialogDashes(item.OriginalText, dialogHelper);
            }
        }

        _updateAudioVisualizer = true;
    }

    private static string AddDialogDashes(string text, DialogSplitMerge dialogHelper)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        var lines = text.SplitToLines();
        if (lines.Count < 2 || lines.Count > 3)
        {
            return text; // only dialog-shaped paragraphs (2-3 lines) get dashes
        }

        var sb = new System.Text.StringBuilder();
        foreach (var line in lines)
        {
            var pre = string.Empty;
            var s = Utilities.SplitStartTags(line, ref pre);
            sb.Append(pre).Append("- ").AppendLine(s);
        }

        return dialogHelper.FixDashesAndSpaces(sb.ToString().Trim());
    }

    private static string RemoveDialogDashes(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        var lines = text.SplitToLines();
        var sb = new System.Text.StringBuilder();
        foreach (var line in lines)
        {
            var pre = string.Empty;
            var s = Utilities.SplitStartTags(line, ref pre);
            sb.Append(pre).AppendLine(s.TrimStart('-', '‐').TrimStart());
        }

        return sb.ToString().Trim();
    }

    [RelayCommand]
    private void MoveStartOneFrameBack() => MoveStartByFrames(-1, keepGapPrevIfClose: false);

    [RelayCommand]
    private void MoveStartOneFrameForward() => MoveStartByFrames(1, keepGapPrevIfClose: false);

    [RelayCommand]
    private void MoveEndOneFrameBack() => MoveEndByFrames(-1, keepGapNextIfClose: false);

    [RelayCommand]
    private void MoveEndOneFrameForward() => MoveEndByFrames(1, keepGapNextIfClose: false);

    // SE 4 parity (issue #11245) — "keep gap" variants: when the previous/next
    // subtitle is already <= MinimumBetweenLines away, dragging the cue also drags the
    // neighbor's edge by the same delta, preserving the existing small gap instead of
    // collapsing into the neighbor or being clamped.
    [RelayCommand]
    private void MoveStartOneFrameBackKeepGapPrev() => MoveStartByFrames(-1, keepGapPrevIfClose: true);

    [RelayCommand]
    private void MoveStartOneFrameForwardKeepGapPrev() => MoveStartByFrames(1, keepGapPrevIfClose: true);

    [RelayCommand]
    private void MoveEndOneFrameBackKeepGapNext() => MoveEndByFrames(-1, keepGapNextIfClose: true);

    [RelayCommand]
    private void MoveEndOneFrameForwardKeepGapNext() => MoveEndByFrames(1, keepGapNextIfClose: true);

    private void MoveStartByFrames(int frames, bool keepGapPrevIfClose)
    {
        var s = SelectedSubtitle;
        if (s == null || LockTimeCodes)
        {
            return;
        }

        var deltaMs = FramesToMilliseconds(frames);
        var gapMs = Se.Settings.General.MinimumBetweenLines.GetMilliseconds();
        var newStartMs = s.StartTime.TotalMilliseconds + deltaMs;

        // Mirror the WaveformSetStart guard: keep start at least MinimumBetweenLines
        // ahead of end, so repeated "move start forward" hotkey presses can't
        // collapse the duration to zero or negative.
        if (newStartMs >= s.EndTime.TotalMilliseconds - gapMs)
        {
            return;
        }

        // KeepGapPrev variant: when the previous subtitle is already "close"
        // (gap to it is within MinimumBetweenLines, i.e. tighter than the default),
        // drag its end along with this start by the same delta so the original gap
        // is preserved. Otherwise SE 4's behavior would either collapse the gap
        // (moving back) or be clamped at the standard gap (moving forward).
        var idx = SelectedSubtitleIndex ?? -1;
        var prev = idx > 0 ? Subtitles[idx - 1] : null;
        var prevGapMs = 0.0;
        var prevIsClose = false;
        if (keepGapPrevIfClose && prev != null
            && prev.EndTime.TotalMilliseconds <= s.StartTime.TotalMilliseconds
            && prev.EndTime.TotalMilliseconds + gapMs >= s.StartTime.TotalMilliseconds)
        {
            prevIsClose = true;
            prevGapMs = s.StartTime.TotalMilliseconds - prev.EndTime.TotalMilliseconds;

            // If moving back would push the previous subtitle below min duration, abort.
            var minDurMs = Se.Settings.General.SubtitleMinimumDisplayMilliseconds;
            if (deltaMs < 0
                && (prev.EndTime.TotalMilliseconds - prev.StartTime.TotalMilliseconds) + deltaMs < minDurMs)
            {
                return;
            }
        }

        s.SetStartTimeOnly(TimeSpan.FromMilliseconds(newStartMs));

        if (prevIsClose && prev != null)
        {
            prev.EndTime = TimeSpan.FromMilliseconds(newStartMs - prevGapMs);
        }

        _updateAudioVisualizer = true;
    }

    private void MoveEndByFrames(int frames, bool keepGapNextIfClose)
    {
        var s = SelectedSubtitle;
        if (s == null || LockTimeCodes)
        {
            return;
        }

        var deltaMs = FramesToMilliseconds(frames);
        var gapMs = Se.Settings.General.MinimumBetweenLines.GetMilliseconds();
        var newEndMs = s.EndTime.TotalMilliseconds + deltaMs;

        // Mirror the WaveformSetEnd guard: keep end at least MinimumBetweenLines
        // behind start so repeated "move end back" hotkey presses can't push
        // end before start.
        if (newEndMs <= s.StartTime.TotalMilliseconds + gapMs)
        {
            return;
        }

        // KeepGapNext variant: symmetric counterpart to KeepGapPrev above.
        var idx = SelectedSubtitleIndex ?? -1;
        var next = idx >= 0 && idx + 1 < Subtitles.Count ? Subtitles[idx + 1] : null;
        var nextGapMs = 0.0;
        var nextIsClose = false;
        if (keepGapNextIfClose && next != null
            && s.EndTime.TotalMilliseconds <= next.StartTime.TotalMilliseconds
            && s.EndTime.TotalMilliseconds + gapMs >= next.StartTime.TotalMilliseconds)
        {
            nextIsClose = true;
            nextGapMs = next.StartTime.TotalMilliseconds - s.EndTime.TotalMilliseconds;

            // If moving forward would push the next subtitle below min duration, abort.
            var minDurMs = Se.Settings.General.SubtitleMinimumDisplayMilliseconds;
            if (deltaMs > 0
                && (next.EndTime.TotalMilliseconds - next.StartTime.TotalMilliseconds) - deltaMs < minDurMs)
            {
                return;
            }
        }

        s.EndTime = TimeSpan.FromMilliseconds(newEndMs);

        if (nextIsClose && next != null)
        {
            next.SetStartTimeOnly(TimeSpan.FromMilliseconds(newEndMs + nextGapMs));
        }

        _updateAudioVisualizer = true;
    }

    private static int FramesToMilliseconds(int frames)
    {
        var frameRate = Se.Settings.General.CurrentFrameRate;
        if (frameRate < 10)
        {
            frameRate = 25; // safe fallback if frame rate hasn't been set
        }

        return (int)Math.Round(frames * 1000.0 / frameRate, MidpointRounding.AwayFromZero);
    }

    [RelayCommand]
    private void MergeSelectedLines()
    {
        MergeLinesSelected();
    }

    [RelayCommand]
    private void MergeSelectedLinesDialog()
    {
        MergeLinesSelectedAsDialog();
    }

    [RelayCommand]
    private void MergeSelectedLinesBilingual()
    {
        MergeLinesSelectedBilingual();
    }

    [RelayCommand]
    private void ToggleCasing()
    {
        if (IsSubtitleGridFocused())
        {
            var selectedItems = _selectedSubtitles?.ToList() ?? [];
            if (selectedItems.Count == 0)
            {
                return;
            }

            foreach (var item in selectedItems)
            {
                item.Text = _casingToggler.ToggleCasing(item.Text, SelectedSubtitleFormat);
            }

            _updateAudioVisualizer = true;
            return;
        }

        if (EditTextBox.SelectedText.Length <= 0)
        {
            return;
        }

        EditTextBox.SelectedText = _casingToggler.ToggleCasing(EditTextBox.SelectedText, SelectedSubtitleFormat);
    }

    [RelayCommand]
    private void SelectionToLower()
    {
        if (EditTextBox.SelectedText.Length <= 0)
        {
            return;
        }

        EditTextBox.SelectedText = EditTextBox.SelectedText.ToLower(CultureInfo.CurrentCulture);
    }

    [RelayCommand]
    private void SelectionToUpper()
    {
        if (EditTextBox.SelectedText.Length <= 0)
        {
            return;
        }

        EditTextBox.SelectedText = EditTextBox.SelectedText.ToUpper(CultureInfo.CurrentCulture);
    }

    [RelayCommand]
    private void GoogleIt()
    {
        var selected = EditTextBox.SelectedText;
        if (string.IsNullOrWhiteSpace(selected))
        {
            return;
        }

        UiUtil.OpenUrl("https://www.google.com/search?q=" + Uri.EscapeDataString(selected));
    }

    [RelayCommand]
    private void ToggleLinesItalic()
    {
        ToggleItalic();
    }

    [RelayCommand]
    private void ToggleLinesItalicOrSelectedText()
    {
        var selectedItems = _selectedSubtitles?.ToList() ?? [];
        if (selectedItems.Count == 0)
        {
            return;
        }

        if (selectedItems.Count == 1 && EditTextBox.SelectedText.Length > 0)
        {
            TextBoxItalic();
            return;
        }

        ToggleItalic();
    }


    [RelayCommand]
    private void ToggleLinesBold()
    {
        ToggleBold();
    }

    [RelayCommand]
    private async Task ShowAlignmentPicker()
    {
        var selected = SelectedSubtitle;
        if (selected == null)
        {
            return;
        }

        var result = await ShowDialogAsync<PickAlignmentWindow, PickAlignmentViewModel>(vm => { vm.Initialize(selected, SubtitleGrid.SelectedItems.Count); });

        if (result.OkPressed)
        {
            SetAlignmentToSelected(result.Alignment);
            _updateAudioVisualizer = true;
        }
    }

    [RelayCommand]
    private void DoAlignmentAn1()
    {
        var selected = SelectedSubtitle;
        if (selected == null)
        {
            return;
        }

        SetAlignmentToSelected("an1");
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void DoAlignmentAn2()
    {
        var selected = SelectedSubtitle;
        if (selected == null)
        {
            return;
        }

        SetAlignmentToSelected("an2");
        _updateAudioVisualizer = true;
    }


    [RelayCommand]
    private void DoAlignmentAn3()
    {
        var selected = SelectedSubtitle;
        if (selected == null)
        {
            return;
        }

        SetAlignmentToSelected("an3");
        _updateAudioVisualizer = true;
    }


    [RelayCommand]
    private void DoAlignmentAn4()
    {
        var selected = SelectedSubtitle;
        if (selected == null)
        {
            return;
        }

        SetAlignmentToSelected("an4");
        _updateAudioVisualizer = true;
    }


    [RelayCommand]
    private void DoAlignmentAn5()
    {
        var selected = SelectedSubtitle;
        if (selected == null)
        {
            return;
        }

        SetAlignmentToSelected("an5");
        _updateAudioVisualizer = true;
    }


    [RelayCommand]
    private void DoAlignmentAn6()
    {
        var selected = SelectedSubtitle;
        if (selected == null)
        {
            return;
        }

        SetAlignmentToSelected("an6");
        _updateAudioVisualizer = true;
    }


    [RelayCommand]
    private void DoAlignmentAn7()
    {
        var selected = SelectedSubtitle;
        if (selected == null)
        {
            return;
        }

        SetAlignmentToSelected("an7");
        _updateAudioVisualizer = true;
    }


    [RelayCommand]
    private void DoAlignmentAn8()
    {
        var selected = SelectedSubtitle;
        if (selected == null)
        {
            return;
        }

        SetAlignmentToSelected("an8");
        _updateAudioVisualizer = true;
    }


    [RelayCommand]
    private void DoAlignmentAn9()
    {
        var selected = SelectedSubtitle;
        if (selected == null)
        {
            return;
        }

        SetAlignmentToSelected("an9");
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private async Task ShowFontNamePicker()
    {
        var selectedItems = _selectedSubtitles?.ToList() ?? [];
        if (selectedItems.Count == 0)
        {
            return;
        }

        var result =
            await ShowDialogAsync<PickFontNameWindow, PickFontNameViewModel>(vm => { vm.Initialize(); });

        if (result.OkPressed && result.SelectedFontName != null)
        {
            _fontNameService.SetFontName(selectedItems, result.SelectedFontName, SelectedSubtitleFormat);
        }
    }

    [RelayCommand]
    private async Task ShowColorPicker()
    {
        var selectedItems = _selectedSubtitles?.ToList() ?? [];
        if (selectedItems.Count == 0)
        {
            return;
        }

        var result = await ShowDialogAsync<ColorPickerWindow, ColorPickerViewModel>(vm => vm.Initialize(Se.Settings.Tools.LastColorPickerColor.FromHexToColor()));
        if (!result.OkPressed)
        {
            return;
        }

        if (ColorTextBoxIfSelected(result.SelectedColor))
        {
            return;
        }

        _colorService.SetColor(selectedItems, result.SelectedColor, GetUpdateSubtitle(), SelectedSubtitleFormat);
        _updateAudioVisualizer = true;
    }

    private ITextBoxWrapper? GetFocusedTextBoxWrapper()
    {
        if (EditTextBox.IsFocused)
        {
            return EditTextBox;
        }

        if (EditTextBoxOriginal.IsFocused)
        {
            return EditTextBoxOriginal;
        }

        return null;
    }

    private void ToggleColor(Color color)
    {
        var selectedItems = _selectedSubtitles?.ToList() ?? [];
        if (selectedItems.Count == 0)
        {
            return;
        }

        if (selectedItems.Count == 1 && ColorTextBoxIfSelected(color))
        {
            return;
        }

        if (_colorService.ContainsColor(color, selectedItems.First(), SelectedSubtitleFormat))
        {
            _colorService.RemoveColorTags(selectedItems, GetUpdateSubtitle(), SelectedSubtitleFormat);
        }
        else
        {
            _colorService.RemoveColorTags(selectedItems, GetUpdateSubtitle(), SelectedSubtitleFormat);
            _colorService.SetColor(selectedItems, color, GetUpdateSubtitle(), SelectedSubtitleFormat);
        }

        _updateAudioVisualizer = true;
    }

    private bool ColorTextBoxIfSelected(Color color)
    {
        var tb = GetFocusedTextBoxWrapper();
        if (tb != null)
        {
            SetTextBoxColor(tb, color);
            return true;
        }

        return false;
    }

    [RelayCommand]
    private void SetColor1()
    {
        ToggleColor(Se.Settings.Color1.FromHexToColor());
    }

    [RelayCommand]
    private void SetColor2()
    {
        ToggleColor(Se.Settings.Color2.FromHexToColor());
    }

    [RelayCommand]
    private void SetColor3()
    {
        ToggleColor(Se.Settings.Color3.FromHexToColor());
    }

    [RelayCommand]
    private void SetColor4()
    {
        ToggleColor(Se.Settings.Color4.FromHexToColor());
    }

    [RelayCommand]
    private void SetColor5()
    {
        ToggleColor(Se.Settings.Color5.FromHexToColor());
    }

    [RelayCommand]
    private void SetColor6()
    {
        ToggleColor(Se.Settings.Color6.FromHexToColor());
    }

    [RelayCommand]
    private void SetColor7()
    {
        ToggleColor(Se.Settings.Color7.FromHexToColor());
    }

    [RelayCommand]
    private void SetColor8()
    {
        ToggleColor(Se.Settings.Color8.FromHexToColor());
    }

    [RelayCommand]
    private void RemoveColor()
    {
        var selectedItems = _selectedSubtitles?.ToList() ?? [];
        if (selectedItems.Count == 0)
        {
            return;
        }

        _colorService.RemoveColorTags(selectedItems, GetUpdateSubtitle(), SelectedSubtitleFormat);
    }

    private void SurroundWith(string surroundLeft, string surroundRight)
    {
        var selectedItems = _selectedSubtitles?.ToList() ?? [];
        if (selectedItems.Count == 0)
        {
            return;
        }

        var first = selectedItems.First();
        var haveSurround = first.Text.StartsWith(surroundLeft) && first.Text.EndsWith(surroundRight);

        // add toggle functionality
        foreach (var item in selectedItems)
        {
            if (haveSurround)
            {
                if (item.Text.StartsWith(surroundLeft) && item.Text.EndsWith(surroundRight))
                {
                    item.Text = item.Text.Substring(surroundLeft.Length,
                        item.Text.Length - surroundLeft.Length - surroundRight.Length);
                }
            }
            else
            {
                item.Text = surroundLeft + item.Text + surroundRight;
            }
        }

        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void SurroundWith1()
    {
        SurroundWith(Se.Settings.Surround1Left, Se.Settings.Surround1Right);
    }

    [RelayCommand]
    private void SurroundWith2()
    {
        SurroundWith(Se.Settings.Surround2Left, Se.Settings.Surround2Right);
    }

    [RelayCommand]
    private void SurroundWith3()
    {
        SurroundWith(Se.Settings.Surround3Left, Se.Settings.Surround3Right);
    }

    [RelayCommand]
    private void RemoveFormattingAll()
    {
        var selectedItems = _selectedSubtitles?.ToList() ?? [];
        if (selectedItems.Count == 0)
        {
            return;
        }

        foreach (var item in selectedItems)
        {
            item.Text = HtmlUtil.RemoveHtmlTags(item.Text, true);
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private void RemoveFormattingItalic()
    {
        var selectedItems = _selectedSubtitles?.ToList() ?? [];
        if (selectedItems.Count == 0)
        {
            return;
        }

        foreach (var item in selectedItems)
        {
            item.Text = item.Text.Replace("{\\i1}", string.Empty);
            item.Text = item.Text.Replace("{\\i0}", string.Empty);
            item.Text = item.Text.Replace("\\i1", string.Empty);
            item.Text = item.Text.Replace("\\i0", string.Empty);

            item.Text = HtmlUtil.RemoveOpenCloseTags(item.Text, "i");
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private void RemoveFormattingBold()
    {
        var selectedItems = _selectedSubtitles?.ToList() ?? [];
        if (selectedItems.Count == 0)
        {
            return;
        }

        foreach (var item in selectedItems)
        {
            item.Text = item.Text.Replace("{\\b1}", string.Empty);
            item.Text = item.Text.Replace("{\\b0}", string.Empty);
            item.Text = item.Text.Replace("\\b1", string.Empty);
            item.Text = item.Text.Replace("\\b0", string.Empty);

            item.Text = HtmlUtil.RemoveOpenCloseTags(item.Text, "b");
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private void RemoveFormattingUnderline()
    {
        var selectedItems = _selectedSubtitles?.ToList() ?? [];
        if (selectedItems.Count == 0)
        {
            return;
        }

        foreach (var item in selectedItems)
        {
            item.Text = item.Text.Replace("{\\u1}", string.Empty);
            item.Text = item.Text.Replace("{\\u0}", string.Empty);
            item.Text = item.Text.Replace("\\u1", string.Empty);
            item.Text = item.Text.Replace("\\u0", string.Empty);

            item.Text = HtmlUtil.RemoveOpenCloseTags(item.Text, "u");
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private void RemoveFormattingColor()
    {
        var selectedItems = _selectedSubtitles?.ToList() ?? [];
        if (selectedItems.Count == 0)
        {
            return;
        }

        _colorService.RemoveColorTags(selectedItems, GetUpdateSubtitle(), SelectedSubtitleFormat);

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private void RemoveFormattingFontName()
    {
        var selectedItems = _selectedSubtitles?.ToList() ?? [];
        if (selectedItems.Count == 0)
        {
            return;
        }

        _fontNameService.RemoveFontNames(selectedItems, SelectedSubtitleFormat);

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private void RemoveFormattingAligment()
    {
        var selectedItems = _selectedSubtitles?.ToList() ?? [];
        if (selectedItems.Count == 0)
        {
            return;
        }

        foreach (var item in selectedItems)
        {
            item.Text = HtmlUtil.RemoveAssAlignmentTags(item.Text);
        }

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task ShowRestoreAutoBackup()
    {
        var viewModel = await ShowDialogAsync<RestoreAutoBackupWindow, RestoreAutoBackupViewModel>();

        if (viewModel.OkPressed && !string.IsNullOrEmpty(viewModel.RestoreFileName))
        {
            await SubtitleOpen(viewModel.RestoreFileName);
        }
    }

    [RelayCommand]
    private async Task ShowHistory()
    {
        _undoRedoManager.CheckForChanges(null);
        _undoRedoManager.StopChangeDetection();

        var result =
            await ShowDialogAsync<ShowHistoryWindow, ShowHistoryViewModel>(vm => { vm.Initialize(_undoRedoManager); });

        if (result.OkPressed && result.SelectedHistoryItem != null)
        {
            for (int i = 0; i <= _undoRedoManager.UndoCount; i++)
            {
                var undoItem = _undoRedoManager.Undo();
                if (undoItem?.Hash == result.SelectedHistoryItem.Hash)
                {
                    RestoreUndoRedoState(undoItem);
                    ShowUndoStatus();
                    break;
                }
            }
        }

        _undoRedoManager.StartChangeDetection();
    }

    [RelayCommand]
    private void ShowFind()
    {
        var selectedSubtitle = SelectedSubtitle;
        if (Subtitles.Count == 0 || selectedSubtitle == null || Window == null)
        {
            return;
        }

        if (_replaceViewModel != null)
        {
            _replaceClosingProgrammatically = true;
            _replaceViewModel.Window?.Close();
            _replaceViewModel = null;
        }

        if (_findViewModel != null && _findViewModel.Window != null && _findViewModel.Window.IsVisible)
        {
            _findPreviousFocus = Window?.FocusManager?.GetFocusedElement() as Control;
            _findViewModel.ResultFound = false;
            _findViewModel.Window.Activate();
            return;
        }

        _findPreviousFocus = Window?.FocusManager?.GetFocusedElement() as Control;
        var subs = Subtitles.Select(p => p.Text).ToList();
        var result = _windowService.ShowWindow<FindWindow, FindViewModel>(Window!, (window, vm) =>
        {
            WindowService.KeepTopmostWhileOwnerActive(window, Window!);
            _findViewModel = vm;

            var selectedText = string.Empty;
            if (EditTextBox != null && !string.IsNullOrEmpty(EditTextBox.SelectedText))
            {
                selectedText = EditTextBox.SelectedText;
            }

            if (string.IsNullOrEmpty(selectedText) && !string.IsNullOrEmpty(_findService.SearchText))
            {
                selectedText = _findService.SearchText;
            }

            vm.InitializeFindData(_findService, subs, selectedText, this);
            window.Closed += (_, _) =>
            {
                if (_findClosingProgrammatically)
                {
                    _findClosingProgrammatically = false;
                    return;
                }

                if (vm.ResultFound)
                {
                    FocusEditTextBox();
                }
                else
                {
                    Dispatcher.UIThread.Post(() => _findPreviousFocus?.Focus());
                }
            };
        });

        _shortcutManager.ClearKeys();
    }

    public void RequestFindData()
    {
        if (Subtitles.Count == 0)
        {
            return;
        }

        var subs = Subtitles.Select(p => p.Text).ToList();

        if (_findViewModel != null)
        {
            _findViewModel.InitializeFindData(_findService, subs, _findService.SearchText, this);
        }

        if (_replaceViewModel != null)
        {
            _replaceViewModel.RefreshSubtitles(subs);
        }
    }

    public async Task HandleFindResult(FindViewModel result)
    {
        result.ResultFound = false;

        var selectedSubtitle = SelectedSubtitle;
        if (Subtitles.Count == 0 || selectedSubtitle == null)
        {
            return;
        }

        if ((result.FindNextPressed || result.FindPreviousPressed) && !string.IsNullOrEmpty(result.SearchText))
        {
            var currentLineIndex = Subtitles.IndexOf(selectedSubtitle);
            var subs = Subtitles.Select(p => p.Text).ToList();
            _findService.Initialize(subs, SelectedSubtitleIndex ?? 0, result.WholeWord, result.FindMode);

            var idx = -1;
            if (result.FindNextPressed)
            {
                idx = _findService.FindNext(result.SearchText, subs, currentLineIndex, EditTextBox.SelectionEnd);
            }
            else
            {
                idx = _findService.FindPrevious(result.SearchText, subs, currentLineIndex, EditTextBox.SelectionStart - 1);
            }

            if (idx < 0)
            {
                var message = result.FindNextPressed
                    ? Se.Language.General.SearchItemNotFoundContinueFromTop
                    : Se.Language.General.SearchItemNotFoundContinueFromBottom;
                var answer = await ShowWrapAroundDialog(message);
                if (answer != MessageBoxResult.Yes)
                {
                    ShowStatus(string.Format(Se.Language.General.XNotFound, _findService.SearchText));
                    return;
                }
                idx = result.FindNextPressed
                    ? _findService.FindNext(result.SearchText, subs, 0, 0)
                    : _findService.FindPrevious(result.SearchText, subs, subs.Count, 0);
                if (idx < 0)
                {
                    ShowStatus(string.Format(Se.Language.General.XNotFound, _findService.SearchText));
                    return;
                }
            }

            if (_findViewModel != null)
            {
                _findViewModel.ResultFound = true;
            }

            Dispatcher.UIThread.Post(() =>
            {
                var subtitle = Subtitles.GetOrNull(idx);
                if (subtitle == null)
                {
                    return;
                }

                SubtitleGrid.SelectedIndex = idx;
                SubtitleGrid.SelectedItem = subtitle;
                SubtitleGrid.ScrollIntoView(subtitle, null);

                ShowStatus(string.Format(Se.Language.General.FoundXInLineYZ, _findService.CurrentTextFound, _findService.CurrentLineNumber + 1, _findService.CurrentTextIndex + 1));

                if (EditTextBox.Text != subtitle.Text)
                {
                    EditTextBox.Text = subtitle.Text;
                }

                EditTextBox.CaretIndex = _findService.CurrentTextIndex;
                EditTextBox.SelectionStart = _findService.CurrentTextIndex;
                EditTextBox.SelectionEnd = _findService.CurrentTextIndex + _findService.CurrentTextFound.Length;
            });
        }
    }

    [RelayCommand]
    private async Task FindNext()
    {
        var selectedSubtitle = SelectedSubtitle;
        if (Subtitles.Count == 0 || selectedSubtitle == null || Window == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(_findService.SearchText))
        {
            ShowFind();
            return;
        }

        var subs = Subtitles.Select(p => p.Text).ToList();
        var currentLineIndex = Subtitles.IndexOf(selectedSubtitle);
        var currentCharIndex = EditTextBox.CaretIndex;
        var idx = _findService.FindNext(_findService.SearchText, subs, currentLineIndex, currentCharIndex + 1);

        if (idx < 0)
        {
            var answer = await ShowWrapAroundDialog(Se.Language.General.SearchItemNotFoundContinueFromTop);
            if (answer != MessageBoxResult.Yes)
            {
                ShowStatus(string.Format(Se.Language.General.XNotFound, _findService.SearchText));
                _shortcutManager.ClearKeys();
                return;
            }
            idx = _findService.FindNext(_findService.SearchText, subs, 0, 0);
            if (idx < 0)
            {
                ShowStatus(string.Format(Se.Language.General.XNotFound, _findService.SearchText));
                _shortcutManager.ClearKeys();
                return;
            }
        }

        var foundText = _findService.CurrentTextFound;
        var foundLine = _findService.CurrentLineNumber;
        var foundIndex = _findService.CurrentTextIndex;

        Dispatcher.UIThread.Post(() =>
        {
            var subtitle = Subtitles.GetOrNull(idx);
            if (subtitle == null)
            {
                return;
            }

            SubtitleGrid.SelectedIndex = idx;
            SubtitleGrid.SelectedItem = subtitle;
            SubtitleGrid.ScrollIntoView(subtitle, null);

            ShowStatus(string.Format(Se.Language.General.FoundXInLineYZ, foundText, foundLine + 1, foundIndex + 1));

            // The text-box may still be empty if the SelectedItem binding hasn't propagated
            // yet by the time this dispatcher post runs; fall back to writing the text
            // ourselves so the selection range below lands on real characters.
            if (EditTextBox.Text == string.Empty)
            {
                EditTextBox.Text = subtitle.Text;
            }

            EditTextBox.CaretIndex = foundIndex;
            EditTextBox.SelectionStart = foundIndex;
            EditTextBox.SelectionEnd = foundIndex + foundText.Length;
        });

        FocusEditTextBox();
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task FindPrevious()
    {
        var selectedSubtitle = SelectedSubtitle;
        if (Subtitles.Count == 0 || selectedSubtitle == null || Window == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(_findService.SearchText))
        {
            ShowFind();
            return;
        }

        var subs = Subtitles.Select(p => p.Text).ToList();
        var currentLineIndex = Subtitles.IndexOf(selectedSubtitle);
        var idx = _findService.FindPrevious(_findService.SearchText, subs, currentLineIndex, EditTextBox.SelectionStart - 1);

        if (idx < 0)
        {
            var answer = await ShowWrapAroundDialog(Se.Language.General.SearchItemNotFoundContinueFromBottom);
            if (answer != MessageBoxResult.Yes)
            {
                ShowStatus(string.Format(Se.Language.General.XNotFound, _findService.SearchText));
                _shortcutManager.ClearKeys();
                return;
            }
            idx = _findService.FindPrevious(_findService.SearchText, subs, subs.Count, 0);
            if (idx < 0)
            {
                ShowStatus(string.Format(Se.Language.General.XNotFound, _findService.SearchText));
                _shortcutManager.ClearKeys();
                return;
            }
        }

        var foundText = _findService.CurrentTextFound;
        var foundLine = _findService.CurrentLineNumber;
        var foundIndex = _findService.CurrentTextIndex;

        Dispatcher.UIThread.Post(() =>
        {
            var subtitle = Subtitles.GetOrNull(idx);
            if (subtitle == null)
            {
                return;
            }

            SubtitleGrid.SelectedIndex = idx;
            SubtitleGrid.SelectedItem = subtitle;
            SubtitleGrid.ScrollIntoView(subtitle, null);

            ShowStatus(string.Format(Se.Language.General.FoundXInLineYZ, foundText, foundLine + 1, foundIndex + 1));

            // The text-box may still be empty if the SelectedItem binding hasn't propagated
            // yet by the time this dispatcher post runs; fall back to writing the text
            // ourselves so the selection range below lands on real characters.
            if (EditTextBox.Text == string.Empty)
            {
                EditTextBox.Text = subtitle.Text;
            }

            EditTextBox.CaretIndex = foundIndex;
            EditTextBox.SelectionStart = foundIndex;
            EditTextBox.SelectionEnd = foundIndex + foundText.Length;
        });

        FocusEditTextBox();
        _shortcutManager.ClearKeys();
    }

    private async Task<MessageBoxResult> ShowWrapAroundDialog(string message)
    {
        if (Window == null) return MessageBoxResult.No;
        var findWin = _findViewModel?.Window;
        var replaceWin = _replaceViewModel?.Window;
        var dialogWindow = (findWin?.IsVisible == true ? findWin : null)
                        ?? (replaceWin?.IsVisible == true ? replaceWin : null);
        var parentWindow = dialogWindow ?? Window!;
        return await MessageBox.Show(parentWindow, Se.Language.General.ContinueFindTitle,
            message, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
    }

    [RelayCommand]
    private void ShowReplace()
    {
        if (Subtitles.Count == 0 || Window == null)
        {
            return;
        }

        if (_findViewModel != null)
        {
            _findClosingProgrammatically = true;
            _findViewModel.Window?.Close();
            _findViewModel = null;
        }

        if (_replaceViewModel != null && _replaceViewModel.Window != null && _replaceViewModel.Window.IsVisible)
        {
            _replacePreviousFocus = Window?.FocusManager?.GetFocusedElement() as Control;
            _replaceViewModel.ResultFound = false;
            _replaceViewModel.Window.Activate();
            return;
        }

        _replacePreviousFocus = Window?.FocusManager?.GetFocusedElement() as Control;
        var subs = Subtitles.Select(p => p.Text).ToList();
        var result = _windowService.ShowWindow<ReplaceWindow, ReplaceViewModel>(Window!, (window, vm) =>
        {
            WindowService.KeepTopmostWhileOwnerActive(window, Window!);
            _replaceViewModel = vm;

            var selectedText = string.Empty;
            if (EditTextBox != null && !string.IsNullOrEmpty(EditTextBox.SelectedText))
            {
                selectedText = EditTextBox.SelectedText;
            }

            if (string.IsNullOrEmpty(selectedText) && !string.IsNullOrEmpty(_findService.SearchText))
            {
                selectedText = _findService.SearchText;
            }

            vm.InitializeFindData(_findService, subs, selectedText, this);
            window.Closed += (_, _) =>
            {
                if (_replaceClosingProgrammatically)
                {
                    _replaceClosingProgrammatically = false;
                    return;
                }

                if (vm.ResultFound)
                {
                    FocusEditTextBox();
                }
                else
                {
                    Dispatcher.UIThread.Post(() => _replacePreviousFocus?.Focus());
                }
            };
        });

        _shortcutManager.ClearKeys();
    }

    public async Task HandleReplaceResult(ReplaceViewModel result)
    {
        result.ResultFound = false;

        var selectedSubtitle = SelectedSubtitle;
        if (Subtitles.Count == 0 || selectedSubtitle == null)
        {
            return;
        }

        if ((result.FindNextPressed || result.ReplacePressed || result.ReplaceAllPressed) && !string.IsNullOrEmpty(result.SearchText))
        {
            var currentLineIndex = Subtitles.IndexOf(selectedSubtitle);
            var subs = Subtitles.Select(p => p.Text).ToList();
            _findService.Initialize(subs, SelectedSubtitleIndex ?? 0, result.WholeWord, result.FindMode);

            var idx = -1;
            if (result.FindNextPressed)
            {
                idx = _findService.FindNext(result.SearchText, subs, currentLineIndex, EditTextBox.SelectionEnd);
            }
            else if (result.ReplaceAllPressed)
            {
                var replaceCount = _findService.ReplaceAll(result.SearchText, result.ReplaceText);

                for (var i = 0; i < Subtitles.Count && i < subs.Count; i++)
                {
                    var s = Subtitles[i];
                    var newText = subs[i];
                    if (newText != s.Text)
                    {
                        s.Text = newText;
                    }
                }

                ShowStatus(string.Format(Se.Language.Main.ReplacedXWithYCountZ, result.SearchText, result.ReplaceText, replaceCount));
                return;
            }
            else // replace requested
            {
                var selectedText = EditTextBox.SelectedText;
                if (selectedText == result.SearchText)
                {
                    EditTextBox.SelectedText = result.ReplaceText;
                    subs[currentLineIndex] = EditTextBox.Text; // reflect replacement so FindNext won't re-match here
                }

                idx = _findService.FindNext(result.SearchText, subs, currentLineIndex, EditTextBox.SelectionEnd);
            }

            if (idx < 0)
            {
                var answer = await ShowWrapAroundDialog(Se.Language.General.SearchItemNotFoundContinueFromTop);
                if (answer != MessageBoxResult.Yes)
                {
                    ShowStatus(string.Format(Se.Language.General.XNotFound, _findService.SearchText));
                    return;
                }
                idx = _findService.FindNext(result.SearchText, subs, 0, 0);
                if (idx < 0)
                {
                    ShowStatus(string.Format(Se.Language.General.XNotFound, _findService.SearchText));
                    return;
                }
            }

            if (_replaceViewModel != null)
            {
                _replaceViewModel.ResultFound = true;
            }

            var foundText = _findService.CurrentTextFound;
            var foundLine = _findService.CurrentLineNumber;
            var foundIndex = _findService.CurrentTextIndex;

            Dispatcher.UIThread.Post(() =>
            {
                var subtitle = Subtitles.GetOrNull(idx);
                if (subtitle == null)
                {
                    return;
                }

                SubtitleGrid.SelectedIndex = idx;
                SubtitleGrid.SelectedItem = subtitle;
                SubtitleGrid.ScrollIntoView(subtitle, null);

                // The text-box binding may not have propagated yet by the time this dispatcher
                // post runs; ensure it shows the target subtitle's text before selecting.
                if (EditTextBox.Text != subtitle.Text)
                {
                    EditTextBox.Text = subtitle.Text;
                }

                EditTextBox.CaretIndex = foundIndex;
                EditTextBox.SelectionStart = foundIndex;
                EditTextBox.SelectionEnd = foundIndex + foundText.Length;

                ShowStatus(string.Format(Se.Language.General.FoundXInLineYZ, foundText, foundLine + 1, foundIndex + 1));
            });
        }
    }

    [RelayCommand]
    private async Task ShowMultipleReplace()
    {
        if (Subtitles.Count == 0)
        {
            return;
        }

        var result =
            await ShowDialogAsync<MultipleReplaceWindow, MultipleReplaceViewModel>(vm => { vm.Initialize(GetUpdateSubtitle()); });

        if (result.OkPressed)
        {
            SetSubtitles(result.FixedSubtitle);
            SelectAndScrollToRow(0);
            ShowStatus(string.Format(Se.Language.Main.ReplacedXOccurrences, result.TotalReplaced));
        }
    }


    [RelayCommand]
    private async Task ShowGoToLine()
    {
        if (Subtitles.Count == 0)
        {
            return;
        }

        var viewModel = await ShowDialogAsync<GoToLineNumberWindow, GoToLineNumberViewModel>(vm =>
        {
            var idx = 1;
            if (SelectedSubtitle != null)
            {
                idx = Subtitles.IndexOf(SelectedSubtitle) + 1;
            }

            vm.Initialize(idx, Subtitles.Count);
        });

        if (viewModel is { OkPressed: true, LineNumber: >= 0 } && viewModel.LineNumber <= Subtitles.Count)
        {
            var no = (int)viewModel.LineNumber;
            SelectAndScrollToRow(no - 1);
            var vp = GetVideoPlayerControl();
            if (Se.Settings.Tools.GoToLineNumberAlsoSetVideoPosition && !string.IsNullOrEmpty(_videoFileName) && vp != null)
            {
                var s = Subtitles.GetOrNull(no - 1);
                if (s != null)
                {
                    vp.Position = s.StartTime.TotalSeconds;
                    AudioVisualizerCenterOnPositionIfNeeded(s.StartTime.TotalSeconds);
                    _updateAudioVisualizer = true;
                }
            }
        }
    }

    [RelayCommand]
    private void RightToLeftToggle()
    {
        if (Window == null)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            if (!Se.Settings.General.IsLanguageRightToLeft())
            {
                if (Se.Settings.Appearance.SubtitleTextBoxColorTags)
                {
                    Se.Settings.Appearance.SubtitleTextBoxColorTags = false;
                    ApplySettings();
                }

                Se.Settings.Appearance.RightToLeft = !Se.Settings.Appearance.RightToLeft;
                IsRightToLeftEnabled = Se.Settings.Appearance.RightToLeft;
                RightToLeftHelper.SetRightToLeftForDataGridAndText(Window);
                return;
            }

            if (Window.FlowDirection == FlowDirection.RightToLeft)
            {
                IsRightToLeftEnabled = false;
                Se.Settings.Appearance.RightToLeft = false;
                Window.FlowDirection = FlowDirection.LeftToRight;
            }
            else
            {
                IsRightToLeftEnabled = true;
                Se.Settings.Appearance.RightToLeft = true;
                Window.FlowDirection = FlowDirection.RightToLeft;
            }

            // Force UI to update layout
            var content = Window.Content;
            Window.Content = null;
            Window.Content = content;
            Task.Delay(50);
            Window.InvalidateMeasure();
            Window.InvalidateArrange();
            Window.InvalidateVisual();
            Task.Delay(50);
            Window.Width += 0.1;
            Task.Delay(50);
            Window.Width -= 0.1;
        });

        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private void FixRightToLeftViaUnicodeControlCharacters()
    {
        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();

        foreach (var item in selectedItems)
        {
            item.Text = Utilities.FixRtlViaUnicodeChars(item.Text);
        }

        ShowStatus(string.Format(Se.Language.Main.FixedRightToLeftUsingUnicodeControlCharactersX, selectedItems.Count));
    }

    [RelayCommand]
    private void RemoveUnicodeControlCharacters()
    {
        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();

        foreach (var item in selectedItems)
        {
            item.Text = Utilities.RemoveUnicodeControlChars(item.Text);
        }

        ShowStatus(string.Format(Se.Language.Main.RemovedUnicodeControlCharactersX, selectedItems.Count));
    }

    [RelayCommand]
    private void ReverseRightToLeftStartEnd()
    {
        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();

        foreach (var item in selectedItems)
        {
            item.Text = Utilities.ReverseStartAndEndingForRightToLeft(item.Text);
        }

        ShowStatus(string.Format(Se.Language.Main.ReversedStartAndEndingsForRightToLeftX, selectedItems.Count));
    }

    [RelayCommand]
    private async Task ShowModifySelection()
    {
        var result = await ShowDialogAsync<ModifySelectionWindow, ModifySelectionViewModel>(vm =>
        {
            var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
            vm.Initialize(Subtitles.ToList(), selectedItems);
        });

        if (result.OkPressed && result.Subtitles.Count > 0)
        {
            if (result.SelectionNew)
            {
                SubtitleGrid.SelectedItems.Clear();
                SelectedSubtitleIndex = null;
                SelectedSubtitle = null;
                foreach (var item in Subtitles)
                {
                    if (result.Selection.Contains(item))
                    {
                        SubtitleGrid.SelectedItems.Add(item);
                    }
                }
            }
            else if (result.SelectionAdd)
            {
                foreach (var item in Subtitles)
                {
                    if (result.Selection.Contains(item) && !SubtitleGrid.SelectedItems.Contains(item))
                    {
                        SubtitleGrid.SelectedItems.Add(item);
                    }
                }
            }
            else if (result.SelectionSubtract)
            {
                foreach (var item in Subtitles)
                {
                    if (result.Selection.Contains(item) && SubtitleGrid.SelectedItems.Contains(item))
                    {
                        SubtitleGrid.SelectedItems.Remove(item);
                    }
                }
            }
            else if (result.SelectionIntersect)
            {
                var oldSelectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
                SubtitleGrid.SelectedItems.Clear();
                foreach (var item in Subtitles)
                {
                    if (result.Selection.Contains(item) && oldSelectedItems.Contains(item))
                    {
                        SubtitleGrid.SelectedItems.Add(item);
                    }
                }
            }
        }

        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void InverseSelection()
    {
        InverseRowSelection();
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private void SelectAllLines()
    {
        if (IsTextInputFocused())
        {
            var textBoxWrapper = GetFocusedTextBoxWrapper();
            if (textBoxWrapper != null)
            {
                textBoxWrapper.SelectAll();
            }
            else
            {
                // Fallback covers the same control set as IsTextInputFocused so Cmd+A from the
                // native menu isn't swallowed in non-edit-box text inputs (timecode MaskedTextBox,
                // source view TextEditor, actor AutoCompleteBox, etc.). MaskedTextBox inherits
                // from TextBox so the first branch already catches it.
                var focused = Window?.FocusManager?.GetFocusedElement();
                if (focused is TextBox tb)
                {
                    tb.SelectAll();
                }
                else if (focused is TextEditor editor)
                {
                    editor.SelectAll();
                }
                else if (focused is AutoCompleteBox acb)
                {
                    acb.FindDescendantOfType<TextBox>()?.SelectAll();
                }
            }
            return;
        }
        SelectAllRows();
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private void RepeatPreviousLine()
    {
        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().OrderBy(p => p.StartTime).ToList();
        var vp = GetVideoPlayerControl();
        if (Window == null || selectedItems.Count == 0 || vp == null)
        {
            return;
        }

        vp.VideoPlayer.Pause();
        var currentIndex = Subtitles.IndexOf(selectedItems.First());
        if (currentIndex <= 0)
        {
            return;
        }

        var p = Subtitles[currentIndex - 1];
        SubtitleGrid.SelectedItem = p;
        vp.Position = p.StartTime.TotalSeconds;
        _playSelectionItem = new PlaySelectionItem(new List<SubtitleLineViewModel> { p }, p.EndTime, true);
        vp.VideoPlayer.Play();
    }

    [RelayCommand]
    private void RepeatNextLine()
    {
        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().OrderBy(p => p.StartTime).ToList();
        var vp = GetVideoPlayerControl();
        if (Window == null || selectedItems.Count == 0 || vp == null)
        {
            return;
        }

        vp.VideoPlayer.Pause();
        var currentIndex = Subtitles.IndexOf(selectedItems.First());
        if (currentIndex >= Subtitles.Count - 1)
        {
            return;
        }

        var p = Subtitles[currentIndex + 1];
        SubtitleGrid.SelectedItem = p;
        vp.Position = p.StartTime.TotalSeconds;
        _playSelectionItem = new PlaySelectionItem(new List<SubtitleLineViewModel> { p }, p.EndTime, true);
        vp.VideoPlayer.Play();
    }

    [RelayCommand]
    private void GoToNextLine()
    {
        var idx = SelectedSubtitleIndex ?? -1;
        if (Subtitles.Count == 0 || idx < 0 || idx >= Subtitles.Count)
        {
            return;
        }

        idx++;
        if (idx >= Subtitles.Count)
        {
            return;
        }

        SelectAndScrollToRow(idx);

        if (WaveformCenter)
        {
            var selectedLine = Subtitles.GetOrNull(idx);
            if (selectedLine != null)
            {
                AudioVisualizer?.CenterOnPosition(selectedLine);
                _updateAudioVisualizer = true;
            }
        }
    }

    [RelayCommand]
    private void GoToPreviousLine()
    {
        var idx = SelectedSubtitleIndex ?? -1;
        if (Subtitles.Count == 0 || idx < 0 || idx >= Subtitles.Count)
        {
            return;
        }

        idx--;
        if (idx < 0)
        {
            return;
        }

        SelectAndScrollToRow(idx);

        if (WaveformCenter)
        {
            var selectedLine = Subtitles.GetOrNull(idx);
            if (selectedLine != null)
            {
                AudioVisualizer?.CenterOnPosition(selectedLine);
                _updateAudioVisualizer = true;
            }
        }
    }

    [RelayCommand]
    private void GoToNextLineAndSetVideoPosition()
    {
        var idx = SelectedSubtitleIndex ?? -1;
        var vp = GetVideoPlayerControl();
        if (Subtitles.Count == 0 ||
            idx < 0 || idx - 2 >= Subtitles.Count ||
            string.IsNullOrEmpty(_videoFileName) ||
            vp == null)
        {
            return;
        }

        idx++;
        if (idx >= Subtitles.Count)
        {
            return;
        }

        SelectAndScrollToRow(idx);
        vp.Position = Subtitles[idx].StartTime.TotalSeconds;

        if (AudioVisualizer != null && AudioVisualizer.WavePeaks != null)
        {
            AudioVisualizerCenterOnPositionIfNeeded(vp.Position);
        }
    }

    [RelayCommand]
    private void GoToPreviousLineAndSetVideoPosition()
    {
        var vp = GetVideoPlayerControl();
        var idx = SelectedSubtitleIndex ?? -1;
        if (Subtitles.Count == 0 ||
            idx <= 0 || idx >= Subtitles.Count ||
            string.IsNullOrEmpty(_videoFileName) ||
            vp == null)
        {
            return;
        }

        idx--;
        if (idx < 0)
        {
            return;
        }

        vp.Position = Subtitles[idx].StartTime.TotalSeconds;
        SelectAndScrollToRow(idx);

        if (AudioVisualizer != null && AudioVisualizer.WavePeaks != null)
        {
            AudioVisualizerCenterOnPositionIfNeeded(vp.Position);
        }
    }

    [RelayCommand]
    private void GoToNextLineFromVideoPosition()
    {
        var vp = GetVideoPlayerControl();
        if (vp == null)
        {
            return;
        }

        // find first line after current video position with duration less than 10 seconds (to avoid jumping to very long lines like ASSA background effects)
        var firstLineAfterPosition = Subtitles.FirstOrDefault(s => s.StartTime.TotalSeconds > vp.Position && s.Duration.TotalSeconds < 10);

        if (firstLineAfterPosition == null)
        {
            // find first line after current video position regardless of duration
            firstLineAfterPosition = Subtitles.FirstOrDefault(s => s.StartTime.TotalSeconds > vp.Position);
        }

        if (firstLineAfterPosition == null)
        {
            return;
        }

        var idx = Subtitles.IndexOf(firstLineAfterPosition);
        var next = Subtitles.GetOrNull(idx);
        if (next == null)
        {
            return;
        }

        vp.Position = next.StartTime.TotalSeconds;
        SelectAndScrollToRow(idx);
        AudioVisualizerCenterOnPositionIfNeeded(next, next.StartTime.TotalSeconds);
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void GoToPreviousLineFromVideoPosition()
    {
        var vp = GetVideoPlayerControl();
        if (vp == null)
        {
            return;
        }

        // find first line before current video position with duration less than 10 seconds (to avoid jumping to very long lines like ASSA background effects)
        var firstLineBeforePosition = Subtitles.LastOrDefault(s => s.StartTime.TotalSeconds < vp.Position - 0.001 && s.Duration.TotalSeconds < 10);
        if (firstLineBeforePosition == null)
        {
            // find first line before current video position regardless of duration
            firstLineBeforePosition = Subtitles.LastOrDefault(s => s.StartTime.TotalSeconds < vp.Position - 0.001);
        }

        if (firstLineBeforePosition == null)
        {
            return;
        }

        var idx = Subtitles.IndexOf(firstLineBeforePosition);
        var previous = Subtitles.GetOrNull(idx);
        if (previous == null)
        {
            return;
        }

        vp.Position = previous.StartTime.TotalSeconds;
        SelectAndScrollToRow(idx);
        AudioVisualizerCenterOnPositionIfNeeded(previous, previous.StartTime.TotalSeconds);
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void FocusSelectedLine()
    {
        var idx = SelectedSubtitleIndex ?? -1;
        if (Subtitles.Count == 0 || idx < 0 || idx >= Subtitles.Count)
        {
            return;
        }

        SelectAndScrollToRow(idx);
        AudioVisualizer?.CenterOnPosition(Subtitles[idx]);
    }

    [RelayCommand]
    private void PlayFromStartOfVideo()
    {
        var vp = GetVideoPlayerControl();
        if (string.IsNullOrEmpty(_videoFileName) || vp == null)
        {
            return;
        }

        vp.VideoPlayer.Stop();
        vp.VideoPlayer.Play();
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void RemoveBlankLines()
    {
        var blankLines = Subtitles.Where(s => string.IsNullOrWhiteSpace(s.Text)).ToList();
        var count = blankLines.Count;
        if (count == 0)
        {
            return;
        }

        foreach (var line in blankLines)
        {
            Subtitles.Remove(line);
        }

        Renumber();
        _updateAudioVisualizer = true;

        ShowStatus(string.Format(Se.Language.Main.RemovedXBlankLines, count));
    }

    private SubtitleLineViewModel? _setEndAtKeyUpLine;
    private bool _setEndAtKeyUpLineGoToNext = false;

    [RelayCommand]
    private void InsertSubtitleAtVideoPositionSetEndAtKeyUp()
    {
        if (_setEndAtKeyUpLine != null)
        {
            return;
        }

        var vp = GetVideoPlayerControl();
        if (vp == null || !vp.VideoPlayer.IsPlaying)
        {
            return;
        }

        var startMs = vp.Position * 1000.0;
        var endMs = startMs + Se.Settings.General.NewEmptyDefaultMs;
        var newParagraph =
            new SubtitleLineViewModel(new Paragraph(string.Empty, startMs, endMs), SelectedSubtitleFormat);
        var idx = _insertService.InsertInCorrectPosition(Subtitles, newParagraph);
        var next = Subtitles.GetOrNull(idx + 1);
        if (next != null)
        {
            if (next.StartTime.TotalMilliseconds < endMs)
            {
                newParagraph.EndTime = TimeSpan.FromMilliseconds(next.StartTime.TotalMilliseconds -
                                                                 Se.Settings.General.MinimumBetweenLines.GetMilliseconds());
            }
        }

        _setEndAtKeyUpLine = newParagraph;
        _setEndAtKeyUpLineGoToNext = false;
        SelectAndScrollToSubtitle(newParagraph);
        Renumber();
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void SetSubtitleStartAtVideoPositionSetEndAtKeyUpAndGoToNext()
    {
        var selectedSubtitle = SelectedSubtitle;
        if (selectedSubtitle == null)
        {
            return;
        }

        if (_setEndAtKeyUpLine != null)
        {
            return;
        }

        var vp = GetVideoPlayerControl();
        var idx = Subtitles.IndexOf(selectedSubtitle);
        if (vp == null || !vp.VideoPlayer.IsPlaying || idx < 0)
        {
            return;
        }

        var startMs = vp.Position * 1000.0;
        selectedSubtitle.StartTime = TimeSpan.FromMilliseconds(startMs);
        _setEndAtKeyUpLine = selectedSubtitle;
        _setEndAtKeyUpLineGoToNext = true;
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void VideoFullScreen()
    {
        var control = GetVideoPlayerControl();
        if (control == null || control.IsFullScreen || string.IsNullOrEmpty(_videoFileName))
        {
            return;
        }

        control.VideoPlayer.Pause();
        var position = control.Position;
        var volume = control.Volume;
        var parent = (Control)control.Parent!;

        _fullScreenVideoPlayerControl = InitVideoPlayer.MakeVideoPlayer();
        _fullScreenVideoPlayerControl.IsFullScreen = true;
        var toggleKeys = Se.Settings.Shortcuts
            .FirstOrDefault(s => s.ActionName == nameof(VideoFullScreenCommand))?.Keys;
        var showMediaInfoKeys = Se.Settings.Shortcuts
            .FirstOrDefault(s => s.ActionName == nameof(ShowMediaInformationCommand))?.Keys;
        Action<Window> showMediaInformationOwnedBy = ownerWindow =>
        {
            var mediaInfo = _mediaInfo;
            var fileName = _videoFileName ?? string.Empty;
            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }

            Dispatcher.UIThread.Post(async void () =>
            {
                await _windowService.ShowDialogAsync<MediaInfoViewWindow, MediaInfoViewViewModel>(
                    ownerWindow,
                    vm => vm.Initialize(fileName, mediaInfo));
            });
        };
        var fullScreenWindow = new FullScreenVideoWindow(_fullScreenVideoPlayerControl, _videoFileName, _subtitleFileName ?? string.Empty, position, volume, () =>
        {
            control!.Position = _fullScreenVideoPlayerControl.Position;
            control!.Volume = _fullScreenVideoPlayerControl.Volume;
            _fullScreenVideoPlayerControl = null;
        }, toggleKeys, showMediaInfoKeys, showMediaInformationOwnedBy);
        fullScreenWindow.Show(Window!);
        _shortcutManager.ClearKeys();

        var vp = GetVideoPlayerControl();
        if (vp != null)
        {
            if (vp.VideoPlayer is LibMpvDynamicPlayer mpv)
            {
                _mpvReloader.Reset();
                _mpvReloader.RefreshMpv(mpv, GetUpdateSubtitle(), _subtitleSecondary, SelectedSubtitleFormat);
            }
            else if (vp.VideoPlayer is LibVlcDynamicPlayer vlc)
            {
                _vlcReloader.Reset();
                _vlcReloader.RefreshVlc(vlc, GetUpdateSubtitle(), _subtitleSecondary, SelectedSubtitleFormat);
            }
        }

        RefreshSubtitlePreview();
    }

    [RelayCommand]
    private void ToggleVideoPlayerDisplayTimeLeft()
    {
        Se.Settings.Video.VideoPlayerDisplayTimeLeft = !Se.Settings.Video.VideoPlayerDisplayTimeLeft;

        var vp = GetVideoPlayerControl();
        if (vp != null)
        {
            vp.VideoPlayerDisplayTimeLeft = Se.Settings.Video.VideoPlayerDisplayTimeLeft;
        }
    }

    [RelayCommand]
    private void Unbreak()
    {
        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
        if (selectedItems.Count == 0)
        {
            return;
        }

        foreach (var s in selectedItems)
        {
            s.Text = Utilities.UnbreakLine(s.Text);
        }

        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void AutoBreak()
    {
        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
        if (selectedItems.Count == 0)
        {
            return;
        }

        foreach (var s in selectedItems)
        {
            s.Text = Utilities.AutoBreakLine(s.Text);
        }

        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void Split()
    {
        SplitSelectedLine(false, false);
    }

    [RelayCommand]
    private void SplitInWaveform()
    {
        var vp = GetVideoPlayerControl();
        var selectedLine = SelectedSubtitle;
        if (vp == null || selectedLine == null || AudioVisualizer == null)
        {
            return;
        }

        var videoPosition = vp.Position;
        if (videoPosition < selectedLine.StartTime.TotalSeconds + 0.3 ||
            videoPosition > selectedLine.EndTime.TotalSeconds - 0.3)
        {
            SplitSelectedLine(false, false);
            return;
        }

        SplitSelectedLine(true, false);
    }


    [RelayCommand]
    private void SplitAtPositionInWaveform()
    {
        var vp = GetVideoPlayerControl();
        if (vp == null || AudioVisualizer == null)
        {
            return;
        }

        var pos = vp.Position;
        var subtitlesAtPosition = Subtitles
            .Where(p =>
                p.StartTime.TotalSeconds < pos &&
                p.EndTime.TotalSeconds > pos).ToList();

        if (subtitlesAtPosition.Count != 1)
        {
            return;
        }

        var line = subtitlesAtPosition[0];

        var videoPosition = vp.Position;
        if (videoPosition < line.StartTime.TotalSeconds + 0.3 ||
            videoPosition > line.EndTime.TotalSeconds - 0.3)
        {
            SplitLine(false, false, line);
            return;
        }

        SplitLine(true, false, line);

        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void SplitAtVideoPosition()
    {
        SplitSelectedLine(true, false);
    }

    [RelayCommand]
    private void SplitAtTextBoxCursorPosition()
    {
        SplitSelectedLine(false, true);
    }

    [RelayCommand]
    private void SplitAtVideoPositionAndTextBoxCursorPosition()
    {
        SplitSelectedLine(true, true);
    }

    [RelayCommand]
    private void TextBoxRemoveAllFormatting()
    {
        var tb = EditTextBox;
        if (tb == null || tb.Text == null)
        {
            return;
        }

        var selectionStart = Math.Min(tb.SelectionStart, tb.SelectionEnd);
        var selectionEnd = Math.Max(tb.SelectionStart, tb.SelectionEnd);
        var selectionLength = selectionEnd - selectionStart;

        if (selectionLength == 0)
        {
            tb.Text = HtmlUtil.RemoveHtmlTags(tb.Text, true);
        }
        else
        {
            var selectedText = tb.Text.Substring(selectionStart, selectionLength);
            var newText = HtmlUtil.RemoveHtmlTags(selectedText, true);
            tb.Text = tb.Text
                .Remove(selectionStart, selectionLength)
                .Insert(selectionStart, newText);
            tb.SelectionStart = selectionStart;
            tb.SelectionEnd = selectionStart + newText.Length;
        }

        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void TextBoxBold()
    {
        var tb = EditTextBox;
        ToggleTextBoxTag(tb, "b", "b1", "b0");
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void TextBoxItalic()
    {
        var tb = EditTextBox;
        ToggleTextBoxTag(tb, "i", "i1", "i0");
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void TextBoxUnderline()
    {
        var tb = EditTextBox;
        ToggleTextBoxTag(tb, "u", "u1", "u0");
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private async Task TextBoxColor()
    {
        var tb = EditTextBox;
        if (tb == null || tb.Text == null)
        {
            return;
        }

        var result = await ShowDialogAsync<ColorPickerWindow, ColorPickerViewModel>();
        if (!result.OkPressed)
        {
            return;
        }

        SetTextBoxColor(tb, result.SelectedColor);
    }

    private void SetTextBoxColor(ITextBoxWrapper tb, Color color)
    {
        var selectionStart = Math.Min(tb.SelectionStart, tb.SelectionEnd);
        var selectionEnd = Math.Max(tb.SelectionStart, tb.SelectionEnd);
        var selectionLength = selectionEnd - selectionStart;
        var isAssa = SelectedSubtitleFormat is AdvancedSubStationAlpha;
        if (selectionLength == 0 || selectionLength == tb.Text.Length)
        {
            tb.Text = _colorService.SetColorTag(tb.Text, color, GetUpdateSubtitle(), SelectedSubtitleFormat);
        }
        else
        {
            var selectedText = tb.Text.Substring(selectionStart, selectionLength);
            selectedText = _colorService.SetColorTag(selectedText, color, GetUpdateSubtitle(), SelectedSubtitleFormat);

            if (isAssa) // close color tag (display normal style color)
            {
                var closeTag = "{\\c&HFFFFFF&}"; // white color
                var styleName = SelectedSubtitle?.Style;
                if (_subtitle != null && _subtitle.Header != null && styleName != null)
                {
                    var style = AdvancedSubStationAlpha.GetSsaStyle(styleName, _subtitle.Header);
                    var endColor =
                        _colorService.SetColorTag("x", style.Primary.ToAvaloniaColor(), _subtitle, SelectedSubtitleFormat);
                    closeTag = endColor.TrimEnd('x');
                }

                selectedText += closeTag;
            }

            tb.Text = tb.Text
                .Remove(selectionStart, selectionLength)
                .Insert(selectionStart, selectedText);

            Dispatcher.UIThread.Post(() =>
            {
                tb.Focus();
                tb.SelectionStart = selectionStart;
                tb.SelectionEnd = selectionStart + selectedText.Length;
            });

            _updateAudioVisualizer = true;
        }
    }

    [RelayCommand]
    private async Task TextBoxFontName()
    {
        var tb = EditTextBox;
        if (tb == null || tb.Text == null)
        {
            return;
        }

        var selectionStart = Math.Min(tb.SelectionStart, tb.SelectionEnd);
        var selectionEnd = Math.Max(tb.SelectionStart, tb.SelectionEnd);
        var selectionLength = selectionEnd - selectionStart;

        var result =
            await ShowDialogAsync<PickFontNameWindow, PickFontNameViewModel>(vm => { vm.Initialize(); });

        if (!result.OkPressed || result.SelectedFontName == null)
        {
            return;
        }

        var isAssa = SelectedSubtitleFormat is AdvancedSubStationAlpha;
        var isWebVtt = SelectedSubtitleFormat is WebVTT;
        if (selectionLength == 0)
        {
            tb.Text = _fontNameService.SetFontName(tb.Text, result.SelectedFontName, isAssa);
        }
        else
        {
            var selectedText = tb.Text.Substring(selectionStart, selectionLength);
            selectedText = _fontNameService.SetFontName(selectedText, result.SelectedFontName, isAssa);
            tb.Text = tb.Text
                .Remove(selectionStart, selectionLength)
                .Insert(selectionStart, selectedText);

            Dispatcher.UIThread.Post(() =>
            {
                tb.Focus();
                tb.SelectionStart = selectionStart;
                tb.SelectionEnd = selectionStart + selectedText.Length;
            });
        }
    }

    [RelayCommand]
    private void ZoomLayoutIn()
    {
        var newFactor = Math.Round(Se.Settings.Appearance.LayoutScale + UiTheme.ScaleStep, 1);
        UiTheme.SetLayoutScale(Math.Min(newFactor, UiTheme.MaxScale));
    }

    [RelayCommand]
    private void ZoomLayoutOut()
    {
        var newFactor = Math.Round(Se.Settings.Appearance.LayoutScale - UiTheme.ScaleStep, 1);
        UiTheme.SetLayoutScale(Math.Max(newFactor, UiTheme.MinScale));
    }

    [RelayCommand]
    private void VideoSetPositionCurrentSubtitleStart()
    {
        var s = SelectedSubtitle;
        var vp = GetVideoPlayerControl();
        if (s == null || vp == null)
        {
            return;
        }

        vp.Position = s.StartTime.TotalSeconds;

        if (AudioVisualizer != null && AudioVisualizer.WavePeaks != null)
        {
            AudioVisualizerCenterOnPositionIfNeeded(s.StartTime.TotalSeconds);
        }

        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void VideoSetPositionCurrentSubtitleEnd()
    {
        var s = SelectedSubtitle;
        var vp = GetVideoPlayerControl();
        if (s == null || vp == null)
        {
            return;
        }

        vp.Position = s.EndTime.TotalSeconds;

        if (AudioVisualizer != null && AudioVisualizer.WavePeaks != null)
        {
            AudioVisualizerCenterOnPositionIfNeeded(s.EndTime.TotalSeconds);
        }

        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void WaveformInsertNewSelection()
    {
        var vp = GetVideoPlayerControl();
        if (vp == null ||
            AudioVisualizer == null ||
            AudioVisualizer.NewSelectionParagraph == null)
        {
            return;
        }

        var newParagraph = AudioVisualizer.NewSelectionParagraph;
        _insertService.InsertInCorrectPosition(Subtitles, newParagraph);
        AudioVisualizer.NewSelectionParagraph = null;
        SelectAndScrollToSubtitle(newParagraph);
        FocusEditTextBox();
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private async Task WaveformSpeechToTextNewSelection()
    {
        var vp = GetVideoPlayerControl();
        if (vp == null ||
            AudioVisualizer == null ||
            AudioVisualizer.NewSelectionParagraph == null)
        {
            return;
        }

        var newParagraph = AudioVisualizer.NewSelectionParagraph;
        _insertService.InsertInCorrectPosition(Subtitles, newParagraph);
        AudioVisualizer.NewSelectionParagraph = null;
        SubtitleGrid.SelectedItem = newParagraph;
        SubtitleGrid.ScrollIntoView(newParagraph, null);
        _updateAudioVisualizer = true;

        await SpeechToTextSelectedLines();
    }

    [RelayCommand]
    private async Task WaveformNewSelectionPasteFromClipboard()
    {
        var vp = GetVideoPlayerControl();
        if (Window == null || Window.Clipboard == null || vp == null || AudioVisualizer == null)
        {
            return;
        }

        var newParagraph = AudioVisualizer.NewSelectionParagraph;
        if (newParagraph == null)
        {
            return;
        }

        var text = await ClipboardHelper.GetTextAsync(Window);
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        newParagraph.Text = text.Trim();
        _insertService.InsertInCorrectPosition(Subtitles, newParagraph);
        AudioVisualizer.NewSelectionParagraph = null;
        SelectAndScrollToSubtitle(newParagraph);
        Renumber();
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private async Task WaveformPasteFromClipboard()
    {
        var vp = GetVideoPlayerControl();
        if (Window == null || Window.Clipboard == null || vp == null || AudioVisualizer == null)
        {
            return;
        }

        var text = await ClipboardHelper.GetTextAsync(Window);
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        var linesInserted = _pasteFromClipboardHelper.PasteFromClipboard(text, vp.Position * 1000.0, Subtitles, SelectedSubtitleFormat);
        Renumber();
        if (linesInserted.Count == 1)
        {
            SelectAndScrollToSubtitle(linesInserted.First());
        }

        _updateAudioVisualizer = true;
    }

    private void FocusEditTextBox()
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (AudioVisualizer != null && AudioVisualizer.IsFocused)
            {
                AudioVisualizer.SkipNextPointerEntered = true;
            }

            EditTextBox.Focus();
            Task.Delay(10);
            EditTextBox.Focus();
        });
    }

    [RelayCommand]
    private void WaveformInsertAtPositionAndFocusTextBox()
    {
        var vp = GetVideoPlayerControl();
        if (vp == null || AudioVisualizer == null)
        {
            return;
        }

        var startMs = vp.Position * 1000.0;
        var endMs = startMs + Se.Settings.General.NewEmptyDefaultMs;
        var newParagraph =
            new SubtitleLineViewModel(new Paragraph(string.Empty, startMs, endMs), SelectedSubtitleFormat);

        RunWithoutChangeDetection(() =>
        {
            var idx = _insertService.InsertInCorrectPosition(Subtitles, newParagraph);
            var next = Subtitles.GetOrNull(idx + 1);
            if (next != null)
            {
                if (next.StartTime.TotalMilliseconds < endMs && next.StartTime.TotalMilliseconds > newParagraph.StartTime.TotalMilliseconds + 200)
                {
                    newParagraph.EndTime = TimeSpan.FromMilliseconds(next.StartTime.TotalMilliseconds -
                                                                     Se.Settings.General.MinimumBetweenLines.GetMilliseconds());
                }
            }
        });

        AudioVisualizer.NewSelectionParagraph = null;
        SelectAndScrollToSubtitle(newParagraph);
        FocusEditTextBox();
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void FocusTextBox()
    {
        FocusEditTextBox();
    }

    [RelayCommand]
    private void WaveformInsertAtPositionNoFocusTextBox()
    {
        var vp = GetVideoPlayerControl();
        if (vp == null || AudioVisualizer == null)
        {
            return;
        }

        var startMs = vp.Position * 1000.0;
        var endMs = startMs + Se.Settings.General.NewEmptyDefaultMs;
        var newParagraph =
            new SubtitleLineViewModel(new Paragraph(string.Empty, startMs, endMs), SelectedSubtitleFormat);

        RunWithoutChangeDetection(() =>
        {
            var idx = _insertService.InsertInCorrectPosition(Subtitles, newParagraph);
            var next = Subtitles.GetOrNull(idx + 1);
            if (next != null)
            {
                if (next.StartTime.TotalMilliseconds < endMs && next.StartTime.TotalMilliseconds > newParagraph.StartTime.TotalMilliseconds + 200)
                {
                    newParagraph.EndTime = TimeSpan.FromMilliseconds(next.StartTime.TotalMilliseconds -
                                                                     Se.Settings.General.MinimumBetweenLines.GetMilliseconds());
                }
            }

        });

        AudioVisualizer.NewSelectionParagraph = null;
        SelectAndScrollToSubtitle(newParagraph);
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void WaveformDeleteAtPosition()
    {
        var vp = GetVideoPlayerControl();
        if (vp == null || AudioVisualizer == null)
        {
            return;
        }

        var pos = vp.Position;
        var subtitlesAtPosition = Subtitles
            .Where(p =>
                p.StartTime.TotalSeconds < pos &&
                p.EndTime.TotalSeconds > pos).ToList();

        foreach (var p in subtitlesAtPosition)
        {
            Subtitles.Remove(p);
        }

        Renumber();

        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void WaveformSetStartAndOffsetTheRest()
    {
        var s = SelectedSubtitle;
        var vp = GetVideoPlayerControl();
        if (s == null || vp == null || LockTimeCodes)
        {
            return;
        }

        var videoPositionSeconds = vp.Position;
        var index = Subtitles.IndexOf(s);
        if (index < 0 || index >= Subtitles.Count)
        {
            return;
        }

        var videoStartTime = TimeSpan.FromSeconds(videoPositionSeconds);
        var subtitleStartTime = s.StartTime;
        var difference = videoStartTime - subtitleStartTime;

        RunWithoutChangeDetection(() =>
        {
            for (var i = index; i < Subtitles.Count; i++)
            {
                var subtitle = Subtitles[i];
                subtitle.StartTime += difference;
            }

            _updateAudioVisualizer = true;
        });
    }

    [RelayCommand]
    private void WaveformSetEndAndOffsetTheRest()
    {
        var s = SelectedSubtitle;
        var vp = GetVideoPlayerControl();
        if (s == null || vp == null || LockTimeCodes)
        {
            return;
        }

        var videoPositionSeconds = vp.Position;
        var index = Subtitles.IndexOf(s);
        if (index < 0 || index >= Subtitles.Count)
        {
            return;
        }

        var videoTime = TimeSpan.FromSeconds(videoPositionSeconds);
        var subtitleEndTime = s.EndTime;
        if (videoTime <= s.StartTime)
        {
            ShowStatus(Se.Language.Main.EndTimeMustBeAfterStartTime);
            return;
        }

        var difference = videoTime - subtitleEndTime;

        RunWithoutChangeDetection(() =>
        {
            for (var i = index; i < Subtitles.Count; i++)
            {
                var subtitle = Subtitles[i];
                subtitle.StartTime += difference;
            }

            _updateAudioVisualizer = true;
        });
    }

    [RelayCommand]
    private void WaveformSetStart()
    {
        var s = SelectedSubtitle;
        var vp = GetVideoPlayerControl();
        if (s == null || vp == null || LockTimeCodes)
        {
            return;
        }

        var videoPositionSeconds = vp.Position;
        var gap = Se.Settings.General.MinimumBetweenLines.GetMilliseconds() / 1000.0;
        if (videoPositionSeconds >= s.EndTime.TotalSeconds - gap)
        {
            return;
        }

        var isAssa = SelectedSubtitleFormat is AdvancedSubStationAlpha;
        if (isAssa)
        {
            var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
            foreach (var item in selectedItems)
            {
                item.SetStartTimeOnly(TimeSpan.FromSeconds(videoPositionSeconds));
            }
        }
        else
        {
            s.SetStartTimeOnly(TimeSpan.FromSeconds(videoPositionSeconds));
        }

        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void WaveformSetEnd()
    {
        var s = SelectedSubtitle;
        var vp = GetVideoPlayerControl();
        if (s == null || vp == null || LockTimeCodes)
        {
            return;
        }

        var videoPositionSeconds = vp.Position;
        var gap = Se.Settings.General.MinimumBetweenLines.GetMilliseconds() / 1000.0;
        if (videoPositionSeconds < s.StartTime.TotalSeconds + gap)
        {
            return;
        }

        var isAssa = SelectedSubtitleFormat is AdvancedSubStationAlpha;
        if (isAssa)
        {
            var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
            foreach (var item in selectedItems)
            {
                item.EndTime = TimeSpan.FromSeconds(videoPositionSeconds);
            }
        }
        else
        {
            s.EndTime = TimeSpan.FromSeconds(videoPositionSeconds);
        }

        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void WaveformSetEndAndGoToNext()
    {
        var s = SelectedSubtitle;
        var vp = GetVideoPlayerControl();
        if (s == null || vp == null || LockTimeCodes)
        {
            return;
        }

        var idx = Subtitles.IndexOf(s);
        if (idx < 0)
        {
            return;
        }

        var videoPositionSeconds = vp.Position;
        var gap = Se.Settings.General.MinimumBetweenLines.GetMilliseconds() / 1000.0;
        if (videoPositionSeconds < s.StartTime.TotalSeconds + gap)
        {
            return;
        }

        s.EndTime = TimeSpan.FromSeconds(videoPositionSeconds);

        SelectAndScrollToRow(idx + 1);

        _updateAudioVisualizer = true;
    }


    [RelayCommand]
    private void WaveformSetEndAndStartOfNextAfterGap()
    {
        var s = SelectedSubtitle;
        var vp = GetVideoPlayerControl();
        if (s == null || AudioVisualizer?.WavePeaks == null || vp == null || LockTimeCodes)
        {
            return;
        }

        var idx = Subtitles.IndexOf(s);
        var next = Subtitles.GetOrNull(idx + 1);
        if (next == null)
        {
            return;
        }

        var videoPositionSeconds = vp.Position;
        var gapMs = Se.Settings.General.MinimumBetweenLines.GetMilliseconds();
        if (videoPositionSeconds < s.StartTime.TotalSeconds + 0.001)
        {
            return;
        }

        s.EndTime = TimeSpan.FromSeconds(videoPositionSeconds);
        var nextStart = s.EndTime.TotalMilliseconds + gapMs;
        next.SetStartTimeOnly(TimeSpan.FromMilliseconds(nextStart));

        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void WaveformSetEndAndStartOfNextAfterGapAndGoToNext()
    {
        var s = SelectedSubtitle;
        var vp = GetVideoPlayerControl();
        if (s == null || AudioVisualizer?.WavePeaks == null || vp == null || LockTimeCodes)
        {
            return;
        }

        var idx = Subtitles.IndexOf(s);
        var next = Subtitles.GetOrNull(idx + 1);
        if (next == null)
        {
            return;
        }

        var videoPositionSeconds = vp.Position;
        var gapMs = Se.Settings.General.MinimumBetweenLines.GetMilliseconds();
        if (videoPositionSeconds < s.StartTime.TotalSeconds + 0.001)
        {
            return;
        }

        s.EndTime = TimeSpan.FromSeconds(videoPositionSeconds);
        var nextStart = s.EndTime.TotalMilliseconds + gapMs;
        next.SetStartTimeOnly(TimeSpan.FromMilliseconds(nextStart));

        SelectAndScrollToRow(idx + 1);

        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void WaveformSetStartAndSetEndOfPreviousMinusGap()
    {
        var s = SelectedSubtitle;
        var vp = GetVideoPlayerControl();
        if (s == null || AudioVisualizer?.WavePeaks == null || vp == null || LockTimeCodes)
        {
            return;
        }

        var idx = Subtitles.IndexOf(s);
        var previous = Subtitles.GetOrNull(idx - 1);
        if (previous == null)
        {
            return;
        }

        var videoPositionSeconds = vp.Position;
        var gapMs = Se.Settings.General.MinimumBetweenLines.GetMilliseconds();
        if (videoPositionSeconds > s.EndTime.TotalSeconds - 0.001)
        {
            return;
        }

        s.SetStartTimeOnly(TimeSpan.FromSeconds(videoPositionSeconds));
        var previousEnd = s.StartTime.TotalMilliseconds - gapMs;
        previous.EndTime = TimeSpan.FromMilliseconds(previousEnd);

        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void HideWaveformToolbar()
    {
        IsWaveformToolbarVisible = false;
        Se.Settings.Waveform.ShowToolbar = false;
    }


    [RelayCommand]
    private void ResetWaveformZoomAndSpeed()
    {
        var vp = GetVideoPlayerControl();
        if (AudioVisualizer == null || vp == null)
        {
            return;
        }

        vp.SetSpeed(1.0);
        SelectedSpeed = Speeds.FirstOrDefault(p => p == "1.0x") ?? Speeds[2];
        AudioVisualizer.ZoomFactor = 1.0;
        AudioVisualizer.VerticalZoomFactor = 1.0;
    }

    [RelayCommand]
    private void WaveformVerticalZoomIn()
    {
        if (AudioVisualizer == null)
        {
            return;
        }

        AudioVisualizer.VerticalZoomFactor = Math.Max(Math.Min(AudioVisualizer.VerticalZoomFactor - 0.1, AudioVisualizer.MaxZoomFactor), AudioVisualizer.MinZoomFactor);
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void WaveformVerticalZoomOut()
    {
        if (AudioVisualizer == null)
        {
            return;
        }

        AudioVisualizer.VerticalZoomFactor = Math.Max(Math.Min(AudioVisualizer.VerticalZoomFactor + 0.1, AudioVisualizer.MaxZoomFactor), AudioVisualizer.MinZoomFactor);
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void WaveformHorizontalZoomIn()
    {
        if (AudioVisualizer == null)
        {
            return;
        }

        AudioVisualizer.ZoomFactor = Math.Max(Math.Min(AudioVisualizer.ZoomFactor - 0.1, AudioVisualizer.MaxZoomFactor), AudioVisualizer.MinZoomFactor);
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void WaveformHorizontalZoomOut()
    {
        if (AudioVisualizer == null)
        {
            return;
        }

        AudioVisualizer.ZoomFactor = Math.Max(Math.Min(AudioVisualizer.ZoomFactor + 0.1, AudioVisualizer.MaxZoomFactor), AudioVisualizer.MinZoomFactor);
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void TogglePlaybackSpeed()
    {
        var vp = GetVideoPlayerControl();
        if (AudioVisualizer == null || vp == null)
        {
            return;
        }

        var idx = Speeds.IndexOf(SelectedSpeed);
        if (idx < Speeds.Count - 1)
        {
            idx++;
        }
        else
        {
            idx = 0;
        }

        SelectedSpeed = Speeds[idx];
        ShowStatus(string.Format(Se.Language.Main.SpeedIsNowX, SelectedSpeed));
    }

    [RelayCommand]
    private void PlaybackSlower()
    {
        var vp = GetVideoPlayerControl();
        if (AudioVisualizer == null || vp == null)
        {
            return;
        }

        var idx = Speeds.IndexOf(SelectedSpeed);
        if (idx > 0)
        {
            idx--;
            SelectedSpeed = Speeds[idx];
            ShowStatus(string.Format(Se.Language.Main.SpeedIsNowX, SelectedSpeed));
        }
    }

    [RelayCommand]
    private void PlaybackFaster()
    {
        var vp = GetVideoPlayerControl();
        if (AudioVisualizer == null || vp == null)
        {
            return;
        }

        var idx = Speeds.IndexOf(SelectedSpeed);
        if (idx < Speeds.Count - 1)
        {
            idx++;
            SelectedSpeed = Speeds[idx];
            ShowStatus(string.Format(Se.Language.Main.SpeedIsNowX, SelectedSpeed));
        }
    }

    [RelayCommand]
    private void FetchFirstWordFromNextSubtitle()
    {
        var s = SelectedSubtitle;
        if (s == null)
        {
            return;
        }

        var idx = Subtitles.IndexOf(s);
        var next = Subtitles.GetOrNull(idx + 1);
        if (next == null)
        {
            return;
        }

        var currentText = s.Text.Trim();
        var nextText = next.Text.Trim();

        var upDown = new MoveWordUpDown(currentText, nextText);
        upDown.MoveWordUp();

        s.Text = upDown.S1;
        next.Text = upDown.S2;

        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void MoveLastWordToNextSubtitle()
    {
        var s = SelectedSubtitle;
        if (s == null)
        {
            return;
        }

        var idx = Subtitles.IndexOf(s);
        var next = Subtitles.GetOrNull(idx + 1);
        if (next == null)
        {
            return;
        }

        var currentText = s.Text.Trim();
        var nextText = next.Text.Trim();

        var upDown = new MoveWordUpDown(currentText, nextText);
        upDown.MoveWordDown();

        s.Text = upDown.S1;
        next.Text = upDown.S2;

        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void MoveLastWordFromFirstLineDownCurrentSubtitle()
    {
        var s = SelectedSubtitle;
        if (s == null)
        {
            return;
        }

        var lines = s.Text.SplitToLines();
        if (!string.IsNullOrWhiteSpace(s.Text) && lines.Count == 1)
        {
            lines.Add(string.Empty);
        }

        if (lines.Count != 2)
        {
            return;
        }

        var currentText = lines[0].Trim();
        var nextText = lines[1].Trim();

        var upDown = new MoveWordUpDown(currentText, nextText);
        upDown.MoveWordDown();

        s.Text = upDown.S1 + Environment.NewLine + upDown.S2;

        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void MoveFirstWordFromNextLineUpCurrentSubtitle()
    {
        var s = SelectedSubtitle;
        if (s == null)
        {
            return;
        }

        var lines = s.Text.SplitToLines();
        if (!string.IsNullOrWhiteSpace(s.Text) && lines.Count == 1)
        {
            lines.Add(string.Empty);
        }

        if (lines.Count != 2)
        {
            return;
        }

        var currentText = lines[0].Trim();
        var nextText = lines[1].Trim();

        var upDown = new MoveWordUpDown(currentText, nextText);
        upDown.MoveWordUp();

        s.Text = upDown.S1 + Environment.NewLine + upDown.S2;

        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void ToggleFocusGridAndWaveform()
    {
        if (AudioVisualizer == null)
        {
            return;
        }

        if (IsSubtitleGridFocused())
        {
            AudioVisualizer.Focus();
        }
        else
        {
            if (AudioVisualizer.IsFocused)
            {
                AudioVisualizer.SkipNextPointerEntered = true;
            }

            SubtitleGrid.Focus();
        }

        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void ToggleFocusTextBoxAndWaveform()
    {
        if (AudioVisualizer == null)
        {
            return;
        }

        if (AudioVisualizer.IsFocused)
        {
            FocusEditTextBox();
        }
        else if (EditTextBox.IsFocused)
        {
            if (AudioVisualizer.IsFocused)
            {
                AudioVisualizer.SkipNextPointerEntered = true;
            }

            AudioVisualizer.Focus();
        }

        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void ToggleFocusTextBoxAndSubtitleGrid()
    {
        if (AudioVisualizer != null && AudioVisualizer.IsFocused)
        {
            AudioVisualizer.SkipNextPointerEntered = true;
        }

        if (IsSubtitleGridFocused())
        {
            FocusEditTextBox();
        }
        else
        {
            SubtitleGrid.Focus();
        }
    }

    [RelayCommand]
    private void VideoOneSecondBack()
    {
        MoveVideoPositionMs(-1000);
    }

    [RelayCommand]
    private void VideoOneSecondForward()
    {
        MoveVideoPositionMs(1000);
    }

    [RelayCommand]
    private void Video500MsBack()
    {
        MoveVideoPositionMs(-500);
    }

    [RelayCommand]
    private void Video500MsForward()
    {
        MoveVideoPositionMs(500);
    }

    [RelayCommand]
    private void Video100MsBack()
    {
        MoveVideoPositionMs(-100);
    }

    [RelayCommand]
    private void Video100MsForward()
    {
        MoveVideoPositionMs(100);
    }

    [RelayCommand]
    private void VideoToggleBrightness()
    {
        var vp = GetVideoPlayerControl();
        if (vp?.VideoPlayer is not LibMpvDynamicPlayer mpv)
        {
            return;
        }

        var value = mpv.ToggleBrightness();
        ShowStatus(string.Format(Se.Language.Main.VideoBrightnessSetTo, value));
    }

    [RelayCommand]
    private void VideoOneFrameBack()
    {
        var vp = GetVideoPlayerControl();
        if (vp != null && vp.VideoPlayer is LibMpvDynamicPlayer mpv)
        {
            mpv.StepOneFrameBack();
            return;
        }

        if (Se.Settings.General.CurrentFrameRate >= 10)
        {
            var frameInMs = (int)Math.Round(1000.0 / Se.Settings.General.CurrentFrameRate, MidpointRounding.AwayFromZero);
            MoveVideoPositionMs(-frameInMs);
            return;
        }

        MoveVideoPositionMs(-40);
    }

    [RelayCommand]
    private void VideoOneFrameForward()
    {
        var vp = GetVideoPlayerControl();
        if (vp != null && vp.VideoPlayer is LibMpvDynamicPlayer mpv)
        {
            mpv.StepOneFrameForward();
            return;
        }

        if (Se.Settings.General.CurrentFrameRate >= 10)
        {
            var frameInMs = (int)Math.Round(1000.0 / Se.Settings.General.CurrentFrameRate, MidpointRounding.AwayFromZero);
            MoveVideoPositionMs(frameInMs);
            return;
        }

        MoveVideoPositionMs(40);
    }

    [RelayCommand]
    private void VideoMoveCustom1Back()
    {
        MoveVideoPositionMs(-Se.Settings.Video.MoveVideoPositionCustom1Back);
    }

    [RelayCommand]
    private void VideoMoveCustom1Forward()
    {
        MoveVideoPositionMs(Se.Settings.Video.MoveVideoPositionCustom1Forward);
    }

    [RelayCommand]
    private void VideoMoveCustom2Back()
    {
        MoveVideoPositionMs(-Se.Settings.Video.MoveVideoPositionCustom2Back);
    }

    [RelayCommand]
    private void VideoMoveCustom2Forward()
    {
        MoveVideoPositionMs(Se.Settings.Video.MoveVideoPositionCustom2Forward);
    }

    [RelayCommand]
    private void ExtendSelectedToPrevious()
    {
        var s = SelectedSubtitle;
        var idx = SelectedSubtitleIndex;
        if (s == null || idx == null || idx == 0 || LockTimeCodes)
        {
            return;
        }

        var prev = Subtitles[idx.Value - 1];
        s.SetStartTimeOnly(TimeSpan.FromMilliseconds(prev.EndTime.TotalMilliseconds + Se.Settings.General.MinimumBetweenLines.GetMilliseconds()));
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void ExtendSelectedToNext()
    {
        var s = SelectedSubtitle;
        var idx = SelectedSubtitleIndex;
        if (s == null || idx == null || idx >= Subtitles.Count - 1 || LockTimeCodes)
        {
            return;
        }

        var next = Subtitles.GetOrNull(idx.Value + 1);
        if (next == null)
        {
            return;
        }

        ;
        s.EndTime = TimeSpan.FromMilliseconds(next.StartTime.TotalMilliseconds -
                                              Se.Settings.General.MinimumBetweenLines.GetMilliseconds());
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void TextBoxCut()
    {
        EditTextBox.Cut();
    }

    [RelayCommand]
    private void TextBoxCut2()
    {
        EditTextBox.Cut();
    }

    [RelayCommand]
    private void TextBoxCopy()
    {
        EditTextBox.Copy();
    }

    [RelayCommand]
    private void TextBoxPaste()
    {
        EditTextBox.Paste();
    }

    [RelayCommand]
    private void TextBoxSelectAll()
    {
        EditTextBox.SelectAll();
    }

    [RelayCommand]
    private void TextBoxInsertUnicodeSymbol(object? commandParameter)
    {
        if (commandParameter is string s)
        {
            EditTextBox.SelectedText = s;
            EditTextBox.SelectionLength = 0;
        }
    }

    [RelayCommand]
    private void TextBoxDeleteSelection()
    {
        EditTextBox.SelectedText = string.Empty;
    }

    [RelayCommand]
    private async Task SubtitleGridCut()
    {
        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
        if (selectedItems.Count == 0 || Window == null)
        {
            return;
        }

        await SubtitleGridCopyPasteHelper.Cut(Window, Subtitles, selectedItems, SelectedSubtitleFormat, _subtitle);
        Renumber();
        _updateAudioVisualizer = true;
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task SubtitleGridCopy()
    {
        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
        if (selectedItems.Count == 0 || Window == null)
        {
            return;
        }

        await SubtitleGridCopyPasteHelper.Copy(Window, selectedItems, SelectedSubtitleFormat, _subtitle);
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task SubtitleGridPaste()
    {
        var idx = SelectedSubtitleIndex ?? -1;
        if (idx < 0 || Window == null)
        {
            return;
        }

        await SubtitleGridCopyPasteHelper.Paste(Window, Subtitles, idx, SelectedSubtitleFormat);
        Renumber();
        _updateAudioVisualizer = true;
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private void TrimWhitespaceSelectedLines()
    {
        var countOfTrimmedLines = 0;

        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
        var languageCode = LanguageAutoDetect.AutoDetectGoogleLanguage(GetUpdateSubtitle());
        foreach (var s in selectedItems)
        {
            var originalText = s.Text;
            s.Text = Utilities.RemoveUnneededSpaces(originalText, languageCode).Trim();
            if (!originalText.Equals(s.Text, StringComparison.Ordinal))
            {
                countOfTrimmedLines++;
            }
        }

        if (countOfTrimmedLines > 0)
        {
            ShowStatus(string.Format(Se.Language.Main.TrimmedXLines, countOfTrimmedLines));
            _updateAudioVisualizer = true;
        }
    }

    [RelayCommand]
    private async Task SetNewStyleForSelectedLines(string styleName)
    {
        var result = await ShowDialogAsync<PromptTextBoxWindow, PromptTextBoxViewModel>(vm =>
        {
            vm.Initialize(Se.Language.General.Style + " - " + Se.Language.General.New, string.Empty, 250, 20, true);
        });

        if (result.OkPressed && !string.IsNullOrWhiteSpace(result.Text))
        {
            _subtitle ??= new Subtitle();

            var header = _subtitle?.Header ?? string.Empty;
            if (header != null && header.Contains("http://www.w3.org/ns/ttml"))
            {
                var s = new Subtitle { Header = header };
                AdvancedSubStationAlpha.LoadStylesFromTimedText10(s, string.Empty, header,
                    AdvancedSubStationAlpha.HeaderNoStyles, new StringBuilder());
                header = s.Header;
            }
            else if (header != null && header.StartsWith("WEBVTT", StringComparison.Ordinal))
            {
                _subtitle = WebVttToAssa.Convert(_subtitle, new SsaStyle(), 0, 0);
                header = _subtitle.Header;
            }

            var defaultHeader = GetDefaultAssaHeader();
            if (header == null || !header.Contains("style:", StringComparison.OrdinalIgnoreCase))
            {
                header = defaultHeader;
            }

            var styles = AdvancedSubStationAlpha.GetSsaStylesFromHeader(header);
            var newStyle = AdvancedSubStationAlpha.GetSsaStylesFromHeader(defaultHeader).First();

            newStyle.Name = result.Text.Trim();

            // ensure unique style name
            var idx = 1;
            while (styles.Any(s => s.Name.Equals(newStyle.Name, StringComparison.OrdinalIgnoreCase)))
            {
                idx++;
                newStyle.Name = $"{result.Text.Trim()}_{idx}";
            }

            styles.Add(newStyle);
            header = AdvancedSubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(header, styles);
            _subtitle!.Header = header;

            SetStyleForSelectedLines(newStyle.Name);
        }
    }

    private static string GetDefaultAssaHeader()
    {
        var format = new AdvancedSubStationAlpha();
        var sub = new Subtitle();
        var text = format.ToText(sub, string.Empty);
        var lines = text.SplitToLines();
        format.LoadSubtitle(sub, lines, string.Empty);
        return sub.Header;
    }

    [RelayCommand]
    private void SetStyleForSelectedLines(string styleName)
    {
        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();

        Dispatcher.UIThread.Post(() =>
        {
            foreach (var p in selectedItems)
            {
                p.Style = styleName;
            }

            RefreshSubtitlePreview();
        });
    }

    [RelayCommand]
    private async Task SetNewActorForSelectedLines(string styleName)
    {
        var result = await ShowDialogAsync<PromptTextBoxWindow, PromptTextBoxViewModel>(vm =>
        {
            vm.Initialize(Se.Language.General.Actor + " - " + Se.Language.General.New, string.Empty, 250, 20, true);
        });

        if (result.OkPressed && !string.IsNullOrWhiteSpace(result.Text))
        {
            SetActorForSelectedLines(result.Text);
        }
    }

    [RelayCommand]
    private void SetActorForSelectedLines(string actorName)
    {
        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();

        Dispatcher.UIThread.Post(() =>
        {
            foreach (var p in selectedItems)
            {
                p.Actor = actorName;
            }
        });
    }

    [RelayCommand]
    private void SetActor1() => SetActorForSelectedLines(Se.Settings.Actor1);

    [RelayCommand]
    private void SetActor2() => SetActorForSelectedLines(Se.Settings.Actor2);

    [RelayCommand]
    private void SetActor3() => SetActorForSelectedLines(Se.Settings.Actor3);

    [RelayCommand]
    private void SetActor4() => SetActorForSelectedLines(Se.Settings.Actor4);

    [RelayCommand]
    private void SetActor5() => SetActorForSelectedLines(Se.Settings.Actor5);

    [RelayCommand]
    private void SetActor6() => SetActorForSelectedLines(Se.Settings.Actor6);

    [RelayCommand]
    private void SetActor7() => SetActorForSelectedLines(Se.Settings.Actor7);

    [RelayCommand]
    private void SetActor8() => SetActorForSelectedLines(Se.Settings.Actor8);

    [RelayCommand]
    private void SetActor9() => SetActorForSelectedLines(Se.Settings.Actor9);

    [RelayCommand]
    private void SetActor10() => SetActorForSelectedLines(Se.Settings.Actor10);

    [RelayCommand]
    private async Task SetNewActor()
    {
        var result = await ShowDialogAsync<PromptTextBoxWindow, PromptTextBoxViewModel>(vm =>
        {
            vm.Initialize(Se.Language.General.Actor + " - " + Se.Language.General.New, string.Empty, 250, 20, true);
        });

        if (result.OkPressed && !string.IsNullOrWhiteSpace(result.Text))
        {
            SetActorForSelectedLines(result.Text);
        }
    }

    private async Task<TViewModel> ShowDialogAsync<TWindow, TViewModel>(
        Action<TViewModel>? configureViewModel = null, Action<TWindow>? configureWindow = null)
        where TWindow : Window
        where TViewModel : class
    {
        GetVideoPlayerControl()?.VideoPlayer.Pause();
        var result = await _windowService.ShowDialogAsync<TWindow, TViewModel>(Window!, configureViewModel, configureWindow);
        _shortcutManager.ClearKeys();
        return result;
    }

    private void SplitSelectedLine(bool atVideoPosition, bool atTextBoxPosition)
    {
        var s = SelectedSubtitle;
        SplitLine(atVideoPosition, atTextBoxPosition, s);
    }

    private void SplitLine(bool atVideoPosition, bool atTextBoxPosition, SubtitleLineViewModel? s)
    {
        var vp = GetVideoPlayerControl();
        if (s == null || EditTextBox == null)
        {
            return;
        }

        var language = LanguageAutoDetect.AutoDetectGoogleLanguage(GetUpdateSubtitle());
        RunWithoutChangeDetection(() =>
        {
            if (atVideoPosition && atTextBoxPosition && vp != null)
            {
                _splitManager.Split(Subtitles, s, vp.Position, EditTextBox.SelectionStart, language);
            }
            else if (atVideoPosition && vp != null)
            {
                _splitManager.Split(Subtitles, s, vp.Position, language);
            }
            else if (atTextBoxPosition)
            {
                _splitManager.Split(Subtitles, s, EditTextBox.SelectionStart, language);
            }
            else
            {
                _splitManager.Split(Subtitles, s, language);
            }

            Renumber();
        });
    }

    private void MoveVideoPositionMs(int ms)
    {
        var vp = GetVideoPlayerControl();
        if (vp == null || string.IsNullOrEmpty(_videoFileName) || AudioVisualizer == null)
        {
            return;
        }

        var newPosition = vp.Position + (ms / 1000.0);
        if (newPosition < 0)
        {
            newPosition = 0;
        }

        if (newPosition > vp.Duration)
        {
            newPosition = vp.Duration;
        }

        vp.Position = newPosition;

        if (vp.IsPlaying)
        {
            return;
        }

        var av = AudioVisualizer;
        if (WaveformCenter)
        {
            var waveformHalfSeconds = (av.EndPositionSeconds - av.StartPositionSeconds) / 2.0;
            av.StartPositionSeconds = newPosition - waveformHalfSeconds;
        }
        else
        {
            if (newPosition <= av.StartPositionSeconds)
            {
                av.StartPositionSeconds = Math.Max(0, newPosition - 0.1);
            }
            else if (newPosition >= av.EndPositionSeconds)
            {
                var waveformWindowSeconds = av.EndPositionSeconds - av.StartPositionSeconds;
                av.StartPositionSeconds = Math.Min(newPosition - waveformWindowSeconds + 0.1, vp.Duration);
            }
        }

        _updateAudioVisualizer = true;
    }

    private async Task<bool> RequireFfmpegOk()
    {
        if (FfmpegHelper.IsFfmpegInstalled())
        {
            return true;
        }

        if (File.Exists(DownloadFfmpegViewModel.GetFfmpegFileName()))
        {
            Se.Settings.General.FfmpegPath = DownloadFfmpegViewModel.GetFfmpegFileName();
            return true;
        }

        if (!OperatingSystem.IsWindows() && File.Exists("/usr/local/bin/ffmpeg"))
        {
            Se.Settings.General.FfmpegPath = "/usr/local/bin/ffmpeg";
            return true;
        }

        if (OperatingSystem.IsWindows() || OperatingSystem.IsMacOS())
        {
            var answer = await MessageBox.Show(
                Window!,
                Se.Language.Main.DownloadFfmpegTitle,
                Se.Language.Main.DownloadFfmpegQuestion,
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (answer != MessageBoxResult.Yes)
            {
                return false;
            }

            var result = await ShowDialogAsync<DownloadFfmpegWindow, DownloadFfmpegViewModel>();
            if (!string.IsNullOrEmpty(result.FfmpegFileName))
            {
                Se.Settings.General.FfmpegPath = result.FfmpegFileName;
                ShowStatus(string.Format(Se.Language.Main.FfmpegDownloadedAndInstalledToX, result.FfmpegFileName));
                return true;
            }
        }

        return false;
    }

    private void PerformUndo()
    {
        if (!_undoRedoManager.CanUndo)
        {
            return;
        }

        RunWithoutChangeDetection(() =>
        {
            var undoRedoObject = _undoRedoManager.Undo()!;
            if (undoRedoObject?.Subtitles == null)
                return;

            RestoreUndoRedoState(undoRedoObject);
            ShowUndoStatus();
        });
    }

    private void ShowUndoStatus()
    {
        if (_undoRedoManager.CanUndo)
        {
            ShowStatus(string.Format(Se.Language.Main.UndoPerformedXActionLeft, _undoRedoManager.UndoList.Count));
        }
        else
        {
            ShowStatus(Se.Language.Main.UndoPerformed);
        }
    }

    private void ShowRedoStatus()
    {
        if (_undoRedoManager.CanRedo)
        {
            ShowStatus(string.Format(Se.Language.Main.RedoPerformedXActionLeft, _undoRedoManager.RedoList.Count));
        }
        else
        {
            ShowStatus(Se.Language.Main.RedoPerformed);
        }
    }

    private void PerformRedo()
    {
        if (!_undoRedoManager.CanRedo)
        {
            return;
        }

        RunWithoutChangeDetection(() =>
        {
            var undoRedoObject = _undoRedoManager.Redo();
            if (undoRedoObject?.Subtitles == null)
            {
                return;
            }

            RestoreUndoRedoState(undoRedoObject);
            ShowRedoStatus();
        });
    }

    public UndoRedoItem MakeUndoRedoObject(string description)
    {
        return new UndoRedoItem(
            description,
            Subtitles.Select(p => new SubtitleLineViewModel(p)).ToArray(),
            GetFastHash(),
            _subtitleFileName,
            [SelectedSubtitleIndex ?? 0],
            1,
            1)
        {
            SelectedEncodingDisplayName = SelectedEncoding.DisplayName,
            SubtitleHeader = _subtitle.Header,
            SubtitleFooter = _subtitle.Footer,
        };
    }

    private void RestoreUndoRedoState(UndoRedoItem undoRedoObject)
    {
        Subtitles.Clear();
        foreach (var p in undoRedoObject.Subtitles)
        {
            Subtitles.Add(p);
        }

        _subtitleFileName = undoRedoObject.SubtitleFileName;
        if (!string.IsNullOrEmpty(undoRedoObject.SelectedEncodingDisplayName))
        {
            var restoredEncoding = Encodings.FirstOrDefault(p => p.DisplayName == undoRedoObject.SelectedEncodingDisplayName);
            if (restoredEncoding != null)
            {
                SelectedEncoding = restoredEncoding;
            }
        }

        _subtitle.Header = undoRedoObject.SubtitleHeader;
        _subtitle.Footer = undoRedoObject.SubtitleFooter;
        SelectAndScrollToRow(undoRedoObject.SelectedLines.First());
    }

    public void AutoFitColumns()
    {
        var columns = SubtitleGrid.Columns.Where(p => p.IsVisible).ToList();

        var showHideWidth = MeasureShowHideColumnWidth();

        var numberOfStarColumns = 0;
        for (var i = 0; i < columns.Count; i++)
        {
            var column = columns[i];

            var originalWidth = column.Width;

            if (column.Header.ToString() == Se.Language.General.Show ||
                column.Header.ToString() == Se.Language.General.Hide)
            {
                var width = Math.Max(column.MinWidth, showHideWidth);
                column.Width = new DataGridLength(width, DataGridLengthUnitType.Pixel);
                continue;
            }
            else
            {
                column.Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
            }

            SubtitleGrid.UpdateLayout();

            if (column.Header.ToString() == Se.Language.General.OriginalText ||
                column.Header.ToString() == Se.Language.General.Text)
            {
                column.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                numberOfStarColumns++;
            }
            else
            {
                column.Width = originalWidth;
            }

            if (i == columns.Count - 1)
            {
                if (numberOfStarColumns == 0)
                {
                    column.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                }
                else if (numberOfStarColumns == 1 && column.Width.IsStar)
                {
                }
                else
                {
                    if (column.Header.ToString() != Se.Language.General.OriginalText &&
                        column.Header.ToString() != Se.Language.General.Text)
                    {
                        column.Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
                    }
                }
            }
        }

        SubtitleGrid.UpdateLayout();
    }

    private double MeasureShowHideColumnWidth()
    {
        // Use "8" digits — typically the widest digit glyph in proportional fonts.
        var sample = Se.Settings.General.UseFrameMode ? "88:88:88.88" : "88:88:88,888";
        if (Se.Settings.General.CurrentVideoOffsetInMs < 0)
        {
            sample = "-" + sample;
        }

        var fontFamily = SubtitleGrid.FontFamily ?? FontFamily.Default;
        var typeface = new Typeface(fontFamily);
        var fontSize = SubtitleGrid.FontSize > 0 ? SubtitleGrid.FontSize : Se.Settings.Appearance.SubtitleGridFontSize;

        var formattedText = new FormattedText(
            sample,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            typeface,
            fontSize,
            Brushes.Black);

        var cellPadding = Se.Settings.Appearance.GridCompactMode ? 0 : 8; // cell theme: 4 left + 4 right
        const int textBlockMargin = 8; // Avalonia DataGridTextColumn wraps text in TextBlock with Margin=Thickness(4)
        // Scale safety buffer with font size: gridline + sort indicator chrome + sub-pixel rounding grows with size.
        var safetyBuffer = Math.Max(10.0, fontSize);
        return Math.Ceiling(formattedText.Width) + cellPadding + textBlockMargin + safetyBuffer;
    }

    private void SelectAllRows()
    {
        SubtitleGrid.SelectAll();
    }

    private void InverseRowSelection()
    {
        if (SubtitleGrid.SelectedItems == null || Subtitles.Count == 0)
        {
            return;
        }

        // Store currently selected items
        var selectedItems =
            new HashSet<SubtitleLineViewModel>(SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>());

        _subtitleGridSelectionChangedSkip = true;
        SubtitleGrid.SelectedItems.Clear();
        foreach (var item in Subtitles)
        {
            if (!selectedItems.Contains(item))
            {
                SubtitleGrid.SelectedItems.Add(item);
            }
        }

        _subtitleGridSelectionChangedSkip = false;
        SubtitleGridSelectionChanged();
    }

    private void SelectAndScrollToRow(int index)
    {
        if (index < 0 || index >= Subtitles.Count)
        {
            return;
        }

        _shiftSelectAnchorIndex = -1;
        _shiftSelectCurrentIndex = -1;

        lock (_scrollLock)
        {
            _pendingScrollIndex = index;
        }

        Dispatcher.UIThread.Post(() =>
        {
            int indexToScroll;
            lock (_scrollLock)
            {
                indexToScroll = _pendingScrollIndex;
                _pendingScrollIndex = -1;
            }

            // Only execute if this is the latest scroll request
            if (indexToScroll >= 0 && indexToScroll < Subtitles.Count)
            {
                var itemToScroll = Subtitles[indexToScroll];
                SubtitleGrid.SelectedItem = itemToScroll;
                SubtitleGrid.ScrollIntoView(itemToScroll, null);

                if (Se.Settings.General.SubtitleGridCenterSelectedRow)
                {
                    CenterSelectedRowInSubtitleGrid(itemToScroll);
                }
            }
        });
    }

    private void CenterSelectedRowInSubtitleGrid(SubtitleLineViewModel itemToCenter)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var row = SubtitleGrid.GetVisualDescendants().OfType<DataGridRow>()
                .FirstOrDefault(r => ReferenceEquals(r.DataContext, itemToCenter));
            if (row == null || row.Bounds.Height <= 0)
            {
                return;
            }

            var rowsPresenter = SubtitleGrid.GetVisualDescendants().OfType<DataGridRowsPresenter>().FirstOrDefault();
            if (rowsPresenter == null || rowsPresenter.Bounds.Height <= 0)
            {
                return;
            }

            var verticalScrollBar = SubtitleGrid.GetVisualDescendants().OfType<ScrollBar>()
                .FirstOrDefault(sb => sb.Orientation == Orientation.Vertical);
            if (verticalScrollBar == null)
            {
                return;
            }

            // Use the row's actual rendered Y inside the rows presenter — this is
            // accurate regardless of variable row heights. The delta is exactly how
            // much we need to shift the scrollbar to center the row.
            var desiredY = (rowsPresenter.Bounds.Height - row.Bounds.Height) / 2.0;
            var delta = row.Bounds.Y - desiredY;
            if (Math.Abs(delta) < 1)
            {
                return;
            }

            var newValue = Math.Max(0, Math.Min(verticalScrollBar.Value + delta, verticalScrollBar.Maximum));
            if (Math.Abs(newValue - verticalScrollBar.Value) < 0.5)
            {
                return;
            }

            verticalScrollBar.Value = newValue;

            // Avalonia's DataGrid hooks the scrollbar's Scroll event (not
            // ValueChanged) to update the visible rows. ScrollEventArgs/ScrollEvent
            // aren't writable in this version, so invoke the internal handler via
            // reflection.
            var processVerticalScroll = typeof(DataGrid).GetMethod(
                "ProcessVerticalScroll",
                BindingFlags.NonPublic | BindingFlags.Instance);
            processVerticalScroll?.Invoke(SubtitleGrid, new object[] { ScrollEventType.EndScroll });
        }, DispatcherPriority.Loaded);
    }

    /// <summary>
    /// Applies the fixed subtitle returned by Fix Common Errors / Fix Netflix Errors.
    /// When the paragraph count is unchanged (the common case), updates each
    /// SubtitleLineViewModel in-place so the grid keeps its current scroll position
    /// and selection without any manipulation.
    /// Falls back to a full SetSubtitles reset only when rows were added or removed.
    /// </summary>
    /// <summary>
    /// Applies the fixed subtitle returned by Fix Common Errors / Fix Netflix Errors.
    /// When the paragraph count is unchanged (the common case), updates each
    /// SubtitleLineViewModel in-place so the grid keeps its current scroll position
    /// and selection without any manipulation.
    /// Falls back to a full SetSubtitles reset only when rows were added or removed.
    /// </summary>
    private void ApplyFixedSubtitle(Subtitle fixedSubtitle, int selectedIndex)
    {
        if (fixedSubtitle.Paragraphs.Count == Subtitles.Count)
        {
            for (var i = 0; i < Subtitles.Count; i++)
                Subtitles[i].UpdateFrom(fixedSubtitle.Paragraphs[i]);
            Renumber();
            UpdateGaps();
        }
        else
        {
            SetSubtitles(fixedSubtitle);
            SelectAndScrollToRow(selectedIndex);
        }
    }

    /// <summary>
    /// Overload for dialogs (ChangeFormatting, ConvertActors) that return a
    /// pre-built List&lt;SubtitleLineViewModel&gt; rather than a raw Subtitle.
    /// </summary>
    private void ApplyFixedSubtitle(List<SubtitleLineViewModel> fixedSubtitles, int selectedIndex)
    {
        if (fixedSubtitles.Count == Subtitles.Count)
        {
            for (var i = 0; i < Subtitles.Count; i++)
                Subtitles[i].UpdateFrom(fixedSubtitles[i]);
            Renumber();
            UpdateGaps();
        }
        else
        {
            SetSubtitles(fixedSubtitles);
            SelectAndScrollToRow(selectedIndex);
        }
    }

    public void SelectAndScrollToSubtitle(SubtitleLineViewModel subtitle)
    {
        if (subtitle == null || !Subtitles.Contains(subtitle))
        {
            return;
        }

        lock (_scrollLock)
        {
            _pendingScrollSubtitle = subtitle;
        }

        Dispatcher.UIThread.Post(() =>
        {
            SubtitleLineViewModel? subtitleToScroll;
            lock (_scrollLock)
            {
                subtitleToScroll = _pendingScrollSubtitle;
                _pendingScrollSubtitle = null;
            }

            // Only execute if this is the latest scroll request
            if (subtitleToScroll != null && Subtitles.Contains(subtitleToScroll))
            {
                SubtitleGrid.SelectedItem = subtitleToScroll;
                SubtitleGrid.ScrollIntoView(subtitleToScroll, null);

                if (Se.Settings.General.SubtitleGridCenterSelectedRow)
                {
                    CenterSelectedRowInSubtitleGrid(subtitleToScroll);
                }
            }
        }, DispatcherPriority.Background);
    }

    private bool ToggleTextBoxTag(ITextBoxWrapper tb, string htmlTag, string assaOn, string assaOff)
    {
        if (tb == null || tb.Text == null)
        {
            return false;
        }

        var selectionStart = Math.Min(tb.SelectionStart, tb.SelectionEnd);
        var selectionEnd = Math.Max(tb.SelectionStart, tb.SelectionEnd);
        var selectionLength = selectionEnd - selectionStart;

        var isAssa = SelectedSubtitleFormat is AdvancedSubStationAlpha;
        if (selectionLength == 0)
        {
            if (isAssa)
            {
                if (tb.Text.Contains("{\\" + assaOn + "}"))
                {
                    tb.Text = tb.Text.Replace("{\\" + assaOn + "}", string.Empty)
                        .Replace("{\\" + assaOff + "}", string.Empty);
                }
                else
                {
                    tb.Text = "{\\" + assaOn + "}" + tb.Text + "{\\" + assaOff + "}";
                }
            }
            else
            {
                if (tb.Text.Contains("<" + htmlTag + ">"))
                {
                    tb.Text = HtmlUtil.RemoveOpenCloseTags(tb.Text, htmlTag);
                }
                else
                {
                    tb.Text = "<" + htmlTag + ">" + tb.Text + "</" + htmlTag + ">";
                }
            }
        }
        else
        {
            var selectedText = tb.Text.Substring(selectionStart, selectionLength);

            if (isAssa)
            {
                if (selectedText.Contains("{\\" + assaOn + "}"))
                {
                    selectedText = selectedText.Replace("{\\" + assaOn + "}", string.Empty)
                        .Replace("{\\" + assaOff + "}", string.Empty);
                }
                else
                {
                    selectedText = "{\\" + assaOn + "}" + selectedText + "{\\" + assaOff + "}";
                }
            }
            else
            {
                if (selectedText.Contains("<" + htmlTag + ">"))
                {
                    selectedText = HtmlUtil.RemoveOpenCloseTags(selectedText, htmlTag);
                }
                else
                {
                    selectedText = "<" + htmlTag + ">" + selectedText + "</" + htmlTag + ">";
                }
            }

            tb.Text = tb.Text
                .Remove(selectionStart, selectionLength)
                .Insert(selectionStart, selectedText);

            Dispatcher.UIThread.Post(() =>
            {
                tb.Focus();
                tb.SelectionStart = selectionStart;
                tb.SelectionEnd = selectionStart + selectedText.Length;
            });
        }

        return true;
    }


    /// <summary>
    /// OpenSubtitle - open subtitle file, video file is optional.
    /// </summary>
    public async Task SubtitleOpen(
        string fileName,
        string? videoFileName = null,
        int? selectedSubtitleIndex = null,
        TextEncoding? textEncoding = null,
        bool skipLoadVideo = false,
        int desiredAudioTrackId = -1)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var ext = Path.GetExtension(fileName);
        var fileSize = (long)0;
        try
        {
            var fi = new FileInfo(fileName);
            fileSize = fi.Length;
        }
        catch
        {
            // ignore
        }

        if (fileSize < 10)
        {
            var message = fileSize == 0 ? "File size is zero!" : $"File size too small - only {fileSize} bytes";
            await MessageBox.Show(Window!,
                Se.Language.General.Error,
                message,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        try
        {
            _opening = true;

            if (FileUtil.IsMatroskaFileFast(fileName) && FileUtil.IsMatroskaFile(fileName))
            {
                await ImportSubtitleFromMatroskaFile(fileName, videoFileName);
                return;
            }

            if (ext == ".sup" && FileUtil.IsBluRaySup(fileName))
            {
                var log = new StringBuilder();
                var subtitles = BluRaySupParser.ParseBluRaySup(fileName, log);
                if (subtitles.Count > 0)
                {
                    Dispatcher.UIThread.Post(async () =>
                    {
                        var result = await ShowDialogAsync<OcrWindow, OcrViewModel>(vm => { vm.Initialize(subtitles, fileName); });

                        if (result.OkPressed)
                        {
                            _subtitleFileName = Path.GetFileNameWithoutExtension(fileName);
                            _converted = true;
                            Subtitles.Clear();
                            Subtitles.AddRange(result.OcredSubtitle);
                            SelectAndScrollToRow(0);
                        }
                    });
                    return;
                }
            }

            if ((ext == ".mp4" || ext == ".m4v" || ext == ".3gp" || ext == ".mov" || ext == ".cmaf" || ext == ".m4a" || ext == ".m4b") &&
                fileSize > 2000 || ext == ".m4s")
            {
                if (!new IsmtDfxp().IsMine(null, fileName))
                {
                    var ok = await ImportSubtitleFromMp4(fileName);
                    if (ok)
                    {
                        return;
                    }
                }
            }

            if ((ext == ".ts" || ext == ".tsv" || ext == ".tts" || ext == ".rec" || ext == ".mpeg" || ext == ".mpg") && fileSize > 10000 && FileUtil.IsTransportStream(fileName))
            {
                await ImportSubtitleFromTransportStream(fileName);
                return;
            }

            if (((ext == ".m2ts" || ext == ".ts" || ext == ".tsv" || ext == ".tts" || ext == ".mts") &&
                 fileSize > 10000 && FileUtil.IsM2TransportStream(fileName)) ||
                (ext == ".textst" && FileUtil.IsMpeg2PrivateStream2(fileName)))
            {
                bool isTextSt = false;
                if (fileSize < 2000000)
                {
                    var textSt = new TextST();
                    isTextSt = textSt.IsMine(null, fileName);
                }

                if (!isTextSt)
                {
                    await ImportSubtitleFromTransportStream(fileName);
                    return;
                }
            }

            if (FileUtil.IsVobSub(fileName) && ext == ".sub")
            {
                var ok = await ImportSubtitleFromVobSubFile(fileName, videoFileName);
                if (ok)
                {
                    SelectAndScrollToRow(0);
                }

                return;
            }

            if (ext == ".idx")
            {
                var subFile = Path.ChangeExtension(fileName, ".sub");
                if (File.Exists(subFile) && FileUtil.IsVobSub(subFile))
                {
                    var ok = await ImportSubtitleFromVobSubFile(subFile, videoFileName);
                    if (ok)
                    {
                        SelectAndScrollToRow(0);
                    }

                    return;
                }
            }

            if (ext == ".ismt" || ext == ".mp4" || ext == ".m4v" || ext == ".mov" || ext == ".3gp" || ext == ".cmaf" || ext == ".m4s" || ext == ".m4a" || ext == ".m4b")
            {
                var f = new IsmtDfxp();
                if (f.IsMine(null, fileName))
                {
                    f.LoadSubtitle(_subtitle, null, fileName);

                    if (_subtitle.OriginalFormat?.Name == new TimedTextBase64Image().Name)
                    {
                        ImportAndInlineBase64(_subtitle, fileName);
                        return;
                    }

                    if (_subtitle.OriginalFormat?.Name == new TimedTextImage().Name)
                    {
                        ImportAndOcrDost(fileName, _subtitle);
                        return;
                    }

                    ResetSubtitle();
                    _subtitleFileName = Utilities.GetPathAndFileNameWithoutExtension(fileName) +
                                        SelectedSubtitleFormat.Extension;
                    _subtitle.Renumber();
                    Subtitles.AddRange(_subtitle.Paragraphs.Select(p =>
                        new SubtitleLineViewModel(p, SelectedSubtitleFormat)));
                    ShowStatus(string.Format(Se.Language.General.SubtitleLoadedX, fileName));
                    SelectAndScrollToRow(0);
                    LoadBookmarks();
                    _converted = true;
                    return;
                }
            }

            if (ext == ".mcc")
            {
                var lines = FileUtil.ReadAllTextShared(fileName, Encoding.ASCII).SplitToLines();
                var f = new MacCaption10();
                if (f.IsMine(lines, fileName))
                {
                    ResetSubtitle();
                    f.LoadSubtitle(_subtitle, lines, fileName);
                    SetSubtitles(_subtitle);
                    _subtitleFileName = Utilities.GetPathAndFileNameWithoutExtension(fileName) +
                                        SelectedSubtitleFormat.Extension;
                    ShowStatus(string.Format(Se.Language.General.SubtitleLoadedX, fileName));
                    SelectAndScrollToRow(0);
                    _converted = true;
                    return;
                }
            }

            if (ext == ".divx" || ext == ".avi")
            {
                if (ImportSubtitleFromDivX(fileName))
                {
                    return;
                }
            }

            // check for video files ext and size >1 MB
            if (fileSize > 1_000_000 && Utilities.VideoFileExtensions.Contains(ext.ToLowerInvariant()))
            {
                var answer = await MessageBox.Show(
                    Window!,
                    Se.Language.General.Error,
                    string.Format(Se.Language.Main.ErrorLoadVideoFilePrompt, fileName),
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                if (answer == MessageBoxResult.Yes)
                {
                    await VideoOpenFile(fileName);
                }

                return;
            }

            // check for large file size - cannot be a subtitle file
            if (fileSize > 100_000_000)
            {
                await MessageBox.Show(Window!, Se.Language.General.Error, Se.Language.Main.ErrorLoadLargeFile, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var fileEncoding = LanguageAutoDetect.GetEncodingFromFile(fileName);
            Subtitle? subtitle = null;

            // For .csv files, try the multi-column CSV importer first. It only succeeds when there is a
            // recognizable header (start/end/text/etc.), so single-text-column CSV formats fall through to Subtitle.Parse.
            if (string.Equals(ext, ".csv", StringComparison.OrdinalIgnoreCase))
            {
                var csvLines = FileUtil.ReadAllLinesShared(fileName, Encoding.UTF8);
                var csvSubtitle = new UnknownFormatImporterCsv().AutoGuessImport(csvLines);
                if (csvSubtitle != null && csvSubtitle.Paragraphs.Count > 0)
                {
                    subtitle = csvSubtitle;
                    _converted = true;
                }
            }

            subtitle ??= Subtitle.Parse(fileName, fileEncoding);
            if (subtitle != null && subtitle.OriginalFormat != null && subtitle.OriginalFormat.Name == new WebVTT().Name)
            {
                var lines = FileUtil.ReadAllLinesShared(fileName, Encoding.UTF8);
                var format = new WebVttThumbnail();
                if (format.IsMine(lines, fileName))
                {
                    subtitle = new Subtitle();
                    format.LoadSubtitle(subtitle, lines, fileName);
                    ImportAndOcrWebVttImages(fileName, subtitle);
                    return;
                }
            }

            if (subtitle == null)
            {
                if (FileUtil.IsSpDvdSup(fileName))
                {
                    ImportAndOcrSpDvdSup(fileName);
                    return;
                }

                foreach (var f in SubtitleFormat.GetBinaryFormats(false))
                {
                    if (f.IsMine(null, fileName))
                    {
                        subtitle = new Subtitle();
                        f.LoadSubtitle(subtitle, null, fileName);
                        subtitle.OriginalFormat = f;
                        break; // format found, exit the loop
                    }
                }

                // check for lyrics in their metadata tags (e.g., LYRICS, UNSYNCED LYRICS) in audio files: mp3, m4a, opus, flac
                if (subtitle == null && fileSize > 100 && (ext == ".mp3" || ext == ".m4a" || ext == ".opus" || ext == ".flac"))
                {
                    var lyrics = GetLyricsFromAudioFile(fileName);
                    if (!string.IsNullOrEmpty(lyrics))
                    {
                        var lyricsSubtitle = Subtitle.Parse(lyrics.SplitToLines(), ".lrc");
                        if (lyricsSubtitle != null && lyricsSubtitle.Paragraphs.Count > 0)
                        {
                            subtitle = lyricsSubtitle;
                            _converted = true;
                        }
                    }
                }

                if (subtitle == null)
                {
                    var uknownFormatImporter = new UnknownFormatImporter { UseFrames = true };
                    var lines = FileUtil.ReadAllLinesShared(fileName, Encoding.UTF8);
                    var genericParseSubtitle = uknownFormatImporter.AutoGuessImport(lines, fileName);
                    if (genericParseSubtitle.Paragraphs.Count > 1)
                    {
                        subtitle = genericParseSubtitle;
                        _converted = true;
                    }
                }

                // check for .rar file
                if (subtitle == null && fileSize > 100 && FileUtil.IsRar(fileName))
                {
                    await MessageBox.Show(Window!, Se.Language.General.Error, Se.Language.Main.ErrorLoadRar);
                    return;
                }

                // check for .zip file
                if (subtitle == null && fileSize > 100 && FileUtil.IsZip(fileName))
                {
                    await MessageBox.Show(Window!, Se.Language.General.Error, Se.Language.Main.ErrorLoadZip);
                    return;
                }

                // check for .gzip file
                if (subtitle == null && fileSize > 100 && FileUtil.IsGZip(fileName))
                {
                    await MessageBox.Show(Window!, Se.Language.General.Error, Se.Language.Main.ErrorLoadGZip);
                    return;
                }

                // check for .7z file
                if (subtitle == null && fileSize > 100 && FileUtil.Is7Zip(fileName))
                {
                    await MessageBox.Show(Window!, Se.Language.General.Error, Se.Language.Main.ErrorLoad7Zip);
                    return;
                }

                // check for .png file
                if (subtitle == null && fileSize > 100 && FileUtil.IsPng(fileName))
                {
                    await MessageBox.Show(Window!, Se.Language.General.Error, Se.Language.Main.ErrorLoadPng);
                    return;
                }

                // check for .jpg file
                if (subtitle == null && fileSize > 100 && FileUtil.IsJpg(fileName))
                {
                    await MessageBox.Show(Window!, Se.Language.General.Error, Se.Language.Main.ErrorLoadJpg);
                    return;
                }

                // check for .srr file
                if (subtitle == null && fileSize > 100 && ext == ".srr" && FileUtil.IsSrr(fileName))
                {
                    await MessageBox.Show(Window!, Se.Language.General.Error, Se.Language.Main.ErrorLoadSrr);
                    return;
                }

                // check for Torrent file
                if (subtitle == null && fileSize > 50 && FileUtil.IsTorrentFile(fileName))
                {
                    await MessageBox.Show(Window!, Se.Language.General.Error, Se.Language.Main.ErrorLoadTorrent);
                    return;
                }

                // check for all binary zeroes (I've heard about this a few times... perhaps related to crashes?)
                if (subtitle == null && FileUtil.IsSubtitleFileAllBinaryZeroes(fileName))
                {
                    await MessageBox.Show(Window!, Se.Language.General.Error, Se.Language.Main.ErrorLoadBinaryZeroes);
                    return;
                }

                // check for mp3 file
                if (subtitle == null && fileSize > 50 && FileUtil.IsMp3(fileName))
                {
                    await MessageBox.Show(Window!, Se.Language.General.Error, "This file seems to be an .mp3 audio file which does not contains subtitles." + Environment.NewLine +
                                                                              Environment.NewLine +
                                                                              "You can open media files via the Video menu.");
                    return;
                }

                // check for wav file
                if (subtitle == null && fileSize > 50 && FileUtil.IsWav(fileName))
                {
                    await MessageBox.Show(Window!, Se.Language.General.Error, "This file seems to be a .wav audio file which does not contains subtitles." + Environment.NewLine +
                                                                              Environment.NewLine +
                                                                              "You can open media files via the Video menu.");
                    return;
                }

                if (subtitle == null)
                {
                    var message = Se.Language.General.UnknownSubtitleFormat;
                    await MessageBox.Show(Window!, Se.Language.General.Error, message, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            ResetSubtitle();

            AudioVisualizer?.StartPositionSeconds = 0;
            AudioVisualizer?.CurrentVideoPositionSeconds = 0;

            SetSubtitleFormat(SubtitleFormats.FirstOrDefault(p => p.Name == subtitle?.OriginalFormat?.Name) ??
                              SelectedSubtitleFormat);

            if (fileEncoding.WebName.StartsWith("utf-8", StringComparison.OrdinalIgnoreCase))
            {
                if (FileUtil.HasUtf8Bom(fileName))
                {
                    SelectedEncoding = Encodings.First(p => p.DisplayName == TextEncoding.Utf8WithBom);
                }
                else
                {
                    SelectedEncoding = Encodings.First(p => p.DisplayName == TextEncoding.Utf8WithoutBom);
                }
            }
            else
            {
                var otherEncoding = Encodings.FirstOrDefault(p => p.Encoding.WebName == fileEncoding.WebName);
                SelectedEncoding = otherEncoding ?? Encodings.First(p => p.DisplayName == TextEncoding.Utf8WithBom);
            }

            if (Se.Settings.General.AutoConvertToUtf8 && !SelectedEncoding.DisplayName.StartsWith("utf-8", StringComparison.OrdinalIgnoreCase))
            {
                SelectedEncoding = Encodings.FirstOrDefault(p => p.DisplayName.StartsWith("utf-8", StringComparison.OrdinalIgnoreCase)) ?? SelectedEncoding;
            }

            _subtitleFileName = fileName;
            _subtitle = subtitle;
            _lastOpenSaveFormat = subtitle.OriginalFormat;
            SetSubtitles(_subtitle);
            _changeSubtitleHash = GetFastHash();
            ShowStatus(string.Format(Se.Language.General.SubtitleLoadedX, fileName));
            LoadBookmarks();

            if (selectedSubtitleIndex != null && selectedSubtitleIndex >= 0 && selectedSubtitleIndex < Subtitles.Count)
            {
                SelectAndScrollToRow(selectedSubtitleIndex.Value);
            }
            else if (Subtitles.Count > 0)
            {
                SelectAndScrollToRow(0);
            }
            Dispatcher.UIThread.Post(() => SubtitleGrid.Focus());

            if (Se.Settings.Video.AutoOpen && skipLoadVideo == false)
            {
                if (!string.IsNullOrEmpty(videoFileName) && File.Exists(videoFileName))
                {
                    await VideoOpenFile(videoFileName, desiredAudioTrackId);
                }
                else if (FindVideoFileName.TryFindVideoFileName(fileName, out videoFileName))
                {
                    await VideoOpenFile(videoFileName, desiredAudioTrackId);
                }
            }

            AddToRecentFiles(true);
        }
        finally
        {
            AutoTrimWhiteSpaces();

            _undoRedoManager.Do(MakeUndoRedoObject(string.Format(Se.Language.General.SubtitleLoadedX, fileName)));
            _undoRedoManager.StartChangeDetection();
            _opening = false;

            SetupLiveSpellCheck();
        }
    }

    private string? GetLyricsFromAudioFile(string fileName)
    {
        try
        {
            using var file = TagLib.File.Create(fileName);

            // 1. Standard place (USLT frame)
            if (!string.IsNullOrWhiteSpace(file.Tag.Lyrics))
            {
                return file.Tag.Lyrics;
            }

            // 2. Fallback: look for ID3v2 lyrics frames manually
            if (file.GetTag(TagLib.TagTypes.Id3v2) is TagLib.Id3v2.Tag id3v2Tag)
            {
                // Unsynchronized lyrics (USLT)
                var uslt = id3v2Tag.GetFrames<TagLib.Id3v2.UnsynchronisedLyricsFrame>()
                                  .FirstOrDefault();
                if (uslt != null && !string.IsNullOrWhiteSpace(uslt.Text))
                {
                    return uslt.Text;
                }

                // Synchronized lyrics (SYLT) – rare, but possible
                var sylt = id3v2Tag.GetFrames<TagLib.Id3v2.SynchronisedLyricsFrame>()
                                  .FirstOrDefault();
                if (sylt != null && sylt.Text != null && sylt.Text.Length > 0)
                {
                    // Convert SYLT to LRC-like format
                    var lines = sylt.Text
                        .Select(t =>
                        {
                            var ts = TimeSpan.FromMilliseconds(t.Time);
                            return $"[{ts:mm\\:ss\\.ff}]{t.Text}";
                        });

                    return string.Join(Environment.NewLine, lines);
                }
            }
        }
        catch
        {
            // Ignore errors (file not found, invalid format, etc.)
        }

        return null;
    }

    private void SetupLiveSpellCheck()
    {
        var dictionaryFound = false;
        var twoLetterLanguageCode = LanguageAutoDetect.AutoDetectGoogleLanguage(GetUpdateSubtitle());
        var threeLetterLanguageCode = Iso639Dash2LanguageCode.GetThreeLetterCodeFromTwoLetterCode(twoLetterLanguageCode);
        if (!string.IsNullOrEmpty(threeLetterLanguageCode))
        {
            var spellCheckLanguages = _spellCheckManager.GetDictionaryLanguages(Se.DictionariesFolder);
            var dictionary = spellCheckLanguages.FirstOrDefault(p => p.GetThreeLetterCode() == threeLetterLanguageCode);
            if (dictionary != null)
            {
                _spellCheckManager.Initialize(dictionary.DictionaryFileName, twoLetterLanguageCode);
                dictionaryFound = true;
            }
        }

        if (EditTextBox is TextEditorWrapper wrapper)
        {
            if (Se.Settings.Appearance.SubtitleTextBoxLiveSpellCheck && dictionaryFound)
            {
                wrapper.EnableSpellCheck(_spellCheckManager);
            }
            else
            {
                wrapper.DisableSpellCheck();
            }
        }

        if (Se.Settings.Appearance.SubtitleGridLiveSpellCheck && dictionaryFound)
        {
            SubtitleDataGridSyntaxHighlighting.SetSpellCheck(_spellCheckManager);
            foreach (var item in Subtitles)
            {
                item.RefreshText();
            }
        }
        else
        {
            SubtitleDataGridSyntaxHighlighting.SetSpellCheck(null);
            foreach (var item in Subtitles)
            {
                item.RefreshText();
            }
        }
    }

    private bool ImportSubtitleFromDivX(string fileName)
    {
        var list = DivXSubParser.ImportSubtitleFromDivX(fileName);
        if (list.Count == 0)
        {
            return false;
        }

        Dispatcher.UIThread.Post(async () =>
        {
            var result = await ShowDialogAsync<OcrWindow, OcrViewModel>(vm => { vm.InitializeDivX(list, fileName); });

            if (result.OkPressed)
            {
                ResetSubtitle();
                _subtitleFileName = Path.GetFileNameWithoutExtension(fileName);
                _converted = true;
                _subtitle.Paragraphs.Clear();
                Subtitles.Clear();
                Subtitles.AddRange(result.OcredSubtitle);
                Renumber();
                ShowStatus(string.Format(Se.Language.General.SubtitleLoadedX, fileName));
                SelectAndScrollToRow(0);
            }
        });

        return true;
    }

    private void AutoTrimWhiteSpaces()
    {
        if (Se.Settings.General.AutoTrimWhiteSpace)
        {
            foreach (var item in Subtitles)
            {
                item.Text = Utilities.RemoveUnneededSpaces(item.Text, string.Empty).Trim();
            }
        }
    }

    private void LoadBookmarks()
    {
        var sub = GetUpdateSubtitle();
        new BookmarkPersistence(sub, _subtitleFileName).Load();
        for (var i = 0; i < Subtitles.Count && i < sub.Paragraphs.Count; i++)
        {
            Subtitles[i].Bookmark = sub.Paragraphs[i].Bookmark;
        }
    }

    private void ImportAndOcrDost(string fileName, Subtitle subtitle)
    {
        Dispatcher.UIThread.Post(async () =>
        {
            var result = await ShowDialogAsync<OcrWindow, OcrViewModel>(vm => { vm.InitializeBdn(subtitle, fileName, false); });

            if (result.OkPressed)
            {
                ResetSubtitle();
                _subtitleFileName = Path.GetFileNameWithoutExtension(fileName);
                _converted = true;
                _subtitle.Paragraphs.Clear();
                Subtitles.Clear();
                Subtitles.AddRange(result.OcredSubtitle);
                Renumber();
                ShowStatus(string.Format(Se.Language.General.SubtitleLoadedX, fileName));
                SelectAndScrollToRow(0);
            }
        });
    }

    private void ImportAndOcrWebVttImages(string fileName, Subtitle subtitle)
    {
        Dispatcher.UIThread.Post(async () =>
        {
            var result = await ShowDialogAsync<OcrWindow, OcrViewModel>(vm => { vm.InitializeWebVtt(subtitle, fileName); });

            if (result.OkPressed)
            {
                ResetSubtitle();
                _subtitleFileName = Path.GetFileNameWithoutExtension(fileName);
                _converted = true;
                _subtitle.Paragraphs.Clear();
                Subtitles.Clear();
                Subtitles.AddRange(result.OcredSubtitle);
                Renumber();
                ShowStatus(string.Format(Se.Language.General.SubtitleLoadedX, fileName));
                SelectAndScrollToRow(0);
            }
        });
    }

    private void ImportAndOcrSpDvdSup(string fileName)
    {
        Dispatcher.UIThread.Post(async () =>
        {
            var result = await ShowDialogAsync<OcrWindow, OcrViewModel>(vm => { vm.InitializeSpDvdSup(fileName); });

            if (result.OkPressed)
            {
                ResetSubtitle();
                _subtitleFileName = Path.GetFileNameWithoutExtension(fileName);
                _converted = true;
                _subtitle.Paragraphs.Clear();
                Subtitles.Clear();
                Subtitles.AddRange(result.OcredSubtitle);
                Renumber();
                ShowStatus(string.Format(Se.Language.General.SubtitleLoadedX, fileName));
                SelectAndScrollToRow(0);
            }
        });
    }

    private void ImportAndInlineBase64(Subtitle subtitle, string fileName)
    {
        IList<IBinaryParagraphWithPosition> list = [];
        foreach (var p in subtitle.Paragraphs)
        {
            var x = new TimedTextBase64Image.Base64PngImage()
            {
                Text = p.Text,
                StartTimeCode = p.StartTime,
                EndTimeCode = p.EndTime,
            };

            using var bitmap = x.GetBitmap();
            var nikseBmp = new NikseBitmap(bitmap);
            var nonTransparentHeight = nikseBmp.GetNonTransparentHeight();
            if (nonTransparentHeight > 1)
            {
                list.Add(x);
            }
        }

        if (list.Count == 0)
        {
            Dispatcher.UIThread.Post(async void () =>
            {
                await MessageBox.Show(
                    Window!,
                    Se.Language.General.Error,
                    Se.Language.General.NoSubtitlesFound,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            });
            return;
        }

        Dispatcher.UIThread.Post(async () =>
        {
            var result = await ShowDialogAsync<OcrWindow, OcrViewModel>(vm => { vm.Initialize(list, fileName); });

            if (result.OkPressed)
            {
                ResetSubtitle();
                _subtitleFileName = Path.GetFileNameWithoutExtension(fileName);
                _converted = true;
                _subtitle.Paragraphs.Clear();
                Subtitles.Clear();
                Subtitles.AddRange(result.OcredSubtitle);
                Renumber();
                ShowStatus(string.Format(Se.Language.General.SubtitleLoadedX, fileName));
                SelectAndScrollToRow(0);
            }
        });
    }

    private void RemoveShotChange(int idx)
    {
        if (AudioVisualizer == null || AudioVisualizer.ShotChanges == null)
        {
            return;
        }

        if (idx >= 0 && idx < AudioVisualizer.ShotChanges.Count)
        {
            var temp = new List<double>(AudioVisualizer.ShotChanges);
            temp.RemoveAt(idx);
            AudioVisualizer.ShotChanges = temp;

            if (!string.IsNullOrEmpty(_videoFileName))
            {
                ShotChangesHelper.SaveShotChanges(_videoFileName, temp, _audioTrack?.FfIndex ?? -1);
            }
        }
    }

    private async Task ImportSubtitleFromTransportStream(string fileName)
    {
        ShowStatus(string.Format(Se.Language.General.ParsingXDotDotDot, fileName));
        var tsParser = new TransportStreamParser();

        await Task.Run(() =>
        {
            tsParser.Parse(fileName,
                (pos, total) =>
                    UpdateProgress(pos, total, string.Format(string.Format(Se.Language.General.ParsingXDotDotDot, fileName), fileName)));
        });
        ShowStatus(string.Empty);

        if (tsParser.SubtitlePacketIds.Count == 0 && tsParser.TeletextSubtitlesLookup.Count == 0)
        {
            await MessageBox.Show(Window!, Se.Language.General.Error, Se.Language.General.NoSubtitlesFound,
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (tsParser.SubtitlePacketIds.Count == 0 && tsParser.TeletextSubtitlesLookup.Count == 1 &&
            tsParser.TeletextSubtitlesLookup.First().Value.Count == 1)
        {
            ResetSubtitle();
            _subtitle = new Subtitle(tsParser.TeletextSubtitlesLookup.First().Value.First().Value);
            _subtitle.Renumber();
            Subtitles.AddRange(_subtitle.Paragraphs.Select(p => new SubtitleLineViewModel(p, SelectedSubtitleFormat)));
            SelectAndScrollToRow(0);
            if (Se.Settings.General.AutoOpenVideo)
            {
                await VideoOpenFile(fileName);
            }

            _subtitleFileName = Path.GetFileNameWithoutExtension(fileName) + SelectedSubtitleFormat.Extension;
            _converted = true;
            return;
        }

        int packetId = 0;
        if (tsParser.SubtitlePacketIds.Count + tsParser.TeletextSubtitlesLookup.Sum(p => p.Value.Count) > 1)
        {
            var result = await ShowDialogAsync<PickTsTrackWindow, PickTsTrackViewModel>(vm => { vm.Initialize(tsParser, fileName); });

            if (result.OkPressed && result.SelectedTrack != null && result.SelectedTrack.IsTeletext)
            {
                ResetSubtitle();
                SelectAndScrollToRow(0);
                SetSubtitles(result.TeletextSubtitle);
                _subtitleFileName = Path.GetFileNameWithoutExtension(fileName) + SelectedSubtitleFormat.Extension;
                _converted = true;
                if (Se.Settings.General.AutoOpenVideo)
                {
                    await VideoOpenFile(fileName);
                }

                return;
            }
            else if (!result.OkPressed || result.SelectedTrack == null)
            {
                return;
            }

            packetId = result.SelectedTrack!.TrackNumber;
        }
        else
        {
            packetId = tsParser.SubtitlePacketIds[0];
        }

        var subtitles = tsParser.GetDvbSubtitles(packetId);
        Dispatcher.UIThread.Post(async () =>
        {
            var result = await ShowDialogAsync<OcrWindow, OcrViewModel>(vm => { vm.Initialize(tsParser, subtitles, fileName); });

            if (result.OkPressed)
            {
                _subtitleFileName = Path.GetFileNameWithoutExtension(fileName);
                Subtitles.Clear();
                Subtitles.AddRange(result.OcredSubtitle);
                SelectAndScrollToRow(0);
            }
        });

        return;
    }

    private int _lastProgressPercent = -1;

    private void UpdateProgress(long position, long total, string statusMessage)
    {
        var percent = (int)Math.Round(position * 100.0 / total, MidpointRounding.AwayFromZero);
        if (percent == _lastProgressPercent)
        {
            return;
        }

        ShowStatus(string.Format("{0}, {1:0}%", statusMessage, percent));
        _lastProgressPercent = percent;
    }

    private async Task<bool> ImportSubtitleFromMp4(string fileName)
    {
        var mp4Parser = new MP4Parser(fileName);
        var mp4SubtitleTracks = mp4Parser.GetSubtitleTracks();
        if (mp4SubtitleTracks.Count == 0)
        {
            if (mp4Parser.VttcSubtitle?.Paragraphs.Count > 0)
            {
                ResetSubtitle();
                SetSubtitleFormat(SubtitleFormats.FirstOrDefault(p => p.Name == new WebVTT().Name) ??
                                  SelectedSubtitleFormat);
                _subtitle = mp4Parser.VttcSubtitle;
                _subtitle.Renumber();
                _subtitleFileName = Utilities.GetPathAndFileNameWithoutExtension(fileName) +
                                    SelectedSubtitleFormat.Extension;
                Subtitles.AddRange(
                    _subtitle.Paragraphs.Select(p => new SubtitleLineViewModel(p, SelectedSubtitleFormat)));
                _converted = true;
                ShowStatus(string.Format(Se.Language.General.SubtitleLoadedX, fileName));
                SelectAndScrollToRow(0);

                if (Se.Settings.General.AutoOpenVideo && mp4Parser.GetVideoTracks().Count > 0)
                {
                    await VideoOpenFile(fileName);
                }

                return true;
            }

            // Prefer CEA-608 if present, otherwise fall back to CEA-708 — most US
            // broadcast MP4s carry both, but newer streams (and many international
            // ones) carry only CEA-708.
            var captionsFromH264 = mp4Parser.TrunCea608Subtitle?.Paragraphs.Count > 0
                ? mp4Parser.TrunCea608Subtitle
                : mp4Parser.TrunCea708Subtitle?.Paragraphs.Count > 0
                    ? mp4Parser.TrunCea708Subtitle
                    : null;

            if (captionsFromH264 != null)
            {
                ResetSubtitle();
                _subtitle = captionsFromH264;
                _subtitle.Renumber();
                _subtitleFileName = Utilities.GetPathAndFileNameWithoutExtension(fileName) +
                                    SelectedSubtitleFormat.Extension;
                Subtitles.AddRange(
                    _subtitle.Paragraphs.Select(p => new SubtitleLineViewModel(p, SelectedSubtitleFormat)));
                _converted = true;
                ShowStatus(string.Format(Se.Language.General.SubtitleLoadedX, fileName));
                SelectAndScrollToRow(0);

                if (Se.Settings.General.AutoOpenVideo && mp4Parser.GetVideoTracks().Count > 0)
                {
                    await VideoOpenFile(fileName);
                }

                return true;
            }
        }
        else if (mp4SubtitleTracks.Count == 1)
        {
            LoadMp4Subtitle(fileName, mp4SubtitleTracks[0]);

            if (Se.Settings.General.AutoOpenVideo && mp4Parser.GetVideoTracks().Count > 0)
            {
                await VideoOpenFile(fileName);
            }

            return true;
        }
        else
        {
            var result =
                await ShowDialogAsync<PickMp4TrackWindow, PickMp4TrackViewModel>(vm => { vm.Initialize(mp4SubtitleTracks, fileName); });

            if (result.OkPressed && result.SelectedTrack != null && result.SelectedTrack.Track != null)
            {
                LoadMp4Subtitle(fileName, result.SelectedTrack.Track);
                return true;
            }
        }

        return false;
    }

    private void LoadMp4Subtitle(string fileName, Trak mp4SubtitleTrack)
    {
        if (mp4SubtitleTrack.Mdia.IsVobSubSubtitle)
        {
            Dispatcher.UIThread.Post(async () =>
            {
                var paragraphs = mp4SubtitleTrack.Mdia.Minf.Stbl.GetParagraphs();
                var result = await ShowDialogAsync<OcrWindow, OcrViewModel>(vm => { vm.Initialize(mp4SubtitleTrack, paragraphs, fileName); });

                if (result.OkPressed)
                {
                    ResetSubtitle();
                    _subtitleFileName = Path.GetFileNameWithoutExtension(fileName);
                    _converted = true;
                    Subtitles.Clear();
                    Subtitles.AddRange(result.OcredSubtitle);
                    Renumber();
                    ShowStatus(string.Format(Se.Language.General.SubtitleLoadedX, fileName));
                    SelectAndScrollToRow(0);
                }
            });
        }
        else
        {
            ResetSubtitle();
            _subtitleFileName = Path.GetFileNameWithoutExtension(fileName);
            _converted = true;
            _subtitle.Paragraphs.Clear();
            _subtitle.Paragraphs.AddRange(mp4SubtitleTrack.Mdia.Minf.Stbl.GetParagraphs());
            Subtitles.Clear();
            Subtitles.AddRange(_subtitle.Paragraphs.Select(p => new SubtitleLineViewModel(p, SelectedSubtitleFormat)));
            Renumber();
            ShowStatus(string.Format(Se.Language.General.SubtitleLoadedX, fileName));
            SelectAndScrollToRow(0);
        }
    }

    private async Task ImportSubtitleFromMatroskaFile(string fileName, string? videoFileName)
    {
        var matroska = new MatroskaFile(fileName);
        var subtitleList = matroska.GetTracks(true);
        if (subtitleList.Count == 0)
        {
            matroska.Dispose();
            Dispatcher.UIThread.Post(async void () =>
            {
                try
                {
                    var answer = await MessageBox.Show(
                        Window!,
                        "No subtitle found",
                        "The Matroska file does not seem to contain any subtitles.",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                catch (Exception e)
                {
                    Se.LogError(e);
                }
            });

            matroska.Dispose();
            return;
        }

        if (subtitleList.Count > 1)
        {
            Dispatcher.UIThread.Post(async void () =>
            {
                var result =
                    await ShowDialogAsync<PickMatroskaTrackWindow, PickMatroskaTrackViewModel>(vm => { vm.Initialize(matroska, subtitleList, fileName); });
                if (result.OkPressed && result.SelectedMatroskaTrack != null)
                {
                    if (await LoadMatroskaSubtitle(result.SelectedMatroskaTrack, matroska, fileName))
                    {
                        _subtitleFileName = Path.GetFileNameWithoutExtension(fileName);
                        _converted = true;
                        SelectAndScrollToRow(0);

                        if (Se.Settings.General.AutoOpenVideo)
                        {
                            if (fileName.EndsWith("mkv", StringComparison.OrdinalIgnoreCase))
                            {
                                await VideoOpenFile(fileName);
                            }
                        }
                    }
                }

                matroska.Dispose();
            });
        }
        else
        {
            var ext = Path.GetExtension(matroska.Path).ToLowerInvariant();
            if (await LoadMatroskaSubtitle(subtitleList[0], matroska, fileName))
            {
                if (Se.Settings.General.AutoOpenVideo)
                {
                    if (ext == ".mkv")
                    {
                        Dispatcher.UIThread.Post(async void () =>
                        {
                            try
                            {
                                await VideoOpenFile(matroska.Path);
                                matroska.Dispose();
                            }
                            catch (Exception e)
                            {
                                Se.LogError(e);
                            }
                        });
                    }
                    else
                    {
                        if (FindVideoFileName.TryFindVideoFileName(matroska.Path, out videoFileName))
                        {
                            Dispatcher.UIThread.Post(async void () =>
                            {
                                try
                                {
                                    await VideoOpenFile(videoFileName);
                                    matroska.Dispose();
                                }
                                catch (Exception e)
                                {
                                    Se.LogError(e);
                                }
                            });
                        }
                        else
                        {
                            matroska.Dispose();
                        }
                    }
                }
            }
            else
            {
                matroska.Dispose();
            }
        }
    }

    private async Task<bool> LoadMatroskaSubtitle(
        MatroskaTrackInfo matroskaSubtitleInfo,
        MatroskaFile matroska,
        string fileName)
    {
        if (matroskaSubtitleInfo.CodecId.Equals("S_HDMV/PGS", StringComparison.OrdinalIgnoreCase))
        {
            var pgsSubtitle =
                _bluRayHelper.LoadBluRaySubFromMatroska(matroskaSubtitleInfo, matroska, out var errorMessage);

            Dispatcher.UIThread.Post(async () =>
            {
                var result = await ShowDialogAsync<OcrWindow, OcrViewModel>(vm => { vm.Initialize(matroskaSubtitleInfo, pgsSubtitle, fileName); });

                if (result.OkPressed)
                {
                    _subtitleFileName = Path.GetFileNameWithoutExtension(fileName);
                    _converted = true;
                    Subtitles.Clear();
                    Subtitles.AddRange(result.OcredSubtitle);
                    Renumber();
                    SelectAndScrollToRow(0);
                }
            });

            return true;
        }

        if (matroskaSubtitleInfo.CodecId.Equals("S_HDMV/TEXTST", StringComparison.OrdinalIgnoreCase))
        {
            return LoadTextSTFromMatroska(matroskaSubtitleInfo, matroska);
        }

        if (matroskaSubtitleInfo.CodecId.Equals("S_DVBSUB", StringComparison.OrdinalIgnoreCase))
        {
            return LoadDvbFromMatroska(matroskaSubtitleInfo, matroska, fileName);
        }

        if (matroskaSubtitleInfo.CodecId.Equals("S_VOBSUB", StringComparison.OrdinalIgnoreCase))
        {
            return await LoadVobSubFromMatroska(matroskaSubtitleInfo, matroska, fileName);
        }

        var sub = matroska.GetSubtitle(matroskaSubtitleInfo.TrackNumber, null);
        var subtitle = new Subtitle();
        var format = Utilities.LoadMatroskaTextSubtitle(matroskaSubtitleInfo, matroska, sub, subtitle);
        ResetSubtitle(format);
        _subtitle = subtitle;
        _subtitle.Renumber();
        Subtitles.Clear();
        Subtitles.AddRange(_subtitle.Paragraphs.Select(p => new SubtitleLineViewModel(p, SelectedSubtitleFormat)));
        _converted = true;

        return true;
    }

    private bool LoadDvbFromMatroska(MatroskaTrackInfo matroskaSubtitleInfo, MatroskaFile matroska, string fileName)
    {
        ShowStatus(Se.Language.Main.ParsingMatroskaFile);
        var sub = matroska.GetSubtitle(matroskaSubtitleInfo.TrackNumber, MatroskaProgress);

        _subtitle.Paragraphs.Clear();
        var subtitleImages = new List<DvbSubPes>();
        var subtitle = new Subtitle();
        Utilities.LoadMatroskaTextSubtitle(matroskaSubtitleInfo, matroska, sub, _subtitle);
        for (var index = 0; index < sub.Count; index++)
        {
            try
            {
                var msub = sub[index];
                DvbSubPes? pes = null;
                var data = msub.GetData(matroskaSubtitleInfo);
                if (data != null && data.Length > 9 && data[0] == 15 &&
                    data[1] >= SubtitleSegment.PageCompositionSegment &&
                    data[1] <= SubtitleSegment.DisplayDefinitionSegment) // sync byte + segment id
                {
                    var buffer = new byte[data.Length + 3];
                    Buffer.BlockCopy(data, 0, buffer, 2, data.Length);
                    buffer[0] = 32;
                    buffer[1] = 0;
                    buffer[buffer.Length - 1] = 255;
                    pes = new DvbSubPes(0, buffer);
                }
                else if (VobSubParser.IsMpeg2PackHeader(data))
                {
                    pes = new DvbSubPes(data, Mpeg2Header.Length);
                }
                else if (VobSubParser.IsPrivateStream1(data, 0))
                {
                    pes = new DvbSubPes(data, 0);
                }
                else if (data!.Length > 9 && data[0] == 32 && data[1] == 0 && data[2] == 14 && data[3] == 16)
                {
                    pes = new DvbSubPes(0, data);
                }

                if (pes == null && subtitle.Paragraphs.Count > 0)
                {
                    var last = subtitle.Paragraphs[subtitle.Paragraphs.Count - 1];
                    if (last.DurationTotalMilliseconds < 100)
                    {
                        last.EndTime.TotalMilliseconds = msub.Start;
                        if (last.DurationTotalMilliseconds >
                            Se.Settings.General.SubtitleMaximumDisplayMilliseconds)
                        {
                            last.EndTime.TotalMilliseconds = last.StartTime.TotalMilliseconds + 3000;
                        }
                    }
                }

                if (pes != null && pes.PageCompositions != null && pes.PageCompositions.Any(p => p.Regions.Count > 0))
                {
                    subtitleImages.Add(pes);
                    subtitle.Paragraphs.Add(new Paragraph(string.Empty, msub.Start, msub.End));
                }
            }
            catch
            {
                // continue
            }
        }

        if (subtitleImages.Count == 0)
        {
            return false;
        }

        for (var index = 0; index < subtitle.Paragraphs.Count; index++)
        {
            var p = subtitle.Paragraphs[index];
            if (p.DurationTotalMilliseconds < 200)
            {
                p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + 3000;
            }

            var next = subtitle.GetParagraphOrDefault(index + 1);
            if (next != null && next.StartTime.TotalMilliseconds < p.EndTime.TotalMilliseconds)
            {
                p.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds -
                                              Se.Settings.General.MinimumBetweenLines.GetMilliseconds();
            }
        }

        Dispatcher.UIThread.Post(async () =>
        {
            var result = await ShowDialogAsync<OcrWindow, OcrViewModel>(vm => { vm.Initialize(matroskaSubtitleInfo, subtitle, subtitleImages, fileName); });

            if (result.OkPressed)
            {
                ResetSubtitle();
                _subtitleFileName = Path.GetFileNameWithoutExtension(fileName);
                Subtitles.Clear();
                Subtitles.AddRange(result.OcredSubtitle);
                Renumber();
                SelectAndScrollToRow(0);
                ShowStatus(string.Format(Se.Language.General.SubtitleLoadedX, fileName));
            }
        });

        return true;
    }

    private async Task<bool> LoadVobSubFromMatroska(MatroskaTrackInfo matroskaSubtitleInfo, MatroskaFile matroska,
        string fileName)
    {
        if (matroskaSubtitleInfo.ContentEncodingType == 1)
        {
            var message = "Encrypted VobSub subtitles are not supported.";
            await MessageBox.Show(Window!, Se.Language.General.Error, message, MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return false;
        }

        var sub = matroska.GetSubtitle(matroskaSubtitleInfo.TrackNumber, MatroskaProgress);
        _subtitle.Paragraphs.Clear();

        List<VobSubMergedPack> mergedVobSubPacks = [];
        var idx = new Core.VobSub.Idx(matroskaSubtitleInfo.GetCodecPrivate().SplitToLines());
        foreach (var p in sub)
        {
            mergedVobSubPacks.Add(new VobSubMergedPack(p.GetData(matroskaSubtitleInfo),
                TimeSpan.FromMilliseconds(p.Start), 32, null));
            if (mergedVobSubPacks.Count > 0)
            {
                mergedVobSubPacks[mergedVobSubPacks.Count - 1].EndTime = TimeSpan.FromMilliseconds(p.End);
            }

            // fix overlapping (some versions of Handbrake makes overlapping time codes - thx Hawke)
            if (mergedVobSubPacks.Count > 1 && mergedVobSubPacks[mergedVobSubPacks.Count - 2].EndTime >
                mergedVobSubPacks[mergedVobSubPacks.Count - 1].StartTime)
            {
                mergedVobSubPacks[mergedVobSubPacks.Count - 2].EndTime =
                    TimeSpan.FromMilliseconds(
                        mergedVobSubPacks[mergedVobSubPacks.Count - 1].StartTime.TotalMilliseconds - 1);
            }
        }

        // Remove bad packs
        for (int i = mergedVobSubPacks.Count - 1; i >= 0; i--)
        {
            if (mergedVobSubPacks[i].SubPicture.SubPictureDateSize <= 2)
            {
                mergedVobSubPacks.RemoveAt(i);
            }
            else if (mergedVobSubPacks[i].SubPicture.SubPictureDateSize <= 67 &&
                     mergedVobSubPacks[i].SubPicture.Delay.TotalMilliseconds < 35)
            {
                mergedVobSubPacks.RemoveAt(i);
            }
        }

        Dispatcher.UIThread.Post(async () =>
        {
            var result = await ShowDialogAsync<OcrWindow, OcrViewModel>(vm => { vm.Initialize(mergedVobSubPacks, idx.Palette, matroskaSubtitleInfo, fileName); });

            if (result.OkPressed)
            {
                _subtitleFileName = Path.GetFileNameWithoutExtension(fileName);
                Subtitles.Clear();
                Subtitles.AddRange(result.OcredSubtitle);
                SelectAndScrollToRow(0);
            }
        });

        return false;
    }

    private void MatroskaProgress(long position, long total)
    {
        // UpdateProgress(position, total, _language.ParsingMatroskaFile);
    }

    private bool LoadTextSTFromMatroska(MatroskaTrackInfo matroskaSubtitleInfo, MatroskaFile matroska)
    {
        ShowStatus(Se.Language.Main.ParsingMatroskaFile);
        var sub = matroska.GetSubtitle(matroskaSubtitleInfo.TrackNumber, MatroskaProgress);

        _subtitle.Paragraphs.Clear();
        Utilities.LoadMatroskaTextSubtitle(matroskaSubtitleInfo, matroska, sub, _subtitle);
        Utilities.ParseMatroskaTextSt(matroskaSubtitleInfo, sub, _subtitle);

        SetSubtitleFormat(
            SubtitleFormats.FirstOrDefault(p => p.Name == Se.Settings.General.DefaultSubtitleFormat) ??
            SelectedSubtitleFormat);
        _converted = true;
        ShowStatus(Se.Language.Main.SubtitleImportedFromMatroskaFile);
        _subtitle.Renumber();
        Subtitles.Clear();
        Subtitles.AddRange(_subtitle.Paragraphs.Select(p => new SubtitleLineViewModel(p, SelectedSubtitleFormat)));


        if (matroska.Path.EndsWith(".mkv", StringComparison.OrdinalIgnoreCase) ||
            matroska.Path.EndsWith(".mks", StringComparison.OrdinalIgnoreCase))
        {
            _subtitleFileName = matroska.Path.Remove(matroska.Path.Length - 4) + SelectedSubtitleFormat.Extension;
        }

        SelectAndScrollToRow(0);

        return true;
    }

    private async Task<bool> ImportSubtitleFromVobSubFile(string vobSubFileName, string? videoFileName)
    {
        var vobSubParser = new VobSubParser(true);
        string idxFileName = Path.ChangeExtension(vobSubFileName, ".idx");
        vobSubParser.OpenSubIdx(vobSubFileName, idxFileName);
        var vobSubMergedPackList = vobSubParser.MergeVobSubPacks();
        var palette = vobSubParser.IdxPalette;
        vobSubParser.VobSubPacks.Clear();

        var languageStreamIds = new List<int>();
        var streamIdDictionary = new Dictionary<int, List<VobSubMergedPack>>();
        foreach (var pack in vobSubMergedPackList)
        {
            if (pack.SubPicture.Delay.TotalMilliseconds > 500 && !languageStreamIds.Contains(pack.StreamId))
            {
                languageStreamIds.Add(pack.StreamId);
            }

            if (!streamIdDictionary.TryGetValue(pack.StreamId, out List<VobSubMergedPack>? value))
            {
                streamIdDictionary.Add(pack.StreamId, new List<VobSubMergedPack>([pack]));
            }
            else
            {
                value.Add(pack);
            }
        }

        if (languageStreamIds.Count == 0)
        {
            return false;
        }

        int streamId;
        if (languageStreamIds.Count > 1)
        {
            var pickResult = await ShowDialogAsync<PickVobSubLanguageWindow, PickVobSubLanguageViewModel>(
                vm => vm.Initialize(streamIdDictionary, palette, vobSubParser.IdxLanguages, vobSubFileName));
            if (!pickResult.OkPressed)
            {
                return false;
            }

            streamId = pickResult.SelectedStreamId;
        }
        else
        {
            streamId = languageStreamIds.First();
        }

        var result = await ShowDialogAsync<OcrWindow, OcrViewModel>(vm => { vm.Initialize(streamIdDictionary[streamId], palette, vobSubFileName); });

        if (result.OkPressed)
        {
            _subtitleFileName = Path.GetFileNameWithoutExtension(vobSubFileName);
            _converted = true;
            Subtitles.Clear();
            Subtitles.AddRange(result.OcredSubtitle);
            SelectAndScrollToRow(0);
            return true;
        }

        return false;
    }

    private void SetSubtitles(Subtitle subtitle, Subtitle? subtitleOriginal = null)
    {
        SubtitleGrid.ItemsSource = null;

        Subtitles.Clear();
        foreach (var p in subtitle.Paragraphs)
        {
            Subtitles.Add(new SubtitleLineViewModel(p, SelectedSubtitleFormat));
        }

        if (subtitleOriginal != null)
        {
            for (var i = 0; i < subtitle.Paragraphs.Count && i < subtitleOriginal.Paragraphs.Count; i++)
            {
                var p = subtitle.Paragraphs[i];
                var original = subtitleOriginal.Paragraphs[i];
                Subtitles[i].OriginalText = original.Text;
            }
        }

        Renumber();
        UpdateGaps();

        SubtitleGrid.ItemsSource = Subtitles;

        _updateAudioVisualizer = true;
    }

    private void SetSubtitles(List<SubtitleLineViewModel> subtitles)
    {
        SubtitleGrid.ItemsSource = null;

        Subtitles.Clear();
        foreach (var p in subtitles)
        {
            Subtitles.Add(p);
        }

        Renumber();
        UpdateGaps();

        SubtitleGrid.ItemsSource = Subtitles;
        _updateAudioVisualizer = true;
    }

    public bool HasChanges()
    {
        var hasChanges = !IsEmpty && _changeSubtitleHash != GetFastHash();
        if (!hasChanges && ShowColumnOriginalText)
        {
            hasChanges = _changeSubtitleHashOriginal != GetFastHashOriginal();
        }

        return hasChanges;
    }


    /// <returns>True, if continue. False, if the use aborts the current action (keep current unchanged work)</returns>
    private async Task<bool> HasChangesContinue()
    {
        var currentSubtitleHash = GetFastHash();
        if (_changeSubtitleHash != currentSubtitleHash && !IsEmpty)
        {
            string promptText = string.Format(Se.Language.General.SaveChangesToX, Se.Language.General.Untitled);
            if (!string.IsNullOrEmpty(_subtitleFileName))
            {
                promptText = string.Format(Se.Language.General.SaveChangesToX, _subtitleFileName);
            }

            var dr = await MessageBox.Show(Window!, Se.Language.General.SaveChangesTitle, promptText,
                MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (dr == MessageBoxResult.Cancel)
            {
                return false;
            }

            if (dr == MessageBoxResult.No)
            {
                return true;
            }

            if (string.IsNullOrEmpty(_subtitleFileName))
            {
                var saved = await SaveSubtitleAs();
                if (!saved)
                {
                    return false;
                }
            }
            else
            {
                await SaveSubtitle();
            }
        }

        return await ContinueNewOrExitOriginal();
    }

    private async Task<bool> ContinueNewOrExitOriginal()
    {
        if (!ShowColumnOriginalText)
        {
            return true;
        }

        var currentSubtitleHash = GetFastHashOriginal();
        if (_changeSubtitleHashOriginal != currentSubtitleHash && !IsEmptyOriginal)
        {
            string promptText = string.Format(Se.Language.General.SaveChangesToXOriginal, Se.Language.General.Untitled);
            if (!string.IsNullOrEmpty(_subtitleFileNameOriginal))
            {
                promptText = string.Format(Se.Language.General.SaveChangesToXOriginal, _subtitleFileNameOriginal);
            }

            var dr = await MessageBox.Show(Window!, Se.Language.General.SaveChangesTitle, promptText,
                MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (dr == MessageBoxResult.Cancel)
            {
                return false;
            }

            if (dr == MessageBoxResult.No)
            {
                return true;
            }

            if (string.IsNullOrEmpty(_subtitleFileNameOriginal))
            {
                var saved = await SaveSubtitleOriginalAs();
                if (!saved)
                {
                    return false;
                }
            }
            else
            {
                await SaveSubtitleOriginal();
            }
        }

        return true;
    }

    private async Task<bool> SaveSubtitle()
    {
        if (Subtitles == null || !Subtitles.Any())
        {
            ShowStatus(Se.Language.Main.NothingToSave);
            return false;
        }

        if (string.IsNullOrEmpty(_subtitleFileName) || _converted)
        {
            var result = await SaveSubtitleAs();
            return result;
        }

        if (_lastOpenSaveFormat == null || _lastOpenSaveFormat.Name != SelectedSubtitleFormat.Name)
        {
            var result = await SaveSubtitleAs();
            return result;
        }

        AutoTrimWhiteSpaces();

        var text = GetUpdateSubtitle(true).ToText(SelectedSubtitleFormat);

        if (Se.Settings.General.ForceCrLfOnSave)
        {
            var lines = text.SplitToLines();
            var sb = new StringBuilder();
            foreach (var line in lines)
            {
                sb.Append(line + "\r\n");
            }

            text = sb.ToString();
        }

        try
        {
            if (SelectedEncoding.DisplayName == TextEncoding.Utf8WithBom)
            {
                await File.WriteAllTextAsync(_subtitleFileName, text, new UTF8Encoding(true));
            }
            else if (SelectedEncoding.DisplayName == TextEncoding.Utf8WithoutBom)
            {
                await File.WriteAllTextAsync(_subtitleFileName, text, new UTF8Encoding(false));
            }
            else
            {
                await File.WriteAllTextAsync(_subtitleFileName, text, SelectedEncoding.Encoding);
            }
        }
        catch (Exception ex)
        {
            var message = string.Format(Se.Language.General.CouldNotSaveFileXErrorY, _subtitleFileName, ex.Message);
            await MessageBox.Show(Window!, Se.Language.General.Error, message, MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return false;
        }

        _changeSubtitleHash = GetFastHash();
        _lastOpenSaveFormat = SelectedSubtitleFormat;

        new BookmarkPersistence(GetUpdateSubtitle(), _subtitleFileName).Save();

        return true;
    }

    private async Task<bool> SaveSubtitleOriginal()
    {
        if (Subtitles == null || !Subtitles.Any())
        {
            ShowStatus(Se.Language.Main.NothingToSaveOriginal);
            return false;
        }

        if (string.IsNullOrEmpty(_subtitleFileNameOriginal))
        {
            return await SaveSubtitleOriginalAs();
        }

        var originalFormat = _subtitleOriginal?.OriginalFormat ?? SelectedSubtitleFormat;
        var text = GetUpdateSubtitleOriginal(true).ToText(originalFormat);

        try
        {
            if (SelectedEncoding.DisplayName == TextEncoding.Utf8WithBom)
            {
                await File.WriteAllTextAsync(_subtitleFileNameOriginal, text, new UTF8Encoding(true));
            }
            else if (SelectedEncoding.DisplayName == TextEncoding.Utf8WithoutBom)
            {
                await File.WriteAllTextAsync(_subtitleFileNameOriginal, text, new UTF8Encoding(false));
            }
            else
            {
                await File.WriteAllTextAsync(_subtitleFileNameOriginal, text, SelectedEncoding.Encoding);
            }
        }
        catch (Exception ex)
        {
            var message = string.Format(Se.Language.General.CouldNotSaveFileXErrorY, _subtitleFileNameOriginal, ex.Message);
            await MessageBox.Show(Window!, Se.Language.General.Error, message, MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return false;
        }

        _changeSubtitleHashOriginal = GetFastHashOriginal();
        return true;
    }

    public Subtitle GetUpdateSubtitle(bool subtractVideoOffset = false)
    {
        _subtitle.Paragraphs.Clear();
        foreach (var line in Subtitles)
        {
            var p = line.ToParagraph(SelectedSubtitleFormat);
            _subtitle.Paragraphs.Add(p);
        }

        return _subtitle;
    }

    public Subtitle GetUpdateSubtitleOriginal(bool subtractVideoOffset = false)
    {
        _subtitleOriginal ??= new Subtitle();
        _subtitleOriginal.OriginalFormat ??= SelectedSubtitleFormat;
        var originalFormat = _subtitleOriginal.OriginalFormat ?? SelectedSubtitleFormat;

        _subtitleOriginal.Paragraphs.Clear();
        foreach (var line in Subtitles)
        {
            var p = line.ToParagraphOriginal(originalFormat);
            _subtitleOriginal.Paragraphs.Add(p);
        }

        return _subtitleOriginal;
    }

    private async Task<bool> SaveSubtitleAs()
    {
        var newFileName = "New";
        var behavior = Se.Settings.General.SaveAsBehavior;
        if (behavior == nameof(SaveAsBehaviourType.UseSubtitleFileNameThenVideoFileName))
        {
            if (!string.IsNullOrEmpty(_subtitleFileName))
            {
                newFileName = GetFileNameWithoutExtension(_subtitleFileName);
            }
            else if (!string.IsNullOrEmpty(_videoFileName))
            {
                newFileName = GetFileNameWithoutExtension(_videoFileName);
            }
        }
        else if (behavior == nameof(SaveAsBehaviourType.UseVideoFileNameThenSubtitleFileName))
        {
            if (!string.IsNullOrEmpty(_videoFileName))
            {
                newFileName = GetFileNameWithoutExtension(_videoFileName);
            }
            else if (!string.IsNullOrEmpty(_subtitleFileName))
            {
                newFileName = GetFileNameWithoutExtension(_subtitleFileName);
            }
        }
        else if (behavior == nameof(SaveAsBehaviourType.UseVideoFileName))
        {
            if (!string.IsNullOrEmpty(_videoFileName))
            {
                newFileName = GetFileNameWithoutExtension(_videoFileName);
            }
        }
        else if (behavior == nameof(SaveAsBehaviourType.UseSubtitleFileName))
        {
            if (!string.IsNullOrEmpty(_subtitleFileName))
            {
                newFileName = GetFileNameWithoutExtension(_subtitleFileName);
            }
        }

        var language = LanguageAutoDetect.AutoDetectGoogleLanguageOrNull2(GetUpdateSubtitle());
        if (!string.IsNullOrEmpty(language) && Se.Settings.General.SaveAsAppendLanguageCode != nameof(SaveAsLanguageAppendType.None))
        {
            var l = Iso639Dash2LanguageCode.List.FirstOrDefault(p => p.TwoLetterCode == language);
            if (l != null)
            {
                if (newFileName.EndsWith("." + l.EnglishName, StringComparison.OrdinalIgnoreCase))
                {
                    newFileName = newFileName.Substring(0, newFileName.Length - (l.EnglishName.Length + 1));
                }

                if (newFileName.EndsWith("." + l.TwoLetterCode, StringComparison.OrdinalIgnoreCase))
                {
                    newFileName = newFileName.Substring(0, newFileName.Length - (l.TwoLetterCode.Length + 1));
                }

                if (newFileName.EndsWith("." + l.ThreeLetterCode, StringComparison.OrdinalIgnoreCase))
                {
                    newFileName = newFileName.Substring(0, newFileName.Length - (l.ThreeLetterCode.Length + 1));
                }

                if (l.BibliographicCode != l.ThreeLetterCode &&
                    newFileName.EndsWith("." + l.BibliographicCode, StringComparison.OrdinalIgnoreCase))
                {
                    newFileName = newFileName.Substring(0, newFileName.Length - (l.BibliographicCode.Length + 1));
                }

                if (Se.Settings.General.SaveAsAppendLanguageCode == nameof(SaveAsLanguageAppendType.TwoLetterLanguageCode))
                {
                    newFileName += "." + l.TwoLetterCode;
                }
                else if (Se.Settings.General.SaveAsAppendLanguageCode == nameof(SaveAsLanguageAppendType.ThreeLEtterLanguageCode))
                {
                    newFileName += "." + l.ThreeLetterCode;
                }
                else if (Se.Settings.General.SaveAsAppendLanguageCode == nameof(SaveAsLanguageAppendType.ThreeLetterLanguageCodeBibliographic))
                {
                    newFileName += "." + l.BibliographicCode;
                }
                else if (Se.Settings.General.SaveAsAppendLanguageCode == nameof(SaveAsLanguageAppendType.FullLanguageName))
                {
                    newFileName += "." + l.EnglishName;
                }
            }
        }

        var title = Se.Language.General.SaveFileAsTitle;
        if (ShowColumnOriginalText)
        {
            title = Se.Language.General.SaveTranslationAsTitle;
        }

        var subtitleFormat = SelectedSubtitleFormat;
        if (!_formatChangedByUser)
        {
            subtitleFormat = SubtitleFormats.FirstOrDefault(p => p.FriendlyName == Se.Settings.General.DefaultSaveAsFormat)
                             ?? SelectedSubtitleFormat;
        }

        var saveAsResult = await _fileHelper.PickSaveSubtitleFileAs(
            Window!,
            subtitleFormat,
            newFileName,
            title);

        _shortcutManager.ClearKeys();

        if (saveAsResult == null)
        {
            return false;
        }

        if (_formatChangedByUser)
        {
            _formatChangedByUser = false;
        }

        _subtitleFileName = saveAsResult.FileName;
        _subtitle.FileName = saveAsResult.FileName;
        _converted = false;
        _lastOpenSaveFormat = saveAsResult.SubtitleFormat;
        SetSubtitleFormat(saveAsResult.SubtitleFormat);
        var result = await SaveSubtitle();
        AddToRecentFiles(true);
        return result;
    }

    private string GetFileNameWithoutExtension(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return fileName;
        }

        var allowedExtensions = SubtitleFormats.Select(f => f.Extension).ToList();
        allowedExtensions.AddRange(Utilities.VideoFileExtensions);
        allowedExtensions.AddRange(Utilities.AudioFileExtensions);
        allowedExtensions.Add(".xlsx");
        allowedExtensions.Add(".ods");

        foreach (var ext in allowedExtensions)
        {
            if (fileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
            {
                fileName = fileName.Substring(0, fileName.Length - ext.Length);
                break;
            }
        }

        return fileName;
    }

    private async Task<bool> SaveSubtitleOriginalAs()
    {
        var newFileName = "New" + SelectedSubtitleFormat.Extension;
        if (!string.IsNullOrEmpty(_subtitleFileNameOriginal))
        {
            newFileName = Path.GetFileNameWithoutExtension(_subtitleFileNameOriginal) +
                          SelectedSubtitleFormat.Extension;
        }
        else if (!string.IsNullOrEmpty(_videoFileName))
        {
            newFileName = Path.GetFileNameWithoutExtension(_videoFileName) + SelectedSubtitleFormat.Extension;
        }

        var fileName = await _fileHelper.PickSaveSubtitleFile(
            Window!,
            SelectedSubtitleFormat,
            newFileName,
            Se.Language.General.SaveOriginalAsTitle);

        if (string.IsNullOrEmpty(fileName))
        {
            return false;
        }

        var oldSubtitleFileNameOriginal = _subtitleFileNameOriginal;
        var oldSubtitleOriginalFileName = _subtitleOriginal.FileName;
        var oldLastOpenSaveFormat = _lastOpenSaveFormat;

        _subtitleFileNameOriginal = fileName;
        _subtitleOriginal ??= new Subtitle();
        _subtitleOriginal.FileName = fileName;

        _lastOpenSaveFormat = SelectedSubtitleFormat;
        var result = await SaveSubtitleOriginal();
        if (!result)
        {
            _subtitleFileNameOriginal = oldSubtitleFileNameOriginal;
            _subtitleOriginal.FileName = oldSubtitleOriginalFileName;
            _lastOpenSaveFormat = oldLastOpenSaveFormat;
            return false;
        }

        AddToRecentFiles(true);
        return true;
    }

    private void AddToRecentFiles(bool updateMenu)
    {
        if (_loading)
        {
            return;
        }

        var idx = SelectedSubtitleIndex ?? 0;
        Se.Settings.File.AddToRecentFiles(
            _subtitleFileName ?? string.Empty,
            _subtitleFileNameOriginal ?? string.Empty,
            _videoFileName ?? string.Empty,
            idx,
            SelectedEncoding.DisplayName,
            Se.Settings.General.CurrentVideoOffsetInMs,
            IsSmpteTimingEnabled,
            _audioTrack?.Id ?? -1);
        Se.SaveSettings();

        if (updateMenu)
        {
            InitMenu.UpdateRecentFiles(this);
            if (OperatingSystem.IsMacOS())
                Layout.InitNativeMacMenu.UpdateRecentFiles(this);
        }
    }

    private void ShowStatus(string message, int delayMs = 3000)
    {
        Task.Run(() => ShowStatusWithWaitAsync(message, delayMs));
    }

    private async Task ShowStatusWithWaitAsync(string message, int delayMs = 3000)
    {
        // Cancel any previous animation
        _statusFadeCts?.Cancel();
        _statusFadeCts = new CancellationTokenSource();
        var token = _statusFadeCts.Token;

        Dispatcher.UIThread.Post(() =>
        {
            StatusTextLeft = message;
            StatusTextLeftLabel.Opacity = 1;
            StatusTextLeftLabel.IsVisible = true;
        }, DispatcherPriority.MaxValue);

        try
        {
            await Task.Delay(delayMs, token); // Wait 3 seconds, cancellable
            Dispatcher.UIThread.Post(() => { StatusTextLeft = string.Empty; }, DispatcherPriority.Background);
        }
        catch
        {
            // Ignore
        }
    }

    internal async void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        _videoOpenTokenSource?.Cancel();
        AddToRecentFiles(false);

        if (Window != null)
        {
            Se.Settings.General.ShowColumnStartTime = ShowColumnStartTime;
            Se.Settings.General.ShowColumnEndTime = ShowColumnEndTime;
            Se.Settings.General.ShowColumnDuration = ShowColumnDuration;
            Se.Settings.General.ShowColumnGap = ShowColumnGap;
            Se.Settings.General.ShowColumnActor = ShowColumnActor;
            Se.Settings.General.ShowColumnStyle = ShowColumnStyle;
            Se.Settings.General.ShowColumnCps = ShowColumnCps;
            Se.Settings.General.ShowColumnWpm = ShowColumnWpm;
            Se.Settings.General.ShowColumnLayer = ShowColumnLayer;
            Se.Settings.General.SelectCurrentSubtitleWhilePlaying = SelectCurrentSubtitleWhilePlaying;
            Se.Settings.Waveform.ShowToolbar = IsWaveformToolbarVisible;
            Se.Settings.Waveform.CenterVideoPosition = WaveformCenter;

            UiUtil.SaveWindowPosition(Window);
            Se.Settings.General.UndockVideoControls = Se.Settings.General.RememberPositionAndSize && AreVideoControlsUndocked;
            Se.Settings.Appearance.CurrentLayoutPositions = InitLayout.SaveLayoutPositions(ContentGrid.Children.FirstOrDefault() as Grid);

            if (_findViewModel != null)
            {
                UiUtil.SaveWindowPosition(_findViewModel.Window);
            }

            if (_videoPlayerUndockedViewModel != null)
            {
                UiUtil.SaveWindowPosition(_videoPlayerUndockedViewModel.Window);
            }

            if (_audioVisualizerUndockedViewModel != null)
            {
                UiUtil.SaveWindowPosition(_audioVisualizerUndockedViewModel.Window);
            }

            if (AudioVisualizer != null && AudioVisualizer.HasSpectrogram())
            {
                Se.Settings.Waveform.LastDisplayMode = AudioVisualizer.GetDisplayMode().ToString();
            }
        }

        Se.SaveSettings();

        if (HasChanges() && Window != null)
        {
            // Cancel the closing to show the dialog
            e.Cancel = true;

            // This is an async-void event handler — any throw from the awaits
            // below (MessageBox or SaveSubtitle: disk full, permission denied,
            // OneDrive sync conflict, ...) lands in the dispatcher's
            // unhandled-exception path and leaves the user staring at a
            // half-closed window. Catch, log, and force-close so shutdown
            // always progresses.
            try
            {
                var result = await MessageBox.Show(
                    Window,
                    Se.Language.General.SaveChangesTitle,
                    Se.Language.General.SaveChangesMessage,
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (result == MessageBoxResult.Cancel)
                {
                    // Stay cancelled - window won't close
                    return;
                }

                if (result == MessageBoxResult.Yes)
                {
                    await SaveSubtitle();
                }

                CleanUp();

                Window.Closing -= OnClosing;
                Window.Close();
            }
            catch (Exception ex)
            {
                Se.LogError(ex, "MainViewModel.OnClosing");
                // Don't trap the user in a frozen window — clean up and let it close.
                CleanUp();
                if (Window != null)
                {
                    Window.Closing -= OnClosing;
                    Window.Close();
                }
            }
        }
        else
        {
            CleanUp();
        }
    }

    private void CleanUp()
    {
        _positionTimer.Stop();
        _slowTimer.Stop();

        if (_findViewModel != null)
        {
            _findViewModel.Window?.Close();
        }

        if (_videoPlayerUndockedViewModel != null)
        {
            _videoPlayerUndockedViewModel.AllowClose = true;
            _videoPlayerUndockedViewModel.Window?.Close();
        }

        if (_audioVisualizerUndockedViewModel != null)
        {
            _audioVisualizerUndockedViewModel.AllowClose = true;
            _audioVisualizerUndockedViewModel.Window?.Close();
        }

        GetVideoPlayerControl()?.VideoPlayer.CloseFile();

        _ = Task.Run(() =>
        {
            try
            {
                _autoBackupService.CleanAutoBackupFolder();
                CheckAndRenameDamagedFiles();
            }
            catch (Exception ex)
            {
                Se.LogError(ex, "Auto-backup cleanup failed");
            }
        });
    }

    internal void OnLoaded()
    {
        if (OperatingSystem.IsMacOS())
            Layout.InitNativeMacMenu.Sync(this);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && string.IsNullOrEmpty(Se.Settings.General.LibMpvPath))
        {
            Dispatcher.UIThread.Post(async void () =>
            {
                try
                {
                    var answer = await MessageBox.Show(
                        Window!,
                        "Download mpv?",
                        $"{Environment.NewLine}\"Subtitle Edit\" requires mpv to play video/audio.{Environment.NewLine}{Environment.NewLine}Download and use mpv?",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question);

                    if (answer != MessageBoxResult.Yes)
                    {
                        return;
                    }

                    var result =
                        await ShowDialogAsync<DownloadLibMpvWindow, DownloadLibMpvViewModel>();

                    if (!string.IsNullOrEmpty(result.LibMpvFileName))
                    {
                        InitializeLibMpv();
                        SetLayout(Se.Settings.General.LayoutNumber);
                    }
                }
                catch (Exception e)
                {
                    Se.LogError(e);
                }
            }, DispatcherPriority.Background);
        }

        if (AudioVisualizer != null)
        {
            AudioVisualizer.IsReadOnly = LockTimeCodes;
        }

        if (Program.FileOpenedViaActivation && !string.IsNullOrEmpty(Program.PendingFileToOpen))
        {
            Dispatcher.UIThread.Post(async void () =>
            {
                if (Se.Settings.General.RememberPositionAndSize)
                {
                    if (Se.Settings.General.UndockVideoControls)
                    {
                        VideoUndockControls();
                    }

                    InitLayout.RestoreLayoutPositions(Se.Settings.Appearance.CurrentLayoutPositions, ContentGrid.Children.FirstOrDefault() as Grid);
                }

                await Task.Delay(100);
                await SubtitleOpen(Program.PendingFileToOpen);
                Program.PendingFileToOpen = null;
            });
        }
        else if (Se.Settings.File.OpenLastFileOnStart)
        {
            var first = Se.Settings.File.RecentFiles.FirstOrDefault();
            if (first != null && File.Exists(first.SubtitleFileName))
            {
                Dispatcher.UIThread.Post(async void () =>
                {
                    try
                    {
                        // Delay to allow Activated event to set FileOpenedViaActivation flag, the Activated event fires asynchronously during startup
                        // and may not have completed by the time OnLoaded runs so wait and recheck the flag
                        await Task.Delay(100);

                        // Check if file was opened via activation (e.g., from Finder on macOS)
                        if (Program.FileOpenedViaActivation)
                        {
                            return;
                        }

                        bool skipLoadVideo = false;
                        _videoFileName = first.VideoFileName;
                        await Task.Delay(25);

                        if (Se.Settings.General.RememberPositionAndSize)
                        {
                            if (Se.Settings.General.UndockVideoControls)
                            {
                                VideoUndockControls();
                                skipLoadVideo = true;
                            }

                            InitLayout.RestoreLayoutPositions(Se.Settings.Appearance.CurrentLayoutPositions, ContentGrid.Children.FirstOrDefault() as Grid);
                        }

                        await SubtitleOpen(first.SubtitleFileName, first.VideoFileName, first.SelectedLine, null, skipLoadVideo, first.AudioTrack);
                        var vp = GetVideoPlayerControl();
                        if (!string.IsNullOrEmpty(_videoFileName) && SelectedSubtitle != null && vp != null)
                        {
                            await vp.WaitForPlayersReadyAsync();
                            await Task.Delay(200);
                            var s = SelectedSubtitle;
                            if (vp != null && s != null)
                            {
                                vp.Position = s.StartTime.TotalSeconds;
                                Dispatcher.UIThread.Post(() => { vp.Position = SelectedSubtitle.StartTime.TotalSeconds; });
                            }
                        }

                        SetRecentFileProperties(first);
                    }
                    catch (Exception e)
                    {
                        Se.LogError(e);
                    }
                });
            }
            else
            {
                if (Se.Settings.General.RememberPositionAndSize)
                {
                    Dispatcher.UIThread.Post(void () =>
                    {
                        UiUtil.RestoreWindowPosition(Window);
                        if (Se.Settings.General.UndockVideoControls)
                        {
                            VideoUndockControls();
                        }

                        InitLayout.RestoreLayoutPositions(Se.Settings.Appearance.CurrentLayoutPositions, ContentGrid.Children.FirstOrDefault() as Grid);
                    });
                }

                Se.Settings.File.RecentFiles = Se.Settings.File.RecentFiles
                    .Where(p => File.Exists(p.SubtitleFileName))
                    .ToList();
            }
        }
        else
        {
            if (Se.Settings.General.RememberPositionAndSize)
            {
                Dispatcher.UIThread.Post(void () =>
                {
                    UiUtil.RestoreWindowPosition(Window);
                    if (Se.Settings.General.UndockVideoControls)
                    {
                        VideoUndockControls();
                    }

                    InitLayout.RestoreLayoutPositions(Se.Settings.Appearance.CurrentLayoutPositions, ContentGrid.Children.FirstOrDefault() as Grid);
                }, DispatcherPriority.Loaded);
            }
        }

        ComboBoxSubtitleFormatChanged(null, new SelectionChangedEventArgs(Avalonia.Controls.Primitives.SelectingItemsControl.SelectionChangedEvent, Array.Empty<object>(), Array.Empty<object>()));

        Task.Run(async () =>
        {
            if (Se.Settings.Appearance.RightToLeft)
            {
                Se.Settings.Appearance.RightToLeft = !Se.Settings.Appearance.RightToLeft;
                RightToLeftToggle();
            }

            await Task.Delay(1000); // delay 1 second (off UI thread)          

            _undoRedoManager.StartChangeDetection();
            _loading = false;

            Dispatcher.UIThread.Post(void () =>
            {
                if (Window != null)
                {
                    Window.Activate();
                    SubtitleGrid.Focus();

                    SurroundWith1Text = string.Format(Se.Language.Options.Shortcuts.SurroundWithXY, Se.Settings.Surround1Left, Se.Settings.Surround1Right);
                    SurroundWith2Text = string.Format(Se.Language.Options.Shortcuts.SurroundWithXY, Se.Settings.Surround2Left, Se.Settings.Surround2Right);
                    SurroundWith3Text = string.Format(Se.Language.Options.Shortcuts.SurroundWithXY, Se.Settings.Surround3Left, Se.Settings.Surround3Right);
                }
            });
        });
    }

    private static void CheckAndRenameDamagedFiles()
    {
        var filesToCheck = new List<string>();
        if (Directory.Exists(Se.DictionariesFolder))
        {
            filesToCheck.AddRange(Directory.GetFiles(Se.DictionariesFolder, "*.xml"));
        }

        foreach (var file in filesToCheck)
        {
            try
            {
                using var stream = File.OpenRead(file);
                var xmlDoc = XDocument.Load(stream);
            }
            catch (Exception ex)
            {
                var damagedFileName = file + ".damaged_" + DateTime.Now.ToString("yyyyMMddHHmmss");
                Se.LogError(ex, $"Damaged file detected: {file}  -- renamed to {damagedFileName}");
                File.Move(file, damagedFileName);
            }
        }


        filesToCheck = new List<string>();
        if (Directory.Exists(Se.TranslationFolder))
        {
            filesToCheck.AddRange(Directory.GetFiles(Se.TranslationFolder, "*.json"));
        }

        foreach (var file in filesToCheck)
        {
            try
            {
                using var stream = File.OpenRead(file);
                var jsonDoc = JsonDocument.Parse(stream);
            }
            catch (Exception ex)
            {
                var damagedFileName = file + ".damaged_" + DateTime.Now.ToString("yyyyMMddHHmmss");
                Se.LogError(ex, $"Damaged file detected: {file}  -- renamed to {damagedFileName}");
                File.Move(file, damagedFileName);
            }
        }
    }

    private static bool IsValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }

    private async Task VideoOpenFile(string videoFileName, int desiredAudioTrackId = -1) // OpenVideoFile
    {
        var vp = GetVideoPlayerControl();
        if (vp == null)
        {
            return;
        }

        _videoOpenTokenSource?.Cancel();
        _audioTrack = null;
        await vp.Open(videoFileName);
        _videoFileName = videoFileName;
        RefreshSubtitlePreview();

        if (IsValidUrl(videoFileName))
        {
            _videoFileName = videoFileName;
            await AddEmptyWaveform();
            IsVideoLoaded = true;
            return;
        }

        IsVideoLoaded = true;

        // Wait until mpv has actually parsed the file before reading the track list.
        // GetAudioTracks() reads "track-list/count" which is 0 until the file is loaded,
        // so racing past it produces a bare-hash peak filename ({hash}.wav) on the first
        // open vs. a track-suffixed one ({hash}-N.wav) on later opens, causing the
        // waveform to be regenerated on re-open.
        await vp.WaitForPlayersReadyAsync();

        // Resolve _audioTrack before LoadWaveformAndSpectrogram so it sees the right FfIndex (and we don't race LoadAudioTrackMenuItems).
        if (vp.VideoPlayer is LibMpvDynamicPlayer mpv)
        {
            var tracks = mpv.GetAudioTracks();
            if (tracks.Count > 0)
            {
                var chosen = desiredAudioTrackId >= 0
                    ? tracks.FirstOrDefault(t => t.Id == desiredAudioTrackId)
                    : null;
                chosen ??= tracks.FirstOrDefault(t => t.IsSelected) ?? tracks[0];

                if (desiredAudioTrackId >= 0 && chosen.Id != -1)
                {
                    mpv.SetAudioTrack(chosen.Id);
                }

                _audioTrack = chosen;
            }
        }

        var _ = Task.Run(() =>
        {
            Dispatcher.UIThread.Post(() => LoadWaveformAndSpectrogram(videoFileName));
            GetMediaInformation(videoFileName);
            LoadAudioTrackMenuItems();
        });
    }

    private void LoadWaveformAndSpectrogram(string videoFileName)
    {
        var trackNumber = _audioTrack?.FfIndex ?? -1;
        var peakWaveFileName = WavePeakGenerator2.GetPeakWaveFileName(videoFileName, trackNumber);
        var spectrogramFileName = WavePeakGenerator2.SpectrogramDrawer.GetSpectrogramFileName(videoFileName, trackNumber);
        if (!File.Exists(peakWaveFileName) || (Se.Settings.Waveform.GenerateSpectrogram && !File.Exists(spectrogramFileName)))
        {
            if (FfmpegHelper.IsFfmpegInstalled())
            {
                lock (_waveformsBeingGeneratedLock)
                {
                    if (!_waveformsBeingGenerated.Add(peakWaveFileName))
                    {
                        return; // already generating for this peak-wave file
                    }
                }

                var tempWaveFileName = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.wav");
                var process = WaveFileExtractor.GetCommandLineProcess(videoFileName, trackNumber, tempWaveFileName,
                    Configuration.Settings.General.VlcWaveTranscodeSettings, out _);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Task.Run(async () => { await ExtractWaveformAndSpectrogramAndShotChanges(process, tempWaveFileName, peakWaveFileName, spectrogramFileName, videoFileName); });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }
        else
        {
            ShowStatus(Se.Language.Main.LoadingWaveInfoFromCache);
            var wavePeaks = WavePeakData2.FromDisk(peakWaveFileName);
            if (AudioVisualizer != null)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    AudioVisualizer.WavePeaks = wavePeaks;

                    if (IsSmpteTimingEnabled)
                    {
                        AudioVisualizer.UseSmpteDropFrameTime();
                    }

                    var spectrogram = SpectrogramData2.FromDisk(spectrogramFileName);
                    if (spectrogram != null)
                    {
                        spectrogram.Load();
                        AudioVisualizer.SetSpectrogram(spectrogram);
                    }

                    InitializeWaveformDisplayMode();

                    AudioVisualizer.ShotChanges = ShotChangesHelper.FromDisk(videoFileName);
                    if (AudioVisualizer.ShotChanges.Count == 0)
                    {
                        ExtractShotChanges(videoFileName, trackNumber);
                    }

                    _updateAudioVisualizer = true;
                });
            }
        }
    }

    private void GetMediaInformation(string videoFileName)
    {
        try
        {
            _mediaInfo = FfmpegMediaInfo2.Parse(videoFileName);
            SelectedFrameRate = _mediaInfo?.FramesRate.ToString(CultureInfo.InvariantCulture) ?? FrameRates[0];
            Se.Settings.General.CurrentFrameRate = (double)(_mediaInfo?.FramesRate ?? 23.976m);
            Configuration.Settings.General.CurrentFrameRate = (double)(_mediaInfo?.FramesRate ?? 23.976m);
            _updateAudioVisualizer = true;

            if (IsFormatAssa)
            {
                SetAssaResolution(true);
            }
        }
        catch
        {
            _mediaInfo = null;
        }
    }

    private void SetAssaResolution(bool checkSettings)
    {
        if (checkSettings && !Se.Settings.Assa.AutoSetResolution)
        {
            return;
        }

        if (_mediaInfo == null || _mediaInfo.Dimension.Width <= 0)
        {
            return;
        }

        var oldHeader = _subtitle.Header;
        _subtitle.Header = AdvancedSubStationAlpha.SetResolution(_subtitle.Header, _mediaInfo.Dimension.Width, _mediaInfo.Dimension.Height);

        if (Se.Settings.Assa.AutoSetResolutionConvert && oldHeader != _subtitle.Header)
        {
            if (string.IsNullOrEmpty(oldHeader) || !oldHeader.Contains("[V4+ Styles]", StringComparison.OrdinalIgnoreCase))
            {
                oldHeader = AdvancedSubStationAlpha.DefaultHeader;
            }

            var oldWidth = AdvancedSubStationAlpha.DefaultWidth;
            var oldHeight = AdvancedSubStationAlpha.DefaultHeight;

            var playResX = AdvancedSubStationAlpha.GetTagValueFromHeader("PlayResX", "[Script Info]", oldHeader);
            if (int.TryParse(playResX, out var width) && width >= 125 && width <= 4096)
            {
                oldWidth = width;
            }

            var playResY = AdvancedSubStationAlpha.GetTagValueFromHeader("PlayResY", "[Script Info]", oldHeader);
            if (int.TryParse(playResY, out var height) && height >= 125 && height <= 4096)
            {
                oldHeight = height;
            }

            AssaResamplerHelper.ApplyResampling(_subtitle, oldWidth, oldHeight, _mediaInfo.Dimension.Width, _mediaInfo.Dimension.Height, true, true, true, true);
        }
    }

    internal void ComboBoxFrameRateSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (double.TryParse(SelectedFrameRate, NumberStyles.Any, CultureInfo.InvariantCulture, out var frameRate))
        {
            Se.Settings.General.CurrentFrameRate = frameRate;
            Configuration.Settings.General.CurrentFrameRate = frameRate;
            _updateAudioVisualizer = true;
        }
    }

    private void LoadAudioTrackMenuItems()
    {
        try
        {
            var vp = GetVideoPlayerControl();
            if (vp?.VideoPlayer is LibMpvDynamicPlayer mpv)
            {
                var audioTracks = mpv.GetAudioTracks();
                if (audioTracks.Count == 0)
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        IsAudioTracksVisible = false;
                        AudioTraksMenuItem.Items.Clear();
                        if (OperatingSystem.IsMacOS())
                            Layout.InitNativeMacMenu.UpdateAudioTracksMenu(this, [], null);
                    });
                    return;
                }

                var selectedTrack = audioTracks.FirstOrDefault(p => p.FfIndex == (_audioTrack?.FfIndex ?? -1));
                if (selectedTrack == null)
                {
                    _audioTrack = audioTracks[0];
                }

                Dispatcher.UIThread.Post(() =>
                {
                    AudioTraksMenuItem.Items.Clear();
                    foreach (var audioTrack in audioTracks)
                    {
                        var trackName = string.Format(Se.Language.Main.AudioTrackX, audioTrack.Id);
                        if (!string.IsNullOrEmpty(audioTrack.Language))
                        {
                            var languageName = Iso639Dash2LanguageCode.List.FirstOrDefault(p => p.ThreeLetterCode == audioTrack.Language);
                            if (string.IsNullOrEmpty(languageName?.EnglishName))
                            {
                                trackName += $" - {audioTrack.Language}";
                            }
                            else
                            {
                                trackName += $" - {languageName.EnglishName}";
                            }
                        }

                        if (!string.IsNullOrEmpty(audioTrack.Title))
                        {
                            trackName += $" - {audioTrack.Title}";
                        }

                        var menuItem = new MenuItem();
                        menuItem.Header = trackName;
                        menuItem.Command = PickAudioTrackCommand;
                        menuItem.CommandParameter = audioTrack;
                        if (audioTrack.FfIndex == (_audioTrack?.FfIndex ?? -1))
                        {
                            menuItem.Icon = new Optris.Icons.Avalonia.Icon
                            {
                                Value = IconNames.CheckBold,
                                VerticalAlignment = VerticalAlignment.Center,
                            };
                        }

                        AudioTraksMenuItem.Items.Add(menuItem);
                    }

                    IsAudioTracksVisible = AudioTraksMenuItem.Items.Count > 1;
                    if (OperatingSystem.IsMacOS())
                        Layout.InitNativeMacMenu.UpdateAudioTracksMenu(this, audioTracks, _audioTrack);
                });
            }
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "UpdateAudioTrackMenuItems failed");
            Dispatcher.UIThread.Post(() => { IsAudioTracksVisible = AudioTraksMenuItem.Items.Count > 1; });
        }
    }

    private void InitializeWaveformDisplayMode()
    {
        if (AudioVisualizer != null && AudioVisualizer.HasSpectrogram())
        {
            if (Se.Settings.Waveform.LastDisplayMode == WaveformDisplayMode.WaveformAndSpectrogram.ToString())
            {
                WaveformShowWaveformAndSpectrogram();
            }
            else if (Se.Settings.Waveform.LastDisplayMode == WaveformDisplayMode.OnlySpectrogram.ToString())
            {
                WaveformShowOnlySpectrogram();
            }
            else
            {
                WaveformShowOnlyWaveform();
            }
        }
        else
        {
            ShowWaveformOnlyWaveform = false;
            ShowWaveformOnlySpectrogram = false;
            ShowWaveformWaveformAndSpectrogram = false;
            ShowWaveformDisplayModeSeparator = false;
        }
    }

    private async Task AddEmptyWaveform()
    {
        var vp = GetVideoPlayerControl();
        if (vp == null || AudioVisualizer == null)
        {
            return;
        }

        await vp.WaitForPlayersReadyAsync(10_000);
        if (vp.VideoPlayer.Duration > 0)
        {
            var peakWaveFileName = WavePeakGenerator2.GetPeakWaveFileName(_videoFileName ?? string.Empty, _audioTrack?.FfIndex ?? -1);
            AudioVisualizer.ZoomFactor = 1.0;
            AudioVisualizer.VerticalZoomFactor = 1.0;
            AudioVisualizer.WavePeaks = WavePeakGenerator2.GenerateEmptyPeaks(peakWaveFileName, (int)vp.VideoPlayer.Duration);
        }
    }

    private async Task ExtractWaveformAndSpectrogramAndShotChanges(
        Process process,
        string tempWaveFileName,
        string peakWaveFileName,
        string spectrogramFolderName,
        string videoFileName)
    {
        IsWaveformGenerating = true;
        WaveformGeneratingText = Se.Language.Main.ExtractingWaveInfo;

        try
        {
            process.Start();

            _videoOpenTokenSource = new CancellationTokenSource();
            while (!process.HasExited)
            {
                await Task.Delay(100, _videoOpenTokenSource.Token);
            }

            if (process.ExitCode != 0)
            {
                ShowStatus(Se.Language.Main.FailedToExtractWaveInfo);
                Dispatcher.UIThread.Post(async () =>
                {
                    await AddEmptyWaveform();
                });
                return;
            }

            if (_videoOpenTokenSource.IsCancellationRequested)
            {
                DeleteTempFile(tempWaveFileName);
                return;
            }

            if (File.Exists(tempWaveFileName))
            {
                WaveformGeneratingText = Se.Language.Main.GeneratingWaveformDotDotDot;
                using var waveFile = new WavePeakGenerator2(tempWaveFileName);
                var wavePeaks = waveFile.GeneratePeaks(0, peakWaveFileName);

                if (Se.Settings.Waveform.GenerateSpectrogram)
                {
                    WaveformGeneratingText = Se.Language.Main.GeneratingSpectrogramDotDotDot;
                    var spectrogram = waveFile.GenerateSpectrogram(0, spectrogramFolderName, _videoOpenTokenSource.Token);
                    AudioVisualizer?.SetSpectrogram(spectrogram);
                }

                Dispatcher.UIThread.Post(() =>
                {
                    ShowWaveformOnlyWaveform = false;
                    ShowWaveformOnlySpectrogram = false;
                    ShowWaveformWaveformAndSpectrogram = false;

                    if (AudioVisualizer != null)
                    {
                        AudioVisualizer.WavePeaks = wavePeaks;
                        if (IsSmpteTimingEnabled)
                        {
                            AudioVisualizer.UseSmpteDropFrameTime();
                        }

                        InitializeWaveformDisplayMode();
                    }

                    _updateAudioVisualizer = true;
                }, DispatcherPriority.Background);


                if (_videoOpenTokenSource.IsCancellationRequested)
                {
                    DeleteTempFile(tempWaveFileName);
                    return;
                }
            }

            ExtractShotChanges(videoFileName, _audioTrack?.FfIndex ?? -1);
        }
        finally
        {
            IsWaveformGenerating = false;
            WaveformGeneratingText = string.Empty;
            DeleteTempFile(tempWaveFileName);
            lock (_waveformsBeingGeneratedLock)
            {
                _waveformsBeingGenerated.Remove(peakWaveFileName);
            }
        }
    }

    private void ExtractShotChanges(string videoFileName, int audioTrackNumber)
    {
        if (Se.Settings.Waveform.ShotChangesAutoGenerate)
        {
            WaveformGeneratingText = Se.Language.Main.ExtractingShotChanges;

            var threshold = Se.Settings.Waveform.ShotChangesSensitivity.ToString(CultureInfo.InvariantCulture);
            var argumentsFormat = Se.Settings.Video.ShowChangesFFmpegArguments;
            var arguments = string.Format(argumentsFormat, videoFileName, threshold);

            var ffmpegProcess = FfmpegGenerator.GetProcess(arguments, OutputHandler);
#pragma warning disable CA1416 // Validate platform compatibility
            ffmpegProcess.Start();
#pragma warning restore CA1416 // Validate platform compatibility
            ffmpegProcess.BeginOutputReadLine();
            ffmpegProcess.BeginErrorReadLine();

            _ = Task.Run(async () =>
            {
                while (!ffmpegProcess.HasExited)
                {
                    if (_videoOpenTokenSource.IsCancellationRequested)
                    {
                        try
                        {
                            ffmpegProcess.Kill(true);
                        }
                        catch
                        {
                            // ignore
                        }

                        break;
                    }

                    await Task.Delay(100, _videoOpenTokenSource.Token).ConfigureAwait(false);
                }

                if (!_videoOpenTokenSource.IsCancellationRequested && AudioVisualizer != null && AudioVisualizer.ShotChanges != null)
                {
                    ShotChangesHelper.SaveShotChanges(videoFileName, AudioVisualizer.ShotChanges, audioTrackNumber);
                }
            });
        }
    }

    private Lock _addShotChangeLock = new Lock();

    private void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
    {
        if (string.IsNullOrWhiteSpace(outLine.Data))
        {
            return;
        }

        var match = ShotChangesViewModel.TimeRegex.Match(outLine.Data);
        if (match.Success)
        {
            var timeCode = match.Value.Replace("pts_time:", string.Empty).Replace(",", ".").Replace("٫", ".").Replace("⠨", ".");
            if (double.TryParse(timeCode, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var seconds) && seconds > 0.2)
            {
                lock (_addShotChangeLock)
                {
                    AudioVisualizer?.ShotChanges.Add(seconds);
                }
            }
        }
    }

    private static void DeleteTempFile(string tempWaveFileName)
    {
        try
        {
            if (File.Exists(tempWaveFileName))
            {
                File.Delete(tempWaveFileName);
            }
        }
        catch
        {
            // ignore
        }
    }

    private void VideoCloseFile()
    {
        _videoOpenTokenSource?.Cancel();
        GetVideoPlayerControl()?.Close();
        _videoFileName = string.Empty;
        IsVideoLoaded = false;
        _mediaInfo = null;

        if (AudioVisualizer != null)
        {
            AudioVisualizer.WavePeaks = null;
            AudioVisualizer.ShotChanges = new List<double>();
            AudioVisualizer.StartPositionSeconds = 0;
            AudioVisualizer.CurrentVideoPositionSeconds = 0;
        }
    }

    public bool IsTyping()
    {
        if (_lastKeyPressedMs == 0)
        {
            return false;
        }

        var ms = Environment.TickCount64;
        var diff = ms - _lastKeyPressedMs;
        if (diff < 500)
        {
            return true;
        }

        return false;
    }

    public int GetFastHash()
    {
        var pre = _subtitleFileName + SelectedEncoding.DisplayName;

        unchecked
        {
            var hash = 17;
            hash = hash * 23 + (pre?.GetHashCode() ?? 0);
            hash = hash * 23 + (_subtitle.Header?.Trim().GetHashCode() ?? 0);
            hash = hash * 23 + (_subtitle.Footer?.Trim().GetHashCode() ?? 0);

            var count = Subtitles.Count;
            for (var i = 0; i < count; i++)
            {
                var p = Subtitles[i];

                hash = hash * 23 + p.Number;
                hash = hash * 23 + p.StartTime.TotalMilliseconds.GetHashCode();
                hash = hash * 23 + p.EndTime.TotalMilliseconds.GetHashCode();

                if (p.Text != null)
                {
                    hash = hash * 23 + p.Text.GetHashCode();
                }

                hash = hash * 23 + (p.Style?.GetHashCode() ?? 0);
                hash = hash * 23 + (p.Extra?.GetHashCode() ?? 0);
                hash = hash * 23 + (p.Actor?.GetHashCode() ?? 0);
                hash = hash * 23 + p.Layer;
            }

            return hash;
        }
    }

    public int GetFastHashOriginal()
    {
        _subtitleOriginal ??= new Subtitle();
        var pre = _subtitleOriginal + SelectedEncoding.DisplayName;

        unchecked
        {
            var hash = 17;
            hash = hash * 23 + (pre?.GetHashCode() ?? 0);
            hash = hash * 23 + (_subtitleOriginal.Header?.Trim().GetHashCode() ?? 0);
            hash = hash * 23 + (_subtitleOriginal.Footer?.Trim().GetHashCode() ?? 0);

            var count = Subtitles.Count;
            for (var i = 0; i < count; i++)
            {
                var p = Subtitles[i];

                hash = hash * 23 + p.Number;
                hash = hash * 23 + p.StartTime.TotalMilliseconds.GetHashCode();
                hash = hash * 23 + p.EndTime.TotalMilliseconds.GetHashCode();

                if (p.OriginalText != null)
                {
                    hash = hash * 23 + p.OriginalText.GetHashCode();
                }

                hash = hash * 23 + (p.Style?.GetHashCode() ?? 0);
                hash = hash * 23 + (p.Extra?.GetHashCode() ?? 0);
                hash = hash * 23 + (p.Actor?.GetHashCode() ?? 0);
                hash = hash * 23 + p.Layer;
            }

            return hash;
        }
    }

    private async Task DeleteSelectedItems()
    {
        var selectedItems = _selectedSubtitles?.ToList() ?? [];
        if (selectedItems.Count == 0)
        {
            return;
        }

        if (Se.Settings.General.PromptBeforeDelete)
        {
            var title = Se.Language.General.Delete;

            var message = string.Format(Se.Language.General.DeleteXLinesPrompt, selectedItems.Count);
            if (selectedItems.Count == 1)
            {
                message = string.Format(Se.Language.General.DeleteLineXPrompt, SelectedSubtitleIndex + 1);
            }

            var answer = await MessageBox.Show(
                Window!,
                title,
                message,
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (answer != MessageBoxResult.Yes)
            {
                return;
            }

            _shortcutManager.ClearKeys();
        }

        _subtitleGridSelectionChangedSkip = true;
        var idx = Subtitles.IndexOf(selectedItems.First());
        _undoRedoManager.StopChangeDetection();

        // Detach ItemsSource to prevent updates
        var itemsSource = SubtitleGrid.ItemsSource;
        var isLargeDelete = selectedItems.Count > 10;
        if (isLargeDelete)
        {
            SubtitleGrid.ItemsSource = null;
        }

        try
        {
            var selectedSet = new HashSet<SubtitleLineViewModel>(selectedItems);
            var indicesToRemove = Subtitles
                .Select((s, i) => (s, i))
                .Where(x => selectedSet.Contains(x.s))
                .Select(x => x.i)
                .OrderByDescending(i => i)
                .ToList();

            foreach (var index in indicesToRemove)
            {
                Subtitles.RemoveAt(index);
            }

            if (idx >= Subtitles.Count)
            {
                idx = Subtitles.Count - 1;
            }

            Renumber();
        }
        finally
        {
            // Reattach ItemsSource
            if (isLargeDelete)
            {
                SubtitleGrid.ItemsSource = itemsSource;
            }

            _subtitleGridSelectionChangedSkip = false;
            SelectAndScrollToRow(idx);
            _undoRedoManager.StartChangeDetection();
            SubtitleGridSelectionChanged();
            _updateAudioVisualizer = true;
        }
    }

    private async Task RippleDeleteSelectedItems()
    {
        var selectedItems = _selectedSubtitles?.ToList() ?? [];
        if (selectedItems.Count == 0)
        {
            return;
        }

        if (Se.Settings.General.PromptBeforeDelete)
        {
            var title = Se.Language.General.Delete;

            var message = string.Format(Se.Language.General.DeleteXLinesPrompt, selectedItems.Count);
            if (selectedItems.Count == 1)
            {
                message = string.Format(Se.Language.General.DeleteLineXPrompt, SelectedSubtitleIndex + 1);
            }

            var answer = await MessageBox.Show(
                Window!,
                title,
                message,
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (answer != MessageBoxResult.Yes)
            {
                return;
            }

            _shortcutManager.ClearKeys();
        }

        _subtitleGridSelectionChangedSkip = true;
        var idx = Subtitles.IndexOf(selectedItems.First());
        _undoRedoManager.StopChangeDetection();

        var sortedIndices = selectedItems
            .Select(item => Subtitles.IndexOf(item))
            .OrderBy(i => i)
            .ToList();

        var firstLine = Subtitles.GetOrNull(sortedIndices.FirstOrDefault());

        var areLinesConsecutive = sortedIndices.Count == 1 ||
                                  sortedIndices.Zip(sortedIndices.Skip(1), (a, b) => b - a).All(diff => diff == 1);

        var nextLine = Subtitles.GetOrNull(idx + selectedItems.Count);

        // Detach ItemsSource to prevent updates
        var itemsSource = SubtitleGrid.ItemsSource;
        var isLargeDelete = selectedItems.Count > 10;
        if (isLargeDelete)
        {
            SubtitleGrid.ItemsSource = null;
        }

        try
        {
            var indicesToRemove = selectedItems
                .Select(Subtitles.IndexOf)
                .Where(i => i >= 0)
                .OrderByDescending(i => i)
                .ToList();

            foreach (var index in indicesToRemove)
            {
                Subtitles.RemoveAt(index);
            }

            if (idx >= Subtitles.Count)
            {
                idx = Subtitles.Count - 1;
            }

            Renumber();

            if (areLinesConsecutive && firstLine != null && nextLine != null)
            {
                var indexOfNext = Subtitles.IndexOf(nextLine);
                var timeToShift = nextLine.StartTime - firstLine.StartTime;
                for (var i = indexOfNext; i < Subtitles.Count; i++)
                {
                    Subtitles[i].StartTime = Subtitles[i].StartTime - timeToShift;
                }

                nextLine.SetStartTimeOnly(firstLine.StartTime);
            }
        }
        finally
        {
            // Reattach ItemsSource
            if (isLargeDelete)
            {
                SubtitleGrid.ItemsSource = itemsSource;
            }

            _subtitleGridSelectionChangedSkip = false;
            SelectAndScrollToRow(idx);
            _undoRedoManager.StartChangeDetection();
            SubtitleGridSelectionChanged();
            _updateAudioVisualizer = true;
        }
    }

    private void InsertBeforeSelectedItem()
    {
        var selectedItem = SelectedSubtitle;
        if (selectedItem != null)
        {
            var index = Subtitles.IndexOf(selectedItem);
            _insertService.InsertBefore(SelectedSubtitleFormat, _subtitle, Subtitles, index, string.Empty);
            Renumber();
            SelectAndScrollToRow(index);
            _updateAudioVisualizer = true;
        }
    }

    private void InsertAfterSelectedItem()
    {
        var selectedItem = SelectedSubtitle;
        if (selectedItem != null)
        {
            var index = Subtitles.IndexOf(selectedItem);
            _insertService.InsertAfter(SelectedSubtitleFormat, _subtitle, Subtitles, index, string.Empty);
            Renumber();
            SelectAndScrollToRow(index + 1);
            _updateAudioVisualizer = true;
        }
    }

    private void Renumber()
    {
        for (var index = 0; index < Subtitles.Count; index++)
        {
            Subtitles[index].Number = index + 1;
        }

        _updateAudioVisualizer = true;
    }

    private void MergeLineBefore()
    {
        var selectedItem = SelectedSubtitle;
        if (selectedItem != null)
        {
            var index = Subtitles.IndexOf(selectedItem);
            var previous = Subtitles.GetOrNull(index - 1);

            if (previous == null)
            {
                return; // no next item to merge with
            }

            var list = new List<SubtitleLineViewModel>()
            {
                previous,
                selectedItem,
            };
            _mergeManager.MergeSelectedLines(Subtitles, list, breakMode: MergeManager.BreakMode.Normal);
            Renumber();
            SelectAndScrollToRow(index - 1);
            _updateAudioVisualizer = true;
        }
    }

    private void MergeLineBeforeKeepBreaks()
    {
        var selectedItem = SelectedSubtitle;
        if (selectedItem != null)
        {
            var index = Subtitles.IndexOf(selectedItem);
            var previous = Subtitles.GetOrNull(index - 1);

            if (previous == null)
            {
                return; // no next item to merge with
            }

            var list = new List<SubtitleLineViewModel>()
            {
                previous,
                selectedItem,
            };
            _mergeManager.MergeSelectedLines(Subtitles, list, breakMode: MergeManager.BreakMode.KeepBreaks);
            Renumber();
            SelectAndScrollToRow(index - 1);
            _updateAudioVisualizer = true;
        }
    }

    private void MergeLineAfter()
    {
        var selectedItem = SelectedSubtitle;
        if (selectedItem != null)
        {
            var index = Subtitles.IndexOf(selectedItem);
            var next = Subtitles.GetOrNull(index + 1);

            if (next == null)
            {
                return; // no next item to merge with
            }

            var list = new List<SubtitleLineViewModel>()
            {
                selectedItem,
                next
            };
            _mergeManager.MergeSelectedLines(Subtitles, list, breakMode: MergeManager.BreakMode.Normal);
            Renumber();
            SelectAndScrollToRow(index);
            _updateAudioVisualizer = true;
        }
    }

    private void MergeLineAfterKeepBreaks()
    {
        var selectedItem = SelectedSubtitle;
        if (selectedItem != null)
        {
            var index = Subtitles.IndexOf(selectedItem);
            var next = Subtitles.GetOrNull(index + 1);

            if (next == null)
            {
                return; // no next item to merge with
            }

            var list = new List<SubtitleLineViewModel>()
            {
                selectedItem,
                next
            };
            _mergeManager.MergeSelectedLines(Subtitles, list, breakMode: MergeManager.BreakMode.KeepBreaks);
            Renumber();
            SelectAndScrollToRow(index);
        }
    }

    private void MergeLinesSelected()
    {
        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
        var selectedItem = SelectedSubtitle;
        if (selectedItems.Count == 0 || selectedItem == null)
        {
            return;
        }

        var index = Subtitles.IndexOf(selectedItem);

        _mergeManager.MergeSelectedLines(Subtitles,
            SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList());

        SelectAndScrollToRow(index);
        Renumber();
    }

    private void MergeLinesSelectedAsDialog()
    {
        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
        if (selectedItems.Count != 2)
        {
            return; // only two items can be merged as dialog
        }

        var index = Subtitles.IndexOf(selectedItems[0]);
        _mergeManager.MergeSelectedLinesAsDialog(Subtitles, selectedItems);
        SelectAndScrollToRow(index);
        Renumber();
    }

    private void MergeLinesSelectedBilingual()
    {
        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
        if (selectedItems.Count < 2)
        {
            return;
        }

        var ordered = selectedItems
            .Select(item => (Item: item, Index: Subtitles.IndexOf(item)))
            .Where(p => p.Index >= 0)
            .OrderBy(p => p.Index)
            .ToList();

        if (ordered.Count != selectedItems.Count)
        {
            return;
        }

        for (var i = 1; i < ordered.Count; i++)
        {
            if (ordered[i].Index != ordered[i - 1].Index + 1)
            {
                return; // selection must be contiguous
            }
        }

        var firstIndex = ordered[0].Index;
        var first = ordered[0].Item;
        var last = ordered[ordered.Count - 1].Item;

        first.Text = MergeBilingualLines(ordered.Select(p => p.Item.Text));
        if (ordered.Any(p => !string.IsNullOrEmpty(p.Item.OriginalText)))
        {
            first.OriginalText = MergeBilingualLines(ordered.Select(p => p.Item.OriginalText));
        }

        first.EndTime = last.EndTime;

        for (var i = ordered.Count - 1; i >= 1; i--)
        {
            Subtitles.Remove(ordered[i].Item);
        }

        Renumber();
        SelectAndScrollToRow(firstIndex);
        _updateAudioVisualizer = true;
    }

    private static string MergeBilingualLines(IEnumerable<string> texts)
    {
        var split = texts.Select(t => (t ?? string.Empty).SplitToLines()).ToList();
        var maxLines = split.Count == 0 ? 0 : split.Max(lines => lines.Count);
        var resultLines = new List<string>(maxLines);
        for (var i = 0; i < maxLines; i++)
        {
            var parts = split
                .Select(lines => i < lines.Count ? lines[i].Trim() : string.Empty)
                .Where(s => s.Length > 0);
            resultLines.Add(string.Join(" ", parts));
        }

        var merged = string.Join(Environment.NewLine, resultLines);
        return HtmlUtil.FixInvalidItalicTags(merged);
    }

    private void ToggleItalic()
    {
        var selectedItems = _selectedSubtitles?.ToList() ?? [];
        if (selectedItems.Count == 0)
        {
            return;
        }

        var first = true;
        var makeItalic = true;
        var isAssa = SelectedSubtitleFormat is AdvancedSubStationAlpha;

        if (isAssa)
        {
            foreach (var item in selectedItems)
            {
                if (first)
                {
                    first = false;
                    makeItalic = !item.Text.Contains("{\\i1}") && !item.Text.Contains("\\i1");
                }

                item.Text = item.Text
                    .Replace("{\\i1}", string.Empty)
                    .Replace("{\\i0}", string.Empty)
                    .Replace("\\i1\\", "\\")
                    .Replace("\\i0\\", "\\");
                if (makeItalic)
                {
                    if (!string.IsNullOrEmpty(item.Text))
                    {
                        item.Text = $"{{\\i1}}{item.Text}";
                    }
                }
            }

            return;
        }

        foreach (var item in selectedItems)
        {
            if (first)
            {
                first = false;
                makeItalic = !item.Text.Contains("<i>");
            }

            item.Text = item.Text.Replace("<i>", string.Empty).Replace("</i>", string.Empty);
            item.Text = item.Text.Replace("<I>", string.Empty).Replace("</I>", string.Empty);
            if (makeItalic)
            {
                if (!string.IsNullOrEmpty(item.Text))
                {
                    item.Text = $"<i>{item.Text}</i>";
                }
            }
        }
    }

    private void ToggleBold()
    {
        var selectedItems = _selectedSubtitles?.ToList() ?? [];
        if (selectedItems.Count == 0)
        {
            return;
        }

        var first = true;
        var makeBold = true;
        var isAssa = SelectedSubtitleFormat is AdvancedSubStationAlpha;

        if (isAssa)
        {
            foreach (var item in selectedItems)
            {
                if (first)
                {
                    first = false;
                    makeBold = !item.Text.Contains("{\\b1}") && !item.Text.Contains("\\b1");
                }

                item.Text = item.Text
                    .Replace("{\\b1}", string.Empty)
                    .Replace("{\\b0}", string.Empty)
                    .Replace("\\b1\\", "\\")
                    .Replace("\\b0\\", "\\");
                if (makeBold)
                {
                    if (!string.IsNullOrEmpty(item.Text))
                    {
                        item.Text = $"{{\\b1}}{item.Text}";
                    }
                }
            }

            return;
        }

        foreach (var item in selectedItems)
        {
            if (first)
            {
                first = false;
                makeBold = !item.Text.Contains("<b>");
            }

            item.Text = item.Text.Replace("<b>", string.Empty).Replace("</b>", string.Empty);
            item.Text = item.Text.Replace("<B>", string.Empty).Replace("</B>", string.Empty);
            if (makeBold)
            {
                if (!string.IsNullOrEmpty(item.Text))
                {
                    item.Text = $"<b>{item.Text}</b>";
                }
            }
        }
    }

    private void SetAlignmentToSelected(string alignment)
    {
        var selectedItems = _selectedSubtitles?.ToList() ?? [];
        if (selectedItems.Count == 0)
        {
            return;
        }

        foreach (var item in selectedItems)
        {
            item.Text = item.Text
                .Replace("{\\an1}", string.Empty)
                .Replace("{\\an2}", string.Empty)
                .Replace("{\\an3}", string.Empty)
                .Replace("{\\an4}", string.Empty)
                .Replace("{\\an5}", string.Empty)
                .Replace("{\\an6}", string.Empty)
                .Replace("{\\an7}", string.Empty)
                .Replace("{\\an8}", string.Empty)
                .Replace("{\\an9}", string.Empty)
                .Replace("\\an1", string.Empty)
                .Replace("\\an2", string.Empty)
                .Replace("\\an3", string.Empty)
                .Replace("\\an4", string.Empty)
                .Replace("\\an5", string.Empty)
                .Replace("\\an6", string.Empty)
                .Replace("\\an7", string.Empty)
                .Replace("\\an8", string.Empty)
                .Replace("\\an9", string.Empty);

            if (alignment == "an2" && Se.Settings.General.WriteAn2Tag == false)
            {
                continue;
            }

            if (!string.IsNullOrEmpty(item.Text))
            {
                if (item.Text.StartsWith("{\\"))
                {
                    item.Text = item.Text.Insert(2, alignment + "\\");
                }
                else
                {
                    item.Text = $"{{\\{alignment}}}{item.Text}";
                }
            }
        }
    }

    public void SubtitleContextOpening(object? sender, EventArgs e)
    {
        var idx = SubtitleGrid.SelectedIndex;
        var count = Subtitles.Count;
        MenuItemMergeAsDialog.IsVisible = SubtitleGrid.SelectedItems.Count == 2;
        MenuItemMerge.IsVisible = SubtitleGrid.SelectedItems.Count > 1;
        MenuItemExtendToLineBefore.IsVisible = SubtitleGrid.SelectedItems.Count == 1 && Subtitles.Count > 1 && idx > 0;
        MenuItemExtendToLineAfter.IsVisible = SubtitleGrid.SelectedItems.Count == 1 && Subtitles.Count > 1 && idx < count - 1;
        AreAssaContentMenuItemsVisible = false;
        ShowAutoTranslateSelectedLines = SubtitleGrid.SelectedItems.Count > 0 && ShowColumnOriginalText;
        HasMultipleLinesSelected = SubtitleGrid.SelectedItems.Count > 1;
        ShowColumnLayerFlyoutMenuItem = IsFormatAssa;

        if (IsSubtitleGridFlyoutHeaderVisible)
        {
            IsSubtitleGridDataMenuVisible = false;
            IsMergeWithNextOrPreviousVisible = false;
            IsInsertLineNoSelectionVisible = false;
            MenuItemExtendToLineBefore.IsVisible = false;
            MenuItemExtendToLineAfter.IsVisible = false;
        }
        else if (Subtitles.Count == 0)
        {
            IsSubtitleGridDataMenuVisible = false;
            IsMergeWithNextOrPreviousVisible = false;
            IsInsertLineNoSelectionVisible = true;
        }
        else
        {
            IsSubtitleGridDataMenuVisible = true;
            IsMergeWithNextOrPreviousVisible = SubtitleGrid.SelectedItems.Count == 1;
            IsInsertLineNoSelectionVisible = false;

            if (IsFormatAssa || IsFormatSsa)
            {
                AreAssaContentMenuItemsVisible = true;

                MenuItemStyles.Items.Clear();
                var styles = AdvancedSubStationAlpha.GetSsaStylesFromHeader(_subtitle.Header);
                var stylesToAdd = styles.Select(p => p.Name).Where(p => !string.IsNullOrEmpty(p)).DistinctBy(p => p)
                    .OrderBy(p => p);
                foreach (var style in stylesToAdd)
                {
                    MenuItemStyles.Items.Add(new MenuItem
                    {
                        Header = style,
                        Command = SetStyleForSelectedLinesCommand,
                        CommandParameter = style,
                    });
                }

                if (stylesToAdd.Any())
                {
                    MenuItemStyles.Items.Add(new Separator());
                }

                MenuItemStyles.Items.Add(new MenuItem
                {
                    Header = Se.Language.General.NewDotDotDot,
                    Command = SetNewStyleForSelectedLinesCommand,
                });

                MenuItemActors.Items.Clear();
                foreach (var actor in Subtitles.Select(p => p.Actor).Where(p => !string.IsNullOrEmpty(p))
                             .DistinctBy(p => p).OrderBy(p => p))
                {
                    MenuItemActors.Items.Add(new MenuItem
                    {
                        Header = actor,
                        Command = SetActorForSelectedLinesCommand,
                        CommandParameter = actor,
                    });
                }

                if (MenuItemActors.Items.Count > 0)
                {
                    MenuItemActors.Items.Add(new Separator());
                }

                MenuItemActors.Items.Add(new MenuItem
                {
                    Header = Se.Language.General.NewDotDotDot,
                    Command = SetNewActorForSelectedLinesCommand,
                });
            }
        }
    }

    private bool IsTextInputFocused()
    {
        var focusedElement = Window?.FocusManager?.GetFocusedElement();

        return focusedElement is TextEditor ||
               focusedElement is TextBox ||
               focusedElement is MaskedTextBox ||
               focusedElement is AutoCompleteBox ||
               (focusedElement is Control control && IsTextInputControl(control));
    }

    private static bool IsTextInputControl(Control control)
    {
        var typeName = control.GetType().Name;
        return typeName.Contains("TextEditor") ||
               typeName.Contains("TextBox") ||
               typeName.Contains("MaskedTextBox") ||
               typeName.Contains("TextArea") ||
               typeName.Contains("TextInput");
    }

    private bool IsSubtitleGridFocused()
    {
        var focusedElement = Window?.FocusManager?.GetFocusedElement();
        if (focusedElement == null)
        {
            return false;
        }

        if (focusedElement == SubtitleGrid)
        {
            return true;
        }

        // Check if the focused element is a child of SubtitleGrid (e.g., DataGridRow, DataGridCell)
        if (focusedElement is Avalonia.Visual visual)
        {
            var parent = visual.GetVisualParent();
            while (parent != null)
            {
                if (parent == SubtitleGrid)
                {
                    return true;
                }

                parent = parent.GetVisualParent();
            }
        }

        return false;
    }

    private bool TryHandleMacOptionBackspace(KeyEventArgs keyEventArgs)
    {
        return TryHandleMacTextDelete(
            keyEventArgs,
            static keyEvent => keyEvent.Key == Key.Back && keyEvent.KeyModifiers == KeyModifiers.Alt,
            DeletePreviousWord);
    }

    private bool TryHandleMacCommandDelete(KeyEventArgs keyEventArgs)
    {
        return TryHandleMacTextDelete(
            keyEventArgs,
            static keyEvent =>
                (keyEvent.Key == Key.Back || keyEvent.Key == Key.Delete) &&
                keyEvent.KeyModifiers == KeyModifiers.Meta,
            DeleteToLineStart);
    }

    private bool TryHandleMacTextDelete(KeyEventArgs keyEventArgs, Func<KeyEventArgs, bool> isKeyMatch, Func<ITextBoxWrapper, bool> deleteAction)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || !isKeyMatch(keyEventArgs))
        {
            return false;
        }

        var textBox = GetFocusedTextBoxWrapper();
        if (textBox == null)
        {
            return false;
        }

        if (!deleteAction(textBox))
        {
            return false;
        }

        keyEventArgs.Handled = true;
        _shortcutManager.ClearKeys();
        return true;
    }

    private static bool DeletePreviousWord(ITextBoxWrapper textBox)
    {
        return DeleteFromCaretToBoundary(textBox, FindPreviousWordBoundary);
    }

    private static bool DeleteToLineStart(ITextBoxWrapper textBox)
    {
        return DeleteFromCaretToBoundary(textBox, static (text, caret) =>
        {
            var previousLineBreak = text.LastIndexOf('\n', Math.Max(0, caret - 1));
            return previousLineBreak < 0 ? 0 : previousLineBreak + 1;
        });
    }

    private static bool DeleteFromCaretToBoundary(ITextBoxWrapper textBox, Func<string, int, int> findBoundary)
    {
        var text = textBox.Text ?? string.Empty;

        if (textBox.SelectionLength > 0)
        {
            textBox.SelectedText = string.Empty;
            return true;
        }

        var caret = textBox.CaretIndex;
        if (caret <= 0 || caret > text.Length)
        {
            return false;
        }

        var start = findBoundary(text, caret);
        if (start == caret)
        {
            return false;
        }

        textBox.Select(start, caret - start);
        textBox.SelectedText = string.Empty;
        textBox.CaretIndex = start;
        return true;
    }

    private static int FindPreviousWordBoundary(string text, int caret)
    {
        var index = caret;

        while (index > 0 && char.IsWhiteSpace(text[index - 1]))
        {
            index--;
        }

        if (index == 0)
        {
            return 0;
        }

        var deleteAlphaNumeric = IsAlphaNumericWordChar(text[index - 1]);
        while (index > 0 &&
               !char.IsWhiteSpace(text[index - 1]) &&
               IsAlphaNumericWordChar(text[index - 1]) == deleteAlphaNumeric)
        {
            index--;
        }

        return index;
    }

    private static bool IsAlphaNumericWordChar(char c)
    {
        return char.IsLetterOrDigit(c) || c == '_';
    }

    private readonly Lock _onKeyDownHandlerLock = new();

    private static readonly HashSet<Key> AlwaysAllowedSingleKeyShortcuts = new HashSet<Key>
    {
        Key.Escape,
        Key.Tab,
        Key.PageUp,
        Key.PageDown,
        Key.BrowserBack,
        Key.BrowserForward,
        Key.BrowserFavorites,
        Key.BrowserHome,
        Key.Execute,
        Key.ExSel,
        Key.LaunchApplication1,
        Key.LaunchApplication2,
        Key.LaunchMail,
        Key.Insert,
        Key.F1,
        Key.F2,
        Key.F3,
        Key.F4,
        Key.F5,
        Key.F6,
        Key.F7,
        Key.F8,
        Key.F9,
        Key.F10,
        Key.F11,
        Key.F12,
        Key.F13,
        Key.F14,
        Key.F15,
        Key.F16,
        Key.F17,
        Key.F18,
        Key.F19,
        Key.F20,
        Key.F21,
        Key.F22,
        Key.F23,
        Key.F24,
    };

    internal void OnKeyDownHandler(object? sender, KeyEventArgs keyEventArgs)
    {
        lock (_onKeyDownHandlerLock)
        {
            AudioVisualizer?.SetKeyModifiers(keyEventArgs);
            var ms = Environment.TickCount64;
            var msDiff = ms - _lastKeyPressedMs;
            var k = keyEventArgs.Key;
            if (msDiff > 5000)
            {
                _shortcutManager.ClearKeys(); // reset shortcuts if no key pressed for 5 seconds
            }

            _lastKeyPressedMs = ms;

            if (UiUtil.TryHandleWindowSystemMenu(keyEventArgs, Window))
            {
                return;
            }

            // This allows command+delete on mac to delete to line start.
            if (TryHandleMacCommandDelete(keyEventArgs))
            {
                return;
            }

            // This allows option backspace on mac to delete the previous word
            if (TryHandleMacOptionBackspace(keyEventArgs))
            {
                return;
            }

            _shortcutManager.OnKeyPressed(this, keyEventArgs);
            if (_shortcutManager.GetActiveKeys().Count == 0)
            {
                return;
            }

            if (IsTextInputFocused())
            {
                var currentKeys = _shortcutManager.GetActiveKeys();
                if (currentKeys.Count == 1)
                {
                    var key = currentKeys.First();

                    if (EditTextBox.IsFocused && key == Key.Return &&
                        keyEventArgs.KeyModifiers == KeyModifiers.None &&
                        Se.Settings.General.SubtitleTextBoxLimitNewLines)
                    {
                        var newLineCount = EditTextBox.Text.SplitToLines().Count;
                        if (newLineCount >= Se.Settings.General.MaxNumberOfLines)
                        {
                            keyEventArgs.Handled = true;
                            return;
                        }
                    }

                    if (!Se.Settings.Tools.AllowSingleLetterShortcutsInTextbox && !AlwaysAllowedSingleKeyShortcuts.Contains(key) &&
                        (keyEventArgs.KeyModifiers == KeyModifiers.None || keyEventArgs.KeyModifiers == KeyModifiers.Shift))
                    {
                        return; // allow single key shortcuts in text input if enabled in settings, or if it's not an allowed single key shortcut
                    }
                }
            }

            if (IsSubtitleGridFocused())
            {
                if (keyEventArgs.Key == Key.Down && keyEventArgs.KeyModifiers == KeyModifiers.Shift && Subtitles.Count > 0)
                {
                    keyEventArgs.Handled = true;
                    HandleShiftArrowSelection(1);
                    return;
                }
                else if (keyEventArgs.Key == Key.Up && keyEventArgs.KeyModifiers == KeyModifiers.Shift && Subtitles.Count > 0)
                {
                    keyEventArgs.Handled = true;
                    HandleShiftArrowSelection(-1);
                    return;
                }
                else if ((keyEventArgs.Key == Key.Down || keyEventArgs.Key == Key.Up) && keyEventArgs.KeyModifiers == KeyModifiers.None)
                {
                    _shiftSelectAnchorIndex = -1;
                    _shiftSelectCurrentIndex = -1;
                }
                else if (keyEventArgs.Key == Key.PageDown && keyEventArgs.KeyModifiers == KeyModifiers.Shift && Subtitles.Count > 0)
                {
                    keyEventArgs.Handled = true;
                    HandleShiftArrowSelection(GetSubtitleGridPageSize());
                    return;
                }
                else if (keyEventArgs.Key == Key.PageUp && keyEventArgs.KeyModifiers == KeyModifiers.Shift && Subtitles.Count > 0)
                {
                    keyEventArgs.Handled = true;
                    HandleShiftArrowSelection(-GetSubtitleGridPageSize());
                    return;
                }
                else if (keyEventArgs.Key == Key.Home && keyEventArgs.KeyModifiers == KeyModifiers.Shift && Subtitles.Count > 0)
                {
                    keyEventArgs.Handled = true;
                    HandleShiftArrowSelection(-Subtitles.Count); // clamps to 0
                    return;
                }
                else if (keyEventArgs.Key == Key.End && keyEventArgs.KeyModifiers == KeyModifiers.Shift && Subtitles.Count > 0)
                {
                    keyEventArgs.Handled = true;
                    HandleShiftArrowSelection(Subtitles.Count); // clamps to Count - 1
                    return;
                }
                else if (keyEventArgs.Key == Key.Home && keyEventArgs.KeyModifiers == KeyModifiers.None && Subtitles.Count > 0)
                {
                    keyEventArgs.Handled = true;
                    SelectAndScrollToRow(0);
                    return;
                }
                else if (keyEventArgs.Key == Key.End && keyEventArgs.KeyModifiers == KeyModifiers.None && Subtitles.Count > 0)
                {
                    keyEventArgs.Handled = true;
                    SelectAndScrollToRow(Subtitles.Count - 1);
                    return;
                }
                else if (keyEventArgs.Key == Key.Enter && keyEventArgs.KeyModifiers == KeyModifiers.None)
                {
                    if (Se.Settings.General.SubtitleEnterKeyAction == SubtitleEnterKeyActionType.GoToSubtitleAndSetVideoPosition.ToString())
                    {
                        keyEventArgs.Handled = true;
                        var idx = SelectedSubtitleIndex;
                        var item = SelectedSubtitle;
                        var vp = GetVideoPlayerControl();
                        if (idx.HasValue && idx >= 0 && item != null && vp != null)
                        {
                            vp.Position = item.StartTime.TotalSeconds;
                            SelectAndScrollToRow(idx.Value);
                            if (AudioVisualizer != null &&
                                (item.StartTime.TotalSeconds < AudioVisualizer.StartPositionSeconds ||
                                 item.StartTime.TotalSeconds + 0.2 > AudioVisualizer.EndPositionSeconds))
                            {
                                AudioVisualizer.CenterOnPosition(item);
                            }

                            _updateAudioVisualizer = true;
                        }
                    }

                    return;
                }

                var relayCommand = _shortcutManager.CheckShortcuts(keyEventArgs, ShortcutCategory.SubtitleGrid.ToString());
                if (relayCommand == null)
                {
                    relayCommand = _shortcutManager.CheckShortcuts(keyEventArgs, ShortcutCategory.SubtitleGridAndTextBox.ToString());
                }

                if (relayCommand != null)
                {
                    keyEventArgs.Handled = true;
                    relayCommand.Execute(null);
                    return;
                }
            }

            if (AudioVisualizer != null && AudioVisualizer.IsFocused)
            {
                var relayCommand = _shortcutManager.CheckShortcuts(keyEventArgs, ShortcutCategory.Waveform.ToString());
                if (relayCommand != null)
                {
                    keyEventArgs.Handled = true;
                    relayCommand.Execute(null);
                    return;
                }
            }

            if (EditTextBox.IsFocused)
            {
                var relayCommand = _shortcutManager.CheckShortcuts(keyEventArgs, ShortcutCategory.SubtitleGridAndTextBox.ToString());
                if (relayCommand != null)
                {
                    keyEventArgs.Handled = true;
                    relayCommand.Execute(null);
                    return;
                }
            }

            var keys = _shortcutManager.GetActiveKeys().Select(p => p.ToString()).ToList();
            var hashCode = ShortCut.CalculateHash(keys, ShortcutCategory.General.ToString());

            var rc = _shortcutManager.CheckShortcuts(keyEventArgs, ShortcutCategory.General.ToString().ToLowerInvariant());
            if (rc != null)
            {
                keyEventArgs.Handled = true;
                rc.Execute(null);
            }
        }
    }

    public void OnKeyUpHandler(object? sender, KeyEventArgs e)
    {
        if (_setEndAtKeyUpLine != null)
        {
            _setEndAtKeyUpLine = null;
            _setEndAtKeyUpLineGoToNext = false;
        }

        _shortcutManager.OnKeyReleased(this, e);
        AudioVisualizer?.SetKeyModifiers(e);
    }

    private bool _subtitleGridIsRightClick = false;
    private bool _subtitleGridIsLeftClick = false;
    private bool _subtitleGridIsControlPressed = false;
    private int _dragSelectStartIndex = -1;
    private int _dragSelectLastIndex = -1;
    private int _shiftSelectAnchorIndex = -1;
    private int _shiftSelectCurrentIndex = -1;
    private int _dragSelectAutoScrollDirection;
    private int _dragSelectAutoScrollStep = 1;
    private bool _dragSelectHasMoved;
    private DispatcherTimer? _dragSelectAutoScrollTimer;
    private const double DragSelectAutoScrollEdgeSize = 28;
    private const double DragSelectAutoScrollAccelerationPixels = 18;
    private const int DragSelectAutoScrollMaxStep = 16;

    public void SubtitleGrid_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        StopSubtitleGridDragSelectAutoScroll();
        _subtitleGridIsControlPressed = false;
        _subtitleGridIsLeftClick = false;
        _subtitleGridIsRightClick = false;
        _dragSelectStartIndex = -1;
        _dragSelectLastIndex = -1;
        _dragSelectHasMoved = false;
        _shiftSelectAnchorIndex = -1;
        _shiftSelectCurrentIndex = -1;
        IsSubtitleGridFlyoutHeaderVisible = false;

        if (sender is Control { ContextFlyout: not null } control)
        {
            var props = e.GetCurrentPoint(control).Properties;
            _subtitleGridIsLeftClick = props.IsLeftButtonPressed;
            _subtitleGridIsRightClick = props.IsRightButtonPressed;
            _subtitleGridIsControlPressed = e.KeyModifiers.HasFlag(KeyModifiers.Control);

            var hitTest = SubtitleGrid.InputHitTest(e.GetPosition(SubtitleGrid));
            var current = hitTest as Control;
            while (current != null)
            {
                if (current is DataGridColumnHeader)
                {
                    IsSubtitleGridFlyoutHeaderVisible = true;
                    IsMergeWithNextOrPreviousVisible = false;
                    return;
                }

                current = current.Parent as Control;
            }

            if (_subtitleGridIsLeftClick && !_subtitleGridIsControlPressed)
            {
                var rowIndex = GetDataGridRowIndexFromPoint(e.GetPosition(SubtitleGrid));
                if (rowIndex >= 0)
                {
                    _dragSelectStartIndex = rowIndex;
                    _dragSelectLastIndex = rowIndex;
                }
            }
        }
    }

    public void SubtitleGridDropHost_DoubleTapped(object? sender, TappedEventArgs e)
    {
        var rowIndex = GetDataGridRowIndexFromPoint(e.GetPosition(SubtitleGrid));
        if (rowIndex < 0 || rowIndex >= Subtitles.Count)
        {
            return;
        }

        StopSubtitleGridDragSelectAutoScroll();
        _dragSelectStartIndex = -1;
        _dragSelectLastIndex = -1;
        _dragSelectHasMoved = false;

        SubtitleGrid.SelectedItem = Subtitles[rowIndex];
        OnSubtitleGridDoubleTapped(SubtitleGrid, e);
        e.Handled = true;
    }

    private int GetDataGridRowIndexFromPoint(Avalonia.Point position)
    {
        var hitTest = SubtitleGrid.InputHitTest(position);
        var current = hitTest as Control;
        while (current != null)
        {
            if (current is DataGridRow row)
            {
                return row.Index;
            }

            current = current.Parent as Control;
        }

        return -1;
    }

    public void SubtitleGrid_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_dragSelectStartIndex < 0 || !_subtitleGridIsLeftClick)
        {
            return;
        }

        var props = e.GetCurrentPoint(SubtitleGrid).Properties;
        if (!props.IsLeftButtonPressed)
        {
            EndSubtitleGridDragSelect(e);
            return;
        }

        var position = e.GetPosition(SubtitleGrid);
        UpdateSubtitleGridDragSelectAutoScroll(position);

        var currentIndex = GetDataGridRowIndexFromPoint(position);
        if (currentIndex < 0)
        {
            return;
        }

        var wasDragging = _dragSelectHasMoved;
        DragSelectSubtitleGridToIndex(currentIndex);
        if (_dragSelectHasMoved)
        {
            if (!wasDragging && sender is Control control)
            {
                e.Pointer.Capture(control);
            }

            e.Handled = true;
        }
    }

    private void DragSelectSubtitleGridToIndex(int currentIndex)
    {
        if (_dragSelectStartIndex < 0 || currentIndex < 0 || currentIndex >= Subtitles.Count)
        {
            return;
        }

        _dragSelectLastIndex = currentIndex;

        if (currentIndex == _dragSelectStartIndex && !_dragSelectHasMoved)
        {
            return;
        }

        _dragSelectHasMoved = true;

        var startIdx = Math.Min(_dragSelectStartIndex, currentIndex);
        var endIdx = Math.Max(_dragSelectStartIndex, currentIndex);

        _subtitleGridSelectionChangedSkip = true;
        SubtitleGrid.SelectedItems.Clear();
        for (var i = startIdx; i <= endIdx; i++)
        {
            if (i < Subtitles.Count)
            {
                SubtitleGrid.SelectedItems.Add(Subtitles[i]);
            }
        }

        _subtitleGridSelectionChangedSkip = false;

        SubtitleGridSelectionChanged();
    }

    private void HandleShiftArrowSelection(int direction)
    {
        if (Subtitles.Count == 0)
        {
            return;
        }

        if (_shiftSelectAnchorIndex < 0)
        {
            var anchor = SelectedSubtitleIndex ?? (SubtitleGrid.SelectedItems.Count > 0
                ? Subtitles.IndexOf((SubtitleLineViewModel)SubtitleGrid.SelectedItems[0]!)
                : -1);
            if (anchor < 0)
            {
                return;
            }

            _shiftSelectAnchorIndex = anchor;
            _shiftSelectCurrentIndex = anchor;
        }

        var newCurrent = Math.Clamp(_shiftSelectCurrentIndex + direction, 0, Subtitles.Count - 1);
        if (newCurrent == _shiftSelectCurrentIndex)
        {
            return;
        }

        _shiftSelectCurrentIndex = newCurrent;

        var startIdx = Math.Min(_shiftSelectAnchorIndex, _shiftSelectCurrentIndex);
        var endIdx = Math.Max(_shiftSelectAnchorIndex, _shiftSelectCurrentIndex);

        _subtitleGridSelectionChangedSkip = true;
        SubtitleGrid.SelectedItems.Clear();
        for (var i = startIdx; i <= endIdx; i++)
        {
            SubtitleGrid.SelectedItems.Add(Subtitles[i]);
        }

        _subtitleGridSelectionChangedSkip = false;

        SubtitleGrid.ScrollIntoView(Subtitles[_shiftSelectCurrentIndex], null);
        SubtitleGridSelectionChanged();
    }

    private int GetSubtitleGridPageSize()
    {
        var rowsPresenter = SubtitleGrid.GetVisualDescendants()
            .OfType<DataGridRowsPresenter>()
            .FirstOrDefault();
        if (rowsPresenter != null && rowsPresenter.Bounds.Height > 0)
        {
            var rowHeight = SubtitleGrid.RowHeight;
            if (!double.IsNaN(rowHeight) && rowHeight > 0)
            {
                return Math.Max(1, (int)Math.Ceiling(rowsPresenter.Bounds.Height / rowHeight) - 1);
            }
        }

        // Fallback for variable row heights: count rendered rows in the visual tree
        var visibleRowCount = SubtitleGrid.GetVisualDescendants()
            .OfType<DataGridRow>()
            .Count(r => r.IsVisible && r.Bounds.Height > 0);
        return Math.Max(1, visibleRowCount - 1);
    }

    private void UpdateSubtitleGridDragSelectAutoScroll(Avalonia.Point position)
    {
        if (SubtitleGrid.Bounds.Height <= 0)
        {
            StopSubtitleGridDragSelectAutoScroll();
            return;
        }

        if (position.Y < DragSelectAutoScrollEdgeSize)
        {
            var distanceFromEdge = DragSelectAutoScrollEdgeSize - position.Y;
            StartSubtitleGridDragSelectAutoScroll(-1, distanceFromEdge);
        }
        else if (position.Y > SubtitleGrid.Bounds.Height - DragSelectAutoScrollEdgeSize)
        {
            var distanceFromEdge = position.Y - (SubtitleGrid.Bounds.Height - DragSelectAutoScrollEdgeSize);
            StartSubtitleGridDragSelectAutoScroll(1, distanceFromEdge);
        }
        else
        {
            StopSubtitleGridDragSelectAutoScroll();
        }
    }

    private void StartSubtitleGridDragSelectAutoScroll(int direction, double distanceFromEdge)
    {
        if (_dragSelectLastIndex < 0 || Subtitles.Count == 0)
        {
            return;
        }

        _dragSelectAutoScrollDirection = direction;
        _dragSelectAutoScrollStep = CalculateSubtitleGridDragSelectAutoScrollStep(distanceFromEdge);

        if (_dragSelectAutoScrollTimer != null)
        {
            if (!_dragSelectAutoScrollTimer.IsEnabled)
            {
                _dragSelectAutoScrollTimer.Start();
            }

            return;
        }

        _dragSelectAutoScrollTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(80),
        };
        _dragSelectAutoScrollTimer.Tick += (_, _) => SubtitleGridDragSelectAutoScrollTick();
        _dragSelectAutoScrollTimer.Start();
    }

    private static int CalculateSubtitleGridDragSelectAutoScrollStep(double distanceFromEdge)
    {
        var step = 1 + (int)Math.Floor(Math.Max(0, distanceFromEdge) / DragSelectAutoScrollAccelerationPixels);
        return Math.Clamp(step, 1, DragSelectAutoScrollMaxStep);
    }

    private void StopSubtitleGridDragSelectAutoScroll()
    {
        _dragSelectAutoScrollDirection = 0;
        _dragSelectAutoScrollStep = 1;
        _dragSelectAutoScrollTimer?.Stop();
    }

    private void SubtitleGridDragSelectAutoScrollTick()
    {
        if (_dragSelectStartIndex < 0 || !_subtitleGridIsLeftClick || _dragSelectAutoScrollDirection == 0)
        {
            StopSubtitleGridDragSelectAutoScroll();
            return;
        }

        var nextIndex = Math.Clamp(
            _dragSelectLastIndex + _dragSelectAutoScrollDirection * _dragSelectAutoScrollStep,
            0,
            Subtitles.Count - 1);
        if (nextIndex == _dragSelectLastIndex)
        {
            StopSubtitleGridDragSelectAutoScroll();
            return;
        }

        DragSelectSubtitleGridToIndex(nextIndex);
        SubtitleGrid.ScrollIntoView(Subtitles[nextIndex], null);
    }

    public void SubtitleGrid_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        EndSubtitleGridDragSelect(e);

        if (sender is Control { ContextFlyout: MenuFlyout menuFlyout } control)
        {
            if (_subtitleGridIsRightClick && !_dragSelectHasMoved)
            {
                menuFlyout.ShowAt(control, true);
            }

            if (OperatingSystem.IsMacOS())
            {
                if (_subtitleGridIsLeftClick && _subtitleGridIsControlPressed && !_dragSelectHasMoved)
                {
                    menuFlyout.ShowAt(control, true);
                    e.Handled = true;
                }
            }
        }
    }

    private void EndSubtitleGridDragSelect(PointerEventArgs e)
    {
        StopSubtitleGridDragSelectAutoScroll();
        e.Pointer.Capture(null);
        _dragSelectStartIndex = -1;
        _dragSelectLastIndex = -1;
        _dragSelectAutoScrollDirection = 0;
    }

    public void SubtitleGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_subtitleGridSelectionChangedSkip)
        {
            return;
        }

        // Any user-driven selection change that isn't our own shift-arrow manipulation
        // (HandleShiftArrowSelection sets _subtitleGridSelectionChangedSkip and short-circuits
        // above) obsoletes the shift-select anchor. Plain Up/Down already clear it in the
        // key handler, but PageUp/PageDown, mouse clicks, Home/End, programmatic jumps,
        // etc. all land here and must reset the anchor so the next Shift+Down starts from
        // the current row instead of resuming an old range.
        _shiftSelectAnchorIndex = -1;
        _shiftSelectCurrentIndex = -1;

        var selectedItems = SubtitleGrid.SelectedItems;

        // If user is trying to deselect the last selected item
        if (selectedItems.Count == 0 && e.AddedItems.Count == 0 && e.RemovedItems.Count == 1)
        {
            if (Subtitles.Count > 0)
            {
                // Re-select the item that was just removed
                _subtitleGridSelectionChangedSkip = true;
                SubtitleGrid.SelectedItem = e.RemovedItems[0];
                _subtitleGridSelectionChangedSkip = false;
                e.Handled = true;
                return;
            }
        }

        if (Se.Settings.General.AutoTrimWhiteSpace && e.RemovedItems.Count < 10)
        {
            foreach (SubtitleLineViewModel? item in e.RemovedItems)
            {
                if (item != null)
                {
                    item.Text = Utilities.RemoveUnneededSpaces(item.Text, string.Empty).Trim();
                }
            }
        }

        SubtitleGridSelectionChanged();
    }

    private void SubtitleGridSelectionChanged()
    {
        var selectedItems = SubtitleGrid.SelectedItems;
        EditTextBox.ClearSelection();
        EditTextBoxOriginal.ClearSelection();
        ResetPlaySelection();

        if (selectedItems == null || selectedItems.Count == 0)
        {
            SelectedSubtitle = null;
            SelectedSubtitleIndex = null;
            _selectedSubtitles = null;
            StatusTextRight = string.Empty;
            EditTextCharactersPerSecond = string.Empty;
            EditTextCharactersPerSecondBackground = Brushes.Transparent;
            EditTextTotalLength = string.Empty;
            EditTextTotalLengthBackground = Brushes.Transparent;
            PanelSingleLineLengths.Children.Clear();

            EditTextCharactersPerSecondOriginal = string.Empty;
            EditTextCharactersPerSecondBackgroundOriginal = Brushes.Transparent;
            EditTextTotalLengthOriginal = string.Empty;
            EditTextTotalLengthBackgroundOriginal = Brushes.Transparent;
            PanelSingleLineLengthsOriginal.Children.Clear();
            return;
        }

        _selectedSubtitles = selectedItems.Cast<SubtitleLineViewModel>().ToList();
        if (selectedItems.Count > 1)
        {
            StatusTextRight = string.Format(Se.Language.Main.XLinesSelectedOfY, selectedItems.Count, Subtitles.Count);
            EditTextCharactersPerSecond = string.Empty;
            EditTextCharactersPerSecondBackground = Brushes.Transparent;
            EditTextTotalLengthBackground = Brushes.Transparent;
            EditTextTotalLength = string.Empty;
            PanelSingleLineLengths.Children.Clear();

            EditTextCharactersPerSecondOriginal = string.Empty;
            EditTextCharactersPerSecondBackgroundOriginal = Brushes.Transparent;
            EditTextTotalLengthOriginal = string.Empty;
            EditTextTotalLengthBackgroundOriginal = Brushes.Transparent;
            EditTextTotalLengthOriginal = string.Empty;
            PanelSingleLineLengthsOriginal.Children.Clear();

            return;
        }

        var item = _selectedSubtitles.FirstOrDefault();
        if (item == null)
        {
            SelectedSubtitle = null;
            SelectedSubtitleIndex = null;
            StatusTextRight = string.Empty;
            return;
        }

        var idx = Subtitles.IndexOf(item);
        StatusTextRight = $"{idx + 1}/{Subtitles.Count}";
        if (item == SelectedSubtitle && item.Text == EditText)
        {
            return;
        }

        try
        {
            _subtitleGridSelectionChangedSkip = true;
            SelectedSubtitle = item;
            SelectedSubtitleIndex = idx;
        }
        finally
        {
            _subtitleGridSelectionChangedSkip = false;
        }

        MakeSubtitleTextInfo(item.Text, item);
        MakeSubtitleTextInfoOriginal(item.OriginalText, item);
        _updateAudioVisualizer = true;
    }

    private void MakeSubtitleTextInfo(string text, SubtitleLineViewModel item)
    {
        if (_subtitleGridSelectionChangedSkip)
        {
            return;
        }

        text ??= string.Empty;

        // Cache settings and frequently accessed values
        var colorTextTooLong = Se.Settings.General.ColorTextTooLong;
        var maxLineLength = Se.Settings.General.SubtitleLineMaximumLength;
        var maxCps = Se.Settings.General.SubtitleMaximumCharactersPerSeconds;

        // Remove HTML tags once
        var cleanText = HtmlUtil.RemoveHtmlTags(text, true);
        var totalLength = (double)cleanText.CountCharacters(false);

        // Calculate CPS once using cached values
        var duration = item.EndTime.TotalMilliseconds - item.StartTime.TotalMilliseconds;
        var cps = duration > 0.001 ? totalLength / (duration / 1000.0) : 0.0;

        // Split lines once and cache count
        var lines = cleanText.SplitToLines();
        var lineCount = lines.Count;

        // Pre-allocate panel children capacity: label + (lines + separators)
        var childrenCapacity = 1 + lineCount + Math.Max(0, lineCount - 1);
        var children = new List<Control>(childrenCapacity);

        // Add header label
        children.Add(UiUtil.MakeTextBlock(Se.Language.Main.SingleLineLength)
            .WithFontSize(12)
            .WithPadding(2));

        // Process lines with minimal allocations
        for (var i = 0; i < lineCount; i++)
        {
            if (i > 0)
            {
                children.Add(UiUtil.MakeTextBlock("/")
                    .WithFontSize(12)
                    .WithPadding(2));
            }

            var lineLength = lines[i].Length;
            var tb = UiUtil.MakeTextBlock(lineLength.ToString(CultureInfo.InvariantCulture))
                .WithFontSize(12)
                .WithPadding(2);

            if (colorTextTooLong && lineLength > maxLineLength)
            {
                tb.Background = _errorBrush;
            }

            children.Add(tb);
        }

        // Batch update to minimize layout recalculations
        PanelSingleLineLengths.Children.Clear();
        PanelSingleLineLengths.Children.AddRange(children);

        // Update CPS display with formatted string
        EditTextCharactersPerSecond = string.Format(Se.Language.Main.CharactersPerSecond, $"{cps:0.0}");
        EditTextTotalLength = string.Format(Se.Language.Main.TotalCharacters, totalLength);

        // Use cached brush instances
        EditTextCharactersPerSecondBackground = colorTextTooLong && cps > maxCps
            ? _errorBrush
            : _transparentBrush;

        EditTextTotalLengthBackground = colorTextTooLong && totalLength > maxLineLength * lineCount
            ? _errorBrush
            : _transparentBrush;
    }

    private void MakeSubtitleTextInfoOriginal(string text, SubtitleLineViewModel item)
    {
        text ??= string.Empty;

        text = HtmlUtil.RemoveHtmlTags(text, true);
        var totalLength = text.CountCharacters(false);
        var cps = new Paragraph(text, item.StartTime.TotalMilliseconds, item.EndTime.TotalMilliseconds)
            .GetCharactersPerSecond();

        var lines = text.SplitToLines();
        var lineLenghts = new List<string>(lines);
        PanelSingleLineLengthsOriginal.Children.Clear();
        PanelSingleLineLengthsOriginal.Children.Add(UiUtil.MakeTextBlock(Se.Language.Main.SingleLineLength)
            .WithFontSize(12)
            .WithPadding(2));
        var first = true;
        for (var i = 0; i < lines.Count; i++)
        {
            if (first)
            {
                first = false;
            }
            else
            {
                PanelSingleLineLengthsOriginal.Children.Add(UiUtil.MakeTextBlock("/").WithFontSize(12).WithPadding(2));
            }

            var tb = UiUtil.MakeTextBlock(lineLenghts[i].Length.ToString(CultureInfo.InvariantCulture)).WithFontSize(12).WithPadding(2);
            if (Se.Settings.General.ColorTextTooLong &&
                lineLenghts[i].Length > Se.Settings.General.SubtitleLineMaximumLength)
            {
                tb.Background = _errorBrush;
            }

            PanelSingleLineLengthsOriginal.Children.Add(tb);
        }

        EditTextCharactersPerSecondOriginal = string.Format(Se.Language.Main.CharactersPerSecond, $"{cps:0.0}");
        EditTextTotalLengthOriginal = string.Format(Se.Language.Main.TotalCharacters, totalLength);

        EditTextCharactersPerSecondBackgroundOriginal = Se.Settings.General.ColorTextTooLong &&
                                                        cps > Se.Settings.General.SubtitleMaximumCharactersPerSeconds
            ? _errorBrush
            : _transparentBrush;

        EditTextTotalLengthBackgroundOriginal = Se.Settings.General.ColorTextTooLong &&
                                                totalLength > Se.Settings.General.SubtitleLineMaximumLength *
                                                lines.Count
            ? _errorBrush
            : _transparentBrush;
    }

    private bool _avLastScrolling = false;

    private void OnSubtitlesCollectionChangedForMpv(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        _mpvPreviewDirty = true;
        if (e.NewItems != null)
            foreach (SubtitleLineViewModel item in e.NewItems)
                item.PropertyChanged += OnSubtitleItemChangedForMpv;
        if (e.OldItems != null)
            foreach (SubtitleLineViewModel item in e.OldItems)
                item.PropertyChanged -= OnSubtitleItemChangedForMpv;
    }

    private void OnSubtitleItemChangedForMpv(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(SubtitleLineViewModel.Text)
            or nameof(SubtitleLineViewModel.StartTime)
            or nameof(SubtitleLineViewModel.EndTime))
        {
            _mpvPreviewDirty = true;
        }
    }

    private void StartTimers()
    {
        Subtitles.CollectionChanged += OnSubtitlesCollectionChangedForMpv;
        foreach (var item in Subtitles)
            item.PropertyChanged += OnSubtitleItemChangedForMpv;
        _positionTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
        _positionTimer.Tick += (s, e) =>
        {
            // update audio visualizer position if available
            var av = AudioVisualizer;
            var vp = GetVideoPlayerControl();
            if (vp != null && !string.IsNullOrEmpty(_videoFileName))
            {
                var isAvScrolloing = av?.IsScrolling ?? false;

                if (_setEndAtKeyUpLine != null)
                {
                    _setEndAtKeyUpLine.EndTime = TimeSpan.FromSeconds(vp.VideoPlayer.Position);
                    if (_setEndAtKeyUpLineGoToNext)
                    {
                        var idx = Subtitles.IndexOf(_setEndAtKeyUpLine);
                        if (idx >= 0)
                        {
                            SelectAndScrollToRow(idx + 1);
                        }

                        _setEndAtKeyUpLine = null;
                        _setEndAtKeyUpLineGoToNext = false;
                    }
                }

                var noLayers = _visibleLayers == null || !Se.Settings.Assa.HideLayersFromWaveform || _visibleLayers.Count == 0;
                var subtitle = _waveformSubtitleBuffer;
                subtitle.Clear();
                if (subtitle.Capacity < Subtitles.Count)
                {
                    subtitle.Capacity = Subtitles.Count;
                }
                if (noLayers)
                {
                    for (var i = 0; i < Subtitles.Count; i++)
                    {
                        subtitle.Add(Subtitles[i]);
                    }
                }
                else
                {
                    var layerSet = _visibleLayers!;
                    for (var i = 0; i < Subtitles.Count; i++)
                    {
                        var p = Subtitles[i];
                        if (layerSet.Contains(p.Layer))
                        {
                            subtitle.Add(p);
                        }
                    }
                }
                subtitle.Sort((a, b) => a.StartTime.Ticks.CompareTo(b.StartTime.Ticks));

                var mediaPlayerSeconds = vp.Position;
                var startPos = mediaPlayerSeconds - 0.01;
                if (startPos < 0)
                {
                    startPos = 0;
                }

                if (av != null)
                {
                    av.CurrentVideoPositionSeconds = vp.Position;
                }

                var isPlaying = vp.IsPlaying;
                var firstSelectedIndex = -1;

                if (WaveformCenter && isPlaying)
                {
                    // calculate the center position based on the waveform width
                    if (av != null)
                    {
                        var waveformHalfSeconds = (av.EndPositionSeconds - av.StartPositionSeconds) / 2.0;
                        av.SetPosition(Math.Max(0, mediaPlayerSeconds - waveformHalfSeconds), subtitle, mediaPlayerSeconds,
                            firstSelectedIndex, _selectedSubtitles ?? []);
                    }
                }
                else if (av != null && isPlaying && _avLastScrolling && !isAvScrolloing)
                {
                    if (vp.Position < av.StartPositionSeconds) // scrolling forward
                    {
                        vp.Position = av.StartPositionSeconds + 0.1;
                    }
                    else if (vp.Position > av.EndPositionSeconds) // scrolling backward
                    {
                        vp.Position = av.StartPositionSeconds + ((av.EndPositionSeconds - av.StartPositionSeconds) / 2.0);
                    }
                }
                else if (av != null && isPlaying &&
                         (mediaPlayerSeconds > av.EndPositionSeconds || mediaPlayerSeconds < av.StartPositionSeconds))
                {
                    av.SetPosition(startPos, subtitle, mediaPlayerSeconds, 0, _selectedSubtitles ?? []);
                }
                else if (av != null)
                {
                    av.SetPosition(av.StartPositionSeconds, subtitle, mediaPlayerSeconds, firstSelectedIndex,
                        _selectedSubtitles ?? []);
                }

                if (_updateAudioVisualizer && av != null)
                {
                    av.InvalidateVisual();
                    _updateAudioVisualizer = false;
                }

                if (isPlaying)
                {
                    Optris.Icons.Avalonia.Attached.SetIcon(ButtonWaveformPlay, IconNames.Pause);

                    if (_playSelectionItem != null && mediaPlayerSeconds >= _playSelectionItem.EndSeconds)
                    {
                        var p = _playSelectionItem.GetNextSubtitle(mediaPlayerSeconds);
                        if (p == null)
                        {
                            vp.VideoPlayer.Pause();
                            vp.Position = _playSelectionItem.EndSeconds;
                            ResetPlaySelection();
                        }
                        else
                        {
                            if (_playSelectionItem.HasGapOrIsFirst())
                            {
                                vp.Position = p.StartTime.TotalSeconds;
                            }

                            Dispatcher.UIThread.Post(() => { SubtitleGrid.ScrollIntoView(p, null); });
                        }
                    }

                    else if (SelectCurrentSubtitleWhilePlaying && _playSelectionItem == null)
                    {
                        var ss = SelectedSubtitle;
                        if (ss == null || mediaPlayerSeconds < ss.StartTime.TotalSeconds ||
                            mediaPlayerSeconds > ss.EndTime.TotalSeconds || ss.Duration.TotalSeconds > 20)
                        {
                            SubtitleLineViewModel? firstMatch = null;
                            var matchFound = false;
                            for (var i = 0; i < subtitle.Count; i++)
                            {
                                var p = subtitle[i];
                                if (mediaPlayerSeconds >= p.StartTime.TotalSeconds &&
                                    mediaPlayerSeconds <= p.EndTime.TotalSeconds)
                                {
                                    if (firstMatch == null)
                                    {
                                        firstMatch = p;
                                    }

                                    if (p.Duration.TotalSeconds < 20)
                                    {
                                        matchFound = true;
                                        SelectAndScrollToSubtitle(p);
                                        break;
                                    }
                                }
                            }

                            if (!matchFound && firstMatch != null)
                            {
                                SelectAndScrollToSubtitle(firstMatch);
                            }
                        }
                    }
                }
                else
                {
                    Optris.Icons.Avalonia.Attached.SetIcon(ButtonWaveformPlay, IconNames.Play);
                    ResetPlaySelection();
                }

                _avLastScrolling = isAvScrolloing;
            }
        };
        _positionTimer.Start();

        _slowTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(400) };
        _slowTimer.Tick += (s, e) =>
        {
            UpdateTitleStatus();
            UpdateGaps();

            var vp = GetVideoPlayerControl();
            if (!_mpvPreviewDirty || vp == null)
            {
                return;
            }

            // Filter once instead of: Where().ToList() + Clear + AddRange (which
            // allocates an extra List and walks the paragraphs three times).
            // Verified with BenchmarkDotNet at ~1.7-2x faster and 0-45 % less
            // allocation across 100/1000/5000-line subtitles.
            var hideLayers = _visibleLayers != null && Se.Settings.Assa.HideLayersFromVideoPreview;

            if (vp.VideoPlayer is LibMpvDynamicPlayer mpv)
            {
                var subtitle = GetUpdateSubtitle();
                _mpvPreviewDirty = false; // clear only after subtitle snapshot is successfully obtained
                if (hideLayers)
                {
                    subtitle.Paragraphs.RemoveAll(p => !_visibleLayers!.Contains(p.Layer));
                }

                _mpvReloader.RefreshMpv(mpv, subtitle, _subtitleSecondary, SelectedSubtitleFormat).ConfigureAwait(false);
            }
            else if (vp.VideoPlayer is LibVlcDynamicPlayer vlc)
            {
                var subtitle = GetUpdateSubtitle();
                _mpvPreviewDirty = false; // clear only after subtitle snapshot is successfully obtained
                if (hideLayers)
                {
                    subtitle.Paragraphs.RemoveAll(p => !_visibleLayers!.Contains(p.Layer));
                }

                _vlcReloader.RefreshVlc(vlc, subtitle, _subtitleSecondary, SelectedSubtitleFormat).ConfigureAwait(false);
            }
        };
        _slowTimer.Start();
    }

    private void UpdateTitleStatus()
    {
        var text = Se.Language.General.Untitled;
        if (!string.IsNullOrEmpty(_subtitleFileName))
        {
            text = Configuration.Settings.General.TitleBarFullFileName
                ? _subtitleFileName
                : Path.GetFileName(_subtitleFileName);
        }

        if (ShowColumnOriginalText)
        {
            text += " + ";

            if (_changeSubtitleHashOriginal != GetFastHashOriginal())
            {
                text += "*";
            }

            if (string.IsNullOrEmpty(_subtitleFileNameOriginal))
            {
                text += Se.Language.General.Untitled;
            }
            else
            {
                text += Configuration.Settings.General.TitleBarFullFileName
                    ? _subtitleFileNameOriginal
                    : Path.GetFileName(_subtitleFileNameOriginal);
            }
        }

        text = text + " - " + Se.Language.Title + " " + Se.Version;
        if (_changeSubtitleHash != GetFastHash())
        {
            text = "*" + text;
        }

        if (Window != null)
        {
            Window.Title = text;
        }
    }

    private void UpdateGaps()
    {
        try
        {
            if (Subtitles.Count == 0)
            {
                return;
            }

            var hasLayers = _visibleLayers != null && Se.Settings.Assa.HideLayersFromSubtitleGrid;

            for (var i = 0; i < Subtitles.Count - 1; i++)
            {
                var p = Subtitles[i];
                var next = Subtitles[i + 1];
                p.Gap = next.StartTime.TotalMilliseconds - p.EndTime.TotalMilliseconds;

                p.IsHidden = hasLayers && !_visibleLayers!.Contains(p.Layer);
            }

            Subtitles[Subtitles.Count - 1].Gap = double.MaxValue;
            Subtitles[Subtitles.Count - 1].IsHidden = hasLayers && !_visibleLayers!.Contains(Subtitles.Last().Layer);
            ;
        }
        catch
        {
            // ignore
        }
    }

    public void SubtitleTextChanged(object? sender, TextChangedEventArgs e)
    {
        var selectedSubtitle = SelectedSubtitle;
        if (selectedSubtitle == null)
        {
            return;
        }

        MakeSubtitleTextInfo(selectedSubtitle.Text, selectedSubtitle);
        _updateAudioVisualizer = true;
    }

    internal void SubtitleTextOriginalChanged(object? sender, TextChangedEventArgs e)
    {
        var selectedSubtitle = SelectedSubtitle;
        if (selectedSubtitle == null)
        {
            return;
        }

        MakeSubtitleTextInfoOriginal(selectedSubtitle.OriginalText, selectedSubtitle);
        _updateAudioVisualizer = true;
    }

    public void AudioVisualizerOnNewSelectionInsert(object sender, ParagraphEventArgs e)
    {
        var index = _insertService.InsertInCorrectPosition(Subtitles, e.Paragraph);
        SelectAndScrollToRow(index);
        Renumber();
        _updateAudioVisualizer = true;

        if (Se.Settings.Waveform.FocusTextBoxAfterInsertNew)
        {
            FocusEditTextBox();
        }

        if (Se.Settings.Tools.OpenAiCompatibleSttAutoTranscribeOnAudioSelection &&
            !string.IsNullOrEmpty(_videoFileName))
        {
            // Replace-in-flight: if the user makes another waveform selection
            // while a previous auto-transcribe is still running, cancel the
            // prior one so we don't pile up ffmpeg processes and HTTP calls.
            var previous = Interlocked.Exchange(ref _autoTranscribeCts, new CancellationTokenSource());
            previous?.Cancel();
            previous?.Dispose();
            _ = AutoTranscribeParagraphAsync(index, e.Paragraph, _autoTranscribeCts.Token);
        }
    }

    private async Task AutoTranscribeParagraphAsync(int index, SubtitleLineViewModel paragraph, CancellationToken cancellationToken)
    {
        // Dispatch by the user's currently selected STT engine. Only engines
        // with a fast headless transcription path are wired up here; engines
        // that require process startup (whisper.cpp, etc.) are not invoked
        // from the waveform-selection hook.
        var choice = Se.Settings.Tools.AudioToText.WhisperChoice;
        if (choice == WhisperChoice.OpenAiCompatible)
        {
            if (!OpenAiSttService.IsConfigured())
            {
                return;
            }

            await AutoTranscribeParagraphViaOpenAiAsync(index, paragraph, cancellationToken);
        }
    }

    private async Task AutoTranscribeParagraphViaOpenAiAsync(int index, SubtitleLineViewModel paragraph, CancellationToken cancellationToken)
    {
        TranscriptionProgressViewModel? progressViewModel = null;
        TranscriptionProgressWindow? progressWindow = null;

        var videoFileName = _videoFileName;
        if (string.IsNullOrEmpty(videoFileName) || Window == null)
        {
            return;
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var ffmpegOk = await RequireFfmpegOk();
            if (!ffmpegOk)
            {
                return;
            }

            var outputFileName = Path.Combine(Path.GetTempPath(), $"se_audioclip_{Guid.NewGuid()}.wav");
            var useCenterChannelOnly = false;
            var arguments = FfmpegGenerator.ExtractAudioClipFromVideoParameters(
                videoFileName,
                paragraph.StartTime.TotalSeconds,
                paragraph.Duration.TotalSeconds,
                useCenterChannelOnly,
                outputFileName);

            var processStartInfo = new ProcessStartInfo
            {
                FileName = Se.Settings.General.FfmpegPath,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var process = Process.Start(processStartInfo);
            if (process == null)
            {
                return;
            }

            try
            {
                await process.WaitForExitAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                try { if (!process.HasExited) { process.Kill(entireProcessTree: true); } } catch { /* ignore */ }
                throw;
            }

            if (process.ExitCode != 0 || !File.Exists(outputFileName))
            {
                return;
            }

            var settings = OpenAiSttService.GetSettingsFromConfiguration();
            progressViewModel = new TranscriptionProgressViewModel();
            progressViewModel.StatusText = Se.Language.Video.AudioToText.Transcribing;
            progressViewModel.ServerUrl = settings.EndpointUrl;
            progressViewModel.ModelName = string.IsNullOrEmpty(settings.Model) ? Se.Language.General.TranscriptionProgressModelAuto : settings.Model;

            var ownerWindow = Window!;
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                progressWindow = new TranscriptionProgressWindow(progressViewModel);
                progressWindow.Show(ownerWindow);
            });

            var service = new OpenAiSttService(settings);

            var streamingText = new StringBuilder();
            var progress = new Progress<string>(delta =>
            {
                streamingText.Append(delta);
                progressViewModel.UpdateStreamedText(delta);
            });

            var segmentProgress = new Progress<OpenAiCompatibleSegment>(seg =>
            {
                progressViewModel.AddSegment(seg.Text, seg.Start, seg.End);
            });

            // Cancel on either: the outer in-flight CTS (a new waveform
            // selection replaces this one) or the progress window's Cancel
            // button.
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                progressViewModel.CancellationToken);

            var response = await service.TranscribeAsync(
                outputFileName,
                settings.Language,
                progress,
                segmentProgress,
                linkedCts.Token);

            progressViewModel.SetCompleted();
            await Task.Delay(300, linkedCts.Token);

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                progressWindow?.Close();
            });

            try { File.Delete(outputFileName); } catch { /* ignore */ }

            if (response?.Segments != null && response.Segments.Count > 0)
            {
                var segmentsOrdered = response.Segments.OrderBy(s => s.Start).ToList();

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (index < 0 || index >= Subtitles.Count)
                    {
                        return;
                    }

                    if (segmentsOrdered.Count == 1)
                    {
                        var text = segmentsOrdered[0].Text.Trim();
                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            Subtitles[index].Text = text;
                        }
                    }
                    else
                    {
                        var originalLine = Subtitles[index];
                        var sourceStartMs = originalLine.StartTime.TotalMilliseconds;
                        var sourceEndMs = originalLine.EndTime.TotalMilliseconds;

                        Subtitles.RemoveAt(index);

                        // Map server-reported times (seconds, relative to the
                        // extracted clip that starts at 0) to the source
                        // selection by simple offset addition. Clamp to the
                        // source end and ensure non-zero duration.
                        for (int segIdx = 0; segIdx < segmentsOrdered.Count; segIdx++)
                        {
                            var seg = segmentsOrdered[segIdx];
                            var text = seg.Text.Trim();
                            if (string.IsNullOrWhiteSpace(text))
                            {
                                continue;
                            }

                            var segStartMs = sourceStartMs + (seg.Start * 1000.0);
                            var segEndMs = sourceStartMs + (seg.End * 1000.0);

                            segEndMs = Math.Min(segEndMs, sourceEndMs);

                            if (segEndMs <= segStartMs)
                            {
                                if (segIdx == segmentsOrdered.Count - 1)
                                {
                                    segEndMs = sourceEndMs;
                                }
                                else
                                {
                                    segEndMs = segStartMs + 500;
                                }
                            }

                            var newParagraph = new Paragraph(text, segStartMs, segEndMs);
                            var newLine = new SubtitleLineViewModel(newParagraph, SelectedSubtitleFormat);
                            _insertService.InsertInCorrectPosition(Subtitles, newLine);
                        }

                        Renumber();
                        _updateAudioVisualizer = true;
                    }
                });
            }
            else if (!string.IsNullOrWhiteSpace(response?.Text))
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (index >= 0 && index < Subtitles.Count)
                    {
                        Subtitles[index].Text = response.Text.Trim();
                    }
                });
            }
        }
        catch (OperationCanceledException)
        {
            // User cancelled
            if (progressWindow != null)
            {
                await Dispatcher.UIThread.InvokeAsync(() => progressWindow.Close());
            }
        }
        catch (Exception ex)
        {
            SeLogger.Error(ex);
            if (progressWindow != null)
            {
                await Dispatcher.UIThread.InvokeAsync(() => progressWindow.Close());
            }
        }
    }

    internal void AudioVisualizerOnVideoPositionChanged(object sender, AudioVisualizer.PositionEventArgs e)
    {
        var vp = GetVideoPlayerControl();
        if (vp == null)
        {
            return;
        }

        var newPosition = e.PositionInSeconds;
        newPosition = Math.Max(0, newPosition);
        newPosition = Math.Min(vp.Duration, newPosition);

        vp.Position = newPosition;

        _updateAudioVisualizer = true;
    }

    internal void OnVideoPlayerUserSeeked(double newPositionSeconds)
    {
        var av = AudioVisualizer;
        if (av?.WavePeaks == null || av.Bounds.Width <= 0 || av.ZoomFactor <= 0 || av.WavePeaks.SampleRate <= 0)
        {
            return;
        }

        var halfWidthInSeconds = (av.Bounds.Width / 2) / (av.WavePeaks.SampleRate * av.ZoomFactor);
        av.StartPositionSeconds = newPositionSeconds - halfWidthInSeconds;
    }

    internal void AudioVisualizerOnStatus(object sender, ParagraphEventArgs e)
    {
        ShowStatus(e.Paragraph.Text, 3000);
    }


    internal void OnSubtitleGridDoubleTapped(object? sender, TappedEventArgs e)
    {
        _singleTapCancellationTokenSource?.Cancel();
        OnSubtitleGridDoubleTapped(sender);
    }

    internal void OnSubtitleGridDoubleTapped(object? sender)
    {
        if (sender is not DataGrid grid || grid.SelectedItem == null)
        {
            return;
        }

        if (grid.SelectedItem is SubtitleLineViewModel selectedItem)
        {
            if (Se.Settings.General.SubtitleDoubleClickAction == SubtitleDoubleClickActionType.None.ToString())
            {
                return;
            }

            var vp = GetVideoPlayerControl();
            if (vp == null)
            {
                return;
            }

            var seconds = selectedItem.StartTime.TotalSeconds;
            if (Se.Settings.General.SubtitleDoubleClickAction == SubtitleDoubleClickActionType.GoToSubtitleOnly.ToString())
            {
                vp.Position = seconds;
                AudioVisualizerCenterOnPositionIfNeeded(selectedItem, seconds);
                return;
            }

            if (Se.Settings.General.SubtitleDoubleClickAction == SubtitleDoubleClickActionType.GoToSubtitleAndPlay.ToString())
            {
                vp.Position = seconds;
                vp.VideoPlayer.Play();
                AudioVisualizerCenterOnPositionIfNeeded(selectedItem, seconds);
                return;
            }

            if (Se.Settings.General.SubtitleDoubleClickAction == SubtitleDoubleClickActionType.GoToSubtitleAndPauseAndFocusTextBox.ToString())
            {
                vp.VideoPlayer.Pause();
                vp.Position = seconds;
                AudioVisualizerCenterOnPositionIfNeeded(selectedItem, seconds);
                FocusEditTextBox();
                return;
            }

            // SubtitleDoubleClickActionType.GoToSubtitleAndPause
            vp.VideoPlayer.Pause();
            vp.Position = seconds;
            AudioVisualizerCenterOnPositionIfNeeded(selectedItem, seconds);
        }
    }

    private void AudioVisualizerCenterOnPositionIfNeeded(SubtitleLineViewModel selectedItem, double seconds)
    {
        if (AudioVisualizer != null)
        {
            if (seconds <= AudioVisualizer.StartPositionSeconds ||
                seconds + 0.2 >= AudioVisualizer.EndPositionSeconds)
            {
                AudioVisualizer.CenterOnPosition(selectedItem);
                _updateAudioVisualizer = true;
            }
        }
    }

    private void AudioVisualizerCenterOnPositionIfNeeded(double seconds)
    {
        if (AudioVisualizer != null)
        {
            if (seconds <= AudioVisualizer.StartPositionSeconds ||
                seconds + 0.2 >= AudioVisualizer.EndPositionSeconds)
            {
                AudioVisualizer.CenterOnPosition(seconds);
                _updateAudioVisualizer = true;
            }
        }
    }

    private CancellationTokenSource? _singleTapCancellationTokenSource;

    internal async void OnSubtitleGridSingleTapped(object? sender, TappedEventArgs e)
    {
        _singleTapCancellationTokenSource?.Cancel();
        var cts = _singleTapCancellationTokenSource = new CancellationTokenSource();

        try
        {
            await Task.Delay(250, cts.Token); // double-tap threshold

            // Single-tap logic here
            OnSubtitleGridSingleTapped(sender);
        }
        catch (TaskCanceledException)
        {
            // ignore
        }
    }

    internal void OnSubtitleGridSingleTapped(object? sender)
    {
        if (sender is not DataGrid grid || grid.SelectedItem == null)
        {
            return;
        }

        if (grid.SelectedItem is SubtitleLineViewModel selectedItem)
        {
            if (Se.Settings.General.SubtitleSingleClickAction == SubtitleSingleClickActionType.None.ToString())
            {
                return;
            }

            var vp = GetVideoPlayerControl();
            if (vp == null)
            {
                return;
            }

            var seconds = selectedItem.StartTime.TotalSeconds;
            if (Se.Settings.General.SubtitleSingleClickAction == SubtitleSingleClickActionType.GoToWaveformOnlyNoVideoPosition.ToString())
            {
                AudioVisualizerCenterOnPositionIfNeeded(selectedItem, seconds);
                return;
            }

            if (Se.Settings.General.SubtitleSingleClickAction == SubtitleSingleClickActionType.GoToSubtitleAndPause.ToString())
            {
                vp.VideoPlayer.Pause();
                vp.Position = seconds;
                AudioVisualizerCenterOnPositionIfNeeded(selectedItem, seconds);
                return;
            }

            if (Se.Settings.General.SubtitleSingleClickAction == SubtitleSingleClickActionType.GoToSubtitleAndSetVideoPosition.ToString())
            {
                vp.Position = seconds;
                AudioVisualizerCenterOnPositionIfNeeded(selectedItem, seconds);
                return;
            }

            if (Se.Settings.General.SubtitleSingleClickAction == SubtitleSingleClickActionType.GoToSubtitleAndPlay.ToString())
            {
                vp.Position = seconds;
                vp.VideoPlayer.Play();
                AudioVisualizerCenterOnPositionIfNeeded(selectedItem, seconds);
                return;
            }

            if (Se.Settings.General.SubtitleSingleClickAction == SubtitleSingleClickActionType.GoToSubtitleAndPauseAndFocusTextBox.ToString())
            {
                vp.VideoPlayer.Pause();
                vp.Position = seconds;
                AudioVisualizerCenterOnPositionIfNeeded(selectedItem, seconds);
                FocusEditTextBox();
            }
        }
    }

    public void AudioVisualizerOnToggleSelection(object sender, ParagraphEventArgs e)
    {
        var vp = GetVideoPlayerControl();
        if (!string.IsNullOrEmpty(_videoFileName) && vp != null)
        {
            var p = Subtitles.FirstOrDefault(p =>
                Math.Abs(p.StartTime.TotalMilliseconds - e.Paragraph.StartTime.TotalMilliseconds) < 0.01);
            if (p != null)
            {
                var selectedItems = SubtitleGrid.SelectedItems;
                if (selectedItems.Contains(p))
                {
                    if (selectedItems.Count != 1 || selectedItems[0] != p)
                    {
                        selectedItems.Remove(p);
                    }
                }
                else
                {
                    selectedItems.Add(p);
                }

                _updateAudioVisualizer = true;
            }
        }
    }

    public void Adjust(TimeSpan adjustment, bool adjustAll, bool adjustSelectedLines,
        bool adjustSelectedLinesAndForward)
    {
        if (Math.Abs(adjustment.TotalMilliseconds) < 0.01)
        {
            return;
        }

        if (adjustSelectedLines)
        {
            foreach (SubtitleLineViewModel p in SubtitleGrid.SelectedItems)
            {
                p.StartTime += adjustment;
                p.UpdateDuration();
            }
        }
        else if (adjustSelectedLinesAndForward)
        {
            var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
            if (selectedItems.Count > 0)
            {
                var first = selectedItems.OrderBy(p => Subtitles.IndexOf(p)).First();
                var firstSelectedIndex = Subtitles.IndexOf(first);
                for (var i = firstSelectedIndex; i < Subtitles.Count; i++)
                {
                    var p = Subtitles[i];
                    p.StartTime += adjustment;
                    p.UpdateDuration();
                }
            }
        }
        else if (adjustAll)
        {
            foreach (var p in Subtitles)
            {
                p.StartTime += adjustment;
                p.UpdateDuration();
            }
        }

        _updateAudioVisualizer = true;
    }


    internal void DurationChanged()
    {
        _updateAudioVisualizer = true;

        var selectedSubtitle = SelectedSubtitle;
        if (selectedSubtitle == null)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            MakeSubtitleTextInfo(selectedSubtitle.Text, selectedSubtitle);
            MakeSubtitleTextInfoOriginal(selectedSubtitle.OriginalText, selectedSubtitle);
        });
    }

    internal void StartTimeChanged(object? sender, TimeSpan e)
    {
        _updateAudioVisualizer = true;

        var selectedSubtitle = SelectedSubtitle;
        if (selectedSubtitle == null)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            MakeSubtitleTextInfo(selectedSubtitle.Text, selectedSubtitle);
            MakeSubtitleTextInfoOriginal(selectedSubtitle.OriginalText, selectedSubtitle);
        });
    }

    internal void EndTimeChanged(object? sender, TimeSpan e)
    {
        _updateAudioVisualizer = true;

        var selectedSubtitle = SelectedSubtitle;
        if (selectedSubtitle == null)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            MakeSubtitleTextInfo(selectedSubtitle.Text, selectedSubtitle);
            MakeSubtitleTextInfoOriginal(selectedSubtitle.OriginalText, selectedSubtitle);
        });
    }

    public void GoToAndFocusLine(SubtitleLineViewModel p)
    {
        SelectAndScrollToSubtitle(p);
    }

    internal void TextBoxContextOpening(object? sender, EventArgs e)
    {
        IsTextBoxSplitAtCursorAndVideoPositionVisible = false;

        var s = SelectedSubtitle;
        var vp = GetVideoPlayerControl();
        if (vp != null && !string.IsNullOrEmpty(_videoFileName) && s != null)
        {
            if (s.StartTime.TotalSeconds < vp.Position &&
                vp.Position < s.EndTime.TotalSeconds)
            {
                IsTextBoxSplitAtCursorAndVideoPositionVisible = true;
            }
        }

        // Clear previous spell check menu items (identified by their Tag)
        if (sender is MenuFlyout flyout)
        {
            var itemsToRemove = flyout.Items.Where(item => item is MenuItem mi && mi.Tag?.ToString() == "SpellCheck" ||
                                                           item is Separator sep && sep.Tag?.ToString() == "SpellCheck")
                .ToList();
            foreach (var item in itemsToRemove)
            {
                flyout.Items.Remove(item);
            }

            // Add spell check suggestions if available
            if (EditTextBox is TextEditorWrapper wrapper && wrapper.IsSpellCheckEnabled && _lastTextEditorPointerArgs != null)
            {
                var word = wrapper.GetWordAtPosition(_lastTextEditorPointerArgs);
                if (word != null && wrapper.IsWordMisspelledAtOffset(word.Index))
                {
                    var suggestions = wrapper.GetSuggestionsForWordAtOffset(word.Index);
                    if (suggestions != null && suggestions.Count > 0)
                    {
                        // Insert suggestions at the beginning of the menu
                        var insertIndex = 0;

                        // Add first 5 suggestions
                        foreach (var suggestion in suggestions.Take(5))
                        {
                            var suggestionItem = new MenuItem
                            {
                                Header = suggestion,
                                FontWeight = FontWeight.Bold,
                                Tag = "SpellCheck"
                            };
                            suggestionItem.Click += (_, _) =>
                            {
                                var text = wrapper.Text;
                                wrapper.Text = text.Remove(word.Index, word.Length)
                                    .Insert(word.Index, suggestion);
                                wrapper.CaretIndex = word.Index + suggestion.Length;
                            };
                            flyout.Items.Insert(insertIndex++, suggestionItem);
                        }

                        // Add separator
                        flyout.Items.Insert(insertIndex++, new Separator { Tag = "SpellCheck" });

                        // Add "Add to Dictionary"
                        var addToDictItem = new MenuItem
                        {
                            Header = string.Format(Se.Language.SpellCheck.AddXToUserDictionary, word.Text),
                            Tag = "SpellCheck"
                        };
                        addToDictItem.Click += (_, _) =>
                        {
                            _spellCheckManager.AdToUserDictionary(word.Text);
                            wrapper.RefreshSpellCheck();
                        };
                        flyout.Items.Insert(insertIndex++, addToDictItem);

                        // Add "Ignore All"
                        var ignoreAllItem = new MenuItem
                        {
                            Header = string.Format(Se.Language.SpellCheck.IgnoreAllX, word.Text),
                            Tag = "SpellCheck"
                        };
                        ignoreAllItem.Click += (_, _) =>
                        {
                            _spellCheckManager.AddIgnoreWord(word.Text);
                            wrapper.RefreshSpellCheck();
                        };
                        flyout.Items.Insert(insertIndex++, ignoreAllItem);

                        var changeDictionary = new MenuItem
                        {
                            Header = Se.Language.SpellCheck.PickSpellCheckDictionaryDotDotDot,
                            Tag = "SpellCheck"
                        };
                        changeDictionary.Click += (_, _) => { PickLiveSpellCheckDictionary(wrapper); };
                        flyout.Items.Insert(insertIndex++, changeDictionary);

                        // Add separator after spell check items
                        flyout.Items.Insert(insertIndex, new Separator { Tag = "SpellCheck" });
                    }
                }

                // Clear the stored pointer args after use
                _lastTextEditorPointerArgs = null;
            }
        }
    }

    [RelayCommand]
    private void CommandPickLiveSpellCheckDictionary()
    {
        if (EditTextBox is TextEditorWrapper wrapper)
        {
            PickLiveSpellCheckDictionary(wrapper);
        }
    }

    private void PickLiveSpellCheckDictionary(TextEditorWrapper wrapper)
    {
        Dispatcher.UIThread.Post(async () =>
        {
            var result = await ShowDialogAsync<PickSpellCheckDictionaryWindow, PickSpellCheckDictionaryViewModel>();
            if (result != null && result.SelectedDictionary != null)
            {
                var twoLetterLanguageCode = Iso639Dash2LanguageCode.GetTwoLetterCodeFromThreeLetterCode(result.SelectedDictionary.GetThreeLetterCode());
                _spellCheckManager.Initialize(result.SelectedDictionary.DictionaryFileName, twoLetterLanguageCode);
                wrapper.RefreshSpellCheck();
                ShowStatus(string.Format(Se.Language.Main.LiveSpellCheckLanguageXLoaded, result.SelectedDictionary.Name));
            }
        });
    }

    private async void OnWindowClosing(object? sender, WindowClosingEventArgs e)
    {
        if (Window == null || !HasChanges())
        {
            return;
        }

        e.Cancel = true;

        var result = await MessageBox.Show(
            Window,
            Se.Language.General.SaveChangesTitle,
            Se.Language.General.SaveChangesMessage,
            MessageBoxButtons.YesNoCancel,
            MessageBoxIcon.Question);

        if (result == MessageBoxResult.Cancel)
        {
            return;
        }

        if (result == MessageBoxResult.Yes)
        {
            await SaveSubtitle();
        }

        Window.Closing -= OnWindowClosing;
        Window.Close();
    }

    public void AudioVisualizerFlyoutMenuOpening(object sender, AudioVisualizer.ContextEventArgs e)
    {
        MenuItemAudioVisualizerInsertNewSelection.IsVisible = false;
        MenuItemAudioVisualizerPasteNewSelection.IsVisible = false;
        MenuIteminsertSubtitleFileAtPositionMenuItem.IsVisible = false;
        MenuItemAudioVisualizerInsertAtPosition.IsVisible = false;
        MenuItemAudioVisualizerPasteFromClipboardMenuItem.IsVisible = false;
        MenuItemAudioVisualizerInsertBefore.IsVisible = false;
        MenuItemAudioVisualizerInsertAfter.IsVisible = false;
        MenuItemAudioVisualizerSeparator1.IsVisible = false;
        MenuItemAudioVisualizerDelete.IsVisible = false;
        MenuItemAudioVisualizerDeleteAtPosition.IsVisible = false;
        MenuItemAudioVisualizerSplitAtPosition.IsVisible = false;
        MenuItemAudioVisualizerSplit.IsVisible = false;
        MenuItemAudioVisualizerMergeWithPrevious.IsVisible = false;
        MenuItemAudioVisualizerMergeWithNext.IsVisible = false;
        MenuItemAudioVisualizerSpeechToTextSelectedLines.IsVisible = false;
        MenuItemAudioVisualizerSpeechToTextNewSelection.IsVisible = false;
        MenuItemAudioVisualizerExtractAudio.IsVisible = false;

        if (e.NewParagraph != null)
        {
            MenuItemAudioVisualizerInsertNewSelection.IsVisible = true;
            MenuItemAudioVisualizerPasteNewSelection.IsVisible = true;
            MenuItemAudioVisualizerSpeechToTextNewSelection.IsVisible = !string.IsNullOrEmpty(_videoFileName);
            return;
        }

        var selectedSubtitles = _selectedSubtitles;
        var subtitlesAtPosition = Subtitles
            .Where(p => p.StartTime.TotalSeconds < e.PositionInSeconds &&
                        p.EndTime.TotalSeconds > e.PositionInSeconds).ToList();
        var selectedIdx = SubtitleGrid.SelectedIndex;
        var isLast = selectedIdx >= 0 && selectedIdx == Subtitles.Count - 1;

        var vp = GetVideoPlayerControl();
        if (vp != null && !string.IsNullOrEmpty(_videoFileName))
        {
            var lastSeconds = Subtitles.LastOrDefault()?.EndTime.TotalSeconds ?? 0;
            MenuIteminsertSubtitleFileAtPositionMenuItem.IsVisible = vp.Position > lastSeconds;
        }

        if (subtitlesAtPosition.Count > 0)
        {
            MenuItemAudioVisualizerDeleteAtPosition.IsVisible = true;
            MenuItemAudioVisualizerSplitAtPosition.IsVisible = true;
            MenuItemAudioVisualizerSpeechToTextSelectedLines.IsVisible = true;
        }

        if (selectedSubtitles?.Count == 1 &&
            subtitlesAtPosition.Count == 1 &&
            selectedSubtitles[0] == subtitlesAtPosition[0])
        {
            MenuItemAudioVisualizerInsertBefore.IsVisible = true;
            MenuItemAudioVisualizerInsertAfter.IsVisible = true;
            MenuItemAudioVisualizerSeparator1.IsVisible = true;
            MenuItemAudioVisualizerDelete.IsVisible = true;
            MenuItemAudioVisualizerSplit.IsVisible = true;
            MenuItemAudioVisualizerMergeWithPrevious.IsVisible = selectedIdx > 0;
            MenuItemAudioVisualizerMergeWithNext.IsVisible = !isLast;
            MenuItemAudioVisualizerExtractAudio.IsVisible = !string.IsNullOrEmpty(_videoFileName);
            return;
        }

        MenuItemAudioVisualizerInsertAtPosition.IsVisible = true;
        if (subtitlesAtPosition.Count == 0)
        {
            // Check clipboard safely with timeout to prevent UI freeze on Linux
            _ = Task.Run(async () =>
            {
                try
                {
                    if (Window != null)
                    {
                        var clipboardText = await ClipboardHelper.GetTextAsync(Window);
                        if (!string.IsNullOrEmpty(clipboardText))
                        {
                            await Dispatcher.UIThread.InvokeAsync(() => { MenuItemAudioVisualizerPasteFromClipboardMenuItem.IsVisible = !string.IsNullOrEmpty(clipboardText); });
                        }
                    }
                }
                catch
                {
                    // On error, hide paste menu item (safe default)
                }
            });

            return;
        }
    }

    internal void SubtitleTextBoxGotFocus()
    {
        if (Subtitles.Count == 0)
        {
            var newSubtitle = new SubtitleLineViewModel
            {
                StartTime = TimeSpan.Zero,
                EndTime = TimeSpan.FromMilliseconds(Se.Settings.General.NewEmptyDefaultMs),
                Text = string.Empty,
                OriginalText = string.Empty,
                Number = 1
            };

            if (SelectedSubtitleFormat is AdvancedSubStationAlpha or SubStationAlpha)
            {
                if (string.IsNullOrEmpty(_subtitle.Header))
                {
                    _subtitle.Header = AdvancedSubStationAlpha.DefaultHeader;
                }

                var styles = AdvancedSubStationAlpha.GetStylesFromHeader(_subtitle.Header);
                newSubtitle.Style = styles.Count > 0 ? styles[0] : "Default";
            }

            Subtitles.Add(newSubtitle);
            SelectedSubtitle = newSubtitle;
            SelectedSubtitleIndex = 0;
        }
    }

    internal void ComboBoxSubtitleFormatChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!_changingFormatProgrammatically)
        {
            _formatChangedByUser = true;
            _formatChangedFrom = e.RemovedItems.Count == 1 ? e.RemovedItems[0] as SubtitleFormat : null;
        }

        IsFormatAssa = SelectedSubtitleFormat is AdvancedSubStationAlpha;
        IsFormatSsa = SelectedSubtitleFormat is SubStationAlpha;
        HasFormatStyle = SelectedSubtitleFormat is AdvancedSubStationAlpha or SubStationAlpha;
        ShowLayer = IsFormatAssa && Se.Settings.Appearance.ShowLayer;
        ShowLayerFilterIcon = IsFormatAssa && Se.Settings.Appearance.ShowLayer && _visibleLayers != null;

        if (!IsFormatAssa)
        {
            ShowColumnLayer = false;
            ShowColumnLayerFlyoutMenuItem = false;
        }

        AutoFitColumns();

        var idx = SubtitleGrid.SelectedIndex;

        if (!_opening && !_changingFormatProgrammatically && e.RemovedItems.Count == 1 && e.AddedItems.Count == 1)
        {
            var format = e.AddedItems[0] as SubtitleFormat;
            var oldFormat = e.RemovedItems[0] as SubtitleFormat;

            if (oldFormat != null && format != null)
            {
                _subtitle = GetUpdateSubtitle();
                _subtitleOriginal = GetUpdateSubtitleOriginal();

                oldFormat.RemoveNativeFormatting(_subtitle, format);

                if (format is AdvancedSubStationAlpha)
                {
                    if (oldFormat is WebVTT || oldFormat is WebVTTFileWithLineNumber)
                    {
                        //                        _subtitle = WebVttToAssa.Convert(_subtitle, new SsaStyle(), VideoPlayerControl?.VideoPlayer?.Width ?? 0, VideoPlayerControl?.VideoPlayer?.Height ?? 0);
                    }

                    foreach (var p in _subtitle.Paragraphs)
                    {
                        p.Text = AdvancedSubStationAlpha.FormatText(p.Text);
                    }

                    if (oldFormat is SubStationAlpha)
                    {
                        if (_subtitle.Header != null && !_subtitle.Header.Contains("[V4+ Styles]"))
                        {
                            _subtitle.Header =
                                AdvancedSubStationAlpha.GetHeaderAndStylesFromSubStationAlpha(_subtitle.Header);
                            foreach (var p in _subtitle.Paragraphs)
                            {
                                if (p.Extra != null)
                                {
                                    p.Extra = p.Extra.TrimStart('*');
                                }
                            }
                        }
                    }
                    else if (oldFormat is AdvancedSubStationAlpha && string.IsNullOrEmpty(_subtitle.Header))
                    {
                        _subtitle.Header = AdvancedSubStationAlpha.DefaultHeader;
                    }

                    SetAssaResolution(true);
                }

                SetSubtitles(_subtitle, _subtitleOriginal);
            }
        }

        IsFilePropertiesVisible = false;
        if (e.AddedItems.Count == 1)
        {
            var format = e.AddedItems[0] as SubtitleFormat;
            if (format is TimedTextImsc11 or ItunesTimedText or TimedText10 or TimedTextImscRosetta or TmpegEncXml or DCinemaSmpte2007 or DCinemaSmpte2010 or DCinemaSmpte2014)
            {
                IsFilePropertiesVisible = true;
                FilePropertiesText = string.Format(Se.Language.Main.XPropertiesDotDotDot, format.Name);
            }
        }

        SelectAndScrollToRow(idx);
    }

    internal void AutoSelectOnPlayCheckedChanged()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Task.Delay(50);
            Se.Settings.General.SelectCurrentSubtitleWhilePlaying = SelectCurrentSubtitleWhilePlaying;
        });
    }

    internal void WaveformCenterCheckedChanged()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Task.Delay(50);
            Se.Settings.Waveform.CenterVideoPosition = WaveformCenter;
        });
    }

    internal void SubtitleGridOnDragOver(object? sender, DragEventArgs e)
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

    internal void SubtitleGridOnDrop(object? sender, DragEventArgs e)
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
                var doContinue = await HasChangesContinue();
                if (!doContinue)
                {
                    return;
                }

                foreach (var file in files)
                {
                    var path = file.Path?.LocalPath;
                    if (path != null && File.Exists(path))
                    {
                        await SubtitleOpen(path);
                    }
                }
            });
        }
    }

    internal void VideoOnDragOver(object? sender, DragEventArgs e)
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

    internal void VideoOnDrop(object? sender, DragEventArgs e)
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
                foreach (var file in files)
                {
                    var path = file.Path?.LocalPath;
                    if (path != null && File.Exists(path))
                    {
                        var ext = Path.GetExtension(path).ToLowerInvariant();
                        var subtitleExtensions = new List<string>
                        {
                            ".ass",
                            ".cap",
                            ".dfxp",
                            ".pac",
                            ".sami",
                            ".smi",
                            ".srt",
                            ".ssa",
                            ".stl",
                            ".sub",
                            ".sup",
                            ".ttml",
                            ".txt",
                            ".vtt",
                            ".xml",
                        };

                        if (subtitleExtensions.Contains(ext))
                        {
                            var doContinue = await HasChangesContinue();
                            if (!doContinue)
                            {
                                return;
                            }

                            await SubtitleOpen(path);
                            return;
                        }

                        await VideoOpenFile(path);
                        await TryLoadAssociatedSubtitle(path);
                    }
                }
            });
        }
    }

    private async Task TryLoadAssociatedSubtitle(string videoFileName)
    {
        if (!FindSubtitleFileName.TryFindSubtitleFileName(videoFileName, out var foundSubtitleFileName))
        {
            return;
        }

        // Already loaded in the editor - nothing to do.
        if (!string.IsNullOrEmpty(_subtitleFileName) &&
            string.Equals(Path.GetFullPath(_subtitleFileName), Path.GetFullPath(foundSubtitleFileName), StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        // Prompt + run the save-if-unsaved flow whenever either column has content,
        // so the original-text column's unsaved edits aren't silently discarded.
        if (!IsEmpty || !IsEmptyOriginal)
        {
            var answer = await MessageBox.Show(
                Window!,
                Se.Language.Title,
                string.Format(Se.Language.Main.OpenSubtitleFileX, Path.GetFileName(foundSubtitleFileName)),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (answer != MessageBoxResult.Yes)
            {
                return;
            }

            var doContinue = await HasChangesContinue();
            if (!doContinue)
            {
                return;
            }
        }

        await SubtitleOpen(foundSubtitleFileName, skipLoadVideo: true);
    }

    internal void AudioVisualizerOnDeletePressed(object sender, ParagraphEventArgs e)
    {
        Dispatcher.UIThread.Post(async void () => { await DeleteSelectedItems(); });
    }

    public void ControlMacPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (OperatingSystem.IsMacOS() &&
            e.KeyModifiers.HasFlag(KeyModifiers.Control) &&
            !e.KeyModifiers.HasFlag(KeyModifiers.Shift) &&
            (e.InitialPressMouseButton == MouseButton.Left || e.InitialPressMouseButton == MouseButton.Right) &&
            sender is Control control)
        {
            var args = new ContextRequestedEventArgs(e);
            control.RaiseEvent(args);
            e.Handled = args.Handled;
            _shortcutManager.ClearKeys();
        }
    }

    public void StoreTextEditorPointerArgs(PointerEventArgs e)
    {
        _lastTextEditorPointerArgs = e;
    }

    internal void VideoPlayerControlPointerPressed(PointerPressedEventArgs args)
    {
        var mediaInfo = _mediaInfo;
        if (mediaInfo == null || Window == null)
        {
            return;
        }

        var props = args.GetCurrentPoint(null).Properties;
        var isMacCtrlLeftClick = OperatingSystem.IsMacOS()
                                 && props.IsLeftButtonPressed
                                 && args.KeyModifiers.HasFlag(KeyModifiers.Control)
                                 && !args.KeyModifiers.HasFlag(KeyModifiers.Shift);

        if (!props.IsRightButtonPressed && !isMacCtrlLeftClick)
        {
            return;
        }

        args.Handled = true;
        ShowMediaInformation();
    }

    [RelayCommand]
    private void ShowMediaInformation()
    {
        if (Window == null || string.IsNullOrEmpty(_videoFileName))
        {
            return;
        }

        var mediaInfo = _mediaInfo;
        Dispatcher.UIThread.Post(async void () =>
        {
            var result = await ShowDialogAsync<MediaInfoViewWindow, MediaInfoViewViewModel>(vm => { vm.Initialize(_videoFileName ?? string.Empty, mediaInfo); });
        });
    }

    internal void ComboBoxSubtitleFormatKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key >= Key.A && e.Key <= Key.Z)
        {
            _dropDownFormatsSearchTimer.Stop();
            _dropDownFormatsSearchText += e.Key.ToString();
            _dropDownFormatsSearchTimer.Start();

            var items = SubtitleFormats;
            if (items != null)
            {
                var match = items.FirstOrDefault(item =>
                    item.Name?.StartsWith(_dropDownFormatsSearchText, StringComparison.OrdinalIgnoreCase) == true);

                if (match != null && sender is ComboBox cb)
                {
                    cb.SelectedItem = match;
                }
            }
        }
    }

    internal void VideoPlayerAreaPointerPressed()
    {
        if (string.IsNullOrEmpty(_videoFileName))
        {
            Dispatcher.UIThread.Post(async () => { await CommandVideoOpen(); });
        }
    }

    internal void ComboBoxSubtitleFormatPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var props = e.GetCurrentPoint(null).Properties;
        var isMacCtrlLeftClick = OperatingSystem.IsMacOS()
                                 && props.IsLeftButtonPressed
                                 && e.KeyModifiers.HasFlag(KeyModifiers.Control)
                                 && !e.KeyModifiers.HasFlag(KeyModifiers.Shift);

        if (props.IsRightButtonPressed || isMacCtrlLeftClick)
        {
            Dispatcher.UIThread.Post(async () => { await ShowSubtitleFormatPicker(); });
            e.Handled = true;
        }
    }

    internal void AudioVisualizerSelectRequested(object sender, ParagraphEventArgs e)
    {
        var s = Subtitles.FirstOrDefault(p => p.Id == e.Paragraph.Id);
        if (s != null)
        {
            SubtitleGrid.SelectedItem = s;
        }
    }

    internal void OnWaveformDoubleTapped(object sender, ParagraphEventArgs e)
    {
        var vp = GetVideoPlayerControl();
        if (!string.IsNullOrEmpty(_videoFileName) && vp != null)
        {
            vp.Position = e.Seconds;
            var p = Subtitles.FirstOrDefault(p =>
                Math.Abs(p.StartTime.TotalMilliseconds - e.Paragraph.StartTime.TotalMilliseconds) < 0.01);
            if (p != null)
            {
                SelectAndScrollToSubtitle(p);
            }
        }
    }

    internal void AudioVisualizerOnPrimarySingleClicked(object sender, ParagraphNullableEventArgs e)
    {
        var vp = GetVideoPlayerControl();
        if (vp == null || string.IsNullOrEmpty(_videoFileName) || AudioVisualizer == null)
        {
            return;
        }

        if (Enum.TryParse<WaveformSingleClickActionType>(Se.Settings.Waveform.SingleClickAction, out var action))
        {
            switch (action)
            {
                case WaveformSingleClickActionType.SetVideoPositionAndPauseAndSelectSubtitle:
                    vp.VideoPlayer.Pause();
                    vp.Position = e.Seconds;
                    if (e.Paragraph != null)
                    {
                        var p1 = Subtitles.FirstOrDefault(p => p.Id == e.Paragraph.Id);
                        if (p1 != null)
                        {
                            SelectAndScrollToSubtitle(p1);
                        }
                    }

                    break;
                case WaveformSingleClickActionType.SetVideopositionAndPauseAndSelectSubtitleAndCenter:
                    vp.VideoPlayer.Pause();
                    vp.Position = e.Seconds;
                    if (e.Paragraph != null)
                    {
                        var p2 = Subtitles.FirstOrDefault(p => p.Id == e.Paragraph.Id);
                        if (p2 != null)
                        {
                            SelectAndScrollToSubtitle(p2);
                            AudioVisualizer.CenterOnPosition(e.Seconds);
                        }
                    }

                    break;
                case WaveformSingleClickActionType.SetVideoPositionAndPause:
                    vp.VideoPlayer.Pause();
                    vp.Position = e.Seconds;
                    break;
                case WaveformSingleClickActionType.SetVideopositionAndPauseAndCenter:
                    vp.VideoPlayer.Pause();
                    vp.Position = e.Seconds;
                    if (e.Paragraph != null)
                    {
                        AudioVisualizer.CenterOnPosition(e.Seconds);
                    }

                    break;
                case WaveformSingleClickActionType.SetVideoposition:
                    vp.Position = e.Seconds;
                    break;
            }

            _updateAudioVisualizer = true;
        }
    }

    internal void AudioVisualizerOnPrimaryDoubleClicked(object sender, ParagraphNullableEventArgs e)
    {
        var vp = GetVideoPlayerControl();
        if (vp == null || string.IsNullOrEmpty(_videoFileName))
        {
            return;
        }

        if (Enum.TryParse<WaveformDoubleClickActionType>(Se.Settings.Waveform.DoubleClickAction, out var action))
        {
            switch (action)
            {
                case WaveformDoubleClickActionType.SelectSubtitle:
                    if (e.Paragraph != null)
                    {
                        var p = Subtitles.FirstOrDefault(p => Math.Abs(p.StartTime.TotalMilliseconds - e.Paragraph.StartTime.TotalMilliseconds) < 0.01);
                        if (p != null)
                        {
                            SelectAndScrollToSubtitle(p);
                        }
                    }

                    break;
                case WaveformDoubleClickActionType.Center:
                    if (e.Paragraph != null)
                    {
                        AudioVisualizerCenterOnPositionIfNeeded(e.Paragraph, e.Seconds);
                    }

                    break;
                case WaveformDoubleClickActionType.Pause:
                    vp.VideoPlayer.Pause();
                    break;
                case WaveformDoubleClickActionType.Play:
                    vp.VideoPlayer.Play();
                    break;
            }

            _updateAudioVisualizer = true;
        }
    }

    internal void AudioVisualizerSetStartAndOffsetTheRest(object sender, AudioVisualizer.PositionEventArgs e)
    {
        var s = SelectedSubtitle;
        if (s == null || LockTimeCodes)
        {
            return;
        }

        var videoPositionSeconds = e.PositionInSeconds;
        var index = Subtitles.IndexOf(s);
        if (index < 0 || index >= Subtitles.Count)
        {
            return;
        }

        var videoStartTime = TimeSpan.FromSeconds(videoPositionSeconds);
        var subtitleStartTime = s.StartTime;
        var difference = videoStartTime - subtitleStartTime;

        RunWithoutChangeDetection(() =>
        {
            for (var i = index; i < Subtitles.Count; i++)
            {
                var subtitle = Subtitles[i];
                subtitle.StartTime += difference;
            }

            _updateAudioVisualizer = true;
        });
    }
}
