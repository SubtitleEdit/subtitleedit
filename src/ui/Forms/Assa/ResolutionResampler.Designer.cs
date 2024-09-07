﻿
namespace Nikse.SubtitleEdit.Forms.Assa
{
    sealed partial class ResolutionResampler
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
            this.labelTargetRes = new System.Windows.Forms.Label();
            this.numericUpDownTargetHeight = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.buttonGetResolutionFromVideo = new System.Windows.Forms.Button();
            this.labelX = new System.Windows.Forms.Label();
            this.numericUpDownTargetWidth = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelSourceRes = new System.Windows.Forms.Label();
            this.numericUpDownSourceHeight = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.buttonSourceRes = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.numericUpDownSourceWidth = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.checkBoxMargins = new System.Windows.Forms.CheckBox();
            this.checkBoxFontSize = new System.Windows.Forms.CheckBox();
            this.checkBoxPosition = new System.Windows.Forms.CheckBox();
            this.checkBoxDrawing = new System.Windows.Forms.CheckBox();
            this.buttonNever = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // labelTargetRes
            // 
            this.labelTargetRes.AutoSize = true;
            this.labelTargetRes.Location = new System.Drawing.Point(12, 65);
            this.labelTargetRes.Name = "labelTargetRes";
            this.labelTargetRes.Size = new System.Drawing.Size(115, 13);
            this.labelTargetRes.TabIndex = 7;
            this.labelTargetRes.Text = "Target video resolution";
            // 
            // numericUpDownTargetHeight
            // 
            this.numericUpDownTargetHeight.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownTargetHeight.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownTargetHeight.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownTargetHeight.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownTargetHeight.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownTargetHeight.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownTargetHeight.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownTargetHeight.DecimalPlaces = 0;
            this.numericUpDownTargetHeight.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownTargetHeight.Location = new System.Drawing.Point(265, 63);
            this.numericUpDownTargetHeight.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDownTargetHeight.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.numericUpDownTargetHeight.Name = "numericUpDownTargetHeight";
            this.numericUpDownTargetHeight.Size = new System.Drawing.Size(47, 20);
            this.numericUpDownTargetHeight.TabIndex = 10;
            this.numericUpDownTargetHeight.TabStop = false;
            this.numericUpDownTargetHeight.ThousandsSeparator = false;
            this.numericUpDownTargetHeight.Value = new decimal(new int[] {
            288,
            0,
            0,
            0});
            // 
            // buttonGetResolutionFromVideo
            // 
            this.buttonGetResolutionFromVideo.Location = new System.Drawing.Point(316, 63);
            this.buttonGetResolutionFromVideo.Name = "buttonGetResolutionFromVideo";
            this.buttonGetResolutionFromVideo.Size = new System.Drawing.Size(27, 23);
            this.buttonGetResolutionFromVideo.TabIndex = 11;
            this.buttonGetResolutionFromVideo.Text = "...";
            this.buttonGetResolutionFromVideo.UseVisualStyleBackColor = true;
            this.buttonGetResolutionFromVideo.Click += new System.EventHandler(this.buttonGetResolutionFromVideo_Click);
            // 
            // labelX
            // 
            this.labelX.AutoSize = true;
            this.labelX.Location = new System.Drawing.Point(247, 66);
            this.labelX.Name = "labelX";
            this.labelX.Size = new System.Drawing.Size(14, 13);
            this.labelX.TabIndex = 9;
            this.labelX.Text = "X";
            // 
            // numericUpDownTargetWidth
            // 
            this.numericUpDownTargetWidth.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownTargetWidth.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownTargetWidth.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownTargetWidth.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownTargetWidth.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownTargetWidth.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownTargetWidth.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownTargetWidth.DecimalPlaces = 0;
            this.numericUpDownTargetWidth.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownTargetWidth.Location = new System.Drawing.Point(196, 63);
            this.numericUpDownTargetWidth.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDownTargetWidth.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.numericUpDownTargetWidth.Name = "numericUpDownTargetWidth";
            this.numericUpDownTargetWidth.Size = new System.Drawing.Size(47, 20);
            this.numericUpDownTargetWidth.TabIndex = 8;
            this.numericUpDownTargetWidth.TabStop = false;
            this.numericUpDownTargetWidth.ThousandsSeparator = false;
            this.numericUpDownTargetWidth.Value = new decimal(new int[] {
            384,
            0,
            0,
            0});
            // 
            // labelSourceRes
            // 
            this.labelSourceRes.AutoSize = true;
            this.labelSourceRes.Location = new System.Drawing.Point(12, 39);
            this.labelSourceRes.Name = "labelSourceRes";
            this.labelSourceRes.Size = new System.Drawing.Size(118, 13);
            this.labelSourceRes.TabIndex = 2;
            this.labelSourceRes.Text = "Source video resolution";
            // 
            // numericUpDownSourceHeight
            // 
            this.numericUpDownSourceHeight.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownSourceHeight.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownSourceHeight.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownSourceHeight.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownSourceHeight.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownSourceHeight.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownSourceHeight.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownSourceHeight.DecimalPlaces = 0;
            this.numericUpDownSourceHeight.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownSourceHeight.Location = new System.Drawing.Point(265, 37);
            this.numericUpDownSourceHeight.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDownSourceHeight.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.numericUpDownSourceHeight.Name = "numericUpDownSourceHeight";
            this.numericUpDownSourceHeight.Size = new System.Drawing.Size(47, 20);
            this.numericUpDownSourceHeight.TabIndex = 5;
            this.numericUpDownSourceHeight.TabStop = false;
            this.numericUpDownSourceHeight.ThousandsSeparator = false;
            this.numericUpDownSourceHeight.Value = new decimal(new int[] {
            288,
            0,
            0,
            0});
            // 
            // buttonSourceRes
            // 
            this.buttonSourceRes.Location = new System.Drawing.Point(316, 37);
            this.buttonSourceRes.Name = "buttonSourceRes";
            this.buttonSourceRes.Size = new System.Drawing.Size(27, 23);
            this.buttonSourceRes.TabIndex = 6;
            this.buttonSourceRes.Text = "...";
            this.buttonSourceRes.UseVisualStyleBackColor = true;
            this.buttonSourceRes.Click += new System.EventHandler(this.buttonSourceRes_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(247, 40);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(14, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "X";
            // 
            // numericUpDownSourceWidth
            // 
            this.numericUpDownSourceWidth.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownSourceWidth.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownSourceWidth.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownSourceWidth.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownSourceWidth.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownSourceWidth.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownSourceWidth.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownSourceWidth.DecimalPlaces = 0;
            this.numericUpDownSourceWidth.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownSourceWidth.Location = new System.Drawing.Point(196, 37);
            this.numericUpDownSourceWidth.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDownSourceWidth.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.numericUpDownSourceWidth.Name = "numericUpDownSourceWidth";
            this.numericUpDownSourceWidth.Size = new System.Drawing.Size(47, 20);
            this.numericUpDownSourceWidth.TabIndex = 3;
            this.numericUpDownSourceWidth.TabStop = false;
            this.numericUpDownSourceWidth.ThousandsSeparator = false;
            this.numericUpDownSourceWidth.Value = new decimal(new int[] {
            384,
            0,
            0,
            0});
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(381, 205);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(300, 205);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 0;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // checkBoxMargins
            // 
            this.checkBoxMargins.AutoSize = true;
            this.checkBoxMargins.Checked = true;
            this.checkBoxMargins.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxMargins.Location = new System.Drawing.Point(15, 99);
            this.checkBoxMargins.Name = "checkBoxMargins";
            this.checkBoxMargins.Size = new System.Drawing.Size(165, 17);
            this.checkBoxMargins.TabIndex = 12;
            this.checkBoxMargins.Text = "Change resolution for margins";
            this.checkBoxMargins.UseVisualStyleBackColor = true;
            // 
            // checkBoxFontSize
            // 
            this.checkBoxFontSize.AutoSize = true;
            this.checkBoxFontSize.Checked = true;
            this.checkBoxFontSize.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxFontSize.Location = new System.Drawing.Point(15, 122);
            this.checkBoxFontSize.Name = "checkBoxFontSize";
            this.checkBoxFontSize.Size = new System.Drawing.Size(168, 17);
            this.checkBoxFontSize.TabIndex = 13;
            this.checkBoxFontSize.Text = "Change resolution for font size";
            this.checkBoxFontSize.UseVisualStyleBackColor = true;
            // 
            // checkBoxPosition
            // 
            this.checkBoxPosition.AutoSize = true;
            this.checkBoxPosition.Checked = true;
            this.checkBoxPosition.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxPosition.Location = new System.Drawing.Point(15, 145);
            this.checkBoxPosition.Name = "checkBoxPosition";
            this.checkBoxPosition.Size = new System.Drawing.Size(165, 17);
            this.checkBoxPosition.TabIndex = 14;
            this.checkBoxPosition.Text = "Change resolution for position";
            this.checkBoxPosition.UseVisualStyleBackColor = true;
            // 
            // checkBoxDrawing
            // 
            this.checkBoxDrawing.AutoSize = true;
            this.checkBoxDrawing.Checked = true;
            this.checkBoxDrawing.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxDrawing.Location = new System.Drawing.Point(14, 168);
            this.checkBoxDrawing.Name = "checkBoxDrawing";
            this.checkBoxDrawing.Size = new System.Drawing.Size(166, 17);
            this.checkBoxDrawing.TabIndex = 15;
            this.checkBoxDrawing.Text = "Change resolution for drawing";
            this.checkBoxDrawing.UseVisualStyleBackColor = true;
            // 
            // buttonNever
            // 
            this.buttonNever.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonNever.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonNever.Location = new System.Drawing.Point(166, 205);
            this.buttonNever.Name = "buttonNever";
            this.buttonNever.Size = new System.Drawing.Size(128, 23);
            this.buttonNever.TabIndex = 16;
            this.buttonNever.Text = "No, never";
            this.buttonNever.UseVisualStyleBackColor = true;
            this.buttonNever.Click += new System.EventHandler(this.buttonNever_Click);
            // 
            // ResolutionResampler
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(468, 240);
            this.Controls.Add(this.buttonNever);
            this.Controls.Add(this.checkBoxDrawing);
            this.Controls.Add(this.checkBoxPosition);
            this.Controls.Add(this.checkBoxFontSize);
            this.Controls.Add(this.checkBoxMargins);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.labelSourceRes);
            this.Controls.Add(this.numericUpDownSourceHeight);
            this.Controls.Add(this.buttonSourceRes);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.numericUpDownSourceWidth);
            this.Controls.Add(this.labelTargetRes);
            this.Controls.Add(this.numericUpDownTargetHeight);
            this.Controls.Add(this.buttonGetResolutionFromVideo);
            this.Controls.Add(this.labelX);
            this.Controls.Add(this.numericUpDownTargetWidth);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ResolutionResampler";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ResolutionResampler";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ResolutionResampler_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelTargetRes;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownTargetHeight;
        private System.Windows.Forms.Button buttonGetResolutionFromVideo;
        private System.Windows.Forms.Label labelX;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownTargetWidth;
        private System.Windows.Forms.Label labelSourceRes;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownSourceHeight;
        private System.Windows.Forms.Button buttonSourceRes;
        private System.Windows.Forms.Label label2;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownSourceWidth;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.CheckBox checkBoxMargins;
        private System.Windows.Forms.CheckBox checkBoxFontSize;
        private System.Windows.Forms.CheckBox checkBoxPosition;
        private System.Windows.Forms.CheckBox checkBoxDrawing;
        private System.Windows.Forms.Button buttonNever;
    }
}