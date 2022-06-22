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
            this.labelContinuationPause = new System.Windows.Forms.Label();
            this.numericUpDownContinuationPause = new System.Windows.Forms.NumericUpDown();
            this.labelMs = new System.Windows.Forms.Label();
            this.buttonEditCustomStyle = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownContinuationPause)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(416, 232);
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
            this.buttonOK.Location = new System.Drawing.Point(335, 232);
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
            // labelContinuationPause
            // 
            this.labelContinuationPause.AutoSize = true;
            this.labelContinuationPause.Location = new System.Drawing.Point(9, 140);
            this.labelContinuationPause.Name = "labelContinuationPause";
            this.labelContinuationPause.Size = new System.Drawing.Size(86, 13);
            this.labelContinuationPause.TabIndex = 11;
            this.labelContinuationPause.Text = "Pause threshold:";
            // 
            // numericUpDownContinuationPause
            // 
            this.numericUpDownContinuationPause.Location = new System.Drawing.Point(101, 138);
            this.numericUpDownContinuationPause.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.numericUpDownContinuationPause.Name = "numericUpDownContinuationPause";
            this.numericUpDownContinuationPause.Size = new System.Drawing.Size(56, 20);
            this.numericUpDownContinuationPause.TabIndex = 6;
            this.numericUpDownContinuationPause.Value = new decimal(new int[] {
            300,
            0,
            0,
            0});
            // 
            // labelMs
            // 
            this.labelMs.AutoSize = true;
            this.labelMs.Location = new System.Drawing.Point(163, 140);
            this.labelMs.Name = "labelMs";
            this.labelMs.Size = new System.Drawing.Size(20, 13);
            this.labelMs.TabIndex = 13;
            this.labelMs.Text = "ms";
            // 
            // buttonEditCustomStyle
            // 
            this.buttonEditCustomStyle.Location = new System.Drawing.Point(12, 177);
            this.buttonEditCustomStyle.Name = "buttonEditCustomStyle";
            this.buttonEditCustomStyle.Size = new System.Drawing.Size(240, 27);
            this.buttonEditCustomStyle.TabIndex = 7;
            this.buttonEditCustomStyle.Text = "Edit custom continuation style...";
            this.buttonEditCustomStyle.UseVisualStyleBackColor = true;
            this.buttonEditCustomStyle.Click += new System.EventHandler(this.buttonEditCustomStyle_Click);
            // 
            // SettingsFixContinuationStyle
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(503, 267);
            this.Controls.Add(this.buttonEditCustomStyle);
            this.Controls.Add(this.labelMs);
            this.Controls.Add(this.numericUpDownContinuationPause);
            this.Controls.Add(this.labelContinuationPause);
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
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownContinuationPause)).EndInit();
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
        private System.Windows.Forms.Label labelContinuationPause;
        private System.Windows.Forms.NumericUpDown numericUpDownContinuationPause;
        private System.Windows.Forms.Label labelMs;
        private System.Windows.Forms.Button buttonEditCustomStyle;
    }
}