namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class FormRemoveTextForHearImpaired
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
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.groupBoxLinesFound = new System.Windows.Forms.GroupBox();
            this.listViewFixes = new System.Windows.Forms.ListView();
            this.columnHeaderApply = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderLine = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderBefore = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderAfter = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemSelAll = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemInvertSel = new System.Windows.Forms.ToolStripMenuItem();
            this.checkBoxRemoveTextBeforeColon = new System.Windows.Forms.CheckBox();
            this.groupBoxRemoveTextConditions = new System.Windows.Forms.GroupBox();
            this.checkBoxInterjectionOnlySeparateLine = new System.Windows.Forms.CheckBox();
            this.checkBoxRemoveIfAllUppercase = new System.Windows.Forms.CheckBox();
            this.checkBoxColonSeparateLine = new System.Windows.Forms.CheckBox();
            this.buttonEditInterjections = new System.Windows.Forms.Button();
            this.checkBoxRemoveInterjections = new System.Windows.Forms.CheckBox();
            this.labelRemoveTextBetween = new System.Windows.Forms.Label();
            this.comboBoxRemoveIfTextContains = new System.Windows.Forms.ComboBox();
            this.checkBoxRemoveWhereContains = new System.Windows.Forms.CheckBox();
            this.comboBoxCustomEnd = new System.Windows.Forms.ComboBox();
            this.comboBoxCustomStart = new System.Windows.Forms.ComboBox();
            this.checkBoxRemoveTextBeforeColonOnlyUppercase = new System.Windows.Forms.CheckBox();
            this.checkBoxOnlyIfInSeparateLine = new System.Windows.Forms.CheckBox();
            this.checkBoxRemoveTextBetweenCustomTags = new System.Windows.Forms.CheckBox();
            this.checkBoxRemoveTextBetweenQuestionMarks = new System.Windows.Forms.CheckBox();
            this.checkBoxRemoveTextBetweenParentheses = new System.Windows.Forms.CheckBox();
            this.checkBoxRemoveTextBetweenBrackets = new System.Windows.Forms.CheckBox();
            this.checkBoxRemoveTextBetweenSquares = new System.Windows.Forms.CheckBox();
            this.labelAnd = new System.Windows.Forms.Label();
            this.buttonApply = new System.Windows.Forms.Button();
            this.textBoxAfterText = new System.Windows.Forms.TextBox();
            this.labelText = new System.Windows.Forms.Label();
            this.groupBoxLinesFound.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.groupBoxRemoveTextConditions.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(716, 563);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 3;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.ButtonOkClick);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(797, 563);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 4;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // groupBoxLinesFound
            // 
            this.groupBoxLinesFound.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxLinesFound.Controls.Add(this.listViewFixes);
            this.groupBoxLinesFound.Location = new System.Drawing.Point(12, 222);
            this.groupBoxLinesFound.Name = "groupBoxLinesFound";
            this.groupBoxLinesFound.Size = new System.Drawing.Size(941, 291);
            this.groupBoxLinesFound.TabIndex = 1;
            this.groupBoxLinesFound.TabStop = false;
            this.groupBoxLinesFound.Text = "Lines found";
            // 
            // listViewFixes
            // 
            this.listViewFixes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewFixes.CheckBoxes = true;
            this.listViewFixes.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderApply,
            this.columnHeaderLine,
            this.columnHeaderBefore,
            this.columnHeaderAfter});
            this.listViewFixes.ContextMenuStrip = this.contextMenuStrip1;
            this.listViewFixes.FullRowSelect = true;
            this.listViewFixes.HideSelection = false;
            this.listViewFixes.Location = new System.Drawing.Point(6, 17);
            this.listViewFixes.Name = "listViewFixes";
            this.listViewFixes.Size = new System.Drawing.Size(929, 268);
            this.listViewFixes.TabIndex = 0;
            this.listViewFixes.UseCompatibleStateImageBehavior = false;
            this.listViewFixes.View = System.Windows.Forms.View.Details;
            this.listViewFixes.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.listViewFixes_ItemChecked);
            this.listViewFixes.SelectedIndexChanged += new System.EventHandler(this.listViewFixes_SelectedIndexChanged);
            // 
            // columnHeaderApply
            // 
            this.columnHeaderApply.Text = "Apply";
            this.columnHeaderApply.Width = 45;
            // 
            // columnHeaderLine
            // 
            this.columnHeaderLine.Text = "Line#";
            this.columnHeaderLine.Width = 61;
            // 
            // columnHeaderBefore
            // 
            this.columnHeaderBefore.Text = "Before";
            this.columnHeaderBefore.Width = 401;
            // 
            // columnHeaderAfter
            // 
            this.columnHeaderAfter.Text = "After";
            this.columnHeaderAfter.Width = 395;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemSelAll,
            this.toolStripMenuItemInvertSel});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(155, 48);
            // 
            // toolStripMenuItemSelAll
            // 
            this.toolStripMenuItemSelAll.Name = "toolStripMenuItemSelAll";
            this.toolStripMenuItemSelAll.Size = new System.Drawing.Size(154, 22);
            this.toolStripMenuItemSelAll.Text = "Select all";
            this.toolStripMenuItemSelAll.Click += new System.EventHandler(this.toolStripMenuItemSelAll_Click);
            // 
            // toolStripMenuItemInvertSel
            // 
            this.toolStripMenuItemInvertSel.Name = "toolStripMenuItemInvertSel";
            this.toolStripMenuItemInvertSel.Size = new System.Drawing.Size(154, 22);
            this.toolStripMenuItemInvertSel.Text = "Invert selection";
            this.toolStripMenuItemInvertSel.Click += new System.EventHandler(this.toolStripMenuItemInvertSel_Click);
            // 
            // checkBoxRemoveTextBeforeColon
            // 
            this.checkBoxRemoveTextBeforeColon.AutoSize = true;
            this.checkBoxRemoveTextBeforeColon.Checked = true;
            this.checkBoxRemoveTextBeforeColon.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxRemoveTextBeforeColon.Location = new System.Drawing.Point(241, 26);
            this.checkBoxRemoveTextBeforeColon.Name = "checkBoxRemoveTextBeforeColon";
            this.checkBoxRemoveTextBeforeColon.Size = new System.Drawing.Size(175, 17);
            this.checkBoxRemoveTextBeforeColon.TabIndex = 10;
            this.checkBoxRemoveTextBeforeColon.Text = "Remove text before a colon (:)";
            this.checkBoxRemoveTextBeforeColon.UseVisualStyleBackColor = true;
            this.checkBoxRemoveTextBeforeColon.CheckedChanged += new System.EventHandler(this.checkBoxRemoveTextBeforeColon_CheckedChanged);
            // 
            // groupBoxRemoveTextConditions
            // 
            this.groupBoxRemoveTextConditions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxRemoveTextConditions.Controls.Add(this.checkBoxInterjectionOnlySeparateLine);
            this.groupBoxRemoveTextConditions.Controls.Add(this.checkBoxRemoveIfAllUppercase);
            this.groupBoxRemoveTextConditions.Controls.Add(this.checkBoxColonSeparateLine);
            this.groupBoxRemoveTextConditions.Controls.Add(this.buttonEditInterjections);
            this.groupBoxRemoveTextConditions.Controls.Add(this.checkBoxRemoveInterjections);
            this.groupBoxRemoveTextConditions.Controls.Add(this.labelRemoveTextBetween);
            this.groupBoxRemoveTextConditions.Controls.Add(this.comboBoxRemoveIfTextContains);
            this.groupBoxRemoveTextConditions.Controls.Add(this.checkBoxRemoveWhereContains);
            this.groupBoxRemoveTextConditions.Controls.Add(this.comboBoxCustomEnd);
            this.groupBoxRemoveTextConditions.Controls.Add(this.comboBoxCustomStart);
            this.groupBoxRemoveTextConditions.Controls.Add(this.checkBoxRemoveTextBeforeColonOnlyUppercase);
            this.groupBoxRemoveTextConditions.Controls.Add(this.checkBoxRemoveTextBeforeColon);
            this.groupBoxRemoveTextConditions.Controls.Add(this.checkBoxOnlyIfInSeparateLine);
            this.groupBoxRemoveTextConditions.Controls.Add(this.checkBoxRemoveTextBetweenCustomTags);
            this.groupBoxRemoveTextConditions.Controls.Add(this.checkBoxRemoveTextBetweenQuestionMarks);
            this.groupBoxRemoveTextConditions.Controls.Add(this.checkBoxRemoveTextBetweenParentheses);
            this.groupBoxRemoveTextConditions.Controls.Add(this.checkBoxRemoveTextBetweenBrackets);
            this.groupBoxRemoveTextConditions.Controls.Add(this.checkBoxRemoveTextBetweenSquares);
            this.groupBoxRemoveTextConditions.Controls.Add(this.labelAnd);
            this.groupBoxRemoveTextConditions.Location = new System.Drawing.Point(12, 12);
            this.groupBoxRemoveTextConditions.Name = "groupBoxRemoveTextConditions";
            this.groupBoxRemoveTextConditions.Size = new System.Drawing.Size(941, 204);
            this.groupBoxRemoveTextConditions.TabIndex = 0;
            this.groupBoxRemoveTextConditions.TabStop = false;
            this.groupBoxRemoveTextConditions.Text = "Remove text conditions";
            // 
            // checkBoxInterjectionOnlySeparateLine
            // 
            this.checkBoxInterjectionOnlySeparateLine.AutoSize = true;
            this.checkBoxInterjectionOnlySeparateLine.Location = new System.Drawing.Point(600, 53);
            this.checkBoxInterjectionOnlySeparateLine.Name = "checkBoxInterjectionOnlySeparateLine";
            this.checkBoxInterjectionOnlySeparateLine.Size = new System.Drawing.Size(137, 17);
            this.checkBoxInterjectionOnlySeparateLine.TabIndex = 18;
            this.checkBoxInterjectionOnlySeparateLine.Text = "Only if on separate line";
            this.checkBoxInterjectionOnlySeparateLine.UseVisualStyleBackColor = true;
            this.checkBoxInterjectionOnlySeparateLine.CheckedChanged += new System.EventHandler(this.checkBoxInterjectionOnlySeparateLine_CheckedChanged);
            // 
            // checkBoxRemoveIfAllUppercase
            // 
            this.checkBoxRemoveIfAllUppercase.AutoSize = true;
            this.checkBoxRemoveIfAllUppercase.Checked = true;
            this.checkBoxRemoveIfAllUppercase.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxRemoveIfAllUppercase.Location = new System.Drawing.Point(241, 102);
            this.checkBoxRemoveIfAllUppercase.Name = "checkBoxRemoveIfAllUppercase";
            this.checkBoxRemoveIfAllUppercase.Size = new System.Drawing.Size(142, 17);
            this.checkBoxRemoveIfAllUppercase.TabIndex = 13;
            this.checkBoxRemoveIfAllUppercase.Text = "Remove uppercase lines";
            this.checkBoxRemoveIfAllUppercase.UseVisualStyleBackColor = true;
            this.checkBoxRemoveIfAllUppercase.CheckedChanged += new System.EventHandler(this.checkBoxRemoveIfAllUppercase_CheckedChanged);
            // 
            // checkBoxColonSeparateLine
            // 
            this.checkBoxColonSeparateLine.AutoSize = true;
            this.checkBoxColonSeparateLine.Location = new System.Drawing.Point(261, 69);
            this.checkBoxColonSeparateLine.Name = "checkBoxColonSeparateLine";
            this.checkBoxColonSeparateLine.Size = new System.Drawing.Size(137, 17);
            this.checkBoxColonSeparateLine.TabIndex = 12;
            this.checkBoxColonSeparateLine.Text = "Only if on separate line";
            this.checkBoxColonSeparateLine.UseVisualStyleBackColor = true;
            this.checkBoxColonSeparateLine.CheckedChanged += new System.EventHandler(this.CheckBoxRemoveTextBetweenCheckedChanged);
            // 
            // buttonEditInterjections
            // 
            this.buttonEditInterjections.Location = new System.Drawing.Point(718, 26);
            this.buttonEditInterjections.Name = "buttonEditInterjections";
            this.buttonEditInterjections.Size = new System.Drawing.Size(103, 23);
            this.buttonEditInterjections.TabIndex = 17;
            this.buttonEditInterjections.Text = "Edit...";
            this.buttonEditInterjections.UseVisualStyleBackColor = true;
            this.buttonEditInterjections.Click += new System.EventHandler(this.buttonEditInterjections_Click);
            // 
            // checkBoxRemoveInterjections
            // 
            this.checkBoxRemoveInterjections.AutoSize = true;
            this.checkBoxRemoveInterjections.Location = new System.Drawing.Point(582, 30);
            this.checkBoxRemoveInterjections.Name = "checkBoxRemoveInterjections";
            this.checkBoxRemoveInterjections.Size = new System.Drawing.Size(130, 17);
            this.checkBoxRemoveInterjections.TabIndex = 16;
            this.checkBoxRemoveInterjections.Text = "Remove interjections ";
            this.checkBoxRemoveInterjections.UseVisualStyleBackColor = true;
            this.checkBoxRemoveInterjections.CheckedChanged += new System.EventHandler(this.checkBoxRemoveInterjections_CheckedChanged);
            // 
            // labelRemoveTextBetween
            // 
            this.labelRemoveTextBetween.AutoSize = true;
            this.labelRemoveTextBetween.Location = new System.Drawing.Point(8, 26);
            this.labelRemoveTextBetween.Name = "labelRemoveTextBetween";
            this.labelRemoveTextBetween.Size = new System.Drawing.Size(114, 13);
            this.labelRemoveTextBetween.TabIndex = 0;
            this.labelRemoveTextBetween.Text = "Remove text between";
            // 
            // comboBoxRemoveIfTextContains
            // 
            this.comboBoxRemoveIfTextContains.FormattingEnabled = true;
            this.comboBoxRemoveIfTextContains.Items.AddRange(new object[] {
            "¶",
            "♪",
            "♫",
            "♪,♫"});
            this.comboBoxRemoveIfTextContains.Location = new System.Drawing.Point(427, 131);
            this.comboBoxRemoveIfTextContains.MaxLength = 25;
            this.comboBoxRemoveIfTextContains.Name = "comboBoxRemoveIfTextContains";
            this.comboBoxRemoveIfTextContains.Size = new System.Drawing.Size(122, 21);
            this.comboBoxRemoveIfTextContains.TabIndex = 15;
            this.comboBoxRemoveIfTextContains.Text = "¶";
            this.comboBoxRemoveIfTextContains.TextChanged += new System.EventHandler(this.CheckBoxRemoveTextBetweenCheckedChanged);
            // 
            // checkBoxRemoveWhereContains
            // 
            this.checkBoxRemoveWhereContains.AutoSize = true;
            this.checkBoxRemoveWhereContains.Location = new System.Drawing.Point(241, 135);
            this.checkBoxRemoveWhereContains.Name = "checkBoxRemoveWhereContains";
            this.checkBoxRemoveWhereContains.Size = new System.Drawing.Size(153, 17);
            this.checkBoxRemoveWhereContains.TabIndex = 14;
            this.checkBoxRemoveWhereContains.Text = "Remove text if it contains:";
            this.checkBoxRemoveWhereContains.UseVisualStyleBackColor = true;
            this.checkBoxRemoveWhereContains.CheckedChanged += new System.EventHandler(this.CheckBoxRemoveTextBetweenCheckedChanged);
            // 
            // comboBoxCustomEnd
            // 
            this.comboBoxCustomEnd.FormattingEnabled = true;
            this.comboBoxCustomEnd.Items.AddRange(new object[] {
            "¶",
            "♪",
            "♫"});
            this.comboBoxCustomEnd.Location = new System.Drawing.Point(116, 135);
            this.comboBoxCustomEnd.MaxLength = 2;
            this.comboBoxCustomEnd.Name = "comboBoxCustomEnd";
            this.comboBoxCustomEnd.Size = new System.Drawing.Size(38, 21);
            this.comboBoxCustomEnd.TabIndex = 8;
            this.comboBoxCustomEnd.Text = "¶";
            this.comboBoxCustomEnd.TextChanged += new System.EventHandler(this.CheckBoxRemoveTextBetweenCheckedChanged);
            // 
            // comboBoxCustomStart
            // 
            this.comboBoxCustomStart.FormattingEnabled = true;
            this.comboBoxCustomStart.Items.AddRange(new object[] {
            "¶",
            "♪",
            "♫"});
            this.comboBoxCustomStart.Location = new System.Drawing.Point(41, 135);
            this.comboBoxCustomStart.MaxLength = 2;
            this.comboBoxCustomStart.Name = "comboBoxCustomStart";
            this.comboBoxCustomStart.Size = new System.Drawing.Size(38, 21);
            this.comboBoxCustomStart.TabIndex = 6;
            this.comboBoxCustomStart.Text = "¶";
            this.comboBoxCustomStart.TextChanged += new System.EventHandler(this.CheckBoxRemoveTextBetweenCheckedChanged);
            // 
            // checkBoxRemoveTextBeforeColonOnlyUppercase
            // 
            this.checkBoxRemoveTextBeforeColonOnlyUppercase.AutoSize = true;
            this.checkBoxRemoveTextBeforeColonOnlyUppercase.Location = new System.Drawing.Point(261, 46);
            this.checkBoxRemoveTextBeforeColonOnlyUppercase.Name = "checkBoxRemoveTextBeforeColonOnlyUppercase";
            this.checkBoxRemoveTextBeforeColonOnlyUppercase.Size = new System.Drawing.Size(151, 17);
            this.checkBoxRemoveTextBeforeColonOnlyUppercase.TabIndex = 11;
            this.checkBoxRemoveTextBeforeColonOnlyUppercase.Text = "Only if text is UPPERCASE";
            this.checkBoxRemoveTextBeforeColonOnlyUppercase.UseVisualStyleBackColor = true;
            this.checkBoxRemoveTextBeforeColonOnlyUppercase.CheckedChanged += new System.EventHandler(this.CheckBoxRemoveTextBetweenCheckedChanged);
            // 
            // checkBoxOnlyIfInSeparateLine
            // 
            this.checkBoxOnlyIfInSeparateLine.AutoSize = true;
            this.checkBoxOnlyIfInSeparateLine.Location = new System.Drawing.Point(21, 172);
            this.checkBoxOnlyIfInSeparateLine.Name = "checkBoxOnlyIfInSeparateLine";
            this.checkBoxOnlyIfInSeparateLine.Size = new System.Drawing.Size(137, 17);
            this.checkBoxOnlyIfInSeparateLine.TabIndex = 9;
            this.checkBoxOnlyIfInSeparateLine.Text = "Only if on separate line";
            this.checkBoxOnlyIfInSeparateLine.UseVisualStyleBackColor = true;
            this.checkBoxOnlyIfInSeparateLine.CheckedChanged += new System.EventHandler(this.CheckBoxRemoveTextBetweenCheckedChanged);
            // 
            // checkBoxRemoveTextBetweenCustomTags
            // 
            this.checkBoxRemoveTextBetweenCustomTags.AutoSize = true;
            this.checkBoxRemoveTextBetweenCustomTags.Checked = true;
            this.checkBoxRemoveTextBetweenCustomTags.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxRemoveTextBetweenCustomTags.Location = new System.Drawing.Point(20, 138);
            this.checkBoxRemoveTextBetweenCustomTags.Name = "checkBoxRemoveTextBetweenCustomTags";
            this.checkBoxRemoveTextBetweenCustomTags.Size = new System.Drawing.Size(15, 14);
            this.checkBoxRemoveTextBetweenCustomTags.TabIndex = 5;
            this.checkBoxRemoveTextBetweenCustomTags.UseVisualStyleBackColor = true;
            this.checkBoxRemoveTextBetweenCustomTags.CheckedChanged += new System.EventHandler(this.CheckBoxRemoveTextBetweenCheckedChanged);
            // 
            // checkBoxRemoveTextBetweenQuestionMarks
            // 
            this.checkBoxRemoveTextBetweenQuestionMarks.AutoSize = true;
            this.checkBoxRemoveTextBetweenQuestionMarks.Checked = true;
            this.checkBoxRemoveTextBetweenQuestionMarks.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxRemoveTextBetweenQuestionMarks.Location = new System.Drawing.Point(20, 115);
            this.checkBoxRemoveTextBetweenQuestionMarks.Name = "checkBoxRemoveTextBetweenQuestionMarks";
            this.checkBoxRemoveTextBetweenQuestionMarks.Size = new System.Drawing.Size(76, 17);
            this.checkBoxRemoveTextBetweenQuestionMarks.TabIndex = 4;
            this.checkBoxRemoveTextBetweenQuestionMarks.Text = "\"?\" and \"?\"";
            this.checkBoxRemoveTextBetweenQuestionMarks.UseVisualStyleBackColor = true;
            this.checkBoxRemoveTextBetweenQuestionMarks.CheckedChanged += new System.EventHandler(this.CheckBoxRemoveTextBetweenCheckedChanged);
            // 
            // checkBoxRemoveTextBetweenParentheses
            // 
            this.checkBoxRemoveTextBetweenParentheses.AutoSize = true;
            this.checkBoxRemoveTextBetweenParentheses.Checked = true;
            this.checkBoxRemoveTextBetweenParentheses.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxRemoveTextBetweenParentheses.Location = new System.Drawing.Point(20, 69);
            this.checkBoxRemoveTextBetweenParentheses.Name = "checkBoxRemoveTextBetweenParentheses";
            this.checkBoxRemoveTextBetweenParentheses.Size = new System.Drawing.Size(74, 17);
            this.checkBoxRemoveTextBetweenParentheses.TabIndex = 2;
            this.checkBoxRemoveTextBetweenParentheses.Text = "\"(\" and \")\"";
            this.checkBoxRemoveTextBetweenParentheses.UseVisualStyleBackColor = true;
            this.checkBoxRemoveTextBetweenParentheses.CheckedChanged += new System.EventHandler(this.CheckBoxRemoveTextBetweenCheckedChanged);
            // 
            // checkBoxRemoveTextBetweenBrackets
            // 
            this.checkBoxRemoveTextBetweenBrackets.AutoSize = true;
            this.checkBoxRemoveTextBetweenBrackets.Checked = true;
            this.checkBoxRemoveTextBetweenBrackets.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxRemoveTextBetweenBrackets.Location = new System.Drawing.Point(20, 92);
            this.checkBoxRemoveTextBetweenBrackets.Name = "checkBoxRemoveTextBetweenBrackets";
            this.checkBoxRemoveTextBetweenBrackets.Size = new System.Drawing.Size(76, 17);
            this.checkBoxRemoveTextBetweenBrackets.TabIndex = 3;
            this.checkBoxRemoveTextBetweenBrackets.Text = "\"{\" and \"}\"";
            this.checkBoxRemoveTextBetweenBrackets.UseVisualStyleBackColor = true;
            this.checkBoxRemoveTextBetweenBrackets.CheckedChanged += new System.EventHandler(this.CheckBoxRemoveTextBetweenCheckedChanged);
            // 
            // checkBoxRemoveTextBetweenSquares
            // 
            this.checkBoxRemoveTextBetweenSquares.AutoSize = true;
            this.checkBoxRemoveTextBetweenSquares.Checked = true;
            this.checkBoxRemoveTextBetweenSquares.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxRemoveTextBetweenSquares.Location = new System.Drawing.Point(20, 45);
            this.checkBoxRemoveTextBetweenSquares.Name = "checkBoxRemoveTextBetweenSquares";
            this.checkBoxRemoveTextBetweenSquares.Size = new System.Drawing.Size(74, 17);
            this.checkBoxRemoveTextBetweenSquares.TabIndex = 1;
            this.checkBoxRemoveTextBetweenSquares.Text = "\"[\" and \"]\"";
            this.checkBoxRemoveTextBetweenSquares.UseVisualStyleBackColor = true;
            this.checkBoxRemoveTextBetweenSquares.CheckedChanged += new System.EventHandler(this.CheckBoxRemoveTextBetweenCheckedChanged);
            // 
            // labelAnd
            // 
            this.labelAnd.AutoSize = true;
            this.labelAnd.Location = new System.Drawing.Point(85, 141);
            this.labelAnd.Name = "labelAnd";
            this.labelAnd.Size = new System.Drawing.Size(25, 13);
            this.labelAnd.TabIndex = 7;
            this.labelAnd.Text = "and";
            // 
            // buttonApply
            // 
            this.buttonApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonApply.Location = new System.Drawing.Point(878, 563);
            this.buttonApply.Name = "buttonApply";
            this.buttonApply.Size = new System.Drawing.Size(75, 23);
            this.buttonApply.TabIndex = 5;
            this.buttonApply.Text = "Apply";
            this.buttonApply.UseVisualStyleBackColor = true;
            this.buttonApply.Click += new System.EventHandler(this.buttonApply_Click);
            // 
            // textBoxAfterText
            // 
            this.textBoxAfterText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBoxAfterText.Location = new System.Drawing.Point(12, 543);
            this.textBoxAfterText.Multiline = true;
            this.textBoxAfterText.Name = "textBoxAfterText";
            this.textBoxAfterText.Size = new System.Drawing.Size(394, 43);
            this.textBoxAfterText.TabIndex = 2;
            this.textBoxAfterText.TextChanged += new System.EventHandler(this.textBoxAfterText_TextChanged);
            // 
            // labelText
            // 
            this.labelText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelText.AutoSize = true;
            this.labelText.Location = new System.Drawing.Point(12, 527);
            this.labelText.Name = "labelText";
            this.labelText.Size = new System.Drawing.Size(29, 13);
            this.labelText.TabIndex = 6;
            this.labelText.Text = "Text";
            // 
            // FormRemoveTextForHearImpaired
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(965, 598);
            this.Controls.Add(this.labelText);
            this.Controls.Add(this.textBoxAfterText);
            this.Controls.Add(this.buttonApply);
            this.Controls.Add(this.groupBoxRemoveTextConditions);
            this.Controls.Add(this.groupBoxLinesFound);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.KeyPreview = true;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(705, 514);
            this.Name = "FormRemoveTextForHearImpaired";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Remove text for hearing impaired";
            this.Load += new System.EventHandler(this.FormRemoveTextForHearImpaired_Load);
            this.Shown += new System.EventHandler(this.FormRemoveTextForHearImpaired_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormRemoveTextForHearImpaired_KeyDown);
            this.Resize += new System.EventHandler(this.FormRemoveTextForHearImpaired_Resize);
            this.groupBoxLinesFound.ResumeLayout(false);
            this.contextMenuStrip1.ResumeLayout(false);
            this.groupBoxRemoveTextConditions.ResumeLayout(false);
            this.groupBoxRemoveTextConditions.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.GroupBox groupBoxLinesFound;
        private System.Windows.Forms.ListView listViewFixes;
        private System.Windows.Forms.ColumnHeader columnHeaderApply;
        private System.Windows.Forms.ColumnHeader columnHeaderLine;
        private System.Windows.Forms.ColumnHeader columnHeaderBefore;
        private System.Windows.Forms.ColumnHeader columnHeaderAfter;
        private System.Windows.Forms.CheckBox checkBoxRemoveTextBeforeColon;
        private System.Windows.Forms.GroupBox groupBoxRemoveTextConditions;
        private System.Windows.Forms.CheckBox checkBoxRemoveTextBetweenCustomTags;
        private System.Windows.Forms.CheckBox checkBoxRemoveTextBetweenQuestionMarks;
        private System.Windows.Forms.CheckBox checkBoxRemoveTextBetweenParentheses;
        private System.Windows.Forms.CheckBox checkBoxRemoveTextBetweenBrackets;
        private System.Windows.Forms.CheckBox checkBoxRemoveTextBetweenSquares;
        private System.Windows.Forms.Label labelAnd;
        private System.Windows.Forms.CheckBox checkBoxRemoveTextBeforeColonOnlyUppercase;
        private System.Windows.Forms.CheckBox checkBoxOnlyIfInSeparateLine;
        private System.Windows.Forms.ComboBox comboBoxCustomStart;
        private System.Windows.Forms.ComboBox comboBoxCustomEnd;
        private System.Windows.Forms.ComboBox comboBoxRemoveIfTextContains;
        private System.Windows.Forms.CheckBox checkBoxRemoveWhereContains;
        private System.Windows.Forms.Label labelRemoveTextBetween;
        private System.Windows.Forms.CheckBox checkBoxRemoveInterjections;
        private System.Windows.Forms.Button buttonEditInterjections;
        private System.Windows.Forms.CheckBox checkBoxColonSeparateLine;
        private System.Windows.Forms.CheckBox checkBoxRemoveIfAllUppercase;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSelAll;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemInvertSel;
        private System.Windows.Forms.Button buttonApply;
        private System.Windows.Forms.CheckBox checkBoxInterjectionOnlySeparateLine;
        private System.Windows.Forms.TextBox textBoxAfterText;
        private System.Windows.Forms.Label labelText;
    }
}