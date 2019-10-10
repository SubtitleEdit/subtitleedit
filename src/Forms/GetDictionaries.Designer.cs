namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class GetDictionaries
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
            this.buttonOK = new System.Windows.Forms.Button();
            this.labelDescription2 = new System.Windows.Forms.Label();
            this.linkLabelOpenDictionaryFolder = new System.Windows.Forms.LinkLabel();
            this.labelDescription1 = new System.Windows.Forms.Label();
            this.comboBoxDictionaries = new System.Windows.Forms.ComboBox();
            this.buttonDownload = new System.Windows.Forms.Button();
            this.labelChooseLanguageAndClickDownload = new System.Windows.Forms.Label();
            this.labelPleaseWait = new System.Windows.Forms.Label();
            this.buttonDownloadAll = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(356, 159);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(104, 23);
            this.buttonOK.TabIndex = 20;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            // 
            // labelDescription2
            // 
            this.labelDescription2.AutoSize = true;
            this.labelDescription2.Location = new System.Drawing.Point(30, 34);
            this.labelDescription2.Name = "labelDescription2";
            this.labelDescription2.Size = new System.Drawing.Size(263, 13);
            this.labelDescription2.TabIndex = 1;
            this.labelDescription2.Text = "uses the spell checking dictionaries from LibreOffice.";
            // 
            // linkLabelOpenDictionaryFolder
            // 
            this.linkLabelOpenDictionaryFolder.AutoSize = true;
            this.linkLabelOpenDictionaryFolder.Location = new System.Drawing.Point(30, 164);
            this.linkLabelOpenDictionaryFolder.Name = "linkLabelOpenDictionaryFolder";
            this.linkLabelOpenDictionaryFolder.Size = new System.Drawing.Size(126, 13);
            this.linkLabelOpenDictionaryFolder.TabIndex = 15;
            this.linkLabelOpenDictionaryFolder.TabStop = true;
            this.linkLabelOpenDictionaryFolder.Text = "Open \'Dictionaries\' folder";
            this.linkLabelOpenDictionaryFolder.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel4LinkClicked);
            // 
            // labelDescription1
            // 
            this.labelDescription1.AutoSize = true;
            this.labelDescription1.Location = new System.Drawing.Point(30, 16);
            this.labelDescription1.Name = "labelDescription1";
            this.labelDescription1.Size = new System.Drawing.Size(316, 13);
            this.labelDescription1.TabIndex = 8;
            this.labelDescription1.Text = "Subtitle Edit\'s spell check is based on the NHunspell engine which";
            // 
            // comboBoxDictionaries
            // 
            this.comboBoxDictionaries.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxDictionaries.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDictionaries.FormattingEnabled = true;
            this.comboBoxDictionaries.Location = new System.Drawing.Point(33, 98);
            this.comboBoxDictionaries.Name = "comboBoxDictionaries";
            this.comboBoxDictionaries.Size = new System.Drawing.Size(317, 21);
            this.comboBoxDictionaries.TabIndex = 0;
            this.comboBoxDictionaries.SelectedIndexChanged += new System.EventHandler(this.comboBoxDictionaries_SelectedIndexChanged);
            // 
            // buttonDownload
            // 
            this.buttonDownload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDownload.Location = new System.Drawing.Point(356, 97);
            this.buttonDownload.Name = "buttonDownload";
            this.buttonDownload.Size = new System.Drawing.Size(104, 25);
            this.buttonDownload.TabIndex = 10;
            this.buttonDownload.Text = "&Download";
            this.buttonDownload.UseVisualStyleBackColor = true;
            this.buttonDownload.Click += new System.EventHandler(this.buttonDownload_Click);
            // 
            // labelChooseLanguageAndClickDownload
            // 
            this.labelChooseLanguageAndClickDownload.AutoSize = true;
            this.labelChooseLanguageAndClickDownload.Location = new System.Drawing.Point(30, 79);
            this.labelChooseLanguageAndClickDownload.Name = "labelChooseLanguageAndClickDownload";
            this.labelChooseLanguageAndClickDownload.Size = new System.Drawing.Size(207, 13);
            this.labelChooseLanguageAndClickDownload.TabIndex = 11;
            this.labelChooseLanguageAndClickDownload.Text = "Choose your language and click download";
            // 
            // labelPleaseWait
            // 
            this.labelPleaseWait.AutoSize = true;
            this.labelPleaseWait.Location = new System.Drawing.Point(30, 126);
            this.labelPleaseWait.Name = "labelPleaseWait";
            this.labelPleaseWait.Size = new System.Drawing.Size(73, 13);
            this.labelPleaseWait.TabIndex = 12;
            this.labelPleaseWait.Text = "Please wait...";
            // 
            // buttonDownloadAll
            // 
            this.buttonDownloadAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDownloadAll.BackColor = System.Drawing.Color.Yellow;
            this.buttonDownloadAll.Location = new System.Drawing.Point(296, 54);
            this.buttonDownloadAll.Name = "buttonDownloadAll";
            this.buttonDownloadAll.Size = new System.Drawing.Size(164, 25);
            this.buttonDownloadAll.TabIndex = 21;
            this.buttonDownloadAll.Text = "Download all [DEBUG ONLY]";
            this.buttonDownloadAll.UseVisualStyleBackColor = false;
            this.buttonDownloadAll.Visible = false;
            this.buttonDownloadAll.Click += new System.EventHandler(this.buttonDownloadAll_Click);
            // 
            // GetDictionaries
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(472, 195);
            this.Controls.Add(this.buttonDownloadAll);
            this.Controls.Add(this.comboBoxDictionaries);
            this.Controls.Add(this.labelPleaseWait);
            this.Controls.Add(this.labelChooseLanguageAndClickDownload);
            this.Controls.Add(this.buttonDownload);
            this.Controls.Add(this.labelDescription1);
            this.Controls.Add(this.linkLabelOpenDictionaryFolder);
            this.Controls.Add(this.labelDescription2);
            this.Controls.Add(this.buttonOK);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GetDictionaries";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Need dictionaries?";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormGetDictionaries_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Label labelDescription2;
        private System.Windows.Forms.LinkLabel linkLabelOpenDictionaryFolder;
        private System.Windows.Forms.Label labelDescription1;
        private System.Windows.Forms.ComboBox comboBoxDictionaries;
        private System.Windows.Forms.Button buttonDownload;
        private System.Windows.Forms.Label labelChooseLanguageAndClickDownload;
        private System.Windows.Forms.Label labelPleaseWait;
        private System.Windows.Forms.Button buttonDownloadAll;
    }
}