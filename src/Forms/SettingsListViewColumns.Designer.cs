namespace Nikse.SubtitleEdit.Forms
{
    partial class SettingsListViewColumns
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
            this.checkBoxShowCps = new System.Windows.Forms.CheckBox();
            this.labelInfo = new System.Windows.Forms.Label();
            this.checkBoxShowWpm = new System.Windows.Forms.CheckBox();
            this.checkBoxShowText = new System.Windows.Forms.CheckBox();
            this.checkBoxShowNumber = new System.Windows.Forms.CheckBox();
            this.checkBoxShowStartTime = new System.Windows.Forms.CheckBox();
            this.checkBoxShowEndTime = new System.Windows.Forms.CheckBox();
            this.checkBoxShowDuration = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(241, 207);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 21);
            this.buttonCancel.TabIndex = 6;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(160, 207);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 21);
            this.buttonOK.TabIndex = 5;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // checkBoxShowCps
            // 
            this.checkBoxShowCps.AutoSize = true;
            this.checkBoxShowCps.Location = new System.Drawing.Point(44, 142);
            this.checkBoxShowCps.Name = "checkBoxShowCps";
            this.checkBoxShowCps.Size = new System.Drawing.Size(99, 17);
            this.checkBoxShowCps.TabIndex = 34;
            this.checkBoxShowCps.Text = "Characters/sec";
            this.checkBoxShowCps.UseVisualStyleBackColor = true;
            // 
            // labelInfo
            // 
            this.labelInfo.AutoSize = true;
            this.labelInfo.Location = new System.Drawing.Point(23, 21);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(208, 13);
            this.labelInfo.TabIndex = 35;
            this.labelInfo.Text = "Choose visible columns for subtitle list view";
            // 
            // checkBoxShowWpm
            // 
            this.checkBoxShowWpm.AutoSize = true;
            this.checkBoxShowWpm.Location = new System.Drawing.Point(44, 165);
            this.checkBoxShowWpm.Name = "checkBoxShowWpm";
            this.checkBoxShowWpm.Size = new System.Drawing.Size(78, 17);
            this.checkBoxShowWpm.TabIndex = 36;
            this.checkBoxShowWpm.Text = "Words/min";
            this.checkBoxShowWpm.UseVisualStyleBackColor = true;
            // 
            // checkBoxShowText
            // 
            this.checkBoxShowText.AutoSize = true;
            this.checkBoxShowText.Checked = true;
            this.checkBoxShowText.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxShowText.Enabled = false;
            this.checkBoxShowText.Location = new System.Drawing.Point(44, 188);
            this.checkBoxShowText.Name = "checkBoxShowText";
            this.checkBoxShowText.Size = new System.Drawing.Size(47, 17);
            this.checkBoxShowText.TabIndex = 37;
            this.checkBoxShowText.Text = "Text";
            this.checkBoxShowText.UseVisualStyleBackColor = true;
            // 
            // checkBoxShowNumber
            // 
            this.checkBoxShowNumber.AutoSize = true;
            this.checkBoxShowNumber.Checked = true;
            this.checkBoxShowNumber.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxShowNumber.Enabled = false;
            this.checkBoxShowNumber.Location = new System.Drawing.Point(44, 50);
            this.checkBoxShowNumber.Name = "checkBoxShowNumber";
            this.checkBoxShowNumber.Size = new System.Drawing.Size(63, 17);
            this.checkBoxShowNumber.TabIndex = 38;
            this.checkBoxShowNumber.Text = "Number";
            this.checkBoxShowNumber.UseVisualStyleBackColor = true;
            // 
            // checkBoxShowStartTime
            // 
            this.checkBoxShowStartTime.AutoSize = true;
            this.checkBoxShowStartTime.Checked = true;
            this.checkBoxShowStartTime.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxShowStartTime.Enabled = false;
            this.checkBoxShowStartTime.Location = new System.Drawing.Point(44, 73);
            this.checkBoxShowStartTime.Name = "checkBoxShowStartTime";
            this.checkBoxShowStartTime.Size = new System.Drawing.Size(70, 17);
            this.checkBoxShowStartTime.TabIndex = 39;
            this.checkBoxShowStartTime.Text = "Start time";
            this.checkBoxShowStartTime.UseVisualStyleBackColor = true;
            // 
            // checkBoxShowEndTime
            // 
            this.checkBoxShowEndTime.AutoSize = true;
            this.checkBoxShowEndTime.Checked = true;
            this.checkBoxShowEndTime.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxShowEndTime.Location = new System.Drawing.Point(44, 96);
            this.checkBoxShowEndTime.Name = "checkBoxShowEndTime";
            this.checkBoxShowEndTime.Size = new System.Drawing.Size(71, 17);
            this.checkBoxShowEndTime.TabIndex = 40;
            this.checkBoxShowEndTime.Text = "End Time";
            this.checkBoxShowEndTime.UseVisualStyleBackColor = true;
            // 
            // checkBoxShowDuration
            // 
            this.checkBoxShowDuration.AutoSize = true;
            this.checkBoxShowDuration.Checked = true;
            this.checkBoxShowDuration.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxShowDuration.Location = new System.Drawing.Point(44, 119);
            this.checkBoxShowDuration.Name = "checkBoxShowDuration";
            this.checkBoxShowDuration.Size = new System.Drawing.Size(66, 17);
            this.checkBoxShowDuration.TabIndex = 41;
            this.checkBoxShowDuration.Text = "Duration";
            this.checkBoxShowDuration.UseVisualStyleBackColor = true;
            // 
            // SettingsListViewColumns
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(328, 240);
            this.Controls.Add(this.checkBoxShowDuration);
            this.Controls.Add(this.checkBoxShowEndTime);
            this.Controls.Add(this.checkBoxShowStartTime);
            this.Controls.Add(this.checkBoxShowNumber);
            this.Controls.Add(this.checkBoxShowText);
            this.Controls.Add(this.checkBoxShowWpm);
            this.Controls.Add(this.labelInfo);
            this.Controls.Add(this.checkBoxShowCps);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsListViewColumns";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Listview columns";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SettingsListViewColumns_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.CheckBox checkBoxShowCps;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.CheckBox checkBoxShowWpm;
        private System.Windows.Forms.CheckBox checkBoxShowText;
        private System.Windows.Forms.CheckBox checkBoxShowNumber;
        private System.Windows.Forms.CheckBox checkBoxShowStartTime;
        private System.Windows.Forms.CheckBox checkBoxShowEndTime;
        private System.Windows.Forms.CheckBox checkBoxShowDuration;
    }
}