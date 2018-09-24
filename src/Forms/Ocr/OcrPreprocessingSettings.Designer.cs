namespace Nikse.SubtitleEdit.Forms.Ocr
{
    partial class OcrPreprocessingSettings
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
            this.panelColorToWhite = new System.Windows.Forms.Panel();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.buttonColorToWhite = new System.Windows.Forms.Button();
            this.buttonColorToRemove = new System.Windows.Forms.Button();
            this.panelColorToRemove = new System.Windows.Forms.Panel();
            this.groupBoxBinaryImageCompareThresshold = new System.Windows.Forms.GroupBox();
            this.groupBoxCropping = new System.Windows.Forms.GroupBox();
            this.checkBoxCropTransparent = new System.Windows.Forms.CheckBox();
            this.groupBoxColors = new System.Windows.Forms.GroupBox();
            this.checkBoxInvertColors = new System.Windows.Forms.CheckBox();
            this.checkBoxYellowToWhite = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownThreshold)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSubtitleImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBoxBinaryImageCompareThresshold.SuspendLayout();
            this.groupBoxCropping.SuspendLayout();
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
            50,
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
            this.buttonCancel.Size = new System.Drawing.Size(75, 21);
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
            this.buttonOK.Size = new System.Drawing.Size(75, 21);
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
            // panelColorToWhite
            // 
            this.panelColorToWhite.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelColorToWhite.Location = new System.Drawing.Point(180, 70);
            this.panelColorToWhite.Name = "panelColorToWhite";
            this.panelColorToWhite.Size = new System.Drawing.Size(21, 20);
            this.panelColorToWhite.TabIndex = 19;
            this.panelColorToWhite.Click += new System.EventHandler(this.ColorToWhite);
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
            this.buttonColorToRemove.Text = "Clor to remove";
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
            // groupBoxBinaryImageCompareThresshold
            // 
            this.groupBoxBinaryImageCompareThresshold.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxBinaryImageCompareThresshold.Controls.Add(this.numericUpDownThreshold);
            this.groupBoxBinaryImageCompareThresshold.Controls.Add(this.labelDescription);
            this.groupBoxBinaryImageCompareThresshold.Location = new System.Drawing.Point(347, 12);
            this.groupBoxBinaryImageCompareThresshold.Name = "groupBoxBinaryImageCompareThresshold";
            this.groupBoxBinaryImageCompareThresshold.Size = new System.Drawing.Size(469, 216);
            this.groupBoxBinaryImageCompareThresshold.TabIndex = 29;
            this.groupBoxBinaryImageCompareThresshold.TabStop = false;
            this.groupBoxBinaryImageCompareThresshold.Text = "Binary image compare thresshold";
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
            // SetForeColorThreshold
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(828, 591);
            this.Controls.Add(this.groupBoxColors);
            this.Controls.Add(this.groupBoxCropping);
            this.Controls.Add(this.groupBoxBinaryImageCompareThresshold);
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
            this.Name = "SetForeColorThreshold";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "OCR image preprocessing";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SetForeColorThreshold_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownThreshold)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSubtitleImage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.groupBoxBinaryImageCompareThresshold.ResumeLayout(false);
            this.groupBoxBinaryImageCompareThresshold.PerformLayout();
            this.groupBoxCropping.ResumeLayout(false);
            this.groupBoxCropping.PerformLayout();
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
        private System.Windows.Forms.Panel panelColorToWhite;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button buttonColorToWhite;
        private System.Windows.Forms.Button buttonColorToRemove;
        private System.Windows.Forms.Panel panelColorToRemove;
        private System.Windows.Forms.GroupBox groupBoxBinaryImageCompareThresshold;
        private System.Windows.Forms.GroupBox groupBoxCropping;
        private System.Windows.Forms.GroupBox groupBoxColors;
        private System.Windows.Forms.CheckBox checkBoxInvertColors;
        private System.Windows.Forms.CheckBox checkBoxCropTransparent;
        private System.Windows.Forms.CheckBox checkBoxYellowToWhite;
    }
}