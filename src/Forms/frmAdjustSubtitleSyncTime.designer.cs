namespace Nikse.SubtitleEdit.Forms
{
    partial class frmAdjustSubtitleSyncTime
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
            Nikse.SubtitleEdit.Core.TimeCode timeCode1 = new Nikse.SubtitleEdit.Core.TimeCode();
            this.btnSync = new System.Windows.Forms.Button();
            this.TimeUpDown = new Nikse.SubtitleEdit.Controls.TimeUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnSync
            // 
            this.btnSync.Location = new System.Drawing.Point(288, 40);
            this.btnSync.Name = "btnSync";
            this.btnSync.Size = new System.Drawing.Size(75, 23);
            this.btnSync.TabIndex = 0;
            this.btnSync.Text = "Sync";
            this.btnSync.UseVisualStyleBackColor = true;
            this.btnSync.Click += new System.EventHandler(this.btnSync_Click);
            // 
            // TimeUpDown
            // 
            this.TimeUpDown.AutoSize = true;
            this.TimeUpDown.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.TimeUpDown.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.TimeUpDown.Location = new System.Drawing.Point(181, 39);
            this.TimeUpDown.Name = "TimeUpDown";
            this.TimeUpDown.Size = new System.Drawing.Size(96, 27);
            this.TimeUpDown.TabIndex = 1;
            timeCode1.Hours = 0;
            timeCode1.Milliseconds = 0;
            timeCode1.Minutes = 0;
            timeCode1.Seconds = 0;
            timeCode1.TimeSpan = System.TimeSpan.Parse("00:00:00");
            timeCode1.TotalMilliseconds = 0D;
            timeCode1.TotalSeconds = 0D;
            this.TimeUpDown.TimeCode = timeCode1;
            this.TimeUpDown.UseVideoOffset = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(56, 45);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(119, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Adjust subtitle sync time";
            // 
            // frmGetSyncingParagraphsTimeSpan
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(436, 110);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.TimeUpDown);
            this.Controls.Add(this.btnSync);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "frmGetSyncingParagraphsTimeSpan";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Resync multiple paragraphs";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnSync;
        private Controls.TimeUpDown TimeUpDown;
        private System.Windows.Forms.Label label1;
    }
}