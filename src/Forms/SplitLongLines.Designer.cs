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
            this.groupBoxLinesFound = new System.Windows.Forms.GroupBox();
            this.listViewFixes = new System.Windows.Forms.ListView();
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.numericUpDownSingleLineMaxCharacters = new System.Windows.Forms.NumericUpDown();
            this.labelLineMaxLength = new System.Windows.Forms.Label();
            this.labelSingleLineMaxLength = new System.Windows.Forms.Label();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.numericUpDownLineMaxCharacters = new System.Windows.Forms.NumericUpDown();
            this.labelMaxSingleLineLengthIs = new System.Windows.Forms.Label();
            this.labelMaxLineLengthIs = new System.Windows.Forms.Label();
            this.comboBoxLineContinuationBegin = new System.Windows.Forms.ComboBox();
            this.labelLineContinuationBeginEnd = new System.Windows.Forms.Label();
            this.comboBoxLineContinuationEnd = new System.Windows.Forms.ComboBox();
            this.SubtitleListview1 = new Nikse.SubtitleEdit.Controls.SubtitleListView();
            this.groupBoxLinesFound.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSingleLineMaxCharacters)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownLineMaxCharacters)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBoxLinesFound
            // 
            this.groupBoxLinesFound.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxLinesFound.Controls.Add(this.listViewFixes);
            this.groupBoxLinesFound.Location = new System.Drawing.Point(12, 64);
            this.groupBoxLinesFound.Name = "groupBoxLinesFound";
            this.groupBoxLinesFound.Size = new System.Drawing.Size(750, 200);
            this.groupBoxLinesFound.TabIndex = 4;
            this.groupBoxLinesFound.TabStop = false;
            this.groupBoxLinesFound.Text = "Lines that will be split";
            // 
            // listViewFixes
            // 
            this.listViewFixes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewFixes.CheckBoxes = true;
            this.listViewFixes.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader7});
            this.listViewFixes.FullRowSelect = true;
            this.listViewFixes.HideSelection = false;
            this.listViewFixes.Location = new System.Drawing.Point(6, 23);
            this.listViewFixes.Name = "listViewFixes";
            this.listViewFixes.Size = new System.Drawing.Size(738, 171);
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
            // numericUpDownSingleLineMaxCharacters
            // 
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
            this.numericUpDownSingleLineMaxCharacters.Size = new System.Drawing.Size(64, 20);
            this.numericUpDownSingleLineMaxCharacters.TabIndex = 0;
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
            this.labelLineMaxLength.Location = new System.Drawing.Point(240, 16);
            this.labelLineMaxLength.Name = "labelLineMaxLength";
            this.labelLineMaxLength.Size = new System.Drawing.Size(105, 13);
            this.labelLineMaxLength.TabIndex = 43;
            this.labelLineMaxLength.Text = "Line maximum length";
            // 
            // labelSingleLineMaxLength
            // 
            this.labelSingleLineMaxLength.AutoSize = true;
            this.labelSingleLineMaxLength.Location = new System.Drawing.Point(25, 16);
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
            this.numericUpDownLineMaxCharacters.Size = new System.Drawing.Size(64, 20);
            this.numericUpDownLineMaxCharacters.TabIndex = 1;
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
            this.labelMaxSingleLineLengthIs.Location = new System.Drawing.Point(9, 579);
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
            this.labelMaxLineLengthIs.Location = new System.Drawing.Point(9, 597);
            this.labelMaxLineLengthIs.Name = "labelMaxLineLengthIs";
            this.labelMaxLineLengthIs.Size = new System.Drawing.Size(83, 13);
            this.labelMaxLineLengthIs.TabIndex = 46;
            this.labelMaxLineLengthIs.Text = "Maximum length";
            this.labelMaxLineLengthIs.Click += new System.EventHandler(this.labelMaxLineLengthIs_Click);
            // 
            // comboBoxLineContinuationBegin
            // 
            this.comboBoxLineContinuationBegin.FormattingEnabled = true;
            this.comboBoxLineContinuationBegin.Items.AddRange(new object[] {
            "",
            "- ",
            "..."});
            this.comboBoxLineContinuationBegin.Location = new System.Drawing.Point(478, 33);
            this.comboBoxLineContinuationBegin.Name = "comboBoxLineContinuationBegin";
            this.comboBoxLineContinuationBegin.Size = new System.Drawing.Size(80, 21);
            this.comboBoxLineContinuationBegin.TabIndex = 2;
            this.comboBoxLineContinuationBegin.SelectedIndexChanged += new System.EventHandler(this.ContinuationBeginEndChanged);
            // 
            // labelLineContinuationBeginEnd
            // 
            this.labelLineContinuationBeginEnd.AutoSize = true;
            this.labelLineContinuationBeginEnd.Location = new System.Drawing.Point(475, 16);
            this.labelLineContinuationBeginEnd.Name = "labelLineContinuationBeginEnd";
            this.labelLineContinuationBeginEnd.Size = new System.Drawing.Size(173, 13);
            this.labelLineContinuationBeginEnd.TabIndex = 48;
            this.labelLineContinuationBeginEnd.Text = "Line continuation begin/end strings";
            // 
            // comboBoxLineContinuationEnd
            // 
            this.comboBoxLineContinuationEnd.FormattingEnabled = true;
            this.comboBoxLineContinuationEnd.Items.AddRange(new object[] {
            "",
            " -",
            "..."});
            this.comboBoxLineContinuationEnd.Location = new System.Drawing.Point(564, 33);
            this.comboBoxLineContinuationEnd.Name = "comboBoxLineContinuationEnd";
            this.comboBoxLineContinuationEnd.Size = new System.Drawing.Size(80, 21);
            this.comboBoxLineContinuationEnd.TabIndex = 3;
            this.comboBoxLineContinuationEnd.SelectedIndexChanged += new System.EventHandler(this.ContinuationBeginEndChanged);
            // 
            // SubtitleListview1
            // 
            this.SubtitleListview1.AllowColumnReorder = true;
            this.SubtitleListview1.AllowDrop = true;
            this.SubtitleListview1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SubtitleListview1.FirstVisibleIndex = -1;
            this.SubtitleListview1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SubtitleListview1.FullRowSelect = true;
            this.SubtitleListview1.GridLines = true;
            this.SubtitleListview1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.SubtitleListview1.HideSelection = false;
            this.SubtitleListview1.Location = new System.Drawing.Point(12, 270);
            this.SubtitleListview1.Name = "SubtitleListview1";
            this.SubtitleListview1.OwnerDraw = true;
            this.SubtitleListview1.Size = new System.Drawing.Size(750, 301);
            this.SubtitleListview1.SubtitleFontBold = false;
            this.SubtitleListview1.SubtitleFontName = "Tahoma";
            this.SubtitleListview1.SubtitleFontSize = 8;
            this.SubtitleListview1.TabIndex = 5;
            this.SubtitleListview1.UseCompatibleStateImageBehavior = false;
            this.SubtitleListview1.UseSyntaxColoring = true;
            this.SubtitleListview1.View = System.Windows.Forms.View.Details;
            // 
            // SplitLongLines
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(774, 614);
            this.Controls.Add(this.comboBoxLineContinuationEnd);
            this.Controls.Add(this.labelLineContinuationBeginEnd);
            this.Controls.Add(this.comboBoxLineContinuationBegin);
            this.Controls.Add(this.labelMaxLineLengthIs);
            this.Controls.Add(this.labelMaxSingleLineLengthIs);
            this.Controls.Add(this.numericUpDownLineMaxCharacters);
            this.Controls.Add(this.groupBoxLinesFound);
            this.Controls.Add(this.numericUpDownSingleLineMaxCharacters);
            this.Controls.Add(this.labelLineMaxLength);
            this.Controls.Add(this.labelSingleLineMaxLength);
            this.Controls.Add(this.SubtitleListview1);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(780, 500);
            this.Name = "SplitLongLines";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Split long lines";
            this.Shown += new System.EventHandler(this.SplitLongLines_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SplitLongLines_KeyDown);
            this.groupBoxLinesFound.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSingleLineMaxCharacters)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownLineMaxCharacters)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxLinesFound;
        private System.Windows.Forms.ListView listViewFixes;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.NumericUpDown numericUpDownSingleLineMaxCharacters;
        private System.Windows.Forms.Label labelLineMaxLength;
        private System.Windows.Forms.Label labelSingleLineMaxLength;
        private Controls.SubtitleListView SubtitleListview1;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.NumericUpDown numericUpDownLineMaxCharacters;
        private System.Windows.Forms.Label labelMaxSingleLineLengthIs;
        private System.Windows.Forms.Label labelMaxLineLengthIs;
        private System.Windows.Forms.ComboBox comboBoxLineContinuationBegin;
        private System.Windows.Forms.Label labelLineContinuationBeginEnd;
        private System.Windows.Forms.ComboBox comboBoxLineContinuationEnd;
    }
}