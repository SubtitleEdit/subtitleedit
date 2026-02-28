﻿using Nikse.SubtitleEdit.Features.Main;
using System;

namespace Nikse.SubtitleEdit.Logic.Config.Language.Options;

public class LanguageSettingsShortcuts
{
    public string Title { get; set; }
    public string SearchShortcuts { get; set; }
    public string Filter { get; set; }

    public string CategoryGeneral { get; set; }
    public string CategorySubtitleGridAndTextBox { get; set; }
    public string CategorySubtitleGrid { get; set; }
    public string CategoryWaveform { get; set; }

    public string GeneralMergeSelectedLines { get; set; }
    public string GeneralMergeWithPrevious { get; set; }
    public string GeneralMergeWithNext { get; set; }
    public string GeneralMergeWithPreviousAndUnbreak { get; set; }
    public string GeneralMergeWithNextAndUnbreak { get; set; }
    public string GeneralMergeWithPreviousAndAutoBreak { get; set; }
    public string GeneralMergeWithNextAndAutoBreak { get; set; }
    public string GeneralMergeSelectedLinesAndAutoBreak { get; set; }
    public string GeneralMergeSelectedLinesAndUnbreak { get; set; }
    public string GeneralMergeSelectedLinesAndUnbreakCjk { get; set; }
    public string GeneralMergeSelectedLinesOnlyFirstText { get; set; }
    public string GeneralMergeSelectedLinesBilingual { get; set; }
    public string GeneralMergeWithPreviousBilingual { get; set; }
    public string GeneralMergeWithNextBilingual { get; set; }
    public string GeneralMergeOriginalAndTranslation { get; set; }
    public string GeneralToggleTranslationMode { get; set; }
    public string GeneralSwitchOriginalAndTranslation { get; set; }
    public string GeneralSwitchOriginalAndTranslationTextBoxes { get; set; }
    public string GeneralChooseLayout { get; set; }
    public string GeneralLayoutChooseX { get; set; }
    public string GeneralPlayFirstSelected { get; set; }
    public string GeneralGoToFirstSelectedLine { get; set; }
    public string GeneralGoToNextEmptyLine { get; set; }
    public string GeneralGoToNextSubtitle { get; set; }
    public string GeneralGoToNextSubtitlePlayTranslate { get; set; }
    public string GeneralGoToNextSubtitleCursorAtEnd { get; set; }
    public string GeneralGoToPrevSubtitle { get; set; }
    public string GeneralGoToPrevSubtitlePlayTranslate { get; set; }
    public string GeneralGoToStartOfCurrentSubtitle { get; set; }
    public string GeneralGoToEndOfCurrentSubtitle { get; set; }
    public string GeneralGoToPreviousSubtitleAndFocusVideo { get; set; }
    public string GeneralGoToNextSubtitleAndFocusVideo { get; set; }
    public string GeneralGoToPrevSubtitleAndPlay { get; set; }
    public string GeneralGoToNextSubtitleAndPlay { get; set; }
    public string GeneralGoToPreviousSubtitleAndFocusWaveform { get; set; }
    public string GeneralGoToNextSubtitleAndFocusWaveform { get; set; }
    public string GeneralGoToLineNumber { get; set; }
    public string GeneralGoToVideoPosition { get; set; }
    public string GeneralToggleItalic { get; set; }
    public string GeneralToggleBold { get; set; }
    public string GeneralToggleBookmarks { get; set; }
    public string GeneralFocusTextBox { get; set; }
    public string GeneralToggleBookmarksWithText { get; set; }
    public string GeneralEditBookmarks { get; set; }



