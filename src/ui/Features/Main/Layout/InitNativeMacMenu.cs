using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Plugins;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Main.Layout;

public static class InitNativeMacMenu
{
    private static PropertyChangedEventHandler? _handler;
    private static readonly List<(string PropertyName, Action<MainViewModel> Updater)> _updaters = [];

    public static void Make(Application app, MainViewModel vm)
    {
        if (_handler != null)
        {
            vm.PropertyChanged -= _handler;
            _handler = null;
        }
        _updaters.Clear();

        var shortcuts = ShortcutsMain.GetUsedShortcuts(vm);
        var l = Se.Language.Main.Menu;

        // ── App menu ("Subtitle Edit" on macOS) ──────────────────────────────
        var appItems = new NativeMenu();
        appItems.Items.Add(MakeItem(Clean(Se.Language.Help.AboutSubtitleEdit), vm.ShowAboutCommand, shortcuts));
        appItems.Items.Add(new NativeMenuItemSeparator());
        var prefsItem = MakeItem(Clean(l.Settings), vm.CommandShowSettingsCommand, shortcuts);
        prefsItem.Gesture = new KeyGesture(Key.OemComma, KeyModifiers.Meta);
        appItems.Items.Add(prefsItem);
        appItems.Items.Add(new NativeMenuItemSeparator());
        var quitItem = new NativeMenuItem(Clean(l.Exit));
        quitItem.Gesture = new KeyGesture(Key.Q, KeyModifiers.Meta);
        quitItem.Click += (_, _) => vm.CommandExitCommand.Execute(null);
        appItems.Items.Add(quitItem);

        // ── File ─────────────────────────────────────────────────────────────
        var fileItems = new NativeMenu();
        fileItems.Items.Add(MakeItem(Clean(l.New), vm.CommandFileNewCommand, shortcuts));
        fileItems.Items.Add(MakeConditional(vm, Clean(l.NewKeepVideo), vm.CommandFileNewKeepVideoCommand, shortcuts,
            v => v.IsVideoLoaded, nameof(vm.IsVideoLoaded)));
        fileItems.Items.Add(new NativeMenuItemSeparator());
        fileItems.Items.Add(MakeItem(Clean(l.Open), vm.CommandFileOpenCommand, shortcuts));
        fileItems.Items.Add(MakeConditional(vm, Clean(l.OpenKeepVideo), vm.CommandFileOpenKeepVideoCommand, shortcuts,
            v => v.IsVideoLoaded, nameof(vm.IsVideoLoaded)));
        fileItems.Items.Add(MakeConditional(vm, Clean(l.OpenOriginal), vm.FileOpenOriginalCommand, shortcuts,
            v => !v.ShowColumnOriginalText, nameof(vm.ShowColumnOriginalText)));
        fileItems.Items.Add(MakeConditional(vm, Clean(l.CloseOriginal), vm.FileCloseOriginalCommand, shortcuts,
            v => v.ShowColumnOriginalText, nameof(vm.ShowColumnOriginalText)));
        fileItems.Items.Add(MakeConditional(vm, Clean(l.CloseTranslation), vm.FileCloseTranslationCommand, shortcuts,
            v => v.ShowColumnOriginalText, nameof(vm.ShowColumnOriginalText)));

        vm.NativeMenuReopen = new NativeMenuItem(Clean(l.Reopen)) { Menu = new NativeMenu() };
        fileItems.Items.Add(vm.NativeMenuReopen);
        fileItems.Items.Add(MakeItem(Clean(l.RestoreAutoBackup), vm.ShowRestoreAutoBackupCommand, shortcuts));
        fileItems.Items.Add(new NativeMenuItemSeparator());
        fileItems.Items.Add(MakeItem(Clean(l.Save), vm.CommandFileSaveCommand, shortcuts));
        fileItems.Items.Add(MakeItem(Clean(l.SaveAs), vm.CommandFileSaveAsCommand, shortcuts));
        fileItems.Items.Add(new NativeMenuItemSeparator());

        var filePropsItem = new NativeMenuItem(Clean(vm.FilePropertiesText));
        filePropsItem.IsEnabled = vm.IsFilePropertiesVisible;
        filePropsItem.Click += (_, _) => vm.FilePropertiesShowCommand.Execute(null);
        RegisterUpdater(v =>
        {
            filePropsItem.Header = Clean(v.FilePropertiesText);
            filePropsItem.IsEnabled = v.IsFilePropertiesVisible;
        }, nameof(vm.FilePropertiesText), nameof(vm.IsFilePropertiesVisible));
        fileItems.Items.Add(filePropsItem);

        fileItems.Items.Add(MakeItem(Clean(l.OpenContainingFolder), vm.OpenContainingFolderCommand, shortcuts));
        fileItems.Items.Add(MakeItem(Clean(l.Compare), vm.ShowCompareCommand, shortcuts));
        fileItems.Items.Add(MakeItem(Clean(l.Statistics), vm.ShowStatisticsCommand, shortcuts));
        fileItems.Items.Add(new NativeMenuItemSeparator());

        var lImport = Se.Language.File.Import;
        var importItems = new NativeMenu();
        importItems.Items.Add(MakeItem(Clean(lImport.SubtitleWithManuallyChosenEncodingDotDotDot), vm.ShowImportSubtitleWithManuallyChosenEncodingCommand, shortcuts));
        importItems.Items.Add(new NativeMenuItemSeparator());
        importItems.Items.Add(MakeItem(Clean(lImport.ImagedBasedSubtitleForOcrDotDotDot), vm.ImportImageSubtitleForOcrCommand, shortcuts));
        importItems.Items.Add(MakeItem(Clean(lImport.ImagedBasedSubtitleForEditDotDotDot), vm.ImportImageSubtitleForEditCommand, shortcuts));
        importItems.Items.Add(MakeItem(Clean(lImport.ImagesForOcrDotDotDot), vm.ImportImagesCommand, shortcuts));
        importItems.Items.Add(MakeItem(Clean(lImport.PlainTextDotDotDot), vm.ImportPlainTextCommand, shortcuts));
        importItems.Items.Add(MakeItem(Clean(lImport.CsvXlsxCustomColumnsDotDotDot), vm.ImportCsvXlsxCustomColumnsCommand, shortcuts));
        importItems.Items.Add(new NativeMenuItemSeparator());
        importItems.Items.Add(MakeItem(Clean(lImport.TimeCodesDotDotDot), vm.ImportTimeCodesCommand, shortcuts));
        importItems.Items.Add(MakeItem(Clean(lImport.FormattingDotDotDot), vm.ImportStylingCommand, shortcuts));
        fileItems.Items.Add(new NativeMenuItem(Clean(l.Import)) { Menu = importItems });

        var lExport = Se.Language.File.Export;
        var exportItems = new NativeMenu();
        exportItems.Items.Add(MakeItem(Se.Language.General.BluRaySup, vm.ExportBluRaySupCommand, shortcuts));
        exportItems.Items.Add(MakeItem(Se.Language.General.BdnXml, vm.ExportBdnXmlCommand, shortcuts));
        exportItems.Items.Add(MakeItem(new CapMakerPlus().Name, vm.ExportCapMakerPlusCommand, shortcuts));
        exportItems.Items.Add(MakeItem(CheetahCaption.NameOfFormat, vm.ExportCheetahCaptionCommand, shortcuts));
        exportItems.Items.Add(MakeItem(CheetahCaptionOld.NameOfFormat, vm.ExportCheetahCaptionOldCommand, shortcuts));
        exportItems.Items.Add(MakeItem(Cavena890.NameOfFormat, vm.ExportCavena890Command, shortcuts));
        exportItems.Items.Add(MakeItem(lExport.TitleExportDCinemaInteropPng, vm.ExportDCinemaInteropPngCommand, shortcuts));
        exportItems.Items.Add(MakeItem(lExport.TitleExportDCinemaSmpte2014Png, vm.ExportDCinemaSmpte2014PngCommand, shortcuts));
        exportItems.Items.Add(MakeItem(Ebu.NameOfFormat, vm.ExportEbuStlCommand, shortcuts));
        exportItems.Items.Add(MakeItem("DOST/png", vm.ExportDostPngCommand, shortcuts));
        exportItems.Items.Add(MakeItem("FCP/png", vm.ExportFcpPngCommand, shortcuts));
        exportItems.Items.Add(MakeItem(Se.Language.General.ImagesWithTimeCode, vm.ExportImagesWithTimeCodeCommand, shortcuts));
        exportItems.Items.Add(MakeItem(Pac.NameOfFormat, vm.ExportPacCommand, shortcuts));
        exportItems.Items.Add(MakeItem(new PacUnicode().Name, vm.ExportPacUnicodeCommand, shortcuts));
        exportItems.Items.Add(MakeItem(lExport.TitleExportVobSub, vm.ExportVobSubCommand, shortcuts));
        exportItems.Items.Add(MakeItem("WebVTT png", vm.ExportWebVttThumbnailsCommand, shortcuts));
        exportItems.Items.Add(new NativeMenuItemSeparator());
        exportItems.Items.Add(MakeItem(Clean(lExport.CustomTextFormatsDotDotDot), vm.ShowExportCustomTextFormatCommand, shortcuts));
        exportItems.Items.Add(MakeItem(Clean(lExport.PlainTextDotDotDot), vm.ShowExportPlainTextCommand, shortcuts));
        fileItems.Items.Add(new NativeMenuItem(Clean(l.Export)) { Menu = exportItems });

        // ── Edit ─────────────────────────────────────────────────────────────
        var editItems = new NativeMenu();
        editItems.Items.Add(MakeItem(Clean(l.Undo), vm.UndoCommand, shortcuts));
        editItems.Items.Add(MakeItem(Clean(l.Redo), vm.RedoCommand, shortcuts));
        editItems.Items.Add(MakeItem(Clean(l.ShowHistory), vm.ShowHistoryCommand, shortcuts));
        editItems.Items.Add(new NativeMenuItemSeparator());
        editItems.Items.Add(MakeItem(Clean(l.Find), vm.ShowFindCommand, shortcuts));
        editItems.Items.Add(MakeItem(Clean(l.FindNext), vm.FindNextCommand, shortcuts));
        editItems.Items.Add(MakeItem(Clean(l.Replace), vm.ShowReplaceCommand, shortcuts));
        editItems.Items.Add(MakeItem(Clean(l.MultipleReplace), vm.ShowMultipleReplaceCommand, shortcuts));
        editItems.Items.Add(MakeItem(Clean(l.GoToLineNumber), vm.ShowGoToLineCommand, shortcuts));
        editItems.Items.Add(new NativeMenuItemSeparator());
        editItems.Items.Add(MakeToggle(Clean(l.RightToLeftMode), vm.RightToLeftToggleCommand,
            vm.IsRightToLeftEnabled, v => v.IsRightToLeftEnabled, nameof(vm.IsRightToLeftEnabled)));
        editItems.Items.Add(MakeItem(Clean(l.FixRightToLeftViaUnicodeControlCharacters), vm.FixRightToLeftViaUnicodeControlCharactersCommand, shortcuts));
        editItems.Items.Add(MakeItem(Clean(l.RemoveUnicodeControlCharacters), vm.RemoveUnicodeControlCharactersCommand, shortcuts));
        editItems.Items.Add(MakeItem(Clean(l.ReverseRightToLeftStartEnd), vm.ReverseRightToLeftStartEndCommand, shortcuts));
        editItems.Items.Add(new NativeMenuItemSeparator());
        editItems.Items.Add(MakeItem(Clean(l.ModifySelectionDotDotDot), vm.ShowModifySelectionCommand, shortcuts));
        editItems.Items.Add(MakeItem(Clean(Se.Language.General.InvertSelection), vm.InverseSelectionCommand, shortcuts));
        editItems.Items.Add(MakeItem(Clean(Se.Language.General.SelectAll), vm.SelectAllLinesCommand, shortcuts));

        // ── Tools ────────────────────────────────────────────────────────────
        var toolsList = new List<NativeMenuItem>
        {
            MakeItem(Clean(l.AdjustDurations), vm.ShowToolsAdjustDurationsCommand, shortcuts),
            MakeItem(Clean(l.ApplyDurationLimits), vm.ShowApplyDurationLimitsCommand, shortcuts),
            MakeItem(Clean(l.BatchConvert), vm.ShowToolsBatchConvertCommand, shortcuts),
            MakeItem(Clean(l.BeautifyTimeCodes), vm.ShowBeautifyTimeCodesCommand, shortcuts),
            MakeItem(Clean(l.BridgeGaps), vm.ShowBridgeGapsCommand, shortcuts),
            MakeItem(Clean(l.ApplyMinGap), vm.ShowApplyMinGapCommand, shortcuts),
            MakeItem(Clean(l.ChangeCasing), vm.ShowToolsChangeCasingCommand, shortcuts),
            MakeItem(Clean(l.ChangeFormatting), vm.ShowToolsChangeFormattingCommand, shortcuts),
            MakeItem(Clean(l.FixCommonErrors), vm.ShowToolsFixCommonErrorsCommand, shortcuts),
            MakeItem(Clean(l.CheckAndFixNetflixErrors), vm.ShowToolsFixNetflixErrorsCommand, shortcuts),
            MakeItem(Clean(l.MakeEmptyTranslationFromCurrentSubtitle), vm.ToolsMakeEmptyTranslationFromCurrentSubtitleCommand, shortcuts),
            MakeItem(Clean(l.MergeLinesWithSameText), vm.ShowToolsMergeLinesWithSameTextCommand, shortcuts),
            MakeItem(Clean(l.MergeLinesWithSameTimeCodes), vm.ShowToolsMergeLinesWithSameTimeCodesCommand, shortcuts),
            MakeItem(Clean(l.SplitBreakLongLines), vm.ShowToolsSplitBreakLongLinesCommand, shortcuts),
            MakeItem(Clean(l.MergeShortLines), vm.ShowToolsMergeShortLinesCommand, shortcuts),
            MakeItem(Clean(l.MergeContinuationLines), vm.ShowToolsMergeContinuationLinesCommand, shortcuts),
            MakeItem(Clean(l.SnapAllTimesToFrames), vm.SnapAllTimesToFramesCommand, shortcuts),
            MakeItem(Clean(l.MergeTwoSubtitles), vm.ShowToolsMergeTwoSubtitlesCommand, shortcuts),
            MakeItem(Clean(l.SortSubtitles), vm.ShowSortByCommand, shortcuts),
            MakeItem(Clean(l.Renumber), vm.ShowToolsRenumberCommand, shortcuts),
            MakeItem(Clean(l.RemoveTextForHearingImpaired), vm.ShowToolsRemoveTextForHearingImpairedCommand, shortcuts),
            MakeItem(Clean(l.ConvertActors), vm.ShowToolsConvertActorsCommand, shortcuts),
        };
        var toolItems = new NativeMenu();
        foreach (var item in toolsList.OrderBy(i => i.Header?.Replace("_", string.Empty)))
            toolItems.Items.Add(item);
        toolItems.Items.Add(new NativeMenuItemSeparator());
        toolItems.Items.Add(MakeItem(Clean(l.JoinSubtitles), vm.ShowToolsJoinCommand, shortcuts));
        toolItems.Items.Add(MakeItem(Clean(l.SplitSubtitle), vm.ShowToolsSplitCommand, shortcuts));

        // ── Plugins ──────────────────────────────────────────────────────────
        vm.NativeMenuPlugins = new NativeMenuItem(Clean(Se.Language.Plugins.Title)) { Menu = new NativeMenu() };
        vm.NativeMenuPlugins.IsEnabled = Se.Settings.Appearance.ShowPluginsMenu;
        UpdatePluginsMenu(vm);

        // ── Spell Check ──────────────────────────────────────────────────────
        var spellItems = new NativeMenu();
        spellItems.Items.Add(MakeItem(Clean(l.SpellCheck), vm.ShowSpellCheckCommand, shortcuts));
        spellItems.Items.Add(MakeItem(Clean(l.FindDoubleWords), vm.ShowFindDoubleWordsCommand, shortcuts));
        spellItems.Items.Add(MakeItem(Clean(l.FindDoubleLines), vm.ShowFindDoubleLinesCommand, shortcuts));
        spellItems.Items.Add(new NativeMenuItemSeparator());
        spellItems.Items.Add(MakeItem(Clean(l.AddNameToNamesList), vm.ShowAddToNameListCommand, shortcuts));
        spellItems.Items.Add(MakeItem(Clean(l.GetDictionaries), vm.ShowSpellCheckDictionariesCommand, shortcuts));

        // ── Video ────────────────────────────────────────────────────────────
        var videoItems = new NativeMenu();
        videoItems.Items.Add(MakeItem(Clean(l.OpenVideo), vm.CommandVideoOpenCommand, shortcuts));
        videoItems.Items.Add(MakeItem(Clean(l.OpenVideoFromUrl), vm.ShowVideoOpenFromUrlCommand, shortcuts));
        videoItems.Items.Add(MakeItem(Clean(l.CloseVideoFile), vm.CommandVideoCloseCommand, shortcuts));

        vm.NativeMenuAudioTracks = new NativeMenuItem(Clean(l.AudioTracks)) { Menu = new NativeMenu() };
        vm.NativeMenuAudioTracks.IsEnabled = vm.IsAudioTracksVisible;
        RegisterUpdater(v => vm.NativeMenuAudioTracks!.IsEnabled = v.IsAudioTracksVisible, nameof(vm.IsAudioTracksVisible));
        videoItems.Items.Add(vm.NativeMenuAudioTracks);

        videoItems.Items.Add(new NativeMenuItemSeparator());
        videoItems.Items.Add(MakeItem(Clean(l.SpeechToText), vm.ShowSpeechToTextWhisperCommand, shortcuts));
        videoItems.Items.Add(MakeItem(Clean(l.TextToSpeech), vm.ShowVideoTextToSpeechCommand, shortcuts));
        videoItems.Items.Add(new NativeMenuItemSeparator());
        videoItems.Items.Add(MakeItem(Clean(l.GenerateBurnIn), vm.ShowVideoBurnInCommand, shortcuts));
        videoItems.Items.Add(MakeItem(Clean(l.GenerateTransparent), vm.ShowVideoTransparentSubtitlesCommand, shortcuts));
        videoItems.Items.Add(MakeItem(Clean(Se.Language.Video.GenerateBlankVideoDotDotDot), vm.VideoGenerateBlankCommand, shortcuts));
        videoItems.Items.Add(MakeItem(Clean(Se.Language.Video.EmbedSubtitlesDotDotDot), vm.VideoEmbedCommand, shortcuts));
        videoItems.Items.Add(new NativeMenuItemSeparator());
        videoItems.Items.Add(MakeItem(Clean(l.GenerateImportShotChanges), vm.ShowShotChangesSubtitlesCommand, shortcuts));
        videoItems.Items.Add(MakeConditional(vm, Clean(l.ListShotChanges), vm.ShowShotChangesListCommand, shortcuts,
            v => v.ShowShotChangesListMenuItem, nameof(vm.ShowShotChangesListMenuItem)));
        videoItems.Items.Add(new NativeMenuItemSeparator());
        videoItems.Items.Add(MakeConditional(vm, Clean(l.UndockVideoControls), vm.VideoUndockControlsCommand, shortcuts,
            v => !v.AreVideoControlsUndocked, nameof(vm.AreVideoControlsUndocked)));
        videoItems.Items.Add(MakeConditional(vm, Clean(l.DockVideoControls), vm.VideoRedockControlsCommand, shortcuts,
            v => v.AreVideoControlsUndocked, nameof(vm.AreVideoControlsUndocked)));
        videoItems.Items.Add(MakeToggle(Clean(l.ToggleSelectSubtitleWhilePlayingCurrentlyOff), vm.ToggleCurrentSubtitleWhilePlayingCommand,
            vm.SelectCurrentSubtitleWhilePlaying, v => v.SelectCurrentSubtitleWhilePlaying, nameof(vm.SelectCurrentSubtitleWhilePlaying)));

        var lVideo = Se.Language.Video;
        var videoMoreList = new List<NativeMenuItem>
        {
            MakeConditional(vm, Clean(lVideo.OpenSecondarySubtitleOnVideoPlayerDotDotDot), vm.OpenSecondarySubtitleCommand, shortcuts,
                v => !v.IsSubtitleSecondaryVisible, nameof(vm.IsSubtitleSecondaryVisible)),
            MakeConditional(vm, Clean(lVideo.RemoveSecondarySubtitleOnVideoPlayer), vm.ClearSecondarySubtitleCommand, shortcuts,
                v => v.IsSubtitleSecondaryVisible, nameof(vm.IsSubtitleSecondaryVisible)),
            MakeItem(Clean(lVideo.ReEncodeVideoForBetterSubtitlingDotDotDot), vm.VideoReEncodeCommand, shortcuts),
            MakeItem(Clean(lVideo.CutVideoDotDotDot), vm.VideoCutCommand, shortcuts),
        };
        videoMoreList.Add(MakeToggle(Clean(Se.Language.Options.Shortcuts.ToggleWaveformToolbar), vm.ToggleIsWaveformToolbarVisibleCommand,
            vm.IsWaveformToolbarVisible, v => v.IsWaveformToolbarVisible, nameof(vm.IsWaveformToolbarVisible)));

        var setOffsetItem = new NativeMenuItem(Clean(vm.SetVideoOffsetText));
        setOffsetItem.Click += (_, _) => vm.ShowVideoSetOffsetCommand.Execute(null);
        RegisterUpdater(v => setOffsetItem.Header = Clean(v.SetVideoOffsetText), nameof(vm.SetVideoOffsetText));
        videoMoreList.Add(setOffsetItem);

        videoMoreList.Add(MakeToggle(Clean(l.SmpteTiming), vm.ToggleSmpteTimingCommand,
            vm.IsSmpteTimingEnabled, v => v.IsSmpteTimingEnabled, nameof(vm.IsSmpteTimingEnabled)));

        var videoMoreItems = new NativeMenu();
        foreach (var item in videoMoreList.OrderBy(i => i.Header?.TrimStart('_', ' ')))
            videoMoreItems.Items.Add(item);

        var videoMoreMenu = new NativeMenuItem(Clean(Se.Language.General.More)) { Menu = videoMoreItems };
        videoMoreMenu.IsEnabled = vm.IsVideoLoaded;
        RegisterUpdater(v => videoMoreMenu.IsEnabled = v.IsVideoLoaded, nameof(vm.IsVideoLoaded));
        videoItems.Items.Add(videoMoreMenu);

        // ── Synchronization ──────────────────────────────────────────────────
        var syncItems = new NativeMenu();
        syncItems.Items.Add(MakeItem(Clean(l.AdjustAllTimes), vm.ShowSyncAdjustAllTimesCommand, shortcuts));
        syncItems.Items.Add(MakeItem(Clean(l.VisualSync), vm.ShowVisualSyncCommand, shortcuts));
        syncItems.Items.Add(MakeItem(Clean(l.PointSync), vm.ShowPointSyncCommand, shortcuts));
        syncItems.Items.Add(MakeItem(Clean(l.PointSyncViaOther), vm.ShowPointSyncViaOtherCommand, shortcuts));
        syncItems.Items.Add(MakeItem(Clean(l.ChangeFrameRate), vm.ShowSyncChangeFrameRateCommand, shortcuts));
        syncItems.Items.Add(MakeItem(Clean(l.ChangeSpeed), vm.ShowSyncChangeSpeedCommand, shortcuts));

        // ── Translate ────────────────────────────────────────────────────────
        var translateItems = new NativeMenu();
        translateItems.Items.Add(MakeItem(Clean(l.AutoTranslate), vm.ShowAutoTranslateCommand, shortcuts));
        translateItems.Items.Add(MakeItem(Clean(l.TranslateViaCopyPaste), vm.ShowTranslateViaCopyPasteCommand, shortcuts));

        // ── Options ──────────────────────────────────────────────────────────
        var optionsItems = new NativeMenu();
        optionsItems.Items.Add(MakeItem(Clean(l.Settings), vm.CommandShowSettingsCommand, shortcuts));
        optionsItems.Items.Add(MakeItem(Clean(l.Shortcuts), vm.CommandShowSettingsShortcutsCommand, shortcuts));
        optionsItems.Items.Add(MakeItem(Clean(l.WordLists), vm.ShowWordListsCommand, shortcuts));
        optionsItems.Items.Add(MakeItem(Clean(l.ChooseLanguage), vm.CommandShowSettingsLanguageCommand, shortcuts));

        // ── Help ─────────────────────────────────────────────────────────────
        var helpItems = new NativeMenu();
        helpItems.Items.Add(MakeItem(Clean(l.CheckForUpdates), vm.ShowCheckForUpdatesCommand, shortcuts));
        helpItems.Items.Add(new NativeMenuItemSeparator());
        helpItems.Items.Add(MakeItem(Clean(l.Help), vm.ShowHelpCommand, shortcuts));
        helpItems.Items.Add(MakeItem(Clean(l.About), vm.ShowAboutCommand, shortcuts));

        // ── ASSA Tools ───────────────────────────────────────────────────────
        var assaList = new List<NativeMenuItem>
        {
            MakeItem(Clean(l.AssaProgressBar), vm.ShowAssaGenerateProgressBarCommand, shortcuts),
            MakeItem(Clean(l.AssaChangeResolution), vm.ShowAssaChangeResolutionCommand, shortcuts),
            MakeItem(Clean(l.AssaGenerateBackground), vm.ShowAssaGenerateBackgroundCommand, shortcuts),
            MakeItem(Clean(l.AssaImageColorPicker), vm.ShowAssaImageColorPickerCommand, shortcuts),
            MakeItem(Clean(l.AssaSetPosition), vm.ShowAssaSetPositionCommand, shortcuts),
            MakeItem(Clean(l.AssaApplyAdvancedEffects), vm.ShowAssaApplyAdvancedEffectCommand, shortcuts),
            MakeItem(Clean(l.AssaApplyCustomOverrideTags), vm.ShowAssaApplyCustomOverrideTagsCommand, shortcuts),
            MakeItem(Clean(l.AssaDraw), vm.ShowAssaDrawCommand, shortcuts),
            MakeItem(Clean(l.AssaProperties), vm.ShowAssaPropertiesCommand, shortcuts),
            MakeItem(Clean(l.AssaAttachments), vm.ShowAssaAttachmentsCommand, shortcuts),
            MakeItem(Clean(l.AssaStyles), vm.ShowAssaStylesCommand, shortcuts),
        };
        var assaItems = new NativeMenu();
        foreach (var item in assaList.OrderBy(i => i.Header?.Replace("_", string.Empty)))
            assaItems.Items.Add(item);
        assaItems.Items.Add(new NativeMenuItemSeparator());
        assaItems.Items.Add(MakeItem(Clean(l.FilterLayersForDisplayDotDotDot), vm.ShowPickLayerFilterCommand, shortcuts));
        var assaMenu = new NativeMenuItem(Clean(l.AssaTools)) { Menu = assaItems };
        assaMenu.IsEnabled = vm.IsFormatAssa;
        RegisterUpdater(v => assaMenu.IsEnabled = v.IsFormatAssa, nameof(vm.IsFormatAssa));

        // ── Assemble ─────────────────────────────────────────────────────────
        var root = new NativeMenu();
        root.Items.Add(new NativeMenuItem("Subtitle Edit") { Menu = appItems });
        root.Items.Add(new NativeMenuItem(Clean(l.File)) { Menu = fileItems });
        root.Items.Add(new NativeMenuItem(Clean(l.Edit)) { Menu = editItems });
        root.Items.Add(new NativeMenuItem(Clean(l.Tools)) { Menu = toolItems });
        root.Items.Add(vm.NativeMenuPlugins);
        root.Items.Add(new NativeMenuItem(Clean(l.SpellCheckTitle)) { Menu = spellItems });
        root.Items.Add(new NativeMenuItem(Clean(l.Video)) { Menu = videoItems });
        root.Items.Add(new NativeMenuItem(Clean(l.Synchronization)) { Menu = syncItems });
        root.Items.Add(new NativeMenuItem(Clean(l.Translate)) { Menu = translateItems });
        root.Items.Add(new NativeMenuItem(Clean(l.Options)) { Menu = optionsItems });
        root.Items.Add(new NativeMenuItem(Clean(l.HelpTitle)) { Menu = helpItems });
        root.Items.Add(assaMenu);

        _handler = (s, e) =>
        {
            if (s is not MainViewModel v2 || e.PropertyName is null)
                return;
            foreach (var (propName, updater) in _updaters)
            {
                if (propName == e.PropertyName)
                    updater(v2);
            }
        };
        vm.PropertyChanged += _handler;

        NativeMenu.SetMenu(app, root);
        UpdateRecentFiles(vm);
    }

