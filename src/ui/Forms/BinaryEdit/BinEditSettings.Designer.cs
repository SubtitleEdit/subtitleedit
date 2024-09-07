﻿
namespace Nikse.SubtitleEdit.Forms.BinaryEdit
{
    sealed partial class BinEditSettings
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
            this.panelBackgroundColor = new System.Windows.Forms.Panel();
            this.buttonBackgroundColor = new System.Windows.Forms.Button();
            this.panelImageBackgroundColor = new System.Windows.Forms.Panel();
            this.buttonImageBackgroundColor = new System.Windows.Forms.Button();
            this.groupBoxMargins = new System.Windows.Forms.GroupBox();
            this.numericUpDownMarginTop = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.numericUpDownMarginRight = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.numericUpDownMarginLeft = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelMarginTop = new System.Windows.Forms.Label();
            this.labelMarginRight = new System.Windows.Forms.Label();
            this.labelMarginLeft = new System.Windows.Forms.Label();
            this.groupBoxColors = new System.Windows.Forms.GroupBox();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.numericUpDownMarginBottom = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelMarginBottom = new System.Windows.Forms.Label();
            this.groupBoxMargins.SuspendLayout();
            this.groupBoxColors.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelBackgroundColor
            // 
            this.panelBackgroundColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelBackgroundColor.Location = new System.Drawing.Point(221, 20);
            this.panelBackgroundColor.Name = "panelBackgroundColor";
            this.panelBackgroundColor.Size = new System.Drawing.Size(21, 20);
            this.panelBackgroundColor.TabIndex = 19;
            this.panelBackgroundColor.Click += new System.EventHandler(this.buttonBackgroundColor_Click);
            // 
            // buttonBackgroundColor
            // 
            this.buttonBackgroundColor.Location = new System.Drawing.Point(6, 19);
            this.buttonBackgroundColor.Name = "buttonBackgroundColor";
            this.buttonBackgroundColor.Size = new System.Drawing.Size(209, 23);
            this.buttonBackgroundColor.TabIndex = 20;
            this.buttonBackgroundColor.Text = "Background screen color";
            this.buttonBackgroundColor.UseVisualStyleBackColor = true;
            this.buttonBackgroundColor.Click += new System.EventHandler(this.buttonBackgroundColor_Click);
            // 
            // panelImageBackgroundColor
            // 
            this.panelImageBackgroundColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelImageBackgroundColor.Location = new System.Drawing.Point(221, 49);
            this.panelImageBackgroundColor.Name = "panelImageBackgroundColor";
            this.panelImageBackgroundColor.Size = new System.Drawing.Size(21, 20);
            this.panelImageBackgroundColor.TabIndex = 21;
            this.panelImageBackgroundColor.Click += new System.EventHandler(this.buttonImageBackgroundColor_Click);
            // 
            // buttonImageBackgroundColor
            // 
            this.buttonImageBackgroundColor.Location = new System.Drawing.Point(6, 48);
            this.buttonImageBackgroundColor.Name = "buttonImageBackgroundColor";
            this.buttonImageBackgroundColor.Size = new System.Drawing.Size(209, 23);
            this.buttonImageBackgroundColor.TabIndex = 22;
            this.buttonImageBackgroundColor.Text = "Image background color";
            this.buttonImageBackgroundColor.UseVisualStyleBackColor = true;
            this.buttonImageBackgroundColor.Click += new System.EventHandler(this.buttonImageBackgroundColor_Click);
            // 
            // groupBoxMargins
            // 
            this.groupBoxMargins.Controls.Add(this.numericUpDownMarginBottom);
            this.groupBoxMargins.Controls.Add(this.labelMarginBottom);
            this.groupBoxMargins.Controls.Add(this.numericUpDownMarginTop);
            this.groupBoxMargins.Controls.Add(this.numericUpDownMarginRight);
            this.groupBoxMargins.Controls.Add(this.numericUpDownMarginLeft);
            this.groupBoxMargins.Controls.Add(this.labelMarginTop);
            this.groupBoxMargins.Controls.Add(this.labelMarginRight);
            this.groupBoxMargins.Controls.Add(this.labelMarginLeft);
            this.groupBoxMargins.Location = new System.Drawing.Point(7, 134);
            this.groupBoxMargins.Name = "groupBoxMargins";
            this.groupBoxMargins.Size = new System.Drawing.Size(384, 65);
            this.groupBoxMargins.TabIndex = 23;
            this.groupBoxMargins.TabStop = false;
            this.groupBoxMargins.Text = "Margins";
            // 
            // numericUpDownMarginTop
            // 
            this.numericUpDownMarginTop.Location = new System.Drawing.Point(188, 33);
            this.numericUpDownMarginTop.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownMarginTop.Name = "numericUpDownMarginTop";
            this.numericUpDownMarginTop.Size = new System.Drawing.Size(44, 20);
            this.numericUpDownMarginTop.TabIndex = 5;
            // 
            // numericUpDownMarginRight
            // 
            this.numericUpDownMarginRight.Location = new System.Drawing.Point(101, 33);
            this.numericUpDownMarginRight.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownMarginRight.Name = "numericUpDownMarginRight";
            this.numericUpDownMarginRight.Size = new System.Drawing.Size(44, 20);
            this.numericUpDownMarginRight.TabIndex = 3;
            // 
            // numericUpDownMarginLeft
            // 
            this.numericUpDownMarginLeft.Location = new System.Drawing.Point(16, 33);
            this.numericUpDownMarginLeft.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownMarginLeft.Name = "numericUpDownMarginLeft";
            this.numericUpDownMarginLeft.Size = new System.Drawing.Size(44, 20);
            this.numericUpDownMarginLeft.TabIndex = 1;
            // 
            // labelMarginTop
            // 
            this.labelMarginTop.AutoSize = true;
            this.labelMarginTop.Location = new System.Drawing.Point(185, 16);
            this.labelMarginTop.Name = "labelMarginTop";
            this.labelMarginTop.Size = new System.Drawing.Size(26, 13);
            this.labelMarginTop.TabIndex = 4;
            this.labelMarginTop.Text = "Top";
            // 
            // labelMarginRight
            // 
            this.labelMarginRight.AutoSize = true;
            this.labelMarginRight.Location = new System.Drawing.Point(98, 16);
            this.labelMarginRight.Name = "labelMarginRight";
            this.labelMarginRight.Size = new System.Drawing.Size(32, 13);
            this.labelMarginRight.TabIndex = 2;
            this.labelMarginRight.Text = "Right";
            // 
            // labelMarginLeft
            // 
            this.labelMarginLeft.AutoSize = true;
            this.labelMarginLeft.Location = new System.Drawing.Point(13, 16);
            this.labelMarginLeft.Name = "labelMarginLeft";
            this.labelMarginLeft.Size = new System.Drawing.Size(25, 13);
            this.labelMarginLeft.TabIndex = 0;
            this.labelMarginLeft.Text = "Left";
            // 
            // groupBoxColors
            // 
            this.groupBoxColors.Controls.Add(this.buttonBackgroundColor);
            this.groupBoxColors.Controls.Add(this.panelBackgroundColor);
            this.groupBoxColors.Controls.Add(this.panelImageBackgroundColor);
            this.groupBoxColors.Controls.Add(this.buttonImageBackgroundColor);
            this.groupBoxColors.Location = new System.Drawing.Point(7, 12);
            this.groupBoxColors.Name = "groupBoxColors";
            this.groupBoxColors.Size = new System.Drawing.Size(384, 116);
            this.groupBoxColors.TabIndex = 24;
            this.groupBoxColors.TabStop = false;
            this.groupBoxColors.Text = "Colors";
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(233, 214);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 25;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(317, 214);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 26;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // numericUpDownMarginBottom
            // 
            this.numericUpDownMarginBottom.Location = new System.Drawing.Point(274, 33);
            this.numericUpDownMarginBottom.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownMarginBottom.Name = "numericUpDownMarginBottom";
            this.numericUpDownMarginBottom.Size = new System.Drawing.Size(44, 20);
            this.numericUpDownMarginBottom.TabIndex = 7;
            // 
            // labelMarginBottom
            // 
            this.labelMarginBottom.AutoSize = true;
            this.labelMarginBottom.Location = new System.Drawing.Point(271, 16);
            this.labelMarginBottom.Name = "labelMarginBottom";
            this.labelMarginBottom.Size = new System.Drawing.Size(40, 13);
            this.labelMarginBottom.TabIndex = 6;
            this.labelMarginBottom.Text = "Bottom";
            // 
            // BinEditSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(404, 249);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.groupBoxColors);
            this.Controls.Add(this.groupBoxMargins);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BinEditSettings";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "BinEditSettings";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.BinEditSettings_KeyDown);
            this.groupBoxMargins.ResumeLayout(false);
            this.groupBoxMargins.PerformLayout();
            this.groupBoxColors.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelBackgroundColor;
        private System.Windows.Forms.Button buttonBackgroundColor;
        private System.Windows.Forms.Panel panelImageBackgroundColor;
        private System.Windows.Forms.Button buttonImageBackgroundColor;
        private System.Windows.Forms.GroupBox groupBoxMargins;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownMarginTop;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownMarginRight;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownMarginLeft;
        private System.Windows.Forms.Label labelMarginTop;
        private System.Windows.Forms.Label labelMarginRight;
        private System.Windows.Forms.Label labelMarginLeft;
        private System.Windows.Forms.GroupBox groupBoxColors;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownMarginBottom;
        private System.Windows.Forms.Label labelMarginBottom;
    }
}