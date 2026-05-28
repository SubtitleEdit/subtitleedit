using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Main.Layout;

/// <summary>
/// Builds and maintains the native macOS menu bar.
///
/// Avalonia routes NativeMenu.SetMenu calls differently depending on the target:
///   - SetMenu(Application, menu) → _factory.SetAppMenu()  → the "Subtitle Edit" app menu dropdown only
///   - SetMenu(Window, menu)      → avnWindow.SetMainMenu() → the full NSMenuBar with separate menus
///
/// Two-phase design:
///   SetupAppMenu + MakeStructure: called from Program.cs after SetupMainWindow.
///     The Window-level menu creates the full NSMenuBar (File, Edit, etc.).
///     The Application-level menu populates the App menu (About, Preferences).
///   Sync: called from OnLoaded once the ViewModel is available to wire up
///     initial states, gestures, PropertyChanged subscriptions, and dynamic submenus.
/// </summary>
public static class InitNativeMacMenu
{
    private static readonly List<(Func<MainViewModel, IRelayCommand> GetCmd, NativeMenuItem Item)> _gestureItems = [];
    // IsVisible maps directly to NSMenuItem.setHidden: via Avalonia's SetIsVisible vtable slot.
    // IsEnabled has no equivalent vtable slot — it only reaches the native layer through the
    // SetAction predicate, which macOS never calls for container items (submenu, no action).
    // All items here were IsVisible bindings in InitMenu.cs, so IsVisible is the correct mapping.
    private static readonly List<(NativeMenuItem Item, Func<MainViewModel, bool> IsVisible, string[] Props)> _visibilities = [];
    private static readonly List<(NativeMenuItem Item, Func<MainViewModel, bool> IsChecked, string[] Props)> _toggles = [];
    private static readonly List<(NativeMenuItem Item, Func<MainViewModel, string> GetHeader, string[] Props)> _dynamicHeaders = [];
    private static PropertyChangedEventHandler? _handler;

    private static NativeMenuItem? _reopenItem;
    private static NativeMenuItem? _pluginsItem;
    private static NativeMenuItem? _audioTracksItem;

    // The real VM instance — set by Sync(), used by all lazy click handlers.
    // MainViewModel is AddTransient in DI, so GetService() returns a new instance;
    // we must hold a reference to the specific instance that owns the main window.
    private static MainViewModel? _vm;

    private static Window? _window;
    private static NativeMenu? _root;

    // ── App menu ─────────────────────────────────────────────────────────────
    // Called from Program.cs. Sets the "Subtitle Edit" app dropdown contents.
    // Avalonia also auto-appends the standard macOS items (Hide, Quit, etc.).

    public static void SetupAppMenu(Application app)
    {
        var appMenu = new NativeMenu();

        var aboutItem = new NativeMenuItem(Clean(Se.Language.Help.AboutSubtitleEdit));
        aboutItem.Click += (_, _) => GetVm()?.ShowAboutCommand.Execute(null);
        appMenu.Items.Add(aboutItem);

        var checkForUpdatesItem = new NativeMenuItem(Clean(Se.Language.Main.Menu.CheckForUpdates));
        checkForUpdatesItem.Click += (_, _) => GetVm()?.ShowCheckForUpdatesCommand.Execute(null);
        appMenu.Items.Add(checkForUpdatesItem);
        appMenu.Items.Add(new NativeMenuItemSeparator());

        var prefsItem = new NativeMenuItem(Clean(Se.Language.Main.Menu.Settings));
        prefsItem.Gesture = new KeyGesture(Key.OemComma, KeyModifiers.Meta);
        prefsItem.Click += (_, _) => GetVm()?.CommandShowSettingsCommand.Execute(null);
        appMenu.Items.Add(prefsItem);

        NativeMenu.SetMenu(app, appMenu);
    }

    // ── Full NSMenuBar ────────────────────────────────────────────────────────
    // MakeStructure populates the root NativeMenu (passed in from Program.cs)
    // with all top-level menus. Called before NativeMenu.SetMenu(window, root).
    // Commands use lazy VM resolution since the ViewModel may not be ready yet.

