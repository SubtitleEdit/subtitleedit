using Nikse.SubtitleEdit.Core.Enums;
using System;

namespace Nikse.SubtitleEdit.Logic.Config.Language.Options;

public class LanguageSettings
{
    public string DialogStyle { get; set; }

    public string DialogStyleDashSecondLineWithoutSpace { get; set; }
    public string DialogStyleDashSecondLineWithSpace { get; set; }
    public string DialogStyleDashBothLinesWithSpace { get; set; }
    public string DialogStyleDashBothLinesWithoutSpace { get; set; }

    public string ContinuationStyle { get; set; }
    public string ContinuationStyleNone { get; set; }
    public string ContinuationStyleNoneTrailingDots { get; set; }
    public string ContinuationStyleNoneLeadingTrailingDots { get; set; }
    public string ContinuationStyleNoneTrailingEllipsis { get; set; }
    public string ContinuationStyleNoneLeadingTrailingEllipsis { get; set; }
    public string ContinuationStyleOnlyTrailingDots { get; set; }
    public string ContinuationStyleLeadingTrailingDots { get; set; }
    public string ContinuationStyleOnlyTrailingEllipsis { get; set; }
    public string ContinuationStyleLeadingTrailingEllipsis { get; set; }
    public string ContinuationStyleLeadingTrailingDash { get; set; }
    public string ContinuationStyleLeadingTrailingDashDots { get; set; }
    public string ContinuationStyleCustom { get; set; }

    public string CpsLineLengthStyle { get; set; }
    public string CpsLineLengthStyleCalcAll { get; set; }
    public string CpsLineLengthStyleCalcNoSpaceCpsOnly { get; set; }
    public string CpsLineLengthStyleCalcNoSpace { get; set; }
    public string CpsLineLengthStyleCalcCjk { get; set; }
    public string CpsLineLengthStyleCalcCjkNoSpace { get; set; }
    public string CpsLineLengthStyleCalcIncludeCompositionCharacters { get; set; }
    public string CpsLineLengthStyleCalcIncludeCompositionCharactersNotSpace { get; set; }
    public string CpsLineLengthStyleCalcNoSpaceOrPunctuation { get; set; }
    public string CpsLineLengthStyleCalcNoSpaceOrPunctuationCpsOnly { get; set; }
    public string CpsLineLengthStyleCalcIgnoreArabicDiacritics { get; set; }
    public string CpsLineLengthStyleCalcIgnoreArabicDiacriticsNoSpace { get; set; }

    public string TimeCodeModeHhMmSsMs { get; set; }
    public string TimeCodeModeHhMmSsFf { get; set; }

    public string SplitBehaviorPrevious { get; set; }
    public string SplitBehaviorHalf { get; set; }
    public string SplitBehaviorNext { get; set; }

    public string SubtitleListActionNothing { get; set; }
    public string SubtitleListActionVideoGoToPositionAndPause { get; set; }
    public string SubtitleListActionVideoGoToPositionAndPlay { get; set; }
    public string SubtitleListActionVideoGoToPositionAndPlayCurrentAndPause { get; set; }
    public string SubtitleListActionEditText { get; set; }
    public string SubtitleListActionVideoGoToPositionMinus1SecAndPause { get; set; }
    public string SubtitleListActionVideoGoToPositionMinusHalfSecAndPause { get; set; }
    public string SubtitleListActionVideoGoToPositionMinus1SecAndPlay { get; set; }
    public string SubtitleListActionEditTextAndPause { get; set; }

    public string AutoBackupEveryMinute { get; set; }
    public string AutoBackupEveryXthMinute { get; set; }

