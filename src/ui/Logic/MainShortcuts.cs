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
        public Keys MainGeneralMergeSelectedLinesOnlyFirstText { get; set; }
        public Keys MainGeneralToggleTranslationMode { get; set; }
        public Keys MainGeneralSwitchTranslationAndOriginal { get; set; }
        public Keys MainGeneralMergeTranslationAndOriginal { get; set; }
        public Keys MainGeneralMergeWithNext { get; set; }
        public Keys MainGeneralMergeWithPrevious { get; set; }
        public Keys MainGeneralGoToNextSubtitle { get; set; }
        public Keys MainGeneralGoToNextSubtitlePlayTranslate { get; set; }
        public Keys MainGeneralGoToNextSubtitleCursorAtEnd { get; set; }
        public Keys MainGeneralGoToPrevSubtitle { get; set; }
        public Keys MainGeneralGoToPrevSubtitlePlayTranslate { get; set; }
        public Keys MainGeneralGoToStartOfCurrentSubtitle { get; set; }
        public Keys MainGeneralGoToEndOfCurrentSubtitle { get; set; }
        public Keys MainGeneralFileSaveAll { get; set; }
        public Keys MainGeneralSetAssaResolution { get; set; }
        public Keys MainToolsAutoDuration { get; set; }
        public Keys MainVideoFoucsSetVideoPosition { get; set; }
        public Keys ToggleVideoDockUndock { get; set; }
        public Keys VideoPause { get; set; }
        public Keys VideoStop { get; set; }
        public Keys VideoPlayPauseToggle { get; set; }
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
        public Keys Video5000MsLeft { get; set; }
        public Keys Video5000MsRight { get; set; }
        public Keys VideoXSMsLeft { get; set; }
        public Keys VideoXSMsRight { get; set; }
        public Keys VideoXLMsLeft { get; set; }
        public Keys VideoXLMsRight { get; set; }
        public Keys MainVideo3000MsLeft { get; set; }
        public Keys MainVideoGoToStartCurrent { get; set; }
        public Keys MainVideoToggleStartEndCurrent { get; set; }
        public Keys MainVideoPlaySelectedLines { get; set; }
        public Keys VideoGoToPrevSubtitle { get; set; }
        public Keys VideoGoToNextSubtitle { get; set; }
        public Keys VideoGoToPrevChapter { get; set; }
        public Keys VideoGoToNextChapter { get; set; }
        public Keys VideoSelectNextSubtitle { get; set; }
        public Keys VideoPlayFirstSelected { get; set; }
        public Keys MainVideoFullscreen { get; set; }
        public Keys MainVideoSlower { get; set; }
        public Keys MainVideoFaster { get; set; }
        public Keys MainVideoReset { get; set; }
        public Keys MainVideoToggleBrightness { get; set; }
        public Keys MainVideoToggleContrast { get; set; }
        public Keys MainGoToPreviousSubtitleAndFocusVideo { get; set; }
        public Keys MainGoToNextSubtitleAndFocusVideo { get; set; }
        public Keys MainGoToPrevSubtitleAndPlay { get; set; }
        public Keys MainGoToNextSubtitleAndPlay { get; set; }
        public Keys MainAutoCalcCurrentDuration { get; set; }
        public Keys MainGeneralToggleBookmarks { get; set; }
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
        public Keys MainAdjustSetEndAndGotoNext { get; set; }
        public Keys MainAdjustInsertViaEndAutoStart { get; set; }
        public Keys MainAdjustInsertViaEndAutoStartAndGoToNext { get; set; }
        public Keys MainAdjustSetEndMinusGapAndStartNextHere { get; set; }
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
        public Keys MainAdjustSnapStartToNextSceneChange { get; set; }
        public Keys MainAdjustSnapStartToNextSceneChangeWithGap { get; set; }
        public Keys MainAdjustSnapEndToPreviousSceneChange { get; set; }
        public Keys MainAdjustSnapEndToPreviousSceneChangeWithGap { get; set; }
        public Keys MainAdjustExtendToNextSceneChange { get; set; }
        public Keys MainAdjustExtendToNextSceneChangeWithGap { get; set; }
        public Keys MainAdjustExtendToPreviousSceneChange { get; set; }
        public Keys MainAdjustExtendToPreviousSceneChangeWithGap { get; set; }
        public Keys MainAdjustExtendToNextSubtitle { get; set; }
        public Keys MainAdjustExtendToPreviousSubtitle { get; set; }
        public Keys MainAdjustExtendCurrentSubtitle { get; set; }
        public Keys MainAdjustExtendPreviousLineEndToCurrentStart { get; set; }
        public Keys MainAdjustExtendNextLineStartToCurrentEnd { get; set; }
        public Keys MainInsertAfter { get; set; }
        public Keys MainInsertBefore { get; set; }
        public Keys MainTextBoxAutoBreak { get; set; }
        public Keys MainTextBoxUnbreak { get; set; }
        public Keys MainTextBoxUnbreakNoSpace { get; set; }
        public Keys MainTextBoxAssaIntellisense { get; set; }
        public Keys MainTextBoxAssaRemoveTag { get; set; }
        public Keys MainTextBoxBreakAtCursorPosition { get; set; }
        public Keys MainTextBoxBreakAtCursorPositionAndGoToNext { get; set; }
        public Keys MainMergeDialog { get; set; }
        public Keys MainToggleFocus { get; set; }
        public Keys MainToggleFocusWaveform { get; set; }
        public Keys MainWaveformAdd { get; set; }
        public Keys MainListViewToggleDashes { get; set; }
        public Keys MainListViewToggleQuotes { get; set; }
        public Keys MainListViewToggleHiTags { get; set; }
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
        public Keys MainListViewGoToNextError { get; set; }
        public Keys MainListViewRemoveBlankLines { get; set; }
        public Keys MainListViewRemoveTimeCodes { get; set; }
        public Keys MainListViewCopyText { get; set; }
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
        public Keys WaveformGoToPreviousSceneChange { get; set; }
        public Keys WaveformGoToNextSceneChange { get; set; }
        public Keys WaveformToggleSceneChange { get; set; }
        public Keys WaveformGuessStart { get; set; }
        public Keys MainTranslateGoogleIt { get; set; }
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
            MainGeneralMergeSelectedLinesOnlyFirstText = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralMergeSelectedLinesOnlyFirstText);
            MainGeneralToggleTranslationMode = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralToggleTranslationMode);
            MainGeneralSwitchTranslationAndOriginal = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralSwitchOriginalAndTranslation);
            MainGeneralMergeTranslationAndOriginal = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralMergeOriginalAndTranslation);
            MainGeneralMergeWithNext = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralMergeWithNext);
            MainGeneralMergeWithPrevious = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralMergeWithPrevious);
            MainGeneralGoToNextSubtitle = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToNextSubtitle);
            MainGeneralGoToNextSubtitlePlayTranslate = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToNextSubtitlePlayTranslate);
            MainGeneralGoToNextSubtitleCursorAtEnd = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToNextSubtitleCursorAtEnd);
            MainGeneralGoToPrevSubtitle = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToPrevSubtitle);
            MainGeneralGoToPrevSubtitlePlayTranslate = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToPrevSubtitlePlayTranslate);
            MainGeneralGoToStartOfCurrentSubtitle = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToStartOfCurrentSubtitle);
            MainGeneralGoToEndOfCurrentSubtitle = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToEndOfCurrentSubtitle);
            MainGeneralFileSaveAll = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainFileSaveAll);
            MainGeneralSetAssaResolution = UiUtil.GetKeys(Configuration.Settings.Shortcuts.SetAssaResolution);
            MainVideoPlayFromJustBefore = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoPlayFromJustBefore);
            MainVideoPlayFromBeginning = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoPlayFromBeginning);
            VideoPause = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoPause);
            VideoStop = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoStop);
            MainVideoFoucsSetVideoPosition = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoFoucsSetVideoPosition);
            ToggleVideoDockUndock = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoToggleVideoControls);
            VideoPlayPauseToggle = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoPlayPauseToggle);
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
            MainVideoGoToStartCurrent = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoGoToStartCurrent);
            MainVideoToggleStartEndCurrent = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoToggleStartEndCurrent);
            MainVideoPlaySelectedLines = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoPlaySelectedLines);
            VideoGoToPrevSubtitle = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoGoToPrevSubtitle);
            VideoGoToNextSubtitle = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoGoToNextSubtitle);
            VideoGoToPrevChapter = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoGoToPrevChapter);
            VideoGoToNextChapter = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoGoToNextChapter);
            VideoSelectNextSubtitle = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoSelectNextSubtitle);
            VideoPlayFirstSelected = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralPlayFirstSelected);
            MainGoToPreviousSubtitleAndFocusVideo = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToPreviousSubtitleAndFocusVideo);
            MainGoToNextSubtitleAndFocusVideo = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToNextSubtitleAndFocusVideo);
            MainGoToPrevSubtitleAndPlay = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToPrevSubtitleAndPlay);
            MainGoToNextSubtitleAndPlay = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToNextSubtitleAndPlay);
            MainAutoCalcCurrentDuration = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralAutoCalcCurrentDuration);
            MainGeneralToggleBookmarks = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralToggleBookmarks);
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
            MainVideoReset = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoReset);
            MainVideoToggleBrightness = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoToggleBrightness);
            MainVideoToggleContrast = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainVideoToggleContrast);
            MainToolsAutoDuration = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainToolsAutoDuration);
            MainListViewToggleDashes = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainListViewToggleDashes);
            MainListViewToggleQuotes = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainListViewToggleQuotes);
            MainListViewToggleHiTags = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainListViewToggleHiTags);
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
            MainListViewGoToNextError = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainListViewGoToNextError);
            MainListViewRemoveBlankLines = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainListViewRemoveBlankLines);
            MainListViewRemoveTimeCodes = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainListViewRemoveTimeCodes);
            MainEditFixRTLViaUnicodeChars = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainEditFixRTLViaUnicodeChars);
            MainEditRemoveRTLUnicodeChars = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainEditRemoveRTLUnicodeChars);
            MainEditReverseStartAndEndingForRtl = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainEditReverseStartAndEndingForRTL);
            MainToggleVideoControls = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainToggleVideoControls);
            MainListViewCopyText = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainListViewCopyText);
            MainTextBoxSplitAtCursor = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainTextBoxSplitAtCursor);
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
            MainAdjustSetEndAndGotoNext = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainAdjustSetEndAndGotoNext);
            MainAdjustInsertViaEndAutoStart = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainAdjustViaEndAutoStart);
            MainAdjustInsertViaEndAutoStartAndGoToNext = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainAdjustViaEndAutoStartAndGoToNext);
            MainAdjustSetEndMinusGapAndStartNextHere = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainAdjustSetEndMinusGapAndStartNextHere);
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
            MainAdjustSnapStartToNextSceneChange = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainAdjustSnapStartToNextSceneChange);
            MainAdjustSnapStartToNextSceneChangeWithGap = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainAdjustSnapStartToNextSceneChangeWithGap);
            MainAdjustSnapEndToPreviousSceneChange = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainAdjustSnapEndToPreviousSceneChange);
            MainAdjustSnapEndToPreviousSceneChangeWithGap = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainAdjustSnapEndToPreviousSceneChangeWithGap);
            MainAdjustExtendToNextSceneChange = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainAdjustExtendToNextSceneChange);
            MainAdjustExtendToNextSceneChangeWithGap = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainAdjustExtendToNextSceneChangeWithGap);
            MainAdjustExtendToPreviousSceneChange = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainAdjustExtendToPreviousSceneChange);
            MainAdjustExtendToPreviousSceneChangeWithGap = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainAdjustExtendToPreviousSceneChangeWithGap);
            MainAdjustExtendToNextSubtitle = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainAdjustExtendToNextSubtitle);
            MainAdjustExtendToPreviousSubtitle = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainAdjustExtendToPreviousSubtitle);
            MainAdjustExtendCurrentSubtitle = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainAdjustExtendCurrentSubtitle);
            MainAdjustExtendPreviousLineEndToCurrentStart = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainAdjustExtendPreviousLineEndToCurrentStart);
            MainAdjustExtendNextLineStartToCurrentEnd = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainAdjustExtendNextLineStartToCurrentEnd);
            MainInsertAfter = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainInsertAfter);
            MainInsertBefore = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainInsertBefore);
            MainTextBoxAutoBreak = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainTextBoxAutoBreak);
            MainTextBoxBreakAtCursorPosition = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainTextBoxBreakAtPosition);
            MainTextBoxBreakAtCursorPositionAndGoToNext = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainTextBoxBreakAtPositionAndGoToNext);
            MainTextBoxUnbreak = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainTextBoxUnbreak);
            MainTextBoxUnbreakNoSpace = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainTextBoxUnbreakNoSpace);
            MainTextBoxAssaIntellisense = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainTextBoxAssaIntellisense);
            MainTextBoxAssaRemoveTag = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainTextBoxAssaRemoveTag);
            MainMergeDialog = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainMergeDialog);
            MainToggleFocus = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainToggleFocus);
            MainToggleFocusWaveform = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainToggleFocusWaveform);
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
            WaveformGoToPreviousSceneChange = UiUtil.GetKeys(Configuration.Settings.Shortcuts.WaveformGoToPreviousSceneChange);
            WaveformGoToNextSceneChange = UiUtil.GetKeys(Configuration.Settings.Shortcuts.WaveformGoToNextSceneChange);
            WaveformToggleSceneChange = UiUtil.GetKeys(Configuration.Settings.Shortcuts.WaveformToggleSceneChange);
            WaveformGuessStart = UiUtil.GetKeys(Configuration.Settings.Shortcuts.WaveformGuessStart);
            MainTranslateGoogleIt = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainTranslateGoogleIt);
            MainTranslateGoogleTranslateIt = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainTranslateGoogleTranslateIt);
            MainTranslateCustomSearch1 = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainTranslateCustomSearch1);
            MainTranslateCustomSearch2 = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainTranslateCustomSearch2);
            MainTranslateCustomSearch3 = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainTranslateCustomSearch3);
            MainTranslateCustomSearch4 = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainTranslateCustomSearch4);
            MainTranslateCustomSearch5 = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainTranslateCustomSearch5);
        }
    }
}
