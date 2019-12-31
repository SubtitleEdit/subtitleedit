namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class NetflixFixErrors
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
            this.groupBoxRules = new System.Windows.Forms.GroupBox();
            this.checkBoxWhiteSpace = new System.Windows.Forms.CheckBox();
            this.checkBoxMaxLineLength = new System.Windows.Forms.CheckBox();
            this.checkBoxTtmlFrameRate = new System.Windows.Forms.CheckBox();
            this.checkBoxNoItalics = new System.Windows.Forms.CheckBox();
            this.checkBoxGapMinTwoFrames = new System.Windows.Forms.CheckBox();
            this.checkBoxCheckValidGlyphs = new System.Windows.Forms.CheckBox();
            this.checkBox17CharsPerSecond = new System.Windows.Forms.CheckBox();
            this.comboBoxLanguage = new System.Windows.Forms.ComboBox();
            this.labelLanguage = new System.Windows.Forms.Label();
            this.checkBoxWriteOutOneToTen = new System.Windows.Forms.CheckBox();
            this.checkBoxSpellOutStartNumbers = new System.Windows.Forms.CheckBox();
            this.checkBoxSquareBracketForHi = new System.Windows.Forms.CheckBox();
            this.checkBoxDialogHypenNoSpace = new System.Windows.Forms.CheckBox();
            this.checkBoxTwoLinesMax = new System.Windows.Forms.CheckBox();
            this.checkBoxMinDuration = new System.Windows.Forms.CheckBox();
            this.checkBoxMaxDuration = new System.Windows.Forms.CheckBox();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.labelTotal = new System.Windows.Forms.Label();
            this.listViewFixes = new System.Windows.Forms.ListView();
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.linkLabelOpenReportFolder = new System.Windows.Forms.LinkLabel();
            this.groupBoxRules.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxRules
            // 
            this.groupBoxRules.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxRules.Controls.Add(this.checkBoxWhiteSpace);
            this.groupBoxRules.Controls.Add(this.checkBoxMaxLineLength);
            this.groupBoxRules.Controls.Add(this.checkBoxTtmlFrameRate);
            this.groupBoxRules.Controls.Add(this.checkBoxNoItalics);
            this.groupBoxRules.Controls.Add(this.checkBoxGapMinTwoFrames);
            this.groupBoxRules.Controls.Add(this.checkBoxCheckValidGlyphs);
            this.groupBoxRules.Controls.Add(this.checkBox17CharsPerSecond);
            this.groupBoxRules.Controls.Add(this.comboBoxLanguage);
            this.groupBoxRules.Controls.Add(this.labelLanguage);
            this.groupBoxRules.Controls.Add(this.checkBoxWriteOutOneToTen);
            this.groupBoxRules.Controls.Add(this.checkBoxSpellOutStartNumbers);
            this.groupBoxRules.Controls.Add(this.checkBoxSquareBracketForHi);
            this.groupBoxRules.Controls.Add(this.checkBoxDialogHypenNoSpace);
            this.groupBoxRules.Controls.Add(this.checkBoxTwoLinesMax);
            this.groupBoxRules.Controls.Add(this.checkBoxMinDuration);
            this.groupBoxRules.Controls.Add(this.checkBoxMaxDuration);
            this.groupBoxRules.Location = new System.Drawing.Point(12, 12);
            this.groupBoxRules.Name = "groupBoxRules";
            this.groupBoxRules.Size = new System.Drawing.Size(1016, 248);
            this.groupBoxRules.TabIndex = 106;
            this.groupBoxRules.TabStop = false;
            this.groupBoxRules.Text = "Rules";
            // 
            // checkBoxWhiteSpace
            // 
            this.checkBoxWhiteSpace.AutoSize = true;
            this.checkBoxWhiteSpace.Checked = true;
            this.checkBoxWhiteSpace.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxWhiteSpace.Location = new System.Drawing.Point(19, 193);
            this.checkBoxWhiteSpace.Name = "checkBoxWhiteSpace";
            this.checkBoxWhiteSpace.Size = new System.Drawing.Size(86, 17);
            this.checkBoxWhiteSpace.TabIndex = 36;
            this.checkBoxWhiteSpace.Text = "White space";
            this.checkBoxWhiteSpace.UseVisualStyleBackColor = true;
            this.checkBoxWhiteSpace.CheckedChanged += new System.EventHandler(this.RuleCheckedChanged);
            // 
            // checkBoxMaxLineLength
            // 
            this.checkBoxMaxLineLength.AutoSize = true;
            this.checkBoxMaxLineLength.Checked = true;
            this.checkBoxMaxLineLength.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxMaxLineLength.Location = new System.Drawing.Point(19, 170);
            this.checkBoxMaxLineLength.Name = "checkBoxMaxLineLength";
            this.checkBoxMaxLineLength.Size = new System.Drawing.Size(130, 17);
            this.checkBoxMaxLineLength.TabIndex = 35;
            this.checkBoxMaxLineLength.Text = "Check max line length";
            this.checkBoxMaxLineLength.UseVisualStyleBackColor = true;
            this.checkBoxMaxLineLength.CheckedChanged += new System.EventHandler(this.RuleCheckedChanged);
            // 
            // checkBoxTtmlFrameRate
            // 
            this.checkBoxTtmlFrameRate.AutoSize = true;
            this.checkBoxTtmlFrameRate.Checked = true;
            this.checkBoxTtmlFrameRate.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxTtmlFrameRate.Location = new System.Drawing.Point(409, 193);
            this.checkBoxTtmlFrameRate.Name = "checkBoxTtmlFrameRate";
            this.checkBoxTtmlFrameRate.Size = new System.Drawing.Size(154, 17);
            this.checkBoxTtmlFrameRate.TabIndex = 34;
            this.checkBoxTtmlFrameRate.Text = "Check frame rate for TTML";
            this.checkBoxTtmlFrameRate.UseVisualStyleBackColor = true;
            this.checkBoxTtmlFrameRate.CheckedChanged += new System.EventHandler(this.RuleCheckedChanged);
            // 
            // checkBoxNoItalics
            // 
            this.checkBoxNoItalics.AutoSize = true;
            this.checkBoxNoItalics.Checked = true;
            this.checkBoxNoItalics.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxNoItalics.Location = new System.Drawing.Point(409, 170);
            this.checkBoxNoItalics.Name = "checkBoxNoItalics";
            this.checkBoxNoItalics.Size = new System.Drawing.Size(109, 17);
            this.checkBoxNoItalics.TabIndex = 33;
            this.checkBoxNoItalics.Text = "Do not allow italic";
            this.checkBoxNoItalics.UseVisualStyleBackColor = true;
            this.checkBoxNoItalics.CheckedChanged += new System.EventHandler(this.RuleCheckedChanged);
            // 
            // checkBoxGapMinTwoFrames
            // 
            this.checkBoxGapMinTwoFrames.AutoSize = true;
            this.checkBoxGapMinTwoFrames.Checked = true;
            this.checkBoxGapMinTwoFrames.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxGapMinTwoFrames.Location = new System.Drawing.Point(19, 124);
            this.checkBoxGapMinTwoFrames.Name = "checkBoxGapMinTwoFrames";
            this.checkBoxGapMinTwoFrames.Size = new System.Drawing.Size(168, 17);
            this.checkBoxGapMinTwoFrames.TabIndex = 6;
            this.checkBoxGapMinTwoFrames.Text = "Frame gap:  minimum 2 frames";
            this.checkBoxGapMinTwoFrames.UseVisualStyleBackColor = true;
            this.checkBoxGapMinTwoFrames.CheckedChanged += new System.EventHandler(this.RuleCheckedChanged);
            // 
            // checkBoxCheckValidGlyphs
            // 
            this.checkBoxCheckValidGlyphs.AutoSize = true;
            this.checkBoxCheckValidGlyphs.Checked = true;
            this.checkBoxCheckValidGlyphs.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxCheckValidGlyphs.Location = new System.Drawing.Point(409, 147);
            this.checkBoxCheckValidGlyphs.Name = "checkBoxCheckValidGlyphs";
            this.checkBoxCheckValidGlyphs.Size = new System.Drawing.Size(330, 17);
            this.checkBoxCheckValidGlyphs.TabIndex = 21;
            this.checkBoxCheckValidGlyphs.Text = "Only text/characters included in the Netflix Glyph List (version 2) ";
            this.checkBoxCheckValidGlyphs.UseVisualStyleBackColor = true;
            this.checkBoxCheckValidGlyphs.CheckedChanged += new System.EventHandler(this.RuleCheckedChanged);
            // 
            // checkBox17CharsPerSecond
            // 
            this.checkBox17CharsPerSecond.AutoSize = true;
            this.checkBox17CharsPerSecond.Checked = true;
            this.checkBox17CharsPerSecond.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox17CharsPerSecond.Location = new System.Drawing.Point(19, 101);
            this.checkBox17CharsPerSecond.Name = "checkBox17CharsPerSecond";
            this.checkBox17CharsPerSecond.Size = new System.Drawing.Size(290, 17);
            this.checkBox17CharsPerSecond.TabIndex = 5;
            this.checkBox17CharsPerSecond.Text = "Maximum 17 characters per second (excl. white spaces)";
            this.checkBox17CharsPerSecond.UseVisualStyleBackColor = true;
            this.checkBox17CharsPerSecond.CheckedChanged += new System.EventHandler(this.RuleCheckedChanged);
            // 
            // comboBoxLanguage
            // 
            this.comboBoxLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLanguage.FormattingEnabled = true;
            this.comboBoxLanguage.Location = new System.Drawing.Point(77, 14);
            this.comboBoxLanguage.Name = "comboBoxLanguage";
            this.comboBoxLanguage.Size = new System.Drawing.Size(196, 21);
            this.comboBoxLanguage.TabIndex = 30;
            this.comboBoxLanguage.SelectedIndexChanged += new System.EventHandler(this.comboBoxLanguage_SelectedIndexChanged);
            // 
            // labelLanguage
            // 
            this.labelLanguage.AutoSize = true;
            this.labelLanguage.Location = new System.Drawing.Point(16, 17);
            this.labelLanguage.Name = "labelLanguage";
            this.labelLanguage.Size = new System.Drawing.Size(55, 13);
            this.labelLanguage.TabIndex = 8;
            this.labelLanguage.Text = "Language";
            // 
            // checkBoxWriteOutOneToTen
            // 
            this.checkBoxWriteOutOneToTen.AutoSize = true;
            this.checkBoxWriteOutOneToTen.Checked = true;
            this.checkBoxWriteOutOneToTen.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxWriteOutOneToTen.Location = new System.Drawing.Point(409, 124);
            this.checkBoxWriteOutOneToTen.Name = "checkBoxWriteOutOneToTen";
            this.checkBoxWriteOutOneToTen.Size = new System.Drawing.Size(335, 17);
            this.checkBoxWriteOutOneToTen.TabIndex = 17;
            this.checkBoxWriteOutOneToTen.Text = "From 1 to 10, numbers should be written out: One, two, three, etc.";
            this.checkBoxWriteOutOneToTen.UseVisualStyleBackColor = true;
            this.checkBoxWriteOutOneToTen.CheckedChanged += new System.EventHandler(this.RuleCheckedChanged);
            // 
            // checkBoxSpellOutStartNumbers
            // 
            this.checkBoxSpellOutStartNumbers.AutoSize = true;
            this.checkBoxSpellOutStartNumbers.Checked = true;
            this.checkBoxSpellOutStartNumbers.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxSpellOutStartNumbers.Location = new System.Drawing.Point(409, 101);
            this.checkBoxSpellOutStartNumbers.Name = "checkBoxSpellOutStartNumbers";
            this.checkBoxSpellOutStartNumbers.Size = new System.Drawing.Size(341, 17);
            this.checkBoxSpellOutStartNumbers.TabIndex = 15;
            this.checkBoxSpellOutStartNumbers.Text = "When a number begins a sentence, it should always be spelled out";
            this.checkBoxSpellOutStartNumbers.UseVisualStyleBackColor = true;
            this.checkBoxSpellOutStartNumbers.CheckedChanged += new System.EventHandler(this.RuleCheckedChanged);
            // 
            // checkBoxSquareBracketForHi
            // 
            this.checkBoxSquareBracketForHi.AutoSize = true;
            this.checkBoxSquareBracketForHi.Checked = true;
            this.checkBoxSquareBracketForHi.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxSquareBracketForHi.Location = new System.Drawing.Point(409, 78);
            this.checkBoxSquareBracketForHi.Name = "checkBoxSquareBracketForHi";
            this.checkBoxSquareBracketForHi.Size = new System.Drawing.Size(286, 17);
            this.checkBoxSquareBracketForHi.TabIndex = 11;
            this.checkBoxSquareBracketForHi.Text = "Use brackets[] to enclose speaker IDs or sound effects";
            this.checkBoxSquareBracketForHi.UseVisualStyleBackColor = true;
            this.checkBoxSquareBracketForHi.CheckedChanged += new System.EventHandler(this.RuleCheckedChanged);
            // 
            // checkBoxDialogHypenNoSpace
            // 
            this.checkBoxDialogHypenNoSpace.AutoSize = true;
            this.checkBoxDialogHypenNoSpace.Checked = true;
            this.checkBoxDialogHypenNoSpace.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxDialogHypenNoSpace.Location = new System.Drawing.Point(409, 54);
            this.checkBoxDialogHypenNoSpace.Name = "checkBoxDialogHypenNoSpace";
            this.checkBoxDialogHypenNoSpace.Size = new System.Drawing.Size(249, 17);
            this.checkBoxDialogHypenNoSpace.TabIndex = 9;
            this.checkBoxDialogHypenNoSpace.Text = " Dual Speakers: Use a hyphen without a space";
            this.checkBoxDialogHypenNoSpace.UseVisualStyleBackColor = true;
            this.checkBoxDialogHypenNoSpace.CheckedChanged += new System.EventHandler(this.RuleCheckedChanged);
            // 
            // checkBoxTwoLinesMax
            // 
            this.checkBoxTwoLinesMax.AutoSize = true;
            this.checkBoxTwoLinesMax.Checked = true;
            this.checkBoxTwoLinesMax.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxTwoLinesMax.Location = new System.Drawing.Point(19, 147);
            this.checkBoxTwoLinesMax.Name = "checkBoxTwoLinesMax";
            this.checkBoxTwoLinesMax.Size = new System.Drawing.Size(117, 17);
            this.checkBoxTwoLinesMax.TabIndex = 7;
            this.checkBoxTwoLinesMax.Text = "Two lines maximum";
            this.checkBoxTwoLinesMax.UseVisualStyleBackColor = true;
            this.checkBoxTwoLinesMax.CheckedChanged += new System.EventHandler(this.RuleCheckedChanged);
            // 
            // checkBoxMinDuration
            // 
            this.checkBoxMinDuration.AutoSize = true;
            this.checkBoxMinDuration.Checked = true;
            this.checkBoxMinDuration.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxMinDuration.Location = new System.Drawing.Point(19, 54);
            this.checkBoxMinDuration.Name = "checkBoxMinDuration";
            this.checkBoxMinDuration.Size = new System.Drawing.Size(212, 17);
            this.checkBoxMinDuration.TabIndex = 1;
            this.checkBoxMinDuration.Text = "Minimum duration: 5/6 second (833 ms)";
            this.checkBoxMinDuration.UseVisualStyleBackColor = true;
            this.checkBoxMinDuration.CheckedChanged += new System.EventHandler(this.RuleCheckedChanged);
            // 
            // checkBoxMaxDuration
            // 
            this.checkBoxMaxDuration.AutoSize = true;
            this.checkBoxMaxDuration.Checked = true;
            this.checkBoxMaxDuration.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxMaxDuration.Location = new System.Drawing.Point(19, 78);
            this.checkBoxMaxDuration.Name = "checkBoxMaxDuration";
            this.checkBoxMaxDuration.Size = new System.Drawing.Size(250, 17);
            this.checkBoxMaxDuration.TabIndex = 3;
            this.checkBoxMaxDuration.Text = "Maximum duration: 7 seconds per subtitle event";
            this.checkBoxMaxDuration.UseVisualStyleBackColor = true;
            this.checkBoxMaxDuration.CheckedChanged += new System.EventHandler(this.RuleCheckedChanged);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(953, 582);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 113;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(872, 582);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 112;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // labelTotal
            // 
            this.labelTotal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelTotal.AutoSize = true;
            this.labelTotal.Location = new System.Drawing.Point(12, 587);
            this.labelTotal.Name = "labelTotal";
            this.labelTotal.Size = new System.Drawing.Size(34, 13);
            this.labelTotal.TabIndex = 115;
            this.labelTotal.Text = "Total:";
            // 
            // listViewFixes
            // 
            this.listViewFixes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewFixes.CheckBoxes = true;
            this.listViewFixes.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6,
            this.columnHeader7,
            this.columnHeader8});
            this.listViewFixes.FullRowSelect = true;
            this.listViewFixes.GridLines = true;
            this.listViewFixes.HideSelection = false;
            this.listViewFixes.Location = new System.Drawing.Point(12, 277);
            this.listViewFixes.Name = "listViewFixes";
            this.listViewFixes.Size = new System.Drawing.Size(1016, 291);
            this.listViewFixes.TabIndex = 114;
            this.listViewFixes.UseCompatibleStateImageBehavior = false;
            this.listViewFixes.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Apply";
            this.columnHeader4.Width = 38;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Line#";
            this.columnHeader5.Width = 50;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "Function";
            this.columnHeader6.Width = 169;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "Before";
            this.columnHeader7.Width = 340;
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "After";
            this.columnHeader8.Width = 318;
            // 
            // linkLabelOpenReportFolder
            // 
            this.linkLabelOpenReportFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkLabelOpenReportFolder.AutoSize = true;
            this.linkLabelOpenReportFolder.Location = new System.Drawing.Point(106, 587);
            this.linkLabelOpenReportFolder.Name = "linkLabelOpenReportFolder";
            this.linkLabelOpenReportFolder.Size = new System.Drawing.Size(92, 13);
            this.linkLabelOpenReportFolder.TabIndex = 116;
            this.linkLabelOpenReportFolder.TabStop = true;
            this.linkLabelOpenReportFolder.Text = "Open report folder";
            this.linkLabelOpenReportFolder.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelOpenReportFolder_LinkClicked);
            // 
            // NetflixFixErrors
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1040, 617);
            this.Controls.Add(this.linkLabelOpenReportFolder);
            this.Controls.Add(this.labelTotal);
            this.Controls.Add(this.listViewFixes);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.groupBoxRules);
            this.MinimumSize = new System.Drawing.Size(928, 505);
            this.Name = "NetflixFixErrors";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "NetflixFixErrors";
            this.groupBoxRules.ResumeLayout(false);
            this.groupBoxRules.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxRules;
        private System.Windows.Forms.CheckBox checkBoxGapMinTwoFrames;
        private System.Windows.Forms.CheckBox checkBoxCheckValidGlyphs;
        private System.Windows.Forms.CheckBox checkBox17CharsPerSecond;
        private System.Windows.Forms.ComboBox comboBoxLanguage;
        private System.Windows.Forms.Label labelLanguage;
        private System.Windows.Forms.CheckBox checkBoxWriteOutOneToTen;
        private System.Windows.Forms.CheckBox checkBoxSpellOutStartNumbers;
        private System.Windows.Forms.CheckBox checkBoxSquareBracketForHi;
        private System.Windows.Forms.CheckBox checkBoxDialogHypenNoSpace;
        private System.Windows.Forms.CheckBox checkBoxTwoLinesMax;
        private System.Windows.Forms.CheckBox checkBoxMinDuration;
        private System.Windows.Forms.CheckBox checkBoxMaxDuration;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Label labelTotal;
        private System.Windows.Forms.ListView listViewFixes;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.CheckBox checkBoxNoItalics;
        private System.Windows.Forms.CheckBox checkBoxTtmlFrameRate;
        private System.Windows.Forms.CheckBox checkBoxMaxLineLength;
        private System.Windows.Forms.CheckBox checkBoxWhiteSpace;
        private System.Windows.Forms.LinkLabel linkLabelOpenReportFolder;
    }
}