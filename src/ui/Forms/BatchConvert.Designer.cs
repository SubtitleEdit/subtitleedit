using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class BatchConvert
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
            Nikse.SubtitleEdit.Core.Common.TimeCode timeCode1 = new Nikse.SubtitleEdit.Core.Common.TimeCode();
            this.buttonConvert = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.groupBoxConvertOptions = new System.Windows.Forms.GroupBox();
            this.buttonConvertOptionsSettings = new System.Windows.Forms.Button();
            this.listViewConvertOptions = new System.Windows.Forms.ListView();
            this.ActionCheckBox = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Action = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStripOptions = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemSelectAll = new System.Windows.Forms.ToolStripMenuItem();
            this.inverseSelectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBoxMergeShortLines = new System.Windows.Forms.GroupBox();
            this.checkBoxOnlyContinuationLines = new System.Windows.Forms.CheckBox();
            this.numericUpDownMaxCharacters = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownMaxMillisecondsBetweenLines = new System.Windows.Forms.NumericUpDown();
            this.labelMaxMillisecondsBetweenLines = new System.Windows.Forms.Label();
            this.labelMaxCharacters = new System.Windows.Forms.Label();
            this.groupBoxAdjustDuration = new System.Windows.Forms.GroupBox();
            this.comboBoxAdjustDurationVia = new System.Windows.Forms.ComboBox();
            this.labelAdjustDurationVia = new System.Windows.Forms.Label();
            this.panelAdjustDurationFixed = new System.Windows.Forms.Panel();
            this.numericUpDownFixedMilliseconds = new System.Windows.Forms.NumericUpDown();
            this.labelMillisecondsFixed = new System.Windows.Forms.Label();
            this.panelAdjustDurationAddPercent = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.numericUpDownAdjustViaPercent = new System.Windows.Forms.NumericUpDown();
            this.labelAdjustViaPercent = new System.Windows.Forms.Label();
            this.panelAdjustDurationAddSeconds = new System.Windows.Forms.Panel();
            this.numericUpDownSeconds = new System.Windows.Forms.NumericUpDown();
            this.labelAddSeconds = new System.Windows.Forms.Label();
            this.panelAdjustDurationRecalc = new System.Windows.Forms.Panel();
            this.checkBoxExtendOnly = new System.Windows.Forms.CheckBox();
            this.numericUpDownOptimalCharsSec = new System.Windows.Forms.NumericUpDown();
            this.labelOptimalCharsSec = new System.Windows.Forms.Label();
            this.numericUpDownMaxCharsSec = new System.Windows.Forms.NumericUpDown();
            this.labelMaxCharsPerSecond = new System.Windows.Forms.Label();
            this.groupBoxDeleteLines = new System.Windows.Forms.GroupBox();
            this.textBoxDeleteContains = new System.Windows.Forms.TextBox();
            this.labelDeleteLinesContaining = new System.Windows.Forms.Label();
            this.numericUpDownDeleteLast = new System.Windows.Forms.NumericUpDown();
            this.labelDeleteLastLines = new System.Windows.Forms.Label();
            this.numericUpDownDeleteFirst = new System.Windows.Forms.NumericUpDown();
            this.labelDeleteFirstLines = new System.Windows.Forms.Label();
            this.groupBoxRemoveStyle = new System.Windows.Forms.GroupBox();
            this.textBoxRemoveStyle = new System.Windows.Forms.TextBox();
            this.labelStyleName = new System.Windows.Forms.Label();
            this.groupBoxOffsetTimeCodes = new System.Windows.Forms.GroupBox();
            this.radioButtonShowLater = new System.Windows.Forms.RadioButton();
            this.radioButtonShowEarlier = new System.Windows.Forms.RadioButton();
            this.timeUpDownAdjust = new Nikse.SubtitleEdit.Controls.TimeUpDown();
            this.labelHourMinSecMilliSecond = new System.Windows.Forms.Label();
            this.groupBoxChangeFrameRate = new System.Windows.Forms.GroupBox();
            this.buttonSwapFrameRate = new System.Windows.Forms.Button();
            this.comboBoxFrameRateTo = new System.Windows.Forms.ComboBox();
            this.labelToFrameRate = new System.Windows.Forms.Label();
            this.comboBoxFrameRateFrom = new System.Windows.Forms.ComboBox();
            this.labelFromFrameRate = new System.Windows.Forms.Label();
            this.groupBoxFixRtl = new System.Windows.Forms.GroupBox();
            this.radioButtonReverseStartEnd = new System.Windows.Forms.RadioButton();
            this.radioButtonRemoveUnicode = new System.Windows.Forms.RadioButton();
            this.radioButtonAddUnicode = new System.Windows.Forms.RadioButton();
            this.groupBoxSpeed = new System.Windows.Forms.GroupBox();
            this.radioButtonToDropFrame = new System.Windows.Forms.RadioButton();
            this.radioButtonSpeedFromDropFrame = new System.Windows.Forms.RadioButton();
            this.radioButtonSpeedCustom = new System.Windows.Forms.RadioButton();
            this.numericUpDownPercent = new System.Windows.Forms.NumericUpDown();
            this.labelPercent = new System.Windows.Forms.Label();
            this.groupBoxChangeCasing = new System.Windows.Forms.GroupBox();
            this.radioButtonFixOnlyNames = new System.Windows.Forms.RadioButton();
            this.radioButtonLowercase = new System.Windows.Forms.RadioButton();
            this.radioButtonUppercase = new System.Windows.Forms.RadioButton();
            this.radioButtonNormal = new System.Windows.Forms.RadioButton();
            this.groupBoxOutput = new System.Windows.Forms.GroupBox();
            this.buttonBrowseEncoding = new System.Windows.Forms.Button();
            this.radioButtonSaveInOutputFolder = new System.Windows.Forms.RadioButton();
            this.buttonTransportStreamSettings = new System.Windows.Forms.Button();
            this.linkLabelOpenOutputFolder = new System.Windows.Forms.LinkLabel();
            this.checkBoxUseStyleFromSource = new System.Windows.Forms.CheckBox();
            this.checkBoxOverwrite = new System.Windows.Forms.CheckBox();
            this.buttonStyles = new System.Windows.Forms.Button();
            this.buttonChooseFolder = new System.Windows.Forms.Button();
            this.comboBoxSubtitleFormats = new System.Windows.Forms.ComboBox();
            this.textBoxOutputFolder = new System.Windows.Forms.TextBox();
            this.labelEncoding = new System.Windows.Forms.Label();
            this.radioButtonSaveInSourceFolder = new System.Windows.Forms.RadioButton();
            this.comboBoxEncoding = new System.Windows.Forms.ComboBox();
            this.labelOutputFormat = new System.Windows.Forms.Label();
            this.groupBoxInput = new System.Windows.Forms.GroupBox();
            this.labelNumberOfFiles = new System.Windows.Forms.Label();
            this.textBoxFilter = new System.Windows.Forms.TextBox();
            this.labelFilter = new System.Windows.Forms.Label();
            this.comboBoxFilter = new System.Windows.Forms.ComboBox();
            this.checkBoxScanFolderRecursive = new System.Windows.Forms.CheckBox();
            this.buttonSearchFolder = new System.Windows.Forms.Button();
            this.buttonInputBrowse = new System.Windows.Forms.Button();
            this.labelChooseInputFiles = new System.Windows.Forms.Label();
            this.listViewInputFiles = new System.Windows.Forms.ListView();
            this.columnHeaderFName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderFormat = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStripFiles = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openContainingFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.convertMkvSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.labelStatus = new System.Windows.Forms.Label();
            this.groupBoxConvertOptions.SuspendLayout();
            this.contextMenuStripOptions.SuspendLayout();
            this.groupBoxMergeShortLines.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxCharacters)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxMillisecondsBetweenLines)).BeginInit();
            this.groupBoxAdjustDuration.SuspendLayout();
            this.panelAdjustDurationFixed.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFixedMilliseconds)).BeginInit();
            this.panelAdjustDurationAddPercent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownAdjustViaPercent)).BeginInit();
            this.panelAdjustDurationAddSeconds.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSeconds)).BeginInit();
            this.panelAdjustDurationRecalc.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownOptimalCharsSec)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxCharsSec)).BeginInit();
            this.groupBoxDeleteLines.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDeleteLast)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDeleteFirst)).BeginInit();
            this.groupBoxRemoveStyle.SuspendLayout();
            this.groupBoxOffsetTimeCodes.SuspendLayout();
            this.groupBoxChangeFrameRate.SuspendLayout();
            this.groupBoxFixRtl.SuspendLayout();
            this.groupBoxSpeed.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPercent)).BeginInit();
            this.groupBoxChangeCasing.SuspendLayout();
            this.groupBoxOutput.SuspendLayout();
            this.groupBoxInput.SuspendLayout();
            this.contextMenuStripFiles.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonConvert
            // 
            this.buttonConvert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonConvert.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonConvert.Location = new System.Drawing.Point(844, 618);
            this.buttonConvert.Name = "buttonConvert";
            this.buttonConvert.Size = new System.Drawing.Size(98, 23);
            this.buttonConvert.TabIndex = 2;
            this.buttonConvert.Text = "&Convert";
            this.buttonConvert.UseVisualStyleBackColor = true;
            this.buttonConvert.Click += new System.EventHandler(this.buttonConvert_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(948, 618);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "&Done";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // groupBoxConvertOptions
            // 
            this.groupBoxConvertOptions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxConvertOptions.Controls.Add(this.buttonConvertOptionsSettings);
            this.groupBoxConvertOptions.Controls.Add(this.listViewConvertOptions);
            this.groupBoxConvertOptions.Controls.Add(this.groupBoxDeleteLines);
            this.groupBoxConvertOptions.Controls.Add(this.groupBoxRemoveStyle);
            this.groupBoxConvertOptions.Controls.Add(this.groupBoxOffsetTimeCodes);
            this.groupBoxConvertOptions.Controls.Add(this.groupBoxChangeFrameRate);
            this.groupBoxConvertOptions.Controls.Add(this.groupBoxFixRtl);
            this.groupBoxConvertOptions.Controls.Add(this.groupBoxSpeed);
            this.groupBoxConvertOptions.Controls.Add(this.groupBoxChangeCasing);
            this.groupBoxConvertOptions.Controls.Add(this.groupBoxMergeShortLines);
            this.groupBoxConvertOptions.Controls.Add(this.groupBoxAdjustDuration);
            this.groupBoxConvertOptions.Location = new System.Drawing.Point(422, 19);
            this.groupBoxConvertOptions.Name = "groupBoxConvertOptions";
            this.groupBoxConvertOptions.Size = new System.Drawing.Size(583, 275);
            this.groupBoxConvertOptions.TabIndex = 11;
            this.groupBoxConvertOptions.TabStop = false;
            this.groupBoxConvertOptions.Text = "Convert options";
            // 
            // buttonConvertOptionsSettings
            // 
            this.buttonConvertOptionsSettings.Location = new System.Drawing.Point(305, 144);
            this.buttonConvertOptionsSettings.Name = "buttonConvertOptionsSettings";
            this.buttonConvertOptionsSettings.Size = new System.Drawing.Size(116, 23);
            this.buttonConvertOptionsSettings.TabIndex = 302;
            this.buttonConvertOptionsSettings.Text = "Settings...";
            this.buttonConvertOptionsSettings.UseVisualStyleBackColor = true;
            this.buttonConvertOptionsSettings.Visible = false;
            this.buttonConvertOptionsSettings.Click += new System.EventHandler(this.ButtonOptionConvertSettings);
            // 
            // listViewConvertOptions
            // 
            this.listViewConvertOptions.CheckBoxes = true;
            this.listViewConvertOptions.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ActionCheckBox,
            this.Action});
            this.listViewConvertOptions.ContextMenuStrip = this.contextMenuStripOptions;
            this.listViewConvertOptions.FullRowSelect = true;
            this.listViewConvertOptions.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listViewConvertOptions.HideSelection = false;
            this.listViewConvertOptions.Location = new System.Drawing.Point(6, 17);
            this.listViewConvertOptions.MultiSelect = false;
            this.listViewConvertOptions.Name = "listViewConvertOptions";
            this.listViewConvertOptions.Size = new System.Drawing.Size(293, 252);
            this.listViewConvertOptions.TabIndex = 301;
            this.listViewConvertOptions.UseCompatibleStateImageBehavior = false;
            this.listViewConvertOptions.View = System.Windows.Forms.View.Details;
            this.listViewConvertOptions.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.listViewConvertOptions_ItemChecked);
            this.listViewConvertOptions.SelectedIndexChanged += new System.EventHandler(this.listViewConvertOptions_SelectedIndexChanged);
            // 
            // ActionCheckBox
            // 
            this.ActionCheckBox.Width = 30;
            // 
            // Action
            // 
            this.Action.Width = 400;
            // 
            // contextMenuStripOptions
            // 
            this.contextMenuStripOptions.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemSelectAll,
            this.inverseSelectionToolStripMenuItem});
            this.contextMenuStripOptions.Name = "contextMenuStripOptions";
            this.contextMenuStripOptions.Size = new System.Drawing.Size(162, 48);
            // 
            // toolStripMenuItemSelectAll
            // 
            this.toolStripMenuItemSelectAll.Name = "toolStripMenuItemSelectAll";
            this.toolStripMenuItemSelectAll.Size = new System.Drawing.Size(161, 22);
            this.toolStripMenuItemSelectAll.Text = "Select all";
            this.toolStripMenuItemSelectAll.Click += new System.EventHandler(this.toolStripMenuItemSelectAll_Click);
            // 
            // inverseSelectionToolStripMenuItem
            // 
            this.inverseSelectionToolStripMenuItem.Name = "inverseSelectionToolStripMenuItem";
            this.inverseSelectionToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
            this.inverseSelectionToolStripMenuItem.Text = "Inverse selection";
            this.inverseSelectionToolStripMenuItem.Click += new System.EventHandler(this.inverseSelectionToolStripMenuItem_Click);
            // 
            // groupBoxMergeShortLines
            // 
            this.groupBoxMergeShortLines.Controls.Add(this.checkBoxOnlyContinuationLines);
            this.groupBoxMergeShortLines.Controls.Add(this.numericUpDownMaxCharacters);
            this.groupBoxMergeShortLines.Controls.Add(this.numericUpDownMaxMillisecondsBetweenLines);
            this.groupBoxMergeShortLines.Controls.Add(this.labelMaxMillisecondsBetweenLines);
            this.groupBoxMergeShortLines.Controls.Add(this.labelMaxCharacters);
            this.groupBoxMergeShortLines.Location = new System.Drawing.Point(308, 23);
            this.groupBoxMergeShortLines.Name = "groupBoxMergeShortLines";
            this.groupBoxMergeShortLines.Size = new System.Drawing.Size(268, 149);
            this.groupBoxMergeShortLines.TabIndex = 304;
            this.groupBoxMergeShortLines.TabStop = false;
            this.groupBoxMergeShortLines.Text = "Merge short lines";
            this.groupBoxMergeShortLines.Visible = false;
            // 
            // checkBoxOnlyContinuationLines
            // 
            this.checkBoxOnlyContinuationLines.AutoSize = true;
            this.checkBoxOnlyContinuationLines.Checked = true;
            this.checkBoxOnlyContinuationLines.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxOnlyContinuationLines.Location = new System.Drawing.Point(16, 127);
            this.checkBoxOnlyContinuationLines.Name = "checkBoxOnlyContinuationLines";
            this.checkBoxOnlyContinuationLines.Size = new System.Drawing.Size(164, 17);
            this.checkBoxOnlyContinuationLines.TabIndex = 42;
            this.checkBoxOnlyContinuationLines.Text = "Only merge continuation lines";
            this.checkBoxOnlyContinuationLines.UseVisualStyleBackColor = true;
            // 
            // numericUpDownMaxCharacters
            // 
            this.numericUpDownMaxCharacters.Location = new System.Drawing.Point(15, 41);
            this.numericUpDownMaxCharacters.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.numericUpDownMaxCharacters.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDownMaxCharacters.Name = "numericUpDownMaxCharacters";
            this.numericUpDownMaxCharacters.Size = new System.Drawing.Size(64, 20);
            this.numericUpDownMaxCharacters.TabIndex = 38;
            this.numericUpDownMaxCharacters.Value = new decimal(new int[] {
            65,
            0,
            0,
            0});
            // 
            // numericUpDownMaxMillisecondsBetweenLines
            // 
            this.numericUpDownMaxMillisecondsBetweenLines.Location = new System.Drawing.Point(15, 90);
            this.numericUpDownMaxMillisecondsBetweenLines.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDownMaxMillisecondsBetweenLines.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownMaxMillisecondsBetweenLines.Name = "numericUpDownMaxMillisecondsBetweenLines";
            this.numericUpDownMaxMillisecondsBetweenLines.Size = new System.Drawing.Size(64, 20);
            this.numericUpDownMaxMillisecondsBetweenLines.TabIndex = 39;
            this.numericUpDownMaxMillisecondsBetweenLines.Value = new decimal(new int[] {
            250,
            0,
            0,
            0});
            // 
            // labelMaxMillisecondsBetweenLines
            // 
            this.labelMaxMillisecondsBetweenLines.AutoSize = true;
            this.labelMaxMillisecondsBetweenLines.Location = new System.Drawing.Point(12, 73);
            this.labelMaxMillisecondsBetweenLines.Name = "labelMaxMillisecondsBetweenLines";
            this.labelMaxMillisecondsBetweenLines.Size = new System.Drawing.Size(178, 13);
            this.labelMaxMillisecondsBetweenLines.TabIndex = 41;
            this.labelMaxMillisecondsBetweenLines.Text = "Maximum milliseconds between lines";
            // 
            // labelMaxCharacters
            // 
            this.labelMaxCharacters.AutoSize = true;
            this.labelMaxCharacters.Location = new System.Drawing.Point(12, 23);
            this.labelMaxCharacters.Name = "labelMaxCharacters";
            this.labelMaxCharacters.Size = new System.Drawing.Size(187, 13);
            this.labelMaxCharacters.TabIndex = 40;
            this.labelMaxCharacters.Text = "Maximum characters in one paragraph";
            // 
            // groupBoxAdjustDuration
            // 
            this.groupBoxAdjustDuration.Controls.Add(this.comboBoxAdjustDurationVia);
            this.groupBoxAdjustDuration.Controls.Add(this.labelAdjustDurationVia);
            this.groupBoxAdjustDuration.Controls.Add(this.panelAdjustDurationFixed);
            this.groupBoxAdjustDuration.Controls.Add(this.panelAdjustDurationAddPercent);
            this.groupBoxAdjustDuration.Controls.Add(this.panelAdjustDurationAddSeconds);
            this.groupBoxAdjustDuration.Controls.Add(this.panelAdjustDurationRecalc);
            this.groupBoxAdjustDuration.Location = new System.Drawing.Point(305, 16);
            this.groupBoxAdjustDuration.Name = "groupBoxAdjustDuration";
            this.groupBoxAdjustDuration.Size = new System.Drawing.Size(271, 251);
            this.groupBoxAdjustDuration.TabIndex = 308;
            this.groupBoxAdjustDuration.TabStop = false;
            this.groupBoxAdjustDuration.Text = "Adjust duration";
            this.groupBoxAdjustDuration.Visible = false;
            // 
            // comboBoxAdjustDurationVia
            // 
            this.comboBoxAdjustDurationVia.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxAdjustDurationVia.FormattingEnabled = true;
            this.comboBoxAdjustDurationVia.Location = new System.Drawing.Point(65, 19);
            this.comboBoxAdjustDurationVia.Name = "comboBoxAdjustDurationVia";
            this.comboBoxAdjustDurationVia.Size = new System.Drawing.Size(121, 21);
            this.comboBoxAdjustDurationVia.TabIndex = 8;
            this.comboBoxAdjustDurationVia.SelectedIndexChanged += new System.EventHandler(this.comboBoxAdjustDurationVia_SelectedIndexChanged);
            // 
            // labelAdjustDurationVia
            // 
            this.labelAdjustDurationVia.AutoSize = true;
            this.labelAdjustDurationVia.Location = new System.Drawing.Point(8, 22);
            this.labelAdjustDurationVia.Name = "labelAdjustDurationVia";
            this.labelAdjustDurationVia.Size = new System.Drawing.Size(51, 13);
            this.labelAdjustDurationVia.TabIndex = 7;
            this.labelAdjustDurationVia.Text = "AdjustVia";
            // 
            // panelAdjustDurationFixed
            // 
            this.panelAdjustDurationFixed.Controls.Add(this.numericUpDownFixedMilliseconds);
            this.panelAdjustDurationFixed.Controls.Add(this.labelMillisecondsFixed);
            this.panelAdjustDurationFixed.Location = new System.Drawing.Point(6, 123);
            this.panelAdjustDurationFixed.Name = "panelAdjustDurationFixed";
            this.panelAdjustDurationFixed.Size = new System.Drawing.Size(257, 57);
            this.panelAdjustDurationFixed.TabIndex = 15;
            // 
            // numericUpDownFixedMilliseconds
            // 
            this.numericUpDownFixedMilliseconds.Location = new System.Drawing.Point(8, 27);
            this.numericUpDownFixedMilliseconds.Maximum = new decimal(new int[] {
            20000,
            0,
            0,
            0});
            this.numericUpDownFixedMilliseconds.Name = "numericUpDownFixedMilliseconds";
            this.numericUpDownFixedMilliseconds.Size = new System.Drawing.Size(80, 20);
            this.numericUpDownFixedMilliseconds.TabIndex = 13;
            this.numericUpDownFixedMilliseconds.Value = new decimal(new int[] {
            3000,
            0,
            0,
            0});
            // 
            // labelMillisecondsFixed
            // 
            this.labelMillisecondsFixed.AutoSize = true;
            this.labelMillisecondsFixed.Location = new System.Drawing.Point(5, 8);
            this.labelMillisecondsFixed.Name = "labelMillisecondsFixed";
            this.labelMillisecondsFixed.Size = new System.Drawing.Size(64, 13);
            this.labelMillisecondsFixed.TabIndex = 14;
            this.labelMillisecondsFixed.Text = "Milliseconds";
            // 
            // panelAdjustDurationAddPercent
            // 
            this.panelAdjustDurationAddPercent.Controls.Add(this.label1);
            this.panelAdjustDurationAddPercent.Controls.Add(this.numericUpDownAdjustViaPercent);
            this.panelAdjustDurationAddPercent.Controls.Add(this.labelAdjustViaPercent);
            this.panelAdjustDurationAddPercent.Location = new System.Drawing.Point(7, 79);
            this.panelAdjustDurationAddPercent.Name = "panelAdjustDurationAddPercent";
            this.panelAdjustDurationAddPercent.Size = new System.Drawing.Size(257, 63);
            this.panelAdjustDurationAddPercent.TabIndex = 14;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(90, 34);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(15, 13);
            this.label1.TabIndex = 25;
            this.label1.Text = "%";
            // 
            // numericUpDownAdjustViaPercent
            // 
            this.numericUpDownAdjustViaPercent.Location = new System.Drawing.Point(8, 27);
            this.numericUpDownAdjustViaPercent.Maximum = new decimal(new int[] {
            150,
            0,
            0,
            0});
            this.numericUpDownAdjustViaPercent.Minimum = new decimal(new int[] {
            75,
            0,
            0,
            0});
            this.numericUpDownAdjustViaPercent.Name = "numericUpDownAdjustViaPercent";
            this.numericUpDownAdjustViaPercent.Size = new System.Drawing.Size(80, 20);
            this.numericUpDownAdjustViaPercent.TabIndex = 24;
            this.numericUpDownAdjustViaPercent.Value = new decimal(new int[] {
            110,
            0,
            0,
            0});
            // 
            // labelAdjustViaPercent
            // 
            this.labelAdjustViaPercent.AutoSize = true;
            this.labelAdjustViaPercent.Location = new System.Drawing.Point(5, 8);
            this.labelAdjustViaPercent.Name = "labelAdjustViaPercent";
            this.labelAdjustViaPercent.Size = new System.Drawing.Size(86, 13);
            this.labelAdjustViaPercent.TabIndex = 23;
            this.labelAdjustViaPercent.Text = "Adjust in percent";
            // 
            // panelAdjustDurationAddSeconds
            // 
            this.panelAdjustDurationAddSeconds.Controls.Add(this.numericUpDownSeconds);
            this.panelAdjustDurationAddSeconds.Controls.Add(this.labelAddSeconds);
            this.panelAdjustDurationAddSeconds.Location = new System.Drawing.Point(8, 47);
            this.panelAdjustDurationAddSeconds.Name = "panelAdjustDurationAddSeconds";
            this.panelAdjustDurationAddSeconds.Size = new System.Drawing.Size(257, 55);
            this.panelAdjustDurationAddSeconds.TabIndex = 13;
            // 
            // numericUpDownSeconds
            // 
            this.numericUpDownSeconds.DecimalPlaces = 3;
            this.numericUpDownSeconds.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.numericUpDownSeconds.Location = new System.Drawing.Point(8, 27);
            this.numericUpDownSeconds.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.numericUpDownSeconds.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            -2147483648});
            this.numericUpDownSeconds.Name = "numericUpDownSeconds";
            this.numericUpDownSeconds.Size = new System.Drawing.Size(80, 20);
            this.numericUpDownSeconds.TabIndex = 12;
            this.numericUpDownSeconds.Value = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            // 
            // labelAddSeconds
            // 
            this.labelAddSeconds.AutoSize = true;
            this.labelAddSeconds.Location = new System.Drawing.Point(5, 8);
            this.labelAddSeconds.Name = "labelAddSeconds";
            this.labelAddSeconds.Size = new System.Drawing.Size(69, 13);
            this.labelAddSeconds.TabIndex = 11;
            this.labelAddSeconds.Text = "Add seconds";
            // 
            // panelAdjustDurationRecalc
            // 
            this.panelAdjustDurationRecalc.Controls.Add(this.checkBoxExtendOnly);
            this.panelAdjustDurationRecalc.Controls.Add(this.numericUpDownOptimalCharsSec);
            this.panelAdjustDurationRecalc.Controls.Add(this.labelOptimalCharsSec);
            this.panelAdjustDurationRecalc.Controls.Add(this.numericUpDownMaxCharsSec);
            this.panelAdjustDurationRecalc.Controls.Add(this.labelMaxCharsPerSecond);
            this.panelAdjustDurationRecalc.Location = new System.Drawing.Point(6, 66);
            this.panelAdjustDurationRecalc.Name = "panelAdjustDurationRecalc";
            this.panelAdjustDurationRecalc.Size = new System.Drawing.Size(257, 170);
            this.panelAdjustDurationRecalc.TabIndex = 16;
            // 
            // checkBoxExtendOnly
            // 
            this.checkBoxExtendOnly.AutoSize = true;
            this.checkBoxExtendOnly.Location = new System.Drawing.Point(8, 122);
            this.checkBoxExtendOnly.Name = "checkBoxExtendOnly";
            this.checkBoxExtendOnly.Size = new System.Drawing.Size(81, 17);
            this.checkBoxExtendOnly.TabIndex = 14;
            this.checkBoxExtendOnly.Text = "Extend only";
            this.checkBoxExtendOnly.UseVisualStyleBackColor = true;
            // 
            // numericUpDownOptimalCharsSec
            // 
            this.numericUpDownOptimalCharsSec.DecimalPlaces = 1;
            this.numericUpDownOptimalCharsSec.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericUpDownOptimalCharsSec.Location = new System.Drawing.Point(8, 85);
            this.numericUpDownOptimalCharsSec.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.numericUpDownOptimalCharsSec.Name = "numericUpDownOptimalCharsSec";
            this.numericUpDownOptimalCharsSec.Size = new System.Drawing.Size(80, 20);
            this.numericUpDownOptimalCharsSec.TabIndex = 13;
            this.numericUpDownOptimalCharsSec.Value = new decimal(new int[] {
            17,
            0,
            0,
            0});
            // 
            // labelOptimalCharsSec
            // 
            this.labelOptimalCharsSec.AutoSize = true;
            this.labelOptimalCharsSec.Location = new System.Drawing.Point(5, 66);
            this.labelOptimalCharsSec.Name = "labelOptimalCharsSec";
            this.labelOptimalCharsSec.Size = new System.Drawing.Size(93, 13);
            this.labelOptimalCharsSec.TabIndex = 15;
            this.labelOptimalCharsSec.Text = "Optimal chars/sec";
            // 
            // numericUpDownMaxCharsSec
            // 
            this.numericUpDownMaxCharsSec.DecimalPlaces = 1;
            this.numericUpDownMaxCharsSec.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericUpDownMaxCharsSec.Location = new System.Drawing.Point(8, 27);
            this.numericUpDownMaxCharsSec.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.numericUpDownMaxCharsSec.Name = "numericUpDownMaxCharsSec";
            this.numericUpDownMaxCharsSec.Size = new System.Drawing.Size(80, 20);
            this.numericUpDownMaxCharsSec.TabIndex = 12;
            this.numericUpDownMaxCharsSec.Value = new decimal(new int[] {
            24,
            0,
            0,
            0});
            // 
            // labelMaxCharsPerSecond
            // 
            this.labelMaxCharsPerSecond.AutoSize = true;
            this.labelMaxCharsPerSecond.Location = new System.Drawing.Point(5, 8);
            this.labelMaxCharsPerSecond.Name = "labelMaxCharsPerSecond";
            this.labelMaxCharsPerSecond.Size = new System.Drawing.Size(81, 13);
            this.labelMaxCharsPerSecond.TabIndex = 11;
            this.labelMaxCharsPerSecond.Text = "Max. chars/sec";
            // 
            // groupBoxDeleteLines
            // 
            this.groupBoxDeleteLines.Controls.Add(this.textBoxDeleteContains);
            this.groupBoxDeleteLines.Controls.Add(this.labelDeleteLinesContaining);
            this.groupBoxDeleteLines.Controls.Add(this.numericUpDownDeleteLast);
            this.groupBoxDeleteLines.Controls.Add(this.labelDeleteLastLines);
            this.groupBoxDeleteLines.Controls.Add(this.numericUpDownDeleteFirst);
            this.groupBoxDeleteLines.Controls.Add(this.labelDeleteFirstLines);
            this.groupBoxDeleteLines.Location = new System.Drawing.Point(305, 94);
            this.groupBoxDeleteLines.Name = "groupBoxDeleteLines";
            this.groupBoxDeleteLines.Size = new System.Drawing.Size(271, 140);
            this.groupBoxDeleteLines.TabIndex = 308;
            this.groupBoxDeleteLines.TabStop = false;
            this.groupBoxDeleteLines.Text = "Delete lines";
            this.groupBoxDeleteLines.Visible = false;
            // 
            // textBoxDeleteContains
            // 
            this.textBoxDeleteContains.Location = new System.Drawing.Point(10, 110);
            this.textBoxDeleteContains.Name = "textBoxDeleteContains";
            this.textBoxDeleteContains.Size = new System.Drawing.Size(237, 20);
            this.textBoxDeleteContains.TabIndex = 5;
            // 
            // labelDeleteLinesContaining
            // 
            this.labelDeleteLinesContaining.AutoSize = true;
            this.labelDeleteLinesContaining.Location = new System.Drawing.Point(9, 89);
            this.labelDeleteLinesContaining.Name = "labelDeleteLinesContaining";
            this.labelDeleteLinesContaining.Size = new System.Drawing.Size(114, 13);
            this.labelDeleteLinesContaining.TabIndex = 4;
            this.labelDeleteLinesContaining.Text = "Delete lines containing";
            // 
            // numericUpDownDeleteLast
            // 
            this.numericUpDownDeleteLast.Location = new System.Drawing.Point(92, 45);
            this.numericUpDownDeleteLast.Name = "numericUpDownDeleteLast";
            this.numericUpDownDeleteLast.Size = new System.Drawing.Size(43, 20);
            this.numericUpDownDeleteLast.TabIndex = 3;
            // 
            // labelDeleteLastLines
            // 
            this.labelDeleteLastLines.AutoSize = true;
            this.labelDeleteLastLines.Location = new System.Drawing.Point(5, 47);
            this.labelDeleteLastLines.Name = "labelDeleteLastLines";
            this.labelDeleteLastLines.Size = new System.Drawing.Size(81, 13);
            this.labelDeleteLastLines.TabIndex = 2;
            this.labelDeleteLastLines.Text = "Delete last lines";
            // 
            // numericUpDownDeleteFirst
            // 
            this.numericUpDownDeleteFirst.Location = new System.Drawing.Point(93, 19);
            this.numericUpDownDeleteFirst.Name = "numericUpDownDeleteFirst";
            this.numericUpDownDeleteFirst.Size = new System.Drawing.Size(43, 20);
            this.numericUpDownDeleteFirst.TabIndex = 1;
            // 
            // labelDeleteFirstLines
            // 
            this.labelDeleteFirstLines.AutoSize = true;
            this.labelDeleteFirstLines.Location = new System.Drawing.Point(6, 20);
            this.labelDeleteFirstLines.Name = "labelDeleteFirstLines";
            this.labelDeleteFirstLines.Size = new System.Drawing.Size(81, 13);
            this.labelDeleteFirstLines.TabIndex = 0;
            this.labelDeleteFirstLines.Text = "Delete first lines";
            // 
            // groupBoxRemoveStyle
            // 
            this.groupBoxRemoveStyle.Controls.Add(this.textBoxRemoveStyle);
            this.groupBoxRemoveStyle.Controls.Add(this.labelStyleName);
            this.groupBoxRemoveStyle.Location = new System.Drawing.Point(307, 12);
            this.groupBoxRemoveStyle.Name = "groupBoxRemoveStyle";
            this.groupBoxRemoveStyle.Size = new System.Drawing.Size(271, 76);
            this.groupBoxRemoveStyle.TabIndex = 307;
            this.groupBoxRemoveStyle.TabStop = false;
            this.groupBoxRemoveStyle.Text = "Remove style/actor";
            this.groupBoxRemoveStyle.Visible = false;
            // 
            // textBoxRemoveStyle
            // 
            this.textBoxRemoveStyle.Location = new System.Drawing.Point(6, 35);
            this.textBoxRemoveStyle.Name = "textBoxRemoveStyle";
            this.textBoxRemoveStyle.Size = new System.Drawing.Size(257, 20);
            this.textBoxRemoveStyle.TabIndex = 8;
            // 
            // labelStyleName
            // 
            this.labelStyleName.AutoSize = true;
            this.labelStyleName.Location = new System.Drawing.Point(6, 20);
            this.labelStyleName.Name = "labelStyleName";
            this.labelStyleName.Size = new System.Drawing.Size(59, 13);
            this.labelStyleName.TabIndex = 0;
            this.labelStyleName.Text = "Style/actor";
            // 
            // groupBoxOffsetTimeCodes
            // 
            this.groupBoxOffsetTimeCodes.Controls.Add(this.radioButtonShowLater);
            this.groupBoxOffsetTimeCodes.Controls.Add(this.radioButtonShowEarlier);
            this.groupBoxOffsetTimeCodes.Controls.Add(this.timeUpDownAdjust);
            this.groupBoxOffsetTimeCodes.Controls.Add(this.labelHourMinSecMilliSecond);
            this.groupBoxOffsetTimeCodes.Location = new System.Drawing.Point(305, 19);
            this.groupBoxOffsetTimeCodes.Name = "groupBoxOffsetTimeCodes";
            this.groupBoxOffsetTimeCodes.Size = new System.Drawing.Size(271, 119);
            this.groupBoxOffsetTimeCodes.TabIndex = 306;
            this.groupBoxOffsetTimeCodes.TabStop = false;
            this.groupBoxOffsetTimeCodes.Text = "Offset time codes";
            this.groupBoxOffsetTimeCodes.Visible = false;
            // 
            // radioButtonShowLater
            // 
            this.radioButtonShowLater.AutoSize = true;
            this.radioButtonShowLater.Checked = true;
            this.radioButtonShowLater.Location = new System.Drawing.Point(9, 89);
            this.radioButtonShowLater.Name = "radioButtonShowLater";
            this.radioButtonShowLater.Size = new System.Drawing.Size(75, 17);
            this.radioButtonShowLater.TabIndex = 3;
            this.radioButtonShowLater.TabStop = true;
            this.radioButtonShowLater.Text = "Show later";
            this.radioButtonShowLater.UseVisualStyleBackColor = true;
            // 
            // radioButtonShowEarlier
            // 
            this.radioButtonShowEarlier.AutoSize = true;
            this.radioButtonShowEarlier.Location = new System.Drawing.Point(9, 66);
            this.radioButtonShowEarlier.Name = "radioButtonShowEarlier";
            this.radioButtonShowEarlier.Size = new System.Drawing.Size(83, 17);
            this.radioButtonShowEarlier.TabIndex = 2;
            this.radioButtonShowEarlier.Text = "Show earlier";
            this.radioButtonShowEarlier.UseVisualStyleBackColor = true;
            // 
            // timeUpDownAdjust
            // 
            this.timeUpDownAdjust.AutoSize = true;
            this.timeUpDownAdjust.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.timeUpDownAdjust.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.timeUpDownAdjust.Location = new System.Drawing.Point(7, 37);
            this.timeUpDownAdjust.Margin = new System.Windows.Forms.Padding(4);
            this.timeUpDownAdjust.Name = "timeUpDownAdjust";
            this.timeUpDownAdjust.Size = new System.Drawing.Size(111, 27);
            this.timeUpDownAdjust.TabIndex = 1;
            timeCode1.Hours = 0;
            timeCode1.Milliseconds = 0;
            timeCode1.Minutes = 0;
            timeCode1.Seconds = 0;
            timeCode1.TimeSpan = System.TimeSpan.Parse("00:00:00");
            timeCode1.TotalMilliseconds = 0D;
            timeCode1.TotalSeconds = 0D;
            this.timeUpDownAdjust.TimeCode = timeCode1;
            this.timeUpDownAdjust.UseVideoOffset = false;
            // 
            // labelHourMinSecMilliSecond
            // 
            this.labelHourMinSecMilliSecond.AutoSize = true;
            this.labelHourMinSecMilliSecond.Location = new System.Drawing.Point(6, 20);
            this.labelHourMinSecMilliSecond.Name = "labelHourMinSecMilliSecond";
            this.labelHourMinSecMilliSecond.Size = new System.Drawing.Size(90, 13);
            this.labelHourMinSecMilliSecond.TabIndex = 0;
            this.labelHourMinSecMilliSecond.Text = "Hours:min:sec.ms";
            // 
            // groupBoxChangeFrameRate
            // 
            this.groupBoxChangeFrameRate.Controls.Add(this.buttonSwapFrameRate);
            this.groupBoxChangeFrameRate.Controls.Add(this.comboBoxFrameRateTo);
            this.groupBoxChangeFrameRate.Controls.Add(this.labelToFrameRate);
            this.groupBoxChangeFrameRate.Controls.Add(this.comboBoxFrameRateFrom);
            this.groupBoxChangeFrameRate.Controls.Add(this.labelFromFrameRate);
            this.groupBoxChangeFrameRate.Location = new System.Drawing.Point(307, 12);
            this.groupBoxChangeFrameRate.Name = "groupBoxChangeFrameRate";
            this.groupBoxChangeFrameRate.Size = new System.Drawing.Size(269, 90);
            this.groupBoxChangeFrameRate.TabIndex = 305;
            this.groupBoxChangeFrameRate.TabStop = false;
            this.groupBoxChangeFrameRate.Text = "Change frame rate";
            this.groupBoxChangeFrameRate.Visible = false;
            // 
            // buttonSwapFrameRate
            // 
            this.buttonSwapFrameRate.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonSwapFrameRate.Location = new System.Drawing.Point(207, 28);
            this.buttonSwapFrameRate.Name = "buttonSwapFrameRate";
            this.buttonSwapFrameRate.Size = new System.Drawing.Size(27, 28);
            this.buttonSwapFrameRate.TabIndex = 9;
            this.buttonSwapFrameRate.Text = "<->";
            this.buttonSwapFrameRate.UseVisualStyleBackColor = true;
            this.buttonSwapFrameRate.Click += new System.EventHandler(this.buttonSwapFrameRate_Click);
            // 
            // comboBoxFrameRateTo
            // 
            this.comboBoxFrameRateTo.FormattingEnabled = true;
            this.comboBoxFrameRateTo.Location = new System.Drawing.Point(130, 46);
            this.comboBoxFrameRateTo.Name = "comboBoxFrameRateTo";
            this.comboBoxFrameRateTo.Size = new System.Drawing.Size(71, 21);
            this.comboBoxFrameRateTo.TabIndex = 3;
            // 
            // labelToFrameRate
            // 
            this.labelToFrameRate.AutoSize = true;
            this.labelToFrameRate.Location = new System.Drawing.Point(6, 50);
            this.labelToFrameRate.Name = "labelToFrameRate";
            this.labelToFrameRate.Size = new System.Drawing.Size(70, 13);
            this.labelToFrameRate.TabIndex = 2;
            this.labelToFrameRate.Text = "To frame rate";
            // 
            // comboBoxFrameRateFrom
            // 
            this.comboBoxFrameRateFrom.FormattingEnabled = true;
            this.comboBoxFrameRateFrom.Location = new System.Drawing.Point(130, 17);
            this.comboBoxFrameRateFrom.Name = "comboBoxFrameRateFrom";
            this.comboBoxFrameRateFrom.Size = new System.Drawing.Size(71, 21);
            this.comboBoxFrameRateFrom.TabIndex = 1;
            // 
            // labelFromFrameRate
            // 
            this.labelFromFrameRate.AutoSize = true;
            this.labelFromFrameRate.Location = new System.Drawing.Point(6, 21);
            this.labelFromFrameRate.Name = "labelFromFrameRate";
            this.labelFromFrameRate.Size = new System.Drawing.Size(80, 13);
            this.labelFromFrameRate.TabIndex = 0;
            this.labelFromFrameRate.Text = "From frame rate";
            // 
            // groupBoxFixRtl
            // 
            this.groupBoxFixRtl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxFixRtl.Controls.Add(this.radioButtonReverseStartEnd);
            this.groupBoxFixRtl.Controls.Add(this.radioButtonRemoveUnicode);
            this.groupBoxFixRtl.Controls.Add(this.radioButtonAddUnicode);
            this.groupBoxFixRtl.Location = new System.Drawing.Point(305, 17);
            this.groupBoxFixRtl.Name = "groupBoxFixRtl";
            this.groupBoxFixRtl.Size = new System.Drawing.Size(271, 115);
            this.groupBoxFixRtl.TabIndex = 303;
            this.groupBoxFixRtl.TabStop = false;
            this.groupBoxFixRtl.Text = "Settings";
            this.groupBoxFixRtl.Visible = false;
            // 
            // radioButtonReverseStartEnd
            // 
            this.radioButtonReverseStartEnd.AutoSize = true;
            this.radioButtonReverseStartEnd.Location = new System.Drawing.Point(19, 77);
            this.radioButtonReverseStartEnd.Name = "radioButtonReverseStartEnd";
            this.radioButtonReverseStartEnd.Size = new System.Drawing.Size(135, 17);
            this.radioButtonReverseStartEnd.TabIndex = 2;
            this.radioButtonReverseStartEnd.TabStop = true;
            this.radioButtonReverseStartEnd.Text = "Reverse RTL start/end";
            this.radioButtonReverseStartEnd.UseVisualStyleBackColor = true;
            // 
            // radioButtonRemoveUnicode
            // 
            this.radioButtonRemoveUnicode.AutoSize = true;
            this.radioButtonRemoveUnicode.Location = new System.Drawing.Point(19, 54);
            this.radioButtonRemoveUnicode.Name = "radioButtonRemoveUnicode";
            this.radioButtonRemoveUnicode.Size = new System.Drawing.Size(153, 17);
            this.radioButtonRemoveUnicode.TabIndex = 1;
            this.radioButtonRemoveUnicode.TabStop = true;
            this.radioButtonRemoveUnicode.Text = "Remove RTL unicode tags";
            this.radioButtonRemoveUnicode.UseVisualStyleBackColor = true;
            // 
            // radioButtonAddUnicode
            // 
            this.radioButtonAddUnicode.AutoSize = true;
            this.radioButtonAddUnicode.Location = new System.Drawing.Point(19, 31);
            this.radioButtonAddUnicode.Name = "radioButtonAddUnicode";
            this.radioButtonAddUnicode.Size = new System.Drawing.Size(145, 17);
            this.radioButtonAddUnicode.TabIndex = 0;
            this.radioButtonAddUnicode.TabStop = true;
            this.radioButtonAddUnicode.Text = "Fix RTL via Unicode tags";
            this.radioButtonAddUnicode.UseVisualStyleBackColor = true;
            // 
            // groupBoxSpeed
            // 
            this.groupBoxSpeed.Controls.Add(this.radioButtonToDropFrame);
            this.groupBoxSpeed.Controls.Add(this.radioButtonSpeedFromDropFrame);
            this.groupBoxSpeed.Controls.Add(this.radioButtonSpeedCustom);
            this.groupBoxSpeed.Controls.Add(this.numericUpDownPercent);
            this.groupBoxSpeed.Controls.Add(this.labelPercent);
            this.groupBoxSpeed.Location = new System.Drawing.Point(305, 17);
            this.groupBoxSpeed.Name = "groupBoxSpeed";
            this.groupBoxSpeed.Size = new System.Drawing.Size(271, 129);
            this.groupBoxSpeed.TabIndex = 307;
            this.groupBoxSpeed.TabStop = false;
            this.groupBoxSpeed.Text = "Change speed";
            this.groupBoxSpeed.Visible = false;
            // 
            // radioButtonToDropFrame
            // 
            this.radioButtonToDropFrame.AutoSize = true;
            this.radioButtonToDropFrame.Location = new System.Drawing.Point(6, 91);
            this.radioButtonToDropFrame.Name = "radioButtonToDropFrame";
            this.radioButtonToDropFrame.Size = new System.Drawing.Size(91, 17);
            this.radioButtonToDropFrame.TabIndex = 3;
            this.radioButtonToDropFrame.Text = "To drop frame";
            this.radioButtonToDropFrame.UseVisualStyleBackColor = true;
            this.radioButtonToDropFrame.CheckedChanged += new System.EventHandler(this.radioButtonToDropFrame_CheckedChanged);
            // 
            // radioButtonSpeedFromDropFrame
            // 
            this.radioButtonSpeedFromDropFrame.AutoSize = true;
            this.radioButtonSpeedFromDropFrame.Location = new System.Drawing.Point(6, 68);
            this.radioButtonSpeedFromDropFrame.Name = "radioButtonSpeedFromDropFrame";
            this.radioButtonSpeedFromDropFrame.Size = new System.Drawing.Size(101, 17);
            this.radioButtonSpeedFromDropFrame.TabIndex = 2;
            this.radioButtonSpeedFromDropFrame.Text = "From drop frame";
            this.radioButtonSpeedFromDropFrame.UseVisualStyleBackColor = true;
            this.radioButtonSpeedFromDropFrame.CheckedChanged += new System.EventHandler(this.radioButtonSpeedFromDropFrame_CheckedChanged);
            // 
            // radioButtonSpeedCustom
            // 
            this.radioButtonSpeedCustom.AutoSize = true;
            this.radioButtonSpeedCustom.Checked = true;
            this.radioButtonSpeedCustom.Location = new System.Drawing.Point(6, 45);
            this.radioButtonSpeedCustom.Name = "radioButtonSpeedCustom";
            this.radioButtonSpeedCustom.Size = new System.Drawing.Size(60, 17);
            this.radioButtonSpeedCustom.TabIndex = 1;
            this.radioButtonSpeedCustom.TabStop = true;
            this.radioButtonSpeedCustom.Text = "Custom";
            this.radioButtonSpeedCustom.UseVisualStyleBackColor = true;
            this.radioButtonSpeedCustom.CheckedChanged += new System.EventHandler(this.radioButtonSpeedCustom_CheckedChanged);
            // 
            // numericUpDownPercent
            // 
            this.numericUpDownPercent.DecimalPlaces = 5;
            this.numericUpDownPercent.Location = new System.Drawing.Point(6, 19);
            this.numericUpDownPercent.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.numericUpDownPercent.Minimum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numericUpDownPercent.Name = "numericUpDownPercent";
            this.numericUpDownPercent.Size = new System.Drawing.Size(81, 20);
            this.numericUpDownPercent.TabIndex = 0;
            this.numericUpDownPercent.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // labelPercent
            // 
            this.labelPercent.AutoSize = true;
            this.labelPercent.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelPercent.Location = new System.Drawing.Point(87, 22);
            this.labelPercent.Name = "labelPercent";
            this.labelPercent.Size = new System.Drawing.Size(15, 13);
            this.labelPercent.TabIndex = 12;
            this.labelPercent.Text = "%";
            // 
            // groupBoxChangeCasing
            // 
            this.groupBoxChangeCasing.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxChangeCasing.Controls.Add(this.radioButtonFixOnlyNames);
            this.groupBoxChangeCasing.Controls.Add(this.radioButtonLowercase);
            this.groupBoxChangeCasing.Controls.Add(this.radioButtonUppercase);
            this.groupBoxChangeCasing.Controls.Add(this.radioButtonNormal);
            this.groupBoxChangeCasing.Location = new System.Drawing.Point(308, 16);
            this.groupBoxChangeCasing.Name = "groupBoxChangeCasing";
            this.groupBoxChangeCasing.Size = new System.Drawing.Size(268, 125);
            this.groupBoxChangeCasing.TabIndex = 308;
            this.groupBoxChangeCasing.TabStop = false;
            this.groupBoxChangeCasing.Text = "Change casing to";
            // 
            // radioButtonFixOnlyNames
            // 
            this.radioButtonFixOnlyNames.AutoSize = true;
            this.radioButtonFixOnlyNames.Location = new System.Drawing.Point(11, 43);
            this.radioButtonFixOnlyNames.Name = "radioButtonFixOnlyNames";
            this.radioButtonFixOnlyNames.Size = new System.Drawing.Size(263, 17);
            this.radioButtonFixOnlyNames.TabIndex = 6;
            this.radioButtonFixOnlyNames.Text = "Fix only names casing (via Dictionaries\\names.xml)";
            this.radioButtonFixOnlyNames.UseVisualStyleBackColor = true;
            // 
            // radioButtonLowercase
            // 
            this.radioButtonLowercase.AutoSize = true;
            this.radioButtonLowercase.Location = new System.Drawing.Point(11, 89);
            this.radioButtonLowercase.Name = "radioButtonLowercase";
            this.radioButtonLowercase.Size = new System.Drawing.Size(86, 17);
            this.radioButtonLowercase.TabIndex = 10;
            this.radioButtonLowercase.Text = "all lowercase";
            this.radioButtonLowercase.UseVisualStyleBackColor = true;
            // 
            // radioButtonUppercase
            // 
            this.radioButtonUppercase.AutoSize = true;
            this.radioButtonUppercase.Location = new System.Drawing.Point(11, 66);
            this.radioButtonUppercase.Name = "radioButtonUppercase";
            this.radioButtonUppercase.Size = new System.Drawing.Size(112, 17);
            this.radioButtonUppercase.TabIndex = 8;
            this.radioButtonUppercase.Text = "ALL UPPERCASE";
            this.radioButtonUppercase.UseVisualStyleBackColor = true;
            // 
            // radioButtonNormal
            // 
            this.radioButtonNormal.AutoSize = true;
            this.radioButtonNormal.Checked = true;
            this.radioButtonNormal.Location = new System.Drawing.Point(11, 18);
            this.radioButtonNormal.Name = "radioButtonNormal";
            this.radioButtonNormal.Size = new System.Drawing.Size(282, 17);
            this.radioButtonNormal.TabIndex = 0;
            this.radioButtonNormal.TabStop = true;
            this.radioButtonNormal.Text = "Normal casing. Sentences begin with uppercase letter.";
            this.radioButtonNormal.UseVisualStyleBackColor = true;
            // 
            // groupBoxOutput
            // 
            this.groupBoxOutput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxOutput.Controls.Add(this.buttonBrowseEncoding);
            this.groupBoxOutput.Controls.Add(this.radioButtonSaveInOutputFolder);
            this.groupBoxOutput.Controls.Add(this.buttonTransportStreamSettings);
            this.groupBoxOutput.Controls.Add(this.linkLabelOpenOutputFolder);
            this.groupBoxOutput.Controls.Add(this.checkBoxUseStyleFromSource);
            this.groupBoxOutput.Controls.Add(this.checkBoxOverwrite);
            this.groupBoxOutput.Controls.Add(this.buttonStyles);
            this.groupBoxOutput.Controls.Add(this.buttonChooseFolder);
            this.groupBoxOutput.Controls.Add(this.comboBoxSubtitleFormats);
            this.groupBoxOutput.Controls.Add(this.textBoxOutputFolder);
            this.groupBoxOutput.Controls.Add(this.labelEncoding);
            this.groupBoxOutput.Controls.Add(this.radioButtonSaveInSourceFolder);
            this.groupBoxOutput.Controls.Add(this.comboBoxEncoding);
            this.groupBoxOutput.Controls.Add(this.labelOutputFormat);
            this.groupBoxOutput.Controls.Add(this.groupBoxConvertOptions);
            this.groupBoxOutput.Location = new System.Drawing.Point(12, 305);
            this.groupBoxOutput.Name = "groupBoxOutput";
            this.groupBoxOutput.Size = new System.Drawing.Size(1014, 300);
            this.groupBoxOutput.TabIndex = 1;
            this.groupBoxOutput.TabStop = false;
            this.groupBoxOutput.Text = "Output";
            // 
            // buttonBrowseEncoding
            // 
            this.buttonBrowseEncoding.Location = new System.Drawing.Point(300, 263);
            this.buttonBrowseEncoding.Name = "buttonBrowseEncoding";
            this.buttonBrowseEncoding.Size = new System.Drawing.Size(26, 23);
            this.buttonBrowseEncoding.TabIndex = 12;
            this.buttonBrowseEncoding.Text = "...";
            this.buttonBrowseEncoding.UseVisualStyleBackColor = true;
            this.buttonBrowseEncoding.Click += new System.EventHandler(this.buttonBrowseEncoding_Click);
            // 
            // radioButtonSaveInOutputFolder
            // 
            this.radioButtonSaveInOutputFolder.AutoSize = true;
            this.radioButtonSaveInOutputFolder.Location = new System.Drawing.Point(10, 56);
            this.radioButtonSaveInOutputFolder.Name = "radioButtonSaveInOutputFolder";
            this.radioButtonSaveInOutputFolder.Size = new System.Drawing.Size(154, 17);
            this.radioButtonSaveInOutputFolder.TabIndex = 11;
            this.radioButtonSaveInOutputFolder.Text = "Save in output folder below";
            this.radioButtonSaveInOutputFolder.UseVisualStyleBackColor = true;
            this.radioButtonSaveInOutputFolder.CheckedChanged += new System.EventHandler(this.radioButtonSaveInOutputFolder_CheckedChanged);
            // 
            // buttonTransportStreamSettings
            // 
            this.buttonTransportStreamSettings.Location = new System.Drawing.Point(300, 236);
            this.buttonTransportStreamSettings.Name = "buttonTransportStreamSettings";
            this.buttonTransportStreamSettings.Size = new System.Drawing.Size(116, 23);
            this.buttonTransportStreamSettings.TabIndex = 10;
            this.buttonTransportStreamSettings.Text = "TS settings...";
            this.buttonTransportStreamSettings.UseVisualStyleBackColor = true;
            this.buttonTransportStreamSettings.Visible = false;
            this.buttonTransportStreamSettings.Click += new System.EventHandler(this.buttonTransportStreamSettings_Click);
            // 
            // linkLabelOpenOutputFolder
            // 
            this.linkLabelOpenOutputFolder.AutoSize = true;
            this.linkLabelOpenOutputFolder.Location = new System.Drawing.Point(357, 81);
            this.linkLabelOpenOutputFolder.Name = "linkLabelOpenOutputFolder";
            this.linkLabelOpenOutputFolder.Size = new System.Drawing.Size(42, 13);
            this.linkLabelOpenOutputFolder.TabIndex = 9;
            this.linkLabelOpenOutputFolder.TabStop = true;
            this.linkLabelOpenOutputFolder.Text = "Open...";
            this.linkLabelOpenOutputFolder.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelOpenOutputFolder_LinkClicked);
            // 
            // checkBoxUseStyleFromSource
            // 
            this.checkBoxUseStyleFromSource.AutoSize = true;
            this.checkBoxUseStyleFromSource.Location = new System.Drawing.Point(80, 236);
            this.checkBoxUseStyleFromSource.Name = "checkBoxUseStyleFromSource";
            this.checkBoxUseStyleFromSource.Size = new System.Drawing.Size(127, 17);
            this.checkBoxUseStyleFromSource.TabIndex = 9;
            this.checkBoxUseStyleFromSource.Text = "Use style from source";
            this.checkBoxUseStyleFromSource.UseVisualStyleBackColor = true;
            this.checkBoxUseStyleFromSource.Visible = false;
            // 
            // checkBoxOverwrite
            // 
            this.checkBoxOverwrite.AutoSize = true;
            this.checkBoxOverwrite.Location = new System.Drawing.Point(10, 122);
            this.checkBoxOverwrite.Name = "checkBoxOverwrite";
            this.checkBoxOverwrite.Size = new System.Drawing.Size(125, 17);
            this.checkBoxOverwrite.TabIndex = 10;
            this.checkBoxOverwrite.Text = "Overwrite exiting files";
            this.checkBoxOverwrite.UseVisualStyleBackColor = true;
            // 
            // buttonStyles
            // 
            this.buttonStyles.Location = new System.Drawing.Point(300, 207);
            this.buttonStyles.Name = "buttonStyles";
            this.buttonStyles.Size = new System.Drawing.Size(116, 23);
            this.buttonStyles.TabIndex = 8;
            this.buttonStyles.Text = "Style...";
            this.buttonStyles.UseVisualStyleBackColor = true;
            this.buttonStyles.Visible = false;
            this.buttonStyles.Click += new System.EventHandler(this.ButtonStylesClick);
            // 
            // buttonChooseFolder
            // 
            this.buttonChooseFolder.Enabled = false;
            this.buttonChooseFolder.Location = new System.Drawing.Point(325, 76);
            this.buttonChooseFolder.Name = "buttonChooseFolder";
            this.buttonChooseFolder.Size = new System.Drawing.Size(26, 23);
            this.buttonChooseFolder.TabIndex = 8;
            this.buttonChooseFolder.Text = "...";
            this.buttonChooseFolder.UseVisualStyleBackColor = true;
            this.buttonChooseFolder.Click += new System.EventHandler(this.buttonChooseFolder_Click);
            // 
            // comboBoxSubtitleFormats
            // 
            this.comboBoxSubtitleFormats.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSubtitleFormats.FormattingEnabled = true;
            this.comboBoxSubtitleFormats.Location = new System.Drawing.Point(80, 209);
            this.comboBoxSubtitleFormats.Name = "comboBoxSubtitleFormats";
            this.comboBoxSubtitleFormats.Size = new System.Drawing.Size(214, 21);
            this.comboBoxSubtitleFormats.TabIndex = 7;
            this.comboBoxSubtitleFormats.SelectedIndexChanged += new System.EventHandler(this.ComboBoxSubtitleFormatsSelectedIndexChanged);
            // 
            // textBoxOutputFolder
            // 
            this.textBoxOutputFolder.Enabled = false;
            this.textBoxOutputFolder.Location = new System.Drawing.Point(17, 79);
            this.textBoxOutputFolder.Name = "textBoxOutputFolder";
            this.textBoxOutputFolder.Size = new System.Drawing.Size(302, 20);
            this.textBoxOutputFolder.TabIndex = 7;
            // 
            // labelEncoding
            // 
            this.labelEncoding.AutoSize = true;
            this.labelEncoding.Location = new System.Drawing.Point(10, 267);
            this.labelEncoding.Name = "labelEncoding";
            this.labelEncoding.Size = new System.Drawing.Size(52, 13);
            this.labelEncoding.TabIndex = 9;
            this.labelEncoding.Text = "Encoding";
            // 
            // radioButtonSaveInSourceFolder
            // 
            this.radioButtonSaveInSourceFolder.AutoSize = true;
            this.radioButtonSaveInSourceFolder.Checked = true;
            this.radioButtonSaveInSourceFolder.Location = new System.Drawing.Point(10, 27);
            this.radioButtonSaveInSourceFolder.Name = "radioButtonSaveInSourceFolder";
            this.radioButtonSaveInSourceFolder.Size = new System.Drawing.Size(141, 17);
            this.radioButtonSaveInSourceFolder.TabIndex = 0;
            this.radioButtonSaveInSourceFolder.TabStop = true;
            this.radioButtonSaveInSourceFolder.Text = "Save in source file folder";
            this.radioButtonSaveInSourceFolder.UseVisualStyleBackColor = true;
            this.radioButtonSaveInSourceFolder.CheckedChanged += new System.EventHandler(this.radioButtonSaveInSourceFolder_CheckedChanged);
            // 
            // comboBoxEncoding
            // 
            this.comboBoxEncoding.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxEncoding.FormattingEnabled = true;
            this.comboBoxEncoding.Location = new System.Drawing.Point(80, 264);
            this.comboBoxEncoding.Name = "comboBoxEncoding";
            this.comboBoxEncoding.Size = new System.Drawing.Size(214, 21);
            this.comboBoxEncoding.TabIndex = 11;
            // 
            // labelOutputFormat
            // 
            this.labelOutputFormat.AutoSize = true;
            this.labelOutputFormat.Location = new System.Drawing.Point(10, 212);
            this.labelOutputFormat.Name = "labelOutputFormat";
            this.labelOutputFormat.Size = new System.Drawing.Size(39, 13);
            this.labelOutputFormat.TabIndex = 6;
            this.labelOutputFormat.Text = "Format";
            // 
            // groupBoxInput
            // 
            this.groupBoxInput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxInput.Controls.Add(this.labelNumberOfFiles);
            this.groupBoxInput.Controls.Add(this.textBoxFilter);
            this.groupBoxInput.Controls.Add(this.labelFilter);
            this.groupBoxInput.Controls.Add(this.comboBoxFilter);
            this.groupBoxInput.Controls.Add(this.checkBoxScanFolderRecursive);
            this.groupBoxInput.Controls.Add(this.buttonSearchFolder);
            this.groupBoxInput.Controls.Add(this.buttonInputBrowse);
            this.groupBoxInput.Controls.Add(this.labelChooseInputFiles);
            this.groupBoxInput.Controls.Add(this.listViewInputFiles);
            this.groupBoxInput.Location = new System.Drawing.Point(12, 12);
            this.groupBoxInput.Name = "groupBoxInput";
            this.groupBoxInput.Size = new System.Drawing.Size(1014, 287);
            this.groupBoxInput.TabIndex = 0;
            this.groupBoxInput.TabStop = false;
            this.groupBoxInput.Text = "Input";
            // 
            // labelNumberOfFiles
            // 
            this.labelNumberOfFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.labelNumberOfFiles.Location = new System.Drawing.Point(872, 258);
            this.labelNumberOfFiles.Name = "labelNumberOfFiles";
            this.labelNumberOfFiles.Size = new System.Drawing.Size(100, 16);
            this.labelNumberOfFiles.TabIndex = 14;
            this.labelNumberOfFiles.Text = "labelNumberOfFiles";
            this.labelNumberOfFiles.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // textBoxFilter
            // 
            this.textBoxFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBoxFilter.Location = new System.Drawing.Point(422, 258);
            this.textBoxFilter.Name = "textBoxFilter";
            this.textBoxFilter.Size = new System.Drawing.Size(158, 20);
            this.textBoxFilter.TabIndex = 13;
            this.textBoxFilter.TextChanged += new System.EventHandler(this.textBoxFilter_TextChanged);
            // 
            // labelFilter
            // 
            this.labelFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelFilter.AutoSize = true;
            this.labelFilter.Location = new System.Drawing.Point(11, 261);
            this.labelFilter.Name = "labelFilter";
            this.labelFilter.Size = new System.Drawing.Size(29, 13);
            this.labelFilter.TabIndex = 11;
            this.labelFilter.Text = "Filter";
            // 
            // comboBoxFilter
            // 
            this.comboBoxFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.comboBoxFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxFilter.FormattingEnabled = true;
            this.comboBoxFilter.Items.AddRange(new object[] {
            "All files",
            "SubRip .srt files without BOM header",
            "Files with subtitle with more than two lines",
            "Files that contains...",
            "File name cotains...",
            "Mkv language code contains..."});
            this.comboBoxFilter.Location = new System.Drawing.Point(81, 258);
            this.comboBoxFilter.Name = "comboBoxFilter";
            this.comboBoxFilter.Size = new System.Drawing.Size(335, 21);
            this.comboBoxFilter.TabIndex = 12;
            this.comboBoxFilter.SelectedIndexChanged += new System.EventHandler(this.comboBoxFilter_SelectedIndexChanged);
            // 
            // checkBoxScanFolderRecursive
            // 
            this.checkBoxScanFolderRecursive.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxScanFolderRecursive.AutoSize = true;
            this.checkBoxScanFolderRecursive.Checked = true;
            this.checkBoxScanFolderRecursive.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxScanFolderRecursive.Location = new System.Drawing.Point(778, 16);
            this.checkBoxScanFolderRecursive.Name = "checkBoxScanFolderRecursive";
            this.checkBoxScanFolderRecursive.Size = new System.Drawing.Size(74, 17);
            this.checkBoxScanFolderRecursive.TabIndex = 0;
            this.checkBoxScanFolderRecursive.Text = "Recursive";
            this.checkBoxScanFolderRecursive.UseVisualStyleBackColor = true;
            // 
            // buttonSearchFolder
            // 
            this.buttonSearchFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSearchFolder.Location = new System.Drawing.Point(858, 12);
            this.buttonSearchFolder.Name = "buttonSearchFolder";
            this.buttonSearchFolder.Size = new System.Drawing.Size(116, 23);
            this.buttonSearchFolder.TabIndex = 1;
            this.buttonSearchFolder.Text = "Search folder...";
            this.buttonSearchFolder.UseVisualStyleBackColor = true;
            this.buttonSearchFolder.Click += new System.EventHandler(this.buttonSearchFolder_Click);
            // 
            // buttonInputBrowse
            // 
            this.buttonInputBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonInputBrowse.Location = new System.Drawing.Point(979, 41);
            this.buttonInputBrowse.Name = "buttonInputBrowse";
            this.buttonInputBrowse.Size = new System.Drawing.Size(26, 23);
            this.buttonInputBrowse.TabIndex = 3;
            this.buttonInputBrowse.Text = "...";
            this.buttonInputBrowse.UseVisualStyleBackColor = true;
            this.buttonInputBrowse.Click += new System.EventHandler(this.buttonInputBrowse_Click);
            // 
            // labelChooseInputFiles
            // 
            this.labelChooseInputFiles.AutoSize = true;
            this.labelChooseInputFiles.Location = new System.Drawing.Point(5, 25);
            this.labelChooseInputFiles.Name = "labelChooseInputFiles";
            this.labelChooseInputFiles.Size = new System.Drawing.Size(202, 13);
            this.labelChooseInputFiles.TabIndex = 0;
            this.labelChooseInputFiles.Text = "Choose input files (browse or drag-n-drop)";
            // 
            // listViewInputFiles
            // 
            this.listViewInputFiles.AllowDrop = true;
            this.listViewInputFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewInputFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderFName,
            this.columnHeaderSize,
            this.columnHeaderFormat,
            this.columnHeaderStatus});
            this.listViewInputFiles.ContextMenuStrip = this.contextMenuStripFiles;
            this.listViewInputFiles.FullRowSelect = true;
            this.listViewInputFiles.HideSelection = false;
            this.listViewInputFiles.Location = new System.Drawing.Point(6, 41);
            this.listViewInputFiles.Name = "listViewInputFiles";
            this.listViewInputFiles.Size = new System.Drawing.Size(968, 211);
            this.listViewInputFiles.TabIndex = 2;
            this.listViewInputFiles.UseCompatibleStateImageBehavior = false;
            this.listViewInputFiles.View = System.Windows.Forms.View.Details;
            this.listViewInputFiles.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listViewInputFiles_ColumnClick);
            this.listViewInputFiles.DragDrop += new System.Windows.Forms.DragEventHandler(this.listViewInputFiles_DragDrop);
            this.listViewInputFiles.DragEnter += new System.Windows.Forms.DragEventHandler(this.listViewInputFiles_DragEnter);
            this.listViewInputFiles.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ListViewInputFilesKeyDown);
            // 
            // columnHeaderFName
            // 
            this.columnHeaderFName.Text = "File name";
            this.columnHeaderFName.Width = 500;
            // 
            // columnHeaderSize
            // 
            this.columnHeaderSize.Text = "Size";
            this.columnHeaderSize.Width = 75;
            // 
            // columnHeaderFormat
            // 
            this.columnHeaderFormat.Text = "Format";
            this.columnHeaderFormat.Width = 200;
            // 
            // columnHeaderStatus
            // 
            this.columnHeaderStatus.Text = "Status";
            this.columnHeaderStatus.Width = 124;
            // 
            // contextMenuStripFiles
            // 
            this.contextMenuStripFiles.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addFilesToolStripMenuItem,
            this.toolStripSeparator2,
            this.removeToolStripMenuItem,
            this.removeAllToolStripMenuItem,
            this.openContainingFolderToolStripMenuItem,
            this.toolStripSeparator1,
            this.convertMkvSettingsToolStripMenuItem});
            this.contextMenuStripFiles.Name = "contextMenuStripStyles";
            this.contextMenuStripFiles.Size = new System.Drawing.Size(400, 126);
            this.contextMenuStripFiles.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenuStripFilesOpening);
            // 
            // addFilesToolStripMenuItem
            // 
            this.addFilesToolStripMenuItem.Name = "addFilesToolStripMenuItem";
            this.addFilesToolStripMenuItem.Size = new System.Drawing.Size(399, 22);
            this.addFilesToolStripMenuItem.Text = "Add files";
            this.addFilesToolStripMenuItem.Click += new System.EventHandler(this.addFilesToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(396, 6);
            // 
            // removeToolStripMenuItem
            // 
            this.removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            this.removeToolStripMenuItem.Size = new System.Drawing.Size(399, 22);
            this.removeToolStripMenuItem.Text = "Remove";
            this.removeToolStripMenuItem.Click += new System.EventHandler(this.RemoveToolStripMenuItemClick);
            // 
            // removeAllToolStripMenuItem
            // 
            this.removeAllToolStripMenuItem.Name = "removeAllToolStripMenuItem";
            this.removeAllToolStripMenuItem.Size = new System.Drawing.Size(399, 22);
            this.removeAllToolStripMenuItem.Text = "Remove all";
            this.removeAllToolStripMenuItem.Click += new System.EventHandler(this.RemoveAllToolStripMenuItemClick);
            // 
            // openContainingFolderToolStripMenuItem
            // 
            this.openContainingFolderToolStripMenuItem.Name = "openContainingFolderToolStripMenuItem";
            this.openContainingFolderToolStripMenuItem.Size = new System.Drawing.Size(399, 22);
            this.openContainingFolderToolStripMenuItem.Text = "Open containing folder";
            this.openContainingFolderToolStripMenuItem.Click += new System.EventHandler(this.openContainingFolderToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(396, 6);
            // 
            // convertMkvSettingsToolStripMenuItem
            // 
            this.convertMkvSettingsToolStripMenuItem.Name = "convertMkvSettingsToolStripMenuItem";
            this.convertMkvSettingsToolStripMenuItem.Size = new System.Drawing.Size(399, 22);
            this.convertMkvSettingsToolStripMenuItem.Text = "Mkv language in output file name: Three letter language code";
            this.convertMkvSettingsToolStripMenuItem.Click += new System.EventHandler(this.convertMkvThreeLetterLanguageCodesToTwoLettersToolStripMenuItem_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(15, 627);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(820, 10);
            this.progressBar1.TabIndex = 8;
            // 
            // labelStatus
            // 
            this.labelStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(12, 611);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(59, 13);
            this.labelStatus.TabIndex = 9;
            this.labelStatus.Text = "labelStatus";
            // 
            // BatchConvert
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1035, 651);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.groupBoxOutput);
            this.Controls.Add(this.groupBoxInput);
            this.Controls.Add(this.buttonConvert);
            this.Controls.Add(this.buttonCancel);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(1024, 578);
            this.Name = "BatchConvert";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Batch convert";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.BatchConvert_FormClosing);
            this.Shown += new System.EventHandler(this.BatchConvert_Shown);
            this.ResizeEnd += new System.EventHandler(this.BatchConvert_ResizeEnd);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.BatchConvert_KeyDown);
            this.groupBoxConvertOptions.ResumeLayout(false);
            this.contextMenuStripOptions.ResumeLayout(false);
            this.groupBoxMergeShortLines.ResumeLayout(false);
            this.groupBoxMergeShortLines.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxCharacters)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxMillisecondsBetweenLines)).EndInit();
            this.groupBoxAdjustDuration.ResumeLayout(false);
            this.groupBoxAdjustDuration.PerformLayout();
            this.panelAdjustDurationFixed.ResumeLayout(false);
            this.panelAdjustDurationFixed.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFixedMilliseconds)).EndInit();
            this.panelAdjustDurationAddPercent.ResumeLayout(false);
            this.panelAdjustDurationAddPercent.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownAdjustViaPercent)).EndInit();
            this.panelAdjustDurationAddSeconds.ResumeLayout(false);
            this.panelAdjustDurationAddSeconds.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSeconds)).EndInit();
            this.panelAdjustDurationRecalc.ResumeLayout(false);
            this.panelAdjustDurationRecalc.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownOptimalCharsSec)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxCharsSec)).EndInit();
            this.groupBoxDeleteLines.ResumeLayout(false);
            this.groupBoxDeleteLines.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDeleteLast)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDeleteFirst)).EndInit();
            this.groupBoxRemoveStyle.ResumeLayout(false);
            this.groupBoxRemoveStyle.PerformLayout();
            this.groupBoxOffsetTimeCodes.ResumeLayout(false);
            this.groupBoxOffsetTimeCodes.PerformLayout();
            this.groupBoxChangeFrameRate.ResumeLayout(false);
            this.groupBoxChangeFrameRate.PerformLayout();
            this.groupBoxFixRtl.ResumeLayout(false);
            this.groupBoxFixRtl.PerformLayout();
            this.groupBoxSpeed.ResumeLayout(false);
            this.groupBoxSpeed.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPercent)).EndInit();
            this.groupBoxChangeCasing.ResumeLayout(false);
            this.groupBoxChangeCasing.PerformLayout();
            this.groupBoxOutput.ResumeLayout(false);
            this.groupBoxOutput.PerformLayout();
            this.groupBoxInput.ResumeLayout(false);
            this.groupBoxInput.PerformLayout();
            this.contextMenuStripFiles.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonConvert;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.GroupBox groupBoxConvertOptions;
        private System.Windows.Forms.GroupBox groupBoxOutput;
        private System.Windows.Forms.ComboBox comboBoxSubtitleFormats;
        private System.Windows.Forms.Label labelEncoding;
        private System.Windows.Forms.ComboBox comboBoxEncoding;
        private System.Windows.Forms.Label labelOutputFormat;
        private System.Windows.Forms.GroupBox groupBoxInput;
        private System.Windows.Forms.Button buttonInputBrowse;
        private System.Windows.Forms.Label labelChooseInputFiles;
        private System.Windows.Forms.ListView listViewInputFiles;
        private System.Windows.Forms.ColumnHeader columnHeaderFName;
        private System.Windows.Forms.ColumnHeader columnHeaderSize;
        private System.Windows.Forms.ColumnHeader columnHeaderFormat;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.ColumnHeader columnHeaderStatus;
        private System.Windows.Forms.Button buttonStyles;
        private System.Windows.Forms.GroupBox groupBoxOffsetTimeCodes;
        private System.Windows.Forms.GroupBox groupBoxChangeFrameRate;
        private Controls.TimeUpDown timeUpDownAdjust;
        private System.Windows.Forms.Label labelHourMinSecMilliSecond;
        private System.Windows.Forms.ComboBox comboBoxFrameRateTo;
        private System.Windows.Forms.Label labelToFrameRate;
        private System.Windows.Forms.ComboBox comboBoxFrameRateFrom;
        private System.Windows.Forms.Label labelFromFrameRate;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripFiles;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeAllToolStripMenuItem;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.RadioButton radioButtonShowLater;
        private System.Windows.Forms.RadioButton radioButtonShowEarlier;
        private System.Windows.Forms.Button buttonSearchFolder;
        private System.Windows.Forms.CheckBox checkBoxScanFolderRecursive;
        private System.Windows.Forms.Label labelFilter;
        private System.Windows.Forms.ComboBox comboBoxFilter;
        private System.Windows.Forms.TextBox textBoxFilter;
        private System.Windows.Forms.GroupBox groupBoxSpeed;
        private System.Windows.Forms.NumericUpDown numericUpDownPercent;
        private System.Windows.Forms.Label labelPercent;
        private System.Windows.Forms.RadioButton radioButtonToDropFrame;
        private System.Windows.Forms.RadioButton radioButtonSpeedFromDropFrame;
        private System.Windows.Forms.RadioButton radioButtonSpeedCustom;
        private System.Windows.Forms.CheckBox checkBoxUseStyleFromSource;
        private System.Windows.Forms.Button buttonTransportStreamSettings;
        private System.Windows.Forms.RadioButton radioButtonSaveInOutputFolder;
        private System.Windows.Forms.LinkLabel linkLabelOpenOutputFolder;
        private System.Windows.Forms.CheckBox checkBoxOverwrite;
        private System.Windows.Forms.Button buttonChooseFolder;
        private System.Windows.Forms.TextBox textBoxOutputFolder;
        private System.Windows.Forms.RadioButton radioButtonSaveInSourceFolder;
        private System.Windows.Forms.ListView listViewConvertOptions;
        private System.Windows.Forms.ColumnHeader ActionCheckBox;
        private System.Windows.Forms.ColumnHeader Action;
        private System.Windows.Forms.Button buttonConvertOptionsSettings;
        private System.Windows.Forms.GroupBox groupBoxFixRtl;
        private System.Windows.Forms.RadioButton radioButtonReverseStartEnd;
        private System.Windows.Forms.RadioButton radioButtonRemoveUnicode;
        private System.Windows.Forms.RadioButton radioButtonAddUnicode;
        private System.Windows.Forms.GroupBox groupBoxMergeShortLines;
        private System.Windows.Forms.CheckBox checkBoxOnlyContinuationLines;
        private System.Windows.Forms.NumericUpDown numericUpDownMaxCharacters;
        private System.Windows.Forms.NumericUpDown numericUpDownMaxMillisecondsBetweenLines;
        private System.Windows.Forms.Label labelMaxMillisecondsBetweenLines;
        private System.Windows.Forms.Label labelMaxCharacters;
        private System.Windows.Forms.Button buttonSwapFrameRate;
        private System.Windows.Forms.GroupBox groupBoxChangeCasing;
        private System.Windows.Forms.RadioButton radioButtonFixOnlyNames;
        private System.Windows.Forms.RadioButton radioButtonLowercase;
        private System.Windows.Forms.RadioButton radioButtonUppercase;
        private System.Windows.Forms.RadioButton radioButtonNormal;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripOptions;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSelectAll;
        private System.Windows.Forms.ToolStripMenuItem inverseSelectionToolStripMenuItem;
        private System.Windows.Forms.Button buttonBrowseEncoding;
        private System.Windows.Forms.Label labelNumberOfFiles;
        private System.Windows.Forms.ToolStripMenuItem openContainingFolderToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem convertMkvSettingsToolStripMenuItem;
        private System.Windows.Forms.GroupBox groupBoxRemoveStyle;
        private System.Windows.Forms.TextBox textBoxRemoveStyle;
        private System.Windows.Forms.Label labelStyleName;
        private System.Windows.Forms.GroupBox groupBoxAdjustDuration;
        private System.Windows.Forms.Label labelAdjustDurationVia;
        private System.Windows.Forms.ComboBox comboBoxAdjustDurationVia;
        private System.Windows.Forms.Panel panelAdjustDurationAddPercent;
        private System.Windows.Forms.Panel panelAdjustDurationAddSeconds;
        private System.Windows.Forms.NumericUpDown numericUpDownSeconds;
        private System.Windows.Forms.Label labelAddSeconds;
        private System.Windows.Forms.Panel panelAdjustDurationFixed;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numericUpDownAdjustViaPercent;
        private System.Windows.Forms.Label labelAdjustViaPercent;
        private System.Windows.Forms.NumericUpDown numericUpDownFixedMilliseconds;
        private System.Windows.Forms.Label labelMillisecondsFixed;
        private System.Windows.Forms.Panel panelAdjustDurationRecalc;
        private System.Windows.Forms.CheckBox checkBoxExtendOnly;
        private System.Windows.Forms.NumericUpDown numericUpDownOptimalCharsSec;
        private System.Windows.Forms.Label labelOptimalCharsSec;
        private System.Windows.Forms.NumericUpDown numericUpDownMaxCharsSec;
        private System.Windows.Forms.Label labelMaxCharsPerSecond;
        private System.Windows.Forms.ToolStripMenuItem addFilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.GroupBox groupBoxDeleteLines;
        private System.Windows.Forms.TextBox textBoxDeleteContains;
        private System.Windows.Forms.Label labelDeleteLinesContaining;
        private System.Windows.Forms.NumericUpDown numericUpDownDeleteLast;
        private System.Windows.Forms.Label labelDeleteLastLines;
        private System.Windows.Forms.NumericUpDown numericUpDownDeleteFirst;
        private System.Windows.Forms.Label labelDeleteFirstLines;
    }
}