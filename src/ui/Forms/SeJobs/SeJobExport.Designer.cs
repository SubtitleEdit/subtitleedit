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
            this.checkBoxIncludeSceneChanges = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxJobId = new System.Windows.Forms.TextBox();
            this.labelSubtitleFileName = new System.Windows.Forms.Label();
            this.textBoxJobDescription = new System.Windows.Forms.TextBox();
            this.labelDescription = new System.Windows.Forms.Label();
            this.checkBoxIncludeRules = new System.Windows.Forms.CheckBox();
            this.checkBoxIncludeWaveform = new System.Windows.Forms.CheckBox();
            this.checkBoxIncludeBookmarks = new System.Windows.Forms.CheckBox();
            this.textBoxSubtitleFileName = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.buttonGapChoose = new System.Windows.Forms.Button();
            this.labelOptimalCharsPerSecond = new System.Windows.Forms.Label();
            this.numericUpDownOptimalCharsSec = new System.Windows.Forms.NumericUpDown();
            this.labelSubMaxLen = new System.Windows.Forms.Label();
            this.numericUpDownMaxWordsMin = new System.Windows.Forms.NumericUpDown();
            this.labelMaxWordsPerMin = new System.Windows.Forms.Label();
            this.labelMinDuration = new System.Windows.Forms.Label();
            this.numericUpDownMaxNumberOfLines = new System.Windows.Forms.NumericUpDown();
            this.labelMaxDuration = new System.Windows.Forms.Label();
            this.labelMaxLines = new System.Windows.Forms.Label();
            this.numericUpDownDurationMin = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownDurationMax = new System.Windows.Forms.NumericUpDown();
            this.labelMaxCharsPerSecond = new System.Windows.Forms.Label();
            this.numericUpDownMinGapMs = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownMaxCharsSec = new System.Windows.Forms.NumericUpDown();
            this.labelMinGapMs = new System.Windows.Forms.Label();
            this.numericUpDownSubtitleLineMaximumLength = new System.Windows.Forms.NumericUpDown();
            this.textBoxJobName = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxVideoUrl = new System.Windows.Forms.TextBox();
            this.labelVideoUrl = new System.Windows.Forms.Label();
            this.labelSubtitleTotalCount = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonSave = new System.Windows.Forms.Button();
            this.buttonImport = new System.Windows.Forms.Button();
            this.checkBoxOriginal = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
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
            // checkBoxIncludeSceneChanges
            // 
            this.checkBoxIncludeSceneChanges.AutoSize = true;
            this.checkBoxIncludeSceneChanges.Location = new System.Drawing.Point(166, 315);
            this.checkBoxIncludeSceneChanges.Name = "checkBoxIncludeSceneChanges";
            this.checkBoxIncludeSceneChanges.Size = new System.Drawing.Size(137, 17);
            this.checkBoxIncludeSceneChanges.TabIndex = 13;
            this.checkBoxIncludeSceneChanges.Text = "Include scene changes";
            this.checkBoxIncludeSceneChanges.UseVisualStyleBackColor = true;
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
            this.buttonGapChoose.Location = new System.Drawing.Point(281, 183);
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
            this.labelOptimalCharsPerSecond.Location = new System.Drawing.Point(23, 54);
            this.labelOptimalCharsPerSecond.Name = "labelOptimalCharsPerSecond";
            this.labelOptimalCharsPerSecond.Size = new System.Drawing.Size(93, 13);
            this.labelOptimalCharsPerSecond.TabIndex = 2;
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
            this.numericUpDownOptimalCharsSec.Location = new System.Drawing.Point(220, 52);
            this.numericUpDownOptimalCharsSec.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.numericUpDownOptimalCharsSec.Name = "numericUpDownOptimalCharsSec";
            this.numericUpDownOptimalCharsSec.Size = new System.Drawing.Size(56, 20);
            this.numericUpDownOptimalCharsSec.TabIndex = 3;
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
            this.numericUpDownMaxWordsMin.Location = new System.Drawing.Point(220, 106);
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
            this.numericUpDownMaxWordsMin.TabIndex = 8;
            this.numericUpDownMaxWordsMin.Value = new decimal(new int[] {
            300,
            0,
            0,
            0});
            // 
            // labelMaxWordsPerMin
            // 
            this.labelMaxWordsPerMin.AutoSize = true;
            this.labelMaxWordsPerMin.Location = new System.Drawing.Point(23, 108);
            this.labelMaxWordsPerMin.Name = "labelMaxWordsPerMin";
            this.labelMaxWordsPerMin.Size = new System.Drawing.Size(82, 13);
            this.labelMaxWordsPerMin.TabIndex = 6;
            this.labelMaxWordsPerMin.Text = "Max. words/min";
            // 
            // labelMinDuration
            // 
            this.labelMinDuration.AutoSize = true;
            this.labelMinDuration.Location = new System.Drawing.Point(23, 135);
            this.labelMinDuration.Name = "labelMinDuration";
            this.labelMinDuration.Size = new System.Drawing.Size(130, 13);
            this.labelMinDuration.TabIndex = 10;
            this.labelMinDuration.Text = "Min. duration, milliseconds";
            // 
            // numericUpDownMaxNumberOfLines
            // 
            this.numericUpDownMaxNumberOfLines.Location = new System.Drawing.Point(220, 212);
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
            this.numericUpDownMaxNumberOfLines.TabIndex = 1;
            this.numericUpDownMaxNumberOfLines.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // labelMaxDuration
            // 
            this.labelMaxDuration.AutoSize = true;
            this.labelMaxDuration.Location = new System.Drawing.Point(23, 161);
            this.labelMaxDuration.Name = "labelMaxDuration";
            this.labelMaxDuration.Size = new System.Drawing.Size(133, 13);
            this.labelMaxDuration.TabIndex = 12;
            this.labelMaxDuration.Text = "Max. duration, milliseconds";
            // 
            // labelMaxLines
            // 
            this.labelMaxLines.AutoSize = true;
            this.labelMaxLines.Location = new System.Drawing.Point(23, 214);
            this.labelMaxLines.Name = "labelMaxLines";
            this.labelMaxLines.Size = new System.Drawing.Size(104, 13);
            this.labelMaxLines.TabIndex = 0;
            this.labelMaxLines.Text = "Max. number of lines";
            // 
            // numericUpDownDurationMin
            // 
            this.numericUpDownDurationMin.Location = new System.Drawing.Point(220, 133);
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
            this.numericUpDownDurationMin.Size = new System.Drawing.Size(56, 20);
            this.numericUpDownDurationMin.TabIndex = 11;
            this.numericUpDownDurationMin.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // numericUpDownDurationMax
            // 
            this.numericUpDownDurationMax.Location = new System.Drawing.Point(220, 159);
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
            this.numericUpDownDurationMax.TabIndex = 13;
            this.numericUpDownDurationMax.Value = new decimal(new int[] {
            50000,
            0,
            0,
            0});
            // 
            // labelMaxCharsPerSecond
            // 
            this.labelMaxCharsPerSecond.AutoSize = true;
            this.labelMaxCharsPerSecond.Location = new System.Drawing.Point(23, 81);
            this.labelMaxCharsPerSecond.Name = "labelMaxCharsPerSecond";
            this.labelMaxCharsPerSecond.Size = new System.Drawing.Size(81, 13);
            this.labelMaxCharsPerSecond.TabIndex = 4;
            this.labelMaxCharsPerSecond.Text = "Max. chars/sec";
            // 
            // numericUpDownMinGapMs
            // 
            this.numericUpDownMinGapMs.Location = new System.Drawing.Point(220, 185);
            this.numericUpDownMinGapMs.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownMinGapMs.Name = "numericUpDownMinGapMs";
            this.numericUpDownMinGapMs.Size = new System.Drawing.Size(56, 20);
            this.numericUpDownMinGapMs.TabIndex = 15;
            this.numericUpDownMinGapMs.Value = new decimal(new int[] {
            25,
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
            this.numericUpDownMaxCharsSec.Location = new System.Drawing.Point(220, 79);
            this.numericUpDownMaxCharsSec.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.numericUpDownMaxCharsSec.Name = "numericUpDownMaxCharsSec";
            this.numericUpDownMaxCharsSec.Size = new System.Drawing.Size(56, 20);
            this.numericUpDownMaxCharsSec.TabIndex = 5;
            this.numericUpDownMaxCharsSec.Value = new decimal(new int[] {
            24,
            0,
            0,
            0});
            // 
            // labelMinGapMs
            // 
            this.labelMinGapMs.AutoSize = true;
            this.labelMinGapMs.Location = new System.Drawing.Point(23, 187);
            this.labelMinGapMs.Name = "labelMinGapMs";
            this.labelMinGapMs.Size = new System.Drawing.Size(133, 13);
            this.labelMinGapMs.TabIndex = 14;
            this.labelMinGapMs.Text = "Min. gap between subtitles";
            // 
            // numericUpDownSubtitleLineMaximumLength
            // 
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
            this.numericUpDownSubtitleLineMaximumLength.Size = new System.Drawing.Size(56, 20);
            this.numericUpDownSubtitleLineMaximumLength.TabIndex = 1;
            this.numericUpDownSubtitleLineMaximumLength.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // textBoxJobName
            // 
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
            this.Controls.Add(this.checkBoxIncludeSceneChanges);
            this.Name = "SeJobExport";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "SE Job file export";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SeJobExport_KeyDown);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownOptimalCharsSec)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxWordsMin)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxNumberOfLines)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDurationMin)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDurationMax)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMinGapMs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxCharsSec)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSubtitleLineMaximumLength)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBoxIncludeSceneChanges;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxJobId;
        private System.Windows.Forms.Label labelSubtitleFileName;
        private System.Windows.Forms.TextBox textBoxJobDescription;
        private System.Windows.Forms.Label labelDescription;
        private System.Windows.Forms.CheckBox checkBoxIncludeRules;
        private System.Windows.Forms.CheckBox checkBoxIncludeWaveform;
        private System.Windows.Forms.CheckBox checkBoxIncludeBookmarks;
        private System.Windows.Forms.TextBox textBoxSubtitleFileName;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox textBoxJobName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBoxVideoUrl;
        private System.Windows.Forms.Label labelVideoUrl;
        private System.Windows.Forms.Label labelSubtitleTotalCount;
        private System.Windows.Forms.Button buttonGapChoose;
        private System.Windows.Forms.Label labelOptimalCharsPerSecond;
        private System.Windows.Forms.NumericUpDown numericUpDownOptimalCharsSec;
        private System.Windows.Forms.Label labelSubMaxLen;
        private System.Windows.Forms.NumericUpDown numericUpDownMaxWordsMin;
        private System.Windows.Forms.Label labelMaxWordsPerMin;
        private System.Windows.Forms.Label labelMinDuration;
        private System.Windows.Forms.NumericUpDown numericUpDownMaxNumberOfLines;
        private System.Windows.Forms.Label labelMaxDuration;
        private System.Windows.Forms.Label labelMaxLines;
        private System.Windows.Forms.NumericUpDown numericUpDownDurationMin;
        private System.Windows.Forms.NumericUpDown numericUpDownDurationMax;
        private System.Windows.Forms.Label labelMaxCharsPerSecond;
        private System.Windows.Forms.NumericUpDown numericUpDownMinGapMs;
        private System.Windows.Forms.NumericUpDown numericUpDownMaxCharsSec;
        private System.Windows.Forms.Label labelMinGapMs;
        private System.Windows.Forms.NumericUpDown numericUpDownSubtitleLineMaximumLength;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Button buttonImport;
        private System.Windows.Forms.CheckBox checkBoxOriginal;
    }
}