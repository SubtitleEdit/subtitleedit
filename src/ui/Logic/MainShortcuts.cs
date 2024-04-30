using Nikse.SubtitleEdit.Core.Common;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Logic
{
    public class MainShortcuts
    {
        public Keys MainGeneralGoToFirstSelectedLine { get; set; }
        public Keys MainGeneralGoToFirstEmptyLine { get; set; }
        public Keys MainGeneralMergeSelectedLines { get; set; }
        public Keys MainGeneralMergeSelectedLinesAndAutoBreak { get; set; }
        public Keys MainGeneralMergeSelectedLinesAndUnbreak { get; set; }
        public Keys MainGeneralMergeSelectedLinesAndUnbreakNoSpace { get; set; }
        public Keys MainGeneralMergeSelectedLinesBilingual { get; set; }
        public Keys MainGeneralMergeWithPreviousBilingual { get; set; }
        public Keys MainGeneralMergeWithNextBilingual { get; set; }
        public Keys MainGeneralMergeSelectedLinesOnlyFirstText { get; set; }
        public Keys MainGeneralToggleTranslationMode { get; set; }
        public Keys MainGeneralSwitchTranslationAndOriginal { get; set; }
        public Keys MainGeneralSwitchTranslationAndOriginalTextBoxes { get; set; }
        public Keys MainGeneralLayoutChoose { get; set; }
        public Keys MainGeneralLayoutChoose1 { get; set; }
        public Keys MainGeneralLayoutChoose2 { get; set; }
        public Keys MainGeneralLayoutChoose3 { get; set; }
        public Keys MainGeneralLayoutChoose4 { get; set; }
        public Keys MainGeneralLayoutChoose5 { get; set; }
        public Keys MainGeneralLayoutChoose6 { get; set; }
        public Keys MainGeneralLayoutChoose7 { get; set; }
        public Keys MainGeneralLayoutChoose8 { get; set; }
        public Keys MainGeneralLayoutChoose9 { get; set; }
        public Keys MainGeneralLayoutChoose10 { get; set; }
        public Keys MainGeneralLayoutChoose11 { get; set; }
        public Keys MainGeneralLayoutChoose12 { get; set; }
        public Keys MainGeneralMergeTranslationAndOriginal { get; set; }
        public Keys MainGeneralMergeWithNext { get; set; }
        public Keys MainGeneralMergeWithPrevious { get; set; }
        public Keys MainGeneralMergeWithNextAndUnbreak { get; set; }
        public Keys MainGeneralMergeWithPreviousAndUnbreak { get; set; }
        public Keys MainGeneralMergeWithNextAndBreak { get; set; }
        public Keys MainGeneralMergeWithPreviousAndBreak { get; set; }
        public Keys MainGeneralGoToNextSubtitle { get; set; }
        public Keys MainGeneralGoToNextSubtitlePlayTranslate { get; set; }
        public Keys MainGeneralGoToNextSubtitleCursorAtEnd { get; set; }
        public Keys MainGeneralGoToPrevSubtitle { get; set; }
        public Keys MainGeneralGoToPrevSubtitlePlayTranslate { get; set; }
        public Keys MainGeneralGoToStartOfCurrentSubtitle { get; set; }
        public Keys MainGeneralGoToEndOfCurrentSubtitle { get; set; }
        public Keys MainGeneralFileSaveAll { get; set; }
        public Keys MainGeneralSetAssaResolution { get; set; }
        public Keys MainGeneralTakeAutoBackupNow { get; set; }
        public Keys MainToolsAutoDuration { get; set; }
        public Keys MainVideoFocusSetVideoPosition { get; set; }
        public Keys ToggleVideoDockUndock { get; set; }
        public Keys VideoPause { get; set; }
        public Keys VideoStop { get; set; }
        public Keys VideoPlayPauseToggle { get; set; }
        public Keys VideoPlay150Speed { get; set; }
        public Keys VideoPlay200Speed { get; set; }
        public Keys MainVideoPlayFromJustBefore { get; set; }
        public Keys MainVideoPlayFromBeginning { get; set; }
        public Keys Video1FrameLeft { get; set; }
        public Keys Video1FrameRight { get; set; }
        public Keys Video1FrameLeftWithPlay { get; set; }
        public Keys Video1FrameRightWithPlay { get; set; }
        public Keys Video100MsLeft { get; set; }
        public Keys Video100MsRight { get; set; }
        public Keys Video500MsLeft { get; set; }
        public Keys Video500MsRight { get; set; }
        public Keys Video1000MsLeft { get; set; }
        public Keys Video1000MsRight { get; set; }
        public Keys MainVideo3000MsLeft { get; set; }
        public Keys MainVideo3000MsRight { get; set; }
        public Keys Video5000MsLeft { get; set; }
        public Keys Video5000MsRight { get; set; }
        public Keys VideoXSMsLeft { get; set; }
        public Keys VideoXSMsRight { get; set; }
        public Keys VideoXLMsLeft { get; set; }
        public Keys VideoXLMsRight { get; set; }
        public Keys MainVideoGoToStartCurrent { get; set; }
        public Keys MainVideoToggleStartEndCurrent { get; set; }
        public Keys MainVideoPlaySelectedLines { get; set; }
        public Keys MainVideoLoopSelectedLines { get; set; }
        public Keys VideoGoToPrevSubtitle { get; set; }
        public Keys VideoGoToNextSubtitle { get; set; }
        public Keys VideoGoToPrevTimeCode { get; set; }
        public Keys VideoGoToNextTimeCode { get; set; }
        public Keys VideoGoToPrevChapter { get; set; }
        public Keys VideoGoToNextChapter { get; set; }
        public Keys VideoSelectNextSubtitle { get; set; }
        public Keys VideoPlayFirstSelected { get; set; }
        public Keys MainVideoFullscreen { get; set; }
        public Keys MainVideoSlower { get; set; }
        public Keys MainVideoFaster { get; set; }
        public Keys MainVideoSpeedToggle { get; set; }
        public Keys MainVideoReset { get; set; }
        public Keys MainVideoToggleBrightness { get; set; }
        public Keys MainVideoAudioToTextVosk { get; set; }
        public Keys MainVideoAudioToTextWhisper { get; set; }
        public Keys MainVideoAudioExtractSelectedLines { get; set; }
        public Keys MainVideoTextToSpeech { get; set; }
        public Keys MainVideoToggleContrast { get; set; }
        public Keys MainGoToPreviousSubtitleAndFocusVideo { get; set; }
        public Keys MainGoToNextSubtitleAndFocusVideo { get; set; }
        public Keys MainGoToPreviousSubtitleAndFocusWaveform { get; set; }
        public Keys MainGoToNextSubtitleAndFocusWaveform { get; set; }
        public Keys MainGoToPrevSubtitleAndPlay { get; set; }
        public Keys MainGoToNextSubtitleAndPlay { get; set; }
        public Keys MainAutoCalcCurrentDuration { get; set; }
        public Keys MainAutoCalcCurrentDurationByOptimalReadingSpeed { get; set; }
        public Keys MainAutoCalcCurrentDurationByMinReadingSpeed { get; set; }
        public Keys MainGeneralToggleBookmarks { get; set; }
        public Keys MainGeneralFocusTextBox { get; set; }
        public Keys MainGeneralToggleBookmarksAddComment { get; set; }
        public Keys MainGeneralEditBookmark { get; set; }
        public Keys MainGeneralClearBookmarks { get; set; }
        public Keys MainGeneralGoToBookmark { get; set; }
        public Keys MainGeneralGoToPreviousBookmark { get; set; }
        public Keys MainGeneralGoToNextBookmark { get; set; }
        public Keys MainGeneralChooseProfile { get; set; }
        public Keys MainGeneralOpenDataFolder { get; set; }
        public Keys MainGeneralDuplicateLine { get; set; }
        public Keys MainGeneralToggleView { get; set; }
        public Keys MainGeneralToggleMode { get; set; }
        public Keys MainGeneralTogglePreviewOnVideo { get; set; }
        public Keys MainTextBoxSplitAtCursor { get; set; }
        public Keys MainTextBoxSplitAtCursorAndAutoBr { get; set; }
        public Keys MainTextBoxSplitAtCursorAndVideoPos { get; set; }
        public Keys MainTextBoxSplitSelectedLineBilingual { get; set; }
        public Keys MainTextBoxMoveLastWordDown { get; set; }
        public Keys MainTextBoxMoveFirstWordFromNextUp { get; set; }
        public Keys MainTextBoxMoveLastWordDownCurrent { get; set; }
        public Keys MainTextBoxMoveFromCursorToNextAndGoToNext { get; set; }
        public Keys MainTextBoxMoveFirstWordUpCurrent { get; set; }
        public Keys MainTextBoxSelectionToLower { get; set; }
        public Keys MainTextBoxSelectionToUpper { get; set; }
        public Keys MainTextBoxSelectionToggleCasing { get; set; }
        public Keys MainTextBoxToggleAutoDuration { get; set; }
        public Keys MainCreateInsertSubAtVideoPos { get; set; }
        public Keys MainCreateInsertSubAtVideoPosNoTextBoxFocus { get; set; }
        public Keys MainCreateInsertSubAtVideoPosMax { get; set; }
        public Keys MainCreateSetStart { get; set; }
        public Keys MainCreateSetEnd { get; set; }
        public Keys MainAdjustVideoSetStartForAppropriateLine { get; set; }
        public Keys MainAdjustVideoSetEndForAppropriateLine { get; set; }
        public Keys MainAdjustSetEndAndPause { get; set; }
        public Keys MainCreateStartDownEndUp { get; set; }
        public Keys MainCreateSetEndAddNewAndGoToNew { get; set; }
        public Keys MainAdjustSetStartAndOffsetTheRest { get; set; }
        public Keys MainAdjustSetStartAndOffsetTheRest2 { get; set; }
        public Keys MainAdjustSetStartAndOffsetTheWholeSubtitle { get; set; }
        public Keys MainAdjustSetEndAndOffsetTheRest { get; set; }
        public Keys MainAdjustSetEndAndOffsetTheRestAndGoToNext { get; set; }
        public Keys MainAdjustSetStartAndGotoNext { get; set; }
        public Keys MainAdjustSetEndAndGotoNext { get; set; }
        public Keys MainAdjustInsertViaEndAutoStart { get; set; }
        public Keys MainAdjustInsertViaEndAutoStartAndGoToNext { get; set; }
        public Keys MainAdjustSetEndMinusGapAndStartNextHere { get; set; }
        public Keys MainAdjustSetEndAndStartOfNextPlusGap { get; set; }
        public Keys MainAdjustSetStartAutoDurationAndGoToNext { get; set; }
        public Keys MainAdjustSetEndNextStartAndGoToNext { get; set; }
        public Keys MainAdjustStartDownEndUpAndGoToNext { get; set; }
        public Keys MainAdjustSetStartAndEndOfPrevious { get; set; }
        public Keys MainAdjustSetStartAndEndOfPreviousAndGoToNext { get; set; }
        public Keys MainAdjustSetStartKeepDuration { get; set; }
        public Keys MainAdjustSelected100MsForward { get; set; }
        public Keys MainAdjustSelected100MsBack { get; set; }
        public Keys MainAdjustAdjustStartXMsBack { get; set; }
        public Keys MainAdjustAdjustStartXMsForward { get; set; }
        public Keys MainAdjustAdjustEndXMsBack { get; set; }
        public Keys MainAdjustAdjustEndXMsForward { get; set; }
        public Keys MainAdjustMoveStartOneFrameBack { get; set; }
        public Keys MainAdjustMoveStartOneFrameForward { get; set; }
        public Keys MainAdjustMoveEndOneFrameBack { get; set; }
        public Keys MainAdjustMoveEndOneFrameForward { get; set; }
        public Keys MainAdjustMoveStartOneFrameBackKeepGapPrev { get; set; }
        public Keys MainAdjustMoveStartOneFrameForwardKeepGapPrev { get; set; }
        public Keys MainAdjustMoveEndOneFrameBackKeepGapNext { get; set; }
        public Keys MainAdjustMoveEndOneFrameForwardKeepGapNext { get; set; }
        public Keys MainAdjustSnapStartToNextShotChange { get; set; }
        public Keys MainAdjustSnapEndToPreviousShotChange { get; set; }
        public Keys MainAdjustExtendToNextShotChange { get; set; }
        public Keys MainAdjustExtendToPreviousShotChange { get; set; }
        public Keys MainAdjustExtendToNextSubtitle { get; set; }
        public Keys MainAdjustExtendToPreviousSubtitle { get; set; }
        public Keys MainAdjustExtendToNextSubtitleMinusChainingGap { get; set; }
        public Keys MainAdjustExtendToPreviousSubtitleMinusChainingGap { get; set; }
        public Keys MainAdjustExtendCurrentSubtitle { get; set; }
        public Keys MainAdjustExtendPreviousLineEndToCurrentStart { get; set; }
        public Keys MainAdjustExtendNextLineStartToCurrentEnd { get; set; }
        public Keys MainSetInCueToClosestShotChangeLeftGreenZone { get; set; }
        public Keys MainSetInCueToClosestShotChangeRightGreenZone { get; set; }
        public Keys MainSetOutCueToClosestShotChangeLeftGreenZone { get; set; }
        public Keys MainSetOutCueToClosestShotChangeRightGreenZone { get; set; }
        public Keys MainInsertAfter { get; set; }
        public Keys MainInsertBefore { get; set; }
        public Keys MainTextBoxAutoBreak { get; set; }
        public Keys MainAutoTranslateSelectedLines { get; set; }
        public Keys MainTextBoxRecord { get; set; }
        public Keys MainTextBoxUnbreak { get; set; }
        public Keys MainTextBoxUnbreakNoSpace { get; set; }
        public Keys MainTextBoxAssaIntellisense { get; set; }
        public Keys MainTextBoxAssaRemoveTag { get; set; }
        public Keys MainTextBoxBreakAtCursorPosition { get; set; }
        public Keys MainTextBoxBreakAtCursorPositionAndGoToNext { get; set; }
        public Keys MainMergeDialog { get; set; }
        public Keys MainMergeDialogWithNext { get; set; }
        public Keys MainMergeDialogWithPrevious { get; set; }
        public Keys MainToggleFocus { get; set; }
        public Keys MainToggleFocusWaveform { get; set; }
        public Keys MainToggleFocusWaveformTextBox { get; set; }
        public Keys MainWaveformAdd { get; set; }
        public Keys MainListViewToggleDashes { get; set; }
        public Keys MainListViewToggleQuotes { get; set; }
        public Keys MainListViewToggleHiTags { get; set; }
        public Keys MainListViewToggleCustomTags { get; set; }
        public Keys MainListViewToggleMusicSymbols { get; set; }
        public Keys MainListViewAutoDuration { get; set; }
        public Keys MainListViewAlignmentN1 { get; set; }
        public Keys MainListViewAlignmentN2 { get; set; }
        public Keys MainListViewAlignmentN3 { get; set; }
        public Keys MainListViewAlignmentN4 { get; set; }
        public Keys MainListViewAlignmentN5 { get; set; }
        public Keys MainListViewAlignmentN6 { get; set; }
        public Keys MainListViewAlignmentN7 { get; set; }
        public Keys MainListViewAlignmentN8 { get; set; }
        public Keys MainListViewAlignmentN9 { get; set; }
        public Keys MainListViewColor1 { get; set; }
        public Keys MainListViewColor2 { get; set; }
        public Keys MainListViewColor3 { get; set; }
        public Keys MainListViewColor4 { get; set; }
        public Keys MainListViewColor5 { get; set; }
        public Keys MainListViewColor6 { get; set; }
        public Keys MainListViewColor7 { get; set; }
        public Keys MainListViewColor8 { get; set; }
        public Keys MainListViewSetNewActor { get; set; }
        public Keys MainListViewSetActor1 { get; set; }
        public Keys MainListViewSetActor2 { get; set; }
        public Keys MainListViewSetActor3 { get; set; }
        public Keys MainListViewSetActor4 { get; set; }
        public Keys MainListViewSetActor5 { get; set; }
        public Keys MainListViewSetActor6 { get; set; }
        public Keys MainListViewSetActor7 { get; set; }
        public Keys MainListViewSetActor8 { get; set; }
        public Keys MainListViewSetActor9 { get; set; }
        public Keys MainListViewSetActor10 { get; set; }
        public Keys MainListViewGoToNextError { get; set; }
        public Keys MainListViewRemoveBlankLines { get; set; }
        public Keys MainListViewRemoveTimeCodes { get; set; }
        public Keys MainListViewCopyText { get; set; }
        public Keys MainListViewCopyPlainText { get; set; }
        public Keys MainEditFixRTLViaUnicodeChars { get; set; }
        public Keys MainEditRemoveRTLUnicodeChars { get; set; }
        public Keys MainEditReverseStartAndEndingForRtl { get; set; }
        public Keys MainToggleVideoControls { get; set; }
        public Keys WaveformVerticalZoom { get; set; }
        public Keys WaveformVerticalZoomOut { get; set; }
        public Keys WaveformZoomIn { get; set; }
        public Keys WaveformZoomOut { get; set; }
        public Keys WaveformSplit { get; set; }
        public Keys WaveformPlaySelection { get; set; }
        public Keys WaveformPlaySelectionEnd { get; set; }
        public Keys WaveformSearchSilenceForward { get; set; }
        public Keys WaveformSearchSilenceBack { get; set; }
        public Keys WaveformAddTextAtHere { get; set; }
        public Keys WaveformAddTextAtHereFromClipboard { get; set; }
        public Keys WaveformSetParagraphAsNewSelection { get; set; }
        public Keys WaveformGoToPreviousShotChange { get; set; }
        public Keys WaveformGoToNextShotChange { get; set; }
        public Keys WaveformToggleShotChange { get; set; }
        public Keys WaveformListShotChanges { get; set; }
        public Keys WaveformGuessStart { get; set; }
        public Keys WaveformAudioToTextVosk { get; set; }
        public Keys WaveformAudioToTextWhisper { get; set; }
        public Keys MainTranslateGoogleIt { get; set; }
        public Keys MainCheckFixTimingViaShotChanges { get; set; }
        public Keys MainTranslateGoogleTranslateIt { get; set; }
        public Keys MainTranslateCustomSearch1 { get; set; }
        public Keys MainTranslateCustomSearch2 { get; set; }
        public Keys MainTranslateCustomSearch3 { get; set; }
        public Keys MainTranslateCustomSearch4 { get; set; }
        public Keys MainTranslateCustomSearch5 { get; set; }

        public void SetShortcuts()
        {
            MainGeneralGoToFirstSelectedLine = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToFirstSelectedLine);
            MainGeneralGoToFirstEmptyLine = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToNextEmptyLine);
            MainGeneralMergeSelectedLines = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralMergeSelectedLines);
            MainGeneralMergeSelectedLinesAndAutoBreak = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralMergeSelectedLinesAndAutoBreak);
            MainGeneralMergeSelectedLinesAndUnbreak = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralMergeSelectedLinesAndUnbreak);
            MainGeneralMergeSelectedLinesAndUnbreakNoSpace = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralMergeSelectedLinesAndUnbreakCjk);
            MainGeneralMergeSelectedLinesBilingual = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralMergeSelectedLinesBilingual);
            MainGeneralMergeWithPreviousBilingual = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralMergeWithPreviousBilingual);
            MainGeneralMergeWithNextBilingual = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralMergeWithNextBilingual);
            MainGeneralMergeSelectedLinesOnlyFirstText = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralMergeSelectedLinesOnlyFirstText);
            MainGeneralToggleTranslationMode = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralToggleTranslationMode);
            MainGeneralSwitchTranslationAndOriginal = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralSwitchOriginalAndTranslation);
            MainGeneralSwitchTranslationAndOriginalTextBoxes = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralSwitchOriginalAndTranslationTextBoxes);
            MainGeneralLayoutChoose = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralLayoutChoose);
            MainGeneralLayoutChoose1 = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralLayoutChoose1);
            MainGeneralLayoutChoose2 = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralLayoutChoose2);
            MainGeneralLayoutChoose3 = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralLayoutChoose3);
            MainGeneralLayoutChoose4 = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralLayoutChoose4);
            MainGeneralLayoutChoose5 = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralLayoutChoose5);
            MainGeneralLayoutChoose6 = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralLayoutChoose6);
            MainGeneralLayoutChoose7 = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralLayoutChoose7);
            MainGeneralLayoutChoose8 = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralLayoutChoose8);
            MainGeneralLayoutChoose9 = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralLayoutChoose9);
            MainGeneralLayoutChoose10 = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralLayoutChoose10);
            MainGeneralLayoutChoose11 = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralLayoutChoose11);
            MainGeneralLayoutChoose12 = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralLayoutChoose12);
            MainGeneralMergeTranslationAndOriginal = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralMergeOriginalAndTranslation);
            MainGeneralMergeWithNext = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralMergeWithNext);
            MainGeneralMergeWithPrevious = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralMergeWithPrevious);
            MainGeneralMergeWithPreviousAndUnbreak = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralMergeWithPreviousAndUnbreak);
            MainGeneralMergeWithNextAndUnbreak = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralMergeWithNextAndUnbreak);
            MainGeneralMergeWithPreviousAndBreak = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralMergeWithPreviousAndBreak);
            MainGeneralMergeWithNextAndBreak = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralMergeWithNextAndBreak);
            MainGeneralGoToNextSubtitle = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToNextSubtitle);
            MainGeneralGoToNextSubtitlePlayTranslate = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToNextSubtitlePlayTranslate);
            MainGeneralGoToNextSubtitleCursorAtEnd = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToNextSubtitleCursorAtEnd);
            MainGeneralGoToPrevSubtitle = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToPrevSubtitle);
            MainGeneralGoToPrevSubtitlePlayTranslate = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToPrevSubtitlePlayTranslate);
            MainGeneralGoToStartOfCurrentSubtitle = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToStartOfCurrentSubtitle);
            MainGeneralGoToEndOfCurrentSubtitle = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToEndOfCurrentSubtitle);
            MainGeneralFileSaveAll = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainFileSaveAll);
            MainGeneralSetAssaResolution = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralSetAssaResolution);
            MainGeneralTakeAutoBackupNow = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralTakeAutoBackup);
            MainVideoPlayFromJustBefore = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoPlayFromJustBefore);
            MainVideoPlayFromBeginning = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoPlayFromBeginning);
            VideoPause = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoPause);
            VideoStop = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoStop);
            MainVideoFocusSetVideoPosition = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoFocusSetVideoPosition);
            ToggleVideoDockUndock = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoToggleVideoControls);
            VideoPlayPauseToggle = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoPlayPauseToggle);
            VideoPlay150Speed = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoPlay150Speed);
            VideoPlay200Speed = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoPlay200Speed);
            Video1FrameLeft = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideo1FrameLeft);
            Video1FrameRight = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideo1FrameRight);
            Video1FrameLeftWithPlay = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideo1FrameLeftWithPlay);
            Video1FrameRightWithPlay = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideo1FrameRightWithPlay);
            Video100MsLeft = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideo100MsLeft);
            Video100MsRight = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideo100MsRight);
            Video500MsLeft = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideo500MsLeft);
            Video500MsRight = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideo500MsRight);
            Video1000MsLeft = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideo1000MsLeft);
            Video1000MsRight = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideo1000MsRight);
            Video5000MsLeft = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideo5000MsLeft);
            Video5000MsRight = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideo5000MsRight);
            VideoXSMsLeft = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoXSMsLeft);
            VideoXSMsRight = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoXSMsRight);
            VideoXLMsLeft = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoXLMsLeft);
            VideoXLMsRight = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoXLMsRight);
            MainVideo3000MsLeft = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideo3000MsLeft);
            MainVideo3000MsRight= UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideo3000MsRight);
            MainVideoGoToStartCurrent = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoGoToStartCurrent);
            MainVideoToggleStartEndCurrent = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoToggleStartEndCurrent);
            MainVideoPlaySelectedLines = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoPlaySelectedLines);
            MainVideoLoopSelectedLines = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoLoopSelectedLines);
            VideoGoToPrevSubtitle = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoGoToPrevSubtitle);
            VideoGoToNextSubtitle = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoGoToNextSubtitle);
            VideoGoToPrevTimeCode = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoGoToPrevTimeCode);
            VideoGoToNextTimeCode = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoGoToNextTimeCode);
            VideoGoToPrevChapter = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoGoToPrevChapter);
            VideoGoToNextChapter = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoGoToNextChapter);
            VideoSelectNextSubtitle = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoSelectNextSubtitle);
            VideoPlayFirstSelected = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralPlayFirstSelected);
            MainGoToPreviousSubtitleAndFocusVideo = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToPreviousSubtitleAndFocusVideo);
            MainGoToNextSubtitleAndFocusVideo = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToNextSubtitleAndFocusVideo);
            MainGoToPreviousSubtitleAndFocusWaveform = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToPreviousSubtitleAndFocusWaveform);
            MainGoToNextSubtitleAndFocusWaveform = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToNextSubtitleAndFocusWaveform);
            MainGoToPrevSubtitleAndPlay = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToPrevSubtitleAndPlay);
            MainGoToNextSubtitleAndPlay = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToNextSubtitleAndPlay);
            MainAutoCalcCurrentDuration = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralAutoCalcCurrentDuration);
            MainAutoCalcCurrentDurationByOptimalReadingSpeed = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralAutoCalcCurrentDurationByOptimalReadingSpeed);
            MainAutoCalcCurrentDurationByMinReadingSpeed = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralAutoCalcCurrentDurationByMinReadingSpeed);
            MainGeneralToggleBookmarks = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralToggleBookmarks);
            MainGeneralFocusTextBox = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralFocusTextBox);
            MainGeneralToggleBookmarksAddComment = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralToggleBookmarksWithText);
            MainGeneralEditBookmark = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralEditBookmarks);
            MainGeneralClearBookmarks = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralClearBookmarks);
            MainGeneralGoToBookmark = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToBookmark);
            MainGeneralGoToPreviousBookmark = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToPreviousBookmark);
            MainGeneralGoToNextBookmark = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToNextBookmark);
            MainGeneralChooseProfile = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralChooseProfile);
            MainGeneralOpenDataFolder = UiUtil.GetKeys(Configuration.Settings.Shortcuts.OpenDataFolder);
            MainGeneralDuplicateLine = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralDuplicateLine);
            MainGeneralToggleView = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralToggleView);
            MainGeneralToggleMode = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralToggleMode);
            MainGeneralTogglePreviewOnVideo = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralTogglePreviewOnVideo);
            MainVideoFullscreen = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoFullscreen);
            MainVideoSlower = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoSlower);
            MainVideoFaster = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoFaster);
            MainVideoSpeedToggle = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoSpeedToggle);
            MainVideoReset = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoReset);
            MainVideoAudioToTextVosk = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoAudioToTextVosk);
            MainVideoAudioToTextWhisper = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoAudioToTextWhisper);
            MainVideoAudioExtractSelectedLines = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoAudioExtractAudioSelectedLines);
            MainVideoTextToSpeech = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoTextToSpeech);
            MainVideoToggleBrightness = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoToggleBrightness);
            MainVideoToggleContrast = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoToggleContrast);
            MainToolsAutoDuration = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainToolsAutoDuration);
            MainListViewToggleDashes = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainListViewToggleDashes);
            MainListViewToggleQuotes = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainListViewToggleQuotes);
            MainListViewToggleHiTags = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainListViewToggleHiTags);
            MainListViewToggleCustomTags = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainListViewToggleCustomTags);
            MainListViewToggleMusicSymbols = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainListViewToggleMusicSymbols);
            MainListViewAutoDuration = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainListViewAutoDuration);
            MainListViewAlignmentN1 = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainListViewAlignmentN1);
            MainListViewAlignmentN2 = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainListViewAlignmentN2);
            MainListViewAlignmentN3 = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainListViewAlignmentN3);
            MainListViewAlignmentN4 = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainListViewAlignmentN4);
            MainListViewAlignmentN5 = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainListViewAlignmentN5);
            MainListViewAlignmentN6 = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainListViewAlignmentN6);
            MainListViewAlignmentN7 = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainListViewAlignmentN7);
            MainListViewAlignmentN8 = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainListViewAlignmentN8);
            MainListViewAlignmentN9 = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainListViewAlignmentN9);
            MainListViewColor1 = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainListViewColor1);
            MainListViewColor2 = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainListViewColor2);
            MainListViewColor3 = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainListViewColor3);
            MainListViewColor4 = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainListViewColor4);
            MainListViewColor5 = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainListViewColor5);
            MainListViewColor6 = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainListViewColor6);
            MainListViewColor7 = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainListViewColor7);
            MainListViewColor8 = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainListViewColor8);
            MainListViewSetNewActor = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainListViewSetNewActor);
            MainListViewSetActor1 = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainListViewSetActor1);
            MainListViewSetActor2 = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainListViewSetActor2);
            MainListViewSetActor3 = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainListViewSetActor3);
            MainListViewSetActor4 = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainListViewSetActor4);
            MainListViewSetActor5 = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainListViewSetActor5);
            MainListViewSetActor6 = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainListViewSetActor6);
            MainListViewSetActor7 = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainListViewSetActor7);
            MainListViewSetActor8 = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainListViewSetActor8);
            MainListViewSetActor9 = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainListViewSetActor9);
            MainListViewSetActor10 = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainListViewSetActor10);
            MainListViewGoToNextError = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainListViewGoToNextError);
            MainListViewRemoveBlankLines = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralRemoveBlankLines);
            MainListViewRemoveTimeCodes = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainListViewRemoveTimeCodes);
            MainEditFixRTLViaUnicodeChars = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainEditFixRTLViaUnicodeChars);
            MainEditRemoveRTLUnicodeChars = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainEditRemoveRTLUnicodeChars);
            MainEditReverseStartAndEndingForRtl = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainEditReverseStartAndEndingForRTL);
            MainToggleVideoControls = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoToggleControls);
            MainListViewCopyText = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainListViewCopyText);
            MainListViewCopyPlainText = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainListViewCopyPlainText);
            MainTextBoxSplitAtCursor = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainTextBoxSplitAtCursor);
            MainTextBoxSplitAtCursorAndAutoBr = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainTextBoxSplitAtCursorAndAutoBr);
            MainTextBoxSplitAtCursorAndVideoPos = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainTextBoxSplitAtCursorAndVideoPos);
            MainTextBoxSplitSelectedLineBilingual = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainTextBoxSplitSelectedLineBilingual);
            MainTextBoxMoveLastWordDown = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainTextBoxMoveLastWordDown);
            MainTextBoxMoveFirstWordFromNextUp = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainTextBoxMoveFirstWordFromNextUp);
            MainTextBoxMoveLastWordDownCurrent = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainTextBoxMoveLastWordDownCurrent);
            MainTextBoxMoveFromCursorToNextAndGoToNext = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainTextBoxMoveFromCursorToNextAndGoToNext);
            MainTextBoxMoveFirstWordUpCurrent = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainTextBoxMoveFirstWordUpCurrent);
            MainTextBoxSelectionToLower = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainTextBoxSelectionToLower);
            MainTextBoxSelectionToUpper = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainTextBoxSelectionToUpper);
            MainTextBoxSelectionToggleCasing = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainTextBoxSelectionToggleCasing);
            MainTextBoxToggleAutoDuration = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainTextBoxToggleAutoDuration);
            MainCreateInsertSubAtVideoPos = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainCreateInsertSubAtVideoPos);
            MainCreateInsertSubAtVideoPosNoTextBoxFocus = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainCreateInsertSubAtVideoPosNoTextBoxFocus);
            MainCreateInsertSubAtVideoPosMax = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainCreateInsertSubAtVideoPosMax);
            MainCreateSetStart = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainCreateSetStart);
            MainCreateSetEnd = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainCreateSetEnd);
            MainAdjustVideoSetStartForAppropriateLine = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainAdjustVideoSetStartForAppropriateLine);
            MainAdjustVideoSetEndForAppropriateLine = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainAdjustVideoSetEndForAppropriateLine);
            MainAdjustSetEndAndPause = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainAdjustSetEndAndPause);
            MainCreateStartDownEndUp = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainCreateStartDownEndUp);
            MainCreateSetEndAddNewAndGoToNew = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainCreateSetEndAddNewAndGoToNew);
            MainAdjustSetStartAndOffsetTheRest = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainAdjustSetStartAndOffsetTheRest);
            MainAdjustSetStartAndOffsetTheRest2 = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainAdjustSetStartAndOffsetTheRest2);
            MainAdjustSetStartAndOffsetTheWholeSubtitle = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainAdjustSetStartAndOffsetTheWholeSubtitle);
            MainAdjustSetEndAndOffsetTheRest = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainAdjustSetEndAndOffsetTheRest);
            MainAdjustSetEndAndOffsetTheRestAndGoToNext = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainAdjustSetEndAndOffsetTheRestAndGoToNext);
            MainAdjustSetStartAndGotoNext = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainAdjustSetStartAndGotoNext);
            MainAdjustSetEndAndGotoNext = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainAdjustSetEndAndGotoNext);
            MainAdjustInsertViaEndAutoStart = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainAdjustViaEndAutoStart);
            MainAdjustInsertViaEndAutoStartAndGoToNext = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainAdjustViaEndAutoStartAndGoToNext);
            MainAdjustSetEndMinusGapAndStartNextHere = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainAdjustSetEndMinusGapAndStartNextHere);
            MainAdjustSetEndAndStartOfNextPlusGap = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainSetEndAndStartNextAfterGap);
            MainAdjustSetStartAutoDurationAndGoToNext = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainAdjustSetStartAutoDurationAndGoToNext);
            MainAdjustSetEndNextStartAndGoToNext = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainAdjustSetEndNextStartAndGoToNext);
            MainAdjustStartDownEndUpAndGoToNext = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainAdjustStartDownEndUpAndGoToNext);
            MainAdjustSetStartAndEndOfPrevious = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainAdjustSetStartAndEndOfPrevious);
            MainAdjustSetStartAndEndOfPreviousAndGoToNext = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainAdjustSetStartAndEndOfPreviousAndGoToNext);
            MainAdjustSetStartKeepDuration = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainAdjustSetStartKeepDuration);
            MainAdjustSelected100MsForward = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainAdjustSelected100MsForward);
            MainAdjustSelected100MsBack = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainAdjustSelected100MsBack);
            MainAdjustAdjustStartXMsBack = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainAdjustStartXMsBack);
            MainAdjustAdjustStartXMsForward = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainAdjustStartXMsForward);
            MainAdjustAdjustEndXMsBack = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainAdjustEndXMsBack);
            MainAdjustAdjustEndXMsForward = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainAdjustEndXMsForward);
            MainAdjustMoveStartOneFrameBack = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MoveStartOneFrameBack);
            MainAdjustMoveStartOneFrameForward = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MoveStartOneFrameForward);
            MainAdjustMoveEndOneFrameBack = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MoveEndOneFrameBack);
            MainAdjustMoveEndOneFrameForward = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MoveEndOneFrameForward);
            MainAdjustMoveStartOneFrameBackKeepGapPrev = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MoveStartOneFrameBackKeepGapPrev);
            MainAdjustMoveStartOneFrameForwardKeepGapPrev = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MoveStartOneFrameForwardKeepGapPrev);
            MainAdjustMoveEndOneFrameBackKeepGapNext = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MoveEndOneFrameBackKeepGapNext);
            MainAdjustMoveEndOneFrameForwardKeepGapNext = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MoveEndOneFrameForwardKeepGapNext);
            MainAdjustSnapStartToNextShotChange = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainAdjustSnapStartToNextShotChange);
            MainAdjustSnapEndToPreviousShotChange = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainAdjustSnapEndToPreviousShotChange);
            MainAdjustExtendToNextShotChange = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainAdjustExtendToNextShotChange);
            MainAdjustExtendToPreviousShotChange = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainAdjustExtendToPreviousShotChange);
            MainAdjustExtendToNextSubtitle = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainAdjustExtendToNextSubtitle);
            MainAdjustExtendToPreviousSubtitle = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainAdjustExtendToPreviousSubtitle);
            MainAdjustExtendToNextSubtitleMinusChainingGap = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainAdjustExtendToNextSubtitleMinusChainingGap);
            MainAdjustExtendToPreviousSubtitleMinusChainingGap = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainAdjustExtendToPreviousSubtitleMinusChainingGap);
            MainAdjustExtendCurrentSubtitle = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainAdjustExtendCurrentSubtitle);
            MainAdjustExtendPreviousLineEndToCurrentStart = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainAdjustExtendPreviousLineEndToCurrentStart);
            MainAdjustExtendNextLineStartToCurrentEnd = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainAdjustExtendNextLineStartToCurrentEnd);
            MainSetInCueToClosestShotChangeLeftGreenZone = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainSetInCueToClosestShotChangeLeftGreenZone);
            MainSetInCueToClosestShotChangeRightGreenZone = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainSetInCueToClosestShotChangeRightGreenZone);
            MainSetOutCueToClosestShotChangeLeftGreenZone = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainSetOutCueToClosestShotChangeLeftGreenZone);
            MainSetOutCueToClosestShotChangeRightGreenZone = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainSetOutCueToClosestShotChangeRightGreenZone);
            MainInsertAfter = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainInsertAfter);
            MainInsertBefore = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainInsertBefore);
            MainTextBoxAutoBreak = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainTextBoxAutoBreak);
            MainAutoTranslateSelectedLines = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainTranslateAutoSelectedLines);
            MainTextBoxBreakAtCursorPosition = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainTextBoxBreakAtPosition);
            MainTextBoxBreakAtCursorPositionAndGoToNext = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainTextBoxBreakAtPositionAndGoToNext);
            MainTextBoxRecord = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainTextBoxRecord);
            MainTextBoxUnbreak = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainTextBoxUnbreak);
            MainTextBoxUnbreakNoSpace = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainTextBoxUnbreakNoSpace);
            MainTextBoxAssaIntellisense = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainTextBoxAssaIntellisense);
            MainTextBoxAssaRemoveTag = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainTextBoxAssaRemoveTag);
            MainMergeDialog = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainMergeDialog);
            MainMergeDialogWithNext = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainMergeDialogWithNext);
            MainMergeDialogWithPrevious = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainMergeDialogWithPrevious);
            MainToggleFocus = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainToggleFocus);
            MainToggleFocusWaveform = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainToggleFocusWaveform);
            MainToggleFocusWaveformTextBox = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainToggleFocusWaveformTextBox);
            MainWaveformAdd = UiUtil.GetKeys(Configuration.Settings.Shortcuts.WaveformAdd);
            WaveformVerticalZoom = UiUtil.GetKeys(Configuration.Settings.Shortcuts.WaveformVerticalZoom);
            WaveformVerticalZoomOut = UiUtil.GetKeys(Configuration.Settings.Shortcuts.WaveformVerticalZoomOut);
            WaveformZoomIn = UiUtil.GetKeys(Configuration.Settings.Shortcuts.WaveformZoomIn);
            WaveformZoomOut = UiUtil.GetKeys(Configuration.Settings.Shortcuts.WaveformZoomOut);
            WaveformSplit = UiUtil.GetKeys(Configuration.Settings.Shortcuts.WaveformSplit);
            WaveformPlaySelection = UiUtil.GetKeys(Configuration.Settings.Shortcuts.WaveformPlaySelection);
            WaveformPlaySelectionEnd = UiUtil.GetKeys(Configuration.Settings.Shortcuts.WaveformPlaySelectionEnd);
            WaveformSearchSilenceForward = UiUtil.GetKeys(Configuration.Settings.Shortcuts.WaveformSearchSilenceForward);
            WaveformSearchSilenceBack = UiUtil.GetKeys(Configuration.Settings.Shortcuts.WaveformSearchSilenceBack);
            WaveformAddTextAtHere = UiUtil.GetKeys(Configuration.Settings.Shortcuts.WaveformAddTextHere);
            WaveformAddTextAtHereFromClipboard = UiUtil.GetKeys(Configuration.Settings.Shortcuts.WaveformAddTextHereFromClipboard);
            WaveformSetParagraphAsNewSelection = UiUtil.GetKeys(Configuration.Settings.Shortcuts.WaveformSetParagraphAsSelection);
            WaveformGoToPreviousShotChange = UiUtil.GetKeys(Configuration.Settings.Shortcuts.WaveformGoToPreviousShotChange);
            WaveformGoToNextShotChange = UiUtil.GetKeys(Configuration.Settings.Shortcuts.WaveformGoToNextShotChange);
            WaveformToggleShotChange = UiUtil.GetKeys(Configuration.Settings.Shortcuts.WaveformToggleShotChange);
            WaveformListShotChanges = UiUtil.GetKeys(Configuration.Settings.Shortcuts.WaveformListShotChanges);
            WaveformGuessStart = UiUtil.GetKeys(Configuration.Settings.Shortcuts.WaveformGuessStart);
            WaveformAudioToTextVosk = UiUtil.GetKeys(Configuration.Settings.Shortcuts.WaveformAudioToTextVosk);
            WaveformAudioToTextWhisper = UiUtil.GetKeys(Configuration.Settings.Shortcuts.WaveformAudioToTextWhisper);
            MainTranslateGoogleIt = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainTranslateGoogleIt);
            MainCheckFixTimingViaShotChanges = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainCheckFixTimingViaShotChanges);
            MainTranslateGoogleTranslateIt = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainTranslateGoogleTranslateIt);
            MainTranslateCustomSearch1 = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainTranslateCustomSearch1);
            MainTranslateCustomSearch2 = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainTranslateCustomSearch2);
            MainTranslateCustomSearch3 = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainTranslateCustomSearch3);
            MainTranslateCustomSearch4 = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainTranslateCustomSearch4);
            MainTranslateCustomSearch5 = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainTranslateCustomSearch5);
        }
    }
}
