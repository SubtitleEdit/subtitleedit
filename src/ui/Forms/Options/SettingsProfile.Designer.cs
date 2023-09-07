namespace Nikse.SubtitleEdit.Forms.Options
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
            this.components = new System.ComponentModel.Container();
            this.groupBoxStyles = new System.Windows.Forms.GroupBox();
            this.labelName = new System.Windows.Forms.Label();
            this.textBoxName = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.groupBoxGeneralRules = new System.Windows.Forms.GroupBox();
            this.comboBoxCpsLineLenCalc = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.buttonEditCustomContinuationStyle = new System.Windows.Forms.Button();
            this.comboBoxContinuationStyle = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.comboBoxDialogStyle = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.comboBoxMergeShortLineLength = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.numericUpDownMaxNumberOfLines = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.numericUpDownMinGapMs = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.numericUpDownDurationMax = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.numericUpDownDurationMin = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.numericUpDownMaxWordsMin = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.numericUpDownMaxCharsSec = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.numericUpDownSubtitleLineMaximumLength = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelCpsLineLenCalc = new System.Windows.Forms.Label();
            this.labelContinuationStyle = new System.Windows.Forms.Label();
            this.labelDialogStyle = new System.Windows.Forms.Label();
            this.labelOptimalCharsPerSecond = new System.Windows.Forms.Label();
            this.numericUpDownOptimalCharsSec = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelSubMaxLen = new System.Windows.Forms.Label();
            this.labelMergeShortLines = new System.Windows.Forms.Label();
            this.labelMaxWordsPerMin = new System.Windows.Forms.Label();
            this.labelMinDuration = new System.Windows.Forms.Label();
            this.labelMaxDuration = new System.Windows.Forms.Label();
            this.labelMaxLines = new System.Windows.Forms.Label();
            this.labelMaxCharsPerSecond = new System.Windows.Forms.Label();
            this.labelMinGapMs = new System.Windows.Forms.Label();
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
            this.toolTipContinuationPreview = new System.Windows.Forms.ToolTip(this.components);
            this.toolTipDialogStylePreview = new System.Windows.Forms.ToolTip(this.components);
            this.groupBoxStyles.SuspendLayout();
            this.groupBoxGeneralRules.SuspendLayout();
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
            this.groupBoxStyles.Size = new System.Drawing.Size(968, 469);
            this.groupBoxStyles.TabIndex = 1;
            this.groupBoxStyles.TabStop = false;
            // 
            // labelName
            // 
            this.labelName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelName.AutoSize = true;
            this.labelName.Location = new System.Drawing.Point(659, 16);
            this.labelName.Name = "labelName";
            this.labelName.Size = new System.Drawing.Size(35, 13);
            this.labelName.TabIndex = 53;
            this.labelName.Text = "Name";
            // 
            // textBoxName
            // 
            this.textBoxName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxName.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxName.Location = new System.Drawing.Point(660, 35);
            this.textBoxName.Name = "textBoxName";
            this.textBoxName.Size = new System.Drawing.Size(250, 20);
            this.textBoxName.TabIndex = 80;
            this.textBoxName.TextChanged += new System.EventHandler(this.textBoxName_TextChanged);
            // 
            // groupBoxGeneralRules
            // 
            this.groupBoxGeneralRules.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxGeneralRules.Controls.Add(this.comboBoxCpsLineLenCalc);
            this.groupBoxGeneralRules.Controls.Add(this.buttonEditCustomContinuationStyle);
            this.groupBoxGeneralRules.Controls.Add(this.comboBoxContinuationStyle);
            this.groupBoxGeneralRules.Controls.Add(this.comboBoxDialogStyle);
            this.groupBoxGeneralRules.Controls.Add(this.comboBoxMergeShortLineLength);
            this.groupBoxGeneralRules.Controls.Add(this.numericUpDownMaxNumberOfLines);
            this.groupBoxGeneralRules.Controls.Add(this.numericUpDownMinGapMs);
            this.groupBoxGeneralRules.Controls.Add(this.numericUpDownDurationMax);
            this.groupBoxGeneralRules.Controls.Add(this.numericUpDownDurationMin);
            this.groupBoxGeneralRules.Controls.Add(this.numericUpDownMaxWordsMin);
            this.groupBoxGeneralRules.Controls.Add(this.numericUpDownMaxCharsSec);
            this.groupBoxGeneralRules.Controls.Add(this.numericUpDownSubtitleLineMaximumLength);
            this.groupBoxGeneralRules.Controls.Add(this.labelCpsLineLenCalc);
            this.groupBoxGeneralRules.Controls.Add(this.labelContinuationStyle);
            this.groupBoxGeneralRules.Controls.Add(this.labelDialogStyle);
            this.groupBoxGeneralRules.Controls.Add(this.labelOptimalCharsPerSecond);
            this.groupBoxGeneralRules.Controls.Add(this.numericUpDownOptimalCharsSec);
            this.groupBoxGeneralRules.Controls.Add(this.labelSubMaxLen);
            this.groupBoxGeneralRules.Controls.Add(this.labelMergeShortLines);
            this.groupBoxGeneralRules.Controls.Add(this.labelMaxWordsPerMin);
            this.groupBoxGeneralRules.Controls.Add(this.labelMinDuration);
            this.groupBoxGeneralRules.Controls.Add(this.labelMaxDuration);
            this.groupBoxGeneralRules.Controls.Add(this.labelMaxLines);
            this.groupBoxGeneralRules.Controls.Add(this.labelMaxCharsPerSecond);
            this.groupBoxGeneralRules.Controls.Add(this.labelMinGapMs);
            this.groupBoxGeneralRules.Location = new System.Drawing.Point(615, 63);
            this.groupBoxGeneralRules.Name = "groupBoxGeneralRules";
            this.groupBoxGeneralRules.Size = new System.Drawing.Size(347, 390);
            this.groupBoxGeneralRules.TabIndex = 90;
            this.groupBoxGeneralRules.TabStop = false;
            this.groupBoxGeneralRules.Text = "Rules";
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
            this.comboBoxCpsLineLenCalc.DropDownWidth = 170;
            this.comboBoxCpsLineLenCalc.FormattingEnabled = true;
            this.comboBoxCpsLineLenCalc.Location = new System.Drawing.Point(132, 339);
            this.comboBoxCpsLineLenCalc.MaxLength = 32767;
            this.comboBoxCpsLineLenCalc.Name = "comboBoxCpsLineLenCalc";
            this.comboBoxCpsLineLenCalc.SelectedIndex = -1;
            this.comboBoxCpsLineLenCalc.SelectedItem = null;
            this.comboBoxCpsLineLenCalc.SelectedText = "";
            this.comboBoxCpsLineLenCalc.Size = new System.Drawing.Size(174, 23);
            this.comboBoxCpsLineLenCalc.TabIndex = 197;
            this.comboBoxCpsLineLenCalc.UsePopupWindow = false;
            // 
            // buttonEditCustomContinuationStyle
            // 
            this.buttonEditCustomContinuationStyle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonEditCustomContinuationStyle.Location = new System.Drawing.Point(307, 309);
            this.buttonEditCustomContinuationStyle.Name = "buttonEditCustomContinuationStyle";
            this.buttonEditCustomContinuationStyle.Size = new System.Drawing.Size(28, 23);
            this.buttonEditCustomContinuationStyle.TabIndex = 196;
            this.buttonEditCustomContinuationStyle.Text = "...";
            this.buttonEditCustomContinuationStyle.UseVisualStyleBackColor = true;
            this.buttonEditCustomContinuationStyle.Visible = false;
            this.buttonEditCustomContinuationStyle.Click += new System.EventHandler(this.buttonEditCustomContinuationStyle_Click);
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
            this.comboBoxContinuationStyle.DropDownWidth = 260;
            this.comboBoxContinuationStyle.FormattingEnabled = true;
            this.comboBoxContinuationStyle.Location = new System.Drawing.Point(132, 310);
            this.comboBoxContinuationStyle.MaxLength = 32767;
            this.comboBoxContinuationStyle.Name = "comboBoxContinuationStyle";
            this.comboBoxContinuationStyle.SelectedIndex = -1;
            this.comboBoxContinuationStyle.SelectedItem = null;
            this.comboBoxContinuationStyle.SelectedText = "";
            this.comboBoxContinuationStyle.Size = new System.Drawing.Size(157, 23);
            this.comboBoxContinuationStyle.TabIndex = 195;
            this.comboBoxContinuationStyle.UsePopupWindow = false;
            this.comboBoxContinuationStyle.SelectedIndexChanged += new System.EventHandler(this.UiElementChanged);
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
            this.comboBoxDialogStyle.DropDownWidth = 260;
            this.comboBoxDialogStyle.FormattingEnabled = true;
            this.comboBoxDialogStyle.Items.AddRange(new object[] {
            "Dash both lines with space",
            "Dash both lines without space",
            "Dash second line with space",
            "Dash second line without space"});
            this.comboBoxDialogStyle.Location = new System.Drawing.Point(103, 281);
            this.comboBoxDialogStyle.MaxLength = 32767;
            this.comboBoxDialogStyle.Name = "comboBoxDialogStyle";
            this.comboBoxDialogStyle.SelectedIndex = -1;
            this.comboBoxDialogStyle.SelectedItem = null;
            this.comboBoxDialogStyle.SelectedText = "";
            this.comboBoxDialogStyle.Size = new System.Drawing.Size(203, 23);
            this.comboBoxDialogStyle.TabIndex = 194;
            this.comboBoxDialogStyle.UsePopupWindow = false;
            this.comboBoxDialogStyle.SelectedIndexChanged += new System.EventHandler(this.UiElementChanged);
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
            this.comboBoxMergeShortLineLength.Location = new System.Drawing.Point(233, 252);
            this.comboBoxMergeShortLineLength.MaxLength = 32767;
            this.comboBoxMergeShortLineLength.Name = "comboBoxMergeShortLineLength";
            this.comboBoxMergeShortLineLength.SelectedIndex = -1;
            this.comboBoxMergeShortLineLength.SelectedItem = null;
            this.comboBoxMergeShortLineLength.SelectedText = "";
            this.comboBoxMergeShortLineLength.Size = new System.Drawing.Size(73, 23);
            this.comboBoxMergeShortLineLength.TabIndex = 180;
            this.comboBoxMergeShortLineLength.UsePopupWindow = false;
            this.comboBoxMergeShortLineLength.SelectedIndexChanged += new System.EventHandler(this.UiElementChanged);
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
            this.numericUpDownMaxNumberOfLines.Location = new System.Drawing.Point(233, 223);
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
            this.numericUpDownMaxNumberOfLines.Size = new System.Drawing.Size(73, 23);
            this.numericUpDownMaxNumberOfLines.TabIndex = 170;
            this.numericUpDownMaxNumberOfLines.TabStop = false;
            this.numericUpDownMaxNumberOfLines.ThousandsSeparator = false;
            this.numericUpDownMaxNumberOfLines.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numericUpDownMaxNumberOfLines.ValueChanged += new System.EventHandler(this.UiElementChanged);
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
            this.numericUpDownMinGapMs.Location = new System.Drawing.Point(233, 194);
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
            this.numericUpDownMinGapMs.Size = new System.Drawing.Size(73, 23);
            this.numericUpDownMinGapMs.TabIndex = 160;
            this.numericUpDownMinGapMs.TabStop = false;
            this.numericUpDownMinGapMs.ThousandsSeparator = false;
            this.numericUpDownMinGapMs.Value = new decimal(new int[] {
            25,
            0,
            0,
            0});
            this.numericUpDownMinGapMs.ValueChanged += new System.EventHandler(this.UiElementChanged);
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
            this.numericUpDownDurationMax.Location = new System.Drawing.Point(233, 165);
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
            this.numericUpDownDurationMax.Size = new System.Drawing.Size(73, 23);
            this.numericUpDownDurationMax.TabIndex = 150;
            this.numericUpDownDurationMax.TabStop = false;
            this.numericUpDownDurationMax.ThousandsSeparator = false;
            this.numericUpDownDurationMax.Value = new decimal(new int[] {
            50000,
            0,
            0,
            0});
            this.numericUpDownDurationMax.ValueChanged += new System.EventHandler(this.UiElementChanged);
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
            this.numericUpDownDurationMin.Location = new System.Drawing.Point(233, 136);
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
            this.numericUpDownDurationMin.Size = new System.Drawing.Size(73, 23);
            this.numericUpDownDurationMin.TabIndex = 140;
            this.numericUpDownDurationMin.TabStop = false;
            this.numericUpDownDurationMin.ThousandsSeparator = false;
            this.numericUpDownDurationMin.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDownDurationMin.ValueChanged += new System.EventHandler(this.UiElementChanged);
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
            this.numericUpDownMaxWordsMin.Location = new System.Drawing.Point(233, 107);
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
            this.numericUpDownMaxWordsMin.Size = new System.Drawing.Size(73, 23);
            this.numericUpDownMaxWordsMin.TabIndex = 130;
            this.numericUpDownMaxWordsMin.TabStop = false;
            this.numericUpDownMaxWordsMin.ThousandsSeparator = false;
            this.numericUpDownMaxWordsMin.Value = new decimal(new int[] {
            300,
            0,
            0,
            0});
            this.numericUpDownMaxWordsMin.ValueChanged += new System.EventHandler(this.UiElementChanged);
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
            this.numericUpDownMaxCharsSec.Location = new System.Drawing.Point(233, 78);
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
            this.numericUpDownMaxCharsSec.Size = new System.Drawing.Size(73, 23);
            this.numericUpDownMaxCharsSec.TabIndex = 120;
            this.numericUpDownMaxCharsSec.TabStop = false;
            this.numericUpDownMaxCharsSec.ThousandsSeparator = false;
            this.numericUpDownMaxCharsSec.Value = new decimal(new int[] {
            24,
            0,
            0,
            0});
            this.numericUpDownMaxCharsSec.ValueChanged += new System.EventHandler(this.UiElementChanged);
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
            this.numericUpDownSubtitleLineMaximumLength.Location = new System.Drawing.Point(233, 20);
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
            this.numericUpDownSubtitleLineMaximumLength.Size = new System.Drawing.Size(73, 23);
            this.numericUpDownSubtitleLineMaximumLength.TabIndex = 100;
            this.numericUpDownSubtitleLineMaximumLength.TabStop = false;
            this.numericUpDownSubtitleLineMaximumLength.ThousandsSeparator = false;
            this.numericUpDownSubtitleLineMaximumLength.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDownSubtitleLineMaximumLength.ValueChanged += new System.EventHandler(this.UiElementChanged);
            // 
            // labelCpsLineLenCalc
            // 
            this.labelCpsLineLenCalc.AutoSize = true;
            this.labelCpsLineLenCalc.Location = new System.Drawing.Point(6, 342);
            this.labelCpsLineLenCalc.Name = "labelCpsLineLenCalc";
            this.labelCpsLineLenCalc.Size = new System.Drawing.Size(102, 13);
            this.labelCpsLineLenCalc.TabIndex = 196;
            this.labelCpsLineLenCalc.Text = "Cps/line length style";
            // 
            // labelContinuationStyle
            // 
            this.labelContinuationStyle.AutoSize = true;
            this.labelContinuationStyle.Location = new System.Drawing.Point(6, 314);
            this.labelContinuationStyle.Name = "labelContinuationStyle";
            this.labelContinuationStyle.Size = new System.Drawing.Size(90, 13);
            this.labelContinuationStyle.TabIndex = 195;
            this.labelContinuationStyle.Text = "Continuation style";
            // 
            // labelDialogStyle
            // 
            this.labelDialogStyle.AutoSize = true;
            this.labelDialogStyle.Location = new System.Drawing.Point(6, 284);
            this.labelDialogStyle.Name = "labelDialogStyle";
            this.labelDialogStyle.Size = new System.Drawing.Size(61, 13);
            this.labelDialogStyle.TabIndex = 193;
            this.labelDialogStyle.Text = "Dialog style";
            // 
            // labelOptimalCharsPerSecond
            // 
            this.labelOptimalCharsPerSecond.AutoSize = true;
            this.labelOptimalCharsPerSecond.Location = new System.Drawing.Point(6, 51);
            this.labelOptimalCharsPerSecond.Name = "labelOptimalCharsPerSecond";
            this.labelOptimalCharsPerSecond.Size = new System.Drawing.Size(93, 13);
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
            this.numericUpDownOptimalCharsSec.Location = new System.Drawing.Point(233, 49);
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
            this.numericUpDownOptimalCharsSec.Size = new System.Drawing.Size(73, 23);
            this.numericUpDownOptimalCharsSec.TabIndex = 110;
            this.numericUpDownOptimalCharsSec.TabStop = false;
            this.numericUpDownOptimalCharsSec.ThousandsSeparator = false;
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
            this.labelSubMaxLen.Location = new System.Drawing.Point(6, 22);
            this.labelSubMaxLen.Name = "labelSubMaxLen";
            this.labelSubMaxLen.Size = new System.Drawing.Size(99, 13);
            this.labelSubMaxLen.TabIndex = 6;
            this.labelSubMaxLen.Text = "Subtitle max. length";
            // 
            // labelMergeShortLines
            // 
            this.labelMergeShortLines.AutoSize = true;
            this.labelMergeShortLines.Location = new System.Drawing.Point(6, 255);
            this.labelMergeShortLines.Name = "labelMergeShortLines";
            this.labelMergeShortLines.Size = new System.Drawing.Size(120, 13);
            this.labelMergeShortLines.TabIndex = 16;
            this.labelMergeShortLines.Text = "Merge lines shorter than";
            // 
            // labelMaxWordsPerMin
            // 
            this.labelMaxWordsPerMin.AutoSize = true;
            this.labelMaxWordsPerMin.Location = new System.Drawing.Point(6, 109);
            this.labelMaxWordsPerMin.Name = "labelMaxWordsPerMin";
            this.labelMaxWordsPerMin.Size = new System.Drawing.Size(82, 13);
            this.labelMaxWordsPerMin.TabIndex = 49;
            this.labelMaxWordsPerMin.Text = "Max. words/min";
            // 
            // labelMinDuration
            // 
            this.labelMinDuration.AutoSize = true;
            this.labelMinDuration.Location = new System.Drawing.Point(6, 138);
            this.labelMinDuration.Name = "labelMinDuration";
            this.labelMinDuration.Size = new System.Drawing.Size(130, 13);
            this.labelMinDuration.TabIndex = 10;
            this.labelMinDuration.Text = "Min. duration, milliseconds";
            // 
            // labelMaxDuration
            // 
            this.labelMaxDuration.AutoSize = true;
            this.labelMaxDuration.Location = new System.Drawing.Point(6, 167);
            this.labelMaxDuration.Name = "labelMaxDuration";
            this.labelMaxDuration.Size = new System.Drawing.Size(133, 13);
            this.labelMaxDuration.TabIndex = 12;
            this.labelMaxDuration.Text = "Max. duration, milliseconds";
            // 
            // labelMaxLines
            // 
            this.labelMaxLines.AutoSize = true;
            this.labelMaxLines.Location = new System.Drawing.Point(6, 225);
            this.labelMaxLines.Name = "labelMaxLines";
            this.labelMaxLines.Size = new System.Drawing.Size(104, 13);
            this.labelMaxLines.TabIndex = 47;
            this.labelMaxLines.Text = "Max. number of lines";
            // 
            // labelMaxCharsPerSecond
            // 
            this.labelMaxCharsPerSecond.AutoSize = true;
            this.labelMaxCharsPerSecond.Location = new System.Drawing.Point(6, 80);
            this.labelMaxCharsPerSecond.Name = "labelMaxCharsPerSecond";
            this.labelMaxCharsPerSecond.Size = new System.Drawing.Size(81, 13);
            this.labelMaxCharsPerSecond.TabIndex = 9;
            this.labelMaxCharsPerSecond.Text = "Max. chars/sec";
            // 
            // labelMinGapMs
            // 
            this.labelMinGapMs.AutoSize = true;
            this.labelMinGapMs.Location = new System.Drawing.Point(6, 194);
            this.labelMinGapMs.Name = "labelMinGapMs";
            this.labelMinGapMs.Size = new System.Drawing.Size(133, 13);
            this.labelMinGapMs.TabIndex = 14;
            this.labelMinGapMs.Text = "Min. gap between subtitles";
            // 
            // buttonExport
            // 
            this.buttonExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonExport.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonExport.Location = new System.Drawing.Point(6, 428);
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
            this.buttonImport.Location = new System.Drawing.Point(94, 428);
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
            this.buttonCopy.Location = new System.Drawing.Point(182, 428);
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
            this.buttonRemoveAll.Location = new System.Drawing.Point(446, 428);
            this.buttonRemoveAll.Name = "buttonRemoveAll";
            this.buttonRemoveAll.Size = new System.Drawing.Size(123, 23);
            this.buttonRemoveAll.TabIndex = 70;
            this.buttonRemoveAll.Text = "Remove all";
            this.buttonRemoveAll.UseVisualStyleBackColor = true;
            this.buttonRemoveAll.Click += new System.EventHandler(this.buttonRemoveAll_Click);
            // 
            // buttonAdd
            // 
            this.buttonAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonAdd.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonAdd.Location = new System.Drawing.Point(270, 428);
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
            this.buttonRemove.Location = new System.Drawing.Point(358, 428);
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
            this.listViewProfiles.Size = new System.Drawing.Size(603, 402);
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
            this.buttonCancel.Location = new System.Drawing.Point(899, 492);
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
            this.buttonOK.Location = new System.Drawing.Point(818, 492);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 200;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // toolTipContinuationPreview
            // 
            this.toolTipContinuationPreview.AutoPopDelay = 60000;
            this.toolTipContinuationPreview.InitialDelay = 500;
            this.toolTipContinuationPreview.ReshowDelay = 100;
            // 
            // toolTipDialogStylePreview
            // 
            this.toolTipDialogStylePreview.AutoPopDelay = 60000;
            this.toolTipDialogStylePreview.InitialDelay = 500;
            this.toolTipDialogStylePreview.ReshowDelay = 100;
            // 
            // SettingsProfile
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(986, 527);
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
            this.Shown += new System.EventHandler(this.SettingsProfile_Shown);
            this.ResizeEnd += new System.EventHandler(this.SettingsProfile_ResizeEnd);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SettingsProfile_KeyDown);
            this.groupBoxStyles.ResumeLayout(false);
            this.groupBoxStyles.PerformLayout();
            this.groupBoxGeneralRules.ResumeLayout(false);
            this.groupBoxGeneralRules.PerformLayout();
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
        private Nikse.SubtitleEdit.Controls.NikseTextBox textBoxName;
        private System.Windows.Forms.GroupBox groupBoxGeneralRules;
        private System.Windows.Forms.Label labelOptimalCharsPerSecond;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownOptimalCharsSec;
        private System.Windows.Forms.Label labelSubMaxLen;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownMaxWordsMin;
        private System.Windows.Forms.Label labelMergeShortLines;
        private System.Windows.Forms.Label labelMaxWordsPerMin;
        private System.Windows.Forms.Label labelMinDuration;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownMaxNumberOfLines;
        private System.Windows.Forms.Label labelMaxDuration;
        private System.Windows.Forms.Label labelMaxLines;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownDurationMin;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownDurationMax;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxMergeShortLineLength;
        private System.Windows.Forms.Label labelMaxCharsPerSecond;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownMinGapMs;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownMaxCharsSec;
        private System.Windows.Forms.Label labelMinGapMs;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownSubtitleLineMaximumLength;
        private System.Windows.Forms.OpenFileDialog openFileDialogImport;
        private System.Windows.Forms.Label labelDialogStyle;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxDialogStyle;
        private System.Windows.Forms.Label labelContinuationStyle;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxContinuationStyle;
        private System.Windows.Forms.ToolTip toolTipContinuationPreview;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxCpsLineLenCalc;
        private System.Windows.Forms.Label labelCpsLineLenCalc;
        private System.Windows.Forms.ToolTip toolTipDialogStylePreview;
        private System.Windows.Forms.Button buttonEditCustomContinuationStyle;
    }
}