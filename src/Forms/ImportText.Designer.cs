namespace Nikse.SubtitleEdit.Forms
{
    partial class ImportText
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
            this.buttonOpenText = new System.Windows.Forms.Button();
            this.groupBoxImportText = new System.Windows.Forms.GroupBox();
            this.textBoxText = new System.Windows.Forms.TextBox();
            this.groupBoxImportOptions = new System.Windows.Forms.GroupBox();
            this.checkBoxMergeShortLines = new System.Windows.Forms.CheckBox();
            this.groupBoxDuration = new System.Windows.Forms.GroupBox();
            this.numericUpDownDurationFixed = new System.Windows.Forms.NumericUpDown();
            this.radioButtonDurationFixed = new System.Windows.Forms.RadioButton();
            this.radioButtonDurationAuto = new System.Windows.Forms.RadioButton();
            this.buttonRefresh = new System.Windows.Forms.Button();
            this.numericUpDownGapBetweenLines = new System.Windows.Forms.NumericUpDown();
            this.labelGapBetweenSubtitles = new System.Windows.Forms.Label();
            this.checkBoxRemoveLinesWithoutLetters = new System.Windows.Forms.CheckBox();
            this.groupBoxSplitting = new System.Windows.Forms.GroupBox();
            this.radioButtonAutoSplit = new System.Windows.Forms.RadioButton();
            this.radioButtonLineMode = new System.Windows.Forms.RadioButton();
            this.checkBoxRemoveEmptyLines = new System.Windows.Forms.CheckBox();
            this.groupBoxImportResult = new System.Windows.Forms.GroupBox();
            this.SubtitleListview1 = new Nikse.SubtitleEdit.Controls.SubtitleListView();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.groupBoxImportText.SuspendLayout();
            this.groupBoxImportOptions.SuspendLayout();
            this.groupBoxDuration.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDurationFixed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownGapBetweenLines)).BeginInit();
            this.groupBoxSplitting.SuspendLayout();
            this.groupBoxImportResult.SuspendLayout();
            this.SuspendLayout();
            //
            // buttonOpenText
            //
            this.buttonOpenText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOpenText.Location = new System.Drawing.Point(358, 19);
            this.buttonOpenText.Name = "buttonOpenText";
            this.buttonOpenText.Size = new System.Drawing.Size(143, 21);
            this.buttonOpenText.TabIndex = 0;
            this.buttonOpenText.Text = "Open file...";
            this.buttonOpenText.UseVisualStyleBackColor = true;
            this.buttonOpenText.Click += new System.EventHandler(this.buttonOpenText_Click);
            //
            // groupBoxImportText
            //
            this.groupBoxImportText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxImportText.Controls.Add(this.textBoxText);
            this.groupBoxImportText.Controls.Add(this.buttonOpenText);
            this.groupBoxImportText.Location = new System.Drawing.Point(12, 12);
            this.groupBoxImportText.Name = "groupBoxImportText";
            this.groupBoxImportText.Size = new System.Drawing.Size(507, 326);
            this.groupBoxImportText.TabIndex = 13;
            this.groupBoxImportText.TabStop = false;
            this.groupBoxImportText.Text = "Import text";
            //
            // textBoxText
            //
            this.textBoxText.AllowDrop = true;
            this.textBoxText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxText.Location = new System.Drawing.Point(6, 48);
            this.textBoxText.MaxLength = 232767;
            this.textBoxText.Multiline = true;
            this.textBoxText.Name = "textBoxText";
            this.textBoxText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxText.Size = new System.Drawing.Size(495, 272);
            this.textBoxText.TabIndex = 2;
            this.textBoxText.TextChanged += new System.EventHandler(this.textBoxText_TextChanged);
            this.textBoxText.DragDrop += new System.Windows.Forms.DragEventHandler(this.textBoxText_DragDrop);
            this.textBoxText.DragEnter += new System.Windows.Forms.DragEventHandler(this.textBoxText_DragEnter);
            //
            // groupBoxImportOptions
            //
            this.groupBoxImportOptions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxImportOptions.Controls.Add(this.checkBoxMergeShortLines);
            this.groupBoxImportOptions.Controls.Add(this.groupBoxDuration);
            this.groupBoxImportOptions.Controls.Add(this.buttonRefresh);
            this.groupBoxImportOptions.Controls.Add(this.numericUpDownGapBetweenLines);
            this.groupBoxImportOptions.Controls.Add(this.labelGapBetweenSubtitles);
            this.groupBoxImportOptions.Controls.Add(this.checkBoxRemoveLinesWithoutLetters);
            this.groupBoxImportOptions.Controls.Add(this.groupBoxSplitting);
            this.groupBoxImportOptions.Controls.Add(this.checkBoxRemoveEmptyLines);
            this.groupBoxImportOptions.Location = new System.Drawing.Point(525, 12);
            this.groupBoxImportOptions.Name = "groupBoxImportOptions";
            this.groupBoxImportOptions.Size = new System.Drawing.Size(310, 326);
            this.groupBoxImportOptions.TabIndex = 14;
            this.groupBoxImportOptions.TabStop = false;
            this.groupBoxImportOptions.Text = "Import options";
            //
            // checkBoxMergeShortLines
            //
            this.checkBoxMergeShortLines.AutoSize = true;
            this.checkBoxMergeShortLines.Checked = true;
            this.checkBoxMergeShortLines.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxMergeShortLines.Location = new System.Drawing.Point(19, 98);
            this.checkBoxMergeShortLines.Name = "checkBoxMergeShortLines";
            this.checkBoxMergeShortLines.Size = new System.Drawing.Size(193, 17);
            this.checkBoxMergeShortLines.TabIndex = 40;
            this.checkBoxMergeShortLines.Text = "Merge short lines with continuation";
            this.checkBoxMergeShortLines.UseVisualStyleBackColor = true;
            this.checkBoxMergeShortLines.CheckedChanged += new System.EventHandler(this.checkBoxMergeShortLines_CheckedChanged);
            //
            // groupBoxDuration
            //
            this.groupBoxDuration.Controls.Add(this.numericUpDownDurationFixed);
            this.groupBoxDuration.Controls.Add(this.radioButtonDurationFixed);
            this.groupBoxDuration.Controls.Add(this.radioButtonDurationAuto);
            this.groupBoxDuration.Location = new System.Drawing.Point(6, 223);
            this.groupBoxDuration.Name = "groupBoxDuration";
            this.groupBoxDuration.Size = new System.Drawing.Size(298, 70);
            this.groupBoxDuration.TabIndex = 39;
            this.groupBoxDuration.TabStop = false;
            this.groupBoxDuration.Text = "Duration";
            //
            // numericUpDownDurationFixed
            //
            this.numericUpDownDurationFixed.Location = new System.Drawing.Point(111, 42);
            this.numericUpDownDurationFixed.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDownDurationFixed.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDownDurationFixed.Name = "numericUpDownDurationFixed";
            this.numericUpDownDurationFixed.Size = new System.Drawing.Size(64, 21);
            this.numericUpDownDurationFixed.TabIndex = 37;
            this.numericUpDownDurationFixed.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            //
            // radioButtonDurationFixed
            //
            this.radioButtonDurationFixed.AutoSize = true;
            this.radioButtonDurationFixed.Location = new System.Drawing.Point(16, 42);
            this.radioButtonDurationFixed.Name = "radioButtonDurationFixed";
            this.radioButtonDurationFixed.Size = new System.Drawing.Size(51, 17);
            this.radioButtonDurationFixed.TabIndex = 3;
            this.radioButtonDurationFixed.Text = "Fixed";
            this.radioButtonDurationFixed.UseVisualStyleBackColor = true;
            this.radioButtonDurationFixed.CheckedChanged += new System.EventHandler(this.radioButtonDurationFixed_CheckedChanged);
            //
            // radioButtonDurationAuto
            //
            this.radioButtonDurationAuto.AutoSize = true;
            this.radioButtonDurationAuto.Checked = true;
            this.radioButtonDurationAuto.Location = new System.Drawing.Point(16, 19);
            this.radioButtonDurationAuto.Name = "radioButtonDurationAuto";
            this.radioButtonDurationAuto.Size = new System.Drawing.Size(48, 17);
            this.radioButtonDurationAuto.TabIndex = 2;
            this.radioButtonDurationAuto.TabStop = true;
            this.radioButtonDurationAuto.Text = "Auto";
            this.radioButtonDurationAuto.UseVisualStyleBackColor = true;
            //
            // buttonRefresh
            //
            this.buttonRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRefresh.Location = new System.Drawing.Point(11, 299);
            this.buttonRefresh.Name = "buttonRefresh";
            this.buttonRefresh.Size = new System.Drawing.Size(102, 21);
            this.buttonRefresh.TabIndex = 38;
            this.buttonRefresh.Text = "Refresh";
            this.buttonRefresh.UseVisualStyleBackColor = true;
            this.buttonRefresh.Click += new System.EventHandler(this.buttonRefresh_Click);
            //
            // numericUpDownGapBetweenLines
            //
            this.numericUpDownGapBetweenLines.Location = new System.Drawing.Point(19, 192);
            this.numericUpDownGapBetweenLines.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDownGapBetweenLines.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownGapBetweenLines.Name = "numericUpDownGapBetweenLines";
            this.numericUpDownGapBetweenLines.Size = new System.Drawing.Size(64, 21);
            this.numericUpDownGapBetweenLines.TabIndex = 36;
            this.numericUpDownGapBetweenLines.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            //
            // labelGapBetweenSubtitles
            //
            this.labelGapBetweenSubtitles.AutoSize = true;
            this.labelGapBetweenSubtitles.Location = new System.Drawing.Point(19, 175);
            this.labelGapBetweenSubtitles.Name = "labelGapBetweenSubtitles";
            this.labelGapBetweenSubtitles.Size = new System.Drawing.Size(180, 13);
            this.labelGapBetweenSubtitles.TabIndex = 37;
            this.labelGapBetweenSubtitles.Text = "Gap between subtitles (milliseconds)";
            //
            // checkBoxRemoveLinesWithoutLetters
            //
            this.checkBoxRemoveLinesWithoutLetters.AutoSize = true;
            this.checkBoxRemoveLinesWithoutLetters.Checked = true;
            this.checkBoxRemoveLinesWithoutLetters.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxRemoveLinesWithoutLetters.Location = new System.Drawing.Point(19, 144);
            this.checkBoxRemoveLinesWithoutLetters.Name = "checkBoxRemoveLinesWithoutLetters";
            this.checkBoxRemoveLinesWithoutLetters.Size = new System.Drawing.Size(162, 17);
            this.checkBoxRemoveLinesWithoutLetters.TabIndex = 5;
            this.checkBoxRemoveLinesWithoutLetters.Text = "Remove lines without letters";
            this.checkBoxRemoveLinesWithoutLetters.UseVisualStyleBackColor = true;
            this.checkBoxRemoveLinesWithoutLetters.CheckedChanged += new System.EventHandler(this.checkBoxRemoveLinesWithoutLettersOrNumbers_CheckedChanged);
            //
            // groupBoxSplitting
            //
            this.groupBoxSplitting.Controls.Add(this.radioButtonAutoSplit);
            this.groupBoxSplitting.Controls.Add(this.radioButtonLineMode);
            this.groupBoxSplitting.Location = new System.Drawing.Point(6, 19);
            this.groupBoxSplitting.Name = "groupBoxSplitting";
            this.groupBoxSplitting.Size = new System.Drawing.Size(298, 68);
            this.groupBoxSplitting.TabIndex = 4;
            this.groupBoxSplitting.TabStop = false;
            this.groupBoxSplitting.Text = "Splitting";
            //
            // radioButtonAutoSplit
            //
            this.radioButtonAutoSplit.AutoSize = true;
            this.radioButtonAutoSplit.Checked = true;
            this.radioButtonAutoSplit.Location = new System.Drawing.Point(14, 19);
            this.radioButtonAutoSplit.Name = "radioButtonAutoSplit";
            this.radioButtonAutoSplit.Size = new System.Drawing.Size(93, 17);
            this.radioButtonAutoSplit.TabIndex = 2;
            this.radioButtonAutoSplit.TabStop = true;
            this.radioButtonAutoSplit.Text = "Auto split text";
            this.radioButtonAutoSplit.UseVisualStyleBackColor = true;
            this.radioButtonAutoSplit.CheckedChanged += new System.EventHandler(this.radioButtonAutoSplit_CheckedChanged);
            //
            // radioButtonLineMode
            //
            this.radioButtonLineMode.AutoSize = true;
            this.radioButtonLineMode.Location = new System.Drawing.Point(14, 42);
            this.radioButtonLineMode.Name = "radioButtonLineMode";
            this.radioButtonLineMode.Size = new System.Drawing.Size(133, 17);
            this.radioButtonLineMode.TabIndex = 0;
            this.radioButtonLineMode.Text = "One line is one subtitle";
            this.radioButtonLineMode.UseVisualStyleBackColor = true;
            this.radioButtonLineMode.CheckedChanged += new System.EventHandler(this.radioButtonLineMode_CheckedChanged);
            //
            // checkBoxRemoveEmptyLines
            //
            this.checkBoxRemoveEmptyLines.AutoSize = true;
            this.checkBoxRemoveEmptyLines.Checked = true;
            this.checkBoxRemoveEmptyLines.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxRemoveEmptyLines.Location = new System.Drawing.Point(19, 121);
            this.checkBoxRemoveEmptyLines.Name = "checkBoxRemoveEmptyLines";
            this.checkBoxRemoveEmptyLines.Size = new System.Drawing.Size(122, 17);
            this.checkBoxRemoveEmptyLines.TabIndex = 3;
            this.checkBoxRemoveEmptyLines.Text = "Remove empty lines";
            this.checkBoxRemoveEmptyLines.UseVisualStyleBackColor = true;
            this.checkBoxRemoveEmptyLines.CheckedChanged += new System.EventHandler(this.checkBoxRemoveEmptyLines_CheckedChanged);
            //
            // groupBoxImportResult
            //
            this.groupBoxImportResult.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxImportResult.Controls.Add(this.SubtitleListview1);
            this.groupBoxImportResult.Location = new System.Drawing.Point(12, 344);
            this.groupBoxImportResult.Name = "groupBoxImportResult";
            this.groupBoxImportResult.Size = new System.Drawing.Size(823, 227);
            this.groupBoxImportResult.TabIndex = 15;
            this.groupBoxImportResult.TabStop = false;
            this.groupBoxImportResult.Text = "Preview";
            //
            // SubtitleListview1
            //
            this.SubtitleListview1.AllowDrop = true;
            this.SubtitleListview1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SubtitleListview1.FirstVisibleIndex = -1;
            this.SubtitleListview1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SubtitleListview1.FullRowSelect = true;
            this.SubtitleListview1.GridLines = true;
            this.SubtitleListview1.HideSelection = false;
            this.SubtitleListview1.Location = new System.Drawing.Point(6, 19);
            this.SubtitleListview1.MultiSelect = false;
            this.SubtitleListview1.Name = "SubtitleListview1";
            this.SubtitleListview1.Size = new System.Drawing.Size(811, 186);
            this.SubtitleListview1.TabIndex = 12;
            this.SubtitleListview1.UseCompatibleStateImageBehavior = false;
            this.SubtitleListview1.View = System.Windows.Forms.View.Details;
            //
            // openFileDialog1
            //
            this.openFileDialog1.FileName = "openFileDialog1";
            //
            // buttonCancel
            //
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(754, 579);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 21);
            this.buttonCancel.TabIndex = 17;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            //
            // buttonOK
            //
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(673, 579);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 21);
            this.buttonOK.TabIndex = 16;
            this.buttonOK.Text = "&Next >";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            //
            // ImportText
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(847, 616);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.groupBoxImportResult);
            this.Controls.Add(this.groupBoxImportOptions);
            this.Controls.Add(this.groupBoxImportText);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(810, 648);
            this.Name = "ImportText";
            this.ShowIcon = false;
            this.Text = "Import text";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ImportText_KeyDown);
            this.groupBoxImportText.ResumeLayout(false);
            this.groupBoxImportText.PerformLayout();
            this.groupBoxImportOptions.ResumeLayout(false);
            this.groupBoxImportOptions.PerformLayout();
            this.groupBoxDuration.ResumeLayout(false);
            this.groupBoxDuration.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDurationFixed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownGapBetweenLines)).EndInit();
            this.groupBoxSplitting.ResumeLayout(false);
            this.groupBoxSplitting.PerformLayout();
            this.groupBoxImportResult.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonOpenText;
        private Controls.SubtitleListView SubtitleListview1;
        private System.Windows.Forms.GroupBox groupBoxImportText;
        private System.Windows.Forms.TextBox textBoxText;
        private System.Windows.Forms.GroupBox groupBoxImportOptions;
        private System.Windows.Forms.GroupBox groupBoxImportResult;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.GroupBox groupBoxSplitting;
        private System.Windows.Forms.RadioButton radioButtonAutoSplit;
        private System.Windows.Forms.RadioButton radioButtonLineMode;
        private System.Windows.Forms.CheckBox checkBoxRemoveEmptyLines;
        private System.Windows.Forms.CheckBox checkBoxRemoveLinesWithoutLetters;
        private System.Windows.Forms.NumericUpDown numericUpDownGapBetweenLines;
        private System.Windows.Forms.Label labelGapBetweenSubtitles;
        private System.Windows.Forms.Button buttonRefresh;
        private System.Windows.Forms.GroupBox groupBoxDuration;
        private System.Windows.Forms.NumericUpDown numericUpDownDurationFixed;
        private System.Windows.Forms.RadioButton radioButtonDurationFixed;
        private System.Windows.Forms.RadioButton radioButtonDurationAuto;
        private System.Windows.Forms.CheckBox checkBoxMergeShortLines;
    }
}