    public static void UpdateRecentFiles(MainViewModel vm)
    {
        if (vm.NativeMenuReopen?.Menu is not NativeMenu menu)
            return;

        menu.Items.Clear();

        var files = Se.Settings.File.RecentFiles
            .Where(p => !string.IsNullOrEmpty(p.SubtitleFileName) && System.IO.File.Exists(p.SubtitleFileName))
            .ToList();

        vm.NativeMenuReopen.IsEnabled = files.Count > 0;

        if (files.Count == 0)
            return;

        foreach (var file in files)
        {
            var header = file.SubtitleFileName ?? string.Empty;
            if (!string.IsNullOrEmpty(file.SubtitleFileNameOriginal) && System.IO.File.Exists(file.SubtitleFileNameOriginal))
            {
                header += " + ";
                header += System.IO.Path.GetDirectoryName(file.SubtitleFileName) == System.IO.Path.GetDirectoryName(file.SubtitleFileNameOriginal)
                    ? System.IO.Path.GetFileName(file.SubtitleFileNameOriginal)
                    : file.SubtitleFileNameOriginal;
            }
            if (header.Length > 80)
                header = "…" + header[^77..];

            var recentItem = new NativeMenuItem(header);
            var capturedFile = file;
            recentItem.Click += (_, _) => vm.CommandFileReopenCommand.Execute(capturedFile);
            menu.Items.Add(recentItem);
        }

        menu.Items.Add(new NativeMenuItemSeparator());
        var clearItem = new NativeMenuItem(Clean(Se.Language.Main.Menu.ClearRecentFiles));
        clearItem.Click += (_, _) => vm.CommandFileClearRecentFilesCommand.Execute(null);
        menu.Items.Add(clearItem);
    }

