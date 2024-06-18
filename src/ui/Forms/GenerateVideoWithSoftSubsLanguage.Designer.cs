namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class GenerateVideoWithSoftSubsLanguage
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
            this.labelLanguage = new System.Windows.Forms.Label();
            this.comboBoxLanguages = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelLanguageCode = new System.Windows.Forms.Label();
            this.textBoxLanguageCode = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.textBoxTitle = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.labelTitle = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(322, 201);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 101;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(241, 201);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 100;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            // 
            // labelLanguage
            // 
            this.labelLanguage.AutoSize = true;
            this.labelLanguage.Location = new System.Drawing.Point(12, 14);
            this.labelLanguage.Name = "labelLanguage";
            this.labelLanguage.Size = new System.Drawing.Size(54, 13);
            this.labelLanguage.TabIndex = 15;
            this.labelLanguage.Text = "Language";
            // 
            // comboBoxLanguages
            // 
            this.comboBoxLanguages.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxLanguages.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxLanguages.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxLanguages.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxLanguages.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxLanguages.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxLanguages.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxLanguages.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxLanguages.DropDownHeight = 400;
            this.comboBoxLanguages.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLanguages.DropDownWidth = 266;
            this.comboBoxLanguages.FormattingEnabled = true;
            this.comboBoxLanguages.Location = new System.Drawing.Point(12, 31);
            this.comboBoxLanguages.MaxLength = 32767;
            this.comboBoxLanguages.Name = "comboBoxLanguages";
            this.comboBoxLanguages.SelectedIndex = -1;
            this.comboBoxLanguages.SelectedItem = null;
            this.comboBoxLanguages.SelectedText = "";
            this.comboBoxLanguages.Size = new System.Drawing.Size(384, 21);
            this.comboBoxLanguages.TabIndex = 0;
            this.comboBoxLanguages.UsePopupWindow = false;
            // 
            // labelLanguageCode
            // 
            this.labelLanguageCode.AutoSize = true;
            this.labelLanguageCode.Location = new System.Drawing.Point(12, 78);
            this.labelLanguageCode.Name = "labelLanguageCode";
            this.labelLanguageCode.Size = new System.Drawing.Size(80, 13);
            this.labelLanguageCode.TabIndex = 16;
            this.labelLanguageCode.Text = "Language code";
            // 
            // textBoxLanguageCode
            // 
            this.textBoxLanguageCode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxLanguageCode.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxLanguageCode.Location = new System.Drawing.Point(12, 94);
            this.textBoxLanguageCode.Name = "textBoxLanguageCode";
            this.textBoxLanguageCode.Size = new System.Drawing.Size(384, 21);
            this.textBoxLanguageCode.TabIndex = 10;
            // 
            // textBoxTitle
            // 
            this.textBoxTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxTitle.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxTitle.Location = new System.Drawing.Point(12, 146);
            this.textBoxTitle.Name = "textBoxTitle";
            this.textBoxTitle.Size = new System.Drawing.Size(384, 21);
            this.textBoxTitle.TabIndex = 102;
            // 
            // labelTitle
            // 
            this.labelTitle.AutoSize = true;
            this.labelTitle.Location = new System.Drawing.Point(12, 130);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(27, 13);
            this.labelTitle.TabIndex = 103;
            this.labelTitle.Text = "Title";
            // 
            // GenerateVideoWithSoftSubsLanguage
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(409, 234);
            this.Controls.Add(this.textBoxTitle);
            this.Controls.Add(this.labelTitle);
            this.Controls.Add(this.textBoxLanguageCode);
            this.Controls.Add(this.labelLanguageCode);
            this.Controls.Add(this.labelLanguage);
            this.Controls.Add(this.comboBoxLanguages);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GenerateVideoWithSoftSubsLanguage";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Choose language";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ChangeLanguage_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Label labelLanguage;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxLanguages;
        private System.Windows.Forms.Label labelLanguageCode;
        private Controls.NikseTextBox textBoxLanguageCode;
        private Controls.NikseTextBox textBoxTitle;
        private System.Windows.Forms.Label labelTitle;
    }
}