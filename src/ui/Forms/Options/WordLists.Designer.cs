namespace Nikse.SubtitleEdit.Forms.Options
{
    sealed partial class WordLists
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
            this.groupBoxWordLists = new System.Windows.Forms.GroupBox();
            this.linkLabelOpenDictionaryFolder = new System.Windows.Forms.LinkLabel();
            this.groupBoxOcrFixList = new System.Windows.Forms.GroupBox();
            this.textBoxOcrFixValue = new Nikse.SubtitleEdit.Controls.SETextBox(true);
            this.buttonRemoveOcrFix = new System.Windows.Forms.Button();
            this.listBoxOcrFixList = new Nikse.SubtitleEdit.Controls.NikseListBox();
            this.textBoxOcrFixKey = new Nikse.SubtitleEdit.Controls.SETextBox(true);
            this.buttonAddOcrFix = new System.Windows.Forms.Button();
            this.groupBoxUserWordList = new System.Windows.Forms.GroupBox();
            this.buttonRemoveUserWord = new System.Windows.Forms.Button();
            this.listBoxUserWordLists = new Nikse.SubtitleEdit.Controls.NikseListBox();
            this.textBoxUserWord = new Nikse.SubtitleEdit.Controls.SETextBox(true);
            this.buttonAddUserWord = new System.Windows.Forms.Button();
            this.groupBoxWordListLocation = new System.Windows.Forms.GroupBox();
            this.checkBoxNamesOnline = new System.Windows.Forms.CheckBox();
            this.textBoxNamesOnline = new Nikse.SubtitleEdit.Controls.SETextBox(true);
            this.groupBoxNamesIgonoreLists = new System.Windows.Forms.GroupBox();
            this.listViewNames = new System.Windows.Forms.ListView();
            this.columnHeaderNames = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonRemoveNameEtc = new System.Windows.Forms.Button();
            this.textBoxNameEtc = new Nikse.SubtitleEdit.Controls.SETextBox(true);
            this.buttonAddNames = new System.Windows.Forms.Button();
            this.labelWordListLanguage = new System.Windows.Forms.Label();
            this.comboBoxWordListLanguage = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.labelStatus = new System.Windows.Forms.Label();
            this.groupBoxWordLists.SuspendLayout();
            this.groupBoxOcrFixList.SuspendLayout();
            this.groupBoxUserWordList.SuspendLayout();
            this.groupBoxWordListLocation.SuspendLayout();
            this.groupBoxNamesIgonoreLists.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxWordLists
            // 
            this.groupBoxWordLists.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxWordLists.Controls.Add(this.linkLabelOpenDictionaryFolder);
            this.groupBoxWordLists.Controls.Add(this.groupBoxOcrFixList);
            this.groupBoxWordLists.Controls.Add(this.groupBoxUserWordList);
            this.groupBoxWordLists.Controls.Add(this.groupBoxWordListLocation);
            this.groupBoxWordLists.Controls.Add(this.groupBoxNamesIgonoreLists);
            this.groupBoxWordLists.Controls.Add(this.labelWordListLanguage);
            this.groupBoxWordLists.Controls.Add(this.comboBoxWordListLanguage);
            this.groupBoxWordLists.Location = new System.Drawing.Point(12, 12);
            this.groupBoxWordLists.Name = "groupBoxWordLists";
            this.groupBoxWordLists.Size = new System.Drawing.Size(836, 520);
            this.groupBoxWordLists.TabIndex = 3;
            this.groupBoxWordLists.TabStop = false;
            this.groupBoxWordLists.Text = "Word lists";
            // 
            // linkLabelOpenDictionaryFolder
            // 
            this.linkLabelOpenDictionaryFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkLabelOpenDictionaryFolder.AutoSize = true;
            this.linkLabelOpenDictionaryFolder.Location = new System.Drawing.Point(6, 495);
            this.linkLabelOpenDictionaryFolder.Name = "linkLabelOpenDictionaryFolder";
            this.linkLabelOpenDictionaryFolder.Size = new System.Drawing.Size(124, 13);
            this.linkLabelOpenDictionaryFolder.TabIndex = 29;
            this.linkLabelOpenDictionaryFolder.TabStop = true;
            this.linkLabelOpenDictionaryFolder.Text = "Open \'Dictionaries\' folder";
            this.linkLabelOpenDictionaryFolder.Click += new System.EventHandler(this.linkLabelOpenDictionaryFolder_LinkClicked);
            // 
            // groupBoxOcrFixList
            // 
            this.groupBoxOcrFixList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxOcrFixList.Controls.Add(this.textBoxOcrFixValue);
            this.groupBoxOcrFixList.Controls.Add(this.buttonRemoveOcrFix);
            this.groupBoxOcrFixList.Controls.Add(this.listBoxOcrFixList);
            this.groupBoxOcrFixList.Controls.Add(this.textBoxOcrFixKey);
            this.groupBoxOcrFixList.Controls.Add(this.buttonAddOcrFix);
            this.groupBoxOcrFixList.Location = new System.Drawing.Point(510, 43);
            this.groupBoxOcrFixList.Name = "groupBoxOcrFixList";
            this.groupBoxOcrFixList.Size = new System.Drawing.Size(301, 345);
            this.groupBoxOcrFixList.TabIndex = 6;
            this.groupBoxOcrFixList.TabStop = false;
            this.groupBoxOcrFixList.Text = "OCR fix list";
            // 
            // textBoxOcrFixValue
            // 
            this.textBoxOcrFixValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBoxOcrFixValue.Location = new System.Drawing.Point(99, 309);
            this.textBoxOcrFixValue.Name = "textBoxOcrFixValue";
            this.textBoxOcrFixValue.Size = new System.Drawing.Size(85, 20);
            this.textBoxOcrFixValue.TabIndex = 45;
            this.textBoxOcrFixValue.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBoxOcrFixValueKeyDown);
            // 
            // buttonRemoveOcrFix
            // 
            this.buttonRemoveOcrFix.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRemoveOcrFix.Location = new System.Drawing.Point(199, 16);
            this.buttonRemoveOcrFix.Name = "buttonRemoveOcrFix";
            this.buttonRemoveOcrFix.Size = new System.Drawing.Size(75, 23);
            this.buttonRemoveOcrFix.TabIndex = 42;
            this.buttonRemoveOcrFix.Text = "Remove";
            this.buttonRemoveOcrFix.UseVisualStyleBackColor = true;
            this.buttonRemoveOcrFix.Click += new System.EventHandler(this.ButtonRemoveOcrFixClick);
            // 
            // listBoxOcrFixList
            // 
            this.listBoxOcrFixList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxOcrFixList.FormattingEnabled = true;
            this.listBoxOcrFixList.Location = new System.Drawing.Point(6, 16);
            this.listBoxOcrFixList.Name = "listBoxOcrFixList";
            this.listBoxOcrFixList.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listBoxOcrFixList.Size = new System.Drawing.Size(187, 277);
            this.listBoxOcrFixList.TabIndex = 40;
            this.listBoxOcrFixList.SelectedIndexChanged += new System.EventHandler(this.ListBoxOcrFixListSelectedIndexChanged);
            this.listBoxOcrFixList.DoubleClick += new System.EventHandler(this.listBoxOcrFixList_DoubleClick);
            this.listBoxOcrFixList.Enter += new System.EventHandler(this.ListBoxSearchReset);
            this.listBoxOcrFixList.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listBoxOcrFixList_KeyDown);
            // 
            // textBoxOcrFixKey
            // 
            this.textBoxOcrFixKey.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBoxOcrFixKey.Location = new System.Drawing.Point(5, 309);
            this.textBoxOcrFixKey.Name = "textBoxOcrFixKey";
            this.textBoxOcrFixKey.Size = new System.Drawing.Size(88, 20);
            this.textBoxOcrFixKey.TabIndex = 44;
            // 
            // buttonAddOcrFix
            // 
            this.buttonAddOcrFix.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonAddOcrFix.Location = new System.Drawing.Point(190, 307);
            this.buttonAddOcrFix.Name = "buttonAddOcrFix";
            this.buttonAddOcrFix.Size = new System.Drawing.Size(75, 23);
            this.buttonAddOcrFix.TabIndex = 46;
            this.buttonAddOcrFix.Text = "Add pair";
            this.buttonAddOcrFix.UseVisualStyleBackColor = true;
            this.buttonAddOcrFix.Click += new System.EventHandler(this.ButtonAddOcrFixClick);
            // 
            // groupBoxUserWordList
            // 
            this.groupBoxUserWordList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxUserWordList.Controls.Add(this.buttonRemoveUserWord);
            this.groupBoxUserWordList.Controls.Add(this.listBoxUserWordLists);
            this.groupBoxUserWordList.Controls.Add(this.textBoxUserWord);
            this.groupBoxUserWordList.Controls.Add(this.buttonAddUserWord);
            this.groupBoxUserWordList.Location = new System.Drawing.Point(259, 43);
            this.groupBoxUserWordList.Name = "groupBoxUserWordList";
            this.groupBoxUserWordList.Size = new System.Drawing.Size(249, 345);
            this.groupBoxUserWordList.TabIndex = 4;
            this.groupBoxUserWordList.TabStop = false;
            this.groupBoxUserWordList.Text = "User word list";
            // 
            // buttonRemoveUserWord
            // 
            this.buttonRemoveUserWord.Location = new System.Drawing.Point(159, 16);
            this.buttonRemoveUserWord.Name = "buttonRemoveUserWord";
            this.buttonRemoveUserWord.Size = new System.Drawing.Size(75, 23);
            this.buttonRemoveUserWord.TabIndex = 32;
            this.buttonRemoveUserWord.Text = "Remove";
            this.buttonRemoveUserWord.UseVisualStyleBackColor = true;
            this.buttonRemoveUserWord.Click += new System.EventHandler(this.ButtonRemoveUserWordClick);
            // 
            // listBoxUserWordLists
            // 
            this.listBoxUserWordLists.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listBoxUserWordLists.FormattingEnabled = true;
            this.listBoxUserWordLists.Location = new System.Drawing.Point(3, 16);
            this.listBoxUserWordLists.Name = "listBoxUserWordLists";
            this.listBoxUserWordLists.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listBoxUserWordLists.Size = new System.Drawing.Size(150, 277);
            this.listBoxUserWordLists.TabIndex = 30;
            this.listBoxUserWordLists.SelectedIndexChanged += new System.EventHandler(this.ListBoxUserWordListsSelectedIndexChanged);
            this.listBoxUserWordLists.DoubleClick += new System.EventHandler(this.listBoxUserWordLists_DoubleClick);
            this.listBoxUserWordLists.Enter += new System.EventHandler(this.ListBoxSearchReset);
            this.listBoxUserWordLists.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listBoxUserWordLists_KeyDown);
            // 
            // textBoxUserWord
            // 
            this.textBoxUserWord.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBoxUserWord.Location = new System.Drawing.Point(2, 309);
            this.textBoxUserWord.Name = "textBoxUserWord";
            this.textBoxUserWord.Size = new System.Drawing.Size(150, 20);
            this.textBoxUserWord.TabIndex = 34;
            this.textBoxUserWord.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBoxUserWordKeyDown);
            // 
            // buttonAddUserWord
            // 
            this.buttonAddUserWord.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonAddUserWord.Location = new System.Drawing.Point(159, 307);
            this.buttonAddUserWord.Name = "buttonAddUserWord";
            this.buttonAddUserWord.Size = new System.Drawing.Size(75, 23);
            this.buttonAddUserWord.TabIndex = 36;
            this.buttonAddUserWord.Text = "Add word";
            this.buttonAddUserWord.UseVisualStyleBackColor = true;
            this.buttonAddUserWord.Click += new System.EventHandler(this.ButtonAddUserWordClick);
            // 
            // groupBoxWordListLocation
            // 
            this.groupBoxWordListLocation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxWordListLocation.Controls.Add(this.checkBoxNamesOnline);
            this.groupBoxWordListLocation.Controls.Add(this.textBoxNamesOnline);
            this.groupBoxWordListLocation.Location = new System.Drawing.Point(6, 405);
            this.groupBoxWordListLocation.Name = "groupBoxWordListLocation";
            this.groupBoxWordListLocation.Size = new System.Drawing.Size(805, 74);
            this.groupBoxWordListLocation.TabIndex = 8;
            this.groupBoxWordListLocation.TabStop = false;
            this.groupBoxWordListLocation.Text = "Location";
            // 
            // checkBoxNamesOnline
            // 
            this.checkBoxNamesOnline.AutoSize = true;
            this.checkBoxNamesOnline.Location = new System.Drawing.Point(7, 22);
            this.checkBoxNamesOnline.Name = "checkBoxNamesOnline";
            this.checkBoxNamesOnline.Size = new System.Drawing.Size(144, 17);
            this.checkBoxNamesOnline.TabIndex = 26;
            this.checkBoxNamesOnline.Text = "Use online names xml file";
            this.checkBoxNamesOnline.UseVisualStyleBackColor = true;
            // 
            // textBoxNamesOnline
            // 
            this.textBoxNamesOnline.Location = new System.Drawing.Point(6, 45);
            this.textBoxNamesOnline.Name = "textBoxNamesOnline";
            this.textBoxNamesOnline.Size = new System.Drawing.Size(764, 20);
            this.textBoxNamesOnline.TabIndex = 28;
            this.textBoxNamesOnline.Text = "https://raw.githubusercontent.com/SubtitleEdit/subtitleedit/master/Dictionaries/n" +
    "ames.xml";
            // 
            // groupBoxNamesIgonoreLists
            // 
            this.groupBoxNamesIgonoreLists.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxNamesIgonoreLists.Controls.Add(this.listViewNames);
            this.groupBoxNamesIgonoreLists.Controls.Add(this.buttonRemoveNameEtc);
            this.groupBoxNamesIgonoreLists.Controls.Add(this.textBoxNameEtc);
            this.groupBoxNamesIgonoreLists.Controls.Add(this.buttonAddNames);
            this.groupBoxNamesIgonoreLists.Location = new System.Drawing.Point(6, 43);
            this.groupBoxNamesIgonoreLists.Name = "groupBoxNamesIgonoreLists";
            this.groupBoxNamesIgonoreLists.Size = new System.Drawing.Size(249, 345);
            this.groupBoxNamesIgonoreLists.TabIndex = 2;
            this.groupBoxNamesIgonoreLists.TabStop = false;
            this.groupBoxNamesIgonoreLists.Text = "Names/ignore lists";
            // 
            // listViewNames
            // 
            this.listViewNames.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listViewNames.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderNames});
            this.listViewNames.FullRowSelect = true;
            this.listViewNames.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listViewNames.HideSelection = false;
            this.listViewNames.Location = new System.Drawing.Point(3, 21);
            this.listViewNames.Name = "listViewNames";
            this.listViewNames.Size = new System.Drawing.Size(148, 281);
            this.listViewNames.TabIndex = 27;
            this.listViewNames.UseCompatibleStateImageBehavior = false;
            this.listViewNames.View = System.Windows.Forms.View.Details;
            this.listViewNames.SelectedIndexChanged += new System.EventHandler(this.ListViewNamesSelectedIndexChanged);
            this.listViewNames.DoubleClick += new System.EventHandler(this.listViewNames_DoubleClick);
            this.listViewNames.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listViewNames_KeyDown);
            // 
            // columnHeaderNames
            // 
            this.columnHeaderNames.Width = 144;
            // 
            // buttonRemoveNameEtc
            // 
            this.buttonRemoveNameEtc.Location = new System.Drawing.Point(159, 16);
            this.buttonRemoveNameEtc.Name = "buttonRemoveNameEtc";
            this.buttonRemoveNameEtc.Size = new System.Drawing.Size(75, 23);
            this.buttonRemoveNameEtc.TabIndex = 22;
            this.buttonRemoveNameEtc.Text = "Remove";
            this.buttonRemoveNameEtc.UseVisualStyleBackColor = true;
            this.buttonRemoveNameEtc.Click += new System.EventHandler(this.ButtonRemoveNameEtcClick);
            // 
            // textBoxNameEtc
            // 
            this.textBoxNameEtc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBoxNameEtc.Location = new System.Drawing.Point(3, 309);
            this.textBoxNameEtc.Name = "textBoxNameEtc";
            this.textBoxNameEtc.Size = new System.Drawing.Size(151, 20);
            this.textBoxNameEtc.TabIndex = 24;
            this.textBoxNameEtc.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBoxNameEtcKeyDown);
            // 
            // buttonAddNames
            // 
            this.buttonAddNames.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonAddNames.Location = new System.Drawing.Point(157, 307);
            this.buttonAddNames.Name = "buttonAddNames";
            this.buttonAddNames.Size = new System.Drawing.Size(75, 23);
            this.buttonAddNames.TabIndex = 26;
            this.buttonAddNames.Text = "Add name";
            this.buttonAddNames.UseVisualStyleBackColor = true;
            this.buttonAddNames.Click += new System.EventHandler(this.ButtonAddNamesClick);
            // 
            // labelWordListLanguage
            // 
            this.labelWordListLanguage.AutoSize = true;
            this.labelWordListLanguage.Location = new System.Drawing.Point(6, 19);
            this.labelWordListLanguage.Name = "labelWordListLanguage";
            this.labelWordListLanguage.Size = new System.Drawing.Size(55, 13);
            this.labelWordListLanguage.TabIndex = 1;
            this.labelWordListLanguage.Text = "Language";
            // 
            // comboBoxWordListLanguage
            // 
            this.comboBoxWordListLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxWordListLanguage.FormattingEnabled = true;
            this.comboBoxWordListLanguage.Location = new System.Drawing.Point(67, 16);
            this.comboBoxWordListLanguage.Name = "comboBoxWordListLanguage";
            this.comboBoxWordListLanguage.Size = new System.Drawing.Size(155, 21);
            this.comboBoxWordListLanguage.TabIndex = 0;
            this.comboBoxWordListLanguage.SelectedIndexChanged += new System.EventHandler(this.ComboBoxWordListLanguageSelectedIndexChanged);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(765, 537);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 16;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(684, 537);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 15;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // labelStatus
            // 
            this.labelStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(22, 553);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(59, 13);
            this.labelStatus.TabIndex = 17;
            this.labelStatus.Text = "labelStatus";
            // 
            // WordLists
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(852, 572);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.groupBoxWordLists);
            this.KeyPreview = true;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(860, 600);
            this.Name = "WordLists";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "WordLists";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.WordLists_KeyDown);
            this.groupBoxWordLists.ResumeLayout(false);
            this.groupBoxWordLists.PerformLayout();
            this.groupBoxOcrFixList.ResumeLayout(false);
            this.groupBoxOcrFixList.PerformLayout();
            this.groupBoxUserWordList.ResumeLayout(false);
            this.groupBoxUserWordList.PerformLayout();
            this.groupBoxWordListLocation.ResumeLayout(false);
            this.groupBoxWordListLocation.PerformLayout();
            this.groupBoxNamesIgonoreLists.ResumeLayout(false);
            this.groupBoxNamesIgonoreLists.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxWordLists;
        private System.Windows.Forms.LinkLabel linkLabelOpenDictionaryFolder;
        private System.Windows.Forms.GroupBox groupBoxOcrFixList;
        private Nikse.SubtitleEdit.Controls.SETextBox textBoxOcrFixValue;
        private System.Windows.Forms.Button buttonRemoveOcrFix;
        private Nikse.SubtitleEdit.Controls.NikseListBox listBoxOcrFixList;
        private Nikse.SubtitleEdit.Controls.SETextBox textBoxOcrFixKey;
        private System.Windows.Forms.Button buttonAddOcrFix;
        private System.Windows.Forms.GroupBox groupBoxUserWordList;
        private System.Windows.Forms.Button buttonRemoveUserWord;
        private Nikse.SubtitleEdit.Controls.NikseListBox listBoxUserWordLists;
        private Nikse.SubtitleEdit.Controls.SETextBox textBoxUserWord;
        private System.Windows.Forms.Button buttonAddUserWord;
        private System.Windows.Forms.GroupBox groupBoxWordListLocation;
        private System.Windows.Forms.CheckBox checkBoxNamesOnline;
        private Nikse.SubtitleEdit.Controls.SETextBox textBoxNamesOnline;
        private System.Windows.Forms.GroupBox groupBoxNamesIgonoreLists;
        private System.Windows.Forms.ListView listViewNames;
        private System.Windows.Forms.ColumnHeader columnHeaderNames;
        private System.Windows.Forms.Button buttonRemoveNameEtc;
        private Nikse.SubtitleEdit.Controls.SETextBox textBoxNameEtc;
        private System.Windows.Forms.Button buttonAddNames;
        private System.Windows.Forms.Label labelWordListLanguage;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxWordListLanguage;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Label labelStatus;
    }
}