    public static void MakeStructure(NativeMenu root, Window window)
    {
        _root = root;
        _window = window;
        _gestureItems.Clear();
        _visibilities.Clear();
        _toggles.Clear();
        _dynamicHeaders.Clear();

        var l = Se.Language.Main.Menu;

        // ── File ──────────────────────────────────────────────────────────────
        var fileItems = new NativeMenu();
        fileItems.Items.Add(Item(Clean(l.New), v => v.CommandFileNewCommand));
        fileItems.Items.Add(Conditional(Clean(l.NewKeepVideo), v => v.CommandFileNewKeepVideoCommand,
            v => v.IsVideoLoaded, nameof(MainViewModel.IsVideoLoaded)));
        fileItems.Items.Add(new NativeMenuItemSeparator());
        fileItems.Items.Add(Item(Clean(l.Open), v => v.CommandFileOpenCommand));
        fileItems.Items.Add(Conditional(Clean(l.OpenKeepVideo), v => v.CommandFileOpenKeepVideoCommand,
            v => v.IsVideoLoaded, nameof(MainViewModel.IsVideoLoaded)));
        fileItems.Items.Add(Conditional(Clean(l.OpenOriginal), v => v.FileOpenOriginalCommand,
            v => !v.ShowColumnOriginalText, nameof(MainViewModel.ShowColumnOriginalText)));
        fileItems.Items.Add(Conditional(Clean(l.CloseOriginal), v => v.FileCloseOriginalCommand,
            v => v.ShowColumnOriginalText, nameof(MainViewModel.ShowColumnOriginalText)));
        fileItems.Items.Add(Conditional(Clean(l.CloseTranslation), v => v.FileCloseTranslationCommand,
            v => v.ShowColumnOriginalText, nameof(MainViewModel.ShowColumnOriginalText)));

        _reopenItem = new NativeMenuItem(Clean(l.Reopen)) { Menu = new NativeMenu() };
        fileItems.Items.Add(_reopenItem);
        fileItems.Items.Add(Item(Clean(l.RestoreAutoBackup), v => v.ShowRestoreAutoBackupCommand));
        fileItems.Items.Add(new NativeMenuItemSeparator());
        fileItems.Items.Add(Item(Clean(l.Save), v => v.CommandFileSaveCommand));
        fileItems.Items.Add(Item(Clean(l.SaveAs), v => v.CommandFileSaveAsCommand));
        fileItems.Items.Add(new NativeMenuItemSeparator());

        var filePropsItem = new NativeMenuItem(string.Empty);
        filePropsItem.Click += (_, _) => GetVm()?.FilePropertiesShowCommand.Execute(null);
        _dynamicHeaders.Add((filePropsItem, v => Clean(v.FilePropertiesText), [nameof(MainViewModel.FilePropertiesText)]));
        _visibilities.Add((filePropsItem, v => v.IsFilePropertiesVisible, [nameof(MainViewModel.IsFilePropertiesVisible)]));
        fileItems.Items.Add(filePropsItem);

        fileItems.Items.Add(Item(Clean(l.OpenContainingFolder), v => v.OpenContainingFolderCommand));
        fileItems.Items.Add(Item(Clean(l.Compare), v => v.ShowCompareCommand));
        fileItems.Items.Add(Item(Clean(l.Statistics), v => v.ShowStatisticsCommand));
        fileItems.Items.Add(new NativeMenuItemSeparator());

        var lImport = Se.Language.File.Import;
        var importItems = new NativeMenu();
        importItems.Items.Add(Item(Clean(lImport.SubtitleWithManuallyChosenEncodingDotDotDot), v => v.ShowImportSubtitleWithManuallyChosenEncodingCommand));
        importItems.Items.Add(new NativeMenuItemSeparator());
        importItems.Items.Add(Item(Clean(lImport.ImagedBasedSubtitleForOcrDotDotDot), v => v.ImportImageSubtitleForOcrCommand));
        importItems.Items.Add(Item(Clean(lImport.ImagedBasedSubtitleForEditDotDotDot), v => v.ImportImageSubtitleForEditCommand));
        importItems.Items.Add(Item(Clean(lImport.ImagesForOcrDotDotDot), v => v.ImportImagesCommand));
        importItems.Items.Add(Item(Clean(lImport.PlainTextDotDotDot), v => v.ImportPlainTextCommand));
        importItems.Items.Add(Item(Clean(lImport.CsvXlsxCustomColumnsDotDotDot), v => v.ImportCsvXlsxCustomColumnsCommand));
        importItems.Items.Add(new NativeMenuItemSeparator());
        importItems.Items.Add(Item(Clean(lImport.TimeCodesDotDotDot), v => v.ImportTimeCodesCommand));
        importItems.Items.Add(Item(Clean(lImport.FormattingDotDotDot), v => v.ImportStylingCommand));
        fileItems.Items.Add(new NativeMenuItem(Clean(l.Import)) { Menu = importItems });

        var lExport = Se.Language.File.Export;
        var exportItems = new NativeMenu();
        exportItems.Items.Add(Item(Se.Language.General.BluRaySup, v => v.ExportBluRaySupCommand));
        exportItems.Items.Add(Item(Se.Language.General.BdnXml, v => v.ExportBdnXmlCommand));
        exportItems.Items.Add(Item(new CapMakerPlus().Name, v => v.ExportCapMakerPlusCommand));
        exportItems.Items.Add(Item(CheetahCaption.NameOfFormat, v => v.ExportCheetahCaptionCommand));
        exportItems.Items.Add(Item(CheetahCaptionOld.NameOfFormat, v => v.ExportCheetahCaptionOldCommand));
        exportItems.Items.Add(Item(Cavena890.NameOfFormat, v => v.ExportCavena890Command));
        exportItems.Items.Add(Item(lExport.TitleExportDCinemaInteropPng, v => v.ExportDCinemaInteropPngCommand));
        exportItems.Items.Add(Item(lExport.TitleExportDCinemaSmpte2014Png, v => v.ExportDCinemaSmpte2014PngCommand));
        exportItems.Items.Add(Item(Ebu.NameOfFormat, v => v.ExportEbuStlCommand));
        exportItems.Items.Add(Item("DOST/png", v => v.ExportDostPngCommand));
        exportItems.Items.Add(Item("FCP/png", v => v.ExportFcpPngCommand));
        exportItems.Items.Add(Item(Se.Language.General.ImagesWithTimeCode, v => v.ExportImagesWithTimeCodeCommand));
        exportItems.Items.Add(Item(Pac.NameOfFormat, v => v.ExportPacCommand));
        exportItems.Items.Add(Item(new PacUnicode().Name, v => v.ExportPacUnicodeCommand));
        exportItems.Items.Add(Item(lExport.TitleExportVobSub, v => v.ExportVobSubCommand));
        exportItems.Items.Add(Item("WebVTT png", v => v.ExportWebVttThumbnailsCommand));
        exportItems.Items.Add(new NativeMenuItemSeparator());
        exportItems.Items.Add(Item(Clean(lExport.CustomTextFormatsDotDotDot), v => v.ShowExportCustomTextFormatCommand));
        exportItems.Items.Add(Item(Clean(lExport.PlainTextDotDotDot), v => v.ShowExportPlainTextCommand));
        fileItems.Items.Add(new NativeMenuItem(Clean(l.Export)) { Menu = exportItems });

        // ── Edit ──────────────────────────────────────────────────────────────
        var editItems = new NativeMenu();
        editItems.Items.Add(Item(Clean(l.Undo), v => v.UndoCommand));
        editItems.Items.Add(Item(Clean(l.Redo), v => v.RedoCommand));
        editItems.Items.Add(Item(Clean(l.ShowHistory), v => v.ShowHistoryCommand));
        editItems.Items.Add(new NativeMenuItemSeparator());
        editItems.Items.Add(Item(Clean(l.Find), v => v.ShowFindCommand));
        editItems.Items.Add(Item(Clean(l.FindNext), v => v.FindNextCommand));
        editItems.Items.Add(Item(Clean(l.Replace), v => v.ShowReplaceCommand));
        editItems.Items.Add(Item(Clean(l.MultipleReplace), v => v.ShowMultipleReplaceCommand));
        editItems.Items.Add(Item(Clean(l.GoToLineNumber), v => v.ShowGoToLineCommand));
        editItems.Items.Add(new NativeMenuItemSeparator());
        editItems.Items.Add(Toggle(Clean(l.RightToLeftMode), v => v.RightToLeftToggleCommand,
            v => v.IsRightToLeftEnabled, nameof(MainViewModel.IsRightToLeftEnabled)));
        editItems.Items.Add(Item(Clean(l.FixRightToLeftViaUnicodeControlCharacters), v => v.FixRightToLeftViaUnicodeControlCharactersCommand));
        editItems.Items.Add(Item(Clean(l.RemoveUnicodeControlCharacters), v => v.RemoveUnicodeControlCharactersCommand));
        editItems.Items.Add(Item(Clean(l.ReverseRightToLeftStartEnd), v => v.ReverseRightToLeftStartEndCommand));
        editItems.Items.Add(new NativeMenuItemSeparator());
        editItems.Items.Add(Item(Clean(l.ModifySelectionDotDotDot), v => v.ShowModifySelectionCommand));
        editItems.Items.Add(Item(Clean(Se.Language.General.InvertSelection), v => v.InverseSelectionCommand));
        editItems.Items.Add(Item(Clean(Se.Language.General.SelectAll), v => v.SelectAllLinesCommand));

        // ── Tools ─────────────────────────────────────────────────────────────
        var toolsList = new List<NativeMenuItem>
        {
            Item(Clean(l.AdjustDurations), v => v.ShowToolsAdjustDurationsCommand),
            Item(Clean(l.ApplyDurationLimits), v => v.ShowApplyDurationLimitsCommand),
            Item(Clean(l.BatchConvert), v => v.ShowToolsBatchConvertCommand),
            Item(Clean(l.BeautifyTimeCodes), v => v.ShowBeautifyTimeCodesCommand),
            Item(Clean(l.BridgeGaps), v => v.ShowBridgeGapsCommand),
            Item(Clean(l.ApplyMinGap), v => v.ShowApplyMinGapCommand),
            Item(Clean(l.ChangeCasing), v => v.ShowToolsChangeCasingCommand),
            Item(Clean(l.ChangeFormatting), v => v.ShowToolsChangeFormattingCommand),
            Item(Clean(l.FixCommonErrors), v => v.ShowToolsFixCommonErrorsCommand),
            Item(Clean(l.CheckAndFixNetflixErrors), v => v.ShowToolsFixNetflixErrorsCommand),
            Item(Clean(l.MakeEmptyTranslationFromCurrentSubtitle), v => v.ToolsMakeEmptyTranslationFromCurrentSubtitleCommand),
            Item(Clean(l.MergeLinesWithSameText), v => v.ShowToolsMergeLinesWithSameTextCommand),
            Item(Clean(l.MergeLinesWithSameTimeCodes), v => v.ShowToolsMergeLinesWithSameTimeCodesCommand),
            Item(Clean(l.SplitBreakLongLines), v => v.ShowToolsSplitBreakLongLinesCommand),
            Item(Clean(l.MergeShortLines), v => v.ShowToolsMergeShortLinesCommand),
            Item(Clean(l.MergeContinuationLines), v => v.ShowToolsMergeContinuationLinesCommand),
            Item(Clean(l.SnapAllTimesToFrames), v => v.SnapAllTimesToFramesCommand),
            Item(Clean(l.MergeTwoSubtitles), v => v.ShowToolsMergeTwoSubtitlesCommand),
            Item(Clean(l.SortSubtitles), v => v.ShowSortByCommand),
            Item(Clean(l.Renumber), v => v.ShowToolsRenumberCommand),
            Item(Clean(l.RemoveTextForHearingImpaired), v => v.ShowToolsRemoveTextForHearingImpairedCommand),
            Item(Clean(l.ConvertActors), v => v.ShowToolsConvertActorsCommand),
        };
        var toolItems = new NativeMenu();
        foreach (var toolItem in toolsList.OrderBy(i => i.Header?.Replace("_", string.Empty)))
            toolItems.Items.Add(toolItem);
        toolItems.Items.Add(new NativeMenuItemSeparator());
        toolItems.Items.Add(Item(Clean(l.JoinSubtitles), v => v.ShowToolsJoinCommand));
        toolItems.Items.Add(Item(Clean(l.SplitSubtitle), v => v.ShowToolsSplitCommand));

        // ── Plugins ───────────────────────────────────────────────────────────
        _pluginsItem = new NativeMenuItem(Clean(Se.Language.Plugins.Title)) { Menu = new NativeMenu() };

        // ── Spell Check ───────────────────────────────────────────────────────
        var spellItems = new NativeMenu();
        spellItems.Items.Add(Item(Clean(l.SpellCheck), v => v.ShowSpellCheckCommand));
        spellItems.Items.Add(Item(Clean(l.FindDoubleWords), v => v.ShowFindDoubleWordsCommand));
        spellItems.Items.Add(Item(Clean(l.FindDoubleLines), v => v.ShowFindDoubleLinesCommand));
        spellItems.Items.Add(new NativeMenuItemSeparator());
        spellItems.Items.Add(Item(Clean(l.AddNameToNamesList), v => v.ShowAddToNameListCommand));
        spellItems.Items.Add(Item(Clean(l.GetDictionaries), v => v.ShowSpellCheckDictionariesCommand));

        // ── Video ─────────────────────────────────────────────────────────────
        var videoItems = new NativeMenu();
        videoItems.Items.Add(Item(Clean(l.OpenVideo), v => v.CommandVideoOpenCommand));
        videoItems.Items.Add(Item(Clean(l.OpenVideoFromUrl), v => v.ShowVideoOpenFromUrlCommand));
        videoItems.Items.Add(Item(Clean(l.CloseVideoFile), v => v.CommandVideoCloseCommand));

        _audioTracksItem = new NativeMenuItem(Clean(l.AudioTracks)) { Menu = new NativeMenu() };
        _visibilities.Add((_audioTracksItem, v => v.IsAudioTracksVisible, [nameof(MainViewModel.IsAudioTracksVisible)]));
        videoItems.Items.Add(_audioTracksItem);

        videoItems.Items.Add(new NativeMenuItemSeparator());
        videoItems.Items.Add(Item(Clean(l.SpeechToText), v => v.ShowSpeechToTextWhisperCommand));
        videoItems.Items.Add(Item(Clean(l.TextToSpeech), v => v.ShowVideoTextToSpeechCommand));
        videoItems.Items.Add(new NativeMenuItemSeparator());
        videoItems.Items.Add(Item(Clean(l.GenerateBurnIn), v => v.ShowVideoBurnInCommand));
        videoItems.Items.Add(Item(Clean(l.GenerateTransparent), v => v.ShowVideoTransparentSubtitlesCommand));
        videoItems.Items.Add(Item(Clean(Se.Language.Video.GenerateBlankVideoDotDotDot), v => v.VideoGenerateBlankCommand));
        videoItems.Items.Add(Item(Clean(Se.Language.Video.EmbedSubtitlesDotDotDot), v => v.VideoEmbedCommand));
        videoItems.Items.Add(new NativeMenuItemSeparator());
        videoItems.Items.Add(Item(Clean(l.GenerateImportShotChanges), v => v.ShowShotChangesSubtitlesCommand));
        videoItems.Items.Add(Conditional(Clean(l.ListShotChanges), v => v.ShowShotChangesListCommand,
            v => v.ShowShotChangesListMenuItem, nameof(MainViewModel.ShowShotChangesListMenuItem)));
        videoItems.Items.Add(new NativeMenuItemSeparator());
        videoItems.Items.Add(Conditional(Clean(l.UndockVideoControls), v => v.VideoUndockControlsCommand,
            v => !v.AreVideoControlsUndocked, nameof(MainViewModel.AreVideoControlsUndocked)));
        videoItems.Items.Add(Conditional(Clean(l.DockVideoControls), v => v.VideoRedockControlsCommand,
            v => v.AreVideoControlsUndocked, nameof(MainViewModel.AreVideoControlsUndocked)));
        videoItems.Items.Add(Toggle(Clean(l.ToggleSelectSubtitleWhilePlayingCurrentlyOff), v => v.ToggleCurrentSubtitleWhilePlayingCommand,
            v => v.SelectCurrentSubtitleWhilePlaying, nameof(MainViewModel.SelectCurrentSubtitleWhilePlaying)));

        var lVideo = Se.Language.Video;
        var videoMoreList = new List<NativeMenuItem>
        {
            Conditional(Clean(lVideo.OpenSecondarySubtitleOnVideoPlayerDotDotDot), v => v.OpenSecondarySubtitleCommand,
                v => !v.IsSubtitleSecondaryVisible, nameof(MainViewModel.IsSubtitleSecondaryVisible)),
            Conditional(Clean(lVideo.RemoveSecondarySubtitleOnVideoPlayer), v => v.ClearSecondarySubtitleCommand,
                v => v.IsSubtitleSecondaryVisible, nameof(MainViewModel.IsSubtitleSecondaryVisible)),
            Item(Clean(lVideo.ReEncodeVideoForBetterSubtitlingDotDotDot), v => v.VideoReEncodeCommand),
            Item(Clean(lVideo.CutVideoDotDotDot), v => v.VideoCutCommand),
        };
        videoMoreList.Add(Toggle(Clean(Se.Language.Options.Shortcuts.ToggleWaveformToolbar), v => v.ToggleIsWaveformToolbarVisibleCommand,
            v => v.IsWaveformToolbarVisible, nameof(MainViewModel.IsWaveformToolbarVisible)));

        var setOffsetItem = new NativeMenuItem(string.Empty);
        setOffsetItem.Click += (_, _) => GetVm()?.ShowVideoSetOffsetCommand.Execute(null);
        _dynamicHeaders.Add((setOffsetItem, v => Clean(v.SetVideoOffsetText), [nameof(MainViewModel.SetVideoOffsetText)]));
        videoMoreList.Add(setOffsetItem);

        videoMoreList.Add(Toggle(Clean(l.SmpteTiming), v => v.ToggleSmpteTimingCommand,
            v => v.IsSmpteTimingEnabled, nameof(MainViewModel.IsSmpteTimingEnabled)));

        var videoMoreItems = new NativeMenu();
        foreach (var item in videoMoreList.OrderBy(i => i.Header?.TrimStart('_', ' ')))
            videoMoreItems.Items.Add(item);
        var videoMoreMenu = new NativeMenuItem(Clean(Se.Language.General.More)) { Menu = videoMoreItems };
        _visibilities.Add((videoMoreMenu, v => v.IsVideoLoaded, [nameof(MainViewModel.IsVideoLoaded)]));
        videoItems.Items.Add(videoMoreMenu);

        // ── Synchronization ───────────────────────────────────────────────────
        var syncItems = new NativeMenu();
        syncItems.Items.Add(Item(Clean(l.AdjustAllTimes), v => v.ShowSyncAdjustAllTimesCommand));
        syncItems.Items.Add(Item(Clean(l.VisualSync), v => v.ShowVisualSyncCommand));
        syncItems.Items.Add(Item(Clean(l.PointSync), v => v.ShowPointSyncCommand));
        syncItems.Items.Add(Item(Clean(l.PointSyncViaOther), v => v.ShowPointSyncViaOtherCommand));
        syncItems.Items.Add(Item(Clean(l.ChangeFrameRate), v => v.ShowSyncChangeFrameRateCommand));
        syncItems.Items.Add(Item(Clean(l.ChangeSpeed), v => v.ShowSyncChangeSpeedCommand));

        // ── Translate ─────────────────────────────────────────────────────────
        var translateItems = new NativeMenu();
        translateItems.Items.Add(Item(Clean(l.AutoTranslate), v => v.ShowAutoTranslateCommand));
        translateItems.Items.Add(Item(Clean(l.TranslateViaCopyPaste), v => v.ShowTranslateViaCopyPasteCommand));

        // ── Options ───────────────────────────────────────────────────────────
        var optionsItems = new NativeMenu();
        optionsItems.Items.Add(Item(Clean(l.Shortcuts), v => v.CommandShowSettingsShortcutsCommand));
        optionsItems.Items.Add(Item(Clean(l.WordLists), v => v.ShowWordListsCommand));
        optionsItems.Items.Add(Item(Clean(l.ChooseLanguage), v => v.CommandShowSettingsLanguageCommand));

        // ── Help ──────────────────────────────────────────────────────────────
        var helpItems = new NativeMenu();
        helpItems.Items.Add(Item($"{Clean(Se.Language.Title)} {Clean(l.HelpTitle)}", v => v.ShowHelpCommand));

        // ── ASSA Tools ────────────────────────────────────────────────────────
        var assaList = new List<NativeMenuItem>
        {
            Item(Clean(l.AssaProgressBar), v => v.ShowAssaGenerateProgressBarCommand),
            Item(Clean(l.AssaChangeResolution), v => v.ShowAssaChangeResolutionCommand),
            Item(Clean(l.AssaGenerateBackground), v => v.ShowAssaGenerateBackgroundCommand),
            Item(Clean(l.AssaImageColorPicker), v => v.ShowAssaImageColorPickerCommand),
            Item(Clean(l.AssaSetPosition), v => v.ShowAssaSetPositionCommand),
            Item(Clean(l.AssaApplyAdvancedEffects), v => v.ShowAssaApplyAdvancedEffectCommand),
            Item(Clean(l.AssaApplyCustomOverrideTags), v => v.ShowAssaApplyCustomOverrideTagsCommand),
            Item(Clean(l.AssaDraw), v => v.ShowAssaDrawCommand),
            Item(Clean(l.AssaProperties), v => v.ShowAssaPropertiesCommand),
            Item(Clean(l.AssaAttachments), v => v.ShowAssaAttachmentsCommand),
            Item(Clean(l.AssaStyles), v => v.ShowAssaStylesCommand),
        };
        var assaItems = new NativeMenu();
        foreach (var assaItem in assaList.OrderBy(i => i.Header?.Replace("_", string.Empty)))
            assaItems.Items.Add(assaItem);
        assaItems.Items.Add(new NativeMenuItemSeparator());
        assaItems.Items.Add(Item(Clean(l.FilterLayersForDisplayDotDotDot), v => v.ShowPickLayerFilterCommand));
        var assaMenu = new NativeMenuItem(Clean(l.AssaTools)) { Menu = assaItems };
        _visibilities.Add((assaMenu, v => v.IsFormatAssa, [nameof(MainViewModel.IsFormatAssa)]));

        // ── Assemble ──────────────────────────────────────────────────────────
        root.Items.Add(new NativeMenuItem(Clean(l.File)) { Menu = fileItems });
        root.Items.Add(new NativeMenuItem(Clean(l.Edit)) { Menu = editItems });
        root.Items.Add(new NativeMenuItem(Clean(l.Tools)) { Menu = toolItems });
        root.Items.Add(_pluginsItem);
        root.Items.Add(new NativeMenuItem(Clean(l.SpellCheckTitle)) { Menu = spellItems });
        root.Items.Add(new NativeMenuItem(Clean(l.Video)) { Menu = videoItems });
        root.Items.Add(new NativeMenuItem(Clean(l.Synchronization)) { Menu = syncItems });
        root.Items.Add(new NativeMenuItem(Clean(l.Translate)) { Menu = translateItems });
        root.Items.Add(new NativeMenuItem(Clean(l.Options)) { Menu = optionsItems });
        root.Items.Add(new NativeMenuItem(Clean(l.HelpTitle)) { Menu = helpItems });
        root.Items.Add(assaMenu);
    }

