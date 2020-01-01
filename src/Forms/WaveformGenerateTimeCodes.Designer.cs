namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class WaveformGenerateTimeCodes
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
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.groupBoxStartFrom = new System.Windows.Forms.GroupBox();
            this.radioButtonStartFromPos = new System.Windows.Forms.RadioButton();
            this.radioButtonStartFromStart = new System.Windows.Forms.RadioButton();
            this.labelAbove1 = new System.Windows.Forms.Label();
            this.numericUpDownMinVol = new System.Windows.Forms.NumericUpDown();
            this.groupBoxDeleteLines = new System.Windows.Forms.GroupBox();
            this.radioButtonDeleteNone = new System.Windows.Forms.RadioButton();
            this.radioButtonForward = new System.Windows.Forms.RadioButton();
            this.radioButtonDeleteAll = new System.Windows.Forms.RadioButton();
            this.groupBoxDetectOptions = new System.Windows.Forms.GroupBox();
            this.labelBelow2 = new System.Windows.Forms.Label();
            this.labelAbove2 = new System.Windows.Forms.Label();
            this.numericUpDownBlockSize = new System.Windows.Forms.NumericUpDown();
            this.labelScanBlocksMs = new System.Windows.Forms.Label();
            this.labelBelow1 = new System.Windows.Forms.Label();
            this.numericUpDownMaxVol = new System.Windows.Forms.NumericUpDown();
            this.groupBoxOther = new System.Windows.Forms.GroupBox();
            this.labelSplit2 = new System.Windows.Forms.Label();
            this.labelSplit1 = new System.Windows.Forms.Label();
            this.numericUpDownDefaultMilliseconds = new System.Windows.Forms.NumericUpDown();
            this.groupBoxStartFrom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMinVol)).BeginInit();
            this.groupBoxDeleteLines.SuspendLayout();
            this.groupBoxDetectOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownBlockSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxVol)).BeginInit();
            this.groupBoxOther.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDefaultMilliseconds)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(598, 389);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 4;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(517, 389);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 3;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // groupBoxStartFrom
            // 
            this.groupBoxStartFrom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxStartFrom.Controls.Add(this.radioButtonStartFromPos);
            this.groupBoxStartFrom.Controls.Add(this.radioButtonStartFromStart);
            this.groupBoxStartFrom.Location = new System.Drawing.Point(12, 12);
            this.groupBoxStartFrom.Name = "groupBoxStartFrom";
            this.groupBoxStartFrom.Size = new System.Drawing.Size(661, 76);
            this.groupBoxStartFrom.TabIndex = 5;
            this.groupBoxStartFrom.TabStop = false;
            this.groupBoxStartFrom.Text = "Start from";
            // 
            // radioButtonStartFromPos
            // 
            this.radioButtonStartFromPos.AutoSize = true;
            this.radioButtonStartFromPos.Checked = true;
            this.radioButtonStartFromPos.Location = new System.Drawing.Point(10, 24);
            this.radioButtonStartFromPos.Name = "radioButtonStartFromPos";
            this.radioButtonStartFromPos.Size = new System.Drawing.Size(127, 17);
            this.radioButtonStartFromPos.TabIndex = 2;
            this.radioButtonStartFromPos.TabStop = true;
            this.radioButtonStartFromPos.Text = "Current video position";
            this.radioButtonStartFromPos.UseVisualStyleBackColor = true;
            // 
            // radioButtonStartFromStart
            // 
            this.radioButtonStartFromStart.AutoSize = true;
            this.radioButtonStartFromStart.Location = new System.Drawing.Point(10, 47);
            this.radioButtonStartFromStart.Name = "radioButtonStartFromStart";
            this.radioButtonStartFromStart.Size = new System.Drawing.Size(47, 17);
            this.radioButtonStartFromStart.TabIndex = 0;
            this.radioButtonStartFromStart.Text = "Start";
            this.radioButtonStartFromStart.UseVisualStyleBackColor = true;
            // 
            // labelAbove1
            // 
            this.labelAbove1.AutoSize = true;
            this.labelAbove1.Location = new System.Drawing.Point(6, 62);
            this.labelAbove1.Name = "labelAbove1";
            this.labelAbove1.Size = new System.Drawing.Size(186, 13);
            this.labelAbove1.TabIndex = 6;
            this.labelAbove1.Text = "Block average volume must be above";
            // 
            // numericUpDownMinVol
            // 
            this.numericUpDownMinVol.Location = new System.Drawing.Point(208, 60);
            this.numericUpDownMinVol.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownMinVol.Name = "numericUpDownMinVol";
            this.numericUpDownMinVol.Size = new System.Drawing.Size(51, 20);
            this.numericUpDownMinVol.TabIndex = 7;
            this.numericUpDownMinVol.Value = new decimal(new int[] {
            35,
            0,
            0,
            0});
            // 
            // groupBoxDeleteLines
            // 
            this.groupBoxDeleteLines.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxDeleteLines.Controls.Add(this.radioButtonDeleteNone);
            this.groupBoxDeleteLines.Controls.Add(this.radioButtonForward);
            this.groupBoxDeleteLines.Controls.Add(this.radioButtonDeleteAll);
            this.groupBoxDeleteLines.Location = new System.Drawing.Point(12, 94);
            this.groupBoxDeleteLines.Name = "groupBoxDeleteLines";
            this.groupBoxDeleteLines.Size = new System.Drawing.Size(661, 99);
            this.groupBoxDeleteLines.TabIndex = 6;
            this.groupBoxDeleteLines.TabStop = false;
            this.groupBoxDeleteLines.Text = "Delete lines";
            // 
            // radioButtonDeleteNone
            // 
            this.radioButtonDeleteNone.AutoSize = true;
            this.radioButtonDeleteNone.Location = new System.Drawing.Point(10, 47);
            this.radioButtonDeleteNone.Name = "radioButtonDeleteNone";
            this.radioButtonDeleteNone.Size = new System.Drawing.Size(51, 17);
            this.radioButtonDeleteNone.TabIndex = 2;
            this.radioButtonDeleteNone.Text = "None";
            this.radioButtonDeleteNone.UseVisualStyleBackColor = true;
            // 
            // radioButtonForward
            // 
            this.radioButtonForward.AutoSize = true;
            this.radioButtonForward.Checked = true;
            this.radioButtonForward.Location = new System.Drawing.Point(10, 70);
            this.radioButtonForward.Name = "radioButtonForward";
            this.radioButtonForward.Size = new System.Drawing.Size(152, 17);
            this.radioButtonForward.TabIndex = 1;
            this.radioButtonForward.TabStop = true;
            this.radioButtonForward.Text = "From current video position";
            this.radioButtonForward.UseVisualStyleBackColor = true;
            // 
            // radioButtonDeleteAll
            // 
            this.radioButtonDeleteAll.AutoSize = true;
            this.radioButtonDeleteAll.Location = new System.Drawing.Point(10, 24);
            this.radioButtonDeleteAll.Name = "radioButtonDeleteAll";
            this.radioButtonDeleteAll.Size = new System.Drawing.Size(36, 17);
            this.radioButtonDeleteAll.TabIndex = 0;
            this.radioButtonDeleteAll.Text = "All";
            this.radioButtonDeleteAll.UseVisualStyleBackColor = true;
            // 
            // groupBoxDetectOptions
            // 
            this.groupBoxDetectOptions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxDetectOptions.Controls.Add(this.labelBelow2);
            this.groupBoxDetectOptions.Controls.Add(this.labelAbove2);
            this.groupBoxDetectOptions.Controls.Add(this.numericUpDownBlockSize);
            this.groupBoxDetectOptions.Controls.Add(this.labelScanBlocksMs);
            this.groupBoxDetectOptions.Controls.Add(this.labelBelow1);
            this.groupBoxDetectOptions.Controls.Add(this.numericUpDownMaxVol);
            this.groupBoxDetectOptions.Controls.Add(this.labelAbove1);
            this.groupBoxDetectOptions.Controls.Add(this.numericUpDownMinVol);
            this.groupBoxDetectOptions.Location = new System.Drawing.Point(12, 199);
            this.groupBoxDetectOptions.Name = "groupBoxDetectOptions";
            this.groupBoxDetectOptions.Size = new System.Drawing.Size(661, 121);
            this.groupBoxDetectOptions.TabIndex = 6;
            this.groupBoxDetectOptions.TabStop = false;
            this.groupBoxDetectOptions.Text = "Detect options";
            // 
            // labelBelow2
            // 
            this.labelBelow2.AutoSize = true;
            this.labelBelow2.Location = new System.Drawing.Point(265, 91);
            this.labelBelow2.Name = "labelBelow2";
            this.labelBelow2.Size = new System.Drawing.Size(109, 13);
            this.labelBelow2.TabIndex = 15;
            this.labelBelow2.Text = "% of total max volume";
            // 
            // labelAbove2
            // 
            this.labelAbove2.AutoSize = true;
            this.labelAbove2.Location = new System.Drawing.Point(265, 62);
            this.labelAbove2.Name = "labelAbove2";
            this.labelAbove2.Size = new System.Drawing.Size(129, 13);
            this.labelAbove2.TabIndex = 14;
            this.labelAbove2.Text = "% of total average volume";
            // 
            // numericUpDownBlockSize
            // 
            this.numericUpDownBlockSize.Location = new System.Drawing.Point(149, 24);
            this.numericUpDownBlockSize.Maximum = new decimal(new int[] {
            300,
            0,
            0,
            0});
            this.numericUpDownBlockSize.Minimum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numericUpDownBlockSize.Name = "numericUpDownBlockSize";
            this.numericUpDownBlockSize.Size = new System.Drawing.Size(51, 20);
            this.numericUpDownBlockSize.TabIndex = 13;
            this.numericUpDownBlockSize.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // labelScanBlocksMs
            // 
            this.labelScanBlocksMs.AutoSize = true;
            this.labelScanBlocksMs.Location = new System.Drawing.Point(6, 26);
            this.labelScanBlocksMs.Name = "labelScanBlocksMs";
            this.labelScanBlocksMs.Size = new System.Drawing.Size(137, 13);
            this.labelScanBlocksMs.TabIndex = 12;
            this.labelScanBlocksMs.Text = "Scan blocks of milliseconds";
            // 
            // labelBelow1
            // 
            this.labelBelow1.AutoSize = true;
            this.labelBelow1.Location = new System.Drawing.Point(7, 89);
            this.labelBelow1.Name = "labelBelow1";
            this.labelBelow1.Size = new System.Drawing.Size(184, 13);
            this.labelBelow1.TabIndex = 9;
            this.labelBelow1.Text = "Block average volume must be below";
            // 
            // numericUpDownMaxVol
            // 
            this.numericUpDownMaxVol.Location = new System.Drawing.Point(208, 89);
            this.numericUpDownMaxVol.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownMaxVol.Name = "numericUpDownMaxVol";
            this.numericUpDownMaxVol.Size = new System.Drawing.Size(51, 20);
            this.numericUpDownMaxVol.TabIndex = 10;
            this.numericUpDownMaxVol.Value = new decimal(new int[] {
            70,
            0,
            0,
            0});
            // 
            // groupBoxOther
            // 
            this.groupBoxOther.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxOther.Controls.Add(this.labelSplit2);
            this.groupBoxOther.Controls.Add(this.labelSplit1);
            this.groupBoxOther.Controls.Add(this.numericUpDownDefaultMilliseconds);
            this.groupBoxOther.Location = new System.Drawing.Point(12, 326);
            this.groupBoxOther.Name = "groupBoxOther";
            this.groupBoxOther.Size = new System.Drawing.Size(661, 57);
            this.groupBoxOther.TabIndex = 7;
            this.groupBoxOther.TabStop = false;
            this.groupBoxOther.Text = "Other";
            // 
            // labelSplit2
            // 
            this.labelSplit2.AutoSize = true;
            this.labelSplit2.Location = new System.Drawing.Point(185, 28);
            this.labelSplit2.Name = "labelSplit2";
            this.labelSplit2.Size = new System.Drawing.Size(63, 13);
            this.labelSplit2.TabIndex = 13;
            this.labelSplit2.Text = "milliseconds";
            // 
            // labelSplit1
            // 
            this.labelSplit1.AutoSize = true;
            this.labelSplit1.Location = new System.Drawing.Point(6, 28);
            this.labelSplit1.Name = "labelSplit1";
            this.labelSplit1.Size = new System.Drawing.Size(103, 13);
            this.labelSplit1.TabIndex = 12;
            this.labelSplit1.Text = "Split long subtitles at";
            // 
            // numericUpDownDefaultMilliseconds
            // 
            this.numericUpDownDefaultMilliseconds.Location = new System.Drawing.Point(111, 26);
            this.numericUpDownDefaultMilliseconds.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.numericUpDownDefaultMilliseconds.Minimum = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            this.numericUpDownDefaultMilliseconds.Name = "numericUpDownDefaultMilliseconds";
            this.numericUpDownDefaultMilliseconds.Size = new System.Drawing.Size(51, 20);
            this.numericUpDownDefaultMilliseconds.TabIndex = 11;
            this.numericUpDownDefaultMilliseconds.Value = new decimal(new int[] {
            3500,
            0,
            0,
            0});
            // 
            // WaveformGenerateTimeCodes
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(685, 422);
            this.Controls.Add(this.groupBoxOther);
            this.Controls.Add(this.groupBoxDetectOptions);
            this.Controls.Add(this.groupBoxDeleteLines);
            this.Controls.Add(this.groupBoxStartFrom);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.KeyPreview = true;
            this.Name = "WaveformGenerateTimeCodes";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Guess time codes";
            this.groupBoxStartFrom.ResumeLayout(false);
            this.groupBoxStartFrom.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMinVol)).EndInit();
            this.groupBoxDeleteLines.ResumeLayout(false);
            this.groupBoxDeleteLines.PerformLayout();
            this.groupBoxDetectOptions.ResumeLayout(false);
            this.groupBoxDetectOptions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownBlockSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxVol)).EndInit();
            this.groupBoxOther.ResumeLayout(false);
            this.groupBoxOther.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDefaultMilliseconds)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.GroupBox groupBoxStartFrom;
        private System.Windows.Forms.RadioButton radioButtonStartFromStart;
        private System.Windows.Forms.Label labelAbove1;
        private System.Windows.Forms.NumericUpDown numericUpDownMinVol;
        private System.Windows.Forms.RadioButton radioButtonStartFromPos;
        private System.Windows.Forms.GroupBox groupBoxDeleteLines;
        private System.Windows.Forms.RadioButton radioButtonDeleteNone;
        private System.Windows.Forms.RadioButton radioButtonForward;
        private System.Windows.Forms.RadioButton radioButtonDeleteAll;
        private System.Windows.Forms.GroupBox groupBoxDetectOptions;
        private System.Windows.Forms.Label labelBelow1;
        private System.Windows.Forms.NumericUpDown numericUpDownMaxVol;
        private System.Windows.Forms.NumericUpDown numericUpDownBlockSize;
        private System.Windows.Forms.Label labelScanBlocksMs;
        private System.Windows.Forms.Label labelBelow2;
        private System.Windows.Forms.Label labelAbove2;
        private System.Windows.Forms.GroupBox groupBoxOther;
        private System.Windows.Forms.Label labelSplit1;
        private System.Windows.Forms.NumericUpDown numericUpDownDefaultMilliseconds;
        private System.Windows.Forms.Label labelSplit2;
    }
}