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
            this.tabPageGenerel = new System.Windows.Forms.TabPage();
            this.groupBoxMiscellaneous = new System.Windows.Forms.GroupBox();
            this.checkBoxCheckForUpdates = new System.Windows.Forms.CheckBox();
            this.numericUpDownMinGapMs = new System.Windows.Forms.NumericUpDown();
            this.labelMinGapMs = new System.Windows.Forms.Label();
            this.labelSpellChecker = new System.Windows.Forms.Label();
            this.checkBoxSubtitleCenter = new System.Windows.Forms.CheckBox();
            this.numericUpDownSubtitleLineMaximumLength = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownMaxCharsSec = new System.Windows.Forms.NumericUpDown();
            this.labelMaxCharsPerSecond = new System.Windows.Forms.Label();
            this.comboBoxTimeCodeMode = new System.Windows.Forms.ComboBox();
            this.labelTimeCodeMode = new System.Windows.Forms.Label();
            this.comboBoxEncoding = new System.Windows.Forms.ComboBox();
            this.checkBoxAutoDetectAnsiEncoding = new System.Windows.Forms.CheckBox();
            this.comboBoxMergeShortLineLength = new System.Windows.Forms.ComboBox();
            this.textBoxShowLineBreaksAs = new System.Windows.Forms.TextBox();
            this.checkBoxAutoWrapWhileTyping = new System.Windows.Forms.CheckBox();
            this.panelSubtitleFontColor = new System.Windows.Forms.Panel();
            this.panelSubtitleBackgroundColor = new System.Windows.Forms.Panel();
            this.numericUpDownDurationMax = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownDurationMin = new System.Windows.Forms.NumericUpDown();
            this.labelMaxDuration = new System.Windows.Forms.Label();
            this.labelMinDuration = new System.Windows.Forms.Label();
            this.checkBoxPromptDeleteLines = new System.Windows.Forms.CheckBox();
            this.labelMergeShortLines = new System.Windows.Forms.Label();
            this.checkBoxAllowEditOfOriginalSubtitle = new System.Windows.Forms.CheckBox();
            this.comboBoxSpellChecker = new System.Windows.Forms.ComboBox();
            this.labelSubtitleFontBackgroundColor = new System.Windows.Forms.Label();
            this.labelSubtitleFontColor = new System.Windows.Forms.Label();
            this.comboBoxAutoBackup = new System.Windows.Forms.ComboBox();
            this.labelAutoBackup = new System.Windows.Forms.Label();
            this.checkBoxRememberSelectedLine = new System.Windows.Forms.CheckBox();
            this.checkBoxRemoveBlankLinesWhenOpening = new System.Windows.Forms.CheckBox();
            this.labelAutoDetectAnsiEncoding = new System.Windows.Forms.Label();
            this.comboBoxListViewDoubleClickEvent = new System.Windows.Forms.ComboBox();
            this.labelListViewDoubleClickEvent = new System.Windows.Forms.Label();
            this.labelShowLineBreaksAs = new System.Windows.Forms.Label();
            this.checkBoxRememberWindowPosition = new System.Windows.Forms.CheckBox();
            this.labelSubMaxLen = new System.Windows.Forms.Label();
            this.labelSubtitleFontSize = new System.Windows.Forms.Label();
            this.comboBoxSubtitleFont = new System.Windows.Forms.ComboBox();
            this.checkBoxStartInSourceView = new System.Windows.Forms.CheckBox();
            this.checkBoxReopenLastOpened = new System.Windows.Forms.CheckBox();
            this.checkBoxRememberRecentFiles = new System.Windows.Forms.CheckBox();
            this.checkBoxSubtitleFontBold = new System.Windows.Forms.CheckBox();
            this.comboBoxSubtitleFontSize = new System.Windows.Forms.ComboBox();
            this.labelSubtitleFont = new System.Windows.Forms.Label();
            this.labelDefaultFileEncoding = new System.Windows.Forms.Label();
            this.comboBoxFrameRate = new System.Windows.Forms.ComboBox();
            this.labelDefaultFrameRate = new System.Windows.Forms.Label();
            this.tabPageToolBar = new System.Windows.Forms.TabPage();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.checkBoxShowFrameRate = new System.Windows.Forms.CheckBox();
            this.groupBoxShowToolBarButtons = new System.Windows.Forms.GroupBox();
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
            this.tabPageVideoPlayer = new System.Windows.Forms.TabPage();
            this.groupBoxMainWindowVideoControls = new System.Windows.Forms.GroupBox();
            this.labelCustomSearch6 = new System.Windows.Forms.Label();
            this.textBoxCustomSearchUrl6 = new System.Windows.Forms.TextBox();
            this.comboBoxCustomSearch6 = new System.Windows.Forms.ComboBox();
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
            this.checkBoxVideoPlayerPreviewFontBold = new System.Windows.Forms.CheckBox();
            this.checkBoxVideoPlayerShowFullscreenButton = new System.Windows.Forms.CheckBox();
            this.checkBoxVideoPlayerShowMuteButton = new System.Windows.Forms.CheckBox();
            this.labelVideoPlayerPreviewFontSize = new System.Windows.Forms.Label();
            this.comboBoxlVideoPlayerPreviewFontSize = new System.Windows.Forms.ComboBox();
            this.checkBoxVideoPlayerShowStopButton = new System.Windows.Forms.CheckBox();
            this.groupBoxVideoEngine = new System.Windows.Forms.GroupBox();
            this.labelPlatform = new System.Windows.Forms.Label();
            this.buttonVlcPathBrowse = new System.Windows.Forms.Button();
            this.textBoxVlcPath = new System.Windows.Forms.TextBox();
            this.labelVlcPath = new System.Windows.Forms.Label();
            this.labelVideoPlayerVLC = new System.Windows.Forms.Label();
            this.radioButtonVideoPlayerVLC = new System.Windows.Forms.RadioButton();
            this.labelVideoPlayerMPlayer = new System.Windows.Forms.Label();
            this.labelDirectShowDescription = new System.Windows.Forms.Label();
            this.labelMpcHcDescription = new System.Windows.Forms.Label();
            this.radioButtonVideoPlayerMPlayer = new System.Windows.Forms.RadioButton();
            this.radioButtonVideoPlayerDirectShow = new System.Windows.Forms.RadioButton();
            this.radioButtonVideoPlayerMpcHc = new System.Windows.Forms.RadioButton();
            this.tabPageWaveform = new System.Windows.Forms.TabPage();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
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
            this.groupBoxToolsMisc = new System.Windows.Forms.GroupBox();
            this.buttonEditDoNotBreakAfterList = new System.Windows.Forms.Button();
            this.checkBoxUseDoNotBreakAfterList = new System.Windows.Forms.CheckBox();
            this.groupBoxSpellCheck = new System.Windows.Forms.GroupBox();
            this.checkBoxTreatINQuoteAsING = new System.Windows.Forms.CheckBox();
            this.checkBoxSpellCheckOneLetterWords = new System.Windows.Forms.CheckBox();
            this.checkBoxSpellCheckAutoChangeNames = new System.Windows.Forms.CheckBox();
            this.groupBoxFixCommonErrors = new System.Windows.Forms.GroupBox();
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
            this.checkBoxNamesEtcOnline = new System.Windows.Forms.CheckBox();
            this.textBoxNamesEtcOnline = new System.Windows.Forms.TextBox();
            this.groupBoxNamesIgonoreLists = new System.Windows.Forms.GroupBox();
            this.buttonRemoveNameEtc = new System.Windows.Forms.Button();
            this.listBoxNamesEtc = new System.Windows.Forms.ListBox();
            this.textBoxNameEtc = new System.Windows.Forms.TextBox();
            this.buttonAddNamesEtc = new System.Windows.Forms.Button();
            this.labelWordListLanguage = new System.Windows.Forms.Label();
            this.comboBoxWordListLanguage = new System.Windows.Forms.ComboBox();
            this.tabPageSsaStyle = new System.Windows.Forms.TabPage();
            this.groupBoxSsaStyle = new System.Windows.Forms.GroupBox();
            this.checkBoxSsaOpaqueBox = new System.Windows.Forms.CheckBox();
            this.groupBoxPreview = new System.Windows.Forms.GroupBox();
            this.pictureBoxPreview = new System.Windows.Forms.PictureBox();
            this.labelSsaOutline = new System.Windows.Forms.Label();
            this.numericUpDownSsaShadow = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownSsaOutline = new System.Windows.Forms.NumericUpDown();
            this.labelSsaShadow = new System.Windows.Forms.Label();
            this.labelSSAFont = new System.Windows.Forms.Label();
            this.buttonSSAChooseColor = new System.Windows.Forms.Button();
            this.buttonSSAChooseFont = new System.Windows.Forms.Button();
            this.tabPageProxy = new System.Windows.Forms.TabPage();
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
            this.tabPageShortcuts = new System.Windows.Forms.TabPage();
            this.groupBoxShortcuts = new System.Windows.Forms.GroupBox();
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
            this.numericUpDownSyntaxColorTextMoreThanXLines = new System.Windows.Forms.NumericUpDown();
            this.checkBoxSyntaxColorTextMoreThanTwoLines = new System.Windows.Forms.CheckBox();
            this.checkBoxSyntaxOverlap = new System.Windows.Forms.CheckBox();
            this.checkBoxSyntaxColorDurationTooSmall = new System.Windows.Forms.CheckBox();
            this.buttonListViewSyntaxColorError = new System.Windows.Forms.Button();
            this.checkBoxSyntaxColorTextTooLong = new System.Windows.Forms.CheckBox();
            this.checkBoxSyntaxColorDurationTooLarge = new System.Windows.Forms.CheckBox();
            this.panelListViewSyntaxColorError = new System.Windows.Forms.Panel();
            this.colorDialogSSAStyle = new System.Windows.Forms.ColorDialog();
            this.fontDialogSSAStyle = new System.Windows.Forms.FontDialog();
            this.labelStatus = new System.Windows.Forms.Label();
            this.openFileDialogFFmpeg = new System.Windows.Forms.OpenFileDialog();
            this.tabControlSettings.SuspendLayout();
            this.tabPageGenerel.SuspendLayout();
            this.groupBoxMiscellaneous.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMinGapMs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSubtitleLineMaximumLength)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxCharsSec)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDurationMax)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDurationMin)).BeginInit();
            this.tabPageToolBar.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBoxShowToolBarButtons.SuspendLayout();
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
            this.groupBoxToolsMisc.SuspendLayout();
            this.groupBoxSpellCheck.SuspendLayout();
            this.groupBoxFixCommonErrors.SuspendLayout();
            this.groupBoxToolsVisualSync.SuspendLayout();
            this.tabPageWordLists.SuspendLayout();
            this.groupBoxWordLists.SuspendLayout();
            this.groupBoxOcrFixList.SuspendLayout();
            this.groupBoxUserWordList.SuspendLayout();
            this.groupBoxWordListLocation.SuspendLayout();
            this.groupBoxNamesIgonoreLists.SuspendLayout();
            this.tabPageSsaStyle.SuspendLayout();
            this.groupBoxSsaStyle.SuspendLayout();
            this.groupBoxPreview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreview)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSsaShadow)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSsaOutline)).BeginInit();
            this.tabPageProxy.SuspendLayout();
            this.groupBoxProxySettings.SuspendLayout();
            this.groupBoxProxyAuthentication.SuspendLayout();
            this.tabPageShortcuts.SuspendLayout();
            this.groupBoxShortcuts.SuspendLayout();
            this.tabPageSyntaxColoring.SuspendLayout();
            this.groupBoxListViewSyntaxColoring.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSyntaxColorTextMoreThanXLines)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(686, 489);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 21);
            this.buttonOK.TabIndex = 0;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.ButtonOkClick);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(767, 489);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 21);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // tabControlSettings
            // 
            this.tabControlSettings.Controls.Add(this.tabPageGenerel);
            this.tabControlSettings.Controls.Add(this.tabPageToolBar);
            this.tabControlSettings.Controls.Add(this.tabPageVideoPlayer);
            this.tabControlSettings.Controls.Add(this.tabPageWaveform);
            this.tabControlSettings.Controls.Add(this.tabPageTools);
            this.tabControlSettings.Controls.Add(this.tabPageWordLists);
            this.tabControlSettings.Controls.Add(this.tabPageSsaStyle);
            this.tabControlSettings.Controls.Add(this.tabPageProxy);
            this.tabControlSettings.Controls.Add(this.tabPageShortcuts);
            this.tabControlSettings.Controls.Add(this.tabPageSyntaxColoring);
            this.tabControlSettings.Location = new System.Drawing.Point(13, 13);
            this.tabControlSettings.Name = "tabControlSettings";
            this.tabControlSettings.SelectedIndex = 0;
            this.tabControlSettings.Size = new System.Drawing.Size(833, 470);
            this.tabControlSettings.TabIndex = 2;
            this.tabControlSettings.SelectedIndexChanged += new System.EventHandler(this.TabControlSettingsSelectedIndexChanged);
            // 
            // tabPageGenerel
            // 
            this.tabPageGenerel.Controls.Add(this.groupBoxMiscellaneous);
            this.tabPageGenerel.Location = new System.Drawing.Point(4, 22);
            this.tabPageGenerel.Name = "tabPageGenerel";
            this.tabPageGenerel.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageGenerel.Size = new System.Drawing.Size(825, 444);
            this.tabPageGenerel.TabIndex = 0;
            this.tabPageGenerel.Text = "Generel";
            this.tabPageGenerel.UseVisualStyleBackColor = true;
            // 
            // groupBoxMiscellaneous
            // 
            this.groupBoxMiscellaneous.Controls.Add(this.checkBoxCheckForUpdates);
            this.groupBoxMiscellaneous.Controls.Add(this.numericUpDownMinGapMs);
            this.groupBoxMiscellaneous.Controls.Add(this.labelMinGapMs);
            this.groupBoxMiscellaneous.Controls.Add(this.labelSpellChecker);
            this.groupBoxMiscellaneous.Controls.Add(this.checkBoxSubtitleCenter);
            this.groupBoxMiscellaneous.Controls.Add(this.numericUpDownSubtitleLineMaximumLength);
            this.groupBoxMiscellaneous.Controls.Add(this.numericUpDownMaxCharsSec);
            this.groupBoxMiscellaneous.Controls.Add(this.labelMaxCharsPerSecond);
            this.groupBoxMiscellaneous.Controls.Add(this.comboBoxTimeCodeMode);
            this.groupBoxMiscellaneous.Controls.Add(this.labelTimeCodeMode);
            this.groupBoxMiscellaneous.Controls.Add(this.comboBoxEncoding);
            this.groupBoxMiscellaneous.Controls.Add(this.checkBoxAutoDetectAnsiEncoding);
            this.groupBoxMiscellaneous.Controls.Add(this.comboBoxMergeShortLineLength);
            this.groupBoxMiscellaneous.Controls.Add(this.textBoxShowLineBreaksAs);
            this.groupBoxMiscellaneous.Controls.Add(this.checkBoxAutoWrapWhileTyping);
            this.groupBoxMiscellaneous.Controls.Add(this.panelSubtitleFontColor);
            this.groupBoxMiscellaneous.Controls.Add(this.panelSubtitleBackgroundColor);
            this.groupBoxMiscellaneous.Controls.Add(this.numericUpDownDurationMax);
            this.groupBoxMiscellaneous.Controls.Add(this.numericUpDownDurationMin);
            this.groupBoxMiscellaneous.Controls.Add(this.labelMaxDuration);
            this.groupBoxMiscellaneous.Controls.Add(this.labelMinDuration);
            this.groupBoxMiscellaneous.Controls.Add(this.checkBoxPromptDeleteLines);
            this.groupBoxMiscellaneous.Controls.Add(this.labelMergeShortLines);
            this.groupBoxMiscellaneous.Controls.Add(this.checkBoxAllowEditOfOriginalSubtitle);
            this.groupBoxMiscellaneous.Controls.Add(this.comboBoxSpellChecker);
            this.groupBoxMiscellaneous.Controls.Add(this.labelSubtitleFontBackgroundColor);
            this.groupBoxMiscellaneous.Controls.Add(this.labelSubtitleFontColor);
            this.groupBoxMiscellaneous.Controls.Add(this.comboBoxAutoBackup);
            this.groupBoxMiscellaneous.Controls.Add(this.labelAutoBackup);
            this.groupBoxMiscellaneous.Controls.Add(this.checkBoxRememberSelectedLine);
            this.groupBoxMiscellaneous.Controls.Add(this.checkBoxRemoveBlankLinesWhenOpening);
            this.groupBoxMiscellaneous.Controls.Add(this.labelAutoDetectAnsiEncoding);
            this.groupBoxMiscellaneous.Controls.Add(this.comboBoxListViewDoubleClickEvent);
            this.groupBoxMiscellaneous.Controls.Add(this.labelListViewDoubleClickEvent);
            this.groupBoxMiscellaneous.Controls.Add(this.labelShowLineBreaksAs);
            this.groupBoxMiscellaneous.Controls.Add(this.checkBoxRememberWindowPosition);
            this.groupBoxMiscellaneous.Controls.Add(this.labelSubMaxLen);
            this.groupBoxMiscellaneous.Controls.Add(this.labelSubtitleFontSize);
            this.groupBoxMiscellaneous.Controls.Add(this.comboBoxSubtitleFont);
            this.groupBoxMiscellaneous.Controls.Add(this.checkBoxStartInSourceView);
            this.groupBoxMiscellaneous.Controls.Add(this.checkBoxReopenLastOpened);
            this.groupBoxMiscellaneous.Controls.Add(this.checkBoxRememberRecentFiles);
            this.groupBoxMiscellaneous.Controls.Add(this.checkBoxSubtitleFontBold);
            this.groupBoxMiscellaneous.Controls.Add(this.comboBoxSubtitleFontSize);
            this.groupBoxMiscellaneous.Controls.Add(this.labelSubtitleFont);
            this.groupBoxMiscellaneous.Controls.Add(this.labelDefaultFileEncoding);
            this.groupBoxMiscellaneous.Controls.Add(this.comboBoxFrameRate);
            this.groupBoxMiscellaneous.Controls.Add(this.labelDefaultFrameRate);
            this.groupBoxMiscellaneous.Location = new System.Drawing.Point(6, 6);
            this.groupBoxMiscellaneous.Name = "groupBoxMiscellaneous";
            this.groupBoxMiscellaneous.Size = new System.Drawing.Size(813, 432);
            this.groupBoxMiscellaneous.TabIndex = 0;
            this.groupBoxMiscellaneous.TabStop = false;
            this.groupBoxMiscellaneous.Text = "Miscellaneous";
            // 
            // checkBoxCheckForUpdates
            // 
            this.checkBoxCheckForUpdates.AutoSize = true;
            this.checkBoxCheckForUpdates.Location = new System.Drawing.Point(441, 399);
            this.checkBoxCheckForUpdates.Name = "checkBoxCheckForUpdates";
            this.checkBoxCheckForUpdates.Size = new System.Drawing.Size(114, 17);
            this.checkBoxCheckForUpdates.TabIndex = 45;
            this.checkBoxCheckForUpdates.Text = "Check for updates";
            this.checkBoxCheckForUpdates.UseVisualStyleBackColor = true;
            // 
            // numericUpDownMinGapMs
            // 
            this.numericUpDownMinGapMs.Location = new System.Drawing.Point(205, 226);
            this.numericUpDownMinGapMs.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownMinGapMs.Name = "numericUpDownMinGapMs";
            this.numericUpDownMinGapMs.Size = new System.Drawing.Size(56, 21);
            this.numericUpDownMinGapMs.TabIndex = 15;
            this.numericUpDownMinGapMs.Value = new decimal(new int[] {
            25,
            0,
            0,
            0});
            // 
            // labelMinGapMs
            // 
            this.labelMinGapMs.AutoSize = true;
            this.labelMinGapMs.Location = new System.Drawing.Point(8, 228);
            this.labelMinGapMs.Name = "labelMinGapMs";
            this.labelMinGapMs.Size = new System.Drawing.Size(136, 13);
            this.labelMinGapMs.TabIndex = 14;
            this.labelMinGapMs.Text = "Min. gap between subtitles";
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
            // checkBoxSubtitleCenter
            // 
            this.checkBoxSubtitleCenter.AutoSize = true;
            this.checkBoxSubtitleCenter.Location = new System.Drawing.Point(271, 368);
            this.checkBoxSubtitleCenter.Name = "checkBoxSubtitleCenter";
            this.checkBoxSubtitleCenter.Size = new System.Drawing.Size(59, 17);
            this.checkBoxSubtitleCenter.TabIndex = 23;
            this.checkBoxSubtitleCenter.Text = "Center";
            this.checkBoxSubtitleCenter.UseVisualStyleBackColor = true;
            // 
            // numericUpDownSubtitleLineMaximumLength
            // 
            this.numericUpDownSubtitleLineMaximumLength.Location = new System.Drawing.Point(205, 122);
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
            this.numericUpDownSubtitleLineMaximumLength.TabIndex = 7;
            this.numericUpDownSubtitleLineMaximumLength.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // numericUpDownMaxCharsSec
            // 
            this.numericUpDownMaxCharsSec.DecimalPlaces = 1;
            this.numericUpDownMaxCharsSec.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericUpDownMaxCharsSec.Location = new System.Drawing.Point(205, 148);
            this.numericUpDownMaxCharsSec.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.numericUpDownMaxCharsSec.Name = "numericUpDownMaxCharsSec";
            this.numericUpDownMaxCharsSec.Size = new System.Drawing.Size(56, 21);
            this.numericUpDownMaxCharsSec.TabIndex = 9;
            this.numericUpDownMaxCharsSec.Value = new decimal(new int[] {
            24,
            0,
            0,
            0});
            // 
            // labelMaxCharsPerSecond
            // 
            this.labelMaxCharsPerSecond.AutoSize = true;
            this.labelMaxCharsPerSecond.Location = new System.Drawing.Point(8, 150);
            this.labelMaxCharsPerSecond.Name = "labelMaxCharsPerSecond";
            this.labelMaxCharsPerSecond.Size = new System.Drawing.Size(80, 13);
            this.labelMaxCharsPerSecond.TabIndex = 8;
            this.labelMaxCharsPerSecond.Text = "Max. chars/sec";
            // 
            // comboBoxTimeCodeMode
            // 
            this.comboBoxTimeCodeMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxTimeCodeMode.FormattingEnabled = true;
            this.comboBoxTimeCodeMode.Items.AddRange(new object[] {
            "HH:MM:SS:MSEC (00:00:01.500)",
            "HH:MM:SS:FF (01:00:02.12)"});
            this.comboBoxTimeCodeMode.Location = new System.Drawing.Point(528, 265);
            this.comboBoxTimeCodeMode.Name = "comboBoxTimeCodeMode";
            this.comboBoxTimeCodeMode.Size = new System.Drawing.Size(207, 21);
            this.comboBoxTimeCodeMode.TabIndex = 40;
            // 
            // labelTimeCodeMode
            // 
            this.labelTimeCodeMode.AutoSize = true;
            this.labelTimeCodeMode.Location = new System.Drawing.Point(438, 268);
            this.labelTimeCodeMode.Name = "labelTimeCodeMode";
            this.labelTimeCodeMode.Size = new System.Drawing.Size(84, 13);
            this.labelTimeCodeMode.TabIndex = 39;
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
            this.comboBoxEncoding.Location = new System.Drawing.Point(205, 52);
            this.comboBoxEncoding.Name = "comboBoxEncoding";
            this.comboBoxEncoding.Size = new System.Drawing.Size(188, 21);
            this.comboBoxEncoding.TabIndex = 3;
            // 
            // checkBoxAutoDetectAnsiEncoding
            // 
            this.checkBoxAutoDetectAnsiEncoding.AutoSize = true;
            this.checkBoxAutoDetectAnsiEncoding.Location = new System.Drawing.Point(205, 81);
            this.checkBoxAutoDetectAnsiEncoding.Name = "checkBoxAutoDetectAnsiEncoding";
            this.checkBoxAutoDetectAnsiEncoding.Size = new System.Drawing.Size(15, 14);
            this.checkBoxAutoDetectAnsiEncoding.TabIndex = 5;
            this.checkBoxAutoDetectAnsiEncoding.UseVisualStyleBackColor = true;
            // 
            // comboBoxMergeShortLineLength
            // 
            this.comboBoxMergeShortLineLength.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxMergeShortLineLength.FormattingEnabled = true;
            this.comboBoxMergeShortLineLength.Location = new System.Drawing.Point(205, 253);
            this.comboBoxMergeShortLineLength.Name = "comboBoxMergeShortLineLength";
            this.comboBoxMergeShortLineLength.Size = new System.Drawing.Size(73, 21);
            this.comboBoxMergeShortLineLength.TabIndex = 17;
            // 
            // textBoxShowLineBreaksAs
            // 
            this.textBoxShowLineBreaksAs.Location = new System.Drawing.Point(594, 237);
            this.textBoxShowLineBreaksAs.MaxLength = 10;
            this.textBoxShowLineBreaksAs.Name = "textBoxShowLineBreaksAs";
            this.textBoxShowLineBreaksAs.Size = new System.Drawing.Size(60, 21);
            this.textBoxShowLineBreaksAs.TabIndex = 38;
            // 
            // checkBoxAutoWrapWhileTyping
            // 
            this.checkBoxAutoWrapWhileTyping.AutoSize = true;
            this.checkBoxAutoWrapWhileTyping.Location = new System.Drawing.Point(441, 212);
            this.checkBoxAutoWrapWhileTyping.Name = "checkBoxAutoWrapWhileTyping";
            this.checkBoxAutoWrapWhileTyping.Size = new System.Drawing.Size(137, 17);
            this.checkBoxAutoWrapWhileTyping.TabIndex = 36;
            this.checkBoxAutoWrapWhileTyping.Text = "Auto-wrap while typing";
            this.checkBoxAutoWrapWhileTyping.UseVisualStyleBackColor = true;
            // 
            // panelSubtitleFontColor
            // 
            this.panelSubtitleFontColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelSubtitleFontColor.Location = new System.Drawing.Point(205, 390);
            this.panelSubtitleFontColor.Name = "panelSubtitleFontColor";
            this.panelSubtitleFontColor.Size = new System.Drawing.Size(46, 15);
            this.panelSubtitleFontColor.TabIndex = 25;
            this.panelSubtitleFontColor.Click += new System.EventHandler(this.panelSubtitleFontColor_Click);
            // 
            // panelSubtitleBackgroundColor
            // 
            this.panelSubtitleBackgroundColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelSubtitleBackgroundColor.Location = new System.Drawing.Point(205, 410);
            this.panelSubtitleBackgroundColor.Name = "panelSubtitleBackgroundColor";
            this.panelSubtitleBackgroundColor.Size = new System.Drawing.Size(46, 15);
            this.panelSubtitleBackgroundColor.TabIndex = 27;
            this.panelSubtitleBackgroundColor.Click += new System.EventHandler(this.panelSubtitleBackgroundColor_Click);
            // 
            // numericUpDownDurationMax
            // 
            this.numericUpDownDurationMax.Location = new System.Drawing.Point(205, 200);
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
            this.numericUpDownDurationMax.TabIndex = 13;
            this.numericUpDownDurationMax.Value = new decimal(new int[] {
            50000,
            0,
            0,
            0});
            // 
            // numericUpDownDurationMin
            // 
            this.numericUpDownDurationMin.Location = new System.Drawing.Point(205, 174);
            this.numericUpDownDurationMin.Maximum = new decimal(new int[] {
            2000,
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
            this.numericUpDownDurationMin.TabIndex = 11;
            this.numericUpDownDurationMin.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // labelMaxDuration
            // 
            this.labelMaxDuration.AutoSize = true;
            this.labelMaxDuration.Location = new System.Drawing.Point(8, 202);
            this.labelMaxDuration.Name = "labelMaxDuration";
            this.labelMaxDuration.Size = new System.Drawing.Size(136, 13);
            this.labelMaxDuration.TabIndex = 12;
            this.labelMaxDuration.Text = "Max. duration, milliseconds";
            // 
            // labelMinDuration
            // 
            this.labelMinDuration.AutoSize = true;
            this.labelMinDuration.Location = new System.Drawing.Point(8, 176);
            this.labelMinDuration.Name = "labelMinDuration";
            this.labelMinDuration.Size = new System.Drawing.Size(132, 13);
            this.labelMinDuration.TabIndex = 10;
            this.labelMinDuration.Text = "Min. duration, milliseconds";
            // 
            // checkBoxPromptDeleteLines
            // 
            this.checkBoxPromptDeleteLines.AutoSize = true;
            this.checkBoxPromptDeleteLines.Location = new System.Drawing.Point(441, 189);
            this.checkBoxPromptDeleteLines.Name = "checkBoxPromptDeleteLines";
            this.checkBoxPromptDeleteLines.Size = new System.Drawing.Size(142, 17);
            this.checkBoxPromptDeleteLines.TabIndex = 35;
            this.checkBoxPromptDeleteLines.Text = "Prompt for deleting lines";
            this.checkBoxPromptDeleteLines.UseVisualStyleBackColor = true;
            // 
            // labelMergeShortLines
            // 
            this.labelMergeShortLines.AutoSize = true;
            this.labelMergeShortLines.Location = new System.Drawing.Point(8, 256);
            this.labelMergeShortLines.Name = "labelMergeShortLines";
            this.labelMergeShortLines.Size = new System.Drawing.Size(124, 13);
            this.labelMergeShortLines.TabIndex = 16;
            this.labelMergeShortLines.Text = "Merge lines shorter than";
            // 
            // checkBoxAllowEditOfOriginalSubtitle
            // 
            this.checkBoxAllowEditOfOriginalSubtitle.AutoSize = true;
            this.checkBoxAllowEditOfOriginalSubtitle.Location = new System.Drawing.Point(441, 166);
            this.checkBoxAllowEditOfOriginalSubtitle.Name = "checkBoxAllowEditOfOriginalSubtitle";
            this.checkBoxAllowEditOfOriginalSubtitle.Size = new System.Drawing.Size(160, 17);
            this.checkBoxAllowEditOfOriginalSubtitle.TabIndex = 34;
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
            // labelSubtitleFontBackgroundColor
            // 
            this.labelSubtitleFontBackgroundColor.AutoSize = true;
            this.labelSubtitleFontBackgroundColor.Location = new System.Drawing.Point(8, 409);
            this.labelSubtitleFontBackgroundColor.Name = "labelSubtitleFontBackgroundColor";
            this.labelSubtitleFontBackgroundColor.Size = new System.Drawing.Size(151, 13);
            this.labelSubtitleFontBackgroundColor.TabIndex = 26;
            this.labelSubtitleFontBackgroundColor.Text = "Subtitle font background color";
            // 
            // labelSubtitleFontColor
            // 
            this.labelSubtitleFontColor.AutoSize = true;
            this.labelSubtitleFontColor.Location = new System.Drawing.Point(8, 389);
            this.labelSubtitleFontColor.Name = "labelSubtitleFontColor";
            this.labelSubtitleFontColor.Size = new System.Drawing.Size(92, 13);
            this.labelSubtitleFontColor.TabIndex = 24;
            this.labelSubtitleFontColor.Text = "Subtitle font color";
            // 
            // comboBoxAutoBackup
            // 
            this.comboBoxAutoBackup.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxAutoBackup.FormattingEnabled = true;
            this.comboBoxAutoBackup.Items.AddRange(new object[] {
            "None",
            "Every minute",
            "Every 5 minutes",
            "Evert 15 minutes"});
            this.comboBoxAutoBackup.Location = new System.Drawing.Point(528, 366);
            this.comboBoxAutoBackup.Name = "comboBoxAutoBackup";
            this.comboBoxAutoBackup.Size = new System.Drawing.Size(121, 21);
            this.comboBoxAutoBackup.TabIndex = 44;
            // 
            // labelAutoBackup
            // 
            this.labelAutoBackup.AutoSize = true;
            this.labelAutoBackup.Location = new System.Drawing.Point(438, 368);
            this.labelAutoBackup.Name = "labelAutoBackup";
            this.labelAutoBackup.Size = new System.Drawing.Size(68, 13);
            this.labelAutoBackup.TabIndex = 43;
            this.labelAutoBackup.Text = "Auto-backup";
            // 
            // checkBoxRememberSelectedLine
            // 
            this.checkBoxRememberSelectedLine.AutoSize = true;
            this.checkBoxRememberSelectedLine.Location = new System.Drawing.Point(449, 71);
            this.checkBoxRememberSelectedLine.Name = "checkBoxRememberSelectedLine";
            this.checkBoxRememberSelectedLine.Size = new System.Drawing.Size(139, 17);
            this.checkBoxRememberSelectedLine.TabIndex = 30;
            this.checkBoxRememberSelectedLine.Text = "Remember selected line";
            this.checkBoxRememberSelectedLine.UseVisualStyleBackColor = true;
            // 
            // checkBoxRemoveBlankLinesWhenOpening
            // 
            this.checkBoxRemoveBlankLinesWhenOpening.AutoSize = true;
            this.checkBoxRemoveBlankLinesWhenOpening.Location = new System.Drawing.Point(441, 143);
            this.checkBoxRemoveBlankLinesWhenOpening.Name = "checkBoxRemoveBlankLinesWhenOpening";
            this.checkBoxRemoveBlankLinesWhenOpening.Size = new System.Drawing.Size(225, 17);
            this.checkBoxRemoveBlankLinesWhenOpening.TabIndex = 33;
            this.checkBoxRemoveBlankLinesWhenOpening.Text = "Remove blank lines when opening subtitle";
            this.checkBoxRemoveBlankLinesWhenOpening.UseVisualStyleBackColor = true;
            // 
            // labelAutoDetectAnsiEncoding
            // 
            this.labelAutoDetectAnsiEncoding.AutoSize = true;
            this.labelAutoDetectAnsiEncoding.Location = new System.Drawing.Point(8, 80);
            this.labelAutoDetectAnsiEncoding.Name = "labelAutoDetectAnsiEncoding";
            this.labelAutoDetectAnsiEncoding.Size = new System.Drawing.Size(137, 13);
            this.labelAutoDetectAnsiEncoding.TabIndex = 4;
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
            this.comboBoxListViewDoubleClickEvent.Location = new System.Drawing.Point(441, 311);
            this.comboBoxListViewDoubleClickEvent.Name = "comboBoxListViewDoubleClickEvent";
            this.comboBoxListViewDoubleClickEvent.Size = new System.Drawing.Size(332, 21);
            this.comboBoxListViewDoubleClickEvent.TabIndex = 42;
            // 
            // labelListViewDoubleClickEvent
            // 
            this.labelListViewDoubleClickEvent.AutoSize = true;
            this.labelListViewDoubleClickEvent.Location = new System.Drawing.Point(438, 295);
            this.labelListViewDoubleClickEvent.Name = "labelListViewDoubleClickEvent";
            this.labelListViewDoubleClickEvent.Size = new System.Drawing.Size(227, 13);
            this.labelListViewDoubleClickEvent.TabIndex = 41;
            this.labelListViewDoubleClickEvent.Text = "Double-click on line in main window listview will";
            // 
            // labelShowLineBreaksAs
            // 
            this.labelShowLineBreaksAs.AutoSize = true;
            this.labelShowLineBreaksAs.Location = new System.Drawing.Point(438, 240);
            this.labelShowLineBreaksAs.Name = "labelShowLineBreaksAs";
            this.labelShowLineBreaksAs.Size = new System.Drawing.Size(150, 13);
            this.labelShowLineBreaksAs.TabIndex = 37;
            this.labelShowLineBreaksAs.Text = "Show line breaks in listview as";
            // 
            // checkBoxRememberWindowPosition
            // 
            this.checkBoxRememberWindowPosition.AutoSize = true;
            this.checkBoxRememberWindowPosition.Location = new System.Drawing.Point(441, 97);
            this.checkBoxRememberWindowPosition.Name = "checkBoxRememberWindowPosition";
            this.checkBoxRememberWindowPosition.Size = new System.Drawing.Size(223, 17);
            this.checkBoxRememberWindowPosition.TabIndex = 31;
            this.checkBoxRememberWindowPosition.Text = "Remember main window position and size";
            this.checkBoxRememberWindowPosition.UseVisualStyleBackColor = true;
            // 
            // labelSubMaxLen
            // 
            this.labelSubMaxLen.AutoSize = true;
            this.labelSubMaxLen.Location = new System.Drawing.Point(8, 124);
            this.labelSubMaxLen.Name = "labelSubMaxLen";
            this.labelSubMaxLen.Size = new System.Drawing.Size(103, 13);
            this.labelSubMaxLen.TabIndex = 6;
            this.labelSubMaxLen.Text = "Subtitle max. length";
            // 
            // labelSubtitleFontSize
            // 
            this.labelSubtitleFontSize.AutoSize = true;
            this.labelSubtitleFontSize.Location = new System.Drawing.Point(8, 344);
            this.labelSubtitleFontSize.Name = "labelSubtitleFontSize";
            this.labelSubtitleFontSize.Size = new System.Drawing.Size(87, 13);
            this.labelSubtitleFontSize.TabIndex = 20;
            this.labelSubtitleFontSize.Text = "Subtitle font size";
            // 
            // comboBoxSubtitleFont
            // 
            this.comboBoxSubtitleFont.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSubtitleFont.FormattingEnabled = true;
            this.comboBoxSubtitleFont.Location = new System.Drawing.Point(205, 313);
            this.comboBoxSubtitleFont.Name = "comboBoxSubtitleFont";
            this.comboBoxSubtitleFont.Size = new System.Drawing.Size(188, 21);
            this.comboBoxSubtitleFont.TabIndex = 19;
            // 
            // checkBoxStartInSourceView
            // 
            this.checkBoxStartInSourceView.AutoSize = true;
            this.checkBoxStartInSourceView.Location = new System.Drawing.Point(441, 120);
            this.checkBoxStartInSourceView.Name = "checkBoxStartInSourceView";
            this.checkBoxStartInSourceView.Size = new System.Drawing.Size(121, 17);
            this.checkBoxStartInSourceView.TabIndex = 32;
            this.checkBoxStartInSourceView.Text = "Start in source view";
            this.checkBoxStartInSourceView.UseVisualStyleBackColor = true;
            // 
            // checkBoxReopenLastOpened
            // 
            this.checkBoxReopenLastOpened.AutoSize = true;
            this.checkBoxReopenLastOpened.Location = new System.Drawing.Point(449, 48);
            this.checkBoxReopenLastOpened.Name = "checkBoxReopenLastOpened";
            this.checkBoxReopenLastOpened.Size = new System.Drawing.Size(145, 17);
            this.checkBoxReopenLastOpened.TabIndex = 29;
            this.checkBoxReopenLastOpened.Text = "Start with last file loaded";
            this.checkBoxReopenLastOpened.UseVisualStyleBackColor = true;
            // 
            // checkBoxRememberRecentFiles
            // 
            this.checkBoxRememberRecentFiles.AutoSize = true;
            this.checkBoxRememberRecentFiles.Location = new System.Drawing.Point(441, 24);
            this.checkBoxRememberRecentFiles.Name = "checkBoxRememberRecentFiles";
            this.checkBoxRememberRecentFiles.Size = new System.Drawing.Size(195, 17);
            this.checkBoxRememberRecentFiles.TabIndex = 28;
            this.checkBoxRememberRecentFiles.Text = "Remember recent files (for reopen)";
            this.checkBoxRememberRecentFiles.UseVisualStyleBackColor = true;
            this.checkBoxRememberRecentFiles.CheckedChanged += new System.EventHandler(this.checkBoxRememberRecentFiles_CheckedChanged);
            // 
            // checkBoxSubtitleFontBold
            // 
            this.checkBoxSubtitleFontBold.AutoSize = true;
            this.checkBoxSubtitleFontBold.Location = new System.Drawing.Point(205, 368);
            this.checkBoxSubtitleFontBold.Name = "checkBoxSubtitleFontBold";
            this.checkBoxSubtitleFontBold.Size = new System.Drawing.Size(46, 17);
            this.checkBoxSubtitleFontBold.TabIndex = 22;
            this.checkBoxSubtitleFontBold.Text = "Bold";
            this.checkBoxSubtitleFontBold.UseVisualStyleBackColor = true;
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
            this.comboBoxSubtitleFontSize.Location = new System.Drawing.Point(205, 341);
            this.comboBoxSubtitleFontSize.Name = "comboBoxSubtitleFontSize";
            this.comboBoxSubtitleFontSize.Size = new System.Drawing.Size(121, 21);
            this.comboBoxSubtitleFontSize.TabIndex = 21;
            // 
            // labelSubtitleFont
            // 
            this.labelSubtitleFont.AutoSize = true;
            this.labelSubtitleFont.Location = new System.Drawing.Point(8, 319);
            this.labelSubtitleFont.Name = "labelSubtitleFont";
            this.labelSubtitleFont.Size = new System.Drawing.Size(66, 13);
            this.labelSubtitleFont.TabIndex = 18;
            this.labelSubtitleFont.Text = "Subtitle font";
            // 
            // labelDefaultFileEncoding
            // 
            this.labelDefaultFileEncoding.AutoSize = true;
            this.labelDefaultFileEncoding.Location = new System.Drawing.Point(8, 56);
            this.labelDefaultFileEncoding.Name = "labelDefaultFileEncoding";
            this.labelDefaultFileEncoding.Size = new System.Drawing.Size(105, 13);
            this.labelDefaultFileEncoding.TabIndex = 2;
            this.labelDefaultFileEncoding.Text = "Default file encoding";
            // 
            // comboBoxFrameRate
            // 
            this.comboBoxFrameRate.FormattingEnabled = true;
            this.comboBoxFrameRate.Location = new System.Drawing.Point(205, 23);
            this.comboBoxFrameRate.Name = "comboBoxFrameRate";
            this.comboBoxFrameRate.Size = new System.Drawing.Size(121, 21);
            this.comboBoxFrameRate.TabIndex = 1;
            // 
            // labelDefaultFrameRate
            // 
            this.labelDefaultFrameRate.AutoSize = true;
            this.labelDefaultFrameRate.Location = new System.Drawing.Point(8, 28);
            this.labelDefaultFrameRate.Name = "labelDefaultFrameRate";
            this.labelDefaultFrameRate.Size = new System.Drawing.Size(96, 13);
            this.labelDefaultFrameRate.TabIndex = 0;
            this.labelDefaultFrameRate.Text = "Default frame rate";
            // 
            // tabPageToolBar
            // 
            this.tabPageToolBar.Controls.Add(this.groupBox2);
            this.tabPageToolBar.Controls.Add(this.groupBoxShowToolBarButtons);
            this.tabPageToolBar.Location = new System.Drawing.Point(4, 22);
            this.tabPageToolBar.Name = "tabPageToolBar";
            this.tabPageToolBar.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageToolBar.Size = new System.Drawing.Size(825, 444);
            this.tabPageToolBar.TabIndex = 7;
            this.tabPageToolBar.Text = "Toolbar ";
            this.tabPageToolBar.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.checkBoxShowFrameRate);
            this.groupBox2.Location = new System.Drawing.Point(7, 248);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(812, 190);
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
            this.groupBoxShowToolBarButtons.Size = new System.Drawing.Size(813, 236);
            this.groupBoxShowToolBarButtons.TabIndex = 0;
            this.groupBoxShowToolBarButtons.TabStop = false;
            this.groupBoxShowToolBarButtons.Text = "Show toolbar buttons";
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
            this.labelTBHelp.Location = new System.Drawing.Point(424, 136);
            this.labelTBHelp.Name = "labelTBHelp";
            this.labelTBHelp.Size = new System.Drawing.Size(28, 13);
            this.labelTBHelp.TabIndex = 33;
            this.labelTBHelp.Text = "Help";
            // 
            // pictureBoxHelp
            // 
            this.pictureBoxHelp.Location = new System.Drawing.Point(423, 155);
            this.pictureBoxHelp.Name = "pictureBoxHelp";
            this.pictureBoxHelp.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxHelp.TabIndex = 32;
            this.pictureBoxHelp.TabStop = false;
            // 
            // checkBoxHelp
            // 
            this.checkBoxHelp.AutoSize = true;
            this.checkBoxHelp.Location = new System.Drawing.Point(426, 195);
            this.checkBoxHelp.Name = "checkBoxHelp";
            this.checkBoxHelp.Size = new System.Drawing.Size(55, 17);
            this.checkBoxHelp.TabIndex = 21;
            this.checkBoxHelp.Text = "Visible";
            this.checkBoxHelp.UseVisualStyleBackColor = true;
            // 
            // labelTBSettings
            // 
            this.labelTBSettings.AutoSize = true;
            this.labelTBSettings.Location = new System.Drawing.Point(319, 136);
            this.labelTBSettings.Name = "labelTBSettings";
            this.labelTBSettings.Size = new System.Drawing.Size(46, 13);
            this.labelTBSettings.TabIndex = 30;
            this.labelTBSettings.Text = "Settings";
            // 
            // pictureBoxSettings
            // 
            this.pictureBoxSettings.Location = new System.Drawing.Point(322, 155);
            this.pictureBoxSettings.Name = "pictureBoxSettings";
            this.pictureBoxSettings.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxSettings.TabIndex = 29;
            this.pictureBoxSettings.TabStop = false;
            // 
            // checkBoxSettings
            // 
            this.checkBoxSettings.AutoSize = true;
            this.checkBoxSettings.Location = new System.Drawing.Point(325, 195);
            this.checkBoxSettings.Name = "checkBoxSettings";
            this.checkBoxSettings.Size = new System.Drawing.Size(55, 17);
            this.checkBoxSettings.TabIndex = 20;
            this.checkBoxSettings.Text = "Visible";
            this.checkBoxSettings.UseVisualStyleBackColor = true;
            // 
            // labelTBSpellCheck
            // 
            this.labelTBSpellCheck.AutoSize = true;
            this.labelTBSpellCheck.Location = new System.Drawing.Point(220, 136);
            this.labelTBSpellCheck.Name = "labelTBSpellCheck";
            this.labelTBSpellCheck.Size = new System.Drawing.Size(59, 13);
            this.labelTBSpellCheck.TabIndex = 27;
            this.labelTBSpellCheck.Text = "Spell check";
            // 
            // pictureBoxSpellCheck
            // 
            this.pictureBoxSpellCheck.Location = new System.Drawing.Point(224, 155);
            this.pictureBoxSpellCheck.Name = "pictureBoxSpellCheck";
            this.pictureBoxSpellCheck.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxSpellCheck.TabIndex = 26;
            this.pictureBoxSpellCheck.TabStop = false;
            // 
            // checkBoxSpellCheck
            // 
            this.checkBoxSpellCheck.AutoSize = true;
            this.checkBoxSpellCheck.Location = new System.Drawing.Point(225, 195);
            this.checkBoxSpellCheck.Name = "checkBoxSpellCheck";
            this.checkBoxSpellCheck.Size = new System.Drawing.Size(55, 17);
            this.checkBoxSpellCheck.TabIndex = 19;
            this.checkBoxSpellCheck.Text = "Visible";
            this.checkBoxSpellCheck.UseVisualStyleBackColor = true;
            // 
            // labelTBVisualSync
            // 
            this.labelTBVisualSync.AutoSize = true;
            this.labelTBVisualSync.Location = new System.Drawing.Point(110, 136);
            this.labelTBVisualSync.Name = "labelTBVisualSync";
            this.labelTBVisualSync.Size = new System.Drawing.Size(59, 13);
            this.labelTBVisualSync.TabIndex = 21;
            this.labelTBVisualSync.Text = "Visual sync";
            // 
            // pictureBoxVisualSync
            // 
            this.pictureBoxVisualSync.Location = new System.Drawing.Point(123, 155);
            this.pictureBoxVisualSync.Name = "pictureBoxVisualSync";
            this.pictureBoxVisualSync.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxVisualSync.TabIndex = 20;
            this.pictureBoxVisualSync.TabStop = false;
            // 
            // checkBoxVisualSync
            // 
            this.checkBoxVisualSync.AutoSize = true;
            this.checkBoxVisualSync.Location = new System.Drawing.Point(126, 195);
            this.checkBoxVisualSync.Name = "checkBoxVisualSync";
            this.checkBoxVisualSync.Size = new System.Drawing.Size(55, 17);
            this.checkBoxVisualSync.TabIndex = 18;
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
            // tabPageVideoPlayer
            // 
            this.tabPageVideoPlayer.Controls.Add(this.groupBoxMainWindowVideoControls);
            this.tabPageVideoPlayer.Controls.Add(this.groupBoxVideoPlayerDefault);
            this.tabPageVideoPlayer.Controls.Add(this.groupBoxVideoEngine);
            this.tabPageVideoPlayer.Location = new System.Drawing.Point(4, 22);
            this.tabPageVideoPlayer.Name = "tabPageVideoPlayer";
            this.tabPageVideoPlayer.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageVideoPlayer.Size = new System.Drawing.Size(825, 444);
            this.tabPageVideoPlayer.TabIndex = 2;
            this.tabPageVideoPlayer.Text = "Video player";
            this.tabPageVideoPlayer.UseVisualStyleBackColor = true;
            // 
            // groupBoxMainWindowVideoControls
            // 
            this.groupBoxMainWindowVideoControls.Controls.Add(this.labelCustomSearch6);
            this.groupBoxMainWindowVideoControls.Controls.Add(this.textBoxCustomSearchUrl6);
            this.groupBoxMainWindowVideoControls.Controls.Add(this.comboBoxCustomSearch6);
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
            this.groupBoxMainWindowVideoControls.Location = new System.Drawing.Point(7, 239);
            this.groupBoxMainWindowVideoControls.Name = "groupBoxMainWindowVideoControls";
            this.groupBoxMainWindowVideoControls.Size = new System.Drawing.Size(813, 199);
            this.groupBoxMainWindowVideoControls.TabIndex = 15;
            this.groupBoxMainWindowVideoControls.TabStop = false;
            this.groupBoxMainWindowVideoControls.Text = "Main window video controls";
            // 
            // labelCustomSearch6
            // 
            this.labelCustomSearch6.AutoSize = true;
            this.labelCustomSearch6.Location = new System.Drawing.Point(12, 175);
            this.labelCustomSearch6.Name = "labelCustomSearch6";
            this.labelCustomSearch6.Size = new System.Drawing.Size(13, 13);
            this.labelCustomSearch6.TabIndex = 18;
            this.labelCustomSearch6.Text = "6";
            // 
            // textBoxCustomSearchUrl6
            // 
            this.textBoxCustomSearchUrl6.Location = new System.Drawing.Point(185, 172);
            this.textBoxCustomSearchUrl6.Name = "textBoxCustomSearchUrl6";
            this.textBoxCustomSearchUrl6.Size = new System.Drawing.Size(574, 21);
            this.textBoxCustomSearchUrl6.TabIndex = 17;
            // 
            // comboBoxCustomSearch6
            // 
            this.comboBoxCustomSearch6.FormattingEnabled = true;
            this.comboBoxCustomSearch6.Items.AddRange(new object[] {
            "Dictionary.com",
            "learnersdictionary.com",
            "Merriam-Webster",
            "The Free Dictionary",
            "Thesaurus.com",
            "urbandictionary.com",
            "VISUWORDS",
            "Wikipedia"});
            this.comboBoxCustomSearch6.Location = new System.Drawing.Point(31, 172);
            this.comboBoxCustomSearch6.Name = "comboBoxCustomSearch6";
            this.comboBoxCustomSearch6.Size = new System.Drawing.Size(148, 21);
            this.comboBoxCustomSearch6.TabIndex = 16;
            this.comboBoxCustomSearch6.SelectedIndexChanged += new System.EventHandler(this.comboBoxCustomSearch_SelectedIndexChanged);
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
            this.groupBoxVideoPlayerDefault.Controls.Add(this.checkBoxVideoPlayerPreviewFontBold);
            this.groupBoxVideoPlayerDefault.Controls.Add(this.checkBoxVideoPlayerShowFullscreenButton);
            this.groupBoxVideoPlayerDefault.Controls.Add(this.checkBoxVideoPlayerShowMuteButton);
            this.groupBoxVideoPlayerDefault.Controls.Add(this.labelVideoPlayerPreviewFontSize);
            this.groupBoxVideoPlayerDefault.Controls.Add(this.comboBoxlVideoPlayerPreviewFontSize);
            this.groupBoxVideoPlayerDefault.Controls.Add(this.checkBoxVideoPlayerShowStopButton);
            this.groupBoxVideoPlayerDefault.Location = new System.Drawing.Point(7, 135);
            this.groupBoxVideoPlayerDefault.Name = "groupBoxVideoPlayerDefault";
            this.groupBoxVideoPlayerDefault.Size = new System.Drawing.Size(813, 98);
            this.groupBoxVideoPlayerDefault.TabIndex = 14;
            this.groupBoxVideoPlayerDefault.TabStop = false;
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
            "20"});
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
            this.groupBoxVideoEngine.Controls.Add(this.labelPlatform);
            this.groupBoxVideoEngine.Controls.Add(this.buttonVlcPathBrowse);
            this.groupBoxVideoEngine.Controls.Add(this.textBoxVlcPath);
            this.groupBoxVideoEngine.Controls.Add(this.labelVlcPath);
            this.groupBoxVideoEngine.Controls.Add(this.labelVideoPlayerVLC);
            this.groupBoxVideoEngine.Controls.Add(this.radioButtonVideoPlayerVLC);
            this.groupBoxVideoEngine.Controls.Add(this.labelVideoPlayerMPlayer);
            this.groupBoxVideoEngine.Controls.Add(this.labelDirectShowDescription);
            this.groupBoxVideoEngine.Controls.Add(this.labelMpcHcDescription);
            this.groupBoxVideoEngine.Controls.Add(this.radioButtonVideoPlayerMPlayer);
            this.groupBoxVideoEngine.Controls.Add(this.radioButtonVideoPlayerDirectShow);
            this.groupBoxVideoEngine.Controls.Add(this.radioButtonVideoPlayerMpcHc);
            this.groupBoxVideoEngine.Location = new System.Drawing.Point(6, 6);
            this.groupBoxVideoEngine.Name = "groupBoxVideoEngine";
            this.groupBoxVideoEngine.Size = new System.Drawing.Size(813, 123);
            this.groupBoxVideoEngine.TabIndex = 0;
            this.groupBoxVideoEngine.TabStop = false;
            this.groupBoxVideoEngine.Text = "Video engine";
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
            this.textBoxVlcPath.Location = new System.Drawing.Point(382, 45);
            this.textBoxVlcPath.MaxLength = 1000;
            this.textBoxVlcPath.Name = "textBoxVlcPath";
            this.textBoxVlcPath.Size = new System.Drawing.Size(390, 21);
            this.textBoxVlcPath.TabIndex = 25;
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
            this.labelVideoPlayerVLC.Font = new System.Drawing.Font("Tahoma", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelVideoPlayerVLC.ForeColor = System.Drawing.Color.Gray;
            this.labelVideoPlayerVLC.Location = new System.Drawing.Point(167, 49);
            this.labelVideoPlayerVLC.Name = "labelVideoPlayerVLC";
            this.labelVideoPlayerVLC.Size = new System.Drawing.Size(208, 11);
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
            this.labelVideoPlayerMPlayer.Font = new System.Drawing.Font("Tahoma", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelVideoPlayerMPlayer.ForeColor = System.Drawing.Color.Gray;
            this.labelVideoPlayerMPlayer.Location = new System.Drawing.Point(167, 93);
            this.labelVideoPlayerMPlayer.Name = "labelVideoPlayerMPlayer";
            this.labelVideoPlayerMPlayer.Size = new System.Drawing.Size(83, 11);
            this.labelVideoPlayerMPlayer.TabIndex = 11;
            this.labelVideoPlayerMPlayer.Text = "MPlayer2/MPlayer";
            // 
            // labelDirectShowDescription
            // 
            this.labelDirectShowDescription.AutoSize = true;
            this.labelDirectShowDescription.Font = new System.Drawing.Font("Tahoma", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDirectShowDescription.ForeColor = System.Drawing.Color.Gray;
            this.labelDirectShowDescription.Location = new System.Drawing.Point(167, 26);
            this.labelDirectShowDescription.Name = "labelDirectShowDescription";
            this.labelDirectShowDescription.Size = new System.Drawing.Size(98, 11);
            this.labelDirectShowDescription.TabIndex = 10;
            this.labelDirectShowDescription.Text = "Quartz.dll in system32";
            // 
            // labelMpcHcDescription
            // 
            this.labelMpcHcDescription.AutoSize = true;
            this.labelMpcHcDescription.Font = new System.Drawing.Font("Tahoma", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelMpcHcDescription.ForeColor = System.Drawing.Color.Gray;
            this.labelMpcHcDescription.Location = new System.Drawing.Point(167, 70);
            this.labelMpcHcDescription.Name = "labelMpcHcDescription";
            this.labelMpcHcDescription.Size = new System.Drawing.Size(160, 11);
            this.labelMpcHcDescription.TabIndex = 9;
            this.labelMpcHcDescription.Text = "Media Player Classic - Home Cinema";
            // 
            // radioButtonVideoPlayerMPlayer
            // 
            this.radioButtonVideoPlayerMPlayer.AutoSize = true;
            this.radioButtonVideoPlayerMPlayer.Location = new System.Drawing.Point(10, 89);
            this.radioButtonVideoPlayerMPlayer.Name = "radioButtonVideoPlayerMPlayer";
            this.radioButtonVideoPlayerMPlayer.Size = new System.Drawing.Size(69, 17);
            this.radioButtonVideoPlayerMPlayer.TabIndex = 28;
            this.radioButtonVideoPlayerMPlayer.TabStop = true;
            this.radioButtonVideoPlayerMPlayer.Text = "MPlayer2";
            this.radioButtonVideoPlayerMPlayer.UseVisualStyleBackColor = true;
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
            this.tabPageWaveform.Size = new System.Drawing.Size(825, 444);
            this.tabPageWaveform.TabIndex = 6;
            this.tabPageWaveform.Text = "Waveform/spectrogram";
            this.tabPageWaveform.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.buttonBrowseToFFmpeg);
            this.groupBox3.Controls.Add(this.textBoxFFmpegPath);
            this.groupBox3.Controls.Add(this.labelFFmpegPath);
            this.groupBox3.Controls.Add(this.checkBoxUseFFmpeg);
            this.groupBox3.Location = new System.Drawing.Point(406, 325);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(416, 116);
            this.groupBox3.TabIndex = 3;
            this.groupBox3.TabStop = false;
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
            this.groupBoxSpectrogram.Controls.Add(this.labelSpectrogramAppearance);
            this.groupBoxSpectrogram.Controls.Add(this.comboBoxSpectrogramAppearance);
            this.groupBoxSpectrogram.Controls.Add(this.checkBoxGenerateSpectrogram);
            this.groupBoxSpectrogram.Location = new System.Drawing.Point(6, 216);
            this.groupBoxSpectrogram.Name = "groupBoxSpectrogram";
            this.groupBoxSpectrogram.Size = new System.Drawing.Size(813, 103);
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
            this.groupBox1.Location = new System.Drawing.Point(6, 325);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(394, 116);
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
            this.groupBoxWaveformAppearence.Size = new System.Drawing.Size(813, 203);
            this.groupBoxWaveformAppearence.TabIndex = 0;
            this.groupBoxWaveformAppearence.TabStop = false;
            this.groupBoxWaveformAppearence.Text = "Waveform appearance";
            // 
            // checkBoxListViewMouseEnterFocus
            // 
            this.checkBoxListViewMouseEnterFocus.AutoSize = true;
            this.checkBoxListViewMouseEnterFocus.Location = new System.Drawing.Point(281, 96);
            this.checkBoxListViewMouseEnterFocus.Name = "checkBoxListViewMouseEnterFocus";
            this.checkBoxListViewMouseEnterFocus.Size = new System.Drawing.Size(214, 17);
            this.checkBoxListViewMouseEnterFocus.TabIndex = 16;
            this.checkBoxListViewMouseEnterFocus.Text = "Focus list view on list view mouse enter";
            this.checkBoxListViewMouseEnterFocus.UseVisualStyleBackColor = true;
            // 
            // checkBoxWaveformHoverFocus
            // 
            this.checkBoxWaveformHoverFocus.AutoSize = true;
            this.checkBoxWaveformHoverFocus.Location = new System.Drawing.Point(262, 78);
            this.checkBoxWaveformHoverFocus.Name = "checkBoxWaveformHoverFocus";
            this.checkBoxWaveformHoverFocus.Size = new System.Drawing.Size(149, 17);
            this.checkBoxWaveformHoverFocus.TabIndex = 15;
            this.checkBoxWaveformHoverFocus.Text = "Set focus on mouse enter";
            this.checkBoxWaveformHoverFocus.UseVisualStyleBackColor = true;
            this.checkBoxWaveformHoverFocus.CheckedChanged += new System.EventHandler(this.checkBoxWaveformHoverFocus_CheckedChanged);
            // 
            // labelWaveformBorderHitMs2
            // 
            this.labelWaveformBorderHitMs2.AutoSize = true;
            this.labelWaveformBorderHitMs2.Location = new System.Drawing.Point(454, 144);
            this.labelWaveformBorderHitMs2.Name = "labelWaveformBorderHitMs2";
            this.labelWaveformBorderHitMs2.Size = new System.Drawing.Size(62, 13);
            this.labelWaveformBorderHitMs2.TabIndex = 14;
            this.labelWaveformBorderHitMs2.Text = "milliseconds";
            // 
            // numericUpDownWaveformBorderHitMs
            // 
            this.numericUpDownWaveformBorderHitMs.Location = new System.Drawing.Point(392, 142);
            this.numericUpDownWaveformBorderHitMs.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDownWaveformBorderHitMs.Name = "numericUpDownWaveformBorderHitMs";
            this.numericUpDownWaveformBorderHitMs.Size = new System.Drawing.Size(44, 21);
            this.numericUpDownWaveformBorderHitMs.TabIndex = 12;
            this.numericUpDownWaveformBorderHitMs.Value = new decimal(new int[] {
            18,
            0,
            0,
            0});
            // 
            // labelWaveformBorderHitMs1
            // 
            this.labelWaveformBorderHitMs1.AutoSize = true;
            this.labelWaveformBorderHitMs1.Location = new System.Drawing.Point(259, 144);
            this.labelWaveformBorderHitMs1.Name = "labelWaveformBorderHitMs1";
            this.labelWaveformBorderHitMs1.Size = new System.Drawing.Size(127, 13);
            this.labelWaveformBorderHitMs1.TabIndex = 13;
            this.labelWaveformBorderHitMs1.Text = "Marker hit must be within";
            // 
            // checkBoxAllowOverlap
            // 
            this.checkBoxAllowOverlap.AutoSize = true;
            this.checkBoxAllowOverlap.Location = new System.Drawing.Point(262, 55);
            this.checkBoxAllowOverlap.Name = "checkBoxAllowOverlap";
            this.checkBoxAllowOverlap.Size = new System.Drawing.Size(212, 17);
            this.checkBoxAllowOverlap.TabIndex = 10;
            this.checkBoxAllowOverlap.Text = "Allow overlap (when dragging/resizing)";
            this.checkBoxAllowOverlap.UseVisualStyleBackColor = true;
            // 
            // checkBoxReverseMouseWheelScrollDirection
            // 
            this.checkBoxReverseMouseWheelScrollDirection.AutoSize = true;
            this.checkBoxReverseMouseWheelScrollDirection.Location = new System.Drawing.Point(262, 31);
            this.checkBoxReverseMouseWheelScrollDirection.Name = "checkBoxReverseMouseWheelScrollDirection";
            this.checkBoxReverseMouseWheelScrollDirection.Size = new System.Drawing.Size(202, 17);
            this.checkBoxReverseMouseWheelScrollDirection.TabIndex = 6;
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
            this.tabPageTools.Controls.Add(this.groupBoxToolsMisc);
            this.tabPageTools.Controls.Add(this.groupBoxSpellCheck);
            this.tabPageTools.Controls.Add(this.groupBoxFixCommonErrors);
            this.tabPageTools.Controls.Add(this.groupBoxToolsVisualSync);
            this.tabPageTools.Location = new System.Drawing.Point(4, 22);
            this.tabPageTools.Name = "tabPageTools";
            this.tabPageTools.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageTools.Size = new System.Drawing.Size(825, 444);
            this.tabPageTools.TabIndex = 5;
            this.tabPageTools.Text = "Tools";
            this.tabPageTools.UseVisualStyleBackColor = true;
            // 
            // groupBoxToolsMisc
            // 
            this.groupBoxToolsMisc.Controls.Add(this.buttonEditDoNotBreakAfterList);
            this.groupBoxToolsMisc.Controls.Add(this.checkBoxUseDoNotBreakAfterList);
            this.groupBoxToolsMisc.Location = new System.Drawing.Point(374, 129);
            this.groupBoxToolsMisc.Name = "groupBoxToolsMisc";
            this.groupBoxToolsMisc.Size = new System.Drawing.Size(445, 309);
            this.groupBoxToolsMisc.TabIndex = 5;
            this.groupBoxToolsMisc.TabStop = false;
            this.groupBoxToolsMisc.Text = "Misc";
            // 
            // buttonEditDoNotBreakAfterList
            // 
            this.buttonEditDoNotBreakAfterList.Location = new System.Drawing.Point(34, 43);
            this.buttonEditDoNotBreakAfterList.Name = "buttonEditDoNotBreakAfterList";
            this.buttonEditDoNotBreakAfterList.Size = new System.Drawing.Size(75, 23);
            this.buttonEditDoNotBreakAfterList.TabIndex = 23;
            this.buttonEditDoNotBreakAfterList.Text = "Edit";
            this.buttonEditDoNotBreakAfterList.UseVisualStyleBackColor = true;
            this.buttonEditDoNotBreakAfterList.Click += new System.EventHandler(this.buttonEditDoNotBreakAfterList_Click);
            // 
            // checkBoxUseDoNotBreakAfterList
            // 
            this.checkBoxUseDoNotBreakAfterList.AutoSize = true;
            this.checkBoxUseDoNotBreakAfterList.Location = new System.Drawing.Point(15, 20);
            this.checkBoxUseDoNotBreakAfterList.Name = "checkBoxUseDoNotBreakAfterList";
            this.checkBoxUseDoNotBreakAfterList.Size = new System.Drawing.Size(218, 17);
            this.checkBoxUseDoNotBreakAfterList.TabIndex = 0;
            this.checkBoxUseDoNotBreakAfterList.Text = "Use \'do-not-beak-after\' list (for auto-br)";
            this.checkBoxUseDoNotBreakAfterList.UseVisualStyleBackColor = true;
            // 
            // groupBoxSpellCheck
            // 
            this.groupBoxSpellCheck.Controls.Add(this.checkBoxTreatINQuoteAsING);
            this.groupBoxSpellCheck.Controls.Add(this.checkBoxSpellCheckOneLetterWords);
            this.groupBoxSpellCheck.Controls.Add(this.checkBoxSpellCheckAutoChangeNames);
            this.groupBoxSpellCheck.Location = new System.Drawing.Point(6, 314);
            this.groupBoxSpellCheck.Name = "groupBoxSpellCheck";
            this.groupBoxSpellCheck.Size = new System.Drawing.Size(362, 124);
            this.groupBoxSpellCheck.TabIndex = 4;
            this.groupBoxSpellCheck.TabStop = false;
            this.groupBoxSpellCheck.Text = "Spell check";
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
            this.groupBoxFixCommonErrors.Controls.Add(this.checkBoxFixShortDisplayTimesAllowMoveStartTime);
            this.groupBoxFixCommonErrors.Controls.Add(this.checkBoxFixCommonOcrErrorsUsingHardcodedRules);
            this.groupBoxFixCommonErrors.Controls.Add(this.comboBoxToolsMusicSymbol);
            this.groupBoxFixCommonErrors.Controls.Add(this.textBoxMusicSymbolsToReplace);
            this.groupBoxFixCommonErrors.Controls.Add(this.labelToolsMusicSymbolsToReplace);
            this.groupBoxFixCommonErrors.Controls.Add(this.labelToolsMusicSymbol);
            this.groupBoxFixCommonErrors.Location = new System.Drawing.Point(7, 129);
            this.groupBoxFixCommonErrors.Name = "groupBoxFixCommonErrors";
            this.groupBoxFixCommonErrors.Size = new System.Drawing.Size(361, 179);
            this.groupBoxFixCommonErrors.TabIndex = 3;
            this.groupBoxFixCommonErrors.TabStop = false;
            this.groupBoxFixCommonErrors.Text = "Fix common errors";
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
            this.labelToolsMusicSymbolsToReplace.Size = new System.Drawing.Size(225, 13);
            this.labelToolsMusicSymbolsToReplace.TabIndex = 34;
            this.labelToolsMusicSymbolsToReplace.Text = "Music symbols to replace (separate by space)";
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
            this.groupBoxToolsVisualSync.Controls.Add(this.labelToolsEndScene);
            this.groupBoxToolsVisualSync.Controls.Add(this.comboBoxToolsEndSceneIndex);
            this.groupBoxToolsVisualSync.Controls.Add(this.labelToolsStartScene);
            this.groupBoxToolsVisualSync.Controls.Add(this.comboBoxToolsStartSceneIndex);
            this.groupBoxToolsVisualSync.Controls.Add(this.comboBoxToolsVerifySeconds);
            this.groupBoxToolsVisualSync.Controls.Add(this.labelVerifyButton);
            this.groupBoxToolsVisualSync.Location = new System.Drawing.Point(6, 6);
            this.groupBoxToolsVisualSync.Name = "groupBoxToolsVisualSync";
            this.groupBoxToolsVisualSync.Size = new System.Drawing.Size(813, 116);
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
            this.tabPageWordLists.Size = new System.Drawing.Size(825, 444);
            this.tabPageWordLists.TabIndex = 3;
            this.tabPageWordLists.Text = "Word lists";
            this.tabPageWordLists.UseVisualStyleBackColor = true;
            // 
            // groupBoxWordLists
            // 
            this.groupBoxWordLists.Controls.Add(this.groupBoxOcrFixList);
            this.groupBoxWordLists.Controls.Add(this.groupBoxUserWordList);
            this.groupBoxWordLists.Controls.Add(this.groupBoxWordListLocation);
            this.groupBoxWordLists.Controls.Add(this.groupBoxNamesIgonoreLists);
            this.groupBoxWordLists.Controls.Add(this.labelWordListLanguage);
            this.groupBoxWordLists.Controls.Add(this.comboBoxWordListLanguage);
            this.groupBoxWordLists.Location = new System.Drawing.Point(6, 6);
            this.groupBoxWordLists.Name = "groupBoxWordLists";
            this.groupBoxWordLists.Size = new System.Drawing.Size(809, 432);
            this.groupBoxWordLists.TabIndex = 2;
            this.groupBoxWordLists.TabStop = false;
            this.groupBoxWordLists.Text = "Word lists";
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
            this.listBoxOcrFixList.Size = new System.Drawing.Size(179, 199);
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
            this.listBoxUserWordLists.Size = new System.Drawing.Size(150, 199);
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
            this.groupBoxWordListLocation.Controls.Add(this.checkBoxNamesEtcOnline);
            this.groupBoxWordListLocation.Controls.Add(this.textBoxNamesEtcOnline);
            this.groupBoxWordListLocation.Location = new System.Drawing.Point(6, 316);
            this.groupBoxWordListLocation.Name = "groupBoxWordListLocation";
            this.groupBoxWordListLocation.Size = new System.Drawing.Size(797, 110);
            this.groupBoxWordListLocation.TabIndex = 8;
            this.groupBoxWordListLocation.TabStop = false;
            this.groupBoxWordListLocation.Text = "Location";
            // 
            // checkBoxNamesEtcOnline
            // 
            this.checkBoxNamesEtcOnline.AutoSize = true;
            this.checkBoxNamesEtcOnline.Location = new System.Drawing.Point(7, 22);
            this.checkBoxNamesEtcOnline.Name = "checkBoxNamesEtcOnline";
            this.checkBoxNamesEtcOnline.Size = new System.Drawing.Size(163, 17);
            this.checkBoxNamesEtcOnline.TabIndex = 26;
            this.checkBoxNamesEtcOnline.Text = "Use online names etc xml file";
            this.checkBoxNamesEtcOnline.UseVisualStyleBackColor = true;
            // 
            // textBoxNamesEtcOnline
            // 
            this.textBoxNamesEtcOnline.Location = new System.Drawing.Point(6, 45);
            this.textBoxNamesEtcOnline.Name = "textBoxNamesEtcOnline";
            this.textBoxNamesEtcOnline.Size = new System.Drawing.Size(235, 21);
            this.textBoxNamesEtcOnline.TabIndex = 28;
            this.textBoxNamesEtcOnline.Text = "https://raw.githubusercontent.com/SubtitleEdit/subtitleedit/master/Dictionaries/n" +
    "ames_etc.xml";
            // 
            // groupBoxNamesIgonoreLists
            // 
            this.groupBoxNamesIgonoreLists.Controls.Add(this.buttonRemoveNameEtc);
            this.groupBoxNamesIgonoreLists.Controls.Add(this.listBoxNamesEtc);
            this.groupBoxNamesIgonoreLists.Controls.Add(this.textBoxNameEtc);
            this.groupBoxNamesIgonoreLists.Controls.Add(this.buttonAddNamesEtc);
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
            // listBoxNamesEtc
            // 
            this.listBoxNamesEtc.FormattingEnabled = true;
            this.listBoxNamesEtc.Location = new System.Drawing.Point(3, 16);
            this.listBoxNamesEtc.Name = "listBoxNamesEtc";
            this.listBoxNamesEtc.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listBoxNamesEtc.Size = new System.Drawing.Size(150, 199);
            this.listBoxNamesEtc.TabIndex = 20;
            this.listBoxNamesEtc.SelectedIndexChanged += new System.EventHandler(this.ListBoxNamesEtcSelectedIndexChanged);
            this.listBoxNamesEtc.Enter += new System.EventHandler(this.ListBoxSearchReset);
            this.listBoxNamesEtc.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ListBoxKeyDownSearch);
            // 
            // textBoxNameEtc
            // 
            this.textBoxNameEtc.Location = new System.Drawing.Point(3, 241);
            this.textBoxNameEtc.Name = "textBoxNameEtc";
            this.textBoxNameEtc.Size = new System.Drawing.Size(151, 21);
            this.textBoxNameEtc.TabIndex = 24;
            this.textBoxNameEtc.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBoxNameEtcKeyDown);
            // 
            // buttonAddNamesEtc
            // 
            this.buttonAddNamesEtc.Location = new System.Drawing.Point(160, 238);
            this.buttonAddNamesEtc.Name = "buttonAddNamesEtc";
            this.buttonAddNamesEtc.Size = new System.Drawing.Size(75, 23);
            this.buttonAddNamesEtc.TabIndex = 26;
            this.buttonAddNamesEtc.Text = "Add name";
            this.buttonAddNamesEtc.UseVisualStyleBackColor = true;
            this.buttonAddNamesEtc.Click += new System.EventHandler(this.ButtonAddNamesEtcClick);
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
            // tabPageSsaStyle
            // 
            this.tabPageSsaStyle.Controls.Add(this.groupBoxSsaStyle);
            this.tabPageSsaStyle.Location = new System.Drawing.Point(4, 22);
            this.tabPageSsaStyle.Name = "tabPageSsaStyle";
            this.tabPageSsaStyle.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageSsaStyle.Size = new System.Drawing.Size(825, 444);
            this.tabPageSsaStyle.TabIndex = 1;
            this.tabPageSsaStyle.Text = "SSA style";
            this.tabPageSsaStyle.UseVisualStyleBackColor = true;
            // 
            // groupBoxSsaStyle
            // 
            this.groupBoxSsaStyle.Controls.Add(this.checkBoxSsaOpaqueBox);
            this.groupBoxSsaStyle.Controls.Add(this.groupBoxPreview);
            this.groupBoxSsaStyle.Controls.Add(this.labelSsaOutline);
            this.groupBoxSsaStyle.Controls.Add(this.numericUpDownSsaShadow);
            this.groupBoxSsaStyle.Controls.Add(this.numericUpDownSsaOutline);
            this.groupBoxSsaStyle.Controls.Add(this.labelSsaShadow);
            this.groupBoxSsaStyle.Controls.Add(this.labelSSAFont);
            this.groupBoxSsaStyle.Controls.Add(this.buttonSSAChooseColor);
            this.groupBoxSsaStyle.Controls.Add(this.buttonSSAChooseFont);
            this.groupBoxSsaStyle.Location = new System.Drawing.Point(6, 6);
            this.groupBoxSsaStyle.Name = "groupBoxSsaStyle";
            this.groupBoxSsaStyle.Size = new System.Drawing.Size(813, 432);
            this.groupBoxSsaStyle.TabIndex = 0;
            this.groupBoxSsaStyle.TabStop = false;
            this.groupBoxSsaStyle.Text = "Sub Station Alpha style";
            // 
            // checkBoxSsaOpaqueBox
            // 
            this.checkBoxSsaOpaqueBox.AutoSize = true;
            this.checkBoxSsaOpaqueBox.Location = new System.Drawing.Point(275, 91);
            this.checkBoxSsaOpaqueBox.Name = "checkBoxSsaOpaqueBox";
            this.checkBoxSsaOpaqueBox.Size = new System.Drawing.Size(85, 17);
            this.checkBoxSsaOpaqueBox.TabIndex = 0;
            this.checkBoxSsaOpaqueBox.Text = "Opaque box";
            this.checkBoxSsaOpaqueBox.UseVisualStyleBackColor = true;
            this.checkBoxSsaOpaqueBox.CheckedChanged += new System.EventHandler(this.checkBoxSsaOpaqueBox_CheckedChanged);
            // 
            // groupBoxPreview
            // 
            this.groupBoxPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxPreview.Controls.Add(this.pictureBoxPreview);
            this.groupBoxPreview.Location = new System.Drawing.Point(17, 114);
            this.groupBoxPreview.Name = "groupBoxPreview";
            this.groupBoxPreview.Size = new System.Drawing.Size(790, 312);
            this.groupBoxPreview.TabIndex = 10;
            this.groupBoxPreview.TabStop = false;
            this.groupBoxPreview.Text = "Preview";
            // 
            // pictureBoxPreview
            // 
            this.pictureBoxPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBoxPreview.Location = new System.Drawing.Point(3, 17);
            this.pictureBoxPreview.Name = "pictureBoxPreview";
            this.pictureBoxPreview.Size = new System.Drawing.Size(784, 292);
            this.pictureBoxPreview.TabIndex = 0;
            this.pictureBoxPreview.TabStop = false;
            // 
            // labelSsaOutline
            // 
            this.labelSsaOutline.AutoSize = true;
            this.labelSsaOutline.Location = new System.Drawing.Point(272, 40);
            this.labelSsaOutline.Name = "labelSsaOutline";
            this.labelSsaOutline.Size = new System.Drawing.Size(41, 13);
            this.labelSsaOutline.TabIndex = 9;
            this.labelSsaOutline.Text = "Outline";
            // 
            // numericUpDownSsaShadow
            // 
            this.numericUpDownSsaShadow.Location = new System.Drawing.Point(319, 63);
            this.numericUpDownSsaShadow.Maximum = new decimal(new int[] {
            9,
            0,
            0,
            0});
            this.numericUpDownSsaShadow.Name = "numericUpDownSsaShadow";
            this.numericUpDownSsaShadow.Size = new System.Drawing.Size(44, 21);
            this.numericUpDownSsaShadow.TabIndex = 8;
            this.numericUpDownSsaShadow.ValueChanged += new System.EventHandler(this.numericUpDownSsaShadow_ValueChanged);
            // 
            // numericUpDownSsaOutline
            // 
            this.numericUpDownSsaOutline.Location = new System.Drawing.Point(319, 36);
            this.numericUpDownSsaOutline.Maximum = new decimal(new int[] {
            9,
            0,
            0,
            0});
            this.numericUpDownSsaOutline.Name = "numericUpDownSsaOutline";
            this.numericUpDownSsaOutline.Size = new System.Drawing.Size(44, 21);
            this.numericUpDownSsaOutline.TabIndex = 6;
            this.numericUpDownSsaOutline.ValueChanged += new System.EventHandler(this.numericUpDownSsaOutline_ValueChanged);
            // 
            // labelSsaShadow
            // 
            this.labelSsaShadow.AutoSize = true;
            this.labelSsaShadow.Location = new System.Drawing.Point(272, 65);
            this.labelSsaShadow.Name = "labelSsaShadow";
            this.labelSsaShadow.Size = new System.Drawing.Size(45, 13);
            this.labelSsaShadow.TabIndex = 7;
            this.labelSsaShadow.Text = "Shadow";
            // 
            // labelSSAFont
            // 
            this.labelSSAFont.AutoSize = true;
            this.labelSSAFont.Location = new System.Drawing.Point(146, 38);
            this.labelSSAFont.Name = "labelSSAFont";
            this.labelSSAFont.Size = new System.Drawing.Size(41, 13);
            this.labelSSAFont.TabIndex = 3;
            this.labelSSAFont.Text = "label16";
            // 
            // buttonSSAChooseColor
            // 
            this.buttonSSAChooseColor.Location = new System.Drawing.Point(26, 73);
            this.buttonSSAChooseColor.Name = "buttonSSAChooseColor";
            this.buttonSSAChooseColor.Size = new System.Drawing.Size(114, 21);
            this.buttonSSAChooseColor.TabIndex = 1;
            this.buttonSSAChooseColor.Text = "Choose font color";
            this.buttonSSAChooseColor.UseVisualStyleBackColor = true;
            this.buttonSSAChooseColor.Click += new System.EventHandler(this.ButtonSsaChooseColorClick);
            // 
            // buttonSSAChooseFont
            // 
            this.buttonSSAChooseFont.Location = new System.Drawing.Point(26, 34);
            this.buttonSSAChooseFont.Name = "buttonSSAChooseFont";
            this.buttonSSAChooseFont.Size = new System.Drawing.Size(114, 21);
            this.buttonSSAChooseFont.TabIndex = 0;
            this.buttonSSAChooseFont.Text = "Choose font";
            this.buttonSSAChooseFont.UseVisualStyleBackColor = true;
            this.buttonSSAChooseFont.Click += new System.EventHandler(this.ButtonSsaChooseFontClick);
            // 
            // tabPageProxy
            // 
            this.tabPageProxy.Controls.Add(this.groupBoxProxySettings);
            this.tabPageProxy.Location = new System.Drawing.Point(4, 22);
            this.tabPageProxy.Name = "tabPageProxy";
            this.tabPageProxy.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageProxy.Size = new System.Drawing.Size(825, 444);
            this.tabPageProxy.TabIndex = 4;
            this.tabPageProxy.Text = "Proxy";
            this.tabPageProxy.UseVisualStyleBackColor = true;
            // 
            // groupBoxProxySettings
            // 
            this.groupBoxProxySettings.Controls.Add(this.groupBoxProxyAuthentication);
            this.groupBoxProxySettings.Controls.Add(this.textBoxProxyAddress);
            this.groupBoxProxySettings.Controls.Add(this.labelProxyAddress);
            this.groupBoxProxySettings.Location = new System.Drawing.Point(6, 6);
            this.groupBoxProxySettings.Name = "groupBoxProxySettings";
            this.groupBoxProxySettings.Size = new System.Drawing.Size(813, 432);
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
            // tabPageShortcuts
            // 
            this.tabPageShortcuts.Controls.Add(this.groupBoxShortcuts);
            this.tabPageShortcuts.Location = new System.Drawing.Point(4, 22);
            this.tabPageShortcuts.Name = "tabPageShortcuts";
            this.tabPageShortcuts.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageShortcuts.Size = new System.Drawing.Size(825, 444);
            this.tabPageShortcuts.TabIndex = 8;
            this.tabPageShortcuts.Text = "Shortcuts";
            this.tabPageShortcuts.UseVisualStyleBackColor = true;
            // 
            // groupBoxShortcuts
            // 
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
            this.groupBoxShortcuts.Size = new System.Drawing.Size(813, 432);
            this.groupBoxShortcuts.TabIndex = 2;
            this.groupBoxShortcuts.TabStop = false;
            this.groupBoxShortcuts.Text = "Shortcuts";
            // 
            // comboBoxShortcutKey
            // 
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
            this.comboBoxShortcutKey.Location = new System.Drawing.Point(353, 405);
            this.comboBoxShortcutKey.Name = "comboBoxShortcutKey";
            this.comboBoxShortcutKey.Size = new System.Drawing.Size(92, 21);
            this.comboBoxShortcutKey.TabIndex = 4;
            this.comboBoxShortcutKey.KeyDown += new System.Windows.Forms.KeyEventHandler(this.comboBoxShortcutKey_KeyDown);
            // 
            // labelShortcutKey
            // 
            this.labelShortcutKey.AutoSize = true;
            this.labelShortcutKey.Location = new System.Drawing.Point(322, 407);
            this.labelShortcutKey.Name = "labelShortcutKey";
            this.labelShortcutKey.Size = new System.Drawing.Size(25, 13);
            this.labelShortcutKey.TabIndex = 35;
            this.labelShortcutKey.Text = "Key";
            // 
            // checkBoxShortcutsShift
            // 
            this.checkBoxShortcutsShift.AutoSize = true;
            this.checkBoxShortcutsShift.Enabled = false;
            this.checkBoxShortcutsShift.Location = new System.Drawing.Point(245, 407);
            this.checkBoxShortcutsShift.Name = "checkBoxShortcutsShift";
            this.checkBoxShortcutsShift.Size = new System.Drawing.Size(48, 17);
            this.checkBoxShortcutsShift.TabIndex = 3;
            this.checkBoxShortcutsShift.Text = "Shift";
            this.checkBoxShortcutsShift.UseVisualStyleBackColor = true;
            // 
            // checkBoxShortcutsAlt
            // 
            this.checkBoxShortcutsAlt.AutoSize = true;
            this.checkBoxShortcutsAlt.Enabled = false;
            this.checkBoxShortcutsAlt.Location = new System.Drawing.Point(176, 407);
            this.checkBoxShortcutsAlt.Name = "checkBoxShortcutsAlt";
            this.checkBoxShortcutsAlt.Size = new System.Drawing.Size(39, 17);
            this.checkBoxShortcutsAlt.TabIndex = 2;
            this.checkBoxShortcutsAlt.Text = "Alt";
            this.checkBoxShortcutsAlt.UseVisualStyleBackColor = true;
            // 
            // checkBoxShortcutsControl
            // 
            this.checkBoxShortcutsControl.AutoSize = true;
            this.checkBoxShortcutsControl.Enabled = false;
            this.checkBoxShortcutsControl.Location = new System.Drawing.Point(89, 407);
            this.checkBoxShortcutsControl.Name = "checkBoxShortcutsControl";
            this.checkBoxShortcutsControl.Size = new System.Drawing.Size(61, 17);
            this.checkBoxShortcutsControl.TabIndex = 1;
            this.checkBoxShortcutsControl.Text = "Control";
            this.checkBoxShortcutsControl.UseVisualStyleBackColor = true;
            // 
            // buttonUpdateShortcut
            // 
            this.buttonUpdateShortcut.Enabled = false;
            this.buttonUpdateShortcut.Location = new System.Drawing.Point(485, 404);
            this.buttonUpdateShortcut.Name = "buttonUpdateShortcut";
            this.buttonUpdateShortcut.Size = new System.Drawing.Size(111, 23);
            this.buttonUpdateShortcut.TabIndex = 5;
            this.buttonUpdateShortcut.Text = "&Update";
            this.buttonUpdateShortcut.UseVisualStyleBackColor = true;
            this.buttonUpdateShortcut.Click += new System.EventHandler(this.buttonUpdateShortcut_Click);
            // 
            // treeViewShortcuts
            // 
            this.treeViewShortcuts.HideSelection = false;
            this.treeViewShortcuts.Location = new System.Drawing.Point(16, 21);
            this.treeViewShortcuts.Name = "treeViewShortcuts";
            this.treeViewShortcuts.Size = new System.Drawing.Size(787, 376);
            this.treeViewShortcuts.TabIndex = 0;
            this.treeViewShortcuts.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewShortcuts_AfterSelect);
            // 
            // labelShortcut
            // 
            this.labelShortcut.AutoSize = true;
            this.labelShortcut.Location = new System.Drawing.Point(15, 407);
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
            this.tabPageSyntaxColoring.Size = new System.Drawing.Size(825, 444);
            this.tabPageSyntaxColoring.TabIndex = 9;
            this.tabPageSyntaxColoring.Text = "Syntax coloring";
            this.tabPageSyntaxColoring.UseVisualStyleBackColor = true;
            // 
            // groupBoxListViewSyntaxColoring
            // 
            this.groupBoxListViewSyntaxColoring.Controls.Add(this.numericUpDownSyntaxColorTextMoreThanXLines);
            this.groupBoxListViewSyntaxColoring.Controls.Add(this.checkBoxSyntaxColorTextMoreThanTwoLines);
            this.groupBoxListViewSyntaxColoring.Controls.Add(this.checkBoxSyntaxOverlap);
            this.groupBoxListViewSyntaxColoring.Controls.Add(this.checkBoxSyntaxColorDurationTooSmall);
            this.groupBoxListViewSyntaxColoring.Controls.Add(this.buttonListViewSyntaxColorError);
            this.groupBoxListViewSyntaxColoring.Controls.Add(this.checkBoxSyntaxColorTextTooLong);
            this.groupBoxListViewSyntaxColoring.Controls.Add(this.checkBoxSyntaxColorDurationTooLarge);
            this.groupBoxListViewSyntaxColoring.Controls.Add(this.panelListViewSyntaxColorError);
            this.groupBoxListViewSyntaxColoring.Location = new System.Drawing.Point(6, 6);
            this.groupBoxListViewSyntaxColoring.Name = "groupBoxListViewSyntaxColoring";
            this.groupBoxListViewSyntaxColoring.Size = new System.Drawing.Size(813, 432);
            this.groupBoxListViewSyntaxColoring.TabIndex = 0;
            this.groupBoxListViewSyntaxColoring.TabStop = false;
            this.groupBoxListViewSyntaxColoring.Text = "List view syntax coloring";
            // 
            // numericUpDownSyntaxColorTextMoreThanXLines
            // 
            this.numericUpDownSyntaxColorTextMoreThanXLines.Location = new System.Drawing.Point(196, 112);
            this.numericUpDownSyntaxColorTextMoreThanXLines.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDownSyntaxColorTextMoreThanXLines.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownSyntaxColorTextMoreThanXLines.Name = "numericUpDownSyntaxColorTextMoreThanXLines";
            this.numericUpDownSyntaxColorTextMoreThanXLines.Size = new System.Drawing.Size(48, 21);
            this.numericUpDownSyntaxColorTextMoreThanXLines.TabIndex = 4;
            this.numericUpDownSyntaxColorTextMoreThanXLines.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
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
            this.checkBoxSyntaxOverlap.Location = new System.Drawing.Point(20, 154);
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
            this.buttonListViewSyntaxColorError.Location = new System.Drawing.Point(20, 192);
            this.buttonListViewSyntaxColorError.Name = "buttonListViewSyntaxColorError";
            this.buttonListViewSyntaxColorError.Size = new System.Drawing.Size(112, 21);
            this.buttonListViewSyntaxColorError.TabIndex = 6;
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
            this.panelListViewSyntaxColorError.Location = new System.Drawing.Point(142, 192);
            this.panelListViewSyntaxColorError.Name = "panelListViewSyntaxColorError";
            this.panelListViewSyntaxColorError.Size = new System.Drawing.Size(21, 20);
            this.panelListViewSyntaxColorError.TabIndex = 7;
            this.panelListViewSyntaxColorError.Click += new System.EventHandler(this.buttonListViewSyntaxColorError_Click);
            // 
            // fontDialogSSAStyle
            // 
            this.fontDialogSSAStyle.Color = System.Drawing.SystemColors.ControlText;
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(14, 493);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(60, 13);
            this.labelStatus.TabIndex = 3;
            this.labelStatus.Text = "labelStatus";
            // 
            // openFileDialogFFmpeg
            // 
            this.openFileDialogFFmpeg.FileName = "openFileDialog1";
            // 
            // Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(858, 522);
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
            this.tabPageGenerel.ResumeLayout(false);
            this.groupBoxMiscellaneous.ResumeLayout(false);
            this.groupBoxMiscellaneous.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMinGapMs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSubtitleLineMaximumLength)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxCharsSec)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDurationMax)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDurationMin)).EndInit();
            this.tabPageToolBar.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBoxShowToolBarButtons.ResumeLayout(false);
            this.groupBoxShowToolBarButtons.PerformLayout();
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
            this.groupBoxToolsMisc.ResumeLayout(false);
            this.groupBoxToolsMisc.PerformLayout();
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
            this.tabPageSsaStyle.ResumeLayout(false);
            this.groupBoxSsaStyle.ResumeLayout(false);
            this.groupBoxSsaStyle.PerformLayout();
            this.groupBoxPreview.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreview)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSsaShadow)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSsaOutline)).EndInit();
            this.tabPageProxy.ResumeLayout(false);
            this.groupBoxProxySettings.ResumeLayout(false);
            this.groupBoxProxySettings.PerformLayout();
            this.groupBoxProxyAuthentication.ResumeLayout(false);
            this.groupBoxProxyAuthentication.PerformLayout();
            this.tabPageShortcuts.ResumeLayout(false);
            this.groupBoxShortcuts.ResumeLayout(false);
            this.groupBoxShortcuts.PerformLayout();
            this.tabPageSyntaxColoring.ResumeLayout(false);
            this.groupBoxListViewSyntaxColoring.ResumeLayout(false);
            this.groupBoxListViewSyntaxColoring.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSyntaxColorTextMoreThanXLines)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.TabControl tabControlSettings;
        private System.Windows.Forms.TabPage tabPageGenerel;
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
        private System.Windows.Forms.CheckBox checkBoxSubtitleFontBold;
        private System.Windows.Forms.ComboBox comboBoxSubtitleFontSize;
        private System.Windows.Forms.Label labelSubtitleFont;
        private System.Windows.Forms.ComboBox comboBoxEncoding;
        private System.Windows.Forms.CheckBox checkBoxRememberRecentFiles;
        private System.Windows.Forms.GroupBox groupBoxSsaStyle;
        private System.Windows.Forms.Label labelSSAFont;
        private System.Windows.Forms.Button buttonSSAChooseColor;
        private System.Windows.Forms.Button buttonSSAChooseFont;
        private System.Windows.Forms.ColorDialog colorDialogSSAStyle;
        private System.Windows.Forms.FontDialog fontDialogSSAStyle;
        private System.Windows.Forms.CheckBox checkBoxStartInSourceView;
        private System.Windows.Forms.CheckBox checkBoxReopenLastOpened;
        private System.Windows.Forms.Label labelSubtitleFontSize;
        private System.Windows.Forms.ComboBox comboBoxSubtitleFont;
        private System.Windows.Forms.Label labelSubMaxLen;
        private System.Windows.Forms.TabPage tabPageVideoPlayer;
        private System.Windows.Forms.CheckBox checkBoxVideoPlayerShowStopButton;
        private System.Windows.Forms.GroupBox groupBoxVideoEngine;
        private System.Windows.Forms.Label labelVideoPlayerMPlayer;
        private System.Windows.Forms.Label labelDirectShowDescription;
        private System.Windows.Forms.Label labelMpcHcDescription;
        private System.Windows.Forms.RadioButton radioButtonVideoPlayerMPlayer;
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
        private System.Windows.Forms.Button buttonAddNamesEtc;
        private System.Windows.Forms.ComboBox comboBoxWordListLanguage;
        private System.Windows.Forms.Button buttonRemoveNameEtc;
        private System.Windows.Forms.ListBox listBoxNamesEtc;
        private System.Windows.Forms.Button buttonRemoveOcrFix;
        private System.Windows.Forms.ListBox listBoxOcrFixList;
        private System.Windows.Forms.TextBox textBoxOcrFixKey;
        private System.Windows.Forms.Button buttonAddOcrFix;
        private System.Windows.Forms.GroupBox groupBoxUserWordList;
        private System.Windows.Forms.Button buttonRemoveUserWord;
        private System.Windows.Forms.ListBox listBoxUserWordLists;
        private System.Windows.Forms.TextBox textBoxUserWord;
        private System.Windows.Forms.Button buttonAddUserWord;
        private System.Windows.Forms.TabPage tabPageProxy;
        private System.Windows.Forms.GroupBox groupBoxProxySettings;
        private System.Windows.Forms.Label labelProxyPassword;
        private System.Windows.Forms.TextBox textBoxProxyAddress;
        private System.Windows.Forms.TextBox textBoxProxyUserName;
        private System.Windows.Forms.TextBox textBoxProxyPassword;
        private System.Windows.Forms.Label labelProxyAddress;
        private System.Windows.Forms.Label labelProxyUserName;
        private System.Windows.Forms.CheckBox checkBoxNamesEtcOnline;
        private System.Windows.Forms.TextBox textBoxNamesEtcOnline;
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
        private System.Windows.Forms.Panel panelSubtitleBackgroundColor;
        private System.Windows.Forms.Panel panelSubtitleFontColor;
        private System.Windows.Forms.Label labelSubtitleFontBackgroundColor;
        private System.Windows.Forms.Label labelSubtitleFontColor;
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
        private System.Windows.Forms.NumericUpDown numericUpDownSyntaxColorTextMoreThanXLines;
        private System.Windows.Forms.CheckBox checkBoxSubtitleCenter;
        private System.Windows.Forms.CheckBox checkBoxVideoPlayerShowFullscreenButton;
        private System.Windows.Forms.CheckBox checkBoxVideoPlayerShowMuteButton;
        private System.Windows.Forms.Label labelCustomSearch1;
        private System.Windows.Forms.Label labelCustomSearch2;
        private System.Windows.Forms.TextBox textBoxCustomSearchUrl2;
        private System.Windows.Forms.ComboBox comboBoxCustomSearch2;
        private System.Windows.Forms.Label labelCustomSearch6;
        private System.Windows.Forms.TextBox textBoxCustomSearchUrl6;
        private System.Windows.Forms.ComboBox comboBoxCustomSearch6;
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
        private System.Windows.Forms.GroupBox groupBoxToolsMisc;
        private System.Windows.Forms.CheckBox checkBoxUseDoNotBreakAfterList;
        private System.Windows.Forms.Button buttonEditDoNotBreakAfterList;
        private System.Windows.Forms.CheckBox checkBoxCheckForUpdates;
        private System.Windows.Forms.Label labelPlatform;
        private System.Windows.Forms.CheckBox checkBoxVideoPlayerPreviewFontBold;
    }
}