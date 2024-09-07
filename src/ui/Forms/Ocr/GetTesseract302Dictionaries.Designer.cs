namespace Nikse.SubtitleEdit.Forms.Ocr
{
    sealed partial class GetTesseract302Dictionaries
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
            this.comboBoxDictionaries = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelPleaseWait = new System.Windows.Forms.Label();
            this.labelChooseLanguageAndClickDownload = new System.Windows.Forms.Label();
            this.buttonDownload = new System.Windows.Forms.Button();
            this.linkLabelOpenDictionaryFolder = new System.Windows.Forms.LinkLabel();
            this.buttonOK = new System.Windows.Forms.Button();
            this.labelDescription1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // comboBoxDictionaries
            // 
            this.comboBoxDictionaries.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxDictionaries.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxDictionaries.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxDictionaries.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxDictionaries.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxDictionaries.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxDictionaries.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxDictionaries.DropDownHeight = 400;
            this.comboBoxDictionaries.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDictionaries.DropDownWidth = 256;
            this.comboBoxDictionaries.FormattingEnabled = true;
            this.comboBoxDictionaries.Location = new System.Drawing.Point(22, 71);
            this.comboBoxDictionaries.MaxLength = 32767;
            this.comboBoxDictionaries.Name = "comboBoxDictionaries";
            this.comboBoxDictionaries.SelectedIndex = -1;
            this.comboBoxDictionaries.SelectedItem = null;
            this.comboBoxDictionaries.SelectedText = "";
            this.comboBoxDictionaries.Size = new System.Drawing.Size(256, 21);
            this.comboBoxDictionaries.TabIndex = 21;
            this.comboBoxDictionaries.UsePopupWindow = false;
            this.comboBoxDictionaries.SelectedIndexChanged += new System.EventHandler(this.comboBoxDictionaries_SelectedIndexChanged);
            // 
            // labelPleaseWait
            // 
            this.labelPleaseWait.AutoSize = true;
            this.labelPleaseWait.Location = new System.Drawing.Point(19, 99);
            this.labelPleaseWait.Name = "labelPleaseWait";
            this.labelPleaseWait.Size = new System.Drawing.Size(70, 13);
            this.labelPleaseWait.TabIndex = 24;
            this.labelPleaseWait.Text = "Please wait...";
            // 
            // labelChooseLanguageAndClickDownload
            // 
            this.labelChooseLanguageAndClickDownload.AutoSize = true;
            this.labelChooseLanguageAndClickDownload.Location = new System.Drawing.Point(19, 52);
            this.labelChooseLanguageAndClickDownload.Name = "labelChooseLanguageAndClickDownload";
            this.labelChooseLanguageAndClickDownload.Size = new System.Drawing.Size(208, 13);
            this.labelChooseLanguageAndClickDownload.TabIndex = 23;
            this.labelChooseLanguageAndClickDownload.Text = "Choose your language and click download";
            // 
            // buttonDownload
            // 
            this.buttonDownload.Location = new System.Drawing.Point(284, 70);
            this.buttonDownload.Name = "buttonDownload";
            this.buttonDownload.Size = new System.Drawing.Size(104, 25);
            this.buttonDownload.TabIndex = 22;
            this.buttonDownload.Text = "&Download";
            this.buttonDownload.UseVisualStyleBackColor = true;
            this.buttonDownload.Click += new System.EventHandler(this.buttonDownload_Click);
            // 
            // linkLabelOpenDictionaryFolder
            // 
            this.linkLabelOpenDictionaryFolder.AutoSize = true;
            this.linkLabelOpenDictionaryFolder.Location = new System.Drawing.Point(19, 137);
            this.linkLabelOpenDictionaryFolder.Name = "linkLabelOpenDictionaryFolder";
            this.linkLabelOpenDictionaryFolder.Size = new System.Drawing.Size(124, 13);
            this.linkLabelOpenDictionaryFolder.TabIndex = 25;
            this.linkLabelOpenDictionaryFolder.TabStop = true;
            this.linkLabelOpenDictionaryFolder.Text = "Open \'Dictionaries\' folder";
            this.linkLabelOpenDictionaryFolder.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelOpenDictionaryFolder_LinkClicked);
            // 
            // buttonOK
            // 
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(284, 132);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(104, 23);
            this.buttonOK.TabIndex = 26;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            // 
            // labelDescription1
            // 
            this.labelDescription1.AutoSize = true;
            this.labelDescription1.Location = new System.Drawing.Point(19, 18);
            this.labelDescription1.Name = "labelDescription1";
            this.labelDescription1.Size = new System.Drawing.Size(220, 13);
            this.labelDescription1.TabIndex = 27;
            this.labelDescription1.Text = "Get Tesseract OCR dictionaries from the web";
            // 
            // GetTesseract302Dictionaries
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(426, 182);
            this.Controls.Add(this.labelDescription1);
            this.Controls.Add(this.comboBoxDictionaries);
            this.Controls.Add(this.labelPleaseWait);
            this.Controls.Add(this.labelChooseLanguageAndClickDownload);
            this.Controls.Add(this.buttonDownload);
            this.Controls.Add(this.linkLabelOpenDictionaryFolder);
            this.Controls.Add(this.buttonOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GetTesseract302Dictionaries";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "GetTesseractDictionaries";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GetTesseractDictionaries_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxDictionaries;
        private System.Windows.Forms.Label labelPleaseWait;
        private System.Windows.Forms.Label labelChooseLanguageAndClickDownload;
        private System.Windows.Forms.Button buttonDownload;
        private System.Windows.Forms.LinkLabel linkLabelOpenDictionaryFolder;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Label labelDescription1;
    }
}