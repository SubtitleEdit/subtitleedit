namespace Nikse.SubtitleEdit.Forms
{
    partial class VideoError
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
            this.buttonMpvSettings = new System.Windows.Forms.Button();
            this.labelMpvInfo = new System.Windows.Forms.Label();
            this.labelError = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(493, 144);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 0;
            this.buttonCancel.Text = "Close";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonMpvSettings
            // 
            this.buttonMpvSettings.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonMpvSettings.Location = new System.Drawing.Point(20, 95);
            this.buttonMpvSettings.Name = "buttonMpvSettings";
            this.buttonMpvSettings.Size = new System.Drawing.Size(272, 23);
            this.buttonMpvSettings.TabIndex = 30;
            this.buttonMpvSettings.Text = "Download and use \"mpv\" as video player";
            this.buttonMpvSettings.UseVisualStyleBackColor = true;
            this.buttonMpvSettings.Click += new System.EventHandler(this.buttonMpvSettings_Click);
            // 
            // labelMpvInfo
            // 
            this.labelMpvInfo.AutoSize = true;
            this.labelMpvInfo.Location = new System.Drawing.Point(17, 79);
            this.labelMpvInfo.Name = "labelMpvInfo";
            this.labelMpvInfo.Size = new System.Drawing.Size(69, 13);
            this.labelMpvInfo.TabIndex = 31;
            this.labelMpvInfo.Text = "labelMpvInfo";
            // 
            // labelError
            // 
            this.labelError.AutoSize = true;
            this.labelError.Location = new System.Drawing.Point(17, 24);
            this.labelError.Name = "labelError";
            this.labelError.Size = new System.Drawing.Size(53, 13);
            this.labelError.TabIndex = 32;
            this.labelError.Text = "labelError";
            // 
            // VideoError
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(580, 179);
            this.Controls.Add(this.labelError);
            this.Controls.Add(this.labelMpvInfo);
            this.Controls.Add(this.buttonMpvSettings);
            this.Controls.Add(this.buttonCancel);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "VideoError";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Error playing video file - ";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.VideoError_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonMpvSettings;
        private System.Windows.Forms.Label labelMpvInfo;
        private System.Windows.Forms.Label labelError;
    }
}