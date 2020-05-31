namespace Nikse.SubtitleEdit.Forms.Ocr
{
    sealed partial class BinaryOcrChooseEditDb
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
            this.buttonEditCharacterDatabase = new System.Windows.Forms.Button();
            this.buttonNewCharacterDatabase = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBoxNOcrLanguage = new System.Windows.Forms.ComboBox();
            this.comboBoxCharacterDatabase = new System.Windows.Forms.ComboBox();
            this.labelImageDatabase = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.linkLabelOpenDictionaryFolder = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // buttonEditCharacterDatabase
            // 
            this.buttonEditCharacterDatabase.Location = new System.Drawing.Point(215, 51);
            this.buttonEditCharacterDatabase.Name = "buttonEditCharacterDatabase";
            this.buttonEditCharacterDatabase.Size = new System.Drawing.Size(86, 23);
            this.buttonEditCharacterDatabase.TabIndex = 5;
            this.buttonEditCharacterDatabase.Text = "Edit";
            this.buttonEditCharacterDatabase.UseVisualStyleBackColor = true;
            this.buttonEditCharacterDatabase.Click += new System.EventHandler(this.buttonEditCharacterDatabase_Click);
            // 
            // buttonNewCharacterDatabase
            // 
            this.buttonNewCharacterDatabase.Location = new System.Drawing.Point(307, 51);
            this.buttonNewCharacterDatabase.Name = "buttonNewCharacterDatabase";
            this.buttonNewCharacterDatabase.Size = new System.Drawing.Size(86, 23);
            this.buttonNewCharacterDatabase.TabIndex = 4;
            this.buttonNewCharacterDatabase.Text = "New";
            this.buttonNewCharacterDatabase.UseVisualStyleBackColor = true;
            this.buttonNewCharacterDatabase.Click += new System.EventHandler(this.buttonNewCharacterDatabase_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(18, 108);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(124, 13);
            this.label2.TabIndex = 37;
            this.label2.Text = "nOCR fallback dictionary";
            // 
            // comboBoxNOcrLanguage
            // 
            this.comboBoxNOcrLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxNOcrLanguage.FormattingEnabled = true;
            this.comboBoxNOcrLanguage.Location = new System.Drawing.Point(21, 124);
            this.comboBoxNOcrLanguage.Name = "comboBoxNOcrLanguage";
            this.comboBoxNOcrLanguage.Size = new System.Drawing.Size(188, 21);
            this.comboBoxNOcrLanguage.TabIndex = 36;
            // 
            // comboBoxCharacterDatabase
            // 
            this.comboBoxCharacterDatabase.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxCharacterDatabase.FormattingEnabled = true;
            this.comboBoxCharacterDatabase.Location = new System.Drawing.Point(21, 51);
            this.comboBoxCharacterDatabase.Name = "comboBoxCharacterDatabase";
            this.comboBoxCharacterDatabase.Size = new System.Drawing.Size(188, 21);
            this.comboBoxCharacterDatabase.TabIndex = 39;
            // 
            // labelImageDatabase
            // 
            this.labelImageDatabase.AutoSize = true;
            this.labelImageDatabase.Location = new System.Drawing.Point(18, 35);
            this.labelImageDatabase.Name = "labelImageDatabase";
            this.labelImageDatabase.Size = new System.Drawing.Size(83, 13);
            this.labelImageDatabase.TabIndex = 38;
            this.labelImageDatabase.Text = "Image database";
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(320, 171);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 41;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(239, 171);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 40;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // linkLabelOpenDictionaryFolder
            // 
            this.linkLabelOpenDictionaryFolder.AutoSize = true;
            this.linkLabelOpenDictionaryFolder.Location = new System.Drawing.Point(18, 181);
            this.linkLabelOpenDictionaryFolder.Name = "linkLabelOpenDictionaryFolder";
            this.linkLabelOpenDictionaryFolder.Size = new System.Drawing.Size(124, 13);
            this.linkLabelOpenDictionaryFolder.TabIndex = 42;
            this.linkLabelOpenDictionaryFolder.TabStop = true;
            this.linkLabelOpenDictionaryFolder.Text = "Open \'Dictionaries\' folder";
            this.linkLabelOpenDictionaryFolder.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelOpenDictionaryFolder_LinkClicked);
            // 
            // BinaryOcrChooseEditDb
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(407, 206);
            this.Controls.Add(this.linkLabelOpenDictionaryFolder);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.comboBoxCharacterDatabase);
            this.Controls.Add(this.labelImageDatabase);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.comboBoxNOcrLanguage);
            this.Controls.Add(this.buttonEditCharacterDatabase);
            this.Controls.Add(this.buttonNewCharacterDatabase);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BinaryOcrChooseEditDb";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "BinaryOcrChooseEditDb";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.BinaryOcrChooseEditDb_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonEditCharacterDatabase;
        private System.Windows.Forms.Button buttonNewCharacterDatabase;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBoxNOcrLanguage;
        private System.Windows.Forms.ComboBox comboBoxCharacterDatabase;
        private System.Windows.Forms.Label labelImageDatabase;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.LinkLabel linkLabelOpenDictionaryFolder;
    }
}