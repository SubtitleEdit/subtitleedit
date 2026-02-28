using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Options.Shortcuts;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Logic;

public static class ShortcutsMain
{
    public static List<ShortCut> GetUsedShortcuts(MainViewModel vm)
    {
        var shortcuts = new List<ShortCut>();

        var keys = Se.Settings.Shortcuts
            .Where(p => !p.ActionName.Contains(' '))
            .GroupBy(p => p.ActionName)
            .Select(g => g.First())
            .ToDictionary(p => p.ActionName, p => p);

        foreach (var shortcut in GetAllAvailableShortcuts(vm))
        {
            if (keys.TryGetValue(shortcut.Name, out var match))
            {
                shortcuts.Add(new ShortCut(shortcut, match));
            }
        }

        return shortcuts;
    }

    public static List<ShortCut> GetAllShortcuts(MainViewModel vm)
    {
        var shortcuts = new List<ShortCut>();
        var keys = Se.Settings.Shortcuts.ToDictionary(p => p.ActionName, p => p);
        foreach (var shortcut in GetAllAvailableShortcuts(vm))
        {
            shortcuts.Add(keys.TryGetValue(shortcut.Name, out var match)
                ? new ShortCut(shortcut, match)
                : new ShortCut(shortcut));
        }

        return shortcuts;
    }

    private static void AddShortcut<T>(IList<AvailableShortcut> list, T command, string name, ShortcutCategory category)
    where T : IRelayCommand
    {
        list.Add(new AvailableShortcut(command, name, category));
    }

