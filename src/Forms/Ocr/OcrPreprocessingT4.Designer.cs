namespace Nikse.SubtitleEdit.Forms.Ocr
{
    sealed partial class OcrPreprocessingT4
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
            this.labelDescription = new System.Windows.Forms.Label();
            this.numericUpDownThreshold = new System.Windows.Forms.NumericUpDown();
            this.pictureBoxSubtitleImage = new System.Windows.Forms.PictureBox();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBoxBinaryImageCompareThreshold = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.numericUpDownScaling = new System.Windows.Forms.NumericUpDown();
            this.groupBoxColors = new System.Windows.Forms.GroupBox();
            this.checkBoxInvertColors = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownThreshold)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSubtitleImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBoxBinaryImageCompareThreshold.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownScaling)).BeginInit();
            this.groupBoxColors.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelDescription
            // 
            this.labelDescription.AutoSize = true;
            this.labelDescription.Location = new System.Drawing.Point(17, 58);
            this.labelDescription.Name = "labelDescription";
            this.labelDescription.Size = new System.Drawing.Size(372, 13);
            this.labelDescription.TabIndex = 16;
            this.labelDescription.Text = "Adjust value until text is shown clearly (normally values between 200 and 300)";
            // 
            // numericUpDownThreshold
            // 
            this.numericUpDownThreshold.Location = new System.Drawing.Point(20, 32);
            this.numericUpDownThreshold.Maximum = new decimal(new int[] {
            765,
            0,
            0,
            0});
            this.numericUpDownThreshold.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownThreshold.Name = "numericUpDownThreshold";
            this.numericUpDownThreshold.Size = new System.Drawing.Size(57, 20);
            this.numericUpDownThreshold.TabIndex = 15;
            this.numericUpDownThreshold.Value = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.numericUpDownThreshold.ValueChanged += new System.EventHandler(this.numericUpDownThreshold_ValueChanged);
            // 
            // pictureBoxSubtitleImage
            // 
            this.pictureBoxSubtitleImage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBoxSubtitleImage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxSubtitleImage.Location = new System.Drawing.Point(12, 247);
            this.pictureBoxSubtitleImage.Name = "pictureBoxSubtitleImage";
            this.pictureBoxSubtitleImage.Size = new System.Drawing.Size(804, 135);
            this.pictureBoxSubtitleImage.TabIndex = 14;
            this.pictureBoxSubtitleImage.TabStop = false;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(741, 558);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 13;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(660, 558);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 12;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox1.Location = new System.Drawing.Point(12, 412);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(804, 140);
            this.pictureBox1.TabIndex = 17;
            this.pictureBox1.TabStop = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 231);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(73, 13);
            this.label2.TabIndex = 21;
            this.label2.Text = "Original image";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 396);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(132, 13);
            this.label3.TabIndex = 22;
            this.label3.Text = "Image after pre-processing";
            // 
            // groupBoxBinaryImageCompareThreshold
            // 
            this.groupBoxBinaryImageCompareThreshold.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxBinaryImageCompareThreshold.Controls.Add(this.label4);
            this.groupBoxBinaryImageCompareThreshold.Controls.Add(this.label1);
            this.groupBoxBinaryImageCompareThreshold.Controls.Add(this.numericUpDownScaling);
            this.groupBoxBinaryImageCompareThreshold.Controls.Add(this.numericUpDownThreshold);
            this.groupBoxBinaryImageCompareThreshold.Controls.Add(this.labelDescription);
            this.groupBoxBinaryImageCompareThreshold.Location = new System.Drawing.Point(348, 12);
            this.groupBoxBinaryImageCompareThreshold.Name = "groupBoxBinaryImageCompareThreshold";
            this.groupBoxBinaryImageCompareThreshold.Size = new System.Drawing.Size(468, 216);
            this.groupBoxBinaryImageCompareThreshold.TabIndex = 29;
            this.groupBoxBinaryImageCompareThreshold.TabStop = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(83, 183);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(15, 13);
            this.label4.TabIndex = 19;
            this.label4.Text = "%";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(17, 165);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(42, 13);
            this.label1.TabIndex = 18;
            this.label1.Text = "Scaling";
            // 
            // numericUpDownScaling
            // 
            this.numericUpDownScaling.Location = new System.Drawing.Point(20, 181);
            this.numericUpDownScaling.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.numericUpDownScaling.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDownScaling.Name = "numericUpDownScaling";
            this.numericUpDownScaling.Size = new System.Drawing.Size(57, 20);
            this.numericUpDownScaling.TabIndex = 17;
            this.numericUpDownScaling.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDownScaling.ValueChanged += new System.EventHandler(this.numericUpDownScaling_ValueChanged);
            // 
            // groupBoxColors
            // 
            this.groupBoxColors.Controls.Add(this.checkBoxInvertColors);
            this.groupBoxColors.Location = new System.Drawing.Point(12, 12);
            this.groupBoxColors.Name = "groupBoxColors";
            this.groupBoxColors.Size = new System.Drawing.Size(326, 216);
            this.groupBoxColors.TabIndex = 31;
            this.groupBoxColors.TabStop = false;
            this.groupBoxColors.Text = "Colors";
            // 
            // checkBoxInvertColors
            // 
            this.checkBoxInvertColors.AutoSize = true;
            this.checkBoxInvertColors.Location = new System.Drawing.Point(9, 19);
            this.checkBoxInvertColors.Name = "checkBoxInvertColors";
            this.checkBoxInvertColors.Size = new System.Drawing.Size(84, 17);
            this.checkBoxInvertColors.TabIndex = 27;
            this.checkBoxInvertColors.Text = "Invert colors";
            this.checkBoxInvertColors.UseVisualStyleBackColor = true;
            this.checkBoxInvertColors.CheckedChanged += new System.EventHandler(this.checkBoxInvertColors_CheckedChanged);
            // 
            // OcrPreprocessingT4
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(828, 591);
            this.Controls.Add(this.groupBoxColors);
            this.Controls.Add(this.groupBoxBinaryImageCompareThreshold);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.pictureBoxSubtitleImage);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(790, 481);
            this.Name = "OcrPreprocessingT4";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "OCR image preprocessing";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SetForeColorThreshold_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownThreshold)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSubtitleImage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.groupBoxBinaryImageCompareThreshold.ResumeLayout(false);
            this.groupBoxBinaryImageCompareThreshold.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownScaling)).EndInit();
            this.groupBoxColors.ResumeLayout(false);
            this.groupBoxColors.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelDescription;
        private System.Windows.Forms.NumericUpDown numericUpDownThreshold;
        private System.Windows.Forms.PictureBox pictureBoxSubtitleImage;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBoxBinaryImageCompareThreshold;
        private System.Windows.Forms.GroupBox groupBoxColors;
        private System.Windows.Forms.CheckBox checkBoxInvertColors;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numericUpDownScaling;
    }
}