namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class SettingsProfile
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
            this.groupBoxStyles = new System.Windows.Forms.GroupBox();
            this.labelName = new System.Windows.Forms.Label();
            this.textBoxName = new System.Windows.Forms.TextBox();
            this.groupBoxGeneralRules = new System.Windows.Forms.GroupBox();
            this.labelDialogStyle = new System.Windows.Forms.Label();
            this.comboBoxDialogStyle = new System.Windows.Forms.ComboBox();
            this.checkBoxCpsIncludeWhiteSpace = new System.Windows.Forms.CheckBox();
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
            this.buttonExport = new System.Windows.Forms.Button();
            this.buttonImport = new System.Windows.Forms.Button();
            this.buttonCopy = new System.Windows.Forms.Button();
            this.buttonRemoveAll = new System.Windows.Forms.Button();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.buttonRemove = new System.Windows.Forms.Button();
            this.listViewProfiles = new System.Windows.Forms.ListView();
            this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderMaxLength = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderOptimalCps = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderMaxCps = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderMinGap = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.openFileDialogImport = new System.Windows.Forms.OpenFileDialog();
            this.groupBoxStyles.SuspendLayout();
            this.groupBoxGeneralRules.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownOptimalCharsSec)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxWordsMin)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxNumberOfLines)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDurationMin)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDurationMax)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMinGapMs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxCharsSec)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSubtitleLineMaximumLength)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBoxStyles
            // 
            this.groupBoxStyles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxStyles.Controls.Add(this.labelName);
            this.groupBoxStyles.Controls.Add(this.textBoxName);
            this.groupBoxStyles.Controls.Add(this.groupBoxGeneralRules);
            this.groupBoxStyles.Controls.Add(this.buttonExport);
            this.groupBoxStyles.Controls.Add(this.buttonImport);
            this.groupBoxStyles.Controls.Add(this.buttonCopy);
            this.groupBoxStyles.Controls.Add(this.buttonRemoveAll);
            this.groupBoxStyles.Controls.Add(this.buttonAdd);
            this.groupBoxStyles.Controls.Add(this.buttonRemove);
            this.groupBoxStyles.Controls.Add(this.listViewProfiles);
            this.groupBoxStyles.Location = new System.Drawing.Point(12, 12);
            this.groupBoxStyles.Name = "groupBoxStyles";
            this.groupBoxStyles.Size = new System.Drawing.Size(927, 417);
            this.groupBoxStyles.TabIndex = 1;
            this.groupBoxStyles.TabStop = false;
            // 
            // labelName
            // 
            this.labelName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelName.AutoSize = true;
            this.labelName.Location = new System.Drawing.Point(616, 19);
            this.labelName.Name = "labelName";
            this.labelName.Size = new System.Drawing.Size(35, 13);
            this.labelName.TabIndex = 53;
            this.labelName.Text = "Name";
            // 
            // textBoxName
            // 
            this.textBoxName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxName.Location = new System.Drawing.Point(619, 35);
            this.textBoxName.Name = "textBoxName";
            this.textBoxName.Size = new System.Drawing.Size(250, 20);
            this.textBoxName.TabIndex = 80;
            this.textBoxName.TextChanged += new System.EventHandler(this.textBoxName_TextChanged);
            // 
            // groupBoxGeneralRules
            // 
            this.groupBoxGeneralRules.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxGeneralRules.Controls.Add(this.labelDialogStyle);
            this.groupBoxGeneralRules.Controls.Add(this.comboBoxDialogStyle);
            this.groupBoxGeneralRules.Controls.Add(this.checkBoxCpsIncludeWhiteSpace);
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
            this.groupBoxGeneralRules.Location = new System.Drawing.Point(619, 61);
            this.groupBoxGeneralRules.Name = "groupBoxGeneralRules";
            this.groupBoxGeneralRules.Size = new System.Drawing.Size(302, 320);
            this.groupBoxGeneralRules.TabIndex = 90;
            this.groupBoxGeneralRules.TabStop = false;
            this.groupBoxGeneralRules.Text = "Rules";
            // 
            // labelDialogStyle
            // 
            this.labelDialogStyle.AutoSize = true;
            this.labelDialogStyle.Location = new System.Drawing.Point(6, 260);
            this.labelDialogStyle.Name = "labelDialogStyle";
            this.labelDialogStyle.Size = new System.Drawing.Size(61, 13);
            this.labelDialogStyle.TabIndex = 193;
            this.labelDialogStyle.Text = "Dialog style";
            // 
            // comboBoxDialogStyle
            // 
            this.comboBoxDialogStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDialogStyle.FormattingEnabled = true;
            this.comboBoxDialogStyle.Items.AddRange(new object[] {
            "Dash both lines with space",
            "Dash both lines without space",
            "Dash second line with space",
            "Dash second line without space"});
            this.comboBoxDialogStyle.Location = new System.Drawing.Point(73, 257);
            this.comboBoxDialogStyle.Name = "comboBoxDialogStyle";
            this.comboBoxDialogStyle.Size = new System.Drawing.Size(203, 21);
            this.comboBoxDialogStyle.TabIndex = 194;
            this.comboBoxDialogStyle.SelectedIndexChanged += new System.EventHandler(this.UiElementChanged);
            // 
            // checkBoxCpsIncludeWhiteSpace
            // 
            this.checkBoxCpsIncludeWhiteSpace.AutoSize = true;
            this.checkBoxCpsIncludeWhiteSpace.Location = new System.Drawing.Point(9, 286);
            this.checkBoxCpsIncludeWhiteSpace.Name = "checkBoxCpsIncludeWhiteSpace";
            this.checkBoxCpsIncludeWhiteSpace.Size = new System.Drawing.Size(270, 17);
            this.checkBoxCpsIncludeWhiteSpace.TabIndex = 190;
            this.checkBoxCpsIncludeWhiteSpace.Text = "Characters per second (CPS) includes white spaces";
            this.checkBoxCpsIncludeWhiteSpace.UseVisualStyleBackColor = true;
            this.checkBoxCpsIncludeWhiteSpace.CheckedChanged += new System.EventHandler(this.UiElementChanged);
            // 
            // labelOptimalCharsPerSecond
            // 
            this.labelOptimalCharsPerSecond.AutoSize = true;
            this.labelOptimalCharsPerSecond.Location = new System.Drawing.Point(6, 43);
            this.labelOptimalCharsPerSecond.Name = "labelOptimalCharsPerSecond";
            this.labelOptimalCharsPerSecond.Size = new System.Drawing.Size(93, 13);
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
            this.numericUpDownOptimalCharsSec.Location = new System.Drawing.Point(203, 41);
            this.numericUpDownOptimalCharsSec.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.numericUpDownOptimalCharsSec.Name = "numericUpDownOptimalCharsSec";
            this.numericUpDownOptimalCharsSec.Size = new System.Drawing.Size(56, 20);
            this.numericUpDownOptimalCharsSec.TabIndex = 110;
            this.numericUpDownOptimalCharsSec.Value = new decimal(new int[] {
            11,
            0,
            0,
            0});
            this.numericUpDownOptimalCharsSec.ValueChanged += new System.EventHandler(this.UiElementChanged);
            // 
            // labelSubMaxLen
            // 
            this.labelSubMaxLen.AutoSize = true;
            this.labelSubMaxLen.Location = new System.Drawing.Point(6, 16);
            this.labelSubMaxLen.Name = "labelSubMaxLen";
            this.labelSubMaxLen.Size = new System.Drawing.Size(99, 13);
            this.labelSubMaxLen.TabIndex = 6;
            this.labelSubMaxLen.Text = "Subtitle max. length";
            // 
            // numericUpDownMaxWordsMin
            // 
            this.numericUpDownMaxWordsMin.Location = new System.Drawing.Point(203, 95);
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
            this.numericUpDownMaxWordsMin.Size = new System.Drawing.Size(56, 20);
            this.numericUpDownMaxWordsMin.TabIndex = 130;
            this.numericUpDownMaxWordsMin.Value = new decimal(new int[] {
            300,
            0,
            0,
            0});
            this.numericUpDownMaxWordsMin.ValueChanged += new System.EventHandler(this.UiElementChanged);
            // 
            // labelMergeShortLines
            // 
            this.labelMergeShortLines.AutoSize = true;
            this.labelMergeShortLines.Location = new System.Drawing.Point(6, 232);
            this.labelMergeShortLines.Name = "labelMergeShortLines";
            this.labelMergeShortLines.Size = new System.Drawing.Size(120, 13);
            this.labelMergeShortLines.TabIndex = 16;
            this.labelMergeShortLines.Text = "Merge lines shorter than";
            // 
            // labelMaxWordsPerMin
            // 
            this.labelMaxWordsPerMin.AutoSize = true;
            this.labelMaxWordsPerMin.Location = new System.Drawing.Point(6, 97);
            this.labelMaxWordsPerMin.Name = "labelMaxWordsPerMin";
            this.labelMaxWordsPerMin.Size = new System.Drawing.Size(82, 13);
            this.labelMaxWordsPerMin.TabIndex = 49;
            this.labelMaxWordsPerMin.Text = "Max. words/min";
            // 
            // labelMinDuration
            // 
            this.labelMinDuration.AutoSize = true;
            this.labelMinDuration.Location = new System.Drawing.Point(6, 124);
            this.labelMinDuration.Name = "labelMinDuration";
            this.labelMinDuration.Size = new System.Drawing.Size(130, 13);
            this.labelMinDuration.TabIndex = 10;
            this.labelMinDuration.Text = "Min. duration, milliseconds";
            // 
            // numericUpDownMaxNumberOfLines
            // 
            this.numericUpDownMaxNumberOfLines.Location = new System.Drawing.Point(203, 201);
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
            this.numericUpDownMaxNumberOfLines.Size = new System.Drawing.Size(56, 20);
            this.numericUpDownMaxNumberOfLines.TabIndex = 170;
            this.numericUpDownMaxNumberOfLines.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numericUpDownMaxNumberOfLines.ValueChanged += new System.EventHandler(this.UiElementChanged);
            // 
            // labelMaxDuration
            // 
            this.labelMaxDuration.AutoSize = true;
            this.labelMaxDuration.Location = new System.Drawing.Point(6, 150);
            this.labelMaxDuration.Name = "labelMaxDuration";
            this.labelMaxDuration.Size = new System.Drawing.Size(133, 13);
            this.labelMaxDuration.TabIndex = 12;
            this.labelMaxDuration.Text = "Max. duration, milliseconds";
            // 
            // labelMaxLines
            // 
            this.labelMaxLines.AutoSize = true;
            this.labelMaxLines.Location = new System.Drawing.Point(6, 203);
            this.labelMaxLines.Name = "labelMaxLines";
            this.labelMaxLines.Size = new System.Drawing.Size(104, 13);
            this.labelMaxLines.TabIndex = 47;
            this.labelMaxLines.Text = "Max. number of lines";
            // 
            // numericUpDownDurationMin
            // 
            this.numericUpDownDurationMin.Location = new System.Drawing.Point(203, 122);
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
            this.numericUpDownDurationMin.Size = new System.Drawing.Size(56, 20);
            this.numericUpDownDurationMin.TabIndex = 140;
            this.numericUpDownDurationMin.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDownDurationMin.ValueChanged += new System.EventHandler(this.UiElementChanged);
            // 
            // numericUpDownDurationMax
            // 
            this.numericUpDownDurationMax.Location = new System.Drawing.Point(203, 148);
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
            this.numericUpDownDurationMax.Size = new System.Drawing.Size(56, 20);
            this.numericUpDownDurationMax.TabIndex = 150;
            this.numericUpDownDurationMax.Value = new decimal(new int[] {
            50000,
            0,
            0,
            0});
            this.numericUpDownDurationMax.ValueChanged += new System.EventHandler(this.UiElementChanged);
            // 
            // comboBoxMergeShortLineLength
            // 
            this.comboBoxMergeShortLineLength.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxMergeShortLineLength.FormattingEnabled = true;
            this.comboBoxMergeShortLineLength.Location = new System.Drawing.Point(203, 229);
            this.comboBoxMergeShortLineLength.Name = "comboBoxMergeShortLineLength";
            this.comboBoxMergeShortLineLength.Size = new System.Drawing.Size(73, 21);
            this.comboBoxMergeShortLineLength.TabIndex = 180;
            this.comboBoxMergeShortLineLength.SelectedIndexChanged += new System.EventHandler(this.UiElementChanged);
            // 
            // labelMaxCharsPerSecond
            // 
            this.labelMaxCharsPerSecond.AutoSize = true;
            this.labelMaxCharsPerSecond.Location = new System.Drawing.Point(6, 70);
            this.labelMaxCharsPerSecond.Name = "labelMaxCharsPerSecond";
            this.labelMaxCharsPerSecond.Size = new System.Drawing.Size(81, 13);
            this.labelMaxCharsPerSecond.TabIndex = 9;
            this.labelMaxCharsPerSecond.Text = "Max. chars/sec";
            // 
            // numericUpDownMinGapMs
            // 
            this.numericUpDownMinGapMs.Location = new System.Drawing.Point(203, 174);
            this.numericUpDownMinGapMs.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownMinGapMs.Name = "numericUpDownMinGapMs";
            this.numericUpDownMinGapMs.Size = new System.Drawing.Size(56, 20);
            this.numericUpDownMinGapMs.TabIndex = 160;
            this.numericUpDownMinGapMs.Value = new decimal(new int[] {
            25,
            0,
            0,
            0});
            this.numericUpDownMinGapMs.ValueChanged += new System.EventHandler(this.UiElementChanged);
            // 
            // numericUpDownMaxCharsSec
            // 
            this.numericUpDownMaxCharsSec.DecimalPlaces = 1;
            this.numericUpDownMaxCharsSec.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericUpDownMaxCharsSec.Location = new System.Drawing.Point(203, 68);
            this.numericUpDownMaxCharsSec.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.numericUpDownMaxCharsSec.Name = "numericUpDownMaxCharsSec";
            this.numericUpDownMaxCharsSec.Size = new System.Drawing.Size(56, 20);
            this.numericUpDownMaxCharsSec.TabIndex = 120;
            this.numericUpDownMaxCharsSec.Value = new decimal(new int[] {
            24,
            0,
            0,
            0});
            this.numericUpDownMaxCharsSec.ValueChanged += new System.EventHandler(this.UiElementChanged);
            // 
            // labelMinGapMs
            // 
            this.labelMinGapMs.AutoSize = true;
            this.labelMinGapMs.Location = new System.Drawing.Point(6, 176);
            this.labelMinGapMs.Name = "labelMinGapMs";
            this.labelMinGapMs.Size = new System.Drawing.Size(133, 13);
            this.labelMinGapMs.TabIndex = 14;
            this.labelMinGapMs.Text = "Min. gap between subtitles";
            // 
            // numericUpDownSubtitleLineMaximumLength
            // 
            this.numericUpDownSubtitleLineMaximumLength.Location = new System.Drawing.Point(203, 14);
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
            this.numericUpDownSubtitleLineMaximumLength.Size = new System.Drawing.Size(56, 20);
            this.numericUpDownSubtitleLineMaximumLength.TabIndex = 100;
            this.numericUpDownSubtitleLineMaximumLength.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDownSubtitleLineMaximumLength.ValueChanged += new System.EventHandler(this.UiElementChanged);
            // 
            // buttonExport
            // 
            this.buttonExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonExport.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonExport.Location = new System.Drawing.Point(6, 386);
            this.buttonExport.Name = "buttonExport";
            this.buttonExport.Size = new System.Drawing.Size(82, 23);
            this.buttonExport.TabIndex = 20;
            this.buttonExport.Text = "Export...";
            this.buttonExport.UseVisualStyleBackColor = true;
            this.buttonExport.Click += new System.EventHandler(this.buttonExport_Click);
            // 
            // buttonImport
            // 
            this.buttonImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonImport.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonImport.Location = new System.Drawing.Point(94, 386);
            this.buttonImport.Name = "buttonImport";
            this.buttonImport.Size = new System.Drawing.Size(82, 23);
            this.buttonImport.TabIndex = 30;
            this.buttonImport.Text = "Import...";
            this.buttonImport.UseVisualStyleBackColor = true;
            this.buttonImport.Click += new System.EventHandler(this.buttonImport_Click);
            // 
            // buttonCopy
            // 
            this.buttonCopy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonCopy.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCopy.Location = new System.Drawing.Point(182, 386);
            this.buttonCopy.Name = "buttonCopy";
            this.buttonCopy.Size = new System.Drawing.Size(82, 23);
            this.buttonCopy.TabIndex = 40;
            this.buttonCopy.Text = "Copy";
            this.buttonCopy.UseVisualStyleBackColor = true;
            this.buttonCopy.Click += new System.EventHandler(this.buttonCopy_Click);
            // 
            // buttonRemoveAll
            // 
            this.buttonRemoveAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonRemoveAll.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonRemoveAll.Location = new System.Drawing.Point(446, 386);
            this.buttonRemoveAll.Name = "buttonRemoveAll";
            this.buttonRemoveAll.Size = new System.Drawing.Size(92, 23);
            this.buttonRemoveAll.TabIndex = 70;
            this.buttonRemoveAll.Text = "Remove all";
            this.buttonRemoveAll.UseVisualStyleBackColor = true;
            this.buttonRemoveAll.Click += new System.EventHandler(this.buttonRemoveAll_Click);
            // 
            // buttonAdd
            // 
            this.buttonAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonAdd.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonAdd.Location = new System.Drawing.Point(270, 386);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(82, 23);
            this.buttonAdd.TabIndex = 50;
            this.buttonAdd.Text = "New";
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // buttonRemove
            // 
            this.buttonRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonRemove.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonRemove.Location = new System.Drawing.Point(358, 386);
            this.buttonRemove.Name = "buttonRemove";
            this.buttonRemove.Size = new System.Drawing.Size(82, 23);
            this.buttonRemove.TabIndex = 60;
            this.buttonRemove.Text = "Remove";
            this.buttonRemove.UseVisualStyleBackColor = true;
            this.buttonRemove.Click += new System.EventHandler(this.buttonRemove_Click);
            // 
            // listViewProfiles
            // 
            this.listViewProfiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewProfiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderName,
            this.columnHeaderMaxLength,
            this.columnHeaderOptimalCps,
            this.columnHeaderMaxCps,
            this.columnHeaderMinGap});
            this.listViewProfiles.FullRowSelect = true;
            this.listViewProfiles.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewProfiles.HideSelection = false;
            this.listViewProfiles.Location = new System.Drawing.Point(6, 19);
            this.listViewProfiles.MultiSelect = false;
            this.listViewProfiles.Name = "listViewProfiles";
            this.listViewProfiles.Size = new System.Drawing.Size(604, 362);
            this.listViewProfiles.TabIndex = 10;
            this.listViewProfiles.UseCompatibleStateImageBehavior = false;
            this.listViewProfiles.View = System.Windows.Forms.View.Details;
            this.listViewProfiles.SelectedIndexChanged += new System.EventHandler(this.listViewProfiles_SelectedIndexChanged);
            // 
            // columnHeaderName
            // 
            this.columnHeaderName.Text = "Name";
            this.columnHeaderName.Width = 140;
            // 
            // columnHeaderMaxLength
            // 
            this.columnHeaderMaxLength.Text = "Single line max. length";
            this.columnHeaderMaxLength.Width = 135;
            // 
            // columnHeaderOptimalCps
            // 
            this.columnHeaderOptimalCps.Text = "Optimal CPS";
            this.columnHeaderOptimalCps.Width = 100;
            // 
            // columnHeaderMaxCps
            // 
            this.columnHeaderMaxCps.Text = "Max. CPS";
            this.columnHeaderMaxCps.Width = 80;
            // 
            // columnHeaderMinGap
            // 
            this.columnHeaderMinGap.Text = "Min. gap (ms)";
            this.columnHeaderMinGap.Width = 120;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(864, 440);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 205;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(783, 440);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 200;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // SettingsProfile
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(951, 475);
            this.Controls.Add(this.groupBoxStyles);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(965, 495);
            this.Name = "SettingsProfile";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Profiles";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SettingsProfile_KeyDown);
            this.groupBoxStyles.ResumeLayout(false);
            this.groupBoxStyles.PerformLayout();
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
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.GroupBox groupBoxStyles;
        private System.Windows.Forms.Button buttonExport;
        private System.Windows.Forms.Button buttonImport;
        private System.Windows.Forms.Button buttonCopy;
        private System.Windows.Forms.Button buttonRemoveAll;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.Button buttonRemove;
        private System.Windows.Forms.ListView listViewProfiles;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.ColumnHeader columnHeaderMaxLength;
        private System.Windows.Forms.ColumnHeader columnHeaderOptimalCps;
        private System.Windows.Forms.ColumnHeader columnHeaderMaxCps;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.ColumnHeader columnHeaderMinGap;
        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.TextBox textBoxName;
        private System.Windows.Forms.GroupBox groupBoxGeneralRules;
        private System.Windows.Forms.Label labelOptimalCharsPerSecond;
        private System.Windows.Forms.NumericUpDown numericUpDownOptimalCharsSec;
        private System.Windows.Forms.Label labelSubMaxLen;
        private System.Windows.Forms.NumericUpDown numericUpDownMaxWordsMin;
        private System.Windows.Forms.Label labelMergeShortLines;
        private System.Windows.Forms.Label labelMaxWordsPerMin;
        private System.Windows.Forms.Label labelMinDuration;
        private System.Windows.Forms.NumericUpDown numericUpDownMaxNumberOfLines;
        private System.Windows.Forms.Label labelMaxDuration;
        private System.Windows.Forms.Label labelMaxLines;
        private System.Windows.Forms.NumericUpDown numericUpDownDurationMin;
        private System.Windows.Forms.NumericUpDown numericUpDownDurationMax;
        private System.Windows.Forms.ComboBox comboBoxMergeShortLineLength;
        private System.Windows.Forms.Label labelMaxCharsPerSecond;
        private System.Windows.Forms.NumericUpDown numericUpDownMinGapMs;
        private System.Windows.Forms.NumericUpDown numericUpDownMaxCharsSec;
        private System.Windows.Forms.Label labelMinGapMs;
        private System.Windows.Forms.NumericUpDown numericUpDownSubtitleLineMaximumLength;
        private System.Windows.Forms.CheckBox checkBoxCpsIncludeWhiteSpace;
        private System.Windows.Forms.OpenFileDialog openFileDialogImport;
        private System.Windows.Forms.Label labelDialogStyle;
        private System.Windows.Forms.ComboBox comboBoxDialogStyle;
    }
}