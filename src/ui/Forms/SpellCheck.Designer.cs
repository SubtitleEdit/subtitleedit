namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class SpellCheck
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;       

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.buttonAddToDictionary = new System.Windows.Forms.Button();
            this.buttonSkipOnce = new System.Windows.Forms.Button();
            this.comboBoxDictionaries = new System.Windows.Forms.ComboBox();
            this.labelLanguage = new System.Windows.Forms.Label();
            this.richTextBoxParagraph = new System.Windows.Forms.RichTextBox();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addXToNamesnoiseListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addXToUserDictionaryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.listBoxSuggestions = new System.Windows.Forms.ListBox();
            this.labelFullText = new System.Windows.Forms.Label();
            this.textBoxWord = new System.Windows.Forms.TextBox();
            this.buttonAbort = new System.Windows.Forms.Button();
            this.buttonSkipAll = new System.Windows.Forms.Button();
            this.buttonChange = new System.Windows.Forms.Button();
            this.buttonUseSuggestion = new System.Windows.Forms.Button();
            this.buttonChangeAll = new System.Windows.Forms.Button();
            this.buttonUseSuggestionAlways = new System.Windows.Forms.Button();
            this.buttonAddToNames = new System.Windows.Forms.Button();
            this.groupBoxWordNotFound = new System.Windows.Forms.GroupBox();
            this.buttonGoogleIt = new System.Windows.Forms.Button();
            this.buttonUndo = new System.Windows.Forms.Button();
            this.groupBoxSuggestions = new System.Windows.Forms.GroupBox();
            this.checkBoxAutoChangeNames = new System.Windows.Forms.CheckBox();
            this.buttonEditWholeText = new System.Windows.Forms.Button();
            this.groupBoxEditWholeText = new System.Windows.Forms.GroupBox();
            this.buttonSkipText = new System.Windows.Forms.Button();
            this.buttonChangeWholeText = new System.Windows.Forms.Button();
            this.textBoxWholeText = new System.Windows.Forms.TextBox();
            this.labelActionInfo = new System.Windows.Forms.Label();
            this.buttonSpellCheckDownload = new System.Windows.Forms.Button();
            this.contextMenuStrip1.SuspendLayout();
            this.groupBoxWordNotFound.SuspendLayout();
            this.groupBoxSuggestions.SuspendLayout();
            this.groupBoxEditWholeText.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonAddToDictionary
            // 
            this.buttonAddToDictionary.Location = new System.Drawing.Point(6, 134);
            this.buttonAddToDictionary.Name = "buttonAddToDictionary";
            this.buttonAddToDictionary.Size = new System.Drawing.Size(280, 23);
            this.buttonAddToDictionary.TabIndex = 6;
            this.buttonAddToDictionary.Text = "Add to user dictionary (not case sensitive)";
            this.buttonAddToDictionary.UseVisualStyleBackColor = true;
            this.buttonAddToDictionary.Click += new System.EventHandler(this.ButtonAddToDictionaryClick);
            this.buttonAddToDictionary.MouseEnter += new System.EventHandler(this.buttonAddToDictionary_MouseEnter);
            this.buttonAddToDictionary.MouseLeave += new System.EventHandler(this.buttonAddToDictionary_MouseLeave);
            // 
            // buttonSkipOnce
            // 
            this.buttonSkipOnce.Location = new System.Drawing.Point(6, 76);
            this.buttonSkipOnce.Name = "buttonSkipOnce";
            this.buttonSkipOnce.Size = new System.Drawing.Size(136, 23);
            this.buttonSkipOnce.TabIndex = 3;
            this.buttonSkipOnce.Text = "Skip &once";
            this.buttonSkipOnce.UseVisualStyleBackColor = true;
            this.buttonSkipOnce.Click += new System.EventHandler(this.ButtonSkipOnceClick);
            this.buttonSkipOnce.MouseEnter += new System.EventHandler(this.buttonSkipOnce_MouseEnter);
            this.buttonSkipOnce.MouseLeave += new System.EventHandler(this.buttonSkipOnce_MouseLeave);
            // 
            // comboBoxDictionaries
            // 
            this.comboBoxDictionaries.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDictionaries.FormattingEnabled = true;
            this.comboBoxDictionaries.Location = new System.Drawing.Point(393, 34);
            this.comboBoxDictionaries.Name = "comboBoxDictionaries";
            this.comboBoxDictionaries.Size = new System.Drawing.Size(245, 21);
            this.comboBoxDictionaries.TabIndex = 8;
            this.comboBoxDictionaries.SelectedIndexChanged += new System.EventHandler(this.ComboBoxDictionariesSelectedIndexChanged);
            // 
            // labelLanguage
            // 
            this.labelLanguage.AutoSize = true;
            this.labelLanguage.Location = new System.Drawing.Point(390, 14);
            this.labelLanguage.Name = "labelLanguage";
            this.labelLanguage.Size = new System.Drawing.Size(54, 13);
            this.labelLanguage.TabIndex = 7;
            this.labelLanguage.Text = "Language";
            // 
            // richTextBoxParagraph
            // 
            this.richTextBoxParagraph.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.richTextBoxParagraph.ContextMenuStrip = this.contextMenuStrip1;
            this.richTextBoxParagraph.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBoxParagraph.Location = new System.Drawing.Point(11, 33);
            this.richTextBoxParagraph.Name = "richTextBoxParagraph";
            this.richTextBoxParagraph.ReadOnly = true;
            this.richTextBoxParagraph.Size = new System.Drawing.Size(358, 92);
            this.richTextBoxParagraph.TabIndex = 5;
            this.richTextBoxParagraph.Text = "";
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addXToNamesnoiseListToolStripMenuItem,
            this.addXToUserDictionaryToolStripMenuItem,
            this.toolStripSeparator1,
            this.deleteToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(209, 76);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenuStrip1Opening);
            // 
            // addXToNamesnoiseListToolStripMenuItem
            // 
            this.addXToNamesnoiseListToolStripMenuItem.Name = "addXToNamesnoiseListToolStripMenuItem";
            this.addXToNamesnoiseListToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            this.addXToNamesnoiseListToolStripMenuItem.Text = "Add x to names/noise list";
            this.addXToNamesnoiseListToolStripMenuItem.Click += new System.EventHandler(this.AddXToNamesNoiseListToolStripMenuItemClick);
            // 
            // addXToUserDictionaryToolStripMenuItem
            // 
            this.addXToUserDictionaryToolStripMenuItem.Name = "addXToUserDictionaryToolStripMenuItem";
            this.addXToUserDictionaryToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            this.addXToUserDictionaryToolStripMenuItem.Text = "Add x to user dictionary";
            this.addXToUserDictionaryToolStripMenuItem.Click += new System.EventHandler(this.AddXToUserDictionaryToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(205, 6);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            this.deleteToolStripMenuItem.Text = "Delete...";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // listBoxSuggestions
            // 
            this.listBoxSuggestions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listBoxSuggestions.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listBoxSuggestions.FormattingEnabled = true;
            this.listBoxSuggestions.ItemHeight = 16;
            this.listBoxSuggestions.Location = new System.Drawing.Point(8, 47);
            this.listBoxSuggestions.Name = "listBoxSuggestions";
            this.listBoxSuggestions.Size = new System.Drawing.Size(272, 180);
            this.listBoxSuggestions.TabIndex = 2;
            this.listBoxSuggestions.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.ListBoxSuggestionsMouseDoubleClick);
            // 
            // labelFullText
            // 
            this.labelFullText.AutoSize = true;
            this.labelFullText.Location = new System.Drawing.Point(8, 14);
            this.labelFullText.Name = "labelFullText";
            this.labelFullText.Size = new System.Drawing.Size(46, 13);
            this.labelFullText.TabIndex = 4;
            this.labelFullText.Text = "Full text";
            // 
            // textBoxWord
            // 
            this.textBoxWord.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxWord.Location = new System.Drawing.Point(6, 20);
            this.textBoxWord.Name = "textBoxWord";
            this.textBoxWord.Size = new System.Drawing.Size(280, 21);
            this.textBoxWord.TabIndex = 0;
            this.textBoxWord.TextChanged += new System.EventHandler(this.textBoxWord_TextChanged);
            // 
            // buttonAbort
            // 
            this.buttonAbort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAbort.Location = new System.Drawing.Point(599, 392);
            this.buttonAbort.Name = "buttonAbort";
            this.buttonAbort.Size = new System.Drawing.Size(85, 23);
            this.buttonAbort.TabIndex = 3;
            this.buttonAbort.Text = "Abort";
            this.buttonAbort.UseVisualStyleBackColor = true;
            this.buttonAbort.Click += new System.EventHandler(this.ButtonAbortClick);
            // 
            // buttonSkipAll
            // 
            this.buttonSkipAll.Location = new System.Drawing.Point(147, 76);
            this.buttonSkipAll.Name = "buttonSkipAll";
            this.buttonSkipAll.Size = new System.Drawing.Size(138, 23);
            this.buttonSkipAll.TabIndex = 4;
            this.buttonSkipAll.Text = "&Skip all";
            this.buttonSkipAll.UseVisualStyleBackColor = true;
            this.buttonSkipAll.Click += new System.EventHandler(this.ButtonSkipAllClick);
            this.buttonSkipAll.MouseEnter += new System.EventHandler(this.buttonSkipAll_MouseEnter);
            this.buttonSkipAll.MouseLeave += new System.EventHandler(this.buttonSkipAll_MouseLeave);
            // 
            // buttonChange
            // 
            this.buttonChange.Location = new System.Drawing.Point(6, 47);
            this.buttonChange.Name = "buttonChange";
            this.buttonChange.Size = new System.Drawing.Size(136, 23);
            this.buttonChange.TabIndex = 1;
            this.buttonChange.Text = "Change";
            this.buttonChange.UseVisualStyleBackColor = true;
            this.buttonChange.Click += new System.EventHandler(this.ButtonChangeClick);
            this.buttonChange.MouseEnter += new System.EventHandler(this.buttonChange_MouseEnter);
            this.buttonChange.MouseLeave += new System.EventHandler(this.buttonChange_MouseLeave);
            // 
            // buttonUseSuggestion
            // 
            this.buttonUseSuggestion.Location = new System.Drawing.Point(68, 17);
            this.buttonUseSuggestion.Name = "buttonUseSuggestion";
            this.buttonUseSuggestion.Size = new System.Drawing.Size(90, 23);
            this.buttonUseSuggestion.TabIndex = 0;
            this.buttonUseSuggestion.Text = "Use";
            this.buttonUseSuggestion.UseVisualStyleBackColor = true;
            this.buttonUseSuggestion.Click += new System.EventHandler(this.ButtonUseSuggestionClick);
            // 
            // buttonChangeAll
            // 
            this.buttonChangeAll.Location = new System.Drawing.Point(148, 47);
            this.buttonChangeAll.Name = "buttonChangeAll";
            this.buttonChangeAll.Size = new System.Drawing.Size(138, 23);
            this.buttonChangeAll.TabIndex = 2;
            this.buttonChangeAll.Text = "Change all";
            this.buttonChangeAll.UseVisualStyleBackColor = true;
            this.buttonChangeAll.Click += new System.EventHandler(this.ButtonChangeAllClick);
            this.buttonChangeAll.MouseEnter += new System.EventHandler(this.buttonChangeAll_MouseEnter);
            this.buttonChangeAll.MouseLeave += new System.EventHandler(this.buttonChangeAll_MouseLeave);
            // 
            // buttonUseSuggestionAlways
            // 
            this.buttonUseSuggestionAlways.Location = new System.Drawing.Point(164, 17);
            this.buttonUseSuggestionAlways.Name = "buttonUseSuggestionAlways";
            this.buttonUseSuggestionAlways.Size = new System.Drawing.Size(115, 23);
            this.buttonUseSuggestionAlways.TabIndex = 1;
            this.buttonUseSuggestionAlways.Text = "Use always";
            this.buttonUseSuggestionAlways.UseVisualStyleBackColor = true;
            this.buttonUseSuggestionAlways.Click += new System.EventHandler(this.ButtonUseSuggestionAlwaysClick);
            // 
            // buttonAddToNames
            // 
            this.buttonAddToNames.Location = new System.Drawing.Point(5, 105);
            this.buttonAddToNames.Name = "buttonAddToNames";
            this.buttonAddToNames.Size = new System.Drawing.Size(280, 23);
            this.buttonAddToNames.TabIndex = 5;
            this.buttonAddToNames.Text = "Add to names/noise list (case sensitive)";
            this.buttonAddToNames.UseVisualStyleBackColor = true;
            this.buttonAddToNames.Click += new System.EventHandler(this.ButtonAddToNamesClick);
            this.buttonAddToNames.MouseEnter += new System.EventHandler(this.buttonAddToNames_MouseEnter);
            this.buttonAddToNames.MouseLeave += new System.EventHandler(this.buttonAddToNames_MouseLeave);
            // 
            // groupBoxWordNotFound
            // 
            this.groupBoxWordNotFound.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBoxWordNotFound.Controls.Add(this.buttonGoogleIt);
            this.groupBoxWordNotFound.Controls.Add(this.buttonUndo);
            this.groupBoxWordNotFound.Controls.Add(this.buttonAddToNames);
            this.groupBoxWordNotFound.Controls.Add(this.buttonAddToDictionary);
            this.groupBoxWordNotFound.Controls.Add(this.buttonSkipOnce);
            this.groupBoxWordNotFound.Controls.Add(this.buttonChangeAll);
            this.groupBoxWordNotFound.Controls.Add(this.textBoxWord);
            this.groupBoxWordNotFound.Controls.Add(this.buttonSkipAll);
            this.groupBoxWordNotFound.Controls.Add(this.buttonChange);
            this.groupBoxWordNotFound.Location = new System.Drawing.Point(11, 165);
            this.groupBoxWordNotFound.Name = "groupBoxWordNotFound";
            this.groupBoxWordNotFound.Size = new System.Drawing.Size(358, 218);
            this.groupBoxWordNotFound.TabIndex = 0;
            this.groupBoxWordNotFound.TabStop = false;
            this.groupBoxWordNotFound.Text = "Word not found";
            // 
            // buttonGoogleIt
            // 
            this.buttonGoogleIt.Location = new System.Drawing.Point(6, 163);
            this.buttonGoogleIt.Name = "buttonGoogleIt";
            this.buttonGoogleIt.Size = new System.Drawing.Size(280, 23);
            this.buttonGoogleIt.TabIndex = 7;
            this.buttonGoogleIt.Text = "&Google it";
            this.buttonGoogleIt.UseVisualStyleBackColor = true;
            this.buttonGoogleIt.Click += new System.EventHandler(this.buttonGoogleIt_Click);
            // 
            // buttonUndo
            // 
            this.buttonUndo.Location = new System.Drawing.Point(6, 192);
            this.buttonUndo.Name = "buttonUndo";
            this.buttonUndo.Size = new System.Drawing.Size(280, 23);
            this.buttonUndo.TabIndex = 8;
            this.buttonUndo.Text = "Undo: skip all \'A\'";
            this.buttonUndo.UseVisualStyleBackColor = true;
            this.buttonUndo.Visible = false;
            this.buttonUndo.Click += new System.EventHandler(this.buttonUndo_Click);
            // 
            // groupBoxSuggestions
            // 
            this.groupBoxSuggestions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxSuggestions.Controls.Add(this.buttonUseSuggestion);
            this.groupBoxSuggestions.Controls.Add(this.buttonUseSuggestionAlways);
            this.groupBoxSuggestions.Controls.Add(this.checkBoxAutoChangeNames);
            this.groupBoxSuggestions.Controls.Add(this.listBoxSuggestions);
            this.groupBoxSuggestions.Location = new System.Drawing.Point(393, 73);
            this.groupBoxSuggestions.Name = "groupBoxSuggestions";
            this.groupBoxSuggestions.Size = new System.Drawing.Size(291, 271);
            this.groupBoxSuggestions.TabIndex = 1;
            this.groupBoxSuggestions.TabStop = false;
            this.groupBoxSuggestions.Text = "Suggestions";
            // 
            // checkBoxAutoChangeNames
            // 
            this.checkBoxAutoChangeNames.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxAutoChangeNames.AutoSize = true;
            this.checkBoxAutoChangeNames.Location = new System.Drawing.Point(8, 247);
            this.checkBoxAutoChangeNames.Name = "checkBoxAutoChangeNames";
            this.checkBoxAutoChangeNames.Size = new System.Drawing.Size(221, 17);
            this.checkBoxAutoChangeNames.TabIndex = 3;
            this.checkBoxAutoChangeNames.Text = "Auto fix names where only casing differs";
            this.checkBoxAutoChangeNames.UseVisualStyleBackColor = true;
            this.checkBoxAutoChangeNames.CheckedChanged += new System.EventHandler(this.CheckBoxAutoChangeNamesCheckedChanged);
            // 
            // buttonEditWholeText
            // 
            this.buttonEditWholeText.Location = new System.Drawing.Point(241, 131);
            this.buttonEditWholeText.Name = "buttonEditWholeText";
            this.buttonEditWholeText.Size = new System.Drawing.Size(128, 23);
            this.buttonEditWholeText.TabIndex = 6;
            this.buttonEditWholeText.Text = "Edit whole text";
            this.buttonEditWholeText.UseVisualStyleBackColor = true;
            this.buttonEditWholeText.Click += new System.EventHandler(this.ButtonEditWholeTextClick);
            // 
            // groupBoxEditWholeText
            // 
            this.groupBoxEditWholeText.Controls.Add(this.buttonSkipText);
            this.groupBoxEditWholeText.Controls.Add(this.buttonChangeWholeText);
            this.groupBoxEditWholeText.Controls.Add(this.textBoxWholeText);
            this.groupBoxEditWholeText.Location = new System.Drawing.Point(11, 166);
            this.groupBoxEditWholeText.Name = "groupBoxEditWholeText";
            this.groupBoxEditWholeText.Size = new System.Drawing.Size(358, 223);
            this.groupBoxEditWholeText.TabIndex = 40;
            this.groupBoxEditWholeText.TabStop = false;
            this.groupBoxEditWholeText.Text = "Edit whole text";
            // 
            // buttonSkipText
            // 
            this.buttonSkipText.Location = new System.Drawing.Point(147, 191);
            this.buttonSkipText.Name = "buttonSkipText";
            this.buttonSkipText.Size = new System.Drawing.Size(135, 23);
            this.buttonSkipText.TabIndex = 35;
            this.buttonSkipText.Text = "Skip once";
            this.buttonSkipText.UseVisualStyleBackColor = true;
            this.buttonSkipText.Click += new System.EventHandler(this.ButtonSkipTextClick);
            // 
            // buttonChangeWholeText
            // 
            this.buttonChangeWholeText.Location = new System.Drawing.Point(6, 191);
            this.buttonChangeWholeText.Name = "buttonChangeWholeText";
            this.buttonChangeWholeText.Size = new System.Drawing.Size(135, 23);
            this.buttonChangeWholeText.TabIndex = 0;
            this.buttonChangeWholeText.Text = "Change";
            this.buttonChangeWholeText.UseVisualStyleBackColor = true;
            this.buttonChangeWholeText.Click += new System.EventHandler(this.ButtonChangeWholeTextClick);
            // 
            // textBoxWholeText
            // 
            this.textBoxWholeText.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxWholeText.Location = new System.Drawing.Point(6, 19);
            this.textBoxWholeText.Multiline = true;
            this.textBoxWholeText.Name = "textBoxWholeText";
            this.textBoxWholeText.Size = new System.Drawing.Size(341, 166);
            this.textBoxWholeText.TabIndex = 31;
            // 
            // labelActionInfo
            // 
            this.labelActionInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelActionInfo.AutoSize = true;
            this.labelActionInfo.Location = new System.Drawing.Point(12, 405);
            this.labelActionInfo.Name = "labelActionInfo";
            this.labelActionInfo.Size = new System.Drawing.Size(79, 13);
            this.labelActionInfo.TabIndex = 2;
            this.labelActionInfo.Text = "labelActionInfo";
            // 
            // buttonSpellCheckDownload
            // 
            this.buttonSpellCheckDownload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSpellCheckDownload.Location = new System.Drawing.Point(644, 32);
            this.buttonSpellCheckDownload.Name = "buttonSpellCheckDownload";
            this.buttonSpellCheckDownload.Size = new System.Drawing.Size(28, 23);
            this.buttonSpellCheckDownload.TabIndex = 9;
            this.buttonSpellCheckDownload.Text = "...";
            this.buttonSpellCheckDownload.UseVisualStyleBackColor = true;
            this.buttonSpellCheckDownload.Click += new System.EventHandler(this.buttonSpellCheckDownload_Click);
            // 
            // SpellCheck
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(696, 427);
            this.Controls.Add(this.buttonSpellCheckDownload);
            this.Controls.Add(this.labelActionInfo);
            this.Controls.Add(this.comboBoxDictionaries);
            this.Controls.Add(this.buttonEditWholeText);
            this.Controls.Add(this.groupBoxSuggestions);
            this.Controls.Add(this.labelFullText);
            this.Controls.Add(this.buttonAbort);
            this.Controls.Add(this.labelLanguage);
            this.Controls.Add(this.richTextBoxParagraph);
            this.Controls.Add(this.groupBoxEditWholeText);
            this.Controls.Add(this.groupBoxWordNotFound);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SpellCheck";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Spell check";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SpellCheck_FormClosing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormSpellCheck_KeyDown);
            this.contextMenuStrip1.ResumeLayout(false);
            this.groupBoxWordNotFound.ResumeLayout(false);
            this.groupBoxWordNotFound.PerformLayout();
            this.groupBoxSuggestions.ResumeLayout(false);
            this.groupBoxSuggestions.PerformLayout();
            this.groupBoxEditWholeText.ResumeLayout(false);
            this.groupBoxEditWholeText.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonAddToDictionary;
        private System.Windows.Forms.Button buttonSkipOnce;
        private System.Windows.Forms.ComboBox comboBoxDictionaries;
        private System.Windows.Forms.Label labelLanguage;
        private System.Windows.Forms.RichTextBox richTextBoxParagraph;
        private System.Windows.Forms.ListBox listBoxSuggestions;
        private System.Windows.Forms.Label labelFullText;
        private System.Windows.Forms.TextBox textBoxWord;
        private System.Windows.Forms.Button buttonAbort;
        private System.Windows.Forms.Button buttonSkipAll;
        private System.Windows.Forms.Button buttonChange;
        private System.Windows.Forms.Button buttonUseSuggestion;
        private System.Windows.Forms.Button buttonChangeAll;
        private System.Windows.Forms.Button buttonUseSuggestionAlways;
        private System.Windows.Forms.Button buttonAddToNames;
        private System.Windows.Forms.GroupBox groupBoxWordNotFound;
        private System.Windows.Forms.GroupBox groupBoxSuggestions;
        private System.Windows.Forms.Button buttonEditWholeText;
        private System.Windows.Forms.GroupBox groupBoxEditWholeText;
        private System.Windows.Forms.Button buttonSkipText;
        private System.Windows.Forms.Button buttonChangeWholeText;
        private System.Windows.Forms.TextBox textBoxWholeText;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem addXToNamesnoiseListToolStripMenuItem;
        private System.Windows.Forms.CheckBox checkBoxAutoChangeNames;
        private System.Windows.Forms.Label labelActionInfo;
        private System.Windows.Forms.Button buttonSpellCheckDownload;
        private System.Windows.Forms.Button buttonUndo;
        private System.Windows.Forms.Button buttonGoogleIt;
        private System.Windows.Forms.ToolStripMenuItem addXToUserDictionaryToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
    }
}