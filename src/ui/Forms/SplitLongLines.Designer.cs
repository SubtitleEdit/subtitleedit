namespace Nikse.SubtitleEdit.Forms
{
    partial class SplitLongLines
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
            this.groupBoxLinesFound = new System.Windows.Forms.GroupBox();
            this.listViewFixes = new System.Windows.Forms.ListView();
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemSelectAll = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemInverseSelection = new System.Windows.Forms.ToolStripMenuItem();
            this.numericUpDownSingleLineMaxCharacters = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelLineMaxLength = new System.Windows.Forms.Label();
            this.labelSingleLineMaxLength = new System.Windows.Forms.Label();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.numericUpDownLineMaxCharacters = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelMaxSingleLineLengthIs = new System.Windows.Forms.Label();
            this.labelMaxLineLengthIs = new System.Windows.Forms.Label();
            this.comboBoxLineContinuationBegin = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelLineContinuationBeginEnd = new System.Windows.Forms.Label();
            this.comboBoxLineContinuationEnd = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.SubtitleListview1 = new Nikse.SubtitleEdit.Controls.SubtitleListView();
            this.checkBoxSplitAtLineBreaks = new System.Windows.Forms.CheckBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.groupBoxLinesFound.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxLinesFound
            // 
            this.groupBoxLinesFound.Controls.Add(this.listViewFixes);
            this.groupBoxLinesFound.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxLinesFound.Location = new System.Drawing.Point(0, 0);
            this.groupBoxLinesFound.Name = "groupBoxLinesFound";
            this.groupBoxLinesFound.Size = new System.Drawing.Size(750, 232);
            this.groupBoxLinesFound.TabIndex = 4;
            this.groupBoxLinesFound.TabStop = false;
            this.groupBoxLinesFound.Text = "Lines that will be split";
            // 
            // listViewFixes
            // 
            this.listViewFixes.CheckBoxes = true;
            this.listViewFixes.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader7});
            this.listViewFixes.ContextMenuStrip = this.contextMenuStrip1;
            this.listViewFixes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewFixes.FullRowSelect = true;
            this.listViewFixes.HideSelection = false;
            this.listViewFixes.Location = new System.Drawing.Point(3, 16);
            this.listViewFixes.Name = "listViewFixes";
            this.listViewFixes.Size = new System.Drawing.Size(744, 213);
            this.listViewFixes.TabIndex = 0;
            this.listViewFixes.UseCompatibleStateImageBehavior = false;
            this.listViewFixes.View = System.Windows.Forms.View.Details;
            this.listViewFixes.SelectedIndexChanged += new System.EventHandler(this.listViewFixes_SelectedIndexChanged);
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Apply";
            this.columnHeader4.Width = 45;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Line#";
            this.columnHeader5.Width = 122;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "New text";
            this.columnHeader7.Width = 500;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemSelectAll,
            this.toolStripMenuItemInverseSelection});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(162, 48);
            // 
            // toolStripMenuItemSelectAll
            // 
            this.toolStripMenuItemSelectAll.Name = "toolStripMenuItemSelectAll";
            this.toolStripMenuItemSelectAll.Size = new System.Drawing.Size(161, 22);
            this.toolStripMenuItemSelectAll.Text = "Select all";
            this.toolStripMenuItemSelectAll.Click += new System.EventHandler(this.toolStripMenuItemSelectAll_Click);
            // 
            // toolStripMenuItemInverseSelection
            // 
            this.toolStripMenuItemInverseSelection.Name = "toolStripMenuItemInverseSelection";
            this.toolStripMenuItemInverseSelection.Size = new System.Drawing.Size(161, 22);
            this.toolStripMenuItemInverseSelection.Text = "Inverse selection";
            this.toolStripMenuItemInverseSelection.Click += new System.EventHandler(this.toolStripMenuItemInverseSelection_Click);
            // 
            // numericUpDownSingleLineMaxCharacters
            // 
            this.numericUpDownSingleLineMaxCharacters.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownSingleLineMaxCharacters.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownSingleLineMaxCharacters.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownSingleLineMaxCharacters.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownSingleLineMaxCharacters.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownSingleLineMaxCharacters.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownSingleLineMaxCharacters.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownSingleLineMaxCharacters.DecimalPlaces = 0;
            this.numericUpDownSingleLineMaxCharacters.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownSingleLineMaxCharacters.Location = new System.Drawing.Point(28, 33);
            this.numericUpDownSingleLineMaxCharacters.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.numericUpDownSingleLineMaxCharacters.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDownSingleLineMaxCharacters.Name = "numericUpDownSingleLineMaxCharacters";
            this.numericUpDownSingleLineMaxCharacters.Size = new System.Drawing.Size(64, 23);
            this.numericUpDownSingleLineMaxCharacters.TabIndex = 0;
            this.numericUpDownSingleLineMaxCharacters.TabStop = false;
            this.numericUpDownSingleLineMaxCharacters.ThousandsSeparator = false;
            this.numericUpDownSingleLineMaxCharacters.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numericUpDownSingleLineMaxCharacters.ValueChanged += new System.EventHandler(this.NumericUpDownMaxCharactersValueChanged);
            // 
            // labelLineMaxLength
            // 
            this.labelLineMaxLength.AutoSize = true;
            this.labelLineMaxLength.Location = new System.Drawing.Point(240, 15);
            this.labelLineMaxLength.Name = "labelLineMaxLength";
            this.labelLineMaxLength.Size = new System.Drawing.Size(105, 13);
            this.labelLineMaxLength.TabIndex = 43;
            this.labelLineMaxLength.Text = "Line maximum length";
            // 
            // labelSingleLineMaxLength
            // 
            this.labelSingleLineMaxLength.AutoSize = true;
            this.labelSingleLineMaxLength.Location = new System.Drawing.Point(25, 15);
            this.labelSingleLineMaxLength.Name = "labelSingleLineMaxLength";
            this.labelSingleLineMaxLength.Size = new System.Drawing.Size(133, 13);
            this.labelSingleLineMaxLength.TabIndex = 42;
            this.labelSingleLineMaxLength.Text = "Single line maximum length";
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(606, 581);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 6;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(687, 581);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 7;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // numericUpDownLineMaxCharacters
            // 
            this.numericUpDownLineMaxCharacters.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownLineMaxCharacters.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownLineMaxCharacters.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownLineMaxCharacters.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownLineMaxCharacters.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownLineMaxCharacters.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownLineMaxCharacters.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownLineMaxCharacters.DecimalPlaces = 0;
            this.numericUpDownLineMaxCharacters.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownLineMaxCharacters.Location = new System.Drawing.Point(243, 33);
            this.numericUpDownLineMaxCharacters.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.numericUpDownLineMaxCharacters.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDownLineMaxCharacters.Name = "numericUpDownLineMaxCharacters";
            this.numericUpDownLineMaxCharacters.Size = new System.Drawing.Size(64, 23);
            this.numericUpDownLineMaxCharacters.TabIndex = 1;
            this.numericUpDownLineMaxCharacters.TabStop = false;
            this.numericUpDownLineMaxCharacters.ThousandsSeparator = false;
            this.numericUpDownLineMaxCharacters.Value = new decimal(new int[] {
            90,
            0,
            0,
            0});
            this.numericUpDownLineMaxCharacters.ValueChanged += new System.EventHandler(this.NumericUpDownMaxCharactersValueChanged);
            // 
            // labelMaxSingleLineLengthIs
            // 
            this.labelMaxSingleLineLengthIs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelMaxSingleLineLengthIs.AutoSize = true;
            this.labelMaxSingleLineLengthIs.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelMaxSingleLineLengthIs.Location = new System.Drawing.Point(9, 576);
            this.labelMaxSingleLineLengthIs.Name = "labelMaxSingleLineLengthIs";
            this.labelMaxSingleLineLengthIs.Size = new System.Drawing.Size(133, 13);
            this.labelMaxSingleLineLengthIs.TabIndex = 45;
            this.labelMaxSingleLineLengthIs.Text = "Single line maximum length";
            this.labelMaxSingleLineLengthIs.Click += new System.EventHandler(this.labelMaxSingleLineLengthIs_Click);
            // 
            // labelMaxLineLengthIs
            // 
            this.labelMaxLineLengthIs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelMaxLineLengthIs.AutoSize = true;
            this.labelMaxLineLengthIs.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelMaxLineLengthIs.Location = new System.Drawing.Point(9, 594);
            this.labelMaxLineLengthIs.Name = "labelMaxLineLengthIs";
            this.labelMaxLineLengthIs.Size = new System.Drawing.Size(83, 13);
            this.labelMaxLineLengthIs.TabIndex = 46;
            this.labelMaxLineLengthIs.Text = "Maximum length";
            this.labelMaxLineLengthIs.Click += new System.EventHandler(this.labelMaxLineLengthIs_Click);
            // 
            // comboBoxLineContinuationBegin
            // 
            this.comboBoxLineContinuationBegin.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxLineContinuationBegin.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxLineContinuationBegin.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxLineContinuationBegin.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxLineContinuationBegin.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxLineContinuationBegin.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxLineContinuationBegin.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxLineContinuationBegin.DropDownHeight = 400;
            this.comboBoxLineContinuationBegin.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.comboBoxLineContinuationBegin.DropDownWidth = 80;
            this.comboBoxLineContinuationBegin.FormattingEnabled = true;
            this.comboBoxLineContinuationBegin.Items.AddRange(new object[] {
            "",
            "- ",
            "..."});
            this.comboBoxLineContinuationBegin.Location = new System.Drawing.Point(478, 33);
            this.comboBoxLineContinuationBegin.MaxLength = 32767;
            this.comboBoxLineContinuationBegin.Name = "comboBoxLineContinuationBegin";
            this.comboBoxLineContinuationBegin.SelectedIndex = -1;
            this.comboBoxLineContinuationBegin.SelectedItem = null;
            this.comboBoxLineContinuationBegin.SelectedText = "";
            this.comboBoxLineContinuationBegin.Size = new System.Drawing.Size(80, 23);
            this.comboBoxLineContinuationBegin.TabIndex = 2;
            this.comboBoxLineContinuationBegin.TabStop = false;
            this.comboBoxLineContinuationBegin.UsePopupWindow = false;
            this.comboBoxLineContinuationBegin.SelectedIndexChanged += new System.EventHandler(this.ContinuationBeginEndChanged);
            // 
            // labelLineContinuationBeginEnd
            // 
            this.labelLineContinuationBeginEnd.AutoSize = true;
            this.labelLineContinuationBeginEnd.Location = new System.Drawing.Point(475, 15);
            this.labelLineContinuationBeginEnd.Name = "labelLineContinuationBeginEnd";
            this.labelLineContinuationBeginEnd.Size = new System.Drawing.Size(173, 13);
            this.labelLineContinuationBeginEnd.TabIndex = 48;
            this.labelLineContinuationBeginEnd.Text = "Line continuation begin/end strings";
            // 
            // comboBoxLineContinuationEnd
            // 
            this.comboBoxLineContinuationEnd.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxLineContinuationEnd.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxLineContinuationEnd.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxLineContinuationEnd.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxLineContinuationEnd.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxLineContinuationEnd.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxLineContinuationEnd.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxLineContinuationEnd.DropDownHeight = 400;
            this.comboBoxLineContinuationEnd.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.comboBoxLineContinuationEnd.DropDownWidth = 80;
            this.comboBoxLineContinuationEnd.FormattingEnabled = true;
            this.comboBoxLineContinuationEnd.Items.AddRange(new object[] {
            "",
            " -",
            "..."});
            this.comboBoxLineContinuationEnd.Location = new System.Drawing.Point(564, 33);
            this.comboBoxLineContinuationEnd.MaxLength = 32767;
            this.comboBoxLineContinuationEnd.Name = "comboBoxLineContinuationEnd";
            this.comboBoxLineContinuationEnd.SelectedIndex = -1;
            this.comboBoxLineContinuationEnd.SelectedItem = null;
            this.comboBoxLineContinuationEnd.SelectedText = "";
            this.comboBoxLineContinuationEnd.Size = new System.Drawing.Size(80, 23);
            this.comboBoxLineContinuationEnd.TabIndex = 3;
            this.comboBoxLineContinuationEnd.TabStop = false;
            this.comboBoxLineContinuationEnd.UsePopupWindow = false;
            this.comboBoxLineContinuationEnd.SelectedIndexChanged += new System.EventHandler(this.ContinuationBeginEndChanged);
            // 
            // SubtitleListview1
            // 
            this.SubtitleListview1.AllowColumnReorder = true;
            this.SubtitleListview1.AllowDrop = true;
            this.SubtitleListview1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SubtitleListview1.FirstVisibleIndex = -1;
            this.SubtitleListview1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SubtitleListview1.FullRowSelect = true;
            this.SubtitleListview1.GridLines = true;
            this.SubtitleListview1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.SubtitleListview1.HideSelection = false;
            this.SubtitleListview1.Location = new System.Drawing.Point(0, 0);
            this.SubtitleListview1.Name = "SubtitleListview1";
            this.SubtitleListview1.OwnerDraw = true;
            this.SubtitleListview1.Size = new System.Drawing.Size(750, 229);
            this.SubtitleListview1.SubtitleFontBold = false;
            this.SubtitleListview1.SubtitleFontName = "Tahoma";
            this.SubtitleListview1.SubtitleFontSize = 8;
            this.SubtitleListview1.TabIndex = 5;
            this.SubtitleListview1.UseCompatibleStateImageBehavior = false;
            this.SubtitleListview1.UseSyntaxColoring = true;
            this.SubtitleListview1.View = System.Windows.Forms.View.Details;
            // 
            // checkBoxSplitAtLineBreaks
            // 
            this.checkBoxSplitAtLineBreaks.AutoSize = true;
            this.checkBoxSplitAtLineBreaks.Location = new System.Drawing.Point(28, 66);
            this.checkBoxSplitAtLineBreaks.Name = "checkBoxSplitAtLineBreaks";
            this.checkBoxSplitAtLineBreaks.Size = new System.Drawing.Size(112, 17);
            this.checkBoxSplitAtLineBreaks.TabIndex = 49;
            this.checkBoxSplitAtLineBreaks.Text = "Split at line breaks";
            this.checkBoxSplitAtLineBreaks.UseVisualStyleBackColor = true;
            this.checkBoxSplitAtLineBreaks.CheckedChanged += new System.EventHandler(this.checkBoxSplitAtLineBreaks_CheckedChanged);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(12, 93);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.groupBoxLinesFound);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.SubtitleListview1);
            this.splitContainer1.Size = new System.Drawing.Size(750, 465);
            this.splitContainer1.SplitterDistance = 232;
            this.splitContainer1.TabIndex = 50;
            // 
            // SplitLongLines
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(774, 614);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.checkBoxSplitAtLineBreaks);
            this.Controls.Add(this.comboBoxLineContinuationEnd);
            this.Controls.Add(this.labelLineContinuationBeginEnd);
            this.Controls.Add(this.comboBoxLineContinuationBegin);
            this.Controls.Add(this.labelMaxLineLengthIs);
            this.Controls.Add(this.labelMaxSingleLineLengthIs);
            this.Controls.Add(this.numericUpDownLineMaxCharacters);
            this.Controls.Add(this.numericUpDownSingleLineMaxCharacters);
            this.Controls.Add(this.labelLineMaxLength);
            this.Controls.Add(this.labelSingleLineMaxLength);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(780, 500);
            this.Name = "SplitLongLines";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Split long lines";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SplitLongLines_FormClosing);
            this.Shown += new System.EventHandler(this.SplitLongLines_Shown);
            this.ResizeEnd += new System.EventHandler(this.SplitLongLines_ResizeEnd);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SplitLongLines_KeyDown);
            this.groupBoxLinesFound.ResumeLayout(false);
            this.contextMenuStrip1.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxLinesFound;
        private System.Windows.Forms.ListView listViewFixes;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownSingleLineMaxCharacters;
        private System.Windows.Forms.Label labelLineMaxLength;
        private System.Windows.Forms.Label labelSingleLineMaxLength;
        private Controls.SubtitleListView SubtitleListview1;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownLineMaxCharacters;
        private System.Windows.Forms.Label labelMaxSingleLineLengthIs;
        private System.Windows.Forms.Label labelMaxLineLengthIs;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxLineContinuationBegin;
        private System.Windows.Forms.Label labelLineContinuationBeginEnd;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxLineContinuationEnd;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSelectAll;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemInverseSelection;
        private System.Windows.Forms.CheckBox checkBoxSplitAtLineBreaks;
        private System.Windows.Forms.SplitContainer splitContainer1;
    }
}