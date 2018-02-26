namespace Nikse.SubtitleEdit.Forms
{
    partial class AddWaveform
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
            this.labelPleaseWait = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.buttonRipWave = new System.Windows.Forms.Button();
            this.labelProgress = new System.Windows.Forms.Label();
            this.labelSourcevideoFile = new System.Windows.Forms.Label();
            this.labelVideoFileName = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.labelInfo = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labelPleaseWait
            // 
            this.labelPleaseWait.AutoSize = true;
            this.labelPleaseWait.Location = new System.Drawing.Point(176, 59);
            this.labelPleaseWait.Name = "labelPleaseWait";
            this.labelPleaseWait.Size = new System.Drawing.Size(203, 13);
            this.labelPleaseWait.TabIndex = 14;
            this.labelPleaseWait.Text = "This may take a few minutes - please wait";
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(12, 80);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(522, 23);
            this.progressBar1.TabIndex = 12;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // buttonRipWave
            // 
            this.buttonRipWave.Location = new System.Drawing.Point(12, 54);
            this.buttonRipWave.Name = "buttonRipWave";
            this.buttonRipWave.Size = new System.Drawing.Size(158, 23);
            this.buttonRipWave.TabIndex = 13;
            this.buttonRipWave.Text = "Generate waveform data";
            this.buttonRipWave.UseVisualStyleBackColor = true;
            this.buttonRipWave.Click += new System.EventHandler(this.buttonRipWave_Click);
            // 
            // labelProgress
            // 
            this.labelProgress.AutoSize = true;
            this.labelProgress.Location = new System.Drawing.Point(12, 106);
            this.labelProgress.Name = "labelProgress";
            this.labelProgress.Size = new System.Drawing.Size(70, 13);
            this.labelProgress.TabIndex = 9;
            this.labelProgress.Text = "labelProgress";
            // 
            // labelSourcevideoFile
            // 
            this.labelSourcevideoFile.AutoSize = true;
            this.labelSourcevideoFile.Location = new System.Drawing.Point(14, 9);
            this.labelSourcevideoFile.Name = "labelSourcevideoFile";
            this.labelSourcevideoFile.Size = new System.Drawing.Size(89, 13);
            this.labelSourcevideoFile.TabIndex = 15;
            this.labelSourcevideoFile.Text = "Source video file:";
            // 
            // labelVideoFileName
            // 
            this.labelVideoFileName.AutoSize = true;
            this.labelVideoFileName.Location = new System.Drawing.Point(12, 26);
            this.labelVideoFileName.Name = "labelVideoFileName";
            this.labelVideoFileName.Size = new System.Drawing.Size(100, 13);
            this.labelVideoFileName.TabIndex = 16;
            this.labelVideoFileName.Text = "labelVideoFileName";
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(442, 109);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(92, 23);
            this.buttonCancel.TabIndex = 17;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // labelInfo
            // 
            this.labelInfo.ForeColor = System.Drawing.Color.Gray;
            this.labelInfo.Location = new System.Drawing.Point(427, 6);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(120, 17);
            this.labelInfo.TabIndex = 18;
            this.labelInfo.Text = "label1";
            this.labelInfo.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // AddWaveform
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(550, 138);
            this.Controls.Add(this.labelInfo);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.labelVideoFileName);
            this.Controls.Add(this.labelSourcevideoFile);
            this.Controls.Add(this.labelPleaseWait);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.buttonRipWave);
            this.Controls.Add(this.labelProgress);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AddWaveform";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Generate waveform data";
            this.Shown += new System.EventHandler(this.AddWaveform_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.AddWaveform_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelPleaseWait;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button buttonRipWave;
        private System.Windows.Forms.Label labelProgress;
        private System.Windows.Forms.Label labelSourcevideoFile;
        private System.Windows.Forms.Label labelVideoFileName;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label labelInfo;
    }
}