    public string FileNew { get; set; }
    public string FileOpen { get; set; }
    public string FileOpenKeepVideo { get; set; }
    public string FileSave { get; set; }
    public string FileSaveAs { get; set; }
    public string FileSaveAll { get; set; }
    public string FileSaveOriginal { get; set; }
    public string FileSaveOriginalAs { get; set; }
    public string FileOpenOriginalSubtitle { get; set; }
    public string FileCloseOriginalSubtitle { get; set; }
    public string FileTranslatedSubtitle { get; set; }
    public string FileCompare { get; set; }
    public string FileStatistics { get; set; }
    public string FileImportPlainText { get; set; }
    public string FileImportBluRaySupForOcr { get; set; }
    public string FileImportBluRaySupForEdit { get; set; }
    public string FileImportTimeCodes { get; set; }
    public string FileExportEbuStl { get; set; }
    public string FileExportPac { get; set; }
    public string FileExportEdlClipName { get; set; }
    public string FileExportPlainText { get; set; }
    public string FileExportCustomTextFormat1 { get; set; }
    public string FileExportCustomTextFormat2 { get; set; }
    public string FileExportCustomTextFormat3 { get; set; }
    public string FileExit { get; set; }
    public string OpenSeDataFolder { get; set; }

    public string EditFind { get; set; }
    public string EditFindNext { get; set; }
    public string EditFindPrevious { get; set; }
    public string EditReplace { get; set; }
    public string EditMultipleReplace { get; set; }
    public string EditModifySelection { get; set; }

    public string ListSelectAll { get; set; }
    public string ListSelectFirst { get; set; }
    public string ListSelectLast { get; set; }
    public string ListInverseSelection { get; set; }
    public string ListDeleteSelection { get; set; }
    public string RippleDeleteSelection { get; set; }

    public string Settings { get; set; }
    public string Assigned { get; set; }
    public string Unassigned { get; set; }
    public string PressedKeyX { get; set; }
    public string PressAKey { get; set; }
    public string DetectKey { get; set; }
    public string Control { get; set; }
    public string Alt { get; set; }
    public string Win { get; set; }
    public string Shift { get; set; }
    public string ControlMac { get; set; }
    public string AltMac { get; set; }
    public string WinMac { get; set; }
    public string ShiftMac { get; set; }
    
