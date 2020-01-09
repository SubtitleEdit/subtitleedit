namespace Nikse.SubtitleEdit.Forms.Ocr
{
    sealed partial class OcrSpellCheck
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
            this.components = new System.ComponentModel.Container();
            this.buttonAbort = new System.Windows.Forms.Button();
            this.labelWordNotFound = new System.Windows.Forms.Label();
            this.groupBoxSuggestions = new System.Windows.Forms.GroupBox();
            this.buttonUseSuggestionAlways = new System.Windows.Forms.Button();
            this.buttonUseSuggestion = new System.Windows.Forms.Button();
            this.listBoxSuggestions = new System.Windows.Forms.ListBox();
            this.GroupBoxEditWord = new System.Windows.Forms.GroupBox();
            this.buttonGoogleIt = new System.Windows.Forms.Button();
            this.buttonAddToDictionary = new System.Windows.Forms.Button();
            this.buttonChangeAll = new System.Windows.Forms.Button();
            this.buttonChange = new System.Windows.Forms.Button();
            this.buttonSkipAll = new System.Windows.Forms.Button();
            this.textBoxWord = new System.Windows.Forms.TextBox();
            this.buttonSkipOnce = new System.Windows.Forms.Button();
            this.buttonAddToNames = new System.Windows.Forms.Button();
            this.groupBoxEditWholeText = new System.Windows.Forms.GroupBox();
            this.buttonChangeAllWholeText = new System.Windows.Forms.Button();
            this.buttonSkipText = new System.Windows.Forms.Button();
            this.buttonChangeWholeText = new System.Windows.Forms.Button();
            this.textBoxWholeText = new System.Windows.Forms.TextBox();
            this.groupBoxTextAsImage = new System.Windows.Forms.GroupBox();
            this.pictureBoxText = new System.Windows.Forms.PictureBox();
            this.groupBoxText = new System.Windows.Forms.GroupBox();
            this.buttonEditImageDb = new System.Windows.Forms.Button();
            this.richTextBoxParagraph = new System.Windows.Forms.RichTextBox();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addXToNamesnoiseListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addXToUserDictionaryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonEditWholeText = new System.Windows.Forms.Button();
            this.buttonEditWord = new System.Windows.Forms.Button();
            this.groupBoxSuggestions.SuspendLayout();
            this.GroupBoxEditWord.SuspendLayout();
            this.groupBoxEditWholeText.SuspendLayout();
            this.groupBoxTextAsImage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxText)).BeginInit();
            this.groupBoxText.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonAbort
            // 
            this.buttonAbort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAbort.Location = new System.Drawing.Point(624, 438);
            this.buttonAbort.Name = "buttonAbort";
            this.buttonAbort.Size = new System.Drawing.Size(85, 23);
            this.buttonAbort.TabIndex = 4;
            this.buttonAbort.Text = "Abort";
            this.buttonAbort.UseVisualStyleBackColor = true;
            this.buttonAbort.Click += new System.EventHandler(this.ButtonAbortClick);
            // 
            // labelWordNotFound
            // 
            this.labelWordNotFound.AutoSize = true;
            this.labelWordNotFound.Location = new System.Drawing.Point(9, 112);
            this.labelWordNotFound.Name = "labelWordNotFound";
            this.labelWordNotFound.Size = new System.Drawing.Size(0, 13);
            this.labelWordNotFound.TabIndex = 23;
            // 
            // groupBoxSuggestions
            // 
            this.groupBoxSuggestions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxSuggestions.Controls.Add(this.buttonUseSuggestionAlways);
            this.groupBoxSuggestions.Controls.Add(this.buttonUseSuggestion);
            this.groupBoxSuggestions.Controls.Add(this.listBoxSuggestions);
            this.groupBoxSuggestions.Location = new System.Drawing.Point(320, 240);
            this.groupBoxSuggestions.Name = "groupBoxSuggestions";
            this.groupBoxSuggestions.Size = new System.Drawing.Size(389, 192);
            this.groupBoxSuggestions.TabIndex = 3;
            this.groupBoxSuggestions.TabStop = false;
            this.groupBoxSuggestions.Text = "Suggestions";
            // 
            // buttonUseSuggestionAlways
            // 
            this.buttonUseSuggestionAlways.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonUseSuggestionAlways.Location = new System.Drawing.Point(292, 16);
            this.buttonUseSuggestionAlways.Name = "buttonUseSuggestionAlways";
            this.buttonUseSuggestionAlways.Size = new System.Drawing.Size(87, 23);
            this.buttonUseSuggestionAlways.TabIndex = 1;
            this.buttonUseSuggestionAlways.Text = "Use always";
            this.buttonUseSuggestionAlways.UseVisualStyleBackColor = true;
            this.buttonUseSuggestionAlways.Click += new System.EventHandler(this.ButtonUseSuggestionAlwaysClick);
            // 
            // buttonUseSuggestion
            // 
            this.buttonUseSuggestion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonUseSuggestion.Location = new System.Drawing.Point(207, 16);
            this.buttonUseSuggestion.Name = "buttonUseSuggestion";
            this.buttonUseSuggestion.Size = new System.Drawing.Size(79, 23);
            this.buttonUseSuggestion.TabIndex = 0;
            this.buttonUseSuggestion.Text = "Use";
            this.buttonUseSuggestion.UseVisualStyleBackColor = true;
            this.buttonUseSuggestion.Click += new System.EventHandler(this.ButtonUseSuggestionClick);
            // 
            // listBoxSuggestions
            // 
            this.listBoxSuggestions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxSuggestions.FormattingEnabled = true;
            this.listBoxSuggestions.Location = new System.Drawing.Point(6, 43);
            this.listBoxSuggestions.Name = "listBoxSuggestions";
            this.listBoxSuggestions.Size = new System.Drawing.Size(373, 121);
            this.listBoxSuggestions.TabIndex = 2;
            // 
            // GroupBoxEditWord
            // 
            this.GroupBoxEditWord.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.GroupBoxEditWord.Controls.Add(this.buttonGoogleIt);
            this.GroupBoxEditWord.Controls.Add(this.buttonAddToDictionary);
            this.GroupBoxEditWord.Controls.Add(this.buttonChangeAll);
            this.GroupBoxEditWord.Controls.Add(this.buttonChange);
            this.GroupBoxEditWord.Controls.Add(this.buttonSkipAll);
            this.GroupBoxEditWord.Controls.Add(this.textBoxWord);
            this.GroupBoxEditWord.Controls.Add(this.buttonSkipOnce);
            this.GroupBoxEditWord.Controls.Add(this.buttonAddToNames);
            this.GroupBoxEditWord.Location = new System.Drawing.Point(12, 240);
            this.GroupBoxEditWord.Name = "GroupBoxEditWord";
            this.GroupBoxEditWord.Size = new System.Drawing.Size(302, 192);
            this.GroupBoxEditWord.TabIndex = 2;
            this.GroupBoxEditWord.TabStop = false;
            this.GroupBoxEditWord.Text = "Word not found";
            // 
            // buttonGoogleIt
            // 
            this.buttonGoogleIt.Location = new System.Drawing.Point(6, 103);
            this.buttonGoogleIt.Name = "buttonGoogleIt";
            this.buttonGoogleIt.Size = new System.Drawing.Size(141, 23);
            this.buttonGoogleIt.TabIndex = 4;
            this.buttonGoogleIt.Text = "&Google it";
            this.buttonGoogleIt.UseVisualStyleBackColor = true;
            this.buttonGoogleIt.Click += new System.EventHandler(this.buttonGoogleIt_Click);
            // 
            // buttonAddToDictionary
            // 
            this.buttonAddToDictionary.Location = new System.Drawing.Point(6, 161);
            this.buttonAddToDictionary.Name = "buttonAddToDictionary";
            this.buttonAddToDictionary.Size = new System.Drawing.Size(290, 23);
            this.buttonAddToDictionary.TabIndex = 7;
            this.buttonAddToDictionary.Text = "Add to user dictionary (not case sensitive)";
            this.buttonAddToDictionary.UseVisualStyleBackColor = true;
            this.buttonAddToDictionary.Click += new System.EventHandler(this.ButtonAddToDictionaryClick);
            // 
            // buttonChangeAll
            // 
            this.buttonChangeAll.Location = new System.Drawing.Point(6, 45);
            this.buttonChangeAll.Name = "buttonChangeAll";
            this.buttonChangeAll.Size = new System.Drawing.Size(290, 23);
            this.buttonChangeAll.TabIndex = 1;
            this.buttonChangeAll.Text = "Change and add to OCR fix list (case sensitive)";
            this.buttonChangeAll.UseVisualStyleBackColor = true;
            this.buttonChangeAll.Click += new System.EventHandler(this.ButtonChangeAllClick);
            // 
            // buttonChange
            // 
            this.buttonChange.Location = new System.Drawing.Point(6, 74);
            this.buttonChange.Name = "buttonChange";
            this.buttonChange.Size = new System.Drawing.Size(141, 23);
            this.buttonChange.TabIndex = 2;
            this.buttonChange.Text = "Change once";
            this.buttonChange.UseVisualStyleBackColor = true;
            this.buttonChange.Click += new System.EventHandler(this.ButtonChangeClick);
            // 
            // buttonSkipAll
            // 
            this.buttonSkipAll.Location = new System.Drawing.Point(155, 103);
            this.buttonSkipAll.Name = "buttonSkipAll";
            this.buttonSkipAll.Size = new System.Drawing.Size(141, 23);
            this.buttonSkipAll.TabIndex = 5;
            this.buttonSkipAll.Text = "&Skip all";
            this.buttonSkipAll.UseVisualStyleBackColor = true;
            this.buttonSkipAll.Click += new System.EventHandler(this.ButtonSkipAllClick);
            // 
            // textBoxWord
            // 
            this.textBoxWord.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxWord.Location = new System.Drawing.Point(6, 19);
            this.textBoxWord.Name = "textBoxWord";
            this.textBoxWord.Size = new System.Drawing.Size(290, 22);
            this.textBoxWord.TabIndex = 0;
            this.textBoxWord.TextChanged += new System.EventHandler(this.textBoxWord_TextChanged);
            this.textBoxWord.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBoxWordKeyDown);
            // 
            // buttonSkipOnce
            // 
            this.buttonSkipOnce.Location = new System.Drawing.Point(155, 74);
            this.buttonSkipOnce.Name = "buttonSkipOnce";
            this.buttonSkipOnce.Size = new System.Drawing.Size(141, 23);
            this.buttonSkipOnce.TabIndex = 3;
            this.buttonSkipOnce.Text = "Skip &once";
            this.buttonSkipOnce.UseVisualStyleBackColor = true;
            this.buttonSkipOnce.Click += new System.EventHandler(this.ButtonSkipOnceClick);
            // 
            // buttonAddToNames
            // 
            this.buttonAddToNames.Location = new System.Drawing.Point(6, 132);
            this.buttonAddToNames.Name = "buttonAddToNames";
            this.buttonAddToNames.Size = new System.Drawing.Size(290, 23);
            this.buttonAddToNames.TabIndex = 6;
            this.buttonAddToNames.Text = "Add to names/noise list (case sensitive)";
            this.buttonAddToNames.UseVisualStyleBackColor = true;
            this.buttonAddToNames.Click += new System.EventHandler(this.ButtonAddToNamesClick);
            // 
            // groupBoxEditWholeText
            // 
            this.groupBoxEditWholeText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBoxEditWholeText.Controls.Add(this.buttonChangeAllWholeText);
            this.groupBoxEditWholeText.Controls.Add(this.buttonSkipText);
            this.groupBoxEditWholeText.Controls.Add(this.buttonChangeWholeText);
            this.groupBoxEditWholeText.Controls.Add(this.textBoxWholeText);
            this.groupBoxEditWholeText.Location = new System.Drawing.Point(12, 240);
            this.groupBoxEditWholeText.Name = "groupBoxEditWholeText";
            this.groupBoxEditWholeText.Size = new System.Drawing.Size(302, 192);
            this.groupBoxEditWholeText.TabIndex = 37;
            this.groupBoxEditWholeText.TabStop = false;
            this.groupBoxEditWholeText.Text = "Edit whole text";
            // 
            // buttonChangeAllWholeText
            // 
            this.buttonChangeAllWholeText.Location = new System.Drawing.Point(6, 115);
            this.buttonChangeAllWholeText.Name = "buttonChangeAllWholeText";
            this.buttonChangeAllWholeText.Size = new System.Drawing.Size(141, 21);
            this.buttonChangeAllWholeText.TabIndex = 36;
            this.buttonChangeAllWholeText.Text = "Change all";
            this.buttonChangeAllWholeText.UseVisualStyleBackColor = true;
            this.buttonChangeAllWholeText.Click += new System.EventHandler(this.buttonChangeAllWholeText_Click);
            // 
            // buttonSkipText
            // 
            this.buttonSkipText.Location = new System.Drawing.Point(156, 88);
            this.buttonSkipText.Name = "buttonSkipText";
            this.buttonSkipText.Size = new System.Drawing.Size(139, 21);
            this.buttonSkipText.TabIndex = 35;
            this.buttonSkipText.Text = "Skip text";
            this.buttonSkipText.UseVisualStyleBackColor = true;
            this.buttonSkipText.Click += new System.EventHandler(this.ButtonSkipTextClick);
            // 
            // buttonChangeWholeText
            // 
            this.buttonChangeWholeText.Location = new System.Drawing.Point(6, 88);
            this.buttonChangeWholeText.Name = "buttonChangeWholeText";
            this.buttonChangeWholeText.Size = new System.Drawing.Size(143, 21);
            this.buttonChangeWholeText.TabIndex = 34;
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
            this.textBoxWholeText.Size = new System.Drawing.Size(290, 63);
            this.textBoxWholeText.TabIndex = 31;
            this.textBoxWholeText.TextChanged += new System.EventHandler(this.textBoxWholeText_TextChanged);
            // 
            // groupBoxTextAsImage
            // 
            this.groupBoxTextAsImage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxTextAsImage.Controls.Add(this.pictureBoxText);
            this.groupBoxTextAsImage.Location = new System.Drawing.Point(12, 12);
            this.groupBoxTextAsImage.Name = "groupBoxTextAsImage";
            this.groupBoxTextAsImage.Size = new System.Drawing.Size(697, 141);
            this.groupBoxTextAsImage.TabIndex = 0;
            this.groupBoxTextAsImage.TabStop = false;
            this.groupBoxTextAsImage.Text = "Image text";
            // 
            // pictureBoxText
            // 
            this.pictureBoxText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBoxText.Location = new System.Drawing.Point(6, 18);
            this.pictureBoxText.Name = "pictureBoxText";
            this.pictureBoxText.Size = new System.Drawing.Size(685, 116);
            this.pictureBoxText.TabIndex = 32;
            this.pictureBoxText.TabStop = false;
            // 
            // groupBoxText
            // 
            this.groupBoxText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxText.Controls.Add(this.buttonEditImageDb);
            this.groupBoxText.Controls.Add(this.richTextBoxParagraph);
            this.groupBoxText.Controls.Add(this.buttonEditWholeText);
            this.groupBoxText.Controls.Add(this.buttonEditWord);
            this.groupBoxText.Location = new System.Drawing.Point(12, 159);
            this.groupBoxText.Name = "groupBoxText";
            this.groupBoxText.Size = new System.Drawing.Size(697, 76);
            this.groupBoxText.TabIndex = 500;
            this.groupBoxText.TabStop = false;
            this.groupBoxText.Text = "Text";
            // 
            // buttonEditImageDb
            // 
            this.buttonEditImageDb.Location = new System.Drawing.Point(514, 17);
            this.buttonEditImageDb.Name = "buttonEditImageDb";
            this.buttonEditImageDb.Size = new System.Drawing.Size(172, 47);
            this.buttonEditImageDb.TabIndex = 403;
            this.buttonEditImageDb.Text = "Edit image db";
            this.buttonEditImageDb.UseVisualStyleBackColor = true;
            this.buttonEditImageDb.Click += new System.EventHandler(this.buttonEditImageDb_Click);
            // 
            // richTextBoxParagraph
            // 
            this.richTextBoxParagraph.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.richTextBoxParagraph.ContextMenuStrip = this.contextMenuStrip1;
            this.richTextBoxParagraph.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBoxParagraph.Location = new System.Drawing.Point(6, 15);
            this.richTextBoxParagraph.Name = "richTextBoxParagraph";
            this.richTextBoxParagraph.ReadOnly = true;
            this.richTextBoxParagraph.Size = new System.Drawing.Size(296, 54);
            this.richTextBoxParagraph.TabIndex = 400;
            this.richTextBoxParagraph.Text = "";
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addXToNamesnoiseListToolStripMenuItem,
            this.addXToUserDictionaryToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(209, 48);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // addXToNamesnoiseListToolStripMenuItem
            // 
            this.addXToNamesnoiseListToolStripMenuItem.Name = "addXToNamesnoiseListToolStripMenuItem";
            this.addXToNamesnoiseListToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            this.addXToNamesnoiseListToolStripMenuItem.Text = "Add x to names/noise list";
            this.addXToNamesnoiseListToolStripMenuItem.Click += new System.EventHandler(this.addXToNamesNoiseListToolStripMenuItem_Click);
            // 
            // addXToUserDictionaryToolStripMenuItem
            // 
            this.addXToUserDictionaryToolStripMenuItem.Name = "addXToUserDictionaryToolStripMenuItem";
            this.addXToUserDictionaryToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            this.addXToUserDictionaryToolStripMenuItem.Text = "Add x to user dictionary";
            this.addXToUserDictionaryToolStripMenuItem.Click += new System.EventHandler(this.addXToUserDictionaryToolStripMenuItem_Click);
            // 
            // buttonEditWholeText
            // 
            this.buttonEditWholeText.Location = new System.Drawing.Point(308, 15);
            this.buttonEditWholeText.Name = "buttonEditWholeText";
            this.buttonEditWholeText.Size = new System.Drawing.Size(128, 23);
            this.buttonEditWholeText.TabIndex = 401;
            this.buttonEditWholeText.Text = "Edit whole text";
            this.buttonEditWholeText.UseVisualStyleBackColor = true;
            this.buttonEditWholeText.Click += new System.EventHandler(this.ButtonEditWholeTextClick);
            // 
            // buttonEditWord
            // 
            this.buttonEditWord.Location = new System.Drawing.Point(308, 44);
            this.buttonEditWord.Name = "buttonEditWord";
            this.buttonEditWord.Size = new System.Drawing.Size(128, 23);
            this.buttonEditWord.TabIndex = 402;
            this.buttonEditWord.Text = "Edit word";
            this.buttonEditWord.UseVisualStyleBackColor = true;
            this.buttonEditWord.Click += new System.EventHandler(this.ButtonEditWordClick);
            // 
            // OcrSpellCheck
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(721, 473);
            this.Controls.Add(this.groupBoxText);
            this.Controls.Add(this.groupBoxTextAsImage);
            this.Controls.Add(this.buttonAbort);
            this.Controls.Add(this.labelWordNotFound);
            this.Controls.Add(this.groupBoxSuggestions);
            this.Controls.Add(this.GroupBoxEditWord);
            this.Controls.Add(this.groupBoxEditWholeText);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinimumSize = new System.Drawing.Size(736, 512);
            this.Name = "OcrSpellCheck";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "OCR spell check";
            this.Shown += new System.EventHandler(this.OcrSpellCheck_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OcrSpellCheck_KeyDown);
            this.groupBoxSuggestions.ResumeLayout(false);
            this.GroupBoxEditWord.ResumeLayout(false);
            this.GroupBoxEditWord.PerformLayout();
            this.groupBoxEditWholeText.ResumeLayout(false);
            this.groupBoxEditWholeText.PerformLayout();
            this.groupBoxTextAsImage.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxText)).EndInit();
            this.groupBoxText.ResumeLayout(false);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonAbort;
        private System.Windows.Forms.Label labelWordNotFound;
        private System.Windows.Forms.GroupBox groupBoxSuggestions;
        private System.Windows.Forms.Button buttonUseSuggestionAlways;
        private System.Windows.Forms.Button buttonUseSuggestion;
        private System.Windows.Forms.ListBox listBoxSuggestions;
        private System.Windows.Forms.GroupBox GroupBoxEditWord;
        private System.Windows.Forms.Button buttonChangeAll;
        private System.Windows.Forms.Button buttonChange;
        private System.Windows.Forms.Button buttonSkipAll;
        private System.Windows.Forms.TextBox textBoxWord;
        private System.Windows.Forms.Button buttonSkipOnce;
        private System.Windows.Forms.Button buttonAddToNames;
        private System.Windows.Forms.GroupBox groupBoxTextAsImage;
        private System.Windows.Forms.PictureBox pictureBoxText;
        private System.Windows.Forms.GroupBox groupBoxEditWholeText;
        private System.Windows.Forms.Button buttonChangeWholeText;
        private System.Windows.Forms.TextBox textBoxWholeText;
        private System.Windows.Forms.GroupBox groupBoxText;
        private System.Windows.Forms.Button buttonEditWord;
        private System.Windows.Forms.Button buttonEditWholeText;
        private System.Windows.Forms.RichTextBox richTextBoxParagraph;
        private System.Windows.Forms.Button buttonAddToDictionary;
        private System.Windows.Forms.Button buttonSkipText;
        private System.Windows.Forms.Button buttonChangeAllWholeText;
        private System.Windows.Forms.Button buttonGoogleIt;
        private System.Windows.Forms.Button buttonEditImageDb;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem addXToNamesnoiseListToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addXToUserDictionaryToolStripMenuItem;
    }
}