    public static readonly Dictionary<string, string> CommandTranslationLookup = new Dictionary<string, string>
    {
        { nameof(MainViewModel.DeleteSelectedLinesCommand), Se.Language.Options.Shortcuts.ListDeleteSelection },
        { nameof(MainViewModel.RippleDeleteSelectedLinesCommand), Se.Language.Options.Shortcuts.RippleDeleteSelection },
        { nameof(MainViewModel.DuplicateSelectedLinesCommand), Se.Language.Options.Shortcuts.DuplicateSelectedLines},
        { nameof(MainViewModel.ShowAlignmentPickerCommand), Se.Language.Options.Shortcuts.ShowAlignmentPicker},
        { nameof(MainViewModel.DoAlignmentAn1Command), string.Format(Se.Language.General.AlignmentX, "an1")},
        { nameof(MainViewModel.DoAlignmentAn2Command), string.Format(Se.Language.General.AlignmentX, "an2")},
        { nameof(MainViewModel.DoAlignmentAn3Command), string.Format(Se.Language.General.AlignmentX, "an3")},
        { nameof(MainViewModel.DoAlignmentAn4Command), string.Format(Se.Language.General.AlignmentX, "an4")},
        { nameof(MainViewModel.DoAlignmentAn5Command), string.Format(Se.Language.General.AlignmentX, "an5")},
        { nameof(MainViewModel.DoAlignmentAn6Command), string.Format(Se.Language.General.AlignmentX, "an6")},
        { nameof(MainViewModel.DoAlignmentAn7Command), string.Format(Se.Language.General.AlignmentX, "an7")},
        { nameof(MainViewModel.DoAlignmentAn8Command), string.Format(Se.Language.General.AlignmentX, "an8")},
        { nameof(MainViewModel.DoAlignmentAn9Command), string.Format(Se.Language.General.AlignmentX, "an9")},
        { nameof(MainViewModel.AddOrEditBookmarkCommand), Se.Language.Options.Shortcuts.AddOrEditBookmark},
        { nameof(MainViewModel.ToggleBookmarkSelectedLinesNoTextCommand), Se.Language.Options.Shortcuts.ToggleBookmark},
        { nameof(MainViewModel.CopyTextFromOriginalToTranslationCommand), Se.Language.Options.Shortcuts.CopyTextFromOriginalSelectedLines},
        { nameof(MainViewModel.SwitchOriginalAndTranslationTextSelectedLinesCommand), Se.Language.Options.Shortcuts.SwitchOriginalAndTranslationSelectedLines},
        { nameof(MainViewModel.MergeOriginalIntoTranslationSelectedLinesCommand), Se.Language.Options.Shortcuts.MergeOriginalIntoTranslationSelectedLines},
        { nameof(MainViewModel.ListBookmarksCommand), Se.Language.Options.Shortcuts.ListBookmarks},
        { nameof(MainViewModel.GoToNextBookmarkCommand), Se.Language.Options.Shortcuts.GoToNextBookmark},
        { nameof(MainViewModel.OpenDataFolderCommand), Se.Language.Options.Shortcuts.OpenSeDataFolder },
        { nameof(MainViewModel.ToggleIsWaveformToolbarVisibleCommand), Se.Language.Options.Shortcuts.ToggleWaveformToolbar },

        // File
        { nameof(MainViewModel.CommandFileOpenCommand), Se.Language.Options.Shortcuts.FileOpen },
        { nameof(MainViewModel.CommandFileOpenKeepVideoCommand), Se.Language.Options.Shortcuts.FileOpenKeepVideo },
        { nameof(MainViewModel.FileOpenOriginalCommand), Se.Language.Options.Shortcuts.FileOpenOriginal },
        { nameof(MainViewModel.FileCloseOriginalCommand), Se.Language.Options.Shortcuts.FileCloseOriginal },
        { nameof(MainViewModel.CommandExitCommand), Se.Language.Options.Shortcuts.FileExit },
        { nameof(MainViewModel.CommandFileNewCommand), Se.Language.Options.Shortcuts.FileNew },
        { nameof(MainViewModel.CommandFileNewKeepVideoCommand), Se.Language.Options.Shortcuts.CommandFileNewKeepVideo },
        { nameof(MainViewModel.CommandFileSaveCommand), Se.Language.Options.Shortcuts.FileSave },
        { nameof(MainViewModel.CommandFileSaveAsCommand), Se.Language.Options.Shortcuts.FileSaveAs },
        { nameof(MainViewModel.ShowStatisticsCommand), Se.Language.Options.Shortcuts.FileStatistics },
        { nameof(MainViewModel.ShowCompareCommand), Se.Language.Options.Shortcuts.FileCompare },
        { nameof(MainViewModel.ShowRestoreAutoBackupCommand), Se.Language.Options.Shortcuts.RestoreAutoBackup },
        { nameof(MainViewModel.OpenContainingFolderCommand), Se.Language.Options.Shortcuts.OpenContainingFolder },
        { nameof(MainViewModel.CopySubtitlePathToClipboardCommand), Se.Language.Options.Shortcuts.CopySubtitlePathToClipboard },
        { nameof(MainViewModel.CopySubtitleOriginalPathToClipboardCommand), Se.Language.Options.Shortcuts.CopySubtitleOriginalPathToClipboard },
        { nameof(MainViewModel.ImportTimeCodesCommand), Se.Language.Options.Shortcuts.ImportTimeCodes },
        { nameof(MainViewModel.ShowImportSubtitleWithManuallyChosenEncodingCommand), Se.Language.Options.Shortcuts.ImportSubtitleWithManuallyChosenEncoding },
        { nameof(MainViewModel.ExportBluRaySupCommand), Se.Language.Options.Shortcuts.ExportBluRaySup },
        { nameof(MainViewModel.ShowExportCustomTextFormatCommand), Se.Language.Options.Shortcuts.ExportCustomTextFormat },
        { nameof(MainViewModel.ShowExportPlainTextCommand), Se.Language.Options.Shortcuts.ExportPlainText },
        { nameof(MainViewModel.CopyMsRelativeToCurrentSubtitleLineToClipboardCommand), Se.Language.Options.Shortcuts.CopyMsRelativeToCurrentSubtitleLineToClipboard },

        // Edit
        { nameof(MainViewModel.UndoCommand), Se.Language.General.Undo },
        { nameof(MainViewModel.RedoCommand), Se.Language.General.Redo },
        { nameof(MainViewModel.ShowHistoryCommand), Se.Language.Options.Shortcuts.ShowHistory },
        { nameof(MainViewModel.ShowFindCommand), Se.Language.Options.Shortcuts.EditFind },
        { nameof(MainViewModel.FindNextCommand), Se.Language.Options.Shortcuts.EditFindNext },
        { nameof(MainViewModel.FindPreviousCommand), Se.Language.Options.Shortcuts.EditFindPrevious },
        { nameof(MainViewModel.ShowReplaceCommand), Se.Language.Options.Shortcuts.EditReplace },
        { nameof(MainViewModel.ShowMultipleReplaceCommand), Se.Language.Options.Shortcuts.EditMultipleReplace },
        { nameof(MainViewModel.ShowGoToLineCommand), Se.Language.Options.Shortcuts.GeneralGoToLineNumber },
        { nameof(MainViewModel.RightToLeftToggleCommand), Se.Language.Options.Shortcuts.ToggleRightToLeft },
        { nameof(MainViewModel.ReverseRightToLeftStartEndCommand), Se.Language.General.ReverseRightToLeftStartEnd },
        { nameof(MainViewModel.ShowModifySelectionCommand), Se.Language.Options.Shortcuts.ModifySelection },
        { nameof(MainViewModel.SelectAllLinesCommand), Se.Language.Options.Shortcuts.ListSelectAll },
        { nameof(MainViewModel.InverseSelectionCommand), Se.Language.Options.Shortcuts.ListInverseSelection },

        // Tools
        { nameof(MainViewModel.ShowToolsAdjustDurationsCommand), Se.Language.Options.Shortcuts.AdjustDurations },
        { nameof(MainViewModel.ShowApplyDurationLimitsCommand), Se.Language.Options.Shortcuts.ApplyDurationLimits },
        { nameof(MainViewModel.ShowToolsBatchConvertCommand), Se.Language.Options.Shortcuts.BatchConvert },
        { nameof(MainViewModel.ShowBridgeGapsCommand), Se.Language.Options.Shortcuts.BridgeGaps },
        { nameof(MainViewModel.ShowApplyMinGapCommand), Se.Language.Options.Shortcuts.ApplyMinGap },
        { nameof(MainViewModel.ShowToolsChangeCasingCommand), Se.Language.General.ChangeCasing },
        { nameof(MainViewModel.ShowToolsChangeFormattingCommand), Se.Language.General.ChangeFormatting },
        { nameof(MainViewModel.ShowToolsFixCommonErrorsCommand), Se.Language.Options.Shortcuts.FixCommonErrors },
        { nameof(MainViewModel.ToolsMakeEmptyTranslationFromCurrentSubtitleCommand), Se.Language.Options.Shortcuts.MakeEmptyTranslationFromCurrentSubtitle },
        { nameof(MainViewModel.ShowToolsMergeLinesWithSameTextCommand), Se.Language.Options.Shortcuts.MergeLinesWithSameText },
        { nameof(MainViewModel.ShowToolsMergeLinesWithSameTimeCodesCommand), Se.Language.Options.Shortcuts.MergeLinesWithSameTimeCodes },
        { nameof(MainViewModel.ShowToolsSplitBreakLongLinesCommand), Se.Language.Options.Shortcuts.SplitBreakLongLines },
        { nameof(MainViewModel.ShowToolsMergeShortLinesCommand), Se.Language.Options.Shortcuts.MergeShortLines },
        { nameof(MainViewModel.ShowToolsRemoveTextForHearingImpairedCommand), Se.Language.Options.Shortcuts.RemoveTextForHearingImpaired },
        { nameof(MainViewModel.ShowToolsJoinCommand), Se.Language.Options.Shortcuts.JoinSubtitles },
        { nameof(MainViewModel.ShowToolsSplitCommand), Se.Language.Options.Shortcuts.SplitSubtitle },

        // Spell check
        { nameof(MainViewModel.ShowSpellCheckCommand), Se.Language.Options.Shortcuts.SpellCheck },
        { nameof(MainViewModel.ShowSpellCheckDictionariesCommand), Se.Language.Options.Shortcuts.SpellCheckGetDictionary },

        // Video
        { nameof(MainViewModel.CommandVideoOpenCommand), Se.Language.Options.Shortcuts.OpenVideo },
        { nameof(MainViewModel.ShowVideoOpenFromUrlCommand), Se.Language.Options.Shortcuts.OpenVideoFromUrl },
        { nameof(MainViewModel.CommandVideoCloseCommand), Se.Language.Options.Shortcuts.CloseVideo },
        { nameof(MainViewModel.ShowSpeechToTextWhisperCommand), Se.Language.Options.Shortcuts.SpeechToText },
        { nameof(MainViewModel.ShowVideoTextToSpeechCommand), Se.Language.Options.Shortcuts.TextToSpeech },
        { nameof(MainViewModel.ShowVideoBurnInCommand), Se.Language.Options.Shortcuts.BurnIn },
        { nameof(MainViewModel.ShowVideoTransparentSubtitlesCommand), Se.Language.Options.Shortcuts.GenerateTransparent },
        { nameof(MainViewModel.ShowShotChangesSubtitlesCommand),  Se.Language.General.GenerateImportShotChanges },
        { nameof(MainViewModel.VideoFullScreenCommand),  Se.Language.Options.Shortcuts.VideoFullScreen },

        // Sync
        { nameof(MainViewModel.ShowSyncAdjustAllTimesCommand),  Se.Language.Options.Shortcuts.AdjustAllTimes },
        { nameof(MainViewModel.ShowVisualSyncCommand),  Se.Language.Options.Shortcuts.VisualSync },
        { nameof(MainViewModel.ShowSyncChangeFrameRateCommand),  Se.Language.General.ChangeFrameRate },
        { nameof(MainViewModel.ShowSyncChangeSpeedCommand),  Se.Language.General.ChangeSpeed },
        { nameof(MainViewModel.ShowPointSyncCommand),  Se.Language.Options.Shortcuts.ShowPointSync },
        { nameof(MainViewModel.ShowPointSyncViaOtherCommand),  Se.Language.Options.Shortcuts.ShowPointSyncViaOther },

        // Translate
        { nameof(MainViewModel.ShowAutoTranslateCommand), Se.Language.General.AutoTranslate },
        { nameof(MainViewModel.ShowTranslateViaCopyPasteCommand), Se.Language.Options.Shortcuts.TranslateViaCopyPaste },

        // Options
        { nameof(MainViewModel.CommandShowSettingsCommand), Se.Language.Options.Shortcuts.Settings },
        { nameof(MainViewModel.CommandShowSettingsShortcutsCommand), Se.Language.Options.Shortcuts.Shortcuts },
        { nameof(MainViewModel.ShowWordListsCommand), Se.Language.Options.Shortcuts.WordLists },
        { nameof(MainViewModel.CommandShowSettingsLanguageCommand), Se.Language.Options.Shortcuts.ChooseUiLanguage },


        { nameof(MainViewModel.ShowGoToVideoPositionCommand), Se.Language.Options.Shortcuts.GeneralGoToVideoPosition },
        { nameof(MainViewModel.ToggleLinesItalicOrSelectedTextCommand), Se.Language.Options.Shortcuts.GeneralToggleItalic },
        { nameof(MainViewModel.ToggleLinesBoldCommand), Se.Language.Options.Shortcuts.GeneralToggleBold },

        { nameof(MainViewModel.PlayCommand), Se.Language.General.Play },
        { nameof(MainViewModel.PlayNextCommand), Se.Language.General.PlayNext },
        { nameof(MainViewModel.PauseCommand), Se.Language.General.Pause },
        { nameof(MainViewModel.TogglePlayPauseCommand), Se.Language.Options.Shortcuts.TogglePlayPause },
        { nameof(MainViewModel.TogglePlayPause2Command), Se.Language.Options.Shortcuts.TogglePlayPause },

        { nameof(MainViewModel.CommandShowLayoutCommand), Se.Language.Options.Shortcuts.GeneralChooseLayout },

        { nameof(MainViewModel.GoToNextLineCommand), Se.Language.Options.Shortcuts.GeneralGoToNextSubtitle },
        { nameof(MainViewModel.GoToPreviousLineCommand), Se.Language.Options.Shortcuts.GeneralGoToPrevSubtitle },
        { nameof(MainViewModel.GoToNextLineAndSetVideoPositionCommand), Se.Language.Options.Shortcuts.GoToNextLineAndSetVideoPosition },
        { nameof(MainViewModel.GoToPreviousLineAndSetVideoPositionCommand), Se.Language.Options.Shortcuts.GoToPreviousLineAndSetVideoPosition },
        { nameof(MainViewModel.GoToPreviousLineFromVideoPositionCommand), Se.Language.Options.Shortcuts.GoToPreviousLineFromVideoPosition },
        { nameof(MainViewModel.GoToNextLineFromVideoPositionCommand), Se.Language.Options.Shortcuts.GoToNextLineFromVideoPosition },

        { nameof(MainViewModel.SaveLanguageFileCommand), Se.Language.Main.SaveLanguageFile },

        { nameof(MainViewModel.UnbreakCommand), Se.Language.General.Unbreak },
        { nameof(MainViewModel.AutoBreakCommand), Se.Language.General.AutoBreak },
        { nameof(MainViewModel.SplitCommand), Se.Language.General.SplitLine },
        { nameof(MainViewModel.SplitAtVideoPositionCommand), Se.Language.General.SplitLineAtVideoPosition },
        { nameof(MainViewModel.SplitAtTextBoxCursorPositionCommand), Se.Language.General.SplitAtTextBoxCursorPosition },
        { nameof(MainViewModel.SplitAtVideoPositionAndTextBoxCursorPositionCommand), Se.Language.General.SplitLineAtVideoAndTextBoxPosition },
        { nameof(MainViewModel.TextBoxDeleteSelectionCommand), Se.Language.Options.Shortcuts.TextBoxDeleteSelectionNoClipboard },
        { nameof(MainViewModel.TextBoxCutCommand), Se.Language.Options.Shortcuts.TextBoxCut },
        { nameof(MainViewModel.TextBoxCut2Command), Se.Language.Options.Shortcuts.TextBoxCut2 },
        { nameof(MainViewModel.TextBoxPasteCommand), Se.Language.Options.Shortcuts.TextBoxPaste },
        { nameof(MainViewModel.TextBoxCopyCommand), Se.Language.Options.Shortcuts.TextBoxCopy },
        { nameof(MainViewModel.TextBoxSelectAllCommand), Se.Language.Options.Shortcuts.TextBoxSelectAll },
        { nameof(MainViewModel.TextBoxRemoveAllFormattingCommand), Se.Language.Options.Shortcuts.TextBoxRemoveAllFormatting },
        { nameof(MainViewModel.TextBoxItalicCommand), Se.Language.Options.Shortcuts.TextBoxItalic },

        { nameof(MainViewModel.VideoOneFrameBackCommand), Se.Language.General.VideoOneFrameBack },
        { nameof(MainViewModel.VideoOneFrameForwardCommand),  Se.Language.General.VideoOneFrameForward },
        { nameof(MainViewModel.Video100MsBackCommand), Se.Language.General.Video100MsBack },
        { nameof(MainViewModel.Video100MsForwardCommand),  Se.Language.General.Video100MsForward },
        { nameof(MainViewModel.Video500MsBackCommand), Se.Language.General.Video500MsBack },
        { nameof(MainViewModel.Video500MsForwardCommand),  Se.Language.General.Video500MsForward },
        { nameof(MainViewModel.VideoOneSecondBackCommand), Se.Language.General.VideoOneSecondBack },
        { nameof(MainViewModel.VideoOneSecondForwardCommand),  Se.Language.General.VideoOneSecondForward },
        { nameof(MainViewModel.VideoMoveCustom1BackCommand),  string.Format(Se.Language.General.VideoCustom1BackX, Se.Settings.Video.MoveVideoPositionCustom1Back) },
        { nameof(MainViewModel.VideoMoveCustom1ForwardCommand),  string.Format(Se.Language.General.VideoCustom1ForwardX, Se.Settings.Video.MoveVideoPositionCustom1Forward) },
        { nameof(MainViewModel.VideoMoveCustom2BackCommand),  string.Format(Se.Language.General.VideoCustom2BackX, Se.Settings.Video.MoveVideoPositionCustom2Back) },
        { nameof(MainViewModel.VideoMoveCustom2ForwardCommand),  string.Format(Se.Language.General.VideoCustom2ForwardX, Se.Settings.Video.MoveVideoPositionCustom2Forward) },

        { nameof(MainViewModel.WaveformSetStartAndOffsetTheRestCommand),  Se.Language.General.SetStartAndOffsetTheRest },
        { nameof(MainViewModel.WaveformSetEndAndOffsetTheRestCommand),  Se.Language.General.SetEndAndOffsetTheRest },
        { nameof(MainViewModel.WaveformSetStartCommand),  Se.Language.General.SetStart },
        { nameof(MainViewModel.WaveformSetEndCommand),  Se.Language.General.SetEnd },
        { nameof(MainViewModel.WaveformSetEndAndGoToNextCommand),  Se.Language.General.SetEndAndGoToNext },
        { nameof(MainViewModel.DoWaveformCenterCommand),  Se.Language.General.WaveformCenterOnVideoPosition },
        { nameof(MainViewModel.ToggleShotChangesAtVideoPositionCommand),  Se.Language.General.ToggleShotChangesAtVideoPosition },
        { nameof(MainViewModel.GoToPreviousShotChangeCommand),  Se.Language.General.GoToPreviousShotChange },
        { nameof(MainViewModel.GoToNextShotChangeCommand),  Se.Language.General.GoToNextShotChange },
        { nameof(MainViewModel.ExtendSelectedLinesToNextShotChangeOrNextSubtitleCommand),  Se.Language.General.ExtendSelectedLinesToNextShotChangeOrNextSubtitle },
        { nameof(MainViewModel.SnapSelectedLinesToNearestShotChangeCommand),  Se.Language.General.SnapSelectedLinesToNearestShotChange },
        { nameof(MainViewModel.MoveAllShotChangeOneFrameBackCommand),  Se.Language.General.MoveAllShotChangeOneFrameBack },
        { nameof(MainViewModel.MoveAllShotChangeOneFrameForwardCommand),  Se.Language.General.MoveAllShotChangeOneFrameForward },
        { nameof(MainViewModel.ShowWaveformSeekSilenceCommand),  Se.Language.Options.Shortcuts.SeekSilence },
        { nameof(MainViewModel.ShowShotChangesListCommand),  Se.Language.General.ShowShotChangesList },
        { nameof(MainViewModel.VideoUndockControlsCommand),  Se.Language.Options.Shortcuts.UndockVideoControls },
        { nameof(MainViewModel.VideoRedockControlsCommand),  Se.Language.Options.Shortcuts.RedockVideoControls },
        { nameof(MainViewModel.VideoGenerateBlankCommand),  Se.Language.Options.Shortcuts.GenerateBlankVideo },
        { nameof(MainViewModel.VideoReEncodeCommand),  Se.Language.Options.Shortcuts.ReencodeVideo},
        { nameof(MainViewModel.VideoCutCommand),  Se.Language.Options.Shortcuts.CutVideo},
        { nameof(MainViewModel.CutVideoSelectedLinesCommand),  Se.Language.Options.Shortcuts.CutVideoSelectedLines},
        { nameof(MainViewModel.ResetWaveformZoomAndSpeedCommand),  Se.Language.Waveform.ResetWaveformZoomAndSpeed },
        { nameof(MainViewModel.ExtendSelectedToPreviousCommand),  Se.Language.General.ExtendSelectedToPrevious },
        { nameof(MainViewModel.ExtendSelectedToNextCommand),  Se.Language.General.ExtendSelectedToNext },
        { nameof(MainViewModel.ToggleLockTimeCodesCommand), Se.Language.Options.Shortcuts.ToggleLockTimeCodes },
        { nameof(MainViewModel.ShowHelpCommand), Se.Language.General.Help },
        { nameof(MainViewModel.ShowSourceViewCommand), Se.Language.Options.Shortcuts.SourceView },
        { nameof(MainViewModel.MergeWithLineBeforeCommand), Se.Language.General.MergeWithLineBeforeAndAutoBreak },
        { nameof(MainViewModel.MergeWithLineAfterCommand), Se.Language.General.MergeWithLineAfterAndAutoBreak },
        { nameof(MainViewModel.MergeWithLineBeforeKeepBreaksCommand), Se.Language.General.MergeWithLineBeforeKeepBreaks },
        { nameof(MainViewModel.MergeWithLineAfterKeepBreaksCommand), Se.Language.General.MergeWithLineAfterKeepBreaks },
        { nameof(MainViewModel.MergeSelectedLinesCommand), Se.Language.General.MergeSelectedLines },
        { nameof(MainViewModel.MergeSelectedLinesDialogCommand), Se.Language.General.MergeSelectedLinesDialog },
        { nameof(MainViewModel.ShowColorPickerCommand), Se.Language.General.ChooseColorDotDotDot },
        { nameof(MainViewModel.FetchFirstWordFromNextSubtitleCommand), Se.Language.Options.Shortcuts.FetchFirstWordFromNextSubtitle },
        { nameof(MainViewModel.WaveformSetEndAndStartOfNextAfterGapCommand), Se.Language.Options.Shortcuts.WaveformSetEndAndStartOfNextAfterGap },
        { nameof(MainViewModel.WaveformSetEndAndStartOfNextAfterGapAndGoToNextCommand), Se.Language.Options.Shortcuts.WaveformSetEndAndStartOfNextAfterGapAndGoToNext },
        { nameof(MainViewModel.WaveformSetStartAndSetEndOfPreviousMinusGapCommand), Se.Language.Options.Shortcuts.WaveformSetStartAndSetEndOfPreviousMinusGap },
        { nameof(MainViewModel.WaveformHorizontalZoomInCommand), Se.Language.Options.Shortcuts.WaveformHorizontalZoomInCommand },
        { nameof(MainViewModel.WaveformHorizontalZoomOutCommand), Se.Language.Options.Shortcuts.WaveformHorizontalZoomOutCommand },
        { nameof(MainViewModel.WaveformVerticalZoomInCommand), Se.Language.Options.Shortcuts.WaveformVerticalZoomInCommand },
        { nameof(MainViewModel.WaveformVerticalZoomOutCommand), Se.Language.Options.Shortcuts.WaveformVerticalZoomOutCommand },
        { nameof(MainViewModel.MoveLastWordToNextSubtitleCommand), Se.Language.Options.Shortcuts.MoveLastWordToNextSubtitle },
        { nameof(MainViewModel.MoveLastWordFromFirstLineDownCurrentSubtitleCommand), Se.Language.Options.Shortcuts.MoveLastWordFromFirstLineDownCurrentSubtitle },
        { nameof(MainViewModel.MoveFirstWordFromNextLineUpCurrentSubtitleCommand), Se.Language.Options.Shortcuts.MoveFirstWordFromNextLineUpCurrentSubtitle },
        { nameof(MainViewModel.ToggleFocusGridAndWaveformCommand), Se.Language.Options.Shortcuts.ToggleFocusGridAndWaveform },
        { nameof(MainViewModel.ToggleFocusTextBoxAndWaveformCommand), Se.Language.Options.Shortcuts.ToggleFocusTextBoxAndWaveform },
        { nameof(MainViewModel.ToggleFocusTextBoxAndSubtitleGridCommand), Se.Language.Options.Shortcuts.ToggleFocusTextBoxAndGrid },
        { nameof(MainViewModel.SubtitleGridCutCommand), Se.Language.Options.Shortcuts.SubtitleGridCut },
        { nameof(MainViewModel.SubtitleGridCopyCommand), Se.Language.Options.Shortcuts.SubtitleGridCopy },
        { nameof(MainViewModel.SubtitleGridPasteCommand), Se.Language.Options.Shortcuts.SubtitleGridPaste },
        { nameof(MainViewModel.ShowChooseProfileCommand), Se.Language.Options.Shortcuts.ChooseRuleProfile },
        { nameof(MainViewModel.TogglePlaybackSpeedCommand), Se.Language.Options.Shortcuts.TogglePlaybackSpeed },
        { nameof(MainViewModel.PlaybackSlowerCommand), Se.Language.Options.Shortcuts.PlaybackSpeedSlower },
        { nameof(MainViewModel.PlaybackFasterCommand), Se.Language.Options.Shortcuts.PlaybackSpeedFaster },
        { nameof(MainViewModel.VideoSetPositionCurrentSubtitleStartCommand), Se.Language.Options.Shortcuts.SetVideoPositionCurrentSubtitleStart },
        { nameof(MainViewModel.VideoSetPositionCurrentSubtitleEndCommand), Se.Language.Options.Shortcuts.SetVideoPositionCurrentSubtitleEnd },
        { nameof(MainViewModel.ToggleAudioTracksCommand), Se.Language.Options.Shortcuts.ToggleAudioTracks },
        { nameof(MainViewModel.ListErrorsCommand), Se.Language.Options.Shortcuts.ListErrors },
        { nameof(MainViewModel.GoToPreviousErrorCommand), Se.Language.Options.Shortcuts.GoToPreviousError },
        { nameof(MainViewModel.GoToNextErrorCommand), Se.Language.Options.Shortcuts.GoToNextError },
        { nameof(MainViewModel.ShowAddToNameListCommand), Se.Language.Options.Shortcuts.AddNameToNameList },
        { nameof(MainViewModel.ShowFindDoubleWordsCommand), Se.Language.Options.Shortcuts.FindDoubleWords },
        { nameof(MainViewModel.SetColor1Command), string.Format(Se.Language.Options.Shortcuts.ColorX, "1") },
        { nameof(MainViewModel.SetColor2Command), string.Format(Se.Language.Options.Shortcuts.ColorX, "2") },
        { nameof(MainViewModel.SetColor3Command), string.Format(Se.Language.Options.Shortcuts.ColorX, "3") },
        { nameof(MainViewModel.SetColor4Command), string.Format(Se.Language.Options.Shortcuts.ColorX, "4") },
        { nameof(MainViewModel.SetColor5Command), string.Format(Se.Language.Options.Shortcuts.ColorX, "5") },
        { nameof(MainViewModel.SetColor6Command), string.Format(Se.Language.Options.Shortcuts.ColorX, "6") },
        { nameof(MainViewModel.SetColor7Command), string.Format(Se.Language.Options.Shortcuts.ColorX, "7") },
        { nameof(MainViewModel.SetColor8Command), string.Format(Se.Language.Options.Shortcuts.ColorX, "8") },
        { nameof(MainViewModel.RemoveColorCommand), Se.Language.Options.Shortcuts.RemoveColor },
        { nameof(MainViewModel.SurroundWith1Command), string.Format(Se.Language.Options.Shortcuts.SurroundWithXY,  Se.Settings.Surround1Left, Se.Settings.Surround1Right) },
        { nameof(MainViewModel.SurroundWith2Command), string.Format(Se.Language.Options.Shortcuts.SurroundWithXY,  Se.Settings.Surround2Left, Se.Settings.Surround2Right) },
        { nameof(MainViewModel.SurroundWith3Command), string.Format(Se.Language.Options.Shortcuts.SurroundWithXY,  Se.Settings.Surround3Left, Se.Settings.Surround3Right) },
        { nameof(MainViewModel.RepeatPreviousLineCommand), Se.Language.Options.Shortcuts.RepeatPreviousLine },
        { nameof(MainViewModel.RepeatNextLineCommand), Se.Language.Options.Shortcuts.RepeatNextLine },
        { nameof(MainViewModel.InsertLineBeforeCommand), Se.Language.General.InsertBefore },
        { nameof(MainViewModel.InsertLineAfterCommand), Se.Language.General.InsertAfter },
        { nameof(MainViewModel.WaveformInsertAtPositionAndFocusTextBoxCommand), Se.Language.General.InsertAtPositionAndFocusTextBox },
        { nameof(MainViewModel.WaveformInsertAtPositionNoFocusTextBoxCommand), Se.Language.General.InsertAtPositionNoFocusTextBox },
        { nameof(MainViewModel.WaveformPasteFromClipboardCommand), Se.Language.General.WaveformPasteFromClipboard },
        { nameof(MainViewModel.FocusSelectedLineCommand), Se.Language.General.FocusSelectedLine },
        { nameof(MainViewModel.PlayFromStartOfVideoCommand), Se.Language.General.PlayFromStartOfVideo },
        { nameof(MainViewModel.RemoveBlankLinesCommand), Se.Language.General.RemoveBlankLines },
        { nameof(MainViewModel.InsertSubtitleAtVideoPositionSetEndAtKeyUpCommand), Se.Language.General.NewSubtitleStartKeyDownSetEndKeyUp },
        { nameof(MainViewModel.SpeechToTextSelectedLinesCommand), Se.Language.General.SpeechToTextSelectedLines },
        { nameof(MainViewModel.SpeechToTextSelectedLinesPromptForLangaugeCommand), Se.Language.General.SpeechToTextSelectedLinesPrompt },
        { nameof(MainViewModel.SpeechToTextSelectedLinesPromptForLangaugeFirstTimeCommand), Se.Language.General.SpeechToTextSelectedLinesPromptFirstTime },
        { nameof(MainViewModel.PlaySelectedLinesWithoutLoopCommand), Se.Language.General.PlaySelectedLines },
        { nameof(MainViewModel.PlaySelectedLinesWithLoopCommand), Se.Language.General.PlaySelectedLinesWithLoop },
        { nameof(MainViewModel.ToggleCasingCommand), Se.Language.General.ToggleCasing },
        { nameof(MainViewModel.ImportImageSubtitleForEditCommand), Se.Language.Options.Shortcuts.ImportImageSubtitleForEdit },
        { nameof(MainViewModel.ShowMediaInformationCommand), Se.Language.Options.Shortcuts.ShowMediaInformation },
        { nameof(MainViewModel.ShowSubtitleFormatPickerCommand), Se.Language.Options.Shortcuts.ChooseSubtitleFormat },
        { nameof(MainViewModel.TrimWhitespaceSelectedLinesCommand), Se.Language.Options.Shortcuts.TrimWhitespaceSelectedLines },
        { nameof(MainViewModel.FocusTextBoxCommand), Se.Language.Options.Shortcuts.FocusTextBox },
        { nameof(MainViewModel.SortByStartTimeCommand), Se.Language.Options.Shortcuts.SortByStartTime },
        { nameof(MainViewModel.SortByEndTimeCommand), Se.Language.Options.Shortcuts.SortByEndTime },
        { nameof(MainViewModel.ShowPickLayerFilterCommand), Se.Language.General.FilterByLayer },
        { nameof(MainViewModel.ShowPickLayerCommand), Se.Language.General.PickLayer },
        { nameof(MainViewModel.CopyTextToClipboardCommand), Se.Language.Options.Shortcuts.CopyTextToClipboard },
        { nameof(MainViewModel.CopyTextFromOriginalToClipboardCommand), Se.Language.Options.Shortcuts.CopyTextFromOriginalToClipboard },
        { nameof(MainViewModel.ShowAssaImageColorPickerCommand), Se.Language.Assa.ImageColorPicker },
        { nameof(MainViewModel.ShowAssaSetPositionCommand), Se.Language.Assa.SetPosition },
        { nameof(MainViewModel.ShowAssaApplyCustomOverrideTagsCommand), Se.Language.Assa.ApplyOverrideTags },
        { nameof(MainViewModel.ShowAssaDrawCommand), Se.Language.Options.Shortcuts.AssaDraw },
        { nameof(MainViewModel.ShowAssaGenerateProgressBarCommand), Se.Language.Options.Shortcuts.AssaGenerateProgressBar },
        { nameof(MainViewModel.ShowAssaGenerateBackgroundCommand), Se.Language.Options.Shortcuts.AssaGenerateBackgroundBox },
        { nameof(MainViewModel.ShowAssaStylesCommand), Se.Language.Options.Shortcuts.AssaStyles },
        { nameof(MainViewModel.ShowAssaPropertiesCommand), Se.Language.Options.Shortcuts.AssaProperties },
        { nameof(MainViewModel.ShowAssaAttachmentsCommand), Se.Language.Options.Shortcuts.AssaAttachments },
        { nameof(MainViewModel.RecalculateDurationSelectedLinesCommand), Se.Language.Options.Shortcuts.RecalculateDurationSelectedLines },
        { nameof(MainViewModel.WaveformToggleWaveformSpectrogramHeightCommand), Se.Language.Options.Shortcuts.ToggleWaveformAndSpectrogramHeight },
        { nameof(MainViewModel.SpectrogramToggleStyleCommand), Se.Language.Options.Shortcuts.ToggleSpectrogramStyle },
        { nameof(MainViewModel.ShowBeautifyTimeCodesCommand), Se.Language.Tools.BeautifyTimeCodes.Title },
    };