    public string ResetShortcuts { get; set; }
    public string ResetShortcutsDetail { get; set; }
    public string TogglePlayPause { get; set; }
    public string ToggleLockTimeCodes { get; set; }
    public string DuplicateSelectedLines { get; set; }
    public string SourceView { get; set; }
    public string ShowAlignmentPicker { get; set; }
    public string AddOrEditBookmark { get; set; }
    public string ListBookmarks { get; set; }
    public string ToggleBookmark { get; set; }
    public string GoToNextBookmark { get; set; }
    public string ToggleWaveformToolbar { get; set; }
    public string WaveformSetStartAndSetEndOfPreviousMinusGap { get; set; }
    public string WaveformSetEndAndStartOfNextAfterGap { get; set; }
    public string WaveformSetEndAndStartOfNextAfterGapAndGoToNext { get; set; }
    public string FetchFirstWordFromNextSubtitle { get; set; }
    public string MoveLastWordToNextSubtitle { get; set; }
    public string MoveLastWordFromFirstLineDownCurrentSubtitle { get; set; }
    public string MoveFirstWordFromNextLineUpCurrentSubtitle { get; set; }
    public string ToggleFocusGridAndWaveform { get; set; }
    public string ToggleFocusTextBoxAndWaveform { get; set; }
    public string ToggleFocusTextBoxAndGrid { get; set; }
    public string GoToPreviousLineAndSetVideoPosition { get; set; }
    public string GoToPreviousLineFromVideoPosition { get; set; }
    public string GoToNextLineFromVideoPosition { get; set; }
    public string GoToNextLineAndSetVideoPosition { get; set; }
    public string TextBoxDeleteSelectionNoClipboard { get; set; }
    public string TextBoxCut { get; set; }
    public string TextBoxCut2 { get; set; }
    public string TextBoxPaste { get; set; }
    public string TextBoxCopy { get; set; }
    public string TextBoxSelectAll { get; set; }
    public string SubtitleGridCut { get; set; }
    public string SubtitleGridCopy { get; set; }
    public string SubtitleGridPaste { get; set; }
    public string SetShortcutForX { get; set; }
    public string CommandFileNewKeepVideo { get; set; }
    public string FileOpenOriginal { get; set; }
    public string FileCloseOriginal { get; set; }
    public string RestoreAutoBackup { get; set; }
    public string OpenContainingFolder { get; set; }
    public string ImportTimeCodes { get; set; }
    public string ImportSubtitleWithManuallyChosenEncoding { get; set; }
    public string ExportBluRaySup { get; set; }
    public string ExportCustomTextFormat { get; set; }
    public string ExportPlainText { get; set; }
    public string ShowHistory { get; set; }
    public string ToggleRightToLeft { get; set; }
    public string ModifySelection { get; set; }
    public string AdjustDurations { get; set; }
    public string ApplyDurationLimits { get; set; }
    public string BatchConvert { get; set; }
    public string BridgeGaps { get; set; }
    public string ApplyMinGap { get; set; }
    public string FixCommonErrors { get; set; }
    public string MakeEmptyTranslationFromCurrentSubtitle { get; set; }
    public string MergeLinesWithSameText { get; set; }
    public string MergeLinesWithSameTimeCodes { get; set; }
    public string SplitBreakLongLines { get; set; }
    public string MergeShortLines { get; set; }
    public string RemoveTextForHearingImpaired { get; set; }
    public string JoinSubtitles { get; set; }
    public string SplitSubtitle { get; set; }
    public string SpellCheck { get;  set; }
    public string SpellCheckGetDictionary { get;  set; }
    public string OpenVideo { get;  set; }
    public string OpenVideoFromUrl { get;  set; }
    public string CloseVideo { get;  set; }
    public string SpeechToText { get;  set; }
    public string TextToSpeech { get;  set; }
    public string BurnIn { get;  set; }
    public string GenerateTransparent { get;  set; }
    public string UndockVideoControls { get; set; }
    public string RedockVideoControls { get; set; }
    public string GenerateBlankVideo { get; set; }
    public string ReencodeVideo { get; set; }
    public string CutVideo { get; set; }
    public string CutVideoSelectedLines { get; set; }
    public string AdjustAllTimes { get; set; }
    public string VisualSync { get; set; }
    public string TranslateViaCopyPaste { get; set; }
    public string Shortcuts { get; set; }
    public string WordLists { get; set; }
    public string ChooseUiLanguage { get; set; }
    public string ChooseRuleProfile { get; set; }
    public string VideoFullScreen { get; set; }
    public string CopyTextFromOriginalSelectedLines { get; set; }
    public string TextBoxRemoveAllFormatting { get; set; }
    public string TextBoxItalic { get; set; }
    public string ResetWaveformZoomAndSpeed { get; set; }
    public string TogglePlaybackSpeed { get; set; }
    public string PlaybackSpeedSlower { get; set; }
    public string PlaybackSpeedFaster { get; set; }
    public string SwitchOriginalAndTranslationSelectedLines { get; set; }
    public string MergeOriginalIntoTranslationSelectedLines { get; set; }
    public string SeekSilence { get; set; }
    public string SetVideoPositionCurrentSubtitleStart { get; set; }
    public string SetVideoPositionCurrentSubtitleEnd { get; set; }
    public string ToggleAudioTracks { get; set; }
    public string ListErrors { get; set; }
    public string GoToNextError { get;set; }
    public string GoToPreviousError { get; set; }
    public string AddNameToNameList { get; set; }
    public string FindDoubleWords { get; set; }
    public string ColorX { get; set; }
    public string RemoveColor { get; set; }
    public string SurroundWith { get; set; }
    public string SurroundWithXY { get; set; }
    public string RepeatLine { get; set; }
    public string RepeatPreviousLine { get; set; }
    public string RepeatNextLine { get; set; }
    public string MoveVideoPositionMilliseconds { get; set; }
    public string ImportShortcutsTitle { get; set; }
    public string ExportShortcutsTitle { get; set; }
    public string XShortcutsImportedFromY { get; set; }
    public string XShortcutsExportedToY { get; set; }
    public string ImportImageSubtitleForEdit { get; set; }
    public string ShowPointSyncViaOther { get; set; }
    public string ShowPointSync { get; set; }
    public string ShowMediaInformation { get; set; }
    public string ChooseSubtitleFormat { get; set; }
    public string TrimWhitespaceSelectedLines { get; set; }
    public string WaveformHorizontalZoomInCommand { get; set; }
    public string WaveformHorizontalZoomOutCommand { get; set; }
    public string WaveformVerticalZoomInCommand { get; set; }
    public string WaveformVerticalZoomOutCommand { get; set; }
    public string CopySubtitlePathToClipboard { get; set; }
    public string CopySubtitleOriginalPathToClipboard { get; set; }
    public string FocusTextBox { get; set; }
    public string SortByStartTime { get; set; }
    public string SortByEndTime { get; set; }
    public string DuplicatesFound { get; set; }
    public string CopyTextToClipboard { get; set; }
    public string CopyTextFromOriginalToClipboard { get; set; }
    public string AssaDraw { get; set; }
    public string AssaGenerateProgressBar { get; set; }
    public string AssaGenerateBackgroundBox { get; set; }
    public string AssaStyles { get; set; }
    public string AssaProperties { get; set; }
    public string AssaAttachments { get; set; }
    public string AssaVideoColorPicker { get; set; }
    public string RecalculateDurationSelectedLines { get; set; }
    public string ToggleWaveformAndSpectrogramHeight { get; set; }
    public string ToggleSpectrogramStyle { get; set; }
    public string CopyMsRelativeToCurrentSubtitleLineToClipboard { get; set; }

