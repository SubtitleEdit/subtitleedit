namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class Main
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            if (disposing)
            {
                if (_networkSession != null)
                {
                    _networkSession.Dispose();
                    _networkSession = null;
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            Nikse.SubtitleEdit.Core.TimeCode timeCode1 = new Nikse.SubtitleEdit.Core.TimeCode();
            Nikse.SubtitleEdit.Core.TimeCode timeCode2 = new Nikse.SubtitleEdit.Core.TimeCode();
            Nikse.SubtitleEdit.Core.TimeCode timeCode3 = new Nikse.SubtitleEdit.Core.TimeCode();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.labelStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripSelected = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelProgress = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusNetworking = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButtonFileNew = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonFileOpen = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonSave = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonSaveAs = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparatorFindReplace = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonFind = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonReplace = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparatorFixSyncSpell = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonFixCommonErrors = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonRemoveTextForHi = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonVisualSync = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonSpellCheck = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonNetflixQualityCheck = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonSettings = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparatorHelp = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonHelp = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparatorToggle = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonToggleWaveform = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonToggleVideo = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparatorSubtitleFormat = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabelSubtitleFormat = new System.Windows.Forms.ToolStripLabel();
            this.comboBoxSubtitleFormats = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripSeparatorEncoding = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabelEncoding = new System.Windows.Forms.ToolStripLabel();
            this.comboBoxEncoding = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripSeparatorFrameRate = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabelFrameRate = new System.Windows.Forms.ToolStripLabel();
            this.toolStripComboBoxFrameRate = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripButtonGetFrameRate = new System.Windows.Forms.ToolStripButton();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemOpenKeepVideo = new System.Windows.Forms.ToolStripMenuItem();
            this.reopenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemRestoreAutoBackup = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemDCinemaProperties = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemTTProperties = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemNuendoProperties = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemFcpProperties = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemSubStationAlpha = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemEbuProperties = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemDvdStudioProProperties = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator20 = new System.Windows.Forms.ToolStripSeparator();
            this.openOriginalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveOriginalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveOriginalAstoolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeOriginalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator12 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemOpenContainingFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemCompare = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemStatistics = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemPlugins = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemImportDvdSubtitles = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemSubIdx = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemImportBluRaySup = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemImportXSub = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemImportOcrHardSub = new System.Windows.Forms.ToolStripMenuItem();
            this.matroskaImportStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemManualAnsi = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemImportText = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemImportImages = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemImportTimeCodes = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator22 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemExport = new System.Windows.Forms.ToolStripMenuItem();
            this.adobeEncoreFABImageScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemAvidStl = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemExportAyato = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemExportPngXml = new System.Windows.Forms.ToolStripMenuItem();
            this.bluraySupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemExportBdTextSt = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemExportCapMakerPlus = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemExportCaptionInc = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemCavena890 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemExportCheetahCap = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemExportDcinemaInterop = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemDost = new System.Windows.Forms.ToolStripMenuItem();
            this.DvdStudioProStl = new System.Windows.Forms.ToolStripMenuItem();
            this.eBUSTLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemEdl = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemEdlClipName = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemExportFcpIImage = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemFcpXmlAdvanced = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemImagePerFrame = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemTextTimeCodePair = new System.Windows.Forms.ToolStripMenuItem();
            this.pACScreenElectronicsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.uniPacExportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.plainTextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemSpumux = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemExportUltech130 = new System.Windows.Forms.ToolStripMenuItem();
            this.vobSubsubidxToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparatorExportCustomText = new System.Windows.Forms.ToolStripSeparator();
            this.exportCustomTextFormatToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemUndo = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemRedo = new System.Windows.Forms.ToolStripMenuItem();
            this.showHistoryforUndoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator14 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemInsertUnicodeCharacter = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparatorInsertUnicodeCharacter = new System.Windows.Forms.ToolStripSeparator();
            this.findToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findNextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.replaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.multipleReplaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gotoLineNumberToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemShowOriginalInPreview = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator25 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemRightToLeftMode = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemRtlUnicodeControlChars = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemReverseRightToLeftStartEnd = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator21 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemModifySelection = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemInverseSelection = new System.Windows.Forms.ToolStripMenuItem();
            this.editSelectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.adjustDisplayTimeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemApplyDurationLimits = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemDurationBridgeGaps = new System.Windows.Forms.ToolStripMenuItem();
            this.fixToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startNumberingFromToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeTextForHearImpairedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ChangeCasingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemAutoMergeShortLines = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemMergeDuplicateText = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemMergeLinesWithSameTimeCodes = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemAutoSplitLongLines = new System.Windows.Forms.ToolStripMenuItem();
            this.setMinimumDisplayTimeBetweenParagraphsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemSortBy = new System.Windows.Forms.ToolStripMenuItem();
            this.sortNumberToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sortStartTimeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sortEndTimeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sortDisplayTimeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sortTextAlphabeticallytoolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sortTextMaxLineLengthToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sortTextTotalLengthToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sortTextNumberOfLinesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.textCharssecToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.textWordsPerMinutewpmToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.actorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.styleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparatorAscOrDesc = new System.Windows.Forms.ToolStripSeparator();
            this.AscendingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.descendingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.netflixQualityCheckToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator23 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemMakeEmptyFromCurrent = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemBatchConvert = new System.Windows.Forms.ToolStripMenuItem();
            this.generateDatetimeInfoFromVideoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemMeasurementConverter = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.splitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.appendTextVisuallyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.joinSubtitlesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemSpellCheckMain = new System.Windows.Forms.ToolStripMenuItem();
            this.spellCheckToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemSpellCheckFromCurrentLine = new System.Windows.Forms.ToolStripMenuItem();
            this.findDoubleWordsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.FindDoubleLinesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.GetDictionariesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addWordToNameListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemVideo = new System.Windows.Forms.ToolStripMenuItem();
            this.openVideoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemOpenVideoFromUrl = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemOpenDvd = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemSetAudioTrack = new System.Windows.Forms.ToolStripMenuItem();
            this.closeVideoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setVideoOffsetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.smpteTimeModedropFrameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemImportSceneChanges = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemRemoveSceneChanges = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemAddWaveformBatch = new System.Windows.Forms.ToolStripMenuItem();
            this.generateTextFromCurrentVideoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.showhideWaveformToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showhideVideoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator19 = new System.Windows.Forms.ToolStripSeparator();
            this.undockVideoControlsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.redockVideoControlsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemSynchronization = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemAdjustAllTimes = new System.Windows.Forms.ToolStripMenuItem();
            this.visualSyncToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemPointSync = new System.Windows.Forms.ToolStripMenuItem();
            this.pointSyncViaOtherSubtitleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemChangeFrameRate2 = new System.Windows.Forms.ToolStripMenuItem();
            this.changeSpeedInPercentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemAutoTranslate = new System.Windows.Forms.ToolStripMenuItem();
            this.translatepoweredByMicrosoftToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.translateByGoogleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.translateFromSwedishToDanishToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changeLanguageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemNetworking = new System.Windows.Forms.ToolStripMenuItem();
            this.startServerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.joinSessionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.chatToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showSessionKeyLogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.leaveSessionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkForUpdatesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemSplitterCheckForUpdates = new System.Windows.Forms.ToolStripSeparator();
            this.helpToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStripListview = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.setStylesForSelectedLinesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setActorForSelectedLinesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemAssStyles = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemSetRegion = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemSetLanguage = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemWebVTT = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemInsertBefore = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemInsertAfter = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemInsertSubtitle = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemCopySourceText = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemColumn = new System.Windows.Forms.ToolStripMenuItem();
            this.columnDeleteTextOnlyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemColumnDeleteText = new System.Windows.Forms.ToolStripMenuItem();
            this.ShiftTextCellsDownToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemInsertTextFromSub = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemColumnImportText = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemPasteSpecial = new System.Windows.Forms.ToolStripMenuItem();
            this.copyOriginalTextToCurrentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveTextUpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveTextDownToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemBookmark = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.splitLineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemMergeLines = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemMergeDialog = new System.Windows.Forms.ToolStripMenuItem();
            this.mergeBeforeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mergeAfterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.removeFormattinglToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeAllFormattingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeBoldToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeItalicToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeUnderlineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeColorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeFontNameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeAlignmentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.boldToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.italicToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.boxToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.underlineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.colorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemFont = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemAlignment = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemSurroundWithMusicSymbols = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemAutoBreakLines = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemUnbreakLines = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparatorBreakLines = new System.Windows.Forms.ToolStripSeparator();
            this.typeEffectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.karokeeEffectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparatorAdvancedFunctions = new System.Windows.Forms.ToolStripSeparator();
            this.showSelectedLinesEarlierlaterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.visualSyncSelectedLinesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemGoogleMicrosoftTranslateSelLine = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemTranslateSelected = new System.Windows.Forms.ToolStripMenuItem();
            this.googleTranslateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.microsoftBingTranslateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.adjustDisplayTimeForSelectedLinesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fixCommonErrorsInSelectedLinesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changeCasingForSelectedLinesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemSaveSelectedLines = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.groupBoxVideo = new System.Windows.Forms.GroupBox();
            this.labelNextWord = new System.Windows.Forms.Label();
            this.audioVisualizer = new Nikse.SubtitleEdit.Controls.AudioVisualizer();
            this.checkBoxSyncListViewWithVideoWhilePlaying = new System.Windows.Forms.CheckBox();
            this.labelVideoInfo = new System.Windows.Forms.Label();
            this.trackBarWaveformPosition = new System.Windows.Forms.TrackBar();
            this.panelWaveformControls = new System.Windows.Forms.Panel();
            this.toolStripWaveControls = new System.Windows.Forms.ToolStrip();
            this.toolStripButtonWaveformZoomOut = new System.Windows.Forms.ToolStripButton();
            this.toolStripComboBoxWaveform = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripButtonWaveformZoomIn = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator16 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonWaveformPause = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonWaveformPlay = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonLockCenter = new System.Windows.Forms.ToolStripButton();
            this.toolStripSplitButtonPlayRate = new System.Windows.Forms.ToolStripSplitButton();
            this.tabControlButtons = new System.Windows.Forms.TabControl();
            this.tabPageTranslate = new System.Windows.Forms.TabPage();
            this.labelTranslateTip = new System.Windows.Forms.Label();
            this.groupBoxTranslateSearch = new System.Windows.Forms.GroupBox();
            this.buttonCustomUrl2 = new System.Windows.Forms.Button();
            this.buttonCustomUrl1 = new System.Windows.Forms.Button();
            this.buttonGoogleTranslateIt = new System.Windows.Forms.Button();
            this.buttonGoogleIt = new System.Windows.Forms.Button();
            this.textBoxSearchWord = new System.Windows.Forms.TextBox();
            this.groupBoxAutoContinue = new System.Windows.Forms.GroupBox();
            this.comboBoxAutoContinue = new System.Windows.Forms.ComboBox();
            this.labelAutoContinueDelay = new System.Windows.Forms.Label();
            this.checkBoxAutoContinue = new System.Windows.Forms.CheckBox();
            this.buttonStop = new System.Windows.Forms.Button();
            this.groupBoxAutoRepeat = new System.Windows.Forms.GroupBox();
            this.comboBoxAutoRepeat = new System.Windows.Forms.ComboBox();
            this.labelAutoRepeatCount = new System.Windows.Forms.Label();
            this.checkBoxAutoRepeatOn = new System.Windows.Forms.CheckBox();
            this.buttonPlayPrevious = new System.Windows.Forms.Button();
            this.buttonPlayCurrent = new System.Windows.Forms.Button();
            this.buttonPlayNext = new System.Windows.Forms.Button();
            this.tabPageCreate = new System.Windows.Forms.TabPage();
            this.timeUpDownVideoPosition = new Nikse.SubtitleEdit.Controls.TimeUpDown();
            this.buttonGotoSub = new System.Windows.Forms.Button();
            this.buttonBeforeText = new System.Windows.Forms.Button();
            this.buttonSetEnd = new System.Windows.Forms.Button();
            this.buttonInsertNewText = new System.Windows.Forms.Button();
            this.buttonSetStartTime = new System.Windows.Forms.Button();
            this.labelCreateF12 = new System.Windows.Forms.Label();
            this.labelCreateF11 = new System.Windows.Forms.Label();
            this.labelCreateF10 = new System.Windows.Forms.Label();
            this.labelCreateF9 = new System.Windows.Forms.Label();
            this.buttonForward2 = new System.Windows.Forms.Button();
            this.numericUpDownSec2 = new System.Windows.Forms.NumericUpDown();
            this.buttonSecBack2 = new System.Windows.Forms.Button();
            this.buttonForward1 = new System.Windows.Forms.Button();
            this.numericUpDownSec1 = new System.Windows.Forms.NumericUpDown();
            this.labelVideoPosition = new System.Windows.Forms.Label();
            this.buttonSecBack1 = new System.Windows.Forms.Button();
            this.tabPageAdjust = new System.Windows.Forms.TabPage();
            this.timeUpDownVideoPositionAdjust = new Nikse.SubtitleEdit.Controls.TimeUpDown();
            this.buttonAdjustSetEndTime = new System.Windows.Forms.Button();
            this.buttonSetEndAndGoToNext = new System.Windows.Forms.Button();
            this.buttonSetStartAndOffsetRest = new System.Windows.Forms.Button();
            this.buttonAdjustSetStartTime = new System.Windows.Forms.Button();
            this.labelAdjustF12 = new System.Windows.Forms.Label();
            this.labelAdjustF11 = new System.Windows.Forms.Label();
            this.labelAdjustF10 = new System.Windows.Forms.Label();
            this.labelAdjustF9 = new System.Windows.Forms.Label();
            this.buttonAdjustSecForward2 = new System.Windows.Forms.Button();
            this.numericUpDownSecAdjust2 = new System.Windows.Forms.NumericUpDown();
            this.buttonAdjustSecBack2 = new System.Windows.Forms.Button();
            this.buttonAdjustSecForward1 = new System.Windows.Forms.Button();
            this.numericUpDownSecAdjust1 = new System.Windows.Forms.NumericUpDown();
            this.buttonAdjustSecBack1 = new System.Windows.Forms.Button();
            this.labelVideoPosition2 = new System.Windows.Forms.Label();
            this.buttonAdjustGoToPosAndPause = new System.Windows.Forms.Button();
            this.buttonAdjustPlayBefore = new System.Windows.Forms.Button();
            this.ShowSubtitleTimer = new System.Windows.Forms.Timer(this.components);
            this.timerAutoDuration = new System.Windows.Forms.Timer(this.components);
            this.timerAutoContinue = new System.Windows.Forms.Timer(this.components);
            this.timerStillTyping = new System.Windows.Forms.Timer(this.components);
            this.timerWaveform = new System.Windows.Forms.Timer(this.components);
            this.contextMenuStripWaveform = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addParagraphHereToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addParagraphAndPasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemSetParagraphAsSelection = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemFocusTextbox = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteParagraphToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.mergeWithPreviousToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mergeWithNextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemWaveformPlaySelection = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator24 = new System.Windows.Forms.ToolStripSeparator();
            this.showWaveformAndSpectrogramToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showOnlyWaveformToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showOnlySpectrogramToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparatorGuessTimeCodes = new System.Windows.Forms.ToolStripSeparator();
            this.removeSceneChangeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addSceneChangeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.guessTimeCodesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.seekSilenceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.insertSubtitleHereToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainerMain = new System.Windows.Forms.SplitContainer();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tabControlSubtitle = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.splitContainerListViewAndText = new System.Windows.Forms.SplitContainer();
            this.SubtitleListview1 = new Nikse.SubtitleEdit.Controls.SubtitleListView();
            this.imageListBookmarks = new System.Windows.Forms.ImageList(this.components);
            this.groupBoxEdit = new System.Windows.Forms.GroupBox();
            this.panelBookmark = new System.Windows.Forms.Panel();
            this.labelBookmark = new System.Windows.Forms.Label();
            this.pictureBoxBookmark = new System.Windows.Forms.PictureBox();
            this.labelSingleLine = new System.Windows.Forms.Label();
            this.labelAlternateSingleLine = new System.Windows.Forms.Label();
            this.labelDurationWarning = new System.Windows.Forms.Label();
            this.labelStartTimeWarning = new System.Windows.Forms.Label();
            this.buttonSplitLine = new System.Windows.Forms.Button();
            this.labelAlternateCharactersPerSecond = new System.Windows.Forms.Label();
            this.labelTextAlternateLineTotal = new System.Windows.Forms.Label();
            this.labelTextAlternateLineLengths = new System.Windows.Forms.Label();
            this.labelAlternateText = new System.Windows.Forms.Label();
            this.labelText = new System.Windows.Forms.Label();
            this.textBoxListViewTextAlternate = new Nikse.SubtitleEdit.Controls.SETextBox();
            this.contextMenuStripTextBoxListView = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemWebVttVoice = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparatorWebVTT = new System.Windows.Forms.ToolStripSeparator();
            this.cutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemSplitTextAtCursor = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemSplitViaWaveform = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator18 = new System.Windows.Forms.ToolStripSeparator();
            this.selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator17 = new System.Windows.Forms.ToolStripSeparator();
            this.normalToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.boldToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.italicToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.underlineToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.colorToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemHorizontalDigits = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemBouten = new System.Windows.Forms.ToolStripMenuItem();
            this.boutendotbeforeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.boutendotafterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.boutendotoutsideToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.boutenfilledcircleoutsideToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.boutenopencircleoutsideToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.boutenopendotoutsideToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.boutenfilledsesameoutsideToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.boutenopensesameoutsideToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.boutenautooutsideToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.boutenautoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemRuby = new System.Windows.Forms.ToolStripMenuItem();
            this.fontNameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator26 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemInsertUnicodeSymbol = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemInsertUnicodeControlCharacters = new System.Windows.Forms.ToolStripMenuItem();
            this.leftToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.righttoleftMarkToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startOfLefttorightEmbeddingLREToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startOfRighttoleftEmbeddingRLEToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startOfLefttorightOverrideLROToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startOfRighttoleftOverrideRLOToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.superscriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.subscriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonAutoBreak = new System.Windows.Forms.Button();
            this.labelTextLineLengths = new System.Windows.Forms.Label();
            this.labelTextLineTotal = new System.Windows.Forms.Label();
            this.labelCharactersPerSecond = new System.Windows.Forms.Label();
            this.buttonUnBreak = new System.Windows.Forms.Button();
            this.timeUpDownStartTime = new Nikse.SubtitleEdit.Controls.TimeUpDown();
            this.numericUpDownDuration = new System.Windows.Forms.NumericUpDown();
            this.buttonPrevious = new System.Windows.Forms.Button();
            this.buttonNext = new System.Windows.Forms.Button();
            this.labelStartTime = new System.Windows.Forms.Label();
            this.textBoxListViewText = new Nikse.SubtitleEdit.Controls.SETextBox();
            this.labelDuration = new System.Windows.Forms.Label();
            this.labelAutoDuration = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.textBoxSource = new System.Windows.Forms.TextBox();
            this.panelVideoPlayer = new System.Windows.Forms.Panel();
            this.mediaPlayer = new Nikse.SubtitleEdit.Controls.VideoPlayerContainer();
            this.contextMenuStripEmpty = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.insertLineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.imageListPlayRate = new System.Windows.Forms.ImageList(this.components);
            this.timerTextUndo = new System.Windows.Forms.Timer(this.components);
            this.timerAlternateTextUndo = new System.Windows.Forms.Timer(this.components);
            this.statusStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.contextMenuStripListview.SuspendLayout();
            this.groupBoxVideo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarWaveformPosition)).BeginInit();
            this.panelWaveformControls.SuspendLayout();
            this.toolStripWaveControls.SuspendLayout();
            this.tabControlButtons.SuspendLayout();
            this.tabPageTranslate.SuspendLayout();
            this.groupBoxTranslateSearch.SuspendLayout();
            this.groupBoxAutoContinue.SuspendLayout();
            this.groupBoxAutoRepeat.SuspendLayout();
            this.tabPageCreate.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSec2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSec1)).BeginInit();
            this.tabPageAdjust.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSecAdjust2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSecAdjust1)).BeginInit();
            this.contextMenuStripWaveform.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).BeginInit();
            this.splitContainerMain.Panel1.SuspendLayout();
            this.splitContainerMain.Panel2.SuspendLayout();
            this.splitContainerMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabControlSubtitle.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerListViewAndText)).BeginInit();
            this.splitContainerListViewAndText.Panel1.SuspendLayout();
            this.splitContainerListViewAndText.Panel2.SuspendLayout();
            this.splitContainerListViewAndText.SuspendLayout();
            this.groupBoxEdit.SuspendLayout();
            this.panelBookmark.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxBookmark)).BeginInit();
            this.contextMenuStripTextBoxListView.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDuration)).BeginInit();
            this.tabPage2.SuspendLayout();
            this.panelVideoPlayer.SuspendLayout();
            this.contextMenuStripEmpty.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.labelStatus,
            this.toolStripSelected,
            this.toolStripStatusLabelProgress,
            this.toolStripStatusNetworking});
            this.statusStrip1.Location = new System.Drawing.Point(0, 624);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(975, 22);
            this.statusStrip1.TabIndex = 4;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // labelStatus
            // 
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(0, 17);
            this.labelStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.labelStatus.Click += new System.EventHandler(this.labelStatus_Click);
            // 
            // toolStripSelected
            // 
            this.toolStripSelected.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripSelected.Name = "toolStripSelected";
            this.toolStripSelected.Size = new System.Drawing.Size(746, 17);
            this.toolStripSelected.Spring = true;
            this.toolStripSelected.Text = "toolStripSelected";
            this.toolStripSelected.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.toolStripSelected.Click += new System.EventHandler(this.toolStripSelected_Click);
            // 
            // toolStripStatusLabelProgress
            // 
            this.toolStripStatusLabelProgress.Name = "toolStripStatusLabelProgress";
            this.toolStripStatusLabelProgress.Size = new System.Drawing.Size(166, 17);
            this.toolStripStatusLabelProgress.Text = "toolStripStatusLabelProgress";
            this.toolStripStatusLabelProgress.Visible = false;
            // 
            // toolStripStatusNetworking
            // 
            this.toolStripStatusNetworking.Image = global::Nikse.SubtitleEdit.Properties.Resources.connect;
            this.toolStripStatusNetworking.Name = "toolStripStatusNetworking";
            this.toolStripStatusNetworking.Padding = new System.Windows.Forms.Padding(50, 0, 0, 0);
            this.toolStripStatusNetworking.Size = new System.Drawing.Size(214, 17);
            this.toolStripStatusNetworking.Text = "toolStripStatusNetworking";
            this.toolStripStatusNetworking.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.toolStripStatusNetworking.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            this.toolStripStatusNetworking.Click += new System.EventHandler(this.toolStripStatusNetworking_Click);
            // 
            // toolStrip1
            // 
            this.toolStrip1.AutoSize = false;
            this.toolStrip1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonFileNew,
            this.toolStripButtonFileOpen,
            this.toolStripButtonSave,
            this.toolStripButtonSaveAs,
            this.toolStripSeparatorFindReplace,
            this.toolStripButtonFind,
            this.toolStripButtonReplace,
            this.toolStripSeparatorFixSyncSpell,
            this.toolStripButtonFixCommonErrors,
            this.toolStripButtonRemoveTextForHi,
            this.toolStripButtonVisualSync,
            this.toolStripButtonSpellCheck,
            this.toolStripButtonNetflixQualityCheck,
            this.toolStripButtonSettings,
            this.toolStripSeparatorHelp,
            this.toolStripButtonHelp,
            this.toolStripSeparatorToggle,
            this.toolStripButtonToggleWaveform,
            this.toolStripButtonToggleVideo,
            this.toolStripSeparatorSubtitleFormat,
            this.toolStripLabelSubtitleFormat,
            this.comboBoxSubtitleFormats,
            this.toolStripSeparatorEncoding,
            this.toolStripLabelEncoding,
            this.comboBoxEncoding,
            this.toolStripSeparatorFrameRate,
            this.toolStripLabelFrameRate,
            this.toolStripComboBoxFrameRate,
            this.toolStripButtonGetFrameRate});
            this.toolStrip1.Location = new System.Drawing.Point(0, 24);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(975, 40);
            this.toolStrip1.TabIndex = 5;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButtonFileNew
            // 
            this.toolStripButtonFileNew.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonFileNew.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripButtonFileNew.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonFileNew.Image")));
            this.toolStripButtonFileNew.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonFileNew.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.toolStripButtonFileNew.Name = "toolStripButtonFileNew";
            this.toolStripButtonFileNew.Size = new System.Drawing.Size(36, 37);
            this.toolStripButtonFileNew.Text = "New";
            this.toolStripButtonFileNew.ToolTipText = "New";
            this.toolStripButtonFileNew.Click += new System.EventHandler(this.ToolStripButtonFileNewClick);
            // 
            // toolStripButtonFileOpen
            // 
            this.toolStripButtonFileOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonFileOpen.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripButtonFileOpen.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonFileOpen.Image")));
            this.toolStripButtonFileOpen.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonFileOpen.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.toolStripButtonFileOpen.Name = "toolStripButtonFileOpen";
            this.toolStripButtonFileOpen.Size = new System.Drawing.Size(36, 37);
            this.toolStripButtonFileOpen.Text = "toolStripButtonOpen";
            this.toolStripButtonFileOpen.ToolTipText = "Open";
            this.toolStripButtonFileOpen.Click += new System.EventHandler(this.ToolStripButtonFileOpenClick);
            // 
            // toolStripButtonSave
            // 
            this.toolStripButtonSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonSave.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripButtonSave.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonSave.Image")));
            this.toolStripButtonSave.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonSave.Name = "toolStripButtonSave";
            this.toolStripButtonSave.Size = new System.Drawing.Size(36, 37);
            this.toolStripButtonSave.Text = "toolStripButtonSave";
            this.toolStripButtonSave.ToolTipText = "Save";
            this.toolStripButtonSave.Click += new System.EventHandler(this.ToolStripButtonSaveClick);
            // 
            // toolStripButtonSaveAs
            // 
            this.toolStripButtonSaveAs.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonSaveAs.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripButtonSaveAs.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonSaveAs.Image")));
            this.toolStripButtonSaveAs.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonSaveAs.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonSaveAs.Name = "toolStripButtonSaveAs";
            this.toolStripButtonSaveAs.Size = new System.Drawing.Size(36, 37);
            this.toolStripButtonSaveAs.Text = "toolStripButtonSaveAs";
            this.toolStripButtonSaveAs.ToolTipText = "Save as";
            this.toolStripButtonSaveAs.Click += new System.EventHandler(this.ToolStripButtonSaveAsClick);
            // 
            // toolStripSeparatorFindReplace
            // 
            this.toolStripSeparatorFindReplace.Name = "toolStripSeparatorFindReplace";
            this.toolStripSeparatorFindReplace.Size = new System.Drawing.Size(6, 40);
            // 
            // toolStripButtonFind
            // 
            this.toolStripButtonFind.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonFind.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripButtonFind.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonFind.Image")));
            this.toolStripButtonFind.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonFind.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonFind.Name = "toolStripButtonFind";
            this.toolStripButtonFind.Size = new System.Drawing.Size(36, 37);
            this.toolStripButtonFind.Text = "Find";
            this.toolStripButtonFind.Click += new System.EventHandler(this.ToolStripButtonFindClick);
            // 
            // toolStripButtonReplace
            // 
            this.toolStripButtonReplace.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonReplace.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripButtonReplace.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonReplace.Image")));
            this.toolStripButtonReplace.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonReplace.ImageTransparentColor = System.Drawing.Color.White;
            this.toolStripButtonReplace.Name = "toolStripButtonReplace";
            this.toolStripButtonReplace.Size = new System.Drawing.Size(36, 37);
            this.toolStripButtonReplace.Text = "Replace";
            this.toolStripButtonReplace.ToolTipText = "Replace";
            this.toolStripButtonReplace.Click += new System.EventHandler(this.ToolStripButtonReplaceClick);
            // 
            // toolStripSeparatorFixSyncSpell
            // 
            this.toolStripSeparatorFixSyncSpell.Name = "toolStripSeparatorFixSyncSpell";
            this.toolStripSeparatorFixSyncSpell.Size = new System.Drawing.Size(6, 40);
            // 
            // toolStripButtonFixCommonErrors
            // 
            this.toolStripButtonFixCommonErrors.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonFixCommonErrors.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripButtonFixCommonErrors.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonFixCommonErrors.Image")));
            this.toolStripButtonFixCommonErrors.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonFixCommonErrors.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonFixCommonErrors.Name = "toolStripButtonFixCommonErrors";
            this.toolStripButtonFixCommonErrors.Size = new System.Drawing.Size(36, 37);
            this.toolStripButtonFixCommonErrors.Text = "Fix common errors";
            this.toolStripButtonFixCommonErrors.ToolTipText = "Fix common errors";
            this.toolStripButtonFixCommonErrors.Click += new System.EventHandler(this.toolStripButtonFixCommonErrors_Click);
            // 
            // toolStripButtonRemoveTextForHi
            // 
            this.toolStripButtonRemoveTextForHi.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonRemoveTextForHi.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripButtonRemoveTextForHi.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonRemoveTextForHi.Image")));
            this.toolStripButtonRemoveTextForHi.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonRemoveTextForHi.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonRemoveTextForHi.Name = "toolStripButtonRemoveTextForHi";
            this.toolStripButtonRemoveTextForHi.Size = new System.Drawing.Size(36, 37);
            this.toolStripButtonRemoveTextForHi.Text = "Remove text for HI";
            this.toolStripButtonRemoveTextForHi.ToolTipText = "Fix common errors";
            this.toolStripButtonRemoveTextForHi.Click += new System.EventHandler(this.toolStripButtonRemoveTextForHi_Click);
            // 
            // toolStripButtonVisualSync
            // 
            this.toolStripButtonVisualSync.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonVisualSync.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripButtonVisualSync.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonVisualSync.Image")));
            this.toolStripButtonVisualSync.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonVisualSync.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonVisualSync.Name = "toolStripButtonVisualSync";
            this.toolStripButtonVisualSync.Size = new System.Drawing.Size(36, 37);
            this.toolStripButtonVisualSync.Text = "Visual sync";
            this.toolStripButtonVisualSync.Click += new System.EventHandler(this.ToolStripButtonVisualSyncClick);
            // 
            // toolStripButtonSpellCheck
            // 
            this.toolStripButtonSpellCheck.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonSpellCheck.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.toolStripButtonSpellCheck.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonSpellCheck.Image")));
            this.toolStripButtonSpellCheck.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonSpellCheck.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonSpellCheck.Name = "toolStripButtonSpellCheck";
            this.toolStripButtonSpellCheck.Size = new System.Drawing.Size(36, 37);
            this.toolStripButtonSpellCheck.Text = "Spell check";
            this.toolStripButtonSpellCheck.Click += new System.EventHandler(this.ToolStripButtonSpellCheckClick);
            // 
            // toolStripButtonNetflixQualityCheck
            // 
            this.toolStripButtonNetflixQualityCheck.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonNetflixQualityCheck.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonNetflixQualityCheck.Image")));
            this.toolStripButtonNetflixQualityCheck.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonNetflixQualityCheck.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonNetflixQualityCheck.Name = "toolStripButtonNetflixQualityCheck";
            this.toolStripButtonNetflixQualityCheck.Size = new System.Drawing.Size(36, 37);
            this.toolStripButtonNetflixQualityCheck.Text = "Netflix quality check";
            this.toolStripButtonNetflixQualityCheck.Click += new System.EventHandler(this.toolStripButtonNetflixGlyphCheck_Click);
            // 
            // toolStripButtonSettings
            // 
            this.toolStripButtonSettings.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonSettings.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.toolStripButtonSettings.Image = global::Nikse.SubtitleEdit.Properties.Resources.Settings;
            this.toolStripButtonSettings.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonSettings.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.toolStripButtonSettings.Name = "toolStripButtonSettings";
            this.toolStripButtonSettings.Size = new System.Drawing.Size(36, 37);
            this.toolStripButtonSettings.Text = "Settings";
            this.toolStripButtonSettings.Click += new System.EventHandler(this.ToolStripButtonSettingsClick);
            // 
            // toolStripSeparatorHelp
            // 
            this.toolStripSeparatorHelp.Name = "toolStripSeparatorHelp";
            this.toolStripSeparatorHelp.Size = new System.Drawing.Size(6, 40);
            // 
            // toolStripButtonHelp
            // 
            this.toolStripButtonHelp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonHelp.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.toolStripButtonHelp.Image = global::Nikse.SubtitleEdit.Properties.Resources.Help;
            this.toolStripButtonHelp.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonHelp.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.toolStripButtonHelp.Name = "toolStripButtonHelp";
            this.toolStripButtonHelp.Size = new System.Drawing.Size(36, 37);
            this.toolStripButtonHelp.Text = "Help";
            this.toolStripButtonHelp.Click += new System.EventHandler(this.ToolStripButtonHelpClick);
            // 
            // toolStripSeparatorToggle
            // 
            this.toolStripSeparatorToggle.Name = "toolStripSeparatorToggle";
            this.toolStripSeparatorToggle.Size = new System.Drawing.Size(6, 40);
            // 
            // toolStripButtonToggleWaveform
            // 
            this.toolStripButtonToggleWaveform.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonToggleWaveform.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.toolStripButtonToggleWaveform.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonToggleWaveform.Image")));
            this.toolStripButtonToggleWaveform.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonToggleWaveform.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonToggleWaveform.Name = "toolStripButtonToggleWaveform";
            this.toolStripButtonToggleWaveform.Size = new System.Drawing.Size(36, 37);
            this.toolStripButtonToggleWaveform.Text = "Show/hide waveform";
            this.toolStripButtonToggleWaveform.Click += new System.EventHandler(this.toolStripButtonToggleWaveform_Click);
            // 
            // toolStripButtonToggleVideo
            // 
            this.toolStripButtonToggleVideo.Checked = true;
            this.toolStripButtonToggleVideo.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripButtonToggleVideo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonToggleVideo.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.toolStripButtonToggleVideo.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonToggleVideo.Image")));
            this.toolStripButtonToggleVideo.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonToggleVideo.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonToggleVideo.Name = "toolStripButtonToggleVideo";
            this.toolStripButtonToggleVideo.Size = new System.Drawing.Size(36, 37);
            this.toolStripButtonToggleVideo.Text = "Show/hide video";
            this.toolStripButtonToggleVideo.Click += new System.EventHandler(this.toolStripButtonToggleVideo_Click);
            // 
            // toolStripSeparatorSubtitleFormat
            // 
            this.toolStripSeparatorSubtitleFormat.Name = "toolStripSeparatorSubtitleFormat";
            this.toolStripSeparatorSubtitleFormat.Size = new System.Drawing.Size(6, 40);
            // 
            // toolStripLabelSubtitleFormat
            // 
            this.toolStripLabelSubtitleFormat.Name = "toolStripLabelSubtitleFormat";
            this.toolStripLabelSubtitleFormat.Size = new System.Drawing.Size(86, 37);
            this.toolStripLabelSubtitleFormat.Text = "Subtitle format";
            // 
            // comboBoxSubtitleFormats
            // 
            this.comboBoxSubtitleFormats.DropDownHeight = 215;
            this.comboBoxSubtitleFormats.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSubtitleFormats.FlatStyle = System.Windows.Forms.FlatStyle.Standard;
            this.comboBoxSubtitleFormats.IntegralHeight = false;
            this.comboBoxSubtitleFormats.Name = "comboBoxSubtitleFormats";
            this.comboBoxSubtitleFormats.Size = new System.Drawing.Size(150, 40);
            this.comboBoxSubtitleFormats.DropDown += new System.EventHandler(this.MenuOpened);
            this.comboBoxSubtitleFormats.DropDownClosed += new System.EventHandler(this.MenuClosed);
            this.comboBoxSubtitleFormats.SelectedIndexChanged += new System.EventHandler(this.ComboBoxSubtitleFormatsSelectedIndexChanged);
            this.comboBoxSubtitleFormats.Enter += new System.EventHandler(this.ComboBoxSubtitleFormatsEnter);
            // 
            // toolStripSeparatorEncoding
            // 
            this.toolStripSeparatorEncoding.Name = "toolStripSeparatorEncoding";
            this.toolStripSeparatorEncoding.Size = new System.Drawing.Size(6, 40);
            // 
            // toolStripLabelEncoding
            // 
            this.toolStripLabelEncoding.Name = "toolStripLabelEncoding";
            this.toolStripLabelEncoding.Size = new System.Drawing.Size(81, 37);
            this.toolStripLabelEncoding.Text = "File encoding";
            // 
            // comboBoxEncoding
            // 
            this.comboBoxEncoding.DropDownHeight = 215;
            this.comboBoxEncoding.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxEncoding.FlatStyle = System.Windows.Forms.FlatStyle.Standard;
            this.comboBoxEncoding.IntegralHeight = false;
            this.comboBoxEncoding.Items.AddRange(new object[] {
            "ANSI",
            "UTF-7",
            "UTF-8",
            "Unicode",
            "Unicode (big endian)"});
            this.comboBoxEncoding.Name = "comboBoxEncoding";
            this.comboBoxEncoding.Size = new System.Drawing.Size(129, 23);
            this.comboBoxEncoding.DropDown += new System.EventHandler(this.MenuOpened);
            this.comboBoxEncoding.DropDownClosed += new System.EventHandler(this.MenuClosed);
            // 
            // toolStripSeparatorFrameRate
            // 
            this.toolStripSeparatorFrameRate.Name = "toolStripSeparatorFrameRate";
            this.toolStripSeparatorFrameRate.Size = new System.Drawing.Size(6, 40);
            // 
            // toolStripLabelFrameRate
            // 
            this.toolStripLabelFrameRate.Name = "toolStripLabelFrameRate";
            this.toolStripLabelFrameRate.Size = new System.Drawing.Size(67, 15);
            this.toolStripLabelFrameRate.Text = "Frame rate";
            // 
            // toolStripComboBoxFrameRate
            // 
            this.toolStripComboBoxFrameRate.DropDownWidth = 75;
            this.toolStripComboBoxFrameRate.FlatStyle = System.Windows.Forms.FlatStyle.Standard;
            this.toolStripComboBoxFrameRate.Name = "toolStripComboBoxFrameRate";
            this.toolStripComboBoxFrameRate.Size = new System.Drawing.Size(75, 23);
            this.toolStripComboBoxFrameRate.DropDown += new System.EventHandler(this.MenuOpened);
            this.toolStripComboBoxFrameRate.DropDownClosed += new System.EventHandler(this.MenuClosed);
            this.toolStripComboBoxFrameRate.TextChanged += new System.EventHandler(this.ToolStripComboBoxFrameRateTextChanged);
            // 
            // toolStripButtonGetFrameRate
            // 
            this.toolStripButtonGetFrameRate.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButtonGetFrameRate.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonGetFrameRate.Image")));
            this.toolStripButtonGetFrameRate.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonGetFrameRate.Name = "toolStripButtonGetFrameRate";
            this.toolStripButtonGetFrameRate.Size = new System.Drawing.Size(23, 19);
            this.toolStripButtonGetFrameRate.Text = "...";
            this.toolStripButtonGetFrameRate.ToolTipText = "Get frame rate from video file";
            this.toolStripButtonGetFrameRate.Click += new System.EventHandler(this.ButtonGetFrameRateClick);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.toolStripMenuItemSpellCheckMain,
            this.toolStripMenuItemVideo,
            this.toolStripMenuItemSynchronization,
            this.toolStripMenuItemAutoTranslate,
            this.optionsToolStripMenuItem,
            this.toolStripMenuItemNetworking,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(975, 24);
            this.menuStrip1.TabIndex = 6;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.openToolStripMenuItem,
            this.toolStripMenuItemOpenKeepVideo,
            this.reopenToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.toolStripMenuItemRestoreAutoBackup,
            this.toolStripMenuItemDCinemaProperties,
            this.toolStripMenuItemTTProperties,
            this.toolStripMenuItemNuendoProperties,
            this.toolStripMenuItemFcpProperties,
            this.toolStripMenuItemSubStationAlpha,
            this.toolStripMenuItemEbuProperties,
            this.toolStripMenuItemDvdStudioProProperties,
            this.toolStripSeparator20,
            this.openOriginalToolStripMenuItem,
            this.saveOriginalToolStripMenuItem,
            this.saveOriginalAstoolStripMenuItem,
            this.removeOriginalToolStripMenuItem,
            this.toolStripSeparator12,
            this.toolStripMenuItemOpenContainingFolder,
            this.toolStripMenuItemCompare,
            this.toolStripMenuItemStatistics,
            this.toolStripMenuItemPlugins,
            this.toolStripSeparator1,
            this.toolStripMenuItemImportDvdSubtitles,
            this.toolStripMenuItemSubIdx,
            this.toolStripMenuItemImportBluRaySup,
            this.toolStripMenuItemImportXSub,
            this.toolStripMenuItemImportOcrHardSub,
            this.matroskaImportStripMenuItem,
            this.toolStripMenuItemManualAnsi,
            this.toolStripMenuItemImportText,
            this.toolStripMenuItemImportImages,
            this.toolStripMenuItemImportTimeCodes,
            this.toolStripSeparator22,
            this.toolStripMenuItemExport,
            this.toolStripSeparator10,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.fileToolStripMenuItem.Text = "File";
            this.fileToolStripMenuItem.DropDownOpening += new System.EventHandler(this.fileToolStripMenuItem_DropDownOpening);
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.newToolStripMenuItem.Size = new System.Drawing.Size(335, 22);
            this.newToolStripMenuItem.Text = "New";
            this.newToolStripMenuItem.Click += new System.EventHandler(this.NewToolStripMenuItemClick);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openToolStripMenuItem.Size = new System.Drawing.Size(335, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.OpenToolStripMenuItemClick);
            // 
            // toolStripMenuItemOpenKeepVideo
            // 
            this.toolStripMenuItemOpenKeepVideo.Name = "toolStripMenuItemOpenKeepVideo";
            this.toolStripMenuItemOpenKeepVideo.Size = new System.Drawing.Size(335, 22);
            this.toolStripMenuItemOpenKeepVideo.Text = "Open (keep video)";
            this.toolStripMenuItemOpenKeepVideo.Click += new System.EventHandler(this.toolStripMenuItemOpenKeepVideo_Click);
            // 
            // reopenToolStripMenuItem
            // 
            this.reopenToolStripMenuItem.Name = "reopenToolStripMenuItem";
            this.reopenToolStripMenuItem.Size = new System.Drawing.Size(335, 22);
            this.reopenToolStripMenuItem.Text = "Reopen";
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(335, 22);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.SaveToolStripMenuItemClick);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(335, 22);
            this.saveAsToolStripMenuItem.Text = "Save as...";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.SaveAsToolStripMenuItemClick);
            // 
            // toolStripMenuItemRestoreAutoBackup
            // 
            this.toolStripMenuItemRestoreAutoBackup.Name = "toolStripMenuItemRestoreAutoBackup";
            this.toolStripMenuItemRestoreAutoBackup.Size = new System.Drawing.Size(335, 22);
            this.toolStripMenuItemRestoreAutoBackup.Text = "Restore auto-backup...";
            this.toolStripMenuItemRestoreAutoBackup.Click += new System.EventHandler(this.toolStripMenuItemRestoreAutoBackup_Click);
            // 
            // toolStripMenuItemDCinemaProperties
            // 
            this.toolStripMenuItemDCinemaProperties.Name = "toolStripMenuItemDCinemaProperties";
            this.toolStripMenuItemDCinemaProperties.Size = new System.Drawing.Size(335, 22);
            this.toolStripMenuItemDCinemaProperties.Text = "DCinema properties...";
            this.toolStripMenuItemDCinemaProperties.Click += new System.EventHandler(this.toolStripMenuItemDCinemaProperties_Click);
            // 
            // toolStripMenuItemTTProperties
            // 
            this.toolStripMenuItemTTProperties.Name = "toolStripMenuItemTTProperties";
            this.toolStripMenuItemTTProperties.Size = new System.Drawing.Size(335, 22);
            this.toolStripMenuItemTTProperties.Text = "Timed Text properties...";
            this.toolStripMenuItemTTProperties.Click += new System.EventHandler(this.toolStripMenuItemTTPropertiesClick);
            // 
            // toolStripMenuItemNuendoProperties
            // 
            this.toolStripMenuItemNuendoProperties.Name = "toolStripMenuItemNuendoProperties";
            this.toolStripMenuItemNuendoProperties.Size = new System.Drawing.Size(335, 22);
            this.toolStripMenuItemNuendoProperties.Text = "Nuendo properties...";
            this.toolStripMenuItemNuendoProperties.Click += new System.EventHandler(this.ToolStripMenuItemNuendoPropertiesClick);
            // 
            // toolStripMenuItemFcpProperties
            // 
            this.toolStripMenuItemFcpProperties.Name = "toolStripMenuItemFcpProperties";
            this.toolStripMenuItemFcpProperties.Size = new System.Drawing.Size(335, 22);
            this.toolStripMenuItemFcpProperties.Text = "Final Cut Pro properties...";
            this.toolStripMenuItemFcpProperties.Click += new System.EventHandler(this.toolStripMenuItemFcpProperties_Click);
            // 
            // toolStripMenuItemSubStationAlpha
            // 
            this.toolStripMenuItemSubStationAlpha.Name = "toolStripMenuItemSubStationAlpha";
            this.toolStripMenuItemSubStationAlpha.Size = new System.Drawing.Size(335, 22);
            this.toolStripMenuItemSubStationAlpha.Text = "Advanced Sub Station Alpha properties...";
            this.toolStripMenuItemSubStationAlpha.Click += new System.EventHandler(this.toolStripMenuItemSubStationAlpha_Click);
            // 
            // toolStripMenuItemEbuProperties
            // 
            this.toolStripMenuItemEbuProperties.Name = "toolStripMenuItemEbuProperties";
            this.toolStripMenuItemEbuProperties.Size = new System.Drawing.Size(335, 22);
            this.toolStripMenuItemEbuProperties.Text = "Ebu properties...";
            this.toolStripMenuItemEbuProperties.Click += new System.EventHandler(this.toolStripMenuItemEbuProperties_Click);
            // 
            // toolStripMenuItemDvdStudioProProperties
            // 
            this.toolStripMenuItemDvdStudioProProperties.Name = "toolStripMenuItemDvdStudioProProperties";
            this.toolStripMenuItemDvdStudioProProperties.Size = new System.Drawing.Size(335, 22);
            this.toolStripMenuItemDvdStudioProProperties.Text = "DVD Studio Pro properties...";
            this.toolStripMenuItemDvdStudioProProperties.Click += new System.EventHandler(this.toolStripMenuDvdStudioProperties_Click);
            // 
            // toolStripSeparator20
            // 
            this.toolStripSeparator20.Name = "toolStripSeparator20";
            this.toolStripSeparator20.Size = new System.Drawing.Size(332, 6);
            // 
            // openOriginalToolStripMenuItem
            // 
            this.openOriginalToolStripMenuItem.Name = "openOriginalToolStripMenuItem";
            this.openOriginalToolStripMenuItem.Size = new System.Drawing.Size(335, 22);
            this.openOriginalToolStripMenuItem.Text = "Open original (translator mode)...";
            this.openOriginalToolStripMenuItem.Click += new System.EventHandler(this.OpenOriginalToolStripMenuItemClick);
            // 
            // saveOriginalToolStripMenuItem
            // 
            this.saveOriginalToolStripMenuItem.Name = "saveOriginalToolStripMenuItem";
            this.saveOriginalToolStripMenuItem.Size = new System.Drawing.Size(335, 22);
            this.saveOriginalToolStripMenuItem.Text = "Save original";
            this.saveOriginalToolStripMenuItem.Click += new System.EventHandler(this.SaveOriginalToolStripMenuItemClick);
            // 
            // saveOriginalAstoolStripMenuItem
            // 
            this.saveOriginalAstoolStripMenuItem.Name = "saveOriginalAstoolStripMenuItem";
            this.saveOriginalAstoolStripMenuItem.Size = new System.Drawing.Size(335, 22);
            this.saveOriginalAstoolStripMenuItem.Text = "Save original as...";
            this.saveOriginalAstoolStripMenuItem.Click += new System.EventHandler(this.SaveOriginalAstoolStripMenuItemClick);
            // 
            // removeOriginalToolStripMenuItem
            // 
            this.removeOriginalToolStripMenuItem.Name = "removeOriginalToolStripMenuItem";
            this.removeOriginalToolStripMenuItem.Size = new System.Drawing.Size(335, 22);
            this.removeOriginalToolStripMenuItem.Text = "Remove original";
            this.removeOriginalToolStripMenuItem.Click += new System.EventHandler(this.RemoveOriginalToolStripMenuItemClick);
            // 
            // toolStripSeparator12
            // 
            this.toolStripSeparator12.Name = "toolStripSeparator12";
            this.toolStripSeparator12.Size = new System.Drawing.Size(332, 6);
            // 
            // toolStripMenuItemOpenContainingFolder
            // 
            this.toolStripMenuItemOpenContainingFolder.Name = "toolStripMenuItemOpenContainingFolder";
            this.toolStripMenuItemOpenContainingFolder.Size = new System.Drawing.Size(335, 22);
            this.toolStripMenuItemOpenContainingFolder.Text = "Open containing folder";
            this.toolStripMenuItemOpenContainingFolder.Click += new System.EventHandler(this.toolStripMenuItemOpenContainingFolder_Click);
            // 
            // toolStripMenuItemCompare
            // 
            this.toolStripMenuItemCompare.Name = "toolStripMenuItemCompare";
            this.toolStripMenuItemCompare.Size = new System.Drawing.Size(335, 22);
            this.toolStripMenuItemCompare.Text = "Compare...";
            this.toolStripMenuItemCompare.Click += new System.EventHandler(this.ToolStripMenuItemCompareClick);
            // 
            // toolStripMenuItemStatistics
            // 
            this.toolStripMenuItemStatistics.Name = "toolStripMenuItemStatistics";
            this.toolStripMenuItemStatistics.Size = new System.Drawing.Size(335, 22);
            this.toolStripMenuItemStatistics.Text = "Statistics...";
            this.toolStripMenuItemStatistics.Click += new System.EventHandler(this.toolStripMenuItemStatistics_Click);
            // 
            // toolStripMenuItemPlugins
            // 
            this.toolStripMenuItemPlugins.Name = "toolStripMenuItemPlugins";
            this.toolStripMenuItemPlugins.Size = new System.Drawing.Size(335, 22);
            this.toolStripMenuItemPlugins.Text = "Plugins...";
            this.toolStripMenuItemPlugins.Click += new System.EventHandler(this.toolStripMenuItemPlugins_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(332, 6);
            // 
            // toolStripMenuItemImportDvdSubtitles
            // 
            this.toolStripMenuItemImportDvdSubtitles.Name = "toolStripMenuItemImportDvdSubtitles";
            this.toolStripMenuItemImportDvdSubtitles.Size = new System.Drawing.Size(335, 22);
            this.toolStripMenuItemImportDvdSubtitles.Text = "Import/OCR subtitle from VOB/IFO (DVD) ...";
            this.toolStripMenuItemImportDvdSubtitles.Click += new System.EventHandler(this.ToolStripMenuItemImportDvdSubtitlesClick);
            // 
            // toolStripMenuItemSubIdx
            // 
            this.toolStripMenuItemSubIdx.Name = "toolStripMenuItemSubIdx";
            this.toolStripMenuItemSubIdx.Size = new System.Drawing.Size(335, 22);
            this.toolStripMenuItemSubIdx.Text = "Import/OCR VobSub (sub/idx) subtitle...";
            this.toolStripMenuItemSubIdx.Click += new System.EventHandler(this.ToolStripMenuItemSubIdxClick1);
            // 
            // toolStripMenuItemImportBluRaySup
            // 
            this.toolStripMenuItemImportBluRaySup.Name = "toolStripMenuItemImportBluRaySup";
            this.toolStripMenuItemImportBluRaySup.Size = new System.Drawing.Size(335, 22);
            this.toolStripMenuItemImportBluRaySup.Text = "Import/OCR Blu-ray sup file...";
            this.toolStripMenuItemImportBluRaySup.Click += new System.EventHandler(this.toolStripMenuItemImportBluRaySup_Click);
            // 
            // toolStripMenuItemImportXSub
            // 
            this.toolStripMenuItemImportXSub.Name = "toolStripMenuItemImportXSub";
            this.toolStripMenuItemImportXSub.Size = new System.Drawing.Size(335, 22);
            this.toolStripMenuItemImportXSub.Text = "Import/OCR XSub from divx/avi...";
            this.toolStripMenuItemImportXSub.Click += new System.EventHandler(this.toolStripMenuItemImportXSub_Click);
            // 
            // toolStripMenuItemImportOcrHardSub
            // 
            this.toolStripMenuItemImportOcrHardSub.Name = "toolStripMenuItemImportOcrHardSub";
            this.toolStripMenuItemImportOcrHardSub.Size = new System.Drawing.Size(335, 22);
            this.toolStripMenuItemImportOcrHardSub.Text = "Import/OCR burned-in subtitles from video file...";
            this.toolStripMenuItemImportOcrHardSub.Click += new System.EventHandler(this.toolStripMenuItemImportOcrHardSub_Click);
            // 
            // matroskaImportStripMenuItem
            // 
            this.matroskaImportStripMenuItem.Name = "matroskaImportStripMenuItem";
            this.matroskaImportStripMenuItem.Size = new System.Drawing.Size(335, 22);
            this.matroskaImportStripMenuItem.Text = "Import subtitle from Matroska file...";
            this.matroskaImportStripMenuItem.Click += new System.EventHandler(this.MatroskaImportStripMenuItemClick);
            // 
            // toolStripMenuItemManualAnsi
            // 
            this.toolStripMenuItemManualAnsi.Name = "toolStripMenuItemManualAnsi";
            this.toolStripMenuItemManualAnsi.Size = new System.Drawing.Size(335, 22);
            this.toolStripMenuItemManualAnsi.Text = "Import subtitle with manually chosen encoding...";
            this.toolStripMenuItemManualAnsi.Click += new System.EventHandler(this.ToolStripMenuItemManualAnsiClick);
            // 
            // toolStripMenuItemImportText
            // 
            this.toolStripMenuItemImportText.Name = "toolStripMenuItemImportText";
            this.toolStripMenuItemImportText.Size = new System.Drawing.Size(335, 22);
            this.toolStripMenuItemImportText.Text = "Import text...";
            this.toolStripMenuItemImportText.Click += new System.EventHandler(this.ToolStripMenuItemImportTextClick);
            // 
            // toolStripMenuItemImportImages
            // 
            this.toolStripMenuItemImportImages.Name = "toolStripMenuItemImportImages";
            this.toolStripMenuItemImportImages.Size = new System.Drawing.Size(335, 22);
            this.toolStripMenuItemImportImages.Text = "Import images...";
            this.toolStripMenuItemImportImages.Click += new System.EventHandler(this.toolStripMenuItemImportImages_Click);
            // 
            // toolStripMenuItemImportTimeCodes
            // 
            this.toolStripMenuItemImportTimeCodes.Name = "toolStripMenuItemImportTimeCodes";
            this.toolStripMenuItemImportTimeCodes.Size = new System.Drawing.Size(335, 22);
            this.toolStripMenuItemImportTimeCodes.Text = "Import time codes into existing subtitle...";
            this.toolStripMenuItemImportTimeCodes.Click += new System.EventHandler(this.toolStripMenuItemImportTimeCodes_Click);
            // 
            // toolStripSeparator22
            // 
            this.toolStripSeparator22.Name = "toolStripSeparator22";
            this.toolStripSeparator22.Size = new System.Drawing.Size(332, 6);
            // 
            // toolStripMenuItemExport
            // 
            this.toolStripMenuItemExport.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.adobeEncoreFABImageScriptToolStripMenuItem,
            this.toolStripMenuItemAvidStl,
            this.toolStripMenuItemExportAyato,
            this.toolStripMenuItemExportPngXml,
            this.bluraySupToolStripMenuItem,
            this.toolStripMenuItemExportBdTextSt,
            this.toolStripMenuItemExportCapMakerPlus,
            this.toolStripMenuItemExportCaptionInc,
            this.toolStripMenuItemCavena890,
            this.toolStripMenuItemExportCheetahCap,
            this.toolStripMenuItemExportDcinemaInterop,
            this.toolStripMenuItemDost,
            this.DvdStudioProStl,
            this.eBUSTLToolStripMenuItem,
            this.toolStripMenuItemEdl,
            this.toolStripMenuItemEdlClipName,
            this.toolStripMenuItemExportFcpIImage,
            this.toolStripMenuItemFcpXmlAdvanced,
            this.toolStripMenuItemImagePerFrame,
            this.toolStripMenuItemTextTimeCodePair,
            this.pACScreenElectronicsToolStripMenuItem,
            this.uniPacExportToolStripMenuItem,
            this.plainTextToolStripMenuItem,
            this.toolStripMenuItemSpumux,
            this.toolStripMenuItemExportUltech130,
            this.vobSubsubidxToolStripMenuItem,
            this.toolStripSeparatorExportCustomText,
            this.exportCustomTextFormatToolStripMenuItem});
            this.toolStripMenuItemExport.Name = "toolStripMenuItemExport";
            this.toolStripMenuItemExport.Size = new System.Drawing.Size(335, 22);
            this.toolStripMenuItemExport.Text = "Export";
            // 
            // adobeEncoreFABImageScriptToolStripMenuItem
            // 
            this.adobeEncoreFABImageScriptToolStripMenuItem.Name = "adobeEncoreFABImageScriptToolStripMenuItem";
            this.adobeEncoreFABImageScriptToolStripMenuItem.Size = new System.Drawing.Size(255, 22);
            this.adobeEncoreFABImageScriptToolStripMenuItem.Text = "Adobe Encore FAB image script...";
            this.adobeEncoreFABImageScriptToolStripMenuItem.Click += new System.EventHandler(this.AdobeEncoreFabImageScriptToolStripMenuItemClick);
            // 
            // toolStripMenuItemAvidStl
            // 
            this.toolStripMenuItemAvidStl.Name = "toolStripMenuItemAvidStl";
            this.toolStripMenuItemAvidStl.Size = new System.Drawing.Size(255, 22);
            this.toolStripMenuItemAvidStl.Text = "Avid STL...";
            this.toolStripMenuItemAvidStl.Click += new System.EventHandler(this.toolStripMenuItemAvidStl_Click);
            // 
            // toolStripMenuItemExportAyato
            // 
            this.toolStripMenuItemExportAyato.Name = "toolStripMenuItemExportAyato";
            this.toolStripMenuItemExportAyato.Size = new System.Drawing.Size(255, 22);
            this.toolStripMenuItemExportAyato.Text = "Ayato...";
            this.toolStripMenuItemExportAyato.Click += new System.EventHandler(this.toolStripMenuItemExportAyato_Click);
            // 
            // toolStripMenuItemExportPngXml
            // 
            this.toolStripMenuItemExportPngXml.Name = "toolStripMenuItemExportPngXml";
            this.toolStripMenuItemExportPngXml.Size = new System.Drawing.Size(255, 22);
            this.toolStripMenuItemExportPngXml.Text = "BDN xml/png...";
            this.toolStripMenuItemExportPngXml.Click += new System.EventHandler(this.ToolStripMenuItemExportPngXmlClick);
            // 
            // bluraySupToolStripMenuItem
            // 
            this.bluraySupToolStripMenuItem.Name = "bluraySupToolStripMenuItem";
            this.bluraySupToolStripMenuItem.Size = new System.Drawing.Size(255, 22);
            this.bluraySupToolStripMenuItem.Text = "Blu-ray sup...";
            this.bluraySupToolStripMenuItem.Click += new System.EventHandler(this.BluraySupToolStripMenuItemClick);
            // 
            // toolStripMenuItemExportBdTextSt
            // 
            this.toolStripMenuItemExportBdTextSt.Name = "toolStripMenuItemExportBdTextSt";
            this.toolStripMenuItemExportBdTextSt.Size = new System.Drawing.Size(255, 22);
            this.toolStripMenuItemExportBdTextSt.Text = "Blu-ray TextST...";
            this.toolStripMenuItemExportBdTextSt.Click += new System.EventHandler(this.toolStripMenuItemExportBdTextSt_Click);
            // 
            // toolStripMenuItemExportCapMakerPlus
            // 
            this.toolStripMenuItemExportCapMakerPlus.Name = "toolStripMenuItemExportCapMakerPlus";
            this.toolStripMenuItemExportCapMakerPlus.Size = new System.Drawing.Size(255, 22);
            this.toolStripMenuItemExportCapMakerPlus.Text = "CapMaker Plus...";
            this.toolStripMenuItemExportCapMakerPlus.Click += new System.EventHandler(this.toolStripMenuItemExportCapMakerPlus_Click);
            // 
            // toolStripMenuItemExportCaptionInc
            // 
            this.toolStripMenuItemExportCaptionInc.Name = "toolStripMenuItemExportCaptionInc";
            this.toolStripMenuItemExportCaptionInc.Size = new System.Drawing.Size(255, 22);
            this.toolStripMenuItemExportCaptionInc.Text = "Captions Inc...";
            this.toolStripMenuItemExportCaptionInc.Click += new System.EventHandler(this.toolStripMenuItemExportCaptionInc_Click);
            // 
            // toolStripMenuItemCavena890
            // 
            this.toolStripMenuItemCavena890.Name = "toolStripMenuItemCavena890";
            this.toolStripMenuItemCavena890.Size = new System.Drawing.Size(255, 22);
            this.toolStripMenuItemCavena890.Text = "Cavena 890...";
            this.toolStripMenuItemCavena890.Click += new System.EventHandler(this.ToolStripMenuItemCavena890Click);
            // 
            // toolStripMenuItemExportCheetahCap
            // 
            this.toolStripMenuItemExportCheetahCap.Name = "toolStripMenuItemExportCheetahCap";
            this.toolStripMenuItemExportCheetahCap.Size = new System.Drawing.Size(255, 22);
            this.toolStripMenuItemExportCheetahCap.Text = "Cheetah CAP...";
            this.toolStripMenuItemExportCheetahCap.Click += new System.EventHandler(this.toolStripMenuItemExportCheetahCap_Click);
            // 
            // toolStripMenuItemExportDcinemaInterop
            // 
            this.toolStripMenuItemExportDcinemaInterop.Name = "toolStripMenuItemExportDcinemaInterop";
            this.toolStripMenuItemExportDcinemaInterop.Size = new System.Drawing.Size(255, 22);
            this.toolStripMenuItemExportDcinemaInterop.Text = "D-Cinema interop/png...";
            this.toolStripMenuItemExportDcinemaInterop.Click += new System.EventHandler(this.toolStripMenuItemExportDcinemaInteropClick);
            // 
            // toolStripMenuItemDost
            // 
            this.toolStripMenuItemDost.Name = "toolStripMenuItemDost";
            this.toolStripMenuItemDost.Size = new System.Drawing.Size(255, 22);
            this.toolStripMenuItemDost.Text = "DOST...";
            this.toolStripMenuItemDost.Click += new System.EventHandler(this.toolStripMenuItemDost_Click);
            // 
            // DvdStudioProStl
            // 
            this.DvdStudioProStl.Name = "DvdStudioProStl";
            this.DvdStudioProStl.Size = new System.Drawing.Size(255, 22);
            this.DvdStudioProStl.Text = "DVD Studio Pro STL";
            this.DvdStudioProStl.Click += new System.EventHandler(this.DvdStudioProStl_Click);
            // 
            // eBUSTLToolStripMenuItem
            // 
            this.eBUSTLToolStripMenuItem.Name = "eBUSTLToolStripMenuItem";
            this.eBUSTLToolStripMenuItem.Size = new System.Drawing.Size(255, 22);
            this.eBUSTLToolStripMenuItem.Text = "EBU STL...";
            this.eBUSTLToolStripMenuItem.Click += new System.EventHandler(this.EBustlToolStripMenuItemClick);
            // 
            // toolStripMenuItemEdl
            // 
            this.toolStripMenuItemEdl.Name = "toolStripMenuItemEdl";
            this.toolStripMenuItemEdl.Size = new System.Drawing.Size(255, 22);
            this.toolStripMenuItemEdl.Text = "EDL...";
            this.toolStripMenuItemEdl.Click += new System.EventHandler(this.ExportToEdl);
            // 
            // toolStripMenuItemEdlClipName
            // 
            this.toolStripMenuItemEdlClipName.Name = "toolStripMenuItemEdlClipName";
            this.toolStripMenuItemEdlClipName.Size = new System.Drawing.Size(255, 22);
            this.toolStripMenuItemEdlClipName.Text = "EDL/CLIPNAME...";
            this.toolStripMenuItemEdlClipName.Click += new System.EventHandler(this.ExportToEdlWithClipName);
            // 
            // toolStripMenuItemExportFcpIImage
            // 
            this.toolStripMenuItemExportFcpIImage.Name = "toolStripMenuItemExportFcpIImage";
            this.toolStripMenuItemExportFcpIImage.Size = new System.Drawing.Size(255, 22);
            this.toolStripMenuItemExportFcpIImage.Text = "Final Cut Pro + image...";
            this.toolStripMenuItemExportFcpIImage.Click += new System.EventHandler(this.toolStripMenuItemExportFcpIImage_Click);
            // 
            // toolStripMenuItemFcpXmlAdvanced
            // 
            this.toolStripMenuItemFcpXmlAdvanced.Name = "toolStripMenuItemFcpXmlAdvanced";
            this.toolStripMenuItemFcpXmlAdvanced.Size = new System.Drawing.Size(255, 22);
            this.toolStripMenuItemFcpXmlAdvanced.Text = "Final Cut Pro XML advanced...";
            this.toolStripMenuItemFcpXmlAdvanced.Click += new System.EventHandler(this.toolStripMenuItemFcpXmlAdvanced_Click);
            // 
            // toolStripMenuItemImagePerFrame
            // 
            this.toolStripMenuItemImagePerFrame.Name = "toolStripMenuItemImagePerFrame";
            this.toolStripMenuItemImagePerFrame.Size = new System.Drawing.Size(255, 22);
            this.toolStripMenuItemImagePerFrame.Text = "Image per frame...";
            this.toolStripMenuItemImagePerFrame.Visible = false;
            this.toolStripMenuItemImagePerFrame.Click += new System.EventHandler(this.ToolStripMenuItemImagePerFrameClick);
            // 
            // toolStripMenuItemTextTimeCodePair
            // 
            this.toolStripMenuItemTextTimeCodePair.Name = "toolStripMenuItemTextTimeCodePair";
            this.toolStripMenuItemTextTimeCodePair.Size = new System.Drawing.Size(255, 22);
            this.toolStripMenuItemTextTimeCodePair.Text = "Korean ATS file pair...";
            this.toolStripMenuItemTextTimeCodePair.Click += new System.EventHandler(this.toolStripMenuItemTextTimeCodePair_Click);
            // 
            // pACScreenElectronicsToolStripMenuItem
            // 
            this.pACScreenElectronicsToolStripMenuItem.Name = "pACScreenElectronicsToolStripMenuItem";
            this.pACScreenElectronicsToolStripMenuItem.Size = new System.Drawing.Size(255, 22);
            this.pACScreenElectronicsToolStripMenuItem.Text = "PAC (Screen Electronics)...";
            this.pACScreenElectronicsToolStripMenuItem.Click += new System.EventHandler(this.PacScreenElectronicsToolStripMenuItemClick);
            // 
            // uniPacExportToolStripMenuItem
            // 
            this.uniPacExportToolStripMenuItem.Name = "uniPacExportToolStripMenuItem";
            this.uniPacExportToolStripMenuItem.Size = new System.Drawing.Size(255, 22);
            this.uniPacExportToolStripMenuItem.Text = "PAC Unicode (UniPac)...";
            this.uniPacExportToolStripMenuItem.Click += new System.EventHandler(this.uniPacExportToolStripMenuItem_Click);
            // 
            // plainTextToolStripMenuItem
            // 
            this.plainTextToolStripMenuItem.Name = "plainTextToolStripMenuItem";
            this.plainTextToolStripMenuItem.Size = new System.Drawing.Size(255, 22);
            this.plainTextToolStripMenuItem.Text = "Plain text...";
            this.plainTextToolStripMenuItem.Click += new System.EventHandler(this.PlainTextToolStripMenuItemClick);
            // 
            // toolStripMenuItemSpumux
            // 
            this.toolStripMenuItemSpumux.Name = "toolStripMenuItemSpumux";
            this.toolStripMenuItemSpumux.Size = new System.Drawing.Size(255, 22);
            this.toolStripMenuItemSpumux.Text = "Spumux...";
            this.toolStripMenuItemSpumux.Click += new System.EventHandler(this.toolStripMenuItem2_Click);
            // 
            // toolStripMenuItemExportUltech130
            // 
            this.toolStripMenuItemExportUltech130.Name = "toolStripMenuItemExportUltech130";
            this.toolStripMenuItemExportUltech130.Size = new System.Drawing.Size(255, 22);
            this.toolStripMenuItemExportUltech130.Text = "Ultech caption...";
            this.toolStripMenuItemExportUltech130.Click += new System.EventHandler(this.toolStripMenuItemExportUltech130_Click);
            // 
            // vobSubsubidxToolStripMenuItem
            // 
            this.vobSubsubidxToolStripMenuItem.Name = "vobSubsubidxToolStripMenuItem";
            this.vobSubsubidxToolStripMenuItem.Size = new System.Drawing.Size(255, 22);
            this.vobSubsubidxToolStripMenuItem.Text = "VobSub (sub/idx)...";
            this.vobSubsubidxToolStripMenuItem.Click += new System.EventHandler(this.VobSubsubidxToolStripMenuItemClick);
            // 
            // toolStripSeparatorExportCustomText
            // 
            this.toolStripSeparatorExportCustomText.Name = "toolStripSeparatorExportCustomText";
            this.toolStripSeparatorExportCustomText.Size = new System.Drawing.Size(252, 6);
            // 
            // exportCustomTextFormatToolStripMenuItem
            // 
            this.exportCustomTextFormatToolStripMenuItem.Name = "exportCustomTextFormatToolStripMenuItem";
            this.exportCustomTextFormatToolStripMenuItem.Size = new System.Drawing.Size(255, 22);
            this.exportCustomTextFormatToolStripMenuItem.Text = "Export custom text format...";
            this.exportCustomTextFormatToolStripMenuItem.Click += new System.EventHandler(this.exportCustomTextFormatToolStripMenuItem_Click);
            // 
            // toolStripSeparator10
            // 
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            this.toolStripSeparator10.Size = new System.Drawing.Size(332, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(335, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.ExitToolStripMenuItemClick);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemUndo,
            this.toolStripMenuItemRedo,
            this.showHistoryforUndoToolStripMenuItem,
            this.toolStripSeparator14,
            this.toolStripMenuItemInsertUnicodeCharacter,
            this.toolStripSeparatorInsertUnicodeCharacter,
            this.findToolStripMenuItem,
            this.findNextToolStripMenuItem,
            this.replaceToolStripMenuItem,
            this.multipleReplaceToolStripMenuItem,
            this.gotoLineNumberToolStripMenuItem,
            this.toolStripMenuItemShowOriginalInPreview,
            this.toolStripSeparator25,
            this.toolStripMenuItemRightToLeftMode,
            this.toolStripMenuItemRtlUnicodeControlChars,
            this.toolStripMenuItemReverseRightToLeftStartEnd,
            this.toolStripSeparator21,
            this.toolStripMenuItemModifySelection,
            this.toolStripMenuItemInverseSelection,
            this.editSelectAllToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(40, 20);
            this.editToolStripMenuItem.Text = "Edit";
            this.editToolStripMenuItem.DropDownOpening += new System.EventHandler(this.EditToolStripMenuItemDropDownOpening);
            // 
            // toolStripMenuItemUndo
            // 
            this.toolStripMenuItemUndo.Name = "toolStripMenuItemUndo";
            this.toolStripMenuItemUndo.Size = new System.Drawing.Size(301, 22);
            this.toolStripMenuItemUndo.Text = "Undo";
            this.toolStripMenuItemUndo.Click += new System.EventHandler(this.toolStripMenuItemUndo_Click);
            // 
            // toolStripMenuItemRedo
            // 
            this.toolStripMenuItemRedo.Name = "toolStripMenuItemRedo";
            this.toolStripMenuItemRedo.Size = new System.Drawing.Size(301, 22);
            this.toolStripMenuItemRedo.Text = "Redo";
            this.toolStripMenuItemRedo.Click += new System.EventHandler(this.toolStripMenuItemRedo_Click);
            // 
            // showHistoryforUndoToolStripMenuItem
            // 
            this.showHistoryforUndoToolStripMenuItem.Name = "showHistoryforUndoToolStripMenuItem";
            this.showHistoryforUndoToolStripMenuItem.Size = new System.Drawing.Size(301, 22);
            this.showHistoryforUndoToolStripMenuItem.Text = "Show history (for undo)";
            this.showHistoryforUndoToolStripMenuItem.Click += new System.EventHandler(this.ShowHistoryforUndoToolStripMenuItemClick);
            // 
            // toolStripSeparator14
            // 
            this.toolStripSeparator14.Name = "toolStripSeparator14";
            this.toolStripSeparator14.Size = new System.Drawing.Size(298, 6);
            // 
            // toolStripMenuItemInsertUnicodeCharacter
            // 
            this.toolStripMenuItemInsertUnicodeCharacter.Name = "toolStripMenuItemInsertUnicodeCharacter";
            this.toolStripMenuItemInsertUnicodeCharacter.Size = new System.Drawing.Size(301, 22);
            this.toolStripMenuItemInsertUnicodeCharacter.Text = "Insert unicode character";
            // 
            // toolStripSeparatorInsertUnicodeCharacter
            // 
            this.toolStripSeparatorInsertUnicodeCharacter.Name = "toolStripSeparatorInsertUnicodeCharacter";
            this.toolStripSeparatorInsertUnicodeCharacter.Size = new System.Drawing.Size(298, 6);
            // 
            // findToolStripMenuItem
            // 
            this.findToolStripMenuItem.Name = "findToolStripMenuItem";
            this.findToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.findToolStripMenuItem.Size = new System.Drawing.Size(301, 22);
            this.findToolStripMenuItem.Text = "Find";
            this.findToolStripMenuItem.Click += new System.EventHandler(this.FindToolStripMenuItemClick);
            // 
            // findNextToolStripMenuItem
            // 
            this.findNextToolStripMenuItem.Name = "findNextToolStripMenuItem";
            this.findNextToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F3;
            this.findNextToolStripMenuItem.Size = new System.Drawing.Size(301, 22);
            this.findNextToolStripMenuItem.Text = "Find next";
            this.findNextToolStripMenuItem.Click += new System.EventHandler(this.FindNextToolStripMenuItemClick);
            // 
            // replaceToolStripMenuItem
            // 
            this.replaceToolStripMenuItem.Name = "replaceToolStripMenuItem";
            this.replaceToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.H)));
            this.replaceToolStripMenuItem.Size = new System.Drawing.Size(301, 22);
            this.replaceToolStripMenuItem.Text = "Replace";
            this.replaceToolStripMenuItem.Click += new System.EventHandler(this.ReplaceToolStripMenuItemClick);
            // 
            // multipleReplaceToolStripMenuItem
            // 
            this.multipleReplaceToolStripMenuItem.Name = "multipleReplaceToolStripMenuItem";
            this.multipleReplaceToolStripMenuItem.Size = new System.Drawing.Size(301, 22);
            this.multipleReplaceToolStripMenuItem.Text = "Multiple replace";
            this.multipleReplaceToolStripMenuItem.Click += new System.EventHandler(this.MultipleReplaceToolStripMenuItemClick);
            // 
            // gotoLineNumberToolStripMenuItem
            // 
            this.gotoLineNumberToolStripMenuItem.Name = "gotoLineNumberToolStripMenuItem";
            this.gotoLineNumberToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.G)));
            this.gotoLineNumberToolStripMenuItem.Size = new System.Drawing.Size(301, 22);
            this.gotoLineNumberToolStripMenuItem.Text = "Goto line number...";
            this.gotoLineNumberToolStripMenuItem.Click += new System.EventHandler(this.GotoLineNumberToolStripMenuItemClick);
            // 
            // toolStripMenuItemShowOriginalInPreview
            // 
            this.toolStripMenuItemShowOriginalInPreview.Name = "toolStripMenuItemShowOriginalInPreview";
            this.toolStripMenuItemShowOriginalInPreview.Size = new System.Drawing.Size(301, 22);
            this.toolStripMenuItemShowOriginalInPreview.Text = "Show original text in video/audio previews";
            this.toolStripMenuItemShowOriginalInPreview.Click += new System.EventHandler(this.ToolStripMenuItemShowOriginalInPreviewClick);
            // 
            // toolStripSeparator25
            // 
            this.toolStripSeparator25.Name = "toolStripSeparator25";
            this.toolStripSeparator25.Size = new System.Drawing.Size(298, 6);
            // 
            // toolStripMenuItemRightToLeftMode
            // 
            this.toolStripMenuItemRightToLeftMode.Name = "toolStripMenuItemRightToLeftMode";
            this.toolStripMenuItemRightToLeftMode.Size = new System.Drawing.Size(301, 22);
            this.toolStripMenuItemRightToLeftMode.Text = "Right to left mode";
            this.toolStripMenuItemRightToLeftMode.Click += new System.EventHandler(this.ToolStripMenuItemRightToLeftModeClick);
            // 
            // toolStripMenuItemRtlUnicodeControlChars
            // 
            this.toolStripMenuItemRtlUnicodeControlChars.Name = "toolStripMenuItemRtlUnicodeControlChars";
            this.toolStripMenuItemRtlUnicodeControlChars.Size = new System.Drawing.Size(301, 22);
            this.toolStripMenuItemRtlUnicodeControlChars.Text = "Fix RTL via Unicode tags";
            this.toolStripMenuItemRtlUnicodeControlChars.Click += new System.EventHandler(this.toolStripMenuItemRtlUnicodeControlChar_Click);
            // 
            // toolStripMenuItemReverseRightToLeftStartEnd
            // 
            this.toolStripMenuItemReverseRightToLeftStartEnd.Name = "toolStripMenuItemReverseRightToLeftStartEnd";
            this.toolStripMenuItemReverseRightToLeftStartEnd.Size = new System.Drawing.Size(301, 22);
            this.toolStripMenuItemReverseRightToLeftStartEnd.Text = "Reverse RTL start/end";
            this.toolStripMenuItemReverseRightToLeftStartEnd.Click += new System.EventHandler(this.toolStripMenuItemReverseRightToLeftStartEnd_Click);
            // 
            // toolStripSeparator21
            // 
            this.toolStripSeparator21.Name = "toolStripSeparator21";
            this.toolStripSeparator21.Size = new System.Drawing.Size(298, 6);
            // 
            // toolStripMenuItemModifySelection
            // 
            this.toolStripMenuItemModifySelection.Name = "toolStripMenuItemModifySelection";
            this.toolStripMenuItemModifySelection.Size = new System.Drawing.Size(301, 22);
            this.toolStripMenuItemModifySelection.Text = "Create/modify selection...";
            this.toolStripMenuItemModifySelection.Click += new System.EventHandler(this.toolStripMenuItemModifySelection_Click);
            // 
            // toolStripMenuItemInverseSelection
            // 
            this.toolStripMenuItemInverseSelection.Name = "toolStripMenuItemInverseSelection";
            this.toolStripMenuItemInverseSelection.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.I)));
            this.toolStripMenuItemInverseSelection.Size = new System.Drawing.Size(301, 22);
            this.toolStripMenuItemInverseSelection.Text = "Inverse selection";
            this.toolStripMenuItemInverseSelection.Click += new System.EventHandler(this.toolStripMenuItemInverseSelection_Click);
            // 
            // editSelectAllToolStripMenuItem
            // 
            this.editSelectAllToolStripMenuItem.Name = "editSelectAllToolStripMenuItem";
            this.editSelectAllToolStripMenuItem.Size = new System.Drawing.Size(301, 22);
            this.editSelectAllToolStripMenuItem.Text = "Select all";
            this.editSelectAllToolStripMenuItem.Click += new System.EventHandler(this.EditSelectAllToolStripMenuItemClick);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.adjustDisplayTimeToolStripMenuItem,
            this.toolStripMenuItemApplyDurationLimits,
            this.toolStripMenuItemDurationBridgeGaps,
            this.fixToolStripMenuItem,
            this.startNumberingFromToolStripMenuItem,
            this.removeTextForHearImpairedToolStripMenuItem,
            this.ChangeCasingToolStripMenuItem,
            this.toolStripMenuItemAutoMergeShortLines,
            this.toolStripMenuItemMergeDuplicateText,
            this.toolStripMenuItemMergeLinesWithSameTimeCodes,
            this.toolStripMenuItemAutoSplitLongLines,
            this.setMinimumDisplayTimeBetweenParagraphsToolStripMenuItem,
            this.toolStripMenuItemSortBy,
            this.netflixQualityCheckToolStripMenuItem,
            this.toolStripSeparator23,
            this.toolStripMenuItemMakeEmptyFromCurrent,
            this.toolStripMenuItemBatchConvert,
            this.generateDatetimeInfoFromVideoToolStripMenuItem,
            this.toolStripMenuItemMeasurementConverter,
            this.toolStripSeparator3,
            this.splitToolStripMenuItem,
            this.appendTextVisuallyToolStripMenuItem,
            this.joinSubtitlesToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(49, 20);
            this.toolsToolStripMenuItem.Text = "Tools";
            this.toolsToolStripMenuItem.DropDownOpening += new System.EventHandler(this.ToolsToolStripMenuItemDropDownOpening);
            // 
            // adjustDisplayTimeToolStripMenuItem
            // 
            this.adjustDisplayTimeToolStripMenuItem.Name = "adjustDisplayTimeToolStripMenuItem";
            this.adjustDisplayTimeToolStripMenuItem.Size = new System.Drawing.Size(338, 22);
            this.adjustDisplayTimeToolStripMenuItem.Text = "Adjust display time...";
            this.adjustDisplayTimeToolStripMenuItem.Click += new System.EventHandler(this.AdjustDisplayTimeToolStripMenuItemClick);
            // 
            // toolStripMenuItemApplyDurationLimits
            // 
            this.toolStripMenuItemApplyDurationLimits.Name = "toolStripMenuItemApplyDurationLimits";
            this.toolStripMenuItemApplyDurationLimits.Size = new System.Drawing.Size(338, 22);
            this.toolStripMenuItemApplyDurationLimits.Text = "Apply duration limits...";
            this.toolStripMenuItemApplyDurationLimits.Click += new System.EventHandler(this.toolStripMenuItemApplyDisplayTimeLimits_Click);
            // 
            // toolStripMenuItemDurationBridgeGaps
            // 
            this.toolStripMenuItemDurationBridgeGaps.Name = "toolStripMenuItemDurationBridgeGaps";
            this.toolStripMenuItemDurationBridgeGaps.Size = new System.Drawing.Size(338, 22);
            this.toolStripMenuItemDurationBridgeGaps.Text = "Bridge gaps in durations...";
            this.toolStripMenuItemDurationBridgeGaps.Click += new System.EventHandler(this.toolStripMenuItemDurationBridgeGaps_Click);
            // 
            // fixToolStripMenuItem
            // 
            this.fixToolStripMenuItem.Name = "fixToolStripMenuItem";
            this.fixToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.F)));
            this.fixToolStripMenuItem.Size = new System.Drawing.Size(338, 22);
            this.fixToolStripMenuItem.Text = "Fix common errors...";
            this.fixToolStripMenuItem.Click += new System.EventHandler(this.FixToolStripMenuItemClick);
            // 
            // startNumberingFromToolStripMenuItem
            // 
            this.startNumberingFromToolStripMenuItem.Name = "startNumberingFromToolStripMenuItem";
            this.startNumberingFromToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.N)));
            this.startNumberingFromToolStripMenuItem.Size = new System.Drawing.Size(338, 22);
            this.startNumberingFromToolStripMenuItem.Text = "Start numbering from...";
            this.startNumberingFromToolStripMenuItem.Click += new System.EventHandler(this.StartNumberingFromToolStripMenuItemClick);
            // 
            // removeTextForHearImpairedToolStripMenuItem
            // 
            this.removeTextForHearImpairedToolStripMenuItem.Name = "removeTextForHearImpairedToolStripMenuItem";
            this.removeTextForHearImpairedToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.H)));
            this.removeTextForHearImpairedToolStripMenuItem.Size = new System.Drawing.Size(338, 22);
            this.removeTextForHearImpairedToolStripMenuItem.Text = "Remove text for hearing impaired...";
            this.removeTextForHearImpairedToolStripMenuItem.Click += new System.EventHandler(this.RemoveTextForHearImpairedToolStripMenuItemClick);
            // 
            // ChangeCasingToolStripMenuItem
            // 
            this.ChangeCasingToolStripMenuItem.Name = "ChangeCasingToolStripMenuItem";
            this.ChangeCasingToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.C)));
            this.ChangeCasingToolStripMenuItem.Size = new System.Drawing.Size(338, 22);
            this.ChangeCasingToolStripMenuItem.Text = "Change casing...";
            this.ChangeCasingToolStripMenuItem.Click += new System.EventHandler(this.ChangeCasingToolStripMenuItem_Click);
            // 
            // toolStripMenuItemAutoMergeShortLines
            // 
            this.toolStripMenuItemAutoMergeShortLines.Name = "toolStripMenuItemAutoMergeShortLines";
            this.toolStripMenuItemAutoMergeShortLines.Size = new System.Drawing.Size(338, 22);
            this.toolStripMenuItemAutoMergeShortLines.Text = "Merge short lines...";
            this.toolStripMenuItemAutoMergeShortLines.Click += new System.EventHandler(this.ToolStripMenuItemAutoMergeShortLinesClick);
            // 
            // toolStripMenuItemMergeDuplicateText
            // 
            this.toolStripMenuItemMergeDuplicateText.Name = "toolStripMenuItemMergeDuplicateText";
            this.toolStripMenuItemMergeDuplicateText.Size = new System.Drawing.Size(338, 22);
            this.toolStripMenuItemMergeDuplicateText.Text = "Merge lines with same text...";
            this.toolStripMenuItemMergeDuplicateText.Click += new System.EventHandler(this.toolStripMenuItemMergeDuplicateText_Click);
            // 
            // toolStripMenuItemMergeLinesWithSameTimeCodes
            // 
            this.toolStripMenuItemMergeLinesWithSameTimeCodes.Name = "toolStripMenuItemMergeLinesWithSameTimeCodes";
            this.toolStripMenuItemMergeLinesWithSameTimeCodes.Size = new System.Drawing.Size(338, 22);
            this.toolStripMenuItemMergeLinesWithSameTimeCodes.Text = "Merge lines with same time codes...";
            this.toolStripMenuItemMergeLinesWithSameTimeCodes.Click += new System.EventHandler(this.toolStripMenuItemMergeLinesWithSameTimeCodes_Click);
            // 
            // toolStripMenuItemAutoSplitLongLines
            // 
            this.toolStripMenuItemAutoSplitLongLines.Name = "toolStripMenuItemAutoSplitLongLines";
            this.toolStripMenuItemAutoSplitLongLines.Size = new System.Drawing.Size(338, 22);
            this.toolStripMenuItemAutoSplitLongLines.Text = "Split long lines...";
            this.toolStripMenuItemAutoSplitLongLines.Click += new System.EventHandler(this.toolStripMenuItemAutoSplitLongLines_Click);
            // 
            // setMinimumDisplayTimeBetweenParagraphsToolStripMenuItem
            // 
            this.setMinimumDisplayTimeBetweenParagraphsToolStripMenuItem.Name = "setMinimumDisplayTimeBetweenParagraphsToolStripMenuItem";
            this.setMinimumDisplayTimeBetweenParagraphsToolStripMenuItem.Size = new System.Drawing.Size(338, 22);
            this.setMinimumDisplayTimeBetweenParagraphsToolStripMenuItem.Text = "Minimum display time between paragraphs...";
            this.setMinimumDisplayTimeBetweenParagraphsToolStripMenuItem.Click += new System.EventHandler(this.SetMinimalDisplayTimeDifferenceToolStripMenuItemClick);
            // 
            // toolStripMenuItemSortBy
            // 
            this.toolStripMenuItemSortBy.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sortNumberToolStripMenuItem,
            this.sortStartTimeToolStripMenuItem,
            this.sortEndTimeToolStripMenuItem,
            this.sortDisplayTimeToolStripMenuItem,
            this.sortTextAlphabeticallytoolStripMenuItem,
            this.sortTextMaxLineLengthToolStripMenuItem,
            this.sortTextTotalLengthToolStripMenuItem,
            this.sortTextNumberOfLinesToolStripMenuItem,
            this.textCharssecToolStripMenuItem,
            this.textWordsPerMinutewpmToolStripMenuItem,
            this.actorToolStripMenuItem,
            this.styleToolStripMenuItem,
            this.toolStripSeparatorAscOrDesc,
            this.AscendingToolStripMenuItem,
            this.descendingToolStripMenuItem});
            this.toolStripMenuItemSortBy.Name = "toolStripMenuItemSortBy";
            this.toolStripMenuItemSortBy.Size = new System.Drawing.Size(338, 22);
            this.toolStripMenuItemSortBy.Text = "Sort by";
            // 
            // sortNumberToolStripMenuItem
            // 
            this.sortNumberToolStripMenuItem.Name = "sortNumberToolStripMenuItem";
            this.sortNumberToolStripMenuItem.Size = new System.Drawing.Size(240, 22);
            this.sortNumberToolStripMenuItem.Text = "Number";
            this.sortNumberToolStripMenuItem.Click += new System.EventHandler(this.SortNumberToolStripMenuItemClick);
            // 
            // sortStartTimeToolStripMenuItem
            // 
            this.sortStartTimeToolStripMenuItem.Name = "sortStartTimeToolStripMenuItem";
            this.sortStartTimeToolStripMenuItem.Size = new System.Drawing.Size(240, 22);
            this.sortStartTimeToolStripMenuItem.Text = "Start time";
            this.sortStartTimeToolStripMenuItem.Click += new System.EventHandler(this.SortStartTimeToolStripMenuItemClick);
            // 
            // sortEndTimeToolStripMenuItem
            // 
            this.sortEndTimeToolStripMenuItem.Name = "sortEndTimeToolStripMenuItem";
            this.sortEndTimeToolStripMenuItem.Size = new System.Drawing.Size(240, 22);
            this.sortEndTimeToolStripMenuItem.Text = "End time";
            this.sortEndTimeToolStripMenuItem.Click += new System.EventHandler(this.SortEndTimeToolStripMenuItemClick);
            // 
            // sortDisplayTimeToolStripMenuItem
            // 
            this.sortDisplayTimeToolStripMenuItem.Name = "sortDisplayTimeToolStripMenuItem";
            this.sortDisplayTimeToolStripMenuItem.Size = new System.Drawing.Size(240, 22);
            this.sortDisplayTimeToolStripMenuItem.Text = "Duration";
            this.sortDisplayTimeToolStripMenuItem.Click += new System.EventHandler(this.SortDisplayTimeToolStripMenuItemClick);
            // 
            // sortTextAlphabeticallytoolStripMenuItem
            // 
            this.sortTextAlphabeticallytoolStripMenuItem.Name = "sortTextAlphabeticallytoolStripMenuItem";
            this.sortTextAlphabeticallytoolStripMenuItem.Size = new System.Drawing.Size(240, 22);
            this.sortTextAlphabeticallytoolStripMenuItem.Text = "Text - alphabetically";
            this.sortTextAlphabeticallytoolStripMenuItem.Click += new System.EventHandler(this.SortTextAlphabeticallytoolStripMenuItemClick);
            // 
            // sortTextMaxLineLengthToolStripMenuItem
            // 
            this.sortTextMaxLineLengthToolStripMenuItem.Name = "sortTextMaxLineLengthToolStripMenuItem";
            this.sortTextMaxLineLengthToolStripMenuItem.Size = new System.Drawing.Size(240, 22);
            this.sortTextMaxLineLengthToolStripMenuItem.Text = "Text - single line max. length";
            this.sortTextMaxLineLengthToolStripMenuItem.Click += new System.EventHandler(this.SortTextMaxLineLengthToolStripMenuItemClick);
            // 
            // sortTextTotalLengthToolStripMenuItem
            // 
            this.sortTextTotalLengthToolStripMenuItem.Name = "sortTextTotalLengthToolStripMenuItem";
            this.sortTextTotalLengthToolStripMenuItem.Size = new System.Drawing.Size(240, 22);
            this.sortTextTotalLengthToolStripMenuItem.Text = "Text - total length";
            this.sortTextTotalLengthToolStripMenuItem.Click += new System.EventHandler(this.SortTextTotalLengthToolStripMenuItemClick);
            // 
            // sortTextNumberOfLinesToolStripMenuItem
            // 
            this.sortTextNumberOfLinesToolStripMenuItem.Name = "sortTextNumberOfLinesToolStripMenuItem";
            this.sortTextNumberOfLinesToolStripMenuItem.Size = new System.Drawing.Size(240, 22);
            this.sortTextNumberOfLinesToolStripMenuItem.Text = "Text - number of lines";
            this.sortTextNumberOfLinesToolStripMenuItem.Click += new System.EventHandler(this.SortTextNumberOfLinesToolStripMenuItemClick);
            // 
            // textCharssecToolStripMenuItem
            // 
            this.textCharssecToolStripMenuItem.Name = "textCharssecToolStripMenuItem";
            this.textCharssecToolStripMenuItem.Size = new System.Drawing.Size(240, 22);
            this.textCharssecToolStripMenuItem.Text = "Text - chars/sec";
            this.textCharssecToolStripMenuItem.Click += new System.EventHandler(this.textCharssecToolStripMenuItem_Click);
            // 
            // textWordsPerMinutewpmToolStripMenuItem
            // 
            this.textWordsPerMinutewpmToolStripMenuItem.Name = "textWordsPerMinutewpmToolStripMenuItem";
            this.textWordsPerMinutewpmToolStripMenuItem.Size = new System.Drawing.Size(240, 22);
            this.textWordsPerMinutewpmToolStripMenuItem.Text = "Text - words per minute (wpm)";
            this.textWordsPerMinutewpmToolStripMenuItem.Click += new System.EventHandler(this.textWordsPerMinutewpmToolStripMenuItem_Click);
            // 
            // actorToolStripMenuItem
            // 
            this.actorToolStripMenuItem.Name = "actorToolStripMenuItem";
            this.actorToolStripMenuItem.Size = new System.Drawing.Size(240, 22);
            this.actorToolStripMenuItem.Text = "Actor";
            this.actorToolStripMenuItem.Click += new System.EventHandler(this.actorToolStripMenuItemClick);
            // 
            // styleToolStripMenuItem
            // 
            this.styleToolStripMenuItem.Name = "styleToolStripMenuItem";
            this.styleToolStripMenuItem.Size = new System.Drawing.Size(240, 22);
            this.styleToolStripMenuItem.Text = "Style";
            this.styleToolStripMenuItem.Click += new System.EventHandler(this.styleToolStripMenuItem_Click);
            // 
            // toolStripSeparatorAscOrDesc
            // 
            this.toolStripSeparatorAscOrDesc.Name = "toolStripSeparatorAscOrDesc";
            this.toolStripSeparatorAscOrDesc.Size = new System.Drawing.Size(237, 6);
            // 
            // AscendingToolStripMenuItem
            // 
            this.AscendingToolStripMenuItem.Checked = true;
            this.AscendingToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.AscendingToolStripMenuItem.Name = "AscendingToolStripMenuItem";
            this.AscendingToolStripMenuItem.Size = new System.Drawing.Size(240, 22);
            this.AscendingToolStripMenuItem.Text = "Ascending";
            this.AscendingToolStripMenuItem.Click += new System.EventHandler(this.AscendingToolStripMenuItem_Click);
            // 
            // descendingToolStripMenuItem
            // 
            this.descendingToolStripMenuItem.Name = "descendingToolStripMenuItem";
            this.descendingToolStripMenuItem.Size = new System.Drawing.Size(240, 22);
            this.descendingToolStripMenuItem.Text = "Descending";
            this.descendingToolStripMenuItem.Click += new System.EventHandler(this.descendingToolStripMenuItem_Click);
            // 
            // netflixQualityCheckToolStripMenuItem
            // 
            this.netflixQualityCheckToolStripMenuItem.Name = "netflixQualityCheckToolStripMenuItem";
            this.netflixQualityCheckToolStripMenuItem.Size = new System.Drawing.Size(338, 22);
            this.netflixQualityCheckToolStripMenuItem.Text = "Netflix quality check";
            this.netflixQualityCheckToolStripMenuItem.Click += new System.EventHandler(this.netflixGlyphCheckToolStripMenuItem_Click);
            // 
            // toolStripSeparator23
            // 
            this.toolStripSeparator23.Name = "toolStripSeparator23";
            this.toolStripSeparator23.Size = new System.Drawing.Size(335, 6);
            // 
            // toolStripMenuItemMakeEmptyFromCurrent
            // 
            this.toolStripMenuItemMakeEmptyFromCurrent.Name = "toolStripMenuItemMakeEmptyFromCurrent";
            this.toolStripMenuItemMakeEmptyFromCurrent.Size = new System.Drawing.Size(338, 22);
            this.toolStripMenuItemMakeEmptyFromCurrent.Text = "Make new empty translation from current subtitle";
            this.toolStripMenuItemMakeEmptyFromCurrent.Click += new System.EventHandler(this.ToolStripMenuItemMakeEmptyFromCurrentClick);
            // 
            // toolStripMenuItemBatchConvert
            // 
            this.toolStripMenuItemBatchConvert.Name = "toolStripMenuItemBatchConvert";
            this.toolStripMenuItemBatchConvert.Size = new System.Drawing.Size(338, 22);
            this.toolStripMenuItemBatchConvert.Text = "Batch convert...";
            this.toolStripMenuItemBatchConvert.Click += new System.EventHandler(this.toolStripMenuItemBatchConvert_Click);
            // 
            // generateDatetimeInfoFromVideoToolStripMenuItem
            // 
            this.generateDatetimeInfoFromVideoToolStripMenuItem.Name = "generateDatetimeInfoFromVideoToolStripMenuItem";
            this.generateDatetimeInfoFromVideoToolStripMenuItem.Size = new System.Drawing.Size(338, 22);
            this.generateDatetimeInfoFromVideoToolStripMenuItem.Text = "Generate date/time info from video...";
            this.generateDatetimeInfoFromVideoToolStripMenuItem.Click += new System.EventHandler(this.generateDatetimeInfoFromVideoToolStripMenuItem_Click);
            // 
            // toolStripMenuItemMeasurementConverter
            // 
            this.toolStripMenuItemMeasurementConverter.Name = "toolStripMenuItemMeasurementConverter";
            this.toolStripMenuItemMeasurementConverter.Size = new System.Drawing.Size(338, 22);
            this.toolStripMenuItemMeasurementConverter.Text = "Measurement converter...";
            this.toolStripMenuItemMeasurementConverter.Click += new System.EventHandler(this.toolStripMenuItemMeasurementConverter_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(335, 6);
            // 
            // splitToolStripMenuItem
            // 
            this.splitToolStripMenuItem.Name = "splitToolStripMenuItem";
            this.splitToolStripMenuItem.Size = new System.Drawing.Size(338, 22);
            this.splitToolStripMenuItem.Text = "Split subtitle...";
            this.splitToolStripMenuItem.Click += new System.EventHandler(this.SplitToolStripMenuItemClick);
            // 
            // appendTextVisuallyToolStripMenuItem
            // 
            this.appendTextVisuallyToolStripMenuItem.Name = "appendTextVisuallyToolStripMenuItem";
            this.appendTextVisuallyToolStripMenuItem.Size = new System.Drawing.Size(338, 22);
            this.appendTextVisuallyToolStripMenuItem.Text = "Append subtitle...";
            this.appendTextVisuallyToolStripMenuItem.Click += new System.EventHandler(this.AppendTextVisuallyToolStripMenuItemClick);
            // 
            // joinSubtitlesToolStripMenuItem
            // 
            this.joinSubtitlesToolStripMenuItem.Name = "joinSubtitlesToolStripMenuItem";
            this.joinSubtitlesToolStripMenuItem.Size = new System.Drawing.Size(338, 22);
            this.joinSubtitlesToolStripMenuItem.Text = "Join subtitles...";
            this.joinSubtitlesToolStripMenuItem.Click += new System.EventHandler(this.joinSubtitlesToolStripMenuItem_Click);
            // 
            // toolStripMenuItemSpellCheckMain
            // 
            this.toolStripMenuItemSpellCheckMain.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.spellCheckToolStripMenuItem,
            this.toolStripMenuItemSpellCheckFromCurrentLine,
            this.findDoubleWordsToolStripMenuItem,
            this.FindDoubleLinesToolStripMenuItem,
            this.toolStripSeparator9,
            this.GetDictionariesToolStripMenuItem,
            this.addWordToNameListToolStripMenuItem});
            this.toolStripMenuItemSpellCheckMain.Name = "toolStripMenuItemSpellCheckMain";
            this.toolStripMenuItemSpellCheckMain.Size = new System.Drawing.Size(82, 20);
            this.toolStripMenuItemSpellCheckMain.Text = "Spell check";
            this.toolStripMenuItemSpellCheckMain.DropDownOpening += new System.EventHandler(this.ToolStripMenuItemSpellCheckMainDropDownOpening);
            // 
            // spellCheckToolStripMenuItem
            // 
            this.spellCheckToolStripMenuItem.Name = "spellCheckToolStripMenuItem";
            this.spellCheckToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.S)));
            this.spellCheckToolStripMenuItem.Size = new System.Drawing.Size(267, 22);
            this.spellCheckToolStripMenuItem.Text = "Spell check...";
            this.spellCheckToolStripMenuItem.Click += new System.EventHandler(this.SpellCheckToolStripMenuItemClick);
            // 
            // toolStripMenuItemSpellCheckFromCurrentLine
            // 
            this.toolStripMenuItemSpellCheckFromCurrentLine.Name = "toolStripMenuItemSpellCheckFromCurrentLine";
            this.toolStripMenuItemSpellCheckFromCurrentLine.Size = new System.Drawing.Size(267, 22);
            this.toolStripMenuItemSpellCheckFromCurrentLine.Text = "Spell check from current line...";
            this.toolStripMenuItemSpellCheckFromCurrentLine.Click += new System.EventHandler(this.toolStripMenuItemSpellCheckFromCurrentLine_Click);
            // 
            // findDoubleWordsToolStripMenuItem
            // 
            this.findDoubleWordsToolStripMenuItem.Name = "findDoubleWordsToolStripMenuItem";
            this.findDoubleWordsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.D)));
            this.findDoubleWordsToolStripMenuItem.Size = new System.Drawing.Size(267, 22);
            this.findDoubleWordsToolStripMenuItem.Text = "Find double words";
            this.findDoubleWordsToolStripMenuItem.Click += new System.EventHandler(this.FindDoubleWordsToolStripMenuItemClick);
            // 
            // FindDoubleLinesToolStripMenuItem
            // 
            this.FindDoubleLinesToolStripMenuItem.Name = "FindDoubleLinesToolStripMenuItem";
            this.FindDoubleLinesToolStripMenuItem.Size = new System.Drawing.Size(267, 22);
            this.FindDoubleLinesToolStripMenuItem.Text = "Find double lines";
            this.FindDoubleLinesToolStripMenuItem.Click += new System.EventHandler(this.FindDoubleLinesToolStripMenuItemClick);
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(264, 6);
            // 
            // GetDictionariesToolStripMenuItem
            // 
            this.GetDictionariesToolStripMenuItem.Name = "GetDictionariesToolStripMenuItem";
            this.GetDictionariesToolStripMenuItem.Size = new System.Drawing.Size(267, 22);
            this.GetDictionariesToolStripMenuItem.Text = "Get dictionary...";
            this.GetDictionariesToolStripMenuItem.Click += new System.EventHandler(this.GetDictionariesToolStripMenuItem_Click);
            // 
            // addWordToNameListToolStripMenuItem
            // 
            this.addWordToNameListToolStripMenuItem.Name = "addWordToNameListToolStripMenuItem";
            this.addWordToNameListToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.L)));
            this.addWordToNameListToolStripMenuItem.Size = new System.Drawing.Size(267, 22);
            this.addWordToNameListToolStripMenuItem.Text = "Add word to names list";
            this.addWordToNameListToolStripMenuItem.Click += new System.EventHandler(this.AddWordToNameListToolStripMenuItemClick);
            // 
            // toolStripMenuItemVideo
            // 
            this.toolStripMenuItemVideo.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openVideoToolStripMenuItem,
            this.toolStripMenuItemOpenVideoFromUrl,
            this.toolStripMenuItemOpenDvd,
            this.toolStripMenuItemSetAudioTrack,
            this.closeVideoToolStripMenuItem,
            this.setVideoOffsetToolStripMenuItem,
            this.smpteTimeModedropFrameToolStripMenuItem,
            this.toolStripMenuItemImportSceneChanges,
            this.toolStripMenuItemRemoveSceneChanges,
            this.toolStripMenuItemAddWaveformBatch,
            this.generateTextFromCurrentVideoToolStripMenuItem,
            this.toolStripSeparator5,
            this.showhideWaveformToolStripMenuItem,
            this.showhideVideoToolStripMenuItem,
            this.toolStripSeparator19,
            this.undockVideoControlsToolStripMenuItem,
            this.redockVideoControlsToolStripMenuItem});
            this.toolStripMenuItemVideo.Name = "toolStripMenuItemVideo";
            this.toolStripMenuItemVideo.Size = new System.Drawing.Size(50, 20);
            this.toolStripMenuItemVideo.Text = "Video";
            this.toolStripMenuItemVideo.DropDownClosed += new System.EventHandler(this.ToolStripMenuItemVideoDropDownClosed);
            this.toolStripMenuItemVideo.DropDownOpening += new System.EventHandler(this.ToolStripMenuItemVideoDropDownOpening);
            // 
            // openVideoToolStripMenuItem
            // 
            this.openVideoToolStripMenuItem.Name = "openVideoToolStripMenuItem";
            this.openVideoToolStripMenuItem.Size = new System.Drawing.Size(257, 22);
            this.openVideoToolStripMenuItem.Text = "Open video...";
            this.openVideoToolStripMenuItem.Click += new System.EventHandler(this.buttonOpenVideo_Click);
            // 
            // toolStripMenuItemOpenVideoFromUrl
            // 
            this.toolStripMenuItemOpenVideoFromUrl.Name = "toolStripMenuItemOpenVideoFromUrl";
            this.toolStripMenuItemOpenVideoFromUrl.Size = new System.Drawing.Size(257, 22);
            this.toolStripMenuItemOpenVideoFromUrl.Text = "Open video from url...";
            this.toolStripMenuItemOpenVideoFromUrl.Click += new System.EventHandler(this.toolStripMenuItemOpenVideoFromUrl_Click);
            // 
            // toolStripMenuItemOpenDvd
            // 
            this.toolStripMenuItemOpenDvd.Name = "toolStripMenuItemOpenDvd";
            this.toolStripMenuItemOpenDvd.Size = new System.Drawing.Size(257, 22);
            this.toolStripMenuItemOpenDvd.Text = "Open DVD...";
            this.toolStripMenuItemOpenDvd.Click += new System.EventHandler(this.toolStripMenuItemOpenDvd_Click);
            // 
            // toolStripMenuItemSetAudioTrack
            // 
            this.toolStripMenuItemSetAudioTrack.Name = "toolStripMenuItemSetAudioTrack";
            this.toolStripMenuItemSetAudioTrack.Size = new System.Drawing.Size(257, 22);
            this.toolStripMenuItemSetAudioTrack.Text = "Choose audio track";
            // 
            // closeVideoToolStripMenuItem
            // 
            this.closeVideoToolStripMenuItem.Name = "closeVideoToolStripMenuItem";
            this.closeVideoToolStripMenuItem.Size = new System.Drawing.Size(257, 22);
            this.closeVideoToolStripMenuItem.Text = "Close video";
            this.closeVideoToolStripMenuItem.Click += new System.EventHandler(this.CloseVideoToolStripMenuItemClick);
            // 
            // setVideoOffsetToolStripMenuItem
            // 
            this.setVideoOffsetToolStripMenuItem.Name = "setVideoOffsetToolStripMenuItem";
            this.setVideoOffsetToolStripMenuItem.Size = new System.Drawing.Size(257, 22);
            this.setVideoOffsetToolStripMenuItem.Text = "Set video offset...";
            this.setVideoOffsetToolStripMenuItem.Click += new System.EventHandler(this.setVideoOffsetToolStripMenuItem_Click);
            // 
            // smpteTimeModedropFrameToolStripMenuItem
            // 
            this.smpteTimeModedropFrameToolStripMenuItem.Name = "smpteTimeModedropFrameToolStripMenuItem";
            this.smpteTimeModedropFrameToolStripMenuItem.Size = new System.Drawing.Size(257, 22);
            this.smpteTimeModedropFrameToolStripMenuItem.Text = "SMPTE timing (drop frame)";
            this.smpteTimeModedropFrameToolStripMenuItem.Click += new System.EventHandler(this.SmpteTimeModedropFrameToolStripMenuItem_Click);
            // 
            // toolStripMenuItemImportSceneChanges
            // 
            this.toolStripMenuItemImportSceneChanges.Name = "toolStripMenuItemImportSceneChanges";
            this.toolStripMenuItemImportSceneChanges.Size = new System.Drawing.Size(257, 22);
            this.toolStripMenuItemImportSceneChanges.Text = "Import scene changes...";
            this.toolStripMenuItemImportSceneChanges.Click += new System.EventHandler(this.toolStripMenuItemImportSceneChanges_Click);
            // 
            // toolStripMenuItemRemoveSceneChanges
            // 
            this.toolStripMenuItemRemoveSceneChanges.Name = "toolStripMenuItemRemoveSceneChanges";
            this.toolStripMenuItemRemoveSceneChanges.Size = new System.Drawing.Size(257, 22);
            this.toolStripMenuItemRemoveSceneChanges.Text = "Remove scene changes";
            this.toolStripMenuItemRemoveSceneChanges.Click += new System.EventHandler(this.toolStripMenuItemRemoveSceneChanges_Click);
            // 
            // toolStripMenuItemAddWaveformBatch
            // 
            this.toolStripMenuItemAddWaveformBatch.Name = "toolStripMenuItemAddWaveformBatch";
            this.toolStripMenuItemAddWaveformBatch.Size = new System.Drawing.Size(257, 22);
            this.toolStripMenuItemAddWaveformBatch.Text = "Add waveform batch...";
            this.toolStripMenuItemAddWaveformBatch.Click += new System.EventHandler(this.ToolStripMenuItemAddWaveformBatchClick);
            // 
            // generateTextFromCurrentVideoToolStripMenuItem
            // 
            this.generateTextFromCurrentVideoToolStripMenuItem.Name = "generateTextFromCurrentVideoToolStripMenuItem";
            this.generateTextFromCurrentVideoToolStripMenuItem.Size = new System.Drawing.Size(257, 22);
            this.generateTextFromCurrentVideoToolStripMenuItem.Text = "Generate text from current video...";
            this.generateTextFromCurrentVideoToolStripMenuItem.Click += new System.EventHandler(this.generateTextFromCurrentVideoToolStripMenuItem_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(254, 6);
            // 
            // showhideWaveformToolStripMenuItem
            // 
            this.showhideWaveformToolStripMenuItem.Name = "showhideWaveformToolStripMenuItem";
            this.showhideWaveformToolStripMenuItem.Size = new System.Drawing.Size(257, 22);
            this.showhideWaveformToolStripMenuItem.Text = "Show/hide waveform";
            this.showhideWaveformToolStripMenuItem.Click += new System.EventHandler(this.ShowhideWaveformToolStripMenuItemClick);
            // 
            // showhideVideoToolStripMenuItem
            // 
            this.showhideVideoToolStripMenuItem.Name = "showhideVideoToolStripMenuItem";
            this.showhideVideoToolStripMenuItem.Size = new System.Drawing.Size(257, 22);
            this.showhideVideoToolStripMenuItem.Text = "Show/hide video";
            this.showhideVideoToolStripMenuItem.Click += new System.EventHandler(this.toolStripButtonToggleVideo_Click);
            // 
            // toolStripSeparator19
            // 
            this.toolStripSeparator19.Name = "toolStripSeparator19";
            this.toolStripSeparator19.Size = new System.Drawing.Size(254, 6);
            // 
            // undockVideoControlsToolStripMenuItem
            // 
            this.undockVideoControlsToolStripMenuItem.Name = "undockVideoControlsToolStripMenuItem";
            this.undockVideoControlsToolStripMenuItem.Size = new System.Drawing.Size(257, 22);
            this.undockVideoControlsToolStripMenuItem.Text = "Un-dock video controls";
            this.undockVideoControlsToolStripMenuItem.Click += new System.EventHandler(this.UndockVideoControlsToolStripMenuItemClick);
            // 
            // redockVideoControlsToolStripMenuItem
            // 
            this.redockVideoControlsToolStripMenuItem.Name = "redockVideoControlsToolStripMenuItem";
            this.redockVideoControlsToolStripMenuItem.Size = new System.Drawing.Size(257, 22);
            this.redockVideoControlsToolStripMenuItem.Text = "Re-dock video controls";
            this.redockVideoControlsToolStripMenuItem.Visible = false;
            this.redockVideoControlsToolStripMenuItem.Click += new System.EventHandler(this.RedockVideoControlsToolStripMenuItemClick);
            // 
            // toolStripMenuItemSynchronization
            // 
            this.toolStripMenuItemSynchronization.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemAdjustAllTimes,
            this.visualSyncToolStripMenuItem,
            this.toolStripMenuItemPointSync,
            this.pointSyncViaOtherSubtitleToolStripMenuItem,
            this.toolStripMenuItemChangeFrameRate2,
            this.changeSpeedInPercentToolStripMenuItem});
            this.toolStripMenuItemSynchronization.Name = "toolStripMenuItemSynchronization";
            this.toolStripMenuItemSynchronization.Size = new System.Drawing.Size(106, 20);
            this.toolStripMenuItemSynchronization.Text = "Synchronization";
            // 
            // toolStripMenuItemAdjustAllTimes
            // 
            this.toolStripMenuItemAdjustAllTimes.Name = "toolStripMenuItemAdjustAllTimes";
            this.toolStripMenuItemAdjustAllTimes.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.A)));
            this.toolStripMenuItemAdjustAllTimes.Size = new System.Drawing.Size(324, 22);
            this.toolStripMenuItemAdjustAllTimes.Text = "Adjust times (show earlier/later)...";
            this.toolStripMenuItemAdjustAllTimes.Click += new System.EventHandler(this.toolStripMenuItemAdjustAllTimes_Click);
            // 
            // visualSyncToolStripMenuItem
            // 
            this.visualSyncToolStripMenuItem.Name = "visualSyncToolStripMenuItem";
            this.visualSyncToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.V)));
            this.visualSyncToolStripMenuItem.Size = new System.Drawing.Size(324, 22);
            this.visualSyncToolStripMenuItem.Text = "Visual sync...";
            this.visualSyncToolStripMenuItem.Click += new System.EventHandler(this.VisualSyncToolStripMenuItemClick);
            // 
            // toolStripMenuItemPointSync
            // 
            this.toolStripMenuItemPointSync.Name = "toolStripMenuItemPointSync";
            this.toolStripMenuItemPointSync.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.P)));
            this.toolStripMenuItemPointSync.Size = new System.Drawing.Size(324, 22);
            this.toolStripMenuItemPointSync.Text = "Point sync...";
            this.toolStripMenuItemPointSync.Click += new System.EventHandler(this.toolStripMenuItemPointSync_Click);
            // 
            // pointSyncViaOtherSubtitleToolStripMenuItem
            // 
            this.pointSyncViaOtherSubtitleToolStripMenuItem.Name = "pointSyncViaOtherSubtitleToolStripMenuItem";
            this.pointSyncViaOtherSubtitleToolStripMenuItem.Size = new System.Drawing.Size(324, 22);
            this.pointSyncViaOtherSubtitleToolStripMenuItem.Text = "Point sync via other subtitle...";
            this.pointSyncViaOtherSubtitleToolStripMenuItem.Click += new System.EventHandler(this.pointSyncViaOtherSubtitleToolStripMenuItem_Click);
            // 
            // toolStripMenuItemChangeFrameRate2
            // 
            this.toolStripMenuItemChangeFrameRate2.Name = "toolStripMenuItemChangeFrameRate2";
            this.toolStripMenuItemChangeFrameRate2.Size = new System.Drawing.Size(324, 22);
            this.toolStripMenuItemChangeFrameRate2.Text = "Change frame rate...";
            this.toolStripMenuItemChangeFrameRate2.Click += new System.EventHandler(this.ToolStripMenuItemChangeFrameRateClick);
            // 
            // changeSpeedInPercentToolStripMenuItem
            // 
            this.changeSpeedInPercentToolStripMenuItem.Name = "changeSpeedInPercentToolStripMenuItem";
            this.changeSpeedInPercentToolStripMenuItem.Size = new System.Drawing.Size(324, 22);
            this.changeSpeedInPercentToolStripMenuItem.Text = "Change speed in percent...";
            this.changeSpeedInPercentToolStripMenuItem.Click += new System.EventHandler(this.changeSpeedInPercentToolStripMenuItem_Click);
            // 
            // toolStripMenuItemAutoTranslate
            // 
            this.toolStripMenuItemAutoTranslate.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.translatepoweredByMicrosoftToolStripMenuItem,
            this.translateByGoogleToolStripMenuItem,
            this.translateFromSwedishToDanishToolStripMenuItem});
            this.toolStripMenuItemAutoTranslate.Name = "toolStripMenuItemAutoTranslate";
            this.toolStripMenuItemAutoTranslate.Size = new System.Drawing.Size(94, 20);
            this.toolStripMenuItemAutoTranslate.Text = "Auto-translate";
            this.toolStripMenuItemAutoTranslate.DropDownOpening += new System.EventHandler(this.toolStripMenuItemAutoTranslate_DropDownOpening);
            // 
            // translatepoweredByMicrosoftToolStripMenuItem
            // 
            this.translatepoweredByMicrosoftToolStripMenuItem.Name = "translatepoweredByMicrosoftToolStripMenuItem";
            this.translatepoweredByMicrosoftToolStripMenuItem.Size = new System.Drawing.Size(402, 22);
            this.translatepoweredByMicrosoftToolStripMenuItem.Text = "Translate (powered by Microsoft)...";
            this.translatepoweredByMicrosoftToolStripMenuItem.Click += new System.EventHandler(this.translatepoweredByMicrosoftToolStripMenuItem_Click);
            // 
            // translateByGoogleToolStripMenuItem
            // 
            this.translateByGoogleToolStripMenuItem.Name = "translateByGoogleToolStripMenuItem";
            this.translateByGoogleToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.G)));
            this.translateByGoogleToolStripMenuItem.Size = new System.Drawing.Size(402, 22);
            this.translateByGoogleToolStripMenuItem.Text = "Translate (powered by Google)...";
            this.translateByGoogleToolStripMenuItem.Click += new System.EventHandler(this.TranslateByGoogleToolStripMenuItemClick);
            // 
            // translateFromSwedishToDanishToolStripMenuItem
            // 
            this.translateFromSwedishToDanishToolStripMenuItem.Name = "translateFromSwedishToDanishToolStripMenuItem";
            this.translateFromSwedishToDanishToolStripMenuItem.Size = new System.Drawing.Size(402, 22);
            this.translateFromSwedishToDanishToolStripMenuItem.Text = "Translate from swedish to danish (powered by nikse.dk/mt)...";
            this.translateFromSwedishToDanishToolStripMenuItem.Click += new System.EventHandler(this.TranslateFromSwedishToDanishToolStripMenuItemClick);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.settingsToolStripMenuItem,
            this.changeLanguageToolStripMenuItem});
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.optionsToolStripMenuItem.Text = "Options";
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(181, 22);
            this.settingsToolStripMenuItem.Text = "Settings";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.SettingsToolStripMenuItemClick);
            // 
            // changeLanguageToolStripMenuItem
            // 
            this.changeLanguageToolStripMenuItem.Name = "changeLanguageToolStripMenuItem";
            this.changeLanguageToolStripMenuItem.Size = new System.Drawing.Size(181, 22);
            this.changeLanguageToolStripMenuItem.Text = "Change language...";
            this.changeLanguageToolStripMenuItem.Click += new System.EventHandler(this.ChangeLanguageToolStripMenuItemClick);
            // 
            // toolStripMenuItemNetworking
            // 
            this.toolStripMenuItemNetworking.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.startServerToolStripMenuItem,
            this.joinSessionToolStripMenuItem,
            this.chatToolStripMenuItem,
            this.showSessionKeyLogToolStripMenuItem,
            this.leaveSessionToolStripMenuItem});
            this.toolStripMenuItemNetworking.Name = "toolStripMenuItemNetworking";
            this.toolStripMenuItemNetworking.Size = new System.Drawing.Size(81, 20);
            this.toolStripMenuItemNetworking.Text = "Networking";
            this.toolStripMenuItemNetworking.DropDownOpening += new System.EventHandler(this.toolStripMenuItemNetworking_DropDownOpening);
            // 
            // startServerToolStripMenuItem
            // 
            this.startServerToolStripMenuItem.Name = "startServerToolStripMenuItem";
            this.startServerToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
            this.startServerToolStripMenuItem.Text = "Start new session";
            this.startServerToolStripMenuItem.Click += new System.EventHandler(this.startServerToolStripMenuItem_Click);
            // 
            // joinSessionToolStripMenuItem
            // 
            this.joinSessionToolStripMenuItem.Name = "joinSessionToolStripMenuItem";
            this.joinSessionToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
            this.joinSessionToolStripMenuItem.Text = "Join session";
            this.joinSessionToolStripMenuItem.Click += new System.EventHandler(this.joinSessionToolStripMenuItem_Click);
            // 
            // chatToolStripMenuItem
            // 
            this.chatToolStripMenuItem.Name = "chatToolStripMenuItem";
            this.chatToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
            this.chatToolStripMenuItem.Text = "Chat";
            this.chatToolStripMenuItem.Click += new System.EventHandler(this.chatToolStripMenuItem_Click);
            // 
            // showSessionKeyLogToolStripMenuItem
            // 
            this.showSessionKeyLogToolStripMenuItem.Name = "showSessionKeyLogToolStripMenuItem";
            this.showSessionKeyLogToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
            this.showSessionKeyLogToolStripMenuItem.Text = "Show session info and log";
            this.showSessionKeyLogToolStripMenuItem.Click += new System.EventHandler(this.showSessionKeyLogToolStripMenuItem_Click);
            // 
            // leaveSessionToolStripMenuItem
            // 
            this.leaveSessionToolStripMenuItem.Name = "leaveSessionToolStripMenuItem";
            this.leaveSessionToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
            this.leaveSessionToolStripMenuItem.Text = "Leave session";
            this.leaveSessionToolStripMenuItem.Click += new System.EventHandler(this.LeaveSessionToolStripMenuItemClick);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.checkForUpdatesToolStripMenuItem,
            this.toolStripMenuItemSplitterCheckForUpdates,
            this.helpToolStripMenuItem1,
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(45, 20);
            this.helpToolStripMenuItem.Text = "Help";
            this.helpToolStripMenuItem.DropDownClosed += new System.EventHandler(this.MenuClosed);
            this.helpToolStripMenuItem.DropDownOpening += new System.EventHandler(this.MenuOpened);
            // 
            // checkForUpdatesToolStripMenuItem
            // 
            this.checkForUpdatesToolStripMenuItem.Name = "checkForUpdatesToolStripMenuItem";
            this.checkForUpdatesToolStripMenuItem.Size = new System.Drawing.Size(181, 22);
            this.checkForUpdatesToolStripMenuItem.Text = "Check for updates...";
            this.checkForUpdatesToolStripMenuItem.Click += new System.EventHandler(this.checkForUpdatesToolStripMenuItem_Click);
            // 
            // toolStripMenuItemSplitterCheckForUpdates
            // 
            this.toolStripMenuItemSplitterCheckForUpdates.Name = "toolStripMenuItemSplitterCheckForUpdates";
            this.toolStripMenuItemSplitterCheckForUpdates.Size = new System.Drawing.Size(178, 6);
            // 
            // helpToolStripMenuItem1
            // 
            this.helpToolStripMenuItem1.Name = "helpToolStripMenuItem1";
            this.helpToolStripMenuItem1.ShortcutKeys = System.Windows.Forms.Keys.F1;
            this.helpToolStripMenuItem1.Size = new System.Drawing.Size(181, 22);
            this.helpToolStripMenuItem1.Text = "Help";
            this.helpToolStripMenuItem1.Click += new System.EventHandler(this.HelpToolStripMenuItem1Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(181, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.AboutToolStripMenuItemClick);
            // 
            // contextMenuStripListview
            // 
            this.contextMenuStripListview.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.setStylesForSelectedLinesToolStripMenuItem,
            this.setActorForSelectedLinesToolStripMenuItem,
            this.toolStripMenuItemAssStyles,
            this.toolStripMenuItemSetRegion,
            this.toolStripMenuItemSetLanguage,
            this.toolStripMenuItemWebVTT,
            this.toolStripMenuItemDelete,
            this.toolStripMenuItemInsertBefore,
            this.toolStripMenuItemInsertAfter,
            this.toolStripMenuItemInsertSubtitle,
            this.toolStripMenuItemCopySourceText,
            this.toolStripMenuItemColumn,
            this.toolStripMenuItemBookmark,
            this.toolStripSeparator7,
            this.splitLineToolStripMenuItem,
            this.toolStripMenuItemMergeLines,
            this.toolStripMenuItemMergeDialog,
            this.mergeBeforeToolStripMenuItem,
            this.mergeAfterToolStripMenuItem,
            this.toolStripSeparator8,
            this.removeFormattinglToolStripMenuItem,
            this.boldToolStripMenuItem,
            this.italicToolStripMenuItem,
            this.boxToolStripMenuItem,
            this.underlineToolStripMenuItem,
            this.colorToolStripMenuItem,
            this.toolStripMenuItemFont,
            this.toolStripMenuItemAlignment,
            this.toolStripMenuItemSurroundWithMusicSymbols,
            this.toolStripSeparator2,
            this.toolStripMenuItemAutoBreakLines,
            this.toolStripMenuItemUnbreakLines,
            this.toolStripSeparatorBreakLines,
            this.typeEffectToolStripMenuItem,
            this.karokeeEffectToolStripMenuItem,
            this.toolStripSeparatorAdvancedFunctions,
            this.showSelectedLinesEarlierlaterToolStripMenuItem,
            this.visualSyncSelectedLinesToolStripMenuItem,
            this.toolStripMenuItemGoogleMicrosoftTranslateSelLine,
            this.toolStripMenuItemTranslateSelected,
            this.adjustDisplayTimeForSelectedLinesToolStripMenuItem,
            this.fixCommonErrorsInSelectedLinesToolStripMenuItem,
            this.changeCasingForSelectedLinesToolStripMenuItem,
            this.toolStripMenuItemSaveSelectedLines});
            this.contextMenuStripListview.Name = "contextMenuStripListview";
            this.contextMenuStripListview.Size = new System.Drawing.Size(285, 892);
            this.contextMenuStripListview.Closed += new System.Windows.Forms.ToolStripDropDownClosedEventHandler(this.MenuClosed);
            this.contextMenuStripListview.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenuStripListviewOpening);
            this.contextMenuStripListview.Opened += new System.EventHandler(this.MenuOpened);
            // 
            // setStylesForSelectedLinesToolStripMenuItem
            // 
            this.setStylesForSelectedLinesToolStripMenuItem.Name = "setStylesForSelectedLinesToolStripMenuItem";
            this.setStylesForSelectedLinesToolStripMenuItem.Size = new System.Drawing.Size(284, 22);
            this.setStylesForSelectedLinesToolStripMenuItem.Text = "ASS: Set styles for selected lines...";
            // 
            // setActorForSelectedLinesToolStripMenuItem
            // 
            this.setActorForSelectedLinesToolStripMenuItem.Name = "setActorForSelectedLinesToolStripMenuItem";
            this.setActorForSelectedLinesToolStripMenuItem.Size = new System.Drawing.Size(284, 22);
            this.setActorForSelectedLinesToolStripMenuItem.Text = "ASS: Set styles for selected lines...";
            // 
            // toolStripMenuItemAssStyles
            // 
            this.toolStripMenuItemAssStyles.Name = "toolStripMenuItemAssStyles";
            this.toolStripMenuItemAssStyles.Size = new System.Drawing.Size(284, 22);
            this.toolStripMenuItemAssStyles.Text = "ASS: Styles...";
            this.toolStripMenuItemAssStyles.Click += new System.EventHandler(this.toolStripMenuItemAssStyles_Click);
            // 
            // toolStripMenuItemSetRegion
            // 
            this.toolStripMenuItemSetRegion.Name = "toolStripMenuItemSetRegion";
            this.toolStripMenuItemSetRegion.Size = new System.Drawing.Size(284, 22);
            this.toolStripMenuItemSetRegion.Text = "Timed text - set region";
            // 
            // toolStripMenuItemSetLanguage
            // 
            this.toolStripMenuItemSetLanguage.Name = "toolStripMenuItemSetLanguage";
            this.toolStripMenuItemSetLanguage.Size = new System.Drawing.Size(284, 22);
            this.toolStripMenuItemSetLanguage.Text = "Timed text - set language";
            // 
            // toolStripMenuItemWebVTT
            // 
            this.toolStripMenuItemWebVTT.Name = "toolStripMenuItemWebVTT";
            this.toolStripMenuItemWebVTT.Size = new System.Drawing.Size(284, 22);
            this.toolStripMenuItemWebVTT.Text = "WebVTT voice";
            // 
            // toolStripMenuItemDelete
            // 
            this.toolStripMenuItemDelete.Name = "toolStripMenuItemDelete";
            this.toolStripMenuItemDelete.Size = new System.Drawing.Size(284, 22);
            this.toolStripMenuItemDelete.Text = "Delete";
            this.toolStripMenuItemDelete.Click += new System.EventHandler(this.ToolStripMenuItemDeleteClick);
            // 
            // toolStripMenuItemInsertBefore
            // 
            this.toolStripMenuItemInsertBefore.Name = "toolStripMenuItemInsertBefore";
            this.toolStripMenuItemInsertBefore.Size = new System.Drawing.Size(284, 22);
            this.toolStripMenuItemInsertBefore.Text = "Insert before";
            this.toolStripMenuItemInsertBefore.Click += new System.EventHandler(this.ToolStripMenuItemInsertBeforeClick);
            // 
            // toolStripMenuItemInsertAfter
            // 
            this.toolStripMenuItemInsertAfter.Name = "toolStripMenuItemInsertAfter";
            this.toolStripMenuItemInsertAfter.Size = new System.Drawing.Size(284, 22);
            this.toolStripMenuItemInsertAfter.Text = "Insert after";
            this.toolStripMenuItemInsertAfter.Click += new System.EventHandler(this.ToolStripMenuItemInsertAfterClick);
            // 
            // toolStripMenuItemInsertSubtitle
            // 
            this.toolStripMenuItemInsertSubtitle.Name = "toolStripMenuItemInsertSubtitle";
            this.toolStripMenuItemInsertSubtitle.Size = new System.Drawing.Size(284, 22);
            this.toolStripMenuItemInsertSubtitle.Text = "Insert subtitle file after this line...";
            this.toolStripMenuItemInsertSubtitle.Click += new System.EventHandler(this.ToolStripMenuItemInsertSubtitleClick);
            // 
            // toolStripMenuItemCopySourceText
            // 
            this.toolStripMenuItemCopySourceText.Name = "toolStripMenuItemCopySourceText";
            this.toolStripMenuItemCopySourceText.Size = new System.Drawing.Size(284, 22);
            this.toolStripMenuItemCopySourceText.Text = "Copy as text to clipboard";
            this.toolStripMenuItemCopySourceText.Click += new System.EventHandler(this.ToolStripMenuItemCopySourceTextClick);
            // 
            // toolStripMenuItemColumn
            // 
            this.toolStripMenuItemColumn.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.columnDeleteTextOnlyToolStripMenuItem,
            this.toolStripMenuItemColumnDeleteText,
            this.ShiftTextCellsDownToolStripMenuItem,
            this.toolStripMenuItemInsertTextFromSub,
            this.toolStripMenuItemColumnImportText,
            this.toolStripMenuItemPasteSpecial,
            this.copyOriginalTextToCurrentToolStripMenuItem,
            this.moveTextUpToolStripMenuItem,
            this.moveTextDownToolStripMenuItem});
            this.toolStripMenuItemColumn.Name = "toolStripMenuItemColumn";
            this.toolStripMenuItemColumn.Size = new System.Drawing.Size(284, 22);
            this.toolStripMenuItemColumn.Text = "Column";
            this.toolStripMenuItemColumn.DropDownOpening += new System.EventHandler(this.toolStripMenuItemColumn_DropDownOpening);
            // 
            // columnDeleteTextOnlyToolStripMenuItem
            // 
            this.columnDeleteTextOnlyToolStripMenuItem.Name = "columnDeleteTextOnlyToolStripMenuItem";
            this.columnDeleteTextOnlyToolStripMenuItem.Size = new System.Drawing.Size(314, 22);
            this.columnDeleteTextOnlyToolStripMenuItem.Text = "Delete text";
            this.columnDeleteTextOnlyToolStripMenuItem.Click += new System.EventHandler(this.columnDeleteTextOnlyToolStripMenuItem_Click);
            // 
            // toolStripMenuItemColumnDeleteText
            // 
            this.toolStripMenuItemColumnDeleteText.Name = "toolStripMenuItemColumnDeleteText";
            this.toolStripMenuItemColumnDeleteText.Size = new System.Drawing.Size(314, 22);
            this.toolStripMenuItemColumnDeleteText.Text = "Delete text and shift text cells up";
            this.toolStripMenuItemColumnDeleteText.Click += new System.EventHandler(this.deleteAndShiftCellsUpToolStripMenuItem_Click);
            // 
            // ShiftTextCellsDownToolStripMenuItem
            // 
            this.ShiftTextCellsDownToolStripMenuItem.Name = "ShiftTextCellsDownToolStripMenuItem";
            this.ShiftTextCellsDownToolStripMenuItem.Size = new System.Drawing.Size(314, 22);
            this.ShiftTextCellsDownToolStripMenuItem.Text = "Insert and shift text cells down";
            this.ShiftTextCellsDownToolStripMenuItem.Click += new System.EventHandler(this.ShiftTextCellsDownToolStripMenuItem_Click);
            // 
            // toolStripMenuItemInsertTextFromSub
            // 
            this.toolStripMenuItemInsertTextFromSub.Name = "toolStripMenuItemInsertTextFromSub";
            this.toolStripMenuItemInsertTextFromSub.Size = new System.Drawing.Size(314, 22);
            this.toolStripMenuItemInsertTextFromSub.Text = "Insert text from subtitle and shift cells down...";
            this.toolStripMenuItemInsertTextFromSub.Click += new System.EventHandler(this.toolStripMenuItemInsertTextFromSub_Click);
            // 
            // toolStripMenuItemColumnImportText
            // 
            this.toolStripMenuItemColumnImportText.Name = "toolStripMenuItemColumnImportText";
            this.toolStripMenuItemColumnImportText.Size = new System.Drawing.Size(314, 22);
            this.toolStripMenuItemColumnImportText.Text = "Import text and shift text cells down...";
            this.toolStripMenuItemColumnImportText.Click += new System.EventHandler(this.toolStripMenuItemColumnImportText_Click);
            // 
            // toolStripMenuItemPasteSpecial
            // 
            this.toolStripMenuItemPasteSpecial.Name = "toolStripMenuItemPasteSpecial";
            this.toolStripMenuItemPasteSpecial.Size = new System.Drawing.Size(314, 22);
            this.toolStripMenuItemPasteSpecial.Text = "Paste from clipboard...";
            this.toolStripMenuItemPasteSpecial.Click += new System.EventHandler(this.toolStripMenuItemPasteSpecial_Click);
            // 
            // copyOriginalTextToCurrentToolStripMenuItem
            // 
            this.copyOriginalTextToCurrentToolStripMenuItem.Name = "copyOriginalTextToCurrentToolStripMenuItem";
            this.copyOriginalTextToCurrentToolStripMenuItem.Size = new System.Drawing.Size(314, 22);
            this.copyOriginalTextToCurrentToolStripMenuItem.Text = "Copy original text to current";
            this.copyOriginalTextToCurrentToolStripMenuItem.Click += new System.EventHandler(this.copyOriginalTextToCurrentToolStripMenuItem_Click);
            // 
            // moveTextUpToolStripMenuItem
            // 
            this.moveTextUpToolStripMenuItem.Name = "moveTextUpToolStripMenuItem";
            this.moveTextUpToolStripMenuItem.Size = new System.Drawing.Size(314, 22);
            this.moveTextUpToolStripMenuItem.Text = "Move text up";
            this.moveTextUpToolStripMenuItem.Click += new System.EventHandler(this.moveTextUpToolStripMenuItem_Click);
            // 
            // moveTextDownToolStripMenuItem
            // 
            this.moveTextDownToolStripMenuItem.Name = "moveTextDownToolStripMenuItem";
            this.moveTextDownToolStripMenuItem.Size = new System.Drawing.Size(314, 22);
            this.moveTextDownToolStripMenuItem.Text = "Move text down";
            this.moveTextDownToolStripMenuItem.Click += new System.EventHandler(this.moveTextDownToolStripMenuItem_Click);
            // 
            // toolStripMenuItemBookmark
            // 
            this.toolStripMenuItemBookmark.Name = "toolStripMenuItemBookmark";
            this.toolStripMenuItemBookmark.Size = new System.Drawing.Size(284, 22);
            this.toolStripMenuItemBookmark.Text = "Bookmark...";
            this.toolStripMenuItemBookmark.Click += new System.EventHandler(this.toolStripMenuItemBookmark_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(281, 6);
            // 
            // splitLineToolStripMenuItem
            // 
            this.splitLineToolStripMenuItem.Name = "splitLineToolStripMenuItem";
            this.splitLineToolStripMenuItem.Size = new System.Drawing.Size(284, 22);
            this.splitLineToolStripMenuItem.Text = "Split";
            this.splitLineToolStripMenuItem.Click += new System.EventHandler(this.SplitLineToolStripMenuItemClick);
            // 
            // toolStripMenuItemMergeLines
            // 
            this.toolStripMenuItemMergeLines.Name = "toolStripMenuItemMergeLines";
            this.toolStripMenuItemMergeLines.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.M)));
            this.toolStripMenuItemMergeLines.Size = new System.Drawing.Size(284, 22);
            this.toolStripMenuItemMergeLines.Text = "Merge selected lines";
            this.toolStripMenuItemMergeLines.Click += new System.EventHandler(this.ToolStripMenuItemMergeLinesClick);
            // 
            // toolStripMenuItemMergeDialog
            // 
            this.toolStripMenuItemMergeDialog.Name = "toolStripMenuItemMergeDialog";
            this.toolStripMenuItemMergeDialog.Size = new System.Drawing.Size(284, 22);
            this.toolStripMenuItemMergeDialog.Text = "Merge selected lines as dialog";
            this.toolStripMenuItemMergeDialog.Click += new System.EventHandler(this.ToolStripMenuItemMergeDialogClick);
            // 
            // mergeBeforeToolStripMenuItem
            // 
            this.mergeBeforeToolStripMenuItem.Name = "mergeBeforeToolStripMenuItem";
            this.mergeBeforeToolStripMenuItem.Size = new System.Drawing.Size(284, 22);
            this.mergeBeforeToolStripMenuItem.Text = "Merge with line before";
            this.mergeBeforeToolStripMenuItem.Click += new System.EventHandler(this.MergeBeforeToolStripMenuItemClick);
            // 
            // mergeAfterToolStripMenuItem
            // 
            this.mergeAfterToolStripMenuItem.Name = "mergeAfterToolStripMenuItem";
            this.mergeAfterToolStripMenuItem.Size = new System.Drawing.Size(284, 22);
            this.mergeAfterToolStripMenuItem.Text = "Merge with line after";
            this.mergeAfterToolStripMenuItem.Click += new System.EventHandler(this.MergeAfterToolStripMenuItemClick);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(281, 6);
            // 
            // removeFormattinglToolStripMenuItem
            // 
            this.removeFormattinglToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.removeAllFormattingsToolStripMenuItem,
            this.removeBoldToolStripMenuItem,
            this.removeItalicToolStripMenuItem,
            this.removeUnderlineToolStripMenuItem,
            this.removeColorToolStripMenuItem,
            this.removeFontNameToolStripMenuItem,
            this.removeAlignmentToolStripMenuItem});
            this.removeFormattinglToolStripMenuItem.Name = "removeFormattinglToolStripMenuItem";
            this.removeFormattinglToolStripMenuItem.Size = new System.Drawing.Size(284, 22);
            this.removeFormattinglToolStripMenuItem.Text = "Remove formatting";
            // 
            // removeAllFormattingsToolStripMenuItem
            // 
            this.removeAllFormattingsToolStripMenuItem.Name = "removeAllFormattingsToolStripMenuItem";
            this.removeAllFormattingsToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            this.removeAllFormattingsToolStripMenuItem.Text = "Remove all formattings";
            this.removeAllFormattingsToolStripMenuItem.Click += new System.EventHandler(this.removeAllFormattingsToolStripMenuItem_Click);
            // 
            // removeBoldToolStripMenuItem
            // 
            this.removeBoldToolStripMenuItem.Name = "removeBoldToolStripMenuItem";
            this.removeBoldToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            this.removeBoldToolStripMenuItem.Text = "Remove bold";
            this.removeBoldToolStripMenuItem.Click += new System.EventHandler(this.removeBoldToolStripMenuItem_Click);
            // 
            // removeItalicToolStripMenuItem
            // 
            this.removeItalicToolStripMenuItem.Name = "removeItalicToolStripMenuItem";
            this.removeItalicToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            this.removeItalicToolStripMenuItem.Text = "Remove italic";
            this.removeItalicToolStripMenuItem.Click += new System.EventHandler(this.removeItalicToolStripMenuItem_Click);
            // 
            // removeUnderlineToolStripMenuItem
            // 
            this.removeUnderlineToolStripMenuItem.Name = "removeUnderlineToolStripMenuItem";
            this.removeUnderlineToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            this.removeUnderlineToolStripMenuItem.Text = "Remove underline";
            this.removeUnderlineToolStripMenuItem.Click += new System.EventHandler(this.removeUnderlineToolStripMenuItem_Click);
            // 
            // removeColorToolStripMenuItem
            // 
            this.removeColorToolStripMenuItem.Name = "removeColorToolStripMenuItem";
            this.removeColorToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            this.removeColorToolStripMenuItem.Text = "Remove color";
            this.removeColorToolStripMenuItem.Click += new System.EventHandler(this.removeColorToolStripMenuItem_Click);
            // 
            // removeFontNameToolStripMenuItem
            // 
            this.removeFontNameToolStripMenuItem.Name = "removeFontNameToolStripMenuItem";
            this.removeFontNameToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            this.removeFontNameToolStripMenuItem.Text = "Remove font name";
            this.removeFontNameToolStripMenuItem.Click += new System.EventHandler(this.removeFontNameToolStripMenuItem_Click);
            // 
            // removeAlignmentToolStripMenuItem
            // 
            this.removeAlignmentToolStripMenuItem.Name = "removeAlignmentToolStripMenuItem";
            this.removeAlignmentToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            this.removeAlignmentToolStripMenuItem.Text = "Remove alignment";
            this.removeAlignmentToolStripMenuItem.Click += new System.EventHandler(this.removeAlignmentToolStripMenuItem_Click);
            // 
            // boldToolStripMenuItem
            // 
            this.boldToolStripMenuItem.Name = "boldToolStripMenuItem";
            this.boldToolStripMenuItem.Size = new System.Drawing.Size(284, 22);
            this.boldToolStripMenuItem.Text = "Bold";
            this.boldToolStripMenuItem.Click += new System.EventHandler(this.BoldToolStripMenuItemClick);
            // 
            // italicToolStripMenuItem
            // 
            this.italicToolStripMenuItem.Name = "italicToolStripMenuItem";
            this.italicToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.I)));
            this.italicToolStripMenuItem.Size = new System.Drawing.Size(284, 22);
            this.italicToolStripMenuItem.Text = "Italic";
            this.italicToolStripMenuItem.Click += new System.EventHandler(this.ItalicToolStripMenuItemClick);
            // 
            // boxToolStripMenuItem
            // 
            this.boxToolStripMenuItem.Name = "boxToolStripMenuItem";
            this.boxToolStripMenuItem.Size = new System.Drawing.Size(284, 22);
            this.boxToolStripMenuItem.Text = "Box";
            this.boxToolStripMenuItem.Click += new System.EventHandler(this.boxToolStripMenuItem_Click);
            // 
            // underlineToolStripMenuItem
            // 
            this.underlineToolStripMenuItem.Name = "underlineToolStripMenuItem";
            this.underlineToolStripMenuItem.Size = new System.Drawing.Size(284, 22);
            this.underlineToolStripMenuItem.Text = "Underline";
            this.underlineToolStripMenuItem.Click += new System.EventHandler(this.UnderlineToolStripMenuItemClick);
            // 
            // colorToolStripMenuItem
            // 
            this.colorToolStripMenuItem.Name = "colorToolStripMenuItem";
            this.colorToolStripMenuItem.Size = new System.Drawing.Size(284, 22);
            this.colorToolStripMenuItem.Text = "Color...";
            this.colorToolStripMenuItem.Click += new System.EventHandler(this.ColorToolStripMenuItemClick);
            // 
            // toolStripMenuItemFont
            // 
            this.toolStripMenuItemFont.Name = "toolStripMenuItemFont";
            this.toolStripMenuItemFont.Size = new System.Drawing.Size(284, 22);
            this.toolStripMenuItemFont.Text = "Font name...";
            this.toolStripMenuItemFont.Click += new System.EventHandler(this.toolStripMenuItemFont_Click);
            // 
            // toolStripMenuItemAlignment
            // 
            this.toolStripMenuItemAlignment.Name = "toolStripMenuItemAlignment";
            this.toolStripMenuItemAlignment.Size = new System.Drawing.Size(284, 22);
            this.toolStripMenuItemAlignment.Text = "Alignment";
            this.toolStripMenuItemAlignment.Click += new System.EventHandler(this.toolStripMenuItemAlignment_Click);
            // 
            // toolStripMenuItemSurroundWithMusicSymbols
            // 
            this.toolStripMenuItemSurroundWithMusicSymbols.Name = "toolStripMenuItemSurroundWithMusicSymbols";
            this.toolStripMenuItemSurroundWithMusicSymbols.Size = new System.Drawing.Size(284, 22);
            this.toolStripMenuItemSurroundWithMusicSymbols.Text = "♪";
            this.toolStripMenuItemSurroundWithMusicSymbols.Click += new System.EventHandler(this.ToolStripMenuItemSurroundWithMusicSymbolsClick);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(281, 6);
            // 
            // toolStripMenuItemAutoBreakLines
            // 
            this.toolStripMenuItemAutoBreakLines.Name = "toolStripMenuItemAutoBreakLines";
            this.toolStripMenuItemAutoBreakLines.Size = new System.Drawing.Size(284, 22);
            this.toolStripMenuItemAutoBreakLines.Text = "Auto balance selected lines...";
            this.toolStripMenuItemAutoBreakLines.Click += new System.EventHandler(this.ToolStripMenuItemAutoBreakLinesClick);
            // 
            // toolStripMenuItemUnbreakLines
            // 
            this.toolStripMenuItemUnbreakLines.Name = "toolStripMenuItemUnbreakLines";
            this.toolStripMenuItemUnbreakLines.Size = new System.Drawing.Size(284, 22);
            this.toolStripMenuItemUnbreakLines.Text = "Remove line-breaks in selected lines...";
            this.toolStripMenuItemUnbreakLines.Click += new System.EventHandler(this.ToolStripMenuItemUnbreakLinesClick);
            // 
            // toolStripSeparatorBreakLines
            // 
            this.toolStripSeparatorBreakLines.Name = "toolStripSeparatorBreakLines";
            this.toolStripSeparatorBreakLines.Size = new System.Drawing.Size(281, 6);
            // 
            // typeEffectToolStripMenuItem
            // 
            this.typeEffectToolStripMenuItem.Name = "typeEffectToolStripMenuItem";
            this.typeEffectToolStripMenuItem.Size = new System.Drawing.Size(284, 22);
            this.typeEffectToolStripMenuItem.Text = "Typewriter effect...";
            this.typeEffectToolStripMenuItem.Click += new System.EventHandler(this.TypeEffectToolStripMenuItemClick);
            // 
            // karokeeEffectToolStripMenuItem
            // 
            this.karokeeEffectToolStripMenuItem.Name = "karokeeEffectToolStripMenuItem";
            this.karokeeEffectToolStripMenuItem.Size = new System.Drawing.Size(284, 22);
            this.karokeeEffectToolStripMenuItem.Text = "Karaoke effect...";
            this.karokeeEffectToolStripMenuItem.Click += new System.EventHandler(this.KarokeeEffectToolStripMenuItemClick);
            // 
            // toolStripSeparatorAdvancedFunctions
            // 
            this.toolStripSeparatorAdvancedFunctions.Name = "toolStripSeparatorAdvancedFunctions";
            this.toolStripSeparatorAdvancedFunctions.Size = new System.Drawing.Size(281, 6);
            // 
            // showSelectedLinesEarlierlaterToolStripMenuItem
            // 
            this.showSelectedLinesEarlierlaterToolStripMenuItem.Name = "showSelectedLinesEarlierlaterToolStripMenuItem";
            this.showSelectedLinesEarlierlaterToolStripMenuItem.Size = new System.Drawing.Size(284, 22);
            this.showSelectedLinesEarlierlaterToolStripMenuItem.Text = "Show selected lines earlier/later...";
            this.showSelectedLinesEarlierlaterToolStripMenuItem.Click += new System.EventHandler(this.ShowSelectedLinesEarlierlaterToolStripMenuItemClick);
            // 
            // visualSyncSelectedLinesToolStripMenuItem
            // 
            this.visualSyncSelectedLinesToolStripMenuItem.Name = "visualSyncSelectedLinesToolStripMenuItem";
            this.visualSyncSelectedLinesToolStripMenuItem.Size = new System.Drawing.Size(284, 22);
            this.visualSyncSelectedLinesToolStripMenuItem.Text = "Visual sync selected lines...";
            this.visualSyncSelectedLinesToolStripMenuItem.Click += new System.EventHandler(this.VisualSyncSelectedLinesToolStripMenuItemClick);
            // 
            // toolStripMenuItemGoogleMicrosoftTranslateSelLine
            // 
            this.toolStripMenuItemGoogleMicrosoftTranslateSelLine.Name = "toolStripMenuItemGoogleMicrosoftTranslateSelLine";
            this.toolStripMenuItemGoogleMicrosoftTranslateSelLine.Size = new System.Drawing.Size(284, 22);
            this.toolStripMenuItemGoogleMicrosoftTranslateSelLine.Text = "Google/Microsoft translate selected line";
            this.toolStripMenuItemGoogleMicrosoftTranslateSelLine.Click += new System.EventHandler(this.ToolStripMenuItemGoogleMicrosoftTranslateSelLineClick);
            // 
            // toolStripMenuItemTranslateSelected
            // 
            this.toolStripMenuItemTranslateSelected.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.googleTranslateToolStripMenuItem,
            this.microsoftBingTranslateToolStripMenuItem});
            this.toolStripMenuItemTranslateSelected.Name = "toolStripMenuItemTranslateSelected";
            this.toolStripMenuItemTranslateSelected.Size = new System.Drawing.Size(284, 22);
            this.toolStripMenuItemTranslateSelected.Text = "Translate selected lines via...";
            // 
            // googleTranslateToolStripMenuItem
            // 
            this.googleTranslateToolStripMenuItem.Name = "googleTranslateToolStripMenuItem";
            this.googleTranslateToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
            this.googleTranslateToolStripMenuItem.Text = "Google translate...";
            this.googleTranslateToolStripMenuItem.Click += new System.EventHandler(this.googleTranslateToolStripMenuItem_Click);
            // 
            // microsoftBingTranslateToolStripMenuItem
            // 
            this.microsoftBingTranslateToolStripMenuItem.Name = "microsoftBingTranslateToolStripMenuItem";
            this.microsoftBingTranslateToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
            this.microsoftBingTranslateToolStripMenuItem.Text = "Microsoft Bing translate...";
            this.microsoftBingTranslateToolStripMenuItem.Click += new System.EventHandler(this.microsoftBingTranslateToolStripMenuItem_Click);
            // 
            // adjustDisplayTimeForSelectedLinesToolStripMenuItem
            // 
            this.adjustDisplayTimeForSelectedLinesToolStripMenuItem.Name = "adjustDisplayTimeForSelectedLinesToolStripMenuItem";
            this.adjustDisplayTimeForSelectedLinesToolStripMenuItem.Size = new System.Drawing.Size(284, 22);
            this.adjustDisplayTimeForSelectedLinesToolStripMenuItem.Text = "Adjust display time for selected lines...";
            this.adjustDisplayTimeForSelectedLinesToolStripMenuItem.Click += new System.EventHandler(this.AdjustDisplayTimeForSelectedLinesToolStripMenuItemClick);
            // 
            // fixCommonErrorsInSelectedLinesToolStripMenuItem
            // 
            this.fixCommonErrorsInSelectedLinesToolStripMenuItem.Name = "fixCommonErrorsInSelectedLinesToolStripMenuItem";
            this.fixCommonErrorsInSelectedLinesToolStripMenuItem.Size = new System.Drawing.Size(284, 22);
            this.fixCommonErrorsInSelectedLinesToolStripMenuItem.Text = "Fix common errors in selected lines...";
            this.fixCommonErrorsInSelectedLinesToolStripMenuItem.Click += new System.EventHandler(this.FixCommonErrorsInSelectedLinesToolStripMenuItemClick);
            // 
            // changeCasingForSelectedLinesToolStripMenuItem
            // 
            this.changeCasingForSelectedLinesToolStripMenuItem.Name = "changeCasingForSelectedLinesToolStripMenuItem";
            this.changeCasingForSelectedLinesToolStripMenuItem.Size = new System.Drawing.Size(284, 22);
            this.changeCasingForSelectedLinesToolStripMenuItem.Text = "Change casing for selected lines...";
            this.changeCasingForSelectedLinesToolStripMenuItem.Click += new System.EventHandler(this.ChangeCasingForSelectedLinesToolStripMenuItemClick);
            // 
            // toolStripMenuItemSaveSelectedLines
            // 
            this.toolStripMenuItemSaveSelectedLines.Name = "toolStripMenuItemSaveSelectedLines";
            this.toolStripMenuItemSaveSelectedLines.Size = new System.Drawing.Size(284, 22);
            this.toolStripMenuItemSaveSelectedLines.Text = "Save selected lines as...";
            this.toolStripMenuItemSaveSelectedLines.Click += new System.EventHandler(this.ToolStripMenuItemSaveSelectedLinesClick);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // groupBoxVideo
            // 
            this.groupBoxVideo.Controls.Add(this.labelNextWord);
            this.groupBoxVideo.Controls.Add(this.audioVisualizer);
            this.groupBoxVideo.Controls.Add(this.checkBoxSyncListViewWithVideoWhilePlaying);
            this.groupBoxVideo.Controls.Add(this.labelVideoInfo);
            this.groupBoxVideo.Controls.Add(this.trackBarWaveformPosition);
            this.groupBoxVideo.Controls.Add(this.panelWaveformControls);
            this.groupBoxVideo.Controls.Add(this.tabControlButtons);
            this.groupBoxVideo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxVideo.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBoxVideo.Location = new System.Drawing.Point(0, 0);
            this.groupBoxVideo.Margin = new System.Windows.Forms.Padding(0);
            this.groupBoxVideo.Name = "groupBoxVideo";
            this.groupBoxVideo.Padding = new System.Windows.Forms.Padding(0);
            this.groupBoxVideo.Size = new System.Drawing.Size(975, 305);
            this.groupBoxVideo.TabIndex = 1;
            this.groupBoxVideo.TabStop = false;
            // 
            // labelNextWord
            // 
            this.labelNextWord.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.labelNextWord.AutoSize = true;
            this.labelNextWord.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelNextWord.Location = new System.Drawing.Point(401, 9);
            this.labelNextWord.Name = "labelNextWord";
            this.labelNextWord.Size = new System.Drawing.Size(71, 17);
            this.labelNextWord.TabIndex = 13;
            this.labelNextWord.Text = "Next: xxx";
            this.labelNextWord.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // audioVisualizer
            // 
            this.audioVisualizer.AllowDrop = true;
            this.audioVisualizer.AllowNewSelection = true;
            this.audioVisualizer.AllowOverlap = false;
            this.audioVisualizer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.audioVisualizer.BackColor = System.Drawing.Color.Black;
            this.audioVisualizer.BackgroundColor = System.Drawing.Color.Black;
            this.audioVisualizer.ClosenessForBorderSelection = 15;
            this.audioVisualizer.Color = System.Drawing.Color.GreenYellow;
            this.audioVisualizer.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.audioVisualizer.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(18)))));
            this.audioVisualizer.InsertAtVideoPositionShortcut = System.Windows.Forms.Keys.Insert;
            this.audioVisualizer.Location = new System.Drawing.Point(472, 32);
            this.audioVisualizer.Margin = new System.Windows.Forms.Padding(0);
            this.audioVisualizer.MouseWheelScrollUpIsForward = true;
            this.audioVisualizer.Name = "audioVisualizer";
            this.audioVisualizer.NewSelectionParagraph = null;
            this.audioVisualizer.ParagraphColor = System.Drawing.Color.LimeGreen;
            this.audioVisualizer.SceneChanges = ((System.Collections.Generic.List<double>)(resources.GetObject("audioVisualizer.SceneChanges")));
            this.audioVisualizer.SelectedColor = System.Drawing.Color.Red;
            this.audioVisualizer.ShowGridLines = true;
            this.audioVisualizer.ShowSpectrogram = false;
            this.audioVisualizer.ShowWaveform = true;
            this.audioVisualizer.Size = new System.Drawing.Size(499, 229);
            this.audioVisualizer.StartPositionSeconds = 0D;
            this.audioVisualizer.TabIndex = 6;
            this.audioVisualizer.TextBold = true;
            this.audioVisualizer.TextColor = System.Drawing.Color.Gray;
            this.audioVisualizer.TextSize = 9F;
            this.audioVisualizer.VerticalZoomFactor = 1D;
            this.audioVisualizer.WaveformNotLoadedText = "Click to add waveform";
            this.audioVisualizer.WavePeaks = null;
            this.audioVisualizer.ZoomFactor = 1D;
            this.audioVisualizer.Click += new System.EventHandler(this.AudioWaveform_Click);
            this.audioVisualizer.DragDrop += new System.Windows.Forms.DragEventHandler(this.AudioWaveformDragDrop);
            this.audioVisualizer.DragEnter += new System.Windows.Forms.DragEventHandler(this.AudioWaveformDragEnter);
            this.audioVisualizer.MouseEnter += new System.EventHandler(this.audioVisualizer_MouseEnter);
            // 
            // checkBoxSyncListViewWithVideoWhilePlaying
            // 
            this.checkBoxSyncListViewWithVideoWhilePlaying.AutoSize = true;
            this.checkBoxSyncListViewWithVideoWhilePlaying.Location = new System.Drawing.Point(558, 11);
            this.checkBoxSyncListViewWithVideoWhilePlaying.Name = "checkBoxSyncListViewWithVideoWhilePlaying";
            this.checkBoxSyncListViewWithVideoWhilePlaying.Size = new System.Drawing.Size(205, 17);
            this.checkBoxSyncListViewWithVideoWhilePlaying.TabIndex = 1;
            this.checkBoxSyncListViewWithVideoWhilePlaying.Text = "Sync listview with movie when playing";
            this.checkBoxSyncListViewWithVideoWhilePlaying.UseVisualStyleBackColor = true;
            // 
            // labelVideoInfo
            // 
            this.labelVideoInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelVideoInfo.Location = new System.Drawing.Point(603, 12);
            this.labelVideoInfo.Name = "labelVideoInfo";
            this.labelVideoInfo.Size = new System.Drawing.Size(369, 19);
            this.labelVideoInfo.TabIndex = 12;
            this.labelVideoInfo.Text = "No video file loaded";
            this.labelVideoInfo.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // trackBarWaveformPosition
            // 
            this.trackBarWaveformPosition.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.trackBarWaveformPosition.AutoSize = false;
            this.trackBarWaveformPosition.Location = new System.Drawing.Point(674, 267);
            this.trackBarWaveformPosition.Maximum = 1000;
            this.trackBarWaveformPosition.Name = "trackBarWaveformPosition";
            this.trackBarWaveformPosition.Size = new System.Drawing.Size(297, 20);
            this.trackBarWaveformPosition.TabIndex = 11;
            this.trackBarWaveformPosition.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBarWaveformPosition.ValueChanged += new System.EventHandler(this.trackBarWaveformPosition_ValueChanged);
            // 
            // panelWaveformControls
            // 
            this.panelWaveformControls.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.panelWaveformControls.Controls.Add(this.toolStripWaveControls);
            this.panelWaveformControls.Location = new System.Drawing.Point(474, 265);
            this.panelWaveformControls.Name = "panelWaveformControls";
            this.panelWaveformControls.Size = new System.Drawing.Size(205, 30);
            this.panelWaveformControls.TabIndex = 10;
            // 
            // toolStripWaveControls
            // 
            this.toolStripWaveControls.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.toolStripWaveControls.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStripWaveControls.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripWaveControls.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonWaveformZoomOut,
            this.toolStripComboBoxWaveform,
            this.toolStripButtonWaveformZoomIn,
            this.toolStripSeparator16,
            this.toolStripButtonWaveformPause,
            this.toolStripButtonWaveformPlay,
            this.toolStripButtonLockCenter,
            this.toolStripSplitButtonPlayRate});
            this.toolStripWaveControls.Location = new System.Drawing.Point(0, 3);
            this.toolStripWaveControls.Name = "toolStripWaveControls";
            this.toolStripWaveControls.Size = new System.Drawing.Size(197, 25);
            this.toolStripWaveControls.TabIndex = 0;
            this.toolStripWaveControls.Text = "toolStrip2";
            // 
            // toolStripButtonWaveformZoomOut
            // 
            this.toolStripButtonWaveformZoomOut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonWaveformZoomOut.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonWaveformZoomOut.Image")));
            this.toolStripButtonWaveformZoomOut.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonWaveformZoomOut.Name = "toolStripButtonWaveformZoomOut";
            this.toolStripButtonWaveformZoomOut.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonWaveformZoomOut.Text = "toolStripButton3";
            this.toolStripButtonWaveformZoomOut.Click += new System.EventHandler(this.toolStripButtonWaveformZoomOut_Click);
            // 
            // toolStripComboBoxWaveform
            // 
            this.toolStripComboBoxWaveform.AutoSize = false;
            this.toolStripComboBoxWaveform.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBoxWaveform.Name = "toolStripComboBoxWaveform";
            this.toolStripComboBoxWaveform.Size = new System.Drawing.Size(62, 23);
            // 
            // toolStripButtonWaveformZoomIn
            // 
            this.toolStripButtonWaveformZoomIn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonWaveformZoomIn.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonWaveformZoomIn.Image")));
            this.toolStripButtonWaveformZoomIn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonWaveformZoomIn.Name = "toolStripButtonWaveformZoomIn";
            this.toolStripButtonWaveformZoomIn.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonWaveformZoomIn.Text = "toolStripButton1";
            this.toolStripButtonWaveformZoomIn.Click += new System.EventHandler(this.toolStripButtonWaveformZoomIn_Click);
            // 
            // toolStripSeparator16
            // 
            this.toolStripSeparator16.Name = "toolStripSeparator16";
            this.toolStripSeparator16.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButtonWaveformPause
            // 
            this.toolStripButtonWaveformPause.AutoToolTip = false;
            this.toolStripButtonWaveformPause.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonWaveformPause.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonWaveformPause.Image")));
            this.toolStripButtonWaveformPause.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonWaveformPause.Name = "toolStripButtonWaveformPause";
            this.toolStripButtonWaveformPause.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonWaveformPause.Text = "toolStripButton1";
            this.toolStripButtonWaveformPause.Visible = false;
            this.toolStripButtonWaveformPause.Click += new System.EventHandler(this.toolStripButtonWaveformPause_Click);
            // 
            // toolStripButtonWaveformPlay
            // 
            this.toolStripButtonWaveformPlay.AutoToolTip = false;
            this.toolStripButtonWaveformPlay.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonWaveformPlay.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonWaveformPlay.Image")));
            this.toolStripButtonWaveformPlay.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonWaveformPlay.Name = "toolStripButtonWaveformPlay";
            this.toolStripButtonWaveformPlay.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonWaveformPlay.Text = "toolStripButton1";
            this.toolStripButtonWaveformPlay.Click += new System.EventHandler(this.toolStripButtonWaveformPlay_Click);
            // 
            // toolStripButtonLockCenter
            // 
            this.toolStripButtonLockCenter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonLockCenter.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonLockCenter.Image")));
            this.toolStripButtonLockCenter.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonLockCenter.Name = "toolStripButtonLockCenter";
            this.toolStripButtonLockCenter.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonLockCenter.Text = "Center";
            this.toolStripButtonLockCenter.Click += new System.EventHandler(this.toolStripButtonLockCenter_Click);
            // 
            // toolStripSplitButtonPlayRate
            // 
            this.toolStripSplitButtonPlayRate.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripSplitButtonPlayRate.Image = ((System.Drawing.Image)(resources.GetObject("toolStripSplitButtonPlayRate.Image")));
            this.toolStripSplitButtonPlayRate.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripSplitButtonPlayRate.Name = "toolStripSplitButtonPlayRate";
            this.toolStripSplitButtonPlayRate.Size = new System.Drawing.Size(32, 22);
            this.toolStripSplitButtonPlayRate.Text = "Play rate (speed)";
            // 
            // tabControlButtons
            // 
            this.tabControlButtons.Controls.Add(this.tabPageTranslate);
            this.tabControlButtons.Controls.Add(this.tabPageCreate);
            this.tabControlButtons.Controls.Add(this.tabPageAdjust);
            this.tabControlButtons.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.tabControlButtons.Location = new System.Drawing.Point(6, 12);
            this.tabControlButtons.Name = "tabControlButtons";
            this.tabControlButtons.SelectedIndex = 0;
            this.tabControlButtons.Size = new System.Drawing.Size(467, 283);
            this.tabControlButtons.TabIndex = 0;
            this.tabControlButtons.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.TabControlButtonsDrawItem);
            this.tabControlButtons.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // tabPageTranslate
            // 
            this.tabPageTranslate.Controls.Add(this.labelTranslateTip);
            this.tabPageTranslate.Controls.Add(this.groupBoxTranslateSearch);
            this.tabPageTranslate.Controls.Add(this.groupBoxAutoContinue);
            this.tabPageTranslate.Controls.Add(this.buttonStop);
            this.tabPageTranslate.Controls.Add(this.groupBoxAutoRepeat);
            this.tabPageTranslate.Controls.Add(this.buttonPlayPrevious);
            this.tabPageTranslate.Controls.Add(this.buttonPlayCurrent);
            this.tabPageTranslate.Controls.Add(this.buttonPlayNext);
            this.tabPageTranslate.Location = new System.Drawing.Point(4, 22);
            this.tabPageTranslate.Name = "tabPageTranslate";
            this.tabPageTranslate.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageTranslate.Size = new System.Drawing.Size(459, 257);
            this.tabPageTranslate.TabIndex = 0;
            this.tabPageTranslate.Text = "Translate";
            this.tabPageTranslate.UseVisualStyleBackColor = true;
            // 
            // labelTranslateTip
            // 
            this.labelTranslateTip.AutoSize = true;
            this.labelTranslateTip.ForeColor = System.Drawing.Color.Gray;
            this.labelTranslateTip.Location = new System.Drawing.Point(16, 225);
            this.labelTranslateTip.Name = "labelTranslateTip";
            this.labelTranslateTip.Size = new System.Drawing.Size(294, 13);
            this.labelTranslateTip.TabIndex = 7;
            this.labelTranslateTip.Text = "Tip: Use <alt+arrow up/down> to go to previous/next subtitle";
            // 
            // groupBoxTranslateSearch
            // 
            this.groupBoxTranslateSearch.Controls.Add(this.buttonCustomUrl2);
            this.groupBoxTranslateSearch.Controls.Add(this.buttonCustomUrl1);
            this.groupBoxTranslateSearch.Controls.Add(this.buttonGoogleTranslateIt);
            this.groupBoxTranslateSearch.Controls.Add(this.buttonGoogleIt);
            this.groupBoxTranslateSearch.Controls.Add(this.textBoxSearchWord);
            this.groupBoxTranslateSearch.Location = new System.Drawing.Point(198, 68);
            this.groupBoxTranslateSearch.Name = "groupBoxTranslateSearch";
            this.groupBoxTranslateSearch.Size = new System.Drawing.Size(256, 150);
            this.groupBoxTranslateSearch.TabIndex = 6;
            this.groupBoxTranslateSearch.TabStop = false;
            this.groupBoxTranslateSearch.Text = "Search text online";
            // 
            // buttonCustomUrl2
            // 
            this.buttonCustomUrl2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCustomUrl2.Location = new System.Drawing.Point(6, 118);
            this.buttonCustomUrl2.Name = "buttonCustomUrl2";
            this.buttonCustomUrl2.Size = new System.Drawing.Size(244, 23);
            this.buttonCustomUrl2.TabIndex = 4;
            this.buttonCustomUrl2.Text = "Custom URL";
            this.buttonCustomUrl2.UseVisualStyleBackColor = true;
            this.buttonCustomUrl2.Click += new System.EventHandler(this.buttonCustomUrl2_Click);
            // 
            // buttonCustomUrl1
            // 
            this.buttonCustomUrl1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCustomUrl1.Location = new System.Drawing.Point(6, 91);
            this.buttonCustomUrl1.Name = "buttonCustomUrl1";
            this.buttonCustomUrl1.Size = new System.Drawing.Size(244, 23);
            this.buttonCustomUrl1.TabIndex = 3;
            this.buttonCustomUrl1.Text = "Custom URL";
            this.buttonCustomUrl1.UseVisualStyleBackColor = true;
            this.buttonCustomUrl1.Click += new System.EventHandler(this.buttonCustomUrl_Click);
            // 
            // buttonGoogleTranslateIt
            // 
            this.buttonGoogleTranslateIt.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonGoogleTranslateIt.Location = new System.Drawing.Point(132, 63);
            this.buttonGoogleTranslateIt.Name = "buttonGoogleTranslateIt";
            this.buttonGoogleTranslateIt.Size = new System.Drawing.Size(118, 23);
            this.buttonGoogleTranslateIt.TabIndex = 2;
            this.buttonGoogleTranslateIt.Text = "Google translate it";
            this.buttonGoogleTranslateIt.UseVisualStyleBackColor = true;
            this.buttonGoogleTranslateIt.Click += new System.EventHandler(this.buttonGoogleTranslateIt_Click);
            // 
            // buttonGoogleIt
            // 
            this.buttonGoogleIt.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonGoogleIt.Location = new System.Drawing.Point(6, 63);
            this.buttonGoogleIt.Name = "buttonGoogleIt";
            this.buttonGoogleIt.Size = new System.Drawing.Size(120, 23);
            this.buttonGoogleIt.TabIndex = 1;
            this.buttonGoogleIt.Text = "Google it";
            this.buttonGoogleIt.UseVisualStyleBackColor = true;
            this.buttonGoogleIt.Click += new System.EventHandler(this.buttonGoogleIt_Click);
            // 
            // textBoxSearchWord
            // 
            this.textBoxSearchWord.Location = new System.Drawing.Point(6, 18);
            this.textBoxSearchWord.Multiline = true;
            this.textBoxSearchWord.Name = "textBoxSearchWord";
            this.textBoxSearchWord.Size = new System.Drawing.Size(244, 39);
            this.textBoxSearchWord.TabIndex = 0;
            // 
            // groupBoxAutoContinue
            // 
            this.groupBoxAutoContinue.Controls.Add(this.comboBoxAutoContinue);
            this.groupBoxAutoContinue.Controls.Add(this.labelAutoContinueDelay);
            this.groupBoxAutoContinue.Controls.Add(this.checkBoxAutoContinue);
            this.groupBoxAutoContinue.Location = new System.Drawing.Point(12, 120);
            this.groupBoxAutoContinue.Name = "groupBoxAutoContinue";
            this.groupBoxAutoContinue.Size = new System.Drawing.Size(182, 98);
            this.groupBoxAutoContinue.TabIndex = 1;
            this.groupBoxAutoContinue.TabStop = false;
            this.groupBoxAutoContinue.Text = "Auto continue";
            // 
            // comboBoxAutoContinue
            // 
            this.comboBoxAutoContinue.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxAutoContinue.FormattingEnabled = true;
            this.comboBoxAutoContinue.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "11",
            "12",
            "13",
            "14",
            "15"});
            this.comboBoxAutoContinue.Location = new System.Drawing.Point(6, 59);
            this.comboBoxAutoContinue.Name = "comboBoxAutoContinue";
            this.comboBoxAutoContinue.Size = new System.Drawing.Size(96, 21);
            this.comboBoxAutoContinue.TabIndex = 2;
            // 
            // labelAutoContinueDelay
            // 
            this.labelAutoContinueDelay.AutoSize = true;
            this.labelAutoContinueDelay.Location = new System.Drawing.Point(7, 43);
            this.labelAutoContinueDelay.Name = "labelAutoContinueDelay";
            this.labelAutoContinueDelay.Size = new System.Drawing.Size(83, 13);
            this.labelAutoContinueDelay.TabIndex = 1;
            this.labelAutoContinueDelay.Text = "Delay (seconds)";
            // 
            // checkBoxAutoContinue
            // 
            this.checkBoxAutoContinue.AutoSize = true;
            this.checkBoxAutoContinue.Location = new System.Drawing.Point(10, 19);
            this.checkBoxAutoContinue.Name = "checkBoxAutoContinue";
            this.checkBoxAutoContinue.Size = new System.Drawing.Size(107, 17);
            this.checkBoxAutoContinue.TabIndex = 0;
            this.checkBoxAutoContinue.Text = "Auto continue on";
            this.checkBoxAutoContinue.UseVisualStyleBackColor = true;
            // 
            // buttonStop
            // 
            this.buttonStop.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonStop.Location = new System.Drawing.Point(288, 42);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(75, 23);
            this.buttonStop.TabIndex = 5;
            this.buttonStop.Text = "Pa&use";
            this.buttonStop.UseVisualStyleBackColor = true;
            this.buttonStop.Click += new System.EventHandler(this.buttonStop_Click);
            // 
            // groupBoxAutoRepeat
            // 
            this.groupBoxAutoRepeat.Controls.Add(this.comboBoxAutoRepeat);
            this.groupBoxAutoRepeat.Controls.Add(this.labelAutoRepeatCount);
            this.groupBoxAutoRepeat.Controls.Add(this.checkBoxAutoRepeatOn);
            this.groupBoxAutoRepeat.Location = new System.Drawing.Point(12, 14);
            this.groupBoxAutoRepeat.Name = "groupBoxAutoRepeat";
            this.groupBoxAutoRepeat.Size = new System.Drawing.Size(182, 100);
            this.groupBoxAutoRepeat.TabIndex = 0;
            this.groupBoxAutoRepeat.TabStop = false;
            this.groupBoxAutoRepeat.Text = "Auto repeat";
            // 
            // comboBoxAutoRepeat
            // 
            this.comboBoxAutoRepeat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxAutoRepeat.FormattingEnabled = true;
            this.comboBoxAutoRepeat.Items.AddRange(new object[] {
            "0",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9"});
            this.comboBoxAutoRepeat.Location = new System.Drawing.Point(6, 60);
            this.comboBoxAutoRepeat.Name = "comboBoxAutoRepeat";
            this.comboBoxAutoRepeat.Size = new System.Drawing.Size(96, 21);
            this.comboBoxAutoRepeat.TabIndex = 2;
            // 
            // labelAutoRepeatCount
            // 
            this.labelAutoRepeatCount.AutoSize = true;
            this.labelAutoRepeatCount.Location = new System.Drawing.Point(6, 44);
            this.labelAutoRepeatCount.Name = "labelAutoRepeatCount";
            this.labelAutoRepeatCount.Size = new System.Drawing.Size(105, 13);
            this.labelAutoRepeatCount.TabIndex = 1;
            this.labelAutoRepeatCount.Text = "Repeat count (times)";
            // 
            // checkBoxAutoRepeatOn
            // 
            this.checkBoxAutoRepeatOn.AutoSize = true;
            this.checkBoxAutoRepeatOn.Checked = true;
            this.checkBoxAutoRepeatOn.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAutoRepeatOn.Location = new System.Drawing.Point(10, 19);
            this.checkBoxAutoRepeatOn.Name = "checkBoxAutoRepeatOn";
            this.checkBoxAutoRepeatOn.Size = new System.Drawing.Size(96, 17);
            this.checkBoxAutoRepeatOn.TabIndex = 0;
            this.checkBoxAutoRepeatOn.Text = "Auto repeat on";
            this.checkBoxAutoRepeatOn.UseVisualStyleBackColor = true;
            // 
            // buttonPlayPrevious
            // 
            this.buttonPlayPrevious.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonPlayPrevious.Location = new System.Drawing.Point(207, 14);
            this.buttonPlayPrevious.Name = "buttonPlayPrevious";
            this.buttonPlayPrevious.Size = new System.Drawing.Size(75, 23);
            this.buttonPlayPrevious.TabIndex = 2;
            this.buttonPlayPrevious.Text = "<< Previous";
            this.buttonPlayPrevious.UseVisualStyleBackColor = true;
            this.buttonPlayPrevious.Click += new System.EventHandler(this.buttonPlayPrevious_Click);
            // 
            // buttonPlayCurrent
            // 
            this.buttonPlayCurrent.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonPlayCurrent.Location = new System.Drawing.Point(288, 14);
            this.buttonPlayCurrent.Name = "buttonPlayCurrent";
            this.buttonPlayCurrent.Size = new System.Drawing.Size(75, 23);
            this.buttonPlayCurrent.TabIndex = 3;
            this.buttonPlayCurrent.Text = "&Play current";
            this.buttonPlayCurrent.UseVisualStyleBackColor = true;
            this.buttonPlayCurrent.Click += new System.EventHandler(this.ButtonPlayCurrentClick);
            // 
            // buttonPlayNext
            // 
            this.buttonPlayNext.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonPlayNext.Location = new System.Drawing.Point(369, 14);
            this.buttonPlayNext.Name = "buttonPlayNext";
            this.buttonPlayNext.Size = new System.Drawing.Size(75, 23);
            this.buttonPlayNext.TabIndex = 4;
            this.buttonPlayNext.Text = "Next >>";
            this.buttonPlayNext.UseVisualStyleBackColor = true;
            this.buttonPlayNext.Click += new System.EventHandler(this.buttonPlayNext_Click);
            // 
            // tabPageCreate
            // 
            this.tabPageCreate.Controls.Add(this.timeUpDownVideoPosition);
            this.tabPageCreate.Controls.Add(this.buttonGotoSub);
            this.tabPageCreate.Controls.Add(this.buttonBeforeText);
            this.tabPageCreate.Controls.Add(this.buttonSetEnd);
            this.tabPageCreate.Controls.Add(this.buttonInsertNewText);
            this.tabPageCreate.Controls.Add(this.buttonSetStartTime);
            this.tabPageCreate.Controls.Add(this.labelCreateF12);
            this.tabPageCreate.Controls.Add(this.labelCreateF11);
            this.tabPageCreate.Controls.Add(this.labelCreateF10);
            this.tabPageCreate.Controls.Add(this.labelCreateF9);
            this.tabPageCreate.Controls.Add(this.buttonForward2);
            this.tabPageCreate.Controls.Add(this.numericUpDownSec2);
            this.tabPageCreate.Controls.Add(this.buttonSecBack2);
            this.tabPageCreate.Controls.Add(this.buttonForward1);
            this.tabPageCreate.Controls.Add(this.numericUpDownSec1);
            this.tabPageCreate.Controls.Add(this.labelVideoPosition);
            this.tabPageCreate.Controls.Add(this.buttonSecBack1);
            this.tabPageCreate.Location = new System.Drawing.Point(4, 22);
            this.tabPageCreate.Name = "tabPageCreate";
            this.tabPageCreate.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageCreate.Size = new System.Drawing.Size(459, 257);
            this.tabPageCreate.TabIndex = 1;
            this.tabPageCreate.Text = "Create";
            this.tabPageCreate.UseVisualStyleBackColor = true;
            // 
            // timeUpDownVideoPosition
            // 
            this.timeUpDownVideoPosition.AutoSize = true;
            this.timeUpDownVideoPosition.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.timeUpDownVideoPosition.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.timeUpDownVideoPosition.Location = new System.Drawing.Point(96, 191);
            this.timeUpDownVideoPosition.Margin = new System.Windows.Forms.Padding(4);
            this.timeUpDownVideoPosition.Name = "timeUpDownVideoPosition";
            this.timeUpDownVideoPosition.Size = new System.Drawing.Size(111, 27);
            this.timeUpDownVideoPosition.TabIndex = 12;
            timeCode1.Hours = 0;
            timeCode1.Milliseconds = 0;
            timeCode1.Minutes = 0;
            timeCode1.Seconds = 0;
            timeCode1.TimeSpan = System.TimeSpan.Parse("00:00:00");
            timeCode1.TotalMilliseconds = 0D;
            timeCode1.TotalSeconds = 0D;
            this.timeUpDownVideoPosition.TimeCode = timeCode1;
            this.timeUpDownVideoPosition.UseVideoOffset = false;
            // 
            // buttonGotoSub
            // 
            this.buttonGotoSub.Location = new System.Drawing.Point(6, 58);
            this.buttonGotoSub.Name = "buttonGotoSub";
            this.buttonGotoSub.Size = new System.Drawing.Size(180, 23);
            this.buttonGotoSub.TabIndex = 2;
            this.buttonGotoSub.Text = "Goto subposition and pause";
            this.buttonGotoSub.UseVisualStyleBackColor = true;
            this.buttonGotoSub.Click += new System.EventHandler(this.buttonGotoSub_Click);
            // 
            // buttonBeforeText
            // 
            this.buttonBeforeText.Location = new System.Drawing.Point(6, 32);
            this.buttonBeforeText.Name = "buttonBeforeText";
            this.buttonBeforeText.Size = new System.Drawing.Size(180, 23);
            this.buttonBeforeText.TabIndex = 1;
            this.buttonBeforeText.Text = "Play from just before &text";
            this.buttonBeforeText.UseVisualStyleBackColor = true;
            this.buttonBeforeText.Click += new System.EventHandler(this.buttonBeforeText_Click);
            // 
            // buttonSetEnd
            // 
            this.buttonSetEnd.Location = new System.Drawing.Point(6, 110);
            this.buttonSetEnd.Name = "buttonSetEnd";
            this.buttonSetEnd.Size = new System.Drawing.Size(180, 23);
            this.buttonSetEnd.TabIndex = 4;
            this.buttonSetEnd.Text = "Set &end time";
            this.buttonSetEnd.UseVisualStyleBackColor = true;
            this.buttonSetEnd.Click += new System.EventHandler(this.ButtonSetEndClick);
            // 
            // buttonInsertNewText
            // 
            this.buttonInsertNewText.Location = new System.Drawing.Point(6, 6);
            this.buttonInsertNewText.Name = "buttonInsertNewText";
            this.buttonInsertNewText.Size = new System.Drawing.Size(180, 23);
            this.buttonInsertNewText.TabIndex = 0;
            this.buttonInsertNewText.Text = "&Insert new subtitle at vpos";
            this.buttonInsertNewText.UseVisualStyleBackColor = true;
            this.buttonInsertNewText.Click += new System.EventHandler(this.ButtonInsertNewTextClick);
            // 
            // buttonSetStartTime
            // 
            this.buttonSetStartTime.Location = new System.Drawing.Point(6, 84);
            this.buttonSetStartTime.Name = "buttonSetStartTime";
            this.buttonSetStartTime.Size = new System.Drawing.Size(180, 23);
            this.buttonSetStartTime.TabIndex = 3;
            this.buttonSetStartTime.Text = "Set &start time";
            this.buttonSetStartTime.UseVisualStyleBackColor = true;
            this.buttonSetStartTime.Click += new System.EventHandler(this.buttonSetStartTime_Click);
            // 
            // labelCreateF12
            // 
            this.labelCreateF12.AutoSize = true;
            this.labelCreateF12.ForeColor = System.Drawing.Color.Gray;
            this.labelCreateF12.Location = new System.Drawing.Point(188, 114);
            this.labelCreateF12.Name = "labelCreateF12";
            this.labelCreateF12.Size = new System.Drawing.Size(25, 13);
            this.labelCreateF12.TabIndex = 65;
            this.labelCreateF12.Text = "F12";
            // 
            // labelCreateF11
            // 
            this.labelCreateF11.AutoSize = true;
            this.labelCreateF11.ForeColor = System.Drawing.Color.Gray;
            this.labelCreateF11.Location = new System.Drawing.Point(188, 88);
            this.labelCreateF11.Name = "labelCreateF11";
            this.labelCreateF11.Size = new System.Drawing.Size(25, 13);
            this.labelCreateF11.TabIndex = 64;
            this.labelCreateF11.Text = "F11";
            // 
            // labelCreateF10
            // 
            this.labelCreateF10.AutoSize = true;
            this.labelCreateF10.ForeColor = System.Drawing.Color.Gray;
            this.labelCreateF10.Location = new System.Drawing.Point(188, 36);
            this.labelCreateF10.Name = "labelCreateF10";
            this.labelCreateF10.Size = new System.Drawing.Size(25, 13);
            this.labelCreateF10.TabIndex = 63;
            this.labelCreateF10.Text = "F10";
            // 
            // labelCreateF9
            // 
            this.labelCreateF9.AutoSize = true;
            this.labelCreateF9.ForeColor = System.Drawing.Color.Gray;
            this.labelCreateF9.Location = new System.Drawing.Point(188, 10);
            this.labelCreateF9.Name = "labelCreateF9";
            this.labelCreateF9.Size = new System.Drawing.Size(19, 13);
            this.labelCreateF9.TabIndex = 62;
            this.labelCreateF9.Text = "F9";
            // 
            // buttonForward2
            // 
            this.buttonForward2.Location = new System.Drawing.Point(130, 163);
            this.buttonForward2.Name = "buttonForward2";
            this.buttonForward2.Size = new System.Drawing.Size(56, 23);
            this.buttonForward2.TabIndex = 10;
            this.buttonForward2.Text = " >>";
            this.buttonForward2.UseVisualStyleBackColor = true;
            this.buttonForward2.Click += new System.EventHandler(this.buttonForward2_Click);
            // 
            // numericUpDownSec2
            // 
            this.numericUpDownSec2.DecimalPlaces = 3;
            this.numericUpDownSec2.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericUpDownSec2.Location = new System.Drawing.Point(66, 164);
            this.numericUpDownSec2.Maximum = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.numericUpDownSec2.Name = "numericUpDownSec2";
            this.numericUpDownSec2.Size = new System.Drawing.Size(58, 20);
            this.numericUpDownSec2.TabIndex = 9;
            this.numericUpDownSec2.Value = new decimal(new int[] {
            5000,
            0,
            0,
            196608});
            this.numericUpDownSec2.ValueChanged += new System.EventHandler(this.NumericUpDownSec2ValueChanged);
            // 
            // buttonSecBack2
            // 
            this.buttonSecBack2.Location = new System.Drawing.Point(6, 163);
            this.buttonSecBack2.Name = "buttonSecBack2";
            this.buttonSecBack2.Size = new System.Drawing.Size(56, 23);
            this.buttonSecBack2.TabIndex = 8;
            this.buttonSecBack2.Text = "<<";
            this.buttonSecBack2.UseVisualStyleBackColor = true;
            this.buttonSecBack2.Click += new System.EventHandler(this.buttonSecBack2_Click);
            // 
            // buttonForward1
            // 
            this.buttonForward1.Location = new System.Drawing.Point(130, 137);
            this.buttonForward1.Name = "buttonForward1";
            this.buttonForward1.Size = new System.Drawing.Size(56, 23);
            this.buttonForward1.TabIndex = 7;
            this.buttonForward1.Text = ">>";
            this.buttonForward1.UseVisualStyleBackColor = true;
            this.buttonForward1.Click += new System.EventHandler(this.buttonForward1_Click);
            // 
            // numericUpDownSec1
            // 
            this.numericUpDownSec1.DecimalPlaces = 3;
            this.numericUpDownSec1.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericUpDownSec1.Location = new System.Drawing.Point(66, 138);
            this.numericUpDownSec1.Maximum = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.numericUpDownSec1.Name = "numericUpDownSec1";
            this.numericUpDownSec1.Size = new System.Drawing.Size(58, 20);
            this.numericUpDownSec1.TabIndex = 6;
            this.numericUpDownSec1.Value = new decimal(new int[] {
            500,
            0,
            0,
            196608});
            this.numericUpDownSec1.ValueChanged += new System.EventHandler(this.NumericUpDownSec1ValueChanged);
            // 
            // labelVideoPosition
            // 
            this.labelVideoPosition.AutoSize = true;
            this.labelVideoPosition.Location = new System.Drawing.Point(6, 196);
            this.labelVideoPosition.Name = "labelVideoPosition";
            this.labelVideoPosition.Size = new System.Drawing.Size(76, 13);
            this.labelVideoPosition.TabIndex = 11;
            this.labelVideoPosition.Text = "Video position:";
            this.labelVideoPosition.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // buttonSecBack1
            // 
            this.buttonSecBack1.Location = new System.Drawing.Point(6, 137);
            this.buttonSecBack1.Name = "buttonSecBack1";
            this.buttonSecBack1.Size = new System.Drawing.Size(56, 23);
            this.buttonSecBack1.TabIndex = 5;
            this.buttonSecBack1.Text = "<<";
            this.buttonSecBack1.UseVisualStyleBackColor = true;
            this.buttonSecBack1.Click += new System.EventHandler(this.buttonSecBack1_Click);
            // 
            // tabPageAdjust
            // 
            this.tabPageAdjust.Controls.Add(this.timeUpDownVideoPositionAdjust);
            this.tabPageAdjust.Controls.Add(this.buttonAdjustSetEndTime);
            this.tabPageAdjust.Controls.Add(this.buttonSetEndAndGoToNext);
            this.tabPageAdjust.Controls.Add(this.buttonSetStartAndOffsetRest);
            this.tabPageAdjust.Controls.Add(this.buttonAdjustSetStartTime);
            this.tabPageAdjust.Controls.Add(this.labelAdjustF12);
            this.tabPageAdjust.Controls.Add(this.labelAdjustF11);
            this.tabPageAdjust.Controls.Add(this.labelAdjustF10);
            this.tabPageAdjust.Controls.Add(this.labelAdjustF9);
            this.tabPageAdjust.Controls.Add(this.buttonAdjustSecForward2);
            this.tabPageAdjust.Controls.Add(this.numericUpDownSecAdjust2);
            this.tabPageAdjust.Controls.Add(this.buttonAdjustSecBack2);
            this.tabPageAdjust.Controls.Add(this.buttonAdjustSecForward1);
            this.tabPageAdjust.Controls.Add(this.numericUpDownSecAdjust1);
            this.tabPageAdjust.Controls.Add(this.buttonAdjustSecBack1);
            this.tabPageAdjust.Controls.Add(this.labelVideoPosition2);
            this.tabPageAdjust.Controls.Add(this.buttonAdjustGoToPosAndPause);
            this.tabPageAdjust.Controls.Add(this.buttonAdjustPlayBefore);
            this.tabPageAdjust.Location = new System.Drawing.Point(4, 22);
            this.tabPageAdjust.Name = "tabPageAdjust";
            this.tabPageAdjust.Size = new System.Drawing.Size(459, 257);
            this.tabPageAdjust.TabIndex = 2;
            this.tabPageAdjust.Text = "Adjust";
            this.tabPageAdjust.UseVisualStyleBackColor = true;
            // 
            // timeUpDownVideoPositionAdjust
            // 
            this.timeUpDownVideoPositionAdjust.AutoSize = true;
            this.timeUpDownVideoPositionAdjust.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.timeUpDownVideoPositionAdjust.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.timeUpDownVideoPositionAdjust.Location = new System.Drawing.Point(96, 213);
            this.timeUpDownVideoPositionAdjust.Margin = new System.Windows.Forms.Padding(4);
            this.timeUpDownVideoPositionAdjust.Name = "timeUpDownVideoPositionAdjust";
            this.timeUpDownVideoPositionAdjust.Size = new System.Drawing.Size(111, 27);
            this.timeUpDownVideoPositionAdjust.TabIndex = 13;
            timeCode2.Hours = 0;
            timeCode2.Milliseconds = 0;
            timeCode2.Minutes = 0;
            timeCode2.Seconds = 0;
            timeCode2.TimeSpan = System.TimeSpan.Parse("00:00:00");
            timeCode2.TotalMilliseconds = 0D;
            timeCode2.TotalSeconds = 0D;
            this.timeUpDownVideoPositionAdjust.TimeCode = timeCode2;
            this.timeUpDownVideoPositionAdjust.UseVideoOffset = false;
            // 
            // buttonAdjustSetEndTime
            // 
            this.buttonAdjustSetEndTime.Location = new System.Drawing.Point(6, 84);
            this.buttonAdjustSetEndTime.Name = "buttonAdjustSetEndTime";
            this.buttonAdjustSetEndTime.Size = new System.Drawing.Size(180, 23);
            this.buttonAdjustSetEndTime.TabIndex = 3;
            this.buttonAdjustSetEndTime.Text = "Set end time";
            this.buttonAdjustSetEndTime.UseVisualStyleBackColor = true;
            this.buttonAdjustSetEndTime.Click += new System.EventHandler(this.ButtonSetEndClick);
            // 
            // buttonSetEndAndGoToNext
            // 
            this.buttonSetEndAndGoToNext.Location = new System.Drawing.Point(6, 32);
            this.buttonSetEndAndGoToNext.Name = "buttonSetEndAndGoToNext";
            this.buttonSetEndAndGoToNext.Size = new System.Drawing.Size(180, 23);
            this.buttonSetEndAndGoToNext.TabIndex = 1;
            this.buttonSetEndAndGoToNext.Text = "Set e&nd && goto next";
            this.buttonSetEndAndGoToNext.UseVisualStyleBackColor = true;
            this.buttonSetEndAndGoToNext.Click += new System.EventHandler(this.ButtonSetEndAndGoToNextClick);
            // 
            // buttonSetStartAndOffsetRest
            // 
            this.buttonSetStartAndOffsetRest.Location = new System.Drawing.Point(6, 6);
            this.buttonSetStartAndOffsetRest.Name = "buttonSetStartAndOffsetRest";
            this.buttonSetStartAndOffsetRest.Size = new System.Drawing.Size(180, 23);
            this.buttonSetStartAndOffsetRest.TabIndex = 0;
            this.buttonSetStartAndOffsetRest.Text = "Set sta&rt and offset the rest";
            this.buttonSetStartAndOffsetRest.UseVisualStyleBackColor = true;
            this.buttonSetStartAndOffsetRest.Click += new System.EventHandler(this.ButtonSetStartAndOffsetRestClick);
            // 
            // buttonAdjustSetStartTime
            // 
            this.buttonAdjustSetStartTime.Location = new System.Drawing.Point(6, 58);
            this.buttonAdjustSetStartTime.Name = "buttonAdjustSetStartTime";
            this.buttonAdjustSetStartTime.Size = new System.Drawing.Size(180, 23);
            this.buttonAdjustSetStartTime.TabIndex = 2;
            this.buttonAdjustSetStartTime.Text = "Set start time";
            this.buttonAdjustSetStartTime.UseVisualStyleBackColor = true;
            this.buttonAdjustSetStartTime.Click += new System.EventHandler(this.buttonSetStartTime_Click);
            // 
            // labelAdjustF12
            // 
            this.labelAdjustF12.AutoSize = true;
            this.labelAdjustF12.ForeColor = System.Drawing.Color.Gray;
            this.labelAdjustF12.Location = new System.Drawing.Point(188, 88);
            this.labelAdjustF12.Name = "labelAdjustF12";
            this.labelAdjustF12.Size = new System.Drawing.Size(25, 13);
            this.labelAdjustF12.TabIndex = 64;
            this.labelAdjustF12.Text = "F12";
            // 
            // labelAdjustF11
            // 
            this.labelAdjustF11.AutoSize = true;
            this.labelAdjustF11.ForeColor = System.Drawing.Color.Gray;
            this.labelAdjustF11.Location = new System.Drawing.Point(188, 62);
            this.labelAdjustF11.Name = "labelAdjustF11";
            this.labelAdjustF11.Size = new System.Drawing.Size(25, 13);
            this.labelAdjustF11.TabIndex = 63;
            this.labelAdjustF11.Text = "F11";
            // 
            // labelAdjustF10
            // 
            this.labelAdjustF10.AutoSize = true;
            this.labelAdjustF10.ForeColor = System.Drawing.Color.Gray;
            this.labelAdjustF10.Location = new System.Drawing.Point(188, 36);
            this.labelAdjustF10.Name = "labelAdjustF10";
            this.labelAdjustF10.Size = new System.Drawing.Size(25, 13);
            this.labelAdjustF10.TabIndex = 62;
            this.labelAdjustF10.Text = "F10";
            // 
            // labelAdjustF9
            // 
            this.labelAdjustF9.AutoSize = true;
            this.labelAdjustF9.ForeColor = System.Drawing.Color.Gray;
            this.labelAdjustF9.Location = new System.Drawing.Point(188, 10);
            this.labelAdjustF9.Name = "labelAdjustF9";
            this.labelAdjustF9.Size = new System.Drawing.Size(19, 13);
            this.labelAdjustF9.TabIndex = 61;
            this.labelAdjustF9.Text = "F9";
            // 
            // buttonAdjustSecForward2
            // 
            this.buttonAdjustSecForward2.Location = new System.Drawing.Point(130, 188);
            this.buttonAdjustSecForward2.Name = "buttonAdjustSecForward2";
            this.buttonAdjustSecForward2.Size = new System.Drawing.Size(56, 23);
            this.buttonAdjustSecForward2.TabIndex = 11;
            this.buttonAdjustSecForward2.Text = ">>";
            this.buttonAdjustSecForward2.UseVisualStyleBackColor = true;
            this.buttonAdjustSecForward2.Click += new System.EventHandler(this.buttonAdjustSecForward2_Click);
            // 
            // numericUpDownSecAdjust2
            // 
            this.numericUpDownSecAdjust2.DecimalPlaces = 3;
            this.numericUpDownSecAdjust2.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericUpDownSecAdjust2.Location = new System.Drawing.Point(66, 189);
            this.numericUpDownSecAdjust2.Maximum = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.numericUpDownSecAdjust2.Name = "numericUpDownSecAdjust2";
            this.numericUpDownSecAdjust2.Size = new System.Drawing.Size(58, 20);
            this.numericUpDownSecAdjust2.TabIndex = 10;
            this.numericUpDownSecAdjust2.Value = new decimal(new int[] {
            5000,
            0,
            0,
            196608});
            this.numericUpDownSecAdjust2.ValueChanged += new System.EventHandler(this.NumericUpDownSecAdjust2ValueChanged);
            // 
            // buttonAdjustSecBack2
            // 
            this.buttonAdjustSecBack2.Location = new System.Drawing.Point(6, 188);
            this.buttonAdjustSecBack2.Name = "buttonAdjustSecBack2";
            this.buttonAdjustSecBack2.Size = new System.Drawing.Size(56, 23);
            this.buttonAdjustSecBack2.TabIndex = 9;
            this.buttonAdjustSecBack2.Text = "<<";
            this.buttonAdjustSecBack2.UseVisualStyleBackColor = true;
            this.buttonAdjustSecBack2.Click += new System.EventHandler(this.buttonAdjustSecBack2_Click);
            // 
            // buttonAdjustSecForward1
            // 
            this.buttonAdjustSecForward1.Location = new System.Drawing.Point(130, 162);
            this.buttonAdjustSecForward1.Name = "buttonAdjustSecForward1";
            this.buttonAdjustSecForward1.Size = new System.Drawing.Size(56, 23);
            this.buttonAdjustSecForward1.TabIndex = 8;
            this.buttonAdjustSecForward1.Text = ">>";
            this.buttonAdjustSecForward1.UseVisualStyleBackColor = true;
            this.buttonAdjustSecForward1.Click += new System.EventHandler(this.ButtonAdjustSecForwardClick);
            // 
            // numericUpDownSecAdjust1
            // 
            this.numericUpDownSecAdjust1.DecimalPlaces = 3;
            this.numericUpDownSecAdjust1.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericUpDownSecAdjust1.Location = new System.Drawing.Point(66, 163);
            this.numericUpDownSecAdjust1.Maximum = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.numericUpDownSecAdjust1.Name = "numericUpDownSecAdjust1";
            this.numericUpDownSecAdjust1.Size = new System.Drawing.Size(58, 20);
            this.numericUpDownSecAdjust1.TabIndex = 7;
            this.numericUpDownSecAdjust1.Value = new decimal(new int[] {
            500,
            0,
            0,
            196608});
            this.numericUpDownSecAdjust1.ValueChanged += new System.EventHandler(this.NumericUpDownSecAdjust1ValueChanged);
            // 
            // buttonAdjustSecBack1
            // 
            this.buttonAdjustSecBack1.Location = new System.Drawing.Point(6, 162);
            this.buttonAdjustSecBack1.Name = "buttonAdjustSecBack1";
            this.buttonAdjustSecBack1.Size = new System.Drawing.Size(56, 23);
            this.buttonAdjustSecBack1.TabIndex = 6;
            this.buttonAdjustSecBack1.Text = "<<";
            this.buttonAdjustSecBack1.UseVisualStyleBackColor = true;
            this.buttonAdjustSecBack1.Click += new System.EventHandler(this.ButtonAdjustSecBackClick);
            // 
            // labelVideoPosition2
            // 
            this.labelVideoPosition2.AutoSize = true;
            this.labelVideoPosition2.Location = new System.Drawing.Point(6, 219);
            this.labelVideoPosition2.Name = "labelVideoPosition2";
            this.labelVideoPosition2.Size = new System.Drawing.Size(76, 13);
            this.labelVideoPosition2.TabIndex = 12;
            this.labelVideoPosition2.Text = "Video position:";
            this.labelVideoPosition2.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // buttonAdjustGoToPosAndPause
            // 
            this.buttonAdjustGoToPosAndPause.Location = new System.Drawing.Point(6, 136);
            this.buttonAdjustGoToPosAndPause.Name = "buttonAdjustGoToPosAndPause";
            this.buttonAdjustGoToPosAndPause.Size = new System.Drawing.Size(180, 23);
            this.buttonAdjustGoToPosAndPause.TabIndex = 5;
            this.buttonAdjustGoToPosAndPause.Text = "&Goto subposition and pause";
            this.buttonAdjustGoToPosAndPause.UseVisualStyleBackColor = true;
            this.buttonAdjustGoToPosAndPause.Click += new System.EventHandler(this.buttonGotoSub_Click);
            // 
            // buttonAdjustPlayBefore
            // 
            this.buttonAdjustPlayBefore.Location = new System.Drawing.Point(6, 110);
            this.buttonAdjustPlayBefore.Name = "buttonAdjustPlayBefore";
            this.buttonAdjustPlayBefore.Size = new System.Drawing.Size(180, 23);
            this.buttonAdjustPlayBefore.TabIndex = 4;
            this.buttonAdjustPlayBefore.Text = "&Play from just before text";
            this.buttonAdjustPlayBefore.UseVisualStyleBackColor = true;
            this.buttonAdjustPlayBefore.Click += new System.EventHandler(this.buttonBeforeText_Click);
            // 
            // ShowSubtitleTimer
            // 
            this.ShowSubtitleTimer.Enabled = true;
            this.ShowSubtitleTimer.Interval = 17;
            this.ShowSubtitleTimer.Tick += new System.EventHandler(this.ShowSubtitleTimerTick);
            // 
            // timerAutoDuration
            // 
            this.timerAutoDuration.Interval = 300;
            this.timerAutoDuration.Tick += new System.EventHandler(this.timerAutoDuration_Tick);
            // 
            // timerAutoContinue
            // 
            this.timerAutoContinue.Interval = 1000;
            this.timerAutoContinue.Tick += new System.EventHandler(this.timerAutoContinue_Tick);
            // 
            // timerStillTyping
            // 
            this.timerStillTyping.Interval = 1500;
            this.timerStillTyping.Tick += new System.EventHandler(this.timerStillTyping_Tick);
            // 
            // timerWaveform
            // 
            this.timerWaveform.Tick += new System.EventHandler(this.timerWaveform_Tick);
            // 
            // contextMenuStripWaveform
            // 
            this.contextMenuStripWaveform.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addParagraphHereToolStripMenuItem,
            this.addParagraphAndPasteToolStripMenuItem,
            this.toolStripMenuItemSetParagraphAsSelection,
            this.toolStripMenuItemFocusTextbox,
            this.deleteParagraphToolStripMenuItem,
            this.splitToolStripMenuItem1,
            this.mergeWithPreviousToolStripMenuItem,
            this.mergeWithNextToolStripMenuItem,
            this.toolStripSeparator11,
            this.toolStripMenuItemWaveformPlaySelection,
            this.toolStripSeparator24,
            this.showWaveformAndSpectrogramToolStripMenuItem,
            this.showOnlyWaveformToolStripMenuItem,
            this.showOnlySpectrogramToolStripMenuItem,
            this.toolStripSeparatorGuessTimeCodes,
            this.removeSceneChangeToolStripMenuItem,
            this.addSceneChangeToolStripMenuItem,
            this.guessTimeCodesToolStripMenuItem,
            this.seekSilenceToolStripMenuItem,
            this.insertSubtitleHereToolStripMenuItem});
            this.contextMenuStripWaveform.Name = "contextMenuStripWaveform";
            this.contextMenuStripWaveform.Size = new System.Drawing.Size(275, 396);
            this.contextMenuStripWaveform.Closing += new System.Windows.Forms.ToolStripDropDownClosingEventHandler(this.contextMenuStripWaveform_Closing);
            this.contextMenuStripWaveform.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenuStripWaveformOpening);
            // 
            // addParagraphHereToolStripMenuItem
            // 
            this.addParagraphHereToolStripMenuItem.Name = "addParagraphHereToolStripMenuItem";
            this.addParagraphHereToolStripMenuItem.Size = new System.Drawing.Size(274, 22);
            this.addParagraphHereToolStripMenuItem.Text = "Add paragraph here";
            this.addParagraphHereToolStripMenuItem.Click += new System.EventHandler(this.addParagraphHereToolStripMenuItem_Click);
            // 
            // addParagraphAndPasteToolStripMenuItem
            // 
            this.addParagraphAndPasteToolStripMenuItem.Name = "addParagraphAndPasteToolStripMenuItem";
            this.addParagraphAndPasteToolStripMenuItem.Size = new System.Drawing.Size(274, 22);
            this.addParagraphAndPasteToolStripMenuItem.Text = "Add paragraph here + paste clipboard";
            this.addParagraphAndPasteToolStripMenuItem.Click += new System.EventHandler(this.addParagraphAndPasteToolStripMenuItem_Click);
            // 
            // toolStripMenuItemSetParagraphAsSelection
            // 
            this.toolStripMenuItemSetParagraphAsSelection.Name = "toolStripMenuItemSetParagraphAsSelection";
            this.toolStripMenuItemSetParagraphAsSelection.Size = new System.Drawing.Size(274, 22);
            this.toolStripMenuItemSetParagraphAsSelection.Text = "Set selected paragraph as selection";
            this.toolStripMenuItemSetParagraphAsSelection.Click += new System.EventHandler(this.toolStripMenuItemSetParagraphAsSelection_Click);
            // 
            // toolStripMenuItemFocusTextbox
            // 
            this.toolStripMenuItemFocusTextbox.Name = "toolStripMenuItemFocusTextbox";
            this.toolStripMenuItemFocusTextbox.Size = new System.Drawing.Size(274, 22);
            this.toolStripMenuItemFocusTextbox.Text = "Focus textbox";
            this.toolStripMenuItemFocusTextbox.Click += new System.EventHandler(this.toolStripMenuItemFocusTextbox_Click);
            // 
            // deleteParagraphToolStripMenuItem
            // 
            this.deleteParagraphToolStripMenuItem.Name = "deleteParagraphToolStripMenuItem";
            this.deleteParagraphToolStripMenuItem.Size = new System.Drawing.Size(274, 22);
            this.deleteParagraphToolStripMenuItem.Text = "Delete paragraph";
            this.deleteParagraphToolStripMenuItem.Click += new System.EventHandler(this.deleteParagraphToolStripMenuItem_Click);
            // 
            // splitToolStripMenuItem1
            // 
            this.splitToolStripMenuItem1.Name = "splitToolStripMenuItem1";
            this.splitToolStripMenuItem1.Size = new System.Drawing.Size(274, 22);
            this.splitToolStripMenuItem1.Text = "Split";
            this.splitToolStripMenuItem1.Click += new System.EventHandler(this.splitToolStripMenuItem1_Click);
            // 
            // mergeWithPreviousToolStripMenuItem
            // 
            this.mergeWithPreviousToolStripMenuItem.Name = "mergeWithPreviousToolStripMenuItem";
            this.mergeWithPreviousToolStripMenuItem.Size = new System.Drawing.Size(274, 22);
            this.mergeWithPreviousToolStripMenuItem.Text = "Merge with previous";
            this.mergeWithPreviousToolStripMenuItem.Click += new System.EventHandler(this.mergeWithPreviousToolStripMenuItem_Click);
            // 
            // mergeWithNextToolStripMenuItem
            // 
            this.mergeWithNextToolStripMenuItem.Name = "mergeWithNextToolStripMenuItem";
            this.mergeWithNextToolStripMenuItem.Size = new System.Drawing.Size(274, 22);
            this.mergeWithNextToolStripMenuItem.Text = "Merge with next";
            this.mergeWithNextToolStripMenuItem.Click += new System.EventHandler(this.mergeWithNextToolStripMenuItem_Click);
            // 
            // toolStripSeparator11
            // 
            this.toolStripSeparator11.Name = "toolStripSeparator11";
            this.toolStripSeparator11.Size = new System.Drawing.Size(271, 6);
            // 
            // toolStripMenuItemWaveformPlaySelection
            // 
            this.toolStripMenuItemWaveformPlaySelection.Name = "toolStripMenuItemWaveformPlaySelection";
            this.toolStripMenuItemWaveformPlaySelection.Size = new System.Drawing.Size(274, 22);
            this.toolStripMenuItemWaveformPlaySelection.Text = "Play selection";
            this.toolStripMenuItemWaveformPlaySelection.Click += new System.EventHandler(this.toolStripMenuItemWaveformPlaySelection_Click);
            // 
            // toolStripSeparator24
            // 
            this.toolStripSeparator24.Name = "toolStripSeparator24";
            this.toolStripSeparator24.Size = new System.Drawing.Size(271, 6);
            // 
            // showWaveformAndSpectrogramToolStripMenuItem
            // 
            this.showWaveformAndSpectrogramToolStripMenuItem.Name = "showWaveformAndSpectrogramToolStripMenuItem";
            this.showWaveformAndSpectrogramToolStripMenuItem.Size = new System.Drawing.Size(274, 22);
            this.showWaveformAndSpectrogramToolStripMenuItem.Text = "Show waveform and spectrogram";
            this.showWaveformAndSpectrogramToolStripMenuItem.Click += new System.EventHandler(this.ShowWaveformAndSpectrogramToolStripMenuItemClick);
            // 
            // showOnlyWaveformToolStripMenuItem
            // 
            this.showOnlyWaveformToolStripMenuItem.Name = "showOnlyWaveformToolStripMenuItem";
            this.showOnlyWaveformToolStripMenuItem.Size = new System.Drawing.Size(274, 22);
            this.showOnlyWaveformToolStripMenuItem.Text = "Show only waveform";
            this.showOnlyWaveformToolStripMenuItem.Click += new System.EventHandler(this.ShowOnlyWaveformToolStripMenuItemClick);
            // 
            // showOnlySpectrogramToolStripMenuItem
            // 
            this.showOnlySpectrogramToolStripMenuItem.Name = "showOnlySpectrogramToolStripMenuItem";
            this.showOnlySpectrogramToolStripMenuItem.Size = new System.Drawing.Size(274, 22);
            this.showOnlySpectrogramToolStripMenuItem.Text = "Show only spectrogram";
            this.showOnlySpectrogramToolStripMenuItem.Click += new System.EventHandler(this.ShowOnlySpectrogramToolStripMenuItemClick);
            // 
            // toolStripSeparatorGuessTimeCodes
            // 
            this.toolStripSeparatorGuessTimeCodes.Name = "toolStripSeparatorGuessTimeCodes";
            this.toolStripSeparatorGuessTimeCodes.Size = new System.Drawing.Size(271, 6);
            // 
            // removeSceneChangeToolStripMenuItem
            // 
            this.removeSceneChangeToolStripMenuItem.Name = "removeSceneChangeToolStripMenuItem";
            this.removeSceneChangeToolStripMenuItem.Size = new System.Drawing.Size(274, 22);
            this.removeSceneChangeToolStripMenuItem.Text = "Remove scene change";
            this.removeSceneChangeToolStripMenuItem.Click += new System.EventHandler(this.removeSceneChangeToolStripMenuItem_Click);
            // 
            // addSceneChangeToolStripMenuItem
            // 
            this.addSceneChangeToolStripMenuItem.Name = "addSceneChangeToolStripMenuItem";
            this.addSceneChangeToolStripMenuItem.Size = new System.Drawing.Size(274, 22);
            this.addSceneChangeToolStripMenuItem.Text = "Add scene change";
            this.addSceneChangeToolStripMenuItem.Click += new System.EventHandler(this.addSceneChangeToolStripMenuItem_Click);
            // 
            // guessTimeCodesToolStripMenuItem
            // 
            this.guessTimeCodesToolStripMenuItem.Name = "guessTimeCodesToolStripMenuItem";
            this.guessTimeCodesToolStripMenuItem.Size = new System.Drawing.Size(274, 22);
            this.guessTimeCodesToolStripMenuItem.Text = "Guess time codes...";
            this.guessTimeCodesToolStripMenuItem.Click += new System.EventHandler(this.GuessTimeCodesToolStripMenuItemClick);
            // 
            // seekSilenceToolStripMenuItem
            // 
            this.seekSilenceToolStripMenuItem.Name = "seekSilenceToolStripMenuItem";
            this.seekSilenceToolStripMenuItem.Size = new System.Drawing.Size(274, 22);
            this.seekSilenceToolStripMenuItem.Text = "Seek silence...";
            this.seekSilenceToolStripMenuItem.Click += new System.EventHandler(this.seekSilenceToolStripMenuItem_Click);
            // 
            // insertSubtitleHereToolStripMenuItem
            // 
            this.insertSubtitleHereToolStripMenuItem.Name = "insertSubtitleHereToolStripMenuItem";
            this.insertSubtitleHereToolStripMenuItem.Size = new System.Drawing.Size(274, 22);
            this.insertSubtitleHereToolStripMenuItem.Text = "Insert subtitle here...";
            this.insertSubtitleHereToolStripMenuItem.Click += new System.EventHandler(this.insertSubtitleHereToolStripMenuItem_Click);
            // 
            // splitContainerMain
            // 
            this.splitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainerMain.Location = new System.Drawing.Point(0, 64);
            this.splitContainerMain.Name = "splitContainerMain";
            this.splitContainerMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerMain.Panel1
            // 
            this.splitContainerMain.Panel1.Controls.Add(this.splitContainer1);
            // 
            // splitContainerMain.Panel2
            // 
            this.splitContainerMain.Panel2.Controls.Add(this.groupBoxVideo);
            this.splitContainerMain.Size = new System.Drawing.Size(975, 560);
            this.splitContainerMain.SplitterDistance = 251;
            this.splitContainerMain.TabIndex = 8;
            this.splitContainerMain.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.SplitContainerMainSplitterMoved);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tabControlSubtitle);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.panelVideoPlayer);
            this.splitContainer1.Size = new System.Drawing.Size(975, 251);
            this.splitContainer1.SplitterDistance = 743;
            this.splitContainer1.TabIndex = 7;
            this.splitContainer1.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.SplitContainer1SplitterMoved);
            // 
            // tabControlSubtitle
            // 
            this.tabControlSubtitle.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControlSubtitle.Controls.Add(this.tabPage1);
            this.tabControlSubtitle.Controls.Add(this.tabPage2);
            this.tabControlSubtitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabControlSubtitle.Location = new System.Drawing.Point(3, 3);
            this.tabControlSubtitle.Name = "tabControlSubtitle";
            this.tabControlSubtitle.SelectedIndex = 0;
            this.tabControlSubtitle.Size = new System.Drawing.Size(738, 248);
            this.tabControlSubtitle.TabIndex = 0;
            this.tabControlSubtitle.SelectedIndexChanged += new System.EventHandler(this.TabControlSubtitleSelectedIndexChanged);
            this.tabControlSubtitle.Selecting += new System.Windows.Forms.TabControlCancelEventHandler(this.TabControlSubtitleSelecting);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.splitContainerListViewAndText);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(730, 222);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "List view";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // splitContainerListViewAndText
            // 
            this.splitContainerListViewAndText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerListViewAndText.Location = new System.Drawing.Point(3, 3);
            this.splitContainerListViewAndText.Name = "splitContainerListViewAndText";
            this.splitContainerListViewAndText.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerListViewAndText.Panel1
            // 
            this.splitContainerListViewAndText.Panel1.Controls.Add(this.SubtitleListview1);
            this.splitContainerListViewAndText.Panel1MinSize = 50;
            // 
            // splitContainerListViewAndText.Panel2
            // 
            this.splitContainerListViewAndText.Panel2.Controls.Add(this.groupBoxEdit);
            this.splitContainerListViewAndText.Panel2MinSize = 105;
            this.splitContainerListViewAndText.Size = new System.Drawing.Size(724, 216);
            this.splitContainerListViewAndText.SplitterDistance = 91;
            this.splitContainerListViewAndText.TabIndex = 2;
            // 
            // SubtitleListview1
            // 
            this.SubtitleListview1.AllowColumnReorder = true;
            this.SubtitleListview1.AllowDrop = true;
            this.SubtitleListview1.ContextMenuStrip = this.contextMenuStripListview;
            this.SubtitleListview1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SubtitleListview1.FirstVisibleIndex = -1;
            this.SubtitleListview1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SubtitleListview1.FullRowSelect = true;
            this.SubtitleListview1.GridLines = true;
            this.SubtitleListview1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.SubtitleListview1.HideSelection = false;
            this.SubtitleListview1.Location = new System.Drawing.Point(0, 0);
            this.SubtitleListview1.Name = "SubtitleListview1";
            this.SubtitleListview1.OwnerDraw = true;
            this.SubtitleListview1.Size = new System.Drawing.Size(724, 91);
            this.SubtitleListview1.StateImageList = this.imageListBookmarks;
            this.SubtitleListview1.SubtitleFontBold = false;
            this.SubtitleListview1.SubtitleFontName = "Tahoma";
            this.SubtitleListview1.SubtitleFontSize = 8;
            this.SubtitleListview1.TabIndex = 0;
            this.SubtitleListview1.UseCompatibleStateImageBehavior = false;
            this.SubtitleListview1.UseSyntaxColoring = true;
            this.SubtitleListview1.View = System.Windows.Forms.View.Details;
            this.SubtitleListview1.SelectedIndexChanged += new System.EventHandler(this.SubtitleListview1_SelectedIndexChanged);
            this.SubtitleListview1.DragDrop += new System.Windows.Forms.DragEventHandler(this.SubtitleListview1_DragDrop);
            this.SubtitleListview1.DragEnter += new System.Windows.Forms.DragEventHandler(this.SubtitleListview1_DragEnter);
            this.SubtitleListview1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SubtitleListview1KeyDown);
            this.SubtitleListview1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.SubtitleListview1_MouseDoubleClick);
            this.SubtitleListview1.MouseEnter += new System.EventHandler(this.SubtitleListview1_MouseEnter);
            // 
            // imageListBookmarks
            // 
            this.imageListBookmarks.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageListBookmarks.ImageSize = new System.Drawing.Size(16, 16);
            this.imageListBookmarks.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // groupBoxEdit
            // 
            this.groupBoxEdit.Controls.Add(this.panelBookmark);
            this.groupBoxEdit.Controls.Add(this.pictureBoxBookmark);
            this.groupBoxEdit.Controls.Add(this.labelSingleLine);
            this.groupBoxEdit.Controls.Add(this.labelAlternateSingleLine);
            this.groupBoxEdit.Controls.Add(this.labelDurationWarning);
            this.groupBoxEdit.Controls.Add(this.labelStartTimeWarning);
            this.groupBoxEdit.Controls.Add(this.buttonSplitLine);
            this.groupBoxEdit.Controls.Add(this.labelAlternateCharactersPerSecond);
            this.groupBoxEdit.Controls.Add(this.labelTextAlternateLineTotal);
            this.groupBoxEdit.Controls.Add(this.labelTextAlternateLineLengths);
            this.groupBoxEdit.Controls.Add(this.labelAlternateText);
            this.groupBoxEdit.Controls.Add(this.labelText);
            this.groupBoxEdit.Controls.Add(this.textBoxListViewTextAlternate);
            this.groupBoxEdit.Controls.Add(this.buttonAutoBreak);
            this.groupBoxEdit.Controls.Add(this.labelTextLineLengths);
            this.groupBoxEdit.Controls.Add(this.labelTextLineTotal);
            this.groupBoxEdit.Controls.Add(this.labelCharactersPerSecond);
            this.groupBoxEdit.Controls.Add(this.buttonUnBreak);
            this.groupBoxEdit.Controls.Add(this.timeUpDownStartTime);
            this.groupBoxEdit.Controls.Add(this.numericUpDownDuration);
            this.groupBoxEdit.Controls.Add(this.buttonPrevious);
            this.groupBoxEdit.Controls.Add(this.buttonNext);
            this.groupBoxEdit.Controls.Add(this.labelStartTime);
            this.groupBoxEdit.Controls.Add(this.textBoxListViewText);
            this.groupBoxEdit.Controls.Add(this.labelDuration);
            this.groupBoxEdit.Controls.Add(this.labelAutoDuration);
            this.groupBoxEdit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxEdit.Location = new System.Drawing.Point(0, 0);
            this.groupBoxEdit.Name = "groupBoxEdit";
            this.groupBoxEdit.Size = new System.Drawing.Size(724, 121);
            this.groupBoxEdit.TabIndex = 1;
            this.groupBoxEdit.TabStop = false;
            // 
            // panelBookmark
            // 
            this.panelBookmark.BackColor = System.Drawing.Color.LemonChiffon;
            this.panelBookmark.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelBookmark.Controls.Add(this.labelBookmark);
            this.panelBookmark.Location = new System.Drawing.Point(207, 38);
            this.panelBookmark.Name = "panelBookmark";
            this.panelBookmark.Size = new System.Drawing.Size(200, 25);
            this.panelBookmark.TabIndex = 40;
            this.panelBookmark.Visible = false;
            // 
            // labelBookmark
            // 
            this.labelBookmark.AutoSize = true;
            this.labelBookmark.Location = new System.Drawing.Point(4, 4);
            this.labelBookmark.Name = "labelBookmark";
            this.labelBookmark.Size = new System.Drawing.Size(77, 13);
            this.labelBookmark.TabIndex = 0;
            this.labelBookmark.Text = "labelBookmark";
            this.labelBookmark.DoubleClick += new System.EventHandler(this.labelBookmark_DoubleClick);
            // 
            // pictureBoxBookmark
            // 
            this.pictureBoxBookmark.Image = global::Nikse.SubtitleEdit.Properties.Resources.bookmark22;
            this.pictureBoxBookmark.Location = new System.Drawing.Point(211, 28);
            this.pictureBoxBookmark.Name = "pictureBoxBookmark";
            this.pictureBoxBookmark.Size = new System.Drawing.Size(22, 22);
            this.pictureBoxBookmark.TabIndex = 41;
            this.pictureBoxBookmark.TabStop = false;
            this.pictureBoxBookmark.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pictureBoxBookmark_MouseClick);
            this.pictureBoxBookmark.MouseEnter += new System.EventHandler(this.pictureBoxBookmark_MouseEnter);
            // 
            // labelSingleLine
            // 
            this.labelSingleLine.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelSingleLine.AutoSize = true;
            this.labelSingleLine.Location = new System.Drawing.Point(346, 94);
            this.labelSingleLine.Name = "labelSingleLine";
            this.labelSingleLine.Size = new System.Drawing.Size(78, 13);
            this.labelSingleLine.TabIndex = 32;
            this.labelSingleLine.Text = "labelSingleLine";
            // 
            // labelAlternateSingleLine
            // 
            this.labelAlternateSingleLine.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelAlternateSingleLine.AutoSize = true;
            this.labelAlternateSingleLine.Location = new System.Drawing.Point(839, 94);
            this.labelAlternateSingleLine.Name = "labelAlternateSingleLine";
            this.labelAlternateSingleLine.Size = new System.Drawing.Size(48, 13);
            this.labelAlternateSingleLine.TabIndex = 36;
            this.labelAlternateSingleLine.Text = "AltSinLin";
            // 
            // labelDurationWarning
            // 
            this.labelDurationWarning.AutoSize = true;
            this.labelDurationWarning.ForeColor = System.Drawing.Color.Red;
            this.labelDurationWarning.Location = new System.Drawing.Point(122, 64);
            this.labelDurationWarning.Name = "labelDurationWarning";
            this.labelDurationWarning.Size = new System.Drawing.Size(109, 13);
            this.labelDurationWarning.TabIndex = 17;
            this.labelDurationWarning.Text = "labelDurationWarning";
            // 
            // labelStartTimeWarning
            // 
            this.labelStartTimeWarning.AutoSize = true;
            this.labelStartTimeWarning.ForeColor = System.Drawing.Color.Red;
            this.labelStartTimeWarning.Location = new System.Drawing.Point(8, 50);
            this.labelStartTimeWarning.Name = "labelStartTimeWarning";
            this.labelStartTimeWarning.Size = new System.Drawing.Size(114, 13);
            this.labelStartTimeWarning.TabIndex = 18;
            this.labelStartTimeWarning.Text = "labelStartTimeWarning";
            // 
            // buttonSplitLine
            // 
            this.buttonSplitLine.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSplitLine.ForeColor = System.Drawing.Color.Red;
            this.buttonSplitLine.Location = new System.Drawing.Point(604, 88);
            this.buttonSplitLine.Name = "buttonSplitLine";
            this.buttonSplitLine.Size = new System.Drawing.Size(115, 23);
            this.buttonSplitLine.TabIndex = 39;
            this.buttonSplitLine.Text = "Split line";
            this.buttonSplitLine.UseVisualStyleBackColor = true;
            this.buttonSplitLine.Visible = false;
            this.buttonSplitLine.Click += new System.EventHandler(this.ButtonSplitLineClick);
            // 
            // labelAlternateCharactersPerSecond
            // 
            this.labelAlternateCharactersPerSecond.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelAlternateCharactersPerSecond.AutoSize = true;
            this.labelAlternateCharactersPerSecond.Location = new System.Drawing.Point(636, 11);
            this.labelAlternateCharactersPerSecond.Name = "labelAlternateCharactersPerSecond";
            this.labelAlternateCharactersPerSecond.Size = new System.Drawing.Size(64, 13);
            this.labelAlternateCharactersPerSecond.TabIndex = 38;
            this.labelAlternateCharactersPerSecond.Text = "altCharsSec";
            // 
            // labelTextAlternateLineTotal
            // 
            this.labelTextAlternateLineTotal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.labelTextAlternateLineTotal.AutoSize = true;
            this.labelTextAlternateLineTotal.Location = new System.Drawing.Point(682, 94);
            this.labelTextAlternateLineTotal.Name = "labelTextAlternateLineTotal";
            this.labelTextAlternateLineTotal.Size = new System.Drawing.Size(35, 13);
            this.labelTextAlternateLineTotal.TabIndex = 37;
            this.labelTextAlternateLineTotal.Text = "AltTot";
            // 
            // labelTextAlternateLineLengths
            // 
            this.labelTextAlternateLineLengths.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelTextAlternateLineLengths.AutoSize = true;
            this.labelTextAlternateLineLengths.Location = new System.Drawing.Point(786, 94);
            this.labelTextAlternateLineLengths.Name = "labelTextAlternateLineLengths";
            this.labelTextAlternateLineLengths.Size = new System.Drawing.Size(57, 13);
            this.labelTextAlternateLineLengths.TabIndex = 35;
            this.labelTextAlternateLineLengths.Text = "AltLineLen";
            // 
            // labelAlternateText
            // 
            this.labelAlternateText.AutoSize = true;
            this.labelAlternateText.Location = new System.Drawing.Point(803, 11);
            this.labelAlternateText.Name = "labelAlternateText";
            this.labelAlternateText.Size = new System.Drawing.Size(28, 13);
            this.labelAlternateText.TabIndex = 34;
            this.labelAlternateText.Text = "Text";
            this.labelAlternateText.Visible = false;
            // 
            // labelText
            // 
            this.labelText.AutoSize = true;
            this.labelText.Location = new System.Drawing.Point(239, 11);
            this.labelText.Name = "labelText";
            this.labelText.Size = new System.Drawing.Size(28, 13);
            this.labelText.TabIndex = 5;
            this.labelText.Text = "Text";
            // 
            // textBoxListViewTextAlternate
            // 
            this.textBoxListViewTextAlternate.AllowDrop = true;
            this.textBoxListViewTextAlternate.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxListViewTextAlternate.ContextMenuStrip = this.contextMenuStripTextBoxListView;
            this.textBoxListViewTextAlternate.Enabled = false;
            this.textBoxListViewTextAlternate.HideSelection = false;
            this.textBoxListViewTextAlternate.Location = new System.Drawing.Point(946, 28);
            this.textBoxListViewTextAlternate.Multiline = true;
            this.textBoxListViewTextAlternate.Name = "textBoxListViewTextAlternate";
            this.textBoxListViewTextAlternate.Size = new System.Drawing.Size(0, 63);
            this.textBoxListViewTextAlternate.TabIndex = 33;
            this.textBoxListViewTextAlternate.Visible = false;
            this.textBoxListViewTextAlternate.MouseClick += new System.Windows.Forms.MouseEventHandler(this.TextBoxListViewTextAlternateMouseClick);
            this.textBoxListViewTextAlternate.TextChanged += new System.EventHandler(this.textBoxListViewTextAlternate_TextChanged);
            this.textBoxListViewTextAlternate.Enter += new System.EventHandler(this.TextBoxListViewTextAlternateEnter);
            this.textBoxListViewTextAlternate.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBoxListViewTextAlternateKeyDown);
            this.textBoxListViewTextAlternate.KeyUp += new System.Windows.Forms.KeyEventHandler(this.TextBoxListViewTextAlternateKeyUp);
            this.textBoxListViewTextAlternate.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TextBoxListViewTextAlternateMouseMove);
            // 
            // contextMenuStripTextBoxListView
            // 
            this.contextMenuStripTextBoxListView.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemWebVttVoice,
            this.toolStripSeparatorWebVTT,
            this.cutToolStripMenuItem,
            this.copyToolStripMenuItem,
            this.pasteToolStripMenuItem,
            this.deleteToolStripMenuItem,
            this.toolStripMenuItemSplitTextAtCursor,
            this.toolStripMenuItemSplitViaWaveform,
            this.toolStripSeparator18,
            this.selectAllToolStripMenuItem,
            this.toolStripSeparator17,
            this.normalToolStripMenuItem1,
            this.boldToolStripMenuItem1,
            this.italicToolStripMenuItem1,
            this.underlineToolStripMenuItem1,
            this.colorToolStripMenuItem1,
            this.toolStripMenuItemHorizontalDigits,
            this.toolStripMenuItemBouten,
            this.toolStripMenuItemRuby,
            this.fontNameToolStripMenuItem,
            this.toolStripSeparator26,
            this.toolStripMenuItemInsertUnicodeSymbol,
            this.toolStripMenuItemInsertUnicodeControlCharacters,
            this.superscriptToolStripMenuItem,
            this.subscriptToolStripMenuItem});
            this.contextMenuStripTextBoxListView.Name = "contextMenuStripTextBoxListView";
            this.contextMenuStripTextBoxListView.Size = new System.Drawing.Size(274, 490);
            this.contextMenuStripTextBoxListView.Closed += new System.Windows.Forms.ToolStripDropDownClosedEventHandler(this.MenuClosed);
            this.contextMenuStripTextBoxListView.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenuStripTextBoxListViewOpening);
            this.contextMenuStripTextBoxListView.Opened += new System.EventHandler(this.MenuOpened);
            // 
            // toolStripMenuItemWebVttVoice
            // 
            this.toolStripMenuItemWebVttVoice.Name = "toolStripMenuItemWebVttVoice";
            this.toolStripMenuItemWebVttVoice.Size = new System.Drawing.Size(273, 22);
            this.toolStripMenuItemWebVttVoice.Text = "WebVTT voice";
            // 
            // toolStripSeparatorWebVTT
            // 
            this.toolStripSeparatorWebVTT.Name = "toolStripSeparatorWebVTT";
            this.toolStripSeparatorWebVTT.Size = new System.Drawing.Size(270, 6);
            // 
            // cutToolStripMenuItem
            // 
            this.cutToolStripMenuItem.Name = "cutToolStripMenuItem";
            this.cutToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
            this.cutToolStripMenuItem.Size = new System.Drawing.Size(273, 22);
            this.cutToolStripMenuItem.Text = "Cut";
            this.cutToolStripMenuItem.Click += new System.EventHandler(this.cutToolStripMenuItem_Click);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(273, 22);
            this.copyToolStripMenuItem.Text = "Copy";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
            // 
            // pasteToolStripMenuItem
            // 
            this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            this.pasteToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
            this.pasteToolStripMenuItem.Size = new System.Drawing.Size(273, 22);
            this.pasteToolStripMenuItem.Text = "Paste";
            this.pasteToolStripMenuItem.Click += new System.EventHandler(this.PasteToolStripMenuItemClick);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(273, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.DeleteToolStripMenuItemClick);
            // 
            // toolStripMenuItemSplitTextAtCursor
            // 
            this.toolStripMenuItemSplitTextAtCursor.Name = "toolStripMenuItemSplitTextAtCursor";
            this.toolStripMenuItemSplitTextAtCursor.Size = new System.Drawing.Size(273, 22);
            this.toolStripMenuItemSplitTextAtCursor.Text = "Split text at cursor position";
            this.toolStripMenuItemSplitTextAtCursor.Click += new System.EventHandler(this.ToolStripMenuItemSplitTextAtCursorClick);
            // 
            // toolStripMenuItemSplitViaWaveform
            // 
            this.toolStripMenuItemSplitViaWaveform.Name = "toolStripMenuItemSplitViaWaveform";
            this.toolStripMenuItemSplitViaWaveform.Size = new System.Drawing.Size(273, 22);
            this.toolStripMenuItemSplitViaWaveform.Text = "Split text at cursor/waveform position";
            this.toolStripMenuItemSplitViaWaveform.Click += new System.EventHandler(this.toolStripMenuItemSplitViaWaveform_Click);
            // 
            // toolStripSeparator18
            // 
            this.toolStripSeparator18.Name = "toolStripSeparator18";
            this.toolStripSeparator18.Size = new System.Drawing.Size(270, 6);
            // 
            // selectAllToolStripMenuItem
            // 
            this.selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
            this.selectAllToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A)));
            this.selectAllToolStripMenuItem.Size = new System.Drawing.Size(273, 22);
            this.selectAllToolStripMenuItem.Text = "Select all";
            this.selectAllToolStripMenuItem.Click += new System.EventHandler(this.selectAllToolStripMenuItem_Click);
            // 
            // toolStripSeparator17
            // 
            this.toolStripSeparator17.Name = "toolStripSeparator17";
            this.toolStripSeparator17.Size = new System.Drawing.Size(270, 6);
            // 
            // normalToolStripMenuItem1
            // 
            this.normalToolStripMenuItem1.Name = "normalToolStripMenuItem1";
            this.normalToolStripMenuItem1.Size = new System.Drawing.Size(273, 22);
            this.normalToolStripMenuItem1.Text = "Normal";
            this.normalToolStripMenuItem1.Click += new System.EventHandler(this.NormalToolStripMenuItem1Click);
            // 
            // boldToolStripMenuItem1
            // 
            this.boldToolStripMenuItem1.Name = "boldToolStripMenuItem1";
            this.boldToolStripMenuItem1.Size = new System.Drawing.Size(273, 22);
            this.boldToolStripMenuItem1.Text = "Bold";
            this.boldToolStripMenuItem1.Click += new System.EventHandler(this.BoldToolStripMenuItem1Click);
            // 
            // italicToolStripMenuItem1
            // 
            this.italicToolStripMenuItem1.Name = "italicToolStripMenuItem1";
            this.italicToolStripMenuItem1.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.I)));
            this.italicToolStripMenuItem1.Size = new System.Drawing.Size(273, 22);
            this.italicToolStripMenuItem1.Text = "Italic";
            this.italicToolStripMenuItem1.Click += new System.EventHandler(this.ItalicToolStripMenuItem1Click);
            // 
            // underlineToolStripMenuItem1
            // 
            this.underlineToolStripMenuItem1.Name = "underlineToolStripMenuItem1";
            this.underlineToolStripMenuItem1.Size = new System.Drawing.Size(273, 22);
            this.underlineToolStripMenuItem1.Text = "Underline";
            this.underlineToolStripMenuItem1.Click += new System.EventHandler(this.UnderlineToolStripMenuItem1Click);
            // 
            // colorToolStripMenuItem1
            // 
            this.colorToolStripMenuItem1.Name = "colorToolStripMenuItem1";
            this.colorToolStripMenuItem1.Size = new System.Drawing.Size(273, 22);
            this.colorToolStripMenuItem1.Text = "Color...";
            this.colorToolStripMenuItem1.Click += new System.EventHandler(this.ColorToolStripMenuItem1Click);
            // 
            // toolStripMenuItemHorizontalDigits
            // 
            this.toolStripMenuItemHorizontalDigits.Name = "toolStripMenuItemHorizontalDigits";
            this.toolStripMenuItemHorizontalDigits.Size = new System.Drawing.Size(273, 22);
            this.toolStripMenuItemHorizontalDigits.Text = "Horizontal digits";
            this.toolStripMenuItemHorizontalDigits.Click += new System.EventHandler(this.toolStripMenuItemHorizontalDigits_Click);
            // 
            // toolStripMenuItemBouten
            // 
            this.toolStripMenuItemBouten.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.boutendotbeforeToolStripMenuItem,
            this.boutendotafterToolStripMenuItem,
            this.boutendotoutsideToolStripMenuItem,
            this.boutenfilledcircleoutsideToolStripMenuItem,
            this.boutenopencircleoutsideToolStripMenuItem,
            this.boutenopendotoutsideToolStripMenuItem,
            this.boutenfilledsesameoutsideToolStripMenuItem,
            this.boutenopensesameoutsideToolStripMenuItem,
            this.boutenautooutsideToolStripMenuItem,
            this.boutenautoToolStripMenuItem});
            this.toolStripMenuItemBouten.Name = "toolStripMenuItemBouten";
            this.toolStripMenuItemBouten.Size = new System.Drawing.Size(273, 22);
            this.toolStripMenuItemBouten.Text = "Bouten";
            // 
            // boutendotbeforeToolStripMenuItem
            // 
            this.boutendotbeforeToolStripMenuItem.Name = "boutendotbeforeToolStripMenuItem";
            this.boutendotbeforeToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.boutendotbeforeToolStripMenuItem.Text = "bouten-dot-before";
            this.boutendotbeforeToolStripMenuItem.Click += new System.EventHandler(this.BoutenToolStripMenuItemClick);
            // 
            // boutendotafterToolStripMenuItem
            // 
            this.boutendotafterToolStripMenuItem.Name = "boutendotafterToolStripMenuItem";
            this.boutendotafterToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.boutendotafterToolStripMenuItem.Text = "bouten-dot-after";
            this.boutendotafterToolStripMenuItem.Click += new System.EventHandler(this.BoutenToolStripMenuItemClick);
            // 
            // boutendotoutsideToolStripMenuItem
            // 
            this.boutendotoutsideToolStripMenuItem.Name = "boutendotoutsideToolStripMenuItem";
            this.boutendotoutsideToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.boutendotoutsideToolStripMenuItem.Text = "bouten-dot-outside";
            this.boutendotoutsideToolStripMenuItem.Click += new System.EventHandler(this.BoutenToolStripMenuItemClick);
            // 
            // boutenfilledcircleoutsideToolStripMenuItem
            // 
            this.boutenfilledcircleoutsideToolStripMenuItem.Name = "boutenfilledcircleoutsideToolStripMenuItem";
            this.boutenfilledcircleoutsideToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.boutenfilledcircleoutsideToolStripMenuItem.Text = "bouten-filled-circle-outside";
            this.boutenfilledcircleoutsideToolStripMenuItem.Click += new System.EventHandler(this.BoutenToolStripMenuItemClick);
            // 
            // boutenopencircleoutsideToolStripMenuItem
            // 
            this.boutenopencircleoutsideToolStripMenuItem.Name = "boutenopencircleoutsideToolStripMenuItem";
            this.boutenopencircleoutsideToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.boutenopencircleoutsideToolStripMenuItem.Text = "bouten-open-circle-outside";
            this.boutenopencircleoutsideToolStripMenuItem.Click += new System.EventHandler(this.BoutenToolStripMenuItemClick);
            // 
            // boutenopendotoutsideToolStripMenuItem
            // 
            this.boutenopendotoutsideToolStripMenuItem.Name = "boutenopendotoutsideToolStripMenuItem";
            this.boutenopendotoutsideToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.boutenopendotoutsideToolStripMenuItem.Text = "bouten-open-dot-outside";
            this.boutenopendotoutsideToolStripMenuItem.Click += new System.EventHandler(this.BoutenToolStripMenuItemClick);
            // 
            // boutenfilledsesameoutsideToolStripMenuItem
            // 
            this.boutenfilledsesameoutsideToolStripMenuItem.Name = "boutenfilledsesameoutsideToolStripMenuItem";
            this.boutenfilledsesameoutsideToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.boutenfilledsesameoutsideToolStripMenuItem.Text = "bouten-filled-sesame-outside";
            this.boutenfilledsesameoutsideToolStripMenuItem.Click += new System.EventHandler(this.BoutenToolStripMenuItemClick);
            // 
            // boutenopensesameoutsideToolStripMenuItem
            // 
            this.boutenopensesameoutsideToolStripMenuItem.Name = "boutenopensesameoutsideToolStripMenuItem";
            this.boutenopensesameoutsideToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.boutenopensesameoutsideToolStripMenuItem.Text = "bouten-open-sesame-outside";
            this.boutenopensesameoutsideToolStripMenuItem.Click += new System.EventHandler(this.BoutenToolStripMenuItemClick);
            // 
            // boutenautooutsideToolStripMenuItem
            // 
            this.boutenautooutsideToolStripMenuItem.Name = "boutenautooutsideToolStripMenuItem";
            this.boutenautooutsideToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.boutenautooutsideToolStripMenuItem.Text = "bouten-auto-outside";
            this.boutenautooutsideToolStripMenuItem.Click += new System.EventHandler(this.BoutenToolStripMenuItemClick);
            // 
            // boutenautoToolStripMenuItem
            // 
            this.boutenautoToolStripMenuItem.Name = "boutenautoToolStripMenuItem";
            this.boutenautoToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.boutenautoToolStripMenuItem.Text = "bouten-auto";
            this.boutenautoToolStripMenuItem.Click += new System.EventHandler(this.BoutenToolStripMenuItemClick);
            // 
            // toolStripMenuItemRuby
            // 
            this.toolStripMenuItemRuby.Name = "toolStripMenuItemRuby";
            this.toolStripMenuItemRuby.Size = new System.Drawing.Size(273, 22);
            this.toolStripMenuItemRuby.Text = "Ruby...";
            this.toolStripMenuItemRuby.Click += new System.EventHandler(this.toolStripMenuItemRuby_Click);
            // 
            // fontNameToolStripMenuItem
            // 
            this.fontNameToolStripMenuItem.Name = "fontNameToolStripMenuItem";
            this.fontNameToolStripMenuItem.Size = new System.Drawing.Size(273, 22);
            this.fontNameToolStripMenuItem.Text = "Font name...";
            this.fontNameToolStripMenuItem.Click += new System.EventHandler(this.FontNameToolStripMenuItemClick);
            // 
            // toolStripSeparator26
            // 
            this.toolStripSeparator26.Name = "toolStripSeparator26";
            this.toolStripSeparator26.Size = new System.Drawing.Size(270, 6);
            // 
            // toolStripMenuItemInsertUnicodeSymbol
            // 
            this.toolStripMenuItemInsertUnicodeSymbol.Name = "toolStripMenuItemInsertUnicodeSymbol";
            this.toolStripMenuItemInsertUnicodeSymbol.Size = new System.Drawing.Size(273, 22);
            this.toolStripMenuItemInsertUnicodeSymbol.Text = "Insert unicode character";
            // 
            // toolStripMenuItemInsertUnicodeControlCharacters
            // 
            this.toolStripMenuItemInsertUnicodeControlCharacters.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.leftToolStripMenuItem,
            this.righttoleftMarkToolStripMenuItem,
            this.startOfLefttorightEmbeddingLREToolStripMenuItem,
            this.startOfRighttoleftEmbeddingRLEToolStripMenuItem,
            this.startOfLefttorightOverrideLROToolStripMenuItem,
            this.startOfRighttoleftOverrideRLOToolStripMenuItem});
            this.toolStripMenuItemInsertUnicodeControlCharacters.Name = "toolStripMenuItemInsertUnicodeControlCharacters";
            this.toolStripMenuItemInsertUnicodeControlCharacters.Size = new System.Drawing.Size(273, 22);
            this.toolStripMenuItemInsertUnicodeControlCharacters.Text = "Insert unicode control character";
            // 
            // leftToolStripMenuItem
            // 
            this.leftToolStripMenuItem.Name = "leftToolStripMenuItem";
            this.leftToolStripMenuItem.Size = new System.Drawing.Size(272, 22);
            this.leftToolStripMenuItem.Text = "Left-to-right mark (LRM)";
            this.leftToolStripMenuItem.Click += new System.EventHandler(this.leftToolStripMenuItem_Click);
            // 
            // righttoleftMarkToolStripMenuItem
            // 
            this.righttoleftMarkToolStripMenuItem.Name = "righttoleftMarkToolStripMenuItem";
            this.righttoleftMarkToolStripMenuItem.Size = new System.Drawing.Size(272, 22);
            this.righttoleftMarkToolStripMenuItem.Text = "Right-to-left mark (RLM)";
            this.righttoleftMarkToolStripMenuItem.Click += new System.EventHandler(this.righttoleftMarkToolStripMenuItem_Click);
            // 
            // startOfLefttorightEmbeddingLREToolStripMenuItem
            // 
            this.startOfLefttorightEmbeddingLREToolStripMenuItem.Name = "startOfLefttorightEmbeddingLREToolStripMenuItem";
            this.startOfLefttorightEmbeddingLREToolStripMenuItem.Size = new System.Drawing.Size(272, 22);
            this.startOfLefttorightEmbeddingLREToolStripMenuItem.Text = "Start of left-to-right embedding (LRE)";
            this.startOfLefttorightEmbeddingLREToolStripMenuItem.Click += new System.EventHandler(this.startOfLefttorightEmbeddingLREToolStripMenuItem_Click);
            // 
            // startOfRighttoleftEmbeddingRLEToolStripMenuItem
            // 
            this.startOfRighttoleftEmbeddingRLEToolStripMenuItem.Name = "startOfRighttoleftEmbeddingRLEToolStripMenuItem";
            this.startOfRighttoleftEmbeddingRLEToolStripMenuItem.Size = new System.Drawing.Size(272, 22);
            this.startOfRighttoleftEmbeddingRLEToolStripMenuItem.Text = "Start of right-to-left embedding (RLE)";
            this.startOfRighttoleftEmbeddingRLEToolStripMenuItem.Click += new System.EventHandler(this.startOfRighttoleftEmbeddingRLEToolStripMenuItem_Click);
            // 
            // startOfLefttorightOverrideLROToolStripMenuItem
            // 
            this.startOfLefttorightOverrideLROToolStripMenuItem.Name = "startOfLefttorightOverrideLROToolStripMenuItem";
            this.startOfLefttorightOverrideLROToolStripMenuItem.Size = new System.Drawing.Size(272, 22);
            this.startOfLefttorightOverrideLROToolStripMenuItem.Text = "Start of left-to-right override (LRO)";
            this.startOfLefttorightOverrideLROToolStripMenuItem.Click += new System.EventHandler(this.startOfLefttorightOverrideLROToolStripMenuItem_Click);
            // 
            // startOfRighttoleftOverrideRLOToolStripMenuItem
            // 
            this.startOfRighttoleftOverrideRLOToolStripMenuItem.Name = "startOfRighttoleftOverrideRLOToolStripMenuItem";
            this.startOfRighttoleftOverrideRLOToolStripMenuItem.Size = new System.Drawing.Size(272, 22);
            this.startOfRighttoleftOverrideRLOToolStripMenuItem.Text = "Start of right-to-left override (RLO)";
            this.startOfRighttoleftOverrideRLOToolStripMenuItem.Click += new System.EventHandler(this.startOfRighttoleftOverrideRLOToolStripMenuItem_Click);
            // 
            // superscriptToolStripMenuItem
            // 
            this.superscriptToolStripMenuItem.Name = "superscriptToolStripMenuItem";
            this.superscriptToolStripMenuItem.Size = new System.Drawing.Size(273, 22);
            this.superscriptToolStripMenuItem.Text = "Superscript";
            this.superscriptToolStripMenuItem.Click += new System.EventHandler(this.SuperscriptToolStripMenuItemClick);
            // 
            // subscriptToolStripMenuItem
            // 
            this.subscriptToolStripMenuItem.Name = "subscriptToolStripMenuItem";
            this.subscriptToolStripMenuItem.Size = new System.Drawing.Size(273, 22);
            this.subscriptToolStripMenuItem.Text = "Subscript";
            this.subscriptToolStripMenuItem.Click += new System.EventHandler(this.SubscriptToolStripMenuItemClick);
            // 
            // buttonAutoBreak
            // 
            this.buttonAutoBreak.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAutoBreak.Location = new System.Drawing.Point(604, 59);
            this.buttonAutoBreak.Name = "buttonAutoBreak";
            this.buttonAutoBreak.Size = new System.Drawing.Size(115, 23);
            this.buttonAutoBreak.TabIndex = 7;
            this.buttonAutoBreak.Text = "Auto br";
            this.buttonAutoBreak.UseVisualStyleBackColor = true;
            this.buttonAutoBreak.Click += new System.EventHandler(this.ButtonAutoBreakClick);
            // 
            // labelTextLineLengths
            // 
            this.labelTextLineLengths.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelTextLineLengths.AutoSize = true;
            this.labelTextLineLengths.Location = new System.Drawing.Point(239, 94);
            this.labelTextLineLengths.Name = "labelTextLineLengths";
            this.labelTextLineLengths.Size = new System.Drawing.Size(108, 13);
            this.labelTextLineLengths.TabIndex = 12;
            this.labelTextLineLengths.Text = "labelTextLineLengths";
            // 
            // labelTextLineTotal
            // 
            this.labelTextLineTotal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelTextLineTotal.AutoSize = true;
            this.labelTextLineTotal.Location = new System.Drawing.Point(1001, 94);
            this.labelTextLineTotal.Name = "labelTextLineTotal";
            this.labelTextLineTotal.Size = new System.Drawing.Size(94, 13);
            this.labelTextLineTotal.TabIndex = 21;
            this.labelTextLineTotal.Text = "labelTextLineTotal";
            // 
            // labelCharactersPerSecond
            // 
            this.labelCharactersPerSecond.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelCharactersPerSecond.AutoSize = true;
            this.labelCharactersPerSecond.Location = new System.Drawing.Point(432, 11);
            this.labelCharactersPerSecond.Name = "labelCharactersPerSecond";
            this.labelCharactersPerSecond.Size = new System.Drawing.Size(133, 13);
            this.labelCharactersPerSecond.TabIndex = 31;
            this.labelCharactersPerSecond.Text = "labelCharactersPerSecond";
            // 
            // buttonUnBreak
            // 
            this.buttonUnBreak.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonUnBreak.Location = new System.Drawing.Point(604, 30);
            this.buttonUnBreak.Name = "buttonUnBreak";
            this.buttonUnBreak.Size = new System.Drawing.Size(115, 23);
            this.buttonUnBreak.TabIndex = 6;
            this.buttonUnBreak.Text = "Unbreak";
            this.buttonUnBreak.UseVisualStyleBackColor = true;
            this.buttonUnBreak.Click += new System.EventHandler(this.ButtonUnBreakClick);
            // 
            // timeUpDownStartTime
            // 
            this.timeUpDownStartTime.AutoSize = true;
            this.timeUpDownStartTime.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.timeUpDownStartTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.timeUpDownStartTime.Location = new System.Drawing.Point(8, 26);
            this.timeUpDownStartTime.Margin = new System.Windows.Forms.Padding(4);
            this.timeUpDownStartTime.Name = "timeUpDownStartTime";
            this.timeUpDownStartTime.Size = new System.Drawing.Size(111, 27);
            this.timeUpDownStartTime.TabIndex = 0;
            timeCode3.Hours = 0;
            timeCode3.Milliseconds = 0;
            timeCode3.Minutes = 0;
            timeCode3.Seconds = 0;
            timeCode3.TimeSpan = System.TimeSpan.Parse("00:00:00");
            timeCode3.TotalMilliseconds = 0D;
            timeCode3.TotalSeconds = 0D;
            this.timeUpDownStartTime.TimeCode = timeCode3;
            this.timeUpDownStartTime.UseVideoOffset = false;
            // 
            // numericUpDownDuration
            // 
            this.numericUpDownDuration.DecimalPlaces = 3;
            this.numericUpDownDuration.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericUpDownDuration.Location = new System.Drawing.Point(122, 27);
            this.numericUpDownDuration.Maximum = new decimal(new int[] {
            999999999,
            0,
            0,
            0});
            this.numericUpDownDuration.Minimum = new decimal(new int[] {
            99999,
            0,
            0,
            -2147483648});
            this.numericUpDownDuration.Name = "numericUpDownDuration";
            this.numericUpDownDuration.Size = new System.Drawing.Size(56, 20);
            this.numericUpDownDuration.TabIndex = 1;
            this.numericUpDownDuration.ValueChanged += new System.EventHandler(this.NumericUpDownDurationValueChanged);
            // 
            // buttonPrevious
            // 
            this.buttonPrevious.Location = new System.Drawing.Point(8, 78);
            this.buttonPrevious.Name = "buttonPrevious";
            this.buttonPrevious.Size = new System.Drawing.Size(64, 23);
            this.buttonPrevious.TabIndex = 2;
            this.buttonPrevious.Text = "< Prev ";
            this.buttonPrevious.UseVisualStyleBackColor = true;
            this.buttonPrevious.Click += new System.EventHandler(this.ButtonPreviousClick);
            // 
            // buttonNext
            // 
            this.buttonNext.Location = new System.Drawing.Point(74, 78);
            this.buttonNext.Name = "buttonNext";
            this.buttonNext.Size = new System.Drawing.Size(64, 23);
            this.buttonNext.TabIndex = 3;
            this.buttonNext.Text = "Next >";
            this.buttonNext.UseVisualStyleBackColor = true;
            this.buttonNext.Click += new System.EventHandler(this.ButtonNextClick);
            // 
            // labelStartTime
            // 
            this.labelStartTime.AutoSize = true;
            this.labelStartTime.Location = new System.Drawing.Point(9, 11);
            this.labelStartTime.Name = "labelStartTime";
            this.labelStartTime.Size = new System.Drawing.Size(51, 13);
            this.labelStartTime.TabIndex = 3;
            this.labelStartTime.Text = "Start time";
            // 
            // textBoxListViewText
            // 
            this.textBoxListViewText.AllowDrop = true;
            this.textBoxListViewText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxListViewText.ContextMenuStrip = this.contextMenuStripTextBoxListView;
            this.textBoxListViewText.Enabled = false;
            this.textBoxListViewText.HideSelection = false;
            this.textBoxListViewText.Location = new System.Drawing.Point(236, 28);
            this.textBoxListViewText.Multiline = true;
            this.textBoxListViewText.Name = "textBoxListViewText";
            this.textBoxListViewText.Size = new System.Drawing.Size(362, 63);
            this.textBoxListViewText.TabIndex = 5;
            this.textBoxListViewText.MouseClick += new System.Windows.Forms.MouseEventHandler(this.TextBoxListViewTextMouseClick);
            this.textBoxListViewText.TextChanged += new System.EventHandler(this.TextBoxListViewTextTextChanged);
            this.textBoxListViewText.Enter += new System.EventHandler(this.TextBoxListViewTextEnter);
            this.textBoxListViewText.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBoxListViewTextKeyDown);
            this.textBoxListViewText.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBoxListViewText_KeyUp);
            this.textBoxListViewText.Leave += new System.EventHandler(this.textBoxListViewText_Leave);
            this.textBoxListViewText.MouseMove += new System.Windows.Forms.MouseEventHandler(this.textBoxListViewText_MouseMove);
            // 
            // labelDuration
            // 
            this.labelDuration.AutoSize = true;
            this.labelDuration.Location = new System.Drawing.Point(122, 11);
            this.labelDuration.Name = "labelDuration";
            this.labelDuration.Size = new System.Drawing.Size(47, 13);
            this.labelDuration.TabIndex = 4;
            this.labelDuration.Text = "Duration";
            // 
            // labelAutoDuration
            // 
            this.labelAutoDuration.AutoSize = true;
            this.labelAutoDuration.Location = new System.Drawing.Point(94, 11);
            this.labelAutoDuration.Name = "labelAutoDuration";
            this.labelAutoDuration.Size = new System.Drawing.Size(29, 13);
            this.labelAutoDuration.TabIndex = 30;
            this.labelAutoDuration.Text = "Auto";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.textBoxSource);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(730, 222);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Source view";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // textBoxSource
            // 
            this.textBoxSource.AllowDrop = true;
            this.textBoxSource.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxSource.HideSelection = false;
            this.textBoxSource.Location = new System.Drawing.Point(3, 3);
            this.textBoxSource.MaxLength = 0;
            this.textBoxSource.Multiline = true;
            this.textBoxSource.Name = "textBoxSource";
            this.textBoxSource.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxSource.Size = new System.Drawing.Size(724, 216);
            this.textBoxSource.TabIndex = 12;
            this.textBoxSource.WordWrap = false;
            this.textBoxSource.Click += new System.EventHandler(this.TextBoxSourceClick);
            this.textBoxSource.TextChanged += new System.EventHandler(this.TextBoxSourceTextChanged);
            this.textBoxSource.DragDrop += new System.Windows.Forms.DragEventHandler(this.TextBoxSourceDragDrop);
            this.textBoxSource.DragEnter += new System.Windows.Forms.DragEventHandler(this.TextBoxSourceDragEnter);
            this.textBoxSource.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBoxSourceKeyDown);
            this.textBoxSource.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBoxSource_KeyUp);
            this.textBoxSource.Leave += new System.EventHandler(this.TextBoxSourceLeave);
            // 
            // panelVideoPlayer
            // 
            this.panelVideoPlayer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelVideoPlayer.Controls.Add(this.mediaPlayer);
            this.panelVideoPlayer.Location = new System.Drawing.Point(1, 1);
            this.panelVideoPlayer.Name = "panelVideoPlayer";
            this.panelVideoPlayer.Size = new System.Drawing.Size(220, 246);
            this.panelVideoPlayer.TabIndex = 5;
            // 
            // mediaPlayer
            // 
            this.mediaPlayer.AllowDrop = true;
            this.mediaPlayer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mediaPlayer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(18)))), ((int)(((byte)(18)))));
            this.mediaPlayer.CurrentPosition = 0D;
            this.mediaPlayer.FontSizeFactor = 1F;
            this.mediaPlayer.LastParagraph = null;
            this.mediaPlayer.Location = new System.Drawing.Point(0, 0);
            this.mediaPlayer.Margin = new System.Windows.Forms.Padding(0);
            this.mediaPlayer.Name = "mediaPlayer";
            this.mediaPlayer.ShowFullscreenButton = true;
            this.mediaPlayer.ShowMuteButton = true;
            this.mediaPlayer.ShowStopButton = true;
            this.mediaPlayer.Size = new System.Drawing.Size(219, 246);
            this.mediaPlayer.SmpteMode = false;
            this.mediaPlayer.SubtitleText = "";
            this.mediaPlayer.TabIndex = 5;
            this.mediaPlayer.TextRightToLeft = System.Windows.Forms.RightToLeft.No;
            this.mediaPlayer.VideoHeight = 0;
            this.mediaPlayer.VideoPlayer = null;
            this.mediaPlayer.VideoWidth = 0;
            this.mediaPlayer.Volume = 0D;
            this.mediaPlayer.DragDrop += new System.Windows.Forms.DragEventHandler(this.mediaPlayer_DragDrop);
            this.mediaPlayer.DragEnter += new System.Windows.Forms.DragEventHandler(this.mediaPlayer_DragEnter);
            // 
            // contextMenuStripEmpty
            // 
            this.contextMenuStripEmpty.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.insertLineToolStripMenuItem});
            this.contextMenuStripEmpty.Name = "contextMenuStripEmpty";
            this.contextMenuStripEmpty.Size = new System.Drawing.Size(126, 26);
            // 
            // insertLineToolStripMenuItem
            // 
            this.insertLineToolStripMenuItem.Name = "insertLineToolStripMenuItem";
            this.insertLineToolStripMenuItem.Size = new System.Drawing.Size(125, 22);
            this.insertLineToolStripMenuItem.Text = "Insert line";
            this.insertLineToolStripMenuItem.Click += new System.EventHandler(this.InsertLineToolStripMenuItemClick);
            // 
            // imageListPlayRate
            // 
            this.imageListPlayRate.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListPlayRate.ImageStream")));
            this.imageListPlayRate.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListPlayRate.Images.SetKeyName(0, "FastForward.png");
            this.imageListPlayRate.Images.SetKeyName(1, "FastForwardHighLight.png");
            // 
            // timerTextUndo
            // 
            this.timerTextUndo.Interval = 700;
            this.timerTextUndo.Tick += new System.EventHandler(this.TimerTextUndoTick);
            // 
            // timerAlternateTextUndo
            // 
            this.timerAlternateTextUndo.Interval = 700;
            this.timerAlternateTextUndo.Tick += new System.EventHandler(this.TimerAlternateTextUndoTick);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(975, 646);
            this.Controls.Add(this.splitContainerMain);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(800, 554);
            this.Name = "Main";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
            this.Load += new System.EventHandler(this.Main_Load);
            this.Shown += new System.EventHandler(this.Main_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainKeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.MainKeyUp);
            this.Resize += new System.EventHandler(this.Main_Resize);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.contextMenuStripListview.ResumeLayout(false);
            this.groupBoxVideo.ResumeLayout(false);
            this.groupBoxVideo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarWaveformPosition)).EndInit();
            this.panelWaveformControls.ResumeLayout(false);
            this.panelWaveformControls.PerformLayout();
            this.toolStripWaveControls.ResumeLayout(false);
            this.toolStripWaveControls.PerformLayout();
            this.tabControlButtons.ResumeLayout(false);
            this.tabPageTranslate.ResumeLayout(false);
            this.tabPageTranslate.PerformLayout();
            this.groupBoxTranslateSearch.ResumeLayout(false);
            this.groupBoxTranslateSearch.PerformLayout();
            this.groupBoxAutoContinue.ResumeLayout(false);
            this.groupBoxAutoContinue.PerformLayout();
            this.groupBoxAutoRepeat.ResumeLayout(false);
            this.groupBoxAutoRepeat.PerformLayout();
            this.tabPageCreate.ResumeLayout(false);
            this.tabPageCreate.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSec2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSec1)).EndInit();
            this.tabPageAdjust.ResumeLayout(false);
            this.tabPageAdjust.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSecAdjust2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSecAdjust1)).EndInit();
            this.contextMenuStripWaveform.ResumeLayout(false);
            this.splitContainerMain.Panel1.ResumeLayout(false);
            this.splitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).EndInit();
            this.splitContainerMain.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tabControlSubtitle.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.splitContainerListViewAndText.Panel1.ResumeLayout(false);
            this.splitContainerListViewAndText.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerListViewAndText)).EndInit();
            this.splitContainerListViewAndText.ResumeLayout(false);
            this.groupBoxEdit.ResumeLayout(false);
            this.groupBoxEdit.PerformLayout();
            this.panelBookmark.ResumeLayout(false);
            this.panelBookmark.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxBookmark)).EndInit();
            this.contextMenuStripTextBoxListView.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDuration)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.panelVideoPlayer.ResumeLayout(false);
            this.contextMenuStripEmpty.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem reopenToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem findToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem findNextToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem replaceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gotoLineNumberToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.TabControl tabControlSubtitle;
        private System.Windows.Forms.TabPage tabPage1;
        private Nikse.SubtitleEdit.Controls.SubtitleListView SubtitleListview1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TextBox textBoxSource;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.ToolStripButton toolStripButtonFileOpen;
        private System.Windows.Forms.ToolStripButton toolStripButtonFileNew;
        private System.Windows.Forms.ToolStripButton toolStripButtonSave;
        private System.Windows.Forms.ToolStripButton toolStripButtonSaveAs;
        private System.Windows.Forms.ToolStripButton toolStripButtonFind;
        private System.Windows.Forms.ToolStripButton toolStripButtonReplace;
        private System.Windows.Forms.ToolStripButton toolStripButtonSpellCheck;
        private System.Windows.Forms.ToolStripButton toolStripButtonSettings;
        private System.Windows.Forms.ToolStripButton toolStripButtonVisualSync;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorFindReplace;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorFixSyncSpell;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorHelp;
        private System.Windows.Forms.ToolStripButton toolStripButtonHelp;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ToolStripStatusLabel labelStatus;
        private System.Windows.Forms.ToolStripMenuItem adjustDisplayTimeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fixToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem startNumberingFromToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeTextForHearImpairedToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem splitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem appendTextVisuallyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showHistoryforUndoToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripListview;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemDelete;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemInsertBefore;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemInsertAfter;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripMenuItem boldToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem italicToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem underlineToolStripMenuItem;
        private System.Windows.Forms.GroupBox groupBoxEdit;
        private System.Windows.Forms.Label labelText;
        private System.Windows.Forms.Label labelDuration;
        private System.Windows.Forms.Label labelStartTime;
        //private System.Windows.Forms.TextBox textBoxListViewText;
        Nikse.SubtitleEdit.Controls.SETextBox textBoxListViewText;
        private System.Windows.Forms.Button buttonPrevious;
        private System.Windows.Forms.Button buttonNext;
        private System.Windows.Forms.ToolStripMenuItem splitLineToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mergeBeforeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mergeAfterToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.Button buttonAutoBreak;
        private System.Windows.Forms.ToolStripMenuItem removeFormattinglToolStripMenuItem;
        private System.Windows.Forms.Label labelTextLineLengths;
        private System.Windows.Forms.NumericUpDown numericUpDownDuration;
        private System.Windows.Forms.Label labelDurationWarning;
        private System.Windows.Forms.Label labelStartTimeWarning;
        private System.Windows.Forms.Button buttonUnBreak;
        private System.Windows.Forms.ToolStripMenuItem colorToolStripMenuItem;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.Label labelTextLineTotal;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem typeEffectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem karokeeEffectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem matroskaImportStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator10;
        private Nikse.SubtitleEdit.Controls.TimeUpDown timeUpDownStartTime;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemManualAnsi;
        private System.Windows.Forms.ToolStripMenuItem ChangeCasingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemMergeLines;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorAdvancedFunctions;
        private System.Windows.Forms.ToolStripMenuItem visualSyncSelectedLinesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showSelectedLinesEarlierlaterToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem adjustDisplayTimeForSelectedLinesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fixCommonErrorsInSelectedLinesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem changeCasingForSelectedLinesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSortBy;
        private System.Windows.Forms.ToolStripMenuItem sortNumberToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sortStartTimeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sortEndTimeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sortDisplayTimeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sortTextMaxLineLengthToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sortTextTotalLengthToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sortTextNumberOfLinesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sortTextAlphabeticallytoolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem changeLanguageToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator12;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemCompare;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemUnbreakLines;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemAutoBreakLines;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorBreakLines;
        private System.Windows.Forms.ToolStripMenuItem multipleReplaceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemImportDvdSubtitles;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSubIdx;
        private System.Windows.Forms.ToolStripStatusLabel toolStripSelected;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemInsertUnicodeCharacter;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorInsertUnicodeCharacter;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemAutoMergeShortLines;
        private System.Windows.Forms.ToolStripMenuItem setMinimumDisplayTimeBetweenParagraphsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemImportText;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemImportTimeCodes;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemFont;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator14;
        private System.Windows.Forms.GroupBox groupBoxVideo;
        private System.Windows.Forms.Button buttonGotoSub;
        private System.Windows.Forms.Button buttonBeforeText;
        private System.Windows.Forms.Button buttonSetEnd;
        private System.Windows.Forms.Button buttonSetStartTime;
        private System.Windows.Forms.Button buttonInsertNewText;
        private System.Windows.Forms.Button buttonSecBack1;
        private System.Windows.Forms.Timer ShowSubtitleTimer;
        private System.Windows.Forms.Timer timerAutoDuration;
        private System.Windows.Forms.Label labelAutoDuration;
        private System.Windows.Forms.Timer timerAutoContinue;
        private System.Windows.Forms.ToolStripComboBox comboBoxSubtitleFormats;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorToggle;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorSubtitleFormat;
        private System.Windows.Forms.ToolStripLabel toolStripLabelSubtitleFormat;
        private System.Windows.Forms.ToolStripLabel toolStripLabelEncoding;
        private System.Windows.Forms.ToolStripComboBox comboBoxEncoding;
        private System.Windows.Forms.ToolStripButton toolStripButtonToggleVideo;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorEncoding;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorFrameRate;
        private System.Windows.Forms.ToolStripLabel toolStripLabelFrameRate;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxFrameRate;
        private System.Windows.Forms.ToolStripButton toolStripButtonGetFrameRate;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSpellCheckMain;
        private System.Windows.Forms.ToolStripMenuItem spellCheckToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem findDoubleWordsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
        private System.Windows.Forms.ToolStripMenuItem GetDictionariesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addWordToNameListToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSynchronization;
        private System.Windows.Forms.ToolStripMenuItem visualSyncToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemPointSync;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemAdjustAllTimes;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemAutoTranslate;
        private System.Windows.Forms.ToolStripMenuItem translateByGoogleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem translateFromSwedishToDanishToolStripMenuItem;
        private System.Windows.Forms.Timer timerStillTyping;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemVideo;
        private System.Windows.Forms.ToolStripMenuItem openVideoToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem showhideVideoToolStripMenuItem;
        private System.Windows.Forms.Label labelVideoPosition;
        private Controls.TimeUpDown timeUpDownVideoPosition;
        private System.Windows.Forms.TabControl tabControlButtons;
        private System.Windows.Forms.TabPage tabPageTranslate;
        private System.Windows.Forms.GroupBox groupBoxTranslateSearch;
        private System.Windows.Forms.Button buttonGoogleTranslateIt;
        private System.Windows.Forms.Button buttonGoogleIt;
        private System.Windows.Forms.TextBox textBoxSearchWord;
        private System.Windows.Forms.GroupBox groupBoxAutoContinue;
        private System.Windows.Forms.ComboBox comboBoxAutoContinue;
        private System.Windows.Forms.Label labelAutoContinueDelay;
        private System.Windows.Forms.CheckBox checkBoxAutoContinue;
        private System.Windows.Forms.GroupBox groupBoxAutoRepeat;
        private System.Windows.Forms.ComboBox comboBoxAutoRepeat;
        private System.Windows.Forms.Label labelAutoRepeatCount;
        private System.Windows.Forms.CheckBox checkBoxAutoRepeatOn;
        private System.Windows.Forms.Button buttonStop;
        private System.Windows.Forms.Button buttonPlayPrevious;
        private System.Windows.Forms.Button buttonPlayCurrent;
        private System.Windows.Forms.Button buttonPlayNext;
        private System.Windows.Forms.TabPage tabPageCreate;
        private System.Windows.Forms.TabPage tabPageAdjust;
        private System.Windows.Forms.Button buttonAdjustGoToPosAndPause;
        private System.Windows.Forms.Button buttonAdjustPlayBefore;
        private System.Windows.Forms.Button buttonAdjustSetEndTime;
        private System.Windows.Forms.Button buttonSetEndAndGoToNext;
        private System.Windows.Forms.Button buttonSetStartAndOffsetRest;
        private System.Windows.Forms.Button buttonAdjustSetStartTime;
        private System.Windows.Forms.Label labelTranslateTip;
        private System.Windows.Forms.Button buttonCustomUrl1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemOpenContainingFolder;
        private System.Windows.Forms.NumericUpDown numericUpDownSec1;
        private System.Windows.Forms.Button buttonForward1;
        private System.Windows.Forms.Label labelVideoPosition2;
        private Controls.TimeUpDown timeUpDownVideoPositionAdjust;
        private System.Windows.Forms.Button buttonAdjustSecForward1;
        private System.Windows.Forms.NumericUpDown numericUpDownSecAdjust1;
        private System.Windows.Forms.Button buttonAdjustSecBack1;
        private System.Windows.Forms.Button buttonForward2;
        private System.Windows.Forms.NumericUpDown numericUpDownSec2;
        private System.Windows.Forms.Button buttonSecBack2;
        private System.Windows.Forms.Button buttonAdjustSecForward2;
        private System.Windows.Forms.NumericUpDown numericUpDownSecAdjust2;
        private System.Windows.Forms.Button buttonAdjustSecBack2;
        private System.Windows.Forms.CheckBox checkBoxSyncListViewWithVideoWhilePlaying;
        private System.Windows.Forms.Label labelAdjustF10;
        private System.Windows.Forms.Label labelAdjustF9;
        private System.Windows.Forms.Label labelAdjustF11;
        private System.Windows.Forms.Label labelAdjustF12;
        private System.Windows.Forms.Label labelCreateF12;
        private System.Windows.Forms.Label labelCreateF11;
        private System.Windows.Forms.Label labelCreateF10;
        private System.Windows.Forms.Label labelCreateF9;
        private System.Windows.Forms.ToolStripMenuItem translatepoweredByMicrosoftToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton toolStripButtonToggleWaveform;
        private Controls.VideoPlayerContainer mediaPlayer;
        private System.Windows.Forms.Panel panelVideoPlayer;
        private Controls.AudioVisualizer audioVisualizer;
        private System.Windows.Forms.Timer timerWaveform;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripWaveform;
        private System.Windows.Forms.ToolStripMenuItem addParagraphHereToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteParagraphToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem splitToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem mergeWithPreviousToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mergeWithNextToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemWaveformPlaySelection;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator11;
        private System.Windows.Forms.Panel panelWaveformControls;
        private System.Windows.Forms.ToolStrip toolStripWaveControls;
        private System.Windows.Forms.ToolStripButton toolStripButtonWaveformZoomIn;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxWaveform;
        private System.Windows.Forms.ToolStripButton toolStripButtonWaveformZoomOut;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator16;
        private System.Windows.Forms.ToolStripButton toolStripButtonWaveformPause;
        private System.Windows.Forms.ToolStripButton toolStripButtonWaveformPlay;
        private System.Windows.Forms.TrackBar trackBarWaveformPosition;
        private System.Windows.Forms.Label labelVideoInfo;
        private System.Windows.Forms.ToolStripMenuItem showhideWaveformToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemImportBluRaySup;
        private System.Windows.Forms.Label labelCharactersPerSecond;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemNetworking;
        private System.Windows.Forms.ToolStripMenuItem startServerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem joinSessionToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusNetworking;
        private System.Windows.Forms.ToolStripMenuItem leaveSessionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showSessionKeyLogToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem chatToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainerMain;
        private System.Windows.Forms.ToolStripMenuItem undockVideoControlsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem redockVideoControlsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator19;
        private System.Windows.Forms.ToolStripButton toolStripButtonLockCenter;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemInsertSubtitle;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemAutoSplitLongLines;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripEmpty;
        private System.Windows.Forms.ToolStripMenuItem insertLineToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeVideoToolStripMenuItem;
        private System.Windows.Forms.Label labelSingleLine;
        private System.Windows.Forms.Label labelAlternateText;
        private System.Windows.Forms.Label labelAlternateCharactersPerSecond;
        private System.Windows.Forms.Label labelTextAlternateLineTotal;
        private System.Windows.Forms.Label labelAlternateSingleLine;
        private System.Windows.Forms.Label labelTextAlternateLineLengths;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator20;
        private System.Windows.Forms.ToolStripMenuItem saveOriginalToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveOriginalAstoolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openOriginalToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeOriginalToolStripMenuItem;
        private System.Windows.Forms.ToolStripSplitButton toolStripSplitButtonPlayRate;
        private System.Windows.Forms.ImageList imageListPlayRate;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSetAudioTrack;
        private System.Windows.Forms.Button buttonSplitLine;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemChangeFrameRate2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemCopySourceText;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator21;
        private System.Windows.Forms.ToolStripMenuItem editSelectAllToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripTextBoxListView;
        private System.Windows.Forms.ToolStripMenuItem cutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSplitTextAtCursor;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator18;
        private System.Windows.Forms.ToolStripMenuItem selectAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator17;
        private System.Windows.Forms.ToolStripMenuItem normalToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem boldToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem italicToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem underlineToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem colorToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem fontNameToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator22;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemExport;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemExportPngXml;
        private System.Windows.Forms.ToolStripMenuItem pointSyncViaOtherSubtitleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemGoogleMicrosoftTranslateSelLine;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator23;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemMakeEmptyFromCurrent;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator24;
        private System.Windows.Forms.ToolStripMenuItem showWaveformAndSpectrogramToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showOnlyWaveformToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showOnlySpectrogramToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator26;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemInsertUnicodeSymbol;
        private System.Windows.Forms.ToolStripMenuItem setStylesForSelectedLinesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem FindDoubleLinesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem eBUSTLToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pACScreenElectronicsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemCavena890;
        private Controls.SETextBox textBoxListViewTextAlternate;
        private System.Windows.Forms.ToolStripMenuItem textCharssecToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem plainTextToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bluraySupToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem vobSubsubidxToolStripMenuItem;
        private System.Windows.Forms.Timer timerTextUndo;
        private System.Windows.Forms.Timer timerAlternateTextUndo;
        private System.Windows.Forms.ToolStripMenuItem adobeEncoreFABImageScriptToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemMergeDialog;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSurroundWithMusicSymbols;
        private System.Windows.Forms.ToolStripMenuItem superscriptToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem subscriptToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemImagePerFrame;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemApplyDurationLimits;
        private System.Windows.Forms.ToolStripMenuItem generateDatetimeInfoFromVideoToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator25;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemRightToLeftMode;
        private System.Windows.Forms.ToolStripMenuItem joinSubtitlesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemReverseRightToLeftStartEnd;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemExportCapMakerPlus;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemExportCheetahCap;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemExportCaptionInc;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemExportUltech130;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemAssStyles;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSubStationAlpha;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemAlignment;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemRestoreAutoBackup;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemStatistics;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemDCinemaProperties;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemTextTimeCodePair;
        private System.Windows.Forms.ToolStripMenuItem textWordsPerMinutewpmToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemTTProperties;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSaveSelectedLines;
        private System.Windows.Forms.Button buttonCustomUrl2;
        private System.Windows.Forms.ToolStripMenuItem addParagraphAndPasteToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorGuessTimeCodes;
        private System.Windows.Forms.ToolStripMenuItem guessTimeCodesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem DvdStudioProStl;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemUndo;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemRedo;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemShowOriginalInPreview;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemPlugins;
        private System.Windows.Forms.ToolStripMenuItem seekSilenceToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainerListViewAndText;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemColumn;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemColumnDeleteText;
        private System.Windows.Forms.ToolStripMenuItem ShiftTextCellsDownToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemPasteSpecial;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemColumnImportText;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemInsertTextFromSub;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemOpenKeepVideo;
        private System.Windows.Forms.ToolStripMenuItem changeSpeedInPercentToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemAvidStl;
        private System.Windows.Forms.ToolStripMenuItem columnDeleteTextOnlyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemBatchConvert;
        private System.Windows.Forms.ToolStripMenuItem copyOriginalTextToCurrentToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemMergeDuplicateText;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSpumux;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemWebVTT;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemWebVttVoice;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorWebVTT;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemModifySelection;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemInverseSelection;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSpellCheckFromCurrentLine;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemImportXSub;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemImportOcrHardSub;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemExportFcpIImage;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemNuendoProperties;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemDost;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemMeasurementConverter;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemImportSceneChanges;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemRemoveSceneChanges;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemDurationBridgeGaps;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemOpenDvd;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemFcpProperties;
        private System.Windows.Forms.ToolStripMenuItem styleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemFocusTextbox;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorAscOrDesc;
        private System.Windows.Forms.ToolStripMenuItem AscendingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem descendingToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorExportCustomText;
        private System.Windows.Forms.ToolStripMenuItem exportCustomTextFormatToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSetLanguage;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemInsertUnicodeControlCharacters;
        private System.Windows.Forms.ToolStripMenuItem leftToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem righttoleftMarkToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem startOfLefttorightEmbeddingLREToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem startOfRighttoleftEmbeddingRLEToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem startOfLefttorightOverrideLROToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem startOfRighttoleftOverrideRLOToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemRtlUnicodeControlChars;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemImportImages;
        private System.Windows.Forms.ToolStripButton toolStripButtonRemoveTextForHi;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemExportDcinemaInterop;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemMergeLinesWithSameTimeCodes;
        private System.Windows.Forms.ToolStripMenuItem checkForUpdatesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItemSplitterCheckForUpdates;
        private System.Windows.Forms.ToolStripMenuItem setVideoOffsetToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemDvdStudioProProperties;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemEdlClipName;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemAddWaveformBatch;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemExportBdTextSt;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelProgress;
        private System.Windows.Forms.ToolStripMenuItem uniPacExportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemExportAyato;
        private System.Windows.Forms.ToolStripButton toolStripButtonFixCommonErrors;
        private System.Windows.Forms.ToolStripMenuItem removeSceneChangeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addSceneChangeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem netflixQualityCheckToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton toolStripButtonNetflixQualityCheck;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemEdl;
        private System.Windows.Forms.ToolStripMenuItem setActorForSelectedLinesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSetRegion;
        private System.Windows.Forms.ToolStripMenuItem insertSubtitleHereToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem actorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemFcpXmlAdvanced;
        private System.Windows.Forms.Label labelNextWord;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemOpenVideoFromUrl;
        private System.Windows.Forms.ToolStripMenuItem smpteTimeModedropFrameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveTextUpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveTextDownToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem generateTextFromCurrentVideoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSplitViaWaveform;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemEbuProperties;
        private System.Windows.Forms.ToolStripMenuItem boxToolStripMenuItem;
        private System.Windows.Forms.ImageList imageListBookmarks;
        private System.Windows.Forms.Panel panelBookmark;
        private System.Windows.Forms.Label labelBookmark;
        private System.Windows.Forms.PictureBox pictureBoxBookmark;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemTranslateSelected;
        private System.Windows.Forms.ToolStripMenuItem googleTranslateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem microsoftBingTranslateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemBookmark;
        private System.Windows.Forms.ToolStripMenuItem removeAllFormattingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeBoldToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeItalicToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeUnderlineToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeColorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeFontNameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeAlignmentToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemBouten;
        private System.Windows.Forms.ToolStripMenuItem boutendotbeforeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem boutendotafterToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem boutendotoutsideToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem boutenfilledcircleoutsideToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem boutenopencircleoutsideToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem boutenopendotoutsideToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem boutenfilledsesameoutsideToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem boutenopensesameoutsideToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem boutenautooutsideToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem boutenautoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemHorizontalDigits;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSetParagraphAsSelection;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemRuby;
    }
}