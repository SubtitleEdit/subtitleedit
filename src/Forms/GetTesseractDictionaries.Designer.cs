namespace Nikse.SubtitleEdit.Forms
{
    partial class GetTesseractDictionaries
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
            comboBoxDictionaries = new System.Windows.Forms.ComboBox();
            labelPleaseWait = new System.Windows.Forms.Label();
            labelChooseLanguageAndClickDownload = new System.Windows.Forms.Label();
            buttonDownload = new System.Windows.Forms.Button();
            linkLabelOpenDictionaryFolder = new System.Windows.Forms.LinkLabel();
            buttonOK = new System.Windows.Forms.Button();
            labelDescription1 = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // comboBoxDictionaries
            // 
            comboBoxDictionaries.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxDictionaries.FormattingEnabled = true;
            comboBoxDictionaries.Location = new System.Drawing.Point(22, 71);
            comboBoxDictionaries.Name = "comboBoxDictionaries";
            comboBoxDictionaries.Size = new System.Drawing.Size(256, 21);
            comboBoxDictionaries.TabIndex = 21;
            // 
            // labelPleaseWait
            // 
            labelPleaseWait.AutoSize = true;
            labelPleaseWait.Location = new System.Drawing.Point(19, 99);
            labelPleaseWait.Name = "labelPleaseWait";
            labelPleaseWait.Size = new System.Drawing.Size(70, 13);
            labelPleaseWait.TabIndex = 24;
            labelPleaseWait.Text = "Please wait...";
            // 
            // labelChooseLanguageAndClickDownload
            // 
            labelChooseLanguageAndClickDownload.AutoSize = true;
            labelChooseLanguageAndClickDownload.Location = new System.Drawing.Point(19, 52);
            labelChooseLanguageAndClickDownload.Name = "labelChooseLanguageAndClickDownload";
            labelChooseLanguageAndClickDownload.Size = new System.Drawing.Size(202, 13);
            labelChooseLanguageAndClickDownload.TabIndex = 23;
            labelChooseLanguageAndClickDownload.Text = "Choose your language and click download";
            // 
            // buttonDownload
            // 
            buttonDownload.Location = new System.Drawing.Point(284, 70);
            buttonDownload.Name = "buttonDownload";
            buttonDownload.Size = new System.Drawing.Size(104, 25);
            buttonDownload.TabIndex = 22;
            buttonDownload.Text = "&Download";
            buttonDownload.UseVisualStyleBackColor = true;
            buttonDownload.Click += new System.EventHandler(buttonDownload_Click);
            // 
            // linkLabelOpenDictionaryFolder
            // 
            linkLabelOpenDictionaryFolder.AutoSize = true;
            linkLabelOpenDictionaryFolder.Location = new System.Drawing.Point(19, 137);
            linkLabelOpenDictionaryFolder.Name = "linkLabelOpenDictionaryFolder";
            linkLabelOpenDictionaryFolder.Size = new System.Drawing.Size(124, 13);
            linkLabelOpenDictionaryFolder.TabIndex = 25;
            linkLabelOpenDictionaryFolder.TabStop = true;
            linkLabelOpenDictionaryFolder.Text = "Open \'Dictionaries\' folder";
            linkLabelOpenDictionaryFolder.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(linkLabelOpenDictionaryFolder_LinkClicked);
            // 
            // buttonOK
            // 
            buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            buttonOK.Location = new System.Drawing.Point(284, 132);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new System.Drawing.Size(104, 23);
            buttonOK.TabIndex = 26;
            buttonOK.Text = "&OK";
            buttonOK.UseVisualStyleBackColor = true;
            // 
            // labelDescription1
            // 
            labelDescription1.AutoSize = true;
            labelDescription1.Location = new System.Drawing.Point(19, 18);
            labelDescription1.Name = "labelDescription1";
            labelDescription1.Size = new System.Drawing.Size(220, 13);
            labelDescription1.TabIndex = 27;
            labelDescription1.Text = "Get Tesseract OCR dictionaries from the web";
            // 
            // GetTesseractDictionaries
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(426, 182);
            Controls.Add(labelDescription1);
            Controls.Add(comboBoxDictionaries);
            Controls.Add(labelPleaseWait);
            Controls.Add(labelChooseLanguageAndClickDownload);
            Controls.Add(buttonDownload);
            Controls.Add(linkLabelOpenDictionaryFolder);
            Controls.Add(buttonOK);
            KeyPreview = true;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "GetTesseractDictionaries";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "GetTesseractDictionaries";
            KeyDown += new System.Windows.Forms.KeyEventHandler(GetTesseractDictionaries_KeyDown);
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxDictionaries;
        private System.Windows.Forms.Label labelPleaseWait;
        private System.Windows.Forms.Label labelChooseLanguageAndClickDownload;
        private System.Windows.Forms.Button buttonDownload;
        private System.Windows.Forms.LinkLabel linkLabelOpenDictionaryFolder;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Label labelDescription1;
    }
}