    public string Profiles { get; set; }
    public string AutoBackupDeleteAfterXMonths { get; set; }
    public string SearchSettingsDotDoDot { get; set; }
    public string SyntaxColoring { get; set; }
    public string WaveformSpectrogram { get; set; }
    public string Network { get; set; }
    public string FileTypeAssociations { get; set; }
    public string TextBoxColorTags { get; set; }
    public string TextBoxLiveSpellCheck { get; set; }
    public string TextBoxCenterText { get; set; }
    public string TextBoxFontBold { get; set; }
    public string TextBoxFontSize { get; set; }
    public string SubtitleTextBoxAndGridFontName { get; set; }
    public string SubtitleGridFontSize { get; set; }
    public string SubtitleGridTextSingleLine { get; set; }
    public string SubtitleGridShowFormatting { get; set; }
    public string ShowUpDownStartTime { get; set; }
    public string ShowUpDownEndTime { get; set; }
    public string ShowUpDownDuration { get; set; }
    public string ShowUpDownLabels { get; set; }
    public string ShowButtonHints { get; set; }
    public string GridCompactMode { get; set; }
    public string UiFont { get; set; }
    public string Theme { get; set; }
    public string DarkThemeForegroundColor { get; set; }
    public string DarkThemeBackgroundColor { get; set; }
    public string ShowGridLines { get; set; }
    public string ResetSettings { get; set; }
    public string ResetSettingsDetail { get; set; }
    public string ShowHorizontalLineAboveToolbar { get; set; }
    public string BookmarkColor { get; set; }
    public string SingleLineMaxLength { get; set; }
    public string OptimalCharsPerSec { get; set; }
    public string MaxCharsPerSec { get; set; }
    public string MaxWordsPerMin { get; set; }
    public string MinDurationMs { get; set; }
    public string MaxDurationMs { get; set; }
    public string MinGapMs { get; set; }
    public string MaxLines { get; set; }
    public string UnbreakSubtitlesShortThan { get; set; }
    public string NewEmptyDefaultMs { get; set; }
    public string PromptDeleteLines { get; set; }
    public string RememberPositionAndSize { get; set; }
    public string AutoBackupOn { get; set; }
    public string AutoBackupIntervalMinutes { get; set; }
    public string AutoBackupDeleteAfterDays { get; set; }
    public string AutoConvertToUtf8 { get; set; }
    public string AutoTrimWhiteSpace { get; set; }
    public string DefaultEncoding { get; set; }
    public string ColorDurationTooShort { get; set; }
    public string ColorDurationTooLong { get; set; }
    public string ColorTextTooLong { get; set; }
    public string ColorTextTooWide { get; set; }
    public string ColorTextTooManyLines { get; set; }
    public string ColorOverlap { get; set; }
    public string ColorGapTooShort { get; set; }
    public string ErrorBackgroundColor { get; set; }
    public string WaveformDrawGridLines { get; set; }
    public string WaveformCenterVideoPosition { get; set; }
    public string WaveformShowToolbar { get; set; }
    public string WaveformSpectrogramCombinedWaveformHeight { get; set; }
    public string ShowWaveformToolbarPlay { get; set; }
    public string ShowWaveformToolbarRepeat { get; set; }
    public string ShowWaveformToolbarRemoveBlankLines { get; set; }
    public string ShowWaveformToolbarNew { get; set; }
    public string ShowWaveformToolbarSetStart { get; set; }
    public string ShowWaveformToolbarSetEnd { get; set; }
    public string ShowWaveformToolbarStartAndOffsetTheRest { get; set; }
    public string ShowWaveformToolbarHorizontalZoom { get; set; }
    public string ShowWaveformToolbarVerticalZoom { get; set; }
    public string ShowWaveformToolbarVideoPositionSlider { get; set; }
    public string ShowWaveformToolbarPlaybackSpeed { get; set; }
    public string WaveformFocusTextboxAfterInsertNew { get; set; }
    public string WaveformInvertMouseWheel { get; set; }
    public string WaveformSnapToShotChanges { get; set; }
    public string WaveformShotChangesAutoGenerate { get; set; }
    public string WaveformColor { get; set; }
    public string WaveformBackgroundColor { get; set; }
    public string WaveformSelectedColor { get; set; }
    public string DownloadFfmpeg { get; set; }

