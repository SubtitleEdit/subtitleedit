﻿namespace Nikse.SubtitleEdit.Forms.DCinema
{
    partial class DCinemaPropertiesInterop
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
            this.components = new System.ComponentModel.Container();
            this.panelFontEffectColor = new System.Windows.Forms.Panel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBoxGenerateIdAuto = new System.Windows.Forms.CheckBox();
            this.comboBoxLanguage = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelLanguage = new System.Windows.Forms.Label();
            this.numericUpDownReelNumber = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.buttonGenerateID = new System.Windows.Forms.Button();
            this.groupBoxFont = new System.Windows.Forms.GroupBox();
            this.numericUpDownFadeDown = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelFadeDownTime = new System.Windows.Forms.Label();
            this.numericUpDownFadeUp = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelFadeUpTime = new System.Windows.Forms.Label();
            this.labelZPositionHelp = new System.Windows.Forms.Label();
            this.numericUpDownZPosition = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelZPosition = new System.Windows.Forms.Label();
            this.numericUpDownTopBottomMargin = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelTopBottomMargin = new System.Windows.Forms.Label();
            this.buttonFontEffectColor = new System.Windows.Forms.Button();
            this.panelFontColor = new System.Windows.Forms.Panel();
            this.buttonFontColor = new System.Windows.Forms.Button();
            this.labelEffectColor = new System.Windows.Forms.Label();
            this.numericUpDownFontSize = new Nikse.SubtitleEdit.Controls.NikseUpDown();
            this.labelFontSize = new System.Windows.Forms.Label();
            this.comboBoxFontEffect = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelEffect = new System.Windows.Forms.Label();
            this.labelFontColor = new System.Windows.Forms.Label();
            this.textBoxFontID = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.labelFontId = new System.Windows.Forms.Label();
            this.textBoxFontUri = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.labelFontUri = new System.Windows.Forms.Label();
            this.labelReelNumber = new System.Windows.Forms.Label();
            this.textBoxMovieTitle = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.labelMovieTitle = new System.Windows.Forms.Label();
            this.textBoxSubtitleID = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.labelSubtitleID = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.contextMenuStripProfile = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.profilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBox1.SuspendLayout();
            this.groupBoxFont.SuspendLayout();
            this.contextMenuStripProfile.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelFontEffectColor
            // 
            this.panelFontEffectColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelFontEffectColor.Location = new System.Drawing.Point(255, 128);
            this.panelFontEffectColor.Name = "panelFontEffectColor";
            this.panelFontEffectColor.Size = new System.Drawing.Size(21, 20);
            this.panelFontEffectColor.TabIndex = 6;
            this.panelFontEffectColor.Click += new System.EventHandler(this.buttonFontEffectColor_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.checkBoxGenerateIdAuto);
            this.groupBox1.Controls.Add(this.comboBoxLanguage);
            this.groupBox1.Controls.Add(this.labelLanguage);
            this.groupBox1.Controls.Add(this.numericUpDownReelNumber);
            this.groupBox1.Controls.Add(this.buttonGenerateID);
            this.groupBox1.Controls.Add(this.groupBoxFont);
            this.groupBox1.Controls.Add(this.labelReelNumber);
            this.groupBox1.Controls.Add(this.textBoxMovieTitle);
            this.groupBox1.Controls.Add(this.labelMovieTitle);
            this.groupBox1.Controls.Add(this.textBoxSubtitleID);
            this.groupBox1.Controls.Add(this.labelSubtitleID);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(660, 505);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            // 
            // checkBoxGenerateIdAuto
            // 
            this.checkBoxGenerateIdAuto.AutoSize = true;
            this.checkBoxGenerateIdAuto.Location = new System.Drawing.Point(139, 57);
            this.checkBoxGenerateIdAuto.Name = "checkBoxGenerateIdAuto";
            this.checkBoxGenerateIdAuto.Size = new System.Drawing.Size(148, 17);
            this.checkBoxGenerateIdAuto.TabIndex = 33;
            this.checkBoxGenerateIdAuto.Text = "Generate new ID on save";
            this.checkBoxGenerateIdAuto.UseVisualStyleBackColor = true;
            // 
            // comboBoxLanguage
            // 
            this.comboBoxLanguage.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxLanguage.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxLanguage.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxLanguage.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxLanguage.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxLanguage.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxLanguage.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxLanguage.DropDownHeight = 400;
            this.comboBoxLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.comboBoxLanguage.DropDownWidth = 233;
            this.comboBoxLanguage.FormattingEnabled = true;
            this.comboBoxLanguage.Location = new System.Drawing.Point(139, 149);
            this.comboBoxLanguage.MaxLength = 32767;
            this.comboBoxLanguage.Name = "comboBoxLanguage";
            this.comboBoxLanguage.SelectedIndex = -1;
            this.comboBoxLanguage.SelectedItem = null;
            this.comboBoxLanguage.SelectedText = "";
            this.comboBoxLanguage.Size = new System.Drawing.Size(233, 21);
            this.comboBoxLanguage.TabIndex = 4;
            this.comboBoxLanguage.TabStop = false;
            this.comboBoxLanguage.UsePopupWindow = false;
            // 
            // labelLanguage
            // 
            this.labelLanguage.AutoSize = true;
            this.labelLanguage.Location = new System.Drawing.Point(15, 152);
            this.labelLanguage.Name = "labelLanguage";
            this.labelLanguage.Size = new System.Drawing.Size(55, 13);
            this.labelLanguage.TabIndex = 32;
            this.labelLanguage.Text = "Language";
            // 
            // numericUpDownReelNumber
            // 
            this.numericUpDownReelNumber.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownReelNumber.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownReelNumber.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownReelNumber.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownReelNumber.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownReelNumber.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownReelNumber.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownReelNumber.DecimalPlaces = 0;
            this.numericUpDownReelNumber.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownReelNumber.Location = new System.Drawing.Point(139, 123);
            this.numericUpDownReelNumber.Maximum = new decimal(new int[] {
            250,
            0,
            0,
            0});
            this.numericUpDownReelNumber.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownReelNumber.Name = "numericUpDownReelNumber";
            this.numericUpDownReelNumber.Size = new System.Drawing.Size(112, 20);
            this.numericUpDownReelNumber.TabIndex = 3;
            this.numericUpDownReelNumber.TabStop = false;
            this.numericUpDownReelNumber.ThousandsSeparator = false;
            this.numericUpDownReelNumber.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // buttonGenerateID
            // 
            this.buttonGenerateID.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonGenerateID.Location = new System.Drawing.Point(371, 29);
            this.buttonGenerateID.Name = "buttonGenerateID";
            this.buttonGenerateID.Size = new System.Drawing.Size(100, 23);
            this.buttonGenerateID.TabIndex = 1;
            this.buttonGenerateID.Text = "Generate ID";
            this.buttonGenerateID.UseVisualStyleBackColor = true;
            this.buttonGenerateID.Click += new System.EventHandler(this.buttonGenerateID_Click);
            // 
            // groupBoxFont
            // 
            this.groupBoxFont.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxFont.Controls.Add(this.numericUpDownFadeDown);
            this.groupBoxFont.Controls.Add(this.labelFadeDownTime);
            this.groupBoxFont.Controls.Add(this.numericUpDownFadeUp);
            this.groupBoxFont.Controls.Add(this.labelFadeUpTime);
            this.groupBoxFont.Controls.Add(this.labelZPositionHelp);
            this.groupBoxFont.Controls.Add(this.numericUpDownZPosition);
            this.groupBoxFont.Controls.Add(this.labelZPosition);
            this.groupBoxFont.Controls.Add(this.numericUpDownTopBottomMargin);
            this.groupBoxFont.Controls.Add(this.labelTopBottomMargin);
            this.groupBoxFont.Controls.Add(this.panelFontEffectColor);
            this.groupBoxFont.Controls.Add(this.buttonFontEffectColor);
            this.groupBoxFont.Controls.Add(this.panelFontColor);
            this.groupBoxFont.Controls.Add(this.buttonFontColor);
            this.groupBoxFont.Controls.Add(this.labelEffectColor);
            this.groupBoxFont.Controls.Add(this.numericUpDownFontSize);
            this.groupBoxFont.Controls.Add(this.labelFontSize);
            this.groupBoxFont.Controls.Add(this.comboBoxFontEffect);
            this.groupBoxFont.Controls.Add(this.labelEffect);
            this.groupBoxFont.Controls.Add(this.labelFontColor);
            this.groupBoxFont.Controls.Add(this.textBoxFontID);
            this.groupBoxFont.Controls.Add(this.labelFontId);
            this.groupBoxFont.Controls.Add(this.textBoxFontUri);
            this.groupBoxFont.Controls.Add(this.labelFontUri);
            this.groupBoxFont.Location = new System.Drawing.Point(6, 185);
            this.groupBoxFont.Name = "groupBoxFont";
            this.groupBoxFont.Size = new System.Drawing.Size(648, 314);
            this.groupBoxFont.TabIndex = 5;
            this.groupBoxFont.TabStop = false;
            this.groupBoxFont.Text = "Font";
            // 
            // numericUpDownFadeDown
            // 
            this.numericUpDownFadeDown.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownFadeDown.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownFadeDown.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownFadeDown.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownFadeDown.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownFadeDown.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownFadeDown.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownFadeDown.DecimalPlaces = 0;
            this.numericUpDownFadeDown.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownFadeDown.Location = new System.Drawing.Point(133, 233);
            this.numericUpDownFadeDown.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numericUpDownFadeDown.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.numericUpDownFadeDown.Name = "numericUpDownFadeDown";
            this.numericUpDownFadeDown.Size = new System.Drawing.Size(112, 20);
            this.numericUpDownFadeDown.TabIndex = 33;
            this.numericUpDownFadeDown.TabStop = false;
            this.numericUpDownFadeDown.ThousandsSeparator = false;
            this.numericUpDownFadeDown.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            // 
            // labelFadeDownTime
            // 
            this.labelFadeDownTime.AutoSize = true;
            this.labelFadeDownTime.Location = new System.Drawing.Point(9, 236);
            this.labelFadeDownTime.Name = "labelFadeDownTime";
            this.labelFadeDownTime.Size = new System.Drawing.Size(82, 13);
            this.labelFadeDownTime.TabIndex = 37;
            this.labelFadeDownTime.Text = "Fade down time";
            // 
            // numericUpDownFadeUp
            // 
            this.numericUpDownFadeUp.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownFadeUp.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownFadeUp.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownFadeUp.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownFadeUp.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownFadeUp.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownFadeUp.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownFadeUp.DecimalPlaces = 0;
            this.numericUpDownFadeUp.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownFadeUp.Location = new System.Drawing.Point(133, 207);
            this.numericUpDownFadeUp.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numericUpDownFadeUp.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.numericUpDownFadeUp.Name = "numericUpDownFadeUp";
            this.numericUpDownFadeUp.Size = new System.Drawing.Size(112, 20);
            this.numericUpDownFadeUp.TabIndex = 32;
            this.numericUpDownFadeUp.TabStop = false;
            this.numericUpDownFadeUp.ThousandsSeparator = false;
            this.numericUpDownFadeUp.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            // 
            // labelFadeUpTime
            // 
            this.labelFadeUpTime.AutoSize = true;
            this.labelFadeUpTime.Location = new System.Drawing.Point(9, 210);
            this.labelFadeUpTime.Name = "labelFadeUpTime";
            this.labelFadeUpTime.Size = new System.Drawing.Size(68, 13);
            this.labelFadeUpTime.TabIndex = 35;
            this.labelFadeUpTime.Text = "Fade up time";
            // 
            // labelZPositionHelp
            // 
            this.labelZPositionHelp.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelZPositionHelp.ForeColor = System.Drawing.Color.Gray;
            this.labelZPositionHelp.Location = new System.Drawing.Point(252, 265);
            this.labelZPositionHelp.Name = "labelZPositionHelp";
            this.labelZPositionHelp.Size = new System.Drawing.Size(343, 47);
            this.labelZPositionHelp.TabIndex = 34;
            this.labelZPositionHelp.Text = "Positive numbers move text away, negative numbers move text closer, if z-position" +
    " is zero then it\'s normal 2D";
            // 
            // numericUpDownZPosition
            // 
            this.numericUpDownZPosition.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownZPosition.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownZPosition.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownZPosition.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownZPosition.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownZPosition.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownZPosition.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownZPosition.DecimalPlaces = 2;
            this.numericUpDownZPosition.Increment = new decimal(new int[] {
            5,
            0,
            0,
            131072});
            this.numericUpDownZPosition.Location = new System.Drawing.Point(133, 259);
            this.numericUpDownZPosition.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDownZPosition.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            -2147483648});
            this.numericUpDownZPosition.Name = "numericUpDownZPosition";
            this.numericUpDownZPosition.Size = new System.Drawing.Size(112, 20);
            this.numericUpDownZPosition.TabIndex = 34;
            this.numericUpDownZPosition.TabStop = false;
            this.numericUpDownZPosition.ThousandsSeparator = false;
            this.numericUpDownZPosition.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            // 
            // labelZPosition
            // 
            this.labelZPosition.AutoSize = true;
            this.labelZPosition.Location = new System.Drawing.Point(9, 262);
            this.labelZPosition.Name = "labelZPosition";
            this.labelZPosition.Size = new System.Drawing.Size(53, 13);
            this.labelZPosition.TabIndex = 32;
            this.labelZPosition.Text = "Z-position";
            // 
            // numericUpDownTopBottomMargin
            // 
            this.numericUpDownTopBottomMargin.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownTopBottomMargin.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownTopBottomMargin.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownTopBottomMargin.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownTopBottomMargin.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownTopBottomMargin.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownTopBottomMargin.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownTopBottomMargin.DecimalPlaces = 0;
            this.numericUpDownTopBottomMargin.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownTopBottomMargin.Location = new System.Drawing.Point(133, 181);
            this.numericUpDownTopBottomMargin.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numericUpDownTopBottomMargin.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownTopBottomMargin.Name = "numericUpDownTopBottomMargin";
            this.numericUpDownTopBottomMargin.Size = new System.Drawing.Size(112, 20);
            this.numericUpDownTopBottomMargin.TabIndex = 31;
            this.numericUpDownTopBottomMargin.TabStop = false;
            this.numericUpDownTopBottomMargin.ThousandsSeparator = false;
            this.numericUpDownTopBottomMargin.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // labelTopBottomMargin
            // 
            this.labelTopBottomMargin.AutoSize = true;
            this.labelTopBottomMargin.Location = new System.Drawing.Point(9, 184);
            this.labelTopBottomMargin.Name = "labelTopBottomMargin";
            this.labelTopBottomMargin.Size = new System.Drawing.Size(97, 13);
            this.labelTopBottomMargin.TabIndex = 30;
            this.labelTopBottomMargin.Text = "Top/bottom margin";
            // 
            // buttonFontEffectColor
            // 
            this.buttonFontEffectColor.Location = new System.Drawing.Point(133, 128);
            this.buttonFontEffectColor.Name = "buttonFontEffectColor";
            this.buttonFontEffectColor.Size = new System.Drawing.Size(112, 23);
            this.buttonFontEffectColor.TabIndex = 5;
            this.buttonFontEffectColor.Text = "Choose color";
            this.buttonFontEffectColor.UseVisualStyleBackColor = true;
            this.buttonFontEffectColor.Click += new System.EventHandler(this.buttonFontEffectColor_Click);
            // 
            // panelFontColor
            // 
            this.panelFontColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelFontColor.Location = new System.Drawing.Point(255, 75);
            this.panelFontColor.Name = "panelFontColor";
            this.panelFontColor.Size = new System.Drawing.Size(21, 20);
            this.panelFontColor.TabIndex = 3;
            this.panelFontColor.Click += new System.EventHandler(this.buttonFontColor_Click);
            // 
            // buttonFontColor
            // 
            this.buttonFontColor.Location = new System.Drawing.Point(133, 75);
            this.buttonFontColor.Name = "buttonFontColor";
            this.buttonFontColor.Size = new System.Drawing.Size(112, 23);
            this.buttonFontColor.TabIndex = 2;
            this.buttonFontColor.Text = "Choose color";
            this.buttonFontColor.UseVisualStyleBackColor = true;
            this.buttonFontColor.Click += new System.EventHandler(this.buttonFontColor_Click);
            // 
            // labelEffectColor
            // 
            this.labelEffectColor.AutoSize = true;
            this.labelEffectColor.Location = new System.Drawing.Point(9, 132);
            this.labelEffectColor.Name = "labelEffectColor";
            this.labelEffectColor.Size = new System.Drawing.Size(61, 13);
            this.labelEffectColor.TabIndex = 27;
            this.labelEffectColor.Text = "Effect color";
            // 
            // numericUpDownFontSize
            // 
            this.numericUpDownFontSize.BackColor = System.Drawing.SystemColors.Window;
            this.numericUpDownFontSize.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.numericUpDownFontSize.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.numericUpDownFontSize.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.numericUpDownFontSize.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.numericUpDownFontSize.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.numericUpDownFontSize.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.numericUpDownFontSize.DecimalPlaces = 0;
            this.numericUpDownFontSize.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownFontSize.Location = new System.Drawing.Point(133, 155);
            this.numericUpDownFontSize.Maximum = new decimal(new int[] {
            250,
            0,
            0,
            0});
            this.numericUpDownFontSize.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.numericUpDownFontSize.Name = "numericUpDownFontSize";
            this.numericUpDownFontSize.Size = new System.Drawing.Size(112, 20);
            this.numericUpDownFontSize.TabIndex = 7;
            this.numericUpDownFontSize.TabStop = false;
            this.numericUpDownFontSize.ThousandsSeparator = false;
            this.numericUpDownFontSize.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            // 
            // labelFontSize
            // 
            this.labelFontSize.AutoSize = true;
            this.labelFontSize.Location = new System.Drawing.Point(9, 162);
            this.labelFontSize.Name = "labelFontSize";
            this.labelFontSize.Size = new System.Drawing.Size(27, 13);
            this.labelFontSize.TabIndex = 25;
            this.labelFontSize.Text = "Size";
            // 
            // comboBoxFontEffect
            // 
            this.comboBoxFontEffect.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxFontEffect.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxFontEffect.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxFontEffect.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxFontEffect.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxFontEffect.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxFontEffect.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxFontEffect.DropDownHeight = 400;
            this.comboBoxFontEffect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxFontEffect.DropDownWidth = 112;
            this.comboBoxFontEffect.FormattingEnabled = true;
            this.comboBoxFontEffect.Items.AddRange(new string[] {
            "None",
            "Border",
            "Shadow"});
            this.comboBoxFontEffect.Location = new System.Drawing.Point(133, 102);
            this.comboBoxFontEffect.MaxLength = 32767;
            this.comboBoxFontEffect.Name = "comboBoxFontEffect";
            this.comboBoxFontEffect.SelectedIndex = -1;
            this.comboBoxFontEffect.SelectedItem = null;
            this.comboBoxFontEffect.SelectedText = "";
            this.comboBoxFontEffect.Size = new System.Drawing.Size(112, 21);
            this.comboBoxFontEffect.TabIndex = 4;
            this.comboBoxFontEffect.UsePopupWindow = false;
            // 
            // labelEffect
            // 
            this.labelEffect.AutoSize = true;
            this.labelEffect.Location = new System.Drawing.Point(9, 105);
            this.labelEffect.Name = "labelEffect";
            this.labelEffect.Size = new System.Drawing.Size(35, 13);
            this.labelEffect.TabIndex = 22;
            this.labelEffect.Text = "Effect";
            // 
            // labelFontColor
            // 
            this.labelFontColor.AutoSize = true;
            this.labelFontColor.Location = new System.Drawing.Point(9, 79);
            this.labelFontColor.Name = "labelFontColor";
            this.labelFontColor.Size = new System.Drawing.Size(31, 13);
            this.labelFontColor.TabIndex = 18;
            this.labelFontColor.Text = "Color";
            // 
            // textBoxFontID
            // 
            this.textBoxFontID.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxFontID.Location = new System.Drawing.Point(133, 24);
            this.textBoxFontID.Name = "textBoxFontID";
            this.textBoxFontID.Size = new System.Drawing.Size(224, 20);
            this.textBoxFontID.TabIndex = 0;
            this.textBoxFontID.Text = "Freds_Font";
            // 
            // labelFontId
            // 
            this.labelFontId.AutoSize = true;
            this.labelFontId.Location = new System.Drawing.Point(9, 27);
            this.labelFontId.Name = "labelFontId";
            this.labelFontId.Size = new System.Drawing.Size(18, 13);
            this.labelFontId.TabIndex = 16;
            this.labelFontId.Text = "ID";
            // 
            // textBoxFontUri
            // 
            this.textBoxFontUri.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxFontUri.Location = new System.Drawing.Point(133, 50);
            this.textBoxFontUri.Name = "textBoxFontUri";
            this.textBoxFontUri.Size = new System.Drawing.Size(224, 20);
            this.textBoxFontUri.TabIndex = 1;
            this.textBoxFontUri.Text = "Fred.ttf";
            // 
            // labelFontUri
            // 
            this.labelFontUri.AutoSize = true;
            this.labelFontUri.Location = new System.Drawing.Point(9, 53);
            this.labelFontUri.Name = "labelFontUri";
            this.labelFontUri.Size = new System.Drawing.Size(26, 13);
            this.labelFontUri.TabIndex = 14;
            this.labelFontUri.Text = "URI";
            // 
            // labelReelNumber
            // 
            this.labelReelNumber.AutoSize = true;
            this.labelReelNumber.Location = new System.Drawing.Point(15, 125);
            this.labelReelNumber.Name = "labelReelNumber";
            this.labelReelNumber.Size = new System.Drawing.Size(67, 13);
            this.labelReelNumber.TabIndex = 4;
            this.labelReelNumber.Text = "Reel number";
            // 
            // textBoxMovieTitle
            // 
            this.textBoxMovieTitle.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxMovieTitle.Location = new System.Drawing.Point(139, 96);
            this.textBoxMovieTitle.Name = "textBoxMovieTitle";
            this.textBoxMovieTitle.Size = new System.Drawing.Size(224, 20);
            this.textBoxMovieTitle.TabIndex = 2;
            // 
            // labelMovieTitle
            // 
            this.labelMovieTitle.AutoSize = true;
            this.labelMovieTitle.Location = new System.Drawing.Point(15, 99);
            this.labelMovieTitle.Name = "labelMovieTitle";
            this.labelMovieTitle.Size = new System.Drawing.Size(55, 13);
            this.labelMovieTitle.TabIndex = 2;
            this.labelMovieTitle.Text = "Movie title";
            // 
            // textBoxSubtitleID
            // 
            this.textBoxSubtitleID.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxSubtitleID.Location = new System.Drawing.Point(139, 30);
            this.textBoxSubtitleID.Name = "textBoxSubtitleID";
            this.textBoxSubtitleID.Size = new System.Drawing.Size(224, 20);
            this.textBoxSubtitleID.TabIndex = 0;
            this.textBoxSubtitleID.Text = "40950d85-63eb-4ee2-b1e8-45c126601b94";
            // 
            // labelSubtitleID
            // 
            this.labelSubtitleID.AutoSize = true;
            this.labelSubtitleID.Location = new System.Drawing.Point(15, 33);
            this.labelSubtitleID.Name = "labelSubtitleID";
            this.labelSubtitleID.Size = new System.Drawing.Size(56, 13);
            this.labelSubtitleID.TabIndex = 0;
            this.labelSubtitleID.Text = "Subtitle ID";
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(597, 523);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 2;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(516, 523);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 1;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // contextMenuStripProfile
            // 
            this.contextMenuStripProfile.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.profilesToolStripMenuItem});
            this.contextMenuStripProfile.Name = "contextMenuStripProfile";
            this.contextMenuStripProfile.Size = new System.Drawing.Size(114, 26);
            // 
            // profilesToolStripMenuItem
            // 
            this.profilesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.importToolStripMenuItem,
            this.exportToolStripMenuItem});
            this.profilesToolStripMenuItem.Name = "profilesToolStripMenuItem";
            this.profilesToolStripMenuItem.Size = new System.Drawing.Size(113, 22);
            this.profilesToolStripMenuItem.Text = "Profiles";
            // 
            // importToolStripMenuItem
            // 
            this.importToolStripMenuItem.Name = "importToolStripMenuItem";
            this.importToolStripMenuItem.Size = new System.Drawing.Size(119, 22);
            this.importToolStripMenuItem.Text = "Import...";
            this.importToolStripMenuItem.Click += new System.EventHandler(this.importToolStripMenuItem_Click);
            // 
            // exportToolStripMenuItem
            // 
            this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            this.exportToolStripMenuItem.Size = new System.Drawing.Size(119, 22);
            this.exportToolStripMenuItem.Text = "Export...";
            this.exportToolStripMenuItem.Click += new System.EventHandler(this.exportToolStripMenuItem_Click);
            // 
            // DCinemaPropertiesInterop
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(684, 556);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DCinemaPropertiesInterop";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "D-Cinema properties (interop)";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DCinemaPropertiesInterop_KeyDown);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBoxFont.ResumeLayout(false);
            this.groupBoxFont.PerformLayout();
            this.contextMenuStripProfile.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelFontEffectColor;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBoxFont;
        private System.Windows.Forms.Button buttonFontEffectColor;
        private System.Windows.Forms.Panel panelFontColor;
        private System.Windows.Forms.Button buttonFontColor;
        private System.Windows.Forms.Label labelEffectColor;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownFontSize;
        private System.Windows.Forms.Label labelFontSize;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxFontEffect;
        private System.Windows.Forms.Label labelEffect;
        private System.Windows.Forms.Label labelFontColor;
        private Nikse.SubtitleEdit.Controls.NikseTextBox textBoxFontID;
        private System.Windows.Forms.Label labelFontId;
        private Nikse.SubtitleEdit.Controls.NikseTextBox textBoxFontUri;
        private System.Windows.Forms.Label labelFontUri;
        private System.Windows.Forms.Label labelReelNumber;
        private Nikse.SubtitleEdit.Controls.NikseTextBox textBoxMovieTitle;
        private System.Windows.Forms.Label labelMovieTitle;
        private Nikse.SubtitleEdit.Controls.NikseTextBox textBoxSubtitleID;
        private System.Windows.Forms.Label labelSubtitleID;
        private System.Windows.Forms.Button buttonGenerateID;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownReelNumber;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxLanguage;
        private System.Windows.Forms.Label labelLanguage;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownTopBottomMargin;
        private System.Windows.Forms.Label labelTopBottomMargin;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownZPosition;
        private System.Windows.Forms.Label labelZPosition;
        private System.Windows.Forms.Label labelZPositionHelp;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownFadeUp;
        private System.Windows.Forms.Label labelFadeUpTime;
        private Nikse.SubtitleEdit.Controls.NikseUpDown numericUpDownFadeDown;
        private System.Windows.Forms.Label labelFadeDownTime;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.CheckBox checkBoxGenerateIdAuto;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripProfile;
        private System.Windows.Forms.ToolStripMenuItem profilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
    }
}