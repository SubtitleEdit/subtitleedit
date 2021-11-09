namespace Nikse.SubtitleEdit.Forms.Options
{
    partial class SettingsLineWidth
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
            this.labelMaximumLineWidth = new System.Windows.Forms.Label();
            this.numericUpDownMaxLineWidth = new System.Windows.Forms.NumericUpDown();
            this.labelPixelsSuffix = new System.Windows.Forms.Label();
            this.comboBoxMeasureFontName = new System.Windows.Forms.ComboBox();
            this.labelMeasureFont = new System.Windows.Forms.Label();
            this.checkBoxMeasureFontBold = new System.Windows.Forms.CheckBox();
            this.numericUpDownMeasureFontSize = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxLineWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMeasureFontSize)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(368, 91);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 8;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(287, 91);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 7;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // labelMaximumLineWidth
            // 
            this.labelMaximumLineWidth.AutoSize = true;
            this.labelMaximumLineWidth.Location = new System.Drawing.Point(12, 14);
            this.labelMaximumLineWidth.Name = "labelMaximumLineWidth";
            this.labelMaximumLineWidth.Size = new System.Drawing.Size(101, 13);
            this.labelMaximumLineWidth.TabIndex = 208;
            this.labelMaximumLineWidth.Text = "Maximum line width:";
            // 
            // numericUpDownMaxLineWidth
            // 
            this.numericUpDownMaxLineWidth.Location = new System.Drawing.Point(119, 12);
            this.numericUpDownMaxLineWidth.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.numericUpDownMaxLineWidth.Name = "numericUpDownMaxLineWidth";
            this.numericUpDownMaxLineWidth.Size = new System.Drawing.Size(56, 20);
            this.numericUpDownMaxLineWidth.TabIndex = 1;
            this.numericUpDownMaxLineWidth.Value = new decimal(new int[] {
            576,
            0,
            0,
            0});
            // 
            // labelPixelsSuffix
            // 
            this.labelPixelsSuffix.AutoSize = true;
            this.labelPixelsSuffix.Location = new System.Drawing.Point(181, 14);
            this.labelPixelsSuffix.Name = "labelPixelsSuffix";
            this.labelPixelsSuffix.Size = new System.Drawing.Size(33, 13);
            this.labelPixelsSuffix.TabIndex = 210;
            this.labelPixelsSuffix.Text = "pixels";
            // 
            // comboBoxMeasureFontName
            // 
            this.comboBoxMeasureFontName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxMeasureFontName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxMeasureFontName.DropDownWidth = 240;
            this.comboBoxMeasureFontName.FormattingEnabled = true;
            this.comboBoxMeasureFontName.Location = new System.Drawing.Point(119, 38);
            this.comboBoxMeasureFontName.Name = "comboBoxMeasureFontName";
            this.comboBoxMeasureFontName.Size = new System.Drawing.Size(199, 21);
            this.comboBoxMeasureFontName.TabIndex = 2;
            // 
            // labelMeasureFont
            // 
            this.labelMeasureFont.AutoSize = true;
            this.labelMeasureFont.Location = new System.Drawing.Point(12, 41);
            this.labelMeasureFont.Name = "labelMeasureFont";
            this.labelMeasureFont.Size = new System.Drawing.Size(72, 13);
            this.labelMeasureFont.TabIndex = 213;
            this.labelMeasureFont.Text = "Measure font:";
            // 
            // checkBoxMeasureFontBold
            // 
            this.checkBoxMeasureFontBold.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxMeasureFontBold.AutoSize = true;
            this.checkBoxMeasureFontBold.Location = new System.Drawing.Point(386, 40);
            this.checkBoxMeasureFontBold.Name = "checkBoxMeasureFontBold";
            this.checkBoxMeasureFontBold.Size = new System.Drawing.Size(47, 17);
            this.checkBoxMeasureFontBold.TabIndex = 4;
            this.checkBoxMeasureFontBold.Text = "Bold";
            this.checkBoxMeasureFontBold.UseVisualStyleBackColor = true;
            // 
            // numericUpDownMeasureFontSize
            // 
            this.numericUpDownMeasureFontSize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.numericUpDownMeasureFontSize.Location = new System.Drawing.Point(324, 38);
            this.numericUpDownMeasureFontSize.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.numericUpDownMeasureFontSize.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownMeasureFontSize.Name = "numericUpDownMeasureFontSize";
            this.numericUpDownMeasureFontSize.Size = new System.Drawing.Size(56, 20);
            this.numericUpDownMeasureFontSize.TabIndex = 3;
            this.numericUpDownMeasureFontSize.Value = new decimal(new int[] {
            24,
            0,
            0,
            0});
            // 
            // SettingsLineWidth
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(455, 126);
            this.Controls.Add(this.numericUpDownMeasureFontSize);
            this.Controls.Add(this.checkBoxMeasureFontBold);
            this.Controls.Add(this.labelMeasureFont);
            this.Controls.Add(this.comboBoxMeasureFontName);
            this.Controls.Add(this.labelPixelsSuffix);
            this.Controls.Add(this.numericUpDownMaxLineWidth);
            this.Controls.Add(this.labelMaximumLineWidth);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsLineWidth";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "SettingsLineWidth";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SettingsLineWidth_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxLineWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMeasureFontSize)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Label labelMaximumLineWidth;
        private System.Windows.Forms.NumericUpDown numericUpDownMaxLineWidth;
        private System.Windows.Forms.Label labelPixelsSuffix;
        private System.Windows.Forms.ComboBox comboBoxMeasureFontName;
        private System.Windows.Forms.Label labelMeasureFont;
        private System.Windows.Forms.CheckBox checkBoxMeasureFontBold;
        private System.Windows.Forms.NumericUpDown numericUpDownMeasureFontSize;
    }
}