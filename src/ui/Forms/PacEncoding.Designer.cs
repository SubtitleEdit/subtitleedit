﻿namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class PacEncoding
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
            this.comboBoxCodePage = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelCodePage = new System.Windows.Forms.Label();
            this.labelPreview = new System.Windows.Forms.Label();
            this.textBoxPreview = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // comboBoxCodePage
            // 
            this.comboBoxCodePage.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxCodePage.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxCodePage.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxCodePage.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxCodePage.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxCodePage.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxCodePage.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxCodePage.DropDownHeight = 400;
            this.comboBoxCodePage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxCodePage.DropDownWidth = 203;
            this.comboBoxCodePage.FormattingEnabled = true;
            this.comboBoxCodePage.Items.AddRange(new string[] {
            "Latin",
            "Greek",
            "Latin Czech",
            "Arabic",
            "Hebrew",
            "Thai",
            "Cyrillic",
            "Chinese Traditional (Big5)",
            "Chinese Simplified (gb2312)",
            "Korean",
            "Japanese",
            "Portuguese"});
            this.comboBoxCodePage.Location = new System.Drawing.Point(12, 37);
            this.comboBoxCodePage.MaxLength = 32767;
            this.comboBoxCodePage.Name = "comboBoxCodePage";
            this.comboBoxCodePage.SelectedIndex = -1;
            this.comboBoxCodePage.SelectedItem = null;
            this.comboBoxCodePage.SelectedText = "";
            this.comboBoxCodePage.Size = new System.Drawing.Size(203, 21);
            this.comboBoxCodePage.TabIndex = 0;
            this.comboBoxCodePage.UsePopupWindow = false;
            this.comboBoxCodePage.SelectedIndexChanged += new System.EventHandler(this.comboBoxCodePage_SelectedIndexChanged);
            // 
            // labelCodePage
            // 
            this.labelCodePage.AutoSize = true;
            this.labelCodePage.Location = new System.Drawing.Point(9, 21);
            this.labelCodePage.Name = "labelCodePage";
            this.labelCodePage.Size = new System.Drawing.Size(155, 13);
            this.labelCodePage.TabIndex = 1;
            this.labelCodePage.Text = "Please choose PAC code page";
            // 
            // labelPreview
            // 
            this.labelPreview.AutoSize = true;
            this.labelPreview.Location = new System.Drawing.Point(9, 73);
            this.labelPreview.Name = "labelPreview";
            this.labelPreview.Size = new System.Drawing.Size(122, 13);
            this.labelPreview.TabIndex = 2;
            this.labelPreview.Text = "PAC code page preview";
            // 
            // textBoxPreview
            // 
            this.textBoxPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxPreview.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxPreview.Location = new System.Drawing.Point(12, 89);
            this.textBoxPreview.Multiline = true;
            this.textBoxPreview.Name = "textBoxPreview";
            this.textBoxPreview.ReadOnly = true;
            this.textBoxPreview.Size = new System.Drawing.Size(361, 115);
            this.textBoxPreview.TabIndex = 3;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(298, 213);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(217, 213);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 4;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // PacEncoding
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(385, 248);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.textBoxPreview);
            this.Controls.Add(this.labelPreview);
            this.Controls.Add(this.labelCodePage);
            this.Controls.Add(this.comboBoxCodePage);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PacEncoding";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "PacEncoding";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PacEncoding_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxCodePage;
        private System.Windows.Forms.Label labelCodePage;
        private System.Windows.Forms.Label labelPreview;
        private Nikse.SubtitleEdit.Controls.NikseTextBox textBoxPreview;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;

    }
}