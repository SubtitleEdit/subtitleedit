namespace Nikse.SubtitleEdit.Forms.Options
{
    partial class SettingsFixContinuationStyle
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
            this.checkBoxUncheckInsertsAllCaps = new System.Windows.Forms.CheckBox();
            this.checkBoxUncheckInsertsItalic = new System.Windows.Forms.CheckBox();
            this.checkBoxUncheckInsertsLowercase = new System.Windows.Forms.CheckBox();
            this.checkBoxHideContinuationCandidatesWithoutName = new System.Windows.Forms.CheckBox();
            this.checkBoxIgnoreLyrics = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(416, 180);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 10;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(335, 180);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 9;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // checkBoxUncheckInsertsAllCaps
            // 
            this.checkBoxUncheckInsertsAllCaps.AutoSize = true;
            this.checkBoxUncheckInsertsAllCaps.Location = new System.Drawing.Point(12, 12);
            this.checkBoxUncheckInsertsAllCaps.Name = "checkBoxUncheckInsertsAllCaps";
            this.checkBoxUncheckInsertsAllCaps.Size = new System.Drawing.Size(332, 17);
            this.checkBoxUncheckInsertsAllCaps.TabIndex = 1;
            this.checkBoxUncheckInsertsAllCaps.Text = "Detect and uncheck inserts in all-caps (for example: NO ENTRY)";
            this.checkBoxUncheckInsertsAllCaps.UseVisualStyleBackColor = true;
            // 
            // checkBoxUncheckInsertsItalic
            // 
            this.checkBoxUncheckInsertsItalic.AutoSize = true;
            this.checkBoxUncheckInsertsItalic.Location = new System.Drawing.Point(12, 35);
            this.checkBoxUncheckInsertsItalic.Name = "checkBoxUncheckInsertsItalic";
            this.checkBoxUncheckInsertsItalic.Size = new System.Drawing.Size(330, 17);
            this.checkBoxUncheckInsertsItalic.TabIndex = 2;
            this.checkBoxUncheckInsertsItalic.Text = "Try to detect and uncheck inserts or lyrics in lowercase and italic";
            this.checkBoxUncheckInsertsItalic.UseVisualStyleBackColor = true;
            // 
            // checkBoxUncheckInsertsLowercase
            // 
            this.checkBoxUncheckInsertsLowercase.AutoSize = true;
            this.checkBoxUncheckInsertsLowercase.Location = new System.Drawing.Point(12, 58);
            this.checkBoxUncheckInsertsLowercase.Name = "checkBoxUncheckInsertsLowercase";
            this.checkBoxUncheckInsertsLowercase.Size = new System.Drawing.Size(285, 17);
            this.checkBoxUncheckInsertsLowercase.TabIndex = 3;
            this.checkBoxUncheckInsertsLowercase.Text = "Try to detect and uncheck inserts or lyrics in lowercase";
            this.checkBoxUncheckInsertsLowercase.UseVisualStyleBackColor = true;
            // 
            // checkBoxHideContinuationCandidatesWithoutName
            // 
            this.checkBoxHideContinuationCandidatesWithoutName.AutoSize = true;
            this.checkBoxHideContinuationCandidatesWithoutName.Location = new System.Drawing.Point(12, 81);
            this.checkBoxHideContinuationCandidatesWithoutName.Name = "checkBoxHideContinuationCandidatesWithoutName";
            this.checkBoxHideContinuationCandidatesWithoutName.Size = new System.Drawing.Size(349, 17);
            this.checkBoxHideContinuationCandidatesWithoutName.TabIndex = 4;
            this.checkBoxHideContinuationCandidatesWithoutName.Text = "Hide interruption continuation candidates that don\'t start with a name";
            this.checkBoxHideContinuationCandidatesWithoutName.UseVisualStyleBackColor = true;
            // 
            // checkBoxIgnoreLyrics
            // 
            this.checkBoxIgnoreLyrics.AutoSize = true;
            this.checkBoxIgnoreLyrics.Location = new System.Drawing.Point(12, 104);
            this.checkBoxIgnoreLyrics.Name = "checkBoxIgnoreLyrics";
            this.checkBoxIgnoreLyrics.Size = new System.Drawing.Size(196, 17);
            this.checkBoxIgnoreLyrics.TabIndex = 5;
            this.checkBoxIgnoreLyrics.Text = "Ignore lyrics between music symbols";
            this.checkBoxIgnoreLyrics.UseVisualStyleBackColor = true;
            // 
            // SettingsFixContinuationStyle
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(503, 215);
            this.Controls.Add(this.checkBoxIgnoreLyrics);
            this.Controls.Add(this.checkBoxHideContinuationCandidatesWithoutName);
            this.Controls.Add(this.checkBoxUncheckInsertsLowercase);
            this.Controls.Add(this.checkBoxUncheckInsertsItalic);
            this.Controls.Add(this.checkBoxUncheckInsertsAllCaps);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsFixContinuationStyle";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "SettingsFixContinuationStyle";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SettingsFixContinuationStyle_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.CheckBox checkBoxUncheckInsertsAllCaps;
        private System.Windows.Forms.CheckBox checkBoxUncheckInsertsItalic;
        private System.Windows.Forms.CheckBox checkBoxUncheckInsertsLowercase;
        private System.Windows.Forms.CheckBox checkBoxHideContinuationCandidatesWithoutName;
        private System.Windows.Forms.CheckBox checkBoxIgnoreLyrics;
    }
}