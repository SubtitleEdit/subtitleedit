namespace Nikse.SubtitleEdit.Forms.SeJobs
{
    partial class SeJobExport
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
            this.checkBoxIncludeShotChanges = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxJobId = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.labelSubtitleFileName = new System.Windows.Forms.Label();
            this.textBoxJobDescription = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.labelDescription = new System.Windows.Forms.Label();
            this.checkBoxIncludeRules = new System.Windows.Forms.CheckBox();
            this.checkBoxIncludeWaveform = new System.Windows.Forms.CheckBox();
            this.checkBoxIncludeBookmarks = new System.Windows.Forms.CheckBox();
            this.textBoxSubtitleFileName = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.buttonGapChoose = new System.Windows.Forms.Button();
            this.labelOptimalCharsPerSecond = new System.Windows.Forms.Label();
            this.numericUpDownOptimalCharsSec = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelSubMaxLen = new System.Windows.Forms.Label();
            this.numericUpDownMaxWordsMin = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelMaxWordsPerMin = new System.Windows.Forms.Label();
            this.labelMinDuration = new System.Windows.Forms.Label();
            this.numericUpDownMaxNumberOfLines = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelMaxDuration = new System.Windows.Forms.Label();
            this.labelMaxLines = new System.Windows.Forms.Label();
            this.numericUpDownDurationMin = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.numericUpDownDurationMax = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelMaxCharsPerSecond = new System.Windows.Forms.Label();
            this.numericUpDownMinGapMs = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.numericUpDownMaxCharsSec = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelMinGapMs = new System.Windows.Forms.Label();
            this.numericUpDownSubtitleLineMaximumLength = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.textBoxJobName = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxVideoUrl = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.labelVideoUrl = new System.Windows.Forms.Label();
            this.labelSubtitleTotalCount = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonSave = new System.Windows.Forms.Button();
            this.buttonImport = new System.Windows.Forms.Button();
            this.checkBoxOriginal = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // checkBoxIncludeShotChanges
            // 
            this.checkBoxIncludeShotChanges.AutoSize = true;
            this.checkBoxIncludeShotChanges.Location = new System.Drawing.Point(166, 315);
            this.checkBoxIncludeShotChanges.Name = "checkBoxIncludeShotChanges";
            this.checkBoxIncludeShotChanges.Size = new System.Drawing.Size(128, 17);
            this.checkBoxIncludeShotChanges.TabIndex = 13;
            this.checkBoxIncludeShotChanges.Text = "Include shot changes";
            this.checkBoxIncludeShotChanges.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Job id";
            // 
            // textBoxJobId
            // 
            this.textBoxJobId.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxJobId.Location = new System.Drawing.Point(166, 19);
            this.textBoxJobId.Name = "textBoxJobId";
            this.textBoxJobId.ReadOnly = true;
            this.textBoxJobId.Size = new System.Drawing.Size(391, 20);
            this.textBoxJobId.TabIndex = 1;
            // 
            // labelSubtitleFileName
            // 
            this.labelSubtitleFileName.AutoSize = true;
            this.labelSubtitleFileName.Location = new System.Drawing.Point(12, 182);
            this.labelSubtitleFileName.Name = "labelSubtitleFileName";
            this.labelSubtitleFileName.Size = new System.Drawing.Size(87, 13);
            this.labelSubtitleFileName.TabIndex = 6;
            this.labelSubtitleFileName.Text = "Subtitle file name";
            // 
            // textBoxJobDescription
            // 
            this.textBoxJobDescription.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxJobDescription.Location = new System.Drawing.Point(166, 72);
            this.textBoxJobDescription.Multiline = true;
            this.textBoxJobDescription.Name = "textBoxJobDescription";
            this.textBoxJobDescription.Size = new System.Drawing.Size(391, 88);
            this.textBoxJobDescription.TabIndex = 5;
            // 
            // labelDescription
            // 
            this.labelDescription.AutoSize = true;
            this.labelDescription.Location = new System.Drawing.Point(12, 75);
            this.labelDescription.Name = "labelDescription";
            this.labelDescription.Size = new System.Drawing.Size(60, 13);
            this.labelDescription.TabIndex = 4;
            this.labelDescription.Text = "Description";
            // 
            // checkBoxIncludeRules
            // 
            this.checkBoxIncludeRules.AutoSize = true;
            this.checkBoxIncludeRules.Location = new System.Drawing.Point(597, 22);
            this.checkBoxIncludeRules.Name = "checkBoxIncludeRules";
            this.checkBoxIncludeRules.Size = new System.Drawing.Size(86, 17);
            this.checkBoxIncludeRules.TabIndex = 15;
            this.checkBoxIncludeRules.Text = "Include rules";
            this.checkBoxIncludeRules.UseVisualStyleBackColor = true;
            // 
            // checkBoxIncludeWaveform
            // 
            this.checkBoxIncludeWaveform.AutoSize = true;
            this.checkBoxIncludeWaveform.Location = new System.Drawing.Point(166, 292);
            this.checkBoxIncludeWaveform.Name = "checkBoxIncludeWaveform";
            this.checkBoxIncludeWaveform.Size = new System.Drawing.Size(110, 17);
            this.checkBoxIncludeWaveform.TabIndex = 12;
            this.checkBoxIncludeWaveform.Text = "Include waveform";
            this.checkBoxIncludeWaveform.UseVisualStyleBackColor = true;
            // 
            // checkBoxIncludeBookmarks
            // 
            this.checkBoxIncludeBookmarks.AutoSize = true;
            this.checkBoxIncludeBookmarks.Location = new System.Drawing.Point(166, 338);
            this.checkBoxIncludeBookmarks.Name = "checkBoxIncludeBookmarks";
            this.checkBoxIncludeBookmarks.Size = new System.Drawing.Size(116, 17);
            this.checkBoxIncludeBookmarks.TabIndex = 14;
            this.checkBoxIncludeBookmarks.Text = "Include bookmarks";
            this.checkBoxIncludeBookmarks.UseVisualStyleBackColor = true;
            // 
            // textBoxSubtitleFileName
            // 
            this.textBoxSubtitleFileName.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxSubtitleFileName.Location = new System.Drawing.Point(166, 175);
            this.textBoxSubtitleFileName.Name = "textBoxSubtitleFileName";
            this.textBoxSubtitleFileName.Size = new System.Drawing.Size(391, 20);
            this.textBoxSubtitleFileName.TabIndex = 7;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.buttonGapChoose);
            this.groupBox1.Controls.Add(this.labelOptimalCharsPerSecond);
            this.groupBox1.Controls.Add(this.numericUpDownOptimalCharsSec);
            this.groupBox1.Controls.Add(this.labelSubMaxLen);
            this.groupBox1.Controls.Add(this.numericUpDownMaxWordsMin);
            this.groupBox1.Controls.Add(this.labelMaxWordsPerMin);
            this.groupBox1.Controls.Add(this.labelMinDuration);
            this.groupBox1.Controls.Add(this.numericUpDownMaxNumberOfLines);
            this.groupBox1.Controls.Add(this.labelMaxDuration);
            this.groupBox1.Controls.Add(this.labelMaxLines);
            this.groupBox1.Controls.Add(this.numericUpDownDurationMin);
            this.groupBox1.Controls.Add(this.numericUpDownDurationMax);
            this.groupBox1.Controls.Add(this.labelMaxCharsPerSecond);
            this.groupBox1.Controls.Add(this.numericUpDownMinGapMs);
            this.groupBox1.Controls.Add(this.numericUpDownMaxCharsSec);
            this.groupBox1.Controls.Add(this.labelMinGapMs);
            this.groupBox1.Controls.Add(this.numericUpDownSubtitleLineMaximumLength);
            this.groupBox1.Location = new System.Drawing.Point(597, 48);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(337, 276);
            this.groupBox1.TabIndex = 17;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Rules";
            // 
            // buttonGapChoose
            // 
            this.buttonGapChoose.Location = new System.Drawing.Point(281, 195);
            this.buttonGapChoose.Name = "buttonGapChoose";
            this.buttonGapChoose.Size = new System.Drawing.Size(28, 23);
            this.buttonGapChoose.TabIndex = 16;
            this.buttonGapChoose.Text = "...";
            this.buttonGapChoose.UseVisualStyleBackColor = true;
            this.buttonGapChoose.Click += new System.EventHandler(this.buttonGapChoose_Click);
            // 
            // labelOptimalCharsPerSecond
            // 
            this.labelOptimalCharsPerSecond.AutoSize = true;
            this.labelOptimalCharsPerSecond.Location = new System.Drawing.Point(23, 56);
            this.labelOptimalCharsPerSecond.Name = "labelOptimalCharsPerSecond";
            this.labelOptimalCharsPerSecond.Size = new System.Drawing.Size(93, 13);
            this.labelOptimalCharsPerSecond.TabIndex = 2;
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
            this.numericUpDownOptimalCharsSec.Location = new System.Drawing.Point(220, 54);
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
            this.numericUpDownOptimalCharsSec.Size = new System.Drawing.Size(56, 23);
            this.numericUpDownOptimalCharsSec.TabIndex = 3;
            this.numericUpDownOptimalCharsSec.TabStop = false;
            this.numericUpDownOptimalCharsSec.ThousandsSeparator = false;
            this.numericUpDownOptimalCharsSec.Value = new decimal(new int[] {
            11,
            0,
            0,
            0});
            // 
            // labelSubMaxLen
            // 
            this.labelSubMaxLen.AutoSize = true;
            this.labelSubMaxLen.Location = new System.Drawing.Point(23, 27);
            this.labelSubMaxLen.Name = "labelSubMaxLen";
            this.labelSubMaxLen.Size = new System.Drawing.Size(99, 13);
            this.labelSubMaxLen.TabIndex = 0;
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
            this.numericUpDownMaxWordsMin.Location = new System.Drawing.Point(220, 112);
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
            this.numericUpDownMaxWordsMin.Size = new System.Drawing.Size(56, 23);
            this.numericUpDownMaxWordsMin.TabIndex = 8;
            this.numericUpDownMaxWordsMin.TabStop = false;
            this.numericUpDownMaxWordsMin.ThousandsSeparator = false;
            this.numericUpDownMaxWordsMin.Value = new decimal(new int[] {
            300,
            0,
            0,
            0});
            // 
            // labelMaxWordsPerMin
            // 
            this.labelMaxWordsPerMin.AutoSize = true;
            this.labelMaxWordsPerMin.Location = new System.Drawing.Point(23, 114);
            this.labelMaxWordsPerMin.Name = "labelMaxWordsPerMin";
            this.labelMaxWordsPerMin.Size = new System.Drawing.Size(82, 13);
            this.labelMaxWordsPerMin.TabIndex = 6;
            this.labelMaxWordsPerMin.Text = "Max. words/min";
            // 
            // labelMinDuration
            // 
            this.labelMinDuration.AutoSize = true;
            this.labelMinDuration.Location = new System.Drawing.Point(23, 143);
            this.labelMinDuration.Name = "labelMinDuration";
            this.labelMinDuration.Size = new System.Drawing.Size(130, 13);
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
            this.numericUpDownMaxNumberOfLines.Location = new System.Drawing.Point(220, 226);
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
            this.numericUpDownMaxNumberOfLines.Size = new System.Drawing.Size(56, 23);
            this.numericUpDownMaxNumberOfLines.TabIndex = 1;
            this.numericUpDownMaxNumberOfLines.TabStop = false;
            this.numericUpDownMaxNumberOfLines.ThousandsSeparator = false;
            this.numericUpDownMaxNumberOfLines.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // labelMaxDuration
            // 
            this.labelMaxDuration.AutoSize = true;
            this.labelMaxDuration.Location = new System.Drawing.Point(23, 171);
            this.labelMaxDuration.Name = "labelMaxDuration";
            this.labelMaxDuration.Size = new System.Drawing.Size(133, 13);
            this.labelMaxDuration.TabIndex = 12;
            this.labelMaxDuration.Text = "Max. duration, milliseconds";
            // 
            // labelMaxLines
            // 
            this.labelMaxLines.AutoSize = true;
            this.labelMaxLines.Location = new System.Drawing.Point(23, 228);
            this.labelMaxLines.Name = "labelMaxLines";
            this.labelMaxLines.Size = new System.Drawing.Size(104, 13);
            this.labelMaxLines.TabIndex = 0;
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
            this.numericUpDownDurationMin.Location = new System.Drawing.Point(220, 141);
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
            this.numericUpDownDurationMin.Size = new System.Drawing.Size(56, 23);
            this.numericUpDownDurationMin.TabIndex = 11;
            this.numericUpDownDurationMin.TabStop = false;
            this.numericUpDownDurationMin.ThousandsSeparator = false;
            this.numericUpDownDurationMin.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
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
            this.numericUpDownDurationMax.Location = new System.Drawing.Point(220, 169);
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
            this.numericUpDownDurationMax.Size = new System.Drawing.Size(56, 23);
            this.numericUpDownDurationMax.TabIndex = 13;
            this.numericUpDownDurationMax.TabStop = false;
            this.numericUpDownDurationMax.ThousandsSeparator = false;
            this.numericUpDownDurationMax.Value = new decimal(new int[] {
            50000,
            0,
            0,
            0});
            // 
            // labelMaxCharsPerSecond
            // 
            this.labelMaxCharsPerSecond.AutoSize = true;
            this.labelMaxCharsPerSecond.Location = new System.Drawing.Point(23, 85);
            this.labelMaxCharsPerSecond.Name = "labelMaxCharsPerSecond";
            this.labelMaxCharsPerSecond.Size = new System.Drawing.Size(81, 13);
            this.labelMaxCharsPerSecond.TabIndex = 4;
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
            this.numericUpDownMinGapMs.Location = new System.Drawing.Point(220, 197);
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
            this.numericUpDownMinGapMs.Size = new System.Drawing.Size(56, 23);
            this.numericUpDownMinGapMs.TabIndex = 15;
            this.numericUpDownMinGapMs.TabStop = false;
            this.numericUpDownMinGapMs.ThousandsSeparator = false;
            this.numericUpDownMinGapMs.Value = new decimal(new int[] {
            25,
            0,
            0,
            0});
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
            this.numericUpDownMaxCharsSec.Location = new System.Drawing.Point(220, 83);
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
            this.numericUpDownMaxCharsSec.Size = new System.Drawing.Size(56, 23);
            this.numericUpDownMaxCharsSec.TabIndex = 5;
            this.numericUpDownMaxCharsSec.TabStop = false;
            this.numericUpDownMaxCharsSec.ThousandsSeparator = false;
            this.numericUpDownMaxCharsSec.Value = new decimal(new int[] {
            24,
            0,
            0,
            0});
            // 
            // labelMinGapMs
            // 
            this.labelMinGapMs.AutoSize = true;
            this.labelMinGapMs.Location = new System.Drawing.Point(23, 199);
            this.labelMinGapMs.Name = "labelMinGapMs";
            this.labelMinGapMs.Size = new System.Drawing.Size(133, 13);
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
            this.numericUpDownSubtitleLineMaximumLength.Location = new System.Drawing.Point(220, 25);
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
            this.numericUpDownSubtitleLineMaximumLength.Size = new System.Drawing.Size(56, 23);
            this.numericUpDownSubtitleLineMaximumLength.TabIndex = 1;
            this.numericUpDownSubtitleLineMaximumLength.TabStop = false;
            this.numericUpDownSubtitleLineMaximumLength.ThousandsSeparator = false;
            this.numericUpDownSubtitleLineMaximumLength.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // textBoxJobName
            // 
            this.textBoxJobName.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxJobName.Location = new System.Drawing.Point(166, 45);
            this.textBoxJobName.Name = "textBoxJobName";
            this.textBoxJobName.Size = new System.Drawing.Size(391, 20);
            this.textBoxJobName.TabIndex = 3;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 48);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "Job name";
            // 
            // textBoxVideoUrl
            // 
            this.textBoxVideoUrl.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxVideoUrl.Location = new System.Drawing.Point(166, 250);
            this.textBoxVideoUrl.Name = "textBoxVideoUrl";
            this.textBoxVideoUrl.Size = new System.Drawing.Size(391, 20);
            this.textBoxVideoUrl.TabIndex = 11;
            // 
            // labelVideoUrl
            // 
            this.labelVideoUrl.AutoSize = true;
            this.labelVideoUrl.Location = new System.Drawing.Point(12, 257);
            this.labelVideoUrl.Name = "labelVideoUrl";
            this.labelVideoUrl.Size = new System.Drawing.Size(117, 13);
            this.labelVideoUrl.TabIndex = 10;
            this.labelVideoUrl.Text = "Video url (for streaming)";
            // 
            // labelSubtitleTotalCount
            // 
            this.labelSubtitleTotalCount.AutoSize = true;
            this.labelSubtitleTotalCount.Location = new System.Drawing.Point(163, 198);
            this.labelSubtitleTotalCount.Name = "labelSubtitleTotalCount";
            this.labelSubtitleTotalCount.Size = new System.Drawing.Size(66, 13);
            this.labelSubtitleTotalCount.TabIndex = 8;
            this.labelSubtitleTotalCount.Text = "510 subtitles";
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(865, 370);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 19;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonSave
            // 
            this.buttonSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSave.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonSave.Location = new System.Drawing.Point(742, 370);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(117, 23);
            this.buttonSave.TabIndex = 18;
            this.buttonSave.Text = "&Save as...";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // buttonImport
            // 
            this.buttonImport.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonImport.Location = new System.Drawing.Point(817, 22);
            this.buttonImport.Name = "buttonImport";
            this.buttonImport.Size = new System.Drawing.Size(117, 23);
            this.buttonImport.TabIndex = 16;
            this.buttonImport.Text = "Import...";
            this.buttonImport.UseVisualStyleBackColor = true;
            this.buttonImport.Click += new System.EventHandler(this.buttonImport_Click);
            // 
            // checkBoxOriginal
            // 
            this.checkBoxOriginal.AutoSize = true;
            this.checkBoxOriginal.Location = new System.Drawing.Point(166, 218);
            this.checkBoxOriginal.Name = "checkBoxOriginal";
            this.checkBoxOriginal.Size = new System.Drawing.Size(169, 17);
            this.checkBoxOriginal.TabIndex = 9;
            this.checkBoxOriginal.Text = "Include original (for translation)";
            this.checkBoxOriginal.UseVisualStyleBackColor = true;
            // 
            // SeJobExport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(952, 405);
            this.Controls.Add(this.checkBoxOriginal);
            this.Controls.Add(this.buttonImport);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.labelSubtitleTotalCount);
            this.Controls.Add(this.textBoxVideoUrl);
            this.Controls.Add(this.labelVideoUrl);
            this.Controls.Add(this.textBoxJobName);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.checkBoxIncludeBookmarks);
            this.Controls.Add(this.checkBoxIncludeWaveform);
            this.Controls.Add(this.checkBoxIncludeRules);
            this.Controls.Add(this.textBoxJobDescription);
            this.Controls.Add(this.labelDescription);
            this.Controls.Add(this.textBoxSubtitleFileName);
            this.Controls.Add(this.labelSubtitleFileName);
            this.Controls.Add(this.textBoxJobId);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.checkBoxIncludeShotChanges);
            this.Name = "SeJobExport";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "SE Job file export";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SeJobExport_KeyDown);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBoxIncludeShotChanges;
        private System.Windows.Forms.Label label1;
        private Nikse.SubtitleEdit.Controls.NikseTextBox textBoxJobId;
        private System.Windows.Forms.Label labelSubtitleFileName;
        private Nikse.SubtitleEdit.Controls.NikseTextBox textBoxJobDescription;
        private System.Windows.Forms.Label labelDescription;
        private System.Windows.Forms.CheckBox checkBoxIncludeRules;
        private System.Windows.Forms.CheckBox checkBoxIncludeWaveform;
        private System.Windows.Forms.CheckBox checkBoxIncludeBookmarks;
        private Nikse.SubtitleEdit.Controls.NikseTextBox textBoxSubtitleFileName;
        private System.Windows.Forms.GroupBox groupBox1;
        private Nikse.SubtitleEdit.Controls.NikseTextBox textBoxJobName;
        private System.Windows.Forms.Label label4;
        private Nikse.SubtitleEdit.Controls.NikseTextBox textBoxVideoUrl;
        private System.Windows.Forms.Label labelVideoUrl;
        private System.Windows.Forms.Label labelSubtitleTotalCount;
        private System.Windows.Forms.Button buttonGapChoose;
        private System.Windows.Forms.Label labelOptimalCharsPerSecond;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownOptimalCharsSec;
        private System.Windows.Forms.Label labelSubMaxLen;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownMaxWordsMin;
        private System.Windows.Forms.Label labelMaxWordsPerMin;
        private System.Windows.Forms.Label labelMinDuration;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownMaxNumberOfLines;
        private System.Windows.Forms.Label labelMaxDuration;
        private System.Windows.Forms.Label labelMaxLines;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownDurationMin;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownDurationMax;
        private System.Windows.Forms.Label labelMaxCharsPerSecond;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownMinGapMs;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownMaxCharsSec;
        private System.Windows.Forms.Label labelMinGapMs;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownSubtitleLineMaximumLength;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Button buttonImport;
        private System.Windows.Forms.CheckBox checkBoxOriginal;
    }
}