namespace Nikse.SubtitleEdit.Forms
{
    partial class StatusLog
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
            this.buttonOK = new System.Windows.Forms.Button();
            this.textBoxStatusLog = new System.Windows.Forms.TextBox();
            this.buttonExportLog = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(877, 407);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 21);
            this.buttonOK.TabIndex = 1;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            // 
            // textBoxStatusLog
            // 
            this.textBoxStatusLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxStatusLog.Location = new System.Drawing.Point(13, 13);
            this.textBoxStatusLog.Multiline = true;
            this.textBoxStatusLog.Name = "textBoxStatusLog";
            this.textBoxStatusLog.ReadOnly = true;
            this.textBoxStatusLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxStatusLog.Size = new System.Drawing.Size(939, 388);
            this.textBoxStatusLog.TabIndex = 2;
            // 
            // buttonExportLog
            // 
            this.buttonExportLog.Location = new System.Drawing.Point(770, 407);
            this.buttonExportLog.Name = "buttonExportLog";
            this.buttonExportLog.Size = new System.Drawing.Size(101, 21);
            this.buttonExportLog.TabIndex = 3;
            this.buttonExportLog.Text = "Export";
            this.buttonExportLog.UseVisualStyleBackColor = true;
            this.buttonExportLog.Click += new System.EventHandler(this.buttonExportLog_Click);
            // 
            // StatusLog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(964, 440);
            this.Controls.Add(this.buttonExportLog);
            this.Controls.Add(this.textBoxStatusLog);
            this.Controls.Add(this.buttonOK);
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(400, 200);
            this.Name = "StatusLog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Status messages";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.StatusLog_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.TextBox textBoxStatusLog;
        private System.Windows.Forms.Button buttonExportLog;
    }
}