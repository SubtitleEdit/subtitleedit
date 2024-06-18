namespace Nikse.SubtitleEdit.Forms.Options
{
    sealed partial class Settings
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
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.listBoxSection = new Nikse.SubtitleEdit.Controls.NikseListBox();
            this.panelGeneral = new System.Windows.Forms.Panel();
            this.groupBoxMiscellaneous = new System.Windows.Forms.GroupBox();
            this.labelDefaultLanguagesList = new System.Windows.Forms.Label();
            this.buttonDefaultLanguages = new System.Windows.Forms.Button();
            this.labelDefaultLanguages = new System.Windows.Forms.Label();
            this.buttonTranslationAutoSuffix = new System.Windows.Forms.Button();
            this.comboBoxTranslationAutoSuffix = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelTranslationAutoSuffix = new System.Windows.Forms.Label();
            this.labelSplitBehavior = new System.Windows.Forms.Label();
            this.comboBoxSplitBehavior = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.checkBoxAutoSave = new System.Windows.Forms.CheckBox();
            this.comboBoxSaveAsFileNameFrom = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelSaveAsFileNameFrom = new System.Windows.Forms.Label();
            this.groupBoxGeneralRules = new System.Windows.Forms.GroupBox();
            this.buttonEditCustomContinuationStyle = new System.Windows.Forms.Button();
            this.comboBoxCpsLineLenCalc = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelCpsLineLenCalc = new System.Windows.Forms.Label();
            this.buttonGapChoose = new System.Windows.Forms.Button();
            this.comboBoxContinuationStyle = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelContinuationStyle = new System.Windows.Forms.Label();
            this.labelDialogStyle = new System.Windows.Forms.Label();
            this.comboBoxDialogStyle = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.buttonEditProfile = new System.Windows.Forms.Button();
            this.comboBoxRulesProfileName = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelRulesProfileName = new System.Windows.Forms.Label();
            this.labelOptimalCharsPerSecond = new System.Windows.Forms.Label();
            this.numericUpDownOptimalCharsSec = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelSubMaxLen = new System.Windows.Forms.Label();
            this.numericUpDownMaxWordsMin = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelMergeShortLines = new System.Windows.Forms.Label();
            this.labelMaxWordsPerMin = new System.Windows.Forms.Label();
            this.labelMinDuration = new System.Windows.Forms.Label();
            this.numericUpDownMaxNumberOfLines = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelMaxDuration = new System.Windows.Forms.Label();
            this.labelMaxLines = new System.Windows.Forms.Label();
            this.numericUpDownDurationMin = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.numericUpDownDurationMax = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.comboBoxMergeShortLineLength = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelMaxCharsPerSecond = new System.Windows.Forms.Label();
            this.numericUpDownMinGapMs = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.numericUpDownMaxCharsSec = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelMinGapMs = new System.Windows.Forms.Label();
            this.numericUpDownSubtitleLineMaximumLength = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.comboBoxAutoBackupDeleteAfter = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelAutoBackupDeleteAfter = new System.Windows.Forms.Label();
            this.checkBoxCheckForUpdates = new System.Windows.Forms.CheckBox();
            this.labelSpellChecker = new System.Windows.Forms.Label();
            this.comboBoxTimeCodeMode = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelTimeCodeMode = new System.Windows.Forms.Label();
            this.comboBoxEncoding = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.checkBoxAutoDetectAnsiEncoding = new System.Windows.Forms.CheckBox();
            this.textBoxShowLineBreaksAs = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.checkBoxAutoWrapWhileTyping = new System.Windows.Forms.CheckBox();
            this.checkBoxPromptDeleteLines = new System.Windows.Forms.CheckBox();
            this.checkBoxAllowEditOfOriginalSubtitle = new System.Windows.Forms.CheckBox();
            this.comboBoxSpellChecker = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.comboBoxAutoBackup = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelAutoBackup = new System.Windows.Forms.Label();
            this.checkBoxRememberSelectedLine = new System.Windows.Forms.CheckBox();
            this.checkBoxRemoveBlankLinesWhenOpening = new System.Windows.Forms.CheckBox();
            this.labelAutoDetectAnsiEncoding = new System.Windows.Forms.Label();
            this.comboBoxListViewDoubleClickEvent = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelListViewDoubleClickEvent = new System.Windows.Forms.Label();
            this.labelShowLineBreaksAs = new System.Windows.Forms.Label();
            this.checkBoxRememberWindowPosition = new System.Windows.Forms.CheckBox();
            this.checkBoxStartInSourceView = new System.Windows.Forms.CheckBox();
            this.checkBoxReopenLastOpened = new System.Windows.Forms.CheckBox();
            this.checkBoxRememberRecentFiles = new System.Windows.Forms.CheckBox();
            this.labelDefaultFileEncoding = new System.Windows.Forms.Label();
            this.comboBoxFrameRate = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelDefaultFrameRate = new System.Windows.Forms.Label();
            this.panelSubtitleFormats = new System.Windows.Forms.Panel();
            this.groupBoxSubtitleFormats = new System.Windows.Forms.GroupBox();
            this.groupBoxFavoriteSubtitleFormats = new System.Windows.Forms.GroupBox();
            this.labelFavoriteSubtitleFormatsNote = new System.Windows.Forms.Label();
            this.listBoxSubtitleFormats = new Nikse.SubtitleEdit.Controls.NikseListBox();
            this.buttonFormatsSearchClear = new System.Windows.Forms.Button();
            this.textBoxFormatsSearch = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.labelFormatsSearch = new System.Windows.Forms.Label();
            this.labelFormats = new System.Windows.Forms.Label();
            this.buttonRemoveFromFavoriteFormats = new System.Windows.Forms.Button();
            this.buttonMoveToFavoriteFormats = new System.Windows.Forms.Button();
            this.listBoxFavoriteSubtitleFormats = new Nikse.SubtitleEdit.Controls.NikseListBox();
            this.contextMenuStripFavoriteFormats = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.moveUpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveDownToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveToTopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveToBottomToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.labelFavoriteFormats = new System.Windows.Forms.Label();
            this.comboBoxSubtitleSaveAsFormats = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelDefaultSaveAsFormat = new System.Windows.Forms.Label();
            this.comboBoxSubtitleFormats = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelDefaultSubtitleFormat = new System.Windows.Forms.Label();
            this.panelShortcuts = new System.Windows.Forms.Panel();
            this.groupBoxShortcuts = new System.Windows.Forms.GroupBox();
            this.nikseComboBoxShortcutsFilter = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelShortcutsFilter = new System.Windows.Forms.Label();
            this.buttonShortcutsClear = new System.Windows.Forms.Button();
            this.labelShortcutsSearch = new System.Windows.Forms.Label();
            this.textBoxShortcutSearch = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.buttonClearShortcut = new System.Windows.Forms.Button();
            this.comboBoxShortcutKey = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelShortcutKey = new System.Windows.Forms.Label();
            this.checkBoxShortcutsShift = new System.Windows.Forms.CheckBox();
            this.checkBoxShortcutsAlt = new System.Windows.Forms.CheckBox();
            this.checkBoxShortcutsControl = new System.Windows.Forms.CheckBox();
            this.buttonUpdateShortcut = new System.Windows.Forms.Button();
            this.treeViewShortcuts = new System.Windows.Forms.TreeView();
            this.contextMenuStripShortcuts = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemShortcutsCollapse = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.importShortcutsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportShortcutsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportAsHtmlToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.labelShortcut = new System.Windows.Forms.Label();
            this.panelSyntaxColoring = new System.Windows.Forms.Panel();
            this.groupBoxListViewSyntaxColoring = new System.Windows.Forms.GroupBox();
            this.buttonLineWidthSettings = new System.Windows.Forms.Button();
            this.checkBoxSyntaxColorTextTooWide = new System.Windows.Forms.CheckBox();
            this.checkBoxSyntaxColorGapTooSmall = new System.Windows.Forms.CheckBox();
            this.checkBoxSyntaxColorTextMoreThanTwoLines = new System.Windows.Forms.CheckBox();
            this.checkBoxSyntaxOverlap = new System.Windows.Forms.CheckBox();
            this.checkBoxSyntaxColorDurationTooSmall = new System.Windows.Forms.CheckBox();
            this.buttonListViewSyntaxColorError = new System.Windows.Forms.Button();
            this.checkBoxSyntaxColorTextTooLong = new System.Windows.Forms.CheckBox();
            this.checkBoxSyntaxColorDurationTooLarge = new System.Windows.Forms.CheckBox();
            this.panelListViewSyntaxColorError = new System.Windows.Forms.Panel();
            this.panelVideoPlayer = new System.Windows.Forms.Panel();
            this.groupBoxMainWindowVideoControls = new System.Windows.Forms.GroupBox();
            this.labelCustomSearch5 = new System.Windows.Forms.Label();
            this.textBoxCustomSearchUrl5 = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.comboBoxCustomSearch5 = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelCustomSearch4 = new System.Windows.Forms.Label();
            this.textBoxCustomSearchUrl4 = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.comboBoxCustomSearch4 = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelCustomSearch3 = new System.Windows.Forms.Label();
            this.textBoxCustomSearchUrl3 = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.comboBoxCustomSearch3 = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelCustomSearch2 = new System.Windows.Forms.Label();
            this.textBoxCustomSearchUrl2 = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.comboBoxCustomSearch2 = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelCustomSearch1 = new System.Windows.Forms.Label();
            this.textBoxCustomSearchUrl1 = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.labelCustomSearch = new System.Windows.Forms.Label();
            this.comboBoxCustomSearch1 = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.groupBoxVideoPlayerDefault = new System.Windows.Forms.GroupBox();
            this.numericUpDownMarginVertical = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelMarginVertical = new System.Windows.Forms.Label();
            this.panelMpvBackColor = new System.Windows.Forms.Panel();
            this.buttonMpvBackColor = new System.Windows.Forms.Button();
            this.panelMpvOutlineColor = new System.Windows.Forms.Panel();
            this.buttonMpvOutlineColor = new System.Windows.Forms.Button();
            this.panelMpvPrimaryColor = new System.Windows.Forms.Panel();
            this.buttonMpvPrimaryColor = new System.Windows.Forms.Button();
            this.groupBoxMpvBorder = new System.Windows.Forms.GroupBox();
            this.comboBoxOpaqueBoxStyle = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.radioButtonMpvOpaqueBox = new System.Windows.Forms.RadioButton();
            this.radioButtonMpvOutline = new System.Windows.Forms.RadioButton();
            this.numericUpDownMpvShadowWidth = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.numericUpDownMpvOutline = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelMpvShadow = new System.Windows.Forms.Label();
            this.checkBoxAllowVolumeBoost = new System.Windows.Forms.CheckBox();
            this.checkBoxVideoAutoOpen = new System.Windows.Forms.CheckBox();
            this.checkBoxVideoPlayerPreviewFontBold = new System.Windows.Forms.CheckBox();
            this.checkBoxVideoPlayerShowFullscreenButton = new System.Windows.Forms.CheckBox();
            this.checkBoxVideoPlayerShowMuteButton = new System.Windows.Forms.CheckBox();
            this.labelVideoPlayerPreviewFontName = new System.Windows.Forms.Label();
            this.comboBoxVideoPlayerPreviewFontName = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelVideoPlayerPreviewFontSize = new System.Windows.Forms.Label();
            this.comboBoxlVideoPlayerPreviewFontSize = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.checkBoxVideoPlayerShowStopButton = new System.Windows.Forms.CheckBox();
            this.groupBoxVideoEngine = new System.Windows.Forms.GroupBox();
            this.checkBoxMpvHandlesPreviewText = new System.Windows.Forms.CheckBox();
            this.labelMpvSettings = new System.Windows.Forms.Label();
            this.buttonMpvSettings = new System.Windows.Forms.Button();
            this.labelPlatform = new System.Windows.Forms.Label();
            this.buttonVlcPathBrowse = new System.Windows.Forms.Button();
            this.textBoxVlcPath = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.labelVlcPath = new System.Windows.Forms.Label();
            this.labelVideoPlayerVLC = new System.Windows.Forms.Label();
            this.radioButtonVideoPlayerVLC = new System.Windows.Forms.RadioButton();
            this.labelVideoPlayerMPlayer = new System.Windows.Forms.Label();
            this.labelDirectShowDescription = new System.Windows.Forms.Label();
            this.labelMpcHcDescription = new System.Windows.Forms.Label();
            this.radioButtonVideoPlayerMPV = new System.Windows.Forms.RadioButton();
            this.radioButtonVideoPlayerDirectShow = new System.Windows.Forms.RadioButton();
            this.radioButtonVideoPlayerMpcHc = new System.Windows.Forms.RadioButton();
            this.panelWaveform = new System.Windows.Forms.Panel();
            this.groupBoxFfmpeg = new System.Windows.Forms.GroupBox();
            this.checkBoxFfmpegUseCenterChannel = new System.Windows.Forms.CheckBox();
            this.buttonDownloadFfmpeg = new System.Windows.Forms.Button();
            this.buttonBrowseToFFmpeg = new System.Windows.Forms.Button();
            this.textBoxFFmpegPath = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.labelFFmpegPath = new System.Windows.Forms.Label();
            this.checkBoxUseFFmpeg = new System.Windows.Forms.CheckBox();
            this.groupBoxSpectrogram = new System.Windows.Forms.GroupBox();
            this.labelSpectrogramAppearance = new System.Windows.Forms.Label();
            this.comboBoxSpectrogramAppearance = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.checkBoxGenerateSpectrogram = new System.Windows.Forms.CheckBox();
            this.groupBoxSpectrogramClean = new System.Windows.Forms.GroupBox();
            this.buttonWaveformsFolderEmpty = new System.Windows.Forms.Button();
            this.labelWaveformsFolderInfo = new System.Windows.Forms.Label();
            this.groupBoxWaveformAppearence = new System.Windows.Forms.GroupBox();
            this.buttonEditShotChangesProfile = new System.Windows.Forms.Button();
            this.checkBoxWaveformAutoGen = new System.Windows.Forms.CheckBox();
            this.panelWaveformCursorColor = new System.Windows.Forms.Panel();
            this.buttonWaveformCursorColor = new System.Windows.Forms.Button();
            this.checkBoxWaveformSnapToShotChanges = new System.Windows.Forms.CheckBox();
            this.checkBoxWaveformSingleClickSelect = new System.Windows.Forms.CheckBox();
            this.checkBoxWaveformShowWpm = new System.Windows.Forms.CheckBox();
            this.checkBoxWaveformShowCps = new System.Windows.Forms.CheckBox();
            this.checkBoxWaveformSetVideoPosMoveStartEnd = new System.Windows.Forms.CheckBox();
            this.labelWaveformTextSize = new System.Windows.Forms.Label();
            this.checkBoxWaveformTextBold = new System.Windows.Forms.CheckBox();
            this.comboBoxWaveformTextSize = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.checkBoxListViewMouseEnterFocus = new System.Windows.Forms.CheckBox();
            this.checkBoxWaveformHoverFocus = new System.Windows.Forms.CheckBox();
            this.labelWaveformBorderHitMs2 = new System.Windows.Forms.Label();
            this.numericUpDownWaveformBorderHitMs = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelWaveformBorderHitMs1 = new System.Windows.Forms.Label();
            this.checkBoxAllowOverlap = new System.Windows.Forms.CheckBox();
            this.checkBoxReverseMouseWheelScrollDirection = new System.Windows.Forms.CheckBox();
            this.panelWaveformTextColor = new System.Windows.Forms.Panel();
            this.buttonWaveformTextColor = new System.Windows.Forms.Button();
            this.panelWaveformGridColor = new System.Windows.Forms.Panel();
            this.buttonWaveformGridColor = new System.Windows.Forms.Button();
            this.panelWaveformBackgroundColor = new System.Windows.Forms.Panel();
            this.buttonWaveformBackgroundColor = new System.Windows.Forms.Button();
            this.panelWaveformColor = new System.Windows.Forms.Panel();
            this.buttonWaveformColor = new System.Windows.Forms.Button();
            this.panelWaveformSelectedColor = new System.Windows.Forms.Panel();
            this.buttonWaveformSelectedColor = new System.Windows.Forms.Button();
            this.checkBoxWaveformShowGrid = new System.Windows.Forms.CheckBox();
            this.panelTools = new System.Windows.Forms.Panel();
            this.groupBoxToolsMisc = new System.Windows.Forms.GroupBox();
            this.comboBoxCustomToggleEnd = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.comboBoxCustomToggleStart = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelShortcutCustomToggle = new System.Windows.Forms.Label();
            this.checkBoxShortcutsAllowLetterOrNumberInTextBox = new System.Windows.Forms.CheckBox();
            this.comboBoxBDOpensIn = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelBDOpensIn = new System.Windows.Forms.Label();
            this.groupBoxToolsAutoBr = new System.Windows.Forms.GroupBox();
            this.labelToolsBreakBottomHeavyPercent = new System.Windows.Forms.Label();
            this.numericUpDownToolsBreakPreferBottomHeavy = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.checkBoxToolsBreakPreferBottomHeavy = new System.Windows.Forms.CheckBox();
            this.checkBoxToolsBreakByPixelWidth = new System.Windows.Forms.CheckBox();
            this.checkBoxToolsBreakEarlyLineEnding = new System.Windows.Forms.CheckBox();
            this.checkBoxToolsBreakEarlyComma = new System.Windows.Forms.CheckBox();
            this.checkBoxToolsBreakEarlyDash = new System.Windows.Forms.CheckBox();
            this.labelUserBingApiId = new System.Windows.Forms.Label();
            this.buttonEditDoNotBreakAfterList = new System.Windows.Forms.Button();
            this.checkBoxUseDoNotBreakAfterList = new System.Windows.Forms.CheckBox();
            this.groupBoxSpellCheck = new System.Windows.Forms.GroupBox();
            this.checkBoxSpellCheckAutoChangeNamesViaSuggestions = new System.Windows.Forms.CheckBox();
            this.checkBoxUseAlwaysToFile = new System.Windows.Forms.CheckBox();
            this.checkBoxTreatINQuoteAsING = new System.Windows.Forms.CheckBox();
            this.checkBoxSpellCheckOneLetterWords = new System.Windows.Forms.CheckBox();
            this.checkBoxSpellCheckAutoChangeNames = new System.Windows.Forms.CheckBox();
            this.groupBoxFixCommonErrors = new System.Windows.Forms.GroupBox();
            this.checkBoxUseWordSplitListAvoidPropercase = new System.Windows.Forms.CheckBox();
            this.checkBoxUseWordSplitList = new System.Windows.Forms.CheckBox();
            this.buttonFixContinuationStyleSettings = new System.Windows.Forms.Button();
            this.checkBoxFceSkipStep1 = new System.Windows.Forms.CheckBox();
            this.checkBoxFixShortDisplayTimesAllowMoveStartTime = new System.Windows.Forms.CheckBox();
            this.checkBoxFixCommonOcrErrorsUsingHardcodedRules = new System.Windows.Forms.CheckBox();
            this.comboBoxToolsMusicSymbol = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.textBoxMusicSymbolsToReplace = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.labelToolsMusicSymbolsToReplace = new System.Windows.Forms.Label();
            this.labelToolsMusicSymbol = new System.Windows.Forms.Label();
            this.groupBoxToolsVisualSync = new System.Windows.Forms.GroupBox();
            this.labelToolsEndScene = new System.Windows.Forms.Label();
            this.comboBoxToolsEndSceneIndex = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelToolsStartScene = new System.Windows.Forms.Label();
            this.comboBoxToolsStartSceneIndex = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.comboBoxToolsVerifySeconds = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelVerifyButton = new System.Windows.Forms.Label();
            this.groupBoxGoogleTranslate = new System.Windows.Forms.GroupBox();
            this.labelGoogleTranslateApiKey = new System.Windows.Forms.Label();
            this.textBoxGoogleTransleApiKey = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.linkLabelGoogleTranslateSignUp = new System.Windows.Forms.LinkLabel();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBoxBing = new System.Windows.Forms.GroupBox();
            this.comboBoxBoxBingTokenEndpoint = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelBingTokenEndpoint = new System.Windows.Forms.Label();
            this.labelBingApiKey = new System.Windows.Forms.Label();
            this.textBoxBingClientSecret = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.linkLabelBingSubscribe = new System.Windows.Forms.LinkLabel();
            this.label1 = new System.Windows.Forms.Label();
            this.panelToolBar = new System.Windows.Forms.Panel();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.checkBoxShowFrameRate = new System.Windows.Forms.CheckBox();
            this.groupBoxShowToolBarButtons = new System.Windows.Forms.GroupBox();
            this.labelTBOpenVideo = new System.Windows.Forms.Label();
            this.pictureBoxTBOpenVideo = new System.Windows.Forms.PictureBox();
            this.checkBoxTBOpenVideo = new System.Windows.Forms.CheckBox();
            this.pictureBoxWebVttStyle = new System.Windows.Forms.PictureBox();
            this.pictureBoxEbuProperties = new System.Windows.Forms.PictureBox();
            this.pictureBoxWebVttProperties = new System.Windows.Forms.PictureBox();
            this.pictureBoxIttProperties = new System.Windows.Forms.PictureBox();
            this.pictureBoxToggleVideo = new System.Windows.Forms.PictureBox();
            this.pictureBoxToggleWaveform = new System.Windows.Forms.PictureBox();
            this.pictureBoxAssaDraw = new System.Windows.Forms.PictureBox();
            this.pictureBoxAssAttachments = new System.Windows.Forms.PictureBox();
            this.pictureBoxAssProperties = new System.Windows.Forms.PictureBox();
            this.pictureBoxAssStyleManager = new System.Windows.Forms.PictureBox();
            this.labelTBToggleSourceView = new System.Windows.Forms.Label();
            this.pictureBoxTBToggleSourceView = new System.Windows.Forms.PictureBox();
            this.checkBoxTBToggleSourceView = new System.Windows.Forms.CheckBox();
            this.labelTBBurnIn = new System.Windows.Forms.Label();
            this.pictureBoxTBBurnIn = new System.Windows.Forms.PictureBox();
            this.checkBoxTBBurnIn = new System.Windows.Forms.CheckBox();
            this.labelTBBeautifyTimeCodes = new System.Windows.Forms.Label();
            this.pictureBoxTBBeautifyTimeCodes = new System.Windows.Forms.PictureBox();
            this.checkBoxTBBeautifyTimeCodes = new System.Windows.Forms.CheckBox();
            this.labelTBNetflixQualityCheck = new System.Windows.Forms.Label();
            this.pictureBoxTBNetflixQualityCheck = new System.Windows.Forms.PictureBox();
            this.checkBoxTBNetflixQualityCheck = new System.Windows.Forms.CheckBox();
            this.labelTBRemoveTextForHi = new System.Windows.Forms.Label();
            this.pictureBoxRemoveTextForHi = new System.Windows.Forms.PictureBox();
            this.checkBoxTBRemoveTextForHi = new System.Windows.Forms.CheckBox();
            this.labelTBFixCommonErrors = new System.Windows.Forms.Label();
            this.pictureBoxTBFixCommonErrors = new System.Windows.Forms.PictureBox();
            this.checkBoxTBFixCommonErrors = new System.Windows.Forms.CheckBox();
            this.labelTBHelp = new System.Windows.Forms.Label();
            this.pictureBoxTBHelp = new System.Windows.Forms.PictureBox();
            this.checkBoxTBHelp = new System.Windows.Forms.CheckBox();
            this.labelTBSettings = new System.Windows.Forms.Label();
            this.pictureBoxTBSettings = new System.Windows.Forms.PictureBox();
            this.checkBoxTBSettings = new System.Windows.Forms.CheckBox();
            this.labelTBSpellCheck = new System.Windows.Forms.Label();
            this.pictureBoxTBSpellCheck = new System.Windows.Forms.PictureBox();
            this.checkBoxTBSpellCheck = new System.Windows.Forms.CheckBox();
            this.labelTBVisualSync = new System.Windows.Forms.Label();
            this.pictureBoxTBVisualSync = new System.Windows.Forms.PictureBox();
            this.checkBoxTBVisualSync = new System.Windows.Forms.CheckBox();
            this.labelTBReplace = new System.Windows.Forms.Label();
            this.pictureBoxTBReplace = new System.Windows.Forms.PictureBox();
            this.checkBoxTBReplace = new System.Windows.Forms.CheckBox();
            this.labelTBFind = new System.Windows.Forms.Label();
            this.pictureBoxTBFind = new System.Windows.Forms.PictureBox();
            this.checkBoxTBFind = new System.Windows.Forms.CheckBox();
            this.labelTBSaveAs = new System.Windows.Forms.Label();
            this.pictureBoxTBSaveAs = new System.Windows.Forms.PictureBox();
            this.checkBoxTBSaveAs = new System.Windows.Forms.CheckBox();
            this.labelTBSave = new System.Windows.Forms.Label();
            this.pictureBoxTBSave = new System.Windows.Forms.PictureBox();
            this.checkBoxTBSave = new System.Windows.Forms.CheckBox();
            this.labelTBOpen = new System.Windows.Forms.Label();
            this.pictureBoxTBOpen = new System.Windows.Forms.PictureBox();
            this.checkBoxTBOpen = new System.Windows.Forms.CheckBox();
            this.labelTBNew = new System.Windows.Forms.Label();
            this.pictureBoxFileNew = new System.Windows.Forms.PictureBox();
            this.checkBoxToolbarNew = new System.Windows.Forms.CheckBox();
            this.panelFont = new System.Windows.Forms.Panel();
            this.groupBoxAppearance = new System.Windows.Forms.GroupBox();
            this.groupBoxGraphicsButtons = new System.Windows.Forms.GroupBox();
            this.pictureBoxPreview3 = new System.Windows.Forms.PictureBox();
            this.pictureBoxPreview2 = new System.Windows.Forms.PictureBox();
            this.pictureBoxPreview1 = new System.Windows.Forms.PictureBox();
            this.labelToolbarIconTheme = new System.Windows.Forms.Label();
            this.comboBoxToolbarIconTheme = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.groupBoxFontInUI = new System.Windows.Forms.GroupBox();
            this.groupBoxFontGeneral = new System.Windows.Forms.GroupBox();
            this.comboBoxSubtitleFont = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelSubtitleFont = new System.Windows.Forms.Label();
            this.panelSubtitleFontColor = new System.Windows.Forms.Panel();
            this.labelSubtitleFontColor = new System.Windows.Forms.Label();
            this.panelSubtitleBackgroundColor = new System.Windows.Forms.Panel();
            this.labelSubtitleFontBackgroundColor = new System.Windows.Forms.Label();
            this.groupBoxFontListViews = new System.Windows.Forms.GroupBox();
            this.labelSubtitleListViewFontSize = new System.Windows.Forms.Label();
            this.comboBoxSubtitleListViewFontSize = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.checkBoxSubtitleListViewFontBold = new System.Windows.Forms.CheckBox();
            this.groupBoxFontTextBox = new System.Windows.Forms.GroupBox();
            this.checkBoxLiveSpellCheck = new System.Windows.Forms.CheckBox();
            this.panelTextBoxAssColor = new System.Windows.Forms.Panel();
            this.buttonTextBoxAssColor = new System.Windows.Forms.Button();
            this.panelTextBoxHtmlColor = new System.Windows.Forms.Panel();
            this.buttonTextBoxHtmlColor = new System.Windows.Forms.Button();
            this.checkBoxSubtitleTextBoxSyntaxColor = new System.Windows.Forms.CheckBox();
            this.labelSubtitleFontSize = new System.Windows.Forms.Label();
            this.comboBoxSubtitleFontSize = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.checkBoxSubtitleFontBold = new System.Windows.Forms.CheckBox();
            this.checkBoxSubtitleCenter = new System.Windows.Forms.CheckBox();
            this.groupBoxDarkTheme = new System.Windows.Forms.GroupBox();
            this.checkBoxDarkThemeShowListViewGridLines = new System.Windows.Forms.CheckBox();
            this.checkBoxDarkThemeEnabled = new System.Windows.Forms.CheckBox();
            this.panelDarkThemeBackColor = new System.Windows.Forms.Panel();
            this.buttonDarkThemeBackColor = new System.Windows.Forms.Button();
            this.panelDarkThemeColor = new System.Windows.Forms.Panel();
            this.buttonDarkThemeColor = new System.Windows.Forms.Button();
            this.labelFontNote = new System.Windows.Forms.Label();
            this.panelNetwork = new System.Windows.Forms.Panel();
            this.groupBoxNetworkSession = new System.Windows.Forms.GroupBox();
            this.buttonNetworkSessionNewMessageSound = new System.Windows.Forms.Button();
            this.textBoxNetworkSessionNewMessageSound = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.labelNetworkSessionNewMessageSound = new System.Windows.Forms.Label();
            this.groupBoxProxySettings = new System.Windows.Forms.GroupBox();
            this.groupBoxProxyAuthentication = new System.Windows.Forms.GroupBox();
            this.labelProxyAuthType = new System.Windows.Forms.Label();
            this.checkBoxProxyUseDefaultCredentials = new System.Windows.Forms.CheckBox();
            this.comboBoxProxyAuthType = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.textBoxProxyDomain = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.labelProxyDomain = new System.Windows.Forms.Label();
            this.textBoxProxyUserName = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.labelProxyPassword = new System.Windows.Forms.Label();
            this.labelProxyUserName = new System.Windows.Forms.Label();
            this.textBoxProxyPassword = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.textBoxProxyAddress = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.labelProxyAddress = new System.Windows.Forms.Label();
            this.labelStatus = new System.Windows.Forms.Label();
            this.openFileDialogFFmpeg = new System.Windows.Forms.OpenFileDialog();
            this.buttonReset = new System.Windows.Forms.Button();
            this.toolTipContinuationPreview = new System.Windows.Forms.ToolTip(this.components);
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.panelFileTypeAssociations = new System.Windows.Forms.Panel();
            this.buttonUpdateFileTypeAssociations = new System.Windows.Forms.Button();
            this.listViewFileTypeAssociations = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.labelUpdateFileTypeAssociationsStatus = new System.Windows.Forms.Label();
            this.imageListFileTypeAssociations = new System.Windows.Forms.ImageList(this.components);
            this.toolTipDialogStylePreview = new System.Windows.Forms.ToolTip(this.components);
            this.panelAutoTranslate = new System.Windows.Forms.Panel();
            this.groupBoxAutoTranslatePapago = new System.Windows.Forms.GroupBox();
            this.nikseTextBoxPapagoClientId = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.nikseTextBoxPapagoClientSecret = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.labelSecretPapago = new System.Windows.Forms.Label();
            this.labelApiKeyPapago = new System.Windows.Forms.Label();
            this.linkLabelPapago = new System.Windows.Forms.LinkLabel();
            this.label11 = new System.Windows.Forms.Label();
            this.groupBoxAutoTranslateChatGpt = new System.Windows.Forms.GroupBox();
            this.nikseTextBoxChatGptUrl = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.nikseComboBoxChatGptModel = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelChatGptModel = new System.Windows.Forms.Label();
            this.nikseTextBoxChatGptApiKey = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.labelApiKeyChatGpt = new System.Windows.Forms.Label();
            this.labelUrlChatGpt = new System.Windows.Forms.Label();
            this.linkLabelMoreInfoChatGpt = new System.Windows.Forms.LinkLabel();
            this.label10 = new System.Windows.Forms.Label();
            this.groupBoxDeepL = new System.Windows.Forms.GroupBox();
            this.nikseTextBoxDeepLUrl = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.nikseTextBoxDeepLApiKey = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.labelDeepLApiKey = new System.Windows.Forms.Label();
            this.labelDeepLUrl = new System.Windows.Forms.Label();
            this.linkLabelMoreInfoDeepl = new System.Windows.Forms.LinkLabel();
            this.label9 = new System.Windows.Forms.Label();
            this.groupBoxMyMemory = new System.Windows.Forms.GroupBox();
            this.nikseTextBoxMyMemoryApiKey = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.labelMyMemoryApiKey = new System.Windows.Forms.Label();
            this.linkLabelMyMemoryTranslate = new System.Windows.Forms.LinkLabel();
            this.label8 = new System.Windows.Forms.Label();
            this.groupBoxLibreTranslate = new System.Windows.Forms.GroupBox();
            this.nikseTextBoxLibreTranslateApiKey = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.labelLibreApiKey = new System.Windows.Forms.Label();
            this.nikseTextBoxLibreTranslateUrl = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.labelLibreUrl = new System.Windows.Forms.Label();
            this.linkLabelLibreTranslateApi = new System.Windows.Forms.LinkLabel();
            this.label7 = new System.Windows.Forms.Label();
            this.groupBoxNllbServe = new System.Windows.Forms.GroupBox();
            this.nikseTextBoxNllbServeUrl = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.nikseTextBoxNllbServeModel = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.labelNllbServeModel = new System.Windows.Forms.Label();
            this.labelNllbServeUrl = new System.Windows.Forms.Label();
            this.linkLabelNllbServe = new System.Windows.Forms.LinkLabel();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBoxNllbApi = new System.Windows.Forms.GroupBox();
            this.nikseTextBoxNllbApiUrl = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.labelNllbApiUrl = new System.Windows.Forms.Label();
            this.linkLabelNllbApi = new System.Windows.Forms.LinkLabel();
            this.label5 = new System.Windows.Forms.Label();
            this.panelGeneral.SuspendLayout();
            this.groupBoxMiscellaneous.SuspendLayout();
            this.groupBoxGeneralRules.SuspendLayout();
            this.panelSubtitleFormats.SuspendLayout();
            this.groupBoxSubtitleFormats.SuspendLayout();
            this.groupBoxFavoriteSubtitleFormats.SuspendLayout();
            this.contextMenuStripFavoriteFormats.SuspendLayout();
            this.panelShortcuts.SuspendLayout();
            this.groupBoxShortcuts.SuspendLayout();
            this.contextMenuStripShortcuts.SuspendLayout();
            this.panelSyntaxColoring.SuspendLayout();
            this.groupBoxListViewSyntaxColoring.SuspendLayout();
            this.panelVideoPlayer.SuspendLayout();
            this.groupBoxMainWindowVideoControls.SuspendLayout();
            this.groupBoxVideoPlayerDefault.SuspendLayout();
            this.groupBoxMpvBorder.SuspendLayout();
            this.groupBoxVideoEngine.SuspendLayout();
            this.panelWaveform.SuspendLayout();
            this.groupBoxFfmpeg.SuspendLayout();
            this.groupBoxSpectrogram.SuspendLayout();
            this.groupBoxSpectrogramClean.SuspendLayout();
            this.groupBoxWaveformAppearence.SuspendLayout();
            this.panelTools.SuspendLayout();
            this.groupBoxToolsMisc.SuspendLayout();
            this.groupBoxToolsAutoBr.SuspendLayout();
            this.groupBoxSpellCheck.SuspendLayout();
            this.groupBoxFixCommonErrors.SuspendLayout();
            this.groupBoxToolsVisualSync.SuspendLayout();
            this.groupBoxGoogleTranslate.SuspendLayout();
            this.groupBoxBing.SuspendLayout();
            this.panelToolBar.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBoxShowToolBarButtons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTBOpenVideo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxWebVttStyle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxEbuProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxWebVttProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxIttProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxToggleVideo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxToggleWaveform)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxAssaDraw)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxAssAttachments)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxAssProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxAssStyleManager)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTBToggleSourceView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTBBurnIn)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTBBeautifyTimeCodes)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTBNetflixQualityCheck)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxRemoveTextForHi)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTBFixCommonErrors)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTBHelp)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTBSettings)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTBSpellCheck)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTBVisualSync)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTBReplace)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTBFind)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTBSaveAs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTBSave)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTBOpen)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxFileNew)).BeginInit();
            this.panelFont.SuspendLayout();
            this.groupBoxAppearance.SuspendLayout();
            this.groupBoxGraphicsButtons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreview3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreview2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreview1)).BeginInit();
            this.groupBoxFontInUI.SuspendLayout();
            this.groupBoxFontGeneral.SuspendLayout();
            this.groupBoxFontListViews.SuspendLayout();
            this.groupBoxFontTextBox.SuspendLayout();
            this.groupBoxDarkTheme.SuspendLayout();
            this.panelNetwork.SuspendLayout();
            this.groupBoxNetworkSession.SuspendLayout();
            this.groupBoxProxySettings.SuspendLayout();
            this.groupBoxProxyAuthentication.SuspendLayout();
            this.panelFileTypeAssociations.SuspendLayout();
            this.panelAutoTranslate.SuspendLayout();
            this.groupBoxAutoTranslatePapago.SuspendLayout();
            this.groupBoxAutoTranslateChatGpt.SuspendLayout();
            this.groupBoxDeepL.SuspendLayout();
            this.groupBoxMyMemory.SuspendLayout();
            this.groupBoxLibreTranslate.SuspendLayout();
            this.groupBoxNllbServe.SuspendLayout();
            this.groupBoxNllbApi.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(733, 539);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 13;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.ButtonOkClick);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(814, 539);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 14;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // listBoxSection
            // 
            this.listBoxSection.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listBoxSection.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listBoxSection.FormattingEnabled = true;
            this.listBoxSection.ItemHeight = 16;
            this.listBoxSection.Location = new System.Drawing.Point(10, 10);
            this.listBoxSection.Name = "listBoxSection";
            this.listBoxSection.SelectedIndex = -1;
            this.listBoxSection.SelectedItem = null;
            this.listBoxSection.SelectionMode = System.Windows.Forms.SelectionMode.One;
            this.listBoxSection.Size = new System.Drawing.Size(214, 516);
            this.listBoxSection.Sorted = false;
            this.listBoxSection.TabIndex = 0;
            this.listBoxSection.TopIndex = 0;
            this.listBoxSection.SelectedIndexChanged += new System.EventHandler(this.ListBoxSectionSelectedIndexChanged);
            this.listBoxSection.LostFocus += new System.EventHandler(this.listBoxSection_LostFocus);
            // 
            // panelGeneral
            // 
            this.panelGeneral.Controls.Add(this.groupBoxMiscellaneous);
            this.panelGeneral.Location = new System.Drawing.Point(230, 6);
            this.panelGeneral.Name = "panelGeneral";
            this.panelGeneral.Padding = new System.Windows.Forms.Padding(3);
            this.panelGeneral.Size = new System.Drawing.Size(864, 521);
            this.panelGeneral.TabIndex = 1;
            this.panelGeneral.Text = "General";
            // 
            // groupBoxMiscellaneous
            // 
            this.groupBoxMiscellaneous.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxMiscellaneous.Controls.Add(this.labelDefaultLanguagesList);
            this.groupBoxMiscellaneous.Controls.Add(this.buttonDefaultLanguages);
            this.groupBoxMiscellaneous.Controls.Add(this.labelDefaultLanguages);
            this.groupBoxMiscellaneous.Controls.Add(this.buttonTranslationAutoSuffix);
            this.groupBoxMiscellaneous.Controls.Add(this.comboBoxTranslationAutoSuffix);
            this.groupBoxMiscellaneous.Controls.Add(this.labelTranslationAutoSuffix);
            this.groupBoxMiscellaneous.Controls.Add(this.labelSplitBehavior);
            this.groupBoxMiscellaneous.Controls.Add(this.comboBoxSplitBehavior);
            this.groupBoxMiscellaneous.Controls.Add(this.checkBoxAutoSave);
            this.groupBoxMiscellaneous.Controls.Add(this.comboBoxSaveAsFileNameFrom);
            this.groupBoxMiscellaneous.Controls.Add(this.labelSaveAsFileNameFrom);
            this.groupBoxMiscellaneous.Controls.Add(this.groupBoxGeneralRules);
            this.groupBoxMiscellaneous.Controls.Add(this.comboBoxAutoBackupDeleteAfter);
            this.groupBoxMiscellaneous.Controls.Add(this.labelAutoBackupDeleteAfter);
            this.groupBoxMiscellaneous.Controls.Add(this.checkBoxCheckForUpdates);
            this.groupBoxMiscellaneous.Controls.Add(this.labelSpellChecker);
            this.groupBoxMiscellaneous.Controls.Add(this.comboBoxTimeCodeMode);
            this.groupBoxMiscellaneous.Controls.Add(this.labelTimeCodeMode);
            this.groupBoxMiscellaneous.Controls.Add(this.comboBoxEncoding);
            this.groupBoxMiscellaneous.Controls.Add(this.checkBoxAutoDetectAnsiEncoding);
            this.groupBoxMiscellaneous.Controls.Add(this.textBoxShowLineBreaksAs);
            this.groupBoxMiscellaneous.Controls.Add(this.checkBoxAutoWrapWhileTyping);
            this.groupBoxMiscellaneous.Controls.Add(this.checkBoxPromptDeleteLines);
            this.groupBoxMiscellaneous.Controls.Add(this.checkBoxAllowEditOfOriginalSubtitle);
            this.groupBoxMiscellaneous.Controls.Add(this.comboBoxSpellChecker);
            this.groupBoxMiscellaneous.Controls.Add(this.comboBoxAutoBackup);
            this.groupBoxMiscellaneous.Controls.Add(this.labelAutoBackup);
            this.groupBoxMiscellaneous.Controls.Add(this.checkBoxRememberSelectedLine);
            this.groupBoxMiscellaneous.Controls.Add(this.checkBoxRemoveBlankLinesWhenOpening);
            this.groupBoxMiscellaneous.Controls.Add(this.labelAutoDetectAnsiEncoding);
            this.groupBoxMiscellaneous.Controls.Add(this.comboBoxListViewDoubleClickEvent);
            this.groupBoxMiscellaneous.Controls.Add(this.labelListViewDoubleClickEvent);
            this.groupBoxMiscellaneous.Controls.Add(this.labelShowLineBreaksAs);
            this.groupBoxMiscellaneous.Controls.Add(this.checkBoxRememberWindowPosition);
            this.groupBoxMiscellaneous.Controls.Add(this.checkBoxStartInSourceView);
            this.groupBoxMiscellaneous.Controls.Add(this.checkBoxReopenLastOpened);
            this.groupBoxMiscellaneous.Controls.Add(this.checkBoxRememberRecentFiles);
            this.groupBoxMiscellaneous.Controls.Add(this.labelDefaultFileEncoding);
            this.groupBoxMiscellaneous.Controls.Add(this.comboBoxFrameRate);
            this.groupBoxMiscellaneous.Controls.Add(this.labelDefaultFrameRate);
            this.groupBoxMiscellaneous.Location = new System.Drawing.Point(0, 0);
            this.groupBoxMiscellaneous.Name = "groupBoxMiscellaneous";
            this.groupBoxMiscellaneous.Size = new System.Drawing.Size(851, 522);
            this.groupBoxMiscellaneous.TabIndex = 0;
            this.groupBoxMiscellaneous.TabStop = false;
            this.groupBoxMiscellaneous.Text = "Miscellaneous";
            // 
            // labelDefaultLanguagesList
            // 
            this.labelDefaultLanguagesList.AutoSize = true;
            this.labelDefaultLanguagesList.Location = new System.Drawing.Point(241, 495);
            this.labelDefaultLanguagesList.Name = "labelDefaultLanguagesList";
            this.labelDefaultLanguagesList.Size = new System.Drawing.Size(38, 13);
            this.labelDefaultLanguagesList.TabIndex = 67;
            this.labelDefaultLanguagesList.Text = "da, en";
            // 
            // buttonDefaultLanguages
            // 
            this.buttonDefaultLanguages.Location = new System.Drawing.Point(205, 489);
            this.buttonDefaultLanguages.Name = "buttonDefaultLanguages";
            this.buttonDefaultLanguages.Size = new System.Drawing.Size(28, 23);
            this.buttonDefaultLanguages.TabIndex = 66;
            this.buttonDefaultLanguages.Text = "...";
            this.buttonDefaultLanguages.UseVisualStyleBackColor = true;
            this.buttonDefaultLanguages.Click += new System.EventHandler(this.buttonDefaultLanguages_Click);
            // 
            // labelDefaultLanguages
            // 
            this.labelDefaultLanguages.AutoSize = true;
            this.labelDefaultLanguages.Location = new System.Drawing.Point(8, 491);
            this.labelDefaultLanguages.Name = "labelDefaultLanguages";
            this.labelDefaultLanguages.Size = new System.Drawing.Size(94, 13);
            this.labelDefaultLanguages.TabIndex = 33;
            this.labelDefaultLanguages.Text = "Default languages";
            // 
            // buttonTranslationAutoSuffix
            // 
            this.buttonTranslationAutoSuffix.Location = new System.Drawing.Point(670, 420);
            this.buttonTranslationAutoSuffix.Name = "buttonTranslationAutoSuffix";
            this.buttonTranslationAutoSuffix.Size = new System.Drawing.Size(28, 23);
            this.buttonTranslationAutoSuffix.TabIndex = 26;
            this.buttonTranslationAutoSuffix.Text = "...";
            this.buttonTranslationAutoSuffix.UseVisualStyleBackColor = true;
            this.buttonTranslationAutoSuffix.Click += new System.EventHandler(this.buttonTranslationAutoSuffix_Click);
            // 
            // comboBoxTranslationAutoSuffix
            // 
            this.comboBoxTranslationAutoSuffix.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxTranslationAutoSuffix.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxTranslationAutoSuffix.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxTranslationAutoSuffix.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxTranslationAutoSuffix.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxTranslationAutoSuffix.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxTranslationAutoSuffix.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxTranslationAutoSuffix.DropDownHeight = 400;
            this.comboBoxTranslationAutoSuffix.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxTranslationAutoSuffix.DropDownWidth = 121;
            this.comboBoxTranslationAutoSuffix.FormattingEnabled = true;
            this.comboBoxTranslationAutoSuffix.Items.AddRange(new string[] {
            "None",
            "Every minute",
            "Every 5 minutes",
            "Every 15 minutes"});
            this.comboBoxTranslationAutoSuffix.Location = new System.Drawing.Point(540, 421);
            this.comboBoxTranslationAutoSuffix.MaxLength = 32767;
            this.comboBoxTranslationAutoSuffix.Name = "comboBoxTranslationAutoSuffix";
            this.comboBoxTranslationAutoSuffix.SelectedIndex = -1;
            this.comboBoxTranslationAutoSuffix.SelectedItem = null;
            this.comboBoxTranslationAutoSuffix.SelectedText = "";
            this.comboBoxTranslationAutoSuffix.Size = new System.Drawing.Size(121, 21);
            this.comboBoxTranslationAutoSuffix.TabIndex = 25;
            this.comboBoxTranslationAutoSuffix.UsePopupWindow = false;
            // 
            // labelTranslationAutoSuffix
            // 
            this.labelTranslationAutoSuffix.AutoSize = true;
            this.labelTranslationAutoSuffix.Location = new System.Drawing.Point(416, 424);
            this.labelTranslationAutoSuffix.Name = "labelTranslationAutoSuffix";
            this.labelTranslationAutoSuffix.Size = new System.Drawing.Size(115, 13);
            this.labelTranslationAutoSuffix.TabIndex = 32;
            this.labelTranslationAutoSuffix.Text = "Translation auto suffix";
            // 
            // labelSplitBehavior
            // 
            this.labelSplitBehavior.AutoSize = true;
            this.labelSplitBehavior.Location = new System.Drawing.Point(416, 291);
            this.labelSplitBehavior.Name = "labelSplitBehavior";
            this.labelSplitBehavior.Size = new System.Drawing.Size(72, 13);
            this.labelSplitBehavior.TabIndex = 20;
            this.labelSplitBehavior.Text = "Split behavior";
            // 
            // comboBoxSplitBehavior
            // 
            this.comboBoxSplitBehavior.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxSplitBehavior.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxSplitBehavior.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxSplitBehavior.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxSplitBehavior.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxSplitBehavior.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxSplitBehavior.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxSplitBehavior.DropDownHeight = 400;
            this.comboBoxSplitBehavior.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSplitBehavior.DropDownWidth = 207;
            this.comboBoxSplitBehavior.FormattingEnabled = true;
            this.comboBoxSplitBehavior.Location = new System.Drawing.Point(506, 288);
            this.comboBoxSplitBehavior.MaxLength = 32767;
            this.comboBoxSplitBehavior.Name = "comboBoxSplitBehavior";
            this.comboBoxSplitBehavior.SelectedIndex = -1;
            this.comboBoxSplitBehavior.SelectedItem = null;
            this.comboBoxSplitBehavior.SelectedText = "";
            this.comboBoxSplitBehavior.Size = new System.Drawing.Size(207, 21);
            this.comboBoxSplitBehavior.TabIndex = 21;
            this.comboBoxSplitBehavior.UsePopupWindow = false;
            // 
            // checkBoxAutoSave
            // 
            this.checkBoxAutoSave.AutoSize = true;
            this.checkBoxAutoSave.Location = new System.Drawing.Point(421, 491);
            this.checkBoxAutoSave.Name = "checkBoxAutoSave";
            this.checkBoxAutoSave.Size = new System.Drawing.Size(75, 17);
            this.checkBoxAutoSave.TabIndex = 30;
            this.checkBoxAutoSave.Text = "Auto save";
            this.checkBoxAutoSave.UseVisualStyleBackColor = true;
            // 
            // comboBoxSaveAsFileNameFrom
            // 
            this.comboBoxSaveAsFileNameFrom.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxSaveAsFileNameFrom.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxSaveAsFileNameFrom.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxSaveAsFileNameFrom.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxSaveAsFileNameFrom.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxSaveAsFileNameFrom.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxSaveAsFileNameFrom.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxSaveAsFileNameFrom.DropDownHeight = 400;
            this.comboBoxSaveAsFileNameFrom.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSaveAsFileNameFrom.DropDownWidth = 332;
            this.comboBoxSaveAsFileNameFrom.FormattingEnabled = true;
            this.comboBoxSaveAsFileNameFrom.Location = new System.Drawing.Point(419, 386);
            this.comboBoxSaveAsFileNameFrom.MaxLength = 32767;
            this.comboBoxSaveAsFileNameFrom.Name = "comboBoxSaveAsFileNameFrom";
            this.comboBoxSaveAsFileNameFrom.SelectedIndex = -1;
            this.comboBoxSaveAsFileNameFrom.SelectedItem = null;
            this.comboBoxSaveAsFileNameFrom.SelectedText = "";
            this.comboBoxSaveAsFileNameFrom.Size = new System.Drawing.Size(332, 21);
            this.comboBoxSaveAsFileNameFrom.TabIndex = 24;
            this.comboBoxSaveAsFileNameFrom.UsePopupWindow = false;
            // 
            // labelSaveAsFileNameFrom
            // 
            this.labelSaveAsFileNameFrom.AutoSize = true;
            this.labelSaveAsFileNameFrom.Location = new System.Drawing.Point(416, 370);
            this.labelSaveAsFileNameFrom.Name = "labelSaveAsFileNameFrom";
            this.labelSaveAsFileNameFrom.Size = new System.Drawing.Size(160, 13);
            this.labelSaveAsFileNameFrom.TabIndex = 24;
            this.labelSaveAsFileNameFrom.Text = "\"Save as...\" will take name from";
            // 
            // groupBoxGeneralRules
            // 
            this.groupBoxGeneralRules.Controls.Add(this.buttonEditCustomContinuationStyle);
            this.groupBoxGeneralRules.Controls.Add(this.comboBoxCpsLineLenCalc);
            this.groupBoxGeneralRules.Controls.Add(this.labelCpsLineLenCalc);
            this.groupBoxGeneralRules.Controls.Add(this.buttonGapChoose);
            this.groupBoxGeneralRules.Controls.Add(this.comboBoxContinuationStyle);
            this.groupBoxGeneralRules.Controls.Add(this.labelContinuationStyle);
            this.groupBoxGeneralRules.Controls.Add(this.labelDialogStyle);
            this.groupBoxGeneralRules.Controls.Add(this.comboBoxDialogStyle);
            this.groupBoxGeneralRules.Controls.Add(this.buttonEditProfile);
            this.groupBoxGeneralRules.Controls.Add(this.comboBoxRulesProfileName);
            this.groupBoxGeneralRules.Controls.Add(this.labelRulesProfileName);
            this.groupBoxGeneralRules.Controls.Add(this.labelOptimalCharsPerSecond);
            this.groupBoxGeneralRules.Controls.Add(this.numericUpDownOptimalCharsSec);
            this.groupBoxGeneralRules.Controls.Add(this.labelSubMaxLen);
            this.groupBoxGeneralRules.Controls.Add(this.numericUpDownMaxWordsMin);
            this.groupBoxGeneralRules.Controls.Add(this.labelMergeShortLines);
            this.groupBoxGeneralRules.Controls.Add(this.labelMaxWordsPerMin);
            this.groupBoxGeneralRules.Controls.Add(this.labelMinDuration);
            this.groupBoxGeneralRules.Controls.Add(this.numericUpDownMaxNumberOfLines);
            this.groupBoxGeneralRules.Controls.Add(this.labelMaxDuration);
            this.groupBoxGeneralRules.Controls.Add(this.labelMaxLines);
            this.groupBoxGeneralRules.Controls.Add(this.numericUpDownDurationMin);
            this.groupBoxGeneralRules.Controls.Add(this.numericUpDownDurationMax);
            this.groupBoxGeneralRules.Controls.Add(this.comboBoxMergeShortLineLength);
            this.groupBoxGeneralRules.Controls.Add(this.labelMaxCharsPerSecond);
            this.groupBoxGeneralRules.Controls.Add(this.numericUpDownMinGapMs);
            this.groupBoxGeneralRules.Controls.Add(this.numericUpDownMaxCharsSec);
            this.groupBoxGeneralRules.Controls.Add(this.labelMinGapMs);
            this.groupBoxGeneralRules.Controls.Add(this.numericUpDownSubtitleLineMaximumLength);
            this.groupBoxGeneralRules.Location = new System.Drawing.Point(6, 23);
            this.groupBoxGeneralRules.Name = "groupBoxGeneralRules";
            this.groupBoxGeneralRules.Size = new System.Drawing.Size(384, 380);
            this.groupBoxGeneralRules.TabIndex = 0;
            this.groupBoxGeneralRules.TabStop = false;
            this.groupBoxGeneralRules.Text = "Rules";
            // 
            // buttonEditCustomContinuationStyle
            // 
            this.buttonEditCustomContinuationStyle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonEditCustomContinuationStyle.Location = new System.Drawing.Point(347, 323);
            this.buttonEditCustomContinuationStyle.Name = "buttonEditCustomContinuationStyle";
            this.buttonEditCustomContinuationStyle.Size = new System.Drawing.Size(28, 23);
            this.buttonEditCustomContinuationStyle.TabIndex = 59;
            this.buttonEditCustomContinuationStyle.Text = "...";
            this.buttonEditCustomContinuationStyle.UseVisualStyleBackColor = true;
            this.buttonEditCustomContinuationStyle.Visible = false;
            this.buttonEditCustomContinuationStyle.Click += new System.EventHandler(this.buttonEditCustomContinuationStyle_Click);
            // 
            // comboBoxCpsLineLenCalc
            // 
            this.comboBoxCpsLineLenCalc.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxCpsLineLenCalc.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxCpsLineLenCalc.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxCpsLineLenCalc.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxCpsLineLenCalc.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxCpsLineLenCalc.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxCpsLineLenCalc.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxCpsLineLenCalc.DropDownHeight = 400;
            this.comboBoxCpsLineLenCalc.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxCpsLineLenCalc.DropDownWidth = 220;
            this.comboBoxCpsLineLenCalc.FormattingEnabled = true;
            this.comboBoxCpsLineLenCalc.Location = new System.Drawing.Point(203, 351);
            this.comboBoxCpsLineLenCalc.MaxLength = 32767;
            this.comboBoxCpsLineLenCalc.Name = "comboBoxCpsLineLenCalc";
            this.comboBoxCpsLineLenCalc.SelectedIndex = -1;
            this.comboBoxCpsLineLenCalc.SelectedItem = null;
            this.comboBoxCpsLineLenCalc.SelectedText = "";
            this.comboBoxCpsLineLenCalc.Size = new System.Drawing.Size(170, 21);
            this.comboBoxCpsLineLenCalc.TabIndex = 65;
            this.comboBoxCpsLineLenCalc.UsePopupWindow = false;
            this.comboBoxCpsLineLenCalc.SelectedIndexChanged += new System.EventHandler(this.ProfileUiValueChanged);
            // 
            // labelCpsLineLenCalc
            // 
            this.labelCpsLineLenCalc.AutoSize = true;
            this.labelCpsLineLenCalc.Location = new System.Drawing.Point(6, 354);
            this.labelCpsLineLenCalc.Name = "labelCpsLineLenCalc";
            this.labelCpsLineLenCalc.Size = new System.Drawing.Size(104, 13);
            this.labelCpsLineLenCalc.TabIndex = 64;
            this.labelCpsLineLenCalc.Text = "Cps/line length style";
            // 
            // buttonGapChoose
            // 
            this.buttonGapChoose.Location = new System.Drawing.Point(265, 214);
            this.buttonGapChoose.Name = "buttonGapChoose";
            this.buttonGapChoose.Size = new System.Drawing.Size(28, 23);
            this.buttonGapChoose.TabIndex = 46;
            this.buttonGapChoose.Text = "...";
            this.buttonGapChoose.UseVisualStyleBackColor = true;
            this.buttonGapChoose.Click += new System.EventHandler(this.buttonGapChoose_Click);
            // 
            // comboBoxContinuationStyle
            // 
            this.comboBoxContinuationStyle.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxContinuationStyle.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxContinuationStyle.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxContinuationStyle.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxContinuationStyle.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxContinuationStyle.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxContinuationStyle.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxContinuationStyle.DropDownHeight = 400;
            this.comboBoxContinuationStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxContinuationStyle.DropDownWidth = 170;
            this.comboBoxContinuationStyle.FormattingEnabled = true;
            this.comboBoxContinuationStyle.Location = new System.Drawing.Point(203, 324);
            this.comboBoxContinuationStyle.MaxLength = 32767;
            this.comboBoxContinuationStyle.Name = "comboBoxContinuationStyle";
            this.comboBoxContinuationStyle.SelectedIndex = -1;
            this.comboBoxContinuationStyle.SelectedItem = null;
            this.comboBoxContinuationStyle.SelectedText = "";
            this.comboBoxContinuationStyle.Size = new System.Drawing.Size(170, 21);
            this.comboBoxContinuationStyle.TabIndex = 58;
            this.comboBoxContinuationStyle.UsePopupWindow = false;
            this.comboBoxContinuationStyle.SelectedIndexChanged += new System.EventHandler(this.ProfileUiValueChanged);
            // 
            // labelContinuationStyle
            // 
            this.labelContinuationStyle.AutoSize = true;
            this.labelContinuationStyle.Location = new System.Drawing.Point(6, 327);
            this.labelContinuationStyle.Name = "labelContinuationStyle";
            this.labelContinuationStyle.Size = new System.Drawing.Size(140, 13);
            this.labelContinuationStyle.TabIndex = 63;
            this.labelContinuationStyle.Text = "Sentence continuation style";
            // 
            // labelDialogStyle
            // 
            this.labelDialogStyle.AutoSize = true;
            this.labelDialogStyle.Location = new System.Drawing.Point(6, 300);
            this.labelDialogStyle.Name = "labelDialogStyle";
            this.labelDialogStyle.Size = new System.Drawing.Size(62, 13);
            this.labelDialogStyle.TabIndex = 61;
            this.labelDialogStyle.Text = "Dialog style";
            // 
            // comboBoxDialogStyle
            // 
            this.comboBoxDialogStyle.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxDialogStyle.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxDialogStyle.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxDialogStyle.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxDialogStyle.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxDialogStyle.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxDialogStyle.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxDialogStyle.DropDownHeight = 400;
            this.comboBoxDialogStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDialogStyle.DropDownWidth = 170;
            this.comboBoxDialogStyle.FormattingEnabled = true;
            this.comboBoxDialogStyle.Items.AddRange(new string[] {
            "Dash both lines with space",
            "Dash both lines without space",
            "Dash second line with space",
            "Dash second line without space"});
            this.comboBoxDialogStyle.Location = new System.Drawing.Point(203, 297);
            this.comboBoxDialogStyle.MaxLength = 32767;
            this.comboBoxDialogStyle.Name = "comboBoxDialogStyle";
            this.comboBoxDialogStyle.SelectedIndex = -1;
            this.comboBoxDialogStyle.SelectedItem = null;
            this.comboBoxDialogStyle.SelectedText = "";
            this.comboBoxDialogStyle.Size = new System.Drawing.Size(170, 21);
            this.comboBoxDialogStyle.TabIndex = 57;
            this.comboBoxDialogStyle.UsePopupWindow = false;
            this.comboBoxDialogStyle.SelectedIndexChanged += new System.EventHandler(this.ProfileUiValueChanged);
            // 
            // buttonEditProfile
            // 
            this.buttonEditProfile.Location = new System.Drawing.Point(322, 19);
            this.buttonEditProfile.Name = "buttonEditProfile";
            this.buttonEditProfile.Size = new System.Drawing.Size(28, 23);
            this.buttonEditProfile.TabIndex = 10;
            this.buttonEditProfile.Text = "...";
            this.buttonEditProfile.UseVisualStyleBackColor = true;
            this.buttonEditProfile.Click += new System.EventHandler(this.buttonEditProfile_Click);
            // 
            // comboBoxRulesProfileName
            // 
            this.comboBoxRulesProfileName.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxRulesProfileName.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxRulesProfileName.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxRulesProfileName.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxRulesProfileName.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxRulesProfileName.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxRulesProfileName.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxRulesProfileName.DropDownHeight = 420;
            this.comboBoxRulesProfileName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxRulesProfileName.DropDownWidth = 238;
            this.comboBoxRulesProfileName.FormattingEnabled = true;
            this.comboBoxRulesProfileName.Location = new System.Drawing.Point(78, 20);
            this.comboBoxRulesProfileName.MaxLength = 32767;
            this.comboBoxRulesProfileName.Name = "comboBoxRulesProfileName";
            this.comboBoxRulesProfileName.SelectedIndex = -1;
            this.comboBoxRulesProfileName.SelectedItem = null;
            this.comboBoxRulesProfileName.SelectedText = "";
            this.comboBoxRulesProfileName.Size = new System.Drawing.Size(238, 21);
            this.comboBoxRulesProfileName.TabIndex = 5;
            this.comboBoxRulesProfileName.UsePopupWindow = false;
            this.comboBoxRulesProfileName.SelectedIndexChanged += new System.EventHandler(this.comboBoxRulesProfileName_SelectedIndexChanged);
            // 
            // labelRulesProfileName
            // 
            this.labelRulesProfileName.AutoSize = true;
            this.labelRulesProfileName.Location = new System.Drawing.Point(6, 24);
            this.labelRulesProfileName.Name = "labelRulesProfileName";
            this.labelRulesProfileName.Size = new System.Drawing.Size(37, 13);
            this.labelRulesProfileName.TabIndex = 50;
            this.labelRulesProfileName.Text = "Profile";
            // 
            // labelOptimalCharsPerSecond
            // 
            this.labelOptimalCharsPerSecond.AutoSize = true;
            this.labelOptimalCharsPerSecond.Location = new System.Drawing.Point(6, 84);
            this.labelOptimalCharsPerSecond.Name = "labelOptimalCharsPerSecond";
            this.labelOptimalCharsPerSecond.Size = new System.Drawing.Size(92, 13);
            this.labelOptimalCharsPerSecond.TabIndex = 8;
            this.labelOptimalCharsPerSecond.Text = "Optimal chars/sec";
            // 
            // numericUpDownOptimalCharsSec
            // 
            this.numericUpDownOptimalCharsSec.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownOptimalCharsSec.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownOptimalCharsSec.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownOptimalCharsSec.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownOptimalCharsSec.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownOptimalCharsSec.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownOptimalCharsSec.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownOptimalCharsSec.DecimalPlaces = 1;
            this.numericUpDownOptimalCharsSec.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericUpDownOptimalCharsSec.Location = new System.Drawing.Point(203, 82);
            this.numericUpDownOptimalCharsSec.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDownOptimalCharsSec.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.numericUpDownOptimalCharsSec.Name = "numericUpDownOptimalCharsSec";
            this.numericUpDownOptimalCharsSec.Size = new System.Drawing.Size(56, 21);
            this.numericUpDownOptimalCharsSec.TabIndex = 20;
            this.numericUpDownOptimalCharsSec.TabStop = false;
            this.numericUpDownOptimalCharsSec.ThousandsSeparator = false;
            this.numericUpDownOptimalCharsSec.Value = new decimal(new int[] {
            11,
            0,
            0,
            0});
            this.numericUpDownOptimalCharsSec.ValueChanged += new System.EventHandler(this.ProfileUiValueChanged);
            // 
            // labelSubMaxLen
            // 
            this.labelSubMaxLen.AutoSize = true;
            this.labelSubMaxLen.Location = new System.Drawing.Point(6, 57);
            this.labelSubMaxLen.Name = "labelSubMaxLen";
            this.labelSubMaxLen.Size = new System.Drawing.Size(103, 13);
            this.labelSubMaxLen.TabIndex = 6;
            this.labelSubMaxLen.Text = "Subtitle max. length";
            // 
            // numericUpDownMaxWordsMin
            // 
            this.numericUpDownMaxWordsMin.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownMaxWordsMin.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownMaxWordsMin.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownMaxWordsMin.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownMaxWordsMin.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownMaxWordsMin.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownMaxWordsMin.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownMaxWordsMin.DecimalPlaces = 0;
            this.numericUpDownMaxWordsMin.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownMaxWordsMin.Location = new System.Drawing.Point(203, 136);
            this.numericUpDownMaxWordsMin.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownMaxWordsMin.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.numericUpDownMaxWordsMin.Name = "numericUpDownMaxWordsMin";
            this.numericUpDownMaxWordsMin.Size = new System.Drawing.Size(56, 21);
            this.numericUpDownMaxWordsMin.TabIndex = 30;
            this.numericUpDownMaxWordsMin.TabStop = false;
            this.numericUpDownMaxWordsMin.ThousandsSeparator = false;
            this.numericUpDownMaxWordsMin.Value = new decimal(new int[] {
            300,
            0,
            0,
            0});
            this.numericUpDownMaxWordsMin.ValueChanged += new System.EventHandler(this.ProfileUiValueChanged);
            // 
            // labelMergeShortLines
            // 
            this.labelMergeShortLines.AutoSize = true;
            this.labelMergeShortLines.Location = new System.Drawing.Point(6, 273);
            this.labelMergeShortLines.Name = "labelMergeShortLines";
            this.labelMergeShortLines.Size = new System.Drawing.Size(124, 13);
            this.labelMergeShortLines.TabIndex = 16;
            this.labelMergeShortLines.Text = "Merge lines shorter than";
            // 
            // labelMaxWordsPerMin
            // 
            this.labelMaxWordsPerMin.AutoSize = true;
            this.labelMaxWordsPerMin.Location = new System.Drawing.Point(6, 138);
            this.labelMaxWordsPerMin.Name = "labelMaxWordsPerMin";
            this.labelMaxWordsPerMin.Size = new System.Drawing.Size(83, 13);
            this.labelMaxWordsPerMin.TabIndex = 49;
            this.labelMaxWordsPerMin.Text = "Max. words/min";
            // 
            // labelMinDuration
            // 
            this.labelMinDuration.AutoSize = true;
            this.labelMinDuration.Location = new System.Drawing.Point(6, 165);
            this.labelMinDuration.Name = "labelMinDuration";
            this.labelMinDuration.Size = new System.Drawing.Size(132, 13);
            this.labelMinDuration.TabIndex = 10;
            this.labelMinDuration.Text = "Min. duration, milliseconds";
            // 
            // numericUpDownMaxNumberOfLines
            // 
            this.numericUpDownMaxNumberOfLines.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownMaxNumberOfLines.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownMaxNumberOfLines.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownMaxNumberOfLines.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownMaxNumberOfLines.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownMaxNumberOfLines.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownMaxNumberOfLines.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownMaxNumberOfLines.DecimalPlaces = 0;
            this.numericUpDownMaxNumberOfLines.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownMaxNumberOfLines.Location = new System.Drawing.Point(203, 242);
            this.numericUpDownMaxNumberOfLines.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.numericUpDownMaxNumberOfLines.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownMaxNumberOfLines.Name = "numericUpDownMaxNumberOfLines";
            this.numericUpDownMaxNumberOfLines.Size = new System.Drawing.Size(56, 21);
            this.numericUpDownMaxNumberOfLines.TabIndex = 50;
            this.numericUpDownMaxNumberOfLines.TabStop = false;
            this.numericUpDownMaxNumberOfLines.ThousandsSeparator = false;
            this.numericUpDownMaxNumberOfLines.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numericUpDownMaxNumberOfLines.ValueChanged += new System.EventHandler(this.numericUpDownMaxNumberOfLines_ValueChanged);
            // 
            // labelMaxDuration
            // 
            this.labelMaxDuration.AutoSize = true;
            this.labelMaxDuration.Location = new System.Drawing.Point(6, 191);
            this.labelMaxDuration.Name = "labelMaxDuration";
            this.labelMaxDuration.Size = new System.Drawing.Size(136, 13);
            this.labelMaxDuration.TabIndex = 12;
            this.labelMaxDuration.Text = "Max. duration, milliseconds";
            // 
            // labelMaxLines
            // 
            this.labelMaxLines.AutoSize = true;
            this.labelMaxLines.Location = new System.Drawing.Point(6, 244);
            this.labelMaxLines.Name = "labelMaxLines";
            this.labelMaxLines.Size = new System.Drawing.Size(107, 13);
            this.labelMaxLines.TabIndex = 47;
            this.labelMaxLines.Text = "Max. number of lines";
            // 
            // numericUpDownDurationMin
            // 
            this.numericUpDownDurationMin.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownDurationMin.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownDurationMin.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownDurationMin.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownDurationMin.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownDurationMin.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownDurationMin.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownDurationMin.DecimalPlaces = 0;
            this.numericUpDownDurationMin.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownDurationMin.Location = new System.Drawing.Point(203, 163);
            this.numericUpDownDurationMin.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDownDurationMin.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDownDurationMin.Name = "numericUpDownDurationMin";
            this.numericUpDownDurationMin.Size = new System.Drawing.Size(56, 21);
            this.numericUpDownDurationMin.TabIndex = 35;
            this.numericUpDownDurationMin.TabStop = false;
            this.numericUpDownDurationMin.ThousandsSeparator = false;
            this.numericUpDownDurationMin.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDownDurationMin.ValueChanged += new System.EventHandler(this.ProfileUiValueChanged);
            // 
            // numericUpDownDurationMax
            // 
            this.numericUpDownDurationMax.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownDurationMax.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownDurationMax.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownDurationMax.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownDurationMax.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownDurationMax.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownDurationMax.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownDurationMax.DecimalPlaces = 0;
            this.numericUpDownDurationMax.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownDurationMax.Location = new System.Drawing.Point(203, 189);
            this.numericUpDownDurationMax.Maximum = new decimal(new int[] {
            50000,
            0,
            0,
            0});
            this.numericUpDownDurationMax.Minimum = new decimal(new int[] {
            3000,
            0,
            0,
            0});
            this.numericUpDownDurationMax.Name = "numericUpDownDurationMax";
            this.numericUpDownDurationMax.Size = new System.Drawing.Size(56, 21);
            this.numericUpDownDurationMax.TabIndex = 40;
            this.numericUpDownDurationMax.TabStop = false;
            this.numericUpDownDurationMax.ThousandsSeparator = false;
            this.numericUpDownDurationMax.Value = new decimal(new int[] {
            50000,
            0,
            0,
            0});
            this.numericUpDownDurationMax.ValueChanged += new System.EventHandler(this.ProfileUiValueChanged);
            // 
            // comboBoxMergeShortLineLength
            // 
            this.comboBoxMergeShortLineLength.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxMergeShortLineLength.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxMergeShortLineLength.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxMergeShortLineLength.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxMergeShortLineLength.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxMergeShortLineLength.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxMergeShortLineLength.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxMergeShortLineLength.DropDownHeight = 400;
            this.comboBoxMergeShortLineLength.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxMergeShortLineLength.DropDownWidth = 73;
            this.comboBoxMergeShortLineLength.FormattingEnabled = true;
            this.comboBoxMergeShortLineLength.Location = new System.Drawing.Point(203, 270);
            this.comboBoxMergeShortLineLength.MaxLength = 32767;
            this.comboBoxMergeShortLineLength.Name = "comboBoxMergeShortLineLength";
            this.comboBoxMergeShortLineLength.SelectedIndex = -1;
            this.comboBoxMergeShortLineLength.SelectedItem = null;
            this.comboBoxMergeShortLineLength.SelectedText = "";
            this.comboBoxMergeShortLineLength.Size = new System.Drawing.Size(73, 21);
            this.comboBoxMergeShortLineLength.TabIndex = 55;
            this.comboBoxMergeShortLineLength.UsePopupWindow = false;
            this.comboBoxMergeShortLineLength.SelectedIndexChanged += new System.EventHandler(this.ProfileUiValueChanged);
            // 
            // labelMaxCharsPerSecond
            // 
            this.labelMaxCharsPerSecond.AutoSize = true;
            this.labelMaxCharsPerSecond.Location = new System.Drawing.Point(6, 111);
            this.labelMaxCharsPerSecond.Name = "labelMaxCharsPerSecond";
            this.labelMaxCharsPerSecond.Size = new System.Drawing.Size(80, 13);
            this.labelMaxCharsPerSecond.TabIndex = 9;
            this.labelMaxCharsPerSecond.Text = "Max. chars/sec";
            // 
            // numericUpDownMinGapMs
            // 
            this.numericUpDownMinGapMs.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownMinGapMs.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownMinGapMs.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownMinGapMs.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownMinGapMs.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownMinGapMs.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownMinGapMs.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownMinGapMs.DecimalPlaces = 0;
            this.numericUpDownMinGapMs.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownMinGapMs.Location = new System.Drawing.Point(203, 215);
            this.numericUpDownMinGapMs.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownMinGapMs.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.numericUpDownMinGapMs.Name = "numericUpDownMinGapMs";
            this.numericUpDownMinGapMs.Size = new System.Drawing.Size(56, 21);
            this.numericUpDownMinGapMs.TabIndex = 45;
            this.numericUpDownMinGapMs.TabStop = false;
            this.numericUpDownMinGapMs.ThousandsSeparator = false;
            this.numericUpDownMinGapMs.Value = new decimal(new int[] {
            25,
            0,
            0,
            0});
            this.numericUpDownMinGapMs.ValueChanged += new System.EventHandler(this.ProfileUiValueChanged);
            // 
            // numericUpDownMaxCharsSec
            // 
            this.numericUpDownMaxCharsSec.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownMaxCharsSec.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownMaxCharsSec.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownMaxCharsSec.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownMaxCharsSec.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownMaxCharsSec.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownMaxCharsSec.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownMaxCharsSec.DecimalPlaces = 1;
            this.numericUpDownMaxCharsSec.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericUpDownMaxCharsSec.Location = new System.Drawing.Point(203, 109);
            this.numericUpDownMaxCharsSec.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDownMaxCharsSec.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.numericUpDownMaxCharsSec.Name = "numericUpDownMaxCharsSec";
            this.numericUpDownMaxCharsSec.Size = new System.Drawing.Size(56, 21);
            this.numericUpDownMaxCharsSec.TabIndex = 25;
            this.numericUpDownMaxCharsSec.TabStop = false;
            this.numericUpDownMaxCharsSec.ThousandsSeparator = false;
            this.numericUpDownMaxCharsSec.Value = new decimal(new int[] {
            24,
            0,
            0,
            0});
            this.numericUpDownMaxCharsSec.ValueChanged += new System.EventHandler(this.ProfileUiValueChanged);
            // 
            // labelMinGapMs
            // 
            this.labelMinGapMs.AutoSize = true;
            this.labelMinGapMs.Location = new System.Drawing.Point(6, 217);
            this.labelMinGapMs.Name = "labelMinGapMs";
            this.labelMinGapMs.Size = new System.Drawing.Size(136, 13);
            this.labelMinGapMs.TabIndex = 14;
            this.labelMinGapMs.Text = "Min. gap between subtitles";
            // 
            // numericUpDownSubtitleLineMaximumLength
            // 
            this.numericUpDownSubtitleLineMaximumLength.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownSubtitleLineMaximumLength.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownSubtitleLineMaximumLength.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownSubtitleLineMaximumLength.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownSubtitleLineMaximumLength.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownSubtitleLineMaximumLength.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownSubtitleLineMaximumLength.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownSubtitleLineMaximumLength.DecimalPlaces = 0;
            this.numericUpDownSubtitleLineMaximumLength.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownSubtitleLineMaximumLength.Location = new System.Drawing.Point(203, 55);
            this.numericUpDownSubtitleLineMaximumLength.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.numericUpDownSubtitleLineMaximumLength.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDownSubtitleLineMaximumLength.Name = "numericUpDownSubtitleLineMaximumLength";
            this.numericUpDownSubtitleLineMaximumLength.Size = new System.Drawing.Size(56, 21);
            this.numericUpDownSubtitleLineMaximumLength.TabIndex = 15;
            this.numericUpDownSubtitleLineMaximumLength.TabStop = false;
            this.numericUpDownSubtitleLineMaximumLength.ThousandsSeparator = false;
            this.numericUpDownSubtitleLineMaximumLength.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDownSubtitleLineMaximumLength.ValueChanged += new System.EventHandler(this.ProfileUiValueChanged);
            // 
            // comboBoxAutoBackupDeleteAfter
            // 
            this.comboBoxAutoBackupDeleteAfter.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxAutoBackupDeleteAfter.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxAutoBackupDeleteAfter.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxAutoBackupDeleteAfter.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxAutoBackupDeleteAfter.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxAutoBackupDeleteAfter.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxAutoBackupDeleteAfter.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxAutoBackupDeleteAfter.DropDownHeight = 400;
            this.comboBoxAutoBackupDeleteAfter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxAutoBackupDeleteAfter.DropDownWidth = 88;
            this.comboBoxAutoBackupDeleteAfter.FormattingEnabled = true;
            this.comboBoxAutoBackupDeleteAfter.Items.AddRange(new string[] {
            "1 month",
            "3 months",
            "6 months"});
            this.comboBoxAutoBackupDeleteAfter.Location = new System.Drawing.Point(687, 459);
            this.comboBoxAutoBackupDeleteAfter.MaxLength = 32767;
            this.comboBoxAutoBackupDeleteAfter.Name = "comboBoxAutoBackupDeleteAfter";
            this.comboBoxAutoBackupDeleteAfter.SelectedIndex = -1;
            this.comboBoxAutoBackupDeleteAfter.SelectedItem = null;
            this.comboBoxAutoBackupDeleteAfter.SelectedText = "";
            this.comboBoxAutoBackupDeleteAfter.Size = new System.Drawing.Size(88, 21);
            this.comboBoxAutoBackupDeleteAfter.TabIndex = 29;
            this.comboBoxAutoBackupDeleteAfter.UsePopupWindow = false;
            // 
            // labelAutoBackupDeleteAfter
            // 
            this.labelAutoBackupDeleteAfter.AutoSize = true;
            this.labelAutoBackupDeleteAfter.Location = new System.Drawing.Point(619, 462);
            this.labelAutoBackupDeleteAfter.Name = "labelAutoBackupDeleteAfter";
            this.labelAutoBackupDeleteAfter.Size = new System.Drawing.Size(65, 13);
            this.labelAutoBackupDeleteAfter.TabIndex = 28;
            this.labelAutoBackupDeleteAfter.Text = "Delete after";
            // 
            // checkBoxCheckForUpdates
            // 
            this.checkBoxCheckForUpdates.AutoSize = true;
            this.checkBoxCheckForUpdates.Location = new System.Drawing.Point(502, 491);
            this.checkBoxCheckForUpdates.Name = "checkBoxCheckForUpdates";
            this.checkBoxCheckForUpdates.Size = new System.Drawing.Size(114, 17);
            this.checkBoxCheckForUpdates.TabIndex = 31;
            this.checkBoxCheckForUpdates.Text = "Check for updates";
            this.checkBoxCheckForUpdates.UseVisualStyleBackColor = true;
            // 
            // labelSpellChecker
            // 
            this.labelSpellChecker.AutoSize = true;
            this.labelSpellChecker.Location = new System.Drawing.Point(647, 19);
            this.labelSpellChecker.Name = "labelSpellChecker";
            this.labelSpellChecker.Size = new System.Drawing.Size(69, 13);
            this.labelSpellChecker.TabIndex = 30;
            this.labelSpellChecker.Text = "Spell checker";
            this.labelSpellChecker.Visible = false;
            // 
            // comboBoxTimeCodeMode
            // 
            this.comboBoxTimeCodeMode.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxTimeCodeMode.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxTimeCodeMode.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxTimeCodeMode.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxTimeCodeMode.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxTimeCodeMode.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxTimeCodeMode.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxTimeCodeMode.DropDownHeight = 400;
            this.comboBoxTimeCodeMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxTimeCodeMode.DropDownWidth = 207;
            this.comboBoxTimeCodeMode.FormattingEnabled = true;
            this.comboBoxTimeCodeMode.Items.AddRange(new string[] {
            "HH:MM:SS:MSEC (00:00:00.000)",
            "HH:MM:SS:FF (00:00:00.00)"});
            this.comboBoxTimeCodeMode.Location = new System.Drawing.Point(506, 261);
            this.comboBoxTimeCodeMode.MaxLength = 32767;
            this.comboBoxTimeCodeMode.Name = "comboBoxTimeCodeMode";
            this.comboBoxTimeCodeMode.SelectedIndex = -1;
            this.comboBoxTimeCodeMode.SelectedItem = null;
            this.comboBoxTimeCodeMode.SelectedText = "";
            this.comboBoxTimeCodeMode.Size = new System.Drawing.Size(207, 21);
            this.comboBoxTimeCodeMode.TabIndex = 19;
            this.comboBoxTimeCodeMode.UsePopupWindow = false;
            // 
            // labelTimeCodeMode
            // 
            this.labelTimeCodeMode.AutoSize = true;
            this.labelTimeCodeMode.Location = new System.Drawing.Point(416, 264);
            this.labelTimeCodeMode.Name = "labelTimeCodeMode";
            this.labelTimeCodeMode.Size = new System.Drawing.Size(84, 13);
            this.labelTimeCodeMode.TabIndex = 18;
            this.labelTimeCodeMode.Text = "Time code mode";
            // 
            // comboBoxEncoding
            // 
            this.comboBoxEncoding.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxEncoding.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxEncoding.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxEncoding.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxEncoding.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxEncoding.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxEncoding.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxEncoding.DropDownHeight = 400;
            this.comboBoxEncoding.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxEncoding.DropDownWidth = 188;
            this.comboBoxEncoding.FormattingEnabled = true;
            this.comboBoxEncoding.Items.AddRange(new string[] {
            "ANSI",
            "UTF-7",
            "UTF-8",
            "Unicode"});
            this.comboBoxEncoding.Location = new System.Drawing.Point(205, 438);
            this.comboBoxEncoding.MaxLength = 32767;
            this.comboBoxEncoding.Name = "comboBoxEncoding";
            this.comboBoxEncoding.SelectedIndex = -1;
            this.comboBoxEncoding.SelectedItem = null;
            this.comboBoxEncoding.SelectedText = "";
            this.comboBoxEncoding.Size = new System.Drawing.Size(188, 21);
            this.comboBoxEncoding.TabIndex = 4;
            this.comboBoxEncoding.UsePopupWindow = false;
            // 
            // checkBoxAutoDetectAnsiEncoding
            // 
            this.checkBoxAutoDetectAnsiEncoding.AutoSize = true;
            this.checkBoxAutoDetectAnsiEncoding.Location = new System.Drawing.Point(205, 467);
            this.checkBoxAutoDetectAnsiEncoding.Name = "checkBoxAutoDetectAnsiEncoding";
            this.checkBoxAutoDetectAnsiEncoding.Size = new System.Drawing.Size(15, 14);
            this.checkBoxAutoDetectAnsiEncoding.TabIndex = 6;
            this.checkBoxAutoDetectAnsiEncoding.UseVisualStyleBackColor = true;
            // 
            // textBoxShowLineBreaksAs
            // 
            this.textBoxShowLineBreaksAs.BackColor = System.Drawing.SystemColors.Window;
            this.textBoxShowLineBreaksAs.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxShowLineBreaksAs.Location = new System.Drawing.Point(572, 233);
            this.textBoxShowLineBreaksAs.MaxLength = 10;
            this.textBoxShowLineBreaksAs.Multiline = true;
            this.textBoxShowLineBreaksAs.Name = "textBoxShowLineBreaksAs";
            this.textBoxShowLineBreaksAs.Size = new System.Drawing.Size(60, 21);
            this.textBoxShowLineBreaksAs.TabIndex = 17;
            // 
            // checkBoxAutoWrapWhileTyping
            // 
            this.checkBoxAutoWrapWhileTyping.AutoSize = true;
            this.checkBoxAutoWrapWhileTyping.Location = new System.Drawing.Point(419, 210);
            this.checkBoxAutoWrapWhileTyping.Name = "checkBoxAutoWrapWhileTyping";
            this.checkBoxAutoWrapWhileTyping.Size = new System.Drawing.Size(137, 17);
            this.checkBoxAutoWrapWhileTyping.TabIndex = 15;
            this.checkBoxAutoWrapWhileTyping.Text = "Auto-wrap while typing";
            this.checkBoxAutoWrapWhileTyping.UseVisualStyleBackColor = true;
            // 
            // checkBoxPromptDeleteLines
            // 
            this.checkBoxPromptDeleteLines.AutoSize = true;
            this.checkBoxPromptDeleteLines.Location = new System.Drawing.Point(419, 187);
            this.checkBoxPromptDeleteLines.Name = "checkBoxPromptDeleteLines";
            this.checkBoxPromptDeleteLines.Size = new System.Drawing.Size(142, 17);
            this.checkBoxPromptDeleteLines.TabIndex = 14;
            this.checkBoxPromptDeleteLines.Text = "Prompt for deleting lines";
            this.checkBoxPromptDeleteLines.UseVisualStyleBackColor = true;
            // 
            // checkBoxAllowEditOfOriginalSubtitle
            // 
            this.checkBoxAllowEditOfOriginalSubtitle.AutoSize = true;
            this.checkBoxAllowEditOfOriginalSubtitle.Location = new System.Drawing.Point(419, 164);
            this.checkBoxAllowEditOfOriginalSubtitle.Name = "checkBoxAllowEditOfOriginalSubtitle";
            this.checkBoxAllowEditOfOriginalSubtitle.Size = new System.Drawing.Size(160, 17);
            this.checkBoxAllowEditOfOriginalSubtitle.TabIndex = 13;
            this.checkBoxAllowEditOfOriginalSubtitle.Text = "Allow edit of original subtitle";
            this.checkBoxAllowEditOfOriginalSubtitle.UseVisualStyleBackColor = true;
            // 
            // comboBoxSpellChecker
            // 
            this.comboBoxSpellChecker.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxSpellChecker.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxSpellChecker.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxSpellChecker.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxSpellChecker.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxSpellChecker.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxSpellChecker.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxSpellChecker.DropDownHeight = 400;
            this.comboBoxSpellChecker.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSpellChecker.DropDownWidth = 121;
            this.comboBoxSpellChecker.FormattingEnabled = true;
            this.comboBoxSpellChecker.Items.AddRange(new string[] {
            "Hunspell",
            "Word"});
            this.comboBoxSpellChecker.Location = new System.Drawing.Point(670, 16);
            this.comboBoxSpellChecker.MaxLength = 32767;
            this.comboBoxSpellChecker.Name = "comboBoxSpellChecker";
            this.comboBoxSpellChecker.SelectedIndex = -1;
            this.comboBoxSpellChecker.SelectedItem = null;
            this.comboBoxSpellChecker.SelectedText = "";
            this.comboBoxSpellChecker.Size = new System.Drawing.Size(121, 21);
            this.comboBoxSpellChecker.TabIndex = 31;
            this.comboBoxSpellChecker.UsePopupWindow = false;
            this.comboBoxSpellChecker.Visible = false;
            // 
            // comboBoxAutoBackup
            // 
            this.comboBoxAutoBackup.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxAutoBackup.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxAutoBackup.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxAutoBackup.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxAutoBackup.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxAutoBackup.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxAutoBackup.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxAutoBackup.DropDownHeight = 400;
            this.comboBoxAutoBackup.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxAutoBackup.DropDownWidth = 121;
            this.comboBoxAutoBackup.FormattingEnabled = true;
            this.comboBoxAutoBackup.Items.AddRange(new string[] {
            "None",
            "Every minute",
            "Every 5 minutes",
            "Every 15 minutes"});
            this.comboBoxAutoBackup.Location = new System.Drawing.Point(492, 459);
            this.comboBoxAutoBackup.MaxLength = 32767;
            this.comboBoxAutoBackup.Name = "comboBoxAutoBackup";
            this.comboBoxAutoBackup.SelectedIndex = -1;
            this.comboBoxAutoBackup.SelectedItem = null;
            this.comboBoxAutoBackup.SelectedText = "";
            this.comboBoxAutoBackup.Size = new System.Drawing.Size(121, 21);
            this.comboBoxAutoBackup.TabIndex = 27;
            this.comboBoxAutoBackup.UsePopupWindow = false;
            // 
            // labelAutoBackup
            // 
            this.labelAutoBackup.AutoSize = true;
            this.labelAutoBackup.Location = new System.Drawing.Point(418, 462);
            this.labelAutoBackup.Name = "labelAutoBackup";
            this.labelAutoBackup.Size = new System.Drawing.Size(68, 13);
            this.labelAutoBackup.TabIndex = 26;
            this.labelAutoBackup.Text = "Auto-backup";
            // 
            // checkBoxRememberSelectedLine
            // 
            this.checkBoxRememberSelectedLine.AutoSize = true;
            this.checkBoxRememberSelectedLine.Location = new System.Drawing.Point(427, 69);
            this.checkBoxRememberSelectedLine.Name = "checkBoxRememberSelectedLine";
            this.checkBoxRememberSelectedLine.Size = new System.Drawing.Size(139, 17);
            this.checkBoxRememberSelectedLine.TabIndex = 9;
            this.checkBoxRememberSelectedLine.Text = "Remember selected line";
            this.checkBoxRememberSelectedLine.UseVisualStyleBackColor = true;
            // 
            // checkBoxRemoveBlankLinesWhenOpening
            // 
            this.checkBoxRemoveBlankLinesWhenOpening.AutoSize = true;
            this.checkBoxRemoveBlankLinesWhenOpening.Location = new System.Drawing.Point(419, 141);
            this.checkBoxRemoveBlankLinesWhenOpening.Name = "checkBoxRemoveBlankLinesWhenOpening";
            this.checkBoxRemoveBlankLinesWhenOpening.Size = new System.Drawing.Size(225, 17);
            this.checkBoxRemoveBlankLinesWhenOpening.TabIndex = 12;
            this.checkBoxRemoveBlankLinesWhenOpening.Text = "Remove blank lines when opening subtitle";
            this.checkBoxRemoveBlankLinesWhenOpening.UseVisualStyleBackColor = true;
            // 
            // labelAutoDetectAnsiEncoding
            // 
            this.labelAutoDetectAnsiEncoding.AutoSize = true;
            this.labelAutoDetectAnsiEncoding.Location = new System.Drawing.Point(8, 466);
            this.labelAutoDetectAnsiEncoding.Name = "labelAutoDetectAnsiEncoding";
            this.labelAutoDetectAnsiEncoding.Size = new System.Drawing.Size(137, 13);
            this.labelAutoDetectAnsiEncoding.TabIndex = 5;
            this.labelAutoDetectAnsiEncoding.Text = "Auto detect ANSI encoding";
            // 
            // comboBoxListViewDoubleClickEvent
            // 
            this.comboBoxListViewDoubleClickEvent.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxListViewDoubleClickEvent.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxListViewDoubleClickEvent.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxListViewDoubleClickEvent.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxListViewDoubleClickEvent.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxListViewDoubleClickEvent.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxListViewDoubleClickEvent.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxListViewDoubleClickEvent.DropDownHeight = 400;
            this.comboBoxListViewDoubleClickEvent.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxListViewDoubleClickEvent.DropDownWidth = 332;
            this.comboBoxListViewDoubleClickEvent.FormattingEnabled = true;
            this.comboBoxListViewDoubleClickEvent.Items.AddRange(new string[] {
            "ANSI",
            "UTF-7",
            "UTF-8",
            "Unicode"});
            this.comboBoxListViewDoubleClickEvent.Location = new System.Drawing.Point(419, 337);
            this.comboBoxListViewDoubleClickEvent.MaxLength = 32767;
            this.comboBoxListViewDoubleClickEvent.Name = "comboBoxListViewDoubleClickEvent";
            this.comboBoxListViewDoubleClickEvent.SelectedIndex = -1;
            this.comboBoxListViewDoubleClickEvent.SelectedItem = null;
            this.comboBoxListViewDoubleClickEvent.SelectedText = "";
            this.comboBoxListViewDoubleClickEvent.Size = new System.Drawing.Size(332, 21);
            this.comboBoxListViewDoubleClickEvent.TabIndex = 23;
            this.comboBoxListViewDoubleClickEvent.UsePopupWindow = false;
            // 
            // labelListViewDoubleClickEvent
            // 
            this.labelListViewDoubleClickEvent.AutoSize = true;
            this.labelListViewDoubleClickEvent.Location = new System.Drawing.Point(416, 321);
            this.labelListViewDoubleClickEvent.Name = "labelListViewDoubleClickEvent";
            this.labelListViewDoubleClickEvent.Size = new System.Drawing.Size(227, 13);
            this.labelListViewDoubleClickEvent.TabIndex = 22;
            this.labelListViewDoubleClickEvent.Text = "Double-click on line in main window listview will";
            // 
            // labelShowLineBreaksAs
            // 
            this.labelShowLineBreaksAs.AutoSize = true;
            this.labelShowLineBreaksAs.Location = new System.Drawing.Point(416, 236);
            this.labelShowLineBreaksAs.Name = "labelShowLineBreaksAs";
            this.labelShowLineBreaksAs.Size = new System.Drawing.Size(150, 13);
            this.labelShowLineBreaksAs.TabIndex = 16;
            this.labelShowLineBreaksAs.Text = "Show line breaks in listview as";
            // 
            // checkBoxRememberWindowPosition
            // 
            this.checkBoxRememberWindowPosition.AutoSize = true;
            this.checkBoxRememberWindowPosition.Location = new System.Drawing.Point(419, 95);
            this.checkBoxRememberWindowPosition.Name = "checkBoxRememberWindowPosition";
            this.checkBoxRememberWindowPosition.Size = new System.Drawing.Size(223, 17);
            this.checkBoxRememberWindowPosition.TabIndex = 10;
            this.checkBoxRememberWindowPosition.Text = "Remember main window position and size";
            this.checkBoxRememberWindowPosition.UseVisualStyleBackColor = true;
            // 
            // checkBoxStartInSourceView
            // 
            this.checkBoxStartInSourceView.AutoSize = true;
            this.checkBoxStartInSourceView.Location = new System.Drawing.Point(419, 118);
            this.checkBoxStartInSourceView.Name = "checkBoxStartInSourceView";
            this.checkBoxStartInSourceView.Size = new System.Drawing.Size(121, 17);
            this.checkBoxStartInSourceView.TabIndex = 11;
            this.checkBoxStartInSourceView.Text = "Start in source view";
            this.checkBoxStartInSourceView.UseVisualStyleBackColor = true;
            // 
            // checkBoxReopenLastOpened
            // 
            this.checkBoxReopenLastOpened.AutoSize = true;
            this.checkBoxReopenLastOpened.Location = new System.Drawing.Point(427, 46);
            this.checkBoxReopenLastOpened.Name = "checkBoxReopenLastOpened";
            this.checkBoxReopenLastOpened.Size = new System.Drawing.Size(145, 17);
            this.checkBoxReopenLastOpened.TabIndex = 8;
            this.checkBoxReopenLastOpened.Text = "Start with last file loaded";
            this.checkBoxReopenLastOpened.UseVisualStyleBackColor = true;
            // 
            // checkBoxRememberRecentFiles
            // 
            this.checkBoxRememberRecentFiles.AutoSize = true;
            this.checkBoxRememberRecentFiles.Location = new System.Drawing.Point(419, 22);
            this.checkBoxRememberRecentFiles.Name = "checkBoxRememberRecentFiles";
            this.checkBoxRememberRecentFiles.Size = new System.Drawing.Size(195, 17);
            this.checkBoxRememberRecentFiles.TabIndex = 7;
            this.checkBoxRememberRecentFiles.Text = "Remember recent files (for reopen)";
            this.checkBoxRememberRecentFiles.UseVisualStyleBackColor = true;
            this.checkBoxRememberRecentFiles.CheckedChanged += new System.EventHandler(this.checkBoxRememberRecentFiles_CheckedChanged);
            // 
            // labelDefaultFileEncoding
            // 
            this.labelDefaultFileEncoding.AutoSize = true;
            this.labelDefaultFileEncoding.Location = new System.Drawing.Point(8, 441);
            this.labelDefaultFileEncoding.Name = "labelDefaultFileEncoding";
            this.labelDefaultFileEncoding.Size = new System.Drawing.Size(105, 13);
            this.labelDefaultFileEncoding.TabIndex = 3;
            this.labelDefaultFileEncoding.Text = "Default file encoding";
            // 
            // comboBoxFrameRate
            // 
            this.comboBoxFrameRate.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxFrameRate.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxFrameRate.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxFrameRate.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxFrameRate.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxFrameRate.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxFrameRate.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxFrameRate.DropDownHeight = 400;
            this.comboBoxFrameRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.comboBoxFrameRate.DropDownWidth = 121;
            this.comboBoxFrameRate.FormattingEnabled = true;
            this.comboBoxFrameRate.Location = new System.Drawing.Point(205, 410);
            this.comboBoxFrameRate.MaxLength = 32767;
            this.comboBoxFrameRate.Name = "comboBoxFrameRate";
            this.comboBoxFrameRate.SelectedIndex = -1;
            this.comboBoxFrameRate.SelectedItem = null;
            this.comboBoxFrameRate.SelectedText = "";
            this.comboBoxFrameRate.Size = new System.Drawing.Size(121, 21);
            this.comboBoxFrameRate.TabIndex = 2;
            this.comboBoxFrameRate.TabStop = false;
            this.comboBoxFrameRate.UsePopupWindow = false;
            // 
            // labelDefaultFrameRate
            // 
            this.labelDefaultFrameRate.AutoSize = true;
            this.labelDefaultFrameRate.Location = new System.Drawing.Point(8, 415);
            this.labelDefaultFrameRate.Name = "labelDefaultFrameRate";
            this.labelDefaultFrameRate.Size = new System.Drawing.Size(96, 13);
            this.labelDefaultFrameRate.TabIndex = 1;
            this.labelDefaultFrameRate.Text = "Default frame rate";
            // 
            // panelSubtitleFormats
            // 
            this.panelSubtitleFormats.Controls.Add(this.groupBoxSubtitleFormats);
            this.panelSubtitleFormats.Location = new System.Drawing.Point(230, 6);
            this.panelSubtitleFormats.Name = "panelSubtitleFormats";
            this.panelSubtitleFormats.Padding = new System.Windows.Forms.Padding(3);
            this.panelSubtitleFormats.Size = new System.Drawing.Size(864, 521);
            this.panelSubtitleFormats.TabIndex = 2;
            this.panelSubtitleFormats.Text = "Subtite formats";
            // 
            // groupBoxSubtitleFormats
            // 
            this.groupBoxSubtitleFormats.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxSubtitleFormats.Controls.Add(this.groupBoxFavoriteSubtitleFormats);
            this.groupBoxSubtitleFormats.Controls.Add(this.comboBoxSubtitleSaveAsFormats);
            this.groupBoxSubtitleFormats.Controls.Add(this.labelDefaultSaveAsFormat);
            this.groupBoxSubtitleFormats.Controls.Add(this.comboBoxSubtitleFormats);
            this.groupBoxSubtitleFormats.Controls.Add(this.labelDefaultSubtitleFormat);
            this.groupBoxSubtitleFormats.Location = new System.Drawing.Point(0, 0);
            this.groupBoxSubtitleFormats.Name = "groupBoxSubtitleFormats";
            this.groupBoxSubtitleFormats.Size = new System.Drawing.Size(851, 521);
            this.groupBoxSubtitleFormats.TabIndex = 2;
            this.groupBoxSubtitleFormats.TabStop = false;
            this.groupBoxSubtitleFormats.Text = "Subtitle formats";
            // 
            // groupBoxFavoriteSubtitleFormats
            // 
            this.groupBoxFavoriteSubtitleFormats.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxFavoriteSubtitleFormats.Controls.Add(this.labelFavoriteSubtitleFormatsNote);
            this.groupBoxFavoriteSubtitleFormats.Controls.Add(this.listBoxSubtitleFormats);
            this.groupBoxFavoriteSubtitleFormats.Controls.Add(this.buttonFormatsSearchClear);
            this.groupBoxFavoriteSubtitleFormats.Controls.Add(this.textBoxFormatsSearch);
            this.groupBoxFavoriteSubtitleFormats.Controls.Add(this.labelFormatsSearch);
            this.groupBoxFavoriteSubtitleFormats.Controls.Add(this.labelFormats);
            this.groupBoxFavoriteSubtitleFormats.Controls.Add(this.buttonRemoveFromFavoriteFormats);
            this.groupBoxFavoriteSubtitleFormats.Controls.Add(this.buttonMoveToFavoriteFormats);
            this.groupBoxFavoriteSubtitleFormats.Controls.Add(this.listBoxFavoriteSubtitleFormats);
            this.groupBoxFavoriteSubtitleFormats.Controls.Add(this.labelFavoriteFormats);
            this.groupBoxFavoriteSubtitleFormats.Location = new System.Drawing.Point(6, 92);
            this.groupBoxFavoriteSubtitleFormats.Name = "groupBoxFavoriteSubtitleFormats";
            this.groupBoxFavoriteSubtitleFormats.Size = new System.Drawing.Size(838, 420);
            this.groupBoxFavoriteSubtitleFormats.TabIndex = 4;
            this.groupBoxFavoriteSubtitleFormats.TabStop = false;
            this.groupBoxFavoriteSubtitleFormats.Text = "Favorites";
            // 
            // labelFavoriteSubtitleFormatsNote
            // 
            this.labelFavoriteSubtitleFormatsNote.AutoSize = true;
            this.labelFavoriteSubtitleFormatsNote.Location = new System.Drawing.Point(10, 367);
            this.labelFavoriteSubtitleFormatsNote.Name = "labelFavoriteSubtitleFormatsNote";
            this.labelFavoriteSubtitleFormatsNote.Size = new System.Drawing.Size(540, 13);
            this.labelFavoriteSubtitleFormatsNote.TabIndex = 9;
            this.labelFavoriteSubtitleFormatsNote.Text = "Note: favorite formats will be shown first when selecting a format, the default f" +
    "ormat will always be shown first";
            // 
            // listBoxSubtitleFormats
            // 
            this.listBoxSubtitleFormats.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listBoxSubtitleFormats.FormattingEnabled = true;
            this.listBoxSubtitleFormats.ItemHeight = 13;
            this.listBoxSubtitleFormats.Location = new System.Drawing.Point(489, 76);
            this.listBoxSubtitleFormats.Name = "listBoxSubtitleFormats";
            this.listBoxSubtitleFormats.SelectedIndex = -1;
            this.listBoxSubtitleFormats.SelectedItem = null;
            this.listBoxSubtitleFormats.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listBoxSubtitleFormats.Size = new System.Drawing.Size(300, 251);
            this.listBoxSubtitleFormats.Sorted = false;
            this.listBoxSubtitleFormats.TabIndex = 8;
            this.listBoxSubtitleFormats.TopIndex = 0;
            this.listBoxSubtitleFormats.SelectedIndexChanged += new System.EventHandler(this.listBoxSubtitleFormats_SelectedIndexChanged);
            this.listBoxSubtitleFormats.LostFocus += new System.EventHandler(this.listBoxSubtitleFormats_LostFocus);
            // 
            // buttonFormatsSearchClear
            // 
            this.buttonFormatsSearchClear.Enabled = false;
            this.buttonFormatsSearchClear.Location = new System.Drawing.Point(683, 52);
            this.buttonFormatsSearchClear.Name = "buttonFormatsSearchClear";
            this.buttonFormatsSearchClear.Size = new System.Drawing.Size(105, 21);
            this.buttonFormatsSearchClear.TabIndex = 7;
            this.buttonFormatsSearchClear.Text = "&Clear";
            this.buttonFormatsSearchClear.UseVisualStyleBackColor = true;
            this.buttonFormatsSearchClear.Click += new System.EventHandler(this.buttonFormatsSearchClear_Click);
            // 
            // textBoxFormatsSearch
            // 
            this.textBoxFormatsSearch.BackColor = System.Drawing.SystemColors.Window;
            this.textBoxFormatsSearch.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxFormatsSearch.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxFormatsSearch.Location = new System.Drawing.Point(529, 52);
            this.textBoxFormatsSearch.Multiline = true;
            this.textBoxFormatsSearch.Name = "textBoxFormatsSearch";
            this.textBoxFormatsSearch.Size = new System.Drawing.Size(151, 21);
            this.textBoxFormatsSearch.TabIndex = 6;
            this.textBoxFormatsSearch.TextChanged += new System.EventHandler(this.textBoxFormatsSearch_TextChanged);
            // 
            // labelFormatsSearch
            // 
            this.labelFormatsSearch.AutoSize = true;
            this.labelFormatsSearch.Location = new System.Drawing.Point(489, 56);
            this.labelFormatsSearch.Name = "labelFormatsSearch";
            this.labelFormatsSearch.Size = new System.Drawing.Size(40, 13);
            this.labelFormatsSearch.TabIndex = 5;
            this.labelFormatsSearch.Text = "Search";
            // 
            // labelFormats
            // 
            this.labelFormats.AutoSize = true;
            this.labelFormats.Location = new System.Drawing.Point(489, 34);
            this.labelFormats.Name = "labelFormats";
            this.labelFormats.Size = new System.Drawing.Size(46, 13);
            this.labelFormats.TabIndex = 4;
            this.labelFormats.Text = "Formats";
            // 
            // buttonRemoveFromFavoriteFormats
            // 
            this.buttonRemoveFromFavoriteFormats.Enabled = false;
            this.buttonRemoveFromFavoriteFormats.Location = new System.Drawing.Point(363, 202);
            this.buttonRemoveFromFavoriteFormats.Name = "buttonRemoveFromFavoriteFormats";
            this.buttonRemoveFromFavoriteFormats.Size = new System.Drawing.Size(111, 23);
            this.buttonRemoveFromFavoriteFormats.TabIndex = 3;
            this.buttonRemoveFromFavoriteFormats.Text = "Remove";
            this.buttonRemoveFromFavoriteFormats.UseVisualStyleBackColor = true;
            this.buttonRemoveFromFavoriteFormats.Click += new System.EventHandler(this.buttonRemoveFromFavoriteFormats_Click);
            // 
            // buttonMoveToFavoriteFormats
            // 
            this.buttonMoveToFavoriteFormats.Enabled = false;
            this.buttonMoveToFavoriteFormats.Location = new System.Drawing.Point(363, 153);
            this.buttonMoveToFavoriteFormats.Name = "buttonMoveToFavoriteFormats";
            this.buttonMoveToFavoriteFormats.Size = new System.Drawing.Size(111, 23);
            this.buttonMoveToFavoriteFormats.TabIndex = 2;
            this.buttonMoveToFavoriteFormats.Text = " < ";
            this.buttonMoveToFavoriteFormats.UseVisualStyleBackColor = true;
            this.buttonMoveToFavoriteFormats.Click += new System.EventHandler(this.buttonMoveToFavorites_Click);
            // 
            // listBoxFavoriteSubtitleFormats
            // 
            this.listBoxFavoriteSubtitleFormats.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listBoxFavoriteSubtitleFormats.ContextMenuStrip = this.contextMenuStripFavoriteFormats;
            this.listBoxFavoriteSubtitleFormats.FormattingEnabled = true;
            this.listBoxFavoriteSubtitleFormats.ItemHeight = 13;
            this.listBoxFavoriteSubtitleFormats.Location = new System.Drawing.Point(48, 50);
            this.listBoxFavoriteSubtitleFormats.Name = "listBoxFavoriteSubtitleFormats";
            this.listBoxFavoriteSubtitleFormats.SelectedIndex = -1;
            this.listBoxFavoriteSubtitleFormats.SelectedItem = null;
            this.listBoxFavoriteSubtitleFormats.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listBoxFavoriteSubtitleFormats.Size = new System.Drawing.Size(300, 277);
            this.listBoxFavoriteSubtitleFormats.Sorted = false;
            this.listBoxFavoriteSubtitleFormats.TabIndex = 1;
            this.listBoxFavoriteSubtitleFormats.TopIndex = 0;
            this.listBoxFavoriteSubtitleFormats.SelectedIndexChanged += new System.EventHandler(this.listBoxFavoriteSubtitleFormats_SelectedIndexChanged);
            this.listBoxFavoriteSubtitleFormats.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listBoxFavoriteSubtitleFormats_KeyDown);
            this.listBoxFavoriteSubtitleFormats.LostFocus += new System.EventHandler(this.listBoxFavoriteSubtitleFormats_LostFocus);
            // 
            // contextMenuStripFavoriteFormats
            // 
            this.contextMenuStripFavoriteFormats.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deleteToolStripMenuItem,
            this.deleteAllToolStripMenuItem,
            this.toolStripSeparator,
            this.moveUpToolStripMenuItem,
            this.moveDownToolStripMenuItem,
            this.moveToTopToolStripMenuItem,
            this.moveToBottomToolStripMenuItem});
            this.contextMenuStripFavoriteFormats.Name = "contextMenuStripFavoriteFormats";
            this.contextMenuStripFavoriteFormats.Size = new System.Drawing.Size(216, 142);
            this.contextMenuStripFavoriteFormats.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStripFavoriteFormats_Opening);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.deleteToolStripMenuItem.Text = "Delete...";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // deleteAllToolStripMenuItem
            // 
            this.deleteAllToolStripMenuItem.Name = "deleteAllToolStripMenuItem";
            this.deleteAllToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.deleteAllToolStripMenuItem.Text = "Delete all";
            this.deleteAllToolStripMenuItem.Click += new System.EventHandler(this.deleteAllToolStripMenuItem_Click);
            // 
            // toolStripSeparator
            // 
            this.toolStripSeparator.Name = "toolStripSeparator";
            this.toolStripSeparator.Size = new System.Drawing.Size(212, 6);
            // 
            // moveUpToolStripMenuItem
            // 
            this.moveUpToolStripMenuItem.Name = "moveUpToolStripMenuItem";
            this.moveUpToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Up)));
            this.moveUpToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.moveUpToolStripMenuItem.Text = "Move up";
            this.moveUpToolStripMenuItem.Click += new System.EventHandler(this.moveUpToolStripMenuItem_Click);
            // 
            // moveDownToolStripMenuItem
            // 
            this.moveDownToolStripMenuItem.Name = "moveDownToolStripMenuItem";
            this.moveDownToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Down)));
            this.moveDownToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.moveDownToolStripMenuItem.Text = "Move down";
            this.moveDownToolStripMenuItem.Click += new System.EventHandler(this.moveDownToolStripMenuItem_Click);
            // 
            // moveToTopToolStripMenuItem
            // 
            this.moveToTopToolStripMenuItem.Name = "moveToTopToolStripMenuItem";
            this.moveToTopToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Home)));
            this.moveToTopToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.moveToTopToolStripMenuItem.Text = "Move to top";
            this.moveToTopToolStripMenuItem.Click += new System.EventHandler(this.moveToTopToolStripMenuItem_Click);
            // 
            // moveToBottomToolStripMenuItem
            // 
            this.moveToBottomToolStripMenuItem.Name = "moveToBottomToolStripMenuItem";
            this.moveToBottomToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.End)));
            this.moveToBottomToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.moveToBottomToolStripMenuItem.Text = "Move to bottom";
            this.moveToBottomToolStripMenuItem.Click += new System.EventHandler(this.moveToBottomToolStripMenuItem_Click);
            // 
            // labelFavoriteFormats
            // 
            this.labelFavoriteFormats.AutoSize = true;
            this.labelFavoriteFormats.Location = new System.Drawing.Point(48, 34);
            this.labelFavoriteFormats.Name = "labelFavoriteFormats";
            this.labelFavoriteFormats.Size = new System.Drawing.Size(87, 13);
            this.labelFavoriteFormats.TabIndex = 0;
            this.labelFavoriteFormats.Text = "Favorite formats";
            // 
            // comboBoxSubtitleSaveAsFormats
            // 
            this.comboBoxSubtitleSaveAsFormats.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxSubtitleSaveAsFormats.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxSubtitleSaveAsFormats.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxSubtitleSaveAsFormats.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxSubtitleSaveAsFormats.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxSubtitleSaveAsFormats.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxSubtitleSaveAsFormats.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxSubtitleSaveAsFormats.DropDownHeight = 400;
            this.comboBoxSubtitleSaveAsFormats.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSubtitleSaveAsFormats.DropDownWidth = 200;
            this.comboBoxSubtitleSaveAsFormats.FormattingEnabled = true;
            this.comboBoxSubtitleSaveAsFormats.Location = new System.Drawing.Point(100, 60);
            this.comboBoxSubtitleSaveAsFormats.MaxLength = 32767;
            this.comboBoxSubtitleSaveAsFormats.Name = "comboBoxSubtitleSaveAsFormats";
            this.comboBoxSubtitleSaveAsFormats.SelectedIndex = -1;
            this.comboBoxSubtitleSaveAsFormats.SelectedItem = null;
            this.comboBoxSubtitleSaveAsFormats.SelectedText = "";
            this.comboBoxSubtitleSaveAsFormats.Size = new System.Drawing.Size(200, 21);
            this.comboBoxSubtitleSaveAsFormats.TabIndex = 3;
            this.comboBoxSubtitleSaveAsFormats.UsePopupWindow = false;
            // 
            // labelDefaultSaveAsFormat
            // 
            this.labelDefaultSaveAsFormat.AutoSize = true;
            this.labelDefaultSaveAsFormat.Location = new System.Drawing.Point(8, 64);
            this.labelDefaultSaveAsFormat.Name = "labelDefaultSaveAsFormat";
            this.labelDefaultSaveAsFormat.Size = new System.Drawing.Size(117, 13);
            this.labelDefaultSaveAsFormat.TabIndex = 2;
            this.labelDefaultSaveAsFormat.Text = "Default save as format";
            // 
            // comboBoxSubtitleFormats
            // 
            this.comboBoxSubtitleFormats.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxSubtitleFormats.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxSubtitleFormats.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxSubtitleFormats.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxSubtitleFormats.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxSubtitleFormats.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxSubtitleFormats.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxSubtitleFormats.DropDownHeight = 400;
            this.comboBoxSubtitleFormats.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSubtitleFormats.DropDownWidth = 200;
            this.comboBoxSubtitleFormats.FormattingEnabled = true;
            this.comboBoxSubtitleFormats.Location = new System.Drawing.Point(100, 26);
            this.comboBoxSubtitleFormats.MaxLength = 32767;
            this.comboBoxSubtitleFormats.Name = "comboBoxSubtitleFormats";
            this.comboBoxSubtitleFormats.SelectedIndex = -1;
            this.comboBoxSubtitleFormats.SelectedItem = null;
            this.comboBoxSubtitleFormats.SelectedText = "";
            this.comboBoxSubtitleFormats.Size = new System.Drawing.Size(200, 21);
            this.comboBoxSubtitleFormats.TabIndex = 1;
            this.comboBoxSubtitleFormats.UsePopupWindow = false;
            // 
            // labelDefaultSubtitleFormat
            // 
            this.labelDefaultSubtitleFormat.AutoSize = true;
            this.labelDefaultSubtitleFormat.Location = new System.Drawing.Point(8, 30);
            this.labelDefaultSubtitleFormat.Name = "labelDefaultSubtitleFormat";
            this.labelDefaultSubtitleFormat.Size = new System.Drawing.Size(77, 13);
            this.labelDefaultSubtitleFormat.TabIndex = 0;
            this.labelDefaultSubtitleFormat.Text = "Default format";
            // 
            // panelShortcuts
            // 
            this.panelShortcuts.Controls.Add(this.groupBoxShortcuts);
            this.panelShortcuts.Location = new System.Drawing.Point(230, 6);
            this.panelShortcuts.Name = "panelShortcuts";
            this.panelShortcuts.Padding = new System.Windows.Forms.Padding(3);
            this.panelShortcuts.Size = new System.Drawing.Size(864, 521);
            this.panelShortcuts.TabIndex = 3;
            this.panelShortcuts.Text = "Shortcuts";
            // 
            // groupBoxShortcuts
            // 
            this.groupBoxShortcuts.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxShortcuts.Controls.Add(this.nikseComboBoxShortcutsFilter);
            this.groupBoxShortcuts.Controls.Add(this.labelShortcutsFilter);
            this.groupBoxShortcuts.Controls.Add(this.buttonShortcutsClear);
            this.groupBoxShortcuts.Controls.Add(this.labelShortcutsSearch);
            this.groupBoxShortcuts.Controls.Add(this.textBoxShortcutSearch);
            this.groupBoxShortcuts.Controls.Add(this.buttonClearShortcut);
            this.groupBoxShortcuts.Controls.Add(this.comboBoxShortcutKey);
            this.groupBoxShortcuts.Controls.Add(this.labelShortcutKey);
            this.groupBoxShortcuts.Controls.Add(this.checkBoxShortcutsShift);
            this.groupBoxShortcuts.Controls.Add(this.checkBoxShortcutsAlt);
            this.groupBoxShortcuts.Controls.Add(this.checkBoxShortcutsControl);
            this.groupBoxShortcuts.Controls.Add(this.buttonUpdateShortcut);
            this.groupBoxShortcuts.Controls.Add(this.treeViewShortcuts);
            this.groupBoxShortcuts.Controls.Add(this.labelShortcut);
            this.groupBoxShortcuts.Location = new System.Drawing.Point(0, 0);
            this.groupBoxShortcuts.Name = "groupBoxShortcuts";
            this.groupBoxShortcuts.Size = new System.Drawing.Size(851, 521);
            this.groupBoxShortcuts.TabIndex = 2;
            this.groupBoxShortcuts.TabStop = false;
            this.groupBoxShortcuts.Text = "Shortcuts";
            // 
            // nikseComboBoxShortcutsFilter
            // 
            this.nikseComboBoxShortcutsFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.nikseComboBoxShortcutsFilter.BackColor = System.Drawing.SystemColors.Window;
            this.nikseComboBoxShortcutsFilter.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.nikseComboBoxShortcutsFilter.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.nikseComboBoxShortcutsFilter.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.nikseComboBoxShortcutsFilter.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.nikseComboBoxShortcutsFilter.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.nikseComboBoxShortcutsFilter.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.nikseComboBoxShortcutsFilter.DropDownHeight = 400;
            this.nikseComboBoxShortcutsFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.nikseComboBoxShortcutsFilter.DropDownWidth = 92;
            this.nikseComboBoxShortcutsFilter.Enabled = false;
            this.nikseComboBoxShortcutsFilter.FormattingEnabled = true;
            this.nikseComboBoxShortcutsFilter.Items.AddRange(new string[] {
            "All",
            "Used",
            "Unused"});
            this.nikseComboBoxShortcutsFilter.Location = new System.Drawing.Point(449, 19);
            this.nikseComboBoxShortcutsFilter.MaxLength = 32767;
            this.nikseComboBoxShortcutsFilter.Name = "nikseComboBoxShortcutsFilter";
            this.nikseComboBoxShortcutsFilter.SelectedIndex = -1;
            this.nikseComboBoxShortcutsFilter.SelectedItem = null;
            this.nikseComboBoxShortcutsFilter.SelectedText = "";
            this.nikseComboBoxShortcutsFilter.Size = new System.Drawing.Size(199, 21);
            this.nikseComboBoxShortcutsFilter.TabIndex = 2;
            this.nikseComboBoxShortcutsFilter.UsePopupWindow = false;
            this.nikseComboBoxShortcutsFilter.SelectedIndexChanged += new System.EventHandler(this.nikseComboBoxShortcutsFilter_SelectedIndexChanged);
            // 
            // labelShortcutsFilter
            // 
            this.labelShortcutsFilter.AutoSize = true;
            this.labelShortcutsFilter.Location = new System.Drawing.Point(406, 22);
            this.labelShortcutsFilter.Name = "labelShortcutsFilter";
            this.labelShortcutsFilter.Size = new System.Drawing.Size(31, 13);
            this.labelShortcutsFilter.TabIndex = 38;
            this.labelShortcutsFilter.Text = "Filter";
            // 
            // buttonShortcutsClear
            // 
            this.buttonShortcutsClear.Enabled = false;
            this.buttonShortcutsClear.Location = new System.Drawing.Point(221, 18);
            this.buttonShortcutsClear.Name = "buttonShortcutsClear";
            this.buttonShortcutsClear.Size = new System.Drawing.Size(111, 23);
            this.buttonShortcutsClear.TabIndex = 1;
            this.buttonShortcutsClear.Text = "Clear";
            this.buttonShortcutsClear.UseVisualStyleBackColor = true;
            this.buttonShortcutsClear.Click += new System.EventHandler(this.buttonShortcutsClear_Click);
            // 
            // labelShortcutsSearch
            // 
            this.labelShortcutsSearch.AutoSize = true;
            this.labelShortcutsSearch.Location = new System.Drawing.Point(18, 23);
            this.labelShortcutsSearch.Name = "labelShortcutsSearch";
            this.labelShortcutsSearch.Size = new System.Drawing.Size(40, 13);
            this.labelShortcutsSearch.TabIndex = 37;
            this.labelShortcutsSearch.Text = "Search";
            // 
            // textBoxShortcutSearch
            // 
            this.textBoxShortcutSearch.BackColor = System.Drawing.SystemColors.Window;
            this.textBoxShortcutSearch.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxShortcutSearch.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxShortcutSearch.Location = new System.Drawing.Point(64, 20);
            this.textBoxShortcutSearch.Multiline = true;
            this.textBoxShortcutSearch.Name = "textBoxShortcutSearch";
            this.textBoxShortcutSearch.Size = new System.Drawing.Size(151, 21);
            this.textBoxShortcutSearch.TabIndex = 0;
            this.textBoxShortcutSearch.TextChanged += new System.EventHandler(this.textBoxShortcutSearch_TextChanged);
            // 
            // buttonClearShortcut
            // 
            this.buttonClearShortcut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonClearShortcut.Enabled = false;
            this.buttonClearShortcut.Location = new System.Drawing.Point(582, 492);
            this.buttonClearShortcut.Name = "buttonClearShortcut";
            this.buttonClearShortcut.Size = new System.Drawing.Size(111, 23);
            this.buttonClearShortcut.TabIndex = 9;
            this.buttonClearShortcut.Text = "&Clear";
            this.buttonClearShortcut.UseVisualStyleBackColor = true;
            this.buttonClearShortcut.Click += new System.EventHandler(this.buttonClearShortcut_Click);
            // 
            // comboBoxShortcutKey
            // 
            this.comboBoxShortcutKey.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.comboBoxShortcutKey.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxShortcutKey.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxShortcutKey.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxShortcutKey.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxShortcutKey.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxShortcutKey.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxShortcutKey.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxShortcutKey.DropDownHeight = 400;
            this.comboBoxShortcutKey.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxShortcutKey.DropDownWidth = 92;
            this.comboBoxShortcutKey.Enabled = false;
            this.comboBoxShortcutKey.FormattingEnabled = true;
            this.comboBoxShortcutKey.Items.AddRange(new string[] {
            "None",
            "A",
            "B",
            "C",
            "D",
            "E",
            "F",
            "G",
            "H",
            "I",
            "J",
            "K",
            "L",
            "M",
            "N",
            "O",
            "P",
            "Q",
            "R",
            "S",
            "T",
            "U",
            "V",
            "W",
            "X",
            "Y",
            "Z",
            "D0",
            "D1",
            "D2",
            "D3",
            "D4",
            "D5",
            "D6",
            "D7",
            "D8",
            "D9",
            "F1",
            "F2",
            "F3",
            "F4",
            "F5",
            "F6",
            "F7",
            "F8",
            "F9",
            "F10",
            "F11",
            "F12",
            "Delete",
            "Down",
            "Home",
            "End",
            "Escape",
            "Insert",
            "Left",
            "Pause",
            "Return",
            "Right",
            "Space",
            "Tab",
            "Up",
            "Back",
            "NumPad0",
            "NumPad1",
            "NumPad2",
            "NumPad3",
            "NumPad4",
            "NumPad5",
            "NumPad6",
            "NumPad7",
            "NumPad8",
            "NumPad9",
            "PageDown",
            "PageUp",
            "Sleep",
            "Multiply",
            "Add",
            "Separator",
            "Subtract",
            "Decimal",
            "Divide",
            "CapsLock",
            "NumLock",
            "Scroll",
            "VolumeMute",
            "VolumeDown",
            "VolumeUp",
            "MediaNextTrack",
            "MediaPreviousTrack",
            "MediaStop",
            "MediaPlayPause",
            "LaunchMail",
            "SelectMedia",
            "LaunchApplication1",
            "LaunchApplication2",
            "Oem1",
            "Oemplus",
            "Oemcomma",
            "OemMinus",
            "OemPeriod",
            "OemQuestion",
            "OemSemicolon",
            "Oemtilde",
            "OemOpenBrackets",
            "Oem5",
            "Oem6",
            "Oem7",
            "Oem8",
            "OemBackslash",
            "ProcessKey",
            "Packet",
            "Attn",
            "Crsel",
            "Exsel",
            "EraseEof",
            "Play",
            "Zoom",
            "NoName",
            "Pa1",
            "Clear",
            "OemClear",
            "KeyCode",
            "F13",
            "F14",
            "F15",
            "F16",
            "F17",
            "F18",
            "F19",
            "F20",
            "F21",
            "F22",
            "F23",
            "F24"});
            this.comboBoxShortcutKey.Location = new System.Drawing.Point(353, 492);
            this.comboBoxShortcutKey.MaxLength = 32767;
            this.comboBoxShortcutKey.Name = "comboBoxShortcutKey";
            this.comboBoxShortcutKey.SelectedIndex = -1;
            this.comboBoxShortcutKey.SelectedItem = null;
            this.comboBoxShortcutKey.SelectedText = "";
            this.comboBoxShortcutKey.Size = new System.Drawing.Size(92, 21);
            this.comboBoxShortcutKey.TabIndex = 7;
            this.comboBoxShortcutKey.UsePopupWindow = false;
            this.comboBoxShortcutKey.SelectedIndexChanged += new System.EventHandler(this.ValidateShortcut);
            this.comboBoxShortcutKey.KeyDown += new System.Windows.Forms.KeyEventHandler(this.comboBoxShortcutKey_KeyDown);
            // 
            // labelShortcutKey
            // 
            this.labelShortcutKey.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelShortcutKey.AutoSize = true;
            this.labelShortcutKey.Location = new System.Drawing.Point(322, 494);
            this.labelShortcutKey.Name = "labelShortcutKey";
            this.labelShortcutKey.Size = new System.Drawing.Size(25, 13);
            this.labelShortcutKey.TabIndex = 35;
            this.labelShortcutKey.Text = "Key";
            // 
            // checkBoxShortcutsShift
            // 
            this.checkBoxShortcutsShift.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxShortcutsShift.AutoSize = true;
            this.checkBoxShortcutsShift.Enabled = false;
            this.checkBoxShortcutsShift.Location = new System.Drawing.Point(245, 494);
            this.checkBoxShortcutsShift.Name = "checkBoxShortcutsShift";
            this.checkBoxShortcutsShift.Size = new System.Drawing.Size(48, 17);
            this.checkBoxShortcutsShift.TabIndex = 6;
            this.checkBoxShortcutsShift.Text = "Shift";
            this.checkBoxShortcutsShift.UseVisualStyleBackColor = true;
            this.checkBoxShortcutsShift.CheckedChanged += new System.EventHandler(this.ValidateShortcut);
            // 
            // checkBoxShortcutsAlt
            // 
            this.checkBoxShortcutsAlt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxShortcutsAlt.AutoSize = true;
            this.checkBoxShortcutsAlt.Enabled = false;
            this.checkBoxShortcutsAlt.Location = new System.Drawing.Point(176, 494);
            this.checkBoxShortcutsAlt.Name = "checkBoxShortcutsAlt";
            this.checkBoxShortcutsAlt.Size = new System.Drawing.Size(39, 17);
            this.checkBoxShortcutsAlt.TabIndex = 5;
            this.checkBoxShortcutsAlt.Text = "Alt";
            this.checkBoxShortcutsAlt.UseVisualStyleBackColor = true;
            this.checkBoxShortcutsAlt.CheckedChanged += new System.EventHandler(this.ValidateShortcut);
            // 
            // checkBoxShortcutsControl
            // 
            this.checkBoxShortcutsControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxShortcutsControl.AutoSize = true;
            this.checkBoxShortcutsControl.Enabled = false;
            this.checkBoxShortcutsControl.Location = new System.Drawing.Point(89, 494);
            this.checkBoxShortcutsControl.Name = "checkBoxShortcutsControl";
            this.checkBoxShortcutsControl.Size = new System.Drawing.Size(61, 17);
            this.checkBoxShortcutsControl.TabIndex = 4;
            this.checkBoxShortcutsControl.Text = "Control";
            this.checkBoxShortcutsControl.UseVisualStyleBackColor = true;
            this.checkBoxShortcutsControl.CheckedChanged += new System.EventHandler(this.ValidateShortcut);
            // 
            // buttonUpdateShortcut
            // 
            this.buttonUpdateShortcut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonUpdateShortcut.Enabled = false;
            this.buttonUpdateShortcut.Location = new System.Drawing.Point(465, 492);
            this.buttonUpdateShortcut.Name = "buttonUpdateShortcut";
            this.buttonUpdateShortcut.Size = new System.Drawing.Size(111, 23);
            this.buttonUpdateShortcut.TabIndex = 8;
            this.buttonUpdateShortcut.Text = "&Update";
            this.buttonUpdateShortcut.UseVisualStyleBackColor = true;
            this.buttonUpdateShortcut.Click += new System.EventHandler(this.buttonUpdateShortcut_Click);
            // 
            // treeViewShortcuts
            // 
            this.treeViewShortcuts.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeViewShortcuts.ContextMenuStrip = this.contextMenuStripShortcuts;
            this.treeViewShortcuts.HideSelection = false;
            this.treeViewShortcuts.Location = new System.Drawing.Point(16, 47);
            this.treeViewShortcuts.Name = "treeViewShortcuts";
            this.treeViewShortcuts.Size = new System.Drawing.Size(829, 439);
            this.treeViewShortcuts.TabIndex = 3;
            this.treeViewShortcuts.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewShortcuts_AfterSelect);
            // 
            // contextMenuStripShortcuts
            // 
            this.contextMenuStripShortcuts.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemShortcutsCollapse,
            this.toolStripSeparator1,
            this.importShortcutsToolStripMenuItem,
            this.exportShortcutsToolStripMenuItem,
            this.exportAsHtmlToolStripMenuItem});
            this.contextMenuStripShortcuts.Name = "contextMenuStripShortcuts";
            this.contextMenuStripShortcuts.Size = new System.Drawing.Size(160, 98);
            // 
            // toolStripMenuItemShortcutsCollapse
            // 
            this.toolStripMenuItemShortcutsCollapse.Name = "toolStripMenuItemShortcutsCollapse";
            this.toolStripMenuItemShortcutsCollapse.Size = new System.Drawing.Size(159, 22);
            this.toolStripMenuItemShortcutsCollapse.Text = "Collapse";
            this.toolStripMenuItemShortcutsCollapse.Click += new System.EventHandler(this.toolStripMenuItemShortcutsCollapse_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(156, 6);
            // 
            // importShortcutsToolStripMenuItem
            // 
            this.importShortcutsToolStripMenuItem.Name = "importShortcutsToolStripMenuItem";
            this.importShortcutsToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.importShortcutsToolStripMenuItem.Text = "Import...";
            this.importShortcutsToolStripMenuItem.Click += new System.EventHandler(this.importShortcutsToolStripMenuItem_Click);
            // 
            // exportShortcutsToolStripMenuItem
            // 
            this.exportShortcutsToolStripMenuItem.Name = "exportShortcutsToolStripMenuItem";
            this.exportShortcutsToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.exportShortcutsToolStripMenuItem.Text = "Export...";
            this.exportShortcutsToolStripMenuItem.Click += new System.EventHandler(this.exportShortcutsToolStripMenuItem_Click);
            // 
            // exportAsHtmlToolStripMenuItem
            // 
            this.exportAsHtmlToolStripMenuItem.Name = "exportAsHtmlToolStripMenuItem";
            this.exportAsHtmlToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.exportAsHtmlToolStripMenuItem.Text = "Export as html...";
            this.exportAsHtmlToolStripMenuItem.Click += new System.EventHandler(this.exportAsHtmlToolStripMenuItem_Click);
            // 
            // labelShortcut
            // 
            this.labelShortcut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelShortcut.AutoSize = true;
            this.labelShortcut.Location = new System.Drawing.Point(15, 494);
            this.labelShortcut.Name = "labelShortcut";
            this.labelShortcut.Size = new System.Drawing.Size(52, 13);
            this.labelShortcut.TabIndex = 3;
            this.labelShortcut.Text = "Shortcut:";
            // 
            // panelSyntaxColoring
            // 
            this.panelSyntaxColoring.Controls.Add(this.groupBoxListViewSyntaxColoring);
            this.panelSyntaxColoring.Location = new System.Drawing.Point(230, 6);
            this.panelSyntaxColoring.Name = "panelSyntaxColoring";
            this.panelSyntaxColoring.Padding = new System.Windows.Forms.Padding(3);
            this.panelSyntaxColoring.Size = new System.Drawing.Size(864, 521);
            this.panelSyntaxColoring.TabIndex = 4;
            this.panelSyntaxColoring.Text = "Syntax coloring";
            // 
            // groupBoxListViewSyntaxColoring
            // 
            this.groupBoxListViewSyntaxColoring.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxListViewSyntaxColoring.Controls.Add(this.buttonLineWidthSettings);
            this.groupBoxListViewSyntaxColoring.Controls.Add(this.checkBoxSyntaxColorTextTooWide);
            this.groupBoxListViewSyntaxColoring.Controls.Add(this.checkBoxSyntaxColorGapTooSmall);
            this.groupBoxListViewSyntaxColoring.Controls.Add(this.checkBoxSyntaxColorTextMoreThanTwoLines);
            this.groupBoxListViewSyntaxColoring.Controls.Add(this.checkBoxSyntaxOverlap);
            this.groupBoxListViewSyntaxColoring.Controls.Add(this.checkBoxSyntaxColorDurationTooSmall);
            this.groupBoxListViewSyntaxColoring.Controls.Add(this.buttonListViewSyntaxColorError);
            this.groupBoxListViewSyntaxColoring.Controls.Add(this.checkBoxSyntaxColorTextTooLong);
            this.groupBoxListViewSyntaxColoring.Controls.Add(this.checkBoxSyntaxColorDurationTooLarge);
            this.groupBoxListViewSyntaxColoring.Controls.Add(this.panelListViewSyntaxColorError);
            this.groupBoxListViewSyntaxColoring.Location = new System.Drawing.Point(0, 0);
            this.groupBoxListViewSyntaxColoring.Name = "groupBoxListViewSyntaxColoring";
            this.groupBoxListViewSyntaxColoring.Size = new System.Drawing.Size(852, 521);
            this.groupBoxListViewSyntaxColoring.TabIndex = 0;
            this.groupBoxListViewSyntaxColoring.TabStop = false;
            this.groupBoxListViewSyntaxColoring.Text = "List view syntax coloring";
            // 
            // buttonLineWidthSettings
            // 
            this.buttonLineWidthSettings.Location = new System.Drawing.Point(160, 112);
            this.buttonLineWidthSettings.Name = "buttonLineWidthSettings";
            this.buttonLineWidthSettings.Size = new System.Drawing.Size(112, 23);
            this.buttonLineWidthSettings.TabIndex = 4;
            this.buttonLineWidthSettings.Text = "Settings...";
            this.buttonLineWidthSettings.UseVisualStyleBackColor = true;
            this.buttonLineWidthSettings.Click += new System.EventHandler(this.buttonLineWidthSettings_Click);
            // 
            // checkBoxSyntaxColorTextTooWide
            // 
            this.checkBoxSyntaxColorTextTooWide.AutoSize = true;
            this.checkBoxSyntaxColorTextTooWide.Location = new System.Drawing.Point(20, 116);
            this.checkBoxSyntaxColorTextTooWide.Name = "checkBoxSyntaxColorTextTooWide";
            this.checkBoxSyntaxColorTextTooWide.Size = new System.Drawing.Size(134, 17);
            this.checkBoxSyntaxColorTextTooWide.TabIndex = 3;
            this.checkBoxSyntaxColorTextTooWide.Text = "Text - color if too wide";
            this.checkBoxSyntaxColorTextTooWide.UseVisualStyleBackColor = true;
            // 
            // checkBoxSyntaxColorGapTooSmall
            // 
            this.checkBoxSyntaxColorGapTooSmall.AutoSize = true;
            this.checkBoxSyntaxColorGapTooSmall.Location = new System.Drawing.Point(20, 209);
            this.checkBoxSyntaxColorGapTooSmall.Name = "checkBoxSyntaxColorGapTooSmall";
            this.checkBoxSyntaxColorGapTooSmall.Size = new System.Drawing.Size(132, 17);
            this.checkBoxSyntaxColorGapTooSmall.TabIndex = 7;
            this.checkBoxSyntaxColorGapTooSmall.Text = "Gap - color if too small";
            this.checkBoxSyntaxColorGapTooSmall.UseVisualStyleBackColor = true;
            // 
            // checkBoxSyntaxColorTextMoreThanTwoLines
            // 
            this.checkBoxSyntaxColorTextMoreThanTwoLines.AutoSize = true;
            this.checkBoxSyntaxColorTextMoreThanTwoLines.Location = new System.Drawing.Point(20, 139);
            this.checkBoxSyntaxColorTextMoreThanTwoLines.Name = "checkBoxSyntaxColorTextMoreThanTwoLines";
            this.checkBoxSyntaxColorTextMoreThanTwoLines.Size = new System.Drawing.Size(170, 17);
            this.checkBoxSyntaxColorTextMoreThanTwoLines.TabIndex = 5;
            this.checkBoxSyntaxColorTextMoreThanTwoLines.Text = "Text - color if more than lines:";
            this.checkBoxSyntaxColorTextMoreThanTwoLines.UseVisualStyleBackColor = true;
            // 
            // checkBoxSyntaxOverlap
            // 
            this.checkBoxSyntaxOverlap.AutoSize = true;
            this.checkBoxSyntaxOverlap.Location = new System.Drawing.Point(20, 174);
            this.checkBoxSyntaxOverlap.Name = "checkBoxSyntaxOverlap";
            this.checkBoxSyntaxOverlap.Size = new System.Drawing.Size(129, 17);
            this.checkBoxSyntaxOverlap.TabIndex = 6;
            this.checkBoxSyntaxOverlap.Text = "Time - color if overlap";
            this.checkBoxSyntaxOverlap.UseVisualStyleBackColor = true;
            // 
            // checkBoxSyntaxColorDurationTooSmall
            // 
            this.checkBoxSyntaxColorDurationTooSmall.AutoSize = true;
            this.checkBoxSyntaxColorDurationTooSmall.Location = new System.Drawing.Point(20, 35);
            this.checkBoxSyntaxColorDurationTooSmall.Name = "checkBoxSyntaxColorDurationTooSmall";
            this.checkBoxSyntaxColorDurationTooSmall.Size = new System.Drawing.Size(154, 17);
            this.checkBoxSyntaxColorDurationTooSmall.TabIndex = 0;
            this.checkBoxSyntaxColorDurationTooSmall.Text = "Duration - color if too small";
            this.checkBoxSyntaxColorDurationTooSmall.UseVisualStyleBackColor = true;
            // 
            // buttonListViewSyntaxColorError
            // 
            this.buttonListViewSyntaxColorError.Location = new System.Drawing.Point(20, 247);
            this.buttonListViewSyntaxColorError.Name = "buttonListViewSyntaxColorError";
            this.buttonListViewSyntaxColorError.Size = new System.Drawing.Size(112, 23);
            this.buttonListViewSyntaxColorError.TabIndex = 8;
            this.buttonListViewSyntaxColorError.Text = "Error color";
            this.buttonListViewSyntaxColorError.UseVisualStyleBackColor = true;
            this.buttonListViewSyntaxColorError.Click += new System.EventHandler(this.buttonListViewSyntaxColorError_Click);
            // 
            // checkBoxSyntaxColorTextTooLong
            // 
            this.checkBoxSyntaxColorTextTooLong.AutoSize = true;
            this.checkBoxSyntaxColorTextTooLong.Location = new System.Drawing.Point(20, 93);
            this.checkBoxSyntaxColorTextTooLong.Name = "checkBoxSyntaxColorTextTooLong";
            this.checkBoxSyntaxColorTextTooLong.Size = new System.Drawing.Size(132, 17);
            this.checkBoxSyntaxColorTextTooLong.TabIndex = 2;
            this.checkBoxSyntaxColorTextTooLong.Text = "Text - color if too long";
            this.checkBoxSyntaxColorTextTooLong.UseVisualStyleBackColor = true;
            // 
            // checkBoxSyntaxColorDurationTooLarge
            // 
            this.checkBoxSyntaxColorDurationTooLarge.AutoSize = true;
            this.checkBoxSyntaxColorDurationTooLarge.Location = new System.Drawing.Point(20, 58);
            this.checkBoxSyntaxColorDurationTooLarge.Name = "checkBoxSyntaxColorDurationTooLarge";
            this.checkBoxSyntaxColorDurationTooLarge.Size = new System.Drawing.Size(155, 17);
            this.checkBoxSyntaxColorDurationTooLarge.TabIndex = 1;
            this.checkBoxSyntaxColorDurationTooLarge.Text = "Duration - color if too large";
            this.checkBoxSyntaxColorDurationTooLarge.UseVisualStyleBackColor = true;
            // 
            // panelListViewSyntaxColorError
            // 
            this.panelListViewSyntaxColorError.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelListViewSyntaxColorError.Location = new System.Drawing.Point(142, 247);
            this.panelListViewSyntaxColorError.Name = "panelListViewSyntaxColorError";
            this.panelListViewSyntaxColorError.Size = new System.Drawing.Size(21, 20);
            this.panelListViewSyntaxColorError.TabIndex = 8;
            this.panelListViewSyntaxColorError.Click += new System.EventHandler(this.buttonListViewSyntaxColorError_Click);
            // 
            // panelVideoPlayer
            // 
            this.panelVideoPlayer.Controls.Add(this.groupBoxMainWindowVideoControls);
            this.panelVideoPlayer.Controls.Add(this.groupBoxVideoPlayerDefault);
            this.panelVideoPlayer.Controls.Add(this.groupBoxVideoEngine);
            this.panelVideoPlayer.Location = new System.Drawing.Point(230, 6);
            this.panelVideoPlayer.Name = "panelVideoPlayer";
            this.panelVideoPlayer.Padding = new System.Windows.Forms.Padding(3);
            this.panelVideoPlayer.Size = new System.Drawing.Size(864, 521);
            this.panelVideoPlayer.TabIndex = 5;
            this.panelVideoPlayer.Text = "Video player";
            // 
            // groupBoxMainWindowVideoControls
            // 
            this.groupBoxMainWindowVideoControls.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxMainWindowVideoControls.Controls.Add(this.labelCustomSearch5);
            this.groupBoxMainWindowVideoControls.Controls.Add(this.textBoxCustomSearchUrl5);
            this.groupBoxMainWindowVideoControls.Controls.Add(this.comboBoxCustomSearch5);
            this.groupBoxMainWindowVideoControls.Controls.Add(this.labelCustomSearch4);
            this.groupBoxMainWindowVideoControls.Controls.Add(this.textBoxCustomSearchUrl4);
            this.groupBoxMainWindowVideoControls.Controls.Add(this.comboBoxCustomSearch4);
            this.groupBoxMainWindowVideoControls.Controls.Add(this.labelCustomSearch3);
            this.groupBoxMainWindowVideoControls.Controls.Add(this.textBoxCustomSearchUrl3);
            this.groupBoxMainWindowVideoControls.Controls.Add(this.comboBoxCustomSearch3);
            this.groupBoxMainWindowVideoControls.Controls.Add(this.labelCustomSearch2);
            this.groupBoxMainWindowVideoControls.Controls.Add(this.textBoxCustomSearchUrl2);
            this.groupBoxMainWindowVideoControls.Controls.Add(this.comboBoxCustomSearch2);
            this.groupBoxMainWindowVideoControls.Controls.Add(this.labelCustomSearch1);
            this.groupBoxMainWindowVideoControls.Controls.Add(this.textBoxCustomSearchUrl1);
            this.groupBoxMainWindowVideoControls.Controls.Add(this.labelCustomSearch);
            this.groupBoxMainWindowVideoControls.Controls.Add(this.comboBoxCustomSearch1);
            this.groupBoxMainWindowVideoControls.Location = new System.Drawing.Point(0, 345);
            this.groupBoxMainWindowVideoControls.Name = "groupBoxMainWindowVideoControls";
            this.groupBoxMainWindowVideoControls.Size = new System.Drawing.Size(851, 175);
            this.groupBoxMainWindowVideoControls.TabIndex = 15;
            this.groupBoxMainWindowVideoControls.TabStop = false;
            this.groupBoxMainWindowVideoControls.Text = "Main window video controls";
            // 
            // labelCustomSearch5
            // 
            this.labelCustomSearch5.AutoSize = true;
            this.labelCustomSearch5.Location = new System.Drawing.Point(12, 149);
            this.labelCustomSearch5.Name = "labelCustomSearch5";
            this.labelCustomSearch5.Size = new System.Drawing.Size(13, 13);
            this.labelCustomSearch5.TabIndex = 15;
            this.labelCustomSearch5.Text = "5";
            // 
            // textBoxCustomSearchUrl5
            // 
            this.textBoxCustomSearchUrl5.BackColor = System.Drawing.SystemColors.Window;
            this.textBoxCustomSearchUrl5.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxCustomSearchUrl5.Location = new System.Drawing.Point(185, 146);
            this.textBoxCustomSearchUrl5.Multiline = true;
            this.textBoxCustomSearchUrl5.Name = "textBoxCustomSearchUrl5";
            this.textBoxCustomSearchUrl5.Size = new System.Drawing.Size(574, 21);
            this.textBoxCustomSearchUrl5.TabIndex = 14;
            // 
            // comboBoxCustomSearch5
            // 
            this.comboBoxCustomSearch5.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxCustomSearch5.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxCustomSearch5.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxCustomSearch5.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxCustomSearch5.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxCustomSearch5.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxCustomSearch5.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxCustomSearch5.DropDownHeight = 400;
            this.comboBoxCustomSearch5.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.comboBoxCustomSearch5.DropDownWidth = 148;
            this.comboBoxCustomSearch5.FormattingEnabled = true;
            this.comboBoxCustomSearch5.Items.AddRange(new string[] {
            "Dictionary.com",
            "learnersdictionary.com",
            "Merriam-Webster",
            "The Free Dictionary",
            "Thesaurus.com",
            "urbandictionary.com",
            "VISUWORDS",
            "Wikipedia"});
            this.comboBoxCustomSearch5.Location = new System.Drawing.Point(31, 146);
            this.comboBoxCustomSearch5.MaxLength = 32767;
            this.comboBoxCustomSearch5.Name = "comboBoxCustomSearch5";
            this.comboBoxCustomSearch5.SelectedIndex = -1;
            this.comboBoxCustomSearch5.SelectedItem = null;
            this.comboBoxCustomSearch5.SelectedText = "";
            this.comboBoxCustomSearch5.Size = new System.Drawing.Size(148, 21);
            this.comboBoxCustomSearch5.TabIndex = 13;
            this.comboBoxCustomSearch5.TabStop = false;
            this.comboBoxCustomSearch5.UsePopupWindow = false;
            this.comboBoxCustomSearch5.SelectedIndexChanged += new System.EventHandler(this.comboBoxCustomSearch_SelectedIndexChanged);
            // 
            // labelCustomSearch4
            // 
            this.labelCustomSearch4.AutoSize = true;
            this.labelCustomSearch4.Location = new System.Drawing.Point(12, 122);
            this.labelCustomSearch4.Name = "labelCustomSearch4";
            this.labelCustomSearch4.Size = new System.Drawing.Size(13, 13);
            this.labelCustomSearch4.TabIndex = 12;
            this.labelCustomSearch4.Text = "4";
            // 
            // textBoxCustomSearchUrl4
            // 
            this.textBoxCustomSearchUrl4.BackColor = System.Drawing.SystemColors.Window;
            this.textBoxCustomSearchUrl4.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxCustomSearchUrl4.Location = new System.Drawing.Point(185, 119);
            this.textBoxCustomSearchUrl4.Multiline = true;
            this.textBoxCustomSearchUrl4.Name = "textBoxCustomSearchUrl4";
            this.textBoxCustomSearchUrl4.Size = new System.Drawing.Size(574, 21);
            this.textBoxCustomSearchUrl4.TabIndex = 11;
            // 
            // comboBoxCustomSearch4
            // 
            this.comboBoxCustomSearch4.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxCustomSearch4.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxCustomSearch4.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxCustomSearch4.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxCustomSearch4.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxCustomSearch4.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxCustomSearch4.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxCustomSearch4.DropDownHeight = 400;
            this.comboBoxCustomSearch4.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.comboBoxCustomSearch4.DropDownWidth = 148;
            this.comboBoxCustomSearch4.FormattingEnabled = true;
            this.comboBoxCustomSearch4.Items.AddRange(new string[] {
            "Dictionary.com",
            "learnersdictionary.com",
            "Merriam-Webster",
            "The Free Dictionary",
            "Thesaurus.com",
            "urbandictionary.com",
            "VISUWORDS",
            "Wikipedia"});
            this.comboBoxCustomSearch4.Location = new System.Drawing.Point(31, 119);
            this.comboBoxCustomSearch4.MaxLength = 32767;
            this.comboBoxCustomSearch4.Name = "comboBoxCustomSearch4";
            this.comboBoxCustomSearch4.SelectedIndex = -1;
            this.comboBoxCustomSearch4.SelectedItem = null;
            this.comboBoxCustomSearch4.SelectedText = "";
            this.comboBoxCustomSearch4.Size = new System.Drawing.Size(148, 21);
            this.comboBoxCustomSearch4.TabIndex = 10;
            this.comboBoxCustomSearch4.TabStop = false;
            this.comboBoxCustomSearch4.UsePopupWindow = false;
            this.comboBoxCustomSearch4.SelectedIndexChanged += new System.EventHandler(this.comboBoxCustomSearch_SelectedIndexChanged);
            // 
            // labelCustomSearch3
            // 
            this.labelCustomSearch3.AutoSize = true;
            this.labelCustomSearch3.Location = new System.Drawing.Point(12, 95);
            this.labelCustomSearch3.Name = "labelCustomSearch3";
            this.labelCustomSearch3.Size = new System.Drawing.Size(13, 13);
            this.labelCustomSearch3.TabIndex = 9;
            this.labelCustomSearch3.Text = "3";
            // 
            // textBoxCustomSearchUrl3
            // 
            this.textBoxCustomSearchUrl3.BackColor = System.Drawing.SystemColors.Window;
            this.textBoxCustomSearchUrl3.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxCustomSearchUrl3.Location = new System.Drawing.Point(185, 92);
            this.textBoxCustomSearchUrl3.Multiline = true;
            this.textBoxCustomSearchUrl3.Name = "textBoxCustomSearchUrl3";
            this.textBoxCustomSearchUrl3.Size = new System.Drawing.Size(574, 21);
            this.textBoxCustomSearchUrl3.TabIndex = 8;
            // 
            // comboBoxCustomSearch3
            // 
            this.comboBoxCustomSearch3.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxCustomSearch3.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxCustomSearch3.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxCustomSearch3.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxCustomSearch3.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxCustomSearch3.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxCustomSearch3.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxCustomSearch3.DropDownHeight = 400;
            this.comboBoxCustomSearch3.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.comboBoxCustomSearch3.DropDownWidth = 148;
            this.comboBoxCustomSearch3.FormattingEnabled = true;
            this.comboBoxCustomSearch3.Items.AddRange(new string[] {
            "Dictionary.com",
            "learnersdictionary.com",
            "Merriam-Webster",
            "The Free Dictionary",
            "Thesaurus.com",
            "urbandictionary.com",
            "VISUWORDS",
            "Wikipedia"});
            this.comboBoxCustomSearch3.Location = new System.Drawing.Point(31, 92);
            this.comboBoxCustomSearch3.MaxLength = 32767;
            this.comboBoxCustomSearch3.Name = "comboBoxCustomSearch3";
            this.comboBoxCustomSearch3.SelectedIndex = -1;
            this.comboBoxCustomSearch3.SelectedItem = null;
            this.comboBoxCustomSearch3.SelectedText = "";
            this.comboBoxCustomSearch3.Size = new System.Drawing.Size(148, 21);
            this.comboBoxCustomSearch3.TabIndex = 7;
            this.comboBoxCustomSearch3.TabStop = false;
            this.comboBoxCustomSearch3.UsePopupWindow = false;
            this.comboBoxCustomSearch3.SelectedIndexChanged += new System.EventHandler(this.comboBoxCustomSearch_SelectedIndexChanged);
            // 
            // labelCustomSearch2
            // 
            this.labelCustomSearch2.AutoSize = true;
            this.labelCustomSearch2.Location = new System.Drawing.Point(12, 68);
            this.labelCustomSearch2.Name = "labelCustomSearch2";
            this.labelCustomSearch2.Size = new System.Drawing.Size(13, 13);
            this.labelCustomSearch2.TabIndex = 6;
            this.labelCustomSearch2.Text = "2";
            // 
            // textBoxCustomSearchUrl2
            // 
            this.textBoxCustomSearchUrl2.BackColor = System.Drawing.SystemColors.Window;
            this.textBoxCustomSearchUrl2.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxCustomSearchUrl2.Location = new System.Drawing.Point(185, 65);
            this.textBoxCustomSearchUrl2.Multiline = true;
            this.textBoxCustomSearchUrl2.Name = "textBoxCustomSearchUrl2";
            this.textBoxCustomSearchUrl2.Size = new System.Drawing.Size(574, 21);
            this.textBoxCustomSearchUrl2.TabIndex = 5;
            // 
            // comboBoxCustomSearch2
            // 
            this.comboBoxCustomSearch2.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxCustomSearch2.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxCustomSearch2.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxCustomSearch2.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxCustomSearch2.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxCustomSearch2.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxCustomSearch2.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxCustomSearch2.DropDownHeight = 400;
            this.comboBoxCustomSearch2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.comboBoxCustomSearch2.DropDownWidth = 148;
            this.comboBoxCustomSearch2.FormattingEnabled = true;
            this.comboBoxCustomSearch2.Items.AddRange(new string[] {
            "Dictionary.com",
            "learnersdictionary.com",
            "Merriam-Webster",
            "The Free Dictionary",
            "Thesaurus.com",
            "urbandictionary.com",
            "VISUWORDS",
            "Wikipedia"});
            this.comboBoxCustomSearch2.Location = new System.Drawing.Point(31, 65);
            this.comboBoxCustomSearch2.MaxLength = 32767;
            this.comboBoxCustomSearch2.Name = "comboBoxCustomSearch2";
            this.comboBoxCustomSearch2.SelectedIndex = -1;
            this.comboBoxCustomSearch2.SelectedItem = null;
            this.comboBoxCustomSearch2.SelectedText = "";
            this.comboBoxCustomSearch2.Size = new System.Drawing.Size(148, 21);
            this.comboBoxCustomSearch2.TabIndex = 4;
            this.comboBoxCustomSearch2.TabStop = false;
            this.comboBoxCustomSearch2.UsePopupWindow = false;
            this.comboBoxCustomSearch2.SelectedIndexChanged += new System.EventHandler(this.comboBoxCustomSearch_SelectedIndexChanged);
            // 
            // labelCustomSearch1
            // 
            this.labelCustomSearch1.AutoSize = true;
            this.labelCustomSearch1.Location = new System.Drawing.Point(12, 41);
            this.labelCustomSearch1.Name = "labelCustomSearch1";
            this.labelCustomSearch1.Size = new System.Drawing.Size(13, 13);
            this.labelCustomSearch1.TabIndex = 3;
            this.labelCustomSearch1.Text = "1";
            // 
            // textBoxCustomSearchUrl1
            // 
            this.textBoxCustomSearchUrl1.BackColor = System.Drawing.SystemColors.Window;
            this.textBoxCustomSearchUrl1.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxCustomSearchUrl1.Location = new System.Drawing.Point(185, 38);
            this.textBoxCustomSearchUrl1.Multiline = true;
            this.textBoxCustomSearchUrl1.Name = "textBoxCustomSearchUrl1";
            this.textBoxCustomSearchUrl1.Size = new System.Drawing.Size(574, 21);
            this.textBoxCustomSearchUrl1.TabIndex = 2;
            // 
            // labelCustomSearch
            // 
            this.labelCustomSearch.AutoSize = true;
            this.labelCustomSearch.Location = new System.Drawing.Point(12, 20);
            this.labelCustomSearch.Name = "labelCustomSearch";
            this.labelCustomSearch.Size = new System.Drawing.Size(144, 13);
            this.labelCustomSearch.TabIndex = 1;
            this.labelCustomSearch.Text = "Custom search text and URL";
            // 
            // comboBoxCustomSearch1
            // 
            this.comboBoxCustomSearch1.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxCustomSearch1.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxCustomSearch1.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxCustomSearch1.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxCustomSearch1.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxCustomSearch1.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxCustomSearch1.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxCustomSearch1.DropDownHeight = 400;
            this.comboBoxCustomSearch1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.comboBoxCustomSearch1.DropDownWidth = 148;
            this.comboBoxCustomSearch1.FormattingEnabled = true;
            this.comboBoxCustomSearch1.Items.AddRange(new string[] {
            "Dictionary.com",
            "learnersdictionary.com",
            "Merriam-Webster",
            "The Free Dictionary",
            "Thesaurus.com",
            "urbandictionary.com",
            "VISUWORDS",
            "Wikipedia"});
            this.comboBoxCustomSearch1.Location = new System.Drawing.Point(31, 38);
            this.comboBoxCustomSearch1.MaxLength = 32767;
            this.comboBoxCustomSearch1.Name = "comboBoxCustomSearch1";
            this.comboBoxCustomSearch1.SelectedIndex = -1;
            this.comboBoxCustomSearch1.SelectedItem = null;
            this.comboBoxCustomSearch1.SelectedText = "";
            this.comboBoxCustomSearch1.Size = new System.Drawing.Size(148, 21);
            this.comboBoxCustomSearch1.TabIndex = 0;
            this.comboBoxCustomSearch1.TabStop = false;
            this.comboBoxCustomSearch1.UsePopupWindow = false;
            this.comboBoxCustomSearch1.SelectedIndexChanged += new System.EventHandler(this.comboBoxCustomSearch_SelectedIndexChanged);
            // 
            // groupBoxVideoPlayerDefault
            // 
            this.groupBoxVideoPlayerDefault.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxVideoPlayerDefault.Controls.Add(this.numericUpDownMarginVertical);
            this.groupBoxVideoPlayerDefault.Controls.Add(this.labelMarginVertical);
            this.groupBoxVideoPlayerDefault.Controls.Add(this.panelMpvBackColor);
            this.groupBoxVideoPlayerDefault.Controls.Add(this.buttonMpvBackColor);
            this.groupBoxVideoPlayerDefault.Controls.Add(this.panelMpvOutlineColor);
            this.groupBoxVideoPlayerDefault.Controls.Add(this.buttonMpvOutlineColor);
            this.groupBoxVideoPlayerDefault.Controls.Add(this.panelMpvPrimaryColor);
            this.groupBoxVideoPlayerDefault.Controls.Add(this.buttonMpvPrimaryColor);
            this.groupBoxVideoPlayerDefault.Controls.Add(this.groupBoxMpvBorder);
            this.groupBoxVideoPlayerDefault.Controls.Add(this.checkBoxAllowVolumeBoost);
            this.groupBoxVideoPlayerDefault.Controls.Add(this.checkBoxVideoAutoOpen);
            this.groupBoxVideoPlayerDefault.Controls.Add(this.checkBoxVideoPlayerPreviewFontBold);
            this.groupBoxVideoPlayerDefault.Controls.Add(this.checkBoxVideoPlayerShowFullscreenButton);
            this.groupBoxVideoPlayerDefault.Controls.Add(this.checkBoxVideoPlayerShowMuteButton);
            this.groupBoxVideoPlayerDefault.Controls.Add(this.labelVideoPlayerPreviewFontName);
            this.groupBoxVideoPlayerDefault.Controls.Add(this.comboBoxVideoPlayerPreviewFontName);
            this.groupBoxVideoPlayerDefault.Controls.Add(this.labelVideoPlayerPreviewFontSize);
            this.groupBoxVideoPlayerDefault.Controls.Add(this.comboBoxlVideoPlayerPreviewFontSize);
            this.groupBoxVideoPlayerDefault.Controls.Add(this.checkBoxVideoPlayerShowStopButton);
            this.groupBoxVideoPlayerDefault.Location = new System.Drawing.Point(0, 160);
            this.groupBoxVideoPlayerDefault.Name = "groupBoxVideoPlayerDefault";
            this.groupBoxVideoPlayerDefault.Size = new System.Drawing.Size(851, 182);
            this.groupBoxVideoPlayerDefault.TabIndex = 14;
            this.groupBoxVideoPlayerDefault.TabStop = false;
            // 
            // numericUpDownMarginVertical
            // 
            this.numericUpDownMarginVertical.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownMarginVertical.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownMarginVertical.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownMarginVertical.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownMarginVertical.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownMarginVertical.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownMarginVertical.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownMarginVertical.DecimalPlaces = 0;
            this.numericUpDownMarginVertical.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownMarginVertical.Location = new System.Drawing.Point(361, 153);
            this.numericUpDownMarginVertical.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownMarginVertical.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.numericUpDownMarginVertical.Name = "numericUpDownMarginVertical";
            this.numericUpDownMarginVertical.Size = new System.Drawing.Size(44, 21);
            this.numericUpDownMarginVertical.TabIndex = 21;
            this.numericUpDownMarginVertical.TabStop = false;
            this.numericUpDownMarginVertical.ThousandsSeparator = false;
            this.numericUpDownMarginVertical.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            // 
            // labelMarginVertical
            // 
            this.labelMarginVertical.AutoSize = true;
            this.labelMarginVertical.Location = new System.Drawing.Point(306, 155);
            this.labelMarginVertical.Name = "labelMarginVertical";
            this.labelMarginVertical.Size = new System.Drawing.Size(42, 13);
            this.labelMarginVertical.TabIndex = 20;
            this.labelMarginVertical.Text = "Vertical";
            // 
            // panelMpvBackColor
            // 
            this.panelMpvBackColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelMpvBackColor.Location = new System.Drawing.Point(401, 125);
            this.panelMpvBackColor.Name = "panelMpvBackColor";
            this.panelMpvBackColor.Size = new System.Drawing.Size(21, 20);
            this.panelMpvBackColor.TabIndex = 19;
            this.panelMpvBackColor.Click += new System.EventHandler(this.buttonMpvBackColor_Click);
            // 
            // buttonMpvBackColor
            // 
            this.buttonMpvBackColor.Location = new System.Drawing.Point(306, 124);
            this.buttonMpvBackColor.Name = "buttonMpvBackColor";
            this.buttonMpvBackColor.Size = new System.Drawing.Size(89, 23);
            this.buttonMpvBackColor.TabIndex = 18;
            this.buttonMpvBackColor.Text = "Shadow";
            this.buttonMpvBackColor.UseVisualStyleBackColor = true;
            this.buttonMpvBackColor.Click += new System.EventHandler(this.buttonMpvBackColor_Click);
            // 
            // panelMpvOutlineColor
            // 
            this.panelMpvOutlineColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelMpvOutlineColor.Location = new System.Drawing.Point(401, 96);
            this.panelMpvOutlineColor.Name = "panelMpvOutlineColor";
            this.panelMpvOutlineColor.Size = new System.Drawing.Size(21, 20);
            this.panelMpvOutlineColor.TabIndex = 17;
            this.panelMpvOutlineColor.Click += new System.EventHandler(this.buttonMpvOutlineColor_Click);
            // 
            // buttonMpvOutlineColor
            // 
            this.buttonMpvOutlineColor.Location = new System.Drawing.Point(306, 95);
            this.buttonMpvOutlineColor.Name = "buttonMpvOutlineColor";
            this.buttonMpvOutlineColor.Size = new System.Drawing.Size(89, 23);
            this.buttonMpvOutlineColor.TabIndex = 16;
            this.buttonMpvOutlineColor.Text = "Outline";
            this.buttonMpvOutlineColor.UseVisualStyleBackColor = true;
            this.buttonMpvOutlineColor.Click += new System.EventHandler(this.buttonMpvOutlineColor_Click);
            // 
            // panelMpvPrimaryColor
            // 
            this.panelMpvPrimaryColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelMpvPrimaryColor.Location = new System.Drawing.Point(401, 67);
            this.panelMpvPrimaryColor.Name = "panelMpvPrimaryColor";
            this.panelMpvPrimaryColor.Size = new System.Drawing.Size(21, 20);
            this.panelMpvPrimaryColor.TabIndex = 15;
            this.panelMpvPrimaryColor.Click += new System.EventHandler(this.buttonMpvPrimaryColor_Click);
            // 
            // buttonMpvPrimaryColor
            // 
            this.buttonMpvPrimaryColor.Location = new System.Drawing.Point(306, 66);
            this.buttonMpvPrimaryColor.Name = "buttonMpvPrimaryColor";
            this.buttonMpvPrimaryColor.Size = new System.Drawing.Size(89, 23);
            this.buttonMpvPrimaryColor.TabIndex = 14;
            this.buttonMpvPrimaryColor.Text = "&Primary";
            this.buttonMpvPrimaryColor.UseVisualStyleBackColor = true;
            this.buttonMpvPrimaryColor.Click += new System.EventHandler(this.buttonMpvPrimaryColor_Click);
            // 
            // groupBoxMpvBorder
            // 
            this.groupBoxMpvBorder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxMpvBorder.Controls.Add(this.comboBoxOpaqueBoxStyle);
            this.groupBoxMpvBorder.Controls.Add(this.radioButtonMpvOpaqueBox);
            this.groupBoxMpvBorder.Controls.Add(this.radioButtonMpvOutline);
            this.groupBoxMpvBorder.Controls.Add(this.numericUpDownMpvShadowWidth);
            this.groupBoxMpvBorder.Controls.Add(this.numericUpDownMpvOutline);
            this.groupBoxMpvBorder.Controls.Add(this.labelMpvShadow);
            this.groupBoxMpvBorder.Location = new System.Drawing.Point(447, 68);
            this.groupBoxMpvBorder.Name = "groupBoxMpvBorder";
            this.groupBoxMpvBorder.Size = new System.Drawing.Size(394, 105);
            this.groupBoxMpvBorder.TabIndex = 13;
            this.groupBoxMpvBorder.TabStop = false;
            this.groupBoxMpvBorder.Text = "Border";
            // 
            // comboBoxOpaqueBoxStyle
            // 
            this.comboBoxOpaqueBoxStyle.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxOpaqueBoxStyle.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxOpaqueBoxStyle.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxOpaqueBoxStyle.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxOpaqueBoxStyle.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxOpaqueBoxStyle.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxOpaqueBoxStyle.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxOpaqueBoxStyle.DropDownHeight = 400;
            this.comboBoxOpaqueBoxStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxOpaqueBoxStyle.DropDownWidth = 300;
            this.comboBoxOpaqueBoxStyle.FormattingEnabled = true;
            this.comboBoxOpaqueBoxStyle.Location = new System.Drawing.Point(35, 69);
            this.comboBoxOpaqueBoxStyle.MaxLength = 32767;
            this.comboBoxOpaqueBoxStyle.Name = "comboBoxOpaqueBoxStyle";
            this.comboBoxOpaqueBoxStyle.SelectedIndex = -1;
            this.comboBoxOpaqueBoxStyle.SelectedItem = null;
            this.comboBoxOpaqueBoxStyle.SelectedText = "";
            this.comboBoxOpaqueBoxStyle.Size = new System.Drawing.Size(212, 21);
            this.comboBoxOpaqueBoxStyle.TabIndex = 5;
            this.comboBoxOpaqueBoxStyle.UsePopupWindow = false;
            // 
            // radioButtonMpvOpaqueBox
            // 
            this.radioButtonMpvOpaqueBox.AutoSize = true;
            this.radioButtonMpvOpaqueBox.Location = new System.Drawing.Point(12, 50);
            this.radioButtonMpvOpaqueBox.Name = "radioButtonMpvOpaqueBox";
            this.radioButtonMpvOpaqueBox.Size = new System.Drawing.Size(84, 17);
            this.radioButtonMpvOpaqueBox.TabIndex = 4;
            this.radioButtonMpvOpaqueBox.Text = "Opaque bo&x";
            this.radioButtonMpvOpaqueBox.UseVisualStyleBackColor = false;
            // 
            // radioButtonMpvOutline
            // 
            this.radioButtonMpvOutline.AutoSize = true;
            this.radioButtonMpvOutline.Location = new System.Drawing.Point(12, 24);
            this.radioButtonMpvOutline.Name = "radioButtonMpvOutline";
            this.radioButtonMpvOutline.Size = new System.Drawing.Size(59, 17);
            this.radioButtonMpvOutline.TabIndex = 0;
            this.radioButtonMpvOutline.Text = "Outline";
            this.radioButtonMpvOutline.UseVisualStyleBackColor = true;
            // 
            // numericUpDownMpvShadowWidth
            // 
            this.numericUpDownMpvShadowWidth.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownMpvShadowWidth.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownMpvShadowWidth.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownMpvShadowWidth.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownMpvShadowWidth.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownMpvShadowWidth.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownMpvShadowWidth.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownMpvShadowWidth.DecimalPlaces = 1;
            this.numericUpDownMpvShadowWidth.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericUpDownMpvShadowWidth.Location = new System.Drawing.Point(134, 24);
            this.numericUpDownMpvShadowWidth.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDownMpvShadowWidth.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.numericUpDownMpvShadowWidth.Name = "numericUpDownMpvShadowWidth";
            this.numericUpDownMpvShadowWidth.Size = new System.Drawing.Size(52, 21);
            this.numericUpDownMpvShadowWidth.TabIndex = 2;
            this.numericUpDownMpvShadowWidth.TabStop = false;
            this.numericUpDownMpvShadowWidth.ThousandsSeparator = false;
            this.numericUpDownMpvShadowWidth.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            // 
            // numericUpDownMpvOutline
            // 
            this.numericUpDownMpvOutline.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownMpvOutline.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownMpvOutline.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownMpvOutline.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownMpvOutline.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownMpvOutline.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownMpvOutline.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownMpvOutline.DecimalPlaces = 1;
            this.numericUpDownMpvOutline.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericUpDownMpvOutline.Location = new System.Drawing.Point(76, 24);
            this.numericUpDownMpvOutline.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDownMpvOutline.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.numericUpDownMpvOutline.Name = "numericUpDownMpvOutline";
            this.numericUpDownMpvOutline.Size = new System.Drawing.Size(52, 21);
            this.numericUpDownMpvOutline.TabIndex = 1;
            this.numericUpDownMpvOutline.TabStop = false;
            this.numericUpDownMpvOutline.ThousandsSeparator = false;
            this.numericUpDownMpvOutline.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            // 
            // labelMpvShadow
            // 
            this.labelMpvShadow.AutoSize = true;
            this.labelMpvShadow.Location = new System.Drawing.Point(134, 11);
            this.labelMpvShadow.Name = "labelMpvShadow";
            this.labelMpvShadow.Size = new System.Drawing.Size(45, 13);
            this.labelMpvShadow.TabIndex = 2;
            this.labelMpvShadow.Text = "Shadow";
            // 
            // checkBoxAllowVolumeBoost
            // 
            this.checkBoxAllowVolumeBoost.AutoSize = true;
            this.checkBoxAllowVolumeBoost.Location = new System.Drawing.Point(9, 95);
            this.checkBoxAllowVolumeBoost.Name = "checkBoxAllowVolumeBoost";
            this.checkBoxAllowVolumeBoost.Size = new System.Drawing.Size(118, 17);
            this.checkBoxAllowVolumeBoost.TabIndex = 4;
            this.checkBoxAllowVolumeBoost.Text = "Allow volume boost";
            this.checkBoxAllowVolumeBoost.UseVisualStyleBackColor = true;
            // 
            // checkBoxVideoAutoOpen
            // 
            this.checkBoxVideoAutoOpen.AutoSize = true;
            this.checkBoxVideoAutoOpen.Location = new System.Drawing.Point(9, 75);
            this.checkBoxVideoAutoOpen.Name = "checkBoxVideoAutoOpen";
            this.checkBoxVideoAutoOpen.Size = new System.Drawing.Size(213, 17);
            this.checkBoxVideoAutoOpen.TabIndex = 3;
            this.checkBoxVideoAutoOpen.Text = "Auto open video when opening subtitle";
            this.checkBoxVideoAutoOpen.UseVisualStyleBackColor = true;
            // 
            // checkBoxVideoPlayerPreviewFontBold
            // 
            this.checkBoxVideoPlayerPreviewFontBold.AutoSize = true;
            this.checkBoxVideoPlayerPreviewFontBold.Location = new System.Drawing.Point(641, 17);
            this.checkBoxVideoPlayerPreviewFontBold.Name = "checkBoxVideoPlayerPreviewFontBold";
            this.checkBoxVideoPlayerPreviewFontBold.Size = new System.Drawing.Size(46, 17);
            this.checkBoxVideoPlayerPreviewFontBold.TabIndex = 7;
            this.checkBoxVideoPlayerPreviewFontBold.Text = "Bold";
            this.checkBoxVideoPlayerPreviewFontBold.UseVisualStyleBackColor = true;
            // 
            // checkBoxVideoPlayerShowFullscreenButton
            // 
            this.checkBoxVideoPlayerShowFullscreenButton.AutoSize = true;
            this.checkBoxVideoPlayerShowFullscreenButton.Location = new System.Drawing.Point(9, 55);
            this.checkBoxVideoPlayerShowFullscreenButton.Name = "checkBoxVideoPlayerShowFullscreenButton";
            this.checkBoxVideoPlayerShowFullscreenButton.Size = new System.Drawing.Size(136, 17);
            this.checkBoxVideoPlayerShowFullscreenButton.TabIndex = 2;
            this.checkBoxVideoPlayerShowFullscreenButton.Text = "Show fullscreen button";
            this.checkBoxVideoPlayerShowFullscreenButton.UseVisualStyleBackColor = true;
            // 
            // checkBoxVideoPlayerShowMuteButton
            // 
            this.checkBoxVideoPlayerShowMuteButton.AutoSize = true;
            this.checkBoxVideoPlayerShowMuteButton.Location = new System.Drawing.Point(9, 35);
            this.checkBoxVideoPlayerShowMuteButton.Name = "checkBoxVideoPlayerShowMuteButton";
            this.checkBoxVideoPlayerShowMuteButton.Size = new System.Drawing.Size(114, 17);
            this.checkBoxVideoPlayerShowMuteButton.TabIndex = 1;
            this.checkBoxVideoPlayerShowMuteButton.Text = "Show mute button";
            this.checkBoxVideoPlayerShowMuteButton.UseVisualStyleBackColor = true;
            // 
            // labelVideoPlayerPreviewFontName
            // 
            this.labelVideoPlayerPreviewFontName.AutoSize = true;
            this.labelVideoPlayerPreviewFontName.Location = new System.Drawing.Point(305, 16);
            this.labelVideoPlayerPreviewFontName.Name = "labelVideoPlayerPreviewFontName";
            this.labelVideoPlayerPreviewFontName.Size = new System.Drawing.Size(136, 13);
            this.labelVideoPlayerPreviewFontName.TabIndex = 5;
            this.labelVideoPlayerPreviewFontName.Text = "Subtitle preview font name";
            // 
            // comboBoxVideoPlayerPreviewFontName
            // 
            this.comboBoxVideoPlayerPreviewFontName.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxVideoPlayerPreviewFontName.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxVideoPlayerPreviewFontName.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxVideoPlayerPreviewFontName.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxVideoPlayerPreviewFontName.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxVideoPlayerPreviewFontName.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxVideoPlayerPreviewFontName.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxVideoPlayerPreviewFontName.DropDownHeight = 400;
            this.comboBoxVideoPlayerPreviewFontName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxVideoPlayerPreviewFontName.DropDownWidth = 188;
            this.comboBoxVideoPlayerPreviewFontName.FormattingEnabled = true;
            this.comboBoxVideoPlayerPreviewFontName.Location = new System.Drawing.Point(446, 13);
            this.comboBoxVideoPlayerPreviewFontName.MaxLength = 32767;
            this.comboBoxVideoPlayerPreviewFontName.Name = "comboBoxVideoPlayerPreviewFontName";
            this.comboBoxVideoPlayerPreviewFontName.SelectedIndex = -1;
            this.comboBoxVideoPlayerPreviewFontName.SelectedItem = null;
            this.comboBoxVideoPlayerPreviewFontName.SelectedText = "";
            this.comboBoxVideoPlayerPreviewFontName.Size = new System.Drawing.Size(186, 21);
            this.comboBoxVideoPlayerPreviewFontName.TabIndex = 6;
            this.comboBoxVideoPlayerPreviewFontName.UsePopupWindow = false;
            // 
            // labelVideoPlayerPreviewFontSize
            // 
            this.labelVideoPlayerPreviewFontSize.AutoSize = true;
            this.labelVideoPlayerPreviewFontSize.Location = new System.Drawing.Point(305, 43);
            this.labelVideoPlayerPreviewFontSize.Name = "labelVideoPlayerPreviewFontSize";
            this.labelVideoPlayerPreviewFontSize.Size = new System.Drawing.Size(128, 13);
            this.labelVideoPlayerPreviewFontSize.TabIndex = 7;
            this.labelVideoPlayerPreviewFontSize.Text = "Subtitle preview font size";
            // 
            // comboBoxlVideoPlayerPreviewFontSize
            // 
            this.comboBoxlVideoPlayerPreviewFontSize.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxlVideoPlayerPreviewFontSize.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxlVideoPlayerPreviewFontSize.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxlVideoPlayerPreviewFontSize.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxlVideoPlayerPreviewFontSize.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxlVideoPlayerPreviewFontSize.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxlVideoPlayerPreviewFontSize.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxlVideoPlayerPreviewFontSize.DropDownHeight = 400;
            this.comboBoxlVideoPlayerPreviewFontSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxlVideoPlayerPreviewFontSize.DropDownWidth = 70;
            this.comboBoxlVideoPlayerPreviewFontSize.FormattingEnabled = true;
            this.comboBoxlVideoPlayerPreviewFontSize.Location = new System.Drawing.Point(446, 40);
            this.comboBoxlVideoPlayerPreviewFontSize.MaxLength = 32767;
            this.comboBoxlVideoPlayerPreviewFontSize.Name = "comboBoxlVideoPlayerPreviewFontSize";
            this.comboBoxlVideoPlayerPreviewFontSize.SelectedIndex = -1;
            this.comboBoxlVideoPlayerPreviewFontSize.SelectedItem = null;
            this.comboBoxlVideoPlayerPreviewFontSize.SelectedText = "";
            this.comboBoxlVideoPlayerPreviewFontSize.Size = new System.Drawing.Size(70, 21);
            this.comboBoxlVideoPlayerPreviewFontSize.TabIndex = 7;
            this.comboBoxlVideoPlayerPreviewFontSize.UsePopupWindow = false;
            // 
            // checkBoxVideoPlayerShowStopButton
            // 
            this.checkBoxVideoPlayerShowStopButton.AutoSize = true;
            this.checkBoxVideoPlayerShowStopButton.Location = new System.Drawing.Point(9, 15);
            this.checkBoxVideoPlayerShowStopButton.Name = "checkBoxVideoPlayerShowStopButton";
            this.checkBoxVideoPlayerShowStopButton.Size = new System.Drawing.Size(111, 17);
            this.checkBoxVideoPlayerShowStopButton.TabIndex = 0;
            this.checkBoxVideoPlayerShowStopButton.Text = "Show stop button";
            this.checkBoxVideoPlayerShowStopButton.UseVisualStyleBackColor = true;
            // 
            // groupBoxVideoEngine
            // 
            this.groupBoxVideoEngine.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxVideoEngine.Controls.Add(this.checkBoxMpvHandlesPreviewText);
            this.groupBoxVideoEngine.Controls.Add(this.labelMpvSettings);
            this.groupBoxVideoEngine.Controls.Add(this.buttonMpvSettings);
            this.groupBoxVideoEngine.Controls.Add(this.labelPlatform);
            this.groupBoxVideoEngine.Controls.Add(this.buttonVlcPathBrowse);
            this.groupBoxVideoEngine.Controls.Add(this.textBoxVlcPath);
            this.groupBoxVideoEngine.Controls.Add(this.labelVlcPath);
            this.groupBoxVideoEngine.Controls.Add(this.labelVideoPlayerVLC);
            this.groupBoxVideoEngine.Controls.Add(this.radioButtonVideoPlayerVLC);
            this.groupBoxVideoEngine.Controls.Add(this.labelVideoPlayerMPlayer);
            this.groupBoxVideoEngine.Controls.Add(this.labelDirectShowDescription);
            this.groupBoxVideoEngine.Controls.Add(this.labelMpcHcDescription);
            this.groupBoxVideoEngine.Controls.Add(this.radioButtonVideoPlayerMPV);
            this.groupBoxVideoEngine.Controls.Add(this.radioButtonVideoPlayerDirectShow);
            this.groupBoxVideoEngine.Controls.Add(this.radioButtonVideoPlayerMpcHc);
            this.groupBoxVideoEngine.Location = new System.Drawing.Point(0, 0);
            this.groupBoxVideoEngine.Name = "groupBoxVideoEngine";
            this.groupBoxVideoEngine.Size = new System.Drawing.Size(852, 158);
            this.groupBoxVideoEngine.TabIndex = 0;
            this.groupBoxVideoEngine.TabStop = false;
            this.groupBoxVideoEngine.Text = "Video engine";
            // 
            // checkBoxMpvHandlesPreviewText
            // 
            this.checkBoxMpvHandlesPreviewText.AutoSize = true;
            this.checkBoxMpvHandlesPreviewText.Location = new System.Drawing.Point(164, 43);
            this.checkBoxMpvHandlesPreviewText.Name = "checkBoxMpvHandlesPreviewText";
            this.checkBoxMpvHandlesPreviewText.Size = new System.Drawing.Size(150, 17);
            this.checkBoxMpvHandlesPreviewText.TabIndex = 3;
            this.checkBoxMpvHandlesPreviewText.Text = "mpv handles preview text";
            this.checkBoxMpvHandlesPreviewText.UseVisualStyleBackColor = true;
            this.checkBoxMpvHandlesPreviewText.CheckedChanged += new System.EventHandler(this.checkBoxMpvHandlesPreviewText_CheckedChanged);
            // 
            // labelMpvSettings
            // 
            this.labelMpvSettings.AutoSize = true;
            this.labelMpvSettings.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelMpvSettings.ForeColor = System.Drawing.Color.Gray;
            this.labelMpvSettings.Location = new System.Drawing.Point(709, 26);
            this.labelMpvSettings.Name = "labelMpvSettings";
            this.labelMpvSettings.Size = new System.Drawing.Size(40, 13);
            this.labelMpvSettings.TabIndex = 30;
            this.labelMpvSettings.Text = "--vo=?";
            // 
            // buttonMpvSettings
            // 
            this.buttonMpvSettings.Location = new System.Drawing.Point(510, 22);
            this.buttonMpvSettings.Name = "buttonMpvSettings";
            this.buttonMpvSettings.Size = new System.Drawing.Size(179, 23);
            this.buttonMpvSettings.TabIndex = 2;
            this.buttonMpvSettings.Text = "Download mpv dll";
            this.buttonMpvSettings.UseVisualStyleBackColor = true;
            this.buttonMpvSettings.Click += new System.EventHandler(this.buttonMpvSettings_Click);
            // 
            // labelPlatform
            // 
            this.labelPlatform.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelPlatform.Font = new System.Drawing.Font("Tahoma", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelPlatform.ForeColor = System.Drawing.Color.Gray;
            this.labelPlatform.Location = new System.Drawing.Point(766, 13);
            this.labelPlatform.Name = "labelPlatform";
            this.labelPlatform.Size = new System.Drawing.Size(83, 11);
            this.labelPlatform.TabIndex = 27;
            this.labelPlatform.Text = "x-bit";
            this.labelPlatform.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // buttonVlcPathBrowse
            // 
            this.buttonVlcPathBrowse.Location = new System.Drawing.Point(779, 105);
            this.buttonVlcPathBrowse.Name = "buttonVlcPathBrowse";
            this.buttonVlcPathBrowse.Size = new System.Drawing.Size(29, 21);
            this.buttonVlcPathBrowse.TabIndex = 7;
            this.buttonVlcPathBrowse.Text = "...";
            this.buttonVlcPathBrowse.UseVisualStyleBackColor = true;
            this.buttonVlcPathBrowse.Click += new System.EventHandler(this.buttonVlcPathBrowse_Click);
            // 
            // textBoxVlcPath
            // 
            this.textBoxVlcPath.BackColor = System.Drawing.SystemColors.Window;
            this.textBoxVlcPath.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxVlcPath.Location = new System.Drawing.Point(411, 105);
            this.textBoxVlcPath.MaxLength = 1000;
            this.textBoxVlcPath.Multiline = true;
            this.textBoxVlcPath.Name = "textBoxVlcPath";
            this.textBoxVlcPath.Size = new System.Drawing.Size(362, 21);
            this.textBoxVlcPath.TabIndex = 6;
            this.textBoxVlcPath.MouseLeave += new System.EventHandler(this.textBoxVlcPath_MouseLeave);
            // 
            // labelVlcPath
            // 
            this.labelVlcPath.AutoSize = true;
            this.labelVlcPath.Location = new System.Drawing.Point(414, 89);
            this.labelVlcPath.Name = "labelVlcPath";
            this.labelVlcPath.Size = new System.Drawing.Size(315, 13);
            this.labelVlcPath.TabIndex = 24;
            this.labelVlcPath.Text = "VLC path (only needed if you using the  portable version of VLC)";
            // 
            // labelVideoPlayerVLC
            // 
            this.labelVideoPlayerVLC.AutoSize = true;
            this.labelVideoPlayerVLC.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelVideoPlayerVLC.ForeColor = System.Drawing.Color.Gray;
            this.labelVideoPlayerVLC.Location = new System.Drawing.Point(164, 109);
            this.labelVideoPlayerVLC.Name = "labelVideoPlayerVLC";
            this.labelVideoPlayerVLC.Size = new System.Drawing.Size(237, 13);
            this.labelVideoPlayerVLC.TabIndex = 13;
            this.labelVideoPlayerVLC.Text = "libvlc.dll from VLC media player (1.1.0 or newer)";
            // 
            // radioButtonVideoPlayerVLC
            // 
            this.radioButtonVideoPlayerVLC.AutoSize = true;
            this.radioButtonVideoPlayerVLC.Location = new System.Drawing.Point(9, 106);
            this.radioButtonVideoPlayerVLC.Name = "radioButtonVideoPlayerVLC";
            this.radioButtonVideoPlayerVLC.Size = new System.Drawing.Size(43, 17);
            this.radioButtonVideoPlayerVLC.TabIndex = 5;
            this.radioButtonVideoPlayerVLC.TabStop = true;
            this.radioButtonVideoPlayerVLC.Text = "VLC";
            this.radioButtonVideoPlayerVLC.UseVisualStyleBackColor = true;
            // 
            // labelVideoPlayerMPlayer
            // 
            this.labelVideoPlayerMPlayer.AutoSize = true;
            this.labelVideoPlayerMPlayer.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelVideoPlayerMPlayer.ForeColor = System.Drawing.Color.Gray;
            this.labelVideoPlayerMPlayer.Location = new System.Drawing.Point(164, 26);
            this.labelVideoPlayerMPlayer.Name = "labelVideoPlayerMPlayer";
            this.labelVideoPlayerMPlayer.Size = new System.Drawing.Size(337, 13);
            this.labelVideoPlayerMPlayer.TabIndex = 11;
            this.labelVideoPlayerMPlayer.Text = "https://mpv.io/ - free, open source, and cross-platform media player";
            // 
            // labelDirectShowDescription
            // 
            this.labelDirectShowDescription.AutoSize = true;
            this.labelDirectShowDescription.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDirectShowDescription.ForeColor = System.Drawing.Color.Gray;
            this.labelDirectShowDescription.Location = new System.Drawing.Point(164, 75);
            this.labelDirectShowDescription.Name = "labelDirectShowDescription";
            this.labelDirectShowDescription.Size = new System.Drawing.Size(114, 13);
            this.labelDirectShowDescription.TabIndex = 10;
            this.labelDirectShowDescription.Text = "Quartz.dll in system32";
            // 
            // labelMpcHcDescription
            // 
            this.labelMpcHcDescription.AutoSize = true;
            this.labelMpcHcDescription.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelMpcHcDescription.ForeColor = System.Drawing.Color.Gray;
            this.labelMpcHcDescription.Location = new System.Drawing.Point(164, 138);
            this.labelMpcHcDescription.Name = "labelMpcHcDescription";
            this.labelMpcHcDescription.Size = new System.Drawing.Size(178, 13);
            this.labelMpcHcDescription.TabIndex = 9;
            this.labelMpcHcDescription.Text = "Media Player Classic - Home Cinema";
            // 
            // radioButtonVideoPlayerMPV
            // 
            this.radioButtonVideoPlayerMPV.AutoSize = true;
            this.radioButtonVideoPlayerMPV.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioButtonVideoPlayerMPV.Location = new System.Drawing.Point(9, 22);
            this.radioButtonVideoPlayerMPV.Name = "radioButtonVideoPlayerMPV";
            this.radioButtonVideoPlayerMPV.Size = new System.Drawing.Size(50, 17);
            this.radioButtonVideoPlayerMPV.TabIndex = 1;
            this.radioButtonVideoPlayerMPV.TabStop = true;
            this.radioButtonVideoPlayerMPV.Text = "mpv";
            this.radioButtonVideoPlayerMPV.UseVisualStyleBackColor = true;
            this.radioButtonVideoPlayerMPV.CheckedChanged += new System.EventHandler(this.radioButtonVideoPlayerMPV_CheckedChanged);
            // 
            // radioButtonVideoPlayerDirectShow
            // 
            this.radioButtonVideoPlayerDirectShow.AutoSize = true;
            this.radioButtonVideoPlayerDirectShow.Location = new System.Drawing.Point(9, 72);
            this.radioButtonVideoPlayerDirectShow.Name = "radioButtonVideoPlayerDirectShow";
            this.radioButtonVideoPlayerDirectShow.Size = new System.Drawing.Size(82, 17);
            this.radioButtonVideoPlayerDirectShow.TabIndex = 4;
            this.radioButtonVideoPlayerDirectShow.TabStop = true;
            this.radioButtonVideoPlayerDirectShow.Text = "DirectShow ";
            this.radioButtonVideoPlayerDirectShow.UseVisualStyleBackColor = true;
            // 
            // radioButtonVideoPlayerMpcHc
            // 
            this.radioButtonVideoPlayerMpcHc.AutoSize = true;
            this.radioButtonVideoPlayerMpcHc.Location = new System.Drawing.Point(9, 135);
            this.radioButtonVideoPlayerMpcHc.Name = "radioButtonVideoPlayerMpcHc";
            this.radioButtonVideoPlayerMpcHc.Size = new System.Drawing.Size(64, 17);
            this.radioButtonVideoPlayerMpcHc.TabIndex = 8;
            this.radioButtonVideoPlayerMpcHc.TabStop = true;
            this.radioButtonVideoPlayerMpcHc.Text = "MPC-HC";
            this.radioButtonVideoPlayerMpcHc.UseVisualStyleBackColor = true;
            // 
            // panelWaveform
            // 
            this.panelWaveform.Controls.Add(this.groupBoxFfmpeg);
            this.panelWaveform.Controls.Add(this.groupBoxSpectrogram);
            this.panelWaveform.Controls.Add(this.groupBoxSpectrogramClean);
            this.panelWaveform.Controls.Add(this.groupBoxWaveformAppearence);
            this.panelWaveform.Location = new System.Drawing.Point(230, 6);
            this.panelWaveform.Name = "panelWaveform";
            this.panelWaveform.Size = new System.Drawing.Size(864, 521);
            this.panelWaveform.TabIndex = 6;
            this.panelWaveform.Text = "Waveform/spectrogram";
            // 
            // groupBoxFfmpeg
            // 
            this.groupBoxFfmpeg.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxFfmpeg.Controls.Add(this.checkBoxFfmpegUseCenterChannel);
            this.groupBoxFfmpeg.Controls.Add(this.buttonDownloadFfmpeg);
            this.groupBoxFfmpeg.Controls.Add(this.buttonBrowseToFFmpeg);
            this.groupBoxFfmpeg.Controls.Add(this.textBoxFFmpegPath);
            this.groupBoxFfmpeg.Controls.Add(this.labelFFmpegPath);
            this.groupBoxFfmpeg.Controls.Add(this.checkBoxUseFFmpeg);
            this.groupBoxFfmpeg.Location = new System.Drawing.Point(400, 293);
            this.groupBoxFfmpeg.Name = "groupBoxFfmpeg";
            this.groupBoxFfmpeg.Size = new System.Drawing.Size(451, 133);
            this.groupBoxFfmpeg.TabIndex = 2;
            this.groupBoxFfmpeg.TabStop = false;
            this.groupBoxFfmpeg.Text = "FFmpeg";
            // 
            // checkBoxFfmpegUseCenterChannel
            // 
            this.checkBoxFfmpegUseCenterChannel.AutoSize = true;
            this.checkBoxFfmpegUseCenterChannel.Location = new System.Drawing.Point(6, 99);
            this.checkBoxFfmpegUseCenterChannel.Name = "checkBoxFfmpegUseCenterChannel";
            this.checkBoxFfmpegUseCenterChannel.Size = new System.Drawing.Size(219, 17);
            this.checkBoxFfmpegUseCenterChannel.TabIndex = 24;
            this.checkBoxFfmpegUseCenterChannel.Text = "Use center channel only for audio tracks";
            this.checkBoxFfmpegUseCenterChannel.UseVisualStyleBackColor = true;
            // 
            // buttonDownloadFfmpeg
            // 
            this.buttonDownloadFfmpeg.Location = new System.Drawing.Point(274, 17);
            this.buttonDownloadFfmpeg.Name = "buttonDownloadFfmpeg";
            this.buttonDownloadFfmpeg.Size = new System.Drawing.Size(136, 23);
            this.buttonDownloadFfmpeg.TabIndex = 2;
            this.buttonDownloadFfmpeg.Text = "Download ffmpeg";
            this.buttonDownloadFfmpeg.UseVisualStyleBackColor = true;
            this.buttonDownloadFfmpeg.Click += new System.EventHandler(this.buttonDownloadFfmpeg_Click);
            // 
            // buttonBrowseToFFmpeg
            // 
            this.buttonBrowseToFFmpeg.Location = new System.Drawing.Point(381, 65);
            this.buttonBrowseToFFmpeg.Name = "buttonBrowseToFFmpeg";
            this.buttonBrowseToFFmpeg.Size = new System.Drawing.Size(29, 21);
            this.buttonBrowseToFFmpeg.TabIndex = 23;
            this.buttonBrowseToFFmpeg.Text = "...";
            this.buttonBrowseToFFmpeg.UseVisualStyleBackColor = true;
            this.buttonBrowseToFFmpeg.Click += new System.EventHandler(this.buttonBrowseToFFmpeg_Click);
            // 
            // textBoxFFmpegPath
            // 
            this.textBoxFFmpegPath.BackColor = System.Drawing.SystemColors.Window;
            this.textBoxFFmpegPath.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxFFmpegPath.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxFFmpegPath.Location = new System.Drawing.Point(9, 65);
            this.textBoxFFmpegPath.MaxLength = 1000;
            this.textBoxFFmpegPath.Multiline = true;
            this.textBoxFFmpegPath.Name = "textBoxFFmpegPath";
            this.textBoxFFmpegPath.Size = new System.Drawing.Size(366, 21);
            this.textBoxFFmpegPath.TabIndex = 22;
            // 
            // labelFFmpegPath
            // 
            this.labelFFmpegPath.AutoSize = true;
            this.labelFFmpegPath.Location = new System.Drawing.Point(6, 49);
            this.labelFFmpegPath.Name = "labelFFmpegPath";
            this.labelFFmpegPath.Size = new System.Drawing.Size(70, 13);
            this.labelFFmpegPath.TabIndex = 2;
            this.labelFFmpegPath.Text = "FFmpeg path";
            // 
            // checkBoxUseFFmpeg
            // 
            this.checkBoxUseFFmpeg.AutoSize = true;
            this.checkBoxUseFFmpeg.Location = new System.Drawing.Point(6, 20);
            this.checkBoxUseFFmpeg.Name = "checkBoxUseFFmpeg";
            this.checkBoxUseFFmpeg.Size = new System.Drawing.Size(200, 17);
            this.checkBoxUseFFmpeg.TabIndex = 1;
            this.checkBoxUseFFmpeg.Text = "Use FFmpeg for wave file extraction";
            this.checkBoxUseFFmpeg.UseVisualStyleBackColor = true;
            // 
            // groupBoxSpectrogram
            // 
            this.groupBoxSpectrogram.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxSpectrogram.Controls.Add(this.labelSpectrogramAppearance);
            this.groupBoxSpectrogram.Controls.Add(this.comboBoxSpectrogramAppearance);
            this.groupBoxSpectrogram.Controls.Add(this.checkBoxGenerateSpectrogram);
            this.groupBoxSpectrogram.Location = new System.Drawing.Point(0, 293);
            this.groupBoxSpectrogram.Name = "groupBoxSpectrogram";
            this.groupBoxSpectrogram.Size = new System.Drawing.Size(393, 132);
            this.groupBoxSpectrogram.TabIndex = 1;
            this.groupBoxSpectrogram.TabStop = false;
            this.groupBoxSpectrogram.Text = "Spectrogram";
            // 
            // labelSpectrogramAppearance
            // 
            this.labelSpectrogramAppearance.AutoSize = true;
            this.labelSpectrogramAppearance.Location = new System.Drawing.Point(10, 52);
            this.labelSpectrogramAppearance.Name = "labelSpectrogramAppearance";
            this.labelSpectrogramAppearance.Size = new System.Drawing.Size(164, 13);
            this.labelSpectrogramAppearance.TabIndex = 1;
            this.labelSpectrogramAppearance.Text = "Appearance (at generation time)";
            // 
            // comboBoxSpectrogramAppearance
            // 
            this.comboBoxSpectrogramAppearance.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxSpectrogramAppearance.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxSpectrogramAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxSpectrogramAppearance.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxSpectrogramAppearance.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxSpectrogramAppearance.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxSpectrogramAppearance.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxSpectrogramAppearance.DropDownHeight = 400;
            this.comboBoxSpectrogramAppearance.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSpectrogramAppearance.DropDownWidth = 325;
            this.comboBoxSpectrogramAppearance.FormattingEnabled = true;
            this.comboBoxSpectrogramAppearance.Items.AddRange(new string[] {
            "Classic",
            "Use waveform color (one color gradient)"});
            this.comboBoxSpectrogramAppearance.Location = new System.Drawing.Point(10, 70);
            this.comboBoxSpectrogramAppearance.MaxLength = 32767;
            this.comboBoxSpectrogramAppearance.Name = "comboBoxSpectrogramAppearance";
            this.comboBoxSpectrogramAppearance.SelectedIndex = -1;
            this.comboBoxSpectrogramAppearance.SelectedItem = null;
            this.comboBoxSpectrogramAppearance.SelectedText = "";
            this.comboBoxSpectrogramAppearance.Size = new System.Drawing.Size(325, 21);
            this.comboBoxSpectrogramAppearance.TabIndex = 2;
            this.comboBoxSpectrogramAppearance.UsePopupWindow = false;
            // 
            // checkBoxGenerateSpectrogram
            // 
            this.checkBoxGenerateSpectrogram.AutoSize = true;
            this.checkBoxGenerateSpectrogram.Location = new System.Drawing.Point(10, 20);
            this.checkBoxGenerateSpectrogram.Name = "checkBoxGenerateSpectrogram";
            this.checkBoxGenerateSpectrogram.Size = new System.Drawing.Size(134, 17);
            this.checkBoxGenerateSpectrogram.TabIndex = 0;
            this.checkBoxGenerateSpectrogram.Text = "Generate spectrogram";
            this.checkBoxGenerateSpectrogram.UseVisualStyleBackColor = true;
            // 
            // groupBoxSpectrogramClean
            // 
            this.groupBoxSpectrogramClean.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBoxSpectrogramClean.Controls.Add(this.buttonWaveformsFolderEmpty);
            this.groupBoxSpectrogramClean.Controls.Add(this.labelWaveformsFolderInfo);
            this.groupBoxSpectrogramClean.Location = new System.Drawing.Point(0, 431);
            this.groupBoxSpectrogramClean.Name = "groupBoxSpectrogramClean";
            this.groupBoxSpectrogramClean.Size = new System.Drawing.Size(852, 89);
            this.groupBoxSpectrogramClean.TabIndex = 3;
            this.groupBoxSpectrogramClean.TabStop = false;
            // 
            // buttonWaveformsFolderEmpty
            // 
            this.buttonWaveformsFolderEmpty.Location = new System.Drawing.Point(10, 37);
            this.buttonWaveformsFolderEmpty.Name = "buttonWaveformsFolderEmpty";
            this.buttonWaveformsFolderEmpty.Size = new System.Drawing.Size(325, 23);
            this.buttonWaveformsFolderEmpty.TabIndex = 1;
            this.buttonWaveformsFolderEmpty.Text = "Empty \'Waveforms\' folder";
            this.buttonWaveformsFolderEmpty.UseVisualStyleBackColor = true;
            this.buttonWaveformsFolderEmpty.Click += new System.EventHandler(this.buttonWaveformsFolderEmpty_Click);
            // 
            // labelWaveformsFolderInfo
            // 
            this.labelWaveformsFolderInfo.AutoSize = true;
            this.labelWaveformsFolderInfo.Location = new System.Drawing.Point(10, 20);
            this.labelWaveformsFolderInfo.Name = "labelWaveformsFolderInfo";
            this.labelWaveformsFolderInfo.Size = new System.Drawing.Size(205, 13);
            this.labelWaveformsFolderInfo.TabIndex = 0;
            this.labelWaveformsFolderInfo.Text = "\'Waveforms\' folder contains x files (x mb)";
            // 
            // groupBoxWaveformAppearence
            // 
            this.groupBoxWaveformAppearence.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxWaveformAppearence.Controls.Add(this.buttonEditShotChangesProfile);
            this.groupBoxWaveformAppearence.Controls.Add(this.checkBoxWaveformAutoGen);
            this.groupBoxWaveformAppearence.Controls.Add(this.panelWaveformCursorColor);
            this.groupBoxWaveformAppearence.Controls.Add(this.buttonWaveformCursorColor);
            this.groupBoxWaveformAppearence.Controls.Add(this.checkBoxWaveformSnapToShotChanges);
            this.groupBoxWaveformAppearence.Controls.Add(this.checkBoxWaveformSingleClickSelect);
            this.groupBoxWaveformAppearence.Controls.Add(this.checkBoxWaveformShowWpm);
            this.groupBoxWaveformAppearence.Controls.Add(this.checkBoxWaveformShowCps);
            this.groupBoxWaveformAppearence.Controls.Add(this.checkBoxWaveformSetVideoPosMoveStartEnd);
            this.groupBoxWaveformAppearence.Controls.Add(this.labelWaveformTextSize);
            this.groupBoxWaveformAppearence.Controls.Add(this.checkBoxWaveformTextBold);
            this.groupBoxWaveformAppearence.Controls.Add(this.comboBoxWaveformTextSize);
            this.groupBoxWaveformAppearence.Controls.Add(this.checkBoxListViewMouseEnterFocus);
            this.groupBoxWaveformAppearence.Controls.Add(this.checkBoxWaveformHoverFocus);
            this.groupBoxWaveformAppearence.Controls.Add(this.labelWaveformBorderHitMs2);
            this.groupBoxWaveformAppearence.Controls.Add(this.numericUpDownWaveformBorderHitMs);
            this.groupBoxWaveformAppearence.Controls.Add(this.labelWaveformBorderHitMs1);
            this.groupBoxWaveformAppearence.Controls.Add(this.checkBoxAllowOverlap);
            this.groupBoxWaveformAppearence.Controls.Add(this.checkBoxReverseMouseWheelScrollDirection);
            this.groupBoxWaveformAppearence.Controls.Add(this.panelWaveformTextColor);
            this.groupBoxWaveformAppearence.Controls.Add(this.buttonWaveformTextColor);
            this.groupBoxWaveformAppearence.Controls.Add(this.panelWaveformGridColor);
            this.groupBoxWaveformAppearence.Controls.Add(this.buttonWaveformGridColor);
            this.groupBoxWaveformAppearence.Controls.Add(this.panelWaveformBackgroundColor);
            this.groupBoxWaveformAppearence.Controls.Add(this.buttonWaveformBackgroundColor);
            this.groupBoxWaveformAppearence.Controls.Add(this.panelWaveformColor);
            this.groupBoxWaveformAppearence.Controls.Add(this.buttonWaveformColor);
            this.groupBoxWaveformAppearence.Controls.Add(this.panelWaveformSelectedColor);
            this.groupBoxWaveformAppearence.Controls.Add(this.buttonWaveformSelectedColor);
            this.groupBoxWaveformAppearence.Controls.Add(this.checkBoxWaveformShowGrid);
            this.groupBoxWaveformAppearence.Location = new System.Drawing.Point(0, 0);
            this.groupBoxWaveformAppearence.Name = "groupBoxWaveformAppearence";
            this.groupBoxWaveformAppearence.Size = new System.Drawing.Size(851, 287);
            this.groupBoxWaveformAppearence.TabIndex = 0;
            this.groupBoxWaveformAppearence.TabStop = false;
            this.groupBoxWaveformAppearence.Text = "Waveform appearance";
            // 
            // buttonEditShotChangesProfile
            // 
            this.buttonEditShotChangesProfile.Location = new System.Drawing.Point(524, 157);
            this.buttonEditShotChangesProfile.Name = "buttonEditShotChangesProfile";
            this.buttonEditShotChangesProfile.Size = new System.Drawing.Size(136, 23);
            this.buttonEditShotChangesProfile.TabIndex = 27;
            this.buttonEditShotChangesProfile.Text = "Edit profile...";
            this.buttonEditShotChangesProfile.UseVisualStyleBackColor = true;
            this.buttonEditShotChangesProfile.Click += new System.EventHandler(this.buttonEditShotChangesProfile_Click);
            // 
            // checkBoxWaveformAutoGen
            // 
            this.checkBoxWaveformAutoGen.AutoSize = true;
            this.checkBoxWaveformAutoGen.Location = new System.Drawing.Point(262, 185);
            this.checkBoxWaveformAutoGen.Name = "checkBoxWaveformAutoGen";
            this.checkBoxWaveformAutoGen.Size = new System.Drawing.Size(220, 17);
            this.checkBoxWaveformAutoGen.TabIndex = 28;
            this.checkBoxWaveformAutoGen.Text = "Auto gen waveform when opening video";
            this.checkBoxWaveformAutoGen.UseVisualStyleBackColor = true;
            // 
            // panelWaveformCursorColor
            // 
            this.panelWaveformCursorColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelWaveformCursorColor.Location = new System.Drawing.Point(138, 142);
            this.panelWaveformCursorColor.Name = "panelWaveformCursorColor";
            this.panelWaveformCursorColor.Size = new System.Drawing.Size(21, 20);
            this.panelWaveformCursorColor.TabIndex = 34;
            this.panelWaveformCursorColor.Click += new System.EventHandler(this.buttonWaveformCursorColor_Click);
            // 
            // buttonWaveformCursorColor
            // 
            this.buttonWaveformCursorColor.Location = new System.Drawing.Point(14, 141);
            this.buttonWaveformCursorColor.Name = "buttonWaveformCursorColor";
            this.buttonWaveformCursorColor.Size = new System.Drawing.Size(112, 23);
            this.buttonWaveformCursorColor.TabIndex = 4;
            this.buttonWaveformCursorColor.Text = "Cursor color";
            this.buttonWaveformCursorColor.UseVisualStyleBackColor = true;
            this.buttonWaveformCursorColor.Click += new System.EventHandler(this.buttonWaveformCursorColor_Click);
            // 
            // checkBoxWaveformSnapToShotChanges
            // 
            this.checkBoxWaveformSnapToShotChanges.AutoSize = true;
            this.checkBoxWaveformSnapToShotChanges.Location = new System.Drawing.Point(262, 161);
            this.checkBoxWaveformSnapToShotChanges.Name = "checkBoxWaveformSnapToShotChanges";
            this.checkBoxWaveformSnapToShotChanges.Size = new System.Drawing.Size(242, 17);
            this.checkBoxWaveformSnapToShotChanges.TabIndex = 26;
            this.checkBoxWaveformSnapToShotChanges.Text = "Snap to shot changes (hold Shift to override)";
            this.checkBoxWaveformSnapToShotChanges.UseVisualStyleBackColor = true;
            // 
            // checkBoxWaveformSingleClickSelect
            // 
            this.checkBoxWaveformSingleClickSelect.AutoSize = true;
            this.checkBoxWaveformSingleClickSelect.Location = new System.Drawing.Point(262, 138);
            this.checkBoxWaveformSingleClickSelect.Name = "checkBoxWaveformSingleClickSelect";
            this.checkBoxWaveformSingleClickSelect.Size = new System.Drawing.Size(178, 17);
            this.checkBoxWaveformSingleClickSelect.TabIndex = 25;
            this.checkBoxWaveformSingleClickSelect.Text = "Single click to select paragraphs";
            this.checkBoxWaveformSingleClickSelect.UseVisualStyleBackColor = true;
            // 
            // checkBoxWaveformShowWpm
            // 
            this.checkBoxWaveformShowWpm.AutoSize = true;
            this.checkBoxWaveformShowWpm.Location = new System.Drawing.Point(16, 249);
            this.checkBoxWaveformShowWpm.Name = "checkBoxWaveformShowWpm";
            this.checkBoxWaveformShowWpm.Size = new System.Drawing.Size(104, 17);
            this.checkBoxWaveformShowWpm.TabIndex = 8;
            this.checkBoxWaveformShowWpm.Text = "Show words/min";
            this.checkBoxWaveformShowWpm.UseVisualStyleBackColor = true;
            // 
            // checkBoxWaveformShowCps
            // 
            this.checkBoxWaveformShowCps.AutoSize = true;
            this.checkBoxWaveformShowCps.Location = new System.Drawing.Point(16, 226);
            this.checkBoxWaveformShowCps.Name = "checkBoxWaveformShowCps";
            this.checkBoxWaveformShowCps.Size = new System.Drawing.Size(96, 17);
            this.checkBoxWaveformShowCps.TabIndex = 7;
            this.checkBoxWaveformShowCps.Text = "Show char/sec";
            this.checkBoxWaveformShowCps.UseVisualStyleBackColor = true;
            // 
            // checkBoxWaveformSetVideoPosMoveStartEnd
            // 
            this.checkBoxWaveformSetVideoPosMoveStartEnd.AutoSize = true;
            this.checkBoxWaveformSetVideoPosMoveStartEnd.Location = new System.Drawing.Point(262, 73);
            this.checkBoxWaveformSetVideoPosMoveStartEnd.Name = "checkBoxWaveformSetVideoPosMoveStartEnd";
            this.checkBoxWaveformSetVideoPosMoveStartEnd.Size = new System.Drawing.Size(225, 17);
            this.checkBoxWaveformSetVideoPosMoveStartEnd.TabIndex = 22;
            this.checkBoxWaveformSetVideoPosMoveStartEnd.Text = "Set video position when moving start/end";
            this.checkBoxWaveformSetVideoPosMoveStartEnd.UseVisualStyleBackColor = true;
            // 
            // labelWaveformTextSize
            // 
            this.labelWaveformTextSize.AutoSize = true;
            this.labelWaveformTextSize.Location = new System.Drawing.Point(259, 220);
            this.labelWaveformTextSize.Name = "labelWaveformTextSize";
            this.labelWaveformTextSize.Size = new System.Drawing.Size(73, 13);
            this.labelWaveformTextSize.TabIndex = 29;
            this.labelWaveformTextSize.Text = "Text font size";
            // 
            // checkBoxWaveformTextBold
            // 
            this.checkBoxWaveformTextBold.AutoSize = true;
            this.checkBoxWaveformTextBold.Location = new System.Drawing.Point(463, 221);
            this.checkBoxWaveformTextBold.Name = "checkBoxWaveformTextBold";
            this.checkBoxWaveformTextBold.Size = new System.Drawing.Size(46, 17);
            this.checkBoxWaveformTextBold.TabIndex = 31;
            this.checkBoxWaveformTextBold.Text = "Bold";
            this.checkBoxWaveformTextBold.UseVisualStyleBackColor = true;
            // 
            // comboBoxWaveformTextSize
            // 
            this.comboBoxWaveformTextSize.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxWaveformTextSize.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxWaveformTextSize.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxWaveformTextSize.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxWaveformTextSize.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxWaveformTextSize.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxWaveformTextSize.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxWaveformTextSize.DropDownHeight = 400;
            this.comboBoxWaveformTextSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxWaveformTextSize.DropDownWidth = 121;
            this.comboBoxWaveformTextSize.FormattingEnabled = true;
            this.comboBoxWaveformTextSize.Items.AddRange(new string[] {
            "7",
            "8",
            "9",
            "10",
            "11",
            "12",
            "13",
            "14",
            "15",
            "16",
            "17",
            "18",
            "19",
            "20"});
            this.comboBoxWaveformTextSize.Location = new System.Drawing.Point(336, 217);
            this.comboBoxWaveformTextSize.MaxLength = 32767;
            this.comboBoxWaveformTextSize.Name = "comboBoxWaveformTextSize";
            this.comboBoxWaveformTextSize.SelectedIndex = -1;
            this.comboBoxWaveformTextSize.SelectedItem = null;
            this.comboBoxWaveformTextSize.SelectedText = "";
            this.comboBoxWaveformTextSize.Size = new System.Drawing.Size(121, 21);
            this.comboBoxWaveformTextSize.TabIndex = 30;
            this.comboBoxWaveformTextSize.UsePopupWindow = false;
            // 
            // checkBoxListViewMouseEnterFocus
            // 
            this.checkBoxListViewMouseEnterFocus.AutoSize = true;
            this.checkBoxListViewMouseEnterFocus.Location = new System.Drawing.Point(281, 115);
            this.checkBoxListViewMouseEnterFocus.Name = "checkBoxListViewMouseEnterFocus";
            this.checkBoxListViewMouseEnterFocus.Size = new System.Drawing.Size(214, 17);
            this.checkBoxListViewMouseEnterFocus.TabIndex = 24;
            this.checkBoxListViewMouseEnterFocus.Text = "Focus list view on list view mouse enter";
            this.checkBoxListViewMouseEnterFocus.UseVisualStyleBackColor = true;
            // 
            // checkBoxWaveformHoverFocus
            // 
            this.checkBoxWaveformHoverFocus.AutoSize = true;
            this.checkBoxWaveformHoverFocus.Location = new System.Drawing.Point(262, 96);
            this.checkBoxWaveformHoverFocus.Name = "checkBoxWaveformHoverFocus";
            this.checkBoxWaveformHoverFocus.Size = new System.Drawing.Size(149, 17);
            this.checkBoxWaveformHoverFocus.TabIndex = 23;
            this.checkBoxWaveformHoverFocus.Text = "Set focus on mouse enter";
            this.checkBoxWaveformHoverFocus.UseVisualStyleBackColor = true;
            this.checkBoxWaveformHoverFocus.CheckedChanged += new System.EventHandler(this.checkBoxWaveformHoverFocus_CheckedChanged);
            // 
            // labelWaveformBorderHitMs2
            // 
            this.labelWaveformBorderHitMs2.AutoSize = true;
            this.labelWaveformBorderHitMs2.Location = new System.Drawing.Point(454, 250);
            this.labelWaveformBorderHitMs2.Name = "labelWaveformBorderHitMs2";
            this.labelWaveformBorderHitMs2.Size = new System.Drawing.Size(62, 13);
            this.labelWaveformBorderHitMs2.TabIndex = 34;
            this.labelWaveformBorderHitMs2.Text = "milliseconds";
            // 
            // numericUpDownWaveformBorderHitMs
            // 
            this.numericUpDownWaveformBorderHitMs.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownWaveformBorderHitMs.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownWaveformBorderHitMs.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownWaveformBorderHitMs.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownWaveformBorderHitMs.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownWaveformBorderHitMs.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownWaveformBorderHitMs.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownWaveformBorderHitMs.DecimalPlaces = 0;
            this.numericUpDownWaveformBorderHitMs.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownWaveformBorderHitMs.Location = new System.Drawing.Point(392, 248);
            this.numericUpDownWaveformBorderHitMs.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDownWaveformBorderHitMs.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDownWaveformBorderHitMs.Name = "numericUpDownWaveformBorderHitMs";
            this.numericUpDownWaveformBorderHitMs.Size = new System.Drawing.Size(44, 21);
            this.numericUpDownWaveformBorderHitMs.TabIndex = 33;
            this.numericUpDownWaveformBorderHitMs.TabStop = false;
            this.numericUpDownWaveformBorderHitMs.ThousandsSeparator = false;
            this.numericUpDownWaveformBorderHitMs.Value = new decimal(new int[] {
            18,
            0,
            0,
            0});
            // 
            // labelWaveformBorderHitMs1
            // 
            this.labelWaveformBorderHitMs1.AutoSize = true;
            this.labelWaveformBorderHitMs1.Location = new System.Drawing.Point(259, 250);
            this.labelWaveformBorderHitMs1.Name = "labelWaveformBorderHitMs1";
            this.labelWaveformBorderHitMs1.Size = new System.Drawing.Size(127, 13);
            this.labelWaveformBorderHitMs1.TabIndex = 32;
            this.labelWaveformBorderHitMs1.Text = "Marker hit must be within";
            // 
            // checkBoxAllowOverlap
            // 
            this.checkBoxAllowOverlap.AutoSize = true;
            this.checkBoxAllowOverlap.Location = new System.Drawing.Point(262, 50);
            this.checkBoxAllowOverlap.Name = "checkBoxAllowOverlap";
            this.checkBoxAllowOverlap.Size = new System.Drawing.Size(212, 17);
            this.checkBoxAllowOverlap.TabIndex = 21;
            this.checkBoxAllowOverlap.Text = "Allow overlap (when dragging/resizing)";
            this.checkBoxAllowOverlap.UseVisualStyleBackColor = true;
            // 
            // checkBoxReverseMouseWheelScrollDirection
            // 
            this.checkBoxReverseMouseWheelScrollDirection.AutoSize = true;
            this.checkBoxReverseMouseWheelScrollDirection.Location = new System.Drawing.Point(262, 27);
            this.checkBoxReverseMouseWheelScrollDirection.Name = "checkBoxReverseMouseWheelScrollDirection";
            this.checkBoxReverseMouseWheelScrollDirection.Size = new System.Drawing.Size(202, 17);
            this.checkBoxReverseMouseWheelScrollDirection.TabIndex = 20;
            this.checkBoxReverseMouseWheelScrollDirection.Text = "Reverse mouse wheel scroll direction";
            this.checkBoxReverseMouseWheelScrollDirection.UseVisualStyleBackColor = true;
            // 
            // panelWaveformTextColor
            // 
            this.panelWaveformTextColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelWaveformTextColor.Location = new System.Drawing.Point(138, 113);
            this.panelWaveformTextColor.Name = "panelWaveformTextColor";
            this.panelWaveformTextColor.Size = new System.Drawing.Size(21, 20);
            this.panelWaveformTextColor.TabIndex = 7;
            this.panelWaveformTextColor.Click += new System.EventHandler(this.buttonWaveformTextColor_Click);
            // 
            // buttonWaveformTextColor
            // 
            this.buttonWaveformTextColor.Location = new System.Drawing.Point(14, 112);
            this.buttonWaveformTextColor.Name = "buttonWaveformTextColor";
            this.buttonWaveformTextColor.Size = new System.Drawing.Size(112, 23);
            this.buttonWaveformTextColor.TabIndex = 3;
            this.buttonWaveformTextColor.Text = "Text color";
            this.buttonWaveformTextColor.UseVisualStyleBackColor = true;
            this.buttonWaveformTextColor.Click += new System.EventHandler(this.buttonWaveformTextColor_Click);
            // 
            // panelWaveformGridColor
            // 
            this.panelWaveformGridColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelWaveformGridColor.Location = new System.Drawing.Point(138, 171);
            this.panelWaveformGridColor.Name = "panelWaveformGridColor";
            this.panelWaveformGridColor.Size = new System.Drawing.Size(21, 20);
            this.panelWaveformGridColor.TabIndex = 9;
            this.panelWaveformGridColor.Click += new System.EventHandler(this.buttonWaveformGridColor_Click);
            // 
            // buttonWaveformGridColor
            // 
            this.buttonWaveformGridColor.Location = new System.Drawing.Point(14, 170);
            this.buttonWaveformGridColor.Name = "buttonWaveformGridColor";
            this.buttonWaveformGridColor.Size = new System.Drawing.Size(112, 23);
            this.buttonWaveformGridColor.TabIndex = 5;
            this.buttonWaveformGridColor.Text = "Grid color";
            this.buttonWaveformGridColor.UseVisualStyleBackColor = true;
            this.buttonWaveformGridColor.Click += new System.EventHandler(this.buttonWaveformGridColor_Click);
            // 
            // panelWaveformBackgroundColor
            // 
            this.panelWaveformBackgroundColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelWaveformBackgroundColor.Location = new System.Drawing.Point(138, 84);
            this.panelWaveformBackgroundColor.Name = "panelWaveformBackgroundColor";
            this.panelWaveformBackgroundColor.Size = new System.Drawing.Size(21, 20);
            this.panelWaveformBackgroundColor.TabIndex = 5;
            this.panelWaveformBackgroundColor.Click += new System.EventHandler(this.buttonWaveformBackgroundColor_Click);
            // 
            // buttonWaveformBackgroundColor
            // 
            this.buttonWaveformBackgroundColor.Location = new System.Drawing.Point(14, 83);
            this.buttonWaveformBackgroundColor.Name = "buttonWaveformBackgroundColor";
            this.buttonWaveformBackgroundColor.Size = new System.Drawing.Size(112, 23);
            this.buttonWaveformBackgroundColor.TabIndex = 2;
            this.buttonWaveformBackgroundColor.Text = "Back color";
            this.buttonWaveformBackgroundColor.UseVisualStyleBackColor = true;
            this.buttonWaveformBackgroundColor.Click += new System.EventHandler(this.buttonWaveformBackgroundColor_Click);
            // 
            // panelWaveformColor
            // 
            this.panelWaveformColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelWaveformColor.Location = new System.Drawing.Point(138, 55);
            this.panelWaveformColor.Name = "panelWaveformColor";
            this.panelWaveformColor.Size = new System.Drawing.Size(21, 20);
            this.panelWaveformColor.TabIndex = 3;
            this.panelWaveformColor.Click += new System.EventHandler(this.buttonWaveformColor_Click);
            // 
            // buttonWaveformColor
            // 
            this.buttonWaveformColor.Location = new System.Drawing.Point(14, 54);
            this.buttonWaveformColor.Name = "buttonWaveformColor";
            this.buttonWaveformColor.Size = new System.Drawing.Size(112, 23);
            this.buttonWaveformColor.TabIndex = 1;
            this.buttonWaveformColor.Text = "Color";
            this.buttonWaveformColor.UseVisualStyleBackColor = true;
            this.buttonWaveformColor.Click += new System.EventHandler(this.buttonWaveformColor_Click);
            // 
            // panelWaveformSelectedColor
            // 
            this.panelWaveformSelectedColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelWaveformSelectedColor.Location = new System.Drawing.Point(138, 26);
            this.panelWaveformSelectedColor.Name = "panelWaveformSelectedColor";
            this.panelWaveformSelectedColor.Size = new System.Drawing.Size(21, 20);
            this.panelWaveformSelectedColor.TabIndex = 1;
            this.panelWaveformSelectedColor.MouseClick += new System.Windows.Forms.MouseEventHandler(this.buttonWaveformSelectedColor_Click);
            // 
            // buttonWaveformSelectedColor
            // 
            this.buttonWaveformSelectedColor.Location = new System.Drawing.Point(14, 25);
            this.buttonWaveformSelectedColor.Name = "buttonWaveformSelectedColor";
            this.buttonWaveformSelectedColor.Size = new System.Drawing.Size(112, 23);
            this.buttonWaveformSelectedColor.TabIndex = 0;
            this.buttonWaveformSelectedColor.Text = "Selected color";
            this.buttonWaveformSelectedColor.UseVisualStyleBackColor = true;
            this.buttonWaveformSelectedColor.Click += new System.EventHandler(this.buttonWaveformSelectedColor_Click);
            // 
            // checkBoxWaveformShowGrid
            // 
            this.checkBoxWaveformShowGrid.AutoSize = true;
            this.checkBoxWaveformShowGrid.Location = new System.Drawing.Point(16, 201);
            this.checkBoxWaveformShowGrid.Name = "checkBoxWaveformShowGrid";
            this.checkBoxWaveformShowGrid.Size = new System.Drawing.Size(73, 17);
            this.checkBoxWaveformShowGrid.TabIndex = 6;
            this.checkBoxWaveformShowGrid.Text = "Show grid";
            this.checkBoxWaveformShowGrid.UseVisualStyleBackColor = true;
            // 
            // panelTools
            // 
            this.panelTools.Controls.Add(this.groupBoxToolsMisc);
            this.panelTools.Controls.Add(this.groupBoxToolsAutoBr);
            this.panelTools.Controls.Add(this.groupBoxSpellCheck);
            this.panelTools.Controls.Add(this.groupBoxFixCommonErrors);
            this.panelTools.Controls.Add(this.groupBoxToolsVisualSync);
            this.panelTools.Location = new System.Drawing.Point(230, 6);
            this.panelTools.Name = "panelTools";
            this.panelTools.Padding = new System.Windows.Forms.Padding(3);
            this.panelTools.Size = new System.Drawing.Size(864, 521);
            this.panelTools.TabIndex = 7;
            this.panelTools.Text = "Tools";
            // 
            // groupBoxToolsMisc
            // 
            this.groupBoxToolsMisc.Controls.Add(this.comboBoxCustomToggleEnd);
            this.groupBoxToolsMisc.Controls.Add(this.comboBoxCustomToggleStart);
            this.groupBoxToolsMisc.Controls.Add(this.labelShortcutCustomToggle);
            this.groupBoxToolsMisc.Controls.Add(this.checkBoxShortcutsAllowLetterOrNumberInTextBox);
            this.groupBoxToolsMisc.Controls.Add(this.comboBoxBDOpensIn);
            this.groupBoxToolsMisc.Controls.Add(this.labelBDOpensIn);
            this.groupBoxToolsMisc.Location = new System.Drawing.Point(415, 180);
            this.groupBoxToolsMisc.Name = "groupBoxToolsMisc";
            this.groupBoxToolsMisc.Size = new System.Drawing.Size(434, 109);
            this.groupBoxToolsMisc.TabIndex = 6;
            this.groupBoxToolsMisc.TabStop = false;
            this.groupBoxToolsMisc.Text = "Misc";
            // 
            // comboBoxCustomToggleEnd
            // 
            this.comboBoxCustomToggleEnd.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxCustomToggleEnd.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxCustomToggleEnd.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxCustomToggleEnd.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxCustomToggleEnd.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxCustomToggleEnd.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxCustomToggleEnd.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxCustomToggleEnd.DropDownHeight = 400;
            this.comboBoxCustomToggleEnd.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.comboBoxCustomToggleEnd.DropDownWidth = 42;
            this.comboBoxCustomToggleEnd.FormattingEnabled = true;
            this.comboBoxCustomToggleEnd.Items.AddRange(new string[] {
            ")",
            "♪"});
            this.comboBoxCustomToggleEnd.Location = new System.Drawing.Point(230, 76);
            this.comboBoxCustomToggleEnd.MaxLength = 32767;
            this.comboBoxCustomToggleEnd.Name = "comboBoxCustomToggleEnd";
            this.comboBoxCustomToggleEnd.SelectedIndex = -1;
            this.comboBoxCustomToggleEnd.SelectedItem = null;
            this.comboBoxCustomToggleEnd.SelectedText = "";
            this.comboBoxCustomToggleEnd.Size = new System.Drawing.Size(42, 21);
            this.comboBoxCustomToggleEnd.TabIndex = 64;
            this.comboBoxCustomToggleEnd.TabStop = false;
            this.comboBoxCustomToggleEnd.UsePopupWindow = false;
            // 
            // comboBoxCustomToggleStart
            // 
            this.comboBoxCustomToggleStart.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxCustomToggleStart.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxCustomToggleStart.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxCustomToggleStart.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxCustomToggleStart.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxCustomToggleStart.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxCustomToggleStart.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxCustomToggleStart.DropDownHeight = 400;
            this.comboBoxCustomToggleStart.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.comboBoxCustomToggleStart.DropDownWidth = 42;
            this.comboBoxCustomToggleStart.FormattingEnabled = true;
            this.comboBoxCustomToggleStart.Items.AddRange(new string[] {
            "(",
            "♪"});
            this.comboBoxCustomToggleStart.Location = new System.Drawing.Point(182, 76);
            this.comboBoxCustomToggleStart.MaxLength = 32767;
            this.comboBoxCustomToggleStart.Name = "comboBoxCustomToggleStart";
            this.comboBoxCustomToggleStart.SelectedIndex = -1;
            this.comboBoxCustomToggleStart.SelectedItem = null;
            this.comboBoxCustomToggleStart.SelectedText = "";
            this.comboBoxCustomToggleStart.Size = new System.Drawing.Size(42, 21);
            this.comboBoxCustomToggleStart.TabIndex = 63;
            this.comboBoxCustomToggleStart.TabStop = false;
            this.comboBoxCustomToggleStart.UsePopupWindow = false;
            // 
            // labelShortcutCustomToggle
            // 
            this.labelShortcutCustomToggle.AutoSize = true;
            this.labelShortcutCustomToggle.Location = new System.Drawing.Point(6, 79);
            this.labelShortcutCustomToggle.Name = "labelShortcutCustomToggle";
            this.labelShortcutCustomToggle.Size = new System.Drawing.Size(169, 13);
            this.labelShortcutCustomToggle.TabIndex = 62;
            this.labelShortcutCustomToggle.Text = "Shortcut toggle custom  start/end";
            // 
            // checkBoxShortcutsAllowLetterOrNumberInTextBox
            // 
            this.checkBoxShortcutsAllowLetterOrNumberInTextBox.AutoSize = true;
            this.checkBoxShortcutsAllowLetterOrNumberInTextBox.Location = new System.Drawing.Point(6, 52);
            this.checkBoxShortcutsAllowLetterOrNumberInTextBox.Name = "checkBoxShortcutsAllowLetterOrNumberInTextBox";
            this.checkBoxShortcutsAllowLetterOrNumberInTextBox.Size = new System.Drawing.Size(223, 17);
            this.checkBoxShortcutsAllowLetterOrNumberInTextBox.TabIndex = 61;
            this.checkBoxShortcutsAllowLetterOrNumberInTextBox.Text = "Allow letter/number shortcuts in text box";
            this.checkBoxShortcutsAllowLetterOrNumberInTextBox.UseVisualStyleBackColor = true;
            // 
            // comboBoxBDOpensIn
            // 
            this.comboBoxBDOpensIn.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxBDOpensIn.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxBDOpensIn.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxBDOpensIn.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxBDOpensIn.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxBDOpensIn.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxBDOpensIn.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxBDOpensIn.DropDownHeight = 400;
            this.comboBoxBDOpensIn.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxBDOpensIn.DropDownWidth = 221;
            this.comboBoxBDOpensIn.FormattingEnabled = true;
            this.comboBoxBDOpensIn.Items.AddRange(new string[] {
            "OCR",
            "BD SUP EDIIT"});
            this.comboBoxBDOpensIn.Location = new System.Drawing.Point(158, 25);
            this.comboBoxBDOpensIn.MaxLength = 32767;
            this.comboBoxBDOpensIn.Name = "comboBoxBDOpensIn";
            this.comboBoxBDOpensIn.SelectedIndex = -1;
            this.comboBoxBDOpensIn.SelectedItem = null;
            this.comboBoxBDOpensIn.SelectedText = "";
            this.comboBoxBDOpensIn.Size = new System.Drawing.Size(221, 21);
            this.comboBoxBDOpensIn.TabIndex = 1;
            this.comboBoxBDOpensIn.UsePopupWindow = false;
            // 
            // labelBDOpensIn
            // 
            this.labelBDOpensIn.AutoSize = true;
            this.labelBDOpensIn.Location = new System.Drawing.Point(6, 27);
            this.labelBDOpensIn.Name = "labelBDOpensIn";
            this.labelBDOpensIn.Size = new System.Drawing.Size(146, 13);
            this.labelBDOpensIn.TabIndex = 0;
            this.labelBDOpensIn.Text = "Blu-ray sup/bdn-xml opens in";
            // 
            // groupBoxToolsAutoBr
            // 
            this.groupBoxToolsAutoBr.Controls.Add(this.labelToolsBreakBottomHeavyPercent);
            this.groupBoxToolsAutoBr.Controls.Add(this.numericUpDownToolsBreakPreferBottomHeavy);
            this.groupBoxToolsAutoBr.Controls.Add(this.checkBoxToolsBreakPreferBottomHeavy);
            this.groupBoxToolsAutoBr.Controls.Add(this.checkBoxToolsBreakByPixelWidth);
            this.groupBoxToolsAutoBr.Controls.Add(this.checkBoxToolsBreakEarlyLineEnding);
            this.groupBoxToolsAutoBr.Controls.Add(this.checkBoxToolsBreakEarlyComma);
            this.groupBoxToolsAutoBr.Controls.Add(this.checkBoxToolsBreakEarlyDash);
            this.groupBoxToolsAutoBr.Controls.Add(this.labelUserBingApiId);
            this.groupBoxToolsAutoBr.Controls.Add(this.buttonEditDoNotBreakAfterList);
            this.groupBoxToolsAutoBr.Controls.Add(this.checkBoxUseDoNotBreakAfterList);
            this.groupBoxToolsAutoBr.Location = new System.Drawing.Point(415, 0);
            this.groupBoxToolsAutoBr.Name = "groupBoxToolsAutoBr";
            this.groupBoxToolsAutoBr.Size = new System.Drawing.Size(434, 174);
            this.groupBoxToolsAutoBr.TabIndex = 5;
            this.groupBoxToolsAutoBr.TabStop = false;
            this.groupBoxToolsAutoBr.Text = "Auto br";
            // 
            // labelToolsBreakBottomHeavyPercent
            // 
            this.labelToolsBreakBottomHeavyPercent.AutoSize = true;
            this.labelToolsBreakBottomHeavyPercent.Location = new System.Drawing.Point(226, 140);
            this.labelToolsBreakBottomHeavyPercent.Name = "labelToolsBreakBottomHeavyPercent";
            this.labelToolsBreakBottomHeavyPercent.Size = new System.Drawing.Size(18, 13);
            this.labelToolsBreakBottomHeavyPercent.TabIndex = 63;
            this.labelToolsBreakBottomHeavyPercent.Text = "%";
            // 
            // numericUpDownToolsBreakPreferBottomHeavy
            // 
            this.numericUpDownToolsBreakPreferBottomHeavy.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownToolsBreakPreferBottomHeavy.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownToolsBreakPreferBottomHeavy.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownToolsBreakPreferBottomHeavy.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownToolsBreakPreferBottomHeavy.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownToolsBreakPreferBottomHeavy.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownToolsBreakPreferBottomHeavy.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownToolsBreakPreferBottomHeavy.DecimalPlaces = 1;
            this.numericUpDownToolsBreakPreferBottomHeavy.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericUpDownToolsBreakPreferBottomHeavy.Location = new System.Drawing.Point(164, 138);
            this.numericUpDownToolsBreakPreferBottomHeavy.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numericUpDownToolsBreakPreferBottomHeavy.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.numericUpDownToolsBreakPreferBottomHeavy.Name = "numericUpDownToolsBreakPreferBottomHeavy";
            this.numericUpDownToolsBreakPreferBottomHeavy.Size = new System.Drawing.Size(56, 21);
            this.numericUpDownToolsBreakPreferBottomHeavy.TabIndex = 62;
            this.numericUpDownToolsBreakPreferBottomHeavy.TabStop = false;
            this.numericUpDownToolsBreakPreferBottomHeavy.ThousandsSeparator = false;
            this.numericUpDownToolsBreakPreferBottomHeavy.Value = new decimal(new int[] {
            11,
            0,
            0,
            0});
            // 
            // checkBoxToolsBreakPreferBottomHeavy
            // 
            this.checkBoxToolsBreakPreferBottomHeavy.AutoSize = true;
            this.checkBoxToolsBreakPreferBottomHeavy.Location = new System.Drawing.Point(32, 139);
            this.checkBoxToolsBreakPreferBottomHeavy.Name = "checkBoxToolsBreakPreferBottomHeavy";
            this.checkBoxToolsBreakPreferBottomHeavy.Size = new System.Drawing.Size(126, 17);
            this.checkBoxToolsBreakPreferBottomHeavy.TabIndex = 61;
            this.checkBoxToolsBreakPreferBottomHeavy.Text = "Prefer bottom heavy";
            this.checkBoxToolsBreakPreferBottomHeavy.UseVisualStyleBackColor = true;
            // 
            // checkBoxToolsBreakByPixelWidth
            // 
            this.checkBoxToolsBreakByPixelWidth.AutoSize = true;
            this.checkBoxToolsBreakByPixelWidth.Location = new System.Drawing.Point(15, 116);
            this.checkBoxToolsBreakByPixelWidth.Name = "checkBoxToolsBreakByPixelWidth";
            this.checkBoxToolsBreakByPixelWidth.Size = new System.Drawing.Size(172, 17);
            this.checkBoxToolsBreakByPixelWidth.TabIndex = 60;
            this.checkBoxToolsBreakByPixelWidth.Text = "Break by estimated pixel width";
            this.checkBoxToolsBreakByPixelWidth.UseVisualStyleBackColor = true;
            this.checkBoxToolsBreakByPixelWidth.CheckedChanged += new System.EventHandler(this.checkBoxToolsBreakByPixelWidth_CheckedChanged);
            // 
            // checkBoxToolsBreakEarlyLineEnding
            // 
            this.checkBoxToolsBreakEarlyLineEnding.AutoSize = true;
            this.checkBoxToolsBreakEarlyLineEnding.Location = new System.Drawing.Point(15, 70);
            this.checkBoxToolsBreakEarlyLineEnding.Name = "checkBoxToolsBreakEarlyLineEnding";
            this.checkBoxToolsBreakEarlyLineEnding.Size = new System.Drawing.Size(175, 17);
            this.checkBoxToolsBreakEarlyLineEnding.TabIndex = 40;
            this.checkBoxToolsBreakEarlyLineEnding.Text = "Break early for line ending (.!?)";
            this.checkBoxToolsBreakEarlyLineEnding.UseVisualStyleBackColor = true;
            // 
            // checkBoxToolsBreakEarlyComma
            // 
            this.checkBoxToolsBreakEarlyComma.AutoSize = true;
            this.checkBoxToolsBreakEarlyComma.Location = new System.Drawing.Point(15, 93);
            this.checkBoxToolsBreakEarlyComma.Name = "checkBoxToolsBreakEarlyComma";
            this.checkBoxToolsBreakEarlyComma.Size = new System.Drawing.Size(133, 17);
            this.checkBoxToolsBreakEarlyComma.TabIndex = 50;
            this.checkBoxToolsBreakEarlyComma.Text = "Break early for comma";
            this.checkBoxToolsBreakEarlyComma.UseVisualStyleBackColor = true;
            // 
            // checkBoxToolsBreakEarlyDash
            // 
            this.checkBoxToolsBreakEarlyDash.AutoSize = true;
            this.checkBoxToolsBreakEarlyDash.Location = new System.Drawing.Point(15, 47);
            this.checkBoxToolsBreakEarlyDash.Name = "checkBoxToolsBreakEarlyDash";
            this.checkBoxToolsBreakEarlyDash.Size = new System.Drawing.Size(123, 17);
            this.checkBoxToolsBreakEarlyDash.TabIndex = 30;
            this.checkBoxToolsBreakEarlyDash.Text = "Break early for dash";
            this.checkBoxToolsBreakEarlyDash.UseVisualStyleBackColor = true;
            // 
            // labelUserBingApiId
            // 
            this.labelUserBingApiId.AutoSize = true;
            this.labelUserBingApiId.Location = new System.Drawing.Point(16, 106);
            this.labelUserBingApiId.Name = "labelUserBingApiId";
            this.labelUserBingApiId.Size = new System.Drawing.Size(0, 13);
            this.labelUserBingApiId.TabIndex = 25;
            // 
            // buttonEditDoNotBreakAfterList
            // 
            this.buttonEditDoNotBreakAfterList.Location = new System.Drawing.Point(239, 20);
            this.buttonEditDoNotBreakAfterList.Name = "buttonEditDoNotBreakAfterList";
            this.buttonEditDoNotBreakAfterList.Size = new System.Drawing.Size(75, 23);
            this.buttonEditDoNotBreakAfterList.TabIndex = 20;
            this.buttonEditDoNotBreakAfterList.Text = "Edit";
            this.buttonEditDoNotBreakAfterList.UseVisualStyleBackColor = true;
            this.buttonEditDoNotBreakAfterList.Click += new System.EventHandler(this.buttonEditDoNotBreakAfterList_Click);
            // 
            // checkBoxUseDoNotBreakAfterList
            // 
            this.checkBoxUseDoNotBreakAfterList.AutoSize = true;
            this.checkBoxUseDoNotBreakAfterList.Location = new System.Drawing.Point(15, 24);
            this.checkBoxUseDoNotBreakAfterList.Name = "checkBoxUseDoNotBreakAfterList";
            this.checkBoxUseDoNotBreakAfterList.Size = new System.Drawing.Size(154, 17);
            this.checkBoxUseDoNotBreakAfterList.TabIndex = 1;
            this.checkBoxUseDoNotBreakAfterList.Text = "Use \'do-not-beak-after\' list";
            this.checkBoxUseDoNotBreakAfterList.UseVisualStyleBackColor = true;
            // 
            // groupBoxSpellCheck
            // 
            this.groupBoxSpellCheck.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxSpellCheck.Controls.Add(this.checkBoxSpellCheckAutoChangeNamesViaSuggestions);
            this.groupBoxSpellCheck.Controls.Add(this.checkBoxUseAlwaysToFile);
            this.groupBoxSpellCheck.Controls.Add(this.checkBoxTreatINQuoteAsING);
            this.groupBoxSpellCheck.Controls.Add(this.checkBoxSpellCheckOneLetterWords);
            this.groupBoxSpellCheck.Controls.Add(this.checkBoxSpellCheckAutoChangeNames);
            this.groupBoxSpellCheck.Location = new System.Drawing.Point(0, 389);
            this.groupBoxSpellCheck.Name = "groupBoxSpellCheck";
            this.groupBoxSpellCheck.Size = new System.Drawing.Size(408, 131);
            this.groupBoxSpellCheck.TabIndex = 4;
            this.groupBoxSpellCheck.TabStop = false;
            this.groupBoxSpellCheck.Text = "Spell check";
            // 
            // checkBoxSpellCheckAutoChangeNamesViaSuggestions
            // 
            this.checkBoxSpellCheckAutoChangeNamesViaSuggestions.AutoSize = true;
            this.checkBoxSpellCheckAutoChangeNamesViaSuggestions.Location = new System.Drawing.Point(30, 40);
            this.checkBoxSpellCheckAutoChangeNamesViaSuggestions.Name = "checkBoxSpellCheckAutoChangeNamesViaSuggestions";
            this.checkBoxSpellCheckAutoChangeNamesViaSuggestions.Size = new System.Drawing.Size(184, 17);
            this.checkBoxSpellCheckAutoChangeNamesViaSuggestions.TabIndex = 1;
            this.checkBoxSpellCheckAutoChangeNamesViaSuggestions.Text = "Also use \'spell check suggestions\'";
            this.checkBoxSpellCheckAutoChangeNamesViaSuggestions.UseVisualStyleBackColor = true;
            // 
            // checkBoxUseAlwaysToFile
            // 
            this.checkBoxUseAlwaysToFile.AutoSize = true;
            this.checkBoxUseAlwaysToFile.Location = new System.Drawing.Point(15, 102);
            this.checkBoxUseAlwaysToFile.Name = "checkBoxUseAlwaysToFile";
            this.checkBoxUseAlwaysToFile.Size = new System.Drawing.Size(192, 17);
            this.checkBoxUseAlwaysToFile.TabIndex = 4;
            this.checkBoxUseAlwaysToFile.Text = "Remember \"Use always\" / \"Skip all\"";
            this.checkBoxUseAlwaysToFile.UseVisualStyleBackColor = true;
            // 
            // checkBoxTreatINQuoteAsING
            // 
            this.checkBoxTreatINQuoteAsING.AutoSize = true;
            this.checkBoxTreatINQuoteAsING.Location = new System.Drawing.Point(15, 82);
            this.checkBoxTreatINQuoteAsING.Name = "checkBoxTreatINQuoteAsING";
            this.checkBoxTreatINQuoteAsING.Size = new System.Drawing.Size(253, 17);
            this.checkBoxTreatINQuoteAsING.TabIndex = 3;
            this.checkBoxTreatINQuoteAsING.Text = "Treat word ending \" in\' \" as \" ing \" (English only)";
            this.checkBoxTreatINQuoteAsING.UseVisualStyleBackColor = true;
            // 
            // checkBoxSpellCheckOneLetterWords
            // 
            this.checkBoxSpellCheckOneLetterWords.AutoSize = true;
            this.checkBoxSpellCheckOneLetterWords.Location = new System.Drawing.Point(15, 62);
            this.checkBoxSpellCheckOneLetterWords.Name = "checkBoxSpellCheckOneLetterWords";
            this.checkBoxSpellCheckOneLetterWords.Size = new System.Drawing.Size(205, 17);
            this.checkBoxSpellCheckOneLetterWords.TabIndex = 2;
            this.checkBoxSpellCheckOneLetterWords.Text = "Prompt for unknown one letter words";
            this.checkBoxSpellCheckOneLetterWords.UseVisualStyleBackColor = true;
            // 
            // checkBoxSpellCheckAutoChangeNames
            // 
            this.checkBoxSpellCheckAutoChangeNames.AutoSize = true;
            this.checkBoxSpellCheckAutoChangeNames.Location = new System.Drawing.Point(15, 20);
            this.checkBoxSpellCheckAutoChangeNames.Name = "checkBoxSpellCheckAutoChangeNames";
            this.checkBoxSpellCheckAutoChangeNames.Size = new System.Drawing.Size(221, 17);
            this.checkBoxSpellCheckAutoChangeNames.TabIndex = 0;
            this.checkBoxSpellCheckAutoChangeNames.Text = "Auto fix names where only casing differs";
            this.checkBoxSpellCheckAutoChangeNames.UseVisualStyleBackColor = true;
            // 
            // groupBoxFixCommonErrors
            // 
            this.groupBoxFixCommonErrors.Controls.Add(this.checkBoxUseWordSplitListAvoidPropercase);
            this.groupBoxFixCommonErrors.Controls.Add(this.checkBoxUseWordSplitList);
            this.groupBoxFixCommonErrors.Controls.Add(this.buttonFixContinuationStyleSettings);
            this.groupBoxFixCommonErrors.Controls.Add(this.checkBoxFceSkipStep1);
            this.groupBoxFixCommonErrors.Controls.Add(this.checkBoxFixShortDisplayTimesAllowMoveStartTime);
            this.groupBoxFixCommonErrors.Controls.Add(this.checkBoxFixCommonOcrErrorsUsingHardcodedRules);
            this.groupBoxFixCommonErrors.Controls.Add(this.comboBoxToolsMusicSymbol);
            this.groupBoxFixCommonErrors.Controls.Add(this.textBoxMusicSymbolsToReplace);
            this.groupBoxFixCommonErrors.Controls.Add(this.labelToolsMusicSymbolsToReplace);
            this.groupBoxFixCommonErrors.Controls.Add(this.labelToolsMusicSymbol);
            this.groupBoxFixCommonErrors.Location = new System.Drawing.Point(0, 123);
            this.groupBoxFixCommonErrors.Name = "groupBoxFixCommonErrors";
            this.groupBoxFixCommonErrors.Size = new System.Drawing.Size(408, 260);
            this.groupBoxFixCommonErrors.TabIndex = 3;
            this.groupBoxFixCommonErrors.TabStop = false;
            this.groupBoxFixCommonErrors.Text = "Fix common errors";
            // 
            // checkBoxUseWordSplitListAvoidPropercase
            // 
            this.checkBoxUseWordSplitListAvoidPropercase.AutoSize = true;
            this.checkBoxUseWordSplitListAvoidPropercase.Location = new System.Drawing.Point(34, 146);
            this.checkBoxUseWordSplitListAvoidPropercase.Name = "checkBoxUseWordSplitListAvoidPropercase";
            this.checkBoxUseWordSplitListAvoidPropercase.Size = new System.Drawing.Size(102, 17);
            this.checkBoxUseWordSplitListAvoidPropercase.TabIndex = 4;
            this.checkBoxUseWordSplitListAvoidPropercase.Text = "Skip propercase";
            this.checkBoxUseWordSplitListAvoidPropercase.UseVisualStyleBackColor = true;
            // 
            // checkBoxUseWordSplitList
            // 
            this.checkBoxUseWordSplitList.AutoSize = true;
            this.checkBoxUseWordSplitList.Location = new System.Drawing.Point(15, 125);
            this.checkBoxUseWordSplitList.Name = "checkBoxUseWordSplitList";
            this.checkBoxUseWordSplitList.Size = new System.Drawing.Size(231, 17);
            this.checkBoxUseWordSplitList.TabIndex = 3;
            this.checkBoxUseWordSplitList.Text = "Fix common OCR errors - use word split list";
            this.checkBoxUseWordSplitList.UseVisualStyleBackColor = true;
            // 
            // buttonFixContinuationStyleSettings
            // 
            this.buttonFixContinuationStyleSettings.Location = new System.Drawing.Point(16, 220);
            this.buttonFixContinuationStyleSettings.Name = "buttonFixContinuationStyleSettings";
            this.buttonFixContinuationStyleSettings.Size = new System.Drawing.Size(271, 23);
            this.buttonFixContinuationStyleSettings.TabIndex = 7;
            this.buttonFixContinuationStyleSettings.Text = "Edit settings for fixing continuation style...";
            this.buttonFixContinuationStyleSettings.UseVisualStyleBackColor = true;
            this.buttonFixContinuationStyleSettings.Click += new System.EventHandler(this.buttonFixContinuationStyleSettings_Click);
            // 
            // checkBoxFceSkipStep1
            // 
            this.checkBoxFceSkipStep1.AutoSize = true;
            this.checkBoxFceSkipStep1.Location = new System.Drawing.Point(15, 193);
            this.checkBoxFceSkipStep1.Name = "checkBoxFceSkipStep1";
            this.checkBoxFceSkipStep1.Size = new System.Drawing.Size(176, 17);
            this.checkBoxFceSkipStep1.TabIndex = 6;
            this.checkBoxFceSkipStep1.Text = "Skip step one (choose fix rules)";
            this.checkBoxFceSkipStep1.UseVisualStyleBackColor = true;
            // 
            // checkBoxFixShortDisplayTimesAllowMoveStartTime
            // 
            this.checkBoxFixShortDisplayTimesAllowMoveStartTime.AutoSize = true;
            this.checkBoxFixShortDisplayTimesAllowMoveStartTime.Location = new System.Drawing.Point(15, 170);
            this.checkBoxFixShortDisplayTimesAllowMoveStartTime.Name = "checkBoxFixShortDisplayTimesAllowMoveStartTime";
            this.checkBoxFixShortDisplayTimesAllowMoveStartTime.Size = new System.Drawing.Size(252, 17);
            this.checkBoxFixShortDisplayTimesAllowMoveStartTime.TabIndex = 5;
            this.checkBoxFixShortDisplayTimesAllowMoveStartTime.Text = "Fix short display time - allow move of start time";
            this.checkBoxFixShortDisplayTimesAllowMoveStartTime.UseVisualStyleBackColor = true;
            // 
            // checkBoxFixCommonOcrErrorsUsingHardcodedRules
            // 
            this.checkBoxFixCommonOcrErrorsUsingHardcodedRules.AutoSize = true;
            this.checkBoxFixCommonOcrErrorsUsingHardcodedRules.Location = new System.Drawing.Point(15, 103);
            this.checkBoxFixCommonOcrErrorsUsingHardcodedRules.Name = "checkBoxFixCommonOcrErrorsUsingHardcodedRules";
            this.checkBoxFixCommonOcrErrorsUsingHardcodedRules.Size = new System.Drawing.Size(268, 17);
            this.checkBoxFixCommonOcrErrorsUsingHardcodedRules.TabIndex = 2;
            this.checkBoxFixCommonOcrErrorsUsingHardcodedRules.Text = "Fix common OCR errors - also use hardcoded rules";
            this.checkBoxFixCommonOcrErrorsUsingHardcodedRules.UseVisualStyleBackColor = true;
            // 
            // comboBoxToolsMusicSymbol
            // 
            this.comboBoxToolsMusicSymbol.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxToolsMusicSymbol.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxToolsMusicSymbol.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxToolsMusicSymbol.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxToolsMusicSymbol.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxToolsMusicSymbol.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxToolsMusicSymbol.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxToolsMusicSymbol.DropDownHeight = 400;
            this.comboBoxToolsMusicSymbol.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxToolsMusicSymbol.DropDownWidth = 86;
            this.comboBoxToolsMusicSymbol.FormattingEnabled = true;
            this.comboBoxToolsMusicSymbol.Items.AddRange(new string[] {
            "♪",
            "♪♪",
            "*",
            "#"});
            this.comboBoxToolsMusicSymbol.Location = new System.Drawing.Point(200, 71);
            this.comboBoxToolsMusicSymbol.MaxLength = 32767;
            this.comboBoxToolsMusicSymbol.Name = "comboBoxToolsMusicSymbol";
            this.comboBoxToolsMusicSymbol.SelectedIndex = -1;
            this.comboBoxToolsMusicSymbol.SelectedItem = null;
            this.comboBoxToolsMusicSymbol.SelectedText = "";
            this.comboBoxToolsMusicSymbol.Size = new System.Drawing.Size(86, 21);
            this.comboBoxToolsMusicSymbol.TabIndex = 1;
            this.comboBoxToolsMusicSymbol.UsePopupWindow = false;
            // 
            // textBoxMusicSymbolsToReplace
            // 
            this.textBoxMusicSymbolsToReplace.BackColor = System.Drawing.SystemColors.Window;
            this.textBoxMusicSymbolsToReplace.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxMusicSymbolsToReplace.Location = new System.Drawing.Point(11, 41);
            this.textBoxMusicSymbolsToReplace.MaxLength = 100;
            this.textBoxMusicSymbolsToReplace.Multiline = true;
            this.textBoxMusicSymbolsToReplace.Name = "textBoxMusicSymbolsToReplace";
            this.textBoxMusicSymbolsToReplace.Size = new System.Drawing.Size(275, 21);
            this.textBoxMusicSymbolsToReplace.TabIndex = 0;
            // 
            // labelToolsMusicSymbolsToReplace
            // 
            this.labelToolsMusicSymbolsToReplace.AutoSize = true;
            this.labelToolsMusicSymbolsToReplace.Location = new System.Drawing.Point(8, 25);
            this.labelToolsMusicSymbolsToReplace.Name = "labelToolsMusicSymbolsToReplace";
            this.labelToolsMusicSymbolsToReplace.Size = new System.Drawing.Size(230, 13);
            this.labelToolsMusicSymbolsToReplace.TabIndex = 34;
            this.labelToolsMusicSymbolsToReplace.Text = "Music symbols to replace (separate by comma)";
            // 
            // labelToolsMusicSymbol
            // 
            this.labelToolsMusicSymbol.AutoSize = true;
            this.labelToolsMusicSymbol.Location = new System.Drawing.Point(8, 74);
            this.labelToolsMusicSymbol.Name = "labelToolsMusicSymbol";
            this.labelToolsMusicSymbol.Size = new System.Drawing.Size(69, 13);
            this.labelToolsMusicSymbol.TabIndex = 32;
            this.labelToolsMusicSymbol.Text = "Music symbol";
            // 
            // groupBoxToolsVisualSync
            // 
            this.groupBoxToolsVisualSync.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxToolsVisualSync.Controls.Add(this.labelToolsEndScene);
            this.groupBoxToolsVisualSync.Controls.Add(this.comboBoxToolsEndSceneIndex);
            this.groupBoxToolsVisualSync.Controls.Add(this.labelToolsStartScene);
            this.groupBoxToolsVisualSync.Controls.Add(this.comboBoxToolsStartSceneIndex);
            this.groupBoxToolsVisualSync.Controls.Add(this.comboBoxToolsVerifySeconds);
            this.groupBoxToolsVisualSync.Controls.Add(this.labelVerifyButton);
            this.groupBoxToolsVisualSync.Location = new System.Drawing.Point(0, 0);
            this.groupBoxToolsVisualSync.Name = "groupBoxToolsVisualSync";
            this.groupBoxToolsVisualSync.Size = new System.Drawing.Size(409, 116);
            this.groupBoxToolsVisualSync.TabIndex = 2;
            this.groupBoxToolsVisualSync.TabStop = false;
            this.groupBoxToolsVisualSync.Text = "Visual sync";
            // 
            // labelToolsEndScene
            // 
            this.labelToolsEndScene.AutoSize = true;
            this.labelToolsEndScene.Location = new System.Drawing.Point(13, 79);
            this.labelToolsEndScene.Name = "labelToolsEndScene";
            this.labelToolsEndScene.Size = new System.Drawing.Size(122, 13);
            this.labelToolsEndScene.TabIndex = 29;
            this.labelToolsEndScene.Text = "End scene paragraph is ";
            // 
            // comboBoxToolsEndSceneIndex
            // 
            this.comboBoxToolsEndSceneIndex.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxToolsEndSceneIndex.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxToolsEndSceneIndex.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxToolsEndSceneIndex.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxToolsEndSceneIndex.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxToolsEndSceneIndex.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxToolsEndSceneIndex.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxToolsEndSceneIndex.DropDownHeight = 400;
            this.comboBoxToolsEndSceneIndex.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxToolsEndSceneIndex.DropDownWidth = 86;
            this.comboBoxToolsEndSceneIndex.FormattingEnabled = true;
            this.comboBoxToolsEndSceneIndex.Items.AddRange(new string[] {
            "Last",
            "Last - 1",
            "Last - 2",
            "Last - 3"});
            this.comboBoxToolsEndSceneIndex.Location = new System.Drawing.Point(200, 76);
            this.comboBoxToolsEndSceneIndex.MaxLength = 32767;
            this.comboBoxToolsEndSceneIndex.Name = "comboBoxToolsEndSceneIndex";
            this.comboBoxToolsEndSceneIndex.SelectedIndex = -1;
            this.comboBoxToolsEndSceneIndex.SelectedItem = null;
            this.comboBoxToolsEndSceneIndex.SelectedText = "";
            this.comboBoxToolsEndSceneIndex.Size = new System.Drawing.Size(86, 21);
            this.comboBoxToolsEndSceneIndex.TabIndex = 2;
            this.comboBoxToolsEndSceneIndex.UsePopupWindow = false;
            // 
            // labelToolsStartScene
            // 
            this.labelToolsStartScene.AutoSize = true;
            this.labelToolsStartScene.Location = new System.Drawing.Point(13, 52);
            this.labelToolsStartScene.Name = "labelToolsStartScene";
            this.labelToolsStartScene.Size = new System.Drawing.Size(125, 13);
            this.labelToolsStartScene.TabIndex = 27;
            this.labelToolsStartScene.Text = "Start scene paragraph is";
            // 
            // comboBoxToolsStartSceneIndex
            // 
            this.comboBoxToolsStartSceneIndex.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxToolsStartSceneIndex.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxToolsStartSceneIndex.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxToolsStartSceneIndex.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxToolsStartSceneIndex.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxToolsStartSceneIndex.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxToolsStartSceneIndex.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxToolsStartSceneIndex.DropDownHeight = 400;
            this.comboBoxToolsStartSceneIndex.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxToolsStartSceneIndex.DropDownWidth = 86;
            this.comboBoxToolsStartSceneIndex.FormattingEnabled = true;
            this.comboBoxToolsStartSceneIndex.Items.AddRange(new string[] {
            "First",
            "First +1",
            "First +2",
            "First +3"});
            this.comboBoxToolsStartSceneIndex.Location = new System.Drawing.Point(200, 49);
            this.comboBoxToolsStartSceneIndex.MaxLength = 32767;
            this.comboBoxToolsStartSceneIndex.Name = "comboBoxToolsStartSceneIndex";
            this.comboBoxToolsStartSceneIndex.SelectedIndex = -1;
            this.comboBoxToolsStartSceneIndex.SelectedItem = null;
            this.comboBoxToolsStartSceneIndex.SelectedText = "";
            this.comboBoxToolsStartSceneIndex.Size = new System.Drawing.Size(86, 21);
            this.comboBoxToolsStartSceneIndex.TabIndex = 1;
            this.comboBoxToolsStartSceneIndex.UsePopupWindow = false;
            // 
            // comboBoxToolsVerifySeconds
            // 
            this.comboBoxToolsVerifySeconds.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxToolsVerifySeconds.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxToolsVerifySeconds.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxToolsVerifySeconds.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxToolsVerifySeconds.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxToolsVerifySeconds.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxToolsVerifySeconds.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxToolsVerifySeconds.DropDownHeight = 400;
            this.comboBoxToolsVerifySeconds.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxToolsVerifySeconds.DropDownWidth = 86;
            this.comboBoxToolsVerifySeconds.FormattingEnabled = true;
            this.comboBoxToolsVerifySeconds.Items.AddRange(new string[] {
            "2",
            "3",
            "4",
            "5"});
            this.comboBoxToolsVerifySeconds.Location = new System.Drawing.Point(200, 22);
            this.comboBoxToolsVerifySeconds.MaxLength = 32767;
            this.comboBoxToolsVerifySeconds.Name = "comboBoxToolsVerifySeconds";
            this.comboBoxToolsVerifySeconds.SelectedIndex = -1;
            this.comboBoxToolsVerifySeconds.SelectedItem = null;
            this.comboBoxToolsVerifySeconds.SelectedText = "";
            this.comboBoxToolsVerifySeconds.Size = new System.Drawing.Size(86, 21);
            this.comboBoxToolsVerifySeconds.TabIndex = 0;
            this.comboBoxToolsVerifySeconds.UsePopupWindow = false;
            // 
            // labelVerifyButton
            // 
            this.labelVerifyButton.AutoSize = true;
            this.labelVerifyButton.Location = new System.Drawing.Point(13, 25);
            this.labelVerifyButton.Name = "labelVerifyButton";
            this.labelVerifyButton.Size = new System.Drawing.Size(147, 13);
            this.labelVerifyButton.TabIndex = 3;
            this.labelVerifyButton.Text = "Play X seconds and back, X is";
            // 
            // groupBoxGoogleTranslate
            // 
            this.groupBoxGoogleTranslate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxGoogleTranslate.Controls.Add(this.labelGoogleTranslateApiKey);
            this.groupBoxGoogleTranslate.Controls.Add(this.textBoxGoogleTransleApiKey);
            this.groupBoxGoogleTranslate.Controls.Add(this.linkLabelGoogleTranslateSignUp);
            this.groupBoxGoogleTranslate.Controls.Add(this.label3);
            this.groupBoxGoogleTranslate.Location = new System.Drawing.Point(9, 9);
            this.groupBoxGoogleTranslate.Name = "groupBoxGoogleTranslate";
            this.groupBoxGoogleTranslate.Size = new System.Drawing.Size(410, 79);
            this.groupBoxGoogleTranslate.TabIndex = 31;
            this.groupBoxGoogleTranslate.TabStop = false;
            this.groupBoxGoogleTranslate.Text = "Google translate";
            // 
            // labelGoogleTranslateApiKey
            // 
            this.labelGoogleTranslateApiKey.AutoSize = true;
            this.labelGoogleTranslateApiKey.Location = new System.Drawing.Point(8, 25);
            this.labelGoogleTranslateApiKey.Name = "labelGoogleTranslateApiKey";
            this.labelGoogleTranslateApiKey.Size = new System.Drawing.Size(44, 13);
            this.labelGoogleTranslateApiKey.TabIndex = 30;
            this.labelGoogleTranslateApiKey.Text = "API key";
            // 
            // textBoxGoogleTransleApiKey
            // 
            this.textBoxGoogleTransleApiKey.BackColor = System.Drawing.SystemColors.Window;
            this.textBoxGoogleTransleApiKey.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxGoogleTransleApiKey.Location = new System.Drawing.Point(9, 43);
            this.textBoxGoogleTransleApiKey.Multiline = true;
            this.textBoxGoogleTransleApiKey.Name = "textBoxGoogleTransleApiKey";
            this.textBoxGoogleTransleApiKey.Size = new System.Drawing.Size(375, 21);
            this.textBoxGoogleTransleApiKey.TabIndex = 26;
            // 
            // linkLabelGoogleTranslateSignUp
            // 
            this.linkLabelGoogleTranslateSignUp.AutoSize = true;
            this.linkLabelGoogleTranslateSignUp.Location = new System.Drawing.Point(306, 19);
            this.linkLabelGoogleTranslateSignUp.Name = "linkLabelGoogleTranslateSignUp";
            this.linkLabelGoogleTranslateSignUp.Size = new System.Drawing.Size(78, 13);
            this.linkLabelGoogleTranslateSignUp.TabIndex = 24;
            this.linkLabelGoogleTranslateSignUp.TabStop = true;
            this.linkLabelGoogleTranslateSignUp.Text = "How to sign up";
            this.linkLabelGoogleTranslateSignUp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelGoogleTranslateSignUp_LinkClicked);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(16, 106);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(0, 13);
            this.label3.TabIndex = 25;
            // 
            // groupBoxBing
            // 
            this.groupBoxBing.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxBing.Controls.Add(this.comboBoxBoxBingTokenEndpoint);
            this.groupBoxBing.Controls.Add(this.labelBingTokenEndpoint);
            this.groupBoxBing.Controls.Add(this.labelBingApiKey);
            this.groupBoxBing.Controls.Add(this.textBoxBingClientSecret);
            this.groupBoxBing.Controls.Add(this.linkLabelBingSubscribe);
            this.groupBoxBing.Controls.Add(this.label1);
            this.groupBoxBing.Location = new System.Drawing.Point(9, 95);
            this.groupBoxBing.Name = "groupBoxBing";
            this.groupBoxBing.Size = new System.Drawing.Size(410, 111);
            this.groupBoxBing.TabIndex = 32;
            this.groupBoxBing.TabStop = false;
            this.groupBoxBing.Text = "Bing translator";
            // 
            // comboBoxBoxBingTokenEndpoint
            // 
            this.comboBoxBoxBingTokenEndpoint.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxBoxBingTokenEndpoint.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxBoxBingTokenEndpoint.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxBoxBingTokenEndpoint.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxBoxBingTokenEndpoint.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxBoxBingTokenEndpoint.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxBoxBingTokenEndpoint.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxBoxBingTokenEndpoint.DropDownHeight = 400;
            this.comboBoxBoxBingTokenEndpoint.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.comboBoxBoxBingTokenEndpoint.DropDownWidth = 375;
            this.comboBoxBoxBingTokenEndpoint.FormattingEnabled = true;
            this.comboBoxBoxBingTokenEndpoint.Items.AddRange(new string[] {
            "https://api.cognitive.microsoft.com/sts/v1.0/issueToken",
            "https://australiaeast.api.cognitive.microsoft.com/sts/v1.0/issueToken",
            "https://canadacentral.api.cognitive.microsoft.com/sts/v1.0/issueToken",
            "https://centralus.api.cognitive.microsoft.com/sts/v1.0/issueToken",
            "https://eastasia.api.cognitive.microsoft.com/sts/v1.0/issueToken",
            "https://eastus.api.cognitive.microsoft.com/sts/v1.0/issueToken",
            "https://eastus2.api.cognitive.microsoft.com/sts/v1.0/issueToken",
            "https://francecentral.api.cognitive.microsoft.com/sts/v1.0/issueToken",
            "https://centralindia.api.cognitive.microsoft.com/sts/v1.0/issueToken",
            "https://japaneast.api.cognitive.microsoft.com/sts/v1.0/issueToken",
            "https://koreacentral.api.cognitive.microsoft.com/sts/v1.0/issueToken",
            "https://northcentralus.api.cognitive.microsoft.com/sts/v1.0/issueToken",
            "https://northeurope.api.cognitive.microsoft.com/sts/v1.0/issueToken",
            "https://southcentralus.api.cognitive.microsoft.com/sts/v1.0/issueToken",
            "https://southeastasia.api.cognitive.microsoft.com/sts/v1.0/issueToken",
            "https://uksouth.api.cognitive.microsoft.com/sts/v1.0/issueToken",
            "https://westeurope.api.cognitive.microsoft.com/sts/v1.0/issueToken",
            "https://westus.api.cognitive.microsoft.com/sts/v1.0/issueToken",
            "https://westus2.api.cognitive.microsoft.com/sts/v1.0/issueToken"});
            this.comboBoxBoxBingTokenEndpoint.Location = new System.Drawing.Point(90, 77);
            this.comboBoxBoxBingTokenEndpoint.MaxLength = 32767;
            this.comboBoxBoxBingTokenEndpoint.Name = "comboBoxBoxBingTokenEndpoint";
            this.comboBoxBoxBingTokenEndpoint.SelectedIndex = -1;
            this.comboBoxBoxBingTokenEndpoint.SelectedItem = null;
            this.comboBoxBoxBingTokenEndpoint.SelectedText = "";
            this.comboBoxBoxBingTokenEndpoint.Size = new System.Drawing.Size(294, 21);
            this.comboBoxBoxBingTokenEndpoint.TabIndex = 33;
            this.comboBoxBoxBingTokenEndpoint.TabStop = false;
            this.comboBoxBoxBingTokenEndpoint.UsePopupWindow = false;
            // 
            // labelBingTokenEndpoint
            // 
            this.labelBingTokenEndpoint.AutoSize = true;
            this.labelBingTokenEndpoint.Location = new System.Drawing.Point(6, 80);
            this.labelBingTokenEndpoint.Name = "labelBingTokenEndpoint";
            this.labelBingTokenEndpoint.Size = new System.Drawing.Size(81, 13);
            this.labelBingTokenEndpoint.TabIndex = 32;
            this.labelBingTokenEndpoint.Text = "Token endpoint";
            // 
            // labelBingApiKey
            // 
            this.labelBingApiKey.AutoSize = true;
            this.labelBingApiKey.Location = new System.Drawing.Point(6, 24);
            this.labelBingApiKey.Name = "labelBingApiKey";
            this.labelBingApiKey.Size = new System.Drawing.Size(44, 13);
            this.labelBingApiKey.TabIndex = 30;
            this.labelBingApiKey.Text = "API key";
            // 
            // textBoxBingClientSecret
            // 
            this.textBoxBingClientSecret.BackColor = System.Drawing.SystemColors.Window;
            this.textBoxBingClientSecret.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxBingClientSecret.Location = new System.Drawing.Point(10, 42);
            this.textBoxBingClientSecret.Multiline = true;
            this.textBoxBingClientSecret.Name = "textBoxBingClientSecret";
            this.textBoxBingClientSecret.Size = new System.Drawing.Size(374, 21);
            this.textBoxBingClientSecret.TabIndex = 26;
            // 
            // linkLabelBingSubscribe
            // 
            this.linkLabelBingSubscribe.AutoSize = true;
            this.linkLabelBingSubscribe.Location = new System.Drawing.Point(306, 17);
            this.linkLabelBingSubscribe.Name = "linkLabelBingSubscribe";
            this.linkLabelBingSubscribe.Size = new System.Drawing.Size(78, 13);
            this.linkLabelBingSubscribe.TabIndex = 24;
            this.linkLabelBingSubscribe.TabStop = true;
            this.linkLabelBingSubscribe.Text = "How to sign up";
            this.linkLabelBingSubscribe.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelBingSubscribe_LinkClicked);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 106);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(0, 13);
            this.label1.TabIndex = 25;
            // 
            // panelToolBar
            // 
            this.panelToolBar.Controls.Add(this.groupBox2);
            this.panelToolBar.Controls.Add(this.groupBoxShowToolBarButtons);
            this.panelToolBar.Location = new System.Drawing.Point(230, 6);
            this.panelToolBar.Name = "panelToolBar";
            this.panelToolBar.Padding = new System.Windows.Forms.Padding(3);
            this.panelToolBar.Size = new System.Drawing.Size(864, 521);
            this.panelToolBar.TabIndex = 9;
            this.panelToolBar.Text = "Toolbar ";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.checkBoxShowFrameRate);
            this.groupBox2.Location = new System.Drawing.Point(0, 449);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(850, 70);
            this.groupBox2.TabIndex = 35;
            this.groupBox2.TabStop = false;
            // 
            // checkBoxShowFrameRate
            // 
            this.checkBoxShowFrameRate.AutoSize = true;
            this.checkBoxShowFrameRate.Location = new System.Drawing.Point(16, 20);
            this.checkBoxShowFrameRate.Name = "checkBoxShowFrameRate";
            this.checkBoxShowFrameRate.Size = new System.Drawing.Size(154, 17);
            this.checkBoxShowFrameRate.TabIndex = 47;
            this.checkBoxShowFrameRate.Text = "Show frame rate in toolbar";
            this.checkBoxShowFrameRate.UseVisualStyleBackColor = true;
            // 
            // groupBoxShowToolBarButtons
            // 
            this.groupBoxShowToolBarButtons.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxShowToolBarButtons.Controls.Add(this.labelTBOpenVideo);
            this.groupBoxShowToolBarButtons.Controls.Add(this.pictureBoxTBOpenVideo);
            this.groupBoxShowToolBarButtons.Controls.Add(this.checkBoxTBOpenVideo);
            this.groupBoxShowToolBarButtons.Controls.Add(this.pictureBoxWebVttStyle);
            this.groupBoxShowToolBarButtons.Controls.Add(this.pictureBoxEbuProperties);
            this.groupBoxShowToolBarButtons.Controls.Add(this.pictureBoxWebVttProperties);
            this.groupBoxShowToolBarButtons.Controls.Add(this.pictureBoxIttProperties);
            this.groupBoxShowToolBarButtons.Controls.Add(this.pictureBoxToggleVideo);
            this.groupBoxShowToolBarButtons.Controls.Add(this.pictureBoxToggleWaveform);
            this.groupBoxShowToolBarButtons.Controls.Add(this.pictureBoxAssaDraw);
            this.groupBoxShowToolBarButtons.Controls.Add(this.pictureBoxAssAttachments);
            this.groupBoxShowToolBarButtons.Controls.Add(this.pictureBoxAssProperties);
            this.groupBoxShowToolBarButtons.Controls.Add(this.pictureBoxAssStyleManager);
            this.groupBoxShowToolBarButtons.Controls.Add(this.labelTBToggleSourceView);
            this.groupBoxShowToolBarButtons.Controls.Add(this.pictureBoxTBToggleSourceView);
            this.groupBoxShowToolBarButtons.Controls.Add(this.checkBoxTBToggleSourceView);
            this.groupBoxShowToolBarButtons.Controls.Add(this.labelTBBurnIn);
            this.groupBoxShowToolBarButtons.Controls.Add(this.pictureBoxTBBurnIn);
            this.groupBoxShowToolBarButtons.Controls.Add(this.checkBoxTBBurnIn);
            this.groupBoxShowToolBarButtons.Controls.Add(this.labelTBBeautifyTimeCodes);
            this.groupBoxShowToolBarButtons.Controls.Add(this.pictureBoxTBBeautifyTimeCodes);
            this.groupBoxShowToolBarButtons.Controls.Add(this.checkBoxTBBeautifyTimeCodes);
            this.groupBoxShowToolBarButtons.Controls.Add(this.labelTBNetflixQualityCheck);
            this.groupBoxShowToolBarButtons.Controls.Add(this.pictureBoxTBNetflixQualityCheck);
            this.groupBoxShowToolBarButtons.Controls.Add(this.checkBoxTBNetflixQualityCheck);
            this.groupBoxShowToolBarButtons.Controls.Add(this.labelTBRemoveTextForHi);
            this.groupBoxShowToolBarButtons.Controls.Add(this.pictureBoxRemoveTextForHi);
            this.groupBoxShowToolBarButtons.Controls.Add(this.checkBoxTBRemoveTextForHi);
            this.groupBoxShowToolBarButtons.Controls.Add(this.labelTBFixCommonErrors);
            this.groupBoxShowToolBarButtons.Controls.Add(this.pictureBoxTBFixCommonErrors);
            this.groupBoxShowToolBarButtons.Controls.Add(this.checkBoxTBFixCommonErrors);
            this.groupBoxShowToolBarButtons.Controls.Add(this.labelTBHelp);
            this.groupBoxShowToolBarButtons.Controls.Add(this.pictureBoxTBHelp);
            this.groupBoxShowToolBarButtons.Controls.Add(this.checkBoxTBHelp);
            this.groupBoxShowToolBarButtons.Controls.Add(this.labelTBSettings);
            this.groupBoxShowToolBarButtons.Controls.Add(this.pictureBoxTBSettings);
            this.groupBoxShowToolBarButtons.Controls.Add(this.checkBoxTBSettings);
            this.groupBoxShowToolBarButtons.Controls.Add(this.labelTBSpellCheck);
            this.groupBoxShowToolBarButtons.Controls.Add(this.pictureBoxTBSpellCheck);
            this.groupBoxShowToolBarButtons.Controls.Add(this.checkBoxTBSpellCheck);
            this.groupBoxShowToolBarButtons.Controls.Add(this.labelTBVisualSync);
            this.groupBoxShowToolBarButtons.Controls.Add(this.pictureBoxTBVisualSync);
            this.groupBoxShowToolBarButtons.Controls.Add(this.checkBoxTBVisualSync);
            this.groupBoxShowToolBarButtons.Controls.Add(this.labelTBReplace);
            this.groupBoxShowToolBarButtons.Controls.Add(this.pictureBoxTBReplace);
            this.groupBoxShowToolBarButtons.Controls.Add(this.checkBoxTBReplace);
            this.groupBoxShowToolBarButtons.Controls.Add(this.labelTBFind);
            this.groupBoxShowToolBarButtons.Controls.Add(this.pictureBoxTBFind);
            this.groupBoxShowToolBarButtons.Controls.Add(this.checkBoxTBFind);
            this.groupBoxShowToolBarButtons.Controls.Add(this.labelTBSaveAs);
            this.groupBoxShowToolBarButtons.Controls.Add(this.pictureBoxTBSaveAs);
            this.groupBoxShowToolBarButtons.Controls.Add(this.checkBoxTBSaveAs);
            this.groupBoxShowToolBarButtons.Controls.Add(this.labelTBSave);
            this.groupBoxShowToolBarButtons.Controls.Add(this.pictureBoxTBSave);
            this.groupBoxShowToolBarButtons.Controls.Add(this.checkBoxTBSave);
            this.groupBoxShowToolBarButtons.Controls.Add(this.labelTBOpen);
            this.groupBoxShowToolBarButtons.Controls.Add(this.pictureBoxTBOpen);
            this.groupBoxShowToolBarButtons.Controls.Add(this.checkBoxTBOpen);
            this.groupBoxShowToolBarButtons.Controls.Add(this.labelTBNew);
            this.groupBoxShowToolBarButtons.Controls.Add(this.pictureBoxFileNew);
            this.groupBoxShowToolBarButtons.Controls.Add(this.checkBoxToolbarNew);
            this.groupBoxShowToolBarButtons.Location = new System.Drawing.Point(0, 0);
            this.groupBoxShowToolBarButtons.Name = "groupBoxShowToolBarButtons";
            this.groupBoxShowToolBarButtons.Size = new System.Drawing.Size(851, 448);
            this.groupBoxShowToolBarButtons.TabIndex = 0;
            this.groupBoxShowToolBarButtons.TabStop = false;
            this.groupBoxShowToolBarButtons.Text = "Show toolbar buttons";
            // 
            // labelTBOpenVideo
            // 
            this.labelTBOpenVideo.AutoSize = true;
            this.labelTBOpenVideo.Location = new System.Drawing.Point(213, 22);
            this.labelTBOpenVideo.Name = "labelTBOpenVideo";
            this.labelTBOpenVideo.Size = new System.Drawing.Size(62, 13);
            this.labelTBOpenVideo.TabIndex = 63;
            this.labelTBOpenVideo.Text = "Open video";
            // 
            // pictureBoxTBOpenVideo
            // 
            this.pictureBoxTBOpenVideo.Location = new System.Drawing.Point(212, 41);
            this.pictureBoxTBOpenVideo.Name = "pictureBoxTBOpenVideo";
            this.pictureBoxTBOpenVideo.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxTBOpenVideo.TabIndex = 62;
            this.pictureBoxTBOpenVideo.TabStop = false;
            // 
            // checkBoxTBOpenVideo
            // 
            this.checkBoxTBOpenVideo.AutoSize = true;
            this.checkBoxTBOpenVideo.Location = new System.Drawing.Point(215, 81);
            this.checkBoxTBOpenVideo.Name = "checkBoxTBOpenVideo";
            this.checkBoxTBOpenVideo.Size = new System.Drawing.Size(55, 17);
            this.checkBoxTBOpenVideo.TabIndex = 61;
            this.checkBoxTBOpenVideo.Text = "Visible";
            this.checkBoxTBOpenVideo.UseVisualStyleBackColor = true;
            // 
            // pictureBoxWebVttStyle
            // 
            this.pictureBoxWebVttStyle.Location = new System.Drawing.Point(285, 400);
            this.pictureBoxWebVttStyle.Name = "pictureBoxWebVttStyle";
            this.pictureBoxWebVttStyle.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxWebVttStyle.TabIndex = 60;
            this.pictureBoxWebVttStyle.TabStop = false;
            // 
            // pictureBoxEbuProperties
            // 
            this.pictureBoxEbuProperties.Location = new System.Drawing.Point(361, 400);
            this.pictureBoxEbuProperties.Name = "pictureBoxEbuProperties";
            this.pictureBoxEbuProperties.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxEbuProperties.TabIndex = 59;
            this.pictureBoxEbuProperties.TabStop = false;
            // 
            // pictureBoxWebVttProperties
            // 
            this.pictureBoxWebVttProperties.Location = new System.Drawing.Point(324, 400);
            this.pictureBoxWebVttProperties.Name = "pictureBoxWebVttProperties";
            this.pictureBoxWebVttProperties.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxWebVttProperties.TabIndex = 58;
            this.pictureBoxWebVttProperties.TabStop = false;
            // 
            // pictureBoxIttProperties
            // 
            this.pictureBoxIttProperties.Location = new System.Drawing.Point(247, 400);
            this.pictureBoxIttProperties.Name = "pictureBoxIttProperties";
            this.pictureBoxIttProperties.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxIttProperties.TabIndex = 57;
            this.pictureBoxIttProperties.TabStop = false;
            // 
            // pictureBoxToggleVideo
            // 
            this.pictureBoxToggleVideo.Location = new System.Drawing.Point(209, 400);
            this.pictureBoxToggleVideo.Name = "pictureBoxToggleVideo";
            this.pictureBoxToggleVideo.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxToggleVideo.TabIndex = 56;
            this.pictureBoxToggleVideo.TabStop = false;
            // 
            // pictureBoxToggleWaveform
            // 
            this.pictureBoxToggleWaveform.Location = new System.Drawing.Point(171, 400);
            this.pictureBoxToggleWaveform.Name = "pictureBoxToggleWaveform";
            this.pictureBoxToggleWaveform.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxToggleWaveform.TabIndex = 55;
            this.pictureBoxToggleWaveform.TabStop = false;
            // 
            // pictureBoxAssaDraw
            // 
            this.pictureBoxAssaDraw.Location = new System.Drawing.Point(133, 400);
            this.pictureBoxAssaDraw.Name = "pictureBoxAssaDraw";
            this.pictureBoxAssaDraw.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxAssaDraw.TabIndex = 54;
            this.pictureBoxAssaDraw.TabStop = false;
            // 
            // pictureBoxAssAttachments
            // 
            this.pictureBoxAssAttachments.Location = new System.Drawing.Point(95, 400);
            this.pictureBoxAssAttachments.Name = "pictureBoxAssAttachments";
            this.pictureBoxAssAttachments.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxAssAttachments.TabIndex = 53;
            this.pictureBoxAssAttachments.TabStop = false;
            // 
            // pictureBoxAssProperties
            // 
            this.pictureBoxAssProperties.Location = new System.Drawing.Point(56, 400);
            this.pictureBoxAssProperties.Name = "pictureBoxAssProperties";
            this.pictureBoxAssProperties.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxAssProperties.TabIndex = 52;
            this.pictureBoxAssProperties.TabStop = false;
            // 
            // pictureBoxAssStyleManager
            // 
            this.pictureBoxAssStyleManager.Location = new System.Drawing.Point(19, 400);
            this.pictureBoxAssStyleManager.Name = "pictureBoxAssStyleManager";
            this.pictureBoxAssStyleManager.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxAssStyleManager.TabIndex = 51;
            this.pictureBoxAssStyleManager.TabStop = false;
            // 
            // labelTBToggleSourceView
            // 
            this.labelTBToggleSourceView.AutoSize = true;
            this.labelTBToggleSourceView.Location = new System.Drawing.Point(224, 257);
            this.labelTBToggleSourceView.Name = "labelTBToggleSourceView";
            this.labelTBToggleSourceView.Size = new System.Drawing.Size(99, 13);
            this.labelTBToggleSourceView.TabIndex = 48;
            this.labelTBToggleSourceView.Text = "Toggle source view";
            // 
            // pictureBoxTBToggleSourceView
            // 
            this.pictureBoxTBToggleSourceView.Location = new System.Drawing.Point(237, 274);
            this.pictureBoxTBToggleSourceView.Name = "pictureBoxTBToggleSourceView";
            this.pictureBoxTBToggleSourceView.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxTBToggleSourceView.TabIndex = 47;
            this.pictureBoxTBToggleSourceView.TabStop = false;
            // 
            // checkBoxTBToggleSourceView
            // 
            this.checkBoxTBToggleSourceView.AutoSize = true;
            this.checkBoxTBToggleSourceView.Location = new System.Drawing.Point(240, 312);
            this.checkBoxTBToggleSourceView.Name = "checkBoxTBToggleSourceView";
            this.checkBoxTBToggleSourceView.Size = new System.Drawing.Size(55, 17);
            this.checkBoxTBToggleSourceView.TabIndex = 46;
            this.checkBoxTBToggleSourceView.Text = "Visible";
            this.checkBoxTBToggleSourceView.UseVisualStyleBackColor = true;
            // 
            // labelTBBurnIn
            // 
            this.labelTBBurnIn.AutoSize = true;
            this.labelTBBurnIn.Location = new System.Drawing.Point(243, 142);
            this.labelTBBurnIn.Name = "labelTBBurnIn";
            this.labelTBBurnIn.Size = new System.Drawing.Size(40, 13);
            this.labelTBBurnIn.TabIndex = 45;
            this.labelTBBurnIn.Text = "Burn in";
            // 
            // pictureBoxTBBurnIn
            // 
            this.pictureBoxTBBurnIn.Location = new System.Drawing.Point(256, 159);
            this.pictureBoxTBBurnIn.Name = "pictureBoxTBBurnIn";
            this.pictureBoxTBBurnIn.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxTBBurnIn.TabIndex = 44;
            this.pictureBoxTBBurnIn.TabStop = false;
            // 
            // checkBoxTBBurnIn
            // 
            this.checkBoxTBBurnIn.AutoSize = true;
            this.checkBoxTBBurnIn.Location = new System.Drawing.Point(259, 198);
            this.checkBoxTBBurnIn.Name = "checkBoxTBBurnIn";
            this.checkBoxTBBurnIn.Size = new System.Drawing.Size(55, 17);
            this.checkBoxTBBurnIn.TabIndex = 20;
            this.checkBoxTBBurnIn.Text = "Visible";
            this.checkBoxTBBurnIn.UseVisualStyleBackColor = true;
            // 
            // labelTBBeautifyTimeCodes
            // 
            this.labelTBBeautifyTimeCodes.AutoSize = true;
            this.labelTBBeautifyTimeCodes.Location = new System.Drawing.Point(8, 257);
            this.labelTBBeautifyTimeCodes.Name = "labelTBBeautifyTimeCodes";
            this.labelTBBeautifyTimeCodes.Size = new System.Drawing.Size(101, 13);
            this.labelTBBeautifyTimeCodes.TabIndex = 45;
            this.labelTBBeautifyTimeCodes.Text = "Beautify time codes";
            // 
            // pictureBoxTBBeautifyTimeCodes
            // 
            this.pictureBoxTBBeautifyTimeCodes.Location = new System.Drawing.Point(19, 274);
            this.pictureBoxTBBeautifyTimeCodes.Name = "pictureBoxTBBeautifyTimeCodes";
            this.pictureBoxTBBeautifyTimeCodes.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxTBBeautifyTimeCodes.TabIndex = 44;
            this.pictureBoxTBBeautifyTimeCodes.TabStop = false;
            // 
            // checkBoxTBBeautifyTimeCodes
            // 
            this.checkBoxTBBeautifyTimeCodes.AutoSize = true;
            this.checkBoxTBBeautifyTimeCodes.Location = new System.Drawing.Point(22, 312);
            this.checkBoxTBBeautifyTimeCodes.Name = "checkBoxTBBeautifyTimeCodes";
            this.checkBoxTBBeautifyTimeCodes.Size = new System.Drawing.Size(55, 17);
            this.checkBoxTBBeautifyTimeCodes.TabIndex = 43;
            this.checkBoxTBBeautifyTimeCodes.Text = "Visible";
            this.checkBoxTBBeautifyTimeCodes.UseVisualStyleBackColor = true;
            // 
            // labelTBNetflixQualityCheck
            // 
            this.labelTBNetflixQualityCheck.AutoSize = true;
            this.labelTBNetflixQualityCheck.Location = new System.Drawing.Point(552, 143);
            this.labelTBNetflixQualityCheck.Name = "labelTBNetflixQualityCheck";
            this.labelTBNetflixQualityCheck.Size = new System.Drawing.Size(103, 13);
            this.labelTBNetflixQualityCheck.TabIndex = 42;
            this.labelTBNetflixQualityCheck.Text = "Netflix quality check";
            // 
            // pictureBoxTBNetflixQualityCheck
            // 
            this.pictureBoxTBNetflixQualityCheck.Location = new System.Drawing.Point(565, 160);
            this.pictureBoxTBNetflixQualityCheck.Name = "pictureBoxTBNetflixQualityCheck";
            this.pictureBoxTBNetflixQualityCheck.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxTBNetflixQualityCheck.TabIndex = 41;
            this.pictureBoxTBNetflixQualityCheck.TabStop = false;
            // 
            // checkBoxTBNetflixQualityCheck
            // 
            this.checkBoxTBNetflixQualityCheck.AutoSize = true;
            this.checkBoxTBNetflixQualityCheck.Location = new System.Drawing.Point(568, 198);
            this.checkBoxTBNetflixQualityCheck.Name = "checkBoxTBNetflixQualityCheck";
            this.checkBoxTBNetflixQualityCheck.Size = new System.Drawing.Size(55, 17);
            this.checkBoxTBNetflixQualityCheck.TabIndex = 40;
            this.checkBoxTBNetflixQualityCheck.Text = "Visible";
            this.checkBoxTBNetflixQualityCheck.UseVisualStyleBackColor = true;
            // 
            // labelTBRemoveTextForHi
            // 
            this.labelTBRemoveTextForHi.AutoSize = true;
            this.labelTBRemoveTextForHi.Location = new System.Drawing.Point(9, 143);
            this.labelTBRemoveTextForHi.Name = "labelTBRemoveTextForHi";
            this.labelTBRemoveTextForHi.Size = new System.Drawing.Size(100, 13);
            this.labelTBRemoveTextForHi.TabIndex = 39;
            this.labelTBRemoveTextForHi.Text = "Remove text for HI";
            // 
            // pictureBoxRemoveTextForHi
            // 
            this.pictureBoxRemoveTextForHi.Location = new System.Drawing.Point(22, 161);
            this.pictureBoxRemoveTextForHi.Name = "pictureBoxRemoveTextForHi";
            this.pictureBoxRemoveTextForHi.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxRemoveTextForHi.TabIndex = 38;
            this.pictureBoxRemoveTextForHi.TabStop = false;
            // 
            // checkBoxTBRemoveTextForHi
            // 
            this.checkBoxTBRemoveTextForHi.AutoSize = true;
            this.checkBoxTBRemoveTextForHi.Location = new System.Drawing.Point(25, 198);
            this.checkBoxTBRemoveTextForHi.Name = "checkBoxTBRemoveTextForHi";
            this.checkBoxTBRemoveTextForHi.Size = new System.Drawing.Size(55, 17);
            this.checkBoxTBRemoveTextForHi.TabIndex = 18;
            this.checkBoxTBRemoveTextForHi.Text = "Visible";
            this.checkBoxTBRemoveTextForHi.UseVisualStyleBackColor = true;
            // 
            // labelTBFixCommonErrors
            // 
            this.labelTBFixCommonErrors.AutoSize = true;
            this.labelTBFixCommonErrors.Location = new System.Drawing.Point(675, 22);
            this.labelTBFixCommonErrors.Name = "labelTBFixCommonErrors";
            this.labelTBFixCommonErrors.Size = new System.Drawing.Size(95, 13);
            this.labelTBFixCommonErrors.TabIndex = 36;
            this.labelTBFixCommonErrors.Text = "Fix common errors";
            // 
            // pictureBoxTBFixCommonErrors
            // 
            this.pictureBoxTBFixCommonErrors.Location = new System.Drawing.Point(688, 41);
            this.pictureBoxTBFixCommonErrors.Name = "pictureBoxTBFixCommonErrors";
            this.pictureBoxTBFixCommonErrors.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxTBFixCommonErrors.TabIndex = 35;
            this.pictureBoxTBFixCommonErrors.TabStop = false;
            // 
            // checkBoxTBFixCommonErrors
            // 
            this.checkBoxTBFixCommonErrors.AutoSize = true;
            this.checkBoxTBFixCommonErrors.Location = new System.Drawing.Point(691, 81);
            this.checkBoxTBFixCommonErrors.Name = "checkBoxTBFixCommonErrors";
            this.checkBoxTBFixCommonErrors.Size = new System.Drawing.Size(55, 17);
            this.checkBoxTBFixCommonErrors.TabIndex = 17;
            this.checkBoxTBFixCommonErrors.Text = "Visible";
            this.checkBoxTBFixCommonErrors.UseVisualStyleBackColor = true;
            // 
            // labelTBHelp
            // 
            this.labelTBHelp.AutoSize = true;
            this.labelTBHelp.Location = new System.Drawing.Point(143, 257);
            this.labelTBHelp.Name = "labelTBHelp";
            this.labelTBHelp.Size = new System.Drawing.Size(28, 13);
            this.labelTBHelp.TabIndex = 33;
            this.labelTBHelp.Text = "Help";
            // 
            // pictureBoxTBHelp
            // 
            this.pictureBoxTBHelp.Location = new System.Drawing.Point(142, 274);
            this.pictureBoxTBHelp.Name = "pictureBoxTBHelp";
            this.pictureBoxTBHelp.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxTBHelp.TabIndex = 32;
            this.pictureBoxTBHelp.TabStop = false;
            // 
            // checkBoxTBHelp
            // 
            this.checkBoxTBHelp.AutoSize = true;
            this.checkBoxTBHelp.Location = new System.Drawing.Point(145, 312);
            this.checkBoxTBHelp.Name = "checkBoxTBHelp";
            this.checkBoxTBHelp.Size = new System.Drawing.Size(55, 17);
            this.checkBoxTBHelp.TabIndex = 23;
            this.checkBoxTBHelp.Text = "Visible";
            this.checkBoxTBHelp.UseVisualStyleBackColor = true;
            // 
            // labelTBSettings
            // 
            this.labelTBSettings.AutoSize = true;
            this.labelTBSettings.Location = new System.Drawing.Point(456, 143);
            this.labelTBSettings.Name = "labelTBSettings";
            this.labelTBSettings.Size = new System.Drawing.Size(46, 13);
            this.labelTBSettings.TabIndex = 30;
            this.labelTBSettings.Text = "Settings";
            // 
            // pictureBoxTBSettings
            // 
            this.pictureBoxTBSettings.Location = new System.Drawing.Point(459, 160);
            this.pictureBoxTBSettings.Name = "pictureBoxTBSettings";
            this.pictureBoxTBSettings.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxTBSettings.TabIndex = 29;
            this.pictureBoxTBSettings.TabStop = false;
            // 
            // checkBoxTBSettings
            // 
            this.checkBoxTBSettings.AutoSize = true;
            this.checkBoxTBSettings.Location = new System.Drawing.Point(462, 198);
            this.checkBoxTBSettings.Name = "checkBoxTBSettings";
            this.checkBoxTBSettings.Size = new System.Drawing.Size(55, 17);
            this.checkBoxTBSettings.TabIndex = 22;
            this.checkBoxTBSettings.Text = "Visible";
            this.checkBoxTBSettings.UseVisualStyleBackColor = true;
            // 
            // labelTBSpellCheck
            // 
            this.labelTBSpellCheck.AutoSize = true;
            this.labelTBSpellCheck.Location = new System.Drawing.Point(357, 143);
            this.labelTBSpellCheck.Name = "labelTBSpellCheck";
            this.labelTBSpellCheck.Size = new System.Drawing.Size(59, 13);
            this.labelTBSpellCheck.TabIndex = 27;
            this.labelTBSpellCheck.Text = "Spell check";
            // 
            // pictureBoxTBSpellCheck
            // 
            this.pictureBoxTBSpellCheck.Location = new System.Drawing.Point(361, 160);
            this.pictureBoxTBSpellCheck.Name = "pictureBoxTBSpellCheck";
            this.pictureBoxTBSpellCheck.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxTBSpellCheck.TabIndex = 26;
            this.pictureBoxTBSpellCheck.TabStop = false;
            // 
            // checkBoxTBSpellCheck
            // 
            this.checkBoxTBSpellCheck.AutoSize = true;
            this.checkBoxTBSpellCheck.Location = new System.Drawing.Point(362, 198);
            this.checkBoxTBSpellCheck.Name = "checkBoxTBSpellCheck";
            this.checkBoxTBSpellCheck.Size = new System.Drawing.Size(55, 17);
            this.checkBoxTBSpellCheck.TabIndex = 21;
            this.checkBoxTBSpellCheck.Text = "Visible";
            this.checkBoxTBSpellCheck.UseVisualStyleBackColor = true;
            // 
            // labelTBVisualSync
            // 
            this.labelTBVisualSync.AutoSize = true;
            this.labelTBVisualSync.Location = new System.Drawing.Point(132, 143);
            this.labelTBVisualSync.Name = "labelTBVisualSync";
            this.labelTBVisualSync.Size = new System.Drawing.Size(59, 13);
            this.labelTBVisualSync.TabIndex = 21;
            this.labelTBVisualSync.Text = "Visual sync";
            // 
            // pictureBoxTBVisualSync
            // 
            this.pictureBoxTBVisualSync.Location = new System.Drawing.Point(145, 161);
            this.pictureBoxTBVisualSync.Name = "pictureBoxTBVisualSync";
            this.pictureBoxTBVisualSync.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxTBVisualSync.TabIndex = 20;
            this.pictureBoxTBVisualSync.TabStop = false;
            // 
            // checkBoxTBVisualSync
            // 
            this.checkBoxTBVisualSync.AutoSize = true;
            this.checkBoxTBVisualSync.Location = new System.Drawing.Point(148, 198);
            this.checkBoxTBVisualSync.Name = "checkBoxTBVisualSync";
            this.checkBoxTBVisualSync.Size = new System.Drawing.Size(55, 17);
            this.checkBoxTBVisualSync.TabIndex = 19;
            this.checkBoxTBVisualSync.Text = "Visible";
            this.checkBoxTBVisualSync.UseVisualStyleBackColor = true;
            // 
            // labelTBReplace
            // 
            this.labelTBReplace.AutoSize = true;
            this.labelTBReplace.Location = new System.Drawing.Point(599, 22);
            this.labelTBReplace.Name = "labelTBReplace";
            this.labelTBReplace.Size = new System.Drawing.Size(45, 13);
            this.labelTBReplace.TabIndex = 18;
            this.labelTBReplace.Text = "Replace";
            // 
            // pictureBoxTBReplace
            // 
            this.pictureBoxTBReplace.Location = new System.Drawing.Point(604, 41);
            this.pictureBoxTBReplace.Name = "pictureBoxTBReplace";
            this.pictureBoxTBReplace.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxTBReplace.TabIndex = 17;
            this.pictureBoxTBReplace.TabStop = false;
            // 
            // checkBoxTBReplace
            // 
            this.checkBoxTBReplace.AutoSize = true;
            this.checkBoxTBReplace.Location = new System.Drawing.Point(607, 81);
            this.checkBoxTBReplace.Name = "checkBoxTBReplace";
            this.checkBoxTBReplace.Size = new System.Drawing.Size(55, 17);
            this.checkBoxTBReplace.TabIndex = 16;
            this.checkBoxTBReplace.Text = "Visible";
            this.checkBoxTBReplace.UseVisualStyleBackColor = true;
            // 
            // labelTBFind
            // 
            this.labelTBFind.AutoSize = true;
            this.labelTBFind.Location = new System.Drawing.Point(503, 22);
            this.labelTBFind.Name = "labelTBFind";
            this.labelTBFind.Size = new System.Drawing.Size(27, 13);
            this.labelTBFind.TabIndex = 15;
            this.labelTBFind.Text = "Find";
            // 
            // pictureBoxTBFind
            // 
            this.pictureBoxTBFind.Location = new System.Drawing.Point(501, 41);
            this.pictureBoxTBFind.Name = "pictureBoxTBFind";
            this.pictureBoxTBFind.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxTBFind.TabIndex = 14;
            this.pictureBoxTBFind.TabStop = false;
            // 
            // checkBoxTBFind
            // 
            this.checkBoxTBFind.AutoSize = true;
            this.checkBoxTBFind.Location = new System.Drawing.Point(504, 81);
            this.checkBoxTBFind.Name = "checkBoxTBFind";
            this.checkBoxTBFind.Size = new System.Drawing.Size(55, 17);
            this.checkBoxTBFind.TabIndex = 13;
            this.checkBoxTBFind.Text = "Visible";
            this.checkBoxTBFind.UseVisualStyleBackColor = true;
            // 
            // labelTBSaveAs
            // 
            this.labelTBSaveAs.AutoSize = true;
            this.labelTBSaveAs.Location = new System.Drawing.Point(394, 22);
            this.labelTBSaveAs.Name = "labelTBSaveAs";
            this.labelTBSaveAs.Size = new System.Drawing.Size(45, 13);
            this.labelTBSaveAs.TabIndex = 12;
            this.labelTBSaveAs.Text = "Save as";
            // 
            // pictureBoxTBSaveAs
            // 
            this.pictureBoxTBSaveAs.Location = new System.Drawing.Point(400, 41);
            this.pictureBoxTBSaveAs.Name = "pictureBoxTBSaveAs";
            this.pictureBoxTBSaveAs.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxTBSaveAs.TabIndex = 11;
            this.pictureBoxTBSaveAs.TabStop = false;
            // 
            // checkBoxTBSaveAs
            // 
            this.checkBoxTBSaveAs.AutoSize = true;
            this.checkBoxTBSaveAs.Location = new System.Drawing.Point(403, 81);
            this.checkBoxTBSaveAs.Name = "checkBoxTBSaveAs";
            this.checkBoxTBSaveAs.Size = new System.Drawing.Size(55, 17);
            this.checkBoxTBSaveAs.TabIndex = 10;
            this.checkBoxTBSaveAs.Text = "Visible";
            this.checkBoxTBSaveAs.UseVisualStyleBackColor = true;
            // 
            // labelTBSave
            // 
            this.labelTBSave.AutoSize = true;
            this.labelTBSave.Location = new System.Drawing.Point(303, 22);
            this.labelTBSave.Name = "labelTBSave";
            this.labelTBSave.Size = new System.Drawing.Size(31, 13);
            this.labelTBSave.TabIndex = 9;
            this.labelTBSave.Text = "Save";
            // 
            // pictureBoxTBSave
            // 
            this.pictureBoxTBSave.Location = new System.Drawing.Point(302, 41);
            this.pictureBoxTBSave.Name = "pictureBoxTBSave";
            this.pictureBoxTBSave.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxTBSave.TabIndex = 8;
            this.pictureBoxTBSave.TabStop = false;
            // 
            // checkBoxTBSave
            // 
            this.checkBoxTBSave.AutoSize = true;
            this.checkBoxTBSave.Location = new System.Drawing.Point(305, 81);
            this.checkBoxTBSave.Name = "checkBoxTBSave";
            this.checkBoxTBSave.Size = new System.Drawing.Size(55, 17);
            this.checkBoxTBSave.TabIndex = 7;
            this.checkBoxTBSave.Text = "Visible";
            this.checkBoxTBSave.UseVisualStyleBackColor = true;
            // 
            // labelTBOpen
            // 
            this.labelTBOpen.AutoSize = true;
            this.labelTBOpen.Location = new System.Drawing.Point(124, 22);
            this.labelTBOpen.Name = "labelTBOpen";
            this.labelTBOpen.Size = new System.Drawing.Size(33, 13);
            this.labelTBOpen.TabIndex = 6;
            this.labelTBOpen.Text = "Open";
            // 
            // pictureBoxTBOpen
            // 
            this.pictureBoxTBOpen.Location = new System.Drawing.Point(123, 41);
            this.pictureBoxTBOpen.Name = "pictureBoxTBOpen";
            this.pictureBoxTBOpen.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxTBOpen.TabIndex = 5;
            this.pictureBoxTBOpen.TabStop = false;
            // 
            // checkBoxTBOpen
            // 
            this.checkBoxTBOpen.AutoSize = true;
            this.checkBoxTBOpen.Location = new System.Drawing.Point(126, 81);
            this.checkBoxTBOpen.Name = "checkBoxTBOpen";
            this.checkBoxTBOpen.Size = new System.Drawing.Size(55, 17);
            this.checkBoxTBOpen.TabIndex = 4;
            this.checkBoxTBOpen.Text = "Visible";
            this.checkBoxTBOpen.UseVisualStyleBackColor = true;
            // 
            // labelTBNew
            // 
            this.labelTBNew.AutoSize = true;
            this.labelTBNew.Location = new System.Drawing.Point(24, 22);
            this.labelTBNew.Name = "labelTBNew";
            this.labelTBNew.Size = new System.Drawing.Size(28, 13);
            this.labelTBNew.TabIndex = 3;
            this.labelTBNew.Text = "New";
            // 
            // pictureBoxFileNew
            // 
            this.pictureBoxFileNew.Location = new System.Drawing.Point(22, 41);
            this.pictureBoxFileNew.Name = "pictureBoxFileNew";
            this.pictureBoxFileNew.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxFileNew.TabIndex = 2;
            this.pictureBoxFileNew.TabStop = false;
            // 
            // checkBoxToolbarNew
            // 
            this.checkBoxToolbarNew.AutoSize = true;
            this.checkBoxToolbarNew.Location = new System.Drawing.Point(25, 81);
            this.checkBoxToolbarNew.Name = "checkBoxToolbarNew";
            this.checkBoxToolbarNew.Size = new System.Drawing.Size(55, 17);
            this.checkBoxToolbarNew.TabIndex = 1;
            this.checkBoxToolbarNew.Text = "Visible";
            this.checkBoxToolbarNew.UseVisualStyleBackColor = true;
            // 
            // panelFont
            // 
            this.panelFont.Controls.Add(this.groupBoxAppearance);
            this.panelFont.Location = new System.Drawing.Point(230, 6);
            this.panelFont.Name = "panelFont";
            this.panelFont.Size = new System.Drawing.Size(864, 521);
            this.panelFont.TabIndex = 10;
            this.panelFont.Text = "Font";
            // 
            // groupBoxAppearance
            // 
            this.groupBoxAppearance.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxAppearance.Controls.Add(this.groupBoxGraphicsButtons);
            this.groupBoxAppearance.Controls.Add(this.groupBoxFontInUI);
            this.groupBoxAppearance.Controls.Add(this.groupBoxDarkTheme);
            this.groupBoxAppearance.Controls.Add(this.labelFontNote);
            this.groupBoxAppearance.Location = new System.Drawing.Point(0, 0);
            this.groupBoxAppearance.Name = "groupBoxAppearance";
            this.groupBoxAppearance.Size = new System.Drawing.Size(852, 521);
            this.groupBoxAppearance.TabIndex = 0;
            this.groupBoxAppearance.TabStop = false;
            this.groupBoxAppearance.Text = "Appearance";
            // 
            // groupBoxGraphicsButtons
            // 
            this.groupBoxGraphicsButtons.Controls.Add(this.pictureBoxPreview3);
            this.groupBoxGraphicsButtons.Controls.Add(this.pictureBoxPreview2);
            this.groupBoxGraphicsButtons.Controls.Add(this.pictureBoxPreview1);
            this.groupBoxGraphicsButtons.Controls.Add(this.labelToolbarIconTheme);
            this.groupBoxGraphicsButtons.Controls.Add(this.comboBoxToolbarIconTheme);
            this.groupBoxGraphicsButtons.Location = new System.Drawing.Point(383, 307);
            this.groupBoxGraphicsButtons.Name = "groupBoxGraphicsButtons";
            this.groupBoxGraphicsButtons.Size = new System.Drawing.Size(461, 114);
            this.groupBoxGraphicsButtons.TabIndex = 41;
            this.groupBoxGraphicsButtons.TabStop = false;
            this.groupBoxGraphicsButtons.Text = "Graphics buttons";
            // 
            // pictureBoxPreview3
            // 
            this.pictureBoxPreview3.Location = new System.Drawing.Point(306, 31);
            this.pictureBoxPreview3.Name = "pictureBoxPreview3";
            this.pictureBoxPreview3.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxPreview3.TabIndex = 56;
            this.pictureBoxPreview3.TabStop = false;
            // 
            // pictureBoxPreview2
            // 
            this.pictureBoxPreview2.Location = new System.Drawing.Point(267, 31);
            this.pictureBoxPreview2.Name = "pictureBoxPreview2";
            this.pictureBoxPreview2.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxPreview2.TabIndex = 55;
            this.pictureBoxPreview2.TabStop = false;
            // 
            // pictureBoxPreview1
            // 
            this.pictureBoxPreview1.Location = new System.Drawing.Point(230, 31);
            this.pictureBoxPreview1.Name = "pictureBoxPreview1";
            this.pictureBoxPreview1.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxPreview1.TabIndex = 54;
            this.pictureBoxPreview1.TabStop = false;
            // 
            // labelToolbarIconTheme
            // 
            this.labelToolbarIconTheme.AutoSize = true;
            this.labelToolbarIconTheme.Location = new System.Drawing.Point(13, 23);
            this.labelToolbarIconTheme.Name = "labelToolbarIconTheme";
            this.labelToolbarIconTheme.Size = new System.Drawing.Size(39, 13);
            this.labelToolbarIconTheme.TabIndex = 52;
            this.labelToolbarIconTheme.Text = "Theme";
            // 
            // comboBoxToolbarIconTheme
            // 
            this.comboBoxToolbarIconTheme.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxToolbarIconTheme.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxToolbarIconTheme.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxToolbarIconTheme.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxToolbarIconTheme.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxToolbarIconTheme.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxToolbarIconTheme.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxToolbarIconTheme.DropDownHeight = 400;
            this.comboBoxToolbarIconTheme.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxToolbarIconTheme.DropDownWidth = 202;
            this.comboBoxToolbarIconTheme.FormattingEnabled = true;
            this.comboBoxToolbarIconTheme.Location = new System.Drawing.Point(13, 38);
            this.comboBoxToolbarIconTheme.MaxLength = 32767;
            this.comboBoxToolbarIconTheme.Name = "comboBoxToolbarIconTheme";
            this.comboBoxToolbarIconTheme.SelectedIndex = -1;
            this.comboBoxToolbarIconTheme.SelectedItem = null;
            this.comboBoxToolbarIconTheme.SelectedText = "";
            this.comboBoxToolbarIconTheme.Size = new System.Drawing.Size(202, 21);
            this.comboBoxToolbarIconTheme.TabIndex = 51;
            this.comboBoxToolbarIconTheme.UsePopupWindow = false;
            // 
            // groupBoxFontInUI
            // 
            this.groupBoxFontInUI.Controls.Add(this.groupBoxFontGeneral);
            this.groupBoxFontInUI.Controls.Add(this.groupBoxFontListViews);
            this.groupBoxFontInUI.Controls.Add(this.groupBoxFontTextBox);
            this.groupBoxFontInUI.Location = new System.Drawing.Point(6, 20);
            this.groupBoxFontInUI.Name = "groupBoxFontInUI";
            this.groupBoxFontInUI.Size = new System.Drawing.Size(838, 281);
            this.groupBoxFontInUI.TabIndex = 5;
            this.groupBoxFontInUI.TabStop = false;
            this.groupBoxFontInUI.Text = "Font in UI";
            // 
            // groupBoxFontGeneral
            // 
            this.groupBoxFontGeneral.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxFontGeneral.Controls.Add(this.comboBoxSubtitleFont);
            this.groupBoxFontGeneral.Controls.Add(this.labelSubtitleFont);
            this.groupBoxFontGeneral.Controls.Add(this.panelSubtitleFontColor);
            this.groupBoxFontGeneral.Controls.Add(this.labelSubtitleFontColor);
            this.groupBoxFontGeneral.Controls.Add(this.panelSubtitleBackgroundColor);
            this.groupBoxFontGeneral.Controls.Add(this.labelSubtitleFontBackgroundColor);
            this.groupBoxFontGeneral.Location = new System.Drawing.Point(6, 20);
            this.groupBoxFontGeneral.Name = "groupBoxFontGeneral";
            this.groupBoxFontGeneral.Size = new System.Drawing.Size(408, 97);
            this.groupBoxFontGeneral.TabIndex = 10;
            this.groupBoxFontGeneral.TabStop = false;
            this.groupBoxFontGeneral.Text = "General";
            // 
            // comboBoxSubtitleFont
            // 
            this.comboBoxSubtitleFont.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxSubtitleFont.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxSubtitleFont.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxSubtitleFont.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxSubtitleFont.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxSubtitleFont.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxSubtitleFont.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxSubtitleFont.DropDownHeight = 400;
            this.comboBoxSubtitleFont.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSubtitleFont.DropDownWidth = 188;
            this.comboBoxSubtitleFont.FormattingEnabled = true;
            this.comboBoxSubtitleFont.Location = new System.Drawing.Point(210, 20);
            this.comboBoxSubtitleFont.MaxLength = 32767;
            this.comboBoxSubtitleFont.Name = "comboBoxSubtitleFont";
            this.comboBoxSubtitleFont.SelectedIndex = -1;
            this.comboBoxSubtitleFont.SelectedItem = null;
            this.comboBoxSubtitleFont.SelectedText = "";
            this.comboBoxSubtitleFont.Size = new System.Drawing.Size(188, 21);
            this.comboBoxSubtitleFont.TabIndex = 29;
            this.comboBoxSubtitleFont.UsePopupWindow = false;
            // 
            // labelSubtitleFont
            // 
            this.labelSubtitleFont.AutoSize = true;
            this.labelSubtitleFont.Location = new System.Drawing.Point(13, 26);
            this.labelSubtitleFont.Name = "labelSubtitleFont";
            this.labelSubtitleFont.Size = new System.Drawing.Size(66, 13);
            this.labelSubtitleFont.TabIndex = 28;
            this.labelSubtitleFont.Text = "Subtitle font";
            // 
            // panelSubtitleFontColor
            // 
            this.panelSubtitleFontColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelSubtitleFontColor.Location = new System.Drawing.Point(210, 51);
            this.panelSubtitleFontColor.Name = "panelSubtitleFontColor";
            this.panelSubtitleFontColor.Size = new System.Drawing.Size(46, 15);
            this.panelSubtitleFontColor.TabIndex = 35;
            this.panelSubtitleFontColor.Click += new System.EventHandler(this.panelSubtitleFontColor_Click);
            // 
            // labelSubtitleFontColor
            // 
            this.labelSubtitleFontColor.AutoSize = true;
            this.labelSubtitleFontColor.Location = new System.Drawing.Point(13, 50);
            this.labelSubtitleFontColor.Name = "labelSubtitleFontColor";
            this.labelSubtitleFontColor.Size = new System.Drawing.Size(92, 13);
            this.labelSubtitleFontColor.TabIndex = 34;
            this.labelSubtitleFontColor.Text = "Subtitle font color";
            // 
            // panelSubtitleBackgroundColor
            // 
            this.panelSubtitleBackgroundColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelSubtitleBackgroundColor.Location = new System.Drawing.Point(210, 71);
            this.panelSubtitleBackgroundColor.Name = "panelSubtitleBackgroundColor";
            this.panelSubtitleBackgroundColor.Size = new System.Drawing.Size(46, 15);
            this.panelSubtitleBackgroundColor.TabIndex = 37;
            this.panelSubtitleBackgroundColor.Click += new System.EventHandler(this.panelSubtitleBackgroundColor_Click);
            // 
            // labelSubtitleFontBackgroundColor
            // 
            this.labelSubtitleFontBackgroundColor.AutoSize = true;
            this.labelSubtitleFontBackgroundColor.Location = new System.Drawing.Point(13, 70);
            this.labelSubtitleFontBackgroundColor.Name = "labelSubtitleFontBackgroundColor";
            this.labelSubtitleFontBackgroundColor.Size = new System.Drawing.Size(151, 13);
            this.labelSubtitleFontBackgroundColor.TabIndex = 36;
            this.labelSubtitleFontBackgroundColor.Text = "Subtitle font background color";
            // 
            // groupBoxFontListViews
            // 
            this.groupBoxFontListViews.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxFontListViews.Controls.Add(this.labelSubtitleListViewFontSize);
            this.groupBoxFontListViews.Controls.Add(this.comboBoxSubtitleListViewFontSize);
            this.groupBoxFontListViews.Controls.Add(this.checkBoxSubtitleListViewFontBold);
            this.groupBoxFontListViews.Location = new System.Drawing.Point(427, 20);
            this.groupBoxFontListViews.Name = "groupBoxFontListViews";
            this.groupBoxFontListViews.Size = new System.Drawing.Size(405, 97);
            this.groupBoxFontListViews.TabIndex = 20;
            this.groupBoxFontListViews.TabStop = false;
            this.groupBoxFontListViews.Text = "List view";
            // 
            // labelSubtitleListViewFontSize
            // 
            this.labelSubtitleListViewFontSize.AutoSize = true;
            this.labelSubtitleListViewFontSize.Location = new System.Drawing.Point(13, 17);
            this.labelSubtitleListViewFontSize.Name = "labelSubtitleListViewFontSize";
            this.labelSubtitleListViewFontSize.Size = new System.Drawing.Size(87, 13);
            this.labelSubtitleListViewFontSize.TabIndex = 33;
            this.labelSubtitleListViewFontSize.Text = "Subtitle font size";
            // 
            // comboBoxSubtitleListViewFontSize
            // 
            this.comboBoxSubtitleListViewFontSize.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxSubtitleListViewFontSize.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxSubtitleListViewFontSize.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxSubtitleListViewFontSize.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxSubtitleListViewFontSize.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxSubtitleListViewFontSize.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxSubtitleListViewFontSize.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxSubtitleListViewFontSize.DropDownHeight = 400;
            this.comboBoxSubtitleListViewFontSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSubtitleListViewFontSize.DropDownWidth = 73;
            this.comboBoxSubtitleListViewFontSize.FormattingEnabled = true;
            this.comboBoxSubtitleListViewFontSize.Items.AddRange(new string[] {
            "7",
            "8",
            "9",
            "10",
            "11",
            "12",
            "13",
            "14",
            "15",
            "16",
            "17",
            "18",
            "19",
            "20",
            "21",
            "22",
            "23",
            "24",
            "25",
            "26",
            "27",
            "28",
            "29",
            "30",
            "31",
            "32",
            "33",
            "34",
            "35",
            "36",
            "37",
            "38",
            "39",
            "40",
            "50",
            "60"});
            this.comboBoxSubtitleListViewFontSize.Location = new System.Drawing.Point(16, 38);
            this.comboBoxSubtitleListViewFontSize.MaxLength = 32767;
            this.comboBoxSubtitleListViewFontSize.Name = "comboBoxSubtitleListViewFontSize";
            this.comboBoxSubtitleListViewFontSize.SelectedIndex = -1;
            this.comboBoxSubtitleListViewFontSize.SelectedItem = null;
            this.comboBoxSubtitleListViewFontSize.SelectedText = "";
            this.comboBoxSubtitleListViewFontSize.Size = new System.Drawing.Size(73, 21);
            this.comboBoxSubtitleListViewFontSize.TabIndex = 34;
            this.comboBoxSubtitleListViewFontSize.UsePopupWindow = false;
            // 
            // checkBoxSubtitleListViewFontBold
            // 
            this.checkBoxSubtitleListViewFontBold.AutoSize = true;
            this.checkBoxSubtitleListViewFontBold.Location = new System.Drawing.Point(95, 44);
            this.checkBoxSubtitleListViewFontBold.Name = "checkBoxSubtitleListViewFontBold";
            this.checkBoxSubtitleListViewFontBold.Size = new System.Drawing.Size(46, 17);
            this.checkBoxSubtitleListViewFontBold.TabIndex = 35;
            this.checkBoxSubtitleListViewFontBold.Text = "Bold";
            this.checkBoxSubtitleListViewFontBold.UseVisualStyleBackColor = true;
            // 
            // groupBoxFontTextBox
            // 
            this.groupBoxFontTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxFontTextBox.Controls.Add(this.checkBoxLiveSpellCheck);
            this.groupBoxFontTextBox.Controls.Add(this.panelTextBoxAssColor);
            this.groupBoxFontTextBox.Controls.Add(this.buttonTextBoxAssColor);
            this.groupBoxFontTextBox.Controls.Add(this.panelTextBoxHtmlColor);
            this.groupBoxFontTextBox.Controls.Add(this.buttonTextBoxHtmlColor);
            this.groupBoxFontTextBox.Controls.Add(this.checkBoxSubtitleTextBoxSyntaxColor);
            this.groupBoxFontTextBox.Controls.Add(this.labelSubtitleFontSize);
            this.groupBoxFontTextBox.Controls.Add(this.comboBoxSubtitleFontSize);
            this.groupBoxFontTextBox.Controls.Add(this.checkBoxSubtitleFontBold);
            this.groupBoxFontTextBox.Controls.Add(this.checkBoxSubtitleCenter);
            this.groupBoxFontTextBox.Location = new System.Drawing.Point(7, 121);
            this.groupBoxFontTextBox.Name = "groupBoxFontTextBox";
            this.groupBoxFontTextBox.Size = new System.Drawing.Size(825, 154);
            this.groupBoxFontTextBox.TabIndex = 30;
            this.groupBoxFontTextBox.TabStop = false;
            this.groupBoxFontTextBox.Text = "Text box";
            // 
            // checkBoxLiveSpellCheck
            // 
            this.checkBoxLiveSpellCheck.AutoSize = true;
            this.checkBoxLiveSpellCheck.Location = new System.Drawing.Point(181, 57);
            this.checkBoxLiveSpellCheck.Name = "checkBoxLiveSpellCheck";
            this.checkBoxLiveSpellCheck.Size = new System.Drawing.Size(99, 17);
            this.checkBoxLiveSpellCheck.TabIndex = 35;
            this.checkBoxLiveSpellCheck.Text = "Live spell check";
            this.checkBoxLiveSpellCheck.UseVisualStyleBackColor = true;
            // 
            // panelTextBoxAssColor
            // 
            this.panelTextBoxAssColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelTextBoxAssColor.Location = new System.Drawing.Point(305, 114);
            this.panelTextBoxAssColor.Name = "panelTextBoxAssColor";
            this.panelTextBoxAssColor.Size = new System.Drawing.Size(21, 20);
            this.panelTextBoxAssColor.TabIndex = 38;
            this.panelTextBoxAssColor.MouseClick += new System.Windows.Forms.MouseEventHandler(this.panelTextBoxAssColor_MouseClick);
            // 
            // buttonTextBoxAssColor
            // 
            this.buttonTextBoxAssColor.Location = new System.Drawing.Point(181, 113);
            this.buttonTextBoxAssColor.Name = "buttonTextBoxAssColor";
            this.buttonTextBoxAssColor.Size = new System.Drawing.Size(112, 23);
            this.buttonTextBoxAssColor.TabIndex = 37;
            this.buttonTextBoxAssColor.Text = "ASSA color";
            this.buttonTextBoxAssColor.UseVisualStyleBackColor = true;
            this.buttonTextBoxAssColor.Click += new System.EventHandler(this.buttonTextBoxAssColor_Click);
            // 
            // panelTextBoxHtmlColor
            // 
            this.panelTextBoxHtmlColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelTextBoxHtmlColor.Location = new System.Drawing.Point(305, 85);
            this.panelTextBoxHtmlColor.Name = "panelTextBoxHtmlColor";
            this.panelTextBoxHtmlColor.Size = new System.Drawing.Size(21, 20);
            this.panelTextBoxHtmlColor.TabIndex = 37;
            this.panelTextBoxHtmlColor.MouseClick += new System.Windows.Forms.MouseEventHandler(this.panelTextBoxHtmlColor_MouseClick);
            // 
            // buttonTextBoxHtmlColor
            // 
            this.buttonTextBoxHtmlColor.Location = new System.Drawing.Point(181, 84);
            this.buttonTextBoxHtmlColor.Name = "buttonTextBoxHtmlColor";
            this.buttonTextBoxHtmlColor.Size = new System.Drawing.Size(112, 23);
            this.buttonTextBoxHtmlColor.TabIndex = 36;
            this.buttonTextBoxHtmlColor.Text = "Html color";
            this.buttonTextBoxHtmlColor.UseVisualStyleBackColor = true;
            this.buttonTextBoxHtmlColor.Click += new System.EventHandler(this.buttonTextBoxHtmlColor_Click);
            // 
            // checkBoxSubtitleTextBoxSyntaxColor
            // 
            this.checkBoxSubtitleTextBoxSyntaxColor.AutoSize = true;
            this.checkBoxSubtitleTextBoxSyntaxColor.Location = new System.Drawing.Point(162, 35);
            this.checkBoxSubtitleTextBoxSyntaxColor.Name = "checkBoxSubtitleTextBoxSyntaxColor";
            this.checkBoxSubtitleTextBoxSyntaxColor.Size = new System.Drawing.Size(120, 17);
            this.checkBoxSubtitleTextBoxSyntaxColor.TabIndex = 34;
            this.checkBoxSubtitleTextBoxSyntaxColor.Text = "Use syntax coloring";
            this.checkBoxSubtitleTextBoxSyntaxColor.UseVisualStyleBackColor = true;
            this.checkBoxSubtitleTextBoxSyntaxColor.CheckedChanged += new System.EventHandler(this.checkBoxSubtitleTextBoxSyntaxColor_CheckedChanged);
            // 
            // labelSubtitleFontSize
            // 
            this.labelSubtitleFontSize.AutoSize = true;
            this.labelSubtitleFontSize.Location = new System.Drawing.Point(18, 17);
            this.labelSubtitleFontSize.Name = "labelSubtitleFontSize";
            this.labelSubtitleFontSize.Size = new System.Drawing.Size(87, 13);
            this.labelSubtitleFontSize.TabIndex = 30;
            this.labelSubtitleFontSize.Text = "Subtitle font size";
            // 
            // comboBoxSubtitleFontSize
            // 
            this.comboBoxSubtitleFontSize.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxSubtitleFontSize.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxSubtitleFontSize.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxSubtitleFontSize.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxSubtitleFontSize.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxSubtitleFontSize.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxSubtitleFontSize.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxSubtitleFontSize.DropDownHeight = 400;
            this.comboBoxSubtitleFontSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSubtitleFontSize.DropDownWidth = 73;
            this.comboBoxSubtitleFontSize.FormattingEnabled = true;
            this.comboBoxSubtitleFontSize.Items.AddRange(new string[] {
            "7",
            "8",
            "9",
            "10",
            "11",
            "12",
            "13",
            "14",
            "15",
            "16",
            "17",
            "18",
            "19",
            "20",
            "21",
            "22",
            "23",
            "24",
            "25",
            "26",
            "27",
            "28",
            "29",
            "30",
            "31",
            "32",
            "33",
            "34",
            "35",
            "36",
            "37",
            "38",
            "39",
            "40",
            "50",
            "60"});
            this.comboBoxSubtitleFontSize.Location = new System.Drawing.Point(21, 35);
            this.comboBoxSubtitleFontSize.MaxLength = 32767;
            this.comboBoxSubtitleFontSize.Name = "comboBoxSubtitleFontSize";
            this.comboBoxSubtitleFontSize.SelectedIndex = -1;
            this.comboBoxSubtitleFontSize.SelectedItem = null;
            this.comboBoxSubtitleFontSize.SelectedText = "";
            this.comboBoxSubtitleFontSize.Size = new System.Drawing.Size(73, 21);
            this.comboBoxSubtitleFontSize.TabIndex = 31;
            this.comboBoxSubtitleFontSize.UsePopupWindow = false;
            // 
            // checkBoxSubtitleFontBold
            // 
            this.checkBoxSubtitleFontBold.AutoSize = true;
            this.checkBoxSubtitleFontBold.Location = new System.Drawing.Point(21, 62);
            this.checkBoxSubtitleFontBold.Name = "checkBoxSubtitleFontBold";
            this.checkBoxSubtitleFontBold.Size = new System.Drawing.Size(46, 17);
            this.checkBoxSubtitleFontBold.TabIndex = 32;
            this.checkBoxSubtitleFontBold.Text = "Bold";
            this.checkBoxSubtitleFontBold.UseVisualStyleBackColor = true;
            // 
            // checkBoxSubtitleCenter
            // 
            this.checkBoxSubtitleCenter.AutoSize = true;
            this.checkBoxSubtitleCenter.Location = new System.Drawing.Point(21, 85);
            this.checkBoxSubtitleCenter.Name = "checkBoxSubtitleCenter";
            this.checkBoxSubtitleCenter.Size = new System.Drawing.Size(59, 17);
            this.checkBoxSubtitleCenter.TabIndex = 33;
            this.checkBoxSubtitleCenter.Text = "Center";
            this.checkBoxSubtitleCenter.UseVisualStyleBackColor = true;
            // 
            // groupBoxDarkTheme
            // 
            this.groupBoxDarkTheme.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxDarkTheme.Controls.Add(this.checkBoxDarkThemeShowListViewGridLines);
            this.groupBoxDarkTheme.Controls.Add(this.checkBoxDarkThemeEnabled);
            this.groupBoxDarkTheme.Controls.Add(this.panelDarkThemeBackColor);
            this.groupBoxDarkTheme.Controls.Add(this.buttonDarkThemeBackColor);
            this.groupBoxDarkTheme.Controls.Add(this.panelDarkThemeColor);
            this.groupBoxDarkTheme.Controls.Add(this.buttonDarkThemeColor);
            this.groupBoxDarkTheme.Location = new System.Drawing.Point(6, 307);
            this.groupBoxDarkTheme.Name = "groupBoxDarkTheme";
            this.groupBoxDarkTheme.Size = new System.Drawing.Size(368, 114);
            this.groupBoxDarkTheme.TabIndex = 40;
            this.groupBoxDarkTheme.TabStop = false;
            this.groupBoxDarkTheme.Text = "Dark theme";
            // 
            // checkBoxDarkThemeShowListViewGridLines
            // 
            this.checkBoxDarkThemeShowListViewGridLines.AutoSize = true;
            this.checkBoxDarkThemeShowListViewGridLines.Location = new System.Drawing.Point(191, 82);
            this.checkBoxDarkThemeShowListViewGridLines.Name = "checkBoxDarkThemeShowListViewGridLines";
            this.checkBoxDarkThemeShowListViewGridLines.Size = new System.Drawing.Size(138, 17);
            this.checkBoxDarkThemeShowListViewGridLines.TabIndex = 46;
            this.checkBoxDarkThemeShowListViewGridLines.Text = "Show list view grid lines";
            this.checkBoxDarkThemeShowListViewGridLines.UseVisualStyleBackColor = true;
            // 
            // checkBoxDarkThemeEnabled
            // 
            this.checkBoxDarkThemeEnabled.AutoSize = true;
            this.checkBoxDarkThemeEnabled.Location = new System.Drawing.Point(16, 25);
            this.checkBoxDarkThemeEnabled.Name = "checkBoxDarkThemeEnabled";
            this.checkBoxDarkThemeEnabled.Size = new System.Drawing.Size(101, 17);
            this.checkBoxDarkThemeEnabled.TabIndex = 41;
            this.checkBoxDarkThemeEnabled.Text = "Use dark theme";
            this.checkBoxDarkThemeEnabled.UseVisualStyleBackColor = true;
            this.checkBoxDarkThemeEnabled.CheckedChanged += new System.EventHandler(this.checkBoxDarkThemeEnabled_CheckedChanged);
            // 
            // panelDarkThemeBackColor
            // 
            this.panelDarkThemeBackColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelDarkThemeBackColor.Location = new System.Drawing.Point(313, 51);
            this.panelDarkThemeBackColor.Name = "panelDarkThemeBackColor";
            this.panelDarkThemeBackColor.Size = new System.Drawing.Size(21, 20);
            this.panelDarkThemeBackColor.TabIndex = 45;
            this.panelDarkThemeBackColor.Click += new System.EventHandler(this.buttonDarkThemeBackColor_Click);
            // 
            // buttonDarkThemeBackColor
            // 
            this.buttonDarkThemeBackColor.Location = new System.Drawing.Point(189, 50);
            this.buttonDarkThemeBackColor.Name = "buttonDarkThemeBackColor";
            this.buttonDarkThemeBackColor.Size = new System.Drawing.Size(112, 23);
            this.buttonDarkThemeBackColor.TabIndex = 44;
            this.buttonDarkThemeBackColor.Text = "Back color";
            this.buttonDarkThemeBackColor.UseVisualStyleBackColor = true;
            this.buttonDarkThemeBackColor.Click += new System.EventHandler(this.buttonDarkThemeBackColor_Click);
            // 
            // panelDarkThemeColor
            // 
            this.panelDarkThemeColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelDarkThemeColor.Location = new System.Drawing.Point(313, 22);
            this.panelDarkThemeColor.Name = "panelDarkThemeColor";
            this.panelDarkThemeColor.Size = new System.Drawing.Size(21, 20);
            this.panelDarkThemeColor.TabIndex = 43;
            this.panelDarkThemeColor.Click += new System.EventHandler(this.buttonDarkThemeColor_Click);
            // 
            // buttonDarkThemeColor
            // 
            this.buttonDarkThemeColor.Location = new System.Drawing.Point(189, 21);
            this.buttonDarkThemeColor.Name = "buttonDarkThemeColor";
            this.buttonDarkThemeColor.Size = new System.Drawing.Size(112, 23);
            this.buttonDarkThemeColor.TabIndex = 42;
            this.buttonDarkThemeColor.Text = "Color";
            this.buttonDarkThemeColor.UseVisualStyleBackColor = true;
            this.buttonDarkThemeColor.Click += new System.EventHandler(this.buttonDarkThemeColor_Click);
            // 
            // labelFontNote
            // 
            this.labelFontNote.AutoSize = true;
            this.labelFontNote.Location = new System.Drawing.Point(10, 433);
            this.labelFontNote.Name = "labelFontNote";
            this.labelFontNote.Size = new System.Drawing.Size(278, 13);
            this.labelFontNote.TabIndex = 41;
            this.labelFontNote.Text = "Note: This is only to set the font in the Subtitle Edit UI...";
            // 
            // panelNetwork
            // 
            this.panelNetwork.Controls.Add(this.groupBoxNetworkSession);
            this.panelNetwork.Controls.Add(this.groupBoxProxySettings);
            this.panelNetwork.Location = new System.Drawing.Point(230, 6);
            this.panelNetwork.Name = "panelNetwork";
            this.panelNetwork.Padding = new System.Windows.Forms.Padding(3);
            this.panelNetwork.Size = new System.Drawing.Size(864, 521);
            this.panelNetwork.TabIndex = 11;
            this.panelNetwork.Text = "Network";
            // 
            // groupBoxNetworkSession
            // 
            this.groupBoxNetworkSession.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxNetworkSession.Controls.Add(this.buttonNetworkSessionNewMessageSound);
            this.groupBoxNetworkSession.Controls.Add(this.textBoxNetworkSessionNewMessageSound);
            this.groupBoxNetworkSession.Controls.Add(this.labelNetworkSessionNewMessageSound);
            this.groupBoxNetworkSession.Location = new System.Drawing.Point(0, 242);
            this.groupBoxNetworkSession.Name = "groupBoxNetworkSession";
            this.groupBoxNetworkSession.Size = new System.Drawing.Size(851, 278);
            this.groupBoxNetworkSession.TabIndex = 30;
            this.groupBoxNetworkSession.TabStop = false;
            this.groupBoxNetworkSession.Text = "Network session settings";
            // 
            // buttonNetworkSessionNewMessageSound
            // 
            this.buttonNetworkSessionNewMessageSound.Location = new System.Drawing.Point(352, 49);
            this.buttonNetworkSessionNewMessageSound.Name = "buttonNetworkSessionNewMessageSound";
            this.buttonNetworkSessionNewMessageSound.Size = new System.Drawing.Size(29, 21);
            this.buttonNetworkSessionNewMessageSound.TabIndex = 24;
            this.buttonNetworkSessionNewMessageSound.Text = "...";
            this.buttonNetworkSessionNewMessageSound.UseVisualStyleBackColor = true;
            this.buttonNetworkSessionNewMessageSound.Click += new System.EventHandler(this.buttonNetworkSessionNewMessageSound_Click);
            // 
            // textBoxNetworkSessionNewMessageSound
            // 
            this.textBoxNetworkSessionNewMessageSound.BackColor = System.Drawing.SystemColors.Window;
            this.textBoxNetworkSessionNewMessageSound.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxNetworkSessionNewMessageSound.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxNetworkSessionNewMessageSound.Location = new System.Drawing.Point(28, 50);
            this.textBoxNetworkSessionNewMessageSound.Multiline = true;
            this.textBoxNetworkSessionNewMessageSound.Name = "textBoxNetworkSessionNewMessageSound";
            this.textBoxNetworkSessionNewMessageSound.Size = new System.Drawing.Size(318, 21);
            this.textBoxNetworkSessionNewMessageSound.TabIndex = 20;
            // 
            // labelNetworkSessionNewMessageSound
            // 
            this.labelNetworkSessionNewMessageSound.AutoSize = true;
            this.labelNetworkSessionNewMessageSound.Location = new System.Drawing.Point(25, 34);
            this.labelNetworkSessionNewMessageSound.Name = "labelNetworkSessionNewMessageSound";
            this.labelNetworkSessionNewMessageSound.Size = new System.Drawing.Size(209, 13);
            this.labelNetworkSessionNewMessageSound.TabIndex = 3;
            this.labelNetworkSessionNewMessageSound.Text = "Play sound file when new message arrives";
            // 
            // groupBoxProxySettings
            // 
            this.groupBoxProxySettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxProxySettings.Controls.Add(this.groupBoxProxyAuthentication);
            this.groupBoxProxySettings.Controls.Add(this.textBoxProxyAddress);
            this.groupBoxProxySettings.Controls.Add(this.labelProxyAddress);
            this.groupBoxProxySettings.Location = new System.Drawing.Point(0, 0);
            this.groupBoxProxySettings.Name = "groupBoxProxySettings";
            this.groupBoxProxySettings.Size = new System.Drawing.Size(851, 233);
            this.groupBoxProxySettings.TabIndex = 1;
            this.groupBoxProxySettings.TabStop = false;
            this.groupBoxProxySettings.Text = "Proxy server settings";
            // 
            // groupBoxProxyAuthentication
            // 
            this.groupBoxProxyAuthentication.Controls.Add(this.labelProxyAuthType);
            this.groupBoxProxyAuthentication.Controls.Add(this.checkBoxProxyUseDefaultCredentials);
            this.groupBoxProxyAuthentication.Controls.Add(this.comboBoxProxyAuthType);
            this.groupBoxProxyAuthentication.Controls.Add(this.textBoxProxyDomain);
            this.groupBoxProxyAuthentication.Controls.Add(this.labelProxyDomain);
            this.groupBoxProxyAuthentication.Controls.Add(this.textBoxProxyUserName);
            this.groupBoxProxyAuthentication.Controls.Add(this.labelProxyPassword);
            this.groupBoxProxyAuthentication.Controls.Add(this.labelProxyUserName);
            this.groupBoxProxyAuthentication.Controls.Add(this.textBoxProxyPassword);
            this.groupBoxProxyAuthentication.Location = new System.Drawing.Point(28, 60);
            this.groupBoxProxyAuthentication.Name = "groupBoxProxyAuthentication";
            this.groupBoxProxyAuthentication.Size = new System.Drawing.Size(459, 162);
            this.groupBoxProxyAuthentication.TabIndex = 29;
            this.groupBoxProxyAuthentication.TabStop = false;
            this.groupBoxProxyAuthentication.Text = "Authentication";
            // 
            // labelProxyAuthType
            // 
            this.labelProxyAuthType.AutoSize = true;
            this.labelProxyAuthType.Location = new System.Drawing.Point(12, 113);
            this.labelProxyAuthType.Name = "labelProxyAuthType";
            this.labelProxyAuthType.Size = new System.Drawing.Size(55, 13);
            this.labelProxyAuthType.TabIndex = 33;
            this.labelProxyAuthType.Text = "Auth type";
            // 
            // checkBoxProxyUseDefaultCredentials
            // 
            this.checkBoxProxyUseDefaultCredentials.AutoSize = true;
            this.checkBoxProxyUseDefaultCredentials.Location = new System.Drawing.Point(139, 132);
            this.checkBoxProxyUseDefaultCredentials.Name = "checkBoxProxyUseDefaultCredentials";
            this.checkBoxProxyUseDefaultCredentials.Size = new System.Drawing.Size(136, 17);
            this.checkBoxProxyUseDefaultCredentials.TabIndex = 32;
            this.checkBoxProxyUseDefaultCredentials.Text = "Use default credentials";
            this.checkBoxProxyUseDefaultCredentials.UseVisualStyleBackColor = true;
            // 
            // comboBoxProxyAuthType
            // 
            this.comboBoxProxyAuthType.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxProxyAuthType.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxProxyAuthType.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxProxyAuthType.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxProxyAuthType.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxProxyAuthType.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxProxyAuthType.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxProxyAuthType.DropDownHeight = 400;
            this.comboBoxProxyAuthType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.comboBoxProxyAuthType.DropDownWidth = 192;
            this.comboBoxProxyAuthType.FormattingEnabled = true;
            this.comboBoxProxyAuthType.Items.AddRange(new string[] {
            "",
            "Basic",
            "Digest",
            "NTLM",
            "Negotiate",
            "Kerberos"});
            this.comboBoxProxyAuthType.Location = new System.Drawing.Point(138, 102);
            this.comboBoxProxyAuthType.MaxLength = 32767;
            this.comboBoxProxyAuthType.Name = "comboBoxProxyAuthType";
            this.comboBoxProxyAuthType.SelectedIndex = -1;
            this.comboBoxProxyAuthType.SelectedItem = null;
            this.comboBoxProxyAuthType.SelectedText = "";
            this.comboBoxProxyAuthType.Size = new System.Drawing.Size(192, 21);
            this.comboBoxProxyAuthType.TabIndex = 31;
            this.comboBoxProxyAuthType.TabStop = false;
            this.comboBoxProxyAuthType.UsePopupWindow = false;
            // 
            // textBoxProxyDomain
            // 
            this.textBoxProxyDomain.BackColor = System.Drawing.SystemColors.Window;
            this.textBoxProxyDomain.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxProxyDomain.Location = new System.Drawing.Point(138, 68);
            this.textBoxProxyDomain.Multiline = true;
            this.textBoxProxyDomain.Name = "textBoxProxyDomain";
            this.textBoxProxyDomain.Size = new System.Drawing.Size(192, 21);
            this.textBoxProxyDomain.TabIndex = 30;
            // 
            // labelProxyDomain
            // 
            this.labelProxyDomain.AutoSize = true;
            this.labelProxyDomain.Location = new System.Drawing.Point(12, 74);
            this.labelProxyDomain.Name = "labelProxyDomain";
            this.labelProxyDomain.Size = new System.Drawing.Size(42, 13);
            this.labelProxyDomain.TabIndex = 29;
            this.labelProxyDomain.Text = "Domain";
            // 
            // textBoxProxyUserName
            // 
            this.textBoxProxyUserName.BackColor = System.Drawing.SystemColors.Window;
            this.textBoxProxyUserName.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxProxyUserName.Location = new System.Drawing.Point(138, 16);
            this.textBoxProxyUserName.Multiline = true;
            this.textBoxProxyUserName.Name = "textBoxProxyUserName";
            this.textBoxProxyUserName.Size = new System.Drawing.Size(192, 21);
            this.textBoxProxyUserName.TabIndex = 22;
            // 
            // labelProxyPassword
            // 
            this.labelProxyPassword.AutoSize = true;
            this.labelProxyPassword.Location = new System.Drawing.Point(12, 48);
            this.labelProxyPassword.Name = "labelProxyPassword";
            this.labelProxyPassword.Size = new System.Drawing.Size(53, 13);
            this.labelProxyPassword.TabIndex = 28;
            this.labelProxyPassword.Text = "Password";
            // 
            // labelProxyUserName
            // 
            this.labelProxyUserName.AutoSize = true;
            this.labelProxyUserName.Location = new System.Drawing.Point(12, 22);
            this.labelProxyUserName.Name = "labelProxyUserName";
            this.labelProxyUserName.Size = new System.Drawing.Size(58, 13);
            this.labelProxyUserName.TabIndex = 2;
            this.labelProxyUserName.Text = "User name";
            // 
            // textBoxProxyPassword
            // 
            this.textBoxProxyPassword.BackColor = System.Drawing.SystemColors.Window;
            this.textBoxProxyPassword.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxProxyPassword.Location = new System.Drawing.Point(138, 42);
            this.textBoxProxyPassword.Multiline = true;
            this.textBoxProxyPassword.Name = "textBoxProxyPassword";
            this.textBoxProxyPassword.Size = new System.Drawing.Size(192, 21);
            this.textBoxProxyPassword.TabIndex = 24;
            this.textBoxProxyPassword.UseSystemPasswordChar = true;
            // 
            // textBoxProxyAddress
            // 
            this.textBoxProxyAddress.BackColor = System.Drawing.SystemColors.Window;
            this.textBoxProxyAddress.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxProxyAddress.Location = new System.Drawing.Point(166, 34);
            this.textBoxProxyAddress.Multiline = true;
            this.textBoxProxyAddress.Name = "textBoxProxyAddress";
            this.textBoxProxyAddress.Size = new System.Drawing.Size(321, 21);
            this.textBoxProxyAddress.TabIndex = 20;
            // 
            // labelProxyAddress
            // 
            this.labelProxyAddress.AutoSize = true;
            this.labelProxyAddress.Location = new System.Drawing.Point(25, 34);
            this.labelProxyAddress.Name = "labelProxyAddress";
            this.labelProxyAddress.Size = new System.Drawing.Size(76, 13);
            this.labelProxyAddress.TabIndex = 3;
            this.labelProxyAddress.Text = "Proxy address";
            // 
            // labelStatus
            // 
            this.labelStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(12, 549);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(60, 13);
            this.labelStatus.TabIndex = 12;
            this.labelStatus.Text = "labelStatus";
            // 
            // openFileDialogFFmpeg
            // 
            this.openFileDialogFFmpeg.FileName = "openFileDialog1";
            // 
            // buttonReset
            // 
            this.buttonReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonReset.Location = new System.Drawing.Point(895, 539);
            this.buttonReset.Name = "buttonReset";
            this.buttonReset.Size = new System.Drawing.Size(185, 23);
            this.buttonReset.TabIndex = 15;
            this.buttonReset.Text = "Restore default settings";
            this.buttonReset.UseVisualStyleBackColor = true;
            this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
            // 
            // toolTipContinuationPreview
            // 
            this.toolTipContinuationPreview.AutoPopDelay = 60000;
            this.toolTipContinuationPreview.InitialDelay = 500;
            this.toolTipContinuationPreview.ReshowDelay = 100;
            // 
            // panelFileTypeAssociations
            // 
            this.panelFileTypeAssociations.Controls.Add(this.buttonUpdateFileTypeAssociations);
            this.panelFileTypeAssociations.Controls.Add(this.listViewFileTypeAssociations);
            this.panelFileTypeAssociations.Location = new System.Drawing.Point(230, 6);
            this.panelFileTypeAssociations.Name = "panelFileTypeAssociations";
            this.panelFileTypeAssociations.Size = new System.Drawing.Size(852, 521);
            this.panelFileTypeAssociations.TabIndex = 16;
            // 
            // buttonUpdateFileTypeAssociations
            // 
            this.buttonUpdateFileTypeAssociations.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonUpdateFileTypeAssociations.Location = new System.Drawing.Point(209, 497);
            this.buttonUpdateFileTypeAssociations.Name = "buttonUpdateFileTypeAssociations";
            this.buttonUpdateFileTypeAssociations.Size = new System.Drawing.Size(218, 23);
            this.buttonUpdateFileTypeAssociations.TabIndex = 23;
            this.buttonUpdateFileTypeAssociations.Text = "Update file type associations";
            this.buttonUpdateFileTypeAssociations.UseVisualStyleBackColor = true;
            this.buttonUpdateFileTypeAssociations.Click += new System.EventHandler(this.buttonUpdateFileTypeAssociations_Click);
            // 
            // listViewFileTypeAssociations
            // 
            this.listViewFileTypeAssociations.CheckBoxes = true;
            this.listViewFileTypeAssociations.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.listViewFileTypeAssociations.HideSelection = false;
            this.listViewFileTypeAssociations.Location = new System.Drawing.Point(0, 4);
            this.listViewFileTypeAssociations.Name = "listViewFileTypeAssociations";
            this.listViewFileTypeAssociations.Size = new System.Drawing.Size(199, 516);
            this.listViewFileTypeAssociations.TabIndex = 22;
            this.listViewFileTypeAssociations.UseCompatibleStateImageBehavior = false;
            this.listViewFileTypeAssociations.View = System.Windows.Forms.View.Details;
            // 
            // labelUpdateFileTypeAssociationsStatus
            // 
            this.labelUpdateFileTypeAssociationsStatus.AutoSize = true;
            this.labelUpdateFileTypeAssociationsStatus.Location = new System.Drawing.Point(439, 531);
            this.labelUpdateFileTypeAssociationsStatus.Name = "labelUpdateFileTypeAssociationsStatus";
            this.labelUpdateFileTypeAssociationsStatus.Size = new System.Drawing.Size(38, 13);
            this.labelUpdateFileTypeAssociationsStatus.TabIndex = 24;
            this.labelUpdateFileTypeAssociationsStatus.Text = "Status";
            // 
            // imageListFileTypeAssociations
            // 
            this.imageListFileTypeAssociations.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageListFileTypeAssociations.ImageSize = new System.Drawing.Size(32, 32);
            this.imageListFileTypeAssociations.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // toolTipDialogStylePreview
            // 
            this.toolTipDialogStylePreview.AutoPopDelay = 60000;
            this.toolTipDialogStylePreview.InitialDelay = 500;
            this.toolTipDialogStylePreview.ReshowDelay = 100;
            // 
            // panelAutoTranslate
            // 
            this.panelAutoTranslate.Controls.Add(this.groupBoxAutoTranslatePapago);
            this.panelAutoTranslate.Controls.Add(this.groupBoxAutoTranslateChatGpt);
            this.panelAutoTranslate.Controls.Add(this.groupBoxDeepL);
            this.panelAutoTranslate.Controls.Add(this.groupBoxMyMemory);
            this.panelAutoTranslate.Controls.Add(this.groupBoxLibreTranslate);
            this.panelAutoTranslate.Controls.Add(this.groupBoxNllbServe);
            this.panelAutoTranslate.Controls.Add(this.groupBoxNllbApi);
            this.panelAutoTranslate.Controls.Add(this.groupBoxGoogleTranslate);
            this.panelAutoTranslate.Controls.Add(this.groupBoxBing);
            this.panelAutoTranslate.Location = new System.Drawing.Point(227, 3);
            this.panelAutoTranslate.Name = "panelAutoTranslate";
            this.panelAutoTranslate.Padding = new System.Windows.Forms.Padding(3);
            this.panelAutoTranslate.Size = new System.Drawing.Size(864, 521);
            this.panelAutoTranslate.TabIndex = 33;
            this.panelAutoTranslate.Text = "Tools";
            // 
            // groupBoxAutoTranslatePapago
            // 
            this.groupBoxAutoTranslatePapago.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxAutoTranslatePapago.Controls.Add(this.nikseTextBoxPapagoClientId);
            this.groupBoxAutoTranslatePapago.Controls.Add(this.nikseTextBoxPapagoClientSecret);
            this.groupBoxAutoTranslatePapago.Controls.Add(this.labelSecretPapago);
            this.groupBoxAutoTranslatePapago.Controls.Add(this.labelApiKeyPapago);
            this.groupBoxAutoTranslatePapago.Controls.Add(this.linkLabelPapago);
            this.groupBoxAutoTranslatePapago.Controls.Add(this.label11);
            this.groupBoxAutoTranslatePapago.Location = new System.Drawing.Point(431, 368);
            this.groupBoxAutoTranslatePapago.Name = "groupBoxAutoTranslatePapago";
            this.groupBoxAutoTranslatePapago.Size = new System.Drawing.Size(411, 110);
            this.groupBoxAutoTranslatePapago.TabIndex = 47;
            this.groupBoxAutoTranslatePapago.TabStop = false;
            this.groupBoxAutoTranslatePapago.Text = "Papago";
            // 
            // nikseTextBoxPapagoClientId
            // 
            this.nikseTextBoxPapagoClientId.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.nikseTextBoxPapagoClientId.Location = new System.Drawing.Point(8, 41);
            this.nikseTextBoxPapagoClientId.Name = "nikseTextBoxPapagoClientId";
            this.nikseTextBoxPapagoClientId.Size = new System.Drawing.Size(384, 21);
            this.nikseTextBoxPapagoClientId.TabIndex = 34;
            // 
            // nikseTextBoxPapagoClientSecret
            // 
            this.nikseTextBoxPapagoClientSecret.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.nikseTextBoxPapagoClientSecret.Location = new System.Drawing.Point(78, 76);
            this.nikseTextBoxPapagoClientSecret.Name = "nikseTextBoxPapagoClientSecret";
            this.nikseTextBoxPapagoClientSecret.Size = new System.Drawing.Size(314, 21);
            this.nikseTextBoxPapagoClientSecret.TabIndex = 36;
            // 
            // labelSecretPapago
            // 
            this.labelSecretPapago.AutoSize = true;
            this.labelSecretPapago.Location = new System.Drawing.Point(6, 80);
            this.labelSecretPapago.Name = "labelSecretPapago";
            this.labelSecretPapago.Size = new System.Drawing.Size(67, 13);
            this.labelSecretPapago.TabIndex = 35;
            this.labelSecretPapago.Text = "Client secret";
            // 
            // labelApiKeyPapago
            // 
            this.labelApiKeyPapago.AutoSize = true;
            this.labelApiKeyPapago.Location = new System.Drawing.Point(6, 23);
            this.labelApiKeyPapago.Name = "labelApiKeyPapago";
            this.labelApiKeyPapago.Size = new System.Drawing.Size(48, 13);
            this.labelApiKeyPapago.TabIndex = 30;
            this.labelApiKeyPapago.Text = "Client ID";
            // 
            // linkLabelPapago
            // 
            this.linkLabelPapago.Location = new System.Drawing.Point(222, 17);
            this.linkLabelPapago.Name = "linkLabelPapago";
            this.linkLabelPapago.Size = new System.Drawing.Size(170, 22);
            this.linkLabelPapago.TabIndex = 24;
            this.linkLabelPapago.TabStop = true;
            this.linkLabelPapago.Text = "More info";
            this.linkLabelPapago.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(16, 106);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(0, 13);
            this.label11.TabIndex = 25;
            // 
            // groupBoxAutoTranslateChatGpt
            // 
            this.groupBoxAutoTranslateChatGpt.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxAutoTranslateChatGpt.Controls.Add(this.nikseTextBoxChatGptUrl);
            this.groupBoxAutoTranslateChatGpt.Controls.Add(this.nikseComboBoxChatGptModel);
            this.groupBoxAutoTranslateChatGpt.Controls.Add(this.labelChatGptModel);
            this.groupBoxAutoTranslateChatGpt.Controls.Add(this.nikseTextBoxChatGptApiKey);
            this.groupBoxAutoTranslateChatGpt.Controls.Add(this.labelApiKeyChatGpt);
            this.groupBoxAutoTranslateChatGpt.Controls.Add(this.labelUrlChatGpt);
            this.groupBoxAutoTranslateChatGpt.Controls.Add(this.linkLabelMoreInfoChatGpt);
            this.groupBoxAutoTranslateChatGpt.Controls.Add(this.label10);
            this.groupBoxAutoTranslateChatGpt.Location = new System.Drawing.Point(430, 210);
            this.groupBoxAutoTranslateChatGpt.Name = "groupBoxAutoTranslateChatGpt";
            this.groupBoxAutoTranslateChatGpt.Size = new System.Drawing.Size(411, 148);
            this.groupBoxAutoTranslateChatGpt.TabIndex = 46;
            this.groupBoxAutoTranslateChatGpt.TabStop = false;
            this.groupBoxAutoTranslateChatGpt.Text = "ChatGPT API";
            // 
            // nikseTextBoxChatGptUrl
            // 
            this.nikseTextBoxChatGptUrl.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.nikseTextBoxChatGptUrl.Location = new System.Drawing.Point(8, 41);
            this.nikseTextBoxChatGptUrl.Name = "nikseTextBoxChatGptUrl";
            this.nikseTextBoxChatGptUrl.Size = new System.Drawing.Size(384, 21);
            this.nikseTextBoxChatGptUrl.TabIndex = 34;
            // 
            // nikseComboBoxChatGptModel
            // 
            this.nikseComboBoxChatGptModel.BackColor = System.Drawing.SystemColors.Window;
            this.nikseComboBoxChatGptModel.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.nikseComboBoxChatGptModel.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.nikseComboBoxChatGptModel.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.nikseComboBoxChatGptModel.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.nikseComboBoxChatGptModel.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.nikseComboBoxChatGptModel.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.nikseComboBoxChatGptModel.DropDownHeight = 400;
            this.nikseComboBoxChatGptModel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.nikseComboBoxChatGptModel.DropDownWidth = 375;
            this.nikseComboBoxChatGptModel.FormattingEnabled = true;
            this.nikseComboBoxChatGptModel.Items.AddRange(new string[] {
            "gpt-4o",
            "gpt-4-turbo",
            "gpt-3.5-turbo",
            "gpt-4"});
            this.nikseComboBoxChatGptModel.Location = new System.Drawing.Point(52, 109);
            this.nikseComboBoxChatGptModel.MaxLength = 32767;
            this.nikseComboBoxChatGptModel.Name = "nikseComboBoxChatGptModel";
            this.nikseComboBoxChatGptModel.SelectedIndex = -1;
            this.nikseComboBoxChatGptModel.SelectedItem = null;
            this.nikseComboBoxChatGptModel.SelectedText = "";
            this.nikseComboBoxChatGptModel.Size = new System.Drawing.Size(340, 21);
            this.nikseComboBoxChatGptModel.TabIndex = 38;
            this.nikseComboBoxChatGptModel.UsePopupWindow = false;
            // 
            // labelChatGptModel
            // 
            this.labelChatGptModel.AutoSize = true;
            this.labelChatGptModel.Location = new System.Drawing.Point(6, 112);
            this.labelChatGptModel.Name = "labelChatGptModel";
            this.labelChatGptModel.Size = new System.Drawing.Size(35, 13);
            this.labelChatGptModel.TabIndex = 37;
            this.labelChatGptModel.Text = "Model";
            // 
            // nikseTextBoxChatGptApiKey
            // 
            this.nikseTextBoxChatGptApiKey.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.nikseTextBoxChatGptApiKey.Location = new System.Drawing.Point(52, 76);
            this.nikseTextBoxChatGptApiKey.Name = "nikseTextBoxChatGptApiKey";
            this.nikseTextBoxChatGptApiKey.Size = new System.Drawing.Size(340, 21);
            this.nikseTextBoxChatGptApiKey.TabIndex = 36;
            // 
            // labelApiKeyChatGpt
            // 
            this.labelApiKeyChatGpt.AutoSize = true;
            this.labelApiKeyChatGpt.Location = new System.Drawing.Point(6, 80);
            this.labelApiKeyChatGpt.Name = "labelApiKeyChatGpt";
            this.labelApiKeyChatGpt.Size = new System.Drawing.Size(44, 13);
            this.labelApiKeyChatGpt.TabIndex = 35;
            this.labelApiKeyChatGpt.Text = "API key";
            // 
            // labelUrlChatGpt
            // 
            this.labelUrlChatGpt.AutoSize = true;
            this.labelUrlChatGpt.Location = new System.Drawing.Point(6, 23);
            this.labelUrlChatGpt.Name = "labelUrlChatGpt";
            this.labelUrlChatGpt.Size = new System.Drawing.Size(20, 13);
            this.labelUrlChatGpt.TabIndex = 30;
            this.labelUrlChatGpt.Text = "Url";
            // 
            // linkLabelMoreInfoChatGpt
            // 
            this.linkLabelMoreInfoChatGpt.Location = new System.Drawing.Point(205, 17);
            this.linkLabelMoreInfoChatGpt.Name = "linkLabelMoreInfoChatGpt";
            this.linkLabelMoreInfoChatGpt.Size = new System.Drawing.Size(187, 21);
            this.linkLabelMoreInfoChatGpt.TabIndex = 24;
            this.linkLabelMoreInfoChatGpt.TabStop = true;
            this.linkLabelMoreInfoChatGpt.Text = "More info";
            this.linkLabelMoreInfoChatGpt.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(16, 106);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(0, 13);
            this.label10.TabIndex = 25;
            // 
            // groupBoxDeepL
            // 
            this.groupBoxDeepL.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxDeepL.Controls.Add(this.nikseTextBoxDeepLUrl);
            this.groupBoxDeepL.Controls.Add(this.nikseTextBoxDeepLApiKey);
            this.groupBoxDeepL.Controls.Add(this.labelDeepLApiKey);
            this.groupBoxDeepL.Controls.Add(this.labelDeepLUrl);
            this.groupBoxDeepL.Controls.Add(this.linkLabelMoreInfoDeepl);
            this.groupBoxDeepL.Controls.Add(this.label9);
            this.groupBoxDeepL.Location = new System.Drawing.Point(9, 399);
            this.groupBoxDeepL.Name = "groupBoxDeepL";
            this.groupBoxDeepL.Size = new System.Drawing.Size(410, 110);
            this.groupBoxDeepL.TabIndex = 37;
            this.groupBoxDeepL.TabStop = false;
            this.groupBoxDeepL.Text = "DeepL";
            // 
            // nikseTextBoxDeepLUrl
            // 
            this.nikseTextBoxDeepLUrl.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.nikseTextBoxDeepLUrl.Location = new System.Drawing.Point(8, 41);
            this.nikseTextBoxDeepLUrl.Name = "nikseTextBoxDeepLUrl";
            this.nikseTextBoxDeepLUrl.Size = new System.Drawing.Size(376, 21);
            this.nikseTextBoxDeepLUrl.TabIndex = 34;
            // 
            // nikseTextBoxDeepLApiKey
            // 
            this.nikseTextBoxDeepLApiKey.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.nikseTextBoxDeepLApiKey.Location = new System.Drawing.Point(54, 76);
            this.nikseTextBoxDeepLApiKey.Name = "nikseTextBoxDeepLApiKey";
            this.nikseTextBoxDeepLApiKey.Size = new System.Drawing.Size(330, 21);
            this.nikseTextBoxDeepLApiKey.TabIndex = 36;
            // 
            // labelDeepLApiKey
            // 
            this.labelDeepLApiKey.AutoSize = true;
            this.labelDeepLApiKey.Location = new System.Drawing.Point(6, 80);
            this.labelDeepLApiKey.Name = "labelDeepLApiKey";
            this.labelDeepLApiKey.Size = new System.Drawing.Size(44, 13);
            this.labelDeepLApiKey.TabIndex = 35;
            this.labelDeepLApiKey.Text = "API key";
            // 
            // labelDeepLUrl
            // 
            this.labelDeepLUrl.AutoSize = true;
            this.labelDeepLUrl.Location = new System.Drawing.Point(6, 23);
            this.labelDeepLUrl.Name = "labelDeepLUrl";
            this.labelDeepLUrl.Size = new System.Drawing.Size(20, 13);
            this.labelDeepLUrl.TabIndex = 30;
            this.labelDeepLUrl.Text = "Url";
            // 
            // linkLabelMoreInfoDeepl
            // 
            this.linkLabelMoreInfoDeepl.Location = new System.Drawing.Point(222, 16);
            this.linkLabelMoreInfoDeepl.Name = "linkLabelMoreInfoDeepl";
            this.linkLabelMoreInfoDeepl.Size = new System.Drawing.Size(160, 20);
            this.linkLabelMoreInfoDeepl.TabIndex = 24;
            this.linkLabelMoreInfoDeepl.TabStop = true;
            this.linkLabelMoreInfoDeepl.Text = "More info";
            this.linkLabelMoreInfoDeepl.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.linkLabelMoreInfoDeepl.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabelMoreInfoDeepLLinkClicked);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(16, 106);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(0, 13);
            this.label9.TabIndex = 25;
            // 
            // groupBoxMyMemory
            // 
            this.groupBoxMyMemory.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxMyMemory.Controls.Add(this.nikseTextBoxMyMemoryApiKey);
            this.groupBoxMyMemory.Controls.Add(this.labelMyMemoryApiKey);
            this.groupBoxMyMemory.Controls.Add(this.linkLabelMyMemoryTranslate);
            this.groupBoxMyMemory.Controls.Add(this.label8);
            this.groupBoxMyMemory.Location = new System.Drawing.Point(431, 127);
            this.groupBoxMyMemory.Name = "groupBoxMyMemory";
            this.groupBoxMyMemory.Size = new System.Drawing.Size(410, 73);
            this.groupBoxMyMemory.TabIndex = 45;
            this.groupBoxMyMemory.TabStop = false;
            this.groupBoxMyMemory.Text = "MyMemory API";
            // 
            // nikseTextBoxMyMemoryApiKey
            // 
            this.nikseTextBoxMyMemoryApiKey.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.nikseTextBoxMyMemoryApiKey.Location = new System.Drawing.Point(8, 42);
            this.nikseTextBoxMyMemoryApiKey.Name = "nikseTextBoxMyMemoryApiKey";
            this.nikseTextBoxMyMemoryApiKey.Size = new System.Drawing.Size(376, 21);
            this.nikseTextBoxMyMemoryApiKey.TabIndex = 36;
            // 
            // labelMyMemoryApiKey
            // 
            this.labelMyMemoryApiKey.AutoSize = true;
            this.labelMyMemoryApiKey.Location = new System.Drawing.Point(6, 24);
            this.labelMyMemoryApiKey.Name = "labelMyMemoryApiKey";
            this.labelMyMemoryApiKey.Size = new System.Drawing.Size(44, 13);
            this.labelMyMemoryApiKey.TabIndex = 35;
            this.labelMyMemoryApiKey.Text = "API key";
            // 
            // linkLabelMyMemoryTranslate
            // 
            this.linkLabelMyMemoryTranslate.Location = new System.Drawing.Point(246, 17);
            this.linkLabelMyMemoryTranslate.Name = "linkLabelMyMemoryTranslate";
            this.linkLabelMyMemoryTranslate.Size = new System.Drawing.Size(138, 19);
            this.linkLabelMyMemoryTranslate.TabIndex = 24;
            this.linkLabelMyMemoryTranslate.TabStop = true;
            this.linkLabelMyMemoryTranslate.Text = "More info";
            this.linkLabelMyMemoryTranslate.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.linkLabelMyMemoryTranslate.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelMyMemoryTranslate_LinkClicked);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(16, 106);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(0, 13);
            this.label8.TabIndex = 25;
            // 
            // groupBoxLibreTranslate
            // 
            this.groupBoxLibreTranslate.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxLibreTranslate.Controls.Add(this.nikseTextBoxLibreTranslateApiKey);
            this.groupBoxLibreTranslate.Controls.Add(this.labelLibreApiKey);
            this.groupBoxLibreTranslate.Controls.Add(this.nikseTextBoxLibreTranslateUrl);
            this.groupBoxLibreTranslate.Controls.Add(this.labelLibreUrl);
            this.groupBoxLibreTranslate.Controls.Add(this.linkLabelLibreTranslateApi);
            this.groupBoxLibreTranslate.Controls.Add(this.label7);
            this.groupBoxLibreTranslate.Location = new System.Drawing.Point(431, 9);
            this.groupBoxLibreTranslate.Name = "groupBoxLibreTranslate";
            this.groupBoxLibreTranslate.Size = new System.Drawing.Size(411, 110);
            this.groupBoxLibreTranslate.TabIndex = 40;
            this.groupBoxLibreTranslate.TabStop = false;
            this.groupBoxLibreTranslate.Text = "LibreTranslate API";
            // 
            // nikseTextBoxLibreTranslateApiKey
            // 
            this.nikseTextBoxLibreTranslateApiKey.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.nikseTextBoxLibreTranslateApiKey.Location = new System.Drawing.Point(52, 76);
            this.nikseTextBoxLibreTranslateApiKey.Name = "nikseTextBoxLibreTranslateApiKey";
            this.nikseTextBoxLibreTranslateApiKey.Size = new System.Drawing.Size(340, 21);
            this.nikseTextBoxLibreTranslateApiKey.TabIndex = 36;
            // 
            // labelLibreApiKey
            // 
            this.labelLibreApiKey.AutoSize = true;
            this.labelLibreApiKey.Location = new System.Drawing.Point(6, 80);
            this.labelLibreApiKey.Name = "labelLibreApiKey";
            this.labelLibreApiKey.Size = new System.Drawing.Size(44, 13);
            this.labelLibreApiKey.TabIndex = 35;
            this.labelLibreApiKey.Text = "API key";
            // 
            // nikseTextBoxLibreTranslateUrl
            // 
            this.nikseTextBoxLibreTranslateUrl.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.nikseTextBoxLibreTranslateUrl.Location = new System.Drawing.Point(8, 41);
            this.nikseTextBoxLibreTranslateUrl.Name = "nikseTextBoxLibreTranslateUrl";
            this.nikseTextBoxLibreTranslateUrl.Size = new System.Drawing.Size(384, 21);
            this.nikseTextBoxLibreTranslateUrl.TabIndex = 34;
            // 
            // labelLibreUrl
            // 
            this.labelLibreUrl.AutoSize = true;
            this.labelLibreUrl.Location = new System.Drawing.Point(6, 23);
            this.labelLibreUrl.Name = "labelLibreUrl";
            this.labelLibreUrl.Size = new System.Drawing.Size(20, 13);
            this.labelLibreUrl.TabIndex = 30;
            this.labelLibreUrl.Text = "Url";
            // 
            // linkLabelLibreTranslateApi
            // 
            this.linkLabelLibreTranslateApi.Location = new System.Drawing.Point(340, 17);
            this.linkLabelLibreTranslateApi.Name = "linkLabelLibreTranslateApi";
            this.linkLabelLibreTranslateApi.Size = new System.Drawing.Size(52, 13);
            this.linkLabelLibreTranslateApi.TabIndex = 24;
            this.linkLabelLibreTranslateApi.TabStop = true;
            this.linkLabelLibreTranslateApi.Text = "More info";
            this.linkLabelLibreTranslateApi.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.linkLabelLibreTranslateApi.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelLibreTranslateApi_LinkClicked);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(16, 106);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(0, 13);
            this.label7.TabIndex = 25;
            // 
            // groupBoxNllbServe
            // 
            this.groupBoxNllbServe.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxNllbServe.Controls.Add(this.nikseTextBoxNllbServeUrl);
            this.groupBoxNllbServe.Controls.Add(this.nikseTextBoxNllbServeModel);
            this.groupBoxNllbServe.Controls.Add(this.labelNllbServeModel);
            this.groupBoxNllbServe.Controls.Add(this.labelNllbServeUrl);
            this.groupBoxNllbServe.Controls.Add(this.linkLabelNllbServe);
            this.groupBoxNllbServe.Controls.Add(this.label4);
            this.groupBoxNllbServe.Location = new System.Drawing.Point(9, 213);
            this.groupBoxNllbServe.Name = "groupBoxNllbServe";
            this.groupBoxNllbServe.Size = new System.Drawing.Size(410, 103);
            this.groupBoxNllbServe.TabIndex = 35;
            this.groupBoxNllbServe.TabStop = false;
            this.groupBoxNllbServe.Text = "NLLM Serve";
            // 
            // nikseTextBoxNllbServeUrl
            // 
            this.nikseTextBoxNllbServeUrl.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.nikseTextBoxNllbServeUrl.Location = new System.Drawing.Point(8, 40);
            this.nikseTextBoxNllbServeUrl.Name = "nikseTextBoxNllbServeUrl";
            this.nikseTextBoxNllbServeUrl.Size = new System.Drawing.Size(376, 21);
            this.nikseTextBoxNllbServeUrl.TabIndex = 34;
            // 
            // nikseTextBoxNllbServeModel
            // 
            this.nikseTextBoxNllbServeModel.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.nikseTextBoxNllbServeModel.Location = new System.Drawing.Point(53, 71);
            this.nikseTextBoxNllbServeModel.Name = "nikseTextBoxNllbServeModel";
            this.nikseTextBoxNllbServeModel.Size = new System.Drawing.Size(331, 21);
            this.nikseTextBoxNllbServeModel.TabIndex = 37;
            // 
            // labelNllbServeModel
            // 
            this.labelNllbServeModel.AutoSize = true;
            this.labelNllbServeModel.Location = new System.Drawing.Point(7, 75);
            this.labelNllbServeModel.Name = "labelNllbServeModel";
            this.labelNllbServeModel.Size = new System.Drawing.Size(35, 13);
            this.labelNllbServeModel.TabIndex = 36;
            this.labelNllbServeModel.Text = "Model";
            // 
            // labelNllbServeUrl
            // 
            this.labelNllbServeUrl.AutoSize = true;
            this.labelNllbServeUrl.Location = new System.Drawing.Point(6, 22);
            this.labelNllbServeUrl.Name = "labelNllbServeUrl";
            this.labelNllbServeUrl.Size = new System.Drawing.Size(20, 13);
            this.labelNllbServeUrl.TabIndex = 30;
            this.labelNllbServeUrl.Text = "Url";
            // 
            // linkLabelNllbServe
            // 
            this.linkLabelNllbServe.Location = new System.Drawing.Point(241, 16);
            this.linkLabelNllbServe.Name = "linkLabelNllbServe";
            this.linkLabelNllbServe.Size = new System.Drawing.Size(143, 19);
            this.linkLabelNllbServe.TabIndex = 24;
            this.linkLabelNllbServe.TabStop = true;
            this.linkLabelNllbServe.Text = "More info";
            this.linkLabelNllbServe.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.linkLabelNllbServe.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelNllbServe_LinkClicked);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(16, 106);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(0, 13);
            this.label4.TabIndex = 25;
            // 
            // groupBoxNllbApi
            // 
            this.groupBoxNllbApi.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxNllbApi.Controls.Add(this.nikseTextBoxNllbApiUrl);
            this.groupBoxNllbApi.Controls.Add(this.labelNllbApiUrl);
            this.groupBoxNllbApi.Controls.Add(this.linkLabelNllbApi);
            this.groupBoxNllbApi.Controls.Add(this.label5);
            this.groupBoxNllbApi.Location = new System.Drawing.Point(9, 322);
            this.groupBoxNllbApi.Name = "groupBoxNllbApi";
            this.groupBoxNllbApi.Size = new System.Drawing.Size(410, 73);
            this.groupBoxNllbApi.TabIndex = 36;
            this.groupBoxNllbApi.TabStop = false;
            this.groupBoxNllbApi.Text = "NLLM API";
            // 
            // nikseTextBoxNllbApiUrl
            // 
            this.nikseTextBoxNllbApiUrl.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.nikseTextBoxNllbApiUrl.Location = new System.Drawing.Point(8, 40);
            this.nikseTextBoxNllbApiUrl.Name = "nikseTextBoxNllbApiUrl";
            this.nikseTextBoxNllbApiUrl.Size = new System.Drawing.Size(376, 21);
            this.nikseTextBoxNllbApiUrl.TabIndex = 34;
            // 
            // labelNllbApiUrl
            // 
            this.labelNllbApiUrl.AutoSize = true;
            this.labelNllbApiUrl.Location = new System.Drawing.Point(6, 22);
            this.labelNllbApiUrl.Name = "labelNllbApiUrl";
            this.labelNllbApiUrl.Size = new System.Drawing.Size(20, 13);
            this.labelNllbApiUrl.TabIndex = 30;
            this.labelNllbApiUrl.Text = "Url";
            // 
            // linkLabelNllbApi
            // 
            this.linkLabelNllbApi.Location = new System.Drawing.Point(221, 16);
            this.linkLabelNllbApi.Name = "linkLabelNllbApi";
            this.linkLabelNllbApi.Size = new System.Drawing.Size(163, 21);
            this.linkLabelNllbApi.TabIndex = 24;
            this.linkLabelNllbApi.TabStop = true;
            this.linkLabelNllbApi.Text = "More info";
            this.linkLabelNllbApi.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.linkLabelNllbApi.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelNllbApi_LinkClicked);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(16, 106);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(0, 13);
            this.label5.TabIndex = 25;
            // 
            // Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1092, 574);
            this.Controls.Add(this.panelVideoPlayer);
            this.Controls.Add(this.panelShortcuts);
            this.Controls.Add(this.panelToolBar);
            this.Controls.Add(this.panelAutoTranslate);
            this.Controls.Add(this.labelUpdateFileTypeAssociationsStatus);
            this.Controls.Add(this.panelGeneral);
            this.Controls.Add(this.panelTools);
            this.Controls.Add(this.panelNetwork);
            this.Controls.Add(this.panelFont);
            this.Controls.Add(this.panelSubtitleFormats);
            this.Controls.Add(this.panelWaveform);
            this.Controls.Add(this.panelFileTypeAssociations);
            this.Controls.Add(this.panelSyntaxColoring);
            this.Controls.Add(this.listBoxSection);
            this.Controls.Add(this.buttonReset);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Settings";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormSettings_KeyDown);
            this.panelGeneral.ResumeLayout(false);
            this.groupBoxMiscellaneous.ResumeLayout(false);
            this.groupBoxMiscellaneous.PerformLayout();
            this.groupBoxGeneralRules.ResumeLayout(false);
            this.groupBoxGeneralRules.PerformLayout();
            this.panelSubtitleFormats.ResumeLayout(false);
            this.groupBoxSubtitleFormats.ResumeLayout(false);
            this.groupBoxSubtitleFormats.PerformLayout();
            this.groupBoxFavoriteSubtitleFormats.ResumeLayout(false);
            this.groupBoxFavoriteSubtitleFormats.PerformLayout();
            this.contextMenuStripFavoriteFormats.ResumeLayout(false);
            this.panelShortcuts.ResumeLayout(false);
            this.groupBoxShortcuts.ResumeLayout(false);
            this.groupBoxShortcuts.PerformLayout();
            this.contextMenuStripShortcuts.ResumeLayout(false);
            this.panelSyntaxColoring.ResumeLayout(false);
            this.groupBoxListViewSyntaxColoring.ResumeLayout(false);
            this.groupBoxListViewSyntaxColoring.PerformLayout();
            this.panelVideoPlayer.ResumeLayout(false);
            this.groupBoxMainWindowVideoControls.ResumeLayout(false);
            this.groupBoxMainWindowVideoControls.PerformLayout();
            this.groupBoxVideoPlayerDefault.ResumeLayout(false);
            this.groupBoxVideoPlayerDefault.PerformLayout();
            this.groupBoxMpvBorder.ResumeLayout(false);
            this.groupBoxMpvBorder.PerformLayout();
            this.groupBoxVideoEngine.ResumeLayout(false);
            this.groupBoxVideoEngine.PerformLayout();
            this.panelWaveform.ResumeLayout(false);
            this.groupBoxFfmpeg.ResumeLayout(false);
            this.groupBoxFfmpeg.PerformLayout();
            this.groupBoxSpectrogram.ResumeLayout(false);
            this.groupBoxSpectrogram.PerformLayout();
            this.groupBoxSpectrogramClean.ResumeLayout(false);
            this.groupBoxSpectrogramClean.PerformLayout();
            this.groupBoxWaveformAppearence.ResumeLayout(false);
            this.groupBoxWaveformAppearence.PerformLayout();
            this.panelTools.ResumeLayout(false);
            this.groupBoxToolsMisc.ResumeLayout(false);
            this.groupBoxToolsMisc.PerformLayout();
            this.groupBoxToolsAutoBr.ResumeLayout(false);
            this.groupBoxToolsAutoBr.PerformLayout();
            this.groupBoxSpellCheck.ResumeLayout(false);
            this.groupBoxSpellCheck.PerformLayout();
            this.groupBoxFixCommonErrors.ResumeLayout(false);
            this.groupBoxFixCommonErrors.PerformLayout();
            this.groupBoxToolsVisualSync.ResumeLayout(false);
            this.groupBoxToolsVisualSync.PerformLayout();
            this.groupBoxGoogleTranslate.ResumeLayout(false);
            this.groupBoxGoogleTranslate.PerformLayout();
            this.groupBoxBing.ResumeLayout(false);
            this.groupBoxBing.PerformLayout();
            this.panelToolBar.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBoxShowToolBarButtons.ResumeLayout(false);
            this.groupBoxShowToolBarButtons.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTBOpenVideo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxWebVttStyle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxEbuProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxWebVttProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxIttProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxToggleVideo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxToggleWaveform)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxAssaDraw)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxAssAttachments)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxAssProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxAssStyleManager)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTBToggleSourceView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTBBurnIn)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTBBeautifyTimeCodes)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTBNetflixQualityCheck)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxRemoveTextForHi)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTBFixCommonErrors)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTBHelp)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTBSettings)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTBSpellCheck)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTBVisualSync)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTBReplace)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTBFind)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTBSaveAs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTBSave)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTBOpen)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxFileNew)).EndInit();
            this.panelFont.ResumeLayout(false);
            this.groupBoxAppearance.ResumeLayout(false);
            this.groupBoxAppearance.PerformLayout();
            this.groupBoxGraphicsButtons.ResumeLayout(false);
            this.groupBoxGraphicsButtons.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreview3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreview2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreview1)).EndInit();
            this.groupBoxFontInUI.ResumeLayout(false);
            this.groupBoxFontGeneral.ResumeLayout(false);
            this.groupBoxFontGeneral.PerformLayout();
            this.groupBoxFontListViews.ResumeLayout(false);
            this.groupBoxFontListViews.PerformLayout();
            this.groupBoxFontTextBox.ResumeLayout(false);
            this.groupBoxFontTextBox.PerformLayout();
            this.groupBoxDarkTheme.ResumeLayout(false);
            this.groupBoxDarkTheme.PerformLayout();
            this.panelNetwork.ResumeLayout(false);
            this.groupBoxNetworkSession.ResumeLayout(false);
            this.groupBoxNetworkSession.PerformLayout();
            this.groupBoxProxySettings.ResumeLayout(false);
            this.groupBoxProxySettings.PerformLayout();
            this.groupBoxProxyAuthentication.ResumeLayout(false);
            this.groupBoxProxyAuthentication.PerformLayout();
            this.panelFileTypeAssociations.ResumeLayout(false);
            this.panelAutoTranslate.ResumeLayout(false);
            this.groupBoxAutoTranslatePapago.ResumeLayout(false);
            this.groupBoxAutoTranslatePapago.PerformLayout();
            this.groupBoxAutoTranslateChatGpt.ResumeLayout(false);
            this.groupBoxAutoTranslateChatGpt.PerformLayout();
            this.groupBoxDeepL.ResumeLayout(false);
            this.groupBoxDeepL.PerformLayout();
            this.groupBoxMyMemory.ResumeLayout(false);
            this.groupBoxMyMemory.PerformLayout();
            this.groupBoxLibreTranslate.ResumeLayout(false);
            this.groupBoxLibreTranslate.PerformLayout();
            this.groupBoxNllbServe.ResumeLayout(false);
            this.groupBoxNllbServe.PerformLayout();
            this.groupBoxNllbApi.ResumeLayout(false);
            this.groupBoxNllbApi.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private Nikse.SubtitleEdit.Controls.NikseListBox listBoxSection;
        private System.Windows.Forms.Panel panelGeneral;
        private System.Windows.Forms.GroupBox groupBoxMiscellaneous;
        private System.Windows.Forms.GroupBox groupBoxShowToolBarButtons;
        private System.Windows.Forms.PictureBox pictureBoxFileNew;
        private System.Windows.Forms.CheckBox checkBoxToolbarNew;
        private System.Windows.Forms.Label labelTBSpellCheck;
        private System.Windows.Forms.PictureBox pictureBoxTBSpellCheck;
        private System.Windows.Forms.CheckBox checkBoxTBSpellCheck;
        private System.Windows.Forms.Label labelTBVisualSync;
        private System.Windows.Forms.PictureBox pictureBoxTBVisualSync;
        private System.Windows.Forms.CheckBox checkBoxTBVisualSync;
        private System.Windows.Forms.Label labelTBReplace;
        private System.Windows.Forms.PictureBox pictureBoxTBReplace;
        private System.Windows.Forms.CheckBox checkBoxTBReplace;
        private System.Windows.Forms.Label labelTBFind;
        private System.Windows.Forms.PictureBox pictureBoxTBFind;
        private System.Windows.Forms.CheckBox checkBoxTBFind;
        private System.Windows.Forms.Label labelTBSaveAs;
        private System.Windows.Forms.PictureBox pictureBoxTBSaveAs;
        private System.Windows.Forms.CheckBox checkBoxTBSaveAs;
        private System.Windows.Forms.Label labelTBSave;
        private System.Windows.Forms.PictureBox pictureBoxTBSave;
        private System.Windows.Forms.CheckBox checkBoxTBSave;
        private System.Windows.Forms.Label labelTBOpen;
        private System.Windows.Forms.PictureBox pictureBoxTBOpen;
        private System.Windows.Forms.CheckBox checkBoxTBOpen;
        private System.Windows.Forms.Label labelTBNew;
        private System.Windows.Forms.Label labelTBHelp;
        private System.Windows.Forms.PictureBox pictureBoxTBHelp;
        private System.Windows.Forms.CheckBox checkBoxTBHelp;
        private System.Windows.Forms.Label labelTBSettings;
        private System.Windows.Forms.PictureBox pictureBoxTBSettings;
        private System.Windows.Forms.CheckBox checkBoxTBSettings;
        private System.Windows.Forms.Label labelDefaultFrameRate;
        private System.Windows.Forms.Label labelDefaultFileEncoding;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxFrameRate;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxEncoding;
        private System.Windows.Forms.CheckBox checkBoxRememberRecentFiles;
        private System.Windows.Forms.CheckBox checkBoxStartInSourceView;
        private System.Windows.Forms.CheckBox checkBoxReopenLastOpened;
        private System.Windows.Forms.Label labelSubMaxLen;
        private System.Windows.Forms.Panel panelVideoPlayer;
        private System.Windows.Forms.CheckBox checkBoxVideoPlayerShowStopButton;
        private System.Windows.Forms.GroupBox groupBoxVideoEngine;
        private System.Windows.Forms.Label labelVideoPlayerMPlayer;
        private System.Windows.Forms.Label labelDirectShowDescription;
        private System.Windows.Forms.Label labelMpcHcDescription;
        private System.Windows.Forms.RadioButton radioButtonVideoPlayerMPV;
        private System.Windows.Forms.RadioButton radioButtonVideoPlayerDirectShow;
        private System.Windows.Forms.RadioButton radioButtonVideoPlayerMpcHc;
        private System.Windows.Forms.CheckBox checkBoxRememberWindowPosition;
        private Nikse.SubtitleEdit.Controls.NikseTextBox textBoxShowLineBreaksAs;
        private System.Windows.Forms.Label labelShowLineBreaksAs;
        private System.Windows.Forms.Panel panelNetwork;
        private System.Windows.Forms.GroupBox groupBoxProxySettings;
        private System.Windows.Forms.Label labelProxyPassword;
        private Nikse.SubtitleEdit.Controls.NikseTextBox textBoxProxyAddress;
        private Nikse.SubtitleEdit.Controls.NikseTextBox textBoxProxyUserName;
        private Nikse.SubtitleEdit.Controls.NikseTextBox textBoxProxyPassword;
        private System.Windows.Forms.Label labelProxyAddress;
        private System.Windows.Forms.Label labelProxyUserName;
        private System.Windows.Forms.Panel panelTools;
        private System.Windows.Forms.GroupBox groupBoxToolsVisualSync;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxToolsVerifySeconds;
        private System.Windows.Forms.Label labelVerifyButton;
        private System.Windows.Forms.Label labelToolsEndScene;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxToolsEndSceneIndex;
        private System.Windows.Forms.Label labelToolsStartScene;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxToolsStartSceneIndex;
        private System.Windows.Forms.GroupBox groupBoxFixCommonErrors;
        private System.Windows.Forms.Label labelToolsMusicSymbol;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxListViewDoubleClickEvent;
        private System.Windows.Forms.Label labelListViewDoubleClickEvent;
        private System.Windows.Forms.Label labelStatus;
        private Nikse.SubtitleEdit.Controls.NikseTextBox textBoxMusicSymbolsToReplace;
        private System.Windows.Forms.Label labelToolsMusicSymbolsToReplace;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxToolsMusicSymbol;
        private System.Windows.Forms.GroupBox groupBoxSpellCheck;
        private System.Windows.Forms.CheckBox checkBoxSpellCheckAutoChangeNames;
        private System.Windows.Forms.GroupBox groupBoxProxyAuthentication;
        private Nikse.SubtitleEdit.Controls.NikseTextBox textBoxProxyDomain;
        private System.Windows.Forms.Label labelProxyDomain;
        private System.Windows.Forms.CheckBox checkBoxAutoDetectAnsiEncoding;
        private System.Windows.Forms.Label labelAutoDetectAnsiEncoding;
        private System.Windows.Forms.Label labelVideoPlayerVLC;
        private System.Windows.Forms.RadioButton radioButtonVideoPlayerVLC;
        private System.Windows.Forms.CheckBox checkBoxRemoveBlankLinesWhenOpening;
        private System.Windows.Forms.GroupBox groupBoxMainWindowVideoControls;
        private System.Windows.Forms.GroupBox groupBoxVideoPlayerDefault;
        private Nikse.SubtitleEdit.Controls.NikseTextBox textBoxCustomSearchUrl1;
        private System.Windows.Forms.Label labelCustomSearch;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxCustomSearch1;
        private System.Windows.Forms.Panel panelWaveform;
        private System.Windows.Forms.GroupBox groupBoxWaveformAppearence;
        private System.Windows.Forms.Panel panelWaveformBackgroundColor;
        private System.Windows.Forms.Button buttonWaveformBackgroundColor;
        private System.Windows.Forms.Panel panelWaveformColor;
        private System.Windows.Forms.Button buttonWaveformColor;
        private System.Windows.Forms.Panel panelWaveformSelectedColor;
        private System.Windows.Forms.Button buttonWaveformSelectedColor;
        private System.Windows.Forms.CheckBox checkBoxWaveformShowGrid;
        private System.Windows.Forms.Panel panelWaveformGridColor;
        private System.Windows.Forms.Button buttonWaveformGridColor;
        private System.Windows.Forms.Panel panelWaveformTextColor;
        private System.Windows.Forms.Button buttonWaveformTextColor;
        private System.Windows.Forms.GroupBox groupBoxSpectrogramClean;
        private System.Windows.Forms.Button buttonWaveformsFolderEmpty;
        private System.Windows.Forms.Label labelWaveformsFolderInfo;
        private System.Windows.Forms.CheckBox checkBoxRememberSelectedLine;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxAutoBackup;
        private System.Windows.Forms.Label labelAutoBackup;
        private System.Windows.Forms.Panel panelToolBar;
        private System.Windows.Forms.CheckBox checkBoxShowFrameRate;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxSpellChecker;
        private System.Windows.Forms.Label labelSpellChecker;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox checkBoxAllowEditOfOriginalSubtitle;
        private System.Windows.Forms.Label labelMergeShortLines;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxMergeShortLineLength;
        private System.Windows.Forms.CheckBox checkBoxAutoWrapWhileTyping;
        private System.Windows.Forms.CheckBox checkBoxFixCommonOcrErrorsUsingHardcodedRules;
        private System.Windows.Forms.Label labelVideoPlayerPreviewFontName;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxVideoPlayerPreviewFontName;
        private System.Windows.Forms.Label labelVideoPlayerPreviewFontSize;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxlVideoPlayerPreviewFontSize;
        private System.Windows.Forms.GroupBox groupBoxShortcuts;
        private System.Windows.Forms.Button buttonUpdateShortcut;
        private System.Windows.Forms.TreeView treeViewShortcuts;
        private System.Windows.Forms.Label labelShortcut;
        private System.Windows.Forms.Label labelShortcutKey;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxShortcutKey;
        private System.Windows.Forms.CheckBox checkBoxShortcutsShift;
        private System.Windows.Forms.CheckBox checkBoxShortcutsAlt;
        private System.Windows.Forms.CheckBox checkBoxShortcutsControl;
        private System.Windows.Forms.CheckBox checkBoxPromptDeleteLines;
        private System.Windows.Forms.GroupBox groupBoxSpectrogram;
        private System.Windows.Forms.CheckBox checkBoxGenerateSpectrogram;
        private System.Windows.Forms.Label labelSpectrogramAppearance;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxSpectrogramAppearance;
        private System.Windows.Forms.CheckBox checkBoxReverseMouseWheelScrollDirection;
        private System.Windows.Forms.Label labelMaxDuration;
        private System.Windows.Forms.Label labelMinDuration;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownDurationMax;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownDurationMin;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxTimeCodeMode;
        private System.Windows.Forms.Label labelTimeCodeMode;
        private System.Windows.Forms.Panel panelSyntaxColoring;
        private System.Windows.Forms.GroupBox groupBoxListViewSyntaxColoring;
        private System.Windows.Forms.CheckBox checkBoxSyntaxOverlap;
        private System.Windows.Forms.CheckBox checkBoxSyntaxColorTextTooLong;
        private System.Windows.Forms.CheckBox checkBoxSyntaxColorDurationTooLarge;
        private System.Windows.Forms.CheckBox checkBoxSyntaxColorDurationTooSmall;
        private System.Windows.Forms.CheckBox checkBoxSyntaxColorTextMoreThanTwoLines;
        private System.Windows.Forms.Button buttonListViewSyntaxColorError;
        private System.Windows.Forms.Panel panelListViewSyntaxColorError;
        private System.Windows.Forms.Label labelMaxCharsPerSecond;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownMaxCharsSec;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownSubtitleLineMaximumLength;
        private System.Windows.Forms.CheckBox checkBoxVideoPlayerShowFullscreenButton;
        private System.Windows.Forms.CheckBox checkBoxVideoPlayerShowMuteButton;
        private System.Windows.Forms.Label labelCustomSearch1;
        private System.Windows.Forms.Label labelCustomSearch2;
        private Nikse.SubtitleEdit.Controls.NikseTextBox textBoxCustomSearchUrl2;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxCustomSearch2;
        private System.Windows.Forms.Label labelCustomSearch5;
        private Nikse.SubtitleEdit.Controls.NikseTextBox textBoxCustomSearchUrl5;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxCustomSearch5;
        private System.Windows.Forms.Label labelCustomSearch4;
        private Nikse.SubtitleEdit.Controls.NikseTextBox textBoxCustomSearchUrl4;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxCustomSearch4;
        private System.Windows.Forms.Label labelCustomSearch3;
        private Nikse.SubtitleEdit.Controls.NikseTextBox textBoxCustomSearchUrl3;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxCustomSearch3;
        private System.Windows.Forms.CheckBox checkBoxAllowOverlap;
        private System.Windows.Forms.Label labelWaveformBorderHitMs2;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownWaveformBorderHitMs;
        private System.Windows.Forms.Label labelWaveformBorderHitMs1;
        private System.Windows.Forms.CheckBox checkBoxSpellCheckOneLetterWords;
        private System.Windows.Forms.CheckBox checkBoxTreatINQuoteAsING;
        private System.Windows.Forms.GroupBox groupBoxFfmpeg;
        private System.Windows.Forms.Button buttonBrowseToFFmpeg;
        private Nikse.SubtitleEdit.Controls.NikseTextBox textBoxFFmpegPath;
        private System.Windows.Forms.Label labelFFmpegPath;
        private System.Windows.Forms.CheckBox checkBoxUseFFmpeg;
        private System.Windows.Forms.OpenFileDialog openFileDialogFFmpeg;
        private System.Windows.Forms.CheckBox checkBoxWaveformHoverFocus;
        private System.Windows.Forms.CheckBox checkBoxListViewMouseEnterFocus;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownMinGapMs;
        private System.Windows.Forms.Label labelMinGapMs;
        private System.Windows.Forms.Label labelTBFixCommonErrors;
        private System.Windows.Forms.PictureBox pictureBoxTBFixCommonErrors;
        private System.Windows.Forms.CheckBox checkBoxTBFixCommonErrors;
        private System.Windows.Forms.CheckBox checkBoxFixShortDisplayTimesAllowMoveStartTime;
        private System.Windows.Forms.Button buttonVlcPathBrowse;
        private Nikse.SubtitleEdit.Controls.NikseTextBox textBoxVlcPath;
        private System.Windows.Forms.Label labelVlcPath;
        private System.Windows.Forms.GroupBox groupBoxToolsAutoBr;
        private System.Windows.Forms.CheckBox checkBoxUseDoNotBreakAfterList;
        private System.Windows.Forms.Button buttonEditDoNotBreakAfterList;
        private System.Windows.Forms.CheckBox checkBoxCheckForUpdates;
        private System.Windows.Forms.Label labelPlatform;
        private System.Windows.Forms.CheckBox checkBoxVideoPlayerPreviewFontBold;
        private System.Windows.Forms.Label labelWaveformTextSize;
        private System.Windows.Forms.CheckBox checkBoxWaveformTextBold;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxWaveformTextSize;
        private System.Windows.Forms.GroupBox groupBoxNetworkSession;
        private System.Windows.Forms.Button buttonNetworkSessionNewMessageSound;
        private Nikse.SubtitleEdit.Controls.NikseTextBox textBoxNetworkSessionNewMessageSound;
        private System.Windows.Forms.Label labelNetworkSessionNewMessageSound;
        private System.Windows.Forms.CheckBox checkBoxFceSkipStep1;
        private System.Windows.Forms.Button buttonMpvSettings;
        private System.Windows.Forms.Label labelMpvSettings;
        private System.Windows.Forms.LinkLabel linkLabelBingSubscribe;
        private System.Windows.Forms.Label labelUserBingApiId;
        private System.Windows.Forms.GroupBox groupBoxBing;
        private System.Windows.Forms.Label labelBingApiKey;
        private Nikse.SubtitleEdit.Controls.NikseTextBox textBoxBingClientSecret;
        private System.Windows.Forms.Label label1;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxAutoBackupDeleteAfter;
        private System.Windows.Forms.Label labelAutoBackupDeleteAfter;
        private System.Windows.Forms.Label labelTBRemoveTextForHi;
        private System.Windows.Forms.PictureBox pictureBoxRemoveTextForHi;
        private System.Windows.Forms.CheckBox checkBoxTBRemoveTextForHi;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownMaxNumberOfLines;
        private System.Windows.Forms.Label labelMaxLines;
        private System.Windows.Forms.Label labelTBNetflixQualityCheck;
        private System.Windows.Forms.PictureBox pictureBoxTBNetflixQualityCheck;
        private System.Windows.Forms.CheckBox checkBoxTBNetflixQualityCheck;
        private System.Windows.Forms.CheckBox checkBoxWaveformSetVideoPosMoveStartEnd;
        private System.Windows.Forms.CheckBox checkBoxWaveformShowWpm;
        private System.Windows.Forms.CheckBox checkBoxWaveformShowCps;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownMaxWordsMin;
        private System.Windows.Forms.Label labelMaxWordsPerMin;
        private System.Windows.Forms.CheckBox checkBoxMpvHandlesPreviewText;
        private System.Windows.Forms.CheckBox checkBoxVideoAutoOpen;
        private System.Windows.Forms.Button buttonClearShortcut;
        private System.Windows.Forms.CheckBox checkBoxAllowVolumeBoost;
        private System.Windows.Forms.Panel panelFont;
        private System.Windows.Forms.GroupBox groupBoxAppearance;
        private System.Windows.Forms.CheckBox checkBoxSubtitleCenter;
        private System.Windows.Forms.Panel panelSubtitleFontColor;
        private System.Windows.Forms.Panel panelSubtitleBackgroundColor;
        private System.Windows.Forms.Label labelSubtitleFontBackgroundColor;
        private System.Windows.Forms.Label labelSubtitleFontColor;
        private System.Windows.Forms.Label labelSubtitleFontSize;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxSubtitleFont;
        private System.Windows.Forms.CheckBox checkBoxSubtitleFontBold;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxSubtitleFontSize;
        private System.Windows.Forms.Label labelSubtitleFont;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxSaveAsFileNameFrom;
        private System.Windows.Forms.Label labelSaveAsFileNameFrom;
        private System.Windows.Forms.GroupBox groupBoxGeneralRules;
        private System.Windows.Forms.GroupBox groupBoxFontTextBox;
        private System.Windows.Forms.GroupBox groupBoxFontListViews;
        private System.Windows.Forms.Label labelSubtitleListViewFontSize;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxSubtitleListViewFontSize;
        private System.Windows.Forms.CheckBox checkBoxSubtitleListViewFontBold;
        private System.Windows.Forms.GroupBox groupBoxFontGeneral;
        private System.Windows.Forms.Label labelFontNote;
        private System.Windows.Forms.Button buttonDownloadFfmpeg;
        private System.Windows.Forms.Button buttonShortcutsClear;
        private System.Windows.Forms.Label labelShortcutsSearch;
        private Nikse.SubtitleEdit.Controls.NikseTextBox textBoxShortcutSearch;
        private System.Windows.Forms.GroupBox groupBoxGoogleTranslate;
        private System.Windows.Forms.Label labelGoogleTranslateApiKey;
        private Nikse.SubtitleEdit.Controls.NikseTextBox textBoxGoogleTransleApiKey;
        private System.Windows.Forms.LinkLabel linkLabelGoogleTranslateSignUp;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox checkBoxAutoSave;
        private System.Windows.Forms.CheckBox checkBoxUseAlwaysToFile;
        private System.Windows.Forms.Label labelOptimalCharsPerSecond;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownOptimalCharsSec;
        private System.Windows.Forms.Button buttonEditProfile;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxRulesProfileName;
        private System.Windows.Forms.Label labelRulesProfileName;
        private System.Windows.Forms.Label labelBingTokenEndpoint;
        private System.Windows.Forms.CheckBox checkBoxToolsBreakEarlyComma;
        private System.Windows.Forms.CheckBox checkBoxToolsBreakEarlyDash;
        private System.Windows.Forms.CheckBox checkBoxToolsBreakEarlyLineEnding;
        private System.Windows.Forms.CheckBox checkBoxToolsBreakByPixelWidth;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownToolsBreakPreferBottomHeavy;
        private System.Windows.Forms.CheckBox checkBoxToolsBreakPreferBottomHeavy;
        private System.Windows.Forms.Label labelToolsBreakBottomHeavyPercent;
        private System.Windows.Forms.CheckBox checkBoxSyntaxColorGapTooSmall;
        private System.Windows.Forms.Button buttonReset;
        private System.Windows.Forms.Label labelDialogStyle;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxDialogStyle;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripShortcuts;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemShortcutsCollapse;
        private System.Windows.Forms.CheckBox checkBoxWaveformSnapToShotChanges;
        private System.Windows.Forms.CheckBox checkBoxWaveformSingleClickSelect;
        private System.Windows.Forms.Label labelSplitBehavior;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxSplitBehavior;
        private System.Windows.Forms.Button buttonLineWidthSettings;
        private System.Windows.Forms.CheckBox checkBoxSyntaxColorTextTooWide;
        private System.Windows.Forms.Label labelContinuationStyle;
        private System.Windows.Forms.Button buttonFixContinuationStyleSettings;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxContinuationStyle;
        private System.Windows.Forms.ToolTip toolTipContinuationPreview;
        private System.Windows.Forms.Panel panelWaveformCursorColor;
        private System.Windows.Forms.Button buttonWaveformCursorColor;
        private System.Windows.Forms.Button buttonGapChoose;
        private System.Windows.Forms.CheckBox checkBoxSpellCheckAutoChangeNamesViaSuggestions;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxBoxBingTokenEndpoint;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem importShortcutsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportShortcutsToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Panel panelTextBoxAssColor;
        private System.Windows.Forms.Button buttonTextBoxAssColor;
        private System.Windows.Forms.Panel panelTextBoxHtmlColor;
        private System.Windows.Forms.Button buttonTextBoxHtmlColor;
        private System.Windows.Forms.CheckBox checkBoxSubtitleTextBoxSyntaxColor;
        private System.Windows.Forms.GroupBox groupBoxDarkTheme;
        private System.Windows.Forms.CheckBox checkBoxDarkThemeEnabled;
        private System.Windows.Forms.Panel panelDarkThemeBackColor;
        private System.Windows.Forms.Button buttonDarkThemeBackColor;
        private System.Windows.Forms.Panel panelDarkThemeColor;
        private System.Windows.Forms.Button buttonDarkThemeColor;
        private System.Windows.Forms.Panel panelSubtitleFormats;
        private System.Windows.Forms.GroupBox groupBoxSubtitleFormats;
        private System.Windows.Forms.Label labelDefaultSubtitleFormat;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxSubtitleFormats;
        private System.Windows.Forms.Label labelDefaultSaveAsFormat;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxSubtitleSaveAsFormats;
        private System.Windows.Forms.GroupBox groupBoxFavoriteSubtitleFormats;
        private System.Windows.Forms.Label labelFavoriteFormats;
        private Nikse.SubtitleEdit.Controls.NikseListBox listBoxFavoriteSubtitleFormats;
        private System.Windows.Forms.Button buttonMoveToFavoriteFormats;
        private System.Windows.Forms.Button buttonRemoveFromFavoriteFormats;
        private System.Windows.Forms.Label labelFormats;
        private System.Windows.Forms.Label labelFormatsSearch;
        private Nikse.SubtitleEdit.Controls.NikseTextBox textBoxFormatsSearch;
        private System.Windows.Forms.Button buttonFormatsSearchClear;
        private Nikse.SubtitleEdit.Controls.NikseListBox listBoxSubtitleFormats;
        private System.Windows.Forms.Label labelFavoriteSubtitleFormatsNote;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripFavoriteFormats;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator;
        private System.Windows.Forms.ToolStripMenuItem moveUpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveDownToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveToTopToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveToBottomToolStripMenuItem;
        private System.Windows.Forms.Panel panelShortcuts;
        private System.Windows.Forms.CheckBox checkBoxLiveSpellCheck;
        private System.Windows.Forms.CheckBox checkBoxDarkThemeShowListViewGridLines;
        private System.Windows.Forms.GroupBox groupBoxFontInUI;
        private System.Windows.Forms.Label labelTBBeautifyTimeCodes;
        private System.Windows.Forms.PictureBox pictureBoxTBBeautifyTimeCodes;
        private System.Windows.Forms.CheckBox checkBoxTBBeautifyTimeCodes;
        private System.Windows.Forms.GroupBox groupBoxToolsMisc;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxBDOpensIn;
        private System.Windows.Forms.Label labelBDOpensIn;
        private System.Windows.Forms.CheckBox checkBoxShortcutsAllowLetterOrNumberInTextBox;
        private System.Windows.Forms.CheckBox checkBoxWaveformAutoGen;
        private System.Windows.Forms.Panel panelFileTypeAssociations;
        private System.Windows.Forms.ListView listViewFileTypeAssociations;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ImageList imageListFileTypeAssociations;
        private System.Windows.Forms.Button buttonUpdateFileTypeAssociations;
        private System.Windows.Forms.Label labelUpdateFileTypeAssociationsStatus;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.Label labelTBBurnIn;
        private System.Windows.Forms.PictureBox pictureBoxTBBurnIn;
        private System.Windows.Forms.CheckBox checkBoxTBBurnIn;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxCpsLineLenCalc;
        private System.Windows.Forms.Label labelCpsLineLenCalc;
        private System.Windows.Forms.CheckBox checkBoxUseWordSplitList;
        private System.Windows.Forms.ToolTip toolTipDialogStylePreview;
        private System.Windows.Forms.Button buttonEditCustomContinuationStyle;
        private System.Windows.Forms.Label labelProxyAuthType;
        private System.Windows.Forms.CheckBox checkBoxProxyUseDefaultCredentials;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxProxyAuthType;
        private System.Windows.Forms.CheckBox checkBoxFfmpegUseCenterChannel;
        private System.Windows.Forms.Label labelTBToggleSourceView;
        private System.Windows.Forms.PictureBox pictureBoxTBToggleSourceView;
        private System.Windows.Forms.CheckBox checkBoxTBToggleSourceView;
        private System.Windows.Forms.Button buttonTranslationAutoSuffix;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxTranslationAutoSuffix;
        private System.Windows.Forms.Label labelTranslationAutoSuffix;
        private System.Windows.Forms.CheckBox checkBoxUseWordSplitListAvoidPropercase;
        private System.Windows.Forms.ToolStripMenuItem exportAsHtmlToolStripMenuItem;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxCustomToggleEnd;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxCustomToggleStart;
        private System.Windows.Forms.Label labelShortcutCustomToggle;
        private System.Windows.Forms.GroupBox groupBoxMpvBorder;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxOpaqueBoxStyle;
        private System.Windows.Forms.RadioButton radioButtonMpvOpaqueBox;
        private System.Windows.Forms.RadioButton radioButtonMpvOutline;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownMpvShadowWidth;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownMpvOutline;
        private System.Windows.Forms.Label labelMpvShadow;
        private System.Windows.Forms.Panel panelMpvBackColor;
        private System.Windows.Forms.Button buttonMpvBackColor;
        private System.Windows.Forms.Panel panelMpvOutlineColor;
        private System.Windows.Forms.Button buttonMpvOutlineColor;
        private System.Windows.Forms.Panel panelMpvPrimaryColor;
        private System.Windows.Forms.Button buttonMpvPrimaryColor;
        private System.Windows.Forms.Button buttonEditShotChangesProfile;
        private System.Windows.Forms.PictureBox pictureBoxAssStyleManager;
        private System.Windows.Forms.PictureBox pictureBoxAssAttachments;
        private System.Windows.Forms.PictureBox pictureBoxAssProperties;
        private System.Windows.Forms.PictureBox pictureBoxAssaDraw;
        private System.Windows.Forms.PictureBox pictureBoxToggleVideo;
        private System.Windows.Forms.PictureBox pictureBoxToggleWaveform;
        private System.Windows.Forms.PictureBox pictureBoxEbuProperties;
        private System.Windows.Forms.PictureBox pictureBoxWebVttProperties;
        private System.Windows.Forms.PictureBox pictureBoxIttProperties;
        private System.Windows.Forms.PictureBox pictureBoxWebVttStyle;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownMarginVertical;
        private System.Windows.Forms.Label labelMarginVertical;
        private System.Windows.Forms.GroupBox groupBoxGraphicsButtons;
        private System.Windows.Forms.PictureBox pictureBoxPreview3;
        private System.Windows.Forms.PictureBox pictureBoxPreview2;
        private System.Windows.Forms.PictureBox pictureBoxPreview1;
        private System.Windows.Forms.Label labelToolbarIconTheme;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxToolbarIconTheme;
        private System.Windows.Forms.Button buttonDefaultLanguages;
        private System.Windows.Forms.Label labelDefaultLanguages;
        private System.Windows.Forms.Label labelDefaultLanguagesList;
        private System.Windows.Forms.Panel panelAutoTranslate;
        private System.Windows.Forms.GroupBox groupBoxNllbApi;
        private System.Windows.Forms.Label labelNllbApiUrl;
        private System.Windows.Forms.LinkLabel linkLabelNllbApi;
        private System.Windows.Forms.Label label5;
        private Controls.NikseTextBox nikseTextBoxNllbApiUrl;
        private System.Windows.Forms.GroupBox groupBoxNllbServe;
        private Controls.NikseTextBox nikseTextBoxNllbServeUrl;
        private System.Windows.Forms.Label labelNllbServeUrl;
        private System.Windows.Forms.LinkLabel linkLabelNllbServe;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBoxLibreTranslate;
        private Controls.NikseTextBox nikseTextBoxLibreTranslateUrl;
        private System.Windows.Forms.Label labelLibreUrl;
        private System.Windows.Forms.LinkLabel linkLabelLibreTranslateApi;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label labelNllbServeModel;
        private Controls.NikseTextBox nikseTextBoxNllbServeModel;
        private Controls.NikseTextBox nikseTextBoxLibreTranslateApiKey;
        private System.Windows.Forms.Label labelLibreApiKey;
        private System.Windows.Forms.GroupBox groupBoxMyMemory;
        private Controls.NikseTextBox nikseTextBoxMyMemoryApiKey;
        private System.Windows.Forms.Label labelMyMemoryApiKey;
        private System.Windows.Forms.LinkLabel linkLabelMyMemoryTranslate;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.GroupBox groupBoxDeepL;
        private Controls.NikseTextBox nikseTextBoxDeepLApiKey;
        private System.Windows.Forms.Label labelDeepLApiKey;
        private Controls.NikseTextBox nikseTextBoxDeepLUrl;
        private System.Windows.Forms.Label labelDeepLUrl;
        private System.Windows.Forms.LinkLabel linkLabelMoreInfoDeepl;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.GroupBox groupBoxAutoTranslatePapago;
        private Controls.NikseTextBox nikseTextBoxPapagoClientSecret;
        private System.Windows.Forms.Label labelSecretPapago;
        private Controls.NikseTextBox nikseTextBoxPapagoClientId;
        private System.Windows.Forms.Label labelApiKeyPapago;
        private System.Windows.Forms.LinkLabel linkLabelPapago;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.GroupBox groupBoxAutoTranslateChatGpt;
        private System.Windows.Forms.Label labelChatGptModel;
        private Controls.NikseTextBox nikseTextBoxChatGptApiKey;
        private System.Windows.Forms.Label labelApiKeyChatGpt;
        private Controls.NikseTextBox nikseTextBoxChatGptUrl;
        private System.Windows.Forms.Label labelUrlChatGpt;
        private System.Windows.Forms.LinkLabel linkLabelMoreInfoChatGpt;
        private System.Windows.Forms.Label label10;
        private Controls.NikseComboBox nikseComboBoxChatGptModel;
        private System.Windows.Forms.Label labelShortcutsFilter;
        private Controls.NikseComboBox nikseComboBoxShortcutsFilter;
        private System.Windows.Forms.Label labelTBOpenVideo;
        private System.Windows.Forms.PictureBox pictureBoxTBOpenVideo;
        private System.Windows.Forms.CheckBox checkBoxTBOpenVideo;
    }
}