    // Toolbar
    public string ShowToolbarNew { get; set; }
    public string ShowToolbarOpen { get; set; }
    public string ShowToolbarSave { get; set; }
    public string ShowToolbarSaveAs { get; set; }
    public string ShowToolbarFind { get; set; }
    public string ShowToolbarReplace { get; set; }
    public string ShowToolbarSpellCheck { get; set; }
    public string ShowToolbarSettings { get; set; }
    public string ShowToolbarLayout { get; set; }
    public string ShowToolbarHelp { get; set; }
    public string ShowToolbarEncoding { get; set; }
    public string ShowToolbarFrameRate { get; set; }

    // Network
    public string ProxyAddress { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }

    public string DefaultFormat { get; set; }
    public string DefaultSaveAsFormat { get; set; }
    public string FavoriteSubtitleFormats { get; set; }

    public string ShowStopButton { get; set; }
    public string ShowFullscreenButton { get; set; }
    public string AutoOpenVideoFile { get; set; }
    public string DownloadMpv { get; set; }
    public string DownloadVlc { get; set; }
    public string GoToLineNumberSetsVideoPosition { get; set; }
    public string AdjustAllTimesRememberLineSelectionChoice { get; set; }
    public string FilesAndLogs { get; set; }
    public string ShowErrorLogFile { get; set; }
    public string ShowWhisperLogFile { get; set; }
    public string ShowSettingsFile { get; set; }
    public string ShowAssaLayer { get; set; }
    public string WaveformCursorColor { get; set; }
    public string WaveformFancyHighColor { get; set; }
    public string WaveformFocusOnMouseOver { get; set; }

    public string ResetAllSettings { get; set; }
    public string ResetShortcuts { get; set; }
    public string ResetRecentFiles { get; set; }
    public string ResetMultipleReplaceRules { get; set; }
    public string ResetAppearance { get; set; }
    public string ResetAutoTranslate { get; set; }
    public string ResetWindowPositionAndSize { get; set; }
    public string ResetSettingsTitle { get; set; }
    public string OpenRuleFile { get; set; }
    public string ExportProfiles { get; set; }
    public string SaveRuleProfilesFile { get; set; }
    public string RuleProfilesExportedX { get; set; }
    public string RuleProfilesImportedX { get; set; }
    public string UseSpecialStyleAfterLongGaps { get; set; }
    public string AddSpace { get; set; }
    public string ProcessIfEndsWithComma { get; set; }
    public string RemoveComma { get; set; }
    public string EditContinuationStyleCustom { get; set; }
    public string LongGapThreshold { get; set; }
    public string AfterLongGap { get; set; }
    public string ResetSyntaxColoring { get; set; }
    public string ResetWaveform { get; set; }
    public string ResetRules { get; set; }
    public string UseFrameMode { get; set; }
    public string TextBoxLimitNewLines { get; set; }
    public string MpvVideoOutput { get; set; }
    public string MpvOpenGl { get; set; }
    public string MpvSoftwareRendering { get; set; }
    public string MpvWidRendering { get; set; }
    public string WaveFormsAndSpectrogramFoldersContainsX { get; set; }
    public string DeleteWaveformAndSpectrogramFoldersQuestion { get; set; }
    public string WaveformGenerateSpectrogram { get; set; }
    public string WaveformSpectrogramMode { get; set; }
    public string WaveformCenterOnSingleClick { get; set; }
    public string WaveformSingleClickSelectsSubtitle { get; set; }
    public string WaveformRightClickSelectsSubtitle { get; set; }
    public string WaveformPauseOnSingleClick { get; set; }
    public string WaveformDrawStyle { get; set; }
    public string VlcWidRendering { get; set; }
    public string SubtitleSEnterKeyAction { get; set; }
    public string SubtitleSingleClickAction { get; set; }
    public string SubtitleDoubleClickAction { get; set; }
    public string SaveAsBehavior { get; set; }
    public string SaveAsAppendLanguageCode { get; set; }
    public string GridGoToSubtitleAndSetVideoPosition { get; set; }
    public string GridGoToNextLine { get; set; }
    public string GridGoToSubtitleOnlyWaveformOnly { get; set; }
    public string GridGoToSubtitleAndPause { get; set; }
    public string GridGoToSubtitleAndPlay { get; set; }
    public string GridGoToSubtitleAndPauseAndFocusTextBox { get; set; }
    public string SubtitleGridFormattingNone { get; set; }
    public string SubtitleGridFormattingShowFormatting { get; set; }
    public string SubtitleGridFormattingShowTags { get; set; }
    public string WaveformParagraphBackgroundColor { get; set; }
    public string WaveformParagraphSelectedBackgroundColor { get; set; }
    public string WaveformAllowOverlap { get; set; }
    public string SaveAsBehaviorUseSubtitleVideoFilename { get; set; }
    public string SaveAsBehaviorUseVideoSubtitleFilename { get; set; }
    public string SaveAsBehaviorUseVideoFileName { get; set; }
    public string SaveAsBehaviorUseSubtitleFileName { get; set; }
    public string SaveAsAppendLanguageCodeTwoLetter { get; set; }
    public string SaveAsAppendLanguageCodeThreeLetter { get; set; }
    public string SaveAsAppendLanguageCodeLanguageName { get; set; }
    public string SplitOddLineActionWeightTop { get; set; }
    public string SplitOddLineActionWeightBottom { get; set; }
    public string SplitOddLinesAction { get; set; }
    public string OcrUseWordSplitList { get; set; }
    public string UseFocusedButtonBackgroundColor { get; set; }
    public string FocusedButtonBackgroundColor { get; set; }
    public string ForceCrLfOnSave { get; set; }
    public string ShowWaveformToolbarPlayNext { get; set; }
    public string ShowWaveformToolbarPlaySelection { get; set; }
    public string TextBoxButtonShowAutoBreak { get; set; }
    public string TextBoxButtonShowUnbreak { get; set; }
    public string TextBoxButtonShowItalic { get; set; }
    public string TextBoxButtonShowColor { get; set; }
    public string TextBoxButtonShowRemoveFormatting { get; set; }
    public string WaveformSingleClickAction { get; set; }
    public string WaveformDoubleClickAction { get; set; }
    public string AllSettings { get; set; }

