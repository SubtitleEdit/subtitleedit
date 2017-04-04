namespace Nikse.SubtitleEdit.Forms
{
    partial class ImportText
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
            this.buttonOpenText = new System.Windows.Forms.Button();
            this.groupBoxImportText = new System.Windows.Forms.GroupBox();
            this.listViewInputFiles = new System.Windows.Forms.ListView();
            this.columnHeaderFName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.checkBoxMultipleFiles = new System.Windows.Forms.CheckBox();
            this.textBoxText = new System.Windows.Forms.TextBox();
            this.groupBoxImportOptions = new System.Windows.Forms.GroupBox();
            this.checkBoxAutoBreak = new System.Windows.Forms.CheckBox();
            this.checkBoxGenerateTimeCodes = new System.Windows.Forms.CheckBox();
            this.groupBoxTimeCodes = new System.Windows.Forms.GroupBox();
            this.labelGapBetweenSubtitles = new System.Windows.Forms.Label();
            this.numericUpDownGapBetweenLines = new System.Windows.Forms.NumericUpDown();
            this.groupBoxDuration = new System.Windows.Forms.GroupBox();
            this.numericUpDownDurationFixed = new System.Windows.Forms.NumericUpDown();
            this.radioButtonDurationFixed = new System.Windows.Forms.RadioButton();
            this.radioButtonDurationAuto = new System.Windows.Forms.RadioButton();
            this.checkBoxMergeShortLines = new System.Windows.Forms.CheckBox();
            this.buttonRefresh = new System.Windows.Forms.Button();
            this.checkBoxRemoveLinesWithoutLetters = new System.Windows.Forms.CheckBox();
            this.groupBoxSplitting = new System.Windows.Forms.GroupBox();
            this.comboBoxLineBreak = new System.Windows.Forms.ComboBox();
            this.labelLineBreak = new System.Windows.Forms.Label();
            this.radioButtonSplitAtBlankLines = new System.Windows.Forms.RadioButton();
            this.radioButtonAutoSplit = new System.Windows.Forms.RadioButton();
            this.radioButtonLineMode = new System.Windows.Forms.RadioButton();
            this.checkBoxRemoveEmptyLines = new System.Windows.Forms.CheckBox();
            this.groupBoxImportResult = new System.Windows.Forms.GroupBox();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.contextMenuStripListView = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.clearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SubtitleListview1 = new Nikse.SubtitleEdit.Controls.SubtitleListView();
            this.groupBoxImportText.SuspendLayout();
            this.groupBoxImportOptions.SuspendLayout();
            this.groupBoxTimeCodes.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownGapBetweenLines)).BeginInit();
            this.groupBoxDuration.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDurationFixed)).BeginInit();
            this.groupBoxSplitting.SuspendLayout();
            this.groupBoxImportResult.SuspendLayout();
            this.contextMenuStripListView.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonOpenText
            // 
            this.buttonOpenText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOpenText.Location = new System.Drawing.Point(382, 19);
            this.buttonOpenText.Name = "buttonOpenText";
            this.buttonOpenText.Size = new System.Drawing.Size(143, 21);
            this.buttonOpenText.TabIndex = 0;
            this.buttonOpenText.Text = "Open file...";
            this.buttonOpenText.UseVisualStyleBackColor = true;
            this.buttonOpenText.Click += new System.EventHandler(this.ButtonOpenTextClick);
            // 
            // groupBoxImportText
            // 
            this.groupBoxImportText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxImportText.Controls.Add(this.listViewInputFiles);
            this.groupBoxImportText.Controls.Add(this.checkBoxMultipleFiles);
            this.groupBoxImportText.Controls.Add(this.textBoxText);
            this.groupBoxImportText.Controls.Add(this.buttonOpenText);
            this.groupBoxImportText.Location = new System.Drawing.Point(12, 12);
            this.groupBoxImportText.Name = "groupBoxImportText";
            this.groupBoxImportText.Size = new System.Drawing.Size(531, 365);
            this.groupBoxImportText.TabIndex = 0;
            this.groupBoxImportText.TabStop = false;
            this.groupBoxImportText.Text = "Import text";
            // 
            // listViewInputFiles
            // 
            this.listViewInputFiles.AllowDrop = true;
            this.listViewInputFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewInputFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderFName,
            this.columnHeaderSize});
            this.listViewInputFiles.ContextMenuStrip = this.contextMenuStripListView;
            this.listViewInputFiles.FullRowSelect = true;
            this.listViewInputFiles.HideSelection = false;
            this.listViewInputFiles.Location = new System.Drawing.Point(6, 48);
            this.listViewInputFiles.Name = "listViewInputFiles";
            this.listViewInputFiles.Size = new System.Drawing.Size(519, 311);
            this.listViewInputFiles.TabIndex = 6;
            this.listViewInputFiles.UseCompatibleStateImageBehavior = false;
            this.listViewInputFiles.View = System.Windows.Forms.View.Details;
            this.listViewInputFiles.DragDrop += new System.Windows.Forms.DragEventHandler(this.listViewInputFiles_DragDrop);
            this.listViewInputFiles.DragEnter += new System.Windows.Forms.DragEventHandler(this.listViewInputFiles_DragEnter);
            // 
            // columnHeaderFName
            // 
            this.columnHeaderFName.Text = "File name";
            this.columnHeaderFName.Width = 420;
            // 
            // columnHeaderSize
            // 
            this.columnHeaderSize.Text = "Size";
            this.columnHeaderSize.Width = 81;
            // 
            // checkBoxMultipleFiles
            // 
            this.checkBoxMultipleFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxMultipleFiles.AutoSize = true;
            this.checkBoxMultipleFiles.Location = new System.Drawing.Point(178, 22);
            this.checkBoxMultipleFiles.Name = "checkBoxMultipleFiles";
            this.checkBoxMultipleFiles.Size = new System.Drawing.Size(198, 17);
            this.checkBoxMultipleFiles.TabIndex = 5;
            this.checkBoxMultipleFiles.Text = "Multiple files - one file is one subtitle";
            this.checkBoxMultipleFiles.UseVisualStyleBackColor = true;
            this.checkBoxMultipleFiles.CheckedChanged += new System.EventHandler(this.checkBoxMultipleFiles_CheckedChanged);
            // 
            // textBoxText
            // 
            this.textBoxText.AllowDrop = true;
            this.textBoxText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxText.Location = new System.Drawing.Point(6, 48);
            this.textBoxText.MaxLength = 0;
            this.textBoxText.Multiline = true;
            this.textBoxText.Name = "textBoxText";
            this.textBoxText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxText.Size = new System.Drawing.Size(519, 297);
            this.textBoxText.TabIndex = 1;
            this.textBoxText.TextChanged += new System.EventHandler(this.TextBoxTextTextChanged);
            this.textBoxText.DragDrop += new System.Windows.Forms.DragEventHandler(this.TextBoxTextDragDrop);
            this.textBoxText.DragEnter += new System.Windows.Forms.DragEventHandler(this.TextBoxTextDragEnter);
            // 
            // groupBoxImportOptions
            // 
            this.groupBoxImportOptions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxImportOptions.Controls.Add(this.checkBoxAutoBreak);
            this.groupBoxImportOptions.Controls.Add(this.checkBoxGenerateTimeCodes);
            this.groupBoxImportOptions.Controls.Add(this.groupBoxTimeCodes);
            this.groupBoxImportOptions.Controls.Add(this.checkBoxMergeShortLines);
            this.groupBoxImportOptions.Controls.Add(this.buttonRefresh);
            this.groupBoxImportOptions.Controls.Add(this.checkBoxRemoveLinesWithoutLetters);
            this.groupBoxImportOptions.Controls.Add(this.groupBoxSplitting);
            this.groupBoxImportOptions.Controls.Add(this.checkBoxRemoveEmptyLines);
            this.groupBoxImportOptions.Location = new System.Drawing.Point(549, 12);
            this.groupBoxImportOptions.Name = "groupBoxImportOptions";
            this.groupBoxImportOptions.Size = new System.Drawing.Size(357, 365);
            this.groupBoxImportOptions.TabIndex = 1;
            this.groupBoxImportOptions.TabStop = false;
            this.groupBoxImportOptions.Text = "Import options";
            // 
            // checkBoxAutoBreak
            // 
            this.checkBoxAutoBreak.AutoSize = true;
            this.checkBoxAutoBreak.Location = new System.Drawing.Point(19, 186);
            this.checkBoxAutoBreak.Name = "checkBoxAutoBreak";
            this.checkBoxAutoBreak.Size = new System.Drawing.Size(104, 17);
            this.checkBoxAutoBreak.TabIndex = 7;
            this.checkBoxAutoBreak.Text = "Auto-break lines";
            this.checkBoxAutoBreak.UseVisualStyleBackColor = true;
            this.checkBoxAutoBreak.CheckedChanged += new System.EventHandler(this.checkBoxAutoBreak_CheckedChanged);
            // 
            // checkBoxGenerateTimeCodes
            // 
            this.checkBoxGenerateTimeCodes.AutoSize = true;
            this.checkBoxGenerateTimeCodes.Location = new System.Drawing.Point(19, 163);
            this.checkBoxGenerateTimeCodes.Name = "checkBoxGenerateTimeCodes";
            this.checkBoxGenerateTimeCodes.Size = new System.Drawing.Size(125, 17);
            this.checkBoxGenerateTimeCodes.TabIndex = 4;
            this.checkBoxGenerateTimeCodes.Text = "Generate time codes";
            this.checkBoxGenerateTimeCodes.UseVisualStyleBackColor = true;
            this.checkBoxGenerateTimeCodes.CheckedChanged += new System.EventHandler(this.checkBoxGenerateTimeCodes_CheckedChanged);
            // 
            // groupBoxTimeCodes
            // 
            this.groupBoxTimeCodes.Controls.Add(this.labelGapBetweenSubtitles);
            this.groupBoxTimeCodes.Controls.Add(this.numericUpDownGapBetweenLines);
            this.groupBoxTimeCodes.Controls.Add(this.groupBoxDuration);
            this.groupBoxTimeCodes.Enabled = false;
            this.groupBoxTimeCodes.Location = new System.Drawing.Point(6, 206);
            this.groupBoxTimeCodes.Name = "groupBoxTimeCodes";
            this.groupBoxTimeCodes.Size = new System.Drawing.Size(340, 126);
            this.groupBoxTimeCodes.TabIndex = 5;
            this.groupBoxTimeCodes.TabStop = false;
            this.groupBoxTimeCodes.Text = "Time codes";
            // 
            // labelGapBetweenSubtitles
            // 
            this.labelGapBetweenSubtitles.AutoSize = true;
            this.labelGapBetweenSubtitles.Location = new System.Drawing.Point(6, 23);
            this.labelGapBetweenSubtitles.Name = "labelGapBetweenSubtitles";
            this.labelGapBetweenSubtitles.Size = new System.Drawing.Size(180, 13);
            this.labelGapBetweenSubtitles.TabIndex = 0;
            this.labelGapBetweenSubtitles.Text = "Gap between subtitles (milliseconds)";
            // 
            // numericUpDownGapBetweenLines
            // 
            this.numericUpDownGapBetweenLines.Location = new System.Drawing.Point(192, 20);
            this.numericUpDownGapBetweenLines.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numericUpDownGapBetweenLines.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownGapBetweenLines.Name = "numericUpDownGapBetweenLines";
            this.numericUpDownGapBetweenLines.Size = new System.Drawing.Size(64, 21);
            this.numericUpDownGapBetweenLines.TabIndex = 1;
            this.numericUpDownGapBetweenLines.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownGapBetweenLines.ValueChanged += new System.EventHandler(this.NumericUpDownGapBetweenLinesValueChanged);
            // 
            // groupBoxDuration
            // 
            this.groupBoxDuration.Controls.Add(this.numericUpDownDurationFixed);
            this.groupBoxDuration.Controls.Add(this.radioButtonDurationFixed);
            this.groupBoxDuration.Controls.Add(this.radioButtonDurationAuto);
            this.groupBoxDuration.Location = new System.Drawing.Point(9, 47);
            this.groupBoxDuration.Name = "groupBoxDuration";
            this.groupBoxDuration.Size = new System.Drawing.Size(247, 70);
            this.groupBoxDuration.TabIndex = 2;
            this.groupBoxDuration.TabStop = false;
            this.groupBoxDuration.Text = "Duration";
            // 
            // numericUpDownDurationFixed
            // 
            this.numericUpDownDurationFixed.Location = new System.Drawing.Point(111, 42);
            this.numericUpDownDurationFixed.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numericUpDownDurationFixed.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDownDurationFixed.Name = "numericUpDownDurationFixed";
            this.numericUpDownDurationFixed.Size = new System.Drawing.Size(64, 21);
            this.numericUpDownDurationFixed.TabIndex = 2;
            this.numericUpDownDurationFixed.Value = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            this.numericUpDownDurationFixed.ValueChanged += new System.EventHandler(this.NumericUpDownDurationFixedValueChanged);
            // 
            // radioButtonDurationFixed
            // 
            this.radioButtonDurationFixed.AutoSize = true;
            this.radioButtonDurationFixed.Location = new System.Drawing.Point(16, 42);
            this.radioButtonDurationFixed.Name = "radioButtonDurationFixed";
            this.radioButtonDurationFixed.Size = new System.Drawing.Size(51, 17);
            this.radioButtonDurationFixed.TabIndex = 1;
            this.radioButtonDurationFixed.Text = "Fixed";
            this.radioButtonDurationFixed.UseVisualStyleBackColor = true;
            this.radioButtonDurationFixed.CheckedChanged += new System.EventHandler(this.RadioButtonDurationFixedCheckedChanged);
            // 
            // radioButtonDurationAuto
            // 
            this.radioButtonDurationAuto.AutoSize = true;
            this.radioButtonDurationAuto.Checked = true;
            this.radioButtonDurationAuto.Location = new System.Drawing.Point(16, 19);
            this.radioButtonDurationAuto.Name = "radioButtonDurationAuto";
            this.radioButtonDurationAuto.Size = new System.Drawing.Size(48, 17);
            this.radioButtonDurationAuto.TabIndex = 0;
            this.radioButtonDurationAuto.TabStop = true;
            this.radioButtonDurationAuto.Text = "Auto";
            this.radioButtonDurationAuto.UseVisualStyleBackColor = true;
            this.radioButtonDurationAuto.CheckedChanged += new System.EventHandler(this.RadioButtonDurationAutoCheckedChanged);
            // 
            // checkBoxMergeShortLines
            // 
            this.checkBoxMergeShortLines.AutoSize = true;
            this.checkBoxMergeShortLines.Checked = true;
            this.checkBoxMergeShortLines.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxMergeShortLines.Location = new System.Drawing.Point(19, 94);
            this.checkBoxMergeShortLines.Name = "checkBoxMergeShortLines";
            this.checkBoxMergeShortLines.Size = new System.Drawing.Size(193, 17);
            this.checkBoxMergeShortLines.TabIndex = 1;
            this.checkBoxMergeShortLines.Text = "Merge short lines with continuation";
            this.checkBoxMergeShortLines.UseVisualStyleBackColor = true;
            this.checkBoxMergeShortLines.CheckedChanged += new System.EventHandler(this.CheckBoxMergeShortLinesCheckedChanged);
            // 
            // buttonRefresh
            // 
            this.buttonRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRefresh.Location = new System.Drawing.Point(22, 338);
            this.buttonRefresh.Name = "buttonRefresh";
            this.buttonRefresh.Size = new System.Drawing.Size(102, 21);
            this.buttonRefresh.TabIndex = 6;
            this.buttonRefresh.Text = "Refresh";
            this.buttonRefresh.UseVisualStyleBackColor = true;
            this.buttonRefresh.Click += new System.EventHandler(this.ButtonRefreshClick);
            // 
            // checkBoxRemoveLinesWithoutLetters
            // 
            this.checkBoxRemoveLinesWithoutLetters.AutoSize = true;
            this.checkBoxRemoveLinesWithoutLetters.Checked = true;
            this.checkBoxRemoveLinesWithoutLetters.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxRemoveLinesWithoutLetters.Location = new System.Drawing.Point(19, 140);
            this.checkBoxRemoveLinesWithoutLetters.Name = "checkBoxRemoveLinesWithoutLetters";
            this.checkBoxRemoveLinesWithoutLetters.Size = new System.Drawing.Size(162, 17);
            this.checkBoxRemoveLinesWithoutLetters.TabIndex = 3;
            this.checkBoxRemoveLinesWithoutLetters.Text = "Remove lines without letters";
            this.checkBoxRemoveLinesWithoutLetters.UseVisualStyleBackColor = true;
            this.checkBoxRemoveLinesWithoutLetters.CheckedChanged += new System.EventHandler(this.CheckBoxRemoveLinesWithoutLettersOrNumbersCheckedChanged);
            // 
            // groupBoxSplitting
            // 
            this.groupBoxSplitting.Controls.Add(this.comboBoxLineBreak);
            this.groupBoxSplitting.Controls.Add(this.labelLineBreak);
            this.groupBoxSplitting.Controls.Add(this.radioButtonSplitAtBlankLines);
            this.groupBoxSplitting.Controls.Add(this.radioButtonAutoSplit);
            this.groupBoxSplitting.Controls.Add(this.radioButtonLineMode);
            this.groupBoxSplitting.Location = new System.Drawing.Point(6, 17);
            this.groupBoxSplitting.Name = "groupBoxSplitting";
            this.groupBoxSplitting.Size = new System.Drawing.Size(340, 68);
            this.groupBoxSplitting.TabIndex = 0;
            this.groupBoxSplitting.TabStop = false;
            this.groupBoxSplitting.Text = "Splitting";
            // 
            // comboBoxLineBreak
            // 
            this.comboBoxLineBreak.FormattingEnabled = true;
            this.comboBoxLineBreak.Items.AddRange(new object[] {
            "|",
            "\\N;\\n",
            "|;\\N;\\n;//;<br>;<br />;<br/>"});
            this.comboBoxLineBreak.Location = new System.Drawing.Point(218, 41);
            this.comboBoxLineBreak.Name = "comboBoxLineBreak";
            this.comboBoxLineBreak.Size = new System.Drawing.Size(116, 21);
            this.comboBoxLineBreak.TabIndex = 5;
            this.comboBoxLineBreak.TextChanged += new System.EventHandler(this.comboBoxLineBreak_TextChanged);
            // 
            // labelLineBreak
            // 
            this.labelLineBreak.AutoSize = true;
            this.labelLineBreak.Location = new System.Drawing.Point(156, 44);
            this.labelLineBreak.Name = "labelLineBreak";
            this.labelLineBreak.Size = new System.Drawing.Size(56, 13);
            this.labelLineBreak.TabIndex = 4;
            this.labelLineBreak.Text = "Line break";
            // 
            // radioButtonSplitAtBlankLines
            // 
            this.radioButtonSplitAtBlankLines.AutoSize = true;
            this.radioButtonSplitAtBlankLines.Location = new System.Drawing.Point(159, 19);
            this.radioButtonSplitAtBlankLines.Name = "radioButtonSplitAtBlankLines";
            this.radioButtonSplitAtBlankLines.Size = new System.Drawing.Size(110, 17);
            this.radioButtonSplitAtBlankLines.TabIndex = 2;
            this.radioButtonSplitAtBlankLines.Text = "Split at blank lines";
            this.radioButtonSplitAtBlankLines.UseVisualStyleBackColor = true;
            this.radioButtonSplitAtBlankLines.CheckedChanged += new System.EventHandler(this.radioButtonSplitAtBlankLines_CheckedChanged);
            // 
            // radioButtonAutoSplit
            // 
            this.radioButtonAutoSplit.AutoSize = true;
            this.radioButtonAutoSplit.Checked = true;
            this.radioButtonAutoSplit.Location = new System.Drawing.Point(14, 19);
            this.radioButtonAutoSplit.Name = "radioButtonAutoSplit";
            this.radioButtonAutoSplit.Size = new System.Drawing.Size(93, 17);
            this.radioButtonAutoSplit.TabIndex = 0;
            this.radioButtonAutoSplit.TabStop = true;
            this.radioButtonAutoSplit.Text = "Auto split text";
            this.radioButtonAutoSplit.UseVisualStyleBackColor = true;
            this.radioButtonAutoSplit.CheckedChanged += new System.EventHandler(this.RadioButtonAutoSplitCheckedChanged);
            // 
            // radioButtonLineMode
            // 
            this.radioButtonLineMode.AutoSize = true;
            this.radioButtonLineMode.Location = new System.Drawing.Point(14, 42);
            this.radioButtonLineMode.Name = "radioButtonLineMode";
            this.radioButtonLineMode.Size = new System.Drawing.Size(133, 17);
            this.radioButtonLineMode.TabIndex = 1;
            this.radioButtonLineMode.Text = "One line is one subtitle";
            this.radioButtonLineMode.UseVisualStyleBackColor = true;
            this.radioButtonLineMode.CheckedChanged += new System.EventHandler(this.RadioButtonLineModeCheckedChanged);
            // 
            // checkBoxRemoveEmptyLines
            // 
            this.checkBoxRemoveEmptyLines.AutoSize = true;
            this.checkBoxRemoveEmptyLines.Checked = true;
            this.checkBoxRemoveEmptyLines.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxRemoveEmptyLines.Location = new System.Drawing.Point(19, 117);
            this.checkBoxRemoveEmptyLines.Name = "checkBoxRemoveEmptyLines";
            this.checkBoxRemoveEmptyLines.Size = new System.Drawing.Size(122, 17);
            this.checkBoxRemoveEmptyLines.TabIndex = 2;
            this.checkBoxRemoveEmptyLines.Text = "Remove empty lines";
            this.checkBoxRemoveEmptyLines.UseVisualStyleBackColor = true;
            this.checkBoxRemoveEmptyLines.CheckedChanged += new System.EventHandler(this.CheckBoxRemoveEmptyLinesCheckedChanged);
            // 
            // groupBoxImportResult
            // 
            this.groupBoxImportResult.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxImportResult.Controls.Add(this.SubtitleListview1);
            this.groupBoxImportResult.Location = new System.Drawing.Point(12, 383);
            this.groupBoxImportResult.Name = "groupBoxImportResult";
            this.groupBoxImportResult.Size = new System.Drawing.Size(894, 245);
            this.groupBoxImportResult.TabIndex = 2;
            this.groupBoxImportResult.TabStop = false;
            this.groupBoxImportResult.Text = "Preview";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(825, 636);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 21);
            this.buttonCancel.TabIndex = 4;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.ButtonCancelClick);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(744, 636);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 21);
            this.buttonOK.TabIndex = 3;
            this.buttonOK.Text = "&Next >";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.ButtonOkClick);
            // 
            // contextMenuStripListView
            // 
            this.contextMenuStripListView.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.clearToolStripMenuItem});
            this.contextMenuStripListView.Name = "contextMenuStripListView";
            this.contextMenuStripListView.Size = new System.Drawing.Size(102, 26);
            // 
            // clearToolStripMenuItem
            // 
            this.clearToolStripMenuItem.Name = "clearToolStripMenuItem";
            this.clearToolStripMenuItem.Size = new System.Drawing.Size(101, 22);
            this.clearToolStripMenuItem.Text = "Clear";
            this.clearToolStripMenuItem.Click += new System.EventHandler(this.clearToolStripMenuItem_Click);
            // 
            // SubtitleListview1
            // 
            this.SubtitleListview1.AllowColumnReorder = true;
            this.SubtitleListview1.AllowDrop = true;
            this.SubtitleListview1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SubtitleListview1.FirstVisibleIndex = -1;
            this.SubtitleListview1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SubtitleListview1.FullRowSelect = true;
            this.SubtitleListview1.GridLines = true;
            this.SubtitleListview1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.SubtitleListview1.HideSelection = false;
            this.SubtitleListview1.Location = new System.Drawing.Point(6, 19);
            this.SubtitleListview1.MultiSelect = false;
            this.SubtitleListview1.Name = "SubtitleListview1";
            this.SubtitleListview1.OwnerDraw = true;
            this.SubtitleListview1.Size = new System.Drawing.Size(882, 204);
            this.SubtitleListview1.SubtitleFontBold = false;
            this.SubtitleListview1.SubtitleFontName = "Tahoma";
            this.SubtitleListview1.SubtitleFontSize = 8;
            this.SubtitleListview1.TabIndex = 0;
            this.SubtitleListview1.UseCompatibleStateImageBehavior = false;
            this.SubtitleListview1.UseSyntaxColoring = true;
            this.SubtitleListview1.View = System.Windows.Forms.View.Details;
            // 
            // ImportText
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(918, 673);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.groupBoxImportResult);
            this.Controls.Add(this.groupBoxImportOptions);
            this.Controls.Add(this.groupBoxImportText);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(810, 648);
            this.Name = "ImportText";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Import text";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ImportText_FormClosing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ImportTextKeyDown);
            this.groupBoxImportText.ResumeLayout(false);
            this.groupBoxImportText.PerformLayout();
            this.groupBoxImportOptions.ResumeLayout(false);
            this.groupBoxImportOptions.PerformLayout();
            this.groupBoxTimeCodes.ResumeLayout(false);
            this.groupBoxTimeCodes.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownGapBetweenLines)).EndInit();
            this.groupBoxDuration.ResumeLayout(false);
            this.groupBoxDuration.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDurationFixed)).EndInit();
            this.groupBoxSplitting.ResumeLayout(false);
            this.groupBoxSplitting.PerformLayout();
            this.groupBoxImportResult.ResumeLayout(false);
            this.contextMenuStripListView.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonOpenText;
        private Controls.SubtitleListView SubtitleListview1;
        private System.Windows.Forms.GroupBox groupBoxImportText;
        private System.Windows.Forms.TextBox textBoxText;
        private System.Windows.Forms.GroupBox groupBoxImportOptions;
        private System.Windows.Forms.GroupBox groupBoxImportResult;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.GroupBox groupBoxSplitting;
        private System.Windows.Forms.RadioButton radioButtonAutoSplit;
        private System.Windows.Forms.RadioButton radioButtonLineMode;
        private System.Windows.Forms.CheckBox checkBoxRemoveEmptyLines;
        private System.Windows.Forms.CheckBox checkBoxRemoveLinesWithoutLetters;
        private System.Windows.Forms.NumericUpDown numericUpDownGapBetweenLines;
        private System.Windows.Forms.Label labelGapBetweenSubtitles;
        private System.Windows.Forms.Button buttonRefresh;
        private System.Windows.Forms.GroupBox groupBoxDuration;
        private System.Windows.Forms.NumericUpDown numericUpDownDurationFixed;
        private System.Windows.Forms.RadioButton radioButtonDurationFixed;
        private System.Windows.Forms.RadioButton radioButtonDurationAuto;
        private System.Windows.Forms.CheckBox checkBoxMergeShortLines;
        private System.Windows.Forms.RadioButton radioButtonSplitAtBlankLines;
        private System.Windows.Forms.CheckBox checkBoxGenerateTimeCodes;
        private System.Windows.Forms.GroupBox groupBoxTimeCodes;
        private System.Windows.Forms.Label labelLineBreak;
        private System.Windows.Forms.CheckBox checkBoxMultipleFiles;
        private System.Windows.Forms.ListView listViewInputFiles;
        private System.Windows.Forms.ColumnHeader columnHeaderFName;
        private System.Windows.Forms.ColumnHeader columnHeaderSize;
        private System.Windows.Forms.CheckBox checkBoxAutoBreak;
        private System.Windows.Forms.ComboBox comboBoxLineBreak;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripListView;
        private System.Windows.Forms.ToolStripMenuItem clearToolStripMenuItem;
    }
}