    private static List<AvailableShortcut> GetAllAvailableShortcuts(MainViewModel vm)
    {
        var shortcuts = new List<AvailableShortcut>();

        AddShortcut(shortcuts, vm.SelectAllLinesCommand, nameof(vm.SelectAllLinesCommand), ShortcutCategory.SubtitleGrid);
        AddShortcut(shortcuts, vm.InverseSelectionCommand, nameof(vm.InverseSelectionCommand), ShortcutCategory.SubtitleGrid);
        AddShortcut(shortcuts, vm.DeleteSelectedLinesCommand, nameof(vm.DeleteSelectedLinesCommand), ShortcutCategory.SubtitleGrid);
        AddShortcut(shortcuts, vm.RippleDeleteSelectedLinesCommand, nameof(vm.RippleDeleteSelectedLinesCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.DuplicateSelectedLinesCommand, nameof(vm.DuplicateSelectedLinesCommand), ShortcutCategory.SubtitleGrid);
        AddShortcut(shortcuts, vm.ShowAlignmentPickerCommand, nameof(vm.ShowAlignmentPickerCommand), ShortcutCategory.SubtitleGrid);
        AddShortcut(shortcuts, vm.DoAlignmentAn1Command, nameof(vm.DoAlignmentAn1Command), ShortcutCategory.SubtitleGrid);
        AddShortcut(shortcuts, vm.DoAlignmentAn2Command, nameof(vm.DoAlignmentAn2Command), ShortcutCategory.SubtitleGrid);
        AddShortcut(shortcuts, vm.DoAlignmentAn3Command, nameof(vm.DoAlignmentAn3Command), ShortcutCategory.SubtitleGrid);
        AddShortcut(shortcuts, vm.DoAlignmentAn4Command, nameof(vm.DoAlignmentAn4Command), ShortcutCategory.SubtitleGrid);
        AddShortcut(shortcuts, vm.DoAlignmentAn5Command, nameof(vm.DoAlignmentAn5Command), ShortcutCategory.SubtitleGrid);
        AddShortcut(shortcuts, vm.DoAlignmentAn6Command, nameof(vm.DoAlignmentAn6Command), ShortcutCategory.SubtitleGrid);
        AddShortcut(shortcuts, vm.DoAlignmentAn7Command, nameof(vm.DoAlignmentAn7Command), ShortcutCategory.SubtitleGrid);
        AddShortcut(shortcuts, vm.DoAlignmentAn8Command, nameof(vm.DoAlignmentAn8Command), ShortcutCategory.SubtitleGrid);
        AddShortcut(shortcuts, vm.DoAlignmentAn9Command, nameof(vm.DoAlignmentAn9Command), ShortcutCategory.SubtitleGrid);
        AddShortcut(shortcuts, vm.AddOrEditBookmarkCommand, nameof(vm.AddOrEditBookmarkCommand), ShortcutCategory.SubtitleGrid);
        AddShortcut(shortcuts, vm.ToggleBookmarkSelectedLinesNoTextCommand, nameof(vm.ToggleBookmarkSelectedLinesNoTextCommand), ShortcutCategory.SubtitleGrid);
        AddShortcut(shortcuts, vm.CopyTextFromOriginalToTranslationCommand, nameof(vm.CopyTextFromOriginalToTranslationCommand), ShortcutCategory.SubtitleGrid);
        AddShortcut(shortcuts, vm.SwitchOriginalAndTranslationTextSelectedLinesCommand, nameof(vm.SwitchOriginalAndTranslationTextSelectedLinesCommand), ShortcutCategory.SubtitleGrid);
        AddShortcut(shortcuts, vm.MergeOriginalIntoTranslationSelectedLinesCommand, nameof(vm.MergeOriginalIntoTranslationSelectedLinesCommand), ShortcutCategory.SubtitleGrid);
        AddShortcut(shortcuts, vm.ListBookmarksCommand, nameof(vm.ListBookmarksCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.GoToNextBookmarkCommand, nameof(vm.GoToNextBookmarkCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.OpenDataFolderCommand, nameof(vm.OpenDataFolderCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ToggleIsWaveformToolbarVisibleCommand, nameof(vm.ToggleIsWaveformToolbarVisibleCommand), ShortcutCategory.General);

        // File
        AddShortcut(shortcuts, vm.CommandFileOpenCommand, nameof(vm.CommandFileOpenCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.CommandExitCommand, nameof(vm.CommandExitCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.CommandFileNewCommand, nameof(vm.CommandFileNewCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.CommandFileNewKeepVideoCommand, nameof(vm.CommandFileNewKeepVideoCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.CommandFileSaveCommand, nameof(vm.CommandFileSaveCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.CommandFileSaveAsCommand, nameof(vm.CommandFileSaveAsCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowStatisticsCommand, nameof(vm.ShowStatisticsCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowCompareCommand, nameof(vm.ShowCompareCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowRestoreAutoBackupCommand, nameof(vm.ShowRestoreAutoBackupCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.OpenContainingFolderCommand, nameof(vm.OpenContainingFolderCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.CopySubtitlePathToClipboardCommand, nameof(vm.CopySubtitlePathToClipboardCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.CopySubtitleOriginalPathToClipboardCommand, nameof(vm.CopySubtitleOriginalPathToClipboardCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ImportTimeCodesCommand, nameof(vm.ImportTimeCodesCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowImportSubtitleWithManuallyChosenEncodingCommand, nameof(vm.ShowImportSubtitleWithManuallyChosenEncodingCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.CommandFileOpenKeepVideoCommand, nameof(vm.CommandFileOpenKeepVideoCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.FileOpenOriginalCommand, nameof(vm.FileOpenOriginalCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.FileCloseOriginalCommand, nameof(vm.FileCloseOriginalCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ExportBluRaySupCommand, nameof(vm.ExportBluRaySupCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowExportCustomTextFormatCommand, nameof(vm.ShowExportCustomTextFormatCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowExportPlainTextCommand, nameof(vm.ShowExportPlainTextCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.CopyMsRelativeToCurrentSubtitleLineToClipboardCommand, nameof(vm.CopyMsRelativeToCurrentSubtitleLineToClipboardCommand), ShortcutCategory.General);

        AddShortcut(shortcuts, vm.UndoCommand, nameof(vm.UndoCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.RedoCommand, nameof(vm.RedoCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowHistoryCommand, nameof(vm.ShowHistoryCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowFindCommand, nameof(vm.ShowFindCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.FindNextCommand, nameof(vm.FindNextCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.FindPreviousCommand, nameof(vm.FindPreviousCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowReplaceCommand, nameof(vm.ShowReplaceCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowMultipleReplaceCommand, nameof(vm.ShowMultipleReplaceCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowGoToLineCommand, nameof(vm.ShowGoToLineCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.RightToLeftToggleCommand, nameof(vm.RightToLeftToggleCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ReverseRightToLeftStartEndCommand, nameof(vm.ReverseRightToLeftStartEndCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowModifySelectionCommand, nameof(vm.ShowModifySelectionCommand), ShortcutCategory.General);

        AddShortcut(shortcuts, vm.ShowGoToVideoPositionCommand, nameof(vm.ShowGoToVideoPositionCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ToggleLinesItalicOrSelectedTextCommand, nameof(vm.ToggleLinesItalicOrSelectedTextCommand), ShortcutCategory.SubtitleGridAndTextBox);
        AddShortcut(shortcuts, vm.ToggleLinesBoldCommand, nameof(vm.ToggleLinesBoldCommand), ShortcutCategory.SubtitleGridAndTextBox);

        AddShortcut(shortcuts, vm.PlayCommand, nameof(vm.PlayCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.PlayNextCommand, nameof(vm.PlayNextCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.PauseCommand, nameof(vm.PauseCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.TogglePlayPauseCommand, nameof(vm.TogglePlayPauseCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.TogglePlayPause2Command, nameof(vm.TogglePlayPause2Command), ShortcutCategory.General);

        AddShortcut(shortcuts, vm.CommandShowLayoutCommand, nameof(vm.CommandShowLayoutCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowAutoTranslateCommand, nameof(vm.ShowAutoTranslateCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowTranslateViaCopyPasteCommand, nameof(vm.ShowTranslateViaCopyPasteCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.CommandShowSettingsCommand, nameof(vm.CommandShowSettingsCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.CommandShowSettingsShortcutsCommand, nameof(vm.CommandShowSettingsShortcutsCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowWordListsCommand, nameof(vm.ShowWordListsCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.CommandShowSettingsLanguageCommand, nameof(vm.CommandShowSettingsLanguageCommand), ShortcutCategory.General);

        AddShortcut(shortcuts, vm.GoToPreviousLineCommand, nameof(vm.GoToPreviousLineCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.GoToNextLineCommand, nameof(vm.GoToNextLineCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.GoToPreviousLineAndSetVideoPositionCommand, nameof(vm.GoToPreviousLineAndSetVideoPositionCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.GoToNextLineAndSetVideoPositionCommand, nameof(vm.GoToNextLineAndSetVideoPositionCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.GoToPreviousLineFromVideoPositionCommand, nameof(vm.GoToPreviousLineFromVideoPositionCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.GoToNextLineFromVideoPositionCommand, nameof(vm.GoToNextLineFromVideoPositionCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.SaveLanguageFileCommand, nameof(vm.SaveLanguageFileCommand), ShortcutCategory.General);

        AddShortcut(shortcuts, vm.UnbreakCommand, nameof(vm.UnbreakCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.AutoBreakCommand, nameof(vm.AutoBreakCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.SplitCommand, nameof(vm.SplitCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.SplitAtVideoPositionCommand, nameof(vm.SplitAtVideoPositionCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.SplitAtTextBoxCursorPositionCommand, nameof(vm.SplitAtTextBoxCursorPositionCommand), ShortcutCategory.TextBox);
        AddShortcut(shortcuts, vm.SplitAtVideoPositionAndTextBoxCursorPositionCommand, nameof(vm.SplitAtVideoPositionAndTextBoxCursorPositionCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.TextBoxDeleteSelectionCommand, nameof(vm.TextBoxDeleteSelectionCommand), ShortcutCategory.TextBox);
        AddShortcut(shortcuts, vm.TextBoxCutCommand, nameof(vm.TextBoxCutCommand), ShortcutCategory.TextBox);
        AddShortcut(shortcuts, vm.TextBoxCut2Command, nameof(vm.TextBoxCut2Command), ShortcutCategory.TextBox);
        AddShortcut(shortcuts, vm.TextBoxPasteCommand, nameof(vm.TextBoxPasteCommand), ShortcutCategory.TextBox);
        AddShortcut(shortcuts, vm.TextBoxCopyCommand, nameof(vm.TextBoxCopyCommand), ShortcutCategory.TextBox);
        AddShortcut(shortcuts, vm.TextBoxSelectAllCommand, nameof(vm.TextBoxSelectAllCommand), ShortcutCategory.TextBox);
        AddShortcut(shortcuts, vm.TextBoxRemoveAllFormattingCommand, nameof(vm.TextBoxRemoveAllFormattingCommand), ShortcutCategory.TextBox);
        AddShortcut(shortcuts, vm.TextBoxItalicCommand, nameof(vm.TextBoxItalicCommand), ShortcutCategory.TextBox);

        // Tools
        AddShortcut(shortcuts, vm.ShowBridgeGapsCommand, nameof(vm.ShowBridgeGapsCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowToolsAdjustDurationsCommand, nameof(vm.ShowToolsAdjustDurationsCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowApplyDurationLimitsCommand, nameof(vm.ShowApplyDurationLimitsCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowToolsBatchConvertCommand, nameof(vm.ShowToolsBatchConvertCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowApplyMinGapCommand, nameof(vm.ShowApplyMinGapCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowToolsChangeCasingCommand, nameof(vm.ShowToolsChangeCasingCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowToolsChangeFormattingCommand, nameof(vm.ShowToolsChangeFormattingCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowToolsFixCommonErrorsCommand, nameof(vm.ShowToolsFixCommonErrorsCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ToolsMakeEmptyTranslationFromCurrentSubtitleCommand, nameof(vm.ToolsMakeEmptyTranslationFromCurrentSubtitleCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowToolsMergeLinesWithSameTextCommand, nameof(vm.ShowToolsMergeLinesWithSameTextCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowToolsMergeLinesWithSameTimeCodesCommand, nameof(vm.ShowToolsMergeLinesWithSameTimeCodesCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowToolsSplitBreakLongLinesCommand, nameof(vm.ShowToolsSplitBreakLongLinesCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowToolsMergeShortLinesCommand, nameof(vm.ShowToolsMergeShortLinesCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowToolsRemoveTextForHearingImpairedCommand, nameof(vm.ShowToolsRemoveTextForHearingImpairedCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowToolsJoinCommand, nameof(vm.ShowToolsJoinCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowToolsSplitCommand, nameof(vm.ShowToolsSplitCommand), ShortcutCategory.General);

        // Spell check
        AddShortcut(shortcuts, vm.ShowSpellCheckCommand, nameof(vm.ShowSpellCheckCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowSpellCheckDictionariesCommand, nameof(vm.ShowSpellCheckDictionariesCommand), ShortcutCategory.General);

        // Video
        AddShortcut(shortcuts, vm.CommandVideoOpenCommand, nameof(vm.CommandVideoOpenCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowVideoOpenFromUrlCommand, nameof(vm.ShowVideoOpenFromUrlCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.CommandVideoCloseCommand, nameof(vm.CommandVideoCloseCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowSpeechToTextWhisperCommand, nameof(vm.ShowSpeechToTextWhisperCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowVideoTextToSpeechCommand, nameof(vm.ShowVideoTextToSpeechCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowVideoBurnInCommand, nameof(vm.ShowVideoBurnInCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowVideoTransparentSubtitlesCommand, nameof(vm.ShowVideoTransparentSubtitlesCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowShotChangesSubtitlesCommand, nameof(vm.ShowShotChangesSubtitlesCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowShotChangesListCommand, nameof(vm.ShowShotChangesListCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.VideoUndockControlsCommand, nameof(vm.VideoUndockControlsCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.VideoRedockControlsCommand, nameof(vm.VideoRedockControlsCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.VideoGenerateBlankCommand, nameof(vm.VideoGenerateBlankCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.VideoReEncodeCommand, nameof(vm.VideoReEncodeCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.VideoCutCommand, nameof(vm.VideoCutCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.CutVideoSelectedLinesCommand, nameof(vm.CutVideoSelectedLinesCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.VideoFullScreenCommand, nameof(vm.VideoFullScreenCommand), ShortcutCategory.General);

        AddShortcut(shortcuts, vm.ShowSyncAdjustAllTimesCommand, nameof(vm.ShowSyncAdjustAllTimesCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowVisualSyncCommand, nameof(vm.ShowVisualSyncCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowSyncChangeFrameRateCommand, nameof(vm.ShowSyncChangeFrameRateCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowSyncChangeSpeedCommand, nameof(vm.ShowSyncChangeSpeedCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowPointSyncCommand, nameof(vm.ShowPointSyncCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowPointSyncViaOtherCommand, nameof(vm.ShowPointSyncViaOtherCommand), ShortcutCategory.General);

        AddShortcut(shortcuts, vm.VideoOneFrameBackCommand, nameof(vm.VideoOneFrameBackCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.VideoOneFrameForwardCommand, nameof(vm.VideoOneFrameForwardCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.Video100MsBackCommand, nameof(vm.Video100MsBackCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.Video100MsForwardCommand, nameof(vm.Video100MsForwardCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.Video500MsBackCommand, nameof(vm.Video500MsBackCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.Video500MsForwardCommand, nameof(vm.Video500MsForwardCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.VideoOneSecondBackCommand, nameof(vm.VideoOneSecondBackCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.VideoOneSecondForwardCommand, nameof(vm.VideoOneSecondForwardCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.VideoMoveCustom1BackCommand, nameof(vm.VideoMoveCustom1BackCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.VideoMoveCustom1ForwardCommand, nameof(vm.VideoMoveCustom1ForwardCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.VideoMoveCustom2BackCommand, nameof(vm.VideoMoveCustom2BackCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.VideoMoveCustom2ForwardCommand, nameof(vm.VideoMoveCustom2ForwardCommand), ShortcutCategory.General);

        AddShortcut(shortcuts, vm.WaveformSetStartAndOffsetTheRestCommand, nameof(vm.WaveformSetStartAndOffsetTheRestCommand), ShortcutCategory.Waveform);
        AddShortcut(shortcuts, vm.WaveformSetEndAndOffsetTheRestCommand, nameof(vm.WaveformSetEndAndOffsetTheRestCommand), ShortcutCategory.Waveform);
        AddShortcut(shortcuts, vm.WaveformSetStartCommand, nameof(vm.WaveformSetStartCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.WaveformSetEndCommand, nameof(vm.WaveformSetEndCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.WaveformSetEndAndGoToNextCommand, nameof(vm.WaveformSetEndAndGoToNextCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.DoWaveformCenterCommand, nameof(vm.DoWaveformCenterCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.WaveformSetEndAndStartOfNextAfterGapCommand, nameof(vm.WaveformSetEndAndStartOfNextAfterGapCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.WaveformSetEndAndStartOfNextAfterGapAndGoToNextCommand, nameof(vm.WaveformSetEndAndStartOfNextAfterGapAndGoToNextCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.WaveformSetStartAndSetEndOfPreviousMinusGapCommand, nameof(vm.WaveformSetStartAndSetEndOfPreviousMinusGapCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.WaveformHorizontalZoomInCommand, nameof(vm.WaveformHorizontalZoomInCommand), ShortcutCategory.Waveform);
        AddShortcut(shortcuts, vm.WaveformHorizontalZoomOutCommand, nameof(vm.WaveformHorizontalZoomOutCommand), ShortcutCategory.Waveform);
        AddShortcut(shortcuts, vm.WaveformVerticalZoomInCommand, nameof(vm.WaveformVerticalZoomInCommand), ShortcutCategory.Waveform);
        AddShortcut(shortcuts, vm.WaveformVerticalZoomOutCommand, nameof(vm.WaveformVerticalZoomOutCommand), ShortcutCategory.Waveform);
        AddShortcut(shortcuts, vm.ToggleShotChangesAtVideoPositionCommand, nameof(vm.ToggleShotChangesAtVideoPositionCommand), ShortcutCategory.Waveform);
        AddShortcut(shortcuts, vm.ShowWaveformSeekSilenceCommand, nameof(vm.ShowWaveformSeekSilenceCommand), ShortcutCategory.Waveform);
        AddShortcut(shortcuts, vm.GoToPreviousShotChangeCommand, nameof(vm.GoToPreviousShotChangeCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.GoToNextShotChangeCommand, nameof(vm.GoToNextShotChangeCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ExtendSelectedLinesToNextShotChangeOrNextSubtitleCommand, nameof(vm.ExtendSelectedLinesToNextShotChangeOrNextSubtitleCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.SnapSelectedLinesToNearestShotChangeCommand, nameof(vm.SnapSelectedLinesToNearestShotChangeCommand), ShortcutCategory.General);   
        AddShortcut(shortcuts, vm.MoveAllShotChangeOneFrameBackCommand, nameof(vm.MoveAllShotChangeOneFrameBackCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.MoveAllShotChangeOneFrameForwardCommand, nameof(vm.MoveAllShotChangeOneFrameForwardCommand), ShortcutCategory.General);   

        AddShortcut(shortcuts, vm.ResetWaveformZoomAndSpeedCommand, nameof(vm.ResetWaveformZoomAndSpeedCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ExtendSelectedToPreviousCommand, nameof(vm.ExtendSelectedToPreviousCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ExtendSelectedToNextCommand, nameof(vm.ExtendSelectedToNextCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ToggleLockTimeCodesCommand, nameof(vm.ToggleLockTimeCodesCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowHelpCommand, nameof(vm.ShowHelpCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowSourceViewCommand, nameof(vm.ShowSourceViewCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.MergeWithLineBeforeCommand, nameof(vm.MergeWithLineBeforeCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.MergeWithLineAfterCommand, nameof(vm.MergeWithLineAfterCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.MergeSelectedLinesCommand, nameof(vm.MergeSelectedLinesCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.MergeWithLineBeforeKeepBreaksCommand, nameof(vm.MergeWithLineBeforeKeepBreaksCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.MergeWithLineAfterKeepBreaksCommand, nameof(vm.MergeWithLineAfterKeepBreaksCommand), ShortcutCategory.General);

        AddShortcut(shortcuts, vm.MergeSelectedLinesDialogCommand, nameof(vm.MergeSelectedLinesDialogCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowColorPickerCommand, nameof(vm.ShowColorPickerCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.FetchFirstWordFromNextSubtitleCommand, nameof(vm.FetchFirstWordFromNextSubtitleCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.MoveLastWordToNextSubtitleCommand, nameof(vm.MoveLastWordToNextSubtitleCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.MoveLastWordFromFirstLineDownCurrentSubtitleCommand, nameof(vm.MoveLastWordFromFirstLineDownCurrentSubtitleCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.MoveFirstWordFromNextLineUpCurrentSubtitleCommand, nameof(vm.MoveFirstWordFromNextLineUpCurrentSubtitleCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ToggleFocusGridAndWaveformCommand, nameof(vm.ToggleFocusGridAndWaveformCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ToggleFocusTextBoxAndWaveformCommand, nameof(vm.ToggleFocusTextBoxAndWaveformCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ToggleFocusTextBoxAndSubtitleGridCommand, nameof(vm.ToggleFocusTextBoxAndSubtitleGridCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.SubtitleGridCutCommand, nameof(vm.SubtitleGridCutCommand), ShortcutCategory.SubtitleGrid);
        AddShortcut(shortcuts, vm.SubtitleGridCopyCommand, nameof(vm.SubtitleGridCopyCommand), ShortcutCategory.SubtitleGrid);
        AddShortcut(shortcuts, vm.SubtitleGridPasteCommand, nameof(vm.SubtitleGridPasteCommand), ShortcutCategory.SubtitleGrid);
        AddShortcut(shortcuts, vm.ShowChooseProfileCommand, nameof(vm.ShowChooseProfileCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.TogglePlaybackSpeedCommand, nameof(vm.TogglePlaybackSpeedCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.PlaybackSlowerCommand, nameof(vm.PlaybackSlowerCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.PlaybackFasterCommand, nameof(vm.PlaybackFasterCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.VideoSetPositionCurrentSubtitleStartCommand, nameof(vm.VideoSetPositionCurrentSubtitleStartCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.VideoSetPositionCurrentSubtitleEndCommand, nameof(vm.VideoSetPositionCurrentSubtitleEndCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ToggleAudioTracksCommand, nameof(vm.ToggleAudioTracksCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ListErrorsCommand, nameof(vm.ListErrorsCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.GoToPreviousErrorCommand, nameof(vm.GoToPreviousErrorCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.GoToNextErrorCommand, nameof(vm.GoToNextErrorCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowAddToNameListCommand, nameof(vm.ShowAddToNameListCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowFindDoubleWordsCommand, nameof(vm.ShowFindDoubleWordsCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.SetColor1Command, nameof(vm.SetColor1Command), ShortcutCategory.SubtitleGridAndTextBox);
        AddShortcut(shortcuts, vm.SetColor2Command, nameof(vm.SetColor2Command), ShortcutCategory.SubtitleGridAndTextBox);
        AddShortcut(shortcuts, vm.SetColor3Command, nameof(vm.SetColor3Command), ShortcutCategory.SubtitleGridAndTextBox);
        AddShortcut(shortcuts, vm.SetColor4Command, nameof(vm.SetColor4Command), ShortcutCategory.SubtitleGridAndTextBox);
        AddShortcut(shortcuts, vm.SetColor5Command, nameof(vm.SetColor5Command), ShortcutCategory.SubtitleGridAndTextBox);
        AddShortcut(shortcuts, vm.SetColor6Command, nameof(vm.SetColor6Command), ShortcutCategory.SubtitleGridAndTextBox);
        AddShortcut(shortcuts, vm.SetColor7Command, nameof(vm.SetColor7Command), ShortcutCategory.SubtitleGridAndTextBox);
        AddShortcut(shortcuts, vm.SetColor8Command, nameof(vm.SetColor8Command), ShortcutCategory.SubtitleGridAndTextBox);
        AddShortcut(shortcuts, vm.RemoveColorCommand, nameof(vm.RemoveColorCommand), ShortcutCategory.SubtitleGridAndTextBox);
        AddShortcut(shortcuts, vm.SurroundWith1Command, nameof(vm.SurroundWith1Command), ShortcutCategory.SubtitleGridAndTextBox);
        AddShortcut(shortcuts, vm.SurroundWith2Command, nameof(vm.SurroundWith2Command), ShortcutCategory.SubtitleGridAndTextBox);
        AddShortcut(shortcuts, vm.SurroundWith3Command, nameof(vm.SurroundWith3Command), ShortcutCategory.SubtitleGridAndTextBox);
        AddShortcut(shortcuts, vm.RepeatPreviousLineCommand, nameof(vm.RepeatPreviousLineCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.RepeatNextLineCommand, nameof(vm.RepeatNextLineCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.InsertLineBeforeCommand, nameof(vm.InsertLineBeforeCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.InsertLineAfterCommand, nameof(vm.InsertLineAfterCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.WaveformInsertAtPositionAndFocusTextBoxCommand, nameof(vm.WaveformInsertAtPositionAndFocusTextBoxCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.WaveformInsertAtPositionNoFocusTextBoxCommand, nameof(vm.WaveformInsertAtPositionNoFocusTextBoxCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.WaveformPasteFromClipboardCommand, nameof(vm.WaveformPasteFromClipboardCommand), ShortcutCategory.Waveform);
        AddShortcut(shortcuts, vm.FocusSelectedLineCommand, nameof(vm.FocusSelectedLineCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.PlayFromStartOfVideoCommand, nameof(vm.PlayFromStartOfVideoCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.RemoveBlankLinesCommand, nameof(vm.RemoveBlankLinesCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.InsertSubtitleAtVideoPositionSetEndAtKeyUpCommand, nameof(vm.InsertSubtitleAtVideoPositionSetEndAtKeyUpCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.SpeechToTextSelectedLinesCommand, nameof(vm.SpeechToTextSelectedLinesCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.SpeechToTextSelectedLinesPromptForLangaugeCommand, nameof(vm.SpeechToTextSelectedLinesPromptForLangaugeCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.SpeechToTextSelectedLinesPromptForLangaugeFirstTimeCommand, nameof(vm.SpeechToTextSelectedLinesPromptForLangaugeFirstTimeCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.PlaySelectedLinesWithoutLoopCommand, nameof(vm.PlaySelectedLinesWithoutLoopCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.PlaySelectedLinesWithLoopCommand, nameof(vm.PlaySelectedLinesWithLoopCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ToggleCasingCommand, nameof(vm.ToggleCasingCommand), ShortcutCategory.SubtitleGridAndTextBox);
        AddShortcut(shortcuts, vm.ImportImageSubtitleForEditCommand, nameof(vm.ImportImageSubtitleForEditCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowMediaInformationCommand, nameof(vm.ShowMediaInformationCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowSubtitleFormatPickerCommand, nameof(vm.ShowSubtitleFormatPickerCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.TrimWhitespaceSelectedLinesCommand, nameof(vm.TrimWhitespaceSelectedLinesCommand), ShortcutCategory.SubtitleGrid);
        AddShortcut(shortcuts, vm.FocusTextBoxCommand, nameof(vm.FocusTextBoxCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.SortByStartTimeCommand, nameof(vm.SortByStartTimeCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.SortByEndTimeCommand, nameof(vm.SortByEndTimeCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowPickLayerFilterCommand, nameof(vm.ShowPickLayerFilterCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowPickLayerCommand, nameof(vm.ShowPickLayerCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.CopyTextToClipboardCommand, nameof(vm.CopyTextToClipboardCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.CopyTextFromOriginalToClipboardCommand, nameof(vm.CopyTextFromOriginalToClipboardCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowAssaImageColorPickerCommand, nameof(vm.ShowAssaImageColorPickerCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowAssaSetPositionCommand, nameof(vm.ShowAssaSetPositionCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowAssaApplyCustomOverrideTagsCommand, nameof(vm.ShowAssaApplyCustomOverrideTagsCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowAssaDrawCommand, nameof(vm.ShowAssaDrawCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowAssaGenerateProgressBarCommand, nameof(vm.ShowAssaGenerateProgressBarCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowAssaGenerateBackgroundCommand, nameof(vm.ShowAssaGenerateBackgroundCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowAssaStylesCommand, nameof(vm.ShowAssaStylesCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowAssaPropertiesCommand, nameof(vm.ShowAssaPropertiesCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowAssaAttachmentsCommand, nameof(vm.ShowAssaAttachmentsCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.RecalculateDurationSelectedLinesCommand, nameof(vm.RecalculateDurationSelectedLinesCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.WaveformToggleWaveformSpectrogramHeightCommand, nameof(vm.WaveformToggleWaveformSpectrogramHeightCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.SpectrogramToggleStyleCommand, nameof(vm.SpectrogramToggleStyleCommand), ShortcutCategory.General);
        AddShortcut(shortcuts, vm.ShowBeautifyTimeCodesCommand, nameof(vm.ShowBeautifyTimeCodesCommand), ShortcutCategory.General);

        return shortcuts;
    }

    public static List<SeShortCut> GetDefaultShortcuts(MainViewModel vm)
    {
        var cmd = GetCommandOrWin();

        return
        [
            new(nameof(vm.UndoCommand), [cmd, "Z"]),
            new(nameof(vm.RedoCommand), [cmd, "Y"]),
            new(nameof(vm.ShowGoToLineCommand), [cmd, "G"]),
            new(nameof(vm.AddOrEditBookmarkCommand), [cmd, "Shift", "B"]),
            new(nameof(vm.GoToPreviousLineCommand), ["Alt", "Up"]),
            new(nameof(vm.GoToNextLineCommand), ["Alt", "Down"]),
            new(nameof(vm.SelectAllLinesCommand), [cmd, "A"], ShortcutCategory.SubtitleGrid),
            new(nameof(vm.InverseSelectionCommand), [cmd, "Shift", "I"], ShortcutCategory.SubtitleGrid),
            new(nameof(vm.ToggleLinesItalicOrSelectedTextCommand), [cmd, "I"], ShortcutCategory.SubtitleGrid),
            new(nameof(vm.DeleteSelectedLinesCommand), ["Delete"], ShortcutCategory.SubtitleGrid),
            new(nameof(vm.ShowFindCommand), [cmd, "F"], ShortcutCategory.General),
            new(nameof(vm.FindNextCommand), [nameof(Avalonia.Input.Key.F3)], ShortcutCategory.General),
            new(nameof(vm.FindPreviousCommand), ["Shift", nameof(Avalonia.Input.Key.F3)], ShortcutCategory.General),
            new(nameof(vm.ShowReplaceCommand), [cmd, "R"], ShortcutCategory.General),
            new(nameof(vm.ShowMultipleReplaceCommand), [cmd, "Shift", "R"], ShortcutCategory.General),
            new(nameof(vm.OpenDataFolderCommand), [cmd, "Alt", "Shift", "D"], ShortcutCategory.General),
            new(nameof(vm.CommandFileNewCommand), [cmd, "N"], ShortcutCategory.General),
            new(nameof(vm.CommandFileOpenCommand), [cmd, "O"], ShortcutCategory.General),
            new(nameof(vm.CommandFileSaveCommand), [cmd, "S"], ShortcutCategory.General),
            new(nameof(vm.TogglePlayPauseCommand), [nameof(Avalonia.Input.Key.Space)], ShortcutCategory.General),
            new(nameof(vm.TogglePlayPause2Command), [cmd, nameof(Avalonia.Input.Key.Space)], ShortcutCategory.General),
            new(nameof(vm.VideoOneSecondBackCommand), [nameof(Avalonia.Input.Key.Left)], ShortcutCategory.General),
            new(nameof(vm.VideoOneSecondForwardCommand), [nameof(Avalonia.Input.Key.Right)], ShortcutCategory.General),
            new(nameof(vm.ShowHelpCommand), [nameof(Avalonia.Input.Key.F1)], ShortcutCategory.General),
            new(nameof(vm.ShowSourceViewCommand), [nameof(Avalonia.Input.Key.F2)], ShortcutCategory.General),
            new(nameof(vm.TextBoxDeleteSelectionCommand), ["Shift", nameof(Avalonia.Input.Key.Back)], ShortcutCategory.TextBox),
            new(nameof(vm.TextBoxCut2Command), ["Shift", nameof(Avalonia.Input.Key.Delete) ], ShortcutCategory.TextBox),
            new(nameof(vm.TextBoxCutCommand), [cmd, nameof(Avalonia.Input.Key.X)], ShortcutCategory.TextBox),
            new(nameof(vm.TextBoxPasteCommand), [cmd, nameof(Avalonia.Input.Key.V)], ShortcutCategory.TextBox),
            new(nameof(vm.TextBoxCopyCommand), [cmd, nameof(Avalonia.Input.Key.C)], ShortcutCategory.TextBox),
            new(nameof(vm.TextBoxSelectAllCommand), [cmd, nameof(Avalonia.Input.Key.A)], ShortcutCategory.TextBox),
            new(nameof(vm.SubtitleGridCutCommand), [cmd, nameof(Avalonia.Input.Key.X)], ShortcutCategory.SubtitleGrid),
            new(nameof(vm.SubtitleGridPasteCommand), [cmd, nameof(Avalonia.Input.Key.V)], ShortcutCategory.SubtitleGrid),
            new(nameof(vm.SubtitleGridCopyCommand), [cmd, nameof(Avalonia.Input.Key.C)], ShortcutCategory.SubtitleGrid),
            new(nameof(vm.ListErrorsCommand), [cmd, nameof(Avalonia.Input.Key.F8)], ShortcutCategory.SubtitleGrid),
            new(nameof(vm.GoToPreviousErrorCommand), ["Shift", nameof(Avalonia.Input.Key.F8)], ShortcutCategory.SubtitleGrid),
            new(nameof(vm.GoToNextErrorCommand), [nameof(Avalonia.Input.Key.F8)], ShortcutCategory.SubtitleGrid),
            new(nameof(vm.ShowSpellCheckCommand), ["Alt", nameof(Avalonia.Input.Key.F7)], ShortcutCategory.SubtitleGrid),
            new(nameof(vm.ShowToolsChangeCasingCommand), [cmd, "Shift", nameof(Avalonia.Input.Key.C)], ShortcutCategory.General),
            new(nameof(vm.ShowToolsRemoveTextForHearingImpairedCommand), [cmd, "Shift", nameof(Avalonia.Input.Key.H)], ShortcutCategory.General),
            new(nameof(vm.ShowSyncAdjustAllTimesCommand), [cmd, "Shift", nameof(Avalonia.Input.Key.A)], ShortcutCategory.General),
            new(nameof(vm.WaveformPasteFromClipboardCommand), [cmd, nameof(Avalonia.Input.Key.V)], ShortcutCategory.Waveform),
        ];
    }

    private static string GetCommandOrWin()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return "Win";
        }

        return "Ctrl";
    }

    public class AvailableShortcut
    {
        public string Name { get; set; }
        public ShortcutCategory Category { get; set; }

        public AvailableShortcut(IRelayCommand relayCommand, string shortcutName)
        {
            Name = shortcutName;
            RelayCommand = relayCommand;
            Category = ShortcutCategory.General;
        }
        public AvailableShortcut(IRelayCommand relayCommand, string shortcutName, ShortcutCategory category)
        {
            Name = shortcutName;
            RelayCommand = relayCommand;
            Category = category;
        }

        public IRelayCommand RelayCommand { get; set; }
    }
}