    public LanguageSettings()
    {
        DialogStyle = "Dialog style";
        DialogStyleDashBothLinesWithSpace = "Dash both lines with space";
        DialogStyleDashBothLinesWithoutSpace = "Dash both lines without space";
        DialogStyleDashSecondLineWithSpace = "Dash second line with space";
        DialogStyleDashSecondLineWithoutSpace = "Dash second line without space";

        ContinuationStyle = "Continuation style";
        ContinuationStyleNone = "None";
        ContinuationStyleNoneTrailingDots = "None, dots for pauses (trailing only)";
        ContinuationStyleNoneLeadingTrailingDots = "None, dots for pauses";
        ContinuationStyleNoneTrailingEllipsis = "None, ellipsis for pauses (trailing only)";
        ContinuationStyleNoneLeadingTrailingEllipsis = "None, ellipsis for pauses";
        ContinuationStyleOnlyTrailingDots = "Dots (trailing only)";
        ContinuationStyleLeadingTrailingDots = "Dots";
        ContinuationStyleOnlyTrailingEllipsis = "Ellipsis (trailing only)";
        ContinuationStyleLeadingTrailingEllipsis = "Ellipsis";
        ContinuationStyleLeadingTrailingDash = "Dash";
        ContinuationStyleLeadingTrailingDashDots = "Dash, but dots for pauses";
        ContinuationStyleCustom = "Custom";

        CpsLineLengthStyle = "Cps/line-length";
        CpsLineLengthStyleCalcAll = "Count all characters";
        CpsLineLengthStyleCalcNoSpaceCpsOnly = "Count all except space, cps only";
        CpsLineLengthStyleCalcNoSpace = "Count all except space";
        CpsLineLengthStyleCalcCjk = "CJK 1, Latin 0.5";
        CpsLineLengthStyleCalcCjkNoSpace = "CJK 1, Latin 0.5, space 0";
        CpsLineLengthStyleCalcIncludeCompositionCharacters = "Include composition characters";
        CpsLineLengthStyleCalcIncludeCompositionCharactersNotSpace = "Include composition characters, not space";
        CpsLineLengthStyleCalcNoSpaceOrPunctuation = "No space or punctuation ()[]-:;,.!?";
        CpsLineLengthStyleCalcNoSpaceOrPunctuationCpsOnly = "No space or punctuation, CPS only";
        CpsLineLengthStyleCalcIgnoreArabicDiacritics = "Ignore Arabic diacritics";
        CpsLineLengthStyleCalcIgnoreArabicDiacriticsNoSpace = "Ignore Arabic diacritics, no space";

        TimeCodeModeHhMmSsMs = "HH:MM:SS:MS";
        TimeCodeModeHhMmSsFf = "HH:MM:SS:FF";

        SplitBehaviorPrevious = "Add gap to the left of split point (focus right)";
        SplitBehaviorHalf = "Add gap in the center of split point (focus left)";
        SplitBehaviorNext = "Add gap to the right of split point (focus left)";

        SubtitleListActionNothing = "Nothing";
        SubtitleListActionVideoGoToPositionAndPause = "Go to video position and pause";
        SubtitleListActionVideoGoToPositionAndPlay = "Go to video position and play";
        SubtitleListActionVideoGoToPositionAndPlayCurrentAndPause = "Go to video position, play current, and pause";
        SubtitleListActionEditText = "Go to edit text box";
        SubtitleListActionVideoGoToPositionMinus1SecAndPause = "Go to video position - 1 s and pause";
        SubtitleListActionVideoGoToPositionMinusHalfSecAndPause = "Go to video position - 0.5 s and pause";
        SubtitleListActionVideoGoToPositionMinus1SecAndPlay = "Go to video position - 1 s and play";
        SubtitleListActionEditTextAndPause = "Go to edit text box, and pause at video position";

        AutoBackupEveryMinute = "Every minute";
        AutoBackupEveryXthMinute = "Every {0}th minute";
        AutoBackupDeleteAfterXMonths = "Delete auto-backups after {0} months";

        Profiles = "Profiles";
        SearchSettingsDotDoDot = "Search for settings...";
        SyntaxColoring = "Syntax coloring";
        WaveformSpectrogram = "Waveform/spectrogram";
        Network = "Network";
        FileTypeAssociations = "File type associations";
        TextBoxCenterText = "Center text in subtitle text box";
        TextBoxColorTags = "Color tags (HTML/ASSA) in subtitle text box";
        TextBoxLiveSpellCheck = "Live spell check in subtitle text box";    
        TextBoxFontBold = "Bold text in subtitle text box";
        TextBoxFontSize = "Font size in subtitle text box";
        SubtitleTextBoxAndGridFontName = "UI font in subtitle text box and grid";
        SubtitleGridFontSize = "Font size in subtitle grid";
        SubtitleGridTextSingleLine = "Show subtitle text as single line in grid";
        SubtitleGridShowFormatting = "Show formatted (HTML/ASSA) text in subtitle grid";
        ShowUpDownStartTime = "Show up/down control for \"Show\"";
        ShowUpDownEndTime = "Show up/down control for \"Hide\"";
        ShowUpDownDuration = "Show up/down control for \"Duration\"";
        ShowUpDownLabels = "Show labels for up/down controls (Show/Hide/Duration)";
        ShowButtonHints = "Show button hints";
        GridCompactMode = "Use compact mode for grids";
        UiFont = "UI font";
        Theme = "Theme";
        DarkThemeForegroundColor = "Dark theme foreground color";
        DarkThemeBackgroundColor = "Dark theme background color";
        ShowGridLines = "Show grid lines";
        ResetSettings = "Reset settings?";
        ResetSettingsDetail = "This will reset all settings to their default values.\n\nContinue?";
        ShowHorizontalLineAboveToolbar = "Show horizontal line above toolbar";
        BookmarkColor = "Bookmark color";
        SingleLineMaxLength = "Single line max length";
        OptimalCharsPerSec = "Optimal chars/sec";
        MaxCharsPerSec = "Max chars/sec";
        MaxWordsPerMin = "Max words/min";
        MinDurationMs = "Min duration (ms)";
        MaxDurationMs = "Max duration (ms)";
        MinGapMs = "Min gap (ms)";
        MaxLines = "Max number of lines";
        UnbreakSubtitlesShortThan = "Unbreak subtitles shorter than";
        NewEmptyDefaultMs = "Default new subtitle duration (ms)";
        PromptDeleteLines = "Prompt for delete lines";
        RememberPositionAndSize = "Remember window position and size";
        AutoBackupOn = "Auto-backup";
        AutoBackupIntervalMinutes = "Auto-backup interval (minutes)";
        AutoBackupDeleteAfterDays = "Auto-backup retention (days)";
        AutoConvertToUtf8 = "Auto-convert to UTF-8 on open";
        AutoTrimWhiteSpace = "Auto-trim white-space";
        DefaultEncoding = "Default encoding";
        ColorDurationTooShort = "Color duration if too short";
        ColorDurationTooLong = "Color duration if too long";
        ColorTextTooLong = "Color text if too long";
        ColorTextTooWide = "Color text if too wide (pixels)";
        ColorTextTooManyLines = "Color text if more than 2 lines";
        ColorOverlap = "Color time code overlap";
        ColorGapTooShort = "Color if gap is too short";
        ErrorBackgroundColor = "Error background color";
        WaveformDrawGridLines = "Draw grid lines";
        WaveformFocusOnMouseOver = "Focus on mouse over";
        WaveformCenterVideoPosition = "Center video position";
        WaveformShowToolbar = "Show toolbar";
        WaveformSpectrogramCombinedWaveformHeight = "Waveform/spectrogram combined, waveform height %";
        ShowWaveformToolbarPlay = "Toolbar: show play button";
        ShowWaveformToolbarRepeat = "Toolbar: show repeat button";
        ShowWaveformToolbarRemoveBlankLines = "Toolbar: show remove blank lines button";
        ShowWaveformToolbarNew = "Toolbar: show new subtitle button";
        ShowWaveformToolbarSetStart = "Toolbar: show set start button";
        ShowWaveformToolbarSetEnd = "Toolbar: show set end button";
        ShowWaveformToolbarStartAndOffsetTheRest = "Toolbar: show set start and offset the rest button";
        ShowWaveformToolbarHorizontalZoom = "Toolbar: show horizontal zoom slider";
        ShowWaveformToolbarVerticalZoom = "Toolbar: show vertical zoom slider";
        ShowWaveformToolbarVideoPositionSlider = "Toolbar: show video position slider";
        ShowWaveformToolbarPlaybackSpeed = "Toolbar: show playback speed";
        WaveformFocusTextboxAfterInsertNew = "Focus text box after insert";
        WaveformInvertMouseWheel = "Invert mouse-wheel";
        WaveformSnapToShotChanges = "Snap to shot changes";
        WaveformShotChangesAutoGenerate = "Shot changes auto-generate";
        WaveformColor = "Waveform color";
        WaveformBackgroundColor = "Waveform background color";
        WaveformSelectedColor = "Waveform selected color";
        DownloadFfmpeg = "Download ffmpeg";

        // Toolbar
        ShowToolbarNew = "Show new icon";
        ShowToolbarOpen = "Show open icon";
        ShowToolbarSave = "Show save icon";
        ShowToolbarSaveAs = "Show save as icon";
        ShowToolbarFind = "Show find icon";
        ShowToolbarReplace = "Show replace icon";
        ShowToolbarSpellCheck = "Show spell check icon";
        ShowToolbarSettings = "Show settings icon";
        ShowToolbarLayout = "Show layout icon";
        ShowToolbarHelp = "Show help icon";
        ShowToolbarEncoding = "Show encoding";
        ShowToolbarFrameRate = "Show frame rate";

        // Network
        ProxyAddress = "Proxy address";
        Username = "Username";
        Password = "Password";

        ShowStopButton = "Show stop button";
        ShowFullscreenButton = "Show full-screen button";
        AutoOpenVideoFile = "Auto-open video file when opening subtitle";
        DownloadMpv = "Download mpv";
        DownloadVlc = "Download VLC";
        GoToLineNumberSetsVideoPosition = "Go-to-line-number also sets video position";
        AdjustAllTimesRememberLineSelectionChoice = "Adjust all times, remember line selection choice";
        DefaultFormat = "Default format";
        DefaultSaveAsFormat = "Default \"Save as\" format";
        FavoriteSubtitleFormats = "Favorite subtitle formats";
        FilesAndLogs = "Files and logs";
        ShowErrorLogFile = "Show error log file";
        ShowWhisperLogFile = "Show Whisper log file";
        ShowSettingsFile = "Show settings file";
        ShowAssaLayer = "Show ASSA layer box";
        WaveformCursorColor = "Waveform cursor/head color";
        WaveformFancyHighColor = "Waveform fancy high color";

        ResetAllSettings = "Reset all settings";
        ResetShortcuts = "Reset shortcuts";
        ResetRecentFiles = "Reset recent files";
        ResetMultipleReplaceRules = "Reset multiple replace rules";
        ResetAppearance = "Reset appearance";
        ResetAutoTranslate = "Reset auto-translate";
        ResetWindowPositionAndSize = "Reset window position and size";
        ResetSettingsTitle = "Reset settings";
        OpenRuleFile = "Open rule file";
        ExportProfiles = "Export profiles";
        SaveRuleProfilesFile = "Save rule profiles file";
        RuleProfilesExportedX = "{0} rule profiles exported";
        RuleProfilesImportedX = "{0} rule profiles imported";
        UseSpecialStyleAfterLongGaps = "Use special style after long gaps";
        AddSpace = "Add space";
        ProcessIfEndsWithComma = "Process if ends with comma";
        RemoveComma = "Remove comma";
        EditContinuationStyleCustom = "Edit custom continuation style";
        LongGapThreshold = "Long gap threshold (ms)";
        AfterLongGap = "After long gap:";
        ResetSyntaxColoring = "Reset syntax coloring";
        ResetWaveform = "Reset waveform";
        ResetRules = "Reset rules";
        UseFrameMode = "Use frame mode (hh.mm.ss.ff)";
        TextBoxLimitNewLines = "Limit number of lines in subtitle text box";
        MpvVideoOutput = "Video output  (mpv)";
        MpvOpenGl = "libmpv - OpenGL";
        MpvWidRendering = "libmpv - Native Window ID rendering";
        MpvSoftwareRendering = "libmpv - Software rendering (slow)";
        VlcWidRendering = "libVLC - Native Window ID rendering";
        WaveFormsAndSpectrogramFoldersContainsX = "\"Waveforms\" and \"spectrogram\" folders contains {0}";
        DeleteWaveformAndSpectrogramFoldersQuestion = "Delete \"Waveforms\" and \"Spectrogram\" files?";
        WaveformGenerateSpectrogram = "Generate spectrogram";
        WaveformSpectrogramMode = "Spectrogram mode";
        WaveformCenterOnSingleClick = "Center on single click";
        WaveformSingleClickSelectsSubtitle = "Select subtitle on single click";
        WaveformRightClickSelectsSubtitle = "Select subtitle on right click";
        WaveformPauseOnSingleClick = "Pause on single click";
        WaveformDrawStyle = "Waveform draw style";
        SubtitleSEnterKeyAction = "Subtitle grid Enter-key action";
        SubtitleSingleClickAction = "Subtitle grid single-click action";
        SubtitleDoubleClickAction = "Subtitle grid double-click action";
        SaveAsBehavior = "\"Save as\" behavior";
        SaveAsAppendLanguageCode = "\"Save as\" append language code";
        GridGoToSubtitleAndSetVideoPosition = "Go to subtitle and set video position";
        GridGoToNextLine = "Go to next line";
        GridGoToSubtitleOnlyWaveformOnly = "Go to subtitle only (waveform only)";
        GridGoToSubtitleAndPause = "Go to subtitle and pause";
        GridGoToSubtitleAndPlay = "Go to subtitle and play";
        GridGoToSubtitleAndPauseAndFocusTextBox = "Go to subtitle and pause and focus text box";
        SubtitleGridFormattingNone = "No formatting";
        SubtitleGridFormattingShowFormatting = "Show formatting";
        SubtitleGridFormattingShowTags = "Show tags";
        WaveformParagraphBackgroundColor = "Waveform subtitle background color";
        WaveformParagraphSelectedBackgroundColor = "Waveform selected subtitle background color";
        WaveformAllowOverlap = "Allow overlap (when moving/resizing)";
        SaveAsBehaviorUseSubtitleVideoFilename = "Use subtitle/video file name";
        SaveAsBehaviorUseVideoSubtitleFilename = "Use video/subtitle file name";
        SaveAsBehaviorUseVideoFileName = "Use video file name";
        SaveAsBehaviorUseSubtitleFileName = "Use subtitle file name";
        SaveAsAppendLanguageCodeTwoLetter = "Two-letter";
        SaveAsAppendLanguageCodeThreeLetter = "Three-letter";
        SaveAsAppendLanguageCodeLanguageName = "Language name";
        SplitOddLineActionWeightTop = "Weight top";
        SplitOddLineActionWeightBottom = "Weight bottom";
        SplitOddLinesAction = "Split odd lines action";
        OcrUseWordSplitList = "OCR: use word split list";
        UseFocusedButtonBackgroundColor = "Use focused button background color";
        FocusedButtonBackgroundColor = "Focused button background color";
        ForceCrLfOnSave = "Force CR+LF on save (text subtitle files)";
        ShowWaveformToolbarPlayNext = "Toolbar: show play next button";
        ShowWaveformToolbarPlaySelection = "Toolbar: show play selection button";
        TextBoxButtonShowAutoBreak = "Text box: show auto-break button";
        TextBoxButtonShowUnbreak = "Text box: show unbreak button";
        TextBoxButtonShowItalic = "Text box: show italic button";
        TextBoxButtonShowColor = "Text box: show color button";
        TextBoxButtonShowRemoveFormatting = "Text box: show remove formatting button";
        WaveformSingleClickAction = "Waveform single-click action";
        WaveformDoubleClickAction = "Waveform double-click action (after single-click action)";
        AllSettings = "All settings";
    }

