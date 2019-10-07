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
            this.checkBoxOverrideOriginalYPosition = new System.Windows.Forms.CheckBox();
            this.numericUpDownBottomMargin = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownWidth = new System.Windows.Forms.NumericUpDown();
            this.labelWidth = new System.Windows.Forms.Label();
            this.checkBoxOverrideVideoSize = new System.Windows.Forms.CheckBox();
            this.numericUpDownHeight = new System.Windows.Forms.NumericUpDown();
            this.labelHeight = new System.Windows.Forms.Label();
            this.numericUpDownXMargin = new System.Windows.Forms.NumericUpDown();
            this.checkBoxOverrideOriginalXPosition = new System.Windows.Forms.CheckBox();
            this.labelXMargin = new System.Windows.Forms.Label();
            this.labelXAlignment = new System.Windows.Forms.Label();
            this.comboBoxHAlign = new System.Windows.Forms.ComboBox();
            this.labelXMarginPercent = new System.Windows.Forms.Label();
            this.labelBottomMarginPercent = new System.Windows.Forms.Label();
            this.buttonChooseResolution = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownBottomMargin)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownXMargin)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(307, 176);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 21);
            this.buttonOK.TabIndex = 90;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.ButtonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(388, 176);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 21);
            this.buttonCancel.TabIndex = 100;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // labelBottomMargin
            // 
            this.labelBottomMargin.AutoSize = true;
            this.labelBottomMargin.Location = new System.Drawing.Point(13, 137);
            this.labelBottomMargin.Name = "labelBottomMargin";
            this.labelBottomMargin.Size = new System.Drawing.Size(74, 13);
            this.labelBottomMargin.TabIndex = 10;
            this.labelBottomMargin.Text = "Bottom margin";
            // 
            // checkBoxOverrideOriginalYPosition
            // 
            this.checkBoxOverrideOriginalYPosition.AutoSize = true;
            this.checkBoxOverrideOriginalYPosition.Location = new System.Drawing.Point(16, 114);
            this.checkBoxOverrideOriginalYPosition.Name = "checkBoxOverrideOriginalYPosition";
            this.checkBoxOverrideOriginalYPosition.Size = new System.Drawing.Size(151, 17);
            this.checkBoxOverrideOriginalYPosition.TabIndex = 40;
            this.checkBoxOverrideOriginalYPosition.Text = "Override original Y position";
            this.checkBoxOverrideOriginalYPosition.UseVisualStyleBackColor = true;
            this.checkBoxOverrideOriginalYPosition.CheckedChanged += new System.EventHandler(this.CheckBoxOverrideOriginalYPosition_CheckedChanged);
            // 
            // numericUpDownBottomMargin
            // 
            this.numericUpDownBottomMargin.Location = new System.Drawing.Point(95, 135);
            this.numericUpDownBottomMargin.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.numericUpDownBottomMargin.Name = "numericUpDownBottomMargin";
            this.numericUpDownBottomMargin.Size = new System.Drawing.Size(62, 20);
            this.numericUpDownBottomMargin.TabIndex = 50;
            // 
            // numericUpDownWidth
            // 
            this.numericUpDownWidth.Location = new System.Drawing.Point(333, 50);
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
            this.numericUpDownWidth.TabIndex = 70;
            this.numericUpDownWidth.Value = new decimal(new int[] {
            1920,
            0,
            0,
            0});
            // 
            // labelWidth
            // 
            this.labelWidth.AutoSize = true;
            this.labelWidth.Location = new System.Drawing.Point(283, 52);
            this.labelWidth.Name = "labelWidth";
            this.labelWidth.Size = new System.Drawing.Size(35, 13);
            this.labelWidth.TabIndex = 14;
            this.labelWidth.Text = "Width";
            // 
            // checkBoxOverrideVideoSize
            // 
            this.checkBoxOverrideVideoSize.AutoSize = true;
            this.checkBoxOverrideVideoSize.Location = new System.Drawing.Point(286, 24);
            this.checkBoxOverrideVideoSize.Name = "checkBoxOverrideVideoSize";
            this.checkBoxOverrideVideoSize.Size = new System.Drawing.Size(149, 17);
            this.checkBoxOverrideVideoSize.TabIndex = 60;
            this.checkBoxOverrideVideoSize.Text = "Override output video size";
            this.checkBoxOverrideVideoSize.UseVisualStyleBackColor = true;
            this.checkBoxOverrideVideoSize.CheckedChanged += new System.EventHandler(this.CheckBoxOverrideVideoSize_CheckedChanged);
            // 
            // numericUpDownHeight
            // 
            this.numericUpDownHeight.Location = new System.Drawing.Point(333, 76);
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
            this.numericUpDownHeight.TabIndex = 80;
            this.numericUpDownHeight.Value = new decimal(new int[] {
            1080,
            0,
            0,
            0});
            // 
            // labelHeight
            // 
            this.labelHeight.AutoSize = true;
            this.labelHeight.Location = new System.Drawing.Point(283, 78);
            this.labelHeight.Name = "labelHeight";
            this.labelHeight.Size = new System.Drawing.Size(38, 13);
            this.labelHeight.TabIndex = 17;
            this.labelHeight.Text = "Height";
            // 
            // numericUpDownXMargin
            // 
            this.numericUpDownXMargin.Location = new System.Drawing.Point(95, 45);
            this.numericUpDownXMargin.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.numericUpDownXMargin.Name = "numericUpDownXMargin";
            this.numericUpDownXMargin.Size = new System.Drawing.Size(62, 20);
            this.numericUpDownXMargin.TabIndex = 20;
            // 
            // checkBoxOverrideOriginalXPosition
            // 
            this.checkBoxOverrideOriginalXPosition.AutoSize = true;
            this.checkBoxOverrideOriginalXPosition.Location = new System.Drawing.Point(16, 24);
            this.checkBoxOverrideOriginalXPosition.Name = "checkBoxOverrideOriginalXPosition";
            this.checkBoxOverrideOriginalXPosition.Size = new System.Drawing.Size(151, 17);
            this.checkBoxOverrideOriginalXPosition.TabIndex = 10;
            this.checkBoxOverrideOriginalXPosition.Text = "Override original X position";
            this.checkBoxOverrideOriginalXPosition.UseVisualStyleBackColor = true;
            this.checkBoxOverrideOriginalXPosition.CheckedChanged += new System.EventHandler(this.checkBoxOverrideOriginalXPosition_CheckedChanged);
            // 
            // labelXMargin
            // 
            this.labelXMargin.AutoSize = true;
            this.labelXMargin.Location = new System.Drawing.Point(13, 47);
            this.labelXMargin.Name = "labelXMargin";
            this.labelXMargin.Size = new System.Drawing.Size(39, 13);
            this.labelXMargin.TabIndex = 18;
            this.labelXMargin.Text = "Margin";
            // 
            // labelXAlignment
            // 
            this.labelXAlignment.AutoSize = true;
            this.labelXAlignment.Location = new System.Drawing.Point(13, 76);
            this.labelXAlignment.Name = "labelXAlignment";
            this.labelXAlignment.Size = new System.Drawing.Size(53, 13);
            this.labelXAlignment.TabIndex = 21;
            this.labelXAlignment.Text = "Alignment";
            // 
            // comboBoxHAlign
            // 
            this.comboBoxHAlign.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxHAlign.FormattingEnabled = true;
            this.comboBoxHAlign.Items.AddRange(new object[] {
            "Left",
            "Center",
            "Right",
            "Center - Left justify"});
            this.comboBoxHAlign.Location = new System.Drawing.Point(95, 73);
            this.comboBoxHAlign.Name = "comboBoxHAlign";
            this.comboBoxHAlign.Size = new System.Drawing.Size(123, 21);
            this.comboBoxHAlign.TabIndex = 30;
            // 
            // labelXMarginPercent
            // 
            this.labelXMarginPercent.AutoSize = true;
            this.labelXMarginPercent.Location = new System.Drawing.Point(161, 48);
            this.labelXMarginPercent.Name = "labelXMarginPercent";
            this.labelXMarginPercent.Size = new System.Drawing.Size(15, 13);
            this.labelXMarginPercent.TabIndex = 101;
            this.labelXMarginPercent.Text = "%";
            // 
            // labelBottomMarginPercent
            // 
            this.labelBottomMarginPercent.AutoSize = true;
            this.labelBottomMarginPercent.Location = new System.Drawing.Point(161, 138);
            this.labelBottomMarginPercent.Name = "labelBottomMarginPercent";
            this.labelBottomMarginPercent.Size = new System.Drawing.Size(15, 13);
            this.labelBottomMarginPercent.TabIndex = 102;
            this.labelBottomMarginPercent.Text = "%";
            // 
            // buttonChooseResolution
            // 
            this.buttonChooseResolution.Location = new System.Drawing.Point(332, 102);
            this.buttonChooseResolution.Name = "buttonChooseResolution";
            this.buttonChooseResolution.Size = new System.Drawing.Size(27, 23);
            this.buttonChooseResolution.TabIndex = 82;
            this.buttonChooseResolution.Text = "...";
            this.buttonChooseResolution.UseVisualStyleBackColor = true;
            this.buttonChooseResolution.Click += new System.EventHandler(this.ButtonChooseResolution_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // BatchConvertTsSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(475, 209);
            this.Controls.Add(this.buttonChooseResolution);
            this.Controls.Add(this.labelBottomMarginPercent);
            this.Controls.Add(this.labelXMarginPercent);
            this.Controls.Add(this.comboBoxHAlign);
            this.Controls.Add(this.labelXAlignment);
            this.Controls.Add(this.numericUpDownXMargin);
            this.Controls.Add(this.checkBoxOverrideOriginalXPosition);
            this.Controls.Add(this.labelXMargin);
            this.Controls.Add(this.numericUpDownHeight);
            this.Controls.Add(this.labelHeight);
            this.Controls.Add(this.checkBoxOverrideVideoSize);
            this.Controls.Add(this.numericUpDownWidth);
            this.Controls.Add(this.labelWidth);
            this.Controls.Add(this.numericUpDownBottomMargin);
            this.Controls.Add(this.checkBoxOverrideOriginalYPosition);
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
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownXMargin)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label labelBottomMargin;
        private System.Windows.Forms.CheckBox checkBoxOverrideOriginalYPosition;
        private System.Windows.Forms.NumericUpDown numericUpDownBottomMargin;
        private System.Windows.Forms.NumericUpDown numericUpDownWidth;
        private System.Windows.Forms.Label labelWidth;
        private System.Windows.Forms.CheckBox checkBoxOverrideVideoSize;
        private System.Windows.Forms.NumericUpDown numericUpDownHeight;
        private System.Windows.Forms.Label labelHeight;
        private System.Windows.Forms.NumericUpDown numericUpDownXMargin;
        private System.Windows.Forms.CheckBox checkBoxOverrideOriginalXPosition;
        private System.Windows.Forms.Label labelXMargin;
        private System.Windows.Forms.Label labelXAlignment;
        private System.Windows.Forms.ComboBox comboBoxHAlign;
        private System.Windows.Forms.Label labelXMarginPercent;
        private System.Windows.Forms.Label labelBottomMarginPercent;
        private System.Windows.Forms.Button buttonChooseResolution;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
    }
}