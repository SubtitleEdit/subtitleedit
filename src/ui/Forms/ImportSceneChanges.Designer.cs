namespace Nikse.SubtitleEdit.Forms
{
    partial class ImportSceneChanges
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
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.groupBoxImportText = new System.Windows.Forms.GroupBox();
            this.textBoxIImport = new System.Windows.Forms.TextBox();
            this.groupBoxTimeCodes = new System.Windows.Forms.GroupBox();
            this.radioButtonMilliseconds = new System.Windows.Forms.RadioButton();
            this.radioButtonSeconds = new System.Windows.Forms.RadioButton();
            this.radioButtonFrames = new System.Windows.Forms.RadioButton();
            this.radioButtonHHMMSSMS = new System.Windows.Forms.RadioButton();
            this.buttonOpenText = new System.Windows.Forms.Button();
            this.textBoxLog = new System.Windows.Forms.TextBox();
            this.groupBoxGenerateSceneChanges = new System.Windows.Forms.GroupBox();
            this.labelThresholdDescription = new System.Windows.Forms.Label();
            this.numericUpDownThreshold = new System.Windows.Forms.NumericUpDown();
            this.labelFfmpegThreshold = new System.Windows.Forms.Label();
            this.buttonDownloadFfmpeg = new System.Windows.Forms.Button();
            this.buttonImportWithFfmpeg = new System.Windows.Forms.Button();
            this.textBoxGenerate = new System.Windows.Forms.TextBox();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.groupBoxImportText.SuspendLayout();
            this.groupBoxTimeCodes.SuspendLayout();
            this.groupBoxGenerateSceneChanges.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownThreshold)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(639, 433);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(87, 23);
            this.buttonCancel.TabIndex = 4;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(546, 433);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(87, 23);
            this.buttonOK.TabIndex = 3;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // groupBoxImportText
            // 
            this.groupBoxImportText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxImportText.Controls.Add(this.textBoxIImport);
            this.groupBoxImportText.Controls.Add(this.groupBoxTimeCodes);
            this.groupBoxImportText.Controls.Add(this.buttonOpenText);
            this.groupBoxImportText.Controls.Add(this.textBoxLog);
            this.groupBoxImportText.Location = new System.Drawing.Point(367, 12);
            this.groupBoxImportText.Name = "groupBoxImportText";
            this.groupBoxImportText.Size = new System.Drawing.Size(359, 412);
            this.groupBoxImportText.TabIndex = 1;
            this.groupBoxImportText.TabStop = false;
            this.groupBoxImportText.Text = "Import scene changes";
            // 
            // textBoxIImport
            // 
            this.textBoxIImport.AllowDrop = true;
            this.textBoxIImport.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxIImport.Location = new System.Drawing.Point(6, 48);
            this.textBoxIImport.MaxLength = 0;
            this.textBoxIImport.Multiline = true;
            this.textBoxIImport.Name = "textBoxIImport";
            this.textBoxIImport.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxIImport.Size = new System.Drawing.Size(338, 232);
            this.textBoxIImport.TabIndex = 1;
            // 
            // groupBoxTimeCodes
            // 
            this.groupBoxTimeCodes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxTimeCodes.Controls.Add(this.radioButtonMilliseconds);
            this.groupBoxTimeCodes.Controls.Add(this.radioButtonSeconds);
            this.groupBoxTimeCodes.Controls.Add(this.radioButtonFrames);
            this.groupBoxTimeCodes.Controls.Add(this.radioButtonHHMMSSMS);
            this.groupBoxTimeCodes.Location = new System.Drawing.Point(6, 286);
            this.groupBoxTimeCodes.Name = "groupBoxTimeCodes";
            this.groupBoxTimeCodes.Size = new System.Drawing.Size(338, 120);
            this.groupBoxTimeCodes.TabIndex = 2;
            this.groupBoxTimeCodes.TabStop = false;
            this.groupBoxTimeCodes.Text = "Time codes";
            // 
            // radioButtonMilliseconds
            // 
            this.radioButtonMilliseconds.AutoSize = true;
            this.radioButtonMilliseconds.Location = new System.Drawing.Point(18, 67);
            this.radioButtonMilliseconds.Name = "radioButtonMilliseconds";
            this.radioButtonMilliseconds.Size = new System.Drawing.Size(82, 17);
            this.radioButtonMilliseconds.TabIndex = 2;
            this.radioButtonMilliseconds.Text = "Milliseconds";
            this.radioButtonMilliseconds.UseVisualStyleBackColor = true;
            // 
            // radioButtonSeconds
            // 
            this.radioButtonSeconds.AutoSize = true;
            this.radioButtonSeconds.Location = new System.Drawing.Point(18, 44);
            this.radioButtonSeconds.Name = "radioButtonSeconds";
            this.radioButtonSeconds.Size = new System.Drawing.Size(67, 17);
            this.radioButtonSeconds.TabIndex = 1;
            this.radioButtonSeconds.Text = "Seconds";
            this.radioButtonSeconds.UseVisualStyleBackColor = true;
            // 
            // radioButtonFrames
            // 
            this.radioButtonFrames.AutoSize = true;
            this.radioButtonFrames.Checked = true;
            this.radioButtonFrames.Location = new System.Drawing.Point(18, 21);
            this.radioButtonFrames.Name = "radioButtonFrames";
            this.radioButtonFrames.Size = new System.Drawing.Size(59, 17);
            this.radioButtonFrames.TabIndex = 0;
            this.radioButtonFrames.TabStop = true;
            this.radioButtonFrames.Text = "Frames";
            this.radioButtonFrames.UseVisualStyleBackColor = true;
            // 
            // radioButtonHHMMSSMS
            // 
            this.radioButtonHHMMSSMS.AutoSize = true;
            this.radioButtonHHMMSSMS.Location = new System.Drawing.Point(19, 90);
            this.radioButtonHHMMSSMS.Name = "radioButtonHHMMSSMS";
            this.radioButtonHHMMSSMS.Size = new System.Drawing.Size(167, 17);
            this.radioButtonHHMMSSMS.TabIndex = 3;
            this.radioButtonHHMMSSMS.Text = "HH:MM:SS.ms (00:00:01.500)";
            this.radioButtonHHMMSSMS.UseVisualStyleBackColor = true;
            // 
            // buttonOpenText
            // 
            this.buttonOpenText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOpenText.Location = new System.Drawing.Point(189, 21);
            this.buttonOpenText.Name = "buttonOpenText";
            this.buttonOpenText.Size = new System.Drawing.Size(153, 23);
            this.buttonOpenText.TabIndex = 0;
            this.buttonOpenText.Text = "Open file...";
            this.buttonOpenText.UseVisualStyleBackColor = true;
            this.buttonOpenText.Click += new System.EventHandler(this.buttonOpenText_Click);
            // 
            // textBoxLog
            // 
            this.textBoxLog.AllowDrop = true;
            this.textBoxLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxLog.Location = new System.Drawing.Point(7, 19);
            this.textBoxLog.MaxLength = 0;
            this.textBoxLog.Multiline = true;
            this.textBoxLog.Name = "textBoxLog";
            this.textBoxLog.ReadOnly = true;
            this.textBoxLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxLog.Size = new System.Drawing.Size(337, 387);
            this.textBoxLog.TabIndex = 6;
            // 
            // groupBoxGenerateSceneChanges
            // 
            this.groupBoxGenerateSceneChanges.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxGenerateSceneChanges.Controls.Add(this.labelThresholdDescription);
            this.groupBoxGenerateSceneChanges.Controls.Add(this.numericUpDownThreshold);
            this.groupBoxGenerateSceneChanges.Controls.Add(this.labelFfmpegThreshold);
            this.groupBoxGenerateSceneChanges.Controls.Add(this.buttonDownloadFfmpeg);
            this.groupBoxGenerateSceneChanges.Controls.Add(this.buttonImportWithFfmpeg);
            this.groupBoxGenerateSceneChanges.Controls.Add(this.textBoxGenerate);
            this.groupBoxGenerateSceneChanges.Location = new System.Drawing.Point(12, 12);
            this.groupBoxGenerateSceneChanges.Name = "groupBoxGenerateSceneChanges";
            this.groupBoxGenerateSceneChanges.Size = new System.Drawing.Size(349, 413);
            this.groupBoxGenerateSceneChanges.TabIndex = 0;
            this.groupBoxGenerateSceneChanges.TabStop = false;
            this.groupBoxGenerateSceneChanges.Text = "Generate scene changes";
            // 
            // labelThresholdDescription
            // 
            this.labelThresholdDescription.AutoSize = true;
            this.labelThresholdDescription.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.labelThresholdDescription.Location = new System.Drawing.Point(6, 387);
            this.labelThresholdDescription.Name = "labelThresholdDescription";
            this.labelThresholdDescription.Size = new System.Drawing.Size(195, 13);
            this.labelThresholdDescription.TabIndex = 5;
            this.labelThresholdDescription.Text = "Lower value gives more scene changes";
            // 
            // numericUpDownThreshold
            // 
            this.numericUpDownThreshold.DecimalPlaces = 1;
            this.numericUpDownThreshold.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericUpDownThreshold.Location = new System.Drawing.Point(66, 364);
            this.numericUpDownThreshold.Maximum = new decimal(new int[] {
            9,
            0,
            0,
            65536});
            this.numericUpDownThreshold.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericUpDownThreshold.Name = "numericUpDownThreshold";
            this.numericUpDownThreshold.Size = new System.Drawing.Size(43, 20);
            this.numericUpDownThreshold.TabIndex = 4;
            this.numericUpDownThreshold.Value = new decimal(new int[] {
            4,
            0,
            0,
            65536});
            // 
            // labelFfmpegThreshold
            // 
            this.labelFfmpegThreshold.AutoSize = true;
            this.labelFfmpegThreshold.Location = new System.Drawing.Point(6, 366);
            this.labelFfmpegThreshold.Name = "labelFfmpegThreshold";
            this.labelFfmpegThreshold.Size = new System.Drawing.Size(54, 13);
            this.labelFfmpegThreshold.TabIndex = 3;
            this.labelFfmpegThreshold.Text = "Sensitivity";
            // 
            // buttonDownloadFfmpeg
            // 
            this.buttonDownloadFfmpeg.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDownloadFfmpeg.Location = new System.Drawing.Point(159, 19);
            this.buttonDownloadFfmpeg.Name = "buttonDownloadFfmpeg";
            this.buttonDownloadFfmpeg.Size = new System.Drawing.Size(184, 23);
            this.buttonDownloadFfmpeg.TabIndex = 0;
            this.buttonDownloadFfmpeg.Text = "Download ffmpeg";
            this.buttonDownloadFfmpeg.UseVisualStyleBackColor = true;
            this.buttonDownloadFfmpeg.Click += new System.EventHandler(this.buttonDownloadFfmpeg_Click);
            // 
            // buttonImportWithFfmpeg
            // 
            this.buttonImportWithFfmpeg.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonImportWithFfmpeg.Location = new System.Drawing.Point(6, 330);
            this.buttonImportWithFfmpeg.Name = "buttonImportWithFfmpeg";
            this.buttonImportWithFfmpeg.Size = new System.Drawing.Size(337, 23);
            this.buttonImportWithFfmpeg.TabIndex = 2;
            this.buttonImportWithFfmpeg.Text = "Generate scene changes with FFmpeg";
            this.buttonImportWithFfmpeg.UseVisualStyleBackColor = true;
            this.buttonImportWithFfmpeg.Click += new System.EventHandler(this.buttonImportWithFfmpeg_Click);
            // 
            // textBoxGenerate
            // 
            this.textBoxGenerate.AllowDrop = true;
            this.textBoxGenerate.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxGenerate.Location = new System.Drawing.Point(6, 48);
            this.textBoxGenerate.MaxLength = 0;
            this.textBoxGenerate.Multiline = true;
            this.textBoxGenerate.Name = "textBoxGenerate";
            this.textBoxGenerate.ReadOnly = true;
            this.textBoxGenerate.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxGenerate.Size = new System.Drawing.Size(337, 276);
            this.textBoxGenerate.TabIndex = 1;
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(12, 433);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(528, 23);
            this.progressBar1.TabIndex = 2;
            this.progressBar1.Visible = false;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // ImportSceneChanges
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(738, 468);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.groupBoxImportText);
            this.Controls.Add(this.groupBoxGenerateSceneChanges);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ImportSceneChanges";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ImportSceneChanges";
            this.Shown += new System.EventHandler(this.ImportSceneChanges_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ImportSceneChanges_KeyDown);
            this.groupBoxImportText.ResumeLayout(false);
            this.groupBoxImportText.PerformLayout();
            this.groupBoxTimeCodes.ResumeLayout(false);
            this.groupBoxTimeCodes.PerformLayout();
            this.groupBoxGenerateSceneChanges.ResumeLayout(false);
            this.groupBoxGenerateSceneChanges.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownThreshold)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.GroupBox groupBoxImportText;
        private System.Windows.Forms.GroupBox groupBoxTimeCodes;
        private System.Windows.Forms.RadioButton radioButtonMilliseconds;
        private System.Windows.Forms.RadioButton radioButtonSeconds;
        private System.Windows.Forms.RadioButton radioButtonFrames;
		private System.Windows.Forms.RadioButton radioButtonHHMMSSMS;
        private System.Windows.Forms.GroupBox groupBoxGenerateSceneChanges;
        private System.Windows.Forms.TextBox textBoxGenerate;
        private System.Windows.Forms.Button buttonOpenText;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button buttonImportWithFfmpeg;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Button buttonDownloadFfmpeg;
        private System.Windows.Forms.TextBox textBoxIImport;
        private System.Windows.Forms.Label labelThresholdDescription;
        private System.Windows.Forms.NumericUpDown numericUpDownThreshold;
        private System.Windows.Forms.Label labelFfmpegThreshold;
        private System.Windows.Forms.TextBox textBoxLog;
    }
}
