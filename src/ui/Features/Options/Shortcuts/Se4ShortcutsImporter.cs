using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Nikse.SubtitleEdit.Features.Options.Shortcuts;

// Maps SE 4 (WinForms) <Shortcuts> entries from Settings.xml onto SE 5
// MainViewModel command names. SE 4 has more action slots than SE 5 ships
// today, so entries with no SE 5 counterpart are silently dropped — the
// importer reports the count of imported and skipped items so the caller
// can surface that to the user.
public static class Se4ShortcutsImporter
{
    public sealed class ImportResult
    {
        public List<SeShortCut> Shortcuts { get; } = new();
        public int SkippedNoMapping { get; set; }
        public int SkippedEmpty { get; set; }
    }

    // SE 4 setting property name → SE 5 MainViewModel command name.
    // Built by walking the <Shortcuts> element of SE 4's Settings.xml and
    // matching each name to its closest SE 5 equivalent. Entries without a
    // confident SE 5 mapping are omitted; the importer reports them as
    // "skipped" so the user knows their full set isn't carried over.
    private static readonly Dictionary<string, string> Map = new(StringComparer.Ordinal)
    {
        // File
        ["MainFileNew"] = nameof(MainViewModel.CommandFileNewCommand),
        ["MainFileOpen"] = nameof(MainViewModel.CommandFileOpenCommand),
        ["MainFileOpenKeepVideo"] = nameof(MainViewModel.CommandFileOpenKeepVideoCommand),
        ["MainFileSave"] = nameof(MainViewModel.CommandFileSaveCommand),
        ["MainFileSaveAll"] = nameof(MainViewModel.CommandFileSaveCommand),
        ["MainFileSaveAs"] = nameof(MainViewModel.CommandFileSaveAsCommand),
        ["MainFileOpenOriginal"] = nameof(MainViewModel.FileOpenOriginalCommand),
        ["MainFileCloseOriginal"] = nameof(MainViewModel.FileCloseOriginalCommand),
        ["MainFileCloseTranslation"] = nameof(MainViewModel.FileCloseTranslationCommand),
        ["MainFileCompare"] = nameof(MainViewModel.ShowCompareCommand),
        ["MainFileImportTimeCodes"] = nameof(MainViewModel.ImportTimeCodesCommand),
        ["MainFileImportPlainText"] = nameof(MainViewModel.ImportPlainTextCommand),
        ["MainFileExportPlainText"] = nameof(MainViewModel.ShowExportPlainTextCommand),
        ["MainFileExportBdSup"] = nameof(MainViewModel.ExportBluRaySupCommand),
        ["MainFileExportEbu"] = nameof(MainViewModel.ExportEbuStlCommand),
        ["MainFileExportPac"] = nameof(MainViewModel.ExportPacCommand),
        ["MainFileExportCustomText1"] = nameof(MainViewModel.ShowExportCustomTextFormatCommand),
        ["MainFileExportCustomText2"] = nameof(MainViewModel.ShowExportCustomTextFormatCommand),
        ["MainFileExportCustomText3"] = nameof(MainViewModel.ShowExportCustomTextFormatCommand),
        ["MainFileImportBdSupForEdit"] = nameof(MainViewModel.ImportImageSubtitleForEditCommand),
        ["MainFileExit"] = nameof(MainViewModel.CommandExitCommand),

        // Edit
        ["MainEditUndo"] = nameof(MainViewModel.UndoCommand),
        ["MainEditRedo"] = nameof(MainViewModel.RedoCommand),
        ["MainEditFind"] = nameof(MainViewModel.ShowFindCommand),
        ["MainEditFindNext"] = nameof(MainViewModel.FindNextCommand),
        ["MainEditReplace"] = nameof(MainViewModel.ShowReplaceCommand),
        ["MainEditMultipleReplace"] = nameof(MainViewModel.ShowMultipleReplaceCommand),
        ["MainEditGoToLineNumber"] = nameof(MainViewModel.ShowGoToLineCommand),
        ["MainEditRightToLeft"] = nameof(MainViewModel.RightToLeftToggleCommand),
        ["MainEditReverseStartAndEndingForRTL"] = nameof(MainViewModel.ReverseRightToLeftStartEndCommand),
        ["MainEditInverseSelection"] = nameof(MainViewModel.InverseSelectionCommand),
        ["MainEditModifySelection"] = nameof(MainViewModel.ShowModifySelectionCommand),
        ["MainEditFixRTLViaUnicodeChars"] = nameof(MainViewModel.FixRightToLeftViaUnicodeControlCharactersCommand),
        ["MainEditRemoveRTLUnicodeChars"] = nameof(MainViewModel.RemoveUnicodeControlCharactersCommand),

        // Tools
        ["MainToolsAdjustDuration"] = nameof(MainViewModel.ShowToolsAdjustDurationsCommand),
        ["MainToolsAdjustDurationLimits"] = nameof(MainViewModel.ShowApplyDurationLimitsCommand),
        ["MainToolsFixCommonErrors"] = nameof(MainViewModel.ShowToolsFixCommonErrorsCommand),
        ["MainToolsMergeShortLines"] = nameof(MainViewModel.ShowToolsMergeShortLinesCommand),
        ["MainToolsMergeDuplicateText"] = nameof(MainViewModel.ShowToolsMergeLinesWithSameTextCommand),
        ["MainToolsMergeSameTimeCodes"] = nameof(MainViewModel.ShowToolsMergeLinesWithSameTimeCodesCommand),
        ["MainToolsMakeEmptyFromCurrent"] = nameof(MainViewModel.ToolsMakeEmptyTranslationFromCurrentSubtitleCommand),
        ["MainToolsSplitLongLines"] = nameof(MainViewModel.ShowToolsSplitBreakLongLinesCommand),
        ["MainToolsMinimumDisplayTimeBetweenParagraphs"] = nameof(MainViewModel.ShowApplyMinGapCommand),
        ["MainToolsDurationsBridgeGap"] = nameof(MainViewModel.ShowBridgeGapsCommand),
        ["MainToolsRemoveTextForHI"] = nameof(MainViewModel.ShowToolsRemoveTextForHearingImpairedCommand),
        ["MainToolsChangeCasing"] = nameof(MainViewModel.ShowToolsChangeCasingCommand),
        ["MainToolsBatchConvert"] = nameof(MainViewModel.ShowToolsBatchConvertCommand),
        ["MainToolsSplit"] = nameof(MainViewModel.ShowToolsSplitCommand),
        ["MainToolsJoin"] = nameof(MainViewModel.ShowToolsJoinCommand),
        ["MainToolsBeautifyTimeCodes"] = nameof(MainViewModel.ShowBeautifyTimeCodesCommand),
        ["MainToolsStyleManager"] = nameof(MainViewModel.ShowAssaStylesCommand),
        ["MainToolsAutoDuration"] = nameof(MainViewModel.RecalculateDurationSelectedLinesCommand),
        ["MainToolsRenumber"] = nameof(MainViewModel.ShowToolsRenumberCommand),

        // Spell check
        ["MainSpellCheck"] = nameof(MainViewModel.ShowSpellCheckCommand),
        ["MainSpellCheckFindDoubleWords"] = nameof(MainViewModel.ShowFindDoubleWordsCommand),
        ["MainSpellCheckAddWordToNames"] = nameof(MainViewModel.ShowAddToNameListCommand),

        // Video
        ["MainVideoOpen"] = nameof(MainViewModel.CommandVideoOpenCommand),
        ["MainVideoClose"] = nameof(MainViewModel.CommandVideoCloseCommand),
        ["MainVideoPause"] = nameof(MainViewModel.PauseCommand),
        ["MainVideoPlayPauseToggle"] = nameof(MainViewModel.TogglePlayPauseCommand),
        ["MainVideo1FrameLeft"] = nameof(MainViewModel.VideoOneFrameBackCommand),
        ["MainVideo1FrameRight"] = nameof(MainViewModel.VideoOneFrameForwardCommand),
        ["MainVideo100MsLeft"] = nameof(MainViewModel.Video100MsBackCommand),
        ["MainVideo100MsRight"] = nameof(MainViewModel.Video100MsForwardCommand),
        ["MainVideo500MsLeft"] = nameof(MainViewModel.Video500MsBackCommand),
        ["MainVideo500MsRight"] = nameof(MainViewModel.Video500MsForwardCommand),
        ["MainVideo1000MsLeft"] = nameof(MainViewModel.VideoOneSecondBackCommand),
        ["MainVideo1000MsRight"] = nameof(MainViewModel.VideoOneSecondForwardCommand),
        ["MainVideoXSMsLeft"] = nameof(MainViewModel.VideoMoveCustom1BackCommand),
        ["MainVideoXSMsRight"] = nameof(MainViewModel.VideoMoveCustom1ForwardCommand),
        ["MainVideoXLMsLeft"] = nameof(MainViewModel.VideoMoveCustom2BackCommand),
        ["MainVideoXLMsRight"] = nameof(MainViewModel.VideoMoveCustom2ForwardCommand),
        ["MainVideoFullscreen"] = nameof(MainViewModel.VideoFullScreenCommand),
        ["MainVideoSlower"] = nameof(MainViewModel.PlaybackSlowerCommand),
        ["MainVideoFaster"] = nameof(MainViewModel.PlaybackFasterCommand),
        ["MainVideoSpeedToggle"] = nameof(MainViewModel.TogglePlaybackSpeedCommand),
        ["MainVideoAudioToTextWhisper"] = nameof(MainViewModel.ShowSpeechToTextWhisperCommand),
        ["MainVideoTextToSpeech"] = nameof(MainViewModel.ShowVideoTextToSpeechCommand),
        ["MainVideoToggleBrightness"] = nameof(MainViewModel.VideoToggleBrightnessCommand),
        ["MainVideoToggleVideoControls"] = nameof(MainViewModel.VideoUndockControlsCommand),
        ["MainVideoPlayFromBeginning"] = nameof(MainViewModel.PlayFromStartOfVideoCommand),
        ["MainVideoPlaySelectedLines"] = nameof(MainViewModel.PlaySelectedLinesWithoutLoopCommand),
        ["MainVideoLoopSelectedLines"] = nameof(MainViewModel.PlaySelectedLinesWithLoopCommand),
        ["MainVideoReset"] = nameof(MainViewModel.ResetWaveformZoomAndSpeedCommand),
        ["MainVideoGoToStartCurrent"] = nameof(MainViewModel.VideoSetPositionCurrentSubtitleStartCommand),
        ["MainVideoGoToPrevSubtitle"] = nameof(MainViewModel.GoToPreviousLineAndSetVideoPositionCommand),
        ["MainVideoGoToNextSubtitle"] = nameof(MainViewModel.GoToNextLineAndSetVideoPositionCommand),
        ["MainVideoStop"] = nameof(MainViewModel.PauseCommand),
        ["MainVideoSelectNextSubtitle"] = nameof(MainViewModel.GoToNextLineCommand),

        // Synchronization
        ["MainSynchronizationAdjustTimes"] = nameof(MainViewModel.ShowSyncAdjustAllTimesCommand),
        ["MainSynchronizationVisualSync"] = nameof(MainViewModel.ShowVisualSyncCommand),
        ["MainSynchronizationPointSync"] = nameof(MainViewModel.ShowPointSyncCommand),
        ["MainSynchronizationPointSyncViaFile"] = nameof(MainViewModel.ShowPointSyncViaOtherCommand),
        ["MainSynchronizationChangeFrameRate"] = nameof(MainViewModel.ShowSyncChangeFrameRateCommand),

        // List view
        ["MainListViewItalic"] = nameof(MainViewModel.ToggleLinesItalicOrSelectedTextCommand),
        ["MainListViewBold"] = nameof(MainViewModel.ToggleLinesBoldCommand),
        ["MainListViewAlignment"] = nameof(MainViewModel.ShowAlignmentPickerCommand),
        ["MainListViewAlignmentN1"] = nameof(MainViewModel.DoAlignmentAn1Command),
        ["MainListViewAlignmentN2"] = nameof(MainViewModel.DoAlignmentAn2Command),
        ["MainListViewAlignmentN3"] = nameof(MainViewModel.DoAlignmentAn3Command),
        ["MainListViewAlignmentN4"] = nameof(MainViewModel.DoAlignmentAn4Command),
        ["MainListViewAlignmentN5"] = nameof(MainViewModel.DoAlignmentAn5Command),
        ["MainListViewAlignmentN6"] = nameof(MainViewModel.DoAlignmentAn6Command),
        ["MainListViewAlignmentN7"] = nameof(MainViewModel.DoAlignmentAn7Command),
        ["MainListViewAlignmentN8"] = nameof(MainViewModel.DoAlignmentAn8Command),
        ["MainListViewAlignmentN9"] = nameof(MainViewModel.DoAlignmentAn9Command),
        ["MainListViewColor1"] = nameof(MainViewModel.SetColor1Command),
        ["MainListViewColor2"] = nameof(MainViewModel.SetColor2Command),
        ["MainListViewColor3"] = nameof(MainViewModel.SetColor3Command),
        ["MainListViewColor4"] = nameof(MainViewModel.SetColor4Command),
        ["MainListViewColor5"] = nameof(MainViewModel.SetColor5Command),
        ["MainListViewColor6"] = nameof(MainViewModel.SetColor6Command),
        ["MainListViewColor7"] = nameof(MainViewModel.SetColor7Command),
        ["MainListViewColor8"] = nameof(MainViewModel.SetColor8Command),
        ["MainListViewColorChoose"] = nameof(MainViewModel.ShowColorPickerCommand),
        ["MainListViewCopyText"] = nameof(MainViewModel.CopyTextToClipboardCommand),
        ["MainListViewCopyOriginalText"] = nameof(MainViewModel.CopyTextFromOriginalToClipboardCommand),
        ["MainListViewCopyTextFromOriginalToCurrent"] = nameof(MainViewModel.CopyTextFromOriginalToTranslationCommand),
        ["MainListViewSortByNumber"] = nameof(MainViewModel.SortByNumberCommand),
        ["MainListViewSortByStartTime"] = nameof(MainViewModel.SortByStartTimeCommand),
        ["MainListViewSortByEndTime"] = nameof(MainViewModel.SortByEndTimeCommand),
        ["MainListViewColumnDeleteText"] = nameof(MainViewModel.ColumnDeleteTextCommand),
        ["MainListViewColumnDeleteTextAndShiftUp"] = nameof(MainViewModel.ColumnDeleteTextAndShiftCellsUpCommand),
        ["MainListViewColumnInsertText"] = nameof(MainViewModel.ColumnInsertEmptyTextAndShiftCellsDownCommand),
        ["MainListViewColumnPaste"] = nameof(MainViewModel.ColumnPasteFromClipboardCommand),
        ["MainListViewColumnTextUp"] = nameof(MainViewModel.ColumnTextUpCommand),
        ["MainListViewColumnTextDown"] = nameof(MainViewModel.ColumnTextDownCommand),
        ["MainListViewListErrors"] = nameof(MainViewModel.ListErrorsCommand),
        ["MainListViewGoToNextError"] = nameof(MainViewModel.GoToNextErrorCommand),
        ["MainListViewAutoDuration"] = nameof(MainViewModel.RecalculateDurationSelectedLinesCommand),
        ["MainListViewSetActor1"] = nameof(MainViewModel.SetActor1Command),
        ["MainListViewSetActor2"] = nameof(MainViewModel.SetActor2Command),
        ["MainListViewSetActor3"] = nameof(MainViewModel.SetActor3Command),
        ["MainListViewSetActor4"] = nameof(MainViewModel.SetActor4Command),
        ["MainListViewSetActor5"] = nameof(MainViewModel.SetActor5Command),
        ["MainListViewSetActor6"] = nameof(MainViewModel.SetActor6Command),
        ["MainListViewSetActor7"] = nameof(MainViewModel.SetActor7Command),
        ["MainListViewSetActor8"] = nameof(MainViewModel.SetActor8Command),
        ["MainListViewSetActor9"] = nameof(MainViewModel.SetActor9Command),
        ["MainListViewSetActor10"] = nameof(MainViewModel.SetActor10Command),
        ["MainListViewSetNewActor"] = nameof(MainViewModel.SetNewActorCommand),
        ["MainListViewSplit"] = nameof(MainViewModel.SplitCommand),
        ["MainListViewToggleDashes"] = nameof(MainViewModel.ToggleDialogDashesCommand),
        ["MainListViewCopyPlainText"] = nameof(MainViewModel.CopyTextToClipboardCommand),
        ["MainListViewCopyOriginalPlainText"] = nameof(MainViewModel.CopyTextFromOriginalToClipboardCommand),
        ["MainRemoveFormatting"] = nameof(MainViewModel.TextBoxRemoveAllFormattingCommand),

        // Text box
        ["MainTextBoxSplitAtCursor"] = nameof(MainViewModel.SplitAtTextBoxCursorPositionCommand),
        ["MainTextBoxSplitAtCursorAndAutoBr"] = nameof(MainViewModel.SplitAtTextBoxCursorPositionCommand),
        ["MainTextBoxSplitAtCursorAndVideoPos"] = nameof(MainViewModel.SplitAtVideoPositionAndTextBoxCursorPositionCommand),
        ["MainTextBoxSplitAtCursorAndVideoPosPlay"] = nameof(MainViewModel.SplitAtVideoPositionAndTextBoxCursorPositionCommand),
        ["MainTextBoxMoveLastWordDown"] = nameof(MainViewModel.MoveLastWordToNextSubtitleCommand),
        ["MainTextBoxMoveLastWordDownCurrent"] = nameof(MainViewModel.MoveLastWordFromFirstLineDownCurrentSubtitleCommand),
        ["MainTextBoxMoveFirstWordUpCurrent"] = nameof(MainViewModel.MoveFirstWordFromNextLineUpCurrentSubtitleCommand),
        ["MainTextBoxMoveFirstWordFromNextUp"] = nameof(MainViewModel.FetchFirstWordFromNextSubtitleCommand),
        ["MainTextBoxSelectionToggleCasing"] = nameof(MainViewModel.ToggleCasingCommand),
        ["MainTextBoxSelectionToLower"] = nameof(MainViewModel.SelectionToLowerCommand),
        ["MainTextBoxSelectionToUpper"] = nameof(MainViewModel.SelectionToUpperCommand),
        ["MainTextBoxAutoBreak"] = nameof(MainViewModel.AutoBreakCommand),
        ["MainTextBoxUnbreak"] = nameof(MainViewModel.UnbreakCommand),
        ["MainTextBoxInsertUnicodeSymbol"] = nameof(MainViewModel.TextBoxInsertUnicodeSymbolCommand),

        // Create / adjust
        ["MainCreateInsertSubAtVideoPos"] = nameof(MainViewModel.WaveformInsertAtPositionAndFocusTextBoxCommand),
        ["MainCreateInsertSubAtVideoPosNoTextBoxFocus"] = nameof(MainViewModel.WaveformInsertAtPositionNoFocusTextBoxCommand),
        ["MainCreateSetStart"] = nameof(MainViewModel.WaveformSetStartCommand),
        ["MainCreateSetEnd"] = nameof(MainViewModel.WaveformSetEndCommand),
        ["MainCreateStartDownEndUp"] = nameof(MainViewModel.InsertSubtitleAtVideoPositionSetEndAtKeyUpCommand),
        ["MainAdjustSetStartAndOffsetTheRest"] = nameof(MainViewModel.WaveformSetStartAndOffsetTheRestCommand),
        ["MainAdjustSetStartAndOffsetTheRest2"] = nameof(MainViewModel.WaveformSetStartAndOffsetTheRestCommand),
        ["MainAdjustSetEndAndOffsetTheRest"] = nameof(MainViewModel.WaveformSetEndAndOffsetTheRestCommand),
        ["MainAdjustSetEndAndGotoNext"] = nameof(MainViewModel.WaveformSetEndAndGoToNextCommand),
        ["MainAdjustSetEndNextStartAndGoToNext"] = nameof(MainViewModel.WaveformSetEndAndStartOfNextAfterGapAndGoToNextCommand),
        ["MainAdjustStartDownEndUpAndGoToNext"] = nameof(MainViewModel.SetSubtitleStartAtVideoPositionSetEndAtKeyUpAndGoToNextCommand),
        ["MainSetEndAndStartNextAfterGap"] = nameof(MainViewModel.WaveformSetEndAndStartOfNextAfterGapCommand),
        ["MainAdjustExtendToNextSubtitle"] = nameof(MainViewModel.ExtendSelectedToNextCommand),
        ["MainAdjustExtendToPreviousSubtitle"] = nameof(MainViewModel.ExtendSelectedToPreviousCommand),
        ["MainAdjustExtendToNextShotChange"] = nameof(MainViewModel.ExtendSelectedLinesToNextShotChangeOrNextSubtitleCommand),
        ["MainAdjustExtendToPreviousShotChange"] = nameof(MainViewModel.ExtendSelectedLinesToPreviousShotChangeCommand),
        ["MainAdjustSelected100MsBack"] = nameof(MainViewModel.Video100MsBackCommand),
        ["MainAdjustSelected100MsForward"] = nameof(MainViewModel.Video100MsForwardCommand),
        ["MainAdjustSnapStartToNextShotChange"] = nameof(MainViewModel.SnapSelectedLinesStartToNextShotChangeCommand),
        ["MainAdjustSnapEndToPreviousShotChange"] = nameof(MainViewModel.SnapSelectedLinesEndToPreviousShotChangeCommand),
        ["MainSetInCueToClosestShotChangeLeftGreenZone"] = nameof(MainViewModel.SetInCueToClosestShotChangeLeftGreenZoneCommand),
        ["MainSetInCueToClosestShotChangeRightGreenZone"] = nameof(MainViewModel.SetInCueToClosestShotChangeRightGreenZoneCommand),
        ["MainSetOutCueToClosestShotChangeLeftGreenZone"] = nameof(MainViewModel.SetOutCueToClosestShotChangeLeftGreenZoneCommand),
        ["MainSetOutCueToClosestShotChangeRightGreenZone"] = nameof(MainViewModel.SetOutCueToClosestShotChangeRightGreenZoneCommand),
        ["MainAutoBalanceSelectedLines"] = nameof(MainViewModel.AutoBreakCommand),
        ["MainMergeDialogWithNext"] = nameof(MainViewModel.MergeWithLineAfterAsDialogCommand),
        ["MainMergeDialogWithPrevious"] = nameof(MainViewModel.MergeWithLineBeforeAsDialogCommand),
        ["MoveStartOneFrameBack"] = nameof(MainViewModel.MoveStartOneFrameBackCommand),
        ["MoveStartOneFrameForward"] = nameof(MainViewModel.MoveStartOneFrameForwardCommand),
        ["MoveEndOneFrameBack"] = nameof(MainViewModel.MoveEndOneFrameBackCommand),
        ["MoveEndOneFrameForward"] = nameof(MainViewModel.MoveEndOneFrameForwardCommand),
        ["MainAdjustMoveStartOneFrameBackKeepGapPrev"] = nameof(MainViewModel.MoveStartOneFrameBackKeepGapPrevCommand),
        ["MainAdjustMoveStartOneFrameForwardKeepGapPrev"] = nameof(MainViewModel.MoveStartOneFrameForwardKeepGapPrevCommand),
        ["MainAdjustMoveEndOneFrameBackKeepGapNext"] = nameof(MainViewModel.MoveEndOneFrameBackKeepGapNextCommand),
        ["MainAdjustMoveEndOneFrameForwardKeepGapNext"] = nameof(MainViewModel.MoveEndOneFrameForwardKeepGapNextCommand),
        ["MainInsertBefore"] = nameof(MainViewModel.InsertLineBeforeCommand),
        ["MainInsertAfter"] = nameof(MainViewModel.InsertLineAfterCommand),
        ["MainWaveformInsertAtCurrentPosition"] = nameof(MainViewModel.WaveformInsertAtPositionAndFocusTextBoxCommand),
        ["MainMergeDialog"] = nameof(MainViewModel.MergeSelectedLinesDialogCommand),
        ["MainToggleFocus"] = nameof(MainViewModel.ToggleFocusGridAndWaveformCommand),
        ["MainToggleFocusWaveform"] = nameof(MainViewModel.ToggleFocusTextBoxAndWaveformCommand),
        ["MainToggleFocusWaveformTextBox"] = nameof(MainViewModel.ToggleFocusTextBoxAndWaveformCommand),

        // Translate
        ["MainTranslateAuto"] = nameof(MainViewModel.ShowAutoTranslateCommand),
        ["MainTranslateAutoSelectedLines"] = nameof(MainViewModel.AutoTranslateSelectedLinesCommand),
        ["MainTranslateGoogleTranslateIt"] = nameof(MainViewModel.ShowTranslateViaCopyPasteCommand),
        ["MainTranslateGoogleIt"] = nameof(MainViewModel.GoogleItCommand),

        // Waveform
        ["WaveformAdd"] = nameof(MainViewModel.WaveformInsertAtPositionAndFocusTextBoxCommand),
        ["WaveformVerticalZoom"] = nameof(MainViewModel.WaveformVerticalZoomInCommand),
        ["WaveformVerticalZoomOut"] = nameof(MainViewModel.WaveformVerticalZoomOutCommand),
        ["WaveformZoomIn"] = nameof(MainViewModel.WaveformHorizontalZoomInCommand),
        ["WaveformZoomOut"] = nameof(MainViewModel.WaveformHorizontalZoomOutCommand),
        ["WaveformAddTextHere"] = nameof(MainViewModel.WaveformInsertAtPositionAndFocusTextBoxCommand),
        ["WaveformAddTextHereFromClipboard"] = nameof(MainViewModel.WaveformPasteFromClipboardCommand),
        ["WaveformGoToPreviousShotChange"] = nameof(MainViewModel.GoToPreviousShotChangeCommand),
        ["WaveformGoToNextShotChange"] = nameof(MainViewModel.GoToNextShotChangeCommand),
        ["WaveformToggleShotChange"] = nameof(MainViewModel.ToggleShotChangesAtVideoPositionCommand),
        ["WaveformAllShotChangesOneFrameForward"] = nameof(MainViewModel.MoveAllShotChangeOneFrameForwardCommand),
        ["WaveformAllShotChangesOneFrameBack"] = nameof(MainViewModel.MoveAllShotChangeOneFrameBackCommand),
        ["WaveformListShotChanges"] = nameof(MainViewModel.ShowShotChangesListCommand),
        ["WaveformSearchSilenceForward"] = nameof(MainViewModel.SeekSilenceForwardCommand),
        ["WaveformSearchSilenceBack"] = nameof(MainViewModel.SeekSilenceBackCommand),
        ["WaveformAudioToTextWhisper"] = nameof(MainViewModel.ShowSpeechToTextWhisperCommand),
        ["WaveformPlaySelection"] = nameof(MainViewModel.PlaySelectedLinesWithoutLoopCommand),
        ["Waveform100MsLeft"] = nameof(MainViewModel.Video100MsBackCommand),
        ["Waveform100MsRight"] = nameof(MainViewModel.Video100MsForwardCommand),
        ["Waveform1000MsLeft"] = nameof(MainViewModel.VideoOneSecondBackCommand),
        ["Waveform1000MsRight"] = nameof(MainViewModel.VideoOneSecondForwardCommand),
        ["WaveformSplit"] = nameof(MainViewModel.SplitAtVideoPositionCommand),

        // General
        ["GeneralGoToFirstSelectedLine"] = nameof(MainViewModel.FocusSelectedLineCommand),
        ["GeneralGoToNextSubtitle"] = nameof(MainViewModel.GoToNextLineCommand),
        ["GeneralGoToPrevSubtitle"] = nameof(MainViewModel.GoToPreviousLineCommand),
        ["GeneralGoToNextBookmark"] = nameof(MainViewModel.GoToNextBookmarkCommand),
        ["GeneralGoToPreviousBookmark"] = nameof(MainViewModel.GoToPreviousBookmarkCommand),
        ["GeneralToggleBookmarksWithText"] = nameof(MainViewModel.AddOrEditBookmarkCommand),
        ["GeneralToggleBookmarks"] = nameof(MainViewModel.ToggleBookmarkSelectedLinesNoTextCommand),
        ["GeneralEditBookmarks"] = nameof(MainViewModel.ListBookmarksCommand),
        ["GeneralRemoveBlankLines"] = nameof(MainViewModel.RemoveBlankLinesCommand),
        ["GeneralChooseProfile"] = nameof(MainViewModel.ShowChooseProfileCommand),
        ["GeneralLayoutChoose"] = nameof(MainViewModel.CommandShowLayoutCommand),
        ["GeneralDuplicateLine"] = nameof(MainViewModel.DuplicateSelectedLinesCommand),
        ["GeneralToggleView"] = nameof(MainViewModel.ShowSourceViewCommand),
        ["GeneralHelp"] = nameof(MainViewModel.ShowHelpCommand),
        ["GeneralFocusTextBox"] = nameof(MainViewModel.FocusTextBoxCommand),
        ["GeneralCycleAudioTrack"] = nameof(MainViewModel.ToggleAudioTracksCommand),
        ["GeneralMergeSelectedLines"] = nameof(MainViewModel.MergeSelectedLinesCommand),
        ["GeneralMergeSelectedLinesBilingual"] = nameof(MainViewModel.MergeSelectedLinesBilingualCommand),
        ["GeneralMergeOriginalAndTranslation"] = nameof(MainViewModel.MergeOriginalIntoTranslationSelectedLinesCommand),
        ["GeneralMergeWithNext"] = nameof(MainViewModel.MergeWithLineAfterCommand),
        ["GeneralMergeWithPrevious"] = nameof(MainViewModel.MergeWithLineBeforeCommand),
        ["GeneralApplyAssaOverrideTags"] = nameof(MainViewModel.ShowAssaApplyCustomOverrideTagsCommand),
        ["GeneralSetAssaPosition"] = nameof(MainViewModel.ShowAssaSetPositionCommand),
        ["GeneralSetAssaResolution"] = nameof(MainViewModel.ShowAssaChangeResolutionCommand),
        ["GeneralSetAssaBgBox"] = nameof(MainViewModel.ShowAssaGenerateBackgroundCommand),
        ["GeneralColorPicker"] = nameof(MainViewModel.ShowColorPickerCommand),
        ["OpenDataFolder"] = nameof(MainViewModel.OpenDataFolderCommand),
        ["OpenContainingFolder"] = nameof(MainViewModel.OpenContainingFolderCommand),
    };