    public string GetContinuationStyleName(ContinuationStyle continuationStyle)
    {
        return continuationStyle switch
        {
            Core.Enums.ContinuationStyle.NoneTrailingDots => ContinuationStyleNoneTrailingDots,
            Core.Enums.ContinuationStyle.NoneLeadingTrailingDots => ContinuationStyleNoneLeadingTrailingDots,
            Core.Enums.ContinuationStyle.NoneTrailingEllipsis => ContinuationStyleNoneTrailingEllipsis,
            Core.Enums.ContinuationStyle.NoneLeadingTrailingEllipsis => ContinuationStyleNoneLeadingTrailingEllipsis,
            Core.Enums.ContinuationStyle.OnlyTrailingDots => ContinuationStyleOnlyTrailingDots,
            Core.Enums.ContinuationStyle.LeadingTrailingDots => ContinuationStyleLeadingTrailingDots,
            Core.Enums.ContinuationStyle.OnlyTrailingEllipsis => ContinuationStyleOnlyTrailingEllipsis,
            Core.Enums.ContinuationStyle.LeadingTrailingEllipsis => ContinuationStyleLeadingTrailingEllipsis,
            Core.Enums.ContinuationStyle.LeadingTrailingDash => ContinuationStyleLeadingTrailingDash,
            Core.Enums.ContinuationStyle.LeadingTrailingDashDots => ContinuationStyleLeadingTrailingDashDots,
            Core.Enums.ContinuationStyle.Custom => ContinuationStyleCustom,
            _ => ContinuationStyleNone,
        };
    }
}