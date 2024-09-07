using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Forms
{
    partial class Cavena890SaveOptions
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
            Nikse.SubtitleEdit.Core.Common.TimeCode timeCode1 = new Nikse.SubtitleEdit.Core.Common.TimeCode();
            this.labelTimeCodeStartOfProgramme = new System.Windows.Forms.Label();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonImport = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.textBoxComment = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.labelComment = new System.Windows.Forms.Label();
            this.textBoxTranslatedTitle = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.labelTranslatedTitle = new System.Windows.Forms.Label();
            this.textBoxOriginalTitle = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.labelOriginalTitle = new System.Windows.Forms.Label();
            this.textBoxTranslator = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.labelTranslator = new System.Windows.Forms.Label();
            this.timeUpDownStartTime = new Nikse.SubtitleEdit.Controls.NikseTimeUpDown();
            this.comboBoxLanguage = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelLanguage = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labelTimeCodeStartOfProgramme
            // 
            this.labelTimeCodeStartOfProgramme.AutoSize = true;
            this.labelTimeCodeStartOfProgramme.Location = new System.Drawing.Point(12, 182);
            this.labelTimeCodeStartOfProgramme.Name = "labelTimeCodeStartOfProgramme";
            this.labelTimeCodeStartOfProgramme.Size = new System.Drawing.Size(152, 13);
            this.labelTimeCodeStartOfProgramme.TabIndex = 10;
            this.labelTimeCodeStartOfProgramme.Text = "Time code: Start of programme";
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(250, 262);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 12;
            this.buttonOK.Text = "Save";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(331, 262);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 13;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonImport
            // 
            this.buttonImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonImport.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonImport.Location = new System.Drawing.Point(290, 12);
            this.buttonImport.Name = "buttonImport";
            this.buttonImport.Size = new System.Drawing.Size(116, 23);
            this.buttonImport.TabIndex = 14;
            this.buttonImport.Text = "Import...";
            this.buttonImport.UseVisualStyleBackColor = true;
            this.buttonImport.Click += new System.EventHandler(this.buttonImport_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // textBoxComment
            // 
            this.textBoxComment.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxComment.Location = new System.Drawing.Point(185, 119);
            this.textBoxComment.MaxLength = 24;
            this.textBoxComment.Name = "textBoxComment";
            this.textBoxComment.Size = new System.Drawing.Size(219, 20);
            this.textBoxComment.TabIndex = 7;
            // 
            // labelComment
            // 
            this.labelComment.AutoSize = true;
            this.labelComment.Location = new System.Drawing.Point(13, 122);
            this.labelComment.Name = "labelComment";
            this.labelComment.Size = new System.Drawing.Size(51, 13);
            this.labelComment.TabIndex = 6;
            this.labelComment.Text = "Comment";
            // 
            // textBoxTranslatedTitle
            // 
            this.textBoxTranslatedTitle.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxTranslatedTitle.Location = new System.Drawing.Point(185, 41);
            this.textBoxTranslatedTitle.MaxLength = 28;
            this.textBoxTranslatedTitle.Name = "textBoxTranslatedTitle";
            this.textBoxTranslatedTitle.Size = new System.Drawing.Size(219, 20);
            this.textBoxTranslatedTitle.TabIndex = 1;
            // 
            // labelTranslatedTitle
            // 
            this.labelTranslatedTitle.AutoSize = true;
            this.labelTranslatedTitle.Location = new System.Drawing.Point(13, 44);
            this.labelTranslatedTitle.Name = "labelTranslatedTitle";
            this.labelTranslatedTitle.Size = new System.Drawing.Size(76, 13);
            this.labelTranslatedTitle.TabIndex = 0;
            this.labelTranslatedTitle.Text = "Translated title";
            // 
            // textBoxOriginalTitle
            // 
            this.textBoxOriginalTitle.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxOriginalTitle.Location = new System.Drawing.Point(185, 67);
            this.textBoxOriginalTitle.MaxLength = 24;
            this.textBoxOriginalTitle.Name = "textBoxOriginalTitle";
            this.textBoxOriginalTitle.Size = new System.Drawing.Size(219, 20);
            this.textBoxOriginalTitle.TabIndex = 3;
            // 
            // labelOriginalTitle
            // 
            this.labelOriginalTitle.AutoSize = true;
            this.labelOriginalTitle.Location = new System.Drawing.Point(13, 70);
            this.labelOriginalTitle.Name = "labelOriginalTitle";
            this.labelOriginalTitle.Size = new System.Drawing.Size(61, 13);
            this.labelOriginalTitle.TabIndex = 2;
            this.labelOriginalTitle.Text = "Original title";
            // 
            // textBoxTranslator
            // 
            this.textBoxTranslator.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxTranslator.Location = new System.Drawing.Point(185, 93);
            this.textBoxTranslator.MaxLength = 24;
            this.textBoxTranslator.Name = "textBoxTranslator";
            this.textBoxTranslator.Size = new System.Drawing.Size(219, 20);
            this.textBoxTranslator.TabIndex = 5;
            // 
            // labelTranslator
            // 
            this.labelTranslator.AutoSize = true;
            this.labelTranslator.Location = new System.Drawing.Point(13, 96);
            this.labelTranslator.Name = "labelTranslator";
            this.labelTranslator.Size = new System.Drawing.Size(54, 13);
            this.labelTranslator.TabIndex = 4;
            this.labelTranslator.Text = "Translator";
            // 
            // timeUpDownStartTime
            // 
            this.timeUpDownStartTime.BackColor = System.Drawing.SystemColors.Window;
            this.timeUpDownStartTime.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.timeUpDownStartTime.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.timeUpDownStartTime.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.timeUpDownStartTime.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.timeUpDownStartTime.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.timeUpDownStartTime.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.timeUpDownStartTime.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.timeUpDownStartTime.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.timeUpDownStartTime.Location = new System.Drawing.Point(183, 177);
            this.timeUpDownStartTime.Margin = new System.Windows.Forms.Padding(4);
            this.timeUpDownStartTime.Name = "timeUpDownStartTime";
            this.timeUpDownStartTime.Size = new System.Drawing.Size(96, 23);
            this.timeUpDownStartTime.TabIndex = 11;
            this.timeUpDownStartTime.TabStop = false;
            timeCode1.Hours = 0;
            timeCode1.Milliseconds = 0;
            timeCode1.Minutes = 0;
            timeCode1.Seconds = 0;
            timeCode1.TimeSpan = System.TimeSpan.Parse("00:00:00");
            timeCode1.TotalMilliseconds = 0D;
            timeCode1.TotalSeconds = 0D;
            this.timeUpDownStartTime.TimeCode = timeCode1;
            this.timeUpDownStartTime.UseVideoOffset = false;
            // 
            // comboBoxLanguage
            // 
            this.comboBoxLanguage.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxLanguage.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxLanguage.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxLanguage.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxLanguage.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxLanguage.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxLanguage.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxLanguage.DropDownHeight = 400;
            this.comboBoxLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLanguage.DropDownWidth = 219;
            this.comboBoxLanguage.FormattingEnabled = true;
            this.comboBoxLanguage.Items.AddRange(new object[] {
            "Arabic",
            "Danish",
            "Chinese Simplified",
            "Chinese Traditional",
            "English",
            "Hebrew",
            "Russian",
            "Romanian"});
            this.comboBoxLanguage.Location = new System.Drawing.Point(185, 149);
            this.comboBoxLanguage.MaxLength = 32767;
            this.comboBoxLanguage.Name = "comboBoxLanguage";
            this.comboBoxLanguage.SelectedIndex = -1;
            this.comboBoxLanguage.SelectedItem = null;
            this.comboBoxLanguage.SelectedText = "";
            this.comboBoxLanguage.Size = new System.Drawing.Size(219, 21);
            this.comboBoxLanguage.TabIndex = 9;
            this.comboBoxLanguage.UsePopupWindow = false;
            // 
            // labelLanguage
            // 
            this.labelLanguage.AutoSize = true;
            this.labelLanguage.Location = new System.Drawing.Point(16, 152);
            this.labelLanguage.Name = "labelLanguage";
            this.labelLanguage.Size = new System.Drawing.Size(55, 13);
            this.labelLanguage.TabIndex = 8;
            this.labelLanguage.Text = "Language";
            // 
            // Cavena890SaveOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(418, 297);
            this.Controls.Add(this.labelLanguage);
            this.Controls.Add(this.comboBoxLanguage);
            this.Controls.Add(this.textBoxTranslator);
            this.Controls.Add(this.labelTranslator);
            this.Controls.Add(this.textBoxOriginalTitle);
            this.Controls.Add(this.labelOriginalTitle);
            this.Controls.Add(this.textBoxComment);
            this.Controls.Add(this.labelComment);
            this.Controls.Add(this.textBoxTranslatedTitle);
            this.Controls.Add(this.labelTranslatedTitle);
            this.Controls.Add(this.buttonImport);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.labelTimeCodeStartOfProgramme);
            this.Controls.Add(this.timeUpDownStartTime);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Cavena890SaveOptions";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Cavena 890 save options";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Cavena890SaveOptions_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelTimeCodeStartOfProgramme;
        private Controls.NikseTimeUpDown timeUpDownStartTime;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonImport;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private Nikse.SubtitleEdit.Controls.NikseTextBox textBoxComment;
        private System.Windows.Forms.Label labelComment;
        private Nikse.SubtitleEdit.Controls.NikseTextBox textBoxTranslatedTitle;
        private System.Windows.Forms.Label labelTranslatedTitle;
        private Nikse.SubtitleEdit.Controls.NikseTextBox textBoxOriginalTitle;
        private System.Windows.Forms.Label labelOriginalTitle;
        private Nikse.SubtitleEdit.Controls.NikseTextBox textBoxTranslator;
        private System.Windows.Forms.Label labelTranslator;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxLanguage;
        private System.Windows.Forms.Label labelLanguage;
    }
}