    // WinForms Keys names that need to be retargeted onto Avalonia Key names.
    // Most overlap (A-Z, D0-D9, F1-F12, Insert/Delete/Home/End, Add/Subtract,
    // arrows, Space, Escape, Return, Back). The few WinForms-only or differently
    // named tokens we may encounter are handled here.
    private static readonly Dictionary<string, string> KeyAliasMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Oemplus"] = "OemPlus",
        ["Oemcomma"] = "OemComma",
        ["Oemperiod"] = "OemPeriod",
        ["OemMinus"] = "OemMinus",
        ["OemQuestion"] = "OemQuestion",
        ["OemTilde"] = "OemTilde",
        ["OemOpenBrackets"] = "OemOpenBrackets",
        ["OemCloseBrackets"] = "OemCloseBrackets",
        ["OemPipe"] = "OemPipe",
        ["OemSemicolon"] = "OemSemicolon",
        ["OemQuotes"] = "OemQuotes",
        ["OemBackslash"] = "OemBackslash",
        ["Capital"] = "CapsLock",
        ["Prior"] = "PageUp",
        ["Next"] = "PageDown",
        ["Snapshot"] = "PrintScreen",
        ["Scroll"] = "Scroll",
    };

    public static ImportResult ImportFromFile(string filePath)
    {
        var xml = File.ReadAllText(filePath);
        return ImportFromXml(xml);
    }

    public static ImportResult ImportFromXml(string xml)
    {
        var result = new ImportResult();
        var doc = XDocument.Parse(xml);

        // SE 4 ships two layouts of the same data: the full Settings.xml has
        // <Settings>...<Shortcuts>...</Shortcuts></Settings>, while exported
        // SE_Shortcuts.xml files have <Shortcuts> as the document root. Accept
        // both — fall back to the root itself when no child element matches.
        var shortcutsElement =
            doc.Root?.Name.LocalName == "Shortcuts"
                ? doc.Root
                : doc.Root?.Element("Shortcuts");
        if (shortcutsElement == null)
        {
            return result;
        }

        foreach (var element in shortcutsElement.Elements())
        {
            var se4Name = element.Name.LocalName;
            var value = element.Value;

            if (string.IsNullOrWhiteSpace(value))
            {
                result.SkippedEmpty++;
                continue;
            }

            if (!Map.TryGetValue(se4Name, out var se5Name))
            {
                result.SkippedNoMapping++;
                continue;
            }

            var keys = ParseShortcutValue(value);
            if (keys.Count == 0)
            {
                result.SkippedEmpty++;
                continue;
            }

            result.Shortcuts.Add(new SeShortCut(se5Name, keys));
        }

        return result;
    }

    private static List<string> ParseShortcutValue(string value)
    {
        var tokens = value.Split('+', StringSplitOptions.RemoveEmptyEntries);
        var keys = new List<string>(tokens.Length);
        foreach (var raw in tokens)
        {
            var token = raw.Trim();
            if (token.Length == 0)
            {
                continue;
            }

            keys.Add(NormalizeKey(token));
        }

        // Run the same OEM legacy migration that ShortCut applies, so values
        // like "OemPeriod" / "Oem1" land on the modern physical-key tokens
        // SE 5 matches against at runtime.
        ShortcutManager.MigrateLegacyOemKeys(keys);
        return keys;
    }

    private static string NormalizeKey(string token)
    {
        if (string.Equals(token, "Ctrl", StringComparison.OrdinalIgnoreCase))
        {
            return "Control";
        }

        if (string.Equals(token, "Cmd", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(token, "Command", StringComparison.OrdinalIgnoreCase))
        {
            return "Win";
        }

        if (KeyAliasMap.TryGetValue(token, out var mapped))
        {
            return mapped;
        }

        return token;
    }

    // Best-effort default path to SE 4's Settings.xml on Windows. SE 4 stores
    // its settings in either the portable folder next to the exe or in
    // %APPDATA%\Subtitle Edit. We only return a path that actually exists.
    public static string? FindDefaultSettingsFile()
    {
        if (!OperatingSystem.IsWindows())
        {
            return null;
        }

        // SE 4 stores Settings.xml in %AppData%\Subtitle Edit when installed, and
        // next to the .exe (in Program Files) when run portable. Check both.
        var candidates = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Subtitle Edit", "Settings.xml"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Subtitle Edit", "Settings.xml"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Subtitle Edit", "Settings.xml"),
        };

        return candidates.FirstOrDefault(File.Exists);
    }
}