    // Called from LoadLanguage to rebuild all menu strings after a language switch.
    public static void Rebuild(MainViewModel vm)
    {
        if (_root == null || _window == null)
            return;
        _root.Items.Clear();
        MakeStructure(_root, _window);
        NativeMenu.SetMenu(_window, _root);
        if (Application.Current != null)
            SetupAppMenu(Application.Current);
        Sync(vm);
    }

    // ── Phase 2 ──────────────────────────────────────────────────────────────
    // Called from MainViewModel.OnLoaded. Applies initial states, gestures,
    // PropertyChanged subscriptions, and dynamic submenus.

    public static void Sync(MainViewModel vm)
    {
        if (_handler != null && _vm != null)
            _vm.PropertyChanged -= _handler;

        _vm = vm;

        vm.NativeMenuReopen = _reopenItem;
        vm.NativeMenuPlugins = _pluginsItem;
        vm.NativeMenuAudioTracks = _audioTracksItem;

        var shortcuts = ShortcutsMain.GetUsedShortcuts(vm);
        foreach (var (getCmd, item) in _gestureItems)
            item.Gesture = FindGesture(getCmd(vm), shortcuts);

        foreach (var (item, isVisible, _) in _visibilities)
            item.IsVisible = isVisible(vm);

        foreach (var (item, isChecked, _) in _toggles)
            item.IsChecked = isChecked(vm);

        foreach (var (item, getHeader, _) in _dynamicHeaders)
            item.Header = getHeader(vm);

        if (_pluginsItem != null)
            _pluginsItem.IsEnabled = Se.Settings.Appearance.ShowPluginsMenu;

        _handler = (s, e) =>
        {
            if (s is not MainViewModel v2 || e.PropertyName is null)
                return;
            foreach (var (item, isVisible, props) in _visibilities)
                if (props.Contains(e.PropertyName)) item.IsVisible = isVisible(v2);
            foreach (var (item, isChecked, props) in _toggles)
                if (props.Contains(e.PropertyName)) item.IsChecked = isChecked(v2);
            foreach (var (item, getHeader, props) in _dynamicHeaders)
                if (props.Contains(e.PropertyName)) item.Header = getHeader(v2);
        };
        vm.PropertyChanged += _handler;

        UpdateRecentFiles(vm);
        UpdatePluginsMenu(vm);
    }

