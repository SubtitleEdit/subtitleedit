namespace Nikse.SubtitleEdit.Forms.Ocr
{
    sealed partial class OcrPreprocessingSettings
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
            this.labelThresholdDescription = new System.Windows.Forms.Label();
            this.numericUpDownThreshold = new System.Windows.Forms.NumericUpDown();
            this.pictureBoxSubtitleImage = new System.Windows.Forms.PictureBox();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.panelColorToWhite = new System.Windows.Forms.Panel();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.labelOriginalImage = new System.Windows.Forms.Label();
            this.labelPostImage = new System.Windows.Forms.Label();
            this.buttonColorToWhite = new System.Windows.Forms.Button();
            this.buttonColorToRemove = new System.Windows.Forms.Button();
            this.panelColorToRemove = new System.Windows.Forms.Panel();
            this.groupBoxBinaryImageCompareThreshold = new System.Windows.Forms.GroupBox();
            this.trackBarThresshold = new System.Windows.Forms.TrackBar();
            this.groupBoxCropping = new System.Windows.Forms.GroupBox();
            this.checkBoxCropTransparent = new System.Windows.Forms.CheckBox();
            this.groupBoxColors = new System.Windows.Forms.GroupBox();
            this.checkBoxYellowToWhite = new System.Windows.Forms.CheckBox();
            this.checkBoxInvertColors = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownThreshold)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSubtitleImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBoxBinaryImageCompareThreshold.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarThresshold)).BeginInit();
            this.groupBoxCropping.SuspendLayout();
            this.groupBoxColors.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelThresholdDescription
            // 
            this.labelThresholdDescription.AutoSize = true;
            this.labelThresholdDescription.Location = new System.Drawing.Point(17, 61);
            this.labelThresholdDescription.Name = "labelThresholdDescription";
            this.labelThresholdDescription.Size = new System.Drawing.Size(372, 13);
            this.labelThresholdDescription.TabIndex = 16;
            this.labelThresholdDescription.Text = "Adjust value until text is shown clearly (normally values between 200 and 300)";
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
            270,
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
            this.pictureBoxSubtitleImage.Click += new System.EventHandler(this.pictureBoxSubtitleImage_Click);
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
            this.pictureBox1.BackColor = System.Drawing.Color.DimGray;
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox1.Location = new System.Drawing.Point(12, 412);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(804, 140);
            this.pictureBox1.TabIndex = 17;
            this.pictureBox1.TabStop = false;
            // 
            // panelColorToWhite
            // 
            this.panelColorToWhite.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelColorToWhite.Location = new System.Drawing.Point(180, 70);
            this.panelColorToWhite.Name = "panelColorToWhite";
            this.panelColorToWhite.Size = new System.Drawing.Size(21, 20);
            this.panelColorToWhite.TabIndex = 19;
            this.panelColorToWhite.Click += new System.EventHandler(this.ColorToWhite);
            // 
            // labelOriginalImage
            // 
            this.labelOriginalImage.AutoSize = true;
            this.labelOriginalImage.Location = new System.Drawing.Point(12, 231);
            this.labelOriginalImage.Name = "labelOriginalImage";
            this.labelOriginalImage.Size = new System.Drawing.Size(73, 13);
            this.labelOriginalImage.TabIndex = 21;
            this.labelOriginalImage.Text = "Original image";
            // 
            // labelPostImage
            // 
            this.labelPostImage.AutoSize = true;
            this.labelPostImage.Location = new System.Drawing.Point(12, 396);
            this.labelPostImage.Name = "labelPostImage";
            this.labelPostImage.Size = new System.Drawing.Size(132, 13);
            this.labelPostImage.TabIndex = 22;
            this.labelPostImage.Text = "Image after pre-processing";
            // 
            // buttonColorToWhite
            // 
            this.buttonColorToWhite.Location = new System.Drawing.Point(9, 68);
            this.buttonColorToWhite.Name = "buttonColorToWhite";
            this.buttonColorToWhite.Size = new System.Drawing.Size(162, 23);
            this.buttonColorToWhite.TabIndex = 24;
            this.buttonColorToWhite.Text = "Color to white";
            this.buttonColorToWhite.UseVisualStyleBackColor = true;
            this.buttonColorToWhite.Click += new System.EventHandler(this.ColorToWhite);
            // 
            // buttonColorToRemove
            // 
            this.buttonColorToRemove.Location = new System.Drawing.Point(9, 99);
            this.buttonColorToRemove.Name = "buttonColorToRemove";
            this.buttonColorToRemove.Size = new System.Drawing.Size(162, 23);
            this.buttonColorToRemove.TabIndex = 26;
            this.buttonColorToRemove.Text = "Color to remove";
            this.buttonColorToRemove.UseVisualStyleBackColor = true;
            this.buttonColorToRemove.Click += new System.EventHandler(this.buttonColorToRemove_Click);
            // 
            // panelColorToRemove
            // 
            this.panelColorToRemove.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelColorToRemove.Location = new System.Drawing.Point(180, 100);
            this.panelColorToRemove.Name = "panelColorToRemove";
            this.panelColorToRemove.Size = new System.Drawing.Size(21, 20);
            this.panelColorToRemove.TabIndex = 25;
            this.panelColorToRemove.Click += new System.EventHandler(this.panelColorToRemove_Click);
            // 
            // groupBoxBinaryImageCompareThreshold
            // 
            this.groupBoxBinaryImageCompareThreshold.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxBinaryImageCompareThreshold.Controls.Add(this.labelThresholdDescription);
            this.groupBoxBinaryImageCompareThreshold.Controls.Add(this.trackBarThresshold);
            this.groupBoxBinaryImageCompareThreshold.Controls.Add(this.numericUpDownThreshold);
            this.groupBoxBinaryImageCompareThreshold.Location = new System.Drawing.Point(347, 12);
            this.groupBoxBinaryImageCompareThreshold.Name = "groupBoxBinaryImageCompareThreshold";
            this.groupBoxBinaryImageCompareThreshold.Size = new System.Drawing.Size(469, 216);
            this.groupBoxBinaryImageCompareThreshold.TabIndex = 29;
            this.groupBoxBinaryImageCompareThreshold.TabStop = false;
            this.groupBoxBinaryImageCompareThreshold.Text = "Binary image compare threshold";
            // 
            // trackBarThresshold
            // 
            this.trackBarThresshold.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.trackBarThresshold.Location = new System.Drawing.Point(83, 29);
            this.trackBarThresshold.Name = "trackBarThresshold";
            this.trackBarThresshold.Size = new System.Drawing.Size(380, 45);
            this.trackBarThresshold.TabIndex = 17;
            this.trackBarThresshold.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBarThresshold.ValueChanged += new System.EventHandler(this.trackBarThresshold_ValueChanged);
            // 
            // groupBoxCropping
            // 
            this.groupBoxCropping.Controls.Add(this.checkBoxCropTransparent);
            this.groupBoxCropping.Location = new System.Drawing.Point(15, 182);
            this.groupBoxCropping.Name = "groupBoxCropping";
            this.groupBoxCropping.Size = new System.Drawing.Size(326, 46);
            this.groupBoxCropping.TabIndex = 30;
            this.groupBoxCropping.TabStop = false;
            this.groupBoxCropping.Text = "Cropping";
            // 
            // checkBoxCropTransparent
            // 
            this.checkBoxCropTransparent.AutoSize = true;
            this.checkBoxCropTransparent.Location = new System.Drawing.Point(9, 19);
            this.checkBoxCropTransparent.Name = "checkBoxCropTransparent";
            this.checkBoxCropTransparent.Size = new System.Drawing.Size(135, 17);
            this.checkBoxCropTransparent.TabIndex = 29;
            this.checkBoxCropTransparent.Text = "Crop transparent colors";
            this.checkBoxCropTransparent.UseVisualStyleBackColor = true;
            this.checkBoxCropTransparent.CheckedChanged += new System.EventHandler(this.checkBoxCropTransparent_CheckedChanged);
            // 
            // groupBoxColors
            // 
            this.groupBoxColors.Controls.Add(this.checkBoxYellowToWhite);
            this.groupBoxColors.Controls.Add(this.checkBoxInvertColors);
            this.groupBoxColors.Controls.Add(this.buttonColorToWhite);
            this.groupBoxColors.Controls.Add(this.panelColorToWhite);
            this.groupBoxColors.Controls.Add(this.panelColorToRemove);
            this.groupBoxColors.Controls.Add(this.buttonColorToRemove);
            this.groupBoxColors.Location = new System.Drawing.Point(15, 12);
            this.groupBoxColors.Name = "groupBoxColors";
            this.groupBoxColors.Size = new System.Drawing.Size(326, 164);
            this.groupBoxColors.TabIndex = 30;
            this.groupBoxColors.TabStop = false;
            this.groupBoxColors.Text = "Colors";
            // 
            // checkBoxYellowToWhite
            // 
            this.checkBoxYellowToWhite.AutoSize = true;
            this.checkBoxYellowToWhite.Location = new System.Drawing.Point(9, 42);
            this.checkBoxYellowToWhite.Name = "checkBoxYellowToWhite";
            this.checkBoxYellowToWhite.Size = new System.Drawing.Size(97, 17);
            this.checkBoxYellowToWhite.TabIndex = 28;
            this.checkBoxYellowToWhite.Text = "Yellow to white";
            this.checkBoxYellowToWhite.UseVisualStyleBackColor = true;
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
            // OcrPreprocessingSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(828, 591);
            this.Controls.Add(this.groupBoxColors);
            this.Controls.Add(this.groupBoxCropping);
            this.Controls.Add(this.groupBoxBinaryImageCompareThreshold);
            this.Controls.Add(this.labelPostImage);
            this.Controls.Add(this.labelOriginalImage);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.pictureBoxSubtitleImage);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(790, 481);
            this.Name = "OcrPreprocessingSettings";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "OCR image preprocessing";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SetForeColorThreshold_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownThreshold)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSubtitleImage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.groupBoxBinaryImageCompareThreshold.ResumeLayout(false);
            this.groupBoxBinaryImageCompareThreshold.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarThresshold)).EndInit();
            this.groupBoxCropping.ResumeLayout(false);
            this.groupBoxCropping.PerformLayout();
            this.groupBoxColors.ResumeLayout(false);
            this.groupBoxColors.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelThresholdDescription;
        private System.Windows.Forms.NumericUpDown numericUpDownThreshold;
        private System.Windows.Forms.PictureBox pictureBoxSubtitleImage;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Panel panelColorToWhite;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.Label labelOriginalImage;
        private System.Windows.Forms.Label labelPostImage;
        private System.Windows.Forms.Button buttonColorToWhite;
        private System.Windows.Forms.Button buttonColorToRemove;
        private System.Windows.Forms.Panel panelColorToRemove;
        private System.Windows.Forms.GroupBox groupBoxBinaryImageCompareThreshold;
        private System.Windows.Forms.GroupBox groupBoxCropping;
        private System.Windows.Forms.GroupBox groupBoxColors;
        private System.Windows.Forms.CheckBox checkBoxInvertColors;
        private System.Windows.Forms.CheckBox checkBoxCropTransparent;
        private System.Windows.Forms.CheckBox checkBoxYellowToWhite;
        private System.Windows.Forms.TrackBar trackBarThresshold;
    }
}