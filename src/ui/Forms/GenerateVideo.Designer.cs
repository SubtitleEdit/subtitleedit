
namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class GenerateVideo
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
            this.panelColor = new System.Windows.Forms.Panel();
            this.buttonColor = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.labelDuration = new System.Windows.Forms.Label();
            this.numericUpDownWidth = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownHeight = new System.Windows.Forms.NumericUpDown();
            this.labelWidth = new System.Windows.Forms.Label();
            this.labelHeight = new System.Windows.Forms.Label();
            this.groupBoxBackground = new System.Windows.Forms.GroupBox();
            this.radioButtonColor = new System.Windows.Forms.RadioButton();
            this.radioButtonCheckeredImage = new System.Windows.Forms.RadioButton();
            this.numericUpDownDurationMinutes = new System.Windows.Forms.NumericUpDown();
            this.labelPleaseWait = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownHeight)).BeginInit();
            this.groupBoxBackground.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDurationMinutes)).BeginInit();
            this.SuspendLayout();
            // 
            // panelColor
            // 
            this.panelColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelColor.Location = new System.Drawing.Point(169, 81);
            this.panelColor.Name = "panelColor";
            this.panelColor.Size = new System.Drawing.Size(21, 20);
            this.panelColor.TabIndex = 19;
            this.panelColor.MouseClick += new System.Windows.Forms.MouseEventHandler(this.panelColor_MouseClick);
            // 
            // buttonColor
            // 
            this.buttonColor.Location = new System.Drawing.Point(42, 80);
            this.buttonColor.Name = "buttonColor";
            this.buttonColor.Size = new System.Drawing.Size(121, 23);
            this.buttonColor.TabIndex = 20;
            this.buttonColor.Text = "Color";
            this.buttonColor.UseVisualStyleBackColor = true;
            this.buttonColor.Click += new System.EventHandler(this.buttonColor_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(451, 275);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(121, 23);
            this.buttonOK.TabIndex = 21;
            this.buttonOK.Text = "Generate";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(578, 275);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 22;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(12, 275);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(433, 23);
            this.progressBar1.TabIndex = 23;
            this.progressBar1.Visible = false;
            // 
            // labelDuration
            // 
            this.labelDuration.AutoSize = true;
            this.labelDuration.Location = new System.Drawing.Point(12, 29);
            this.labelDuration.Name = "labelDuration";
            this.labelDuration.Size = new System.Drawing.Size(97, 13);
            this.labelDuration.TabIndex = 24;
            this.labelDuration.Text = "Duration in minutes";
            // 
            // numericUpDownWidth
            // 
            this.numericUpDownWidth.Location = new System.Drawing.Point(165, 56);
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
            this.numericUpDownWidth.Size = new System.Drawing.Size(110, 20);
            this.numericUpDownWidth.TabIndex = 25;
            this.numericUpDownWidth.Value = new decimal(new int[] {
            1080,
            0,
            0,
            0});
            // 
            // numericUpDownHeight
            // 
            this.numericUpDownHeight.Location = new System.Drawing.Point(165, 82);
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
            this.numericUpDownHeight.Size = new System.Drawing.Size(110, 20);
            this.numericUpDownHeight.TabIndex = 26;
            this.numericUpDownHeight.Value = new decimal(new int[] {
            720,
            0,
            0,
            0});
            // 
            // labelWidth
            // 
            this.labelWidth.AutoSize = true;
            this.labelWidth.Location = new System.Drawing.Point(12, 58);
            this.labelWidth.Name = "labelWidth";
            this.labelWidth.Size = new System.Drawing.Size(35, 13);
            this.labelWidth.TabIndex = 27;
            this.labelWidth.Text = "Width";
            // 
            // labelHeight
            // 
            this.labelHeight.AutoSize = true;
            this.labelHeight.Location = new System.Drawing.Point(12, 84);
            this.labelHeight.Name = "labelHeight";
            this.labelHeight.Size = new System.Drawing.Size(38, 13);
            this.labelHeight.TabIndex = 28;
            this.labelHeight.Text = "Height";
            // 
            // groupBoxBackground
            // 
            this.groupBoxBackground.Controls.Add(this.radioButtonColor);
            this.groupBoxBackground.Controls.Add(this.radioButtonCheckeredImage);
            this.groupBoxBackground.Controls.Add(this.buttonColor);
            this.groupBoxBackground.Controls.Add(this.panelColor);
            this.groupBoxBackground.Location = new System.Drawing.Point(15, 118);
            this.groupBoxBackground.Name = "groupBoxBackground";
            this.groupBoxBackground.Size = new System.Drawing.Size(314, 118);
            this.groupBoxBackground.TabIndex = 29;
            this.groupBoxBackground.TabStop = false;
            this.groupBoxBackground.Text = "Background";
            // 
            // radioButtonColor
            // 
            this.radioButtonColor.AutoSize = true;
            this.radioButtonColor.Location = new System.Drawing.Point(18, 57);
            this.radioButtonColor.Name = "radioButtonColor";
            this.radioButtonColor.Size = new System.Drawing.Size(74, 17);
            this.radioButtonColor.TabIndex = 22;
            this.radioButtonColor.Text = "Solid color";
            this.radioButtonColor.UseVisualStyleBackColor = true;
            // 
            // radioButtonCheckeredImage
            // 
            this.radioButtonCheckeredImage.AutoSize = true;
            this.radioButtonCheckeredImage.Checked = true;
            this.radioButtonCheckeredImage.Location = new System.Drawing.Point(18, 34);
            this.radioButtonCheckeredImage.Name = "radioButtonCheckeredImage";
            this.radioButtonCheckeredImage.Size = new System.Drawing.Size(108, 17);
            this.radioButtonCheckeredImage.TabIndex = 21;
            this.radioButtonCheckeredImage.TabStop = true;
            this.radioButtonCheckeredImage.Text = "Checkered image";
            this.radioButtonCheckeredImage.UseVisualStyleBackColor = true;
            // 
            // numericUpDownDurationMinutes
            // 
            this.numericUpDownDurationMinutes.Location = new System.Drawing.Point(165, 30);
            this.numericUpDownDurationMinutes.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownDurationMinutes.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownDurationMinutes.Name = "numericUpDownDurationMinutes";
            this.numericUpDownDurationMinutes.Size = new System.Drawing.Size(110, 20);
            this.numericUpDownDurationMinutes.TabIndex = 30;
            this.numericUpDownDurationMinutes.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // labelPleaseWait
            // 
            this.labelPleaseWait.AutoSize = true;
            this.labelPleaseWait.Location = new System.Drawing.Point(12, 259);
            this.labelPleaseWait.Name = "labelPleaseWait";
            this.labelPleaseWait.Size = new System.Drawing.Size(70, 13);
            this.labelPleaseWait.TabIndex = 31;
            this.labelPleaseWait.Text = "Please wait...";
            // 
            // GenerateVideo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(665, 310);
            this.Controls.Add(this.labelPleaseWait);
            this.Controls.Add(this.numericUpDownDurationMinutes);
            this.Controls.Add(this.groupBoxBackground);
            this.Controls.Add(this.labelHeight);
            this.Controls.Add(this.labelWidth);
            this.Controls.Add(this.numericUpDownHeight);
            this.Controls.Add(this.numericUpDownWidth);
            this.Controls.Add(this.labelDuration);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GenerateVideo";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "GenerateVideo";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GenerateVideo_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownHeight)).EndInit();
            this.groupBoxBackground.ResumeLayout(false);
            this.groupBoxBackground.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDurationMinutes)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Panel panelColor;
        private System.Windows.Forms.Button buttonColor;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label labelDuration;
        private System.Windows.Forms.NumericUpDown numericUpDownWidth;
        private System.Windows.Forms.NumericUpDown numericUpDownHeight;
        private System.Windows.Forms.Label labelWidth;
        private System.Windows.Forms.Label labelHeight;
        private System.Windows.Forms.GroupBox groupBoxBackground;
        private System.Windows.Forms.RadioButton radioButtonColor;
        private System.Windows.Forms.RadioButton radioButtonCheckeredImage;
        private System.Windows.Forms.NumericUpDown numericUpDownDurationMinutes;
        private System.Windows.Forms.Label labelPleaseWait;
    }
}