    public static void UpdatePluginsMenu(MainViewModel vm)
    {
        if (vm.NativeMenuPlugins?.Menu is not NativeMenu menu)
            return;

        menu.Items.Clear();

        var enabled = vm.GetInstalledPlugins()
            .Where(p => !Se.Settings.Plugins.DisabledPluginNames.Contains(p.Manifest.Name))
            .OrderBy(p => p.Manifest.Name)
            .ToList();

        if (enabled.Count == 0)
        {
            menu.Items.Add(new NativeMenuItem(Se.Language.Plugins.NoPluginsInstalled) { IsEnabled = false });
        }
        else
        {
            foreach (var plugin in enabled)
            {
                var pluginItem = new NativeMenuItem(plugin.Manifest.Name) { IsEnabled = plugin.CanRun };
                var captured = plugin;
                pluginItem.Click += (_, _) => vm.RunPluginCommand.Execute(captured);
                menu.Items.Add(pluginItem);
            }
        }

        menu.Items.Add(new NativeMenuItemSeparator());
        var manageItem = new NativeMenuItem(Se.Language.Plugins.ManagePlugins);
        manageItem.Click += (_, _) => vm.ShowPluginManagerCommand.Execute(null);
        menu.Items.Add(manageItem);
    }

    public static void UpdateAudioTracksMenu(MainViewModel vm, IList<AudioTrackInfo> tracks, AudioTrackInfo? current)
    {
        if (vm.NativeMenuAudioTracks?.Menu is not NativeMenu menu)
            return;

        menu.Items.Clear();

        foreach (var track in tracks)
        {
            var trackName = string.Format(Se.Language.Main.AudioTrackX, track.Id);
            if (!string.IsNullOrEmpty(track.Language))
            {
                var lang = Iso639Dash2LanguageCode.List.FirstOrDefault(p => p.ThreeLetterCode == track.Language);
                trackName += string.IsNullOrEmpty(lang?.EnglishName) ? $" - {track.Language}" : $" - {lang.EnglishName}";
            }
            if (!string.IsNullOrEmpty(track.Title))
                trackName += $" - {track.Title}";

            var item = new NativeMenuItem(trackName);
            item.ToggleType = MenuItemToggleType.CheckBox;
            item.IsChecked = track.FfIndex == (current?.FfIndex ?? -1);
            var captured = track;
            item.Click += (_, _) => vm.PickAudioTrackCommand.Execute(captured);
            menu.Items.Add(item);
        }
    }

