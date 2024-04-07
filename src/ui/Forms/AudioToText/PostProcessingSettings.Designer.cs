namespace Nikse.SubtitleEdit.Forms.AudioToText
{
    sealed partial class PostProcessingSettings
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
            this.checkBoxFixCasing = new System.Windows.Forms.CheckBox();
            this.checkBoxAddPeriods = new System.Windows.Forms.CheckBox();
            this.checkBoxMergeShortLines = new System.Windows.Forms.CheckBox();
            this.checkBoxSplitLongLines = new System.Windows.Forms.CheckBox();
            this.checkBoxFixShortDuration = new System.Windows.Forms.CheckBox();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // checkBoxFixCasing
            // 
            this.checkBoxFixCasing.AutoSize = true;
            this.checkBoxFixCasing.Location = new System.Drawing.Point(24, 98);
            this.checkBoxFixCasing.Name = "checkBoxFixCasing";
            this.checkBoxFixCasing.Size = new System.Drawing.Size(73, 17);
            this.checkBoxFixCasing.TabIndex = 3;
            this.checkBoxFixCasing.Text = "Fix casing";
            this.checkBoxFixCasing.UseVisualStyleBackColor = true;
            // 
            // checkBoxAddPeriods
            // 
            this.checkBoxAddPeriods.AutoSize = true;
            this.checkBoxAddPeriods.Location = new System.Drawing.Point(24, 121);
            this.checkBoxAddPeriods.Name = "checkBoxAddPeriods";
            this.checkBoxAddPeriods.Size = new System.Drawing.Size(82, 17);
            this.checkBoxAddPeriods.TabIndex = 4;
            this.checkBoxAddPeriods.Text = "Add periods";
            this.checkBoxAddPeriods.UseVisualStyleBackColor = true;
            // 
            // checkBoxMergeShortLines
            // 
            this.checkBoxMergeShortLines.AutoSize = true;
            this.checkBoxMergeShortLines.Location = new System.Drawing.Point(24, 29);
            this.checkBoxMergeShortLines.Name = "checkBoxMergeShortLines";
            this.checkBoxMergeShortLines.Size = new System.Drawing.Size(106, 17);
            this.checkBoxMergeShortLines.TabIndex = 0;
            this.checkBoxMergeShortLines.Text = "Merge short lines";
            this.checkBoxMergeShortLines.UseVisualStyleBackColor = true;
            // 
            // checkBoxSplitLongLines
            // 
            this.checkBoxSplitLongLines.AutoSize = true;
            this.checkBoxSplitLongLines.Location = new System.Drawing.Point(24, 52);
            this.checkBoxSplitLongLines.Name = "checkBoxSplitLongLines";
            this.checkBoxSplitLongLines.Size = new System.Drawing.Size(93, 17);
            this.checkBoxSplitLongLines.TabIndex = 1;
            this.checkBoxSplitLongLines.Text = "Split long lines";
            this.checkBoxSplitLongLines.UseVisualStyleBackColor = true;
            // 
            // checkBoxFixShortDuration
            // 
            this.checkBoxFixShortDuration.AutoSize = true;
            this.checkBoxFixShortDuration.Location = new System.Drawing.Point(24, 75);
            this.checkBoxFixShortDuration.Name = "checkBoxFixShortDuration";
            this.checkBoxFixShortDuration.Size = new System.Drawing.Size(106, 17);
            this.checkBoxFixShortDuration.TabIndex = 2;
            this.checkBoxFixShortDuration.Text = "Fix short duration";
            this.checkBoxFixShortDuration.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(259, 151);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 6;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(178, 151);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 5;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // PostProcessingSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(346, 186);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.checkBoxFixShortDuration);
            this.Controls.Add(this.checkBoxSplitLongLines);
            this.Controls.Add(this.checkBoxMergeShortLines);
            this.Controls.Add(this.checkBoxAddPeriods);
            this.Controls.Add(this.checkBoxFixCasing);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PostProcessingSettings";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings";
            this.Shown += new System.EventHandler(this.PostProcessingSettings_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PostProcessingSettings_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBoxFixCasing;
        private System.Windows.Forms.CheckBox checkBoxAddPeriods;
        private System.Windows.Forms.CheckBox checkBoxMergeShortLines;
        private System.Windows.Forms.CheckBox checkBoxSplitLongLines;
        private System.Windows.Forms.CheckBox checkBoxFixShortDuration;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
    }
}