using Avalonia.Controls;
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
using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes;
using Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream;
using Nikse.SubtitleEdit.Core.Forms;
using Nikse.SubtitleEdit.Core.Interfaces;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Core.VobSub;
using Nikse.SubtitleEdit.Features.Assa;
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
using Nikse.SubtitleEdit.Features.Files.FormatProperties.RosettaProperties;
using Nikse.SubtitleEdit.Features.Files.FormatProperties.TmpegEncXmlProperties;
using Nikse.SubtitleEdit.Features.Files.ImportImages;
using Nikse.SubtitleEdit.Features.Files.ImportPlainText;
using Nikse.SubtitleEdit.Features.Files.ManualChosenEncoding;
using Nikse.SubtitleEdit.Features.Files.RestoreAutoBackup;
using Nikse.SubtitleEdit.Features.Files.Statistics;
using Nikse.SubtitleEdit.Features.Help;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Features.Main.MainHelpers;
using Nikse.SubtitleEdit.Features.Ocr;
using Nikse.SubtitleEdit.Features.Options.Language;
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
using Nikse.SubtitleEdit.Features.Shared.PromptTextBox;
using Nikse.SubtitleEdit.Features.Shared.SetVideoOffset;
using Nikse.SubtitleEdit.Features.Shared.SourceView;
using Nikse.SubtitleEdit.Features.Shared.TextBoxUtils;
using Nikse.SubtitleEdit.Features.Shared.Undocked;
using Nikse.SubtitleEdit.Features.Shared.WaveformGuessTimeCodes;
using Nikse.SubtitleEdit.Features.Shared.WaveformSeekSilence;
using Nikse.SubtitleEdit.Features.SpellCheck;
using Nikse.SubtitleEdit.Features.SpellCheck.FindDoubleWords;
using Nikse.SubtitleEdit.Features.SpellCheck.GetDictionaries;
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
using Nikse.SubtitleEdit.Features.Tools.FixCommonErrors;
using Nikse.SubtitleEdit.Features.Tools.FixNetflixErrors;
using Nikse.SubtitleEdit.Features.Tools.JoinSubtitles;
using Nikse.SubtitleEdit.Features.Tools.MergeShortLines;
using Nikse.SubtitleEdit.Features.Tools.MergeSubtitlesWithSameText;
using Nikse.SubtitleEdit.Features.Tools.MergeSubtitlesWithSameTimeCodes;
using Nikse.SubtitleEdit.Features.Tools.RemoveTextForHearingImpaired;
using Nikse.SubtitleEdit.Features.Tools.SplitBreakLongLines;
using Nikse.SubtitleEdit.Features.Tools.SplitSubtitle;
using Nikse.SubtitleEdit.Features.Translate;
using Nikse.SubtitleEdit.Features.Video.BlankVideo;
using Nikse.SubtitleEdit.Features.Video.BurnIn;
using Nikse.SubtitleEdit.Features.Video.CutVideo;
using Nikse.SubtitleEdit.Features.Video.EmbeddedSubtitlesEdit;
using Nikse.SubtitleEdit.Features.Video.GoToVideoPosition;
using Nikse.SubtitleEdit.Features.Video.OpenFromUrl;
using Nikse.SubtitleEdit.Features.Video.ReEncodeVideo;
using Nikse.SubtitleEdit.Features.Video.ShotChanges;
using Nikse.SubtitleEdit.Features.Video.SpeechToText;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech;
using Nikse.SubtitleEdit.Features.Video.TransparentSubtitles;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Config.Language;
using Nikse.SubtitleEdit.Logic.Download;
using Nikse.SubtitleEdit.Logic.Initializers;
using Nikse.SubtitleEdit.Logic.Media;
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
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
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
    IApplyAssaStyles
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
    [ObservableProperty] private string _editTextLineLengths;

    [ObservableProperty] private string _editTextOriginal;
    [ObservableProperty] private string _editTextCharactersPerSecondOriginal;
    [ObservableProperty] private IBrush _editTextCharactersPerSecondBackgroundOriginal;
    [ObservableProperty] private string _editTextTotalLengthOriginal;
    [ObservableProperty] private IBrush _editTextTotalLengthBackgroundOriginal;
    [ObservableProperty] private string _editTextLineLengthsOriginal;

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
    [ObservableProperty] private bool _isMergeWithNextOrPreviousVisible;
    [ObservableProperty] private bool _showColumnOriginalText;
    [ObservableProperty] private bool _showColumnEndTime;
    [ObservableProperty] private bool _showColumnGap;
    [ObservableProperty] private bool _showColumnDuration;
    [ObservableProperty] private bool _showColumnActor;
    [ObservableProperty] private bool _showColumnStyle;
    [ObservableProperty] private bool _showColumnCps;
    [ObservableProperty] private bool _showColumnWpm;
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

    public DataGrid SubtitleGrid { get; set; }
    public Window? Window { get; set; }
    public Grid ContentGrid { get; set; }
    public MainView? MainView { get; set; }
    public TextBlock StatusTextLeftLabel { get; set; }
    public MenuItem MenuReopen { get; set; }
    public AudioVisualizer? AudioVisualizer { get; set; }

    VideoPlayerUndockedViewModel? _videoPlayerUndockedViewModel;
    AudioVisualizerUndockedViewModel? _audioVisualizerUndockedViewModel;
    FindViewModel? _findViewModel;
    ReplaceViewModel? _replaceViewModel;
    AdjustAllTimesViewModel? _adjustAllTimesViewModel;

    private static Color _errorColor = Se.Settings.General.ErrorColor.FromHexToColor();

    private bool _updateAudioVisualizer;
    private string? _subtitleFileName;
    private string? _subtitleFileNameOriginal;
    private bool _converted;
    private Subtitle _subtitle;
    private Subtitle _subtitleOriginal;
    private SubtitleFormat? _lastOpenSaveFormat;
    private string? _videoFileName;
    private AudioTrackInfo? _audioTrack;
    private string? _audioTrackLangauge;
    private CancellationTokenSource? _statusFadeCts;
    private int _changeSubtitleHash = -1;
    private int _changeSubtitleHashOriginal = -1;
    private bool _subtitleGridSelectionChangedSkip;
    private long _lastKeyPressedMs;
    private bool _loading;
    private bool _opening;
    private PointerEventArgs? _lastTextEditorPointerArgs;
    private List<int>? _visibleLayers;
    private DispatcherTimer _positionTimer = new();
    private DispatcherTimer _slowTimer = new();
    private CancellationTokenSource _videoOpenTokenSource;
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
    private readonly IFindService _findService;
    private readonly IColorService _colorService;
    private readonly IFontNameService _fontNameService;
    private readonly ISpellCheckManager _spellCheckManager;
    private readonly ICasingToggler _casingToggler;
    private readonly IPasteFromClipboardHelper _pasteFromClipboardHelper;

    private bool IsEmpty => Subtitles.Count == 0 || (Subtitles.Count == 1 && string.IsNullOrEmpty(Subtitles[0].Text));

    private bool IsEmptyOriginal => Subtitles.Count == 0 ||
                                    (Subtitles.Count == 1 && string.IsNullOrEmpty(Subtitles[0].OriginalText));

    public VideoPlayerControl? VideoPlayerControl { get; internal set; }
    public Menu Menu { get; internal set; }
    public Border Toolbar { get; internal set; }
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
        IFindService findService,
        IDictionaryInitializer dictionaryInitializer,
        ILanguageInitializer languageInitializer,
        IOcrInitializer ocrInitializer,
        IThemeInitializer themeInitializer,
        IColorService colorService,
        IFontNameService fontNameService,
        ISpellCheckManager spellCheckManager,
        ICasingToggler casingToggler,
        IPasteFromClipboardHelper pasteFromClipboardHelper)
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
        _findService = findService;
        _colorService = colorService;
        _fontNameService = fontNameService;
        _spellCheckManager = spellCheckManager;
        _casingToggler = casingToggler;
        _pasteFromClipboardHelper = pasteFromClipboardHelper;

        _loading = true;
        Configuration.DataDirectoryOverride = Se.DataFolder;
        EditText = string.Empty;
        EditTextCharactersPerSecond = string.Empty;
        EditTextCharactersPerSecondBackground = Brushes.Transparent;
        EditTextTotalLength = string.Empty;
        EditTextTotalLengthBackground = Brushes.Transparent;
        EditTextLineLengths = string.Empty;
        StatusTextLeftLabel = new TextBlock();
        SubtitleGrid = new DataGrid();
        EditTextBox = new TextBoxWrapper(new TextBox());
        ContentGrid = new Grid();
        MenuReopen = new MenuItem();
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
        SelectedSubtitleFormat = SubtitleFormats[0];
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
        ShowColumnEndTime = Se.Settings.General.ShowColumnEndTime;
        ShowColumnDuration = Se.Settings.General.ShowColumnDuration;
        ShowColumnGap = Se.Settings.General.ShowColumnGap;
        ShowColumnActor = Se.Settings.General.ShowColumnActor;
        ShowColumnStyle = Se.Settings.General.ShowColumnStyle;
        ShowColumnCps = Se.Settings.General.ShowColumnCps;
        ShowColumnWpm = Se.Settings.General.ShowColumnWpm;
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
        EditTextLineLengthsOriginal = string.Empty;
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
        SurroundWith1Text = string.Format(Se.Language.Options.Shortcuts.SurroundWithXY, Se.Settings.Surround1Left, Se.Settings.Surround1Right);
        SurroundWith2Text = string.Format(Se.Language.Options.Shortcuts.SurroundWithXY, Se.Settings.Surround2Left, Se.Settings.Surround2Right);
        SurroundWith3Text = string.Format(Se.Language.Options.Shortcuts.SurroundWithXY, Se.Settings.Surround3Left, Se.Settings.Surround3Right);

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
        InitializeFfmpeg();
        InitializeLibMpv();
        LoadShortcuts();

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
        Configuration.Settings.General.MinimumMillisecondsBetweenLines = Se.Settings.General.MinimumMillisecondsBetweenLines;
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
            if (string.IsNullOrEmpty(Se.Settings.General.LibMpvPath) || File.Exists(newFileName))
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
        Se.Settings.General.LayoutNumber = InitLayout.MakeLayout(MainView!, this, layoutNumber);
        SelectAndScrollToRow(Math.Max(0, idx));
    }

    [RelayCommand]
    private void Play()
    {
        var vp = GetVideoPlayerControl();
        if (vp == null)
        {
            return;
        }

        vp.VideoPlayerInstance.Play();
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
        vp.VideoPlayerInstance.Play();
    }

    [RelayCommand]
    private void Pause()
    {
        var vp = GetVideoPlayerControl();
        if (vp == null)
        {
            return;
        }

        vp.VideoPlayerInstance.Pause();
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
        }
    }

    public void ApplyAssaStyles(AssaStylesViewModel result)
    {
        _subtitle.Header = result.Header;
        var styles = AdvancedSubStationAlpha.GetStylesFromHeader(_subtitle.Header);
        var first = styles.FirstOrDefault() ?? "Default";

        foreach (var s in Subtitles)
        {
            if (string.IsNullOrEmpty(s.Style) || !styles.Contains(s.Style))
            {
                s.Style = first;
            }
        }
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
                StartTime = TimeSpan.FromSeconds(0),
                EndTime = TimeSpan.FromSeconds(2000),
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
                newP.StartTime = lastParagraph!.StartTime;
                newP.EndTime = lastParagraph.EndTime;
                var insertIndex = lastParagraph == null ? 0 : Subtitles.IndexOf(lastParagraph) + 1;
                Subtitles.Insert(insertIndex, new SubtitleLineViewModel(p, SelectedSubtitleFormat));
            }
        }

        Renumber();
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
        for (var index = 0; index < result.ResultSubtitle.Paragraphs.Count; index++)
        {
            var p = result.ResultSubtitle.Paragraphs[index];
            Subtitles.Insert(index, new SubtitleLineViewModel(p, SelectedSubtitleFormat));
        }

        Renumber();
        _updateAudioVisualizer = true;
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
    }

    private static string RemovePositionTags(string text)
    {
        string result = Regex.Replace(text, @"\\pos\(\d+,\d+\)", string.Empty).Replace("{}", string.Empty);
        return result;
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

        var json = System.Text.Json.JsonSerializer.Serialize(Se.Language, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
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
        var newWindow = new AboutWindow(new AboutViewModel());
        await newWindow.ShowDialog(Window!);
        _shortcutManager.ClearKeys();
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

        _mpvReloader.Reset();

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
            _adjustAllTimesViewModel.Window?.Close();
            _adjustAllTimesViewModel = null;
        }

        var vp = GetVideoPlayerControl();
        if (vp != null)
        {
            vp.IsSmpteTimingEnabled = IsSmpteTimingEnabled;
        }

        if (_mpvReloader != null)
        {
            _mpvReloader.SmpteMode = IsSmpteTimingEnabled;
        }

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
            await SubtitleOpen(recentFile.SubtitleFileName, recentFile.VideoFileName, recentFile.SelectedLine);

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

        if (vp != null && vp.VideoPlayerInstance is LibMpvDynamicPlayer mpv)
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
        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private async Task CommandFileSave()
    {
        var saved = await SaveSubtitle();
        var savedOriginal = false;

        if (ShowColumnOriginalText)
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

            var result = await ShowDialogAsync<DCinemaSmptePropertiesWindow, DCinemaSmptePropertiesViewModel>(vm => 
            {
                vm.Initialize(format);
            });

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

        var result = await ShowDialogAsync<ImportPlainTextWindow, ImportPlainTextViewModel>(vm => vm.SetCurrentSubtitle(_subtitle));
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
        }
    }

    [RelayCommand]
    private async Task ImportImageSubtitleForOcr()
    {
        if (Window == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickOpenFile(Window!, Se.Language.General.OpenImageBasedSubtitle, Se.Language.General.ImagedBasedSubtitles, "*.sup;*.sub;*.ts;*.xml",
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

        var fileName = await _fileHelper.PickOpenFile(Window!, Se.Language.General.OpenImageBasedSubtitle, Se.Language.General.ImagedBasedSubtitles, "*.sup;*.sub;*.ts;*.xml",
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
        else // ClassicTurbo or any other value, cycle back to Classic
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
                var tempMaxEntTime = TimeSpan.FromMilliseconds(nextSubtitle.StartTime.TotalMilliseconds - Se.Settings.General.MinimumMillisecondsBetweenLines);
                if (tempMaxEntTime < maxEndTime)
                {
                    maxEndTime = tempMaxEntTime;
                }
            }

            var proposedEndTime = selectedLine.StartTime + optimalDuration;
            if (proposedEndTime.TotalMilliseconds < Se.Settings.General.SubtitleMaximumDisplayMilliseconds)
            {
                proposedEndTime = TimeSpan.FromMilliseconds(selectedLine.StartTime.TotalMilliseconds + Se.Settings.General.SubtitleMaximumDisplayMilliseconds);
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
            await ShowDialogAsync<PickLayerFilterWindow, PickLayerFilterViewModel>(vm => { vm.Initialize(Subtitles.ToList(), _visibleLayers); });

        if (!result.OkPressed)
        {
            return;
        }

        _visibleLayers = result.SelectedLayers;
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
        var total = Subtitles.Count;
        var overWrite = result.ModeOverwrite; // either overwrite or insert mode (shift texts down)
        for (var i = 0; i < subtitle.Paragraphs.Count && idx < Subtitles.Count; i++)
        {
            if (overWrite == false)
            {
                for (int j = total - 1; j > idx; j--)
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

        if (string.IsNullOrEmpty(_videoFileName))
        {
            await CommandVideoOpen();
        }

        if (string.IsNullOrEmpty(_videoFileName))
        {
            return;
        }

        var result = await ShowDialogAsync<EmbeddedSubtitlesEditWindow, EmbeddedSubtitlesEditViewModel>(vm =>
        {
            vm.Initialize(_videoFileName, GetUpdateSubtitle(), SelectedSubtitleFormat, _mediaInfo);
        });

        if (!result.OkPressed)
        {
            return;
        }
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
            result.AdjustDuration(Subtitles);
            _updateAudioVisualizer = true;
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
            var idx = SelectedSubtitleIndex;
            Subtitles.Clear();
            Subtitles.AddRange(result.AllSubtitlesFixed);
            SelectAndScrollToRow(idx ?? 0);
            _updateAudioVisualizer = true;
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

        var result =
            await ShowDialogAsync<ChangeFormattingWindow, ChangeFormattingViewModel>(vm => { vm.Initialize(Subtitles.ToList(), SelectedSubtitleFormat); });

        if (!result.OkPressed)
        {
            return;
        }

        var idx = SelectedSubtitleIndex ?? 0;
        if (result.OkPressed)
        {
            SetSubtitles(result.FixedSubtitle);
            SelectAndScrollToRow(idx);
        }
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

        var viewModel = await ShowDialogAsync<FixCommonErrorsWindow, FixCommonErrorsViewModel>(vm => { vm.Initialize(GetUpdateSubtitle(), SelectedSubtitleFormat); });

        if (viewModel.OkPressed)
        {
            SetSubtitles(viewModel.FixedSubtitle);
            SelectAndScrollToRow(0);
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

        var viewModel = await ShowDialogAsync<FixNetflixErrorsWindow, FixNetflixErrorsViewModel>(vm => { vm.Initialize(GetUpdateSubtitle(), _videoFileName ?? string.Empty); });

        if (viewModel.OkPressed)
        {
            SetSubtitles(viewModel.FixedSubtitle);
            SelectAndScrollToRow(0);
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

        var result = await _windowService
            .ShowDialogAsync<SplitBreakLongLinesWindow, SplitBreakLongLinesViewModel>(
                Window!, vm => { vm.Initialize(Subtitles.ToList()); });

        if (result.OkPressed && result.AllSubtitlesFixed.Count > 0)
        {
            Subtitles.Clear();
            Subtitles.AddRange(result.AllSubtitlesFixed);
            SelectAndScrollToRow(0);
            _updateAudioVisualizer = true;
            _mpvReloader.Reset();
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

        var result = await _windowService
            .ShowDialogAsync<MergeShortLinesWindow, MergeShortLinesViewModel>(
                Window!, vm => { vm.Initialize(Subtitles.ToList(), AudioVisualizer?.ShotChanges ?? new List<double>()); });

        if (result.OkPressed)
        {
            Subtitles.Clear();
            Subtitles.AddRange(result.AllSubtitlesFixed);
            SelectAndScrollToRow(0);
            _updateAudioVisualizer = true;
            _mpvReloader.Reset();
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

        var result = await _windowService
            .ShowDialogAsync<RemoveTextForHearingImpairedWindow, RemoveTextForHearingImpairedViewModel>(
                Window!, vm => { vm.Initialize(GetUpdateSubtitle()); });

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

        if (vp.VideoPlayerInstance is LibMpvDynamicPlayer mpv && parameter is AudioTrackInfo audioTrack)
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
            var videoFileName = vp.VideoPlayerInstance.FileName;
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

            _windowService.ShowWindow<VideoPlayerUndockedWindow, VideoPlayerUndockedViewModel>(Window, (window, vm) =>
            {
                _videoPlayerUndockedViewModel = vm;
                vm.Initialize(_videoFileName ?? string.Empty, position, volume, this);
            });

            _windowService.ShowWindow<AudioVisualizerUndockedWindow, AudioVisualizerUndockedViewModel>(Window, (window, vm) =>
            {
                _audioVisualizerUndockedViewModel = vm;
                vm.Initialize(AudioVisualizer, this);
                ReloadAudioVisualizer();
            });

            InitLayout.MakeLayout12KeepVideo(MainView!, this);
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
            Se.Settings.General.MinimumMillisecondsBetweenLines = p.MinimumMillisecondsBetweenLines;
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
            await ShowDialogAsync<AudioToTextWhisperWindow, AudioToTextWhisperViewModel>(vm => { vm.Initialize(_videoFileName, _audioTrack?.FfIndex ?? -1); });

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
            ShowStatus(string.Format(Se.Language.Main.TranscriptionCompletedWithXLines, result.TranscribedSubtitle.Paragraphs.Count));
        }
    }

    [RelayCommand]
    private async Task SpeechToTextSelectedLines()
    {
        await SpeechToTextSelectedLines(false);
    }

    [RelayCommand]
    private async Task SpeechToTextSelectedLinesPromptForLangauge()
    {
        await SpeechToTextSelectedLines(true);
    }

    [RelayCommand]
    private async Task SpeechToTextSelectedLinesPromptForLangaugeFirstTime()
    {
        var language = LanguageAutoDetect.AutoDetectGoogleLanguage(GetUpdateSubtitle());
        await SpeechToTextSelectedLines(_audioTrackLangauge == language);
        _audioTrackLangauge = language;
    }

    private async Task<bool> SpeechToTextSelectedLines(bool promptEngineAndLanguage)
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

        var resultGetAudioClips = await ShowDialogAsync<GetAudioClipsWindow, GetAudioClipsViewModel>(vm => { vm.Initialize(_videoFileName ?? string.Empty, selectedItems); });

        if (!resultGetAudioClips.OkPressed || resultGetAudioClips.AudioClips.Count == 0)
        {
            return false;
        }

        var resultSpeechToText = await ShowDialogAsync<AudioToTextWhisperWindow, AudioToTextWhisperViewModel>(vm =>
        {
            vm.InitializeBatch(resultGetAudioClips.AudioClips, _audioTrack?.FfIndex ?? -1, promptEngineAndLanguage);
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

    private bool PlayerSelectedLines(bool loop)
    {
        var selectedItems = SubtitleGrid.SelectedItems.Cast<SubtitleLineViewModel>().OrderBy(p => p.StartTime).ToList();
        var vp = GetVideoPlayerControl();
        if (Window == null || selectedItems.Count == 0 || vp == null)
        {
            return false;
        }

        vp.VideoPlayerInstance.Pause();
        var p = selectedItems.First();
        vp.Position = p.StartTime.TotalSeconds;
        _playSelectionItem = new PlaySelectionItem(selectedItems, p.EndTime, loop);
        vp.VideoPlayerInstance.Play();

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
        if (!isYouTubeDlInstalled)
        {
            var download = await MessageBox.Show(Window, Se.Language.General.Information,
                Se.Language.Main.YoutubeDlNotInstalledDownloadNow,
                MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (download == MessageBoxResult.Yes)
            {
                var downloadResult = await ShowDialogAsync<DownloadYtDlpWindow, DownloadYtDlpViewModel>();
                isYouTubeDlInstalled = File.Exists(YtDlpDownloadService.GetFullFileName());
                if (isYouTubeDlInstalled)
                {
                    ShowStatus(Se.Language.Main.YoutubeDlDownloadedSuccessfully);
                }
                else
                {
                    return;
                }
            }
            else
            {
                return;
            }
        }

        var result = await ShowDialogAsync<OpenFromUrlWindow, OpenFromUrlViewModel>();

        if (result.OkPressed)
        {
            var videoFileName = result.Url.Trim();
            if (!string.IsNullOrEmpty(videoFileName))
            {
                await VideoOpenFile(videoFileName);
            }
        }
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

        if (_mpvReloader != null)
        {
            _mpvReloader.SmpteMode = IsSmpteTimingEnabled;
        }
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
            vm.Initialize(GetUpdateSubtitle(), _videoFileName ?? string.Empty, AudioVisualizer?.WavePeaks,
                Path.GetTempPath());
        });
    }

    [RelayCommand]
    private async Task ShowVideoTransparentSubtitles()
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
                    line.EndTime = TimeSpan.FromMilliseconds(next.StartTime.TotalMilliseconds - Se.Settings.General.MinimumMillisecondsBetweenLines);
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
                    line.EndTime = TimeSpan.FromMilliseconds(next.StartTime.TotalMilliseconds - Se.Settings.General.MinimumMillisecondsBetweenLines);
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

        var result = await ShowDialogAsync<ChangeFrameRateWindow, ChangeFrameRateViewModel>();
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
        _subtitleFileName = string.Empty;
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

        //for (var i = 0; i < Subtitles.Count; i++)
        //{
        //    if (result.Rows.Count <= i)
        //    {
        //        break;
        //    }

        //    Subtitles[i].OriginalText = Subtitles[i].Text;
        //    Subtitles[i].Text = result.Rows[i].TranslatedText;
        //}

        _subtitleFileNameOriginal = _subtitleFileName;
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

        var viewModel = await _windowService
            .ShowDialogAsync<BeautifyTimeCodesWindow, BeautifyTimeCodesViewModel>(Window!, vm => { vm.Initialize(Subtitles.ToList(), AudioVisualizer, _videoFileName); });

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

        var viewModel = await _windowService
            .ShowDialogAsync<SettingsWindow, SettingsViewModel>(Window!, vm => { vm.Initialize(this); });

        if (!viewModel.OkPressed)
        {
            _shortcutManager.ClearKeys();
            return;
        }

        Se.Settings.General.WindowPositions = Se.Settings.General.WindowPositions.OrderBy(p => p.WindowName).ToList();
        var newSettingsSerialized = JsonSerializer.Serialize(Se.Settings);

        if (oldSettngsSerialized != newSettingsSerialized)
        {
            var firstSelectedIndex = SubtitleGrid.SelectedIndex;
            ApplySettings();
            SelectAndScrollToRow(firstSelectedIndex);
        }
    }

    public void ApplySettings()
    {
        UiUtil.SetFontName(Se.Settings.Appearance.FontName);
        UiTheme.SetCurrentTheme();

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

        LockTimeCodes = Se.Settings.General.LockTimeCodes;
        IsWaveformToolbarVisible = Se.Settings.Waveform.ShowToolbar;

        if (AudioVisualizer != null)
        {
            AudioVisualizer.DrawGridLines = Se.Settings.Waveform.DrawGridLines;
            AudioVisualizer.WaveformColor = Se.Settings.Waveform.WaveformColor.FromHexToColor();
            AudioVisualizer.WaveformBackgroundColor = Se.Settings.Waveform.WaveformBackgroundColor.FromHexToColor();
            AudioVisualizer.WaveformSelectedColor = Se.Settings.Waveform.WaveformSelectedColor.FromHexToColor();
            AudioVisualizer.WaveformCursorColor = Se.Settings.Waveform.WaveformCursorColor.FromHexToColor();
            AudioVisualizer.WaveformFancyHighColor = Se.Settings.Waveform.WaveformFancyHighColor.FromHexToColor();
            AudioVisualizer.ParagraphBackground = Se.Settings.Waveform.ParagraphBackground.FromHexToColor();
            AudioVisualizer.ParagraphSelectedBackground = Se.Settings.Waveform.ParagraphSelectedBackground.FromHexToColor();
            AudioVisualizer.InvertMouseWheel = Se.Settings.Waveform.InvertMouseWheel;
            AudioVisualizer.UpdateTheme();
            AudioVisualizer.IsReadOnly = LockTimeCodes;
            AudioVisualizer.WaveformDrawStyle = InitWaveform.GetWaveformDrawStyle(Se.Settings.Waveform.WaveformDrawStyle);
            AudioVisualizer.MinGapSeconds = Se.Settings.General.MinimumMillisecondsBetweenLines / 1000.0;
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
        if (vp != null && vp.VideoPlayerInstance is LibMpvDynamicPlayer mpv)
        {
            _mpvReloader.Reset();
            _mpvReloader.RefreshMpv(mpv, GetUpdateSubtitle(), SelectedSubtitleFormat);
        }

        if (Se.Settings.Appearance.RightToLeft)
        {
            Se.Settings.Appearance.RightToLeft = !Se.Settings.Appearance.RightToLeft;
            RightToLeftToggle();
        }

        UpdateVideoOffsetStatus();
        SetLibSeSettings();

        var selectedSubtitleFormatName = SelectedSubtitleFormat.Name;
        SubtitleFormats.Clear();
        foreach (var format in SubtitleFormatHelper.GetSubtitleFormatsWithFavoritesAtTop())
        {
            SubtitleFormats.Add(format);
        }

        SetSubtitleFormat(SubtitleFormats.FirstOrDefault(p => p.Name == selectedSubtitleFormatName) ?? SubtitleFormats.First());

        SetupLiveSpellCheck();
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

        ShowStatus(string.Format(Se.Language.General.XNotFound, _findService.SearchText));
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
            var jsonFileName = viewModel.SelectedLanguage.FileName;
            var json = await File.ReadAllTextAsync(jsonFileName, Encoding.UTF8);
            var language = System.Text.Json.JsonSerializer.Deserialize<SeLanguage>(json,
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                });

            Se.Language = language ?? new SeLanguage();

            // reload current layout
            InitMenu.Make(this);
            SetLayout(Se.Settings.General.LayoutNumber);
        }
    }

    [RelayCommand]
    private async Task OpenDataFolder()
    {
        await _folderHelper.OpenFolder(Window!, Se.DataFolder);
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
        _undoRedoManager.StopChangeDetection();
        InsertBeforeSelectedItem();
        _undoRedoManager.StartChangeDetection();
    }

    [RelayCommand]
    private void InsertLineAfter()
    {
        _undoRedoManager.StopChangeDetection();
        InsertAfterSelectedItem();
        _undoRedoManager.StartChangeDetection();
    }

    [RelayCommand]
    private void MergeWithLineBefore()
    {
        _undoRedoManager.StopChangeDetection();
        MergeLineBefore();
        _undoRedoManager.StartChangeDetection();
    }

    [RelayCommand]
    private void MergeWithLineAfter()
    {
        _undoRedoManager.StopChangeDetection();
        MergeLineAfter();
        _undoRedoManager.StartChangeDetection();
    }

    [RelayCommand]
    private void MergeWithLineBeforeKeepBreaks()
    {
        _undoRedoManager.StopChangeDetection();
        MergeLineBeforeKeepBreaks();
        _undoRedoManager.StartChangeDetection();
    }

    [RelayCommand]
    private void MergeWithLineAfterKeepBreaks()
    {
        _undoRedoManager.StopChangeDetection();
        MergeLineAfterKeepBreaks();
        _undoRedoManager.StartChangeDetection();
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
            _colorService.RemoveColorTags(selectedItems);
        }
        else
        {
            _colorService.RemoveColorTags(selectedItems);
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

        _colorService.RemoveColorTags(selectedItems);
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

        _colorService.RemoveColorTags(selectedItems);

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
        var viewModel = await _windowService
            .ShowDialogAsync<RestoreAutoBackupWindow, RestoreAutoBackupViewModel>(Window!);

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
            _replaceViewModel.Window?.Close();
            _replaceViewModel = null;
        }

        if (_findViewModel != null && _findViewModel.Window != null && _findViewModel.Window.IsVisible)
        {
            _findViewModel.Window.Activate();
            return;
        }

        var subs = Subtitles.Select(p => p.Text).ToList();
        var result = _windowService.ShowWindow<FindWindow, FindViewModel>(Window!, (window, vm) =>
        {
            window.Topmost = true;
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
        });

        _shortcutManager.ClearKeys();
    }

    public void RequestFindData()
    {
        var selectedSubtitle = SelectedSubtitle;
        if (Subtitles.Count == 0 || selectedSubtitle == null || _findViewModel == null)
        {
            return;
        }

        var currentLineIndex = Subtitles.IndexOf(selectedSubtitle);
        var currentCharIndex = EditTextBox.CaretIndex;
        var subs = Subtitles.Select(p => p.Text).ToList();
        _findViewModel.InitializeFindData(_findService, subs, _findService.SearchText, this);
    }

    public void HandleFindResult(FindViewModel result)
    {
        var selectedSubtitle = SelectedSubtitle;
        if (Subtitles.Count == 0 || selectedSubtitle == null)
        {
            return;
        }

        if ((result.FindNextPressed || result.FindPreviousPressed) && !string.IsNullOrEmpty(result.SearchText))
        {
            var findMode = FindService.FindMode.CaseSensitive;
            if (result.FindTypeCanseInsensitive)
            {
                findMode = FindService.FindMode.CaseInsensitive;
            }
            else if (result.FindTypeRegularExpression)
            {
                findMode = FindService.FindMode.RegularExpression;
            }

            var currentLineIndex = Subtitles.IndexOf(selectedSubtitle);
            var currentCharIndex = EditTextBox.CaretIndex;
            var subs = Subtitles.Select(p => p.Text).ToList();
            _findService.Initialize(subs, SelectedSubtitleIndex ?? 0, result.WholeWord, findMode);

            var idx = -1;
            if (result.FindNextPressed)
            {
                idx = _findService.FindNext(result.SearchText, subs, currentLineIndex, currentCharIndex + 1);
            }
            else
            {
                idx = _findService.FindPrevious(result.SearchText, subs, currentLineIndex, currentCharIndex - 1);
            }

            if (idx < 0)
            {
                ShowStatus(string.Format(Se.Language.General.XNotFound, _findService.SearchText));
                return;
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

                // wait for text box to update
                Task.Delay(75);

                if (EditTextBox.Text == string.Empty)
                {
                    EditTextBox.Text = Subtitles[idx].Text;
                }

                EditTextBox.CaretIndex = _findService.CurrentTextIndex;
                EditTextBox.SelectionStart = _findService.CurrentTextIndex;
                EditTextBox.SelectionEnd = _findService.CurrentTextIndex + _findService.CurrentTextFound.Length;
            });
        }
    }

    [RelayCommand]
    private void FindNext()
    {
        var selectedSubtitle = SelectedSubtitle;
        if (Subtitles.Count == 0 || selectedSubtitle == null)
        {
            return;
        }

        var subs = Subtitles.Select(p => p.Text).ToList();
        var currentLineIndex = Subtitles.IndexOf(selectedSubtitle);
        var currentCharIndex = EditTextBox.CaretIndex;
        var idx = _findService.FindNext(_findService.SearchText, subs, currentLineIndex, currentCharIndex + 1);

        if (idx < 0)
        {
            ShowStatus(string.Format(Se.Language.General.XNotFound, _findService.SearchText));
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            SubtitleGrid.SelectedIndex = idx;
            SubtitleGrid.ScrollIntoView(SubtitleGrid.SelectedItem, null);

            ShowStatus(string.Format(Se.Language.General.FoundXInLineYZ, _findService.CurrentTextFound, _findService.CurrentLineNumber + 1, _findService.CurrentTextIndex + 1));

            // wait for text box to update
            Task.Delay(50);

            EditTextBox.CaretIndex = _findService.CurrentTextIndex;
            EditTextBox.SelectionStart = _findService.CurrentTextIndex;
            EditTextBox.SelectionEnd = _findService.CurrentTextIndex + _findService.CurrentTextFound.Length;
        });


        _shortcutManager.ClearKeys();
    }

    [RelayCommand]
    private void FindPrevious()
    {
        var selectedSubtitle = SelectedSubtitle;
        if (Subtitles.Count == 0 || selectedSubtitle == null)
        {
            return;
        }

        var subs = Subtitles.Select(p => p.Text).ToList();
        var currentLineIndex = Subtitles.IndexOf(selectedSubtitle);
        var currentCharIndex = EditTextBox.CaretIndex;
        var idx = _findService.FindPrevious(_findService.SearchText, subs, currentLineIndex, currentCharIndex - 1);

        if (idx < 0)
        {
            ShowStatus(string.Format(Se.Language.General.XNotFound, _findService.SearchText));
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            SubtitleGrid.SelectedIndex = idx;
            SubtitleGrid.ScrollIntoView(SubtitleGrid.SelectedItem, null);

            ShowStatus(string.Format(Se.Language.General.FoundXInLineYZ, _findService.CurrentTextFound, _findService.CurrentLineNumber + 1, _findService.CurrentTextIndex + 1));

            // wait for text box to update
            Task.Delay(50);

            EditTextBox.CaretIndex = _findService.CurrentTextIndex;
            EditTextBox.SelectionStart = _findService.CurrentTextIndex;
            EditTextBox.SelectionEnd = _findService.CurrentTextIndex + _findService.CurrentTextFound.Length;
        });
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
            _findViewModel.Window?.Close();
            _findViewModel = null;
        }

        if (_replaceViewModel != null && _replaceViewModel.Window != null && _replaceViewModel.Window.IsVisible)
        {
            _replaceViewModel.Window.Activate();
            return;
        }

        var subs = Subtitles.Select(p => p.Text).ToList();
        var result = _windowService.ShowWindow<ReplaceWindow, ReplaceViewModel>(Window, (window, vm) =>
        {
            window.Topmost = true;
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
        });

        _shortcutManager.ClearKeys();
    }

    public void HandleReplaceResult(ReplaceViewModel result)
    {
        var selectedSubtitle = SelectedSubtitle;
        if (Subtitles.Count == 0 || selectedSubtitle == null)
        {
            return;
        }

        if ((result.FindNextPressed || result.ReplacePressed || result.ReplaceAllPressed) && !string.IsNullOrEmpty(result.SearchText))
        {
            var findMode = FindService.FindMode.CaseSensitive;
            if (result.FindTypeCanseInsensitive)
            {
                findMode = FindService.FindMode.CaseInsensitive;
            }
            else if (result.FindTypeRegularExpression)
            {
                findMode = FindService.FindMode.RegularExpression;
            }

            var currentLineIndex = Subtitles.IndexOf(selectedSubtitle);
            var currentCharIndex = EditTextBox.CaretIndex;
            var subs = Subtitles.Select(p => p.Text).ToList();
            _findService.Initialize(subs, SelectedSubtitleIndex ?? 0, result.WholeWord, findMode);

            var idx = -1;
            if (result.FindNextPressed)
            {
                idx = _findService.FindNext(result.SearchText, subs, currentLineIndex, currentCharIndex + 1);
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
                }

                idx = _findService.FindNext(result.SearchText, subs, currentLineIndex, currentCharIndex + 1);
            }

            if (idx < 0)
            {
                ShowStatus(string.Format(Se.Language.General.XNotFound, _findService.SearchText));
                return;
            }

            Dispatcher.UIThread.Post(() =>
            {
                SubtitleGrid.SelectedIndex = idx;
                SubtitleGrid.ScrollIntoView(SubtitleGrid.SelectedItem, null);

                ShowStatus(string.Format(Se.Language.General.FoundXInLineYZ, _findService.CurrentTextFound, _findService.CurrentLineNumber + 1, _findService.CurrentTextIndex + 1));

                // wait for text box to update
                Task.Delay(50);

                EditTextBox.CaretIndex = _findService.CurrentTextIndex;
                EditTextBox.SelectionStart = _findService.CurrentTextIndex;
                EditTextBox.SelectionEnd = _findService.CurrentTextIndex + _findService.CurrentTextFound.Length;
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

        vp.VideoPlayerInstance.Pause();
        var currentIndex = Subtitles.IndexOf(selectedItems.First());
        if (currentIndex <= 0)
        {
            return;
        }

        var p = Subtitles[currentIndex - 1];
        SubtitleGrid.SelectedItem = p;
        vp.Position = p.StartTime.TotalSeconds;
        _playSelectionItem = new PlaySelectionItem(new List<SubtitleLineViewModel> { p }, p.EndTime, true);
        vp.VideoPlayerInstance.Play();
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

        vp.VideoPlayerInstance.Pause();
        var currentIndex = Subtitles.IndexOf(selectedItems.First());
        if (currentIndex >= Subtitles.Count - 1)
        {
            return;
        }

        var p = Subtitles[currentIndex + 1];
        SubtitleGrid.SelectedItem = p;
        vp.Position = p.StartTime.TotalSeconds;
        _playSelectionItem = new PlaySelectionItem(new List<SubtitleLineViewModel> { p }, p.EndTime, true);
        vp.VideoPlayerInstance.Play();
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

        vp.VideoPlayerInstance.Stop();
        vp.VideoPlayerInstance.Play();
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

    [RelayCommand]
    private void InsertSubtitleAtVideoPositionSetEndAtKeyUp()
    {
        if (_setEndAtKeyUpLine != null)
        {
            return;
        }

        var vp = GetVideoPlayerControl();
        if (vp == null || !vp.VideoPlayerInstance.IsPlaying)
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
                                                                 Se.Settings.General.MinimumMillisecondsBetweenLines);
            }
        }

        _setEndAtKeyUpLine = newParagraph;
        SelectAndScrollToSubtitle(newParagraph);
        Renumber();
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

        control.VideoPlayerInstance.Pause();
        var position = control.Position;
        var volume = control.Volume;
        var parent = (Control)control.Parent!;

        _fullScreenVideoPlayerControl = InitVideoPlayer.MakeVideoPlayer();
        _fullScreenVideoPlayerControl.IsFullScreen = true;
        var fullScreenWindow = new FullScreenVideoWindow(_fullScreenVideoPlayerControl, _videoFileName, _subtitleFileName ?? string.Empty, position, volume, () =>
        {
            control!.Position = _fullScreenVideoPlayerControl.Position;
            control!.Volume = _fullScreenVideoPlayerControl.Volume;
            _fullScreenVideoPlayerControl = null;
        });
        fullScreenWindow.Show(Window!);
        _shortcutManager.ClearKeys();

        var vp = GetVideoPlayerControl();
        if (vp != null && vp.VideoPlayerInstance is LibMpvDynamicPlayer mpv)
        {
            _mpvReloader.Reset();
            _mpvReloader.RefreshMpv(mpv, GetUpdateSubtitle(), SelectedSubtitleFormat);
        }
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
        var isWebVtt = SelectedSubtitleFormat is WebVTT;
        if (selectionLength == 0 || selectionLength == tb.Text.Length)
        {
            tb.Text = _colorService.SetColorTag(tb.Text, color, isAssa, isWebVtt, GetUpdateSubtitle());
        }
        else
        {
            var selectedText = tb.Text.Substring(selectionStart, selectionLength);
            selectedText = _colorService.SetColorTag(selectedText, color, isAssa, isWebVtt, GetUpdateSubtitle());

            if (isAssa) // close color tag (display normal style color)
            {
                var closeTag = "{\\c&HFFFFFF&}"; // white color
                var styleName = SelectedSubtitle?.Style;
                if (_subtitle != null && _subtitle.Header != null && styleName != null)
                {
                    var style = AdvancedSubStationAlpha.GetSsaStyle(styleName, _subtitle.Header);
                    var endColor =
                        _colorService.SetColorTag("x", style.Primary.ToAvaloniaColor(), true, false, _subtitle);
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
        _undoRedoManager.StopChangeDetection();
        var idx = _insertService.InsertInCorrectPosition(Subtitles, newParagraph);
        var next = Subtitles.GetOrNull(idx + 1);
        if (next != null)
        {
            if (next.StartTime.TotalMilliseconds < endMs && next.StartTime.TotalMilliseconds > newParagraph.StartTime.TotalMilliseconds + 200)
            {
                newParagraph.EndTime = TimeSpan.FromMilliseconds(next.StartTime.TotalMilliseconds -
                                                                 Se.Settings.General.MinimumMillisecondsBetweenLines);
            }
        }

        _undoRedoManager.StartChangeDetection();

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
        _undoRedoManager.StopChangeDetection();
        var idx = _insertService.InsertInCorrectPosition(Subtitles, newParagraph);
        var next = Subtitles.GetOrNull(idx + 1);
        if (next != null)
        {
            if (next.StartTime.TotalMilliseconds < endMs && next.StartTime.TotalMilliseconds > newParagraph.StartTime.TotalMilliseconds + 200)
            {
                newParagraph.EndTime = TimeSpan.FromMilliseconds(next.StartTime.TotalMilliseconds -
                                                                 Se.Settings.General.MinimumMillisecondsBetweenLines);
            }
        }

        _undoRedoManager.StartChangeDetection();

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

        _undoRedoManager.StopChangeDetection();
        for (var i = index; i < Subtitles.Count; i++)
        {
            var subtitle = Subtitles[i];
            subtitle.StartTime += difference;
        }

        _updateAudioVisualizer = true;
        _undoRedoManager.StartChangeDetection();
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

        _undoRedoManager.StopChangeDetection();
        for (var i = index; i < Subtitles.Count; i++)
        {
            var subtitle = Subtitles[i];
            subtitle.StartTime += difference;
        }

        _updateAudioVisualizer = true;
        _undoRedoManager.StartChangeDetection();
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
        var gap = Se.Settings.General.MinimumMillisecondsBetweenLines / 1000.0;
        if (videoPositionSeconds >= s.EndTime.TotalSeconds - gap)
        {
            return;
        }

        s.SetStartTimeOnly(TimeSpan.FromSeconds(videoPositionSeconds));
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
        var gap = Se.Settings.General.MinimumMillisecondsBetweenLines / 1000.0;
        if (videoPositionSeconds < s.StartTime.TotalSeconds + gap)
        {
            return;
        }

        s.EndTime = TimeSpan.FromSeconds(videoPositionSeconds);
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
        var gap = Se.Settings.General.MinimumMillisecondsBetweenLines / 1000.0;
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
        var gapMs = Se.Settings.General.MinimumMillisecondsBetweenLines;
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
        var gapMs = Se.Settings.General.MinimumMillisecondsBetweenLines;
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
        var gapMs = Se.Settings.General.MinimumMillisecondsBetweenLines;
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
    private void VideoOneFrameBack()
    {
        MoveVideoPositionMs(-40);
    }

    [RelayCommand]
    private void VideoOneFrameForward()
    {
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
        s.SetStartTimeOnly(TimeSpan.FromMilliseconds(prev.EndTime.TotalMilliseconds + Se.Settings.General.MinimumMillisecondsBetweenLines));
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
                                              Se.Settings.General.MinimumMillisecondsBetweenLines);
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

    private async Task<TViewModel> ShowDialogAsync<TWindow, TViewModel>(
        Action<TViewModel>? configureViewModel = null, Action<TWindow>? configureWindow = null)
        where TWindow : Window
        where TViewModel : class
    {
        GetVideoPlayerControl()?.VideoPlayerInstance.Pause();
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

        if (atTextBoxPosition && atTextBoxPosition && vp != null)
        {
            _splitManager.Split(Subtitles, s, vp.Position, EditTextBox.SelectionStart);
        }
        else if (atVideoPosition && vp != null)
        {
            _splitManager.Split(Subtitles, s, vp.Position);
        }
        else if (atTextBoxPosition)
        {
            _splitManager.Split(Subtitles, s, EditTextBox.SelectionStart);
        }
        else
        {
            _splitManager.Split(Subtitles, s);
        }
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
                "Download ffmpeg?",
                $"{Environment.NewLine}Some functions in Subtitle Edit requires ffmpeg.{Environment.NewLine}{Environment.NewLine}Download and use ffmpeg?",
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

        _undoRedoManager.CheckForChanges(null);
        _undoRedoManager.StopChangeDetection();
        var undoRedoObject = _undoRedoManager.Undo()!;
        RestoreUndoRedoState(undoRedoObject);
        ShowUndoStatus();
        _undoRedoManager.StartChangeDetection();
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
        _undoRedoManager.CheckForChanges(null);
        if (!_undoRedoManager.CanRedo)
        {
            return;
        }

        _undoRedoManager.StopChangeDetection();
        var undoRedoObject = _undoRedoManager.Redo()!;
        RestoreUndoRedoState(undoRedoObject);
        ShowRedoStatus();
        _undoRedoManager.StartChangeDetection();
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
            1);
    }

    private void RestoreUndoRedoState(UndoRedoItem undoRedoObject)
    {
        Subtitles.Clear();
        foreach (var p in undoRedoObject.Subtitles)
        {
            Subtitles.Add(p);
        }

        _subtitleFileName = undoRedoObject.SubtitleFileName;
        SelectAndScrollToRow(undoRedoObject.SelectedLines.First());
    }

    public void AutoFitColumns()
    {
        var columns = SubtitleGrid.Columns.Where(p => p.IsVisible).ToList();

        var numberOfStarColumns = 0;
        for (var i = 0; i < columns.Count; i++)
        {
            var column = columns[i];

            var originalWidth = column.Width;

            if (column.Header.ToString() == Se.Language.General.Show ||
                column.Header.ToString() == Se.Language.General.Hide)
            {
                column.Width = new DataGridLength(column.MinWidth, DataGridLengthUnitType.Pixel);
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

        lock (_scrollLock)
        {
            _pendingScrollIndex = index;
        }

        try
        {
            var item = Subtitles[index];
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
                }
            });
        }
        catch
        {
            // ignore
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
                if (tb.Text.Contains("{\\" + assaOn + " }"))
                {
                    tb.Text = tb.Text.Replace("{\\" + assaOn + "}", string.Empty)
                        .Replace("{\\" + assaOff + "0}", string.Empty);
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
        bool skipLoadVideo = false)
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
            await MessageBox.Show(Window!, Se.Language.General.Error, message, MessageBoxButtons.OK,
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
                        }
                    });
                    return;
                }
            }

            if ((ext == ".mp4" || ext == ".m4v" || ext == ".3gp" || ext == ".mov" || ext == ".cmaf") &&
                fileSize > 2000 || ext == ".m4s")
            {
                if (!new IsmtDfxp().IsMine(null, fileName))
                {
                    await ImportSubtitleFromMp4(fileName);
                    return;
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


            if (ext == ".ismt" || ext == ".mp4" || ext == ".m4v" || ext == ".mov" || ext == ".3gp" || ext == ".cmaf" ||
                ext == ".m4s")
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

            if (ext == ".divx" || ext == ".avi")
            {
                if (ImportSubtitleFromDivX(fileName))
                {
                }
                else
                {
                    //   MessageBox.Show(_language.NotAValidXSubFile);
                }

                return;
            }

            var fileEncoding = LanguageAutoDetect.GetEncodingFromFile(fileName);
            var subtitle = Subtitle.Parse(fileName, fileEncoding);
            if (subtitle != null && subtitle.OriginalFormat.Name == new WebVTT().Name)
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

            SetSubtitleFormat(SubtitleFormats.FirstOrDefault(p => p.Name == subtitle.OriginalFormat.Name) ??
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

            if (Se.Settings.Video.AutoOpen && skipLoadVideo == false)
            {
                if (!string.IsNullOrEmpty(videoFileName) && File.Exists(videoFileName))
                {
                    await VideoOpenFile(videoFileName);
                }
                else if (FindVideoFileName.TryFindVideoFileName(fileName, out videoFileName))
                {
                    await VideoOpenFile(videoFileName);
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

    private void SetupLiveSpellCheck()
    {
        if (EditTextBox is TextEditorWrapper wrapper)
        {
            if (Se.Settings.Appearance.SubtitleTextBoxLiveSpellCheck)
            {
                var twoLetterLanguageCode = LanguageAutoDetect.AutoDetectGoogleLanguage(GetUpdateSubtitle());
                var threeLetterLanguageCode = Iso639Dash2LanguageCode.GetThreeLetterCodeFromTwoLetterCode(twoLetterLanguageCode);
                if (!string.IsNullOrEmpty(threeLetterLanguageCode))
                {
                    var spellCheckLanguages = _spellCheckManager.GetDictionaryLanguages(Se.DictionariesFolder);
                    var dictionary = spellCheckLanguages.FirstOrDefault(p => p.GetThreeLetterCode() == threeLetterLanguageCode);
                    if (dictionary != null)
                    {
                        _spellCheckManager.Initialize(dictionary.DictionaryFileName, twoLetterLanguageCode);
                        wrapper.EnableSpellCheck(_spellCheckManager);
                        SubtitleDataGridSyntaxHighlighting.EnableSpellCheck(_spellCheckManager);
                    }
                }
            }
            else if (!Se.Settings.Appearance.SubtitleTextBoxLiveSpellCheck)
            {
                wrapper.DisableSpellCheck();
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
            }
        });

        return;
    }

    private int _lastProgressPercent = -1;

    private void UpdateProgress(long position, long total, string statusMessage)
    {
        var percent = (int)Math.Round(position * 100.0 / total);
        if (percent == _lastProgressPercent)
        {
            return;
        }

        ShowStatus(string.Format("{0}, {1:0}%", statusMessage, _lastProgressPercent));
        _lastProgressPercent = percent;
    }

    private async Task ImportSubtitleFromMp4(string fileName)
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
                return;
            }

            if (mp4Parser.TrunCea608Subtitle?.Paragraphs.Count > 0)
            {
                ResetSubtitle();
                _subtitle = mp4Parser.TrunCea608Subtitle;
                _subtitle.Renumber();
                _subtitleFileName = Utilities.GetPathAndFileNameWithoutExtension(fileName) +
                                    SelectedSubtitleFormat.Extension;
                Subtitles.AddRange(
                    _subtitle.Paragraphs.Select(p => new SubtitleLineViewModel(p, SelectedSubtitleFormat)));
                _converted = true;
                ShowStatus(string.Format(Se.Language.General.SubtitleLoadedX, fileName));
                SelectAndScrollToRow(0);
                return;
            }

            await MessageBox.Show(Window!, Se.Language.General.Error, Se.Language.General.NoSubtitlesFound,
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        else if (mp4SubtitleTracks.Count == 1)
        {
            LoadMp4Subtitle(fileName, mp4SubtitleTracks[0]);

            if (Se.Settings.General.AutoOpenVideo && mp4Parser.GetVideoTracks().Count > 0)
            {
                await VideoOpenFile(fileName);
            }
        }
        else
        {
            var result =
                await ShowDialogAsync<PickMp4TrackWindow, PickMp4TrackViewModel>(vm => { vm.Initialize(mp4SubtitleTracks, fileName); });

            if (result.OkPressed && result.SelectedTrack != null && result.SelectedTrack.Track != null)
            {
                LoadMp4Subtitle(fileName, result.SelectedTrack.Track);
            }
        }
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
                                              Se.Settings.General.MinimumMillisecondsBetweenLines;
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

        if (languageStreamIds.Count > 1)
        {
            //using (var chooseLanguage = new DvdSubRipChooseLanguage())
            //{
            //    if (ShowInTaskbar)
            //    {
            //        chooseLanguage.Icon = (Icon)Icon.Clone();
            //        chooseLanguage.ShowInTaskbar = true;
            //        chooseLanguage.ShowIcon = true;
            //    }

            //    chooseLanguage.Initialize(_vobSubMergedPackList, _palette, vobSubParser.IdxLanguages, string.Empty);
            //    var form = _main ?? (Form)this;
            //    if (batchMode)
            //    {
            //        chooseLanguage.SelectActive();
            //        vobSubMergedPackList = chooseLanguage.SelectedVobSubMergedPacks;
            //        SetTesseractLanguageFromLanguageString(chooseLanguage.SelectedLanguageString);
            //        _importLanguageString = chooseLanguage.SelectedLanguageString;
            //        return true;
            //    }

            //    chooseLanguage.Activate();
            //    if (chooseLanguage.ShowDialog(form) == DialogResult.OK)
            //    {
            //        _vobSubMergedPackList = chooseLanguage.SelectedVobSubMergedPacks;
            //        SetTesseractLanguageFromLanguageString(chooseLanguage.SelectedLanguageString);
            //        _importLanguageString = chooseLanguage.SelectedLanguageString;
            //        return true;
            //    }

            //    return false;
            //}
        }

        var streamId = languageStreamIds.First();
        var result = await ShowDialogAsync<OcrWindow, OcrViewModel>(vm => { vm.Initialize(streamIdDictionary[streamId], palette, vobSubFileName); });

        if (result.OkPressed)
        {
            _subtitleFileName = Path.GetFileNameWithoutExtension(vobSubFileName);
            _converted = true;
            Subtitles.Clear();
            Subtitles.AddRange(result.OcredSubtitle);
            return true;
        }

        return false;
    }

    private void SetSubtitles(Subtitle subtitle)
    {
        Subtitles.Clear();
        foreach (var p in subtitle.Paragraphs)
        {
            Subtitles.Add(new SubtitleLineViewModel(p, SelectedSubtitleFormat));
        }

        Renumber();
        UpdateGaps();
        _updateAudioVisualizer = true;
    }

    private void SetSubtitles(List<SubtitleLineViewModel> subtitles)
    {
        Subtitles.Clear();
        foreach (var p in subtitles)
        {
            Subtitles.Add(p);
        }

        Renumber();
        UpdateGaps();
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

        var currentSubtitleHash = GetFastHash();
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

        return true;
    }

    private async Task<bool> SaveSubtitleOriginal()
    {
        if (Subtitles == null || !Subtitles.Any())
        {
            ShowStatus(Se.Language.Main.NothingToSaveOriginal);
            return false;
        }

        if (string.IsNullOrEmpty(_subtitleFileNameOriginal) || _converted)
        {
            return await SaveSubtitleOriginalAs();
        }

        if (_lastOpenSaveFormat == null || _lastOpenSaveFormat.Name != SelectedSubtitleFormat.Name)
        {
            return await SaveSubtitleOriginalAs();
        }

        var text = GetUpdateSubtitleOriginal(true).ToText(SelectedSubtitleFormat);

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

        _changeSubtitleHashOriginal = GetFastHashOriginal();
        _lastOpenSaveFormat = SelectedSubtitleFormat;
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

        _subtitleOriginal.Paragraphs.Clear();
        foreach (var line in Subtitles)
        {
            var p = line.ToParagraphOriginal(SelectedSubtitleFormat);
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

            if (!string.IsNullOrEmpty(_subtitleFileName))
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

                if (Se.Settings.General.SaveAsAppendLanguageCode == nameof(SaveAsLanguageAppendType.TwoLetterLanguageCode))
                {
                    newFileName += "." + l.TwoLetterCode;
                }
                else if (Se.Settings.General.SaveAsAppendLanguageCode == nameof(SaveAsLanguageAppendType.ThreeLEtterLanguageCode))
                {
                    newFileName += "." + l.ThreeLetterCode;
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

        _subtitleFileNameOriginal = fileName;

        _subtitleOriginal ??= new Subtitle();
        _subtitleOriginal.FileName = fileName;

        _lastOpenSaveFormat = SelectedSubtitleFormat;
        await SaveSubtitleOriginal();
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
        else
        {
            CleanUp();
        }
    }

    private void CleanUp()
    {
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

        GetVideoPlayerControl()?.VideoPlayerInstance.CloseFile();

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
        else if (Se.Settings.File.ShowRecentFiles)
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

                        await SubtitleOpen(first.SubtitleFileName, first.VideoFileName, first.SelectedLine, null, skipLoadVideo);
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

    private async Task VideoOpenFile(string videoFileName) // OpenVideoFile
    {
        var vp = GetVideoPlayerControl();
        if (vp == null)
        {
            return;
        }

        _videoOpenTokenSource?.Cancel();
        await vp.Open(videoFileName);
        _videoFileName = videoFileName;
        _mpvReloader.Reset();

        if (IsValidUrl(videoFileName))
        {
            _videoFileName = videoFileName;
            await AddEmptyWaveform();
            IsVideoLoaded = true;
            return;
        }

        IsVideoLoaded = true;

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
        }
    }

    private void LoadAudioTrackMenuItems()
    {
        try
        {
            var vp = GetVideoPlayerControl();
            if (vp?.VideoPlayerInstance is LibMpvDynamicPlayer mpv)
            {
                var audioTracks = mpv.GetAudioTracks();
                if (audioTracks.Count == 0)
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        IsAudioTracksVisible = false;
                        AudioTraksMenuItem.Items.Clear();
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
                            menuItem.Icon = new Projektanker.Icons.Avalonia.Icon
                            {
                                Value = IconNames.CheckBold,
                                VerticalAlignment = VerticalAlignment.Center,
                            };
                        }

                        AudioTraksMenuItem.Items.Add(menuItem);
                    }

                    IsAudioTracksVisible = AudioTraksMenuItem.Items.Count > 1;
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
        if (vp.VideoPlayerInstance.Duration > 0)
        {
            var peakWaveFileName = WavePeakGenerator2.GetPeakWaveFileName(_videoFileName ?? string.Empty, _audioTrack?.FfIndex ?? -1);
            AudioVisualizer.ZoomFactor = 1.0;
            AudioVisualizer.VerticalZoomFactor = 1.0;
            AudioVisualizer.WavePeaks = WavePeakGenerator2.GenerateEmptyPeaks(peakWaveFileName, (int)vp.VideoPlayerInstance.Duration);
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

            _ = Task.Run(() =>
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

                    Task.Delay(100).Wait();
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
        unchecked // Overflow is fine, just wrap
        {
            var hash = 17;
            hash = hash * 23 + pre.GetHashCode();

            if (_subtitle.Header != null)
            {
                hash = hash * 23 + _subtitle.Header.Trim().GetHashCode();
            }

            if (_subtitle.Footer != null)
            {
                hash = hash * 23 + _subtitle.Footer.Trim().GetHashCode();
            }

            var max = Subtitles.Count;
            for (var i = 0; i < max; i++)
            {
                var p = Subtitles[i];
                hash = hash * 23 + p.Number.GetHashCode();
                hash = hash * 23 + p.StartTime.TotalMilliseconds.GetHashCode();
                hash = hash * 23 + p.EndTime.TotalMilliseconds.GetHashCode();

                foreach (var line in p.Text.SplitToLines())
                {
                    hash = hash * 23 + line.GetHashCode();
                }

                if (p.Style != null)
                {
                    hash = hash * 23 + p.Style.GetHashCode();
                }

                if (p.Extra != null)
                {
                    hash = hash * 23 + p.Extra.GetHashCode();
                }

                if (p.Actor != null)
                {
                    hash = hash * 23 + p.Actor.GetHashCode();
                }

                hash = hash * 23 + p.Layer.GetHashCode();
            }

            return hash;
        }
    }

    public int GetFastHashOriginal()
    {
        _subtitleOriginal ??= new Subtitle();

        var pre = _subtitleFileNameOriginal + SelectedEncoding.DisplayName;
        unchecked // Overflow is fine, just wrap
        {
            var hash = 17;
            hash = hash * 23 + pre.GetHashCode();

            if (_subtitleOriginal.Header != null)
            {
                hash = hash * 23 + _subtitleOriginal.Header.Trim().GetHashCode();
            }

            if (_subtitleOriginal.Footer != null)
            {
                hash = hash * 23 + _subtitleOriginal.Footer.Trim().GetHashCode();
            }

            var max = Subtitles.Count;
            for (var i = 0; i < max; i++)
            {
                var p = Subtitles[i];
                hash = hash * 23 + p.Number.GetHashCode();
                hash = hash * 23 + p.StartTime.TotalMilliseconds.GetHashCode();
                hash = hash * 23 + p.EndTime.TotalMilliseconds.GetHashCode();

                foreach (var line in p.OriginalText.SplitToLines())
                {
                    hash = hash * 23 + line.GetHashCode();
                }

                if (p.Style != null)
                {
                    hash = hash * 23 + p.Style.GetHashCode();
                }

                if (p.Extra != null)
                {
                    hash = hash * 23 + p.Extra.GetHashCode();
                }

                if (p.Actor != null)
                {
                    hash = hash * 23 + p.Actor.GetHashCode();
                }

                hash = hash * 23 + p.Layer.GetHashCode();
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

        if (Se.Settings.General.PromptDeleteLines)
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

        if (Se.Settings.General.PromptDeleteLines)
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

        SelectAndScrollToRow(index - 1);
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
        ShowColumnLayerFlyoutMenuItem = IsFormatAssa;

        if (IsSubtitleGridFlyoutHeaderVisible)
        {
            IsMergeWithNextOrPreviousVisible = false;
        }
        else
        {
            IsMergeWithNextOrPreviousVisible = SubtitleGrid.SelectedItems.Count == 1;

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

    private readonly Lock _onKeyDownHandlerLock = new();

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

            _shortcutManager.OnKeyPressed(this, keyEventArgs);
            if (_shortcutManager.GetActiveKeys().Count == 0)
            {
                return;
            }

            if (IsTextInputFocused())
            {
                var currentKeys = _shortcutManager.GetActiveKeys();
                if (currentKeys.Count == 1 && (keyEventArgs.KeyModifiers == KeyModifiers.None || keyEventArgs.KeyModifiers == KeyModifiers.Shift))
                {
                    var key = currentKeys.First();
                    var allowedSingleKeyShortcuts = new HashSet<Key>
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

                    if (!allowedSingleKeyShortcuts.Contains(key))
                    {
                        if (EditTextBox.IsFocused)
                        {
                            if (key == Key.Return &&
                                Se.Settings.General.SubtitleTextBoxLimitNewLines)
                            {
                                var newLineCount = EditTextBox.Text.SplitToLines().Count;
                                if (newLineCount >= Se.Settings.General.MaxNumberOfLines)
                                {
                                    keyEventArgs.Handled = true;
                                }
                            }
                        }


                        return;
                    }
                }
            }

            if (IsSubtitleGridFocused())
            {
                if (keyEventArgs.Key == Key.Home && keyEventArgs.KeyModifiers == KeyModifiers.None && Subtitles.Count > 0)
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
        }

        _shortcutManager.OnKeyReleased(this, e);
        AudioVisualizer?.SetKeyModifiers(e);
    }

    private bool _subtitleGridIsRightClick = false;
    private bool _subtitleGridIsLeftClick = false;
    private bool _subtitleGridIsControlPressed = false;

    public void SubtitleGrid_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _subtitleGridIsControlPressed = false;
        _subtitleGridIsLeftClick = false;
        _subtitleGridIsRightClick = false;
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
        }
    }

    public void SubtitleGrid_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is Control { ContextFlyout: MenuFlyout menuFlyout } control)
        {
            if (_subtitleGridIsRightClick)
            {
                menuFlyout.ShowAt(control, true);
            }

            if (OperatingSystem.IsMacOS())
            {
                if (_subtitleGridIsLeftClick && _subtitleGridIsControlPressed)
                {
                    menuFlyout.ShowAt(control, true);
                    e.Handled = true;
                }
            }
        }
    }

    public void SubtitleGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_subtitleGridSelectionChangedSkip)
        {
            return;
        }

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

        if (selectedItems == null)
        {
            SelectedSubtitle = null;
            SelectedSubtitleIndex = null;
            _selectedSubtitles = null;
            StatusTextRight = string.Empty;
            EditTextCharactersPerSecond = string.Empty;
            EditTextCharactersPerSecondBackground = Brushes.Transparent;
            EditTextTotalLength = string.Empty;
            EditTextTotalLengthBackground = Brushes.Transparent;
            EditTextLineLengths = string.Empty;

            EditTextCharactersPerSecondOriginal = string.Empty;
            EditTextCharactersPerSecondBackgroundOriginal = Brushes.Transparent;
            EditTextTotalLengthOriginal = string.Empty;
            EditTextTotalLengthBackgroundOriginal = Brushes.Transparent;
            EditTextLineLengthsOriginal = string.Empty;

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
            EditTextLineLengths = string.Empty;

            EditTextCharactersPerSecondOriginal = string.Empty;
            EditTextCharactersPerSecondBackgroundOriginal = Brushes.Transparent;
            EditTextTotalLengthOriginal = string.Empty;
            EditTextTotalLengthBackgroundOriginal = Brushes.Transparent;
            EditTextTotalLengthOriginal = string.Empty;
            EditTextLineLengthsOriginal = string.Empty;

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

        StatusTextRight = $"{Subtitles.IndexOf(item) + 1}/{Subtitles.Count}";
        if (item == SelectedSubtitle && item.Text == EditText)
        {
            return;
        }

        try
        {
            _subtitleGridSelectionChangedSkip = true;
            SelectedSubtitle = item;
            SelectedSubtitleIndex = Subtitles.IndexOf(item);
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
                tb.Background = new SolidColorBrush(_errorColor);
            }

            PanelSingleLineLengthsOriginal.Children.Add(tb);
        }

        EditTextCharactersPerSecondOriginal = string.Format(Se.Language.Main.CharactersPerSecond, $"{cps:0.0}");
        EditTextTotalLengthOriginal = string.Format(Se.Language.Main.TotalCharacters, totalLength);

        EditTextCharactersPerSecondBackgroundOriginal = Se.Settings.General.ColorTextTooLong &&
                                                        cps > Se.Settings.General.SubtitleMaximumCharactersPerSeconds
            ? new SolidColorBrush(_errorColor)
            : new SolidColorBrush(Colors.Transparent);

        EditTextTotalLengthBackgroundOriginal = Se.Settings.General.ColorTextTooLong &&
                                                totalLength > Se.Settings.General.SubtitleLineMaximumLength *
                                                lines.Count
            ? new SolidColorBrush(_errorColor)
            : new SolidColorBrush(Colors.Transparent);
    }

    private bool _avLastScrolling = false;

    private void StartTimers()
    {
        _positionTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
        _positionTimer.Tick += (s, e) =>
        {
            // update audio visualizer position if available
            var av = AudioVisualizer;
            var vp = GetVideoPlayerControl();
            if (av != null && vp != null && !string.IsNullOrEmpty(_videoFileName))
            {
                var isAvScrolloing = av.IsScrolling;

                if (_setEndAtKeyUpLine != null)
                {
                    _setEndAtKeyUpLine.EndTime = TimeSpan.FromSeconds(vp.VideoPlayerInstance.Position);
                }

                var noLayers = _visibleLayers == null || !Se.Settings.Assa.HideLayersFromWaveform || _visibleLayers.Count == 0;
                var subtitle = new ObservableCollection<SubtitleLineViewModel>(
                    Subtitles
                        .OrderBy(p => p.StartTime.TotalMilliseconds)
                        .Where(p => noLayers || _visibleLayers!.Contains(p.Layer))
                );

                var mediaPlayerSeconds = vp.Position;
                var startPos = mediaPlayerSeconds - 0.01;
                if (startPos < 0)
                {
                    startPos = 0;
                }

                av.CurrentVideoPositionSeconds = vp.Position;
                var isPlaying = vp.IsPlaying;
                var firstSelectedIndex = -1;

                if (WaveformCenter && isPlaying)
                {
                    // calculate the center position based on the waveform width
                    var waveformHalfSeconds = (av.EndPositionSeconds - av.StartPositionSeconds) / 2.0;
                    av.SetPosition(Math.Max(0, mediaPlayerSeconds - waveformHalfSeconds), subtitle, mediaPlayerSeconds,
                        firstSelectedIndex, _selectedSubtitles ?? []);
                }
                else if (isPlaying && _avLastScrolling && !isAvScrolloing)
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
                else if ((isPlaying) &&
                         (mediaPlayerSeconds > av.EndPositionSeconds || mediaPlayerSeconds < av.StartPositionSeconds))
                {
                    av.SetPosition(startPos, subtitle, mediaPlayerSeconds, 0, _selectedSubtitles ?? []);
                }
                else
                {
                    av.SetPosition(av.StartPositionSeconds, subtitle, mediaPlayerSeconds, firstSelectedIndex,
                        _selectedSubtitles ?? []);
                }

                if (_updateAudioVisualizer)
                {
                    av.InvalidateVisual();
                    _updateAudioVisualizer = false;
                }

                if (isPlaying)
                {
                    Projektanker.Icons.Avalonia.Attached.SetIcon(ButtonWaveformPlay, IconNames.Pause);

                    if (_playSelectionItem != null && mediaPlayerSeconds >= _playSelectionItem.EndSeconds)
                    {
                        var p = _playSelectionItem.GetNextSubtitle(mediaPlayerSeconds);
                        if (p == null)
                        {
                            vp.VideoPlayerInstance.Pause();
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
                    Projektanker.Icons.Avalonia.Attached.SetIcon(ButtonWaveformPlay, IconNames.Play);
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
            if (vp?.VideoPlayerInstance is LibMpvDynamicPlayer mpv)
            {
                var subtitle = GetUpdateSubtitle();
                var hasVisibleLayers = _visibleLayers != null && Se.Settings.Assa.HideLayersFromVideoPreview;
                if (hasVisibleLayers)
                {
                    var paragraphs = subtitle.Paragraphs.Where(p => _visibleLayers!.Contains(p.Layer)).ToList();
                    subtitle.Paragraphs.Clear();
                    subtitle.Paragraphs.AddRange(paragraphs);
                }

                _mpvReloader.RefreshMpv(mpv, subtitle, SelectedSubtitleFormat).ConfigureAwait(false);
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
            Subtitles[Subtitles.Count - 1].IsHidden = hasLayers && !_visibleLayers!.Contains(Subtitles.Last().Layer); ;
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

        _updateAudioVisualizer = true; // Update the audio visualizer position
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
                vp.VideoPlayerInstance.Play();
                AudioVisualizerCenterOnPositionIfNeeded(selectedItem, seconds);
                return;
            }

            if (Se.Settings.General.SubtitleDoubleClickAction == SubtitleDoubleClickActionType.GoToSubtitleAndPauseAndFocusTextBox.ToString())
            {
                vp.VideoPlayerInstance.Pause();
                vp.Position = seconds;
                AudioVisualizerCenterOnPositionIfNeeded(selectedItem, seconds);
                FocusEditTextBox();
                return;
            }

            // SubtitleDoubleClickActionType.GoToSubtitleAndPause
            vp.VideoPlayerInstance.Pause();
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
                vp.VideoPlayerInstance.Pause();
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
                vp.VideoPlayerInstance.Play();
                AudioVisualizerCenterOnPositionIfNeeded(selectedItem, seconds);
                return;
            }

            if (Se.Settings.General.SubtitleSingleClickAction == SubtitleSingleClickActionType.GoToSubtitleAndPauseAndFocusTextBox.ToString())
            {
                vp.VideoPlayerInstance.Pause();
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
                        changeDictionary.Click += (_, _) =>
                        {
                            PickLiveSpellCheckDictionary(wrapper);
                        };
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

        if (e.NewParagraph != null)
        {
            MenuItemAudioVisualizerInsertNewSelection.IsVisible = true;
            MenuItemAudioVisualizerPasteNewSelection.IsVisible = true;
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
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                MenuItemAudioVisualizerPasteFromClipboardMenuItem.IsVisible = !string.IsNullOrEmpty(clipboardText);
                            });
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

        if (subtitlesAtPosition.Count > 0)
        {
            MenuItemAudioVisualizerDeleteAtPosition.IsVisible = true;
            MenuItemAudioVisualizerSplitAtPosition.IsVisible = true;
            MenuItemAudioVisualizerSpeechToTextSelectedLines.IsVisible = true;
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

        if (!_opening && e.RemovedItems.Count == 1 && e.AddedItems.Count == 1)
        {
            var format = e.AddedItems[0] as SubtitleFormat;
            var oldFormat = e.RemovedItems[0] as SubtitleFormat;

            if (oldFormat != null && format != null)
            {
                _subtitle = GetUpdateSubtitle();

                oldFormat.RemoveNativeFormatting(_subtitle, format);

                if (format is AdvancedSubStationAlpha)
                {
                    if (oldFormat is WebVTT || oldFormat is WebVTTFileWithLineNumber)
                    {
                        //                        _subtitle = WebVttToAssa.Convert(_subtitle, new SsaStyle(), VideoPlayerControl?.VideoPlayerInstance?.Width ?? 0, VideoPlayerControl?.VideoPlayerInstance?.Height ?? 0);
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

                SetSubtitles(_subtitle);
            }
        }

        IsFilePropertiesVisible = false;
        if (e.AddedItems.Count == 1)
        {
            var format = e.AddedItems[0] as SubtitleFormat;
            if (format is TimedTextImscRosetta or TmpegEncXml or DCinemaSmpte2007 or DCinemaSmpte2010 or DCinemaSmpte2014)
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
                    }
                }
            });
        }
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
        if (mediaInfo == null || Window == null || args.Properties.IsLeftButtonPressed)
        {
            return;
        }

        ShowMediaInformation();
    }

    [RelayCommand]
    private void ShowMediaInformation()
    {
        var mediaInfo = _mediaInfo;
        if (mediaInfo == null || Window == null)
        {
            return;
        }

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
        if (e.GetCurrentPoint(null).Properties.IsRightButtonPressed)
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
                    vp.VideoPlayerInstance.Pause();
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
                    vp.VideoPlayerInstance.Pause();
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
                    vp.VideoPlayerInstance.Pause();
                    vp.Position = e.Seconds;
                    break;
                case WaveformSingleClickActionType.SetVideopositionAndPauseAndCenter:
                    vp.VideoPlayerInstance.Pause();
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
                    vp.VideoPlayerInstance.Pause();
                    break;
                case WaveformDoubleClickActionType.Play:
                    vp.VideoPlayerInstance.Play();
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

        _undoRedoManager.StopChangeDetection();
        for (var i = index; i < Subtitles.Count; i++)
        {
            var subtitle = Subtitles[i];
            subtitle.StartTime += difference;
        }

        _updateAudioVisualizer = true;
        _undoRedoManager.StartChangeDetection();
    }
}