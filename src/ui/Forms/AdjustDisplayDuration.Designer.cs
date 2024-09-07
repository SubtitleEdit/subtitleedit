﻿namespace Nikse.SubtitleEdit.Forms
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
            this.numericUpDownMaxCharsSec = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelMaxCharsPerSecond = new System.Windows.Forms.Label();
            this.numericUpDownSeconds = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.numericUpDownPercent = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.numericUpDownOptimalCharsSec = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelOptimalCharsSec = new System.Windows.Forms.Label();
            this.numericUpDownFixedMilliseconds = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelMillisecondsFixed = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.checkBoxExtendOnly = new System.Windows.Forms.CheckBox();
            this.checkBoxCheckShotChanges = new System.Windows.Forms.CheckBox();
            this.checkBoxEnforceDurationLimits = new System.Windows.Forms.CheckBox();
            this.groupBoxAdjustVia.SuspendLayout();
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
            this.labelNote.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelNote.AutoSize = true;
            this.labelNote.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.labelNote.Location = new System.Drawing.Point(10, 275);
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
            this.buttonCancel.Location = new System.Drawing.Point(537, 270);
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
            this.buttonOK.Location = new System.Drawing.Point(456, 270);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 20;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.ButtonOkClick);
            // 
            // numericUpDownMaxCharsSec
            // 
            this.numericUpDownMaxCharsSec.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownMaxCharsSec.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownMaxCharsSec.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownMaxCharsSec.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownMaxCharsSec.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownMaxCharsSec.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownMaxCharsSec.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownMaxCharsSec.DecimalPlaces = 1;
            this.numericUpDownMaxCharsSec.Enabled = false;
            this.numericUpDownMaxCharsSec.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericUpDownMaxCharsSec.Location = new System.Drawing.Point(357, 89);
            this.numericUpDownMaxCharsSec.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDownMaxCharsSec.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.numericUpDownMaxCharsSec.Name = "numericUpDownMaxCharsSec";
            this.numericUpDownMaxCharsSec.Size = new System.Drawing.Size(80, 23);
            this.numericUpDownMaxCharsSec.TabIndex = 6;
            this.numericUpDownMaxCharsSec.TabStop = false;
            this.numericUpDownMaxCharsSec.ThousandsSeparator = false;
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
            this.numericUpDownSeconds.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownSeconds.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownSeconds.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownSeconds.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownSeconds.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownSeconds.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownSeconds.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
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
            this.numericUpDownSeconds.Size = new System.Drawing.Size(80, 23);
            this.numericUpDownSeconds.TabIndex = 3;
            this.numericUpDownSeconds.TabStop = false;
            this.numericUpDownSeconds.ThousandsSeparator = false;
            this.numericUpDownSeconds.Value = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            // 
            // numericUpDownPercent
            // 
            this.numericUpDownPercent.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownPercent.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownPercent.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownPercent.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownPercent.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownPercent.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownPercent.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownPercent.DecimalPlaces = 0;
            this.numericUpDownPercent.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownPercent.Location = new System.Drawing.Point(182, 89);
            this.numericUpDownPercent.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.numericUpDownPercent.Minimum = new decimal(new int[] {
            75,
            0,
            0,
            0});
            this.numericUpDownPercent.Name = "numericUpDownPercent";
            this.numericUpDownPercent.Size = new System.Drawing.Size(80, 23);
            this.numericUpDownPercent.TabIndex = 4;
            this.numericUpDownPercent.TabStop = false;
            this.numericUpDownPercent.ThousandsSeparator = false;
            this.numericUpDownPercent.Value = new decimal(new int[] {
            110,
            0,
            0,
            0});
            // 
            // numericUpDownOptimalCharsSec
            // 
            this.numericUpDownOptimalCharsSec.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownOptimalCharsSec.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownOptimalCharsSec.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownOptimalCharsSec.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownOptimalCharsSec.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownOptimalCharsSec.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownOptimalCharsSec.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownOptimalCharsSec.DecimalPlaces = 1;
            this.numericUpDownOptimalCharsSec.Enabled = false;
            this.numericUpDownOptimalCharsSec.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericUpDownOptimalCharsSec.Location = new System.Drawing.Point(357, 147);
            this.numericUpDownOptimalCharsSec.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDownOptimalCharsSec.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.numericUpDownOptimalCharsSec.Name = "numericUpDownOptimalCharsSec";
            this.numericUpDownOptimalCharsSec.Size = new System.Drawing.Size(80, 23);
            this.numericUpDownOptimalCharsSec.TabIndex = 7;
            this.numericUpDownOptimalCharsSec.TabStop = false;
            this.numericUpDownOptimalCharsSec.ThousandsSeparator = false;
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
            this.numericUpDownFixedMilliseconds.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownFixedMilliseconds.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownFixedMilliseconds.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownFixedMilliseconds.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownFixedMilliseconds.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownFixedMilliseconds.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownFixedMilliseconds.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownFixedMilliseconds.DecimalPlaces = 0;
            this.numericUpDownFixedMilliseconds.Enabled = false;
            this.numericUpDownFixedMilliseconds.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownFixedMilliseconds.Location = new System.Drawing.Point(501, 89);
            this.numericUpDownFixedMilliseconds.Maximum = new decimal(new int[] {
            20000,
            0,
            0,
            0});
            this.numericUpDownFixedMilliseconds.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.numericUpDownFixedMilliseconds.Name = "numericUpDownFixedMilliseconds";
            this.numericUpDownFixedMilliseconds.Size = new System.Drawing.Size(80, 23);
            this.numericUpDownFixedMilliseconds.TabIndex = 9;
            this.numericUpDownFixedMilliseconds.TabStop = false;
            this.numericUpDownFixedMilliseconds.ThousandsSeparator = false;
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
            // checkBoxCheckShotChanges
            // 
            this.checkBoxCheckShotChanges.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxCheckShotChanges.AutoSize = true;
            this.checkBoxCheckShotChanges.Location = new System.Drawing.Point(12, 247);
            this.checkBoxCheckShotChanges.Name = "checkBoxCheckShotChanges";
            this.checkBoxCheckShotChanges.Size = new System.Drawing.Size(122, 17);
            this.checkBoxCheckShotChanges.TabIndex = 19;
            this.checkBoxCheckShotChanges.Text = "Check shot changes";
            this.checkBoxCheckShotChanges.UseVisualStyleBackColor = true;
            // 
            // checkBoxEnforceDurationLimits
            // 
            this.checkBoxEnforceDurationLimits.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxEnforceDurationLimits.AutoSize = true;
            this.checkBoxEnforceDurationLimits.Location = new System.Drawing.Point(12, 224);
            this.checkBoxEnforceDurationLimits.Name = "checkBoxEnforceDurationLimits";
            this.checkBoxEnforceDurationLimits.Size = new System.Drawing.Size(217, 17);
            this.checkBoxEnforceDurationLimits.TabIndex = 18;
            this.checkBoxEnforceDurationLimits.Text = "Enforce minimum and maximum duration";
            this.checkBoxEnforceDurationLimits.UseVisualStyleBackColor = true;
            // 
            // AdjustDisplayDuration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 307);
            this.Controls.Add(this.checkBoxEnforceDurationLimits);
            this.Controls.Add(this.checkBoxCheckShotChanges);
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
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Adjust display time";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormAdjustDisplayTime_KeyDown);
            this.groupBoxAdjustVia.ResumeLayout(false);
            this.groupBoxAdjustVia.PerformLayout();
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
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownMaxCharsSec;
        private System.Windows.Forms.Label labelMaxCharsPerSecond;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownSeconds;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownPercent;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownOptimalCharsSec;
        private System.Windows.Forms.Label labelOptimalCharsSec;
        private System.Windows.Forms.RadioButton radioButtonFixed;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownFixedMilliseconds;
        private System.Windows.Forms.Label labelMillisecondsFixed;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkBoxExtendOnly;
        private System.Windows.Forms.CheckBox checkBoxCheckShotChanges;
        private System.Windows.Forms.CheckBox checkBoxEnforceDurationLimits;
    }
}