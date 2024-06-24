namespace Nikse.SubtitleEdit.Forms.Tts
{
    sealed partial class TtsAudioEncoding
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
            this.checkBoxMakeStereo = new System.Windows.Forms.CheckBox();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.labelAudioEnc = new Nikse.SubtitleEdit.Controls.NikseLabel();
            this.comboBoxAudioEnc = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.SuspendLayout();
            // 
            // checkBoxMakeStereo
            // 
            this.checkBoxMakeStereo.AutoSize = true;
            this.checkBoxMakeStereo.Checked = true;
            this.checkBoxMakeStereo.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxMakeStereo.Location = new System.Drawing.Point(102, 50);
            this.checkBoxMakeStereo.Name = "checkBoxMakeStereo";
            this.checkBoxMakeStereo.Size = new System.Drawing.Size(57, 17);
            this.checkBoxMakeStereo.TabIndex = 2;
            this.checkBoxMakeStereo.Text = "Stereo";
            this.checkBoxMakeStereo.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(179, 110);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 92;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(263, 110);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 93;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // labelAudioEnc
            // 
            this.labelAudioEnc.AutoSize = true;
            this.labelAudioEnc.Location = new System.Drawing.Point(25, 23);
            this.labelAudioEnc.Name = "labelAudioEnc";
            this.labelAudioEnc.Size = new System.Drawing.Size(52, 13);
            this.labelAudioEnc.TabIndex = 0;
            this.labelAudioEnc.Text = "Encoding";
            // 
            // comboBoxAudioEnc
            // 
            this.comboBoxAudioEnc.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxAudioEnc.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxAudioEnc.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxAudioEnc.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxAudioEnc.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxAudioEnc.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxAudioEnc.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxAudioEnc.DropDownHeight = 400;
            this.comboBoxAudioEnc.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxAudioEnc.DropDownWidth = 121;
            this.comboBoxAudioEnc.FormattingEnabled = true;
            this.comboBoxAudioEnc.Items.AddRange(new string[] {
            "copy",
            "aac",
            "ac3",
            "eac3",
            "truehd",
            "libvorbis",
            "libmp3lame",
            "libopus"});
            this.comboBoxAudioEnc.Location = new System.Drawing.Point(102, 21);
            this.comboBoxAudioEnc.MaxLength = 32767;
            this.comboBoxAudioEnc.Name = "comboBoxAudioEnc";
            this.comboBoxAudioEnc.SelectedIndex = -1;
            this.comboBoxAudioEnc.SelectedItem = null;
            this.comboBoxAudioEnc.SelectedText = "";
            this.comboBoxAudioEnc.Size = new System.Drawing.Size(121, 21);
            this.comboBoxAudioEnc.TabIndex = 1;
            this.comboBoxAudioEnc.UsePopupWindow = false;
            // 
            // TtsAudioEncoding
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(350, 145);
            this.Controls.Add(this.labelAudioEnc);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.comboBoxAudioEnc);
            this.Controls.Add(this.checkBoxMakeStereo);
            this.Controls.Add(this.buttonCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TtsAudioEncoding";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "TtsAudioEncoding";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private Controls.NikseLabel labelAudioEnc;
        private Controls.NikseComboBox comboBoxAudioEnc;
        private System.Windows.Forms.CheckBox checkBoxMakeStereo;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
    }
}