    public static void UpdateShortcuts(MainViewModel vm)
    {
        if (Application.Current is { } app)
            Make(app, vm);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static NativeMenuItem MakeItem(string? header, IRelayCommand command, List<ShortCut> shortcuts)
    {
        var item = new NativeMenuItem(header ?? string.Empty);
        item.Click += (_, _) => command.Execute(null);
        item.Gesture = FindGesture(command, shortcuts);
        return item;
    }

    private static NativeMenuItem MakeConditional(MainViewModel vm, string? header, IRelayCommand command,
        List<ShortCut> shortcuts, Func<MainViewModel, bool> isEnabled, params string[] propertyNames)
    {
        var item = MakeItem(header, command, shortcuts);
        item.IsEnabled = isEnabled(vm);
        RegisterUpdater(v => item.IsEnabled = isEnabled(v), propertyNames);
        return item;
    }

    private static NativeMenuItem MakeToggle(string? header, IRelayCommand command, bool initialChecked,
        Func<MainViewModel, bool> isChecked, params string[] propertyNames)
    {
        var item = new NativeMenuItem(header ?? string.Empty);
        item.ToggleType = MenuItemToggleType.CheckBox;
        item.IsChecked = initialChecked;
        item.Click += (_, _) => command.Execute(null);
        RegisterUpdater(v => item.IsChecked = isChecked(v), propertyNames);
        return item;
    }

    private static KeyGesture? FindGesture(IRelayCommand command, List<ShortCut> shortcuts)
    {
        foreach (var sc in shortcuts)
        {
            if (ReferenceEquals(sc.Action, command))
                return InitMenu.ToKeyGesture(sc);
        }
        return null;
    }

    private static void RegisterUpdater(Action<MainViewModel> updater, params string[] propertyNames)
    {
        foreach (var name in propertyNames)
            _updaters.Add((name, updater));
    }

    private static string Clean(string? s) => s?.Replace("_", string.Empty) ?? string.Empty;
}
