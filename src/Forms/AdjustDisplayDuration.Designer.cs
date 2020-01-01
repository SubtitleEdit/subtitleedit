namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class AdjustDisplayDuration
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
            this.radioButtonPercent = new System.Windows.Forms.RadioButton();
            this.radioButtonSeconds = new System.Windows.Forms.RadioButton();
            this.groupBoxAdjustVia = new System.Windows.Forms.GroupBox();
            this.radioButtonFixed = new System.Windows.Forms.RadioButton();
            this.radioButtonAutoRecalculate = new System.Windows.Forms.RadioButton();
            this.labelNote = new System.Windows.Forms.Label();
            this.labelAddInPercent = new System.Windows.Forms.Label();
            this.labelAddSeconds = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.numericUpDownMaxCharsSec = new System.Windows.Forms.NumericUpDown();
            this.labelMaxCharsPerSecond = new System.Windows.Forms.Label();
            this.numericUpDownSeconds = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownPercent = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownOptimalCharsSec = new System.Windows.Forms.NumericUpDown();
            this.labelOptimalCharsSec = new System.Windows.Forms.Label();
            this.numericUpDownFixedMilliseconds = new System.Windows.Forms.NumericUpDown();
            this.labelMillisecondsFixed = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.checkBoxExtendOnly = new System.Windows.Forms.CheckBox();
            this.groupBoxAdjustVia.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxCharsSec)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSeconds)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPercent)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownOptimalCharsSec)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFixedMilliseconds)).BeginInit();
            this.SuspendLayout();
            // 
            // radioButtonPercent
            // 
            this.radioButtonPercent.AutoSize = true;
            this.radioButtonPercent.Location = new System.Drawing.Point(171, 21);
            this.radioButtonPercent.Name = "radioButtonPercent";
            this.radioButtonPercent.Size = new System.Drawing.Size(62, 17);
            this.radioButtonPercent.TabIndex = 1;
            this.radioButtonPercent.Text = "Percent";
            this.radioButtonPercent.UseVisualStyleBackColor = true;
            this.radioButtonPercent.CheckedChanged += new System.EventHandler(this.RadioButtonPercentCheckedChanged);
            // 
            // radioButtonSeconds
            // 
            this.radioButtonSeconds.AutoSize = true;
            this.radioButtonSeconds.Checked = true;
            this.radioButtonSeconds.Location = new System.Drawing.Point(11, 21);
            this.radioButtonSeconds.Name = "radioButtonSeconds";
            this.radioButtonSeconds.Size = new System.Drawing.Size(65, 17);
            this.radioButtonSeconds.TabIndex = 0;
            this.radioButtonSeconds.TabStop = true;
            this.radioButtonSeconds.Text = "Seconds";
            this.radioButtonSeconds.UseVisualStyleBackColor = true;
            this.radioButtonSeconds.CheckedChanged += new System.EventHandler(this.RadioButtonSecondsCheckedChanged);
            // 
            // groupBoxAdjustVia
            // 
            this.groupBoxAdjustVia.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxAdjustVia.Controls.Add(this.radioButtonFixed);
            this.groupBoxAdjustVia.Controls.Add(this.radioButtonAutoRecalculate);
            this.groupBoxAdjustVia.Controls.Add(this.radioButtonPercent);
            this.groupBoxAdjustVia.Controls.Add(this.radioButtonSeconds);
            this.groupBoxAdjustVia.Location = new System.Drawing.Point(13, 13);
            this.groupBoxAdjustVia.Name = "groupBoxAdjustVia";
            this.groupBoxAdjustVia.Size = new System.Drawing.Size(599, 47);
            this.groupBoxAdjustVia.TabIndex = 0;
            this.groupBoxAdjustVia.TabStop = false;
            this.groupBoxAdjustVia.Text = "Adjust via";
            // 
            // radioButtonFixed
            // 
            this.radioButtonFixed.AutoSize = true;
            this.radioButtonFixed.Location = new System.Drawing.Point(491, 21);
            this.radioButtonFixed.Name = "radioButtonFixed";
            this.radioButtonFixed.Size = new System.Drawing.Size(51, 17);
            this.radioButtonFixed.TabIndex = 3;
            this.radioButtonFixed.Text = "Fixed";
            this.radioButtonFixed.UseVisualStyleBackColor = true;
            this.radioButtonFixed.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged);
            // 
            // radioButtonAutoRecalculate
            // 
            this.radioButtonAutoRecalculate.AutoSize = true;
            this.radioButtonAutoRecalculate.Location = new System.Drawing.Point(344, 21);
            this.radioButtonAutoRecalculate.Name = "radioButtonAutoRecalculate";
            this.radioButtonAutoRecalculate.Size = new System.Drawing.Size(80, 17);
            this.radioButtonAutoRecalculate.TabIndex = 2;
            this.radioButtonAutoRecalculate.Text = "Recalculate";
            this.radioButtonAutoRecalculate.UseVisualStyleBackColor = true;
            this.radioButtonAutoRecalculate.CheckedChanged += new System.EventHandler(this.radioButtonAutoRecalculate_CheckedChanged);
            // 
            // labelNote
            // 
            this.labelNote.AutoSize = true;
            this.labelNote.Location = new System.Drawing.Point(10, 211);
            this.labelNote.Name = "labelNote";
            this.labelNote.Size = new System.Drawing.Size(279, 13);
            this.labelNote.TabIndex = 7;
            this.labelNote.Text = "Note: Display time will not overlap start time of next text";
            // 
            // labelAddInPercent
            // 
            this.labelAddInPercent.AutoSize = true;
            this.labelAddInPercent.Location = new System.Drawing.Point(179, 70);
            this.labelAddInPercent.Name = "labelAddInPercent";
            this.labelAddInPercent.Size = new System.Drawing.Size(89, 13);
            this.labelAddInPercent.TabIndex = 3;
            this.labelAddInPercent.Text = "Adjust in percent";
            // 
            // labelAddSeconds
            // 
            this.labelAddSeconds.AutoSize = true;
            this.labelAddSeconds.Location = new System.Drawing.Point(10, 70);
            this.labelAddSeconds.Name = "labelAddSeconds";
            this.labelAddSeconds.Size = new System.Drawing.Size(68, 13);
            this.labelAddSeconds.TabIndex = 1;
            this.labelAddSeconds.Text = "Add seconds";
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(537, 231);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 21;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(456, 231);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 20;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.ButtonOkClick);
            // 
            // numericUpDownMaxCharsSec
            // 
            this.numericUpDownMaxCharsSec.DecimalPlaces = 1;
            this.numericUpDownMaxCharsSec.Enabled = false;
            this.numericUpDownMaxCharsSec.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericUpDownMaxCharsSec.Location = new System.Drawing.Point(357, 89);
            this.numericUpDownMaxCharsSec.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.numericUpDownMaxCharsSec.Name = "numericUpDownMaxCharsSec";
            this.numericUpDownMaxCharsSec.Size = new System.Drawing.Size(80, 21);
            this.numericUpDownMaxCharsSec.TabIndex = 6;
            this.numericUpDownMaxCharsSec.Value = new decimal(new int[] {
            24,
            0,
            0,
            0});
            // 
            // labelMaxCharsPerSecond
            // 
            this.labelMaxCharsPerSecond.AutoSize = true;
            this.labelMaxCharsPerSecond.Location = new System.Drawing.Point(354, 70);
            this.labelMaxCharsPerSecond.Name = "labelMaxCharsPerSecond";
            this.labelMaxCharsPerSecond.Size = new System.Drawing.Size(80, 13);
            this.labelMaxCharsPerSecond.TabIndex = 5;
            this.labelMaxCharsPerSecond.Text = "Max. chars/sec";
            // 
            // numericUpDownSeconds
            // 
            this.numericUpDownSeconds.DecimalPlaces = 3;
            this.numericUpDownSeconds.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.numericUpDownSeconds.Location = new System.Drawing.Point(13, 89);
            this.numericUpDownSeconds.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.numericUpDownSeconds.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            -2147483648});
            this.numericUpDownSeconds.Name = "numericUpDownSeconds";
            this.numericUpDownSeconds.Size = new System.Drawing.Size(80, 21);
            this.numericUpDownSeconds.TabIndex = 3;
            this.numericUpDownSeconds.Value = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            // 
            // numericUpDownPercent
            // 
            this.numericUpDownPercent.Location = new System.Drawing.Point(182, 89);
            this.numericUpDownPercent.Maximum = new decimal(new int[] {
            150,
            0,
            0,
            0});
            this.numericUpDownPercent.Minimum = new decimal(new int[] {
            75,
            0,
            0,
            0});
            this.numericUpDownPercent.Name = "numericUpDownPercent";
            this.numericUpDownPercent.Size = new System.Drawing.Size(80, 21);
            this.numericUpDownPercent.TabIndex = 4;
            this.numericUpDownPercent.Value = new decimal(new int[] {
            110,
            0,
            0,
            0});
            // 
            // numericUpDownOptimalCharsSec
            // 
            this.numericUpDownOptimalCharsSec.DecimalPlaces = 1;
            this.numericUpDownOptimalCharsSec.Enabled = false;
            this.numericUpDownOptimalCharsSec.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericUpDownOptimalCharsSec.Location = new System.Drawing.Point(357, 147);
            this.numericUpDownOptimalCharsSec.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.numericUpDownOptimalCharsSec.Name = "numericUpDownOptimalCharsSec";
            this.numericUpDownOptimalCharsSec.Size = new System.Drawing.Size(80, 21);
            this.numericUpDownOptimalCharsSec.TabIndex = 7;
            this.numericUpDownOptimalCharsSec.Value = new decimal(new int[] {
            17,
            0,
            0,
            0});
            // 
            // labelOptimalCharsSec
            // 
            this.labelOptimalCharsSec.AutoSize = true;
            this.labelOptimalCharsSec.Location = new System.Drawing.Point(354, 128);
            this.labelOptimalCharsSec.Name = "labelOptimalCharsSec";
            this.labelOptimalCharsSec.Size = new System.Drawing.Size(92, 13);
            this.labelOptimalCharsSec.TabIndex = 10;
            this.labelOptimalCharsSec.Text = "Optimal chars/sec";
            // 
            // numericUpDownFixedMilliseconds
            // 
            this.numericUpDownFixedMilliseconds.Enabled = false;
            this.numericUpDownFixedMilliseconds.Location = new System.Drawing.Point(501, 89);
            this.numericUpDownFixedMilliseconds.Maximum = new decimal(new int[] {
            20000,
            0,
            0,
            0});
            this.numericUpDownFixedMilliseconds.Name = "numericUpDownFixedMilliseconds";
            this.numericUpDownFixedMilliseconds.Size = new System.Drawing.Size(80, 21);
            this.numericUpDownFixedMilliseconds.TabIndex = 9;
            this.numericUpDownFixedMilliseconds.Value = new decimal(new int[] {
            3000,
            0,
            0,
            0});
            // 
            // labelMillisecondsFixed
            // 
            this.labelMillisecondsFixed.AutoSize = true;
            this.labelMillisecondsFixed.Location = new System.Drawing.Point(498, 70);
            this.labelMillisecondsFixed.Name = "labelMillisecondsFixed";
            this.labelMillisecondsFixed.Size = new System.Drawing.Size(62, 13);
            this.labelMillisecondsFixed.TabIndex = 12;
            this.labelMillisecondsFixed.Text = "Milliseconds";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(264, 93);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(18, 13);
            this.label1.TabIndex = 22;
            this.label1.Text = "%";
            // 
            // checkBoxExtendOnly
            // 
            this.checkBoxExtendOnly.AutoSize = true;
            this.checkBoxExtendOnly.Location = new System.Drawing.Point(357, 184);
            this.checkBoxExtendOnly.Name = "checkBoxExtendOnly";
            this.checkBoxExtendOnly.Size = new System.Drawing.Size(83, 17);
            this.checkBoxExtendOnly.TabIndex = 8;
            this.checkBoxExtendOnly.Text = "Extend only";
            this.checkBoxExtendOnly.UseVisualStyleBackColor = true;
            // 
            // AdjustDisplayDuration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 268);
            this.Controls.Add(this.checkBoxExtendOnly);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.numericUpDownFixedMilliseconds);
            this.Controls.Add(this.labelMillisecondsFixed);
            this.Controls.Add(this.numericUpDownOptimalCharsSec);
            this.Controls.Add(this.labelOptimalCharsSec);
            this.Controls.Add(this.numericUpDownPercent);
            this.Controls.Add(this.numericUpDownSeconds);
            this.Controls.Add(this.numericUpDownMaxCharsSec);
            this.Controls.Add(this.labelMaxCharsPerSecond);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.labelAddSeconds);
            this.Controls.Add(this.labelAddInPercent);
            this.Controls.Add(this.labelNote);
            this.Controls.Add(this.groupBoxAdjustVia);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AdjustDisplayDuration";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Adjust display time";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormAdjustDisplayTime_KeyDown);
            this.groupBoxAdjustVia.ResumeLayout(false);
            this.groupBoxAdjustVia.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxCharsSec)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSeconds)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPercent)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownOptimalCharsSec)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFixedMilliseconds)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton radioButtonPercent;
        private System.Windows.Forms.RadioButton radioButtonSeconds;
        private System.Windows.Forms.GroupBox groupBoxAdjustVia;
        private System.Windows.Forms.Label labelNote;
        private System.Windows.Forms.Label labelAddInPercent;
        private System.Windows.Forms.Label labelAddSeconds;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.RadioButton radioButtonAutoRecalculate;
        private System.Windows.Forms.NumericUpDown numericUpDownMaxCharsSec;
        private System.Windows.Forms.Label labelMaxCharsPerSecond;
        private System.Windows.Forms.NumericUpDown numericUpDownSeconds;
        private System.Windows.Forms.NumericUpDown numericUpDownPercent;
        private System.Windows.Forms.NumericUpDown numericUpDownOptimalCharsSec;
        private System.Windows.Forms.Label labelOptimalCharsSec;
        private System.Windows.Forms.RadioButton radioButtonFixed;
        private System.Windows.Forms.NumericUpDown numericUpDownFixedMilliseconds;
        private System.Windows.Forms.Label labelMillisecondsFixed;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkBoxExtendOnly;
    }
}