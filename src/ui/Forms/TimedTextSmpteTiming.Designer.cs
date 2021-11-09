
namespace Nikse.SubtitleEdit.Forms
{
    partial class TimedTextSmpteTiming
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
            this.labelUseSmpteTiming = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonNever = new System.Windows.Forms.Button();
            this.buttonAlways = new System.Windows.Forms.Button();
            this.labelInfo = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labelUseSmpteTiming
            // 
            this.labelUseSmpteTiming.AutoSize = true;
            this.labelUseSmpteTiming.Location = new System.Drawing.Point(21, 22);
            this.labelUseSmpteTiming.Name = "labelUseSmpteTiming";
            this.labelUseSmpteTiming.Size = new System.Drawing.Size(189, 13);
            this.labelUseSmpteTiming.TabIndex = 0;
            this.labelUseSmpteTiming.Text = "Use SMPTE timing for current subtitle?";
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(522, 131);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(441, 131);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 0;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonNever
            // 
            this.buttonNever.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonNever.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonNever.Location = new System.Drawing.Point(270, 131);
            this.buttonNever.Name = "buttonNever";
            this.buttonNever.Size = new System.Drawing.Size(165, 23);
            this.buttonNever.TabIndex = 3;
            this.buttonNever.Text = "No, never";
            this.buttonNever.UseVisualStyleBackColor = true;
            this.buttonNever.Click += new System.EventHandler(this.ButtonNeverClick);
            // 
            // buttonAlways
            // 
            this.buttonAlways.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAlways.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonAlways.Location = new System.Drawing.Point(270, 102);
            this.buttonAlways.Name = "buttonAlways";
            this.buttonAlways.Size = new System.Drawing.Size(327, 23);
            this.buttonAlways.TabIndex = 2;
            this.buttonAlways.Text = "Yes, always";
            this.buttonAlways.UseVisualStyleBackColor = true;
            this.buttonAlways.Click += new System.EventHandler(this.ButtonAlwaysClick);
            // 
            // labelInfo
            // 
            this.labelInfo.AutoSize = true;
            this.labelInfo.Location = new System.Drawing.Point(21, 53);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(301, 13);
            this.labelInfo.TabIndex = 4;
            this.labelInfo.Text = "Note: SMPTE timing can be changed later in the ´Video menu´";
            // 
            // TimedTextSmpteTiming
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(609, 166);
            this.Controls.Add(this.labelInfo);
            this.Controls.Add(this.buttonAlways);
            this.Controls.Add(this.buttonNever);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.labelUseSmpteTiming);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TimedTextSmpteTiming";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "SMPTE timing";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelUseSmpteTiming;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonNever;
        private System.Windows.Forms.Button buttonAlways;
        private System.Windows.Forms.Label labelInfo;
    }
}