    // ── Dynamic submenu updaters ──────────────────────────────────────────────

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
            var captured = file;
            recentItem.Click += (_, _) => vm.CommandFileReopenCommand.Execute(captured);
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
        var shortcuts = ShortcutsMain.GetUsedShortcuts(vm);
        foreach (var (getCmd, item) in _gestureItems)
            item.Gesture = FindGesture(getCmd(vm), shortcuts);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static MainViewModel? GetVm() => _vm;

    private static NativeMenuItem Item(string? header, Func<MainViewModel, IRelayCommand> getCmd)
    {
        var item = new NativeMenuItem(header ?? string.Empty);
        item.Click += (_, _) => { var v = GetVm(); if (v != null) getCmd(v).Execute(null); };
        _gestureItems.Add((getCmd, item));
        return item;
    }

    private static NativeMenuItem Conditional(string? header, Func<MainViewModel, IRelayCommand> getCmd,
        Func<MainViewModel, bool> isVisible, params string[] propertyNames)
    {
        var item = Item(header, getCmd);
        _visibilities.Add((item, isVisible, propertyNames));
        return item;
    }

    private static NativeMenuItem Toggle(string? header, Func<MainViewModel, IRelayCommand> getCmd,
        Func<MainViewModel, bool> isChecked, params string[] propertyNames)
    {
        var item = new NativeMenuItem(header ?? string.Empty);
        item.ToggleType = MenuItemToggleType.CheckBox;
        item.Click += (_, _) => { var v = GetVm(); if (v != null) getCmd(v).Execute(null); };
        _toggles.Add((item, isChecked, propertyNames));
        _gestureItems.Add((getCmd, item));
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

    private static string Clean(string? s) => s?.Replace("_", string.Empty) ?? string.Empty;
}
