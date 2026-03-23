using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Core.Settings
{
    public class Shortcuts
    {
        // General
        public string GeneralMergeSelectedLines { get; set; }
        public string GeneralMergeWithPrevious { get; set; }
        public string GeneralMergeWithNext { get; set; }
        public string GeneralMergeWithPreviousAndUnbreak { get; set; }
        public string GeneralMergeWithNextAndUnbreak { get; set; }
        public string GeneralMergeWithPreviousAndBreak { get; set; }
        public string GeneralMergeWithNextAndBreak { get; set; }
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
        public string GeneralLayoutChoose { get; set; }
        public string GeneralLayoutChoose1 { get; set; }
        public string GeneralLayoutChoose2 { get; set; }
        public string GeneralLayoutChoose3 { get; set; }
        public string GeneralLayoutChoose4 { get; set; }
        public string GeneralLayoutChoose5 { get; set; }
        public string GeneralLayoutChoose6 { get; set; }
        public string GeneralLayoutChoose7 { get; set; }
        public string GeneralLayoutChoose8 { get; set; }
        public string GeneralLayoutChoose9 { get; set; }
        public string GeneralLayoutChoose10 { get; set; }
        public string GeneralLayoutChoose11 { get; set; }
        public string GeneralLayoutChoose12 { get; set; }
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
        public string GeneralGoToPreviousSubtitleAndFocusWaveform { get; set; }
        public string GeneralGoToNextSubtitleAndFocusWaveform { get; set; }
        public string GeneralGoToPrevSubtitleAndPlay { get; set; }
        public string GeneralGoToNextSubtitleAndPlay { get; set; }
        public string GeneralToggleBookmarks { get; set; }
        public string GeneralToggleBookmarksWithText { get; set; }
        public string GeneralEditBookmarks { get; set; }
        public string GeneralClearBookmarks { get; set; }
        public string GeneralGoToBookmark { get; set; }
        public string GeneralGoToPreviousBookmark { get; set; }
        public string GeneralGoToNextBookmark { get; set; }
        public string GeneralChooseProfile { get; set; }
        public string GeneralDuplicateLine { get; set; }
        public string OpenDataFolder { get; set; }
        public string OpenContainingFolder { get; set; }
        public string GeneralToggleView { get; set; }
        public string GeneralToggleMode { get; set; }
        public string GeneralTogglePreviewOnVideo { get; set; }
        public string GeneralRemoveBlankLines { get; set; }
        public string GeneralApplyAssaOverrideTags { get; set; }
        public string GeneralSetAssaPosition { get; set; }
        public string GeneralSetAssaResolution { get; set; }
        public string GeneralSetAssaBgBox { get; set; }
        public string GeneralColorPicker { get; set; }
        public string GeneralTakeAutoBackup { get; set; }
        public string GeneralHelp { get; set; }
        public string GeneralFocusTextBox { get; set; }
        public string GeneralCycleAudioTrack { get; set; }

        // File
        public string MainFileNew { get; set; }
        public string MainFileOpen { get; set; }
        public string MainFileOpenKeepVideo { get; set; }
        public string MainFileSave { get; set; }
        public string MainFileSaveOriginal { get; set; }
        public string MainFileSaveOriginalAs { get; set; }
        public string MainFileSaveAs { get; set; }
        public string MainFileSaveAll { get; set; }
        public string MainFileOpenOriginal { get; set; }
        public string MainFileCloseOriginal { get; set; }
        public string MainFileCloseTranslation { get; set; }
        public string MainFileCompare { get; set; }
        public string MainFileVerifyCompleteness { get; set; }
        public string MainFileImportPlainText { get; set; }
        public string MainFileImportBdSupForEdit { get; set; }
        public string MainFileImportTimeCodes { get; set; }
        public string MainFileExportEbu { get; set; }
        public string MainFileExportPac { get; set; }
        public string MainFileExportBdSup { get; set; }
        public string MainFileExportEdlClip { get; set; }
        public string MainFileExportPlainText { get; set; }
        public string MainFileExportCustomText1 { get; set; }
        public string MainFileExportCustomText2 { get; set; }
        public string MainFileExportCustomText3 { get; set; }
        public string MainFileExit { get; set; }

        // Edit
        public string MainEditUndo { get; set; }
        public string MainEditRedo { get; set; }
        public string MainEditFind { get; set; }
        public string MainEditFindNext { get; set; }
        public string MainEditReplace { get; set; }
        public string MainEditMultipleReplace { get; set; }
        public string MainEditGoToLineNumber { get; set; }
        public string MainEditRightToLeft { get; set; }
        public string MainEditFixRTLViaUnicodeChars { get; set; }
        public string MainEditRemoveRTLUnicodeChars { get; set; }
        public string MainEditReverseStartAndEndingForRTL { get; set; }
        public string MainVideoToggleControls { get; set; }
        public string MainEditToggleTranslationOriginalInPreviews { get; set; }
        public string MainEditInverseSelection { get; set; }
        public string MainEditModifySelection { get; set; }

        // Tools
        public string MainToolsAdjustDuration { get; set; }
        public string MainToolsAdjustDurationLimits { get; set; }
        public string MainToolsFixCommonErrors { get; set; }
        public string MainToolsFixCommonErrorsPreview { get; set; }
        public string MainToolsMergeShortLines { get; set; }
        public string MainToolsMergeDuplicateText { get; set; }
        public string MainToolsMergeSameTimeCodes { get; set; }
        public string MainToolsMakeEmptyFromCurrent { get; set; }
        public string MainToolsSplitLongLines { get; set; }
        public string MainToolsDurationsBridgeGap { get; set; }
        public string MainToolsMinimumDisplayTimeBetweenParagraphs { get; set; }

        public string MainToolsRenumber { get; set; }
        public string MainToolsRemoveTextForHI { get; set; }
        public string MainToolsConvertColorsToDialog { get; set; }
        public string MainToolsChangeCasing { get; set; }
        public string MainToolsAutoDuration { get; set; }
        public string MainToolsBatchConvert { get; set; }
        public string MainToolsMeasurementConverter { get; set; }
        public string MainToolsSplit { get; set; }
        public string MainToolsAppend { get; set; }
        public string MainToolsJoin { get; set; }
        public string MainToolsStyleManager { get; set; }

        // Video
        public string MainVideoOpen { get; set; }
        public string MainVideoClose { get; set; }
        public string MainVideoPause { get; set; }
        public string MainVideoStop { get; set; }
        public string MainVideoPlayFromJustBefore { get; set; }
        public string MainVideoPlayFromBeginning { get; set; }
        public string MainVideoPlayPauseToggle { get; set; }
        public string MainVideoPlay150Speed { get; set; }
        public string MainVideoPlay200Speed { get; set; }
        public string MainVideoFocusSetVideoPosition { get; set; }
        public string MainVideoToggleVideoControls { get; set; }
        public string MainVideo1FrameLeft { get; set; }
        public string MainVideo1FrameRight { get; set; }
        public string MainVideo1FrameLeftWithPlay { get; set; }
        public string MainVideo1FrameRightWithPlay { get; set; }
        public string MainVideo100MsLeft { get; set; }
        public string MainVideo100MsRight { get; set; }
        public string MainVideo500MsLeft { get; set; }
        public string MainVideo500MsRight { get; set; }
        public string MainVideo1000MsLeft { get; set; }
        public string MainVideo1000MsRight { get; set; }
        public string MainVideo5000MsLeft { get; set; }
        public string MainVideo5000MsRight { get; set; }
        public string MainVideoXSMsLeft { get; set; }
        public string MainVideoXSMsRight { get; set; }
        public string MainVideoXLMsLeft { get; set; }
        public string MainVideoXLMsRight { get; set; }
        public string MainVideo3000MsLeft { get; set; }
        public string MainVideo3000MsRight { get; set; }
        public string MainVideoGoToStartCurrent { get; set; }
        public string MainVideoToggleStartEndCurrent { get; set; }
        public string MainVideoPlaySelectedLines { get; set; }
        public string MainVideoLoopSelectedLines { get; set; }
        public string MainVideoGoToPrevSubtitle { get; set; }
        public string MainVideoGoToNextSubtitle { get; set; }
        public string MainVideoGoToPrevTimeCode { get; set; }
        public string MainVideoGoToNextTimeCode { get; set; }
        public string MainVideoGoToPrevChapter { get; set; }
        public string MainVideoGoToNextChapter { get; set; }
        public string MainVideoSelectNextSubtitle { get; set; }
        public string MainVideoFullscreen { get; set; }
        public string MainVideoSlower { get; set; }
        public string MainVideoFaster { get; set; }
        public string MainVideoSpeedToggle { get; set; }
        public string MainVideoReset { get; set; }
        public string MainVideoToggleBrightness { get; set; }
        public string MainVideoToggleContrast { get; set; }
        public string MainVideoAudioToTextVosk { get; set; }
        public string MainVideoAudioToTextWhisper { get; set; }
        public string MainVideoAudioExtractAudioSelectedLines { get; set; }
        public string MainVideoTextToSpeech { get; set; }

        // spell check
        public string MainSpellCheck { get; set; }
        public string MainSpellCheckFindDoubleWords { get; set; }
        public string MainSpellCheckAddWordToNames { get; set; }

        // Sync
        public string MainSynchronizationAdjustTimes { get; set; }
        public string MainSynchronizationVisualSync { get; set; }
        public string MainSynchronizationPointSync { get; set; }
        public string MainSynchronizationPointSyncViaFile { get; set; }
        public string MainSynchronizationChangeFrameRate { get; set; }

        // List view
        public string MainListViewItalic { get; set; }
        public string MainListViewBold { get; set; }
        public string MainListViewUnderline { get; set; }
        public string MainListViewBox { get; set; }
        public string MainListViewToggleQuotes { get; set; }
        public string MainListViewToggleHiTags { get; set; }
        public string MainListViewToggleCustomTags { get; set; }
        public string MainListViewSplit { get; set; }
        public string MainListViewToggleDashes { get; set; }
        public string MainListViewToggleMusicSymbols { get; set; }
        public string MainListViewAlignment { get; set; }
        public string MainListViewAlignmentN1 { get; set; }
        public string MainListViewAlignmentN2 { get; set; }
        public string MainListViewAlignmentN3 { get; set; }
        public string MainListViewAlignmentN4 { get; set; }
        public string MainListViewAlignmentN5 { get; set; }
        public string MainListViewAlignmentN6 { get; set; }
        public string MainListViewAlignmentN7 { get; set; }
        public string MainListViewAlignmentN8 { get; set; }
        public string MainListViewAlignmentN9 { get; set; }
        public string MainListViewColor1 { get; set; }
        public string MainListViewColor2 { get; set; }
        public string MainListViewColor3 { get; set; }
        public string MainListViewColor4 { get; set; }
        public string MainListViewColor5 { get; set; }
        public string MainListViewColor6 { get; set; }
        public string MainListViewColor7 { get; set; }
        public string MainListViewColor8 { get; set; }
        public string MainListViewSetNewActor { get; set; }
        public string MainListViewSetActor1 { get; set; }
        public string MainListViewSetActor2 { get; set; }
        public string MainListViewSetActor3 { get; set; }
        public string MainListViewSetActor4 { get; set; }
        public string MainListViewSetActor5 { get; set; }
        public string MainListViewSetActor6 { get; set; }
        public string MainListViewSetActor7 { get; set; }
        public string MainListViewSetActor8 { get; set; }
        public string MainListViewSetActor9 { get; set; }
        public string MainListViewSetActor10 { get; set; }
        public string MainListViewColorChoose { get; set; }
        public string MainRemoveFormatting { get; set; }
        public string MainListViewCopyText { get; set; }
        public string MainListViewCopyPlainText { get; set; }
        public string MainListViewCopyTextFromOriginalToCurrent { get; set; }
        public string MainListViewAutoDuration { get; set; }
        public string MainListViewColumnDeleteText { get; set; }
        public string MainListViewColumnDeleteTextAndShiftUp { get; set; }
        public string MainListViewColumnInsertText { get; set; }
        public string MainListViewColumnPaste { get; set; }
        public string MainListViewColumnTextUp { get; set; }
        public string MainListViewColumnTextDown { get; set; }
        public string MainListViewGoToNextError { get; set; }
        public string MainListViewListErrors { get; set; }
        public string MainListViewSortByNumber { get; set; }
        public string MainListViewSortByStartTime { get; set; }
        public string MainListViewSortByEndTime { get; set; }
        public string MainListViewSortByDuration { get; set; }
        public string MainListViewSortByGap { get; set; }
        public string MainListViewSortByText { get; set; }
        public string MainListViewSortBySingleLineMaxLen { get; set; }
        public string MainListViewSortByTextTotalLength { get; set; }
        public string MainListViewSortByCps { get; set; }
        public string MainListViewSortByWpm { get; set; }
        public string MainListViewSortByNumberOfLines { get; set; }
        public string MainListViewSortByActor { get; set; }
        public string MainListViewSortByStyle { get; set; }
        public string MainListViewRemoveTimeCodes { get; set; }
        public string MainTextBoxSplitAtCursor { get; set; }
        public string MainTextBoxSplitAtCursorAndAutoBr { get; set; }
        public string MainTextBoxSplitAtCursorAndVideoPos { get; set; }
        public string MainTextBoxSplitAtCursorAndVideoPosPlay { get; set; }
        public string MainTextBoxSplitSelectedLineBilingual { get; set; }
        public string MainTextBoxMoveLastWordDown { get; set; }
        public string MainTextBoxMoveFirstWordFromNextUp { get; set; }
        public string MainTextBoxMoveLastWordDownCurrent { get; set; }
        public string MainTextBoxMoveFirstWordUpCurrent { get; set; }
        public string MainTextBoxMoveFromCursorToNextAndGoToNext { get; set; }
        public string MainTextBoxMoveFirstWordToPrev { get; set; }
        public string MainTextBoxSelectionToLower { get; set; }
        public string MainTextBoxSelectionToUpper { get; set; }
        public string MainTextBoxSelectionToggleCasing { get; set; }
        public string MainTextBoxSelectionToRuby { get; set; }
        public string MainTextBoxToggleAutoDuration { get; set; }
        public string MainCreateInsertSubAtVideoPos { get; set; }
        public string MainCreateInsertSubAtVideoPosMax { get; set; }
        public string MainCreateInsertSubAtVideoPosNoTextBoxFocus { get; set; }
        public string MainCreateSetStart { get; set; }
        public string MainCreateSetEnd { get; set; }
        public string MainAdjustVideoSetStartForAppropriateLine { get; set; }
        public string MainAdjustVideoSetEndForAppropriateLine { get; set; }
        public string MainAdjustSetEndAndPause { get; set; }
        public string MainCreateSetEndAddNewAndGoToNew { get; set; }
        public string MainCreateStartDownEndUp { get; set; }
        public string MainAdjustSetStartAndOffsetTheRest { get; set; }
        public string MainAdjustSetStartAndOffsetTheRest2 { get; set; }
        public string MainAdjustSetStartAndOffsetTheWholeSubtitle { get; set; }
        public string MainAdjustSetEndAndOffsetTheRest { get; set; }
        public string MainAdjustSetEndAndOffsetTheRestAndGoToNext { get; set; }
        public string MainAdjustSetStartAndGotoNext { get; set; }
        public string MainAdjustSetEndAndGotoNext { get; set; }
        public string MainAdjustViaEndAutoStart { get; set; }
        public string MainAdjustViaEndAutoStartAndGoToNext { get; set; }
        public string MainAdjustSetEndMinusGapAndStartNextHere { get; set; }
        public string MainSetEndAndStartNextAfterGap { get; set; }
        public string MainAdjustSetStartAutoDurationAndGoToNext { get; set; }
        public string MainAdjustSetEndNextStartAndGoToNext { get; set; }
        public string MainAdjustStartDownEndUpAndGoToNext { get; set; }
        public string MainAdjustSetStartAndEndOfPrevious { get; set; }
        public string MainAdjustSetStartAndEndOfPreviousAndGoToNext { get; set; }
        public string MainAdjustSetStartKeepDuration { get; set; }
        public string MainAdjustSelected100MsForward { get; set; }
        public string MainAdjustSelected100MsBack { get; set; }
        public string MainAdjustStartXMsBack { get; set; }
        public string MainAdjustStartXMsForward { get; set; }
        public string MainAdjustEndXMsBack { get; set; }
        public string MainAdjustEndXMsForward { get; set; }
        public string MoveStartOneFrameBack { get; set; }
        public string MoveStartOneFrameForward { get; set; }
        public string MoveEndOneFrameBack { get; set; }
        public string MoveEndOneFrameForward { get; set; }
        public string MoveStartOneFrameBackKeepGapPrev { get; set; }
        public string MoveStartOneFrameForwardKeepGapPrev { get; set; }
        public string MoveEndOneFrameBackKeepGapNext { get; set; }
        public string MoveEndOneFrameForwardKeepGapNext { get; set; }
        public string MainAdjustSnapStartToNextShotChange { get; set; }
        public string MainAdjustSnapEndToPreviousShotChange { get; set; }
        public string MainAdjustExtendToNextShotChange { get; set; }
        public string MainAdjustExtendToPreviousShotChange { get; set; }
        public string MainAdjustExtendToNextSubtitle { get; set; }
        public string MainAdjustExtendToPreviousSubtitle { get; set; }
        public string MainAdjustExtendToNextSubtitleMinusChainingGap { get; set; }
        public string MainAdjustExtendToPreviousSubtitleMinusChainingGap { get; set; }
        public string MainAdjustExtendCurrentSubtitle { get; set; }
        public string MainAdjustExtendPreviousLineEndToCurrentStart { get; set; }
        public string MainAdjustExtendNextLineStartToCurrentEnd { get; set; }
        public string MainSetInCueToClosestShotChangeLeftGreenZone { get; set; }
        public string MainSetInCueToClosestShotChangeRightGreenZone { get; set; }
        public string MainSetOutCueToClosestShotChangeLeftGreenZone { get; set; }
        public string MainSetOutCueToClosestShotChangeRightGreenZone { get; set; }
        public string GeneralAutoCalcCurrentDuration { get; set; }
        public string GeneralAutoCalcCurrentDurationByOptimalReadingSpeed { get; set; }
        public string GeneralAutoCalcCurrentDurationByMinReadingSpeed { get; set; }
        public string MainInsertAfter { get; set; }
        public string MainTextBoxAutoBreak { get; set; }
        public string MainTextBoxBreakAtPosition { get; set; }
        public string MainTextBoxBreakAtPositionAndGoToNext { get; set; }
        public string MainTextBoxRecord { get; set; }
        public string MainTextBoxUnbreak { get; set; }
        public string MainTextBoxUnbreakNoSpace { get; set; }
        public string MainTextBoxAssaIntellisense { get; set; }
        public string MainTextBoxAssaRemoveTag { get; set; }
        public string MainTextBoxInsertUnicodeSymbol { get; set; }
        public string MainWaveformInsertAtCurrentPosition { get; set; }
        public string MainInsertBefore { get; set; }
        public string MainMergeDialog { get; set; }
        public string MainMergeDialogWithNext { get; set; }
        public string MainMergeDialogWithPrevious { get; set; }
        public string MainAutoBalanceSelectedLines { get; set; }
        public string MainEvenlyDistributeSelectedLines { get; set; }
        public string MainToggleFocus { get; set; }
        public string MainToggleFocusWaveform { get; set; }
        public string MainToggleFocusWaveformTextBox { get; set; }
        public string WaveformAdd { get; set; }
        public string WaveformVerticalZoom { get; set; }
        public string WaveformVerticalZoomOut { get; set; }
        public string WaveformZoomIn { get; set; }
        public string WaveformZoomOut { get; set; }
        public string WaveformSplit { get; set; }
        public string WaveformPlaySelection { get; set; }
        public string WaveformPlaySelectionEnd { get; set; }
        public string WaveformSearchSilenceForward { get; set; }
        public string WaveformSearchSilenceBack { get; set; }
        public string WaveformAddTextHere { get; set; }
        public string WaveformAddTextHereFromClipboard { get; set; }
        public string WaveformSetParagraphAsSelection { get; set; }
        public string WaveformGoToPreviousShotChange { get; set; }
        public string WaveformGoToNextShotChange { get; set; }
        public string WaveformToggleShotChange { get; set; }
        public string WaveformAllShotChangesOneFrameForward { get; set; }
        public string WaveformAllShotChangesOneFrameBack { get; set; }
        public string WaveformListShotChanges { get; set; }
        public string WaveformGuessStart { get; set; }
        public string Waveform100MsLeft { get; set; }
        public string Waveform100MsRight { get; set; }
        public string Waveform1000MsLeft { get; set; }
        public string Waveform1000MsRight { get; set; }
        public string WaveformAudioToTextVosk { get; set; }
        public string WaveformAudioToTextWhisper { get; set; }
        public string WaveformToggleSpectrogramStyle { get; set; }
        public string MainCheckFixTimingViaShotChanges { get; set; }
        public string MainTranslateGoogleIt { get; set; }
        public string MainTranslateGoogleTranslateIt { get; set; }
        public string MainTranslateAuto { get; set; }
        public string MainTranslateAutoSelectedLines { get; set; }
        public string MainTranslateCustomSearch1 { get; set; }
        public string MainTranslateCustomSearch2 { get; set; }
        public string MainTranslateCustomSearch3 { get; set; }
        public string MainTranslateCustomSearch4 { get; set; }
        public string MainTranslateCustomSearch5 { get; set; }
        public List<PluginShortcut> PluginShortcuts { get; set; }


        public Shortcuts()
        {
            GeneralGoToFirstSelectedLine = "Control+L";
            GeneralMergeSelectedLines = "Control+Shift+M";
            GeneralToggleTranslationMode = "Control+Shift+O";
            GeneralMergeOriginalAndTranslation = "Control+Alt+Shift+M";
            GeneralGoToNextSubtitle = "Shift+Return";
            GeneralGoToNextSubtitlePlayTranslate = "Alt+Down";
            GeneralGoToPrevSubtitlePlayTranslate = "Alt+Up";
            GeneralToggleBookmarksWithText = "Control+Shift+B";
            OpenDataFolder = "Control+Alt+Shift+D";
            GeneralToggleView = "F2";
            GeneralHelp = "F1";
            MainFileNew = "Control+N";
            MainFileOpen = "Control+O";
            MainFileSave = "Control+S";
            MainFileSaveAs = "Control+Shift+S";
            MainEditUndo = "Control+Z";
            MainEditRedo = "Control+Y";
            MainEditFind = "Control+F";
            MainEditFindNext = "F3";
            MainEditReplace = "Control+H";
            MainEditMultipleReplace = "Control+Alt+M";
            MainEditGoToLineNumber = "Control+G";
            MainEditRightToLeft = "Control+Shift+Alt+R";
            MainEditInverseSelection = "Control+Shift+I";
            MainToolsFixCommonErrors = "Control+Shift+F";
            MainToolsFixCommonErrorsPreview = "Control+P";
            MainToolsRenumber = "Control+Shift+N";
            MainToolsRemoveTextForHI = "Control+Shift+H";
            MainToolsChangeCasing = "Control+Shift+C";
            MainVideoPlayFromJustBefore = "Shift+F10";
            MainVideoPlayPauseToggle = "Control+P";
            MainVideoPause = "Control+Alt+P";
            MainVideo500MsLeft = "Alt+Left";
            MainVideo500MsRight = "Alt+Right";
            MainVideoFullscreen = "Alt+Return";
            MainVideoReset = "Control+D0";
            MainSpellCheck = "Alt+F7";
            MainSpellCheckFindDoubleWords = "Control+Shift+D";
            MainSpellCheckAddWordToNames = "Control+Shift+L";
            MainSynchronizationAdjustTimes = "Control+Shift+A";
            MainSynchronizationVisualSync = "Control+Shift+V";
            MainSynchronizationPointSync = "Control+Shift+P";
            MainListViewItalic = "Control+I";
            MainTextBoxSplitAtCursor = "Control+Alt+V";
            MainTextBoxSelectionToLower = "Control+U";
            MainTextBoxSelectionToUpper = "Control+Shift+U";
            MainTextBoxSelectionToggleCasing = "Control+Shift+F3";
            MainCreateInsertSubAtVideoPos = "Shift+F9";
            MainVideoGoToStartCurrent = "Shift+F11";
            MainVideoToggleStartEndCurrent = "F4";
            MainVideoPlaySelectedLines = "F5";
            MainVideoGoToStartCurrent = "F6";
            MainVideo3000MsLeft = "F7";
            MainListViewGoToNextError = "F8";
            MainListViewListErrors = "Control+F8";
            MainCreateSetStart = "F11";
            MainCreateSetEnd = "F12";
            MainAdjustSetStartAndOffsetTheRest = "Control+Space";
            MainAdjustSetStartAndOffsetTheRest2 = "F9";
            MainAdjustSetEndAndGotoNext = "F10";
            MainInsertAfter = "Alt+Insert";
            MainWaveformInsertAtCurrentPosition = "Insert";
            MainInsertBefore = "Control+Shift+Insert";
            MainTextBoxAutoBreak = "Control+R";
            MainTranslateAuto = "Control+Shift+G";
            MainAdjustExtendToNextSubtitle = "Control+Shift+E";
            MainAdjustExtendToPreviousSubtitle = "Alt+Shift+E";
            MainAdjustExtendToNextSubtitleMinusChainingGap = "Control+Shift+W";
            MainAdjustExtendToPreviousSubtitleMinusChainingGap = "Alt+Shift+W";
            WaveformVerticalZoom = "Shift+Add";
            WaveformVerticalZoomOut = "Shift+Subtract";
            WaveformAddTextHere = "Return";
            Waveform100MsLeft = "Shift+Left";
            Waveform100MsRight = "Shift+Right";
            Waveform1000MsLeft = "Left";
            Waveform1000MsRight = "Right";
            MainCheckFixTimingViaShotChanges = "Control+Shift+D9";
            PluginShortcuts = new List<PluginShortcut>();
        }

        public Shortcuts Clone()
        {
            var xws = new XmlWriterSettings { Indent = true };
            var sb = new StringBuilder();
            using (var textWriter = XmlWriter.Create(sb, xws))
            {
                textWriter.WriteStartDocument();
                textWriter.WriteStartElement("Settings", string.Empty);
                Settings.WriteShortcuts(this, textWriter);
                textWriter.WriteEndElement();
                textWriter.WriteEndDocument();
            }

            var doc = new XmlDocument { PreserveWhitespace = true };
            doc.LoadXml(sb.ToString().Replace("encoding=\"utf-16\"", "encoding=\"utf-8\""));
            var shortcuts = new Shortcuts();
            Settings.ReadShortcuts(doc, shortcuts);
            return shortcuts;
        }

        public static void Save(string fileName, Shortcuts shortcuts)
        {
            var xws = new XmlWriterSettings { Indent = true, Encoding = Encoding.UTF8 };
            var sb = new StringBuilder();
            using (var textWriter = XmlWriter.Create(sb, xws))
            {
                textWriter.WriteStartDocument();
                textWriter.WriteStartElement("Settings", string.Empty);
                Settings.WriteShortcuts(shortcuts, textWriter);
                textWriter.WriteEndElement();
                textWriter.WriteEndDocument();
            }
            File.WriteAllText(fileName, sb.ToString().Replace("encoding=\"utf-16\"", "encoding=\"utf-8\""), Encoding.UTF8);
        }

        public static Shortcuts Load(string fileName)
        {
            var doc = new XmlDocument { PreserveWhitespace = true };
            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                doc.Load(stream);
                var shortcuts = new Shortcuts();
                Settings.ReadShortcuts(doc, shortcuts);
                return shortcuts;
            }
        }
    }
}