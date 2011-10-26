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
            this.checkBoxPromptDeleteLines = new System.Windows.Forms.CheckBox();
            this.checkBoxAutoWrapWhileTyping = new System.Windows.Forms.CheckBox();
            this.labelMergeShortLines = new System.Windows.Forms.Label();
            this.comboBoxMergeShortLineLength = new System.Windows.Forms.ComboBox();
            this.checkBoxAllowEditOfOriginalSubtitle = new System.Windows.Forms.CheckBox();
            this.comboBoxSpellChecker = new System.Windows.Forms.ComboBox();
            this.labelSpellChecker = new System.Windows.Forms.Label();
            this.panelSubtitleBackgroundColor = new System.Windows.Forms.Panel();
            this.panelSubtitleFontColor = new System.Windows.Forms.Panel();
            this.labelSubtitleFontBackgroundColor = new System.Windows.Forms.Label();
            this.labelSubtitleFontColor = new System.Windows.Forms.Label();
            this.comboBoxAutoBackup = new System.Windows.Forms.ComboBox();
            this.labelAutoBackup = new System.Windows.Forms.Label();
            this.checkBoxRememberSelectedLine = new System.Windows.Forms.CheckBox();
            this.checkBoxRemoveBlankLinesWhenOpening = new System.Windows.Forms.CheckBox();
            this.checkBoxAutoDetectAnsiEncoding = new System.Windows.Forms.CheckBox();
            this.labelAutoDetectAnsiEncoding = new System.Windows.Forms.Label();
            this.comboBoxListViewDoubleClickEvent = new System.Windows.Forms.ComboBox();
            this.labelListViewDoubleClickEvent = new System.Windows.Forms.Label();
            this.textBoxShowLineBreaksAs = new System.Windows.Forms.TextBox();
            this.labelShowLineBreaksAs = new System.Windows.Forms.Label();
            this.checkBoxRememberWindowPosition = new System.Windows.Forms.CheckBox();
            this.textBoxSubtitleLineMaximumLength = new System.Windows.Forms.TextBox();
            this.labelSubMaxLen = new System.Windows.Forms.Label();
            this.labelSubtitleFontSize = new System.Windows.Forms.Label();
            this.comboBoxSubtitleFont = new System.Windows.Forms.ComboBox();
            this.checkBoxStartInSourceView = new System.Windows.Forms.CheckBox();
            this.checkBoxReopenLastOpened = new System.Windows.Forms.CheckBox();
            this.checkBoxRememberRecentFiles = new System.Windows.Forms.CheckBox();
            this.checkBoxSubtitleFontBold = new System.Windows.Forms.CheckBox();
            this.comboBoxSubtitleFontSize = new System.Windows.Forms.ComboBox();
            this.labelSubtitleFont = new System.Windows.Forms.Label();
            this.comboBoxEncoding = new System.Windows.Forms.ComboBox();
            this.labelDefaultFileEncoding = new System.Windows.Forms.Label();
            this.comboBoxFramerate = new System.Windows.Forms.ComboBox();
            this.labelDefaultFrameRate = new System.Windows.Forms.Label();
            this.tabPageToolBar = new System.Windows.Forms.TabPage();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.checkBoxShowFrameRate = new System.Windows.Forms.CheckBox();
            this.groupBoxShowToolBarButtons = new System.Windows.Forms.GroupBox();
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
            this.textBoxCustomSearchUrl = new System.Windows.Forms.TextBox();
            this.labelCustomSearch = new System.Windows.Forms.Label();
            this.comboBoxCustomSearch = new System.Windows.Forms.ComboBox();
            this.groupBoxVideoPlayerDefault = new System.Windows.Forms.GroupBox();
            this.labelVideoPlayerPreviewFontSize = new System.Windows.Forms.Label();
            this.comboBoxlVideoPlayerPreviewFontSize = new System.Windows.Forms.ComboBox();
            this.checkBoxVideoPlayerShowStopButton = new System.Windows.Forms.CheckBox();
            this.comboBoxVideoPlayerDefaultVolume = new System.Windows.Forms.ComboBox();
            this.labelDefaultVol = new System.Windows.Forms.Label();
            this.labelVolDescr = new System.Windows.Forms.Label();
            this.groupBoxVideoEngine = new System.Windows.Forms.GroupBox();
            this.labelVideoPlayerVLC = new System.Windows.Forms.Label();
            this.radioButtonVideoPlayerVLC = new System.Windows.Forms.RadioButton();
            this.labelVideoPlayerMPlayer = new System.Windows.Forms.Label();
            this.labelDirectShowDescription = new System.Windows.Forms.Label();
            this.labelManagedDirectXDescription = new System.Windows.Forms.Label();
            this.radioButtonVideoPlayerMPlayer = new System.Windows.Forms.RadioButton();
            this.radioButtonVideoPlayerDirectShow = new System.Windows.Forms.RadioButton();
            this.radioButtonVideoPlayerManagedDirectX = new System.Windows.Forms.RadioButton();
            this.tabPageWaveForm = new System.Windows.Forms.TabPage();
            this.groupBoxSpectrogram = new System.Windows.Forms.GroupBox();
            this.labelSpectrogramAppearance = new System.Windows.Forms.Label();
            this.comboBoxSpectrogramAppearance = new System.Windows.Forms.ComboBox();
            this.checkBoxGenerateSpectrogram = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.buttonWaveFormsFolderEmpty = new System.Windows.Forms.Button();
            this.labelWaveFormsFolderInfo = new System.Windows.Forms.Label();
            this.groupBoxWaveFormAppearence = new System.Windows.Forms.GroupBox();
            this.panelWaveFormTextColor = new System.Windows.Forms.Panel();
            this.buttonWaveFormTextColor = new System.Windows.Forms.Button();
            this.panelWaveFormGridColor = new System.Windows.Forms.Panel();
            this.buttonWaveFormGridColor = new System.Windows.Forms.Button();
            this.panelWaveFormBackgroundColor = new System.Windows.Forms.Panel();
            this.buttonWaveFormBackgroundColor = new System.Windows.Forms.Button();
            this.panelWaveFormColor = new System.Windows.Forms.Panel();
            this.buttonWaveFormColor = new System.Windows.Forms.Button();
            this.panelWaveFormSelectedColor = new System.Windows.Forms.Panel();
            this.buttonWaveFormSelectedColor = new System.Windows.Forms.Button();
            this.checkBoxWaveFormShowGrid = new System.Windows.Forms.CheckBox();
            this.tabPageTools = new System.Windows.Forms.TabPage();
            this.groupBoxSpellCheck = new System.Windows.Forms.GroupBox();
            this.checkBoxSpellCheckAutoChangeNames = new System.Windows.Forms.CheckBox();
            this.groupBoxFixCommonErrors = new System.Windows.Forms.GroupBox();
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
            this.labelSSAExample = new System.Windows.Forms.Label();
            this.labelSSAFont = new System.Windows.Forms.Label();
            this.labelExampleColon = new System.Windows.Forms.Label();
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
            this.labelShortcutKey = new System.Windows.Forms.Label();
            this.comboBoxShortcutKey = new System.Windows.Forms.ComboBox();
            this.checkBoxShortcutsShift = new System.Windows.Forms.CheckBox();
            this.checkBoxShortcutsAlt = new System.Windows.Forms.CheckBox();
            this.checkBoxShortcutsControl = new System.Windows.Forms.CheckBox();
            this.buttonUpdateShortcut = new System.Windows.Forms.Button();
            this.treeViewShortcuts = new System.Windows.Forms.TreeView();
            this.labelShortcut = new System.Windows.Forms.Label();
            this.colorDialogSSAStyle = new System.Windows.Forms.ColorDialog();
            this.fontDialogSSAStyle = new System.Windows.Forms.FontDialog();
            this.labelStatus = new System.Windows.Forms.Label();
            this.checkBoxReverseMouseWheelScrollDirection = new System.Windows.Forms.CheckBox();
            this.tabControlSettings.SuspendLayout();
            this.tabPageGenerel.SuspendLayout();
            this.groupBoxMiscellaneous.SuspendLayout();
            this.tabPageToolBar.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBoxShowToolBarButtons.SuspendLayout();
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
            this.tabPageWaveForm.SuspendLayout();
            this.groupBoxSpectrogram.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBoxWaveFormAppearence.SuspendLayout();
            this.tabPageTools.SuspendLayout();
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
            this.tabPageProxy.SuspendLayout();
            this.groupBoxProxySettings.SuspendLayout();
            this.groupBoxProxyAuthentication.SuspendLayout();
            this.tabPageShortcuts.SuspendLayout();
            this.groupBoxShortcuts.SuspendLayout();
            this.SuspendLayout();
            //
            // buttonOK
            //
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(661, 456);
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
            this.buttonCancel.Location = new System.Drawing.Point(740, 456);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 21);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            //
            // tabControlSettings
            //
            this.tabControlSettings.Controls.Add(this.tabPageGenerel);
            this.tabControlSettings.Controls.Add(this.tabPageToolBar);
            this.tabControlSettings.Controls.Add(this.tabPageVideoPlayer);
            this.tabControlSettings.Controls.Add(this.tabPageWaveForm);
            this.tabControlSettings.Controls.Add(this.tabPageTools);
            this.tabControlSettings.Controls.Add(this.tabPageWordLists);
            this.tabControlSettings.Controls.Add(this.tabPageSsaStyle);
            this.tabControlSettings.Controls.Add(this.tabPageProxy);
            this.tabControlSettings.Controls.Add(this.tabPageShortcuts);
            this.tabControlSettings.Location = new System.Drawing.Point(13, 13);
            this.tabControlSettings.Name = "tabControlSettings";
            this.tabControlSettings.SelectedIndex = 0;
            this.tabControlSettings.Size = new System.Drawing.Size(806, 437);
            this.tabControlSettings.TabIndex = 2;
            this.tabControlSettings.SelectedIndexChanged += new System.EventHandler(this.TabControlSettingsSelectedIndexChanged);
            //
            // tabPageGenerel
            //
            this.tabPageGenerel.Controls.Add(this.groupBoxMiscellaneous);
            this.tabPageGenerel.Location = new System.Drawing.Point(4, 22);
            this.tabPageGenerel.Name = "tabPageGenerel";
            this.tabPageGenerel.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageGenerel.Size = new System.Drawing.Size(798, 411);
            this.tabPageGenerel.TabIndex = 0;
            this.tabPageGenerel.Text = "Generel";
            this.tabPageGenerel.UseVisualStyleBackColor = true;
            //
            // groupBoxMiscellaneous
            //
            this.groupBoxMiscellaneous.Controls.Add(this.checkBoxPromptDeleteLines);
            this.groupBoxMiscellaneous.Controls.Add(this.checkBoxAutoWrapWhileTyping);
            this.groupBoxMiscellaneous.Controls.Add(this.labelMergeShortLines);
            this.groupBoxMiscellaneous.Controls.Add(this.comboBoxMergeShortLineLength);
            this.groupBoxMiscellaneous.Controls.Add(this.checkBoxAllowEditOfOriginalSubtitle);
            this.groupBoxMiscellaneous.Controls.Add(this.comboBoxSpellChecker);
            this.groupBoxMiscellaneous.Controls.Add(this.labelSpellChecker);
            this.groupBoxMiscellaneous.Controls.Add(this.panelSubtitleBackgroundColor);
            this.groupBoxMiscellaneous.Controls.Add(this.panelSubtitleFontColor);
            this.groupBoxMiscellaneous.Controls.Add(this.labelSubtitleFontBackgroundColor);
            this.groupBoxMiscellaneous.Controls.Add(this.labelSubtitleFontColor);
            this.groupBoxMiscellaneous.Controls.Add(this.comboBoxAutoBackup);
            this.groupBoxMiscellaneous.Controls.Add(this.labelAutoBackup);
            this.groupBoxMiscellaneous.Controls.Add(this.checkBoxRememberSelectedLine);
            this.groupBoxMiscellaneous.Controls.Add(this.checkBoxRemoveBlankLinesWhenOpening);
            this.groupBoxMiscellaneous.Controls.Add(this.checkBoxAutoDetectAnsiEncoding);
            this.groupBoxMiscellaneous.Controls.Add(this.labelAutoDetectAnsiEncoding);
            this.groupBoxMiscellaneous.Controls.Add(this.comboBoxListViewDoubleClickEvent);
            this.groupBoxMiscellaneous.Controls.Add(this.labelListViewDoubleClickEvent);
            this.groupBoxMiscellaneous.Controls.Add(this.textBoxShowLineBreaksAs);
            this.groupBoxMiscellaneous.Controls.Add(this.labelShowLineBreaksAs);
            this.groupBoxMiscellaneous.Controls.Add(this.checkBoxRememberWindowPosition);
            this.groupBoxMiscellaneous.Controls.Add(this.textBoxSubtitleLineMaximumLength);
            this.groupBoxMiscellaneous.Controls.Add(this.labelSubMaxLen);
            this.groupBoxMiscellaneous.Controls.Add(this.labelSubtitleFontSize);
            this.groupBoxMiscellaneous.Controls.Add(this.comboBoxSubtitleFont);
            this.groupBoxMiscellaneous.Controls.Add(this.checkBoxStartInSourceView);
            this.groupBoxMiscellaneous.Controls.Add(this.checkBoxReopenLastOpened);
            this.groupBoxMiscellaneous.Controls.Add(this.checkBoxRememberRecentFiles);
            this.groupBoxMiscellaneous.Controls.Add(this.checkBoxSubtitleFontBold);
            this.groupBoxMiscellaneous.Controls.Add(this.comboBoxSubtitleFontSize);
            this.groupBoxMiscellaneous.Controls.Add(this.labelSubtitleFont);
            this.groupBoxMiscellaneous.Controls.Add(this.comboBoxEncoding);
            this.groupBoxMiscellaneous.Controls.Add(this.labelDefaultFileEncoding);
            this.groupBoxMiscellaneous.Controls.Add(this.comboBoxFramerate);
            this.groupBoxMiscellaneous.Controls.Add(this.labelDefaultFrameRate);
            this.groupBoxMiscellaneous.Location = new System.Drawing.Point(6, 6);
            this.groupBoxMiscellaneous.Name = "groupBoxMiscellaneous";
            this.groupBoxMiscellaneous.Size = new System.Drawing.Size(786, 399);
            this.groupBoxMiscellaneous.TabIndex = 0;
            this.groupBoxMiscellaneous.TabStop = false;
            this.groupBoxMiscellaneous.Text = "Miscellaneous";
            //
            // checkBoxPromptDeleteLines
            //
            this.checkBoxPromptDeleteLines.AutoSize = true;
            this.checkBoxPromptDeleteLines.Location = new System.Drawing.Point(436, 319);
            this.checkBoxPromptDeleteLines.Name = "checkBoxPromptDeleteLines";
            this.checkBoxPromptDeleteLines.Size = new System.Drawing.Size(138, 17);
            this.checkBoxPromptDeleteLines.TabIndex = 40;
            this.checkBoxPromptDeleteLines.Text = "Prompt for deleting lines";
            this.checkBoxPromptDeleteLines.UseVisualStyleBackColor = true;
            //
            // checkBoxAutoWrapWhileTyping
            //
            this.checkBoxAutoWrapWhileTyping.AutoSize = true;
            this.checkBoxAutoWrapWhileTyping.Location = new System.Drawing.Point(193, 146);
            this.checkBoxAutoWrapWhileTyping.Name = "checkBoxAutoWrapWhileTyping";
            this.checkBoxAutoWrapWhileTyping.Size = new System.Drawing.Size(132, 17);
            this.checkBoxAutoWrapWhileTyping.TabIndex = 4;
            this.checkBoxAutoWrapWhileTyping.Text = "Auto-wrap while typing";
            this.checkBoxAutoWrapWhileTyping.UseVisualStyleBackColor = true;
            //
            // labelMergeShortLines
            //
            this.labelMergeShortLines.AutoSize = true;
            this.labelMergeShortLines.Location = new System.Drawing.Point(14, 176);
            this.labelMergeShortLines.Name = "labelMergeShortLines";
            this.labelMergeShortLines.Size = new System.Drawing.Size(124, 13);
            this.labelMergeShortLines.TabIndex = 39;
            this.labelMergeShortLines.Text = "Merge lines shorter than";
            //
            // comboBoxMergeShortLineLength
            //
            this.comboBoxMergeShortLineLength.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxMergeShortLineLength.FormattingEnabled = true;
            this.comboBoxMergeShortLineLength.Location = new System.Drawing.Point(193, 173);
            this.comboBoxMergeShortLineLength.Name = "comboBoxMergeShortLineLength";
            this.comboBoxMergeShortLineLength.Size = new System.Drawing.Size(73, 21);
            this.comboBoxMergeShortLineLength.TabIndex = 5;
            //
            // checkBoxAllowEditOfOriginalSubtitle
            //
            this.checkBoxAllowEditOfOriginalSubtitle.AutoSize = true;
            this.checkBoxAllowEditOfOriginalSubtitle.Location = new System.Drawing.Point(436, 296);
            this.checkBoxAllowEditOfOriginalSubtitle.Name = "checkBoxAllowEditOfOriginalSubtitle";
            this.checkBoxAllowEditOfOriginalSubtitle.Size = new System.Drawing.Size(155, 17);
            this.checkBoxAllowEditOfOriginalSubtitle.TabIndex = 24;
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
            this.comboBoxSpellChecker.Location = new System.Drawing.Point(193, 354);
            this.comboBoxSpellChecker.Name = "comboBoxSpellChecker";
            this.comboBoxSpellChecker.Size = new System.Drawing.Size(121, 21);
            this.comboBoxSpellChecker.TabIndex = 11;
            //
            // labelSpellChecker
            //
            this.labelSpellChecker.AutoSize = true;
            this.labelSpellChecker.Location = new System.Drawing.Point(14, 357);
            this.labelSpellChecker.Name = "labelSpellChecker";
            this.labelSpellChecker.Size = new System.Drawing.Size(69, 13);
            this.labelSpellChecker.TabIndex = 36;
            this.labelSpellChecker.Text = "Spell checker";
            //
            // panelSubtitleBackgroundColor
            //
            this.panelSubtitleBackgroundColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelSubtitleBackgroundColor.Location = new System.Drawing.Point(193, 309);
            this.panelSubtitleBackgroundColor.Name = "panelSubtitleBackgroundColor";
            this.panelSubtitleBackgroundColor.Size = new System.Drawing.Size(46, 15);
            this.panelSubtitleBackgroundColor.TabIndex = 10;
            this.panelSubtitleBackgroundColor.Click += new System.EventHandler(this.panelSubtitleBackgroundColor_Click);
            //
            // panelSubtitleFontColor
            //
            this.panelSubtitleFontColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelSubtitleFontColor.Location = new System.Drawing.Point(193, 289);
            this.panelSubtitleFontColor.Name = "panelSubtitleFontColor";
            this.panelSubtitleFontColor.Size = new System.Drawing.Size(46, 15);
            this.panelSubtitleFontColor.TabIndex = 9;
            this.panelSubtitleFontColor.Click += new System.EventHandler(this.panelSubtitleFontColor_Click);
            //
            // labelSubtitleFontBackgroundColor
            //
            this.labelSubtitleFontBackgroundColor.AutoSize = true;
            this.labelSubtitleFontBackgroundColor.Location = new System.Drawing.Point(14, 310);
            this.labelSubtitleFontBackgroundColor.Name = "labelSubtitleFontBackgroundColor";
            this.labelSubtitleFontBackgroundColor.Size = new System.Drawing.Size(151, 13);
            this.labelSubtitleFontBackgroundColor.TabIndex = 33;
            this.labelSubtitleFontBackgroundColor.Text = "Subtitle font background color";
            //
            // labelSubtitleFontColor
            //
            this.labelSubtitleFontColor.AutoSize = true;
            this.labelSubtitleFontColor.Location = new System.Drawing.Point(14, 290);
            this.labelSubtitleFontColor.Name = "labelSubtitleFontColor";
            this.labelSubtitleFontColor.Size = new System.Drawing.Size(92, 13);
            this.labelSubtitleFontColor.TabIndex = 32;
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
            this.comboBoxAutoBackup.Location = new System.Drawing.Point(540, 252);
            this.comboBoxAutoBackup.Name = "comboBoxAutoBackup";
            this.comboBoxAutoBackup.Size = new System.Drawing.Size(121, 21);
            this.comboBoxAutoBackup.TabIndex = 23;
            //
            // labelAutoBackup
            //
            this.labelAutoBackup.AutoSize = true;
            this.labelAutoBackup.Location = new System.Drawing.Point(433, 255);
            this.labelAutoBackup.Name = "labelAutoBackup";
            this.labelAutoBackup.Size = new System.Drawing.Size(68, 13);
            this.labelAutoBackup.TabIndex = 22;
            this.labelAutoBackup.Text = "Auto-backup";
            //
            // checkBoxRememberSelectedLine
            //
            this.checkBoxRememberSelectedLine.AutoSize = true;
            this.checkBoxRememberSelectedLine.Location = new System.Drawing.Point(444, 75);
            this.checkBoxRememberSelectedLine.Name = "checkBoxRememberSelectedLine";
            this.checkBoxRememberSelectedLine.Size = new System.Drawing.Size(139, 17);
            this.checkBoxRememberSelectedLine.TabIndex = 14;
            this.checkBoxRememberSelectedLine.Text = "Remember selected line";
            this.checkBoxRememberSelectedLine.UseVisualStyleBackColor = true;
            //
            // checkBoxRemoveBlankLinesWhenOpening
            //
            this.checkBoxRemoveBlankLinesWhenOpening.AutoSize = true;
            this.checkBoxRemoveBlankLinesWhenOpening.Location = new System.Drawing.Point(436, 147);
            this.checkBoxRemoveBlankLinesWhenOpening.Name = "checkBoxRemoveBlankLinesWhenOpening";
            this.checkBoxRemoveBlankLinesWhenOpening.Size = new System.Drawing.Size(225, 17);
            this.checkBoxRemoveBlankLinesWhenOpening.TabIndex = 17;
            this.checkBoxRemoveBlankLinesWhenOpening.Text = "Remove blank lines when opening subtitle";
            this.checkBoxRemoveBlankLinesWhenOpening.UseVisualStyleBackColor = true;
            //
            // checkBoxAutoDetectAnsiEncoding
            //
            this.checkBoxAutoDetectAnsiEncoding.AutoSize = true;
            this.checkBoxAutoDetectAnsiEncoding.Location = new System.Drawing.Point(193, 92);
            this.checkBoxAutoDetectAnsiEncoding.Name = "checkBoxAutoDetectAnsiEncoding";
            this.checkBoxAutoDetectAnsiEncoding.Size = new System.Drawing.Size(15, 14);
            this.checkBoxAutoDetectAnsiEncoding.TabIndex = 2;
            this.checkBoxAutoDetectAnsiEncoding.UseVisualStyleBackColor = true;
            //
            // labelAutoDetectAnsiEncoding
            //
            this.labelAutoDetectAnsiEncoding.AutoSize = true;
            this.labelAutoDetectAnsiEncoding.Location = new System.Drawing.Point(13, 91);
            this.labelAutoDetectAnsiEncoding.Name = "labelAutoDetectAnsiEncoding";
            this.labelAutoDetectAnsiEncoding.Size = new System.Drawing.Size(137, 13);
            this.labelAutoDetectAnsiEncoding.TabIndex = 16;
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
            this.comboBoxListViewDoubleClickEvent.Location = new System.Drawing.Point(436, 219);
            this.comboBoxListViewDoubleClickEvent.Name = "comboBoxListViewDoubleClickEvent";
            this.comboBoxListViewDoubleClickEvent.Size = new System.Drawing.Size(332, 21);
            this.comboBoxListViewDoubleClickEvent.TabIndex = 21;
            //
            // labelListViewDoubleClickEvent
            //
            this.labelListViewDoubleClickEvent.AutoSize = true;
            this.labelListViewDoubleClickEvent.Location = new System.Drawing.Point(433, 202);
            this.labelListViewDoubleClickEvent.Name = "labelListViewDoubleClickEvent";
            this.labelListViewDoubleClickEvent.Size = new System.Drawing.Size(227, 13);
            this.labelListViewDoubleClickEvent.TabIndex = 20;
            this.labelListViewDoubleClickEvent.Text = "Double-click on line in main window listview will";
            //
            // textBoxShowLineBreaksAs
            //
            this.textBoxShowLineBreaksAs.Location = new System.Drawing.Point(589, 172);
            this.textBoxShowLineBreaksAs.MaxLength = 10;
            this.textBoxShowLineBreaksAs.Name = "textBoxShowLineBreaksAs";
            this.textBoxShowLineBreaksAs.Size = new System.Drawing.Size(69, 21);
            this.textBoxShowLineBreaksAs.TabIndex = 19;
            //
            // labelShowLineBreaksAs
            //
            this.labelShowLineBreaksAs.AutoSize = true;
            this.labelShowLineBreaksAs.Location = new System.Drawing.Point(433, 175);
            this.labelShowLineBreaksAs.Name = "labelShowLineBreaksAs";
            this.labelShowLineBreaksAs.Size = new System.Drawing.Size(150, 13);
            this.labelShowLineBreaksAs.TabIndex = 18;
            this.labelShowLineBreaksAs.Text = "Show line breaks in listview as";
            //
            // checkBoxRememberWindowPosition
            //
            this.checkBoxRememberWindowPosition.AutoSize = true;
            this.checkBoxRememberWindowPosition.Location = new System.Drawing.Point(436, 101);
            this.checkBoxRememberWindowPosition.Name = "checkBoxRememberWindowPosition";
            this.checkBoxRememberWindowPosition.Size = new System.Drawing.Size(222, 17);
            this.checkBoxRememberWindowPosition.TabIndex = 15;
            this.checkBoxRememberWindowPosition.Text = "Remember main window position and size";
            this.checkBoxRememberWindowPosition.UseVisualStyleBackColor = true;
            //
            // textBoxSubtitleLineMaximumLength
            //
            this.textBoxSubtitleLineMaximumLength.Location = new System.Drawing.Point(193, 121);
            this.textBoxSubtitleLineMaximumLength.MaxLength = 3;
            this.textBoxSubtitleLineMaximumLength.Name = "textBoxSubtitleLineMaximumLength";
            this.textBoxSubtitleLineMaximumLength.Size = new System.Drawing.Size(121, 21);
            this.textBoxSubtitleLineMaximumLength.TabIndex = 3;
            this.textBoxSubtitleLineMaximumLength.Text = "68";
            this.textBoxSubtitleLineMaximumLength.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBoxAjustSecondsKeyDown);
            //
            // labelSubMaxLen
            //
            this.labelSubMaxLen.AutoSize = true;
            this.labelSubMaxLen.Location = new System.Drawing.Point(13, 125);
            this.labelSubMaxLen.Name = "labelSubMaxLen";
            this.labelSubMaxLen.Size = new System.Drawing.Size(103, 13);
            this.labelSubMaxLen.TabIndex = 11;
            this.labelSubMaxLen.Text = "Subtitle max. length";
            //
            // labelSubtitleFontSize
            //
            this.labelSubtitleFontSize.AutoSize = true;
            this.labelSubtitleFontSize.Location = new System.Drawing.Point(13, 245);
            this.labelSubtitleFontSize.Name = "labelSubtitleFontSize";
            this.labelSubtitleFontSize.Size = new System.Drawing.Size(87, 13);
            this.labelSubtitleFontSize.TabIndex = 10;
            this.labelSubtitleFontSize.Text = "Subtitle font size";
            //
            // comboBoxSubtitleFont
            //
            this.comboBoxSubtitleFont.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSubtitleFont.FormattingEnabled = true;
            this.comboBoxSubtitleFont.Location = new System.Drawing.Point(193, 214);
            this.comboBoxSubtitleFont.Name = "comboBoxSubtitleFont";
            this.comboBoxSubtitleFont.Size = new System.Drawing.Size(121, 21);
            this.comboBoxSubtitleFont.TabIndex = 6;
            //
            // checkBoxStartInSourceView
            //
            this.checkBoxStartInSourceView.AutoSize = true;
            this.checkBoxStartInSourceView.Location = new System.Drawing.Point(436, 124);
            this.checkBoxStartInSourceView.Name = "checkBoxStartInSourceView";
            this.checkBoxStartInSourceView.Size = new System.Drawing.Size(119, 17);
            this.checkBoxStartInSourceView.TabIndex = 16;
            this.checkBoxStartInSourceView.Text = "Start in source view";
            this.checkBoxStartInSourceView.UseVisualStyleBackColor = true;
            //
            // checkBoxReopenLastOpened
            //
            this.checkBoxReopenLastOpened.AutoSize = true;
            this.checkBoxReopenLastOpened.Location = new System.Drawing.Point(444, 52);
            this.checkBoxReopenLastOpened.Name = "checkBoxReopenLastOpened";
            this.checkBoxReopenLastOpened.Size = new System.Drawing.Size(140, 17);
            this.checkBoxReopenLastOpened.TabIndex = 13;
            this.checkBoxReopenLastOpened.Text = "Start with last file loaded";
            this.checkBoxReopenLastOpened.UseVisualStyleBackColor = true;
            //
            // checkBoxRememberRecentFiles
            //
            this.checkBoxRememberRecentFiles.AutoSize = true;
            this.checkBoxRememberRecentFiles.Location = new System.Drawing.Point(436, 28);
            this.checkBoxRememberRecentFiles.Name = "checkBoxRememberRecentFiles";
            this.checkBoxRememberRecentFiles.Size = new System.Drawing.Size(188, 17);
            this.checkBoxRememberRecentFiles.TabIndex = 12;
            this.checkBoxRememberRecentFiles.Text = "Remember recent files (for reopen)";
            this.checkBoxRememberRecentFiles.UseVisualStyleBackColor = true;
            this.checkBoxRememberRecentFiles.CheckedChanged += new System.EventHandler(this.checkBoxRememberRecentFiles_CheckedChanged);
            //
            // checkBoxSubtitleFontBold
            //
            this.checkBoxSubtitleFontBold.AutoSize = true;
            this.checkBoxSubtitleFontBold.Location = new System.Drawing.Point(193, 269);
            this.checkBoxSubtitleFontBold.Name = "checkBoxSubtitleFontBold";
            this.checkBoxSubtitleFontBold.Size = new System.Drawing.Size(47, 17);
            this.checkBoxSubtitleFontBold.TabIndex = 8;
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
            this.comboBoxSubtitleFontSize.Location = new System.Drawing.Point(193, 242);
            this.comboBoxSubtitleFontSize.Name = "comboBoxSubtitleFontSize";
            this.comboBoxSubtitleFontSize.Size = new System.Drawing.Size(121, 21);
            this.comboBoxSubtitleFontSize.TabIndex = 7;
            //
            // labelSubtitleFont
            //
            this.labelSubtitleFont.AutoSize = true;
            this.labelSubtitleFont.Location = new System.Drawing.Point(13, 220);
            this.labelSubtitleFont.Name = "labelSubtitleFont";
            this.labelSubtitleFont.Size = new System.Drawing.Size(66, 13);
            this.labelSubtitleFont.TabIndex = 8;
            this.labelSubtitleFont.Text = "Subtitle font";
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
            this.comboBoxEncoding.Location = new System.Drawing.Point(193, 63);
            this.comboBoxEncoding.Name = "comboBoxEncoding";
            this.comboBoxEncoding.Size = new System.Drawing.Size(121, 21);
            this.comboBoxEncoding.TabIndex = 1;
            //
            // labelDefaultFileEncoding
            //
            this.labelDefaultFileEncoding.AutoSize = true;
            this.labelDefaultFileEncoding.Location = new System.Drawing.Point(13, 67);
            this.labelDefaultFileEncoding.Name = "labelDefaultFileEncoding";
            this.labelDefaultFileEncoding.Size = new System.Drawing.Size(105, 13);
            this.labelDefaultFileEncoding.TabIndex = 2;
            this.labelDefaultFileEncoding.Text = "Default file encoding";
            //
            // comboBoxFramerate
            //
            this.comboBoxFramerate.FormattingEnabled = true;
            this.comboBoxFramerate.Location = new System.Drawing.Point(193, 28);
            this.comboBoxFramerate.Name = "comboBoxFramerate";
            this.comboBoxFramerate.Size = new System.Drawing.Size(121, 21);
            this.comboBoxFramerate.TabIndex = 0;
            //
            // labelDefaultFrameRate
            //
            this.labelDefaultFrameRate.AutoSize = true;
            this.labelDefaultFrameRate.Location = new System.Drawing.Point(13, 32);
            this.labelDefaultFrameRate.Name = "labelDefaultFrameRate";
            this.labelDefaultFrameRate.Size = new System.Drawing.Size(93, 13);
            this.labelDefaultFrameRate.TabIndex = 1;
            this.labelDefaultFrameRate.Text = "Default framerate";
            //
            // tabPageToolBar
            //
            this.tabPageToolBar.Controls.Add(this.groupBox2);
            this.tabPageToolBar.Controls.Add(this.groupBoxShowToolBarButtons);
            this.tabPageToolBar.Location = new System.Drawing.Point(4, 22);
            this.tabPageToolBar.Name = "tabPageToolBar";
            this.tabPageToolBar.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageToolBar.Size = new System.Drawing.Size(798, 411);
            this.tabPageToolBar.TabIndex = 7;
            this.tabPageToolBar.Text = "Toolbar ";
            this.tabPageToolBar.UseVisualStyleBackColor = true;
            //
            // groupBox2
            //
            this.groupBox2.Controls.Add(this.checkBoxShowFrameRate);
            this.groupBox2.Location = new System.Drawing.Point(7, 131);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(785, 274);
            this.groupBox2.TabIndex = 35;
            this.groupBox2.TabStop = false;
            //
            // checkBoxShowFrameRate
            //
            this.checkBoxShowFrameRate.AutoSize = true;
            this.checkBoxShowFrameRate.Location = new System.Drawing.Point(16, 29);
            this.checkBoxShowFrameRate.Name = "checkBoxShowFrameRate";
            this.checkBoxShowFrameRate.Size = new System.Drawing.Size(149, 17);
            this.checkBoxShowFrameRate.TabIndex = 34;
            this.checkBoxShowFrameRate.Text = "Show frame rate in toolbar";
            this.checkBoxShowFrameRate.UseVisualStyleBackColor = true;
            //
            // groupBoxShowToolBarButtons
            //
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
            this.groupBoxShowToolBarButtons.Size = new System.Drawing.Size(786, 118);
            this.groupBoxShowToolBarButtons.TabIndex = 0;
            this.groupBoxShowToolBarButtons.TabStop = false;
            this.groupBoxShowToolBarButtons.Text = "Show toolbar buttons";
            //
            // labelTBHelp
            //
            this.labelTBHelp.AutoSize = true;
            this.labelTBHelp.Location = new System.Drawing.Point(702, 21);
            this.labelTBHelp.Name = "labelTBHelp";
            this.labelTBHelp.Size = new System.Drawing.Size(28, 13);
            this.labelTBHelp.TabIndex = 33;
            this.labelTBHelp.Text = "Help";
            //
            // pictureBoxHelp
            //
            this.pictureBoxHelp.Location = new System.Drawing.Point(701, 40);
            this.pictureBoxHelp.Name = "pictureBoxHelp";
            this.pictureBoxHelp.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxHelp.TabIndex = 32;
            this.pictureBoxHelp.TabStop = false;
            //
            // checkBoxHelp
            //
            this.checkBoxHelp.AutoSize = true;
            this.checkBoxHelp.Location = new System.Drawing.Point(704, 80);
            this.checkBoxHelp.Name = "checkBoxHelp";
            this.checkBoxHelp.Size = new System.Drawing.Size(56, 17);
            this.checkBoxHelp.TabIndex = 31;
            this.checkBoxHelp.Text = "Visible";
            this.checkBoxHelp.UseVisualStyleBackColor = true;
            //
            // labelTBSettings
            //
            this.labelTBSettings.AutoSize = true;
            this.labelTBSettings.Location = new System.Drawing.Point(622, 21);
            this.labelTBSettings.Name = "labelTBSettings";
            this.labelTBSettings.Size = new System.Drawing.Size(46, 13);
            this.labelTBSettings.TabIndex = 30;
            this.labelTBSettings.Text = "Settings";
            //
            // pictureBoxSettings
            //
            this.pictureBoxSettings.Location = new System.Drawing.Point(625, 40);
            this.pictureBoxSettings.Name = "pictureBoxSettings";
            this.pictureBoxSettings.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxSettings.TabIndex = 29;
            this.pictureBoxSettings.TabStop = false;
            //
            // checkBoxSettings
            //
            this.checkBoxSettings.AutoSize = true;
            this.checkBoxSettings.Location = new System.Drawing.Point(628, 80);
            this.checkBoxSettings.Name = "checkBoxSettings";
            this.checkBoxSettings.Size = new System.Drawing.Size(56, 17);
            this.checkBoxSettings.TabIndex = 28;
            this.checkBoxSettings.Text = "Visible";
            this.checkBoxSettings.UseVisualStyleBackColor = true;
            //
            // labelTBSpellCheck
            //
            this.labelTBSpellCheck.AutoSize = true;
            this.labelTBSpellCheck.Location = new System.Drawing.Point(545, 21);
            this.labelTBSpellCheck.Name = "labelTBSpellCheck";
            this.labelTBSpellCheck.Size = new System.Drawing.Size(59, 13);
            this.labelTBSpellCheck.TabIndex = 27;
            this.labelTBSpellCheck.Text = "Spell check";
            //
            // pictureBoxSpellCheck
            //
            this.pictureBoxSpellCheck.Location = new System.Drawing.Point(549, 40);
            this.pictureBoxSpellCheck.Name = "pictureBoxSpellCheck";
            this.pictureBoxSpellCheck.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxSpellCheck.TabIndex = 26;
            this.pictureBoxSpellCheck.TabStop = false;
            //
            // checkBoxSpellCheck
            //
            this.checkBoxSpellCheck.AutoSize = true;
            this.checkBoxSpellCheck.Location = new System.Drawing.Point(550, 80);
            this.checkBoxSpellCheck.Name = "checkBoxSpellCheck";
            this.checkBoxSpellCheck.Size = new System.Drawing.Size(56, 17);
            this.checkBoxSpellCheck.TabIndex = 26;
            this.checkBoxSpellCheck.Text = "Visible";
            this.checkBoxSpellCheck.UseVisualStyleBackColor = true;
            //
            // labelTBVisualSync
            //
            this.labelTBVisualSync.AutoSize = true;
            this.labelTBVisualSync.Location = new System.Drawing.Point(460, 21);
            this.labelTBVisualSync.Name = "labelTBVisualSync";
            this.labelTBVisualSync.Size = new System.Drawing.Size(59, 13);
            this.labelTBVisualSync.TabIndex = 21;
            this.labelTBVisualSync.Text = "Visual sync";
            //
            // pictureBoxVisualSync
            //
            this.pictureBoxVisualSync.Location = new System.Drawing.Point(473, 40);
            this.pictureBoxVisualSync.Name = "pictureBoxVisualSync";
            this.pictureBoxVisualSync.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxVisualSync.TabIndex = 20;
            this.pictureBoxVisualSync.TabStop = false;
            //
            // checkBoxVisualSync
            //
            this.checkBoxVisualSync.AutoSize = true;
            this.checkBoxVisualSync.Location = new System.Drawing.Point(476, 80);
            this.checkBoxVisualSync.Name = "checkBoxVisualSync";
            this.checkBoxVisualSync.Size = new System.Drawing.Size(56, 17);
            this.checkBoxVisualSync.TabIndex = 19;
            this.checkBoxVisualSync.Text = "Visible";
            this.checkBoxVisualSync.UseVisualStyleBackColor = true;
            //
            // labelTBReplace
            //
            this.labelTBReplace.AutoSize = true;
            this.labelTBReplace.Location = new System.Drawing.Point(392, 21);
            this.labelTBReplace.Name = "labelTBReplace";
            this.labelTBReplace.Size = new System.Drawing.Size(45, 13);
            this.labelTBReplace.TabIndex = 18;
            this.labelTBReplace.Text = "Replace";
            //
            // pictureBoxReplace
            //
            this.pictureBoxReplace.Location = new System.Drawing.Point(397, 40);
            this.pictureBoxReplace.Name = "pictureBoxReplace";
            this.pictureBoxReplace.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxReplace.TabIndex = 17;
            this.pictureBoxReplace.TabStop = false;
            //
            // checkBoxReplace
            //
            this.checkBoxReplace.AutoSize = true;
            this.checkBoxReplace.Location = new System.Drawing.Point(400, 80);
            this.checkBoxReplace.Name = "checkBoxReplace";
            this.checkBoxReplace.Size = new System.Drawing.Size(56, 17);
            this.checkBoxReplace.TabIndex = 16;
            this.checkBoxReplace.Text = "Visible";
            this.checkBoxReplace.UseVisualStyleBackColor = true;
            //
            // labelTBFind
            //
            this.labelTBFind.AutoSize = true;
            this.labelTBFind.Location = new System.Drawing.Point(323, 21);
            this.labelTBFind.Name = "labelTBFind";
            this.labelTBFind.Size = new System.Drawing.Size(27, 13);
            this.labelTBFind.TabIndex = 15;
            this.labelTBFind.Text = "Find";
            //
            // pictureBoxFind
            //
            this.pictureBoxFind.Location = new System.Drawing.Point(321, 40);
            this.pictureBoxFind.Name = "pictureBoxFind";
            this.pictureBoxFind.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxFind.TabIndex = 14;
            this.pictureBoxFind.TabStop = false;
            //
            // checkBoxToolbarFind
            //
            this.checkBoxToolbarFind.AutoSize = true;
            this.checkBoxToolbarFind.Location = new System.Drawing.Point(324, 80);
            this.checkBoxToolbarFind.Name = "checkBoxToolbarFind";
            this.checkBoxToolbarFind.Size = new System.Drawing.Size(56, 17);
            this.checkBoxToolbarFind.TabIndex = 13;
            this.checkBoxToolbarFind.Text = "Visible";
            this.checkBoxToolbarFind.UseVisualStyleBackColor = true;
            //
            // labelTBSaveAs
            //
            this.labelTBSaveAs.AutoSize = true;
            this.labelTBSaveAs.Location = new System.Drawing.Point(239, 21);
            this.labelTBSaveAs.Name = "labelTBSaveAs";
            this.labelTBSaveAs.Size = new System.Drawing.Size(45, 13);
            this.labelTBSaveAs.TabIndex = 12;
            this.labelTBSaveAs.Text = "Save as";
            //
            // pictureBoxSaveAs
            //
            this.pictureBoxSaveAs.Location = new System.Drawing.Point(245, 40);
            this.pictureBoxSaveAs.Name = "pictureBoxSaveAs";
            this.pictureBoxSaveAs.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxSaveAs.TabIndex = 11;
            this.pictureBoxSaveAs.TabStop = false;
            //
            // checkBoxToolbarSaveAs
            //
            this.checkBoxToolbarSaveAs.AutoSize = true;
            this.checkBoxToolbarSaveAs.Location = new System.Drawing.Point(248, 80);
            this.checkBoxToolbarSaveAs.Name = "checkBoxToolbarSaveAs";
            this.checkBoxToolbarSaveAs.Size = new System.Drawing.Size(56, 17);
            this.checkBoxToolbarSaveAs.TabIndex = 10;
            this.checkBoxToolbarSaveAs.Text = "Visible";
            this.checkBoxToolbarSaveAs.UseVisualStyleBackColor = true;
            //
            // labelTBSave
            //
            this.labelTBSave.AutoSize = true;
            this.labelTBSave.Location = new System.Drawing.Point(170, 21);
            this.labelTBSave.Name = "labelTBSave";
            this.labelTBSave.Size = new System.Drawing.Size(31, 13);
            this.labelTBSave.TabIndex = 9;
            this.labelTBSave.Text = "Save";
            //
            // pictureBoxSave
            //
            this.pictureBoxSave.Location = new System.Drawing.Point(169, 40);
            this.pictureBoxSave.Name = "pictureBoxSave";
            this.pictureBoxSave.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxSave.TabIndex = 8;
            this.pictureBoxSave.TabStop = false;
            //
            // checkBoxToolbarSave
            //
            this.checkBoxToolbarSave.AutoSize = true;
            this.checkBoxToolbarSave.Location = new System.Drawing.Point(172, 80);
            this.checkBoxToolbarSave.Name = "checkBoxToolbarSave";
            this.checkBoxToolbarSave.Size = new System.Drawing.Size(56, 17);
            this.checkBoxToolbarSave.TabIndex = 7;
            this.checkBoxToolbarSave.Text = "Visible";
            this.checkBoxToolbarSave.UseVisualStyleBackColor = true;
            //
            // labelTBOpen
            //
            this.labelTBOpen.AutoSize = true;
            this.labelTBOpen.Location = new System.Drawing.Point(94, 21);
            this.labelTBOpen.Name = "labelTBOpen";
            this.labelTBOpen.Size = new System.Drawing.Size(33, 13);
            this.labelTBOpen.TabIndex = 6;
            this.labelTBOpen.Text = "Open";
            //
            // pictureBoxOpen
            //
            this.pictureBoxOpen.Location = new System.Drawing.Point(93, 40);
            this.pictureBoxOpen.Name = "pictureBoxOpen";
            this.pictureBoxOpen.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxOpen.TabIndex = 5;
            this.pictureBoxOpen.TabStop = false;
            //
            // checkBoxToolbarOpen
            //
            this.checkBoxToolbarOpen.AutoSize = true;
            this.checkBoxToolbarOpen.Location = new System.Drawing.Point(96, 80);
            this.checkBoxToolbarOpen.Name = "checkBoxToolbarOpen";
            this.checkBoxToolbarOpen.Size = new System.Drawing.Size(56, 17);
            this.checkBoxToolbarOpen.TabIndex = 4;
            this.checkBoxToolbarOpen.Text = "Visible";
            this.checkBoxToolbarOpen.UseVisualStyleBackColor = true;
            //
            // labelTBNew
            //
            this.labelTBNew.AutoSize = true;
            this.labelTBNew.Location = new System.Drawing.Point(19, 21);
            this.labelTBNew.Name = "labelTBNew";
            this.labelTBNew.Size = new System.Drawing.Size(28, 13);
            this.labelTBNew.TabIndex = 3;
            this.labelTBNew.Text = "New";
            //
            // pictureBoxNew
            //
            this.pictureBoxNew.Location = new System.Drawing.Point(17, 40);
            this.pictureBoxNew.Name = "pictureBoxNew";
            this.pictureBoxNew.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxNew.TabIndex = 2;
            this.pictureBoxNew.TabStop = false;
            //
            // checkBoxToolbarNew
            //
            this.checkBoxToolbarNew.AutoSize = true;
            this.checkBoxToolbarNew.Location = new System.Drawing.Point(20, 80);
            this.checkBoxToolbarNew.Name = "checkBoxToolbarNew";
            this.checkBoxToolbarNew.Size = new System.Drawing.Size(56, 17);
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
            this.tabPageVideoPlayer.Size = new System.Drawing.Size(798, 411);
            this.tabPageVideoPlayer.TabIndex = 2;
            this.tabPageVideoPlayer.Text = "Video player";
            this.tabPageVideoPlayer.UseVisualStyleBackColor = true;
            //
            // groupBoxMainWindowVideoControls
            //
            this.groupBoxMainWindowVideoControls.Controls.Add(this.textBoxCustomSearchUrl);
            this.groupBoxMainWindowVideoControls.Controls.Add(this.labelCustomSearch);
            this.groupBoxMainWindowVideoControls.Controls.Add(this.comboBoxCustomSearch);
            this.groupBoxMainWindowVideoControls.Location = new System.Drawing.Point(7, 296);
            this.groupBoxMainWindowVideoControls.Name = "groupBoxMainWindowVideoControls";
            this.groupBoxMainWindowVideoControls.Size = new System.Drawing.Size(785, 109);
            this.groupBoxMainWindowVideoControls.TabIndex = 15;
            this.groupBoxMainWindowVideoControls.TabStop = false;
            this.groupBoxMainWindowVideoControls.Text = "Main window video controls";
            //
            // textBoxCustomSearchUrl
            //
            this.textBoxCustomSearchUrl.Location = new System.Drawing.Point(166, 38);
            this.textBoxCustomSearchUrl.Name = "textBoxCustomSearchUrl";
            this.textBoxCustomSearchUrl.Size = new System.Drawing.Size(601, 21);
            this.textBoxCustomSearchUrl.TabIndex = 2;
            //
            // labelCustomSearch
            //
            this.labelCustomSearch.AutoSize = true;
            this.labelCustomSearch.Location = new System.Drawing.Point(12, 20);
            this.labelCustomSearch.Name = "labelCustomSearch";
            this.labelCustomSearch.Size = new System.Drawing.Size(137, 13);
            this.labelCustomSearch.TabIndex = 1;
            this.labelCustomSearch.Text = "Custom search text and url";
            //
            // comboBoxCustomSearch
            //
            this.comboBoxCustomSearch.FormattingEnabled = true;
            this.comboBoxCustomSearch.Items.AddRange(new object[] {
            "The Free Dictionary",
            "Dictionary.com",
            "VISUWORDS"});
            this.comboBoxCustomSearch.Location = new System.Drawing.Point(12, 38);
            this.comboBoxCustomSearch.Name = "comboBoxCustomSearch";
            this.comboBoxCustomSearch.Size = new System.Drawing.Size(148, 21);
            this.comboBoxCustomSearch.TabIndex = 0;
            this.comboBoxCustomSearch.SelectedIndexChanged += new System.EventHandler(this.comboBoxCustomSearch_SelectedIndexChanged);
            //
            // groupBoxVideoPlayerDefault
            //
            this.groupBoxVideoPlayerDefault.Controls.Add(this.labelVideoPlayerPreviewFontSize);
            this.groupBoxVideoPlayerDefault.Controls.Add(this.comboBoxlVideoPlayerPreviewFontSize);
            this.groupBoxVideoPlayerDefault.Controls.Add(this.checkBoxVideoPlayerShowStopButton);
            this.groupBoxVideoPlayerDefault.Controls.Add(this.comboBoxVideoPlayerDefaultVolume);
            this.groupBoxVideoPlayerDefault.Controls.Add(this.labelDefaultVol);
            this.groupBoxVideoPlayerDefault.Controls.Add(this.labelVolDescr);
            this.groupBoxVideoPlayerDefault.Location = new System.Drawing.Point(7, 135);
            this.groupBoxVideoPlayerDefault.Name = "groupBoxVideoPlayerDefault";
            this.groupBoxVideoPlayerDefault.Size = new System.Drawing.Size(785, 155);
            this.groupBoxVideoPlayerDefault.TabIndex = 14;
            this.groupBoxVideoPlayerDefault.TabStop = false;
            //
            // labelVideoPlayerPreviewFontSize
            //
            this.labelVideoPlayerPreviewFontSize.AutoSize = true;
            this.labelVideoPlayerPreviewFontSize.Location = new System.Drawing.Point(11, 103);
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
            this.comboBoxlVideoPlayerPreviewFontSize.Location = new System.Drawing.Point(145, 100);
            this.comboBoxlVideoPlayerPreviewFontSize.Name = "comboBoxlVideoPlayerPreviewFontSize";
            this.comboBoxlVideoPlayerPreviewFontSize.Size = new System.Drawing.Size(121, 21);
            this.comboBoxlVideoPlayerPreviewFontSize.TabIndex = 13;
            //
            // checkBoxVideoPlayerShowStopButton
            //
            this.checkBoxVideoPlayerShowStopButton.AutoSize = true;
            this.checkBoxVideoPlayerShowStopButton.Location = new System.Drawing.Point(9, 19);
            this.checkBoxVideoPlayerShowStopButton.Name = "checkBoxVideoPlayerShowStopButton";
            this.checkBoxVideoPlayerShowStopButton.Size = new System.Drawing.Size(109, 17);
            this.checkBoxVideoPlayerShowStopButton.TabIndex = 10;
            this.checkBoxVideoPlayerShowStopButton.Text = "Show stop button";
            this.checkBoxVideoPlayerShowStopButton.UseVisualStyleBackColor = true;
            //
            // comboBoxVideoPlayerDefaultVolume
            //
            this.comboBoxVideoPlayerDefaultVolume.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxVideoPlayerDefaultVolume.FormattingEnabled = true;
            this.comboBoxVideoPlayerDefaultVolume.Location = new System.Drawing.Point(127, 53);
            this.comboBoxVideoPlayerDefaultVolume.Name = "comboBoxVideoPlayerDefaultVolume";
            this.comboBoxVideoPlayerDefaultVolume.Size = new System.Drawing.Size(121, 21);
            this.comboBoxVideoPlayerDefaultVolume.TabIndex = 12;
            //
            // labelDefaultVol
            //
            this.labelDefaultVol.AutoSize = true;
            this.labelDefaultVol.Location = new System.Drawing.Point(9, 56);
            this.labelDefaultVol.Name = "labelDefaultVol";
            this.labelDefaultVol.Size = new System.Drawing.Size(79, 13);
            this.labelDefaultVol.TabIndex = 6;
            this.labelDefaultVol.Text = "Default volume";
            //
            // labelVolDescr
            //
            this.labelVolDescr.AutoSize = true;
            this.labelVolDescr.Font = new System.Drawing.Font("Tahoma", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelVolDescr.ForeColor = System.Drawing.Color.Gray;
            this.labelVolDescr.Location = new System.Drawing.Point(254, 57);
            this.labelVolDescr.Name = "labelVolDescr";
            this.labelVolDescr.Size = new System.Drawing.Size(150, 11);
            this.labelVolDescr.TabIndex = 9;
            this.labelVolDescr.Text = "0 is no sound, 100 is higest volume";
            //
            // groupBoxVideoEngine
            //
            this.groupBoxVideoEngine.Controls.Add(this.labelVideoPlayerVLC);
            this.groupBoxVideoEngine.Controls.Add(this.radioButtonVideoPlayerVLC);
            this.groupBoxVideoEngine.Controls.Add(this.labelVideoPlayerMPlayer);
            this.groupBoxVideoEngine.Controls.Add(this.labelDirectShowDescription);
            this.groupBoxVideoEngine.Controls.Add(this.labelManagedDirectXDescription);
            this.groupBoxVideoEngine.Controls.Add(this.radioButtonVideoPlayerMPlayer);
            this.groupBoxVideoEngine.Controls.Add(this.radioButtonVideoPlayerDirectShow);
            this.groupBoxVideoEngine.Controls.Add(this.radioButtonVideoPlayerManagedDirectX);
            this.groupBoxVideoEngine.Location = new System.Drawing.Point(6, 6);
            this.groupBoxVideoEngine.Name = "groupBoxVideoEngine";
            this.groupBoxVideoEngine.Size = new System.Drawing.Size(786, 123);
            this.groupBoxVideoEngine.TabIndex = 0;
            this.groupBoxVideoEngine.TabStop = false;
            this.groupBoxVideoEngine.Text = "Video engine";
            //
            // labelVideoPlayerVLC
            //
            this.labelVideoPlayerVLC.AutoSize = true;
            this.labelVideoPlayerVLC.Font = new System.Drawing.Font("Tahoma", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelVideoPlayerVLC.ForeColor = System.Drawing.Color.Gray;
            this.labelVideoPlayerVLC.Location = new System.Drawing.Point(167, 49);
            this.labelVideoPlayerVLC.Name = "labelVideoPlayerVLC";
            this.labelVideoPlayerVLC.Size = new System.Drawing.Size(209, 11);
            this.labelVideoPlayerVLC.TabIndex = 13;
            this.labelVideoPlayerVLC.Text = "libvlc.dll from VLC Media Player (1.1.0 or newer)";
            //
            // radioButtonVideoPlayerVLC
            //
            this.radioButtonVideoPlayerVLC.AutoSize = true;
            this.radioButtonVideoPlayerVLC.Location = new System.Drawing.Point(10, 46);
            this.radioButtonVideoPlayerVLC.Name = "radioButtonVideoPlayerVLC";
            this.radioButtonVideoPlayerVLC.Size = new System.Drawing.Size(45, 17);
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
            this.labelVideoPlayerMPlayer.Location = new System.Drawing.Point(167, 70);
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
            // labelManagedDirectXDescription
            //
            this.labelManagedDirectXDescription.AutoSize = true;
            this.labelManagedDirectXDescription.Font = new System.Drawing.Font("Tahoma", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelManagedDirectXDescription.ForeColor = System.Drawing.Color.Gray;
            this.labelManagedDirectXDescription.Location = new System.Drawing.Point(167, 93);
            this.labelManagedDirectXDescription.Name = "labelManagedDirectXDescription";
            this.labelManagedDirectXDescription.Size = new System.Drawing.Size(314, 11);
            this.labelManagedDirectXDescription.TabIndex = 9;
            this.labelManagedDirectXDescription.Text = "Microsoft.DirectX.AudioVideoPlayback -  .NET Managed code from DirectX";
            this.labelManagedDirectXDescription.Visible = false;
            //
            // radioButtonVideoPlayerMPlayer
            //
            this.radioButtonVideoPlayerMPlayer.AutoSize = true;
            this.radioButtonVideoPlayerMPlayer.Location = new System.Drawing.Point(10, 67);
            this.radioButtonVideoPlayerMPlayer.Name = "radioButtonVideoPlayerMPlayer";
            this.radioButtonVideoPlayerMPlayer.Size = new System.Drawing.Size(69, 17);
            this.radioButtonVideoPlayerMPlayer.TabIndex = 8;
            this.radioButtonVideoPlayerMPlayer.TabStop = true;
            this.radioButtonVideoPlayerMPlayer.Text = "MPlayer2";
            this.radioButtonVideoPlayerMPlayer.UseVisualStyleBackColor = true;
            //
            // radioButtonVideoPlayerDirectShow
            //
            this.radioButtonVideoPlayerDirectShow.AutoSize = true;
            this.radioButtonVideoPlayerDirectShow.Location = new System.Drawing.Point(10, 23);
            this.radioButtonVideoPlayerDirectShow.Name = "radioButtonVideoPlayerDirectShow";
            this.radioButtonVideoPlayerDirectShow.Size = new System.Drawing.Size(83, 17);
            this.radioButtonVideoPlayerDirectShow.TabIndex = 1;
            this.radioButtonVideoPlayerDirectShow.TabStop = true;
            this.radioButtonVideoPlayerDirectShow.Text = "DirectShow ";
            this.radioButtonVideoPlayerDirectShow.UseVisualStyleBackColor = true;
            //
            // radioButtonVideoPlayerManagedDirectX
            //
            this.radioButtonVideoPlayerManagedDirectX.AutoSize = true;
            this.radioButtonVideoPlayerManagedDirectX.Location = new System.Drawing.Point(10, 90);
            this.radioButtonVideoPlayerManagedDirectX.Name = "radioButtonVideoPlayerManagedDirectX";
            this.radioButtonVideoPlayerManagedDirectX.Size = new System.Drawing.Size(108, 17);
            this.radioButtonVideoPlayerManagedDirectX.TabIndex = 6;
            this.radioButtonVideoPlayerManagedDirectX.TabStop = true;
            this.radioButtonVideoPlayerManagedDirectX.Text = "Managed DirectX";
            this.radioButtonVideoPlayerManagedDirectX.UseVisualStyleBackColor = true;
            this.radioButtonVideoPlayerManagedDirectX.Visible = false;
            //
            // tabPageWaveForm
            //
            this.tabPageWaveForm.Controls.Add(this.groupBoxSpectrogram);
            this.tabPageWaveForm.Controls.Add(this.groupBox1);
            this.tabPageWaveForm.Controls.Add(this.groupBoxWaveFormAppearence);
            this.tabPageWaveForm.Location = new System.Drawing.Point(4, 22);
            this.tabPageWaveForm.Name = "tabPageWaveForm";
            this.tabPageWaveForm.Size = new System.Drawing.Size(798, 411);
            this.tabPageWaveForm.TabIndex = 6;
            this.tabPageWaveForm.Text = "Waveform/spectrogram";
            this.tabPageWaveForm.UseVisualStyleBackColor = true;
            //
            // groupBoxSpectrogram
            //
            this.groupBoxSpectrogram.Controls.Add(this.labelSpectrogramAppearance);
            this.groupBoxSpectrogram.Controls.Add(this.comboBoxSpectrogramAppearance);
            this.groupBoxSpectrogram.Controls.Add(this.checkBoxGenerateSpectrogram);
            this.groupBoxSpectrogram.Location = new System.Drawing.Point(6, 216);
            this.groupBoxSpectrogram.Name = "groupBoxSpectrogram";
            this.groupBoxSpectrogram.Size = new System.Drawing.Size(786, 103);
            this.groupBoxSpectrogram.TabIndex = 4;
            this.groupBoxSpectrogram.TabStop = false;
            this.groupBoxSpectrogram.Text = "Spectrogram";
            //
            // labelSpectrogramAppearance
            //
            this.labelSpectrogramAppearance.AutoSize = true;
            this.labelSpectrogramAppearance.Location = new System.Drawing.Point(10, 54);
            this.labelSpectrogramAppearance.Name = "labelSpectrogramAppearance";
            this.labelSpectrogramAppearance.Size = new System.Drawing.Size(164, 13);
            this.labelSpectrogramAppearance.TabIndex = 15;
            this.labelSpectrogramAppearance.Text = "Appearence (at generation time)";
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
            this.comboBoxSpectrogramAppearance.TabIndex = 14;
            //
            // checkBoxGenerateSpectrogram
            //
            this.checkBoxGenerateSpectrogram.AutoSize = true;
            this.checkBoxGenerateSpectrogram.Location = new System.Drawing.Point(10, 20);
            this.checkBoxGenerateSpectrogram.Name = "checkBoxGenerateSpectrogram";
            this.checkBoxGenerateSpectrogram.Size = new System.Drawing.Size(131, 17);
            this.checkBoxGenerateSpectrogram.TabIndex = 11;
            this.checkBoxGenerateSpectrogram.Text = "Generate spectrogram";
            this.checkBoxGenerateSpectrogram.UseVisualStyleBackColor = true;
            //
            // groupBox1
            //
            this.groupBox1.Controls.Add(this.buttonWaveFormsFolderEmpty);
            this.groupBox1.Controls.Add(this.labelWaveFormsFolderInfo);
            this.groupBox1.Location = new System.Drawing.Point(6, 325);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(786, 77);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            //
            // buttonWaveFormsFolderEmpty
            //
            this.buttonWaveFormsFolderEmpty.Location = new System.Drawing.Point(10, 37);
            this.buttonWaveFormsFolderEmpty.Name = "buttonWaveFormsFolderEmpty";
            this.buttonWaveFormsFolderEmpty.Size = new System.Drawing.Size(325, 23);
            this.buttonWaveFormsFolderEmpty.TabIndex = 1;
            this.buttonWaveFormsFolderEmpty.Text = "Empty \'WaveForms\' folder";
            this.buttonWaveFormsFolderEmpty.UseVisualStyleBackColor = true;
            this.buttonWaveFormsFolderEmpty.Click += new System.EventHandler(this.buttonWaveFormsFolderEmpty_Click);
            //
            // labelWaveFormsFolderInfo
            //
            this.labelWaveFormsFolderInfo.AutoSize = true;
            this.labelWaveFormsFolderInfo.Location = new System.Drawing.Point(10, 20);
            this.labelWaveFormsFolderInfo.Name = "labelWaveFormsFolderInfo";
            this.labelWaveFormsFolderInfo.Size = new System.Drawing.Size(207, 13);
            this.labelWaveFormsFolderInfo.TabIndex = 0;
            this.labelWaveFormsFolderInfo.Text = "\'WaveForms\' folder contains x files (x mb)";
            //
            // groupBoxWaveFormAppearence
            //
            this.groupBoxWaveFormAppearence.Controls.Add(this.checkBoxReverseMouseWheelScrollDirection);
            this.groupBoxWaveFormAppearence.Controls.Add(this.panelWaveFormTextColor);
            this.groupBoxWaveFormAppearence.Controls.Add(this.buttonWaveFormTextColor);
            this.groupBoxWaveFormAppearence.Controls.Add(this.panelWaveFormGridColor);
            this.groupBoxWaveFormAppearence.Controls.Add(this.buttonWaveFormGridColor);
            this.groupBoxWaveFormAppearence.Controls.Add(this.panelWaveFormBackgroundColor);
            this.groupBoxWaveFormAppearence.Controls.Add(this.buttonWaveFormBackgroundColor);
            this.groupBoxWaveFormAppearence.Controls.Add(this.panelWaveFormColor);
            this.groupBoxWaveFormAppearence.Controls.Add(this.buttonWaveFormColor);
            this.groupBoxWaveFormAppearence.Controls.Add(this.panelWaveFormSelectedColor);
            this.groupBoxWaveFormAppearence.Controls.Add(this.buttonWaveFormSelectedColor);
            this.groupBoxWaveFormAppearence.Controls.Add(this.checkBoxWaveFormShowGrid);
            this.groupBoxWaveFormAppearence.Location = new System.Drawing.Point(6, 6);
            this.groupBoxWaveFormAppearence.Name = "groupBoxWaveFormAppearence";
            this.groupBoxWaveFormAppearence.Size = new System.Drawing.Size(786, 203);
            this.groupBoxWaveFormAppearence.TabIndex = 2;
            this.groupBoxWaveFormAppearence.TabStop = false;
            this.groupBoxWaveFormAppearence.Text = "Wave form appearence";
            //
            // panelWaveFormTextColor
            //
            this.panelWaveFormTextColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelWaveFormTextColor.Location = new System.Drawing.Point(138, 109);
            this.panelWaveFormTextColor.Name = "panelWaveFormTextColor";
            this.panelWaveFormTextColor.Size = new System.Drawing.Size(21, 20);
            this.panelWaveFormTextColor.TabIndex = 7;
            this.panelWaveFormTextColor.Click += new System.EventHandler(this.buttonWaveFormTextColor_Click);
            //
            // buttonWaveFormTextColor
            //
            this.buttonWaveFormTextColor.Location = new System.Drawing.Point(16, 108);
            this.buttonWaveFormTextColor.Name = "buttonWaveFormTextColor";
            this.buttonWaveFormTextColor.Size = new System.Drawing.Size(112, 21);
            this.buttonWaveFormTextColor.TabIndex = 6;
            this.buttonWaveFormTextColor.Text = "Text color";
            this.buttonWaveFormTextColor.UseVisualStyleBackColor = true;
            this.buttonWaveFormTextColor.Click += new System.EventHandler(this.buttonWaveFormTextColor_Click);
            //
            // panelWaveFormGridColor
            //
            this.panelWaveFormGridColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelWaveFormGridColor.Location = new System.Drawing.Point(138, 135);
            this.panelWaveFormGridColor.Name = "panelWaveFormGridColor";
            this.panelWaveFormGridColor.Size = new System.Drawing.Size(21, 20);
            this.panelWaveFormGridColor.TabIndex = 9;
            this.panelWaveFormGridColor.Click += new System.EventHandler(this.buttonWaveFormGridColor_Click);
            //
            // buttonWaveFormGridColor
            //
            this.buttonWaveFormGridColor.Location = new System.Drawing.Point(16, 135);
            this.buttonWaveFormGridColor.Name = "buttonWaveFormGridColor";
            this.buttonWaveFormGridColor.Size = new System.Drawing.Size(112, 21);
            this.buttonWaveFormGridColor.TabIndex = 8;
            this.buttonWaveFormGridColor.Text = "Grid color";
            this.buttonWaveFormGridColor.UseVisualStyleBackColor = true;
            this.buttonWaveFormGridColor.Click += new System.EventHandler(this.buttonWaveFormGridColor_Click);
            //
            // panelWaveFormBackgroundColor
            //
            this.panelWaveFormBackgroundColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelWaveFormBackgroundColor.Location = new System.Drawing.Point(138, 82);
            this.panelWaveFormBackgroundColor.Name = "panelWaveFormBackgroundColor";
            this.panelWaveFormBackgroundColor.Size = new System.Drawing.Size(21, 20);
            this.panelWaveFormBackgroundColor.TabIndex = 5;
            this.panelWaveFormBackgroundColor.Click += new System.EventHandler(this.buttonWaveFormBackgroundColor_Click);
            //
            // buttonWaveFormBackgroundColor
            //
            this.buttonWaveFormBackgroundColor.Location = new System.Drawing.Point(16, 81);
            this.buttonWaveFormBackgroundColor.Name = "buttonWaveFormBackgroundColor";
            this.buttonWaveFormBackgroundColor.Size = new System.Drawing.Size(112, 21);
            this.buttonWaveFormBackgroundColor.TabIndex = 4;
            this.buttonWaveFormBackgroundColor.Text = "Back color";
            this.buttonWaveFormBackgroundColor.UseVisualStyleBackColor = true;
            this.buttonWaveFormBackgroundColor.Click += new System.EventHandler(this.buttonWaveFormBackgroundColor_Click);
            //
            // panelWaveFormColor
            //
            this.panelWaveFormColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelWaveFormColor.Location = new System.Drawing.Point(138, 55);
            this.panelWaveFormColor.Name = "panelWaveFormColor";
            this.panelWaveFormColor.Size = new System.Drawing.Size(21, 20);
            this.panelWaveFormColor.TabIndex = 3;
            this.panelWaveFormColor.Click += new System.EventHandler(this.buttonWaveFormColor_Click);
            //
            // buttonWaveFormColor
            //
            this.buttonWaveFormColor.Location = new System.Drawing.Point(16, 54);
            this.buttonWaveFormColor.Name = "buttonWaveFormColor";
            this.buttonWaveFormColor.Size = new System.Drawing.Size(112, 21);
            this.buttonWaveFormColor.TabIndex = 2;
            this.buttonWaveFormColor.Text = "Color";
            this.buttonWaveFormColor.UseVisualStyleBackColor = true;
            this.buttonWaveFormColor.Click += new System.EventHandler(this.buttonWaveFormColor_Click);
            //
            // panelWaveFormSelectedColor
            //
            this.panelWaveFormSelectedColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelWaveFormSelectedColor.Location = new System.Drawing.Point(138, 27);
            this.panelWaveFormSelectedColor.Name = "panelWaveFormSelectedColor";
            this.panelWaveFormSelectedColor.Size = new System.Drawing.Size(21, 20);
            this.panelWaveFormSelectedColor.TabIndex = 1;
            this.panelWaveFormSelectedColor.MouseClick += new System.Windows.Forms.MouseEventHandler(this.buttonWaveFormSelectedColor_Click);
            //
            // buttonWaveFormSelectedColor
            //
            this.buttonWaveFormSelectedColor.Location = new System.Drawing.Point(16, 27);
            this.buttonWaveFormSelectedColor.Name = "buttonWaveFormSelectedColor";
            this.buttonWaveFormSelectedColor.Size = new System.Drawing.Size(112, 21);
            this.buttonWaveFormSelectedColor.TabIndex = 0;
            this.buttonWaveFormSelectedColor.Text = "Selected color";
            this.buttonWaveFormSelectedColor.UseVisualStyleBackColor = true;
            this.buttonWaveFormSelectedColor.Click += new System.EventHandler(this.buttonWaveFormSelectedColor_Click);
            //
            // checkBoxWaveFormShowGrid
            //
            this.checkBoxWaveFormShowGrid.AutoSize = true;
            this.checkBoxWaveFormShowGrid.Location = new System.Drawing.Point(16, 162);
            this.checkBoxWaveFormShowGrid.Name = "checkBoxWaveFormShowGrid";
            this.checkBoxWaveFormShowGrid.Size = new System.Drawing.Size(73, 17);
            this.checkBoxWaveFormShowGrid.TabIndex = 10;
            this.checkBoxWaveFormShowGrid.Text = "Show grid";
            this.checkBoxWaveFormShowGrid.UseVisualStyleBackColor = true;
            //
            // tabPageTools
            //
            this.tabPageTools.Controls.Add(this.groupBoxSpellCheck);
            this.tabPageTools.Controls.Add(this.groupBoxFixCommonErrors);
            this.tabPageTools.Controls.Add(this.groupBoxToolsVisualSync);
            this.tabPageTools.Location = new System.Drawing.Point(4, 22);
            this.tabPageTools.Name = "tabPageTools";
            this.tabPageTools.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageTools.Size = new System.Drawing.Size(798, 411);
            this.tabPageTools.TabIndex = 5;
            this.tabPageTools.Text = "Tools";
            this.tabPageTools.UseVisualStyleBackColor = true;
            //
            // groupBoxSpellCheck
            //
            this.groupBoxSpellCheck.Controls.Add(this.checkBoxSpellCheckAutoChangeNames);
            this.groupBoxSpellCheck.Location = new System.Drawing.Point(6, 283);
            this.groupBoxSpellCheck.Name = "groupBoxSpellCheck";
            this.groupBoxSpellCheck.Size = new System.Drawing.Size(785, 122);
            this.groupBoxSpellCheck.TabIndex = 4;
            this.groupBoxSpellCheck.TabStop = false;
            this.groupBoxSpellCheck.Text = "Spell check";
            //
            // checkBoxSpellCheckAutoChangeNames
            //
            this.checkBoxSpellCheckAutoChangeNames.AutoSize = true;
            this.checkBoxSpellCheckAutoChangeNames.Location = new System.Drawing.Point(15, 20);
            this.checkBoxSpellCheckAutoChangeNames.Name = "checkBoxSpellCheckAutoChangeNames";
            this.checkBoxSpellCheckAutoChangeNames.Size = new System.Drawing.Size(209, 17);
            this.checkBoxSpellCheckAutoChangeNames.TabIndex = 0;
            this.checkBoxSpellCheckAutoChangeNames.Text = "Auto fix names where only casing differ";
            this.checkBoxSpellCheckAutoChangeNames.UseVisualStyleBackColor = true;
            //
            // groupBoxFixCommonErrors
            //
            this.groupBoxFixCommonErrors.Controls.Add(this.checkBoxFixCommonOcrErrorsUsingHardcodedRules);
            this.groupBoxFixCommonErrors.Controls.Add(this.comboBoxToolsMusicSymbol);
            this.groupBoxFixCommonErrors.Controls.Add(this.textBoxMusicSymbolsToReplace);
            this.groupBoxFixCommonErrors.Controls.Add(this.labelToolsMusicSymbolsToReplace);
            this.groupBoxFixCommonErrors.Controls.Add(this.labelToolsMusicSymbol);
            this.groupBoxFixCommonErrors.Location = new System.Drawing.Point(7, 129);
            this.groupBoxFixCommonErrors.Name = "groupBoxFixCommonErrors";
            this.groupBoxFixCommonErrors.Size = new System.Drawing.Size(785, 148);
            this.groupBoxFixCommonErrors.TabIndex = 3;
            this.groupBoxFixCommonErrors.TabStop = false;
            this.groupBoxFixCommonErrors.Text = "Fix common errors";
            //
            // checkBoxFixCommonOcrErrorsUsingHardcodedRules
            //
            this.checkBoxFixCommonOcrErrorsUsingHardcodedRules.AutoSize = true;
            this.checkBoxFixCommonOcrErrorsUsingHardcodedRules.Location = new System.Drawing.Point(15, 115);
            this.checkBoxFixCommonOcrErrorsUsingHardcodedRules.Name = "checkBoxFixCommonOcrErrorsUsingHardcodedRules";
            this.checkBoxFixCommonOcrErrorsUsingHardcodedRules.Size = new System.Drawing.Size(264, 17);
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
            this.groupBoxToolsVisualSync.Size = new System.Drawing.Size(786, 116);
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
            this.tabPageWordLists.Size = new System.Drawing.Size(798, 411);
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
            this.groupBoxWordLists.Size = new System.Drawing.Size(786, 399);
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
            this.groupBoxOcrFixList.Size = new System.Drawing.Size(264, 267);
            this.groupBoxOcrFixList.TabIndex = 6;
            this.groupBoxOcrFixList.TabStop = false;
            this.groupBoxOcrFixList.Text = "OCR fix list";
            //
            // textBoxOcrFixValue
            //
            this.textBoxOcrFixValue.Location = new System.Drawing.Point(82, 238);
            this.textBoxOcrFixValue.Name = "textBoxOcrFixValue";
            this.textBoxOcrFixValue.Size = new System.Drawing.Size(74, 21);
            this.textBoxOcrFixValue.TabIndex = 45;
            this.textBoxOcrFixValue.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBoxOcrFixValueKeyDown);
            //
            // buttonRemoveOcrFix
            //
            this.buttonRemoveOcrFix.Location = new System.Drawing.Point(162, 16);
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
            this.listBoxOcrFixList.Size = new System.Drawing.Size(150, 199);
            this.listBoxOcrFixList.TabIndex = 40;
            this.listBoxOcrFixList.SelectedIndexChanged += new System.EventHandler(this.ListBoxOcrFixListSelectedIndexChanged);
            this.listBoxOcrFixList.Enter += new System.EventHandler(this.ListBoxSearchReset);
            this.listBoxOcrFixList.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ListBoxKeyDownSearch);
            //
            // textBoxOcrFixKey
            //
            this.textBoxOcrFixKey.Location = new System.Drawing.Point(6, 238);
            this.textBoxOcrFixKey.Name = "textBoxOcrFixKey";
            this.textBoxOcrFixKey.Size = new System.Drawing.Size(74, 21);
            this.textBoxOcrFixKey.TabIndex = 44;
            //
            // buttonAddOcrFix
            //
            this.buttonAddOcrFix.Location = new System.Drawing.Point(162, 236);
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
            this.groupBoxWordListLocation.Size = new System.Drawing.Size(774, 77);
            this.groupBoxWordListLocation.TabIndex = 8;
            this.groupBoxWordListLocation.TabStop = false;
            this.groupBoxWordListLocation.Text = "Location";
            //
            // checkBoxNamesEtcOnline
            //
            this.checkBoxNamesEtcOnline.AutoSize = true;
            this.checkBoxNamesEtcOnline.Location = new System.Drawing.Point(7, 22);
            this.checkBoxNamesEtcOnline.Name = "checkBoxNamesEtcOnline";
            this.checkBoxNamesEtcOnline.Size = new System.Drawing.Size(162, 17);
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
            this.textBoxNamesEtcOnline.Text = "http://www.nikse.dk/se/Names_Etc.xml";
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
            this.tabPageSsaStyle.Size = new System.Drawing.Size(798, 411);
            this.tabPageSsaStyle.TabIndex = 1;
            this.tabPageSsaStyle.Text = "SSA style";
            this.tabPageSsaStyle.UseVisualStyleBackColor = true;
            //
            // groupBoxSsaStyle
            //
            this.groupBoxSsaStyle.Controls.Add(this.labelSSAExample);
            this.groupBoxSsaStyle.Controls.Add(this.labelSSAFont);
            this.groupBoxSsaStyle.Controls.Add(this.labelExampleColon);
            this.groupBoxSsaStyle.Controls.Add(this.buttonSSAChooseColor);
            this.groupBoxSsaStyle.Controls.Add(this.buttonSSAChooseFont);
            this.groupBoxSsaStyle.Location = new System.Drawing.Point(6, 6);
            this.groupBoxSsaStyle.Name = "groupBoxSsaStyle";
            this.groupBoxSsaStyle.Size = new System.Drawing.Size(786, 399);
            this.groupBoxSsaStyle.TabIndex = 0;
            this.groupBoxSsaStyle.TabStop = false;
            this.groupBoxSsaStyle.Text = "Sub Station Alpha style";
            //
            // labelSSAExample
            //
            this.labelSSAExample.BackColor = System.Drawing.Color.Black;
            this.labelSSAExample.Font = new System.Drawing.Font("Tahoma", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSSAExample.ForeColor = System.Drawing.Color.White;
            this.labelSSAExample.Location = new System.Drawing.Point(23, 152);
            this.labelSSAExample.Name = "labelSSAExample";
            this.labelSSAExample.Size = new System.Drawing.Size(737, 101);
            this.labelSSAExample.TabIndex = 4;
            this.labelSSAExample.Text = "Testing 123...";
            this.labelSSAExample.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
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
            // labelExampleColon
            //
            this.labelExampleColon.AutoSize = true;
            this.labelExampleColon.Location = new System.Drawing.Point(25, 137);
            this.labelExampleColon.Name = "labelExampleColon";
            this.labelExampleColon.Size = new System.Drawing.Size(51, 13);
            this.labelExampleColon.TabIndex = 2;
            this.labelExampleColon.Text = "Example:";
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
            this.tabPageProxy.Size = new System.Drawing.Size(798, 411);
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
            this.groupBoxProxySettings.Size = new System.Drawing.Size(786, 399);
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
            this.tabPageShortcuts.Size = new System.Drawing.Size(798, 411);
            this.tabPageShortcuts.TabIndex = 8;
            this.tabPageShortcuts.Text = "Shortcuts";
            this.tabPageShortcuts.UseVisualStyleBackColor = true;
            //
            // groupBoxShortcuts
            //
            this.groupBoxShortcuts.Controls.Add(this.labelShortcutKey);
            this.groupBoxShortcuts.Controls.Add(this.comboBoxShortcutKey);
            this.groupBoxShortcuts.Controls.Add(this.checkBoxShortcutsShift);
            this.groupBoxShortcuts.Controls.Add(this.checkBoxShortcutsAlt);
            this.groupBoxShortcuts.Controls.Add(this.checkBoxShortcutsControl);
            this.groupBoxShortcuts.Controls.Add(this.buttonUpdateShortcut);
            this.groupBoxShortcuts.Controls.Add(this.treeViewShortcuts);
            this.groupBoxShortcuts.Controls.Add(this.labelShortcut);
            this.groupBoxShortcuts.Location = new System.Drawing.Point(6, 6);
            this.groupBoxShortcuts.Name = "groupBoxShortcuts";
            this.groupBoxShortcuts.Size = new System.Drawing.Size(786, 399);
            this.groupBoxShortcuts.TabIndex = 2;
            this.groupBoxShortcuts.TabStop = false;
            this.groupBoxShortcuts.Text = "Shortcuts";
            //
            // labelShortcutKey
            //
            this.labelShortcutKey.AutoSize = true;
            this.labelShortcutKey.Location = new System.Drawing.Point(320, 367);
            this.labelShortcutKey.Name = "labelShortcutKey";
            this.labelShortcutKey.Size = new System.Drawing.Size(25, 13);
            this.labelShortcutKey.TabIndex = 35;
            this.labelShortcutKey.Text = "Key";
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
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "0",
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
            "End",
            "Ins",
            "Left",
            "Pause",
            "Right",
            "Space",
            "Tab",
            "Up",
            "NumPad0",
            "NumPad1",
            "NumPad2",
            "NumPad3",
            "NumPad4",
            "NumPad5",
            "NumPad6",
            "NumPad7",
            "NumPad8",
            "NumPad9"});
            this.comboBoxShortcutKey.Location = new System.Drawing.Point(351, 365);
            this.comboBoxShortcutKey.Name = "comboBoxShortcutKey";
            this.comboBoxShortcutKey.Size = new System.Drawing.Size(92, 21);
            this.comboBoxShortcutKey.TabIndex = 4;
            //
            // checkBoxShortcutsShift
            //
            this.checkBoxShortcutsShift.AutoSize = true;
            this.checkBoxShortcutsShift.Enabled = false;
            this.checkBoxShortcutsShift.Location = new System.Drawing.Point(243, 367);
            this.checkBoxShortcutsShift.Name = "checkBoxShortcutsShift";
            this.checkBoxShortcutsShift.Size = new System.Drawing.Size(47, 17);
            this.checkBoxShortcutsShift.TabIndex = 3;
            this.checkBoxShortcutsShift.Text = "Shift";
            this.checkBoxShortcutsShift.UseVisualStyleBackColor = true;
            //
            // checkBoxShortcutsAlt
            //
            this.checkBoxShortcutsAlt.AutoSize = true;
            this.checkBoxShortcutsAlt.Enabled = false;
            this.checkBoxShortcutsAlt.Location = new System.Drawing.Point(174, 367);
            this.checkBoxShortcutsAlt.Name = "checkBoxShortcutsAlt";
            this.checkBoxShortcutsAlt.Size = new System.Drawing.Size(38, 17);
            this.checkBoxShortcutsAlt.TabIndex = 2;
            this.checkBoxShortcutsAlt.Text = "Alt";
            this.checkBoxShortcutsAlt.UseVisualStyleBackColor = true;
            //
            // checkBoxShortcutsControl
            //
            this.checkBoxShortcutsControl.AutoSize = true;
            this.checkBoxShortcutsControl.Enabled = false;
            this.checkBoxShortcutsControl.Location = new System.Drawing.Point(87, 367);
            this.checkBoxShortcutsControl.Name = "checkBoxShortcutsControl";
            this.checkBoxShortcutsControl.Size = new System.Drawing.Size(59, 17);
            this.checkBoxShortcutsControl.TabIndex = 1;
            this.checkBoxShortcutsControl.Text = "Control";
            this.checkBoxShortcutsControl.UseVisualStyleBackColor = true;
            //
            // buttonUpdateShortcut
            //
            this.buttonUpdateShortcut.Enabled = false;
            this.buttonUpdateShortcut.Location = new System.Drawing.Point(482, 363);
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
            this.treeViewShortcuts.Size = new System.Drawing.Size(751, 325);
            this.treeViewShortcuts.TabIndex = 0;
            this.treeViewShortcuts.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewShortcuts_AfterSelect);
            //
            // labelShortcut
            //
            this.labelShortcut.AutoSize = true;
            this.labelShortcut.Location = new System.Drawing.Point(13, 367);
            this.labelShortcut.Name = "labelShortcut";
            this.labelShortcut.Size = new System.Drawing.Size(52, 13);
            this.labelShortcut.TabIndex = 3;
            this.labelShortcut.Text = "Shortcut:";
            //
            // labelStatus
            //
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(10, 456);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(60, 13);
            this.labelStatus.TabIndex = 3;
            this.labelStatus.Text = "labelStatus";
            //
            // checkBoxReverseMouseWheelScrollDirection
            //
            this.checkBoxReverseMouseWheelScrollDirection.AutoSize = true;
            this.checkBoxReverseMouseWheelScrollDirection.Location = new System.Drawing.Point(262, 31);
            this.checkBoxReverseMouseWheelScrollDirection.Name = "checkBoxReverseMouseWheelScrollDirection";
            this.checkBoxReverseMouseWheelScrollDirection.Size = new System.Drawing.Size(202, 17);
            this.checkBoxReverseMouseWheelScrollDirection.TabIndex = 11;
            this.checkBoxReverseMouseWheelScrollDirection.Text = "Reverse mouse wheel scroll direction";
            this.checkBoxReverseMouseWheelScrollDirection.UseVisualStyleBackColor = true;
            //
            // Settings
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(831, 484);
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
            this.Text = "Settings";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormSettings_KeyDown);
            this.tabControlSettings.ResumeLayout(false);
            this.tabPageGenerel.ResumeLayout(false);
            this.groupBoxMiscellaneous.ResumeLayout(false);
            this.groupBoxMiscellaneous.PerformLayout();
            this.tabPageToolBar.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBoxShowToolBarButtons.ResumeLayout(false);
            this.groupBoxShowToolBarButtons.PerformLayout();
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
            this.tabPageWaveForm.ResumeLayout(false);
            this.groupBoxSpectrogram.ResumeLayout(false);
            this.groupBoxSpectrogram.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBoxWaveFormAppearence.ResumeLayout(false);
            this.groupBoxWaveFormAppearence.PerformLayout();
            this.tabPageTools.ResumeLayout(false);
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
            this.tabPageProxy.ResumeLayout(false);
            this.groupBoxProxySettings.ResumeLayout(false);
            this.groupBoxProxySettings.PerformLayout();
            this.groupBoxProxyAuthentication.ResumeLayout(false);
            this.groupBoxProxyAuthentication.PerformLayout();
            this.tabPageShortcuts.ResumeLayout(false);
            this.groupBoxShortcuts.ResumeLayout(false);
            this.groupBoxShortcuts.PerformLayout();
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
        private System.Windows.Forms.ComboBox comboBoxFramerate;
        private System.Windows.Forms.CheckBox checkBoxSubtitleFontBold;
        private System.Windows.Forms.ComboBox comboBoxSubtitleFontSize;
        private System.Windows.Forms.Label labelSubtitleFont;
        private System.Windows.Forms.ComboBox comboBoxEncoding;
        private System.Windows.Forms.CheckBox checkBoxRememberRecentFiles;
        private System.Windows.Forms.GroupBox groupBoxSsaStyle;
        private System.Windows.Forms.Label labelSSAExample;
        private System.Windows.Forms.Label labelSSAFont;
        private System.Windows.Forms.Label labelExampleColon;
        private System.Windows.Forms.Button buttonSSAChooseColor;
        private System.Windows.Forms.Button buttonSSAChooseFont;
        private System.Windows.Forms.ColorDialog colorDialogSSAStyle;
        private System.Windows.Forms.FontDialog fontDialogSSAStyle;
        private System.Windows.Forms.CheckBox checkBoxStartInSourceView;
        private System.Windows.Forms.CheckBox checkBoxReopenLastOpened;
        private System.Windows.Forms.Label labelSubtitleFontSize;
        private System.Windows.Forms.ComboBox comboBoxSubtitleFont;
        private System.Windows.Forms.Label labelSubMaxLen;
        private System.Windows.Forms.TextBox textBoxSubtitleLineMaximumLength;
        private System.Windows.Forms.TabPage tabPageVideoPlayer;
        private System.Windows.Forms.CheckBox checkBoxVideoPlayerShowStopButton;
        private System.Windows.Forms.ComboBox comboBoxVideoPlayerDefaultVolume;
        private System.Windows.Forms.Label labelDefaultVol;
        private System.Windows.Forms.Label labelVolDescr;
        private System.Windows.Forms.GroupBox groupBoxVideoEngine;
        private System.Windows.Forms.Label labelVideoPlayerMPlayer;
        private System.Windows.Forms.Label labelDirectShowDescription;
        private System.Windows.Forms.Label labelManagedDirectXDescription;
        private System.Windows.Forms.RadioButton radioButtonVideoPlayerMPlayer;
        private System.Windows.Forms.RadioButton radioButtonVideoPlayerDirectShow;
        private System.Windows.Forms.RadioButton radioButtonVideoPlayerManagedDirectX;
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
        private System.Windows.Forms.TextBox textBoxCustomSearchUrl;
        private System.Windows.Forms.Label labelCustomSearch;
        private System.Windows.Forms.ComboBox comboBoxCustomSearch;
        private System.Windows.Forms.TabPage tabPageWaveForm;
        private System.Windows.Forms.GroupBox groupBoxWaveFormAppearence;
        private System.Windows.Forms.Panel panelWaveFormBackgroundColor;
        private System.Windows.Forms.Button buttonWaveFormBackgroundColor;
        private System.Windows.Forms.Panel panelWaveFormColor;
        private System.Windows.Forms.Button buttonWaveFormColor;
        private System.Windows.Forms.Panel panelWaveFormSelectedColor;
        private System.Windows.Forms.Button buttonWaveFormSelectedColor;
        private System.Windows.Forms.CheckBox checkBoxWaveFormShowGrid;
        private System.Windows.Forms.Panel panelWaveFormGridColor;
        private System.Windows.Forms.Button buttonWaveFormGridColor;
        private System.Windows.Forms.Panel panelWaveFormTextColor;
        private System.Windows.Forms.Button buttonWaveFormTextColor;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button buttonWaveFormsFolderEmpty;
        private System.Windows.Forms.Label labelWaveFormsFolderInfo;
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
    }
}