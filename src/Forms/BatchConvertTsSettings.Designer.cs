namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class BatchConvertTsSettings
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
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.labelBottomMargin = new System.Windows.Forms.Label();
            this.checkBoxOverrideOriginalPosition = new System.Windows.Forms.CheckBox();
            this.numericUpDownBottomMargin = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownWidth = new System.Windows.Forms.NumericUpDown();
            this.labelWidth = new System.Windows.Forms.Label();
            this.checkBoxOverrideVideoSize = new System.Windows.Forms.CheckBox();
            this.numericUpDownHeight = new System.Windows.Forms.NumericUpDown();
            this.labelHeight = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownBottomMargin)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownHeight)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(231, 145);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 21);
            this.buttonOK.TabIndex = 8;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.ButtonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(312, 145);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 21);
            this.buttonCancel.TabIndex = 9;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // labelBottomMargin
            // 
            this.labelBottomMargin.AutoSize = true;
            this.labelBottomMargin.Location = new System.Drawing.Point(23, 47);
            this.labelBottomMargin.Name = "labelBottomMargin";
            this.labelBottomMargin.Size = new System.Drawing.Size(74, 13);
            this.labelBottomMargin.TabIndex = 10;
            this.labelBottomMargin.Text = "Bottom margin";
            // 
            // checkBoxOverrideOriginalPosition
            // 
            this.checkBoxOverrideOriginalPosition.AutoSize = true;
            this.checkBoxOverrideOriginalPosition.Location = new System.Drawing.Point(26, 24);
            this.checkBoxOverrideOriginalPosition.Name = "checkBoxOverrideOriginalPosition";
            this.checkBoxOverrideOriginalPosition.Size = new System.Drawing.Size(141, 17);
            this.checkBoxOverrideOriginalPosition.TabIndex = 12;
            this.checkBoxOverrideOriginalPosition.Text = "Override original position";
            this.checkBoxOverrideOriginalPosition.UseVisualStyleBackColor = true;
            this.checkBoxOverrideOriginalPosition.CheckedChanged += new System.EventHandler(this.CheckBoxOverrideOriginalPosition_CheckedChanged);
            // 
            // numericUpDownBottomMargin
            // 
            this.numericUpDownBottomMargin.Location = new System.Drawing.Point(105, 45);
            this.numericUpDownBottomMargin.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.numericUpDownBottomMargin.Name = "numericUpDownBottomMargin";
            this.numericUpDownBottomMargin.Size = new System.Drawing.Size(62, 20);
            this.numericUpDownBottomMargin.TabIndex = 13;
            // 
            // numericUpDownWidth
            // 
            this.numericUpDownWidth.Location = new System.Drawing.Point(291, 50);
            this.numericUpDownWidth.Maximum = new decimal(new int[] {
            50000,
            0,
            0,
            0});
            this.numericUpDownWidth.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownWidth.Name = "numericUpDownWidth";
            this.numericUpDownWidth.Size = new System.Drawing.Size(62, 20);
            this.numericUpDownWidth.TabIndex = 15;
            this.numericUpDownWidth.Value = new decimal(new int[] {
            1920,
            0,
            0,
            0});
            // 
            // labelWidth
            // 
            this.labelWidth.AutoSize = true;
            this.labelWidth.Location = new System.Drawing.Point(241, 52);
            this.labelWidth.Name = "labelWidth";
            this.labelWidth.Size = new System.Drawing.Size(35, 13);
            this.labelWidth.TabIndex = 14;
            this.labelWidth.Text = "Width";
            // 
            // checkBoxOverrideVideoSize
            // 
            this.checkBoxOverrideVideoSize.AutoSize = true;
            this.checkBoxOverrideVideoSize.Location = new System.Drawing.Point(244, 24);
            this.checkBoxOverrideVideoSize.Name = "checkBoxOverrideVideoSize";
            this.checkBoxOverrideVideoSize.Size = new System.Drawing.Size(116, 17);
            this.checkBoxOverrideVideoSize.TabIndex = 16;
            this.checkBoxOverrideVideoSize.Text = "Override video size";
            this.checkBoxOverrideVideoSize.UseVisualStyleBackColor = true;
            this.checkBoxOverrideVideoSize.CheckedChanged += new System.EventHandler(this.CheckBoxOverrideVideoSize_CheckedChanged);
            // 
            // numericUpDownHeight
            // 
            this.numericUpDownHeight.Location = new System.Drawing.Point(291, 76);
            this.numericUpDownHeight.Maximum = new decimal(new int[] {
            50000,
            0,
            0,
            0});
            this.numericUpDownHeight.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownHeight.Name = "numericUpDownHeight";
            this.numericUpDownHeight.Size = new System.Drawing.Size(62, 20);
            this.numericUpDownHeight.TabIndex = 18;
            this.numericUpDownHeight.Value = new decimal(new int[] {
            1080,
            0,
            0,
            0});
            // 
            // labelHeight
            // 
            this.labelHeight.AutoSize = true;
            this.labelHeight.Location = new System.Drawing.Point(241, 78);
            this.labelHeight.Name = "labelHeight";
            this.labelHeight.Size = new System.Drawing.Size(38, 13);
            this.labelHeight.TabIndex = 17;
            this.labelHeight.Text = "Height";
            // 
            // BatchConvertTsSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(399, 178);
            this.Controls.Add(this.numericUpDownHeight);
            this.Controls.Add(this.labelHeight);
            this.Controls.Add(this.checkBoxOverrideVideoSize);
            this.Controls.Add(this.numericUpDownWidth);
            this.Controls.Add(this.labelWidth);
            this.Controls.Add(this.numericUpDownBottomMargin);
            this.Controls.Add(this.checkBoxOverrideOriginalPosition);
            this.Controls.Add(this.labelBottomMargin);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BatchConvertTsSettings";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "BatchConvertTsSettings";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.BatchConvertTsSettings_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownBottomMargin)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownHeight)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label labelBottomMargin;
        private System.Windows.Forms.CheckBox checkBoxOverrideOriginalPosition;
        private System.Windows.Forms.NumericUpDown numericUpDownBottomMargin;
        private System.Windows.Forms.NumericUpDown numericUpDownWidth;
        private System.Windows.Forms.Label labelWidth;
        private System.Windows.Forms.CheckBox checkBoxOverrideVideoSize;
        private System.Windows.Forms.NumericUpDown numericUpDownHeight;
        private System.Windows.Forms.Label labelHeight;
    }
}