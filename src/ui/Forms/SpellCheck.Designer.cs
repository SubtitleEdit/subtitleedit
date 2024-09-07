﻿namespace Nikse.SubtitleEdit.Forms
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
            this.comboBoxDictionaries = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelLanguage = new System.Windows.Forms.Label();
            this.richTextBoxParagraph = new System.Windows.Forms.RichTextBox();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addXToNamesnoiseListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addXToUserDictionaryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bookmarkCommentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openImagedBasedSourceFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.useLargerFontForThisWindowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.listBoxSuggestions = new Nikse.SubtitleEdit.Controls.NikseListBox();
            this.labelFullText = new System.Windows.Forms.Label();
            this.textBoxWord = new Nikse.SubtitleEdit.Controls.NikseTextBox();
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
            this.textBoxWholeText = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.labelActionInfo = new System.Windows.Forms.Label();
            this.buttonSpellCheckDownload = new System.Windows.Forms.Button();
            this.pictureBoxBdSup = new System.Windows.Forms.PictureBox();
            this.pictureBoxBookmark = new System.Windows.Forms.PictureBox();
            this.panelBookmark = new System.Windows.Forms.Panel();
            this.labelBookmark = new System.Windows.Forms.Label();
            this.contextMenuStripWindow = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.useLargerFontForThisWindowToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStripSearchEngine = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.contextMenuStrip1.SuspendLayout();
            this.groupBoxWordNotFound.SuspendLayout();
            this.groupBoxSuggestions.SuspendLayout();
            this.groupBoxEditWholeText.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxBdSup)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxBookmark)).BeginInit();
            this.panelBookmark.SuspendLayout();
            this.contextMenuStripWindow.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonAddToDictionary
            // 
            this.buttonAddToDictionary.Location = new System.Drawing.Point(6, 134);
            this.buttonAddToDictionary.Name = "buttonAddToDictionary";
            this.buttonAddToDictionary.Size = new System.Drawing.Size(374, 23);
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
            this.buttonSkipOnce.Size = new System.Drawing.Size(182, 23);
            this.buttonSkipOnce.TabIndex = 3;
            this.buttonSkipOnce.Text = "Skip &once";
            this.buttonSkipOnce.UseVisualStyleBackColor = true;
            this.buttonSkipOnce.Click += new System.EventHandler(this.ButtonSkipOnceClick);
            this.buttonSkipOnce.MouseEnter += new System.EventHandler(this.buttonSkipOnce_MouseEnter);
            this.buttonSkipOnce.MouseLeave += new System.EventHandler(this.buttonSkipOnce_MouseLeave);
            // 
            // comboBoxDictionaries
            // 
            this.comboBoxDictionaries.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxDictionaries.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDictionaries.FormattingEnabled = true;
            this.comboBoxDictionaries.Location = new System.Drawing.Point(412, 32);
            this.comboBoxDictionaries.Name = "comboBoxDictionaries";
            this.comboBoxDictionaries.Size = new System.Drawing.Size(249, 21);
            this.comboBoxDictionaries.TabIndex = 8;
            this.comboBoxDictionaries.SelectedIndexChanged += new System.EventHandler(this.ComboBoxDictionariesSelectedIndexChanged);
            // 
            // labelLanguage
            // 
            this.labelLanguage.AutoSize = true;
            this.labelLanguage.Location = new System.Drawing.Point(409, 14);
            this.labelLanguage.Name = "labelLanguage";
            this.labelLanguage.Size = new System.Drawing.Size(54, 13);
            this.labelLanguage.TabIndex = 7;
            this.labelLanguage.Text = "Language";
            // 
            // richTextBoxParagraph
            // 
            this.richTextBoxParagraph.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.richTextBoxParagraph.ContextMenuStrip = this.contextMenuStrip1;
            this.richTextBoxParagraph.Location = new System.Drawing.Point(11, 33);
            this.richTextBoxParagraph.Name = "richTextBoxParagraph";
            this.richTextBoxParagraph.ReadOnly = true;
            this.richTextBoxParagraph.Size = new System.Drawing.Size(386, 92);
            this.richTextBoxParagraph.TabIndex = 5;
            this.richTextBoxParagraph.Text = "";
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addXToNamesnoiseListToolStripMenuItem,
            this.addXToUserDictionaryToolStripMenuItem,
            this.toolStripSeparator1,
            this.deleteToolStripMenuItem,
            this.bookmarkCommentToolStripMenuItem,
            this.openImagedBasedSourceFileToolStripMenuItem,
            this.toolStripSeparator2,
            this.useLargerFontForThisWindowToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(247, 148);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenuStrip1Opening);
            // 
            // addXToNamesnoiseListToolStripMenuItem
            // 
            this.addXToNamesnoiseListToolStripMenuItem.Name = "addXToNamesnoiseListToolStripMenuItem";
            this.addXToNamesnoiseListToolStripMenuItem.Size = new System.Drawing.Size(246, 22);
            this.addXToNamesnoiseListToolStripMenuItem.Text = "Add x to names/noise list";
            this.addXToNamesnoiseListToolStripMenuItem.Click += new System.EventHandler(this.AddXToNamesNoiseListToolStripMenuItemClick);
            // 
            // addXToUserDictionaryToolStripMenuItem
            // 
            this.addXToUserDictionaryToolStripMenuItem.Name = "addXToUserDictionaryToolStripMenuItem";
            this.addXToUserDictionaryToolStripMenuItem.Size = new System.Drawing.Size(246, 22);
            this.addXToUserDictionaryToolStripMenuItem.Text = "Add x to user dictionary";
            this.addXToUserDictionaryToolStripMenuItem.Click += new System.EventHandler(this.AddXToUserDictionaryToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(243, 6);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(246, 22);
            this.deleteToolStripMenuItem.Text = "Delete...";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // bookmarkCommentToolStripMenuItem
            // 
            this.bookmarkCommentToolStripMenuItem.Name = "bookmarkCommentToolStripMenuItem";
            this.bookmarkCommentToolStripMenuItem.Size = new System.Drawing.Size(246, 22);
            this.bookmarkCommentToolStripMenuItem.Text = "Bookmark text...";
            this.bookmarkCommentToolStripMenuItem.Click += new System.EventHandler(this.bookmarkCommentToolStripMenuItem_Click);
            // 
            // openImagedBasedSourceFileToolStripMenuItem
            // 
            this.openImagedBasedSourceFileToolStripMenuItem.Name = "openImagedBasedSourceFileToolStripMenuItem";
            this.openImagedBasedSourceFileToolStripMenuItem.Size = new System.Drawing.Size(246, 22);
            this.openImagedBasedSourceFileToolStripMenuItem.Text = "Open imaged based source file...";
            this.openImagedBasedSourceFileToolStripMenuItem.Click += new System.EventHandler(this.openImagedBasedSourceFileToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(243, 6);
            // 
            // useLargerFontForThisWindowToolStripMenuItem
            // 
            this.useLargerFontForThisWindowToolStripMenuItem.Name = "useLargerFontForThisWindowToolStripMenuItem";
            this.useLargerFontForThisWindowToolStripMenuItem.Size = new System.Drawing.Size(246, 22);
            this.useLargerFontForThisWindowToolStripMenuItem.Text = "Use larger font for this window";
            this.useLargerFontForThisWindowToolStripMenuItem.Click += new System.EventHandler(this.useLargerFontForThisWindowToolStripMenuItem_Click);
            // 
            // listBoxSuggestions
            // 
            this.listBoxSuggestions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listBoxSuggestions.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listBoxSuggestions.FormattingEnabled = true;
            this.listBoxSuggestions.ItemHeight = 16;
            this.listBoxSuggestions.Location = new System.Drawing.Point(6, 46);
            this.listBoxSuggestions.Name = "listBoxSuggestions";
            this.listBoxSuggestions.Size = new System.Drawing.Size(277, 228);
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
            this.textBoxWord.Location = new System.Drawing.Point(6, 20);
            this.textBoxWord.Name = "textBoxWord";
            this.textBoxWord.Size = new System.Drawing.Size(374, 21);
            this.textBoxWord.TabIndex = 0;
            this.textBoxWord.TextChanged += new System.EventHandler(this.textBoxWord_TextChanged);
            // 
            // buttonAbort
            // 
            this.buttonAbort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAbort.Location = new System.Drawing.Point(616, 392);
            this.buttonAbort.Name = "buttonAbort";
            this.buttonAbort.Size = new System.Drawing.Size(85, 23);
            this.buttonAbort.TabIndex = 3;
            this.buttonAbort.Text = "Abort";
            this.buttonAbort.UseVisualStyleBackColor = true;
            this.buttonAbort.Click += new System.EventHandler(this.ButtonAbortClick);
            // 
            // buttonSkipAll
            // 
            this.buttonSkipAll.Location = new System.Drawing.Point(198, 76);
            this.buttonSkipAll.Name = "buttonSkipAll";
            this.buttonSkipAll.Size = new System.Drawing.Size(182, 23);
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
            this.buttonChange.Size = new System.Drawing.Size(182, 23);
            this.buttonChange.TabIndex = 1;
            this.buttonChange.Text = "Change";
            this.buttonChange.UseVisualStyleBackColor = true;
            this.buttonChange.Click += new System.EventHandler(this.ButtonChangeClick);
            this.buttonChange.MouseEnter += new System.EventHandler(this.buttonChange_MouseEnter);
            this.buttonChange.MouseLeave += new System.EventHandler(this.buttonChange_MouseLeave);
            // 
            // buttonUseSuggestion
            // 
            this.buttonUseSuggestion.Location = new System.Drawing.Point(6, 17);
            this.buttonUseSuggestion.Name = "buttonUseSuggestion";
            this.buttonUseSuggestion.Size = new System.Drawing.Size(132, 23);
            this.buttonUseSuggestion.TabIndex = 0;
            this.buttonUseSuggestion.Text = "Use";
            this.buttonUseSuggestion.UseVisualStyleBackColor = true;
            this.buttonUseSuggestion.Click += new System.EventHandler(this.ButtonUseSuggestionClick);
            // 
            // buttonChangeAll
            // 
            this.buttonChangeAll.Location = new System.Drawing.Point(198, 47);
            this.buttonChangeAll.Name = "buttonChangeAll";
            this.buttonChangeAll.Size = new System.Drawing.Size(182, 23);
            this.buttonChangeAll.TabIndex = 2;
            this.buttonChangeAll.Text = "Change all";
            this.buttonChangeAll.UseVisualStyleBackColor = true;
            this.buttonChangeAll.Click += new System.EventHandler(this.ButtonChangeAllClick);
            this.buttonChangeAll.MouseEnter += new System.EventHandler(this.buttonChangeAll_MouseEnter);
            this.buttonChangeAll.MouseLeave += new System.EventHandler(this.buttonChangeAll_MouseLeave);
            // 
            // buttonUseSuggestionAlways
            // 
            this.buttonUseSuggestionAlways.Location = new System.Drawing.Point(151, 17);
            this.buttonUseSuggestionAlways.Name = "buttonUseSuggestionAlways";
            this.buttonUseSuggestionAlways.Size = new System.Drawing.Size(132, 23);
            this.buttonUseSuggestionAlways.TabIndex = 1;
            this.buttonUseSuggestionAlways.Text = "Use always";
            this.buttonUseSuggestionAlways.UseVisualStyleBackColor = true;
            this.buttonUseSuggestionAlways.Click += new System.EventHandler(this.ButtonUseSuggestionAlwaysClick);
            // 
            // buttonAddToNames
            // 
            this.buttonAddToNames.Location = new System.Drawing.Point(6, 105);
            this.buttonAddToNames.Name = "buttonAddToNames";
            this.buttonAddToNames.Size = new System.Drawing.Size(374, 23);
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
            this.groupBoxWordNotFound.Size = new System.Drawing.Size(386, 222);
            this.groupBoxWordNotFound.TabIndex = 0;
            this.groupBoxWordNotFound.TabStop = false;
            this.groupBoxWordNotFound.Text = "Word not found";
            // 
            // buttonGoogleIt
            // 
            this.buttonGoogleIt.ContextMenuStrip = this.contextMenuStripSearchEngine;
            this.buttonGoogleIt.Location = new System.Drawing.Point(6, 163);
            this.buttonGoogleIt.Name = "buttonGoogleIt";
            this.buttonGoogleIt.Size = new System.Drawing.Size(374, 23);
            this.buttonGoogleIt.TabIndex = 7;
            this.buttonGoogleIt.Text = "&Google it";
            this.buttonGoogleIt.UseVisualStyleBackColor = true;
            this.buttonGoogleIt.Click += new System.EventHandler(this.buttonGoogleIt_Click);
            // 
            // buttonUndo
            // 
            this.buttonUndo.Location = new System.Drawing.Point(6, 192);
            this.buttonUndo.Name = "buttonUndo";
            this.buttonUndo.Size = new System.Drawing.Size(374, 23);
            this.buttonUndo.TabIndex = 8;
            this.buttonUndo.Text = "Undo: skip all \'A\'";
            this.buttonUndo.UseVisualStyleBackColor = true;
            this.buttonUndo.Visible = false;
            this.buttonUndo.Click += new System.EventHandler(this.buttonUndo_Click);
            // 
            // groupBoxSuggestions
            // 
            this.groupBoxSuggestions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxSuggestions.Controls.Add(this.buttonUseSuggestion);
            this.groupBoxSuggestions.Controls.Add(this.buttonUseSuggestionAlways);
            this.groupBoxSuggestions.Controls.Add(this.checkBoxAutoChangeNames);
            this.groupBoxSuggestions.Controls.Add(this.listBoxSuggestions);
            this.groupBoxSuggestions.Location = new System.Drawing.Point(412, 60);
            this.groupBoxSuggestions.Name = "groupBoxSuggestions";
            this.groupBoxSuggestions.Size = new System.Drawing.Size(289, 327);
            this.groupBoxSuggestions.TabIndex = 1;
            this.groupBoxSuggestions.TabStop = false;
            this.groupBoxSuggestions.Text = "Suggestions";
            // 
            // checkBoxAutoChangeNames
            // 
            this.checkBoxAutoChangeNames.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxAutoChangeNames.AutoSize = true;
            this.checkBoxAutoChangeNames.Location = new System.Drawing.Point(20, 297);
            this.checkBoxAutoChangeNames.Name = "checkBoxAutoChangeNames";
            this.checkBoxAutoChangeNames.Size = new System.Drawing.Size(221, 17);
            this.checkBoxAutoChangeNames.TabIndex = 3;
            this.checkBoxAutoChangeNames.Text = "Auto fix names where only casing differs";
            this.checkBoxAutoChangeNames.UseVisualStyleBackColor = true;
            this.checkBoxAutoChangeNames.CheckedChanged += new System.EventHandler(this.CheckBoxAutoChangeNamesCheckedChanged);
            // 
            // buttonEditWholeText
            // 
            this.buttonEditWholeText.Location = new System.Drawing.Point(269, 131);
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
            this.groupBoxEditWholeText.Size = new System.Drawing.Size(386, 221);
            this.groupBoxEditWholeText.TabIndex = 40;
            this.groupBoxEditWholeText.TabStop = false;
            this.groupBoxEditWholeText.Text = "Edit whole text";
            // 
            // buttonSkipText
            // 
            this.buttonSkipText.Location = new System.Drawing.Point(198, 191);
            this.buttonSkipText.Name = "buttonSkipText";
            this.buttonSkipText.Size = new System.Drawing.Size(182, 23);
            this.buttonSkipText.TabIndex = 35;
            this.buttonSkipText.Text = "Skip once";
            this.buttonSkipText.UseVisualStyleBackColor = true;
            this.buttonSkipText.Click += new System.EventHandler(this.ButtonSkipTextClick);
            // 
            // buttonChangeWholeText
            // 
            this.buttonChangeWholeText.Location = new System.Drawing.Point(6, 191);
            this.buttonChangeWholeText.Name = "buttonChangeWholeText";
            this.buttonChangeWholeText.Size = new System.Drawing.Size(182, 23);
            this.buttonChangeWholeText.TabIndex = 0;
            this.buttonChangeWholeText.Text = "Change";
            this.buttonChangeWholeText.UseVisualStyleBackColor = true;
            this.buttonChangeWholeText.Click += new System.EventHandler(this.ButtonChangeWholeTextClick);
            // 
            // textBoxWholeText
            // 
            this.textBoxWholeText.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxWholeText.Location = new System.Drawing.Point(6, 19);
            this.textBoxWholeText.Multiline = true;
            this.textBoxWholeText.Name = "textBoxWholeText";
            this.textBoxWholeText.Size = new System.Drawing.Size(374, 166);
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
            this.buttonSpellCheckDownload.Location = new System.Drawing.Point(667, 31);
            this.buttonSpellCheckDownload.Name = "buttonSpellCheckDownload";
            this.buttonSpellCheckDownload.Size = new System.Drawing.Size(28, 23);
            this.buttonSpellCheckDownload.TabIndex = 9;
            this.buttonSpellCheckDownload.Text = "...";
            this.buttonSpellCheckDownload.UseVisualStyleBackColor = true;
            this.buttonSpellCheckDownload.Click += new System.EventHandler(this.buttonSpellCheckDownload_Click);
            // 
            // pictureBoxBdSup
            // 
            this.pictureBoxBdSup.Location = new System.Drawing.Point(412, 60);
            this.pictureBoxBdSup.Name = "pictureBoxBdSup";
            this.pictureBoxBdSup.Size = new System.Drawing.Size(283, 119);
            this.pictureBoxBdSup.TabIndex = 41;
            this.pictureBoxBdSup.TabStop = false;
            // 
            // pictureBoxBookmark
            // 
            this.pictureBoxBookmark.Image = global::Nikse.SubtitleEdit.Properties.Resources.bookmark22transparent;
            this.pictureBoxBookmark.Location = new System.Drawing.Point(11, 131);
            this.pictureBoxBookmark.Name = "pictureBoxBookmark";
            this.pictureBoxBookmark.Size = new System.Drawing.Size(22, 22);
            this.pictureBoxBookmark.TabIndex = 42;
            this.pictureBoxBookmark.TabStop = false;
            this.pictureBoxBookmark.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pictureBoxBookmark_MouseClick);
            this.pictureBoxBookmark.MouseEnter += new System.EventHandler(this.pictureBoxBookmark_MouseEnter);
            // 
            // panelBookmark
            // 
            this.panelBookmark.BackColor = System.Drawing.Color.LemonChiffon;
            this.panelBookmark.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelBookmark.Controls.Add(this.labelBookmark);
            this.panelBookmark.Location = new System.Drawing.Point(39, 130);
            this.panelBookmark.Name = "panelBookmark";
            this.panelBookmark.Size = new System.Drawing.Size(224, 25);
            this.panelBookmark.TabIndex = 43;
            this.panelBookmark.Visible = false;
            // 
            // labelBookmark
            // 
            this.labelBookmark.AutoSize = true;
            this.labelBookmark.Location = new System.Drawing.Point(4, 4);
            this.labelBookmark.Name = "labelBookmark";
            this.labelBookmark.Size = new System.Drawing.Size(75, 13);
            this.labelBookmark.TabIndex = 0;
            this.labelBookmark.Text = "labelBookmark";
            // 
            // contextMenuStripWindow
            // 
            this.contextMenuStripWindow.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.useLargerFontForThisWindowToolStripMenuItem1});
            this.contextMenuStripWindow.Name = "contextMenuStrip2";
            this.contextMenuStripWindow.Size = new System.Drawing.Size(237, 26);
            // 
            // useLargerFontForThisWindowToolStripMenuItem1
            // 
            this.useLargerFontForThisWindowToolStripMenuItem1.Name = "useLargerFontForThisWindowToolStripMenuItem1";
            this.useLargerFontForThisWindowToolStripMenuItem1.Size = new System.Drawing.Size(236, 22);
            this.useLargerFontForThisWindowToolStripMenuItem1.Text = "Use larger font for this window";
            this.useLargerFontForThisWindowToolStripMenuItem1.Click += new System.EventHandler(this.useLargerFontForThisWindowToolStripMenuItem_Click);
            // 
            // contextMenuStripSearchEngine
            // 
            this.contextMenuStripSearchEngine.Name = "contextMenuStripSearchEngine";
            this.contextMenuStripSearchEngine.Size = new System.Drawing.Size(181, 26);
            // 
            // SpellCheck
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(713, 427);
            this.ContextMenuStrip = this.contextMenuStripWindow;
            this.Controls.Add(this.panelBookmark);
            this.Controls.Add(this.pictureBoxBookmark);
            this.Controls.Add(this.buttonSpellCheckDownload);
            this.Controls.Add(this.labelActionInfo);
            this.Controls.Add(this.comboBoxDictionaries);
            this.Controls.Add(this.buttonEditWholeText);
            this.Controls.Add(this.groupBoxSuggestions);
            this.Controls.Add(this.labelFullText);
            this.Controls.Add(this.buttonAbort);
            this.Controls.Add(this.labelLanguage);
            this.Controls.Add(this.richTextBoxParagraph);
            this.Controls.Add(this.groupBoxWordNotFound);
            this.Controls.Add(this.groupBoxEditWholeText);
            this.Controls.Add(this.pictureBoxBdSup);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SpellCheck";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Kom";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SpellCheck_FormClosing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormSpellCheck_KeyDown);
            this.contextMenuStrip1.ResumeLayout(false);
            this.groupBoxWordNotFound.ResumeLayout(false);
            this.groupBoxWordNotFound.PerformLayout();
            this.groupBoxSuggestions.ResumeLayout(false);
            this.groupBoxSuggestions.PerformLayout();
            this.groupBoxEditWholeText.ResumeLayout(false);
            this.groupBoxEditWholeText.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxBdSup)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxBookmark)).EndInit();
            this.panelBookmark.ResumeLayout(false);
            this.panelBookmark.PerformLayout();
            this.contextMenuStripWindow.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonAddToDictionary;
        private System.Windows.Forms.Button buttonSkipOnce;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxDictionaries;
        private System.Windows.Forms.Label labelLanguage;
        private System.Windows.Forms.RichTextBox richTextBoxParagraph;
        private Nikse.SubtitleEdit.Controls.NikseListBox listBoxSuggestions;
        private System.Windows.Forms.Label labelFullText;
        private Nikse.SubtitleEdit.Controls.NikseTextBox textBoxWord;
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
        private Nikse.SubtitleEdit.Controls.NikseTextBox textBoxWholeText;
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
        private System.Windows.Forms.PictureBox pictureBoxBdSup;
        private System.Windows.Forms.ToolStripMenuItem openImagedBasedSourceFileToolStripMenuItem;
        private System.Windows.Forms.PictureBox pictureBoxBookmark;
        private System.Windows.Forms.Panel panelBookmark;
        private System.Windows.Forms.Label labelBookmark;
        private System.Windows.Forms.ToolStripMenuItem bookmarkCommentToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem useLargerFontForThisWindowToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripWindow;
        private System.Windows.Forms.ToolStripMenuItem useLargerFontForThisWindowToolStripMenuItem1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripSearchEngine;
    }
}