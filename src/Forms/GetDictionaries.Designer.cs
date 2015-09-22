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
            buttonOK = new System.Windows.Forms.Button();
            labelDescription2 = new System.Windows.Forms.Label();
            linkLabelOpenDictionaryFolder = new System.Windows.Forms.LinkLabel();
            labelDescription1 = new System.Windows.Forms.Label();
            comboBoxDictionaries = new System.Windows.Forms.ComboBox();
            buttonDownload = new System.Windows.Forms.Button();
            labelChooseLanguageAndClickDownload = new System.Windows.Forms.Label();
            labelPleaseWait = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // buttonOK
            // 
            buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            buttonOK.Location = new System.Drawing.Point(356, 159);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new System.Drawing.Size(104, 23);
            buttonOK.TabIndex = 20;
            buttonOK.Text = "&OK";
            buttonOK.UseVisualStyleBackColor = true;
            // 
            // labelDescription2
            // 
            labelDescription2.AutoSize = true;
            labelDescription2.Location = new System.Drawing.Point(30, 34);
            labelDescription2.Name = "labelDescription2";
            labelDescription2.Size = new System.Drawing.Size(263, 13);
            labelDescription2.TabIndex = 1;
            labelDescription2.Text = "uses the spell checking dictionaries from Open Office.";
            // 
            // linkLabelOpenDictionaryFolder
            // 
            linkLabelOpenDictionaryFolder.AutoSize = true;
            linkLabelOpenDictionaryFolder.Location = new System.Drawing.Point(30, 164);
            linkLabelOpenDictionaryFolder.Name = "linkLabelOpenDictionaryFolder";
            linkLabelOpenDictionaryFolder.Size = new System.Drawing.Size(126, 13);
            linkLabelOpenDictionaryFolder.TabIndex = 15;
            linkLabelOpenDictionaryFolder.TabStop = true;
            linkLabelOpenDictionaryFolder.Text = "Open \'Dictionaries\' folder";
            linkLabelOpenDictionaryFolder.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(LinkLabel4LinkClicked);
            // 
            // labelDescription1
            // 
            labelDescription1.AutoSize = true;
            labelDescription1.Location = new System.Drawing.Point(30, 16);
            labelDescription1.Name = "labelDescription1";
            labelDescription1.Size = new System.Drawing.Size(316, 13);
            labelDescription1.TabIndex = 8;
            labelDescription1.Text = "Subtitle Edit\'s spell check is based on the NHunspell engine which";
            // 
            // comboBoxDictionaries
            // 
            comboBoxDictionaries.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            comboBoxDictionaries.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxDictionaries.FormattingEnabled = true;
            comboBoxDictionaries.Location = new System.Drawing.Point(33, 98);
            comboBoxDictionaries.Name = "comboBoxDictionaries";
            comboBoxDictionaries.Size = new System.Drawing.Size(317, 21);
            comboBoxDictionaries.TabIndex = 0;
            comboBoxDictionaries.SelectedIndexChanged += new System.EventHandler(comboBoxDictionaries_SelectedIndexChanged);
            // 
            // buttonDownload
            // 
            buttonDownload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            buttonDownload.Location = new System.Drawing.Point(356, 97);
            buttonDownload.Name = "buttonDownload";
            buttonDownload.Size = new System.Drawing.Size(104, 25);
            buttonDownload.TabIndex = 10;
            buttonDownload.Text = "&Download";
            buttonDownload.UseVisualStyleBackColor = true;
            buttonDownload.Click += new System.EventHandler(buttonDownload_Click);
            // 
            // labelChooseLanguageAndClickDownload
            // 
            labelChooseLanguageAndClickDownload.AutoSize = true;
            labelChooseLanguageAndClickDownload.Location = new System.Drawing.Point(30, 79);
            labelChooseLanguageAndClickDownload.Name = "labelChooseLanguageAndClickDownload";
            labelChooseLanguageAndClickDownload.Size = new System.Drawing.Size(201, 13);
            labelChooseLanguageAndClickDownload.TabIndex = 11;
            labelChooseLanguageAndClickDownload.Text = "Choose your language and click download";
            // 
            // labelPleaseWait
            // 
            labelPleaseWait.AutoSize = true;
            labelPleaseWait.Location = new System.Drawing.Point(30, 126);
            labelPleaseWait.Name = "labelPleaseWait";
            labelPleaseWait.Size = new System.Drawing.Size(73, 13);
            labelPleaseWait.TabIndex = 12;
            labelPleaseWait.Text = "Please wait...";
            // 
            // GetDictionaries
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(472, 195);
            Controls.Add(comboBoxDictionaries);
            Controls.Add(labelPleaseWait);
            Controls.Add(labelChooseLanguageAndClickDownload);
            Controls.Add(buttonDownload);
            Controls.Add(labelDescription1);
            Controls.Add(linkLabelOpenDictionaryFolder);
            Controls.Add(labelDescription2);
            Controls.Add(buttonOK);
            Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            KeyPreview = true;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "GetDictionaries";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Need dictionaries?";
            KeyDown += new System.Windows.Forms.KeyEventHandler(FormGetDictionaries_KeyDown);
            ResumeLayout(false);
            PerformLayout();

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
    }
}