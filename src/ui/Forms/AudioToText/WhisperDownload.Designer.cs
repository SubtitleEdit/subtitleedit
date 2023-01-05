namespace Nikse.SubtitleEdit.Forms.AudioToText
{
    sealed partial class WhisperDownload
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
            this.labelDescription1 = new System.Windows.Forms.Label();
            this.labelPleaseWait = new System.Windows.Forms.Label();
            this.labelAVX2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labelDescription1
            // 
            this.labelDescription1.AutoSize = true;
            this.labelDescription1.Location = new System.Drawing.Point(21, 27);
            this.labelDescription1.Name = "labelDescription1";
            this.labelDescription1.Size = new System.Drawing.Size(132, 13);
            this.labelDescription1.TabIndex = 29;
            this.labelDescription1.Text = "Downloading Whisper.cpp";
            // 
            // labelPleaseWait
            // 
            this.labelPleaseWait.AutoSize = true;
            this.labelPleaseWait.Location = new System.Drawing.Point(21, 59);
            this.labelPleaseWait.Name = "labelPleaseWait";
            this.labelPleaseWait.Size = new System.Drawing.Size(70, 13);
            this.labelPleaseWait.TabIndex = 28;
            this.labelPleaseWait.Text = "Please wait...";
            // 
            // labelAVX2
            // 
            this.labelAVX2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelAVX2.AutoSize = true;
            this.labelAVX2.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.labelAVX2.Location = new System.Drawing.Point(274, 9);
            this.labelAVX2.Name = "labelAVX2";
            this.labelAVX2.Size = new System.Drawing.Size(34, 13);
            this.labelAVX2.TabIndex = 30;
            this.labelAVX2.Text = "AVX2";
            // 
            // WhisperDownload
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(320, 93);
            this.Controls.Add(this.labelAVX2);
            this.Controls.Add(this.labelDescription1);
            this.Controls.Add(this.labelPleaseWait);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "WhisperDownload";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Download Tesseract";
            this.Shown += new System.EventHandler(this.WhisperDownload_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelDescription1;
        private System.Windows.Forms.Label labelPleaseWait;
        private System.Windows.Forms.Label labelAVX2;
    }
}