    public LanguageSettingsShortcuts()
    {
        Title = "Shortcuts";
        SearchShortcuts = "Search shortcuts...";
        Filter = "Filter";

        CategoryGeneral = "General";
        CategorySubtitleGridAndTextBox = "Subtitle list view & text box";
        CategorySubtitleGrid = "Subtitle list view";
        CategoryWaveform = "Waveform";

        GeneralMergeSelectedLines = "Merge selected lines";
        GeneralMergeWithPrevious = "Merge with previous";
        GeneralMergeWithNext = "Merge with next";
        GeneralMergeWithPreviousAndUnbreak = "Merge with previous and unbreak";
        GeneralMergeWithNextAndUnbreak = "Merge with next and unbreak";
        GeneralMergeWithPreviousAndAutoBreak = "Merge with previous and auto-break";
        GeneralMergeWithNextAndAutoBreak = "Merge with next and auto-break";
        GeneralMergeSelectedLinesAndAutoBreak = "Merge selected lines and auto-break";
        GeneralMergeSelectedLinesAndUnbreak = "Merge selected lines and unbreak";
        GeneralMergeSelectedLinesAndUnbreakCjk = "Merge selected lines and unbreak CJK";
        GeneralMergeSelectedLinesOnlyFirstText = "Merge selected lines only first text";
        GeneralMergeSelectedLinesBilingual = "Merge selected lines bilingual";
        GeneralMergeWithPreviousBilingual = "Merge with previous bilingual";
        GeneralMergeWithNextBilingual = "Merge with next bilingual";
        GeneralMergeOriginalAndTranslation = "Merge original and translation";
        GeneralToggleTranslationMode = "Toggle translation mode";
        GeneralSwitchOriginalAndTranslation = "Switch original and translation";
        GeneralSwitchOriginalAndTranslationTextBoxes = "Switch original and translation text boxes";
        GeneralChooseLayout = "Choose layout";
        GeneralLayoutChooseX = "Layout {0}";
        GeneralPlayFirstSelected = "Play first selected";
        GeneralGoToFirstSelectedLine = "Go to first selected line";
        GeneralGoToNextEmptyLine = "Go to next empty line";
        GeneralGoToNextSubtitle = "Go to next subtitle";
        GeneralGoToNextSubtitlePlayTranslate = "Go to next subtitle (play translate)";
        GeneralGoToNextSubtitleCursorAtEnd = "Go to next subtitle (cursor at end)";
        GeneralGoToPrevSubtitle = "Go to previous subtitle";
        GeneralGoToPrevSubtitlePlayTranslate = "Go to previous subtitle (play translate)";
        GeneralGoToStartOfCurrentSubtitle = "Go to start of current subtitle";
        GeneralGoToEndOfCurrentSubtitle = "Go to end of current subtitle";
        GeneralGoToPreviousSubtitleAndFocusVideo = "Go to previous subtitle and focus video";
        GeneralGoToNextSubtitleAndFocusVideo = "Go to next subtitle and focus video";
        GeneralGoToPrevSubtitleAndPlay = "Go to previous subtitle and play";
        GeneralGoToNextSubtitleAndPlay = "Go to next subtitle and play";
        GeneralGoToPreviousSubtitleAndFocusWaveform = "Go to previous subtitle and focus waveform";
        GeneralGoToNextSubtitleAndFocusWaveform = "Go to next subtitle and focus waveform";
        GeneralGoToLineNumber = "Go to line number";
        GeneralGoToVideoPosition = "Go to video position";
        GeneralToggleItalic = "Toggle italic";
        GeneralToggleBold = "Toggle bold";
        GeneralToggleBookmarks = "Toggle bookmarks";
        GeneralFocusTextBox = "Focus text box";
        GeneralToggleBookmarksWithText = "Toggle bookmarks with text";
        GeneralEditBookmarks = "Edit bookmarks";

        FileNew = "New";
        FileOpen = "Open";
        FileOpenKeepVideo = "Open (keep video)";
        FileSave = "Save";
        FileSaveAs = "Save as";
        FileSaveAll = "Save all";
        FileSaveOriginal = "Save original";
        FileSaveOriginalAs = "Save original as";
        FileOpenOriginalSubtitle = "Open original subtitle";
        FileCloseOriginalSubtitle = "Close original subtitle";
        FileTranslatedSubtitle = "Translated subtitle";
        FileStatistics = "Statistics";
        FileCompare = "Compare";
        FileImportPlainText = "Import plain text";
        FileImportBluRaySupForOcr = "Import Blu-ray SUP for OCR";
        FileImportBluRaySupForEdit = "Import Blu-ray SUP for edit";
        FileImportTimeCodes = "Import time codes";
        FileExportEbuStl = "Export EBU STL";
        FileExportPac = "Export PAC";
        FileExportEdlClipName = "Export EDL clip name";
        FileExportPlainText = "Export plain text";
        FileExportCustomTextFormat1 = "Export custom text format 1";
        FileExportCustomTextFormat2 = "Export custom text format 2";
        FileExportCustomTextFormat3 = "Export custom text format 3";
        FileExit = "Exit";
        OpenSeDataFolder = "Open Subtitle Edit folder";

        EditFind = "Find";
        EditFindNext = "Find next";
        EditFindPrevious = "Find previous";
        EditReplace = "Replace";
        EditMultipleReplace = "Multiple replace";
        EditModifySelection = "Modify selection";

        ListSelectAll = "Select all";
        ListSelectFirst = "Select first";
        ListSelectLast = "Select last";
        ListInverseSelection = "Inverse selection";
        ListDeleteSelection = "Delete selection";
        RippleDeleteSelection = "Ripple delete selection";

        TogglePlayPause = "Toggle play/pause";

        Settings = "Settings";

        Assigned = "Assigned";
        Unassigned = "Unassigned";
        
        PressedKeyX = "Pressed key: {0}";
        PressAKey = "Press a key";
        
        DetectKey = "Detect key";
        
        Control = "Control";
        Alt = "Alt";
        Win = "Win";
        Shift = "Shift";

        ControlMac = "⌃";
        AltMac = "⌥";
        ShiftMac = "⇧";
        WinMac = "⌘";

        ResetShortcuts = "Reset shortcuts";
        ResetShortcutsDetail = "Do you want to reset all shortcuts to default values?";
        ToggleLockTimeCodes = "Toggle lock time codes";
        DuplicateSelectedLines = "Duplicate selected lines";
        SourceView = "Source view";
        ShowAlignmentPicker = "Alignment";
        AddOrEditBookmark = "Add or edit bookmark";
        ListBookmarks = "List bookmarks";
        ToggleBookmark = "Toggle bookmark (selected lines, no text)";
        GoToNextBookmark = "Go to next bookmark";
        ToggleWaveformToolbar = "Toggle waveform toolbar";
        WaveformSetStartAndSetEndOfPreviousMinusGap = "Set start and set end of previous minus gap";
        WaveformSetEndAndStartOfNextAfterGap = "Set end and start of next plus gap";
        WaveformSetEndAndStartOfNextAfterGapAndGoToNext = "Set end and start of next plus gap and go to next";
        FetchFirstWordFromNextSubtitle = "Fetch first word from next subtitle";
        MoveLastWordToNextSubtitle = "Move last word to next subtitle";
        MoveLastWordFromFirstLineDownCurrentSubtitle = "Move last word from first line down (current subtitle)";
        MoveFirstWordFromNextLineUpCurrentSubtitle = "Move first word from next line up (current subtitle)";
        ToggleFocusGridAndWaveform = "Toggle focus between subtitle grid and waveform/spectrogram";
        ToggleFocusTextBoxAndWaveform = "Toggle focus between text box and waveform/spectrogram";
        ToggleFocusTextBoxAndGrid = "Toggle focus between text box and subtitle grid";
        GoToPreviousLineAndSetVideoPosition = "Go to previous subtitle and set video position";
        GoToNextLineAndSetVideoPosition = "Go to next subtitle and set video position";
        GoToPreviousLineFromVideoPosition = "Go to previous subtitle (from current video position)";
        GoToNextLineFromVideoPosition = "Go to next subtitle (from current video position)";
        TextBoxDeleteSelectionNoClipboard = "Text box: Delete selection (no clipboard)";
        TextBoxCut = "Text box: Cut";
        TextBoxCut2 = "Text box: Cut (alternative)";
        TextBoxPaste = "Text box: Paste";
        TextBoxCopy = "Text box: Copy";
        TextBoxSelectAll = "Text box: Select all";
        SubtitleGridCut = "Subtitle grid: Cut";
        SubtitleGridCopy = "Subtitle grid: Copy";
        SubtitleGridPaste = "Subtitle grid: Paste";
        SetShortcutForX = "Set shortcut for \"{0}\"";
        CommandFileNewKeepVideo = "New (keep video)";
        FileOpenOriginal = "Open original";
        FileCloseOriginal = "Close original";
        RestoreAutoBackup = "Restore auto-backup";
        OpenContainingFolder = "Open containing folder";
        ImportTimeCodes = "Import time codes";
        ImportSubtitleWithManuallyChosenEncoding = "Import subtitle with manually chosen encoding";
        ExportBluRaySup = "Export Blu-ray SUP";
        ExportCustomTextFormat = "Export custom text format";
        ExportPlainText = "Export plain text";
        ShowHistory = "Show history";
        ToggleRightToLeft = "Toggle right-to-left";
        ModifySelection = "Modify selection";
        AdjustDurations = "Adjust durations";
        ApplyDurationLimits = "Apply duration limits";
        BatchConvert = "Batch convert";
        BridgeGaps = "Bridge gaps";
        ApplyMinGap = "Apply min gap";
        FixCommonErrors = "Fix common errors";
        MakeEmptyTranslationFromCurrentSubtitle = "Make empty translation from current subtitle";
        MergeLinesWithSameText = "Merge lines with same text";
        MergeLinesWithSameTimeCodes = "Merge lines with same time codes";
        SplitBreakLongLines = "Split/rebalance long lines";
        MergeShortLines = "Merge short lines";
        RemoveTextForHearingImpaired = "Remove text for hearing impaired";
        JoinSubtitles = "Join subtitles";
        SplitSubtitle = "Split subtitle";
        SpellCheck = "Spell check";
        SpellCheckGetDictionary = "Get spell check dictionary";
        OpenVideo = "Open video";
        OpenVideoFromUrl = "Open video from URL";
        CloseVideo = "Close video";
        SpeechToText = "Speech to text (Whisper)";
        TextToSpeech = "Text to speech";
        BurnIn = "Generate video with burned-in subtitles";
        GenerateTransparent = "Generate transparent video with subtitles";
        UndockVideoControls = "Un-dock video controls";
        RedockVideoControls = "Re-dock video controls";
        GenerateBlankVideo = "Generate blank video";
        ReencodeVideo = "Re-encode video";
        CutVideo = "Cut video";
        CutVideoSelectedLines = "Cut video (selected lines)";
        AdjustAllTimes = "Adjust all times";
        VisualSync = "Visual sync";
        TranslateViaCopyPaste = "Translate via copy-paste";
        Shortcuts = "Shortcuts";
        WordLists = "Word lists";
        ChooseUiLanguage = "Choose UI language";
        ChooseRuleProfile = "Choose rule profile";
        VideoFullScreen = "Video full screen";
        CopyTextFromOriginalSelectedLines = "Copy text from original (selected lines)";
        TextBoxRemoveAllFormatting = "Text box, remove all formatting";
        TextBoxItalic = "Text box italic";
        ResetWaveformZoomAndSpeed = "Reset waveform zoom and playback speed (play rate)";
        TogglePlaybackSpeed = "Toggle playback speed (play rate)";
        PlaybackSpeedSlower = "Playback speed slower (play rate)";
        PlaybackSpeedFaster = "Playback speed faster (play rate)";
        SwitchOriginalAndTranslationSelectedLines = "Switch original and translation (selected lines)";
        MergeOriginalIntoTranslationSelectedLines = "Merge original and translation (selected lines)";
        SeekSilence = "Seek silence";
        SetVideoPositionCurrentSubtitleStart = "Set video position to current line start";
        SetVideoPositionCurrentSubtitleEnd = "Set video position to current line end";
        ToggleAudioTracks = "Toggle audio tracks";
        ListErrors = "List errors";
        GoToPreviousError = "GoTo previous error";
        GoToNextError = "GoTo next error";
        AddNameToNameList = "Add name to name list";
        FindDoubleWords = "Find double words";
        ColorX = "Color {0}";
        RemoveColor = "Remove color";
        SurroundWith = "Surround with...";
        SurroundWithXY = "Surround with {0}/{1}";
        RepeatLine = "Repeat line";
        RepeatPreviousLine = "Repeat previous line";
        RepeatNextLine = "Repeat next line";
        MoveVideoPositionMilliseconds = "Move video position in milliseconds";
        ImportShortcutsTitle = "Import shortcuts";
        ExportShortcutsTitle = "Export shortcuts";
        XShortcutsImportedFromY = "{0} shortcuts imported from {1}";
        XShortcutsExportedToY = "{0} shortcuts exported to {1}";
        ImportImageSubtitleForEdit = "Import image-based subtitle for edit";
        ShowPointSyncViaOther = "Show point sync via other subtitle";
        ShowPointSync = "Show point sync";
        ShowMediaInformation = "Show media information";
        ChooseSubtitleFormat = "Choose subtitle format";
        TrimWhitespaceSelectedLines = "Trim whitespace (selected lines)";
        WaveformHorizontalZoomInCommand = "Waveform horizontal zoom in";
        WaveformHorizontalZoomOutCommand = "Waveform horizontal zoom out";
        WaveformVerticalZoomInCommand = "Waveform vertical zoom in";
        WaveformVerticalZoomOutCommand = "Waveform vertical zoom out";
        CopySubtitlePathToClipboard = "Copy subtitle path to clipboard";
        CopySubtitleOriginalPathToClipboard = "Copy subtitle path of original to clipboard";
        FocusTextBox = "Focus text box";
        SortByStartTime = "Sort by \"Show\" time";
        SortByEndTime = "Sort by \"Hide\" time";
        DuplicatesFound = "The following duplicate shortcuts were found:";
        CopyTextToClipboard = "Copy text to clipboard (selected lines)";
        CopyTextFromOriginalToClipboard = "Copy text from original to clipboard (selected lines)";
        AssaDraw = "ASSA Draw";
        AssaGenerateProgressBar = "ASSA Generate progress bar";
        AssaGenerateBackgroundBox = "ASSA Generate background box";
        AssaStyles = "ASSA Styles";
        AssaAttachments = "ASSA Attachments";   
        AssaProperties = "ASSA Properties";
        AssaVideoColorPicker = "ASSA Video color picker";
        RecalculateDurationSelectedLines = "Recalculate duration (selected lines)";
        ToggleWaveformAndSpectrogramHeight = "Toggle waveform/spectrogram divided height";
        ToggleSpectrogramStyle = "Toggle spectrogram style";
        CopyMsRelativeToCurrentSubtitleLineToClipboard = "Copy milliseconds relative to current subtitle line to clipboard";
    }
}