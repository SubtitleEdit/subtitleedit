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
            this.groupBoxImportOptions = new System.Windows.Forms.GroupBox();
            this.groupBoxTimeCodes = new System.Windows.Forms.GroupBox();
            this.radioButtonMilliseconds = new System.Windows.Forms.RadioButton();
            this.radioButtonSeconds = new System.Windows.Forms.RadioButton();
            this.radioButtonFrames = new System.Windows.Forms.RadioButton();
            this.radioButtonHHMMSSMS = new System.Windows.Forms.RadioButton();
            this.groupBoxImportText = new System.Windows.Forms.GroupBox();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.buttonImportWithFfmpeg = new System.Windows.Forms.Button();
            this.textBoxText = new System.Windows.Forms.TextBox();
            this.buttonOpenText = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.groupBoxImportOptions.SuspendLayout();
            this.groupBoxTimeCodes.SuspendLayout();
            this.groupBoxImportText.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(615, 374);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 21);
            this.buttonCancel.TabIndex = 8;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(534, 374);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 21);
            this.buttonOK.TabIndex = 7;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // groupBoxImportOptions
            // 
            this.groupBoxImportOptions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxImportOptions.Controls.Add(this.groupBoxTimeCodes);
            this.groupBoxImportOptions.Location = new System.Drawing.Point(349, 12);
            this.groupBoxImportOptions.Name = "groupBoxImportOptions";
            this.groupBoxImportOptions.Size = new System.Drawing.Size(341, 351);
            this.groupBoxImportOptions.TabIndex = 6;
            this.groupBoxImportOptions.TabStop = false;
            this.groupBoxImportOptions.Text = "Import options";
            // 
            // groupBoxTimeCodes
            // 
            this.groupBoxTimeCodes.Controls.Add(this.radioButtonMilliseconds);
            this.groupBoxTimeCodes.Controls.Add(this.radioButtonSeconds);
            this.groupBoxTimeCodes.Controls.Add(this.radioButtonFrames);
            this.groupBoxTimeCodes.Controls.Add(this.radioButtonHHMMSSMS);
            this.groupBoxTimeCodes.Location = new System.Drawing.Point(6, 19);
            this.groupBoxTimeCodes.Name = "groupBoxTimeCodes";
            this.groupBoxTimeCodes.Size = new System.Drawing.Size(329, 120);
            this.groupBoxTimeCodes.TabIndex = 5;
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
            // groupBoxImportText
            // 
            this.groupBoxImportText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxImportText.Controls.Add(this.progressBar1);
            this.groupBoxImportText.Controls.Add(this.buttonImportWithFfmpeg);
            this.groupBoxImportText.Controls.Add(this.textBoxText);
            this.groupBoxImportText.Controls.Add(this.buttonOpenText);
            this.groupBoxImportText.Location = new System.Drawing.Point(12, 12);
            this.groupBoxImportText.Name = "groupBoxImportText";
            this.groupBoxImportText.Size = new System.Drawing.Size(331, 351);
            this.groupBoxImportText.TabIndex = 5;
            this.groupBoxImportText.TabStop = false;
            this.groupBoxImportText.Text = "Import scene changes";
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(6, 317);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(319, 23);
            this.progressBar1.TabIndex = 3;
            this.progressBar1.Visible = false;
            // 
            // buttonImportWithFfmpeg
            // 
            this.buttonImportWithFfmpeg.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonImportWithFfmpeg.Location = new System.Drawing.Point(6, 290);
            this.buttonImportWithFfmpeg.Name = "buttonImportWithFfmpeg";
            this.buttonImportWithFfmpeg.Size = new System.Drawing.Size(319, 21);
            this.buttonImportWithFfmpeg.TabIndex = 2;
            this.buttonImportWithFfmpeg.Text = "Get scene changes with FFmpeg";
            this.buttonImportWithFfmpeg.UseVisualStyleBackColor = true;
            this.buttonImportWithFfmpeg.Click += new System.EventHandler(this.buttonImportWithFfmpeg_Click);
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
            this.textBoxText.Size = new System.Drawing.Size(319, 188);
            this.textBoxText.TabIndex = 1;
            // 
            // buttonOpenText
            // 
            this.buttonOpenText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOpenText.Location = new System.Drawing.Point(182, 19);
            this.buttonOpenText.Name = "buttonOpenText";
            this.buttonOpenText.Size = new System.Drawing.Size(143, 21);
            this.buttonOpenText.TabIndex = 0;
            this.buttonOpenText.Text = "Open file...";
            this.buttonOpenText.UseVisualStyleBackColor = true;
            this.buttonOpenText.Click += new System.EventHandler(this.buttonOpenText_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // ImportSceneChanges
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(702, 407);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.groupBoxImportOptions);
            this.Controls.Add(this.groupBoxImportText);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ImportSceneChanges";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "ImportSceneChanges";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ImportSceneChanges_KeyDown);
            this.groupBoxImportOptions.ResumeLayout(false);
            this.groupBoxTimeCodes.ResumeLayout(false);
            this.groupBoxTimeCodes.PerformLayout();
            this.groupBoxImportText.ResumeLayout(false);
            this.groupBoxImportText.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.GroupBox groupBoxImportOptions;
        private System.Windows.Forms.GroupBox groupBoxTimeCodes;
        private System.Windows.Forms.RadioButton radioButtonMilliseconds;
        private System.Windows.Forms.RadioButton radioButtonSeconds;
        private System.Windows.Forms.RadioButton radioButtonFrames;
		private System.Windows.Forms.RadioButton radioButtonHHMMSSMS;
        private System.Windows.Forms.GroupBox groupBoxImportText;
        private System.Windows.Forms.TextBox textBoxText;
        private System.Windows.Forms.Button buttonOpenText;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button buttonImportWithFfmpeg;
        private System.Windows.Forms.ProgressBar progressBar1;
    }
}
