namespace Nikse.SubtitleEdit.Forms
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
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.tabControlSettings = new System.Windows.Forms.TabControl();
            this.tabPageGeneral = new System.Windows.Forms.TabPage();
            this.groupBoxMiscellaneous = new System.Windows.Forms.GroupBox();
            this.checkBoxAutoSave = new System.Windows.Forms.CheckBox();
            this.comboBoxSaveAsFileNameFrom = new System.Windows.Forms.ComboBox();
            this.labelSaveAsFileNameFrom = new System.Windows.Forms.Label();
            this.groupBoxGeneralRules = new System.Windows.Forms.GroupBox();
            this.checkBoxCpsIncludeWhiteSpace = new System.Windows.Forms.CheckBox();
            this.buttonEditProfile = new System.Windows.Forms.Button();
            this.comboBoxRulesProfileName = new System.Windows.Forms.ComboBox();
            this.labelRulesProfileName = new System.Windows.Forms.Label();
            this.labelOptimalCharsPerSecond = new System.Windows.Forms.Label();
            this.numericUpDownOptimalCharsSec = new System.Windows.Forms.NumericUpDown();
            this.labelSubMaxLen = new System.Windows.Forms.Label();
            this.numericUpDownMaxWordsMin = new System.Windows.Forms.NumericUpDown();
            this.labelMergeShortLines = new System.Windows.Forms.Label();
            this.labelMaxWordsPerMin = new System.Windows.Forms.Label();
            this.labelMinDuration = new System.Windows.Forms.Label();
            this.numericUpDownMaxNumberOfLines = new System.Windows.Forms.NumericUpDown();
            this.labelMaxDuration = new System.Windows.Forms.Label();
            this.labelMaxLines = new System.Windows.Forms.Label();
            this.numericUpDownDurationMin = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownDurationMax = new System.Windows.Forms.NumericUpDown();
            this.comboBoxMergeShortLineLength = new System.Windows.Forms.ComboBox();
            this.labelMaxCharsPerSecond = new System.Windows.Forms.Label();
            this.numericUpDownMinGapMs = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownMaxCharsSec = new System.Windows.Forms.NumericUpDown();
            this.labelMinGapMs = new System.Windows.Forms.Label();
            this.numericUpDownSubtitleLineMaximumLength = new System.Windows.Forms.NumericUpDown();
            this.comboBoxAutoBackupDeleteAfter = new System.Windows.Forms.ComboBox();
            this.labelAutoBackupDeleteAfter = new System.Windows.Forms.Label();
            this.checkBoxCheckForUpdates = new System.Windows.Forms.CheckBox();
            this.labelSpellChecker = new System.Windows.Forms.Label();
            this.comboBoxTimeCodeMode = new System.Windows.Forms.ComboBox();
            this.labelTimeCodeMode = new System.Windows.Forms.Label();
            this.comboBoxEncoding = new System.Windows.Forms.ComboBox();
            this.checkBoxAutoDetectAnsiEncoding = new System.Windows.Forms.CheckBox();
            this.textBoxShowLineBreaksAs = new System.Windows.Forms.TextBox();
            this.checkBoxAutoWrapWhileTyping = new System.Windows.Forms.CheckBox();
            this.checkBoxPromptDeleteLines = new System.Windows.Forms.CheckBox();
            this.checkBoxAllowEditOfOriginalSubtitle = new System.Windows.Forms.CheckBox();
            this.comboBoxSpellChecker = new System.Windows.Forms.ComboBox();
            this.comboBoxAutoBackup = new System.Windows.Forms.ComboBox();
            this.labelAutoBackup = new System.Windows.Forms.Label();
            this.checkBoxRememberSelectedLine = new System.Windows.Forms.CheckBox();
            this.checkBoxRemoveBlankLinesWhenOpening = new System.Windows.Forms.CheckBox();
            this.labelAutoDetectAnsiEncoding = new System.Windows.Forms.Label();
            this.comboBoxListViewDoubleClickEvent = new System.Windows.Forms.ComboBox();
            this.labelListViewDoubleClickEvent = new System.Windows.Forms.Label();
            this.labelShowLineBreaksAs = new System.Windows.Forms.Label();
            this.checkBoxRememberWindowPosition = new System.Windows.Forms.CheckBox();
            this.checkBoxStartInSourceView = new System.Windows.Forms.CheckBox();
            this.checkBoxReopenLastOpened = new System.Windows.Forms.CheckBox();
            this.checkBoxRememberRecentFiles = new System.Windows.Forms.CheckBox();
            this.labelDefaultFileEncoding = new System.Windows.Forms.Label();
            this.comboBoxFrameRate = new System.Windows.Forms.ComboBox();
            this.labelDefaultFrameRate = new System.Windows.Forms.Label();
            this.tabPageShortcuts = new System.Windows.Forms.TabPage();
            this.groupBoxShortcuts = new System.Windows.Forms.GroupBox();
            this.buttonShortcutsClear = new System.Windows.Forms.Button();
            this.labelShortcutsSearch = new System.Windows.Forms.Label();
            this.textBoxShortcutSearch = new System.Windows.Forms.TextBox();
            this.buttonClearShortcut = new System.Windows.Forms.Button();
            this.comboBoxShortcutKey = new System.Windows.Forms.ComboBox();
            this.labelShortcutKey = new System.Windows.Forms.Label();
            this.checkBoxShortcutsShift = new System.Windows.Forms.CheckBox();
            this.checkBoxShortcutsAlt = new System.Windows.Forms.CheckBox();
            this.checkBoxShortcutsControl = new System.Windows.Forms.CheckBox();
            this.buttonUpdateShortcut = new System.Windows.Forms.Button();
            this.treeViewShortcuts = new System.Windows.Forms.TreeView();
            this.labelShortcut = new System.Windows.Forms.Label();
            this.tabPageSyntaxColoring = new System.Windows.Forms.TabPage();
            this.groupBoxListViewSyntaxColoring = new System.Windows.Forms.GroupBox();
            this.checkBoxSyntaxColorGapTooSmall = new System.Windows.Forms.CheckBox();
            this.checkBoxSyntaxColorTextMoreThanTwoLines = new System.Windows.Forms.CheckBox();
            this.checkBoxSyntaxOverlap = new System.Windows.Forms.CheckBox();
            this.checkBoxSyntaxColorDurationTooSmall = new System.Windows.Forms.CheckBox();
            this.buttonListViewSyntaxColorError = new System.Windows.Forms.Button();
            this.checkBoxSyntaxColorTextTooLong = new System.Windows.Forms.CheckBox();
            this.checkBoxSyntaxColorDurationTooLarge = new System.Windows.Forms.CheckBox();
            this.panelListViewSyntaxColorError = new System.Windows.Forms.Panel();
            this.tabPageVideoPlayer = new System.Windows.Forms.TabPage();
            this.groupBoxMainWindowVideoControls = new System.Windows.Forms.GroupBox();
            this.labelCustomSearch5 = new System.Windows.Forms.Label();
            this.textBoxCustomSearchUrl5 = new System.Windows.Forms.TextBox();
            this.comboBoxCustomSearch5 = new System.Windows.Forms.ComboBox();
            this.labelCustomSearch4 = new System.Windows.Forms.Label();
            this.textBoxCustomSearchUrl4 = new System.Windows.Forms.TextBox();
            this.comboBoxCustomSearch4 = new System.Windows.Forms.ComboBox();
            this.labelCustomSearch3 = new System.Windows.Forms.Label();
            this.textBoxCustomSearchUrl3 = new System.Windows.Forms.TextBox();
            this.comboBoxCustomSearch3 = new System.Windows.Forms.ComboBox();
            this.labelCustomSearch2 = new System.Windows.Forms.Label();
            this.textBoxCustomSearchUrl2 = new System.Windows.Forms.TextBox();
            this.comboBoxCustomSearch2 = new System.Windows.Forms.ComboBox();
            this.labelCustomSearch1 = new System.Windows.Forms.Label();
            this.textBoxCustomSearchUrl1 = new System.Windows.Forms.TextBox();
            this.labelCustomSearch = new System.Windows.Forms.Label();
            this.comboBoxCustomSearch1 = new System.Windows.Forms.ComboBox();
            this.groupBoxVideoPlayerDefault = new System.Windows.Forms.GroupBox();
            this.checkBoxAllowVolumeBoost = new System.Windows.Forms.CheckBox();
            this.checkBoxVideoAutoOpen = new System.Windows.Forms.CheckBox();
            this.checkBoxVideoPlayerPreviewFontBold = new System.Windows.Forms.CheckBox();
            this.checkBoxVideoPlayerShowFullscreenButton = new System.Windows.Forms.CheckBox();
            this.checkBoxVideoPlayerShowMuteButton = new System.Windows.Forms.CheckBox();
            this.labelVideoPlayerPreviewFontSize = new System.Windows.Forms.Label();
            this.comboBoxlVideoPlayerPreviewFontSize = new System.Windows.Forms.ComboBox();
            this.checkBoxVideoPlayerShowStopButton = new System.Windows.Forms.CheckBox();
            this.groupBoxVideoEngine = new System.Windows.Forms.GroupBox();
            this.checkBoxMpvHandlesPreviewText = new System.Windows.Forms.CheckBox();
            this.labelMpvSettings = new System.Windows.Forms.Label();
            this.buttonMpvSettings = new System.Windows.Forms.Button();
            this.labelPlatform = new System.Windows.Forms.Label();
            this.buttonVlcPathBrowse = new System.Windows.Forms.Button();
            this.textBoxVlcPath = new System.Windows.Forms.TextBox();
            this.labelVlcPath = new System.Windows.Forms.Label();
            this.labelVideoPlayerVLC = new System.Windows.Forms.Label();
            this.radioButtonVideoPlayerVLC = new System.Windows.Forms.RadioButton();
            this.labelVideoPlayerMPlayer = new System.Windows.Forms.Label();
            this.labelDirectShowDescription = new System.Windows.Forms.Label();
            this.labelMpcHcDescription = new System.Windows.Forms.Label();
            this.radioButtonVideoPlayerMPV = new System.Windows.Forms.RadioButton();
            this.radioButtonVideoPlayerDirectShow = new System.Windows.Forms.RadioButton();
            this.radioButtonVideoPlayerMpcHc = new System.Windows.Forms.RadioButton();
            this.tabPageWaveform = new System.Windows.Forms.TabPage();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.buttonDownloadFfmpeg = new System.Windows.Forms.Button();
            this.buttonBrowseToFFmpeg = new System.Windows.Forms.Button();
            this.textBoxFFmpegPath = new System.Windows.Forms.TextBox();
            this.labelFFmpegPath = new System.Windows.Forms.Label();
            this.checkBoxUseFFmpeg = new System.Windows.Forms.CheckBox();
            this.groupBoxSpectrogram = new System.Windows.Forms.GroupBox();
            this.labelSpectrogramAppearance = new System.Windows.Forms.Label();
            this.comboBoxSpectrogramAppearance = new System.Windows.Forms.ComboBox();
            this.checkBoxGenerateSpectrogram = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.buttonWaveformsFolderEmpty = new System.Windows.Forms.Button();
            this.labelWaveformsFolderInfo = new System.Windows.Forms.Label();
            this.groupBoxWaveformAppearence = new System.Windows.Forms.GroupBox();
            this.checkBoxWaveformShowWpm = new System.Windows.Forms.CheckBox();
            this.checkBoxWaveformShowCps = new System.Windows.Forms.CheckBox();
            this.checkBoxWaveformSetVideoPosMoveStartEnd = new System.Windows.Forms.CheckBox();
            this.labelWaveformTextSize = new System.Windows.Forms.Label();
            this.checkBoxWaveformTextBold = new System.Windows.Forms.CheckBox();
            this.comboBoxWaveformTextSize = new System.Windows.Forms.ComboBox();
            this.checkBoxListViewMouseEnterFocus = new System.Windows.Forms.CheckBox();
            this.checkBoxWaveformHoverFocus = new System.Windows.Forms.CheckBox();
            this.labelWaveformBorderHitMs2 = new System.Windows.Forms.Label();
            this.numericUpDownWaveformBorderHitMs = new System.Windows.Forms.NumericUpDown();
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
            this.tabPageTools = new System.Windows.Forms.TabPage();
            this.groupBoxGoogleTranslate = new System.Windows.Forms.GroupBox();
            this.labelGoogleTranslateApiKey = new System.Windows.Forms.Label();
            this.textBoxGoogleTransleApiKey = new System.Windows.Forms.TextBox();
            this.linkLabelGoogleTranslateSignUp = new System.Windows.Forms.LinkLabel();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBoxBing = new System.Windows.Forms.GroupBox();
            this.labelBingTokenEndpoint = new System.Windows.Forms.Label();
            this.textBoxBingTokenEndpoint = new System.Windows.Forms.TextBox();
            this.labelBingApiKey = new System.Windows.Forms.Label();
            this.textBoxBingClientSecret = new System.Windows.Forms.TextBox();
            this.linkLabelBingSubscribe = new System.Windows.Forms.LinkLabel();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBoxToolsAutoBr = new System.Windows.Forms.GroupBox();
            this.labelToolsBreakBottomHeavyPercent = new System.Windows.Forms.Label();
            this.numericUpDownToolsBreakPreferBottomHeavy = new System.Windows.Forms.NumericUpDown();
            this.checkBoxToolsBreakPreferBottomHeavy = new System.Windows.Forms.CheckBox();
            this.checkBoxToolsBreakByPixelWidth = new System.Windows.Forms.CheckBox();
            this.checkBoxToolsBreakEarlyLineEnding = new System.Windows.Forms.CheckBox();
            this.checkBoxToolsBreakEarlyComma = new System.Windows.Forms.CheckBox();
            this.checkBoxToolsBreakEarlyDash = new System.Windows.Forms.CheckBox();
            this.labelUserBingApiId = new System.Windows.Forms.Label();
            this.buttonEditDoNotBreakAfterList = new System.Windows.Forms.Button();
            this.checkBoxUseDoNotBreakAfterList = new System.Windows.Forms.CheckBox();
            this.groupBoxSpellCheck = new System.Windows.Forms.GroupBox();
            this.checkBoxUseAlwaysToFile = new System.Windows.Forms.CheckBox();
            this.checkBoxTreatINQuoteAsING = new System.Windows.Forms.CheckBox();
            this.checkBoxSpellCheckOneLetterWords = new System.Windows.Forms.CheckBox();
            this.checkBoxSpellCheckAutoChangeNames = new System.Windows.Forms.CheckBox();
            this.groupBoxFixCommonErrors = new System.Windows.Forms.GroupBox();
            this.checkBoxFceSkipStep1 = new System.Windows.Forms.CheckBox();
            this.checkBoxFixShortDisplayTimesAllowMoveStartTime = new System.Windows.Forms.CheckBox();
            this.checkBoxFixCommonOcrErrorsUsingHardcodedRules = new System.Windows.Forms.CheckBox();
            this.comboBoxToolsMusicSymbol = new System.Windows.Forms.ComboBox();
            this.textBoxMusicSymbolsToReplace = new System.Windows.Forms.TextBox();
            this.labelToolsMusicSymbolsToReplace = new System.Windows.Forms.Label();
            this.labelToolsMusicSymbol = new System.Windows.Forms.Label();
            this.groupBoxToolsVisualSync = new System.Windows.Forms.GroupBox();
            this.labelToolsEndScene = new System.Windows.Forms.Label();
            this.comboBoxToolsEndSceneIndex = new System.Windows.Forms.ComboBox();
            this.labelToolsStartScene = new System.Windows.Forms.Label();
            this.comboBoxToolsStartSceneIndex = new System.Windows.Forms.ComboBox();
            this.comboBoxToolsVerifySeconds = new System.Windows.Forms.ComboBox();
            this.labelVerifyButton = new System.Windows.Forms.Label();
            this.tabPageWordLists = new System.Windows.Forms.TabPage();
            this.groupBoxWordLists = new System.Windows.Forms.GroupBox();
            this.linkLabelOpenDictionaryFolder = new System.Windows.Forms.LinkLabel();
            this.groupBoxOcrFixList = new System.Windows.Forms.GroupBox();
            this.textBoxOcrFixValue = new System.Windows.Forms.TextBox();
            this.buttonRemoveOcrFix = new System.Windows.Forms.Button();
            this.listBoxOcrFixList = new System.Windows.Forms.ListBox();
            this.textBoxOcrFixKey = new System.Windows.Forms.TextBox();
            this.buttonAddOcrFix = new System.Windows.Forms.Button();
            this.groupBoxUserWordList = new System.Windows.Forms.GroupBox();
            this.buttonRemoveUserWord = new System.Windows.Forms.Button();
            this.listBoxUserWordLists = new System.Windows.Forms.ListBox();
            this.textBoxUserWord = new System.Windows.Forms.TextBox();
            this.buttonAddUserWord = new System.Windows.Forms.Button();
            this.groupBoxWordListLocation = new System.Windows.Forms.GroupBox();
            this.checkBoxNamesOnline = new System.Windows.Forms.CheckBox();
            this.textBoxNamesOnline = new System.Windows.Forms.TextBox();
            this.groupBoxNamesIgonoreLists = new System.Windows.Forms.GroupBox();
            this.buttonRemoveNameEtc = new System.Windows.Forms.Button();
            this.listBoxNames = new System.Windows.Forms.ListBox();
            this.textBoxNameEtc = new System.Windows.Forms.TextBox();
            this.buttonAddNames = new System.Windows.Forms.Button();
            this.labelWordListLanguage = new System.Windows.Forms.Label();
            this.comboBoxWordListLanguage = new System.Windows.Forms.ComboBox();
            this.tabPageToolBar = new System.Windows.Forms.TabPage();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.checkBoxShowFrameRate = new System.Windows.Forms.CheckBox();
            this.groupBoxShowToolBarButtons = new System.Windows.Forms.GroupBox();
            this.labelTBNetflixQualityCheck = new System.Windows.Forms.Label();
            this.pictureBoxNetflixQualityCheck = new System.Windows.Forms.PictureBox();
            this.checkBoxNetflixQualityCheck = new System.Windows.Forms.CheckBox();
            this.labelTBRemoveTextForHi = new System.Windows.Forms.Label();
            this.pictureBoxTBRemoveTextForHi = new System.Windows.Forms.PictureBox();
            this.checkBoxTBRemoveTextForHi = new System.Windows.Forms.CheckBox();
            this.labelTBFixCommonErrors = new System.Windows.Forms.Label();
            this.pictureBoxTBFixCommonErrors = new System.Windows.Forms.PictureBox();
            this.checkBoxTBFixCommonErrors = new System.Windows.Forms.CheckBox();
            this.labelTBHelp = new System.Windows.Forms.Label();
            this.pictureBoxHelp = new System.Windows.Forms.PictureBox();
            this.checkBoxHelp = new System.Windows.Forms.CheckBox();
            this.labelTBSettings = new System.Windows.Forms.Label();
            this.pictureBoxSettings = new System.Windows.Forms.PictureBox();
            this.checkBoxSettings = new System.Windows.Forms.CheckBox();
            this.labelTBSpellCheck = new System.Windows.Forms.Label();
            this.pictureBoxSpellCheck = new System.Windows.Forms.PictureBox();
            this.checkBoxSpellCheck = new System.Windows.Forms.CheckBox();
            this.labelTBVisualSync = new System.Windows.Forms.Label();
            this.pictureBoxVisualSync = new System.Windows.Forms.PictureBox();
            this.checkBoxVisualSync = new System.Windows.Forms.CheckBox();
            this.labelTBReplace = new System.Windows.Forms.Label();
            this.pictureBoxReplace = new System.Windows.Forms.PictureBox();
            this.checkBoxReplace = new System.Windows.Forms.CheckBox();
            this.labelTBFind = new System.Windows.Forms.Label();
            this.pictureBoxFind = new System.Windows.Forms.PictureBox();
            this.checkBoxToolbarFind = new System.Windows.Forms.CheckBox();
            this.labelTBSaveAs = new System.Windows.Forms.Label();
            this.pictureBoxSaveAs = new System.Windows.Forms.PictureBox();
            this.checkBoxToolbarSaveAs = new System.Windows.Forms.CheckBox();
            this.labelTBSave = new System.Windows.Forms.Label();
            this.pictureBoxSave = new System.Windows.Forms.PictureBox();
            this.checkBoxToolbarSave = new System.Windows.Forms.CheckBox();
            this.labelTBOpen = new System.Windows.Forms.Label();
            this.pictureBoxOpen = new System.Windows.Forms.PictureBox();
            this.checkBoxToolbarOpen = new System.Windows.Forms.CheckBox();
            this.labelTBNew = new System.Windows.Forms.Label();
            this.pictureBoxNew = new System.Windows.Forms.PictureBox();
            this.checkBoxToolbarNew = new System.Windows.Forms.CheckBox();
            this.tabPageFont = new System.Windows.Forms.TabPage();
            this.groupBoxFont = new System.Windows.Forms.GroupBox();
            this.labelFontNote = new System.Windows.Forms.Label();
            this.groupBoxFontTextBox = new System.Windows.Forms.GroupBox();
            this.labelSubtitleFontSize = new System.Windows.Forms.Label();
            this.comboBoxSubtitleFontSize = new System.Windows.Forms.ComboBox();
            this.checkBoxSubtitleFontBold = new System.Windows.Forms.CheckBox();
            this.checkBoxSubtitleCenter = new System.Windows.Forms.CheckBox();
            this.groupBoxFontListViews = new System.Windows.Forms.GroupBox();
            this.labelSubtitleListViewFontSize = new System.Windows.Forms.Label();
            this.comboBoxSubtitleListViewFontSize = new System.Windows.Forms.ComboBox();
            this.checkBoxSubtitleListViewFontBold = new System.Windows.Forms.CheckBox();
            this.groupBoxFontGeneral = new System.Windows.Forms.GroupBox();
            this.comboBoxSubtitleFont = new System.Windows.Forms.ComboBox();
            this.labelSubtitleFont = new System.Windows.Forms.Label();
            this.panelSubtitleFontColor = new System.Windows.Forms.Panel();
            this.labelSubtitleFontColor = new System.Windows.Forms.Label();
            this.panelSubtitleBackgroundColor = new System.Windows.Forms.Panel();
            this.labelSubtitleFontBackgroundColor = new System.Windows.Forms.Label();
            this.tabPageSsaStyle = new System.Windows.Forms.TabPage();
            this.groupBoxSsaStyle = new System.Windows.Forms.GroupBox();
            this.groupBoxSsaBorder = new System.Windows.Forms.GroupBox();
            this.numericUpDownSsaOutline = new System.Windows.Forms.NumericUpDown();
            this.labelSsaShadow = new System.Windows.Forms.Label();
            this.numericUpDownSsaShadow = new System.Windows.Forms.NumericUpDown();
            this.checkBoxSsaOpaqueBox = new System.Windows.Forms.CheckBox();
            this.labelSsaOutline = new System.Windows.Forms.Label();
            this.groupSsaBoxFont = new System.Windows.Forms.GroupBox();
            this.checkBoxSsaFontBold = new System.Windows.Forms.CheckBox();
            this.buttonSsaColor = new System.Windows.Forms.Button();
            this.panelPrimaryColor = new System.Windows.Forms.Panel();
            this.numericUpDownFontSize = new System.Windows.Forms.NumericUpDown();
            this.comboBoxFontName = new System.Windows.Forms.ComboBox();
            this.labelSsaFontSize = new System.Windows.Forms.Label();
            this.labelFontName = new System.Windows.Forms.Label();
            this.groupBoxMargins = new System.Windows.Forms.GroupBox();
            this.numericUpDownSsaMarginVertical = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownSsaMarginRight = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownSsaMarginLeft = new System.Windows.Forms.NumericUpDown();
            this.labelMarginVertical = new System.Windows.Forms.Label();
            this.labelMarginRight = new System.Windows.Forms.Label();
            this.labelMarginLeft = new System.Windows.Forms.Label();
            this.groupBoxPreview = new System.Windows.Forms.GroupBox();
            this.pictureBoxPreview = new System.Windows.Forms.PictureBox();
            this.tabPageNetwork = new System.Windows.Forms.TabPage();
            this.groupBoxNetworkSession = new System.Windows.Forms.GroupBox();
            this.buttonNetworkSessionNewMessageSound = new System.Windows.Forms.Button();
            this.textBoxNetworkSessionNewMessageSound = new System.Windows.Forms.TextBox();
            this.labelNetworkSessionNewMessageSound = new System.Windows.Forms.Label();
            this.groupBoxProxySettings = new System.Windows.Forms.GroupBox();
            this.groupBoxProxyAuthentication = new System.Windows.Forms.GroupBox();
            this.textBoxProxyDomain = new System.Windows.Forms.TextBox();
            this.labelProxyDomain = new System.Windows.Forms.Label();
            this.textBoxProxyUserName = new System.Windows.Forms.TextBox();
            this.labelProxyPassword = new System.Windows.Forms.Label();
            this.labelProxyUserName = new System.Windows.Forms.Label();
            this.textBoxProxyPassword = new System.Windows.Forms.TextBox();
            this.textBoxProxyAddress = new System.Windows.Forms.TextBox();
            this.labelProxyAddress = new System.Windows.Forms.Label();
            this.colorDialogSSAStyle = new System.Windows.Forms.ColorDialog();
            this.labelStatus = new System.Windows.Forms.Label();
            this.openFileDialogFFmpeg = new System.Windows.Forms.OpenFileDialog();
            this.buttonReset = new System.Windows.Forms.Button();
            this.tabControlSettings.SuspendLayout();
            this.tabPageGeneral.SuspendLayout();
            this.groupBoxMiscellaneous.SuspendLayout();
            this.groupBoxGeneralRules.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownOptimalCharsSec)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxWordsMin)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxNumberOfLines)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDurationMin)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDurationMax)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMinGapMs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxCharsSec)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSubtitleLineMaximumLength)).BeginInit();
            this.tabPageShortcuts.SuspendLayout();
            this.groupBoxShortcuts.SuspendLayout();
            this.tabPageSyntaxColoring.SuspendLayout();
            this.groupBoxListViewSyntaxColoring.SuspendLayout();
            this.tabPageVideoPlayer.SuspendLayout();
            this.groupBoxMainWindowVideoControls.SuspendLayout();
            this.groupBoxVideoPlayerDefault.SuspendLayout();
            this.groupBoxVideoEngine.SuspendLayout();
            this.tabPageWaveform.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBoxSpectrogram.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBoxWaveformAppearence.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownWaveformBorderHitMs)).BeginInit();
            this.tabPageTools.SuspendLayout();
            this.groupBoxGoogleTranslate.SuspendLayout();
            this.groupBoxBing.SuspendLayout();
            this.groupBoxToolsAutoBr.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownToolsBreakPreferBottomHeavy)).BeginInit();
            this.groupBoxSpellCheck.SuspendLayout();
            this.groupBoxFixCommonErrors.SuspendLayout();
            this.groupBoxToolsVisualSync.SuspendLayout();
            this.tabPageWordLists.SuspendLayout();
            this.groupBoxWordLists.SuspendLayout();
            this.groupBoxOcrFixList.SuspendLayout();
            this.groupBoxUserWordList.SuspendLayout();
            this.groupBoxWordListLocation.SuspendLayout();
            this.groupBoxNamesIgonoreLists.SuspendLayout();
            this.tabPageToolBar.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBoxShowToolBarButtons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxNetflixQualityCheck)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTBRemoveTextForHi)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTBFixCommonErrors)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxHelp)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSettings)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSpellCheck)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxVisualSync)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxReplace)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxFind)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSaveAs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSave)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxOpen)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxNew)).BeginInit();
            this.tabPageFont.SuspendLayout();
            this.groupBoxFont.SuspendLayout();
            this.groupBoxFontTextBox.SuspendLayout();
            this.groupBoxFontListViews.SuspendLayout();
            this.groupBoxFontGeneral.SuspendLayout();
            this.tabPageSsaStyle.SuspendLayout();
            this.groupBoxSsaStyle.SuspendLayout();
            this.groupBoxSsaBorder.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSsaOutline)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSsaShadow)).BeginInit();
            this.groupSsaBoxFont.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFontSize)).BeginInit();
            this.groupBoxMargins.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSsaMarginVertical)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSsaMarginRight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSsaMarginLeft)).BeginInit();
            this.groupBoxPreview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreview)).BeginInit();
            this.tabPageNetwork.SuspendLayout();
            this.groupBoxNetworkSession.SuspendLayout();
            this.groupBoxProxySettings.SuspendLayout();
            this.groupBoxProxyAuthentication.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(506, 526);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 0;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.ButtonOkClick);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(587, 526);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // tabControlSettings
            // 
            this.tabControlSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControlSettings.Controls.Add(this.tabPageGeneral);
            this.tabControlSettings.Controls.Add(this.tabPageShortcuts);
            this.tabControlSettings.Controls.Add(this.tabPageSyntaxColoring);
            this.tabControlSettings.Controls.Add(this.tabPageVideoPlayer);
            this.tabControlSettings.Controls.Add(this.tabPageWaveform);
            this.tabControlSettings.Controls.Add(this.tabPageTools);
            this.tabControlSettings.Controls.Add(this.tabPageWordLists);
            this.tabControlSettings.Controls.Add(this.tabPageToolBar);
            this.tabControlSettings.Controls.Add(this.tabPageFont);
            this.tabControlSettings.Controls.Add(this.tabPageSsaStyle);
            this.tabControlSettings.Controls.Add(this.tabPageNetwork);
            this.tabControlSettings.Location = new System.Drawing.Point(13, 13);
            this.tabControlSettings.Name = "tabControlSettings";
            this.tabControlSettings.SelectedIndex = 0;
            this.tabControlSettings.Size = new System.Drawing.Size(840, 509);
            this.tabControlSettings.TabIndex = 2;
            this.tabControlSettings.SelectedIndexChanged += new System.EventHandler(this.TabControlSettingsSelectedIndexChanged);
            // 
            // tabPageGeneral
            // 
            this.tabPageGeneral.Controls.Add(this.groupBoxMiscellaneous);
            this.tabPageGeneral.Location = new System.Drawing.Point(4, 22);
            this.tabPageGeneral.Name = "tabPageGeneral";
            this.tabPageGeneral.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageGeneral.Size = new System.Drawing.Size(832, 483);
            this.tabPageGeneral.TabIndex = 0;
            this.tabPageGeneral.Text = "General";
            this.tabPageGeneral.UseVisualStyleBackColor = true;
            // 
            // groupBoxMiscellaneous
            // 
            this.groupBoxMiscellaneous.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
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
            this.groupBoxMiscellaneous.Location = new System.Drawing.Point(6, 6);
            this.groupBoxMiscellaneous.Name = "groupBoxMiscellaneous";
            this.groupBoxMiscellaneous.Size = new System.Drawing.Size(819, 471);
            this.groupBoxMiscellaneous.TabIndex = 0;
            this.groupBoxMiscellaneous.TabStop = false;
            this.groupBoxMiscellaneous.Text = "Miscellaneous";
            // 
            // checkBoxAutoSave
            // 
            this.checkBoxAutoSave.AutoSize = true;
            this.checkBoxAutoSave.Location = new System.Drawing.Point(441, 426);
            this.checkBoxAutoSave.Name = "checkBoxAutoSave";
            this.checkBoxAutoSave.Size = new System.Drawing.Size(75, 17);
            this.checkBoxAutoSave.TabIndex = 28;
            this.checkBoxAutoSave.Text = "Auto save";
            this.checkBoxAutoSave.UseVisualStyleBackColor = true;
            // 
            // comboBoxSaveAsFileNameFrom
            // 
            this.comboBoxSaveAsFileNameFrom.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSaveAsFileNameFrom.FormattingEnabled = true;
            this.comboBoxSaveAsFileNameFrom.Location = new System.Drawing.Point(442, 359);
            this.comboBoxSaveAsFileNameFrom.Name = "comboBoxSaveAsFileNameFrom";
            this.comboBoxSaveAsFileNameFrom.Size = new System.Drawing.Size(332, 21);
            this.comboBoxSaveAsFileNameFrom.TabIndex = 23;
            // 
            // labelSaveAsFileNameFrom
            // 
            this.labelSaveAsFileNameFrom.AutoSize = true;
            this.labelSaveAsFileNameFrom.Location = new System.Drawing.Point(439, 343);
            this.labelSaveAsFileNameFrom.Name = "labelSaveAsFileNameFrom";
            this.labelSaveAsFileNameFrom.Size = new System.Drawing.Size(160, 13);
            this.labelSaveAsFileNameFrom.TabIndex = 22;
            this.labelSaveAsFileNameFrom.Text = "\"Save as...\" will take name from";
            // 
            // groupBoxGeneralRules
            // 
            this.groupBoxGeneralRules.Controls.Add(this.checkBoxCpsIncludeWhiteSpace);
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
            this.groupBoxGeneralRules.Location = new System.Drawing.Point(6, 24);
            this.groupBoxGeneralRules.Name = "groupBoxGeneralRules";
            this.groupBoxGeneralRules.Size = new System.Drawing.Size(387, 337);
            this.groupBoxGeneralRules.TabIndex = 0;
            this.groupBoxGeneralRules.TabStop = false;
            this.groupBoxGeneralRules.Text = "Rules";
            // 
            // checkBoxCpsIncludeWhiteSpace
            // 
            this.checkBoxCpsIncludeWhiteSpace.AutoSize = true;
            this.checkBoxCpsIncludeWhiteSpace.Location = new System.Drawing.Point(9, 299);
            this.checkBoxCpsIncludeWhiteSpace.Name = "checkBoxCpsIncludeWhiteSpace";
            this.checkBoxCpsIncludeWhiteSpace.Size = new System.Drawing.Size(271, 17);
            this.checkBoxCpsIncludeWhiteSpace.TabIndex = 60;
            this.checkBoxCpsIncludeWhiteSpace.Text = "Characters per second (CPS) includes white spaces";
            this.checkBoxCpsIncludeWhiteSpace.UseVisualStyleBackColor = true;
            this.checkBoxCpsIncludeWhiteSpace.CheckedChanged += new System.EventHandler(this.ProfileUiValueChanged);
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
            this.comboBoxRulesProfileName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxRulesProfileName.FormattingEnabled = true;
            this.comboBoxRulesProfileName.Location = new System.Drawing.Point(78, 20);
            this.comboBoxRulesProfileName.Name = "comboBoxRulesProfileName";
            this.comboBoxRulesProfileName.Size = new System.Drawing.Size(238, 21);
            this.comboBoxRulesProfileName.TabIndex = 5;
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
            this.numericUpDownOptimalCharsSec.DecimalPlaces = 1;
            this.numericUpDownOptimalCharsSec.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericUpDownOptimalCharsSec.Location = new System.Drawing.Point(203, 82);
            this.numericUpDownOptimalCharsSec.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.numericUpDownOptimalCharsSec.Name = "numericUpDownOptimalCharsSec";
            this.numericUpDownOptimalCharsSec.Size = new System.Drawing.Size(56, 21);
            this.numericUpDownOptimalCharsSec.TabIndex = 20;
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
            this.numericUpDownMaxNumberOfLines.Location = new System.Drawing.Point(203, 242);
            this.numericUpDownMaxNumberOfLines.Maximum = new decimal(new int[] {
            25,
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
            this.numericUpDownDurationMin.Location = new System.Drawing.Point(203, 163);
            this.numericUpDownDurationMin.Maximum = new decimal(new int[] {
            3000,
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
            this.numericUpDownDurationMin.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDownDurationMin.ValueChanged += new System.EventHandler(this.ProfileUiValueChanged);
            // 
            // numericUpDownDurationMax
            // 
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
            this.numericUpDownDurationMax.Value = new decimal(new int[] {
            50000,
            0,
            0,
            0});
            this.numericUpDownDurationMax.ValueChanged += new System.EventHandler(this.ProfileUiValueChanged);
            // 
            // comboBoxMergeShortLineLength
            // 
            this.comboBoxMergeShortLineLength.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxMergeShortLineLength.FormattingEnabled = true;
            this.comboBoxMergeShortLineLength.Location = new System.Drawing.Point(203, 270);
            this.comboBoxMergeShortLineLength.Name = "comboBoxMergeShortLineLength";
            this.comboBoxMergeShortLineLength.Size = new System.Drawing.Size(73, 21);
            this.comboBoxMergeShortLineLength.TabIndex = 55;
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
            this.numericUpDownMinGapMs.Location = new System.Drawing.Point(203, 215);
            this.numericUpDownMinGapMs.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownMinGapMs.Name = "numericUpDownMinGapMs";
            this.numericUpDownMinGapMs.Size = new System.Drawing.Size(56, 21);
            this.numericUpDownMinGapMs.TabIndex = 45;
            this.numericUpDownMinGapMs.Value = new decimal(new int[] {
            25,
            0,
            0,
            0});
            this.numericUpDownMinGapMs.ValueChanged += new System.EventHandler(this.ProfileUiValueChanged);
            // 
            // numericUpDownMaxCharsSec
            // 
            this.numericUpDownMaxCharsSec.DecimalPlaces = 1;
            this.numericUpDownMaxCharsSec.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericUpDownMaxCharsSec.Location = new System.Drawing.Point(203, 109);
            this.numericUpDownMaxCharsSec.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.numericUpDownMaxCharsSec.Name = "numericUpDownMaxCharsSec";
            this.numericUpDownMaxCharsSec.Size = new System.Drawing.Size(56, 21);
            this.numericUpDownMaxCharsSec.TabIndex = 25;
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
            this.numericUpDownSubtitleLineMaximumLength.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDownSubtitleLineMaximumLength.ValueChanged += new System.EventHandler(this.ProfileUiValueChanged);
            // 
            // comboBoxAutoBackupDeleteAfter
            // 
            this.comboBoxAutoBackupDeleteAfter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxAutoBackupDeleteAfter.FormattingEnabled = true;
            this.comboBoxAutoBackupDeleteAfter.Items.AddRange(new object[] {
            "1 month",
            "3 months",
            "6 months"});
            this.comboBoxAutoBackupDeleteAfter.Location = new System.Drawing.Point(707, 394);
            this.comboBoxAutoBackupDeleteAfter.Name = "comboBoxAutoBackupDeleteAfter";
            this.comboBoxAutoBackupDeleteAfter.Size = new System.Drawing.Size(88, 21);
            this.comboBoxAutoBackupDeleteAfter.TabIndex = 27;
            // 
            // labelAutoBackupDeleteAfter
            // 
            this.labelAutoBackupDeleteAfter.AutoSize = true;
            this.labelAutoBackupDeleteAfter.Location = new System.Drawing.Point(639, 397);
            this.labelAutoBackupDeleteAfter.Name = "labelAutoBackupDeleteAfter";
            this.labelAutoBackupDeleteAfter.Size = new System.Drawing.Size(65, 13);
            this.labelAutoBackupDeleteAfter.TabIndex = 26;
            this.labelAutoBackupDeleteAfter.Text = "Delete after";
            // 
            // checkBoxCheckForUpdates
            // 
            this.checkBoxCheckForUpdates.AutoSize = true;
            this.checkBoxCheckForUpdates.Location = new System.Drawing.Point(522, 426);
            this.checkBoxCheckForUpdates.Name = "checkBoxCheckForUpdates";
            this.checkBoxCheckForUpdates.Size = new System.Drawing.Size(114, 17);
            this.checkBoxCheckForUpdates.TabIndex = 29;
            this.checkBoxCheckForUpdates.Text = "Check for updates";
            this.checkBoxCheckForUpdates.UseVisualStyleBackColor = true;
            // 
            // labelSpellChecker
            // 
            this.labelSpellChecker.AutoSize = true;
            this.labelSpellChecker.Location = new System.Drawing.Point(669, 19);
            this.labelSpellChecker.Name = "labelSpellChecker";
            this.labelSpellChecker.Size = new System.Drawing.Size(69, 13);
            this.labelSpellChecker.TabIndex = 30;
            this.labelSpellChecker.Text = "Spell checker";
            this.labelSpellChecker.Visible = false;
            // 
            // comboBoxTimeCodeMode
            // 
            this.comboBoxTimeCodeMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxTimeCodeMode.FormattingEnabled = true;
            this.comboBoxTimeCodeMode.Items.AddRange(new object[] {
            "HH:MM:SS:MSEC (00:00:00.000)",
            "HH:MM:SS:FF (00:00:00.00)"});
            this.comboBoxTimeCodeMode.Location = new System.Drawing.Point(528, 261);
            this.comboBoxTimeCodeMode.Name = "comboBoxTimeCodeMode";
            this.comboBoxTimeCodeMode.Size = new System.Drawing.Size(207, 21);
            this.comboBoxTimeCodeMode.TabIndex = 19;
            // 
            // labelTimeCodeMode
            // 
            this.labelTimeCodeMode.AutoSize = true;
            this.labelTimeCodeMode.Location = new System.Drawing.Point(438, 264);
            this.labelTimeCodeMode.Name = "labelTimeCodeMode";
            this.labelTimeCodeMode.Size = new System.Drawing.Size(84, 13);
            this.labelTimeCodeMode.TabIndex = 18;
            this.labelTimeCodeMode.Text = "Time code mode";
            // 
            // comboBoxEncoding
            // 
            this.comboBoxEncoding.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxEncoding.FormattingEnabled = true;
            this.comboBoxEncoding.Items.AddRange(new object[] {
            "ANSI",
            "UTF-7",
            "UTF-8",
            "Unicode"});
            this.comboBoxEncoding.Location = new System.Drawing.Point(205, 400);
            this.comboBoxEncoding.Name = "comboBoxEncoding";
            this.comboBoxEncoding.Size = new System.Drawing.Size(188, 21);
            this.comboBoxEncoding.TabIndex = 4;
            // 
            // checkBoxAutoDetectAnsiEncoding
            // 
            this.checkBoxAutoDetectAnsiEncoding.AutoSize = true;
            this.checkBoxAutoDetectAnsiEncoding.Location = new System.Drawing.Point(205, 429);
            this.checkBoxAutoDetectAnsiEncoding.Name = "checkBoxAutoDetectAnsiEncoding";
            this.checkBoxAutoDetectAnsiEncoding.Size = new System.Drawing.Size(15, 14);
            this.checkBoxAutoDetectAnsiEncoding.TabIndex = 6;
            this.checkBoxAutoDetectAnsiEncoding.UseVisualStyleBackColor = true;
            // 
            // textBoxShowLineBreaksAs
            // 
            this.textBoxShowLineBreaksAs.Location = new System.Drawing.Point(594, 233);
            this.textBoxShowLineBreaksAs.MaxLength = 10;
            this.textBoxShowLineBreaksAs.Name = "textBoxShowLineBreaksAs";
            this.textBoxShowLineBreaksAs.Size = new System.Drawing.Size(60, 21);
            this.textBoxShowLineBreaksAs.TabIndex = 17;
            // 
            // checkBoxAutoWrapWhileTyping
            // 
            this.checkBoxAutoWrapWhileTyping.AutoSize = true;
            this.checkBoxAutoWrapWhileTyping.Location = new System.Drawing.Point(441, 210);
            this.checkBoxAutoWrapWhileTyping.Name = "checkBoxAutoWrapWhileTyping";
            this.checkBoxAutoWrapWhileTyping.Size = new System.Drawing.Size(137, 17);
            this.checkBoxAutoWrapWhileTyping.TabIndex = 15;
            this.checkBoxAutoWrapWhileTyping.Text = "Auto-wrap while typing";
            this.checkBoxAutoWrapWhileTyping.UseVisualStyleBackColor = true;
            // 
            // checkBoxPromptDeleteLines
            // 
            this.checkBoxPromptDeleteLines.AutoSize = true;
            this.checkBoxPromptDeleteLines.Location = new System.Drawing.Point(441, 187);
            this.checkBoxPromptDeleteLines.Name = "checkBoxPromptDeleteLines";
            this.checkBoxPromptDeleteLines.Size = new System.Drawing.Size(142, 17);
            this.checkBoxPromptDeleteLines.TabIndex = 14;
            this.checkBoxPromptDeleteLines.Text = "Prompt for deleting lines";
            this.checkBoxPromptDeleteLines.UseVisualStyleBackColor = true;
            // 
            // checkBoxAllowEditOfOriginalSubtitle
            // 
            this.checkBoxAllowEditOfOriginalSubtitle.AutoSize = true;
            this.checkBoxAllowEditOfOriginalSubtitle.Location = new System.Drawing.Point(441, 164);
            this.checkBoxAllowEditOfOriginalSubtitle.Name = "checkBoxAllowEditOfOriginalSubtitle";
            this.checkBoxAllowEditOfOriginalSubtitle.Size = new System.Drawing.Size(160, 17);
            this.checkBoxAllowEditOfOriginalSubtitle.TabIndex = 13;
            this.checkBoxAllowEditOfOriginalSubtitle.Text = "Allow edit of original subtitle";
            this.checkBoxAllowEditOfOriginalSubtitle.UseVisualStyleBackColor = true;
            // 
            // comboBoxSpellChecker
            // 
            this.comboBoxSpellChecker.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSpellChecker.FormattingEnabled = true;
            this.comboBoxSpellChecker.Items.AddRange(new object[] {
            "Hunspell",
            "Word"});
            this.comboBoxSpellChecker.Location = new System.Drawing.Point(692, 16);
            this.comboBoxSpellChecker.Name = "comboBoxSpellChecker";
            this.comboBoxSpellChecker.Size = new System.Drawing.Size(121, 21);
            this.comboBoxSpellChecker.TabIndex = 31;
            this.comboBoxSpellChecker.Visible = false;
            // 
            // comboBoxAutoBackup
            // 
            this.comboBoxAutoBackup.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxAutoBackup.FormattingEnabled = true;
            this.comboBoxAutoBackup.Items.AddRange(new object[] {
            "None",
            "Every minute",
            "Every 5 minutes",
            "Every 15 minutes"});
            this.comboBoxAutoBackup.Location = new System.Drawing.Point(512, 394);
            this.comboBoxAutoBackup.Name = "comboBoxAutoBackup";
            this.comboBoxAutoBackup.Size = new System.Drawing.Size(121, 21);
            this.comboBoxAutoBackup.TabIndex = 25;
            // 
            // labelAutoBackup
            // 
            this.labelAutoBackup.AutoSize = true;
            this.labelAutoBackup.Location = new System.Drawing.Point(438, 397);
            this.labelAutoBackup.Name = "labelAutoBackup";
            this.labelAutoBackup.Size = new System.Drawing.Size(68, 13);
            this.labelAutoBackup.TabIndex = 24;
            this.labelAutoBackup.Text = "Auto-backup";
            // 
            // checkBoxRememberSelectedLine
            // 
            this.checkBoxRememberSelectedLine.AutoSize = true;
            this.checkBoxRememberSelectedLine.Location = new System.Drawing.Point(449, 69);
            this.checkBoxRememberSelectedLine.Name = "checkBoxRememberSelectedLine";
            this.checkBoxRememberSelectedLine.Size = new System.Drawing.Size(139, 17);
            this.checkBoxRememberSelectedLine.TabIndex = 9;
            this.checkBoxRememberSelectedLine.Text = "Remember selected line";
            this.checkBoxRememberSelectedLine.UseVisualStyleBackColor = true;
            // 
            // checkBoxRemoveBlankLinesWhenOpening
            // 
            this.checkBoxRemoveBlankLinesWhenOpening.AutoSize = true;
            this.checkBoxRemoveBlankLinesWhenOpening.Location = new System.Drawing.Point(441, 141);
            this.checkBoxRemoveBlankLinesWhenOpening.Name = "checkBoxRemoveBlankLinesWhenOpening";
            this.checkBoxRemoveBlankLinesWhenOpening.Size = new System.Drawing.Size(225, 17);
            this.checkBoxRemoveBlankLinesWhenOpening.TabIndex = 12;
            this.checkBoxRemoveBlankLinesWhenOpening.Text = "Remove blank lines when opening subtitle";
            this.checkBoxRemoveBlankLinesWhenOpening.UseVisualStyleBackColor = true;
            // 
            // labelAutoDetectAnsiEncoding
            // 
            this.labelAutoDetectAnsiEncoding.AutoSize = true;
            this.labelAutoDetectAnsiEncoding.Location = new System.Drawing.Point(8, 428);
            this.labelAutoDetectAnsiEncoding.Name = "labelAutoDetectAnsiEncoding";
            this.labelAutoDetectAnsiEncoding.Size = new System.Drawing.Size(137, 13);
            this.labelAutoDetectAnsiEncoding.TabIndex = 5;
            this.labelAutoDetectAnsiEncoding.Text = "Auto detect ANSI encoding";
            // 
            // comboBoxListViewDoubleClickEvent
            // 
            this.comboBoxListViewDoubleClickEvent.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxListViewDoubleClickEvent.FormattingEnabled = true;
            this.comboBoxListViewDoubleClickEvent.Items.AddRange(new object[] {
            "ANSI",
            "UTF-7",
            "UTF-8",
            "Unicode"});
            this.comboBoxListViewDoubleClickEvent.Location = new System.Drawing.Point(441, 310);
            this.comboBoxListViewDoubleClickEvent.Name = "comboBoxListViewDoubleClickEvent";
            this.comboBoxListViewDoubleClickEvent.Size = new System.Drawing.Size(332, 21);
            this.comboBoxListViewDoubleClickEvent.TabIndex = 21;
            // 
            // labelListViewDoubleClickEvent
            // 
            this.labelListViewDoubleClickEvent.AutoSize = true;
            this.labelListViewDoubleClickEvent.Location = new System.Drawing.Point(438, 294);
            this.labelListViewDoubleClickEvent.Name = "labelListViewDoubleClickEvent";
            this.labelListViewDoubleClickEvent.Size = new System.Drawing.Size(227, 13);
            this.labelListViewDoubleClickEvent.TabIndex = 20;
            this.labelListViewDoubleClickEvent.Text = "Double-click on line in main window listview will";
            // 
            // labelShowLineBreaksAs
            // 
            this.labelShowLineBreaksAs.AutoSize = true;
            this.labelShowLineBreaksAs.Location = new System.Drawing.Point(438, 236);
            this.labelShowLineBreaksAs.Name = "labelShowLineBreaksAs";
            this.labelShowLineBreaksAs.Size = new System.Drawing.Size(150, 13);
            this.labelShowLineBreaksAs.TabIndex = 16;
            this.labelShowLineBreaksAs.Text = "Show line breaks in listview as";
            // 
            // checkBoxRememberWindowPosition
            // 
            this.checkBoxRememberWindowPosition.AutoSize = true;
            this.checkBoxRememberWindowPosition.Location = new System.Drawing.Point(441, 95);
            this.checkBoxRememberWindowPosition.Name = "checkBoxRememberWindowPosition";
            this.checkBoxRememberWindowPosition.Size = new System.Drawing.Size(223, 17);
            this.checkBoxRememberWindowPosition.TabIndex = 10;
            this.checkBoxRememberWindowPosition.Text = "Remember main window position and size";
            this.checkBoxRememberWindowPosition.UseVisualStyleBackColor = true;
            // 
            // checkBoxStartInSourceView
            // 
            this.checkBoxStartInSourceView.AutoSize = true;
            this.checkBoxStartInSourceView.Location = new System.Drawing.Point(441, 118);
            this.checkBoxStartInSourceView.Name = "checkBoxStartInSourceView";
            this.checkBoxStartInSourceView.Size = new System.Drawing.Size(121, 17);
            this.checkBoxStartInSourceView.TabIndex = 11;
            this.checkBoxStartInSourceView.Text = "Start in source view";
            this.checkBoxStartInSourceView.UseVisualStyleBackColor = true;
            // 
            // checkBoxReopenLastOpened
            // 
            this.checkBoxReopenLastOpened.AutoSize = true;
            this.checkBoxReopenLastOpened.Location = new System.Drawing.Point(449, 46);
            this.checkBoxReopenLastOpened.Name = "checkBoxReopenLastOpened";
            this.checkBoxReopenLastOpened.Size = new System.Drawing.Size(145, 17);
            this.checkBoxReopenLastOpened.TabIndex = 8;
            this.checkBoxReopenLastOpened.Text = "Start with last file loaded";
            this.checkBoxReopenLastOpened.UseVisualStyleBackColor = true;
            // 
            // checkBoxRememberRecentFiles
            // 
            this.checkBoxRememberRecentFiles.AutoSize = true;
            this.checkBoxRememberRecentFiles.Location = new System.Drawing.Point(441, 22);
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
            this.labelDefaultFileEncoding.Location = new System.Drawing.Point(8, 404);
            this.labelDefaultFileEncoding.Name = "labelDefaultFileEncoding";
            this.labelDefaultFileEncoding.Size = new System.Drawing.Size(105, 13);
            this.labelDefaultFileEncoding.TabIndex = 3;
            this.labelDefaultFileEncoding.Text = "Default file encoding";
            // 
            // comboBoxFrameRate
            // 
            this.comboBoxFrameRate.FormattingEnabled = true;
            this.comboBoxFrameRate.Location = new System.Drawing.Point(205, 372);
            this.comboBoxFrameRate.Name = "comboBoxFrameRate";
            this.comboBoxFrameRate.Size = new System.Drawing.Size(121, 21);
            this.comboBoxFrameRate.TabIndex = 2;
            // 
            // labelDefaultFrameRate
            // 
            this.labelDefaultFrameRate.AutoSize = true;
            this.labelDefaultFrameRate.Location = new System.Drawing.Point(8, 377);
            this.labelDefaultFrameRate.Name = "labelDefaultFrameRate";
            this.labelDefaultFrameRate.Size = new System.Drawing.Size(96, 13);
            this.labelDefaultFrameRate.TabIndex = 1;
            this.labelDefaultFrameRate.Text = "Default frame rate";
            // 
            // tabPageShortcuts
            // 
            this.tabPageShortcuts.Controls.Add(this.groupBoxShortcuts);
            this.tabPageShortcuts.Location = new System.Drawing.Point(4, 22);
            this.tabPageShortcuts.Name = "tabPageShortcuts";
            this.tabPageShortcuts.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageShortcuts.Size = new System.Drawing.Size(832, 483);
            this.tabPageShortcuts.TabIndex = 8;
            this.tabPageShortcuts.Text = "Shortcuts";
            this.tabPageShortcuts.UseVisualStyleBackColor = true;
            // 
            // groupBoxShortcuts
            // 
            this.groupBoxShortcuts.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
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
            this.groupBoxShortcuts.Location = new System.Drawing.Point(6, 6);
            this.groupBoxShortcuts.Name = "groupBoxShortcuts";
            this.groupBoxShortcuts.Size = new System.Drawing.Size(819, 475);
            this.groupBoxShortcuts.TabIndex = 2;
            this.groupBoxShortcuts.TabStop = false;
            this.groupBoxShortcuts.Text = "Shortcuts";
            // 
            // buttonShortcutsClear
            // 
            this.buttonShortcutsClear.Enabled = false;
            this.buttonShortcutsClear.Location = new System.Drawing.Point(221, 18);
            this.buttonShortcutsClear.Name = "buttonShortcutsClear";
            this.buttonShortcutsClear.Size = new System.Drawing.Size(111, 23);
            this.buttonShortcutsClear.TabIndex = 38;
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
            this.textBoxShortcutSearch.Location = new System.Drawing.Point(64, 20);
            this.textBoxShortcutSearch.Name = "textBoxShortcutSearch";
            this.textBoxShortcutSearch.Size = new System.Drawing.Size(151, 21);
            this.textBoxShortcutSearch.TabIndex = 36;
            this.textBoxShortcutSearch.TextChanged += new System.EventHandler(this.textBoxShortcutSearch_TextChanged);
            // 
            // buttonClearShortcut
            // 
            this.buttonClearShortcut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonClearShortcut.Enabled = false;
            this.buttonClearShortcut.Location = new System.Drawing.Point(582, 446);
            this.buttonClearShortcut.Name = "buttonClearShortcut";
            this.buttonClearShortcut.Size = new System.Drawing.Size(111, 23);
            this.buttonClearShortcut.TabIndex = 6;
            this.buttonClearShortcut.Text = "&Clear";
            this.buttonClearShortcut.UseVisualStyleBackColor = true;
            this.buttonClearShortcut.Click += new System.EventHandler(this.buttonClearShortcut_Click);
            // 
            // comboBoxShortcutKey
            // 
            this.comboBoxShortcutKey.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.comboBoxShortcutKey.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxShortcutKey.Enabled = false;
            this.comboBoxShortcutKey.FormattingEnabled = true;
            this.comboBoxShortcutKey.Items.AddRange(new object[] {
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
            "Del",
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
            this.comboBoxShortcutKey.Location = new System.Drawing.Point(353, 446);
            this.comboBoxShortcutKey.Name = "comboBoxShortcutKey";
            this.comboBoxShortcutKey.Size = new System.Drawing.Size(92, 21);
            this.comboBoxShortcutKey.TabIndex = 4;
            this.comboBoxShortcutKey.SelectedIndexChanged += new System.EventHandler(this.ValidateShortcut);
            this.comboBoxShortcutKey.KeyDown += new System.Windows.Forms.KeyEventHandler(this.comboBoxShortcutKey_KeyDown);
            // 
            // labelShortcutKey
            // 
            this.labelShortcutKey.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelShortcutKey.AutoSize = true;
            this.labelShortcutKey.Location = new System.Drawing.Point(322, 448);
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
            this.checkBoxShortcutsShift.Location = new System.Drawing.Point(245, 448);
            this.checkBoxShortcutsShift.Name = "checkBoxShortcutsShift";
            this.checkBoxShortcutsShift.Size = new System.Drawing.Size(48, 17);
            this.checkBoxShortcutsShift.TabIndex = 3;
            this.checkBoxShortcutsShift.Text = "Shift";
            this.checkBoxShortcutsShift.UseVisualStyleBackColor = true;
            this.checkBoxShortcutsShift.CheckedChanged += new System.EventHandler(this.ValidateShortcut);
            // 
            // checkBoxShortcutsAlt
            // 
            this.checkBoxShortcutsAlt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxShortcutsAlt.AutoSize = true;
            this.checkBoxShortcutsAlt.Enabled = false;
            this.checkBoxShortcutsAlt.Location = new System.Drawing.Point(176, 448);
            this.checkBoxShortcutsAlt.Name = "checkBoxShortcutsAlt";
            this.checkBoxShortcutsAlt.Size = new System.Drawing.Size(39, 17);
            this.checkBoxShortcutsAlt.TabIndex = 2;
            this.checkBoxShortcutsAlt.Text = "Alt";
            this.checkBoxShortcutsAlt.UseVisualStyleBackColor = true;
            this.checkBoxShortcutsAlt.CheckedChanged += new System.EventHandler(this.ValidateShortcut);
            // 
            // checkBoxShortcutsControl
            // 
            this.checkBoxShortcutsControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxShortcutsControl.AutoSize = true;
            this.checkBoxShortcutsControl.Enabled = false;
            this.checkBoxShortcutsControl.Location = new System.Drawing.Point(89, 448);
            this.checkBoxShortcutsControl.Name = "checkBoxShortcutsControl";
            this.checkBoxShortcutsControl.Size = new System.Drawing.Size(61, 17);
            this.checkBoxShortcutsControl.TabIndex = 1;
            this.checkBoxShortcutsControl.Text = "Control";
            this.checkBoxShortcutsControl.UseVisualStyleBackColor = true;
            this.checkBoxShortcutsControl.CheckedChanged += new System.EventHandler(this.ValidateShortcut);
            // 
            // buttonUpdateShortcut
            // 
            this.buttonUpdateShortcut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonUpdateShortcut.Enabled = false;
            this.buttonUpdateShortcut.Location = new System.Drawing.Point(465, 446);
            this.buttonUpdateShortcut.Name = "buttonUpdateShortcut";
            this.buttonUpdateShortcut.Size = new System.Drawing.Size(111, 23);
            this.buttonUpdateShortcut.TabIndex = 5;
            this.buttonUpdateShortcut.Text = "&Update";
            this.buttonUpdateShortcut.UseVisualStyleBackColor = true;
            this.buttonUpdateShortcut.Click += new System.EventHandler(this.buttonUpdateShortcut_Click);
            // 
            // treeViewShortcuts
            // 
            this.treeViewShortcuts.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeViewShortcuts.HideSelection = false;
            this.treeViewShortcuts.Location = new System.Drawing.Point(16, 47);
            this.treeViewShortcuts.Name = "treeViewShortcuts";
            this.treeViewShortcuts.Size = new System.Drawing.Size(797, 393);
            this.treeViewShortcuts.TabIndex = 0;
            this.treeViewShortcuts.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewShortcuts_AfterSelect);
            // 
            // labelShortcut
            // 
            this.labelShortcut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelShortcut.AutoSize = true;
            this.labelShortcut.Location = new System.Drawing.Point(15, 448);
            this.labelShortcut.Name = "labelShortcut";
            this.labelShortcut.Size = new System.Drawing.Size(52, 13);
            this.labelShortcut.TabIndex = 3;
            this.labelShortcut.Text = "Shortcut:";
            // 
            // tabPageSyntaxColoring
            // 
            this.tabPageSyntaxColoring.Controls.Add(this.groupBoxListViewSyntaxColoring);
            this.tabPageSyntaxColoring.Location = new System.Drawing.Point(4, 22);
            this.tabPageSyntaxColoring.Name = "tabPageSyntaxColoring";
            this.tabPageSyntaxColoring.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageSyntaxColoring.Size = new System.Drawing.Size(832, 483);
            this.tabPageSyntaxColoring.TabIndex = 9;
            this.tabPageSyntaxColoring.Text = "Syntax coloring";
            this.tabPageSyntaxColoring.UseVisualStyleBackColor = true;
            // 
            // groupBoxListViewSyntaxColoring
            // 
            this.groupBoxListViewSyntaxColoring.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxListViewSyntaxColoring.Controls.Add(this.checkBoxSyntaxColorGapTooSmall);
            this.groupBoxListViewSyntaxColoring.Controls.Add(this.checkBoxSyntaxColorTextMoreThanTwoLines);
            this.groupBoxListViewSyntaxColoring.Controls.Add(this.checkBoxSyntaxOverlap);
            this.groupBoxListViewSyntaxColoring.Controls.Add(this.checkBoxSyntaxColorDurationTooSmall);
            this.groupBoxListViewSyntaxColoring.Controls.Add(this.buttonListViewSyntaxColorError);
            this.groupBoxListViewSyntaxColoring.Controls.Add(this.checkBoxSyntaxColorTextTooLong);
            this.groupBoxListViewSyntaxColoring.Controls.Add(this.checkBoxSyntaxColorDurationTooLarge);
            this.groupBoxListViewSyntaxColoring.Controls.Add(this.panelListViewSyntaxColorError);
            this.groupBoxListViewSyntaxColoring.Location = new System.Drawing.Point(6, 6);
            this.groupBoxListViewSyntaxColoring.Name = "groupBoxListViewSyntaxColoring";
            this.groupBoxListViewSyntaxColoring.Size = new System.Drawing.Size(820, 475);
            this.groupBoxListViewSyntaxColoring.TabIndex = 0;
            this.groupBoxListViewSyntaxColoring.TabStop = false;
            this.groupBoxListViewSyntaxColoring.Text = "List view syntax coloring";
            // 
            // checkBoxSyntaxColorGapTooSmall
            // 
            this.checkBoxSyntaxColorGapTooSmall.AutoSize = true;
            this.checkBoxSyntaxColorGapTooSmall.Location = new System.Drawing.Point(20, 186);
            this.checkBoxSyntaxColorGapTooSmall.Name = "checkBoxSyntaxColorGapTooSmall";
            this.checkBoxSyntaxColorGapTooSmall.Size = new System.Drawing.Size(132, 17);
            this.checkBoxSyntaxColorGapTooSmall.TabIndex = 6;
            this.checkBoxSyntaxColorGapTooSmall.Text = "Gap - color if too small";
            this.checkBoxSyntaxColorGapTooSmall.UseVisualStyleBackColor = true;
            // 
            // checkBoxSyntaxColorTextMoreThanTwoLines
            // 
            this.checkBoxSyntaxColorTextMoreThanTwoLines.AutoSize = true;
            this.checkBoxSyntaxColorTextMoreThanTwoLines.Location = new System.Drawing.Point(20, 116);
            this.checkBoxSyntaxColorTextMoreThanTwoLines.Name = "checkBoxSyntaxColorTextMoreThanTwoLines";
            this.checkBoxSyntaxColorTextMoreThanTwoLines.Size = new System.Drawing.Size(170, 17);
            this.checkBoxSyntaxColorTextMoreThanTwoLines.TabIndex = 3;
            this.checkBoxSyntaxColorTextMoreThanTwoLines.Text = "Text - color if more than lines:";
            this.checkBoxSyntaxColorTextMoreThanTwoLines.UseVisualStyleBackColor = true;
            // 
            // checkBoxSyntaxOverlap
            // 
            this.checkBoxSyntaxOverlap.AutoSize = true;
            this.checkBoxSyntaxOverlap.Location = new System.Drawing.Point(20, 151);
            this.checkBoxSyntaxOverlap.Name = "checkBoxSyntaxOverlap";
            this.checkBoxSyntaxOverlap.Size = new System.Drawing.Size(129, 17);
            this.checkBoxSyntaxOverlap.TabIndex = 5;
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
            this.buttonListViewSyntaxColorError.Location = new System.Drawing.Point(20, 224);
            this.buttonListViewSyntaxColorError.Name = "buttonListViewSyntaxColorError";
            this.buttonListViewSyntaxColorError.Size = new System.Drawing.Size(112, 23);
            this.buttonListViewSyntaxColorError.TabIndex = 7;
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
            this.panelListViewSyntaxColorError.Location = new System.Drawing.Point(142, 224);
            this.panelListViewSyntaxColorError.Name = "panelListViewSyntaxColorError";
            this.panelListViewSyntaxColorError.Size = new System.Drawing.Size(21, 20);
            this.panelListViewSyntaxColorError.TabIndex = 8;
            this.panelListViewSyntaxColorError.Click += new System.EventHandler(this.buttonListViewSyntaxColorError_Click);
            // 
            // tabPageVideoPlayer
            // 
            this.tabPageVideoPlayer.Controls.Add(this.groupBoxMainWindowVideoControls);
            this.tabPageVideoPlayer.Controls.Add(this.groupBoxVideoPlayerDefault);
            this.tabPageVideoPlayer.Controls.Add(this.groupBoxVideoEngine);
            this.tabPageVideoPlayer.Location = new System.Drawing.Point(4, 22);
            this.tabPageVideoPlayer.Name = "tabPageVideoPlayer";
            this.tabPageVideoPlayer.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageVideoPlayer.Size = new System.Drawing.Size(832, 483);
            this.tabPageVideoPlayer.TabIndex = 2;
            this.tabPageVideoPlayer.Text = "Video player";
            this.tabPageVideoPlayer.UseVisualStyleBackColor = true;
            // 
            // groupBoxMainWindowVideoControls
            // 
            this.groupBoxMainWindowVideoControls.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
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
            this.groupBoxMainWindowVideoControls.Location = new System.Drawing.Point(7, 281);
            this.groupBoxMainWindowVideoControls.Name = "groupBoxMainWindowVideoControls";
            this.groupBoxMainWindowVideoControls.Size = new System.Drawing.Size(819, 181);
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
            this.textBoxCustomSearchUrl5.Location = new System.Drawing.Point(185, 146);
            this.textBoxCustomSearchUrl5.Name = "textBoxCustomSearchUrl5";
            this.textBoxCustomSearchUrl5.Size = new System.Drawing.Size(574, 21);
            this.textBoxCustomSearchUrl5.TabIndex = 14;
            // 
            // comboBoxCustomSearch5
            // 
            this.comboBoxCustomSearch5.FormattingEnabled = true;
            this.comboBoxCustomSearch5.Items.AddRange(new object[] {
            "Dictionary.com",
            "learnersdictionary.com",
            "Merriam-Webster",
            "The Free Dictionary",
            "Thesaurus.com",
            "urbandictionary.com",
            "VISUWORDS",
            "Wikipedia"});
            this.comboBoxCustomSearch5.Location = new System.Drawing.Point(31, 146);
            this.comboBoxCustomSearch5.Name = "comboBoxCustomSearch5";
            this.comboBoxCustomSearch5.Size = new System.Drawing.Size(148, 21);
            this.comboBoxCustomSearch5.TabIndex = 13;
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
            this.textBoxCustomSearchUrl4.Location = new System.Drawing.Point(185, 119);
            this.textBoxCustomSearchUrl4.Name = "textBoxCustomSearchUrl4";
            this.textBoxCustomSearchUrl4.Size = new System.Drawing.Size(574, 21);
            this.textBoxCustomSearchUrl4.TabIndex = 11;
            // 
            // comboBoxCustomSearch4
            // 
            this.comboBoxCustomSearch4.FormattingEnabled = true;
            this.comboBoxCustomSearch4.Items.AddRange(new object[] {
            "Dictionary.com",
            "learnersdictionary.com",
            "Merriam-Webster",
            "The Free Dictionary",
            "Thesaurus.com",
            "urbandictionary.com",
            "VISUWORDS",
            "Wikipedia"});
            this.comboBoxCustomSearch4.Location = new System.Drawing.Point(31, 119);
            this.comboBoxCustomSearch4.Name = "comboBoxCustomSearch4";
            this.comboBoxCustomSearch4.Size = new System.Drawing.Size(148, 21);
            this.comboBoxCustomSearch4.TabIndex = 10;
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
            this.textBoxCustomSearchUrl3.Location = new System.Drawing.Point(185, 92);
            this.textBoxCustomSearchUrl3.Name = "textBoxCustomSearchUrl3";
            this.textBoxCustomSearchUrl3.Size = new System.Drawing.Size(574, 21);
            this.textBoxCustomSearchUrl3.TabIndex = 8;
            // 
            // comboBoxCustomSearch3
            // 
            this.comboBoxCustomSearch3.FormattingEnabled = true;
            this.comboBoxCustomSearch3.Items.AddRange(new object[] {
            "Dictionary.com",
            "learnersdictionary.com",
            "Merriam-Webster",
            "The Free Dictionary",
            "Thesaurus.com",
            "urbandictionary.com",
            "VISUWORDS",
            "Wikipedia"});
            this.comboBoxCustomSearch3.Location = new System.Drawing.Point(31, 92);
            this.comboBoxCustomSearch3.Name = "comboBoxCustomSearch3";
            this.comboBoxCustomSearch3.Size = new System.Drawing.Size(148, 21);
            this.comboBoxCustomSearch3.TabIndex = 7;
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
            this.textBoxCustomSearchUrl2.Location = new System.Drawing.Point(185, 65);
            this.textBoxCustomSearchUrl2.Name = "textBoxCustomSearchUrl2";
            this.textBoxCustomSearchUrl2.Size = new System.Drawing.Size(574, 21);
            this.textBoxCustomSearchUrl2.TabIndex = 5;
            // 
            // comboBoxCustomSearch2
            // 
            this.comboBoxCustomSearch2.FormattingEnabled = true;
            this.comboBoxCustomSearch2.Items.AddRange(new object[] {
            "Dictionary.com",
            "learnersdictionary.com",
            "Merriam-Webster",
            "The Free Dictionary",
            "Thesaurus.com",
            "urbandictionary.com",
            "VISUWORDS",
            "Wikipedia"});
            this.comboBoxCustomSearch2.Location = new System.Drawing.Point(31, 65);
            this.comboBoxCustomSearch2.Name = "comboBoxCustomSearch2";
            this.comboBoxCustomSearch2.Size = new System.Drawing.Size(148, 21);
            this.comboBoxCustomSearch2.TabIndex = 4;
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
            this.textBoxCustomSearchUrl1.Location = new System.Drawing.Point(185, 38);
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
            this.comboBoxCustomSearch1.FormattingEnabled = true;
            this.comboBoxCustomSearch1.Items.AddRange(new object[] {
            "Dictionary.com",
            "learnersdictionary.com",
            "Merriam-Webster",
            "The Free Dictionary",
            "Thesaurus.com",
            "urbandictionary.com",
            "VISUWORDS",
            "Wikipedia"});
            this.comboBoxCustomSearch1.Location = new System.Drawing.Point(31, 38);
            this.comboBoxCustomSearch1.Name = "comboBoxCustomSearch1";
            this.comboBoxCustomSearch1.Size = new System.Drawing.Size(148, 21);
            this.comboBoxCustomSearch1.TabIndex = 0;
            this.comboBoxCustomSearch1.SelectedIndexChanged += new System.EventHandler(this.comboBoxCustomSearch_SelectedIndexChanged);
            // 
            // groupBoxVideoPlayerDefault
            // 
            this.groupBoxVideoPlayerDefault.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxVideoPlayerDefault.Controls.Add(this.checkBoxAllowVolumeBoost);
            this.groupBoxVideoPlayerDefault.Controls.Add(this.checkBoxVideoAutoOpen);
            this.groupBoxVideoPlayerDefault.Controls.Add(this.checkBoxVideoPlayerPreviewFontBold);
            this.groupBoxVideoPlayerDefault.Controls.Add(this.checkBoxVideoPlayerShowFullscreenButton);
            this.groupBoxVideoPlayerDefault.Controls.Add(this.checkBoxVideoPlayerShowMuteButton);
            this.groupBoxVideoPlayerDefault.Controls.Add(this.labelVideoPlayerPreviewFontSize);
            this.groupBoxVideoPlayerDefault.Controls.Add(this.comboBoxlVideoPlayerPreviewFontSize);
            this.groupBoxVideoPlayerDefault.Controls.Add(this.checkBoxVideoPlayerShowStopButton);
            this.groupBoxVideoPlayerDefault.Location = new System.Drawing.Point(7, 155);
            this.groupBoxVideoPlayerDefault.Name = "groupBoxVideoPlayerDefault";
            this.groupBoxVideoPlayerDefault.Size = new System.Drawing.Size(819, 120);
            this.groupBoxVideoPlayerDefault.TabIndex = 14;
            this.groupBoxVideoPlayerDefault.TabStop = false;
            // 
            // checkBoxAllowVolumeBoost
            // 
            this.checkBoxAllowVolumeBoost.AutoSize = true;
            this.checkBoxAllowVolumeBoost.Location = new System.Drawing.Point(334, 97);
            this.checkBoxAllowVolumeBoost.Name = "checkBoxAllowVolumeBoost";
            this.checkBoxAllowVolumeBoost.Size = new System.Drawing.Size(118, 17);
            this.checkBoxAllowVolumeBoost.TabIndex = 25;
            this.checkBoxAllowVolumeBoost.Text = "Allow volume boost";
            this.checkBoxAllowVolumeBoost.UseVisualStyleBackColor = true;
            // 
            // checkBoxVideoAutoOpen
            // 
            this.checkBoxVideoAutoOpen.AutoSize = true;
            this.checkBoxVideoAutoOpen.Location = new System.Drawing.Point(334, 74);
            this.checkBoxVideoAutoOpen.Name = "checkBoxVideoAutoOpen";
            this.checkBoxVideoAutoOpen.Size = new System.Drawing.Size(213, 17);
            this.checkBoxVideoAutoOpen.TabIndex = 24;
            this.checkBoxVideoAutoOpen.Text = "Auto open video when opening subtitle";
            this.checkBoxVideoAutoOpen.UseVisualStyleBackColor = true;
            // 
            // checkBoxVideoPlayerPreviewFontBold
            // 
            this.checkBoxVideoPlayerPreviewFontBold.AutoSize = true;
            this.checkBoxVideoPlayerPreviewFontBold.Location = new System.Drawing.Point(465, 45);
            this.checkBoxVideoPlayerPreviewFontBold.Name = "checkBoxVideoPlayerPreviewFontBold";
            this.checkBoxVideoPlayerPreviewFontBold.Size = new System.Drawing.Size(46, 17);
            this.checkBoxVideoPlayerPreviewFontBold.TabIndex = 23;
            this.checkBoxVideoPlayerPreviewFontBold.Text = "Bold";
            this.checkBoxVideoPlayerPreviewFontBold.UseVisualStyleBackColor = true;
            // 
            // checkBoxVideoPlayerShowFullscreenButton
            // 
            this.checkBoxVideoPlayerShowFullscreenButton.AutoSize = true;
            this.checkBoxVideoPlayerShowFullscreenButton.Location = new System.Drawing.Point(9, 65);
            this.checkBoxVideoPlayerShowFullscreenButton.Name = "checkBoxVideoPlayerShowFullscreenButton";
            this.checkBoxVideoPlayerShowFullscreenButton.Size = new System.Drawing.Size(136, 17);
            this.checkBoxVideoPlayerShowFullscreenButton.TabIndex = 12;
            this.checkBoxVideoPlayerShowFullscreenButton.Text = "Show fullscreen button";
            this.checkBoxVideoPlayerShowFullscreenButton.UseVisualStyleBackColor = true;
            // 
            // checkBoxVideoPlayerShowMuteButton
            // 
            this.checkBoxVideoPlayerShowMuteButton.AutoSize = true;
            this.checkBoxVideoPlayerShowMuteButton.Location = new System.Drawing.Point(9, 42);
            this.checkBoxVideoPlayerShowMuteButton.Name = "checkBoxVideoPlayerShowMuteButton";
            this.checkBoxVideoPlayerShowMuteButton.Size = new System.Drawing.Size(114, 17);
            this.checkBoxVideoPlayerShowMuteButton.TabIndex = 11;
            this.checkBoxVideoPlayerShowMuteButton.Text = "Show mute button";
            this.checkBoxVideoPlayerShowMuteButton.UseVisualStyleBackColor = true;
            // 
            // labelVideoPlayerPreviewFontSize
            // 
            this.labelVideoPlayerPreviewFontSize.AutoSize = true;
            this.labelVideoPlayerPreviewFontSize.Location = new System.Drawing.Point(331, 21);
            this.labelVideoPlayerPreviewFontSize.Name = "labelVideoPlayerPreviewFontSize";
            this.labelVideoPlayerPreviewFontSize.Size = new System.Drawing.Size(128, 13);
            this.labelVideoPlayerPreviewFontSize.TabIndex = 14;
            this.labelVideoPlayerPreviewFontSize.Text = "Subtitle preview font size";
            // 
            // comboBoxlVideoPlayerPreviewFontSize
            // 
            this.comboBoxlVideoPlayerPreviewFontSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxlVideoPlayerPreviewFontSize.FormattingEnabled = true;
            this.comboBoxlVideoPlayerPreviewFontSize.Items.AddRange(new object[] {
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
            "30"});
            this.comboBoxlVideoPlayerPreviewFontSize.Location = new System.Drawing.Point(465, 18);
            this.comboBoxlVideoPlayerPreviewFontSize.Name = "comboBoxlVideoPlayerPreviewFontSize";
            this.comboBoxlVideoPlayerPreviewFontSize.Size = new System.Drawing.Size(121, 21);
            this.comboBoxlVideoPlayerPreviewFontSize.TabIndex = 13;
            // 
            // checkBoxVideoPlayerShowStopButton
            // 
            this.checkBoxVideoPlayerShowStopButton.AutoSize = true;
            this.checkBoxVideoPlayerShowStopButton.Location = new System.Drawing.Point(9, 19);
            this.checkBoxVideoPlayerShowStopButton.Name = "checkBoxVideoPlayerShowStopButton";
            this.checkBoxVideoPlayerShowStopButton.Size = new System.Drawing.Size(111, 17);
            this.checkBoxVideoPlayerShowStopButton.TabIndex = 10;
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
            this.groupBoxVideoEngine.Location = new System.Drawing.Point(6, 6);
            this.groupBoxVideoEngine.Name = "groupBoxVideoEngine";
            this.groupBoxVideoEngine.Size = new System.Drawing.Size(820, 143);
            this.groupBoxVideoEngine.TabIndex = 0;
            this.groupBoxVideoEngine.TabStop = false;
            this.groupBoxVideoEngine.Text = "Video engine";
            // 
            // checkBoxMpvHandlesPreviewText
            // 
            this.checkBoxMpvHandlesPreviewText.AutoSize = true;
            this.checkBoxMpvHandlesPreviewText.Location = new System.Drawing.Point(169, 111);
            this.checkBoxMpvHandlesPreviewText.Name = "checkBoxMpvHandlesPreviewText";
            this.checkBoxMpvHandlesPreviewText.Size = new System.Drawing.Size(150, 17);
            this.checkBoxMpvHandlesPreviewText.TabIndex = 31;
            this.checkBoxMpvHandlesPreviewText.Text = "mpv handles preview text";
            this.checkBoxMpvHandlesPreviewText.UseVisualStyleBackColor = true;
            // 
            // labelMpvSettings
            // 
            this.labelMpvSettings.AutoSize = true;
            this.labelMpvSettings.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelMpvSettings.ForeColor = System.Drawing.Color.Gray;
            this.labelMpvSettings.Location = new System.Drawing.Point(709, 93);
            this.labelMpvSettings.Name = "labelMpvSettings";
            this.labelMpvSettings.Size = new System.Drawing.Size(40, 13);
            this.labelMpvSettings.TabIndex = 30;
            this.labelMpvSettings.Text = "--vo=?";
            // 
            // buttonMpvSettings
            // 
            this.buttonMpvSettings.Location = new System.Drawing.Point(510, 89);
            this.buttonMpvSettings.Name = "buttonMpvSettings";
            this.buttonMpvSettings.Size = new System.Drawing.Size(179, 23);
            this.buttonMpvSettings.TabIndex = 29;
            this.buttonMpvSettings.Text = "Download mpv dll";
            this.buttonMpvSettings.UseVisualStyleBackColor = true;
            this.buttonMpvSettings.Click += new System.EventHandler(this.buttonMpvSettings_Click);
            // 
            // labelPlatform
            // 
            this.labelPlatform.Font = new System.Drawing.Font("Tahoma", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelPlatform.ForeColor = System.Drawing.Color.Gray;
            this.labelPlatform.Location = new System.Drawing.Point(724, 13);
            this.labelPlatform.Name = "labelPlatform";
            this.labelPlatform.Size = new System.Drawing.Size(83, 11);
            this.labelPlatform.TabIndex = 27;
            this.labelPlatform.Text = "x-bit";
            this.labelPlatform.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // buttonVlcPathBrowse
            // 
            this.buttonVlcPathBrowse.Location = new System.Drawing.Point(778, 44);
            this.buttonVlcPathBrowse.Name = "buttonVlcPathBrowse";
            this.buttonVlcPathBrowse.Size = new System.Drawing.Size(29, 21);
            this.buttonVlcPathBrowse.TabIndex = 26;
            this.buttonVlcPathBrowse.Text = "...";
            this.buttonVlcPathBrowse.UseVisualStyleBackColor = true;
            this.buttonVlcPathBrowse.Click += new System.EventHandler(this.buttonVlcPathBrowse_Click);
            // 
            // textBoxVlcPath
            // 
            this.textBoxVlcPath.Location = new System.Drawing.Point(410, 45);
            this.textBoxVlcPath.MaxLength = 1000;
            this.textBoxVlcPath.Name = "textBoxVlcPath";
            this.textBoxVlcPath.Size = new System.Drawing.Size(362, 21);
            this.textBoxVlcPath.TabIndex = 25;
            this.textBoxVlcPath.MouseLeave += new System.EventHandler(this.textBoxVlcPath_MouseLeave);
            // 
            // labelVlcPath
            // 
            this.labelVlcPath.AutoSize = true;
            this.labelVlcPath.Location = new System.Drawing.Point(379, 29);
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
            this.labelVideoPlayerVLC.Location = new System.Drawing.Point(167, 49);
            this.labelVideoPlayerVLC.Name = "labelVideoPlayerVLC";
            this.labelVideoPlayerVLC.Size = new System.Drawing.Size(237, 13);
            this.labelVideoPlayerVLC.TabIndex = 13;
            this.labelVideoPlayerVLC.Text = "libvlc.dll from VLC media player (1.1.0 or newer)";
            // 
            // radioButtonVideoPlayerVLC
            // 
            this.radioButtonVideoPlayerVLC.AutoSize = true;
            this.radioButtonVideoPlayerVLC.Location = new System.Drawing.Point(10, 46);
            this.radioButtonVideoPlayerVLC.Name = "radioButtonVideoPlayerVLC";
            this.radioButtonVideoPlayerVLC.Size = new System.Drawing.Size(43, 17);
            this.radioButtonVideoPlayerVLC.TabIndex = 4;
            this.radioButtonVideoPlayerVLC.TabStop = true;
            this.radioButtonVideoPlayerVLC.Text = "VLC";
            this.radioButtonVideoPlayerVLC.UseVisualStyleBackColor = true;
            // 
            // labelVideoPlayerMPlayer
            // 
            this.labelVideoPlayerMPlayer.AutoSize = true;
            this.labelVideoPlayerMPlayer.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelVideoPlayerMPlayer.ForeColor = System.Drawing.Color.Gray;
            this.labelVideoPlayerMPlayer.Location = new System.Drawing.Point(167, 93);
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
            this.labelDirectShowDescription.Location = new System.Drawing.Point(167, 26);
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
            this.labelMpcHcDescription.Location = new System.Drawing.Point(167, 70);
            this.labelMpcHcDescription.Name = "labelMpcHcDescription";
            this.labelMpcHcDescription.Size = new System.Drawing.Size(178, 13);
            this.labelMpcHcDescription.TabIndex = 9;
            this.labelMpcHcDescription.Text = "Media Player Classic - Home Cinema";
            // 
            // radioButtonVideoPlayerMPV
            // 
            this.radioButtonVideoPlayerMPV.AutoSize = true;
            this.radioButtonVideoPlayerMPV.Location = new System.Drawing.Point(10, 89);
            this.radioButtonVideoPlayerMPV.Name = "radioButtonVideoPlayerMPV";
            this.radioButtonVideoPlayerMPV.Size = new System.Drawing.Size(45, 17);
            this.radioButtonVideoPlayerMPV.TabIndex = 28;
            this.radioButtonVideoPlayerMPV.TabStop = true;
            this.radioButtonVideoPlayerMPV.Text = "mpv";
            this.radioButtonVideoPlayerMPV.UseVisualStyleBackColor = true;
            this.radioButtonVideoPlayerMPV.CheckedChanged += new System.EventHandler(this.radioButtonVideoPlayerMPV_CheckedChanged);
            // 
            // radioButtonVideoPlayerDirectShow
            // 
            this.radioButtonVideoPlayerDirectShow.AutoSize = true;
            this.radioButtonVideoPlayerDirectShow.Location = new System.Drawing.Point(10, 23);
            this.radioButtonVideoPlayerDirectShow.Name = "radioButtonVideoPlayerDirectShow";
            this.radioButtonVideoPlayerDirectShow.Size = new System.Drawing.Size(82, 17);
            this.radioButtonVideoPlayerDirectShow.TabIndex = 1;
            this.radioButtonVideoPlayerDirectShow.TabStop = true;
            this.radioButtonVideoPlayerDirectShow.Text = "DirectShow ";
            this.radioButtonVideoPlayerDirectShow.UseVisualStyleBackColor = true;
            // 
            // radioButtonVideoPlayerMpcHc
            // 
            this.radioButtonVideoPlayerMpcHc.AutoSize = true;
            this.radioButtonVideoPlayerMpcHc.Location = new System.Drawing.Point(10, 66);
            this.radioButtonVideoPlayerMpcHc.Name = "radioButtonVideoPlayerMpcHc";
            this.radioButtonVideoPlayerMpcHc.Size = new System.Drawing.Size(64, 17);
            this.radioButtonVideoPlayerMpcHc.TabIndex = 27;
            this.radioButtonVideoPlayerMpcHc.TabStop = true;
            this.radioButtonVideoPlayerMpcHc.Text = "MPC-HC";
            this.radioButtonVideoPlayerMpcHc.UseVisualStyleBackColor = true;
            // 
            // tabPageWaveform
            // 
            this.tabPageWaveform.Controls.Add(this.groupBox3);
            this.tabPageWaveform.Controls.Add(this.groupBoxSpectrogram);
            this.tabPageWaveform.Controls.Add(this.groupBox1);
            this.tabPageWaveform.Controls.Add(this.groupBoxWaveformAppearence);
            this.tabPageWaveform.Location = new System.Drawing.Point(4, 22);
            this.tabPageWaveform.Name = "tabPageWaveform";
            this.tabPageWaveform.Size = new System.Drawing.Size(832, 483);
            this.tabPageWaveform.TabIndex = 6;
            this.tabPageWaveform.Text = "Waveform/spectrogram";
            this.tabPageWaveform.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.buttonDownloadFfmpeg);
            this.groupBox3.Controls.Add(this.buttonBrowseToFFmpeg);
            this.groupBox3.Controls.Add(this.textBoxFFmpegPath);
            this.groupBox3.Controls.Add(this.labelFFmpegPath);
            this.groupBox3.Controls.Add(this.checkBoxUseFFmpeg);
            this.groupBox3.Location = new System.Drawing.Point(406, 363);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(419, 102);
            this.groupBox3.TabIndex = 3;
            this.groupBox3.TabStop = false;
            // 
            // buttonDownloadFfmpeg
            // 
            this.buttonDownloadFfmpeg.Location = new System.Drawing.Point(274, 17);
            this.buttonDownloadFfmpeg.Name = "buttonDownloadFfmpeg";
            this.buttonDownloadFfmpeg.Size = new System.Drawing.Size(136, 21);
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
            this.textBoxFFmpegPath.Location = new System.Drawing.Point(9, 65);
            this.textBoxFFmpegPath.MaxLength = 1000;
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
            this.groupBoxSpectrogram.Location = new System.Drawing.Point(6, 254);
            this.groupBoxSpectrogram.Name = "groupBoxSpectrogram";
            this.groupBoxSpectrogram.Size = new System.Drawing.Size(819, 103);
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
            this.comboBoxSpectrogramAppearance.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSpectrogramAppearance.FormattingEnabled = true;
            this.comboBoxSpectrogramAppearance.Items.AddRange(new object[] {
            "Classic",
            "Use waveform color (one color gradient)"});
            this.comboBoxSpectrogramAppearance.Location = new System.Drawing.Point(10, 70);
            this.comboBoxSpectrogramAppearance.Name = "comboBoxSpectrogramAppearance";
            this.comboBoxSpectrogramAppearance.Size = new System.Drawing.Size(325, 21);
            this.comboBoxSpectrogramAppearance.TabIndex = 2;
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
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.buttonWaveformsFolderEmpty);
            this.groupBox1.Controls.Add(this.labelWaveformsFolderInfo);
            this.groupBox1.Location = new System.Drawing.Point(6, 363);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(394, 102);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
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
            this.groupBoxWaveformAppearence.Location = new System.Drawing.Point(6, 6);
            this.groupBoxWaveformAppearence.Name = "groupBoxWaveformAppearence";
            this.groupBoxWaveformAppearence.Size = new System.Drawing.Size(819, 242);
            this.groupBoxWaveformAppearence.TabIndex = 0;
            this.groupBoxWaveformAppearence.TabStop = false;
            this.groupBoxWaveformAppearence.Text = "Waveform appearance";
            // 
            // checkBoxWaveformShowWpm
            // 
            this.checkBoxWaveformShowWpm.AutoSize = true;
            this.checkBoxWaveformShowWpm.Location = new System.Drawing.Point(16, 218);
            this.checkBoxWaveformShowWpm.Name = "checkBoxWaveformShowWpm";
            this.checkBoxWaveformShowWpm.Size = new System.Drawing.Size(104, 17);
            this.checkBoxWaveformShowWpm.TabIndex = 7;
            this.checkBoxWaveformShowWpm.Text = "Show words/min";
            this.checkBoxWaveformShowWpm.UseVisualStyleBackColor = true;
            // 
            // checkBoxWaveformShowCps
            // 
            this.checkBoxWaveformShowCps.AutoSize = true;
            this.checkBoxWaveformShowCps.Location = new System.Drawing.Point(16, 195);
            this.checkBoxWaveformShowCps.Name = "checkBoxWaveformShowCps";
            this.checkBoxWaveformShowCps.Size = new System.Drawing.Size(96, 17);
            this.checkBoxWaveformShowCps.TabIndex = 6;
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
            this.labelWaveformTextSize.Location = new System.Drawing.Point(259, 154);
            this.labelWaveformTextSize.Name = "labelWaveformTextSize";
            this.labelWaveformTextSize.Size = new System.Drawing.Size(73, 13);
            this.labelWaveformTextSize.TabIndex = 23;
            this.labelWaveformTextSize.Text = "Text font size";
            // 
            // checkBoxWaveformTextBold
            // 
            this.checkBoxWaveformTextBold.AutoSize = true;
            this.checkBoxWaveformTextBold.Location = new System.Drawing.Point(463, 155);
            this.checkBoxWaveformTextBold.Name = "checkBoxWaveformTextBold";
            this.checkBoxWaveformTextBold.Size = new System.Drawing.Size(46, 17);
            this.checkBoxWaveformTextBold.TabIndex = 31;
            this.checkBoxWaveformTextBold.Text = "Bold";
            this.checkBoxWaveformTextBold.UseVisualStyleBackColor = true;
            // 
            // comboBoxWaveformTextSize
            // 
            this.comboBoxWaveformTextSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxWaveformTextSize.FormattingEnabled = true;
            this.comboBoxWaveformTextSize.Items.AddRange(new object[] {
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
            this.comboBoxWaveformTextSize.Location = new System.Drawing.Point(336, 151);
            this.comboBoxWaveformTextSize.Name = "comboBoxWaveformTextSize";
            this.comboBoxWaveformTextSize.Size = new System.Drawing.Size(121, 21);
            this.comboBoxWaveformTextSize.TabIndex = 30;
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
            this.labelWaveformBorderHitMs2.Location = new System.Drawing.Point(454, 195);
            this.labelWaveformBorderHitMs2.Name = "labelWaveformBorderHitMs2";
            this.labelWaveformBorderHitMs2.Size = new System.Drawing.Size(62, 13);
            this.labelWaveformBorderHitMs2.TabIndex = 14;
            this.labelWaveformBorderHitMs2.Text = "milliseconds";
            // 
            // numericUpDownWaveformBorderHitMs
            // 
            this.numericUpDownWaveformBorderHitMs.Location = new System.Drawing.Point(392, 193);
            this.numericUpDownWaveformBorderHitMs.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDownWaveformBorderHitMs.Name = "numericUpDownWaveformBorderHitMs";
            this.numericUpDownWaveformBorderHitMs.Size = new System.Drawing.Size(44, 21);
            this.numericUpDownWaveformBorderHitMs.TabIndex = 35;
            this.numericUpDownWaveformBorderHitMs.Value = new decimal(new int[] {
            18,
            0,
            0,
            0});
            // 
            // labelWaveformBorderHitMs1
            // 
            this.labelWaveformBorderHitMs1.AutoSize = true;
            this.labelWaveformBorderHitMs1.Location = new System.Drawing.Point(259, 195);
            this.labelWaveformBorderHitMs1.Name = "labelWaveformBorderHitMs1";
            this.labelWaveformBorderHitMs1.Size = new System.Drawing.Size(127, 13);
            this.labelWaveformBorderHitMs1.TabIndex = 13;
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
            this.panelWaveformTextColor.Location = new System.Drawing.Point(138, 109);
            this.panelWaveformTextColor.Name = "panelWaveformTextColor";
            this.panelWaveformTextColor.Size = new System.Drawing.Size(21, 20);
            this.panelWaveformTextColor.TabIndex = 7;
            this.panelWaveformTextColor.Click += new System.EventHandler(this.buttonWaveformTextColor_Click);
            // 
            // buttonWaveformTextColor
            // 
            this.buttonWaveformTextColor.Location = new System.Drawing.Point(16, 108);
            this.buttonWaveformTextColor.Name = "buttonWaveformTextColor";
            this.buttonWaveformTextColor.Size = new System.Drawing.Size(112, 21);
            this.buttonWaveformTextColor.TabIndex = 3;
            this.buttonWaveformTextColor.Text = "Text color";
            this.buttonWaveformTextColor.UseVisualStyleBackColor = true;
            this.buttonWaveformTextColor.Click += new System.EventHandler(this.buttonWaveformTextColor_Click);
            // 
            // panelWaveformGridColor
            // 
            this.panelWaveformGridColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelWaveformGridColor.Location = new System.Drawing.Point(138, 135);
            this.panelWaveformGridColor.Name = "panelWaveformGridColor";
            this.panelWaveformGridColor.Size = new System.Drawing.Size(21, 20);
            this.panelWaveformGridColor.TabIndex = 9;
            this.panelWaveformGridColor.Click += new System.EventHandler(this.buttonWaveformGridColor_Click);
            // 
            // buttonWaveformGridColor
            // 
            this.buttonWaveformGridColor.Location = new System.Drawing.Point(16, 135);
            this.buttonWaveformGridColor.Name = "buttonWaveformGridColor";
            this.buttonWaveformGridColor.Size = new System.Drawing.Size(112, 21);
            this.buttonWaveformGridColor.TabIndex = 4;
            this.buttonWaveformGridColor.Text = "Grid color";
            this.buttonWaveformGridColor.UseVisualStyleBackColor = true;
            this.buttonWaveformGridColor.Click += new System.EventHandler(this.buttonWaveformGridColor_Click);
            // 
            // panelWaveformBackgroundColor
            // 
            this.panelWaveformBackgroundColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelWaveformBackgroundColor.Location = new System.Drawing.Point(138, 82);
            this.panelWaveformBackgroundColor.Name = "panelWaveformBackgroundColor";
            this.panelWaveformBackgroundColor.Size = new System.Drawing.Size(21, 20);
            this.panelWaveformBackgroundColor.TabIndex = 5;
            this.panelWaveformBackgroundColor.Click += new System.EventHandler(this.buttonWaveformBackgroundColor_Click);
            // 
            // buttonWaveformBackgroundColor
            // 
            this.buttonWaveformBackgroundColor.Location = new System.Drawing.Point(16, 81);
            this.buttonWaveformBackgroundColor.Name = "buttonWaveformBackgroundColor";
            this.buttonWaveformBackgroundColor.Size = new System.Drawing.Size(112, 21);
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
            this.buttonWaveformColor.Location = new System.Drawing.Point(16, 54);
            this.buttonWaveformColor.Name = "buttonWaveformColor";
            this.buttonWaveformColor.Size = new System.Drawing.Size(112, 21);
            this.buttonWaveformColor.TabIndex = 1;
            this.buttonWaveformColor.Text = "Color";
            this.buttonWaveformColor.UseVisualStyleBackColor = true;
            this.buttonWaveformColor.Click += new System.EventHandler(this.buttonWaveformColor_Click);
            // 
            // panelWaveformSelectedColor
            // 
            this.panelWaveformSelectedColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelWaveformSelectedColor.Location = new System.Drawing.Point(138, 27);
            this.panelWaveformSelectedColor.Name = "panelWaveformSelectedColor";
            this.panelWaveformSelectedColor.Size = new System.Drawing.Size(21, 20);
            this.panelWaveformSelectedColor.TabIndex = 1;
            this.panelWaveformSelectedColor.MouseClick += new System.Windows.Forms.MouseEventHandler(this.buttonWaveformSelectedColor_Click);
            // 
            // buttonWaveformSelectedColor
            // 
            this.buttonWaveformSelectedColor.Location = new System.Drawing.Point(16, 27);
            this.buttonWaveformSelectedColor.Name = "buttonWaveformSelectedColor";
            this.buttonWaveformSelectedColor.Size = new System.Drawing.Size(112, 21);
            this.buttonWaveformSelectedColor.TabIndex = 0;
            this.buttonWaveformSelectedColor.Text = "Selected color";
            this.buttonWaveformSelectedColor.UseVisualStyleBackColor = true;
            this.buttonWaveformSelectedColor.Click += new System.EventHandler(this.buttonWaveformSelectedColor_Click);
            // 
            // checkBoxWaveformShowGrid
            // 
            this.checkBoxWaveformShowGrid.AutoSize = true;
            this.checkBoxWaveformShowGrid.Location = new System.Drawing.Point(16, 162);
            this.checkBoxWaveformShowGrid.Name = "checkBoxWaveformShowGrid";
            this.checkBoxWaveformShowGrid.Size = new System.Drawing.Size(73, 17);
            this.checkBoxWaveformShowGrid.TabIndex = 5;
            this.checkBoxWaveformShowGrid.Text = "Show grid";
            this.checkBoxWaveformShowGrid.UseVisualStyleBackColor = true;
            // 
            // tabPageTools
            // 
            this.tabPageTools.Controls.Add(this.groupBoxGoogleTranslate);
            this.tabPageTools.Controls.Add(this.groupBoxBing);
            this.tabPageTools.Controls.Add(this.groupBoxToolsAutoBr);
            this.tabPageTools.Controls.Add(this.groupBoxSpellCheck);
            this.tabPageTools.Controls.Add(this.groupBoxFixCommonErrors);
            this.tabPageTools.Controls.Add(this.groupBoxToolsVisualSync);
            this.tabPageTools.Location = new System.Drawing.Point(4, 22);
            this.tabPageTools.Name = "tabPageTools";
            this.tabPageTools.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageTools.Size = new System.Drawing.Size(832, 483);
            this.tabPageTools.TabIndex = 5;
            this.tabPageTools.Text = "Tools";
            this.tabPageTools.UseVisualStyleBackColor = true;
            // 
            // groupBoxGoogleTranslate
            // 
            this.groupBoxGoogleTranslate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxGoogleTranslate.Controls.Add(this.labelGoogleTranslateApiKey);
            this.groupBoxGoogleTranslate.Controls.Add(this.textBoxGoogleTransleApiKey);
            this.groupBoxGoogleTranslate.Controls.Add(this.linkLabelGoogleTranslateSignUp);
            this.groupBoxGoogleTranslate.Controls.Add(this.label3);
            this.groupBoxGoogleTranslate.Location = new System.Drawing.Point(421, 244);
            this.groupBoxGoogleTranslate.Name = "groupBoxGoogleTranslate";
            this.groupBoxGoogleTranslate.Size = new System.Drawing.Size(404, 78);
            this.groupBoxGoogleTranslate.TabIndex = 31;
            this.groupBoxGoogleTranslate.TabStop = false;
            this.groupBoxGoogleTranslate.Text = "Google translate";
            // 
            // labelGoogleTranslateApiKey
            // 
            this.labelGoogleTranslateApiKey.AutoSize = true;
            this.labelGoogleTranslateApiKey.Location = new System.Drawing.Point(13, 47);
            this.labelGoogleTranslateApiKey.Name = "labelGoogleTranslateApiKey";
            this.labelGoogleTranslateApiKey.Size = new System.Drawing.Size(44, 13);
            this.labelGoogleTranslateApiKey.TabIndex = 30;
            this.labelGoogleTranslateApiKey.Text = "API key";
            // 
            // textBoxGoogleTransleApiKey
            // 
            this.textBoxGoogleTransleApiKey.Location = new System.Drawing.Point(94, 47);
            this.textBoxGoogleTransleApiKey.Name = "textBoxGoogleTransleApiKey";
            this.textBoxGoogleTransleApiKey.Size = new System.Drawing.Size(304, 21);
            this.textBoxGoogleTransleApiKey.TabIndex = 26;
            // 
            // linkLabelGoogleTranslateSignUp
            // 
            this.linkLabelGoogleTranslateSignUp.Location = new System.Drawing.Point(201, 15);
            this.linkLabelGoogleTranslateSignUp.Name = "linkLabelGoogleTranslateSignUp";
            this.linkLabelGoogleTranslateSignUp.Size = new System.Drawing.Size(183, 27);
            this.linkLabelGoogleTranslateSignUp.TabIndex = 24;
            this.linkLabelGoogleTranslateSignUp.TabStop = true;
            this.linkLabelGoogleTranslateSignUp.Text = "How to sign up";
            this.linkLabelGoogleTranslateSignUp.TextAlign = System.Drawing.ContentAlignment.TopRight;
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
            this.groupBoxBing.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxBing.Controls.Add(this.labelBingTokenEndpoint);
            this.groupBoxBing.Controls.Add(this.textBoxBingTokenEndpoint);
            this.groupBoxBing.Controls.Add(this.labelBingApiKey);
            this.groupBoxBing.Controls.Add(this.textBoxBingClientSecret);
            this.groupBoxBing.Controls.Add(this.linkLabelBingSubscribe);
            this.groupBoxBing.Controls.Add(this.label1);
            this.groupBoxBing.Location = new System.Drawing.Point(421, 331);
            this.groupBoxBing.Name = "groupBoxBing";
            this.groupBoxBing.Size = new System.Drawing.Size(404, 131);
            this.groupBoxBing.TabIndex = 32;
            this.groupBoxBing.TabStop = false;
            this.groupBoxBing.Text = "Bing translator";
            // 
            // labelBingTokenEndpoint
            // 
            this.labelBingTokenEndpoint.AutoSize = true;
            this.labelBingTokenEndpoint.Location = new System.Drawing.Point(6, 82);
            this.labelBingTokenEndpoint.Name = "labelBingTokenEndpoint";
            this.labelBingTokenEndpoint.Size = new System.Drawing.Size(81, 13);
            this.labelBingTokenEndpoint.TabIndex = 32;
            this.labelBingTokenEndpoint.Text = "Token endpoint";
            // 
            // textBoxBingTokenEndpoint
            // 
            this.textBoxBingTokenEndpoint.Location = new System.Drawing.Point(10, 98);
            this.textBoxBingTokenEndpoint.Name = "textBoxBingTokenEndpoint";
            this.textBoxBingTokenEndpoint.Size = new System.Drawing.Size(374, 21);
            this.textBoxBingTokenEndpoint.TabIndex = 31;
            // 
            // labelBingApiKey
            // 
            this.labelBingApiKey.AutoSize = true;
            this.labelBingApiKey.Location = new System.Drawing.Point(6, 29);
            this.labelBingApiKey.Name = "labelBingApiKey";
            this.labelBingApiKey.Size = new System.Drawing.Size(44, 13);
            this.labelBingApiKey.TabIndex = 30;
            this.labelBingApiKey.Text = "API key";
            // 
            // textBoxBingClientSecret
            // 
            this.textBoxBingClientSecret.Location = new System.Drawing.Point(10, 45);
            this.textBoxBingClientSecret.Name = "textBoxBingClientSecret";
            this.textBoxBingClientSecret.Size = new System.Drawing.Size(374, 21);
            this.textBoxBingClientSecret.TabIndex = 26;
            // 
            // linkLabelBingSubscribe
            // 
            this.linkLabelBingSubscribe.Location = new System.Drawing.Point(201, 15);
            this.linkLabelBingSubscribe.Name = "linkLabelBingSubscribe";
            this.linkLabelBingSubscribe.Size = new System.Drawing.Size(183, 27);
            this.linkLabelBingSubscribe.TabIndex = 24;
            this.linkLabelBingSubscribe.TabStop = true;
            this.linkLabelBingSubscribe.Text = "How to sign up";
            this.linkLabelBingSubscribe.TextAlign = System.Drawing.ContentAlignment.TopRight;
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
            this.groupBoxToolsAutoBr.Location = new System.Drawing.Point(421, 6);
            this.groupBoxToolsAutoBr.Name = "groupBoxToolsAutoBr";
            this.groupBoxToolsAutoBr.Size = new System.Drawing.Size(404, 232);
            this.groupBoxToolsAutoBr.TabIndex = 5;
            this.groupBoxToolsAutoBr.TabStop = false;
            this.groupBoxToolsAutoBr.Text = "Auto br";
            // 
            // labelToolsBreakBottomHeavyPercent
            // 
            this.labelToolsBreakBottomHeavyPercent.AutoSize = true;
            this.labelToolsBreakBottomHeavyPercent.Location = new System.Drawing.Point(226, 142);
            this.labelToolsBreakBottomHeavyPercent.Name = "labelToolsBreakBottomHeavyPercent";
            this.labelToolsBreakBottomHeavyPercent.Size = new System.Drawing.Size(18, 13);
            this.labelToolsBreakBottomHeavyPercent.TabIndex = 63;
            this.labelToolsBreakBottomHeavyPercent.Text = "%";
            // 
            // numericUpDownToolsBreakPreferBottomHeavy
            // 
            this.numericUpDownToolsBreakPreferBottomHeavy.DecimalPlaces = 1;
            this.numericUpDownToolsBreakPreferBottomHeavy.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericUpDownToolsBreakPreferBottomHeavy.Location = new System.Drawing.Point(164, 140);
            this.numericUpDownToolsBreakPreferBottomHeavy.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numericUpDownToolsBreakPreferBottomHeavy.Name = "numericUpDownToolsBreakPreferBottomHeavy";
            this.numericUpDownToolsBreakPreferBottomHeavy.Size = new System.Drawing.Size(56, 21);
            this.numericUpDownToolsBreakPreferBottomHeavy.TabIndex = 62;
            this.numericUpDownToolsBreakPreferBottomHeavy.Value = new decimal(new int[] {
            11,
            0,
            0,
            0});
            // 
            // checkBoxToolsBreakPreferBottomHeavy
            // 
            this.checkBoxToolsBreakPreferBottomHeavy.AutoSize = true;
            this.checkBoxToolsBreakPreferBottomHeavy.Location = new System.Drawing.Point(32, 141);
            this.checkBoxToolsBreakPreferBottomHeavy.Name = "checkBoxToolsBreakPreferBottomHeavy";
            this.checkBoxToolsBreakPreferBottomHeavy.Size = new System.Drawing.Size(126, 17);
            this.checkBoxToolsBreakPreferBottomHeavy.TabIndex = 61;
            this.checkBoxToolsBreakPreferBottomHeavy.Text = "Prefer bottom heavy";
            this.checkBoxToolsBreakPreferBottomHeavy.UseVisualStyleBackColor = true;
            // 
            // checkBoxToolsBreakByPixelWidth
            // 
            this.checkBoxToolsBreakByPixelWidth.AutoSize = true;
            this.checkBoxToolsBreakByPixelWidth.Location = new System.Drawing.Point(15, 118);
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
            this.checkBoxToolsBreakEarlyLineEnding.Location = new System.Drawing.Point(15, 72);
            this.checkBoxToolsBreakEarlyLineEnding.Name = "checkBoxToolsBreakEarlyLineEnding";
            this.checkBoxToolsBreakEarlyLineEnding.Size = new System.Drawing.Size(175, 17);
            this.checkBoxToolsBreakEarlyLineEnding.TabIndex = 40;
            this.checkBoxToolsBreakEarlyLineEnding.Text = "Break early for line ending (.!?)";
            this.checkBoxToolsBreakEarlyLineEnding.UseVisualStyleBackColor = true;
            // 
            // checkBoxToolsBreakEarlyComma
            // 
            this.checkBoxToolsBreakEarlyComma.AutoSize = true;
            this.checkBoxToolsBreakEarlyComma.Location = new System.Drawing.Point(15, 95);
            this.checkBoxToolsBreakEarlyComma.Name = "checkBoxToolsBreakEarlyComma";
            this.checkBoxToolsBreakEarlyComma.Size = new System.Drawing.Size(133, 17);
            this.checkBoxToolsBreakEarlyComma.TabIndex = 50;
            this.checkBoxToolsBreakEarlyComma.Text = "Break early for comma";
            this.checkBoxToolsBreakEarlyComma.UseVisualStyleBackColor = true;
            // 
            // checkBoxToolsBreakEarlyDash
            // 
            this.checkBoxToolsBreakEarlyDash.AutoSize = true;
            this.checkBoxToolsBreakEarlyDash.Location = new System.Drawing.Point(15, 49);
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
            this.buttonEditDoNotBreakAfterList.Location = new System.Drawing.Point(239, 22);
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
            this.checkBoxUseDoNotBreakAfterList.Location = new System.Drawing.Point(15, 26);
            this.checkBoxUseDoNotBreakAfterList.Name = "checkBoxUseDoNotBreakAfterList";
            this.checkBoxUseDoNotBreakAfterList.Size = new System.Drawing.Size(154, 17);
            this.checkBoxUseDoNotBreakAfterList.TabIndex = 1;
            this.checkBoxUseDoNotBreakAfterList.Text = "Use \'do-not-beak-after\' list";
            this.checkBoxUseDoNotBreakAfterList.UseVisualStyleBackColor = true;
            // 
            // groupBoxSpellCheck
            // 
            this.groupBoxSpellCheck.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxSpellCheck.Controls.Add(this.checkBoxUseAlwaysToFile);
            this.groupBoxSpellCheck.Controls.Add(this.checkBoxTreatINQuoteAsING);
            this.groupBoxSpellCheck.Controls.Add(this.checkBoxSpellCheckOneLetterWords);
            this.groupBoxSpellCheck.Controls.Add(this.checkBoxSpellCheckAutoChangeNames);
            this.groupBoxSpellCheck.Location = new System.Drawing.Point(7, 331);
            this.groupBoxSpellCheck.Name = "groupBoxSpellCheck";
            this.groupBoxSpellCheck.Size = new System.Drawing.Size(408, 131);
            this.groupBoxSpellCheck.TabIndex = 4;
            this.groupBoxSpellCheck.TabStop = false;
            this.groupBoxSpellCheck.Text = "Spell check";
            // 
            // checkBoxUseAlwaysToFile
            // 
            this.checkBoxUseAlwaysToFile.AutoSize = true;
            this.checkBoxUseAlwaysToFile.Location = new System.Drawing.Point(16, 86);
            this.checkBoxUseAlwaysToFile.Name = "checkBoxUseAlwaysToFile";
            this.checkBoxUseAlwaysToFile.Size = new System.Drawing.Size(192, 17);
            this.checkBoxUseAlwaysToFile.TabIndex = 3;
            this.checkBoxUseAlwaysToFile.Text = "Remember \"Use always\" / \"Skip all\"";
            this.checkBoxUseAlwaysToFile.UseVisualStyleBackColor = true;
            // 
            // checkBoxTreatINQuoteAsING
            // 
            this.checkBoxTreatINQuoteAsING.AutoSize = true;
            this.checkBoxTreatINQuoteAsING.Location = new System.Drawing.Point(16, 66);
            this.checkBoxTreatINQuoteAsING.Name = "checkBoxTreatINQuoteAsING";
            this.checkBoxTreatINQuoteAsING.Size = new System.Drawing.Size(253, 17);
            this.checkBoxTreatINQuoteAsING.TabIndex = 2;
            this.checkBoxTreatINQuoteAsING.Text = "Treat word ending \" in\' \" as \" ing \" (English only)";
            this.checkBoxTreatINQuoteAsING.UseVisualStyleBackColor = true;
            // 
            // checkBoxSpellCheckOneLetterWords
            // 
            this.checkBoxSpellCheckOneLetterWords.AutoSize = true;
            this.checkBoxSpellCheckOneLetterWords.Location = new System.Drawing.Point(16, 43);
            this.checkBoxSpellCheckOneLetterWords.Name = "checkBoxSpellCheckOneLetterWords";
            this.checkBoxSpellCheckOneLetterWords.Size = new System.Drawing.Size(205, 17);
            this.checkBoxSpellCheckOneLetterWords.TabIndex = 1;
            this.checkBoxSpellCheckOneLetterWords.Text = "Prompt for unknown one letter words";
            this.checkBoxSpellCheckOneLetterWords.UseVisualStyleBackColor = true;
            // 
            // checkBoxSpellCheckAutoChangeNames
            // 
            this.checkBoxSpellCheckAutoChangeNames.AutoSize = true;
            this.checkBoxSpellCheckAutoChangeNames.Location = new System.Drawing.Point(15, 20);
            this.checkBoxSpellCheckAutoChangeNames.Name = "checkBoxSpellCheckAutoChangeNames";
            this.checkBoxSpellCheckAutoChangeNames.Size = new System.Drawing.Size(216, 17);
            this.checkBoxSpellCheckAutoChangeNames.TabIndex = 0;
            this.checkBoxSpellCheckAutoChangeNames.Text = "Auto fix names where only casing differ";
            this.checkBoxSpellCheckAutoChangeNames.UseVisualStyleBackColor = true;
            // 
            // groupBoxFixCommonErrors
            // 
            this.groupBoxFixCommonErrors.Controls.Add(this.checkBoxFceSkipStep1);
            this.groupBoxFixCommonErrors.Controls.Add(this.checkBoxFixShortDisplayTimesAllowMoveStartTime);
            this.groupBoxFixCommonErrors.Controls.Add(this.checkBoxFixCommonOcrErrorsUsingHardcodedRules);
            this.groupBoxFixCommonErrors.Controls.Add(this.comboBoxToolsMusicSymbol);
            this.groupBoxFixCommonErrors.Controls.Add(this.textBoxMusicSymbolsToReplace);
            this.groupBoxFixCommonErrors.Controls.Add(this.labelToolsMusicSymbolsToReplace);
            this.groupBoxFixCommonErrors.Controls.Add(this.labelToolsMusicSymbol);
            this.groupBoxFixCommonErrors.Location = new System.Drawing.Point(7, 129);
            this.groupBoxFixCommonErrors.Name = "groupBoxFixCommonErrors";
            this.groupBoxFixCommonErrors.Size = new System.Drawing.Size(408, 196);
            this.groupBoxFixCommonErrors.TabIndex = 3;
            this.groupBoxFixCommonErrors.TabStop = false;
            this.groupBoxFixCommonErrors.Text = "Fix common errors";
            // 
            // checkBoxFceSkipStep1
            // 
            this.checkBoxFceSkipStep1.AutoSize = true;
            this.checkBoxFceSkipStep1.Location = new System.Drawing.Point(15, 161);
            this.checkBoxFceSkipStep1.Name = "checkBoxFceSkipStep1";
            this.checkBoxFceSkipStep1.Size = new System.Drawing.Size(176, 17);
            this.checkBoxFceSkipStep1.TabIndex = 36;
            this.checkBoxFceSkipStep1.Text = "Skip step one (choose fix rules)";
            this.checkBoxFceSkipStep1.UseVisualStyleBackColor = true;
            // 
            // checkBoxFixShortDisplayTimesAllowMoveStartTime
            // 
            this.checkBoxFixShortDisplayTimesAllowMoveStartTime.AutoSize = true;
            this.checkBoxFixShortDisplayTimesAllowMoveStartTime.Location = new System.Drawing.Point(15, 138);
            this.checkBoxFixShortDisplayTimesAllowMoveStartTime.Name = "checkBoxFixShortDisplayTimesAllowMoveStartTime";
            this.checkBoxFixShortDisplayTimesAllowMoveStartTime.Size = new System.Drawing.Size(252, 17);
            this.checkBoxFixShortDisplayTimesAllowMoveStartTime.TabIndex = 35;
            this.checkBoxFixShortDisplayTimesAllowMoveStartTime.Text = "Fix short display time - allow move of start time";
            this.checkBoxFixShortDisplayTimesAllowMoveStartTime.UseVisualStyleBackColor = true;
            // 
            // checkBoxFixCommonOcrErrorsUsingHardcodedRules
            // 
            this.checkBoxFixCommonOcrErrorsUsingHardcodedRules.AutoSize = true;
            this.checkBoxFixCommonOcrErrorsUsingHardcodedRules.Location = new System.Drawing.Point(15, 115);
            this.checkBoxFixCommonOcrErrorsUsingHardcodedRules.Name = "checkBoxFixCommonOcrErrorsUsingHardcodedRules";
            this.checkBoxFixCommonOcrErrorsUsingHardcodedRules.Size = new System.Drawing.Size(268, 17);
            this.checkBoxFixCommonOcrErrorsUsingHardcodedRules.TabIndex = 2;
            this.checkBoxFixCommonOcrErrorsUsingHardcodedRules.Text = "Fix common OCR errors - also use hardcoded rules";
            this.checkBoxFixCommonOcrErrorsUsingHardcodedRules.UseVisualStyleBackColor = true;
            // 
            // comboBoxToolsMusicSymbol
            // 
            this.comboBoxToolsMusicSymbol.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxToolsMusicSymbol.FormattingEnabled = true;
            this.comboBoxToolsMusicSymbol.Items.AddRange(new object[] {
            "♪",
            "♪♪",
            "*",
            "#"});
            this.comboBoxToolsMusicSymbol.Location = new System.Drawing.Point(200, 71);
            this.comboBoxToolsMusicSymbol.Name = "comboBoxToolsMusicSymbol";
            this.comboBoxToolsMusicSymbol.Size = new System.Drawing.Size(86, 21);
            this.comboBoxToolsMusicSymbol.TabIndex = 1;
            // 
            // textBoxMusicSymbolsToReplace
            // 
            this.textBoxMusicSymbolsToReplace.Location = new System.Drawing.Point(11, 41);
            this.textBoxMusicSymbolsToReplace.MaxLength = 100;
            this.textBoxMusicSymbolsToReplace.Name = "textBoxMusicSymbolsToReplace";
            this.textBoxMusicSymbolsToReplace.Size = new System.Drawing.Size(274, 21);
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
            this.groupBoxToolsVisualSync.Location = new System.Drawing.Point(6, 6);
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
            this.comboBoxToolsEndSceneIndex.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxToolsEndSceneIndex.FormattingEnabled = true;
            this.comboBoxToolsEndSceneIndex.Items.AddRange(new object[] {
            "Last",
            "Last - 1",
            "Last - 2",
            "Last - 3"});
            this.comboBoxToolsEndSceneIndex.Location = new System.Drawing.Point(200, 76);
            this.comboBoxToolsEndSceneIndex.Name = "comboBoxToolsEndSceneIndex";
            this.comboBoxToolsEndSceneIndex.Size = new System.Drawing.Size(86, 21);
            this.comboBoxToolsEndSceneIndex.TabIndex = 2;
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
            this.comboBoxToolsStartSceneIndex.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxToolsStartSceneIndex.FormattingEnabled = true;
            this.comboBoxToolsStartSceneIndex.Items.AddRange(new object[] {
            "First",
            "First +1",
            "First +2",
            "First +3"});
            this.comboBoxToolsStartSceneIndex.Location = new System.Drawing.Point(200, 49);
            this.comboBoxToolsStartSceneIndex.Name = "comboBoxToolsStartSceneIndex";
            this.comboBoxToolsStartSceneIndex.Size = new System.Drawing.Size(86, 21);
            this.comboBoxToolsStartSceneIndex.TabIndex = 1;
            // 
            // comboBoxToolsVerifySeconds
            // 
            this.comboBoxToolsVerifySeconds.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxToolsVerifySeconds.FormattingEnabled = true;
            this.comboBoxToolsVerifySeconds.Items.AddRange(new object[] {
            "2",
            "3",
            "4",
            "5"});
            this.comboBoxToolsVerifySeconds.Location = new System.Drawing.Point(200, 22);
            this.comboBoxToolsVerifySeconds.Name = "comboBoxToolsVerifySeconds";
            this.comboBoxToolsVerifySeconds.Size = new System.Drawing.Size(86, 21);
            this.comboBoxToolsVerifySeconds.TabIndex = 0;
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
            // tabPageWordLists
            // 
            this.tabPageWordLists.Controls.Add(this.groupBoxWordLists);
            this.tabPageWordLists.Location = new System.Drawing.Point(4, 22);
            this.tabPageWordLists.Name = "tabPageWordLists";
            this.tabPageWordLists.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageWordLists.Size = new System.Drawing.Size(832, 483);
            this.tabPageWordLists.TabIndex = 3;
            this.tabPageWordLists.Text = "Word lists";
            this.tabPageWordLists.UseVisualStyleBackColor = true;
            // 
            // groupBoxWordLists
            // 
            this.groupBoxWordLists.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxWordLists.Controls.Add(this.linkLabelOpenDictionaryFolder);
            this.groupBoxWordLists.Controls.Add(this.groupBoxOcrFixList);
            this.groupBoxWordLists.Controls.Add(this.groupBoxUserWordList);
            this.groupBoxWordLists.Controls.Add(this.groupBoxWordListLocation);
            this.groupBoxWordLists.Controls.Add(this.groupBoxNamesIgonoreLists);
            this.groupBoxWordLists.Controls.Add(this.labelWordListLanguage);
            this.groupBoxWordLists.Controls.Add(this.comboBoxWordListLanguage);
            this.groupBoxWordLists.Location = new System.Drawing.Point(6, 6);
            this.groupBoxWordLists.Name = "groupBoxWordLists";
            this.groupBoxWordLists.Size = new System.Drawing.Size(819, 475);
            this.groupBoxWordLists.TabIndex = 2;
            this.groupBoxWordLists.TabStop = false;
            this.groupBoxWordLists.Text = "Word lists";
            // 
            // linkLabelOpenDictionaryFolder
            // 
            this.linkLabelOpenDictionaryFolder.AutoSize = true;
            this.linkLabelOpenDictionaryFolder.Location = new System.Drawing.Point(6, 413);
            this.linkLabelOpenDictionaryFolder.Name = "linkLabelOpenDictionaryFolder";
            this.linkLabelOpenDictionaryFolder.Size = new System.Drawing.Size(126, 13);
            this.linkLabelOpenDictionaryFolder.TabIndex = 29;
            this.linkLabelOpenDictionaryFolder.TabStop = true;
            this.linkLabelOpenDictionaryFolder.Text = "Open \'Dictionaries\' folder";
            this.linkLabelOpenDictionaryFolder.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelOpenDictionaryFolder_LinkClicked);
            // 
            // groupBoxOcrFixList
            // 
            this.groupBoxOcrFixList.Controls.Add(this.textBoxOcrFixValue);
            this.groupBoxOcrFixList.Controls.Add(this.buttonRemoveOcrFix);
            this.groupBoxOcrFixList.Controls.Add(this.listBoxOcrFixList);
            this.groupBoxOcrFixList.Controls.Add(this.textBoxOcrFixKey);
            this.groupBoxOcrFixList.Controls.Add(this.buttonAddOcrFix);
            this.groupBoxOcrFixList.Location = new System.Drawing.Point(510, 43);
            this.groupBoxOcrFixList.Name = "groupBoxOcrFixList";
            this.groupBoxOcrFixList.Size = new System.Drawing.Size(293, 267);
            this.groupBoxOcrFixList.TabIndex = 6;
            this.groupBoxOcrFixList.TabStop = false;
            this.groupBoxOcrFixList.Text = "OCR fix list";
            // 
            // textBoxOcrFixValue
            // 
            this.textBoxOcrFixValue.Location = new System.Drawing.Point(100, 238);
            this.textBoxOcrFixValue.Name = "textBoxOcrFixValue";
            this.textBoxOcrFixValue.Size = new System.Drawing.Size(85, 21);
            this.textBoxOcrFixValue.TabIndex = 45;
            this.textBoxOcrFixValue.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBoxOcrFixValueKeyDown);
            // 
            // buttonRemoveOcrFix
            // 
            this.buttonRemoveOcrFix.Location = new System.Drawing.Point(191, 16);
            this.buttonRemoveOcrFix.Name = "buttonRemoveOcrFix";
            this.buttonRemoveOcrFix.Size = new System.Drawing.Size(75, 23);
            this.buttonRemoveOcrFix.TabIndex = 42;
            this.buttonRemoveOcrFix.Text = "Remove";
            this.buttonRemoveOcrFix.UseVisualStyleBackColor = true;
            this.buttonRemoveOcrFix.Click += new System.EventHandler(this.ButtonRemoveOcrFixClick);
            // 
            // listBoxOcrFixList
            // 
            this.listBoxOcrFixList.FormattingEnabled = true;
            this.listBoxOcrFixList.Location = new System.Drawing.Point(6, 16);
            this.listBoxOcrFixList.Name = "listBoxOcrFixList";
            this.listBoxOcrFixList.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listBoxOcrFixList.Size = new System.Drawing.Size(179, 186);
            this.listBoxOcrFixList.TabIndex = 40;
            this.listBoxOcrFixList.SelectedIndexChanged += new System.EventHandler(this.ListBoxOcrFixListSelectedIndexChanged);
            this.listBoxOcrFixList.Enter += new System.EventHandler(this.ListBoxSearchReset);
            this.listBoxOcrFixList.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ListBoxKeyDownSearch);
            // 
            // textBoxOcrFixKey
            // 
            this.textBoxOcrFixKey.Location = new System.Drawing.Point(6, 238);
            this.textBoxOcrFixKey.Name = "textBoxOcrFixKey";
            this.textBoxOcrFixKey.Size = new System.Drawing.Size(88, 21);
            this.textBoxOcrFixKey.TabIndex = 44;
            // 
            // buttonAddOcrFix
            // 
            this.buttonAddOcrFix.Location = new System.Drawing.Point(191, 236);
            this.buttonAddOcrFix.Name = "buttonAddOcrFix";
            this.buttonAddOcrFix.Size = new System.Drawing.Size(75, 23);
            this.buttonAddOcrFix.TabIndex = 46;
            this.buttonAddOcrFix.Text = "Add pair";
            this.buttonAddOcrFix.UseVisualStyleBackColor = true;
            this.buttonAddOcrFix.Click += new System.EventHandler(this.ButtonAddOcrFixClick);
            // 
            // groupBoxUserWordList
            // 
            this.groupBoxUserWordList.Controls.Add(this.buttonRemoveUserWord);
            this.groupBoxUserWordList.Controls.Add(this.listBoxUserWordLists);
            this.groupBoxUserWordList.Controls.Add(this.textBoxUserWord);
            this.groupBoxUserWordList.Controls.Add(this.buttonAddUserWord);
            this.groupBoxUserWordList.Location = new System.Drawing.Point(259, 43);
            this.groupBoxUserWordList.Name = "groupBoxUserWordList";
            this.groupBoxUserWordList.Size = new System.Drawing.Size(241, 267);
            this.groupBoxUserWordList.TabIndex = 4;
            this.groupBoxUserWordList.TabStop = false;
            this.groupBoxUserWordList.Text = "User word list";
            // 
            // buttonRemoveUserWord
            // 
            this.buttonRemoveUserWord.Location = new System.Drawing.Point(159, 16);
            this.buttonRemoveUserWord.Name = "buttonRemoveUserWord";
            this.buttonRemoveUserWord.Size = new System.Drawing.Size(75, 23);
            this.buttonRemoveUserWord.TabIndex = 32;
            this.buttonRemoveUserWord.Text = "Remove";
            this.buttonRemoveUserWord.UseVisualStyleBackColor = true;
            this.buttonRemoveUserWord.Click += new System.EventHandler(this.ButtonRemoveUserWordClick);
            // 
            // listBoxUserWordLists
            // 
            this.listBoxUserWordLists.FormattingEnabled = true;
            this.listBoxUserWordLists.Location = new System.Drawing.Point(3, 16);
            this.listBoxUserWordLists.Name = "listBoxUserWordLists";
            this.listBoxUserWordLists.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listBoxUserWordLists.Size = new System.Drawing.Size(150, 186);
            this.listBoxUserWordLists.TabIndex = 30;
            this.listBoxUserWordLists.SelectedIndexChanged += new System.EventHandler(this.ListBoxUserWordListsSelectedIndexChanged);
            this.listBoxUserWordLists.Enter += new System.EventHandler(this.ListBoxSearchReset);
            this.listBoxUserWordLists.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ListBoxKeyDownSearch);
            // 
            // textBoxUserWord
            // 
            this.textBoxUserWord.Location = new System.Drawing.Point(3, 238);
            this.textBoxUserWord.Name = "textBoxUserWord";
            this.textBoxUserWord.Size = new System.Drawing.Size(147, 21);
            this.textBoxUserWord.TabIndex = 34;
            this.textBoxUserWord.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBoxUserWordKeyDown);
            // 
            // buttonAddUserWord
            // 
            this.buttonAddUserWord.Location = new System.Drawing.Point(156, 236);
            this.buttonAddUserWord.Name = "buttonAddUserWord";
            this.buttonAddUserWord.Size = new System.Drawing.Size(75, 23);
            this.buttonAddUserWord.TabIndex = 36;
            this.buttonAddUserWord.Text = "Add word";
            this.buttonAddUserWord.UseVisualStyleBackColor = true;
            this.buttonAddUserWord.Click += new System.EventHandler(this.ButtonAddUserWordClick);
            // 
            // groupBoxWordListLocation
            // 
            this.groupBoxWordListLocation.Controls.Add(this.checkBoxNamesOnline);
            this.groupBoxWordListLocation.Controls.Add(this.textBoxNamesOnline);
            this.groupBoxWordListLocation.Location = new System.Drawing.Point(6, 316);
            this.groupBoxWordListLocation.Name = "groupBoxWordListLocation";
            this.groupBoxWordListLocation.Size = new System.Drawing.Size(797, 82);
            this.groupBoxWordListLocation.TabIndex = 8;
            this.groupBoxWordListLocation.TabStop = false;
            this.groupBoxWordListLocation.Text = "Location";
            // 
            // checkBoxNamesOnline
            // 
            this.checkBoxNamesOnline.AutoSize = true;
            this.checkBoxNamesOnline.Location = new System.Drawing.Point(7, 22);
            this.checkBoxNamesOnline.Name = "checkBoxNamesOnline";
            this.checkBoxNamesOnline.Size = new System.Drawing.Size(145, 17);
            this.checkBoxNamesOnline.TabIndex = 26;
            this.checkBoxNamesOnline.Text = "Use online names xml file";
            this.checkBoxNamesOnline.UseVisualStyleBackColor = true;
            // 
            // textBoxNamesOnline
            // 
            this.textBoxNamesOnline.Location = new System.Drawing.Point(6, 45);
            this.textBoxNamesOnline.Name = "textBoxNamesOnline";
            this.textBoxNamesOnline.Size = new System.Drawing.Size(488, 21);
            this.textBoxNamesOnline.TabIndex = 28;
            this.textBoxNamesOnline.Text = "https://raw.githubusercontent.com/SubtitleEdit/subtitleedit/master/Dictionaries/n" +
    "ames.xml";
            // 
            // groupBoxNamesIgonoreLists
            // 
            this.groupBoxNamesIgonoreLists.Controls.Add(this.buttonRemoveNameEtc);
            this.groupBoxNamesIgonoreLists.Controls.Add(this.listBoxNames);
            this.groupBoxNamesIgonoreLists.Controls.Add(this.textBoxNameEtc);
            this.groupBoxNamesIgonoreLists.Controls.Add(this.buttonAddNames);
            this.groupBoxNamesIgonoreLists.Location = new System.Drawing.Point(6, 43);
            this.groupBoxNamesIgonoreLists.Name = "groupBoxNamesIgonoreLists";
            this.groupBoxNamesIgonoreLists.Size = new System.Drawing.Size(241, 267);
            this.groupBoxNamesIgonoreLists.TabIndex = 2;
            this.groupBoxNamesIgonoreLists.TabStop = false;
            this.groupBoxNamesIgonoreLists.Text = "Names/ignore lists";
            // 
            // buttonRemoveNameEtc
            // 
            this.buttonRemoveNameEtc.Location = new System.Drawing.Point(159, 16);
            this.buttonRemoveNameEtc.Name = "buttonRemoveNameEtc";
            this.buttonRemoveNameEtc.Size = new System.Drawing.Size(75, 23);
            this.buttonRemoveNameEtc.TabIndex = 22;
            this.buttonRemoveNameEtc.Text = "Remove";
            this.buttonRemoveNameEtc.UseVisualStyleBackColor = true;
            this.buttonRemoveNameEtc.Click += new System.EventHandler(this.ButtonRemoveNameEtcClick);
            // 
            // listBoxNames
            // 
            this.listBoxNames.FormattingEnabled = true;
            this.listBoxNames.Location = new System.Drawing.Point(3, 16);
            this.listBoxNames.Name = "listBoxNames";
            this.listBoxNames.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listBoxNames.Size = new System.Drawing.Size(150, 186);
            this.listBoxNames.TabIndex = 20;
            this.listBoxNames.SelectedIndexChanged += new System.EventHandler(this.ListBoxNamesSelectedIndexChanged);
            this.listBoxNames.Enter += new System.EventHandler(this.ListBoxSearchReset);
            this.listBoxNames.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ListBoxKeyDownSearch);
            // 
            // textBoxNameEtc
            // 
            this.textBoxNameEtc.Location = new System.Drawing.Point(3, 241);
            this.textBoxNameEtc.Name = "textBoxNameEtc";
            this.textBoxNameEtc.Size = new System.Drawing.Size(151, 21);
            this.textBoxNameEtc.TabIndex = 24;
            this.textBoxNameEtc.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBoxNameEtcKeyDown);
            // 
            // buttonAddNames
            // 
            this.buttonAddNames.Location = new System.Drawing.Point(160, 238);
            this.buttonAddNames.Name = "buttonAddNames";
            this.buttonAddNames.Size = new System.Drawing.Size(75, 23);
            this.buttonAddNames.TabIndex = 26;
            this.buttonAddNames.Text = "Add name";
            this.buttonAddNames.UseVisualStyleBackColor = true;
            this.buttonAddNames.Click += new System.EventHandler(this.ButtonAddNamesClick);
            // 
            // labelWordListLanguage
            // 
            this.labelWordListLanguage.AutoSize = true;
            this.labelWordListLanguage.Location = new System.Drawing.Point(6, 19);
            this.labelWordListLanguage.Name = "labelWordListLanguage";
            this.labelWordListLanguage.Size = new System.Drawing.Size(54, 13);
            this.labelWordListLanguage.TabIndex = 1;
            this.labelWordListLanguage.Text = "Language";
            // 
            // comboBoxWordListLanguage
            // 
            this.comboBoxWordListLanguage.FormattingEnabled = true;
            this.comboBoxWordListLanguage.Location = new System.Drawing.Point(67, 16);
            this.comboBoxWordListLanguage.Name = "comboBoxWordListLanguage";
            this.comboBoxWordListLanguage.Size = new System.Drawing.Size(155, 21);
            this.comboBoxWordListLanguage.TabIndex = 0;
            this.comboBoxWordListLanguage.SelectedIndexChanged += new System.EventHandler(this.ComboBoxWordListLanguageSelectedIndexChanged);
            // 
            // tabPageToolBar
            // 
            this.tabPageToolBar.Controls.Add(this.groupBox2);
            this.tabPageToolBar.Controls.Add(this.groupBoxShowToolBarButtons);
            this.tabPageToolBar.Location = new System.Drawing.Point(4, 22);
            this.tabPageToolBar.Name = "tabPageToolBar";
            this.tabPageToolBar.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageToolBar.Size = new System.Drawing.Size(832, 483);
            this.tabPageToolBar.TabIndex = 7;
            this.tabPageToolBar.Text = "Toolbar ";
            this.tabPageToolBar.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.checkBoxShowFrameRate);
            this.groupBox2.Location = new System.Drawing.Point(7, 248);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(818, 214);
            this.groupBox2.TabIndex = 35;
            this.groupBox2.TabStop = false;
            // 
            // checkBoxShowFrameRate
            // 
            this.checkBoxShowFrameRate.AutoSize = true;
            this.checkBoxShowFrameRate.Location = new System.Drawing.Point(16, 20);
            this.checkBoxShowFrameRate.Name = "checkBoxShowFrameRate";
            this.checkBoxShowFrameRate.Size = new System.Drawing.Size(154, 17);
            this.checkBoxShowFrameRate.TabIndex = 34;
            this.checkBoxShowFrameRate.Text = "Show frame rate in toolbar";
            this.checkBoxShowFrameRate.UseVisualStyleBackColor = true;
            // 
            // groupBoxShowToolBarButtons
            // 
            this.groupBoxShowToolBarButtons.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxShowToolBarButtons.Controls.Add(this.labelTBNetflixQualityCheck);
            this.groupBoxShowToolBarButtons.Controls.Add(this.pictureBoxNetflixQualityCheck);
            this.groupBoxShowToolBarButtons.Controls.Add(this.checkBoxNetflixQualityCheck);
            this.groupBoxShowToolBarButtons.Controls.Add(this.labelTBRemoveTextForHi);
            this.groupBoxShowToolBarButtons.Controls.Add(this.pictureBoxTBRemoveTextForHi);
            this.groupBoxShowToolBarButtons.Controls.Add(this.checkBoxTBRemoveTextForHi);
            this.groupBoxShowToolBarButtons.Controls.Add(this.labelTBFixCommonErrors);
            this.groupBoxShowToolBarButtons.Controls.Add(this.pictureBoxTBFixCommonErrors);
            this.groupBoxShowToolBarButtons.Controls.Add(this.checkBoxTBFixCommonErrors);
            this.groupBoxShowToolBarButtons.Controls.Add(this.labelTBHelp);
            this.groupBoxShowToolBarButtons.Controls.Add(this.pictureBoxHelp);
            this.groupBoxShowToolBarButtons.Controls.Add(this.checkBoxHelp);
            this.groupBoxShowToolBarButtons.Controls.Add(this.labelTBSettings);
            this.groupBoxShowToolBarButtons.Controls.Add(this.pictureBoxSettings);
            this.groupBoxShowToolBarButtons.Controls.Add(this.checkBoxSettings);
            this.groupBoxShowToolBarButtons.Controls.Add(this.labelTBSpellCheck);
            this.groupBoxShowToolBarButtons.Controls.Add(this.pictureBoxSpellCheck);
            this.groupBoxShowToolBarButtons.Controls.Add(this.checkBoxSpellCheck);
            this.groupBoxShowToolBarButtons.Controls.Add(this.labelTBVisualSync);
            this.groupBoxShowToolBarButtons.Controls.Add(this.pictureBoxVisualSync);
            this.groupBoxShowToolBarButtons.Controls.Add(this.checkBoxVisualSync);
            this.groupBoxShowToolBarButtons.Controls.Add(this.labelTBReplace);
            this.groupBoxShowToolBarButtons.Controls.Add(this.pictureBoxReplace);
            this.groupBoxShowToolBarButtons.Controls.Add(this.checkBoxReplace);
            this.groupBoxShowToolBarButtons.Controls.Add(this.labelTBFind);
            this.groupBoxShowToolBarButtons.Controls.Add(this.pictureBoxFind);
            this.groupBoxShowToolBarButtons.Controls.Add(this.checkBoxToolbarFind);
            this.groupBoxShowToolBarButtons.Controls.Add(this.labelTBSaveAs);
            this.groupBoxShowToolBarButtons.Controls.Add(this.pictureBoxSaveAs);
            this.groupBoxShowToolBarButtons.Controls.Add(this.checkBoxToolbarSaveAs);
            this.groupBoxShowToolBarButtons.Controls.Add(this.labelTBSave);
            this.groupBoxShowToolBarButtons.Controls.Add(this.pictureBoxSave);
            this.groupBoxShowToolBarButtons.Controls.Add(this.checkBoxToolbarSave);
            this.groupBoxShowToolBarButtons.Controls.Add(this.labelTBOpen);
            this.groupBoxShowToolBarButtons.Controls.Add(this.pictureBoxOpen);
            this.groupBoxShowToolBarButtons.Controls.Add(this.checkBoxToolbarOpen);
            this.groupBoxShowToolBarButtons.Controls.Add(this.labelTBNew);
            this.groupBoxShowToolBarButtons.Controls.Add(this.pictureBoxNew);
            this.groupBoxShowToolBarButtons.Controls.Add(this.checkBoxToolbarNew);
            this.groupBoxShowToolBarButtons.Location = new System.Drawing.Point(6, 6);
            this.groupBoxShowToolBarButtons.Name = "groupBoxShowToolBarButtons";
            this.groupBoxShowToolBarButtons.Size = new System.Drawing.Size(819, 236);
            this.groupBoxShowToolBarButtons.TabIndex = 0;
            this.groupBoxShowToolBarButtons.TabStop = false;
            this.groupBoxShowToolBarButtons.Text = "Show toolbar buttons";
            // 
            // labelTBNetflixQualityCheck
            // 
            this.labelTBNetflixQualityCheck.AutoSize = true;
            this.labelTBNetflixQualityCheck.Location = new System.Drawing.Point(636, 136);
            this.labelTBNetflixQualityCheck.Name = "labelTBNetflixQualityCheck";
            this.labelTBNetflixQualityCheck.Size = new System.Drawing.Size(103, 13);
            this.labelTBNetflixQualityCheck.TabIndex = 42;
            this.labelTBNetflixQualityCheck.Text = "Netflix quality check";
            // 
            // pictureBoxNetflixQualityCheck
            // 
            this.pictureBoxNetflixQualityCheck.Location = new System.Drawing.Point(649, 155);
            this.pictureBoxNetflixQualityCheck.Name = "pictureBoxNetflixQualityCheck";
            this.pictureBoxNetflixQualityCheck.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxNetflixQualityCheck.TabIndex = 41;
            this.pictureBoxNetflixQualityCheck.TabStop = false;
            // 
            // checkBoxNetflixQualityCheck
            // 
            this.checkBoxNetflixQualityCheck.AutoSize = true;
            this.checkBoxNetflixQualityCheck.Location = new System.Drawing.Point(652, 195);
            this.checkBoxNetflixQualityCheck.Name = "checkBoxNetflixQualityCheck";
            this.checkBoxNetflixQualityCheck.Size = new System.Drawing.Size(55, 17);
            this.checkBoxNetflixQualityCheck.TabIndex = 40;
            this.checkBoxNetflixQualityCheck.Text = "Visible";
            this.checkBoxNetflixQualityCheck.UseVisualStyleBackColor = true;
            // 
            // labelTBRemoveTextForHi
            // 
            this.labelTBRemoveTextForHi.AutoSize = true;
            this.labelTBRemoveTextForHi.Location = new System.Drawing.Point(124, 136);
            this.labelTBRemoveTextForHi.Name = "labelTBRemoveTextForHi";
            this.labelTBRemoveTextForHi.Size = new System.Drawing.Size(100, 13);
            this.labelTBRemoveTextForHi.TabIndex = 39;
            this.labelTBRemoveTextForHi.Text = "Remove text for HI";
            // 
            // pictureBoxTBRemoveTextForHi
            // 
            this.pictureBoxTBRemoveTextForHi.Location = new System.Drawing.Point(137, 155);
            this.pictureBoxTBRemoveTextForHi.Name = "pictureBoxTBRemoveTextForHi";
            this.pictureBoxTBRemoveTextForHi.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxTBRemoveTextForHi.TabIndex = 38;
            this.pictureBoxTBRemoveTextForHi.TabStop = false;
            // 
            // checkBoxTBRemoveTextForHi
            // 
            this.checkBoxTBRemoveTextForHi.AutoSize = true;
            this.checkBoxTBRemoveTextForHi.Location = new System.Drawing.Point(140, 195);
            this.checkBoxTBRemoveTextForHi.Name = "checkBoxTBRemoveTextForHi";
            this.checkBoxTBRemoveTextForHi.Size = new System.Drawing.Size(55, 17);
            this.checkBoxTBRemoveTextForHi.TabIndex = 18;
            this.checkBoxTBRemoveTextForHi.Text = "Visible";
            this.checkBoxTBRemoveTextForHi.UseVisualStyleBackColor = true;
            // 
            // labelTBFixCommonErrors
            // 
            this.labelTBFixCommonErrors.AutoSize = true;
            this.labelTBFixCommonErrors.Location = new System.Drawing.Point(9, 136);
            this.labelTBFixCommonErrors.Name = "labelTBFixCommonErrors";
            this.labelTBFixCommonErrors.Size = new System.Drawing.Size(95, 13);
            this.labelTBFixCommonErrors.TabIndex = 36;
            this.labelTBFixCommonErrors.Text = "Fix common errors";
            // 
            // pictureBoxTBFixCommonErrors
            // 
            this.pictureBoxTBFixCommonErrors.Location = new System.Drawing.Point(22, 155);
            this.pictureBoxTBFixCommonErrors.Name = "pictureBoxTBFixCommonErrors";
            this.pictureBoxTBFixCommonErrors.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxTBFixCommonErrors.TabIndex = 35;
            this.pictureBoxTBFixCommonErrors.TabStop = false;
            // 
            // checkBoxTBFixCommonErrors
            // 
            this.checkBoxTBFixCommonErrors.AutoSize = true;
            this.checkBoxTBFixCommonErrors.Location = new System.Drawing.Point(25, 195);
            this.checkBoxTBFixCommonErrors.Name = "checkBoxTBFixCommonErrors";
            this.checkBoxTBFixCommonErrors.Size = new System.Drawing.Size(55, 17);
            this.checkBoxTBFixCommonErrors.TabIndex = 17;
            this.checkBoxTBFixCommonErrors.Text = "Visible";
            this.checkBoxTBFixCommonErrors.UseVisualStyleBackColor = true;
            // 
            // labelTBHelp
            // 
            this.labelTBHelp.AutoSize = true;
            this.labelTBHelp.Location = new System.Drawing.Point(561, 136);
            this.labelTBHelp.Name = "labelTBHelp";
            this.labelTBHelp.Size = new System.Drawing.Size(28, 13);
            this.labelTBHelp.TabIndex = 33;
            this.labelTBHelp.Text = "Help";
            // 
            // pictureBoxHelp
            // 
            this.pictureBoxHelp.Location = new System.Drawing.Point(560, 155);
            this.pictureBoxHelp.Name = "pictureBoxHelp";
            this.pictureBoxHelp.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxHelp.TabIndex = 32;
            this.pictureBoxHelp.TabStop = false;
            // 
            // checkBoxHelp
            // 
            this.checkBoxHelp.AutoSize = true;
            this.checkBoxHelp.Location = new System.Drawing.Point(563, 195);
            this.checkBoxHelp.Name = "checkBoxHelp";
            this.checkBoxHelp.Size = new System.Drawing.Size(55, 17);
            this.checkBoxHelp.TabIndex = 22;
            this.checkBoxHelp.Text = "Visible";
            this.checkBoxHelp.UseVisualStyleBackColor = true;
            // 
            // labelTBSettings
            // 
            this.labelTBSettings.AutoSize = true;
            this.labelTBSettings.Location = new System.Drawing.Point(456, 136);
            this.labelTBSettings.Name = "labelTBSettings";
            this.labelTBSettings.Size = new System.Drawing.Size(46, 13);
            this.labelTBSettings.TabIndex = 30;
            this.labelTBSettings.Text = "Settings";
            // 
            // pictureBoxSettings
            // 
            this.pictureBoxSettings.Location = new System.Drawing.Point(459, 155);
            this.pictureBoxSettings.Name = "pictureBoxSettings";
            this.pictureBoxSettings.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxSettings.TabIndex = 29;
            this.pictureBoxSettings.TabStop = false;
            // 
            // checkBoxSettings
            // 
            this.checkBoxSettings.AutoSize = true;
            this.checkBoxSettings.Location = new System.Drawing.Point(462, 195);
            this.checkBoxSettings.Name = "checkBoxSettings";
            this.checkBoxSettings.Size = new System.Drawing.Size(55, 17);
            this.checkBoxSettings.TabIndex = 21;
            this.checkBoxSettings.Text = "Visible";
            this.checkBoxSettings.UseVisualStyleBackColor = true;
            // 
            // labelTBSpellCheck
            // 
            this.labelTBSpellCheck.AutoSize = true;
            this.labelTBSpellCheck.Location = new System.Drawing.Point(357, 136);
            this.labelTBSpellCheck.Name = "labelTBSpellCheck";
            this.labelTBSpellCheck.Size = new System.Drawing.Size(59, 13);
            this.labelTBSpellCheck.TabIndex = 27;
            this.labelTBSpellCheck.Text = "Spell check";
            // 
            // pictureBoxSpellCheck
            // 
            this.pictureBoxSpellCheck.Location = new System.Drawing.Point(361, 155);
            this.pictureBoxSpellCheck.Name = "pictureBoxSpellCheck";
            this.pictureBoxSpellCheck.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxSpellCheck.TabIndex = 26;
            this.pictureBoxSpellCheck.TabStop = false;
            // 
            // checkBoxSpellCheck
            // 
            this.checkBoxSpellCheck.AutoSize = true;
            this.checkBoxSpellCheck.Location = new System.Drawing.Point(362, 195);
            this.checkBoxSpellCheck.Name = "checkBoxSpellCheck";
            this.checkBoxSpellCheck.Size = new System.Drawing.Size(55, 17);
            this.checkBoxSpellCheck.TabIndex = 20;
            this.checkBoxSpellCheck.Text = "Visible";
            this.checkBoxSpellCheck.UseVisualStyleBackColor = true;
            // 
            // labelTBVisualSync
            // 
            this.labelTBVisualSync.AutoSize = true;
            this.labelTBVisualSync.Location = new System.Drawing.Point(247, 136);
            this.labelTBVisualSync.Name = "labelTBVisualSync";
            this.labelTBVisualSync.Size = new System.Drawing.Size(59, 13);
            this.labelTBVisualSync.TabIndex = 21;
            this.labelTBVisualSync.Text = "Visual sync";
            // 
            // pictureBoxVisualSync
            // 
            this.pictureBoxVisualSync.Location = new System.Drawing.Point(260, 155);
            this.pictureBoxVisualSync.Name = "pictureBoxVisualSync";
            this.pictureBoxVisualSync.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxVisualSync.TabIndex = 20;
            this.pictureBoxVisualSync.TabStop = false;
            // 
            // checkBoxVisualSync
            // 
            this.checkBoxVisualSync.AutoSize = true;
            this.checkBoxVisualSync.Location = new System.Drawing.Point(263, 195);
            this.checkBoxVisualSync.Name = "checkBoxVisualSync";
            this.checkBoxVisualSync.Size = new System.Drawing.Size(55, 17);
            this.checkBoxVisualSync.TabIndex = 19;
            this.checkBoxVisualSync.Text = "Visible";
            this.checkBoxVisualSync.UseVisualStyleBackColor = true;
            // 
            // labelTBReplace
            // 
            this.labelTBReplace.AutoSize = true;
            this.labelTBReplace.Location = new System.Drawing.Point(521, 22);
            this.labelTBReplace.Name = "labelTBReplace";
            this.labelTBReplace.Size = new System.Drawing.Size(45, 13);
            this.labelTBReplace.TabIndex = 18;
            this.labelTBReplace.Text = "Replace";
            // 
            // pictureBoxReplace
            // 
            this.pictureBoxReplace.Location = new System.Drawing.Point(526, 41);
            this.pictureBoxReplace.Name = "pictureBoxReplace";
            this.pictureBoxReplace.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxReplace.TabIndex = 17;
            this.pictureBoxReplace.TabStop = false;
            // 
            // checkBoxReplace
            // 
            this.checkBoxReplace.AutoSize = true;
            this.checkBoxReplace.Location = new System.Drawing.Point(529, 81);
            this.checkBoxReplace.Name = "checkBoxReplace";
            this.checkBoxReplace.Size = new System.Drawing.Size(55, 17);
            this.checkBoxReplace.TabIndex = 16;
            this.checkBoxReplace.Text = "Visible";
            this.checkBoxReplace.UseVisualStyleBackColor = true;
            // 
            // labelTBFind
            // 
            this.labelTBFind.AutoSize = true;
            this.labelTBFind.Location = new System.Drawing.Point(425, 22);
            this.labelTBFind.Name = "labelTBFind";
            this.labelTBFind.Size = new System.Drawing.Size(27, 13);
            this.labelTBFind.TabIndex = 15;
            this.labelTBFind.Text = "Find";
            // 
            // pictureBoxFind
            // 
            this.pictureBoxFind.Location = new System.Drawing.Point(423, 41);
            this.pictureBoxFind.Name = "pictureBoxFind";
            this.pictureBoxFind.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxFind.TabIndex = 14;
            this.pictureBoxFind.TabStop = false;
            // 
            // checkBoxToolbarFind
            // 
            this.checkBoxToolbarFind.AutoSize = true;
            this.checkBoxToolbarFind.Location = new System.Drawing.Point(426, 81);
            this.checkBoxToolbarFind.Name = "checkBoxToolbarFind";
            this.checkBoxToolbarFind.Size = new System.Drawing.Size(55, 17);
            this.checkBoxToolbarFind.TabIndex = 13;
            this.checkBoxToolbarFind.Text = "Visible";
            this.checkBoxToolbarFind.UseVisualStyleBackColor = true;
            // 
            // labelTBSaveAs
            // 
            this.labelTBSaveAs.AutoSize = true;
            this.labelTBSaveAs.Location = new System.Drawing.Point(316, 22);
            this.labelTBSaveAs.Name = "labelTBSaveAs";
            this.labelTBSaveAs.Size = new System.Drawing.Size(45, 13);
            this.labelTBSaveAs.TabIndex = 12;
            this.labelTBSaveAs.Text = "Save as";
            // 
            // pictureBoxSaveAs
            // 
            this.pictureBoxSaveAs.Location = new System.Drawing.Point(322, 41);
            this.pictureBoxSaveAs.Name = "pictureBoxSaveAs";
            this.pictureBoxSaveAs.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxSaveAs.TabIndex = 11;
            this.pictureBoxSaveAs.TabStop = false;
            // 
            // checkBoxToolbarSaveAs
            // 
            this.checkBoxToolbarSaveAs.AutoSize = true;
            this.checkBoxToolbarSaveAs.Location = new System.Drawing.Point(325, 81);
            this.checkBoxToolbarSaveAs.Name = "checkBoxToolbarSaveAs";
            this.checkBoxToolbarSaveAs.Size = new System.Drawing.Size(55, 17);
            this.checkBoxToolbarSaveAs.TabIndex = 10;
            this.checkBoxToolbarSaveAs.Text = "Visible";
            this.checkBoxToolbarSaveAs.UseVisualStyleBackColor = true;
            // 
            // labelTBSave
            // 
            this.labelTBSave.AutoSize = true;
            this.labelTBSave.Location = new System.Drawing.Point(225, 22);
            this.labelTBSave.Name = "labelTBSave";
            this.labelTBSave.Size = new System.Drawing.Size(31, 13);
            this.labelTBSave.TabIndex = 9;
            this.labelTBSave.Text = "Save";
            // 
            // pictureBoxSave
            // 
            this.pictureBoxSave.Location = new System.Drawing.Point(224, 41);
            this.pictureBoxSave.Name = "pictureBoxSave";
            this.pictureBoxSave.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxSave.TabIndex = 8;
            this.pictureBoxSave.TabStop = false;
            // 
            // checkBoxToolbarSave
            // 
            this.checkBoxToolbarSave.AutoSize = true;
            this.checkBoxToolbarSave.Location = new System.Drawing.Point(227, 81);
            this.checkBoxToolbarSave.Name = "checkBoxToolbarSave";
            this.checkBoxToolbarSave.Size = new System.Drawing.Size(55, 17);
            this.checkBoxToolbarSave.TabIndex = 7;
            this.checkBoxToolbarSave.Text = "Visible";
            this.checkBoxToolbarSave.UseVisualStyleBackColor = true;
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
            // pictureBoxOpen
            // 
            this.pictureBoxOpen.Location = new System.Drawing.Point(123, 41);
            this.pictureBoxOpen.Name = "pictureBoxOpen";
            this.pictureBoxOpen.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxOpen.TabIndex = 5;
            this.pictureBoxOpen.TabStop = false;
            // 
            // checkBoxToolbarOpen
            // 
            this.checkBoxToolbarOpen.AutoSize = true;
            this.checkBoxToolbarOpen.Location = new System.Drawing.Point(126, 81);
            this.checkBoxToolbarOpen.Name = "checkBoxToolbarOpen";
            this.checkBoxToolbarOpen.Size = new System.Drawing.Size(55, 17);
            this.checkBoxToolbarOpen.TabIndex = 4;
            this.checkBoxToolbarOpen.Text = "Visible";
            this.checkBoxToolbarOpen.UseVisualStyleBackColor = true;
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
            // pictureBoxNew
            // 
            this.pictureBoxNew.Location = new System.Drawing.Point(22, 41);
            this.pictureBoxNew.Name = "pictureBoxNew";
            this.pictureBoxNew.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxNew.TabIndex = 2;
            this.pictureBoxNew.TabStop = false;
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
            // tabPageFont
            // 
            this.tabPageFont.Controls.Add(this.groupBoxFont);
            this.tabPageFont.Location = new System.Drawing.Point(4, 22);
            this.tabPageFont.Name = "tabPageFont";
            this.tabPageFont.Size = new System.Drawing.Size(832, 483);
            this.tabPageFont.TabIndex = 10;
            this.tabPageFont.Text = "Font";
            this.tabPageFont.UseVisualStyleBackColor = true;
            // 
            // groupBoxFont
            // 
            this.groupBoxFont.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxFont.Controls.Add(this.labelFontNote);
            this.groupBoxFont.Controls.Add(this.groupBoxFontTextBox);
            this.groupBoxFont.Controls.Add(this.groupBoxFontListViews);
            this.groupBoxFont.Controls.Add(this.groupBoxFontGeneral);
            this.groupBoxFont.Location = new System.Drawing.Point(3, 3);
            this.groupBoxFont.Name = "groupBoxFont";
            this.groupBoxFont.Size = new System.Drawing.Size(826, 477);
            this.groupBoxFont.TabIndex = 0;
            this.groupBoxFont.TabStop = false;
            this.groupBoxFont.Text = "Font in UI";
            // 
            // labelFontNote
            // 
            this.labelFontNote.AutoSize = true;
            this.labelFontNote.Location = new System.Drawing.Point(11, 352);
            this.labelFontNote.Name = "labelFontNote";
            this.labelFontNote.Size = new System.Drawing.Size(278, 13);
            this.labelFontNote.TabIndex = 41;
            this.labelFontNote.Text = "Note: This is only to set the font in the Subtitle Edit UI...";
            // 
            // groupBoxFontTextBox
            // 
            this.groupBoxFontTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxFontTextBox.Controls.Add(this.labelSubtitleFontSize);
            this.groupBoxFontTextBox.Controls.Add(this.comboBoxSubtitleFontSize);
            this.groupBoxFontTextBox.Controls.Add(this.checkBoxSubtitleFontBold);
            this.groupBoxFontTextBox.Controls.Add(this.checkBoxSubtitleCenter);
            this.groupBoxFontTextBox.Location = new System.Drawing.Point(14, 228);
            this.groupBoxFontTextBox.Name = "groupBoxFontTextBox";
            this.groupBoxFontTextBox.Size = new System.Drawing.Size(806, 103);
            this.groupBoxFontTextBox.TabIndex = 40;
            this.groupBoxFontTextBox.TabStop = false;
            this.groupBoxFontTextBox.Text = "Text box";
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
            this.comboBoxSubtitleFontSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSubtitleFontSize.FormattingEnabled = true;
            this.comboBoxSubtitleFontSize.Items.AddRange(new object[] {
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
            this.comboBoxSubtitleFontSize.Location = new System.Drawing.Point(215, 14);
            this.comboBoxSubtitleFontSize.Name = "comboBoxSubtitleFontSize";
            this.comboBoxSubtitleFontSize.Size = new System.Drawing.Size(73, 21);
            this.comboBoxSubtitleFontSize.TabIndex = 31;
            // 
            // checkBoxSubtitleFontBold
            // 
            this.checkBoxSubtitleFontBold.AutoSize = true;
            this.checkBoxSubtitleFontBold.Location = new System.Drawing.Point(215, 41);
            this.checkBoxSubtitleFontBold.Name = "checkBoxSubtitleFontBold";
            this.checkBoxSubtitleFontBold.Size = new System.Drawing.Size(46, 17);
            this.checkBoxSubtitleFontBold.TabIndex = 32;
            this.checkBoxSubtitleFontBold.Text = "Bold";
            this.checkBoxSubtitleFontBold.UseVisualStyleBackColor = true;
            // 
            // checkBoxSubtitleCenter
            // 
            this.checkBoxSubtitleCenter.AutoSize = true;
            this.checkBoxSubtitleCenter.Location = new System.Drawing.Point(215, 64);
            this.checkBoxSubtitleCenter.Name = "checkBoxSubtitleCenter";
            this.checkBoxSubtitleCenter.Size = new System.Drawing.Size(59, 17);
            this.checkBoxSubtitleCenter.TabIndex = 33;
            this.checkBoxSubtitleCenter.Text = "Center";
            this.checkBoxSubtitleCenter.UseVisualStyleBackColor = true;
            // 
            // groupBoxFontListViews
            // 
            this.groupBoxFontListViews.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxFontListViews.Controls.Add(this.labelSubtitleListViewFontSize);
            this.groupBoxFontListViews.Controls.Add(this.comboBoxSubtitleListViewFontSize);
            this.groupBoxFontListViews.Controls.Add(this.checkBoxSubtitleListViewFontBold);
            this.groupBoxFontListViews.Location = new System.Drawing.Point(14, 141);
            this.groupBoxFontListViews.Name = "groupBoxFontListViews";
            this.groupBoxFontListViews.Size = new System.Drawing.Size(806, 81);
            this.groupBoxFontListViews.TabIndex = 39;
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
            this.comboBoxSubtitleListViewFontSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSubtitleListViewFontSize.FormattingEnabled = true;
            this.comboBoxSubtitleListViewFontSize.Items.AddRange(new object[] {
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
            this.comboBoxSubtitleListViewFontSize.Location = new System.Drawing.Point(210, 14);
            this.comboBoxSubtitleListViewFontSize.Name = "comboBoxSubtitleListViewFontSize";
            this.comboBoxSubtitleListViewFontSize.Size = new System.Drawing.Size(73, 21);
            this.comboBoxSubtitleListViewFontSize.TabIndex = 34;
            // 
            // checkBoxSubtitleListViewFontBold
            // 
            this.checkBoxSubtitleListViewFontBold.AutoSize = true;
            this.checkBoxSubtitleListViewFontBold.Location = new System.Drawing.Point(210, 41);
            this.checkBoxSubtitleListViewFontBold.Name = "checkBoxSubtitleListViewFontBold";
            this.checkBoxSubtitleListViewFontBold.Size = new System.Drawing.Size(46, 17);
            this.checkBoxSubtitleListViewFontBold.TabIndex = 35;
            this.checkBoxSubtitleListViewFontBold.Text = "Bold";
            this.checkBoxSubtitleListViewFontBold.UseVisualStyleBackColor = true;
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
            this.groupBoxFontGeneral.Location = new System.Drawing.Point(14, 20);
            this.groupBoxFontGeneral.Name = "groupBoxFontGeneral";
            this.groupBoxFontGeneral.Size = new System.Drawing.Size(806, 115);
            this.groupBoxFontGeneral.TabIndex = 38;
            this.groupBoxFontGeneral.TabStop = false;
            this.groupBoxFontGeneral.Text = "General";
            // 
            // comboBoxSubtitleFont
            // 
            this.comboBoxSubtitleFont.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSubtitleFont.FormattingEnabled = true;
            this.comboBoxSubtitleFont.Location = new System.Drawing.Point(210, 20);
            this.comboBoxSubtitleFont.Name = "comboBoxSubtitleFont";
            this.comboBoxSubtitleFont.Size = new System.Drawing.Size(188, 21);
            this.comboBoxSubtitleFont.TabIndex = 29;
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
            // tabPageSsaStyle
            // 
            this.tabPageSsaStyle.Controls.Add(this.groupBoxSsaStyle);
            this.tabPageSsaStyle.Location = new System.Drawing.Point(4, 22);
            this.tabPageSsaStyle.Name = "tabPageSsaStyle";
            this.tabPageSsaStyle.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageSsaStyle.Size = new System.Drawing.Size(832, 483);
            this.tabPageSsaStyle.TabIndex = 1;
            this.tabPageSsaStyle.Text = "SSA style";
            this.tabPageSsaStyle.UseVisualStyleBackColor = true;
            // 
            // groupBoxSsaStyle
            // 
            this.groupBoxSsaStyle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxSsaStyle.Controls.Add(this.groupBoxSsaBorder);
            this.groupBoxSsaStyle.Controls.Add(this.groupSsaBoxFont);
            this.groupBoxSsaStyle.Controls.Add(this.groupBoxMargins);
            this.groupBoxSsaStyle.Controls.Add(this.groupBoxPreview);
            this.groupBoxSsaStyle.Location = new System.Drawing.Point(6, 6);
            this.groupBoxSsaStyle.Name = "groupBoxSsaStyle";
            this.groupBoxSsaStyle.Size = new System.Drawing.Size(819, 456);
            this.groupBoxSsaStyle.TabIndex = 0;
            this.groupBoxSsaStyle.TabStop = false;
            this.groupBoxSsaStyle.Text = "Sub Station Alpha style";
            // 
            // groupBoxSsaBorder
            // 
            this.groupBoxSsaBorder.Controls.Add(this.numericUpDownSsaOutline);
            this.groupBoxSsaBorder.Controls.Add(this.labelSsaShadow);
            this.groupBoxSsaBorder.Controls.Add(this.numericUpDownSsaShadow);
            this.groupBoxSsaBorder.Controls.Add(this.checkBoxSsaOpaqueBox);
            this.groupBoxSsaBorder.Controls.Add(this.labelSsaOutline);
            this.groupBoxSsaBorder.Location = new System.Drawing.Point(329, 21);
            this.groupBoxSsaBorder.Name = "groupBoxSsaBorder";
            this.groupBoxSsaBorder.Size = new System.Drawing.Size(185, 96);
            this.groupBoxSsaBorder.TabIndex = 8;
            this.groupBoxSsaBorder.TabStop = false;
            this.groupBoxSsaBorder.Text = "Border";
            // 
            // numericUpDownSsaOutline
            // 
            this.numericUpDownSsaOutline.Location = new System.Drawing.Point(64, 16);
            this.numericUpDownSsaOutline.Maximum = new decimal(new int[] {
            9,
            0,
            0,
            0});
            this.numericUpDownSsaOutline.Name = "numericUpDownSsaOutline";
            this.numericUpDownSsaOutline.Size = new System.Drawing.Size(44, 21);
            this.numericUpDownSsaOutline.TabIndex = 5;
            this.numericUpDownSsaOutline.ValueChanged += new System.EventHandler(this.numericUpDownSsaOutline_ValueChanged);
            // 
            // labelSsaShadow
            // 
            this.labelSsaShadow.AutoSize = true;
            this.labelSsaShadow.Location = new System.Drawing.Point(10, 45);
            this.labelSsaShadow.Name = "labelSsaShadow";
            this.labelSsaShadow.Size = new System.Drawing.Size(45, 13);
            this.labelSsaShadow.TabIndex = 6;
            this.labelSsaShadow.Text = "Shadow";
            // 
            // numericUpDownSsaShadow
            // 
            this.numericUpDownSsaShadow.Location = new System.Drawing.Point(64, 43);
            this.numericUpDownSsaShadow.Maximum = new decimal(new int[] {
            9,
            0,
            0,
            0});
            this.numericUpDownSsaShadow.Name = "numericUpDownSsaShadow";
            this.numericUpDownSsaShadow.Size = new System.Drawing.Size(44, 21);
            this.numericUpDownSsaShadow.TabIndex = 7;
            this.numericUpDownSsaShadow.ValueChanged += new System.EventHandler(this.numericUpDownSsaShadow_ValueChanged);
            // 
            // checkBoxSsaOpaqueBox
            // 
            this.checkBoxSsaOpaqueBox.AutoSize = true;
            this.checkBoxSsaOpaqueBox.Location = new System.Drawing.Point(13, 71);
            this.checkBoxSsaOpaqueBox.Name = "checkBoxSsaOpaqueBox";
            this.checkBoxSsaOpaqueBox.Size = new System.Drawing.Size(85, 17);
            this.checkBoxSsaOpaqueBox.TabIndex = 8;
            this.checkBoxSsaOpaqueBox.Text = "Opaque box";
            this.checkBoxSsaOpaqueBox.UseVisualStyleBackColor = true;
            this.checkBoxSsaOpaqueBox.CheckedChanged += new System.EventHandler(this.checkBoxSsaOpaqueBox_CheckedChanged);
            // 
            // labelSsaOutline
            // 
            this.labelSsaOutline.AutoSize = true;
            this.labelSsaOutline.Location = new System.Drawing.Point(10, 20);
            this.labelSsaOutline.Name = "labelSsaOutline";
            this.labelSsaOutline.Size = new System.Drawing.Size(41, 13);
            this.labelSsaOutline.TabIndex = 4;
            this.labelSsaOutline.Text = "Outline";
            // 
            // groupSsaBoxFont
            // 
            this.groupSsaBoxFont.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupSsaBoxFont.Controls.Add(this.checkBoxSsaFontBold);
            this.groupSsaBoxFont.Controls.Add(this.buttonSsaColor);
            this.groupSsaBoxFont.Controls.Add(this.panelPrimaryColor);
            this.groupSsaBoxFont.Controls.Add(this.numericUpDownFontSize);
            this.groupSsaBoxFont.Controls.Add(this.comboBoxFontName);
            this.groupSsaBoxFont.Controls.Add(this.labelSsaFontSize);
            this.groupSsaBoxFont.Controls.Add(this.labelFontName);
            this.groupSsaBoxFont.Location = new System.Drawing.Point(6, 20);
            this.groupSsaBoxFont.Name = "groupSsaBoxFont";
            this.groupSsaBoxFont.Size = new System.Drawing.Size(323, 97);
            this.groupSsaBoxFont.TabIndex = 7;
            this.groupSsaBoxFont.TabStop = false;
            this.groupSsaBoxFont.Text = "Font";
            // 
            // checkBoxSsaFontBold
            // 
            this.checkBoxSsaFontBold.AutoSize = true;
            this.checkBoxSsaFontBold.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxSsaFontBold.Location = new System.Drawing.Point(184, 45);
            this.checkBoxSsaFontBold.Name = "checkBoxSsaFontBold";
            this.checkBoxSsaFontBold.Size = new System.Drawing.Size(51, 17);
            this.checkBoxSsaFontBold.TabIndex = 7;
            this.checkBoxSsaFontBold.Text = "Bold";
            this.checkBoxSsaFontBold.UseVisualStyleBackColor = true;
            this.checkBoxSsaFontBold.CheckedChanged += new System.EventHandler(this.checkBoxSsaFontBold_CheckedChanged);
            // 
            // buttonSsaColor
            // 
            this.buttonSsaColor.Location = new System.Drawing.Point(6, 66);
            this.buttonSsaColor.Name = "buttonSsaColor";
            this.buttonSsaColor.Size = new System.Drawing.Size(109, 23);
            this.buttonSsaColor.TabIndex = 6;
            this.buttonSsaColor.Text = "Choose color";
            this.buttonSsaColor.UseVisualStyleBackColor = true;
            this.buttonSsaColor.Click += new System.EventHandler(this.buttonSsaColor_Click);
            // 
            // panelPrimaryColor
            // 
            this.panelPrimaryColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelPrimaryColor.Location = new System.Drawing.Point(121, 68);
            this.panelPrimaryColor.Name = "panelPrimaryColor";
            this.panelPrimaryColor.Size = new System.Drawing.Size(21, 20);
            this.panelPrimaryColor.TabIndex = 4;
            this.panelPrimaryColor.MouseClick += new System.Windows.Forms.MouseEventHandler(this.panelPrimaryColor_MouseClick);
            // 
            // numericUpDownFontSize
            // 
            this.numericUpDownFontSize.Location = new System.Drawing.Point(121, 44);
            this.numericUpDownFontSize.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.numericUpDownFontSize.Name = "numericUpDownFontSize";
            this.numericUpDownFontSize.Size = new System.Drawing.Size(51, 21);
            this.numericUpDownFontSize.TabIndex = 3;
            this.numericUpDownFontSize.ValueChanged += new System.EventHandler(this.numericUpDownFontSize_ValueChanged);
            // 
            // comboBoxFontName
            // 
            this.comboBoxFontName.FormattingEnabled = true;
            this.comboBoxFontName.Location = new System.Drawing.Point(121, 17);
            this.comboBoxFontName.Name = "comboBoxFontName";
            this.comboBoxFontName.Size = new System.Drawing.Size(188, 21);
            this.comboBoxFontName.TabIndex = 1;
            this.comboBoxFontName.TextChanged += new System.EventHandler(this.comboBoxFontName_TextChanged);
            // 
            // labelSsaFontSize
            // 
            this.labelSsaFontSize.AutoSize = true;
            this.labelSsaFontSize.Location = new System.Drawing.Point(10, 46);
            this.labelSsaFontSize.Name = "labelSsaFontSize";
            this.labelSsaFontSize.Size = new System.Drawing.Size(50, 13);
            this.labelSsaFontSize.TabIndex = 2;
            this.labelSsaFontSize.Text = "Font size";
            // 
            // labelFontName
            // 
            this.labelFontName.AutoSize = true;
            this.labelFontName.Location = new System.Drawing.Point(10, 20);
            this.labelFontName.Name = "labelFontName";
            this.labelFontName.Size = new System.Drawing.Size(58, 13);
            this.labelFontName.TabIndex = 0;
            this.labelFontName.Text = "Font name";
            // 
            // groupBoxMargins
            // 
            this.groupBoxMargins.Controls.Add(this.numericUpDownSsaMarginVertical);
            this.groupBoxMargins.Controls.Add(this.numericUpDownSsaMarginRight);
            this.groupBoxMargins.Controls.Add(this.numericUpDownSsaMarginLeft);
            this.groupBoxMargins.Controls.Add(this.labelMarginVertical);
            this.groupBoxMargins.Controls.Add(this.labelMarginRight);
            this.groupBoxMargins.Controls.Add(this.labelMarginLeft);
            this.groupBoxMargins.Location = new System.Drawing.Point(520, 20);
            this.groupBoxMargins.Name = "groupBoxMargins";
            this.groupBoxMargins.Size = new System.Drawing.Size(281, 97);
            this.groupBoxMargins.TabIndex = 9;
            this.groupBoxMargins.TabStop = false;
            this.groupBoxMargins.Text = "Margins";
            // 
            // numericUpDownSsaMarginVertical
            // 
            this.numericUpDownSsaMarginVertical.Location = new System.Drawing.Point(168, 33);
            this.numericUpDownSsaMarginVertical.Maximum = new decimal(new int[] {
            250,
            0,
            0,
            0});
            this.numericUpDownSsaMarginVertical.Name = "numericUpDownSsaMarginVertical";
            this.numericUpDownSsaMarginVertical.Size = new System.Drawing.Size(44, 21);
            this.numericUpDownSsaMarginVertical.TabIndex = 5;
            this.numericUpDownSsaMarginVertical.ValueChanged += new System.EventHandler(this.numericUpDownSsaMarginVertical_ValueChanged);
            // 
            // numericUpDownSsaMarginRight
            // 
            this.numericUpDownSsaMarginRight.Location = new System.Drawing.Point(93, 33);
            this.numericUpDownSsaMarginRight.Maximum = new decimal(new int[] {
            250,
            0,
            0,
            0});
            this.numericUpDownSsaMarginRight.Name = "numericUpDownSsaMarginRight";
            this.numericUpDownSsaMarginRight.Size = new System.Drawing.Size(44, 21);
            this.numericUpDownSsaMarginRight.TabIndex = 3;
            // 
            // numericUpDownSsaMarginLeft
            // 
            this.numericUpDownSsaMarginLeft.Location = new System.Drawing.Point(16, 33);
            this.numericUpDownSsaMarginLeft.Maximum = new decimal(new int[] {
            250,
            0,
            0,
            0});
            this.numericUpDownSsaMarginLeft.Name = "numericUpDownSsaMarginLeft";
            this.numericUpDownSsaMarginLeft.Size = new System.Drawing.Size(44, 21);
            this.numericUpDownSsaMarginLeft.TabIndex = 1;
            // 
            // labelMarginVertical
            // 
            this.labelMarginVertical.AutoSize = true;
            this.labelMarginVertical.Location = new System.Drawing.Point(165, 17);
            this.labelMarginVertical.Name = "labelMarginVertical";
            this.labelMarginVertical.Size = new System.Drawing.Size(42, 13);
            this.labelMarginVertical.TabIndex = 4;
            this.labelMarginVertical.Text = "Vertical";
            // 
            // labelMarginRight
            // 
            this.labelMarginRight.AutoSize = true;
            this.labelMarginRight.Location = new System.Drawing.Point(90, 16);
            this.labelMarginRight.Name = "labelMarginRight";
            this.labelMarginRight.Size = new System.Drawing.Size(32, 13);
            this.labelMarginRight.TabIndex = 2;
            this.labelMarginRight.Text = "Right";
            // 
            // labelMarginLeft
            // 
            this.labelMarginLeft.AutoSize = true;
            this.labelMarginLeft.Location = new System.Drawing.Point(13, 16);
            this.labelMarginLeft.Name = "labelMarginLeft";
            this.labelMarginLeft.Size = new System.Drawing.Size(26, 13);
            this.labelMarginLeft.TabIndex = 0;
            this.labelMarginLeft.Text = "Left";
            // 
            // groupBoxPreview
            // 
            this.groupBoxPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxPreview.Controls.Add(this.pictureBoxPreview);
            this.groupBoxPreview.Location = new System.Drawing.Point(6, 118);
            this.groupBoxPreview.Name = "groupBoxPreview";
            this.groupBoxPreview.Size = new System.Drawing.Size(807, 332);
            this.groupBoxPreview.TabIndex = 10;
            this.groupBoxPreview.TabStop = false;
            this.groupBoxPreview.Text = "Preview";
            // 
            // pictureBoxPreview
            // 
            this.pictureBoxPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBoxPreview.Location = new System.Drawing.Point(3, 17);
            this.pictureBoxPreview.Name = "pictureBoxPreview";
            this.pictureBoxPreview.Size = new System.Drawing.Size(801, 312);
            this.pictureBoxPreview.TabIndex = 0;
            this.pictureBoxPreview.TabStop = false;
            // 
            // tabPageNetwork
            // 
            this.tabPageNetwork.Controls.Add(this.groupBoxNetworkSession);
            this.tabPageNetwork.Controls.Add(this.groupBoxProxySettings);
            this.tabPageNetwork.Location = new System.Drawing.Point(4, 22);
            this.tabPageNetwork.Name = "tabPageNetwork";
            this.tabPageNetwork.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageNetwork.Size = new System.Drawing.Size(832, 483);
            this.tabPageNetwork.TabIndex = 4;
            this.tabPageNetwork.Text = "Network";
            this.tabPageNetwork.UseVisualStyleBackColor = true;
            // 
            // groupBoxNetworkSession
            // 
            this.groupBoxNetworkSession.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxNetworkSession.Controls.Add(this.buttonNetworkSessionNewMessageSound);
            this.groupBoxNetworkSession.Controls.Add(this.textBoxNetworkSessionNewMessageSound);
            this.groupBoxNetworkSession.Controls.Add(this.labelNetworkSessionNewMessageSound);
            this.groupBoxNetworkSession.Location = new System.Drawing.Point(6, 189);
            this.groupBoxNetworkSession.Name = "groupBoxNetworkSession";
            this.groupBoxNetworkSession.Size = new System.Drawing.Size(819, 273);
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
            this.textBoxNetworkSessionNewMessageSound.Location = new System.Drawing.Point(28, 50);
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
            this.groupBoxProxySettings.Location = new System.Drawing.Point(6, 6);
            this.groupBoxProxySettings.Name = "groupBoxProxySettings";
            this.groupBoxProxySettings.Size = new System.Drawing.Size(819, 177);
            this.groupBoxProxySettings.TabIndex = 1;
            this.groupBoxProxySettings.TabStop = false;
            this.groupBoxProxySettings.Text = "Proxy server settings";
            // 
            // groupBoxProxyAuthentication
            // 
            this.groupBoxProxyAuthentication.Controls.Add(this.textBoxProxyDomain);
            this.groupBoxProxyAuthentication.Controls.Add(this.labelProxyDomain);
            this.groupBoxProxyAuthentication.Controls.Add(this.textBoxProxyUserName);
            this.groupBoxProxyAuthentication.Controls.Add(this.labelProxyPassword);
            this.groupBoxProxyAuthentication.Controls.Add(this.labelProxyUserName);
            this.groupBoxProxyAuthentication.Controls.Add(this.textBoxProxyPassword);
            this.groupBoxProxyAuthentication.Location = new System.Drawing.Point(28, 60);
            this.groupBoxProxyAuthentication.Name = "groupBoxProxyAuthentication";
            this.groupBoxProxyAuthentication.Size = new System.Drawing.Size(318, 101);
            this.groupBoxProxyAuthentication.TabIndex = 29;
            this.groupBoxProxyAuthentication.TabStop = false;
            this.groupBoxProxyAuthentication.Text = "Authentication";
            // 
            // textBoxProxyDomain
            // 
            this.textBoxProxyDomain.Location = new System.Drawing.Point(106, 71);
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
            this.textBoxProxyUserName.Location = new System.Drawing.Point(106, 19);
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
            this.textBoxProxyPassword.Location = new System.Drawing.Point(106, 45);
            this.textBoxProxyPassword.Name = "textBoxProxyPassword";
            this.textBoxProxyPassword.Size = new System.Drawing.Size(192, 21);
            this.textBoxProxyPassword.TabIndex = 24;
            this.textBoxProxyPassword.UseSystemPasswordChar = true;
            // 
            // textBoxProxyAddress
            // 
            this.textBoxProxyAddress.Location = new System.Drawing.Point(134, 34);
            this.textBoxProxyAddress.Name = "textBoxProxyAddress";
            this.textBoxProxyAddress.Size = new System.Drawing.Size(192, 21);
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
            this.labelStatus.Location = new System.Drawing.Point(12, 536);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(60, 13);
            this.labelStatus.TabIndex = 3;
            this.labelStatus.Text = "labelStatus";
            // 
            // openFileDialogFFmpeg
            // 
            this.openFileDialogFFmpeg.FileName = "openFileDialog1";
            // 
            // buttonReset
            // 
            this.buttonReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonReset.Location = new System.Drawing.Point(668, 526);
            this.buttonReset.Name = "buttonReset";
            this.buttonReset.Size = new System.Drawing.Size(185, 23);
            this.buttonReset.TabIndex = 2;
            this.buttonReset.Text = "Restore default settings";
            this.buttonReset.UseVisualStyleBackColor = true;
            this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
            // 
            // Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(865, 561);
            this.Controls.Add(this.buttonReset);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.tabControlSettings);
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
            this.tabControlSettings.ResumeLayout(false);
            this.tabPageGeneral.ResumeLayout(false);
            this.groupBoxMiscellaneous.ResumeLayout(false);
            this.groupBoxMiscellaneous.PerformLayout();
            this.groupBoxGeneralRules.ResumeLayout(false);
            this.groupBoxGeneralRules.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownOptimalCharsSec)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxWordsMin)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxNumberOfLines)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDurationMin)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDurationMax)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMinGapMs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxCharsSec)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSubtitleLineMaximumLength)).EndInit();
            this.tabPageShortcuts.ResumeLayout(false);
            this.groupBoxShortcuts.ResumeLayout(false);
            this.groupBoxShortcuts.PerformLayout();
            this.tabPageSyntaxColoring.ResumeLayout(false);
            this.groupBoxListViewSyntaxColoring.ResumeLayout(false);
            this.groupBoxListViewSyntaxColoring.PerformLayout();
            this.tabPageVideoPlayer.ResumeLayout(false);
            this.groupBoxMainWindowVideoControls.ResumeLayout(false);
            this.groupBoxMainWindowVideoControls.PerformLayout();
            this.groupBoxVideoPlayerDefault.ResumeLayout(false);
            this.groupBoxVideoPlayerDefault.PerformLayout();
            this.groupBoxVideoEngine.ResumeLayout(false);
            this.groupBoxVideoEngine.PerformLayout();
            this.tabPageWaveform.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBoxSpectrogram.ResumeLayout(false);
            this.groupBoxSpectrogram.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBoxWaveformAppearence.ResumeLayout(false);
            this.groupBoxWaveformAppearence.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownWaveformBorderHitMs)).EndInit();
            this.tabPageTools.ResumeLayout(false);
            this.groupBoxGoogleTranslate.ResumeLayout(false);
            this.groupBoxGoogleTranslate.PerformLayout();
            this.groupBoxBing.ResumeLayout(false);
            this.groupBoxBing.PerformLayout();
            this.groupBoxToolsAutoBr.ResumeLayout(false);
            this.groupBoxToolsAutoBr.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownToolsBreakPreferBottomHeavy)).EndInit();
            this.groupBoxSpellCheck.ResumeLayout(false);
            this.groupBoxSpellCheck.PerformLayout();
            this.groupBoxFixCommonErrors.ResumeLayout(false);
            this.groupBoxFixCommonErrors.PerformLayout();
            this.groupBoxToolsVisualSync.ResumeLayout(false);
            this.groupBoxToolsVisualSync.PerformLayout();
            this.tabPageWordLists.ResumeLayout(false);
            this.groupBoxWordLists.ResumeLayout(false);
            this.groupBoxWordLists.PerformLayout();
            this.groupBoxOcrFixList.ResumeLayout(false);
            this.groupBoxOcrFixList.PerformLayout();
            this.groupBoxUserWordList.ResumeLayout(false);
            this.groupBoxUserWordList.PerformLayout();
            this.groupBoxWordListLocation.ResumeLayout(false);
            this.groupBoxWordListLocation.PerformLayout();
            this.groupBoxNamesIgonoreLists.ResumeLayout(false);
            this.groupBoxNamesIgonoreLists.PerformLayout();
            this.tabPageToolBar.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBoxShowToolBarButtons.ResumeLayout(false);
            this.groupBoxShowToolBarButtons.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxNetflixQualityCheck)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTBRemoveTextForHi)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTBFixCommonErrors)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxHelp)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSettings)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSpellCheck)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxVisualSync)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxReplace)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxFind)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSaveAs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSave)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxOpen)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxNew)).EndInit();
            this.tabPageFont.ResumeLayout(false);
            this.groupBoxFont.ResumeLayout(false);
            this.groupBoxFont.PerformLayout();
            this.groupBoxFontTextBox.ResumeLayout(false);
            this.groupBoxFontTextBox.PerformLayout();
            this.groupBoxFontListViews.ResumeLayout(false);
            this.groupBoxFontListViews.PerformLayout();
            this.groupBoxFontGeneral.ResumeLayout(false);
            this.groupBoxFontGeneral.PerformLayout();
            this.tabPageSsaStyle.ResumeLayout(false);
            this.groupBoxSsaStyle.ResumeLayout(false);
            this.groupBoxSsaBorder.ResumeLayout(false);
            this.groupBoxSsaBorder.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSsaOutline)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSsaShadow)).EndInit();
            this.groupSsaBoxFont.ResumeLayout(false);
            this.groupSsaBoxFont.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFontSize)).EndInit();
            this.groupBoxMargins.ResumeLayout(false);
            this.groupBoxMargins.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSsaMarginVertical)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSsaMarginRight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSsaMarginLeft)).EndInit();
            this.groupBoxPreview.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreview)).EndInit();
            this.tabPageNetwork.ResumeLayout(false);
            this.groupBoxNetworkSession.ResumeLayout(false);
            this.groupBoxNetworkSession.PerformLayout();
            this.groupBoxProxySettings.ResumeLayout(false);
            this.groupBoxProxySettings.PerformLayout();
            this.groupBoxProxyAuthentication.ResumeLayout(false);
            this.groupBoxProxyAuthentication.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.TabControl tabControlSettings;
        private System.Windows.Forms.TabPage tabPageGeneral;
        private System.Windows.Forms.TabPage tabPageSsaStyle;
        private System.Windows.Forms.GroupBox groupBoxMiscellaneous;
        private System.Windows.Forms.GroupBox groupBoxShowToolBarButtons;
        private System.Windows.Forms.PictureBox pictureBoxNew;
        private System.Windows.Forms.CheckBox checkBoxToolbarNew;
        private System.Windows.Forms.Label labelTBSpellCheck;
        private System.Windows.Forms.PictureBox pictureBoxSpellCheck;
        private System.Windows.Forms.CheckBox checkBoxSpellCheck;
        private System.Windows.Forms.Label labelTBVisualSync;
        private System.Windows.Forms.PictureBox pictureBoxVisualSync;
        private System.Windows.Forms.CheckBox checkBoxVisualSync;
        private System.Windows.Forms.Label labelTBReplace;
        private System.Windows.Forms.PictureBox pictureBoxReplace;
        private System.Windows.Forms.CheckBox checkBoxReplace;
        private System.Windows.Forms.Label labelTBFind;
        private System.Windows.Forms.PictureBox pictureBoxFind;
        private System.Windows.Forms.CheckBox checkBoxToolbarFind;
        private System.Windows.Forms.Label labelTBSaveAs;
        private System.Windows.Forms.PictureBox pictureBoxSaveAs;
        private System.Windows.Forms.CheckBox checkBoxToolbarSaveAs;
        private System.Windows.Forms.Label labelTBSave;
        private System.Windows.Forms.PictureBox pictureBoxSave;
        private System.Windows.Forms.CheckBox checkBoxToolbarSave;
        private System.Windows.Forms.Label labelTBOpen;
        private System.Windows.Forms.PictureBox pictureBoxOpen;
        private System.Windows.Forms.CheckBox checkBoxToolbarOpen;
        private System.Windows.Forms.Label labelTBNew;
        private System.Windows.Forms.Label labelTBHelp;
        private System.Windows.Forms.PictureBox pictureBoxHelp;
        private System.Windows.Forms.CheckBox checkBoxHelp;
        private System.Windows.Forms.Label labelTBSettings;
        private System.Windows.Forms.PictureBox pictureBoxSettings;
        private System.Windows.Forms.CheckBox checkBoxSettings;
        private System.Windows.Forms.Label labelDefaultFrameRate;
        private System.Windows.Forms.Label labelDefaultFileEncoding;
        private System.Windows.Forms.ComboBox comboBoxFrameRate;
        private System.Windows.Forms.ComboBox comboBoxEncoding;
        private System.Windows.Forms.CheckBox checkBoxRememberRecentFiles;
        private System.Windows.Forms.GroupBox groupBoxSsaStyle;
        private System.Windows.Forms.ColorDialog colorDialogSSAStyle;
        private System.Windows.Forms.CheckBox checkBoxStartInSourceView;
        private System.Windows.Forms.CheckBox checkBoxReopenLastOpened;
        private System.Windows.Forms.Label labelSubMaxLen;
        private System.Windows.Forms.TabPage tabPageVideoPlayer;
        private System.Windows.Forms.CheckBox checkBoxVideoPlayerShowStopButton;
        private System.Windows.Forms.GroupBox groupBoxVideoEngine;
        private System.Windows.Forms.Label labelVideoPlayerMPlayer;
        private System.Windows.Forms.Label labelDirectShowDescription;
        private System.Windows.Forms.Label labelMpcHcDescription;
        private System.Windows.Forms.RadioButton radioButtonVideoPlayerMPV;
        private System.Windows.Forms.RadioButton radioButtonVideoPlayerDirectShow;
        private System.Windows.Forms.RadioButton radioButtonVideoPlayerMpcHc;
        private System.Windows.Forms.CheckBox checkBoxRememberWindowPosition;
        private System.Windows.Forms.TextBox textBoxShowLineBreaksAs;
        private System.Windows.Forms.Label labelShowLineBreaksAs;
        private System.Windows.Forms.TabPage tabPageWordLists;
        private System.Windows.Forms.GroupBox groupBoxWordLists;
        private System.Windows.Forms.GroupBox groupBoxWordListLocation;
        private System.Windows.Forms.GroupBox groupBoxOcrFixList;
        private System.Windows.Forms.GroupBox groupBoxNamesIgonoreLists;
        private System.Windows.Forms.TextBox textBoxNameEtc;
        private System.Windows.Forms.Label labelWordListLanguage;
        private System.Windows.Forms.Button buttonAddNames;
        private System.Windows.Forms.ComboBox comboBoxWordListLanguage;
        private System.Windows.Forms.Button buttonRemoveNameEtc;
        private System.Windows.Forms.ListBox listBoxNames;
        private System.Windows.Forms.Button buttonRemoveOcrFix;
        private System.Windows.Forms.ListBox listBoxOcrFixList;
        private System.Windows.Forms.TextBox textBoxOcrFixKey;
        private System.Windows.Forms.Button buttonAddOcrFix;
        private System.Windows.Forms.GroupBox groupBoxUserWordList;
        private System.Windows.Forms.Button buttonRemoveUserWord;
        private System.Windows.Forms.ListBox listBoxUserWordLists;
        private System.Windows.Forms.TextBox textBoxUserWord;
        private System.Windows.Forms.Button buttonAddUserWord;
        private System.Windows.Forms.TabPage tabPageNetwork;
        private System.Windows.Forms.GroupBox groupBoxProxySettings;
        private System.Windows.Forms.Label labelProxyPassword;
        private System.Windows.Forms.TextBox textBoxProxyAddress;
        private System.Windows.Forms.TextBox textBoxProxyUserName;
        private System.Windows.Forms.TextBox textBoxProxyPassword;
        private System.Windows.Forms.Label labelProxyAddress;
        private System.Windows.Forms.Label labelProxyUserName;
        private System.Windows.Forms.CheckBox checkBoxNamesOnline;
        private System.Windows.Forms.TextBox textBoxNamesOnline;
        private System.Windows.Forms.TextBox textBoxOcrFixValue;
        private System.Windows.Forms.TabPage tabPageTools;
        private System.Windows.Forms.GroupBox groupBoxToolsVisualSync;
        private System.Windows.Forms.ComboBox comboBoxToolsVerifySeconds;
        private System.Windows.Forms.Label labelVerifyButton;
        private System.Windows.Forms.Label labelToolsEndScene;
        private System.Windows.Forms.ComboBox comboBoxToolsEndSceneIndex;
        private System.Windows.Forms.Label labelToolsStartScene;
        private System.Windows.Forms.ComboBox comboBoxToolsStartSceneIndex;
        private System.Windows.Forms.GroupBox groupBoxFixCommonErrors;
        private System.Windows.Forms.Label labelToolsMusicSymbol;
        private System.Windows.Forms.ComboBox comboBoxListViewDoubleClickEvent;
        private System.Windows.Forms.Label labelListViewDoubleClickEvent;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.TextBox textBoxMusicSymbolsToReplace;
        private System.Windows.Forms.Label labelToolsMusicSymbolsToReplace;
        private System.Windows.Forms.ComboBox comboBoxToolsMusicSymbol;
        private System.Windows.Forms.GroupBox groupBoxSpellCheck;
        private System.Windows.Forms.CheckBox checkBoxSpellCheckAutoChangeNames;
        private System.Windows.Forms.GroupBox groupBoxProxyAuthentication;
        private System.Windows.Forms.TextBox textBoxProxyDomain;
        private System.Windows.Forms.Label labelProxyDomain;
        private System.Windows.Forms.CheckBox checkBoxAutoDetectAnsiEncoding;
        private System.Windows.Forms.Label labelAutoDetectAnsiEncoding;
        private System.Windows.Forms.Label labelVideoPlayerVLC;
        private System.Windows.Forms.RadioButton radioButtonVideoPlayerVLC;
        private System.Windows.Forms.CheckBox checkBoxRemoveBlankLinesWhenOpening;
        private System.Windows.Forms.GroupBox groupBoxMainWindowVideoControls;
        private System.Windows.Forms.GroupBox groupBoxVideoPlayerDefault;
        private System.Windows.Forms.TextBox textBoxCustomSearchUrl1;
        private System.Windows.Forms.Label labelCustomSearch;
        private System.Windows.Forms.ComboBox comboBoxCustomSearch1;
        private System.Windows.Forms.TabPage tabPageWaveform;
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
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button buttonWaveformsFolderEmpty;
        private System.Windows.Forms.Label labelWaveformsFolderInfo;
        private System.Windows.Forms.CheckBox checkBoxRememberSelectedLine;
        private System.Windows.Forms.ComboBox comboBoxAutoBackup;
        private System.Windows.Forms.Label labelAutoBackup;
        private System.Windows.Forms.TabPage tabPageToolBar;
        private System.Windows.Forms.CheckBox checkBoxShowFrameRate;
        private System.Windows.Forms.ComboBox comboBoxSpellChecker;
        private System.Windows.Forms.Label labelSpellChecker;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox checkBoxAllowEditOfOriginalSubtitle;
        private System.Windows.Forms.Label labelMergeShortLines;
        private System.Windows.Forms.ComboBox comboBoxMergeShortLineLength;
        private System.Windows.Forms.CheckBox checkBoxAutoWrapWhileTyping;
        private System.Windows.Forms.CheckBox checkBoxFixCommonOcrErrorsUsingHardcodedRules;
        private System.Windows.Forms.Label labelVideoPlayerPreviewFontSize;
        private System.Windows.Forms.ComboBox comboBoxlVideoPlayerPreviewFontSize;
        private System.Windows.Forms.TabPage tabPageShortcuts;
        private System.Windows.Forms.GroupBox groupBoxShortcuts;
        private System.Windows.Forms.Button buttonUpdateShortcut;
        private System.Windows.Forms.TreeView treeViewShortcuts;
        private System.Windows.Forms.Label labelShortcut;
        private System.Windows.Forms.Label labelShortcutKey;
        private System.Windows.Forms.ComboBox comboBoxShortcutKey;
        private System.Windows.Forms.CheckBox checkBoxShortcutsShift;
        private System.Windows.Forms.CheckBox checkBoxShortcutsAlt;
        private System.Windows.Forms.CheckBox checkBoxShortcutsControl;
        private System.Windows.Forms.CheckBox checkBoxPromptDeleteLines;
        private System.Windows.Forms.GroupBox groupBoxSpectrogram;
        private System.Windows.Forms.CheckBox checkBoxGenerateSpectrogram;
        private System.Windows.Forms.Label labelSpectrogramAppearance;
        private System.Windows.Forms.ComboBox comboBoxSpectrogramAppearance;
        private System.Windows.Forms.CheckBox checkBoxReverseMouseWheelScrollDirection;
        private System.Windows.Forms.Label labelMaxDuration;
        private System.Windows.Forms.Label labelMinDuration;
        private System.Windows.Forms.NumericUpDown numericUpDownDurationMax;
        private System.Windows.Forms.NumericUpDown numericUpDownDurationMin;
        private System.Windows.Forms.ComboBox comboBoxTimeCodeMode;
        private System.Windows.Forms.Label labelTimeCodeMode;
        private System.Windows.Forms.TabPage tabPageSyntaxColoring;
        private System.Windows.Forms.GroupBox groupBoxListViewSyntaxColoring;
        private System.Windows.Forms.CheckBox checkBoxSyntaxOverlap;
        private System.Windows.Forms.CheckBox checkBoxSyntaxColorTextTooLong;
        private System.Windows.Forms.CheckBox checkBoxSyntaxColorDurationTooLarge;
        private System.Windows.Forms.CheckBox checkBoxSyntaxColorDurationTooSmall;
        private System.Windows.Forms.CheckBox checkBoxSyntaxColorTextMoreThanTwoLines;
        private System.Windows.Forms.Button buttonListViewSyntaxColorError;
        private System.Windows.Forms.Panel panelListViewSyntaxColorError;
        private System.Windows.Forms.Label labelMaxCharsPerSecond;
        private System.Windows.Forms.NumericUpDown numericUpDownMaxCharsSec;
        private System.Windows.Forms.NumericUpDown numericUpDownSubtitleLineMaximumLength;
        private System.Windows.Forms.CheckBox checkBoxVideoPlayerShowFullscreenButton;
        private System.Windows.Forms.CheckBox checkBoxVideoPlayerShowMuteButton;
        private System.Windows.Forms.Label labelCustomSearch1;
        private System.Windows.Forms.Label labelCustomSearch2;
        private System.Windows.Forms.TextBox textBoxCustomSearchUrl2;
        private System.Windows.Forms.ComboBox comboBoxCustomSearch2;
        private System.Windows.Forms.Label labelCustomSearch5;
        private System.Windows.Forms.TextBox textBoxCustomSearchUrl5;
        private System.Windows.Forms.ComboBox comboBoxCustomSearch5;
        private System.Windows.Forms.Label labelCustomSearch4;
        private System.Windows.Forms.TextBox textBoxCustomSearchUrl4;
        private System.Windows.Forms.ComboBox comboBoxCustomSearch4;
        private System.Windows.Forms.Label labelCustomSearch3;
        private System.Windows.Forms.TextBox textBoxCustomSearchUrl3;
        private System.Windows.Forms.ComboBox comboBoxCustomSearch3;
        private System.Windows.Forms.CheckBox checkBoxAllowOverlap;
        private System.Windows.Forms.Label labelWaveformBorderHitMs2;
        private System.Windows.Forms.NumericUpDown numericUpDownWaveformBorderHitMs;
        private System.Windows.Forms.Label labelWaveformBorderHitMs1;
        private System.Windows.Forms.Label labelSsaOutline;
        private System.Windows.Forms.NumericUpDown numericUpDownSsaShadow;
        private System.Windows.Forms.NumericUpDown numericUpDownSsaOutline;
        private System.Windows.Forms.Label labelSsaShadow;
        private System.Windows.Forms.GroupBox groupBoxPreview;
        private System.Windows.Forms.PictureBox pictureBoxPreview;
        private System.Windows.Forms.CheckBox checkBoxSsaOpaqueBox;
        private System.Windows.Forms.CheckBox checkBoxSpellCheckOneLetterWords;
        private System.Windows.Forms.CheckBox checkBoxTreatINQuoteAsING;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button buttonBrowseToFFmpeg;
        private System.Windows.Forms.TextBox textBoxFFmpegPath;
        private System.Windows.Forms.Label labelFFmpegPath;
        private System.Windows.Forms.CheckBox checkBoxUseFFmpeg;
        private System.Windows.Forms.OpenFileDialog openFileDialogFFmpeg;
        private System.Windows.Forms.CheckBox checkBoxWaveformHoverFocus;
        private System.Windows.Forms.CheckBox checkBoxListViewMouseEnterFocus;
        private System.Windows.Forms.NumericUpDown numericUpDownMinGapMs;
        private System.Windows.Forms.Label labelMinGapMs;
        private System.Windows.Forms.Label labelTBFixCommonErrors;
        private System.Windows.Forms.PictureBox pictureBoxTBFixCommonErrors;
        private System.Windows.Forms.CheckBox checkBoxTBFixCommonErrors;
        private System.Windows.Forms.CheckBox checkBoxFixShortDisplayTimesAllowMoveStartTime;
        private System.Windows.Forms.Button buttonVlcPathBrowse;
        private System.Windows.Forms.TextBox textBoxVlcPath;
        private System.Windows.Forms.Label labelVlcPath;
        private System.Windows.Forms.GroupBox groupBoxToolsAutoBr;
        private System.Windows.Forms.CheckBox checkBoxUseDoNotBreakAfterList;
        private System.Windows.Forms.Button buttonEditDoNotBreakAfterList;
        private System.Windows.Forms.CheckBox checkBoxCheckForUpdates;
        private System.Windows.Forms.Label labelPlatform;
        private System.Windows.Forms.CheckBox checkBoxVideoPlayerPreviewFontBold;
        private System.Windows.Forms.Label labelWaveformTextSize;
        private System.Windows.Forms.CheckBox checkBoxWaveformTextBold;
        private System.Windows.Forms.ComboBox comboBoxWaveformTextSize;
        private System.Windows.Forms.LinkLabel linkLabelOpenDictionaryFolder;
        private System.Windows.Forms.GroupBox groupBoxNetworkSession;
        private System.Windows.Forms.Button buttonNetworkSessionNewMessageSound;
        private System.Windows.Forms.TextBox textBoxNetworkSessionNewMessageSound;
        private System.Windows.Forms.Label labelNetworkSessionNewMessageSound;
        private System.Windows.Forms.GroupBox groupBoxMargins;
        private System.Windows.Forms.NumericUpDown numericUpDownSsaMarginVertical;
        private System.Windows.Forms.NumericUpDown numericUpDownSsaMarginRight;
        private System.Windows.Forms.NumericUpDown numericUpDownSsaMarginLeft;
        private System.Windows.Forms.Label labelMarginVertical;
        private System.Windows.Forms.Label labelMarginRight;
        private System.Windows.Forms.Label labelMarginLeft;
        private System.Windows.Forms.GroupBox groupBoxSsaBorder;
        private System.Windows.Forms.GroupBox groupSsaBoxFont;
        private System.Windows.Forms.NumericUpDown numericUpDownFontSize;
        private System.Windows.Forms.ComboBox comboBoxFontName;
        private System.Windows.Forms.Label labelSsaFontSize;
        private System.Windows.Forms.Label labelFontName;
        private System.Windows.Forms.Panel panelPrimaryColor;
        private System.Windows.Forms.Button buttonSsaColor;
        private System.Windows.Forms.CheckBox checkBoxSsaFontBold;
        private System.Windows.Forms.CheckBox checkBoxFceSkipStep1;
        private System.Windows.Forms.Button buttonMpvSettings;
        private System.Windows.Forms.Label labelMpvSettings;
        private System.Windows.Forms.LinkLabel linkLabelBingSubscribe;
        private System.Windows.Forms.Label labelUserBingApiId;
        private System.Windows.Forms.GroupBox groupBoxBing;
        private System.Windows.Forms.Label labelBingApiKey;
        private System.Windows.Forms.TextBox textBoxBingClientSecret;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBoxAutoBackupDeleteAfter;
        private System.Windows.Forms.Label labelAutoBackupDeleteAfter;
        private System.Windows.Forms.Label labelTBRemoveTextForHi;
        private System.Windows.Forms.PictureBox pictureBoxTBRemoveTextForHi;
        private System.Windows.Forms.CheckBox checkBoxTBRemoveTextForHi;
        private System.Windows.Forms.NumericUpDown numericUpDownMaxNumberOfLines;
        private System.Windows.Forms.Label labelMaxLines;
        private System.Windows.Forms.Label labelTBNetflixQualityCheck;
        private System.Windows.Forms.PictureBox pictureBoxNetflixQualityCheck;
        private System.Windows.Forms.CheckBox checkBoxNetflixQualityCheck;
        private System.Windows.Forms.CheckBox checkBoxWaveformSetVideoPosMoveStartEnd;
        private System.Windows.Forms.CheckBox checkBoxWaveformShowWpm;
        private System.Windows.Forms.CheckBox checkBoxWaveformShowCps;
        private System.Windows.Forms.NumericUpDown numericUpDownMaxWordsMin;
        private System.Windows.Forms.Label labelMaxWordsPerMin;
        private System.Windows.Forms.CheckBox checkBoxMpvHandlesPreviewText;
        private System.Windows.Forms.CheckBox checkBoxVideoAutoOpen;
        private System.Windows.Forms.Button buttonClearShortcut;
        private System.Windows.Forms.CheckBox checkBoxAllowVolumeBoost;
        private System.Windows.Forms.TabPage tabPageFont;
        private System.Windows.Forms.GroupBox groupBoxFont;
        private System.Windows.Forms.CheckBox checkBoxSubtitleCenter;
        private System.Windows.Forms.Panel panelSubtitleFontColor;
        private System.Windows.Forms.Panel panelSubtitleBackgroundColor;
        private System.Windows.Forms.Label labelSubtitleFontBackgroundColor;
        private System.Windows.Forms.Label labelSubtitleFontColor;
        private System.Windows.Forms.Label labelSubtitleFontSize;
        private System.Windows.Forms.ComboBox comboBoxSubtitleFont;
        private System.Windows.Forms.CheckBox checkBoxSubtitleFontBold;
        private System.Windows.Forms.ComboBox comboBoxSubtitleFontSize;
        private System.Windows.Forms.Label labelSubtitleFont;
        private System.Windows.Forms.ComboBox comboBoxSaveAsFileNameFrom;
        private System.Windows.Forms.Label labelSaveAsFileNameFrom;
        private System.Windows.Forms.GroupBox groupBoxGeneralRules;
        private System.Windows.Forms.GroupBox groupBoxFontTextBox;
        private System.Windows.Forms.GroupBox groupBoxFontListViews;
        private System.Windows.Forms.Label labelSubtitleListViewFontSize;
        private System.Windows.Forms.ComboBox comboBoxSubtitleListViewFontSize;
        private System.Windows.Forms.CheckBox checkBoxSubtitleListViewFontBold;
        private System.Windows.Forms.GroupBox groupBoxFontGeneral;
        private System.Windows.Forms.Label labelFontNote;
        private System.Windows.Forms.Button buttonDownloadFfmpeg;
        private System.Windows.Forms.Button buttonShortcutsClear;
        private System.Windows.Forms.Label labelShortcutsSearch;
        private System.Windows.Forms.TextBox textBoxShortcutSearch;
        private System.Windows.Forms.GroupBox groupBoxGoogleTranslate;
        private System.Windows.Forms.Label labelGoogleTranslateApiKey;
        private System.Windows.Forms.TextBox textBoxGoogleTransleApiKey;
        private System.Windows.Forms.LinkLabel linkLabelGoogleTranslateSignUp;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox checkBoxAutoSave;
        private System.Windows.Forms.CheckBox checkBoxUseAlwaysToFile;
        private System.Windows.Forms.Label labelOptimalCharsPerSecond;
        private System.Windows.Forms.NumericUpDown numericUpDownOptimalCharsSec;
        private System.Windows.Forms.Button buttonEditProfile;
        private System.Windows.Forms.ComboBox comboBoxRulesProfileName;
        private System.Windows.Forms.Label labelRulesProfileName;
        private System.Windows.Forms.CheckBox checkBoxCpsIncludeWhiteSpace;
        private System.Windows.Forms.Label labelBingTokenEndpoint;
        private System.Windows.Forms.TextBox textBoxBingTokenEndpoint;
        private System.Windows.Forms.CheckBox checkBoxToolsBreakEarlyComma;
        private System.Windows.Forms.CheckBox checkBoxToolsBreakEarlyDash;
        private System.Windows.Forms.CheckBox checkBoxToolsBreakEarlyLineEnding;
        private System.Windows.Forms.CheckBox checkBoxToolsBreakByPixelWidth;
        private System.Windows.Forms.NumericUpDown numericUpDownToolsBreakPreferBottomHeavy;
        private System.Windows.Forms.CheckBox checkBoxToolsBreakPreferBottomHeavy;
        private System.Windows.Forms.Label labelToolsBreakBottomHeavyPercent;
        private System.Windows.Forms.CheckBox checkBoxSyntaxColorGapTooSmall;
        private System.Windows.Forms.Button buttonReset;
    }
}