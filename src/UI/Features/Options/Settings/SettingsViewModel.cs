using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Assa;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Shared.PickSubtitleFormat;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using DownloadFfmpegViewModel = Nikse.SubtitleEdit.Features.Shared.DownloadFfmpegViewModel;
using DownloadLibMpvViewModel = Nikse.SubtitleEdit.Features.Shared.DownloadLibMpvViewModel;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<string> _profiles;
    [ObservableProperty] private string _selectedProfile;
    [ObservableProperty] private int? _singleLineMaxLength;
    [ObservableProperty] private double? _optimalCharsPerSec;
    [ObservableProperty] private double? _maxCharsPerSec;
    [ObservableProperty] private double? _maxWordsPerMin;
    [ObservableProperty] private int? _minDurationMs;
    [ObservableProperty] private int? _maxDurationMs;
    [ObservableProperty] private int? _minGapMs;
    [ObservableProperty] private int? _maxLines;
    [ObservableProperty] private int? _unbreakLinesShorterThan;
    [ObservableProperty] private ObservableCollection<DialogStyleDisplay> _dialogStyles;
    [ObservableProperty] private DialogStyleDisplay _dialogStyle;
    [ObservableProperty] private ObservableCollection<ContinuationStyleDisplay> _continuationStyles;
    [ObservableProperty] private ContinuationStyleDisplay _continuationStyle;
    [ObservableProperty] private ObservableCollection<CpsLineLengthStrategyDisplay> _cpsLineLengthStrategies;
    [ObservableProperty] private CpsLineLengthStrategyDisplay _cpsLineLengthStrategy;
    [ObservableProperty] private ObservableCollection<string> _fonts;
    [ObservableProperty] private string _mpvPreviewFontName;
    [ObservableProperty] private int _mpvPreviewFontSize;
    [ObservableProperty] private bool _mpvPreviewFontBold;
    [ObservableProperty] private Color _mpvPreviewColorPrimary;
    [ObservableProperty] private Color _mpvPreviewColorOutline;
    [ObservableProperty] private Color _mpvPreviewColorShadow;
    [ObservableProperty] private ObservableCollection<BorderStyleItem> _mpvPreviewBorderTypes;
    [ObservableProperty] private BorderStyleItem _mpvPreviewSelectedBorderType;
    [ObservableProperty] private decimal _mpvPreviewOutlineWidth;
    [ObservableProperty] private decimal _mpvPreviewShadowWidth;

    [ObservableProperty] private int? _newEmptyDefaultMs;
    [ObservableProperty] private bool _promptDeleteLines;
    [ObservableProperty] private bool _lockTimeCodes;
    [ObservableProperty] private bool _rememberPositionAndSize;
    [ObservableProperty] private bool _useFrameMode;
    [ObservableProperty] private bool _textBoxLimitNewLines;
    [ObservableProperty] private bool _autoBackupOn;
    [ObservableProperty] private int? _autoBackupIntervalMinutes;
    [ObservableProperty] private int? _autoBackupDeleteAfterDays;

    [ObservableProperty] private ObservableCollection<string> _defaultSubtitleFormats;
    [ObservableProperty] private string _selectedDefaultSubtitleFormat;

    [ObservableProperty] private ObservableCollection<string> _saveSubtitleFormats;
    [ObservableProperty] private string _selectedSaveSubtitleFormat;

    [ObservableProperty] private ObservableCollection<string> _favoriteSubtitleFormats;
    [ObservableProperty] private string? _selectedFavoriteSubtitleFormat;

    [ObservableProperty] private ObservableCollection<TextEncoding> _encodings;
    [ObservableProperty] private TextEncoding _defaultEncoding;
    [ObservableProperty] private bool _autoConvertToUtf8;
    [ObservableProperty] private bool _forceCrLfOnSave;
    [ObservableProperty] private bool _autoTrimWhiteSpace;

    [ObservableProperty] private ObservableCollection<string> _subtitleEnterKeyActionTypes;
    [ObservableProperty] private string _selectedSubtitleEnterKeyActionType;

    [ObservableProperty] private ObservableCollection<string> _subtitleSingleClickActionTypes;
    [ObservableProperty] private string _selectedSubtitleSingleClickActionType;

    [ObservableProperty] private ObservableCollection<string> _subtitleDoubleClickActionTypes;
    [ObservableProperty] private string _selectedSubtitleDoubleClickActionType;

    [ObservableProperty] private ObservableCollection<string> _saveAsBehaviorTypes;
    [ObservableProperty] private string _selectedSaveAsBehaviorType;

    [ObservableProperty] private ObservableCollection<string> _saveAsAppendLanguageCode;
    [ObservableProperty] private string _selectedSaveAsAppendLanguageCode;

    [ObservableProperty] private bool _goToLineNumberAlsoSetVideoPosition;
    [ObservableProperty] private bool _adjustAllTimesRememberLineSelectionChoice;
    [ObservableProperty] private ObservableCollection<string> _splitOddNumberOfLinesActions;
    [ObservableProperty] private string _selectedSplitOddNumberOfLinesAction;
    [ObservableProperty] private bool _ocrUseWordSplitList;

    [ObservableProperty] private bool _showUpDownStartTime;
    [ObservableProperty] private bool _showUpDownEndTime;
    [ObservableProperty] private bool _showUpDownDuration;
    [ObservableProperty] private bool _showUpDownLabels;

    [ObservableProperty] private bool _showToolbarNew;
    [ObservableProperty] private bool _showToolbarOpen;
    [ObservableProperty] private bool _showToolbarSave;
    [ObservableProperty] private bool _showToolbarSaveAs;
    [ObservableProperty] private bool _showToolbarFind;
    [ObservableProperty] private bool _showToolbarReplace;
    [ObservableProperty] private bool _showToolbarSpellCheck;
    [ObservableProperty] private bool _showToolbarSettings;
    [ObservableProperty] private bool _showToolbarLayout;
    [ObservableProperty] private bool _showToolbarHelp;
    [ObservableProperty] private bool _showToolbarEncoding;
    [ObservableProperty] private bool _showToolbarFrameRate;

    [ObservableProperty] private bool _textBoxButtonShowAutoBreak;
    [ObservableProperty] private bool _textBoxButtonShowUnbreak;
    [ObservableProperty] private bool _textBoxButtonShowItalic;
    [ObservableProperty] private bool _textBoxButtonShowColor;
    [ObservableProperty] private bool _textBoxButtonShowRemoveFormatting;

    [ObservableProperty] private bool _colorDurationTooShort;
    [ObservableProperty] private bool _colorDurationTooLong;
    [ObservableProperty] private bool _colorTextTooLong;
    [ObservableProperty] private bool _colorTextTooWide;
    [ObservableProperty] private bool _colorTextTooManyLines;
    [ObservableProperty] private bool _colorOverlap;
    [ObservableProperty] private bool _colorGapTooShort;
    [ObservableProperty] private Color _errorColor;

    [ObservableProperty] private ObservableCollection<VideoPlayerItem> _videoPlayers;
    [ObservableProperty] private VideoPlayerItem _selectedVideoPlayer;
    [ObservableProperty] private bool _showStopButton;
    [ObservableProperty] private bool _showFullscreenButton;
    [ObservableProperty] private bool _autoOpenVideoFile;

    [ObservableProperty] private bool _waveformDrawGridLines;
    [ObservableProperty] private bool _waveformFocusOnMouseOver;
    [ObservableProperty] private bool _waveformCenterVideoPosition;

    [ObservableProperty] private ObservableCollection<string> _waveformDrawStyles;
    [ObservableProperty] private string _selectedWaveformDrawStyle;

    [ObservableProperty] private bool _waveformGenerateSpectrogram;
    [ObservableProperty] private ObservableCollection<string> _waveformSpectrogramStyles;
    [ObservableProperty] private string _selectedWaveformSpectrogramStyle;

    [ObservableProperty] private int _waveformSpectrogramCombinedWaveformHeight;

    [ObservableProperty] private bool _waveformShowToolbar;

    [ObservableProperty] private bool _showWaveformToolbarPlay;
    [ObservableProperty] private bool _showWaveformToolbarPlayNext;
    [ObservableProperty] private bool _showWaveformToolbarPlaySelection;
    [ObservableProperty] private bool _showWaveformToolbarRepeat;
    [ObservableProperty] private bool _showWaveformToolbarRemoveBlankLines;
    [ObservableProperty] private bool _showWaveformToolbarNew;
    [ObservableProperty] private bool _showWaveformToolbarSetStart;
    [ObservableProperty] private bool _showWaveformToolbarSetEnd;
    [ObservableProperty] private bool _showWaveformToolbarSetStartAndOffsetTheRest;
    [ObservableProperty] private bool _showWaveformToolbarVerticalZoom;
    [ObservableProperty] private bool _showWaveformToolbarHorizontalZoom;
    [ObservableProperty] private bool _showWaveformToolbarVideoPositionSlider;
    [ObservableProperty] private bool _showWaveformToolbarPlaybackSpeed;

    [ObservableProperty] private bool _waveformFocusTextboxAfterInsertNew;
    [ObservableProperty] private string _waveformSpaceInfo;
    [ObservableProperty] private string _libMpvPath;
    [ObservableProperty] private string _libMpvStatus;
    [ObservableProperty] private string _libVlcStatus;
    [ObservableProperty] private bool _isLibMpvDownloadVisible;
    [ObservableProperty] private bool _isLibVlcDownloadVisible;
    [ObservableProperty] private string _ffmpegPath;
    [ObservableProperty] private string _ffmpegStatus;
    [ObservableProperty] private Color _waveformColor;
    [ObservableProperty] private Color _waveformBackgroundColor;
    [ObservableProperty] private Color _waveformParagraphBackgroundColor;
    [ObservableProperty] private Color _waveformSelectedColor;
    [ObservableProperty] private Color _waveformParagraphSelectedBackgroundColor;
    [ObservableProperty] private Color _waveformCursorColor;
    [ObservableProperty] private Color _waveformFancyHighColor;
    [ObservableProperty] private bool _waveformInvertMouseWheel;
    [ObservableProperty] private bool _waveformSnapToShotChanges;
    [ObservableProperty] private bool _waveformShotChangesAutoGenerate;
    [ObservableProperty] private bool _waveformAllowOverlap;

    [ObservableProperty] private ObservableCollection<string> _waveformSingleClickActionTypes;
    [ObservableProperty] private string _selectedWaveformSingleClickActionType;

    [ObservableProperty] private ObservableCollection<string> _waveformDoubleClickActionTypes;
    [ObservableProperty] private string _selectedWaveformDoubleClickActionType;

    [ObservableProperty] private bool _waveformRightClickSelectsSubtitle;
    [ObservableProperty] private ObservableCollection<string> _themes;
    [ObservableProperty] private string _selectedTheme;
    [ObservableProperty] private ObservableCollection<string> _fontNames;
    [ObservableProperty] private string _selectedFontName;
    [ObservableProperty] private double _subtitleGridFontSize;
    [ObservableProperty] private bool _subtitleGridTextSingleLine;
    [ObservableProperty] private ObservableCollection<string> _subtitleGridFormattings;
    [ObservableProperty] private string _subtitleGridFormatting;
    [ObservableProperty] private string _subtitleTextBoxAndGridFontName;
    [ObservableProperty] private double _textBoxFontSize;
    [ObservableProperty] private bool _textBoxFontBold;
    [ObservableProperty] private bool _textBoxColorTags;
    [ObservableProperty] private bool _textBoxLiveSpellCheck;
    [ObservableProperty] private bool _textBoxCenterText;
    [ObservableProperty] private bool _showButtonHints;
    [ObservableProperty] private bool _gridCompactMode;
    [ObservableProperty] private bool _showAssaLayer;
    [ObservableProperty] private bool _showHorizontalLineAboveToolbar;
    [ObservableProperty] private ObservableCollection<GridLinesVisibilityDisplay> _gridLinesVisibilities;
    [ObservableProperty] private GridLinesVisibilityDisplay _selectedGridLinesVisibility;
    [ObservableProperty] private Color _darkModeForegroundColor;
    [ObservableProperty] private Color _darkModeBackgroundColor;
    [ObservableProperty] private bool _useFocusedButtonBackgroundColor;
    [ObservableProperty] private Color _focusedButtonBackgroundColor;
    [ObservableProperty] private Color _bookmarkColor;
    [ObservableProperty] private bool _isEditCustomContinuationStyleVisible;
    [ObservableProperty] private bool _isMpvChosen;

    [ObservableProperty] private bool _existsErrorLogFile;
    [ObservableProperty] private bool _existsWhisperLogFile;
    [ObservableProperty] private bool _existsSettingsFile;

    private int _continuationPause;
    private string _customContinuationStyleSuffix;
    private bool _customContinuationStyleSuffixApplyIfComma;
    private bool _customContinuationStyleSuffixAddSpace;
    private bool _customContinuationStyleSuffixReplaceComma;
    private string _customContinuationStylePrefix;
    private bool _customContinuationStylePrefixAddSpace;
    private bool _customContinuationStyleUseDifferentStyleGap;
    private string _customContinuationStyleGapSuffix;
    private bool _customContinuationStyleGapSuffixApplyIfComma;
    private bool _customContinuationStyleGapSuffixAddSpace;
    private bool _customContinuationStyleGapSuffixReplaceComma;
    private string _customContinuationStyleGapPrefix;
    private bool _customContinuationStyleGapPrefixAddSpace;

    public ObservableCollection<FileTypeAssociationViewModel> FileTypeAssociations { get; set; } = new()
    {
        new() { Extension = ".ass", IconPath = "avares://SubtitleEdit/Assets/FileTypes/ass.ico" },
        new() { Extension = ".dfxp", IconPath = "avares://SubtitleEdit/Assets/FileTypes/dfxp.ico" },
        new() { Extension = ".itt", IconPath = "avares://SubtitleEdit/Assets/FileTypes/itt.ico" },
        new() { Extension = ".lrc", IconPath = "avares://SubtitleEdit/Assets/FileTypes/lrc.ico" },
        new() { Extension = ".sbv", IconPath = "avares://SubtitleEdit/Assets/FileTypes/sbv.ico" },
        new() { Extension = ".smi", IconPath = "avares://SubtitleEdit/Assets/FileTypes/smi.ico" },
        new() { Extension = ".srt", IconPath = "avares://SubtitleEdit/Assets/FileTypes/srt.ico" },
        new() { Extension = ".ssa", IconPath = "avares://SubtitleEdit/Assets/FileTypes/ssa.ico" },
        new() { Extension = ".stl", IconPath = "avares://SubtitleEdit/Assets/FileTypes/stl.ico" },
        new() { Extension = ".sub", IconPath = "avares://SubtitleEdit/Assets/FileTypes/sub.ico" },
        new() { Extension = ".sup", IconPath = "avares://SubtitleEdit/Assets/FileTypes/sup.ico" },
        new() { Extension = ".vtt", IconPath = "avares://SubtitleEdit/Assets/FileTypes/vtt.ico" },
    };

    public bool OkPressed { get; set; }
    public Window? Window { get; internal set; }
    public ScrollViewer ScrollView { get; internal set; }
    public List<SettingsSection> Sections { get; internal set; }

    private readonly IWindowService _windowService;
    private readonly IFolderHelper _folderHelper;
    private MainViewModel? _mainViewModel;
    private List<ProfileDisplay> _profilesForEdit;
    private bool _skipRuleValueChanged = false;

    public SettingsViewModel(IWindowService windowService, IFolderHelper folderHelper)
    {
        _windowService = windowService;
        _folderHelper = folderHelper;

        Profiles = new ObservableCollection<string>();
        SelectedProfile = "Default";
        DialogStyles = new ObservableCollection<DialogStyleDisplay>(DialogStyleDisplay.List());
        ContinuationStyles = new ObservableCollection<ContinuationStyleDisplay>(ContinuationStyleDisplay.List());
        CpsLineLengthStrategies = new ObservableCollection<CpsLineLengthStrategyDisplay>(CpsLineLengthStrategyDisplay.List());
        SubtitleTextBoxAndGridFontName = "Default";
        DialogStyle = DialogStyles.First();
        ContinuationStyle = ContinuationStyles.First();
        CpsLineLengthStrategy = CpsLineLengthStrategies.First();
        Fonts = new ObservableCollection<string>(FontHelper.GetSystemFonts());
        MpvPreviewBorderTypes = new ObservableCollection<BorderStyleItem>(BorderStyleItem.List());
        LibVlcStatus = string.Empty;

        Themes = [Se.Language.General.System, Se.Language.General.Light, Se.Language.General.Dark, Se.Language.General.Classic, "Pastel"];
        SelectedTheme = Themes[0];

        FontNames = new ObservableCollection<string>(FontHelper.GetSystemFonts());
        FontNames.Insert(0, "Default");
        SelectedFontName = FontNames.First();

        ScrollView = new ScrollViewer();
        Sections = new List<SettingsSection>();

        VideoPlayers = new ObservableCollection<VideoPlayerItem>(VideoPlayerItem.ListVideoPlayerItem());
        SelectedVideoPlayer = VideoPlayers[0];

        SubtitleGridFormattings = new ObservableCollection<string>
        {
            Se.Language.Options.Settings.SubtitleGridFormattingNone,
            Se.Language.Options.Settings.SubtitleGridFormattingShowFormatting,
            Se.Language.Options.Settings.SubtitleGridFormattingShowTags,
        };
        SubtitleGridFormatting = SubtitleGridFormattings[0];

        WaveformDrawStyles = new ObservableCollection<string>
        {
            Se.Language.Waveform.WaveformDrawStyleClassic,
            Se.Language.Waveform.WaveformDrawStyleFancy,
        };
        SelectedWaveformDrawStyle = WaveformDrawStyles[0];

        WaveformSpectrogramStyles = new ObservableCollection<string>
        {
            Se.Language.Waveform.SpectrogramClassic,
            Se.Language.Waveform.SpectrogramClassicViridis,
            Se.Language.Waveform.SpectrogramClassicPlasma,
            Se.Language.Waveform.SpectrogramClassicInferno,
            Se.Language.Waveform.SpectrogramClassicTurbo,
        };
        SelectedWaveformSpectrogramStyle = WaveformSpectrogramStyles[0];

        WaveformSingleClickActionTypes =
        [
            Se.Language.Waveform.SetVideoPositionAndPauseAndSelectSubtitle,
            Se.Language.Waveform.SetVideopositionAndPauseAndSelectSubtitleAndCenter,
            Se.Language.Waveform.SetVideoPositionAndPause,
            Se.Language.Waveform.SetVideopositionAndPauseAndCenter,
            Se.Language.Waveform.SetVideoposition,
        ];
        SelectedWaveformSingleClickActionType = WaveformSingleClickActionTypes[0];

        WaveformDoubleClickActionTypes =
        [
            Se.Language.General.None,
            Se.Language.General.SelectSubtitle,
            Se.Language.General.Center,
            Se.Language.General.Pause,
            Se.Language.General.Play,
        ];
        SelectedWaveformDoubleClickActionType = WaveformDoubleClickActionTypes[0];

        var subtitleFormats = SubtitleFormat.AllSubtitleFormats;
        var defaultSubtitleFormats = new List<string>();
        var saveSubtitleFormats = new List<string> { "Auto" };
        foreach (var format in subtitleFormats)
        {
            if (format.Name.StartsWith("Unknown", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            defaultSubtitleFormats.Add(format.FriendlyName);
            saveSubtitleFormats.Add(format.FriendlyName);
        }

        DefaultSubtitleFormats = new ObservableCollection<string>(defaultSubtitleFormats);
        SaveSubtitleFormats = new ObservableCollection<string>(saveSubtitleFormats);
        FavoriteSubtitleFormats = new ObservableCollection<string>();
        SelectedDefaultSubtitleFormat = DefaultSubtitleFormats.First();
        SelectedSaveSubtitleFormat = SaveSubtitleFormats.First();
        Encodings = new ObservableCollection<TextEncoding>(EncodingHelper.GetEncodings());
        DefaultEncoding = Encodings.First();

        SubtitleEnterKeyActionTypes =
        [
            Se.Language.Options.Settings.GridGoToSubtitleAndSetVideoPosition,
            Se.Language.Options.Settings.GridGoToNextLine,
        ];
        SelectedSubtitleEnterKeyActionType = SubtitleEnterKeyActionTypes[0];

        SubtitleSingleClickActionTypes =
        [
            Se.Language.General.None,
            Se.Language.Options.Settings.GridGoToSubtitleOnlyWaveformOnly,
            Se.Language.Options.Settings.GridGoToSubtitleAndPause,
            Se.Language.Options.Settings.GridGoToSubtitleAndPlay,
            Se.Language.Options.Settings.GridGoToSubtitleAndSetVideoPosition,
            Se.Language.Options.Settings.GridGoToSubtitleAndPauseAndFocusTextBox
        ];
        SelectedSubtitleSingleClickActionType = SubtitleSingleClickActionTypes[0];

        SubtitleDoubleClickActionTypes =
        [
            Se.Language.General.None,
            Se.Language.Options.Settings.GridGoToSubtitleAndPause,
            Se.Language.Options.Settings.GridGoToSubtitleAndPlay,
            Se.Language.Options.Settings.GridGoToSubtitleAndSetVideoPosition,
            Se.Language.Options.Settings.GridGoToSubtitleAndPauseAndFocusTextBox
        ];
        SelectedSubtitleDoubleClickActionType = SubtitleDoubleClickActionTypes[0];

        SaveAsBehaviorTypes =
        [
            Se.Language.Options.Settings.SaveAsBehaviorUseSubtitleVideoFilename,
            Se.Language.Options.Settings.SaveAsBehaviorUseVideoSubtitleFilename,
            Se.Language.Options.Settings.SaveAsBehaviorUseVideoFileName,
            Se.Language.Options.Settings.SaveAsBehaviorUseSubtitleFileName,
            Se.Language.General.Name,
        ];
        SelectedSaveAsBehaviorType = SaveAsBehaviorTypes[0];

        SaveAsAppendLanguageCode =
        [
            Se.Language.General.None,
            Se.Language.Options.Settings.SaveAsAppendLanguageCodeTwoLetter,
            Se.Language.Options.Settings.SaveAsAppendLanguageCodeThreeLetter,
            Se.Language.Options.Settings.SaveAsAppendLanguageCodeLanguageName,
        ];
        SelectedSaveAsAppendLanguageCode = SaveAsAppendLanguageCode[0];

        SplitOddNumberOfLinesActions =
        [
            Se.Language.General.Smart,
            Se.Language.Options.Settings.SplitOddLineActionWeightTop,
            Se.Language.Options.Settings.SplitOddLineActionWeightBottom,
        ];
        SelectedSplitOddNumberOfLinesAction = SplitOddNumberOfLinesActions[0];

        WaveformSpaceInfo = string.Empty;
        IsMpvChosen = true;

        GridLinesVisibilities = new ObservableCollection<GridLinesVisibilityDisplay>(GridLinesVisibilityDisplay.GetAll());
        SelectedGridLinesVisibility = GridLinesVisibilities[0];

        ErrorColor = Color.FromArgb(50, 255, 0, 0);

        FfmpegStatus = Se.Language.General.NotInstalled;
        FfmpegPath = string.Empty;

        LibMpvStatus = Se.Language.General.NotInstalled;
        LibMpvPath = string.Empty;
        IsLibMpvDownloadVisible = OperatingSystem.IsWindows();
        IsLibVlcDownloadVisible = OperatingSystem.IsMacOS() && RuntimeInformation.ProcessArchitecture == Architecture.X64 ||
                                  OperatingSystem.IsWindows() ||
                                  OperatingSystem.IsLinux();

        MpvPreviewFontName = FontNames.First();
        MpvPreviewSelectedBorderType = MpvPreviewBorderTypes.First();

        _profilesForEdit = new List<ProfileDisplay>();

        _customContinuationStyleGapPrefix = string.Empty;
        _customContinuationStyleGapPrefixAddSpace = false;
        _customContinuationStyleGapSuffix = string.Empty;
        _customContinuationStyleGapSuffixAddSpace = false;
        _customContinuationStyleGapSuffixApplyIfComma = false;
        _customContinuationStyleGapSuffixReplaceComma = false;
        _customContinuationStylePrefix = string.Empty;
        _customContinuationStylePrefixAddSpace = false;
        _customContinuationStyleSuffix = string.Empty;
        _customContinuationStyleSuffixAddSpace = false;
        _customContinuationStyleSuffixApplyIfComma = false;
        _customContinuationStyleSuffixReplaceComma = false;

        LoadSettings();
    }

    private void LoadSettings()
    {
        var general = Se.Settings.General;
        var appearance = Se.Settings.Appearance;

        _profilesForEdit.Clear();
        foreach (var profile in general.Profiles)
        {
            var pd = new ProfileDisplay
            {
                Name = profile.Name,
                SingleLineMaxLength = profile.SubtitleLineMaximumLength,
                OptimalCharsPerSec = (double)profile.SubtitleOptimalCharactersPerSeconds,
                MaxCharsPerSec = (double)profile.SubtitleMaximumCharactersPerSeconds,
                MaxWordsPerMin = (double)profile.SubtitleMaximumWordsPerMinute,
                MinDurationMs = profile.SubtitleMinimumDisplayMilliseconds,
                MaxDurationMs = profile.SubtitleMaximumDisplayMilliseconds,
                MinGapMs = profile.MinimumMillisecondsBetweenLines,
                MaxLines = profile.MaxNumberOfLines,
                UnbreakLinesShorterThan = profile.MergeLinesShorterThan,
                DialogStyle = DialogStyles.FirstOrDefault(p => p.Code == profile.DialogStyle.ToString()) ?? DialogStyles.First(),
                ContinuationStyle = ContinuationStyles.FirstOrDefault(p => p.Code == profile.ContinuationStyle.ToString()) ?? ContinuationStyles.First(),
                CpsLineLengthStrategy = CpsLineLengthStrategies.FirstOrDefault(p => p.Code == profile.CpsLineLengthStrategy) ?? CpsLineLengthStrategies.First()
            };
            _profilesForEdit.Add(pd);
        }

        Profiles.Clear();
        foreach (var profile in Se.Settings.General.Profiles)
        {
            Profiles.Add(profile.Name);
        }

        if (Profiles.Count == 0)
        {
            Profiles.Add("Default");
        }

        SelectedProfile = general.CurrentProfile;
        if (!Profiles.Contains(SelectedProfile))
        {
            SelectedProfile = Profiles.First();
        }

        SingleLineMaxLength = general.SubtitleLineMaximumLength;
        OptimalCharsPerSec = general.SubtitleOptimalCharactersPerSeconds;
        MaxCharsPerSec = general.SubtitleMaximumCharactersPerSeconds;
        MaxWordsPerMin = general.SubtitleMaximumWordsPerMinute;
        MinDurationMs = general.SubtitleMinimumDisplayMilliseconds;
        MaxDurationMs = general.SubtitleMaximumDisplayMilliseconds;
        MinGapMs = general.MinimumMillisecondsBetweenLines;
        MaxLines = general.MaxNumberOfLines;
        UnbreakLinesShorterThan = general.UnbreakLinesShorterThan;
        DialogStyle = DialogStyles.FirstOrDefault(p => p.Code == general.DialogStyle) ?? DialogStyles.First();
        ContinuationStyle = ContinuationStyles.FirstOrDefault(p => p.Code == general.ContinuationStyle) ?? ContinuationStyles.First();
        CpsLineLengthStrategy = CpsLineLengthStrategies.FirstOrDefault(p => p.Code == general.CpsLineLengthStrategy) ?? CpsLineLengthStrategies.First();

        UseFrameMode = general.UseFrameMode;
        TextBoxLimitNewLines = general.SubtitleTextBoxLimitNewLines;
        NewEmptyDefaultMs = general.NewEmptyDefaultMs;
        PromptDeleteLines = general.PromptDeleteLines;
        LockTimeCodes = general.LockTimeCodes;
        RememberPositionAndSize = general.RememberPositionAndSize;
        AutoBackupOn = general.AutoBackupOn;
        AutoBackupIntervalMinutes = general.AutoBackupIntervalMinutes;
        AutoBackupDeleteAfterDays = general.AutoBackupDeleteAfterDays;
        DefaultEncoding = Encodings.FirstOrDefault(e => e.DisplayName == general.DefaultEncoding) ?? Encodings.First();
        SelectedSubtitleEnterKeyActionType = MapFromSelectedSubtitleEnterKeyAction(Se.Settings.General.SubtitleEnterKeyAction);
        SelectedSubtitleSingleClickActionType = MapFromSelectedSubtitleSingleClickAction(Se.Settings.General.SubtitleSingleClickAction);
        SelectedSubtitleDoubleClickActionType = MapFromSelectedSubtitleDoubleClickAction(Se.Settings.General.SubtitleDoubleClickAction);
        SelectedSaveAsBehaviorType = MapFromSelectedSaveAsBehavior(Se.Settings.General.SaveAsBehavior);
        SelectedSaveAsAppendLanguageCode = MapFromSelectedSaveAsAppendLanguageCode(Se.Settings.General.SaveAsAppendLanguageCode);
        AutoConvertToUtf8 = general.AutoConvertToUtf8;
        ForceCrLfOnSave = general.ForceCrLfOnSave;
        AutoTrimWhiteSpace = general.AutoTrimWhiteSpace;

        SelectedDefaultSubtitleFormat = general.DefaultSubtitleFormat;
        if (!DefaultSubtitleFormats.Contains(SelectedDefaultSubtitleFormat))
        {
            SelectedDefaultSubtitleFormat = DefaultSubtitleFormats.FirstOrDefault() ?? string.Empty;
        }

        SelectedSaveSubtitleFormat = general.DefaultSaveAsFormat;
        if (!SaveSubtitleFormats.Contains(SelectedSaveSubtitleFormat))
        {
            SelectedSaveSubtitleFormat = SaveSubtitleFormats.FirstOrDefault() ?? string.Empty;
        }

        if (!string.IsNullOrEmpty(general.FavoriteSubtitleFormats))
        {
            var favoriteFormats = general.FavoriteSubtitleFormats.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            FavoriteSubtitleFormats.Clear();
            foreach (var format in favoriteFormats)
            {
                if (SaveSubtitleFormats.Contains(format))
                {
                    FavoriteSubtitleFormats.Add(format);
                }
            }
        }

        GoToLineNumberAlsoSetVideoPosition = Se.Settings.Tools.GoToLineNumberAlsoSetVideoPosition;
        AdjustAllTimesRememberLineSelectionChoice = Se.Settings.Synchronization.AdjustAllTimesRememberLineSelectionChoice;
        SelectedSplitOddNumberOfLinesAction = MapFromSplitOddActionToLanguageCode(Se.Settings.Tools.SplitOddLinesAction);
        OcrUseWordSplitList = Se.Settings.Ocr.UseWordSplitList;

        SelectedTheme = MapThemeToTranslation(appearance.Theme);
        SelectedFontName = FontNames.FirstOrDefault(p => p == appearance.FontName) ?? FontNames.First();
        ShowToolbarNew = appearance.ToolbarShowFileNew;
        ShowToolbarOpen = appearance.ToolbarShowFileOpen;
        ShowToolbarSave = appearance.ToolbarShowSave;
        ShowToolbarSaveAs = appearance.ToolbarShowSaveAs;
        ShowToolbarFind = appearance.ToolbarShowFind;
        ShowToolbarReplace = appearance.ToolbarShowReplace;
        ShowToolbarSpellCheck = appearance.ToolbarShowSpellCheck;
        ShowToolbarSettings = appearance.ToolbarShowSettings;
        ShowToolbarLayout = appearance.ToolbarShowLayout;
        ShowToolbarHelp = appearance.ToolbarShowHelp;
        ShowToolbarEncoding = appearance.ToolbarShowEncoding;
        ShowToolbarFrameRate = appearance.ToolbarShowFrameRate;
        SubtitleGridFontSize = appearance.SubtitleGridFontSize;
        SubtitleGridTextSingleLine = appearance.SubtitleGridTextSingleLine;
        SubtitleGridFormatting = MapGridFormattingToText(appearance.SubtitleGridFormattingType);
        SubtitleTextBoxAndGridFontName = appearance.SubtitleTextBoxAndGridFontName;
        TextBoxFontSize = appearance.SubtitleTextBoxFontSize;
        TextBoxFontBold = appearance.SubtitleTextBoxFontBold;
        TextBoxColorTags = appearance.SubtitleTextBoxColorTags;
        TextBoxLiveSpellCheck = appearance.SubtitleTextBoxLiveSpellCheck;
        TextBoxCenterText = appearance.SubtitleTextBoxCenterText;
        TextBoxButtonShowAutoBreak = appearance.TextBoxShowButtonAutoBreak;
        TextBoxButtonShowUnbreak = appearance.TextBoxShowButtonUnbreak;
        TextBoxButtonShowItalic = appearance.TextBoxShowButtonItalic;
        TextBoxButtonShowColor = appearance.TextBoxShowButtonColor;
        TextBoxButtonShowRemoveFormatting = appearance.TextBoxShowButtonRemoveFormatting;
        ShowButtonHints = appearance.ShowHints;
        GridCompactMode = appearance.GridCompactMode;
        ShowAssaLayer = appearance.ShowLayer;
        ShowHorizontalLineAboveToolbar = appearance.ShowHorizontalLineAboveToolbar;
        SelectedGridLinesVisibility = GridLinesVisibilities.FirstOrDefault(p => p.Type.ToString() == appearance.GridLinesAppearance) ?? GridLinesVisibilities[0];
        DarkModeBackgroundColor = appearance.DarkModeBackgroundColor.FromHexToColor();
        DarkModeForegroundColor = appearance.DarkModeForegroundColor.FromHexToColor();
        UseFocusedButtonBackgroundColor = appearance.UseFocusedButtonBackgroundColor;
        FocusedButtonBackgroundColor = appearance.FocusedButtonBackgroundColor.FromHexToColor();
        BookmarkColor = appearance.BookmarkColor.FromHexToColor();
        ShowUpDownStartTime = appearance.ShowUpDownStartTime;
        ShowUpDownEndTime = appearance.ShowUpDownEndTime;
        ShowUpDownDuration = appearance.ShowUpDownDuration;
        ShowUpDownLabels = appearance.ShowUpDownLabels;

        WaveformDrawGridLines = Se.Settings.Waveform.DrawGridLines;
        WaveformFocusOnMouseOver = Se.Settings.Waveform.FocusOnMouseOver;
        WaveformCenterVideoPosition = Se.Settings.Waveform.CenterVideoPosition;
        WaveformShowToolbar = Se.Settings.Waveform.ShowToolbar;

        if (Se.Settings.Waveform.WaveformDrawStyle == WaveformDrawStyle.Classic.ToString())
        {
            SelectedWaveformDrawStyle = WaveformDrawStyles[0];
        }
        else if (Se.Settings.Waveform.WaveformDrawStyle == WaveformDrawStyle.Fancy.ToString())
        {
            SelectedWaveformDrawStyle = WaveformDrawStyles[1];
        }
        else
        {
            SelectedWaveformDrawStyle = WaveformDrawStyles[0];
        }

        WaveformGenerateSpectrogram = Se.Settings.Waveform.GenerateSpectrogram;
        if (Se.Settings.Waveform.SpectrogramStyle == SeSpectrogramStyle.Classic.ToString())
        {
            SelectedWaveformSpectrogramStyle = WaveformSpectrogramStyles[0];
        }
        else if (Se.Settings.Waveform.SpectrogramStyle == SeSpectrogramStyle.ClassicViridis.ToString())
        {
            SelectedWaveformSpectrogramStyle = WaveformSpectrogramStyles[1];
        }
        else if (Se.Settings.Waveform.SpectrogramStyle == SeSpectrogramStyle.ClassicPlasma.ToString())
        {
            SelectedWaveformSpectrogramStyle = WaveformSpectrogramStyles[2];
        }
        else if (Se.Settings.Waveform.SpectrogramStyle == SeSpectrogramStyle.ClassicInferno.ToString())
        {
            SelectedWaveformSpectrogramStyle = WaveformSpectrogramStyles[3];
        }
        else if (Se.Settings.Waveform.SpectrogramStyle == SeSpectrogramStyle.ClassicTurbo.ToString())
        {
            SelectedWaveformSpectrogramStyle = WaveformSpectrogramStyles[4];
        }
        else
        {
            SelectedWaveformSpectrogramStyle = WaveformSpectrogramStyles[0];
        }

        WaveformSpectrogramCombinedWaveformHeight = Se.Settings.Waveform.SpectrogramCombinedWaveformHeight;

        ShowWaveformToolbarPlay = Se.Settings.Waveform.ShowToolbarPlay;
        ShowWaveformToolbarPlayNext = Se.Settings.Waveform.ShowToolbarPlayNext;
        ShowWaveformToolbarPlaySelection = Se.Settings.Waveform.ShowToolbarPlaySelection;
        ShowWaveformToolbarRepeat = Se.Settings.Waveform.ShowToolbarRepeat;
        ShowWaveformToolbarRemoveBlankLines = Se.Settings.Waveform.ShowToolbarRemoveBlankLines;
        ShowWaveformToolbarNew = Se.Settings.Waveform.ShowToolbarNew;
        ShowWaveformToolbarSetStart = Se.Settings.Waveform.ShowToolbarSetStart;
        ShowWaveformToolbarSetEnd = Se.Settings.Waveform.ShowToolbarSetEnd;
        ShowWaveformToolbarSetStartAndOffsetTheRest = Se.Settings.Waveform.ShowToolbarSetStartAndOffsetTheRest;
        ShowWaveformToolbarVerticalZoom = Se.Settings.Waveform.ShowToolbarVerticalZoom;
        ShowWaveformToolbarHorizontalZoom = Se.Settings.Waveform.ShowToolbarHorizontalZoom;
        ShowWaveformToolbarVideoPositionSlider = Se.Settings.Waveform.ShowToolbarVideoPositionSlider;
        ShowWaveformToolbarPlaybackSpeed = Se.Settings.Waveform.ShowToolbarPlaybackSpeed;
        WaveformFocusTextboxAfterInsertNew = Se.Settings.Waveform.FocusTextBoxAfterInsertNew;
        WaveformColor = Se.Settings.Waveform.WaveformColor.FromHexToColor();
        WaveformBackgroundColor = Se.Settings.Waveform.WaveformBackgroundColor.FromHexToColor();
        WaveformParagraphBackgroundColor = Se.Settings.Waveform.ParagraphBackground.FromHexToColor();
        WaveformSelectedColor = Se.Settings.Waveform.WaveformSelectedColor.FromHexToColor();
        WaveformParagraphSelectedBackgroundColor = Se.Settings.Waveform.ParagraphSelectedBackground.FromHexToColor();
        WaveformCursorColor = Se.Settings.Waveform.WaveformCursorColor.FromHexToColor();
        WaveformFancyHighColor = Se.Settings.Waveform.WaveformFancyHighColor.FromHexToColor();
        WaveformInvertMouseWheel = Se.Settings.Waveform.InvertMouseWheel;
        WaveformSnapToShotChanges = Se.Settings.Waveform.SnapToShotChanges;
        WaveformShotChangesAutoGenerate = Se.Settings.Waveform.ShotChangesAutoGenerate;
        WaveformAllowOverlap = Se.Settings.Waveform.AllowOverlap;

        SelectedWaveformSingleClickActionType = MapWaveformSingleClickToTranslation(Se.Settings.Waveform.SingleClickAction);
        SelectedWaveformDoubleClickActionType = MapWaveformDoubleClickToTranslation(Se.Settings.Waveform.DoubleClickAction);

        WaveformRightClickSelectsSubtitle = Se.Settings.Waveform.RightClickSelectsSubtitle;

        ColorDurationTooLong = general.ColorDurationTooLong;
        ColorDurationTooShort = general.ColorDurationTooShort;
        ColorTextTooLong = general.ColorTextTooLong;
        ColorTextTooWide = general.ColorTextTooWide;
        ColorTextTooManyLines = general.ColorTextTooManyLines;
        ColorOverlap = general.ColorTimeCodeOverlap;
        ColorGapTooShort = general.ColorGapTooShort;
        ErrorColor = general.ErrorColor.FromHexToColor();

        _customContinuationStyleSuffix = general.CustomContinuationStyleSuffix;
        _customContinuationStyleSuffixApplyIfComma = general.CustomContinuationStyleSuffixApplyIfComma;
        _customContinuationStyleSuffixAddSpace = general.CustomContinuationStyleSuffixAddSpace;
        _customContinuationStyleSuffixReplaceComma = general.CustomContinuationStyleSuffixReplaceComma;
        _customContinuationStylePrefix = general.CustomContinuationStylePrefix;
        _customContinuationStylePrefixAddSpace = general.CustomContinuationStylePrefixAddSpace;
        _customContinuationStyleUseDifferentStyleGap = general.CustomContinuationStyleUseDifferentStyleGap;
        _continuationPause = general.ContinuationPause;
        _customContinuationStyleGapSuffix = general.CustomContinuationStyleGapSuffix;
        _customContinuationStyleGapSuffixApplyIfComma = general.CustomContinuationStyleGapSuffixApplyIfComma;
        _customContinuationStyleGapSuffixAddSpace = general.CustomContinuationStyleGapSuffixAddSpace;
        _customContinuationStyleGapSuffixReplaceComma = general.CustomContinuationStyleGapSuffixReplaceComma;
        _customContinuationStyleGapPrefix = general.CustomContinuationStyleGapPrefix;
        _customContinuationStyleGapPrefixAddSpace = general.CustomContinuationStyleGapPrefixAddSpace;

        var video = Se.Settings.Video;
        var videoPlayer = VideoPlayers.FirstOrDefault(p => p.Code == video.VideoPlayer);
        if (videoPlayer != null)
        {
            SelectedVideoPlayer = videoPlayer;
        }

        ShowStopButton = video.ShowStopButton;
        ShowFullscreenButton = video.ShowFullscreenButton;
        AutoOpenVideoFile = video.AutoOpen;

        MpvPreviewFontName = video.MpvPreviewFontName;
        MpvPreviewFontSize = video.MpvPreviewFontSize;
        MpvPreviewFontBold = video.MpvPreviewFontBold;
        MpvPreviewOutlineWidth = video.MpvPreviewOutlineWidth;
        MpvPreviewShadowWidth = video.MpvPreviewShadowWidth;
        MpvPreviewColorPrimary = video.MpvPreviewColorPrimary.FromHexToColor();
        MpvPreviewColorOutline = video.MpvPreviewColorOutline.FromHexToColor();
        MpvPreviewColorShadow = video.MpvPreviewColorShadow.FromHexToColor();
        MpvPreviewSelectedBorderType = MpvPreviewBorderTypes.FirstOrDefault(p => p.Style == (BorderStyleType)video.MpvPreviewBorderType) ?? MpvPreviewBorderTypes.First();

        FfmpegPath = Se.Settings.General.FfmpegPath;
        LibMpvPath = Se.Settings.General.LibMpvPath;
        SetFfmpegStatus();
        SetLibMpvStatus();
        SetLibVlcStatus();
        LoadFileTypeAssociations();

        ExistsErrorLogFile = File.Exists(Se.GetErrorLogFilePath());
        ExistsWhisperLogFile = File.Exists(Se.GetWhisperLogFilePath());
        ExistsSettingsFile = File.Exists(Se.GetSettingsFilePath());
    }

    private static readonly Dictionary<string, string> _waveformSingleClickActionToTextMap = new Dictionary<string, string>
    {
        { WaveformSingleClickActionType.SetVideoPositionAndPauseAndSelectSubtitle.ToString(), Se.Language.Waveform.SetVideoPositionAndPauseAndSelectSubtitle },
        { WaveformSingleClickActionType.SetVideopositionAndPauseAndSelectSubtitleAndCenter.ToString(), Se.Language.Waveform.SetVideopositionAndPauseAndSelectSubtitleAndCenter },
        { WaveformSingleClickActionType.SetVideoPositionAndPause.ToString(), Se.Language.Waveform.SetVideoPositionAndPause },
        { WaveformSingleClickActionType.SetVideopositionAndPauseAndCenter.ToString(), Se.Language.Waveform.SetVideopositionAndPauseAndCenter },
        { WaveformSingleClickActionType.SetVideoposition.ToString(), Se.Language.Waveform.SetVideoposition },
    };

    private static Dictionary<string, string> WaveformSingleClickTextToActionMap => _waveformSingleClickActionToTextMap.ToDictionary(x => x.Value, x => x.Key);

    private string MapWaveformSingleClickToTranslation(string singleClickAction)
    {
        if (string.IsNullOrEmpty(singleClickAction))
        {
            return Se.Language.Waveform.SetVideoPositionAndPauseAndSelectSubtitle;
        }

        return _waveformSingleClickActionToTextMap.TryGetValue(singleClickAction, out var text)
            ? text
            : Se.Language.Waveform.SetVideoPositionAndPauseAndSelectSubtitle;
    }

    private static readonly Dictionary<string, string> _waveformDoubleClickActionToTextMap = new Dictionary<string, string>
    {
        { WaveformDoubleClickActionType.None.ToString(), Se.Language.General.None },
        { WaveformDoubleClickActionType.SelectSubtitle.ToString(), Se.Language.General.SelectSubtitle },
        { WaveformDoubleClickActionType.Center.ToString(), Se.Language.General.Center },
        { WaveformDoubleClickActionType.Pause.ToString(), Se.Language.General.Pause },
        { WaveformDoubleClickActionType.Play.ToString(), Se.Language.General.Play },
    };

    private static Dictionary<string, string> WaveformDoubleClickTextToActionMap => _waveformDoubleClickActionToTextMap.ToDictionary(x => x.Value, x => x.Key);

    private string MapWaveformDoubleClickToTranslation(string doubleClickAction)
    {
        if (string.IsNullOrEmpty(doubleClickAction))
        {
            return Se.Language.General.None;
        }

        return _waveformDoubleClickActionToTextMap.TryGetValue(doubleClickAction, out var text)
            ? text
            : Se.Language.General.None;
    }

    private string GetTranslationModifierFromSetting(string modifier)
    {
        if (modifier == "Control" || modifier == "Ctrl")
        {
            return OperatingSystem.IsMacOS() ? Se.Language.Options.Shortcuts.ControlMac : Se.Language.Options.Shortcuts.Control;
        }
        else if (modifier == "Shift")
        {
            return Se.Language.Options.Shortcuts.Shift;
        }
        else if (modifier == "Alt")
        {
            return Se.Language.Options.Shortcuts.Alt;
        }
        else
        {
            return string.Empty;
        }
    }

    private static string MapFromSplitOddActionToLanguageCode(string splitAction)
    {
        if (splitAction == nameof(SplitOddLinesActionType.WeightTop))
        {
            return Se.Language.Options.Settings.SplitOddLineActionWeightTop;
        }

        if (splitAction == nameof(SplitOddLinesActionType.WeightBottom))
        {
            return Se.Language.Options.Settings.SplitOddLineActionWeightBottom;
        }

        return Se.Language.General.Smart;
    }

    private static string MapFromSelectedSaveAsBehavior(string saveAsBehavior)
    {
        if (saveAsBehavior == nameof(SaveAsBehaviourType.UseSubtitleFileNameThenVideoFileName))
        {
            return Se.Language.Options.Settings.SaveAsBehaviorUseSubtitleVideoFilename;
        }

        if (saveAsBehavior == nameof(SaveAsBehaviourType.UseVideoFileNameThenSubtitleFileName))
        {
            return Se.Language.Options.Settings.SaveAsBehaviorUseVideoSubtitleFilename;
        }

        if (saveAsBehavior == nameof(SaveAsBehaviourType.UseVideoFileName))
        {
            return Se.Language.Options.Settings.SaveAsBehaviorUseVideoFileName;
        }

        if (saveAsBehavior == nameof(SaveAsBehaviourType.UseSubtitleFileName))
        {
            return Se.Language.Options.Settings.SaveAsBehaviorUseSubtitleFileName;
        }

        return Se.Language.General.Default;
    }

    private static string MapFromSelectedSaveAsAppendLanguageCode(string languageAppendType)
    {
        if (languageAppendType == nameof(SaveAsLanguageAppendType.TwoLetterLanguageCode))
        {
            return Se.Language.Options.Settings.SaveAsAppendLanguageCodeTwoLetter;
        }

        if (languageAppendType == nameof(SaveAsLanguageAppendType.ThreeLEtterLanguageCode))
        {
            return Se.Language.Options.Settings.SaveAsAppendLanguageCodeThreeLetter;
        }

        if (languageAppendType == nameof(SaveAsLanguageAppendType.FullLanguageName))
        {
            return Se.Language.Options.Settings.SaveAsAppendLanguageCodeLanguageName;
        }

        return Se.Language.General.None;
    }

    private static string MapThemeFromTranslation(string translation)
    {
        if (translation == Se.Language.General.System)
        {
            return UiTheme.ThemeNameSystem;
        }
        else if (translation == Se.Language.General.Light)
        {
            return UiTheme.ThemeNameLight;
        }
        else if (translation == Se.Language.General.Dark)
        {
            return UiTheme.ThemeNameDark;
        }
        else if (translation == Se.Language.General.Classic)
        {
            return UiTheme.ThemeNameClassic;
        }
        else if (translation == "Pastel")
        {
            return UiTheme.ThemeNamePastel;
        }
        else
        {
            return UiTheme.ThemeNameSystem;
        }
    }

    private string MapThemeToTranslation(string theme)
    {
        if (theme == UiTheme.ThemeNameSystem)
        {
            return Se.Language.General.System;
        }
        else if (theme == UiTheme.ThemeNameLight)
        {
            return Se.Language.General.Light;
        }
        else if (theme == UiTheme.ThemeNameDark)
        {
            return Se.Language.General.Dark;
        }
        else if (theme == UiTheme.ThemeNameClassic)
        {
            return Se.Language.General.Classic;
        }
        else if (theme == UiTheme.ThemeNamePastel)
        {
            return "Pastel";
        }
        else
        {
            return Se.Language.General.System;
        }
    }

    private static string MapGridFormattingToText(int subtitleGridFormattingType)
    {
        if (subtitleGridFormattingType == (int)SubtitleGridFormattingTypes.ShowFormatting)
        {
            return Se.Language.Options.Settings.SubtitleGridFormattingShowFormatting;
        }
        else if (subtitleGridFormattingType == (int)SubtitleGridFormattingTypes.ShowTags)
        {
            return Se.Language.Options.Settings.SubtitleGridFormattingShowTags;
        }
        else
        {
            return Se.Language.Options.Settings.SubtitleGridFormattingNone;
        }
    }

    private static int MapGridFormattingToCode(string translation)
    {
        if (translation == Se.Language.Options.Settings.SubtitleGridFormattingShowFormatting)
        {
            return (int)SubtitleGridFormattingTypes.ShowFormatting;
        }
        else if (translation == Se.Language.Options.Settings.SubtitleGridFormattingShowTags)
        {
            return (int)SubtitleGridFormattingTypes.ShowTags;
        }
        else
        {
            return (int)SubtitleGridFormattingTypes.NoFormatting;
        }
    }

    private static readonly Dictionary<string, string> _keyEnterActionToTextMap = new Dictionary<string, string>
    {
        { SubtitleEnterKeyActionType.GoToSubtitleAndSetVideoPosition.ToString(), Se.Language.Options.Settings.GridGoToSubtitleAndSetVideoPosition },
        { SubtitleEnterKeyActionType.GoToNextLine.ToString(), Se.Language.Options.Settings.GridGoToNextLine },
    };

    private static Dictionary<string, string> KeyEnterTextToActionMap => _keyEnterActionToTextMap.ToDictionary(x => x.Value, x => x.Key);

    private static string MapFromSelectedSubtitleEnterKeyAction(string action)
    {
        if (string.IsNullOrEmpty(action))
        {
            return Se.Language.General.None;
        }

        return _keyEnterActionToTextMap.TryGetValue(action, out var text)
            ? text
            : Se.Language.General.None; ;
    }

    public static string MapToSelectedSubtitleEnterKeyAction(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return SubtitleSingleClickActionType.None.ToString();
        }

        return KeyEnterTextToActionMap.TryGetValue(text, out var action)
            ? action
            : SubtitleSingleClickActionType.None.ToString();
    }

    private static readonly Dictionary<string, string> _singleClickActionToTextMap = new Dictionary<string, string>
    {
        { SubtitleSingleClickActionType.None.ToString(), Se.Language.General.None },
        { SubtitleSingleClickActionType.GoToWaveformOnlyNoVideoPosition.ToString(), Se.Language.Options.Settings.GridGoToSubtitleOnlyWaveformOnly },
        { SubtitleSingleClickActionType.GoToSubtitleAndPause.ToString(), Se.Language.Options.Settings.GridGoToSubtitleAndPause },
        { SubtitleSingleClickActionType.GoToSubtitleAndPlay.ToString(), Se.Language.Options.Settings.GridGoToSubtitleAndPlay },
        { SubtitleSingleClickActionType.GoToSubtitleAndSetVideoPosition.ToString(), Se.Language.Options.Settings.GridGoToSubtitleAndSetVideoPosition },
        { SubtitleSingleClickActionType.GoToSubtitleAndPauseAndFocusTextBox.ToString(), Se.Language.Options.Settings.GridGoToSubtitleAndPauseAndFocusTextBox },
    };

    private static Dictionary<string, string> SingleClickTextToActionMap => _singleClickActionToTextMap.ToDictionary(x => x.Value, x => x.Key);

    private static string MapFromSelectedSubtitleSingleClickAction(string action)
    {
        if (string.IsNullOrEmpty(action))
        {
            return Se.Language.General.None;
        }

        return _singleClickActionToTextMap.TryGetValue(action, out var text)
            ? text
            : Se.Language.General.None; ;
    }

    public static string MapToSelectedSubtitleSingleClickAction(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return SubtitleSingleClickActionType.None.ToString();
        }

        return SingleClickTextToActionMap.TryGetValue(text, out var action)
            ? action
            : SubtitleSingleClickActionType.None.ToString();
    }

    private static readonly Dictionary<string, string> _actionToTextMap = new Dictionary<string, string>
    {
        { SubtitleDoubleClickActionType.None.ToString(), Se.Language.General.None },
        { SubtitleDoubleClickActionType.GoToSubtitleAndPause.ToString(), Se.Language.Options.Settings.GridGoToSubtitleAndPause },
        { SubtitleDoubleClickActionType.GoToSubtitleAndPlay.ToString(), Se.Language.Options.Settings.GridGoToSubtitleAndPlay },
        { SubtitleDoubleClickActionType.GoToSubtitleOnly.ToString(), Se.Language.Options.Settings.GridGoToSubtitleAndSetVideoPosition },
        { SubtitleDoubleClickActionType.GoToSubtitleAndPauseAndFocusTextBox.ToString(), Se.Language.Options.Settings.GridGoToSubtitleAndPauseAndFocusTextBox },
    };

    private static Dictionary<string, string> TextToActionMap => _actionToTextMap.ToDictionary(x => x.Value, x => x.Key);

    private static string MapFromSelectedSubtitleDoubleClickAction(string action)
    {
        if (string.IsNullOrEmpty(action))
        {
            return Se.Language.Options.Settings.GridGoToSubtitleAndPause;
        }

        return _actionToTextMap.TryGetValue(action, out var text)
            ? text
            : Se.Language.Options.Settings.GridGoToSubtitleAndPause;
    }

    public static string MapToSelectedSubtitleDoubleClickAction(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return SubtitleDoubleClickActionType.GoToSubtitleAndPause.ToString();
        }

        return TextToActionMap.TryGetValue(text, out var action)
            ? action
            : SubtitleDoubleClickActionType.GoToSubtitleAndPause.ToString();
    }

    private void SaveSettings()
    {
        var general = Se.Settings.General;
        var appearance = Se.Settings.Appearance;
        var video = Se.Settings.Video;

        general.SubtitleLineMaximumLength = SingleLineMaxLength ?? general.SubtitleLineMaximumLength;
        general.SubtitleOptimalCharactersPerSeconds = OptimalCharsPerSec ?? general.SubtitleOptimalCharactersPerSeconds;
        general.SubtitleMaximumCharactersPerSeconds = MaxCharsPerSec ?? general.SubtitleMaximumCharactersPerSeconds;
        general.SubtitleMaximumWordsPerMinute = MaxWordsPerMin ?? general.SubtitleMaximumWordsPerMinute;
        general.SubtitleMinimumDisplayMilliseconds = MinDurationMs ?? general.SubtitleMinimumDisplayMilliseconds;
        general.SubtitleMaximumDisplayMilliseconds = MaxDurationMs ?? general.SubtitleMaximumDisplayMilliseconds;
        general.MinimumMillisecondsBetweenLines = MinGapMs ?? general.MinimumMillisecondsBetweenLines;
        general.MaxNumberOfLines = MaxLines ?? general.MaxNumberOfLines;
        general.UnbreakLinesShorterThan = UnbreakLinesShorterThan ?? general.UnbreakLinesShorterThan;
        general.DialogStyle = DialogStyle.Code;
        general.ContinuationStyle = ContinuationStyle.Code;
        general.CpsLineLengthStrategy = CpsLineLengthStrategy.Code;

        general.UseFrameMode = UseFrameMode;
        general.SubtitleTextBoxLimitNewLines = TextBoxLimitNewLines;
        general.NewEmptyDefaultMs = NewEmptyDefaultMs ?? general.NewEmptyDefaultMs;
        general.PromptDeleteLines = PromptDeleteLines;
        general.LockTimeCodes = LockTimeCodes;
        general.RememberPositionAndSize = RememberPositionAndSize;
        general.AutoBackupOn = AutoBackupOn;
        general.AutoBackupIntervalMinutes = AutoBackupIntervalMinutes ?? general.AutoBackupIntervalMinutes;
        general.AutoBackupDeleteAfterDays = AutoBackupDeleteAfterDays ?? general.AutoBackupDeleteAfterDays;
        general.DefaultEncoding = DefaultEncoding?.DisplayName ?? Encodings.First().DisplayName;
        general.SubtitleEnterKeyAction = MapToSelectedSubtitleEnterKeyAction(SelectedSubtitleEnterKeyActionType);
        general.SubtitleSingleClickAction = MapToSelectedSubtitleSingleClickAction(SelectedSubtitleSingleClickActionType);
        general.SubtitleDoubleClickAction = MapToSelectedSubtitleDoubleClickAction(SelectedSubtitleDoubleClickActionType);
        general.SaveAsBehavior = MapToSaveAsBehavior(SelectedSaveAsBehaviorType);
        general.SaveAsAppendLanguageCode = MapToSaveAsAppendLanguageCode(SelectedSaveAsAppendLanguageCode);
        general.AutoConvertToUtf8 = AutoConvertToUtf8;
        general.ForceCrLfOnSave = ForceCrLfOnSave;
        general.AutoTrimWhiteSpace = AutoTrimWhiteSpace;

        general.DefaultSubtitleFormat = SelectedDefaultSubtitleFormat;
        general.DefaultSaveAsFormat = SelectedSaveSubtitleFormat;

        var sbFavorites = new StringBuilder();
        foreach (var format in FavoriteSubtitleFormats)
        {
            sbFavorites.Append(format + ";");
        }
        general.FavoriteSubtitleFormats = sbFavorites.ToString().TrimEnd(';');

        Se.Settings.Tools.GoToLineNumberAlsoSetVideoPosition = GoToLineNumberAlsoSetVideoPosition;
        Se.Settings.Synchronization.AdjustAllTimesRememberLineSelectionChoice = AdjustAllTimesRememberLineSelectionChoice;
        Se.Settings.Tools.SplitOddLinesAction = MapFromSplitOddActionTranslationToCode(SelectedSplitOddNumberOfLinesAction);
        Se.Settings.Ocr.UseWordSplitList = OcrUseWordSplitList;

        appearance.Theme = MapThemeFromTranslation(SelectedTheme);
        appearance.FontName = SelectedFontName == FontNames.First()
            ? new Label().FontFamily.Name
            : SelectedFontName;
        appearance.ToolbarShowFileNew = ShowToolbarNew;
        appearance.ToolbarShowFileOpen = ShowToolbarOpen;
        appearance.ToolbarShowSave = ShowToolbarSave;
        appearance.ToolbarShowSaveAs = ShowToolbarSaveAs;
        appearance.ToolbarShowFind = ShowToolbarFind;
        appearance.ToolbarShowReplace = ShowToolbarReplace;
        appearance.ToolbarShowSpellCheck = ShowToolbarSpellCheck;
        appearance.ToolbarShowSettings = ShowToolbarSettings;
        appearance.ToolbarShowLayout = ShowToolbarLayout;
        appearance.ToolbarShowHelp = ShowToolbarHelp;
        appearance.ToolbarShowEncoding = ShowToolbarEncoding;
        appearance.ToolbarShowFrameRate = ShowToolbarFrameRate;
        appearance.SubtitleGridFontSize = SubtitleGridFontSize;
        appearance.SubtitleGridTextSingleLine = SubtitleGridTextSingleLine;
        appearance.SubtitleGridFormattingType = MapGridFormattingToCode(SubtitleGridFormatting);
        appearance.SubtitleTextBoxAndGridFontName = string.IsNullOrEmpty(SubtitleTextBoxAndGridFontName) ? new Label().FontFamily.Name : SubtitleTextBoxAndGridFontName;
        appearance.SubtitleTextBoxFontSize = TextBoxFontSize;
        appearance.SubtitleTextBoxFontBold = TextBoxFontBold;
        appearance.SubtitleTextBoxColorTags = TextBoxColorTags;
        appearance.SubtitleTextBoxLiveSpellCheck = TextBoxLiveSpellCheck;
        appearance.SubtitleTextBoxCenterText = TextBoxCenterText;
        appearance.TextBoxShowButtonAutoBreak = TextBoxButtonShowAutoBreak;
        appearance.TextBoxShowButtonUnbreak = TextBoxButtonShowUnbreak;
        appearance.TextBoxShowButtonItalic = TextBoxButtonShowItalic;
        appearance.TextBoxShowButtonColor = TextBoxButtonShowColor;
        appearance.TextBoxShowButtonRemoveFormatting = TextBoxButtonShowRemoveFormatting;
        appearance.ShowHints = ShowButtonHints;
        appearance.DarkModeBackgroundColor = DarkModeBackgroundColor.FromColorToHex();
        appearance.DarkModeForegroundColor = DarkModeForegroundColor.FromColorToHex();
        appearance.UseFocusedButtonBackgroundColor = UseFocusedButtonBackgroundColor;
        appearance.FocusedButtonBackgroundColor = FocusedButtonBackgroundColor.FromColorToHex();
        appearance.BookmarkColor = BookmarkColor.FromColorToHex();
        appearance.ShowUpDownStartTime = ShowUpDownStartTime;
        appearance.ShowUpDownEndTime = ShowUpDownEndTime;
        appearance.ShowUpDownDuration = ShowUpDownDuration;
        appearance.ShowUpDownLabels = ShowUpDownLabels;
        appearance.GridCompactMode = GridCompactMode;
        appearance.GridLinesAppearance = SelectedGridLinesVisibility.Type.ToString();
        appearance.ShowLayer = ShowAssaLayer;
        appearance.ShowHorizontalLineAboveToolbar = ShowHorizontalLineAboveToolbar;

        Se.Settings.Waveform.DrawGridLines = WaveformDrawGridLines;
        Se.Settings.Waveform.FocusOnMouseOver = WaveformFocusOnMouseOver;
        Se.Settings.Waveform.CenterVideoPosition = WaveformCenterVideoPosition;
        Se.Settings.Waveform.FocusTextBoxAfterInsertNew = WaveformFocusTextboxAfterInsertNew;
        Se.Settings.Waveform.ShowToolbar = WaveformShowToolbar;

        if (SelectedWaveformDrawStyle == Se.Language.Waveform.WaveformDrawStyleClassic)
        {
            Se.Settings.Waveform.WaveformDrawStyle = WaveformDrawStyle.Classic.ToString();
        }
        else if (SelectedWaveformDrawStyle == Se.Language.Waveform.WaveformDrawStyleFancy)
        {
            Se.Settings.Waveform.WaveformDrawStyle = WaveformDrawStyle.Fancy.ToString();
        }

        Se.Settings.Waveform.GenerateSpectrogram = WaveformGenerateSpectrogram;
        if (SelectedWaveformSpectrogramStyle == Se.Language.Waveform.SpectrogramClassic)
        {
            Se.Settings.Waveform.SpectrogramStyle = SeSpectrogramStyle.Classic.ToString();
        }
        else if (SelectedWaveformSpectrogramStyle == Se.Language.Waveform.SpectrogramClassicPlasma)
        {
            Se.Settings.Waveform.SpectrogramStyle = SeSpectrogramStyle.ClassicPlasma.ToString();
        }
        else if (SelectedWaveformSpectrogramStyle == Se.Language.Waveform.SpectrogramClassicViridis)
        {
            Se.Settings.Waveform.SpectrogramStyle = SeSpectrogramStyle.ClassicViridis.ToString();
        }
        else if (SelectedWaveformSpectrogramStyle == Se.Language.Waveform.SpectrogramClassicInferno)
        {
            Se.Settings.Waveform.SpectrogramStyle = SeSpectrogramStyle.ClassicInferno.ToString();
        }
        else if (SelectedWaveformSpectrogramStyle == Se.Language.Waveform.SpectrogramClassicTurbo)
        {
            Se.Settings.Waveform.SpectrogramStyle = SeSpectrogramStyle.ClassicTurbo.ToString();
        }

        Se.Settings.Waveform.SpectrogramCombinedWaveformHeight = WaveformSpectrogramCombinedWaveformHeight;

        Se.Settings.Waveform.ShowToolbarPlay = ShowWaveformToolbarPlay;
        Se.Settings.Waveform.ShowToolbarPlayNext = ShowWaveformToolbarPlayNext;
        Se.Settings.Waveform.ShowToolbarPlaySelection = ShowWaveformToolbarPlaySelection;
        Se.Settings.Waveform.ShowToolbarRepeat = ShowWaveformToolbarRepeat;
        Se.Settings.Waveform.ShowToolbarRemoveBlankLines = ShowWaveformToolbarRemoveBlankLines;
        Se.Settings.Waveform.ShowToolbarNew = ShowWaveformToolbarNew;
        Se.Settings.Waveform.ShowToolbarSetStart = ShowWaveformToolbarSetStart;
        Se.Settings.Waveform.ShowToolbarSetEnd = ShowWaveformToolbarSetEnd;
        Se.Settings.Waveform.ShowToolbarSetStartAndOffsetTheRest = ShowWaveformToolbarSetStartAndOffsetTheRest;
        Se.Settings.Waveform.ShowToolbarVerticalZoom = ShowWaveformToolbarVerticalZoom;
        Se.Settings.Waveform.ShowToolbarHorizontalZoom = ShowWaveformToolbarHorizontalZoom;
        Se.Settings.Waveform.ShowToolbarVideoPositionSlider = ShowWaveformToolbarVideoPositionSlider;
        Se.Settings.Waveform.ShowToolbarPlaybackSpeed = ShowWaveformToolbarPlaybackSpeed;
        Se.Settings.Waveform.WaveformColor = WaveformColor.FromColorToHex();
        Se.Settings.Waveform.WaveformBackgroundColor = WaveformBackgroundColor.FromColorToHex();
        Se.Settings.Waveform.ParagraphBackground = WaveformParagraphBackgroundColor.FromColorToHex();
        Se.Settings.Waveform.ParagraphSelectedBackground = WaveformParagraphSelectedBackgroundColor.FromColorToHex();
        Se.Settings.Waveform.WaveformSelectedColor = WaveformSelectedColor.FromColorToHex();
        Se.Settings.Waveform.WaveformCursorColor = WaveformCursorColor.FromColorToHex();
        Se.Settings.Waveform.WaveformFancyHighColor = WaveformFancyHighColor.FromColorToHex();
        Se.Settings.Waveform.InvertMouseWheel = WaveformInvertMouseWheel;
        Se.Settings.Waveform.SnapToShotChanges = WaveformSnapToShotChanges;
        Se.Settings.Waveform.ShotChangesAutoGenerate = WaveformShotChangesAutoGenerate;
        Se.Settings.Waveform.AllowOverlap = WaveformAllowOverlap;

        Se.Settings.Waveform.SingleClickAction = MapWaveformSingleClickFromTranslation(SelectedWaveformSingleClickActionType);
        Se.Settings.Waveform.DoubleClickAction = MapWaveformDoubleClickFromTranslation(SelectedWaveformDoubleClickActionType);

        Se.Settings.Waveform.RightClickSelectsSubtitle = WaveformRightClickSelectsSubtitle;

        general.ColorDurationTooLong = ColorDurationTooLong;
        general.ColorDurationTooShort = ColorDurationTooShort;
        general.ColorTextTooLong = ColorTextTooLong;
        general.ColorTextTooWide = ColorTextTooWide;
        general.ColorTextTooManyLines = ColorTextTooManyLines;
        general.ColorTimeCodeOverlap = ColorOverlap;
        general.ColorGapTooShort = ColorGapTooShort;
        general.ErrorColor = ErrorColor.FromColorToHex();

        general.CustomContinuationStyleSuffix = _customContinuationStyleSuffix;
        general.CustomContinuationStyleSuffixApplyIfComma = _customContinuationStyleSuffixApplyIfComma;
        general.CustomContinuationStyleSuffixAddSpace = _customContinuationStyleSuffixAddSpace;
        general.CustomContinuationStyleSuffixReplaceComma = _customContinuationStyleSuffixReplaceComma;
        general.CustomContinuationStylePrefix = _customContinuationStylePrefix;
        general.CustomContinuationStylePrefixAddSpace = _customContinuationStylePrefixAddSpace;
        general.CustomContinuationStyleUseDifferentStyleGap = _customContinuationStyleUseDifferentStyleGap;
        general.ContinuationPause = _continuationPause;
        general.CustomContinuationStyleGapSuffix = _customContinuationStyleGapSuffix;
        general.CustomContinuationStyleGapSuffixApplyIfComma = _customContinuationStyleGapSuffixApplyIfComma;
        general.CustomContinuationStyleGapSuffixAddSpace = _customContinuationStyleGapSuffixAddSpace;
        general.CustomContinuationStyleGapSuffixReplaceComma = _customContinuationStyleGapSuffixReplaceComma;
        general.CustomContinuationStyleGapPrefix = _customContinuationStyleGapPrefix;
        general.CustomContinuationStyleGapPrefixAddSpace = _customContinuationStyleGapPrefixAddSpace;

        video.VideoPlayer = SelectedVideoPlayer.Code;
        video.ShowStopButton = ShowStopButton;
        video.ShowFullscreenButton = ShowFullscreenButton;
        video.AutoOpen = AutoOpenVideoFile;

        video.MpvPreviewFontName = MpvPreviewFontName;
        video.MpvPreviewFontSize = MpvPreviewFontSize;
        video.MpvPreviewFontBold = MpvPreviewFontBold;
        video.MpvPreviewOutlineWidth = MpvPreviewOutlineWidth;
        video.MpvPreviewShadowWidth = MpvPreviewShadowWidth;
        video.MpvPreviewColorPrimary = MpvPreviewColorPrimary.FromColorToHex();
        video.MpvPreviewColorOutline = MpvPreviewColorOutline.FromColorToHex();
        video.MpvPreviewColorShadow = MpvPreviewColorShadow.FromColorToHex();
        video.MpvPreviewBorderType = (int)(MpvPreviewSelectedBorderType?.Style ?? BorderStyleType.Outline);

        general.FfmpegPath = FfmpegPath;
        general.LibMpvPath = LibMpvPath;

        general.CurrentProfile = SelectedProfile;
        general.Profiles.Clear();
        foreach (var profile in _profilesForEdit)
        {
            var p = new RulesProfile
            {
                Name = profile.Name,
                SubtitleLineMaximumLength = profile.SingleLineMaxLength ?? general.SubtitleLineMaximumLength,
                SubtitleOptimalCharactersPerSeconds = (decimal?)profile.OptimalCharsPerSec ?? (decimal)general.SubtitleOptimalCharactersPerSeconds,
                SubtitleMaximumCharactersPerSeconds = (decimal?)profile.MaxCharsPerSec ?? (decimal)general.SubtitleMaximumCharactersPerSeconds,
                SubtitleMaximumWordsPerMinute = (decimal?)profile.MaxWordsPerMin ?? (decimal)general.SubtitleMaximumWordsPerMinute,
                SubtitleMinimumDisplayMilliseconds = profile.MinDurationMs ?? general.SubtitleMinimumDisplayMilliseconds,
                SubtitleMaximumDisplayMilliseconds = profile.MaxDurationMs ?? general.SubtitleMaximumDisplayMilliseconds,
                MinimumMillisecondsBetweenLines = profile.MinGapMs ?? general.MinimumMillisecondsBetweenLines,
                MaxNumberOfLines = profile.MaxLines ?? general.MaxNumberOfLines,
                MergeLinesShorterThan = profile.UnbreakLinesShorterThan ?? general.UnbreakLinesShorterThan,
                DialogStyle = Enum.Parse<DialogType>(profile.DialogStyle.Code),
                ContinuationStyle = Enum.TryParse<ContinuationStyle>(profile.ContinuationStyle.Code, out var cs) ? cs : Core.Enums.ContinuationStyle.None,
                CpsLineLengthStrategy = profile.CpsLineLengthStrategy.Code
            };
            general.Profiles.Add(p);
        }

        Se.SaveSettings();
    }

    private string MapWaveformSingleClickFromTranslation(string selectedWaveformSingleClickActionType)
    {
        if (string.IsNullOrEmpty(selectedWaveformSingleClickActionType))
        {
            return WaveformSingleClickActionType.SetVideoPositionAndPauseAndSelectSubtitle.ToString();
        }

        return WaveformSingleClickTextToActionMap.TryGetValue(selectedWaveformSingleClickActionType, out var action)
            ? action
            : WaveformSingleClickActionType.SetVideoPositionAndPauseAndSelectSubtitle.ToString();
    }

    private string MapWaveformDoubleClickFromTranslation(string selectedWaveformDoubleClickActionType)
    {
        if (string.IsNullOrEmpty(selectedWaveformDoubleClickActionType))
        {
            return WaveformDoubleClickActionType.None.ToString();
        }

        return WaveformDoubleClickTextToActionMap.TryGetValue(selectedWaveformDoubleClickActionType, out var action)
            ? action
            : WaveformDoubleClickActionType.None.ToString();
    }

    private string MapFromSplitOddActionTranslationToCode(string translation)
    {
        if (translation == Se.Language.Options.Settings.SplitOddLineActionWeightTop)
        {
            return nameof(SplitOddLinesActionType.WeightTop);
        }

        if (translation == Se.Language.Options.Settings.SplitOddLineActionWeightBottom)
        {
            return nameof(SplitOddLinesActionType.WeightBottom);
        }

        return nameof(SplitOddLinesActionType.Smart);
    }

    private static string MapToSaveAsBehavior(string selectedSaveAsBehaviorType)
    {
        if (selectedSaveAsBehaviorType == Se.Language.Options.Settings.SaveAsBehaviorUseSubtitleVideoFilename)
        {
            return nameof(SaveAsBehaviourType.UseSubtitleFileNameThenVideoFileName);
        }

        if (selectedSaveAsBehaviorType == Se.Language.Options.Settings.SaveAsBehaviorUseVideoSubtitleFilename)
        {
            return nameof(SaveAsBehaviourType.UseVideoFileNameThenSubtitleFileName);
        }

        if (selectedSaveAsBehaviorType == Se.Language.Options.Settings.SaveAsBehaviorUseVideoFileName)
        {
            return nameof(SaveAsBehaviourType.UseVideoFileName);
        }

        if (selectedSaveAsBehaviorType == Se.Language.Options.Settings.SaveAsBehaviorUseSubtitleFileName)
        {
            return nameof(SaveAsBehaviourType.UseSubtitleFileName);
        }

        return nameof(SaveAsBehaviourType.New);
    }

    private static string MapToSaveAsAppendLanguageCode(string selectedSaveAsAppendLanguageCode)
    {
        if (selectedSaveAsAppendLanguageCode == Se.Language.Options.Settings.SaveAsAppendLanguageCodeTwoLetter)
        {
            return nameof(SaveAsLanguageAppendType.TwoLetterLanguageCode);
        }

        if (selectedSaveAsAppendLanguageCode == Se.Language.Options.Settings.SaveAsAppendLanguageCodeThreeLetter)
        {
            return nameof(SaveAsLanguageAppendType.ThreeLEtterLanguageCode);
        }

        if (selectedSaveAsAppendLanguageCode == Se.Language.Options.Settings.SaveAsAppendLanguageCodeLanguageName)
        {
            return nameof(SaveAsLanguageAppendType.FullLanguageName);
        }

        return nameof(SaveAsLanguageAppendType.None);
    }

    private void SetFfmpegStatus()
    {
        if (!string.IsNullOrEmpty(FfmpegPath) && File.Exists(FfmpegPath))
        {
            FfmpegStatus = "Installed";
        }
        else if (File.Exists(DownloadFfmpegViewModel.GetFfmpegFileName()))
        {
            FfmpegStatus = "Installed";
        }
        else
        {
            if (FfmpegHelper.IsFfmpegInstalled())
            {
                FfmpegStatus = string.Empty;
            }
            else
            {
                FfmpegStatus = "Not installed";
            }
        }
    }

    private void SetLibMpvStatus()
    {
        if (!string.IsNullOrEmpty(LibMpvPath) && File.Exists(LibMpvPath))
        {
            LibMpvStatus = "Installed";
        }
        else
        {
            LibMpvStatus = "Not installed";
        }
    }

    private void SetLibVlcStatus()
    {
        _ = Task.Run(() =>
        {
            var canLoad = new LibVlcDynamicPlayer().CanLoad();
            Dispatcher.UIThread.Post(() =>
            {
                if (canLoad)
                {
                    LibVlcStatus = "Installed";
                }
                else
                {
                    LibVlcStatus = "Not installed";
                }
            });
        });
    }

    public async void ScrollElementIntoView(ScrollViewer scrollViewer, Control target)
    {
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            await Task.Yield(); // Ensures target has been laid out

            // Fade out
            //await FadeToAsync(ScrollView, 0, TimeSpan.FromMilliseconds(100));
            await RunFadeAnimation(ScrollView, from: 1, to: 0, TimeSpan.FromMilliseconds(100));


            await Task.Yield(); // Ensures target has been laid out
            scrollViewer.ScrollToHome();
            await Task.Yield(); // Ensures target has been laid out

            var targetPosition = target.TranslatePoint(new Point(0, 0), scrollViewer);
            if (targetPosition.HasValue)
            {
                scrollViewer.Offset = new Vector(scrollViewer.Offset.X, targetPosition.Value.Y);
            }

            await Task.Yield(); // Ensures target has been laid out
            await RunFadeAnimation(ScrollView, from: 0, to: 1, TimeSpan.FromMilliseconds(200));
        }, DispatcherPriority.Background);
    }

    private static Task RunFadeAnimation(Control control, double from, double to, TimeSpan duration)
    {
        var animation = new Animation
        {
            Duration = duration,
            Easing = new CubicEaseOut(),
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0),
                    Setters = { new Setter(Visual.OpacityProperty, from) }
                },
                new KeyFrame
                {
                    Cue = new Cue(1),
                    Setters = { new Setter(Visual.OpacityProperty, to) }
                }
            }
        };

        return animation.RunAsync(control);
    }

    [RelayCommand]
    private async Task AddFavoriteSubtitleFormat()
    {
        if (Window == null)
        {
            return;
        }

        var viewModel = await _windowService.ShowDialogAsync<PickSubtitleFormatWindow, PickSubtitleFormatViewModel>(Window, vm => { vm.Initialize(null, new Subtitle()); });

        if (viewModel.OkPressed)
        {
            var selectedFormat = viewModel.GetSelectedFormat();
            if (selectedFormat != null)
            {
                FavoriteSubtitleFormats.Add(selectedFormat.FriendlyName);
            }
        }
    }

    [RelayCommand]
    private async Task RemoveFavoriteSubtitleFormat()
    {
        if (SelectedFavoriteSubtitleFormat != null && FavoriteSubtitleFormats.Contains(SelectedFavoriteSubtitleFormat))
        {
            var index = FavoriteSubtitleFormats.IndexOf(SelectedFavoriteSubtitleFormat);
            FavoriteSubtitleFormats.Remove(SelectedFavoriteSubtitleFormat);

            if (FavoriteSubtitleFormats.Count > 0)
            {
                SelectedFavoriteSubtitleFormat = index < FavoriteSubtitleFormats.Count
                    ? FavoriteSubtitleFormats[index]
                    : FavoriteSubtitleFormats[FavoriteSubtitleFormats.Count - 1];
            }
        }

        await Task.CompletedTask;
    }


    [RelayCommand]
    private async Task MoveUpFavoriteSubtitleFormat()
    {
        if (SelectedFavoriteSubtitleFormat != null && FavoriteSubtitleFormats.Contains(SelectedFavoriteSubtitleFormat))
        {
            var index = FavoriteSubtitleFormats.IndexOf(SelectedFavoriteSubtitleFormat);
            if (index > 0)
            {
                var selectedItem = SelectedFavoriteSubtitleFormat;
                FavoriteSubtitleFormats.Move(index, index - 1);
                SelectedFavoriteSubtitleFormat = selectedItem;
            }
        }

        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task MoveDownFavoriteSubtitleFormat()
    {
        if (SelectedFavoriteSubtitleFormat != null && FavoriteSubtitleFormats.Contains(SelectedFavoriteSubtitleFormat))
        {
            var index = FavoriteSubtitleFormats.IndexOf(SelectedFavoriteSubtitleFormat);
            if (index < FavoriteSubtitleFormats.Count - 1)
            {
                var selectedItem = SelectedFavoriteSubtitleFormat;
                FavoriteSubtitleFormats.Move(index, index + 1);
                SelectedFavoriteSubtitleFormat = selectedItem;
            }
        }

        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task EmptyWaveformsAndSpectrograms()
    {
        var answer = MessageBoxResult.Yes;
        if (Se.Settings.General.PromptDeleteLines)
        {
            answer = await MessageBox.Show(
                Window!,
                Se.Language.General.Delete,
                Se.Language.Options.Settings.DeleteWaveformAndSpectrogramFoldersQuestion,
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);
        }

        if (answer != MessageBoxResult.Yes)
        {
            return;
        }

        DeleteWaveformAndSpectrogramFiles();
        _ = UpdateWaveformSpaceInfoAsync();
    }

    public static void DeleteWaveformAndSpectrogramFiles()
    {
        if (Directory.Exists(Se.WaveformsFolder))
        {
            foreach (var file in Directory.GetFiles(Se.WaveformsFolder, "*.wav").ToList())
            {
                try
                {
                    File.Delete(file);
                }
                catch
                {
                    // ignore
                }
            }
        }

        if (Directory.Exists(Se.SpectrogramsFolder))
        {
            foreach (var file in Directory.GetFiles(Se.SpectrogramsFolder, "*.spectrogram").ToList())
            {
                try
                {
                    File.Delete(file);
                }
                catch
                {
                    // ignore
                }
            }
        }
    }

    private static List<string> GetWaveformAndSpecgtrogramFiles()
    {
        var files = Directory.GetFiles(Se.WaveformsFolder, "*.wav").ToList();
        files.AddRange(Directory.GetFiles(Se.SpectrogramsFolder, "*.spectrogram", SearchOption.AllDirectories));
        return files;
    }

    private async Task UpdateWaveformSpaceInfoAsync()
    {
        var files = GetWaveformAndSpecgtrogramFiles();

        long totalBytes = await Task.Run(() =>
        {
            long total = 0;
            foreach (var file in files)
            {
                try
                {
                    total += new FileInfo(file).Length;
                }
                catch
                {
                    // ignore
                }
            }

            return total;
        });

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            WaveformSpaceInfo = string.Format(
                Se.Language.Options.Settings.WaveFormsAndSpectrogramFoldersContainsX,
                Utilities.FormatBytesToDisplayFileSize(totalBytes)
            );
        });
    }


    [RelayCommand]
    private void ScrollToSection(string title)
    {
        var section = Sections.FirstOrDefault(section => section.IsVisible && section.Title == title);
        if (section != null)
        {
            ScrollElementIntoView(ScrollView, section.Panel!);
        }
    }

    [RelayCommand]
    private async Task DownloadFfmpeg()
    {
        var vm = await _windowService.ShowDialogAsync<DownloadFfmpegWindow, DownloadFfmpegViewModel>(Window!);
        if (string.IsNullOrEmpty(vm.FfmpegFileName))
        {
            return;
        }

        FfmpegPath = vm.FfmpegFileName;
        SetFfmpegStatus();
    }

    [RelayCommand]
    private async Task DownloadLibMpv()
    {
        var result = await _windowService.ShowDialogAsync<DownloadLibMpvWindow, DownloadLibMpvViewModel>(Window!);
        if (string.IsNullOrEmpty(result.LibMpvFileName))
        {
            return;
        }

        LibMpvPath = result.LibMpvFileName;
        SetLibMpvStatus();
    }

    [RelayCommand]
    private async Task DownloadLibVlc()
    {
        var result = await _windowService.ShowDialogAsync<DownloadLibVlcWindow, DownloadLibVlcViewModel>(Window!);
        if (!result.OkPressed)
        {
            return;
        }

        SetLibVlcStatus();
    }

    [RelayCommand]
    private async Task ResetAllSettings()
    {
        if (Window == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<SettingsResetWindow, SettingsResetViewModel>(Window!);
        if (!result.OkPressed)
        {
            return;
        }

        if (result.ResetAll)
        {
            Se.Settings = new Se();
        }
        else
        {
            if (result.ResetRecentFiles)
            {
                Se.Settings.File.RecentFiles = new List<RecentFile>();
            }

            if (result.ResetWindowPositionAndSize)
            {
                Se.Settings.General.WindowPositions = new List<SeWindowPosition>();
            }

            if (result.ResetShortcuts)
            {
                Se.Settings.Shortcuts = new List<SeShortCut>();
            }

            if (result.ResetMultipleReplaceRules)
            {
                Se.Settings.Edit.MultipleReplace = new SeEditMultipleReplace();
            }

            if (result.ResetRules)
            {
                var g = new SeGeneral();
                Se.Settings.General.Profiles = g.Profiles;
                Se.Settings.General.CurrentProfile = g.CurrentProfile;
                Se.Settings.General.SubtitleLineMaximumLength = g.SubtitleLineMaximumLength;
                Se.Settings.General.SubtitleOptimalCharactersPerSeconds = g.SubtitleOptimalCharactersPerSeconds;
                Se.Settings.General.SubtitleMaximumCharactersPerSeconds = g.SubtitleMaximumCharactersPerSeconds;
                Se.Settings.General.SubtitleMaximumWordsPerMinute = g.SubtitleMaximumWordsPerMinute;
                Se.Settings.General.SubtitleMinimumDisplayMilliseconds = g.SubtitleMinimumDisplayMilliseconds;
                Se.Settings.General.SubtitleMaximumDisplayMilliseconds = g.SubtitleMaximumDisplayMilliseconds;
                Se.Settings.General.MinimumMillisecondsBetweenLines = g.MinimumMillisecondsBetweenLines;
                Se.Settings.General.MaxNumberOfLines = g.MaxNumberOfLines;
                Se.Settings.General.UnbreakLinesShorterThan = g.UnbreakLinesShorterThan;
                Se.Settings.General.DialogStyle = g.DialogStyle;
                Se.Settings.General.ContinuationStyle = g.ContinuationStyle;
                Se.Settings.General.CpsLineLengthStrategy = g.CpsLineLengthStrategy;
            }

            if (result.ResetAppearance)
            {
                Se.Settings.Appearance = new SeAppearance();
            }

            if (result.ResetAutoTranslate)
            {
                Se.Settings.AutoTranslate = new SeAutoTranslate();
            }

            if (result.ResetWaveform)
            {
                Se.Settings.Waveform = new SeWaveform();
            }

            if (result.ResetSyntaxColoring)
            {
                var g = new SeGeneral();

                Se.Settings.General.ColorDurationTooLong = Se.Settings.General.ColorDurationTooLong;
                Se.Settings.General.ColorDurationTooShort = g.ColorDurationTooShort;
                Se.Settings.General.ColorTextTooLong = g.ColorTextTooLong;
                Se.Settings.General.ColorTextTooWide = g.ColorTextTooWide;
                Se.Settings.General.ColorTextTooManyLines = g.ColorTextTooManyLines;
                Se.Settings.General.ColorTimeCodeOverlap = g.ColorTimeCodeOverlap;
                Se.Settings.General.ColorGapTooShort = g.ColorGapTooShort;
                Se.Settings.General.ErrorColor = g.ErrorColor;
            }
        }

        Se.SaveSettings();
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void CommandOk()
    {
        SaveSettings();
        SaveFileTypeAssociations();

        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Apply()
    {
        SaveSettings();
        SaveFileTypeAssociations();
        _mainViewModel?.ApplySettings();
    }

    [RelayCommand]
    private async Task EditProfiles()
    {
        if (Window == null)
        {
            return;
        }

        var currentProfile = _profilesForEdit.FirstOrDefault(p => p.Name == SelectedProfile);
        if (currentProfile != null)
        {
            currentProfile.SingleLineMaxLength = SingleLineMaxLength;
            currentProfile.OptimalCharsPerSec = OptimalCharsPerSec;
            currentProfile.MaxCharsPerSec = MaxCharsPerSec;
            currentProfile.MaxWordsPerMin = MaxWordsPerMin;
            currentProfile.MinDurationMs = MinDurationMs;
            currentProfile.MaxDurationMs = MaxDurationMs;
            currentProfile.MinGapMs = MinGapMs;
            currentProfile.MaxLines = MaxLines;
            currentProfile.UnbreakLinesShorterThan = UnbreakLinesShorterThan;
            currentProfile.DialogStyle = DialogStyle;
            currentProfile.ContinuationStyle = ContinuationStyle;
            currentProfile.CpsLineLengthStrategy = CpsLineLengthStrategy;
        }

        var result = await _windowService
            .ShowDialogAsync<ProfilesWindow, ProfilesViewModel>(Window, vm => { vm.Initialize(_profilesForEdit, SelectedProfile); });


        if (!result.OkPressed)
        {
            return;
        }

        var oldProfileName = SelectedProfile;
        _profilesForEdit = result.Profiles.ToList();

        if (_profilesForEdit.Count == 0)
        {
            var g = new SeGeneral();
            var pd = new ProfileDisplay
            {
                Name = "Default",
                SingleLineMaxLength = g.SubtitleLineMaximumLength,
                OptimalCharsPerSec = (double)g.SubtitleOptimalCharactersPerSeconds,
                MaxCharsPerSec = (double)g.SubtitleMaximumCharactersPerSeconds,
                MaxWordsPerMin = (double)g.SubtitleMaximumWordsPerMinute,
                MinDurationMs = g.SubtitleMinimumDisplayMilliseconds,
                MaxDurationMs = g.SubtitleMaximumDisplayMilliseconds,
                MinGapMs = g.MinimumMillisecondsBetweenLines,
                MaxLines = g.MaxNumberOfLines,
                UnbreakLinesShorterThan = g.UnbreakLinesShorterThan,
                DialogStyle = DialogStyles.FirstOrDefault(p => p.Code == g.DialogStyle) ?? DialogStyles.First(),
                ContinuationStyle = ContinuationStyles.FirstOrDefault(p => p.Code == g.ContinuationStyle) ?? ContinuationStyles.First(),
                CpsLineLengthStrategy = CpsLineLengthStrategies.FirstOrDefault(p => p.Code == g.CpsLineLengthStrategy) ?? CpsLineLengthStrategies.First()
            };
            _profilesForEdit.Add(pd);
        }

        Profiles.Clear();
        var profilesForEdit = new List<ProfileDisplay>();
        foreach (var profile in _profilesForEdit)
        {
            var pd = new ProfileDisplay(profile);
            pd.DialogStyle = DialogStyles.FirstOrDefault(p => p.Code == profile.DialogStyle?.Code) ?? DialogStyles.First();
            pd.ContinuationStyle = ContinuationStyles.FirstOrDefault(p => p.Code == profile.ContinuationStyle?.Code) ?? ContinuationStyles.First();
            pd.CpsLineLengthStrategy = CpsLineLengthStrategies.FirstOrDefault(p => p.Code == profile.CpsLineLengthStrategy?.Code) ?? CpsLineLengthStrategies.First();
            profilesForEdit.Add(pd);
            Profiles.Add(profile.Name);
        }

        _profilesForEdit = profilesForEdit;

        SelectedProfile = Profiles.FirstOrDefault(p => p == oldProfileName) ?? Profiles.First();
    }

    [RelayCommand]
    private async Task ShowErrorLogFile()
    {
        if (Window == null || !File.Exists(Se.GetErrorLogFilePath()))
        {
            return;
        }

        await _folderHelper.OpenFolderWithFileSelected(Window!, Se.GetErrorLogFilePath());
    }

    [RelayCommand]
    private async Task ShowWhisperLogFile()
    {
        if (Window == null || !File.Exists(Se.GetWhisperLogFilePath()))
        {
            return;
        }

        await _folderHelper.OpenFolderWithFileSelected(Window!, Se.GetWhisperLogFilePath());
    }

    [RelayCommand]
    private async Task ShowSettingsFile()
    {
        if (Window == null || !File.Exists(Se.GetSettingsFilePath()))
        {
            return;
        }

        await _folderHelper.OpenFolderWithFileSelected(Window!, Se.GetSettingsFilePath());
    }

    [RelayCommand]
    private async Task ShowEditCustomContinuationStyle()
    {
        if (Window == null)
        {
            return;
        }

        var result = await _windowService
            .ShowDialogAsync<CustomContinuationStyleWindow, CustomContinuationStyleViewModel>(Window, vm =>
            {
                vm.Initialize(
                    _continuationPause,
                    _customContinuationStyleSuffix,
                    _customContinuationStyleSuffixApplyIfComma,
                    _customContinuationStyleSuffixAddSpace,
                    _customContinuationStyleSuffixReplaceComma,
                    _customContinuationStylePrefix,
                    _customContinuationStylePrefixAddSpace,
                    _customContinuationStyleUseDifferentStyleGap,
                    _customContinuationStyleGapSuffix,
                    _customContinuationStyleGapSuffixApplyIfComma,
                    _customContinuationStyleGapSuffixAddSpace,
                    _customContinuationStyleGapSuffixReplaceComma,
                    _customContinuationStyleGapPrefix,
                    _customContinuationStyleGapPrefixAddSpace);
            });

        if (!result.OkPressed)
        {
            return;
        }

        _customContinuationStyleSuffix = result.SelectedSuffix;
        _customContinuationStyleSuffixApplyIfComma = result.SelectedSuffixesProcessIfEndWithComma;
        _customContinuationStyleSuffixAddSpace = result.SelectedSuffixesAddSpace;
        _customContinuationStyleSuffixReplaceComma = result.SelectedSuffixesRemoveComma;
        _customContinuationStylePrefix = result.SelectedPrefix;
        _customContinuationStylePrefixAddSpace = result.SelectedPrefixAddSpace;
        _customContinuationStyleUseDifferentStyleGap = result.UseSpecialStyleAfterLongGaps;
        _continuationPause = result.LongGapMs;
        _customContinuationStyleGapSuffix = result.SelectedLongGapSuffix;
        _customContinuationStyleGapSuffixApplyIfComma = result.SelectedLongGapSuffixesProcessIfEndWithComma;
        _customContinuationStyleGapSuffixAddSpace = result.SelectedLongGapSuffixesAddSpace;
        _customContinuationStyleGapSuffixReplaceComma = result.SelectedLongGapSuffixesRemoveComma;
        _customContinuationStyleGapPrefix = result.SelectedLongGapPrefix;
        _customContinuationStyleGapPrefixAddSpace = result.SelectedLongGapPrefixAddSpace;

        RuleValueChanged();
    }

    private void LoadFileTypeAssociations()
    {
        FileTypeAssociationsManager.LoadFileTypeAssociations(FileTypeAssociations);
    }

    private async void SaveFileTypeAssociations()
    {
        await FileTypeAssociationsManager.SaveFileTypeAssociationsAsync(FileTypeAssociations, Window);
    }

    [RelayCommand]
    private void CommandCancel()
    {
        Window?.Close();
    }

    [RelayCommand]
    private async Task ImportSettings()
    {
        if (Window == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<SettingsImportExport.SettingsImportExportWindow, SettingsImportExport.SettingsImportExportViewModel>(
            Window,
            vm => vm.SetIsExport(false));

        if (result.OkPressed)
        {
            // Reload the settings from disk since they were updated
            LoadSettings();
        }
    }

    [RelayCommand]
    private async Task ExportSettings()
    {
        if (Window == null)
        {
            return;
        }

        await _windowService.ShowDialogAsync<SettingsImportExport.SettingsImportExportWindow, SettingsImportExport.SettingsImportExportViewModel>(
            Window,
            vm => vm.SetIsExport(true));
    }

    public void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/settings");
        }
    }

    internal void Initialize(MainViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;
    }

    internal void ProfileChanged()
    {
        var profile = _profilesForEdit.FirstOrDefault(p => p.Name == SelectedProfile);
        if (profile == null)
        {
            return;
        }

        _skipRuleValueChanged = true;

        try
        {
            SingleLineMaxLength = profile.SingleLineMaxLength;
            OptimalCharsPerSec = profile.OptimalCharsPerSec;
            MaxCharsPerSec = profile.MaxCharsPerSec;
            MaxWordsPerMin = profile.MaxWordsPerMin;
            MinDurationMs = profile.MinDurationMs;
            MaxDurationMs = profile.MaxDurationMs;
            MinGapMs = profile.MinGapMs;
            MaxLines = profile.MaxLines;
            UnbreakLinesShorterThan = profile.UnbreakLinesShorterThan;
            DialogStyle = profile.DialogStyle;
            ContinuationStyle = profile.ContinuationStyle;
            CpsLineLengthStrategy = profile.CpsLineLengthStrategy;
        }
        finally
        {
            _skipRuleValueChanged = false;
        }
    }

    internal void RuleValueChanged()
    {
        var profileItem = _profilesForEdit.FirstOrDefault(p => p.Name == SelectedProfile);
        if (profileItem == null)
        {
            return;
        }

        if (_skipRuleValueChanged)
        {
            return;
        }

        profileItem.SingleLineMaxLength = SingleLineMaxLength;
        profileItem.OptimalCharsPerSec = OptimalCharsPerSec;
        profileItem.MaxCharsPerSec = MaxCharsPerSec;
        profileItem.MaxWordsPerMin = MaxWordsPerMin;
        profileItem.MinDurationMs = MinDurationMs;
        profileItem.MaxDurationMs = MaxDurationMs;
        profileItem.MinGapMs = MinGapMs;
        profileItem.MaxLines = MaxLines;
        profileItem.UnbreakLinesShorterThan = UnbreakLinesShorterThan;
        profileItem.DialogStyle = DialogStyle;
        profileItem.ContinuationStyle = ContinuationStyle;
        profileItem.CpsLineLengthStrategy = CpsLineLengthStrategy;
    }

    internal void ContinuationStyleChanged()
    {
        if (ContinuationStyle == null)
        {
            IsEditCustomContinuationStyleVisible = false;
            return;
        }

        IsEditCustomContinuationStyleVisible = ContinuationStyle.Code == nameof(Core.Enums.ContinuationStyle.Custom);
        RuleValueChanged();
    }

    public void VideoPlayerChanged()
    {
        IsMpvChosen = SelectedVideoPlayer.Name == "mpv";
    }

    internal void Onloaded(object? sender, RoutedEventArgs e)
    {
        UiUtil.RestoreWindowPosition(Window);
        _ = UpdateWaveformSpaceInfoAsync();
    }

    internal void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        UiUtil.SaveWindowPosition(Window);
    }
}